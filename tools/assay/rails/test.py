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
from tools.assay.composition.settings import AssaySettings  # noqa: TC001  # beartype resolves rail annotations at runtime
from tools.assay.composition.store import (  # noqa: TC001  # beartype resolves ArtifactScope/ArtifactStore annotations at runtime
    ArtifactScope,
    ArtifactStore,
    CS_ARTIFACT_ROOTS,
    DOTNET_BUILD_CLOSURE,
    PY_ARTIFACT_ROOTS,
    PY_COVERAGE_FILES,
)
from tools.assay.core.exec import Executor  # noqa: TC001  # beartype resolves the executor-port annotation at runtime
from tools.assay.core.govern import leased
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,  # noqa: TC001  # _roster_matches uses Completed as a runtime type in the tuple annotation
    Counts,
    Fault,  # beartype @checked resolves the rail's forward-ref (PEP 649)
    Input,
    Language,
    language_choice,
    Match,
    Mode,
    MutationLane,
    RailStatus,
    receipt,
    Report,  # noqa: TC001  # beartype @checked resolves the rail's forward-ref (PEP 649)
    Runner,
    TestRun,
    Tool,
    ToolArgs,
    ToolGroup,
)
from tools.assay.core.routing import expand, parse_csproj, resolve_languages, route, Scope
from tools.assay.diagnostics import fold


if TYPE_CHECKING:
    from expression.collections import Block

    from tools.assay.core.model import AnyDetail
    from tools.assay.core.routing import Routed


# --- [TYPES] ----------------------------------------------------------------------------


class _TestProjectLane(StrEnum):
    MANAGED = "managed"
    HOST_BOUND = "host_bound"
    SHELL = "shell"
    SUPPORT = "support"
    BENCHMARK = "benchmark"
    NON_TEST = "non_test"


class _DiscoveryLane(StrEnum):
    LISTED = "listed"
    EMPTY_OR_FAILED = "empty_or_failed_discovery"


# --- [CONSTANTS] ------------------------------------------------------------------------

_GAP_NOTE: str = "mutation requested but no eligible lane (typescript has no mutation runner)"
# Only these lanes reach dotnet dispatch; SHELL/SUPPORT/BENCHMARK/NON_TEST rows are report evidence, never test targets.
_RUNNABLE_LANES: frozenset[_TestProjectLane] = frozenset((_TestProjectLane.MANAGED, _TestProjectLane.HOST_BOUND))
_COVERAGE_JSON: str = PY_COVERAGE_FILES["json"]
_COVERAGE_OUTPUTS: tuple[tuple[str, tuple[str, ...]], ...] = (
    ("coverage.json", ("uv", "run", "coverage", "json", "-o", PY_COVERAGE_FILES["json"])),
    ("coverage.xml", ("uv", "run", "coverage", "xml", "-o", PY_COVERAGE_FILES["xml"])),
    ("coverage.lcov", ("uv", "run", "coverage", "lcov", "-o", PY_COVERAGE_FILES["lcov"])),
)

# --- [TABLES] ---------------------------------------------------------------------------

_TESTS = msgspec.json.Decoder(TestRun)
_ROSTER_ENCODER = msgspec.json.Encoder(order="deterministic")
# csproj marker -> lane rows in dispatch priority: the shell marker wins because shell content ships via the bridge closure.
_MARKER_LANES: tuple[tuple[str, str, _TestProjectLane], ...] = (
    ("AssayTestShell", "true", _TestProjectLane.SHELL),
    ("IsTestProject", "false", _TestProjectLane.NON_TEST),
    ("AssayHostBound", "true", _TestProjectLane.HOST_BOUND),
)
# Runner-specific CHANGED mutation scopes: Stryker accepts file globs; mutmut requires module-dotted mutant names.
# Runners absent here surface UNSUPPORTED on the CHANGED lane.
_MUTATION_SCOPE: dict[str, Callable[[tuple[str, ...]], tuple[str, ...]]] = {
    "dotnet-stryker": lambda files: tuple(flag for f in files for flag in ("--mutate", f)),
    "mutmut": lambda files: tuple(f"{f.removesuffix('.py').replace('/', '.')}.*" for f in files),
}
# mutmut self-parallelizes below the rail governor; --max-children caps that second tier.
_MUTATION_GOVERNOR: frozenset[str] = frozenset(("mutmut",))

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
    trx: bool = False
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


