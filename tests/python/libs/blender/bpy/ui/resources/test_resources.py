"""Staged glyphs, preview slots and tiles, GPU preview textures, region theme colors, interface metrics, and display colors under the fixture extension."""

from typing import Final

from expression import downcast, Error, Nothing, Ok
import pytest

from tests.python.libs.blender.bpy.conftest import observing
lazy from tests.python.libs.blender.bpy.conftest import Observations, Steps

# --- [CONSTANTS] ------------------------------------------------------------------------

CASES: Final[tuple[tuple[Steps, object], ...]] = (
    (
        ("enabled", "resources", "glyphs"),
        {"brand": True, "staged": Ok(True), "built in": Ok(True), "unknown": Error(("ABSENT",)), "missing": Error(("ABSENT",)), "tool path": ("icons", "tools", "brush")},
    ),
    (("enabled", "resources", "tinted"), {"opaque": True, "deviation": 0.0, "red": (True, False), "restored": (True, 0.0)}),
    (
        ("enabled", "resources", "previewed"),
        {"written": Ok(True), "after write": (True, True, (8, 4)), "emptied": Ok(Nothing), "after empty": (True, Nothing), "cleared": Ok(None), "after clear": False},
    ),
    (
        ("enabled", "resources", "checkered"),
        {"transparent": ((1.0, 1.0, 0.0, 0.0), (1.0, 1.0, 0.0, 0.0), (0.0, 0.0, 1.0, 1.0), (0.0, 0.0, 1.0, 1.0)), "opaque": (1.0, 0.0, 0.0, 1.0), "alpha": 1.0, "side": True},
    ),
    (("enabled", "resources", "textured", "uploaded"), Ok((5, 3, "RGBA16F"))),
    (("enabled", "resources", "textured", "stored"), (Ok(("first",)), Ok(("first", "second")), Ok(("first", "third")))),
    (("enabled", "resources", "textured", "shown"), True),
    (("enabled", "resources", "textured", "order"), ("third", "first")),
    (("enabled", "resources", "textured", "memory"), 2 * 5 * 3 * 4 * 2),
    (("enabled", "resources", "themed", "project editor"), True),
    (("enabled", "resources", "themed", "absent editor"), True),
    (("enabled", "resources", "measured", "scale"), (1.0, 0.0, 1.0)),
    (("enabled", "resources", "measured", "unit"), 20),
    (("enabled", "resources", "measured", "panel header"), 25),
    (("enabled", "resources", "measured", "line"), (1.0, 2.0)),
    (("enabled", "resources", "measured", "points"), True),
    (("enabled", "resources", "measured", "checker"), True),
    (("enabled", "resources", "colored"), {"color": (0.25, 0.5, 0.75, 1.0), "rgb": (0.25, 0.5, 0.75, 1.0), "rgba": (0.25, 0.5, 0.75, 0.5)}),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@observing(CASES)
def test_resources(observed: Observations, path: Steps, expected: object) -> None:
    """Each observation equals its case."""
    assert observed[path] == expected


@pytest.mark.parametrize("member", ["view back", "sidebar back", "preview back", "panel text", "channel text", "editor text"])
def test_region_colors_read_their_theme_member(observed: Observations, member: str) -> None:
    """Main region backs read the editor gradient, overlapping sidebars keep their stored alpha, the sequencer preview its own back, and text its region's theme member."""
    library, host = downcast(tuple, observed["enabled", "resources", "themed", member])
    assert library == host


def test_regions_without_overlap_draw_opaque(observed: Observations) -> None:
    """A header of an editor whose regions never overlap reads its theme color fully opaque."""
    library, host = downcast(tuple, observed["enabled", "resources", "themed", "header back"])
    assert (library[:3], library[3]) == (host[:3], 1.0)


def test_header_height_matches_host_headers(observed: Observations) -> None:
    """The header height matches one of the host's header regions."""
    height, headers = downcast(tuple, observed["enabled", "resources", "measured", "header"])
    assert height in headers
