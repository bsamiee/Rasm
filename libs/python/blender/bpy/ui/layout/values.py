"""Value stores over attached form groups and the preset catalogs they list."""

from collections.abc import Callable, Mapping
import datetime
from enum import Enum, unique
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
from blender.bpy.commands.operators import Command, Placement
from blender.bpy.data.rna.conditions import Scoped
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import definition, rna, RnaPath, rooted, Struct
from blender.bpy.data.rna.properties import annotated
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.extension.reports import alerted
from blender.bpy.settings.attachments import Attached, Attachment, Host
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import ConditionalRow, Keywords, operator_button, OperatorButton, realized, RowContent, RowDraw
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Retention
from blender.records.stores import removed

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import IconItems

# --- [TYPES] ----------------------------------------------------------------------------

type Replacement = Callable[[Member], RowContent]


@unique
class StoreCommand(Enum):
    """Store or preset command with the icon its buttons share and the undo step of its runs, none for a command writing outside the blend file alone, each operator keyed `<command>_<stem>`."""

    RESET = ("LOOP_BACK", operators.UndoStep())
    COPY = ("COPYDOWN", operators.NoUndo())
    PASTE = ("PASTEDOWN", operators.UndoStep())
    RESTORE = ("RECOVER_LAST", operators.UndoStep())
    FORGET = ("X", operators.UndoStep())
    LOCK = ("LOCKED", operators.UndoStep())
    APPLY = ("PRESET", operators.UndoStep())
    STEP = ("FORWARD", operators.UndoStep())
    SAVE = ("ADD", operators.NoUndo())
    DELETE = ("X", operators.NoUndo())
    IMPORT = ("IMPORT", operators.NoUndo())
    EXPORT = ("EXPORT", operators.NoUndo())
    REPLACE = ("FILE_REFRESH", operators.NoUndo())
    RENAME = ("GREASEPENCIL", operators.NoUndo())

    def __init__(self, icon: "IconItems", undo: operators.Undo) -> None:
        """Holds the command's icon and the undo step of its runs."""
        self.icon, self.undo = icon, undo

    def command[T: bpy.types.PropertyGroup](
        self,
        stem: Key,
        form: type[T],
        run: Callable[[operators.Call[T]], Outcome[operators.Answer]],
        invoke: Option[operators.Invoke[T]] = Nothing,
        seed: Option[Callable[[T, bpy.types.Event, Identity], Mapping[str, Value]]] = Nothing,
    ) -> operators.Operator[T]:
        """Builds the internal operator over a store or preset stem under the command's undo step."""
        return operators.Operator(icon=Some(self.icon), options=frozenset({"INTERNAL"}), undo=self.undo, form=form, run=run, invoke=invoke, seed=seed, named=Some(f"{self.name.lower()}_{stem}"))


# --- [CONSTANTS] ------------------------------------------------------------------------

_AS_FILENAME: Final[Callable[[str], str]] = AddPresetBase.as_filename  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_NAME: Final = AddPresetBase.__annotations__["name"]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [FORMS]


class _Offset(bpy.types.PropertyGroup):
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


class _Lock(bpy.types.PropertyGroup):
    """Member a lock button names, each store declaring the enum of its own form members in a subclass."""

    field: str


# --- [PRESETS]


