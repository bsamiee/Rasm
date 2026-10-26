"""Rendering refusal reasons."""

from enum import StrEnum

# --- [TYPES] ----------------------------------------------------------------------------


class RenderingReason(StrEnum):
    """Name, path, exchange, process, and calendar refusals, each value the report template of its fault."""

    UNMATCHED = "{subject} does not match {member}"
    TEMPLATE = "{subject} is no output name template. {cause}"
    ROOT = "{subject} lies outside {member}"
    STATUS = "{subject} answered HTTP {member}"
    EXCHANGE = "{subject} failed. {cause}"
    PREREQUISITE = "{subject} cannot enforce prerequisites for queue parts {member}"
    EXITED = "{subject} exited with code {member}"
    UNSTARTED = "{subject} failed to start. {cause}"
    SYSTEM = "System call {subject} failed. {cause}"
    CALENDAR = "Calendar calculation for {subject} failed. {cause}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RenderingReason"]
