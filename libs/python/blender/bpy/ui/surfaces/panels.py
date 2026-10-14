"""Panel rows registered under a Properties tab, a stock panel, a Sidebar tab, another region, or a popover."""

from collections.abc import Callable, Generator, Iterable, Iterator, Mapping
from contextlib import contextmanager, ExitStack
import re
from typing import Annotated, ClassVar, TYPE_CHECKING

from bl_ui.space_properties import PropertiesAnimationMixin
import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions, Band, entered, Registrable
from blender.bpy.faults import BlenderChoiceFault, BlenderResult
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import acquired, Caption, painted, placed, RowContent, RowDraw
from blender.bpy.ui.layout.sections import Drawn, Each, Section, Single
from blender.bpy.ui.resources.icons import Icons
from blender.bpy.ui.surfaces.preferences import listening, PreferencesPage
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, markers

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type FixedPlacement = Tab | Sidebar | Joined | Child | EditorRegion
type Placed = FixedPlacement | PopoverPanel
type PanelPlacement = Placed | Docked


# --- [ROSTERS] --------------------------------------------------------------------------

tabs = Choices(lambda *_: enumerated(bpy.types.SpaceProperties.bl_rna.properties["context"]))
editors = Choices(
    lambda *_: (
        (identifier, name, description, icon, value)
        for spaces in (sidebars(),)
        for identifier, name, description, icon, value in enumerated(bpy.types.Space.bl_rna.properties["type"])
        if identifier in spaces
    )
)
categories = Choices(
    lambda _, owner, __: ((name, name, "") for field in msgspec.structs.fields(Joined) if editors in markers(field.type, Choices) for name in sidebars().get(getattr(owner, field.name), ()))
)


# --- [MODELS] ---------------------------------------------------------------------------


class Tab(Case, frozen=True, kw_only=True, tag=1):
    """Stock Properties editor tab the panels join."""

    title: ClassVar[str] = "Properties Tab"
    tab: Annotated[str, msgspec.Meta(title="Tab", description="Properties editor tab the panels join"), tabs]


class Sidebar(Case, frozen=True, kw_only=True, tag=2):
    """Extension's own tab in an editor's Sidebar, carrying the product mark."""

    title: ClassVar[str] = "Sidebar Tab"
    space: Annotated[str, msgspec.Meta(title="Editor", description="Editor whose Sidebar holds the extension's tab"), editors]
    category: str


class Joined(Case, frozen=True, kw_only=True, tag=3):
    """Stock tab in an editor's Sidebar the panels join."""

    title: ClassVar[str] = "Shared Sidebar Tab"
    space: Annotated[str, msgspec.Meta(title="Editor", description="Editor whose Sidebar holds the tab"), editors]
    category: Annotated[str, msgspec.Meta(title="Tab", description="Sidebar tab the panels join"), categories]


class Child(msgspec.Struct, frozen=True, kw_only=True):
    """Stock panel the panels nest under."""

    parent: type[bpy.types.Panel]


class EditorRegion(msgspec.Struct, frozen=True, kw_only=True, gc=False):
    """Editor region with no tab and no popover."""

    space: "rna_enums.SpaceTypeItems"
    region: "rna_enums.RegionTypeItems"


class PopoverPanel(msgspec.Struct, frozen=True, kw_only=True):
    """Top bar popover a `rows.Popover` row opens by key from any editor."""

    units: Option[int] = Nothing


class Docked(msgspec.Struct, frozen=True, kw_only=True):
    """Placement read at registration and each preference edit."""

    read: Callable[[Identity], BlenderResult[Tab | Sidebar | Joined]]


class Mirror(msgspec.Struct, frozen=True, kw_only=True):
    """Further placement of a panel's sections, polled by its own condition."""

    at: FixedPlacement
    condition: Option[Condition] = Nothing


class Panel(msgspec.Struct, frozen=True, kw_only=True):
    """Section placed as a registered panel with its mirrors and child panels."""

    section: Section
    at: PanelPlacement
    condition: Option[Condition] = Nothing
    mirrors: tuple[Mirror, ...] = ()
    children: tuple[Section, ...] = ()
    headed: bool = True
    header: tuple[RowContent, ...] = ()


