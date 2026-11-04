"""Native editor queries, screen geometry, reversible editor state, and owned workspaces."""

from collections.abc import Generator
from contextlib import ExitStack
from typing import Literal

import bpy
from expression import Option, Result, Some
import pytest

from blender.bpy.ui.screens.areas import AreaQuery, areas, Arrangement, flipped, Located, quadded, region_in, Retype, Split, to_region, to_view, visibility, visible_bounds
from blender.bpy.ui.screens.workspaces import WorkspaceSetup
from blender.records.faults import Fault, Nothing, Ok, Reason
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_editor_queries_preserve_native_order_and_rank_by_pixel_area(view: Located) -> None:
    """Enumeration preserves screen order and matching editor queries rank by native pixel area."""
    listed = areas((view.window,))
    assert tuple(at.area for at in listed) == (*view.window.screen.areas, *view.window.global_areas)
    ranked = AreaQuery(space=bpy.types.Space).located((view.window,))
    sizes = tuple(at.area.width * at.area.height for at in ranked)
    assert sizes == tuple(sorted(sizes, reverse=True))
    assert AreaQuery(space=bpy.types.SpaceView3D).first((view.window,)).map(lambda at: at.area) == Ok(view.area)
    assert region_in(view.area).map(lambda region: region.type) == Ok("WINDOW")
    match region_in(view.area, "PREVIEW"):
        case Result(error=(Fault(reason=reason, member=member),)):
            assert (reason, member) == (Reason.ABSENT, "PREVIEW")
        case found:
            pytest.fail(repr(found))


def test_retype_arrangement_reverts_the_native_editor(view: Located) -> None:
    """Retype applies on the host and records the native editor value to restore."""
    before = view.area.ui_type
    pending = Arrangement(at=view, steps=(Retype(ui_type="TEXT_EDITOR"),))
    match pending.advanced():
        case Result(tag="ok", ok=Option(some=applied)):
            try:
                assert view.area.ui_type == "TEXT_EDITOR"
                assert applied.advanced() == Ok(Nothing)
            finally:
                assert applied.reverted() == Ok(None)
        case failed:
            pytest.fail(repr(failed))
    assert view.area.ui_type == before


def test_split_adds_a_native_editor_at_the_requested_edge(view: Located) -> None:
    """Split creates a native editor at the requested edge and its inverse restores the original screen."""
    before = {area.as_pointer() for area in view.window.screen.areas}
    width = view.area.width
    pending = Arrangement(at=view, steps=(Split(side="RIGHT", share=0.25, ui_type="TEXT_EDITOR"),))
    match pending.advanced():
        case Result(tag="ok", ok=Option(some=split)):
            pass
        case failed:
            pytest.fail(repr(failed))
    try:
        assert split.advanced() == Ok(Nothing)
        (created,) = tuple(area for area in view.window.screen.areas if area.as_pointer() not in before)
        assert created.type == "TEXT_EDITOR"
        assert view.area.x < created.x
        assert 0 < created.width < width
    finally:
        assert split.reverted() == Ok(None)
    assert {area.as_pointer() for area in view.window.screen.areas} == before


def region_visibility_returns_the_state_it_replaced(view: Located) -> Generator[bool]:
    """Visibility records its prior value and restores it across native animation passes."""
    space = view.area.spaces.active
    assert isinstance(space, bpy.types.SpaceView3D)
    before = space.show_region_ui
    match visibility(bpy.types.SpaceView3D, "UI", visible=not before).bind(lambda state: state.applied(view)):
        case Result(tag="ok", ok=Option(some=prior)):
            try:
                assert tuple(value for _, value in prior.values) == (before,)
                yield from iter(lambda: space.show_region_ui is before, False)  # ruff: ignore[boolean-positional-value-in-call]
            finally:
                assert prior.applied(view).is_ok()
            yield from iter(lambda: space.show_region_ui is not before, False)  # ruff: ignore[boolean-positional-value-in-call]
        case found:
            pytest.fail(repr(found))
    assert space.show_region_ui is before