@dataclass(frozen=True, slots=True)
class _Selected:
    """One language's dispatch-admitted routing plus its full marker-lane classification."""

    routed: Routed
    lanes: tuple[tuple[str, _TestProjectLane], ...] = ()


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
    modes = {Mode.MUTATION: frozenset((Mode.MUTATION,)), Mode.CLIENT: frozenset((Mode.CLIENT,)), Mode.STAGE: frozenset((Mode.STAGE,))}.get(
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


def _mutation_args(tool: Tool, params: TestParams, settings: AssaySettings, files: tuple[str, ...]) -> ToolArgs | None:
    """Project one mutation row's typed splice values: governor cap, changed-file scope, and the staged Stryker anchors.

    Returns:
        Typed splice values for the row's holes, or ``None`` when the CHANGED lane has no scope projection for the runner.
    """
    match (params.mutation, _MUTATION_SCOPE.get(tool.name)):
        case (MutationLane.CHANGED, None):
            return None
        case (MutationLane.CHANGED, scoped) if scoped is not None:
            scope = scoped(files)
        case _:
            scope = ()
    # A staged dotnet mutation row (Stryker) runs from an empty work root: absolute --solution/--output anchor it to
    # the real tree; Stryker.NET requires the report --output dir to pre-exist, so the rail creates it here.
    staged_dotnet = tool.runner is Runner.DOTNET and bool(tool.stage.root)
    output = str(Path(str(settings.root)).resolve() / CS_ARTIFACT_ROOTS["stryker-output"]) if staged_dotnet else ""
    if output:
        Path(output).mkdir(parents=True, exist_ok=True)
    return ToolArgs(
        max_children=str(settings.mutation_max_cpu) if tool.name in _MUTATION_GOVERNOR else "",
        output=output,
        scope=scope,
        solution=str(settings.solution) if staged_dotnet else "",
    )


def _relative_project(project: str | Path, settings: AssaySettings) -> str:
    raw = PurePosixPath(str(project).replace("\\", "/"))
    root = PurePosixPath(str(settings.root).replace("\\", "/"))
    return raw.relative_to(root).as_posix() if raw.is_relative_to(root) else raw.as_posix()


def _solution_projects(settings: AssaySettings) -> Result[tuple[str, ...], Fault]:
    # A corrupt or missing Workspace.slnx must fault loudly: folding to () would green-exit --all with zero checks.
    try:
        tree = ET.fromstring(settings.solution.read_bytes() or b"<Solution/>")  # noqa: S314  # trusted local solution XML
    except (OSError, ET.ParseError) as exc:
        return Error(Fault(("test", "solution", str(settings.solution)), RailStatus.FAULTED, str(exc)[:1024]))
    rows = (path for node in tree.iter() for path in (node.get("Path") or "",) if path.endswith(".csproj"))
    return Ok(tuple(PurePosixPath(path.replace("\\", "/")).as_posix() for path in rows))


def _marker_lane(project: str, settings: AssaySettings) -> _TestProjectLane:
    # One csproj read owns marker classification; an unreadable project is fault-shaped NON_TEST, never silently MANAGED.
    try:
        raw = (settings.root / project).read_bytes()
    except OSError:
        return _TestProjectLane.NON_TEST
    return next(
        (lane for tag, token, lane in _MARKER_LANES if any(value.casefold() == token for value in parse_csproj(raw, tag))), _TestProjectLane.MANAGED
    )


def _project_lane(project: str | Path, settings: AssaySettings) -> _TestProjectLane:
    rel = _relative_project(project, settings)
    match PurePosixPath(rel).parts:
        case ("tests", "csharp", "_testkit", *_):
            return _TestProjectLane.SUPPORT
        case ("tests", "csharp", "_benchmarks", *_):
            return _TestProjectLane.BENCHMARK
        case ("tests", "csharp", *_):
            return _marker_lane(rel, settings)
        case _:
            return _TestProjectLane.NON_TEST


def _lane_counts(rows: Iterable[tuple[str, StrEnum]]) -> tuple[tuple[str, int], ...]:
    counts = Counter(lane.value for _, lane in rows)
    return tuple((lane.value, counts[lane.value]) for lane in (*_TestProjectLane, *_DiscoveryLane) if counts[lane.value])


def _checks(routed: Routed, params: TestParams, settings: AssaySettings, mode: Mode) -> tuple[Check, ...]:
    # MTP consumes the filter through the dotnet rows' {filter*} hole; CHANGED mutation rows scope to routed files.
    filt = _filter(params.filter)

    def _args(tool: Tool) -> ToolArgs | None:
        match (tool.mode, bool(filt), tool.runner):
            case (Mode.MUTATION, _, _):
                return _mutation_args(tool, params, settings, routed.files)
            case (Mode.RUN | Mode.LIST, True, Runner.DOTNET):
                return ToolArgs(filter=filt)
            case _:
                return ToolArgs()

    # Explicit [paths...] scope the pytest family; the changed-file default and --all keep the full configured suite.
    suite_wide = not params.paths or params.all

    def _check(tool: Tool, args: ToolArgs) -> Check:
        pinned = suite_wide and tool.runner is Runner.UV and tool.input is Input.FILES
        return Check(tool=tool, paths=routed.files, tail=() if pinned else None, args=args)

    def _trx(check: Check) -> Check:
        # Opt-in TRX evidence rides the {flags*} hole per expanded project; the pinned tail (or the deferred
        # single-project placement) names the results dir, an unpinned suite run lands under "solution".
        if not (params.trx and check.tool.runner is Runner.DOTNET and check.tool.mode is Mode.RUN):
            return check
        deferred = routed.projects[0] if check.tail is None and len(routed.projects) == 1 else ""
        sources = (*(check.tail or ()), deferred)
        project = next((PurePosixPath(part.replace("\\", "/")).stem for part in sources if part.endswith(".csproj")), "solution")
        trx_dir = Path(str(settings.root)).resolve() / CS_ARTIFACT_ROOTS["trx"] / project
        return msgspec.structs.replace(check, args=msgspec.structs.replace(check.args, flags=("--report-trx", "--results-directory", str(trx_dir))))

    selected = tuple(_check(t, args) for t in _rows(routed.language, params, mode) for args in (_args(t),) if args is not None)
    return tuple(_trx(check) for check in expand(selected, routed, settings=settings))


def _unsupported_scope(routed: Routed, params: TestParams, settings: AssaySettings, mode: Mode) -> tuple[Result[Completed, Fault], ...]:
    def _host_status() -> RailStatus:
        managed = any(project not in routed.host_bound for project in routed.projects)
        return RailStatus.DEGRADED if managed else RailStatus.UNSUPPORTED

    def _target_status() -> tuple[Result[Completed, Fault], ...]:
        match (routed.language, params.target):
            case (Language.CSHARP, Path() as target) if (lane := _project_lane(target, settings)) not in _RUNNABLE_LANES:
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
                Ok(
                    receipt(
                        t.command,
                        RailStatus.UNSUPPORTED.exit_code,
                        status=RailStatus.UNSUPPORTED,
                        notes=(f"{t.name}: no changed-file mutation scope",),
                    )
                )
                for t in _rows(routed.language, params, mode)
                if t.name not in _MUTATION_SCOPE
            )
        case _:
            return (*_target_status(), *_host_receipt())


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...], settings: AssaySettings) -> Result[Block[Routed], Fault]:
    return sequence(block.of_seq(route(language, paths, settings=settings) for language in languages))


