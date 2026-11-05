"""Collection lists drawn through `UIList` over a host collection."""

import builtins
from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import partial
from itertools import product
from types import MappingProxyType
from typing import Generic, TypeVar

import _bpy_types
from bl_ui.generic_ui_list import draw_ui_list
import bpy
from expression import Option, Some
import msgspec

from blender.bpy.commands.modal import Gesture, Modal, Travel
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import assign
from blender.bpy.data.rna.paths import context_path, definition, rna, Struct, struct_at
from blender.bpy.data.rna.properties import annotated, Callbacks, declared, pointed
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import acquired, Acquisitions, Classes, Declaration, Tier
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.layout.members import At, Member, owner_key, ResolvedMember
from blender.bpy.ui.layout.rows import ConditionalRow, FUNCTIONS, Keywords, Property, realized, RowContent, RowDraw
from blender.bpy.ui.resources.theme import Metrics
from blender.records.faults import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Predicate[O: Struct] = Condition | Callable[[bpy.types.Context, bpy.types.UIList, O], Outcome[bool]]
type Move = Callable[[bpy.types.Context, int, int], Outcome[None]]
_T_co = TypeVar("_T_co", bound=bpy.types.PropertyGroup, covariant=True, default=bpy.types.PropertyGroup)
_O_contra = TypeVar("_O_contra", bound=Struct, contravariant=True, default=Struct)

# --- [MODELS] ---------------------------------------------------------------------------


