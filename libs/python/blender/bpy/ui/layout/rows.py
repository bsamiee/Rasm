"""Native property bindings and acquired or callable layout drawing."""

from collections.abc import Callable, Iterable
from enum import auto, Enum
from itertools import pairwise, product
from pathlib import PurePath
import typing
from typing import Annotated, ClassVar, Generic, Literal, overload, Protocol, runtime_checkable, TypeVar

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
from msgspec import inspect

from blender.bpy.commands import keymaps, operators
from blender.bpy.commands.calls import Operation
from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import Descriptor, root_in
from blender.bpy.data.rna.snapshots import Scoped, Snapshot
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisition, Acquisitions, Band, Registrable
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderFaults, BlenderResult
from blender.bpy.settings.shapes import vector
from blender.bpy.ui.resources.icons import Glyph, Icons
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Gated, Rgb

if typing.TYPE_CHECKING:
    from bpy.stub_internal import rna_enums
    from bpy.stub_internal.rna_enums import IconItems

# --- [TYPES] ----------------------------------------------------------------------------

P_co = TypeVar("P_co", bound=bpy.types.Property, covariant=True, default=bpy.types.Property)

type RowDraw = Callable[[bpy.types.UILayout, bpy.types.bpy_struct[object], bpy.types.Context], None]
type SurfaceDraw = Callable[[bpy.types.UILayout, bpy.types.Context], None]
type Owner = bpy.types.bpy_struct[object] | bpy.types.bpy_prop_collection[bpy.types.bpy_struct[object]]
type Reach = Callable[[bpy.types.Context], Option[bpy.types.bpy_struct[object]]]
type Binding = Member | Across | ResolvedMember | msgspec.structs.FieldInfo
type Resolved = ResolvedMember | msgspec.structs.FieldInfo
type Opener = operators.Command | Operation
type Template = Callable[[bpy.types.UILayout, Owner, str, bpy.types.bpy_struct[object], bpy.types.Context], None]


@runtime_checkable
class Row(Protocol):
    """Layout declaration acquiring its drawing under an extension identity."""

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Acquires the declaration's drawing or its complete typed refusals."""
        ...


type RowContent = Row | RowDraw | tuple[Registrable, Callable[[Identity], BlenderResult[RowDraw]]]


class Flow(Enum):
    """Native arrangement of a caption's content."""

    STACK = auto()
    GRID = auto()
    ALIGNED = auto()

    def opened(self, layout: bpy.types.UILayout) -> bpy.types.UILayout:
        """Returns the caption's native layout with its property split policy."""
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
    """Native RNA definition and its optional explicit root source."""

    descriptor: Descriptor[P_co]
    reach: "bpy.types.bpy_struct[object] | Option[Reach]" = Nothing
    scope: Option[Scoped] = Nothing


