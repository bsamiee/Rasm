"""Scoped offscreen drawing and framebuffer pixel reads."""

from collections.abc import Callable
from typing import Literal

import bpy
from expression import Error, Nothing, Ok
from expression.extra.result import catch
import gpu
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.faults import BlenderReason
from blender.bpy.ui.drawing.canvas import Canvas, Rect
from blender.bpy.ui.resources.theme import Metrics, Theme
from blender.bpy.ui.screens.areas import Located
from blender.bpy.ui.screens.editors import EditorState
from blender.records.faults import Fault, Outcome, paired

# --- [MODELS] ---------------------------------------------------------------------------


class Offscreen(msgspec.Struct, frozen=True, kw_only=True):
    """Color buffer at caller device-pixel size and precision."""

    width: int
    height: int
    format: Literal["RGBA8", "RGBA16", "RGBA16F", "RGBA32F"] = "RGBA8"

    def read(self, draw: Callable[[gpu.types.GPUOffScreen], Outcome[None]]) -> Outcome[npt.NDArray[np.float32]]:
        """Draws into a buffer it owns for the call, returning top-first RGBA, refusing sizes past the texture limit."""
        try:
            gpu.init()
            limit = gpu.capabilities.max_texture_size_get()
        except (RuntimeError, SystemError) as error:
            return Error((Fault(reason=BlenderReason.REFUSED, subject="gpu.init", cause=error),))
        if max(self.width, self.height) > limit:
            return Error((Fault(reason=BlenderReason.REFUSED, subject="gpu.types.GPUOffScreen", cause=ValueError(f"{self.width}x{self.height} exceeds {limit}px texture limit")),))
        try:
            buffer = gpu.types.GPUOffScreen(self.width, self.height, format=self.format)
        except (RuntimeError, SystemError) as error:
            return Error((Fault(reason=BlenderReason.REFUSED, subject="gpu.types.GPUOffScreen", cause=error),))
        try:
            return draw(buffer).map(lambda _: unpacked(buffer.texture_color.read()))
        finally:
            buffer.free()

    def drawn(self, theme: Theme, metrics: Metrics, draw: Callable[[Canvas], None]) -> Outcome[npt.NDArray[np.float32]]:
        """Draws on pixel canvas, returning top-first RGBA."""

        def painted(buffer: gpu.types.GPUOffScreen) -> Outcome[None]:
            """Draws callback under pixel projection with depth test off."""
            canvas = Canvas(theme=theme, metrics=metrics, extent=Rect(x=0.0, y=0.0, width=float(self.width), height=float(self.height)))
            depth = gpu.state.depth_test_get()
            try:
                with buffer.bind(), canvas.drawn():  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
                    gpu.state.depth_test_set("NONE")
                    gpu.state.active_framebuffer_get().clear(color=(0.0, 0.0, 0.0, 0.0))
                    gpu.state.scissor_set(0, 0, self.width, self.height)
                    gpu.state.scissor_test_set(True)
                    gpu.matrix.load_projection_matrix(Matrix.Identity(4))
                    gpu.matrix.load_matrix(Matrix.Translation((-1.0, -1.0, 0.0)) @ Matrix.Diagonal((2.0 / self.width, 2.0 / self.height, 1.0, 1.0)))
                    draw(canvas)
                return Ok(None)
            finally:
                gpu.state.depth_test_set(depth)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

        return self.read(painted)

    def viewed(
        self, located: Located, camera: bpy.types.Object, scene: bpy.types.Scene, view_layer: bpy.types.ViewLayer, *, do_color_management: bool = True, draw_background: bool = True
    ) -> Outcome[npt.NDArray[np.float32]]:
        """Draws camera view with overlays and gizmos off, restoring the editor values after."""
        space = located.area.spaces.active
        if not isinstance(space, bpy.types.SpaceView3D):
            return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(located.area), member=bpy.types.SpaceView3D.__name__),))

        def painted(region: bpy.types.Region, buffer: gpu.types.GPUOffScreen) -> Outcome[None]:
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
                return Error((Fault(reason=BlenderReason.REFUSED, subject="gpu.types.GPUOffScreen.draw_view3d", cause=error),))
            return Ok(None)

        def restored(region: bpy.types.Region, state: EditorState) -> Outcome[npt.NDArray[np.float32]]:
            """Reads view pixels between the editor write and its restore."""
            return state.applied(located.area).bind(
                lambda prior: paired(self.read(lambda buffer: painted(region, buffer)), prior.map(lambda held: held.applied(located.area)).default_value(Ok(Nothing))).map(lambda pair: pair[0])
            )

        return paired(located.region(), EditorState.of(bpy.types.SpaceView3D, {"overlay.show_overlays": False, "show_gizmo": False})).bind(lambda pair: restored(*pair))


# --- [OPERATIONS] -----------------------------------------------------------------------


def unpacked(read: gpu.types.Buffer) -> npt.NDArray[np.float32]:
    """Converts bottom-first pixels to top-first float32 plane."""
    pixels = np.asarray(read)
    scaled = np.divide(pixels, np.iinfo(pixels.dtype).max, dtype=np.float32) if np.issubdtype(pixels.dtype, np.integer) else pixels
    return np.ascontiguousarray(scaled[::-1], dtype=np.float32)


def captured(rect: Rect) -> Outcome[npt.NDArray[np.float32]]:
    """Reads float RGBA from active framebuffer inside a draw callback."""
    return (
        catch(gpu.state.active_framebuffer_get().read_color, exception=ValueError)(round(rect.x), round(rect.y), round(rect.width), round(rect.height), 4, 0, "FLOAT")
        .map(unpacked)
        .map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject="gpu.types.GPUFrameBuffer.read_color", cause=error),))
    )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Offscreen", "captured", "unpacked"]
