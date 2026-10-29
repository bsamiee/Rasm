"""Frame region handles in the image editor: one host cage over a region quad, with corner handles and an outline over a quad edited corner by corner."""

from collections.abc import Callable
from functools import partial
from typing import Final, Literal

import bpy
from expression import Nothing, Ok, Option, Some
from mathutils import Matrix, Vector
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.paths import root_in
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.drawing.canvas import Rect
from blender.bpy.ui.drawing.handlers import Refresh
from blender.bpy.ui.images.overlays import Edges, overlaid
from blender.bpy.ui.images.views import framed, RESULT
from blender.bpy.ui.surfaces.gizmos import GizmoPart, Gizmos, TargetHandler
from blender.imaging.geometry.frames import Quad, Region
from blender.records.faults import Fault, Outcome

# --- [CONSTANTS] ------------------------------------------------------------------------

HANDLE: Final = 0.2

# --- [MODELS] ---------------------------------------------------------------------------


class FrameRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Quad over images `shown` accepts in frame fractions, read and written through its owner and edited through one host cage.

    The cage moves, turns, and scales the quad as `transform` allows, Shift scaling uniformly and Escape restoring the press value.
    `corners` adds the quad outline and one move handle per corner, ahead of the cage so a press on a corner takes the corner, and every edit stays inside `bounds`, a pure move shifting the whole quad.
    """

    key: Key
    read: Callable[[bpy.types.Context], Outcome[Option[Quad]]]
    write: Callable[[bpy.types.Context, Quad], Outcome[None]]
    transform: frozenset[Literal["TRANSLATE", "ROTATE", "SCALE", "SCALE_UNIFORM"]] = frozenset({"TRANSLATE", "SCALE"})
    corners: bool = False
    bounds: Option[Region] = Nothing
    shown: Setting = RESULT
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the handle group, then the quad outline when corners are edited."""
        cage = GizmoPart(
            kind="GIZMO_GT_cage_2d", place=self.origin, bind=TargetHandler[tuple[float, ...]](get=self.caging, set=self.caged), properties={"transform": self.transform, "draw_options": frozenset()}
        )
        corners = tuple(
            GizmoPart(
                kind="GIZMO_GT_move_3d",
                place=self.origin,
                bind=TargetHandler[tuple[float, ...]](get=partial(self.corner, index), set=partial(self.cornered, index)),
                properties={"scale_basis": HANDLE, "draw_style": "CROSS_2D"},
            )
            for index in range(len(Quad().corners) if self.corners else 0)
        )
        group = Gizmos(key=self.key, space="IMAGE_EDITOR", parts=(*corners, cage), condition=Some(self.shown))
        outline = overlaid(Edges(quad=self.read, role=lambda ui: ui.gizmo_primary), self.shown, self.refreshes)
        return Ok((group, *((outline,) if self.corners else ())))

    # --- [READING]

    def held(self, context: bpy.types.Context) -> Outcome[Option[tuple[Quad, Rect]]]:
        """Reads the quad with the frame of the region drawing it."""
        return self.read(context).map(lambda found: found.bind(lambda quad: root_in(bpy.types.Region, context).map(lambda region: (quad, framed(region)))))

    def origin(self, context: bpy.types.Context) -> Outcome[Option[Matrix]]:
        """Places a handle at the frame origin while the owner holds a quad."""
        return self.held(context).map(lambda found: found.map(lambda pair: Matrix.Translation((pair[1].x, pair[1].y, 0.0))))

    def caging(self, context: bpy.types.Context) -> tuple[float, ...]:
        """Returns the cage's frame of the quad column by column, the order of the host's float matrix target, identity while the handle is unplaced, its placement holding any read fault."""
        frame = self.held(context).to_option().bind(lambda found: found).map(lambda pair: _frame(_pixels(*pair))).default_with(lambda: Matrix.Identity(4))
        return tuple(np.asarray(frame).ravel(order="F").tolist())

    def corner(self, index: int, context: bpy.types.Context) -> tuple[float, ...]:
        """Returns the corner's offset from the frame origin in region pixels, zero while the handle is unplaced."""
        return self.held(context).to_option().bind(lambda found: found).map(lambda pair: (*_pixels(*pair)[index], 0.0)).default_value((0.0, 0.0, 0.0))

    # --- [WRITING]

    def caged(self, context: bpy.types.Context, values: tuple[float, ...]) -> Outcome[None]:
        """Writes the quad the cage's change of frame carries, a pure move shifting it whole inside the bounds."""

        def carried(quad: Quad, frame: Rect) -> Outcome[None]:
            pixels = _pixels(quad, frame)
            change = Matrix(np.reshape(values, (4, 4)).tolist()).transposed() @ _frame(pixels).inverted_safe()
            moved = np.asarray([(change @ Vector((x, y, 0.0))).to_2d() for x, y in pixels]) / (frame.width, frame.height)
            return self.write(context, Quad.of(self.bounded(moved, shifted=np.allclose(np.asarray(change.to_2x2()), np.eye(2)))))

        return self.pressed(context).bind(lambda pair: carried(*pair))

    def cornered(self, index: int, context: bpy.types.Context, values: tuple[float, ...]) -> Outcome[None]:
        """Writes the quad with one corner at the handle's offset."""

        def carried(quad: Quad, frame: Rect) -> Outcome[None]:
            fractions = quad.samples[:, :2].copy()
            fractions[index] = np.asarray(values[:2]) / (frame.width, frame.height)
            return self.write(context, Quad.of(self.bounded(fractions, shifted=False)))

        return self.pressed(context).bind(lambda pair: carried(*pair))

    def pressed(self, context: bpy.types.Context) -> Outcome[tuple[Quad, Rect]]:
        """Reads the quad and frame under a handle, absent fault while the owner holds none."""
        return self.held(context).bind(lambda found: found.to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(context.area), member=self.key)))

    def bounded(self, fractions: npt.NDArray[np.float64], *, shifted: bool) -> npt.NDArray[np.float64]:
        """Keeps corners inside the bounds, a shifted quad moving whole before any corner is clipped."""
        return self.bounds.map(partial(_clipped, fractions, shifted=shifted)).default_value(fractions)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _clipped(fractions: npt.NDArray[np.float64], region: Region, *, shifted: bool) -> npt.NDArray[np.float64]:
    """Clips corners into the region, a shifted quad first moving whole as far as the region lets it."""
    floor, ceiling = np.array((region.left, region.bottom)), np.array((region.right, region.top))
    lower = fractions.min(axis=0)
    offset = np.clip(lower, floor, np.maximum(ceiling - np.ptp(fractions, axis=0), floor)) - lower if shifted else 0.0
    return np.clip(fractions + offset, floor, ceiling)


def _pixels(quad: Quad, frame: Rect) -> npt.NDArray[np.float64]:
    """Returns quad corners in pixels from the frame origin."""
    return quad.samples[:, :2] * (frame.width, frame.height)


def _frame(pixels: npt.NDArray[np.float64]) -> Matrix:
    """Returns the frame the cage edits: the corner mean with the mean edge across and the mean edge up, the exact rectangle of an aligned quad."""
    top_left, top_right, bottom_right, bottom_left = pixels
    (across_x, across_y), (up_x, up_y) = (top_right - top_left + bottom_right - bottom_left) / 2.0, (top_left - bottom_left + top_right - bottom_right) / 2.0
    center_x, center_y = pixels.mean(axis=0)
    return Matrix(((across_x, up_x, 0.0, center_x), (across_y, up_y, 0.0, center_y), (0.0, 0.0, 1.0, 0.0), (0.0, 0.0, 0.0, 1.0)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["HANDLE", "FrameRegion"]
