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
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.mirrors import annotated
from blender.bpy.data.rna.paths import definition, root_in, Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.settings.attachments import Attached, Host
from blender.bpy.ui.layout.lists import context_path, ItemList
from blender.bpy.ui.layout.members import Member, ResolvedMember
from blender.bpy.ui.layout.presets import Verb
from blender.bpy.ui.layout.rows import acquired, Container, Opens, OperatorButton, painted, Property, realized, RowContent, RowDraw, SurfaceDraw
from blender.bpy.ui.layout.values import Replacement, Store
from blender.records.faults import Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Holding = Member | ItemList[Any, Any] | Items
type _Reach = Callable[[Struct, bpy.types.Context], tuple[Struct, ...]]
type _Hosting = Callable[[Struct, bpy.types.Context], Option[Host]]
type _Title = Callable[[bpy.types.UILayout], None]
type _Opening = Callable[[bpy.types.UILayout, str, Struct, int], tuple[tuple[bpy.types.UILayout, bpy.types.UILayout | None], _Title]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_DEFAULT_CONTEXT: Final[str] = bpy.app.translations.contexts.default  # type: ignore[attr-defined]

# --- [MODELS] ---------------------------------------------------------------------------


class Items(msgspec.Struct, frozen=True, kw_only=True):
    """Collection whose items each draw the section under their name, open state in the item boolean `expanded` names, else per item in the region."""

    collection: Member
    expanded: Option[str] = Nothing


class SectionDraws(msgspec.Struct, frozen=True, kw_only=True):
    """Realized section that a panel and a layout panel both draw: layout panel name, title, header boolean and rows, body, holder reach, and the opener of each collection item."""

    name: str
    title: _Title
    closed: bool
    gate: Option[RowDraw]
    header: Option[RowDraw]
    rows: RowDraw
    reach: _Reach
    opening: Option[_Opening]
    absent: Option[RowDraw]
    condition: Option[Condition]

    @property
    def head(self) -> Option[SurfaceDraw]:
        """Panel header boolean over the holder the context reaches, none for a collection section."""
        return self.gate.filter(lambda _: self.opening.is_none()).map(lambda draw: partial(_over, self, draw))

    @property
    def tail(self) -> Option[SurfaceDraw]:
        """Panel header rows over the holder the context reaches, none for a collection section."""
        return self.header.filter(lambda _: self.opening.is_none()).map(lambda draw: partial(_over, self, draw))

    @property
    def body(self) -> SurfaceDraw:
        """Panel body over the holder the context reaches, every item's layout panel for a collection section."""
        return painted(partial(_inline, self)) if self.opening.is_some() else partial(_paneled, self)

    def present(self, context: bpy.types.Context) -> bool:
        """Whether the panel draws: absent rows declared or a holder reached."""
        return self.absent.is_some() or bool(self.reach(context, context))


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

    def realized(self, identity: Identity) -> Outcome[SectionDraws]:
        """Realizes the layout panel name, header boolean, header rows before the store buttons, body publishing the store host before the rows and drawing its fields after them, and holder reach once."""

        def holding() -> Outcome[tuple[_Reach, Option[_Opening]]]:
            match self.holder.to_optional():
                case None:
                    return Ok((lambda holder, _context: (holder,), Nothing))
                case Member() as member:
                    return member.resolved().map(lambda bound: (partial(_followed, bound), Nothing))
                case ItemList() as listed:
                    return listed.picked(identity).map(lambda pointer: (partial(_pointed, pointer), Nothing))
                case Items(collection=collection, expanded=expanded):
                    return collection.resolved(bpy.types.CollectionProperty).bind(lambda bound: itemized(expanded, bound))

        def itemized(expanded: Option[str], bound: ResolvedMember[bpy.types.CollectionProperty]) -> Outcome[tuple[_Reach, Option[_Opening]]]:
            fixed: Struct = bound.descriptor.prop.fixed_type  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            flag = expanded.map(lambda name: Descriptor.resolve(type(fixed), (name,), bpy.types.BoolProperty).map(lambda held: Some(held.prop.identifier))).default_value(Ok(Nothing))
            return flag.map(lambda found: (partial(_listed, bound), Some(partial(_opened_item, self, found))))

        def stored(store: Store[T]) -> Outcome[tuple[RowDraw, RowDraw, RowDraw]]:
            owner = store.attachment.owner
            hosting: Outcome[_Hosting] = store.host.map(lambda member: member.resolved().bind(lambda bound: bound.accepts((owner,))).map(lambda bound: partial(_hosted, bound, owner))).default_value(
                Ok(partial(_owned, owner))
            )
            buttons = (Opens(target=Opened(store.panel, Called.PANEL), keywords={"text": ""}, icon=Some("PRESET")), OperatorButton(target=store.commands[Verb.RESET], keywords={"text": ""}))
            return paired(store.attached(identity), hosting, realized(buttons, identity), realized(fields(store.attachment.form, store.attachment.key, store.fields), identity)).map(
                lambda parts: (partial(_buttons, parts[0], parts[1], parts[2]), partial(_pointer, parts[0], parts[1]), partial(_fielded, parts[0], parts[1], parts[3]))
            )

        def assembled(
            named: tuple[str, Option[tuple[ResolvedMember, RowDraw]], Option[tuple[RowDraw, RowDraw, RowDraw]]],
            header: RowDraw,
            rows: RowDraw,
            held: tuple[_Reach, Option[_Opening]],
            absent: Option[RowDraw],
        ) -> SectionDraws:
            (name, gated, kept), (reach, opening) = named, held
            trailing = (*((header,) if self.header else ()), *(parts[0] for parts in kept.to_list()))
            body = (*(parts[1] for parts in kept.to_list()), rows, *(parts[2] for parts in kept.to_list()))
            return SectionDraws(
                name=name,
                title=partial(_titled, self.title, self.translation_context, translate=True),
                closed=not self.opened,
                gate=gated.map(lambda pair: pair[1]),
                header=Some(partial(_tail, trailing)) if trailing else Nothing,
                rows=partial(_body, gated.map(lambda pair: pair[0]), body, split=self.split),
                reach=reach,
                opening=opening,
                absent=absent,
                condition=self.condition,
            )

        gate = self.enable.map(lambda member: paired(member.resolved(bpy.types.BoolProperty), realized((Property(target=member, keywords={"text": ""}),), identity)).map(Some)).default_value(
            Ok(Nothing)
        )
        store = self.store.map(lambda held: stored(held).map(Some)).default_value(Ok(Nothing))
        absent = realized(self.absent, identity).map(lambda draw: Some(draw) if self.absent else Nothing)
        return paired(paired(identity.identifier(self.key), gate, store), realized(self.header, identity), realized(self.rows, identity), holding(), absent).map(lambda parts: assembled(*parts))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the section as a layout panel per holder in a drawing layout, which the host refuses in menus, pies, and code-built popovers."""
        return self.realized(identity).map(lambda draws: partial(_inline, draws))


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


# --- [STORES]


def _hosted(bound: ResolvedMember, owner: type[Host], holder: Struct, context: bpy.types.Context) -> Option[Host]:
    """Host the store's host member names at the holder."""
    return bound.at(holder, context).bind(lambda at: Option.of_optional(try_downcast(owner, getattr(*at))))


def _owned(owner: type[Host], holder: Struct, context: bpy.types.Context) -> Option[Host]:
    """Holder itself when it is an owner of the store, else the context root of the owner type."""
    return Some(holder) if isinstance(holder, owner) else root_in(owner, context)


def _buttons[G: bpy.types.PropertyGroup](attached: Attached[G], hosting: _Hosting, buttons: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the unembossed store buttons under the host pointer."""
    for host in hosting(holder, context).to_list():
        layout.emboss = "NONE"
        layout.context_pointer_set(attached.name, host)
        buttons(layout, holder, context)


def _pointer[G: bpy.types.PropertyGroup](attached: Attached[G], hosting: _Hosting, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Publishes the store host for the commands the rows after it draw."""
    for host in hosting(holder, context).to_list():
        layout.context_pointer_set(attached.name, host)


def _fielded[G: bpy.types.PropertyGroup](attached: Attached[G], hosting: _Hosting, fielded: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the store's form fields over the hosted group."""
    for host in hosting(holder, context).to_list():
        fielded(layout, attached.struct(host), context)


# --- [HOLDERS]


def _followed(bound: ResolvedMember, holder: Struct, context: bpy.types.Context) -> tuple[Struct, ...]:
    """Struct the bound member points at from the holder."""
    return tuple(bound.at(holder, context).bind(lambda at: Option.of_optional(try_downcast(bpy.types.bpy_struct, getattr(*at)))).to_list())


def _pointed(pointer: Callable[[Struct, bpy.types.Context], Option[Struct]], holder: Struct, context: bpy.types.Context) -> tuple[Struct, ...]:
    """Item the list highlights from the holder."""
    return tuple(pointer(holder, context).to_list())


def _listed(bound: ResolvedMember[bpy.types.CollectionProperty], holder: Struct, context: bpy.types.Context) -> tuple[Struct, ...]:
    """Items of the collection at the holder."""
    return tuple(item for at in bound.at(holder, context).to_list() for item in getattr(*at))


# --- [OPENERS]


def _titled(text: str, translation: str, layout: bpy.types.UILayout, *, translate: bool) -> None:
    """Draws a layout panel title."""
    layout.label(text=text, text_ctxt=translation, translate=translate)


def _opened(title: _Title, layout: bpy.types.UILayout, name: str, _holder: Struct, _index: int, *, closed: bool) -> tuple[tuple[bpy.types.UILayout, bpy.types.UILayout | None], _Title]:
    """Opens the section's layout panel under its title, open state in the region."""
    return layout.panel(name, default_closed=closed), title


def _opened_item[G: bpy.types.PropertyGroup](
    section: Section[G], expanded: Option[str], layout: bpy.types.UILayout, name: str, item: Struct, index: int
) -> tuple[tuple[bpy.types.UILayout, bpy.types.UILayout | None], _Title]:
    """Opens an item's layout panel through its boolean or a region key per item, titled with the item's untranslated name or the section title."""
    opened = expanded.map(lambda flag: layout.panel_prop(item, flag)).default_with(
        lambda: layout.panel(f"{name}_{item.session_uid if isinstance(item, bpy.types.ID) else index}", default_closed=not section.opened)
    )
    named = Option.of_optional(definition(item).name_property).map(lambda prop: partial(_titled, getattr(item, prop.identifier), section.translation_context, translate=False))
    return opened, named.default_with(lambda: partial(_titled, section.title, section.translation_context, translate=True))


# --- [DRAWING]


def _tail(draws: tuple[RowDraw, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the header rows in one aligned row."""
    line = layout.row(align=True)
    for draw in draws:
        draw(line, holder, context)


def _body(gate: Option[ResolvedMember], draws: tuple[RowDraw, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context, *, split: bool) -> None:
    """Draws the body parts in order, dimmed while the header boolean is off."""
    layout.use_property_split = layout.use_property_decorate = split
    layout.active = layout.active and all(starmap(getattr, gate.bind(lambda bound: bound.at(holder, context)).to_list()))
    for draw in draws:
        draw(layout, holder, context)


def _dimmed(condition: Option[Condition], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Dims a layout while the section condition fails over the holder."""
    layout.active = layout.active and all(required.holds(context, Some(holder)) for required in condition.to_list())


def _inline(draws: SectionDraws, layout: bpy.types.UILayout, outer: Struct, context: bpy.types.Context) -> None:
    """Draws one layout panel per reached holder, absent rows while none is reached: header boolean, title, right-aligned header rows, then the open body, dimmed while the condition fails."""

    def framed(opened: tuple[bpy.types.UILayout, bpy.types.UILayout | None], title: _Title, holder: Struct) -> None:
        header, body = opened
        _dimmed(draws.condition, header, holder, context)
        for draw in draws.gate.to_list():
            draw(header, holder, context)
        title(header)
        right = header.row(align=True)
        right.alignment = "RIGHT"
        for draw in draws.header.to_list():
            draw(right, holder, context)
        for shown in Option.of_optional(body).to_list():
            _dimmed(draws.condition, shown, holder, context)
            draws.rows(shown, holder, context)

    found, opening = draws.reach(outer, context), draws.opening.default_value(partial(_opened, draws.title, closed=draws.closed))
    if not found:
        for absent in draws.absent.to_list():
            absent(layout, outer, context)
    for index, holder in enumerate(found):
        framed(*opening(layout, draws.name, holder, index), holder)


def _over(draws: SectionDraws, draw: RowDraw, layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
    """Draws a panel header part over the holder the context reaches, dimmed while the condition fails."""
    for holder in draws.reach(context, context):
        _dimmed(draws.condition, layout, holder, context)
        draw(layout, holder, context)


def _paneled(draws: SectionDraws, layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
    """Draws the panel body over the holder the context reaches, dimmed while the condition fails, absent rows while none is reached."""
    if not (found := draws.reach(context, context)):
        for absent in draws.absent.to_list():
            absent(layout, context, context)
    for holder in found:
        _dimmed(draws.condition, layout, holder, context)
        draws.rows(layout, holder, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Items", "Section", "SectionDraws", "fields", "reached"]
