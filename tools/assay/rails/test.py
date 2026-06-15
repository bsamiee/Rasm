"""Run test, discovery, coverage, and mutation rails."""

from collections.abc import Callable  # noqa: TC003  # _MUTATION_SCOPE binds the projection type at import time
from dataclasses import dataclass, replace
from pathlib import Path
from typing import Annotated, override, TYPE_CHECKING

from cyclopts import Parameter
from cyclopts.types import NonNegativeInt  # noqa: TC002  # cyclopts resolves Param-annotated dataclass fields at runtime
from expression import Error, Ok, Result  # beartype @checked resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec
import structlog

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import (  # noqa: TC001  # beartype resolves ArtifactScope/ArtifactStore/AssaySettings annotations at runtime
    ArtifactScope,
    ArtifactStore,
    AssaySettings,
)
from tools.assay.core.engine import fan_out, leased
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,  # noqa: TC001  # _roster_matches uses Completed as a runtime type in the tuple annotation
    Counts,
    Fault,  # beartype @checked resolves the rail's forward-ref (PEP 649)
    fold,
    Input,
    Language,
    language_choice,
    Match,
    Mode,
    MutationLane,
    receipt,
    Report,  # noqa: TC001  # beartype @checked resolves the rail's forward-ref (PEP 649)
    Runner,
    Stage,
    TestRun,
    Tool,
    ToolGroup,
)
from tools.assay.core.routing import expand, resolve_languages, route
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from expression.collections import Block

    from tools.assay.core.model import AnyDetail
    from tools.assay.core.routing import Routed


# --- [CONSTANTS] ------------------------------------------------------------------------

_GAP_NOTE: str = "mutation requested but no eligible lane (typescript has no mutation runner)"
_COVERAGE_JSON: str = ".artifacts/python/coverage/coverage.json"
_COVERAGE_OUTPUTS: tuple[tuple[str, tuple[str, ...]], ...] = (
    ("coverage.json", ("uv", "run", "coverage", "json", "-o", ".artifacts/python/coverage/coverage.json")),
    ("coverage.xml", ("uv", "run", "coverage", "xml", "-o", ".artifacts/python/coverage/coverage.xml")),
    ("coverage.lcov", ("uv", "run", "coverage", "lcov", "-o", ".artifacts/python/coverage/coverage.lcov")),
)

# --- [TABLES] ---------------------------------------------------------------------------

_TESTS = msgspec.json.Decoder(TestRun)
_ROSTER_ENCODER = msgspec.json.Encoder(order="deterministic")
# Runner-specific CHANGED mutation scopes: Stryker accepts file globs; mutmut requires module-dotted mutant names.
# Runners absent here surface UNSUPPORTED on the CHANGED lane.
_MUTATION_SCOPE: dict[str, Callable[[tuple[str, ...]], tuple[str, ...]]] = {
    "dotnet-stryker": lambda files: tuple(flag for f in files for flag in ("--mutate", f)),
    "mutmut": lambda files: tuple(f"{f.removesuffix('.py').replace('/', '.')}.*" for f in files),
}
# mutmut self-parallelizes below the rail governor; --max-children caps that second tier.
_MUTATION_GOVERNOR: frozenset[str] = frozenset(("mutmut",))
# Lease-riding kill-rate gate over the staged mutmut cache; group splice only.
_GATE_TOOL = Tool(
    name="mutmut-gate",
    runner=Runner.UV,
    command=("python", "-m", "tools.assay.rails.mutation_gate"),
    input=Input.NONE,
    language=Language.PYTHON,
    claim=Claim.TEST,
    mode=Mode.MUTATION,
    groups=(ToolGroup.MUTATION,),
    stage=Stage(project=True),
)

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class TestParams(BaseParams):
    """Parameters shared by test verbs."""

    # language selectors are optional; hide default so help does not advertise an unset flag
    csharp: Annotated[bool, Parameter(name="--csharp", negative="", show_default=False, help="Restrict the command to C# targets.")] = False
    python: Annotated[bool, Parameter(name="--python", negative="", show_default=False, help="Restrict the command to Python targets.")] = False
    typescript: Annotated[
        bool, Parameter(name="--typescript", negative="", show_default=False, help="Restrict the command to TypeScript targets.")
    ] = False
    target: Path | None = None
    all: bool = False
    mutation: MutationLane = MutationLane.OFF
    benchmark: bool = False
    coverage: bool = False
    filter: str = ""
    limit: NonNegativeInt = 0
    grep: str = ""

    @override
    def bound(self, verb: str) -> TestParams | Fault:
        """Validate language flag exclusivity while preserving variadic targets.

        Returns:
            Bound params, or a parse Fault when language flags conflict.
        """
        match language_choice(verb, csharp=self.csharp, python=self.python, typescript=self.typescript):
            case Fault() as fault:
                return fault
            case _:
                return super().bound(verb)


