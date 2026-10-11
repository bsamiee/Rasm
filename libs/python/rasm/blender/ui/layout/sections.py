"""Sections under a host header with their record fields, value-set slots, item sections, and preset listings."""

from collections.abc import Callable, Mapping
import datetime
from enum import Enum
from functools import partial
from itertools import product, starmap
from operator import itemgetter
from pathlib import Path
from types import MappingProxyType, MemberDescriptorType
import typing
from typing import Annotated, Protocol, runtime_checkable, Self

from bl_operators.presets import AddPresetBase
import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
from msgspec import inspect

from rasm.blender.commands import operators
from rasm.blender.commands.calls import called
from rasm.blender.data.rna.conditions import Condition, holds
from rasm.blender.data.rna.snapshots import Applied, Scoped, Snapshot
from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.extension.registration import Acquisitions, Registrable
from rasm.blender.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderResult
from rasm.blender.settings.attachments import Attached, Attachment, Host, Slot
from rasm.blender.settings.history import ValueHistory
from rasm.blender.settings.presets import copied, pasted, Preset, PresetName
from rasm.blender.settings.properties import RecordGroup
from rasm.blender.ui.layout.lists import keyed
from rasm.blender.ui.layout.rows import (
    acquired,
    alerted,
    fits,
    invoking,
    Line,
    located,
    Member,
    operated,
    OperatorButton,
    Owner,
    placed,
    Popover,
    Property,
    realized,
    refused,
    ResolvedMember,
    Row,
    RowContent,
    RowDraw,
    SurfaceDraw,
)
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Gated, Knot, Ramp, Rgb
from rasm.rendering.history import Retention

if typing.TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import IconItems

# --- [TYPES] ----------------------------------------------------------------------------

type Nest = Callable[[bpy.types.UILayout, bpy.types.bpy_struct[object], bpy.types.Context, str], None]
type Action = operators.Operator[Named] | operators.Operator[Renamed] | operators.Operator[Saved] | operators.Operator[Selected] | operators.Operator[operators.Empty]


class Verb(Enum):
    """Operator rows a store and its preset listing place, each keyed `<verb>_<stem>`."""

    RESET = ("Reset Values", "Reset every value of the section to its default", "LOOP_BACK", True)
    COPY = ("Copy Values", "Copy the section's values to the clipboard", "COPYDOWN", False)
    PASTE = ("Paste Values", "Paste the clipboard's values into the section's hosts", "PASTEDOWN", True)
    OLDER = ("Older Values", "Restore the older neighbour in the section's history", "BACK", "History Step")
    NEWER = ("Newer Values", "Restore the newer neighbour in the section's history", "FORWARD", "History Step")
    RESTORE = ("Restore Values", "Restore this value set of the section's history", "RECOVER_LAST", True)
    FORGET = ("Remove Entry", "Remove this value set from the section's history", "X", True)
    APPLY = ("Apply Preset", "Apply this preset", "PRESET", True)
    SAVE = ("Add Preset", "Add the current values as a preset", "ADD", False)
    DELETE = ("Remove Preset", "Remove this saved preset file", "X", False)
    IMPORT = ("Import Presets", "Copy preset files into the saved presets", "IMPORT", False)
    EXPORT = ("Export Preset", "Write this preset to a chosen file", "EXPORT", False)
    REPLACE = ("Replace Preset", "Replace this saved preset with the current values", "FILE_REFRESH", False)
    RENAME = ("Rename Preset", "Rename this saved preset file", "GREASEPENCIL", False)
    PREVIOUS = ("Previous Preset", "Apply the preceding preset", "BACK", True)
    NEXT = ("Next Preset", "Apply the following preset", "FORWARD", True)
    LOCK = ("Lock Value", "Keep or release this value when a preset applies", "LOCKED", True)

    def __init__(self, title: str, description: str, icon: "IconItems", undo: bool | str) -> None:
        """Holds the verb's title, description, icon, and undo choice."""
        self.title = title
        self.description = description
        self.icon = icon
        self.undo = undo

    def key(self, stem: Key) -> Key:
        """Operator key of the verb over a store or preset key."""
        return f"{self.name.lower()}_{stem}"

    def row[T: msgspec.Struct](
        self, stem: Key, form: type[T], run: Callable[[operators.Call[T]], BlenderResult[operators.Done]], invoke: Option[operators.Invoke[T, None]] = Nothing
    ) -> operators.Operator[T]:
        """Operator row of the verb over a stem, internal, drawn with the verb's icon, and undone as the verb states."""
        return operators.Operator(key=self.key(stem), title=self.title, description=self.description, icon=Some(self.icon), internal=True, form=form, run=run, undo=self.undo, invoke=invoke)


@runtime_checkable
class Section(Row, Registrable, Protocol):
    """Section a panel, popover, or append places."""

    @property
    def key(self) -> Key:
        """Key every idname of the section derives from."""

    @property
    def title(self) -> str:
        """Title the section's header draws."""

    @property
    def popovers(self) -> tuple["Single[operators.Empty]", ...]:
        """Values popovers of the section's store."""

    def realized(self, identity: Identity) -> BlenderResult["Drawn"]:
        """Realizes the section into its panel and layout panel paints."""


# --- [MODELS] ---------------------------------------------------------------------------


class Picked(msgspec.Struct, frozen=True, kw_only=True):
    """History entry a restore or remove row names."""

    offset: Annotated[int, msgspec.Meta(title="Entry", description="Signed one-based history offset")]


class Locked(msgspec.Struct, frozen=True, kw_only=True):
    """Record field a lock row flips."""

    field: Annotated[str, msgspec.Meta(title="Field", description="Key of the record field the lock keeps")]


