"""Output dither: triangular blue-noise offsets at the output code step, or a Floyd-Steinberg diffused PNG copy beside each written float frame."""

from collections.abc import Mapping
from functools import partial
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
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.content import Generated
from blender.bpy.nodes.desired import MenuItem, MenuItems
from blender.bpy.nodes.graphs import Expr, math, mixed, switched, vector
from blender.bpy.nodes.interfaces import Interface, Socket
from blender.bpy.render.color.spaces import UNTAGGED
from blender.bpy.render.compositor.colors import space_item
from blender.bpy.render.compositor.coordinates import dimensions, PixelGrid, uv_mapped
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.inputs import Derives, entry_value, Holds, Shell
from blender.bpy.render.compositor.placement import Enabled
from blender.bpy.render.outputs.formats import code_step, rounds_straight
from blender.bpy.render.outputs.results import Finish, Written
from blender.bpy.settings.attachments import Attachment
from blender.imaging.files.buffers import Coding, Depth
from blender.imaging.files.images import Format, ImageFile, read_file, Storage
from blender.imaging.sampling.tiles import ranks
from blender.records.faults import accumulated, Fault, Faults, Outcome, Reason
from blender.records.fields import Case
from blender.records.stores import content_hash
from blender.rendering.outputs.naming import Output

from ..formation.display import displayed_frame

# --- [CONSTANTS] ------------------------------------------------------------------------

_BLUE_NOISE: Final = MenuItem(name="Blue Noise", description="Triangular blue-noise dither at one output code step with exact black and white endpoints")
_DIFFUSED: Final = MenuItem(name="Diffused", description="Floyd-Steinberg diffusion of separate color errors in raster order, alpha rounded")
_METHOD: Final = Socket(
    key="method", kind=bpy.types.NodeSocketMenu, description="Blue-noise dither or whole-frame error diffusion", default_value=_BLUE_NOISE.name, items=Some(MenuItems(items=(_BLUE_NOISE, _DIFFUSED)))
)
_STEP: Final = Socket(key="step", kind=bpy.types.NodeSocketFloat, description="Output code step")
_STRAIGHT: Final = Socket(key="straight", kind=bpy.types.NodeSocketBool, description="Whether the writer rounds straight color")
_COPY_FORMAT: Final = "png"
TILE: Final = Ownership(kind=bpy.types.Image, key="dither_tile", saved=Saved.USED)

# --- [MODELS] ---------------------------------------------------------------------------


class Dither(Case, frozen=True, kw_only=True):
    """Blue-noise dither of the output codes, or whole-frame error diffusion of a PNG copy."""


class DiffusedCopy(bpy.types.PropertyGroup):
    """PNG copy written beside each float frame a diffusing dither entry renders."""

    depth: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Copy Depth",
        description="Channel depth of diffused PNG copy",
        items=choices(item for item in Descriptor.declared(bpy.types.ImageFormatSettings, ("color_depth",), bpy.types.EnumProperty).prop.enum_items if item.identifier in {"8", "16"}),
        default="8",
    )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _coded(_identity: Identity, scene: bpy.types.Scene, _entry: bpy.types.SceneCompositorEffect, _record: Dither) -> Outcome[Mapping[Key, Value]]:
    """Reads the output code step and whether the writer rounds straight color from the scene's output format."""
    settings = scene.render.image_settings
    return Ok({_STEP.key: code_step(settings), _STRAIGHT.key: rounds_straight(settings)})


