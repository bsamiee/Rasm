"""Frame region handles in the image editor: one cage over an aligned outline, corner and whole-quad handles over a free one."""

from collections.abc import Callable
from enum import auto, Enum
import math
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.modal import Gesture, Modal, Travel
from blender.bpy.commands.operators import Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import root_in
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.ui.drawing.canvas import Rect
from blender.bpy.ui.drawing.handlers import Refresh
from blender.bpy.ui.images.overlays import Edges, overlaid
from blender.bpy.ui.images.views import framed, image_type
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.surfaces.gizmos import Cage, Gizmos, Handler, MatrixValues, Move
from blender.imaging.geometry.homography import Quad
from blender.imaging.geometry.regions import Region
from blender.records.fields import heading

# --- [TYPES] ----------------------------------------------------------------------------

type Outline = Aligned | Free


class Transform(Enum):
    """Whole-quad handles beside the corner handles."""

    TRANSLATE = auto()
    ROTATE = auto()
    SCALE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

HANDLE: Final = 0.2

# --- [MODELS] ---------------------------------------------------------------------------


class Access(msgspec.Struct, frozen=True, kw_only=True):
    """Region quad read from its owner with the owner's writer."""

    quad: Quad
    write: Callable[[Quad], BlenderResult[None]]


class Aligned(msgspec.Struct, frozen=True, kw_only=True):
    """Axis-aligned outline, its aspect kept while locked and each edit clipped to `bounds`."""

    locked: bool = False
    bounds: Option[Region] = Nothing


class Free(msgspec.Struct, frozen=True, kw_only=True):
    """Four corners edited one at a time or translated, rotated, and scaled together."""


class FrameRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Region handles over images of `kinds`, named for `key`, each drag restored on Escape."""

    key: Key
    access: Callable[[bpy.types.Context], BlenderResult[Option[Access]]]
    shape: Outline = Aligned()
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the handle group, with each free handle's drag operator and the quad outline."""
        shown = image_type(self.kinds)
        match self.shape:
            case Aligned() as aligned:
                return Ok((Gizmos(key=self.key, title=heading(self.key), space="IMAGE_EDITOR", parts=(self.cage(identity, aligned),), condition=Some(shown)),))
            case Free():
                return Ok(self.moves(shown))

    def held(self, context: bpy.types.Context) -> BlenderResult[Option[tuple[Access, Rect]]]:
        """Reads region access with the frame of the region drawing it."""
        return self.access(context).map(lambda found: found.bind(lambda access: root_in(bpy.types.Region, context).map(lambda region: (access, framed(region)))))

    def pressed(self, context: bpy.types.Context) -> BlenderResult[tuple[Access, Rect]]:
        """Reads region access at a press, absent fault while the owner holds none."""
        return self.held(context).bind(lambda found: found.to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.area), key=self.key),)))

    def cage(self, identity: Identity, aligned: Aligned) -> Cage:
        """Returns a cage over the region rectangle in frame pixels."""

        def matrix(access: Access, frame: Rect) -> MatrixValues:
            held = Region.bounding(access.quad.samples)
            across, up = (held.right - held.left) * frame.width, (held.top - held.bottom) * frame.height
            return (across, 0.0, 0.0, 0.0, 0.0, up, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, (held.left + held.right) / 2.0 * frame.width, (held.bottom + held.top) / 2.0 * frame.height, 0.0, 1.0)

        def place(context: bpy.types.Context) -> BlenderResult[Option[Matrix]]:
            return self.held(context).map(lambda found: found.map(lambda pair: Matrix.Translation((pair[1].x, pair[1].y, 0.0))))

        def get(context: bpy.types.Context) -> MatrixValues:
            return (
                identity.reports
                .reported(self.held)(context)
                .bind(lambda found: found)
                .map(lambda pair: matrix(*pair))
                .default_with(lambda: msgspec.convert(np.eye(4).ravel().tolist(), type=MatrixValues))
            )

        def wrote(context: bpy.types.Context, values: MatrixValues) -> BlenderResult[None]:
            def written(access: Access, frame: Rect) -> BlenderResult[None]:
                size = np.array((frame.width, frame.height))
                center, half = np.array((values[12], values[13])) / size, np.abs((values[0], values[5])) / 2.0 / size
                held = access.quad.samples[:, :2]
                moved = np.allclose(half * 2.0 * size, (held.max(axis=0) - held.min(axis=0)) * size, atol=0.5)

                def bounded(region: Region) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
                    floor, ceiling = np.array((region.left, region.bottom)), np.array((region.right, region.top))
                    kept = np.minimum(half, (ceiling - floor) / 2.0)
                    shifted = np.clip(center, floor + kept, ceiling - kept)
                    return (shifted - kept, shifted + kept) if moved else (np.clip(center - half, floor, ceiling), np.clip(center + half, floor, ceiling))

                return access.write(Quad.of_region(Region.bounding(np.stack(aligned.bounds.map(bounded).default_value((center - half, center + half))))))

            return self.pressed(context).bind(lambda pair: written(*pair))

        return Cage(
            place=place,
            bind=Handler(get=get, set=wrote),
            dimensions=(1.0, 1.0),
            transform=frozenset({"TRANSLATE", "SCALE_UNIFORM"} if aligned.locked else {"TRANSLATE", "SCALE"}),
            draw_style="BOX",
            draw_options=frozenset({"CORNER_HANDLES"}),
        )

    def moves(self, shown: Setting) -> tuple[Registrable, ...]:
        """Returns each free handle's drag operator, the handle group, then the quad outline."""
        corners = len(Quad.__struct_fields__)

        def reach() -> float:
            metrics = Metrics.read()
            return 2.0 * HANDLE * metrics.scale * metrics.preferences.view.gizmo_size

        def handle(part: int | Transform, order: int) -> tuple[Operator[Empty, tuple[Access, Rect]], Move]:
            def point(quad: Quad, frame: Rect) -> npt.NDArray[np.float64]:
                samples = quad.samples[:, :2] * (frame.width, frame.height) + (frame.x, frame.y)
                match part:
                    case int(index):
                        return samples[index, :]
                    case Transform.TRANSLATE:
                        return samples.mean(axis=0)
                    case Transform.ROTATE:
                        return np.add(samples.mean(axis=0), (0.0, reach()))
                    case Transform.SCALE:
                        return np.add(samples.mean(axis=0), (reach(), 0.0))

            def place(context: bpy.types.Context) -> BlenderResult[Option[Matrix]]:
                return self.held(context).map(lambda found: found.map(lambda pair: Matrix.Translation((*point(pair[0].quad, pair[1]), 0.0))))

            def preview(_context: bpy.types.Context, state: tuple[Access, Rect], travel: Travel) -> BlenderResult[Option[str]]:
                held, frame = state
                pixels = held.quad.samples[:, :2] * (frame.width, frame.height)
                delta = np.array((travel.dx, travel.dy), dtype=np.float64) * (0.1 if travel.precise else 1.0)
                center, arm = pixels.mean(axis=0), reach()
                match part:
                    case Transform.TRANSLATE:
                        changed = pixels + delta
                    case Transform.ROTATE:
                        angle = math.atan2(-delta[0], arm + delta[1])
                        cosine, sine = math.cos(angle), math.sin(angle)
                        changed = (pixels - center) @ np.array(((cosine, sine), (-sine, cosine))) + center
                    case Transform.SCALE:
                        changed = (pixels - center) * math.hypot(arm + delta[0], delta[1]) / arm + center
                    case int(index):
                        changed = pixels + np.where((np.arange(corners) == index)[:, None], delta, 0.0)
                return held.write(Quad.of(changed / (frame.width, frame.height))).map(lambda _: Nothing)

            command = Operator[Empty, tuple[Access, Rect]](
                key=f"{self.key}_{order}",
                title=heading(self.key),
                description="Drag region handle, Shift for tenth speed, Escape restores",
                form=Empty,
                run=lambda _call: Ok(Done()),
                invoke=Some(Modal(gesture=Gesture.DRAG, begin=lambda context, _travel: self.pressed(context), preview=preview, restore=lambda _context, state: state[0].write(state[0].quad))),
                condition=Some(shown),
                internal=True,
            )
            return command, Move(place=place, bind=command, scale_basis=HANDLE, draw_style="CROSS_2D" if isinstance(part, int) else "RING_2D")

        pairs = tuple(handle(part, order) for order, part in enumerate((*range(corners), *Transform)))
        moves = tuple(move for _, move in pairs)
        return (
            *(command for command, _ in pairs),
            Gizmos(key=self.key, title=heading(self.key), space="IMAGE_EDITOR", parts=moves, condition=Some(shown)),
            overlaid(Edges(quad=lambda context: self.access(context).map(lambda found: found.map(lambda access: access.quad)), role=moves[0].role), shown, self.refreshes),
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["HANDLE", "Access", "Aligned", "FrameRegion", "Free", "Outline", "Transform"]
