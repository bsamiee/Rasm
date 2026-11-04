"""Display structures: mosaic blocks and Voronoi cells resampling the frame, palette posterization with HSY hue retention under an ordered Bayer threshold and a luminance gradient map, then halftone screens, tone-ranked glyphs, and pixel sort streaks replacing it."""

from collections.abc import Mapping
from types import MappingProxyType
from typing import Annotated, Final, override

import bpy
from expression import Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.content import Ramp
from blender.bpy.nodes.desired import MenuItem
from blender.bpy.nodes.functions import grouped
from blender.bpy.nodes.graphs import Expr, math, mixed, rgba, Scalar, switched, vector, where, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket, StockInput
from blender.bpy.nodes.textures import MapRange, Noise, Voronoi, WhiteNoise
from blender.bpy.render.compositor.colors import hsy, with_alpha
from blender.bpy.render.compositor.coordinates import dimensions, PixelGrid, shorter, uv_mapped
from blender.bpy.render.compositor.effects import CompositorEffect, CompositorRecord, Mix, precise
from blender.bpy.render.compositor.filters import block_mean, blurred
from blender.bpy.render.compositor.inputs import GraphInputs
from blender.bpy.render.compositor.motion import CLOCK, Clock, clocked, origin, SEED, timing
from blender.imaging.sampling.tiles import stream
from blender.records.faults import Ok, Outcome, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

