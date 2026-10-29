"""Menus of ordered rows, submenus, or pie slots, preset menus, editor headers, content appended to stock panels, menus, headers, and context menus, and asset shelves, each refusing the sections whose layout panels the host refuses there."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from enum import Enum
from functools import partial, wraps
from itertools import dropwhile
import operator
from typing import TYPE_CHECKING

import _bpy_types
from bl_ui import UI_MT_button_context_menu, UI_MT_list_item_context_menu
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from blender.bpy.commands.calls import Called, Command, idname, Opened, Operation, Placement
from blender.bpy.commands.operators import Answer, Call, Empty, Operator
from blender.bpy.data.rna.conditions import admits, Condition, polled
from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.files.assets import AssetMark
from blender.bpy.ui.layout.rows import acquired, layout_method, Line, Member, Opens, OperatorButton, Property, realized, Row, RowContent, RowDraw
from blender.bpy.ui.layout.sections import reached
from blender.bpy.ui.layout.values import Captured, Listing, Store
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Slot = OperatorButton | Opens | Property | RowDraw
type _Extended = bpy.types.Panel | bpy.types.Menu | bpy.types.Header
type _Pointed = Callable[[bpy.types.Context], bool]
type _Built = Outcome[tuple[type, ...]]
type _Use = Callable[[Call[Empty], bpy.types.AssetRepresentation], Outcome[Answer]]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [MENUS]


class Dropdown(msgspec.Struct, frozen=True, kw_only=True):
    """Entries in declaration order, or in the order `order` reads from the context as entry indices, an index past the entries skipped."""

    entries: tuple[RowContent, ...]
    order: Option[Callable[[bpy.types.Context], Iterable[int]]] = Nothing
    search: bool = False


class Submenus[G: Enum](msgspec.Struct, frozen=True, kw_only=True):
    """Entries filed into one submenu per group in group declaration order, the native add-menu form."""

    entries: tuple[tuple[G, RowContent], ...]
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
    body: Dropdown | Submenus[Enum] | Pie
    title: Option[str] = Nothing
    condition: Option[Condition] = Nothing

    # --- [PLACEMENT]

    @property
    def popup(self) -> Placement:
        """Placement buttons and keymaps open the menu through, as a pie for a pie body."""
        return Placement(Opened(self.key, Called.PIE if isinstance(self.body, Pie) else Called.MENU))

    # --- [ACQUISITION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns submenus, row acquisitions, then the menu class built when the interface band opens."""
        match self.body:
            case Dropdown(entries=rows, order=order, search=search):
                submenus: tuple[Menu, ...] = ()
                drawing = partial(_ordered, identity, rows, order)
            case Submenus(entries=entries, search=search):
                present = frozenset(held for held, _ in entries)
                submenus = tuple(
                    Menu(
                        key=f"{self.key}_{group.name.lower()}",
                        title=Some(bpy.path.display_name(group.name.lower())),
                        description=f"{bpy.path.display_name(group.name.lower())}: {self.description}",
                        body=Dropdown(entries=tuple(row for held, row in entries if held is group)),
                        condition=self.condition,
                    )
                    for kind in dict.fromkeys(type(held) for held, _ in entries)
                    for group in kind
                    if group in present
                )
                rows = tuple(Opens(target=Opened(group.key, Called.MENU)) for group in submenus)
                drawing = partial(realized, rows, identity)
            case Pie():
                slots, search, submenus = msgspec.structs.astuple(self.body), False, ()
                rows = tuple(row for slot in slots for row in slot.to_list())
                drawing = partial(_radial_drawing, identity, slots)
        title = self.title.default_with(lambda: bpy.path.display_name(self.key))
        return Ok((*submenus, *acquired(rows), (Band.INTERFACE, partial(_menu, identity, self.key, title, self.description, rows, drawing, search=search, condition=self.condition))))


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
                    (Band.INTERFACE, lambda: store.listing(identity).bind(lambda found: found.map(partial(_preset_menu, identity, self.entries)).default_value(Ok(())))),
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
            return menus.bind(lambda named: realized(self.rows, identity).map(lambda paint: named.map(lambda menu: partial(_collapsed, menu, paint)).default_value(paint)))

        members = {"bl_space_type": self.space, "bl_region_type": self.region}
        return Ok((*self.menus.to_list(), *acquired(self.rows), (Band.INTERFACE, partial(_classed, identity, bpy.types.Header, self.key, self.rows, members, self.condition, drawing))))


