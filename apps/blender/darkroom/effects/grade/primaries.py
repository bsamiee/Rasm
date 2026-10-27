"""Primaries remap before gamut compression: unclamped channel mixing or colorspace interpretation through OCIO."""

from collections.abc import Mapping

import bpy
from expression import Error, Ok, Option, Some
import numpy as np
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Expr, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor.colors import space_item
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Shell
from blender.bpy.render.compositor.grades import matrixed
from blender.imaging.color.processors import ColorSpace, matrix
from blender.records.faults import Fault, Outcome, paired, RecordReason

from ...chain.stages import Effect, Stage, transform

# --- [MODELS] ---------------------------------------------------------------------------


class Primaries(Effect, frozen=True, kw_only=True):
    """Remaps scene-linear channels before gamut compression, the conversion between the colorspaces values read as and convert to."""

    stage = Stage.PRIMARIES
    pointwise = True
    source: ColorSpace = PyOpenColorIO.ROLE_SCENE_LINEAR
    target: ColorSpace = PyOpenColorIO.ROLE_SCENE_LINEAR


# --- [OPERATIONS] -----------------------------------------------------------------------


@transform.register
def _(record: Primaries, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns the matrix operation of the mixer weight rows, alpha passed through, or the conversion from source to target."""
    match values:
        case {"remap": "Conversion"}:
            return Ok(Some(PyOpenColorIO.ColorSpaceTransform(src=record.source, dst=record.target)))
        case {"remap": "Mixer", "mixer_red": tuple(red), "mixer_green": tuple(green), "mixer_blue": tuple(blue)}:
            return Ok(Some(matrix(np.array((red, green, blue), dtype=np.float64))))
        case _:
            return Error(Fault.of(RecordReason.UNREADABLE, Primaries.__name__, repr(values)))


def remapped(shell: Shell[Primaries]) -> Outcome[Expr]:
    """Switches at the remap menu between the matrix form of the mixer weights and a colorspace conversion of the record's spaces."""
    mixer = matrixed(shell.color, (shell.input("mixer_red"), shell.input("mixer_green"), shell.input("mixer_blue")), xyz())
    return paired(space_item(shell.config, shell.record.source), space_item(shell.config, shell.record.target)).bind(
        lambda named: shell.menu(
            "remap",
            cases={
                "Mixer": mixer,
                "Conversion": Expr(
                    kind=bpy.types.CompositorNodeConvertColorSpace,
                    inputs=(("Image", shell.color),),
                    members=(("from_color_space", named[0]), ("to_color_space", named[1])),
                    socket="Image",
                    shape="RGBA",
                ),
            },
            kind="RGBA",
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

primaries = CompositorEffect(
    record=Primaries,
    graph=remapped,
    alpha=Alpha.STRAIGHT,
    interface=Interface(
        inputs=(
            Socket(
                key="remap",
                name="Remap",
                kind=bpy.types.NodeSocketMenu,
                description="Channel mixing or colorspace interpretation",
                members={"default_value": "Mixer"},
                items=Some(
                    MenuItems(
                        items=(
                            MenuItem(name="Mixer", description="Linear channel mixing with signed weights and unclamped output"),
                            MenuItem(name="Conversion", description="Colorspace interpretation and conversion through the OCIO config"),
                        )
                    )
                ),
            ),
        ),
        panels=(
            InterfacePanel(
                key="mixer",
                name="Mixer",
                description="Red, green, and blue input weights summed by each output channel",
                inputs=tuple(
                    Socket(
                        key=item_key("mixer", channel),
                        name=bpy.path.display_name(channel),
                        kind=bpy.types.NodeSocketVector,
                        description=f"Red, green, and blue input weights summed by the {channel} output",
                        members={"dimensions": 3, "default_value": weights},
                    )
                    for channel, weights in (("red", (1.0, 0.0, 0.0)), ("green", (0.0, 1.0, 0.0)), ("blue", (0.0, 0.0, 1.0)))
                ),
            ),
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Primaries", "primaries"]