_DISTANCE: Final = Descriptor[bpy.types.EnumProperty](root=bpy.types.ShaderNodeTexVoronoi, prop=bpy.types.ShaderNodeTexVoronoi.bl_rna.properties["distance"])  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=0.0))
_BLOCK: Final = Socket(key="size", kind=bpy.types.NodeSocketFloat, description="Block side over greater frame side", min_value=1e-6, default_value=0.005)
_MOSAIC: Final = InterfacePanel.of("mosaic", Interface(inputs=(_BLOCK,)), description="Replaces frame with square blocks")
_SPACING: Final = Socket(key="size", kind=bpy.types.NodeSocketFloat, description="Mean cell site spacing over greater frame side", min_value=1e-6, default_value=0.0225)
_SITE_JITTER: Final = Socket(key="jitter", kind=StockInput(node=Voronoi.kind, input="Randomness"))
_EXPONENT: Final = Socket(key="exponent", kind=StockInput(node=Voronoi.kind, input="Exponent"))
_CELLS: Final = InterfacePanel.of("cells", Interface(inputs=(_SPACING, _SITE_JITTER, _EXPONENT)), description="Fills each Voronoi cell with image color at its site")
_SAMPLING: Final = Socket(key="structure", kind=bpy.types.NodeSocketMenu, description="Sampling blocks or cells", default_value=_MOSAIC.title, items=(_MOSAIC.item, _CELLS.item))
_PITCH: Final = Socket(key="pitch", kind=bpy.types.NodeSocketFloat, description="Screen lattice spacing over greater frame side", min_value=1e-6, default_value=1 / 16)
_ANGLE: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="Screen turn about frame center", default_value=np.pi / 4)
_DOT_JITTER: Final = Socket(key="jitter", kind=StockInput(node=Voronoi.kind, input="Randomness"), default_value=0.0)
_DOT_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Dot display color", default_value=(0.0, 0.0, 0.0, 1.0))
_DOT_PAPER: Final = Socket(key="paper", kind=bpy.types.NodeSocketColor, description="Display color between dots", default_value=(1.0, 1.0, 1.0, 1.0))
_MONO: Final = InterfacePanel.of("mono", Interface(inputs=(_DOT_COLOR, _DOT_PAPER)), description="Screens luma in one color on paper")
_OFFSET: Final = Socket(key="offset", kind=bpy.types.NodeSocketVector2D, description="Lattice shift in cells")
_SCALE: Final = Socket(key="scale", kind=bpy.types.NodeSocketFloat, description="Multiple of halftone pitch", min_value=1e-6, default_value=1.0)
_SEPARATIONS: Final = tuple(
    InterfacePanel.of(name, Interface(inputs=(_OFFSET, _SCALE)).defaulted({_OFFSET.key: offset}), description=f"{bpy.path.display_name(name)} screen lattice")
    for name, offset in (("cyan", (0.772, 0.208)), ("magenta", (0.208, 0.772)), ("yellow", (0.0, 0.8)), ("key", (0.5656, 0.5656)))
)
_PROCESS: Final = InterfacePanel.of("process", Interface(panels=_SEPARATIONS), description="Prints cyan, magenta, yellow, and key screens on white paper")
_SCREENS: Final = Socket(key="screens", kind=bpy.types.NodeSocketMenu, description="One luma screen or four process screens", default_value=_MONO.title, items=(_MONO.item, _PROCESS.item))
_HALFTONE: Final = InterfacePanel.of(
    "halftone", Interface(inputs=(_PITCH, _ANGLE, _DOT_JITTER, _SCREENS), panels=(_MONO, _PROCESS)), description="Prints tone as dots with edges antialiased over one pixel"
)
_ROWS: Final = Socket(key="rows", kind=bpy.types.NodeSocketInt, description="Glyph cells across short side", min_value=1, default_value=24)
_TONE_JITTER: Final = Socket(key="jitter", kind=bpy.types.NodeSocketFloatFactor, description="Random shift of each cell tone")
_GLYPH_PAPER: Final = Socket(key="paper", kind=bpy.types.NodeSocketColor, description="Display color behind glyphs", default_value=(0.0, 0.0, 0.0, 1.0))
_GLYPH_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Glyph display color", default_value=(1.0, 1.0, 1.0, 1.0))
_TWO_TONE: Final = InterfacePanel.of("two_tone", Interface(inputs=(_GLYPH_COLOR,)), description="Draws every glyph in one color")
_SATURATION: Final = Socket(key="saturation", kind=bpy.types.NodeSocketFloatFactor, description="Chroma of drawn colors", default_value=1.0)
_RANDOM_COLOR: Final = InterfacePanel.of("random_color", Interface(inputs=(_SATURATION,)), description="Draws each glyph in a random hue per cell")
_SOURCE_COLOR: Final = MenuItem(name="Source Color", description="Draws each glyph in its cell's color")
_COLORING: Final = Socket(key="coloring", kind=bpy.types.NodeSocketMenu, description="Glyph color source", default_value=_SOURCE_COLOR.name, items=(_SOURCE_COLOR, _TWO_TONE.item, _RANDOM_COLOR.item))
_GLYPHS: Final = InterfacePanel.of(
    "glyphs", Interface(inputs=(_ROWS, _TONE_JITTER, _GLYPH_PAPER, _COLORING), panels=(_TWO_TONE, _RANDOM_COLOR)), description="Replaces text-grid cells with glyphs selected by their tone"
)
_RIGHT: Final = MenuItem(name="Right")
_HEADINGS: Final = MappingProxyType({MenuItem(name="Left"): (-1.0, 0.0, 0.0), _RIGHT: (1.0, 0.0, 0.0), MenuItem(name="Up"): (0.0, 1.0, 0.0), MenuItem(name="Down"): (0.0, -1.0, 0.0)})
_DIRECTION: Final = Socket(key="direction", kind=bpy.types.NodeSocketMenu, description="Streak direction", default_value=_RIGHT.name, items=tuple(_HEADINGS))
_LOW: Final = Socket(key="low", kind=bpy.types.NodeSocketFloatFactor, description="Lower luma boundary, wrapping when above High")
_HIGH: Final = Socket(key="high", kind=bpy.types.NodeSocketFloatFactor, description="Upper luma boundary", default_value=0.3)
_LENGTH: Final = Socket(key="length", kind=bpy.types.NodeSocketFloatFactor, description="Longest streak as a frame share", default_value=0.3)
_SOFTNESS: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Luma blur along axis before band key over greater frame side", min_value=0.0, default_value=0.01)
_FALLOFF: Final = Socket(key="falloff", kind=bpy.types.NodeSocketFloat, description="Key blur along axis over greater frame side", min_value=0.0, default_value=0.04)
_SCATTER: Final = Socket(key="scatter", kind=bpy.types.NodeSocketFloatFactor, description="Run start noise", default_value=0.2)
_CLUMP: Final = Socket(key="clump", kind=StockInput(node=Noise.kind, input="Roughness"))
_CHUNK: Final = Socket(key="chunk", kind=bpy.types.NodeSocketFloat, description="Streak map block size over greater frame side", min_value=0.0, default_value=0.0005)
_EDGE_FADE: Final = Socket(key="edge_fade", kind=bpy.types.NodeSocketFloatFactor, description="Streak fade toward frame ends along axis")
_PIXEL_SORT: Final = InterfacePanel.of(
    "pixel_sort",
    Interface(inputs=(_DIRECTION, _LOW, _HIGH, _LENGTH, _SOFTNESS, _FALLOFF, _SCATTER, _CLUMP, _CHUNK, _EDGE_FADE)),
    description="Streaks pixels in a wrapped luma band along one direction",
)
_REPLACING: Final = Socket(
    key="structure", kind=bpy.types.NodeSocketMenu, description="Cells or runs over frame", default_value=_HALFTONE.title, items=(_HALFTONE.item, _GLYPHS.item, _PIXEL_SORT.item)
)
_RGB: Final = MenuItem(name="RGB")
_LUMINANCE: Final = MenuItem(name="Luminance")
_LEVELS: Final = Socket(key="levels", kind=bpy.types.NodeSocketInt, description="Values each channel rounds to, evenly spaced from 0 to 1", min_value=2, default_value=9)
_CHANNELS: Final = Socket(key="channels", kind=bpy.types.NodeSocketMenu, description="Each RGB channel or luminance as gray", default_value=_RGB.name, items=(_RGB, _LUMINANCE))
_RETAIN_HUE: Final = Socket(key="retain_hue", kind=bpy.types.NodeSocketFloatFactor, description="Blend toward input hue at rounded HSY saturation and luminance")
_STRENGTH: Final = Socket(key="value", name="Dither", kind=bpy.types.NodeSocketFloatFactor, default_value=1.0)
_DITHER: Final = InterfacePanel.of("dither", Interface(inputs=(_STRENGTH,)), description="Ordered 8-by-8 Bayer offset of rounding threshold in levels", toggle=Some(False))
_SIDE: Final = Socket(key="value", name="Dither Cell", kind=bpy.types.NodeSocketFloat, min_value=0.0)
_DITHER_CELL: Final = InterfacePanel.of("dither_cell", Interface(inputs=(_SIDE,)), description="Square Bayer cell size, one pixel when off or smaller", toggle=Some(False))
_POSTERIZE: Final = InterfacePanel.of(
    "posterize",
    Interface(inputs=(_LEVELS, _CHANNELS, _RETAIN_HUE), panels=(_DITHER, _DITHER_CELL)),
    description="Rounds display values to evenly spaced levels with hue retention and Bayer dithering",
    toggle=Some(True),
)
_GRADIENT: Final = InterfacePanel(key="gradient", name="Gradient Map", description="Luminance mapped through a color ramp", toggle=Some(False))