def _select(routed: Routed, params: TestParams, settings: AssaySettings) -> Result[_Selected, Fault]:
    """Classify C# projects through the marker-lane table once; only MANAGED/HOST_BOUND rows reach dotnet dispatch.

    Every scoping arm (--target, --all, changed closure) classifies and filters, so the marker table is the single
    authority over dispatch admission and host_bound is rebuilt from it rather than trusted from routing.

    Returns:
        Lane-filtered selection carrying the full classification, or the solution-roster fault under ``--all``.
    """

    def classify(projects: Iterable[str]) -> tuple[tuple[str, _TestProjectLane], ...]:
        return tuple((project, _project_lane(project, settings)) for project in projects)

    def admit(lanes: tuple[tuple[str, _TestProjectLane], ...], scope: Scope = routed.scope) -> _Selected:
        return _Selected(
            routed=msgspec.structs.replace(
                routed,
                scope=scope,
                projects=tuple(project for project, lane in lanes if lane in _RUNNABLE_LANES),
                host_bound=tuple(project for project, lane in lanes if lane is _TestProjectLane.HOST_BOUND),
            ),
            lanes=lanes,
        )

    match (routed.language.strategy, params.target, params.all):
        case ("glob", _, _):
            return Ok(_Selected(routed=routed))
        case (_, Path() as target, _):
            return Ok(admit(classify((_relative_project(target, settings),))))
        case (_, None, True):
            return _solution_projects(settings).map(lambda projects: admit(classify(projects), Scope.FULL))
        case _:
            return Ok(admit(classify(routed.projects)))


