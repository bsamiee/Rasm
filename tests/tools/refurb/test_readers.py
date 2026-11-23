"""RASM106: names one unit reads or none reads, private names read outside their module, and private-module exports no other file reads, counted across the cross-module reader graph of every file in the build."""

from textwrap import dedent

import pytest

from rasm.refurb.readers import ErrorNameReadersError

# --- [CONSTANTS] ------------------------------------------------------------------------

RULES = (ErrorNameReadersError,)
CASES = frozendict(
    nested_readers=dedent("""\
        def _within(value: float) -> float:  # expect: RASM106 `_within` `nested_readers.summed` "one reader"
            return value * 2.0

        def _beyond(value: float) -> float:
            return value * 3.0

        def _shared(value: float) -> float:
            return value * 4.0

        def _layered(values: list[float]) -> float:
            total = 0.0
            for value in values:
                if value:
                    while total < value:
                        total += 1.0
            return total

        def _twice(value: float) -> float:
            return value + 1.0

        def _unread(value: float) -> float:  # expect: RASM106 `_unread` "no reader, delete"
            return value

        def summed(rows: list[list[float]], flag: bool) -> float:
            total = 0.0
            for row in rows:
                for value in row:
                    if flag:
                        total += _within(value)
            return total + _shared(total)

        def counted(rows: list[list[float]], flag: bool) -> float:
            total = 0.0
            for row in rows:
                for value in row:
                    if flag:
                        while total < 1:
                            total += _beyond(value)
            return total + _shared(total)

        def layered(values: list[float]) -> float:
            return _layered(values) + _twice(_twice(1.0))
    """),
    _shapes=dedent("""\
        def _hidden(value: float) -> float:
            return value

        def lonely(value: float) -> float:
            return value

        def wanted(value: float) -> float:
            return value

        def renamed(value: float) -> float:
            return value

        __all__ = ["lonely", "renamed", "wanted"]  # expect: RASM106 `lonely` "no reader outside its file"
    """),
    shapes_reader=dedent("""\
        from _shapes import _hidden, wanted  # expect: RASM106 `_hidden` `_shapes` "is read outside"

        def doubled(value: float) -> float:
            return _hidden(value) + wanted(value)

        def tripled(value: float) -> float:
            return _hidden(value) * 3 + wanted(value)
    """),
    aliased_reader=dedent("""\
        import _shapes as shapes
        from _shapes import renamed as alias

        def halved(value: float) -> float:
            return shapes._hidden(value) / 2 + alias(value)  # expect: RASM106 `_hidden` `_shapes` "is read outside"

        def quartered(value: float) -> float:
            return shapes.wanted(value) / 4 + alias(value)
    """),
    public_exports=dedent("""\
        def offered(value: float) -> float:
            return value

        __all__ = ["offered"]
    """),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("case", ["nested_readers", "_shapes", "shapes_reader", "aliased_reader", "public_exports"])
def test_a_name_reports_by_the_readers_the_build_graph_gives_it(unmatched: frozendict[str, tuple[tuple[str, ...], tuple[str, ...]]], case: str) -> None:
    """A name reports by the readers the build graph gives it, each message naming its kind.

    A function one unit reads once, its read nesting and body depth summing to three or less, reports with the unit it moves into, and one past that sum, one read twice in its unit, or one two units read passes.
    An unread private function reports for deletion.
    A private name another module imports or reads through a module alias reports there, a private module's export no other file reads reports at its `__all__`, and an export read under an import alias or a public module's export passes.
    """
    assert unmatched[case] == ((), ())
