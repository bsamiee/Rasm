"""Add-on preferences page and record reads and writes."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager
from functools import partial
from types import MemberDescriptorType
from typing import Annotated, Protocol, runtime_checkable

import bpy
from expression import Error, Nothing, Option, Some
import msgspec

from blender.bpy.commands import keymaps, operators
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions, Band, Gathering, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.settings.attachments import Attached, Attachment, preferred
from blender.bpy.settings.properties import Choices, RecordGroup
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.layout.rows import Member, OperatorButton, placed, Property, RowDraw
from blender.bpy.ui.layout.sections import fields, Nest, Section, Single
from blender.bpy.ui.layout.values import Verb
from blender.bpy.ui.screens.areas import Located
from blender.bpy.ui.surfaces.panels import Panel, PopoverPanel, reached
from blender.records.faults import accumulated, paired

# --- [TYPES] ----------------------------------------------------------------------------


@runtime_checkable
class Preferring(Protocol):
    """Registered row adding sections and preference field choices to its extension's preferences page."""

    @property
    def preferences(self) -> "PageRows":
        """Sections and field choices the row adds."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class PageRows(msgspec.Struct, frozen=True, kw_only=True):
    """Sections and preference field choices one registered row adds to its extension's preferences page."""

    sections: tuple[Section, ...] = ()
    bindings: Mapping[MemberDescriptorType, Choices] = msgspec.field(default_factory=dict)