class Listing[T: msgspec.Struct](ConditionalRow, frozen=True, kw_only=True, dict=True):
    """Preset catalog applying, saving, importing, exporting, replacing, renaming, deleting, and stepping through presets of the current values."""

    preset: Preset[T]
    current: Callable[[bpy.types.Context], Outcome[T]]
    matches: Callable[[bpy.types.Context, tuple[T, ...]], Outcome[tuple[bool, ...]]]
    applied: Callable[[bpy.types.Context, T], Outcome[operators.Answer]]

    @property
    def key(self) -> Key:
        """Listing menu key an `Opens` row opens."""
        return menu_key(self.preset)

    # --- [COMMANDS]

    @cached_property
    def commands(self) -> Mapping[StoreCommand, Command]:
        """StoreCommand operators over the preset stem, preset files filtered as JSON and the rename dialog seeded with the preset's typed name."""
        stem = self.preset.key

        def json(_properties: object) -> tuple[str, *tuple[str, ...]]:
            return (".json",)

        def renaming(properties: _Renamed, _event: bpy.types.Event, _identity: Identity) -> Mapping[str, Value]:
            return {"name": bpy.path.display_name(properties.stem, title_case=False)}

        return MappingProxyType({
            StoreCommand.APPLY: StoreCommand.APPLY.command(stem, _Named, self.apply_preset),
            StoreCommand.STEP: StoreCommand.STEP.command(stem, _Offset, self.step_preset),
            StoreCommand.SAVE: StoreCommand.SAVE.command(stem, _Saved, self.save_preset, Some(invocations.Popup(partial(bpy.types.WindowManager.invoke_props_dialog)))),
            StoreCommand.DELETE: StoreCommand.DELETE.command(
                stem,
                _Named,
                self.delete_preset,
                Some(invocations.Popup[_Named](partial(bpy.types.WindowManager.invoke_confirm, confirm_text="Remove", icon="WARNING"), message=Some(partial(_asked, "Remove {}")))),
            ),
            StoreCommand.IMPORT: StoreCommand.IMPORT.command(stem, operators.Empty, self.import_presets, Some(invocations.Open[operators.Empty](extensions=json))),
            StoreCommand.EXPORT: StoreCommand.EXPORT.command(stem, _Named, self.export_preset, Some(invocations.Save[_Named](extensions=json))),
            StoreCommand.REPLACE: StoreCommand.REPLACE.command(
                stem,
                _Named,
                self.replace_preset,
                Some(invocations.Popup[_Named](partial(bpy.types.WindowManager.invoke_confirm, confirm_text="Replace"), message=Some(partial(_asked, "Replace preset {} with current values?")))),
            ),
            StoreCommand.RENAME: StoreCommand.RENAME.command(stem, _Renamed, self.rename_preset, Some(invocations.Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), Some(renaming)),
        })

    def apply_preset(self, call: operators.Call[_Named]) -> Outcome[operators.Answer]:
        """Applies the preset."""
        return self.preset.loaded(call.identity, call.properties.stem).bind(partial(self.applied, call.context))

    def step_preset(self, call: operators.Call[_Offset]) -> Outcome[operators.Answer]:
        """Applies the preset the offset places from the first one the current values match, the first or last while none matches."""
        step = call.properties.offset
        if not (values := tuple(self.preset.read(source) for source in self.preset.listed(call.identity).values())):
            return Ok(operators.Unchanged(reason="No presets"))

        def stepped(stored: tuple[T, ...], marks: tuple[bool, ...]) -> Outcome[operators.Answer]:
            return self.applied(call.context, stored[next(((index + step) % len(stored) for index, matched in enumerate(marks) if matched), 0 if step > 0 else -1)])

        return paired(accumulated(values), self.matches(call.context, tuple(held for value in values for held in value.to_option().to_list()))).bind(lambda pair: stepped(*pair))

    def save_preset(self, call: operators.Call[_Saved]) -> Outcome[operators.Answer]:
        """Saves the current values as a preset, replacing the saved preset of that name."""
        name, identity = call.properties.name, call.identity
        match _claimed(self.preset, identity, name).to_optional():
            case (_, Path() as path):
                return self.current(call.context).bind(partial(self.preset.written, path)).map(lambda _: operators.Done(notes=(("INFO", _reported("Replaced {}", path.stem)),)))
            case (_, _):
                return Error(Fault.of(Reason.TAKEN, name, member=str(self.preset.folder(identity))))
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
        return accumulated(self.preset.found(call.identity, call.properties.stem).bind(lambda stored: self.preset.exported(stored, path)) for path in call.paths).map(operators.Done.wrote)

    def replace_preset(self, call: operators.Call[_Named]) -> Outcome[operators.Answer]:
        """Replaces the saved preset with the current values."""
        return paired(_user(self.preset, call.identity, call.properties.stem), self.current(call.context)).bind(lambda pair: self.preset.written(*pair)).map(lambda _: operators.Done())

    def rename_preset(self, call: operators.Call[_Renamed]) -> Outcome[operators.Answer]:
        """Renames the saved preset file, a case change of its own name included."""
        identity, name, stem = call.identity, call.properties.name, call.properties.stem
        free = Ok(name).filter_with(
            lambda held: _claimed(self.preset, identity, held).filter(lambda pair: pair[0] != stem).is_none(), partial(Fault.of, Reason.TAKEN, name, member=str(self.preset.folder(identity)))
        )
        return paired(_user(self.preset, identity, stem), free).bind(lambda pair: self.preset.renamed(*pair)).map(lambda _: operators.Done())

    # --- [ROW]

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the stepping, save, and import buttons beneath the entries read each redraw."""
        commands = self.commands
        footer = (*_steps(commands[StoreCommand.STEP], -1, {"text": ""}), *(OperatorButton(target=commands[command], keywords={"text": ""}) for command in (StoreCommand.SAVE, StoreCommand.IMPORT)))
        return realized(footer, identity).map(lambda buttons: partial(self.shown, identity, buttons))

    def shown(self, identity: Identity, footer: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws one apply button per listed preset, checked while the current values match it, beside its file buttons, then the footer buttons."""
        commands = self.commands
        catalog = tuple((stem, kept, self.preset.read(kept)) for stem, kept in self.preset.listed(identity).items())
        valid = tuple((stem, held) for stem, _, value in catalog for held in value.to_option().to_list())
        matched = self.matches(context, tuple(value for _, value in valid)).map(lambda marks: dict(zip((stem for stem, _ in valid), marks)))
        alerted(layout, matched)
        for stem, kept, value in catalog:
            line, chosen, user = layout.row(align=True), {"stem": stem}, isinstance(kept, Path)
            label: Keywords = {"text": bpy.path.display_name(stem, title_case=not user), "translate": False, "icon": "CHECKMARK" if matched.default_value({}).get(stem, False) else "BLANK1"}
            alerted(line, value)
            for _ in value.to_option().to_list():
                alerted(line, operator_button(line, identity, Placement(commands[StoreCommand.APPLY], chosen), label))
            for command in (StoreCommand.REPLACE, StoreCommand.RENAME, StoreCommand.EXPORT, StoreCommand.DELETE) if user else (StoreCommand.EXPORT,):
                alerted(line, operator_button(line, identity, Placement(commands[command], chosen), {"text": "", "icon": command.icon}))
        layout.separator()
        footer(layout.row(align=True), holder, context)

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the command operators."""
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
        """Resolves the selected settings once into the listing over the scope's context owner."""
        return accumulated(Descriptor.resolve(self.scope.owner_type, path, bpy.types.Property) for path in self.selection).map(
            lambda selected: Listing(preset=self.preset, current=partial(self.current, selected), matches=partial(self.matching, selected), applied=self.spread)
        )

    def _captured(self, selected: tuple[Descriptor, ...], context: bpy.types.Context) -> Outcome[tuple[Struct, Snapshot]]:
        """Context owner of the scope's type with its selected settings."""
        return rooted(self.scope.owner_type, context).bind(lambda owner: Snapshot.capture(owner, Some(selected)).map(lambda snapshot: (owner, snapshot)))

    def current(self, selected: tuple[Descriptor, ...], context: bpy.types.Context) -> Outcome[Snapshot]:
        """Selected settings of the context owner."""
        return self._captured(selected, context).map(lambda pair: pair[1])

    def matching(self, selected: tuple[Descriptor, ...], context: bpy.types.Context, stored: tuple[Snapshot, ...]) -> Outcome[tuple[bool, ...]]:
        """Whether each stored preset projects onto the context owner's current settings."""
        return self._captured(selected, context).bind(lambda held: accumulated(value.projected(held[0], held[1]).map(lambda pair: pair[1] == held[1]) for value in stored))

    def spread(self, context: bpy.types.Context, stored: Snapshot) -> Outcome[operators.Answer]:
        """Applies a preset over every owner of the scope, an absent fault per member a preset holds and no owner takes."""
        owner = self.scope.owner_type.__name__

        def finished(outcomes: tuple[Applied, ...]) -> Outcome[operators.Answer]:
            if not outcomes:
                return Ok(operators.Unchanged(reason=bpy.app.translations.pgettext_rpt("No {} in scope").format(owner)))
            absent = (Applied(outcome=Error(Fault.of(Reason.ABSENT, owner, member=rna(path)))) for path in dict.fromkeys(path for outcome in outcomes for path in outcome.skipped))
            return operators.answered(Applied.join((*outcomes, *absent)), "Settings hold preset")

        return self.scope.owners(context).map(lambda owners: tuple(map(stored.apply, owners))).bind(finished)


