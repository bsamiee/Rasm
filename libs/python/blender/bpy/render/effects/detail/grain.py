"""Film grain from packed normal fields and signal-dependent sensor noise."""

from enum import IntEnum
from functools import partial
from math import exp
from pathlib import Path

import bpy
from expression import Nothing, Ok, Option, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import read, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderImagingFault, BlenderResult
from blender.bpy.nodes.coordinates import framed, FrameSpace, gridded
from blender.bpy.nodes.graphs import Clock, compared, End, ended, Fragment, Generated, masked, math, menu, mixed, nested, node, Operand, origin, vector
from blender.bpy.nodes.interface import item_key, MenuItems
from blender.bpy.render.color.spaces import COLOR_INPUT, space_item
from blender.bpy.render.effects.compositing.masks import luminance
from blender.bpy.render.effects.generators.noise import gaussian
from blender.bpy.render.effects.stack import CompositorEffect, precise, Shell
from blender.bpy.render.effects.viewers import inspection
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


def grained(shell: Shell[Grain]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds film and sensor grain on straight scene-linear color.

    Args:
        shell: Entry inputs, stored scan case, config, and displayed frame geometry.

    Returns:
        Selected medium with its graph, or accumulated graph and image faults.
    """
    socket = shell.socket
    medium, chroma_field, motion, seed = Grain.__struct_fields__
    speed, hold = Motion.__struct_fields__
    chroma = socket(chroma_field)

    def developed(key: Key, sample: Operand, texel: Operand, correlation: npt.NDArray[np.float64], chroma: Operand, granularity: Operand) -> BlenderResult[tuple[Fragment, End]]:
        width, height, diagonal, antidiagonal = correlation
        terms = np.stack((-2.0 * (1.0 - width), -2.0 * (1.0 - height), 4.0 - 4.0 * width - 4.0 * height + 2.0 * (diagonal + antidiagonal)))
        own_across, own_up, own_joint = (tuple(map(float, row[:3])) for row in terms)
        shared_across, shared_up, shared_joint = map(float, terms[:, 3])

        def at(name: str, socket: str = "Value") -> End:
            return End(node=f"{key}_{name}", socket=socket)

        horizontal, vertical = at("spread_axes", "X"), at("spread_axes", "Y")

        return Fragment.joined(
            math(f"{key}_own_weight", "SQRT", chroma),
            math(f"{key}_shared_variance", "SUBTRACT", 1.0, chroma),
            math(f"{key}_shared_weight", "SQRT", at("shared_variance")),
            math(f"{key}_variance", "MULTIPLY", granularity, granularity),
            math(f"{key}_correction", "MULTIPLY", at("variance"), -0.5),
            vector(f"{key}_phase", "FRACTION", texel),
            vector(f"{key}_complement", "SUBTRACT", (1.0, 1.0, 1.0), at("phase", "Vector")),
            vector(f"{key}_spread", "MULTIPLY", at("phase", "Vector"), at("complement", "Vector")),
            node(f"{key}_spread_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": at("spread", "Vector")}, {}),
            vector(f"{key}_own_across", "MULTIPLY_ADD", horizontal, own_across, (1.0, 1.0, 1.0)),
            vector(f"{key}_own_up", "MULTIPLY_ADD", horizontal, own_joint, own_up),
            vector(f"{key}_own_sampled", "MULTIPLY_ADD", vertical, at("own_up", "Vector"), at("own_across", "Vector")),
            vector(f"{key}_own_deviation", "POWER", at("own_sampled", "Vector"), (0.5, 0.5, 0.5)),
            vector(f"{key}_own_normal", "DIVIDE", sample, at("own_deviation", "Vector")),
            node(f"{key}_split", bpy.types.CompositorNodeSeparateColor, {"Image": sample}, {"mode": "RGB"}),
            math(f"{key}_shared_across", "MULTIPLY_ADD", horizontal, shared_across, 1.0),
            math(f"{key}_shared_up", "MULTIPLY_ADD", horizontal, shared_joint, shared_up),
            math(f"{key}_shared_sampled", "MULTIPLY_ADD", vertical, at("shared_up"), at("shared_across")),
            math(f"{key}_shared_deviation", "SQRT", at("shared_sampled")),
            math(f"{key}_shared_normal", "DIVIDE", at("split", "Alpha"), at("shared_deviation")),
            math(f"{key}_shared", "MULTIPLY", at("shared_normal"), at("shared_weight")),
            vector(f"{key}_normal", "MULTIPLY_ADD", at("own_normal", "Vector"), at("own_weight"), at("shared")),
            vector(f"{key}_logarithm", "MULTIPLY_ADD", at("normal", "Vector"), granularity, at("correction")),
            vector(key, "POWER", (exp(1.0),) * 3, at("logarithm", "Vector")),
        ).map(ended(key, "Vector"))

    def scan_plane(scan: Scan) -> BlenderResult[npt.NDArray[np.float32]]:
        return Scan(path=bpy.path.abspath(scan.path)).tile(shell.config).map_error(BlenderImagingFault.lifted)

    def fielded(point: Operand, offset: Operand, space: str) -> BlenderResult[tuple[Fragment, End]]:
        field, granularity_field, _, _, _, _, _, _ = Film.__struct_fields__
        (size,) = Procedural.__struct_fields__
        field_path = medium, Film, field
        granularity = socket(medium, Film, granularity_field)

        def sampled(key: Key, content: Generated, width: Operand, diameters: float, correlation: npt.NDArray[np.float64]) -> BlenderResult[tuple[Fragment, End]]:
            image = End(node=key, socket="Image")
            dimensions = End(node=f"{key}_info", socket="Dimensions")
            scale = End(node=f"{key}_scale", socket="Vector")
            snapped = End(node=f"{key}_snapped", socket="Vector")
            weight_step = 1.0 / 256.0
            return Fragment.joined(
                node(key, bpy.types.CompositorNodeImage, {}, {}, Some(content)),
                node(f"{key}_info", bpy.types.CompositorNodeImageInfo, {"Image": image}, {}),
                node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": dimensions}, {}),
                node(f"{key}_scale", bpy.types.ShaderNodeCombineXYZ, {"X": End(node=f"{key}_axes", socket="X"), "Y": End(node=f"{key}_axes", socket="Y"), "Z": 1.0}, {}),
                vector(f"{key}_aspect", "DIVIDE", End(node=f"{key}_axes", socket="X"), dimensions),
                vector(f"{key}_diameters", "DIVIDE", End(node=f"{key}_aspect", socket="Vector"), (diameters,) * 3),
                vector(f"{key}_ratio", "DIVIDE", End(node=f"{key}_diameters", socket="Vector"), width),
                vector(f"{key}_uv", "MULTIPLY_ADD", point, End(node=f"{key}_ratio", socket="Vector"), offset),
                vector(f"{key}_centre", "MULTIPLY_ADD", End(node=f"{key}_uv", socket="Vector"), scale, (0.5 * weight_step, 0.5 * weight_step, 0.0)),
                vector(f"{key}_snapped", "SNAP", End(node=f"{key}_centre", socket="Vector"), (weight_step,) * 3),
                vector(f"{key}_grid", "DIVIDE", snapped, scale),
                node(
                    f"{key}_sample",
                    bpy.types.CompositorNodeMapUV,
                    {"Image": image, "UV": End(node=f"{key}_grid", socket="Vector"), "Interpolation": "Bilinear", "Extension X": "Repeat", "Extension Y": "Repeat"},
                    {},
                ),
                vector(f"{key}_texel", "SUBTRACT", snapped, (0.5, 0.5, 0.5)),
                developed(f"{key}_factor", End(node=f"{key}_sample", socket="Image"), End(node=f"{key}_texel", socket="Vector"), correlation, chroma, granularity),
            ).map(ended(f"{key}_factor", "Vector"))

        def from_scan(scan: Scan, tag: Tag) -> BlenderResult[Option[tuple[Fragment, End]]]:
            _, span = Scan.__struct_fields__
            key = content_hash(Scan, Scan(path=scan.path))
            plane = tiles.found(shell.identity, content=Some(key)).map(lambda image: read(image, shell.config).map(lambda file: file.pixels)).default_with(lambda: scan_plane(scan))
            return plane.bind(
                lambda values: sampled(
                    item_key(Scan),
                    Generated(ownership=tiles, content=key, name=f"{Grain.title} {Path(scan.path).name}", plane=lambda: Ok(Some(values)), tag=tag),
                    socket(*field_path, Scan, span),
                    1.0,
                    correlated(values),
                )
            ).map(Some)

        def chosen(parts: tuple[tuple[Fragment, End], Option[tuple[Fragment, End]]]) -> BlenderResult[tuple[Fragment, End]]:
            procedural, scan = parts
            cases: dict[str, Operand] = {Procedural.title: procedural, Scan.title: scan.default_value((1.0, 1.0, 1.0, 1.0))}
            return menu("field", "RGBA", socket(*field_path), MenuItems.of((Procedural, Scan)), cases)

        tag = Tag(space=Some(space), alpha=Some("CHANNEL_PACKED"))
        procedural = Generated(ownership=tiles, content=Procedural.packed(), name=f"{Grain.title} {Procedural.title}", plane=lambda: Ok(Some(Procedural.tile())), tag=tag)
        scan = shell.case(Scan, *field_path).map(partial(from_scan, tag=tag)).default_value(Ok(Nothing))
        return paired(sampled(item_key(Procedural), procedural, socket(*field_path, Procedural, size), float(Procedural.diameters), Procedural.correlation()), scan).bind(chosen)

    def filmed(field: Operand, level: Operand) -> BlenderResult[tuple[Fragment, End]]:
        _, _, shadows, midtones, highlights, shadow_edge, midtone_span, highlight_span = (socket(medium, Film, name) for name in Film.__struct_fields__)

        def band(key: Key, distance: Operand, span: Operand) -> BlenderResult[tuple[Fragment, End]]:
            smooth = (
                math(f"{key}_position", "DIVIDE", distance, span)
                .bind(
                    lambda position: node(
                        f"{key}_smooth",
                        bpy.types.ShaderNodeMapRange,
                        {"Value": position, "From Min": 0.0, "From Max": 1.0, "To Min": 0.0, "To Max": 1.0},
                        {"interpolation_type": "SMOOTHSTEP", "clamp": True},
                    )
                )
                .map(ended(f"{key}_smooth", "Result"))
            )
            return Fragment.joined(
                smooth,
                math(f"{key}_positive", "GREATER_THAN", span, 0.0),
                math(f"{key}_step", "GREATER_THAN", distance, 0.0),
                masked(key, "FLOAT", End(node=f"{key}_positive"), End(node=f"{key}_step"), End(node=f"{key}_smooth", socket="Result")),
            ).map(ended(key, "Value"))

        def weighted(shadow: Operand, highlight: Operand) -> BlenderResult[tuple[Fragment, End]]:
            return (
                paired(math("highs", "SUBTRACT", highlights, midtones), math("lows", "SUBTRACT", shadows, midtones).bind(lambda lows: math("toned", "MULTIPLY_ADD", shadow, lows, midtones)))
                .bind(lambda parts: math("weight", "MULTIPLY_ADD", highlight, *parts))
                .bind(lambda weight: mixed("film", "RGBA", weight, shell.source, field, "MULTIPLY"))
            )

        return paired(
            band("shadow", level, shadow_edge).bind(lambda shadow: math("shadow_weight", "SUBTRACT", 1.0, shadow)),
            math("highlight_offset", "SUBTRACT", level, shadow_edge)
            .bind(lambda shifted: math("highlight_distance", "SUBTRACT", shifted, midtone_span))
            .bind(lambda distance: band("highlight", distance, highlight_span)),
        ).bind(lambda bands: weighted(*bands))

    def sensored(level: Operand, pattern: Operand) -> BlenderResult[tuple[Fragment, End]]:
        source = shell.source
        shot, read = (socket(medium, Sensor, name) for name in Sensor.__struct_fields__)
        gain, floor = End(node="gain"), End(node="floor")
        own_weight, shared_weight = End(node="own_weight"), End(node="shared_weight")
        sited = End(node="sited", socket="Vector")
        draws = tuple(End(node=f"draw_{stream}") for stream in range(4))
        radius_a, angle_a, radius_b, angle_b = draws

        def scattered(normals: tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], tuple[tuple[Fragment, End], tuple[Fragment, End]]]) -> BlenderResult[tuple[Fragment, End]]:
            (shared, red), (green, blue) = normals
            return Fragment.joined(
                gridded("photosites", source).bind(lambda grid: vector("pixel", "DIVIDE", grid.base, grid.inverse)),
                vector("photosite", "FLOOR", End(node="pixel", socket="Vector")),
                origin("draw_origin", socket(seed), Noise.SENSOR, pattern),
                vector("sited", "ADD", End(node="photosite", socket="Vector"), End(node="draw_origin", socket="Vector")),
                *(node(draw.node, bpy.types.ShaderNodeTexWhiteNoise, {"Vector": sited, "W": float(stream)}, {"noise_dimensions": "4D"}) for stream, draw in enumerate(draws)),
                math("shot_variance", "MULTIPLY", shot, shot),
                math("gain", "DIVIDE", End(node="shot_variance"), GREY),
                math("floor", "MULTIPLY", read, read),
                node("own", bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y", "Z"), (red, green, blue))), {}),
                vector("signal", "MAXIMUM", source, (0.0, 0.0, 0.0)),
                vector("variance", "MULTIPLY_ADD", End(node="signal", socket="Vector"), gain, floor),
                vector("deviation", "POWER", End(node="variance", socket="Vector"), (0.5, 0.5, 0.5)),
                math("luminance_signal", "MAXIMUM", level, 0.0),
                math("luminance_variance", "MULTIPLY_ADD", End(node="luminance_signal"), gain, floor),
                math("luminance_deviation", "SQRT", End(node="luminance_variance")),
                math("own_weight", "SQRT", chroma),
                math("shared_variance", "SUBTRACT", 1.0, chroma),
                math("shared_weight", "SQRT", End(node="shared_variance")),
                vector("own_scaled", "SCALE", End(node="own", socket="Vector"), own_weight),
                vector("own_scatter", "MULTIPLY", End(node="own_scaled", socket="Vector"), End(node="deviation", socket="Vector")),
                math("shared_scaled", "MULTIPLY", shared, shared_weight),
                math("shared_scatter", "MULTIPLY", End(node="shared_scaled"), End(node="luminance_deviation")),
                vector("scatter", "ADD", End(node="own_scatter", socket="Vector"), End(node="shared_scatter")),
                vector("sensor_noise", "ADD", source, End(node="scatter", socket="Vector")),
                node("source_alpha", bpy.types.CompositorNodeSeparateColor, {"Image": source}, {"mode": "RGB"}),
                compared("zero_alpha", "EQUAL", End(node="source_alpha", socket="Alpha"), 0.0, epsilon=Some(0.0)),
                masked("sensor", "RGBA", End(node="zero_alpha", socket="Result"), End(node="sensor_noise", socket="Vector"), source),
            ).map(ended("sensor", "Image"))

        return paired(gaussian("normal_a", radius_a, angle_a), gaussian("normal_b", radius_b, angle_b)).bind(scattered)

    def built(parts: tuple[tuple[tuple[Fragment, End], FrameSpace], tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
        (level, space), pattern = parts
        film = (
            origin("shift", shell.socket(seed), Noise.FILM, pattern)
            .bind(lambda shift: vector("offset", "MULTIPLY_ADD", shift, (1.0, 1.0, 0.0), (0.0, 0.0, 1.0)))
            .bind(lambda offset: space_item(shell.config, PyOpenColorIO.ROLE_DATA).bind(partial(fielded, space.point, offset)))
            .bind(lambda field: filmed(field, level))
        )
        return paired(film, sensored(level, pattern)).bind(lambda media: menu("medium", "RGBA", shell.socket(medium), shell.items[medium,], dict(zip((Film.title, Sensor.title), media))))

    return paired(
        paired(luminance(shell.config).bind(lambda working: nested("luminance", working, {COLOR_INPUT.key: shell.source}, "luminance")), framed("frame", shell.source, shell.aspect, shell.scale)),
        Clock(key="clock", speed=shell.socket(motion, speed), hold=shell.socket(motion, hold)).pattern,
    ).bind(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

tiles = Ownership(kind=bpy.types.Image, key="grain_tile", saved=Saved.USED)
grain: CompositorEffect[Grain] = CompositorEffect(
    record=Grain,
    graph=grained,
    straight=True,
    requested=Some(precise),
    registrables=(
        tiles,
        *(
            inspection(row=lambda: grain, key=item_key(Grain, node), title=title, description=description, target=partial(End, node=node, socket=socket))
            for node, socket, title, description in (
                ("weight", "Value", "View Grain Weight", "Film grain tonal weight"),
                ("field", "Output", "View Grain Field", "Developed film grain factor"),
                ("deviation", "Vector", "View Sensor Deviation", "Signal-dependent channel deviation before noise draw"),
            )
        ),
    ),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Noise", "grain", "grained", "tiles"]