class ItemList(ConditionalRow, Generic[_T_co, _O_contra], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """`UIList` over a host collection: attached fields and `cells` after each item's name, `side` rows beside it, `absent` rows while no owner holds the collection, `menu` rows in a row's context menu.

    Without `active` the list declares its own selection index. A `bpy.props` collection drags its active item by rows, and takes the host's add, remove, and move buttons unless `move` states the reorder.
    `filters` declares per-list filter properties drawn under the stock filter options, which `where` reads from the list, and `enabled` refuses edits on the rows it fails.
    `keywords` are `template_list` parameters, `sort_lock` held while the list drags and `rows` left to the host beside its entry buttons.
    """

    key: Key
    collection: Member
    active: Option[Member] = Nothing
    attachment: Option[Attachment[_T_co]] = Nothing
    cells: tuple[RowContent, ...] = ()
    side: tuple[RowContent, ...] = ()
    absent: tuple[RowContent, ...] = ()
    menu: tuple[RowContent, ...] = ()
    tip: Option[str] = Nothing
    order: Option[str] = Nothing
    icon: Option[str] = Nothing
    where: Option[Predicate[_O_contra]] = Nothing
    enabled: Option[Predicate[_O_contra]] = Nothing
    move: Option[Move] = Nothing
    filters: Option[builtins.type[bpy.types.PropertyGroup]] = Nothing
    keywords: Keywords = MappingProxyType({})

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Attachments the collection and `active` members read."""
        return tuple(held for member in (self.collection, *self.active.to_list()) for held in member.prerequisites)

    @property
    def children(self) -> tuple[RowContent, ...]:
        """Cell, side, absent, and menu rows."""
        return (*self.cells, *self.side, *self.absent, *self.menu)

    @property
    def _drag_key(self) -> Key:
        """Key of the drag operator."""
        return f"{self.key}_drag"

    # --- [SELECTION]

    def picked(self, identity: Identity) -> Outcome[Callable[[Struct, bpy.types.Context], tuple[Struct, ...]]]:
        """Reads the item the list highlights at the drawing holder, none past either end."""

        def pick(collection: ResolvedMember[bpy.types.CollectionProperty], selection: ResolvedMember[bpy.types.IntProperty], holder: Struct, context: bpy.types.Context) -> tuple[Struct, ...]:
            for source, chosen in product(collection.at(holder, context).to_list(), selection.at(holder, context).to_list()):
                items: bpy.types.bpy_prop_collection[Struct] = getattr(*source)  # ty: ignore[unsound-assignment]
                index: int = getattr(*chosen)  # ty: ignore[unsound-assignment]
                return (items[index],) if 0 <= index < len(items) else ()
            return ()

        return self._resolved(identity).bind(lambda parts: parts[2].resolved(bpy.types.IntProperty).map(lambda selection: partial(pick, parts[0], selection)))

    # --- [ROW]

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the attachment, own selection index, drag operator, list class, and row context menu."""

        def drag_item(_call: Call[Empty]) -> Outcome[Answer]:
            """Drags the active item by list rows, Escape restoring its row."""
            return Ok(Done())

        def built(name: str, collection: ResolvedMember[bpy.types.CollectionProperty], active: Option[ResolvedMember], selection: Member[type[Struct]]) -> Acquisitions:
            pointer = active.filter(lambda bound: not isinstance(bound.descriptor.prop, bpy.types.IntProperty))
            index = pointer.map(
                lambda bound: declared(bpy.props.IntProperty(options={"HIDDEN", "SKIP_SAVE"}), Callbacks(get=partial(_selected, collection, bound), set=partial(_written, collection, bound)))
            ).or_else_with(lambda: Some(bpy.props.IntProperty(options={"HIDDEN"})).filter(lambda _: active.is_none()))
            dragging = _dragging(self.move, collection)
            begin = partial(_began, self.where, name, collection.descriptor.prop.identifier, str(selection.path[-1]))
            modal = Modal(gesture=Gesture.DRAG_Y, begin=begin, preview=partial(_previewed, self.move), restore=partial(_restored, self.move))
            return (
                *self.attachment.to_list(),
                *acquired(self.children),
                *((Tier.ATTACHMENTS, pointed(selection.root, rna(selection.path), prop)) for prop in index.to_list()),
                *((Operator[Empty](form=Empty, run=drag_item, invoke=Some(modal), options=frozenset({"INTERNAL", "UNDO"}), named=Some(self._drag_key)),) if dragging else ()),
                (Tier.INTERFACE, Classes(build=partial(self._classes, identity, name, collection, pointer, dragging=dragging))),
                *(((Tier.APPENDS, self._menu(identity, name)),) if self.menu else ()),
            )

        return self._resolved(identity).map(lambda parts: built(identity.name(bpy.types.UIList, self.key), *parts))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the list with its entry buttons and side column, absent rows while no owner holds the collection."""

        def built(collection: ResolvedMember[bpy.types.CollectionProperty], selection: Member[type[Struct]]) -> Outcome[RowDraw]:
            fixed: Struct = collection.descriptor.prop.fixed_type  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            tip = _member(self.tip, type(fixed))
            keywords = {**self.keywords, **({"sort_lock": True} if _dragging(self.move, collection) else {})}
            tipped = tip.map(lambda held: {**keywords, **held.map(lambda bound: {"item_dyntip_propname": bound.descriptor.prop.identifier}).default_value({})})
            return paired(selection.resolved(bpy.types.IntProperty), tipped, realized(self.side, identity), realized(self.absent, identity)).map(
                lambda parts: partial(self.shown, collection, parts[0], identity.name(bpy.types.UIList, self.key), *parts[1:])
            )

        return self._resolved(identity).bind(lambda parts: built(parts[0], parts[2]))

    def shown(
        self,
        collection: ResolvedMember[bpy.types.CollectionProperty],
        selection: ResolvedMember[bpy.types.IntProperty],
        name: str,
        keywords: Keywords,
        side: RowDraw,
        absent: RowDraw,
        layout: bpy.types.UILayout,
        holder: Struct,
        context: bpy.types.Context,
    ) -> None:
        """Draws the list with its side rows and, for a `bpy.props` collection without a stated move, the host's entry buttons, or the absent rows."""
        if (found := collection.at(holder, context)).is_none():
            absent(layout, holder, context)
        for listing, picking in product(found.to_list(), selection.at(holder, context).to_list()):
            match tuple(path for owner, key in (listing, picking) if isinstance(owner, bpy.types.bpy_struct) for path in context_path(owner, key).to_list()):
                case (items, index) if collection.descriptor.prop.is_runtime and self.move.is_none():
                    column: bpy.types.UILayout = draw_ui_list(layout, context, name, unique_id=owner_key(listing[0]), list_path=items, active_index_path=index, **keywords)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                case _:
                    row = layout.row()
                    getattr(row, FUNCTIONS["template_list"].identifier)(name, owner_key(listing[0]), *listing, *picking, **keywords)
                    column = row.column(align=True)
            side(column, holder, context)

    # --- [RESOLUTION]

    def _resolved(self, identity: Identity) -> Outcome[tuple[ResolvedMember[bpy.types.CollectionProperty], Option[ResolvedMember], Member[type[Struct]]]]:
        """Resolves the collection, `active` as an integer index or item pointer, and the highlighted index member, the list's own index while `active` is no integer."""

        def resolved(
            listing: Member[type[Struct]], pointing: Option[Member[type[Struct]]]
        ) -> Outcome[tuple[ResolvedMember[bpy.types.CollectionProperty], Option[ResolvedMember], Member[type[Struct]]]]:
            shared = isinstance(listing.root, type(bpy.types.ID)) and pointing.map(lambda member: member.root is listing.root and member.reach == listing.reach).default_value(value=True)

            active: Outcome[Option[ResolvedMember]] = pointing.map(lambda member: member.resolved().map(Some)).default_value(Ok(Nothing))
            return paired(listing.resolved(bpy.types.CollectionProperty), active).map(
                lambda found: (
                    found[0],
                    active := found[1],
                    active
                    .filter(lambda held: isinstance(held.descriptor.prop, bpy.types.IntProperty))
                    .bind(lambda _: pointing)
                    .default_with(
                        lambda: Member[type[Struct]](root=listing.root if shared else bpy.types.WindowManager, path=(identity.identifier(self.key),), reach=listing.reach if shared else Nothing)
                    ),
                )
            )

        pointing: Outcome[Option[Member[type[Struct]]]] = self.active.map(lambda member: member.located(identity).map(Some)).default_value(Ok(Nothing))
        return paired(self.collection.located(identity), pointing).bind(lambda pair: resolved(*pair))

    # --- [REGISTRATION]

    def _classes(
        self, identity: Identity, name: str, collection: ResolvedMember[bpy.types.CollectionProperty], pointer: Option[ResolvedMember], *, dragging: bool
    ) -> Outcome[tuple[builtins.type[bpy.types.UIList], ...]]:
        """Builds the list class once its tier opens and its fields, cells, item members, and drag operator resolve."""
        fixed: Struct = collection.descriptor.prop.fixed_type  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        kind = type(fixed)
        label = Option.of_optional(definition(fixed).name_property).map(lambda named: named.identifier)

        def fielded(attachment: Attachment[_T_co]) -> Outcome[tuple[RowDraw, ...]]:
            return attachment.bound(identity).bind(
                lambda attached: accumulated(
                    realized((Property(target=Member(root=attached.form, path=(member,)), keywords={"text": ""}),), identity).map(lambda draw: partial(_grouped, attached, draw))
                    for member in annotated(attached.form)
                )
            )

        cells = paired(self.attachment.map(fielded).default_value(Ok(())), realized(self.cells, identity))
        icon = _member(self.icon, kind)
        drag = Some(identity.name(bpy.types.Operator, self._drag_key)) if dragging else Nothing

        def classed(parts: tuple[tuple[tuple[RowDraw, ...], RowDraw], Option[ResolvedMember], Option[ResolvedMember]]) -> builtins.type[bpy.types.UIList]:
            (fields, drawn), icons, order = parts
            shown = (*fields, drawn)

            def draw_item(
                ui: bpy.types.UIList,
                context: bpy.types.Context,
                layout: bpy.types.UILayout,
                data: Struct,
                item: _O_contra,
                icon: int,
                active_data: Struct,
                active_property: str,
                index: int = 0,
                _flt_flag: int = 0,
            ) -> None:
                editable = self.enabled.map(lambda predicate: _held(predicate, context, ui, item)).default_value(Ok(value=True))
                for enabled in identity.reports.settled(editable).to_list():
                    row = layout.row(align=True)
                    row.enabled = enabled
                    value = icons.bind(lambda bound: bound.at(item, context).starmap(getattr)).default_value(icon)
                    for named in label.to_list():
                        row.prop(item, named, text="", emboss=False, icon_value=value)
                    if label.is_none():
                        row.label(text="", icon_value=value)
                    for cell in shown if ui.layout_type == "DEFAULT" else ():
                        cell(row, item, context)
                    for grip in drag.filter(
                        lambda _: pointer.map(lambda held: held.at(active_data, context).map(lambda at: getattr(*at) == item).default_value(value=False)).default_with(
                            lambda: getattr(active_data, active_property) == index
                        )
                    ).to_list():
                        row.context_pointer_set(name, data)
                        row.context_pointer_set(_active(name), active_data)
                        row.operator(grip, text="", icon="GRIP", emboss=False)

            def filter_items(ui: bpy.types.UIList, context: bpy.types.Context, data: Struct, propname: str) -> tuple[list[int], list[int]]:
                items: bpy.types.bpy_prop_collection[_O_contra] = getattr(data, propname)  # ty: ignore[unsound-assignment]
                flag, held = ui.bitflag_filter_item, identity.reports.reported(_held)
                named = label.map(lambda member: bpy.types.UI_UL_list.filter_items_by_name(ui.filter_name, flag, items, member, reverse=ui.use_filter_invert)).default_value(list[int]()) or [  # type: ignore[arg-type]
                    flag
                ] * len(items)
                flags = [kept if all(held(where, context, ui, item) == Some(True) for where in self.where.to_list()) else ui.bitflag_item_never_show for kept, item in zip(named, items)]
                match order, label:
                    case _ if ui.use_filter_sort_lock:
                        return flags, []
                    case _, Option(tag="some", some=member) if ui.use_filter_sort_alpha:
                        return flags, bpy.types.UI_UL_list.sort_items_by_name(items, member)  # type: ignore[arg-type]  # ty: ignore[invalid-return-type]
                    case Option(tag="some", some=bound), _:
                        ranked = partial(_ranked, bound)
                        return flags, bpy.types.UI_UL_list.sort_items_helper([(position, ranked(item)) for position, item in enumerate(items)], lambda pair: pair[1])  # type: ignore[misc]  # ty: ignore[invalid-return-type]
                    case _:
                        return flags, []

            def draw_filter(ui: bpy.types.UIList, _context: bpy.types.Context, layout: bpy.types.UILayout) -> None:
                line = layout.row()
                names = line.row(align=True)
                names.prop(ui, "filter_name", text="", icon="VIEWZOOM", placeholder="Search")
                names.prop(ui, "use_filter_invert", toggle=True, icon_only=True, icon="ARROW_LEFTRIGHT")
                if not ui.use_filter_sort_lock:
                    sorts = line.row(align=True)
                    sorts.prop(ui, "use_filter_sort_alpha", toggle=True, icon_only=True)
                    sorts.prop(ui, "use_filter_sort_reverse", toggle=True, icon_only=True, icon="SORT_DESC" if ui.use_filter_sort_reverse else "SORT_ASC")
                for form in self.filters.to_list():
                    for member in annotated(form):
                        layout.prop(ui, member)

            filtering = {"filter_items": filter_items} if self.where.is_some() or order.is_some() else {}
            declared = self.filters.map(lambda form: {"__annotations__": annotated(form), "draw_filter": draw_filter}).default_value({})
            return type(name, (bpy.types.UIList,), {"bl_idname": name, "draw_item": draw_item, **filtering, **declared})

        return paired(cells, icon, _member(self.order, kind)).map(lambda parts: (classed(parts),))

    @contextmanager
    def _menu(self, identity: Identity, name: str) -> Generator[Outcome[None]]:
        """Appends the menu rows to the list row context menu, drawn while the clicked list is this one."""
        menu = bpy.types.UI_MT_list_item_context_menu
        with ExitStack() as stack:

            def added(draw: RowDraw) -> None:
                shown: Callable[[_bpy_types.Context], None] = partial(_menued, name, draw)  # ty: ignore[invalid-assignment]
                menu.append(shown)
                stack.callback(menu.remove, shown)

            yield realized(self.menu, identity).map(added)


class _Drag(msgspec.Struct, frozen=True, kw_only=True):
    """Dragged row at press with the signed pixels per row, and the collection and selection it moves."""

    origin: int
    pitch: float
    items: At
    selection: At


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RESOLUTION]


