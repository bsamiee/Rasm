"""Menus of ordered rows, submenus, or pie slots, preset menus, editor headers, and content appended to stock panels, menus, headers, and context menus, each refusing the sections whose layout panels the host refuses there."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial
from itertools import dropwhile
import operator
from typing import TYPE_CHECKING

import _bpy_types
from bl_ui import UI_MT_button_context_menu, UI_MT_list_item_context_menu
import bpy
from expression import Nothing, Ok, Option, Result, Some
import msgspec

from blender.bpy.commands.calls import Command, idname, Operation, Placement, PopupKind, PopupTarget
from blender.bpy.data.rna.conditions import Condition, polled, satisfied
from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Tier
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import acquired, ConditionalRow, layout_method, LayoutOwner, Opens, OperatorButton, Property, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import sections_in
from blender.bpy.ui.layout.values import Captured, Listing, Store
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Slot = OperatorButton | Opens | Property | RowDraw
type _Pointed = Callable[[bpy.types.Context], bool]
type _Built = Outcome[tuple[type, ...]]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [MENUS]


class Dropdown(msgspec.Struct, frozen=True, kw_only=True):
    """Entries in declaration order, or in the order `order` reads from the context as entry indices, an index past the entries skipped."""

    entries: tuple[RowContent, ...]
    order: Option[Callable[[bpy.types.Context], Iterable[int]]] = Nothing
    search: bool = False


class Submenus(msgspec.Struct, frozen=True, kw_only=True):
    """Entries filed into one submenu per group key in the order keys first appear, each titled with its key's display name, the native add-menu form."""

    entries: tuple[tuple[Key, RowContent], ...]
    search: bool = False


class Pie(msgspec.Struct, frozen=True, kw_only=True):
    """Pie slots in Blender's radial fill order, an empty slot kept free while a later slot holds an entry, an expanded enum or `operator_enum` button taking one slot per item."""

    west: Option[_Slot] = Nothing
    east: Option[_Slot] = Nothing
    south: Option[_Slot] = Nothing
    north: Option[_Slot] = Nothing
    north_west: Option[_Slot] = Nothing
    north_east: Option[_Slot] = Nothing
    south_west: Option[_Slot] = Nothing
    south_east: Option[_Slot] = Nothing


class Menu(msgspec.Struct, frozen=True, kw_only=True):
    """Extension menu with poll condition, titled after its key unless `title` states otherwise."""

    key: Key
    description: str
    body: Dropdown | Submenus | Pie
    title: Option[str] = Nothing
    condition: Option[Condition] = Nothing

    # --- [PLACEMENT]

    @property
    def popup(self) -> Placement:
        """Placement buttons and keymaps open the menu through, as a pie for a pie body."""
        return Placement(PopupTarget(self.key, PopupKind.PIE if isinstance(self.body, Pie) else PopupKind.MENU))

    # --- [ACQUISITION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns submenus, row acquisitions, then the menu class built when the interface band opens."""
        match self.body:
            case Dropdown(entries=rows, order=order, search=search):
                submenus: tuple[Menu, ...] = ()
                drawing = partial(_ordered, identity, rows, order)
            case Submenus(entries=entries, search=search):
                submenus = tuple(
                    Menu(
                        key=f"{self.key}_{group}",
                        title=Some(bpy.path.display_name(group)),
                        description=f"{bpy.path.display_name(group)}: {self.description}",
                        body=Dropdown(entries=tuple(row for held, row in entries if held == group)),
                        condition=self.condition,
                    )
                    for group in dict.fromkeys(held for held, _ in entries)
                )
                rows = tuple(Opens(target=PopupTarget(group.key, PopupKind.MENU)) for group in submenus)
                drawing = partial(realized, rows, identity)
            case Pie():
                slots, search, submenus = msgspec.structs.astuple(self.body), False, ()
                rows = tuple(row for slot in slots for row in slot.to_list())
                drawing = partial(_radial_drawing, identity, slots)
        title = self.title.default_with(lambda: bpy.path.display_name(self.key))
        return Ok((*submenus, *acquired(rows), (Tier.INTERFACE, partial(_menu, identity, self.key, title, self.description, rows, drawing, search=search, condition=self.condition))))


