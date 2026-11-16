"""Iterables flattened through `itertools.chain` or a nested comprehension on Python 3.15."""

from textwrap import dedent
lazy from collections.abc import Callable, Mapping

import pytest
lazy from refurb.error import Error

lazy from rasm.refurb.flattening import ErrorChainFlatteningError

# --- [TYPES] ----------------------------------------------------------------------------

type Findings = Callable[[str | Mapping[str, str], *tuple[type[Error], ...]], tuple[Error, ...]]

# --- [CONSTANTS] ------------------------------------------------------------------------

SOURCE = dedent("""
    from itertools import chain

    rows: list[list[int]] = [[1], [2]]
    tables: list[dict[str, int]] = [{"a": 1}]
    flat = list(chain.from_iterable(rows))
    spread = chain(*rows)
    nested = [item for row in rows for item in row]
    merged = {key: value for table in tables for key, value in table.items()}
    pairs = chain(rows[0], rows[1])
    doubled = [item * 2 for row in rows for item in row]
    kept = [item for row in rows for item in row if item]
""")

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_flattening_reports_on_315(findings: Findings) -> None:
    """`chain.from_iterable`, `chain(*rows)`, and two-level comprehensions yielding the inner item, each reported."""
    assert [error.line for error in findings(SOURCE, ErrorChainFlatteningError)] == [6, 7, 8, 9]


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
def test_unpacked_chain_reports_below_315(findings: Findings) -> None:
    """`chain(*rows)` alone reports where comprehension unpacking does not parse."""
    assert [error.line for error in findings(SOURCE, ErrorChainFlatteningError)] == [7]
