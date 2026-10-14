"""Blend file saves, scoped reads, dependency packs, projects, and saved-data comparison."""

from collections import Counter
from collections.abc import Callable, Generator, Iterable, Mapping, Sequence
from contextlib import contextmanager, ExitStack
from enum import auto, Enum
from itertools import accumulate, batched, count, starmap, takewhile
from math import prod
import os
from pathlib import Path
import re
import shutil
import struct
from typing import Literal, Self

from _blendfile_header import BlendFileHeader, BlendHeaderError, BlockHeader
import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderReadFault, BlenderResult, BlenderWriteFault
from blender.imaging.faults import accumulated, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Load = OpenFile | Revert | Recover | Home
type Relink = Mirrored | Rebased | Mapping[tuple[Handle[bpy.types.ID], str], str]


class SaveMode(Enum):
    """Active file, independent copy, or native numbered save."""

    ACTIVE = auto()
    COPY = auto()
    INCREMENTAL = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class BlendFile(msgspec.Struct, frozen=True, kw_only=True):
    """Session file state without references invalidated by a load."""

    path: Option[Path]
    dirty: bool
    version: tuple[int, int, int]
    autopack: bool
    blocked: Option[str]

    @classmethod
    def read(cls) -> Self:
        """Returns current file state and any script blocked during loading."""
        return cls(
            path=Option.of_optional(bpy.data.filepath or None).map(Path),
            dirty=bpy.data.is_dirty,
            version=bpy.data.version,  # type: ignore[arg-type]
            autopack=bpy.data.use_autopack,
            blocked=Option.of_optional(bpy.app.autoexec_fail_message or None),
        )


class OpenFile(msgspec.Struct, frozen=True, kw_only=True):
    """File replacing session data, with optional UI and script choices."""

    path: Path
    ui: Option[bool] = Nothing
    scripts: Option[bool] = Nothing


class Revert(msgspec.Struct, frozen=True, kw_only=True):
    """Session's saved file loaded again."""

    scripts: Option[bool] = Nothing


class Recover(msgspec.Struct, frozen=True, kw_only=True):
    """Last session's quit file loaded."""

    scripts: Option[bool] = Nothing


class Home(msgspec.Struct, frozen=True, kw_only=True):
    """Startup file, optionally factory data or empty data."""

    factory: bool = False
    empty: bool = False
    ui: Option[bool] = Nothing


class Dependency(msgspec.Struct, frozen=True, kw_only=True):
    """External file with its original data-block owner and current presence."""

    owner: Handle[bpy.types.ID]
    path: Path
    present: bool


class Mirrored(msgspec.Struct, frozen=True, kw_only=True):
    """Absolute local paths rewritten between mirrored file locations."""

    root: Path
    source: Path


class Rebased(msgspec.Struct, frozen=True, kw_only=True):
    """Paths under a source directory moved under another directory."""

    source: Path
    target: Path


class Pack(msgspec.Struct, frozen=True, kw_only=True):
    """Written blend copy, copied dependencies, and missing dependencies."""

    blend: Path
    files: tuple[Path, ...]
    missing: tuple[Dependency, ...]


class TemplateVariable(msgspec.Struct, frozen=True, kw_only=True):
    """Project value and description; its Python type determines native storage."""

    value: Path | str | int | float
    description: str = ""


class Project(msgspec.Struct, frozen=True, kw_only=True):
    """Desired project name, root, variables, and asset libraries."""

    name: str
    root: Path
    variables: Mapping[str, TemplateVariable] = msgspec.field(default_factory=dict)
    libraries: Mapping[str, str] = msgspec.field(default_factory=dict)


class Difference(msgspec.Struct, frozen=True, kw_only=True):
    """Unequal normalized block counts for one code and DNA struct."""

    code: str
    name: str
    left: int
    right: int


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LIFTS]


