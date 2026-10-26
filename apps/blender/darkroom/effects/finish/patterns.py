"""Display structures: mosaic blocks and Voronoi cells resampling the frame, then halftone screens, tone-ranked glyphs, and pixel sort streaks replacing it."""

from collections.abc import Mapping
from typing import Annotated, Literal

import bpy
from expression import Ok, Some
import msgspec
import numpy as np

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, grouped, math, mixed, Scalar, vector, where, xyz
from blender.bpy.nodes.textures import stream
from blender.bpy.render.compositor.coordinates import Clock, clocked, gridded, origin, pixels, sampled, shorter
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Inspects, precise, Requests, Shell
from blender.bpy.render.compositor.filters import block_mean
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.color.processors import Domain
from blender.imaging.geometry.regions import FrameExtent, FrameLength
from blender.records.fields import Case, Choice, Finite, Positive, Presentation, Quantity, Rgb, Seed, Turn, Unit

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------


class Direction(Choice):
    """Pixel sort streak direction with its frame heading."""

    heading: tuple[float, float, float]

    def __init__(self, value: int, heading: tuple[float, float, float]) -> None:
        """Sets the unit frame heading of the direction."""
        super().__init__(value)
        self.heading = heading

    LEFT = 1, (-1.0, 0.0, 0.0)
    RIGHT = 2, (1.0, 0.0, 0.0)
    UP = 3, (0.0, 1.0, 0.0)
    DOWN = 4, (0.0, -1.0, 0.0)


type Inks = Mono | Process
type Coloring = SourceColor | TwoTone | RandomColor
type Structure = Halftone | Glyphs | PixelSort

# --- [MODELS] ---------------------------------------------------------------------------


class Screen(msgspec.Struct, frozen=True, kw_only=True):
    """Lattice shift and pitch multiple of one process screen."""

    offset: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Lattice shift in cells")] = (0.0, 0.0)
    scale: Annotated[Positive, msgspec.Meta(description="Multiple of halftone pitch"), Presentation(soft=Some((0.5, 2.0)))] = 1.0


