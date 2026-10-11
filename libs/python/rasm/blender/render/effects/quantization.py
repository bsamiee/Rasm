"""Output code grids and canonical blue-noise compositor quantization."""

from pathlib import Path
from typing import Annotated

import bpy
from expression import Ok, Result, Some
import msgspec
import numpy as np
import OpenImageIO
import PyOpenColorIO

from rasm.blender.data.ids.images import Tag
from rasm.blender.data.ids.ownership import Ownership, Saved
from rasm.blender.extension.identity import Identity
from rasm.blender.faults import BlenderFaults, BlenderImagingFault, BlenderResult
from rasm.blender.nodes.graphs import End, ended, Fragment, Generated, gridded, math, menu, mixed, node, switched, vector
from rasm.blender.render.color.luts import Enabled
from rasm.blender.render.color.spaces import space_item
from rasm.blender.render.effects.stack import CompositorEffect, Coverage, Derived, entries, formation, precise, Shell
from rasm.blender.render.effects.viewers import inspection
from rasm.blender.render.outputs.exports import config_of, displayed
from rasm.blender.render.outputs.finishing import Finish, stamped, Written
from rasm.blender.render.outputs.formats import Kind
from rasm.blender.settings.attachments import Attachment
from rasm.imaging.color.processors import unassociated
from rasm.imaging.effects.quantization import BlueNoise, Diffusion, Dither, IntegerDepth
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.files.images import Depth, ImageFile, Storage
from rasm.imaging.hashing import content_hash
from rasm.rendering.outputs.naming import Output

# --- [MODELS] ---------------------------------------------------------------------------


class CodeGrid(msgspec.Struct, frozen=True, kw_only=True):
    """Integer output spacing and whether its writer rounds straight color."""

    step: float
    straight: bool

    @classmethod
    def of(cls, scene: bpy.types.Scene) -> "CodeGrid":
        """Derives the scene output's code spacing and native alpha form."""
        settings = scene.render.image_settings
        return cls(
            step=0.0 if settings.has_linear_colorspace else 1.0 / (2 ** int(settings.color_depth) - 1),
            straight=settings.color_mode == "RGBA" and not (settings.file_format == "TIFF" and settings.color_depth == "16"),
        )


class Diffused(msgspec.Struct, frozen=True, kw_only=True):
    """Integer channel depth of the PNG beside each written float frame."""

    depth: Annotated[IntegerDepth, msgspec.Meta(title="Copy Depth", description="Integer channel depth of the error-diffused PNG copy")] = 8


# --- [OPERATIONS] -----------------------------------------------------------------------


