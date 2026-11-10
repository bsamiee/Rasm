"""Menu and header classes an extension registers or appends to stock ones."""

from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager, ExitStack
from functools import partial
from itertools import dropwhile
import operator
from typing import TYPE_CHECKING

import _bpy_types  # ruff: ignore[import-private-name]
from bl_ui import UI_MT_button_context_menu, UI_MT_list_item_context_menu
import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands.operators import Command, idname, Operation, Placement, PopupKind, PopupTarget
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import acquired_rows, Acquisitions, Classes, Tier
from rasm_bpy.data._rna.conditions import Condition, polled, satisfied
from rasm_bpy.data._rna.paths import rna_definition, Struct, subclass
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import ConditionalRow, Layout, layout_method, LayoutOwner, Opens, OperatorButton, Property, realized, RowContent, RowDraw
from rasm_bpy.ui._layout.values import Captured, Listing, Store
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Slot = OperatorButton | Opens | Property | RowDraw

# --- [MODELS] ---------------------------------------------------------------------------

# --- [MENUS]


class Dropdown(msgspec.Struct, frozen=True, kw_only=True):
    """Entries in declaration order, or in the order `order` reads from the context as entry indices, an index past the entries skipped."""

    entries: tuple[RowContent, ...]
    order: Option[Callable[[bpy.types.Context], Iterable[int]]] = Nothing
    search: bool = False


class Submenus(msgspec.Struct, frozen=True, kw_only=True):
    """Entries filed into one submenu per group key in the order keys first appear, each titled with its key's display name, a single group drawing as one searchable list."""

    entries: tuple[tuple[Key, RowContent], ...]
    search: bool = False


