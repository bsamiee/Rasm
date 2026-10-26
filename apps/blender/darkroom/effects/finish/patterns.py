"""Display structures: mosaic blocks and Voronoi cells resampling the frame, then halftone screens, tone-ranked glyphs, and pixel sort streaks replacing it."""

from collections.abc import Mapping
from typing import Annotated, Final, Literal

import bpy
from expression import Some
import msgspec
import numpy as np

from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Expr, grouped, math, mixed, Scalar, vector, where, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.textures import stream
from blender.bpy.render.compositor.coordinates import clocked, gridded, origin, pixels, sampled, shorter
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, precise, Requests, Shell
from blender.bpy.render.compositor.filters import block_mean
from blender.imaging.color.processors import Domain
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from ..optics.procedural import CLOCK

# --- [CONSTANTS] ------------------------------------------------------------------------

_DRAWS: Final = (
    msgspec.structs.replace(
        CLOCK, inputs=tuple(msgspec.structs.replace(socket, members={**socket.members, "default_value": 0.0}) if socket.key == "clock_speed" else socket for socket in CLOCK.inputs)
    ),
    Socket(key="seed", name="Seed", kind=bpy.types.NodeSocketInt, description="Number selecting every random site, tone, and streak", members={"min_value": 0, "max_value": 2**24}),
)

# --- [MODELS] ---------------------------------------------------------------------------


class Pixelation(Effect, frozen=True, kw_only=True):
    """Resamples display values into blocks or cells before palette reduction, cell borders shaped by a Voronoi distance."""

    stage = Stage.PIXELATE
    pointwise = False
    distance: Annotated[Literal["EUCLIDEAN", "MANHATTAN", "CHEBYCHEV"], msgspec.Meta(description="Voronoi distance shaping cell borders")] = "EUCLIDEAN"


class Pattern(Effect, frozen=True, kw_only=True):
    """Replaces display values with dots, glyphs, or streaks after palette reduction, glyphs drawn from characters ordered from least to most ink."""

    stage = Stage.PATTERN
    pointwise = False
    glyphs: Annotated[str, msgspec.Meta(min_length=1, description="Characters ordered from least to most ink")] = " .:-=+*#%@"


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SCREENS]


@grouped("halftone_screen", "Halftone Screen", bpy.types.CompositorNodeTree)
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
    disc = sites * sites * np.pi
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


def drawn(shell: Shell[Pixelation] | Shell[Pattern], key: Key, instance: int = 0) -> Expr:
    """Decorrelation origin of one structure's draws, redrawn per whole clock frame."""
    return origin(shell.input("seed"), stream(type(shell.record), key), math("FLOOR", clocked(shell.input("clock_speed"), shell.input("clock_hold"))), instance)


def filled(shell: Shell[Pixelation] | Shell[Pattern], uv: Expr) -> Expr:
    """Bilinear entry color at Map UV positions."""
    return Expr(
        kind=bpy.types.CompositorNodeMapUV,
        inputs=(("Image", shell.color), ("UV", uv), ("Interpolation", "Bilinear"), ("Extension X", "Extend"), ("Extension Y", "Extend")),
        socket="Image",
        shape="RGBA",
    )


def structured(shell: Shell[Pixelation] | Shell[Pattern], choices: Mapping[str, Expr], *, covered: bool) -> Outcome[Expr]:
    """Selects the structure color keeping its replacement weight, the structure's alpha when covered, else the whole frame."""
    return shell.menu("structure", cases=choices, kind="RGBA").map(
        lambda result: result.keeping(
            Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", result if covered else (1.0,) * 4),), members=(("mode", "RGB"),), socket="Alpha").named("structure_weight")
        )
    )


def toned(image: Scalar) -> Expr:
    """Luminance of an image through RGB to BW."""
    return Expr(kind=bpy.types.CompositorNodeRGBToBW, inputs=(("Image", image),), socket="Val")


