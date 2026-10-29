"""Menus of every body, editor headers, the sections menus and headers refuse, and content appended or prepended to stock menus, panels, and context menus for the registration scope."""

import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "name,label,description,options",
    [
        ("FIXTURE_MT_main", "Main", "Fixture commands", ()),
        ("FIXTURE_MT_searched", "Searched", "Searchable commands", ("SEARCH_ON_KEY_PRESS",)),
        ("FIXTURE_MT_grouped", "Grouped", "Grouped commands", ()),
        ("FIXTURE_MT_grouped_first", "First", "First: Grouped commands", ()),
        ("FIXTURE_MT_grouped_second", "Second", "Second: Grouped commands", ()),
        ("FIXTURE_MT_pie", "Pie", "Radial commands", ()),
    ],
)
def test_menus_register_their_texts_and_options(observed: Observations, name: str, label: str, description: str, options: tuple[str, ...]) -> None:
    """Dropdowns, searchable dropdowns, pies, and one submenu per group present register with their texts, submenus titled by their group."""
    assert tuple(observed["surfaces.menus", name, "some", member] for member in ("bl_label", "bl_description", "bl_options")) == (label, description, options)


def test_header_registers_in_its_editor_region(observed: Observations) -> None:
    """A header registers in its editor's header region."""
    assert (observed["surfaces.headed", "some", "bl_space_type"], observed["surfaces.headed", "some", "bl_region_type"]) == ("VIEW_3D", "HEADER")


@pytest.mark.parametrize("case", ["menu section", "header section", "pie value"])
def test_refused_bodies_fail_registration(released: Observations, case: str) -> None:
    """A menu or header holding a section, whose layout panel the host refuses there, and a pie slot over a value that is no boolean or enum fail registration."""
    assert released["surfaces.refusals", case] == "RuntimeError"


def test_release_removes_menus_and_headers(released: Observations) -> None:
    """No menu or header class stays registered after release."""
    classes = released.of(dict, "surfaces.classes")
    assert [name for name, held in classes.items() if ("_MT_" in name or "_HT_" in name) and held] == []


def test_appends_extend_each_surface_while_registered(observed: Observations, released: Observations) -> None:
    """Appending to a menu, prepending to a panel, and appending to a context menu each extend a surface holding only its own draw, and release removes every added draw."""
    assert observed["surfaces.appended"] == {"before": (False, False, False), "now": (True, True, True)}
    assert released["surfaces.surfaces", "appended"] == (False, False, False)
