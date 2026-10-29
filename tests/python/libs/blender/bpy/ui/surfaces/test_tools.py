"""Toolbar tools registered through the host, their keymap, activation placement, and brush declarations."""

from expression import Ok, Some

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_tools_join_the_toolbar_after_their_anchor(observed: Observations) -> None:
    """The first tool lands after the named stock tool behind a separator, opening a flyout group the second tool joins."""
    toolbar = observed.of(tuple, ("surfaces.tooled", "toolbar"))
    group = toolbar.index(("group", ("fixture.probe", "fixture.probe_second")))
    assert ("tool", "builtin.transform") in toolbar[:group]
    assert toolbar[group - 1] == ("separator", None)


def test_tool_bindings_fill_the_host_tool_keymap(observed: Observations) -> None:
    """A tool's bindings become items of the keymap the host names for the tool, with the placement's values."""
    assert len(observed.of(tuple, ("surfaces.tooled", "keymaps"))) == 1
    assert (observed["surfaces.tooled", "keymaps", 0, 0], observed["surfaces.tooled", "keymaps", 0, 1, "event", 0], observed["surfaces.tooled", "keymaps", 0, 1, "properties"]) == (
        "3D View Tool: Object, Probe",
        "LEFTMOUSE",
        {"amount": 6},
    )


def test_activation_places_the_tool_set_operator(observed: Observations) -> None:
    """Activation places `wm.tool_set_by_id` over the tool and its space, which makes the tool active in its mode."""
    assert observed["surfaces.tooled", "activation"] == Ok(("wm.tool_set_by_id", (("name", "fixture.probe"), ("space_type", "VIEW_3D"))))
    assert (observed["surfaces.tooled", "ran"], observed["surfaces.tooled", "active"]) == (Ok(("FINISHED",)), Some("fixture.probe"))


def test_brush_type_requires_brushes(observed: Observations) -> None:
    """A brush type on a tool that offers no brushes raises `ValueError` at construction."""
    assert observed["surfaces.tooled", "brush"] == Ok("ValueError")


def test_release_removes_the_tools(released: Observations) -> None:
    """No extension tool stays in the toolbar after release."""
    assert released["surfaces.surfaces", "tools"] == ()
