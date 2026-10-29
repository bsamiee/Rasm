"""Collection lists with a computed selection, filters, sorting, entry buttons, row context menus, and drag reorder."""

import builtins
from collections.abc import Callable, Generator, Sequence
from contextlib import contextmanager, ExitStack
from functools import partial
from itertools import product
from types import MappingProxyType
from typing import Final, Protocol

import _bpy_types
from bl_ui.generic_ui_list import draw_ui_list
import bpy
from expression import Error, Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch

from blender.bpy.commands.modal import Gesture, Modal, Travel
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Scalar
from blender.bpy.data.rna.mirrors import annotated, pointed
from blender.bpy.data.rna.paths import definition, Native, rna, Struct, struct_at
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.layout.members import At, Member, ResolvedMember
from blender.bpy.ui.layout.rows import accepted, acquired, alerted, FUNCTIONS, Keywords, Line, placed, Property, realized, RowContent, RowDraw
from blender.bpy.ui.resources.theme import Metrics
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason

# --- [TYPES] ----------------------------------------------------------------------------

type Predicate[O: Struct] = Condition | Callable[[bpy.types.Context, bpy.types.UIList, O], Outcome[bool]]
type Move = Callable[[bpy.types.Context, int, int], Outcome[None]]
type _Drag = tuple[int, float, At, At]


