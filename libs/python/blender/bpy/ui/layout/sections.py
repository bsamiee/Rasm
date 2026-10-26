"""Header sections over one holder or each collection item, with form group fields and store values popovers."""

from collections.abc import Callable, Mapping
from functools import partial
from itertools import product, starmap
from types import MappingProxyType
from typing import Protocol, runtime_checkable, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands import operators
from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import root_in, Struct
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import Host
from blender.bpy.ui.layout.lists import ItemList, keyed
from blender.bpy.ui.layout.rows import acquired, At, Member, placed, Property, realized, refused, ResolvedMember, Row, RowContent, RowDraw, SurfaceDraw
from blender.bpy.ui.layout.values import Hosted, Replacement, Slots, Store, Values
from blender.records.faults import accumulated, Fault, Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Nest = Callable[[bpy.types.UILayout, bpy.types.bpy_struct[object], bpy.types.Context, str], None]
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

    def realized(self, identity: Identity) -> Outcome["SectionDraws"]:
        """Realizes section into panel and layout panel draws."""


# --- [MODELS] ---------------------------------------------------------------------------


class SectionDraws(msgspec.Struct, frozen=True, kw_only=True):
    """Section draws for panel header, body, and layout panel, and whether a context holds its holder."""

    head: Option[SurfaceDraw]
    tail: Option[SurfaceDraw]
    body: SurfaceDraw
    inline: RowDraw
    present: Callable[[bpy.types.Context], bool] = lambda _context: True


