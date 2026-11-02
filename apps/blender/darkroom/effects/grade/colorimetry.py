"""Scene-linear colorimetry ahead of grading: white balance between two white points, primaries remapped by channel mixing or colorspace conversion, and ACES gamut compression toward a colorspace's boundary."""

from functools import partial
from typing import Final

import bpy
from expression import Error, Ok, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.extension.identity import Identity
from blender.bpy.nodes.desired import MenuItem, MenuItems
from blender.bpy.nodes.graphs import Expr, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket, StockInput
from blender.bpy.render.color.spaces import tint_of
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.colors import space_item
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect
from blender.bpy.render.compositor.grades import matrixed
from blender.bpy.render.compositor.inputs import entered, GraphInputs
from blender.bpy.render.compositor.viewers import READ
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.colorimetry import WhitePoint
from blender.imaging.color.processors import ColorSpace
from blender.imaging.files.images import ImageFile
from blender.records.faults import Fault, Faults, Outcome, paired, Reason
from blender.records.fields import Case

# --- [CONSTANTS] ------------------------------------------------------------------------

_WHITES: Final = tuple(
    InterfacePanel.of(
        key,
        Interface(inputs=tuple(Socket(key=field.lower(), kind=StockInput(node=bpy.types.CompositorNodeColorBalance, input=f"{side} {field}")) for field in ("Temperature", "Tint"))),
        description=description,
    )
    for key, side, description in (("source", "Input", "Frame white"), ("target", "Output", "Output white"))
)
MIXER: Final = InterfacePanel.of(
    "mixer",
    Interface(
        inputs=tuple(
            Socket(key=channel, kind=bpy.types.NodeSocketVector, description=f"Red, green, and blue input weights summed by the {channel} output", default_value=weights)
            for channel, weights in (("red", (1.0, 0.0, 0.0)), ("green", (0.0, 1.0, 0.0)), ("blue", (0.0, 0.0, 1.0)))
        )
    ),
    description="Linear channel mixing with signed weights and unclamped output",
)
_CONVERSION: Final = MenuItem(name="Conversion", description="Colorspace interpretation and conversion through the OCIO config")
REMAP: Final = Socket(
    key="remap", kind=bpy.types.NodeSocketMenu, description="Channel mixing or colorspace interpretation", default_value=MIXER.item.name, items=Some(MenuItems(items=(MIXER.item, _CONVERSION)))
)
_HALF: Final = float(np.finfo(np.float16).max)
_POWER: Final = Socket(key="power", kind=bpy.types.NodeSocketFloat, description="Curve sharpness between threshold and limit", min_value=1.0, max_value=_HALF, default_value=1.2)
_CHANNELS: Final = tuple(
    InterfacePanel.of(
        channel,
        Interface(
            inputs=(
                Socket(key="limit", kind=bpy.types.NodeSocketFloat, description="Distance compressed onto the gamut boundary", min_value=1.001, max_value=_HALF, default_value=limit),
                Socket(key="threshold", kind=bpy.types.NodeSocketFloat, description="Largest unchanged distance", min_value=0.0, max_value=0.9995, default_value=threshold),
            )
        ),
        description=description,
    )
    for channel, description, limit, threshold in (
        ("cyan", "Red channel toward cyan", 1.147, 0.815),
        ("magenta", "Green channel toward magenta", 1.264, 0.803),
        ("yellow", "Blue channel toward yellow", 1.312, 0.880),
    )
)

# --- [MODELS] ---------------------------------------------------------------------------


class WhiteBalance(Case, frozen=True, kw_only=True):
    """Adapts the source white to the target white through Bradford cone response."""


class Primaries(Case, frozen=True, kw_only=True):
    """Remaps scene-linear channels before gamut compression, the conversion between the colorspaces values read as and convert to."""

    source: ColorSpace = PyOpenColorIO.ROLE_SCENE_LINEAR
    target: ColorSpace = PyOpenColorIO.ROLE_SCENE_LINEAR


class GamutCompression(Case, frozen=True, kw_only=True):
    """Compresses scene-linear excursions toward the gamut boundary of the target colorspace."""

    space: ColorSpace = PyOpenColorIO.ROLE_SCENE_LINEAR


# --- [OPERATIONS] -----------------------------------------------------------------------