def folder_made(folder: Path) -> BlenderResult[Path]:
    """Returns an existing or created folder, or its native write refusal."""
    return catch(lambda: folder.mkdir(parents=True, exist_ok=True), exception=OSError)().map(lambda _: folder).map_error(lambda error: (BlenderWriteFault(request=str(folder), error=error),))


def files_copied(pairs: Iterable[tuple[Path, Path]]) -> BlenderResult[tuple[Path, ...]]:
    """Returns distinct copied targets in path order, or every independent write refusal."""

    def copy(source: Path, target: Path) -> Path:
        return target if target.exists() and target.samefile(source) else Path(shutil.copytree(source, target, dirs_exist_ok=True) if source.is_dir() else shutil.copy2(source, target))

    def one(source: Path, target: Path) -> BlenderResult[Path]:
        return folder_made(target.parent).bind(lambda _: catch(lambda: copy(source, target), exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=str(target), error=error),)))

    return accumulated(starmap(one, sorted(frozenset(pairs))))


def sourced[T](
    data: bpy.types.BlendData, path: Path, take: Callable[[object], tuple[T, Mapping[str, Sequence[str]]]], **keywords: bool
) -> BlenderResult[tuple[T, dict[str, tuple[bpy.types.ID | str | None, ...]]]]:
    """Returns a source selection and native destination slots, or a file-read refusal."""
    try:
        with data.libraries.load(str(path), **keywords) as (source, target):  # type: ignore[attr-defined, func-returns-value]  # ty: ignore[invalid-context-manager]
            value, chosen = take(source)
            for name, names in chosen.items():
                setattr(target, name, list(names))
        return Ok((value, {name: tuple(getattr(target, name)) for name in chosen}))
    except (OSError, ValueError) as error:
        return Error((BlenderReadFault(request=str(path), error=error),))


# --- [SAVE]


def saved(path: Path, kind: SaveMode, *, compress: Option[bool] = Nothing, remap: Option[bool] = Nothing) -> BlenderResult[Path]:
    """Returns the native saved path, or folder and operator refusals."""
    relative = remap.default_with(lambda: bpy.context.preferences.filepaths.use_relative_paths)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    def written() -> BlenderResult[Path]:
        match kind:
            case SaveMode.ACTIVE | SaveMode.INCREMENTAL:
                return called(bpy.ops.wm.save_mainfile, {"filepath": str(path), "relative_remap": relative, "incremental": kind is SaveMode.INCREMENTAL, "compress": compress}).map(
                    lambda _: Path(bpy.data.filepath)
                )
            case SaveMode.COPY:
                return called(bpy.ops.wm.save_as_mainfile, {"filepath": str(path), "relative_remap": relative, "copy": True, "compress": compress}).map(lambda _: path)

    return folder_made(path.parent).bind(lambda _: written())


def snapshot(path: Path) -> BlenderResult[Path]:
    """Returns an uncompressed comparison copy with unchanged paths, or save refusals."""
    return saved(path, SaveMode.COPY, compress=Some(False), remap=Some(False))


# --- [LOAD]


def session_loaded(load: Load) -> BlenderResult[BlendFile]:
    """Returns file state after replacing session data, or the native operator refusal."""
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
def inspected(path: Path) -> Generator[BlenderResult[bpy.types.BlendData]]:
    """Yields another file's temporary native store, or its read refusal, until scope exit."""
    with bpy.data.temp_data(filepath="") as data:  # type: ignore[var-annotated]
        stores = frozenset(
            prop.identifier
            for prop in bpy.types.BlendData.bl_rna.properties
            if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, (bpy.types.Library, bpy.types.WorkSpace))
        )
        yield sourced(data, path, lambda source: (None, {name: getattr(source, name) for name in sorted(stores.intersection(dir(source)))}), link=True).map(lambda _: data)  # ty: ignore[unsound-yield]


# --- [PATHS]


def mirrored(root: Path, path: Path) -> Path:
    """Returns a path under a mirror root, retaining its drive as a directory."""
    normal = Path(os.path.abspath(path))  # ruff: ignore[os-path-abspath]
    return root / normal.drive.removesuffix(":").lstrip("\\/") / normal.relative_to(normal.anchor)


