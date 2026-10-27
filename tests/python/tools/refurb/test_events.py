"""Structlog values lost to event text, positional arguments, or a discarded logger."""

from textwrap import dedent

from tools.refurb.events import ErrorLostLogFieldError
lazy from tests.python.support.doubles import Findings

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
    assert findings(ErrorLostLogFieldError, source) == [
        (8, "`logger.info` formats values into event text, pass each value as a keyword argument"),
        (9, "`logger.info` takes positional event arguments, pass each value as a keyword argument"),
        (10, "Statement discards the bound logger `logger.bind` returns, assign it and log through it"),
    ]


def test_keyword_fields_pass(findings: Findings) -> None:
    """Keyword fields and a logger bound then used, not reported."""
    source = dedent("""
        import structlog
        from structlog.typing import FilteringBoundLogger

        logger: FilteringBoundLogger = structlog.get_logger()

        def sent(issued: int) -> None:
            logger.info("Queue sent", issued=issued)
            logger.bind(issued=issued).info("Queue sent")
    """)
    assert findings(ErrorLostLogFieldError, source) == []
