"""Primary grades in OCIO's linear style: gain and offset wheels with contrast about gray, saturation with vibrance about working luminance, and tonal zones over blacks, shadows, midtones, highlights, and whites with s-contrast."""

from typing import Final

import bpy
from expression import Ok, Some
import PyOpenColorIO

from blender.bpy.nodes.graphs import Expr, math, switched, vector, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.colors import AXES
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect
from blender.bpy.render.compositor.inputs import GraphInputs
from blender.imaging.color.processors import luma
from blender.records.faults import Outcome
from blender.records.fields import Case

# --- [CONSTANTS] ------------------------------------------------------------------------

_GAIN: Final = InterfacePanel.of(
    "gain",
    Interface(
        inputs=(
            Socket(key="color", kind=bpy.types.NodeSocketColor, description="Channel multiplier color, white neutral", default_value=(1.0, 1.0, 1.0, 1.0)),
            Socket(key="master", kind=bpy.types.NodeSocketFloat, description="Gain of every channel in stops"),
        )
    ),
    description="Channel multiplier color and master gain in stops",
)
_OFFSET: Final = InterfacePanel.of(
    "offset",
    Interface(
        inputs=(
            Socket(key="color", kind=bpy.types.NodeSocketColor, description="Channel color, white neutral", default_value=(1.0, 1.0, 1.0, 1.0)),
            Socket(key="master", kind=bpy.types.NodeSocketFloat, description="Value added to every channel"),
        )
    ),
    description="Channel color less white and master value added to each channel",
)
_SLOPE: Final = Socket(key="slope", kind=bpy.types.NodeSocketFloat, description="Power about pivot, slope in stops", min_value=0.01, max_value=2.0, default_value=1.0)
_PIVOT: Final = Socket(key="pivot", kind=bpy.types.NodeSocketFloat, description="Pivot in stops from scene-linear gray", min_value=-32.0, max_value=32.0)
_CONTRAST: Final = InterfacePanel.of("contrast", Interface(inputs=(_SLOPE, _PIVOT)), description="Signed power about a gray pivot")
SLOPE: Final = _CONTRAST.socket(_SLOPE.key)
_SCALE: Final = Socket(key="scale", kind=bpy.types.NodeSocketFloat, description="Chroma scale about luminance", min_value=0.0, default_value=1.0)
_VIBRANCE: Final = Socket(key="vibrance", kind=bpy.types.NodeSocketFloat, description="Saturation gain weighted by pixel saturation", min_value=-1.0, max_value=1.0)
_NEUTRAL: Final = PyOpenColorIO.GradingTone(PyOpenColorIO.GRADING_LIN)
_ZONES: Final = (
    ("blacks", 0.9, _NEUTRAL.blacks, _NEUTRAL.blacks.width),
    ("shadows", 0.8, _NEUTRAL.shadows, _NEUTRAL.shadows.start - _NEUTRAL.shadows.width),
    ("midtones", 0.9, _NEUTRAL.midtones, _NEUTRAL.midtones.width),
    ("highlights", 0.8, _NEUTRAL.highlights, _NEUTRAL.highlights.width - _NEUTRAL.highlights.start),
    ("whites", 0.9, _NEUTRAL.whites, _NEUTRAL.whites.width),
)
_S_CONTRAST: Final = Socket(
    key="s_contrast", kind=bpy.types.NodeSocketFloat, description="S-curve about middle gray with rolled-off ends, neutral at 1", min_value=0.01, max_value=1.99, default_value=_NEUTRAL.scontrast
)
_ZONE_PANELS: Final = tuple(
    InterfacePanel.of(
        key,
        Interface(
            inputs=(
                Socket(
                    key="color", kind=bpy.types.NodeSocketColor, description="Channel color, white neutral, black and twice white spanning the adjustment range", default_value=(1.0, 1.0, 1.0, 1.0)
                ),
                Socket(
                    key="master", kind=bpy.types.NodeSocketFloat, description="All-channel adjustment, above 1 brighter", min_value=1.0 - reach, max_value=1.0 + reach, default_value=neutral.master
                ),
                Socket(
                    key="start", kind=bpy.types.NodeSocketFloat, description="Boundary nearest gray in stops from 0.18, midtone center", min_value=-32.0, max_value=32.0, default_value=neutral.start
                ),
                Socket(key="width", kind=bpy.types.NodeSocketFloat, description="Extent in stops, start to pivot for shadows and highlights", min_value=0.01, default_value=width),
            )
        ),
        description=f"Color, master adjustment, and placement in stops of the {key}",
    )
    for key, reach, neutral, width in _ZONES
)