def test_visible_bounds_follow_actual_overlap_preference(view: Located) -> None:
    """Native nonoverlap uses the whole region; overlap removes supported sibling coverage."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    with ExitStack() as scope:
        scope.callback(setattr, preferences.system, "use_region_overlap", preferences.system.use_region_overlap)
        region = next(region for region in view.area.regions if region.type == "WINDOW")
        tools = next(region for region in view.area.regions if region.type == "TOOLS")
        preferences.system.use_region_overlap = False
        assert visible_bounds(view.area, region) == (0, 0, region.width, region.height)
        preferences.system.use_region_overlap = True
        left, bottom, width, height = visible_bounds(view.area, region)
        assert (left, bottom, left + width) == (tools.width - 1, 0, region.width)
        assert 0 < height < region.height


def test_toolbar_axis_and_quadview_revert_native_changes(view: Located) -> None:
    """Toolbar moves across its axis and quad views appear and disappear through native operations."""
    tools = next(region for region in view.area.regions if region.type == "TOOLS")
    at = Located(window=view.window, area=view.area, region=Some(tools))
    match tools.alignment:
        case "LEFT" | "RIGHT" as before:
            other: Literal["LEFT", "RIGHT"] = "RIGHT" if before == "LEFT" else "LEFT"
        case outside:
            pytest.fail(repr(outside))
    try:
        assert flipped(at, other) == Ok(None)
        assert tools.alignment == other
    finally:
        assert flipped(at, before) == Ok(None)
    assert tools.alignment == before
    space = view.area.spaces.active
    assert isinstance(space, bpy.types.SpaceView3D)
    shown = bool(space.region_quadviews)
    try:
        assert quadded(view, shown=True) == Ok(None)
        assert len(space.region_quadviews) == 4
        assert quadded(view, shown=False) == Ok(None)
        assert len(space.region_quadviews) == 0
    finally:
        assert quadded(view, shown=shown) == Ok(None)


def test_view2d_coordinates_roundtrip_with_native_integer_rounding(view: Located) -> None:
    """Region pixels return within native integer rounding and clipped points remain absent."""
    area = next(area for area in view.window.screen.areas if area.type == "DOPESHEET_EDITOR")
    at = Located(window=view.window, area=area, region=Some(next(region for region in area.regions if region.type == "WINDOW")))
    match to_view(at, 40.0, 20.0):
        case Option(some=point):
            match to_region(at, *point):
                case Option(some=back):
                    assert all(abs(actual - wanted) <= 1 for actual, wanted in zip(back, (40, 20), strict=True))
                case absent:
                    pytest.fail(repr(absent))
            assert to_region(at, point[0] + 1.0e6, point[1]) == Nothing
        case absent:
            pytest.fail(repr(absent))
    assert to_view(Located(window=view.window, area=area), 40.0, 20.0) == Nothing


def test_workspace_creation_reuses_owned_native_identity(acquired: Acquired, view: Located) -> None:
    """Repeated workspace creation returns the same owned copy and pending activation can revert."""
    layout = WorkspaceSetup(key="layout", name="Fixture Layout", source=view.window.workspace.name, mode=Some("OBJECT"), use_pin_scene=Some(True), owners=Some(()))
    with acquired((layout,)) as identity:
        match layout.made(identity):
            case Result(tag="ok", ok=workspace):
                assert (workspace.name, workspace.object_mode, workspace.use_pin_scene, workspace.use_filter_by_owner) == (layout.name, "OBJECT", True, True)
                assert layout.made(identity).map(lambda again: again.session_uid) == Ok(workspace.session_uid)
            case Result(error=faults):
                pytest.fail(repr(faults))
        match layout.activated(identity, view.window):
            case Result(tag="ok", ok=activation):
                assert activation.advanced().map(lambda pending: pending.is_some()) == Ok(value=True)
                assert activation.reverted() == Ok(None)
            case Result(error=faults):
                pytest.fail(repr(faults))


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def view() -> Located:
    """Returns the native factory window's 3D View main region."""
    window = bpy.data.window_managers[0].windows[0]
    area = next(area for area in window.screen.areas if area.type == "VIEW_3D")
    return Located(window=window, area=area, region=Some(next(region for region in area.regions if region.type == "WINDOW")))
