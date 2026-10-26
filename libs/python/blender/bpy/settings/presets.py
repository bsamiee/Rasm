"""Typed stock presets, user preset files, and snapshot clipboard transfer."""

from collections.abc import Callable, Iterator, Mapping
from itertools import repeat
from pathlib import Path
from typing import Annotated, Final

from atomicwrites import move_atomic
from bl_operators.presets import AddPresetBase
import bpy
import colour
from expression import Error, Nothing, Ok, Option, Result
from expression.extra.result import catch
import msgspec
import msgspec.inspect as native
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from typing_extensions import TypeForm

from blender.bpy.data.rna.edits import merge, transfer
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderFaults, BlenderResult
from blender.bpy.settings.properties import Unit
from blender.imaging.color.processors import basis
from blender.imaging.faults import Access, accumulated, ImagingFileFault, ImagingResult
from blender.imaging.files.records import Record
from blender.imaging.parameters import Rgb

# --- [TYPES] ----------------------------------------------------------------------------

type PresetName = Annotated[str, msgspec.Meta(pattern=r"[^\W_]", max_length=64, description="Preset and file name")]
type Stock[T] = Mapping[str, T | Path]

# --- [CONSTANTS] ------------------------------------------------------------------------

_SWITCH: Final = 0.5

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

    def file(self, path: Path) -> Record[T]:
        """Returns record file of preset form at path."""
        return Record(path=path, form=self.form)

    def read(self, source: T | Path) -> BlenderResult[T]:
        """Decodes and validates user file, passing stock preset through."""
        return self.file(source).read().bind(lambda value: self.admitted.map(lambda admit: admit(value)).default_value(Ok(value))) if isinstance(source, Path) else Ok(source)

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
        try:
            move_atomic(str(source), str(target))
        except FileExistsError:
            return Error((BlenderCollisionFault(request=name, error=target),))
        except OSError as error:
            return Error((ImagingFileFault(access=Access.WRITE, request=f"{source} -> {target}", error=error),))
        return Ok(target)

    def imported(self, identity: Identity, source: Path) -> BlenderResult[Path]:
        """Copies validated file into user folder."""
        return self.read(source).bind(lambda value: self.written((self.folder(identity) / source.name).with_suffix(".json"), value))

    def exported(self, source: Path, target: Path) -> BlenderResult[Path]:
        """Writes validated user file to target."""
        return self.read(source).bind(lambda value: self.written(target, value))

    # --- [STRENGTH]

    @staticmethod
    def blended[S: msgspec.Struct](rest: S, preset: S, amount: float, config: PyOpenColorIO.Config) -> BlenderResult[S]:
        """Moves each declared field of `rest` toward `preset` by `amount`, 1 reaching the preset.

        Numbers move linearly within their declared bounds as host storage holds them, integers rounded; `Rgb` values move in Oklab over the
        scene-linear XYZ basis of `config`; nested records and equal-length tuples move per member; every other value
        takes the preset above half.

        Args:
            rest: Record at amount 0.
            preset: Record at amount 1.
            amount: Share of the move, past 1 extrapolating.
            config: Config whose scene-linear role carries `Rgb` values.

        Returns:
            Moved record, or the base fault of a scene-linear space without an XYZ matrix.
        """
        match native.type_info(type(preset)):
            case native.StructType() as kind:
                return _record(kind, rest, preset, amount, config)
            case _:
                return Ok(preset if amount > _SWITCH else rest)


# --- [OPERATIONS] -----------------------------------------------------------------------


def removed(source: Path) -> BlenderResult[None]:
    """Deletes file."""
    return catch(source.unlink, exception=OSError)().map_error(lambda error: (ImagingFileFault(access=Access.WRITE, request=source, error=error),))


def copied(manager: bpy.types.WindowManager, snapshot: Snapshot) -> None:
    """Copies snapshot to system clipboard as JSON."""
    manager.clipboard = msgspec.json.encode(snapshot, order="deterministic").decode()


