"""Typed presets over the host preset paths: stock records, stock JSON files under a registered preset path, and user JSON files uninstall leaves."""

from collections.abc import Callable, Mapping
from contextlib import AbstractContextManager
from functools import partial
import importlib.resources
from pathlib import Path
from types import MappingProxyType
from typing import Annotated, Final, overload

from bl_operators.presets import AddPresetBase
import bpy
from expression import Option
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import _scoped, Resource
from rasm_bpy.data._rna.properties import choices, EnumItem, IconedItem
from rasm_records import Fault, no_clobber_move, Nothing, Ok, Outcome, Reason, RecordFile

# --- [TYPES] ----------------------------------------------------------------------------

type _PresetName = Annotated[str, msgspec.Meta(pattern=r"[^\W_]")]

# --- [MODELS] ---------------------------------------------------------------------------


class Preset[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Record type presets under `presets/<extension id>/<key>` of every host preset path, user files overriding stock by stem."""

    # --- [STATE]

    key: Key
    form: TypeForm[T]
    stock: Mapping[str, T] = MappingProxyType({})
    validated: Option[Callable[[T], Outcome[T]]] = Nothing

    # --- [LISTINGS]

    def _subdirectory(self, identity: Identity) -> str:
        """Returns the preset subdirectory every host preset path holds for this key."""
        return f"{identity.manifest.id}/{self.key}"

    def folder(self, identity: Identity) -> Path:
        """Returns the user preset folder under the user scripts folder, which uninstall leaves."""
        return Path(bpy.utils.user_resource("SCRIPTS", path=f"presets/{self._subdirectory(identity)}"))

    def owned(self, identity: Identity, source: T | Path) -> bool:
        """Whether source is a user file, which edits may replace, rename, or delete."""
        return isinstance(source, Path) and source.parent == self.folder(identity)

    def listed(self, identity: Identity) -> Mapping[str, T | Path]:
        """Returns stock records in declared order, then files by case-folded stem, a file taking the place of a record of its stem and a user file the place of a stock file."""
        user = self.folder(identity)
        files = {path.stem: path for folder in sorted(map(Path, bpy.utils.preset_paths(self._subdirectory(identity))), key=lambda folder: folder == user) for path in sorted(folder.glob("*.json"))}
        added = sorted(files.keys() - self.stock.keys(), key=lambda stem: (stem.casefold(), stem))
        return {**{stem: files.get(stem, value) for stem, value in self.stock.items()}, **{stem: files[stem] for stem in added}}

    @staticmethod
    def titled(stem: str, source: msgspec.Struct | Path) -> str:
        """Returns the name a preset reads under, stock records title-cased and files keeping their typed name."""
        return bpy.path.display_name(stem, title_case=not isinstance(source, Path))

    @staticmethod
    def stem_of(name: str) -> str:
        """Returns the file stem the host gives a preset of name."""
        return str(AddPresetBase.as_filename(name))

    @staticmethod
    @overload
    def entries(sources: Mapping[str, msgspec.Struct | Path]) -> tuple[EnumItem, ...]: ...
    @staticmethod
    @overload
    def entries(sources: Mapping[str, msgspec.Struct | Path], *, prefix: tuple[EnumItem | IconedItem, ...]) -> tuple[EnumItem | IconedItem, ...]: ...
    @staticmethod
    def entries(sources: Mapping[str, msgspec.Struct | Path], *, prefix: tuple[EnumItem | IconedItem, ...] = ()) -> tuple[EnumItem | IconedItem, ...]:
        """Returns enum items with a value fixed per stem after supplied native prefix items, files taking their path as tooltip."""
        return choices((*prefix, *((stem, Preset.titled(stem, value), str(value) if isinstance(value, Path) else "") for stem, value in sources.items())))

    def found(self, identity: Identity, stem: str) -> Outcome[T | Path]:
        """Returns stock preset or file of stem."""
        found: Option[T | Path] = Option.of_optional(self.listed(identity).get(stem))
        return found.to_result_with(lambda: Fault.of(Reason.ABSENT, str(self.folder(identity)), member=stem))

    def loaded(self, identity: Identity, stem: str) -> Outcome[T]:
        """Returns the validated preset of stem, stock or file."""
        return self.found(identity, stem).bind(self.read)

    # --- [FILES]

    def read(self, source: T | Path) -> Outcome[T]:
        """Decodes and validates a file, passing a stock record through."""
        return (
            RecordFile(path=source, form=self.form).read().bind(lambda value: self.validated.map(lambda validate: validate(value)).default_value(Ok(value))) if isinstance(source, Path) else Ok(source)
        )

    def written(self, target: Path, value: T) -> Outcome[Path]:
        """Writes JSON at target through one atomic commit."""
        return RecordFile(path=target, form=self.form).written(value)

    def saved(self, identity: Identity, name: str, value: T) -> Outcome[Path]:
        """Creates user preset file under a valid preset name, refusing a name a user file holds, a stock file of that stem staying overridable."""
        return _named(name).bind(lambda stem: RecordFile(path=self.folder(identity) / f"{stem}.json", form=self.form).written(value, place=no_clobber_move))

    @staticmethod
    def renamed(source: Path, name: str) -> Outcome[Path]:
        """Moves user preset to a free, valid preset name, refusing a name a user file holds, a stock file of that stem staying overridable."""

        @catch(exception=OSError)
        def moved(stem: str) -> Path:
            target = source.with_stem(stem)
            no_clobber_move(source, target)
            return target

        return _named(name).bind(lambda stem: moved(stem).map_error(partial(Fault.of, Reason.UNWRITABLE, str(source))))

    def imported(self, identity: Identity, source: Path) -> Outcome[Path]:
        """Copies validated file into user folder."""
        return self.exported(source, (self.folder(identity) / source.name).with_suffix(".json"))

    def exported(self, source: T | Path, target: Path) -> Outcome[Path]:
        """Writes validated file or stock preset to target."""
        return self.read(source).bind(partial(self.written, target))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _named(name: str) -> Outcome[str]:
    """Returns the file stem of a name validated as a preset name, invalid fault naming the bound it misses."""
    return catch(msgspec.convert, exception=msgspec.ValidationError)(name, _PresetName).map(Preset.stem_of).map_error(partial(Fault.of, Reason.INVALID, name))


def _registered(identity: Identity) -> AbstractContextManager[Outcome[None]]:
    """Holds the extension package folder as a host preset path, its `presets/<extension id>/<key>` folders holding stock files."""
    folder = str(importlib.resources.files(identity.package))
    return _scoped(folder, partial(bpy.utils.register_preset_path, folder), lambda _: bpy.utils.unregister_preset_path(folder))


# --- [COMPOSITION] ----------------------------------------------------------------------

_PRESET_PATH: Final = Resource[Outcome[None]](made=_registered)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Preset"]
