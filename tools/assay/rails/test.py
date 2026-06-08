"""Run test, list, coverage, and mutation rails."""

from collections.abc import Callable  # noqa: TC003  # unconditional: _MUTATION_SCOPE binds the projection type at import time
from dataclasses import dataclass, replace
from pathlib import Path  # unconditional: cyclopts resolves TestParams.target's `Path | None` at CLI-build (PEP 649)
from typing import TYPE_CHECKING

from cyclopts.types import NonNegativeInt  # noqa: TC002  # Cyclopts evaluates Param dataclass annotations at runtime.
from expression import Ok, Result  # noqa: TC002  # unconditional: beartype @checked resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec
import structlog

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import ArtifactScope, ArtifactStore, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out, leased
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,  # noqa: TC001  # unconditional: _roster_matches uses Completed as a runtime type in the tuple annotation
    Counts,
    Fault,  # noqa: TC001  # unconditional: beartype @checked resolves the rail's forward-ref (PEP 649)
    fold,
    Match,
    Mode,
    MutationLane,
    receipt,
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

_GAP_NOTE: str = "mutation requested but no eligible lane (typescript has no mutation runner)"
_COVERAGE_OUTPUTS: tuple[tuple[str, tuple[str, ...]], ...] = (
    ("coverage.json", ("uv", "run", "coverage", "json", "-o", ".artifacts/python/coverage/coverage.json")),
    ("coverage.xml", ("uv", "run", "coverage", "xml", "-o", ".artifacts/python/coverage/coverage.xml")),
    ("coverage.lcov", ("uv", "run", "coverage", "lcov", "-o", ".artifacts/python/coverage/coverage.lcov")),
)
_TESTS = msgspec.json.Decoder(TestRun)
_ROSTER_ENCODER = msgspec.json.Encoder(order="deterministic")
# Per-runner changed-file mutation scope: Stryker.NET takes repeatable `--mutate <glob>`; runners absent here cannot scope (UNSUPPORTED).
_MUTATION_SCOPE: dict[str, Callable[[tuple[str, ...]], tuple[str, ...]]] = {
    "dotnet-stryker": lambda files: tuple(flag for f in files for flag in ("--mutate", f))
}


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class TestParams(BaseParams):
    """Parameters shared by test verbs."""

    target: Path | None = None
    all: bool = False
    no_build: bool = False
    mutation: MutationLane = MutationLane.OFF
    benchmark: bool = False
    coverage: bool = False
    filter: str = ""
    limit: NonNegativeInt = 0
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
    # Reject ineligible mutation rows before they can acquire mutation.lock; the C# selector (target/all)
    # narrows which project mutates via routed.projects, never whether mutation runs.
    match (tool.mode, params.mutation):
        case (Mode.MUTATION, MutationLane.OFF):
            return False
        case (Mode.MUTATION, MutationLane.CHANGED | MutationLane.FULL):
            return True
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
    return params.mutation is not MutationLane.OFF and not any(t.mode is Mode.MUTATION for t in rows)


def _filter(expr: str) -> tuple[str, ...]:
    # MTP filter discriminant: query (/...), trait (k=v), class (suffix/+), else method.
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


def _scoped_mutation(tool: Tool, params: TestParams, files: tuple[str, ...]) -> Tool | None:
    # CHANGED lane narrows the mutation runner to the changed-file set; runners absent from the table cannot scope (None -> UNSUPPORTED).
    match (tool.mode, params.mutation, _MUTATION_SCOPE.get(tool.name)):
        case (Mode.MUTATION, MutationLane.CHANGED, None):
            return None
        case (Mode.MUTATION, MutationLane.CHANGED, scoped) if scoped is not None:
            return msgspec.structs.replace(tool, command=(*tool.command, *scoped(files)))
        case _:
            return tool


