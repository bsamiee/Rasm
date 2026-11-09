"""Sections drawn as panels or layout panels over the holder they reach."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial
from types import MappingProxyType
from typing import Final, Generic, Never, TypeVar

import bpy
from expression import Option, Some, try_downcast
import msgspec
import rna_prop_ui

from rasm_bpy.commands.operators import PopupKind, PopupTarget
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import _acquired, Acquisitions
from rasm_bpy.data._rna.conditions import Condition, satisfied
from rasm_bpy.data._rna.paths import _context_path, _definition, root_in, Struct
from rasm_bpy.data._rna.properties import annotated
from rasm_bpy.settings._attachments import Attached, Host
from rasm_bpy.ui._layout.lists import _ItemList
from rasm_bpy.ui._layout.members import _owner_key, Member, ResolvedMember
from rasm_bpy.ui._layout.rows import _realized, Container, Opens, OperatorButton, Property, RowContent, RowDraw
from rasm_bpy.ui._layout.values import Replacement, Store, StoreCommand
from rasm_records import Nothing, Ok, Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Holding = Member | _ItemList[bpy.types.PropertyGroup, Never] | _Items
type _Holders = Callable[[Struct, bpy.types.Context], tuple[Struct, ...]]
type _Hosting = Callable[[Struct, bpy.types.Context], Option[Host]]
type _Stored = tuple[Attached[bpy.types.PropertyGroup], _Hosting, RowDraw, RowDraw]
_T_co = TypeVar("_T_co", bound=bpy.types.PropertyGroup, covariant=True, default=bpy.types.PropertyGroup)

# --- [CONSTANTS] ------------------------------------------------------------------------

_DEFAULT_CONTEXT: Final[str] = bpy.app.translations.contexts.default  # ty: ignore[unsound-assignment]

# --- [MODELS] ---------------------------------------------------------------------------


class _Items(msgspec.Struct, frozen=True, kw_only=True):
    """Collection whose items each draw the section under their name, open state in the item boolean `expanded` names, else per item in the region."""

    collection: Member
    expanded: Option[str] = Nothing


class Section(msgspec.Struct, Generic[_T_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Rows under a header over the holder `holder` reaches from the drawing holder, the drawing holder itself while unset, drawn as a panel or a layout panel.

    Header reads `title`, the key's display name while no title is stated.
    `enable` draws its boolean in the header and dims the body while off, `condition` dims header and body while it fails, and `absent` rows draw while no holder is reached.
    Store adds its values popover and reset buttons to the header, its form fields after the rows, and its host as the context pointer the store's commands read.
    `split` lays out properties with labels in their own column and the decorator column the host aligns.
    """

    key: Key
    title: Option[str] = Nothing
    description: str = ""
    rows: tuple[RowContent, ...] = ()
    header: tuple[RowContent, ...] = ()
    enable: Option[Member] = Nothing
    store: Option[Store[_T_co]] = Nothing
    condition: Option[Condition] = Nothing
    holder: Option[Holding] = Nothing
    absent: tuple[RowContent, ...] = ()
    opened: bool = False
    split: bool = True
    translation_context: str = _DEFAULT_CONTEXT

    @property
    def label(self) -> str:
        """Header text: the stated title, else the key's display name."""
        return self.title.default_with(lambda: bpy.path.display_name(self.key))

    @property
    def popover(self) -> Option["Section"]:
        """Values popover section of the store that a panel registers."""
        return self.store.map(
            lambda store: Section(
                key=store.panel,
                title=Some("Values"),
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
            *(row for store in self.store.to_list() for row in _fields(store.attachment.form, store.attachment.key, store.fields)),
            *(held for held in self.holder.to_list() if isinstance(held, _ItemList)),
        )

    def acquisitions(self, _identity: Identity, /) -> Acquisitions:
        """Acquires the attachments its holder, item collection, header boolean, and store host read, then the store and every held row that registers what it holds."""
        match self.holder.to_optional():
            case Member() as member:
                held: tuple[Member, ...] = (member,)
            case _Items(collection=collection):
                held = (collection,)
            case _:
                held = ()
        members = (*held, *self.enable.to_list(), *(member for store in self.store.to_list() for member in store.host.to_list()))
        return (*(declaration for member in members for declaration in member.prerequisites), *_acquired((*self.store.to_list(), *self.children)))

    def realized(self, identity: Identity) -> "Outcome[_SectionDraws]":
        """Realizes the layout panel name, header boolean, store parts, header, body, and absent rows, and the holder reach once."""

        def holding() -> Outcome[_Holders]:
            match self.holder.to_optional():
                case None:
                    return Ok(lambda holder, _context: (holder,))
                case Member() as member:
                    return member.realized(identity).map(lambda bound: partial(_held, bound))
                case _ItemList() as listed:
                    return listed.picked(identity)
                case _Items(collection=collection):
                    return collection.realized(identity, bpy.types.CollectionProperty).map(lambda bound: partial(_held, bound))

        def stored(store: Store[_T_co]) -> Outcome[_Stored]:
            owner = store.attachment.owner
            host = store.host.map(lambda member: member.realized(identity).map(Some)).default_value(Ok(Nothing))
            buttons = (
                Opens(target=PopupTarget(store.panel, PopupKind.PANEL), keywords={"text": ""}, icon=Some("PRESET")),
                OperatorButton(target=store.commands[StoreCommand.RESET], keywords={"text": ""}),
            )
            return paired(store.attachment.bound(identity), host, _realized(buttons, identity), _realized(_fields(store.attachment.form, store.attachment.key, store.fields), identity)).map(
                lambda parts: (parts[0], partial(_host, parts[1], owner), parts[2], parts[3])
            )

        enable = self.enable.map(
            lambda member: paired(member.realized(identity, bpy.types.BoolProperty), _realized((Property(target=member, keywords={"text": ""}),), identity)).map(Some)
        ).default_value(Ok(Nothing))
        store = self.store.map(lambda held: stored(held).map(Some)).default_value(Ok(Nothing))
        drawn = paired(_realized(self.header, identity), _realized(self.rows, identity), _realized(self.absent, identity))
        return paired(enable, store, drawn, holding()).map(
            lambda found: _SectionDraws(section=self, name=identity.identifier(self.key), enable=found[0], store=found[1], header=found[2][0], rows=found[2][1], absent=found[2][2], reach=found[3])
        )

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the section as a layout panel per holder in a drawing layout, which the host refuses in menus, pies, and code-built popovers."""
        return self.realized(identity).map(lambda draws: draws.shown)


class _SectionDraws(msgspec.Struct, frozen=True, kw_only=True):
    """Realized section that a panel and a layout panel both draw."""

    # --- [STATE]

    section: Section[bpy.types.PropertyGroup]
    name: str
    enable: Option[tuple[ResolvedMember, RowDraw]]
    store: Option[_Stored]
    header: RowDraw
    rows: RowDraw
    absent: RowDraw
    reach: _Holders

    @property
    def itemized(self) -> bool:
        """Whether the section draws one layout panel per collection item."""
        return isinstance(self.section.holder.to_optional(), _Items)

    # --- [PANELS]

    @property
    def draw_header(self) -> Option[RowDraw]:
        """Panel header boolean over the holder the context reaches, none for a collection section."""
        return self.enable.filter(lambda _: not self.itemized).starmap(lambda _, draw: partial(self.reached, draw))

    @property
    def draw_header_preset(self) -> Option[RowDraw]:
        """Panel header rows and store buttons over the holder the context reaches, none for a collection section."""
        return Some(partial(self.reached, self.header_preset)).filter(lambda _: not self.itemized and bool(self.section.header or self.store.is_some()))

    @property
    def draw(self) -> RowDraw:
        """Panel body over the holder the context reaches, every item's layout panel for a collection section."""
        return self.shown if self.itemized else self.panel

    def present(self, context: bpy.types.Context) -> bool:
        """Whether the panel draws: absent rows declared or a holder reached."""
        return bool(self.section.absent) or bool(self.reach(context, context))

    def reached(self, draw: RowDraw, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws a panel header part over the holder the context reaches, dimmed while the condition fails."""
        for holder in self.reach(context, context):
            self.dimmed(layout, holder, context)
            draw(layout, holder, context)

    def panel(self, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the panel body over the holder the context reaches, dimmed while the condition fails, absent rows while none is reached."""
        for holder in self.holders(layout, context, context):
            self.dimmed(layout, holder, context)
            self.body(layout, holder, context)

    # --- [LAYOUT_PANEL]

    def shown(self, layout: bpy.types.UILayout, outer: Struct, context: bpy.types.Context) -> None:
        """Draws one layout panel per reached holder, absent rows while none is reached, the header holding the boolean, title, and right-aligned header rows."""
        for holder in self.holders(layout, outer, context):
            (drawn, body), title, translate = self.opened(layout, holder)
            for header in Option.of_optional(drawn).to_list():
                self.dimmed(header, holder, context)
                for _, enable in self.enable.to_list():
                    enable(header, holder, context)
                header.label(text=title, text_ctxt=self.section.translation_context, translate=translate)
                right = header.row(align=True)
                right.alignment = "RIGHT"
                self.header_preset(right, holder, context)
            for opened in Option.of_optional(body).to_list():
                self.dimmed(opened, holder, context)
                self.body(opened, holder, context)

    def opened(self, layout: bpy.types.UILayout, holder: Struct) -> tuple[tuple[bpy.types.UILayout | None, bpy.types.UILayout | None], str, bool]:
        """Opens a holder's layout panel, returning its header and body layouts, title, and whether the title translates."""
        section = self.section
        match section.holder.to_optional():
            case _Items(expanded=expanded):
                panel = expanded.map(lambda flag: layout.panel_prop(holder, flag)).default_with(lambda: layout.panel(f"{self.name}_{_owner_key(holder)}", default_closed=not section.opened))
                named = Option.of_optional(_definition(holder).name_property).map(lambda prop: (str(getattr(holder, prop.identifier)), False))
                return (panel, *named.default_value((section.label, True)))
            case _:
                return layout.panel(self.name, default_closed=not section.opened), section.label, True

    # --- [PARTS]

    def holders(self, layout: bpy.types.UILayout, outer: Struct, context: bpy.types.Context) -> tuple[Struct, ...]:
        """Holders the outer holder reaches, drawing the absent rows while it reaches none."""
        if not (found := self.reach(outer, context)):
            self.absent(layout, outer, context)
        return found

    def header_preset(self, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the header rows, then the unembossed store buttons under the host pointer, in one aligned row."""
        if not self.section.header and self.store.is_none():
            return
        line = layout.row(align=True)
        self.header(line, holder, context)
        for attached, hosting, buttons, _ in self.store.to_list():
            for host in hosting(holder, context).to_list():
                line.emboss = "NONE"
                line.context_pointer_set(attached.name, host)
                buttons(line, holder, context)

    def body(self, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the body dimmed while the header boolean is off: the store host as context pointer, the rows, then the store fields over the hosted group."""
        layout.use_property_split = layout.use_property_decorate = self.section.split
        layout.active = layout.active and all(bound.descriptor.read(owner) for bound, _ in self.enable.to_list() for owner, _ in bound.at(holder, context).to_list())
        hosts = tuple((attached, host, fielded) for attached, hosting, _, fielded in self.store.to_list() for host in hosting(holder, context).to_list())
        for attached, host, _ in hosts:
            layout.context_pointer_set(attached.name, host)
        self.rows(layout, holder, context)
        for attached, host, fielded in hosts:
            fielded(layout, attached.struct(host), context)

    def dimmed(self, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Dims a layout while the section condition fails over the holder."""
        layout.active = layout.active and satisfied(self.section.condition, context, Some(holder))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WALKS]


def _sections_in(rows: Iterable[object]) -> tuple[Section, ...]:
    """Sections the rows hold, each ahead of the sections its own rows hold, through every container."""
    return tuple(found for row in rows for found in (*((row,) if isinstance(row, Section) else ()), *_sections_in(row.children if isinstance(row, Container) else ())))


# --- [FIELDS]


def _fields(struct: type[Struct], key: Key, replacements: Mapping[str, Replacement] = MappingProxyType({})) -> tuple[RowContent, ...]:
    """Rows over the properties a struct class declares, hidden ones left out, and the custom properties block of host data.

    Scalars draw as property rows or their replacement, group pointers as a section over the group titled with the name its property registers, the identifier when none is stated,
    and group collections as a list with a section over the item it highlights, nested keys extending `key`.

    Args:
        struct: Struct class whose declared properties the rows draw.
        key: Key the nested section and list keys extend.
        replacements: Row per member name drawn in place of its property row.

    Returns:
        Field rows in declaration order, then the custom properties section for a struct holding custom properties.
    """

    def field(name: str, deferred: bpy.props._PropertyDeferred) -> tuple[RowContent, ...]:
        member, nested, target = Member(root=struct, path=(name,)), f"{key}_{name}", deferred.keywords.get("type")
        title = str(deferred.keywords.get("name", name))
        translation = str(deferred.keywords.get("translation_context", _DEFAULT_CONTEXT))
        match deferred.function:
            case _ if name in replacements:
                return (replacements[name](member),)
            case bpy.props.PointerProperty if isinstance(target, type) and issubclass(target, bpy.types.PropertyGroup):
                return (Section(key=nested, title=Some(title), translation_context=translation, holder=Some(member), rows=_fields(target, nested, replacements)),)
            case bpy.props.CollectionProperty if isinstance(target, type) and issubclass(target, bpy.types.PropertyGroup):
                listed = _ItemList(key=nested, collection=member)
                return (listed, Section(key=f"{nested}_item", title=Some(title), translation_context=translation, holder=Some(listed), rows=_fields(target, f"{nested}_item", replacements)))
            case _:
                return (Property(target=member),)

    declared = tuple(row for name, deferred in annotated(struct).items() if "HIDDEN" not in deferred.keywords.get("options", ()) for row in field(name, deferred))
    custom = isinstance(struct, type(bpy.types.ID)) and not issubclass(struct, (bpy.types.PropertyGroup, bpy.types.AddonPreferences))
    return (*declared, *((Section(key=f"{key}_custom_properties", title=Some(rna_prop_ui.PropertyPanel.bl_label), rows=(partial(_customized, struct),)),) if custom else ()))


def _customized(struct: type[Struct], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the holder's custom properties through the host's property block, over the context path that reaches it."""
    for member in _context_path(holder).to_list():
        rna_prop_ui.draw(layout, context, member, struct)


# --- [HOLDERS]


def _held[P: bpy.types.Property](bound: ResolvedMember[P], holder: Struct, context: bpy.types.Context) -> tuple[Struct, ...]:
    """Struct the bound pointer reaches from the holder, or each item of the bound collection."""
    for at in bound.at(holder, context).to_list():
        if isinstance(bound.descriptor.prop, bpy.types.CollectionProperty):
            items: Iterable[Struct] = getattr(*at)  # ty: ignore[unsound-assignment]
            return tuple(items)
        item: Struct | None = getattr(*at)  # ty: ignore[unsound-assignment]
        return tuple(Option.of_optional(item).to_list())
    return ()


def _host[H: Host](bound: Option[ResolvedMember], owner: type[H], holder: Struct, context: bpy.types.Context) -> Option[H]:
    """Host the store's host member names at the holder, else the holder itself when it is an owner of the store, else the context root of the owner type."""
    return bound.map(lambda held: held.at(holder, context).bind(lambda at: Option.of_optional(try_downcast(owner, getattr(*at))))).default_with(
        lambda: Option.of_optional(try_downcast(owner, holder)).or_else_with(lambda: root_in(owner, context))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Section"]
