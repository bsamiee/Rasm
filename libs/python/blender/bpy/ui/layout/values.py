"""Store value sets with lock and history rows, and preset listing actions."""

from collections.abc import Callable, Mapping
import datetime
from enum import auto, Enum
from functools import partial
from pathlib import Path
from types import MappingProxyType
import typing
from typing import Final

from bl_operators.presets import AddPresetBase
import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands import operators
from blender.bpy.data.rna.descriptors import Struct
from blender.bpy.data.rna.snapshots import Scoped, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import annotated, Attached, Attachment, Host, Slot
from blender.bpy.settings.history import Retention, ValueHistory
from blender.bpy.settings.presets import copied, pasted, Preset, PresetName, removed
from blender.bpy.ui.layout.rows import acquired, alerted, invoking, Line, Member, OperatorButton, Popover, pressed, realized, refused, RowContent, RowDraw
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason

if typing.TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import IconItems

# --- [TYPES] ----------------------------------------------------------------------------

type Replacement = Callable[[Member], RowContent]


class Verb(Enum):
    """Store and listing operator rows, each keyed `<verb>_<stem>`."""

    RESET = auto()
    COPY = auto()
    PASTE = auto()
    OLDER = auto()
    NEWER = auto()
    RESTORE = auto()
    FORGET = auto()
    APPLY = auto()
    SAVE = auto()
    DELETE = auto()
    IMPORT = auto()
    EXPORT = auto()
    REPLACE = auto()
    RENAME = auto()
    PREVIOUS = auto()
    NEXT = auto()
    LOCK = auto()

    def key(self, stem: Key) -> Key:
        """Returns operator key over store or preset key."""
        return f"{self.name.lower()}_{stem}"

    def row[T: bpy.types.PropertyGroup](
        self, stem: Key, form: type[T], run: Callable[[operators.Call[T]], Outcome[operators.Answer]], invoke: Option[operators.Invoke[T, None]] = Nothing
    ) -> operators.Operator[T]:
        """Builds internal verb operator over stem from the verb's template, naming the body for the key it registers as."""
        held = TEMPLATES[self]
        run.__name__ = run.__qualname__ = self.key(stem)  # ty: ignore[unresolved-attribute]
        return operators.Operator(icon=Some(held.icon), internal=True, form=form, run=run, undo=held.undo, invoke=invoke)


# --- [MODELS] ---------------------------------------------------------------------------


class Template(msgspec.Struct, frozen=True, kw_only=True):
    """Icon and undo step every operator row of one verb shares."""

    icon: "IconItems"
    undo: bool | str


