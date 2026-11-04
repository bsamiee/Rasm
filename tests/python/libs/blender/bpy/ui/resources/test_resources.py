"""Native preview pixels, GPU upload orientation, cache lifetimes, and theme colors."""

from contextlib import ExitStack
from pathlib import Path
import weakref

import bpy
from expression import Option, Result, Some
import numpy as np
import pytest

from blender.bpy.data.rna.paths import definition
from blender.bpy.ui.resources.icons import ICONS, Icons
from blender.bpy.ui.resources.previews import PreviewKey, Previews, Textures, uploaded
from blender.bpy.ui.resources.theme import Checker, color, Metrics, Theme
from blender.records.faults import Nothing, Ok
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_icon_ids_and_retint_follow_native_previews(acquired: Acquired) -> None:
    """Packaged IDs match native preview storage and retint writes only changed colors."""
    mark = Path("mark.svg")
    with acquired((Icons(brand=Some(mark)),)) as identity:
        icons = ICONS[identity]
        preview = icons.collection[mark.as_posix()]
        assert icons.brand == Some(preview.icon_id)
        match definition(bpy.types.UILayout).functions["prop"].parameters["icon"]:
            case bpy.types.EnumProperty(enum_items=items):
                assert Icons.value(identity, "ADD") == Ok(items["ADD"].value)
            case native:
                pytest.fail(repr(native))
        assert Icons.value(identity, mark) == Ok(preview.icon_id)
        rgba = np.asarray(preview.icon_pixels_float, np.float32).reshape(-1, 4).copy()
        assert np.any(rgba[:, 3] == 1.0)
        assert icons.retinted(lambda _: Some((1.0, 0.0, 0.0, 1.0)))
        red = np.asarray(preview.icon_pixels_float, np.float32).reshape(-1, 4)
        np.testing.assert_array_equal(red[red[:, 3] == 1.0, :3], np.broadcast_to((1.0, 0.0, 0.0), red[red[:, 3] == 1.0, :3].shape))
        assert not icons.retinted(lambda _: Some((1.0, 0.0, 0.0, 1.0)))
        assert icons.retinted(Icons().tint)
        restored = np.asarray(preview.icon_pixels_float, np.float32).reshape(-1, 4)
        np.testing.assert_array_equal(restored, rgba)


def test_preview_pixels_and_pixelless_completion_have_distinct_lifetimes() -> None:
    """Native byte-aligned image and icon buffers reverse source rows; pixelless None requests remain current."""
    source = np.arange(4 * 8 * 4, dtype=np.float32).reshape(4, 8, 4)
    source *= np.float32(1 / np.iinfo(np.uint8).max)
    with Previews.opened() as previews:
        assert previews.read("missing", None) == Nothing
        written = previews.written("probe", "first", Some(source))
        match written, previews.read("probe", "first"):
            case Result(tag="ok", ok=Option(some=icon)), Option(some=Option(some=preview)):
                assert icon == preview.icon_id
                assert tuple(preview.image_size) == (source.shape[1], source.shape[0])  # type: ignore[arg-type]
                for pixels in (preview.image_pixels_float, preview.icon_pixels_float):
                    np.testing.assert_array_equal(np.asarray(pixels).reshape(source.shape), source[::-1])
            case found:
                pytest.fail(repr(found))
        assert previews.written("probe", None, Nothing) == Ok(Nothing)
        assert previews.read("probe", None) == Some(Nothing)
        assert previews.read("probe", "first") == Nothing
        assert previews.dropped("probe") == Ok(None)
        assert previews.read("probe", None) == Nothing


def test_preview_slots_distinguish_native_owners_with_the_same_local_key() -> None:
    """Native scene handles form independent slot identities without string conversion."""
    with ExitStack() as scope, Previews.opened() as previews:
        first = bpy.data.scenes.new("First preview owner")
        scope.callback(bpy.data.scenes.remove, first)
        second = bpy.data.scenes.new("Second preview owner")
        scope.callback(bpy.data.scenes.remove, second)
        local, shape, values = "main", (2, 3, 4), (0.0, 1.0)
        slots = (first, local), (second, local)
        for slot, value in zip(slots, values, strict=True):
            assert previews.written(slot, None, Some(np.full(shape, value, np.float32))).is_ok()
        for slot, value in zip(slots, values, strict=True):
            match previews.read(slot, None):
                case Option(some=Option(some=preview)):
                    np.testing.assert_array_equal(np.asarray(preview.image_pixels_float), np.full(np.prod(shape), value, np.float32))
                case absent:
                    pytest.fail(repr(absent))
        assert previews.dropped(slots[0]) == Ok(None)
        assert previews.read(slots[0], None) == Nothing
        match previews.read(slots[1], None):
            case Option(some=Option(some=preview)):
                np.testing.assert_array_equal(np.asarray(preview.image_pixels_float), np.full(np.prod(shape), values[1], np.float32))
            case absent:
                pytest.fail(repr(absent))


