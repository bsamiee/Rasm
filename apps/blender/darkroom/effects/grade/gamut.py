"""ACES gamut compression of scene-linear excursions toward a colorspace's boundary."""

from collections.abc import Mapping

import bpy
from expression import Error, Ok, Option, Some
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Shell, Spaces
from blender.imaging.color.processors import ColorSpace
from blender.records.faults import Fault, Outcome, RecordReason

from ...chain.stages import Effect, Stage, transform

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
            return Error((Fault(reason=RecordReason.UNREADABLE, subject=GamutCompression.__name__, cause=repr(values)),))


def compressed(shell: Shell[GamutCompression]) -> Outcome[Expr]:
    """Binds each channel's limit and threshold and the power to the node form of OCIO's gamut compression."""
    cyan, magenta, yellow = ((shell.input(channel, "limit"), shell.input(channel, "threshold")) for channel in ("cyan", "magenta", "yellow"))
    return Ok(grades.gamut_compression(shell.color, limits=(cyan[0], magenta[0], yellow[0]), thresholds=(cyan[1], magenta[1], yellow[1]), power=shell.input("power")))


# --- [COMPOSITION] ----------------------------------------------------------------------

gamut_compression = CompositorEffect(
    record=GamutCompression,
    graph=compressed,
    alpha=Alpha.STRAIGHT,
    interface=Interface(
        inputs=(
            Socket(
                key="power",
                name="Power",
                kind=bpy.types.NodeSocketFloat,
                description="Curve sharpness between threshold and limit",
                members={"min_value": 1.0, "max_value": 65504.0, "default_value": 1.2},
            ),
        ),
        panels=tuple(
            InterfacePanel(
                key=channel,
                name=bpy.path.display_name(channel),
                description=description,
                inputs=tuple(
                    Socket(key=item_key(channel, field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
                    for field, text, members in (
                        ("limit", "Distance compressed onto the gamut boundary", {"min_value": 1.001, "max_value": 65504.0, "default_value": limit}),
                        ("threshold", "Largest unchanged distance", {"min_value": 0.0, "max_value": 0.9995, "default_value": threshold}),
                    )
                ),
            )
            for channel, description, limit, threshold in (
                ("cyan", "Red channel toward cyan", 1.147, 0.815),
                ("magenta", "Green channel toward magenta", 1.264, 0.803),
                ("yellow", "Blue channel toward yellow", 1.312, 0.880),
            )
        ),
    ),
    parts=(Spaces(space=lambda record: record.space),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["GamutCompression", "gamut_compression"]