class Named(msgspec.Struct, frozen=True, kw_only=True):
    """Preset an apply, remove, or export row names."""

    stem: Annotated[str, msgspec.Meta(title="Preset", description="Stem of the preset file")]


class Renamed(Named, frozen=True, kw_only=True):
    """Saved preset and its requested new file name."""

    name: PresetName


class Saved(msgspec.Struct, frozen=True, kw_only=True):
    """Name entered in the native save-preset dialog."""

    name: PresetName


class Selected(msgspec.Struct, frozen=True, kw_only=True):
    """Presets selected in shown order for independent file operations."""

    items: Annotated[tuple[Named, ...], msgspec.Meta(min_length=1)]


class Actions(msgspec.Struct, frozen=True, kw_only=True):
    """Operator row of each verb over one preset listing."""

    apply: operators.Operator[Named]
    save: operators.Operator[Saved]
    delete: operators.Operator[Selected]
    load: operators.Operator[operators.Empty]
    export: operators.Operator[Selected]
    replace: operators.Operator[Named]
    rename: operators.Operator[Renamed]
    previous: operators.Operator[operators.Empty]
    following: operators.Operator[operators.Empty]

    @property
    def verbs(self) -> dict[Verb, Action]:
        """Rows by verb in listing order."""
        return {
            Verb.APPLY: self.apply,
            Verb.SAVE: self.save,
            Verb.DELETE: self.delete,
            Verb.IMPORT: self.load,
            Verb.EXPORT: self.export,
            Verb.REPLACE: self.replace,
            Verb.RENAME: self.rename,
            Verb.PREVIOUS: self.previous,
            Verb.NEXT: self.following,
        }


class Drawn(msgspec.Struct, frozen=True, kw_only=True):
    """Section drawing for a panel's header enable, preset area, body, and layout panel over a holder."""

    head: Option[SurfaceDraw]
    tail: Option[SurfaceDraw]
    body: SurfaceDraw
    inline: RowDraw


