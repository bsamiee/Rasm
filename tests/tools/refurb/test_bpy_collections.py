"""RASM105: type tests against RNA collection structs, which no `bpy_prop_collection` value matches."""

from collections.abc import Callable
from sys import getrecursionlimit
from textwrap import dedent

from mypy.nodes import ARG_POS, Block, CallExpr, ClassDef, Expression, NameExpr, OpExpr, SymbolTable, TupleExpr, TypeInfo
import pytest
lazy from refurb.error import Error

from rasm.refurb.bpy_collections import check, ErrorCollectionTypeTestError

# --- [CONSTANTS] ------------------------------------------------------------------------

CASES = frozendict(
    collection_instance=dedent("""\
        import bpy

        def moved(stack: object, before: int, index: int) -> None:
            if isinstance(stack, bpy.types.ObjectModifiers):
                stack.move(before, index)
    """),
    owner_instance=dedent("""\
        import bpy

        def moved(item: object, before: int, index: int) -> None:
            if isinstance(item, bpy.types.Object):
                item.modifiers.move(before, index)
    """),
    nested_tuple_and_union=dedent("""\
        import bpy

        def tested(item: object) -> bool:
            return isinstance(item, (bpy.types.Object, (bpy.types.ObjectModifiers, bpy.types.ObjectModifiers | bpy.types.Object)))
    """),
    collection_subclass=dedent("""\
        import bpy

        def tested(item: object) -> bool:
            return issubclass(type(item), bpy.types.ObjectModifiers)
    """),
    class_pattern=dedent("""\
        import bpy

        def matched(item: object) -> bool:
            match item:
                case bpy.types.ObjectModifiers():
                    return True
                case _:
                    return False
    """),
    aliases=dedent("""\
        import bpy

        Stack = bpy.types.ObjectModifiers
        Kinds = Stack | object

        def tested(item: object) -> tuple[bool, bool]:
            return isinstance(item, Stack), isinstance(item, Kinds)
    """),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("rule", [ErrorCollectionTypeTestError], scope="module")
@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
@pytest.mark.parametrize(
    "case, lines", [("collection_instance", [4]), ("owner_instance", []), ("nested_tuple_and_union", [4, 4]), ("collection_subclass", [4]), ("class_pattern", [5]), ("aliases", [7, 7])]
)
def test_a_type_test_naming_a_collection_struct_reports(findings: frozendict[str, tuple[Error, ...]], case: str, lines: list[int]) -> None:
    """`isinstance`, `issubclass`, and class patterns naming the struct report through tuples, unions, and aliases, and a test of the owner passes."""
    assert [error.line for error in findings[case]] == lines
    assert all(error.msg.startswith("`bpy.types.ObjectModifiers` ") for error in findings[case])


@pytest.mark.parametrize(
    "combined", [pytest.param(lambda left, right: TupleExpr([left, right]), id=TupleExpr.__name__), pytest.param(lambda left, right: OpExpr("|", left, right), id=OpExpr.__name__)]
)
def test_collection_references_past_the_recursion_limit_each_report(combined: Callable[[Expression, Expression], Expression]) -> None:
    """A type test nesting the struct twice Python's recursion limit deep reports every reference."""
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
