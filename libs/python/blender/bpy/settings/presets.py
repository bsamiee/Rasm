"""Typed stock presets, user preset files, and snapshot clipboard transfer."""

from collections.abc import Callable, Iterator, Mapping
from pathlib import Path
import shutil
from typing import Annotated

from bl_operators.presets import AddPresetBase
import bpy
from expression import Error, Nothing, Ok, Option, Result
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.imaging.faults import ImagingResult
from blender.records.faults import accumulated, FileFault
from blender.records.stores import committed, RecordFile
from blender.records.trees import merge, transfer

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
        """Returns stock presets in declared order, then user paths by case-folded stem, a user file taking its stock stem's place."""
        users = {path.stem: path for path in self.folder(identity).glob("*.json")}
        added = sorted(users.keys() - self.stock.keys(), key=lambda stem: (stem.casefold(), stem))
        return {**{stem: users.get(stem, value) for stem, value in self.stock.items()}, **{stem: users[stem] for stem in added}}

    def entries(self, identity: Identity, *_: object) -> Iterator[tuple[str, str, str]]:
        """Returns enum rows, user files tooltipped by their path."""
        return ((stem, bpy.path.display_name(stem), str(value) if isinstance(value, Path) else "") for stem, value in self.listed(identity).items())

    def found(self, identity: Identity, stem: str) -> BlenderResult[T | Path]:
        """Returns stock preset or user path of stem."""
        found: Option[T | Path] = Option.of_optional(self.listed(identity).get(stem))
        return found.to_result_with(lambda: (BlenderAbsentFault(owner=str(self.folder(identity)), key=stem),))

    # --- [FILES]

    def file(self, path: Path) -> RecordFile[T]:
        """Returns record file of preset form at path."""
        return RecordFile(path=path, form=self.form)

    def read(self, source: T | Path) -> BlenderResult[T]:
        """Decodes and validates user file, passing stock preset through."""
        return Result[T, BlenderFaults].bind(self.file(source).read(), lambda value: self.admitted.map(lambda admit: admit(value)).default_value(Ok(value))) if isinstance(source, Path) else Ok(source)

    def written(self, target: Path, value: T) -> BlenderResult[Path]:
        """Writes JSON at target through one atomic commit."""
        return self.file(target).written(value)

    def saved(self, identity: Identity, name: PresetName, value: T) -> BlenderResult[Path]:
        """Creates user preset file, refusing a name a file holds."""
        return self.file(self.folder(identity) / f"{AddPresetBase.as_filename(name)}.json").written(value, exclusive=True)

    @staticmethod
    def renamed(source: Path, name: PresetName) -> BlenderResult[Path]:
        """Moves user preset to a free name."""
        target = source.with_stem(AddPresetBase.as_filename(name))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        copied = committed(
            target, lambda stage: catch(shutil.copyfile, exception=OSError)(source, stage).map_error(lambda error: (FileFault(access="read", request=source, error=error),)), exclusive=True
        )
        return Result[Path, BlenderFaults].bind(copied, lambda _: removed(source)).map(lambda _: target)

    def imported(self, identity: Identity, source: Path) -> BlenderResult[Path]:
        """Copies validated file into user folder."""
        return self.read(source).bind(lambda value: self.written((self.folder(identity) / source.name).with_suffix(".json"), value))

    def exported(self, source: Path, target: Path) -> BlenderResult[Path]:
        """Writes validated user file to target."""
        return self.read(source).bind(lambda value: self.written(target, value))


# --- [OPERATIONS] -----------------------------------------------------------------------


def removed(source: Path) -> BlenderResult[None]:
    """Deletes file."""
    return catch(source.unlink, exception=OSError)().map_error(lambda error: (FileFault(access="write", request=source, error=error),))


def copied(manager: bpy.types.WindowManager, snapshot: Snapshot) -> None:
    """Copies snapshot to system clipboard as JSON."""
    manager.clipboard = msgspec.json.encode(snapshot, order="deterministic").decode()


def pasted(manager: bpy.types.WindowManager, owners: "tuple[bpy.types.bpy_struct[object], ...]") -> BlenderResult[tuple[Applied, ...]]:
    """Applies clipboard snapshot to each owner."""
    try:
        clip = msgspec.json.decode(manager.clipboard, type=Snapshot)
    except msgspec.DecodeError as error:
        return Error((FileFault(access="read", request=f"{manager!r}.clipboard", error=error),))
    return accumulated(
        Snapshot
        .capture(owner)
        .map(lambda target: msgspec.structs.replace(clip, tree=merge(target.tree, transfer(target.tree, clip.tree))).apply(owner))
        .bind(lambda applied: applied.outcome.map(lambda _: applied))
        for owner in owners
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Preset", "PresetName", "Stock", "copied", "pasted", "removed"]