class PreferencesPage[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Preference record with sections, gathering the sections, field choices, confirmations, shortcuts, and stores every other row declares, firing `changed` on each edit."""

    record: type[T]
    sections: tuple[Section, ...] = ()
    bindings: Option[Mapping[MemberDescriptorType, Choices]] = Nothing
    changed: Signal = msgspec.field(default_factory=Signal)

    # --- [ACCESS]

    def value[F](self, member: Callable[[T], F]) -> Callable[[Identity], BlenderResult[F]]:
        """Returns reader of one record field per identity."""
        return lambda identity: read(identity, self.record).map(member)

    def stored[F](self, replace: Callable[[T, F], T]) -> Callable[[Identity, F], BlenderResult[None]]:
        """Returns writer replacing one record field per identity."""
        return lambda identity, field: read(identity, self.record).bind(lambda held: write(identity, replace(held, field)))

    # --- [GATHERING]

    def gathered(self, identity: Identity, declared: tuple[Registrable | Gathering, ...], /) -> BlenderResult[Acquisitions]:
        """Declares page class, commands, and value popovers, building draw after operators register."""
        asks = tuple(row for row in declared if isinstance(row, operators.Operator) and any(isinstance(form, operators.Suppressible) for form in row.invoke.to_list()))
        bindings = tuple(row for row in declared if isinstance(row, keymaps.Keymap))
        stores = tuple(row for row in declared if isinstance(row, Attachment))
        added = tuple(row.preferences for row in declared if isinstance(row, Preferring))
        choices = dict(item for mapping in (*self.bindings.to_list(), *(part.bindings for part in added)) for item in mapping.items())
        asked = msgspec.defstruct("Asks", [(row.ask, Annotated[bool, msgspec.Meta(title=row.title, description=row.description)], True) for row in asks], frozen=True, kw_only=True)

        def attached() -> BlenderResult[tuple[Attached[msgspec.Struct], ...]]:
            return accumulated(row.bound(identity) for row in stores)

        reset = msgspec.structs.replace(Verb.RESET.row("preferences_asks", operators.Empty, lambda call: write(call.identity, asked()).map(lambda _: operators.Done())), undo=False)
        purge = operators.Operator[operators.Empty](
            key="purge_preferences",
            title="Clear Stored Settings",
            description="Reset extension settings stored on data-blocks to defaults",
            icon=Some("TRASH"),
            internal=True,
            form=operators.Empty,
            run=lambda _: (
                attached()
                .map(lambda held: sum(each.purged() for each in held))
                .map(
                    lambda count: (
                        operators.Done(notes=(("INFO", f"Cleared settings stored on {count} data-blocks"),)) if count else operators.Unchanged(reason="No data-block stores extension settings")
                    )
                )
            ),
            invoke=Some(
                operators.Confirm[operators.Empty](
                    message=Some(lambda *_: attached().map(lambda held: f"Clear settings stored on {sum(each.stored(host) for each in held for host in each.hosts())} data-blocks")),
                    confirm_text=Some("Clear"),
                    icon="WARNING",
                )
            ),
        )

        def extra(kind: type[bpy.types.AddonPreferences]) -> tuple[Single, ...]:
            return (
                *(
                    (Single(key="preferences_shortcuts", title="Shortcuts", rows=tuple(partial(lambda row, layout, _holder, context: row.drawn(layout, context), row) for row in bindings)),)
                    if bindings
                    else ()
                ),
                *(
                    (
                        Single(
                            key="preferences_asks",
                            title="Confirmations",
                            rows=(*(Property(target=Member(root=kind, path=row.name)) for row in msgspec.structs.fields(asked)), OperatorButton(target=reset)),
                        ),
                    )
                    if asks
                    else ()
                ),
                *((Single(key="preferences_stored", title="Stored Settings", rows=(OperatorButton(target=purge),)),) if stores else ()),
            )

        commands = (show, *((reset,) if asks else ()), *((purge,) if stores else ()))

        def built(group: RecordGroup[T], flags: RecordGroup[msgspec.Struct], prefix: str) -> Acquisitions:
            paint: RowDraw

            def draw(holder: bpy.types.AddonPreferences, context: bpy.types.Context) -> None:
                layout = holder.layout
                layout.use_property_split, layout.use_property_decorate = True, False
                paint(layout, holder, context)

            kind = type(identity.package, (bpy.types.AddonPreferences,), {"bl_idname": identity.package, **RecordGroup.body(group.struct, flags.struct), "draw": draw})
            sections = (*self.sections, *(section for part in added for section in part.sections), *extra(kind))

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
                """Writes record back, storing collection defaults RNA cannot hold as items."""
                return read(identity, self.record).bind(lambda settings: write(identity, settings)).map(lambda _: Nothing)

            return (
                *((Band.GROUPS, nested) for built in (group, flags) for nested in built.nested),
                *commands,
                *sections,
                *(Panel(section=popover, at=PopoverPanel()) for popover in reached(sections)),
                (Band.PREFERENCES, kind),
                (Band.INTERFACE, prepared()),
                Timer(body=seeded),
            )

        return paired(
            paired(
                RecordGroup.bound(identity, "preferences", self.record, Some(lambda *_: identity.reports.reported(self.changed.fired)().to_optional()), bindings=Some(choices)),
                RecordGroup.bound(identity, "preferences_asks", asked),
            ),
            identity.name(Name.ATTACHMENT, "preferences"),
        ).map(lambda parts: built(*parts[0], parts[1]))


# --- [OPERATIONS] -----------------------------------------------------------------------


def read[T: msgspec.Struct](identity: Identity, record: type[T]) -> BlenderResult[T]:
    """Returns stored preference record."""
    return (
        Option
        .of_optional(bpy.context.preferences.addons.get(identity.package))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        .bind(lambda addon: Option.of_optional(addon.preferences))
        .map(lambda holder: RecordGroup(record=record, struct=type(holder)).read(holder))
        .default_with(lambda: Error((BlenderAbsentFault(owner=identity.package, key="preferences"),)))
    )


def write(identity: Identity, settings: msgspec.Struct) -> BlenderResult[None]:
    """Writes record, marking preferences dirty."""
    return preferred(identity, bpy.context.preferences, lambda holder: RecordGroup(record=type(settings), struct=type(holder)).write(holder, settings))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


# --- [COMPOSITION] ----------------------------------------------------------------------


def opened(call: operators.Call[operators.Empty]) -> BlenderResult[operators.Done]:
    """Opens extension preferences from context window and area."""
    return Located.current(call.context).bind(lambda located: located.run(bpy.ops.preferences.addon_show, {"module": call.identity.package})).map(lambda _: operators.Done())


show = operators.Operator[operators.Empty](
    key="show_preferences", title="Preferences", description="Open extension preferences", icon=Some("PREFERENCES"), undo=False, form=operators.Empty, run=opened
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PageRows", "PreferencesPage", "Preferring", "read", "show", "write"]