class _Frame[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Realized section parts."""

    name: str
    gate: Option[tuple[ResolvedMember, RowDraw]]
    header: tuple[RowDraw, ...]
    rows: tuple[RowDraw, ...]
    fields: tuple[RowDraw, ...]
    nest: Option[Nest]
    slots: Option[Slots[T]]

    @classmethod
    def of(cls, identity: Identity, key: Key, rows: tuple[RowContent, ...], header: tuple[RowContent, ...], enable: Option[Member], slots: Option[Slots[T]]) -> Outcome[Self]:
        """Realizes section parts at acquisition, rows and store scalars on one grid."""
        walk = slots.map(lambda held: fields(held.attached.form, identity, held.fields).map(Some)).default_value(Ok(Nothing))
        gate = enable.map(
            lambda member: member.resolved(bpy.types.BoolProperty).bind(lambda held: realized(Property(target=member, text=""), identity).map(lambda draw: Some((held, draw))))
        ).default_value(Ok(Nothing))

        def built(parts: tuple[tuple[str, Option[tuple[ResolvedMember, RowDraw]]], tuple[tuple[RowDraw, ...], Option[tuple[tuple[RowContent, ...], Nest]]]]) -> Outcome[Self]:
            (name, gated), (heads, walked) = parts
            scalars = walked.map(lambda nested: nested[0]).default_value(())
            return placed((*rows, *scalars), identity).map(
                lambda draws: cls(name=name, gate=gated, header=heads, rows=draws[: len(rows)], fields=draws[len(rows) :], nest=walked.map(lambda nested: nested[1]), slots=slots)
            )

        return paired(paired(identity.name(Name.ATTACHMENT, key), gate), paired(accumulated(realized(row, identity) for row in header), walk)).bind(built)

    def head(self, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws textless header enable."""
        for _, draw in self.gate.to_list():
            draw(layout, holder, context)

    def tail(self, layout: bpy.types.UILayout, holder: Struct, host: Option[Host], context: bpy.types.Context) -> None:
        """Draws header rows, then flat store slots with host as context pointer."""
        line = layout.row(align=True)
        for draw in self.header:
            draw(line, holder, context)
        for slots, found in product(self.slots.to_list(), host.to_list()):
            line.emboss = "NONE"
            line.context_pointer_set(slots.attached.name, found)
            for draw in slots.tail:
                draw(line, holder, context)

    def body(self, layout: bpy.types.UILayout, holder: Struct, host: Option[Host], context: bpy.types.Context, prefix: str) -> None:
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
        self, layout: bpy.types.UILayout, idname: str, title: str, holder: Struct, host: Option[Host], context: bpy.types.Context, *, opened: At | bool, active: bool = True
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


class Single[T: bpy.types.PropertyGroup = operators.Empty](msgspec.Struct, frozen=True, kw_only=True):
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

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Acquires registrable rows, holding list, and store rows."""
        return Ok((
            *acquired((*self.rows, *self.header, *(held for held in self.holder.to_list() if isinstance(held, ItemList)))),
            *(row for hosted in self.store.to_list() for row in (hosted.store.attachment, hosted.store)),
        ))

    def realized(self, identity: Identity) -> Outcome[SectionDraws]:
        """Realizes section into panel and layout panel draws, the store host read from its member or the context root of the store's owner."""

        def followed(bound: ResolvedMember, holder: Struct, context: bpy.types.Context) -> Option[Struct]:
            return bound.at(holder, context).bind(lambda pair: Option.of_optional(getattr(*pair)))

        def hosting(hosted: Hosted[T]) -> Outcome[tuple[Hosting, Slots[T]]]:
            owner = hosted.store.attachment.owner
            host: Outcome[Hosting] = hosted.host.map(
                lambda member: (
                    member
                    .resolved()
                    .bind(lambda bound: bound.accepts((owner,)))
                    .map(lambda bound: lambda holder, context: bound.at(holder, context).bind(lambda pair: Option.of_optional(getattr(*pair))))
                )
            ).default_value(Ok(lambda _holder, context: root_in(owner, context)))
            return paired(host, hosted.store.bound(identity))

        def reached(source: Member | ItemList) -> Outcome[Holding]:
            match source:
                case Member():
                    return source.resolved().map(lambda bound: partial(followed, bound))
                case ItemList():
                    return source.picked(identity).map(lambda reach: lambda _holder, context: reach(context))

        holding = self.holder.map(reached).default_value(Ok(lambda holder, _context: Some(holder)))

        def drawn(parts: tuple[tuple[_Frame[T], Option[Hosting]], Holding]) -> SectionDraws:
            (frame, host), held = parts

            def found(holder: Struct, context: bpy.types.Context) -> Option[Host]:
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

            def inline(layout: bpy.types.UILayout, outer: Struct, context: bpy.types.Context) -> None:
                for holder in held(outer, context).to_list():
                    active = all(holds(required, context, Some(holder)) for required in self.condition.to_list())
                    host = found(holder, context)
                    for shown in frame.framed(layout, frame.name, self.title, holder, host, context, opened=self.opened, active=active).to_list():
                        frame.body(shown, holder, host, context, frame.name)

            return SectionDraws(
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

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns section as layout panel row."""
        return self.realized(identity).map(lambda drawn: drawn.inline)


class Each[T: bpy.types.PropertyGroup = operators.Empty](msgspec.Struct, frozen=True, kw_only=True):
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

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Acquires registrable rows and store rows."""
        return Ok((*acquired((*self.rows, *self.header, *self.empty)), *(row for store in self.store.to_list() for row in (store.attachment, store))))

    def realized(self, identity: Identity) -> Outcome[SectionDraws]:
        """Realizes layout panel per item, store bound to item type."""

        def items(bound: ResolvedMember[bpy.types.CollectionProperty]) -> Outcome[tuple[ResolvedMember[bpy.types.CollectionProperty], bpy.types.StringProperty, ResolvedMember, Option[Slots[T]]]]:
            match bound.descriptor.prop.fixed_type:
                case None:
                    return Error((Fault(reason=BlenderReason.ABSENT, subject=bound.descriptor.path, member="fixed_type"),))
                case item:
                    opened = Member(root=type(item), path=self.open).resolved(bpy.types.BoolProperty)
                    stored = self.store.map(
                        lambda store: (
                            store.bound(identity) if isinstance(item, store.attachment.owner) else Error(refused(bound.descriptor.path, type(item).__name__, (store.attachment.owner.__name__,)))
                        ).map(Some)
                    ).default_value(Ok(Nothing))
                    return paired(
                        Option.of_optional(item.bl_rna.name_property).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=bound.descriptor.path, member="name_property"),)),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                        paired(opened, stored),
                    ).map(lambda pair: (bound, pair[0], *pair[1]))

        def drawn(parts: tuple[tuple[ResolvedMember, bpy.types.StringProperty, ResolvedMember, Option[Slots[T]]], tuple[_Frame[T], tuple[RowDraw, ...]]]) -> SectionDraws:
            ((collection, named, opened, _), (frame, empties)) = parts

            def section(layout: bpy.types.UILayout, item: Host, context: bpy.types.Context) -> None:
                idname = f"{frame.name}_{keyed(item)}"
                active = all(holds(required, context, Some(item)) for required in self.condition.to_list())
                for place in opened.at(item, context).to_list():
                    for shown in frame.framed(layout, idname, getattr(item, named.identifier), item, Some(item), context, opened=place, active=active).to_list():
                        frame.body(shown, item, Some(item), context, idname)

            def listed(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
                for owner, key in collection.at(holder, context).to_list():
                    members = getattr(owner, key)
                    for draw in () if members else empties:
                        draw(layout, holder, context)
                    for item in members:
                        section(layout, item, context)

            return SectionDraws(head=Nothing, tail=Nothing, body=lambda layout, context: listed(layout, context, context), inline=listed)

        return (
            self.collection
            .resolved(bpy.types.CollectionProperty)
            .bind(items)
            .bind(lambda found: paired(_Frame.of(identity, self.key, self.rows, self.header, self.enable, found[3]), placed(self.empty, identity)).map(lambda pair: (found, pair)))
            .map(drawn)
        )

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns item sections as one row."""
        return self.realized(identity).map(lambda drawn: drawn.inline)


# --- [OPERATIONS] -----------------------------------------------------------------------


def popover[T: bpy.types.PropertyGroup](store: Store[T]) -> Single[operators.Empty]:
    """Values popover section of store that `surfaces.panels` registers."""
    return Single(
        key=store.panel,
        title="Values",
        description="Apply presets, lock fields, copy values, and restore earlier values" if store.presets.is_some() else "Copy values and restore earlier values",
        rows=(Values(store=store),),
    )


def fields(struct: type[Struct], identity: Identity, replacements: Mapping[str, Replacement] = MappingProxyType({})) -> Outcome[tuple[tuple[RowContent, ...], Nest]]:
    """Realizes a registered group's properties as scalar rows, its group pointers as panels, and its group collections as a panel per item."""
    inherited = bpy.types.PropertyGroup.bl_rna.properties

    def drawn(drawn: tuple[RowDraw, ...], nest: Nest, layout: bpy.types.UILayout, held: Struct, context: bpy.types.Context, prefix: str) -> None:
        for each in drawn:
            each(layout, held, context)
        nest(layout, held, context, prefix)

    def stacked(found: tuple[tuple[RowContent, ...], Nest]) -> Outcome[Nest]:
        rows, nest = found
        return placed(rows, identity).map(lambda draws: partial(drawn, draws, nest))

    def paneled(prop: bpy.types.Property, nest: Nest, layout: bpy.types.UILayout, owner: Struct, context: bpy.types.Context, prefix: str, *, items: bool) -> None:
        idname, held = f"{prefix}_{prop.identifier}", getattr(owner, prop.identifier)
        header, body = layout.panel(idname, default_closed=True)
        header.label(text=prop.name)
        for shown in Option.of_optional(body).to_list():
            if not items:
                nest(shown, held, context, idname)
            for item in held if items else ():
                inner, opened = shown.panel(f"{idname}_{keyed(item)}", default_closed=True)
                inner.label(text=item.name)
                for content in Option.of_optional(opened).to_list():
                    nest(content, item, context, f"{idname}_{keyed(item)}")

    def split(prop: bpy.types.Property) -> Outcome[tuple[tuple[RowContent, ...], tuple[Nest, ...]]]:
        match prop:
            case bpy.types.PointerProperty(fixed_type=bpy.types.PropertyGroup() as group):
                return fields(type(group), identity, replacements).bind(stacked).map(lambda nest: ((), (partial(paneled, prop, nest, items=False),)))  # type: ignore[unreachable]
            case bpy.types.CollectionProperty(fixed_type=bpy.types.PropertyGroup() as item):
                return fields(type(item), identity, replacements).bind(stacked).map(lambda nest: ((), (partial(paneled, prop, nest, items=True),)))  # type: ignore[unreachable]
            case _:
                member = Member(root=struct, path=prop.identifier)
                return Ok(((replacements[prop.identifier](member) if prop.identifier in replacements else Property(target=member),), ()))

    def joined(parts: tuple[tuple[tuple[RowContent, ...], tuple[Nest, ...]], ...]) -> tuple[tuple[RowContent, ...], Nest]:
        nests = tuple(nest for _, held in parts for nest in held)

        def draw(layout: bpy.types.UILayout, held: Struct, context: bpy.types.Context, prefix: str) -> None:
            for nest in nests:
                nest(layout, held, context, prefix)

        return tuple(row for rows, _ in parts for row in rows), draw

    return accumulated(split(prop) for prop in struct.bl_rna.properties if prop.identifier not in inherited and not prop.is_hidden).map(joined)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Each", "Nest", "Section", "SectionDraws", "Single", "fields", "popover"]
