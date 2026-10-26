"""Film grain from packed normal fields and signal-dependent sensor noise."""

from enum import IntEnum
from math import exp
from pathlib import Path

import bpy
from expression import Nothing, Ok, Option, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import read, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, Generated, math, mixed, NodeGroup, Scalar, vector, where, xyz
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.color.spaces import COLOR_INPUT, space_item
from blender.bpy.render.effects.compositing import masks
from blender.bpy.render.effects.coordinates import clocked, gridded, origin
from blender.bpy.render.effects.generators.noise import gaussian
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Inspected, precise, Shell
from blender.imaging.chain import GREY
from blender.imaging.effects.detail.grain import correlated, Film, Grain, Procedural, Scan, Sensor
from blender.imaging.faults import paired
from blender.imaging.hashing import content_hash
from blender.imaging.parameters import Motion

# --- [TYPES] ----------------------------------------------------------------------------


class Noise(IntEnum):
    """Separate seed domains of film tile shifts and sensor draws."""

    FILM = 0
    SENSOR = 1


# --- [OPERATIONS] -----------------------------------------------------------------------


def grained(shell: Shell[Grain]) -> BlenderResult[Expr]:
    """Builds film and sensor grain on straight scene-linear color.

    Args:
        shell: Entry inputs, stored scan case, config, and displayed frame geometry.

    Returns:
        Selected medium, or accumulated luminance, data space, and field faults.
    """
    medium, chroma_field, motion, seed = Grain.__struct_fields__
    field, granularity_field, shadows, midtones, highlights, shadow_edge, midtone_span, highlight_span = Film.__struct_fields__
    (size,) = Procedural.__struct_fields__
    _, span = Scan.__struct_fields__
    shot_field, read_field = Sensor.__struct_fields__
    speed, hold = Motion.__struct_fields__
    inlet, source = shell.input, shell.color
    chroma = inlet(chroma_field)
    own_weight, shared_weight = math("SQRT", chroma), math("SQRT", 1.0 - chroma)
    granularity = inlet(medium, Film, granularity_field)
    pattern = math("FLOOR", clocked(inlet(motion, speed), inlet(motion, hold)))

    def developed(sample: Expr, texel: Expr, correlation: npt.NDArray[np.float64]) -> Expr:
        width, height, diagonal, antidiagonal = correlation
        terms = np.stack((-2.0 * (1.0 - width), -2.0 * (1.0 - height), 4.0 - 4.0 * width - 4.0 * height + 2.0 * (diagonal + antidiagonal)))
        own_across, own_up, own_joint = (tuple(map(float, row[:3])) for row in terms)
        shared_across, shared_up, shared_joint = map(float, terms[:, 3])
        phase = vector("FRACTION", texel)
        spread = phase * ((1.0, 1.0, 1.0) - phase)
        across, up = spread["X"], spread["Y"]
        own = vector("MULTIPLY_ADD", up, vector("MULTIPLY_ADD", across, own_joint, own_up), vector("MULTIPLY_ADD", across, own_across, (1.0, 1.0, 1.0)))
        shared = math("MULTIPLY_ADD", up, math("MULTIPLY_ADD", across, shared_joint, shared_up), math("MULTIPLY_ADD", across, shared_across, 1.0))
        normal = vector("MULTIPLY_ADD", vector("DIVIDE", sample, vector("POWER", own, (0.5, 0.5, 0.5))), own_weight, sample["Alpha"] / math("SQRT", shared) * shared_weight)
        return vector("POWER", (exp(1.0),) * 3, vector("MULTIPLY_ADD", normal, granularity, granularity * granularity * -0.5))

    def sampled(content: Generated, width: Scalar, diameters: float, correlation: npt.NDArray[np.float64], offset: Expr) -> Expr:
        step = 1.0 / 256.0
        image = Expr(kind=bpy.types.CompositorNodeImage, content=Some(content), socket="Image", shape="RGBA")
        dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", image),), socket="Dimensions", shape="VECTOR")
        scale = xyz(dimensions["X"], dimensions["Y"], 1.0)
        ratio = vector("DIVIDE", dimensions["X"], dimensions) / (diameters, diameters, diameters) / width
        snapped = vector("SNAP", vector("MULTIPLY_ADD", vector("MULTIPLY_ADD", shell.space.point, ratio, offset), scale, (0.5 * step, 0.5 * step, 0.0)), (step, step, step))
        tiled = Expr(
            kind=bpy.types.CompositorNodeMapUV,
            inputs=(("Image", image), ("UV", snapped / scale), ("Interpolation", "Bilinear"), ("Extension X", "Repeat"), ("Extension Y", "Repeat")),
            socket="Image",
            shape="RGBA",
        )
        return developed(tiled, snapped - (0.5, 0.5, 0.5), correlation)

    def fielded(offset: Expr, space: str) -> BlenderResult[Expr]:
        tag = Tag(space=Some(space), alpha=Some("CHANNEL_PACKED"))
        procedural = Generated(ownership=tiles, content=Procedural.packed(), name=f"{Grain.title} {Procedural.title}", plane=lambda: Ok(Some(Procedural.tile())), tag=tag)

        def scanned(scan: Scan) -> BlenderResult[Expr]:
            key = content_hash(Scan, Scan(path=scan.path))
            plane = (
                tiles
                .found(shell.identity, content=Some(key))
                .map(lambda image: read(image, shell.config).map(lambda file: file.pixels))
                .default_with(lambda: Scan(path=bpy.path.abspath(scan.path)).tile(shell.config))
            )
            return plane.map(
                lambda values: sampled(
                    Generated(ownership=tiles, content=key, name=f"{Grain.title} {Path(scan.path).name}", plane=lambda: Ok(Some(values)), tag=tag),
                    inlet(medium, Film, field, Scan, span),
                    1.0,
                    correlated(values),
                    offset,
                )
            )

        scan: BlenderResult[Option[Expr]] = shell.case(Scan, medium, Film, field).map(lambda held: scanned(held).map(Some)).default_value(Ok(Nothing))
        return scan.map(
            lambda held: shell.menu(
                medium,
                Film,
                field,
                cases={
                    Procedural.title: sampled(procedural, inlet(medium, Film, field, Procedural, size), float(Procedural.diameters), Procedural.correlation(), offset),
                    Scan.title: held.default_value((1.0, 1.0, 1.0, 1.0)),
                },
                kind="RGBA",
            ).named("field")
        )

    def filmed(factor: Expr, level: Expr) -> Expr:
        def band(distance: Expr, width: Expr) -> Expr:
            smooth = Expr(
                kind=bpy.types.ShaderNodeMapRange,
                inputs=(("Value", distance / width), ("From Min", 0.0), ("From Max", 1.0), ("To Min", 0.0), ("To Max", 1.0)),
                members=(("interpolation_type", "SMOOTHSTEP"), ("clamp", True)),
                socket="Result",
            )
            return where(width > 0.0, smooth, distance > 0.0)

        low, middle, high, edge, middle_span, high_span = (inlet(medium, Film, name) for name in (shadows, midtones, highlights, shadow_edge, midtone_span, highlight_span))
        toned = math("MULTIPLY_ADD", 1.0 - band(level, edge), low - middle, middle)
        weight = math("MULTIPLY_ADD", band(level - edge - middle_span, high_span), high - middle, toned).named("weight")
        return mixed(weight, source, factor, blend="MULTIPLY")

    def sensored(level: Expr) -> Expr:
        grid = gridded(source)
        sited = vector("FLOOR", grid.base / grid.inverse) + origin(inlet(seed), Noise.SENSOR, pattern)
        draws = tuple(Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", sited), ("W", float(stream))), members=(("noise_dimensions", "4D"),)) for stream in range(4))
        (shared, red), (green, blue) = gaussian(*draws[:2]), gaussian(*draws[2:])
        shot, readout = inlet(medium, Sensor, shot_field), inlet(medium, Sensor, read_field)
        gain, floor = shot * shot / GREY, readout * readout
        deviation = vector("POWER", vector("MULTIPLY_ADD", vector("MAXIMUM", source, (0.0, 0.0, 0.0)), gain, floor), (0.5, 0.5, 0.5)).named("deviation")
        scatter = vector("SCALE", xyz(red, green, blue), own_weight) * deviation + shared * shared_weight * math("SQRT", math("MULTIPLY_ADD", math("MAXIMUM", level, 0.0), gain, floor))
        empty = Expr(kind=bpy.types.FunctionNodeCompare, inputs=(("A", source["Alpha"]), ("B", 0.0), ("Epsilon", 0.0)), members=(("data_type", "FLOAT"), ("operation", "EQUAL")), socket="Result")
        return where(empty, source, vector("ADD", source, scatter))

    def built(working: NodeGroup, factor: Expr) -> Expr:
        level = working.placed({COLOR_INPUT.key: source}, masks.LUMINANCE.key)
        return shell.menu(medium, cases={Film.title: filmed(factor, level), Sensor.title: sensored(level)}, kind="RGBA")

    offset = vector("MULTIPLY_ADD", origin(inlet(seed), Noise.FILM, pattern), (1.0, 1.0, 0.0), (0.0, 0.0, 1.0))
    return paired(masks.luminance(shell.config), space_item(shell.config, PyOpenColorIO.ROLE_DATA).bind(lambda space: fielded(offset, space))).map(lambda parts: built(*parts))


# --- [COMPOSITION] ----------------------------------------------------------------------

tiles = Ownership(kind=bpy.types.Image, key="grain_tile", saved=Saved.USED)
grain = CompositorEffect(
    record=Grain,
    graph=grained,
    alpha=Alpha.STRAIGHT,
    requested=Some(precise),
    inspected=tuple(
        Inspected(key=item_key(Grain, node), end=End(node=node, socket=socket), description=description)
        for node, socket, description in (
            ("weight", "Value", "Film grain tonal weight"),
            ("field", "Output", "Developed film grain factor"),
            ("deviation", "Vector", "Signal-dependent channel deviation before noise draw"),
        )
    ),
    registrables=(tiles,),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Noise", "grain", "grained", "tiles"]
