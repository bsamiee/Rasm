"""Collection lists with computed selection, filtering, sorting, and drag reorder."""

import builtins
from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import partial
import typing
from typing import override

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
from msgspec import UNSET, UnsetType

from blender.bpy.commands.operators import Answer, Done, Empty, Gesture, Modal, Operator, Travel
from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import Native, Struct, struct_at
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisition, Acquisitions, Band, entered
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import annotated, Attached, Attachment, pointed
from blender.bpy.ui.layout.rows import acquired, alerted, At, Call, FUNCTIONS, Member, OperatorButton, placed, Property, Reach, realized, refused, ResolvedMember, RowContent, RowDraw
from blender.bpy.ui.resources.theme import Metrics
from blender.records.faults import accumulated, Fault, Outcome, paired

if typing.TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Where[O: Struct] = Condition | Callable[[bpy.types.Context, O], bool]

# --- [MODELS] ---------------------------------------------------------------------------


class Dragged(msgspec.Struct, frozen=True, kw_only=True):
    """Dragged item's index at press and the list row pitch in device pixels."""

    origin: int
    pitch: float


class _Listed(msgspec.Struct, frozen=True, kw_only=True):
    """List with host names resolved and rows realized."""

    name: str
    label: str
    collection: ResolvedMember[bpy.types.CollectionProperty]
    active: ResolvedMember
    chosen: Option[ResolvedMember]
    item_dyntip_propname: Option[ResolvedMember]
    order: Option[ResolvedMember]
    icon: Option[ResolvedMember]
    columns: tuple[RowDraw, ...]
    sides: tuple[RowDraw, ...]


