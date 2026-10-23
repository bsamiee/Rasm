"""Scoped offscreen drawing and framebuffer pixel reads."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from typing import Literal

import bpy
from expression import Error, Nothing, Ok, Option, Result
import gpu
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.ui.drawing.canvas import Canvas, Rect
from blender.bpy.ui.resources.theme import Metrics, Theme
from blender.bpy.ui.screens.areas import Located
from blender.bpy.ui.screens.editors import EditorState
from blender.imaging.faults import paired

# --- [MODELS] ---------------------------------------------------------------------------


class Offscreen(msgspec.Struct, frozen=True, kw_only=True):
    """Color buffer at caller device-pixel size and precision."""

    width: int
    height: int
    format: Literal["RGBA8", "RGBA16", "RGBA16F", "RGBA32F"] = "RGBA8"

    @contextmanager
    def allocated(self) -> Generator[BlenderResult[gpu.types.GPUOffScreen]]:
        """Initializes GPU and owns buffer, refusing sizes past texture limit."""
        held: gpu.types.GPUOffScreen | ValueError | RuntimeError | SystemError
        try:
            gpu.init()
            limit = gpu.capabilities.max_texture_size_get()
            held = (
                gpu.types.GPUOffScreen(self.width, self.height, format=self.format)
                if max(self.width, self.height) <= limit
                else ValueError(f"{self.width}x{self.height} exceeds {limit}px texture limit")
            )
        except (RuntimeError, SystemError) as error:
            held = error
        match held:
            case gpu.types.GPUOffScreen() as buffer:
                try:
                    yield Result[gpu.types.GPUOffScreen, BlenderFaults].Ok(buffer)
                finally:
                    buffer.free()
            case refused:
                yield Result[gpu.types.GPUOffScreen, BlenderFaults].Error((BlenderEditFault(request="gpu.types.GPUOffScreen", error=refused),))

    def drawn(self, theme: Theme, metrics: Metrics, draw: Callable[[Canvas], None]) -> BlenderResult[npt.NDArray[np.float32]]:
        """Draws on pixel canvas, returning top-first RGBA."""

        def rendered(buffer: gpu.types.GPUOffScreen) -> npt.NDArray[np.float32]:
            """Draws callback under pixel projection."""
            width, height = buffer.width, buffer.height
            canvas = Canvas(theme=theme, metrics=metrics, extent=Rect(x=0.0, y=0.0, width=float(width), height=float(height)))
            depth = gpu.state.depth_test_get()
            try:
                with buffer.bind(), canvas.drawn():  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
                    gpu.state.depth_test_set("NONE")
                    gpu.state.active_framebuffer_get().clear(color=(0.0, 0.0, 0.0, 0.0))
                    gpu.state.scissor_set(0, 0, width, height)
                    gpu.state.scissor_test_set(True)
                    gpu.matrix.load_projection_matrix(Matrix.Identity(4))
                    gpu.matrix.load_matrix(Matrix.Translation((-1.0, -1.0, 0.0)) @ Matrix.Diagonal((2.0 / width, 2.0 / height, 1.0, 1.0)))
                    draw(canvas)
                return unpacked(buffer.texture_color.read())
            finally:
                gpu.state.depth_test_set(depth)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

        with self.allocated() as allocation:
            return allocation.map(rendered)

    def viewed(
        self, located: Located, camera: bpy.types.Object, scene: bpy.types.Scene, view_layer: bpy.types.ViewLayer, *, do_color_management: bool = True, draw_background: bool = True
    ) -> BlenderResult[npt.NDArray[np.float32]]:
        """Draws camera view with overlays and gizmos off."""
        space = located.area.spaces.active
        if not isinstance(space, bpy.types.SpaceView3D):
            return Error((BlenderAbsentFault(owner=repr(located.area), key=bpy.types.SpaceView3D.__name__),))

        def restored(region: bpy.types.Region, prior: Option[EditorState]) -> BlenderResult[npt.NDArray[np.float32]]:
            """Reads view pixels, restoring editor values."""

            def rendered(buffer: gpu.types.GPUOffScreen) -> BlenderResult[npt.NDArray[np.float32]]:
                """Draws camera view into buffer."""
                try:
                    buffer.draw_view3d(
                        scene,
                        view_layer,
                        space,
                        region,
                        camera.matrix_world.normalized().inverted(),
                        camera.calc_matrix_camera(view_layer.depsgraph, x=buffer.width, y=buffer.height),
                        do_color_management=do_color_management,
                        draw_background=draw_background,
                    )
                except (RuntimeError, ValueError) as error:
                    return Error((BlenderEditFault(request="gpu.types.GPUOffScreen.draw_view3d", error=error),))
                return Ok(unpacked(buffer.texture_color.read()))

            try:
                with self.allocated() as allocation:
                    plane = allocation.bind(rendered)
            finally:
                restoration = prior.map(lambda state: state.applied(located.area)).default_value(Ok(Nothing))
            return paired(plane, restoration).map(lambda pair: pair[0])

        return paired(located.region(), EditorState.of(bpy.types.SpaceView3D, {"overlay.show_overlays": False, "show_gizmo": False})).bind(
            lambda pair: pair[1].applied(located.area).bind(lambda prior: restored(pair[0], prior))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def unpacked(read: gpu.types.Buffer) -> npt.NDArray[np.float32]:
    """Converts bottom-first pixels to top-first float32 plane."""
    pixels = np.asarray(read)
    scaled = np.divide(pixels, np.iinfo(pixels.dtype).max, dtype=np.float32) if np.issubdtype(pixels.dtype, np.integer) else pixels
    return np.ascontiguousarray(scaled[::-1], dtype=np.float32)


def captured(rect: Rect) -> BlenderResult[npt.NDArray[np.float32]]:
    """Reads float RGBA from active framebuffer inside a draw callback."""
    try:
        read = gpu.state.active_framebuffer_get().read_color(round(rect.x), round(rect.y), round(rect.width), round(rect.height), 4, 0, "FLOAT")
    except ValueError as error:
        return Error((BlenderEditFault(request="gpu.types.GPUFrameBuffer.read_color", error=error),))
    return Ok(unpacked(read))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Offscreen", "captured", "unpacked"]
