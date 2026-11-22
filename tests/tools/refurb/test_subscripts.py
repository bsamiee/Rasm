"""RASM109: runtime subscripts of classes that gained `__class_getitem__` in Python 3.14."""

import pytest
lazy from refurb.error import Error

from rasm.refurb.subscripts import ErrorUnsubscriptableBuiltinError

# --- [CONSTANTS] ------------------------------------------------------------------------

pytestmark = pytest.mark.parametrize("rule", [ErrorUnsubscriptableBuiltinError], scope="module")

CASES = frozendict(
    assigned_memoryview="View = memoryview[int]\n",
    assigned_slice="Part = slice[int, int, int]\n",
    base_class="class Window(memoryview[int]): ...\n",
    generic_builtin="Items = list[int]\n",
    type_alias="type Lazy = memoryview[int]\n",
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
@pytest.mark.parametrize("case, lines", [("assigned_memoryview", [1]), ("assigned_slice", [1]), ("base_class", [1]), ("generic_builtin", []), ("type_alias", [])])
def test_an_evaluated_subscript_reports_below_314(findings: frozendict[str, tuple[Error, ...]], case: str, lines: list[int]) -> None:
    """Assigned and base-class subscripts of `memoryview` and `slice` report, a generic builtin and a lazy `type` alias pass."""
    assert [error.line for error in findings[case]] == lines


def test_every_subscript_passes_on_315(findings: frozendict[str, tuple[Error, ...]]) -> None:
    """No subscript reports where each class declares `__class_getitem__`."""
    assert findings == frozendict.fromkeys(CASES, ())
