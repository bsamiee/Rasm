"""ACES gamut compression of scene-linear excursions toward a colorspace's boundary."""

from collections.abc import Mapping
from typing import Final

import bpy
from expression import Error, Ok, Option, Some
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.desired import Interface, InterfacePanel, item_key, Socket
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect
from blender.bpy.render.compositor.inputs import Shell
from blender.imaging.color.processors import ColorSpace
from blender.records.faults import Fault, Outcome, Reason

from ...chain.stages import Effect, Stage, transform

# --- [CONSTANTS] ------------------------------------------------------------------------

_POWER: Final = Socket(key="power", kind=bpy.types.NodeSocketFloat, description="Curve sharpness between threshold and limit", min_value=1.0, max_value=65504.0, default_value=1.2)
_CHANNELS: Final = tuple(
    InterfacePanel(
        key=channel,
        name=bpy.path.display_name(channel),
        description=description,
        inputs=(
            Socket(
                key=item_key(channel, "limit"),
                name="Limit",
                kind=bpy.types.NodeSocketFloat,
                description="Distance compressed onto the gamut boundary",
                min_value=1.001,
                max_value=65504.0,
                default_value=limit,
            ),
            Socket(
                key=item_key(channel, "threshold"), name="Threshold", kind=bpy.types.NodeSocketFloat, description="Largest unchanged distance", min_value=0.0, max_value=0.9995, default_value=threshold
            ),
        ),
    )
    for channel, description, limit, threshold in (
        ("cyan", "Red channel toward cyan", 1.147, 0.815),
        ("magenta", "Green channel toward magenta", 1.264, 0.803),
        ("yellow", "Blue channel toward yellow", 1.312, 0.880),
    )
)

# --- [MODELS] ---------------------------------------------------------------------------


class GamutCompression(Effect, frozen=True, kw_only=True):
    """Compresses scene-linear excursions toward the gamut boundary of the target colorspace."""

    stage = Stage.GAMUT
    pointwise = True
    space: ColorSpace = PyOpenColorIO.ROLE_SCENE_LINEAR


# --- [OPERATIONS] -----------------------------------------------------------------------


@transform.register
def _(_record: GamutCompression, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns OCIO's ACES gamut compression with the channels' limits, then thresholds, then power."""
    match values:
        case {
            "cyan_limit": float(cyan_limit),
            "magenta_limit": float(magenta_limit),
            "yellow_limit": float(yellow_limit),
            "cyan_threshold": float(cyan_threshold),
            "magenta_threshold": float(magenta_threshold),
            "yellow_threshold": float(yellow_threshold),
            "power": float(power),
        }:
            params = [cyan_limit, magenta_limit, yellow_limit, cyan_threshold, magenta_threshold, yellow_threshold, power]
            return Ok(Some(PyOpenColorIO.FixedFunctionTransform(style=PyOpenColorIO.FIXED_FUNCTION_ACES_GAMUT_COMP_13, params=params)))
        case _:
            return Error(Fault.of(Reason.UNREADABLE, GamutCompression.__name__, repr(values)))


def _gamut_compression(shell: Shell[GamutCompression]) -> Outcome[Expr]:
    """Binds each channel's limit and threshold and the power to the node form of OCIO's gamut compression."""
    cyan, magenta, yellow = (tuple(shell.input(socket) for socket in panel.inputs) for panel in _CHANNELS)
    return Ok(grades.gamut_compression(shell.color, limits=(cyan[0], magenta[0], yellow[0]), thresholds=(cyan[1], magenta[1], yellow[1]), power=shell.input(_POWER)))


gamut_compression: Final = CompositorEffect(
    graph=_gamut_compression, record=GamutCompression, mix=Blended(alpha=Alpha.STRAIGHT), interface=Interface(inputs=(_POWER,), panels=_CHANNELS), space=Some(lambda record: record.space)
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["GamutCompression", "gamut_compression"]
