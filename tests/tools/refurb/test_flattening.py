"""RASM107: iterables flattened through `itertools.chain` or a nested comprehension, where Python 3.15 comprehension unpacking states the flattening."""

import pytest
lazy from refurb.error import Error

from rasm.refurb.flattening import ErrorChainFlatteningError

# --- [CONSTANTS] ------------------------------------------------------------------------

pytestmark = pytest.mark.parametrize("rule", [ErrorChainFlatteningError], scope="module")

CASES = frozendict(
    from_iterable="from itertools import chain\n\nflat = list(chain.from_iterable([[1], [2]]))\n",
    unpacked_chain="from itertools import chain\n\nrows = [[1], [2]]\nspread = chain(*rows)\n",
    nested_list="rows = [[1], [2]]\nnested = [item for row in rows for item in row]\n",
    nested_dictionary="tables = [{'a': 1}]\nmerged = {key: value for table in tables for key, value in table.items()}\n",
    listed_chain="from itertools import chain\n\nrows = [[1], [2]]\npairs = chain(rows[0], rows[1])\n",
    transformed_item="rows = [[1], [2]]\ndoubled = [item * 2 for row in rows for item in row]\n",
    filtered_item="rows = [[1], [2]]\nkept = [item for row in rows for item in row if item]\n",
    other_source="rows = [[1], [2]]\nother = [3]\ncrossed = [item for row in rows for item in other]\n",
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "case, lines",
    [("from_iterable", [3]), ("unpacked_chain", [4]), ("nested_list", [2]), ("nested_dictionary", [2]), ("listed_chain", []), ("transformed_item", []), ("filtered_item", []), ("other_source", [])],
)
def test_flattening_reports_on_315(findings: frozendict[str, tuple[Error, ...]], case: str, lines: list[int]) -> None:
    """`chain.from_iterable`, `chain(*rows)`, and two-level comprehensions yielding each inner item of the row they iterate report, a listed chain or a changed, filtered, or foreign item passes."""
    assert [error.line for error in findings[case]] == lines


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
def test_unpacked_chain_alone_reports_below_315(findings: frozendict[str, tuple[Error, ...]]) -> None:
    """`chain(*rows)` alone reports where comprehension unpacking does not parse."""
    assert {case: tuple(error.line for error in errors) for case, errors in findings.items()} == dict.fromkeys(CASES, ()) | {"unpacked_chain": (4,)}
