"""Store value sets with lock and history rows, and preset listing actions."""

from collections.abc import Callable, Mapping
import datetime
from enum import Enum
from pathlib import Path
from types import MemberDescriptorType
import typing
from typing import Annotated

from bl_operators.presets import AddPresetBase
import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands import operators
from blender.bpy.commands.calls import called, Writer
from blender.bpy.data.rna.snapshots import Scoped, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderResult
from blender.bpy.settings.attachments import Attached, Attachment, Host, Slot
from blender.bpy.settings.history import Retention, ValueHistory
from blender.bpy.settings.presets import copied, pasted, Preset, PresetName, removed
from blender.bpy.ui.layout.rows import acquired, alerted, invoking, Line, Member, OperatorButton, Popover, pressed, realized, refused, RowContent, RowDraw
from blender.imaging.faults import accumulated, paired

if typing.TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import IconItems

# --- [TYPES] ----------------------------------------------------------------------------

type Replacement = Callable[[Member], RowContent]


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

        def claimed(name: PresetName) -> Option[tuple[str, T | Path]]:
            normalized = AddPresetBase.as_filename(name).casefold()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return Option.of_optional(next(((key, value) for key, value in self.preset.listed(identity).items() if key.casefold() == normalized), None))

        def available(name: PresetName) -> BlenderResult[PresetName]:
            return claimed(name).map(lambda _: Error((BlenderCollisionFault(request=name, error=self.preset.folder(identity)),))).default_value(Ok(name))

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
            match claimed(call.record.name).to_optional():
                case (key, Path()):

                    def prompted(binding: tuple[str, Writer]) -> BlenderResult[operators.Done]:
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
        footer = (OperatorButton(target=commands.save, text=""), OperatorButton(target=commands.load, text=""))

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
        return Ok(msgspec.structs.astuple(self.commands(identity)))


class Slots[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Store bound to an identity."""

    attached: Attached[T]
    history: ValueHistory[T, Host, None]
    plain: dict[Verb, operators.Operator[operators.Empty]]
    picked: dict[Verb, operators.Operator[Picked]]
    lock: operators.Operator[Locked]
    listing: Option[Listing[T]]
    tail: tuple[RowDraw, ...]
    fields: Mapping[MemberDescriptorType, Replacement]


class Store[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Value sets of an attached record."""

    attachment: Attachment[T]
    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing
    presets: Option[Preset[T]] = Nothing
    scope: Option[Scoped[Host]] = Nothing
    fields: Mapping[MemberDescriptorType, Replacement] = msgspec.field(default_factory=dict)

    @property
    def panel(self) -> Key:
        """Key of values popover."""
        return f"{self.attachment.key}_values"

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
        slots = (Popover(panel=self.panel, text="", icon=Some("PRESET")), OperatorButton(target=plain[Verb.RESET], text=""))
        return accumulated(realized(row, identity) for row in slots).map(
            lambda tail: Slots(attached=attached, history=history, plain=plain, picked=picked, lock=lock, listing=listing, tail=tail, fields=self.fields)
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires store verb rows, listing rows, and field rows."""
        return self.bound(identity).map(
            lambda slots: (
                *slots.plain.values(),
                *slots.picked.values(),
                slots.lock,
                *slots.listing.to_list(),
                *acquired(tuple(build(Member(root=slots.attached.group.struct, path=slot.__name__)) for slot, build in self.fields.items())),
            )
        )


class Hosted[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Store beside member naming its host, the context root of the store's owner type while unset."""

    store: Store[T]
    host: Option[Member] = Nothing


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


# --- [OPERATIONS] -----------------------------------------------------------------------


def hosted(name: str, context: bpy.types.Context) -> BlenderResult[Host]:
    """Returns host from header context pointer, absent fault when unset."""
    return catch(lambda: getattr(context, name), exception=AttributeError)().map_error(lambda _: (BlenderAbsentFault(owner="bpy.context", key=name),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Actions", "Hosted", "Listing", "Locked", "Named", "Picked", "Renamed", "Replacement", "Saved", "Selected", "Slots", "Store", "Values", "Verb", "hosted"]