def pixelated(shell: Shell[Pixelation]) -> Outcome[Expr]:
    """Builds mosaic block means and Voronoi cells filled at their sites, each replacing the whole frame."""
    frame = shell.space
    offset, middle, side = vector("FLOOR", drawn(shell, "cells")), vector("SCALE", frame.extent, 0.5), shell.input("cells", "size")
    site = Expr(
        kind=bpy.types.ShaderNodeTexVoronoi,
        inputs=(("Vector", (frame.point - middle) / side + offset), ("Scale", 1.0), ("Detail", 0.0), ("Randomness", shell.input("cells", "jitter"))),
        members=(("voronoi_dimensions", "2D"), ("feature", "F1"), ("distance", shell.record.distance), ("normalize", False)),
        socket="Position",
        shape="VECTOR",
    )
    cells = filled(shell, sampled(frame, vector("MULTIPLY_ADD", site - offset, side, middle) / frame.extent))
    return structured(shell, {"Mosaic": block_mean(frame, shell.color, shell.input("mosaic", "size")), "Cells": cells}, covered=False)


def patterned(shell: Shell[Pattern]) -> Outcome[Expr]:
    """Builds halftone screens, tone-ranked glyph strip lookups, and pixel sort streaks, each with its own coverage."""
    source, frame = shell.color, shell.space
    centered = frame.point - vector("SCALE", frame.extent, 0.5)

    def weighted(color: Scalar, weight: Scalar) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", color), ("Alpha", weight), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

    def screened(tone: Expr, instance: int, offset: Scalar | tuple[float, ...] = (0.0, 0.0, 0.0), multiple: Scalar = 1.0) -> Expr:
        return halftone_screen(
            coordinate=centered,
            offset=vector("ADD", vector("FLOOR", drawn(shell, "halftone", instance)), offset),
            angle=shell.input("halftone", "angle"),
            pitch=math("MULTIPLY", shell.input("halftone", "pitch"), multiple),
            jitter=shell.input("halftone", "jitter"),
            tone=tone,
        )

    mono = screened(1.0 - toned(source), 0)
    peak = math("MAXIMUM", math("MAXIMUM", source["Red"], source["Green"]), source["Blue"])
    separations = vector("DIVIDE", vector("SUBTRACT", peak, source), peak)
    tones = (("cyan", separations["X"]), ("magenta", separations["Y"]), ("yellow", separations["Z"]), ("key", 1.0 - peak))
    clear = tuple(1.0 - screened(tone, instance, shell.input("halftone", "process", ink, "offset"), shell.input("halftone", "process", ink, "scale")) for instance, (ink, tone) in enumerate(tones))
    inked = {
        "Mono": weighted(mixed(mono, shell.input("halftone", "mono", "paper"), shell.input("halftone", "mono", "ink")), mono),
        "Process": weighted(vector("SCALE", xyz(*clear[:3]), clear[3]), 1.0 - clear[0] * clear[1] * clear[2] * clear[3]),
    }
    glyphs = shell.record.glyphs
    levels = len(glyphs)
    height = shorter(frame.extent) / shell.input("glyphs", "rows")
    strip = Expr(kind=bpy.types.CompositorNodeStringToImage, inputs=(("String", "\n".join(glyphs)), ("Size", height * frame.greater), ("Wrap", False)), socket="Image", shape="RGBA")
    sides = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", strip),), socket="Dimensions", shape="VECTOR")
    cell = xyz(height * (sides["X"] / sides["Y"] * levels), height, 1.0)
    indices, within = vector("FLOOR", frame.point / cell), vector("FRACTION", frame.point / cell)
    center = filled(shell, sampled(frame, vector("ADD", indices, (0.5, 0.5, 0.0)) * cell / frame.extent))
    draw = Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", indices + drawn(shell, "glyphs")),), members=(("noise_dimensions", "3D"),))
    tone = math("MULTIPLY_ADD", draw - 0.5, shell.input("glyphs", "jitter"), toned(center), clamp=True)
    rank = (levels - 1) - math("MINIMUM", math("FLOOR", tone * levels), levels - 1)
    mask = Expr(
        kind=bpy.types.CompositorNodeMapUV,
        inputs=(("Image", strip), ("UV", xyz(within["X"], (rank + within["Y"]) / levels, 1.0)), ("Interpolation", "Bilinear"), ("Extension X", "Extend"), ("Extension Y", "Extend")),
        socket="Image",
        shape="RGBA",
    )
    random = Expr(
        kind=bpy.types.CompositorNodeCombineColor,
        inputs=(("Red", msgspec.structs.replace(draw, socket="Color", shape="RGBA")["X"]), ("Green", shell.input("glyphs", "random_color", "saturation")), ("Blue", 1.0), ("Alpha", 1.0)),
        members=(("mode", "HSV"),),
        socket="Image",
        shape="RGBA",
    )
    colorings = {"Source Color": center, "Two Tone": shell.input("glyphs", "two_tone", "ink"), "Random Color": random}

    def sorting(heading: Expr) -> Expr:
        axis = abs(heading)

        def blurred(image: Scalar, reach: str) -> Expr:
            return Expr(
                kind=bpy.types.CompositorNodeBlur,
                inputs=(("Image", image), ("Size", pixels(frame, vector("SCALE", axis, shell.input("pixel_sort", reach)), "VECTOR")), ("Type", "Gaussian")),
                socket="Image",
                shape="RGBA",
            )

        soft = blurred(toned(source), "softness")
        low, high = shell.input("pixel_sort", "low"), shell.input("pixel_sort", "high")
        above, below = 1.0 - (soft < low), 1.0 - (soft > high)
        band = mixed(low > high, above * below, math("MAXIMUM", above, below))
        grain = Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", frame.point + drawn(shell, "pixel_sort", 0)),), members=(("noise_dimensions", "2D"),))
        streak = Expr(
            kind=bpy.types.ShaderNodeTexNoise,
            inputs=(
                ("W", vector("DOT_PRODUCT", frame.fraction, (1.0, 1.0, 0.0) - axis) + math("MULTIPLY_ADD", toned(source), 2.0, -1.0) + drawn(shell, "pixel_sort", 1)["X"]),
                ("Roughness", shell.input("pixel_sort", "clump")),
            ),
            members=(("noise_dimensions", "1D"), ("noise_type", "FBM"), ("normalize", True)),
            socket="Fac",
        )
        along = vector("DOT_PRODUCT", frame.fraction, axis)
        fade = Expr(
            kind=bpy.types.ShaderNodeMapRange,
            inputs=(("Value", math("MINIMUM", along, 1.0 - along)), ("From Min", shell.input("pixel_sort", "edge_fade")), ("From Max", 0.0), ("To Min", 1.0), ("To Max", 0.0)),
            members=(("clamp", True),),
            socket="Result",
        )
        weight = math("MULTIPLY", block_mean(frame, math("MULTIPLY", blurred(band, "falloff"), 1.0 - grain * shell.input("pixel_sort", "scatter")) * streak, shell.input("pixel_sort", "chunk")), fade)
        displaced = Expr(
            kind=bpy.types.CompositorNodeDisplace,
            inputs=(
                ("Image", source),
                ("Displacement", pixels(frame, vector("SCALE", vector("SCALE", heading, shell.input("pixel_sort", "length")), weight), "VECTOR")),
                ("Interpolation", "Bilinear"),
                ("Extension X", "Extend"),
                ("Extension Y", "Extend"),
            ),
            socket="Image",
            shape="RGBA",
        )
        return weighted(mixed(weight, source, displaced), weight)

    headings = {"Left": (-1.0, 0.0, 0.0), "Right": (1.0, 0.0, 0.0), "Up": (0.0, 1.0, 0.0), "Down": (0.0, -1.0, 0.0)}
    return shell.menu("halftone", "inks", cases=inked, kind="RGBA").bind(
        lambda halftone: shell.menu("glyphs", "coloring", cases=colorings, kind="RGBA").bind(
            lambda color: shell.menu("pixel_sort", "direction", cases=headings, kind="VECTOR").bind(
                lambda heading: structured(shell, {"Halftone": halftone, "Glyphs": weighted(mixed(mask, shell.input("glyphs", "paper"), color), mask), "Pixel Sort": sorting(heading)}, covered=True)
            )
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

pixelation: Final = CompositorEffect(
    record=Pixelation,
    graph=pixelated,
    interface=Interface(
        inputs=(
            Socket(
                key="structure",
                name="Structure",
                kind=bpy.types.NodeSocketMenu,
                description="Sampling blocks or cells",
                members={"default_value": "Mosaic"},
                items=Some(
                    MenuItems(
                        items=(MenuItem(name="Mosaic", description="Replaces frame with square blocks"), MenuItem(name="Cells", description="Fills each Voronoi cell with image color at its site"))
                    )
                ),
            ),
            _DRAWS[1],
        ),
        panels=(
            *(
                InterfacePanel(
                    key=key,
                    name=bpy.path.display_name(key),
                    inputs=tuple(
                        Socket(key=item_key(key, field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=description, members={"subtype": subtype, **members})
                        for field, subtype, description, members in rows
                    ),
                )
                for key, rows in (
                    ("mosaic", (("size", "NONE", "Block side over greater frame side", {"min_value": 1e-6, "default_value": 0.005}),)),
                    (
                        "cells",
                        (
                            ("size", "NONE", "Mean cell site spacing over greater frame side", {"min_value": 1e-6, "default_value": 0.0225}),
                            ("jitter", "FACTOR", "Site displacement within lattice cell", {"min_value": 0.0, "max_value": 1.0, "default_value": 1.0}),
                        ),
                    ),
                )
            ),
            _DRAWS[0],
        ),
    ),
    domain=Domain.DISPLAY,
    alpha=Alpha.RESULT,
    parts=(Requests[Pixelation](request=precise),),
)
pattern: Final = CompositorEffect(
    record=Pattern,
    graph=patterned,
    interface=Interface(
        inputs=(
            Socket(
                key="structure",
                name="Structure",
                kind=bpy.types.NodeSocketMenu,
                description="Cells or runs over frame",
                members={"default_value": "Halftone"},
                items=Some(
                    MenuItems(
                        items=(
                            MenuItem(name="Halftone", description="Prints tone as dots with edges antialiased over one pixel"),
                            MenuItem(name="Glyphs", description="Replaces text-grid cells with glyphs selected by their tone"),
                            MenuItem(name="Pixel Sort", description="Streaks pixels in a wrapped luma band along one direction"),
                        )
                    )
                ),
            ),
            _DRAWS[1],
        ),
        panels=(
            InterfacePanel(
                key="halftone",
                name="Halftone",
                inputs=(
                    *(
                        Socket(key=item_key("halftone", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=description, members={"subtype": subtype, **members})
                        for field, subtype, description, members in (
                            ("pitch", "NONE", "Screen lattice spacing over greater frame side", {"min_value": 1e-6, "default_value": 1 / 16}),
                            ("angle", "ANGLE", "Screen turn about frame center", {"default_value": np.pi / 4}),
                            ("jitter", "FACTOR", "Dot displacement within cell", {"min_value": 0.0, "max_value": 1.0}),
                        )
                    ),
                    Socket(
                        key="halftone_inks",
                        name="Inks",
                        kind=bpy.types.NodeSocketMenu,
                        description="One luma screen or four process screens",
                        members={"default_value": "Mono"},
                        items=Some(
                            MenuItems(
                                items=(
                                    MenuItem(name="Mono", description="Screens luma in one ink on paper"),
                                    MenuItem(name="Process", description="Prints cyan, magenta, yellow, and key screens on white paper"),
                                )
                            )
                        ),
                    ),
                ),
                panels=(
                    InterfacePanel(
                        key="halftone_mono",
                        name="Mono",
                        inputs=tuple(
                            Socket(key=item_key("halftone_mono", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketColor, description=description, members={"default_value": color})
                            for field, description, color in (("ink", "Dot display color", (0.0, 0.0, 0.0, 1.0)), ("paper", "Display color between dots", (1.0, 1.0, 1.0, 1.0)))
                        ),
                    ),
                    InterfacePanel(
                        key="halftone_process",
                        name="Process",
                        panels=tuple(
                            InterfacePanel(
                                key=item_key("halftone_process", ink),
                                name=bpy.path.display_name(ink),
                                description=f"{bpy.path.display_name(ink)} screen lattice",
                                inputs=(
                                    Socket(
                                        key=item_key("halftone_process", ink, "offset"),
                                        name="Offset",
                                        kind=bpy.types.NodeSocketVector,
                                        description="Lattice shift in cells",
                                        members={"dimensions": 2, "default_value": offset},
                                    ),
                                    Socket(
                                        key=item_key("halftone_process", ink, "scale"),
                                        name="Scale",
                                        kind=bpy.types.NodeSocketFloat,
                                        description="Multiple of halftone pitch",
                                        members={"min_value": 1e-6, "default_value": 1.0},
                                    ),
                                ),
                            )
                            for ink, offset in (("cyan", (0.772, 0.208)), ("magenta", (0.208, 0.772)), ("yellow", (0.0, 0.8)), ("key", (0.5656, 0.5656)))
                        ),
                    ),
                ),
            ),
            InterfacePanel(
                key="glyphs",
                name="Glyphs",
                inputs=(
                    Socket(key="glyphs_rows", name="Rows", kind=bpy.types.NodeSocketInt, description="Glyph cells across short side", members={"min_value": 1, "default_value": 24}),
                    Socket(
                        key="glyphs_jitter",
                        name="Jitter",
                        kind=bpy.types.NodeSocketFloat,
                        description="Random shift of each cell tone",
                        members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0},
                    ),
                    Socket(key="glyphs_paper", name="Paper", kind=bpy.types.NodeSocketColor, description="Display color behind glyphs", members={"default_value": (0.0, 0.0, 0.0, 1.0)}),
                    Socket(
                        key="glyphs_coloring",
                        name="Coloring",
                        kind=bpy.types.NodeSocketMenu,
                        description="Glyph color source",
                        members={"default_value": "Source Color"},
                        items=Some(
                            MenuItems(
                                items=(
                                    MenuItem(name="Source Color", description="Draws each glyph in its cell's color"),
                                    MenuItem(name="Two Tone", description="Draws every glyph in one ink"),
                                    MenuItem(name="Random Color", description="Draws each glyph in a random hue per cell"),
                                )
                            )
                        ),
                    ),
                ),
                panels=(
                    InterfacePanel(
                        key="glyphs_two_tone",
                        name="Two Tone",
                        inputs=(Socket(key="glyphs_two_tone_ink", name="Ink", kind=bpy.types.NodeSocketColor, description="Glyph display color", members={"default_value": (1.0, 1.0, 1.0, 1.0)}),),
                    ),
                    InterfacePanel(
                        key="glyphs_random_color",
                        name="Random Color",
                        inputs=(
                            Socket(
                                key="glyphs_random_color_saturation",
                                name="Saturation",
                                kind=bpy.types.NodeSocketFloat,
                                description="Chroma of drawn colors",
                                members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": 1.0},
                            ),
                        ),
                    ),
                ),
            ),
            InterfacePanel(
                key="pixel_sort",
                name="Pixel Sort",
                inputs=(
                    Socket(
                        key="pixel_sort_direction",
                        name="Direction",
                        kind=bpy.types.NodeSocketMenu,
                        description="Streak direction",
                        members={"default_value": "Right"},
                        items=Some(MenuItems(items=(MenuItem(name="Left"), MenuItem(name="Right"), MenuItem(name="Up"), MenuItem(name="Down")))),
                    ),
                    *(
                        Socket(key=item_key("pixel_sort", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=description, members={"subtype": subtype, **members})
                        for field, subtype, description, members in (
                            ("low", "FACTOR", "Lower luma boundary, wrapping when above High", {"min_value": 0.0, "max_value": 1.0}),
                            ("high", "FACTOR", "Upper luma boundary", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.3}),
                            ("length", "FACTOR", "Longest streak as a frame share", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.3}),
                            ("softness", "NONE", "Luma blur along axis before band key over greater frame side", {"min_value": 0.0, "default_value": 0.01}),
                            ("falloff", "NONE", "Key blur along axis over greater frame side", {"min_value": 0.0, "default_value": 0.04}),
                            ("scatter", "FACTOR", "Run start noise", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.2}),
                            ("clump", "FACTOR", "Streak field roughness across axis", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.5}),
                            ("chunk", "NONE", "Streak map block size over greater frame side", {"min_value": 0.0, "default_value": 0.0005}),
                            ("edge_fade", "FACTOR", "Streak fade toward frame ends along axis", {"min_value": 0.0, "max_value": 1.0}),
                        )
                    ),
                ),
            ),
            _DRAWS[0],
        ),
    ),
    domain=Domain.DISPLAY,
    alpha=Alpha.STRAIGHT,
    parts=(Requests[Pattern](request=precise),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Pattern", "Pixelation", "pattern", "pixelation"]
