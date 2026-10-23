"""Header sections with record fields, value sets, and preset listings."""

from collections.abc import Callable, Mapping
import datetime
from enum import Enum
from functools import partial
from itertools import product, starmap
from pathlib import Path
from types import MappingProxyType, MemberDescriptorType
import typing
from typing import Annotated, Protocol, runtime_checkable, Self

from bl_operators.presets import AddPresetBase
import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands import operators
from blender.bpy.commands.calls import called
from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import Native
from blender.bpy.data.rna.snapshots import Scoped, Snapshot
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderResult
from blender.bpy.settings.attachments import Attached, Attachment, Host, Slot
from blender.bpy.settings.history import ValueHistory
from blender.bpy.settings.presets import copied, pasted, Preset, PresetName, removed
from blender.bpy.settings.properties import RecordGroup
from blender.bpy.settings.shapes import ENABLED, Entries, field_shapes, Gate, Nested, Records, Shape, Tagged
from blender.bpy.ui.layout.lists import keyed
from blender.bpy.ui.layout.rows import (
    acquired,
    alerted,
    fits,
    invoking,
    Line,
    located,
    Member,
    OperatorButton,
    placed,
    Popover,
    pressed,
    Property,
    realized,
    refused,
    Resolved,
    ResolvedMember,
    Row,
    RowContent,
    RowDraw,
    SurfaceDraw,
)
from blender.imaging.faults import accumulated, paired
from blender.rendering.history import Retention

if typing.TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import IconItems

# --- [TYPES] ----------------------------------------------------------------------------

type Nest = Callable[[bpy.types.UILayout, bpy.types.bpy_struct[object], bpy.types.Context, str], None]
type Action = operators.Operator[Named] | operators.Operator[Renamed] | operators.Operator[Saved] | operators.Operator[Selected] | operators.Operator[operators.Empty]


class Verb(Enum):
    """Store and listing operator rows, each keyed `<verb>_<stem>`."""

    RESET = ("Reset Values", "Reset section values to defaults", "LOOP_BACK", True)
    COPY = ("Copy Values", "Copy section values to clipboard", "COPYDOWN", False)
    PASTE = ("Paste Values", "Paste clipboard values into section", "PASTEDOWN", True)
    OLDER = ("Older Values", "Restore older value set", "BACK", "History Step")
    NEWER = ("Newer Values", "Restore newer value set", "FORWARD", "History Step")
    RESTORE = ("Restore Values", "Restore value set from history", "RECOVER_LAST", True)
    FORGET = ("Remove Entry", "Remove value set from history", "X", True)
    APPLY = ("Apply Preset", "Apply preset", "PRESET", True)
    SAVE = ("Add Preset", "Save section values as preset", "ADD", False)
    DELETE = ("Remove Preset", "Delete preset files", "X", False)
    IMPORT = ("Import Presets", "Copy files into saved presets", "IMPORT", False)
    EXPORT = ("Export Preset", "Write presets to files", "EXPORT", False)
    REPLACE = ("Replace Preset", "Replace preset with section values", "FILE_REFRESH", False)
    RENAME = ("Rename Preset", "Rename preset file", "GREASEPENCIL", False)
    PREVIOUS = ("Previous Preset", "Apply previous preset", "BACK", True)
    NEXT = ("Next Preset", "Apply next preset", "FORWARD", True)
    LOCK = ("Lock Value", "Keep value when presets apply", "LOCKED", True)

    def __init__(self, title: str, description: str, icon: "IconItems", undo: bool | str) -> None:
        """Holds verb operator fields."""
        self.title = title
        self.description = description
        self.icon = icon
        self.undo = undo

    def key(self, stem: Key) -> Key:
        """Returns operator key over store or preset key."""
        return f"{self.name.lower()}_{stem}"

    def row[T: msgspec.Struct](
        self, stem: Key, form: type[T], run: Callable[[operators.Call[T]], BlenderResult[operators.Outcome]], invoke: Option[operators.Invoke[T, None]] = Nothing
    ) -> operators.Operator[T]:
        """Builds internal verb operator over stem."""
        return operators.Operator(key=self.key(stem), title=self.title, description=self.description, icon=Some(self.icon), internal=True, form=form, run=run, undo=self.undo, invoke=invoke)