def pasted(manager: bpy.types.WindowManager, owners: "tuple[bpy.types.bpy_struct[object], ...]") -> BlenderResult[tuple[Applied, ...]]:
    """Applies clipboard snapshot to each owner."""
    try:
        clip = msgspec.json.decode(manager.clipboard, type=Snapshot)
    except msgspec.DecodeError as error:
        return Error((ImagingFileFault(access=Access.READ, request=f"{manager!r}.clipboard", error=error),))
    return accumulated(
        Snapshot
        .capture(owner)
        .map(lambda target: msgspec.structs.replace(clip, tree=merge(target.tree, transfer(target.tree, clip.tree))).apply(owner))
        .bind(lambda applied: applied.outcome.map(lambda _: applied))
        for owner in owners
    )


def _record[S: msgspec.Struct](kind: native.StructType, rest: msgspec.Struct, preset: S, amount: float, config: PyOpenColorIO.Config) -> BlenderResult[S]:
    """Moves every field of one record pair."""
    moved = accumulated(_moved(field.type, getattr(rest, field.name), getattr(preset, field.name), amount, config) for field in kind.fields)
    return moved.map(lambda values: msgspec.structs.replace(preset, **{field.name: value for field, value in zip(kind.fields, values)}))


def _moved(kind: native.Type, rest: object, preset: object, amount: float, config: PyOpenColorIO.Config) -> BlenderResult[object]:
    """Moves one value pair by its declared kind, generic and union members by their runtime kind."""

    def held(value: float, declared: native.Type) -> BlenderResult[float]:
        return Unit().bounds(declared).map(lambda ends: float(np.clip(value, *ends)))

    def oklab(fields: tuple[native.Field, ...], start: Rgb[float], end: Rgb[float], matrix: npt.NDArray[np.float64]) -> BlenderResult[Rgb[float]]:
        ends = colour.XYZ_to_Oklab(np.stack((np.asarray(start), np.asarray(end))) @ matrix.T)
        values = np.linalg.solve(matrix, colour.Oklab_to_XYZ(ends[0] + amount * (ends[1] - ends[0])))
        return accumulated(held(float(value), field.type) for field, value in zip(fields, values)).map(
            lambda channels: msgspec.structs.replace(end, **{field.name: channel for field, channel in zip(fields, channels)})
        )

    match kind, rest, preset:
        case _ if rest == preset:
            return Ok(preset)
        case native.Metadata(type=inner), _, _:
            return _moved(inner, rest, preset, amount, config)
        case native.StructType(fields=fields), Rgb() as start, Rgb() as end:
            return Result[npt.NDArray[np.float64], BlenderFaults].bind(basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR), lambda matrix: oklab(fields, start, end, matrix))
        case native.StructType(), msgspec.Struct(), msgspec.Struct() if type(rest) is type(preset):
            return _record(kind, rest, preset, amount, config)
        case native.TupleType(item_types=items), tuple(), tuple() if len(rest) == len(preset):
            return accumulated(_moved(*each, amount, config) for each in zip(items, rest, preset)).map(tuple)
        case native.VarTupleType(item_type=item), tuple(), tuple() if len(rest) == len(preset):
            return accumulated(_moved(*each, amount, config) for each in zip(repeat(item), rest, preset)).map(tuple)
        case native.FloatType(), float() | int(), float() | int():
            return held(rest + amount * (preset - rest), kind)
        case native.IntType(), int(), int():
            return Ok(int(np.clip(round(rest + amount * (preset - rest)), *Unit.integral(kind))))
        case native.UnionType() | native.AnyType(), _, _ if type(rest) is type(preset):
            return _moved(native.type_info(type(preset)), rest, preset, amount, config)
        case _:
            return Ok(preset if amount > _SWITCH else rest)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Preset", "PresetName", "Stock", "copied", "pasted", "removed"]
