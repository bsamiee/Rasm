"""Changed/path-scoped C# fix, report, build, and plan routing."""

# --- [IMPORTS] ------------------------------------------------------------------------

from dataclasses import dataclass
from functools import reduce
import hashlib
from itertools import groupby, starmap
from operator import itemgetter
from pathlib import Path
from typing import assert_never, Final, Literal

from beartype import beartype
from expression import Error, Ok, Result
import msgspec

from tools.quality.process import dotnet, dotnet_args, dotnet_build, fold, leased, ProcessFault, ProjectIndex, run, Workspace
from tools.quality.settings import (
    ArtifactScope,
    CS_SUFFIXES,
    FULL_TRIGGER_FILES,
    IGNORE_FIXTURE_PREFIXES,
    PROJECT_EXCLUDE_DIRS,
    QualitySettings,
    STATIC_FULL_TRIGGER_PREFIXES,
)


# --- [TYPES] ---------------------------------------------------------------------------

type FormatMode = Literal["fix", "report"]
type ProjectMode = Literal["parity", "closure"]
type StaticMode = Literal["fix", "report", "build", "full", "plan"]
type StaticOutcome = Literal["skip", "full-trigger-skip", "done"]
type StaticScope = Literal["changed", "full"]


# --- [CONSTANTS] -----------------------------------------------------------------------

_ORPHAN_FULL_SUFFIXES: Final[tuple[str, ...]] = (".cs", ".csproj", ".props", ".targets")


# --- [MODELS] ----------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class StaticPlan:
    scope: StaticScope
    projects: tuple[str, ...]
    format_groups: tuple[tuple[str, tuple[str, ...]], ...] = ()
    inputs: tuple[str, ...] = ()
    owners: tuple[str, ...] = ()
    full_triggers: tuple[str, ...] = ()


@dataclass(frozen=True, slots=True)
class _ChangedRoute:
    projects: frozenset[str] = frozenset()
    full: bool = False
    format_routes: tuple[tuple[str, str], ...] = ()
    full_triggers: tuple[str, ...] = ()


class StaticPlanReport(msgspec.Struct, frozen=True, gc=False):
    scope: StaticScope
    inputs: tuple[str, ...]
    owners: tuple[str, ...]
    projects: tuple[str, ...]
    full_triggers: tuple[str, ...]
    format_groups: tuple[tuple[str, tuple[str, ...]], ...]
    commands: dict[str, tuple[tuple[str, ...], ...]]


# --- [OPERATIONS] ------------------------------------------------------------------------


def _build_commands(settings: QualitySettings, plan: StaticPlan, root: Path, mode: StaticScope) -> tuple[tuple[str, ...], ...]:
    def commands(rows: tuple[tuple[str | Path, ...], tuple[str | Path, ...]]) -> tuple[tuple[str, ...], ...]:
        targets, restores = rows
        configurations = settings.static_configurations(mode)
        version_args = settings.version_props()
        return (
            *(dotnet_args("restore", restore) for restore in restores),
            *(
                dotnet_args("build", target, configuration=configuration, version=version_args, max_cpu=settings.dotnet_max_cpu, quiet=True)
                for configuration in configurations
                for target in targets
            ),
        )

    return _build_targets(settings, plan, root, mode).map(commands).default_value(())


def _closure_key(plan: StaticPlan, mode: StaticScope) -> str:
    return "solution" if mode == "full" else hashlib.sha256(",".join(sorted(plan.projects)).encode()).hexdigest()[:16]


def _build_plan(plan: StaticPlan, settings: QualitySettings, root: Path, mode: StaticScope) -> Result[StaticOutcome, ProcessFault]:
    # Stable per-closure --artifacts-path keeps builds warm/incremental across runs; the shared lease serializes the same
    # closure (busy -> exit 5) while distinct closures build concurrently with isolated artifact + obj output.
    closure = _closure_key(plan, mode)
    build_scope = ArtifactScope.build(settings, closure)
    return _build_targets(settings, plan, root, mode).bind(
        lambda rows: leased(
            settings,
            settings.build_lock(closure),
            f"build-{closure}",
            lambda: dotnet_build(
                settings,
                build_scope,
                restore_targets=rows[1],
                targets=rows[0],
                configurations=settings.static_configurations(mode),
                max_cpu=settings.dotnet_max_cpu,
                quiet=True,
            ).map(lambda _: "done"),
        )
    )


