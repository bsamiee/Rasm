"""Typed stock presets and user preset files."""

from collections.abc import Callable, Mapping
from functools import cache, partial
from pathlib import Path
from types import MappingProxyType
from typing import Annotated, Final

from bl_operators.presets import AddPresetBase
import bpy
from expression import Nothing, Ok, Option, Result
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from blender.bpy.data.rna.mirrors import choices
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome, RecordReason
from blender.records.stores import committed, RecordFile, removed

# --- [TYPES] ----------------------------------------------------------------------------

type PresetName = Annotated[str, msgspec.Meta(pattern=r"[^\W_]")]

# --- [CONSTANTS] ------------------------------------------------------------------------

_CHOICES: Final = cache(choices)

# --- [MODELS] ---------------------------------------------------------------------------


class Preset[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Record type presets, user JSON files overriding stock by stem."""

    key: Key
    form: TypeForm[T]
    stock: Mapping[str, T] = MappingProxyType({})
    directory: Option[Path] = Nothing
    admitted: Option[Callable[[T], Outcome[T]]] = Nothing

    # --- [LISTING]

    def folder(self, identity: Identity) -> Path:
        """Returns user preset folder."""
        return self.directory.default_with(lambda: identity.folder / "presets" / self.key)

    def listed(self, identity: Identity) -> Mapping[str, T | Path]:
        """Returns stock presets in declared order, then user paths by case-folded stem, a user file taking its stock stem's place."""
        users = {path.stem: path for path in self.folder(identity).glob("*.json")}
        added = sorted(users.keys() - self.stock.keys(), key=lambda stem: (stem.casefold(), stem))
        return {**{stem: users.get(stem, value) for stem, value in self.stock.items()}, **{stem: users[stem] for stem in added}}

    def entries(self, identity: Identity, *_: object) -> tuple[tuple[str, str, str, str, int], ...]:
        """Returns enum items with a value fixed per stem, held while the listing stands so the strings Blender reads stay referenced.

        Stock stems read title-cased, user files keep their typed name and take their path as tooltip.
        """
        return _CHOICES(
            tuple(
                (stem, bpy.path.display_name(stem, title_case=False), str(value)) if isinstance(value, Path) else (stem, bpy.path.display_name(stem), "")
                for stem, value in self.listed(identity).items()
            )
        )

    def found(self, identity: Identity, stem: str) -> Outcome[T | Path]:
        """Returns stock preset or user path of stem."""
        found: Option[T | Path] = Option.of_optional(self.listed(identity).get(stem))
        return found.to_result_with(lambda: Fault.of(BlenderReason.ABSENT, str(self.folder(identity)), member=stem))

    def loaded(self, identity: Identity, stem: str) -> Outcome[T]:
        """Returns the validated preset of stem, stock or user."""
        return self.found(identity, stem).bind(self.read)

    # --- [FILES]

    def _file(self, path: Path) -> RecordFile[T]:
        """Returns record file of preset form at path."""
        return RecordFile(path=path, form=self.form)

    def read(self, source: T | Path) -> Outcome[T]:
        """Decodes and validates user file, passing stock preset through."""
        return Result[T, Faults].bind(self._file(source).read(), lambda value: self.admitted.map(lambda admit: admit(value)).default_value(Ok(value))) if isinstance(source, Path) else Ok(source)

    def written(self, target: Path, value: T) -> Outcome[Path]:
        """Writes JSON at target through one atomic commit."""
        return self._file(target).written(value)

    def saved(self, identity: Identity, name: str, value: T) -> Outcome[Path]:
        """Creates user preset file under a valid preset name, refusing a name a file holds."""
        return _named(name).bind(lambda valid: self._file(self.folder(identity) / f"{AddPresetBase.as_filename(valid)}.json").written(value, exclusive=True))

    @staticmethod
    def renamed(source: Path, name: str) -> Outcome[Path]:
        """Moves user preset to a free, valid preset name."""

        def moved(valid: PresetName) -> Outcome[Path]:
            target = source.with_stem(AddPresetBase.as_filename(valid))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            copied = catch(source.read_bytes, exception=OSError)().map_error(partial(Fault.of, RecordReason.UNREADABLE, str(source))).bind(lambda data: committed(target, data, exclusive=True))
            return copied.bind(lambda _: removed(source)).map(lambda _: target)

        return _named(name).bind(moved)

    def imported(self, identity: Identity, source: Path) -> Outcome[Path]:
        """Copies validated file into user folder."""
        return self.read(source).bind(lambda value: self.written((self.folder(identity) / source.name).with_suffix(".json"), value))

    def exported(self, source: Path, target: Path) -> Outcome[Path]:
        """Writes validated user file to target."""
        return self.read(source).bind(lambda value: self.written(target, value))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _named(name: str) -> Outcome[PresetName]:
    """Returns name validated as a preset name, invalid fault naming the bound it misses."""
    return catch(msgspec.convert, exception=msgspec.ValidationError)(name, PresetName).map_error(partial(Fault.of, RecordReason.INVALID, name))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Preset", "PresetName"]
