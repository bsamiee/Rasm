"""Located editors, editor queries, and arrangements applied one host pass per step and reverted."""

from expression import Error, Ok, Some

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_editors_locate_by_query_and_region(observed: Observations) -> None:
    """A background context holds no area, queries find the largest matching editor or fault, every screen and global area lists, and regions locate by type."""
    assert observed["screens.located", "current"] == Error(("ABSENT",))
    assert (observed["screens.located", "view"], observed["screens.located", "image"]) == (Ok("VIEW_3D"), Error(("ABSENT",)))
    assert observed["screens.located", "areas"] == (*observed.of(tuple, ("screens.located", "screen")), "TOPBAR", "STATUSBAR")
    assert (observed["screens.located", "main"], observed["screens.located", "preview"], observed["screens.located", "within"]) == (Ok("WINDOW"), Error(("ABSENT",)), Ok(Some("HEADER")))


def test_query_orders_editors_largest_first(observed: Observations) -> None:
    """Matching editors list from the largest area down."""
    sizes = observed.of(tuple, ("screens.located", "sizes"))
    assert sizes == tuple(sorted(sizes, reverse=True))


def test_retype_applies_and_reverts(observed: Observations) -> None:
    """An arrangement retypes the dope sheet to the Graph Editor, settles, and its revert restores the editor type."""
    assert observed["screens.retyped"] == {"before": "TIMELINE", "applied": "FCURVES", "settled": Ok(None), "reverted": Ok(None), "after": "TIMELINE"}


def test_split_adds_the_editor_at_its_edge(observed: Observations) -> None:
    """A right split adds a Text Editor right of the source area at its share and holds one revert."""
    width, before = observed.of(tuple, ("screens.split", "before"))
    _, after = observed.of(tuple, ("screens.split", "after"))
    _, text_x = observed.of(tuple, ("screens.split", "x"))
    text_width = observed.of(int, ("screens.split", "text"))
    assert (after, observed["screens.split", "reverts"]) == ((*before, "TEXT_EDITOR"), Ok(1))
    assert (abs(text_width - 0.25 * width) <= 1, text_x + text_width) == (True, width)


def test_split_refuses_a_share_past_half(observed: Observations) -> None:
    """A share leaving the source smaller than the new editor raises `ValueError` at construction."""
    assert observed["screens.shared"] == "ValueError"
