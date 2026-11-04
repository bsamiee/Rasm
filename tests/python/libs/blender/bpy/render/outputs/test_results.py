"""Combined pass selection from native multipart EXR files."""

from functools import partial
from itertools import cycle
from math import prod
from pathlib import Path

from expression import Some
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO
import pytest

from blender.bpy.render.outputs.results import COMBINED, combined, COMPOSITE
from blender.imaging.files.images import emitted, interop, Subimage
from blender.records.faults import Error, Ok, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type RenderParts = tuple[Path, PyOpenColorIO.Config, dict[tuple[str, str], tuple[npt.NDArray[np.float32], PyOpenColorIO.ColorSpace]]]

# --- [OPERATIONS] -----------------------------------------------------------------------


def _render_parts(path: Path, pairs: tuple[tuple[str, str], ...]) -> RenderParts:
    """Writes distinct channels and native metadata for each supplied layer and eye."""
    config = PyOpenColorIO.GetCurrentConfig()
    rgba = "RGBA"
    shape = 2, 3, len(rgba)
    count = prod(shape)
    spaces = cycle(config.getColorSpace(role) for role in (PyOpenColorIO.ROLE_SCENE_LINEAR, PyOpenColorIO.ROLE_DATA))
    parts = {pair: (np.arange(count, dtype=np.float32).reshape(shape) + index * count, space) for index, (pair, space) in enumerate(zip(pairs, spaces))}

    def subimages(_output: OpenImageIO.ImageOutput) -> tuple[Subimage, ...]:
        """Returns native specs and pixels for each layer and eye."""
        images: list[Subimage] = []
        for (layer, view), (pixels, space) in parts.items():
            height, width, channels = pixels.shape
            spec = OpenImageIO.ImageSpec(width, height, channels, OpenImageIO.FLOAT)
            stem = f"{layer}.{COMBINED}" if layer else ""
            spec.channelnames = tuple(f"{stem}.{channel}" if stem else channel for channel in rgba)
            images.append(
                Subimage(
                    spec=spec,
                    attributes=(
                        ("name", OpenImageIO.TypeString, f"{stem}-{view}" if view else stem),
                        ("view", OpenImageIO.TypeString, view),
                        ("colorInteropID", OpenImageIO.TypeString, interop(space)),
                    ),
                    pixels=partial(Ok, pixels),
                )
            )
        return tuple(images)

    assert emitted(path, subimages) == Ok(path)
    return path, config, parts


@pytest.fixture
def render_parts(tmp_path: Path) -> RenderParts:
    """Writes raw stereo layers and a Composite holding only a left eye."""
    return _render_parts(tmp_path / "render.exr", (*((layer, view) for layer in ("Layer A", "Layer B") for view in ("left", "right")), (COMPOSITE, "left")))


@pytest.mark.parametrize("view", ["left", "right"])
def test_explicit_layer_and_eye_read_their_native_pixels(render_parts: RenderParts, view: str) -> None:
    """Layer and eye selectors read their own channel values from native part metadata."""
    path, config, parts = render_parts
    pixels, space = parts["Layer B", view]
    answer = combined(str(path), config, Some("Layer B"), view=Some(view)).default_with(lambda faults: pytest.fail(str(faults)))
    np.testing.assert_array_equal(answer.pixels, pixels)
    assert answer.colorspace == config.getCanonicalName(interop(space))
    assert answer.header.get_string_attribute("view") == view
    assert answer.header.get_string_attribute("colorInteropID") == interop(space)


def test_default_output_reads_composite_before_raw_layer(render_parts: RenderParts) -> None:
    """Saved Composite pixels take precedence over raw view layers with matching eye metadata."""
    path, config, parts = render_parts
    pixels, space = parts[COMPOSITE, "left"]
    answer = combined(str(path), config).default_with(lambda faults: pytest.fail(str(faults)))
    np.testing.assert_array_equal(answer.pixels, pixels)
    assert answer.colorspace == config.getCanonicalName(interop(space))
    assert answer.header.get_string_attribute("view") == "left"


@pytest.mark.parametrize("owner", ["", "Only Layer"])
def test_default_output_reads_unnamed_or_single_raw_layer(tmp_path: Path, owner: str) -> None:
    """Unnamed sequencer output and a sole raw layer read their stored pixels and source space."""
    path, config, parts = _render_parts(tmp_path / "native.exr", ((owner, ""),))
    pixels, space = parts[owner, ""]
    answer = combined(str(path), config).default_with(lambda faults: pytest.fail(str(faults)))
    np.testing.assert_array_equal(answer.pixels, pixels)
    assert answer.colorspace == config.getCanonicalName(interop(space))


def test_missing_composite_eye_does_not_select_another_layer(render_parts: RenderParts) -> None:
    """Missing eye on saved Composite fails while the same eye exists on raw layers."""
    path, config, _parts = render_parts
    answer = combined(str(path), config, view=Some("right"))
    assert answer.map_error(lambda faults: tuple(fault.reason for fault in faults)) == Error((Reason.ABSENT,))


def test_multiview_layer_requires_eye_selection(render_parts: RenderParts) -> None:
    """Unselected stereo view preserves native alternatives in its typed failure."""
    path, config, _parts = render_parts
    answer = combined(str(path), config, Some("Layer B"))
    assert answer.map_error(lambda faults: tuple((fault.reason, fault.cause) for fault in faults)) == Error(((Reason.REQUIRED, (Some("left"), Some("right"))),))


@pytest.mark.parametrize("layer,view", [("Missing", "left"), ("Layer B", "center")])
def test_absent_layer_or_eye_returns_typed_failure(render_parts: RenderParts, layer: str, view: str) -> None:
    """Selectors absent from native file metadata fail instead of choosing another part."""
    path, config, _parts = render_parts
    answer = combined(str(path), config, Some(layer), view=Some(view))
    assert answer.map_error(lambda faults: tuple(fault.reason for fault in faults)) == Error((Reason.ABSENT,))