class _CoverageTotals(msgspec.Struct, frozen=True, gc=False):
    percent_covered: float


class _CoverageReport(msgspec.Struct, frozen=True, gc=False):
    totals: _CoverageTotals


# Decoder follows the structs it decodes; required fields make totals-less JSON degrade to None.
_COVERAGE_DECODER: msgspec.json.Decoder[_CoverageReport] = msgspec.json.Decoder(_CoverageReport)

# --- [SERVICES] -------------------------------------------------------------------------

_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.test")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _eligible(tool: Tool, params: TestParams) -> bool:
    # Mutation eligibility is independent of target/all; those narrow projects only.
    match (tool.mode, params.mutation):
        case (Mode.MUTATION, MutationLane.OFF):
            return False
        case (Mode.MUTATION, MutationLane.CHANGED | MutationLane.FULL):
            return True
        case _:
            # coverage/benchmark each re-run their own pytest, so a run-default row yields to them when either is set.
            special = params.benchmark or params.coverage
            return (
                (ToolGroup.REQUIRES_BENCHMARK not in tool.groups or params.benchmark)
                and (ToolGroup.REQUIRES_COVERAGE not in tool.groups or params.coverage)
                and (ToolGroup.RUN_DEFAULT not in tool.groups or not special)
            )


def _rows(language: Language, params: TestParams, mode: Mode) -> tuple[Tool, ...]:
    modes = {Mode.MUTATION: frozenset((Mode.MUTATION,)), Mode.CLIENT: frozenset((Mode.CLIENT,))}.get(
        mode, frozenset((mode, Mode.RESTORE, Mode.BUILD))
    )
    return tuple(t for t in select(Claim.TEST, language) if t.mode in modes and _eligible(t, params))


def _mutation_gap(params: TestParams, rows: tuple[Tool, ...]) -> bool:
    return params.mutation is not MutationLane.OFF and not any(t.mode is Mode.MUTATION for t in rows)


def _filter(expr: str) -> tuple[str, ...]:
    # MTP filter discriminant: leading `/` = query, `k=v` = trait, Tests/Laws/Spec suffix or `+` = class, else method.
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


def _governor(tool: Tool, settings: AssaySettings) -> tuple[str, ...]:
    # --max-children binds mutmut's internal pytest fan-out to mutation_max_cpu.
    return ("--max-children", str(settings.mutation_max_cpu)) if tool.name in _MUTATION_GOVERNOR else ()


def _scoped_mutation(tool: Tool, params: TestParams, settings: AssaySettings, files: tuple[str, ...]) -> Tool | None:
    # One mutation argv shaper owns governors and changed-file scoping for every runner.
    govern = _governor(tool, settings)
    match (tool.mode, params.mutation, _MUTATION_SCOPE.get(tool.name)):
        case (Mode.MUTATION, MutationLane.CHANGED, None):
            return None
        case (Mode.MUTATION, MutationLane.CHANGED, scoped) if scoped is not None:
            return msgspec.structs.replace(tool, command=(*tool.command, *govern, *scoped(files)))
        case (Mode.MUTATION, _, _):
            return msgspec.structs.replace(tool, command=(*tool.command, *govern)) if govern else tool
        case _:
            return tool


