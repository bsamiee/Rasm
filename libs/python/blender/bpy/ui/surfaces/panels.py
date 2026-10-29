"""Panels beside a stock panel, under a stock parent, in an extension Sidebar tab, in an editor region, or opened as a popover, and the add-on preferences page over a form group, the sections other rows add, confirmations, shortcuts, and stored settings."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial
import re
from typing import Any, Final, Literal, Protocol, runtime_checkable, Self, TYPE_CHECKING

import _bpy_types
from bl_ui.space_properties import PropertiesAnimationMixin
import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands import invocations, keymaps, operators
from blender.bpy.data.rna.conditions import admits, Condition
from blender.bpy.data.rna.mirrors import annotated, nested, Updated, updated
from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, Declaration, entered, Gathering, Registrable
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.layout.rows import acquired, layout_method, Member, OperatorButton, Property, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import fields, reached, Section, SectionDraws
from blender.bpy.ui.resources.icons import ICONS
from blender.bpy.ui.screens.areas import Located
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type FixedPlacement = Beside | Child | Sidebar | EditorRegion
type _PanelPlacement = FixedPlacement | Docked
type _Site = tuple[str, _PanelPlacement, Option[Condition]]


@runtime_checkable
class Preferring(Registrable, Protocol):
    """Registered row adding sections to its extension's preferences page."""

    @property
    def preferences(self) -> tuple[Section[Any], ...]:
        """Sections the row adds."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------

# --- [PLACEMENTS]


class Beside(msgspec.Struct, frozen=True):
    """Place of a stock panel: its editor and region, with the Properties tab or Sidebar tab it declares."""

    panel: type[_bpy_types.Panel]

    def members(self, _identity: Identity) -> dict[str, object]:
        """Copies the stock panel's editor, region, and whichever of tab context and Sidebar category it declares."""
        declared = _stock(self.panel)
        tabs = {"bl_context": catch(lambda: declared.bl_context, exception=AttributeError)(), "bl_category": catch(lambda: declared.bl_category, exception=AttributeError)()}
        return {"bl_space_type": declared.bl_space_type, "bl_region_type": declared.bl_region_type, **{member: tab for member, held in tabs.items() for tab in held.to_option().to_list()}}


class Child(msgspec.Struct, frozen=True, kw_only=True):
    """Place under a stock parent panel, which draws its children whatever their category."""

    parent: type[_bpy_types.Panel]

    def members(self, _identity: Identity) -> dict[str, object]:
        """Copies the parent's editor and region and names it as parent."""
        parent = _stock(self.parent)
        return {"bl_space_type": parent.bl_space_type, "bl_region_type": parent.bl_region_type, "bl_parent_id": definition(parent).identifier}


class Sidebar(msgspec.Struct, frozen=True, kw_only=True):
    """Extension Sidebar tab of an editor, marked with the extension's brand glyph."""

    space: "rna_enums.SpaceTypeItems"
    category: str

    def members(self, identity: Identity) -> dict[str, object]:
        """Places the panel in the tab, its icon the brand glyph while one is staged."""
        mark = identity.held.get(ICONS).bind(lambda icons: icons.brand)
        return {"bl_space_type": self.space, "bl_region_type": "UI", "bl_category": self.category, **mark.map(lambda value: {"bl_icon_value": value}).default_value({})}


class EditorRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Editor region laying out panels with no tab."""

    space: "rna_enums.SpaceTypeItems"
    region: "rna_enums.RegionTypeItems"

    def members(self, _identity: Identity) -> dict[str, object]:
        """Places the panel in the region."""
        return {"bl_space_type": self.space, "bl_region_type": self.region}


class Docked(msgspec.Struct, frozen=True, kw_only=True):
    """Placement read at registration and again after each `changed` firing."""

    read: Callable[[Identity], Outcome[FixedPlacement]]
    changed: Signal


class PanelCopy(msgspec.Struct, frozen=True, kw_only=True):
    """Further placement registering its own copy of the panel, with its own poll condition."""

    at: FixedPlacement
    condition: Option[Condition] = Nothing


# --- [PANELS]


class Panel(msgspec.Struct, frozen=True, kw_only=True):
    """Section registered as a panel at each placement, with child panels and a popover panel per store values popover its sections reach.

    `condition` hides the panel while it fails, as does a section holder the context does not reach, and `units` sets its width as a popover.
    """

    section: Section[Any]
    at: _PanelPlacement
    condition: Option[Condition] = Nothing
    copies: tuple[PanelCopy, ...] = ()
    children: tuple[Section[Any], ...] = ()
    options: frozenset[Literal["HIDE_HEADER", "HEADER_LAYOUT_EXPAND"]] = frozenset()
    units: Option[int] = Nothing

    @classmethod
    def popovers(cls, rows: Iterable[object]) -> tuple[Self, ...]:
        """Popover panel of each store values popover the rows' sections reach, registered in the top bar header, which draws no panels, so only a popover button or `wm.call_panel` opens it."""
        return tuple(cls(section=popover, at=POPOVER) for section in reached(rows) for popover in section.popover.to_list())

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the sections, a popover panel per values popover they reach, and the scope registering every placement."""
        roots = (self.section, *self.children)
        return Ok((*roots, *Panel.popovers(roots), (Band.INTERFACE, _displayed(self, identity))))


# --- [PREFERENCES]


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

        confirmation = invocations.Popup[operators.Empty](
            partial(bpy.types.WindowManager.invoke_confirm, confirm_text="Clear", icon="WARNING"),
            message=Some(lambda *_: attached().map(lambda held: f"Clear settings stored on {sum(each.stored(host) for each in held for host in each.hosts())} data-blocks")),
        )
        purge = operators.Operator.of(operators.Empty, icon=Some("TRASH"), internal=True, invoke=Some(confirmation))(purge_preferences)

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
        return Ok((
            Updated(groups=groups, change=changed),
            show_preferences,
            *((purge,) if stores else ()),
            *sections,
            *Panel.popovers((*rows, *sections)),
            *acquired(rows),
            (Band.PREFERENCES, prepared()),
        ))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [REGISTRATION]


@contextmanager
def _displayed(panel: Panel, identity: Identity) -> Generator[Outcome[None]]:
    """Registers the panel classes at every placement for the scope, re-registering a docked placement after each `changed` firing that moves it."""

    def slug(at: FixedPlacement) -> str:
        """Returns the key suffix of a copy placed at a placement: the stock panel's identifier, or the editor with its tab or region, in lowercase words joined by underscores."""
        match at:
            case Beside(panel=stock) | Child(parent=stock):
                text = definition(_stock(stock)).identifier
            case Sidebar(space=space, category=named) | EditorRegion(space=space, region=named):
                text = f"{space} {named}"
        return "_".join(re.findall(r"[a-z0-9]+", text.lower()))

    sites: tuple[_Site, ...] = ((panel.section.key, panel.at, panel.condition), *((f"{panel.section.key}_{slug(copy.at)}", copy.at, copy.condition) for copy in panel.copies))
    drawn = paired(panel.section.realized(identity), accumulated(child.realized(identity) for child in panel.children))
    held: list[tuple[FixedPlacement, ...]] = []
    classes = ExitStack()

    def classed(section: Section[Any], draws: SectionDraws, name: str, members: Mapping[str, object], options: frozenset[str], condition: Option[Condition]) -> type[bpy.types.Panel]:
        """Builds a panel class over the section's draws, closed unless the section opens first, polled by the condition and whether the context reaches the section's holder."""

        def poll(_cls: type[bpy.types.Panel], context: bpy.types.Context) -> bool:
            return admits(condition, context) and draws.present(context)

        headers = {member: layout_method(paint) for member, shown in (("draw_header", draws.head), ("draw_header_preset", draws.tail)) for paint in shown.to_list()}
        return type(
            name,
            (bpy.types.Panel,),
            {
                **members,
                "bl_idname": name,
                "bl_label": section.title,
                "bl_description": section.description,
                "bl_translation_context": section.translation_context,
                "bl_options": {*options, *(() if section.opened else ("DEFAULT_CLOSED",))},
                "poll": classmethod(poll),
                "draw": layout_method(draws.body),
                **headers,
            },
        )

    def placed(draws: tuple[SectionDraws, tuple[SectionDraws, ...]], stem: str, at: FixedPlacement, condition: Option[Condition]) -> Outcome[tuple[type[bpy.types.Panel], ...]]:
        """Builds the panel class at one placement, ordered just above the Animation panel as stock panels order themselves, the host inserting it after every panel of a lower or equal order, then its child panel classes in registration order."""
        top, children = draws
        placing = {**at.members(identity), "bl_owner_id": identity.package}
        ordered = {**placing, "bl_order": PropertiesAnimationMixin.bl_order - 1, **panel.units.map(lambda width: {"bl_ui_units_x": width}).default_value({})}
        shared = {key: value for key, value in placing.items() if key != "bl_parent_id"}

        def built(name: str, nested: tuple[str, ...]) -> tuple[type[bpy.types.Panel], ...]:
            return (
                classed(panel.section, top, name, ordered, panel.options, condition),
                *(classed(child, each, inner, {**shared, "bl_parent_id": name}, frozenset(), Nothing) for child, each, inner in zip(panel.children, children, nested, strict=True)),
            )

        return paired(identity.name(bpy.types.Panel, stem), accumulated(identity.name(bpy.types.Panel, f"{stem}_{child.key}") for child in panel.children)).map(lambda names: built(*names))

    def resolved() -> Outcome[tuple[FixedPlacement, ...]]:
        return accumulated(at.read(identity) if isinstance(at, Docked) else Ok(at) for _, at, _ in sites)

    def registered(now: tuple[FixedPlacement, ...]) -> Outcome[None]:
        with ExitStack() as entering:
            answer = drawn.bind(lambda draws: accumulated(placed(draws, stem, at, condition) for (stem, _, condition), at in zip(sites, now, strict=True))).bind(
                lambda built: entered(entering, tuple(cls for group in built for cls in group))
            )
            if answer.is_ok():
                classes.enter_context(entering.pop_all())
                held[:] = [now]
            return answer

    def rebuilt() -> Outcome[Option[str]]:
        def replaced(now: tuple[FixedPlacement, ...]) -> Outcome[None]:
            if held == [now]:
                return Ok(None)
            previous = tuple(held)
            classes.close()
            held.clear()
            return registered(now).or_else_with(lambda faults: paired(Error(faults), accumulated(map(registered, previous))).map(lambda _: None))

        return resolved().bind(replaced).map(lambda _: Nothing)

    with ExitStack() as scope:
        scope.enter_context(classes)
        pending = scope.enter_context(identity.reports.deferred(rebuilt))
        for signal in dict.fromkeys(at.changed for _, at, _ in sites if isinstance(at, Docked)):
            scope.enter_context(signal.followed(lambda: Ok(pending())))
        yield resolved().bind(registered)


