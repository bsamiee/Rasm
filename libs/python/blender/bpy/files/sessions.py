"""Open session: file state, loads and saves through the host operators, dependency paths, packs through blender-asset-tracer, and projects."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from functools import partial
from itertools import starmap
import os
from pathlib import Path, PurePath, PurePosixPath
from types import MappingProxyType
from typing import Final, Literal, override, Self

from blender_asset_tracer.blendfile.exceptions import BlendFileError
from blender_asset_tracer.file_usage import FileInfo, Options
from blender_asset_tracer.hashing import get_hasher
from blender_asset_tracer.pack import BATPacker, BATPackReporter, FileTransferProtocol
from blender_asset_tracer.path_rewriting_process import BackgroundProcessNotRunningError
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called, Operation, Setting
from blender.bpy.data.ids.ownership import retained
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Faults, Outcome, RecordReason
from blender.records.stores import folder_made
from blender.rendering.batch.queue import PackedFile

# --- [TYPES] ----------------------------------------------------------------------------

type Relink = Rebased | Mapping[tuple[Handle[bpy.types.ID], str], str]

# --- [CONSTANTS] ------------------------------------------------------------------------

_OPTIONS: Final = Options()
_TRACING: Final[type[OSError | BlendFileError | BackgroundProcessNotRunningError]] = (OSError, BlendFileError, BackgroundProcessNotRunningError)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

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


class Dependency(msgspec.Struct, frozen=True, kw_only=True):
    """External file of original ID."""

    owner: Handle[bpy.types.ID]
    path: Path
    present: bool


class Rebased(msgspec.Struct, frozen=True, kw_only=True):
    """Paths under source directory moved under another directory."""

    source: Path
    target: Path


class Pack(msgspec.Struct, frozen=True, kw_only=True):
    """Packed blend file, every file the pack copied, faults of files the pack skipped, and the total and copied counts of its transfer."""

    blend: Path
    files: tuple[Path, ...]
    skipped: tuple[Fault, ...]
    transferred: tuple[int, int]


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


# --- [SERVICES] -------------------------------------------------------------------------


class PackReport(BATPackReporter):
    """blender-asset-tracer pack reporter holding a fault per missing file, refused copy, and refused rewrite."""

    __slots__ = ("_copied", "_faults")

    def __init__(self) -> None:
        """Starts with no copies and no faults."""
        self._copied: list[Path] = []
        self._faults: list[Fault] = []

    @property
    def copied(self) -> tuple[Path, ...]:
        """Files copied into the pack so far, in path order."""
        return tuple(sorted(self._copied))

    @property
    def skipped(self) -> tuple[Fault, ...]:
        """Faults reported so far, in report order."""
        return tuple(self._faults)

    @override
    def on_error_on_error(self, message: str, exception: Exception) -> None:
        """Holds a failed copy or rewrite callback."""
        self._faults.extend(Fault.of(BlenderReason.FAILED, message, exception))

    @override
    def on_copy_start(self, src: Path, dest: PurePath) -> None:
        """Ignores a copy start."""

    @override
    def on_copy_done(self, src: Path, dest: PurePath) -> None:
        """Holds a finished copy."""
        self._copied.append(Path(dest))

    @override
    def on_copy_error(self, src: Path, dest: PurePath, errormsg: str) -> None:
        """Holds a refused copy."""
        self._faults.extend(Fault.of(RecordReason.UNWRITABLE, str(dest), errormsg))

    @override
    def on_rewrite_start(self, blendfile: Path, save_to: Path) -> None:
        """Ignores a rewrite start."""

    @override
    def on_rewrite_done(self, blendfile: Path, save_to: Path) -> None:
        """Ignores a finished rewrite."""

    @override
    def on_rewrite_error(self, blendfile: Path, save_to: Path, errormsg: str) -> None:
        """Holds a refused path rewrite."""
        self._faults.extend(Fault.of(RecordReason.UNWRITABLE, str(save_to), errormsg))

    @override
    def on_missing_file(self, blendfile: Path, relpath_in_pack: PurePath) -> None:
        """Holds a dependency missing on disk."""
        self._faults.extend(Fault.of(BlenderReason.ABSENT, str(blendfile.parent), member=blendfile.name))


class Packing:
    """blender-asset-tracer pack the extension's reports queue advances one step per pass, aborted at a refusal before it finishes."""

    __slots__ = ("_finished", "_packer", "_release", "_report")

    def __init__(self, packer: BATPacker, report: PackReport, release: ExitStack, finished: Callable[[Outcome[Pack]], Outcome[None]]) -> None:
        """Holds a started packer, its reporter, the scope holding its abort, and the step reading the finished pack."""
        self._packer, self._report, self._release, self._finished = packer, report, release, finished

    @property
    def transferred(self) -> tuple[int, int]:
        """Total and copied file counts of the transfer so far."""
        return self._packer.num_files_to_transfer()

    def advanced(self) -> Outcome[Option[Self]]:
        """Runs one pack step, keeping the pack queued while steps remain, else finishing with the pack, or aborting and finishing with the refusal."""
        match _traced(self._packer.step).map_error(partial(Fault.of, RecordReason.UNREADABLE, str(self._packer.project_root))):
            case Result(tag="ok", ok=True):
                return Ok(Some(self))
            case Result(tag="ok"):
                self._release.pop_all()
                pack = Pack(blend=self._packer.blendfile_location_abspath(), files=self._report.copied, skipped=self._report.skipped, transferred=self.transferred)
                return self._finished(Ok(pack)).map(lambda _: Nothing)
            case Result(error=faults):
                self._release.close()
                return self._finished(Error(faults)).map(lambda _: Nothing)


