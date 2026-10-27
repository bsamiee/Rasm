"""Layout rows over UILayout functions, from typed declarations or callbacks."""

from collections.abc import Callable, Iterable, Mapping
from enum import auto, Enum
from functools import partial
from itertools import pairwise, product, starmap
from pathlib import PurePath
import typing
from typing import Annotated, ClassVar, Final, Generic, overload, override, Protocol, runtime_checkable, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
import msgspec
from msgspec import UNSET, UnsetType

from blender.bpy.commands import operators
from blender.bpy.commands.calls import dotted, idname, Operation, Placed, ShelfPopup, Writer
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor, Native, P_co, rna, RnaPath, root_in, Struct, Value
from blender.bpy.data.rna.snapshots import Scoped, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.resources.icons import Glyph, Icons
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Positive, Unit

if typing.TYPE_CHECKING:
    from bpy.stub_internal import rna_enums
    from bpy.stub_internal.rna_enums import IconItems

# --- [TYPES] ----------------------------------------------------------------------------


type At = tuple[Native, str]
type RowDraw = Callable[[bpy.types.UILayout, Struct, bpy.types.Context], None]
type SurfaceDraw = Callable[[bpy.types.UILayout, bpy.types.Context], None]
type Reach = Callable[[bpy.types.Context], Option[Struct]]
type Binding = Member | Across
type Accept = Callable[[ResolvedMember], Outcome[ResolvedMember]]
type Opener = operators.Command | Operation


@runtime_checkable
class Row(Protocol):
    """Layout declaration acquiring its draw."""

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Acquires draw or every refusal."""
        ...


type RowContent = Row | RowDraw | tuple[Registrable, Callable[[Identity], Outcome[RowDraw]]]


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
    """RNA definition with its live root source."""

    descriptor: Descriptor[P_co]
    reach: Struct | Option[Reach] = Nothing
    scope: Option[Scoped] = Nothing

    def at(self, holder: Struct, context: bpy.types.Context) -> Option[At]:
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

    def accepts(self, kinds: tuple[type[Struct], ...]) -> Outcome[Self]:
        """Accepts property or pointer target kinds."""
        prop = self.descriptor.prop
        target = prop.fixed_type if isinstance(prop, bpy.types.PointerProperty) else prop
        return Ok(self).filter_with(
            lambda _: isinstance(prop, kinds) or isinstance(target, kinds), lambda _: refused(self.descriptor.path, type(target).__name__, tuple(kind.__name__ for kind in kinds))
        )


class Member(msgspec.Struct, frozen=True, kw_only=True):
    """Property path rooted at its owner type."""

    root: type[Struct]
    path: RnaPath
    reach: Option[Reach] = Nothing

    @overload
    def resolved(self, *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember]: ...

    @overload
    def resolved[P: bpy.types.Property](self, kind: type[P], *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember[P]]: ...

    def resolved(self, kind: type[bpy.types.Property] = bpy.types.Property, *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember]:
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
    path: RnaPath

    def resolved(self) -> Outcome[ResolvedMember]:
        """Resolves metadata on the scope's owner type."""
        return Descriptor.resolve(self.scope.owner_type, self.path, bpy.types.Property).map(lambda descriptor: ResolvedMember(descriptor=descriptor, scope=Some(self.scope)))


class PreviewGrid(msgspec.Struct, frozen=True, kw_only=True):
    """Selector preview grid, unset counts taking Blender's."""

    rows: Annotated[int, msgspec.Meta(ge=0)] | UnsetType = UNSET
    cols: Annotated[int, msgspec.Meta(ge=0)] | UnsetType = UNSET


class DataBlocks(msgspec.Struct, frozen=True, kw_only=True):
    """Data-block browse through `template_ID`."""

    kinds: ClassVar[tuple[type[Struct], ...]] = (bpy.types.ID,)
    new: Option[Opener] = Nothing
    open: Option[Opener] = Nothing
    unlink: Option[Opener] = Nothing
    filter: str | UnsetType = UNSET


class Items(msgspec.Struct, frozen=True, kw_only=True):
    """Item browse over an RNA collection through `template_search`."""

    kinds: ClassVar[tuple[type[Struct], ...]] = (bpy.types.PointerProperty,)
    collection: Member
    new: Option[Opener] = Nothing
    unlink: Option[Opener] = Nothing


class Work(msgspec.Struct, frozen=True, kw_only=True):
    """Running work text and fraction of items done."""

    text: str
    done: Unit


