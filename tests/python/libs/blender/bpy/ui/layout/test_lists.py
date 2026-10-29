"""Collection lists: the registered list class, its declared selection, drag reorder, and refused declarations."""

from expression import Error, Ok

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_list_registers_its_class_selection_and_drag(observed: Observations) -> None:
    """A list registers its class, declares its selection index beside the collection, and registers the drag command a script collection reorders through."""
    assert {key: observed["sections.listed", key] for key in ("class", "index", "drag", "picked", "drawn")} == {"class": True, "index": True, "drag": True, "picked": Ok(True), "drawn": Ok(True)}


def test_list_refuses_compact_filters_and_foreign_items(observed: Observations) -> None:
    """A filter on a compact list, which never filters, and an item type the collection does not hold fault."""
    assert (observed["sections.listed", "compact"], observed["sections.listed", "foreign"]) == (Error(("OUTSIDE",)), Error(("OUTSIDE",)))


def test_release_removes_list_and_selection(released: Observations) -> None:
    """Neither the list class nor its selection index stays registered after release."""
    assert (released["sections.lists", "class"], released["sections.lists", "index"]) == (False, False)
