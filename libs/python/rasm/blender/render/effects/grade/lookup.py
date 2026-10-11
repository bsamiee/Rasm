"""Authored-domain native LUT sampling and tetrahedral compositor tables."""

from collections.abc import Mapping
import functools
from pathlib import Path

import bpy
from expression import Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from rasm.blender.commands.operators import Call, Done, Operator
from rasm.blender.data.ids.images import Tag
from rasm.blender.data.ids.ownership import Ownership, Saved
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.faults import BlenderFaults, BlenderImagingFault, BlenderResult
from rasm.blender.nodes.graphs import End, ended, Fragment, Generated, INPUTS, Interface, item_key, math, nested, node, NodeGroup, Operand, Socket, vector
from rasm.blender.render.color.luts import lut_files
from rasm.blender.render.color.spaces import space_item
from rasm.blender.render.effects.stack import CameraFrame, CompositorEffect, Named, Opened, Shell
from rasm.imaging.color.luts import Lattice, Logarithmic, LutFormat, Rational, Uniform
from rasm.imaging.color.processors import applied, processor
from rasm.imaging.effects.generators.noise import Estimate, Precision
from rasm.imaging.effects.grade.lookup import Lookup
from rasm.imaging.faults import ImagingBufferFault, paired
from rasm.imaging.hashing import content_hash

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GRAPH]


