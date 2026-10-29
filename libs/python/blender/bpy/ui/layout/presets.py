"""Preset catalogs applying, saving, importing, exporting, replacing, renaming, deleting, and stepping through presets, with the store and preset operator verbs."""

from collections.abc import Callable, Mapping
from enum import auto, Enum, unique
from functools import cached_property, partial
from pathlib import Path
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

from bl_operators.presets import AddPresetBase
import bpy
from expression import Error, Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
import msgspec

from blender.bpy.commands import invocations, operators
from blender.bpy.commands.calls import Command, Placement
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import rna, RnaPath, rooted, Struct
from blender.bpy.data.rna.scopes import Scoped
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import alerted, Keywords, Line, OperatorButton, pressed, realized, RowDraw
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.stores import removed

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import IconItems

# --- [TYPES] ----------------------------------------------------------------------------


@unique
class Verb(Enum):
    """Store and preset operator verb drawn under the icon its operators share, each keyed `<verb>_<stem>`."""

    RESET = auto()
    COPY = auto()
    PASTE = auto()
    RESTORE = auto()
    FORGET = auto()
    LOCK = auto()
    APPLY = auto()
    STEP = auto()
    SAVE = auto()
    DELETE = auto()
    IMPORT = auto()
    EXPORT = auto()
    REPLACE = auto()
    RENAME = auto()

    @property
    def icon(self) -> "IconItems":
        """Icon of the verb's operators and buttons."""
        return _ICONS[self]

    def command[T: bpy.types.PropertyGroup](
        self,
        stem: Key,
        form: type[T],
        run: Callable[[operators.Call[T]], Outcome[operators.Answer]],
        invoke: Option[operators.Invoke[T, None]] = Nothing,
        seed: Option[Callable[[T, bpy.types.Event, Identity], Mapping[str, Value]]] = Nothing,
    ) -> operators.Operator[T]:
        """Builds the internal verb operator over a store or preset stem, one undo step per run unless the verb writes only outside the blend file."""
        undo: Option[Option[str]] = Nothing if self in _OUTSIDE_BLEND else Some(Nothing)
        return operators.Operator(icon=Some(self.icon), internal=True, form=form, run=run, undo=undo, invoke=invoke, seed=seed, named=Some(f"{self.name.lower()}_{stem}"))


# --- [CONSTANTS] ------------------------------------------------------------------------

_ICONS: Final[Mapping[Verb, "IconItems"]] = MappingProxyType({
    Verb.RESET: "LOOP_BACK",
    Verb.COPY: "COPYDOWN",
    Verb.PASTE: "PASTEDOWN",
    Verb.RESTORE: "RECOVER_LAST",
    Verb.FORGET: "X",
    Verb.LOCK: "LOCKED",
    Verb.APPLY: "PRESET",
    Verb.STEP: "FORWARD",
    Verb.SAVE: "ADD",
    Verb.DELETE: "X",
    Verb.IMPORT: "IMPORT",
    Verb.EXPORT: "EXPORT",
    Verb.REPLACE: "FILE_REFRESH",
    Verb.RENAME: "GREASEPENCIL",
})
_OUTSIDE_BLEND: Final = frozenset({Verb.COPY, Verb.SAVE, Verb.DELETE, Verb.IMPORT, Verb.EXPORT, Verb.REPLACE, Verb.RENAME})
_AS_FILENAME: Final[Callable[[str], str]] = AddPresetBase.as_filename  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_NAME: Final = AddPresetBase.__annotations__["name"]

# --- [MODELS] ---------------------------------------------------------------------------


