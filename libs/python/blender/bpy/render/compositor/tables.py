"""Compositor tables of OCIO transforms with no node form: lattice-sampled tetrahedral tables and curve tables sampled through a transform's processor."""

from collections.abc import Mapping
from typing import Annotated, Final

import bpy
from expression import Ok, Option, Result, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderFaults, BlenderResult
from blender.bpy.nodes.graphs import Expr, Generated, grouped, math, Points, vector, xyz
from blender.bpy.render.compositor.colors import space_item
from blender.imaging.color.luts import Lattice, Logarithmic, Rational, split, Uniform
from blender.imaging.color.processors import converted, processor
from blender.imaging.files.images import applied
from blender.records.identity import content_hash

# --- [CONSTANTS] ------------------------------------------------------------------------

TILES: Final = Ownership(kind=bpy.types.Image, key="table", saved=Saved.USED)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LATTICES]


@grouped("tetrahedral", "Tetrahedral Lookup")
def tetrahedral(encoded: Annotated[Expr, bpy.types.NodeSocketVector], tile: Annotated[Expr, bpy.types.NodeSocketColor], size: Expr | float) -> Mapping[Key, Expr]:
    """Tetrahedral interpolation of a packed table tile at encoded coordinates over the table's cube size."""
    limit = size - 1.0
    limits = xyz(limit, limit, limit)
    index = vector("MINIMUM", vector("MAXIMUM", (0.0, 0.0, 0.0), encoded * limits), limits)
    base = vector("FLOOR", index)
    fraction = index - base
    red, green, blue = fraction["X"], fraction["Y"], fraction["Z"]
    ordered = (red > green, green > blue, red > blue)
    unordered = tuple(1.0 - order for order in ordered)
    largest = (ordered[0] * ordered[2], unordered[0] * ordered[1])
    smallest = (unordered[0] * unordered[2], ordered[0] * unordered[1])
    toward, away = xyz(*largest, 1.0 - (largest[0] + largest[1])), xyz(*smallest, 1.0 - (smallest[0] + smallest[1]))
    high, low = vector("DOT_PRODUCT", fraction, toward), vector("DOT_PRODUCT", fraction, away)
    middle = vector("DOT_PRODUCT", fraction, (1.0, 1.0, 1.0)) - high - low
    opposite = vector("ADD", base, (1.0, 1.0, 1.0))
    reciprocal = 1.0 / size
    square = reciprocal * reciprocal
    cells: tuple[tuple[Expr | float, ...], ...] = (
        (square, reciprocal, 0.0, square * 0.5),
        (0.0, 0.0, -reciprocal, math("MULTIPLY_ADD", reciprocal, -0.5, 1.0)),
        (0.0, 0.0, 0.0, 1.0),
        (0.0, 0.0, 0.0, 1.0),
    )
    addressing = Expr(
        kind=bpy.types.FunctionNodeCombineMatrix,
        inputs=tuple((f"Column {column} Row {row}", cell) for row, row_cells in enumerate(cells, 1) for column, cell in enumerate(row_cells, 1)),
        socket="Matrix",
    )
    vertices = (base, vector("MINIMUM", base + toward, limits), vector("MINIMUM", opposite - away, limits), vector("MINIMUM", opposite, limits))
    terms = tuple(
        vector(
            "SCALE",
            Expr(
                kind=bpy.types.CompositorNodeMapUV,
                inputs=(
                    ("Image", tile),
                    ("UV", Expr(kind=bpy.types.FunctionNodeTransformPoint, inputs=(("Vector", vertex), ("Transform", addressing)), socket="Vector", shape="VECTOR")),
                    ("Interpolation", "Nearest"),
                    ("Extension X", "Extend"),
                    ("Extension Y", "Extend"),
                ),
                socket="Image",
                shape="RGBA",
            ),
            weight,
        )
        for vertex, weight in zip(vertices, (1.0 - high, high - middle, middle - low, low))
    )
    return {"color": sum(terms[1:], terms[0])}