def _checks(routed: Routed, params: TestParams, settings: AssaySettings, mode: Mode) -> tuple[Check, ...]:
    # MTP consumes the filter; non-dotnet rows ignore it. CHANGED mutation rows scope to routed files.
    filt = _filter(params.filter)

    def _splice(tool: Tool) -> Tool | None:
        scoped = _scoped_mutation(tool, params, settings, routed.files)
        match (scoped, tool.mode, bool(filt), tool.runner):
            case (None, _, _, _) | (_, Mode.MUTATION, _, _):
                return scoped
            case (_, Mode.RUN | Mode.LIST, True, Runner.DOTNET):
                return msgspec.structs.replace(tool, command=(*tool.command, *filt))
            case _:
                return tool

    selected = tuple(
        Check(tool=spliced, paths=routed.files) for t in _rows(routed.language, params, mode) for spliced in (_splice(t),) if spliced is not None
    )
    return expand(selected, routed, settings=settings)


def _unsupported_scope(routed: Routed, params: TestParams, mode: Mode) -> tuple[Result[Completed, Fault], ...]:
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
    # C# dotnet rows consume projects; --target pins one project, --all adds the default test target.
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
    checks = _checks(routed, params, settings, mode)
    unsupported = _unsupported_scope(routed, params, mode)
    match checks:
        case ():
            return unsupported
        case _:
            return (*fan_out(checks, settings=settings, scope=scope, routed=routed), *unsupported)


def _roster_matches(outcomes: tuple[Completed, ...]) -> tuple[Match, ...]:
    # Accepts dotnet list-test and pytest collect output; skips MTP headers and count summary lines.
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


def _test_detail(done: Completed) -> TestRun:
    try:
        return _TESTS.decode(done.stdout or b"{}")
    except msgspec.DecodeError:
        return TestRun()


def _detail(done: tuple[Completed, ...], params: TestParams, root: Path) -> AnyDetail | None:
    # Mutation evidence is stdout-derived; use the first decoded receipt carrying a real mutation lane.
    mutation = next(
        (d for c in done if (d := _test_detail(c)) is not None and isinstance(d, TestRun) and d.mutation is not MutationLane.OFF), TestRun()
    )
    coverage = coverage_percent(root)
    match (params.mutation, params.coverage, coverage):
        case (MutationLane.OFF, False, _):
            return None
        case (_, True, float() as total):
            return msgspec.structs.replace(mutation, coverage=total)
        case (MutationLane.CHANGED | MutationLane.FULL, _, _):
            return mutation if mutation.mutation is not MutationLane.OFF else None
        case _:
            return None


def coverage_percent(root: Path) -> float | None:
    """Decode the coverage percentage from the coverage JSON artifact under ``root``.

    Returns:
        ``totals.percent_covered`` from ``.artifacts/python/coverage/coverage.json``,
        or None when the file is absent or undecodable.
    """
    try:
        return _COVERAGE_DECODER.decode((root / _COVERAGE_JSON).read_bytes()).totals.percent_covered
    except OSError, msgspec.DecodeError:
        return None


def _results_artifact(scope: ArtifactScope) -> Artifact:
    # dotnet --results-directory writes into the per-run scope path; expose it as the test results artifact.
    return Artifact(id="results", kind=ArtifactKind.TEST, path=str(scope.path))


def _adopt_coverage(store: ArtifactStore, run_id: str, path: Path) -> Artifact:
    raw = path.read_bytes()
    stored = store.adopt_file(path, ArtifactKind.TEST.value, run_id, "coverage", path.name)
    return Artifact(id=path.name, kind=ArtifactKind.TEST, path=stored, bytes=len(raw), lines=len(raw.decode(errors="replace").splitlines()))


