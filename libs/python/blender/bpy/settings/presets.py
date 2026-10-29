"""Typed stock presets and user preset files."""

from collections.abc import Callable, Mapping
from functools import partial
from pathlib import Path
from types import MappingProxyType
from typing import Annotated

from atomicwrites import move_atomic
from bl_operators.presets import AddPresetBase
import bpy
from expression import Nothing, Ok, Option
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from blender.bpy.data.rna.mirrors import held_choices
from blender.bpy.extension.identity import Identity, Key
from blender.records.faults import Fault, Outcome, Reason
from blender.records.stores import RecordFile

# --- [TYPES] ----------------------------------------------------------------------------

type PresetName = Annotated[str, msgspec.Meta(pattern=r"[^\W_]")]

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
        return held_choices(
            tuple(
                (stem, bpy.path.display_name(stem, title_case=False), str(value)) if isinstance(value, Path) else (stem, bpy.path.display_name(stem), "")
                for stem, value in self.listed(identity).items()
            )
        )

    def found(self, identity: Identity, stem: str) -> Outcome[T | Path]:
        """Returns stock preset or user path of stem."""
        found: Option[T | Path] = Option.of_optional(self.listed(identity).get(stem))
        return found.to_result_with(lambda: Fault.of(Reason.ABSENT, str(self.folder(identity)), member=stem))

    def loaded(self, identity: Identity, stem: str) -> Outcome[T]:
        """Returns the validated preset of stem, stock or user."""
        return self.found(identity, stem).bind(self.read)

    # --- [FILES]

    def read(self, source: T | Path) -> Outcome[T]:
        """Decodes and validates user file, passing stock preset through."""
        return RecordFile(path=source, form=self.form).read().bind(lambda value: self.admitted.map(lambda admit: admit(value)).default_value(Ok(value))) if isinstance(source, Path) else Ok(source)

    def written(self, target: Path, value: T) -> Outcome[Path]:
        """Writes JSON at target through one atomic commit."""
        return RecordFile(path=target, form=self.form).written(value)

    def saved(self, identity: Identity, name: str, value: T) -> Outcome[Path]:
        """Creates user preset file under a valid preset name, refusing a name a file holds."""
        return _named(name).bind(lambda valid: RecordFile(path=self.folder(identity) / f"{AddPresetBase.as_filename(valid)}.json", form=self.form).written(value, exclusive=True))

    @staticmethod
    def renamed(source: Path, name: str) -> Outcome[Path]:
        """Moves user preset to a free, valid preset name, refusing a name a file holds."""

        @catch(exception=OSError)
        def moved(valid: PresetName) -> Path:
            target = source.with_stem(AddPresetBase.as_filename(valid))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            move_atomic(str(source), str(target))
            return target

        return _named(name).bind(lambda valid: moved(valid).map_error(partial(Fault.of, Reason.UNWRITABLE, str(source))))

    def imported(self, identity: Identity, source: Path) -> Outcome[Path]:
        """Copies validated file into user folder."""
        return self.exported(source, (self.folder(identity) / source.name).with_suffix(".json"))

    def exported(self, source: T | Path, target: Path) -> Outcome[Path]:
        """Writes validated user file or stock preset to target."""
        return self.read(source).bind(partial(self.written, target))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _named(name: str) -> Outcome[PresetName]:
    """Returns name validated as a preset name, invalid fault naming the bound it misses."""
    return catch(msgspec.convert, exception=msgspec.ValidationError)(name, PresetName).map_error(partial(Fault.of, Reason.INVALID, name))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Preset", "PresetName"]
