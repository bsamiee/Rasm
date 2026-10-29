"""Screen probes over the background window: region overlap and visible bounds, region visibility, located editors and queries, editor coordinates, and arrangements applied and reverted."""

from collections.abc import Callable, Mapping
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Some
from expression.extra.result import catch

from blender.bpy.extension.identity import Identity
from blender.bpy.ui.resources.theme import overlaps
from blender.bpy.ui.screens.areas import AreaQuery, areas, Arrangement, Located, region_in, Retype, Split, Step
from blender.bpy.ui.screens.regions import flipped, quadded, to_region, to_view, visibility, visible_bounds
from blender.records.faults import Outcome

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [READS]


def _window() -> bpy.types.Window:
    """Returns the background session's one window."""
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return manager.windows[0]


def _area(kind: str) -> bpy.types.Area:
    """Returns the window's first area of an editor type."""
    return next(area for area in _window().screen.areas if area.type == kind)


def _region(area: bpy.types.Area, kind: str) -> bpy.types.Region:
    """Returns the area's first region of a type."""
    return next(region for region in area.regions if region.type == kind)


def _rect(region: bpy.types.Region) -> tuple[int, int, int, int]:
    """Returns a region's window rectangle."""
    return region.x, region.y, region.width, region.height


def _settled(arrangement: Arrangement) -> Outcome[Arrangement]:
    """Advances an arrangement until it settles, answering its last pending form, which holds every revert."""
    return arrangement.advanced().bind(lambda pending: pending.map(_settled).default_value(Ok(arrangement)))


def _arranged(area: bpy.types.Area, steps: tuple[Step, ...], read: Callable[[], object]) -> dict[str, object]:
    """Applies steps to an area and reverts them, reading the screen before, between, and after."""
    before = read()
    settled = _settled(Arrangement(at=Located(window=_window(), area=area), steps=steps))
    applied = read()
    reverted = settled.bind(lambda held: held.reverted())
    return {"before": before, "applied": applied, "settled": settled.map(lambda _: None), "reverted": reverted, "after": read()}


# --- [PROBES]


def overlapping(_identity: Identity) -> dict[str, object]:
    """Reads which regions overlap their editor's main region with region overlap on and off."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    pairs: tuple[tuple[rna_enums.SpaceTypeItems, rna_enums.RegionTypeItems], ...] = (
        ("VIEW_3D", "UI"),
        ("VIEW_3D", "TOOLS"),
        ("VIEW_3D", "HUD"),
        ("NODE_EDITOR", "UI"),
        ("PROPERTIES", "UI"),
        ("IMAGE_EDITOR", "TOOL_HEADER"),
        ("SEQUENCE_EDITOR", "UI"),
    )
    prior = preferences.system.use_region_overlap

    def read(*, enabled: bool) -> dict[str, bool]:
        preferences.system.use_region_overlap = enabled
        return {f"{space} {region}": overlaps(preferences, space, region) for space, region in pairs}

    answer: dict[str, object] = {"on": read(enabled=True), "off": read(enabled=False)}
    preferences.system.use_region_overlap = prior
    return answer


def bounded(_identity: Identity) -> dict[str, object]:
    """Reads the 3D View main region's visible bounds with region overlap on and off, beside its own and its toolbar's rectangles."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    area = _area("VIEW_3D")
    window, tools = _region(area, "WINDOW"), _region(area, "TOOLS")
    prior = preferences.system.use_region_overlap
    preferences.system.use_region_overlap = True
    shared = visible_bounds(area, window)
    preferences.system.use_region_overlap = False
    alone = visible_bounds(area, window)
    preferences.system.use_region_overlap = prior
    return {"overlapping": shared, "alone": alone, "region": _rect(window), "toolbar": _rect(tools), "toolbar alignment": tools.alignment}


def shown(_identity: Identity) -> dict[str, object]:
    """Shows the 3D View Sidebar through an editor state and applies the state it replaced, refusing region types no member toggles."""
    area = _area("VIEW_3D")
    space: bpy.types.SpaceView3D = area.spaces.active  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    before = space.show_region_ui
    prior = visibility(bpy.types.SpaceView3D, "UI", visible=not before).bind(lambda state: state.applied(area))
    flipped_to = space.show_region_ui
    restored = prior.bind(lambda held: held.map(lambda state: state.applied(area)).default_value(Ok(Nothing)))
    return {
        "states": (before, flipped_to, space.show_region_ui),
        "prior": prior.map(lambda held: held.is_some()),
        "restored": restored.map(lambda held: held.is_some()),
        "main": visibility(bpy.types.SpaceView3D, "WINDOW", visible=True).map(lambda _: None),
        "channels": visibility(bpy.types.SpaceView3D, "CHANNELS", visible=True).map(lambda _: None),
    }