def _coverage_artifacts(settings: AssaySettings, scope: ArtifactScope, done: tuple[Completed, ...]) -> tuple[Artifact, ...]:
    root = Path(str(settings.root)) / ".artifacts/python/coverage"
    produced = frozenset(name for name, head in _COVERAGE_OUTPUTS if any(c.argv[: len(head)] == head and c.returncode == 0 for c in done))
    paths = tuple(p for name in produced for p in (root / name,) if p.is_file())
    return tuple(_adopt_coverage(scope.store, settings.run_id, path) for path in paths)


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


def _gate(
    done: tuple[Result[Completed, Fault], ...], routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope
) -> tuple[Result[Completed, Fault], ...]:
    # Kill-rate gate rides the held mutation lease: one sequential check against the staged mutmut cache.
    staged = next((t for t in _rows(routed.language, params, Mode.MUTATION) if t.stage.root), None)
    succeeded = any(c.returncode == 0 and "mutmut" in c.argv for r in done if r.is_ok() for c in (r.ok,))
    match (staged, succeeded):
        case (Tool() as row, True):
            work = Path(str(settings.root)) / row.stage.root
            return fan_out((Check(tool=_GATE_TOOL, cwd=work),), settings=settings, scope=scope, routed=routed)
        case _:
            return ()


def _dispatch_all(
    routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode
) -> tuple[Result[Completed, Fault], ...]:
    run = _dispatch(routed, params, settings=settings, scope=scope, mode=mode)
    mutation = (
        _dispatch(routed, params, settings=settings, scope=scope, mode=Mode.MUTATION)
        if params.mutation is not MutationLane.OFF and mode is Mode.RUN
        else ()
    )
    gate = _gate(mutation, routed, params, settings=settings, scope=scope) if mutation else ()
    coverage = _dispatch(routed, params, settings=settings, scope=scope, mode=Mode.CLIENT) if params.coverage and mode is Mode.RUN else ()
    return (*run, *mutation, *gate, *coverage)


