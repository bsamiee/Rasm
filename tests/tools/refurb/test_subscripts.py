"""Runtime subscripts of classes that gained `__class_getitem__` in Python 3.14."""

from textwrap import dedent
lazy from collections.abc import Callable, Mapping

import pytest
lazy from refurb.error import Error

lazy from rasm.refurb.subscripts import ErrorUnsubscriptableBuiltinError

# --- [TYPES] ----------------------------------------------------------------------------

type Findings = Callable[[str | Mapping[str, str], *tuple[type[Error], ...]], tuple[Error, ...]]

# --- [CONSTANTS] ------------------------------------------------------------------------

SOURCE = dedent("""
    View = memoryview[int]
    Part = slice[int, int, int]
    Items = list[int]
    type Lazy = memoryview[int]

    class Window(memoryview[int]): ...
""")

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
def test_unsubscriptable_reports_below_314(findings: Findings) -> None:
    """Assigned and base-class subscripts of `memoryview` and `slice` report, a generic builtin and a lazy `type` alias pass."""
    assert [error.line for error in findings(SOURCE, ErrorUnsubscriptableBuiltinError)] == [2, 3, 7]


def test_unsubscriptable_passes_on_315(findings: Findings) -> None:
    """Every subscript passes where the classes declare `__class_getitem__`."""
    assert findings(SOURCE, ErrorUnsubscriptableBuiltinError) == ()
