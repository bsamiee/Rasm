"""Rendering refusal reasons."""

from enum import StrEnum

# --- [TYPES] ----------------------------------------------------------------------------


class RenderingReason(StrEnum):
    """Exchange and process refusals, each value the report template of its fault."""

    STATUS = "{subject} answered HTTP {member}"
    EXCHANGE = "Exchange with {subject} failed. {cause}"
    PREREQUISITE = "{subject} cannot enforce prerequisites for queue parts {member}"
    EXITED = "{subject} exited with code {member}"
    UNSTARTED = "{subject} failed to start. {cause}"
    SYSTEM = "System call {subject} failed. {cause}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RenderingReason"]
