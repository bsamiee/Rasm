"""Screen coverage and sampled structures for display stylization."""

from collections.abc import Mapping
from enum import IntEnum
import functools
from itertools import product, starmap
from math import pi, sqrt
from types import MappingProxyType
from typing import Annotated, Final

import bpy
from expression import Ok, Some
import msgspec

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, grouped, math, mixed, Scalar, switched, vector, where, xyz
from blender.bpy.render.effects.coordinates import clocked, FrameSpace, gridded, origin, pixels, sampled, shorter
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Inspected, precise, Shell
from blender.imaging.effects.stylize.patterns import Cells, Direction, Glyphs, Halftone, Metric, Mono, Mosaic, Pattern, Pixelation, PixelSort, Process, RandomColor, Screen, SourceColor, TwoTone
from blender.imaging.parameters import Motion

# --- [TYPES] ----------------------------------------------------------------------------


class Noise(IntEnum):
    """Persistent decorrelation keys for independent pattern draws."""

    SCREEN = 0
    GLYPH = 1
    CELL = 2
    SCATTER = 3
    STREAK = 4


# --- [CONSTANTS] ------------------------------------------------------------------------

_HEADINGS: Final = MappingProxyType({Direction.LEFT: (-1.0, 0.0, 0.0), Direction.RIGHT: (1.0, 0.0, 0.0), Direction.UP: (0.0, 1.0, 0.0), Direction.DOWN: (0.0, -1.0, 0.0)})

# --- [OPERATIONS] -----------------------------------------------------------------------


@grouped("halftone_screen", "Halftone Screen")
def halftone_screen(
    coordinate: Annotated[Expr, bpy.types.NodeSocketVector], offset: Annotated[Expr, bpy.types.NodeSocketVector], angle: Expr, pitch: Expr, jitter: Expr, tone: Expr
) -> Mapping[Key, Expr]:
    """Unit-square ink coverage of a rotated dot lattice, antialiased over pixel derivative width.

    Args:
        coordinate: Displayed point about frame center.
        offset: Lattice shift in cells.
        angle: Screen turn.
        pitch: Lattice spacing.
        jitter: Dot displacement within its cell.
        tone: Ink share each cell covers.

    Returns:
        Coverage output.
    """
    cosine, sine = math("COSINE", angle), math("SINE", angle)
    x, y = coordinate["X"], coordinate["Y"]
    lattice = vector("DIVIDE", xyz(math("MULTIPLY_ADD", x, cosine, y * sine), math("MULTIPLY_ADD", y, cosine, -(x * sine))), pitch) + offset
    sites = Expr(
        kind=bpy.types.ShaderNodeTexVoronoi,
        inputs=(("Vector", lattice), ("Scale", 1.0), ("Detail", 0.0), ("Randomness", jitter)),
        members=(("voronoi_dimensions", "2D"), ("feature", "F1"), ("distance", "EUCLIDEAN"), ("normalize", False)),
        socket="Distance",
    )
    disc = sites * sites * pi
    radius = math("MAXIMUM", sites, 0.5)
    segment = math("MULTIPLY_ADD", radius * radius, math("ARCCOSINE", 0.5 / radius), math("SQRT", radius * radius - 0.25) * -0.5)
    area = where(sites < sqrt(0.5), where(math("GREATER_THAN", sites, 0.5), disc - segment * 4.0, disc), 1.0)
    grid = gridded(area)

    def slope(step: tuple[float, float, float]) -> Expr:
        neighbour = Expr(
            kind=bpy.types.CompositorNodeMapUV,
            inputs=(("Image", area), ("UV", grid.at(step, grid.inverse)), ("Interpolation", "Nearest"), ("Extension X", "Extend"), ("Extension Y", "Extend")),
            socket="Image",
            shape="RGBA",
        )
        return abs(math("SUBTRACT", neighbour, area))

    width = slope((1.0, 0.0, 0.0)) + slope((0.0, 1.0, 0.0))
    inside = where(width > 0.0, math("ADD", tone, (tone - area) / width, clamp=True), area < tone)
    return {"coverage": (tone > 0.0) * where(tone < 1.0, inside, 1.0)}


