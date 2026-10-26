"""Authored-domain LUT sampling and tetrahedral compositor tables."""

from collections.abc import Mapping
from pathlib import Path
from typing import Annotated

import bpy
from expression import Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Unchanged
from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderFaults, BlenderResult
from blender.bpy.nodes.graphs import Expr, Generated, grouped, math, vector, xyz
from blender.bpy.render.color.luts import current
from blender.bpy.render.color.spaces import space_item
from blender.bpy.render.effects.rows import Alpha, CameraFrame, CompositorEffect, Named, Opened, revised, Shell
from blender.imaging.color.luts import Lattice, Logarithmic, LutFormat, Rational, Uniform
from blender.imaging.color.processors import converted
from blender.imaging.effects.grade.lookup import Lookup
from blender.imaging.faults import ImagingBufferFault
from blender.imaging.hashing import content_hash
from blender.imaging.sampling import Estimate, Precision

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TABLES]


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
        image = Generated(ownership=tiles, content=content_hash(Lattice, lattice, arrays=(plane,)), name=name, plane=lambda: Ok(Some(plane)), tag=Tag(space=Some(data)))
        return tetrahedral(encoded, Expr(kind=bpy.types.CompositorNodeImage, content=Some(image), socket="Image", shape="RGBA").named(key), float(lattice.size))

    return space_item(config, PyOpenColorIO.ROLE_DATA).map(sampled)


def tiled(record: Lookup, config: PyOpenColorIO.Config) -> BlenderResult[tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]]:
    """Splits the record's current file into native prefix operations and its first sampled 3D table."""
    return current(record.source, config).bind(lambda path: msgspec.structs.replace(record, source=path).tile(config))


# --- [GRAPH]


def looked_up(shell: Shell[Lookup]) -> BlenderResult[Expr]:
    """Samples prefix on its authored domain, then suffix table."""
    record = shell.record
    name = Path(record.source).name
    source_field, _, prefix_field = Lookup.__struct_fields__

    def built(parts: tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]) -> BlenderResult[Expr]:
        head, suffix = parts
        if len(head) and not record.prefix.enabled and Shell.image not in shell.images:
            return shell.passed()
        lattice = record.prefix.value
        entering = (
            Result[npt.NDArray[np.float32], BlenderFaults].bind(
                lattice.samples().bind(lambda samples: converted(PyOpenColorIO.Config(), head, samples)),
                lambda plane: tabled(prefix_field, f"{name} Prefix", lattice, plane, shell.color, shell.config),
            )
            if len(head) and record.prefix.enabled
            else Ok(shell.color)
        )
        return entering.bind(lambda source: suffix.map(lambda table: tabled(source_field, name, *table, source, shell.config)).default_value(Ok(source)))

    return tiled(record, shell.config).bind(built)


def prepared(
    _identity: Identity,
    _scene: bpy.types.Scene,
    record: Lookup,
    config: PyOpenColorIO.Config,
    image: OpenImageIO.ImageBuf,
    _precision: Precision,
    _frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[tuple[Lookup, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
    """Applies unbounded prefix to entering pixels before suffix sampling."""

    def processed(head: PyOpenColorIO.GroupTransform) -> BlenderResult[tuple[Lookup, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
        if not len(head) or record.prefix.enabled:
            return Ok((record, {}))
        operation = PyOpenColorIO.GroupTransform([
            PyOpenColorIO.ColorSpaceTransform(src=PyOpenColorIO.ROLE_SCENE_LINEAR, dst=record.space),
            head,
            PyOpenColorIO.ColorSpaceTransform(src=record.space, dst=PyOpenColorIO.ROLE_SCENE_LINEAR),
        ])

        def published(values: npt.NDArray[np.float32]) -> BlenderResult[tuple[Lookup, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
            result = OpenImageIO.ImageBuf(OpenImageIO.ImageSpec(image.spec()))
            return Ok((record, {Shell.image: result})) if result.set_pixels(result.roi, values) else Result.Error((ImagingBufferFault(request=result, error=result.geterror()),))

        return Result[npt.NDArray[np.float32], BlenderFaults].bind(ImagingBufferFault.pixels(image).bind(lambda pixels: converted(config, operation, pixels)), published)

    return tiled(record, config).bind(lambda parts: processed(parts[0]))


# --- [RELOAD]


def reloaded(call: Call[Named]) -> BlenderResult[Outcome]:
    """Rebuilds named LUT entry from current file."""
    return (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: lookup.named(call.identity, scene, call.record.name).bind(lambda entry: revised(lookup, call.identity, scene, entry, Ok)))
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.record.name} matches current file"))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

tiles = Ownership(kind=bpy.types.Image, key="lookup", saved=Saved.USED)
reloading = Operator(key="lookup_reload", title="Reload LUT", description="Rebuild entry from LUT file", form=Named, run=reloaded)
lookup: CompositorEffect[Lookup] = CompositorEffect(
    record=Lookup,
    graph=looked_up,
    alpha=Alpha.STRAIGHT,
    preparation=Some(prepared),
    opened=Some(Opened(extensions=lambda: tuple(dict.fromkeys(f".{row.extension}" for row in LutFormat.readable())), of=lambda path: Lookup(source=path))),
    registrables=(tiles, reloading),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["looked_up", "lookup", "prepared", "reloaded", "reloading", "tabled", "tetrahedral", "tiled", "tiles"]
