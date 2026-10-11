"""Typed stock recipes and user preset files, with native snapshot clipboard transfer."""

from collections.abc import Callable, Iterator, Mapping
from pathlib import Path
from typing import Annotated

from atomicwrites import move_atomic
from bl_operators.presets import AddPresetBase
import bpy
from expression import Error, Nothing, Ok, Option
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from rasm.blender.data.rna.snapshots import Applied, Snapshot
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderImagingFault, BlenderReadFault, BlenderResult, BlenderWriteFault
from rasm.imaging.faults import accumulated, ImagingResult
from rasm.rendering.edits import merge, transfer

# --- [TYPES] ----------------------------------------------------------------------------

type PresetName = Annotated[str, msgspec.Meta(pattern=r"[^\W_]", max_length=64, title="Name", description="Name of the preset and its file")]
type Stock[T] = Mapping[str, T | Path]

# --- [MODELS] ---------------------------------------------------------------------------


class Preset[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Typed compiled recipes shadowed by user-owned JSON files of one record type."""

    key: Key
    form: TypeForm[T]
    stock: Mapping[str, T] = msgspec.field(default_factory=dict)
    directory: Option[Path] = Nothing
    admitted: Option[Callable[[T], ImagingResult[T]]] = Nothing

    # --- [LISTING]

    def folder(self, identity: Identity) -> Path:
        """Returns the store's writable user folder."""
        return self.directory.default_with(lambda: identity.folder / "presets" / self.key)

    def listed(self, identity: Identity) -> Stock[T]:
        """Returns stem-keyed compiled recipes and user paths in case-folded order."""
        rows: dict[str, T | Path] = {**self.stock, **{path.stem: path for path in self.folder(identity).glob("*.json")}}
        return {stem: rows[stem] for stem in sorted(rows, key=lambda stem: (stem.casefold(), stem))}

    def entries(self, identity: Identity, *_: object) -> Iterator[tuple[str, str, str]]:
        """Returns native enum rows with stable stems and source tooltips."""
        return ((stem, bpy.path.display_name(stem), str(value) if isinstance(value, Path) else value.__doc__ or "") for stem, value in self.listed(identity).items())

    def found(self, identity: Identity, stem: str) -> BlenderResult[T | Path]:
        """Returns a compiled recipe or user path, or an absent fault for its stem."""
        found: Option[T | Path] = Option.of_optional(self.listed(identity).get(stem))
        return found.to_result_with(lambda: (BlenderAbsentFault(owner=str(self.folder(identity)), key=stem),))

    # --- [FILES]

    def read(self, source: Path) -> BlenderResult[T]:
        """Decodes a user file, returning read faults or its domain admission faults."""
        try:
            value = msgspec.json.decode(source.read_bytes(), type=self.form)
        except (OSError, msgspec.DecodeError) as error:
            return Error((BlenderReadFault(request=str(source), error=error),))
        return self.admitted.map(lambda admit: admit(value).map_error(lambda faults: (BlenderImagingFault(faults=faults),))).default_value(Ok(value))

    @staticmethod
    def written(target: Path, value: msgspec.Struct) -> BlenderResult[Path]:
        """Writes typed JSON at the requested path, returning that path or a write fault."""
        try:
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(msgspec.json.encode(value, order="deterministic"))
        except OSError as error:
            return Error((BlenderWriteFault(request=str(target), error=error),))
        return Ok(target)

    def saved(self, identity: Identity, name: PresetName, value: T) -> BlenderResult[Path]:
        """Creates one user preset, returning its path, a collision, or a write fault."""
        target = self.folder(identity) / f"{AddPresetBase.as_filename(name)}.json"
        try:
            target.parent.mkdir(parents=True, exist_ok=True)
            with target.open("xb") as destination:
                destination.write(msgspec.json.encode(value, order="deterministic"))
        except FileExistsError:
            return Error((BlenderCollisionFault(request=name, error=str(target)),))
        except OSError as error:
            return Error((BlenderWriteFault(request=str(target), error=error),))
        return Ok(target)

    @staticmethod
    def renamed(source: Path, name: PresetName) -> BlenderResult[Path]:
        """Moves a user preset to an unoccupied name, returning its path or native file faults."""
        target = source.with_stem(AddPresetBase.as_filename(name))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        try:
            move_atomic(str(source), str(target))
        except FileExistsError:
            return Error((BlenderCollisionFault(request=name, error=str(target)),))
        except OSError as error:
            return Error((BlenderWriteFault(request=f"{source} -> {target}", error=error),))
        return Ok(target)

    def imported(self, identity: Identity, source: Path) -> BlenderResult[Path]:
        """Imports a decoded file, returning the user path or read, admission, or write faults."""
        return self.read(source).bind(lambda value: self.written((self.folder(identity) / source.name).with_suffix(".json"), value))

    def exported(self, source: Path, target: Path) -> BlenderResult[Path]:
        """Exports a user file, returning the target or read, admission, or write faults."""
        return self.read(source).bind(lambda value: self.written(target, value))

    @staticmethod
    def removed(source: Path) -> BlenderResult[None]:
        """Removes a user file, returning None or its native write fault."""
        return catch(source.unlink, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=str(source), error=error),))


# --- [OPERATIONS] -----------------------------------------------------------------------


def copied(manager: bpy.types.WindowManager, snapshot: Snapshot) -> None:
    """Copies a typed native snapshot to the system clipboard."""
    manager.clipboard = msgspec.json.encode(snapshot, order="deterministic").decode()


def pasted(manager: bpy.types.WindowManager, owners: tuple[bpy.types.bpy_struct, ...]) -> BlenderResult[tuple[Applied, ...]]:  # type: ignore[type-arg]  # ty: ignore[missing-type-argument]
    """Transfers clipboard values onto each owner's members, returning native outcomes or faults."""
    try:
        clip = msgspec.json.decode(manager.clipboard, type=Snapshot)
    except msgspec.DecodeError as error:
        return Error((BlenderReadFault(request=f"{manager!r}.clipboard", error=error),))
    return accumulated(
        Snapshot
        .capture(owner)
        .map(lambda target: Snapshot(struct=clip.struct, tree=merge(target.tree, transfer(target.tree, clip.tree))).apply(owner))
        .bind(lambda applied: applied.outcome.map(lambda _: applied))
        for owner in owners
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Preset", "PresetName", "Stock", "copied", "pasted"]
