"""Structlog values lost to event text, positional arguments, or a discarded logger."""

from textwrap import dedent
lazy from sys import getrecursionlimit

lazy from mypy.nodes import ArgKind, CallExpr, MemberExpr, NameExpr, OpExpr, StrExpr
lazy from refurb.error import Error
lazy import structlog

from tools.refurb.events import check, ErrorLostLogFieldError
lazy from tests.python.tools.refurb.conftest import Findings
lazy from tools.refurb.findings import fullname

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_formatted_positional_and_discarded_fields_report(findings: Findings) -> None:
    """Formatted event text, positional arguments, and a discarded bound logger, each reported."""
    source = dedent("""
        import structlog
        from structlog.typing import FilteringBoundLogger

        logger: FilteringBoundLogger = structlog.get_logger()

        def sent(issued: int) -> None:
            logger.info(f"Queue sent {issued}")
            logger.info("Queue sent %s", issued)
            logger.bind(issued=issued)
    """)
    assert [error.line for error in findings(ErrorLostLogFieldError, source)] == [8, 9, 10]


def test_keyword_fields_pass(findings: Findings) -> None:
    """Keyword fields and a logger bound then used, not reported."""
    source = dedent("""
        import structlog
        from structlog.typing import FilteringBoundLogger

        logger: FilteringBoundLogger = structlog.get_logger()

        def sent(issued: int, arguments: tuple[str, *tuple[int, ...]]) -> None:
            logger.info("Queue sent", issued=issued)
            logger.bind(issued=issued).info("Queue sent")
            logger.info(*arguments)
            logger.info(*("Queue sent", *()))
    """)
    assert findings(ErrorLostLogFieldError, source) == ()


def test_unpacked_events_and_fields_report(findings: Findings) -> None:
    """Literal unpacking and fixed tuple parameters retain event positions and field counts."""
    source = dedent("""
        import structlog
        from structlog.typing import FilteringBoundLogger

        logger: FilteringBoundLogger = structlog.get_logger()

        def sent(issued: int, arguments: tuple[str, int]) -> None:
            logger.info(*(f"Queue sent {issued}",))
            logger.info(*("Queue sent %s", issued))
            logger.log(*(20, f"Queue sent {issued}", issued))
            logger.info(*arguments)
            logger.info("Queue sent", *())
            logger.info(*[f"Queue sent {issued}", *()])
    """)
    assert [error.line for error in findings(ErrorLostLogFieldError, source)] == [8, 9, 10, 10, 11, 13]


def test_method_aliases_and_named_level_report(findings: Findings) -> None:
    """Bound method aliases keep native argument names and named levels leave event positions unchanged."""
    source = dedent("""
        import structlog
        from structlog.typing import FilteringBoundLogger
        from typing import Final

        logger: FilteringBoundLogger = structlog.get_logger()
        emit: Final = logger.info
        log: Final = logger.log
        bind: Final = logger.bind
        factory: Final = structlog.get_logger

        def sent(issued: int) -> None:
            emit(f"Queue sent {issued}")
            log(20, "Queue sent %s", issued)
            logger.log(level=20, event=f"Queue sent {issued}")
            logger.log(20, event="Queue sent", issued=issued)
            bind(issued=issued)
            factory().bind(issued=issued).info(f"Queue sent {issued}")
    """)
    assert [error.line for error in findings(ErrorLostLogFieldError, source)] == [13, 14, 15, 17, 18]


def test_reassigned_callable_aliases_keep_their_active_contract(findings: Findings) -> None:
    """Mutable alias declarations retain no earlier logger method or factory identity."""
    source = dedent("""
        import structlog
        from structlog.typing import FilteringBoundLogger

        logger: FilteringBoundLogger = structlog.get_logger()

        class Output:
            def info(self, event: str, *args: object, **kw: object) -> None:
                pass

            def bind(self, **values: object) -> Output:
                return self

        def captured(**new_values: object) -> FilteringBoundLogger:
            print(new_values)
            return logger

        def plain_factory(*args: object, **initial_values: object) -> Output:
            return Output()

        def emitted(issued: int) -> None:
            emit = logger.info
            emit = Output().info
            emit(f"Queue sent {issued}")
            bind = logger.bind
            bind = captured
            bind(issued=issued)
            factory = structlog.get_logger
            factory = plain_factory
            factory().bind(issued=issued).info(f"Queue sent {issued}")
    """)
    assert findings(ErrorLostLogFieldError, source) == ()


def test_unknown_unpacking_preserves_argument_bounds(findings: Findings) -> None:
    """Unknown unpacking leaves event positions unresolved and keeps definite positional field counts."""
    source = dedent("""
        import structlog
        from structlog.typing import FilteringBoundLogger

        logger: FilteringBoundLogger = structlog.get_logger()

        def sent(values: tuple[str, ...], required: tuple[str, *tuple[str, ...]], issued: int) -> None:
            logger.info(*values, f"Queue sent {issued}")
            logger.info(*values, "Queue sent %s", issued)
            logger.info(event=f"Queue sent {issued}")
            logger.info(*required, f"Queue sent {issued}")
    """)
    assert [error.line for error in findings(ErrorLostLogFieldError, source)] == [9, 10, 11]


def test_other_log_methods_pass(findings: Findings) -> None:
    """Unrelated event methods and aliases retain their own contracts."""
    source = dedent("""
        class Output:
            def info(self, event: str, *values: object) -> None:
                pass

        logger = Output()
        emit = logger.info
        logger.info("Queue sent %s", 4)
        emit("Queue sent %s", 4)
    """)
    assert findings(ErrorLostLogFieldError, source) == ()


def test_long_logger_chain_reports() -> None:
    """Logger recognition retains the actual formatted-event finding beyond Python's recursion limit."""
    factory = NameExpr(structlog.get_logger.__name__)
    factory.fullname = fullname(structlog.get_logger)
    receiver = CallExpr(factory, [], [], [])
    for _ in range(getrecursionlimit() * 2):
        receiver = CallExpr(MemberExpr(receiver, "bind"), [], [], [])
    errors: list[Error] = []
    check(CallExpr(MemberExpr(receiver, "info"), [OpExpr("+", StrExpr("Queue sent "), NameExpr("issued"))], [ArgKind.ARG_POS], []), errors)
    assert list(map(type, errors)) == [ErrorLostLogFieldError]
