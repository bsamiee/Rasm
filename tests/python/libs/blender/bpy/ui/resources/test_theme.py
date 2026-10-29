"""Region theme colors by the host's theme rules, interface metrics against host sizes, and display colors."""

import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("member", ["view back", "sidebar back", "preview back", "panel text", "channel text", "editor text"])
def test_region_colors_read_their_theme_member(observed: Observations, member: str) -> None:
    """Main region backs read the editor gradient, overlapping sidebars keep their stored alpha, the sequencer preview its own back, and text its region's theme member."""
    library, host = observed.of(tuple, ("resources.themed", member))
    assert library == host


def test_regions_without_overlap_draw_opaque(observed: Observations) -> None:
    """A header of an editor whose regions never overlap reads its theme color fully opaque."""
    library, host = observed.of(tuple, ("resources.themed", "header back"))
    assert (library[:3], library[3]) == (host[:3], 1.0)


def test_editor_theme_falls_back_by_host_rule(observed: Observations) -> None:
    """Project Settings read the Preferences theme and a space the theme lacks reads the 3D View theme."""
    assert (observed["resources.themed", "project editor"], observed["resources.themed", "absent editor"]) == (True, True)


def test_metrics_match_host_sizes(observed: Observations) -> None:
    """With no window scale the view scale serves, the header height matches the host's header regions, and widget sizes derive from the unit."""
    assert observed["resources.measured", "scale"] == (1.0, 0.0, 1.0)
    height, headers = observed.of(tuple, ("resources.measured", "header"))
    assert height in headers
    assert (observed["resources.measured", "unit"], observed["resources.measured", "panel header"], observed["resources.measured", "line"]) == (20, 25, (1.0, 2.0))
    assert (observed["resources.measured", "points"], observed["resources.measured", "checker"]) == (True, True)


def test_colors_read_as_display_rgba(observed: Observations) -> None:
    """Color values and RGB sequences read opaque, RGBA sequences keep their alpha."""
    assert observed["resources.colored"] == {"color": (0.25, 0.5, 0.75, 1.0), "rgb": (0.25, 0.5, 0.75, 1.0), "rgba": (0.25, 0.5, 0.75, 0.5)}