class Branch[L: Struct, C: Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Tree row with depth, label struct, and cell holder per column owner."""

    depth: int
    label: L
    cells: tuple[Option[C], ...]


class Toggle(msgspec.Struct, frozen=True, kw_only=True):
    """Tree cell operator button with the property values it presses."""

    values: Mapping[str, Value]
    icon: "IconItems"
    depress: bool


class Line(msgspec.Struct, frozen=True, kw_only=True):
    """Row with optional availability condition, drawn inactive while it fails."""

    condition: Option[Condition] = Nothing

    def conditioned(self, draw: RowDraw) -> RowDraw:
        """Wraps draw in column inactive while condition fails."""

        def shown(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
            for required in self.condition.to_list():
                layout = layout.column()
                layout.active = required.holds(context, Some(holder))
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

    def drawn(self, _identity: Identity, /) -> Outcome[RowDraw]:
        """Draws the call over no bound member."""
        options = self.options

        def shown(layout: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
            self.called(layout, **options)

        return Ok(shown)


class Bound(Call, frozen=True, kw_only=True):
    """Call over one bound RNA member, resolved once and located on each draw."""

    target: Binding
    kinds: ClassVar[tuple[type[Struct], ...]] = (bpy.types.Property,)

    def accepted(self, held: ResolvedMember) -> Outcome[ResolvedMember]:
        """Accepts the target the row's function draws."""
        return held.accepts(self.kinds)

    def values(self, _identity: Identity, /) -> Outcome[dict[str, object]]:
        """Function keywords the row resolves once at acquisition, its set scalar fields alone by default."""
        return Ok(self.options)

    def placed(self, layout: bpy.types.UILayout, at: At, _context: bpy.types.Context, options: dict[str, object], /) -> None:
        """Calls the function over the located member."""
        self.called(layout, *at, **options)

    def located(self, options: dict[str, object], held: tuple[ResolvedMember, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Places the function at each owner the bound member locates."""
        for at in held[0].at(holder, context).to_list():
            self.placed(layout, at, context, options)

    @override
    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Binds target, drawing at each located owner."""
        return self.values(identity).bind(lambda options: bound(((self.target, self.accepted),), lambda held: partial(self.located, options, held)))


class Property(Bound, frozen=True, kw_only=True):
    """Setting drawn with its RNA widget."""

    function = FUNCTIONS["prop"]
    kinds = (bpy.types.BoolProperty, bpy.types.IntProperty, bpy.types.FloatProperty, bpy.types.StringProperty, bpy.types.EnumProperty, bpy.types.ID)
    text: str | UnsetType = UNSET
    icon_only: bool | UnsetType = UNSET
    expand: bool | UnsetType = UNSET
    slider: bool | UnsetType = UNSET
    placeholder: str | UnsetType = UNSET
    icon: Option[Glyph] = Nothing
    action: Option[RowContent] = Nothing

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Acquires action row registrations."""
        return Ok(acquired(self.action.to_list()))

    def labelled(self, action: Option[RowDraw], icon: int, held: tuple[ResolvedMember, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the setting at each located owner under its mixed-value label, its action beside it."""
        member = held[0]
        options = {key: value for key, value in self.options.items() if key != "text"}
        line = layout.row() if action.is_some() else layout
        labelled = member.scope.map(lambda scope: scope.owners(context).bind(lambda owners: marked(member.descriptor, owners))).default_value(Ok(Nothing))
        for owner, key in member.at(holder, context).to_list():
            alerted(
                line,
                labelled.map(lambda label: label.default_value(self.text)).map(
                    lambda text: self.called(line, owner, key, **options, index=member.descriptor.index.default_value(-1), icon_value=icon, **({} if isinstance(text, UnsetType) else {"text": text}))
                ),
            )
            for extra in action.to_list():
                extra(line, holder, context)

    @override
    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Binds setting, glyph, and action once for every redraw."""
        return paired(self.action.map(lambda row: realized(row, identity).map(Some)).default_value(Ok(Nothing)), iconed(identity, self.icon)).bind(
            lambda parts: bound(((self.target, self.accepted),), lambda held: partial(self.labelled, *parts, held))
        )


class PropertyPopover(Bound, frozen=True, kw_only=True):
    """Setting beside a button opening a stock or extension panel as popover."""

    function = FUNCTIONS["prop_with_popover"]
    panel: type[bpy.types.Panel] | Key
    text: str | UnsetType = UNSET
    icon: "IconItems | UnsetType" = UNSET
    icon_only: bool | UnsetType = UNSET

    @override
    def values(self, identity: Identity, /) -> Outcome[dict[str, object]]:
        """Resolves panel idname."""
        return idname(identity, bpy.types.Panel, self.panel).map(lambda panel: self.options | {"panel": panel})


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

    def searched(self, parts: tuple[ResolvedMember, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the search field at each located property and collection pair."""
        held, items = parts
        for (owner, key), (source, listed) in product(held.at(holder, context).to_list(), items.at(holder, context).to_list()):
            self.called(layout, owner, key, source, listed, **self.options)

    @override
    def drawn(self, _identity: Identity) -> Outcome[RowDraw]:
        """Binds property and collection together."""
        return bound(
            ((self.target, lambda held: held.accepts((bpy.types.StringProperty, bpy.types.PointerProperty))), (self.collection, lambda held: held.accepts((bpy.types.CollectionProperty,)))),
            lambda parts: partial(self.searched, parts),
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

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Acquires extension openers."""
        return Ok(tuple(opener for value in msgspec.structs.asdict(self.browse).values() if isinstance(value, Option) for opener in value.to_list() if isinstance(opener, Registrable)))

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Resolves pointer, collection, and openers into template draw."""
        grid = {key: value for preview in self.preview.to_list() for key, value in msgspec.structs.asdict(preview).items() if value is not UNSET}

        def valued(name: str, value: object) -> Outcome[Option[tuple[str, object]]]:
            match value:
                case Option(tag="some", some=operators.Command(key=key)):
                    return identity.name(bpy.types.Operator, key).map(lambda found: Some((name, found)))
                case Option(tag="some", some=operation):
                    return Ok(Some((name, dotted(operation))))
                case str():
                    return Ok(Some((name, value)))
                case _:
                    return Ok(Nothing)

        bindings: tuple[tuple[Binding, Accept], ...] = (
            (self.target, lambda held: held.accepts(self.browse.kinds)),
            *(((self.browse.collection, lambda held: held.accepts((bpy.types.CollectionProperty,))),) if isinstance(self.browse, Items) else ()),
        )
        return (
            accumulated(starmap(valued, msgspec.structs.asdict(self.browse).items()))
            .map(lambda pairs: dict(pair for found in pairs for pair in found.to_list()) | grid)
            .bind(lambda options: bound(bindings, lambda held: partial(self.templated, options, held)))
        )

    def templated(self, options: dict[str, object], held: tuple[ResolvedMember, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the template over the located pointer and each located collection."""
        line = gridded(layout, self.label)
        for at in held[0].at(holder, context).to_list():
            for searched in product(*(each.at(holder, context).to_list() for each in held[1:])):
                getattr(line, self.function.identifier)(*at, *(part for pair in searched for part in pair), **options)


class Matrix(Line, frozen=True, kw_only=True):
    """Setting cells across scoped owners, compound settings through caller editors."""

    scope: Scoped
    settings: tuple[str | RnaPath, ...]
    compound: tuple[tuple[type[Struct], Callable[[bpy.types.UILayout, Native, str], None]], ...] = ()
    action: Option[tuple[Registrable, Callable[[Identity, Descriptor], Outcome[RowDraw]]]] = Nothing

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Acquires action owner."""
        return Ok(tuple(owner for owner, _ in self.action.to_list()))

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Resolves leaf settings and owner names into table draw."""
        owner_type = self.scope.owner_type
        compound = tuple(kind for kind, _ in self.compound)
        named = Option.of_optional(owner_type.bl_rna.name_property).to_result_with(lambda: refused(owner_type.__name__, "name_property", (bpy.types.StringProperty.__name__,)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

        def cell(setting: Descriptor) -> Outcome[tuple[Descriptor, RowDraw]]:
            action = self.action.map(lambda policy: (policy[0], lambda at: policy[1](at, setting)))
            template = (
                Block
                .of_seq(self.compound)
                .filter(lambda pair: isinstance(setting.prop, bpy.types.PointerProperty) and isinstance(setting.prop.fixed_type, pair[0]))
                .try_head()
                .map(lambda pair: pair[1])
            )
            return (
                template
                .map(lambda place: action.map(lambda row: realized(row, identity).map(Some)).default_value(Ok(Nothing)).map(lambda draw: partial(Matrix.composed, setting, place, draw)))
                .default_with(lambda: Property(target=Member(root=owner_type, path=setting.path), text="", action=action).drawn(identity))
                .map(lambda draw: (setting, draw))
            )

        settings = accumulated(Descriptor.resolve(owner_type, path, bpy.types.Property) for path in self.settings).map(
            lambda found: tuple(leaf for each in found for leaf in Snapshot.settings(each, compound))
        )
        return paired(named, settings.bind(lambda found: accumulated(map(cell, found)))).map(lambda parts: partial(self.tabled, *parts))

    @staticmethod
    def composed(
        setting: Descriptor, place: Callable[[bpy.types.UILayout, Native, str], None], action: Option[RowDraw], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context
    ) -> None:
        """Draws a compound setting through its editor at each located owner, its action row first."""
        for owner, key in ResolvedMember(descriptor=setting).at(holder, context).to_list():
            for draw in action.to_list():
                draw(layout.row(align=True), holder, context)
            place(layout, owner, key)

    def tabled(self, name: bpy.types.StringProperty, held: tuple[tuple[Descriptor, RowDraw], ...], layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws owner names over one cell row per setting across the scope's owners."""

        def table(found: tuple[Struct, ...]) -> None:
            names = gridded(layout, Some(""))
            for owner in found:
                names.label(text=getattr(owner, name.identifier))
            for setting, drawing in held:
                cells = marked(setting, found).map(lambda label: gridded(layout, label.or_else(Some(setting.prop.name))))
                alerted(layout, cells)
                for line, owner in product(cells.to_option().to_list(), found):
                    drawing(line.column(), owner, context)

        alerted(layout, self.scope.owners(context).map(table))


class Tree(Line, frozen=True, kw_only=True):
    """Resolved paths listed once per prefix, indented by depth."""

    paths: Callable[[bpy.types.Context], Outcome[tuple[PurePath, ...]]]

    def drawn(self, _identity: Identity) -> Outcome[RowDraw]:
        """Returns tree draw over source paths."""

        def listed(layout: bpy.types.UILayout, paths: tuple[PurePath, ...]) -> None:
            files = {path.parts for path in paths}
            for node in sorted({path.parts[:end] for path in paths for end in range(1, len(path.parts) + 1)}):
                line = layout.row(align=True)
                for _ in node[1:]:
                    line.label(text="", icon="BLANK1")
                line.label(text=node[-1], icon="FILE_BLANK" if node in files else "FILE_FOLDER")

        return Ok(lambda layout, _holder, context: alerted(layout, self.paths(context).map(lambda paths: listed(layout, paths))))


class TreeMatrix[R: bpy.types.PropertyGroup, L: Struct, C: Struct](Line, frozen=True, kw_only=True):
    """Tree rows indented by depth with toggle cells per column owner."""

    tree: Callable[[bpy.types.Context], tuple[tuple[str, ...], tuple[Branch[L, C], ...]]]
    command: operators.Operator[R]
    toggles: Callable[[str, C, L], tuple[Toggle, ...]]
    active: Option[Callable[[C], bool]] = Nothing
    marks: tuple[RowContent, ...] = ()

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Acquires toggle command and mark rows."""
        return Ok((self.command, *acquired(self.marks)))

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Realizes label marks into tree draw."""

        def draw(marks: tuple[RowDraw, ...]) -> RowDraw:
            def cell(line: bpy.types.UILayout, owner: str, found: C, label: L) -> None:
                line.active = all(read(found) for read in self.active.to_list())
                for toggle in self.toggles(owner, found, label):
                    alerted(line, pressed(line, identity, self.command, toggle.values, text="", icon=toggle.icon, depress=toggle.depress))

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

            def shown(layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
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

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Acquires extension command."""
        return Ok(tuple(command for command in (self.command,) if isinstance(command, Registrable)))

    @override
    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
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

            def shown(layout: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
                line = gridded(layout, self.label)
                alerted(line, operated(self.called(line, operator, **options, icon_value=icon), operator).bind(write))

            return invoking(shown, self.context) if invoked else shown

        return paired(self.target.invocation(identity), iconed(identity, glyph)).map(draw)


class Opens(Call, frozen=True, kw_only=True):
    """Button opening a stock or extension panel, menu, or shelf by key or class, on the property grid when labelled."""

    label: Option[str] = Nothing
    icon: Option[Glyph] = Nothing

    def opened(self, identity: Identity, /) -> Outcome[str]:
        """Resolves the opened idname."""
        raise NotImplementedError

    def shown(self, options: dict[str, object], layout: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
        """Draws the opener button on the property grid when labelled."""
        self.called(gridded(layout, self.label), **options)

    @override
    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Resolves idname into the function's leading parameter and glyph once."""
        return paired(self.opened(identity), iconed(identity, self.icon)).map(
            lambda parts: partial(self.shown, self.options | {self.function.parameters[0].identifier: parts[0], "icon_value": parts[1]})
        )


class Popover(Opens, frozen=True, kw_only=True):
    """Popover button over a panel."""

    function = FUNCTIONS["popover"]
    panel: type[bpy.types.Panel] | Key
    text: str | UnsetType = UNSET

    @override
    def opened(self, identity: Identity, /) -> Outcome[str]:
        """Resolves panel idname."""
        return idname(identity, bpy.types.Panel, self.panel)


class Pulldown(Opens, frozen=True, kw_only=True):
    """Menu button over a menu."""

    function = FUNCTIONS["menu"]
    menu: type[bpy.types.Menu] | Key
    text: str | UnsetType = UNSET

    @override
    def opened(self, identity: Identity, /) -> Outcome[str]:
        """Resolves menu idname."""
        return idname(identity, bpy.types.Menu, self.menu)


class ShelfPopover(Opens, frozen=True, kw_only=True):
    """Asset shelf popover button."""

    function = FUNCTIONS["template_asset_shelf_popover"]
    popup: ShelfPopup
    name: str | UnsetType = UNSET

    @override
    def opened(self, identity: Identity, /) -> Outcome[str]:
        """Resolves shelf idname."""
        return idname(identity, bpy.types.AssetShelf, self.popup.shelf)


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

    scale_x: Positive = 1.0
    scale_y: Positive = 1.0
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

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Acquires registrable rows."""
        return Ok(acquired(self.rows))

    def framed(self, draws: tuple[RowDraw, ...]) -> RowDraw:
        """Returns joined draws in caption arrangement."""
        draw = joined(draws)

        def shown(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
            for separator in self.separator.to_list():
                separator.called(layout, **separator.options)
            draw(self.flow.opened(self.sizing.applied(layout.column(heading=self.heading.default_value("")))), holder, context)

        return shown

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Places rows on enclosing property grid."""
        return placed(self.rows, identity).map(self.framed)


class Rows(Line, frozen=True, kw_only=True):
    """Rows built from the extension identity on one grid, hidden instead of inactive while condition fails when declared."""

    rows: Callable[[Identity], Outcome[tuple[RowContent, ...]]]
    hidden: bool = False

    @override
    def conditioned(self, draw: RowDraw) -> RowDraw:
        """Hides draw while condition fails when declared, else draws it inactive."""
        if not self.hidden:
            return super().conditioned(draw)

        def shown(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
            if all(required.holds(context, Some(holder)) for required in self.condition.to_list()):
                draw(layout, holder, context)

        return shown

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Places identity rows on one grid."""
        return self.rows(identity).bind(lambda rows: placed(rows, identity)).map(joined)


class Label(Line, frozen=True, kw_only=True):
    """Fixed or live text with glyph wrapped to the layout, one line in headers, Markdown without glyph when declared."""

    text: str | Callable[[bpy.types.Context], Option[str]]
    icon: Option[Glyph] = Nothing
    markdown: bool = False

    def shown(self, icon: int, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the text read this redraw, one line in headers."""
        for text in (Some(self.text) if isinstance(self.text, str) else self.text(context)).to_list():
            if Option.of_optional(context.region).filter(lambda region: region.type in {"HEADER", "TOOL_HEADER"}).is_some():
                layout.label(text=text, icon_value=icon)
            elif self.markdown:
                layout.label_markdown(text=text)
            else:
                layout.label_multiline(text=text, icon_value=icon)

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Resolves glyph once, returning draw reading text each redraw."""
        return iconed(identity, self.icon).map(lambda icon: partial(self.shown, icon))


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
    def drawn(self, _identity: Identity) -> Outcome[RowDraw]:
        """Returns draw of every running work."""
        options = self.options

        def shown(layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
            for run in self.runs(context):
                self.called(gridded(layout, self.label), **options, text=run.text, factor=run.done)

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BINDING]


def refused(path: str | RnaPath, found: str, accepted: tuple[str, ...]) -> Faults:
    """Returns choice fault for an unsupported binding at host path text or a path."""
    return Fault.of(RecordReason.OUTSIDE, rna(path), found, member=", ".join(accepted))


def bound(bindings: tuple[tuple[Binding, Accept], ...], place: Callable[[tuple[ResolvedMember, ...]], RowDraw]) -> Outcome[RowDraw]:
    """Resolves static bindings once and reached members each draw, drawing nothing while one is absent."""

    def reader(binding: Binding, accept: Accept) -> Outcome[Callable[[bpy.types.Context], Outcome[Option[ResolvedMember]]]]:
        match binding:
            case Member(reach=Option(tag="some", some=reach)):
                return Ok(lambda context: reach(context).map(lambda root: binding.resolved(root=Some(root)).bind(accept).map(Some)).default_value(Ok(Nothing)))
            case _:
                return binding.resolved().bind(accept).map(lambda held: lambda _context: Ok(Some(held)))

    def reached(readers: tuple[Callable[[bpy.types.Context], Outcome[Option[ResolvedMember]]], ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        found = accumulated(read(context) for read in readers)
        alerted(layout, found)
        for held in found.to_option().to_list():
            present = tuple(value for each in held for value in each.to_list())
            if len(present) == len(held):
                place(present)(layout, holder, context)

    if any(isinstance(binding, Member) and binding.reach.is_some() for binding, _ in bindings):
        return accumulated(starmap(reader, bindings)).map(lambda readers: partial(reached, readers))
    return accumulated(binding.resolved().bind(accept) for binding, accept in bindings).map(place)


def marked(setting: Descriptor, owners: tuple[Struct, ...]) -> Outcome[Option[str]]:
    """Returns mixed-value label when owner values differ."""
    return accumulated(Snapshot.capture(owner, Some((setting,))) for owner in owners).map(
        lambda values: Some(f"{setting.prop.name} —") if any(left != right for left, right in pairwise(values)) else Nothing
    )


# --- [DRAWING]


def iconed(identity: Identity, icon: Option[Glyph]) -> Outcome[int]:
    """Resolves glyph icon ID, 0 leaving the RNA icon, absent fault for a glyph the host or icon set lacks."""
    return icon.map(lambda glyph: Icons.value(identity, glyph)).default_value(Ok(0))


def operated(properties: object, operator: str) -> Outcome[bpy.types.OperatorProperties]:
    """Maps missing operator properties to absent fault."""
    return Option.of_optional(try_downcast(bpy.types.OperatorProperties, properties)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, "bpy.ops", member=operator))


def pressed[T: bpy.types.PropertyGroup, S](
    layout: bpy.types.UILayout, identity: Identity, command: operators.Operator[T, S], values: Mapping[str, Value], *, text: str, icon: "IconItems", depress: bool = False
) -> Outcome[None]:
    """Draws operator button over property values."""
    return (
        operators
        .Own(command=command, values=values)
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


def alerted[T](layout: bpy.types.UILayout, result: Outcome[T]) -> None:
    """Draws failures as alert labels."""
    for faults in result.swap().to_option().to_list():
        column = layout.column(align=True)
        column.alert = True
        for _, text in rendered(faults):
            column.label(text=text, icon="STATUS_ERROR_FILLED")


def invoking(draw: RowDraw, mode: "rna_enums.OperatorContextItems" = "INVOKE_REGION_WIN") -> RowDraw:
    """Scopes operator context around drawing."""

    def shown(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
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


def realized(row: RowContent, identity: Identity) -> Outcome[RowDraw]:
    """Acquires typed row draw under its condition, passing callbacks through."""
    match row:
        case tuple():
            return row[1](identity)
        case Row():
            return row.drawn(identity).map(lambda draw: row.conditioned(draw) if isinstance(row, Line) else draw)
        case Callable():
            return Ok(row)


def placed(rows: tuple[RowContent, ...], identity: Identity, *, inherited: bool = False) -> Outcome[tuple[RowDraw, ...]]:
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

    def blanked(draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        line = layout.row()
        draw(line, holder, context)
        line.label(text="", icon="BLANK1")

    def each(row: RowContent) -> Outcome[RowDraw]:
        match row:
            case Caption():
                return placed(row.rows, identity, inherited=picked).map(row.framed).map(row.conditioned)
            case Property(action=action) if picked and action.is_none():
                return row.drawn(identity).map(lambda draw: partial(blanked, draw)).map(row.conditioned)
            case _:
                return realized(row, identity)

    return accumulated(map(each, rows))


def joined(draws: tuple[RowDraw, ...]) -> RowDraw:
    """Joins draws in declaration order over one holder."""

    def shown(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
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
