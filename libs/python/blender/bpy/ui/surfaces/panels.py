"""Panels in a Properties tab, stock panel, Sidebar tab, region, or popover."""

from collections import defaultdict
from collections.abc import Callable, Generator, Iterable, Iterator
from contextlib import contextmanager, ExitStack
import re
from typing import Annotated, ClassVar, Literal, TYPE_CHECKING

import _bpy_types
from bl_ui.space_properties import PropertiesAnimationMixin
import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions, Band, entered, Registrable
from blender.bpy.faults import BlenderChoiceFault, BlenderResult
from blender.bpy.settings.properties import Changed, Choices
from blender.bpy.ui.layout.rows import acquired, Caption, painted, placed, RowContent, RowDraw
from blender.bpy.ui.layout.sections import Drawn, Each, Section, Single
from blender.bpy.ui.resources.icons import Icons
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, heading, markers

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type FixedPlacement = Tab | Sidebar | Joined | Child | EditorRegion
type Placed = FixedPlacement | PopoverPanel
type PanelPlacement = Placed | Docked


# --- [CHOICES] --------------------------------------------------------------------------

tabs = Choices(lambda *_: ((context, heading(context), "") for context in contexts()))
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
    """Stock Properties tab panels join."""

    title: ClassVar[str] = "Properties Tab"
    context: Annotated[str, msgspec.Meta(title="Tab", description="Properties tab for extension panels"), tabs]


class Sidebar(Case, frozen=True, kw_only=True, tag=2):
    """Extension Sidebar tab with product mark icon."""

    title: ClassVar[str] = "Sidebar Tab"
    space: Annotated[str, msgspec.Meta(title="Editor", description="Editor showing extension Sidebar tab"), editors]
    category: str


class Joined(Case, frozen=True, kw_only=True, tag=3):
    """Stock Sidebar tab panels join."""

    title: ClassVar[str] = "Shared Sidebar Tab"
    space: Annotated[str, msgspec.Meta(title="Editor", description="Editor holding Sidebar tab"), editors]
    category: Annotated[str, msgspec.Meta(title="Tab", description="Sidebar tab for extension panels"), categories]


class Child(msgspec.Struct, frozen=True, kw_only=True):
    """Stock parent panel."""

    parent: type[_bpy_types.Panel]


class EditorRegion(msgspec.Struct, frozen=True, kw_only=True, gc=False):
    """Editor region laying out panels, with no tab and no popover."""

    space: "rna_enums.SpaceTypeItems"
    region: Literal["UI", "TOOLS", "TOOL_PROPS", "HUD", "HEADER", "TOOL_HEADER", "FOOTER"]


class PopoverPanel(msgspec.Struct, frozen=True, kw_only=True):
    """Top bar popover a `rows.Popover` row opens from any editor."""

    units: Option[int] = Nothing


class Docked(msgspec.Struct, frozen=True, kw_only=True):
    """Placement read at registration and each preference edit."""

    read: Callable[[Identity], BlenderResult[Tab | Sidebar | Joined]]


class Mirror(msgspec.Struct, frozen=True, kw_only=True):
    """Extra panel placement with its own poll condition."""

    at: FixedPlacement
    condition: Option[Condition] = Nothing


