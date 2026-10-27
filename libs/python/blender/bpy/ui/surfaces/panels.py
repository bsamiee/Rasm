"""Panels in a Properties tab, stock panel, Sidebar tab, region, or popover."""

from collections.abc import Callable, Generator, Iterable, Iterator
from contextlib import contextmanager, ExitStack
import re
from typing import Final, Literal, Self, TYPE_CHECKING

import _bpy_types
from bl_ui.space_properties import PropertiesAnimationMixin
import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor, subclass
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, entered, Registrable
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.layout.rows import acquired, Caption, painted, placed, RowContent, RowDraw
from blender.bpy.ui.layout.sections import Each, Section, SectionDraws, Single
from blender.bpy.ui.resources.icons import Icons
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason
from blender.records.fields import Case

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type FixedPlacement = Tab | Sidebar | Joined | Child | EditorRegion
type Placed = FixedPlacement | PopoverPanel
type PanelPlacement = Placed | Docked


# --- [CONSTANTS] ------------------------------------------------------------------------

SPACES: Final = Descriptor.declared(bpy.types.Space, "type", bpy.types.EnumProperty).prop


# --- [MODELS] ---------------------------------------------------------------------------


class Tab(Case, frozen=True, kw_only=True):
    """Stock Properties tab panels join."""

    context: str

    @classmethod
    def beside(cls, panel: type[_bpy_types.Panel]) -> Self:
        """Returns the Properties tab a stock panel class sits in."""
        return cls(context=_declared(panel).bl_context)


class Sidebar(Case, frozen=True, kw_only=True):
    """Extension Sidebar tab with product mark icon."""

    space: str
    category: str