class FarmTransfer(FileTransferProtocol):
    """blender-asset-tracer transfer placing every traced file present on disk at its path in the pack with its SHA-256, copying nothing, a farm upload sending the placements."""

    __slots__ = ("_placed",)

    def __init__(self) -> None:
        """Holds no placement before the packer starts the transfer."""
        self._placed: tuple[PackedFile, tuple[PackedFile, ...]] | None = None

    @property
    def placed(self) -> tuple[PackedFile, tuple[PackedFile, ...]]:
        """Placement of the session blend and of every traced file, which exist once the packer started the transfer."""
        if self._placed is None:
            raise ValueError(self)
        return self._placed

    @override
    def start(self, batpacker: BATPacker) -> None:
        """Places the session blend and every traced file present on disk, reporting each missing one."""
        hasher = get_hasher()

        def packed_at(info: FileInfo) -> PurePath:
            """Returns the path a traced file takes in the pack, which the trace sets on every file it hands a transfer."""
            match info.relpath_in_pack:
                case PurePath() as held:
                    return held
                case None:
                    raise ValueError(info)

        def placement(info: FileInfo) -> PackedFile:
            return PackedFile(source=str(info.path_to_pack), path=packed_at(info).as_posix(), digest=bytes.fromhex(hasher(info.path_to_pack)))

        infos = tuple(batpacker.all_files_to_copy().values())
        for info in infos:
            if not info.path_to_pack.exists():
                batpacker.reporter.on_missing_file(info.path_to_pack, packed_at(info))
        self._placed = (placement(batpacker.source_file_info()), tuple(placement(info) for info in infos if info.path_to_pack.exists()))

    @override
    def step(self) -> bool:
        """Places nothing further, every placement made at start."""
        return False

    @override
    def blendfile_location_in_pack(self) -> PurePosixPath:
        """Path of the session blend in the pack."""
        return PurePosixPath(self.placed[0].path)

    @override
    def blendfile_location_abspath(self) -> Path:
        """Local file the pack sends as the session blend, its paths rewritten to the pack."""
        return Path(self.placed[0].source)

    @override
    def num_files_to_transfer(self) -> tuple[int, int]:
        """Total and placed file counts, equal once started."""
        count = len(self.placed[1])
        return count, count


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SAVE]


