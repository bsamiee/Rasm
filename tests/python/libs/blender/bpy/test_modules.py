"""Every `blender` library module imports under Blender's interpreter."""

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_modules(observed: Observations) -> None:
    """No library module raises on import."""
    assert observed["enabled", "modules", "imported"] == ()
