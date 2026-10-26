"""Type tests against RNA collection structs no runtime value matches."""

lazy from mypy.nodes import CallExpr, Expression, OpExpr, RefExpr, TupleExpr, TypeInfo
lazy from mypy.patterns import ClassPattern
lazy from refurb.checks.common import stringify
lazy from refurb.error import Error

from tools.refurb.findings import RasmError

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorCollectionTypeTestError(RasmError):
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

    code = 105
    categories = ("bpy",)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _named(expression: Expression) -> tuple[RefExpr, ...]:
    match expression:
        case TupleExpr(items=items):
            return tuple(*_named(item) for item in items)
        case OpExpr(op="|", left=left, right=right):
            return (*_named(left), *_named(right))
        case RefExpr(node=TypeInfo() as info) if info.has_base("bpy.types.bpy_prop_collection") and info.has_base("bpy.types.bpy_struct"):
            return (expression,)
        case _:
            return ()


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: CallExpr | ClassPattern, errors: list[Error]) -> None:
    """Reports each `isinstance`, `issubclass`, or class pattern naming a struct the stubs derive from both `bpy_prop_collection` and `bpy_struct`."""
    match node:
        case CallExpr(callee=RefExpr(fullname="builtins.isinstance" | "builtins.issubclass"), args=[_, tested]) | ClassPattern(class_ref=tested):
            errors.extend(
                ErrorCollectionTypeTestError.from_node(reference, f"`{stringify(reference)}` matches no `bpy_prop_collection` value, read the collection through its owner's member")
                for reference in _named(tested)
            )
        case _:
            pass
