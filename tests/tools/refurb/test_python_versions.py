"""RASM107, RASM108, RASM109: rules whose finding depends on the Python version refurb types against, over one build per version on each side of every gate."""

import ast
from itertools import chain
from textwrap import dedent

from hypothesis import example, given, strategies
import pytest
lazy from refurb.error import Error

from rasm.refurb.flattening import ErrorChainFlatteningError
from rasm.refurb.frozen_mappings import ErrorLiteralMappingProxyError
from rasm.refurb.subscripts import ErrorUnsubscriptableBuiltinError

# --- [CONSTANTS] ------------------------------------------------------------------------

pytestmark = pytest.mark.parametrize("version", ["3.13", "3.14", "3.15"], scope="module")

RULES = (ErrorChainFlatteningError, ErrorLiteralMappingProxyError, ErrorUnsubscriptableBuiltinError)
CASES = frozendict(
    flattened=dedent("""\
        from itertools import chain

        def flat(rows: list[dict[str, int]]) -> tuple[object, ...]:
            return (
                chain.from_iterable(rows),  # expect: 3.15 RASM107 `[*row for row in rows]`
                chain(*rows),  # expect: 3.13 RASM107 `chain.from_iterable(<rows>)`; 3.14 RASM107 `chain.from_iterable(<rows>)`; 3.15 RASM107 `[*row for row in rows]`
                [item for row in rows for item in row],  # expect: 3.15 RASM107 `[*row for row in rows]`
                {key: value for row in rows for key, value in row.items()},  # expect: 3.15 RASM107 `{**row for row in rows}`
            )
    """),
    reshaped=dedent("""\
        from itertools import chain

        def reshaped(rows: list[dict[str, int]], other: list[int]) -> tuple[object, ...]:
            return (
                chain(rows[0], rows[1]),
                [item * 2 for row in rows for item in row],
                [item for row in rows for item in row if item],
                [item for row in rows for item in other],
                [*row for row in rows],
                {**row for row in rows},
            )
    """),
    proxied=dedent("""\
        from types import MappingProxyType

        def tables(held: dict[str, int]) -> tuple[MappingProxyType[str, int], ...]:
            return (
                MappingProxyType({"a": 1}),  # expect: 3.15 RASM108 `frozendict(<mapping>)`
                MappingProxyType({key: len(key) for key in held}),  # expect: 3.15 RASM108 `frozendict(<mapping>)`
                MappingProxyType(held),
            )
    """),
    subscripted=dedent("""\
        import ctypes
        import sqlite3
        import types
        import typing

        View = memoryview[int]  # expect: 3.13 RASM109 `builtins.memoryview`
        Part = slice[int, int, int]  # expect: 3.13 RASM109 `builtins.slice`
        Kinds = types.UnionType[int]  # expect: 3.13 RASM109 `types.UnionType`
        Packed = sqlite3.Binary[int]  # expect: 3.13 RASM109 `builtins.memoryview`
        Choice = typing.Union[int, str]

        class Holder(ctypes.py_object[int]): ...  # expect: 3.13 RASM109 `ctypes.py_object`

        type Lazy = memoryview[int]
    """),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("case", ["flattened", "reshaped", "proxied", "subscripted"])
def test_a_version_gated_finding_reports_on_its_side_of_the_gate_alone(unmatched: frozendict[str, tuple[tuple[str, ...], tuple[str, ...]]], case: str) -> None:
    """A version-gated finding reports on its side of each gate alone.

    `chain(*rows)` reports on every version, `chain.from_iterable`, two-level comprehensions, and literal mapping proxies once 3.15 parses comprehension unpacking and holds `frozendict`, subscripts of classes lacking `__class_getitem__` before 3.14.
    A listed chain, a changed, filtered, or foreign item, an unpacking comprehension, a held mapping, `typing.Union`, or a lazy `type` alias never report.
    """
    assert unmatched[case] == ((), ())


@given(rows=strategies.lists(strategies.dictionaries(strategies.sampled_from("abc"), strategies.integers(), max_size=3), max_size=5))
@example(rows=[])
@example(rows=[{}, {"a": 0}])
@example(rows=[{"a": 0, "b": 1}, {"a": 2}])
def test_a_proposed_flattening_yields_what_the_expression_it_replaces_yields(findings: frozendict[str, tuple[Error, ...]], rows: list[dict[str, int]]) -> None:
    """Over drawn rows whose keys overlap, each proposed rewrite builds a list equal to a flagged list, a dict equal in items and key order to a flagged dict, and the items a flagged lazy chain yields, a proposal for a lazy chain being eager."""
    expressions = {case: {(node.lineno, node.col_offset): ast.unparse(node) for node in reversed(tuple(ast.walk(ast.parse(source)))) if isinstance(node, ast.expr)} for case, source in CASES.items()}
    flagged = tuple((expressions[case][error.line, error.column], error.msg) for case, errors in findings.items() for error in errors if isinstance(error, ErrorChainFlatteningError))
    assert flagged, "no flattening reports to compare"
    for expression, message in flagged:
        before = eval(expression, {"chain": chain, "rows": rows})
        after = eval(message.split("`")[1].replace("<rows>", "rows"), {"chain": chain, "rows": rows})
        match before:
            case dict() | list():
                assert (type(after), after, [*after]) == (type(before), before, [*before]), f"{expression} against {message}"
            case _:
                assert [*after] == [*before], f"{expression} against {message}"
