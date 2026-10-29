"""Tonal zone grade of OCIO's linear tone transform over blacks, shadows, midtones, highlights, and whites, with s-contrast."""

from collections.abc import Mapping
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Expr, vector
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.colors import AXES
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect
from blender.bpy.render.compositor.shells import Shell
from blender.records.faults import Fault, Outcome, RecordReason

from ...chain.stages import Effect, Stage, transform

# --- [CONSTANTS] ------------------------------------------------------------------------

_NEUTRAL: Final = PyOpenColorIO.GradingTone(PyOpenColorIO.GRADING_LIN)
_ZONES: Final = (
    ("blacks", 0.9, 0.0, _NEUTRAL.blacks, _NEUTRAL.blacks.width),
    ("shadows", 0.8, -1.0, _NEUTRAL.shadows, _NEUTRAL.shadows.start - _NEUTRAL.shadows.width),
    ("midtones", 0.9, 0.0, _NEUTRAL.midtones, _NEUTRAL.midtones.width),
    ("highlights", 0.8, 1.0, _NEUTRAL.highlights, _NEUTRAL.highlights.width - _NEUTRAL.highlights.start),
    ("whites", 0.9, 0.0, _NEUTRAL.whites, _NEUTRAL.whites.width),
)
_S_CONTRAST: Final = Socket(
    key="s_contrast", kind=bpy.types.NodeSocketFloat, description="S-curve about middle gray with rolled-off ends, neutral at 1", min_value=0.01, max_value=1.99, default_value=_NEUTRAL.scontrast
)
_PANELS: Final = tuple(
    InterfacePanel(
        key=key,
        name=bpy.path.display_name(key),
        description=f"Color, master adjustment, and placement in stops of the {key}",
        inputs=(
            Socket(
                key=item_key(key, "color"),
                name="Color",
                kind=bpy.types.NodeSocketColor,
                description="Channel color, white neutral, black and twice white spanning the adjustment range",
                default_value=(1.0, 1.0, 1.0, 1.0),
            ),
            Socket(
                key=item_key(key, "master"),
                name="Master",
                kind=bpy.types.NodeSocketFloat,
                description="All-channel adjustment, above 1 brighter",
                min_value=1.0 - reach,
                max_value=1.0 + reach,
                default_value=neutral.master,
            ),
            Socket(
                key=item_key(key, "start"),
                name="Start",
                kind=bpy.types.NodeSocketFloat,
                description="Boundary nearest gray in stops from 0.18, midtone center",
                min_value=-32.0,
                max_value=32.0,
                default_value=neutral.start,
            ),
            Socket(
                key=item_key(key, "width"), name="Width", kind=bpy.types.NodeSocketFloat, description="Extent in stops, start to pivot for shadows and highlights", min_value=0.01, default_value=width
            ),
        ),
    )
    for key, reach, _, neutral, width in _ZONES
)

# --- [MODELS] ---------------------------------------------------------------------------


class Zones(Effect, frozen=True, kw_only=True):
    """Grades five scene-linear tonal zones per channel and applies s-contrast."""

    stage = Stage.ZONES
    pointwise = True


# --- [OPERATIONS] -----------------------------------------------------------------------


@transform.register
def _(_record: Zones, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns OCIO's linear tonal zone operation, each zone's color spanning its adjustment range about neutral, shadows and highlights placed by pivot."""

    def zone(panel: InterfacePanel, reach: float, side: float) -> Option[PyOpenColorIO.GradingRGBMSW]:
        match tuple(values.get(socket.key) for socket in panel.inputs):
            case (float(red), float(green), float(blue), _), float(master), float(start), float(width):
                spread = 1.0 - reach
                return Some(PyOpenColorIO.GradingRGBMSW(spread + reach * red, spread + reach * green, spread + reach * blue, master, start, width if side == 0.0 else start + side * width))
            case _:
                return Nothing

    match (*(zone(panel, reach, side).to_optional() for panel, (_, reach, side, _, _) in zip(_PANELS, _ZONES, strict=True)), values.get(_S_CONTRAST.key)):
        case (
            PyOpenColorIO.GradingRGBMSW() as blacks,
            PyOpenColorIO.GradingRGBMSW() as shadows,
            PyOpenColorIO.GradingRGBMSW() as midtones,
            PyOpenColorIO.GradingRGBMSW() as highlights,
            PyOpenColorIO.GradingRGBMSW() as whites,
            float(contrast),
        ):
            tone = PyOpenColorIO.GradingTone(PyOpenColorIO.GRADING_LIN)
            tone.blacks, tone.shadows, tone.midtones, tone.highlights, tone.whites, tone.scontrast = blacks, shadows, midtones, highlights, whites, contrast
            return Ok(Some(PyOpenColorIO.GradingToneTransform(values=tone, style=PyOpenColorIO.GRADING_LIN)))
        case _:
            return Error(Fault.of(RecordReason.UNREADABLE, Zones.__name__, repr(values)))


def _zones(shell: Shell[Zones]) -> Outcome[Expr]:
    """Binds each zone's color over its adjustment range, master, and placement, and the s-contrast, to the node form of OCIO's tone transform."""

    def zone(panel: InterfacePanel, reach: float) -> grades.Zone:
        color, master, start, width = (shell.input(socket) for socket in panel.inputs)
        tint = vector("MULTIPLY_ADD", color, (reach,) * 3, (1.0 - reach,) * 3)
        red, green, blue = (tint[axis] for axis in AXES)
        return grades.Zone(rgb=(red, green, blue), master=master, start=start, width=width)

    blacks, shadows, midtones, highlights, whites = (zone(panel, reach) for panel, (_, reach, *_) in zip(_PANELS, _ZONES, strict=True))
    return grades.tone(shell.color, blacks=blacks, shadows=shadows, midtones=midtones, highlights=highlights, whites=whites, contrast=shell.input(_S_CONTRAST))


zones: Final = CompositorEffect(graph=_zones, record=Zones, mix=Blended(alpha=Alpha.STRAIGHT), interface=Interface(inputs=(_S_CONTRAST,), panels=_PANELS))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Zones", "zones"]