# --- [MODELS] ---------------------------------------------------------------------------


class Pixelation(CompositorRecord, frozen=True, kw_only=True):
    """Resamples display values into blocks or cells before palette reduction, cell borders shaped by a Voronoi distance."""

    distance: Annotated[str, _DISTANCE, msgspec.Meta(description="Voronoi distance shaping cell borders")] = _DISTANCE.prop.default

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Builds mosaic block means and Voronoi cells filled at their sites, each replacing the whole frame."""
        frame = shell.space
        offset, middle, side = vector("FLOOR", _drawn(type(self), shell, _CELLS)), vector("SCALE", frame.extent, 0.5), shell.input(_CELLS, _SPACING)
        site = Voronoi(voronoi_dimensions="2D", distance=self.distance, scale=1.0).node(
            (frame.point - middle) / side + offset,
            socket="Position",
            socket_type=bpy.types.NodeSocketVector,
            linked=Some({Voronoi.randomness: shell.input(_CELLS, _SITE_JITTER), Voronoi.exponent: shell.input(_CELLS, _EXPONENT)}),
        )
        cells = uv_mapped(shell.color, frame.sampled(vector("MULTIPLY_ADD", site - offset, side, middle) / frame.extent), extension=("Extend", "Extend"))
        return Ok(shell.menu(_SAMPLING, cases={_MOSAIC.item: block_mean(frame, shell.color, shell.input(_MOSAIC, _BLOCK)), _CELLS.item: cells}))

    requested = staticmethod(precise)


class Palette(CompositorRecord, frozen=True, kw_only=True):
    """Rounds display colors to evenly spaced levels, then maps luminance through a color ramp."""

    gradient: Annotated[Ramp, msgspec.Meta(description="Scene colors along luminance 0 to 1 of the gradient map")] = Ramp()

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Quantizes straight display color under an 8-by-8 Bayer threshold, three interleaved bit planes of the cell index, and maps luminance through the color ramp.

        Args:
            shell: Entry inputs and gradient ramp.

        Returns:
            Straight color, or the HSY group faults.
        """
        source = shell.color
        spans = (1.0, 2.0, 4.0)
        ranks = 4 ** len(spans)

        chosen = shell.menu(_POSTERIZE, _CHANNELS, cases={_RGB: source, _LUMINANCE: _toned(source)})
        cell = switched(shell.input(_POSTERIZE, _DITHER_CELL), (1.0, 1.0, 0.0), vector("MAXIMUM", (1.0, 1.0, 0.0), shell.space.pixel_size(shell.input(_POSTERIZE, _DITHER_CELL, _SIDE))))
        indices = vector("FLOOR", vector("DIVIDE", PixelGrid.of(source).point, cell))
        x_bits, y_bits = (vector("SNAP", indices[axis], spans) for axis in ("X", "Y"))
        periods = tuple(2.0 * span for span in spans)
        rank = vector("MULTIPLY_ADD", vector("MODULO", x_bits + y_bits, periods), (2.0,) * 3, vector("MODULO", y_bits, periods))
        weights = tuple(4.0 ** (len(spans) - 1 - bit) / (span * ranks) for bit, span in enumerate(spans))
        offset = (vector("DOT_PRODUCT", rank, weights) + (0.5 / ranks - 0.5)) * (shell.input(_POSTERIZE, _DITHER) * shell.input(_POSTERIZE, _DITHER, _STRENGTH))
        steps = shell.input(_POSTERIZE, _LEVELS) - 1.0
        rounded = Expr(
            kind=bpy.types.ShaderNodeMapRange,
            inputs=(
                ("Vector", vector("FLOOR", vector("MULTIPLY_ADD", chosen, steps, offset) + (0.5,) * 3)),
                ("From_Min_FLOAT3", (0.0, 0.0, 0.0)),
                ("From_Max_FLOAT3", steps),
                ("To_Min_FLOAT3", (0.0, 0.0, 0.0)),
                ("To_Max_FLOAT3", (1.0, 1.0, 1.0)),
            ),
            members={"data_type": "FLOAT_VECTOR", "clamp": True},
            socket="Vector",
            socket_type=bpy.types.NodeSocketVector,
        )

        def finished(kept: Expr) -> Expr:
            retention = shell.input(_POSTERIZE, _RETAIN_HUE) * chosen.at(_RGB.name)
            posterized = switched(shell.input(_POSTERIZE), source, mixed(retention, rounded, kept))
            ramp = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", _toned(posterized)),), content=Some(self.gradient), socket="Color", socket_type=bpy.types.NodeSocketColor).keyed(
                "gradient_value"
            )
            return switched(shell.input(_GRADIENT), posterized, ramp)

        forward = PyOpenColorIO.TRANSFORM_DIR_FORWARD
        return (
            paired(hsy(shell.config, forward, source), hsy(shell.config, forward, rounded))
            .bind(lambda pair: hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_INVERSE, rgba(pair[0]["Red"], pair[1]["Green"], pair[1]["Blue"])))
            .map(finished)
        )


