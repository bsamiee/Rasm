"""Run test, discovery, coverage, and mutation rails."""

from collections import Counter
from collections.abc import Callable, Iterable  # noqa: TC003  # _MUTATION_SCOPE binds the projection type at import time
from dataclasses import dataclass, replace
from enum import StrEnum
from pathlib import Path, PurePosixPath
from typing import Annotated, override, TYPE_CHECKING
import xml.etree.ElementTree as ET  # noqa: S405  # trusted local Workspace.slnx XML from the repo root

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
    DOTNET_BUILD_CLOSURE,
    PY_ARTIFACT_ROOTS,
    PY_COVERAGE_FILES,
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
from tools.assay.core.routing import expand, parse_csproj, resolve_languages, route, Scope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from expression.collections import Block

    from tools.assay.core.model import AnyDetail
    from tools.assay.core.routing import Routed


# --- [TYPES] ----------------------------------------------------------------------------


class _TestProjectLane(StrEnum):
    MANAGED = "managed"
    HOST_BOUND = "host_bound"
    SUPPORT = "support"
    BENCHMARK = "benchmark"
    NON_TEST = "non_test"


class _DiscoveryLane(StrEnum):
    LISTED = "listed"
    EMPTY_OR_FAILED = "empty_or_failed_discovery"


# --- [CONSTANTS] ------------------------------------------------------------------------