def unread(entry: bpy.types.SceneCompositorEffect) -> Faults:
    """Absent fault of a frame entering an entry that holds nothing its effect reads."""
    return Fault.of(Reason.ABSENT, f"Region of frame entering {entry.name}", member="value its effect reads")


def finite_mean(config: PyOpenColorIO.Config, entry: bpy.types.SceneCompositorEffect, file: ImageFile) -> Outcome[npt.NDArray[np.float64]]:
    """Returns the mean of the entering frame's covered finite pixels, the `unread` fault without one."""
    return Samples.of(config, file.pixels).bind(lambda samples: Ok(samples.color[samples.finite].mean(axis=0, dtype=np.float64)) if samples.finite.any() else Error(unread(entry)))


def white_pick(identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile) -> Outcome[bool]:
    """Measure the source white from the stage input's covered mean."""
    config = PyOpenColorIO.GetCurrentConfig()
    temperature, tint = _WHITES[0].inputs
    return (
        finite_mean(config, entry, file)
        .bind(partial(WhitePoint.of, config))
        .bind(lambda white: white.to_result_with(partial(unread, entry)))
        .bind(lambda white: entered(identity, entry, {temperature.key: white.temperature, tint.key: tint_of(white)}))
    )


def _white_balance(shell: GraphInputs[WhiteBalance]) -> Outcome[Expr]:
    """Adapts straight scene-linear color between the white points through the Color Balance node's white point mode."""
    return Ok(
        Expr(
            kind=bpy.types.CompositorNodeColorBalance,
            inputs=(("Image", shell.color), ("Type", "White Point"), *((stock.input, shell.input(socket)) for panel in _WHITES for socket in panel.inputs for stock in socket.stock.to_list())),
            socket="Image",
            socket_type=bpy.types.NodeSocketColor,
        )
    )


def _primaries(shell: GraphInputs[Primaries]) -> Outcome[Expr]:
    """Switches at the remap menu between the matrix form of the mixer weights and a colorspace conversion of the record's spaces."""
    red, green, blue = map(shell.input, MIXER.inputs)
    return paired(space_item(shell.config, shell.record.source), space_item(shell.config, shell.record.target)).map(
        lambda named: shell.menu(
            REMAP,
            cases={
                MIXER.item: matrixed(shell.color, (red, green, blue), xyz()),
                _CONVERSION: Expr(
                    kind=bpy.types.CompositorNodeConvertColorSpace,
                    inputs=(("Image", shell.color),),
                    members={"from_color_space": named[0], "to_color_space": named[1]},
                    socket="Image",
                    socket_type=bpy.types.NodeSocketColor,
                ),
            },
        )
    )


def _gamut_compression(shell: GraphInputs[GamutCompression]) -> Outcome[Expr]:
    """Binds each channel's limit and threshold and the power to the node form of OCIO's gamut compression."""
    cyan, magenta, yellow = (tuple(map(shell.input, panel.inputs)) for panel in _CHANNELS)
    return Ok(grades.gamut_compression(shell.color, limits=(cyan[0], magenta[0], yellow[0]), thresholds=(cyan[1], magenta[1], yellow[1]), power=shell.input(_POWER)))


# --- [COMPOSITION] ----------------------------------------------------------------------

white_balance: Final = CompositorEffect(
    _white_balance, record=WhiteBalance, mix=Blended(alpha=Alpha.STRAIGHT), interface=Interface(panels=_WHITES), pointwise=True, registers=(READ.measuring(white_pick, icon=Some("EYEDROPPER")),)
)
primaries: Final = CompositorEffect(_primaries, record=Primaries, mix=Blended(alpha=Alpha.STRAIGHT), interface=Interface(inputs=(REMAP,), panels=(MIXER,)), pointwise=True)
gamut_compression: Final = CompositorEffect(
    _gamut_compression, record=GamutCompression, mix=Blended(alpha=Alpha.STRAIGHT), interface=Interface(inputs=(_POWER,), panels=_CHANNELS), pointwise=True, space=Some(lambda record: record.space)
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["MIXER", "REMAP", "GamutCompression", "Primaries", "WhiteBalance", "finite_mean", "gamut_compression", "primaries", "unread", "white_balance"]