@runtime_checkable
class Section(Row, Registrable, Protocol):
    """Section a panel, popover, or append places."""

    @property
    def key(self) -> Key:
        """Key every section idname derives from."""

    @property
    def title(self) -> str:
        """Title section header draws."""

    @property
    def description(self) -> str:
        """Hover text of a popover button opening the section."""

    @property
    def popovers(self) -> tuple["Single[operators.Empty]", ...]:
        """Values popovers of section store."""

    def realized(self, identity: Identity) -> BlenderResult["Drawn"]:
        """Realizes section into panel and layout panel draws."""


# --- [MODELS] ---------------------------------------------------------------------------


class Picked(msgspec.Struct, frozen=True, kw_only=True):
    """History entry a restore or remove row names."""

    offset: Annotated[int, msgspec.Meta(title="Entry", description="Signed one-based history offset")]


class Locked(msgspec.Struct, frozen=True, kw_only=True):
    """Field a lock row toggles."""

    field: Annotated[str, msgspec.Meta(description="Field to lock or unlock")]


class Named(msgspec.Struct, frozen=True, kw_only=True):
    """Preset an apply, remove, or export row names."""

    stem: Annotated[str, msgspec.Meta(title="Preset", description="Preset file stem")]


class Renamed(Named, frozen=True, kw_only=True):
    """Saved preset and its new file name."""

    name: PresetName


class Saved(msgspec.Struct, frozen=True, kw_only=True):
    """Name entered in save-preset dialog."""

    name: PresetName


class Selected(msgspec.Struct, frozen=True, kw_only=True):
    """Selected presets in shown order."""

    items: Annotated[tuple[Named, ...], msgspec.Meta(min_length=1)]


class Actions(msgspec.Struct, frozen=True, kw_only=True):
    """Verb operator rows over one preset listing."""

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
    """Section draws for panel header, body, and layout panel, and whether a context holds its holder."""

    head: Option[SurfaceDraw]
    tail: Option[SurfaceDraw]
    body: SurfaceDraw
    inline: RowDraw
    present: Callable[[bpy.types.Context], bool] = lambda _context: True