# --- [STORES]


class Store[T: bpy.types.PropertyGroup](ConditionalRow, frozen=True, kw_only=True, dict=True):
    """Value sets of an attached form group held as snapshots, its host read through `host` at the section holder, drawn as the values popover.

    `fields` replace the property rows a section draws for the named form members.
    """

    attachment: Attachment[T]
    host: Option[Member] = Nothing
    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing
    presets: Option[Preset[Snapshot]] = Nothing
    scope: Option[Scoped[Host]] = Nothing
    fields: Mapping[str, Replacement] = MappingProxyType({})

    def __post_init__(self) -> None:
        """Refuses a scope whose owners never hold the attachment."""
        owner = self.attachment.owner
        for scope in self.scope.filter(lambda scope: not issubclass(scope.owner_type, owner)).to_list():
            raise ValueError(f"{scope.owner_type.__name__} scope holds no {owner.__name__} attachment {self.attachment.key}")

    @property
    def panel(self) -> Key:
        """Key of the values popover."""
        return f"{self.attachment.key}_values"

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Attachment the store binds."""
        return (self.attachment,)

    def listing(self, identity: Identity) -> Outcome[Option[Listing[Snapshot]]]:
        """Preset catalog over the host the context pointer names, locked members kept, a preset's unlocked members applied as one history step."""
        return self.attachment.bound(identity).map(
            lambda attached: self.presets.map(
                lambda preset: Listing(
                    preset=preset,
                    current=lambda context: hosted(attached.name, context).bind(attached.preset),
                    matches=lambda context, values: hosted(attached.name, context).bind(
                        lambda host: accumulated(attached.projected(host, value).map(lambda ends: ends[2] == ends[0]) for value in values)
                    ),
                    applied=lambda context, value: self._recorded(identity, context, lambda held, host: held.applied(host, value).map(lambda wrote: Some(operators.Done()).filter(lambda _: wrote))),
                )
            )
        )

    # --- [COMMANDS]

    @cached_property
    def commands(self) -> Mapping[StoreCommand, Command]:
        """Command operators over the attachment key, the lock command present while presets apply over a hidden form naming one declared member."""
        stem, form = self.attachment.key, self.attachment.form

        def locked() -> type[_Lock]:
            field = bpy.props.EnumProperty(name="Member", description="Member to lock or unlock", items=tuple((name, name, "") for name in annotated(form)), options={"HIDDEN"})
            return type(f"{form.__name__}Lock", (_Lock,), {"__annotations__": {"field": field}})

        locking = self.presets.map(lambda _: {StoreCommand.LOCK: StoreCommand.LOCK.command(stem, locked(), self.lock_value)}).default_value({})
        return MappingProxyType({
            StoreCommand.RESET: StoreCommand.RESET.command(stem, operators.Empty, self.reset_values),
            StoreCommand.COPY: StoreCommand.COPY.command(stem, operators.Empty, self.copy_values),
            StoreCommand.PASTE: StoreCommand.PASTE.command(stem, operators.Empty, self.paste_values),
            StoreCommand.RESTORE: StoreCommand.RESTORE.command(stem, _Offset, self.restore_values),
            StoreCommand.FORGET: StoreCommand.FORGET.command(stem, _Offset, self.forget_values),
            **locking,
        })

    def reset_values(self, call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
        """Unsets every set and editable member the form declares, running each member's update, unchanged while the values after equal the values before."""
        inherited = bpy.types.PropertyGroup.bl_rna.properties

        def reset(attached: Attached[T], host: Host) -> Outcome[Option[operators.Done]]:
            group = attached.struct(host)
            held = tuple(
                prop.identifier
                for prop in definition(group).properties
                if prop.identifier not in inherited and not group.is_property_readonly(prop.identifier) and group.is_property_set(prop.identifier)
            )

            def unset(before: Snapshot) -> Outcome[Option[operators.Done]]:
                for identifier in held:
                    group.property_unset(identifier)
                    group.path_resolve(identifier, False).update()
                return Snapshot.capture(group).map(lambda after: Some(operators.Done()).filter(lambda _: after != before))

            return Snapshot.capture(group).bind(unset)

        return self._recorded(call.identity, call.context, reset)

    def copy_values(self, call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
        """Copies the values to the clipboard."""
        return (
            paired(self._held(call.identity, call.context).bind(lambda held: Snapshot.capture(held[0].struct(held[1]))), rooted(bpy.types.WindowManager, call.context))
            .map(lambda pair: pair[0].copied(pair[1]))
            .map(lambda _: operators.Done())
        )

    def paste_values(self, call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
        """Pastes clipboard values over every scope owner, the host alone while no scope is stated."""
        context = call.context

        def pasted(attached: Attached[T], host: Host) -> Outcome[Option[operators.Done]]:
            return (
                paired(self.scope.map(lambda scope: scope.owners(context)).default_value(Ok((host,))), rooted(bpy.types.WindowManager, context))
                .bind(lambda pair: Snapshot.pasted(pair[1], tuple(map(attached.struct, pair[0]))))
                .map(lambda outcomes: Some(operators.Done()).filter(lambda _: any(outcome.written for outcome in outcomes)))
            )

        return self._recorded(call.identity, context, pasted)

    def restore_values(self, call: operators.Call[_Offset]) -> Outcome[operators.Answer]:
        """Restores the value set the offset names in history, older ones positive."""
        return (
            paired(self._held(call.identity, call.context), self._kept(call.identity))
            .bind(lambda pair: pair[0][0].stepped(pair[0][1], call.properties.offset, datetime.datetime.now(datetime.UTC), pair[1]))
            .map(_concluded)
        )

    def forget_values(self, call: operators.Call[_Offset]) -> Outcome[operators.Answer]:
        """Removes a value set from history."""
        return self._held(call.identity, call.context).bind(lambda held: held[0].forgotten(held[1], call.properties.offset)).map(_concluded)

    def lock_value(self, call: operators.Call[_Lock]) -> Outcome[operators.Answer]:
        """Keeps the member's value when presets apply."""
        return self._held(call.identity, call.context).bind(lambda held: held[0].toggled(held[1], call.properties.field)).map(lambda _: operators.Done())

    # --- [STEPS]

    def _held(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Attached[T], Host]]:
        """Binds the attachment and reads the host the context pointer names."""
        return self.attachment.bound(identity).bind(lambda attached: hosted(attached.name, context).map(lambda host: (attached, host)))

    def _kept(self, identity: Identity) -> Outcome[Retention]:
        """History retention the store reads, the default while it states none."""
        return self.retention.map(lambda read: read(identity)).default_with(lambda: Ok(Retention()))

    def _recorded(self, identity: Identity, context: bpy.types.Context, write: Callable[[Attached[T], Host], Outcome[Option[operators.Done]]]) -> Outcome[operators.Answer]:
        """Runs an edit, adding the prior value set to history when it changes a value."""
        return (
            paired(self._held(identity, context), self._kept(identity))
            .bind(lambda pair: pair[0][0].recorded(pair[0][1], lambda: write(*pair[0]), datetime.datetime.now(datetime.UTC), pair[1]))
            .map(lambda change: change.default_value(operators.Unchanged(reason="No value changed")))
        )

    # --- [ROW]

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the preset catalog, member locks, clipboard and history steps, and one restore and forget row per history entry."""
        commands = self.commands
        lines = accumulated(
            realized(buttons, identity)
            for buttons in (tuple(OperatorButton(target=commands[command]) for command in (StoreCommand.COPY, StoreCommand.PASTE)), _steps(commands[StoreCommand.RESTORE], 1))
        )
        listing = self.listing(identity).bind(lambda found: found.map(lambda held: realized((held,), identity).map(Some)).default_value(Ok(Nothing)))
        return paired(self.attachment.bound(identity), listing, lines).map(lambda parts: partial(self.shown, identity, *parts))

    def shown(self, identity: Identity, attached: Attached[T], listing: Option[RowDraw], lines: tuple[RowDraw, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the catalog and member locks, the clipboard and history step lines, then one restore and forget line per history entry, times untranslated."""
        commands, found = self.commands, hosted(attached.name, context)
        for draw in listing.to_list():
            draw(layout, holder, context)
        for host, lock in ((host, lock) for host in found.to_option().to_list() for lock in Option.of_optional(commands.get(StoreCommand.LOCK)).to_list()):
            kept, group, declared = attached.locks(host), attached.struct(host), annotated(attached.form)
            alerted(layout, kept)
            locks = kept.default_value(frozenset())
            for prop in (prop for prop in definition(group).properties if prop.identifier in declared):
                keywords: Keywords = {"text": prop.name, "text_ctxt": prop.translation_context, "icon": "LOCKED" if prop.identifier in locks else "UNLOCKED"}
                alerted(layout, operator_button(layout, identity, Placement(lock, {"field": prop.identifier}), keywords))
        layout.separator()
        for draw in lines:
            draw(layout.row(align=True), holder, context)
        held = found.bind(attached.history)
        entries = held.map(
            lambda value: (*((index, entry.moment) for index, entry in enumerate(value.past, 1)), *((-index, entry.moment) for index, entry in enumerate(value.future, 1)))
        ).default_value(())
        for offset, moment in entries:
            line, chosen = layout.row(align=True), {"offset": offset}
            alerted(
                line,
                paired(
                    operator_button(
                        line, identity, Placement(commands[StoreCommand.RESTORE], chosen), {"text": f"{moment.astimezone():%X}", "translate": False, "icon": "BACK" if offset > 0 else "FORWARD"}
                    ),
                    operator_button(line, identity, Placement(commands[StoreCommand.FORGET], chosen), {"text": "", "icon": "X"}),
                ),
            )
        alerted(layout, held)

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the command operators and the preset catalog over the bound attachment."""
        return self.listing(identity).map(lambda listing: (*self.commands.values(), *listing.to_list()))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [NAMING]


def menu_key[T: msgspec.Struct](preset: Preset[T], /) -> Key:
    """Key of the menu listing a preset's catalog."""
    return f"{preset.key}_presets"


def _reported(template: str, stem: str, translate: Callable[[str], str] = bpy.app.translations.pgettext_rpt) -> str:
    """Translated template over a preset's typed name, report text unless `translate` states another context."""
    return translate(template).format(bpy.path.display_name(stem, title_case=False))


def _steps(command: Command, back: int, keywords: Keywords = MappingProxyType({})) -> tuple[OperatorButton, ...]:
    """Step buttons over a command's signed offset, the one moving back first."""
    return tuple(OperatorButton(target=Placement(command, {"offset": offset}), keywords=keywords, icon=Some("BACK" if offset == back else "FORWARD")) for offset in (back, -back))


def _asked(template: str, _context: bpy.types.Context, properties: _Named, /) -> Outcome[str]:
    """Confirmation text of a translated template over the preset's typed name."""
    return Ok(_reported(template, properties.stem, bpy.app.translations.pgettext_iface))


# --- [FILES]


def _claimed[T: msgspec.Struct](preset: Preset[T], identity: Identity, name: str) -> Option[tuple[str, T | Path]]:
    """Returns the listed preset whose file name the name takes, compared case-folded."""
    normalized = _AS_FILENAME(name).casefold()
    return Block.of_seq(preset.listed(identity).items()).filter(lambda pair: pair[0].casefold() == normalized).try_head()


def _user[T: msgspec.Struct](preset: Preset[T], identity: Identity, stem: str) -> Outcome[Path]:
    """Returns the user file of a stem, absent fault for a stock preset."""
    return preset.found(identity, stem).bind(lambda stored: Option.of_optional(try_downcast(Path, stored)).to_result_with(partial(Fault.of, Reason.ABSENT, str(preset.folder(identity)), member=stem)))


# --- [HOSTS]


def hosted(name: str, context: bpy.types.Context) -> Outcome[Host]:
    """Returns the host a layout's context pointer names, absent fault when unset."""
    return Option.of_optional(getattr(context, name, None)).to_result_with(partial(Fault.of, Reason.ABSENT, "bpy.context", member=name))


def _concluded[V](change: Option[V]) -> operators.Answer:
    """Done for a history move, unchanged past either end."""
    return change.map(lambda _: operators.Done()).default_value(operators.Unchanged(reason="No history entry at offset"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Captured", "Listing", "Replacement", "Store", "StoreCommand", "hosted", "menu_key"]