def _checks(routed: Routed, params: TestParams, mode: Mode) -> tuple[Check, ...]:
    # dotnet test (MTP) consumes the filter expression; non-dotnet rows ignore it. CHANGED mutation rows scope to routed files.
    filt = _filter(params.filter)

    def _splice(tool: Tool) -> Tool | None:
        scoped = _scoped_mutation(tool, params, routed.files)
        match (scoped, tool.mode, bool(filt), tool.runner):
            case (None, _, _, _) | (_, Mode.MUTATION, _, _):
                return scoped
            case (_, Mode.RUN | Mode.LIST, True, Runner.DOTNET):
                return msgspec.structs.replace(tool, command=(*tool.command, *filt))
            case _:
                return tool

    return tuple(
        Check(tool=spliced, paths=routed.files) for t in _rows(routed.language, params, mode) for spliced in (_splice(t),) if spliced is not None
    )


def _unsupported_scope(routed: Routed, params: TestParams, mode: Mode) -> tuple[Result[Completed, Fault], ...]:
    # CHANGED mutation rows whose runner has no scope table entry surface UNSUPPORTED rather than mutate the whole tree.
    match (mode, params.mutation):
        case (Mode.MUTATION, MutationLane.CHANGED):
            return tuple(
                Ok(receipt(t.command, 3, status=RailStatus.UNSUPPORTED, notes=(f"{t.name}: no changed-file mutation scope",)))
                for t in _rows(routed.language, params, mode)
                if t.name not in _MUTATION_SCOPE
            )
        case _:
            return ()


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...], settings: AssaySettings) -> Result[Block[Routed], Fault]:
    return sequence(block.of_seq(route(language, paths, settings=settings) for language in languages))


def _select(routed: Routed, params: TestParams, settings: AssaySettings) -> Routed:
    # The C# selector projects onto routed.projects so dotnet test/list/stryker rows (Input.PROJECT) splice the
    # chosen csproj(s); glob-strategy languages have no project axis and ignore target/all. `--target` narrows to one
    # project; `--all` unions the default test target with the changed-file closure so the canonical suite always runs.
    match (routed.language.strategy, params.target, params.all):
        case ("glob", _, _):
            return routed
        case (_, Path() as target, _):
            return msgspec.structs.replace(routed, projects=(str(target),))
        case (_, None, True):
            return msgspec.structs.replace(routed, projects=tuple(sorted({str(settings.test_target), *routed.projects})))
        case _:
            return routed


def _dispatch(
    routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode
) -> tuple[Result[Completed, Fault], ...]:
    checks = _checks(routed, params, mode)
    unsupported = _unsupported_scope(routed, params, mode)
    match checks:
        case ():
            return unsupported
        case _:
            return (*fan_out(checks, settings=settings, scope=scope, routed=routed), *unsupported)


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
    coverage = coverage_percent(done)
    match (params.mutation, params.coverage, coverage):
        case (MutationLane.OFF, False, _):
            return None
        case (_, True, float() as total):
            return msgspec.structs.replace(mutation, coverage=total)
        case (MutationLane.CHANGED | MutationLane.FULL, _, _):
            return mutation if mutation.mutation is not MutationLane.OFF else None
        case _:
            return None


def _test_detail(done: Completed) -> TestRun:
    try:
        return _TESTS.decode(done.stdout or b"{}")
    except msgspec.DecodeError:
        return TestRun()


def coverage_percent(done: tuple[Completed, ...]) -> float | None:
    """Extract the coverage percentage from a completed ``coverage report`` run.

    Returns:
        Coverage percentage as a float, or ``None`` when no coverage report is present.
    """
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


def _coverage_artifacts(settings: AssaySettings, scope: ArtifactScope, done: tuple[Completed, ...]) -> tuple[Artifact, ...]:
    root = Path(str(settings.root)) / ".artifacts/python/coverage"
    produced = frozenset(name for name, head in _COVERAGE_OUTPUTS if any(c.argv[: len(head)] == head and c.returncode == 0 for c in done))
    paths = tuple(p for name in produced for p in (root / name,) if p.is_file())
    return tuple(_adopt_coverage(scope.store, settings.run_id, path) for path in paths)


def _adopt_coverage(store: ArtifactStore, run_id: str, path: Path) -> Artifact:
    raw = path.read_bytes()
    stored = store.adopt_file(path, ArtifactKind.TEST.value, run_id, "coverage", path.name)
    return Artifact(id=path.name, kind=ArtifactKind.TEST, path=stored, bytes=len(raw), lines=len(raw.decode(errors="replace").splitlines()))


