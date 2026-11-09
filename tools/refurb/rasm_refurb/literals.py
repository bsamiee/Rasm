"""Record fields typed as a union of string literals that a comparison against a string decides."""

from itertools import pairwise

import msgspec
lazy from mypy.nodes import ComparisonExpr, MemberExpr, StrExpr, Var
lazy from mypy.types import flatten_nested_unions, get_proper_type, Instance, LiteralType, Type
lazy from refurb.checks.common import get_mypy_type
lazy from refurb.error import Error

from rasm_refurb._findings import fullname, RasmError

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorLiteralFieldComparisonError(RasmError):
    """Record field typed as a union of string literals, compared against one of its strings in place of a `StrEnum` member.

    Bad:

    ```
    class Digits(msgspec.Struct, frozen=True):
        advance: Literal["tabular", "proportional"]


    other = "proportional" if digits.advance == "tabular" else "tabular"
    ```

    Good:

    ```
    class Advance(StrEnum):
        TABULAR = auto()
        PROPORTIONAL = auto()


    other = digits.advance.other
    ```
    """

    code = 104
    categories = ("readability",)


# --- [CONSTANTS] ------------------------------------------------------------------------

_STRUCT = fullname(msgspec.Struct)

# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: ComparisonExpr, errors: list[Error]) -> None:
    """Reports each `==` or `!=` between a string and a `msgspec.Struct` field whose declared type unions two or more string literals."""
    errors.extend(
        ErrorLiteralFieldComparisonError.from_node(field, f"`{field.name}` holds one of {len(members)} string literals, declare them as a `StrEnum` and compare its members")
        for (left, right), operator in zip(pairwise(node.operands), node.operators)
        if operator in {"==", "!="}
        for field, other in ((left, right), (right, left))
        if isinstance(field, MemberExpr) and isinstance(other, StrExpr)
        if isinstance(found := get_mypy_type(field.expr), Type) and isinstance(owner := get_proper_type(found), Instance) and owner.type.has_base(_STRUCT)
        if (symbol := owner.type.get(field.name)) is not None and isinstance(variable := symbol.node, Var) and variable.type is not None
        for members in (tuple(found for found in map(get_proper_type, flatten_nested_unions([variable.type])) if isinstance(found, LiteralType) and isinstance(found.value, str)),)
        if len(members) > 1
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorLiteralFieldComparisonError", "check")
