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
RULES = (ErrorLostLogFieldError,)
CASES = frozendict(
    direct=LOGGER
    + dedent("""\

        def sent(issued: int) -> None:
            logger.info(f"Queue sent {issued}")  # expect: RASM103 `info` "formats"
            logger.info("Queue sent %s" % issued)  # expect: RASM103 `info` "formats"
            logger.info("Queue sent %s", issued)  # expect: RASM103 `info` "positional"
            logger.bind(issued=issued)  # expect: RASM103 `bind` "discards"
            logger.info("Queue sent", issued=issued)
            logger.bind(issued=issued).info("Queue sent")
    """),
    unpacked=LOGGER
    + dedent("""\

        def sent(issued: int, pair: tuple[str, int], values: tuple[str, ...], required: tuple[str, *tuple[str, ...]]) -> None:
            logger.info(*[f"Queue sent {issued}", *()])  # expect: RASM103 `info` "formats"
            logger.info(*("Queue sent", *()))
            logger.log(*(20, f"Queue sent {issued}", issued))  # expect: RASM103 `log` "formats"; RASM103 `log` "positional"
            logger.info(*pair)  # expect: RASM103 `info` "positional"
            logger.info(*values, f"Queue sent {issued}")
            logger.info(*required, f"Queue sent {issued}")  # expect: RASM103 `info` "positional"
    """),
    aliased=dedent("""\
        import structlog
        from structlog.typing import FilteringBoundLogger
        from typing import Final

        logger: FilteringBoundLogger = structlog.get_logger()
        emit: Final = logger.info
        log: Final = logger.log
        bind: Final = logger.bind
        factory: Final = structlog.get_logger

        def sent(issued: int) -> None:
            emit(f"Queue sent {issued}")  # expect: RASM103 `info` "formats"
            log(20, "Queue sent %s", issued)  # expect: RASM103 `log` "positional"
            log(level=20, event=f"Queue sent {issued}")  # expect: RASM103 `log` "formats"
            log(20, event="Queue sent", issued=issued)
            bind(issued=issued)  # expect: RASM103 `bind` "discards"
            factory().bind(issued=issued).info(f"Queue sent {issued}")  # expect: RASM103 `info` "formats"
    """),
    rebound=LOGGER
    + dedent("""\

        class Output:
            def info(self, event: str, *args: object) -> None:
                print(event, args)

        def emitted(issued: int) -> None:
            emit = logger.info
            emit = Output().info
            emit(f"Queue sent {issued}")
            Output().info("Queue sent %s", issued)
    """),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("case", ["direct", "unpacked", "aliased", "rebound"])
def test_a_log_field_lost_to_event_text_a_positional_value_or_a_discarded_binding_reports(unmatched: frozendict[str, tuple[tuple[str, ...], tuple[str, ...]]], case: str) -> None:
    """f-string and `%` events report as formatted text, values past the event as positional arguments, and a discarded `bind` as discarded.

    Literal and typed spreads, nested spreads, and final aliases, `log` reading its event after the level; keyword fields, a used binding, a value after a spread of unknown length, a name rebound off structlog, and another logger pass.
    """
    assert unmatched[case] == ((), ())


def test_a_logger_bound_past_the_recursion_limit_reports_its_formatted_event() -> None:
    """A logger bound twice Python's recursion limit deep is still recognized, and its formatted event reports as formatted text."""
    factory = NameExpr(structlog.get_logger.__name__)
    factory.fullname = fullname(structlog.get_logger)
    receiver = CallExpr(factory, [], [], [])
    for _ in range(getrecursionlimit() * 2):
        receiver = CallExpr(MemberExpr(receiver, "bind"), [], [], [])
    errors: list[Error] = []
    check(CallExpr(MemberExpr(receiver, "info"), [OpExpr("+", StrExpr("Queue sent "), NameExpr("issued"))], [ArgKind.ARG_POS], []), errors)
    assert [(type(error), error.msg.split("`")[1], "formats" in error.msg) for error in errors] == [(ErrorLostLogFieldError, "info", True)]
