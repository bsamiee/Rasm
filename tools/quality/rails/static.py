"""Changed/path-scoped C# fix, report, build, and plan routing."""

# --- [IMPORTS] ------------------------------------------------------------------------

from collections.abc import Callable
from dataclasses import dataclass
from functools import reduce
from itertools import groupby
from operator import itemgetter
from pathlib import Path
from typing import assert_never, Final, Literal

from beartype import beartype
from expression import Error, Ok, Result
import msgspec

from tools.quality.process import dotnet, dotnet_args, dotnet_build, fold, ProcessFault, ProjectIndex, run, Workspace
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

type FormatKind = Literal["whitespace", "style", "analyzers"]
type FormatMode = Literal["fix", "report"]
type ProjectMode = Literal["parity", "closure"]
type StaticMode = Literal["fix", "report", "build", "full", "plan"]
type StaticOutcome = Literal["skip", "done"]
type StaticScope = Literal["changed", "full"]


# --- [CONSTANTS] -----------------------------------------------------------------------

_FORMAT_ARGS: Final[dict[FormatKind, tuple[str, ...]]] = {
    "whitespace": ("--no-restore",),
    "style": ("--severity", "error", "--no-restore"),
    "analyzers": ("--severity", "error", "--no-restore"),
}
_FORMAT_KINDS: Final[tuple[FormatKind, ...]] = ("whitespace", "style", "analyzers")
_ORPHAN_FULL_SUFFIXES: Final[tuple[str, ...]] = (".cs", ".props", ".targets")


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
                dotnet_args(
                    "build", target, configuration=configuration, version=version_args, max_cpu=settings.dotnet_max_cpu, fresh=True, quiet=True
                )
                for configuration in configurations
                for target in targets
            ),
        )

    return _build_targets(settings, plan, root, mode).map(commands).default_value(())


def _build_plan(
    plan: StaticPlan, settings: QualitySettings, scope: ArtifactScope, root: Path, mode: StaticScope
) -> Result[StaticOutcome, ProcessFault]:
    return _build_targets(settings, plan, root, mode).bind(
        lambda rows: dotnet_build(
            settings,
            scope,
            restore_targets=rows[1],
            targets=rows[0],
            configurations=settings.static_configurations(mode),
            max_cpu=settings.dotnet_max_cpu,
            fresh=True,
            quiet=True,
        ).map(lambda _: "done")
    )


def _build_targets(
    settings: QualitySettings, plan: StaticPlan, root: Path, mode: StaticScope
) -> Result[tuple[tuple[str | Path, ...], tuple[str | Path, ...]], ProcessFault]:
    match plan.scope:
        case "changed":
            match plan.projects:
                case ():
                    return Error(ProcessFault.fail("static", mode, detail=b"No C# projects selected", returncode=0))
                case projects:
                    targets: tuple[str | Path, ...] = tuple(str(root / project) for project in projects)
                    return Ok((targets, targets))
        case "full":
            match plan.projects:
                case ():
                    return Error(ProcessFault.fail("static", mode, detail=b"No C# projects selected"))
                case _:
                    return Ok(((settings.solution,), (settings.solution,)))
        case unreachable:
            assert_never(unreachable)


def _format_commands(settings: QualitySettings, scope: ArtifactScope, plan: StaticPlan, mode: FormatMode) -> tuple[tuple[str, ...], ...]:
    match plan.format_groups:
        case ():
            return ()
        case groups:
            return tuple(_format_command(settings, scope, project, files, kind, mode) for project, files in groups for kind in _FORMAT_KINDS)


def _format_command(
    settings: QualitySettings, scope: ArtifactScope, project: str, files: tuple[str, ...], kind: FormatKind, mode: FormatMode
) -> tuple[str, ...]:
    report = ("--verify-no-changes", "--report", str(scope.path / "static-report" / kind / Path(project).stem)) if mode == "report" else ()
    return ("format", kind, str(settings.root / project), "--include", *files, *_FORMAT_ARGS[kind], *report)


def _format_groups(routes: tuple[tuple[str, str], ...]) -> tuple[tuple[str, tuple[str, ...]], ...]:
    return tuple((project, tuple(file for _, file in files)) for project, files in groupby(sorted(routes, key=itemgetter(0)), key=itemgetter(0)))


def _format_plan(settings: QualitySettings, scope: ArtifactScope, plan: StaticPlan, mode: FormatMode) -> Result[StaticOutcome, ProcessFault]:
    commands = _format_commands(settings, scope, plan, mode)
    match commands:
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


def _inputs(settings: QualitySettings, workspace: Workspace, paths: tuple[Path, ...]) -> tuple[str, ...]:
    match paths:
        case ():
            return workspace.changed()
        case _:
            return tuple(sorted({file for path in paths for file in _input_path(settings, workspace, path)}))