@functools.cache
def tetrahedral() -> BlenderResult[NodeGroup]:
    """Builds the shared interpolation group over a table's native cube size.

    Returns:
        The group carrying ordered tetrahedral vertex samples, or every native
        node construction failure.
    """
    encoded, tile, size = (End(node=INPUTS, socket=key) for key in ("encoded", "tile", "size"))
    (
        limit,
        order_a,
        order_b,
        order_c,
        not_a,
        not_b,
        not_c,
        largest_x,
        largest_y,
        largest_xy,
        largest_z,
        smallest_x,
        smallest_y,
        smallest_xy,
        smallest_z,
        upper,
        middle,
        reciprocal,
        square,
        centre,
        descent,
        top,
    ) = (
        End(node=key)
        for key in (
            "limit",
            "order_a",
            "order_b",
            "order_c",
            "not_a",
            "not_b",
            "not_c",
            "largest_x",
            "largest_y",
            "largest_xy",
            "largest_z",
            "smallest_x",
            "smallest_y",
            "smallest_xy",
            "smallest_z",
            "upper",
            "middle",
            "reciprocal",
            "square",
            "centre",
            "descent",
            "top",
        )
    )
    limits, scaled, raised, index, base, fraction, largest, smallest, step_one, opposite, step_two = (
        End(node=key, socket="Vector") for key in ("limits", "scaled", "raised", "index", "base", "fraction", "largest", "smallest", "step_1", "opposite", "step_2")
    )
    high, low, total = (End(node=key) for key in ("high", "low", "total"))
    red, green, blue = (End(node="fractions", socket=axis) for axis in ("X", "Y", "Z"))
    weights = (*(End(node=f"weight_{vertex}") for vertex in range(3)), low)
    vertices = (base, *(End(node=f"vertex_{vertex}", socket="Vector") for vertex in range(1, 4)))
    terms = tuple(End(node=f"term_{vertex}", socket="Vector") for vertex in range(4))
    address: tuple[tuple[Operand, ...], ...] = ((square, reciprocal, 0.0, centre), (0.0, 0.0, descent, top), (0.0, 0.0, 0.0, 1.0), (0.0, 0.0, 0.0, 1.0))
    body = Fragment.joined(
        math(limit.node, "SUBTRACT", size, 1.0),
        node(limits.node, bpy.types.ShaderNodeCombineXYZ, dict.fromkeys(("X", "Y", "Z"), limit), {}),
        vector(scaled.node, "MULTIPLY", encoded, limits),
        vector(raised.node, "MAXIMUM", (0.0, 0.0, 0.0), scaled),
        vector(index.node, "MINIMUM", raised, limits),
        vector(base.node, "FLOOR", index),
        vector(fraction.node, "SUBTRACT", index, base),
        node("fractions", bpy.types.ShaderNodeSeparateXYZ, {"Vector": fraction}, {}),
        math(order_a.node, "GREATER_THAN", red, green),
        math(order_b.node, "GREATER_THAN", green, blue),
        math(order_c.node, "GREATER_THAN", red, blue),
        math(not_a.node, "SUBTRACT", 1.0, order_a),
        math(not_b.node, "SUBTRACT", 1.0, order_b),
        math(not_c.node, "SUBTRACT", 1.0, order_c),
        math(largest_x.node, "MULTIPLY", order_a, order_c),
        math(largest_y.node, "MULTIPLY", not_a, order_b),
        math(largest_xy.node, "ADD", largest_x, largest_y),
        math(largest_z.node, "SUBTRACT", 1.0, largest_xy),
        math(smallest_x.node, "MULTIPLY", not_a, not_c),
        math(smallest_y.node, "MULTIPLY", order_a, not_b),
        math(smallest_xy.node, "ADD", smallest_x, smallest_y),
        math(smallest_z.node, "SUBTRACT", 1.0, smallest_xy),
        node(largest.node, bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y", "Z"), (largest_x, largest_y, largest_z))), {}),
        node(smallest.node, bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y", "Z"), (smallest_x, smallest_y, smallest_z))), {}),
        vector(high.node, "DOT_PRODUCT", fraction, largest),
        vector(low.node, "DOT_PRODUCT", fraction, smallest),
        vector(total.node, "DOT_PRODUCT", fraction, (1.0, 1.0, 1.0)),
        math(upper.node, "SUBTRACT", total, high),
        math(middle.node, "SUBTRACT", upper, low),
        math(weights[0].node, "SUBTRACT", 1.0, high),
        math(weights[1].node, "SUBTRACT", high, middle),
        math(weights[2].node, "SUBTRACT", middle, low),
        vector(step_one.node, "ADD", base, largest),
        vector(opposite.node, "ADD", base, (1.0, 1.0, 1.0)),
        vector(step_two.node, "SUBTRACT", opposite, smallest),
        *(vector(vertex.node, "MINIMUM", step, limits) for vertex, step in zip(vertices[1:], (step_one, step_two, opposite))),
        math(reciprocal.node, "DIVIDE", 1.0, size),
        math(square.node, "MULTIPLY", reciprocal, reciprocal),
        math(centre.node, "MULTIPLY", square, 0.5),
        math(descent.node, "MULTIPLY", reciprocal, -1.0),
        math(top.node, "MULTIPLY_ADD", reciprocal, -0.5, 1.0),
        node("addressing", bpy.types.FunctionNodeCombineMatrix, {f"Column {column} Row {row}": cell for row, cells in enumerate(address, 1) for column, cell in enumerate(cells, 1)}, {}),
        *(node(f"place_{vertex}", bpy.types.FunctionNodeTransformPoint, {"Vector": value, "Transform": End(node="addressing", socket="Matrix")}, {}) for vertex, value in enumerate(vertices)),
        *(
            node(
                f"sample_{vertex}",
                bpy.types.CompositorNodeMapUV,
                {"Image": tile, "UV": End(node=f"place_{vertex}", socket="Vector"), "Interpolation": "Nearest", "Extension X": "Extend", "Extension Y": "Extend"},
                {},
            )
            for vertex in range(4)
        ),
        *(vector(term.node, "SCALE", End(node=f"sample_{vertex}", socket="Image"), weight) for vertex, (term, weight) in enumerate(zip(terms, weights))),
        vector("sum_1", "ADD", *terms[:2]),
        vector("sum_2", "ADD", End(node="sum_1", socket="Vector"), terms[2]),
        vector("color", "ADD", End(node="sum_2", socket="Vector"), terms[3]),
    ).map(ended("color", "Vector"))
    face = Interface(
        outputs=(Socket(key="color", name="Color", kind=bpy.types.NodeSocketColor),),
        inputs=(
            Socket(key=encoded.socket, name="Encoded", kind=bpy.types.NodeSocketVector),
            Socket(key=tile.socket, name="Tile", kind=bpy.types.NodeSocketColor),
            Socket(key=size.socket, name="Size", kind=bpy.types.NodeSocketFloat),
        ),
    )
    return body.bind(lambda built: NodeGroup.built("tetrahedral", "Tetrahedral Lookup", face, {"color": built}))


# --- [TABLES]


def tabled(key: Key, name: str, lattice: Lattice, plane: npt.NDArray[np.float32], source: Operand, config: PyOpenColorIO.Config) -> BlenderResult[tuple[Fragment, End]]:
    """Samples packed outputs at coordinates encoded by an authored lattice.

    Args:
        key: Native node prefix and image address.
        name: Generated image name.
        lattice: Table size and input encoding.
        plane: Top-first RGB table outputs in blue, green, red node order.
        source: Straight RGB entering the table.
        config: Configuration supplying the data image role.

    Returns:
        Tetrahedral table output, or native image and graph construction faults.
    """

    def encoded() -> BlenderResult[tuple[Fragment, End]]:
        match lattice.spacing:
            case Uniform(low=low, high=high):
                scale = max(abs(low), abs(high))
                return (
                    vector(f"{key}_scale", "DIVIDE", source, (scale,) * 3)
                    .bind(lambda scaled: vector(f"{key}_offset", "SUBTRACT", scaled, (low / scale,) * 3))
                    .bind(lambda offset: vector(f"{key}_encoded", "DIVIDE", offset, ((high / scale - low / scale),) * 3))
                )
            case Logarithmic(low=low, span=span):
                channels = node(f"{key}_channels", bpy.types.ShaderNodeSeparateXYZ, {"Vector": source}, {})
                return Fragment.joined(
                    channels,
                    *(
                        part
                        for axis in "XYZ"
                        for part in (
                            math(f"{key}_{axis.lower()}_offset", "ADD", End(node=f"{key}_channels", socket=axis), 2.0**low),
                            math(f"{key}_{axis.lower()}_positive", "MAXIMUM", End(node=f"{key}_{axis.lower()}_offset"), float(np.finfo(np.float32).tiny)),
                            math(f"{key}_{axis.lower()}_log", "LOGARITHM", End(node=f"{key}_{axis.lower()}_positive"), 2.0),
                            math(f"{key}_{axis.lower()}_fit", "MULTIPLY_ADD", End(node=f"{key}_{axis.lower()}_log"), 1.0 / span, -low / span),
                        )
                    ),
                    node(f"{key}_encoded", bpy.types.ShaderNodeCombineXYZ, {axis: End(node=f"{key}_{axis.lower()}_fit") for axis in "XYZ"}, {}),
                ).map(ended(f"{key}_encoded", "Vector"))
            case Rational():
                return vector(f"{key}_positive", "MAXIMUM", source, (0.0,) * 3).bind(
                    lambda positive: vector(f"{key}_denominator", "ADD", positive, (1.0,) * 3).bind(lambda denominator: vector(f"{key}_encoded", "DIVIDE", positive, denominator))
                )

    def sampled(parts: tuple[NodeGroup, str, tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
        group, data, coordinates = parts
        image = Generated(ownership=tiles, content=content_hash(Lattice, lattice, arrays=(plane,)), name=name, plane=lambda: Ok(Some(plane)), tag=Tag(space=Some(data)))
        return node(key, bpy.types.CompositorNodeImage, {}, {}, Some(image)).bind(
            lambda tile: nested(f"{key}_lookup", group, {"encoded": coordinates, "tile": ended(key, "Image")(tile), "size": float(lattice.size)}, "color")
        )

    return paired(paired(tetrahedral(), space_item(config, PyOpenColorIO.ROLE_DATA)), encoded()).map(lambda pair: (*pair[0], pair[1])).bind(sampled)


def looked_up(shell: Shell[Lookup]) -> BlenderResult[tuple[Fragment, End]]:
    """Samples every native prefix operation on its authored domain, then its suffix."""
    record = shell.record
    name = Path(record.source).name

    def built(parts: tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]) -> BlenderResult[tuple[Fragment, End]]:
        head, suffix = parts
        if len(head) and not record.prefix.enabled and Shell.image not in shell.images:
            return Ok((Fragment(), shell.source))

        def prefix(lattice: Lattice) -> BlenderResult[tuple[Fragment, End]]:
            sampled: BlenderResult[npt.NDArray[np.float32]] = (
                paired(processor(PyOpenColorIO.Config(), head), lattice.samples())
                .map(lambda pair: applied(pair[0].getDefaultCPUProcessor(), pair[1]))
                .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
            )
            return Result[npt.NDArray[np.float32], BlenderFaults].bind(sampled, lambda plane: tabled(item_key(Lookup.prefix.__name__), f"{name} Prefix", lattice, plane, shell.source, shell.config))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

        entering = prefix(record.prefix.value) if len(head) and record.prefix.enabled else Ok((Fragment(), shell.source))
        return entering.bind(lambda source: suffix.map(lambda table: tabled(item_key(Lookup.source.__name__), name, *table, source, shell.config)).default_value(Ok(source)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    return (
        lut_files
        .current(record.source, shell.config)
        .bind(lambda path: msgspec.structs.replace(record, source=path).tile(shell.config).map_error(lambda faults: (BlenderImagingFault(faults=faults),)))
        .bind(built)
    )


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
    """Applies an unbounded native prefix to actual entering pixels before suffix sampling."""

    def processed(head: PyOpenColorIO.GroupTransform) -> BlenderResult[tuple[Lookup, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
        if not len(head) or record.prefix.enabled:
            return Ok((record, {}))
        operation = PyOpenColorIO.GroupTransform([
            PyOpenColorIO.ColorSpaceTransform(src=PyOpenColorIO.ROLE_SCENE_LINEAR, dst=record.space),
            head,
            PyOpenColorIO.ColorSpaceTransform(src=record.space, dst=PyOpenColorIO.ROLE_SCENE_LINEAR),
        ])
        pixels = Option.of_optional(image.get_pixels(OpenImageIO.FLOAT)).to_result_with(lambda: (BlenderImagingFault(faults=(ImagingBufferFault(request=image, error=image.geterror()),)),))

        def published(values: npt.NDArray[np.float32]) -> BlenderResult[tuple[Lookup, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
            result = OpenImageIO.ImageBuf(OpenImageIO.ImageSpec(image.spec()))
            return (
                Ok((record, {Shell.image: result}))
                if result.set_pixels(result.roi, values)
                else Result.Error((BlenderImagingFault(faults=(ImagingBufferFault(request=result, error=result.geterror()),)),))
            )

        return Result[tuple[PyOpenColorIO.Processor, npt.NDArray[np.float32]], BlenderFaults].bind(
            paired(processor(config, operation).map_error(lambda faults: (BlenderImagingFault(faults=faults),)), pixels), lambda pair: published(applied(pair[0].getDefaultCPUProcessor(), pair[1]))
        )

    return (
        lut_files
        .current(record.source, config)
        .bind(lambda path: msgspec.structs.replace(record, source=path).tile(config).map_error(lambda faults: (BlenderImagingFault(faults=faults),)))
        .bind(lambda parts: processed(parts[0]))
    )


# --- [RELOAD]


def reloaded(call: Call[Named]) -> BlenderResult[Done]:
    """Rebuilds a named LUT entry from current file content."""
    return (
        call
        .root(bpy.types.Scene)
        .bind(
            lambda scene: lookup.named(call.identity, scene, call.record.name).bind(lambda entry: lookup.read(call.identity, entry).bind(lambda held: lookup.write(call.identity, scene, entry, held)))
        )
        .map(lambda _: Done())
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

tiles = Ownership(kind=bpy.types.Image, key="lookup", saved=Saved.USED)
reloading = Operator(key="lookup_reload", title="Reload LUT", description="Rebuilds the entry from its current LUT file", form=Named, run=reloaded)
lookup: CompositorEffect[Lookup] = CompositorEffect(
    record=Lookup,
    graph=looked_up,
    straight=True,
    preparation=Some(prepared),
    opened=Some(Opened(extensions=lambda: tuple(dict.fromkeys(f".{row.extension}" for row in LutFormat.of(PyOpenColorIO.FileTransform.getFormats()))), of=lambda path: Lookup(source=path))),
    registrables=(tiles, reloading),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["looked_up", "lookup", "reloaded", "reloading", "tabled", "tetrahedral", "tiles"]
