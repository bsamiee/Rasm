"""Halftone, glyph, mosaic, Voronoi cell, and pixel sort structures."""

import math
from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.parameters import Case, Choice, Finite, FrameExtent, FrameLength, GraphForm, Motion, Positive, Presentation, Quantity, Rgb, Seed, Turn, Unit

# --- [TYPES] ----------------------------------------------------------------------------


class Direction(Choice):
    """Direction of a pixel sort run."""

    LEFT = 1
    RIGHT = 2
    UP = 3
    DOWN = 4


class Metric(Choice):
    """Distance shaping Voronoi cell borders."""

    EUCLIDEAN = 1
    MANHATTAN = 2
    CHEBYCHEV = 3


type Inks = Mono | Process
type Coloring = SourceColor | TwoTone | RandomColor
type Structure = Halftone | Glyphs | PixelSort

# --- [MODELS] ---------------------------------------------------------------------------


class Screen(msgspec.Struct, frozen=True, kw_only=True):
    """Lattice shift and pitch multiple of one process screen."""

    offset: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Shift of the screen lattice in cells")] = (0.0, 0.0)
    scale: Annotated[Positive, msgspec.Meta(description="Multiple of the halftone pitch"), Presentation(soft=Some((0.5, 2.0)))] = 1.0


class Mono(Case, frozen=True, kw_only=True, tag=1):
    """Screens luma in one ink on paper."""

    ink: Annotated[Rgb[Finite], msgspec.Meta(description="Display color of the dots")] = Rgb(red=0.0, green=0.0, blue=0.0)
    paper: Annotated[Rgb[Finite], msgspec.Meta(description="Display color between the dots")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Process(Case, frozen=True, kw_only=True, tag=2):
    """Prints cyan, magenta, yellow, and key screens on white paper."""

    cyan: Annotated[Screen, msgspec.Meta(description="Lattice of the cyan screen")] = Screen(offset=(0.772, 0.208))
    magenta: Annotated[Screen, msgspec.Meta(description="Lattice of the magenta screen")] = Screen(offset=(0.208, 0.772))
    yellow: Annotated[Screen, msgspec.Meta(description="Lattice of the yellow screen")] = Screen(offset=(0.0, 0.8))
    key: Annotated[Screen, msgspec.Meta(description="Lattice of the black screen")] = Screen(offset=(0.5656, 0.5656))


class Halftone(Case, frozen=True, kw_only=True, tag=1):
    """Prints tone as dots with edges antialiased over one pixel."""

    pitch: Annotated[FrameExtent, msgspec.Meta(description="Spacing of the screen lattice"), Presentation(soft=Some((1 / 128, 0.125)), decimals=Some(4))] = 1 / 16
    angle: Annotated[Turn, msgspec.Meta(description="Rotation of the screen about the frame center")] = math.pi / 4
    jitter: Annotated[Unit, msgspec.Meta(description="Displacement of each dot within its cell")] = 0.0
    inks: Annotated[Inks, msgspec.Meta(description="One luma screen or four process screens")] = Mono()


class SourceColor(Case, frozen=True, kw_only=True, tag=1):
    """Draws each glyph in its cell's color."""


class TwoTone(Case, frozen=True, kw_only=True, tag=2):
    """Draws every glyph in one ink."""

    ink: Annotated[Rgb[Finite], msgspec.Meta(description="Display color of every glyph")] = Rgb(red=1.0, green=1.0, blue=1.0)


class RandomColor(Case, frozen=True, kw_only=True, tag=3):
    """Draws each glyph in a hue drawn per cell."""

    saturation: Annotated[Unit, msgspec.Meta(description="Chroma of the drawn colors")] = 1.0


class Glyphs(Case, frozen=True, kw_only=True, tag=2):
    """Replaces text-grid cells with glyphs selected by their tone."""

    glyphs: Annotated[str, msgspec.Meta(min_length=1, description="Characters ordered from least to most ink"), GraphForm.SETTING] = " .:-=+*#%@"
    rows: Annotated[int, msgspec.Meta(ge=1, description="Glyph cells across the frame's short side"), Presentation(soft=Some((4.0, 512.0)))] = 24
    jitter: Annotated[Unit, msgspec.Meta(description="Random shift of each cell's tone")] = 0.0
    paper: Annotated[Rgb[Finite], msgspec.Meta(description="Display color behind the glyphs")] = Rgb(red=0.0, green=0.0, blue=0.0)
    coloring: Annotated[Coloring, msgspec.Meta(description="Color each glyph draws in")] = SourceColor()


class Mosaic(Case, frozen=True, kw_only=True, tag=4):
    """Replaces the frame with square blocks."""

    size: Annotated[FrameExtent, msgspec.Meta(description="Side of each block"), Presentation(soft=Some((0.001, 0.1)), decimals=Some(4))] = 0.005


class Cells(Case, frozen=True, kw_only=True, tag=5):
    """Fills each Voronoi cell with the image color at its site."""

    size: Annotated[FrameExtent, msgspec.Meta(description="Mean spacing of the cell sites"), Presentation(soft=Some((0.003, 0.06)), decimals=Some(4))] = 0.0225
    jitter: Annotated[Unit, msgspec.Meta(description="Displacement of each site within its lattice cell")] = 1.0
    metric: Annotated[Metric, msgspec.Meta(description="Distance shaping the cell borders")] = Metric.EUCLIDEAN


class PixelSort(Case, frozen=True, kw_only=True, tag=6):
    """Streaks pixels in a wrapped luma band along one direction."""

    direction: Annotated[Direction, msgspec.Meta(description="Way each run streaks")] = Direction.RIGHT
    low: Annotated[Unit, msgspec.Meta(description="Lower luma boundary, wrapping when above High")] = 0.0
    high: Annotated[Unit, msgspec.Meta(description="Upper luma boundary of the selected band")] = 0.3
    length: Annotated[Unit, msgspec.Meta(description="Longest streak"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.3
    softness: Annotated[FrameLength, msgspec.Meta(description="Blur of luma along the axis ahead of the band key"), Presentation(soft=Some((0.0, 0.1)), decimals=Some(4))] = 0.01
    falloff: Annotated[FrameLength, msgspec.Meta(description="Blur of the key along the axis"), Presentation(soft=Some((0.0, 0.1)), decimals=Some(4))] = 0.04
    scatter: Annotated[Unit, msgspec.Meta(description="Noise breaking up the starts of runs")] = 0.2
    clump: Annotated[Unit, msgspec.Meta(description="Roughness of the streak field across the axis")] = 0.5
    chunk: Annotated[FrameLength, msgspec.Meta(description="Block size of the streak map"), Presentation(soft=Some((0.0, 0.006)), decimals=Some(4))] = 0.0005
    edge_fade: Annotated[Unit, msgspec.Meta(description="Fade of streaks toward the frame ends along the axis")] = 0.0


class Pixelation(Effect, frozen=True, kw_only=True, tag=46):
    """Resamples display values into blocks or cells before palette reduction."""

    stage = Stage.PIXELATE
    pointwise = False
    structure: Annotated[Mosaic | Cells, msgspec.Meta(description="Blocks or cells sampling the frame")] = Mosaic()
    motion: Annotated[Motion, msgspec.Meta(description="Speed and hold of the clock every draw reads")] = Motion(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(description="Draw of every random site")] = 0


class Pattern(Effect, frozen=True, kw_only=True, tag=41):
    """Replaces display values with dots, glyphs, or streaks after palette reduction."""

    stage = Stage.PATTERN
    pointwise = False
    structure: Annotated[Structure, msgspec.Meta(description="Cells or runs laid over the frame")] = Halftone()
    motion: Annotated[Motion, msgspec.Meta(description="Speed and hold of the clock every draw reads")] = Motion(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(description="Draw of every random site, tone, and streak")] = 0


# --- [EXPORTS] --------------------------------------------------------------------------


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
