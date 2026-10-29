"""Value stores over attached form groups: value sets held as history, clipboard copies, member locks, and the preset catalog their values popover draws."""

from collections.abc import Callable, Mapping
import datetime
from functools import cached_property, partial
from types import MappingProxyType

import bpy
from expression import Error, Nothing, Ok, Option, Some

from blender.bpy.commands import operators
from blender.bpy.commands.calls import Command, Placement
from blender.bpy.data.rna.mirrors import annotated
from blender.bpy.data.rna.paths import definition, rooted, Struct
from blender.bpy.data.rna.scopes import Scoped
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import Attached, Attachment, Host
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.presets import Listing, Offset, Verb
from blender.bpy.ui.layout.rows import alerted, Keywords, Line, OperatorButton, pressed, realized, RowContent, RowDraw
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason
from blender.records.fields import Retention

# --- [TYPES] ----------------------------------------------------------------------------

type Replacement = Callable[[Member], RowContent]

# --- [MODELS] ---------------------------------------------------------------------------


class Store[T: bpy.types.PropertyGroup](Line, frozen=True, kw_only=True, dict=True):
    """Value sets of an attached form group held as snapshots, its host read through `host` at the section holder, drawn as the values popover.

    `fields` replace the property rows a section draws for the named form members.
    """

    attachment: Attachment[T]
    host: Option[Member] = Nothing
    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing
    presets: Option[Preset[Snapshot]] = Nothing
    scope: Option[Scoped[Host]] = Nothing
    fields: Mapping[str, Replacement] = MappingProxyType({})

    # --- [NAMING]

    @property
    def panel(self) -> Key:
        """Key of the values popover."""
        return f"{self.attachment.key}_values"

    def attached(self, identity: Identity) -> Outcome[Attached[T]]:
        """Binds the attachment, refusing a scope whose owners never hold it."""
        return self.attachment.bound(identity).bind(
            lambda attached: (
                self.scope
                .filter(lambda scope: not issubclass(scope.owner_type, attached.owner))
                .map(lambda scope: Error(Fault.of(RecordReason.OUTSIDE, attached.name, scope.owner_type.__name__, member=attached.owner.__name__)))
                .default_value(Ok(attached))
            )
        )

    def listing(self, identity: Identity) -> Outcome[Option[Listing[Snapshot]]]:
        """Preset catalog over the host the context pointer names, locked members kept, a preset's unlocked members applied as one history step."""

        def applied(context: bpy.types.Context, value: Snapshot) -> Outcome[operators.Answer]:
            return self._recorded(identity, context, lambda held, host: held.applied(host, value).map(lambda wrote: Some(operators.Done()).filter(lambda _: wrote)))

        return self.attached(identity).map(
            lambda attached: self.presets.map(lambda preset: Listing(preset=preset, current=partial(_current, attached), matches=partial(_matching, attached), applied=applied))
        )

    # --- [COMMANDS]

    @cached_property
    def commands(self) -> Mapping[Verb, Command]:
        """Verb operators over the attachment key, the lock verb present while presets apply over a hidden form naming one declared member."""
        stem, form = self.attachment.key, self.attachment.form

        def locked() -> type[bpy.types.PropertyGroup]:
            field = bpy.props.EnumProperty(name="Member", description="Member to lock or unlock", items=tuple((name, name, "") for name in annotated(form)), options={"HIDDEN"})
            return type(f"{form.__name__}Lock", (bpy.types.PropertyGroup,), {"__annotations__": {"field": field}})

        locking = self.presets.map(lambda _: {Verb.LOCK: Verb.LOCK.command(stem, locked(), self.lock_value)}).default_value({})
        return MappingProxyType({
            Verb.RESET: Verb.RESET.command(stem, operators.Empty, self.reset_values),
            Verb.COPY: Verb.COPY.command(stem, operators.Empty, self.copy_values),
            Verb.PASTE: Verb.PASTE.command(stem, operators.Empty, self.paste_values),
            Verb.RESTORE: Verb.RESTORE.command(stem, Offset, self.restore_values),
            Verb.FORGET: Verb.FORGET.command(stem, Offset, self.forget_values),
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

    def restore_values(self, call: operators.Call[Offset]) -> Outcome[operators.Answer]:
        """Restores the value set the offset names in history, older ones positive."""
        retention = self.retention.map(lambda read: read(call.identity)).default_with(lambda: Ok(Retention()))
        return (
            paired(self._held(call.identity, call.context), retention)
            .bind(lambda pair: pair[0][0].stepped(pair[0][1], call.properties.offset, datetime.datetime.now(datetime.UTC), pair[1]))
            .map(_concluded)
        )

    def forget_values(self, call: operators.Call[Offset]) -> Outcome[operators.Answer]:
        """Removes a value set from history."""
        return self._held(call.identity, call.context).bind(lambda held: held[0].forgotten(held[1], call.properties.offset)).map(_concluded)

    def lock_value(self, call: operators.Call[bpy.types.PropertyGroup]) -> Outcome[operators.Answer]:
        """Keeps the member's value when presets apply."""
        field: str = call.properties.field  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return self._held(call.identity, call.context).bind(lambda held: held[0].toggled(held[1], field)).map(lambda _: operators.Done())

    # --- [STEPS]

    def _held(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Attached[T], Host]]:
        """Binds the attachment and reads the host the context pointer names."""
        return self.attached(identity).bind(lambda attached: hosted(attached.name, context).map(lambda host: (attached, host)))

    def _recorded(self, identity: Identity, context: bpy.types.Context, write: Callable[[Attached[T], Host], Outcome[Option[operators.Done]]]) -> Outcome[operators.Answer]:
        """Runs an edit, adding the prior value set to history when it changes a value."""
        retention = self.retention.map(lambda read: read(identity)).default_with(lambda: Ok(Retention()))
        return (
            paired(self._held(identity, context), retention)
            .bind(lambda pair: pair[0][0].replaced(pair[0][1], lambda: write(*pair[0]), datetime.datetime.now(datetime.UTC), pair[1]))
            .map(lambda change: change.default_value(operators.Unchanged(reason="No value changed")))
        )

    # --- [ROW]

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the preset catalog, member locks, clipboard and history steps, and one restore and forget row per history entry."""
        commands = self.commands
        clipboard = tuple(OperatorButton(target=commands[verb]) for verb in (Verb.COPY, Verb.PASTE))
        steps = tuple(OperatorButton(target=Placement(commands[Verb.RESTORE], {"offset": offset}), icon=Some("BACK" if offset > 0 else "FORWARD")) for offset in (1, -1))
        lines = accumulated(realized(buttons, identity) for buttons in (clipboard, steps))
        listing = self.listing(identity).bind(lambda found: found.map(lambda held: realized((held,), identity).map(Some)).default_value(Ok(Nothing)))
        return paired(self.attached(identity), listing, lines).map(lambda parts: partial(_values, identity, commands, *parts))

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Attachment the store binds."""
        return (self.attachment,)

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the verb operators and the preset catalog over the bound attachment."""
        return self.listing(identity).map(lambda listing: (*self.commands.values(), *listing.to_list()))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [HOSTS]


def hosted(name: str, context: bpy.types.Context) -> Outcome[Host]:
    """Returns the host a layout's context pointer names, absent fault when unset."""
    return Option.of_optional(getattr(context, name, None)).to_result_with(partial(Fault.of, BlenderReason.ABSENT, "bpy.context", member=name))


def _current[T: bpy.types.PropertyGroup](attached: Attached[T], context: bpy.types.Context) -> Outcome[Snapshot]:
    """Captures the hosted group as a preset."""
    return hosted(attached.name, context).bind(attached.preset)


def _matching[T: bpy.types.PropertyGroup](attached: Attached[T], context: bpy.types.Context, values: tuple[Snapshot, ...]) -> Outcome[tuple[bool, ...]]:
    """Whether applying each preset would leave the hosted group unchanged."""
    return hosted(attached.name, context).bind(lambda host: accumulated(attached.projected(host, value).map(lambda ends: ends[2] == ends[0]) for value in values))


# --- [HISTORY]


def _concluded[V](change: Option[V]) -> operators.Answer:
    """Done for a history move, unchanged past either end."""
    return change.map(lambda _: operators.Done()).default_value(operators.Unchanged(reason="No history entry at offset"))


# --- [DRAWING]


def _values[T: bpy.types.PropertyGroup](
    identity: Identity,
    commands: Mapping[Verb, Command],
    attached: Attached[T],
    listing: Option[RowDraw],
    lines: tuple[RowDraw, ...],
    layout: bpy.types.UILayout,
    holder: Struct,
    context: bpy.types.Context,
) -> None:
    """Draws the catalog and member locks, the clipboard and history step lines, then one restore and forget line per history entry, times untranslated."""
    found = hosted(attached.name, context)
    for draw in listing.to_list():
        draw(layout, holder, context)
    for host, lock in ((host, lock) for host in found.to_option().to_list() for lock in Option.of_optional(commands.get(Verb.LOCK)).to_list()):
        kept, group, declared = attached.locks(host), attached.struct(host), annotated(attached.form)
        alerted(layout, kept)
        locks = kept.default_value(frozenset())
        for prop in (prop for prop in definition(group).properties if prop.identifier in declared):
            keywords: Keywords = {"text": prop.name, "text_ctxt": prop.translation_context, "icon": "LOCKED" if prop.identifier in locks else "UNLOCKED"}
            alerted(layout, pressed(layout, identity, Placement(lock, {"field": prop.identifier}), keywords))
    layout.separator()
    for draw in lines:
        draw(layout.row(align=True), holder, context)
    held = found.bind(attached.held)
    entries = held.map(
        lambda value: (*((index, entry.moment) for index, entry in enumerate(value.past, 1)), *((-index, entry.moment) for index, entry in enumerate(value.future, 1)))
    ).default_value(())
    for offset, moment in entries:
        line, chosen = layout.row(align=True), {"offset": offset}
        alerted(
            line,
            paired(
                pressed(line, identity, Placement(commands[Verb.RESTORE], chosen), {"text": f"{moment.astimezone():%X}", "translate": False, "icon": "BACK" if offset > 0 else "FORWARD"}),
                pressed(line, identity, Placement(commands[Verb.FORGET], chosen), {"text": "", "icon": "X"}),
            ),
        )
    alerted(layout, held)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Replacement", "Store", "hosted"]