class Mono(Case, frozen=True, kw_only=True):
    """Screens luma in one ink on paper."""

    ink: Annotated[Rgb[Finite], msgspec.Meta(description="Dot display color")] = Rgb(red=0.0, green=0.0, blue=0.0)
    paper: Annotated[Rgb[Finite], msgspec.Meta(description="Display color between dots")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Process(Case, frozen=True, kw_only=True):
    """Prints cyan, magenta, yellow, and key screens on white paper."""

    cyan: Annotated[Screen, msgspec.Meta(description="Cyan screen lattice")] = Screen(offset=(0.772, 0.208))
    magenta: Annotated[Screen, msgspec.Meta(description="Magenta screen lattice")] = Screen(offset=(0.208, 0.772))
    yellow: Annotated[Screen, msgspec.Meta(description="Yellow screen lattice")] = Screen(offset=(0.0, 0.8))
    key: Annotated[Screen, msgspec.Meta(description="Black screen lattice")] = Screen(offset=(0.5656, 0.5656))


class Halftone(Case, frozen=True, kw_only=True):
    """Prints tone as dots with edges antialiased over one pixel."""

    pitch: Annotated[FrameExtent, msgspec.Meta(description="Screen lattice spacing"), Presentation(soft=Some((1 / 128, 0.125)), precision=Some(4))] = 1 / 16
    angle: Annotated[Turn, msgspec.Meta(description="Screen turn about frame center")] = float(np.pi / 4)
    jitter: Annotated[Unit, msgspec.Meta(description="Dot displacement within cell")] = 0.0
    inks: Annotated[Inks, msgspec.Meta(description="One luma screen or four process screens")] = Mono()


class SourceColor(Case, frozen=True, kw_only=True):
    """Draws each glyph in its cell's color."""


class TwoTone(Case, frozen=True, kw_only=True):
    """Draws every glyph in one ink."""

    ink: Annotated[Rgb[Finite], msgspec.Meta(description="Glyph display color")] = Rgb(red=1.0, green=1.0, blue=1.0)


class RandomColor(Case, frozen=True, kw_only=True):
    """Draws each glyph in a random hue per cell."""

    saturation: Annotated[Unit, msgspec.Meta(description="Chroma of drawn colors")] = 1.0


class Glyphs(Case, frozen=True, kw_only=True):
    """Replaces text-grid cells with glyphs selected by their tone."""

    glyphs: Annotated[str, msgspec.Meta(min_length=1, description="Characters ordered from least to most ink"), GraphForm.SETTING] = " .:-=+*#%@"
    rows: Annotated[int, msgspec.Meta(ge=1, description="Glyph cells across short side"), Presentation(soft=Some((4.0, 512.0)))] = 24
    jitter: Annotated[Unit, msgspec.Meta(description="Random shift of each cell tone")] = 0.0
    paper: Annotated[Rgb[Finite], msgspec.Meta(description="Display color behind glyphs")] = Rgb(red=0.0, green=0.0, blue=0.0)
    coloring: Annotated[Coloring, msgspec.Meta(description="Glyph color source")] = SourceColor()


class Mosaic(Case, frozen=True, kw_only=True):
    """Replaces frame with square blocks."""

    size: Annotated[FrameExtent, msgspec.Meta(description="Block side"), Presentation(soft=Some((0.001, 0.1)), precision=Some(4))] = 0.005


class Cells(Case, frozen=True, kw_only=True):
    """Fills each Voronoi cell with image color at its site."""

    size: Annotated[FrameExtent, msgspec.Meta(description="Mean cell site spacing"), Presentation(soft=Some((0.003, 0.06)), precision=Some(4))] = 0.0225
    jitter: Annotated[Unit, msgspec.Meta(description="Site displacement within lattice cell")] = 1.0
    distance: Annotated[Literal["EUCLIDEAN", "MANHATTAN", "CHEBYCHEV"], msgspec.Meta(description="Voronoi distance shaping cell borders"), GraphForm.SETTING] = "EUCLIDEAN"


class PixelSort(Case, frozen=True, kw_only=True):
    """Streaks pixels in a wrapped luma band along one direction."""

    direction: Annotated[Direction, msgspec.Meta(description="Streak direction")] = Direction.RIGHT
    low: Annotated[Unit, msgspec.Meta(description="Lower luma boundary, wrapping when above High")] = 0.0
    high: Annotated[Unit, msgspec.Meta(description="Upper luma boundary")] = 0.3
    length: Annotated[Unit, msgspec.Meta(description="Longest streak"), Presentation(quantity=Some(Quantity.FRAME))] = 0.3
    softness: Annotated[FrameLength, msgspec.Meta(description="Luma blur along axis before band key"), Presentation(soft=Some((0.0, 0.1)), precision=Some(4))] = 0.01
    falloff: Annotated[FrameLength, msgspec.Meta(description="Key blur along axis"), Presentation(soft=Some((0.0, 0.1)), precision=Some(4))] = 0.04
    scatter: Annotated[Unit, msgspec.Meta(description="Run start noise")] = 0.2
    clump: Annotated[Unit, msgspec.Meta(description="Streak field roughness across axis")] = 0.5
    chunk: Annotated[FrameLength, msgspec.Meta(description="Streak map block size"), Presentation(soft=Some((0.0, 0.006)), precision=Some(4))] = 0.0005
    edge_fade: Annotated[Unit, msgspec.Meta(description="Streak fade toward frame ends along axis")] = 0.0


class Pixelation(Effect, frozen=True, kw_only=True):
    """Resamples display values into blocks or cells before palette reduction."""

    stage = Stage.PIXELATE
    pointwise = False
    structure: Annotated[Mosaic | Cells, msgspec.Meta(description="Sampling blocks or cells")] = Mosaic()
    clock: Annotated[Clock, msgspec.Meta(description="Clock every draw reads")] = Clock(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting every random site")] = 0


class Pattern(Effect, frozen=True, kw_only=True):
    """Replaces display values with dots, glyphs, or streaks after palette reduction."""

    stage = Stage.PATTERN
    pointwise = False
    structure: Annotated[Structure, msgspec.Meta(description="Cells or runs over frame")] = Halftone()
    clock: Annotated[Clock, msgspec.Meta(description="Clock every draw reads")] = Clock(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting every random site, tone, and streak")] = 0


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SCREENS]


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
    disc = sites * sites * float(np.pi)
    radius = math("MAXIMUM", sites, 0.5)
    segment = math("MULTIPLY_ADD", radius * radius, math("ARCCOSINE", 0.5 / radius), math("SQRT", radius * radius - 0.25) * -0.5)
    area = where(sites < float(np.sqrt(0.5)), where(math("GREATER_THAN", sites, 0.5), disc - segment * 4.0, disc), 1.0)
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


# --- [STRUCTURES]


def _drawn(shell: Shell[Pixelation] | Shell[Pattern], kind: type[Case], instance: int = 0) -> Expr:
    """Decorrelation origin of one structure's draws, redrawn per whole clock frame."""
    _, clock, seed = type(shell.record).__struct_fields__
    speed, hold = Clock.__struct_fields__
    return origin(shell.input(seed), stream(kind), math("FLOOR", clocked(shell.input(clock, speed), shell.input(clock, hold))), instance)


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


def _structured(shell: Shell[Pixelation] | Shell[Pattern], choices: Mapping[str, Expr], *, covered: bool) -> Expr:
    """Selects the structure color keeping its replacement weight, the structure's alpha when covered, else the whole frame."""
    structure, _, _ = type(shell.record).__struct_fields__
    result = shell.menu(structure, cases=choices, kind="RGBA")
    weight = Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", result if covered else (1.0,) * 4),), members=(("mode", "RGB"),), socket="Alpha").named("structure_weight")
    return result.keeping(weight)


def _toned(image: Scalar) -> Expr:
    """Luminance of an image through RGB to BW."""
    return Expr(kind=bpy.types.CompositorNodeRGBToBW, inputs=(("Image", image),), socket="Val")


def pixelated(shell: Shell[Pixelation]) -> BlenderResult[Expr]:
    """Builds mosaic block means and Voronoi cells filled at their sites, each replacing the whole frame."""
    structure, _, _ = Pixelation.__struct_fields__
    (block,) = Mosaic.__struct_fields__
    size, jitter, _ = Cells.__struct_fields__
    frame = shell.space

    def built(case: Mosaic | Cells) -> Expr:
        match case:
            case Mosaic():
                return block_mean(frame, shell.color, shell.input(structure, Mosaic, block))
            case Cells(distance=distance):
                offset, middle, side = vector("FLOOR", _drawn(shell, Cells)), vector("SCALE", frame.extent, 0.5), shell.input(structure, Cells, size)
                site = Expr(
                    kind=bpy.types.ShaderNodeTexVoronoi,
                    inputs=(("Vector", (frame.point - middle) / side + offset), ("Scale", 1.0), ("Detail", 0.0), ("Randomness", shell.input(structure, Cells, jitter))),
                    members=(("voronoi_dimensions", "2D"), ("feature", "F1"), ("distance", distance), ("normalize", False)),
                    socket="Position",
                    shape="VECTOR",
                )
                return _filled(shell, sampled(frame, vector("MULTIPLY_ADD", site - offset, side, middle) / frame.extent))

    return Ok(_structured(shell, {case.title: built(case) for case in shell.cases[structure,] if isinstance(case, Mosaic | Cells)}, covered=False))


def patterned(shell: Shell[Pattern]) -> BlenderResult[Expr]:
    """Builds halftone screens, tone-ranked glyph strip lookups, and pixel sort streaks, each with its own coverage."""
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

    def halftoned() -> Expr:
        at = (structure, Halftone)

        def screened(tone: Expr, instance: int, offset: Scalar | tuple[float, ...] = (0.0, 0.0, 0.0), multiple: Scalar = 1.0) -> Expr:
            return halftone_screen(
                coordinate=centered,
                offset=vector("ADD", vector("FLOOR", _drawn(shell, Halftone, instance)), offset),
                angle=shell.input(*at, angle),
                pitch=math("MULTIPLY", shell.input(*at, pitch), multiple),
                jitter=shell.input(*at, jitter),
                tone=tone,
            )

        mono = screened(1.0 - _toned(source), 0)
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
        draw = Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", indices + _drawn(shell, Glyphs)),), members=(("noise_dimensions", "3D"),))
        tone = math("MULTIPLY_ADD", draw - 0.5, shell.input(*at, glyph_jitter), _toned(center), clamp=True)
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
        heading = shell.menu(*at, direction, cases={choice.label: choice.heading for choice in Direction}, kind="VECTOR")
        axis = abs(heading)

        def blurred(image: Scalar, reach: str) -> Expr:
            return Expr(
                kind=bpy.types.CompositorNodeBlur,
                inputs=(("Image", image), ("Size", pixels(frame, vector("SCALE", axis, shell.input(*at, reach)), "VECTOR")), ("Type", "Gaussian")),
                socket="Image",
                shape="RGBA",
            )

        soft = blurred(_toned(source), softness)
        above, below = 1.0 - (soft < shell.input(*at, low)), 1.0 - (soft > shell.input(*at, high))
        band = mixed(shell.input(*at, low) > shell.input(*at, high), above * below, math("MAXIMUM", above, below))
        grain = Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", frame.point + _drawn(shell, PixelSort, 0)),), members=(("noise_dimensions", "2D"),))
        streak = Expr(
            kind=bpy.types.ShaderNodeTexNoise,
            inputs=(
                ("W", vector("DOT_PRODUCT", frame.fraction, (1.0, 1.0, 0.0) - axis) + math("MULTIPLY_ADD", _toned(source), 2.0, -1.0) + _drawn(shell, PixelSort, 1)["X"]),
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

    return Ok(_structured(shell, {case.title: built(case) for case in shell.cases[structure,] if isinstance(case, Halftone | Glyphs | PixelSort)}, covered=True))


# --- [COMPOSITION] ----------------------------------------------------------------------

pixelation = CompositorEffect(
    record=Pixelation,
    graph=pixelated,
    domain=Domain.DISPLAY,
    alpha=Alpha.RESULT,
    parts=(Requests[Pixelation](request=precise), Inspects(key="pixelation_weight", end=End(node="structure_weight", socket="Alpha"), description="Structure replacement weight")),
)
pattern = CompositorEffect(
    record=Pattern,
    graph=patterned,
    domain=Domain.DISPLAY,
    alpha=Alpha.STRAIGHT,
    parts=(Requests[Pattern](request=precise), Inspects(key="pattern_weight", end=End(node="structure_weight", socket="Alpha"), description="Selected structure coverage")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Cells",
    "Coloring",
    "Direction",
    "Glyphs",
    "Halftone",
    "Inks",
    "Mono",
    "Mosaic",
    "Pattern",
    "PixelSort",
    "Pixelation",
    "Process",
    "RandomColor",
    "Screen",
    "SourceColor",
    "Structure",
    "TwoTone",
    "pattern",
    "pixelation",
]