class Listing[T: msgspec.Struct](Line, frozen=True, kw_only=True):
    """One catalog's native apply, file, and neighbouring-preset actions."""

    preset: Preset[T]
    current: Callable[[bpy.types.Context], BlenderResult[T]]
    matches: Callable[[bpy.types.Context, tuple[T, ...]], BlenderResult[tuple[bool, ...]]]
    applied: Callable[[bpy.types.Context, T], BlenderResult[operators.Done]]

    @property
    def key(self) -> Key:
        """Key of the menu `surfaces.menus` registers over the listing and a `Pulldown` opens."""
        return Listing.keyed(self.preset)

    @staticmethod
    def keyed[S: msgspec.Struct](preset: Preset[S]) -> Key:
        """Key of the menu over a preset's listing, read before any listing exists."""
        return f"{preset.key}_presets"

    def commands(self, identity: Identity) -> Actions:
        """Apply, add, remove, import, export, replace, rename, and neighbour rows."""
        stem, shown, json = self.preset.key, bpy.path.display_name, lambda _: (".json",)
        (renamed,) = frozenset(Renamed.__struct_fields__) - frozenset(Named.__struct_fields__)

        def user(stored: T | Path, key: str) -> BlenderResult[Path]:
            return Ok(stored) if isinstance(stored, Path) else Error((BlenderAbsentFault(owner=str(self.preset.folder(identity)), key=key),))

        def read(stored: T | Path) -> BlenderResult[T]:
            return self.preset.read(stored) if isinstance(stored, Path) else Ok(stored)

        def available(name: PresetName) -> BlenderResult[PresetName]:
            normalized = AddPresetBase.as_filename(name).casefold()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return Error((BlenderCollisionFault(request=name, error=str(self.preset.folder(identity))),)) if any(key.casefold() == normalized for key in self.preset.listed(identity)) else Ok(name)

        def neighbour(call: operators.Call[operators.Empty], step: int) -> BlenderResult[operators.Done]:
            values = tuple(read(value) for value in self.preset.listed(identity).values())
            return (
                paired(accumulated(values), self.matches(call.context, tuple(held for value in values for held in value.to_option().to_list())))
                .map(lambda pair: tuple(zip(pair[0], pair[1])))
                .bind(
                    lambda rows: (
                        Option
                        .of_optional(next((index for index, (_, matched) in enumerate(rows) if matched), None))
                        .map(lambda index: self.applied(call.context, rows[(index + step) % len(rows)][0]))
                        .default_with(lambda: self.applied(call.context, rows[0 if step > 0 else -1][0]) if rows else Ok(operators.Done()))
                    )
                )
            )

        replace = Verb.REPLACE.row(
            stem,
            Named,
            lambda call: (
                paired(self.preset.found(identity, call.record.stem).bind(lambda stored: user(stored, call.record.stem)), self.current(call.context))
                .bind(lambda pair: self.preset.written(*pair))
                .map(lambda _: operators.Done())
            ),
            invoke=Some(operators.Confirm[Named](message=Some(lambda _context, record: Ok(f"Replace the preset {shown(record.stem)}")), confirm_text=Some("Replace"))),
        )

        def saved(call: operators.Call[Saved]) -> BlenderResult[operators.Done]:
            normalized = AddPresetBase.as_filename(call.record.name).casefold()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            held = next(((key, value) for key, value in self.preset.listed(identity).items() if key.casefold() == normalized), None)
            match held:
                case (key, Path()):

                    def prompted(binding: tuple[str, operators.Writer]) -> BlenderResult[operators.Done]:
                        module, _, operation = binding[0].partition(".")
                        return called(getattr(getattr(bpy.ops, module), operation), msgspec.to_builtins(Named(stem=key)), context="INVOKE_DEFAULT").map(lambda _: operators.Done())

                    return replace.invocation(identity).bind(prompted)
                case (_, _):
                    return Error((BlenderCollisionFault(request=call.record.name, error=str(self.preset.folder(identity))),))
                case None:
                    return (
                        self
                        .current(call.context)
                        .bind(lambda value: self.preset.saved(identity, call.record.name, value))
                        .map(lambda path: operators.Done(notes=(("INFO", f"Added {shown(path.stem)}"),)))
                    )

        def destinations(path: Path, record: Selected) -> tuple[Path, ...]:
            return (path,) if len(record.items) == 1 else tuple(path.parent / f"{item.stem}{path.suffix}" for item in record.items)

        return Actions(
            apply=Verb.APPLY.row(stem, Named, lambda call: self.preset.found(identity, call.record.stem).bind(read).bind(lambda value: self.applied(call.context, value))),
            save=Verb.SAVE.row(stem, Saved, saved, invoke=Some(operators.Properties())),
            delete=Verb.DELETE.row(
                stem,
                Selected,
                lambda call: accumulated(self.preset.found(identity, item.stem).bind(lambda stored: user(stored, item.stem)).bind(self.preset.removed) for item in call.record.items).map(
                    lambda _: operators.Done()
                ),
                invoke=Some(
                    operators.Confirm[Selected](
                        message=Some(lambda _context, record: Ok("Remove " + ", ".join(shown(item.stem) for item in record.items))), confirm_text=Some("Remove"), icon="WARNING"
                    )
                ),
            ),
            load=Verb.IMPORT.row(
                stem,
                operators.Empty,
                lambda call: accumulated(self.preset.imported(identity, path) for path in call.paths).map(
                    lambda paths: operators.Done(notes=tuple(("INFO", f"Added {shown(path.stem)}") for path in paths))
                ),
                invoke=Some(operators.Open[operators.Empty](extensions=json)),
            ),
            export=Verb.EXPORT.row(
                stem,
                Selected,
                lambda call: accumulated(
                    self.preset.found(identity, item.stem).bind(lambda stored: self.preset.exported(stored, path) if isinstance(stored, Path) else self.preset.written(path, stored))
                    for item, path in zip(call.record.items, call.paths)
                ).map(lambda paths: operators.Done(notes=tuple(("INFO", f"Wrote {path}") for path in paths))),
                invoke=Some(operators.Save[Selected](extensions=json, destinations=Some(destinations))),
            ),
            replace=replace,
            rename=msgspec.structs.replace(
                Verb.RENAME.row(
                    stem,
                    Renamed,
                    lambda call: (
                        paired(self.preset.found(identity, call.record.stem).bind(lambda stored: user(stored, call.record.stem)), available(call.record.name))
                        .bind(lambda pair: self.preset.renamed(*pair))
                        .map(lambda _: operators.Done())
                    ),
                    invoke=Some(operators.Properties()),
                ),
                draw=Some(lambda layout, _context, properties: layout.prop(properties, renamed)),
            ),
            previous=Verb.PREVIOUS.row(stem, operators.Empty, lambda call: neighbour(call, -1)),
            following=Verb.NEXT.row(stem, operators.Empty, lambda call: neighbour(call, 1)),
        )

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Realizes native apply entries, single-item file actions, and add/import asks."""
        commands = self.commands(identity)
        apply, delete, export = commands.apply, commands.delete, commands.export
        footer = (OperatorButton(target=commands.save, text=Some("")), OperatorButton(target=commands.load, text=Some("")))

        def draw(parts: tuple[tuple[str, ...], tuple[RowDraw, ...]]) -> RowDraw:
            (applying, deleting, exporting), buttons = parts

            def entry(layout: bpy.types.UILayout, context: bpy.types.Context, stem: str, kept: T | Path, value: BlenderResult[T], matched: Option[bool]) -> None:
                line, chosen = layout.row(align=True), Named(stem=stem)
                alerted(
                    line,
                    value.bind(lambda _: operated(line.operator(applying, text=bpy.path.display_name(stem), icon="CHECKMARK" if matched == Some(True) else "BLANK1"), applying)).bind(
                        lambda properties: operators.Own(command=apply, values=chosen).invocation(identity).bind(lambda placement: placement[1](properties))
                    ),
                )

                def actions(row: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
                    alerted(
                        row,
                        operated(row.operator(exporting, text="", icon="EXPORT"), exporting).bind(
                            lambda properties: operators.Own(command=export, values=Selected(items=(chosen,))).invocation(identity).bind(lambda placement: placement[1](properties))
                        ),
                    )
                    if isinstance(kept, Path):
                        alerted(
                            row,
                            operated(row.operator(deleting, text="", icon="X"), deleting).bind(
                                lambda properties: operators.Own(command=delete, values=Selected(items=(chosen,))).invocation(identity).bind(lambda placement: placement[1](properties))
                            ),
                        )

                invoking(actions)(line, context, context)

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                layout.emboss = "PULLDOWN_MENU"
                catalog = tuple((stem, kept, self.preset.read(kept) if isinstance(kept, Path) else Ok(kept)) for stem, kept in self.preset.listed(identity).items())
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

        return paired(accumulated(identity.name(Name.OPERATOR, command.key) for command in (apply, delete, export)), accumulated(realized(row, identity) for row in footer)).map(draw)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires the listing's rows."""
        return Ok(tuple(self.commands(identity).verbs.values()))


