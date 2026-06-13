"""Static analysis rail: scoped route preview, diagnostics/build, and native fix."""

from collections.abc import Callable  # noqa: TC003  # runtime: callable annotation is resolved through the rail layer
from dataclasses import dataclass
from hashlib import sha256
from pathlib import PurePosixPath
from typing import Annotated, TYPE_CHECKING

from cyclopts import Parameter
from expression import Error, Ok, Result
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # runtime: public rail signatures are inspected
from tools.assay.core.engine import _concurrency_pressure, argv_for, fan_out, leased  # noqa: PLC2701
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    Check,
    Claim,
    Fault,
    fold,
    Input,
    Language,
    Match,
    Mode,
    receipt,
    Report,  # noqa: TC001 - beartype resolves runtime annotations
    Runner,
    StaticRun,
    Tool,  # noqa: TC001 - runtime: Tool participates in public check expansion annotations
)
from tools.assay.core.routing import expand, infer_languages, route, Routed, Scope, target_files, TargetFiles
from tools.assay.core.status import RailStatus, Step


if TYPE_CHECKING:
    from tools.assay.core.model import Completed


# --- [TYPES] ----------------------------------------------------------------------------

type PhaseChecks = tuple[tuple[str, tuple[Check, ...]], ...]
type SkipRows = tuple[tuple[str, str, str], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

_BUILD_MODES: tuple[Mode, ...] = (Mode.CHECK, Mode.RESTORE, Mode.BUILD)
_FIX_MODES: tuple[Mode, ...] = (Mode.WRITE,)
_PREVIEW: tuple[tuple[str, tuple[Mode, ...]], ...] = (("build", _BUILD_MODES), ("fix", _FIX_MODES))

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class StaticParams:
    """Parameters for static check/build/fix."""

    all: Annotated[
        bool, Parameter(name="--all", show_default=False, help="Whole-workspace C# target; explicit because it may build the full solution.")
    ] = False
    project: Annotated[str, Parameter(name="--project", show_default=False, help="Single C# project target.")] = ""
    folders: Annotated[
        tuple[str, ...],
        Parameter(
            name="--folder",
            consume_multiple=True,
            allow_repeating=True,
            negative_iterable=(),
            show_default=False,
            help="Folder target(s); consumes values until the next option.",
        ),
    ] = ()
    files: Annotated[
        tuple[str, ...],
        Parameter(
            name="--file",
            consume_multiple=True,
            allow_repeating=True,
            negative_iterable=(),
            show_default=False,
            help="File target(s); consumes values until the next option.",
        ),
    ] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _closure_sha(routed: Routed) -> str:
    return sha256("\n".join(routed.projects).encode()).hexdigest()[:16]


def _route_sha(routed: Routed) -> str:
    seed = "\n".join((*routed.projects, *routed.files, *routed.full_triggers)) or routed.language.name
    return sha256(seed.encode()).hexdigest()[:16]


def _builds_closure(routed: Routed) -> bool:
    return bool(routed.projects) or _workspace_route(routed)


def _build_sha(routed: Routed) -> str:
    return _closure_sha(routed) if routed.projects else _route_sha(routed)


def _scoped_settings(settings: AssaySettings) -> AssaySettings:
    return settings.model_copy(update={"trigger_files": frozenset(), "trigger_prefixes": ()})


def _workspace_route(routed: Routed) -> bool:
    return routed.language is Language.CSHARP and routed.scope is Scope.FULL and not routed.files and not routed.projects


def _phase(mode: Mode) -> str:
    return {Mode.CHECK: "diagnostic", Mode.RESTORE: "restore", Mode.BUILD: "build", Mode.WRITE: "fix"}.get(mode, mode.value)


def _sarif_pin(tool: Tool, scope: ArtifactScope) -> Tool:
    match (tool.runner, tool.mode):
        case (Runner.DOTNET, Mode.BUILD):
            return msgspec.structs.replace(tool, command=(*tool.command, f"-p:CspSarifDir={scope.sarif_dir}"))
        case _:
            return tool


def _routed_tool(tool: Tool, routed: Routed) -> Tool:
    match (routed.language, routed.scope, tool.input, bool(routed.projects)):
        case (Language.CSHARP, Scope.FULL, Input.INCLUDE | Input.PROJECT, _):
            return msgspec.structs.replace(tool, input=Input.SOLUTION)
        case (Language.CSHARP, Scope.CHANGED, Input.INCLUDE, True) if not routed.groups:
            return msgspec.structs.replace(tool, input=Input.PROJECT)
        case _:
            return tool


def _tool_skip(tool: Tool, routed: Routed) -> str:
    match (tool.input, routed.language.strategy):
        case (Input.SOLUTION, _) if not _workspace_route(routed):
            return "solution input unsupported by scoped static"
        case (Input.PROJECT, "glob"):
            return "project-wide tool unsupported by scoped static"
        case _:
            return ""


def _phase_checks(routed: Routed, modes: tuple[Mode, ...], settings: AssaySettings, scope: ArtifactScope) -> tuple[PhaseChecks, SkipRows]:
    rows = tuple(
        (_phase(active), projected, _tool_skip(projected, routed))
        for active in modes
        for tool in select(Claim.STATIC, routed.language)
        if tool.mode is active
        for projected in (_routed_tool(tool, routed),)
    )
    selected = tuple((phase, Check(tool=_sarif_pin(tool, scope), paths=routed.files)) for phase, tool, reason in rows if not reason)
    skipped = tuple((phase, tool.name, reason) for phase, tool, reason in rows if reason)
    expanded = tuple((phase, clone) for phase, check in selected for clone in expand((check,), routed, settings=settings))
    phases = tuple(dict.fromkeys(_phase(mode) for mode in modes))
    return tuple((phase, tuple(check for row_phase, check in expanded if row_phase == phase)) for phase in phases), skipped


def _all_checks(phases: PhaseChecks) -> tuple[Check, ...]:
    return tuple(check for _, checks in phases for check in checks)


def _argv(check: Check, routed: Routed, settings: AssaySettings, scope: ArtifactScope) -> str:
    return argv_for(check, routed, settings=settings, scope=scope).map(" ".join).default_value(f"{check.tool.name}:<unplaced>")


def _planned(verb: str, routed: Routed, phases: PhaseChecks, settings: AssaySettings, scope: ArtifactScope) -> tuple[tuple[str, str, str], ...]:
    active_scope = ArtifactScope.build(settings, _build_sha(routed)) if verb == "build" and _builds_closure(routed) else scope
    return tuple((phase, check.tool.name, _argv(check, routed, settings, active_scope)) for phase, checks in phases for check in checks)


def _artifacts(settings: AssaySettings, routed: tuple[Routed, ...]) -> tuple[Artifact, ...]:
    return tuple(
        Artifact(id=f"build-{sha}", kind=ArtifactKind.SCOPE, path=ArtifactScope.build(settings, sha).path)
        for route_row in routed
        for sha in (_build_sha(route_row),)
        if _builds_closure(route_row)
    )


def _slot_waits(notes: tuple[str, ...]) -> tuple[float, ...]:
    return tuple(
        float(part.removeprefix("wait_ms="))
        for note in notes
        if note.startswith("dotnet.slot ")
        for part in note.split()
        if part.startswith("wait_ms=") and part.removeprefix("wait_ms=").replace(".", "", 1).isdigit()
    )


def _resources(settings: AssaySettings, checks: tuple[Check, ...], notes: tuple[str, ...] = ()) -> tuple[tuple[str, float], ...]:
    pressure = _concurrency_pressure(settings, checks)
    waits = _slot_waits(notes)
    return (
        ("dotnet.slots", float(pressure.slots)),
        ("dotnet.foreign", float(pressure.foreign_dotnet)),
        ("memory.percent", pressure.mem_percent),
        ("concurrency.original", float(pressure.original)),
        ("concurrency.reduced", float(pressure.reduced)),
        ("dotnet.pressure", float(pressure.dotnet_pressure)),
        ("memory.pressure", float(pressure.mem_pressure)),
        ("dotnet.slot_wait_ms.max", max(waits, default=0.0)),
    )


def _route_rows(routed: tuple[Routed, ...]) -> tuple[tuple[str, ...], ...]:
    return tuple(
        (
            route_row.language.value,
            route_row.scope.value,
            str(len(route_row.files)),
            str(len(route_row.projects)),
            str(len(route_row.full_triggers)),
            str(len(route_row.groups)),
        )
        for route_row in routed
    )


def _build_target(targets: TargetFiles) -> bool:
    return len(targets.targets) == 1 and targets.targets[0][0] in {"all", "project"}


def _matches(targets: TargetFiles, routed: tuple[Routed, ...], skipped: SkipRows) -> tuple[Match, ...]:
    route_matches = tuple(
        Match(
            id=route_row.language.value,
            kind=ArtifactKind.SCOPE,
            text=(
                f"scope={route_row.scope.value} files={len(route_row.files)} projects={len(route_row.projects)} "
                f"triggers={len(route_row.full_triggers)} groups={len(route_row.groups)}"
            ),
        )
        for route_row in routed
    )
    rejected = tuple(Match(id=path or kind, kind=ArtifactKind.SCOPE, text=reason, severity="skipped") for kind, path, reason in targets.rejected)
    skipped_matches = tuple(Match(id=name, kind=ArtifactKind.SCOPE, text=reason, severity="skipped") for _, name, reason in skipped)
    return (*route_matches, *rejected, *skipped_matches)


def _detail(
    targets: TargetFiles,
    routed: tuple[Routed, ...],
    planned: tuple[tuple[str, str, str], ...],
    skipped: SkipRows,
    artifacts: tuple[Artifact, ...],
    settings: AssaySettings,
    checks: tuple[Check, ...],
    notes: tuple[str, ...] = (),
) -> StaticRun:
    return StaticRun(
        targets=targets.targets,
        routes=_route_rows(routed),
        planned=planned,
        skipped=(*targets.rejected, *skipped),
        phases=tuple(dict.fromkeys(row[0] for row in planned)),
        resources=_resources(settings, checks, notes),
        artifacts=tuple(artifact.path for artifact in artifacts),
    )


def _target_result(settings: AssaySettings, params: StaticParams, verb: str) -> Result[TargetFiles, Fault]:
    path_targets = bool(params.folders or params.files)
    direct_targets = int(params.all) + int(bool(params.project)) + int(path_targets)
    if direct_targets > 1:
        result = Error(Fault((), RailStatus.UNSUPPORTED, f"{Step.PARSE}: {verb}: choose only one of --all, --project, or folder/file targets"))
    elif params.all:
        result = Ok(TargetFiles(targets=(("all", str(settings.solution)),)))
    elif params.project:
        result = _project_target(settings, params.project, verb)
    elif verb == "check":
        result = target_files(params.folders, params.files, settings=settings, changed=not path_targets)
    elif verb == "build" or not path_targets:
        status = RailStatus.UNSUPPORTED if verb == "build" else RailStatus.FAULTED
        message = (
            f"{Step.PARSE}: build: requires --project or --all"
            if verb == "build"
            else f"{Step.PARSE}: {verb}: requires --folder, --file, --project, or --all"
        )
        result = Error(Fault((), status, message))
    else:
        result = target_files(params.folders, params.files, settings=settings).bind(lambda targets: _execution_targets(targets, verb))
    return result


def _project_target(settings: AssaySettings, project: str, verb: str) -> Result[TargetFiles, Fault]:
    rel = PurePosixPath(project).as_posix()
    match (PurePosixPath(rel).suffix, (settings.root / rel).is_file()):
        case (".csproj", True):
            return Ok(TargetFiles(targets=(("project", rel),)))
        case (".csproj", False):
            return Error(Fault((), RailStatus.UNSUPPORTED, f"{Step.PARSE}: {verb}: project not found: {rel}"))
        case _:
            return Error(Fault((), RailStatus.UNSUPPORTED, f"{Step.PARSE}: {verb}: --project requires a .csproj path: {rel}"))


def _execution_targets(targets: TargetFiles, verb: str) -> Result[TargetFiles, Fault]:
    explicit = tuple(path for kind, path, _ in targets.rejected if kind == "file")
    match (explicit, targets.files):
        case ((_, *_), _):
            return Error(Fault((), RailStatus.UNSUPPORTED, f"{Step.PARSE}: {verb}: unsupported --file target(s): {', '.join(explicit)}"))
        case (_, ()):
            return Error(Fault((), RailStatus.UNSUPPORTED, f"{Step.PARSE}: {verb}: no supported source files under target(s)"))
        case _:
            return Ok(targets)


def _routed(targets: TargetFiles, settings: AssaySettings) -> Result[tuple[Routed, ...], Fault]:
    match targets.targets:
        case (("all", _),):
            return Ok((Routed(Language.CSHARP, Scope.FULL, full_triggers=(str(settings.solution),)),))
        case (("project", project),):
            return Ok((Routed(Language.CSHARP, Scope.CHANGED, projects=(project,)),))
        case _:
            pass
    match targets.files:
        case ():
            return Ok(())
        case files:
            scoped = _scoped_settings(settings)
            return sequence(
                block.of_seq(route(language, files, settings=scoped).map(_static_route) for language in infer_languages(files, tuple(Language)))
            ).map(tuple)


def _static_route(routed: Routed) -> Routed:
    match (routed.language, routed.groups):
        case (Language.CSHARP, (first, *rest)):
            groups = tuple(
                (project, csharp_files)
                for project, files in (first, *rest)
                for csharp_files in (tuple(f for f in files if PurePosixPath(f).suffix == ".cs"),)
                if csharp_files
            )
            direct = tuple(project for project, _ in groups)
            files = tuple(sorted({file for _, group_files in groups for file in group_files}))
            return msgspec.structs.replace(
                routed, files=files, projects=direct, groups=groups, host_bound=tuple(p for p in routed.host_bound if p in direct)
            )
        case _:
            return routed


def _empty_route(routed: Routed) -> bool:
    return (routed.language.strategy == "glob" and not routed.files) or (
        routed.language.strategy == "closure" and not routed.files and not routed.projects and not _workspace_route(routed)
    )


def _leased_run(
    resource: str, project: str, run: Callable[[], tuple[Result[Completed, Fault], ...]], settings: AssaySettings
) -> tuple[Result[Completed, Fault], ...]:
    outcome = leased(resource, lambda _held: Ok(run()), settings=settings, run_id=settings.run_id, project=project, mode="exclusive")
    match outcome:
        case Result(tag="ok", ok=rows):
            return rows
        case Result(error=fault):
            return (Error(fault),)


def _phase_failed(rows: tuple[Result[Completed, Fault], ...]) -> bool:
    for row in rows:
        match row:
            case Result(tag="error"):
                return True
            case Result(tag="ok", ok=done) if done.status.severity >= RailStatus.FAILED.severity:
                return True
            case _:
                pass
    return False


def _skipped(checks: tuple[Check, ...], routed: Routed, settings: AssaySettings, scope: ArtifactScope) -> tuple[Result[Completed, Fault], ...]:
    return tuple(
        Ok(
            receipt(
                argv_for(check, routed, settings=settings, scope=scope).default_value((check.tool.name,)),
                0,
                status=RailStatus.SKIP,
                notes=("restore failed; build skipped",),
            )
        )
        for check in checks
    )


def _build_fan(phases: PhaseChecks, routed: Routed, settings: AssaySettings) -> tuple[Result[Completed, Fault], ...]:
    closure = _build_sha(routed)
    active_scope = ArtifactScope.build(settings, closure)
    resource = f"build-{closure}-{settings.configuration.value}"
    project = f"{settings.configuration.value}:{','.join(routed.projects or routed.files)}"

    def run() -> tuple[Result[Completed, Fault], ...]:
        rows: tuple[Result[Completed, Fault], ...] = ()
        blocked = False
        for phase, checks in phases:
            phase_rows = (
                _skipped(checks, routed, settings, active_scope)
                if phase == "build" and blocked
                else fan_out(checks, settings=settings, scope=active_scope, routed=routed)
            )
            rows = (*rows, *phase_rows)
            blocked = blocked or (phase == "restore" and _phase_failed(phase_rows))
        return rows

    return _leased_run(resource, project, run, settings)


def _write_fan(checks: tuple[Check, ...], routed: Routed, settings: AssaySettings, scope: ArtifactScope) -> tuple[Result[Completed, Fault], ...]:
    resource = f"write-{routed.language.value}-{_route_sha(routed)}"
    project = ",".join((*routed.projects, *routed.files))
    return _leased_run(resource, project, lambda: fan_out(checks, settings=settings, scope=scope, routed=routed), settings)


def _dispatch(
    routed: Routed, *, phases: PhaseChecks, settings: AssaySettings, scope: ArtifactScope, verb: str
) -> tuple[Result[Completed, Fault], ...]:
    if _empty_route(routed):
        return ()
    checks = _all_checks(phases)
    match checks:
        case ():
            return ()
        case _ if verb == "build" and _builds_closure(routed):
            return _build_fan(phases, routed, settings)
        case _ if verb == "fix":
            return _write_fan(checks, routed, settings, scope)
        case _:
            return tuple(row for _, phase_checks in phases for row in fan_out(phase_checks, settings=settings, scope=scope, routed=routed))


def _closure_notes(report: Report, routed: tuple[Routed, ...]) -> Report:
    notes = tuple(note for route_row in routed for note in route_row.closure_note())
    return msgspec.structs.replace(report, notes=(*report.notes, *notes)) if notes else report


def _rail(
    settings: AssaySettings, scope: ArtifactScope, params: StaticParams, *, verb: str, modes: tuple[Mode, ...], execute: bool
) -> Result[Report, Fault]:
    return _target_result(settings, params, verb).bind(
        lambda targets: _routed(targets, settings).bind(
            lambda routed: _fold(settings, scope, targets, routed, verb=verb, modes=modes, execute=execute)
        )
    )


def _fold(
    settings: AssaySettings,
    scope: ArtifactScope,
    targets: TargetFiles,
    routed: tuple[Routed, ...],
    *,
    verb: str,
    modes: tuple[Mode, ...],
    execute: bool,
) -> Result[Report, Fault]:
    phase_sets = tuple((route_row, *_phase_checks(route_row, modes, settings, scope)) for route_row in routed)
    planned = tuple(row for route_row, phases, _ in phase_sets for row in _planned(verb, route_row, phases, settings, scope))
    checks = tuple(check for _, phases, _ in phase_sets for check in _all_checks(phases))
    skipped = tuple(row for _, _, rows in phase_sets for row in rows)
    artifacts = _artifacts(settings, routed) if verb == "build" else ()
    matches = _matches(targets, routed, skipped)

    def executed(done: tuple[Completed, ...]) -> Report:
        notes = tuple(note for item in done for note in item.notes)
        report = fold(
            Claim.STATIC, verb, done, detail=_detail(targets, routed, planned, skipped, artifacts, settings, checks, notes), sarif_dir=scope.sarif_dir
        )
        return _closure_notes(msgspec.structs.replace(report, results=(*matches, *report.results), artifacts=(*artifacts, *report.artifacts)), routed)

    match execute:
        case False:
            base = fold(Claim.STATIC, verb, (), detail=_detail(targets, routed, planned, skipped, artifacts, settings, checks))
            status = RailStatus.OK if targets.targets or planned or matches else RailStatus.EMPTY
            return Ok(
                msgspec.structs.replace(
                    base, status=status, results=matches, artifacts=artifacts, notes=(f"planned={len(planned)} skipped={len(skipped)}",)
                )
            )
        case True:
            return sequence(
                block.of_seq(
                    row
                    for route_row, phases, _ in phase_sets
                    for row in _dispatch(route_row, phases=phases, settings=settings, scope=scope, verb=verb)
                )
            ).map(lambda done: executed(tuple(done)))


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Preview scoped static routing and exact build/fix work without spawning tools.

    Returns:
        Static report carrying route, planned check, skipped check, resource, and artifact detail.
    """
    return _target_result(settings, params, "check").bind(
        lambda targets: _routed(targets, settings).map(lambda routed: _check_report(settings, scope, targets, routed))
    )


def _check_report(settings: AssaySettings, scope: ArtifactScope, targets: TargetFiles, routed: tuple[Routed, ...]) -> Report:
    preview = _PREVIEW if _build_target(targets) else (("fix", _FIX_MODES),)
    phase_sets = tuple((verb, route_row, *_phase_checks(route_row, modes, settings, scope)) for route_row in routed for verb, modes in preview)
    planned = tuple(row for verb, route_row, phases, _ in phase_sets for row in _planned(verb, route_row, phases, settings, scope))
    checks = tuple(check for _, _, phases, _ in phase_sets for check in _all_checks(phases))
    skipped = tuple(row for _, _, _, rows in phase_sets for row in rows)
    artifacts = _artifacts(settings, routed) if _build_target(targets) else ()
    matches = _matches(targets, routed, skipped)
    base = fold(Claim.STATIC, "check", (), detail=_detail(targets, routed, planned, skipped, artifacts, settings, checks))
    status = RailStatus.OK if targets.targets or planned or matches else RailStatus.EMPTY
    return msgspec.structs.replace(
        base, status=status, results=matches, artifacts=artifacts, notes=(f"planned={len(planned)} skipped={len(skipped)}",)
    )


def build(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run scoped non-mutating diagnostics, restore, and build.

    Returns:
        Static report carrying process receipts, diagnostics, route detail, resources, and artifacts.
    """
    return _rail(settings, scope, params, verb="build", modes=_BUILD_MODES, execute=True)


def fix(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run scoped native static fixers.

    Returns:
        Static report carrying mutating receipts, route detail, resources, and artifacts.
    """
    return _rail(settings, scope, params, verb="fix", modes=_FIX_MODES, execute=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__: list[str] = ["StaticParams", "build", "check", "fix"]