def _copied(scene: bpy.types.Scene, file: Written, identity: Identity) -> Outcome[None]:
    """Writes a diffused PNG copy of a written float frame as the compositor stack displays it, when a render-enabled dither entry diffuses.

    Args:
        scene: Scene holding the dither entries.
        file: Finished single-layer EXR and its display window.
        identity: Extension owning entries, copy settings, and software stamp.

    Returns:
        None, or entry, attachment, format, read, formation, and write faults.
    """
    config = PyOpenColorIO.GetCurrentConfig()
    suffix = Format.writer(_COPY_FORMAT).map(lambda row: row.suffixes[0]).to_result_with(partial(Fault.of, Reason.ABSENT, "OpenImageIO writers", member=_COPY_FORMAT))

    def write(settings: DiffusedCopy, extension: str) -> Outcome[None]:
        target = Path(file.path).with_suffix(extension) if scene.render.use_file_extension else Path(f"{file.path}{extension}")
        storage = Storage(depth=Depth.UINT16 if settings.depth == "16" else Depth.UINT8, coding=Coding.DIFFUSED)

        def displayed(source: ImageFile) -> Outcome[None]:
            plane = file.display.map(lambda window: source.pixels[window.ybegin : window.yend, window.xbegin : window.xend]).default_value(source.pixels)
            return (
                displayed_frame(identity, scene, np.repeat(plane, 3, axis=-1) if plane.shape[-1] == 1 else plane, source.colorspace, config)
                .bind(lambda shown: ImageFile(pixels=shown.pixels, colorspace=shown.colorspace, header=source.header).written(target, storage, identity.manifest.stamp, config))
                .map(lambda _: None)
            )

        return Result[ImageFile, Faults].bind(read_file(Path(file.path), config, untagged=Some(UNTAGGED)), displayed)

    diffusing = accumulated(
        entry_value(identity, entry, _METHOD).map(lambda method: method == _DIFFUSED.name) for entry in dither.entries(identity, scene) if Enabled.RENDER.descriptor.read(entry) is True
    ).map(lambda found: scene.render.use_compositing and any(found))
    return diffusing.bind(lambda diffuses: COPY.read(identity, scene).bind(lambda settings: suffix.bind(partial(write, settings))) if diffuses else Ok(None))


def _dither(shell: Shell[Dither]) -> Outcome[Expr]:
    """Adds triangular blue-noise offsets of one code step as the last display step, passing the source under diffusion.

    Args:
        shell: Associated display image, method menu, code grid, and config.

    Returns:
        Selected image, or the data space fault.
    """
    source, step, straight = shell.color, shell.input(_STEP), shell.input(_STRAIGHT)

    def built(data: str) -> Expr:
        generated = Generated(
            ownership=TILE,
            content=content_hash(Dither, providers=(OpenImageIO,)),
            name="Blue Noise Tile",
            plane=lambda: ranks(OpenImageIO.ImageBufAlgo.bluenoise_image()).map(Some),
            tag=Tag(space=Some(data)),
        )
        tile = Expr(kind=bpy.types.CompositorNodeImage, content=Some(generated), socket="Image", shape=bpy.types.NodeSocketColor)
        rank = uv_mapped(tile, vector("ADD", vector("DIVIDE", PixelGrid.of(source).point, dimensions(tile)), (0.0, 0.0, 1.0)), interpolation="Nearest", extension=("Repeat", "Repeat"))["Red"]
        rectangular, signed = rank - 0.5, math("MULTIPLY_ADD", rank, 2.0, -1.0)
        triangular_rank = math("SIGN", signed) * (1.0 - math("SQRT", 1.0 - abs(signed)))
        weight = switched(straight, 1.0, source["Alpha"])
        value = vector("DIVIDE", source, weight)
        nearest = vector("MINIMUM", value, vector("SUBTRACT", (1.0, 1.0, 1.0), value))
        ends = vector("MINIMUM", vector("MAXIMUM", vector("DIVIDE", nearest, step * 0.5), (0.0, 0.0, 0.0)), (1.0, 1.0, 1.0))
        triangular = vector("MULTIPLY_ADD", ends, triangular_rank - rectangular, rectangular).named("triangular")
        added = mixed(1.0, source, vector("SCALE", triangular, step * weight), blend="ADD")
        return shell.menu(_METHOD, cases={_BLUE_NOISE: added, _DIFFUSED: source})

    return space_item(shell.config, PyOpenColorIO.ROLE_DATA).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

COPY: Final = Attachment(key="diffusion", form=DiffusedCopy, owner=bpy.types.Scene)
dither: Final = CompositorEffect(
    _dither,
    record=Dither,
    interface=Interface(inputs=(_METHOD,)),
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(precise),
    derives=(
        Derives[Dither](
            of=_coded,
            sockets=(_STEP, _STRAIGHT),
            reads=tuple((bpy.types.ImageFormatSettings, bpy.types.ImageFormatSettings.bl_rna.properties[name]) for name in ("media_type", "file_format", "color_depth", "color_mode")),
        ),
    ),
    holds=Holds(scene=Snapshot.of(bpy.types.Scene, {("render", "dither_intensity"): 0.0})),
    registers=(TILE, COPY),
)
diffuse: Final = Finish(run=_copied, outputs=frozenset({Output.PREVIEW}), formats=frozenset({"OPEN_EXR"}), depths=frozenset({Depth.HALF, Depth.FLOAT}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COPY", "TILE", "DiffusedCopy", "Dither", "diffuse", "dither"]
