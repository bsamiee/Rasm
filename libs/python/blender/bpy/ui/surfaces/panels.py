"""Panels beside a stock panel, under a stock parent, in an extension Sidebar tab, in an editor region, or opened as a popover."""

from collections.abc import Callable, Generator, Iterable, Iterator, Mapping
from contextlib import contextmanager, ExitStack
import re
from typing import Any, Literal, Self, TYPE_CHECKING

import _bpy_types
from bl_ui.space_properties import PropertiesAnimationMixin
import bpy
from expression import Error, Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.paths import definition
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.layout.members import admits
from blender.bpy.ui.layout.rows import layout_method
from blender.bpy.ui.layout.sections import reached, Section, SectionDraws
from blender.bpy.ui.resources.icons import ICONS
from blender.records.faults import accumulated, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type FixedPlacement = Beside | Child | Sidebar | EditorRegion
type Placed = FixedPlacement | PopoverPanel
type PanelPlacement = Placed | Docked
type _Site = tuple[str, PanelPlacement, Option[Condition]]

# --- [MODELS] ---------------------------------------------------------------------------


class Beside(msgspec.Struct, frozen=True):
    """Place of a stock panel: its editor and region, with the Properties tab or Sidebar tab it declares."""

    panel: type[_bpy_types.Panel]

    @property
    def slug(self) -> str:
        """Key suffix of a copy placed here."""
        return _slug(definition(_stock(self.panel)).identifier)

    def members(self, _identity: Identity) -> dict[str, object]:
        """Copies the stock panel's editor, region, and whichever of tab context and Sidebar category it declares."""
        declared = _stock(self.panel)
        contexts = catch(lambda: declared.bl_context, exception=AttributeError)().to_option()
        categories = catch(lambda: declared.bl_category, exception=AttributeError)().to_option()
        return {
            "bl_space_type": declared.bl_space_type,
            "bl_region_type": declared.bl_region_type,
            **contexts.map(lambda value: {"bl_context": value}).default_value({}),
            **categories.map(lambda value: {"bl_category": value}).default_value({}),
        }


class Child(msgspec.Struct, frozen=True, kw_only=True):
    """Place under a stock parent panel, which draws its children whatever their category."""

    parent: type[_bpy_types.Panel]

    @property
    def slug(self) -> str:
        """Key suffix of a copy placed here."""
        return _slug(definition(_stock(self.parent)).identifier)

    def members(self, _identity: Identity) -> dict[str, object]:
        """Copies the parent's editor and region and names it as parent."""
        parent = _stock(self.parent)
        return {"bl_space_type": parent.bl_space_type, "bl_region_type": parent.bl_region_type, "bl_parent_id": definition(parent).identifier}


class Sidebar(msgspec.Struct, frozen=True, kw_only=True):
    """Extension Sidebar tab of an editor, marked with the extension's brand glyph."""

    space: "rna_enums.SpaceTypeItems"
    category: str

    @property
    def slug(self) -> str:
        """Key suffix of a copy placed here."""
        return _slug(f"{self.space} {self.category}")

    def members(self, identity: Identity) -> dict[str, object]:
        """Places the panel in the tab, its icon the brand glyph while one is staged."""
        mark = identity.held.get(ICONS).bind(lambda icons: icons.brand)
        return {"bl_space_type": self.space, "bl_region_type": "UI", "bl_category": self.category, **mark.map(lambda value: {"bl_icon_value": value}).default_value({})}


class EditorRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Editor region laying out panels with no tab."""

    space: "rna_enums.SpaceTypeItems"
    region: "rna_enums.RegionTypeItems"

    @property
    def slug(self) -> str:
        """Key suffix of a copy placed here."""
        return _slug(f"{self.space} {self.region}")

    def members(self, _identity: Identity) -> dict[str, object]:
        """Places the panel in the region."""
        return {"bl_space_type": self.space, "bl_region_type": self.region}


class PopoverPanel(msgspec.Struct, frozen=True):
    """Panel only a popover button or `wm.call_panel` opens, registered in a header region that draws no panels."""

    @staticmethod
    def members(_identity: Identity) -> dict[str, object]:
        """Places the panel in the top bar header."""
        return {"bl_space_type": "TOPBAR", "bl_region_type": "HEADER"}


class Docked(msgspec.Struct, frozen=True, kw_only=True):
    """Placement read at registration and again after each `changed` firing."""

    read: Callable[[Identity], Outcome[FixedPlacement]]
    changed: Signal


class PanelCopy(msgspec.Struct, frozen=True, kw_only=True):
    """Further placement registering its own copy of the panel, with its own poll condition."""

    at: FixedPlacement
    condition: Option[Condition] = Nothing


class Panel(msgspec.Struct, frozen=True, kw_only=True):
    """Section registered as a panel at each placement, with child panels and a popover panel per store values popover its sections reach.

    `condition` hides the panel while it fails, as does a section holder the context does not reach, and `units` sets its width as a popover.
    """

    section: Section[Any]
    at: PanelPlacement
    condition: Option[Condition] = Nothing
    copies: tuple[PanelCopy, ...] = ()
    children: tuple[Section[Any], ...] = ()
    options: frozenset[Literal["HIDE_HEADER", "HEADER_LAYOUT_EXPAND"]] = frozenset()
    units: Option[int] = Nothing

    @classmethod
    def popovers(cls, rows: Iterable[object]) -> tuple[Self, ...]:
        """Popover panel of each store values popover the rows' sections reach."""
        return tuple(cls(section=popover, at=PopoverPanel()) for section in reached(rows) for popover in section.popover.to_list())

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the sections, a popover panel per values popover they reach, and the scope registering every placement."""
        roots = (self.section, *self.children)
        return Ok((*roots, *Panel.popovers(roots), (Band.INTERFACE, _displayed(self, identity))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [REGISTRATION]


@contextmanager
def _displayed(panel: Panel, identity: Identity) -> Generator[Outcome[None]]:
    """Registers the panel classes at every placement for the scope, re-registering a docked placement after each `changed` firing that moves it."""
    sites: tuple[_Site, ...] = ((panel.section.key, panel.at, panel.condition), *((f"{panel.section.key}_{copy.at.slug}", copy.at, copy.condition) for copy in panel.copies))
    drawn = paired(panel.section.realized(identity), accumulated(child.realized(identity) for child in panel.children))
    held: list[tuple[Placed, ...]] = []
    classes = ExitStack()

    def classed(section: Section[Any], draws: SectionDraws, name: str, members: Mapping[str, object], options: frozenset[str], condition: Option[Condition]) -> type[bpy.types.Panel]:
        """Builds a panel class over the section's draws, closed unless the section opens first, polled by the condition and whether the context reaches the section's holder."""

        def poll(_cls: type[bpy.types.Panel], context: bpy.types.Context) -> bool:
            return admits(condition, context) and draws.present(context)

        headers = {member: layout_method(paint) for member, shown in (("draw_header", draws.head), ("draw_header_preset", draws.tail)) for paint in shown.to_list()}
        return type(
            name,
            (bpy.types.Panel,),
            {
                **members,
                "bl_idname": name,
                "bl_label": section.title,
                "bl_description": section.description,
                "bl_translation_context": section.translation_context,
                "bl_options": {*options, *(() if section.opened else ("DEFAULT_CLOSED",))},
                "poll": classmethod(poll),
                "draw": layout_method(draws.body),
                **headers,
            },
        )

    def placed(draws: tuple[SectionDraws, tuple[SectionDraws, ...]], stem: str, at: Placed, condition: Option[Condition]) -> Outcome[tuple[type[bpy.types.Panel], ...]]:
        """Builds the panel class at one placement, ordered after the highest order a registered panel of its editor, region, and parent holds below the Animation panel, then its child panel classes in order."""
        top, children = draws
        placing = {**at.members(identity), "bl_owner_id": identity.package}
        beside = (placing["bl_space_type"], placing["bl_region_type"], placing.get("bl_parent_id"))
        preceding = max((
            0,
            *(
                order
                for stock in dict.fromkeys(_registered_panels(bpy.types.Panel))
                if (stock.bl_space_type, stock.bl_region_type, catch(lambda: stock.bl_parent_id, exception=AttributeError)().to_option().to_optional()) == beside
                for order in catch(lambda: stock.bl_order, exception=AttributeError)().to_option().filter(lambda found: found < PropertiesAnimationMixin.bl_order).to_list()
            ),
        ))
        ordered = {**placing, "bl_order": preceding, **panel.units.map(lambda width: {"bl_ui_units_x": width}).default_value({})}
        shared = {key: value for key, value in placing.items() if key != "bl_parent_id"}

        def built(name: str, nested: tuple[str, ...]) -> tuple[type[bpy.types.Panel], ...]:
            return (
                classed(panel.section, top, name, ordered, panel.options, condition),
                *(
                    classed(child, each, inner, {**shared, "bl_parent_id": name, "bl_order": index}, frozenset(), Nothing)
                    for index, (child, each, inner) in enumerate(zip(panel.children, children, nested, strict=True))
                ),
            )

        return paired(identity.name(bpy.types.Panel, stem), accumulated(identity.name(bpy.types.Panel, f"{stem}_{child.key}") for child in panel.children)).map(lambda names: built(*names))

    def resolved() -> Outcome[tuple[Placed, ...]]:
        return accumulated(at.read(identity) if isinstance(at, Docked) else Ok(at) for _, at, _ in sites)

    def registered(now: tuple[Placed, ...]) -> Outcome[None]:
        with ExitStack() as entering:
            answer = drawn.bind(lambda draws: accumulated(placed(draws, stem, at, condition) for (stem, _, condition), at in zip(sites, now, strict=True))).bind(
                lambda built: entered(entering, tuple(cls for group in built for cls in group))
            )
            if answer.is_ok():
                classes.enter_context(entering.pop_all())
                held[:] = [now]
            return answer

    def rebuilt() -> Outcome[Option[str]]:
        def replaced(now: tuple[Placed, ...]) -> Outcome[None]:
            if held == [now]:
                return Ok(None)
            previous = tuple(held)
            classes.close()
            held.clear()
            return registered(now).or_else_with(lambda faults: paired(Error(faults), accumulated(map(registered, previous))).map(lambda _: None))

        return resolved().bind(replaced).map(lambda _: Nothing)

    with ExitStack() as scope:
        scope.enter_context(classes)
        pending = scope.enter_context(identity.reports.deferred(rebuilt))
        for signal in dict.fromkeys(at.changed for _, at, _ in sites if isinstance(at, Docked)):
            scope.enter_context(signal.followed(lambda: Ok(pending())))
        yield resolved().bind(registered)


# --- [ORDER]


def _slug(text: str) -> str:
    """Returns lowercase words of text joined by underscores."""
    return "_".join(re.findall(r"[a-z0-9]+", text.lower()))


def _registered_panels(base: type[bpy.types.Panel]) -> Iterator[type[bpy.types.Panel]]:
    """Walks the panel classes under base that the host holds registered."""
    for cls in base.__subclasses__():
        if "bl_rna" in vars(cls):
            yield cls
        yield from _registered_panels(cls)


def _stock(panel: type[_bpy_types.Panel]) -> type[bpy.types.Panel]:
    """Returns a stock panel as the RNA panel type declaring its placement members, which the stubs give `_bpy_types.Panel` no sight of."""
    declared: type[bpy.types.Panel] = panel  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return declared


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Beside", "Child", "Docked", "EditorRegion", "FixedPlacement", "Panel", "PanelCopy", "PanelPlacement", "Placed", "PopoverPanel", "Sidebar"]