class _DrawUiList(Protocol):
    """Host list drawing with entry buttons, taking the drawing context and passing `template_list` keywords through."""

    def __call__(self, layout: bpy.types.UILayout, context: bpy.types.Context, class_name: str, /, *, unique_id: str, list_path: str, active_index_path: str, **kwargs: Scalar) -> bpy.types.UILayout:
        """Draws the list and its entry buttons, returning the side column."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_ORDERED: Final = (bpy.types.BoolProperty, bpy.types.IntProperty, bpy.types.FloatProperty, bpy.types.StringProperty, bpy.types.EnumProperty)
_DRAW_UI_LIST: Final[_DrawUiList] = draw_ui_list  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

# --- [MODELS] ---------------------------------------------------------------------------


class ItemList[T: bpy.types.PropertyGroup = bpy.types.PropertyGroup, O: Struct = Struct](Line, frozen=True, kw_only=True):
    """`UIList` over a host collection: attached fields and `cells` after each item's name, `side` rows beside it, `absent` rows while no owner holds the collection, `menu` rows in a row's context menu.

    Without `active` the list declares its own selection index. A `bpy.props` collection drags its active item by rows, and takes the host's add, remove, and move buttons unless `move` states the reorder.
    `filters` declares per-list filter properties drawn under the stock filter options, which `where` reads from the list, and `enabled` refuses edits on the rows it fails.
    `keywords` are `template_list` parameters, `sort_lock` held while the list drags and `rows` left to the host beside its entry buttons.
    """

    key: Key
    collection: Member
    item: builtins.type[O]
    active: Option[Member] = Nothing
    attachment: Option[Attachment[T]] = Nothing
    cells: tuple[RowContent, ...] = ()
    side: tuple[RowContent, ...] = ()
    absent: tuple[RowContent, ...] = ()
    menu: tuple[RowContent, ...] = ()
    tip: Option[str] = Nothing
    order: Option[str] = Nothing
    icon: Option[str] = Nothing
    where: Option[Predicate[O]] = Nothing
    enabled: Option[Predicate[O]] = Nothing
    move: Option[Move] = Nothing
    filters: Option[builtins.type[bpy.types.PropertyGroup]] = Nothing
    keywords: Keywords = MappingProxyType({})

    @property
    def children(self) -> tuple[RowContent, ...]:
        """Cell, side, absent, and menu rows."""
        return (*self.cells, *self.side, *self.absent, *self.menu)

    # --- [SELECTION]

    def picked(self, identity: Identity) -> Outcome[Callable[[Struct, bpy.types.Context], Option[Struct]]]:
        """Reads the item the list highlights at the drawing holder."""
        return self._resolved(identity).bind(lambda parts: parts[2].resolved(bpy.types.IntProperty).map(lambda selection: partial(_picked, parts[0], selection)))

    # --- [ROW]

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the attachment, the selection index the list declares, the drag operator, the list class, and the row context menu, refusing `where` and `order` on a compact list, which never filters."""
        compact = Ok(self).filter_with(
            lambda held: held.keywords.get("type") != "COMPACT" or (held.where.is_none() and held.order.is_none()),
            lambda held: Fault.of(RecordReason.OUTSIDE, held.key, "COMPACT", member="a filtering layout"),
        )

        def drag_item(_call: Call[Empty]) -> Outcome[Answer]:
            """Drags the active item by list rows, Escape restoring its row."""
            return Ok(Done())

        def built(name: str, collection: ResolvedMember[bpy.types.CollectionProperty], active: Option[ResolvedMember], selection: Member) -> Acquisitions:
            pointer = active.filter(lambda bound: isinstance(bound.descriptor.prop, bpy.types.PointerProperty))
            index = pointer.map(lambda bound: bpy.props.IntProperty(get=partial(_selected, collection, bound), set=partial(_written, collection, bound), options={"HIDDEN", "SKIP_SAVE"})).or_else_with(
                lambda: Some(bpy.props.IntProperty(options={"HIDDEN"})).filter(lambda _: active.is_none())
            )
            dragging = _dragging(self.move, collection)
            begin = partial(_began, self.where, name, collection.descriptor.prop.identifier, str(selection.path[-1]))
            modal = Modal(gesture=Gesture.DRAG_Y, begin=begin, preview=partial(_previewed, self.move), restore=partial(_restored, self.move))
            return (
                *self.attachment.to_list(),
                *acquired(self.children),
                *((Band.ATTACHMENTS, pointed(selection.root, rna(selection.path), prop)) for prop in index.to_list()),
                *((Operator[Empty, _Drag](form=Empty, run=drag_item, invoke=Some(modal), internal=True, named=Some(f"{self.key}_drag")),) if dragging else ()),
                (Band.INTERFACE, self._registered(identity, name, collection, pointer, dragging=dragging)),
                *(((Band.APPENDS, self._menu(identity, name)),) if self.menu else ()),
            )

        return paired(identity.name(bpy.types.UIList, self.key), self._resolved(identity), compact).map(lambda parts: built(parts[0], *parts[1]))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the list with its entry buttons and side column, absent rows while no owner holds the collection."""

        def built(collection: ResolvedMember[bpy.types.CollectionProperty], selection: Member) -> Outcome[RowDraw]:
            fixed: Struct = collection.descriptor.prop.fixed_type  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            entries = collection.descriptor.prop.is_runtime and self.move.is_none()
            tip = _member(self.tip, type(fixed), (bpy.types.StringProperty,)).filter_with(
                lambda held: held.map(lambda bound: not bound.descriptor.owner).default_value(True), lambda _: Fault.of(RecordReason.OUTSIDE, self.key, "nested member", member="item string property")
            )
            keywords = accepted(FUNCTIONS["template_list"], {**self.keywords, **({"sort_lock": True} if _dragging(self.move, collection) else {})}).filter_with(
                lambda held: not entries or "rows" not in held, lambda _: Fault.of(RecordReason.OUTSIDE, self.key, "rows", member="the row count the host gives a list beside its entry buttons")
            )
            tipped = paired(keywords, tip).map(lambda pair: {**pair[0], **pair[1].map(lambda held: {"item_dyntip_propname": held.descriptor.prop.identifier}).default_value({})})
            return paired(identity.name(bpy.types.UIList, self.key), selection.resolved(bpy.types.IntProperty), tipped, realized(self.side, identity), realized(self.absent, identity)).map(
                lambda parts: partial(_listed, collection, parts[1], partial(_framed, parts[0], parts[2], entries=entries), parts[3], parts[4])
            )

        return self._resolved(identity).bind(lambda parts: built(parts[0], parts[2]))

    # --- [RESOLUTION]

    def _resolved(self, identity: Identity) -> Outcome[tuple[ResolvedMember[bpy.types.CollectionProperty], Option[ResolvedMember], Member]]:
        """Resolves the collection, `active` as an integer index or item pointer, and the highlighted index: `active` when an integer, else the list's own index on a custom-property collection root sharing the pointer's root, or on the window manager."""
        shared = isinstance(self.collection.root, type(bpy.types.ID)) and self.active.map(lambda member: member.root is self.collection.root and member.reach == self.collection.reach).default_value(
            True
        )

        def activated(collection: ResolvedMember[bpy.types.CollectionProperty]) -> Outcome[Option[ResolvedMember]]:
            item = collection.descriptor.prop.fixed_type

            def checked(held: ResolvedMember) -> Outcome[ResolvedMember]:
                match held.descriptor.prop:
                    case bpy.types.IntProperty(is_array=False):
                        return Ok(held)
                    case bpy.types.PointerProperty(fixed_type=target) if isinstance(item, type(target)):
                        return Ok(held)
                    case prop:
                        return Error(Fault.of(RecordReason.OUTSIDE, rna(held.descriptor.path), type(prop).__name__, member="scalar integer or item pointer"))

            return self.active.map(lambda member: member.resolved().bind(checked).map(Some)).default_value(Ok(Nothing))

        def selected(
            collection: ResolvedMember[bpy.types.CollectionProperty], active: Option[ResolvedMember]
        ) -> Outcome[tuple[ResolvedMember[bpy.types.CollectionProperty], Option[ResolvedMember], Member]]:
            indexed = active.filter(lambda held: isinstance(held.descriptor.prop, bpy.types.IntProperty)).bind(lambda _: self.active).map(Ok)
            declared = indexed.default_with(
                lambda: identity.identifier(self.key).map(
                    lambda name: Member(root=self.collection.root, path=(name,), reach=self.collection.reach) if shared else Member(root=bpy.types.WindowManager, path=(name,))
                )
            )
            return declared.map(lambda selection: (collection, active, selection))

        collected = self.collection.resolved(bpy.types.CollectionProperty).filter_with(
            lambda bound: isinstance(bound.descriptor.prop.fixed_type, self.item),
            lambda bound: Fault.of(RecordReason.OUTSIDE, rna(bound.descriptor.path), type(bound.descriptor.prop.fixed_type).__name__, member=self.item.__name__),
        )
        return collected.bind(lambda collection: activated(collection).bind(lambda active: selected(collection, active)))

    # --- [REGISTRATION]

    @contextmanager
    def _registered(self, identity: Identity, name: str, collection: ResolvedMember[bpy.types.CollectionProperty], pointer: Option[ResolvedMember], *, dragging: bool) -> Generator[Outcome[None]]:
        """Registers the list class once the attachment fields, cells, item members, and drag operator resolve, filtering while `where` or `order` is set and drawing the filter properties `filters` declares."""
        fixed: Struct = collection.descriptor.prop.fixed_type  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        kind = type(fixed)
        label = Option.of_optional(definition(fixed).name_property).map(lambda named: named.identifier)

        def fielded(attachment: Attachment[T]) -> Outcome[tuple[RowDraw, ...]]:
            outside = Fault.of(RecordReason.OUTSIDE, rna(collection.descriptor.path), kind.__name__, member=attachment.owner.__name__)
            return (attachment.bound(identity) if isinstance(fixed, attachment.owner) else Error(outside)).bind(
                lambda attached: accumulated(
                    realized((Property(target=Member(root=attached.form, path=(member,)), keywords={"text": ""}),), identity).map(lambda draw: partial(_grouped, attached, draw))
                    for member in annotated(attached.form)
                )
            )

        cells = paired(self.attachment.map(fielded).default_value(Ok(())), realized(self.cells, identity))
        icon = _member(self.icon, kind, (bpy.types.IntProperty,)).filter_with(
            lambda held: held.map(lambda bound: not (isinstance(bound.descriptor.prop, bpy.types.IntProperty) and bound.descriptor.prop.is_array) or bound.descriptor.index.is_some()).default_value(
                True
            ),
            lambda _: Fault.of(RecordReason.OUTSIDE, self.key, "integer array", member="integer component"),
        )
        drag = identity.name(bpy.types.Operator, f"{self.key}_drag").map(Some) if dragging else Ok(Nothing)

        def classed(parts: tuple[tuple[tuple[RowDraw, ...], RowDraw], Option[ResolvedMember], Option[ResolvedMember], Option[str]]) -> builtins.type[bpy.types.UIList]:
            (fields, drawn), glyphs, order, operator = parts
            shown = (*fields, drawn)

            def draw_item(
                ui: bpy.types.UIList,
                context: bpy.types.Context,
                layout: bpy.types.UILayout,
                data: Struct,
                item: O,
                glyph: int,
                active_data: Struct,
                active_property: str,
                index: int = 0,
                _flt_flag: int = 0,
            ) -> None:
                editable = self.enabled.map(lambda predicate: _held(predicate, context, ui, item)).default_value(Ok(True))
                alerted(layout, editable)
                for enabled in editable.to_option().to_list():
                    row = layout.row(align=True)
                    row.enabled = enabled
                    value = (
                        glyphs
                        .bind(lambda bound: struct_at(item, bound.descriptor.owner).map(bound.descriptor.read))
                        .bind(lambda held: Option.of_optional(try_downcast(int, held)))
                        .default_value(glyph)
                    )
                    for named in label.to_list():
                        row.prop(item, named, text="", emboss=False, icon_value=value)
                    if label.is_none():
                        row.label(text="", icon_value=value)
                    for cell in shown if ui.layout_type == "DEFAULT" else ():
                        cell(row, item, context)
                    for grip in operator.filter(
                        lambda _: pointer.map(lambda held: held.at(active_data, context).map(lambda at: getattr(*at) == item).default_value(False)).default_with(
                            lambda: getattr(active_data, active_property) == index
                        )
                    ).to_list():
                        row.context_pointer_set(name, data)
                        row.context_pointer_set(f"{name}_active", active_data)
                        row.operator(grip, text="", icon="GRIP", emboss=False)

            def filter_items(ui: bpy.types.UIList, context: bpy.types.Context, data: Struct, propname: str) -> tuple[list[int], list[int]]:
                items: Sequence[O] = getattr(data, propname)[:]
                flag, held = ui.bitflag_filter_item, identity.reports.reported(_held)
                named = label.map(lambda member: bpy.types.UI_UL_list.filter_items_by_name(ui.filter_name, flag, items, member, reverse=ui.use_filter_invert)).default_value([]) or [flag] * len(items)
                flags = [kept if all(held(where, context, ui, item).default_value(True) for where in self.where.to_list()) else ui.bitflag_item_never_show for kept, item in zip(named, items)]
                match order, label:
                    case _ if ui.use_filter_sort_lock:
                        return flags, []
                    case _, Option(tag="some", some=member) if ui.use_filter_sort_alpha:
                        return flags, bpy.types.UI_UL_list.sort_items_by_name(items, member)  # ty: ignore[invalid-return-type]
                    case Option(tag="some", some=bound), _:
                        ranked = partial(_ranked, bound)
                        return flags, bpy.types.UI_UL_list.sort_items_helper([(position, ranked(item)) for position, item in enumerate(items)], lambda pair: pair[1])  # type: ignore[misc]  # ty: ignore[invalid-return-type]
                    case _:
                        return flags, []

            def draw_filter(ui: bpy.types.UIList, _context: bpy.types.Context, layout: bpy.types.UILayout) -> None:
                line = layout.row(align=True)
                line.prop(ui, "filter_name", text="")
                line.prop(ui, "use_filter_invert", text="", icon="ARROW_LEFTRIGHT")
                if not ui.use_filter_sort_lock:
                    line.separator()
                    line.prop(ui, "use_filter_sort_alpha", text="", icon="SORTALPHA")
                    line.prop(ui, "use_filter_sort_reverse", text="", icon="SORT_DESC" if ui.use_filter_sort_reverse else "SORT_ASC")
                for form in self.filters.to_list():
                    for member in annotated(form):
                        layout.prop(ui, member)

            filtering = {"filter_items": filter_items} if self.where.is_some() or order.is_some() else {}
            declared = self.filters.map(lambda form: {"__annotations__": annotated(form), "draw_filter": draw_filter}).default_value({})
            return type(name, (bpy.types.UIList,), {"bl_idname": name, "draw_item": draw_item, **filtering, **declared})

        with ExitStack() as stack:
            yield paired(cells, icon, _member(self.order, kind, _ORDERED), drag).map(classed).bind(lambda cls: entered(stack, (cls,)))

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


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RESOLUTION]


def context_path(owner: Struct, name: str = "") -> Option[str]:
    """Context data path of an owner, or of its named member, through `blend_data` for ID data and the context for preferences, `Nothing` for data neither reaches."""
    return (
        catch(owner.path_from_module, exception=ValueError)(name)
        .to_option()
        .bind(
            lambda path: (
                Some(f"blend_data.{path.removeprefix('bpy.data.')}") if path.startswith("bpy.data.") else Some(path.removeprefix("bpy.context.")) if path.startswith("bpy.context.") else Nothing
            )
        )
    )


def _member(found: Option[str], item: type[Struct], kinds: tuple[type[Struct], ...]) -> Outcome[Option[ResolvedMember]]:
    """Resolves an optional item member within the accepted kinds."""
    return found.map(lambda path: Member(root=item, path=(path,)).resolved().bind(lambda held: held.accepts(kinds)).map(Some)).default_value(Ok(Nothing))


def _dragging(move: Option[Move], collection: ResolvedMember[bpy.types.CollectionProperty]) -> bool:
    """Whether the list reorders by drag: a stated move or a `bpy.props` collection."""
    return move.is_some() or collection.descriptor.prop.is_runtime


def _keyed(struct: Native) -> str:
    """List identifier per owner: ID `session_uid`, struct name, or nameless struct address."""
    if isinstance(struct, bpy.types.bpy_prop_collection):
        return _keyed(struct.data)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    match struct, definition(struct).name_property:
        case bpy.types.ID(), _:
            return str(struct.session_uid)
        case _, bpy.types.StringProperty(identifier=name):
            return str(getattr(struct, name))
        case _:
            return str(struct.as_pointer())


# --- [SELECTION]


def _picked(collection: ResolvedMember[bpy.types.CollectionProperty], selection: ResolvedMember[bpy.types.IntProperty], holder: Struct, context: bpy.types.Context) -> Option[Struct]:
    """Item at the selection index, `Nothing` past either end."""
    return (
        collection
        .at(holder, context)
        .bind(lambda found: selection.at(holder, context).map(lambda pick: (getattr(*found), getattr(*pick))))
        .bind(lambda pair: Some(pair[0]).filter(lambda items: 0 <= pair[1] < len(items)).map(lambda items: items[pair[1]]))
    )


def _selected(collection: ResolvedMember[bpy.types.CollectionProperty], active: ResolvedMember, owner: Struct) -> int:
    """Index of the item the active pointer holds, -1 for none."""
    items = collection.at(owner, bpy.context).starmap(getattr)
    pointer = active.at(owner, bpy.context).starmap(getattr)
    return Block.of_seq(index for members in items.to_list() for index, item in enumerate(members) for held in pointer.to_list() if item == held).try_head().default_value(-1)


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


# --- [DRAWING]


def _framed(name: str, keywords: Keywords, layout: bpy.types.UILayout, context: bpy.types.Context, listing: At, picking: At, *, entries: bool) -> bpy.types.UILayout:
    """Draws the host list at the located owners, with entry buttons while `entries` holds and context paths reach both owners, returning its side column."""

    def located(at: At) -> Option[str]:
        owner, key = at
        return Option.of_optional(try_downcast(bpy.types.bpy_struct, owner)).bind(lambda held: context_path(held, key))

    match Block.of_seq(product(located(listing).to_list(), located(picking).to_list())).filter(lambda _: entries).try_head():
        case Option(tag="some", some=(items, index)):
            return _DRAW_UI_LIST(layout, context, name, unique_id=_keyed(listing[0]), list_path=items, active_index_path=index, **keywords)
        case _:
            row = layout.row()
            placed(row, FUNCTIONS["template_list"], name, _keyed(listing[0]), *listing, *picking, **keywords)
            return row.column(align=True)


def _listed(
    collection: ResolvedMember[bpy.types.CollectionProperty],
    selection: ResolvedMember[bpy.types.IntProperty],
    framed: Callable[[bpy.types.UILayout, bpy.types.Context, At, At], bpy.types.UILayout],
    side: RowDraw,
    absent: RowDraw,
    layout: bpy.types.UILayout,
    holder: Struct,
    context: bpy.types.Context,
) -> None:
    """Draws the list at the located collection and selection with the side rows in its side column, absent rows while no owner holds the collection."""
    if (found := collection.at(holder, context)).is_none():
        absent(layout, holder, context)
    for listing, picking in product(found.to_list(), selection.at(holder, context).to_list()):
        side(framed(layout, context, listing, picking), holder, context)


# --- [DRAG]


def _began[O: Struct](where: Option[Predicate[O]], name: str, items: str, index: str, context: bpy.types.Context, _travel: Travel) -> Outcome[_Drag]:
    """Reads the owners the grip row publishes, refusing while the list filters by name, `where` hides an item, or the selection sits outside the collection, rows stepping up the list unless it shows reversed."""

    def pointer(member: str) -> Outcome[Struct]:
        return Option.of_optional(getattr(context, member, None)).to_result_with(partial(Fault.of, BlenderReason.ABSENT, "bpy.context", member=member))

    def state(ui: bpy.types.UIList, data: Struct, chosen: Struct) -> Outcome[_Drag]:
        origin, members = getattr(chosen, index), getattr(data, items)
        pitch = float(Metrics.read().unit) * (1.0 if ui.use_filter_sort_reverse else -1.0)
        hidden = any(not _held(predicate, context, ui, item).default_value(True) for predicate in where.to_list() for item in members)
        return (
            Ok((origin, pitch, (data, items), (chosen, index)))
            .filter_with(lambda _: not hidden, lambda _: Fault.of(BlenderReason.REQUIRED, f"{ui.list_id} without hidden items"))
            .filter_with(lambda _: 0 <= origin < len(members), lambda _: Fault.of(BlenderReason.ABSENT, items, member=f"{index} {origin}"))
        )

    return paired(
        Option
        .of_optional(context.ui_list)
        .to_result_with(partial(Fault.of, BlenderReason.ABSENT, "bpy.context", member="ui_list"))
        .filter_with(lambda held: not held.filter_name, lambda held: Fault.of(BlenderReason.REQUIRED, f"{held.list_id} without name filter")),
        pointer(name),
        pointer(f"{name}_active"),
    ).bind(lambda parts: state(*parts))


def _previewed(move: Option[Move], context: bpy.types.Context, state: _Drag, travel: Travel) -> Outcome[Option[str]]:
    """Places the dragged item at the row the travel reaches."""
    origin, pitch, _, _ = state
    return _placed(move, context, state, origin + round(travel.dy / pitch)).map(lambda _: Nothing)


def _restored(move: Option[Move], context: bpy.types.Context, state: _Drag) -> Outcome[None]:
    """Returns the dragged item to its row at press."""
    return _placed(move, context, state, state[0])


def _placed(move: Option[Move], context: bpy.types.Context, state: _Drag, target: int) -> Outcome[None]:
    """Moves the selected item to the clamped target row, the stated move or the host collection's own, and points the selection at it."""
    _, _, items, selection = state
    members, current = getattr(*items), getattr(*selection)
    if (row := min(max(target, 0), len(members) - 1)) == current:
        return Ok(None)
    moved = move.map(lambda reorder: reorder(context, current, row)).default_with(lambda: Ok(members.move(current, row)))
    return moved.map(lambda _: setattr(*selection, row))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ItemList", "Move", "Predicate", "context_path"]
