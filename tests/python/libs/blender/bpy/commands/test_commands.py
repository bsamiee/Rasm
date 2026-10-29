"""Commands: host names, keymap-data placements and calls, operator and macro classes with their texts and flags, invoke forms, the modal form, add-on shortcuts, and body answers through the host."""

import sys
from typing import Final

from expression import Error, Nothing, Ok, Some

from tests.python.libs.blender.bpy.conftest import observing, Steps
lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [CONSTANTS] ------------------------------------------------------------------------

ON: Final = ("enabled", "commands")
OFF: Final = ("released", "commands")
COUNTED: Final = {"idname": "fixture.counted", "event": ("F", "PRESS", 0, 1, 0, 0), "properties": {"amount": 4}}
ITEMS: Final = (
    *(({**COUNTED, "event": ("F", "PRESS", 0, 0, 0, 1)},) if sys.platform == "darwin" else ()),
    COUNTED,
    {"idname": "wm.context_toggle", "event": ("RIGHTMOUSE", "PRESS", 0, 0, 1, 0), "properties": {"data_path": "scene.use_gravity"}},
    {"idname": "wm.context_set_value", "event": ("G", "PRESS", 1, 0, 0, 0), "properties": {"data_path": "scene.frame_current", "value": "5"}},
    {"idname": "wm.call_menu_pie", "event": ("Q", "PRESS", 0, 0, 1, 0), "properties": {"name": "VIEW3D_MT_pivot_pie"}},
)
CASES: Final[tuple[tuple[Steps, object], ...]] = (
    *(
        ((*ON, "named", case), Ok(name))
        for case, name in (
            ("command", "fixture.counted"),
            ("menu", "FIXTURE_MT_main"),
            ("pie", "FIXTURE_MT_main"),
            ("panel", "FIXTURE_PT_main"),
            ("shelf", "FIXTURE_AST_main"),
            ("stock menu", "VIEW3D_MT_add"),
            ("stock operator", "wm.context_toggle"),
        )
    ),
    ((*ON, "placed", "command"), Ok(("fixture.counted", (("amount", 4),)))),
    ((*ON, "placed", "pie"), Ok(("wm.call_menu_pie", (("name", "VIEW3D_MT_pivot_pie"),)))),
    ((*ON, "placed", "toggle"), Ok(("wm.context_toggle", (("data_path", "scene.use_gravity"),)))),
    ((*ON, "placed", "assign"), Ok(("wm.context_set_value", (("data_path", "scene.frame_current"), ("value", "5"))))),
    ((*ON, "placed", "unfilled"), Error(("ABSENT",))),
    ((*ON, "mismatched"), "TypeError"),
    ((*ON, "unbound"), Error(("ABSENT",))),
    *(
        ((*ON, "classes", key, member), value)
        for key, options, members, properties in (
            ("counted", ("REGISTER", "UNDO"), (), ("amount", "mode")),
            ("kept", ("REGISTER", "UNDO"), (), ()),
            ("grouped", ("REGISTER", "UNDO_GROUPED"), (), ()),
            ("plain", ("INTERNAL", "PRESET", "REGISTER"), (), ()),
            ("seeded", ("REGISTER", "UNDO"), ("invoke",), ("amount", "mode")),
            ("stepped", ("REGISTER", "UNDO"), (), ("amount", "mode")),
            ("dialog", ("REGISTER", "UNDO"), ("invoke",), ("amount", "mode")),
            ("redo", ("REGISTER", "UNDO"), ("invoke",), ("amount", "mode")),
            ("search", ("REGISTER", "UNDO"), ("invoke",), ("amount", "mode")),
            ("confirmed", ("REGISTER", "UNDO"), ("draw", "invoke"), ("amount", "mode", "suppress")),
            ("opened", ("REGISTER", "UNDO"), ("check", "invoke"), ("directory", "filepath", "files", "filter_glob")),
            ("saved", ("REGISTER", "UNDO"), ("check", "invoke"), ("check_existing", "filepath", "filter_glob")),
            ("folder", ("REGISTER", "UNDO"), ("invoke",), ("directory", "filter_folder")),
            ("picked", ("DEPENDS_ON_CURSOR", "REGISTER", "UNDO"), (), ()),
            ("dragged", ("BLOCKING", "GRAB_CURSOR", "MODAL_PRIORITY", "REGISTER", "UNDO"), ("cancel", "invoke", "modal"), ()),
        )
        for member, value in (("options", options), ("members", members), ("properties", properties))
    ),
    ((*ON, "classes", "twice", "options"), ("REGISTER", "UNDO")),
    ((*ON, "classes", "twice", "properties"), ("FIXTURE_OT_counted", "FIXTURE_OT_counted")),
    ((*ON, "classes", "counted", "label"), "Counted"),
    ((*ON, "classes", "counted", "description"), "Adds the amount to the scene count, or sets the count to it"),
    ((*ON, "classes", "stepped", "description"), "Sets the scene count to the amount times the length of the bound manifest ID"),
    ((*ON, "classes", "twice", "label"), "Twice"),
    ((*ON, "classes", "twice", "description"), "Counts by two, then by three"),
    ((*ON, "classes", "counted", "property"), Some("mode")),
    ((*ON, "classes", "kept", "property"), Nothing),
    ((*ON, "classes", "grouped", "undo_group"), Some("fixture")),
    ((*ON, "classes", "opened", "helpers"), ("ImportHelper",)),
    ((*ON, "classes", "saved", "helpers"), ("ExportHelper",)),
    ((*ON, "classes", "picked", "cursor"), Some("EYEDROPPER")),
    *(
        ((*ON, "executed", case), (answer, count))
        for case, answer, count in (
            ("counted", Ok(("FINISHED",)), 3),
            ("set", Ok(("FINISHED",)), 3),
            ("unset", Ok(("FINISHED",)), 1),
            ("kept", Error(("ANSWERED",)), 0),
            ("refused", Error(("FAILED",)), 0),
            ("macro", Ok(("FINISHED",)), 5),
            ("bogus", Error(("FAILED",)), 0),
            ("over", Ok(("FINISHED",)), 14),
        )
    ),
    ((*ON, "refusal"), "Invalid Context Error: Requires a fixture strip\n"),
    ((*ON, "answered"), {"all": Ok({"notes": (), "skipped": Nothing}), "some": Ok({"notes": (), "skipped": Some(("ABSENT",))}), "none": Error(("ABSENT", "ABSENT"))}),
    ((*ON, "suppressed", "before", "some"), True),
    ((*ON, "suppressed", "ran"), (Ok(("FINISHED",)), 1)),
    ((*ON, "suppressed", "after", "some"), False),
    ((*ON, "browsed"), {"unchosen": (Error(("FAILED",)), ""), "folder": (Ok(("FINISHED",)), "<folder>"), "opened": (Ok(("FINISHED",)), "chosen.txt")}),
    ((*ON, "shortcuts"), {"items": ITEMS, "counterparts": len(ITEMS)}),
    ((*ON, "nested"), {"view": Ok((("3D View", "VIEW_3D", "WINDOW"),)), "absent": Error(("ABSENT",))}),
    *(
        ((*ON, "gestures", gesture), options)
        for gesture, options in (("DRAG", ("BLOCKING", "GRAB_CURSOR")), ("DRAG_X", ("BLOCKING", "GRAB_CURSOR_X")), ("DRAG_Y", ("BLOCKING", "GRAB_CURSOR_Y")), ("TRACK", ("BLOCKING",)), ("TIMED", ()))
    ),
    ((*OFF, "operators"), ()),
    ((*OFF, "keys"), ()),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@observing(CASES)
def test_commands(observed: Observations, path: Steps, expected: object) -> None:
    """Each command observation equals its case."""
    assert observed[path] == expected