class Listing[T: msgspec.Struct](Line, frozen=True, kw_only=True):
    """Preset catalog actions."""

    preset: Preset[T]
    current: Callable[[bpy.types.Context], BlenderResult[T]]
    matches: Callable[[bpy.types.Context, tuple[T, ...]], BlenderResult[tuple[bool, ...]]]
    applied: Callable[[bpy.types.Context, T], BlenderResult[operators.Outcome]]

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
        (renamed,) = frozenset(Renamed.__struct_fields__) - frozenset(Named.__struct_fields__)

        def user(stored: T | Path, key: str) -> BlenderResult[Path]:
            return Ok(stored) if isinstance(stored, Path) else Error((BlenderAbsentFault(owner=str(self.preset.folder(identity)), key=key),))

        def available(name: PresetName) -> BlenderResult[PresetName]:
            normalized = AddPresetBase.as_filename(name).casefold()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return Error((BlenderCollisionFault(request=name, error=self.preset.folder(identity)),)) if any(key.casefold() == normalized for key in self.preset.listed(identity)) else Ok(name)

        def neighbor(call: operators.Call[operators.Empty], step: int) -> BlenderResult[operators.Outcome]:
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

        replace = Verb.REPLACE.row(
            stem,
            Named,
            lambda call: (
                paired(self.preset.found(identity, call.record.stem).bind(lambda stored: user(stored, call.record.stem)), self.current(call.context))
                .bind(lambda pair: self.preset.written(*pair))
                .map(lambda _: operators.Done())
            ),
            invoke=Some(operators.Confirm[Named](message=Some(lambda _context, record: Ok(f"Replace preset {shown(record.stem)} with section values?")), confirm_text=Some("Replace"))),
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
                    return Error((BlenderCollisionFault(request=call.record.name, error=self.preset.folder(identity)),))
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
            apply=Verb.APPLY.row(stem, Named, lambda call: self.preset.found(identity, call.record.stem).bind(self.preset.read).bind(lambda value: self.applied(call.context, value))),
            save=Verb.SAVE.row(stem, Saved, saved, invoke=Some(operators.Properties())),
            delete=Verb.DELETE.row(
                stem,
                Selected,
                lambda call: accumulated(self.preset.found(identity, item.stem).bind(lambda stored: user(stored, item.stem)).bind(removed) for item in call.record.items).map(
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
                ).map(operators.Done.wrote),
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
                draw=Some(lambda layout, _context, properties, _identity: layout.prop(properties, renamed)),
            ),
            previous=Verb.PREVIOUS.row(stem, operators.Empty, lambda call: neighbor(call, -1)),
            following=Verb.NEXT.row(stem, operators.Empty, lambda call: neighbor(call, 1)),
        )

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Realizes apply entries with file actions and add and import buttons."""
        commands = self.commands(identity)
        apply, delete, export = commands.apply, commands.delete, commands.export
        footer = (OperatorButton(target=commands.save, text=Some("")), OperatorButton(target=commands.load, text=Some("")))

        def draw(buttons: tuple[RowDraw, ...]) -> RowDraw:
            def entry(layout: bpy.types.UILayout, context: bpy.types.Context, stem: str, kept: T | Path, value: BlenderResult[T], matched: Option[bool]) -> None:
                line, chosen = layout.row(align=True), Named(stem=stem)
                alerted(line, value.bind(lambda _: pressed(line, identity, apply, chosen, text=bpy.path.display_name(stem), icon="CHECKMARK" if matched == Some(True) else "BLANK1")))

                def actions(row: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
                    alerted(row, pressed(row, identity, export, Selected(items=(chosen,)), text="", icon="EXPORT"))
                    if isinstance(kept, Path):
                        alerted(row, pressed(row, identity, delete, Selected(items=(chosen,)), text="", icon="X"))

                invoking(actions)(line, context, context)

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
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

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires listing rows."""
        return Ok(tuple(self.commands(identity).verbs.values()))


