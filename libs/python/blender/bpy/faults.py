"""Blender refusal reasons."""

from enum import StrEnum

# --- [TYPES] ----------------------------------------------------------------------------


class BlenderReason(StrEnum):
    """Registration, edit, lookup, context requirement, and operator refusals, each value the report template of its fault."""

    UNREGISTERED = "Registration of {subject} refused. {cause}"
    REFUSED = "{subject} refused edit. {cause}"
    ABSENT = "{subject} holds no {member}"
    TAKEN = "{subject} is taken at {member}"
    REQUIRED = "Requires {subject}"
    UNSEEN = "Requires {subject} in front of scene camera"
    UNDATED = "Requires template of {subject} without {{date}}, Blender cannot expand it"
    UNROOTED = "Requires saved file or absolute output folder, {subject} resolves against working directory"
    UNVARIED = "Requires {{{subject}}} in output template to name one file per {subject}"
    UNCHOSEN = "{subject} requires a chosen file"
    ANSWERED = "{subject} answered {member}"
    FAILED = "{subject} failed. {cause}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BlenderReason"]
