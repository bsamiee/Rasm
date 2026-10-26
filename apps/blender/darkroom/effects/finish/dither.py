"""Output dither: triangular blue-noise offsets at the output code step, or a Floyd-Steinberg diffused PNG copy beside each written float frame."""

from collections.abc import Mapping
from pathlib import Path
from typing import Final

import bpy
from expression import Ok, Result, Some
import numpy as np
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.graphs import Expr, Generated, math, mixed, switched, vector
from blender.bpy.nodes.interface import Interface, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor.colors import space_item
from blender.bpy.render.compositor.coordinates import gridded
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Derives, Enabled, Holds, precise, Registers, Requests, Shell
from blender.bpy.render.compositor.inputs import entry_input
from blender.bpy.render.outputs.finishing import Finish, stamped, Written
from blender.bpy.render.outputs.formats import Kind
from blender.bpy.settings.attachments import Attachment
from blender.imaging.color.processors import Domain
from blender.imaging.files.images import Depth, Diffused, ImageFile, IntegerDepth, read_file, Storage, unassociated
from blender.imaging.sampling.tiles import ranks
from blender.records.faults import accumulated, Faults, Outcome
from blender.records.identity import content_hash
from blender.rendering.outputs.naming import Output

from ...chain.stages import Effect, Stage
from ..formation.display import displayed_frame

# --- [MODELS] ---------------------------------------------------------------------------


class Dither(Effect, frozen=True, kw_only=True):
    """Blue-noise dither of the output codes, or whole-frame error diffusion of a PNG copy."""

    stage = Stage.QUANTIZATION
    pointwise = False


class DiffusedCopy(bpy.types.PropertyGroup):
    """PNG copy written beside each float frame a diffusing dither entry renders."""

    depth: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Copy Depth",
        description="Channel depth of diffused PNG copy",
        items=tuple(
            (item.identifier, item.name, item.description)
            for item in Descriptor.declared(bpy.types.ImageFormatSettings, "color_depth", bpy.types.EnumProperty).prop.enum_items
            if item.identifier in {"8", "16"}
        ),
        default="8",
    )


# --- [OPERATIONS] -----------------------------------------------------------------------


def dithered(shell: Shell[Dither]) -> Outcome[Expr]:
    """Adds triangular blue-noise offsets of one code step as the last display step, passing the source under diffusion.

    Args:
        shell: Associated display image, method menu, code grid, and config.

    Returns:
        Selected image, or the data space and method faults.
    """
    source, step, straight = shell.color, shell.input("step"), shell.input("straight")

    def built(data: str) -> Outcome[Expr]:
        generated = Generated(
            ownership=TILE,
            content=content_hash(Dither, providers=(OpenImageIO,)),
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
        return shell.menu("method", cases={"Blue Noise": added, "Diffused": source}, kind="RGBA")

    return space_item(shell.config, PyOpenColorIO.ROLE_DATA).bind(built)


def code_grid(_identity: Identity, scene: bpy.types.Scene, _entry: bpy.types.SceneCompositorEffect, _record: Dither) -> Outcome[Mapping[Key, Value]]:
    """Reads the output code step and whether the writer rounds straight color from the scene's output format."""
    settings = scene.render.image_settings
    return Ok({
        "step": 0.0 if settings.has_linear_colorspace else 1.0 / (2 ** int(settings.color_depth) - 1),
        "straight": settings.color_mode == "RGBA" and not (settings.file_format == "TIFF" and settings.color_depth == "16"),
    })


def copied(scene: bpy.types.Scene, file: Written, identity: Identity) -> Outcome[None]:
    """Writes a diffused PNG copy of a written float frame as the chain displays it, when a render-enabled dither entry diffuses.

    Args:
        scene: Scene holding the dither entries.
        file: Finished single-layer EXR and its display window.
        identity: Extension owning entries, copy settings, and software stamp.

    Returns:
        None, or entry, attachment, read, formation, and write faults.
    """
    config = PyOpenColorIO.GetCurrentConfig()

    def write(settings: DiffusedCopy) -> Outcome[None]:
        target = str(Path(file.path).with_suffix(".png")) if scene.render.use_file_extension else f"{file.path}.png"
        depth: IntegerDepth = 16 if settings.depth == "16" else 8

        def displayed(source: ImageFile[np.float32]) -> Outcome[None]:
            plane = file.display.map(lambda window: source.pixels[window.ybegin : window.yend, window.xbegin : window.xend]).default_value(source.pixels)
            return (
                displayed_frame(identity, scene, np.repeat(plane, 3, axis=-1) if plane.shape[-1] == 1 else plane, source.colorspace, config)
                .bind(
                    lambda shown: ImageFile(pixels=Diffused.coded(unassociated(shown.pixels), depth), colorspace=shown.colorspace, header=source.header).write(
                        target, Storage(), stamped(identity), config
                    )
                )
                .map(lambda _: None)
            )

        return Result[ImageFile[np.float32], Faults].bind(read_file(file.path, config), displayed)

    diffusing = accumulated(
        entry_input(identity, entry, "method").bind(lambda held: Descriptor.resolve(held, "value", bpy.types.Property).map(lambda descriptor: descriptor.read(held) == "Diffused"))
        for entry in dither.entries(identity, scene)
        if Enabled.RENDER.descriptor.read(entry) is True
    ).map(lambda found: scene.render.use_compositing and any(found))
    return diffusing.bind(lambda diffuses: COPY.read(identity, scene).bind(write) if diffuses else Ok(None))


# --- [COMPOSITION] ----------------------------------------------------------------------

TILE: Final = Ownership(kind=bpy.types.Image, key="dither_tile", saved=Saved.USED)
COPY: Final = Attachment(key="diffusion", form=DiffusedCopy, owner=bpy.types.Scene)
dither: Final[CompositorEffect[Dither]] = CompositorEffect(
    record=Dither,
    graph=dithered,
    interface=Interface(
        inputs=(
            Socket(
                key="method",
                name="Method",
                kind=bpy.types.NodeSocketMenu,
                description="Blue-noise dither or whole-frame error diffusion",
                members={"default_value": "Blue Noise"},
                items=Some(
                    MenuItems(
                        items=(
                            MenuItem(name="Blue Noise", description="Triangular blue-noise dither at one output code step with exact black and white endpoints"),
                            MenuItem(name="Diffused", description="Floyd-Steinberg diffusion of separate color errors in raster order, alpha rounded"),
                        )
                    )
                ),
            ),
        )
    ),
    domain=Domain.DISPLAY,
    alpha=Alpha.RESULT,
    parts=(
        Requests[Dither](request=precise),
        Derives[Dither](
            of=code_grid,
            sockets=(
                Socket(key="step", name="Step", kind=bpy.types.NodeSocketFloat, description="Output code step"),
                Socket(key="straight", name="Straight", kind=bpy.types.NodeSocketBool, description="Whether the writer rounds straight color"),
            ),
            reads=tuple((bpy.types.ImageFormatSettings, bpy.types.ImageFormatSettings.bl_rna.properties[name]) for name in ("media_type", "file_format", "color_depth", "color_mode")),
        ),
        Holds(scene=Snapshot.of(bpy.types.Scene, {"render.dither_intensity": 0.0})),
        Registers(rows=(TILE, COPY)),
    ),
)
diffuse: Final = Finish(run=copied, outputs=frozenset({Output.PREVIEW}), kinds=frozenset({Kind.EXR}), depths=frozenset({Depth.HALF, Depth.FLOAT}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COPY", "TILE", "DiffusedCopy", "Dither", "diffuse", "dither"]
