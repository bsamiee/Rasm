"""Literal mappings wrapped in `types.MappingProxyType` where Python 3.15 `frozendict` holds them."""

from mypy.nodes import ArgKind, CallExpr, DictExpr, DictionaryComprehension, RefExpr
from refurb.error import Error
from refurb.settings import Settings

from rasm.refurb._findings import RasmError

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorLiteralMappingProxyError(RasmError):
    """Literal mapping wrapped in `types.MappingProxyType` on Python 3.15, where `frozendict` holds it with no backing dictionary.

    Bad:

    ```
    TABLE = MappingProxyType({"a": 1})
    ```

    Good:

    ```
    TABLE = frozendict({"a": 1})
    ```
    """

    code = 108
    categories = ("builtin",)


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: CallExpr, errors: list[Error], settings: Settings) -> None:
    """Reports each `MappingProxyType` call over one dictionary display or comprehension when refurb runs Python 3.15 or later."""
    match node:
        case CallExpr(callee=RefExpr(fullname=name), args=[DictExpr() | DictionaryComprehension()], arg_kinds=[ArgKind.ARG_POS]) if (
            name == "types.MappingProxyType" and settings.get_python_version() >= (3, 15)
        ):
            errors.append(ErrorLiteralMappingProxyError.from_node(node, "Write `frozendict(<mapping>)`"))
        case _:
            pass


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorLiteralMappingProxyError", "check")