def dependencies() -> tuple[Dependency, ...]:
    """Returns absolute external files, expanding image tiles and sequences, with their presence."""
    found: list[Dependency] = []

    def visited(owner: bpy.types.ID, path: str, _: bpy.types.BlendDataPathMeta) -> None:
        absolute = Path(os.path.abspath(bpy.path.abspath(path, library=owner.library)))  # ruff: ignore[os-path-abspath]
        found.append(Dependency(owner=Handle.of(owner), path=absolute, present=absolute.exists()))

    bpy.data.file_path_foreach(visited, flags={"SKIP_PACKED", "SKIP_WEAK_REFERENCES", "EXPAND_TOKENS", "EXPAND_SEQUENCES"})
    return tuple(found)


def relinked(relink: Relink) -> dict[tuple[Handle[bpy.types.ID], str], str]:
    """Returns original local writable paths keyed by owner and rewritten path."""
    changes: dict[tuple[Handle[bpy.types.ID], str], str] = {}

    def rewritten(owner: bpy.types.ID, path: str) -> Option[str]:
        match relink:
            case Mirrored(root=root, source=source):
                return Nothing if path.startswith("//") else Some(bpy.path.relpath(str(mirrored(root, Path(path))), start=str(mirrored(root, source.parent))))
            case Rebased(source=source, target=target):
                return (
                    Some(Path(os.path.abspath(bpy.path.abspath(path))))  # ruff: ignore[os-path-abspath]
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


def packed(root: Path, *, compress: Option[bool] = Nothing) -> BlenderResult[Pack]:
    """Returns a dependency pack, unsaved-file absence, or every save and copy refusal."""

    def written(source: Path) -> BlenderResult[Path]:
        changes = relinked(Mirrored(root=root, source=source))
        try:
            return saved(mirrored(root, source), SaveMode.COPY, compress=compress, remap=Some(False))
        finally:
            relinked(changes)

    found = dependencies()
    match BlendFile.read().path:
        case Option(tag="some", some=source):
            return paired(written(source), files_copied((item.path, mirrored(root, item.path)) for item in found if item.present)).map(
                lambda pair: Pack(blend=pair[0], files=pair[1], missing=tuple(item for item in found if not item.present))
            )
        case _:
            return Error((BlenderAbsentFault(owner="bpy.data", key="filepath"),))


# --- [PROJECT]


def projected(project: Project) -> BlenderResult[None]:
    """Returns success after reconciling declared project entries, or native edit/save refusals."""

    def initialized() -> BlenderResult[bpy.types.BlenderProject]:
        try:
            bpy.data.project_init(project.name, str(project.root))
        except RuntimeError as error:
            return Error((BlenderEditFault(request="bpy.data", error=error),))
        return Option.of_optional(bpy.data.project).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.data", key="project"),))

    def reconciled(current: bpy.types.BlenderProject) -> BlenderResult[None]:
        variables, libraries = dict(current.variables.items()), tuple(current.asset_libraries.items())
        with ExitStack() as cleanup:

            def placed(name: str, variable: TemplateVariable) -> BlenderResult[None]:
                kind: Literal["STRING", "INTEGER", "FLOAT"]
                values: Mapping[str, Value]
                match variable.value:
                    case Path() as path:
                        kind, values = "STRING", {"subtype": "FILEPATH", "value": str(path)}
                    case str() as text:
                        kind, values = "STRING", {"subtype": "NONE", "value": text}
                    case int() as number:
                        kind, values = "INTEGER", {"value": number}
                    case float() as number:
                        kind, values = "FLOAT", {"value": number}
                match Option.of_optional(variables.get(name)):
                    case Option(tag="some", some=item) if item.type == kind:
                        made = Some(item)
                    case held:
                        for item in held.to_list():
                            current.variables.remove(item)
                        try:
                            made = Option.of_optional(current.variables.new(name, type=kind))
                        except RuntimeError as error:
                            return Error((BlenderEditFault(request=repr(current.variables), error=error),))
                        for item in made.to_list():
                            cleanup.callback(current.variables.remove, item)
                return made.map(lambda item: accumulated(written(item, key, value) for key, value in {**values, "description": variable.description}.items()).map(lambda _: None)).default_with(
                    lambda: Error((BlenderAbsentFault(owner=repr(current.variables), key=name),))
                )

            def library(name: str, path: str) -> BlenderResult[None]:
                match tuple(item for held, item in libraries if held == name):
                    case ():
                        try:
                            made = Option.of_optional(current.asset_libraries.new(name=name, directory=path)).to_result_with(
                                lambda: (BlenderAbsentFault(owner=repr(current.asset_libraries), key=name),)
                            )
                        except RuntimeError as error:
                            return Error((BlenderEditFault(request=repr(current.asset_libraries), error=error),))
                        for item in made.to_option().to_list():
                            cleanup.callback(current.asset_libraries.remove, item)
                        return made.map(lambda _: None)
                    case items:
                        return accumulated(written(item, "path", path) for item in items).map(lambda _: None)

            return (
                accumulated((written(current, "name", project.name).map(lambda _: None), *starmap(placed, project.variables.items()), *starmap(library, project.libraries.items())))
                .bind(lambda _: called(bpy.ops.project.save_project, {}).map(lambda _: None) if current.is_dirty else Ok(None))
                .map(lambda _: cleanup.pop_all())
                .map(lambda _: None)
            )

    match bpy.data.project:
        case bpy.types.BlenderProject(root_path=root) as current if Path(root) == project.root:
            return reconciled(current)
        case _:
            return initialized().bind(reconciled)


# --- [COMPARISON]


def compared(left: Path, right: Path) -> BlenderResult[tuple[Difference, ...]]:
    """Returns unequal normalized block counts, or independent file-read/header refusals."""
    type BlockKey = tuple[bytes, bytes, int]
    type Normal = bytes | tuple[BlockKey | None, ...]
    type Canonical = tuple[str, str, bytes | tuple[Normal, ...]]

    class Pointers(msgspec.Struct, frozen=True, kw_only=True):
        offset: int
        codec: struct.Struct

        def normalized(self, data: bytes, start: int, addressed: Mapping[int, BlockKey]) -> Normal:
            return tuple(addressed.get(address) for address in self.codec.unpack_from(data, start + self.offset))

    class Span(msgspec.Struct, frozen=True, kw_only=True):
        name: str
        offset: int
        size: int
        text: bool

        def normalized(self, data: bytes, start: int) -> bytes:
            held = data[start + self.offset : start + self.offset + self.size]
            return held.partition(b"\0")[0] if self.text else held

    class StructLayout(msgspec.Struct, frozen=True, kw_only=True):
        name: str
        size: int
        members: tuple[Pointers | Span, ...]

        def normalized(self, data: bytes, total: int, addressed: Mapping[int, BlockKey]) -> bytes | tuple[Normal, ...]:
            return (
                tuple(
                    member.normalized(data, start, addressed) if isinstance(member, Pointers) else member.normalized(data, start) for start in range(0, len(data), self.size) for member in self.members
                )
                if 0 < len(data) == self.size * total
                else data
            )

    def layouts(dna: bytes, order: str, pointer: int) -> tuple[StructLayout, ...]:
        int32, address = struct.Struct(f"{order}i"), "Q" if pointer == 8 else "I"

        def strings(offset: int) -> tuple[tuple[str, ...], int]:
            (total,) = int32.unpack_from(dna, offset)
            items = dna[offset + int32.size :].split(b"\0", total)[:total]
            return tuple(item.decode() for item in items), (offset + int32.size + sum(len(item) + 1 for item in items) + 3) & ~3

        names, typed = strings(8)
        types, sized = strings(typed + 4)
        lengths = struct.unpack_from(f"{order}{len(types)}H", dna, sized + 4)
        offset = ((sized + 4 + 2 * len(types) + 3) & ~3) + 4
        (total,) = int32.unpack_from(dna, offset)
        offset += int32.size
        structures: dict[int, tuple[tuple[int, int], ...]] = {}
        for _ in range(total):
            kind, width = struct.unpack_from(f"{order}2H", dna, offset)
            structures[kind] = tuple(batched(struct.unpack_from(f"{order}{2 * width}H", dna, offset + 4), 2))
            offset += 4 + 4 * width

        def members(kind: int, offset: int) -> tuple[Pointers | Span, ...]:
            found: list[Pointers | Span] = []
            for member, name in structures[kind]:
                size = (pointer if "*" in names[name] else lengths[member]) * prod(int(n) for n in re.findall(r"\[(\d+)\]", names[name]))
                if "*" in names[name]:
                    found.append(Pointers(offset=offset, codec=struct.Struct(f"{order}{size // pointer}{address}")))
                elif member in structures:
                    found.extend(leaf for start in range(offset, offset + size, lengths[member]) for leaf in members(member, start))
                else:
                    found.append(Span(name=names[name].partition("[")[0], offset=offset, size=size, text=types[member] == "char" and "[" in names[name]))
                offset += size
            return tuple(found)

        return tuple(StructLayout(name=types[kind], size=lengths[kind], members=members(kind, 0)) for kind in structures)

    def canonical(path: Path) -> BlenderResult[Counter[Canonical]]:
        try:
            with path.open("rb") as file:
                header = BlendFileHeader(file)  # type: ignore[call-arg]  # ty: ignore[too-many-positional-arguments]
                shape = header.create_block_header_struct()  # type: ignore[func-returns-value]
                blocks = tuple(takewhile(lambda block: block[0].code != b"ENDB", ((head, file.read(head.size)) for head in (BlockHeader(file, shape) for _ in count()))))  # type: ignore[call-arg]  # ty: ignore[too-many-positional-arguments]
        except OSError as error:
            return Error((BlenderReadFault(request=str(path), error=error),))
        except BlendHeaderError:  # type: ignore[misc]  # ty: ignore[invalid-exception-caught]
            return Error((BlenderAbsentFault(owner=str(path), key=BlendFileHeader.__name__),))
        shapes = layouts(next(data for head, data in blocks if head.code == b"DNA1"), "<" if header.is_little_endian else ">", header.pointer_size)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        named = next(member for member in next(layout for layout in shapes if layout.name == "ID").members if isinstance(member, Span) and member.name == "name")

        def owned(above: BlockKey, block: tuple[BlockHeader, bytes]) -> BlockKey:
            code = block[0].code
            if len(code) == 4:
                return above[0], above[1], above[2] + 1
            name = named.normalized(block[1], 0)
            return (name, name, 0) if code == b"LI" else (above[0], name, 0)

        keys = tuple(accumulate(blocks, owned, initial=(b"", b"", 0)))[1:]
        addressed = {head.addr_old: key for (head, _), key in zip(blocks, keys)}
        return Ok(Counter((head.code.decode(), shapes[head.sdna_index].name, shapes[head.sdna_index].normalized(data, head.count, addressed)) for head, data in blocks if head.code != b"TEST"))

    def differences(before: Counter[Canonical], after: Counter[Canonical]) -> tuple[Difference, ...]:
        lost = Counter(key[:2] for key in (before - after).elements())
        gained = Counter(key[:2] for key in (after - before).elements())
        return tuple(Difference(code=code, name=name, left=lost[code, name], right=gained[code, name]) for code, name in sorted(lost.keys() | gained.keys()))

    return paired(canonical(left), canonical(right)).map(lambda pair: differences(*pair))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "BlendFile",
    "Dependency",
    "Difference",
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
    "compared",
    "dependencies",
    "files_copied",
    "folder_made",
    "inspected",
    "mirrored",
    "packed",
    "projected",
    "relinked",
    "saved",
    "session_loaded",
    "snapshot",
    "sourced",
]