_GAP_NOTE: str = "mutation requested but no eligible lane (typescript has no mutation runner)"
_COVERAGE_JSON: str = PY_COVERAGE_FILES["json"]
_COVERAGE_OUTPUTS: tuple[tuple[str, tuple[str, ...]], ...] = (
    ("coverage.json", ("uv", "run", "coverage", "json", "-o", PY_COVERAGE_FILES["json"])),
    ("coverage.xml", ("uv", "run", "coverage", "xml", "-o", PY_COVERAGE_FILES["xml"])),
    ("coverage.lcov", ("uv", "run", "coverage", "lcov", "-o", PY_COVERAGE_FILES["lcov"])),
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
    input=Input.OWNED,
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
    language: Annotated[Language | None, Parameter(parse=False)] = None
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
            case language:
                match super().bound(verb):
                    case Fault() as fault:
                        return fault
                    case bound:
                        return replace(bound, language=language)


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


def _relative_project(project: str | Path, settings: AssaySettings) -> str:
    raw = PurePosixPath(str(project).replace("\\", "/")).as_posix()
    root = PurePosixPath(str(settings.root).replace("\\", "/")).as_posix().rstrip("/")
    return raw.removeprefix(f"{root}/") if root and raw.startswith(f"{root}/") else raw


def _solution_projects(settings: AssaySettings) -> tuple[str, ...]:
    try:
        tree = ET.fromstring(settings.solution.read_bytes() or b"<Solution/>")  # noqa: S314  # trusted local solution XML
    except OSError, ET.ParseError:
        return ()
    return tuple(
        PurePosixPath(path.replace("\\", "/")).as_posix() for node in tree.iter() for path in (node.get("Path") or "",) if path.endswith(".csproj")
    )


def _host_bound(project: str, settings: AssaySettings) -> bool:
    try:
        raw = (settings.root / project).read_bytes()
    except OSError:
        return False
    return any(value.casefold() == "true" for value in parse_csproj(raw, "AssayHostBound"))


def _test_project(project: str, settings: AssaySettings) -> bool:
    try:
        raw = (settings.root / project).read_bytes()
    except OSError:
        return True
    return not any(value.casefold() == "false" for value in parse_csproj(raw, "IsTestProject"))


def _project_lane(project: str | Path, settings: AssaySettings) -> _TestProjectLane:
    rel = _relative_project(project, settings)
    match PurePosixPath(rel).parts:
        case ("tests", "csharp", "_testkit", *_):
            return _TestProjectLane.SUPPORT
        case ("tests", "csharp", "_benchmarks", *_):
            return _TestProjectLane.BENCHMARK
        case ("tests", "csharp", *_) if not _test_project(rel, settings):
            return _TestProjectLane.NON_TEST
        case ("tests", "csharp", *_):
            return _TestProjectLane.HOST_BOUND if _host_bound(rel, settings) else _TestProjectLane.MANAGED
        case _:
            return _TestProjectLane.NON_TEST


def _classified_projects(params: TestParams, routed: Routed, settings: AssaySettings) -> tuple[tuple[str, _TestProjectLane], ...]:
    match (routed.language, params.target, params.all):
        case (Language.CSHARP, Path() as target, _):
            project = _relative_project(target, settings)
            return ((project, _project_lane(project, settings)),)
        case (Language.CSHARP, None, True):
            return tuple((project, _project_lane(project, settings)) for project in _solution_projects(settings))
        case (Language.CSHARP, None, False):
            host = frozenset(routed.host_bound)
            return tuple((project, _TestProjectLane.HOST_BOUND if project in host else _TestProjectLane.MANAGED) for project in routed.projects)
        case _:
            return ()


def _lane_counts(rows: Iterable[tuple[str, StrEnum]]) -> tuple[tuple[str, int], ...]:
    counts = Counter(lane.value for _, lane in rows)
    return tuple((lane.value, counts[lane.value]) for lane in (*_TestProjectLane, *_DiscoveryLane) if counts[lane.value])


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

    # Explicit [paths...] scope the pytest family; the changed-file default and --all keep the full configured suite.
    suite_wide = not params.paths or params.all

    def _check(tool: Tool) -> Check:
        pinned = suite_wide and tool.runner is Runner.UV and tool.input is Input.FILES
        return Check(tool=tool, paths=routed.files, tail=()) if pinned else Check(tool=tool, paths=routed.files)

    selected = tuple(_check(spliced) for t in _rows(routed.language, params, mode) for spliced in (_splice(t),) if spliced is not None)
    return expand(selected, routed, settings=settings)


def _unsupported_scope(routed: Routed, params: TestParams, settings: AssaySettings, mode: Mode) -> tuple[Result[Completed, Fault], ...]:
    def _host_status() -> RailStatus:
        managed = any(project not in routed.host_bound for project in routed.projects)
        return RailStatus.DEGRADED if managed else RailStatus.UNSUPPORTED

    def _target_status() -> tuple[Result[Completed, Fault], ...]:
        match (routed.language, params.target):
            case (Language.CSHARP, Path() as target) if (lane := _project_lane(target, settings)) not in {
                _TestProjectLane.MANAGED,
                _TestProjectLane.HOST_BOUND,
            }:
                project = _relative_project(target, settings)
                return (
                    Ok(
                        receipt(
                            ("assay", "test", "unsupported-target", project),
                            RailStatus.UNSUPPORTED.exit_code,
                            status=RailStatus.UNSUPPORTED,
                            notes=(f"test-target[{lane.value}]: {project}",),
                        )
                    ),
                )
            case _:
                return ()

    def _host_receipt() -> tuple[Result[Completed, Fault], ...]:
        match (routed.language, mode, routed.host_bound):
            case (Language.CSHARP, Mode.RUN | Mode.LIST, (*host,)) if host:
                status = _host_status()
                return (
                    Ok(receipt(("assay", "test", "host-bound"), status.exit_code, status=status, notes=(f"host-bound[csharp]: {', '.join(host)}",))),
                )
            case _:
                return ()

    match (mode, params.mutation):
        case (Mode.MUTATION, MutationLane.CHANGED):
            return tuple(
                Ok(receipt(t.command, 3, status=RailStatus.UNSUPPORTED, notes=(f"{t.name}: no changed-file mutation scope",)))
                for t in _rows(routed.language, params, mode)
                if t.name not in _MUTATION_SCOPE
            )
        case _:
            return (*_target_status(), *_host_receipt())


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...], settings: AssaySettings) -> Result[Block[Routed], Fault]:
    return sequence(block.of_seq(route(language, paths, settings=settings) for language in languages))