def _roster_artifacts(settings: AssaySettings, scope: ArtifactScope, discovered: tuple[Match, ...]) -> tuple[Artifact, ...]:
    text = "\n".join(m.text for m in discovered)
    raw = _ROSTER_ENCODER.encode(discovered)
    text_raw = text.encode()
    text_path, json_path = scope.store.write_many((
        (text_raw, (ArtifactKind.TEST.value, settings.run_id, "roster.txt")),
        (raw, (ArtifactKind.TEST.value, settings.run_id, "roster.json")),
    ))
    return (
        Artifact(id="test-roster", kind=ArtifactKind.TEST, path=text_path, bytes=len(text_raw), lines=len(discovered)),
        Artifact(id="test-roster-json", kind=ArtifactKind.TEST, path=json_path, bytes=len(raw), lines=len(discovered)),
    )


def _thin_rail(settings: AssaySettings, scope: ArtifactScope, params: TestParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """Run the shared test route, eligibility, fan-out, and fold body.

    Returns:
        Folded test report, or routing/spawn/lease fault.
    """
    languages = _languages(params.language)
    eligible = tuple(t for language in languages for t in _rows(language, params, Mode.MUTATION if params.mutation is not MutationLane.OFF else mode))
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
                artifacts=(*base.artifacts, _results_artifact(scope), *_coverage_artifacts(settings, scope, outcomes)),
                notes=(*base.notes, *((_GAP_NOTE,) if gap else ())),
            )

        return _routed(languages, params.paths, settings).bind(
            lambda routed: sequence(
                routed.collect(lambda r: block.of_seq(_dispatch_all(_select(r, params, settings), params, settings=settings, scope=scope, mode=mode)))
            ).map(_settle)
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
    return _thin_rail(settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN)


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
        artifacts = (*base.artifacts, _results_artifact(scope), *_roster_artifacts(settings, scope, discovered))
        roster = discovered[: params.limit] if params.limit > 0 else discovered
        note = (f"discovery: total={len(discovered)} returned={len(roster)}",) if discovered else ()
        diagnostics = tuple(
            f"discovery {c.status.value}: {' '.join(c.argv)[:120]}{f': {tail}' if tail else ''}"
            for c in outcomes
            if c.status.severity > RailStatus.OK.severity
            for tail in ((c.stderr or c.stdout)[-256:].decode(errors="replace").strip(),)
        )
        notes = (*base.notes, *note, *diagnostics)
        return (
            msgspec.structs.replace(
                base, status=RailStatus.OK, counts=Counts(len(roster), 0, len(roster)), results=roster, artifacts=artifacts, notes=notes
            )
            if roster
            else msgspec.structs.replace(base, artifacts=artifacts, notes=notes)
        )

    return _routed(languages, params.paths, settings).bind(
        lambda routed: sequence(
            routed.collect(lambda r: block.of_seq(_dispatch(_select(r, params, settings), params, settings=settings, scope=scope, mode=Mode.LIST)))
        ).map(_settle)
    )


def coverage(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:
    """Run eligible suites under coverage.

    Returns:
        Coverage test report, or routing/spawn fault.
    """
    return _thin_rail(settings, scope, replace(params, coverage=True, benchmark=False), claim=Claim.TEST, verb="coverage", mode=Mode.RUN)


def _dispatch_all(
    routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode
) -> tuple[Result[Completed, Fault], ...]:
    run = _dispatch(routed, params, settings=settings, scope=scope, mode=mode)
    mutation = (
        _dispatch(routed, params, settings=settings, scope=scope, mode=Mode.MUTATION)
        if params.mutation is not MutationLane.OFF and mode is Mode.RUN
        else ()
    )
    coverage = _dispatch(routed, params, settings=settings, scope=scope, mode=Mode.CLIENT) if params.coverage and mode is Mode.RUN else ()
    return (*run, *mutation, *coverage)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TestParams", "coverage", "coverage_percent", "list", "run"]
