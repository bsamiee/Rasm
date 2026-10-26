"""Type tests against RNA collection structs no runtime value matches."""

from collections.abc import Iterator

from mypy.nodes import CallExpr, Expression, OpExpr, RefExpr, TupleExpr, TypeInfo
from mypy.patterns import ClassPattern
from refurb.checks.common import stringify
from refurb.error import Error

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorCollectionTypeTestError(Error):
    """Type test against an RNA collection struct no `bpy_prop_collection` value matches.

    Bad:

    ```
    if isinstance(stack, bpy.types.ObjectModifiers):
        stack.move(before, index)
    ```

    Good:

    ```
    item.modifiers.move(before, index)
    ```
    """

    prefix = "RASM"
    code = 105
    name = "collection-type-test"
    categories = ("bpy",)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _named(expression: Expression) -> Iterator[RefExpr]:
    match expression:
        case TupleExpr(items=items):
            yield from (*_named(item) for item in items)
        case OpExpr(op="|", left=left, right=right):
            yield from (*_named(left), *_named(right))
        case RefExpr(node=TypeInfo() as info) if info.has_base("bpy.types.bpy_prop_collection") and info.has_base("bpy.types.bpy_struct"):
            yield expression
        case _:
            pass


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: CallExpr | ClassPattern, errors: list[Error]) -> None:
    """Reports each `isinstance`, `issubclass`, or class pattern naming a struct the stubs derive from both `bpy_prop_collection` and `bpy_struct`."""
    match node:
        case CallExpr(callee=RefExpr(fullname="builtins.isinstance" | "builtins.issubclass"), args=[_, tested]):
            found = tuple(_named(tested))
        case ClassPattern(class_ref=reference):
            found = tuple(_named(reference))
        case _:
            found = ()
    errors.extend(
        ErrorCollectionTypeTestError.from_node(reference, f"`{stringify(reference)}` matches no `bpy_prop_collection` value, read the collection through its owner's member") for reference in found
    )
