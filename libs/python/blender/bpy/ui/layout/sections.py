"""Sections over the drawing holder, a bound member, the item a list highlights, or each collection item, with store values popovers and form fields."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial
from itertools import starmap
from types import MappingProxyType
from typing import Any, Final

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
import msgspec
import rna_prop_ui

from blender.bpy.commands import operators
from blender.bpy.commands.calls import Called, Opened
from blender.bpy.data.rna.conditions import admits, Condition
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.mirrors import annotated
from blender.bpy.data.rna.paths import context_path, definition, root_in, Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.settings.attachments import Attached, Host
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import acquired, Container, Member, Opens, OperatorButton, Property, realized, ResolvedMember, RowContent, RowDraw
from blender.bpy.ui.layout.values import Replacement, Store, Verb
from blender.records.faults import Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Holding = Member | ItemList[Any, Any] | Items
type _Reach = Callable[[Struct, bpy.types.Context], tuple[Struct, ...]]
type _Hosting = Callable[[Struct, bpy.types.Context], Option[Host]]
type _Stored = tuple[Attached[Any], _Hosting, RowDraw, RowDraw]

# --- [CONSTANTS] ------------------------------------------------------------------------

_DEFAULT_CONTEXT: Final[str] = bpy.app.translations.contexts.default  # type: ignore[attr-defined]

# --- [MODELS] ---------------------------------------------------------------------------


class Items(msgspec.Struct, frozen=True, kw_only=True):
    """Collection whose items each draw the section under their name, open state in the item boolean `expanded` names, else per item in the region."""

    collection: Member
    expanded: Option[str] = Nothing


class Section[T: bpy.types.PropertyGroup = operators.Empty](msgspec.Struct, frozen=True, kw_only=True):
    """Rows under a header over the holder `holder` reaches from the drawing holder, the drawing holder itself while unset, drawn as a panel or a layout panel.

    `enable` draws its boolean in the header and dims the body while off, `condition` dims header and body while it fails, and `absent` rows draw while no holder is reached.
    A store adds its values popover and reset buttons to the header, its form fields after the rows, and its host as the context pointer the store's commands read.
    `split` lays out properties with labels in their own column and the decorator column the host aligns.
    """

    key: Key
    title: str
    description: str = ""
    rows: tuple[RowContent, ...] = ()
    header: tuple[RowContent, ...] = ()
    enable: Option[Member] = Nothing
    store: Option[Store[T]] = Nothing
    condition: Option[Condition] = Nothing
    holder: Option[Holding] = Nothing
    absent: tuple[RowContent, ...] = ()
    opened: bool = False
    split: bool = True
    translation_context: str = _DEFAULT_CONTEXT

    @property
    def popover(self) -> Option["Section[operators.Empty]"]:
        """Values popover section of the store that a panel registers."""
        return self.store.map(
            lambda store: Section(
                key=store.panel,
                title="Values",
                description="Apply presets, lock fields, copy values, and restore earlier values" if store.presets.is_some() else "Copy values and restore earlier values",
                rows=(store,),
            )
        )

    @property
    def children(self) -> tuple[RowContent, ...]:
        """Rows, header rows, absent rows, store fields, and the list the holder reads."""
        return (
            *self.rows,
            *self.header,
            *self.absent,
            *(row for store in self.store.to_list() for row in fields(store.attachment.form, store.attachment.key, store.fields)),
            *(held for held in self.holder.to_list() if isinstance(held, ItemList)),
        )

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the store and every held row that registers what it holds."""
        return Ok(acquired((*self.store.to_list(), *self.children)))

    def realized(self, identity: Identity) -> "Outcome[SectionDraws]":
        """Realizes the layout panel name, header boolean, header rows, store parts, body rows, holder reach, and absent rows once."""

        def holding() -> Outcome[tuple[_Reach, bool, Option[str]]]:
            match self.holder.to_optional():
                case None:
                    return Ok((lambda holder, _context: (holder,), False, Nothing))
                case Member() as member:
                    return member.resolved().map(lambda bound: (partial(_reached, bound), False, Nothing))
                case ItemList() as listed:
                    return listed.picked(identity).map(lambda pointer: (pointer, False, Nothing))
                case Items(collection=collection, expanded=expanded):
                    return collection.resolved(bpy.types.CollectionProperty).bind(lambda bound: flagged(bound, expanded).map(lambda flag: (partial(_reached, bound), True, flag)))

        def flagged(bound: ResolvedMember[bpy.types.CollectionProperty], expanded: Option[str]) -> Outcome[Option[str]]:
            match bound.descriptor.prop.fixed_type, expanded.to_optional():
                case bpy.types.bpy_struct() as item, str(name):
                    return Descriptor.resolve(type(item), (name,), bpy.types.BoolProperty).map(lambda held: Some(held.prop.identifier))
                case _:
                    return Ok(Nothing)

        def stored(store: Store[T]) -> Outcome[_Stored]:
            owner = store.attachment.owner
            host = store.host.map(lambda member: member.resolved().bind(lambda bound: bound.accepts((owner,))).map(Some)).default_value(Ok(Nothing))
            buttons = (Opens(target=Opened(store.panel, Called.PANEL), keywords={"text": ""}, icon=Some("PRESET")), OperatorButton(target=store.commands[Verb.RESET], keywords={"text": ""}))
            return paired(store.attached(identity), host, realized(buttons, identity), realized(fields(store.attachment.form, store.attachment.key, store.fields), identity)).map(
                lambda parts: (parts[0], partial(_host, parts[1], owner), parts[2], parts[3])
            )

        gate = self.enable.map(lambda member: paired(member.resolved(bpy.types.BoolProperty), realized((Property(target=member, keywords={"text": ""}),), identity)).map(Some)).default_value(
            Ok(Nothing)
        )
        store = self.store.map(lambda held: stored(held).map(Some)).default_value(Ok(Nothing))
        parts = paired(realized(self.header, identity), realized(self.rows, identity), realized(self.absent, identity))
        return paired(identity.identifier(self.key), gate, store, parts, holding()).map(
            lambda found: SectionDraws(
                section=self,
                name=found[0],
                gate=found[1],
                store=found[2],
                header=Some(found[3][0]).filter(lambda _: bool(self.header)),
                rows=found[3][1],
                absent=Some(found[3][2]).filter(lambda _: bool(self.absent)),
                reach=found[4][0],
                itemized=found[4][1],
                expanded=found[4][2],
            )
        )

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the section as a layout panel per holder in a drawing layout, which the host refuses in menus, pies, and code-built popovers."""
        return self.realized(identity).map(lambda draws: draws.shown)


class SectionDraws(msgspec.Struct, frozen=True, kw_only=True):
    """Realized section that a panel and a layout panel both draw: layout panel name, header boolean, header rows, store parts, body rows, holder reach, and the per-item panels of a collection section."""

    section: Section[Any]
    name: str
    gate: Option[tuple[ResolvedMember, RowDraw]]
    store: Option[_Stored]
    header: Option[RowDraw]
    rows: RowDraw
    absent: Option[RowDraw]
    reach: _Reach
    itemized: bool
    expanded: Option[str]

    # --- [PANEL]

    @property
    def head(self) -> Option[RowDraw]:
        """Panel header boolean over the holder the context reaches, none for a collection section."""
        return self.gate.filter(lambda _: not self.itemized).map(lambda gate: partial(self.over, gate[1]))

    @property
    def tail(self) -> Option[RowDraw]:
        """Panel header rows and store buttons over the holder the context reaches, none for a collection section."""
        return Some(partial(self.over, self.trailed)).filter(lambda _: not self.itemized and (self.header.is_some() or self.store.is_some()))

    @property
    def body(self) -> RowDraw:
        """Panel body over the holder the context reaches, every item's layout panel for a collection section."""
        return self.shown if self.itemized else self.paneled

    def present(self, context: bpy.types.Context) -> bool:
        """Whether the panel draws: absent rows declared or a holder reached."""
        return self.absent.is_some() or bool(self.reach(context, context))

    def over(self, draw: RowDraw, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws a panel header part over the holder the context reaches, dimmed while the condition fails."""
        for holder in self.reach(context, context):
            self.dimmed(layout, holder, context)
            draw(layout, holder, context)

    def paneled(self, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the panel body over the holder the context reaches, dimmed while the condition fails, absent rows while none is reached."""
        for holder in self.reached(layout, context, context):
            self.dimmed(layout, holder, context)
            self.inner(layout, holder, context)

    # --- [LAYOUT_PANEL]

    def shown(self, layout: bpy.types.UILayout, outer: Struct, context: bpy.types.Context) -> None:
        """Draws one layout panel per reached holder, absent rows while none is reached: header boolean, title, right-aligned header rows, then the open body, dimmed while the condition fails."""
        for index, holder in enumerate(self.reached(layout, outer, context)):
            (header, body), title, translate = self.opened(layout, holder, index)
            self.dimmed(header, holder, context)
            for _, gate in self.gate.to_list():
                gate(header, holder, context)
            header.label(text=title, text_ctxt=self.section.translation_context, translate=translate)
            right = header.row(align=True)
            right.alignment = "RIGHT"
            self.trailed(right, holder, context)
            for opened in Option.of_optional(body).to_list():
                self.dimmed(opened, holder, context)
                self.inner(opened, holder, context)

    def opened(self, layout: bpy.types.UILayout, holder: Struct, index: int) -> tuple[tuple[bpy.types.UILayout, bpy.types.UILayout | None], str, bool]:
        """Opens a holder's layout panel with its title and whether it translates: the section's panel, or an item's through its boolean or a region key per item under its untranslated name."""
        section = self.section
        if not self.itemized:
            return layout.panel(self.name, default_closed=not section.opened), section.title, True
        panel = self.expanded.map(lambda flag: layout.panel_prop(holder, flag)).default_with(
            lambda: layout.panel(f"{self.name}_{holder.session_uid if isinstance(holder, bpy.types.ID) else index}", default_closed=not section.opened)
        )
        named = Option.of_optional(definition(holder).name_property).map(lambda prop: (str(getattr(holder, prop.identifier)), False))
        return (panel, *named.default_value((section.title, True)))

    # --- [PARTS]

    def reached(self, layout: bpy.types.UILayout, outer: Struct, context: bpy.types.Context) -> tuple[Struct, ...]:
        """Holders the outer holder reaches, drawing the absent rows while it reaches none."""
        if not (found := self.reach(outer, context)):
            for absent in self.absent.to_list():
                absent(layout, outer, context)
        return found

    def trailed(self, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the header rows, then the unembossed store buttons under the host pointer, in one aligned row."""
        if self.header.is_none() and self.store.is_none():
            return
        line = layout.row(align=True)
        for draw in self.header.to_list():
            draw(line, holder, context)
        for attached, hosting, buttons, _ in self.store.to_list():
            for host in hosting(holder, context).to_list():
                line.emboss = "NONE"
                line.context_pointer_set(attached.name, host)
                buttons(line, holder, context)

    def inner(self, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the body dimmed while the header boolean is off: the store host as context pointer, the rows, then the store fields over the hosted group."""
        layout.use_property_split = layout.use_property_decorate = self.section.split
        layout.active = layout.active and all(starmap(getattr, self.gate.bind(lambda gate: gate[0].at(holder, context)).to_list()))
        hosts = tuple((attached, host, fielded) for attached, hosting, _, fielded in self.store.to_list() for host in hosting(holder, context).to_list())
        for attached, host, _ in hosts:
            layout.context_pointer_set(attached.name, host)
        self.rows(layout, holder, context)
        for attached, host, fielded in hosts:
            fielded(layout, attached.struct(host), context)

    def dimmed(self, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Dims a layout while the section condition fails over the holder."""
        layout.active = layout.active and admits(self.section.condition, context, Some(holder))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WALK]


def reached(rows: Iterable[object]) -> tuple[Section, ...]:
    """Sections the rows hold, each ahead of the sections its own rows hold, through every container."""
    return tuple(found for row in rows for found in (*((row,) if isinstance(row, Section) else ()), *reached(row.children if isinstance(row, Container) else ())))


# --- [FIELDS]


def fields(struct: type[Struct], key: Key, replacements: Mapping[str, Replacement] = MappingProxyType({})) -> tuple[RowContent, ...]:
    """Rows over the properties a struct class declares, hidden ones left out, and the custom properties block of host data.

    Scalars draw as property rows or their replacement, group pointers as a section over the group titled with the name its property registers, the identifier when none is stated,
    and group collections as a list with a section over the item it highlights, nested keys extending `key`.

    Args:
        struct: Struct class whose declared properties the rows draw.
        key: Key the nested section and list keys extend.
        replacements: Row per member name drawn in place of its property row.

    Returns:
        The field rows in declaration order, then the custom properties section for ID data.
    """

    def field(name: str, deferred: bpy.props._PropertyDeferred) -> tuple[RowContent, ...]:
        member, nested, target = Member(root=struct, path=(name,)), f"{key}_{name}", deferred.keywords.get("type")
        title = str(deferred.keywords.get("name", name))
        translation = str(deferred.keywords.get("translation_context", _DEFAULT_CONTEXT))
        match deferred.function:
            case _ if name in replacements:
                return (replacements[name](member),)
            case bpy.props.PointerProperty if isinstance(target, type) and issubclass(target, bpy.types.PropertyGroup):
                return (Section(key=nested, title=title, translation_context=translation, holder=Some(member), rows=fields(target, nested, replacements)),)
            case bpy.props.CollectionProperty if isinstance(target, type) and issubclass(target, bpy.types.PropertyGroup):
                listed = ItemList(key=nested, collection=member, item=target)
                return (listed, Section(key=f"{nested}_item", title=title, translation_context=translation, holder=Some(listed), rows=fields(target, f"{nested}_item", replacements)))
            case _:
                return (Property(target=member),)

    declared = tuple(row for name, deferred in annotated(struct).items() if "HIDDEN" not in deferred.keywords.get("options", ()) for row in field(name, deferred))
    custom = isinstance(struct, type(bpy.types.ID)) and not issubclass(struct, (bpy.types.PropertyGroup, bpy.types.AddonPreferences))
    return (*declared, *((Section(key=f"{key}_custom_properties", title=rna_prop_ui.PropertyPanel.bl_label, rows=(partial(_customized, struct),)),) if custom else ()))


def _customized(struct: type[Struct], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the holder's custom properties through the host's property block, over the context path that reaches it."""
    for member in context_path(holder).to_list():
        rna_prop_ui.draw(layout, context, member, struct)


# --- [HOLDERS]


def _reached[P: bpy.types.Property](bound: ResolvedMember[P], holder: Struct, context: bpy.types.Context) -> tuple[Struct, ...]:
    """Struct the bound pointer reaches from the holder, or each item of the bound collection."""
    found = bound.at(holder, context).map(lambda at: getattr(*at))
    if isinstance(bound.descriptor.prop, bpy.types.CollectionProperty):
        return tuple(item for items in found.to_list() for item in items)
    return tuple(found.bind(lambda value: Option.of_optional(try_downcast(bpy.types.bpy_struct, value))).to_list())


def _host[H: Host](bound: Option[ResolvedMember], owner: type[H], holder: Struct, context: bpy.types.Context) -> Option[H]:
    """Host the store's host member names at the holder, else the holder itself when it is an owner of the store, else the context root of the owner type."""
    return bound.map(lambda held: held.at(holder, context).bind(lambda at: Option.of_optional(try_downcast(owner, getattr(*at))))).default_with(
        lambda: Option.of_optional(try_downcast(owner, holder)).or_else_with(lambda: root_in(owner, context))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Items", "Section", "SectionDraws", "fields", "reached"]