class ItemList[T: bpy.types.PropertyGroup = bpy.types.PropertyGroup, O: Struct = Struct](Call, frozen=True, kw_only=True):
    """`UIList` over host collection, attached fields before item columns."""

    function = FUNCTIONS["template_list"]
    key: Key
    collection: Member
    active: Member
    item: builtins.type[O]
    attachment: Option[Attachment[T]] = Nothing
    columns: tuple[RowContent, ...] = ()
    side: tuple[RowContent, ...] = ()
    absent: tuple[RowContent, ...] = ()
    item_dyntip_propname: Option[str] = Nothing
    order: Option[str] = Nothing
    icon: Option[str] = Nothing
    where: Option[Where[O]] = Nothing
    enabled: Option[Callable[[bpy.types.Context, O], Outcome[bool]]] = Nothing
    rows: int | UnsetType = UNSET
    maxrows: int | UnsetType = UNSET
    type: "rna_enums.UilistLayoutTypeItems | UnsetType" = UNSET
    sort_reverse: bool | UnsetType = UNSET
    sort_lock: bool | UnsetType = UNSET
    move: Option[Callable[[bpy.types.Context, int, int], Outcome[None]]] = Nothing

    @property
    def sides(self) -> tuple[RowContent, ...]:
        """Side rows, with a grip dragging the active item by list rows while `move` reorders the collection."""
        return (*self.side, *self.dragging.map(lambda command: OperatorButton(target=command, text="", icon=Some("GRIP"))).to_list())

    @property
    def dragging(self) -> Option[Operator[Empty, Dragged]]:
        """Drag of the active item by list rows, `move` placing it at each row and the active index following it, Escape restoring its row."""

        def held(context: bpy.types.Context) -> Outcome[tuple[int, int, ResolvedMember]]:
            def indexed(collection: ResolvedMember[bpy.types.CollectionProperty], active: ResolvedMember) -> Outcome[tuple[int, int, ResolvedMember]]:
                items = collection.at(context, context).map(lambda at: getattr(*at))
                chosen = active.at(context, context).map(lambda at: getattr(*at))
                return (
                    items
                    .bind(lambda members: chosen.map(lambda pick: (pick if isinstance(pick, int) else next((index for index, item in enumerate(members) if item == pick), -1), len(members))))
                    .filter(lambda pair: 0 <= pair[0] < pair[1])
                    .map(lambda pair: (*pair, active))
                    .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=collection.descriptor.path, member=active.descriptor.path),))
                )

            return paired(self.collection.resolved(bpy.types.CollectionProperty), self.active.resolved()).bind(lambda pair: indexed(*pair))

        def relocated(move: Callable[[bpy.types.Context, int, int], Outcome[None]], context: bpy.types.Context, target: int) -> Outcome[None]:
            def moved(current: int, count: int, active: ResolvedMember) -> Outcome[None]:
                row = min(max(target, 0), count - 1)

                def followed(_: None) -> None:
                    for owner, key in active.at(context, context).to_list() if isinstance(active.descriptor.prop, bpy.types.IntProperty) else ():
                        setattr(owner, key, row)

                return Ok(None) if row == current else move(context, current, row).map(followed)

            return held(context).bind(lambda found: moved(*found))

        def gesture(move: Callable[[bpy.types.Context, int, int], Outcome[None]]) -> Modal[Dragged]:
            def preview(context: bpy.types.Context, state: Dragged, travel: Travel) -> Outcome[Option[str]]:
                return relocated(move, context, state.origin + round((travel.dy if self.sort_reverse is True else -travel.dy) / state.pitch)).map(lambda _: Nothing)

            return Modal(
                gesture=Gesture.DRAG_Y,
                begin=lambda context, _travel: held(context).map(lambda found: Dragged(origin=found[0], pitch=float(Metrics.read().unit))),
                preview=preview,
                restore=lambda context, state: relocated(move, context, state.origin),
            )

        def dragged(_call: object) -> Outcome[Answer]:
            """Drags the active item by list rows, Escape restoring its row."""
            return Ok(Done())

        dragged.__name__ = dragged.__qualname__ = f"{self.key}_drag"
        return self.move.map(lambda move: Operator[Empty, Dragged](form=Empty, run=dragged, invoke=Some(gesture(move)), internal=True))

    def selection(self, identity: Identity) -> Outcome[Member]:
        """Names selection property on shared owner or window manager."""
        shared = self.active.root is self.collection.root and self.active.reach == self.collection.reach and isinstance(self.active.root, bpy.types.bpy_struct_meta_idprop)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        root, reach = (self.active.root, self.active.reach) if shared else (bpy.types.WindowManager, Nothing)
        return identity.identifier(self.key).map(lambda name: Member(root=root, path=name, reach=reach))

    def resolved(self, identity: Identity) -> Outcome[_Listed]:
        """Resolves selection, item members, attached fields, and row draws."""

        def itemized(bound: ResolvedMember[bpy.types.CollectionProperty]) -> Outcome[tuple[ResolvedMember[bpy.types.CollectionProperty], O, str]]:
            item = bound.descriptor.prop.fixed_type
            if not isinstance(item, self.item):
                return Error(refused(bound.descriptor.path, type(item).__name__, (self.item.__name__,)))
            return (
                Option
                .of_optional(item.bl_rna.name_property)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                .map(lambda named: (bound, item, named.identifier))
                .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=bound.descriptor.path, member="name_property"),))
            )

        def over(parts: tuple[str, tuple[tuple[ResolvedMember[bpy.types.CollectionProperty], O, str], ResolvedMember]]) -> Outcome[_Listed]:
            name, ((collection, item, label), active) = parts
            match active.descriptor.prop:
                case bpy.types.IntProperty(is_array=True):
                    return Error(refused(active.descriptor.path, "integer array", ("scalar integer selection",)))
                case bpy.types.PointerProperty(fixed_type=target) if not isinstance(item, type(target)):
                    return Error(refused(active.descriptor.path, type(target).__name__, (type(item).__name__,)))
                case bpy.types.IntProperty() | bpy.types.PointerProperty():
                    pass
                case prop:
                    return Error(refused(active.descriptor.path, type(prop).__name__, (bpy.types.IntProperty.__name__, bpy.types.PointerProperty.__name__)))

            def member(found: Option[str], kinds: tuple[type[Struct], ...], admitted: Callable[[ResolvedMember], bool], expected: str) -> Outcome[Option[ResolvedMember]]:
                return found.map(
                    lambda path: (
                        Member(root=type(item), path=path)
                        .resolved()
                        .bind(lambda held: held.accepts(kinds))
                        .bind(lambda held: Ok(Some(held)) if admitted(held) else Error(refused(held.descriptor.path, type(held.descriptor.prop).__name__, (expected,))))
                    )
                ).default_value(Ok(Nothing))

            def chosen() -> Outcome[Option[ResolvedMember]]:
                match active.descriptor.prop:
                    case bpy.types.PointerProperty():
                        return self.selection(identity).bind(lambda member: member.resolved(bpy.types.IntProperty)).map(Some)
                    case _:
                        return Ok(Nothing)

            def fielded(attachment: Attachment[T]) -> Outcome[tuple[RowDraw, ...]]:
                def grouped(attached: Attached[T], draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
                    draw(layout, getattr(holder, attached.name), context)

                return (attachment.bound(identity) if isinstance(item, attachment.owner) else Error(refused(collection.descriptor.path, type(item).__name__, (attachment.owner.__name__,)))).bind(
                    lambda attached: accumulated(
                        realized(Property(target=Member(root=attached.form, path=name), text=""), identity).map(lambda draw: partial(grouped, attached, draw)) for name in annotated(attached.form)
                    )
                )

            def listed(found: tuple[tuple[Option[ResolvedMember], ...], tuple[tuple[RowDraw, ...], ...]]) -> _Listed:
                (picked, tip, order, icon), (fields, columns, sides) = found
                return _Listed(name=name, label=label, collection=collection, active=active, chosen=picked, item_dyntip_propname=tip, order=order, icon=icon, columns=(*fields, *columns), sides=sides)

            return paired(
                accumulated((
                    chosen(),
                    member(self.item_dyntip_propname, (bpy.types.StringProperty,), lambda held: not held.descriptor.owner, "direct item string property"),
                    member(
                        self.order, (bpy.types.BoolProperty, bpy.types.IntProperty, bpy.types.FloatProperty, bpy.types.StringProperty, bpy.types.EnumProperty), lambda _: True, "ordered value property"
                    ),
                    member(
                        self.icon,
                        (bpy.types.IntProperty,),
                        lambda held: not (isinstance(held.descriptor.prop, bpy.types.IntProperty) and held.descriptor.prop.is_array) or held.descriptor.index.is_some(),
                        "integer component",
                    ),
                )),
                accumulated((
                    self.attachment.map(fielded).default_value(Ok(())),
                    accumulated(realized(row, identity) for row in self.columns),
                    accumulated(realized(row, identity) for row in self.sides),
                )),
            ).map(listed)

        held = paired(self.collection.resolved(bpy.types.CollectionProperty).bind(itemized), self.active.resolved())
        return paired(identity.name(bpy.types.UIList, self.key), held).bind(over)

    def uilist(self, listed: _Listed) -> builtins.type[bpy.types.UIList]:
        """Builds list class, filtering when `where` or `order` is set."""

        def accepts(where: Where[O], context: bpy.types.Context, item: O) -> bool:
            match where:
                case Callable():
                    return where(context, item)
                case _:
                    return holds(where, context, Some(item))

        def draw_item(
            ui: bpy.types.UIList,
            context: bpy.types.Context,
            layout: bpy.types.UILayout,
            _data: Struct,
            item: O,
            icon: int,
            _active_data: Struct,
            _active_property: str,
            _index: int = 0,
            _flt_flag: int = 0,
        ) -> None:
            def shown(*, active: bool) -> None:
                row = layout.row(align=True)
                row.active = active
                glyph = (
                    listed.icon
                    .bind(lambda bound: catch(lambda: item.path_resolve(bound.descriptor.path), exception=ValueError)().to_option())
                    .bind(lambda held: Some(held) if isinstance(held, int) else Nothing)
                    .default_value(icon)
                )
                row.prop(item, listed.label, text="", emboss=False, icon_value=glyph)
                for column in listed.columns if ui.layout_type == "DEFAULT" else ():
                    column(row, item, context)

            alerted(layout, self.enabled.map(lambda policy: policy(context, item)).default_value(Ok(True)).map(lambda active: shown(active=active)))

        def filter_items(ui: bpy.types.UIList, context: bpy.types.Context, data: Struct, propname: str) -> tuple[list[int], list[int]]:
            items, shown = getattr(data, propname), ui.bitflag_filter_item
            named = bpy.types.UI_UL_list.filter_items_by_name(ui.filter_name, shown, items, listed.label, reverse=ui.use_filter_invert) or [shown] * len(items)
            flags = [flag if all(accepts(where, context, item) for where in self.where.to_list()) else 0 for flag, item in zip(named, items)]
            if ui.use_filter_sort_lock:
                return flags, []
            match listed.order:
                case Option(tag="some", some=bound):
                    return flags, bpy.types.UI_UL_list.sort_items_helper(  # ty: ignore[invalid-return-type]
                        [(index, struct_at(item, bound.descriptor.owner).map(bound.descriptor.read).to_optional()) for index, item in enumerate(items)],  # type: ignore[misc]
                        lambda pair: (pair[1] is None, pair[1]),
                    )
                case _:
                    return flags, bpy.types.UI_UL_list.sort_items_by_name(items, listed.label) if ui.use_filter_sort_alpha else []  # ty: ignore[invalid-return-type]

        filtered = {"filter_items": filter_items} if self.where.is_some() or listed.order.is_some() else {}
        return type(listed.name, (bpy.types.UIList,), {"bl_idname": listed.name, "draw_item": draw_item} | filtered)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Acquires selection property and deferred list class."""

        @contextmanager
        def registered() -> Generator[Outcome[None]]:
            with ExitStack() as stack:
                yield self.resolved(identity).bind(lambda listed: entered(stack, (self.uilist(listed),)))

        def bridge(parts: tuple[Member, ResolvedMember[bpy.types.CollectionProperty]], active: ResolvedMember, pointer: bpy.types.PointerProperty) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
            member, collection = parts
            if not isinstance(collection.descriptor.prop.fixed_type, type(pointer.fixed_type)):
                return Error(refused(active.descriptor.path, type(pointer.fixed_type).__name__, (type(collection.descriptor.prop.fixed_type).__name__,)))

            def selected(owner: Struct) -> int:
                items = collection.at(owner, bpy.context).starmap(getattr)
                pointer = active.at(owner, bpy.context).starmap(getattr)
                return next((index for members in items.to_list() for index, item in enumerate(members) for held in pointer.to_list() if item == held), -1)

            def written(owner: Struct, index: int) -> None:
                for source in collection.at(owner, bpy.context).to_list():
                    members = getattr(*source)
                    for holder, key in active.at(owner, bpy.context).to_list():
                        setattr(holder, key, members[index] if index >= 0 else None)

            return Ok(((Band.ATTACHMENTS, pointed(member.root, member.path, bpy.props.IntProperty(get=selected, set=written, options={"HIDDEN", "SKIP_SAVE"}))),))

        def schema(active: ResolvedMember) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
            match active.descriptor.prop:
                case bpy.types.PointerProperty() as pointer:
                    return paired(self.selection(identity), self.collection.resolved(bpy.types.CollectionProperty)).bind(lambda parts: bridge(parts, active, pointer))
                case _:
                    return Ok(())

        return self.active.resolved().bind(schema).map(lambda pairs: (*self.attachment.to_list(), *acquired((*self.columns, *self.sides, *self.absent)), *pairs, (Band.INTERFACE, registered())))

    def picked(self, identity: Identity) -> Outcome[Reach]:
        """Returns reach of the active item the list highlights, through the selection the list draws."""

        def reach(listed: _Listed) -> Reach:
            def indexed(found: At, pick: At) -> Option[Struct]:
                items, index = getattr(*found), getattr(*pick)
                return Some(items[index]) if 0 <= index < len(items) else Nothing

            def active(context: bpy.types.Context) -> Option[Struct]:
                match listed.active.descriptor.prop:
                    case bpy.types.PointerProperty():
                        return listed.active.at(context, context).bind(lambda pair: Option.of_optional(getattr(*pair)))
                    case _:
                        return listed.collection.at(context, context).bind(lambda found: listed.active.at(context, context).bind(lambda pick: indexed(found, pick)))

            return active

        return self.resolved(identity).map(reach)

    def shown(self, listed: _Listed, absent: tuple[RowDraw, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the list with its side column at the located collection, absent rows while no owner holds it."""
        tip = listed.item_dyntip_propname.map(lambda held: {"item_dyntip_propname": held.descriptor.prop.identifier}).default_value({})
        found = listed.collection.at(holder, context)
        for draw in () if found.is_some() else absent:
            draw(layout, holder, context)
        for (owner, key), (picked, member) in ((held, pick) for held in found.to_list() for pick in listed.chosen.default_value(listed.active).at(holder, context).to_list()):
            row = layout.row()
            self.called(row, listed.name, keyed(owner), owner, key, picked, member, **self.options, **tip)
            column = row.column(align=True)
            for side in listed.sides:
                side(column, holder, context)

    @override
    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Draws list with its side column, absent rows while no owner holds the collection."""
        return paired(self.resolved(identity), placed(self.absent, identity)).map(lambda parts: partial(self.shown, *parts))


# --- [OPERATIONS] -----------------------------------------------------------------------


def keyed(struct: Native) -> str:
    """Returns ID `session_uid`, struct name, or nameless struct address."""
    if isinstance(struct, bpy.types.bpy_prop_collection):
        return keyed(struct.data)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    match struct, struct.bl_rna.name_property:  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        case bpy.types.ID(), _:
            return str(struct.session_uid)
        case _, bpy.types.StringProperty(identifier=name):
            return str(getattr(struct, name))
        case _:
            return str(struct.as_pointer())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Dragged", "ItemList", "Where", "keyed"]