# --- [EXTENSIONS]


class ContextMenu(msgspec.Struct, frozen=True, kw_only=True):
    """Button or list row context menu showing content only over the hovered button a struct type, a bound member, or an operator names."""

    menu: type[UI_MT_button_context_menu | UI_MT_list_item_context_menu]
    hovered: type[Struct] | Member | Command | Operation

    def pointing(self, identity: Identity) -> Outcome[_Pointed]:
        """Resolves the hovered button test once, a member's property and an operator's type definition read at realization."""
        match self.hovered:
            case Member() as member:
                return member.resolved().map(lambda held: partial(_hovered, "button_prop", partial(operator.eq, held.descriptor.prop)))
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
        return Ok((*acquired(self.rows), (Band.APPENDS, _extended(self, identity))))


# --- [SHELVES]


class Shelf(msgspec.Struct, frozen=True, kw_only=True):
    """Asset shelf listing the assets an extension marked under the keys `assets` names, or the assets its predicate accepts, context menu rows drawing over the clicked asset.

    `activate` and a `drag` body each register an internal operator over the context asset, described by the body's docstring.
    """

    key: Key
    space: "rna_enums.SpaceTypeItems"
    filters: frozenset[str]
    assets: frozenset[Key] | Callable[[bpy.types.AssetRepresentation], bool]
    activate: Option[_Use] = Nothing
    drag: bool | _Use = True
    active: Option[Callable[[bpy.types.Context], Option[bpy.types.AssetWeakReference]]] = Nothing
    menu: tuple[Row | RowDraw, ...] = ()
    preview: Option[int] = Nothing
    visible: bool = False
    catalogs_in_preferences: bool = True
    condition: Option[Condition] = Nothing

    @property
    def popup(self) -> Placement:
        """Placement buttons and keymaps open the shelf through as a popover."""
        return Placement(Opened(self.key, Called.SHELF))

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Returns the action operators and menu rows, then the shelf class built when the interface band opens.

        Args:
            identity: Extension owning class names and asset marks.

        Returns:
            Acquisitions, the class build faulting per refused name, absent filter, and failed menu row.
        """
        dragging = Nothing if isinstance(self.drag, bool) else Some(self.drag)
        actions = tuple(
            (role, Operator(form=Empty, internal=True, run=wraps(use)(partial(_acted, use)), named=Some(f"{role}_{self.key}")))
            for role, offered in (("activate", self.activate), ("drag", dragging))
            for use in offered.to_list()
        )
        return Ok((*(row for _, row in actions), *acquired(self.menu), (Band.INTERFACE, partial(self._classed, identity, actions))))

    def _classed(self, identity: Identity, actions: tuple[tuple[str, Operator[Empty]], ...]) -> _Built:
        """Builds the shelf class once its operator names, filters, and menu rows resolve."""
        struct = bpy.types.AssetShelf.bl_rna
        named = accumulated(identity.name(bpy.types.Operator, row.key).map(lambda name: (role, name)) for role, row in actions)
        filtered = accumulated(
            Option
            .of_optional(struct.properties.get(key))
            .filter(lambda prop: isinstance(prop, bpy.types.BoolProperty) and prop.is_registered_optional)
            .map(lambda _: key)
            .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(bpy.types.AssetShelf), member=key))
            for key in sorted(self.filters)
        )

        def admitted(_cls: type[bpy.types.AssetShelf], asset: bpy.types.AssetRepresentation) -> bool:
            """Whether the shelf lists the asset: one the extension marked under a named key, or one the predicate accepts."""
            match self.assets:
                case frozenset(keys):
                    return AssetMark.key_of(identity, asset).filter(keys.__contains__).is_some()
                case accepts:
                    return accepts(asset)

        def built(parts: tuple[str, tuple[tuple[str, str], ...], tuple[str, ...], RowDraw]) -> tuple[type, ...]:
            name, operators, filters, draw = parts
            options = {
                *(("STORE_ENABLED_CATALOGS_IN_PREFERENCES",) if self.catalogs_in_preferences else ()),
                *(("DEFAULT_VISIBLE",) if self.visible else ()),
                *(("NO_ASSET_DRAG",) if self.drag is False else ()),
                *(("ACTIVATE_FOR_CONTEXT_MENU",) if self.activate.is_some() and self.menu else ()),
            }
            native = {
                "bl_idname": name,
                "bl_space_type": self.space,
                "bl_options": options,
                "asset_poll": classmethod(admitted),
                **dict.fromkeys(filters, True),
                **polled(self.condition),
                **self.active.map(lambda active: {"get_active_asset": classmethod(lambda _cls: active(bpy.context).to_optional())}).default_value({}),
                **({"draw_context_menu": classmethod(lambda _cls, context, asset, layout: draw(layout, asset, context))} if self.menu else {}),
                **{f"bl_{role}_operator": name for role, name in operators},
                **self.preview.map(lambda size: {"bl_default_preview_size": size}).default_value({}),
            }
            return (type(name, (bpy.types.AssetShelf,), native),)

        return paired(identity.name(bpy.types.AssetShelf, self.key), named, filtered, realized(self.menu, identity)).map(built)


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
        Ok(reached(rows))
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
    return Ok((*acquired((listing, *entries)), (Band.INTERFACE, partial(_preset_menu, identity, entries, listing))))


@contextmanager
def _extended(append: Append, identity: Identity) -> Generator[Outcome[None]]:
    """Adds the draw function to the target for the scope once the content and hovered test realize."""
    target = append.target.menu if isinstance(append.target, ContextMenu) else append.target
    pointed: Outcome[Option[_Pointed]] = append.target.pointing(identity).map(Some) if isinstance(append.target, ContextMenu) else Ok(Nothing)
    drawn = pointed.bind(lambda test: realized(append.rows, identity).map(lambda paint: partial(_shown, append, paint, test)))
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


def _collapsed(menu: str, paint: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the editor menus through the host's collapsible header form, then the rows."""
    if isinstance(kind := getattr(bpy.types, menu, None), type) and issubclass(kind, bpy.types.Menu):
        kind.draw_collapsible(context, layout)
    paint(layout, holder, context)


