"""Run test, list, coverage, and mutation rails."""

from dataclasses import dataclass, replace
from pathlib import Path  # unconditional: cyclopts resolves TestParams.target's `Path | None` at CLI-build (PEP 649)
from typing import TYPE_CHECKING

from expression import Result  # noqa: TC002  # unconditional: beartype @checked resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec
import structlog

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out, leased
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,  # noqa: TC001  # unconditional: _roster_matches uses Completed as a runtime type in the tuple annotation
    Fault,  # noqa: TC001  # unconditional: beartype @checked resolves the rail's forward-ref (PEP 649)
    fold,
    Match,
    Mode,
    MutationLane,
    Report,  # noqa: TC001  # unconditional: beartype @checked resolves the rail's forward-ref (PEP 649)
    Runner,
    TestRun,
)
from tools.assay.core.routing import route
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from expression.collections import Block

    from tools.assay.core.model import AnyDetail, Language, Tool
    from tools.assay.core.routing import Routed


# --- [CONSTANTS] ------------------------------------------------------------------------

_GAP_NOTE: str = "mutation requested but no eligible lane (typescript has no mutation runner; or target/all overrides the default)"
_COVERAGE_OUTPUTS: tuple[tuple[str, tuple[str, ...]], ...] = (
    ("coverage.json", ("uv", "run", "coverage", "json")),
    ("coverage.xml", ("uv", "run", "coverage", "xml")),
    ("coverage.lcov", ("uv", "run", "coverage", "lcov")),
)
_TESTS = msgspec.json.Decoder(TestRun)


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class TestParams(BaseParams):
    """Parameters shared by test verbs."""

    target: Path | None = None
    all: bool = False
    no_build: bool = False
    mutation: bool = False
    benchmark: bool = False
    coverage: bool = False
    filter: str = ""
    limit: int = 0
    grep: str = ""


# --- [SERVICES] -------------------------------------------------------------------------

_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.test")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _languages(selected: Language | None) -> tuple[Language, ...]:
    match selected:
        case None:
            return tuple(sorted({t.language for t in select(Claim.TEST)}, key=lambda member: member.value))
        case language:
            return (language,)


def _eligible(tool: Tool, params: TestParams) -> bool:
    # Reject ineligible mutation rows before they can acquire mutation.lock.
    match (tool.mode, params.mutation):
        case (Mode.MUTATION, False):
            return False
        case (Mode.MUTATION, True):
            return params.target is None and not params.all
        case (Mode.RESTORE, _) | (Mode.BUILD, _):
            return not params.no_build
        case _:
            return (
                (tool.name != "pytest" or not (params.benchmark or params.coverage))
                and (tool.name != "coverage" or params.coverage)
                and (not tool.name.startswith("coverage-") or params.coverage)
                and (tool.name != "pytest-benchmark" or params.benchmark)
            )


def _rows(language: Language, params: TestParams, mode: Mode) -> tuple[Tool, ...]:
    modes = {Mode.MUTATION: frozenset((Mode.MUTATION,)), Mode.CLIENT: frozenset((Mode.CLIENT,))}.get(
        mode, frozenset((mode, Mode.RESTORE, Mode.BUILD))
    )
    return tuple(t for t in select(Claim.TEST, language) if t.mode in modes and _eligible(t, params))


def _mutation_gap(params: TestParams, rows: tuple[Tool, ...]) -> bool:
    # Valid but inapplicable mutation requests fold to EMPTY, not FAULTED.
    return params.mutation and not any(t.mode is Mode.MUTATION for t in rows)


def _filter(expr: str) -> tuple[str, ...]:
    # Mirror the quality MTP filter discriminant: query (/...), trait (k=v), class (suffix/+), else method.
    match expr.strip():
        case "":
            return ()
        case query if query.startswith("/"):
            return ("--filter-query", query)
        case trait if "=" in trait:
            return ("--filter-trait", trait)
        case class_name if class_name.endswith(("Tests", "Laws", "Spec")) or "+" in class_name:
            return ("--filter-class", f"*{class_name}*")
        case method:
            return ("--filter-method", f"*{method}*")


def _checks(routed: Routed, params: TestParams, mode: Mode) -> tuple[Check, ...]:
    # dotnet test (MTP) consumes the filter expression; non-dotnet rows ignore it.
    filt = _filter(params.filter)

    def _splice(tool: Tool) -> Tool:
        match (bool(filt), tool.runner, tool.mode):
            case (True, Runner.DOTNET, Mode.RUN | Mode.LIST):
                return msgspec.structs.replace(tool, command=(*tool.command, *filt))
            case _:
                return tool

    return tuple(Check(tool=_splice(t), paths=routed.files) for t in _rows(routed.language, params, mode))


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...], settings: AssaySettings) -> Result[Block[Routed], Fault]:
    return sequence(block.of_seq(route(language, paths, settings=settings) for language in languages))


def _dispatch(
    routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode
) -> tuple[Result[Completed, Fault], ...]:
    checks = _checks(routed, params, mode)
    match checks:
        case ():
            return ()
        case _:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)


def _roster_matches(outcomes: tuple[Completed, ...]) -> tuple[Match, ...]:
    # Accept dotnet list-test rows and pytest collect rows; skip headers and count summaries.
    def _skip(name: str) -> bool:
        return name.startswith("The ") or (name[:1].isdigit() and name.endswith("found"))

    rows = []
    for c in outcomes:
        if c.returncode == 0 and c.stdout:
            for raw in c.stdout.decode(errors="replace").splitlines():
                name = raw.strip()
                if name and not _skip(name):
                    rows.append(Match(id=name, kind=ArtifactKind.PROCESS, text=name))
    return tuple(rows)