# --- [PLACEMENTS]


def _stock(panel: type[_bpy_types.Panel]) -> type[bpy.types.Panel]:
    """Returns a stock panel as the RNA panel type declaring its placement members, which the stubs give `_bpy_types.Panel` no sight of."""
    declared: type[bpy.types.Panel] = panel  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return declared


# --- [PREFERENCES]


def read[T: bpy.types.PropertyGroup](identity: Identity, form: type[T]) -> Outcome[T]:
    """Returns the extension's add-on preferences as the form group its class declares, which no host type states.

    Args:
        identity: Extension whose preferences row is read.
        form: Form group the preferences class copies.

    Returns:
        The preferences, absent fault while the extension holds none.
    """
    preferences = identity.preferences(bpy.types.AddonPreferences)
    held: Option[T] = preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return held.to_result_with(partial(Fault.of, Reason.ABSENT, identity.package, member=form.__name__))


@operators.Operator.of(operators.Empty, icon=Some("PREFERENCES"), undo=Nothing)
def show_preferences(call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
    """Opens extension preferences from context window and area."""
    return Located.current(call.context).bind(lambda located: located.run(bpy.ops.preferences.addon_show, {"module": call.identity.package})).map(lambda _: operators.Done())


# --- [COMPOSITION] ----------------------------------------------------------------------

POPOVER: Final = EditorRegion(space="TOPBAR", region="HEADER")

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["POPOVER", "Beside", "Child", "Docked", "EditorRegion", "FixedPlacement", "Panel", "PanelCopy", "PreferencesPage", "Preferring", "Sidebar", "read", "show_preferences"]