def _select(routed: Routed, params: TestParams, settings: AssaySettings) -> Routed:
    # C# dotnet rows consume projects; --target pins one project, --all pins the solution-admitted managed/host lanes.
    match (routed.language.strategy, params.target, params.all):
        case ("glob", _, _):
            return routed
        case (_, Path() as target, _):
            project = _relative_project(target, settings)
            lane = _project_lane(project, settings)
            pinned_projects = (project,) if lane in {_TestProjectLane.MANAGED, _TestProjectLane.HOST_BOUND} else ()
            pinned_host = (project,) if lane is _TestProjectLane.HOST_BOUND else ()
            return msgspec.structs.replace(routed, projects=pinned_projects, host_bound=pinned_host)
        case (_, None, True):
            classified = _classified_projects(params, routed, settings)
            admitted_projects: tuple[str, ...] = tuple(
                project for project, lane in classified if lane in {_TestProjectLane.MANAGED, _TestProjectLane.HOST_BOUND}
            )
            admitted_host: tuple[str, ...] = tuple(project for project, lane in classified if lane is _TestProjectLane.HOST_BOUND)
            return msgspec.structs.replace(routed, scope=Scope.FULL, projects=admitted_projects, host_bound=admitted_host)
        case _:
            return routed


def _dispatch(
    routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode
) -> tuple[Result[Completed, Fault], ...]:
    checks = _checks(routed, params, settings, mode)
    unsupported = _unsupported_scope(routed, params, settings, mode)
    match checks:
        case ():
            return unsupported
        case _ if routed.language is Language.CSHARP and mode in {Mode.RUN, Mode.LIST}:
            # dotnet test builds land in the one shared closure tree; a per-run tree would persist a full
            # solution build per invocation under scope retention. The exclusive lease serializes writers.
            build_scope = ArtifactScope.build(settings, DOTNET_BUILD_CLOSURE)
            resource = f"build-{DOTNET_BUILD_CLOSURE}-{settings.configuration.value}"
            project = ",".join(routed.projects or (routed.language.value,))
            outcome = leased(
                resource,
                lambda _held: Ok(fan_out(checks, settings=settings, scope=build_scope, routed=routed)),
                settings=settings,
                run_id=settings.run_id,
                project=project,
                mode="exclusive",
            )
            match outcome:
                case Result(tag="ok", ok=rows):
                    return (*rows, *unsupported)
                case Result(error=fault):
                    return (Error(fault), *unsupported)
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


def _status_matches(outcomes: tuple[Completed, ...]) -> tuple[Match, ...]:
    return tuple(
        Match(
            id=" ".join(c.argv) if c.argv else "test",
            kind=ArtifactKind.TEST,
            text=(c.stderr or c.stdout or "\n".join(c.notes).encode())[-4096:].decode(errors="replace").strip(),
            severity=c.status.value,
        )
        for c in outcomes
        if c.status not in {RailStatus.OK, RailStatus.EMPTY, RailStatus.SKIP, RailStatus.FAILED}
    )


def _discovery_counts(outcomes: tuple[Completed, ...], discovered: tuple[Match, ...]) -> tuple[tuple[str, int], ...]:
    empty_or_failed = sum(
        1
        for c in outcomes
        if c.status.severity > RailStatus.OK.severity or (c.status.severity <= RailStatus.OK.severity and not _roster_matches((c,)))
    )
    return _lane_counts((m.id, _DiscoveryLane.LISTED) for m in discovered) + _lane_counts(
        tuple((str(index), _DiscoveryLane.EMPTY_OR_FAILED) for index in range(empty_or_failed))
    )


def _run_detail(
    detail: AnyDetail | None, *, project_counts: tuple[tuple[str, int], ...], discovery_counts: tuple[tuple[str, int], ...] = ()
) -> TestRun | AnyDetail | None:
    match (detail, bool(project_counts or discovery_counts)):
        case (TestRun() as run, _):
            return msgspec.structs.replace(run, project_counts=project_counts, discovery_counts=discovery_counts)
        case (None, True):
            return TestRun(project_counts=project_counts, discovery_counts=discovery_counts)
        case _:
            return detail


def _test_detail(done: Completed) -> TestRun:
    try:
        return _TESTS.decode(done.stdout or b"{}")
    except msgspec.DecodeError:
        return TestRun()