def block_mean(frame: FrameSpace, source: Scalar, length: Scalar) -> Expr:
    """Averages occupied displayed-square bins through rectangular convolution.

    Args:
        frame: Displayed camera placement within the source data window.
        source: Image averaged.
        length: Bin side in displayed units, single pixels at 0.

    Returns:
        Bin mean of each pixel's bin.
    """
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", source),), socket="Dimensions", shape="VECTOR")
    span = switched(math("GREATER_THAN", length, 0.0), (1.0, 1.0, 0.0), pixels(frame, length, "VECTOR"))
    anchor = vector("MULTIPLY", dimensions, frame.margin)
    grid = gridded(source)

    def bounded(center: Scalar | tuple[float, ...]) -> tuple[Expr, Expr]:
        cell = vector("FLOOR", vector("DIVIDE", vector("SUBTRACT", center, anchor), span))
        lower = vector("MAXIMUM", vector("CEIL", vector("MULTIPLY_ADD", cell, span, anchor) - (0.5, 0.5, 0.0)), (0.0, 0.0, 0.0))
        upper = vector("MINIMUM", vector("CEIL", vector("MULTIPLY_ADD", vector("ADD", cell, (1.0, 1.0, 0.0)), span, anchor) - (0.5, 0.5, 0.0)), dimensions)
        return lower, upper - lower

    candidates = (vector("MAXIMUM", vector("FLOOR", span), (1.0, 1.0, 0.0)), vector("CEIL", span), bounded((0.5, 0.5, 0.0))[1], bounded(dimensions - (0.5, 0.5, 0.0))[1])
    lower, count = bounded(vector("DIVIDE", grid.base, grid.inverse))
    uv = vector("ADD", vector("DIVIDE", vector("ADD", lower + vector("FLOOR", vector("SCALE", count, 0.5)), (0.5, 0.5, 0.0)), dimensions), (0.0, 0.0, 1.0))

    def convolved(x: int, y: int) -> tuple[Expr, Expr]:
        horizontal, vertical = candidates[x]["X"], candidates[y]["Y"]
        kernel = Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Color", (1.0,) * 4), ("Size", xyz(horizontal, vertical))), socket="Image", shape="RGBA")
        image = Expr(
            kind=bpy.types.CompositorNodeConvolve, inputs=(("Image", source), ("Kernel Data Type", "Float"), ("Float Kernel", kernel), ("Normalize Kernel", True)), socket="Image", shape="RGBA"
        )
        sample = Expr(
            kind=bpy.types.CompositorNodeMapUV, inputs=(("Image", image), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", "Extend"), ("Extension Y", "Extend")), socket="Image", shape="RGBA"
        )
        return sample, math("COMPARE", count["X"], horizontal, 0.0) * math("COMPARE", count["Y"], vertical, 0.0)

    (first, _), *rest = starmap(convolved, product(range(4), repeat=2))
    return functools.reduce(lambda held, step: where(step[1], step[0], held), rest, first)


def _drawn(shell: Shell[Pixelation] | Shell[Pattern], noise: Noise, instance: int = 0) -> Expr:
    """Decorrelation origin of one draw, redrawn per whole clock frame."""
    _, motion, seed = type(shell.record).__struct_fields__
    speed, hold = Motion.__struct_fields__
    return origin(shell.input(seed), noise, math("FLOOR", clocked(shell.input(motion, speed), shell.input(motion, hold))), instance)


def _filled(shell: Shell[Pixelation] | Shell[Pattern], uv: Expr) -> Expr:
    """Bilinear entry color at Map UV positions."""
    return Expr(
        kind=bpy.types.CompositorNodeMapUV,
        inputs=(("Image", shell.color), ("UV", uv), ("Interpolation", "Bilinear"), ("Extension X", "Extend"), ("Extension Y", "Extend")),
        socket="Image",
        shape="RGBA",
    )


def _weighted(color: Scalar, weight: Scalar) -> Expr:
    """Color carrying weight as its alpha."""
    return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", color), ("Alpha", weight), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")


