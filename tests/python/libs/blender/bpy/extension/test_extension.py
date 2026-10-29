"""Extension: identity names and reads, band order and reverse release, dependency-ordered classes, registries, refused registrations unwinding, host event callbacks, held resources, and callback fault reports."""

from typing import Final
from uuid import NAMESPACE_URL, uuid5

from expression import Error, Nothing, Ok, Some

from tests.python.libs.blender.bpy.conftest import observing, Steps
lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [CONSTANTS] ------------------------------------------------------------------------

ON: Final = ("enabled", "extension")
OFF: Final = ("released", "extension")
ENTERED: Final = ("enter GROUPS", "enter INTERFACE", "enter TIMERS")
CLASSES: Final = ("ChildPanel", "ParentPanel", "Outer", "Inner", "FIXTURE_PT_deferred")
OPERATORS: Final = ("confirmed", "counted", "dialog", "dragged", "folder", "grouped", "kept", "opened", "picked", "plain", "redo", "refused", "saved", "search", "seeded", "stepped", "twice")
CASES: Final[tuple[tuple[Steps, object], ...]] = (
    *(
        ((*ON, "names", base), Ok(name))
        for base, name in (
            ("Panel", "FIXTURE_PT_main"),
            ("Menu", "FIXTURE_MT_main"),
            ("Header", "FIXTURE_HT_main"),
            ("UIList", "FIXTURE_UL_main"),
            ("AssetShelf", "FIXTURE_AST_main"),
            ("FileHandler", "FIXTURE_FH_main"),
            ("GizmoGroup", "FIXTURE_GGT_main"),
            ("Gizmo", "FIXTURE_GT_main"),
            ("RenderEngine", "FIXTURE_MAIN"),
            ("Operator", "fixture.main"),
            ("PropertyGroup", f"GROUP_{uuid5(NAMESPACE_URL, 'fixture/main').hex}"),
        )
    ),
    ((*ON, "refusals"), {"long": Error(("UNREGISTERED",)), "key": Error(("UNREGISTERED",)), "identifier": Ok("fixture_amount")}),
    ((*ON, "read", "package"), "bl_ext.user_default.fixture"),
    ((*ON, "read", "manifest"), ("fixture", "Fixture", "1.0.0", "https://docs.example.org/fixture/")),
    ((*ON, "read", "folder"), ("user_default", "fixture")),
    ((*ON, "read", "foreign"), Error(("UNREADABLE", "UNREGISTERED"))),
    ((*ON, "foreign"), Nothing),
    ((*ON, "ordered", "events"), ENTERED),
    ((*ON, "ordered", "registered"), dict.fromkeys(CLASSES, True)),
    ((*ON, "ordered", "parent"), "FIXTURE_PT_child"),
    ((*ON, "texts", "translated"), "Bonjour Fixture"),
    ((*ON, "texts", "preferences"), Some("bl_ext.user_default.fixture")),
    ((*ON, "texts", "tally"), Some(0)),
    ((*ON, "depsgraph"), (("Probe", "TRANSFORM"),)),
    ((*ON, "scened"), False),
    *(
        ((*ON, "levels", reason), level)
        for reason, level in (
            ("OUTSIDE", "ERROR_INVALID_INPUT"),
            ("INVALID", "ERROR_INVALID_INPUT"),
            ("ABSENT", "ERROR_INVALID_INPUT"),
            ("TAKEN", "ERROR_INVALID_INPUT"),
            ("REQUIRED", "ERROR_INVALID_CONTEXT"),
            ("UNREGISTERED", "ERROR"),
            ("REFUSED", "ERROR"),
            ("ANSWERED", "ERROR"),
            ("FAILED", "ERROR"),
            ("UNREADABLE", "ERROR"),
            ("UNWRITABLE", "ERROR"),
        )
    ),
    ((*ON, "held"), ("ERROR_INVALID_INPUT", "ERROR")),
    ((*ON, "queued"), {"outcome": Ok(None), "steps": (3, 2, 1)}),
    ((*ON, "deferred"), ("inside", "inside")),
    ((*ON, "reloaded", "answer"), ("FINISHED",)),
    ((*ON, "reloaded", "loads"), (Nothing,)),
    ((*ON, "reloaded", "starts"), 1),
    ((*ON, "reloaded", "entries"), ("ERROR_INVALID_CONTEXT",)),
    ((*ON, "reloaded", "tally"), (Some(0), Some(1))),
    (("enabled", "commands", "commanded"), {"code": Ok(3), "missing": Error(("ABSENT",)), "garbled": Error(("UNREADABLE",))}),
    ((*OFF, "order", "events"), (*ENTERED, "exit TIMERS", "exit INTERFACE", "exit GROUPS")),
    ((*OFF, "order", "registered"), dict.fromkeys(CLASSES, False)),
    ((*OFF, "order", "handlers"), True),
    ((*OFF, "withdrawn"), {"translated": "Fixture Hello", "patterns": (), "preferences": Nothing, "tally": Nothing}),
    ((*OFF, "refusing"), {panel: {"raised": "RuntimeError", "operators": (), "panel": False, "handlers": True} for panel in ("orphan", "drawless")}),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@observing(CASES)
def test_extension(observed: Observations, path: Steps, expected: object) -> None:
    """Each extension observation equals its case."""
    assert observed[path] == expected


def test_manual_map_links_operators_types_and_preferences(observed: Observations) -> None:
    """The manifest website maps every extension operator, extension types and members, the preferences struct, and attached groups."""
    patterns = observed[*ON, "texts", "patterns"]
    assert isinstance(patterns, tuple)
    expected = {("bpy.types.fixture_*", ""), ("bpy.types.*.fixture_*", ""), ("bpy.types.bl_ext.user_default.fixture.*", ""), ("bpy.types.look.*", "")}
    assert {*((f"bpy.ops.fixture.{key}", f"#{key}") for key in OPERATORS), *expected} <= frozenset(patterns)
