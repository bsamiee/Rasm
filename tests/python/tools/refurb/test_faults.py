"""`Result` error types other than the package fault."""

from textwrap import dedent

from tools.refurb.faults import ErrorResultFaultError
lazy from tests.python.tools.refurb.conftest import Findings

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_text_and_exception_errors_report(findings: Findings) -> None:
    """`Result` errors typed `str` and `Exception`, both reported."""
    source = dedent("""
        from expression import Error, Result

        type Checked = Result[int, Exception]

        def parsed(text: str) -> Result[int, str]:
            return Error(text)

        def optional(text: str) -> Result[int, str] | None:
            return Error(text) if text else None
    """)
    assert [error.line for error in findings(source, ErrorResultFaultError)] == [4, 6, 9]


def test_fault_tuple_errors_pass(findings: Findings) -> None:
    """`Result` error typed as a fault tuple, not reported."""
    source = dedent("""
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
    """)
    assert findings(source, ErrorResultFaultError) == ()


def test_async_and_overload_errors_report(findings: Findings) -> None:
    """Async annotations and overload declarations report their error type."""
    source = dedent("""
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
    """)
    assert [error.line for error in findings(source, ErrorResultFaultError)] == [5, 9, 12, 14]


def test_tuple_tail_and_union_errors_report(findings: Findings) -> None:
    """Tuple tails and union members report each rejected type once."""
    source = dedent("""
        import msgspec
        from expression import Result

        class Fault(msgspec.Struct, frozen=True):
            subject: str

        type Checked = Result[int, tuple[Fault, str, *tuple[str | BaseException, ...]]]
    """)
    assert [(error.line, error.msg) for error in findings(source, ErrorResultFaultError)] == [
        (8, f"`Checked` declares `{kind.__name__}` as `Result` error type, declare the package fault type") for kind in (str, BaseException)
    ]


def test_recursive_faults_and_return_aliases_pass(findings: Findings) -> None:
    """Recursive typed faults terminate, and return aliases leave one declaration owner."""
    source = dedent("""
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
    """)
    assert [error.line for error in findings(source, ErrorResultFaultError)] == [10]


def test_generic_aliases_report_at_the_specialization_owner(findings: Findings) -> None:
    """Generic errors resolve at uses while closed aliases and partial specializations retain one declaration owner."""
    source = dedent("""
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
    """)
    assert [error.line for error in findings(source, ErrorResultFaultError)] == [6, 7, 8, 11, 14, 23]
