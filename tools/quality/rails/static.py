"""Changed-file and full-workspace static C# gate: restore, build, format, and analyzer routing."""

# --- [IMPORTS] ------------------------------------------------------------------------

from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass
from functools import reduce
from itertools import groupby
from operator import itemgetter
from pathlib import Path
from typing import assert_never, Final, Literal

from beartype import beartype
from expression import Error, Ok, Result

from tools.quality.process import dotnet, dotnet_build, fold, ProcessFault, ProjectIndex, run, Workspace
from tools.quality.settings import (
    ArtifactScope,
    CS_SUFFIXES,
    FULL_TRIGGER_FILES,
    IGNORE_FIXTURE_PREFIXES,
    QualitySettings,
    STATIC_FULL_TRIGGER_PREFIXES,
)


# --- [TYPES] ---------------------------------------------------------------------------

type ProjectMode = Literal["parity", "closure"]
type StaticOutcome = Literal["skip", "done"]
type StaticScope = Literal["changed", "full"]


# --- [CONSTANTS] -----------------------------------------------------------------------

_FORMAT_COMMON: Final[tuple[str, ...]] = ("--verify-no-changes",)
_FORMAT_ARGS: Final[dict[str, tuple[str, ...]]] = {"style": (*_FORMAT_COMMON, "--severity", "error"), "whitespace": _FORMAT_COMMON}
_ORPHAN_FULL_SUFFIXES: Final[tuple[str, ...]] = (".cs", ".props", ".targets")


# --- [MODELS] ----------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class StaticPlan:
    scope: StaticScope
    projects: tuple[str, ...]
    format_groups: tuple[tuple[str, tuple[str, ...]], ...] = ()


@dataclass(frozen=True, slots=True)
class _ChangedRoute:
    projects: frozenset[str] = frozenset()
    full: bool = False
    format_routes: tuple[tuple[str, str], ...] = ()


# --- [OPERATIONS] ------------------------------------------------------------------------


def _format_commands(root: Path, settings: QualitySettings, plan: StaticPlan) -> tuple[tuple[str, ...], ...]:
    match plan.scope:
        case "full":
            solution = str(settings.solution)
            return tuple(("format", kind, solution, *_FORMAT_ARGS[kind]) for kind in _FORMAT_ARGS)
        case "changed":
            return tuple(
                command
                for project, files in plan.format_groups
                for command in (
                    ("format", "style", str(root / project), "--include", *files, *_FORMAT_ARGS["style"]),
                    ("format", "whitespace", str(root / project), "--include", *files, *_FORMAT_ARGS["whitespace"]),
                )
            )
        case unreachable:
            assert_never(unreachable)


def _format_groups(routes: tuple[tuple[str, str], ...]) -> tuple[tuple[str, tuple[str, ...]], ...]:
    return tuple((project, tuple(file for _, file in files)) for project, files in groupby(sorted(routes, key=itemgetter(0)), key=itemgetter(0)))


def _gate_plan(
    plan: StaticPlan, settings: QualitySettings, scope: ArtifactScope, root: Path, mode: StaticScope
) -> Result[StaticOutcome, ProcessFault]:
    targets: tuple[str, ...] = ()
    match (plan.scope, plan.projects):
        case ("changed", ()):
            return Ok("skip")
        case ("full", ()):
            return Error(ProcessFault.fail("static", mode, detail=b"No C# projects selected"))
        case ("changed", projects):
            targets = tuple(str(root / project) for project in projects)
        case ("full", _):
            targets = (str(settings.solution),)
    configurations = settings.static_configurations(mode)
    return (
        dotnet_build(settings, scope, restore=settings.solution, targets=targets, configurations=configurations, max_cpu=settings.dotnet_max_cpu)
        .bind(
            lambda _: fold(
                _format_commands(root, settings, plan),
                None,
                lambda _, command: dotnet(*command, scope=scope, scoped=False, check=True).map(lambda _: None),
            )
        )
        .map(lambda _: "done")
    )


def _plan(settings: QualitySettings, scope: ArtifactScope) -> Result[StaticPlan, ProcessFault]:
    workspace = Workspace(settings.root)
    index = workspace.index()
    routed = reduce(
        lambda acc, file: _route_step(acc, file, index=index, workspace=workspace, root=settings.root), workspace.changed(), _ChangedRoute()
    )

    def full() -> Result[StaticPlan, ProcessFault]:
        return _projects(settings, workspace, "parity", scope=scope).map(lambda rows: StaticPlan(scope="full", projects=rows))

    def changed() -> Result[StaticPlan, ProcessFault]:
        return _projects(settings, workspace, "closure", tuple(sorted(routed.projects)), scope=scope).map(
            lambda rows: StaticPlan(scope="changed", projects=rows, format_groups=_format_groups(routed.format_routes))
        )

    plan_by_route: dict[tuple[bool, bool], Callable[[], Result[StaticPlan, ProcessFault]]] = {
        (False, False): lambda: Ok(StaticPlan(scope="changed", projects=())),
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
            return (
                Ok(rows)
                if delta == ()
                else Error(
                    ProcessFault.fail(
                        "static", "solution-parity", detail=f"Workspace.slnx project parity failed:\n{'\n'.join(f'- {item}' for item in delta)}"
                    )
                )
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


def _route_step(acc: _ChangedRoute, file: str, *, index: ProjectIndex, workspace: Workspace, root: Path) -> _ChangedRoute:
    ignored = not file.endswith(".csproj") and any(file.startswith(prefix) for prefix in IGNORE_FIXTURE_PREFIXES)
    full_trigger = file in FULL_TRIGGER_FILES or file.startswith(STATIC_FULL_TRIGGER_PREFIXES)
    relevant = full_trigger or file.endswith(".csproj") or Path(file).suffix in CS_SUFFIXES
    match (ignored, relevant, full_trigger):
        case (True, _, _) | (_, False, _):
            return acc
        case (_, _, True):
            return _ChangedRoute(acc.projects, full=True, format_routes=acc.format_routes)
        case _:
            owner = workspace.owner_rel(index, root / file)
            match owner:
                case str(owner):
                    return _ChangedRoute(
                        acc.projects | {owner},
                        full=acc.full,
                        format_routes=(*acc.format_routes, (owner, file)) if file.endswith(".cs") else acc.format_routes,
                    )
                case None if file.endswith(_ORPHAN_FULL_SUFFIXES):
                    return _ChangedRoute(acc.projects, full=True, format_routes=acc.format_routes)
                case _:
                    return acc


@beartype
def run_static_rail(settings: QualitySettings, scope: ArtifactScope, mode: StaticScope) -> Result[StaticOutcome, ProcessFault]:
    return _plan(settings, scope).bind(lambda plan: _gate_plan(plan, settings, scope, settings.root, mode))
