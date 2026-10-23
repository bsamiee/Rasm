"""Layout rows from typed declarations or callbacks."""

from collections.abc import Callable, Iterable
from enum import auto, Enum
from itertools import pairwise, product, starmap
from pathlib import PurePath
import typing
from typing import Annotated, ClassVar, Generic, Literal, overload, Protocol, runtime_checkable

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands import keymaps, operators
from blender.bpy.commands.calls import Operation
from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import Descriptor, Native, P_co, root_in
from blender.bpy.data.rna.snapshots import Scoped, Snapshot
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisition, Acquisitions, Band, Registrable
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderFaults, BlenderResult
from blender.bpy.settings import shapes
from blender.bpy.ui.resources.icons import Glyph, Icons
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Unit

if typing.TYPE_CHECKING:
    from bpy.stub_internal import rna_enums
    from bpy.stub_internal.rna_enums import IconItems

# --- [TYPES] ----------------------------------------------------------------------------


type RowDraw = Callable[[bpy.types.UILayout, bpy.types.bpy_struct[object], bpy.types.Context], None]
type SurfaceDraw = Callable[[bpy.types.UILayout, bpy.types.Context], None]
type Reach = Callable[[bpy.types.Context], Option[bpy.types.bpy_struct[object]]]
type Binding = Member | Across | ResolvedMember | msgspec.structs.FieldInfo
type Resolved = ResolvedMember | msgspec.structs.FieldInfo
type Opener = operators.Command | Operation
type Filter = Literal["ALL", "AVAILABLE"]
type Template = Callable[[bpy.types.UILayout, Native, str, bpy.types.bpy_struct[object], bpy.types.Context], None]


@runtime_checkable
class Row(Protocol):
    """Layout declaration acquiring its draw."""

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Acquires draw or every refusal."""
        ...


type RowContent = Row | RowDraw | tuple[Registrable, Callable[[Identity], BlenderResult[RowDraw]]]


class Flow(Enum):
    """Caption content arrangement."""

    STACK = auto()
    GRID = auto()
    ALIGNED = auto()

    def opened(self, layout: bpy.types.UILayout) -> bpy.types.UILayout:
        """Returns caption layout, property split off in grid and aligned flows."""
        match self:
            case Flow.STACK:
                return layout
            case Flow.GRID:
                inner = layout.grid_flow(row_major=True, columns=0, even_columns=True)
            case Flow.ALIGNED:
                inner = layout.row(align=True)
        inner.use_property_split = False
        return inner


# --- [MODELS] ---------------------------------------------------------------------------


class ResolvedMember(msgspec.Struct, Generic[P_co], frozen=True, kw_only=True):
    """RNA definition with optional explicit root source."""

    descriptor: Descriptor[P_co]
    reach: "bpy.types.bpy_struct[object] | Option[Reach]" = Nothing
    scope: Option[Scoped] = Nothing


class Member(msgspec.Struct, frozen=True, kw_only=True):
    """Property path rooted at its owner type."""

    root: "type[bpy.types.bpy_struct[object]]"
    path: str
    reach: Option[Reach] = Nothing

    @overload
    def resolved(self, *, root: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> BlenderResult[ResolvedMember]: ...

    @overload
    def resolved[P: bpy.types.Property](self, kind: type[P], *, root: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> BlenderResult[ResolvedMember[P]]: ...

    def resolved(self, kind: type[bpy.types.Property] = bpy.types.Property, *, root: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> BlenderResult[ResolvedMember]:
        """Resolves metadata on root type, or on live `root` when given."""
        return root.map(
            lambda actual: (
                Descriptor.resolve(actual, self.path, kind).map(lambda descriptor: ResolvedMember(descriptor=descriptor, reach=actual))
                if isinstance(actual, self.root)
                else Error(refused(self.path, type(actual).__name__, (self.root.__name__,)))
            )
        ).default_with(lambda: Descriptor.resolve(self.root, self.path, kind).map(lambda descriptor: ResolvedMember(descriptor=descriptor, reach=self.reach)))


class Across(msgspec.Struct, frozen=True, kw_only=True):
    """Host member read across every owner of an edit scope."""

    scope: Scoped
    path: str


class PreviewGrid(msgspec.Struct, frozen=True, kw_only=True):
    """Selector preview grid, zero taking Blender's count."""

    rows: Annotated[int, msgspec.Meta(ge=0)] = 0
    cols: Annotated[int, msgspec.Meta(ge=0)] = 0


