"""Native channel mixing and color-space interpretation selected by a union menu."""

import bpy
from expression import Ok

from rasm.blender.faults import BlenderResult
from rasm.blender.nodes.graphs import End, ended, Fragment, menu, node
from rasm.blender.render.color.spaces import space_item
from rasm.blender.render.effects.stack import CompositorEffect, Shell
from rasm.imaging.effects.grade.primaries import Conversion, Mixer, Primaries, Weights
from rasm.imaging.faults import paired

# --- [OPERATIONS] -----------------------------------------------------------------------


def remapped(shell: Shell[Primaries]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns both remap branches with an animatable native union menu.

    Args:
        shell: Native entry inputs, menu items, and retained conversion parameters.

    Returns:
        Native body and result endpoint, or accumulated space and graph faults.
    """
    (remap,) = Primaries.__struct_fields__
    entering = shell.source
    mixer = (
        node(
            "matrix",
            bpy.types.FunctionNodeCombineMatrix,
            {
                f"Column {column} Row {row}": shell.socket(remap, Mixer, output, source)
                for row, output in enumerate(Mixer.__struct_fields__, 1)
                for column, source in enumerate(Weights.__struct_fields__, 1)
            },
            {},
        )
        .map(ended("matrix", "Matrix"))
        .bind(lambda matrix: node("mixer", bpy.types.FunctionNodeTransformDirection, {"Direction": entering, "Transform": matrix}, {}))
        .map(ended("mixer", "Direction"))
    )

    def converted(kept: Conversion) -> BlenderResult[dict[str, str]]:
        return paired(space_item(shell.config, kept.source), space_item(shell.config, kept.target)).map(lambda spaces: dict(zip(("from_color_space", "to_color_space"), spaces)))

    converting = (
        shell
        .case(Conversion, remap)
        .map(converted)
        .default_value(Ok({}))
        .bind(lambda members: node("conversion", bpy.types.CompositorNodeConvertColorSpace, {"Image": entering}, members))
        .map(ended("conversion", "Image"))
    )
    return paired(mixer, converting).bind(lambda branches: menu("remap", "RGBA", shell.socket(remap), shell.items[remap,], dict(zip((Mixer.title, Conversion.title), branches))))


# --- [COMPOSITION] ----------------------------------------------------------------------

primaries = CompositorEffect(record=Primaries, graph=remapped, straight=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["primaries", "remapped"]