class Panel(msgspec.Struct, frozen=True, kw_only=True):
    """Section registered as panel with mirrors, children, and section value popovers."""

    section: Section
    at: PanelPlacement
    condition: Option[Condition] = Nothing
    mirrors: tuple[Mirror, ...] = ()
    children: tuple[Section, ...] = ()
    headed: bool = True
    expand_header: bool = False
    header: tuple[RowContent, ...] = ()

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns section acquisitions, value popover panels, and one scope registering every placement."""

        def members(at: Placed, mark: int) -> dict[str, object]:
            match at:
                case Tab(context=context):
                    return {"bl_space_type": "PROPERTIES", "bl_region_type": "WINDOW", "bl_context": context}
                case Sidebar(space=space, category=category):
                    return {"bl_space_type": space, "bl_region_type": "UI", "bl_category": category, "bl_icon_value": mark}
                case Joined(space=space, category=tab):
                    return {"bl_space_type": space, "bl_region_type": "UI", "bl_category": tab}
                case Child(parent=parent):
                    return {
                        "bl_space_type": parent.bl_space_type,  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                        "bl_region_type": parent.bl_region_type,  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                        "bl_parent_id": parent.bl_rna.identifier,
                        **stated(parent, lambda held: held.bl_category).map(lambda tab: {"bl_category": tab}).default_value({}),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    }
                case EditorRegion(space=space, region=region):
                    return {"bl_space_type": space, "bl_region_type": region}
                case PopoverPanel(units=units):
                    return {"bl_space_type": "TOPBAR", "bl_region_type": "HEADER", "bl_options": set(), **units.map(lambda width: {"bl_ui_units_x": width}).default_value({})}

        def suffix(at: FixedPlacement) -> str:
            match at:
                case Tab(context=context):
                    text = context
                case Sidebar(space=space, category=tab) | Joined(space=space, category=tab):
                    text = f"{space} {tab}"
                case Child(parent=parent):
                    text = parent.bl_rna.identifier
                case EditorRegion(space=space, region=region):
                    text = f"{space} {region}"
            return "_".join(re.findall(r"[a-z0-9]+", text.lower()))

        def accepted(at: PanelPlacement) -> BlenderResult[PanelPlacement]:
            match at:
                case Joined(space=space, category=tab) if tab not in (listed := sidebars().get(space, ())):
                    return Error((BlenderChoiceFault(path=space, identifier=tab, accepted=listed),))
                case Tab(context=context) if context not in (offered := contexts()):
                    return Error((BlenderChoiceFault(path="PROPERTIES", identifier=context, accepted=offered),))
            return Ok(at)

        def preceding(placing: dict[str, object]) -> int:
            rows = (((panel.bl_space_type, panel.bl_region_type, stated(panel, lambda held: held.bl_parent_id)), stated(panel, lambda held: held.bl_order)) for panel in registered())
            beside = (placing["bl_space_type"], placing["bl_region_type"], Option.of_optional(placing.get("bl_parent_id")))
            return max((0, *(order for at, held in rows if at == beside for order in held.filter(lambda order: order < PropertiesAnimationMixin.bl_order).to_list())))

        def sited(drawn: tuple[tuple[Drawn, tuple[tuple[Section, Drawn], ...]], tuple[RowDraw, ...]]) -> BlenderResult[tuple[PanelSite, ...]]:
            (top, children), header = drawn

            def site(stem: str, at: PanelPlacement, condition: Option[Condition]) -> BlenderResult[PanelSite]:
                nested = accumulated(identity.name(Name.PANEL, f"{stem}_{child.key}").map(lambda name: Drawing(name=name, section=child, drawn=each)) for child, each in children)
                return paired(paired(identity.name(Name.PANEL, stem), nested), accepted(at)).map(
                    lambda parts: PanelSite(at=at, condition=condition, headed=self.headed, top=Drawing(name=parts[0][0], section=self.section, drawn=top), children=parts[0][1], header=header)
                )

            key = self.section.key
            return accumulated((site(key, self.at, self.condition), *(site(f"{key}_{suffix(mirror.at)}", mirror.at, mirror.condition) for mirror in self.mirrors)))

        def options(section: Section) -> set[str]:
            return set() if section.opened else {"DEFAULT_CLOSED"}

        def built(site: PanelSite, at: Placed, mark: int) -> tuple[type[bpy.types.Panel], ...]:
            shared = members(at, mark) | {"bl_owner_id": identity.package}

            def poll(_cls: type[bpy.types.Panel], context: bpy.types.Context) -> bool:
                return site.condition.map(lambda condition: holds(condition, context)).default_value(True) and site.top.drawn.present(context)

            def classed(drawing: Drawing, placing: dict[str, object], header: tuple[RowDraw, ...] = ()) -> type[bpy.types.Panel]:
                drawn = drawing.drawn
                tail = (*drawn.tail.to_list(), *((painted(header),) if header else ()))

                def preset(panel: bpy.types.Panel, context: bpy.types.Context) -> None:
                    for layout in Option.of_optional(panel.layout).to_list():
                        layout.emboss = "NONE"
                        for paint in tail:
                            paint(layout, context)

                return type(
                    drawing.name,
                    (bpy.types.Panel,),
                    {
                        **placing,
                        "bl_idname": drawing.name,
                        "bl_label": drawing.section.title,
                        "bl_description": drawing.section.description,
                        "draw": lambda panel, context: drawn.body(panel.layout, context),
                        **drawn.head.map(lambda paint: {"draw_header": lambda panel, context: paint(panel.layout, context)}).default_value({}),
                        **({"draw_header_preset": preset} if tail else {}),
                    },
                )

            top = {"bl_options": options(site.top.section) | ({"HEADER_LAYOUT_EXPAND"} if self.expand_header else set()) if site.headed else {"HIDE_HEADER"}, **shared, "poll": classmethod(poll)}
            return (
                classed(site.top, top | {"bl_order": preceding(top)}, site.header),
                *(classed(child, shared | {"bl_parent_id": site.top.name, "bl_order": index, "bl_options": options(child.section)}) for index, child in enumerate(site.children)),
            )

        @contextmanager
        def displayed(sites: tuple[PanelSite, ...]) -> Generator[BlenderResult[None]]:
            held: Option[tuple[Placed, ...]] = Nothing
            classes = ExitStack()

            def resolved() -> BlenderResult[tuple[Placed, ...]]:
                return accumulated(site.at.read(identity) if isinstance(site.at, Docked) else Ok(site.at) for site in sites)

            def registered(now: tuple[Placed, ...]) -> BlenderResult[None]:
                nonlocal held
                mark = Icons.mark(identity).default_value(0)
                acquisitions = tuple(cls for site, at in zip(sites, now) for cls in built(site, at, mark))
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
            children = accumulated(child.realized(identity) for child in self.children).map(lambda drawn: tuple(zip(self.children, drawn)))
            with ExitStack() as stack:
                yield paired(paired(self.section.realized(identity), children), placed(self.header, identity)).bind(sited).bind(lambda sites: stack.enter_context(displayed(sites)))

        roots = (self.section, *self.children)
        return Ok((*roots, *acquired(self.header), *(Panel(section=popover, at=PopoverPanel()) for popover in reached((*roots, *self.header))), (Band.INTERFACE, prepared())))


class Drawing(msgspec.Struct, frozen=True, kw_only=True):
    """Panel class name, its section, and section draws."""

    name: str
    section: Section
    drawn: Drawn


class PanelSite(msgspec.Struct, frozen=True, kw_only=True):
    """Panel placement with resolved drawings."""

    at: PanelPlacement
    condition: Option[Condition]
    headed: bool
    top: Drawing
    children: tuple[Drawing, ...]
    header: tuple[RowDraw, ...]


# --- [REGISTRIES] -----------------------------------------------------------------------

_listeners: defaultdict[Identity, list[Changed]] = defaultdict(list)


# --- [OPERATIONS] -----------------------------------------------------------------------


def notified(identity: Identity) -> Changed:
    """Returns preference update callback re-reading every docked placement."""

    def body(holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context, row: bpy.types.Property) -> None:
        for heard in tuple(_listeners.get(identity, ())):
            heard(holder, context, row)

    return body


@contextmanager
def listening(identity: Identity, heard: Changed) -> Generator[None]:
    """Holds preference listener until scope exits."""
    listeners = _listeners[identity]
    listeners.append(heard)
    try:
        yield
    finally:
        listeners.remove(heard)
        if not listeners:
            del _listeners[identity]


def reached(rows: Iterable[RowContent | Registrable]) -> Iterator[Section]:
    """Yields section popovers of nested rows and registrable owners."""
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
    """Returns enum property items as choice items, none for other properties."""
    return ((item.identifier, item.name, item.description, item.icon, item.value) for held in (prop,) if isinstance(held, bpy.types.EnumProperty) for item in held.enum_items)


def stated[P: type, T](panel: P, member: Callable[[P], T]) -> Option[T]:
    """Returns registration member a panel class or mixin states."""
    return catch(lambda: member(panel), exception=AttributeError)().to_option()


def registered() -> tuple[type[bpy.types.Panel], ...]:
    """Returns every registered Python panel class, subclasses included."""
    return tuple(panel for name in dir(bpy.types) for panel in Option.of_optional(bpy.types.Panel.bl_rna_get_subclass_py(name)).to_list())


def sidebars() -> dict[str, tuple[str, ...]]:
    """Returns registered Sidebar tabs per editor."""
    panels = tuple(panel for panel in registered() if panel.bl_region_type == "UI")
    return {
        space: tuple(sorted({tab for panel in panels if panel.bl_space_type == space for tab in stated(panel, lambda held: held.bl_category).to_list()}))
        for space in {panel.bl_space_type for panel in panels}
    }


def contexts() -> tuple[str, ...]:
    """Returns Properties tab contexts registered panels name."""
    return tuple(sorted({context for panel in registered() if panel.bl_space_type == "PROPERTIES" for context in stated(panel, lambda held: held.bl_context).to_list()}))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Child", "Docked", "EditorRegion", "Joined", "Mirror", "Panel", "PopoverPanel", "Sidebar", "Tab", "notified", "reached"]
