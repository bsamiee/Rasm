"""Type tests against RNA collection structs."""

from sys import getrecursionlimit
from textwrap import dedent
lazy from collections.abc import Callable

from mypy.nodes import ARG_POS, Block, CallExpr, ClassDef, Expression, NameExpr, OpExpr, SymbolTable, TupleExpr, TypeInfo
import pytest
lazy from refurb.error import Error

from tools.refurb.bpy_collections import check, ErrorCollectionTypeTestError
lazy from tests.python.tools.refurb.conftest import Findings

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
def test_collection_struct_reports_and_owner_passes(findings: Findings) -> None:
    """Reports RNA collection type tests and accepts an owner type test in the same module."""
    source = dedent("""
        import bpy

        def collection(stack: object, before: int, index: int) -> None:
            if isinstance(stack, bpy.types.ObjectModifiers):
                stack.move(before, index)

        def owner(item: object, before: int, index: int) -> None:
            if isinstance(item, bpy.types.Object):
                item.modifiers.move(before, index)
    """)
    assert [error.line for error in findings(source, ErrorCollectionTypeTestError)] == [5]


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
def test_nested_collection_types_report(findings: Findings) -> None:
    """Nested tuples, unions, aliases, and class patterns report collection references."""
    source = dedent("""
        import bpy

        Modifiers = bpy.types.ObjectModifiers
        Stack = Modifiers

        def moved(item: object) -> None:
            if isinstance(item, (bpy.types.Object, (Stack, bpy.types.ObjectModifiers | bpy.types.Object))):
                pass
            if issubclass(type(item), Modifiers):
                pass

        def matched(item: Modifiers) -> None:
            match item:
                case Stack():
                    pass

        Kinds = Stack | object

        def union(item: object) -> bool:
            return isinstance(item, Kinds)
    """)
    found = findings(source, ErrorCollectionTypeTestError)
    assert [error.line for error in found] == [8, 8, 10, 15, 21]
    assert all(error.msg.startswith("`bpy.types.ObjectModifiers` ") for error in found)


@pytest.mark.parametrize(
    "combined", [pytest.param(lambda left, right: TupleExpr([left, right]), id=TupleExpr.__name__), pytest.param(lambda left, right: OpExpr("|", left, right), id=OpExpr.__name__)]
)
def test_deep_collection_types_report(combined: Callable[[Expression, Expression], Expression]) -> None:
    """Collection references beyond Python's recursion limit retain every diagnostic."""
    declarations = [ClassDef(name, Block([])) for name in ("ObjectModifiers", "bpy_prop_collection", "bpy_struct")]
    for declaration in declarations:
        declaration.fullname = f"bpy.types.{declaration.name}"
    owners = [TypeInfo(SymbolTable(), declaration, "bpy.types") for declaration in declarations]
    owners[0].mro = owners
    reference = NameExpr(owners[0].name)
    reference.node = owners[0]
    depth = getrecursionlimit() * 2
    tested = combined(reference, reference)
    for _ in range(1, depth):
        tested = combined(tested, reference)
    callee = NameExpr("isinstance")
    callee.fullname = "builtins.isinstance"
    errors: list[Error] = []
    check(CallExpr(callee, [NameExpr("item"), tested], [ARG_POS, ARG_POS], []), errors)
    assert len(errors) == depth + 1
