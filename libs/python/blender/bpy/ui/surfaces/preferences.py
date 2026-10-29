"""Add-on preferences page over a form group, the sections other rows add, confirmations, shortcuts, and stored settings, with its reads."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import partial
from typing import Any, Protocol, runtime_checkable

import bpy
from expression import Nothing, Ok, Some
import msgspec

from blender.bpy.commands import invocations, keymaps, operators
from blender.bpy.data.rna.mirrors import annotated, nested, Updated, updated
from blender.bpy.data.rna.paths import Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, Declaration, entered, Gathering, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import acquired, OperatorButton, Property, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import fields, Section
from blender.bpy.ui.screens.areas import Located
from blender.bpy.ui.surfaces.panels import Panel
from blender.records.faults import accumulated, Fault, Outcome

# --- [TYPES] ----------------------------------------------------------------------------


@runtime_checkable
class Preferring(Registrable, Protocol):
    """Registered row adding sections to its extension's preferences page."""

    @property
    def preferences(self) -> tuple[Section[Any], ...]:
        """Sections the row adds."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class PreferencesPage[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Preference form group with sections, gathering the sections, confirmations, shortcuts, and stores every other row declares, firing `changed` on each edit.

    Each suppressible command's `ask_<key>` flag sits on the preferences beside the form fields, and the Confirmations section turns a dismissed confirmation back on.
    """

    form: type[T]
    sections: tuple[Section[Any], ...] = ()
    changed: Signal = msgspec.field(default_factory=Signal)

    # --- [ACCESS]

    def value[F](self, member: Callable[[T], F]) -> Callable[[Identity], Outcome[F]]:
        """Returns reader of one preference member per identity."""
        return lambda identity: read(identity, self.form).map(member)

    # --- [GATHERING]

    def gathered(self, identity: Identity, declared: tuple[Declaration | Gathering, ...], /) -> Outcome[Acquisitions]:
        """Declares the preferences class, its commands, sections, and value popovers, registering the class where its draws realize."""
        asks = tuple(row for row in declared if isinstance(row, operators.Operator) and any(isinstance(form, invocations.Suppressible) for form in row.invoke.to_list()))
        bindings = tuple(row for row in declared if isinstance(row, keymaps.Keymap))
        stores = tuple(row for row in declared if isinstance(row, Attachment))

        def attached() -> Outcome[tuple[Attached[bpy.types.PropertyGroup], ...]]:
            return accumulated(row.bound(identity) for row in stores)

        def purge_preferences(_call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Resets extension settings stored on data-blocks to defaults."""
            return attached().map(
                lambda held: (
                    operators.Done(notes=(("INFO", f"Cleared settings stored on {count} data-blocks"),))
                    if (count := sum(each.purged() for each in held))
                    else operators.Unchanged(reason="No data-block stores extension settings")
                )
            )

        purge = operators.Operator[operators.Empty](
            icon=Some("TRASH"),
            internal=True,
            form=operators.Empty,
            run=purge_preferences,
            invoke=Some(
                invocations.Popup[operators.Empty](
                    partial(bpy.types.WindowManager.invoke_confirm, confirm_text="Clear", icon="WARNING"),
                    message=Some(lambda *_: attached().map(lambda held: f"Clear settings stored on {sum(each.stored(host) for each in held for host in each.hosts())} data-blocks")),
                )
            ),
        )

        def changed(_owner: Struct, _context: bpy.types.Context) -> None:
            self.changed.fired().default_with(identity.reports.hold)

        flags = {invocations.Suppressible.flag(row.key): bpy.props.BoolProperty(name=row.title, description=row.description, default=True, update=changed) for row in asks}
        paints: list[RowDraw] = []

        def draw(holder: bpy.types.AddonPreferences, context: bpy.types.Context) -> None:
            layout = holder.layout
            layout.use_property_split, layout.use_property_decorate = True, False
            for paint in paints:
                paint(layout, holder, context)

        kind = type(
            identity.package,
            (bpy.types.AddonPreferences,),
            {"bl_idname": identity.package, "__annotations__": {**{name: updated(held, changed) for name, held in annotated(self.form).items()}, **flags}, "draw": draw},
        )
        rows: tuple[RowContent, ...] = tuple(row for row in fields(kind, "preferences") if not (isinstance(row, Property) and row.target.path[0] in flags))
        sections = (
            *self.sections,
            *(section for row in declared if isinstance(row, Preferring) for section in row.preferences),
            *((Section(key="preferences_confirmations", title="Confirmations", rows=tuple(Property(target=Member(root=kind, path=(ask,))) for ask in flags)),) if flags else ()),
            *((Section(key="preferences_shortcuts", title="Shortcuts", rows=tuple(binding.drawn for binding in bindings)),) if bindings else ()),
            *((Section(key="preferences_stored", title="Stored Settings", rows=(OperatorButton(target=purge),)),) if stores else ()),
        )

        @contextmanager
        def prepared() -> Generator[Outcome[None]]:
            with ExitStack() as stack:
                stack.callback(paints.clear)
                yield entered(stack, (kind,)).bind(lambda _: realized((*rows, *sections), identity)).map(paints.append)

        groups: tuple[type[bpy.types.PropertyGroup], ...] = (*nested(self.form), self.form)
        return Ok((Updated(groups=groups, change=changed), show, *((purge,) if stores else ()), *sections, *Panel.popovers((*rows, *sections)), *acquired(rows), (Band.PREFERENCES, prepared())))


# --- [OPERATIONS] -----------------------------------------------------------------------


def read[T: bpy.types.PropertyGroup](identity: Identity, form: type[T]) -> Outcome[T]:
    """Returns the extension's add-on preferences as the form group its class declares."""
    return identity.preferences(bpy.types.AddonPreferences).map(partial(_as_form, form)).to_result_with(partial(Fault.of, BlenderReason.ABSENT, identity.package, member=form.__name__))


def show_preferences(call: operators.Call[operators.Empty]) -> Outcome[operators.Done]:
    """Opens extension preferences from context window and area."""
    return Located.current(call.context).bind(lambda located: located.run(bpy.ops.preferences.addon_show, {"module": call.identity.package})).map(lambda _: operators.Done())


def _as_form[T: bpy.types.PropertyGroup](_form: type[T], held: bpy.types.AddonPreferences) -> T:
    """Returns the preferences as the form group whose properties its class copies, which no host type states."""
    copied: T = held  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return copied


# --- [COMPOSITION] ----------------------------------------------------------------------

show = operators.Operator[operators.Empty](icon=Some("PREFERENCES"), undo=Nothing, form=operators.Empty, run=show_preferences)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PreferencesPage", "Preferring", "read", "show"]
