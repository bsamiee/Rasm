"""Layout rows over UILayout functions, from typed declarations or callbacks."""

from collections.abc import Callable, Iterable
from enum import auto, Enum
from itertools import pairwise, product, starmap
from pathlib import PurePath
import typing
from typing import Annotated, ClassVar, Final, Generic, overload, override, Protocol, runtime_checkable, Self

import _bpy_types
import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
from msgspec import UNSET, UnsetType

from blender.bpy.commands import operators
from blender.bpy.commands.calls import idname, Operation, Placed, ShelfPopup, Writer
from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import Descriptor, Native, P_co, root_in
from blender.bpy.data.rna.snapshots import Scoped, Snapshot
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.settings import shapes
from blender.bpy.ui.resources.icons import Glyph, Icons
from blender.records.faults import accumulated, DomainFault, paired
from blender.records.fields import Unit

if typing.TYPE_CHECKING:
    from bpy.stub_internal import rna_enums
    from bpy.stub_internal.rna_enums import IconItems

# --- [TYPES] ----------------------------------------------------------------------------


type At = tuple[Native, str]
type RowDraw = Callable[[bpy.types.UILayout, bpy.types.bpy_struct[object], bpy.types.Context], None]
type SurfaceDraw = Callable[[bpy.types.UILayout, bpy.types.Context], None]
type Reach = Callable[[bpy.types.Context], Option[bpy.types.bpy_struct[object]]]
type Binding = Member | Across
type Accept = Callable[[ResolvedMember], BlenderResult[ResolvedMember]]
type Opener = operators.Command | Operation


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


# --- [CONSTANTS] ------------------------------------------------------------------------

FUNCTIONS: "Final[bpy.types.bpy_prop_collection[bpy.types.Function]]" = bpy.types.UILayout.bl_rna.functions  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

# --- [MODELS] ---------------------------------------------------------------------------


