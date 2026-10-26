"""Blender refusal reasons."""

from enum import StrEnum

# --- [TYPES] ----------------------------------------------------------------------------


class BlenderReason(StrEnum):
    """Registration, edit, lookup, context, and operator refusals, each value the report template of its fault."""

    UNREGISTERED = "Registration of {subject} refused. {cause}"
    REFUSED = "{subject} refused edit. {cause}"
    ABSENT = "{subject} holds no {member}"
    TAKEN = "{subject} is taken at {member}"
    UNMET = "{subject}"
    ANSWERED = "{subject} answered {member}"
    FAILED = "{subject} failed. {cause}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BlenderReason"]
