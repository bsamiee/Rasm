"""Header sections over one holder or each collection item, with record fields and store values popovers."""

from collections.abc import Callable, Mapping
from itertools import product, starmap
from types import MappingProxyType, MemberDescriptorType
import typing
from typing import Protocol, runtime_checkable, Self, TypeIs

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands import operators
from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import root_in
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.settings.attachments import Host
from blender.bpy.settings.properties import RecordGroup
from blender.bpy.settings.shapes import ENABLED, Entries, EXPANDED, field_shapes, Gate, Nested, Records, Shape, Tagged
from blender.bpy.ui.layout.lists import ItemList, keyed
from blender.bpy.ui.layout.rows import acquired, At, Member, placed, Property, realized, refused, ResolvedMember, Row, RowContent, RowDraw, SurfaceDraw
from blender.bpy.ui.layout.values import Hosted, Replacement, Slots, Store, Values
from blender.imaging.faults import accumulated, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Nest = Callable[[bpy.types.UILayout, bpy.types.bpy_struct[object], bpy.types.Context, str], None]
type Nesting = Gate | Nested | Entries | Tagged | Records
type Holding = Callable[[bpy.types.bpy_struct[object], bpy.types.Context], Option[bpy.types.bpy_struct[object]]]
type Hosting = Callable[[bpy.types.bpy_struct[object], bpy.types.Context], Option[Host]]


@runtime_checkable
class Section(Row, Registrable, Protocol):
    """Section a panel, popover, or append places."""

    @property
    def key(self) -> Key:
        """Key every section idname derives from."""

    @property
    def title(self) -> str:
        """Title section header draws."""

    @property
    def description(self) -> str:
        """Hover text of a popover button opening the section."""

    @property
    def opened(self) -> bool:
        """Whether the section starts open."""

    @property
    def popovers(self) -> tuple["Single[operators.Empty]", ...]:
        """Values popovers of section store."""

    def realized(self, identity: Identity) -> BlenderResult["Drawn"]:
        """Realizes section into panel and layout panel draws."""


# --- [MODELS] ---------------------------------------------------------------------------


class Drawn(msgspec.Struct, frozen=True, kw_only=True):
    """Section draws for panel header, body, and layout panel, and whether a context holds its holder."""

    head: Option[SurfaceDraw]
    tail: Option[SurfaceDraw]
    body: SurfaceDraw
    inline: RowDraw
    present: Callable[[bpy.types.Context], bool] = lambda _context: True


