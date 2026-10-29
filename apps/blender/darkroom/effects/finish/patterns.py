"""Display structures: mosaic blocks and Voronoi cells resampling the frame, then halftone screens, tone-ranked glyphs, and pixel sort streaks replacing it."""

from collections.abc import Mapping
from typing import Annotated, Final, Literal

import bpy
from expression import Ok, Some
import msgspec
import numpy as np

from blender.bpy.extension.identity import Key
from blender.bpy.nodes.desired import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.functions import grouped
from blender.bpy.nodes.graphs import Expr, math, mixed, Scalar, vector, where, xyz
from blender.bpy.nodes.textures import White
from blender.bpy.render.compositor.colors import with_alpha
from blender.bpy.render.compositor.coordinates import dimensions, PixelGrid, shorter
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.filters import block_mean
from blender.bpy.render.compositor.inputs import Shell
from blender.bpy.render.compositor.motion import clocked, origin
from blender.imaging.color.processors import Domain
from blender.imaging.sampling.tiles import stream
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from ..optics.procedural import clock

# --- [CONSTANTS] ------------------------------------------------------------------------

_MOSAIC: Final = MenuItem(name="Mosaic", description="Replaces frame with square blocks")
_CELLS: Final = MenuItem(name="Cells", description="Fills each Voronoi cell with image color at its site")
_HALFTONE: Final = MenuItem(name="Halftone", description="Prints tone as dots with edges antialiased over one pixel")
_GLYPHS: Final = MenuItem(name="Glyphs", description="Replaces text-grid cells with glyphs selected by their tone")
_PIXEL_SORT: Final = MenuItem(name="Pixel Sort", description="Streaks pixels in a wrapped luma band along one direction")
_MONO: Final = MenuItem(name="Mono", description="Screens luma in one ink on paper")
_PROCESS: Final = MenuItem(name="Process", description="Prints cyan, magenta, yellow, and key screens on white paper")
_SOURCE_COLOR: Final = MenuItem(name="Source Color", description="Draws each glyph in its cell's color")
_TWO_TONE: Final = MenuItem(name="Two Tone", description="Draws every glyph in one ink")
_RANDOM_COLOR: Final = MenuItem(name="Random Color", description="Draws each glyph in a random hue per cell")
_LEFT: Final = MenuItem(name="Left")
_RIGHT: Final = MenuItem(name="Right")
_UP: Final = MenuItem(name="Up")
_DOWN: Final = MenuItem(name="Down")
_CLOCK: Final = clock("clock", speed=0.0)
_CLOCK_SPEED, _CLOCK_HOLD = _CLOCK.inputs
_SEED: Final = Socket(key="seed", kind=bpy.types.NodeSocketInt, description="Number selecting every random site, tone, and streak", min_value=0, max_value=2**24)
_PIXELATION_STRUCTURE: Final = Socket(
    key="structure", kind=bpy.types.NodeSocketMenu, description="Sampling blocks or cells", default_value=_MOSAIC.name, items=Some(MenuItems(items=(_MOSAIC, _CELLS)))
)
_MOSAIC_SIZE: Final = Socket(key="mosaic_size", name="Size", kind=bpy.types.NodeSocketFloat, description="Block side over greater frame side", min_value=1e-6, default_value=0.005)
_CELLS_SIZE: Final = Socket(key="cells_size", name="Size", kind=bpy.types.NodeSocketFloat, description="Mean cell site spacing over greater frame side", min_value=1e-6, default_value=0.0225)
_CELLS_JITTER: Final = Socket(
    key="cells_jitter", name="Jitter", kind=bpy.types.NodeSocketFloatFactor, description="Site displacement within lattice cell", min_value=0.0, max_value=1.0, default_value=1.0
)
_PATTERN_STRUCTURE: Final = Socket(
    key="structure", kind=bpy.types.NodeSocketMenu, description="Cells or runs over frame", default_value=_HALFTONE.name, items=Some(MenuItems(items=(_HALFTONE, _GLYPHS, _PIXEL_SORT)))
)
_HALFTONE_PITCH: Final = Socket(key="halftone_pitch", name="Pitch", kind=bpy.types.NodeSocketFloat, description="Screen lattice spacing over greater frame side", min_value=1e-6, default_value=1 / 16)
_HALFTONE_ANGLE: Final = Socket(key="halftone_angle", name="Angle", kind=bpy.types.NodeSocketFloatAngle, description="Screen turn about frame center", default_value=np.pi / 4)
_HALFTONE_JITTER: Final = Socket(key="halftone_jitter", name="Jitter", kind=bpy.types.NodeSocketFloatFactor, description="Dot displacement within cell", min_value=0.0, max_value=1.0)
_HALFTONE_INKS: Final = Socket(
    key="halftone_inks", name="Inks", kind=bpy.types.NodeSocketMenu, description="One luma screen or four process screens", default_value=_MONO.name, items=Some(MenuItems(items=(_MONO, _PROCESS)))
)
_MONO_INK: Final = Socket(key="halftone_mono_ink", name="Ink", kind=bpy.types.NodeSocketColor, description="Dot display color", default_value=(0.0, 0.0, 0.0, 1.0))
_MONO_PAPER: Final = Socket(key="halftone_mono_paper", name="Paper", kind=bpy.types.NodeSocketColor, description="Display color between dots", default_value=(1.0, 1.0, 1.0, 1.0))
_PROCESS_INKS: Final = tuple(
    InterfacePanel(
        key=item_key("halftone_process", ink),
        name=bpy.path.display_name(ink),
        description=f"{bpy.path.display_name(ink)} screen lattice",
        inputs=(
            Socket(key=item_key("halftone_process", ink, "offset"), name="Offset", kind=bpy.types.NodeSocketVector2D, description="Lattice shift in cells", default_value=offset),
            Socket(key=item_key("halftone_process", ink, "scale"), name="Scale", kind=bpy.types.NodeSocketFloat, description="Multiple of halftone pitch", min_value=1e-6, default_value=1.0),
        ),
    )
    for ink, offset in (("cyan", (0.772, 0.208)), ("magenta", (0.208, 0.772)), ("yellow", (0.0, 0.8)), ("key", (0.5656, 0.5656)))
)
_GLYPHS_ROWS: Final = Socket(key="glyphs_rows", name="Rows", kind=bpy.types.NodeSocketInt, description="Glyph cells across short side", min_value=1, default_value=24)
_GLYPHS_JITTER: Final = Socket(key="glyphs_jitter", name="Jitter", kind=bpy.types.NodeSocketFloatFactor, description="Random shift of each cell tone", min_value=0.0, max_value=1.0)
_GLYPHS_PAPER: Final = Socket(key="glyphs_paper", name="Paper", kind=bpy.types.NodeSocketColor, description="Display color behind glyphs", default_value=(0.0, 0.0, 0.0, 1.0))
_GLYPHS_COLORING: Final = Socket(
    key="glyphs_coloring",
    name="Coloring",
    kind=bpy.types.NodeSocketMenu,
    description="Glyph color source",
    default_value=_SOURCE_COLOR.name,
    items=Some(MenuItems(items=(_SOURCE_COLOR, _TWO_TONE, _RANDOM_COLOR))),
)
_TWO_TONE_INK: Final = Socket(key="glyphs_two_tone_ink", name="Ink", kind=bpy.types.NodeSocketColor, description="Glyph display color", default_value=(1.0, 1.0, 1.0, 1.0))
_RANDOM_SATURATION: Final = Socket(
    key="glyphs_random_color_saturation", name="Saturation", kind=bpy.types.NodeSocketFloatFactor, description="Chroma of drawn colors", min_value=0.0, max_value=1.0, default_value=1.0
)
_SORT_DIRECTION: Final = Socket(
    key="pixel_sort_direction", name="Direction", kind=bpy.types.NodeSocketMenu, description="Streak direction", default_value=_RIGHT.name, items=Some(MenuItems(items=(_LEFT, _RIGHT, _UP, _DOWN)))
)
_SORT_LOW, _SORT_HIGH, _SORT_LENGTH, _SORT_SOFTNESS, _SORT_FALLOFF, _SORT_SCATTER, _SORT_CLUMP, _SORT_CHUNK, _SORT_EDGE_FADE = (
    Socket(key=item_key("pixel_sort", field), name=bpy.path.display_name(field), kind=kind, description=description, min_value=low, max_value=high, default_value=default)
    for field, kind, description, low, high, default in (
        ("low", bpy.types.NodeSocketFloatFactor, "Lower luma boundary, wrapping when above High", 0.0, 1.0, None),
        ("high", bpy.types.NodeSocketFloatFactor, "Upper luma boundary", 0.0, 1.0, 0.3),
        ("length", bpy.types.NodeSocketFloatFactor, "Longest streak as a frame share", 0.0, 1.0, 0.3),
        ("softness", bpy.types.NodeSocketFloat, "Luma blur along axis before band key over greater frame side", 0.0, None, 0.01),
        ("falloff", bpy.types.NodeSocketFloat, "Key blur along axis over greater frame side", 0.0, None, 0.04),
        ("scatter", bpy.types.NodeSocketFloatFactor, "Run start noise", 0.0, 1.0, 0.2),
        ("clump", bpy.types.NodeSocketFloatFactor, "Streak field roughness across axis", 0.0, 1.0, 0.5),
        ("chunk", bpy.types.NodeSocketFloat, "Streak map block size over greater frame side", 0.0, None, 0.0005),
        ("edge_fade", bpy.types.NodeSocketFloatFactor, "Streak fade toward frame ends along axis", 0.0, 1.0, None),
    )
)
_PIXELATION_FACE: Final = Interface(
    inputs=(_PIXELATION_STRUCTURE, _SEED),
    panels=(InterfacePanel(key="mosaic", name="Mosaic", inputs=(_MOSAIC_SIZE,)), InterfacePanel(key="cells", name="Cells", inputs=(_CELLS_SIZE, _CELLS_JITTER)), _CLOCK),
)
_PATTERN_FACE: Final = Interface(
    inputs=(_PATTERN_STRUCTURE, _SEED),
    panels=(
        InterfacePanel(
            key="halftone",
            name="Halftone",
            inputs=(_HALFTONE_PITCH, _HALFTONE_ANGLE, _HALFTONE_JITTER, _HALFTONE_INKS),
            panels=(InterfacePanel(key="halftone_mono", name="Mono", inputs=(_MONO_INK, _MONO_PAPER)), InterfacePanel(key="halftone_process", name="Process", panels=_PROCESS_INKS)),
        ),
        InterfacePanel(
            key="glyphs",
            name="Glyphs",
            inputs=(_GLYPHS_ROWS, _GLYPHS_JITTER, _GLYPHS_PAPER, _GLYPHS_COLORING),
            panels=(InterfacePanel(key="glyphs_two_tone", name="Two Tone", inputs=(_TWO_TONE_INK,)), InterfacePanel(key="glyphs_random_color", name="Random Color", inputs=(_RANDOM_SATURATION,))),
        ),
        InterfacePanel(
            key="pixel_sort", name="Pixel Sort", inputs=(_SORT_DIRECTION, _SORT_LOW, _SORT_HIGH, _SORT_LENGTH, _SORT_SOFTNESS, _SORT_FALLOFF, _SORT_SCATTER, _SORT_CLUMP, _SORT_CHUNK, _SORT_EDGE_FADE)
        ),
        _CLOCK,
    ),
)