def saved(path: Path, operator: Operation, /, **params: Setting) -> Outcome[Path]:
    """Saves the session through a save operator and its keywords, returning the file written.

    Args:
        path: File to write.
        operator: `bpy.ops.wm.save_mainfile` or `bpy.ops.wm.save_as_mainfile`.
        **params: Operator keywords besides `filepath` (`copy`, `compress`, `relative_remap`, `incremental`).

    Returns:
        Path of the written file, the session's new path unless saving a copy.
    """
    return folder_made(path.parent).bind(lambda _: called(operator, {"filepath": str(path), **params})).map(lambda _: path if params.get("copy") is True else Path(bpy.data.filepath))


# --- [LOAD]


def session_loaded(operator: Operation, /, **params: Setting) -> Outcome[BlendFile]:
    """Replaces session data through a load operator and its keywords, returning file state.

    Args:
        operator: `bpy.ops.wm.open_mainfile`, `revert_mainfile`, `recover_last_session`, or `read_homefile`.
        **params: Operator keywords (`filepath`, `load_ui`, `use_scripts`, `use_factory_startup`, `use_empty`).

    Returns:
        File state after the load.
    """
    return called(operator, params).map(lambda _: BlendFile.read())


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
            case Rebased(source=source, target=target):
                return (
                    Some(Path(os.path.abspath(bpy.path.abspath(path))))
                    .filter(lambda held: held.is_relative_to(source))
                    .map(lambda held: target / held.relative_to(source))
                    .map(lambda moved: bpy.path.relpath(str(moved)) if path.startswith("//") and moved.anchor == Path(bpy.data.filepath).anchor else str(moved))
                )
            case Mapping() as held:
                return Option.of_optional(held.get((Handle.of(owner), path)))

    def visited(owner: bpy.types.ID, path: str, meta: bpy.types.BlendDataPathMeta) -> str | None:
        after = Nothing if meta.is_readonly else rewritten(owner, path)
        for text in after.filter(lambda text: text != path).to_list():
            changes[Handle.of(owner), text] = path
        return after.to_optional()

    bpy.data.file_path_foreach(visited, flags={"SKIP_PACKED", "SKIP_WEAK_REFERENCES", "SKIP_LINKED"})
    return changes


# --- [PACKS]


def _released[T](release: ExitStack, faults: Faults) -> Outcome[T]:
    """Runs the held abort, keeping the refusal."""
    release.close()
    return Error(faults)


def _session() -> Outcome[str]:
    """Returns the saved session file a pack reads, required fault for an unsaved or modified session."""
    return Option.of_optional(bpy.data.filepath or None).filter(lambda _: not bpy.data.is_dirty).to_result_with(lambda: Fault.of(BlenderReason.REQUIRED, "saved session file"))


def _packer(target: Path | FileTransferProtocol, project: Path, options: Options, report: PackReport) -> BATPacker:
    """Builds a packer copying into a pack folder, or handing each traced file to a transfer."""
    return BATPacker(project, options, report, pack_target_dir=target) if isinstance(target, Path) else BATPacker(project, options, report, file_transfer=target)


def _drained(packer: BATPacker) -> None:
    """Runs every remaining step of a started packer."""
    while packer.step():
        pass


def _traced[T](step: Callable[[], T]) -> Result[T, OSError | BlendFileError | BackgroundProcessNotRunningError]:
    """Runs one blender-asset-tracer call, which raises for an unreadable file, an unparsable blend file, and a stopped rewrite process."""
    return catch(step, exception=_TRACING)()


