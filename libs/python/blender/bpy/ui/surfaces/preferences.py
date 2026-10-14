"""Native add-on preferences, typed record access, change listeners, and page acquisition."""

from collections import defaultdict
from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager
from functools import partial
from types import MemberDescriptorType
from typing import Annotated

import bpy
from expression import Error, Nothing, Option, Result, Some
import msgspec

from blender.bpy.commands import keymaps, operators
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions, Band, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.settings.attachments import Attached, Attachment, preferred
from blender.bpy.settings.properties import Changed, Choices, RecordGroup
from blender.bpy.ui.layout.rows import OperatorButton, placed, Property, RowDraw
from blender.bpy.ui.layout.sections import fields, Nest, Section, Single, Verb
from blender.bpy.ui.screens.areas import Located, NewWindow, windowed
from blender.imaging.faults import accumulated, paired

# --- [MODELS] ---------------------------------------------------------------------------


class PreferencesPage(msgspec.Struct, frozen=True, kw_only=True):
    """Extension's preference record, custom sections, and owned registration rows."""

    record: type[msgspec.Struct]
    sections: tuple[Section, ...] = ()
    rows: tuple[Registrable, ...] = ()
    bindings: Option[Mapping[MemberDescriptorType, Choices]] = Nothing

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Declares native schemas and the page, binding its draws after operator acquisition."""
        asks = tuple(row for row in self.rows if isinstance(row, operators.Operator) and any(isinstance(form, operators.Suppressible) for form in row.invoke.to_list()))
        bindings = tuple(row for row in self.rows if isinstance(row, keymaps.Keymap))
        stores = tuple(row for row in self.rows if isinstance(row, Attachment))
        asked = msgspec.defstruct("Asks", [(row.ask, Annotated[bool, msgspec.Meta(title=row.title, description=row.description)], True) for row in asks], frozen=True, kw_only=True)

        def attached() -> BlenderResult[tuple[Attached[msgspec.Struct], ...]]:
            return accumulated(row.bound(identity) for row in stores)

        reset = msgspec.structs.replace(Verb.RESET.row("preferences_asks", operators.Empty, lambda call: write(call.identity, asked()).map(lambda _: operators.Done())), undo=False)
        purge = operators.Operator[operators.Empty](
            key="purge_preferences",
            title="Clear Stored Settings",
            description="Return every setting the extension stores on data-blocks to its default",
            icon=Some("TRASH"),
            internal=True,
            form=operators.Empty,
            run=lambda _: (
                attached()
                .map(lambda held: sum(each.purged() for each in held))
                .map(
                    lambda count: (
                        operators.Done(notes=(("INFO", f"Cleared the settings stored on {count} data-blocks"),)) if count else operators.Unchanged(reason="No data-block stores extension settings")
                    )
                )
            ),
            invoke=Some(
                operators.Confirm[operators.Empty](
                    message=Some(lambda *_: attached().map(lambda held: f"Clear the settings stored on {sum(each.stored(host) for each in held for host in each.hosts())} data-blocks")),
                    confirm_text=Some("Clear"),
                    icon="WARNING",
                )
            ),
        )
        extra = (
            *(
                (Single(key="preferences_shortcuts", title="Shortcuts", rows=tuple(partial(lambda row, layout, _holder, context: row.drawn(layout, context), row) for row in bindings)),)
                if bindings
                else ()
            ),
            *((Single(key="preferences_asks", title="Confirmations", rows=(*(Property(target=row) for row in msgspec.structs.fields(asked)), OperatorButton(target=reset))),) if asks else ()),
            *((Single(key="preferences_stored", title="Stored Settings", rows=(OperatorButton(target=purge),)),) if stores else ()),
        )
        commands = (show, *((reset,) if asks else ()), *((purge,) if stores else ()))

        def built(group: RecordGroup[msgspec.Struct], flags: RecordGroup[msgspec.Struct], prefix: str) -> Acquisitions:
            sections = (*self.sections, *extra)
            paint: RowDraw

            def draw(holder: bpy.types.AddonPreferences, context: bpy.types.Context) -> None:
                layout = holder.layout
                layout.use_property_split, layout.use_property_decorate = True, False
                paint(layout, holder, context)

            kind = type(
                identity.package,
                (bpy.types.AddonPreferences,),
                {
                    "bl_idname": identity.package,
                    "__annotations__": {**group.struct.__annotations__, **flags.struct.__annotations__},
                    "__record_defaults__": {**vars(group.struct)["__record_defaults__"], **vars(flags.struct)["__record_defaults__"]},
                    "draw": draw,
                },
            )

            def arranged(parts: tuple[tuple[RowDraw, ...], Nest, tuple[RowDraw, ...]]) -> None:
                nonlocal paint
                scalars, nest, draws = parts

                def body(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                    for each in scalars:
                        each(layout, holder, context)
                    nest(layout, holder, context, prefix)
                    for each in draws:
                        each(layout, holder, context)

                paint = body

            @contextmanager
            def prepared() -> Generator[BlenderResult[None]]:
                yield paired(
                    fields(group, identity).bind(lambda pair: placed(pair[0], identity).map(lambda scalars: (scalars, pair[1]))), accumulated(section.drawn(identity) for section in sections)
                ).map(lambda pair: arranged((*pair[0], pair[1])))

            def seeded() -> BlenderResult[Option[float]]:
                """Writes the record read, so record defaults of collections RNA cannot default become native items the page edits."""
                return read(identity, self.record).bind(lambda settings: write(identity, settings)).map(lambda _: Nothing)

            return (
                *self.rows,
                *((Band.GROUPS, nested) for built in (group, flags) for nested in built.nested),
                *commands,
                *sections,
                (Band.PREFERENCES, kind),
                (Band.INTERFACE, prepared()),
                Timer(body=seeded),
            )

        return paired(
            paired(RecordGroup.bound(identity, "preferences", self.record, Some(notified(identity)), bindings=self.bindings), RecordGroup.bound(identity, "preferences_asks", asked)),
            identity.name(Name.ATTACHMENT, "preferences"),
        ).map(lambda parts: built(*parts[0], parts[1]))


# --- [REGISTRIES] -----------------------------------------------------------------------

_listeners: defaultdict[Identity, list[Changed]] = defaultdict(list)


# --- [OPERATIONS] -----------------------------------------------------------------------


def notified(identity: Identity) -> Changed:
    """Returns a native update callback dispatching to active preference listeners."""

    def body(holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context, row: bpy.types.Property) -> None:
        for heard in tuple(_listeners.get(identity, ())):
            heard(holder, context, row)

    return body


@contextmanager
def listening(identity: Identity, heard: Changed) -> Generator[None]:
    """Owns one preference listener until the surrounding acquisition releases it."""
    listeners = _listeners[identity]
    listeners.append(heard)
    try:
        yield
    finally:
        listeners.remove(heard)
        if not listeners:
            del _listeners[identity]


def read[T: msgspec.Struct](identity: Identity, record: type[T]) -> BlenderResult[T]:
    """Returns the stored preference record or the native missing-page fault."""
    return (
        Option
        .of_optional(bpy.context.preferences.addons.get(identity.package))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        .bind(lambda addon: Option.of_optional(addon.preferences))
        .map(lambda holder: RecordGroup(record=record, struct=type(holder)).read(holder))
        .default_with(lambda: Error((BlenderAbsentFault(owner=identity.package, key="preferences"),)))
    )


def write(identity: Identity, settings: msgspec.Struct) -> BlenderResult[None]:
    """Marks attempted preference writes dirty and returns the native outcome."""
    return preferred(identity, bpy.context.preferences, lambda holder: RecordGroup(record=type(settings), struct=type(holder)).write(holder, settings))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


def value[T: msgspec.Struct, F](record: type[T], member: Callable[[T], F]) -> Callable[[Identity], BlenderResult[F]]:
    """Returns an identity reader through the caller's typed record projection."""
    return lambda identity: read(identity, record).map(member)


def stored[T: msgspec.Struct, F](record: type[T], replace: Callable[[T, F], T]) -> Callable[[Identity, F], BlenderResult[None]]:
    """Returns an identity writer through the caller's typed record update."""
    return lambda identity, field: read(identity, record).bind(lambda held: write(identity, replace(held, field)))


# --- [COMPOSITION] ----------------------------------------------------------------------


def opened(call: operators.Call[operators.Empty]) -> BlenderResult[operators.Done]:
    """Opens preferences from the native window and area, retaining both absence faults."""
    return Result[tuple[bpy.types.Window, bpy.types.Area], BlenderFaults].bind(
        paired(
            Option.of_optional(call.context.window).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="window"),)),
            Option.of_optional(call.context.area).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="area"),)),
        ),
        lambda held: windowed(NewWindow.PREFERENCES, Located(window=held[0], area=held[1]), call.identity).map(lambda _: operators.Done()),
    )


show = operators.Operator[operators.Empty](
    key="show_preferences", title="Preferences", description="Open the extension's preferences", icon=Some("PREFERENCES"), undo=False, form=operators.Empty, run=opened
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PreferencesPage", "listening", "notified", "read", "show", "stored", "value", "write"]