def _build_targets(
    settings: QualitySettings, plan: StaticPlan, root: Path, mode: StaticScope
) -> Result[tuple[tuple[str | Path, ...], tuple[str | Path, ...]], ProcessFault]:
    match (plan.scope, plan.projects):
        # A `changed` scope with no projects is a benign skip (returncode 0); a `full` scope with none is a parity failure.
        case (scope, ()):
            return Error(ProcessFault.fail("static", mode, detail=b"No C# projects selected", returncode=0 if scope == "changed" else 2))
        case ("changed", projects):
            targets: tuple[str | Path, ...] = tuple(str(root / project) for project in projects)
            return Ok((targets, targets))
        case _:
            return Ok(((settings.solution,), (settings.solution,)))


def _format_commands(settings: QualitySettings, scope: ArtifactScope, plan: StaticPlan, mode: FormatMode) -> tuple[tuple[str, ...], ...]:
    # Bare `dotnet format` runs whitespace+style+analyzers in one pass with an implicit restore so semantic IDE rules
    # (IDE0001 name simplification, IDE0005 usings) resolve against a real compilation; `--no-restore` silently skips them.
    def command(project: str, files: tuple[str, ...]) -> tuple[str, ...]:
        report = ("--verify-no-changes", "--report", str(scope.path / "static-report" / Path(project).stem)) if mode == "report" else ()
        return ("format", str(settings.root / project), "--include", *files, "--severity", "error", *report)

    return tuple(starmap(command, plan.format_groups))


def _format_groups(routes: tuple[tuple[str, str], ...]) -> tuple[tuple[str, tuple[str, ...]], ...]:
    return tuple((project, tuple(file for _, file in files)) for project, files in groupby(sorted(routes, key=itemgetter(0)), key=itemgetter(0)))


def _format_plan(settings: QualitySettings, scope: ArtifactScope, plan: StaticPlan, mode: FormatMode) -> Result[StaticOutcome, ProcessFault]:
    commands = _format_commands(settings, scope, plan, mode)
    match commands:
        case () if plan.scope == "full" and plan.full_triggers:
            return Ok("full-trigger-skip")
        case ():
            return Ok("skip")
        case _:
            for command in commands:
                match _report_dir(command):
                    case Path() as report:
                        report.mkdir(parents=True, exist_ok=True)
                    case None:
                        pass
            return fold(
                commands, None, lambda _, command: dotnet(*command, scope=scope, scoped=False, check=True, mode="stream").map(lambda _: None)
            ).map(lambda _: "done")


def _inputs(settings: QualitySettings, workspace: Workspace, paths: tuple[Path, ...]) -> Result[tuple[str, ...], ProcessFault]:
    match paths:
        case ():
            return workspace.changed()
        case _:
            seed: tuple[str, ...] = ()
            return fold(paths, seed, lambda acc, path: _input_path(settings, workspace, path).map(lambda rows: (*acc, *rows))).map(
                lambda rows: tuple(sorted(frozenset(rows)))
            )


def _input_path(settings: QualitySettings, workspace: Workspace, path: Path) -> Result[tuple[str, ...], ProcessFault]:
    target = (path.expanduser() if path.is_absolute() else settings.root / path).resolve()
    match target:
        case Path() if target.is_dir():
            excludes = tuple(item for name in PROJECT_EXCLUDE_DIRS for item in ("--exclude", name))
            return workspace.paths(("fd", "-H", "-t", "f", ".", str(target), *excludes)).map(
                lambda rows: tuple(sorted(_relative(settings.root, item) for item in rows))
            )
        case Path() if not target.exists():
            return Error(ProcessFault.fail("static", "path", str(path), detail=f"Explicit path does not exist: {path}"))
        case _:
            return Ok((_relative(settings.root, target),))


