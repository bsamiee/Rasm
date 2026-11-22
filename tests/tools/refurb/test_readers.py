"""RASM106: names one unit reads or none reads, private names read outside their module, and private-module exports no other file reads, counted across every file of the build."""

from textwrap import dedent

import pytest
lazy from refurb.error import Error

from rasm.refurb.readers import ErrorNameReadersError

# --- [CONSTANTS] ------------------------------------------------------------------------

pytestmark = pytest.mark.parametrize("rule", [ErrorNameReadersError], scope="module")

CASES = frozendict(
    one_reader=dedent("""\
        def _scaled(value: float) -> float:
            return value * 2.0

        def area(width: float, height: float) -> float:
            return _scaled(width) * height
    """),
    two_readers=dedent("""\
        def _scaled(value: float) -> float:
            return value * 2.0

        def area(width: float, height: float) -> float:
            return _scaled(width) * height

        def perimeter(width: float, height: float) -> float:
            return _scaled(width + height)
    """),
    deep_reader=dedent("""\
        def _scaled(value: float) -> float:
            return value * 2.0

        def area(rows: list[list[float]], flag: bool) -> float:
            total = 0.0
            for row in rows:
                for value in row:
                    if flag:
                        while total < 1:
                            total += _scaled(value)
            return total
    """),
    unread=dedent("""\
        def _unused(value: float) -> float:
            return value

        def area(width: float, height: float) -> float:
            return width * height
    """),
    _shapes=dedent("""\
        def _shared(value: float) -> float:
            return value

        def lonely(value: float) -> float:
            return value

        def wanted(value: float) -> float:
            return value

        __all__ = ["lonely", "wanted"]
    """),
    public_exports=dedent("""\
        def offered(value: float) -> float:
            return value

        __all__ = ["offered"]
    """),
    shapes_reader=dedent("""\
        from _shapes import _shared, wanted

        def doubled(value: float) -> float:
            return _shared(value) + wanted(value)

        def tripled(value: float) -> float:
            return _shared(value) * 3 + wanted(value)
    """),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "case, found",
    [
        ("one_reader", [(1, "`_scaled` has one reader, move it into `one_reader.area`")]),
        ("two_readers", []),
        ("deep_reader", []),
        ("unread", [(1, "`_unused` has no reader, delete it")]),
        ("_shapes", [(10, "`lonely` has no reader outside its file, remove it from `__all__` and prefix it with `_`")]),
        ("public_exports", []),
        ("shapes_reader", [(1, "`_shared` is read outside `_shapes`, name it without `_` there and list it in its `__all__`")]),
    ],
)
def test_a_name_with_the_wrong_readers_reports(findings: frozendict[str, tuple[Error, ...]], case: str, found: list[tuple[int, str]]) -> None:
    """A function one unit reads within three levels, an unread private function, a private name another module imports, and a private module's export no other file reads report, a function two units read or one read past three levels and a public module's export pass."""
    assert [(error.line, error.msg) for error in findings[case]] == found
