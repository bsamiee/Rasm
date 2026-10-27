"""Partial case reads of `Result` and `Option` outside a `match` on the receiver."""

from textwrap import dedent

from tools.refurb.cases import ErrorPartialCaseReadError
lazy from tests.python.support.doubles import Findings

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_case_read_outside_match_reports(findings: Findings) -> None:
    """Case reads outside a `match`, reported with the matching arm."""
    source = dedent("""
        from expression import Option, Result

        def record(row: Option[int], parsed: Result[int, ValueError]) -> int:
            return row.value if row.is_some() else parsed.ok
    """)
    assert findings(ErrorPartialCaseReadError, source) == [
        (5, '`row.value` raises outside the `some` case, match `row` with `case Option(tag="some", some=<name>)` or call `map`, `bind`, or `default_value`'),
        (5, '`parsed.ok` raises outside the `ok` case, match `parsed` with `case Result(tag="ok", ok=<name>)` or call `map`, `bind`, or `default_value`'),
    ]


def test_case_read_in_match_and_chain_passes(findings: Findings) -> None:
    """Case read inside its `match` arm and a `map` chain, not reported."""
    source = dedent("""
        from expression import Option, Result

        def record(row: Option[int], parsed: Result[int, ValueError]) -> int:
            match parsed:
                case Result(tag="ok"):
                    return parsed.ok
                case _:
                    return row.map(lambda value: value + 1).default_value(0)
    """)
    assert findings(ErrorPartialCaseReadError, source) == []
