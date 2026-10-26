"""Output dither: triangular blue-noise offsets at the output code step, or a Floyd-Steinberg diffused PNG copy beside each written float frame."""

from pathlib import Path
from typing import Annotated, Self

import bpy
from expression import Ok, Result, Some
import msgspec
import numpy as np
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderFaults, BlenderResult
from blender.bpy.nodes.graphs import End, Expr, Generated, math, mixed, switched, vector
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.compositor.colors import space_item
from blender.bpy.render.compositor.coordinates import gridded
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Derives, Enabled, Holds, Inspects, precise, Registers, Requests, Shell
from blender.bpy.render.outputs.finishing import Finish, stamped, Written
from blender.bpy.render.outputs.formats import Kind
from blender.bpy.settings.attachments import Attachment
from blender.imaging.color.processors import Domain
from blender.imaging.files.images import Depth, Diffused, ImageFile, IntegerDepth, read_file, Storage, unassociated
from blender.imaging.sampling.tiles import ranks
from blender.records.faults import accumulated, paired
from blender.records.fields import Case
from blender.records.identity import content_hash
from blender.rendering.outputs.naming import Output

from ...chain.stages import Effect, Stage
from ..formation.display import displayed_frame

# --- [MODELS] ---------------------------------------------------------------------------


class BlueNoise(Case, frozen=True, kw_only=True):
    """Triangular blue-noise dither at one output code step with exact black and white endpoints."""


class Dither(Effect, frozen=True, kw_only=True):
    """Blue-noise dither of the output codes, or whole-frame error diffusion of a PNG copy."""

    stage = Stage.QUANTIZATION
    pointwise = False
    method: Annotated[BlueNoise | Diffused, msgspec.Meta(description="Blue-noise dither or whole-frame error diffusion")] = BlueNoise()


class CodeGrid(msgspec.Struct, frozen=True, kw_only=True):
    """Output code step and whether the writer rounds straight color, read from the scene's output format."""

    step: float
    straight: bool

    @classmethod
    def of(cls, scene: bpy.types.Scene) -> Self:
        """Reads code step and alpha form from the output format."""
        settings = scene.render.image_settings
        return cls(
            step=0.0 if settings.has_linear_colorspace else 1.0 / (2 ** int(settings.color_depth) - 1),
            straight=settings.color_mode == "RGBA" and not (settings.file_format == "TIFF" and settings.color_depth == "16"),
        )


class DiffusedCopy(msgspec.Struct, frozen=True, kw_only=True):
    """PNG copy written beside each float frame a diffusing dither entry renders."""

    depth: Annotated[IntegerDepth, msgspec.Meta(title="Copy Depth", description="Channel depth of diffused PNG copy")] = 8


# --- [OPERATIONS] -----------------------------------------------------------------------