class Joined(Case, frozen=True, kw_only=True):
    """Stock Sidebar tab panels join."""

    space: str
    category: str

    @classmethod
    def beside(cls, panel: type[_bpy_types.Panel]) -> Self:
        """Returns the Sidebar tab a stock panel class sits in."""
        declared = _declared(panel)
        return cls(space=declared.bl_space_type, category=declared.bl_category)


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
    """Placement read at registration and again after each `changed` firing."""

    read: Callable[[Identity], Outcome[Tab | Sidebar | Joined]]
    changed: Signal


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
    options: frozenset[Literal["HIDE_HEADER", "INSTANCED", "HEADER_LAYOUT_EXPAND"]] = frozenset()
    header: tuple[RowContent, ...] = ()

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns section acquisitions, value popover panels, and one scope registering every placement."""

        def stated[P, T](panel: P, member: Callable[[P], T]) -> Option[T]:
            return catch(member, exception=AttributeError)(panel).to_option()

        def members(at: Placed, mark: int) -> dict[str, object]:
            match at:
                case Tab(context=context):
                    return {"bl_space_type": "PROPERTIES", "bl_region_type": "WINDOW", "bl_context": context}
                case Sidebar(space=space, category=category):
                    return {"bl_space_type": space, "bl_region_type": "UI", "bl_category": category, "bl_icon_value": mark}
                case Joined(space=space, category=tab):
                    return {"bl_space_type": space, "bl_region_type": "UI", "bl_category": tab}
                case Child(parent=parent):
                    declared = _declared(parent)
                    return {
                        "bl_space_type": declared.bl_space_type,
                        "bl_region_type": declared.bl_region_type,
                        "bl_parent_id": parent.bl_rna.identifier,
                        **stated(declared, lambda held: held.bl_category).map(lambda tab: {"bl_category": tab}).default_value({}),
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

        def tabs(space: str) -> tuple[str, ...]:
            return tuple(sorted({tab for panel in registered() if (panel.bl_region_type, panel.bl_space_type) == ("UI", space) for tab in stated(panel, lambda held: held.bl_category).to_list()}))

        def contexts() -> tuple[str, ...]:
            return tuple(sorted({context for panel in registered() if panel.bl_space_type == "PROPERTIES" for context in stated(panel, lambda held: held.bl_context).to_list()}))

        def accepted(at: PanelPlacement) -> Outcome[PanelPlacement]:
            match at:
                case Joined(space=space, category=tab) if tab not in (listed := tabs(space)):
                    return Error(Fault.of(RecordReason.OUTSIDE, space, tab, member=", ".join(listed)))
                case Tab(context=context) if context not in (offered := contexts()):
                    return Error(Fault.of(RecordReason.OUTSIDE, "PROPERTIES", context, member=", ".join(offered)))
            return Ok(at)

        def preceding(placing: dict[str, object]) -> int:
            rows = (((panel.bl_space_type, panel.bl_region_type, stated(panel, lambda held: held.bl_parent_id)), stated(panel, lambda held: held.bl_order)) for panel in registered())
            beside = (placing["bl_space_type"], placing["bl_region_type"], Option.of_optional(placing.get("bl_parent_id")))
            return max((0, *(order for at, held in rows if at == beside for order in held.filter(lambda order: order < PropertiesAnimationMixin.bl_order).to_list())))

        def sited(drawn: tuple[tuple[SectionDraws, tuple[tuple[Section, SectionDraws], ...]], tuple[RowDraw, ...]]) -> Outcome[tuple[PanelSite, ...]]:
            (top, children), header = drawn

            def site(stem: str, at: PanelPlacement, condition: Option[Condition]) -> Outcome[PanelSite]:
                nested = accumulated(identity.name(bpy.types.Panel, f"{stem}_{child.key}").map(lambda name: Drawing(name=name, section=child, drawn=each)) for child, each in children)
                return paired(paired(identity.name(bpy.types.Panel, stem), nested), accepted(at)).map(
                    lambda parts: PanelSite(at=at, condition=condition, top=Drawing(name=parts[0][0], section=self.section, drawn=top), children=parts[0][1], header=header)
                )

            key = self.section.key
            return accumulated((site(key, self.at, self.condition), *(site(f"{key}_{suffix(mirror.at)}", mirror.at, mirror.condition) for mirror in self.mirrors)))

        def closed(section: Section) -> set[Literal["DEFAULT_CLOSED"]]:
            return set() if section.opened else {"DEFAULT_CLOSED"}

        def built(site: PanelSite, at: Placed, mark: int) -> tuple[type[bpy.types.Panel], ...]:
            shared = members(at, mark) | {"bl_owner_id": identity.package}

            def poll(_cls: type[bpy.types.Panel], context: bpy.types.Context) -> bool:
                return site.condition.map(lambda condition: condition.holds(context)).default_value(True) and site.top.drawn.present(context)

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

            top = {"bl_options": closed(site.top.section) | self.options, **shared, "poll": classmethod(poll)}
            return (
                classed(site.top, top | {"bl_order": preceding(top)}, site.header),
                *(classed(child, shared | {"bl_parent_id": site.top.name, "bl_order": index, "bl_options": closed(child.section)}) for index, child in enumerate(site.children)),
            )

        @contextmanager
        def displayed(sites: tuple[PanelSite, ...]) -> Generator[Outcome[None]]:
            held: Option[tuple[Placed, ...]] = Nothing
            classes = ExitStack()

            def resolved() -> Outcome[tuple[Placed, ...]]:
                return accumulated(site.at.read(identity) if isinstance(site.at, Docked) else Ok(site.at) for site in sites)

            def registered(now: tuple[Placed, ...]) -> Outcome[None]:
                nonlocal held
                mark = Icons.mark(identity).default_value(0)
                acquisitions = tuple(cls for site, at in zip(sites, now) for cls in built(site, at, mark))
                with ExitStack() as entering:
                    if (answer := entered(entering, acquisitions)).is_ok():
                        classes.enter_context(entering.pop_all())
                        held = Some(now)
                    return answer

            def rebuilt() -> Outcome[Option[str]]:
                def replaced(now: tuple[Placed, ...]) -> Outcome[None]:
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

                def requested() -> Outcome[None]:
                    pending()
                    return Ok(None)

                for signal in {id(site.at.changed): site.at.changed for site in sites if isinstance(site.at, Docked)}.values():
                    scope.enter_context(signal.followed(requested))
                yield resolved().bind(registered)

        @contextmanager
        def prepared() -> Generator[Outcome[None]]:
            children = accumulated(child.realized(identity) for child in self.children).map(lambda drawn: tuple(zip(self.children, drawn)))
            with ExitStack() as stack:
                yield paired(paired(self.section.realized(identity), children), placed(self.header, identity)).bind(sited).bind(lambda sites: stack.enter_context(displayed(sites)))

        roots = (self.section, *self.children)
        return Ok((*roots, *acquired(self.header), *(Panel(section=popover, at=PopoverPanel()) for popover in reached((*roots, *self.header))), (Band.INTERFACE, prepared())))


class Drawing(msgspec.Struct, frozen=True, kw_only=True):
    """Panel class name, its section, and section draws."""

    name: str
    section: Section
    drawn: SectionDraws


class PanelSite(msgspec.Struct, frozen=True, kw_only=True):
    """Panel placement with resolved drawings."""

    at: PanelPlacement
    condition: Option[Condition]
    top: Drawing
    children: tuple[Drawing, ...]
    header: tuple[RowDraw, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


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


def registered() -> tuple[type[bpy.types.Panel], ...]:
    """Returns every registered Python panel class, subclasses included."""
    return tuple(panel for name in dir(bpy.types) for panel in subclass(bpy.types.Panel, name).to_option().to_list())


def _declared(panel: type[_bpy_types.Panel]) -> type[bpy.types.Panel]:
    """Returns a stock panel class as the RNA panel type declaring its placement members, which the stubs give `_bpy_types.Panel` no sight of."""
    declared: type[bpy.types.Panel] = panel  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return declared


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SPACES", "Child", "Docked", "EditorRegion", "Joined", "Mirror", "Panel", "PopoverPanel", "Sidebar", "Tab", "reached"]
