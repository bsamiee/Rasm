"""Typed stock presets, user preset files, and snapshot clipboard transfer."""

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

from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderImagingFault, BlenderReadFault, BlenderResult, BlenderWriteFault
from blender.imaging.faults import accumulated, ImagingResult
from blender.rendering.edits import merge, transfer

# --- [TYPES] ----------------------------------------------------------------------------

type PresetName = Annotated[str, msgspec.Meta(pattern=r"[^\W_]", max_length=64, description="Preset and file name")]
type Stock[T] = Mapping[str, T | Path]

# --- [MODELS] ---------------------------------------------------------------------------


class Preset[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Record type presets, user JSON files overriding stock by stem."""

    key: Key
    form: TypeForm[T]
    stock: Mapping[str, T] = msgspec.field(default_factory=dict)
    directory: Option[Path] = Nothing
    admitted: Option[Callable[[T], ImagingResult[T]]] = Nothing

    # --- [LISTING]

    def folder(self, identity: Identity) -> Path:
        """Returns user preset folder."""
        return self.directory.default_with(lambda: identity.folder / "presets" / self.key)

    def listed(self, identity: Identity) -> Stock[T]:
        """Returns presets and user paths by case-folded stem."""
        rows: dict[str, T | Path] = {**self.stock, **{path.stem: path for path in self.folder(identity).glob("*.json")}}
        return {stem: rows[stem] for stem in sorted(rows, key=lambda stem: (stem.casefold(), stem))}

    def entries(self, identity: Identity, *_: object) -> Iterator[tuple[str, str, str]]:
        """Returns enum rows with path or docstring tooltips."""
        return ((stem, bpy.path.display_name(stem), str(value) if isinstance(value, Path) else value.__doc__ or "") for stem, value in self.listed(identity).items())

    def found(self, identity: Identity, stem: str) -> BlenderResult[T | Path]:
        """Returns stock preset or user path of stem."""
        found: Option[T | Path] = Option.of_optional(self.listed(identity).get(stem))
        return found.to_result_with(lambda: (BlenderAbsentFault(owner=str(self.folder(identity)), key=stem),))

    # --- [FILES]

    def read(self, source: T | Path) -> BlenderResult[T]:
        """Decodes and validates user file, passing stock preset through."""
        if not isinstance(source, Path):
            return Ok(source)
        try:
            value = msgspec.json.decode(source.read_bytes(), type=self.form)
        except (OSError, msgspec.DecodeError) as error:
            return Error((BlenderReadFault(request=source, error=error),))
        return self.admitted.map(lambda admit: admit(value).map_error(BlenderImagingFault.lifted)).default_value(Ok(value))

    @staticmethod
    def written(target: Path, value: msgspec.Struct) -> BlenderResult[Path]:
        """Writes JSON at target."""
        try:
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(msgspec.json.encode(value, order="deterministic"))
        except OSError as error:
            return Error((BlenderWriteFault(request=target, error=error),))
        return Ok(target)

    def saved(self, identity: Identity, name: PresetName, value: T) -> BlenderResult[Path]:
        """Creates user preset file, `collision` fault when it exists."""
        target = self.folder(identity) / f"{AddPresetBase.as_filename(name)}.json"
        try:
            target.parent.mkdir(parents=True, exist_ok=True)
            with target.open("xb") as destination:
                destination.write(msgspec.json.encode(value, order="deterministic"))
        except FileExistsError:
            return Error((BlenderCollisionFault(request=name, error=target),))
        except OSError as error:
            return Error((BlenderWriteFault(request=target, error=error),))
        return Ok(target)

    @staticmethod
    def renamed(source: Path, name: PresetName) -> BlenderResult[Path]:
        """Moves user preset to a free name."""
        target = source.with_stem(AddPresetBase.as_filename(name))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        try:
            move_atomic(str(source), str(target))
        except FileExistsError:
            return Error((BlenderCollisionFault(request=name, error=target),))
        except OSError as error:
            return Error((BlenderWriteFault(request=f"{source} -> {target}", error=error),))
        return Ok(target)

    def imported(self, identity: Identity, source: Path) -> BlenderResult[Path]:
        """Copies validated file into user folder."""
        return self.read(source).bind(lambda value: self.written((self.folder(identity) / source.name).with_suffix(".json"), value))

    def exported(self, source: Path, target: Path) -> BlenderResult[Path]:
        """Writes validated user file to target."""
        return self.read(source).bind(lambda value: self.written(target, value))


# --- [OPERATIONS] -----------------------------------------------------------------------


def removed(source: Path) -> BlenderResult[None]:
    """Deletes file."""
    return catch(source.unlink, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=source, error=error),))


def copied(manager: bpy.types.WindowManager, snapshot: Snapshot) -> None:
    """Copies snapshot to system clipboard as JSON."""
    manager.clipboard = msgspec.json.encode(snapshot, order="deterministic").decode()


def pasted(manager: bpy.types.WindowManager, owners: "tuple[bpy.types.bpy_struct[object], ...]") -> BlenderResult[tuple[Applied, ...]]:
    """Applies clipboard snapshot to each owner."""
    try:
        clip = msgspec.json.decode(manager.clipboard, type=Snapshot)
    except msgspec.DecodeError as error:
        return Error((BlenderReadFault(request=f"{manager!r}.clipboard", error=error),))
    return accumulated(
        Snapshot
        .capture(owner)
        .map(lambda target: msgspec.structs.replace(clip, tree=merge(target.tree, transfer(target.tree, clip.tree))).apply(owner))
        .bind(lambda applied: applied.outcome.map(lambda _: applied))
        for owner in owners
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Preset", "PresetName", "Stock", "copied", "pasted", "removed"]