class Offset(bpy.types.PropertyGroup):
    """Signed step a restore, forget, or preset step button names."""

    offset: bpy.props.IntProperty(name="Offset", description="Signed step count", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class _Named(bpy.types.PropertyGroup):
    """Preset an apply, delete, export, replace, or rename button names."""

    stem: bpy.props.StringProperty(name="Preset", description="Preset file stem", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class _Renamed(_Named):
    """Saved preset and its new name."""

    name: _NAME  # type: ignore[valid-type]


class _Saved(bpy.types.PropertyGroup):
    """Name entered in the save-preset dialog."""

    name: _NAME  # type: ignore[valid-type]


class Listing[T: msgspec.Struct](Line, frozen=True, kw_only=True, dict=True):
    """Preset catalog applying, saving, importing, exporting, replacing, renaming, deleting, and stepping through presets of the current values."""

    preset: Preset[T]
    current: Callable[[bpy.types.Context], Outcome[T]]
    matches: Callable[[bpy.types.Context, tuple[T, ...]], Outcome[tuple[bool, ...]]]
    applied: Callable[[bpy.types.Context, T], Outcome[operators.Answer]]

    # --- [NAMING]

    @property
    def key(self) -> Key:
        """Listing menu key an `Opens` row opens."""
        return menu_key(self.preset)

    # --- [COMMANDS]

    @cached_property
    def commands(self) -> Mapping[Verb, Command]:
        """Verb operators over the preset stem, preset files filtered as JSON and the rename dialog seeded with the preset's typed name."""
        stem = self.preset.key

        def json(_properties: object) -> tuple[str, *tuple[str, ...]]:
            return (".json",)

        def renaming(properties: _Renamed, _event: bpy.types.Event, _identity: Identity) -> Mapping[str, Value]:
            return {"name": bpy.path.display_name(properties.stem, title_case=False)}

        return MappingProxyType({
            Verb.APPLY: Verb.APPLY.command(stem, _Named, self.apply_preset),
            Verb.STEP: Verb.STEP.command(stem, Offset, self.step_preset),
            Verb.SAVE: Verb.SAVE.command(stem, _Saved, self.save_preset, Some(invocations.Popup(partial(bpy.types.WindowManager.invoke_props_dialog)))),
            Verb.DELETE: Verb.DELETE.command(
                stem,
                _Named,
                self.delete_preset,
                Some(invocations.Popup[_Named](partial(bpy.types.WindowManager.invoke_confirm, confirm_text="Remove", icon="WARNING"), message=Some(partial(_asked, "Remove {}")))),
            ),
            Verb.IMPORT: Verb.IMPORT.command(stem, operators.Empty, self.import_presets, Some(invocations.Open[operators.Empty](extensions=json))),
            Verb.EXPORT: Verb.EXPORT.command(stem, _Named, self.export_preset, Some(invocations.Save[_Named](extensions=json))),
            Verb.REPLACE: Verb.REPLACE.command(
                stem,
                _Named,
                self.replace_preset,
                Some(invocations.Popup[_Named](partial(bpy.types.WindowManager.invoke_confirm, confirm_text="Replace"), message=Some(partial(_asked, "Replace preset {} with current values?")))),
            ),
            Verb.RENAME: Verb.RENAME.command(stem, _Renamed, self.rename_preset, Some(invocations.Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), Some(renaming)),
        })

    def apply_preset(self, call: operators.Call[_Named]) -> Outcome[operators.Answer]:
        """Applies the preset."""
        return self.preset.loaded(call.identity, call.properties.stem).bind(partial(self.applied, call.context))

    def step_preset(self, call: operators.Call[Offset]) -> Outcome[operators.Answer]:
        """Applies the preset the offset places from the first one the current values match, the first or last while none matches."""
        step = call.properties.offset
        if not (values := tuple(map(self.preset.read, self.preset.listed(call.identity).values()))):
            return Ok(operators.Unchanged(reason="No presets"))
        return (
            paired(accumulated(values), self.matches(call.context, tuple(held for value in values for held in value.to_option().to_list())))
            .map(lambda pair: tuple(zip(pair[0], pair[1])))
            .bind(
                lambda rows: self.applied(
                    call.context, rows[Block.of_seq(enumerate(rows)).filter(lambda pair: pair[1][1]).try_head().map(lambda pair: (pair[0] + step) % len(rows)).default_value(0 if step > 0 else -1)][0]
                )
            )
        )

    def save_preset(self, call: operators.Call[_Saved]) -> Outcome[operators.Answer]:
        """Saves the current values as a preset, replacing the saved preset of that name."""
        name, identity = call.properties.name, call.identity
        match _claimed(self.preset, identity, name).to_optional():
            case (_, Path() as path):
                return self.current(call.context).bind(partial(self.preset.written, path)).map(lambda _: operators.Done(notes=(("INFO", _reported("Replaced {}", path.stem)),)))
            case (_, _):
                return Error(Fault.of(BlenderReason.TAKEN, name, member=str(self.preset.folder(identity))))
            case None:
                return self.current(call.context).bind(partial(self.preset.saved, identity, name)).map(lambda path: operators.Done(notes=(("INFO", _reported("Added {}", path.stem)),)))

    def delete_preset(self, call: operators.Call[_Named]) -> Outcome[operators.Answer]:
        """Deletes the preset file."""
        return _user(self.preset, call.identity, call.properties.stem).bind(removed).map(lambda _: operators.Done())

    def import_presets(self, call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
        """Copies files into saved presets."""
        return accumulated(self.preset.imported(call.identity, path) for path in call.paths).map(
            lambda paths: operators.Done(notes=tuple(("INFO", _reported("Added {}", path.stem)) for path in paths))
        )

    def export_preset(self, call: operators.Call[_Named]) -> Outcome[operators.Answer]:
        """Writes the preset to files."""
        return accumulated(
            self.preset.found(call.identity, call.properties.stem).bind(lambda stored: self.preset.exported(stored, path) if isinstance(stored, Path) else self.preset.written(path, stored))
            for path in call.paths
        ).map(operators.Done.wrote)

    def replace_preset(self, call: operators.Call[_Named]) -> Outcome[operators.Answer]:
        """Replaces the saved preset with the current values."""
        return paired(_user(self.preset, call.identity, call.properties.stem), self.current(call.context)).bind(lambda pair: self.preset.written(*pair)).map(lambda _: operators.Done())

    def rename_preset(self, call: operators.Call[_Renamed]) -> Outcome[operators.Answer]:
        """Renames the saved preset file, a case change of its own name included."""
        identity, name, stem = call.identity, call.properties.name, call.properties.stem
        free = Ok(name).filter_with(
            lambda held: _claimed(self.preset, identity, held).filter(lambda pair: pair[0] != stem).is_none(), partial(Fault.of, BlenderReason.TAKEN, name, member=str(self.preset.folder(identity)))
        )
        return paired(_user(self.preset, identity, stem), free).bind(lambda pair: self.preset.renamed(*pair)).map(lambda _: operators.Done())

    # --- [ROW]

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the stepping, save, and import buttons beneath the entries read each redraw."""
        commands = self.commands
        steps = tuple(OperatorButton(target=Placement(commands[Verb.STEP], {"offset": offset}), keywords={"text": ""}, icon=Some("BACK" if offset < 0 else "FORWARD")) for offset in (-1, 1))
        footer = (*steps, *(OperatorButton(target=commands[verb], keywords={"text": ""}) for verb in (Verb.SAVE, Verb.IMPORT)))
        return realized(footer, identity).map(lambda buttons: partial(_catalog, self, identity, commands, buttons))

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the verb operators."""
        return Ok(tuple(self.commands.values()))


class Captured(msgspec.Struct, frozen=True, kw_only=True):
    """Preset catalog of selected settings captured from the context owner and applied across a scope."""

    preset: Preset[Snapshot]
    scope: Scoped
    selection: tuple[RnaPath, ...]

    @property
    def key(self) -> Key:
        """Menu key shared with the preset listing."""
        return menu_key(self.preset)

    def listing(self) -> Outcome[Listing[Snapshot]]:
        """Resolves the selected settings once into the listing."""
        return accumulated(Descriptor.resolve(self.scope.owner_type, path, bpy.types.Property) for path in self.selection).map(partial(_listed, self))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [NAMING]


def menu_key[T: msgspec.Struct](preset: Preset[T], /) -> Key:
    """Key of the menu listing a preset's catalog."""
    return f"{preset.key}_presets"


def _reported(template: str, stem: str) -> str:
    """Report text of a translated template over a preset's typed name."""
    return bpy.app.translations.pgettext_rpt(template).format(bpy.path.display_name(stem, title_case=False))


def _asked(template: str, _context: bpy.types.Context, properties: _Named) -> Outcome[str]:
    """Confirmation text of a translated template over the preset's typed name."""
    return Ok(bpy.app.translations.pgettext_iface(template).format(bpy.path.display_name(properties.stem, title_case=False)))


# --- [FILES]


def _claimed[T: msgspec.Struct](preset: Preset[T], identity: Identity, name: str) -> Option[tuple[str, T | Path]]:
    """Returns the listed preset whose file name the name takes, compared case-folded."""
    normalized = _AS_FILENAME(name).casefold()
    return Block.of_seq(preset.listed(identity).items()).filter(lambda pair: pair[0].casefold() == normalized).try_head()


def _user[T: msgspec.Struct](preset: Preset[T], identity: Identity, stem: str) -> Outcome[Path]:
    """Returns the user file of a stem, absent fault for a stock preset."""
    return preset.found(identity, stem).bind(
        lambda stored: Option.of_optional(try_downcast(Path, stored)).to_result_with(partial(Fault.of, BlenderReason.ABSENT, str(preset.folder(identity)), member=stem))
    )


# --- [CAPTURES]


def _listed(captured: Captured, selected: tuple[Descriptor, ...]) -> Listing[Snapshot]:
    """Listing over the selected settings of the scope's context owner."""
    scope = captured.scope
    return Listing(preset=captured.preset, current=partial(_current, scope, selected), matches=partial(_matching, scope, selected), applied=partial(_spread, scope))


def _captured(scope: Scoped, selected: tuple[Descriptor, ...], context: bpy.types.Context) -> Outcome[tuple[Struct, Snapshot]]:
    """Context owner of the scope's type with its selected settings."""
    return rooted(scope.owner_type, context).bind(lambda owner: Snapshot.capture(owner, Some(selected)).map(lambda snapshot: (owner, snapshot)))


def _current(scope: Scoped, selected: tuple[Descriptor, ...], context: bpy.types.Context) -> Outcome[Snapshot]:
    """Selected settings of the context owner."""
    return _captured(scope, selected, context).map(lambda pair: pair[1])


def _matching(scope: Scoped, selected: tuple[Descriptor, ...], context: bpy.types.Context, stored: tuple[Snapshot, ...]) -> Outcome[tuple[bool, ...]]:
    """Whether each stored preset projects onto the context owner's current settings."""
    return _captured(scope, selected, context).bind(lambda held: accumulated(value.projected(held[0], held[1]).map(lambda pair: pair[1] == held[1]) for value in stored))


def _spread(scope: Scoped, context: bpy.types.Context, stored: Snapshot) -> Outcome[operators.Answer]:
    """Applies a preset over every owner of the scope."""
    return scope.owners(context).map(lambda owners: tuple(map(stored.apply, owners))).bind(partial(_finished, scope))


def _finished(scope: Scoped, outcomes: tuple[Applied, ...]) -> Outcome[operators.Answer]:
    """Answer over the applied owners, an absent fault per member a preset holds and no owner takes."""
    owner = scope.owner_type.__name__
    if not outcomes:
        return Ok(operators.Unchanged(reason=bpy.app.translations.pgettext_rpt("No {} in scope").format(owner)))
    absent = (Applied(outcome=Error(Fault.of(BlenderReason.ABSENT, owner, member=rna(path)))) for path in dict.fromkeys(path for outcome in outcomes for path in outcome.skipped))
    return operators.answered(Applied.join((*outcomes, *absent)), "Settings already hold preset")


# --- [DRAWING]


def _catalog[T: msgspec.Struct](
    listing: Listing[T], identity: Identity, commands: Mapping[Verb, Command], footer: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context
) -> None:
    """Draws one apply button per listed preset under its untranslated name, checked while the current values match it, the file buttons its kind offers beside it, then the footer buttons."""
    catalog = tuple((stem, kept, listing.preset.read(kept)) for stem, kept in listing.preset.listed(identity).items())
    valid = tuple((stem, held) for stem, _, value in catalog for held in value.to_option().to_list())
    matched = listing.matches(context, tuple(value for _, value in valid)).map(lambda marks: dict(zip((stem for stem, _ in valid), marks)))
    alerted(layout, matched)
    for stem, kept, value in catalog:
        line, chosen, user = layout.row(align=True), {"stem": stem}, isinstance(kept, Path)
        label: Keywords = {"text": bpy.path.display_name(stem, title_case=not user), "translate": False, "icon": "CHECKMARK" if matched.default_value({}).get(stem, False) else "BLANK1"}
        alerted(line, value)
        for _ in value.to_option().to_list():
            alerted(line, pressed(line, identity, Placement(commands[Verb.APPLY], chosen), label))
        for verb in (Verb.REPLACE, Verb.RENAME, Verb.EXPORT, Verb.DELETE) if user else (Verb.EXPORT,):
            alerted(line, pressed(line, identity, Placement(commands[verb], chosen), {"text": "", "icon": verb.icon}))
    layout.separator()
    footer(layout.row(align=True), holder, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Captured", "Listing", "Offset", "Verb", "menu_key"]