def _detail(done: tuple[Completed, ...], params: TestParams) -> AnyDetail | None:
    # Mutation evidence is stdout-derived; choose the first decoded receipt with a real mutation lane.
    mutation = next(
        (d for c in done if (d := _test_detail(c)) is not None and isinstance(d, TestRun) and d.mutation is not MutationLane.OFF), TestRun()
    )
    coverage = _coverage_percent(done)
    match (params.mutation, params.coverage, coverage):
        case (False, False, _):
            return None
        case (_, True, float() as total):
            return msgspec.structs.replace(mutation, coverage=total)
        case (True, _, _):
            return mutation if mutation.mutation is not MutationLane.OFF else None
        case _:
            return None


def _test_detail(done: Completed) -> TestRun:
    try:
        return _TESTS.decode(done.stdout or b"{}")
    except msgspec.DecodeError:
        return TestRun()


def _coverage_percent(done: tuple[Completed, ...]) -> float | None:
    return next(
        (
            float(text)
            for c in done
            if c.argv[:4] == ("uv", "run", "coverage", "report")
            for text in (c.stdout.decode(errors="replace").strip(),)
            if text.replace(".", "", 1).isdigit()
        ),
        None,
    )


def _results_artifact(scope: ArtifactScope) -> Artifact:
    # The per-run scope directory is where dotnet --results-directory writes; surface it as the test results artifact.
    return Artifact(id="results", kind=ArtifactKind.TEST, path=str(scope.path))


def _coverage_artifacts(settings: AssaySettings, done: tuple[Completed, ...]) -> tuple[Artifact, ...]:
    root = Path(str(settings.root)) / ".artifacts/python/coverage"
    produced = frozenset(name for name, head in _COVERAGE_OUTPUTS if any(c.argv[: len(head)] == head and c.returncode == 0 for c in done))
    paths = tuple(p for name in produced for p in (root / name,) if p.is_file())
    return tuple(
        Artifact(
            id=path.name, kind=ArtifactKind.TEST, path=str(path), bytes=path.stat().st_size, lines=len(path.read_text(errors="replace").splitlines())
        )
        for path in paths
    )


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: TestParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """Run the shared test route, eligibility, fan-out, and fold body.

    Returns:
        Folded test report, or routing/spawn/lease fault.
    """
    languages = _languages(params.language)
    eligible = tuple(t for language in languages for t in _rows(language, params, Mode.MUTATION if params.mutation else mode))
    gap = _mutation_gap(params, eligible)
    match gap:
        case True:
            _LOG.warning(_GAP_NOTE, claim=claim.value, verb=verb, language=params.language)
        case False:
            pass

    def _work(_held: object = None) -> Result[Report, Fault]:
        def _settle(done: Block[Completed]) -> Report:
            outcomes = tuple(done)
            base = fold(claim, verb, outcomes, detail=_detail(outcomes, params))
            return msgspec.structs.replace(
                base,
                artifacts=(*base.artifacts, _results_artifact(scope), *_coverage_artifacts(settings, outcomes)),
                notes=(*base.notes, *((_GAP_NOTE,) if gap else ())),
            )

        return _routed(languages, params.paths, settings).bind(
            lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch_all(r, params, settings=settings, scope=scope, mode=mode)))).map(
                _settle
            )
        )

    match any(t.mode is Mode.MUTATION for t in eligible):
        case True:
            return leased("mutation", _work, settings=settings, run_id=settings.run_id, project="mutation", mode="exclusive")
        case False:
            return _work()


# --- [COMPOSITION] ----------------------------------------------------------------------


def run(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:
    """Run eligible test suites.

    Returns:
        Test run report, or routing/spawn fault.
    """
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN)


def list(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:  # noqa: A001  # the verb IS "list"; this is the registry-bound Handler name
    """List discovered tests.

    Returns:
        Test roster report, or routing/spawn fault.
    """
    languages = _languages(params.language)
    needle = params.grep.strip().lower()

    def _settle(done: block.Block[Completed]) -> Report:
        outcomes = tuple(done)
        base = fold(Claim.TEST, "list", outcomes)
        discovered = tuple(m for m in _roster_matches(outcomes) if not needle or needle in m.text.lower())
        roster = discovered[: params.limit] if params.limit > 0 else discovered
        note = (f"discovery: total={len(discovered)} returned={len(roster)}",) if discovered else ()
        return (
            msgspec.structs.replace(
                base,
                status=RailStatus.OK,
                results=(*base.results, *roster),
                artifacts=(*base.artifacts, _results_artifact(scope)),
                notes=(*base.notes, *note),
            )
            if roster
            else msgspec.structs.replace(base, artifacts=(*base.artifacts, _results_artifact(scope)))
        )

    return _routed(languages, params.paths, settings).bind(
        lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, params, settings=settings, scope=scope, mode=Mode.LIST)))).map(
            _settle
        )
    )


def coverage(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:
    """Run eligible suites under coverage.

    Returns:
        Coverage test report, or routing/spawn fault.
    """
    return thin_rail(settings, scope, replace(params, coverage=True, benchmark=False), claim=Claim.TEST, verb="coverage", mode=Mode.RUN)


def _dispatch_all(
    routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode
) -> tuple[Result[Completed, Fault], ...]:
    run = _dispatch(routed, params, settings=settings, scope=scope, mode=mode)
    mutation = _dispatch(routed, params, settings=settings, scope=scope, mode=Mode.MUTATION) if params.mutation and mode is Mode.RUN else ()
    coverage = _dispatch(routed, params, settings=settings, scope=scope, mode=Mode.CLIENT) if params.coverage and mode is Mode.RUN else ()
    return (*run, *mutation, *coverage)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TestParams", "coverage", "list", "run", "thin_rail"]
