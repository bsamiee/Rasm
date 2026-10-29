"""Image editor rows: overlays over Render Result, a pick command, the render view command, and frame region handles, with probes reading what they registered and the frame math they run."""

from typing import Final

import bpy
from expression import Ok, Option, Some
import numpy as np

from blender.bpy.commands.operators import Answer, Call, Done
from blender.bpy.extension.identity import Identity
from blender.bpy.ui.images.views import Edges, FrameRegion, Masks, overlaid, Picked, picking, RenderView, RESULT, Tint, viewing
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.surfaces.panels import Sidebar
from blender.imaging.geometry.frames import Quad, Region
from blender.records.faults import Outcome

from .commands import described, operation

# --- [SERVICES] -------------------------------------------------------------------------


class Framed:
    """Quad the frame region and its edge overlay read, and its cage writes."""

    __slots__ = ("quad",)

    def __init__(self) -> None:
        """Holds the unit quad."""
        self.quad = Quad()

    def read(self, _context: bpy.types.Context) -> Outcome[Option[Quad]]:
        """Reads the held quad."""
        return Ok(Some(self.quad))

    def written(self, _context: bpy.types.Context, quad: Quad) -> Outcome[None]:
        """Holds a written quad."""
        self.quad = quad
        return Ok(None)


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [BODIES]


def sampled(_call: Call[Picked], _region: Region) -> Outcome[Answer]:
    """Samples the picked frame region."""
    return Ok(Done())


def _tints(_metrics: Metrics) -> Outcome[Option[tuple[Tint, ...]]]:
    """Holds one coverage tint."""
    return Ok(Some((Tint(plane=np.ones((2, 2), np.float32), color=(1.0, 0.0, 0.0, 0.5)),)))


# --- [PROBES]


def picked(identity: Identity) -> dict[str, object]:
    """Reads the pick command's registered class."""
    return described(identity, PICK)


def viewed(identity: Identity) -> dict[str, object]:
    """Reads the render view command's registration and looks for an editor showing Render Result."""
    return {
        "command": bpy.types.Operator.bl_rna_get_subclass_py("FIXTURE_OT_show_render_view") is not None,
        "viewing": viewing(bpy.context).map(lambda at: at.area.type),
        "operation": callable(operation(identity, WINDOW.command)),
    }


def caged(_identity: Identity) -> dict[str, object]:
    """Reads the frame region group and its handles, its cage frame and edits with no region in context, and its bounds over a moved and a stretched quad."""
    bounds = Region.bounding(np.array(((0.0, 0.0), (1.0, 1.0))))
    held = FrameRegion(key="frame", read=FRAMED.read, write=FRAMED.written, corners=True, bounds=Some(bounds))
    moved = held.bounded(np.array(((0.8, 0.9), (1.2, 0.9), (1.2, 0.5), (0.8, 0.5))), shifted=True)
    stretched = held.bounded(np.array(((-0.2, 1.4), (1.2, 1.4), (1.2, 0.5), (-0.2, 0.5))), shifted=False)
    native = bpy.types.GizmoGroup.bl_rna_get_subclass_py("FIXTURE_GGT_frame")
    return {
        "group": native is not None and (native.bl_space_type, native.bl_region_type),
        "cage": FRAME.caging(bpy.context),
        "corner": FRAME.corner(0, bpy.context),
        "edit": FRAME.caged(bpy.context, tuple(np.identity(4).ravel())),
        "moved": np.round(moved, 6),
        "stretched": np.round(stretched, 6),
        "unbounded": FrameRegion(key="loose", read=FRAMED.read, write=FRAMED.written).bounded(np.array(((2.0, 2.0),)), shifted=True),
    }


# --- [RELEASES]


def released_images() -> dict[str, bool]:
    """Answers whether the pick command and frame region group outlive release."""
    return {"group": bpy.types.GizmoGroup.bl_rna_get_subclass_py("FIXTURE_GGT_frame") is not None, "pick": bpy.types.Operator.bl_rna_get_subclass_py("FIXTURE_OT_sampled") is not None}


# --- [COMPOSITION] ----------------------------------------------------------------------

PICK: Final = picking("EYEDROPPER", Picked, sampled)
WINDOW: Final = RenderView(at=Sidebar(space="IMAGE_EDITOR", category="Fixture"))
FRAMED: Final = Framed()
FRAME: Final = FrameRegion(key="frame", read=FRAMED.read, write=FRAMED.written, corners=True)

rows: Final = (overlaid(Masks(source=_tints), RESULT, ()), overlaid(Edges(quad=FRAMED.read, role=lambda ui: ui.gizmo_primary), RESULT, ()), PICK, WINDOW, FRAME)
PROBES: Final = (picked, viewed, caged)
RELEASED: Final = (released_images,)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "rows"]
