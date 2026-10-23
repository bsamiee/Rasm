"""Collection lists with computed selection, filtering, and sorting."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import Native, struct_at
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisition, Acquisitions, Band, entered
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.settings.attachments import Attached, Attachment, pointed
from blender.bpy.ui.layout.rows import acquired, alerted, Line, located, Member, Property, realized, refused, ResolvedMember, RowContent, RowDraw
from blender.imaging.faults import accumulated, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Where[O: "bpy.types.bpy_struct[object]"] = Condition | Callable[[bpy.types.Context, O], bool]

# --- [MODELS] ---------------------------------------------------------------------------


class Listed(msgspec.Struct, frozen=True, kw_only=True):
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


class ItemList[T: msgspec.Struct = msgspec.Struct, O: "bpy.types.bpy_struct[object]" = "bpy.types.bpy_struct[object]"](Line, frozen=True, kw_only=True):
    """`UIList` over host collection, attached fields before item columns."""

    key: Key
    collection: Member
    active: Member
    item: type[O]
    attachment: Option[Attachment[T]] = Nothing
    columns: tuple[RowContent, ...] = ()
    item_dyntip_propname: Option[str] = Nothing
    order: Option[str] = Nothing
    icon: Option[str] = Nothing
    where: Option[Where[O]] = Nothing
    enabled: Option[Callable[[bpy.types.Context, O], BlenderResult[bool]]] = Nothing
    sort_reverse: bool = False
    side: tuple[RowContent, ...] = ()

    def selection(self, identity: Identity) -> BlenderResult[Member]:
        """Names selection property on shared owner or window manager."""
        shared = self.active.root is self.collection.root and self.active.reach == self.collection.reach and isinstance(self.active.root, bpy.types.bpy_struct_meta_idprop)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        root, reach = (self.active.root, self.active.reach) if shared else (bpy.types.WindowManager, Nothing)
        return identity.name(Name.ATTACHMENT, self.key).map(lambda name: Member(root=root, path=name, reach=reach))

    def resolved(self, identity: Identity) -> BlenderResult[Listed]:
        """Resolves selection, attached fields, and row draws."""

        def itemized(bound: ResolvedMember[bpy.types.CollectionProperty]) -> BlenderResult[tuple[ResolvedMember[bpy.types.CollectionProperty], O, str]]:
            item = bound.descriptor.prop.fixed_type
            if not isinstance(item, self.item):
                return Error(refused(bound.descriptor.path, type(item).__name__, (self.item.__name__,)))
            return (
                Option
                .of_optional(item.bl_rna.name_property)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                .map(lambda named: (bound, item, named.identifier))
                .to_result_with(lambda: (BlenderAbsentFault(owner=bound.descriptor.path, key="name_property"),))
            )

        def over(parts: tuple[str, tuple[tuple[ResolvedMember[bpy.types.CollectionProperty], O, str], ResolvedMember]]) -> BlenderResult[Listed]:
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

            def path[P: bpy.types.Property](found: Option[str], kind: type[P]) -> BlenderResult[Option[ResolvedMember[P]]]:
                return found.map(lambda text: Member(root=type(item), path=text).resolved(kind).map(Some)).default_value(Ok(Nothing))

            def tooltip(held: Option[ResolvedMember]) -> BlenderResult[Option[ResolvedMember]]:
                return (
                    held
                    .filter(lambda member: bool(member.descriptor.owner))
                    .map(lambda member: Error(refused(member.descriptor.path, member.descriptor.owner, ("direct item string property",))))
                    .default_value(Ok(held))
                )

            def sortable(held: Option[ResolvedMember]) -> BlenderResult[Option[ResolvedMember]]:
                return (
                    held
                    .filter(
                        lambda member: (
                            not isinstance(member.descriptor.prop, (bpy.types.BoolProperty, bpy.types.IntProperty, bpy.types.FloatProperty, bpy.types.StringProperty, bpy.types.EnumProperty))
                        )
                    )
                    .map(lambda member: Error(refused(member.descriptor.path, type(member.descriptor.prop).__name__, ("ordered value property",))))
                    .default_value(Ok(held))
                )

            def scalar(held: Option[ResolvedMember[bpy.types.IntProperty]]) -> BlenderResult[Option[ResolvedMember[bpy.types.IntProperty]]]:
                return (
                    held
                    .filter(lambda member: member.descriptor.prop.is_array and member.descriptor.index.is_none())
                    .map(lambda member: Error(refused(member.descriptor.path, "array", ("integer component",))))
                    .default_value(Ok(held))
                )

            def chosen() -> BlenderResult[Option[ResolvedMember]]:
                match active.descriptor.prop:
                    case bpy.types.PointerProperty():
                        return self.selection(identity).bind(lambda member: member.resolved(bpy.types.IntProperty)).map(Some)
                    case _:
                        return Ok(Nothing)

            def grouped(attached: Attached[T]) -> Callable[[RowDraw], RowDraw]:
                return lambda draw: lambda layout, holder, context: draw(layout, getattr(holder, attached.name), context)

            def fielded(attachment: Attachment[T]) -> BlenderResult[tuple[RowDraw, ...]]:
                return (attachment.bound(identity) if isinstance(item, attachment.owner) else Error(refused(collection.descriptor.path, type(item).__name__, (attachment.owner.__name__,)))).bind(
                    lambda attached: accumulated(
                        realized(Property(target=row, text=Some("")), identity)
                        for row in msgspec.structs.fields(attached.group.record)
                        if row.name in attached.group.struct.__annotations__ or row.name in attached.group.struct.bl_rna.properties
                    ).map(lambda draws: tuple(map(grouped(attached), draws)))
                )

            def listed(
                found: tuple[
                    tuple[Option[ResolvedMember], tuple[Option[ResolvedMember], tuple[Option[ResolvedMember], Option[ResolvedMember]]]],
                    tuple[tuple[tuple[RowDraw, ...], tuple[RowDraw, ...]], tuple[RowDraw, ...]],
                ],
            ) -> Listed:
                (picked, (tip, (order, icon))), ((fields, columns), sides) = found
                return Listed(name=name, label=label, collection=collection, active=active, chosen=picked, item_dyntip_propname=tip, order=order, icon=icon, columns=(*fields, *columns), sides=sides)

            return paired(
                paired(
                    chosen(),
                    paired(
                        path(self.item_dyntip_propname, bpy.types.StringProperty).bind(tooltip),
                        paired(path(self.order, bpy.types.Property).bind(sortable), path(self.icon, bpy.types.IntProperty).bind(scalar)),
                    ),
                ),
                paired(
                    paired(self.attachment.map(fielded).default_value(Ok(())), accumulated(realized(row, identity) for row in self.columns)), accumulated(realized(row, identity) for row in self.side)
                ),
            ).map(listed)

        held = paired(self.collection.resolved(bpy.types.CollectionProperty).bind(itemized), self.active.resolved())
        return paired(identity.name(Name.LIST, self.key), held).bind(over)

    def uilist(self, listed: Listed) -> type[bpy.types.UIList]:
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
            _data: "bpy.types.bpy_struct[object]",
            item: O,
            icon: int,
            _active_data: "bpy.types.bpy_struct[object]",
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

        def filter_items(ui: bpy.types.UIList, context: bpy.types.Context, data: "bpy.types.bpy_struct[object]", propname: str) -> tuple[list[int], list[int]]:
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

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires selection property and deferred list class."""

        @contextmanager
        def registered() -> Generator[BlenderResult[None]]:
            with ExitStack() as stack:
                yield self.resolved(identity).bind(lambda listed: entered(stack, (self.uilist(listed),)))

        def bridge(
            parts: tuple[Member, ResolvedMember[bpy.types.CollectionProperty]], active: ResolvedMember, pointer: bpy.types.PointerProperty
        ) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
            member, collection = parts
            if not isinstance(collection.descriptor.prop.fixed_type, type(pointer.fixed_type)):
                return Error(refused(active.descriptor.path, type(pointer.fixed_type).__name__, (type(collection.descriptor.prop.fixed_type).__name__,)))

            def selected(owner: "bpy.types.bpy_struct[object]") -> int:
                items = located(collection, owner, bpy.context).starmap(getattr)
                pointer = located(active, owner, bpy.context).starmap(getattr)
                return next((index for members in items.to_list() for index, item in enumerate(members) for held in pointer.to_list() if item == held), -1)

            def written(owner: "bpy.types.bpy_struct[object]", index: int) -> None:
                for source in located(collection, owner, bpy.context).to_list():
                    members = getattr(*source)
                    for holder, key in located(active, owner, bpy.context).to_list():
                        setattr(holder, key, members[index] if index >= 0 else None)

            return Ok(((Band.ATTACHMENTS, pointed(member.root, member.path, bpy.props.IntProperty(get=selected, set=written, options={"HIDDEN", "SKIP_SAVE"}))),))

        def schema(active: ResolvedMember) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
            match active.descriptor.prop:
                case bpy.types.PointerProperty() as pointer:
                    return paired(self.selection(identity), self.collection.resolved(bpy.types.CollectionProperty)).bind(lambda parts: bridge(parts, active, pointer))
                case _:
                    return Ok(())

        return self.active.resolved().bind(schema).map(lambda pairs: (*self.attachment.to_list(), *acquired((*self.columns, *self.side)), *pairs, (Band.INTERFACE, registered())))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Draws list with its side column."""

        def draw(listed: Listed) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for (owner, key), (picked, member) in (
                    (found, pick) for found in located(listed.collection, holder, context).to_list() for pick in located(listed.chosen.default_value(listed.active), holder, context).to_list()
                ):
                    row = layout.row()
                    row.template_list(
                        listed.name,
                        keyed(owner),
                        owner,
                        key,
                        picked,
                        member,
                        item_dyntip_propname=listed.item_dyntip_propname.map(lambda bound: bound.descriptor.prop.identifier).default_value(""),
                        sort_reverse=self.sort_reverse,
                        sort_lock=self.sort_reverse,
                    )
                    column = row.column(align=True)
                    for side in listed.sides:
                        side(column, holder, context)

            return shown

        return self.resolved(identity).map(draw)


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

__all__ = ["ItemList", "Listed", "Where", "keyed"]
