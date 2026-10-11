"""Halftone, glyph, mosaic, Voronoi cell, and pixel sort structures."""

import math
import sys
from typing import Annotated

from expression import Some
import msgspec

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.parameters import Case, Choice, Finite, GraphForm, Motion, Presentation, Quantity, Rgb, Seed

# --- [TYPES] ------------------------------------------------------------------------------


class Direction(Choice):
    """Direction of a pixel sort run."""

    LEFT = 1, "Left"
    RIGHT = 2, "Right"
    UP = 3, "Up"
    DOWN = 4, "Down"


class Metric(Choice):
    """Distance shaping Voronoi cell borders."""

    EUCLIDEAN = 1, "Euclidean"
    MANHATTAN = 2, "Manhattan"
    CHEBYCHEV = 3, "Chebychev"


type Inks = Mono | Process
type Coloring = SourceColor | TwoTone | RandomColor
type Structure = Halftone | Glyphs | PixelSort

# --- [MODELS] -----------------------------------------------------------------------------


class Screen(msgspec.Struct, frozen=True, kw_only=True):
    """Lattice shift and pitch multiple of one process screen."""

    offset: Annotated[tuple[Finite, Finite], msgspec.Meta(title="Offset", description="Shift of the screen lattice in cells")] = (0.0, 0.0)
    scale: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Scale", description="Multiple of the halftone pitch"), Presentation(soft=Some((0.5, 2.0)))] = 1.0