def dithered(shell: Shell[Dither]) -> BlenderResult[Expr]:
    """Adds triangular blue-noise offsets of one code step as the last display step, passing the source under diffusion.

    Args:
        shell: Associated display image, method menu, code grid, and config.

    Returns:
        Selected image, or the data space fault.
    """
    step_field, straight_field = CodeGrid.__struct_fields__
    (method,) = Dither.__struct_fields__
    source, step, straight = shell.color, shell.input(step_field), shell.input(straight_field)

    def built(data: str) -> Expr:
        generated = Generated(
            ownership=TILE,
            content=content_hash(BlueNoise, providers=(OpenImageIO,)),
            name="Blue Noise Tile",
            plane=lambda: ranks(OpenImageIO.ImageBufAlgo.bluenoise_image()).map(Some),
            tag=Tag(space=Some(data)),
        )
        tile = Expr(kind=bpy.types.CompositorNodeImage, content=Some(generated), socket="Image", shape="RGBA")
        dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", tile),), socket="Dimensions", shape="VECTOR")
        uv = vector("ADD", vector("DIVIDE", gridded(source).point, dimensions), (0.0, 0.0, 1.0))
        sampled = Expr(
            kind=bpy.types.CompositorNodeMapUV, inputs=(("Image", tile), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", "Repeat"), ("Extension Y", "Repeat")), socket="Image", shape="RGBA"
        )
        rank = sampled["Red"]
        rectangular, signed = rank - 0.5, math("MULTIPLY_ADD", rank, 2.0, -1.0)
        triangular_rank = math("SIGN", signed) * (1.0 - math("SQRT", 1.0 - abs(signed)))
        weight = switched(straight, 1.0, source["Alpha"])
        value = vector("DIVIDE", source, weight)
        nearest = vector("MINIMUM", value, vector("SUBTRACT", (1.0, 1.0, 1.0), value))
        ends = vector("MINIMUM", vector("MAXIMUM", vector("DIVIDE", nearest, step * 0.5), (0.0, 0.0, 0.0)), (1.0, 1.0, 1.0))
        triangular = vector("MULTIPLY_ADD", ends, triangular_rank - rectangular, rectangular).named("triangular")
        added = mixed(1.0, source, vector("SCALE", triangular, step * weight), blend="ADD")
        return shell.menu(method, cases={BlueNoise.title: added, Diffused.title: source}, kind="RGBA")

    return space_item(shell.config, PyOpenColorIO.ROLE_DATA).map(built)


def copied(scene: bpy.types.Scene, file: Written, identity: Identity) -> BlenderResult[None]:
    """Writes a diffused PNG copy of a written float frame as the chain displays it, when a render-enabled dither entry diffuses.

    Args:
        scene: Scene holding the dither entries.
        file: Finished single-layer EXR and its display window.
        identity: Extension owning entries, copy settings, and software stamp.

    Returns:
        None, or entry, attachment, read, formation, and write faults.
    """

    def write(settings: DiffusedCopy, config: PyOpenColorIO.Config) -> BlenderResult[None]:
        target = str(Path(file.path).with_suffix(".png")) if scene.render.use_file_extension else f"{file.path}.png"

        def displayed(source: ImageFile[np.float32]) -> BlenderResult[None]:
            plane = file.display.map(lambda window: source.pixels[window.ybegin : window.yend, window.xbegin : window.xend]).default_value(source.pixels)
            return (
                displayed_frame(identity, scene, np.repeat(plane, 3, axis=-1) if plane.shape[-1] == 1 else plane, source.colorspace, config)
                .bind(
                    lambda shown: ImageFile(pixels=Diffused().coded(unassociated(shown.pixels), settings.depth), colorspace=shown.colorspace, header=source.header).write(
                        target, Storage(), stamped(identity), config
                    )
                )
                .map(lambda _: None)
            )

        return Result[ImageFile[np.float32], BlenderFaults].bind(read_file(file.path, config), displayed)

    diffusing = accumulated(dither.read(identity, entry) for entry in dither.entries(identity, scene) if Enabled.RENDER.descriptor.read(entry) is True).map(
        lambda records: scene.render.use_compositing and any(isinstance(record.record.method, Diffused) for record in records)
    )
    return diffusing.bind(lambda diffuses: paired(COPY.bound(identity).bind(lambda attached: attached.read(scene)), config_of()).bind(lambda pair: write(*pair)) if diffuses else Ok(None))


# --- [COMPOSITION] ----------------------------------------------------------------------

TILE = Ownership(kind=bpy.types.Image, key="dither_tile", saved=Saved.USED)
COPY = Attachment(key="diffusion", form=DiffusedCopy, owner=bpy.types.Scene)
dither: CompositorEffect[Dither] = CompositorEffect(
    record=Dither,
    graph=dithered,
    domain=Domain.DISPLAY,
    alpha=Alpha.RESULT,
    parts=(
        Requests[Dither](request=precise),
        Derives[Dither, msgspec.Struct](
            record=Some(CodeGrid),
            of=lambda _identity, scene, _record: Ok(CodeGrid.of(scene)),
            reads=tuple((bpy.types.ImageFormatSettings, bpy.types.ImageFormatSettings.bl_rna.properties[name]) for name in ("media_type", "file_format", "color_depth", "color_mode")),
        ),
        Holds(scene={"render.dither_intensity": 0.0}),
        Inspects(key="dither_offset", end=End(node="triangular", socket="Vector"), description="Show dither offset in output code steps"),
        Registers(rows=(TILE, COPY)),
    ),
)
diffuse = Finish(run=copied, outputs=frozenset({Output.PREVIEW}), kinds=frozenset({Kind.EXR}), depths=frozenset({Depth.HALF, Depth.FLOAT}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COPY", "TILE", "BlueNoise", "CodeGrid", "DiffusedCopy", "Dither", "diffuse", "dither"]
