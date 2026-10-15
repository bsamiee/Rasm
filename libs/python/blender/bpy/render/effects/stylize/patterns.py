"""Native screen coverage and sampled structures for display stylization."""

from enum import IntEnum
import functools
from itertools import product
from math import pi, sqrt

import bpy
from expression import Ok, Some

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.coordinates import framed, FrameSpace, gridded, PixelGrid, pixels, sampled
from blender.bpy.nodes.graphs import Clock, compared, End, ended, Fragment, INPUTS, masked, math, menu, mixed, nested, node, NodeGroup, Operand, origin, switched, vector
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.render.effects.stack import CompositorEffect, Coverage, precise, Shell
from blender.bpy.render.effects.viewers import inspection
from blender.imaging.effects.stylize.patterns import Cells, Direction, Glyphs, Halftone, Metric, Mono, Mosaic, Pattern, Pixelation, PixelSort, Process, RandomColor, Screen, SourceColor, TwoTone
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Motion

# --- [TYPES] ----------------------------------------------------------------------------


class Noise(IntEnum):
    """Persistent decorrelation keys for independent pattern draws."""

    SCREEN = 0
    GLYPH = 1
    CELL = 2
    SCATTER = 3
    STREAK = 4


# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def halftone_screen() -> BlenderResult[NodeGroup]:
    """Builds shared unit-square ink coverage with a pixel derivative antialias width.

    Returns:
        Screen group with coordinate, offset, angle, pitch, jitter, and tone inputs,
        or accumulated native graph faults.
    """
    coordinate = End(node=INPUTS, socket="coordinate")
    angle, pitch, tone = (End(node=INPUTS, socket=name) for name in ("angle", "pitch", "tone"))
    area = End(node="area", socket="Value")

    def antialiased(grid: PixelGrid) -> BlenderResult[tuple[Fragment, End]]:
        return Fragment.joined(
            *(
                grid.at(f"{name}_uv", offset, grid.inverse).bind(
                    lambda uv: node(name, bpy.types.CompositorNodeMapUV, {"Image": area, "UV": uv, "Interpolation": "Nearest", "Extension X": "Extend", "Extension Y": "Extend"}, {})
                )
                for name, offset in (("right", (1.0, 0.0, 0.0)), ("up", (0.0, 1.0, 0.0)))
            ),
            math("right_difference", "SUBTRACT", End(node="right", socket="Image"), area),
            math("up_difference", "SUBTRACT", End(node="up", socket="Image"), area),
            math("right_width", "ABSOLUTE", End(node="right_difference")),
            math("up_width", "ABSOLUTE", End(node="up_difference")),
            math("width", "ADD", End(node="right_width"), End(node="up_width")),
            math("distance", "SUBTRACT", tone, area),
            math("fraction", "DIVIDE", End(node="distance"), End(node="width")),
            math("smooth", "ADD", tone, End(node="fraction"), clamp=True),
            math("hard", "LESS_THAN", area, tone),
            math("varying", "GREATER_THAN", End(node="width"), 0.0),
            masked("inside", "FLOAT", End(node="varying"), End(node="hard"), End(node="smooth")),
            math("nonempty", "GREATER_THAN", tone, 0.0),
            math("incomplete", "LESS_THAN", tone, 1.0),
            masked("filled", "FLOAT", End(node="incomplete"), 1.0, End(node="inside")),
            math("coverage", "MULTIPLY", End(node="nonempty"), End(node="filled")),
        ).map(ended("coverage", "Value"))

    return (
        Fragment
        .joined(
            node("axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": coordinate}, {}),
            math("cosine", "COSINE", angle),
            math("sine", "SINE", angle),
            math("rotated_x_term", "MULTIPLY", End(node="axes", socket="Y"), End(node="sine")),
            math("rotated_x", "MULTIPLY_ADD", End(node="axes", socket="X"), End(node="cosine"), End(node="rotated_x_term")),
            math("rotated_y_term", "MULTIPLY", End(node="axes", socket="X"), End(node="sine")),
            math("negative_x", "MULTIPLY", End(node="rotated_y_term"), -1.0),
            math("rotated_y", "MULTIPLY_ADD", End(node="axes", socket="Y"), End(node="cosine"), End(node="negative_x")),
            node("rotated", bpy.types.ShaderNodeCombineXYZ, {"X": End(node="rotated_x"), "Y": End(node="rotated_y")}, {}),
            vector("lattice", "DIVIDE", End(node="rotated", socket="Vector"), pitch),
            vector("shifted", "ADD", End(node="lattice", socket="Vector"), End(node=INPUTS, socket="offset")),
            node(
                "sites",
                bpy.types.ShaderNodeTexVoronoi,
                {"Vector": End(node="shifted", socket="Vector"), "Scale": 1.0, "Detail": 0.0, "Randomness": End(node=INPUTS, socket="jitter")},
                {"voronoi_dimensions": "2D", "feature": "F1", "distance": "EUCLIDEAN", "normalize": False},
            ),
            math("squared", "MULTIPLY", End(node="sites", socket="Distance"), End(node="sites", socket="Distance")),
            math("disc", "MULTIPLY", End(node="squared"), pi),
            math("segment_radius", "MAXIMUM", End(node="sites", socket="Distance"), 0.5),
            math("segment_squared", "MULTIPLY", End(node="segment_radius"), End(node="segment_radius")),
            math("segment_ratio", "DIVIDE", 0.5, End(node="segment_radius")),
            math("segment_angle", "ARCCOSINE", End(node="segment_ratio")),
            math("segment_triangle_squared", "SUBTRACT", End(node="segment_squared"), 0.25),
            math("segment_triangle", "SQRT", End(node="segment_triangle_squared")),
            math("segment_half_triangle", "MULTIPLY", End(node="segment_triangle"), -0.5),
            math("segment", "MULTIPLY_ADD", End(node="segment_squared"), End(node="segment_angle"), End(node="segment_half_triangle")),
            math("segments", "MULTIPLY", End(node="segment"), 4.0),
            math("clipped_disc", "SUBTRACT", End(node="disc"), End(node="segments")),
            math("crosses_side", "GREATER_THAN", End(node="sites", socket="Distance"), 0.5),
            masked("square_area", "FLOAT", End(node="crosses_side"), End(node="disc"), End(node="clipped_disc")),
            math("inside_corner", "LESS_THAN", End(node="sites", socket="Distance"), sqrt(0.5)),
            masked("area", "FLOAT", End(node="inside_corner"), 1.0, End(node="square_area")),
        )
        .bind(lambda body: gridded("grid", (body, area)).bind(antialiased))
        .bind(
            lambda coverage: NodeGroup.built(
                "halftone_screen",
                "Halftone Screen",
                Interface(
                    inputs=(
                        *(Socket(key=name, name=name.title(), kind=bpy.types.NodeSocketVector) for name in ("coordinate", "offset")),
                        *(Socket(key=name, name=name.title(), kind=bpy.types.NodeSocketFloat) for name in ("angle", "pitch", "jitter", "tone")),
                    ),
                    outputs=(Socket(key="coverage", name="Coverage", kind=bpy.types.NodeSocketFloat),),
                ),
                {"coverage": coverage},
            )
        )
    )