def structured(shell: Shell[Pixelation] | Shell[Pattern], choices: Mapping[str, Expr], *, covered: bool) -> Expr:
    """Selects the structure color keeping its replacement weight.

    Args:
        shell: Entry inputs and structure menu.
        choices: Color per structure case title.
        covered: Whether the structure's alpha weighs the replacement, else the whole frame is replaced.

    Returns:
        Selected structure keeping its weight.
    """
    structure, _, _ = type(shell.record).__struct_fields__
    result = shell.menu(structure, cases=choices, kind="RGBA")
    weight = Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", result if covered else (1.0,) * 4),), members=(("mode", "RGB"),), socket="Alpha").named("structure_weight")
    return result.keeping(weight)


def pixelated(shell: Shell[Pixelation]) -> BlenderResult[Expr]:
    """Builds mosaic blocks and Voronoi cells, each replacing the whole frame.

    Args:
        shell: Entry inputs and retained structure cases.

    Returns:
        Selected structure keeping its weight.
    """
    structure, _, _ = Pixelation.__struct_fields__
    (block,) = Mosaic.__struct_fields__
    size, jitter, metric = Cells.__struct_fields__
    frame = shell.space

    def built(case: Mosaic | Cells) -> Expr:
        match case:
            case Mosaic():
                return block_mean(frame, shell.color, shell.input(structure, Mosaic, block))
            case Cells():
                offset, middle, side = vector("FLOOR", _drawn(shell, Noise.CELL)), vector("SCALE", frame.extent, 0.5), shell.input(structure, Cells, size)
                lattice = (frame.point - middle) / side + offset
                sites = {
                    choice.label: Expr(
                        kind=bpy.types.ShaderNodeTexVoronoi,
                        inputs=(("Vector", lattice), ("Scale", 1.0), ("Detail", 0.0), ("Randomness", shell.input(structure, Cells, jitter))),
                        members=(("voronoi_dimensions", "2D"), ("feature", "F1"), ("distance", choice.name), ("normalize", False)),
                        socket="Position",
                        shape="VECTOR",
                    )
                    for choice in Metric
                }
                site = shell.menu(structure, Cells, metric, cases=sites, kind="VECTOR")
                return _filled(shell, sampled(frame, vector("MULTIPLY_ADD", site - offset, side, middle) / frame.extent))

    return Ok(structured(shell, {case.title: built(case) for case in shell.cases[structure,] if isinstance(case, Mosaic | Cells)}, covered=False))


