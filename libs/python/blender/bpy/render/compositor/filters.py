"""Filter primitives a compositor graph composes: rank statistics over tap sets, bilateral grids, variance-kept tile sampling, kernel convolution, and block means."""

from functools import reduce
from itertools import product, starmap

import bpy
from expression import Some
import numpy as np
import numpy.typing as npt

from blender.bpy.data.ids.images import Generated
from blender.bpy.nodes.graphs import Component, Expr, math, rgba, Scalar, switched, Term, vector, where, xyz
from blender.bpy.render.compositor.coordinates import dimensions, FrameSpace, gridded, pixels

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [STATISTICS]


def ranked(source: Expr, offsets: tuple[tuple[float, float], ...], rank: int) -> tuple[Expr, Expr]:
    """Orders a single-channel source over pixel offsets through a Batcher merge network, taps outside the data window ordered first.

    Args:
        source: Single-channel image sampled at each offset.
        offsets: Pixel offsets of the taps.
        rank: Position from the brightest of the value read.

    Returns:
        The `rank`-th brightest tap value and the count of taps inside the data window.
    """
    grid = gridded(source)
    count = len(offsets)
    pairs = tuple(
        (i + j, i + j + k)
        for p in (1 << exponent for exponent in range(count.bit_length() - 1))
        for k in (p >> shift for shift in range(p.bit_length()))
        for j in range(k % p, count - k, 2 * k)
        for i in range(min(k, count - j - k))
        if (i + j) // (2 * p) == (i + j + k) // (2 * p)
    )

    def tap(dx: float, dy: float) -> tuple[Expr, Expr]:
        uv = grid.at((dx, dy, 0.0), grid.inverse)
        sampled = Expr(kind=bpy.types.CompositorNodeMapUV, inputs=(("Image", source), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", "Clip"), ("Extension Y", "Clip")), socket="Image")
        return sampled, (uv["X"] > 0.0) * (uv["X"] < 1.0) * ((uv["Y"] > 0.0) * (uv["Y"] < 1.0))

    def exchanged(wires: tuple[tuple[Expr, Expr], ...], pair: tuple[int, int]) -> tuple[tuple[Expr, Expr], ...]:
        low, high = pair
        (left, left_valid), (right, right_valid) = wires[low], wires[high]
        choose = (left_valid > right_valid) + math("COMPARE", left_valid, right_valid, 0.0) * (left > right)
        moved = {low: (where(choose, right, left), math("MINIMUM", left_valid, right_valid)), high: (where(choose, left, right), math("MAXIMUM", left_valid, right_valid))}
        return tuple(starmap(moved.get, enumerate(wires)))

    taps = tuple(starmap(tap, offsets))
    value, _ = reduce(exchanged, pairs, taps)[count - rank]
    return value, sum((valid for _, valid in taps[1:]), taps[0][1])


def bilateral(value: Expr, guide: Expr, coverage: Expr, frame: FrameSpace, fallback: Scalar, *, bounds: tuple[float, float], pairs: int, radius: float) -> Expr:
    """Smooths a value within guide slices through a coverage-weighted bilateral grid, cells holding no coverage reading the fallback.

    Args:
        value: Value the grid averages.
        guide: Guide whose range splits the grid into slices.
        coverage: Weight of each pixel, its alpha for associated images.
        frame: Displayed frame the blur radius measures against.
        fallback: Value where the guide's slices hold no coverage.
        bounds: Guide range the slices span, values past it clamped to the end slices.
        pairs: Count of slice pairs, each pair packed into one blurred image.
        radius: Blur radius over the greater frame side.

    Returns:
        Grid average at each pixel's guide.
    """
    low, high = bounds
    position = math("MULTIPLY_ADD", guide, 1.0 / (high - low), low / (low - high), clamp=True)
    weights = tuple(math("SUBTRACT", 1.0, abs(math("MULTIPLY_ADD", position, float(2 * pairs - 1), float(-index))), clamp=True) for index in range(2 * pairs))
    weighted = coverage * value
    sides: tuple[tuple[Component, Component], ...] = (("Red", "Green"), ("Blue", "Alpha"))

    def blurred(lower: int, upper: int) -> Expr:
        red, green, blue, alpha = weighted * weights[lower], coverage * weights[lower], weighted * weights[upper], coverage * weights[upper]
        return Expr(
            kind=bpy.types.CompositorNodeBlur,
            inputs=(("Image", rgba(red, green, blue, alpha)), ("Size", pixels(frame, (radius, radius, 0.0), bpy.types.NodeSocketVector)), ("Type", "Fast Gaussian")),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )

    cells = tuple((weights[index], grid[moment], grid[mass]) for lower in range(0, 2 * pairs, 2) for grid in (blurred(lower, lower + 1),) for index, (moment, mass) in zip((lower, lower + 1), sides))
    first, *rest = cells
    numerator, denominator = (reduce(lambda total, cell: math("MULTIPLY_ADD", cell[0], cell[side], total), rest, math("MULTIPLY", first[0], first[side])) for side in (1, 2))
    return where(denominator > 0.0, numerator / denominator, fallback)


# --- [SAMPLING]


def tiled(tile: Expr, position: Expr, density: Scalar, correlation: npt.NDArray[np.float64]) -> Expr:
    """Samples a wrapped tile of standard normal scores, each channel divided by the deviation bilinear interpolation leaves at its sub-texel phase.

    Args:
        tile: Tile image of four channels.
        position: Sampled positions in any planar unit.
        density: Tile texels per position unit.
        correlation: Per-channel neighbor products of the tile from `imaging.sampling.tiles.correlated`.

    Returns:
        Unit-variance samples of every channel.
    """
    step = 1.0 / 256.0
    sides = dimensions(tile)
    scale = xyz(sides["X"], sides["Y"], 1.0)
    texel = vector("SNAP", vector("MULTIPLY_ADD", vector("SCALE", position, density), (1.0, 1.0, 0.0), (0.5 * step, 0.5 * step, 0.0)), (step, step, step))
    sample = Expr(
        kind=bpy.types.CompositorNodeMapUV,
        inputs=(("Image", tile), ("UV", texel / scale), ("Interpolation", "Bilinear"), ("Extension X", "Repeat"), ("Extension Y", "Repeat")),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    )
    phase = vector("FRACTION", texel - (0.5, 0.5, 0.5))
    spread = phase * ((1.0, 1.0, 1.0) - phase)
    across, up = spread["X"], spread["Y"]

    def restored(channel: Component, width: float, height: float, diagonal: float, antidiagonal: float) -> Expr:
        variance = math(
            "MULTIPLY_ADD",
            up,
            math("MULTIPLY_ADD", across, 4.0 - 4.0 * width - 4.0 * height + 2.0 * (diagonal + antidiagonal), -2.0 * (1.0 - height)),
            math("MULTIPLY_ADD", across, -2.0 * (1.0 - width), 1.0),
        )
        return sample[channel] / math("SQRT", variance)

    red, green, blue, alpha = map(restored, ("Red", "Green", "Blue", "Alpha"), *map(tuple, np.asarray(correlation, dtype=np.float64).tolist()))
    return rgba(red, green, blue, alpha)


# --- [CONVOLUTION]


def convolved(source: Term, kernel: Generated) -> Expr:
    """Convolves a source by a generated color kernel whose weights hold their own sums."""
    image = Expr(kind=bpy.types.CompositorNodeImage, content=Some(kernel), socket="Image", shape=bpy.types.NodeSocketColor).named(kernel.ownership.key)
    return Expr(
        kind=bpy.types.CompositorNodeConvolve,
        inputs=(("Image", source), ("Kernel Data Type", "Color"), ("Color Kernel", image), ("Normalize Kernel", False)),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    )


def block_mean(frame: FrameSpace, source: Scalar, length: Scalar) -> Expr:
    """Averages occupied displayed-square bins through rectangular convolution.

    Args:
        frame: Displayed camera placement within the source data window.
        source: Image averaged.
        length: Bin side in displayed units, single pixels at 0.

    Returns:
        Bin mean of each pixel's bin.
    """
    sides = dimensions(source)
    span = switched(math("GREATER_THAN", length, 0.0), (1.0, 1.0, 0.0), pixels(frame, length, bpy.types.NodeSocketVector))
    anchor = vector("MULTIPLY", sides, frame.margin)
    grid = gridded(source)

    def bounded(center: Scalar | tuple[float, ...]) -> tuple[Expr, Expr]:
        cell = vector("FLOOR", vector("DIVIDE", vector("SUBTRACT", center, anchor), span))
        lower = vector("MAXIMUM", vector("CEIL", vector("MULTIPLY_ADD", cell, span, anchor) - (0.5, 0.5, 0.0)), (0.0, 0.0, 0.0))
        upper = vector("MINIMUM", vector("CEIL", vector("MULTIPLY_ADD", vector("ADD", cell, (1.0, 1.0, 0.0)), span, anchor) - (0.5, 0.5, 0.0)), sides)
        return lower, upper - lower

    candidates = (vector("MAXIMUM", vector("FLOOR", span), (1.0, 1.0, 0.0)), vector("CEIL", span), bounded((0.5, 0.5, 0.0))[1], bounded(sides - (0.5, 0.5, 0.0))[1])
    lower, count = bounded(vector("DIVIDE", grid.base, grid.inverse))
    uv = vector("ADD", vector("DIVIDE", vector("ADD", lower + vector("FLOOR", vector("SCALE", count, 0.5)), (0.5, 0.5, 0.0)), sides), (0.0, 0.0, 1.0))

    def averaged(x: int, y: int) -> tuple[Expr, Expr]:
        horizontal, vertical = candidates[x]["X"], candidates[y]["Y"]
        kernel = Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Color", (1.0,) * 4), ("Size", xyz(horizontal, vertical))), socket="Image", shape=bpy.types.NodeSocketColor)
        image = Expr(
            kind=bpy.types.CompositorNodeConvolve,
            inputs=(("Image", source), ("Kernel Data Type", "Float"), ("Float Kernel", kernel), ("Normalize Kernel", True)),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )
        sample = Expr(
            kind=bpy.types.CompositorNodeMapUV,
            inputs=(("Image", image), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", "Extend"), ("Extension Y", "Extend")),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )
        return sample, math("COMPARE", count["X"], horizontal, 0.0) * math("COMPARE", count["Y"], vertical, 0.0)

    (first, _), *rest = starmap(averaged, product(range(len(candidates)), repeat=2))
    return reduce(lambda held, step: where(step[1], step[0], held), rest, first)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["bilateral", "block_mean", "convolved", "ranked", "tiled"]