def _member(found: Option[str], item: type[Struct]) -> Outcome[Option[ResolvedMember]]:
    """Resolves an optional item member."""
    return found.map(lambda path: Member(root=item, path=(path,)).resolved().map(Some)).default_value(Ok(Nothing))


def _dragging(move: Option[Move], collection: ResolvedMember[bpy.types.CollectionProperty]) -> bool:
    """Whether the list reorders by drag: a stated move or a `bpy.props` collection."""
    return move.is_some() or collection.descriptor.prop.is_runtime


# --- [SELECTION]


def _selected(collection: ResolvedMember[bpy.types.CollectionProperty], active: ResolvedMember, owner: Struct) -> int:
    """Index of the item the active pointer holds, -1 for none."""
    items = collection.at(owner, bpy.context).starmap(getattr)
    pointer = active.at(owner, bpy.context).starmap(getattr)
    return next((index for members in items.to_list() for held in pointer.to_list() for index, item in enumerate(members) if item == held), -1)


def _written(collection: ResolvedMember[bpy.types.CollectionProperty], active: ResolvedMember, owner: Struct, index: int) -> None:
    """Points the active pointer at the item at index, clearing it for a negative index."""
    for source in collection.at(owner, bpy.context).to_list():
        members = getattr(*source)
        for holder, key in active.at(owner, bpy.context).to_list():
            setattr(holder, key, members[index] if index >= 0 else None)