class Member(msgspec.Struct, frozen=True, kw_only=True):
    """Property path rooted at its actual native owner type."""

    root: "type[bpy.types.bpy_struct[object]]"
    path: str
    reach: Option[Reach] = Nothing

    @overload
    def resolved(self, *, root: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> BlenderResult[ResolvedMember]: ...

    @overload
    def resolved[P: bpy.types.Property](self, kind: type[P], *, root: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> BlenderResult[ResolvedMember[P]]: ...

    def resolved(self, kind: type[bpy.types.Property] = bpy.types.Property, *, root: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> BlenderResult[ResolvedMember]:
        """Resolves declared metadata or an admitted live root's concrete native metadata."""
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
    """Preview grid of a selector, zero taking Blender's own count."""

    rows: Annotated[int, msgspec.Meta(ge=0)] = 0
    cols: Annotated[int, msgspec.Meta(ge=0)] = 0


class DataBlocks(msgspec.Struct, frozen=True, kw_only=True):
    """Data-block browse through `template_ID`."""

    kinds: "ClassVar[tuple[type[bpy.types.bpy_struct[object]], ...]]" = (bpy.types.ID,)
    new: Option[Opener] = Nothing
    open: Option[Opener] = Nothing
    unlink: Option[Opener] = Nothing
    filter: Literal["ALL", "AVAILABLE"] = "ALL"


class Items(msgspec.Struct, frozen=True, kw_only=True):
    """Item browse over an RNA collection through `template_search`."""

    kinds: "ClassVar[tuple[type[bpy.types.bpy_struct[object]], ...]]" = (bpy.types.PointerProperty,)
    collection: Member
    new: Option[Opener] = Nothing
    unlink: Option[Opener] = Nothing


class Work(msgspec.Struct, frozen=True, kw_only=True):
    """Running work's text and the fraction of its items done."""

    text: str
    done: Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]


class Branch[L: "bpy.types.bpy_struct[object]", C: "bpy.types.bpy_struct[object]"](msgspec.Struct, frozen=True, kw_only=True):
    """Tree row: its depth below the root, the struct labelling it, and its cell holder per column owner."""

    depth: int
    label: L
    cells: tuple[Option[C], ...]


class Toggle[R: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Operator button of a tree cell: the record it writes, its stock icon, and its pressed state."""

    record: R
    icon: "IconItems"
    depress: bool


class Line(msgspec.Struct, frozen=True, kw_only=True):
    """Row with an optional native availability condition."""

    condition: Option[Condition] = Nothing


class Property(Line, frozen=True, kw_only=True):
    """Setting whose native RNA determines its widget."""

    target: Binding
    text: Option[str] = Nothing
    icon: Option[Glyph] = Nothing
    icon_only: bool = False
    placeholder: Option[str] = Nothing
    action: Option[RowContent] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the binding and optional action once for native redraws."""
        enabled, value = Gated.__struct_fields__
        kinds = (bpy.types.BoolProperty, bpy.types.IntProperty, bpy.types.FloatProperty, bpy.types.StringProperty, bpy.types.EnumProperty, bpy.types.ID)
        forms = (inspect.BoolType, inspect.IntType, inspect.FloatType, inspect.StrType, Enum, inspect.TupleType, inspect.FrozenSetType, bpy.types.ID, Rgb, Gated)
        shape = inspect.type_info(self.target.type) if isinstance(self.target, msgspec.structs.FieldInfo) else self.target

        def valued(layout: bpy.types.UILayout, owner: Owner, key: str, kind: object, label: Option[str], index: int = -1) -> None:
            while isinstance(kind, inspect.Metadata):
                kind = kind.type
            prop = owner.bl_rna.properties[key]  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            match kind:
                case inspect.StructType(cls=record, fields=native) if (typing.get_origin(record) or record) is Gated:
                    group = getattr(owner, key)
                    gate = layout.row(heading=label.default_value(prop.name), align=True)
                    gate.prop(group, enabled, text="")
                    inner = gate.row(align=True)
                    inner.active = getattr(group, enabled)
                    valued(inner, group, value, next(field.type for field in native if field.name == value), Some(""))
                case inspect.FrozenSetType(item_type=inspect.EnumType(cls=choices)):
                    group = getattr(owner, key)
                    inner = layout.column(heading=label.default_value(prop.name), align=True)
                    for member in choices:
                        inner.prop(group, member.name)
                case inspect.TupleType(item_types=components) if not vector(kind):
                    group = getattr(owner, key)
                    inner = layout.column(heading=label.default_value(prop.name), align=True)
                    for name, component in zip(type(group).__annotations__, components):
                        valued(inner, group, name, component, Nothing)
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

        def draw(parts: tuple[Resolved, Option[RowDraw]]) -> RowDraw:
            held, action = parts
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
        return bound(((self.target, lambda held: fits(held, kinds, forms)),), action.map(lambda action: lambda held: Ok(held[0].map(lambda target: draw((target, action))))))


class SearchField(Line, frozen=True, kw_only=True):
    """Native string or pointer search over a declared RNA collection."""

    target: Member
    collection: Member
    text: Option[str] = Nothing
    results_are_suggestions: bool = False

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the property and collection definitions together."""

        def draw(parts: tuple[Resolved, Resolved]) -> RowDraw:
            held, items = parts

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for owner, key in located(held, holder, context).to_list():
                    for source, listed in located(items, holder, context).to_list():
                        layout.prop_search(owner, key, source, listed, text=self.text.to_optional(), results_are_suggestions=self.results_are_suggestions)

            return shown

        return bound(
            ((self.target, lambda held: fits(held, (bpy.types.StringProperty, bpy.types.PointerProperty))), (self.collection, lambda held: fits(held, (bpy.types.CollectionProperty,)))),
            Ok(lambda held: Ok(held[0].map2(lambda target, collection: draw((target, collection)), held[1]))),
        )


class Selector(Line, frozen=True, kw_only=True):
    """Data-block or item pointer drawn through Blender's ID or search template."""

    target: Member | msgspec.structs.FieldInfo
    browse: DataBlocks | Items = DataBlocks()
    preview: Option[PreviewGrid] = Nothing
    label: Option[str] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves the pointer, the browsed collection, and the openers into the template's draw."""

        def named(opener: Opener) -> BlenderResult[str]:
            match opener:
                case operators.Command(key=key):
                    return identity.name(Name.OPERATOR, key)
                case _:
                    return Ok(opener.idname_py())  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

        def opened(opener: Option[Opener]) -> BlenderResult[str]:
            return opener.map(named).default_value(Ok(""))

        def blocks(chosen: Literal["ALL", "AVAILABLE"], ids: tuple[str, ...]) -> Template:
            new, open_, unlink = ids

            def shown(line: bpy.types.UILayout, owner: Owner, key: str, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
                match self.preview.to_list():
                    case [PreviewGrid(rows=rows, cols=cols)]:
                        line.template_ID_preview(owner, key, new=new, open=open_, unlink=unlink, filter=chosen, rows=rows, cols=cols)
                    case _:
                        line.template_ID(owner, key, new=new, open=open_, unlink=unlink, filter=chosen)

            return shown

        def searched(parts: tuple[Resolved, tuple[str, ...]]) -> Template:
            collection, (new, unlink) = parts

            def shown(line: bpy.types.UILayout, owner: Owner, key: str, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for items, listed in located(collection, holder, context).to_list():
                    match self.preview.to_list():
                        case [PreviewGrid(rows=rows, cols=cols)]:
                            line.template_search_preview(owner, key, items, listed, new=new, unlink=unlink, rows=rows, cols=cols)
                        case _:
                            line.template_search(owner, key, items, listed, new=new, unlink=unlink)

            return shown

        def draw(parts: tuple[Resolved, Template]) -> RowDraw:
            held, placed_template = parts

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                line = gridded(layout, self.label)
                for owner, key in located(held, holder, context).to_list():
                    placed_template(line, owner, key, holder, context)

            return shown

        match self.browse:
            case DataBlocks(new=new, open=open_, unlink=unlink, filter=chosen):
                template = accumulated(map(opened, (new, open_, unlink))).map(lambda ids: blocks(chosen, ids))
                return bound(
                    ((self.target, lambda held: fits(held, self.browse.kinds, (bpy.types.ID,))),), template.map(lambda template: lambda held: Ok(held[0].map(lambda target: draw((target, template)))))
                )
            case Items(collection=collection, new=new, unlink=unlink):
                openers = accumulated(map(opened, (new, unlink)))
                return bound(
                    ((self.target, lambda held: fits(held, self.browse.kinds, (bpy.types.ID,))), (collection, lambda held: fits(held, (bpy.types.CollectionProperty,)))),
                    openers.map(lambda ids: lambda held: Ok(held[0].map2(lambda target, collection: draw((target, searched((collection, ids)))), held[1]))),
                )


class Matrix(Line, frozen=True, kw_only=True):
    """Native setting cells across scoped owners, preserving supplied compound editors."""

    scope: Scoped
    settings: tuple[str, ...]
    compound: "tuple[tuple[type[bpy.types.bpy_struct[object]], Callable[[bpy.types.UILayout, Owner, str], None]], ...]" = ()
    action: Option[tuple[Registrable, Callable[[Identity, Descriptor], BlenderResult[RowDraw]]]] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves each setting into its value settings and the owners' name under the scope's owner type into the table's draw."""
        owner_type = self.scope.owner_type
        compound = tuple(kind for kind, _ in self.compound)
        named = Option.of_optional(owner_type.bl_rna.name_property).to_result_with(lambda: refused(owner_type.__name__, "name_property", (bpy.types.StringProperty.__name__,)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

        def cell(setting: Descriptor) -> BlenderResult[tuple[Descriptor, RowDraw]]:
            action = self.action.map(lambda policy: (policy[0], lambda at: policy[1](at, setting)))
            template = Option.of_optional(next((place for kind, place in self.compound if isinstance(setting.prop, bpy.types.PointerProperty) and isinstance(setting.prop.fixed_type, kind)), None))

            def composed(place: Callable[[bpy.types.UILayout, Owner, str], None], action: Option[RowDraw]) -> RowDraw:
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
        """Answers the tree's draw over the paths its source answers."""

        def listed(layout: bpy.types.UILayout, paths: tuple[PurePath, ...]) -> None:
            files = {path.parts for path in paths}
            for node in sorted({path.parts[:end] for path in paths for end in range(1, len(path.parts) + 1)}):
                line = layout.row(align=True)
                for _ in node[1:]:
                    line.label(text="", icon="BLANK1")
                line.label(text=node[-1], icon="FILE_BLANK" if node in files else "FILE_FOLDER")

        return Ok(lambda layout, _holder, context: alerted(layout, self.paths(context).map(lambda paths: listed(layout, paths))))


class TreeMatrix[R: msgspec.Struct, L: "bpy.types.bpy_struct[object]", C: "bpy.types.bpy_struct[object]"](Line, frozen=True, kw_only=True):
    """Tree rows indented by depth with one cell of operator toggles per column owner."""

    tree: Callable[[bpy.types.Context], tuple[tuple[str, ...], tuple[Branch[L, C], ...]]]
    command: operators.Operator[R]
    toggles: Callable[[str, C, L], tuple[Toggle[R], ...]]
    active: Option[Callable[[C], bool]] = Nothing
    marks: tuple[RowContent, ...] = ()

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves the operator row and realizes the label marks into the tree's draw."""

        def draw(parts: tuple[str, tuple[RowDraw, ...]]) -> RowDraw:
            idname, marks = parts

            def cell(line: bpy.types.UILayout, owner: str, found: C, label: L) -> None:
                line.active = all(read(found) for read in self.active.to_list())
                for toggle in self.toggles(owner, found, label):
                    alerted(
                        line,
                        operated(line.operator(idname, text="", icon=toggle.icon, depress=toggle.depress), idname).bind(
                            lambda properties: operators.Own(command=self.command, values=toggle.record).invocation(identity).bind(lambda placement: placement[1](properties))
                        ),
                    )

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

        return paired(identity.name(Name.OPERATOR, self.command.key), accumulated(realized(row, identity) for row in self.marks)).map(draw)


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
        """Resolves the target's operator id, writer, and call context into the button's draw."""
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
    """Native popover button for a stock panel or an extension panel key."""

    panel: type[bpy.types.Panel] | Key
    text: Option[str] = Nothing
    label: Option[str] = Nothing
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the native panel identifier and reads the glyph each draw."""
        named = identity.name(Name.PANEL, self.panel) if isinstance(self.panel, str) else Ok(self.panel.bl_rna.identifier)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return named.map(lambda name: lambda layout, _holder, _context: gridded(layout, self.label).popover(panel=name, text=self.text.to_optional(), icon_value=iconed(identity, self.icon)))


class Pulldown(Line, frozen=True, kw_only=True):
    """Native menu button for a stock menu or an extension menu key."""

    menu: type[bpy.types.Menu] | Key
    text: Option[str] = Nothing
    label: Option[str] = Nothing
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the native menu identifier and reads the glyph each draw."""
        named = identity.name(Name.MENU, self.menu) if isinstance(self.menu, str) else Ok(self.menu.bl_rna.identifier)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return named.map(lambda name: lambda layout, _holder, _context: gridded(layout, self.label).menu(name, text=self.text.to_optional(), icon_value=iconed(identity, self.icon)))


class Caption(Line, frozen=True, kw_only=True):
    """Native rows and callbacks grouped under a heading or a rule."""

    rows: tuple[RowContent, ...]
    heading: Option[str] = Nothing
    flow: Flow = Flow.STACK

    def framed(self, draws: tuple[RowDraw, ...]) -> RowDraw:
        """Returns the composed content within the caption's native arrangement."""
        draw = joined(draws)

        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            if self.heading.is_none():
                layout.separator(type="LINE")
            draw(self.flow.opened(layout.column(heading=self.heading.default_value(""))), holder, context)

        return shown

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires content on the enclosing native property grid."""
        return placed(self.rows, identity).map(self.framed)


class Label(Line, frozen=True, kw_only=True):
    """Fixed text or a live reading in native Markdown, header, or glyph form."""

    text: str | Callable[[bpy.types.Context], Option[str]]
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns drawing with current text and resource IDs at every redraw."""

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
    """Native string drawn in a multiline text box."""

    target: Binding
    initial_visible_lines: Annotated[int, msgspec.Meta(ge=1)] = 3
    placeholder: Option[str] = Nothing
    label: Option[str] = Nothing

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the string binding and its native owner rule."""

        def draw(held: Resolved) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for owner, key in located(held, holder, context).to_list():
                    gridded(layout, self.label).textbox(owner, key, initial_visible_lines=self.initial_visible_lines, placeholder=self.placeholder.to_optional())

            return shown

        return bound(((self.target, lambda held: fits(held, (bpy.types.StringProperty,), (inspect.StrType,))),), Ok(lambda held: Ok(held[0].map(draw))))


class Progress(Line, frozen=True, kw_only=True):
    """Native progress bars or rings over current task fractions."""

    runs: Callable[[bpy.types.Context], Iterable[Work]]
    type: Literal["BAR", "RING"] = "BAR"
    label: Option[str] = Nothing

    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Returns native drawing of every current task in source order."""

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for run in self.runs(context):
                gridded(layout, self.label).progress(text=run.text, factor=run.done, type=self.type)

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BINDING]


def refused(path: str, found: str, accepted: tuple[str, ...]) -> BlenderFaults:
    """Returns the typed refusal for a control's unsupported binding."""
    return (BlenderChoiceFault(path=path, identifier=found, accepted=accepted),)


def bound(
    bindings: tuple[tuple[Binding, Callable[[Resolved], BlenderResult[Resolved]]], ...], place: BlenderResult[Callable[[tuple[Option[Resolved], ...]], BlenderResult[Option[RowDraw]]]]
) -> BlenderResult[RowDraw]:
    """Acquires static bindings and resolves explicit sources at the native draw boundary."""

    def resolved(binding: Binding, admit: Callable[[Resolved], BlenderResult[Resolved]]) -> BlenderResult[Resolved]:
        match binding:
            case Member():
                return binding.resolved().bind(admit)
            case Across(scope=scope, path=path):
                return Descriptor.resolve(scope.owner_type, path, bpy.types.Property).map(lambda descriptor: ResolvedMember(descriptor=descriptor, scope=Some(scope))).bind(admit)
            case ResolvedMember() | msgspec.structs.FieldInfo():
                return admit(binding)

    def draw(placement: Callable[[bpy.types.Context], BlenderResult[Option[RowDraw]]]) -> RowDraw:
        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            result = placement(context)
            alerted(layout, result)
            for each in result.to_option().bind(lambda held: held).to_list():
                each(layout, holder, context)

        return shown

    if not any(isinstance(binding, Member) and binding.reach.is_some() for binding, _ in bindings):
        return paired(accumulated(resolved(binding, admit).map(Some) for binding, admit in bindings), place).bind(lambda parts: parts[1](parts[0])).map(lambda held: draw(lambda _context: Ok(held)))

    def reached(binding: Resolved | tuple[Member, Callable[[Resolved], BlenderResult[Resolved]]], context: bpy.types.Context) -> BlenderResult[Option[Resolved]]:
        match binding:
            case tuple():
                member, admit = binding
                return member.reach.bind(lambda source: source(context)).map(lambda root: member.resolved(root=Some(root)).bind(admit).map(Some)).default_value(Ok(Nothing))
            case ResolvedMember() | msgspec.structs.FieldInfo():
                return Ok(Some(binding))

    return paired(accumulated(Ok((binding, admit)) if isinstance(binding, Member) and binding.reach.is_some() else resolved(binding, admit) for binding, admit in bindings), place).map(
        lambda parts: draw(lambda context: accumulated(reached(binding, context) for binding in parts[0]).bind(parts[1]))
    )


def fits(held: Resolved, kinds: "tuple[type[bpy.types.bpy_struct[object]], ...]", forms: tuple[type[object], ...] = ()) -> BlenderResult[Resolved]:
    """Selects the native property or admitted field categories a control draws."""
    if isinstance(held, ResolvedMember):
        descriptor = held.descriptor
        prop = descriptor.prop
        target = prop.fixed_type if isinstance(prop, bpy.types.PointerProperty) else prop
        return Ok(held) if isinstance(prop, kinds) or isinstance(target, kinds) else Error(refused(descriptor.path, type(target).__name__, tuple(kind.__name__ for kind in kinds)))
    kind = inspect.type_info(held.type)
    while isinstance(kind, inspect.Metadata):
        kind = kind.type
    match kind:
        case (
            inspect.StructType(cls=record)
            | inspect.EnumType(cls=record)
            | inspect.CustomType(cls=record)
            | inspect.UnionType(types=(inspect.CustomType(cls=record), inspect.NoneType()) | (inspect.NoneType(), inspect.CustomType(cls=record)))
        ):
            accepted = issubclass(typing.get_origin(record) or record, forms)
        case _:
            accepted = isinstance(kind, forms)
    return Ok(held) if accepted else Error(refused(held.name, type(kind).__name__, tuple(form.__name__ for form in forms)))


def colored(held: Resolved) -> BlenderResult[Resolved]:
    """Selects native RNA color arrays or the domain RGB record."""
    if isinstance(held, ResolvedMember):
        match held.descriptor.prop:
            case bpy.types.FloatProperty(is_array=True, subtype="COLOR" | "COLOR_GAMMA"):
                return Ok(held)
            case prop:
                return Error(refused(held.descriptor.path, prop.subtype, ("COLOR", "COLOR_GAMMA")))
    return fits(held, (), (Rgb,))


def located(held: Resolved, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> Option[tuple[Owner, str]]:
    """Locates the holder-compatible, explicit, or contextual property owner."""
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
    """Compares captured native values and returns an indeterminate label or typed refusals."""
    return accumulated(Snapshot.capture(owner, Some((setting,))) for owner in owners).map(
        lambda values: Some(f"{setting.prop.name} —") if any(left != right for left, right in pairwise(values)) else Nothing
    )


# --- [DRAWING]


def iconed(identity: Identity, icon: Option[Glyph]) -> int:
    """Reads the current native glyph ID, or Blender's NONE icon."""
    return icon.map(lambda glyph: Icons.value(identity, glyph)).default_value(0)


def operated(properties: bpy.types.OperatorProperties | None, idname: str) -> BlenderResult[bpy.types.OperatorProperties]:
    """Converts the native absent operator-item answer at the layout boundary."""
    return Option.of_optional(properties).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.ops", key=idname),))


def split(layout: bpy.types.UILayout) -> tuple[bpy.types.UILayout, bpy.types.UILayout]:
    """Places label and value cells beside Blender's reserved decorator cell."""
    line = layout.row(align=True)
    halves = line.split(factor=layout.property_split_factor, align=True)
    names, values = halves.column(align=True), halves.row(align=True)
    values.use_property_split = False
    values.use_property_decorate = False
    if layout.use_property_decorate:
        line.label(text="", icon="BLANK1")
    return names, values


def gridded(layout: bpy.types.UILayout, label: Option[str]) -> bpy.types.UILayout:
    """Places an explicit label on the native property grid when requested."""
    match label:
        case Option(tag="some", some=text):
            names, values = split(layout)
            names.alignment = "RIGHT"
            names.label(text=text)
            return values
        case _:
            return layout


def alerted[T](layout: bpy.types.UILayout, result: BlenderResult[T]) -> None:
    """Renders complete callback failures as native alert labels."""
    for faults in result.swap().to_option().to_list():
        column = layout.column(align=True)
        column.alert = True
        for _, text in rendered(faults):
            column.label(text=text, icon="STATUS_ERROR_FILLED")


def invoking(draw: RowDraw, mode: "rna_enums.OperatorContextItems" = "INVOKE_REGION_WIN") -> RowDraw:
    """Scopes native invocation context around drawing, restoring it on every exit."""

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
    """Retains open resource declarations and the capabilities nested in composed rows."""
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
    """Acquires a typed row or directly composes a native holder-aware callback."""
    match row:
        case tuple():
            return row[1](identity)
        case Row():
            return row.drawn(identity).map(lambda draw: guarded(draw, row.condition) if isinstance(row, Line) else draw)
        case Callable():
            return Ok(row)


def guarded(draw: RowDraw, condition: Option[Condition]) -> RowDraw:
    """Returns a native inactive column only where the row declares a condition."""

    def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
        for required in condition.to_list():
            layout = layout.column()
            layout.active = holds(required, context, Some(holder))
        draw(layout, holder, context)

    return shown


def placed(rows: tuple[RowContent, ...], identity: Identity, *, inherited: bool = False) -> BlenderResult[tuple[RowDraw, ...]]:
    """Acquires independent rows and native callbacks, accumulating every refusal."""

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
    """Draws acquired content in declaration order over one native holder."""

    def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
        for draw in draws:
            draw(layout, holder, context)

    return shown


def painted(draws: tuple[RowDraw, ...]) -> SurfaceDraw:
    """Uses the surface context as the holder of acquired row drawings."""
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
    "Owner",
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
    "realized",
    "refused",
    "split",
]