class Slots[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Store bound to an identity."""

    attached: Attached[T]
    history: ValueHistory[T, Host, None]
    plain: dict[Verb, operators.Operator[operators.Empty]]
    picked: dict[Verb, operators.Operator[Picked]]
    lock: operators.Operator[Locked]
    listing: Option[Listing[T]]
    tail: tuple[RowDraw, ...]
    fields: Mapping[MemberDescriptorType, RowContent]


class Store[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Value sets of an attached record."""

    attachment: Attachment[T]
    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing
    presets: Option[Preset[T]] = Nothing
    scope: Option[Scoped[Host]] = Nothing
    fields: Mapping[MemberDescriptorType, RowContent] = msgspec.field(default_factory=dict)

    @property
    def panel(self) -> Key:
        """Key of values popover."""
        return f"{self.attachment.key}_values"

    @property
    def popover(self) -> "Single[operators.Empty]":
        """Values popover section that `surfaces.panels` registers."""
        return Single(
            key=self.panel,
            title="Values",
            description="Apply presets, lock fields, copy values, and restore earlier values" if self.presets.is_some() else "Copy values and restore earlier values",
            rows=(Values(store=self),),
        )

    def bound(self, identity: Identity) -> BlenderResult[Slots[T]]:
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

    def slotted(self, identity: Identity, attached: Attached[T]) -> BlenderResult[Slots[T]]:
        """Builds store rows over bound attachment."""
        history = ValueHistory(form=attached.group.record, read=attached.read, write=attached.write, storage=attached.struct, key=Slot.HISTORY)
        stem = self.attachment.key
        vacant = operators.Unchanged(reason="No history entry at offset")

        def concluded[V](change: Option[V]) -> operators.Outcome:
            return change.map(lambda _: operators.Done()).default_value(vacant)

        def now() -> datetime.datetime:
            return datetime.datetime.now(datetime.UTC)

        def retained() -> BlenderResult[Retention]:
            return self.retention.map(lambda read: read(identity)).default_with(lambda: Ok(Retention()))

        def recorded(context: bpy.types.Context, write: Callable[[Host], BlenderResult[Option[operators.Done]]]) -> BlenderResult[operators.Outcome]:
            return (
                paired(hosted(attached.name, context), retained())
                .bind(lambda pair: history.replaced(pair[0], lambda: write(pair[0]), now(), pair[1]))
                .map(lambda change: change.default_value(operators.Unchanged(reason="No value changed")))
            )

        def stepped(context: bpy.types.Context, offset: int) -> BlenderResult[operators.Outcome]:
            return paired(hosted(attached.name, context), retained()).bind(lambda pair: history.stepped(pair[0], offset, now(), pair[1])).map(concluded)

        def scoped(call: operators.Call[operators.Empty], host: Host) -> BlenderResult[Option[operators.Done]]:
            return (
                paired(self.scope.map(lambda scope: scope.owners(call.context)).default_value(Ok((host,))), call.root(bpy.types.WindowManager))
                .bind(lambda pair: pasted(pair[1], tuple(map(attached.struct, pair[0]))))
                .map(lambda outcomes: Some(operators.Done()) if any(outcome.written for outcome in outcomes) else Nothing)
            )

        plain = {
            Verb.RESET: Verb.RESET.row(stem, operators.Empty, lambda call: recorded(call.context, lambda host: attached.group.reset(attached.struct(host)).map(lambda _: Some(operators.Done())))),
            Verb.COPY: Verb.COPY.row(
                stem,
                operators.Empty,
                lambda call: (
                    paired(hosted(attached.name, call.context).bind(lambda host: Snapshot.capture(attached.struct(host))), call.root(bpy.types.WindowManager))
                    .map(lambda pair: copied(pair[1], pair[0]))
                    .map(lambda _: operators.Done())
                ),
            ),
            Verb.PASTE: Verb.PASTE.row(stem, operators.Empty, lambda call: recorded(call.context, lambda host: scoped(call, host))),
            Verb.OLDER: Verb.OLDER.row(stem, operators.Empty, lambda call: stepped(call.context, 1)),
            Verb.NEWER: Verb.NEWER.row(stem, operators.Empty, lambda call: stepped(call.context, -1)),
        }
        picked = {
            Verb.RESTORE: Verb.RESTORE.row(stem, Picked, lambda call: stepped(call.context, call.record.offset)),
            Verb.FORGET: Verb.FORGET.row(stem, Picked, lambda call: hosted(attached.name, call.context).bind(lambda host: history.removed(host, call.record.offset)).map(concluded)),
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
                applied=lambda context, value: recorded(context, lambda host: attached.applied(host, value).map(lambda wrote: Some(operators.Done()) if wrote else Nothing)),
            )
        )
        slots = (Popover(panel=self.panel, text=Some(""), icon=Some("PRESET")), OperatorButton(target=plain[Verb.RESET], text=Some("")))
        return accumulated(realized(row, identity) for row in slots).map(
            lambda tail: Slots(attached=attached, history=history, plain=plain, picked=picked, lock=lock, listing=listing, tail=tail, fields=self.fields)
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires store verb rows, listing rows, and field rows."""
        return self.bound(identity).map(lambda slots: (*slots.plain.values(), *slots.picked.values(), slots.lock, *slots.listing.to_list(), *acquired(tuple(self.fields.values()))))


class Hosted[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Store beside member naming its host."""

    host: Member
    store: Store[T]


class Values[T: msgspec.Struct](Line, frozen=True, kw_only=True):
    """Store value sets in its popover."""

    store: Store[T]

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires attachment and store operators."""
        return Ok((self.store.attachment, self.store))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Realizes listing, locks, clipboard, and history rows."""

        def built(slots: Slots[T]) -> BlenderResult[RowDraw]:
            attached, history = slots.attached, slots.history
            fields = slots.listing.map(lambda _: tuple(row for row in msgspec.structs.fields(attached.group.record) if row.name in attached.group.struct.bl_rna.properties)).default_value(())
            clipboard, steps = (tuple(OperatorButton(target=slots.plain[verb]) for verb in verbs) for verbs in ((Verb.COPY, Verb.PASTE), (Verb.OLDER, Verb.NEWER)))

            def draw(parts: tuple[Option[RowDraw], tuple[tuple[RowDraw, ...], tuple[RowDraw, ...]]]) -> RowDraw:
                listing, (copying, stepping) = parts

                def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                    found = hosted(attached.name, context)
                    for each in listing.to_list():
                        each(layout, holder, context)
                    for host in found.to_option().to_list():
                        locks, group = attached.locks(host), attached.struct(host)
                        for row in fields:
                            icon: IconItems = "LOCKED" if row.name in locks else "UNLOCKED"
                            alerted(layout, pressed(layout, identity, slots.lock, Locked(field=row.name), text=group.bl_rna.properties[row.name].name, icon=icon))
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
                                pressed(line, identity, slots.picked[Verb.RESTORE], chosen, text=f"{moment.astimezone():%X}", icon="BACK" if offset > 0 else "FORWARD"),
                                pressed(line, identity, slots.picked[Verb.FORGET], chosen, text="", icon="X"),
                            ),
                        )
                    alerted(layout, held)

                return shown

            return paired(
                slots.listing.map(lambda listing: realized(listing, identity).map(Some)).default_value(Ok(Nothing)),
                paired(accumulated(realized(row, identity) for row in clipboard), accumulated(realized(row, identity) for row in steps)),
            ).map(draw)

        return self.store.bound(identity).bind(built)