class Pattern(CompositorRecord, frozen=True, kw_only=True):
    """Replaces display values with dots, glyphs, or streaks after palette reduction, glyphs drawn from characters ordered from least to most coverage."""

    glyphs: Annotated[str, msgspec.Meta(min_length=1, description="Characters ordered from least to most coverage")] = " .:-=+*#%@"

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Builds halftone screens, tone-ranked glyph strip lookups, and pixel sort streaks, each with its own coverage, kept as the structure weight."""
        source, frame = shell.color, shell.space
        centered = frame.point - vector("SCALE", frame.extent, 0.5)

        def screened(tone: Expr, instance: int, offset: Scalar | tuple[float, ...] = (0.0, 0.0, 0.0), multiple: Scalar = 1.0) -> Expr:
            return _halftone_screen(
                coordinate=centered,
                offset=vector("ADD", vector("FLOOR", _drawn(type(self), shell, _HALFTONE, instance)), offset),
                angle=shell.input(_HALFTONE, _ANGLE),
                pitch=math("MULTIPLY", shell.input(_HALFTONE, _PITCH), multiple),
                jitter=shell.input(_HALFTONE, _DOT_JITTER),
                tone=tone,
            )

        mono = screened(1.0 - _toned(source), 0)
        peak = math("MAXIMUM", math("MAXIMUM", source["Red"], source["Green"]), source["Blue"])
        separated = vector("DIVIDE", vector("SUBTRACT", peak, source), peak)
        tones = (separated["X"], separated["Y"], separated["Z"], 1.0 - peak)
        clear = tuple(
            1.0 - screened(tone, instance, shell.input(_HALFTONE, _PROCESS, separation, _OFFSET), shell.input(_HALFTONE, _PROCESS, separation, _SCALE))
            for instance, (separation, tone) in enumerate(zip(_SEPARATIONS, tones))
        )
        halftone = shell.menu(
            _HALFTONE,
            _SCREENS,
            cases={
                _MONO.item: with_alpha(mixed(mono, shell.input(_HALFTONE, _MONO, _DOT_PAPER), shell.input(_HALFTONE, _MONO, _DOT_COLOR)), mono),
                _PROCESS.item: with_alpha(vector("SCALE", xyz(*clear[:3]), clear[3]), 1.0 - clear[0] * clear[1] * clear[2] * clear[3]),
            },
        )
        glyphs = self.glyphs
        levels = len(glyphs)
        height = shorter(frame.extent) / shell.input(_GLYPHS, _ROWS)
        strip = Expr(
            kind=bpy.types.CompositorNodeStringToImage, inputs=(("String", "\n".join(glyphs)), ("Size", height * frame.greater), ("Wrap", False)), socket="Image", socket_type=bpy.types.NodeSocketColor
        )
        sides = dimensions(strip)
        cell = xyz(height * (sides["X"] / sides["Y"] * levels), height, 1.0)
        indices, within = vector("FLOOR", frame.point / cell), vector("FRACTION", frame.point / cell)
        center = uv_mapped(source, frame.sampled(vector("ADD", indices, (0.5, 0.5, 0.0)) * cell / frame.extent), extension=("Extend", "Extend"))
        draw = WhiteNoise().node(indices + _drawn(type(self), shell, _GLYPHS))
        tone = math("MULTIPLY_ADD", draw - 0.5, shell.input(_GLYPHS, _TONE_JITTER), _toned(center), clamp=True)
        rank = (levels - 1) - math("MINIMUM", math("FLOOR", tone * levels), levels - 1)
        mask = uv_mapped(strip, xyz(within["X"], (rank + within["Y"]) / levels, 1.0), extension=("Extend", "Extend"))
        random = Expr(
            kind=bpy.types.CompositorNodeCombineColor,
            inputs=(("Red", draw.at("Color", bpy.types.NodeSocketColor)["Red"]), ("Green", shell.input(_GLYPHS, _RANDOM_COLOR, _SATURATION)), ("Blue", 1.0), ("Alpha", 1.0)),
            members={"mode": "HSV"},
            socket="Image",
            socket_type=bpy.types.NodeSocketColor,
        )
        color = shell.menu(_GLYPHS, _COLORING, cases={_SOURCE_COLOR: center, _TWO_TONE.item: shell.input(_GLYPHS, _TWO_TONE, _GLYPH_COLOR), _RANDOM_COLOR.item: random})

        def sorted_along(heading: Expr) -> Expr:
            axis = abs(heading)

            def along(image: Scalar, reach: Socket) -> Expr:
                return blurred(image, frame.pixel_size(vector("SCALE", axis, shell.input(_PIXEL_SORT, reach))))

            soft = along(_toned(source), _SOFTNESS)
            low, high = shell.input(_PIXEL_SORT, _LOW), shell.input(_PIXEL_SORT, _HIGH)
            above, below = 1.0 - (soft < low), 1.0 - (soft > high)
            band = mixed(low > high, above * below, math("MAXIMUM", above, below))
            grain = WhiteNoise(noise_dimensions="2D").node(frame.point + _drawn(type(self), shell, _PIXEL_SORT, 0))
            streak = Noise(noise_dimensions="1D").node(
                (0.0, 0.0, 0.0),
                vector("DOT_PRODUCT", frame.fraction, (1.0, 1.0, 0.0) - axis) + math("MULTIPLY_ADD", _toned(source), 2.0, -1.0) + _drawn(type(self), shell, _PIXEL_SORT, 1)["X"],
                linked=Some({Noise.roughness: shell.input(_PIXEL_SORT, _CLUMP)}),
            )
            position = vector("DOT_PRODUCT", frame.fraction, axis)
            fade = MapRange(from_max=0.0, to_min=1.0, to_max=0.0).node(math("MINIMUM", position, 1.0 - position), linked=Some({MapRange.from_min: shell.input(_PIXEL_SORT, _EDGE_FADE)}))
            weight = math("MULTIPLY", block_mean(frame, math("MULTIPLY", along(band, _FALLOFF), 1.0 - grain * shell.input(_PIXEL_SORT, _SCATTER)) * streak, shell.input(_PIXEL_SORT, _CHUNK)), fade)
            displaced = Expr(
                kind=bpy.types.CompositorNodeDisplace,
                inputs=(
                    ("Image", source),
                    ("Displacement", frame.pixel_size(vector("SCALE", vector("SCALE", heading, shell.input(_PIXEL_SORT, _LENGTH)), weight))),
                    ("Extension X", "Extend"),
                    ("Extension Y", "Extend"),
                ),
                socket="Image",
                socket_type=bpy.types.NodeSocketColor,
            )
            return with_alpha(mixed(weight, source, displaced), weight)

        structured = shell.menu(
            _REPLACING,
            cases={
                _HALFTONE.item: halftone,
                _GLYPHS.item: with_alpha(mixed(mask, shell.input(_GLYPHS, _GLYPH_PAPER), color), mask),
                _PIXEL_SORT.item: sorted_along(shell.menu(_PIXEL_SORT, _DIRECTION, cases=_HEADINGS)),
            },
        )
        return Ok(structured.keeping(structured["Alpha"].keyed("structure_weight")))

    requested = staticmethod(precise)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TONES]


def _toned(image: Scalar) -> Expr:
    """Display luma of an image through the RGB to BW node."""
    return Expr(kind=bpy.types.CompositorNodeRGBToBW, inputs=(("Image", image),), socket="Val")


# --- [SCREENS]


@grouped(kind=bpy.types.CompositorNodeTree)
def _halftone_screen(
    coordinate: Annotated[Expr, bpy.types.NodeSocketVector], offset: Annotated[Expr, bpy.types.NodeSocketVector], angle: Expr, pitch: Expr, jitter: Expr, tone: Expr
) -> Mapping[Key, Expr]:
    """Unit-square dot coverage of a rotated dot lattice, antialiased over pixel derivative width.

    Args:
        coordinate: Displayed point about frame center.
        offset: Lattice shift in cells.
        angle: Screen turn.
        pitch: Lattice spacing.
        jitter: Dot displacement within its cell.
        tone: Share each cell covers.

    Returns:
        Coverage output.
    """
    cosine, sine = math("COSINE", angle), math("SINE", angle)
    x, y = coordinate["X"], coordinate["Y"]
    lattice = vector("DIVIDE", xyz(math("MULTIPLY_ADD", x, cosine, y * sine), math("MULTIPLY_ADD", y, cosine, -(x * sine))), pitch) + offset
    sites = Voronoi(voronoi_dimensions="2D", scale=1.0).node(lattice, socket="Distance", linked=Some({Voronoi.randomness: jitter}))
    disc = sites * sites * np.pi
    radius = math("MAXIMUM", sites, 0.5)
    segment = math("MULTIPLY_ADD", radius * radius, math("ARCCOSINE", 0.5 / radius), math("SQRT", radius * radius - 0.25) * -0.5)
    area = where(sites < float(np.sqrt(0.5)), where(math("GREATER_THAN", sites, 0.5), disc - segment * 4.0, disc), 1.0)
    grid = PixelGrid.of(area)

    def slope(step: tuple[float, float, float]) -> Expr:
        return abs(math("SUBTRACT", uv_mapped(area, grid.at(step, grid.inverse), interpolation="Nearest", extension=("Extend", "Extend")), area))

    width = slope((1.0, 0.0, 0.0)) + slope((0.0, 1.0, 0.0))
    inside = where(width > 0.0, math("ADD", tone, (tone - area) / width, clamp=True), area < tone)
    return {"coverage": (tone > 0.0) * where(tone < 1.0, inside, 1.0)}


# --- [STRUCTURES]


def _drawn(kind: type[CompositorRecord], shell: GraphInputs, structure: InterfacePanel, instance: int = 0) -> Expr:
    """Decorrelation origin of one structure's draws, redrawn per whole clock unit."""
    return origin(shell.input(SEED), stream(kind, structure.key), math("FLOOR", clocked(shell.input, _CLOCK)), instance)


# --- [COMPOSITION] ----------------------------------------------------------------------

pixelation: Final = CompositorEffect(record=Pixelation, interface=Interface(inputs=(_SAMPLING, SEED), panels=(_MOSAIC, _CELLS, _CLOCK)), mix=Mix.RESULT, derives=(timing,))
palette: Final = CompositorEffect(record=Palette, interface=Interface(panels=(_POSTERIZE, _GRADIENT)), mix=Mix.STRAIGHT)
pattern: Final = CompositorEffect(record=Pattern, interface=Interface(inputs=(_REPLACING, SEED), panels=(_HALFTONE, _GLYPHS, _PIXEL_SORT, _CLOCK)), mix=Mix.STRAIGHT, derives=(timing,))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Palette", "Pattern", "Pixelation", "palette", "pattern", "pixelation"]
