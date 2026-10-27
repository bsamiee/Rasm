"""`Result` error types other than the package fault."""

from textwrap import dedent

from tools.refurb.faults import ErrorResultFaultError
lazy from tests.python.support.doubles import Findings

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_text_and_exception_errors_report(findings: Findings) -> None:
    """`Result` errors typed `str` and `Exception`, both reported."""
    source = dedent("""
        from expression import Result

        type Checked = Result[int, Exception]

        def parsed(text: str) -> Result[int, str]: ...
    """)
    assert findings(ErrorResultFaultError, source) == [
        (4, "`Checked` declares `Exception` as `Result` error type, declare the package fault type"),
        (6, "`parsed` declares `str` as `Result` error type, declare the package fault type"),
    ]


def test_fault_tuple_errors_pass(findings: Findings) -> None:
    """`Result` error typed as a fault tuple, not reported."""
    source = dedent("""
        import msgspec
        from expression import Result

        class Fault(msgspec.Struct, frozen=True, kw_only=True):
            subject: str

        type Faults = tuple[Fault, *tuple[Fault, ...]]
        type Outcome[T] = Result[T, Faults]

        def parsed(text: str) -> Outcome[int]: ...

        def counted(text: str) -> Result[int, Faults]: ...
    """)
    assert findings(ErrorResultFaultError, source) == []
