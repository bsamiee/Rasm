"""Blend file saves, loads, dependency packs, and projects."""

from collections.abc import Callable, Generator, Mapping, Sequence
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import auto, Enum
from functools import partial
from itertools import starmap
import os
from pathlib import Path
from types import MappingProxyType
from typing import Final, Literal, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.data.ids.ownership import retained
from blender.bpy.data.ids.store import Handle, STORES
from blender.bpy.data.rna.descriptors import assigned, typed, Value
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason
from blender.records.stores import files_copied, folder_made, mirrored

# --- [TYPES] ----------------------------------------------------------------------------

type Load = OpenFile | Revert | Recover | Home
type Relink = Mirrored | Rebased | Mapping[tuple[Handle[bpy.types.ID], str], str]


class SaveMode(Enum):
    """Save to active, copied, or numbered file."""

    ACTIVE = auto()
    COPY = auto()
    INCREMENTAL = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_ENTERED: Final[Callable[[ExitStack, AbstractContextManager[tuple[object, object]]], Result[object, OSError | ValueError]]] = catch(
    catch(ExitStack.enter_context, exception=ValueError), exception=OSError
)

# --- [MODELS] ---------------------------------------------------------------------------


class BlendFile(msgspec.Struct, frozen=True, kw_only=True):
    """Session file state safe across loads."""

    path: Option[Path]
    dirty: bool
    version: tuple[int, int, int]
    autopack: bool
    blocked: Option[str]

    @classmethod
    def read(cls) -> Self:
        """Returns current file state."""
        return cls(
            path=Option.of_optional(bpy.data.filepath or None).map(Path),
            dirty=bpy.data.is_dirty,
            version=bpy.data.version,  # type: ignore[arg-type]
            autopack=bpy.data.use_autopack,
            blocked=Option.of_optional(bpy.app.autoexec_fail_message or None),
        )


class OpenFile(msgspec.Struct, frozen=True, kw_only=True):
    """File replacing session data."""

    path: Path
    ui: Option[bool] = Nothing
    scripts: Option[bool] = Nothing


class Revert(msgspec.Struct, frozen=True, kw_only=True):
    """Revert to saved file."""

    scripts: Option[bool] = Nothing


class Recover(msgspec.Struct, frozen=True, kw_only=True):
    """Last session quit file."""

    scripts: Option[bool] = Nothing


class Home(msgspec.Struct, frozen=True, kw_only=True):
    """Startup file, optionally factory or empty."""

    factory: bool = False
    empty: bool = False
    ui: Option[bool] = Nothing


class Dependency(msgspec.Struct, frozen=True, kw_only=True):
    """External file of original ID."""

    owner: Handle[bpy.types.ID]
    path: Path
    present: bool


class Mirrored(msgspec.Struct, frozen=True, kw_only=True):
    """Absolute paths made relative to mirrored source under root."""

    root: Path
    source: Path


class Rebased(msgspec.Struct, frozen=True, kw_only=True):
    """Paths under source directory moved under another directory."""

    source: Path
    target: Path


class Pack(msgspec.Struct, frozen=True, kw_only=True):
    """Dependency pack result."""

    blend: Path
    files: tuple[Path, ...]
    missing: tuple[Dependency, ...]


class TemplateVariable(msgspec.Struct, frozen=True, kw_only=True):
    """Project variable, value type choosing variable type."""

    value: Path | str | int | float
    description: str = ""


class Project(msgspec.Struct, frozen=True, kw_only=True):
    """Desired project state."""

    name: str
    root: Path
    variables: Mapping[str, TemplateVariable] = MappingProxyType({})
    libraries: Mapping[str, str] = MappingProxyType({})


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LIFTS]


def sourced[T](
    data: bpy.types.BlendData, path: Path, take: Callable[[object], tuple[T, Mapping[str, Sequence[str]]]], **keywords: bool
) -> Outcome[tuple[T, dict[str, tuple[bpy.types.ID | str | None, ...]]]]:
    """Returns source selection with loaded destination slots, read fault when the library load refuses the file."""
    load: AbstractContextManager[tuple[object, object]] = data.libraries.load(str(path), **keywords)  # type: ignore[func-returns-value, assignment]  # ty: ignore[invalid-assignment]

    def requested(ends: tuple[object, ...]) -> tuple[object, T, Mapping[str, Sequence[str]]]:
        value, chosen = take(ends[0])
        for name, names in chosen.items():
            setattr(ends[1], name, list(names))
        return ends[1], value, chosen

    with ExitStack() as loading:
        requests = _ENTERED(loading, load).map_error(partial(Fault.of, RecordReason.UNREADABLE, str(path))).bind(lambda ends: typed(ends, tuple, str(path))).map(requested)
    return requests.map(lambda held: (held[1], {name: tuple(getattr(held[0], name)) for name in held[2]}))