# --- [MODELS] ---------------------------------------------------------------------------


class Pixelation(Effect, frozen=True, kw_only=True):
    """Resamples display values into blocks or cells before palette reduction, cell borders shaped by a Voronoi distance."""

    stage = Stage.PIXELATE
    pointwise = False
    domain = Domain.DISPLAY
    distance: Annotated[Literal["EUCLIDEAN", "MANHATTAN", "CHEBYCHEV"], msgspec.Meta(description="Voronoi distance shaping cell borders")] = "EUCLIDEAN"


class Pattern(Effect, frozen=True, kw_only=True):
    """Replaces display values with dots, glyphs, or streaks after palette reduction, glyphs drawn from characters ordered from least to most ink."""

    stage = Stage.PATTERN
    pointwise = False
    domain = Domain.DISPLAY
    glyphs: Annotated[str, msgspec.Meta(min_length=1, description="Characters ordered from least to most ink")] = " .:-=+*#%@"


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SCREENS]


@grouped(kind=bpy.types.CompositorNodeTree)
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
        members={"voronoi_dimensions": "2D", "feature": "F1", "distance": "EUCLIDEAN", "normalize": False},
        socket="Distance",
    )
    disc = sites * sites * np.pi
    radius = math("MAXIMUM", sites, 0.5)
    segment = math("MULTIPLY_ADD", radius * radius, math("ARCCOSINE", 0.5 / radius), math("SQRT", radius * radius - 0.25) * -0.5)
    area = where(sites < float(np.sqrt(0.5)), where(math("GREATER_THAN", sites, 0.5), disc - segment * 4.0, disc), 1.0)
    grid = PixelGrid.of(area)

    def slope(step: tuple[float, float, float]) -> Expr:
        neighbour = Expr(
            kind=bpy.types.CompositorNodeMapUV,
            inputs=(("Image", area), ("UV", grid.at(step, grid.inverse)), ("Interpolation", "Nearest"), ("Extension X", "Extend"), ("Extension Y", "Extend")),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )
        return abs(math("SUBTRACT", neighbour, area))

    width = slope((1.0, 0.0, 0.0)) + slope((0.0, 1.0, 0.0))
    inside = where(width > 0.0, math("ADD", tone, (tone - area) / width, clamp=True), area < tone)
    return {"coverage": (tone > 0.0) * where(tone < 1.0, inside, 1.0)}