def dithered(shell: Shell[Dither]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds the last display-stage offset, retaining the source under diffusion.

    Args:
        shell: Associated display image, method menu, output grid, and admitted config.

    Returns:
        The complete fragment and selected image end, or all independent builder
        faults. Tile acquisition retains its imaging failure at reconciliation.
    """
    source = shell.source
    step = shell.socket(CodeGrid.step.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    straight = shell.socket(CodeGrid.straight.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    method = Dither.method.__name__  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    tile = End(node="tile", socket="Image")
    dimensions = End(node="dimensions", socket="Dimensions")
    scaled = End(node="scaled", socket="Vector")
    uv = End(node="uv", socket="Vector")
    sampled = End(node="sampled", socket="Image")
    rank = End(node="rank", socket="Red")
    rectangular = End(node="rectangular")
    signed = End(node="signed")
    sign = End(node="sign")
    absolute = End(node="absolute")
    remainder = End(node="remainder")
    root = End(node="root")
    complement = End(node="complement")
    triangular_rank = End(node="triangular_rank")
    alpha = End(node="alpha", socket="Alpha")
    weight = End(node="weight", socket="Output")
    value = End(node="value", socket="Vector")
    inverse = End(node="inverse", socket="Vector")
    nearest = End(node="nearest", socket="Vector")
    half_step = End(node="half_step")
    distance = End(node="distance", socket="Vector")
    lower = End(node="lower", socket="Vector")
    ends = End(node="ends", socket="Vector")
    difference = End(node="difference")
    triangular = End(node="triangular", socket="Vector")
    spacing = End(node="spacing")
    offset = End(node="offset", socket="Vector")
    added = End(node="added", socket="Result_Color")
    selected = End(node="selected", socket="Output")
    return Fragment.joined(
        space_item(shell.config, PyOpenColorIO.ROLE_DATA).bind(
            lambda data: node(
                tile.node,
                bpy.types.CompositorNodeImage,
                {},
                {},
                Some(
                    Generated(
                        ownership=tiles,
                        content=content_hash(BlueNoise, providers=(OpenImageIO,)),
                        name="Blue Noise Tile",
                        plane=lambda: BlueNoise.tile().map(Some).map_error(lambda faults: (BlenderImagingFault(faults=faults),)),
                        tag=Tag(space=Some(data)),
                    )
                ),
            )
        ),
        node(dimensions.node, bpy.types.CompositorNodeImageInfo, {"Image": tile}, {}),
        gridded("grid", source).bind(lambda grid: vector(scaled.node, "DIVIDE", grid.point, dimensions)),
        vector(uv.node, "ADD", scaled, (0.0, 0.0, 1.0)),
        node(sampled.node, bpy.types.CompositorNodeMapUV, {"Image": tile, "UV": uv, "Interpolation": "Nearest", "Extension X": "Repeat", "Extension Y": "Repeat"}, {}),
        node(rank.node, bpy.types.CompositorNodeSeparateColor, {"Image": sampled}, {"mode": "RGB"}),
        math(rectangular.node, "SUBTRACT", rank, 0.5),
        math(signed.node, "MULTIPLY_ADD", rank, 2.0, -1.0),
        math(sign.node, "SIGN", signed),
        math(absolute.node, "ABSOLUTE", signed),
        math(remainder.node, "SUBTRACT", 1.0, absolute),
        math(root.node, "SQRT", remainder),
        math(complement.node, "SUBTRACT", 1.0, root),
        math(triangular_rank.node, "MULTIPLY", sign, complement),
        node(alpha.node, bpy.types.CompositorNodeSeparateColor, {"Image": source}, {"mode": "RGB"}),
        switched(weight.node, "FLOAT", straight, 1.0, alpha),
        vector(value.node, "DIVIDE", source, weight),
        vector(inverse.node, "SUBTRACT", (1.0,) * 3, value),
        vector(nearest.node, "MINIMUM", value, inverse),
        math(half_step.node, "MULTIPLY", step, 0.5),
        vector(distance.node, "DIVIDE", nearest, half_step),
        vector(lower.node, "MAXIMUM", distance, (0.0,) * 3),
        vector(ends.node, "MINIMUM", lower, (1.0,) * 3),
        math(difference.node, "SUBTRACT", triangular_rank, rectangular),
        vector(triangular.node, "MULTIPLY_ADD", ends, difference, rectangular),
        math(spacing.node, "MULTIPLY", step, weight),
        vector(offset.node, "SCALE", triangular, spacing),
        mixed(added.node, "RGBA", 1.0, source, offset, "ADD"),
        menu(selected.node, "RGBA", shell.socket(method), shell.items[method,], {BlueNoise.title: added, Diffusion.title: source}),
    ).map(ended(selected.node, selected.socket))


def copied(scene: bpy.types.Scene, file: Written, identity: Identity) -> BlenderResult[None]:
    """Forms and diffuses one written float frame into a tagged integer PNG copy.

    Args:
        scene: Scene whose render-enabled dither entry chooses diffusion.
        file: Completed single-layer EXR and its original display window.
        identity: Extension owning the entry, copy settings, and software stamp.

    Returns:
        Completion, or the entry, attachment, read, formation, and write faults.
    """

    def write(settings: Diffused, config: PyOpenColorIO.Config) -> BlenderResult[None]:
        target = str(Path(file.path).with_suffix(".png")) if scene.render.use_file_extension else f"{file.path}.png"

        def formed(source: ImageFile[np.float32]) -> BlenderResult[None]:
            plane = file.display.map(lambda window: source.pixels[window.y : window.y + window.height, window.x : window.x + window.width]).default_value(source.pixels)
            return (
                displayed(formation, identity, scene, np.repeat(plane, 3, axis=-1) if plane.shape[-1] == 1 else plane, source.colorspace, config)
                .bind(
                    lambda shown: (
                        ImageFile(pixels=Diffusion.quantize(unassociated(shown.pixels), settings.depth), colorspace=shown.colorspace, header=source.header)
                        .write(target, Storage(), stamped(identity), config)
                        .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
                    )
                )
                .map(lambda _: None)
            )

        return Result[ImageFile[np.float32], BlenderFaults].bind(ImageFile.read(file.path, config).map_error(lambda faults: (BlenderImagingFault(faults=faults),)), formed)

    return (
        accumulated(dither.read(identity, entry) for entry in entries(identity, scene, Dither) if bool(Enabled.RENDER.descriptor.read(entry))).bind(
            lambda records: (
                paired(attachment.bound(identity).bind(lambda attached: attached.read(scene)), config_of()).bind(lambda pair: write(*pair))
                if any(isinstance(record.effect.method, Diffusion) for record in records)
                else Ok(None)
            )
        )
        if scene.render.use_compositing
        else Ok(None)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

tiles = Ownership(kind=bpy.types.Image, key="dither_tile", saved=Saved.USED)
attachment = Attachment(key="diffusion", form=Diffused, owner=bpy.types.Scene)
dither: CompositorEffect[Dither] = CompositorEffect(
    record=Dither,
    graph=dithered,
    coverage=Coverage.RESULT,
    requested=Some(precise),
    derived=Some(
        Derived(
            record=CodeGrid,
            of=Some(lambda _identity, scene, _record: Ok(CodeGrid.of(scene))),
            reads=tuple((bpy.types.ImageFormatSettings, bpy.types.ImageFormatSettings.bl_rna.properties[name]) for name in ("media_type", "file_format", "color_depth", "color_mode")),
        )
    ),
    holds={"render.dither_intensity": 0.0},
    registrables=(
        tiles,
        attachment,
        inspection(lambda: dither, "dither_offset", "Inspect Dither Offset", "Shows the canonical offset in output code steps", lambda: End(node="triangular", socket="Vector")),
    ),
)
diffuse = Finish(run=copied, outputs=frozenset({Output.PREVIEW}), kinds=frozenset({Kind.EXR}), depths=frozenset({Depth.HALF, Depth.FLOAT}))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CodeGrid", "Diffused", "attachment", "copied", "diffuse", "dither", "dithered", "tiles"]
