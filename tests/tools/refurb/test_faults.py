"""RASM101: `Result` error types other than the package fault, and values iterated while they can hold `Nothing` or `Error`."""

from textwrap import dedent

import pytest

from rasm.refurb.faults import ErrorMisusedExpressionError

# --- [CONSTANTS] ------------------------------------------------------------------------

RULES = (ErrorMisusedExpressionError,)
CASES = frozendict(
    error_types=dedent("""\
        from typing import overload
        import msgspec
        from expression import Error, Result

        class Fault(msgspec.Struct, frozen=True, kw_only=True):
            subject: str

        type Faults = tuple[Fault, *tuple[Fault, ...]]
        type Outcome[T] = Result[T, Faults]
        type Nested = Fault | tuple[Nested, ...]
        type Checked = Result[int, Exception]  # expect: RASM101 `Checked` `Exception`
        type Mixed = Result[int, tuple[Fault, str, *tuple[str | BaseException, ...]]]  # expect: RASM101 `Mixed` `str`; RASM101 `Mixed` `BaseException`
        type Generic[E] = Result[int, E]
        type Partial[E] = Result[int, str | E]  # expect: RASM101 `Partial` `str`
        type Fixed = Partial[Exception]  # expect: RASM101 `Fixed` `Exception`
        type Optional = Result[int, str] | None  # expect: RASM101 `Optional` `str`

        def parsed(text: str) -> Outcome[int]:
            return Error((Fault(subject=text),))

        def counted(text: str) -> Result[int, Faults | Nested]:
            return Error((Fault(subject=text),))

        def described(text: str) -> Result[int, str]:  # expect: RASM101 `described` `str`
            return Error(text)

        async def awaited(text: str) -> Generic[str]:  # expect: RASM101 `awaited` `str`
            return Error(text)

        def fixed(text: str) -> Fixed:
            return Error(Exception(text))

        @overload
        def measured(text: str) -> Result[int, Exception]: ...  # expect: RASM101 `measured` `Exception`

        @overload
        def measured(text: bytes) -> Result[int, Exception]: ...  # expect: RASM101 `measured` `Exception`

        def measured(text: str | bytes) -> Result[int, Exception]:  # expect: RASM101 `measured` `Exception`
            return Error(Exception(text))
    """),
    iterated=dedent("""\
        from collections.abc import Callable, Iterable
        from expression import Option, Result, Some

        def traced[**P, R](function: Callable[P, R]) -> Callable[P, R]:
            return function

        @traced
        def total(values: Iterable[int]) -> int:
            return sum(values)

        def read(found: Option[int], outcome: Result[int, int]) -> None:
            for value in found:  # expect: RASM101 `found` `Nothing`
                print(value)
            print(*outcome, tuple(found))  # expect: RASM101 `outcome` `Error`; RASM101 `found` `Nothing`
            print(str(found), repr(outcome))
            print(sum(found))  # expect: RASM101 `found` `Nothing`
            print(total(found))  # expect: RASM101 `found` `Nothing`
            match found:
                case Some() as held:
                    print(*held, *found.to_list())
    """),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("case", ["error_types", "iterated"])
def test_a_misused_result_or_option_reports_where_it_is_declared_or_iterated(unmatched: frozendict[str, tuple[tuple[str, ...], tuple[str, ...]]], case: str) -> None:
    """Text and exception errors report where declared, and an `Option` or `Result` reports where iterated.

    Each rejected type reports once at the alias or function stating it, an alias reporting only the types it adds to the alias it specializes, and a function returning a reported alias, the package fault, or a recursive fault union passes.
    A loop, a star unpacking, and an iterable parameter over an `Option` or `Result` report, and a parameter taking the `Option` itself or a value a pattern narrowed passes.
    """
    assert unmatched[case] == ((), ())