class Pie(msgspec.Struct, frozen=True, kw_only=True):
    """Pie slots in radial fill order, an empty slot kept free before a filled one, an expanded enum or `operator_enum` button taking a slot per item."""

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

    # --- [STATE]

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

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns submenus, row acquisitions, then the menu class built when the interface tier opens."""
        match self.body:
            case Dropdown(entries=rows, order=order, search=search):
                submenus: tuple[Menu, ...] = ()
                drawing = partial(_ordered, identity, rows, order)
            case Submenus(entries=entries) if len(dict.fromkeys(group for group, _ in entries)) < 2:
                return msgspec.structs.replace(self, body=Dropdown(entries=tuple(row for _, row in entries), search=True)).acquisitions(identity)
            case Submenus(entries=entries, search=search):
                submenus = tuple(
                    Menu(
                        key=f"{self.key}_{group}",
                        title=Some(label := bpy.path.display_name(group)),
                        description=f"{label}: {self.description}",
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
        return (*submenus, *acquired_rows(rows), (Tier.INTERFACE, Classes(build=partial(_menu, identity, self.key, title, self.description, drawing, search=search, condition=self.condition))))


class PresetMenu[T: msgspec.Struct = Snapshot](msgspec.Struct, frozen=True, kw_only=True):
    """Searchable preset listing with trailing rows."""

    source: Store[bpy.types.PropertyGroup] | Listing[T] | Captured
    entries: tuple[RowContent, ...] = ()

    def acquisitions(self, identity: Identity) -> Acquisitions | Outcome[Acquisitions]:
        """Returns source acquisitions, then the preset menu, none while a store holds no presets."""
        match self.source:
            case Store() as store:
                return (
                    store,
                    *acquired_rows(self.entries),
                    (Tier.INTERFACE, Classes(build=lambda: store.listing(identity).bind(lambda found: found.map(partial(_preset_menu, identity, self.entries)).default_value(Ok(()))))),
                )
            case Listing() as listing:
                return _listed(identity, self.entries, listing)
            case Captured() as captured:
                return captured.listing().map(partial(_listed, identity, self.entries))


# --- [HEADERS]


class Header(msgspec.Struct, frozen=True, kw_only=True):
    """Extension header drawn after an editor region's stock headers while its condition holds, `menus` leading the rows inline or folded by `area.show_menus`."""

    key: Key
    space: "rna_enums.SpaceTypeItems"
    rows: tuple[RowContent, ...]
    menus: Option[Menu] = Nothing
    region: "rna_enums.RegionTypeItems" = "HEADER"
    condition: Option[Condition] = Nothing

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns the editor menus, row acquisitions, then the header class built when the interface tier opens."""

        def built() -> Outcome[tuple[type, ...]]:
            name, menus = identity.name(bpy.types.Header, self.key), self.menus.map(lambda menu: identity.name(bpy.types.Menu, menu.key))
            members = {"bl_idname": name, "bl_space_type": self.space, "bl_region_type": self.region, **polled(self.condition)}
            return realized(self.rows, identity).map(
                lambda draw: (type(name, (bpy.types.Header,), {**members, "draw": layout_method(menus.map(lambda menu: partial(_collapsed, identity, menu, draw)).default_value(draw))}),)
            )

        return (*self.menus.to_list(), *acquired_rows(self.rows), (Tier.INTERFACE, Classes(build=built)))


# --- [EXTENSIONS]


class ContextMenu(msgspec.Struct, frozen=True, kw_only=True):
    """Button or list row context menu showing content only over the hovered button a struct type, a bound member, or an operator names."""

    menu: type[UI_MT_button_context_menu | UI_MT_list_item_context_menu]
    hovered: type[Struct] | Member | Command | Operation

    def pointing(self, identity: Identity) -> Outcome[Callable[[bpy.types.Context], bool]]:
        """Resolves the hovered button test once, a member's property and an operator's type definition read at realization."""
        match self.hovered:
            case Member() as member:
                return member.realized(identity).map(lambda held: partial(_hovered, "button_prop", partial(operator.eq, held.descriptor.prop)))
            case type() as kind:
                return Ok(partial(_hovered, "button_pointer", lambda pointer: isinstance(pointer, kind)))
            case command:
                dotted = idname(identity, command)

                def operation() -> bpy.types.Struct:
                    module, _, name = dotted.partition(".")
                    held = getattr(getattr(bpy.ops, module), name)
                    held.get_rna_type()
                    return rna_definition(held.get_rna_type())

                return (
                    catch(operation, exception=KeyError)()
                    .map_error(partial(Fault.of, Reason.ABSENT, "bpy.ops", member=dotted))
                    .map(lambda kind: partial(_hovered, "button_operator", lambda held: rna_definition(held) == kind))
                )


class Append(msgspec.Struct, frozen=True, kw_only=True):
    """Layout content drawn after a stock panel, menu, header, or context menu, or before it when `prepend` is set, while the condition holds.

    Appended content inherits the layout the stock draw leaves, its property split included, prepended content draws ahead of it, and menu content keeps a separator from the stock entries.
    """

    target: type[_bpy_types.Panel | _bpy_types.Menu | _bpy_types.Header] | ContextMenu
    rows: tuple[RowContent, ...]
    prepend: bool = False
    condition: Option[Condition] = Nothing

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns what the content holds, then the draw function held on the target for the scope."""
        return (*acquired_rows(self.rows), (Tier.APPENDS, _extended(self, identity)))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSES]


def _menu(identity: Identity, key: Key, title: str, description: str, drawing: Callable[[], Outcome[RowDraw]], *, search: bool, condition: Option[Condition] = Nothing) -> Outcome[tuple[type, ...]]:
    """Builds the menu class, a searchable menu opened from code or a key leading with its search entry."""
    name = identity.name(bpy.types.Menu, key)
    entry = OperatorButton(target=Placement(bpy.ops.wm.search_single_menu, {"menu_idname": name}), keywords={"text": "Search..."}, icon=Some("VIEWZOOM"))
    heading = realized((entry,), identity).map(Some) if search else Ok(Nothing)
    members = {"bl_idname": name, "bl_label": title, "bl_description": description, "bl_options": {"SEARCH_ON_KEY_PRESS"} if search else set(), **polled(condition)}
    return paired(heading, drawing()).map(lambda pair: (type(name, (bpy.types.Menu,), {**members, "draw": layout_method(partial(_headed, *pair))}),))