def _plan(
    settings: QualitySettings, scope: ArtifactScope, mode: StaticScope = "changed", paths: tuple[Path, ...] = ()
) -> Result[StaticPlan, ProcessFault]:
    workspace = Workspace(settings.root)

    def route(inputs: tuple[str, ...], index: ProjectIndex) -> _ChangedRoute:
        return reduce(lambda acc, file: _route_step(acc, file, index=index, workspace=workspace, root=settings.root), inputs, _ChangedRoute())

    # `full` lists the whole solution for parity; `changed` grows the touched-project closure. Both project the same
    # StaticPlan shape, so one parameterized builder owns both — the only axis is which _projects mode resolves `projects`.
    def plan_for(plan_scope: StaticScope, inputs: tuple[str, ...], routed: _ChangedRoute) -> Result[StaticPlan, ProcessFault]:
        resolved = (
            _projects(settings, workspace, "parity", scope=scope)
            if plan_scope == "full"
            else _projects(settings, workspace, "closure", tuple(sorted(routed.projects)), scope=scope)
        )
        return resolved.map(
            lambda rows: StaticPlan(
                scope=plan_scope,
                projects=rows,
                format_groups=_format_groups(routed.format_routes),
                inputs=inputs,
                owners=tuple(sorted(routed.projects)),
                full_triggers=tuple(sorted(routed.full_triggers)),
            )
        )

    match mode:
        case "full":
            return workspace.index().bind(
                lambda i: _inputs(settings, workspace, paths).bind(lambda inputs: plan_for("full", inputs, route(inputs, i)))
            )
        case "changed":
            pass
        case unreachable:
            assert_never(unreachable)

    def routed_plan(inputs: tuple[str, ...], routed: _ChangedRoute) -> Result[StaticPlan, ProcessFault]:
        # No full-trigger and no owned project => empty changed plan without resolving the closure (fast path).
        return (
            plan_for("full" if routed.full else "changed", inputs, routed)
            if routed.full or routed.projects
            else Ok(StaticPlan(scope="changed", projects=(), inputs=inputs))
        )

    return _inputs(settings, workspace, paths).bind(
        lambda inputs: (
            Ok(StaticPlan(scope="changed", projects=(), inputs=inputs))
            if not inputs
            else workspace.index().bind(lambda index: routed_plan(inputs, route(inputs, index)))
        )
    )


def _projects(
    settings: QualitySettings, workspace: Workspace, mode: ProjectMode, seeds: tuple[str, ...] = (), *, scope: ArtifactScope
) -> Result[tuple[str, ...], ProcessFault]:
    root = settings.root

    def resolve(rows: tuple[str, ...]) -> Result[tuple[str, ...], ProcessFault]:
        match mode:
            case "parity":
                return run(("dotnet", "sln", str(settings.solution), "list"), env=scope.dotnet_env, check=True).bind(
                    lambda done: (
                        listed := frozenset(line.strip() for line in done.lines() if line.strip().endswith(".csproj")),
                        delta := (*sorted(frozenset(rows) - listed), *sorted(listed - frozenset(rows))),
                        Ok(rows).filter_with(
                            lambda _: delta == (),
                            lambda _: ProcessFault.fail(
                                "static",
                                "solution-parity",
                                detail=f"Workspace.slnx project parity failed:\n{'\n'.join(f'- {item}' for item in delta)}",
                            ),
                        ),
                    )[2]
                )
            case "closure":

                def ref(project: Path, include: str) -> str:
                    match (include.startswith("/"), "/" in include):
                        case (True, _):
                            return str(Path(include).resolve().relative_to(root))
                        case (_, True):
                            ref_dir = (project.parent / Path(include).parent).resolve()
                            return str((ref_dir / Path(include).name).resolve().relative_to(root))
                        case _:
                            return str((project.parent / include).resolve().relative_to(root))

                def refs(project: Path) -> frozenset[str]:
                    match workspace.csproj(project):
                        case None | str():
                            return frozenset()
                        case element:
                            return frozenset(
                                ref(root / project, include)
                                for node in element.iter()
                                if node.tag.rpartition("}")[-1] == "ProjectReference"
                                for include in (node.attrib.get("Include", ""),)
                                if include.endswith(".csproj")
                                and node.attrib.get("OutputItemType") != "Analyzer"
                                and node.attrib.get("ReferenceOutputAssembly") != "false"
                            )

                graph = {project: refs(root / project) for project in rows}
                expanded = reduce(
                    lambda current, _: (
                        current
                        | frozenset(
                            candidate for candidate, references in graph.items() if candidate not in current and current.intersection(references)
                        )
                    ),
                    range(len(graph)),
                    frozenset(seeds),
                )
                return Ok(tuple(sorted(expanded)))
            case unreachable:
                assert_never(unreachable)

    return workspace.projects().bind(resolve)