def _detail(done: tuple[Completed, ...], params: TestParams, root: Path) -> AnyDetail | None:
    # Mutation evidence is stdout-derived; use the first decoded receipt carrying a real mutation lane.
    mutation = next((d for c in done if (d := _test_detail(c)).mutation is not MutationLane.OFF), TestRun())
    # coverage_percent reads/decodes the artifact; only pay that cost on the coverage-requested arm.
    match (params.mutation, params.coverage):
        case (MutationLane.OFF, False):
            return None
        case (_, True) if (total := coverage_percent(root)) is not None:
            return msgspec.structs.replace(mutation, coverage=total)
        case (MutationLane.CHANGED | MutationLane.FULL, _):
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
    root = Path(str(settings.root)) / PY_ARTIFACT_ROOTS["coverage"]
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

    def _resolved(languages: tuple[Language, ...]) -> Result[Report, Fault]:
        eligible = tuple(
            t for language in languages for t in _rows(language, params, Mode.MUTATION if params.mutation is not MutationLane.OFF else mode)
        )
        gap = _mutation_gap(params, eligible)
        match gap:
            case True:
                _LOG.warning(_GAP_NOTE, claim=claim.value, verb=verb, language=params.language.value if params.language is not None else "")
            case False:
                pass

        def _work(_held: object = None) -> Result[Report, Fault]:
            def _settle(done: Block[Completed], selected: tuple[Routed, ...]) -> Report:
                outcomes = tuple(done)
                project_counts = _lane_counts(row for r in selected for row in _classified_projects(params, r, settings))
                detail = _run_detail(_detail(outcomes, params, Path(str(settings.root))), project_counts=project_counts)
                base = fold(claim, verb, outcomes, detail=detail, promote_empty=True)
                return msgspec.structs.replace(
                    base,
                    results=(*base.results, *_status_matches(outcomes)),
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
                        f"mutation-{head}",
                        lambda _held: _nested(tuple(rest)),
                        settings=settings,
                        run_id=settings.run_id,
                        project=head,
                        mode="exclusive",
                    )
                case _:
                    return _work()

        return _nested(tuple(sorted({t.language.value for t in eligible if t.mode is Mode.MUTATION})))

    return resolve_languages(params.language, params.paths, claim=claim).bind(_resolved)


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
    needle = params.grep.strip().lower()

    def _settle(done: block.Block[Completed], selected: tuple[Routed, ...]) -> Report:
        outcomes = tuple(done)
        base = fold(Claim.TEST, "list", outcomes, promote_empty=True)
        discovered = tuple(m for m in _roster_matches(outcomes) if not needle or needle in m.text.lower())
        roster = discovered[: params.limit] if params.limit > 0 else discovered
        project_counts = _lane_counts(row for r in selected for row in _classified_projects(params, r, settings))
        discovery_counts = _discovery_counts(outcomes, discovered)
        return msgspec.structs.replace(
            base,
            status=RailStatus.OK if roster and base.status.severity <= RailStatus.OK.severity else base.status,
            counts=Counts(len(roster), base.counts.failed, len(roster) + base.counts.failed),
            results=(*roster, *base.results, *_status_matches(outcomes)),
            artifacts=(*base.artifacts, _results_artifact(scope), *_roster_artifacts(settings, scope, discovered)),
            notes=(
                *base.notes,
                *(n for r in selected for n in r.closure_note()),
                *((f"discovery: total={len(discovered)} returned={len(roster)}",) if discovered else ()),
                *(
                    f"discovery {c.status.value}: {' '.join(c.argv)[:120]}{f': {tail}' if tail else ''}"
                    for c in outcomes
                    if c.status.severity > RailStatus.OK.severity
                    for tail in ((c.stderr or c.stdout)[-256:].decode(errors="replace").strip(),)
                ),
            ),
            detail=_run_detail(TestRun(selected=len(discovered)), project_counts=project_counts, discovery_counts=discovery_counts),
        )

    return resolve_languages(params.language, params.paths, claim=Claim.TEST).bind(
        lambda languages: (
            _routed(languages, params.paths, settings)
            .map(lambda routed: tuple(_select(r, params, settings) for r in routed))
            .bind(
                lambda selected: sequence(
                    block.of_seq(row for r in selected for row in _dispatch(r, params, settings=settings, scope=scope, mode=Mode.LIST))
                ).map(lambda done: _settle(done, selected))
            )
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