def block_mean(key: str, frame: FrameSpace, source: Operand, length: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Averages occupied displayed-square bins through native rectangular convolution."""
    dimensions = node(f"{key}_info", bpy.types.CompositorNodeImageInfo, {"Image": source}, {}).map(ended(f"{key}_info", "Dimensions"))
    span = pixels(f"{key}_span", frame, length, "VECTOR").bind(
        lambda span: math(f"{key}_positive", "GREATER_THAN", length, 0.0).bind(lambda positive: switched(f"{key}_working_span", "VECTOR", positive, (1.0, 1.0, 0.0), span))
    )
    anchor = dimensions.bind(lambda dimensions: vector(f"{key}_anchor", "MULTIPLY", dimensions, frame.margin))
    coordinate = gridded(f"{key}_grid", source).bind(lambda grid: vector(f"{key}_centers", "DIVIDE", grid.base, grid.inverse))

    def bounded(
        prefix: str, center: Operand, span: tuple[Fragment, End], anchor: tuple[Fragment, End], dimensions: tuple[Fragment, End]
    ) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        return (
            vector(f"{prefix}_relative", "SUBTRACT", center, anchor)
            .bind(lambda relative: vector(f"{prefix}_scaled", "DIVIDE", relative, span))
            .bind(lambda scaled: vector(f"{prefix}_bin", "FLOOR", scaled))
            .bind(
                lambda cell: paired(
                    vector(f"{prefix}_start", "MULTIPLY_ADD", cell, span, anchor)
                    .bind(lambda start: vector(f"{prefix}_lower_center", "SUBTRACT", start, (0.5, 0.5, 0.0)))
                    .bind(lambda start: vector(f"{prefix}_lower_index", "CEIL", start))
                    .bind(lambda start: vector(f"{prefix}_lower", "MAXIMUM", start, (0.0,) * 3)),
                    vector(f"{prefix}_next", "ADD", cell, (1.0, 1.0, 0.0))
                    .bind(lambda following: vector(f"{prefix}_stop", "MULTIPLY_ADD", following, span, anchor))
                    .bind(lambda stop: vector(f"{prefix}_upper_center", "SUBTRACT", stop, (0.5, 0.5, 0.0)))
                    .bind(lambda stop: vector(f"{prefix}_upper_index", "CEIL", stop))
                    .bind(lambda stop: vector(f"{prefix}_upper", "MINIMUM", stop, dimensions)),
                )
            )
            .bind(lambda bounds: vector(f"{prefix}_count", "SUBTRACT", bounds[1], bounds[0]).map(lambda count: (bounds[0], count)))
        )

    def averaged(parts: tuple[tuple[Fragment, End], tuple[Fragment, End], tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
        span, anchor, dimensions, coordinate = parts
        first = bounded(f"{key}_first", (0.5, 0.5, 0.0), span, anchor, dimensions).map(lambda bounds: bounds[1])
        last = vector(f"{key}_last_center", "SUBTRACT", dimensions, (0.5, 0.5, 0.0)).bind(lambda center: bounded(f"{key}_last", center, span, anchor, dimensions)).map(lambda bounds: bounds[1])
        floor = vector(f"{key}_floor", "FLOOR", span).bind(lambda floor: vector(f"{key}_interior", "MAXIMUM", floor, (1.0, 1.0, 0.0)))
        ceiling = vector(f"{key}_ceiling", "CEIL", span)
        counts = accumulated((floor, ceiling, first, last)).bind(
            lambda counts: accumulated(node(f"{key}_candidate_{index}", bpy.types.ShaderNodeSeparateXYZ, {"Vector": count}, {}) for index, count in enumerate(counts))
        )
        bounds = bounded(f"{key}_occupied", coordinate, span, anchor, dimensions)

        def selected(candidates: tuple[Fragment, ...], bounds: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
            lower, count = bounds
            axes = node(f"{key}_count", bpy.types.ShaderNodeSeparateXYZ, {"Vector": count}, {})
            uv = (
                vector(f"{key}_half_count", "SCALE", count, 0.5)
                .bind(lambda half: vector(f"{key}_kernel_center", "FLOOR", half))
                .bind(lambda center: vector(f"{key}_sample_index", "ADD", lower, center))
                .bind(lambda center: vector(f"{key}_sample_center", "ADD", center, (0.5, 0.5, 0.0)))
                .bind(lambda center: vector(f"{key}_sample_fraction", "DIVIDE", center, dimensions))
                .bind(lambda fraction: vector(f"{key}_uv", "ADD", fraction, (0.0, 0.0, 1.0)))
            )

            def convolved(indices: tuple[int, int]) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
                x, y = indices
                prefix = f"{key}_{x}_{y}"
                horizontal = ended(f"{key}_candidate_{x}", "X")(candidates[x])
                vertical = ended(f"{key}_candidate_{y}", "Y")(candidates[y])
                size = node(f"{prefix}_size", bpy.types.ShaderNodeCombineXYZ, {"X": horizontal, "Y": vertical}, {}).map(ended(f"{prefix}_size", "Vector"))
                kernel = size.bind(lambda size: node(f"{prefix}_kernel", bpy.types.CompositorNodeBlankImage, {"Color": (1.0,) * 4, "Size": size}, {})).map(ended(f"{prefix}_kernel", "Image"))
                image = kernel.bind(
                    lambda kernel: node(f"{prefix}_convolution", bpy.types.CompositorNodeConvolve, {"Image": source, "Kernel Data Type": "Float", "Float Kernel": kernel, "Normalize Kernel": True}, {})
                ).map(ended(f"{prefix}_convolution", "Image"))
                image = (
                    paired(image, uv)
                    .bind(
                        lambda parts: node(
                            f"{prefix}_sample", bpy.types.CompositorNodeMapUV, {"Image": parts[0], "UV": parts[1], "Interpolation": "Nearest", "Extension X": "Extend", "Extension Y": "Extend"}, {}
                        )
                    )
                    .map(ended(f"{prefix}_sample", "Image"))
                )
                matches = axes.bind(
                    lambda axes: paired(
                        compared(f"{prefix}_x", "EQUAL", ended(f"{key}_count", "X")(axes), horizontal, epsilon=Some(0.0)),
                        compared(f"{prefix}_y", "EQUAL", ended(f"{key}_count", "Y")(axes), vertical, epsilon=Some(0.0)),
                    )
                ).bind(lambda matches: math(f"{prefix}_matches", "MULTIPLY", *matches))
                return paired(image, matches)

            def chosen(samples: tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], ...]) -> BlenderResult[tuple[Fragment, End]]:
                return functools.reduce(
                    lambda held, step: held.bind(lambda previous: masked(f"{key}_selected_{step[0]}", "RGBA", step[1][1], previous, step[1][0])), enumerate(samples[1:], 1), Ok(samples[0][0])
                )

            return accumulated(convolved(indices) for indices in product(range(4), range(4))).bind(chosen)

        return paired(counts, bounds).bind(lambda parts: selected(*parts))

    return paired(paired(span, anchor), paired(dimensions, coordinate)).bind(lambda parts: averaged((*parts[0], *parts[1])))


def patterned[R: Pixelation | Pattern](shell: Shell[R]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds the selected native display structure with its own color and coverage."""
    match shell.record:
        case Pixelation():
            structure_field, motion, seed = Pixelation.__struct_fields__
        case Pattern():
            structure_field, motion, seed = Pattern.__struct_fields__
    speed, hold = Motion.__struct_fields__
    structure = (structure_field,)
    clock = Clock(key="clock", speed=shell.socket(motion, speed), hold=shell.socket(motion, hold)).pattern

    def built(frame: FrameSpace, index: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        (size_mosaic,) = Mosaic.__struct_fields__
        size_cells, jitter_cells, metric_cells = Cells.__struct_fields__
        pitch_halftone, angle, jitter_halftone, inks_halftone = Halftone.__struct_fields__
        ink_mono, paper_mono = Mono.__struct_fields__
        cyan, magenta, yellow, key_process = Process.__struct_fields__
        offset_screen, scale_screen = Screen.__struct_fields__
        _, rows, jitter_glyphs, paper_glyphs, coloring_glyphs = Glyphs.__struct_fields__
        (saturation,) = RandomColor.__struct_fields__
        (ink_two_tone,) = TwoTone.__struct_fields__
        direction_sort, low_sort, high_sort, length_sort, softness_sort, falloff_sort, scatter_sort, clump, chunk, edge_fade = PixelSort.__struct_fields__
        middle = vector("middle", "SCALE", frame.extent, 0.5)
        centered = middle.bind(lambda middle: vector("centered", "SUBTRACT", frame.point, middle))
        luma = node("luma", bpy.types.CompositorNodeRGBToBW, {"Image": shell.source}, {}).map(ended("luma", "Val"))

        def drawn(key: str, noise: Noise, instance: int = 0) -> BlenderResult[tuple[Fragment, End]]:
            return origin(key, shell.socket(seed), noise, index, instance)

        def weighted(key: str, color: Operand, weight: Operand) -> BlenderResult[tuple[Fragment, End]]:
            return node(key, bpy.types.CompositorNodeSetAlpha, {"Image": color, "Alpha": weight, "Type": "Replace Alpha"}, {}).map(ended(key, "Image"))

        def filled(key: str, position: Operand) -> BlenderResult[tuple[Fragment, End]]:
            return node(key, bpy.types.CompositorNodeMapUV, {"Image": shell.source, "UV": position, "Interpolation": "Bilinear", "Extension X": "Extend", "Extension Y": "Extend"}, {}).map(
                ended(key, "Image")
            )

        if isinstance(shell.record, Pixelation):
            mosaic = block_mean("mosaic", frame, shell.source, shell.socket(*structure, Mosaic, size_mosaic))
            offset = drawn("cell_origin", Noise.CELL).bind(lambda origin: vector("cell_offset", "FLOOR", origin))
            size = shell.socket(*structure, Cells, size_cells)
            lattice = centered.bind(lambda point: vector("cell_scaled", "DIVIDE", point, size)).bind(lambda point: offset.bind(lambda offset: vector("cell_lattice", "ADD", point, offset)))
            sites = accumulated(
                lattice
                .bind(
                    lambda lattice: node(
                        f"cell_{metric.name.lower()}",
                        bpy.types.ShaderNodeTexVoronoi,
                        {"Vector": lattice, "Scale": 1.0, "Detail": 0.0, "Randomness": shell.socket(*structure, Cells, jitter_cells)},
                        {"voronoi_dimensions": "2D", "feature": "F1", "distance": metric.name, "normalize": False},
                    )
                )
                .map(ended(f"cell_{metric.name.lower()}", "Position"))
                .map(lambda position: (metric.label, position))
                for metric in Metric
            )
            site = sites.bind(lambda sites: menu("cell_site", "VECTOR", shell.socket(*structure, Cells, metric_cells), shell.items[*structure, Cells, metric_cells], dict(sites)))
            point = (
                paired(site, offset)
                .bind(lambda parts: vector("cell_local", "SUBTRACT", *parts))
                .bind(lambda local: middle.bind(lambda middle: vector("cell_point", "MULTIPLY_ADD", local, size, middle)))
            )
            cells = point.bind(lambda point: vector("cell_fraction", "DIVIDE", point, frame.extent)).bind(lambda fraction: sampled("cell_uv", frame, fraction)).bind(lambda uv: filled("cell_fill", uv))
            choices = paired(mosaic, cells).map(lambda results: {Mosaic.title: results[0], Cells.title: results[1]})
        else:
            ht = (*structure, Halftone)
            inks = (*ht, inks_halftone)
            screen = halftone_screen()

            def screened(key: str, tone: Operand, instance: int, *, offset: Operand = (0.0, 0.0, 0.0), scale: Operand = 1.0) -> BlenderResult[tuple[Fragment, End]]:
                lattice = (
                    drawn(f"{key}_origin", Noise.SCREEN, instance).bind(lambda origin: vector(f"{key}_lattice", "FLOOR", origin)).bind(lambda lattice: vector(f"{key}_offset", "ADD", lattice, offset))
                )
                pitch = math(f"{key}_pitch", "MULTIPLY", shell.socket(*ht, pitch_halftone), scale)
                return paired(paired(lattice, pitch), centered).bind(
                    lambda parts: screen.bind(
                        lambda group: nested(
                            key,
                            group,
                            {"coordinate": parts[1], "offset": parts[0][0], "pitch": parts[0][1], "angle": shell.socket(*ht, angle), "jitter": shell.socket(*ht, jitter_halftone), "tone": tone},
                            "coverage",
                        )
                    )
                )

            mono = luma.bind(lambda luma: math("mono_tone", "SUBTRACT", 1.0, luma)).bind(lambda tone: screened("mono", tone, 0))
            mono = mono.bind(
                lambda coverage: mixed("mono_print", "RGBA", coverage, shell.socket(*inks, Mono, paper_mono), shell.socket(*inks, Mono, ink_mono)).bind(
                    lambda color: weighted("mono_weighted", color, coverage)
                )
            )
            channels = node("halftone_channels", bpy.types.CompositorNodeSeparateColor, {"Image": shell.source}, {"mode": "RGB"})
            peak = channels.bind(lambda channels: math("peak_rg", "MAXIMUM", ended("halftone_channels", "Red")(channels), End(node="halftone_channels", socket="Green"))).bind(
                lambda peak: math("peak", "MAXIMUM", peak, End(node="halftone_channels", socket="Blue"))
            )
            separations = peak.bind(lambda peak: vector("absent_ink", "SUBTRACT", peak, shell.source).bind(lambda absent: vector("process_ink", "DIVIDE", absent, peak))).bind(
                lambda ink: node("ink_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": ink}, {})
            )
            black = peak.bind(lambda peak: math("black_ink", "SUBTRACT", 1.0, peak))
            tones = separations.map(lambda axes: ((cyan, ended("ink_axes", "X")(axes)), (magenta, (axes, End(node="ink_axes", socket="Y"))), (yellow, (axes, End(node="ink_axes", socket="Z"))))).map2(
                black, lambda colors, black: (*colors, (key_process, black))
            )
            process = tones.bind(
                lambda tones: accumulated(
                    screened(f"process_{field}", tone, instance, offset=shell.socket(*inks, Process, field, offset_screen), scale=shell.socket(*inks, Process, field, scale_screen)).bind(
                        lambda coverage: math(f"process_{field}_clear", "SUBTRACT", 1.0, coverage)
                    )
                    for instance, (field, tone) in enumerate(tones)
                )
            )

            def printed(clear: tuple[tuple[Fragment, End], ...]) -> BlenderResult[tuple[Fragment, End]]:
                color = (
                    node("process_clear", bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y", "Z"), clear[:3])), {})
                    .map(ended("process_clear", "Vector"))
                    .bind(lambda color: vector("process_print", "SCALE", color, clear[3]))
                )
                paper = (
                    math("process_paper_cm", "MULTIPLY", clear[0], clear[1])
                    .bind(lambda paper: math("process_paper_cmy", "MULTIPLY", paper, clear[2]))
                    .bind(lambda paper: math("process_paper", "MULTIPLY", paper, clear[3]))
                    .bind(lambda paper: math("process_coverage", "SUBTRACT", 1.0, paper))
                )
                return paired(color, paper).bind(lambda parts: weighted("process_weighted", *parts))

            halftone = paired(mono, process.bind(printed)).bind(lambda prints: menu("halftone", "RGBA", shell.socket(*inks), shell.items[inks], {Mono.title: prints[0], Process.title: prints[1]}))

            gl = (*structure, Glyphs)
            glyphs = next(case for case in shell.cases[structure] if isinstance(case, Glyphs)).glyphs
            levels = len(glyphs)
            height = (
                node("glyph_extent", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.extent}, {})
                .bind(lambda extent: math("glyph_short", "MINIMUM", ended("glyph_extent", "X")(extent), End(node="glyph_extent", socket="Y")))
                .bind(lambda short: math("glyph_height", "DIVIDE", short, shell.socket(*gl, rows)))
            )
            strip = (
                height
                .bind(lambda height: math("glyph_size", "MULTIPLY", height, frame.greater))
                .bind(lambda size: node("glyph_strip", bpy.types.CompositorNodeStringToImage, {"String": "\n".join(glyphs), "Size": size, "Wrap": False}, {}))
                .map(ended("glyph_strip", "Image"))
            )
            ratio = (
                strip
                .bind(lambda strip: node("glyph_strip_info", bpy.types.CompositorNodeImageInfo, {"Image": strip}, {}))
                .bind(lambda info: node("glyph_strip_sides", bpy.types.ShaderNodeSeparateXYZ, {"Vector": ended("glyph_strip_info", "Dimensions")(info)}, {}))
                .bind(lambda sides: math("glyph_strip_ratio", "DIVIDE", ended("glyph_strip_sides", "X")(sides), End(node="glyph_strip_sides", socket="Y")))
                .bind(lambda ratio: math("glyph_ratio", "MULTIPLY", ratio, levels))
            )
            cell = (
                paired(height, ratio)
                .bind(lambda parts: math("glyph_width", "MULTIPLY", *parts).bind(lambda width: node("glyph_cell", bpy.types.ShaderNodeCombineXYZ, {"X": width, "Y": parts[0], "Z": 1.0}, {})))
                .map(ended("glyph_cell", "Vector"))
            )
            coordinates = cell.bind(lambda cell: vector("glyph_cells", "DIVIDE", frame.point, cell))
            indices = coordinates.bind(lambda coordinates: vector("glyph_index", "FLOOR", coordinates))
            within = coordinates.bind(lambda coordinates: vector("glyph_within", "FRACTION", coordinates)).bind(
                lambda within: node("glyph_within_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": within}, {})
            )
            center = (
                indices
                .bind(lambda indices: vector("glyph_middle", "ADD", indices, (0.5, 0.5, 0.0)))
                .bind(lambda center: cell.bind(lambda cell: vector("glyph_middle_point", "MULTIPLY", center, cell)))
                .bind(lambda point: vector("glyph_middle_fraction", "DIVIDE", point, frame.extent))
                .bind(lambda fraction: sampled("glyph_center", frame, fraction))
                .bind(lambda uv: filled("glyph_sample", uv))
            )
            draw = (
                paired(indices, drawn("glyph_origin", Noise.GLYPH))
                .bind(lambda parts: vector("glyph_key", "ADD", *parts))
                .bind(lambda point: node("glyph_draw", bpy.types.ShaderNodeTexWhiteNoise, {"Vector": point}, {"noise_dimensions": "3D"}))
            )
            jitter = draw.bind(lambda draw: math("glyph_jitter", "SUBTRACT", ended("glyph_draw", "Value")(draw), 0.5))
            tone = center.bind(lambda center: node("glyph_luma", bpy.types.CompositorNodeRGBToBW, {"Image": center}, {})).bind(
                lambda luma: jitter.bind(lambda jitter: math("glyph_tone", "MULTIPLY_ADD", jitter, shell.socket(*gl, jitter_glyphs), ended("glyph_luma", "Val")(luma), clamp=True))
            )
            rank = (
                tone
                .bind(lambda tone: math("glyph_scaled", "MULTIPLY", tone, levels))
                .bind(lambda tone: math("glyph_floor", "FLOOR", tone))
                .bind(lambda level: math("glyph_level", "MINIMUM", level, levels - 1))
                .bind(lambda level: math("glyph_rank", "SUBTRACT", levels - 1, level))
            )
            uv = (
                paired(rank, within)
                .bind(
                    lambda parts: (
                        math("glyph_line", "ADD", parts[0], ended("glyph_within_axes", "Y")(parts[1]))
                        .bind(lambda line: math("glyph_v", "DIVIDE", line, levels))
                        .bind(lambda y: node("glyph_uv", bpy.types.ShaderNodeCombineXYZ, {"X": ended("glyph_within_axes", "X")(parts[1]), "Y": y, "Z": 1.0}, {}))
                    )
                )
                .map(ended("glyph_uv", "Vector"))
            )
            mask = (
                paired(strip, uv)
                .bind(
                    lambda parts: node(
                        "glyph_mask", bpy.types.CompositorNodeMapUV, {"Image": parts[0], "UV": parts[1], "Interpolation": "Bilinear", "Extension X": "Extend", "Extension Y": "Extend"}, {}
                    )
                )
                .map(ended("glyph_mask", "Image"))
            )
            coloring = (*gl, coloring_glyphs)
            random = (
                draw
                .bind(lambda draw: node("glyph_hue", bpy.types.ShaderNodeSeparateXYZ, {"Vector": ended("glyph_draw", "Color")(draw)}, {}))
                .bind(
                    lambda hue: node(
                        "glyph_random",
                        bpy.types.CompositorNodeCombineColor,
                        {"Red": ended("glyph_hue", "X")(hue), "Green": shell.socket(*coloring, RandomColor, saturation), "Blue": 1.0, "Alpha": 1.0},
                        {"mode": "HSV"},
                    )
                )
                .map(ended("glyph_random", "Image"))
            )
            color = paired(center, random).bind(
                lambda colors: menu(
                    "glyph_coloring",
                    "RGBA",
                    shell.socket(*coloring),
                    shell.items[coloring],
                    {SourceColor.title: colors[0], TwoTone.title: shell.socket(*coloring, TwoTone, ink_two_tone), RandomColor.title: colors[1]},
                )
            )
            glyph = paired(mask, color).bind(
                lambda parts: mixed("glyph_print", "RGBA", parts[0], shell.socket(*gl, paper_glyphs), parts[1]).bind(lambda color: weighted("glyph_weighted", color, parts[0]))
            )

            ps = (*structure, PixelSort)
            headings = {Direction.LEFT: (-1.0, 0.0, 0.0), Direction.RIGHT: (1.0, 0.0, 0.0), Direction.UP: (0.0, 1.0, 0.0), Direction.DOWN: (0.0, -1.0, 0.0)}
            heading = menu("sort_heading", "VECTOR", shell.socket(*ps, direction_sort), shell.items[*ps, direction_sort], {direction.label: axis for direction, axis in headings.items()})
            axis = heading.bind(lambda heading: vector("sort_axis", "ABSOLUTE", heading))
            cross = axis.bind(lambda axis: vector("sort_cross", "SUBTRACT", (1.0, 1.0, 0.0), axis))
            softness = axis.bind(lambda axis: vector("sort_softness", "SCALE", axis, shell.socket(*ps, softness_sort))).bind(lambda length: pixels("sort_softness_size", frame, length, "VECTOR"))
            soft = (
                paired(luma, softness)
                .bind(lambda parts: node("sort_soft", bpy.types.CompositorNodeBlur, {"Image": parts[0], "Size": parts[1], "Type": "Gaussian"}, {}))
                .map(ended("sort_soft", "Image"))
            )
            low = soft.bind(lambda tone: math("sort_less_low", "LESS_THAN", tone, shell.socket(*ps, low_sort))).bind(lambda less: math("sort_ge_low", "SUBTRACT", 1.0, less))
            high = soft.bind(lambda tone: math("sort_greater_high", "GREATER_THAN", tone, shell.socket(*ps, high_sort))).bind(lambda greater: math("sort_le_high", "SUBTRACT", 1.0, greater))
            band = (
                paired(low, high)
                .bind(lambda ends: paired(math("sort_inside", "MULTIPLY", *ends), math("sort_outside", "MAXIMUM", *ends)))
                .bind(lambda bands: math("sort_wrapped", "GREATER_THAN", shell.socket(*ps, low_sort), shell.socket(*ps, high_sort)).bind(lambda wrapped: mixed("sort_key", "FLOAT", wrapped, *bands)))
            )
            falloff = axis.bind(lambda axis: vector("sort_falloff", "SCALE", axis, shell.socket(*ps, falloff_sort))).bind(lambda length: pixels("sort_falloff_size", frame, length, "VECTOR"))
            fall = (
                paired(band, falloff)
                .bind(lambda parts: node("sort_fall", bpy.types.CompositorNodeBlur, {"Image": parts[0], "Size": parts[1], "Type": "Gaussian"}, {}))
                .map(ended("sort_fall", "Image"))
            )
            scatter = (
                drawn("sort_scatter_origin", Noise.SCATTER)
                .bind(lambda origin: vector("sort_grain_at", "ADD", frame.point, origin))
                .bind(lambda point: node("sort_grain", bpy.types.ShaderNodeTexWhiteNoise, {"Vector": point}, {"noise_dimensions": "2D"}))
                .bind(lambda grain: math("sort_scatter", "MULTIPLY", ended("sort_grain", "Value")(grain), shell.socket(*ps, scatter_sort)))
                .bind(lambda scatter: math("sort_kept", "SUBTRACT", 1.0, scatter))
            )
            across = cross.bind(lambda axis: vector("sort_across", "DOT_PRODUCT", frame.fraction, axis))
            shift = (
                drawn("sort_streak_origin", Noise.STREAK).bind(lambda origin: node("sort_streak_shift", bpy.types.ShaderNodeSeparateXYZ, {"Vector": origin}, {})).map(ended("sort_streak_shift", "X"))
            )
            streak = (
                paired(across, luma)
                .bind(lambda parts: math("sort_tone_offset", "MULTIPLY_ADD", parts[1], 2.0, -1.0).bind(lambda tone: math("sort_streak_across", "ADD", parts[0], tone)))
                .bind(lambda across: shift.bind(lambda shift: math("sort_streak_at", "ADD", across, shift)))
                .bind(
                    lambda at: node("sort_streak", bpy.types.ShaderNodeTexNoise, {"W": at, "Roughness": shell.socket(*ps, clump)}, {"noise_dimensions": "1D", "noise_type": "FBM", "normalize": True})
                )
                .map(ended("sort_streak", "Fac"))
            )
            field = (
                paired(fall, scatter)
                .bind(lambda parts: math("sort_keyed", "MULTIPLY", *parts))
                .bind(lambda keyed: streak.bind(lambda streak: math("sort_unchunked", "MULTIPLY", keyed, streak)))
                .bind(lambda field: block_mean("sort_chunk", frame, field, shell.socket(*ps, chunk)))
            )
            along = axis.bind(lambda axis: vector("sort_along", "DOT_PRODUCT", frame.fraction, axis))
            fade = (
                along
                .bind(lambda along: math("sort_from_end", "SUBTRACT", 1.0, along).bind(lambda other: math("sort_end", "MINIMUM", along, other)))
                .bind(
                    lambda distance: node(
                        "sort_fade", bpy.types.ShaderNodeMapRange, {"Value": distance, "From Min": shell.socket(*ps, edge_fade), "From Max": 0.0, "To Min": 1.0, "To Max": 0.0}, {"clamp": True}
                    )
                )
                .map(ended("sort_fade", "Result"))
            )
            weight = paired(field, fade).bind(lambda parts: math("sort_map", "MULTIPLY", *parts))
            displacement = (
                heading
                .bind(lambda heading: vector("sort_reach", "SCALE", heading, shell.socket(*ps, length_sort)))
                .bind(lambda reach: weight.bind(lambda weight: vector("sort_travel", "SCALE", reach, weight)))
                .bind(lambda displacement: pixels("sort_displacement", frame, displacement, "VECTOR"))
            )
            sorted_image = displacement.bind(
                lambda displacement: node(
                    "sort_displaced",
                    bpy.types.CompositorNodeDisplace,
                    {"Image": shell.source, "Displacement": displacement, "Interpolation": "Bilinear", "Extension X": "Extend", "Extension Y": "Extend"},
                    {},
                )
            ).map(ended("sort_displaced", "Image"))
            sort = paired(sorted_image, weight).bind(lambda parts: mixed("sort_print", "RGBA", parts[1], shell.source, parts[0]).bind(lambda color: weighted("sort_weighted", color, parts[1])))
            choices = accumulated((halftone, glyph, sort)).map(lambda results: {Halftone.title: results[0], Glyphs.title: results[1], PixelSort.title: results[2]})

        result = choices.bind(lambda choices: menu("structure", "RGBA", shell.socket(*structure), shell.items[structure], choices))
        return result.bind(
            lambda result: node("structure_weight", bpy.types.CompositorNodeSeparateColor, {"Image": (1.0,) * 4 if isinstance(shell.record, Pixelation) else result}, {"mode": "RGB"}).bind(
                lambda weight: Fragment.merged((result[0], weight)).map(lambda joined: (joined, result[1]))
            )
        )

    return paired(framed("frame", shell.source, shell.aspect, shell.scale), clock).bind(lambda parts: built(*parts))


# --- [COMPOSITION] ----------------------------------------------------------------------

pixelation: CompositorEffect[Pixelation] = CompositorEffect(
    record=Pixelation,
    graph=patterned,
    coverage=Coverage.RESULT,
    requested=Some(precise),
    registrables=(inspection(lambda: pixelation, "pixelation_weight", "Pixelation Weight", "Inspect the structure's replacement weight", lambda: End(node="structure_weight", socket="Alpha")),),
)
pattern: CompositorEffect[Pattern] = CompositorEffect(
    record=Pattern,
    graph=patterned,
    straight=True,
    requested=Some(precise),
    registrables=(inspection(lambda: pattern, "pattern_weight", "Pattern Weight", "Inspect the selected structure's coverage", lambda: End(node="structure_weight", socket="Alpha")),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Noise", "block_mean", "halftone_screen", "pattern", "patterned", "pixelation"]
