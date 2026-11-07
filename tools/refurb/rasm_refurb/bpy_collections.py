"""Type tests against RNA collection structs no runtime value matches."""

lazy from collections import deque

lazy from mypy.nodes import CallExpr, OpExpr, RefExpr, TupleExpr, TypeAlias, TypeInfo
lazy from mypy.patterns import ClassPattern
lazy from mypy.types import flatten_nested_unions, get_proper_type, Instance
lazy from refurb.error import Error

from rasm_refurb._findings import RasmError

# --- [ERRORS] ---------------------------------------------------------------------------


class ErrorCollectionTypeTestError(RasmError):
    """Type test against an RNA collection struct no `bpy_prop_collection` value matches in place of a read through the owner.

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


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(node: CallExpr | ClassPattern, errors: list[Error]) -> None:
    """Reports each `isinstance`, `issubclass`, or class pattern naming a struct the stubs derive from both `bpy_prop_collection` and `bpy_struct`."""
    match node:
        case CallExpr(callee=RefExpr(fullname="builtins.isinstance" | "builtins.issubclass"), args=[_, tested]) | ClassPattern(class_ref=tested):
            pending = deque((tested,))
        case _:
            return
    while pending:
        match reference := pending.popleft():
            case TupleExpr(items=items):
                pending.extendleft(reversed(items))
            case OpExpr(op="|", left=left, right=right):
                pending.extendleft((right, left))
            case RefExpr(node=(TypeInfo() | TypeAlias()) as owner):
                errors.extend(
                    ErrorCollectionTypeTestError.from_node(reference, f"`{info.fullname}` names a collection declaration, read the collection as its owner's attribute")
                    for found in ((owner,) if isinstance(owner, TypeInfo) else map(get_proper_type, flatten_nested_unions([owner.target])))
                    if isinstance(info := found.type if isinstance(found, Instance) else found, TypeInfo) and info.has_base("bpy.types.bpy_prop_collection") and info.has_base("bpy.types.bpy_struct")
                )
            case _:
                pass


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ErrorCollectionTypeTestError", "check")
