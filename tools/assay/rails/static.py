"""Run one polyglot static lane: fix, diagnose, restore, then build."""

from collections.abc import Callable  # noqa: TC003  # runtime: callable annotation is resolved through the rail layer
from dataclasses import dataclass
from enum import StrEnum
from hashlib import sha256
from pathlib import PurePosixPath
from typing import Annotated, Self, TYPE_CHECKING

from cyclopts import Parameter
from expression import Error, Ok, Result
from expression.collections import block
from expression.extra.result import sequence
import msgspec
import structlog

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # runtime: public rail signatures are inspected
from tools.assay.core.engine import argv_for, fan_out, leased, resource_projection, run_check
from tools.assay.core.model import (
    # _sarif_status is a model-owned reader the rail forwards; private cross-module reach is intentional.
    _sarif_status,  # noqa: PLC2701
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
from tools.assay.core.routing import expand, infer_languages, place, route, Routed, Scope, target_files, TargetFiles
from tools.assay.core.status import RailStatus, Step


if TYPE_CHECKING:
    from tools.assay.core.model import Completed


# --- [TYPES] ----------------------------------------------------------------------------


class Phase(StrEnum):
    """Static-lane phase in fixed execution order, each carrying the lane Mode that produces it."""

    mode: Mode

    FIX = "fix", Mode.WRITE
    DIAGNOSTIC = "diagnostic", Mode.CHECK
    RESTORE = "restore", Mode.RESTORE
    BUILD = "build", Mode.BUILD

    def __new__(cls, value: str, mode: Mode) -> Self:
        """Attach the producing lane Mode not represented by the StrEnum value."""
        m = str.__new__(cls, value)
        m._value_, m.mode = value, mode
        return m


type PhaseChecks = tuple[tuple[Phase, tuple[Check, ...]], ...]
type SkipRows = tuple[tuple[Phase, str, str], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

# Fixed per-language order: writes land before diagnostics; restore precedes closure builds.
_MODES: tuple[Mode, ...] = (Mode.WRITE, Mode.CHECK, Mode.RESTORE, Mode.BUILD)
# Reverse of the Phase->Mode payload; a non-lane mode is a loud KeyError, not a silent default.
_PHASE: dict[Mode, Phase] = {phase.mode: phase for phase in Phase}
# Analyzer-free, SARIF-free compile probe: dotnet-format mutates and binds against the analyzer view, so a non-compiling
# target must gate the format phase before any write lands. RunAnalyzers=false isolates the raw C# compile from analyzer
# diagnostics; the throwaway scope's --artifacts-path keeps the probe's output off the real build scope and its SARIF drop.
_PROBE_COMMAND: tuple[str, ...] = ("build", "-p:RunAnalyzers=false", "-tl:off", "-v:quiet")
# Probe status that proves the target compiles; FAULTED/TIMEOUT/BUSY and any Error are infra-ambiguous and take the safe
# no-mutation path, never read as "does not compile".
_COMPILES: frozenset[RailStatus] = frozenset((RailStatus.OK, RailStatus.EMPTY))

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class StaticParams:
    """Targets for the polyglot static lane."""

    all: Annotated[
        bool,
        Parameter(name="--all", negative="", show_default=False, help="Fan every detected language at full scope, including the C# solution build."),
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
    plan: Annotated[
        bool, Parameter(name="--plan", negative="", show_default=False, help="Plan-only: emit planned argv without running any tool.")
    ] = False


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


def _phase(mode: Mode) -> Phase:
    return _PHASE[mode]


def _dotnet_policy(tool: Tool, settings: AssaySettings) -> Tool:
    match (tool.runner, tool.mode, any(part.startswith("-maxCpuCount:") for part in tool.command)):
        case (Runner.DOTNET, Mode.BUILD, False):
            return msgspec.structs.replace(tool, command=(*tool.command, f"-maxCpuCount:{settings.dotnet_max_cpu}"))
        case _:
            return tool


def _sarif_key(check: Check, routed: Routed, settings: AssaySettings) -> str:
    tail = check.tail or next(iter(place(routed, check.tool, settings=settings)), ())
    seed = "\n".join((*check.tool.command, *tail, *check.paths))
    stem = next((PurePosixPath(part).stem for part in tail if part.endswith((".csproj", ".slnx"))), check.tool.name)
    return f"{stem}-{sha256(seed.encode()).hexdigest()[:12]}"


def _sarif_pin(check: Check, routed: Routed, settings: AssaySettings, scope: ArtifactScope) -> Check:
    match (check.tool.runner, check.tool.mode):
        case (Runner.DOTNET, Mode.BUILD):
            tool = msgspec.structs.replace(
                check.tool,
                command=(*check.tool.command, f"-p:CspSarifDir={scope.sarif_dir}/{_sarif_key(check, routed, settings)}"),
            )
            return msgspec.structs.replace(check, tool=tool)
        case _:
            return check


def _routed_tool(tool: Tool, routed: Routed) -> Tool:
    match (routed.language, routed.scope, tool.input, bool(routed.projects)):
        case (Language.TYPESCRIPT, Scope.FULL, Input.PROJECT, _):
            return msgspec.structs.replace(tool, input=Input.OWNED)
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
        case (Input.PROJECT, "glob") if routed.scope is not Scope.FULL:
            return "project-wide tool unsupported by scoped static"
        case _:
            return ""


def _phase_checks(routed: Routed, settings: AssaySettings, scope: ArtifactScope) -> tuple[PhaseChecks, SkipRows]:
    rows = tuple(
        (_phase(active), projected, _tool_skip(projected, routed))
        for active in _MODES
        for tool in select(Claim.STATIC, routed.language)
        if tool.mode is active
        for projected in (_routed_tool(tool, routed),)
    )
    selected = tuple((phase, Check(tool=_dotnet_policy(tool, settings), paths=routed.files)) for phase, tool, reason in rows if not reason)
    skipped = tuple((phase, tool.name, reason) for phase, tool, reason in rows if reason)
    expanded = tuple(
        (phase, _sarif_pin(clone, routed, settings, scope))
        for phase, check in selected
        for clone in expand((check,), routed, settings=settings)
    )
    phases = tuple(dict.fromkeys(_phase(mode) for mode in _MODES))
    return tuple((phase, tuple(check for row_phase, check in expanded if row_phase is phase)) for phase in phases), skipped


def _all_checks(phases: PhaseChecks) -> tuple[Check, ...]:
    return tuple(check for _, checks in phases for check in checks)


def _argv(check: Check, routed: Routed, settings: AssaySettings, scope: ArtifactScope) -> str:
    return argv_for(check, routed, settings=settings, scope=scope).map(" ".join).default_value(f"{check.tool.name}:<unplaced>")


def _planned(routed: Routed, phases: PhaseChecks, settings: AssaySettings, scope: ArtifactScope) -> tuple[tuple[str, str, str], ...]:
    active_scope = ArtifactScope.build(settings, _build_sha(routed)) if _uses_build_scope(routed, phases) else scope
    return tuple((phase, check.tool.name, _argv(check, routed, settings, active_scope)) for phase, checks in phases for check in checks)


def _uses_build_scope(routed: Routed, phases: PhaseChecks) -> bool:
    return _builds_closure(routed) and any(phase in {Phase.RESTORE, Phase.BUILD} for phase, _ in phases)


def _artifacts(settings: AssaySettings, routed: tuple[Routed, ...]) -> tuple[Artifact, ...]:
    return tuple(
        Artifact(id=f"build-{sha}", kind=ArtifactKind.SCOPE, path=ArtifactScope.build(settings, sha).path)
        for route_row in routed
        for sha in (_build_sha(route_row),)
        if _builds_closure(route_row)
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


def _matches(targets: TargetFiles, routed: tuple[Routed, ...], skipped: SkipRows) -> tuple[Match, ...]:
    route_matches = tuple(
        Match(
            id=route_row.language.value,
            kind=ArtifactKind.SCOPE,
            text=(
                # A --project target routes through the CHANGED machinery; the raw enum then reads as "changed files=0"
                # and misleads agents into "stale cache", so the label names what was actually targeted.
                f"scope={'project' if route_row.scope is Scope.CHANGED and route_row.projects else route_row.scope.value} "
                f"files={len(route_row.files)} projects={len(route_row.projects)} "
                f"triggers={len(route_row.full_triggers)} groups={len(route_row.groups)}"
            ),
        )
        for route_row in routed
    )
    rejected = tuple(Match(id=path or kind, kind=ArtifactKind.SCOPE, text=reason, severity="skipped") for kind, path, reason in targets.rejected)
    skipped_matches = tuple(Match(id=name, kind=ArtifactKind.SCOPE, text=reason, severity="skipped") for _, name, reason in skipped)
    return (*route_matches, *rejected, *skipped_matches)


def _detail(  # noqa: PLR0913  # each param is a distinct StaticRun projection input; no grouping reduces the count without a synthetic carrier
    targets: TargetFiles,
    routed: tuple[Routed, ...],
    planned: tuple[tuple[str, str, str], ...],
    skipped: SkipRows,
    artifacts: tuple[Artifact, ...],
    settings: AssaySettings,
    checks: tuple[Check, ...],
    *,
    sarif_status: tuple[tuple[str, str], ...],
    done: tuple[Completed, ...] = (),
) -> StaticRun:
    notes = tuple(note for item in done for note in item.notes)
    return StaticRun(
        targets=targets.targets,
        routes=_route_rows(routed),
        planned=planned,
        skipped=(*targets.rejected, *skipped),
        phases=tuple(dict.fromkeys(row[0] for row in planned)),
        resources=resource_projection(settings, checks, notes=notes, receipts=done),
        artifacts=tuple(artifact.path for artifact in artifacts),
        sarif_status=sarif_status,
    )


def _params_argv(params: StaticParams) -> tuple[str, ...]:
    return (
        "static",
        *(("--all",) if params.all else ()),
        *(("--project", params.project) if params.project else ()),
        *(("--folder", *params.folders) if params.folders else ()),
        *(("--file", *params.files) if params.files else ()),
        *(("--plan",) if params.plan else ()),
    )


def _target_result(settings: AssaySettings, params: StaticParams) -> Result[TargetFiles, Fault]:
    # Target value alone selects all, project, folder/file, or changed-default scope.
    # Unsupported files remain skipped target rows, not hard faults.
    argv = _params_argv(params)
    path_targets = bool(params.folders or params.files)
    match (int(params.all) + int(bool(params.project)) + int(path_targets), params.all, params.project):
        case (count, _, _) if count > 1:
            return Error(Fault(argv, RailStatus.UNSUPPORTED, f"{Step.PARSE}: static: choose only one of --all, --project, or folder/file targets"))
        case (_, True, _):
            return Ok(TargetFiles(targets=(("all", str(settings.solution)),)))
        case (_, _, project) if project:
            return _project_target(settings, project)
        case _:
            return target_files(params.folders, params.files, settings=settings, changed=not path_targets)


def _project_target(settings: AssaySettings, project: str) -> Result[TargetFiles, Fault]:
    rel = PurePosixPath(project).as_posix()
    argv = ("static", "--project", rel)
    match (PurePosixPath(rel).suffix, (settings.root / rel).is_file()):
        case (".csproj", True):
            return Ok(TargetFiles(targets=(("project", rel),)))
        case (".csproj", False):
            return Error(Fault(argv, RailStatus.UNSUPPORTED, f"{Step.PARSE}: static: project not found: {rel}"))
        case _:
            return Error(Fault(argv, RailStatus.UNSUPPORTED, f"{Step.PARSE}: static: --project requires a .csproj path: {rel}"))


def _routed(targets: TargetFiles, settings: AssaySettings) -> Result[tuple[Routed, ...], Fault]:
    match targets.targets:
        case (("all", _),):
            languages = tuple(dict.fromkeys(tool.language for tool in select(Claim.STATIC)))
            return sequence(
                block.of_seq(
                    Ok(Routed(Language.CSHARP, Scope.FULL, full_triggers=(str(settings.solution),)))
                    if language is Language.CSHARP
                    else route(language, (".",), settings=settings).map(lambda row: msgspec.structs.replace(row, scope=Scope.FULL))
                    for language in languages
                )
            ).map(tuple)
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


def _build_route(routed: Routed) -> Routed:
    return (
        msgspec.structs.replace(routed, scope=Scope.FULL)
        if routed.scope is Scope.CHANGED
        and routed.language.strategy == "glob"
        and any(tool.mode is Mode.BUILD and tool.input is Input.PROJECT for tool in select(Claim.STATIC, routed.language))
        else routed
    )


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
            _LOG.info("phase.start", phase=phase, checks=len(checks), run_id=settings.run_id, route=routed.language.value)
            phase_rows = (
                _skipped(checks, routed, settings, active_scope)
                if phase is Phase.BUILD and blocked
                else fan_out(checks, settings=settings, scope=active_scope, routed=routed)
            )
            rows = (*rows, *phase_rows)
            failed = _phase_failed(phase_rows)
            _LOG.info("phase.end", phase=phase, checks=len(checks), failed=failed, run_id=settings.run_id, route=routed.language.value)
            blocked = blocked or (phase is Phase.RESTORE and failed)
        return rows

    return _leased_run(resource, project, run, settings)


def _write_fan(checks: tuple[Check, ...], routed: Routed, settings: AssaySettings, scope: ArtifactScope) -> tuple[Result[Completed, Fault], ...]:
    resource = f"write-{routed.language.value}-{_route_sha(routed)}"
    project = ",".join((*routed.projects, *routed.files))

    def run() -> tuple[Result[Completed, Fault], ...]:
        _LOG.info("phase.start", phase="fix", checks=len(checks), run_id=settings.run_id, route=routed.language.value)
        rows = fan_out(checks, settings=settings, scope=scope, routed=routed)
        _LOG.info("phase.end", phase="fix", checks=len(checks), failed=_phase_failed(rows), run_id=settings.run_id, route=routed.language.value)
        return rows

    return _leased_run(resource, project, run, settings)


def _probe_check(build_check: Check) -> Check:
    # Reuse the build check's resolved input/tail/placement so the probe targets exactly what the closure build will,
    # but swap in the analyzer-free, CspSarifDir-free command. CspSarifDir rides _dotnet_policy on the real build tool
    # only; the fresh probe command never carries it, so the probe drops no SARIF the report could fold.
    probe = msgspec.structs.replace(build_check.tool, name="dotnet-probe", command=_PROBE_COMMAND)
    return msgspec.structs.replace(build_check, tool=probe)


def _probe_compiles(closure_phases: PhaseChecks, routed: Routed, settings: AssaySettings) -> bool:
    # A throwaway per-closure scope isolates the probe's --artifacts-path from the real build scope; its receipt never
    # reaches report.results. compiles is True only when every build target probes OK/EMPTY; a FAILED probe means
    # "does not compile" and an Error/ambiguous probe takes the same safe no-mutation path.
    builds = tuple(_probe_check(check) for phase, checks in closure_phases if phase is Phase.BUILD for check in checks)
    if not builds:
        return True
    throwaway = ArtifactScope.build(settings, f"probe-{_build_sha(routed)}")
    _LOG.info("phase.start", phase="probe", checks=len(builds), run_id=settings.run_id, route=routed.language.value)
    outcomes = tuple(run_check(check, settings=settings, scope=throwaway, routed=routed) for check in builds)
    compiles = all(outcome.map(lambda done: done.status in _COMPILES).default_value(False) for outcome in outcomes)  # noqa: FBT003  # expression sentinel default: an Error/ambiguous probe collapses to the safe no-compile path, not a behavior flag
    _LOG.info("phase.end", phase="probe", checks=len(builds), compiles=compiles, run_id=settings.run_id, route=routed.language.value)
    return compiles


def _format_gated(checks: tuple[Check, ...], phases: PhaseChecks) -> tuple[Check, ...]:
    # A non-compiling target drops both the write fix and its read-only check twin: the format tool binds against the
    # analyzer view and may fault or emit spurious drift on a target the compiler itself rejects. The write-capable tool
    # names drive the gate, so the format CHECK row falls with its WRITE sibling without naming the tool.
    writable = frozenset(check.tool.name for _, lane in phases for check in lane if check.tool.mode.writes)
    return tuple(check for check in checks if check.tool.name not in writable)


def _dispatch(routed: Routed, *, phases: PhaseChecks, settings: AssaySettings, scope: ArtifactScope) -> tuple[Result[Completed, Fault], ...]:
    if _empty_route(routed):
        return ()
    # Writes run under the mutating lease; C# restore/build runs under the closure lease.
    # Non-closure build checks stay in the plain fan-out with diagnostics.
    write = tuple(check for phase, checks in phases if phase is Phase.FIX for check in checks)
    closure_phases = tuple((phase, checks) for phase, checks in phases if phase in {Phase.RESTORE, Phase.BUILD})
    uses_closure = _uses_build_scope(routed, closure_phases)
    # The probe gates the format phase before the write lease; it is orthogonal to _build_fan's RESTORE->BUILD block gate.
    compiles = not (uses_closure and write) or _probe_compiles(closure_phases, routed, settings)
    gated_write = write if compiles else _format_gated(write, phases)
    plain = tuple(
        check
        for phase, checks in phases
        if phase is not Phase.FIX and not (uses_closure and phase in {Phase.RESTORE, Phase.BUILD})
        for check in (checks if compiles else _format_gated(checks, phases))
    )
    return (
        *(_write_fan(gated_write, routed, settings, scope) if gated_write else ()),
        *(fan_out(plain, settings=settings, scope=scope, routed=routed) if plain else ()),
        *(_build_fan(closure_phases, routed, settings) if uses_closure else ()),
    )


def _backpressure_note(resources: tuple[tuple[str, float], ...]) -> tuple[str, ...]:
    # Structured threading: every field rides the typed resource rows resource_projection already folded from
    # _ConcurrencyPressure and notes (slot max-wait is dotnet.slot_wait_ms.max). The note touches no receipt text.
    row = dict(resources)
    original, reduced = int(row.get("concurrency.original", 0.0)), int(row.get("concurrency.reduced", 0.0))
    foreign, mem = int(row.get("dotnet.foreign", 0.0)), row.get("memory.percent", 0.0)
    slot_wait_max = row.get("dotnet.slot_wait_ms.max", 0.0)
    note = (
        f"concurrency.backpressure: reduced {original}->{reduced} (mem={mem:.0f}% foreign_dotnet={foreign}); "
        f"dotnet.slot max_wait={slot_wait_max:.0f}ms"
    )
    return (note,) if reduced < original or slot_wait_max > 0.0 else ()


def _closure_notes(report: Report, routed: tuple[Routed, ...]) -> Report:
    notes = tuple(note for route_row in routed for note in route_row.closure_note())
    return msgspec.structs.replace(report, notes=(*report.notes, *notes)) if notes else report


def _fold(
    settings: AssaySettings, scope: ArtifactScope, targets: TargetFiles, routed: tuple[Routed, ...], *, plan: bool = False
) -> Result[Report, Fault]:
    routed = tuple(_build_route(route_row) for route_row in routed)
    phase_sets = tuple((route_row, *_phase_checks(route_row, settings, scope)) for route_row in routed)
    planned = tuple(row for route_row, phases, _ in phase_sets for row in _planned(route_row, phases, settings, scope))
    checks = tuple(check for _, phases, _ in phase_sets for check in _all_checks(phases))
    skipped = tuple(row for _, _, rows in phase_sets for row in rows)
    artifacts = _artifacts(settings, routed)
    matches = _matches(targets, routed, skipped)

    def executed(done: tuple[Completed, ...]) -> Report:
        detail = _detail(targets, routed, planned, skipped, artifacts, settings, checks, sarif_status=_sarif_status(done, scope.sarif_dir), done=done)
        report = fold(Claim.STATIC, "static", done, detail=detail, sarif_dir=scope.sarif_dir, promote_empty=True)
        return _closure_notes(
            msgspec.structs.replace(
                report,
                results=(*report.results, *matches),
                artifacts=(*artifacts, *report.artifacts),
                notes=(*report.notes, f"planned={len(planned)} skipped={len((*targets.rejected, *skipped))}", *_backpressure_note(detail.resources)),
            ),
            routed,
        )

    if plan:
        # Dry-run: the populated `planned` argv rides the detail; no tool spawns and no file mutates.
        return Ok(executed(()))
    rows = (row for route_row, phases, _ in phase_sets for row in _dispatch(route_row, phases=phases, settings=settings, scope=scope))
    return sequence(block.of_seq(rows)).map(lambda done: executed(tuple(done)))


# --- [COMPOSITION] ----------------------------------------------------------------------


def run(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run the static quality lane for the selected target scope.

    The target value alone selects whole-workspace, project, folder/file, or changed-default routing. Every
    touched language runs write-capable tools before diagnostics, and C# closure builds observe restored outputs.

    Returns:
        Folded static report, or a routing/restore/strict-promotion fault.
    """
    return _target_result(settings, params).bind(
        lambda targets: _routed(targets, settings).bind(lambda routed: _fold(settings, scope, targets, routed, plan=params.plan))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

_LOG = structlog.get_logger("assay.static")

__all__: list[str] = ["StaticParams", "run"]