def test_checker_composites_associated_color_without_changing_opaque_pixels() -> None:
    """Transparent pixels expose checker cells and opaque pixels retain all source channels."""
    key = PreviewKey(key="probe", side=4, checker=Checker(primary=(1.0, 1.0, 1.0, 1.0), secondary=(0.0, 0.0, 0.0, 1.0), cell=2))
    transparent = key.checkered(np.zeros((4, 4, 4), np.float32))
    np.testing.assert_array_equal(transparent[..., 0], ((1, 1, 0, 0), (1, 1, 0, 0), (0, 0, 1, 1), (0, 0, 1, 1)))
    np.testing.assert_array_equal(transparent[..., 3], np.ones((4, 4)))
    opaque = np.zeros((4, 4, 4), np.float32)
    opaque[..., 0] = opaque[..., 3] = 1.0
    np.testing.assert_array_equal(key.checkered(opaque), opaque)
    assert PreviewKey.read("probe").side == bpy.app.render_preview_size


def test_gpu_upload_preserves_native_channels_and_row_orientation() -> None:
    """Asymmetric fractions survive GPU upload in bottom-first rows and source storage is released."""
    source = np.arange(3 * 5 * 4, dtype=np.float32).reshape(3, 5, 4)
    source /= 64
    height, width, _ = source.shape
    reference = weakref.ref(source)
    expected = source[::-1].copy()
    uploaded_texture = uploaded(source)
    del source
    match uploaded_texture:
        case Result(tag="ok", ok=texture):
            assert (texture.width, texture.height, texture.format) == (width, height, "RGBA16F")
            assert reference() is None
            np.testing.assert_array_equal(np.asarray(texture.read()), expected)
        case failed:
            pytest.fail(repr(failed))


def test_texture_cache_evicts_texture_and_metadata_together() -> None:
    """Pinned entries survive eviction, access changes recency, and clear releases complete entries."""
    textures = Textures[str]()
    source = np.arange(3 * 5 * 4, dtype=np.float32).reshape(3, 5, 4)
    source /= 64
    limit = 2
    for index, key in enumerate(("first", "second", "third")):
        shifted = source.copy()
        shifted += index / 16
        assert textures.stored(key, shifted, limit, frozenset({"first"}), metadata=key).is_ok()
    assert textures.shown("second") == Nothing
    assert textures.shown("first").map(lambda entry: entry[1]) == Some("first")
    shifted = source.copy()
    shifted += 3 / 16
    assert textures.stored("fourth", shifted, limit, frozenset(), metadata="fourth").is_ok()
    assert textures.shown("third") == Nothing
    for index, key in ((0, "first"), (3, "fourth")):
        match textures.shown(key):
            case Option(some=(texture, metadata)):
                assert metadata == key
                np.testing.assert_array_equal(np.asarray(texture.read()), (source + np.float32(index) / np.float32(16))[::-1])
            case absent:
                pytest.fail(repr(absent))
    assert textures.memory == limit * source.size * np.dtype(np.float16).itemsize
    assert textures.cleared() == limit
    assert textures.memory == 0
    assert textures.shown("first") == textures.shown("fourth") == Nothing


def test_region_colors_and_header_metrics_read_native_preferences() -> None:
    """Region colors keep native theme values and header height matches a native region."""
    metrics = Metrics.read()
    preferences = metrics.preferences
    theme = preferences.themes[0]
    assert Theme(space="VIEW_3D", region="WINDOW", preferences=preferences).back == color(theme.view_3d.space.gradients.high_gradient)
    assert Theme(space="VIEW_3D", region="UI", preferences=preferences).back == color(theme.regions.sidebars.back)
    assert Theme(space="TEXT_EDITOR", region="WINDOW", preferences=preferences).text == color(theme.text_editor.space.text)
    assert Theme(space="PROPERTIES", region="HEADER", preferences=preferences).back == (*color(theme.properties.space.header)[:3], 1.0)
    assert any(
        metrics.header == region.height for manager in bpy.data.window_managers for window in manager.windows for area in window.screen.areas for region in area.regions if region.type == "HEADER"
    )
    assert metrics.pixel_size == preferences.system.pixel_size
