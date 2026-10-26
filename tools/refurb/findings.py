"""Finding base every Rasm check derives and the mypy fullname of a runtime declaration."""

import re
from typing import override, Protocol

from refurb.error import Error

# --- [TYPES] ----------------------------------------------------------------------------


class Declared(Protocol):
    """Runtime class or function a check names by its mypy fullname."""

    __module__: str
    __name__: str
    __qualname__: str


# --- [ERRORS] ---------------------------------------------------------------------------


class RasmError(Error):
    """Finding a Rasm check reports under the `RASM` prefix, named by the words its class name spells between the `Error` prefix and suffix."""

    prefix = "RASM"

    @override
    def __init_subclass__(cls) -> None:
        super().__init_subclass__()
        cls.name = "-".join(word.lower() for word in re.findall(r"[A-Z]+(?![a-z])|[A-Z][a-z\d]*", cls.__name__.removeprefix("Error").removesuffix("Error")))


# --- [OPERATIONS] -----------------------------------------------------------------------


def fullname(declared: Declared) -> str:
    """Returns the fullname mypy gives the runtime class or function."""
    return f"{declared.__module__}.{declared.__qualname__}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("Declared", "RasmError", "fullname")