def _input_path(settings: QualitySettings, workspace: Workspace, path: Path) -> tuple[str, ...]:
    target = (path.expanduser() if path.is_absolute() else settings.root / path).resolve()
    match target:
        case Path() if target.is_dir():
            excludes = tuple(item for name in PROJECT_EXCLUDE_DIRS for item in ("--exclude", name))
            return tuple(sorted(_relative(settings.root, item) for item in workspace.paths(("fd", "-H", "-t", "f", ".", str(target), *excludes))))
        case _:
            return (_relative(settings.root, target),)


def _plan(
    settings: QualitySettings, scope: ArtifactScope, mode: StaticScope = "changed", paths: tuple[Path, ...] = ()
) -> Result[StaticPlan, ProcessFault]:
    workspace = Workspace(settings.root)
    index = workspace.index()
    inputs = _inputs(settings, workspace, paths)
    routed = reduce(lambda acc, file: _route_step(acc, file, index=index, workspace=workspace, root=settings.root), inputs, _ChangedRoute())

    def full() -> Result[StaticPlan, ProcessFault]:
        return _projects(settings, workspace, "parity", scope=scope).map(
            lambda rows: StaticPlan(
                scope="full",
                projects=rows,
                format_groups=_format_groups(routed.format_routes),
                inputs=inputs,
                owners=tuple(sorted(routed.projects)),
                full_triggers=tuple(sorted(routed.full_triggers)),
            )
        )

    def changed() -> Result[StaticPlan, ProcessFault]:
        return _projects(settings, workspace, "closure", tuple(sorted(routed.projects)), scope=scope).map(
            lambda rows: StaticPlan(
                scope="changed",
                projects=rows,
                format_groups=_format_groups(routed.format_routes),
                inputs=inputs,
                owners=tuple(sorted(routed.projects)),
                full_triggers=tuple(sorted(routed.full_triggers)),
            )
        )

    match mode:
        case "full":
            return full()
        case "changed":
            pass
        case unreachable:
            assert_never(unreachable)

    plan_by_route: dict[tuple[bool, bool], Callable[[], Result[StaticPlan, ProcessFault]]] = {
        (False, False): lambda: Ok(StaticPlan(scope="changed", projects=(), inputs=inputs)),
        (False, True): changed,
        (True, False): full,
        (True, True): full,
    }
    return plan_by_route[routed.full, bool(routed.projects)]()


def _projects(
    settings: QualitySettings, workspace: Workspace, mode: ProjectMode, seeds: tuple[str, ...] = (), *, scope: ArtifactScope
) -> Result[tuple[str, ...], ProcessFault]:
    root = settings.root
    rows = workspace.projects()
    match mode:
        case "parity":
            listed = frozenset(
                run(("dotnet", "sln", str(settings.solution), "list"), env=scope.dotnet_env, check=False)
                .map(lambda done: (line.strip() for line in done.lines() if line.strip().endswith(".csproj")))
                .default_with(lambda _: ())
            )
            delta = (*sorted(frozenset(rows) - listed), *sorted(listed - frozenset(rows)))
            return Ok(rows).filter_with(
                lambda _: delta == (),
                lambda _: ProcessFault.fail(
                    "static", "solution-parity", detail=f"Workspace.slnx project parity failed:\n{'\n'.join(f'- {item}' for item in delta)}"
                ),
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
                    | frozenset(candidate for candidate, references in graph.items() if candidate not in current and current.intersection(references))
                ),
                range(len(graph)),
                frozenset(seeds),
            )
            return Ok(tuple(sorted(expanded)))
        case unreachable:
            assert_never(unreachable)


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
    return _plan(settings, scope, paths=paths).map(
        lambda plan: msgspec.json.encode(
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
                    "build": tuple(
                        ("dotnet", *command, *scope.dotnet_flags) for command in _build_commands(settings, plan, settings.root, plan.scope)
                    ),
                },
            )
        )
    )


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
            return _plan(settings, scope, paths=paths).bind(
                lambda plan: _build_plan(plan, settings, scope, settings.root, "changed").or_else_with(_skip)
            )
        case "full":
            return _plan(settings, scope, "full", paths=paths).bind(lambda plan: _build_plan(plan, settings, scope, settings.root, "full"))
        case "plan":
            return plan_payload(settings, scope, paths=paths).map(lambda _: "done")
        case unreachable:
            assert_never(unreachable)


def _skip(fault: ProcessFault) -> Result[StaticOutcome, ProcessFault]:
    return Ok("skip") if fault.returncode == 0 else Error(fault)
