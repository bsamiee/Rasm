"""Layout rows, templates, tables, sections, lists, value stores, and preset listings realized and run under the fixture extension."""

from typing import Final

from expression import Error, Ok, Some

from tests.python.libs.blender.bpy.conftest import observing
lazy from tests.python.libs.blender.bpy.conftest import Observations, Steps

# --- [CONSTANTS] ------------------------------------------------------------------------

_DRAWN: Final = Ok(True)
_FINISHED: Final = Ok(("FINISHED",))
_ABSENT: Final = Error(("ABSENT",))
_OUTSIDE: Final = Error(("OUTSIDE",))

CASES: Final[tuple[tuple[Steps, object], ...]] = (
    *((("enabled", "layout", "bound", case), _DRAWN) for case in ("present", "element", "reached", "scoped", "glyph", "function", "search", "struct", "mixed", "equal")),
    (("enabled", "layout", "bound", "absent"), _ABSENT),
    (("enabled", "layout", "bound", "foreign struct"), _OUTSIDE),
    (("enabled", "layout", "bound", "keyword"), _OUTSIDE),
    (("enabled", "layout", "bound", "unknown glyph"), _ABSENT),
    *((("enabled", "layout", "pressed", case), _DRAWN) for case in ("command", "placement", "menu", "panel")),
    (("enabled", "layout", "pressed", "keyword"), _OUTSIDE),
    (("enabled", "layout", "pressed", "pie"), _ABSENT),
    (("enabled", "layout", "pressed", "acquired"), ("Operator",)),
    *((("enabled", "layout", "labelled", case), _DRAWN) for case in ("fixed", "read", "single", "status")),
    (("enabled", "layout", "labelled", "keyword"), _OUTSIDE),
    *((("enabled", "layout", "templated", case), _DRAWN) for case in ("preview", "format", "wheel", "shortcut")),
    (("enabled", "layout", "templated", "preview of value"), _OUTSIDE),
    (("enabled", "layout", "templated", "format keyword"), _OUTSIDE),
    *((("enabled", "layout", "tabled", case), _DRAWN) for case in ("matrix", "tree", "tree matrix")),
    (("enabled", "layout", "tabled", "absent setting"), _ABSENT),
    (("enabled", "layout", "composed"), {"layout": _DRAWN, "built": _DRAWN, "acquired": ("OperatorButton",), "refusals": Error(("OUTSIDE", "ABSENT"))}),
    (("enabled", "layout", "checked"), {"prop": Ok({"text": "Fixture", "slider": True}), "unknown": _OUTSIDE}),
    (("enabled", "layout", "fielded", "look"), ("Property", "Property", "ItemList", "Section look_entries_item")),
    (("enabled", "layout", "fielded", "replaced"), ("Label", "Property", "ItemList", "Section look_entries_item")),
    (("enabled", "layout", "fielded", "scene"), ("Section scene_custom_properties",)),
    (("enabled", "layout", "walked"), ("nested", "inner", "headed", "absent", "fallback")),
    (("enabled", "layout", "sectioned", "plain"), Ok((False, False))),
    (("enabled", "layout", "sectioned", "enabled"), Ok((True, False))),
    (("enabled", "layout", "sectioned", "stored"), Ok((False, True))),
    (("enabled", "layout", "sectioned", "items"), Ok((False, False))),
    (("enabled", "layout", "sectioned", "listed"), Ok((False, False))),
    (("enabled", "layout", "sectioned", "absent holder"), _ABSENT),
    (("enabled", "layout", "sectioned", "unknown flag"), _ABSENT),
    (("enabled", "layout", "sectioned", "drawn"), _DRAWN),
    (("enabled", "layout", "listed"), {"class": True, "index": True, "drag": True, "picked": _DRAWN, "drawn": _DRAWN, "compact": _OUTSIDE, "foreign": _OUTSIDE}),
    (("enabled", "layout", "stored", "verbs"), ("COPY", "FORGET", "PASTE", "RESET", "RESTORE")),
    (("enabled", "layout", "stored", "reset"), (_FINISHED, 1.0)),
    (("enabled", "layout", "stored", "older"), (_FINISHED, 2.0)),
    (("enabled", "layout", "stored", "newer"), (_FINISHED, 1.0)),
    (("enabled", "layout", "stored", "unchanged"), Error(("ANSWERED",))),
    (("enabled", "layout", "stored", "unhosted"), Error(("FAILED",))),
    (("enabled", "layout", "preset", "verbs"), ("APPLY", "DELETE", "EXPORT", "IMPORT", "RENAME", "REPLACE", "SAVE", "STEP")),
    (("enabled", "layout", "preset", "applied"), (_FINISHED, 1)),
    (("enabled", "layout", "preset", "next"), (_FINISHED, 5)),
    (("enabled", "layout", "preset", "wrapped"), (_FINISHED, 1)),
    (("enabled", "layout", "preset", "saved"), (_FINISHED, 9)),
    (("enabled", "layout", "preset", "listed"), ("soft", "hard", "Mine")),
    (("enabled", "layout", "preset", "taken"), Error(("FAILED",))),
    (("enabled", "layout", "preset", "menu"), Some(("SEARCH_ON_KEY_PRESS",))),
    (("released", "layout", "released_lists"), {"class": False, "index": False, "attached": False, "menu": False}),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@observing(CASES)
def test_layout(observed: Observations, path: Steps, expected: object) -> None:
    """Each observation equals its case: rows realize or fault by kind, sections expose their header parts, and store, list, and preset commands leave their values."""
    assert observed[path] == expected
