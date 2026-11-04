"""Native scene scope releasing every data-block its test creates."""

from collections.abc import Generator

import bpy
import pytest

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def scene() -> Generator[bpy.types.Scene]:
    """Yields an isolated scene and releases its test's native data-blocks at exit."""
    before = frozenset(bpy.data.all_ids)
    try:
        yield bpy.data.scenes.new("Scene regression")
    finally:
        bpy.data.batch_remove(ids=tuple(frozenset(bpy.data.all_ids) - before))