# --- [SAVE]


def saved(path: Path, kind: SaveMode, *, compress: Option[bool] = Nothing, remap: Option[bool] = Nothing) -> Outcome[Path]:
    """Saves file, returning saved path."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    relative = remap.default_with(lambda: preferences.filepaths.use_relative_paths)

    def written() -> Outcome[Path]:
        match kind:
            case SaveMode.ACTIVE | SaveMode.INCREMENTAL:
                return called(bpy.ops.wm.save_mainfile, {"filepath": str(path), "relative_remap": relative, "incremental": kind is SaveMode.INCREMENTAL, "compress": compress}).map(
                    lambda _: Path(bpy.data.filepath)
                )
            case SaveMode.COPY:
                return called(bpy.ops.wm.save_as_mainfile, {"filepath": str(path), "relative_remap": relative, "copy": True, "compress": compress}).map(lambda _: path)

    return folder_made(path.parent).bind(lambda _: written())


# --- [LOAD]


def session_loaded(load: Load) -> Outcome[BlendFile]:
    """Replaces session data, returning file state."""
    match load:
        case OpenFile(path=path, ui=ui, scripts=scripts):
            answer = called(bpy.ops.wm.open_mainfile, {"filepath": str(path), "load_ui": ui, "use_scripts": scripts})
        case Revert(scripts=scripts):
            answer = called(bpy.ops.wm.revert_mainfile, {"use_scripts": scripts})
        case Recover(scripts=scripts):
            answer = called(bpy.ops.wm.recover_last_session, {"use_scripts": scripts})
        case Home(factory=factory, empty=empty, ui=ui):
            answer = called(bpy.ops.wm.read_homefile, {"use_factory_startup": factory, "use_empty": empty, "load_ui": ui})
    return answer.map(lambda _: BlendFile.read())


@contextmanager
def inspected(path: Path) -> Generator[Outcome[bpy.types.BlendData]]:
    """Yields temporary BlendData of another file until scope exit, libraries and workspaces left unlinked."""
    stores = frozenset(identifier for kind, identifier in STORES.items() if not issubclass(kind, bpy.types.Library | bpy.types.WorkSpace))
    with bpy.data.temp_data(filepath="") as data:  # type: ignore[var-annotated]
        yield sourced(data, path, lambda source: (None, {name: getattr(source, name) for name in sorted(stores.intersection(dir(source)))}), link=True).map(lambda _: data)  # ty: ignore[unsound-yield]


# --- [PATHS]


def dependencies() -> tuple[Dependency, ...]:
    """Returns absolute external files, tiles and sequences expanded."""
    found: list[Dependency] = []

    def visited(owner: bpy.types.ID, path: str, _: bpy.types.BlendDataPathMeta) -> None:
        absolute = Path(os.path.abspath(bpy.path.abspath(path, library=owner.library)))
        found.append(Dependency(owner=Handle.of(owner), path=absolute, present=absolute.exists()))

    bpy.data.file_path_foreach(visited, flags={"SKIP_PACKED", "SKIP_WEAK_REFERENCES", "EXPAND_TOKENS", "EXPAND_SEQUENCES"})
    return tuple(found)


def relinked(relink: Relink) -> dict[tuple[Handle[bpy.types.ID], str], str]:
    """Rewrites local writable paths, returning relink that restores them."""
    changes: dict[tuple[Handle[bpy.types.ID], str], str] = {}

    def rewritten(owner: bpy.types.ID, path: str) -> Option[str]:
        match relink:
            case Mirrored(root=root, source=source):
                return Nothing if path.startswith("//") else Some(bpy.path.relpath(str(mirrored(root, Path(path))), start=str(mirrored(root, source.parent))))
            case Rebased(source=source, target=target):
                return (
                    Some(Path(os.path.abspath(bpy.path.abspath(path))))
                    .filter(lambda held: held.is_relative_to(source))
                    .map(lambda held: target / held.relative_to(source))
                    .map(lambda moved: bpy.path.relpath(str(moved)) if path.startswith("//") and moved.anchor == Path(bpy.data.filepath).anchor else str(moved))
                )
            case Mapping() as written:
                return Option.of_optional(written.get((Handle.of(owner), path)))

    def visited(owner: bpy.types.ID, path: str, meta: bpy.types.BlendDataPathMeta) -> str | None:
        after = Nothing if meta.is_readonly else rewritten(owner, path)
        for text in after.filter(lambda text: text != path).to_list():
            changes[Handle.of(owner), text] = path
        return after.to_optional()

    bpy.data.file_path_foreach(visited, flags={"SKIP_PACKED", "SKIP_WEAK_REFERENCES", "SKIP_LINKED"})
    return changes


def packed(root: Path, *, compress: Option[bool] = Nothing) -> Outcome[Pack]:
    """Packs file and dependencies under root, absent fault for unsaved file."""

    def written(source: Path) -> Outcome[Path]:
        with ExitStack() as restoring:
            restoring.callback(relinked, relinked(Mirrored(root=root, source=source)))
            return saved(mirrored(root, source), SaveMode.COPY, compress=compress, remap=Some(False))

    found = dependencies()
    match BlendFile.read().path:
        case Option(tag="some", some=source):
            return paired(written(source), files_copied((item.path, mirrored(root, item.path)) for item in found if item.present)).map(
                lambda pair: Pack(blend=pair[0], files=pair[1], missing=tuple(item for item in found if not item.present))
            )
        case _:
            return Error(Fault.of(BlenderReason.ABSENT, "bpy.data", member="filepath"))


# --- [PROJECT]


def projected(project: Project) -> Outcome[None]:
    """Reconciles open project with desired state, removing items it created when any write refuses."""

    def initialized() -> Outcome[bpy.types.BlenderProject]:
        return (
            catch(bpy.data.project_init, exception=RuntimeError)(project.name, str(project.root))
            .map_error(partial(Fault.of, BlenderReason.REFUSED, "bpy.data"))
            .bind(lambda _: Option.of_optional(bpy.data.project).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, "bpy.data", member="project")))
        )

    def reconciled(current: bpy.types.BlenderProject) -> Outcome[None]:
        variables, libraries = dict(current.variables.items()), tuple(current.asset_libraries.items())
        with ExitStack() as cleanup:

            def created[T](request: str, name: str, made: Callable[[], T | None], removed: Callable[[T], object]) -> Outcome[T]:
                def held(item: T) -> T:
                    cleanup.callback(removed, item)
                    return item

                return (
                    catch(made, exception=RuntimeError)()
                    .map_error(partial(Fault.of, BlenderReason.REFUSED, request))
                    .bind(lambda item: Option.of_optional(item).map(held).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, request, member=name)))
                )

            def placed(name: str, variable: TemplateVariable) -> Outcome[None]:
                kind: Literal["STRING", "INTEGER", "FLOAT"]
                values: Mapping[str, Value]
                match variable.value:
                    case Path() as path:
                        kind, values = "STRING", {"subtype": "FILEPATH", "value": str(path)}
                    case str(text):
                        kind, values = "STRING", {"subtype": "NONE", "value": text}
                    case int(number):
                        kind, values = "INTEGER", {"value": number}
                    case float(number):
                        kind, values = "FLOAT", {"value": number}

                def replaced() -> Outcome[bpy.types.ProjectVariable]:
                    for item in Option.of_optional(variables.get(name)).to_list():
                        current.variables.remove(item)
                    return created(repr(current.variables), name, lambda: current.variables.new(name, type=kind), current.variables.remove)

                return (
                    Option
                    .of_optional(variables.get(name))
                    .filter(lambda item: item.type == kind)
                    .map(Ok)
                    .default_with(replaced)
                    .bind(lambda item: assigned(item, {**values, "description": variable.description}))
                )

            def library(name: str, path: str) -> Outcome[None]:
                match tuple(item for held, item in libraries if held == name):
                    case ():
                        return created(repr(current.asset_libraries), name, lambda: current.asset_libraries.new(name=name, directory=path), current.asset_libraries.remove).map(lambda _: None)
                    case items:
                        return accumulated(assigned(item, {"path": path}) for item in items).map(lambda _: None)

            return retained(
                accumulated((assigned(current, {"name": project.name}), *starmap(placed, project.variables.items()), *starmap(library, project.libraries.items()))).bind(
                    lambda _: called(bpy.ops.project.save_project, {}).map(lambda _: None) if current.is_dirty else Ok(None)
                ),
                cleanup,
            )

    match bpy.data.project:
        case bpy.types.BlenderProject(root_path=root) as current if Path(root) == project.root:
            return reconciled(current)
        case _:
            return initialized().bind(reconciled)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "BlendFile",
    "Dependency",
    "Home",
    "Load",
    "Mirrored",
    "OpenFile",
    "Pack",
    "Project",
    "Rebased",
    "Recover",
    "Relink",
    "Revert",
    "SaveMode",
    "TemplateVariable",
    "dependencies",
    "inspected",
    "packed",
    "projected",
    "relinked",
    "saved",
    "session_loaded",
    "sourced",
]