def located(_identity: Identity) -> dict[str, object]:
    """Locates the context editor, editors by query largest first, every area of the window, and regions by type."""
    view = _area("VIEW_3D")
    return {
        "current": Located.current(bpy.context).map(lambda at: at.area.type),
        "view": AreaQuery(space=bpy.types.SpaceView3D).first((_window(),)).map(lambda at: at.area.type),
        "image": AreaQuery(space=bpy.types.SpaceImageEditor).first((_window(),)).map(lambda at: at.area.type),
        "sizes": tuple(at.area.width * at.area.height for at in AreaQuery(space=bpy.types.Space).located((_window(),))),
        "areas": tuple(at.area.type for at in areas((_window(),))),
        "screen": tuple(area.type for area in _window().screen.areas),
        "main": region_in(view).map(lambda region: region.type),
        "preview": region_in(view, "PREVIEW").map(lambda region: region.type),
        "within": Located(window=_window(), area=view).within("HEADER").map(lambda at: at.region.map(lambda region: region.type)),
    }


def converted(_identity: Identity) -> dict[str, object]:
    """Converts dope sheet region pixels to editor coordinates and back, a point outside the region, and an unlocated region."""
    area = _area("DOPESHEET_EDITOR")
    at = Located(window=_window(), area=area, region=Some(_region(area, "WINDOW")))
    viewed = to_view(at, 40.0, 20.0)
    return {
        "back": viewed.bind(lambda point: to_region(at, *point)),
        "outside": viewed.bind(lambda point: to_region(at, point[0] + 1.0e6, point[1])),
        "unlocated": to_view(Located(window=_window(), area=area), 40.0, 20.0),
    }


def retyped(_identity: Identity) -> dict[str, object]:
    """Retypes the dope sheet to the Graph Editor through an arrangement and reverts it."""
    area = _area("DOPESHEET_EDITOR")
    return _arranged(area, (Retype(ui_type="FCURVES"),), lambda: area.ui_type)


def split(_identity: Identity) -> dict[str, object]:
    """Splits a Text Editor off the 3D View's right edge through an arrangement, its revert left unrun since `screen.area_close` never returns in a background session."""
    view = _area("VIEW_3D")
    before = (view.width, tuple(area.type for area in _window().screen.areas))
    settled = _settled(Arrangement(at=Located(window=_window(), area=view), steps=(Split(side="RIGHT", share=0.25, ui_type="TEXT_EDITOR"),)))
    text = _area("TEXT_EDITOR")
    return {
        "before": before,
        "after": (view.width, tuple(area.type for area in _window().screen.areas)),
        "text": text.width,
        "reverts": settled.map(lambda held: len(held.undo)),
        "x": (view.x, text.x),
    }


def shared(_identity: Identity) -> str:
    """Builds a split whose share leaves the source less than the new editor, answering the error type construction raises."""
    return catch(Split, exception=ValueError)(side="LEFT", share=0.7, ui_type="TEXT_EDITOR").swap().map(lambda error: type(error).__name__).default_value("")


def flips(_identity: Identity) -> dict[str, object]:
    """Moves the 3D View toolbar across its axis and back, and asks the header to cross axes."""
    area = _area("VIEW_3D")
    tools = Located(window=_window(), area=area, region=Some(_region(area, "TOOLS")))
    header = Located(window=_window(), area=area, region=Some(_region(area, "HEADER")))
    return {
        "same": flipped(tools, "LEFT"),
        "across": flipped(tools, "RIGHT").map(lambda _: _region(area, "TOOLS").alignment),
        "back": flipped(tools, "LEFT").map(lambda _: _region(area, "TOOLS").alignment),
        "crossing": flipped(header, "LEFT"),
        "unlocated": flipped(Located(window=_window(), area=area), "LEFT"),
    }


def quads(_identity: Identity) -> dict[str, object]:
    """Shows and hides quad view of the 3D View, reading the quad regions after each."""
    area = _area("VIEW_3D")
    at = Located(window=_window(), area=area, region=Some(_region(area, "WINDOW")))
    space: bpy.types.SpaceView3D = area.spaces.active  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    shown_quads = quadded(at, shown=True).map(lambda _: len(space.region_quadviews))
    hidden_quads = quadded(Located(window=_window(), area=area, region=Some(_region(area, "WINDOW"))), shown=False).map(lambda _: len(space.region_quadviews))
    return {"shown": shown_quads, "hidden": hidden_quads}


# --- [COMPOSITION] ----------------------------------------------------------------------

rows: Final = ()
PROBES: Final[Mapping[str, Callable[[Identity], object]]] = MappingProxyType({
    "overlapping": overlapping,
    "bounded": bounded,
    "shown": shown,
    "located": located,
    "converted": converted,
    "retyped": retyped,
    "shared": shared,
    "flips": flips,
    "quads": quads,
    "split": split,
})
RELEASED: Final[Mapping[str, Callable[[], object]]] = MappingProxyType({})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "rows"]
