"""Stock node evaluation for code: expressions over named per-point columns evaluated by Blender's own nodes in one scratch geometry pass."""

from collections.abc import Mapping
from contextlib import ExitStack
from functools import partial, reduce
from itertools import starmap
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

import bpy
from expression import Error, Ok, Option, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.desired import RESULT
from blender.bpy.nodes.functions import built
from blender.bpy.nodes.graphs import Expr, group_input, Shape
from blender.bpy.nodes.interface import Interface, Socket
from blender.records.faults import accumulated, Fault, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Values = (
    bpy.types.bpy_prop_collection[bpy.types.FloatAttributeValue]
    | bpy.types.bpy_prop_collection[bpy.types.IntAttributeValue]
    | bpy.types.bpy_prop_collection[bpy.types.BoolAttributeValue]
    | bpy.types.bpy_prop_collection[bpy.types.FloatVectorAttributeValue]
    | bpy.types.bpy_prop_collection[bpy.types.FloatColorAttributeValue]
)

# --- [CONSTANTS] ------------------------------------------------------------------------

_STORED: Final[Mapping[Shape, tuple["rna_enums.AttributeTypeItems", str, int]]] = MappingProxyType({
    bpy.types.NodeSocketFloat: ("FLOAT", "value", 1),
    bpy.types.NodeSocketInt: ("INT", "value", 1),
    bpy.types.NodeSocketBool: ("BOOLEAN", "value", 1),
    bpy.types.NodeSocketVector: ("FLOAT_VECTOR", "vector", 3),
    bpy.types.NodeSocketColor: ("FLOAT_COLOR", "color", 4),
})
_COLUMNS: Final[Mapping[int, Shape]] = MappingProxyType({1: bpy.types.NodeSocketFloat, 3: bpy.types.NodeSocketVector, 4: bpy.types.NodeSocketColor})
_GEOMETRY: Final = Socket(key="geometry", kind=bpy.types.NodeSocketGeometry)

# --- [OPERATIONS] -----------------------------------------------------------------------


def column(name: str, shape: Shape = bpy.types.NodeSocketFloat) -> Expr:
    """Reads one named per-point column `sampled` writes, as the socket class its width gives."""
    return Expr(kind=bpy.types.GeometryNodeInputNamedAttribute, inputs=(("Name", name),), members={"data_type": _STORED[shape][0]}, socket="Attribute", shape=shape)


