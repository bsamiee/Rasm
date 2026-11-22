"""RASM103: structlog values lost to event text, positional arguments, or a discarded bound logger."""

from sys import getrecursionlimit
from textwrap import dedent

from mypy.nodes import ArgKind, CallExpr, MemberExpr, NameExpr, OpExpr, StrExpr
import pytest
import structlog
lazy from refurb.error import Error

from rasm.refurb import fullname
from rasm.refurb.events import check, ErrorLostLogFieldError

# --- [CONSTANTS] ------------------------------------------------------------------------

LOGGER = dedent("""\
    import structlog
    from structlog.typing import FilteringBoundLogger

    logger: FilteringBoundLogger = structlog.get_logger()
""")
CASES = frozendict(
    formatted_positional_and_discarded=LOGGER
    + dedent("""\

        def sent(issued: int) -> None:
            logger.info(f"Queue sent {issued}")
            logger.info("Queue sent %s", issued)
            logger.info("Queue sent " + str(issued))
            logger.bind(issued=issued)
    """),
    keyword_fields=LOGGER
    + dedent("""\

        def sent(issued: int, arguments: tuple[str, *tuple[int, ...]]) -> None:
            logger.info("Queue sent", issued=issued)
            logger.bind(issued=issued).info("Queue sent")
            logger.info(*arguments)
            logger.info(*("Queue sent", *()))
    """),
    unpacked_events_and_fields=LOGGER
    + dedent("""\

        def sent(issued: int, arguments: tuple[str, int]) -> None:
            logger.info(*(f"Queue sent {issued}",))
            logger.info(*("Queue sent %s", issued))
            logger.log(*(20, f"Queue sent {issued}", issued))
            logger.info(*arguments)
            logger.info("Queue sent", *())
            logger.info(*[f"Queue sent {issued}", *()])
    """),
    final_aliases_and_named_level=dedent("""\
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
    """),
    reassigned_aliases=LOGGER
    + dedent("""\

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
    """),
    unknown_unpacking=LOGGER
    + dedent("""\

        def sent(values: tuple[str, ...], required: tuple[str, *tuple[str, ...]], issued: int) -> None:
            logger.info(*values, f"Queue sent {issued}")
            logger.info(*values, "Queue sent %s", issued)
            logger.info(event=f"Queue sent {issued}")
            logger.info(*required, f"Queue sent {issued}")
    """),
    other_logger=dedent("""\
        class Output:
            def info(self, event: str, *values: object) -> None:
                pass

        logger = Output()
        emit = logger.info
        logger.info("Queue sent %s", 4)
        emit("Queue sent %s", 4)
    """),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("rule", [ErrorLostLogFieldError], scope="module")
@pytest.mark.parametrize(
    "case, lines",
    [
        ("formatted_positional_and_discarded", [7, 8, 9, 10]),
        ("keyword_fields", []),
        ("unpacked_events_and_fields", [7, 8, 9, 9, 10, 12]),
        ("final_aliases_and_named_level", [12, 13, 14, 16, 17]),
        ("reassigned_aliases", []),
        ("unknown_unpacking", [8, 9, 10]),
        ("other_logger", []),
    ],
)
def test_a_lost_log_field_reports(findings: frozendict[str, tuple[Error, ...]], case: str, lines: list[int]) -> None:
    """Formatted events, positional values, and discarded bound loggers report through unpacking and final aliases, keyword fields, rebound aliases, and other loggers pass."""
    assert [error.line for error in findings[case]] == lines


def test_a_logger_bound_past_the_recursion_limit_reports_its_formatted_event() -> None:
    """A logger bound twice Python's recursion limit deep is still recognized, and its formatted event reports."""
    factory = NameExpr(structlog.get_logger.__name__)
    factory.fullname = fullname(structlog.get_logger)
    receiver = CallExpr(factory, [], [], [])
    for _ in range(getrecursionlimit() * 2):
        receiver = CallExpr(MemberExpr(receiver, "bind"), [], [], [])
    errors: list[Error] = []
    check(CallExpr(MemberExpr(receiver, "info"), [OpExpr("+", StrExpr("Queue sent "), NameExpr("issued"))], [ArgKind.ARG_POS], []), errors)
    assert list(map(type, errors)) == [ErrorLostLogFieldError]