def tabled(key: Key, name: str, lattice: Lattice, plane: npt.NDArray[np.float32], source: Expr, config: PyOpenColorIO.Config) -> BlenderResult[Expr]:
    """Samples packed outputs at lattice-encoded coordinates.

    Args:
        key: Table image node key, stable while the table content changes.
        name: Generated image name.
        lattice: Table size and input encoding.
        plane: Top-first RGB table outputs in blue, green, red node order.
        source: Straight RGB input.
        config: Configuration with data image role.

    Returns:
        Tetrahedral table output, or the data role fault.
    """
    match lattice.spacing:
        case Uniform(low=low, high=high):
            scale = max(abs(low), abs(high))
            encoded = vector("DIVIDE", vector("SUBTRACT", vector("DIVIDE", source, (scale,) * 3), (low / scale,) * 3), (high / scale - low / scale,) * 3)
        case Logarithmic(low=low, span=span):
            tiny = float(np.finfo(np.float32).tiny)
            encoded = xyz(*(math("MULTIPLY_ADD", math("LOGARITHM", math("MAXIMUM", source[axis] + 2.0**low, tiny), 2.0), 1.0 / span, -low / span) for axis in ("X", "Y", "Z")))
        case Rational():
            positive = vector("MAXIMUM", source, (0.0,) * 3)
            encoded = positive / (positive + (1.0,) * 3)

    def sampled(data: str) -> Expr:
        image = Generated(ownership=TILES, content=content_hash(Lattice, lattice, arrays=(plane,)), name=name, plane=lambda: Ok(Some(plane)), tag=Tag(space=Some(data)))
        return tetrahedral(encoded, Expr(kind=bpy.types.CompositorNodeImage, content=Some(image), socket="Image", shape="RGBA").named(key), float(lattice.size))

    return space_item(config, PyOpenColorIO.ROLE_DATA).map(sampled)


def latticed(key: Key, name: str, config: PyOpenColorIO.Config, transform: PyOpenColorIO.Transform, lattice: Lattice, source: Expr) -> BlenderResult[Expr]:
    """Samples a transform with no node form at the lattice's nodes into a tetrahedral table of the straight RGB source."""
    return Result[npt.NDArray[np.float32], BlenderFaults].bind(
        lattice.samples().bind(lambda samples: converted(config, transform, samples)), lambda plane: tabled(key, name, lattice, plane, source, config)
    )


def suffixed(key: Key, name: str, config: PyOpenColorIO.Config, transform: PyOpenColorIO.Transform, source: Expr) -> BlenderResult[tuple[PyOpenColorIO.GroupTransform, Expr]]:
    """Tables a transform from its first 3D table on at that table's own lattice, answering the operations before it and the table output, the source where it holds none."""
    return Result[tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]], BlenderFaults].bind(
        split(config, transform), lambda parts: parts[1].map(lambda found: tabled(key, name, *found, source, config)).default_value(Ok(source)).map(lambda table: (parts[0], table))
    )


# --- [CURVES]


def curved(config: PyOpenColorIO.Config, transform: PyOpenColorIO.Transform, domain: tuple[float, float], count: int = 257) -> BlenderResult[tuple[Points, Points, Points]]:
    """Samples a channel-separable transform's unoptimized processor on a grey ramp over `domain` into one curve table per RGB channel, a drawn-only grade the optimizer reads as identity kept.

    Args:
        config: Configuration resolving the transform.
        transform: Transform mapping each channel by its own curve, as `GradingRGBCurveTransform` does.
        domain: Lowest and highest input of the tables.
        count: Points per table.

    Returns:
        Red, green, and blue curve points, or the transform's color fault.
    """
    ramp = np.linspace(*domain, count, dtype=np.float32)

    def table(plane: npt.NDArray[np.float32], channel: int) -> Points:
        return tuple(zip(ramp.astype(float).tolist(), plane[0, :, channel].astype(float).tolist()))

    return (
        processor(config, transform)
        .map(lambda built: applied(built.getOptimizedCPUProcessor(PyOpenColorIO.OPTIMIZATION_NONE), np.repeat(ramp[None, :, None], 3, axis=2)))
        .map(lambda plane: (table(plane, 0), table(plane, 1), table(plane, 2)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TILES", "curved", "latticed", "suffixed", "tabled", "tetrahedral"]
