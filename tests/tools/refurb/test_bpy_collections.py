"""Type tests against RNA collection structs."""

from pathlib import Path
from sys import getrecursionlimit
from textwrap import dedent
lazy from collections.abc import Callable, Mapping

from mypy.nodes import ARG_POS, Block, CallExpr, ClassDef, Expression, NameExpr, OpExpr, SymbolTable, TupleExpr, TypeInfo
import pytest
lazy from refurb.error import Error

from rasm.refurb.bpy_collections import check, ErrorCollectionTypeTestError

# --- [TYPES] ----------------------------------------------------------------------------

type Findings = Callable[[str | Mapping[str, str], *tuple[type[Error], ...]], tuple[Error, ...]]

# --- [CONSTANTS] ------------------------------------------------------------------------

CASES = frozendict(
    collection_struct_reports_and_owner_passes=(
        dedent("""
            import bpy

            def collection(stack: object, before: int, index: int) -> None:
                if isinstance(stack, bpy.types.ObjectModifiers):
                    stack.move(before, index)

            def owner(item: object, before: int, index: int) -> None:
                if isinstance(item, bpy.types.Object):
                    item.modifiers.move(before, index)
        """),
        (5,),
    ),
    nested_tuples_unions_aliases_and_class_patterns_report=(
        dedent("""
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
        """),
        (8, 8, 10, 15, 21),
    ),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
@pytest.mark.parametrize("case", tuple(CASES))
def test_collection_types_report_file_local_diagnostics(case: str, diagnostics: frozendict[str, tuple[Error, ...]]) -> None:
    """Reports each type test naming the collection struct and keeps owner type tests outside each case's exact findings."""
    assert tuple(error.line for error in diagnostics[case]) == CASES[case][1]
    assert all(error.msg.startswith("`bpy.types.ObjectModifiers` ") for error in diagnostics[case])


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


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="module")
def diagnostics(findings: Findings) -> frozendict[str, tuple[Error, ...]]:
    """Compiles every collection example in one build over the Blender stubs and indexes diagnostics by source module."""
    found = findings({case: source for case, (source, _) in CASES.items()}, ErrorCollectionTypeTestError)
    return frozendict({case: tuple(error for error in found if error.filename is not None and Path(error.filename).stem == case) for case in CASES})
