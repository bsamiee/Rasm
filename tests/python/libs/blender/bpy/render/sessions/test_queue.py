"""Queue `prepare` launch: exit unread while the child runs, its status once it exits, and a `FAILED` fault only for an exit that left no sent or refused record."""

from typing import Final

from expression import Nothing, Ok, Some

from tests.python.libs.blender.bpy.conftest import observing, Steps
lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [CONSTANTS] ------------------------------------------------------------------------

ON: Final = ("enabled", "queue", "prepared")
CASES: Final[tuple[tuple[Steps, object], ...]] = (
    ((*ON, "launched"), (Ok(None), Ok(None))),
    ((*ON, "unstarted"), Nothing),
    ((*ON, "running"), Nothing),
    ((*ON, "exited"), Some(3)),
    ((*ON, "unrecorded"), Some(("FAILED",))),
    ((*ON, "message"), Some("prepare failed. exit status 3 with no .sent.json or .refused.json record")),
    ((*ON, "refused"), Nothing),
    ((*ON, "alive"), Nothing),
    ((*ON, "logged"), True),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@observing(CASES)
def test_queue(observed: Observations, path: Steps, expected: object) -> None:
    """Each observation equals its case."""
    assert observed[path] == expected