class Picked(bpy.types.PropertyGroup):
    """History entry a restore or remove row names."""

    offset: bpy.props.IntProperty(name="Entry", description="Signed one-based history offset")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Locked(bpy.types.PropertyGroup):
    """Member a lock row toggles."""

    field: bpy.props.StringProperty(description="Member to lock or unlock")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Named(bpy.types.PropertyGroup):
    """Preset an apply, remove, export, or replace row names."""

    stem: bpy.props.StringProperty(name="Preset", description="Preset file stem")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Renamed(bpy.types.PropertyGroup):
    """Saved preset and its new file name."""

    stem: bpy.props.StringProperty(name="Preset", description="Preset file stem", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    name: bpy.props.StringProperty(name="Name", description="Preset and file name", maxlen=64)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Saved(bpy.types.PropertyGroup):
    """Name entered in save-preset dialog."""

    name: bpy.props.StringProperty(name="Name", description="Preset and file name", maxlen=64)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Actions(msgspec.Struct, frozen=True, kw_only=True):
    """Verb operator rows over one preset listing."""

    apply: operators.Operator[Named]
    save: operators.Operator[Saved]
    delete: operators.Operator[Named]
    load: operators.Operator[operators.Empty]
    export: operators.Operator[Named]
    replace: operators.Operator[Named]
    rename: operators.Operator[Renamed]
    previous: operators.Operator[operators.Empty]
    following: operators.Operator[operators.Empty]


class Listing[T: msgspec.Struct](Line, frozen=True, kw_only=True):
    """Preset catalog actions."""

    preset: Preset[T]
    current: Callable[[bpy.types.Context], Outcome[T]]
    matches: Callable[[bpy.types.Context, tuple[T, ...]], Outcome[tuple[bool, ...]]]
    applied: Callable[[bpy.types.Context, T], Outcome[operators.Answer]]

    @property
    def key(self) -> Key:
        """Listing menu key a `Pulldown` opens."""
        return Listing.keyed(self.preset)

    @staticmethod
    def keyed[S: msgspec.Struct](preset: Preset[S]) -> Key:
        """Returns listing menu key of a preset before any listing exists."""
        return f"{preset.key}_presets"

    def commands(self, identity: Identity) -> Actions:
        """Builds verb rows over listing."""
        stem, shown, json = self.preset.key, bpy.path.display_name, lambda _: (".json",)

        def user(stored: T | Path, key: str) -> Outcome[Path]:
            return Ok(stored) if isinstance(stored, Path) else Error((Fault(reason=BlenderReason.ABSENT, subject=str(self.preset.folder(identity)), member=key),))

        def named(text: str) -> Outcome[PresetName]:
            return catch(msgspec.convert, exception=msgspec.ValidationError)(text, PresetName).map_error(lambda error: (Fault(reason=RecordReason.OUTSIDE, subject=text, cause=error),))

        def claimed(name: PresetName) -> Option[tuple[str, T | Path]]:
            normalized = AddPresetBase.as_filename(name).casefold()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return Option.of_optional(next(((key, value) for key, value in self.preset.listed(identity).items() if key.casefold() == normalized), None))

        def available(name: PresetName) -> Outcome[PresetName]:
            return claimed(name).map(lambda _: Error((Fault(reason=BlenderReason.TAKEN, subject=name, member=str(self.preset.folder(identity))),))).default_value(Ok(name))

        def neighbor(call: operators.Call[operators.Empty], step: int) -> Outcome[operators.Answer]:
            if not (values := tuple(map(self.preset.read, self.preset.listed(identity).values()))):
                return Ok(operators.Unchanged(reason="No presets"))
            return (
                paired(accumulated(values), self.matches(call.context, tuple(held for value in values for held in value.to_option().to_list())))
                .map(lambda pair: tuple(zip(pair[0], pair[1])))
                .bind(
                    lambda rows: self.applied(
                        call.context,
                        rows[
                            Option
                            .of_optional(next((index for index, (_, matched) in enumerate(rows) if matched), None))
                            .map(lambda index: (index + step) % len(rows))
                            .default_value(0 if step > 0 else -1)
                        ][0],
                    )
                )
            )

        def replace(call: operators.Call[Named]) -> Outcome[operators.Answer]:
            """Replaces the preset with section values."""
            return (
                paired(self.preset.found(identity, call.properties.stem).bind(lambda stored: user(stored, call.properties.stem)), self.current(call.context))
                .bind(lambda pair: self.preset.written(*pair))
                .map(lambda _: operators.Done())
            )

        def apply(call: operators.Call[Named]) -> Outcome[operators.Answer]:
            """Applies the preset."""
            return self.preset.found(identity, call.properties.stem).bind(self.preset.read).bind(lambda value: self.applied(call.context, value))

        def save(call: operators.Call[Saved]) -> Outcome[operators.Answer]:
            """Saves section values as a preset."""
            return named(call.properties.name).bind(lambda name: saved(call, name))

        def delete(call: operators.Call[Named]) -> Outcome[operators.Answer]:
            """Deletes the preset file."""
            return self.preset.found(identity, call.properties.stem).bind(lambda stored: user(stored, call.properties.stem)).bind(removed).map(lambda _: operators.Done())

        def load(call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Copies files into saved presets."""
            return accumulated(self.preset.imported(identity, path) for path in call.paths).map(lambda paths: operators.Done(notes=tuple(("INFO", f"Added {shown(path.stem)}") for path in paths)))

        def export(call: operators.Call[Named]) -> Outcome[operators.Answer]:
            """Writes the preset to files."""
            return accumulated(
                self.preset.found(identity, call.properties.stem).bind(lambda stored: self.preset.exported(stored, path) if isinstance(stored, Path) else self.preset.written(path, stored))
                for path in call.paths
            ).map(operators.Done.wrote)

        def rename(call: operators.Call[Renamed]) -> Outcome[operators.Answer]:
            """Renames the preset file."""
            return (
                paired(self.preset.found(identity, call.properties.stem).bind(lambda stored: user(stored, call.properties.stem)), named(call.properties.name).bind(available))
                .bind(lambda pair: self.preset.renamed(*pair))
                .map(lambda _: operators.Done())
            )

        def previous(call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Applies the previous preset."""
            return neighbor(call, -1)

        def following(call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Applies the next preset."""
            return neighbor(call, 1)

        def saved(call: operators.Call[Saved], name: PresetName) -> Outcome[operators.Done]:
            match claimed(name).to_optional():
                case (_, Path() as path):
                    return self.current(call.context).bind(lambda value: self.preset.written(path, value)).map(lambda _: operators.Done(notes=(("INFO", f"Replaced {shown(path.stem)}"),)))
                case (_, _):
                    return Error((Fault(reason=BlenderReason.TAKEN, subject=name, member=str(self.preset.folder(identity))),))
                case None:
                    return self.current(call.context).bind(lambda value: self.preset.saved(identity, name, value)).map(lambda path: operators.Done(notes=(("INFO", f"Added {shown(path.stem)}"),)))

        return Actions(
            apply=Verb.APPLY.row(stem, Named, apply),
            save=Verb.SAVE.row(stem, Saved, save, invoke=Some(operators.Properties())),
            delete=Verb.DELETE.row(
                stem,
                Named,
                delete,
                invoke=Some(operators.Confirm[Named](message=Some(lambda _context, properties: Ok(f"Remove {shown(properties.stem)}")), confirm_text=Some("Remove"), icon="WARNING")),
            ),
            load=Verb.IMPORT.row(stem, operators.Empty, load, invoke=Some(operators.Open[operators.Empty](extensions=json))),
            export=Verb.EXPORT.row(stem, Named, export, invoke=Some(operators.Save[Named](extensions=json))),
            replace=Verb.REPLACE.row(
                stem,
                Named,
                replace,
                invoke=Some(operators.Confirm[Named](message=Some(lambda _context, properties: Ok(f"Replace preset {shown(properties.stem)} with section values?")), confirm_text=Some("Replace"))),
            ),
            rename=msgspec.structs.replace(
                Verb.RENAME.row(stem, Renamed, rename, invoke=Some(operators.Properties())), draw=Some(lambda layout, _context, properties, _identity: layout.prop(properties, "name"))
            ),
            previous=Verb.PREVIOUS.row(stem, operators.Empty, previous),
            following=Verb.NEXT.row(stem, operators.Empty, following),
        )

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Realizes apply entries with file actions and add and import buttons."""
        commands = self.commands(identity)
        apply, delete, export = commands.apply, commands.delete, commands.export
        footer = (OperatorButton(target=commands.save, text=""), OperatorButton(target=commands.load, text=""))

        def draw(buttons: tuple[RowDraw, ...]) -> RowDraw:
            def entry(layout: bpy.types.UILayout, context: bpy.types.Context, stem: str, kept: T | Path, value: Outcome[T], matched: Option[bool]) -> None:
                line, chosen = layout.row(align=True), {"stem": stem}
                alerted(line, value.bind(lambda _: pressed(line, identity, apply, chosen, text=bpy.path.display_name(stem), icon="CHECKMARK" if matched == Some(True) else "BLANK1")))

                def actions(row: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
                    alerted(row, pressed(row, identity, export, chosen, text="", icon="EXPORT"))
                    if isinstance(kept, Path):
                        alerted(row, pressed(row, identity, delete, chosen, text="", icon="X"))

                invoking(actions)(line, context, context)

            def shown(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
                layout.emboss = "PULLDOWN_MENU"
                catalog = tuple((stem, kept, self.preset.read(kept)) for stem, kept in self.preset.listed(identity).items())
                valid = tuple((stem, held) for stem, _, value in catalog for held in value.to_option().to_list())
                matched = self.matches(context, tuple(value for _, value in valid)).map(lambda marks: dict(zip((stem for stem, _ in valid), marks)))
                alerted(layout, matched)
                for stem, kept, value in catalog:
                    entry(layout, context, stem, kept, value, matched.to_option().bind(lambda marks: Option.of_optional(marks.get(stem))))
                layout.separator()
                line = layout.row()
                for button in buttons:
                    button(line, holder, context)

            return shown

        return accumulated(realized(row, identity) for row in footer).map(draw)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Acquires listing rows."""
        return Ok(msgspec.structs.astuple(self.commands(identity)))


class Slots[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Store bound to an identity."""

    attached: Attached[T]
    history: ValueHistory[Snapshot, Host, None]
    plain: Mapping[Verb, operators.Operator[operators.Empty]]
    picked: Mapping[Verb, operators.Operator[Picked]]
    lock: operators.Operator[Locked]
    listing: Option[Listing[Snapshot]]
    tail: tuple[RowDraw, ...]
    fields: Mapping[str, Replacement]

    def shown(self, identity: Identity, parts: tuple[Option[RowDraw], tuple[tuple[RowDraw, ...], tuple[RowDraw, ...]]], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws listing, member locks, clipboard and history steps, then one restore and remove row per history entry."""
        (listing, (copying, stepping)), attached = parts, self.attached
        found = hosted(attached.name, context)
        for each in listing.to_list():
            each(layout, holder, context)
        for host in found.to_option().to_list():
            locks, group = attached.locks(host), attached.struct(host)
            for member in tuple(annotated(attached.form)) if listing.is_some() else ():
                icon: IconItems = "LOCKED" if member in locks else "UNLOCKED"
                alerted(layout, pressed(layout, identity, self.lock, {"field": member}, text=group.bl_rna.properties[member].name, icon=icon))
        layout.separator()
        for draws in (copying, stepping):
            line = layout.row(align=True)
            for each in draws:
                each(line, holder, context)
        held = found.bind(self.history.held)
        entries = held.map(
            lambda value: tuple((index, entry.moment) for index, entry in enumerate(value.past, 1)) + tuple((-index, entry.moment) for index, entry in enumerate(value.future, 1))
        ).default_value(())
        for offset, moment in entries:
            line, chosen = layout.row(align=True), {"offset": offset}
            alerted(
                line,
                paired(
                    pressed(line, identity, self.picked[Verb.RESTORE], chosen, text=f"{moment.astimezone():%X}", icon="BACK" if offset > 0 else "FORWARD"),
                    pressed(line, identity, self.picked[Verb.FORGET], chosen, text="", icon="X"),
                ),
            )
        alerted(layout, held)


class Store[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Value sets of an attached form group, held as snapshots."""

    attachment: Attachment[T]
    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing
    presets: Option[Preset[Snapshot]] = Nothing
    scope: Option[Scoped[Host]] = Nothing
    fields: Mapping[str, Replacement] = msgspec.field(default_factory=dict)

    @property
    def panel(self) -> Key:
        """Key of values popover."""
        return f"{self.attachment.key}_values"

    def bound(self, identity: Identity) -> Outcome[Slots[T]]:
        """Binds attachment to scope owners holding it, then builds store rows."""
        return (
            self.attachment
            .bound(identity)
            .bind(
                lambda attached: (
                    self.scope
                    .filter(lambda scope: not issubclass(scope.owner_type, attached.owner))
                    .map(lambda scope: Error(refused(attached.name, scope.owner_type.__name__, (attached.owner.__name__,))))
                    .default_value(Ok(attached))
                )
            )
            .bind(lambda attached: self.slotted(identity, attached))
        )

    def slotted(self, identity: Identity, attached: Attached[T]) -> Outcome[Slots[T]]:
        """Builds store rows over bound attachment."""
        history = ValueHistory(
            form=Snapshot,
            read=lambda host: Snapshot.capture(attached.struct(host)),
            write=lambda host, snapshot: snapshot.apply(attached.struct(host)).outcome,
            storage=attached.struct,
            key=Slot.HISTORY,
        )
        stem = self.attachment.key
        vacant = operators.Unchanged(reason="No history entry at offset")
        inherited = bpy.types.PropertyGroup.bl_rna.properties

        def concluded[V](change: Option[V]) -> operators.Answer:
            return change.map(lambda _: operators.Done()).default_value(vacant)

        def now() -> datetime.datetime:
            return datetime.datetime.now(datetime.UTC)

        def retained() -> Outcome[Retention]:
            return self.retention.map(lambda read: read(identity)).default_with(lambda: Ok(Retention()))

        def recorded(context: bpy.types.Context, write: Callable[[Host], Outcome[Option[operators.Done]]]) -> Outcome[operators.Answer]:
            return (
                paired(hosted(attached.name, context), retained())
                .bind(lambda pair: history.replaced(pair[0], lambda: write(pair[0]), now(), pair[1]))
                .map(lambda change: change.default_value(operators.Unchanged(reason="No value changed")))
            )

        def stepped(context: bpy.types.Context, offset: int) -> Outcome[operators.Answer]:
            return paired(hosted(attached.name, context), retained()).bind(lambda pair: history.stepped(pair[0], offset, now(), pair[1])).map(concluded)

        def reset(group: bpy.types.PropertyGroup) -> Option[operators.Done]:
            for prop in group.bl_rna.properties:
                if prop.identifier not in inherited and not prop.is_readonly:
                    group.property_unset(prop.identifier)
                    group.path_resolve(prop.identifier, False).update()
            return Some(operators.Done())

        def pasting(call: operators.Call[operators.Empty], host: Host) -> Outcome[Option[operators.Done]]:
            return (
                paired(self.scope.map(lambda scope: scope.owners(call.context)).default_value(Ok((host,))), call.root(bpy.types.WindowManager))
                .bind(lambda pair: pasted(pair[1], tuple(map(attached.struct, pair[0]))))
                .map(lambda outcomes: Some(operators.Done()) if any(outcome.written for outcome in outcomes) else Nothing)
            )

        def reset_values(call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Resets section values to defaults."""
            return recorded(call.context, lambda host: Ok(reset(attached.struct(host))))

        def copy(call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Copies section values to the clipboard."""
            return (
                paired(hosted(attached.name, call.context).bind(lambda host: Snapshot.capture(attached.struct(host))), call.root(bpy.types.WindowManager))
                .map(lambda pair: copied(pair[1], pair[0]))
                .map(lambda _: operators.Done())
            )

        def paste(call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Pastes clipboard values into the section."""
            return recorded(call.context, lambda host: pasting(call, host))

        def older(call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Restores the older value set."""
            return stepped(call.context, 1)

        def newer(call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Restores the newer value set."""
            return stepped(call.context, -1)

        def restore(call: operators.Call[Picked]) -> Outcome[operators.Answer]:
            """Restores a value set from history."""
            return stepped(call.context, call.properties.offset)

        def forget(call: operators.Call[Picked]) -> Outcome[operators.Answer]:
            """Removes a value set from history."""
            return hosted(attached.name, call.context).bind(lambda host: history.removed(host, call.properties.offset)).map(concluded)

        def lock(call: operators.Call[Locked]) -> Outcome[operators.Answer]:
            """Keeps the value when presets apply."""
            return hosted(attached.name, call.context).map(lambda host: attached.toggled(host, call.properties.field)).map(lambda _: operators.Done())

        plain = {
            Verb.RESET: Verb.RESET.row(stem, operators.Empty, reset_values),
            Verb.COPY: Verb.COPY.row(stem, operators.Empty, copy),
            Verb.PASTE: Verb.PASTE.row(stem, operators.Empty, paste),
            Verb.OLDER: Verb.OLDER.row(stem, operators.Empty, older),
            Verb.NEWER: Verb.NEWER.row(stem, operators.Empty, newer),
        }
        picked = {Verb.RESTORE: Verb.RESTORE.row(stem, Picked, restore), Verb.FORGET: Verb.FORGET.row(stem, Picked, forget)}
        locking = Verb.LOCK.row(stem, Locked, lock)
        listing = self.presets.map(
            lambda preset: Listing(
                preset=preset,
                current=lambda context: hosted(attached.name, context).bind(lambda host: Snapshot.capture(attached.struct(host))),
                matches=lambda context, values: hosted(attached.name, context).bind(lambda host: accumulated(attached.projected(host, value).map(lambda ends: ends[2] == ends[0]) for value in values)),
                applied=lambda context, value: recorded(context, lambda host: attached.applied(host, value).map(lambda wrote: Some(operators.Done()) if wrote else Nothing)),
            )
        )
        slots = (Popover(panel=self.panel, text="", icon=Some("PRESET")), OperatorButton(target=plain[Verb.RESET], text=""))
        return accumulated(realized(row, identity) for row in slots).map(
            lambda tail: Slots(attached=attached, history=history, plain=plain, picked=picked, lock=locking, listing=listing, tail=tail, fields=self.fields)
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Acquires store verb rows, listing rows, and field rows."""
        return self.bound(identity).map(
            lambda slots: (
                *slots.plain.values(),
                *slots.picked.values(),
                slots.lock,
                *slots.listing.to_list(),
                *acquired(tuple(build(Member(root=slots.attached.form, path=name)) for name, build in self.fields.items())),
            )
        )


class Hosted[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Store beside member naming its host, the context root of the store's owner type while unset."""

    store: Store[T]
    host: Option[Member] = Nothing


class Values[T: bpy.types.PropertyGroup](Line, frozen=True, kw_only=True):
    """Store value sets in its popover."""

    store: Store[T]

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Acquires attachment and store operators."""
        return Ok((self.store.attachment, self.store))

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Realizes listing, locks, clipboard, and history rows."""

        def built(slots: Slots[T]) -> Outcome[RowDraw]:
            clipboard, steps = (tuple(OperatorButton(target=slots.plain[verb]) for verb in verbs) for verbs in ((Verb.COPY, Verb.PASTE), (Verb.OLDER, Verb.NEWER)))
            return paired(
                slots.listing.map(lambda listing: realized(listing, identity).map(Some)).default_value(Ok(Nothing)),
                paired(accumulated(realized(row, identity) for row in clipboard), accumulated(realized(row, identity) for row in steps)),
            ).map(lambda parts: partial(slots.shown, identity, parts))

        return self.store.bound(identity).bind(built)


# --- [TABLES] ---------------------------------------------------------------------------

TEMPLATES: Final = MappingProxyType({
    Verb.RESET: Template(icon="LOOP_BACK", undo=True),
    Verb.COPY: Template(icon="COPYDOWN", undo=False),
    Verb.PASTE: Template(icon="PASTEDOWN", undo=True),
    Verb.OLDER: Template(icon="BACK", undo="History Step"),
    Verb.NEWER: Template(icon="FORWARD", undo="History Step"),
    Verb.RESTORE: Template(icon="RECOVER_LAST", undo=True),
    Verb.FORGET: Template(icon="X", undo=True),
    Verb.APPLY: Template(icon="PRESET", undo=True),
    Verb.SAVE: Template(icon="ADD", undo=False),
    Verb.DELETE: Template(icon="X", undo=False),
    Verb.IMPORT: Template(icon="IMPORT", undo=False),
    Verb.EXPORT: Template(icon="EXPORT", undo=False),
    Verb.REPLACE: Template(icon="FILE_REFRESH", undo=False),
    Verb.RENAME: Template(icon="GREASEPENCIL", undo=False),
    Verb.PREVIOUS: Template(icon="BACK", undo=True),
    Verb.NEXT: Template(icon="FORWARD", undo=True),
    Verb.LOCK: Template(icon="LOCKED", undo=True),
})
# --- [OPERATIONS] -----------------------------------------------------------------------


def hosted(name: str, context: bpy.types.Context) -> Outcome[Host]:
    """Returns host from header context pointer, absent fault when unset."""
    return catch(lambda: getattr(context, name), exception=AttributeError)().map_error(lambda _: (Fault(reason=BlenderReason.ABSENT, subject="bpy.context", member=name),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Actions", "Hosted", "Listing", "Locked", "Named", "Picked", "Renamed", "Replacement", "Saved", "Slots", "Store", "Values", "Verb", "hosted"]
