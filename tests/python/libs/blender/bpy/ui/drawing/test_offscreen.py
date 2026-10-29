"""Offscreen buffers: framebuffer reads top row first under one bind, blend scopes, and the texture size limit."""

from expression import Error, Ok

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_reads_return_normalized_rgba(observed: Observations) -> None:
    """A rectangle read inside the draw and the whole buffer read after it return float RGBA of the cleared color at their sizes."""
    assert observed["drawing.read back", "corner"] == Ok(((3, 2, 4), (0.0, 1.0, 0.0, 1.0)))
    assert observed["drawing.read back", "whole"] == Ok(((8, 8, 4), (0.0, 1.0, 0.0, 1.0)))


def test_blend_scope_restores_the_mode_it_found(observed: Observations) -> None:
    """A blend scope sets its mode inside and restores the prior mode at exit."""
    assert observed["drawing.read back", "blend"] == ("ADDITIVE", "NONE")


def test_buffers_past_the_texture_limit_are_refused(observed: Observations) -> None:
    """A buffer wider than the GPU's texture limit faults before allocation."""
    assert observed["drawing.read back", "past limit"] == Error(("OUTSIDE",))