class PresetMenu[T: msgspec.Struct = Snapshot](msgspec.Struct, frozen=True, kw_only=True):
    """Searchable preset listing with trailing rows."""

    source: Store[bpy.types.PropertyGroup] | Listing[T] | Captured
    entries: tuple[RowContent, ...] = ()

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns source acquisitions, then the preset menu, none while a store holds no presets."""
        match self.source:
            case Store() as store:
                return Ok((
                    store,
                    *acquired(self.entries),
                    (Tier.INTERFACE, lambda: store.listing(identity).bind(lambda found: found.map(partial(_preset_menu, identity, self.entries)).default_value(Ok(())))),
                ))
            case Listing() as listing:
                return _listed(identity, self.entries, listing)
            case Captured() as captured:
                return captured.listing().bind(partial(_listed, identity, self.entries))


# --- [HEADERS]


class Header(msgspec.Struct, frozen=True, kw_only=True):
    """Extension header drawn after the stock headers of an editor's header region, with poll condition, `menus` drawn ahead of the rows inline or folded by `area.show_menus`."""

    key: Key
    space: "rna_enums.SpaceTypeItems"
    rows: tuple[RowContent, ...]
    menus: Option[Menu] = Nothing
    region: "rna_enums.RegionTypeItems" = "HEADER"
    condition: Option[Condition] = Nothing

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the editor menus, row acquisitions, then the header class built when the interface band opens."""

        def drawing(_name: str) -> Outcome[RowDraw]:
            menus = self.menus.map(lambda menu: identity.name(bpy.types.Menu, menu.key).map(Some)).default_value(Ok(Nothing))
            return menus.bind(lambda named: realized(self.rows, identity).map(lambda draw: named.map(lambda menu: partial(_collapsed, menu, draw)).default_value(draw)))

        members = {"bl_space_type": self.space, "bl_region_type": self.region}
        return Ok((*self.menus.to_list(), *acquired(self.rows), (Tier.INTERFACE, partial(_classed, identity, bpy.types.Header, self.key, self.rows, members, self.condition, drawing))))


# --- [EXTENSIONS]


class ContextMenu(msgspec.Struct, frozen=True, kw_only=True):
    """Button or list row context menu showing content only over the hovered button a struct type, a bound member, or an operator names."""

    menu: type[UI_MT_button_context_menu | UI_MT_list_item_context_menu]
    hovered: type[Struct] | Member | Command | Operation

    def pointing(self, identity: Identity) -> Outcome[_Pointed]:
        """Resolves the hovered button test once, a member's property and an operator's type definition read at realization."""
        match self.hovered:
            case Member() as member:
                return member.realized(identity).map(lambda held: partial(_hovered, "button_prop", partial(operator.eq, held.descriptor.prop)))
            case type() as kind:
                return Ok(partial(_hovered, "button_pointer", lambda pointer: isinstance(pointer, kind)))
            case command:
                return idname(identity, command).bind(
                    lambda dotted: (
                        Result[tuple[str, str, str], Faults]
                        .Ok(dotted.partition("."))
                        .filter_with(lambda parts: parts[2] in dir(getattr(bpy.ops, parts[0])), lambda _: Fault.of(Reason.ABSENT, "bpy.ops", member=dotted))
                        .map(lambda parts: partial(_hovered, "button_operator", lambda held: definition(held).identifier == f"{parts[0].upper()}_OT_{parts[2]}"))
                    )
                )


class Append(msgspec.Struct, frozen=True, kw_only=True):
    """Layout content drawn after a stock panel, menu, header, or context menu, or before it when `prepend` is set, while the condition holds.

    Appended content inherits the layout the stock draw leaves, its property split included, prepended content draws ahead of it, and menu content keeps a separator from the stock entries.
    """

    target: type[_bpy_types.Panel | _bpy_types.Menu | _bpy_types.Header] | ContextMenu
    rows: tuple[RowContent, ...]
    prepend: bool = False
    condition: Option[Condition] = Nothing

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns what the content holds, then the draw function held on the target for the scope."""
        return Ok((*acquired(self.rows), (Tier.APPENDS, _extended(self, identity))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSES]


def _classed(
    identity: Identity,
    base: type[bpy.types.Menu | bpy.types.Header],
    key: Key,
    rows: tuple[RowContent, ...],
    members: Mapping[str, object],
    condition: Option[Condition],
    drawing: Callable[[str], Outcome[RowDraw]],
) -> _Built:
    """Builds the menu or header class once its rows realize under the name its draw reads, refusing a section among the rows, whose layout panel the host refuses in menus, pies, and headers."""
    return (
        Ok(sections_in(rows))
        .filter_with(lambda found: not found, lambda found: Fault.of(Reason.OUTSIDE, key, ", ".join(section.key for section in found), member="rows without layout panels"))
        .bind(lambda _: identity.name(base, key))
        .bind(lambda name: drawing(name).map(lambda draw: (type(name, (base,), {"bl_idname": name, **members, "draw": layout_method(draw), **polled(condition)}),)))
    )


def _menu(
    identity: Identity, key: Key, title: str, description: str, rows: tuple[RowContent, ...], drawing: Callable[[], Outcome[RowDraw]], *, search: bool, condition: Option[Condition] = Nothing
) -> _Built:
    """Builds the menu class, a searchable menu opened from code or a key leading with its search entry."""

    def headed(name: str) -> Outcome[RowDraw]:
        entry = OperatorButton(target=Placement(bpy.ops.wm.search_single_menu, {"menu_idname": name}), keywords={"text": "Search..."}, icon=Some("VIEWZOOM"))
        heading = realized((entry,), identity).map(Some) if search else Ok(Nothing)
        return paired(heading, drawing()).map(lambda pair: partial(_headed, *pair))

    members = {"bl_label": title, "bl_description": description, "bl_options": {"SEARCH_ON_KEY_PRESS"} if search else set()}
    return _classed(identity, bpy.types.Menu, key, rows, members, condition, headed)


def _preset_menu[S: msgspec.Struct](identity: Identity, entries: tuple[RowContent, ...], listing: Listing[S]) -> _Built:
    """Builds the searchable preset menu titled after the listing's preset."""
    name = bpy.path.display_name(listing.preset.key)
    owned = (listing, *entries)
    return _menu(identity, listing.key, f"{name} Presets", f"Apply, add, or import {name.lower()} presets", owned, partial(realized, owned, identity), search=True)


def _listed[S: msgspec.Struct](identity: Identity, entries: tuple[RowContent, ...], listing: Listing[S]) -> Outcome[Acquisitions]:
    """Returns the listing and trailing rows, then the preset menu over them."""
    return Ok((*acquired((listing, *entries)), (Tier.INTERFACE, partial(_preset_menu, identity, entries, listing))))


@contextmanager
def _extended(append: Append, identity: Identity) -> Generator[Outcome[None]]:
    """Adds the draw function to the target for the scope once the content and hovered test realize."""
    target = append.target.menu if isinstance(append.target, ContextMenu) else append.target
    pointed: Outcome[Option[_Pointed]] = append.target.pointing(identity).map(Some) if isinstance(append.target, ContextMenu) else Ok(Nothing)
    drawn = pointed.bind(lambda test: realized(append.rows, identity).map(lambda draw: partial(_shown, append, draw, test)))
    with ExitStack() as stack:
        for draw in drawn.to_option().to_list():
            (target.prepend if append.prepend else target.append)(draw)  # ty: ignore[invalid-argument-type]
            stack.callback(target.remove, draw)  # ty: ignore[invalid-argument-type]
        yield drawn.map(lambda _: None)


# --- [DRAWING]


def _ordered(identity: Identity, rows: tuple[RowContent, ...], order: Option[Callable[[bpy.types.Context], Iterable[int]]]) -> Outcome[RowDraw]:
    """Realizes each entry into a draw picked by the context's order, declaration order while none is stated."""
    return accumulated(realized((row,), identity) for row in rows).map(lambda draws: partial(_picked, draws, order))


def _picked(draws: tuple[RowDraw, ...], order: Option[Callable[[bpy.types.Context], Iterable[int]]], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the entries the order names, skipping an index past them."""
    for index in order.map(lambda read: tuple(read(context))).default_value(tuple(range(len(draws)))):
        for draw in draws[index : index + 1] if index >= 0 else ():
            draw(layout, holder, context)


def _headed(heading: Option[RowDraw], draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Leads with the search entry where the opener sets the `EXEC_REGION_WIN` context of code and keymap calls, as stock add menus do, then draws the entries."""
    for entry in heading.to_list() if layout.operator_context == "EXEC_REGION_WIN" else ():
        entry(layout, holder, context)
        layout.separator()
    draw(layout, holder, context)


def _collapsed(menu: str, draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the editor menus through the host's collapsible header form, then the rows."""
    if isinstance(kind := getattr(bpy.types, menu, None), type) and issubclass(kind, bpy.types.Menu):
        kind.draw_collapsible(context, layout)
    draw(layout, holder, context)


def _radial_drawing(identity: Identity, slots: tuple[Option[_Slot], ...]) -> Outcome[RowDraw]:
    """Realizes slots up to the last filled one into the pie draw, a property slot holding a boolean or enum."""

    def slot(row: _Slot) -> Outcome[tuple[RowDraw, bool]]:
        checked = row.target.realized(identity).bind(lambda held: held.accepts((bpy.types.BoolProperty, bpy.types.EnumProperty))).map(lambda _: None) if isinstance(row, Property) else Ok(None)
        return checked.bind(lambda _: realized((row,), identity)).map(lambda draw: (draw, isinstance(row, ConditionalRow)))

    filled = reversed(tuple(dropwhile(Option.is_none, reversed(slots))))
    return accumulated(each.map(lambda row: slot(row).map(Some)).default_with(lambda: Ok(Nothing)) for each in filled).map(lambda draws: partial(_radial, draws))


def _radial(draws: tuple[Option[tuple[RowDraw, bool]], ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws each slot into the pie, a separator keeping an empty slot free."""
    pie = layout.menu_pie()
    for drawn in draws:
        drawn.map(lambda slot: slot[0](pie if slot[1] else pie.column(), holder, context)).default_with(pie.separator)


def _shown(append: Append, draw: RowDraw, pointed: Option[_Pointed], extended: LayoutOwner, context: bpy.types.Context) -> None:
    """Draws the content while the condition holds and a context menu's hovered button passes its test."""
    if not (pointed.map(lambda test: test(context)).default_value(True) and satisfied(append.condition, context)):
        return
    layout: bpy.types.UILayout = extended.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    match extended, append.prepend:
        case bpy.types.Menu(), False:
            layout.separator()
            draw(layout, context, context)
        case bpy.types.Menu(), True:
            draw(layout, context, context)
            layout.separator()
        case _:
            draw(layout, context, context)


def _hovered(member: str, test: Callable[[Struct], bool], context: bpy.types.Context) -> bool:
    """Whether the context member a button context menu publishes (`button_pointer`, `button_prop`, `button_operator`) passes the test, false outside such a menu."""
    return Option.of_optional(getattr(context, member, None)).filter(test).is_some()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Append", "ContextMenu", "Dropdown", "Header", "Menu", "Pie", "PresetMenu", "Submenus"]