def packed(identity: Identity, target: Path | FileTransferProtocol, *, project: Path, finished: Callable[[Outcome[Pack]], Outcome[None]], options: Options = _OPTIONS) -> Outcome[None]:
    """Starts packing the saved session file and every dependency under target, blend paths rewritten to the pack, the reports queue stepping it.

    Args:
        identity: Extension whose reports queue advances the pack, whole at once in background runs.
        target: Pack folder, or the transfer placing each traced file in place of a copy.
        project: Root whose relative layout the pack keeps, files outside it relocated under the pack.
        finished: Step reading the finished pack or the refusal that aborted it.
        options: blender-asset-tracer trace options.

    Returns:
        Ok(None) once the first step ran, required fault for an unsaved or modified session, read fault for an unreadable file.
    """
    report = PackReport()

    def started(_: str) -> Outcome[None]:
        packer = _packer(target, project, options, report)
        release = ExitStack()
        release.callback(packer.abort)
        return (
            _traced(packer.start)
            .map_error(partial(Fault.of, RecordReason.UNREADABLE, str(project)))
            .or_else_with(partial(_released, release))
            .bind(lambda _: identity.reports.queued(Packing(packer, report, release, finished)))
        )

    return _session().bind(started)


def traced(target: Path | FileTransferProtocol, *, project: Path, options: Options = _OPTIONS) -> Outcome[Pack]:
    """Packs the saved session file and every dependency under target in one call, the form a background run takes, where no pass steps a pack.

    Args:
        target: Pack folder, or the transfer placing each traced file in place of a copy.
        project: Root whose relative layout the pack keeps, files outside it relocated under the pack.
        options: blender-asset-tracer trace options.

    Returns:
        The finished pack with its skipped files as faults, required fault for an unsaved or modified session, read fault for an unreadable file.
    """
    report = PackReport()

    def started(_: str) -> Outcome[Pack]:
        packer = _packer(target, project, options, report)
        release = ExitStack()
        release.callback(packer.abort)
        return (
            _traced(packer.start)
            .bind(lambda _: _traced(partial(_drained, packer)))
            .map_error(partial(Fault.of, RecordReason.UNREADABLE, str(project)))
            .or_else_with(partial(_released, release))
            .map(lambda _: Pack(blend=packer.blendfile_location_abspath(), files=report.copied, skipped=report.skipped, transferred=packer.num_files_to_transfer()))
        )

    return _session().bind(started)


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
                values: Mapping[RnaPath, Value]
                match variable.value:
                    case Path() as path:
                        kind = "STRING"
                        values = {("subtype",): "FILEPATH", ("value",): str(path)}
                    case str(text):
                        kind = "STRING"
                        values = {("subtype",): "NONE", ("value",): text}
                    case int(number):
                        kind = "INTEGER"
                        values = {("value",): number}
                    case float(number):
                        kind = "FLOAT"
                        values = {("value",): number}

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
                    .bind(lambda item: assigned(item, {**values, ("description",): variable.description}))
                )

            def library(name: str, path: str) -> Outcome[None]:
                match tuple(item for held, item in libraries if held == name):
                    case ():
                        return created(repr(current.asset_libraries), name, lambda: current.asset_libraries.new(name=name, directory=path), current.asset_libraries.remove).map(lambda _: None)
                    case items:
                        return accumulated(assigned(item, {("path",): path}) for item in items).map(lambda _: None)

            def saved_project(_: object) -> Outcome[None]:
                return catch(bpy.data.project_save, exception=RuntimeError)().map_error(partial(Fault.of, BlenderReason.REFUSED, "bpy.data.project")) if current.is_dirty else Ok(None)

            return retained(
                accumulated((assigned(current, {("name",): project.name}), *starmap(placed, project.variables.items()), *starmap(library, project.libraries.items()))).bind(saved_project), cleanup
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
    "FarmTransfer",
    "Pack",
    "PackReport",
    "Project",
    "Rebased",
    "Relink",
    "TemplateVariable",
    "dependencies",
    "packed",
    "projected",
    "relinked",
    "saved",
    "session_loaded",
    "traced",
]
