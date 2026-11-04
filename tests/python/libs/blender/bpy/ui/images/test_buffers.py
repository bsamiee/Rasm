"""Native Image Editor OCIO policy and full RGBA projection."""

from collections.abc import Generator
from contextlib import closing, ExitStack

import bpy
from expression import Result
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO
import pytest

from blender.bpy.render.outputs.results import built
from blender.bpy.ui.images.buffers import projection
from blender.imaging.color.processors import DisplayView
from blender.records.faults import Fault, Ok, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type _Editor = tuple[bpy.types.Context, bpy.types.Scene, bpy.types.SpaceImageEditor, bpy.types.Image, bpy.types.Image, bpy.types.ColorManagedViewSettings, bpy.types.ColorManagedDisplaySettings]

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_file_color_uses_the_live_render_view(editor: _Editor) -> None:
    """A render-view file uses the calling scene's exposure and gamma."""
    context, _, space, _, image, settings, display = editor
    space.image, space.display_channels, image.use_view_as_render = image, "COLOR", True
    settings.exposure, settings.gamma = 2.0, 1.5
    match projection(context, image, PyOpenColorIO.GetCurrentConfig()):
        case Result(tag="ok", ok=view):
            assert (view.display, view.view, view.exposure, view.gamma) == (display.display_device, settings.view_transform, 2.0, 1.5)
        case failed:
            pytest.fail(repr(failed))


def test_generated_images_use_the_display_default(editor: _Editor) -> None:
    """Generated buffers ignore scene viewing adjustments even with view-as-render enabled."""
    context, _, space, image, _, settings, display = editor
    config = PyOpenColorIO.GetCurrentConfig()
    space.image, space.display_channels, image.use_view_as_render = image, "COLOR", True
    settings.exposure, settings.gamma = 3.0, 1.5
    match projection(context, image, config):
        case Result(tag="ok", ok=view):
            assert view == DisplayView(display=display.display_device, view=config.getDefaultView(display.display_device))
        case failed:
            pytest.fail(repr(failed))


def test_file_without_render_view_uses_the_display_default(editor: _Editor) -> None:
    """Disabling view-as-render removes the scene's exposure, gamma and look."""
    context, _, space, _, image, settings, display = editor
    config = PyOpenColorIO.GetCurrentConfig()
    space.image, space.display_channels, image.use_view_as_render = image, "COLOR", False
    settings.exposure, settings.gamma = 2.0, 1.5
    match projection(context, image, config):
        case Result(tag="ok", ok=view):
            assert view == DisplayView(display=display.display_device, view=config.getDefaultView(display.display_device))
        case failed:
            pytest.fail(repr(failed))


def test_alpha_display_uses_the_default_even_for_a_render_view_file(editor: _Editor) -> None:
    """Alpha display selects the display default while leaving RGBA interpretation to its caller."""
    context, _, space, _, image, settings, display = editor
    config = PyOpenColorIO.GetCurrentConfig()
    space.image, space.display_channels, image.use_view_as_render = image, "ALPHA", True
    settings.exposure, settings.gamma = 2.0, 1.5
    match projection(context, image, config):
        case Result(tag="ok", ok=view):
            assert view == DisplayView(display=display.display_device, view=config.getDefaultView(display.display_device))
        case failed:
            pytest.fail(repr(failed))


@pytest.mark.parametrize("render_view", [False, True])
def test_projection_matches_independent_ocio_operations_and_retains_alpha(editor: _Editor, *, render_view: bool) -> None:
    """Full associated RGBA agrees with separate exposure, display-view and gamma transforms."""
    context, _, space, _, image, settings, shown = editor
    config = PyOpenColorIO.GetCurrentConfig()
    display = shown.display_device
    settings.view_transform = config.getDefaultView(display)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    settings.look, settings.exposure, settings.gamma = "None", 1.5, 1.25  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    space.image, space.display_channels, image.use_view_as_render = image, "COLOR", render_view
    source = config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()
    straight = np.array((((0.05, 0.2, 1.5), (0.8, 0.1, 0.4)), ((0.3, 2.0, 0.1), (1.2, 0.6, 0.25))), np.float32)
    alpha = np.array(((0.125, 0.25), (0.5, 1.0)), np.float32)[..., None]
    associated = np.concatenate((straight * alpha, alpha), axis=-1)
    original = associated.copy()
    expected = associated[..., :3] / alpha
    transforms = PyOpenColorIO.GroupTransform([
        PyOpenColorIO.MatrixTransform.Scale([2.0 ** (settings.exposure if render_view else 0.0)] * 3 + [1.0]),
        PyOpenColorIO.DisplayViewTransform(src=source, display=display, view=config.getDefaultView(display)),
        PyOpenColorIO.ExponentTransform([1.0 / (settings.gamma if render_view else 1.0)] * 3 + [1.0]),
    ])
    config.getProcessor(transforms).getDefaultCPUProcessor().applyRGB(expected)

    def matches(pixels: npt.NDArray[np.float32]) -> None:
        np.testing.assert_array_equal(pixels[..., :3], expected * alpha)
        np.testing.assert_array_equal(pixels[..., 3:], alpha)
        np.testing.assert_array_equal(associated, original)

    assert projection(context, image, config).bind(lambda view: view.form(config, associated, source)).map(matches) == Ok(None)