class DataBlocks(msgspec.Struct, frozen=True, kw_only=True):
    """Data-block browse through `template_ID`."""

    kinds: "ClassVar[tuple[type[bpy.types.bpy_struct[object]], ...]]" = (bpy.types.ID,)
    new: Option[Opener] = Nothing
    open: Option[Opener] = Nothing
    unlink: Option[Opener] = Nothing
    filter: Filter = "ALL"


class Items(msgspec.Struct, frozen=True, kw_only=True):
    """Item browse over an RNA collection through `template_search`."""

    kinds: "ClassVar[tuple[type[bpy.types.bpy_struct[object]], ...]]" = (bpy.types.PointerProperty,)
    collection: Member
    new: Option[Opener] = Nothing
    unlink: Option[Opener] = Nothing


class Work(msgspec.Struct, frozen=True, kw_only=True):
    """Running work text and fraction of items done."""

    text: str
    done: Unit


class Branch[L: "bpy.types.bpy_struct[object]", C: "bpy.types.bpy_struct[object]"](msgspec.Struct, frozen=True, kw_only=True):
    """Tree row with depth, label struct, and cell holder per column owner."""

    depth: int
    label: L
    cells: tuple[Option[C], ...]


class Toggle[R: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Tree cell operator button."""

    record: R
    icon: "IconItems"
    depress: bool


class Line(msgspec.Struct, frozen=True, kw_only=True):
    """Row with optional availability condition."""

    condition: Option[Condition] = Nothing


class Property(Line, frozen=True, kw_only=True):
    """Setting drawn with its RNA widget."""

    target: Binding
    text: Option[str] = Nothing
    icon: Option[Glyph] = Nothing
    icon_only: bool = False
    placeholder: Option[str] = Nothing
    action: Option[RowContent] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Binds setting and action once for every redraw."""
        kinds = (bpy.types.BoolProperty, bpy.types.IntProperty, bpy.types.FloatProperty, bpy.types.StringProperty, bpy.types.EnumProperty, bpy.types.ID)
        forms = (shapes.Flag, shapes.Integer, shapes.Number, shapes.Text, shapes.Enumerated, shapes.Vector, shapes.Components, shapes.Flags, shapes.Pointer, shapes.Color, shapes.Gate)

        def valued(layout: bpy.types.UILayout, owner: Native, key: str, shape: Option[shapes.Shape], label: Option[str], index: int = -1) -> None:
            prop = owner.bl_rna.properties[key]  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            match shape.to_optional():
                case shapes.Gate(value=value):
                    group = getattr(owner, key)
                    gate = layout.row(heading=label.default_value(prop.name), align=True)
                    gate.prop(group, shapes.ENABLED, text="")
                    inner = gate.row(align=True)
                    inner.active = getattr(group, shapes.ENABLED)
                    for field, held in value.to_list():
                        valued(inner, group, field.name, Some(held.shape), Some(""))
                case shapes.Flags(enum=choices):
                    group = getattr(owner, key)
                    inner = layout.column(heading=label.default_value(prop.name), align=True)
                    for member in choices:
                        inner.prop(group, member.name)
                case shapes.Components(items=items):
                    group = getattr(owner, key)
                    inner = layout.column(heading=label.default_value(prop.name), align=True)
                    for name, item in zip(type(group).__annotations__, items):
                        valued(inner, group, name, Some(item.shape), Nothing)
                case _:
                    layout.prop(
                        owner,
                        key,
                        index=index,
                        text=label.to_optional(),
                        icon_value=iconed(identity, self.icon),
                        icon_only=self.icon_only,
                        placeholder=self.placeholder.to_optional(),
                        slider=isinstance(prop, bpy.types.FloatProperty) and prop.subtype == "ANGLE",
                        expand=isinstance(prop, bpy.types.EnumProperty) and not prop.is_enum_flag and 2 <= len(prop.enum_items) <= 5,
                    )

        def draw(held: Resolved, action: Option[RowDraw], shape: Option[shapes.Shape]) -> RowDraw:
            index = held.descriptor.index.default_value(-1) if isinstance(held, ResolvedMember) else -1

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                line = layout.row() if action.is_some() else layout
                match held:
                    case ResolvedMember(descriptor=descriptor, scope=Option(tag="some", some=scope)):
                        labelled = scope.owners(context).bind(lambda owners: marked(descriptor, owners)).map(lambda label: label.or_else(self.text))
                    case _:
                        labelled = Ok(self.text)
                for owner, key in located(held, holder, context).to_list():
                    alerted(line, labelled.map(lambda label: valued(line, owner, key, shape, label, index)))
                    for extra in action.to_list():
                        extra(line, holder, context)

            return shown

        action = self.action.map(lambda row: realized(row, identity).map(Some)).default_value(Ok(Nothing))
        shape = shapes.field_shape(self.target.type).map(lambda declared: Some(declared.shape)) if isinstance(self.target, msgspec.structs.FieldInfo) else Ok(Nothing)
        return bound(((self.target, lambda held: fits(held, kinds, forms)),), paired(action, shape).map(lambda pair: lambda held: draw(held[0], *pair)))


class SearchField(Line, frozen=True, kw_only=True):
    """String or pointer search over declared RNA collection."""

    target: Member
    collection: Member
    text: Option[str] = Nothing
    results_are_suggestions: bool = False

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds property and collection together."""

        def draw(parts: tuple[Resolved, ...]) -> RowDraw:
            held, items = parts

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for owner, key in located(held, holder, context).to_list():
                    for source, listed in located(items, holder, context).to_list():
                        layout.prop_search(owner, key, source, listed, text=self.text.to_optional(), results_are_suggestions=self.results_are_suggestions)

            return shown

        return bound(
            ((self.target, lambda held: fits(held, (bpy.types.StringProperty, bpy.types.PointerProperty))), (self.collection, lambda held: fits(held, (bpy.types.CollectionProperty,)))), Ok(draw)
        )


class Selector(Line, frozen=True, kw_only=True):
    """Data-block or item pointer in ID or search template."""

    target: Member | msgspec.structs.FieldInfo
    browse: DataBlocks | Items = DataBlocks()
    preview: Option[PreviewGrid] = Nothing
    label: Option[str] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves pointer, collection, and openers into template draw."""

        def named(opener: Opener) -> BlenderResult[str]:
            match opener:
                case operators.Command(key=key):
                    return identity.name(Name.OPERATOR, key)
                case _:
                    return Ok(opener.idname_py())  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

        def opened(opener: Option[Opener]) -> BlenderResult[str]:
            return opener.map(named).default_value(Ok(""))

        def blocks(chosen: Filter, ids: tuple[str, ...]) -> Template:
            new, open_, unlink = ids

            def shown(line: bpy.types.UILayout, owner: Native, key: str, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
                match self.preview.to_list():
                    case [PreviewGrid(rows=rows, cols=cols)]:
                        line.template_ID_preview(owner, key, new=new, open=open_, unlink=unlink, filter=chosen, rows=rows, cols=cols)
                    case _:
                        line.template_ID(owner, key, new=new, open=open_, unlink=unlink, filter=chosen)

            return shown

        def searched(collection: Resolved, new: str, unlink: str) -> Template:

            def shown(line: bpy.types.UILayout, owner: Native, key: str, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for items, listed in located(collection, holder, context).to_list():
                    match self.preview.to_list():
                        case [PreviewGrid(rows=rows, cols=cols)]:
                            line.template_search_preview(owner, key, items, listed, new=new, unlink=unlink, rows=rows, cols=cols)
                        case _:
                            line.template_search(owner, key, items, listed, new=new, unlink=unlink)

            return shown

        def draw(held: Resolved, placed_template: Template) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                line = gridded(layout, self.label)
                for owner, key in located(held, holder, context).to_list():
                    placed_template(line, owner, key, holder, context)

            return shown

        match self.browse:
            case DataBlocks(new=new, open=open_, unlink=unlink, filter=chosen):
                template = accumulated(map(opened, (new, open_, unlink))).map(lambda ids: blocks(chosen, ids))
                return bound(((self.target, lambda held: fits(held, self.browse.kinds, (shapes.Pointer,))),), template.map(lambda template: lambda held: draw(held[0], template)))
            case Items(collection=collection, new=new, unlink=unlink):
                openers = accumulated(map(opened, (new, unlink)))
                return bound(
                    ((self.target, lambda held: fits(held, self.browse.kinds, (shapes.Pointer,))), (collection, lambda held: fits(held, (bpy.types.CollectionProperty,)))),
                    openers.map(lambda ids: lambda held: draw(held[0], searched(held[1], *ids))),
                )


class Matrix(Line, frozen=True, kw_only=True):
    """Setting cells across scoped owners, compound settings through caller editors."""

    scope: Scoped
    settings: tuple[str, ...]
    compound: "tuple[tuple[type[bpy.types.bpy_struct[object]], Callable[[bpy.types.UILayout, Native, str], None]], ...]" = ()
    action: Option[tuple[Registrable, Callable[[Identity, Descriptor], BlenderResult[RowDraw]]]] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves leaf settings and owner names into table draw."""
        owner_type = self.scope.owner_type
        compound = tuple(kind for kind, _ in self.compound)
        named = Option.of_optional(owner_type.bl_rna.name_property).to_result_with(lambda: refused(owner_type.__name__, "name_property", (bpy.types.StringProperty.__name__,)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

        def cell(setting: Descriptor) -> BlenderResult[tuple[Descriptor, RowDraw]]:
            action = self.action.map(lambda policy: (policy[0], lambda at: policy[1](at, setting)))
            template = Option.of_optional(next((place for kind, place in self.compound if isinstance(setting.prop, bpy.types.PointerProperty) and isinstance(setting.prop.fixed_type, kind)), None))

            def composed(place: Callable[[bpy.types.UILayout, Native, str], None], action: Option[RowDraw]) -> RowDraw:
                def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                    for owner, key in located(ResolvedMember(descriptor=setting), holder, context).to_list():
                        for draw in action.to_list():
                            draw(layout.row(align=True), holder, context)
                        place(layout, owner, key)

                return shown

            return (
                template
                .map(lambda place: action.map(lambda row: realized(row, identity).map(Some)).default_value(Ok(Nothing)).map(lambda draw: composed(place, draw)))
                .default_with(lambda: Property(target=ResolvedMember(descriptor=setting), text=Some(""), action=action).drawn(identity))
                .map(lambda draw: (setting, draw))
            )

        def draw(parts: tuple[bpy.types.StringProperty, tuple[tuple[Descriptor, RowDraw], ...]]) -> RowDraw:
            name, held = parts

            def table(layout: bpy.types.UILayout, found: "tuple[bpy.types.bpy_struct[object], ...]", context: bpy.types.Context) -> None:
                names = gridded(layout, Some(""))
                for owner in found:
                    names.label(text=getattr(owner, name.identifier))
                for setting, drawing in held:
                    cells = marked(setting, found).map(lambda label: gridded(layout, label.or_else(Some(setting.prop.name))))
                    alerted(layout, cells)
                    for line, owner in product(cells.to_option().to_list(), found):
                        drawing(line.column(), owner, context)

            return lambda layout, _holder, context: alerted(layout, self.scope.owners(context).map(lambda found: table(layout, found, context)))

        settings = accumulated(Descriptor.resolve(owner_type, path, bpy.types.Property) for path in self.settings).map(
            lambda found: tuple(leaf for each in found for leaf in Snapshot.settings(each, compound))
        )
        return paired(named, settings.bind(lambda found: accumulated(map(cell, found)))).map(draw)


class Tree(Line, frozen=True, kw_only=True):
    """Resolved paths listed once per prefix, indented by depth."""

    paths: Callable[[bpy.types.Context], BlenderResult[tuple[PurePath, ...]]]

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Returns tree draw over source paths."""

        def listed(layout: bpy.types.UILayout, paths: tuple[PurePath, ...]) -> None:
            files = {path.parts for path in paths}
            for node in sorted({path.parts[:end] for path in paths for end in range(1, len(path.parts) + 1)}):
                line = layout.row(align=True)
                for _ in node[1:]:
                    line.label(text="", icon="BLANK1")
                line.label(text=node[-1], icon="FILE_BLANK" if node in files else "FILE_FOLDER")

        return Ok(lambda layout, _holder, context: alerted(layout, self.paths(context).map(lambda paths: listed(layout, paths))))


class TreeMatrix[R: msgspec.Struct, L: "bpy.types.bpy_struct[object]", C: "bpy.types.bpy_struct[object]"](Line, frozen=True, kw_only=True):
    """Tree rows indented by depth with toggle cells per column owner."""

    tree: Callable[[bpy.types.Context], tuple[tuple[str, ...], tuple[Branch[L, C], ...]]]
    command: operators.Operator[R]
    toggles: Callable[[str, C, L], tuple[Toggle[R], ...]]
    active: Option[Callable[[C], bool]] = Nothing
    marks: tuple[RowContent, ...] = ()

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Realizes label marks into tree draw."""

        def draw(marks: tuple[RowDraw, ...]) -> RowDraw:
            def cell(line: bpy.types.UILayout, owner: str, found: C, label: L) -> None:
                line.active = all(read(found) for read in self.active.to_list())
                for toggle in self.toggles(owner, found, label):
                    alerted(line, pressed(line, identity, self.command, toggle.record, text="", icon=toggle.icon, depress=toggle.depress))

            def branch(layout: bpy.types.UILayout, owners: tuple[str, ...], held: Branch[L, C], context: bpy.types.Context) -> None:
                names, values = split(layout)
                for _ in range(held.depth):
                    names.label(text="", icon="BLANK1")
                for text in Option.of_optional(held.label.bl_rna.name_property).map(lambda prop: getattr(held.label, prop.identifier)).to_list():  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    names.label(text=text)
                for mark in marks:
                    mark(names, held.label, context)
                cells = values.grid_flow(row_major=True, columns=len(owners), even_columns=True, align=True)
                for owner, found in zip(owners, held.cells):
                    line = cells.row(align=True)
                    for each in found.to_list():
                        cell(line, owner, each, held.label)

            def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                owners, branches = self.tree(context)
                _, values = split(layout)
                heads = values.grid_flow(row_major=True, columns=len(owners), even_columns=True, align=True)
                for owner in owners:
                    heads.label(text=owner)
                for held in branches:
                    branch(layout, owners, held, context)

            return invoking(shown) if self.command.invoked else shown

        return accumulated(realized(row, identity) for row in self.marks).map(draw)


class OperatorButton(Line, frozen=True, kw_only=True):
    """Operator button over a keymap target."""

    target: keymaps.Target
    text: Option[str] = Nothing
    icon: Option[Glyph] = Nothing
    label: Option[str] = Nothing
    emboss: bool = True
    search_weight: float = 0.0
    context: "rna_enums.OperatorContextItems" = "INVOKE_REGION_WIN"

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves operator idname, writer, and call context into button draw."""
        command = self.target.command if isinstance(self.target, operators.Own) else self.target
        match command:
            case operators.Operator():
                glyph, invoked = self.icon.or_else(command.icon), command.invoked
            case operators.Macro() as macro:
                glyph, invoked = self.icon.or_else(macro.icon), True
            case _:
                glyph, invoked = self.icon, True

        def draw(parts: tuple[str, operators.Writer]) -> RowDraw:
            idname, write = parts

            def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
                line = gridded(layout, self.label)
                alerted(
                    line, operated(line.operator(idname, text=self.text.to_optional(), emboss=self.emboss, icon_value=iconed(identity, glyph), search_weight=self.search_weight), idname).bind(write)
                )

            return invoking(shown, self.context) if invoked else shown

        return keymaps.invocation(self.target, identity).map(draw)


class Popover(Line, frozen=True, kw_only=True):
    """Popover button for stock panel or extension panel key."""

    panel: type[bpy.types.Panel] | Key
    text: Option[str] = Nothing
    label: Option[str] = Nothing
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves panel idname, reading glyph each draw."""
        named = identity.name(Name.PANEL, self.panel) if isinstance(self.panel, str) else Ok(self.panel.bl_rna.identifier)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return named.map(lambda name: lambda layout, _holder, _context: gridded(layout, self.label).popover(panel=name, text=self.text.to_optional(), icon_value=iconed(identity, self.icon)))


class Pulldown(Line, frozen=True, kw_only=True):
    """Menu button for stock menu or extension menu key."""

    menu: type[bpy.types.Menu] | Key
    text: Option[str] = Nothing
    label: Option[str] = Nothing
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves menu idname, reading glyph each draw."""
        named = identity.name(Name.MENU, self.menu) if isinstance(self.menu, str) else Ok(self.menu.bl_rna.identifier)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return named.map(lambda name: lambda layout, _holder, _context: gridded(layout, self.label).menu(name, text=self.text.to_optional(), icon_value=iconed(identity, self.icon)))


class Caption(Line, frozen=True, kw_only=True):
    """Rows grouped under heading or separator."""

    rows: tuple[RowContent, ...]
    heading: Option[str] = Nothing
    flow: Flow = Flow.STACK

    def framed(self, draws: tuple[RowDraw, ...]) -> RowDraw:
        """Returns joined draws in caption arrangement."""
        draw = joined(draws)

        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            if self.heading.is_none():
                layout.separator(type="LINE")
            draw(self.flow.opened(layout.column(heading=self.heading.default_value(""))), holder, context)

        return shown

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Places rows on enclosing property grid."""
        return placed(self.rows, identity).map(self.framed)


class Label(Line, frozen=True, kw_only=True):
    """Fixed or live text as Markdown, header, or glyph label."""

    text: str | Callable[[bpy.types.Context], Option[str]]
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns draw reading text and icon each redraw."""

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for text in (Some(self.text) if isinstance(self.text, str) else self.text(context)).to_list():
                match self.icon:
                    case Option(tag="some", some=glyph):
                        layout.label_multiline(text=text, icon_value=Icons.value(identity, glyph))
                    case _ if Option.of_optional(context.region).filter(lambda region: region.type == "HEADER").is_some():
                        layout.label(text=text)
                    case _:
                        layout.label_markdown(text=text)

        return Ok(shown)


class Text(Line, frozen=True, kw_only=True):
    """String drawn in multiline text box."""

    target: Binding
    initial_visible_lines: Annotated[int, msgspec.Meta(ge=1)] = 3
    placeholder: Option[str] = Nothing
    label: Option[str] = Nothing

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds string property."""

        def draw(held: Resolved) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for owner, key in located(held, holder, context).to_list():
                    gridded(layout, self.label).textbox(owner, key, initial_visible_lines=self.initial_visible_lines, placeholder=self.placeholder.to_optional())

            return shown

        return bound(((self.target, lambda held: fits(held, (bpy.types.StringProperty,), (shapes.Text,))),), Ok(lambda held: draw(held[0])))


class Progress(Line, frozen=True, kw_only=True):
    """Progress bars or rings over running work fractions."""

    runs: Callable[[bpy.types.Context], Iterable[Work]]
    type: Literal["BAR", "RING"] = "BAR"
    label: Option[str] = Nothing

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Returns draw of every running work."""

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for run in self.runs(context):
                gridded(layout, self.label).progress(text=run.text, factor=run.done, type=self.type)

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BINDING]


def refused(path: str, found: str, accepted: tuple[str, ...]) -> BlenderFaults:
    """Returns choice fault for an unsupported binding."""
    return (BlenderChoiceFault(path=path, identifier=found, accepted=accepted),)


def bound(bindings: tuple[tuple[Binding, Callable[[Resolved], BlenderResult[Resolved]]], ...], place: BlenderResult[Callable[[tuple[Resolved, ...]], RowDraw]]) -> BlenderResult[RowDraw]:
    """Resolves static bindings once and reached sources each draw, drawing nothing while one is absent."""

    def resolved(binding: Binding, accept: Callable[[Resolved], BlenderResult[Resolved]]) -> BlenderResult[Resolved]:
        match binding:
            case Member():
                return binding.resolved().bind(accept)
            case Across(scope=scope, path=path):
                return Descriptor.resolve(scope.owner_type, path, bpy.types.Property).map(lambda descriptor: ResolvedMember(descriptor=descriptor, scope=Some(scope))).bind(accept)
            case ResolvedMember() | msgspec.structs.FieldInfo():
                return accept(binding)

    def reached(binding: Resolved | tuple[Member, Callable[[Resolved], BlenderResult[Resolved]]], context: bpy.types.Context) -> BlenderResult[Option[Resolved]]:
        match binding:
            case tuple():
                member, accept = binding
                return member.reach.bind(lambda source: source(context)).map(lambda root: member.resolved(root=Some(root)).bind(accept).map(Some)).default_value(Ok(Nothing))
            case ResolvedMember() | msgspec.structs.FieldInfo():
                return Ok(Some(binding))

    def draw(parts: tuple[tuple[Resolved | tuple[Member, Callable[[Resolved], BlenderResult[Resolved]]], ...], Callable[[tuple[Resolved, ...]], RowDraw]]) -> RowDraw:
        held, placement = parts

        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            result = accumulated(reached(binding, context) for binding in held)
            alerted(layout, result)
            for found in result.to_option().to_list():
                present = tuple(value for each in found for value in each.to_list())
                if len(present) == len(found):
                    placement(present)(layout, holder, context)

        return shown

    if not any(isinstance(binding, Member) and binding.reach.is_some() for binding, _ in bindings):
        return paired(accumulated(starmap(resolved, bindings)), place).map(lambda parts: parts[1](parts[0]))
    return paired(accumulated(Ok((binding, accept)) if isinstance(binding, Member) and binding.reach.is_some() else resolved(binding, accept) for binding, accept in bindings), place).map(draw)


def fits(held: Resolved, kinds: "tuple[type[bpy.types.bpy_struct[object]], ...]", forms: tuple[type[object], ...] = ()) -> BlenderResult[Resolved]:
    """Accepts property kinds or field shapes a control draws, enum classes included."""
    if isinstance(held, ResolvedMember):
        descriptor = held.descriptor
        prop = descriptor.prop
        target = prop.fixed_type if isinstance(prop, bpy.types.PointerProperty) else prop
        return Ok(held) if isinstance(prop, kinds) or isinstance(target, kinds) else Error(refused(descriptor.path, type(target).__name__, tuple(kind.__name__ for kind in kinds)))
    return shapes.field_shape(held.type).bind(
        lambda declared: (
            Ok(held)
            if isinstance(declared.shape, forms) or (isinstance(declared.shape, shapes.Enumerated) and issubclass(declared.shape.enum, forms))
            else Error(refused(held.name, type(declared.shape).__name__, tuple(form.__name__ for form in forms)))
        )
    )


def colored(held: Resolved) -> BlenderResult[Resolved]:
    """Accepts RNA color arrays or domain RGB record."""
    if isinstance(held, ResolvedMember):
        match held.descriptor.prop:
            case bpy.types.FloatProperty(is_array=True, subtype="COLOR" | "COLOR_GAMMA"):
                return Ok(held)
            case prop:
                return Error(refused(held.descriptor.path, prop.subtype, ("COLOR", "COLOR_GAMMA")))
    return fits(held, (), (shapes.Color,))


def located(held: Resolved, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> Option[tuple[Native, str]]:
    """Locates property owner through root, holder, or context."""
    match held:
        case ResolvedMember(descriptor=descriptor, reach=reach):
            root = (
                Some(reach)
                if isinstance(reach, bpy.types.bpy_struct)
                else Some(holder)
                if isinstance(holder, descriptor.root)
                else reach.map(lambda source: source(context)).default_with(lambda: root_in(descriptor.root, context))
            )
            return root.bind(descriptor.owner_of).map(lambda owner: (owner, descriptor.prop.identifier))
        case msgspec.structs.FieldInfo(name=name):
            return Some((holder, name))


def marked(setting: Descriptor, owners: "tuple[bpy.types.bpy_struct[object], ...]") -> BlenderResult[Option[str]]:
    """Returns mixed-value label when owner values differ."""
    return accumulated(Snapshot.capture(owner, Some((setting,))) for owner in owners).map(
        lambda values: Some(f"{setting.prop.name} —") if any(left != right for left, right in pairwise(values)) else Nothing
    )


# --- [DRAWING]


def iconed(identity: Identity, icon: Option[Glyph]) -> int:
    """Reads glyph icon ID, 0 for `NONE`."""
    return icon.map(lambda glyph: Icons.value(identity, glyph)).default_value(0)


def operated(properties: bpy.types.OperatorProperties | None, idname: str) -> BlenderResult[bpy.types.OperatorProperties]:
    """Maps missing operator properties to absent fault."""
    return Option.of_optional(properties).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.ops", key=idname),))


def pressed[T: msgspec.Struct, S](
    layout: bpy.types.UILayout, identity: Identity, command: operators.Operator[T, S], record: T, *, text: str, icon: "IconItems", depress: bool = False
) -> BlenderResult[None]:
    """Draws operator button over record."""
    return (
        operators
        .Own(command=command, values=record)
        .invocation(identity)
        .bind(lambda placement: operated(layout.operator(placement[0], text=text, icon=icon, depress=depress), placement[0]).bind(placement[1]))
    )


def split(layout: bpy.types.UILayout) -> tuple[bpy.types.UILayout, bpy.types.UILayout]:
    """Places label and value cells beside reserved decorator cell."""
    line = layout.row(align=True)
    halves = line.split(factor=layout.property_split_factor, align=True)
    names, values = halves.column(align=True), halves.row(align=True)
    values.use_property_split = False
    values.use_property_decorate = False
    if layout.use_property_decorate:
        line.label(text="", icon="BLANK1")
    return names, values


def gridded(layout: bpy.types.UILayout, label: Option[str]) -> bpy.types.UILayout:
    """Places label on property grid when one is given."""
    match label:
        case Option(tag="some", some=text):
            names, values = split(layout)
            names.alignment = "RIGHT"
            names.label(text=text)
            return values
        case _:
            return layout


def alerted[T](layout: bpy.types.UILayout, result: BlenderResult[T]) -> None:
    """Draws failures as alert labels."""
    for faults in result.swap().to_option().to_list():
        column = layout.column(align=True)
        column.alert = True
        for _, text in rendered(faults):
            column.label(text=text, icon="STATUS_ERROR_FILLED")


def invoking(draw: RowDraw, mode: "rna_enums.OperatorContextItems" = "INVOKE_REGION_WIN") -> RowDraw:
    """Scopes operator context around drawing."""

    def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
        prior = layout.operator_context
        layout.operator_context = mode
        try:
            draw(layout, holder, context)
        finally:
            layout.operator_context = prior

    return shown


# --- [REALIZATION]


def acquired(rows: tuple[RowContent, ...]) -> Acquisitions:
    """Collects registrable declarations nested in composed rows."""
    declarations: list[Registrable | tuple[Band, Acquisition]] = []
    for row in rows:
        match row:
            case (Registrable() as owner, _):
                declarations.append(owner)
            case Callable():
                continue
            case Registrable():
                declarations.append(row)
            case Caption(rows=nested):
                declarations.extend(acquired(nested))
            case Property(action=action):
                declarations.extend(acquired(tuple(action.to_list())))
            case TreeMatrix(command=command, marks=marks):
                declarations.extend((command, *acquired(marks)))
            case Matrix(action=action):
                declarations.extend(owner for owner, _ in action.to_list())
            case OperatorButton(target=operators.Own(command=command)):
                declarations.append(command)
            case OperatorButton(target=operators.Operator() | operators.Macro() as command):
                declarations.append(command)
            case Selector(browse=browse):
                openers = (browse.new, browse.open, browse.unlink) if isinstance(browse, DataBlocks) else (browse.new, browse.unlink)
                declarations.extend(opener for placed in openers for opener in placed.to_list() if isinstance(opener, Registrable))
            case Row():
                continue
    return tuple(declarations)


def realized(row: RowContent, identity: Identity) -> BlenderResult[RowDraw]:
    """Acquires typed row draw, passing callbacks through."""
    match row:
        case tuple():
            return row[1](identity)
        case Row():
            return row.drawn(identity).map(lambda draw: guarded(draw, row.condition) if isinstance(row, Line) else draw)
        case Callable():
            return Ok(row)


def guarded(draw: RowDraw, condition: Option[Condition]) -> RowDraw:
    """Wraps draw in column inactive while row condition fails."""

    def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
        for required in condition.to_list():
            layout = layout.column()
            layout.active = holds(required, context, Some(holder))
        draw(layout, holder, context)

    return shown


def placed(rows: tuple[RowContent, ...], identity: Identity, *, inherited: bool = False) -> BlenderResult[tuple[RowDraw, ...]]:
    """Realizes rows, padding actionless properties beside action rows."""

    def acted(row: RowContent) -> bool:
        match row:
            case Property(action=action):
                return action.is_some()
            case Caption(rows=inner):
                return any(map(acted, inner))
            case _:
                return False

    picked = inherited or any(map(acted, rows))

    def blanked(draw: RowDraw) -> RowDraw:
        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            line = layout.row()
            draw(line, holder, context)
            line.label(text="", icon="BLANK1")

        return shown

    def each(row: RowContent) -> BlenderResult[RowDraw]:
        match row:
            case Caption():
                return placed(row.rows, identity, inherited=picked).map(row.framed).map(lambda draw: guarded(draw, row.condition))
            case Property(action=action) if picked and action.is_none():
                return row.drawn(identity).map(blanked).map(lambda draw: guarded(draw, row.condition))
            case _:
                return realized(row, identity)

    return accumulated(map(each, rows))


def joined(draws: tuple[RowDraw, ...]) -> RowDraw:
    """Joins draws in declaration order over one holder."""

    def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
        for draw in draws:
            draw(layout, holder, context)

    return shown


def painted(draws: tuple[RowDraw, ...]) -> SurfaceDraw:
    """Uses context as holder of joined row draws."""
    draw = joined(draws)
    return lambda layout, context: draw(layout, context, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Across",
    "Binding",
    "Branch",
    "Caption",
    "DataBlocks",
    "Flow",
    "Items",
    "Label",
    "Line",
    "Matrix",
    "Member",
    "Opener",
    "OperatorButton",
    "Popover",
    "PreviewGrid",
    "Progress",
    "Property",
    "Pulldown",
    "Reach",
    "Resolved",
    "ResolvedMember",
    "Row",
    "RowContent",
    "RowDraw",
    "SearchField",
    "Selector",
    "SurfaceDraw",
    "Template",
    "Text",
    "Toggle",
    "Tree",
    "TreeMatrix",
    "Work",
    "acquired",
    "alerted",
    "bound",
    "colored",
    "fits",
    "gridded",
    "guarded",
    "iconed",
    "invoking",
    "joined",
    "located",
    "marked",
    "operated",
    "painted",
    "placed",
    "pressed",
    "realized",
    "refused",
    "split",
]