def patterned(shell: Shell[Pattern]) -> BlenderResult[Expr]:
    """Builds halftone screens, tone-ranked glyphs, and pixel sort streaks, each with its own coverage.

    Args:
        shell: Entry inputs and retained structure cases.

    Returns:
        Selected structure keeping its weight.
    """
    structure, _, _ = Pattern.__struct_fields__
    pitch, angle, jitter, inks = Halftone.__struct_fields__
    ink, paper = Mono.__struct_fields__
    shift, scale = Screen.__struct_fields__
    cyan, magenta, yellow, black = Process.__struct_fields__
    _, rows, glyph_jitter, glyph_paper, coloring = Glyphs.__struct_fields__
    (saturation,) = RandomColor.__struct_fields__
    (tint,) = TwoTone.__struct_fields__
    direction, low, high, length, softness, falloff, scatter, clump, chunk, edge_fade = PixelSort.__struct_fields__
    source, frame = shell.color, shell.space
    centered = frame.point - vector("SCALE", frame.extent, 0.5)

    def toned(image: Scalar) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeRGBToBW, inputs=(("Image", image),), socket="Val")

    def halftoned() -> Expr:
        at = (structure, Halftone)

        def screened(tone: Expr, instance: int, offset: Scalar | tuple[float, ...] = (0.0, 0.0, 0.0), multiple: Scalar = 1.0) -> Expr:
            return halftone_screen(
                coordinate=centered,
                offset=vector("ADD", vector("FLOOR", _drawn(shell, Noise.SCREEN, instance)), offset),
                angle=shell.input(*at, angle),
                pitch=math("MULTIPLY", shell.input(*at, pitch), multiple),
                jitter=shell.input(*at, jitter),
                tone=tone,
            )

        mono = screened(1.0 - toned(source), 0)
        peak = math("MAXIMUM", math("MAXIMUM", source["Red"], source["Green"]), source["Blue"])
        separations = vector("DIVIDE", vector("SUBTRACT", peak, source), peak)
        tones = ((cyan, separations["X"]), (magenta, separations["Y"]), (yellow, separations["Z"]), (black, 1.0 - peak))
        clear = tuple(1.0 - screened(tone, instance, shell.input(*at, inks, Process, field, shift), shell.input(*at, inks, Process, field, scale)) for instance, (field, tone) in enumerate(tones))
        cases = {
            Mono.title: _weighted(mixed(mono, shell.input(*at, inks, Mono, paper), shell.input(*at, inks, Mono, ink)), mono),
            Process.title: _weighted(vector("SCALE", xyz(*clear[:3]), clear[3]), 1.0 - clear[0] * clear[1] * clear[2] * clear[3]),
        }
        return shell.menu(*at, inks, cases=cases, kind="RGBA")

    def glyphed(glyphs: str) -> Expr:
        at, levels = (structure, Glyphs), len(glyphs)
        height = shorter(frame.extent) / shell.input(*at, rows)
        strip = Expr(kind=bpy.types.CompositorNodeStringToImage, inputs=(("String", "\n".join(glyphs)), ("Size", height * frame.greater), ("Wrap", False)), socket="Image", shape="RGBA")
        sides = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", strip),), socket="Dimensions", shape="VECTOR")
        cell = xyz(height * (sides["X"] / sides["Y"] * levels), height, 1.0)
        indices, within = vector("FLOOR", frame.point / cell), vector("FRACTION", frame.point / cell)
        center = _filled(shell, sampled(frame, vector("ADD", indices, (0.5, 0.5, 0.0)) * cell / frame.extent))
        draw = Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", indices + _drawn(shell, Noise.GLYPH)),), members=(("noise_dimensions", "3D"),))
        tone = math("MULTIPLY_ADD", draw - 0.5, shell.input(*at, glyph_jitter), toned(center), clamp=True)
        rank = (levels - 1) - math("MINIMUM", math("FLOOR", tone * levels), levels - 1)
        mask = Expr(
            kind=bpy.types.CompositorNodeMapUV,
            inputs=(("Image", strip), ("UV", xyz(within["X"], (rank + within["Y"]) / levels, 1.0)), ("Interpolation", "Bilinear"), ("Extension X", "Extend"), ("Extension Y", "Extend")),
            socket="Image",
            shape="RGBA",
        )
        hue = msgspec.structs.replace(draw, socket="Color", shape="RGBA")["X"]
        random = Expr(
            kind=bpy.types.CompositorNodeCombineColor,
            inputs=(("Red", hue), ("Green", shell.input(*at, coloring, RandomColor, saturation)), ("Blue", 1.0), ("Alpha", 1.0)),
            members=(("mode", "HSV"),),
            socket="Image",
            shape="RGBA",
        )
        color = shell.menu(*at, coloring, cases={SourceColor.title: center, TwoTone.title: shell.input(*at, coloring, TwoTone, tint), RandomColor.title: random}, kind="RGBA")
        return _weighted(mixed(mask, shell.input(*at, glyph_paper), color), mask)

    def sorting() -> Expr:
        at = (structure, PixelSort)
        heading = shell.menu(*at, direction, cases={choice.label: axis for choice, axis in _HEADINGS.items()}, kind="VECTOR")
        axis = abs(heading)

        def blurred(image: Scalar, reach: str) -> Expr:
            return Expr(
                kind=bpy.types.CompositorNodeBlur,
                inputs=(("Image", image), ("Size", pixels(frame, vector("SCALE", axis, shell.input(*at, reach)), "VECTOR")), ("Type", "Gaussian")),
                socket="Image",
                shape="RGBA",
            )

        soft = blurred(toned(source), softness)
        above, below = 1.0 - (soft < shell.input(*at, low)), 1.0 - (soft > shell.input(*at, high))
        band = mixed(shell.input(*at, low) > shell.input(*at, high), above * below, math("MAXIMUM", above, below))
        grain = Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", frame.point + _drawn(shell, Noise.SCATTER)),), members=(("noise_dimensions", "2D"),))
        streak = Expr(
            kind=bpy.types.ShaderNodeTexNoise,
            inputs=(
                ("W", vector("DOT_PRODUCT", frame.fraction, (1.0, 1.0, 0.0) - axis) + math("MULTIPLY_ADD", toned(source), 2.0, -1.0) + _drawn(shell, Noise.STREAK)["X"]),
                ("Roughness", shell.input(*at, clump)),
            ),
            members=(("noise_dimensions", "1D"), ("noise_type", "FBM"), ("normalize", True)),
            socket="Fac",
        )
        along = vector("DOT_PRODUCT", frame.fraction, axis)
        fade = Expr(
            kind=bpy.types.ShaderNodeMapRange,
            inputs=(("Value", math("MINIMUM", along, 1.0 - along)), ("From Min", shell.input(*at, edge_fade)), ("From Max", 0.0), ("To Min", 1.0), ("To Max", 0.0)),
            members=(("clamp", True),),
            socket="Result",
        )
        weight = math("MULTIPLY", block_mean(frame, math("MULTIPLY", blurred(band, falloff), 1.0 - grain * shell.input(*at, scatter)) * streak, shell.input(*at, chunk)), fade)
        displaced = Expr(
            kind=bpy.types.CompositorNodeDisplace,
            inputs=(
                ("Image", source),
                ("Displacement", pixels(frame, vector("SCALE", vector("SCALE", heading, shell.input(*at, length)), weight), "VECTOR")),
                ("Interpolation", "Bilinear"),
                ("Extension X", "Extend"),
                ("Extension Y", "Extend"),
            ),
            socket="Image",
            shape="RGBA",
        )
        return _weighted(mixed(weight, source, displaced), weight)

    def built(case: Halftone | Glyphs | PixelSort) -> Expr:
        match case:
            case Halftone():
                return halftoned()
            case Glyphs(glyphs=glyphs):
                return glyphed(glyphs)
            case PixelSort():
                return sorting()

    return Ok(structured(shell, {case.title: built(case) for case in shell.cases[structure,] if isinstance(case, Halftone | Glyphs | PixelSort)}, covered=True))


# --- [COMPOSITION] ----------------------------------------------------------------------

pixelation = CompositorEffect(
    record=Pixelation,
    graph=pixelated,
    alpha=Alpha.RESULT,
    requested=Some(precise),
    inspected=(Inspected(key="pixelation_weight", end=End(node="structure_weight", socket="Alpha"), description="Structure replacement weight"),),
)
pattern = CompositorEffect(
    record=Pattern,
    graph=patterned,
    alpha=Alpha.STRAIGHT,
    requested=Some(precise),
    inspected=(Inspected(key="pattern_weight", end=End(node="structure_weight", socket="Alpha"), description="Selected structure coverage"),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Noise", "block_mean", "halftone_screen", "pattern", "patterned", "pixelated", "pixelation", "structured"]