# --- [STRUCTURES]


def drawn[R: (Pixelation, Pattern)](shell: Shell[R], key: Key, instance: int = 0) -> Expr:
    """Decorrelation origin of one structure's draws, redrawn per whole clock unit."""
    return origin(shell.input(_SEED), stream(type(shell.record), key), math("FLOOR", clocked(shell.input(_CLOCK_SPEED), shell.input(_CLOCK_HOLD))), instance)


def filled[R: (Pixelation, Pattern)](shell: Shell[R], uv: Expr) -> Expr:
    """Bilinear entry color at Map UV positions."""
    return Expr(
        kind=bpy.types.CompositorNodeMapUV,
        inputs=(("Image", shell.color), ("UV", uv), ("Interpolation", "Bilinear"), ("Extension X", "Extend"), ("Extension Y", "Extend")),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    )


def structured[R: (Pixelation, Pattern)](shell: Shell[R], structure: Socket, choices: Mapping[MenuItem, Expr], *, covered: bool) -> Expr:
    """Selects the structure color keeping its replacement weight, the structure's alpha when covered, else the whole frame."""
    result = shell.menu(structure, cases=choices)
    return result.keeping(Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", result if covered else (1.0,) * 4),), members={"mode": "RGB"}, socket="Alpha").named("structure_weight"))


def _pixelation(shell: Shell[Pixelation]) -> Outcome[Expr]:
    """Builds mosaic block means and Voronoi cells filled at their sites, each replacing the whole frame."""
    frame = shell.space
    offset, middle, side = vector("FLOOR", drawn(shell, "cells")), vector("SCALE", frame.extent, 0.5), shell.input(_CELLS_SIZE)
    site = Expr(
        kind=bpy.types.ShaderNodeTexVoronoi,
        inputs=(("Vector", (frame.point - middle) / side + offset), ("Scale", 1.0), ("Detail", 0.0), ("Randomness", shell.input(_CELLS_JITTER))),
        members={"voronoi_dimensions": "2D", "feature": "F1", "distance": shell.record.distance, "normalize": False},
        socket="Position",
        shape=bpy.types.NodeSocketVector,
    )
    cells = filled(shell, frame.sampled(vector("MULTIPLY_ADD", site - offset, side, middle) / frame.extent))
    return Ok(structured(shell, _PIXELATION_STRUCTURE, {_MOSAIC: block_mean(frame, shell.color, shell.input(_MOSAIC_SIZE)), _CELLS: cells}, covered=False))


pixelation: Final = CompositorEffect(graph=_pixelation, record=Pixelation, interface=_PIXELATION_FACE, mix=Blended(alpha=Alpha.RESULT), request=Some(precise))


def _pattern(shell: Shell[Pattern]) -> Outcome[Expr]:
    """Builds halftone screens, tone-ranked glyph strip lookups, and pixel sort streaks, each with its own coverage."""
    source, frame = shell.color, shell.space
    centered = frame.point - vector("SCALE", frame.extent, 0.5)

    def toned(image: Scalar) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeRGBToBW, inputs=(("Image", image),), socket="Val")

    def screened(tone: Expr, instance: int, offset: Scalar | tuple[float, ...] = (0.0, 0.0, 0.0), multiple: Scalar = 1.0) -> Expr:
        return halftone_screen(
            coordinate=centered,
            offset=vector("ADD", vector("FLOOR", drawn(shell, "halftone", instance)), offset),
            angle=shell.input(_HALFTONE_ANGLE),
            pitch=math("MULTIPLY", shell.input(_HALFTONE_PITCH), multiple),
            jitter=shell.input(_HALFTONE_JITTER),
            tone=tone,
        )

    mono = screened(1.0 - toned(source), 0)
    peak = math("MAXIMUM", math("MAXIMUM", source["Red"], source["Green"]), source["Blue"])
    separations = vector("DIVIDE", vector("SUBTRACT", peak, source), peak)
    tones = (separations["X"], separations["Y"], separations["Z"], 1.0 - peak)
    clear = tuple(1.0 - screened(tone, instance, *map(shell.input, ink.inputs)) for instance, (ink, tone) in enumerate(zip(_PROCESS_INKS, tones, strict=True)))
    inked = {
        _MONO: with_alpha(mixed(mono, shell.input(_MONO_PAPER), shell.input(_MONO_INK)), mono),
        _PROCESS: with_alpha(vector("SCALE", xyz(*clear[:3]), clear[3]), 1.0 - clear[0] * clear[1] * clear[2] * clear[3]),
    }
    glyphs = shell.record.glyphs
    levels = len(glyphs)
    height = shorter(frame.extent) / shell.input(_GLYPHS_ROWS)
    strip = Expr(kind=bpy.types.CompositorNodeStringToImage, inputs=(("String", "\n".join(glyphs)), ("Size", height * frame.greater), ("Wrap", False)), socket="Image", shape=bpy.types.NodeSocketColor)
    sides = dimensions(strip)
    cell = xyz(height * (sides["X"] / sides["Y"] * levels), height, 1.0)
    indices, within = vector("FLOOR", frame.point / cell), vector("FRACTION", frame.point / cell)
    center = filled(shell, frame.sampled(vector("ADD", indices, (0.5, 0.5, 0.0)) * cell / frame.extent))
    draw = White().node(indices + drawn(shell, "glyphs"))
    tone = math("MULTIPLY_ADD", draw - 0.5, shell.input(_GLYPHS_JITTER), toned(center), clamp=True)
    rank = (levels - 1) - math("MINIMUM", math("FLOOR", tone * levels), levels - 1)
    mask = Expr(
        kind=bpy.types.CompositorNodeMapUV,
        inputs=(("Image", strip), ("UV", xyz(within["X"], (rank + within["Y"]) / levels, 1.0)), ("Interpolation", "Bilinear"), ("Extension X", "Extend"), ("Extension Y", "Extend")),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    )
    random = Expr(
        kind=bpy.types.CompositorNodeCombineColor,
        inputs=(("Red", msgspec.structs.replace(draw, socket="Color", shape=bpy.types.NodeSocketColor)["X"]), ("Green", shell.input(_RANDOM_SATURATION)), ("Blue", 1.0), ("Alpha", 1.0)),
        members={"mode": "HSV"},
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    )
    colorings = {_SOURCE_COLOR: center, _TWO_TONE: shell.input(_TWO_TONE_INK), _RANDOM_COLOR: random}

    def sorting(heading: Expr) -> Expr:
        axis = abs(heading)

        def blurred(image: Scalar, reach: Socket) -> Expr:
            return Expr(
                kind=bpy.types.CompositorNodeBlur,
                inputs=(("Image", image), ("Size", frame.pixel_size(vector("SCALE", axis, shell.input(reach)))), ("Type", "Gaussian")),
                socket="Image",
                shape=bpy.types.NodeSocketColor,
            )

        soft = blurred(toned(source), _SORT_SOFTNESS)
        low, high = shell.input(_SORT_LOW), shell.input(_SORT_HIGH)
        above, below = 1.0 - (soft < low), 1.0 - (soft > high)
        band = mixed(low > high, above * below, math("MAXIMUM", above, below))
        grain = White(noise_dimensions="2D").node(frame.point + drawn(shell, "pixel_sort", 0))
        streak = Expr(
            kind=bpy.types.ShaderNodeTexNoise,
            inputs=(
                ("W", vector("DOT_PRODUCT", frame.fraction, (1.0, 1.0, 0.0) - axis) + math("MULTIPLY_ADD", toned(source), 2.0, -1.0) + drawn(shell, "pixel_sort", 1)["X"]),
                ("Roughness", shell.input(_SORT_CLUMP)),
            ),
            members={"noise_dimensions": "1D", "noise_type": "FBM", "normalize": True},
            socket="Fac",
        )
        along = vector("DOT_PRODUCT", frame.fraction, axis)
        fade = Expr(
            kind=bpy.types.ShaderNodeMapRange,
            inputs=(("Value", math("MINIMUM", along, 1.0 - along)), ("From Min", shell.input(_SORT_EDGE_FADE)), ("From Max", 0.0), ("To Min", 1.0), ("To Max", 0.0)),
            members={"clamp": True},
            socket="Result",
        )
        weight = math("MULTIPLY", block_mean(frame, math("MULTIPLY", blurred(band, _SORT_FALLOFF), 1.0 - grain * shell.input(_SORT_SCATTER)) * streak, shell.input(_SORT_CHUNK)), fade)
        displaced = Expr(
            kind=bpy.types.CompositorNodeDisplace,
            inputs=(
                ("Image", source),
                ("Displacement", frame.pixel_size(vector("SCALE", vector("SCALE", heading, shell.input(_SORT_LENGTH)), weight))),
                ("Interpolation", "Bilinear"),
                ("Extension X", "Extend"),
                ("Extension Y", "Extend"),
            ),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )
        return with_alpha(mixed(weight, source, displaced), weight)

    headings = {_LEFT: (-1.0, 0.0, 0.0), _RIGHT: (1.0, 0.0, 0.0), _UP: (0.0, 1.0, 0.0), _DOWN: (0.0, -1.0, 0.0)}
    halftone, color = shell.menu(_HALFTONE_INKS, cases=inked), shell.menu(_GLYPHS_COLORING, cases=colorings)
    heading = shell.menu(_SORT_DIRECTION, cases=headings)
    return Ok(structured(shell, _PATTERN_STRUCTURE, {_HALFTONE: halftone, _GLYPHS: with_alpha(mixed(mask, shell.input(_GLYPHS_PAPER), color), mask), _PIXEL_SORT: sorting(heading)}, covered=True))


pattern: Final = CompositorEffect(graph=_pattern, record=Pattern, interface=_PATTERN_FACE, mix=Blended(alpha=Alpha.STRAIGHT), request=Some(precise))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Pattern", "Pixelation", "pattern", "pixelation"]