class Drawing(msgspec.Struct, frozen=True, kw_only=True):
    """Class's idname, label, and section paints."""

    name: str
    title: str
    drawn: Drawn


class PanelSite(msgspec.Struct, frozen=True, kw_only=True):
    """One placement of a panel with its class, child classes, order, header, and condition."""

    at: PanelPlacement
    condition: Option[Condition]
    headed: bool
    top: Drawing
    children: tuple[Drawing, ...]
    header: tuple[RowDraw, ...]


class Panels(msgspec.Struct, frozen=True, kw_only=True):
    """Row registering panels, their child panels, and the value popovers of every section they and the preferences page place."""

    panels: tuple[Panel, ...]
    page: Option[PreferencesPage] = Nothing
    header: Mapping[type[Placed], tuple[RowContent, ...]] = msgspec.field(default_factory=dict)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Realizes every site once and pairs the sections' acquisitions with the generator owning every class.

        Args:
            identity: Identity whose names, reports, mark, and preferences the panels read.

        Returns:
            `Ok` of section declarations and the `INTERFACE` acquisition; native realization and registration
            failures belong to that acquisition.
        """
        headers = (*(row for panel in self.panels for row in panel.header), *(row for rows in self.header.values() for row in rows))
        roots = (*(section for panel in self.panels for section in (panel.section, *panel.children)), *(section for page in self.page.to_list() for section in page.sections), *headers)
        popovers = {popover.key: Panel(section=popover, at=PopoverPanel()) for popover in reached(roots)}
        panels = (*self.panels, *popovers.values())

        def sited(panel: Panel, drawn: tuple[tuple[Drawn, tuple[tuple[Section, Drawn], ...]], tuple[RowDraw, ...]]) -> BlenderResult[tuple[PanelSite, ...]]:
            (top, children), header = drawn

            def site(stem: str, at: PanelPlacement, condition: Option[Condition]) -> BlenderResult[PanelSite]:
                nested = accumulated(identity.name(Name.PANEL, f"{stem}_{child.key}").map(lambda name: Drawing(name=name, title=child.title, drawn=each)) for child, each in children)
                return paired(paired(identity.name(Name.PANEL, stem), nested), admitted(at)).map(
                    lambda parts: PanelSite(at=at, condition=condition, headed=panel.headed, top=Drawing(name=parts[0][0], title=panel.section.title, drawn=top), children=parts[0][1], header=header)
                )

            key = panel.section.key
            return accumulated((site(key, panel.at, panel.condition), *(site(f"{key}_{suffix(mirror.at)}", mirror.at, mirror.condition) for mirror in panel.mirrors)))

        def realized(panel: Panel) -> BlenderResult[tuple[PanelSite, ...]]:
            children = accumulated(child.realized(identity) for child in panel.children).map(lambda drawn: tuple(zip(panel.children, drawn)))
            return paired(paired(panel.section.realized(identity), children), placed(panel.header, identity)).bind(lambda drawn: sited(panel, drawn))

        def built(site: PanelSite, at: Placed, mark: int, headers: Mapping[type[Placed], tuple[RowDraw, ...]]) -> tuple[type[bpy.types.Panel], ...]:
            shared = {**members(at, mark), "bl_owner_id": identity.package}

            def poll(_cls: type[bpy.types.Panel], context: bpy.types.Context) -> bool:
                return site.condition.map(lambda condition: holds(condition, context)).default_value(True)

            def classed(drawing: Drawing, placing: dict[str, object], header: tuple[RowDraw, ...] = ()) -> type[bpy.types.Panel]:
                drawn = drawing.drawn
                tail = (*drawn.tail.to_list(), *((painted(header),) if header else ()))

                def preset(panel: bpy.types.Panel, context: bpy.types.Context) -> None:
                    for layout in Option.of_optional(panel.layout).to_list():
                        line = layout.row(align=True)
                        for paint in tail:
                            paint(line, context)

                return type(
                    drawing.name,
                    (bpy.types.Panel,),
                    {
                        **placing,
                        "bl_idname": drawing.name,
                        "bl_label": drawing.title,
                        "draw": lambda panel, context: drawn.body(panel.layout, context),
                        **drawn.head.map(lambda paint: {"draw_header": lambda panel, context: paint(panel.layout, context)}).default_value({}),
                        **({"draw_header_preset": preset} if tail else {}),
                    },
                )

            top = {"bl_options": {"DEFAULT_CLOSED"} if site.headed else {"HIDE_HEADER"}, **shared, "poll": classmethod(poll)}
            return (
                classed(site.top, {**top, "bl_order": preceding(top)}, (*site.header, *headers.get(type(at), ()))),
                *(classed(child, {**shared, "bl_parent_id": site.top.name, "bl_order": index, "bl_options": {"DEFAULT_CLOSED"}}) for index, child in enumerate(site.children)),
            )

        @contextmanager
        def displayed(sites: tuple[PanelSite, ...], headers: Mapping[type[Placed], tuple[RowDraw, ...]]) -> Generator[BlenderResult[None]]:
            held: Option[tuple[Placed, ...]] = Nothing
            classes = ExitStack()

            def resolved() -> BlenderResult[tuple[Placed, ...]]:
                return accumulated(site.at.read(identity) if isinstance(site.at, Docked) else Ok(site.at) for site in sites)

            def registered(now: tuple[Placed, ...]) -> BlenderResult[None]:
                nonlocal held
                mark = Icons.mark(identity).default_value(0)
                acquisitions = tuple(cls for site, at in zip(sites, now) for cls in built(site, at, mark, headers))
                with ExitStack() as entering:
                    answer = entered(entering, acquisitions)
                    match answer.tag:
                        case "error":
                            return answer
                        case "ok":
                            classes.enter_context(entering.pop_all())
                            held = Some(now)
                    return answer

            def rebuilt() -> BlenderResult[Option[str]]:
                def replaced(now: tuple[Placed, ...]) -> BlenderResult[None]:
                    nonlocal held
                    if held == Some(now):
                        return Ok(None)
                    previous = held
                    classes.close()
                    held = Nothing
                    return registered(now).or_else_with(lambda faults: paired(Error(faults), previous.map(registered).default_value(Ok(None))).map(lambda _: None))

                return resolved().bind(replaced).map(lambda _: Nothing)

            with ExitStack() as scope:
                scope.enter_context(classes)
                pending = scope.enter_context(identity.reports.deferred(rebuilt))
                scope.enter_context(listening(identity, lambda *_: pending()))
                yield resolved().bind(registered)

        @contextmanager
        def prepared() -> Generator[BlenderResult[None]]:
            with ExitStack() as stack:
                yield paired(
                    accumulated(realized(panel) for panel in panels).map(lambda parts: tuple(site for part in parts for site in part)),
                    accumulated(placed(rows, identity).map(lambda draws: (kind, draws)) for kind, rows in self.header.items()).map(dict),
                ).bind(lambda parts: stack.enter_context(displayed(*parts)))

        sections = tuple(section for panel in panels for section in (panel.section, *panel.children))
        return Ok((*sections, *acquired(headers), (Band.INTERFACE, prepared())))


# --- [OPERATIONS] -----------------------------------------------------------------------


def members(at: Placed, mark: int) -> dict[str, object]:
    """Answers the registration members a placement gives every class it places.

    Args:
        at: Placement of the classes.
        mark: Icon value of the product mark, `0` where the package holds none.

    Returns:
        The space, region, and the case's context, category, parent, icon, width, or options.
    """
    match at:
        case Tab(tab=tab):
            return {"bl_space_type": "PROPERTIES", "bl_region_type": "WINDOW", "bl_context": "particle" if tab == "PARTICLES" else tab.lower()}
        case Sidebar(space=space, category=category):
            return {"bl_space_type": space, "bl_region_type": "UI", "bl_category": category, "bl_icon_value": mark}
        case Joined(space=space, category=tab):
            return {"bl_space_type": space, "bl_region_type": "UI", "bl_category": tab}
        case Child(parent=parent):
            return {
                "bl_space_type": parent.bl_space_type,
                "bl_region_type": parent.bl_region_type,
                "bl_parent_id": parent.bl_rna.identifier,  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                **stated(parent, lambda held: held.bl_category).map(lambda tab: {"bl_category": tab}).default_value({}),
            }
        case EditorRegion(space=space, region=region):
            return {"bl_space_type": space, "bl_region_type": region}
        case PopoverPanel(units=units):
            return {"bl_space_type": "TOPBAR", "bl_region_type": "HEADER", "bl_options": set(), **units.map(lambda width: {"bl_ui_units_x": width}).default_value({})}


def suffix(at: FixedPlacement) -> str:
    """Answers the key a mirror at a placement adds to its section key."""
    match at:
        case Tab(tab=words):
            text = words
        case Sidebar(space=space, category=tab) | Joined(space=space, category=tab):
            text = f"{space} {tab}"
        case Child(parent=parent):
            text = parent.bl_rna.identifier  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        case EditorRegion(space=space, region=region):
            text = f"{space} {region}"
    return "_".join(re.findall(r"[a-z0-9]+", text.lower()))


def admitted(at: PanelPlacement) -> BlenderResult[PanelPlacement]:
    """Accepts a placement, a stock Sidebar tab against the tabs its editor lists.

    Args:
        at: Placement a panel or mirror states.

    Returns:
        `Ok` of the placement, else `Error` of a `choice` fault holding the editor and its tabs.
    """
    match at:
        case Joined(space=space, category=tab) if tab not in (listed := sidebars().get(space, ())):
            return Error((BlenderChoiceFault(path=space, identifier=tab, accepted=listed),))
    return Ok(at)


def reached(rows: Iterable[RowContent | Registrable]) -> Iterator[Section]:
    """Answers declared section popovers through nested content and its acquired owners."""
    for row in rows:
        if isinstance(row, Section):
            yield from row.popovers
        match row:
            case (Registrable() as owner, _):
                yield from reached((owner,))
            case Single(rows=inner, header=header):
                yield from reached((*inner, *header))
            case Each(rows=inner, header=header, empty=empty):
                yield from reached((*inner, *header, *empty))
            case Caption(rows=inner):
                yield from reached(inner)
            case _:
                pass


def enumerated(prop: bpy.types.Property) -> Iterator[tuple[str, str, str, str, int]]:
    """Answers an RNA property's enum items as roster items, a property of another type holding none."""
    return ((item.identifier, item.name, item.description, item.icon, item.value) for held in (prop,) if isinstance(held, bpy.types.EnumProperty) for item in held.enum_items)