def _preset_menu[S: msgspec.Struct](identity: Identity, entries: tuple[RowContent, ...], listing: Listing[S]) -> Outcome[tuple[type, ...]]:
    """Builds the searchable preset menu titled after the listing's preset."""
    name = bpy.path.display_name(listing.preset.key)
    owned = (listing, *entries)
    return _menu(identity, listing.key, f"{name} Presets", f"Apply, add, or import {name.lower()} presets", partial(realized, owned, identity), search=True)


def _listed[S: msgspec.Struct](identity: Identity, entries: tuple[RowContent, ...], listing: Listing[S]) -> Acquisitions:
    """Returns the listing and trailing rows, then the preset menu over them."""
    return (*acquired_rows((listing, *entries)), (Tier.INTERFACE, Classes(build=partial(_preset_menu, identity, entries, listing))))


@contextmanager
def _extended(append: Append, identity: Identity) -> Generator[Outcome[None]]:
    """Adds the draw function to the target for the scope once the content and hovered test realize."""
    target = append.target.menu if isinstance(append.target, ContextMenu) else append.target
    pointed: Outcome[Option[Callable[[bpy.types.Context], bool]]] = append.target.pointing(identity).map(Some) if isinstance(append.target, ContextMenu) else Ok(Nothing)
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
    for index in order.map(lambda read: read(context)).default_with(lambda: range(len(draws))):
        if 0 <= index < len(draws):
            draws[index](layout, holder, context)


def _headed(heading: Option[RowDraw], draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Leads with the search entry where the opener sets the `EXEC_REGION_WIN` context of code and keymap calls, as stock add menus do, then draws the entries."""
    for entry in heading.to_list() if layout.operator_context == "EXEC_REGION_WIN" else ():
        entry(layout, holder, context)
        layout.separator()
    draw(layout, holder, context)


def _collapsed(identity: Identity, menu: str, draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the editor menus through the host's collapsible header form, logging a menu `bpy.types` lacks, then the rows."""
    identity.reports.faulted(subclass(bpy.types.Menu, menu).map(lambda kind: kind.draw_collapsible(context, layout)))
    draw(layout, holder, context)


def _radial_drawing(identity: Identity, slots: tuple[Option[_Slot], ...]) -> Outcome[RowDraw]:
    """Realizes slots up to the last filled one into the pie draw, a plain draw taking one column slot."""

    def slot(row: _Slot) -> Outcome[RowDraw]:
        return realized((row if isinstance(row, ConditionalRow) else Layout(rows=(row,), container=Some(bpy.types.UILayout.column)),), identity)

    filled = reversed(tuple(dropwhile(Option.is_none, reversed(slots))))
    return accumulated(each.map(lambda row: slot(row).map(Some)).default_with(lambda: Ok(Nothing)) for each in filled).map(lambda draws: partial(_radial, draws))


def _radial(draws: tuple[Option[RowDraw], ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws each slot into the pie, a separator keeping an empty slot free."""
    pie = layout.menu_pie()
    for drawn in draws:
        drawn.map(lambda slot: slot(pie, holder, context)).default_with(pie.separator)


def _shown(append: Append, draw: RowDraw, pointed: Option[Callable[[bpy.types.Context], bool]], extended: LayoutOwner, context: bpy.types.Context) -> None:
    """Draws the content while the condition holds and a context menu's hovered button passes its test."""
    if (layout := extended.layout) is None or not (pointed.map(lambda test: test(context)).default_value(value=True) and satisfied(append.condition, context)):
        return
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