class ResolvedMember(msgspec.Struct, Generic[P_co], frozen=True, kw_only=True):
    """RNA definition with its live root source and declared record shape."""

    descriptor: Descriptor[P_co]
    reach: "bpy.types.bpy_struct[object] | Option[Reach]" = Nothing
    scope: Option[Scoped] = Nothing
    shape: Option[shapes.Shape] = Nothing

    def at(self, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> Option[At]:
        """Locates property owner through fixed root, drawing holder, reach, or context."""
        descriptor = self.descriptor
        match self.reach:
            case bpy.types.bpy_struct() as fixed:
                root: Option[Native] = Some(fixed)
            case _ if isinstance(holder, descriptor.root):
                root = Some(holder)
            case Option() as reach:
                root = reach.map(lambda source: source(context)).default_with(lambda: root_in(descriptor.root, context))
        return root.bind(descriptor.owner_of).map(lambda owner: (owner, descriptor.prop.identifier))

    def accepts(self, kinds: "tuple[type[bpy.types.bpy_struct[object]], ...]", forms: tuple[type[object], ...] = ()) -> BlenderResult[Self]:
        """Accepts property or pointer target kinds, or declared record shapes, enum classes included."""
        prop = self.descriptor.prop
        target = prop.fixed_type if isinstance(prop, bpy.types.PointerProperty) else prop
        shaped = self.shape.filter(lambda shape: isinstance(shape, forms) or (isinstance(shape, shapes.Enumerated) and issubclass(shape.enum, forms)))
        return (
            Ok(self)
            if isinstance(prop, kinds) or isinstance(target, kinds) or shaped.is_some()
            else Error(refused(self.descriptor.path, type(target).__name__, tuple(kind.__name__ for kind in (*kinds, *forms))))
        )


class Member(msgspec.Struct, frozen=True, kw_only=True):
    """Property path rooted at its owner type, record fields carrying their declared shape."""

    root: "type[bpy.types.bpy_struct[object]]"
    path: str
    reach: Option[Reach] = Nothing
    shape: Option[shapes.Shape] = Nothing

    @overload
    def resolved(self, *, root: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> BlenderResult[ResolvedMember]: ...

    @overload
    def resolved[P: bpy.types.Property](self, kind: type[P], *, root: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> BlenderResult[ResolvedMember[P]]: ...

    def resolved(self, kind: type[bpy.types.Property] = bpy.types.Property, *, root: "Option[bpy.types.bpy_struct[object]]" = Nothing) -> BlenderResult[ResolvedMember]:
        """Resolves metadata on root type, or on live `root` when given."""
        return root.map(
            lambda actual: (
                Descriptor.resolve(actual, self.path, kind).map(lambda descriptor: ResolvedMember(descriptor=descriptor, reach=actual, shape=self.shape))
                if isinstance(actual, self.root)
                else Error(refused(self.path, type(actual).__name__, (self.root.__name__,)))
            )
        ).default_with(lambda: Descriptor.resolve(self.root, self.path, kind).map(lambda descriptor: ResolvedMember(descriptor=descriptor, reach=self.reach, shape=self.shape)))


class Across(msgspec.Struct, frozen=True, kw_only=True):
    """Host member read across every owner of an edit scope."""

    scope: Scoped
    path: str

    def resolved(self) -> BlenderResult[ResolvedMember]:
        """Resolves metadata on the scope's owner type."""
        return Descriptor.resolve(self.scope.owner_type, self.path, bpy.types.Property).map(lambda descriptor: ResolvedMember(descriptor=descriptor, scope=Some(self.scope)))


class PreviewGrid(msgspec.Struct, frozen=True, kw_only=True):
    """Selector preview grid, unset counts taking Blender's."""

    rows: Annotated[int, msgspec.Meta(ge=0)] | UnsetType = UNSET
    cols: Annotated[int, msgspec.Meta(ge=0)] | UnsetType = UNSET


class DataBlocks(msgspec.Struct, frozen=True, kw_only=True):
    """Data-block browse through `template_ID`."""

    kinds: "ClassVar[tuple[type[bpy.types.bpy_struct[object]], ...]]" = (bpy.types.ID,)
    new: Option[Opener] = Nothing
    open: Option[Opener] = Nothing
    unlink: Option[Opener] = Nothing
    filter: str | UnsetType = UNSET


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
    """Row with optional availability condition, drawn inactive while it fails."""

    condition: Option[Condition] = Nothing

    def guarded(self, draw: RowDraw) -> RowDraw:
        """Wraps draw in column inactive while condition fails."""

        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for required in self.condition.to_list():
                layout = layout.column()
                layout.active = holds(required, context, Some(holder))
            draw(layout, holder, context)

        return shown


class Call(Line, frozen=True, kw_only=True):
    """Row over one UILayout function, fields named after its parameters passing their scalar values, unset fields leaving host defaults."""

    function: ClassVar[bpy.types.Function]

    @property
    def options(self) -> dict[str, object]:
        """Set scalar fields the function names as parameters."""
        parameters = self.function.parameters
        return {name: value for name in self.__struct_fields__ if name in parameters and isinstance(value := getattr(self, name), bool | int | float | str)}

    def called(self, layout: bpy.types.UILayout, *args: object, **options: object) -> object:
        """Calls the function on layout, the one boundary call of every function row."""
        return getattr(layout, self.function.identifier)(*args, **options)

    def drawn(self, _identity: Identity, /) -> BlenderResult[RowDraw]:
        """Draws the call over no bound member."""
        options = self.options

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
            self.called(layout, **options)

        return Ok(shown)


class Bound(Call, frozen=True, kw_only=True):
    """Call over one bound RNA member, resolved once and located on each draw."""

    target: Binding
    kinds: "ClassVar[tuple[type[bpy.types.bpy_struct[object]], ...]]" = (bpy.types.Property,)
    forms: ClassVar[tuple[type[object], ...]] = ()

    def accepted(self, held: ResolvedMember) -> BlenderResult[ResolvedMember]:
        """Accepts the target the row's function draws."""
        return held.accepts(self.kinds, self.forms)

    def values(self, _identity: Identity, /) -> BlenderResult[dict[str, object]]:
        """Host values the row resolves once at acquisition."""
        return Ok({})

    def placed(self, layout: bpy.types.UILayout, at: At, _context: bpy.types.Context, options: dict[str, object], /) -> None:
        """Calls the function over the located member."""
        self.called(layout, *at, **options)

    @override
    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Binds target, drawing at each located owner."""

        def draw(options: dict[str, object]) -> BlenderResult[RowDraw]:
            def place(held: tuple[ResolvedMember, ...]) -> RowDraw:
                def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                    for at in held[0].at(holder, context).to_list():
                        self.placed(layout, at, context, options)

                return shown

            return bound(((self.target, self.accepted),), place)

        return self.values(identity).map(lambda resolved: self.options | resolved).bind(draw)


class Property(Bound, frozen=True, kw_only=True):
    """Setting drawn with its RNA widget, gated, flag, and component records as grouped widgets."""

    function = FUNCTIONS["prop"]
    kinds = (bpy.types.BoolProperty, bpy.types.IntProperty, bpy.types.FloatProperty, bpy.types.StringProperty, bpy.types.EnumProperty, bpy.types.ID)
    forms = (shapes.Gate, shapes.Flags, shapes.Components)
    text: str | UnsetType = UNSET
    icon_only: bool | UnsetType = UNSET
    expand: bool | UnsetType = UNSET
    slider: bool | UnsetType = UNSET
    placeholder: str | UnsetType = UNSET
    icon: Option[Glyph] = Nothing
    action: Option[RowContent] = Nothing

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires action row registrations."""
        return Ok(acquired(self.action.to_list()))

    @override
    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Binds setting, glyph, and action once for every redraw."""
        options = {key: value for key, value in self.options.items() if key != "text"}

        def headed(owner: Native, key: str, text: str | UnsetType) -> str:
            return text if isinstance(text, str) else owner.bl_rna.properties[key].name  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

        def valued(layout: bpy.types.UILayout, owner: Native, key: str, shape: Option[shapes.Shape], text: str | UnsetType, icon: int, index: int = -1) -> None:
            match shape.to_optional():
                case shapes.Gate(value=value):
                    group = getattr(owner, key)
                    gate = layout.row(heading=headed(owner, key, text), align=True)
                    gate.prop(group, shapes.ENABLED, text="")
                    inner = gate.row(align=True)
                    inner.active = getattr(group, shapes.ENABLED)
                    for field, held in value.to_list():
                        valued(inner, group, field.name, Some(held.shape), "", icon)
                case shapes.Flags(enum=choices):
                    group = getattr(owner, key)
                    inner = layout.column(heading=headed(owner, key, text), align=True)
                    for member in choices:
                        inner.prop(group, member.name)
                case shapes.Components(items=items):
                    group = getattr(owner, key)
                    inner = layout.column(heading=headed(owner, key, text), align=True)
                    inherited = bpy.types.PropertyGroup.bl_rna.properties
                    for prop, item in zip((prop for prop in group.bl_rna.properties if prop.identifier not in inherited), items):
                        valued(inner, group, prop.identifier, Some(item.shape), UNSET, icon)
                case _:
                    self.called(layout, owner, key, **options, index=index, icon_value=icon, **({} if isinstance(text, UnsetType) else {"text": text}))

        def draw(held: ResolvedMember, action: Option[RowDraw], icon: int) -> RowDraw:
            index = held.descriptor.index.default_value(-1)

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                line = layout.row() if action.is_some() else layout
                labelled = held.scope.map(lambda scope: scope.owners(context).bind(lambda owners: marked(held.descriptor, owners))).default_value(Ok(Nothing))
                for owner, key in held.at(holder, context).to_list():
                    alerted(line, labelled.map(lambda label: valued(line, owner, key, held.shape, label.default_value(self.text), icon, index)))
                    for extra in action.to_list():
                        extra(line, holder, context)

            return shown

        return paired(self.action.map(lambda row: realized(row, identity).map(Some)).default_value(Ok(Nothing)), iconed(identity, self.icon)).bind(
            lambda parts: bound(((self.target, self.accepted),), lambda held: draw(held[0], *parts))
        )


class PropertyPopover(Bound, frozen=True, kw_only=True):
    """Setting beside a button opening a stock or extension panel as popover."""

    function = FUNCTIONS["prop_with_popover"]
    panel: type[_bpy_types.Panel] | Key
    text: str | UnsetType = UNSET
    icon: "IconItems | UnsetType" = UNSET
    icon_only: bool | UnsetType = UNSET

    @override
    def values(self, identity: Identity, /) -> BlenderResult[dict[str, object]]:
        """Resolves panel idname."""
        return idname(identity, Name.PANEL, self.panel).map(lambda panel: {"panel": panel})


class PopoverGroup(Call, frozen=True, kw_only=True):
    """Popover buttons of every panel registered in one space, region, context, and category."""

    function = FUNCTIONS["popover_group"]
    space_type: "rna_enums.SpaceTypeItems | UnsetType" = UNSET
    region_type: "rna_enums.RegionTypeItems | UnsetType" = UNSET
    context: str | UnsetType = UNSET
    category: str | UnsetType = UNSET


class SearchField(Call, frozen=True, kw_only=True):
    """String or pointer search over declared RNA collection."""

    function = FUNCTIONS["prop_search"]
    target: Member
    collection: Member
    text: str | UnsetType = UNSET
    results_are_suggestions: bool | UnsetType = UNSET

    @override
    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Binds property and collection together."""
        options = self.options

        def draw(parts: tuple[ResolvedMember, ...]) -> RowDraw:
            held, items = parts

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for (owner, key), (source, listed) in product(held.at(holder, context).to_list(), items.at(holder, context).to_list()):
                    self.called(layout, owner, key, source, listed, **options)

            return shown

        return bound(
            ((self.target, lambda held: held.accepts((bpy.types.StringProperty, bpy.types.PointerProperty))), (self.collection, lambda held: held.accepts((bpy.types.CollectionProperty,)))), draw
        )


class Selector(Line, frozen=True, kw_only=True):
    """Data-block or item pointer in ID or search template, the browse's fields passing as the template's opener and filter values."""

    target: Member
    browse: DataBlocks | Items = DataBlocks()
    preview: Option[PreviewGrid] = Nothing
    label: Option[str] = Nothing

    @property
    def function(self) -> bpy.types.Function:
        """Template function of the browse and preview form."""
        match self.browse:
            case DataBlocks():
                return FUNCTIONS["template_ID_preview" if self.preview.is_some() else "template_ID"]
            case Items():
                return FUNCTIONS["template_search_preview" if self.preview.is_some() else "template_search"]

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires extension openers."""
        return Ok(tuple(opener for value in msgspec.structs.asdict(self.browse).values() if isinstance(value, Option) for opener in value.to_list() if isinstance(opener, Registrable)))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves pointer, collection, and openers into template draw."""
        template = self.function.identifier
        grid = {key: value for preview in self.preview.to_list() for key, value in msgspec.structs.asdict(preview).items() if value is not UNSET}

        def valued(name: str, value: object) -> BlenderResult[Option[tuple[str, object]]]:
            match value:
                case Option(tag="some", some=operators.Command(key=key)):
                    return identity.name(Name.OPERATOR, key).map(lambda found: Some((name, found)))
                case Option(tag="some", some=operation):
                    return Ok(Some((name, operation.idname_py())))
                case str():
                    return Ok(Some((name, value)))
                case _:
                    return Ok(Nothing)

        def draw(options: dict[str, object]) -> Callable[[tuple[ResolvedMember, ...]], RowDraw]:
            def place(held: tuple[ResolvedMember, ...]) -> RowDraw:
                def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                    line = gridded(layout, self.label)
                    for at in held[0].at(holder, context).to_list():
                        for searched in product(*(each.at(holder, context).to_list() for each in held[1:])):
                            getattr(line, template)(*at, *(part for pair in searched for part in pair), **options)

                return shown

            return place

        bindings: tuple[tuple[Binding, Accept], ...] = (
            (self.target, lambda held: held.accepts(self.browse.kinds, (shapes.Pointer,))),
            *(((self.browse.collection, lambda held: held.accepts((bpy.types.CollectionProperty,))),) if isinstance(self.browse, Items) else ()),
        )
        return (
            accumulated(starmap(valued, msgspec.structs.asdict(self.browse).items()))
            .map(lambda pairs: dict(pair for found in pairs for pair in found.to_list()) | grid)
            .bind(lambda options: bound(bindings, draw(options)))
        )


class Matrix(Line, frozen=True, kw_only=True):
    """Setting cells across scoped owners, compound settings through caller editors."""

    scope: Scoped
    settings: tuple[str, ...]
    compound: "tuple[tuple[type[bpy.types.bpy_struct[object]], Callable[[bpy.types.UILayout, Native, str], None]], ...]" = ()
    action: Option[tuple[Registrable, Callable[[Identity, Descriptor], BlenderResult[RowDraw]]]] = Nothing

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires action owner."""
        return Ok(tuple(owner for owner, _ in self.action.to_list()))

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
                    for owner, key in ResolvedMember(descriptor=setting).at(holder, context).to_list():
                        for draw in action.to_list():
                            draw(layout.row(align=True), holder, context)
                        place(layout, owner, key)

                return shown

            return (
                template
                .map(lambda place: action.map(lambda row: realized(row, identity).map(Some)).default_value(Ok(Nothing)).map(lambda draw: composed(place, draw)))
                .default_with(lambda: Property(target=Member(root=owner_type, path=setting.path), text="", action=action).drawn(identity))
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

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires toggle command and mark rows."""
        return Ok((self.command, *acquired(self.marks)))

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


class OperatorButton(Call, frozen=True, kw_only=True):
    """Operator button over a keymap target."""

    function = FUNCTIONS["operator"]
    target: Placed
    text: str | UnsetType = UNSET
    emboss: bool | UnsetType = UNSET
    depress: bool | UnsetType = UNSET
    search_weight: float | UnsetType = UNSET
    icon: Option[Glyph] = Nothing
    label: Option[str] = Nothing
    context: "rna_enums.OperatorContextItems" = "INVOKE_REGION_WIN"

    @property
    def command(self) -> Placed:
        """Placed command, an owned placement's operator."""
        return self.target.command if isinstance(self.target, operators.Own) else self.target

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires extension command."""
        return Ok(tuple(command for command in (self.command,) if isinstance(command, Registrable)))

    @override
    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves operator idname, writer, and call context into button draw."""
        options = self.options
        match self.command:
            case operators.Operator() as command:
                glyph, invoked = self.icon.or_else(command.icon), command.invoked
            case operators.Macro() as macro:
                glyph, invoked = self.icon.or_else(macro.icon), True
            case _:
                glyph, invoked = self.icon, True

        def draw(parts: tuple[tuple[str, Writer], int]) -> RowDraw:
            (operator, write), icon = parts

            def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
                line = gridded(layout, self.label)
                alerted(line, operated(self.called(line, operator, **options, icon_value=icon), operator).bind(write))

            return invoking(shown, self.context) if invoked else shown

        return paired(self.target.invocation(identity), iconed(identity, glyph)).map(draw)


class Opens(Call, frozen=True, kw_only=True):
    """Button opening a stock or extension panel, menu, or shelf by key or class, on the property grid when labelled."""

    label: Option[str] = Nothing
    icon: Option[Glyph] = Nothing

    def opened(self, identity: Identity, /) -> BlenderResult[str]:
        """Resolves the opened idname."""
        raise NotImplementedError

    @override
    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves idname into the function's leading parameter and glyph once."""

        def draw(options: dict[str, object]) -> RowDraw:
            def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
                self.called(gridded(layout, self.label), **options)

            return shown

        return paired(self.opened(identity), iconed(identity, self.icon)).map(lambda parts: draw(self.options | {self.function.parameters[0].identifier: parts[0], "icon_value": parts[1]}))


class Popover(Opens, frozen=True, kw_only=True):
    """Popover button over a panel."""

    function = FUNCTIONS["popover"]
    panel: type[_bpy_types.Panel] | Key
    text: str | UnsetType = UNSET

    @override
    def opened(self, identity: Identity, /) -> BlenderResult[str]:
        """Resolves panel idname."""
        return idname(identity, Name.PANEL, self.panel)


class Pulldown(Opens, frozen=True, kw_only=True):
    """Menu button over a menu."""

    function = FUNCTIONS["menu"]
    menu: type[_bpy_types.Menu] | Key
    text: str | UnsetType = UNSET

    @override
    def opened(self, identity: Identity, /) -> BlenderResult[str]:
        """Resolves menu idname."""
        return idname(identity, Name.MENU, self.menu)


class ShelfPopover(Opens, frozen=True, kw_only=True):
    """Asset shelf popover button."""

    function = FUNCTIONS["template_asset_shelf_popover"]
    popup: ShelfPopup
    name: str | UnsetType = UNSET

    @override
    def opened(self, identity: Identity, /) -> BlenderResult[str]:
        """Resolves shelf idname."""
        return idname(identity, Name.SHELF, self.popup.shelf)


class Divider(Call, frozen=True, kw_only=True):
    """Gap or line between rows, a vertical bar in headers."""

    function = FUNCTIONS["separator"]
    type: str | UnsetType = "LINE"
    factor: Annotated[float, msgspec.Meta(ge=0.0)] | UnsetType = UNSET


class Spacer(Call, frozen=True, kw_only=True):
    """Header split pushing later rows to the far edge, each side drawn as its own section."""

    function = FUNCTIONS["separator_spacer"]


class Sizing(msgspec.Struct, frozen=True, kw_only=True):
    """Layout scale and fixed width or height in interface units, zero units leaving the size to content."""

    scale_x: Annotated[float, msgspec.Meta(gt=0.0)] = 1.0
    scale_y: Annotated[float, msgspec.Meta(gt=0.0)] = 1.0
    ui_units_x: Annotated[float, msgspec.Meta(ge=0.0)] = 0.0
    ui_units_y: Annotated[float, msgspec.Meta(ge=0.0)] = 0.0

    def applied(self, layout: bpy.types.UILayout) -> bpy.types.UILayout:
        """Writes sizes onto layout, returning it."""
        layout.scale_x, layout.scale_y, layout.ui_units_x, layout.ui_units_y = self.scale_x, self.scale_y, self.ui_units_x, self.ui_units_y
        return layout


class Caption(Line, frozen=True, kw_only=True):
    """Rows grouped under heading after separator, in caption flow and sizing."""

    rows: tuple[RowContent, ...]
    heading: Option[str] = Nothing
    flow: Flow = Flow.STACK
    separator: Option[Divider | Spacer] = Some(Divider())
    sizing: Sizing = Sizing()

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires registrable rows."""
        return Ok(acquired(self.rows))

    def framed(self, draws: tuple[RowDraw, ...]) -> RowDraw:
        """Returns joined draws in caption arrangement."""
        draw = joined(draws)

        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for separator in self.separator.to_list():
                separator.called(layout, **separator.options)
            draw(self.flow.opened(self.sizing.applied(layout.column(heading=self.heading.default_value("")))), holder, context)

        return shown

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Places rows on enclosing property grid."""
        return placed(self.rows, identity).map(self.framed)


class Rows(Line, frozen=True, kw_only=True):
    """Rows built from the extension identity on one grid, hidden instead of inactive while condition fails when declared."""

    rows: Callable[[Identity], BlenderResult[tuple[RowContent, ...]]]
    hidden: bool = False

    @override
    def guarded(self, draw: RowDraw) -> RowDraw:
        """Hides draw while condition fails when declared, else draws it inactive."""
        if not self.hidden:
            return super().guarded(draw)

        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            if all(holds(required, context, Some(holder)) for required in self.condition.to_list()):
                draw(layout, holder, context)

        return shown

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Places identity rows on one grid."""
        return self.rows(identity).bind(lambda rows: placed(rows, identity)).map(joined)


class Label(Line, frozen=True, kw_only=True):
    """Fixed or live text with glyph wrapped to the layout, one line in headers, Markdown without glyph when declared."""

    text: str | Callable[[bpy.types.Context], Option[str]]
    icon: Option[Glyph] = Nothing
    markdown: bool = False

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Resolves glyph once, returning draw reading text each redraw."""

        def draw(icon: int) -> RowDraw:
            def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for text in (Some(self.text) if isinstance(self.text, str) else self.text(context)).to_list():
                    if Option.of_optional(context.region).filter(lambda region: region.type in {"HEADER", "TOOL_HEADER"}).is_some():
                        layout.label(text=text, icon_value=icon)
                    elif self.markdown:
                        layout.label_markdown(text=text)
                    else:
                        layout.label_multiline(text=text, icon_value=icon)

            return shown

        return iconed(identity, self.icon).map(draw)


class Text(Bound, frozen=True, kw_only=True):
    """String drawn in multiline text box."""

    function = FUNCTIONS["textbox"]
    kinds = (bpy.types.StringProperty,)
    text: str | UnsetType = UNSET
    initial_visible_lines: Annotated[int, msgspec.Meta(ge=1)] | UnsetType = UNSET
    placeholder: str | UnsetType = UNSET
    label: Option[str] = Nothing

    @override
    def placed(self, layout: bpy.types.UILayout, at: At, _context: bpy.types.Context, options: dict[str, object], /) -> None:
        """Draws text box on the property grid when labelled."""
        self.called(gridded(layout, self.label), *at, **options)


class Progress(Call, frozen=True, kw_only=True):
    """Progress bars or rings over running work fractions."""

    function = FUNCTIONS["progress"]
    runs: Callable[[bpy.types.Context], Iterable[Work]]
    type: str | UnsetType = UNSET
    label: Option[str] = Nothing

    @override
    def drawn(self, _identity: Identity) -> BlenderResult[RowDraw]:
        """Returns draw of every running work."""
        options = self.options

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for run in self.runs(context):
                self.called(gridded(layout, self.label), **options, text=run.text, factor=run.done)

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BINDING]


def refused(path: str, found: str, accepted: tuple[str, ...]) -> BlenderFaults:
    """Returns choice fault for an unsupported binding."""
    return (DomainFault(subject=path, value=found, accepted=", ".join(accepted)),)


def bound(bindings: tuple[tuple[Binding, Accept], ...], place: Callable[[tuple[ResolvedMember, ...]], RowDraw]) -> BlenderResult[RowDraw]:
    """Resolves static bindings once and reached members each draw, drawing nothing while one is absent."""

    def reader(binding: Binding, accept: Accept) -> BlenderResult[Callable[[bpy.types.Context], BlenderResult[Option[ResolvedMember]]]]:
        match binding:
            case Member(reach=Option(tag="some", some=reach)):
                return Ok(lambda context: reach(context).map(lambda root: binding.resolved(root=Some(root)).bind(accept).map(Some)).default_value(Ok(Nothing)))
            case _:
                return binding.resolved().bind(accept).map(lambda held: lambda _context: Ok(Some(held)))

    def draw(readers: tuple[Callable[[bpy.types.Context], BlenderResult[Option[ResolvedMember]]], ...]) -> RowDraw:
        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            found = accumulated(read(context) for read in readers)
            alerted(layout, found)
            for held in found.to_option().to_list():
                present = tuple(value for each in held for value in each.to_list())
                if len(present) == len(held):
                    place(present)(layout, holder, context)

        return shown

    if any(isinstance(binding, Member) and binding.reach.is_some() for binding, _ in bindings):
        return accumulated(starmap(reader, bindings)).map(draw)
    return accumulated(binding.resolved().bind(accept) for binding, accept in bindings).map(place)


def marked(setting: Descriptor, owners: "tuple[bpy.types.bpy_struct[object], ...]") -> BlenderResult[Option[str]]:
    """Returns mixed-value label when owner values differ."""
    return accumulated(Snapshot.capture(owner, Some((setting,))) for owner in owners).map(
        lambda values: Some(f"{setting.prop.name} —") if any(left != right for left, right in pairwise(values)) else Nothing
    )


# --- [DRAWING]


def iconed(identity: Identity, icon: Option[Glyph]) -> BlenderResult[int]:
    """Resolves glyph icon ID, 0 leaving the RNA icon, absent fault for a glyph the host or icon set lacks."""
    return icon.map(lambda glyph: Icons.value(identity, glyph)).default_value(Ok(0))


def operated(properties: object, operator: str) -> BlenderResult[bpy.types.OperatorProperties]:
    """Maps missing operator properties to absent fault."""
    return Ok(properties) if isinstance(properties, bpy.types.OperatorProperties) else Error((BlenderAbsentFault(owner="bpy.ops", key=operator),))


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


def acquired(rows: Iterable[RowContent]) -> Acquisitions:
    """Registrable rows and builder owners, each row registering what it nests."""
    return tuple(owner for row in rows for owner in ((row[0],) if isinstance(row, tuple) else (row,) if isinstance(row, Registrable) else ()))


def realized(row: RowContent, identity: Identity) -> BlenderResult[RowDraw]:
    """Acquires typed row draw guarded by its condition, passing callbacks through."""
    match row:
        case tuple():
            return row[1](identity)
        case Row():
            return row.drawn(identity).map(lambda draw: row.guarded(draw) if isinstance(row, Line) else draw)
        case Callable():
            return Ok(row)


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
                return placed(row.rows, identity, inherited=picked).map(row.framed).map(row.guarded)
            case Property(action=action) if picked and action.is_none():
                return row.drawn(identity).map(blanked).map(row.guarded)
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
    "Accept",
    "Across",
    "At",
    "Binding",
    "Bound",
    "Branch",
    "Call",
    "Caption",
    "DataBlocks",
    "Divider",
    "Flow",
    "Items",
    "Label",
    "Line",
    "Matrix",
    "Member",
    "Opener",
    "Opens",
    "OperatorButton",
    "Popover",
    "PopoverGroup",
    "PreviewGrid",
    "Progress",
    "Property",
    "PropertyPopover",
    "Pulldown",
    "Reach",
    "ResolvedMember",
    "Row",
    "RowContent",
    "RowDraw",
    "Rows",
    "SearchField",
    "Selector",
    "ShelfPopover",
    "Sizing",
    "Spacer",
    "SurfaceDraw",
    "Text",
    "Toggle",
    "Tree",
    "TreeMatrix",
    "Work",
    "acquired",
    "alerted",
    "bound",
    "gridded",
    "iconed",
    "invoking",
    "joined",
    "operated",
    "painted",
    "placed",
    "pressed",
    "realized",
    "refused",
    "split",
]
