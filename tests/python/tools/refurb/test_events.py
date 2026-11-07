"""Structlog values lost to event text, positional arguments, or a discarded logger."""

from pathlib import Path
from sys import getrecursionlimit
from textwrap import dedent

import pytest
lazy from mypy.nodes import ArgKind, CallExpr, MemberExpr, NameExpr, OpExpr, StrExpr
lazy from refurb.error import Error
lazy import structlog

from rasm_refurb.events import check, ErrorLostLogFieldError
lazy from rasm_refurb import Findings, fullname

# --- [CONSTANTS] ------------------------------------------------------------------------

LOGGER = dedent("""
    import structlog
    from structlog.typing import FilteringBoundLogger

    logger: FilteringBoundLogger = structlog.get_logger()
""")
CASES = frozendict(
    formatted_positional_and_discarded_fields_report=(
        LOGGER
        + dedent("""
            def sent(issued: int) -> None:
                logger.info(f"Queue sent {issued}")
                logger.info("Queue sent %s", issued)
                logger.bind(issued=issued)
        """),
        (8, 9, 10),
    ),
    keyword_fields_pass=(
        LOGGER
        + dedent("""
            def sent(issued: int, arguments: tuple[str, *tuple[int, ...]]) -> None:
                logger.info("Queue sent", issued=issued)
                logger.bind(issued=issued).info("Queue sent")
                logger.info(*arguments)
                logger.info(*("Queue sent", *()))
        """),
        (),
    ),
    unpacked_events_and_fields_report=(
        LOGGER
        + dedent("""
            def sent(issued: int, arguments: tuple[str, int]) -> None:
                logger.info(*(f"Queue sent {issued}",))
                logger.info(*("Queue sent %s", issued))
                logger.log(*(20, f"Queue sent {issued}", issued))
                logger.info(*arguments)
                logger.info("Queue sent", *())
                logger.info(*[f"Queue sent {issued}", *()])
        """),
        (8, 9, 10, 10, 11, 13),
    ),
    method_aliases_and_named_level_report=(
        dedent("""
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
        (13, 14, 15, 17, 18),
    ),
    reassigned_callable_aliases_keep_their_active_contract=(
        LOGGER
        + dedent("""
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
        (),
    ),
    unknown_unpacking_preserves_argument_bounds=(
        LOGGER
        + dedent("""
            def sent(values: tuple[str, ...], required: tuple[str, *tuple[str, ...]], issued: int) -> None:
                logger.info(*values, f"Queue sent {issued}")
                logger.info(*values, "Queue sent %s", issued)
                logger.info(event=f"Queue sent {issued}")
                logger.info(*required, f"Queue sent {issued}")
        """),
        (9, 10, 11),
    ),
    other_log_methods_pass=(
        dedent("""
            class Output:
                def info(self, event: str, *values: object) -> None:
                    pass

            logger = Output()
            emit = logger.info
            logger.info("Queue sent %s", 4)
            emit("Queue sent %s", 4)
        """),
        (),
    ),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("case", tuple(CASES))
def test_logging_reports_file_local_diagnostics(case: str, diagnostics: dict[str, list[int]]) -> None:
    """Reports lost fields and keeps valid calls outside each case's exact findings."""
    assert tuple(diagnostics[case]) == CASES[case][1]


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


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="module")
def diagnostics(findings: Findings) -> dict[str, list[int]]:
    """Compiles distinct logging examples together and indexes diagnostics by source module."""
    lines: dict[str, list[int]] = {case: [] for case in CASES}
    for error in findings({case: source for case, (source, _) in CASES.items()}, ErrorLostLogFieldError):
        assert error.filename is not None
        lines[Path(error.filename).stem].append(error.line)
    return lines
