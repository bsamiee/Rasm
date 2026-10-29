"""Image editor views: the render view command, the editor showing Render Result, the pick command's deferred invocation and properties, and frame region handles with their bounds."""

from typing import Final

from expression import Error, Some

from tests.python.libs.blender.bpy.conftest import observing, Steps
lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [CONSTANTS] ------------------------------------------------------------------------

CASES: Final[tuple[tuple[Steps, object], ...]] = (
    (("enabled", "images", "viewed", "command"), True),
    (("enabled", "images", "viewed", "operation"), True),
    (("enabled", "images", "viewed", "viewing"), Error(("ABSENT",))),
    (("enabled", "images", "picked", "options"), ("DEPENDS_ON_CURSOR", "REGISTER", "UNDO")),
    (("enabled", "images", "picked", "cursor"), Some("EYEDROPPER")),
    (("enabled", "images", "picked", "members"), ("invoke",)),
    (("enabled", "images", "picked", "properties"), ("radius", "x", "y")),
    (("released", "images", "released_images", "pick"), False),
    (("enabled", "images", "caged", "group"), ("IMAGE_EDITOR", "WINDOW")),
    (("released", "images", "released_images", "group"), False),
    (("enabled", "images", "caged", "cage"), (1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0)),
    (("enabled", "images", "caged", "corner"), (0.0, 0.0, 0.0)),
    (("enabled", "images", "caged", "edit"), Error(("ABSENT",))),
    (("enabled", "images", "caged", "moved"), ((0.6, 0.9), (1.0, 0.9), (1.0, 0.5), (0.6, 0.5))),
    (("enabled", "images", "caged", "stretched"), ((0.0, 1.0), (1.0, 1.0), (1.0, 0.5), (0.0, 0.5))),
    (("enabled", "images", "caged", "unbounded"), ((2.0, 2.0),)),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@observing(CASES)
def test_images(observed: Observations, path: Steps, expected: object) -> None:
    """Each observation equals its case."""
    assert observed[path] == expected
