"""RASM101: `Result` error types other than the package fault, and values iterated while they can hold `Nothing` or `Error`."""

from textwrap import dedent

import pytest
lazy from refurb.error import Error

from rasm.refurb.faults import ErrorMisusedExpressionError

# --- [CONSTANTS] ------------------------------------------------------------------------

pytestmark = pytest.mark.parametrize("rule", [ErrorMisusedExpressionError], scope="module")

CASES = frozendict(
    text_and_exception=dedent("""\
        from expression import Error, Result

        type Checked = Result[int, Exception]

        def parsed(text: str) -> Result[int, str]:
            return Error(text)

        def optional(text: str) -> Result[int, str] | None:
            return Error(text) if text else None
    """),
    fault_tuple=dedent("""\
        import msgspec
        from expression import Error, Result

        class Fault(msgspec.Struct, frozen=True, kw_only=True):
            subject: str

        type Faults = tuple[Fault, *tuple[Fault, ...]]
        type Outcome[T] = Result[T, Faults]

        def parsed(text: str) -> Outcome[int]:
            return Error((Fault(subject=text),))

        def counted(text: str) -> Result[int, Faults]:
            return Error((Fault(subject=text),))
    """),
    coroutines_and_overloads=dedent("""\
        from typing import overload
        from expression import Error, Result

        async def parsed(text: str) -> Result[int, str]:
            return Error(text)

        @overload
        def counted(text: str) -> Result[int, Exception]: ...

        @overload
        def counted(text: bytes) -> Result[int, Exception]: ...

        def counted(text: str | bytes) -> Result[int, Exception]:
            return Error(Exception(text))
    """),
    recursive_faults_and_return_aliases=dedent("""\
        import msgspec
        from expression import Error, Result

        class Fault(msgspec.Struct, frozen=True):
            subject: str

        type Faults = Fault | tuple[Faults, ...]
        type Outcome = Result[int, Faults]
        type Invalid = Result[int, str] | None

        async def parsed(text: str) -> Invalid:
            return Error(text)

        def counted(text: str) -> Invalid:
            return Error(text)

        def opaque(text: str) -> dict[str, Result[int, str]]:
            return {text: Error(text)}
    """),
    generic_aliases=dedent("""\
        from expression import Error, Result

        type Generic[E] = Result[int, E]
        type Chain[E] = Generic[E]
        type Mixed[E] = Result[int, str | E]
        type Fixed = Mixed[Exception]
        type Closed[T] = Result[T, str]
        type Maybe[E] = Result[int, E] | None

        def direct() -> Generic[str]:
            return Error('invalid')

        async def chained() -> Chain[Exception]:
            return Error(Exception())

        def fixed() -> Fixed:
            return Error(Exception())

        def closed() -> Closed[int]:
            return Error('invalid')

        def optional(text: str) -> Maybe[str]:
            return Error(text) if text else None
    """),
    tuple_tail_and_union=dedent("""\
        import msgspec
        from expression import Result

        class Fault(msgspec.Struct, frozen=True):
            subject: str

        type Checked = Result[int, tuple[Fault, str, *tuple[str | BaseException, ...]]]
    """),
    iterated_values=dedent("""\
        from expression import Ok, Option, Result, Some

        def read(found: Option[int], outcome: Result[int, str]) -> None:
            for value in found:
                print(value)
            print(*outcome, tuple(found), dict(zip(("value",), found)))
            match found:
                case Some() as held:
                    print(*held, *Ok(1), *found.to_list())
    """),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "case, lines",
    [("text_and_exception", [3, 5, 8]), ("fault_tuple", []), ("coroutines_and_overloads", [4, 8, 11, 13]), ("recursive_faults_and_return_aliases", [9]), ("generic_aliases", [5, 6, 7, 10, 13, 22])],
)
def test_a_result_error_other_than_the_package_fault_reports(findings: frozendict[str, tuple[Error, ...]], case: str, lines: list[int]) -> None:
    """Text and exception errors report at the alias or function declaring them, through coroutines, overloads, unions, and generic specializations, and fault errors pass."""
    assert [error.line for error in findings[case]] == lines


def test_a_rejected_error_type_reports_once_per_type(findings: frozendict[str, tuple[Error, ...]]) -> None:
    """Tuple tails and union members report each rejected type once, naming it."""
    assert [(error.line, error.msg) for error in findings["tuple_tail_and_union"]] == [
        (7, "`Checked` declares `str` as `Result` error type, declare the package fault type"),
        (7, "`Checked` declares `BaseException` as `Result` error type, declare the package fault type"),
    ]


def test_an_operand_iterated_while_it_can_hold_no_value_reports(findings: frozendict[str, tuple[Error, ...]]) -> None:
    """A loop, star unpacking, and iterable parameters reading an `Option` or `Result` report, a narrowed case and its members pass."""
    assert [(error.line, error.msg) for error in findings["iterated_values"]] == [
        (4, "`found` can hold `Nothing`, whose iteration raises `EffectError`, read the value through its members or a pattern"),
        (6, "`outcome` can hold `Error`, whose iteration raises `EffectError`, read the value through its members or a pattern"),
        (6, "`found` can hold `Nothing`, whose iteration raises `EffectError`, read the value through its members or a pattern"),
        (6, "`found` can hold `Nothing`, whose iteration raises `EffectError`, read the value through its members or a pattern"),
    ]