# --- [MODELS] ---------------------------------------------------------------------------


class Primary(Case, frozen=True, kw_only=True):
    """Applies scene-linear gain after an additive offset, each a color wheel with master, then a signed power about a gray pivot."""


class Saturation(Case, frozen=True, kw_only=True):
    """Scales chroma about working-space luminance with saturation-dependent vibrance."""


class Zones(Case, frozen=True, kw_only=True):
    """Grades five scene-linear tonal zones per channel and applies s-contrast."""


# --- [OPERATIONS] -----------------------------------------------------------------------


def _primary(shell: GraphInputs[Primary]) -> Outcome[Expr]:
    """Binds the offset wheel to the primary form's offset, the gain wheel to its exposure in stops, and the slope to its contrast about the pivot."""
    tint, stops = map(shell.input, _GAIN.inputs)
    offset, added = map(shell.input, _OFFSET.inputs)
    slope, pivot = map(shell.input, _CONTRAST.inputs)
    return Ok(
        grades.primary(
            shell.color,
            offset=vector("ADD", vector("SUBTRACT", offset, (1.0, 1.0, 1.0)), added),
            exposure=xyz(*(math("LOGARITHM", tint[axis], 2.0) + stops for axis in AXES)),
            contrast=xyz(slope, slope, slope),
            pivot=Some(pivot),
        )
    )


def _saturation(shell: GraphInputs[Saturation]) -> Outcome[Expr]:
    """Scales chroma about configured luminance by the saturation, raised for muted pixels and lowered for vivid ones by the vibrance."""
    scale, vibrance = shell.input(_SCALE), shell.input(_VIBRANCE)
    color = shell.color
    red, green, blue = (color[axis] for axis in AXES)
    sigma = 1.0 - math("MAXIMUM", math("MINIMUM", math("MINIMUM", red, green), blue), 0.0) / math("MAXIMUM", math("MAXIMUM", red, green), blue)
    inverse = 1.0 - sigma
    factor = scale * math("MULTIPLY_ADD", vibrance, switched(vibrance > 0.0, sigma * sigma * 0.5, inverse * inverse), 1.0)

    def built(weights: tuple[float, ...]) -> Expr:
        luminance = vector("DOT_PRODUCT", color, weights)
        return vector("MULTIPLY_ADD", vector("SUBTRACT", color, luminance), factor, luminance)

    return luma(shell.config).map(lambda values: built(tuple(map(float, values))))


def _zones(shell: GraphInputs[Zones]) -> Outcome[Expr]:
    """Binds each zone's color over its adjustment range, master, and placement, and the s-contrast, to the node form of OCIO's tone transform."""

    def zone(panel: InterfacePanel, reach: float) -> grades.Zone:
        color, master, start, width = map(shell.input, panel.inputs)
        tint = vector("MULTIPLY_ADD", color, (reach,) * 3, (1.0 - reach,) * 3)
        red, green, blue = (tint[axis] for axis in AXES)
        return grades.Zone(rgb=(red, green, blue), master=master, start=start, width=width)

    blacks, shadows, midtones, highlights, whites = (zone(panel, reach) for panel, (_, reach, _, _) in zip(_ZONE_PANELS, _ZONES, strict=True))
    return grades.tone(shell.color, blacks=blacks, shadows=shadows, midtones=midtones, highlights=highlights, whites=whites, contrast=shell.input(_S_CONTRAST))


# --- [COMPOSITION] ----------------------------------------------------------------------

primary: Final = CompositorEffect(_primary, record=Primary, mix=Blended(alpha=Alpha.STRAIGHT), interface=Interface(panels=(_GAIN, _OFFSET, _CONTRAST)), pointwise=True)
saturation: Final = CompositorEffect(_saturation, record=Saturation, mix=Blended(alpha=Alpha.STRAIGHT), interface=Interface(inputs=(_SCALE, _VIBRANCE)), pointwise=True)
zones: Final = CompositorEffect(_zones, record=Zones, mix=Blended(alpha=Alpha.STRAIGHT), interface=Interface(inputs=(_S_CONTRAST,), panels=_ZONE_PANELS), pointwise=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SLOPE", "Primary", "Saturation", "Zones", "primary", "saturation", "zones"]
