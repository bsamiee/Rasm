"""Type tests against RNA collection structs."""

from textwrap import dedent

import pytest

from tools.refurb.bpy_collections import ErrorCollectionTypeTestError
lazy from tests.python.tools.refurb.conftest import Findings

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
def test_collection_struct_type_test_reports(findings: Findings) -> None:
    """Type test against an RNA collection struct, reported for each named struct."""
    source = dedent("""
        import bpy

        def moved(stack: object, before: int, index: int) -> None:
            if isinstance(stack, bpy.types.ObjectModifiers):
                stack.move(before, index)
    """)
    assert findings(ErrorCollectionTypeTestError, source) == [(5, "`bpy.types.ObjectModifiers` matches no `bpy_prop_collection` value, read the collection as its owner's attribute")]


@pytest.mark.parametrize("findings", ["3.13"], indirect=True)
def test_owner_type_test_passes(findings: Findings) -> None:
    """Type test against the owner struct, not reported."""
    source = dedent("""
        import bpy

        def moved(item: object, before: int, index: int) -> None:
            if isinstance(item, bpy.types.Object):
                item.modifiers.move(before, index)
    """)
    assert findings(ErrorCollectionTypeTestError, source) == []