def stated[T](panel: type[bpy.types.Panel], member: Callable[[type[bpy.types.Panel]], T]) -> Option[T]:
    """Answers a registration member a panel class states, a mixin's included, absent where it states none."""
    return catch(lambda: member(panel), exception=AttributeError)().to_option()


def registered() -> tuple[type[bpy.types.Panel], ...]:
    """Answers every registered Python panel class, a class deriving from a concrete panel included."""
    return tuple(panel for name in dir(bpy.types) for panel in Option.of_optional(bpy.types.Panel.bl_rna_get_subclass_py(name)).to_list())


def preceding(placing: dict[str, object]) -> int:
    """Answers the greatest order among the panel types a class with these members registers beside.

    Args:
        placing: Members of the class, holding its space and region and its parent where it states one.

    Returns:
        The greatest order below Blender's closing Animation and Custom Properties band among the types sharing the class's
        space, region, and parent, the C-registered image editor Metadata panel included, `0` where none states one.
    """
    rows = (
        *(((panel.bl_space_type, panel.bl_region_type, stated(panel, lambda held: held.bl_parent_id)), stated(panel, lambda held: held.bl_order)) for panel in registered()),
        (("IMAGE_EDITOR", "UI", Nothing), Some(10)),
    )
    beside = (placing["bl_space_type"], placing["bl_region_type"], Option.of_optional(placing.get("bl_parent_id")))
    return max((0, *(order for at, held in rows if at == beside for order in held.filter(lambda order: order < PropertiesAnimationMixin.bl_order).to_list())))


def sidebars() -> dict[str, tuple[str, ...]]:
    """Answers each editor holding a registered Sidebar panel with the tabs its panels state."""
    panels = tuple(panel for panel in registered() if panel.bl_region_type == "UI")
    return {
        space: tuple(sorted({tab for panel in panels if panel.bl_space_type == space for tab in stated(panel, lambda held: held.bl_category).to_list()}))
        for space in {panel.bl_space_type for panel in panels}
    }


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Child", "Docked", "EditorRegion", "Joined", "Mirror", "Panel", "Panels", "PopoverPanel", "Sidebar", "Tab"]