def _relative(root: Path, path: Path) -> str:
    resolved = path.resolve()
    return str(resolved.relative_to(root)) if resolved.is_relative_to(root) else str(path)


def _report_dir(command: tuple[str, ...]) -> Path | None:
    return Path(command[command.index("--report") + 1]) if "--report" in command else None


def _route_step(acc: _ChangedRoute, file: str, *, index: ProjectIndex, workspace: Workspace, root: Path) -> _ChangedRoute:
    ignored = not file.endswith(".csproj") and any(file.startswith(prefix) for prefix in IGNORE_FIXTURE_PREFIXES)
    full_trigger = file in FULL_TRIGGER_FILES or file.startswith(STATIC_FULL_TRIGGER_PREFIXES)
    relevant = full_trigger or file.endswith(".csproj") or Path(file).suffix in CS_SUFFIXES
    match (ignored, relevant, full_trigger):
        case (True, _, _) | (_, False, _):
            return acc
        case (_, _, True):
            return _ChangedRoute(acc.projects, full=True, format_routes=acc.format_routes, full_triggers=(*acc.full_triggers, file))
        case _:
            owner = workspace.owner_rel(index, root / file)
            match owner:
                case str(owner):
                    return _ChangedRoute(
                        acc.projects | {owner},
                        full=acc.full,
                        format_routes=(*acc.format_routes, (owner, file)) if file.endswith(".cs") else acc.format_routes,
                        full_triggers=acc.full_triggers,
                    )
                case None if file.endswith(_ORPHAN_FULL_SUFFIXES):
                    return _ChangedRoute(acc.projects, full=True, format_routes=acc.format_routes, full_triggers=(*acc.full_triggers, file))
                case _:
                    return acc


def plan_payload(settings: QualitySettings, scope: ArtifactScope, paths: tuple[Path, ...] = ()) -> Result[bytes, ProcessFault]:
    def report(plan: StaticPlan) -> bytes:
        build_flags = ArtifactScope.build(settings, _closure_key(plan, plan.scope)).dotnet_flags
        return msgspec.json.encode(
            StaticPlanReport(
                scope=plan.scope,
                inputs=plan.inputs,
                owners=plan.owners,
                projects=plan.projects,
                full_triggers=plan.full_triggers,
                format_groups=plan.format_groups,
                commands={
                    "fix": tuple(("dotnet", *command) for command in _format_commands(settings, scope, plan, "fix")),
                    "report": tuple(("dotnet", *command) for command in _format_commands(settings, scope, plan, "report")),
                    "build": tuple(("dotnet", *command, *build_flags) for command in _build_commands(settings, plan, settings.root, plan.scope)),
                },
            )
        )

    return _plan(settings, scope, paths=paths).map(report)


@beartype
def run_static_rail(
    settings: QualitySettings, scope: ArtifactScope, mode: StaticMode, paths: tuple[Path, ...] = ()
) -> Result[StaticOutcome, ProcessFault]:
    match mode:
        case "fix":
            return _plan(settings, scope, paths=paths).bind(lambda plan: _format_plan(settings, scope, plan, "fix"))
        case "report":
            return _plan(settings, scope, paths=paths).bind(lambda plan: _format_plan(settings, scope, plan, "report"))
        case "build":
            return _plan(settings, scope, paths=paths).bind(lambda plan: _build_plan(plan, settings, settings.root, plan.scope).or_else_with(_skip))
        case "full":
            return _plan(settings, scope, "full", paths=paths).bind(lambda plan: _build_plan(plan, settings, settings.root, "full"))
        case "plan":
            return plan_payload(settings, scope, paths=paths).map(lambda _: "done")
        case unreachable:
            assert_never(unreachable)


def _skip(fault: ProcessFault) -> Result[StaticOutcome, ProcessFault]:
    return Ok("skip") if fault.returncode == 0 else Error(fault)
