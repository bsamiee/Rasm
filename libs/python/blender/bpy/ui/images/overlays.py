"""Overlays drawn over the image editor's displayed frame: coverage tints and quad outlines in image fractions, and any part placed over images a condition accepts."""

from collections.abc import Callable
from typing import ClassVar

import bpy
from expression import Nothing, Ok, Option, Some
import gpu
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.rna.conditions import Setting
from blender.bpy.ui.drawing.canvas import Canvas, DrawType, Rect
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Part, Refresh
from blender.bpy.ui.images.views import editor
from blender.bpy.ui.resources.textures import uploaded
from blender.bpy.ui.resources.theme import Color, color, Metrics, Shade
from blender.imaging.geometry.frames import Quad
from blender.records.faults import accumulated, Outcome

# --- [MODELS] ---------------------------------------------------------------------------


class Tint(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Top-first frame coverage plane with its tint color."""

    plane: npt.NDArray[np.float32]
    color: Color


class Masks(msgspec.Struct, frozen=True, kw_only=True):
    """Part uploading each tint's coverage once per refresh and drawing it over the frame in its color."""

    draw_type: ClassVar[DrawType] = "POST_VIEW"
    source: Callable[[Metrics], Outcome[Option[tuple[Tint, ...]]]]

    def read(self, metrics: Metrics) -> Outcome[Option[tuple[tuple[gpu.types.GPUTexture, Tint], ...]]]:
        """Uploads each coverage as associated white of that alpha, paired with its tint."""
        return self.source(metrics).bind(
            lambda found: found.map(
                lambda tints: accumulated(uploaded(np.repeat(tint.plane[..., None], 4, axis=-1)) for tint in tints).map(lambda textures: Some(tuple(zip(textures, tints, strict=True))))
            ).default_value(Ok(Nothing))
        )

    @staticmethod
    def drawn(canvas: Canvas, state: tuple[tuple[gpu.types.GPUTexture, Tint], ...], /) -> None:
        """Draws each coverage over the unit frame in its tint associated with its alpha, nearest filtering past full zoom."""
        match canvas.space:
            case bpy.types.SpaceImageEditor(zoom=zoom):
                across, _ = np.asarray(zoom)
                for texture, tint in state:
                    red, green, blue, alpha = tint.color
                    texture.filter_mode(bool(across < 1.0))
                    canvas.picture(texture, Rect(x=0.0, y=0.0, width=1.0, height=1.0), blend="ALPHA_PREMULT", color=(red * alpha, green * alpha, blue * alpha, alpha))
            case _:
                pass


class Edges(msgspec.Struct, frozen=True, kw_only=True):
    """Part outlining a frame quad in a theme color."""

    draw_type: ClassVar[DrawType] = "POST_VIEW"
    quad: Callable[[bpy.types.Context], Outcome[Option[Quad]]]
    role: Callable[[bpy.types.ThemeUserInterface], Shade]

    def read(self, _metrics: Metrics, /) -> Outcome[Option[gpu.types.GPUBatch]]:
        """Batches the quad as a closed corner strip in frame fractions."""
        return self.quad(bpy.context).map(
            lambda found: found.map(lambda held: Canvas.batched("POLYLINE_UNIFORM_COLOR", "LINE_STRIP", {"pos": np.ascontiguousarray(held.samples[(0, 1, 2, 3, 0), :2], np.float32)}))
        )

    def drawn(self, canvas: Canvas, state: gpu.types.GPUBatch, /) -> None:
        """Draws the strip in the role's theme color."""
        with canvas.lines(1.0) as lines:
            lines.uniform_float("color", color(self.role(canvas.metrics.preferences.themes[0].user_interface)))
            state.draw(lines)


# --- [OPERATIONS] -----------------------------------------------------------------------


def overlaid[S](part: Part[S], shown: Setting, refreshes: tuple[Refresh, ...]) -> Overlay[S]:
    """Places a part over the image editor while `shown` holds, in the draw phase the part states.

    Args:
        part: Part drawn over the editor.
        shown: Condition over the image editor space.
        refreshes: Sources invalidating the part's read.

    Returns:
        Overlay drawing over the image editor's main region.
    """
    return Overlay(drawing=Drawing(area=editor(shown), part=part, refreshes=refreshes))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Edges", "Masks", "Tint", "overlaid"]