def _thin_rail(settings: AssaySettings, scope: ArtifactScope, params: TestParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """Run routed test eligibility, fan-out, mutation gating, and folding.

    Returns:
        Folded test report, or routing/spawn/lease fault.
    """
    choice = language_choice(verb, csharp=params.csharp, python=params.python, typescript=params.typescript)
    language_result = resolve_languages(choice, params.paths, claim=claim)
    match language_result:
        case Result(tag="error", error=fault):
            return Error(fault)
        case Result(tag="ok", ok=languages):
            pass
        case unreachable:
            return Error(Fault(("test", verb), status=RailStatus.FAULTED, message=f"language selection unresolved: {unreachable!r}"))
    eligible = tuple(t for language in languages for t in _rows(language, params, Mode.MUTATION if params.mutation is not MutationLane.OFF else mode))
    gap = _mutation_gap(params, eligible)
    match gap:
        case True:
            _LOG.warning(_GAP_NOTE, claim=claim.value, verb=verb, language=choice.value if isinstance(choice, Language) else "")
        case False:
            pass

    def _work(_held: object = None) -> Result[Report, Fault]:
        def _settle(done: Block[Completed], selected: tuple[Routed, ...]) -> Report:
            outcomes = tuple(done)
            base = fold(claim, verb, outcomes, detail=_detail(outcomes, params, Path(str(settings.root))), promote_empty=True)
            return msgspec.structs.replace(
                base,
                artifacts=(*base.artifacts, _results_artifact(scope), *_coverage_artifacts(settings, scope, outcomes)),
                notes=(*base.notes, *(note for r in selected for note in r.closure_note()), *((_GAP_NOTE,) if gap else ())),
            )

        return (
            _routed(languages, params.paths, settings)
            .map(lambda routed: tuple(_select(r, params, settings) for r in routed))
            .bind(
                lambda selected: sequence(
                    block.of_seq(row for r in selected for row in _dispatch_all(r, params, settings=settings, scope=scope, mode=mode))
                ).map(lambda done: _settle(done, selected))
            )
        )

    # Sorted per-language mutation leases keep cross-agent contention deterministic and deadlock-free.
    def _nested(resources: tuple[str, ...]) -> Result[Report, Fault]:
        match resources:
            case (head, *rest):
                return leased(
                    f"mutation-{head}", lambda _held: _nested(tuple(rest)), settings=settings, run_id=settings.run_id, project=head, mode="exclusive"
                )
            case _:
                return _work()

    return _nested(tuple(sorted({t.language.value for t in eligible if t.mode is Mode.MUTATION})))


# --- [COMPOSITION] ----------------------------------------------------------------------


def run(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:
    """Run eligible test suites.

    Returns:
        Test run report, or routing/spawn fault.
    """
    return _thin_rail(settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN)


def list(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:  # noqa: A001
    """List discovered tests.

    The name must match the registry-bound Handler name for verb dispatch.

    Returns:
        Test roster report, or routing/spawn fault.
    """
    language_result = resolve_languages(
        language_choice("list", csharp=params.csharp, python=params.python, typescript=params.typescript), params.paths, claim=Claim.TEST
    )
    match language_result:
        case Result(tag="error", error=fault):
            return Error(fault)
        case Result(tag="ok", ok=languages):
            pass
        case unreachable:
            return Error(Fault(("test", "list"), status=RailStatus.FAULTED, message=f"language selection unresolved: {unreachable!r}"))
    needle = params.grep.strip().lower()

    def _settle(done: block.Block[Completed], selected: tuple[Routed, ...]) -> Report:
        outcomes = tuple(done)
        base = fold(Claim.TEST, "list", outcomes, promote_empty=True)
        discovered = tuple(m for m in _roster_matches(outcomes) if not needle or needle in m.text.lower())
        artifacts = (*base.artifacts, _results_artifact(scope), *_roster_artifacts(settings, scope, discovered))
        roster = discovered[: params.limit] if params.limit > 0 else discovered
        # detail.selected preserves the pre-limit discovery total; registry caps own later clipping notes.
        detail = TestRun(selected=len(discovered)) if discovered else None
        note = (f"discovery: total={len(discovered)} returned={len(roster)}",) if discovered else ()
        diagnostics = tuple(
            f"discovery {c.status.value}: {' '.join(c.argv)[:120]}{f': {tail}' if tail else ''}"
            for c in outcomes
            if c.status.severity > RailStatus.OK.severity
            for tail in ((c.stderr or c.stdout)[-256:].decode(errors="replace").strip(),)
        )
        notes = (*base.notes, *(n for r in selected for n in r.closure_note()), *note, *diagnostics)
        return (
            msgspec.structs.replace(
                base,
                status=RailStatus.OK,
                counts=Counts(len(roster), 0, len(roster)),
                results=roster,
                artifacts=artifacts,
                notes=notes,
                detail=detail,
            )
            if roster
            else msgspec.structs.replace(base, artifacts=artifacts, notes=notes, detail=detail)
        )

    return (
        _routed(languages, params.paths, settings)
        .map(lambda routed: tuple(_select(r, params, settings) for r in routed))
        .bind(
            lambda selected: sequence(
                block.of_seq(row for r in selected for row in _dispatch(r, params, settings=settings, scope=scope, mode=Mode.LIST))
            ).map(lambda done: _settle(done, selected))
        )
    )


def coverage(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:
    """Run eligible suites under coverage.

    Returns:
        Coverage test report, or routing/spawn fault.
    """
    return _thin_rail(settings, scope, replace(params, coverage=True, benchmark=False), claim=Claim.TEST, verb="coverage", mode=Mode.RUN)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TestParams", "coverage", "coverage_percent", "list", "run"]
