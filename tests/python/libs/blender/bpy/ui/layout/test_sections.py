"""Sections over holders, their header and store parts, the section walk, and field rows derived from declared properties."""

from expression import Error, Ok
import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_fields_derive_rows_from_declared_properties(observed: Observations) -> None:
    """Scalars become property rows or their replacement, hidden members drop, a group collection becomes a list with a section over its highlighted item, and a data-block type ends with its custom properties."""
    assert observed["sections.fielded"] == {
        "look": ("Property", "Property", "ItemList", "Section look_entries_item"),
        "replaced": ("Label", "Property", "ItemList", "Section look_entries_item"),
        "scene": ("Section scene_custom_properties",),
    }


def test_walk_reaches_nested_sections_in_order(observed: Observations) -> None:
    """The walk finds each section ahead of those its rows, sub-layouts, header, and absent rows hold."""
    assert observed["sections.walked"] == ("nested", "inner", "headed", "absent", "fallback")


@pytest.mark.parametrize(
    "case,parts",
    [
        ("plain", Ok((False, False))),
        ("enabled", Ok((True, False))),
        ("stored", Ok((False, True))),
        ("items", Ok((False, False))),
        ("listed", Ok((False, False))),
        ("absent holder", Error(("ABSENT",))),
        ("unknown flag", Error(("ABSENT",))),
    ],
)
def test_sections_realize_their_header_parts(observed: Observations, case: str, parts: object) -> None:
    """A header boolean adds a header draw, a store adds the header preset draw, holders of every kind realize, and absent holder members or open flags fault."""
    assert observed["sections.realized", case] == parts


def test_section_realizes_as_a_layout_panel(observed: Observations) -> None:
    """A section realizes into a draw for a drawing layout's own panel."""
    assert observed["sections.realized", "drawn"] == Ok(True)