class _Frame[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Realized section parts."""

    name: str
    gate: Option[tuple[ResolvedMember, RowDraw]]
    header: tuple[RowDraw, ...]
    rows: tuple[RowDraw, ...]
    fields: tuple[RowDraw, ...]
    nest: Option[Nest]
    slots: Option[Slots[T]]

    @classmethod
    def of(cls, identity: Identity, key: Key, rows: tuple[RowContent, ...], header: tuple[RowContent, ...], enable: Option[Member], slots: Option[Slots[T]]) -> BlenderResult[Self]:
        """Realizes section parts at acquisition, rows and store scalars on one grid."""
        walk = slots.map(lambda held: fields(held.attached.group, identity, held.fields).map(Some)).default_value(Ok(Nothing))
        gate = enable.map(
            lambda member: member.resolved(bpy.types.BoolProperty).bind(lambda held: realized(Property(target=member, text=""), identity).map(lambda draw: Some((held, draw))))
        ).default_value(Ok(Nothing))

        def built(parts: tuple[tuple[str, Option[tuple[ResolvedMember, RowDraw]]], tuple[tuple[RowDraw, ...], Option[tuple[tuple[RowContent, ...], Nest]]]]) -> BlenderResult[Self]:
            (name, gated), (heads, walked) = parts
            scalars = walked.map(lambda nested: nested[0]).default_value(())
            return placed((*rows, *scalars), identity).map(
                lambda draws: cls(name=name, gate=gated, header=heads, rows=draws[: len(rows)], fields=draws[len(rows) :], nest=walked.map(lambda nested: nested[1]), slots=slots)
            )

        return paired(paired(identity.name(Name.ATTACHMENT, key), gate), paired(accumulated(realized(row, identity) for row in header), walk)).bind(built)

    def head(self, layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
        """Draws textless header enable."""
        for _, draw in self.gate.to_list():
            draw(layout, holder, context)

    def tail(self, layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", host: Option[Host], context: bpy.types.Context) -> None:
        """Draws header rows, then flat store slots with host as context pointer."""
        line = layout.row(align=True)
        for draw in self.header:
            draw(line, holder, context)
        for slots, found in product(self.slots.to_list(), host.to_list()):
            line.emboss = "NONE"
            line.context_pointer_set(slots.attached.name, found)
            for draw in slots.tail:
                draw(line, holder, context)

    def body(self, layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", host: Option[Host], context: bpy.types.Context, prefix: str) -> None:
        """Draws rows and store fields, inactive while enable is off."""
        layout.use_property_split, layout.use_property_decorate = True, True
        layout.active = layout.active and all(starmap(getattr, self.gate.bind(lambda pair: pair[0].at(holder, context)).to_list()))
        for draw in self.rows:
            draw(layout, holder, context)
        for slots, found in product(self.slots.to_list(), host.to_list()):
            group = slots.attached.struct(found)
            for draw in self.fields:
                draw(layout, group, context)
            for nest in self.nest.to_list():
                nest(layout, group, context, prefix)

    def framed(
        self, layout: bpy.types.UILayout, idname: str, title: str, holder: "bpy.types.bpy_struct[object]", host: Option[Host], context: bpy.types.Context, *, opened: At | bool, active: bool = True
    ) -> Option[bpy.types.UILayout]:
        """Draws layout panel header open by stored property or by default, returning body while open."""
        header, body = layout.panel(idname, default_closed=not opened) if isinstance(opened, bool) else layout.panel_prop(*opened)
        header.active = header.active and active
        header.context_pointer_set(self.name, holder)
        self.head(header, holder, context)
        header.label(text=title)
        right = header.row(align=True)
        right.alignment = "RIGHT"
        self.tail(right, holder, host, context)
        opened_body = Option.of_optional(body)
        for shown in opened_body.to_list():
            shown.active = shown.active and active
            shown.context_pointer_set(self.name, holder)
        return opened_body


class Single[T: msgspec.Struct = operators.Empty](msgspec.Struct, frozen=True, kw_only=True):
    """Rows under header over one holder, a member of the drawing holder, the active item of a list, or the drawing holder while `holder` is unset."""

    key: Key
    title: str
    description: str = ""
    rows: tuple[RowContent, ...] = ()
    header: tuple[RowContent, ...] = ()
    enable: Option[Member] = Nothing
    store: Option[Hosted[T]] = Nothing
    condition: Option[Condition] = Nothing
    holder: Option[Member | ItemList] = Nothing
    opened: bool = False

    @property
    def popovers(self) -> tuple["Single[operators.Empty]", ...]:
        """Values popover of store."""
        return tuple(popover(hosted.store) for hosted in self.store.to_list())

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires registrable rows, holding list, and store rows."""
        return Ok((
            *acquired((*self.rows, *self.header, *(held for held in self.holder.to_list() if isinstance(held, ItemList)))),
            *(row for hosted in self.store.to_list() for row in (hosted.store.attachment, hosted.store)),
        ))

    def realized(self, identity: Identity) -> BlenderResult[Drawn]:
        """Realizes section into panel and layout panel draws, the store host read from its member or the context root of the store's owner."""

        def reading(bound: ResolvedMember) -> Holding:
            return lambda holder, context: bound.at(holder, context).bind(lambda pair: Option.of_optional(getattr(*pair)))

        def hosting(hosted: Hosted[T]) -> BlenderResult[tuple[Hosting, Slots[T]]]:
            owner = hosted.store.attachment.owner
            host: BlenderResult[Hosting] = hosted.host.map(
                lambda member: (
                    member
                    .resolved()
                    .bind(lambda bound: bound.accepts((owner,)))
                    .map(lambda bound: lambda holder, context: bound.at(holder, context).bind(lambda pair: Option.of_optional(getattr(*pair))))
                )
            ).default_value(Ok(lambda _holder, context: root_in(owner, context)))
            return paired(host, hosted.store.bound(identity))

        def reached(source: Member | ItemList) -> BlenderResult[Holding]:
            match source:
                case Member():
                    return source.resolved().map(reading)
                case ItemList():
                    return source.picked(identity).map(lambda reach: lambda _holder, context: reach(context))

        holding = self.holder.map(reached).default_value(Ok(lambda holder, _context: Some(holder)))

        def drawn(parts: tuple[tuple[_Frame[T], Option[Hosting]], Holding]) -> Drawn:
            (frame, host), held = parts

            def found(holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> Option[Host]:
                return host.bind(lambda read: read(holder, context))

            def head(layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
                for holder in held(context, context).to_list():
                    frame.head(layout, holder, context)

            def tail(layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
                for holder in held(context, context).to_list():
                    frame.tail(layout, holder, found(holder, context), context)

            def body(layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
                for holder in held(context, context).to_list():
                    layout.active = layout.active and all(holds(required, context, Some(holder)) for required in self.condition.to_list())
                    frame.body(layout, holder, found(holder, context), context, frame.name)

            def inline(layout: bpy.types.UILayout, outer: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for holder in held(outer, context).to_list():
                    active = all(holds(required, context, Some(holder)) for required in self.condition.to_list())
                    host = found(holder, context)
                    for shown in frame.framed(layout, frame.name, self.title, holder, host, context, opened=self.opened, active=active).to_list():
                        frame.body(shown, holder, host, context, frame.name)

            return Drawn(
                head=frame.gate.map(lambda _: head),
                tail=Some(tail) if self.header or self.store.is_some() else Nothing,
                body=body,
                inline=inline,
                present=lambda context: held(context, context).is_some(),
            )

        return paired(
            self.store
            .map(lambda hosted: hosting(hosted).map(Some))
            .default_value(Ok(Nothing))
            .bind(lambda held: _Frame.of(identity, self.key, self.rows, self.header, self.enable, held.map(lambda pair: pair[1])).map(lambda frame: (frame, held.map(lambda pair: pair[0])))),
            holding,
        ).map(drawn)

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns section as layout panel row."""
        return self.realized(identity).map(lambda drawn: drawn.inline)


class Each[T: msgspec.Struct = operators.Empty](msgspec.Struct, frozen=True, kw_only=True):
    """Section per collection item, item as holder and store host, each item storing its open state in its `open` property."""

    key: Key
    title: str
    collection: Member
    open: str
    description: str = ""
    rows: tuple[RowContent, ...] = ()
    header: tuple[RowContent, ...] = ()
    enable: Option[Member] = Nothing
    store: Option[Store[T]] = Nothing
    empty: tuple[RowContent, ...] = ()
    condition: Option[Condition] = Nothing
    opened: bool = False

    @property
    def popovers(self) -> tuple[Single[operators.Empty], ...]:
        """Values popover of store."""
        return tuple(popover(store) for store in self.store.to_list())

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires registrable rows and store rows."""
        return Ok((*acquired((*self.rows, *self.header, *self.empty)), *(row for store in self.store.to_list() for row in (store.attachment, store))))

    def realized(self, identity: Identity) -> BlenderResult[Drawn]:
        """Realizes layout panel per item, store bound to item type."""

        def items(
            bound: ResolvedMember[bpy.types.CollectionProperty],
        ) -> BlenderResult[tuple[ResolvedMember[bpy.types.CollectionProperty], bpy.types.StringProperty, ResolvedMember, Option[Slots[T]]]]:
            match bound.descriptor.prop.fixed_type:
                case None:
                    return Error((BlenderAbsentFault(owner=bound.descriptor.path, key="fixed_type"),))
                case item:
                    opened = Member(root=type(item), path=self.open).resolved(bpy.types.BoolProperty)
                    stored = self.store.map(
                        lambda store: (
                            store.bound(identity) if isinstance(item, store.attachment.owner) else Error(refused(bound.descriptor.path, type(item).__name__, (store.attachment.owner.__name__,)))
                        ).map(Some)
                    ).default_value(Ok(Nothing))
                    return paired(
                        Option.of_optional(item.bl_rna.name_property).to_result_with(lambda: (BlenderAbsentFault(owner=bound.descriptor.path, key="name_property"),)),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                        paired(opened, stored),
                    ).map(lambda pair: (bound, pair[0], *pair[1]))

        def drawn(parts: tuple[tuple[ResolvedMember, bpy.types.StringProperty, ResolvedMember, Option[Slots[T]]], tuple[_Frame[T], tuple[RowDraw, ...]]]) -> Drawn:
            ((collection, named, opened, _), (frame, empties)) = parts

            def section(layout: bpy.types.UILayout, item: Host, context: bpy.types.Context) -> None:
                idname = f"{frame.name}_{keyed(item)}"
                active = all(holds(required, context, Some(item)) for required in self.condition.to_list())
                for place in opened.at(item, context).to_list():
                    for shown in frame.framed(layout, idname, getattr(item, named.identifier), item, Some(item), context, opened=place, active=active).to_list():
                        frame.body(shown, item, Some(item), context, idname)

            def listed(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for owner, key in collection.at(holder, context).to_list():
                    members = getattr(owner, key)
                    for draw in () if members else empties:
                        draw(layout, holder, context)
                    for item in members:
                        section(layout, item, context)

            return Drawn(head=Nothing, tail=Nothing, body=lambda layout, context: listed(layout, context, context), inline=listed)

        return (
            self.collection
            .resolved(bpy.types.CollectionProperty)
            .bind(items)
            .bind(lambda found: paired(_Frame.of(identity, self.key, self.rows, self.header, self.enable, found[3]), placed(self.empty, identity)).map(lambda pair: (found, pair)))
            .map(drawn)
        )

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns item sections as one row."""
        return self.realized(identity).map(lambda drawn: drawn.inline)


# --- [OPERATIONS] -----------------------------------------------------------------------


def popover[T: msgspec.Struct](store: Store[T]) -> Single[operators.Empty]:
    """Values popover section of store that `surfaces.panels` registers."""
    return Single(
        key=store.panel,
        title="Values",
        description="Apply presets, lock fields, copy values, and restore earlier values" if store.presets.is_some() else "Copy values and restore earlier values",
        rows=(Values(store=store),),
    )


def nesting(shape: Shape) -> TypeIs[Nesting]:
    """Whether a field shape draws as nested panels."""
    return isinstance(shape, Gate | Nested | Entries | Tagged | Records)


def grouped(struct: "type[bpy.types.bpy_struct[object]]", name: str) -> type[bpy.types.PropertyGroup]:
    """Property group class a pointer or collection member of a registered group holds."""
    kind: type[bpy.types.PropertyGroup] = type(struct.bl_rna.properties[name].fixed_type)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return kind


def fields[T: msgspec.Struct, S: "bpy.types.bpy_struct[object]"](
    group: RecordGroup[T, S], identity: Identity, replacements: Mapping[MemberDescriptorType, Replacement] = MappingProxyType({})
) -> BlenderResult[tuple[tuple[RowContent, ...], Nest]]:
    """Realizes fields as scalar rows and nested record, union, or collection panels, nested group types read from RNA."""
    supplied = {slot.__name__: row for slot, row in replacements.items() if issubclass(typing.get_origin(group.record) or group.record, slot.__objclass__)}

    def stacked(found: tuple[tuple[RowContent, ...], Nest]) -> BlenderResult[Nest]:
        rows, nest = found

        def draws(drawn: tuple[RowDraw, ...]) -> Nest:
            def draw(layout: bpy.types.UILayout, held: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
                for each in drawn:
                    each(layout, held, context)
                nest(layout, held, context, prefix)

            return draw

        return placed(rows, identity).map(draws)

    def nested(record: type[msgspec.Struct], native: type[bpy.types.PropertyGroup]) -> BlenderResult[Nest]:
        return fields(RecordGroup(record=record, struct=native), identity, replacements).bind(stacked)

    def paneled(name: str, nest: Nest, *, gated: bool) -> Nest:
        def draw(layout: bpy.types.UILayout, group: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
            held, idname = getattr(group, name), f"{prefix}_{name}"
            header, body = layout.panel(idname, default_closed=True)
            header.use_property_split = False
            if gated:
                header.prop(held, ENABLED, text="")
            header.label(text=group.bl_rna.properties[name].name)
            for shown in Option.of_optional(body).to_list():
                shown.active = bool(getattr(held, ENABLED)) if gated else True
                nest(shown, held if gated else group, context, idname)

        return draw

    def reached(name: str, nest: Nest) -> Nest:
        return lambda layout, owner, context, prefix: nest(layout, getattr(owner, name), context, prefix)

    def chosen(name: str, cases: tuple[tuple[str, Nest], ...], tag: str) -> Nest:
        by = dict(cases)

        def draw(layout: bpy.types.UILayout, owner: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
            group = getattr(owner, name)
            layout.prop(group, tag)
            picked = getattr(group, tag)
            by[picked](layout, getattr(group, picked), context, f"{prefix}_{picked}")

        return draw

    def each(name: str, nest: Nest) -> Nest:
        def draw(layout: bpy.types.UILayout, owner: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
            for item in getattr(owner, name):
                header, body = layout.panel_prop(item, EXPANDED)
                header.label(text=item.name)
                for shown in Option.of_optional(body).to_list():
                    nest(shown, item, context, f"{prefix}_{keyed(item)}")

        return draw

    def split(row: msgspec.structs.FieldInfo, shape: Shape, struct: "type[bpy.types.bpy_struct[object]]", *, panel: bool = True) -> BlenderResult[Property | Nest]:
        member = Member(root=struct, path=row.name, shape=Some(shape))
        if not nesting(shape):
            return Ok(Property(target=member))
        native = grouped(struct, row.name)
        match shape:
            case Gate(value=Option(tag="some", some=(field, inner))) if nesting(inner.shape):
                return split(field, inner.shape, native, panel=False).bind(
                    lambda child: (
                        realized(child, identity).map(lambda draw: paneled(row.name, lambda layout, held, context, _prefix: draw(layout, held, context), gated=True))
                        if isinstance(child, Property)
                        else Ok(paneled(row.name, child, gated=True))
                    )
                )
            case Gate():
                return Ok(Property(target=member))
            case Nested(record=record):
                found = nested(record, native).map(lambda nest: reached(row.name, nest))
            case Entries(gate=gate):
                found = (
                    accumulated(split(entry, gate, native).map(lambda part: (entry, part)) for entry in RecordGroup.entries(row, shape))
                    .map(lambda parts: joined(parts, {}))
                    .bind(stacked)
                    .map(lambda nest: reached(row.name, nest))
                )
            case Tagged(tag_field=tag, cases=cases):
                found = accumulated(nested(case, grouped(native, case.__name__)).map(lambda nest: (case.__name__, nest)) for case, _ in cases).map(lambda parts: chosen(row.name, parts, tag))
            case Records(record=record):
                found = nested(record, native).map(lambda nest: each(row.name, nest))
        return found.map(lambda nest: paneled(row.name, nest, gated=False) if panel else nest)

    def joined(parts: tuple[tuple[msgspec.structs.FieldInfo, Property | Nest], ...], supplied: Mapping[str, Replacement]) -> tuple[tuple[RowContent, ...], Nest]:
        nests = tuple(part for _, part in parts if not isinstance(part, Property))

        def draw(layout: bpy.types.UILayout, held: "bpy.types.bpy_struct[object]", context: bpy.types.Context, prefix: str) -> None:
            for nest in nests:
                nest(layout, held, context, prefix)

        rows = tuple(supplied[row.name](part.target) if row.name in supplied and isinstance(part.target, Member) else part for row, part in parts if isinstance(part, Property))
        return rows, draw

    return field_shapes(group.record).bind(
        lambda rows: accumulated(split(row, shape.shape, group.struct).map(lambda part: (row, part)) for row, shape in rows if row.name in group.struct.bl_rna.properties).map(
            lambda parts: joined(parts, supplied)
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Drawn", "Each", "Nest", "Nesting", "Section", "Single", "fields", "grouped", "nesting", "popover"]
