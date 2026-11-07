"""Finding base every Rasm check derives, the build facts checks read beyond their own tree, and the mypy fullname of a runtime declaration."""

from typing import override, Protocol
lazy from collections.abc import Mapping
lazy import re

from refurb.error import Error
lazy from mypy.nodes import Expression, Statement, TypeInfo
lazy from mypy.types import Type

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
        cls.name = "-".join(word.lower() for word in re.findall(r"[A-Z\d]+(?![a-z])|[A-Z\d][a-z\d]*", cls.__name__.removeprefix("Error").removesuffix("Error")))


# --- [SERVICES] -------------------------------------------------------------------------


class Reads:
    """Expression types of one build, its source modules, and each source module whose class body a check reads, gathered until `taken`."""

    def __init__(self, types: Mapping[Expression, Type] = frozendict(), sources: frozenset[str] = frozenset()) -> None:
        """Holds the build's expression types and source modules and no read yet."""
        self.types = types
        self.sources = sources
        self._modules: set[str] = set()

    def body(self, info: TypeInfo, /) -> list[Statement]:
        """Returns the statements of a source class body, recording its module, and no statement for a class outside the sources."""
        if info.module_name not in self.sources:
            return []
        self._modules.add(info.module_name)
        return info.defn.defs.body

    def taken(self) -> frozenset[str]:
        """Returns the modules whose class bodies checks read since the last call, starting a new record."""
        held = frozenset(self._modules)
        self._modules.clear()
        return held


# --- [OPERATIONS] -----------------------------------------------------------------------


def fullname(declared: Declared) -> str:
    """Returns the fullname mypy gives the runtime class or function."""
    return f"{declared.__module__}.{declared.__qualname__}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("Declared", "RasmError", "Reads", "fullname")