# --- [CLASS]


def _held[O: Struct](predicate: Predicate[O], context: bpy.types.Context, ui: bpy.types.UIList, item: O) -> Outcome[bool]:
    """Answers a predicate over one item."""
    match predicate:
        case Condition():
            return Ok(predicate.holds(context, Some(item)))
        case _:
            return predicate(context, ui, item)


def _ranked(bound: ResolvedMember, item: Struct) -> tuple[bool, object]:
    """Sort key of an item's member value, absent values last and enum identifiers at their item position."""
    prop = bound.descriptor.prop
    value = struct_at(item, bound.descriptor.owner).map(bound.descriptor.read)
    position = value.map(lambda held: prop.enum_items.find(held) if isinstance(prop, bpy.types.EnumProperty) and isinstance(held, str) else held)
    return position.is_none(), position.default_value(0)


def _menued(name: str, draw: RowDraw, menu: bpy.types.Menu, context: bpy.types.Context) -> None:
    """Draws the menu rows while the clicked list is the named list."""
    layout: bpy.types.UILayout = menu.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    if Option.of_optional(context.ui_list).filter(lambda ui: ui.bl_idname == name).is_some():
        draw(layout, context, context)


def _grouped[T: bpy.types.PropertyGroup](attached: Attached[T], draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws over the item's attached group."""
    draw(layout, getattr(holder, attached.name), context)


# --- [DRAG]


def _active(name: str, /) -> str:
    """Context member the grip row publishes the selection owner under."""
    return f"{name}_active"


def _began[O: Struct](where: Option[Predicate[O]], name: str, items: str, index: str, context: bpy.types.Context, _travel: Travel) -> Outcome[_Drag]:
    """Reads the drag state from the owners the grip row publishes, refusing a name-filtered list, a hidden item, or a selection outside the collection."""

    def pointer(member: str) -> Outcome[Struct]:
        owner: Struct | None = getattr(context, member, None)  # ty: ignore[unsound-assignment]
        return Option.of_optional(owner).to_result_with(partial(Fault.of, Reason.ABSENT, "bpy.context", member=member))

    def state(ui: bpy.types.UIList, data: Struct, chosen: Struct) -> Outcome[_Drag]:
        origin, members = getattr(chosen, index), getattr(data, items)
        pitch = float(Metrics.read().unit) * (1.0 if ui.use_filter_sort_reverse else -1.0)
        visible = accumulated(_held(predicate, context, ui, item) for predicate in where.to_list() for item in members).filter_with(
            all, lambda _: Fault.of(Reason.REQUIRED, f"{ui.list_id} without hidden items")
        )
        selected = Ok(origin).filter_with(lambda held: 0 <= held < len(members), lambda _: Fault.of(Reason.ABSENT, items, member=f"{index} {origin}"))
        return paired(visible, selected).map(lambda _: _Drag(origin=origin, pitch=pitch, items=(data, items), selection=(chosen, index)))

    return paired(
        Option
        .of_optional(context.ui_list)
        .to_result_with(partial(Fault.of, Reason.ABSENT, "bpy.context", member="ui_list"))
        .filter_with(lambda held: not held.filter_name, lambda held: Fault.of(Reason.REQUIRED, f"{held.list_id} without name filter")),
        pointer(name),
        pointer(_active(name)),
    ).bind(lambda parts: state(*parts))


def _previewed(move: Option[Move], context: bpy.types.Context, state: _Drag, travel: Travel) -> Outcome[Option[str]]:
    """Places the dragged item at the row the travel reaches."""
    return _placed(move, context, state, state.origin + round(travel.dy / state.pitch)).map(lambda _: Nothing)


def _restored(move: Option[Move], context: bpy.types.Context, state: _Drag) -> Outcome[None]:
    """Returns the dragged item to its row at press."""
    return _placed(move, context, state, state.origin)


def _placed(move: Option[Move], context: bpy.types.Context, state: _Drag, target: int) -> Outcome[None]:
    """Moves the selected item to the clamped target row, the stated move or the host collection's own, and points the selection at it."""
    try:
        members, current = getattr(*state.items), getattr(*state.selection)
        row = min(max(target, 0), len(members) - 1)
    except ReferenceError as error:
        return Error(Fault.of(Reason.REFUSED, state.items[1], error))
    if row == current:
        return Ok(None)

    def native() -> Outcome[None]:
        try:
            members.move(current, row)
        except (IndexError, TypeError) as error:
            return Error(Fault.of(Reason.REFUSED, "bpy_prop_collection.move", error))
        return Ok(None)

    return (
        move
        .map(lambda reorder: reorder(context, current, row))
        .default_with(native)
        .bind(lambda _: assign(*state.selection, row).map_error(partial(Fault.of, Reason.REFUSED, state.selection[1])))
        .map(lambda _: None)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ItemList", "Move", "Predicate"]