def test_actual_render_result_uses_the_calling_scene_view(editor: _Editor) -> None:
    """A real native render product follows the current editor scene after rendering completes."""
    context, scene, space, _, _, settings, _ = editor
    with ExitStack() as release:
        camera = bpy.data.cameras.new("Projection Camera")
        release.callback(bpy.data.cameras.remove, camera)
        item = bpy.data.objects.new("Projection Camera", camera)
        release.callback(bpy.data.objects.remove, item, do_unlink=True)
        scene.collection.objects.link(item)
        scene.camera = item
        scene.render.engine = "BLENDER_WORKBENCH"  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        scene.render.resolution_x, scene.render.resolution_y, scene.render.resolution_percentage = 8, 8, 100
        bpy.ops.render.render(scene=scene.name)
        match built("RENDER_RESULT"):
            case Result(tag="ok", ok=image):
                space.image, space.display_channels, image.use_view_as_render = image, "COLOR", True
                settings.exposure = exposure = 2.5
                match projection(context, image, PyOpenColorIO.GetCurrentConfig()):
                    case Result(tag="ok", ok=view):
                        assert view.exposure == exposure
                    case failed:
                        pytest.fail(repr(failed))
            case failed:
                pytest.fail(repr(failed))


def test_projection_accumulates_absent_context_owners(editor: _Editor) -> None:
    """Missing scene and Image Editor remain separate typed context faults."""
    _, _, _, _, image, _, _ = editor
    with bpy.context.temp_override(area=None, region=None, scene=None):
        match projection(bpy.context, image, PyOpenColorIO.GetCurrentConfig()):
            case Result(error=(Fault(reason=first, member=scene), Fault(reason=second, member=space))):
                assert (first, second) == (Reason.ABSENT, Reason.ABSENT)
                assert (scene, space) == ("scene", "space_data")
            case failed:
                pytest.fail(repr(failed))


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="module")
def editor(tmp_path_factory: pytest.TempPathFactory) -> Generator[_Editor]:
    """Holds an actual Image Editor, scene, generated buffer and loaded float EXR."""
    with ExitStack() as release:
        window = bpy.data.window_managers[0].windows[0]
        area = next(area for area in window.screen.areas if area.type == "VIEW_3D")
        release.enter_context(bpy.context.temp_override(window=window, area=area, region=None))
        release.callback(setattr, area, "ui_type", area.ui_type)
        scene = bpy.data.scenes.new("Projection")
        release.callback(bpy.data.scenes.remove, scene)
        release.callback(setattr, window, "scene", window.scene)
        window.scene = scene
        settings: bpy.types.ColorManagedViewSettings = scene.view_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        display: bpy.types.ColorManagedDisplaySettings = scene.display_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        generated = bpy.data.images.new("Generated Projection", width=2, height=2, alpha=True, float_buffer=True)
        release.callback(bpy.data.images.remove, generated)
        path = tmp_path_factory.mktemp("projection") / "source.exr"
        output = OpenImageIO.ImageOutput.create(str(path))
        assert output is not None
        with closing(output):
            assert output.open(str(path), OpenImageIO.ImageSpec(2, 2, 4, OpenImageIO.FLOAT))
            assert output.write_image(np.ones((2, 2, 4), np.float32))
        image = bpy.data.images.load(str(path), check_existing=False)
        release.callback(bpy.data.images.remove, image)
        area.ui_type = "IMAGE_EDITOR"
        match area.spaces.active:
            case bpy.types.SpaceImageEditor() as space:
                region = next(region for region in area.regions if region.type == "WINDOW")
                with bpy.context.temp_override(region=region):
                    yield bpy.context, scene, space, generated, image, settings, display
            case failed:
                pytest.fail(repr(failed))
