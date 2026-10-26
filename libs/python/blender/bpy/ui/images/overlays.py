"""Overlays drawn over the image editor's displayed frame: coverage tints and quad outlines."""

from collections.abc import Callable, Iterable

import bpy
from expression import Nothing, Ok, Option, Some
import gpu
from gpu_extras.batch import batch_for_shader
import mathutils
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import root_in
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.ui.drawing.canvas import Canvas
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Part, Refresh
from blender.bpy.ui.images.views import framed, image_type
from blender.bpy.ui.resources.textures import uploaded
from blender.bpy.ui.resources.theme import Color, color, Metrics
from blender.bpy.ui.screens.areas import AreaQuery
from blender.imaging.geometry.frames import Quad
from blender.records.faults import accumulated, Outcome

# --- [MODELS] ---------------------------------------------------------------------------


class Tint(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Top-first frame coverage plane with its tint color."""

    plane: npt.NDArray[np.float32]
    color: Color


class Masks(msgspec.Struct, frozen=True, kw_only=True):
    """Part uploading coverage tints per refresh and drawing them over the frame."""

    source: Callable[[Metrics], Outcome[Option[tuple[Tint, ...]]]]

    def read(self, metrics: Metrics) -> Outcome[Option[tuple[gpu.types.GPUTexture, ...]]]:
        """Uploads each tint as its color with alpha times coverage."""
        return self.source(metrics).bind(
            lambda found: found.map(
                lambda tints: accumulated(
                    uploaded(np.concatenate((np.broadcast_to(np.asarray(tint.color[:3], np.float32), (*tint.plane.shape, 3)), (tint.plane * tint.color[3])[..., None]), axis=-1)) for tint in tints
                ).map(Some)
            ).default_value(Ok(Nothing))
        )

    @staticmethod
    def drawn(canvas: Canvas, state: tuple[gpu.types.GPUTexture, ...], /) -> None:
        """Draws masks over the frame, nearest filtering past full zoom."""
        for space, texture, region in ((space, texture, region) for space in root_in(bpy.types.SpaceImageEditor, bpy.context).to_list() for texture in state for region in canvas.region.to_list()):
            texture.filter_mode(bool(np.asarray(space.zoom)[0] < 1.0))
            canvas.picture(texture, framed(region), gpu.shader.from_builtin("IMAGE_COLOR"), blend="ALPHA")


class Edges(msgspec.Struct, frozen=True, kw_only=True):
    """Part outlining a frame quad in a theme color."""

    quad: Callable[[bpy.types.Context], Outcome[Option[Quad]]]
    role: Callable[[bpy.types.ThemeUserInterface], mathutils.Color | bpy.types.bpy_prop_array | Iterable[float]]

    def read(self, _metrics: Metrics, /) -> Outcome[Option[npt.NDArray[np.float32]]]:
        """Reads the quad as a closed corner strip in frame fractions."""
        return self.quad(bpy.context).map(lambda found: found.map(lambda held: held.samples[(0, 1, 2, 3, 0), :2].astype(np.float32)))

    def drawn(self, canvas: Canvas, state: npt.NDArray[np.float32], /) -> None:
        """Draws the strip through the frame in the role's theme color."""
        lines = canvas.lines(1.0)
        lines.uniform_float("color", color(self.role(canvas.metrics.preferences.themes[0].user_interface)))
        for frame in canvas.region.map(framed).to_list():
            batch_for_shader(lines, "LINE_STRIP", {"pos": (state * (frame.width, frame.height) + (frame.x, frame.y)).tolist()}).draw(lines)


class FrameMask(msgspec.Struct, frozen=True, kw_only=True):
    """Row drawing coverage masks over images of `kinds`."""

    read: Callable[[Metrics], Outcome[Option[tuple[Tint, ...]]]]
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the mask overlay."""
        return Ok((overlaid(Masks(source=self.read), image_type(self.kinds), self.refreshes),))


# --- [OPERATIONS] -----------------------------------------------------------------------


def overlaid[S](part: Part[S], shown: Setting, refreshes: tuple[Refresh, ...]) -> Overlay[S]:
    """Places a part over the image editor while `shown` holds."""
    return Overlay(drawing=Drawing(area=AreaQuery(space="IMAGE_EDITOR", ui_type=Some("IMAGE_EDITOR"), shows=Some(shown)), part=part, refreshes=refreshes), space=bpy.types.SpaceImageEditor)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Edges", "FrameMask", "Masks", "Tint", "overlaid"]