class Slots[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Store bound to the identity: its attachment, history, operator rows, listing, and header slot draws."""

    attached: Attached[T]
    history: ValueHistory[T, Host, None]
    plain: dict[Verb, operators.Operator[operators.Empty]]
    picked: dict[Verb, operators.Operator[Picked]]
    lock: operators.Operator[Locked]
    listing: Option[Listing[T]]
    tail: tuple[RowDraw, ...]
    fields: Mapping[MemberDescriptorType, RowContent]


class Store[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Attached record's value sets: reset, presets with locks, clipboard, and history."""

    attachment: Attachment[T]
    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing
    presets: Option[Preset[T]] = Nothing
    scope: Option[Scoped[Host]] = Nothing
    fields: Mapping[MemberDescriptorType, RowContent] = msgspec.field(default_factory=dict)

    @property
    def panel(self) -> Key:
        """Key of the values popover."""
        return f"{self.attachment.key}_values"

    @property
    def popover(self) -> "Single[operators.Empty]":
        """Values popover section that `surfaces.panels` registers."""
        return Single(key=self.panel, title="Values", rows=(Values(store=self),))

    def bound(self, identity: Identity) -> BlenderResult[Slots[T]]:
        """Binds the attachment, admits a scope whose owners hold it, and builds the verb rows and header slots."""
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

    def slotted(self, identity: Identity, attached: Attached[T]) -> BlenderResult[Slots[T]]:
        """Builds the store's rows over its bound attachment."""
        history = ValueHistory(form=attached.group.record, read=attached.read, write=attached.write, storage=attached.struct, key=Slot.HISTORY)
        stem = self.attachment.key

        def managed(context: bpy.types.Context) -> BlenderResult[bpy.types.WindowManager]:
            return Option.of_optional(context.window_manager).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="window_manager"),))

        def now() -> datetime.datetime:
            return datetime.datetime.now(datetime.UTC)

        def retained() -> BlenderResult[Retention]:
            return self.retention.map(lambda read: read(identity)).default_with(lambda: Ok(Retention()))

        def recorded(context: bpy.types.Context, write: Callable[[Host], BlenderResult[operators.Done]]) -> BlenderResult[operators.Done]:
            return paired(hosted(attached.name, context), retained()).bind(lambda pair: history.replaced(pair[0], lambda: write(pair[0]), now(), pair[1]))

        def stepped(offset: int) -> Callable[[operators.Call[operators.Empty]], BlenderResult[operators.Done]]:
            return lambda call: paired(hosted(attached.name, call.context), retained()).bind(lambda pair: history.stepped(pair[0], offset, now(), pair[1])).map(lambda _: operators.Done())

        def scoped(context: bpy.types.Context, host: Host) -> BlenderResult[operators.Done]:
            def done(outcomes: tuple[Applied, ...]) -> operators.Done:
                missing = tuple(BlenderAbsentFault(owner=attached.name, key=path) for outcome in outcomes for path in outcome.skipped)
                return operators.Done(skipped=Some((missing[0], *missing[1:])) if missing else Nothing)

            return (
                paired(self.scope.map(lambda scope: scope.owners(context)).default_value(Ok((host,))), managed(context))
                .bind(lambda pair: pasted(pair[1], tuple(map(attached.struct, pair[0]))))
                .bind(lambda outcomes: accumulated(outcome.outcome for outcome in outcomes).map(lambda _: done(outcomes)))
            )

        plain = {
            Verb.RESET: Verb.RESET.row(stem, operators.Empty, lambda call: recorded(call.context, lambda host: attached.group.reset(attached.struct(host)).map(lambda _: operators.Done()))),
            Verb.COPY: Verb.COPY.row(
                stem,
                operators.Empty,
                lambda call: (
                    paired(hosted(attached.name, call.context).bind(lambda host: Snapshot.capture(attached.struct(host))), managed(call.context))
                    .map(lambda pair: copied(pair[1], pair[0]))
                    .map(lambda _: operators.Done())
                ),
            ),
            Verb.PASTE: Verb.PASTE.row(stem, operators.Empty, lambda call: recorded(call.context, lambda host: scoped(call.context, host))),
            Verb.OLDER: Verb.OLDER.row(stem, operators.Empty, stepped(1)),
            Verb.NEWER: Verb.NEWER.row(stem, operators.Empty, stepped(-1)),
        }
        picked = {
            Verb.RESTORE: Verb.RESTORE.row(
                stem,
                Picked,
                lambda call: paired(hosted(attached.name, call.context), retained()).bind(lambda found: history.stepped(found[0], call.record.offset, now(), found[1])).map(lambda _: operators.Done()),
            ),
            Verb.FORGET: Verb.FORGET.row(stem, Picked, lambda call: hosted(attached.name, call.context).bind(lambda host: history.removed(host, call.record.offset)).map(lambda _: operators.Done())),
        }
        lock = Verb.LOCK.row(stem, Locked, lambda call: hosted(attached.name, call.context).map(lambda host: attached.toggled(host, call.record.field)).map(lambda _: operators.Done()))
        listing = self.presets.map(
            lambda preset: Listing(
                preset=preset,
                current=lambda context: hosted(attached.name, context).bind(attached.read),
                matches=lambda context, values: (
                    hosted(attached.name, context)
                    .bind(lambda host: attached.read(host).map(lambda held: (held, attached.locks(host))))
                    .map(lambda pair: tuple(attached.kept(pair[1], pair[0], value) == pair[0] for value in values))
                ),
                applied=lambda context, value: recorded(context, lambda host: attached.applied(host, value).map(lambda _: operators.Done())),
            )
        )
        slots = (Popover(panel=self.panel, text=Some(""), icon=Some("PRESET")), OperatorButton(target=plain[Verb.RESET], text=Some("")))
        return accumulated(realized(row, identity) for row in slots).map(
            lambda tail: Slots(attached=attached, history=history, plain=plain, picked=picked, lock=lock, listing=listing, tail=tail, fields=self.fields)
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires the store's verb rows and its listing's rows."""
        return self.bound(identity).map(lambda slots: (*slots.plain.values(), *slots.picked.values(), slots.lock, *slots.listing.to_list(), *acquired(tuple(self.fields.values()))))


class Hosted[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Store beside the member naming its host."""

    host: Member
    store: Store[T]


class Values[T: msgspec.Struct](Line, frozen=True, kw_only=True):
    """Store's value sets in its popover: presets and locks, clipboard, and history."""

    store: Store[T]

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Retains the store supplying this value editor's native commands."""
        return Ok((self.store.attachment, self.store))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Realizes the listing, locks, clipboard and step buttons, and the history entries of the store."""

        def built(slots: Slots[T]) -> BlenderResult[RowDraw]:
            attached, history = slots.attached, slots.history
            fields = slots.listing.map(lambda _: tuple(row for row in msgspec.structs.fields(attached.group.record) if row.name in attached.group.struct.bl_rna.properties)).default_value(())
            clipboard, steps = (tuple(OperatorButton(target=slots.plain[verb]) for verb in verbs) for verbs in ((Verb.COPY, Verb.PASTE), (Verb.OLDER, Verb.NEWER)))

            def draw(parts: tuple[tuple[Option[RowDraw], tuple[tuple[RowDraw, ...], tuple[RowDraw, ...]]], tuple[str, tuple[str, str]]]) -> RowDraw:
                (listing, (copying, stepping)), (locking, (restoring, forgetting)) = parts

                def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                    found = hosted(attached.name, context)
                    for each in listing.to_list():
                        each(layout, holder, context)
                    for host in found.to_option().to_list():
                        locks, group = attached.locks(host), attached.struct(host)
                        for row in fields:
                            icon: IconItems = "LOCKED" if row.name in locks else "UNLOCKED"
                            alerted(
                                layout,
                                operated(layout.operator(locking, text=group.bl_rna.properties[row.name].name, icon=icon), locking).bind(
                                    lambda properties: operators.Own(command=slots.lock, values=Locked(field=row.name)).invocation(identity).bind(lambda placement: placement[1](properties))
                                ),
                            )
                    layout.separator()
                    for draws in (copying, stepping):
                        line = layout.row(align=True)
                        for each in draws:
                            each(line, holder, context)
                    held = found.bind(history.held)
                    entries = held.map(
                        lambda value: tuple((index, moment) for index, (moment, _) in enumerate(value.past, 1)) + tuple((-index, moment) for index, (moment, _) in enumerate(value.future, 1))
                    ).default_value(())
                    for offset, moment in entries:
                        line, chosen = layout.row(align=True), Picked(offset=offset)
                        alerted(
                            line,
                            paired(
                                operated(line.operator(restoring, text=f"{moment.astimezone():%X}", icon="BACK" if offset > 0 else "FORWARD"), restoring).bind(
                                    lambda properties: operators.Own(command=slots.picked[Verb.RESTORE], values=chosen).invocation(identity).bind(lambda placement: placement[1](properties))
                                ),
                                operated(line.operator(forgetting, text="", icon="X"), forgetting).bind(
                                    lambda properties: operators.Own(command=slots.picked[Verb.FORGET], values=chosen).invocation(identity).bind(lambda placement: placement[1](properties))
                                ),
                            ),
                        )
                    alerted(layout, held)

                return shown

            return paired(
                paired(
                    slots.listing.map(lambda listing: realized(listing, identity).map(Some)).default_value(Ok(Nothing)),
                    paired(accumulated(realized(row, identity) for row in clipboard), accumulated(realized(row, identity) for row in steps)),
                ),
                paired(identity.name(Name.OPERATOR, slots.lock.key), paired(identity.name(Name.OPERATOR, slots.picked[Verb.RESTORE].key), identity.name(Name.OPERATOR, slots.picked[Verb.FORGET].key))),
            ).map(draw)

        return self.store.bound(identity).bind(built)


class Assembly[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Section's realized header enable, header rows, body rows, and store fields."""

    name: str
    gate: Option[tuple[ResolvedMember, RowDraw]]
    header: tuple[RowDraw, ...]
    rows: tuple[RowDraw, ...]
    fields: tuple[RowDraw, ...]
    nest: Option[Nest]
    slots: Option[Slots[T]]

    @classmethod
    def of(cls, identity: Identity, key: Key, rows: tuple[RowContent, ...], header: tuple[RowContent, ...], enable: Option[Member], slots: Option[Slots[T]]) -> BlenderResult[Self]:
        """Realizes a section's parts once at acquisition, its rows and store scalars on one grid."""
        walk = slots.map(lambda held: fields(held.attached.group, identity, held.fields).map(Some)).default_value(Ok(Nothing))
        gate = enable.map(
            lambda member: member.resolved(bpy.types.BoolProperty).bind(lambda held: realized(Property(target=held, text=Some("")), identity).map(lambda draw: Some((held, draw))))
        ).default_value(Ok(Nothing))

        def built(parts: tuple[tuple[str, Option[tuple[ResolvedMember, RowDraw]]], tuple[tuple[RowDraw, ...], Option[tuple[tuple[RowContent, ...], Nest]]]]) -> BlenderResult[Self]:
            (name, gated), (heads, walked) = parts
            scalars = walked.map(itemgetter(0)).default_value(())
            count = len(rows)
            return placed((*rows, *scalars), identity).map(
                lambda draws: cls(name=name, gate=gated, header=heads, rows=draws[:count], fields=draws[count:], nest=walked.map(itemgetter(1)), slots=slots)
            )

        return paired(paired(identity.name(Name.ATTACHMENT, key), gate), paired(accumulated(realized(row, identity) for row in header), walk)).bind(built)

    def head(self, layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
        """Draws the enable with no text."""
        for _, draw in self.gate.to_list():
            draw(layout, holder, context)

    def tail(self, layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", host: Option[Host], context: bpy.types.Context) -> None:
        """Draws the header rows, then the store slots with the host as the context pointer."""
        line = layout.row(align=True)
        for draw in self.header:
            draw(line, holder, context)
        for slots, found in product(self.slots.to_list(), host.to_list()):
            line.context_pointer_set(slots.attached.name, found)
            for draw in slots.tail:
                draw(line, holder, context)

    def body(self, layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", host: Option[Host], context: bpy.types.Context, prefix: str) -> None:
        """Draws the rows over the holder and the store fields over the host's group, inactive while the enable is off."""
        layout.use_property_split, layout.use_property_decorate = True, True
        layout.active = layout.active and all(starmap(getattr, self.gate.bind(lambda pair: located(pair[0], holder, context)).to_list()))
        for draw in self.rows:
            draw(layout, holder, context)
        for slots, found in product(self.slots.to_list(), host.to_list()):
            group = slots.attached.struct(found)
            for draw in self.fields:
                draw(layout, group, context)
            for nest in self.nest.to_list():
                nest(layout, group, context, prefix)

    def framed(
        self,
        layout: bpy.types.UILayout,
        idname: str,
        title: str,
        holder: "bpy.types.bpy_struct[object]",
        host: Option[Host],
        context: bpy.types.Context,
        opened: Option[tuple[Owner, str]],
        *,
        active: bool = True,
    ) -> Option[bpy.types.UILayout]:
        """Draws a layout panel's header and answers its body while open."""
        header, body = opened.map(lambda pair: layout.panel_prop(*pair)).default_with(lambda: layout.panel(idname, default_closed=True))
        header.active = header.active and active
        header.context_pointer_set(self.name, holder)
        self.head(header, holder, context)
        header.label(text=title)
        right = header.row(align=True)
        right.alignment = "RIGHT"
        self.tail(right, holder, host, context)
        opened_body = Option.of_optional(body)
        for shown in opened_body.to_list():
            shown.active = shown.active and active
            shown.context_pointer_set(self.name, holder)
        return opened_body


class Single[T: msgspec.Struct = operators.Empty](msgspec.Struct, frozen=True, kw_only=True):
    """Rows under a host header over one holder, with an optional store of its host."""

    key: Key
    title: str
    rows: tuple[RowContent, ...] = ()
    header: tuple[RowContent, ...] = ()
    enable: Option[Member] = Nothing
    store: Option[Hosted[T]] = Nothing
    condition: Option[Condition] = Nothing

    @property
    def popovers(self) -> tuple["Single[operators.Empty]", ...]:
        """Values popover of the store."""
        return tuple(hosted.store.popover for hosted in self.store.to_list())

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires the lists and nested sections among the rows and the store's rows."""
        return Ok((*acquired((*self.rows, *self.header)), *(row for hosted in self.store.to_list() for row in (hosted.store.attachment, hosted.store))))

    def realized(self, identity: Identity) -> BlenderResult[Drawn]:
        """Realizes the section into its panel and layout panel paints."""
        hosting = self.store.map(lambda hosted: paired(hosted.host.resolved().bind(lambda bound: fits(bound, (hosted.store.attachment.owner,))), hosted.store.bound(identity)).map(Some)).default_value(
            Ok(Nothing)
        )

        def drawn(parts: tuple[Assembly[T], Option[ResolvedMember]]) -> Drawn:
            frame, host = parts

            def found(holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> Option[Host]:
                return host.bind(lambda bound: located(bound, holder, context)).bind(lambda pair: Option.of_optional(getattr(*pair)))

            def body(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                layout.active = layout.active and all(holds(required, context, Some(holder)) for required in self.condition.to_list())
                frame.body(layout, holder, found(holder, context), context, frame.name)

            def inline(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                active = all(holds(required, context, Some(holder)) for required in self.condition.to_list())
                host = found(holder, context)
                for shown in frame.framed(layout, frame.name, self.title, holder, host, context, Nothing, active=active).to_list():
                    frame.body(shown, holder, host, context, frame.name)

            return Drawn(
                head=frame.gate.map(lambda _: lambda layout, context: frame.head(layout, context, context)),
                tail=Some(lambda layout, context: frame.tail(layout, context, found(context, context), context)) if self.header or self.store.is_some() else Nothing,
                body=lambda layout, context: body(layout, context, context),
                inline=inline,
            )

        return hosting.bind(lambda held: Assembly.of(identity, self.key, self.rows, self.header, self.enable, held.map(itemgetter(1))).map(lambda frame: (frame, held.map(itemgetter(0))))).map(drawn)

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Answers the section as a layout panel row."""
        return self.realized(identity).map(lambda drawn: drawn.inline)


class Each[T: msgspec.Struct = operators.Empty](msgspec.Struct, frozen=True, kw_only=True):
    """One section per item of a host collection, the item the holder and the store's host."""

    key: Key
    title: str
    collection: Member
    rows: tuple[RowContent, ...] = ()
    header: tuple[RowContent, ...] = ()
    enable: Option[Member] = Nothing
    store: Option[Store[T]] = Nothing
    open: Option[str] = Nothing
    empty: tuple[RowContent, ...] = ()
    condition: Option[Condition] = Nothing

    @property
    def popovers(self) -> tuple[Single[operators.Empty], ...]:
        """Values popover of the store."""
        return tuple(store.popover for store in self.store.to_list())

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires the lists and nested sections among the rows and the store's rows."""
        return Ok((*acquired((*self.rows, *self.header, *self.empty)), *(row for store in self.store.to_list() for row in (store.attachment, store))))

    def realized(self, identity: Identity) -> BlenderResult[Drawn]:
        """Realizes one layout panel per item, the store bound to the item type."""

        def items(
            bound: ResolvedMember[bpy.types.CollectionProperty],
        ) -> BlenderResult[tuple[ResolvedMember[bpy.types.CollectionProperty], bpy.types.StringProperty, Option[ResolvedMember], Option[Slots[T]]]]:
            match bound.descriptor.prop.fixed_type:
                case None:
                    return Error((BlenderAbsentFault(owner=bound.descriptor.path, key="fixed_type"),))
                case item:
                    opened = self.open.map(lambda path: Member(root=type(item), path=path).resolved(bpy.types.BoolProperty).map(Some)).default_value(Ok(Nothing))
                    stored = self.store.map(
                        lambda store: (
                            store.bound(identity) if isinstance(item, store.attachment.owner) else Error(refused(bound.descriptor.path, type(item).__name__, (store.attachment.owner.__name__,)))
                        ).map(Some)
                    ).default_value(Ok(Nothing))
                    return paired(
                        Option.of_optional(item.bl_rna.name_property).to_result_with(lambda: (BlenderAbsentFault(owner=bound.descriptor.path, key="name_property"),)),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                        paired(opened, stored),
                    ).map(lambda pair: (bound, pair[0], *pair[1]))

        def drawn(parts: tuple[tuple[ResolvedMember, bpy.types.StringProperty, Option[ResolvedMember], Option[Slots[T]]], tuple[Assembly[T], tuple[RowDraw, ...]]]) -> Drawn:
            ((collection, named, opened, _), (frame, empties)) = parts

            def listed(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for owner, key in located(collection, holder, context).to_list():
                    members = getattr(owner, key)
                    for draw in () if members else empties:
                        draw(layout, holder, context)
                    for item in members:
                        idname = f"{frame.name}_{keyed(item)}"
                        title = getattr(item, named.identifier)
                        active = all(holds(required, context, Some(item)) for required in self.condition.to_list())
                        for shown in frame.framed(layout, idname, title, item, Some(item), context, opened.bind(partial(located, holder=item, context=context)), active=active).to_list():
                            frame.body(shown, item, Some(item), context, idname)

            return Drawn(head=Nothing, tail=Nothing, body=lambda layout, context: listed(layout, context, context), inline=listed)

        return (
            self.collection
            .resolved(bpy.types.CollectionProperty)
            .bind(items)
            .bind(lambda found: paired(Assembly.of(identity, self.key, self.rows, self.header, self.enable, found[3]), placed(self.empty, identity)).map(lambda pair: (found, pair)))
            .map(drawn)
        )

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Answers the item sections as one row."""
        return self.realized(identity).map(lambda drawn: drawn.inline)


# --- [OPERATIONS] -----------------------------------------------------------------------


def hosted(name: str, context: bpy.types.Context) -> BlenderResult[Host]:
    """Answers the host a header's context pointer names, or `absent` where no header set it."""
    return catch(lambda: getattr(context, name), exception=AttributeError)().map_error(lambda _: (BlenderAbsentFault(owner="bpy.context", key=name),))


def fields[T: msgspec.Struct, S: "bpy.types.bpy_struct[object]"](
    group: RecordGroup[T, S], identity: Identity, replacements: Mapping[MemberDescriptorType, RowContent] = MappingProxyType({})
) -> BlenderResult[tuple[tuple[RowContent, ...], Nest]]:
    """Realizes native fields as scalar rows and nested record, union, or collection panels."""
    enabled, value = Gated.__struct_fields__
    supplied = {slot.__name__: row for slot, row in replacements.items() if issubclass(typing.get_origin(group.record) or group.record, slot.__objclass__)}

    def inside(found: tuple[tuple[RowContent, ...], Nest]) -> BlenderResult[Nest]:
        rows, nest = found

        def stacked(draws: tuple[RowDraw, ...]) -> Nest:
            def draw(layout: bpy.types.UILayout, group: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
                for each in draws:
                    each(layout, group, context)
                nest(layout, group, context, prefix)

            return draw

        return placed(rows, identity).map(stacked)

    def split(row: msgspec.structs.FieldInfo, kind: inspect.Type, struct: "type[bpy.types.bpy_struct[object]]", *, panel: bool = True) -> BlenderResult[Property | Nest]:
        while isinstance(kind, inspect.Metadata):
            kind = kind.type
        key = row.name

        def nested(record: type[msgspec.Struct], native: type[bpy.types.PropertyGroup]) -> BlenderResult[Nest]:
            return fields(RecordGroup(record=record, struct=native), identity, replacements).bind(inside)

        def paneled(nest: Nest, *, gated: bool) -> Nest:
            def draw(layout: bpy.types.UILayout, group: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
                held, idname = getattr(group, row.name), f"{prefix}_{row.name}"
                header, body = layout.panel(idname, default_closed=True)
                header.use_property_split = False
                if gated:
                    header.prop(held, enabled, text="")
                header.label(text=group.bl_rna.properties[row.name].name)
                for shown in Option.of_optional(body).to_list():
                    shown.active = bool(getattr(held, enabled)) if gated else True
                    nest(shown, held if gated else group, context, idname)

            return draw

        def reached(nest: Nest) -> Nest:
            def draw(layout: bpy.types.UILayout, owner: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
                nest(layout, getattr(owner, key), context, prefix)

            return draw

        def chosen(cases: tuple[tuple[str, Nest], ...], native: type[bpy.types.PropertyGroup]) -> Nest:
            by = dict(cases)
            tag_field = next(name for name, definition in native.__annotations__.items() if definition.function is bpy.props.EnumProperty)

            def draw(layout: bpy.types.UILayout, owner: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
                group = getattr(owner, key)
                layout.prop(group, tag_field)
                picked = getattr(group, tag_field)
                by[picked](layout, getattr(group, picked), context, f"{prefix}_{picked}")

            return draw

        def each(nest: Nest) -> Nest:
            def draw(layout: bpy.types.UILayout, owner: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
                for index, item in enumerate(getattr(owner, key)):
                    idname = f"{prefix}_{index}"
                    header, body = layout.panel(idname, default_closed=True)
                    title = Option.of_optional(item.bl_rna.name_property).map(lambda prop: getattr(item, prop.identifier)).default_value(f"{owner.bl_rna.properties[key].name} {index + 1}")
                    header.label(text=title)
                    for shown in Option.of_optional(body).to_list():
                        nest(shown, item, context, idname)

            return draw

        match kind:
            case inspect.StructType(cls=declaration, fields=native) if (typing.get_origin(declaration) or declaration) is Gated:
                inner = next(field.type for field in native if field.name == value)
                while isinstance(inner, inspect.Metadata):
                    inner = inner.type
                nested_gate = isinstance(inner, inspect.StructType) and (typing.get_origin(inner.cls) or inner.cls) is Gated
                field = next(field for field in msgspec.structs.fields(declaration) if field.name == value)
                return split(field, inner, struct.__annotations__[key].keywords["type"], panel=False).bind(
                    lambda child: (
                        Ok(Property(target=row))
                        if isinstance(child, Property) and not nested_gate
                        else (realized(child, identity).map(lambda draw: lambda layout, holder, context, _prefix: draw(layout, holder, context)) if isinstance(child, Property) else Ok(child)).map(
                            lambda nest: paneled(nest, gated=True)
                        )
                    )
                )
            case inspect.StructType(cls=inner) if (typing.get_origin(inner) or inner) not in {Rgb, Ramp}:
                found = nested(inner, struct.__annotations__[key].keywords["type"]).map(reached)
            case inspect.DictType(key_type=inspect.EnumType(cls=enum)):
                native = struct.__annotations__[key].keywords["type"]
                found = (
                    accumulated(split(entry, inspect.type_info(entry.type), native).map(lambda part: (entry, part)) for entry in RecordGroup.entries(row, enum))
                    .map(lambda parts: joined(parts, {}))
                    .bind(inside)
                    .map(reached)
                )
            case inspect.UnionType(types=(inspect.CustomType(cls=target), inspect.NoneType()) | (inspect.NoneType(), inspect.CustomType(cls=target))) if issubclass(target, bpy.types.ID):
                return Ok(Property(target=row))
            case inspect.UnionType(types=alternatives):
                cases: list[BlenderResult[tuple[str, Nest]]] = []
                for alternative in alternatives:
                    case = alternative
                    while isinstance(case, inspect.Metadata):
                        case = case.type
                    if isinstance(case, inspect.StructType):
                        name = (typing.get_origin(case.cls) or case.cls).__name__
                        cases.append(nested(case.cls, struct.__annotations__[key].keywords["type"].__annotations__[name].keywords["type"]).map(lambda nest: (name, nest)))
                    else:
                        cases.append(Error(refused(row.name, type(case).__name__, (inspect.StructType.__name__,))))
                found = accumulated(cases).map(lambda parts: chosen(parts, struct.__annotations__[key].keywords["type"]))
            case inspect.VarTupleType(item_type=inspect.StructType(cls=inner)) if (typing.get_origin(inner) or inner) is not Knot:
                found = nested(inner, struct.__annotations__[key].keywords["type"]).map(each)
            case _:
                return Ok(Property(target=row))
        return found.map(lambda nest: paneled(nest, gated=False) if panel else nest)

    def joined(parts: tuple[tuple[msgspec.structs.FieldInfo, Property | Nest], ...], supplied: Mapping[str, RowContent]) -> tuple[tuple[RowContent, ...], Nest]:
        nests = tuple(part for _, part in parts if not isinstance(part, Property))

        def draw(layout: bpy.types.UILayout, group: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
            for nest in nests:
                nest(layout, group, context, prefix)

        return tuple(supplied.get(row.name, part) for row, part in parts if isinstance(part, Property)), draw

    rows = tuple(row for row in msgspec.structs.fields(group.record) if row.name in group.struct.__annotations__ or row.name in group.struct.bl_rna.properties)
    kinds = inspect.multi_type_info(row.type for row in rows)
    return accumulated(split(row, kind, group.struct).map(lambda part: (row, part)) for row, kind in zip(rows, kinds)).map(lambda parts: joined(parts, supplied))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Assembly",
    "Drawn",
    "Each",
    "Hosted",
    "Listing",
    "Locked",
    "Named",
    "Nest",
    "Picked",
    "Renamed",
    "Saved",
    "Section",
    "Selected",
    "Single",
    "Slots",
    "Store",
    "Values",
    "Verb",
    "fields",
    "hosted",
]