class Mono(Case, frozen=True, kw_only=True, tag=1):
    """Screens luma in one ink on paper."""

    title = "Mono"
    ink: Annotated[Rgb[Finite], msgspec.Meta(title="Ink", description="Display color of the dots")] = Rgb(red=0.0, green=0.0, blue=0.0)
    paper: Annotated[Rgb[Finite], msgspec.Meta(title="Paper", description="Display color between the dots")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Process(Case, frozen=True, kw_only=True, tag=2):
    """Prints cyan, magenta, yellow, and key screens on white paper."""

    title = "Process"
    cyan: Annotated[Screen, msgspec.Meta(title="Cyan", description="Lattice of the cyan screen")] = Screen(offset=(0.772, 0.208))
    magenta: Annotated[Screen, msgspec.Meta(title="Magenta", description="Lattice of the magenta screen")] = Screen(offset=(0.208, 0.772))
    yellow: Annotated[Screen, msgspec.Meta(title="Yellow", description="Lattice of the yellow screen")] = Screen(offset=(0.0, 0.8))
    key: Annotated[Screen, msgspec.Meta(title="Key", description="Lattice of the black screen")] = Screen(offset=(0.5656, 0.5656))


class Halftone(Case, frozen=True, kw_only=True, tag=1):
    """Prints tone as dots with edges antialiased over one pixel."""

    title = "Halftone"
    pitch: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Pitch", description="Spacing of the screen lattice"),
        Presentation(soft=Some((1 / 128, 0.125)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 1 / 16
    angle: Annotated[float, msgspec.Meta(ge=-math.pi, le=math.pi, title="Angle", description="Rotation of the screen about the frame center"), Presentation(quantity=Some(Quantity.ANGLE))] = (
        math.pi / 4
    )
    jitter: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Jitter", description="Displacement of each dot within its cell")] = 0.0
    inks: Annotated[Inks, msgspec.Meta(title="Inks", description="One luma screen or four process screens")] = Mono()


class SourceColor(Case, frozen=True, kw_only=True, tag=1):
    """Draws each glyph in its cell's color."""

    title = "Source Color"


class TwoTone(Case, frozen=True, kw_only=True, tag=2):
    """Draws every glyph in one ink."""

    title = "Two Tone"
    ink: Annotated[Rgb[Finite], msgspec.Meta(title="Ink", description="Display color of every glyph")] = Rgb(red=1.0, green=1.0, blue=1.0)


class RandomColor(Case, frozen=True, kw_only=True, tag=3):
    """Draws each glyph in a hue drawn per cell."""

    title = "Random Color"
    saturation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Saturation", description="Chroma of the drawn colors")] = 1.0


class Glyphs(Case, frozen=True, kw_only=True, tag=2):
    """Replaces text-grid cells with glyphs selected by their tone."""

    title = "Glyphs"
    glyphs: Annotated[str, msgspec.Meta(min_length=1, title="Glyphs", description="Characters ordered from least to most ink"), GraphForm.SETTING] = " .:-=+*#%@"
    rows: Annotated[int, msgspec.Meta(ge=1, title="Rows", description="Glyph cells across the frame's short side"), Presentation(soft=Some((4.0, 512.0)))] = 24
    jitter: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Jitter", description="Random shift of each cell's tone")] = 0.0
    paper: Annotated[Rgb[Finite], msgspec.Meta(title="Paper", description="Display color behind the glyphs")] = Rgb(red=0.0, green=0.0, blue=0.0)
    coloring: Annotated[Coloring, msgspec.Meta(title="Coloring", description="Color each glyph draws in")] = SourceColor()


class Mosaic(Case, frozen=True, kw_only=True, tag=4):
    """Replaces the frame with square blocks."""

    title = "Mosaic"
    size: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Size", description="Side of each block"),
        Presentation(soft=Some((0.001, 0.1)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.005


class Cells(Case, frozen=True, kw_only=True, tag=5):
    """Fills each Voronoi cell with the image color at its site."""

    title = "Cells"
    size: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Size", description="Mean spacing of the cell sites"),
        Presentation(soft=Some((0.003, 0.06)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.0225
    jitter: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Jitter", description="Displacement of each site within its lattice cell")] = 1.0
    metric: Annotated[Metric, msgspec.Meta(title="Metric", description="Distance shaping the cell borders")] = Metric.EUCLIDEAN


class PixelSort(Case, frozen=True, kw_only=True, tag=6):
    """Streaks pixels in a wrapped luma band along one direction."""

    title = "Pixel Sort"
    direction: Annotated[Direction, msgspec.Meta(title="Direction", description="Way each run streaks")] = Direction.RIGHT
    low: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Low", description="Lower luma boundary, wrapping when above High")] = 0.0
    high: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="High", description="Upper luma boundary of the selected band")] = 0.3
    length: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Length", description="Longest streak"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.3
    softness: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Softness", description="Blur of luma along the axis ahead of the band key"),
        Presentation(soft=Some((0.0, 0.1)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.01
    falloff: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Falloff", description="Blur of the key along the axis"),
        Presentation(soft=Some((0.0, 0.1)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.04
    scatter: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Scatter", description="Noise breaking up the starts of runs")] = 0.2
    clump: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Clump", description="Roughness of the streak field across the axis")] = 0.5
    chunk: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Chunk", description="Block size of the streak map"),
        Presentation(soft=Some((0.0, 0.006)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.0005
    edge_fade: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Edge Fade", description="Fade of streaks toward the frame ends along the axis")] = 0.0


class Pixelation(Effect, frozen=True, kw_only=True, tag=46):
    """Resamples display values into blocks or cells before palette reduction."""

    stage = Stage.PIXELATE
    title = "Pixelation"
    pointwise = False
    structure: Annotated[Mosaic | Cells, msgspec.Meta(title="Structure", description="Blocks or cells sampling the frame")] = Mosaic()
    motion: Annotated[Motion, msgspec.Meta(title="Motion", description="Speed and hold of the clock every draw reads")] = Motion(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(title="Seed", description="Draw of every random site")] = 0


class Pattern(Effect, frozen=True, kw_only=True, tag=41):
    """Replaces display values with dots, glyphs, or streaks after palette reduction."""

    stage = Stage.PATTERN
    title = "Pattern"
    pointwise = False
    structure: Annotated[Structure, msgspec.Meta(title="Structure", description="Cells or runs laid over the frame")] = Halftone()
    motion: Annotated[Motion, msgspec.Meta(title="Motion", description="Speed and hold of the clock every draw reads")] = Motion(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(title="Seed", description="Draw of every random site, tone, and streak")] = 0


# --- [EXPORTS] ----------------------------------------------------------------------------


__all__ = [
    "Cells",
    "Coloring",
    "Direction",
    "Glyphs",
    "Halftone",
    "Inks",
    "Metric",
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
]