def sampled(identity: Identity, columns: Mapping[str, npt.ArrayLike], *roots: Expr) -> Outcome[tuple[npt.NDArray[np.float32], ...]]:
    """Evaluates expressions over named per-point columns through the stock nodes, in one geometry pass over a scratch point cloud.

    Columns of one value, three, or four per point become float, vector, or color attributes `column` reads, a `position` column placing
    the points. The scratch tree takes the expressions through the reconciler, so ramps, curves, images, menus, and groups evaluate, and
    every scratch data-block is removed on every path.

    Args:
        identity: Extension owning the scratch tree's marks.
        columns: Per-point values by attribute name, every column one row per point.
        *roots: Expressions to evaluate.

    Returns:
        Values per root, `(n,)` for single values and `(n, 3)` or `(n, 4)` for vectors and colors. `Error` of refused nodes, links, or writes.
    """
    held = {name: np.ascontiguousarray(values, dtype=np.float32) for name, values in columns.items()}
    count = len(next(iter(held.values()), np.zeros(0)))
    names = tuple(f".sampled_{index}" for index in range(len(roots)))
    stored = reduce(
        lambda geometry, pair: Expr(
            kind=bpy.types.GeometryNodeStoreNamedAttribute,
            inputs=(("Geometry", geometry), ("Name", pair[0]), ("Value", pair[1])),
            members={"data_type": _STORED[pair[1].shape][0], "domain": "POINT"},
            socket="Geometry",
            shape=bpy.types.NodeSocketGeometry,
        ),
        zip(names, roots),
        group_input(_GEOMETRY.key, bpy.types.NodeSocketGeometry),
    )

    def data(attribute: bpy.types.Attribute) -> Outcome[_Values]:
        match attribute:
            case bpy.types.FloatAttribute() | bpy.types.IntAttribute() | bpy.types.BoolAttribute() | bpy.types.FloatVectorAttribute() | bpy.types.FloatColorAttribute():
                return Ok(attribute.data)
            case _:
                return Error(Fault.of(BlenderReason.ABSENT, repr(attribute), member="data"))

    def evaluated(scratch: ExitStack) -> Outcome[tuple[npt.NDArray[np.float32], ...]]:
        scene = bpy.data.scenes.new(".sampled")
        cloud = bpy.data.pointclouds.new(".sampled")
        holder = bpy.data.objects.new(".sampled", cloud)
        made = bpy.data.node_groups.new(".sampled", "GeometryNodeTree")
        scratch.callback(bpy.data.batch_remove, ids=tuple(each for each in (holder, cloud, made, scene) if each is not None))
        scene.collection.objects.link(holder)
        cloud.resize(count)
        attributes = cloud.attributes

        def attributed(name: str, values: npt.NDArray[np.float32]) -> Outcome[None]:
            kind, member, _ = _STORED[_COLUMNS[1 if values.ndim == 1 else values.shape[1]]]
            attribute = attributes[name] if name in attributes else attributes.new(name, kind, "POINT")
            return data(attribute).bind(lambda found: catch(found.foreach_set, exception=TypeError)(member, values.ravel()).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(attribute))))

        def column_read(points: bpy.types.PointCloud, name: str, shape: Shape) -> Outcome[npt.NDArray[np.float32]]:
            _, member, width = _STORED[shape]
            values = np.empty(count * width, dtype=np.float32)
            return (
                Block
                .of_seq(points.attributes)
                .filter(lambda held: held.name == name)
                .try_head()
                .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(points.attributes), member=name))
                .bind(data)
                .bind(lambda found: catch(found.foreach_get, exception=TypeError)(member, values).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(found))))
                .map(lambda _: values if width == 1 else values.reshape(count, width))
            )

        def read(_: object) -> Outcome[tuple[npt.NDArray[np.float32], ...]]:
            modifier: bpy.types.NodesModifier = holder.modifiers.new("Field", "NODES")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            modifier.node_group = made
            layer: bpy.types.ViewLayer = scene.view_layers[0]
            layer.update()
            depsgraph: bpy.types.Depsgraph = layer.depsgraph  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return (
                catch(holder.evaluated_get(depsgraph).evaluated_geometry, exception=TypeError)()
                .map_error(partial(Fault.of, BlenderReason.REFUSED, repr(holder)))
                .bind(
                    lambda geometry: (
                        Option
                        .of_optional(try_downcast(bpy.types.PointCloud, geometry.pointcloud))
                        .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(geometry), member="pointcloud"))
                        .bind(lambda evaluated_cloud: accumulated(column_read(evaluated_cloud, name, root.shape) for name, root in zip(names, roots)))
                    )
                )
            )

        face = Interface(inputs=(_GEOMETRY,), outputs=(msgspec.structs.replace(_GEOMETRY, key=RESULT),))
        tree = Option.of_optional(made).to_result_with(lambda: Fault.of(BlenderReason.REFUSED, repr(bpy.data.node_groups), member=".sampled"))
        return (
            accumulated(starmap(attributed, held.items()))
            .bind(lambda _: paired(tree, built(".sampled", ".sampled", face, {RESULT: stored}, bpy.types.GeometryNodeTree)))
            .bind(lambda pair: reconciled(pair[1], pair[0], identity))
            .bind(read)
        )

    with ExitStack() as scratch:
        return evaluated(scratch)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["column", "sampled"]