def _dispatch(
    routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode, executor: Executor
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
                lambda _held: Ok(executor.fan(checks, settings=settings, scope=build_scope, routed=routed)),
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
            return (*executor.fan(checks, settings=settings, scope=scope, routed=routed), *unsupported)


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
    done: tuple[Result[Completed, Fault], ...],
    routed: Routed,
    params: TestParams,
    *,
    settings: AssaySettings,
    scope: ArtifactScope,
    executor: Executor,
) -> tuple[Result[Completed, Fault], ...]:
    # Kill-rate gate rides the held mutation lease: one sequential check against the staged mutmut cache.
    # The gate is the catalog VERIFY row tagged MUTATION; VERIFY keeps it off every dispatch fan.
    staged = next((t for t in _rows(routed.language, params, Mode.MUTATION) if t.stage.root), None)
    gate_row = next((t for t in select(Claim.TEST, routed.language) if t.mode is Mode.VERIFY and ToolGroup.MUTATION in t.groups), None)
    succeeded = any(c.returncode == 0 and "mutmut" in c.argv for r in done if r.is_ok() for c in (r.ok,))
    match (staged, gate_row, succeeded):
        case (Tool() as row, Tool() as gate_tool, True):
            work = Path(str(settings.root)) / row.stage.root
            return executor.fan((Check(tool=gate_tool, cwd=work),), settings=settings, scope=scope, routed=routed)
        case _:
            return ()


def _dispatch_all(
    routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode, executor: Executor
) -> tuple[Result[Completed, Fault], ...]:
    run = _dispatch(routed, params, settings=settings, scope=scope, mode=mode, executor=executor)
    mutation = (
        _dispatch(routed, params, settings=settings, scope=scope, mode=Mode.MUTATION, executor=executor)
        if params.mutation is not MutationLane.OFF and mode is Mode.RUN
        else ()
    )
    gate = _gate(mutation, routed, params, settings=settings, scope=scope, executor=executor) if mutation else ()
    # The STAGE combine fan completes before the CLIENT report fan starts, so every report row reads combined data.
    reporting = params.coverage and mode is Mode.RUN
    combine = _dispatch(routed, params, settings=settings, scope=scope, mode=Mode.STAGE, executor=executor) if reporting else ()
    coverage = _dispatch(routed, params, settings=settings, scope=scope, mode=Mode.CLIENT, executor=executor) if reporting else ()
    return (*run, *mutation, *gate, *combine, *coverage)


