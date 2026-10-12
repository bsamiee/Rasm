"""Scoped native offscreen drawing and framebuffer pixel reads."""

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

from rasm.blender.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from rasm.blender.ui.drawing.canvas import Canvas, Rect
from rasm.blender.ui.resources.theme import Metrics, Theme
from rasm.blender.ui.screens.areas import Located
from rasm.blender.ui.screens.editors import EditorState
from rasm.imaging.faults import paired

# --- [MODELS] ---------------------------------------------------------------------------


class Offscreen(msgspec.Struct, frozen=True, kw_only=True):
    """Native color buffer with a caller's device-pixel size and precision."""

    width: int
    height: int
    format: Literal["RGBA8", "RGBA16", "RGBA16F", "RGBA32F"] = "RGBA8"

    @contextmanager
    def allocated(self) -> Generator[BlenderResult[gpu.types.GPUOffScreen]]:
        """Initializes the GPU and owns a native buffer until scope exit, refusing a side past the device texture limit that aborts Blender at its first clear."""
        held: gpu.types.GPUOffScreen | ValueError | RuntimeError | SystemError
        try:
            gpu.init()
            limit = gpu.capabilities.max_texture_size_get()
            held = (
                gpu.types.GPUOffScreen(self.width, self.height, format=self.format)
                if max(self.width, self.height) <= limit
                else ValueError(f"{self.width}x{self.height} passes the {limit} pixel texture limit")
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
        """Draws on a pixel canvas and returns top-first float32 RGBA pixels."""

        def rendered(buffer: gpu.types.GPUOffScreen) -> npt.NDArray[np.float32]:
            """Owns the framebuffer and 2D state while the callback draws."""
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
        """Returns a camera view with overlays and gizmos restored after drawing."""
        space = located.area.spaces.active
        if not isinstance(space, bpy.types.SpaceView3D):
            return Error((BlenderAbsentFault(owner=repr(located.area), key=bpy.types.SpaceView3D.__name__),))

        def restored(region: bpy.types.Region, prior: Option[EditorState]) -> BlenderResult[npt.NDArray[np.float32]]:
            """Reads the native view and restores changed editor values on every exit."""

            def rendered(buffer: gpu.types.GPUOffScreen) -> BlenderResult[npt.NDArray[np.float32]]:
                """Draws the native camera and retains its allocation refusals."""
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
            completed = paired(plane, restoration)
            match completed.tag:
                case "ok":
                    pixels, _ = completed.ok
                    return Ok(pixels)
                case "error":
                    return Error(completed.error)

        prepared = paired(located.region(), EditorState.of(bpy.types.SpaceView3D, {"overlay.show_overlays": False, "show_gizmo": False}))
        match prepared.tag:
            case "ok":
                region, state = prepared.ok
                return state.applied(located.area).bind(lambda prior: restored(region, prior))
            case "error":
                return Error(prepared.error)


# --- [OPERATIONS] -----------------------------------------------------------------------


def unpacked(read: gpu.types.Buffer) -> npt.NDArray[np.float32]:
    """Converts native bottom-first pixels to a top-first float32 plane."""
    pixels = np.asarray(read)
    scaled = np.divide(pixels, np.iinfo(pixels.dtype).max, dtype=np.float32) if np.issubdtype(pixels.dtype, np.integer) else pixels
    return np.ascontiguousarray(scaled[::-1], dtype=np.float32)


def captured(rect: Rect) -> BlenderResult[npt.NDArray[np.float32]]:
    """Reads float RGBA from the active framebuffer inside a draw callback."""
    try:
        read = gpu.state.active_framebuffer_get().read_color(round(rect.x), round(rect.y), round(rect.width), round(rect.height), 4, 0, "FLOAT")
    except ValueError as error:
        return Error((BlenderEditFault(request="gpu.types.GPUFrameBuffer.read_color", error=error),))
    return Ok(unpacked(read))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Offscreen", "captured", "unpacked"]
