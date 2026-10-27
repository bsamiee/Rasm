"""Add-on preferences page over a form group and its reads."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from functools import partial
from typing import Protocol, runtime_checkable

import bpy
from expression import Option, Some
import msgspec

from blender.bpy.commands import keymaps, operators
from blender.bpy.data.rna.descriptors import Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, Declaration, Gathering, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import annotated, Attached, Attachment, nested, updating
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.layout.rows import OperatorButton, placed, RowDraw
from blender.bpy.ui.layout.sections import fields, Nest, Section, Single
from blender.bpy.ui.screens.areas import Located
from blender.bpy.ui.surfaces.panels import Panel, PopoverPanel, reached
from blender.records.faults import accumulated, Fault, Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------


@runtime_checkable
class Preferring(Registrable, Protocol):
    """Registered row adding sections to its extension's preferences page."""

    @property
    def preferences(self) -> "PageRows":
        """Sections the row adds."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class PageRows(msgspec.Struct, frozen=True, kw_only=True):
    """Sections one registered row adds to its extension's preferences page."""

    sections: tuple[Section, ...] = ()


class PreferencesPage[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Preference form group with sections, gathering the sections, confirmations, shortcuts, and stores every other row declares, firing `changed` on each edit."""

    form: type[T]
    sections: tuple[Section, ...] = ()
    changed: Signal = msgspec.field(default_factory=Signal)

    # --- [ACCESS]

    def value[F](self, member: Callable[[T], F]) -> Callable[[Identity], Outcome[F]]:
        """Returns reader of one preference member per identity."""
        return lambda identity: read(identity, self.form).map(member)

    # --- [GATHERING]

    def gathered(self, identity: Identity, declared: tuple[Declaration | Gathering, ...], /) -> Outcome[Acquisitions]:
        """Declares page class, confirmation group, commands, and value popovers, building draw after operators register."""
        asks = tuple(row for row in declared if isinstance(row, operators.Operator) and any(isinstance(form, operators.Suppressible) for form in row.invoke.to_list()))
        bindings = tuple(row for row in declared if isinstance(row, keymaps.Keymap))
        stores = tuple(row for row in declared if isinstance(row, Attachment))
        added = tuple(row.preferences for row in declared if isinstance(row, Preferring))

        def attached() -> Outcome[tuple[Attached[bpy.types.PropertyGroup], ...]]:
            return accumulated(row.bound(identity) for row in stores)

        def purge_preferences(_call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Resets extension settings stored on data-blocks to defaults."""
            return (
                attached()
                .map(lambda held: sum(each.purged() for each in held))
                .map(
                    lambda count: (
                        operators.Done(notes=(("INFO", f"Cleared settings stored on {count} data-blocks"),)) if count else operators.Unchanged(reason="No data-block stores extension settings")
                    )
                )
            )

        purge = operators.Operator[operators.Empty](
            icon=Some("TRASH"),
            internal=True,
            form=operators.Empty,
            run=purge_preferences,
            invoke=Some(
                operators.Confirm[operators.Empty](
                    message=Some(lambda *_: attached().map(lambda held: f"Clear settings stored on {sum(each.stored(host) for each in held for host in each.hosts())} data-blocks")),
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
            *((Single(key="preferences_stored", title="Stored Settings", rows=(OperatorButton(target=purge),)),) if stores else ()),
        )
        sections = (*self.sections, *(section for part in added for section in part.sections), *extra)
        groups: tuple[type[bpy.types.PropertyGroup], ...] = (*nested(self.form), self.form)

        def built(names: tuple[str, str]) -> Acquisitions:
            prefix, grouped = names
            confirmations = type(grouped, (bpy.types.PropertyGroup,), {"__annotations__": {row.ask: bpy.props.BoolProperty(name=row.title, description=row.description, default=True) for row in asks}})
            paint: RowDraw

            def draw(holder: bpy.types.AddonPreferences, context: bpy.types.Context) -> None:
                layout = holder.layout
                layout.use_property_split, layout.use_property_decorate = True, False
                paint(layout, holder, context)

            confirming = {"asks": bpy.props.PointerProperty(type=confirmations, name="Confirmations")} if asks else {}
            kind = type(identity.package, (bpy.types.AddonPreferences,), {"bl_idname": identity.package, "__annotations__": {**annotated(self.form), **confirming}, "draw": draw})

            def arranged(parts: tuple[tuple[RowDraw, ...], Nest, tuple[RowDraw, ...]]) -> None:
                nonlocal paint
                scalars, nest, draws = parts

                def body(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
                    for each in scalars:
                        each(layout, holder, context)
                    nest(layout, holder, context, prefix)
                    for each in draws:
                        each(layout, holder, context)

                paint = body

            @contextmanager
            def prepared() -> Generator[Outcome[None]]:
                yield paired(
                    fields(kind, identity).bind(lambda pair: placed(pair[0], identity).map(lambda scalars: (scalars, pair[1]))), accumulated(section.drawn(identity) for section in sections)
                ).map(lambda pair: arranged((*pair[0], pair[1])))

            updating((*groups, confirmations), lambda _owner, _context: identity.reports.reported(self.changed.fired)().to_optional())
            return (
                *((Band.GROUPS, group) for group in (*groups, *((confirmations,) if asks else ()))),
                *(command for command in (show, *((purge,) if stores else ()))),
                *sections,
                *(Panel(section=popover, at=PopoverPanel()) for popover in reached(sections)),
                (Band.PREFERENCES, kind),
                (Band.INTERFACE, prepared()),
            )

        return paired(identity.identifier("preferences"), identity.name(bpy.types.PropertyGroup, "preferences_asks")).map(built)


# --- [OPERATIONS] -----------------------------------------------------------------------


def read[T: bpy.types.PropertyGroup](identity: Identity, form: type[T]) -> Outcome[T]:
    """Returns the extension's add-on preferences under the form group type its class declares."""
    holder: Option[T] = Option.of_optional(bpy.context.preferences.addons.get(identity.package)).bind(lambda addon: Option.of_optional(addon.preferences))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    return holder.to_result_with(partial(Fault.of, BlenderReason.ABSENT, identity.package, member=form.__name__))


# --- [COMPOSITION] ----------------------------------------------------------------------


def show_preferences(call: operators.Call[operators.Empty]) -> Outcome[operators.Done]:
    """Opens extension preferences from context window and area."""
    return Located.current(call.context).bind(lambda located: located.run(bpy.ops.preferences.addon_show, {"module": call.identity.package})).map(lambda _: operators.Done())


show = operators.Operator[operators.Empty](icon=Some("PREFERENCES"), undo=False, form=operators.Empty, run=show_preferences)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PageRows", "PreferencesPage", "Preferring", "read", "show"]