class Assembly[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Realized section parts."""

    name: str
    gate: Option[tuple[ResolvedMember, RowDraw]]
    header: tuple[RowDraw, ...]
    rows: tuple[RowDraw, ...]
    fields: tuple[RowDraw, ...]
    nest: Option[Nest]
    slots: Option[Slots[T]]

    @classmethod
    def of(cls, identity: Identity, key: Key, rows: tuple[RowContent, ...], header: tuple[RowContent, ...], enable: Option[Member], slots: Option[Slots[T]]) -> BlenderResult[Self]:
        """Realizes section parts at acquisition, rows and store scalars on one grid."""
        walk = slots.map(lambda held: fields(held.attached.group, identity, held.fields).map(Some)).default_value(Ok(Nothing))
        gate = enable.map(
            lambda member: member.resolved(bpy.types.BoolProperty).bind(lambda held: realized(Property(target=held, text=Some("")), identity).map(lambda draw: Some((held, draw))))
        ).default_value(Ok(Nothing))

        def built(parts: tuple[tuple[str, Option[tuple[ResolvedMember, RowDraw]]], tuple[tuple[RowDraw, ...], Option[tuple[tuple[RowContent, ...], Nest]]]]) -> BlenderResult[Self]:
            (name, gated), (heads, walked) = parts
            scalars = walked.map(lambda nested: nested[0]).default_value(())
            count = len(rows)
            return placed((*rows, *scalars), identity).map(
                lambda draws: cls(name=name, gate=gated, header=heads, rows=draws[:count], fields=draws[count:], nest=walked.map(lambda nested: nested[1]), slots=slots)
            )

        return paired(paired(identity.name(Name.ATTACHMENT, key), gate), paired(accumulated(realized(row, identity) for row in header), walk)).bind(built)

    def head(self, layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
        """Draws textless header enable."""
        for _, draw in self.gate.to_list():
            draw(layout, holder, context)

    def tail(self, layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", host: Option[Host], context: bpy.types.Context) -> None:
        """Draws header rows, then flat store slots with host as context pointer."""
        line = layout.row(align=True)
        for draw in self.header:
            draw(line, holder, context)
        for slots, found in product(self.slots.to_list(), host.to_list()):
            line.emboss = "NONE"
            line.context_pointer_set(slots.attached.name, found)
            for draw in slots.tail:
                draw(line, holder, context)

    def body(self, layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", host: Option[Host], context: bpy.types.Context, prefix: str) -> None:
        """Draws rows and store fields, inactive while enable is off."""
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
        opened: Option[tuple[Native, str]],
        *,
        active: bool = True,
    ) -> Option[bpy.types.UILayout]:
        """Draws layout panel header, returning body while open."""
        header, body = opened.starmap(layout.panel_prop).default_with(lambda: layout.panel(idname, default_closed=True))
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
    """Rows under header over one holder, the drawing holder while `holder` is unset."""

    key: Key
    title: str
    description: str = ""
    rows: tuple[RowContent, ...] = ()
    header: tuple[RowContent, ...] = ()
    enable: Option[Member] = Nothing
    store: Option[Hosted[T]] = Nothing
    condition: Option[Condition] = Nothing
    holder: Option[Member] = Nothing

    @property
    def popovers(self) -> tuple["Single[operators.Empty]", ...]:
        """Values popover of store."""
        return tuple(hosted.store.popover for hosted in self.store.to_list())

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires registrable rows and store rows."""
        return Ok((*acquired((*self.rows, *self.header)), *(row for hosted in self.store.to_list() for row in (hosted.store.attachment, hosted.store))))

    def realized(self, identity: Identity) -> BlenderResult[Drawn]:
        """Realizes section into panel and layout panel draws."""
        hosting = self.store.map(lambda hosted: paired(hosted.host.resolved().bind(lambda bound: fits(bound, (hosted.store.attachment.owner,))), hosted.store.bound(identity)).map(Some)).default_value(
            Ok(Nothing)
        )

        holding = self.holder.map(lambda member: member.resolved().map(Some)).default_value(Ok(Nothing))

        def drawn(parts: tuple[tuple[Assembly[T], Option[Resolved]], Option[Resolved]]) -> Drawn:
            (frame, host), reach = parts

            def held(holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> "Option[bpy.types.bpy_struct[object]]":
                return reach.map(lambda bound: located(bound, holder, context).bind(lambda pair: Option.of_optional(getattr(*pair)))).default_value(Some(holder))

            def found(holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> Option[Host]:
                return host.bind(lambda bound: located(bound, holder, context)).bind(lambda pair: Option.of_optional(getattr(*pair)))

            def head(layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
                for holder in held(context, context).to_list():
                    frame.head(layout, holder, context)

            def tail(layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
                for holder in held(context, context).to_list():
                    frame.tail(layout, holder, found(holder, context), context)

            def body(layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
                for holder in held(context, context).to_list():
                    layout.active = layout.active and all(holds(required, context, Some(holder)) for required in self.condition.to_list())
                    frame.body(layout, holder, found(holder, context), context, frame.name)

            def inline(layout: bpy.types.UILayout, outer: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for holder in held(outer, context).to_list():
                    active = all(holds(required, context, Some(holder)) for required in self.condition.to_list())
                    host = found(holder, context)
                    for shown in frame.framed(layout, frame.name, self.title, holder, host, context, Nothing, active=active).to_list():
                        frame.body(shown, holder, host, context, frame.name)

            return Drawn(
                head=frame.gate.map(lambda _: head),
                tail=Some(tail) if self.header or self.store.is_some() else Nothing,
                body=body,
                inline=inline,
                present=lambda context: held(context, context).is_some(),
            )

        return paired(
            hosting.bind(
                lambda held: Assembly.of(identity, self.key, self.rows, self.header, self.enable, held.map(lambda hosted: hosted[1])).map(lambda frame: (frame, held.map(lambda hosted: hosted[0])))
            ),
            holding,
        ).map(drawn)

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns section as layout panel row."""
        return self.realized(identity).map(lambda drawn: drawn.inline)


class Each[T: msgspec.Struct = operators.Empty](msgspec.Struct, frozen=True, kw_only=True):
    """Section per collection item, item as holder and store host."""

    key: Key
    title: str
    collection: Member
    description: str = ""
    rows: tuple[RowContent, ...] = ()
    header: tuple[RowContent, ...] = ()
    enable: Option[Member] = Nothing
    store: Option[Store[T]] = Nothing
    open: Option[str] = Nothing
    empty: tuple[RowContent, ...] = ()
    condition: Option[Condition] = Nothing

    @property
    def popovers(self) -> tuple[Single[operators.Empty], ...]:
        """Values popover of store."""
        return tuple(store.popover for store in self.store.to_list())

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires registrable rows and store rows."""
        return Ok((*acquired((*self.rows, *self.header, *self.empty)), *(row for store in self.store.to_list() for row in (store.attachment, store))))

    def realized(self, identity: Identity) -> BlenderResult[Drawn]:
        """Realizes layout panel per item, store bound to item type."""

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
        """Returns item sections as one row."""
        return self.realized(identity).map(lambda drawn: drawn.inline)


# --- [OPERATIONS] -----------------------------------------------------------------------


def hosted(name: str, context: bpy.types.Context) -> BlenderResult[Host]:
    """Returns host from header context pointer, absent fault when unset."""
    return catch(lambda: getattr(context, name), exception=AttributeError)().map_error(lambda _: (BlenderAbsentFault(owner="bpy.context", key=name),))


def fields[T: msgspec.Struct, S: "bpy.types.bpy_struct[object]"](
    group: RecordGroup[T, S], identity: Identity, replacements: Mapping[MemberDescriptorType, RowContent] = MappingProxyType({})
) -> BlenderResult[tuple[tuple[RowContent, ...], Nest]]:
    """Realizes fields as scalar rows and nested record, union, or collection panels."""
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

    def split(row: msgspec.structs.FieldInfo, shape: Shape, struct: "type[bpy.types.bpy_struct[object]]", *, panel: bool = True) -> BlenderResult[Property | Nest]:
        key = row.name

        def nested(record: type[msgspec.Struct], native: type[bpy.types.PropertyGroup]) -> BlenderResult[Nest]:
            return fields(RecordGroup(record=record, struct=native), identity, replacements).bind(inside)

        def paneled(nest: Nest, *, gated: bool) -> Nest:
            def draw(layout: bpy.types.UILayout, group: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
                held, idname = getattr(group, row.name), f"{prefix}_{row.name}"
                header, body = layout.panel(idname, default_closed=True)
                header.use_property_split = False
                if gated:
                    header.prop(held, ENABLED, text="")
                header.label(text=group.bl_rna.properties[row.name].name)
                for shown in Option.of_optional(body).to_list():
                    shown.active = bool(getattr(held, ENABLED)) if gated else True
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

        if not isinstance(shape, Gate | Nested | Entries | Tagged | Records):
            return Ok(Property(target=row))
        native = struct.__annotations__[key].keywords["type"]
        match shape:
            case Gate(value=Option(tag="some", some=(field, inner))):
                return split(field, inner.shape, native, panel=False).bind(
                    lambda child: (
                        Ok(Property(target=row))
                        if isinstance(child, Property) and not isinstance(inner.shape, Gate)
                        else realized(child, identity).map(lambda draw: paneled(lambda layout, holder, context, _prefix: draw(layout, holder, context), gated=True))
                        if isinstance(child, Property)
                        else Ok(paneled(child, gated=True))
                    )
                )
            case Gate():
                return Ok(Property(target=row))
            case Nested(record=record):
                found = nested(record, native).map(reached)
            case Entries(gate=gate):
                found = (
                    accumulated(split(entry, gate, native).map(lambda part: (entry, part)) for entry in RecordGroup.entries(row, shape)).map(lambda parts: joined(parts, {})).bind(inside).map(reached)
                )
            case Tagged(cases=cases):
                found = accumulated(nested(case, native.__annotations__[case.__name__].keywords["type"]).map(lambda nest: (case.__name__, nest)) for case, _ in cases).map(
                    lambda parts: chosen(parts, native)
                )
            case Records(record=record):
                found = nested(record, native).map(each)
        return found.map(lambda nest: paneled(nest, gated=False) if panel else nest)

    def joined(parts: tuple[tuple[msgspec.structs.FieldInfo, Property | Nest], ...], supplied: Mapping[str, RowContent]) -> tuple[tuple[RowContent, ...], Nest]:
        nests = tuple(part for _, part in parts if not isinstance(part, Property))

        def draw(layout: bpy.types.UILayout, group: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
            for nest in nests:
                nest(layout, group, context, prefix)

        return tuple(supplied.get(row.name, part) for row, part in parts if isinstance(part, Property)), draw

    return field_shapes(group.record).bind(
        lambda rows: accumulated(
            split(row, shape.shape, group.struct).map(lambda part: (row, part)) for row, shape in rows if row.name in group.struct.__annotations__ or row.name in group.struct.bl_rna.properties
        ).map(lambda parts: joined(parts, supplied))
    )


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