def _radial_drawing(identity: Identity, slots: tuple[Option[_Slot], ...]) -> Outcome[RowDraw]:
    """Realizes each slot up to the last one holding an entry, a property slot holding a boolean or enum, into the pie draw, a button slot drawing as the pie's own item and a plain draw in a column taking one slot."""

    def slot(row: _Slot) -> Outcome[tuple[RowDraw, bool]]:
        checked = row.target.resolved().bind(lambda held: held.accepts((bpy.types.BoolProperty, bpy.types.EnumProperty))).map(lambda _: None) if isinstance(row, Property) else Ok(None)
        return checked.bind(lambda _: realized((row,), identity)).map(lambda draw: (draw, isinstance(row, Line)))

    filled = reversed(tuple(dropwhile(Option.is_none, reversed(slots))))
    return accumulated(each.map(lambda row: slot(row).map(Some)).default_with(lambda: Ok(Nothing)) for each in filled).map(lambda draws: partial(_radial, draws))


def _radial(draws: tuple[Option[tuple[RowDraw, bool]], ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws each slot into the pie, a separator keeping an empty slot free."""
    pie = layout.menu_pie()
    for drawn in draws:
        drawn.map(lambda slot: slot[0](pie if slot[1] else pie.column(), holder, context)).default_with(pie.separator)


def _shown(append: Append, paint: RowDraw, pointed: Option[_Pointed], surface: _Extended, context: bpy.types.Context) -> None:
    """Paints the content while the condition holds and a context menu's hovered button passes its test."""
    if not (pointed.map(lambda test: test(context)).default_value(True) and admits(append.condition, context)):
        return
    layout: bpy.types.UILayout = surface.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    match surface, append.prepend:
        case bpy.types.Menu(), False:
            layout.separator()
            paint(layout, context, context)
        case bpy.types.Menu(), True:
            paint(layout, context, context)
            layout.separator()
        case _:
            paint(layout, context, context)


def _hovered(member: str, test: Callable[[Struct], bool], context: bpy.types.Context) -> bool:
    """Whether the context member a button context menu publishes (`button_pointer`, `button_prop`, `button_operator`) passes the test, false outside such a menu."""
    return Option.of_optional(getattr(context, member, None)).filter(test).is_some()


def _acted(use: _Use, call: Call[Empty]) -> Outcome[Answer]:
    """Runs a shelf action over the context asset, absent fault without one."""
    return Option.of_optional(call.context.asset).map(lambda asset: use(call, asset)).default_with(lambda: Error(Fault.of(Reason.ABSENT, "bpy.context", member="asset")))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Append", "ContextMenu", "Dropdown", "Header", "Menu", "Pie", "PresetMenu", "Shelf", "Submenus"]