def _thin_rail(
    settings: AssaySettings, scope: ArtifactScope, params: TestParams, executor: Executor, *, claim: Claim, verb: str, mode: Mode
) -> Result[Report, Fault]:
    """Run routed test eligibility, fan-out, mutation gating, and folding.

    Returns:
        Folded test report, or routing/spawn/lease fault.
    """

    def _resolved(languages: tuple[Language, ...]) -> Result[Report, Fault]:
        eligible = tuple(
            t for language in languages for t in _rows(language, params, Mode.MUTATION if params.mutation is not MutationLane.OFF else mode)
        )
        gap = _mutation_gap(params, eligible)
        if gap:
            _LOG.warning(_GAP_NOTE, claim=claim.value, verb=verb, language=params.language.value if params.language is not None else "")

        def _work(_held: object = None) -> Result[Report, Fault]:
            def _settle(done: Block[Completed], selected: tuple[_Selected, ...]) -> Report:
                outcomes = tuple(done)
                project_counts = _lane_counts(row for s in selected for row in s.lanes)
                detail = _run_detail(_detail(outcomes, params, Path(str(settings.root))), project_counts=project_counts)
                base = fold(claim, verb, outcomes, detail=detail, promote_empty=True)
                return msgspec.structs.replace(
                    base,
                    results=(*base.results, *_status_matches(outcomes)),
                    artifacts=(*base.artifacts, _results_artifact(scope), *_coverage_artifacts(settings, scope, outcomes)),
                    notes=(*base.notes, *(note for s in selected for note in s.routed.closure_note()), *((_GAP_NOTE,) if gap else ())),
                )

            return (
                _routed(languages, params.paths, settings)
                .bind(lambda routed: sequence(block.of_seq(_select(r, params, settings) for r in routed)))
                .bind(
                    lambda selected: sequence(
                        block.of_seq(
                            row
                            for s in selected
                            for row in _dispatch_all(s.routed, params, settings=settings, scope=scope, mode=mode, executor=executor)
                        )
                    ).map(lambda done: _settle(done, tuple(selected)))
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


def run(settings: AssaySettings, scope: ArtifactScope, params: TestParams, executor: Executor) -> Result[Report, Fault]:
    """Run eligible test suites.

    Returns:
        Test run report, or routing/spawn fault.
    """
    return _thin_rail(settings, scope, params, executor, claim=Claim.TEST, verb="run", mode=Mode.RUN)


def list(settings: AssaySettings, scope: ArtifactScope, params: TestParams, executor: Executor) -> Result[Report, Fault]:  # noqa: A001
    """List discovered tests.

    The name must match the registry-bound Handler name for verb dispatch.

    Returns:
        Test roster report, or routing/spawn fault.
    """
    needle = params.grep.strip().lower()

    def _settle(done: block.Block[Completed], selected: tuple[_Selected, ...]) -> Report:
        outcomes = tuple(done)
        base = fold(Claim.TEST, "list", outcomes, promote_empty=True)
        discovered = tuple(m for m in _roster_matches(outcomes) if not needle or needle in m.text.lower())
        roster = discovered[: params.limit] if params.limit > 0 else discovered
        project_counts = _lane_counts(row for s in selected for row in s.lanes)
        discovery_counts = _discovery_counts(outcomes, discovered)
        return msgspec.structs.replace(
            base,
            status=RailStatus.OK if roster and base.status.severity <= RailStatus.OK.severity else base.status,
            counts=Counts(len(roster), base.counts.failed, len(roster) + base.counts.failed),
            results=(*roster, *base.results, *_status_matches(outcomes)),
            artifacts=(*base.artifacts, _results_artifact(scope), *_roster_artifacts(settings, scope, discovered)),
            notes=(
                *base.notes,
                *(n for s in selected for n in s.routed.closure_note()),
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
            .bind(lambda routed: sequence(block.of_seq(_select(r, params, settings) for r in routed)))
            .bind(
                lambda selected: sequence(
                    block.of_seq(
                        row
                        for s in selected
                        for row in _dispatch(s.routed, params, settings=settings, scope=scope, mode=Mode.LIST, executor=executor)
                    )
                ).map(lambda done: _settle(done, tuple(selected)))
            )
        )
    )


def coverage(settings: AssaySettings, scope: ArtifactScope, params: TestParams, executor: Executor) -> Result[Report, Fault]:
    """Run eligible suites under coverage.

    Returns:
        Coverage test report, or routing/spawn fault.
    """
    return _thin_rail(settings, scope, replace(params, coverage=True, benchmark=False), executor, claim=Claim.TEST, verb="coverage", mode=Mode.RUN)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TestParams", "coverage", "coverage_percent", "list", "run"]
