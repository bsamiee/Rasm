"""Functions over graph expressions: lowering them into desired graphs, groups built around them, bodies declared as groups, the Array API namespace of elementwise functions, and stock-node evaluation over per-point columns."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from functools import partial, reduce
import inspect
from itertools import starmap
from math import e
from types import MappingProxyType
import typing
from typing import Final, TYPE_CHECKING

import bpy
from expression import Nothing, Option, Result, Some, try_downcast
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.changes import Nested, reconciled
from blender.bpy.nodes.desired import End, Graph, INPUTS, Interface, item_key, Link, member_paths, Node, NodeGroup, OUTPUTS, RESULT, Socket
from blender.bpy.nodes.graphs import Expr, group_input, math, menu, Scalar, Shape, Term, vector, where, widest, xyz
from blender.bpy.nodes.trees import GraphTree, native
from blender.records.faults import accumulated, Fault, Outcome, Reason
from blender.records.stores import content_hash

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type ColorExpr = typing.Annotated[Expr, bpy.types.NodeSocketColor]
type VectorExpr = typing.Annotated[Expr, bpy.types.NodeSocketVector]
type _Values = (
    bpy.types.bpy_prop_collection[bpy.types.FloatAttributeValue]
    | bpy.types.bpy_prop_collection[bpy.types.IntAttributeValue]
    | bpy.types.bpy_prop_collection[bpy.types.BoolAttributeValue]
    | bpy.types.bpy_prop_collection[bpy.types.FloatVectorAttributeValue]
    | bpy.types.bpy_prop_collection[bpy.types.FloatColorAttributeValue]
)


class Body[**P](typing.Protocol):
    """Named function `grouped` declares as a group, read by its name and declaring module."""

    __name__: str
    __module__: str

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> Scalar | Mapping[Key, Expr]:
        """Builds the group outputs over its input expressions."""


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

# --- [MODELS] ---------------------------------------------------------------------------


class Group[**P](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Owned group `grouped` declares from one body, placed by calling it like the body."""

    body: Body[P]
    group: NodeGroup

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> Expr:
        """Places the group node over arguments bound like the body, read at its first output."""
        return self.output(self.group.interface.outputs[0].key, *args, **kwargs)

    def output(self, key: Key, /, *args: P.args, **kwargs: P.kwargs) -> Expr:
        """Places the group node over arguments bound like the body, read at the output of `key`."""
        bound = inspect.signature(self.body).bind(*args, **kwargs)
        bound.apply_defaults()
        return placed(self.group, bound.arguments, key)


class ExpressionNamespace:
    """Elementwise functions over expressions under their Array API names, `EXPRESSIONS` the namespace a map written over numpy takes."""

    sqrt = staticmethod(partial(math, "SQRT"))
    sin = staticmethod(partial(math, "SINE"))
    cos = staticmethod(partial(math, "COSINE"))
    asin = staticmethod(partial(math, "ARCSINE"))
    atan2 = staticmethod(partial(math, "ARCTAN2"))
    floor = staticmethod(partial(math, "FLOOR"))
    minimum = staticmethod(partial(math, "MINIMUM"))
    maximum = staticmethod(partial(math, "MAXIMUM"))
    logical_or = staticmethod(partial(math, "MAXIMUM"))
    less = staticmethod(partial(math, "LESS_THAN"))
    greater = staticmethod(partial(math, "GREATER_THAN"))
    logical_and = staticmethod(partial(math, "MULTIPLY"))
    where = staticmethod(where)

    @staticmethod
    def negative(x: Expr, /) -> Expr:
        """Negation in the operand's shape."""
        return -x

    @staticmethod
    def abs(x: Expr, /) -> Expr:
        """Magnitude of every component."""
        return abs(x)

    @staticmethod
    def hypot(x1: Expr, x2: Expr, /) -> Expr:
        """Length of the vector of `x1` and `x2`."""
        return vector("LENGTH", xyz(x1, x2))

    @staticmethod
    def log(x: Expr, /) -> Expr:
        """Natural logarithm."""
        return math("LOGARITHM", x, e)

    @staticmethod
    def less_equal(x1: Expr, x2: Expr | float, /) -> Expr:
        """1 where `x1` lies at or below `x2`."""
        return 1.0 - (x1 > x2)

    @staticmethod
    def greater_equal(x1: Expr, x2: Expr | float, /) -> Expr:
        """1 where `x1` lies at or above `x2`."""
        return 1.0 - (x1 < x2)


EXPRESSIONS: Final = ExpressionNamespace()

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOWERING]


def lowered(tree: type[GraphTree], *roots: Expr) -> Outcome[Graph]:
    """Lowers root expressions and every node they read into the desired contents of a `tree` tree, each node class the tree's own.

    Each expression lowers once per call, with the expressions each keeps, keyed by label, else by `n` and a content digest of kind,
    members, input keys or literals, and content; member resolution faults, refused node classes, and differing nodes under one key accumulate.

    Returns:
        `Ok` of graph holding the roots and every node they read. `Error` of member resolution faults and key collisions.
    """
    done: dict[Expr, None] = {}
    pending = [(root, False) for root in reversed(roots)]
    while pending:
        held, expanded = pending.pop()
        children = tuple(term for term in (*(term for _, term in held.inputs), *held.kept) if isinstance(term, Expr) and term not in done)
        if held not in done and (expanded or not children):
            done[held] = None
        elif held not in done:
            pending.extend(((held, True), *((child, False) for child in reversed(children))))
    keys: dict[Expr, Key] = {}

    def entry(term: Term) -> tuple[object, ...]:
        match term:
            case Expr():
                return ("key", keys[term], term.socket)
            case End(node=node, socket=socket):
                return ("end", node, socket)
            case _:
                return ("value", term)

    def digest(expr: Expr) -> Key:
        payload = (expr.kind.__name__, tuple(expr.members.items()), tuple((address, entry(term)) for address, term in expr.inputs), expr.content.map(lambda content: content.keyed).to_optional())
        return f"n{content_hash(Expr, msgspec.msgpack.encode(payload, order='deterministic')).hex()}"

    for expr in done:
        keys[expr] = expr.label.default_with(partial(digest, expr))

    def claimed(expr: Expr) -> Outcome[Graph]:
        literals = {address: term for address, term in expr.inputs if not isinstance(term, Expr | End)}
        return native(tree, expr.kind).bind(
            lambda kind: accumulated(Descriptor.resolve(kind, path, bpy.types.Property) for path in member_paths(expr.members)).map(
                lambda _: Graph(kind=tree, nodes={keys[expr]: Node(kind=kind, members=expr.members, inputs=literals, content=expr.content)})
            )
        )

    links = tuple(
        Link(source=End(node=keys[term], socket=term.socket) if isinstance(term, Expr) else term, target=End(node=keys[expr], socket=address))
        for expr in done
        for address, term in expr.inputs
        if isinstance(term, Expr | End)
    )
    return accumulated(map(claimed, done)).bind(lambda claims: Graph.merged(tree, (*claims, Graph(kind=tree, links=links))))


# --- [GROUPS]


def built(key: Key, interface: Interface, outputs: Mapping[Key, Term], kind: type[GraphTree], /, *, name: Option[str] = Nothing) -> Outcome[NodeGroup]:
    """Builds group around output terms, adding Group Input node for interface inputs or panels.

    Args:
        key: Group ownership key.
        interface: Interface group declares.
        outputs: Term per output item key, lowered once into the active Group Output node.
        kind: Group tree class.
        name: Group name at creation, the key's display name by default.

    Returns:
        `Ok` of group, each unread menu input linked into a Menu Switch holding its items. `Error` of lowering faults.
    """
    entered = (Graph(kind=kind, nodes={INPUTS: Node(kind=bpy.types.NodeGroupInput)}),) if interface.inputs or interface.panels else ()

    def held(body: Graph) -> Outcome[Graph]:
        read = frozenset(link.source.socket for link in body.links if link.source.node == INPUTS)
        menus = tuple(
            menu(End(node=INPUTS, socket=key), dict.fromkeys(items.items, 0.0)).named(item_key(INPUTS, key))
            for key, entry, _, _ in interface.walked()
            if isinstance(entry, tuple) and entry[1] == "INPUT" and key not in read
            for items in entry[0].items.to_list()
        )
        return lowered(kind, *menus).bind(lambda selected: Graph.merged(kind, (*entered, body, selected)))

    output = Expr(kind=bpy.types.NodeGroupOutput, inputs=tuple(outputs.items()), members={"is_active_output": True}, label=Some(OUTPUTS))
    return lowered(kind, output).bind(held).map(lambda graph: NodeGroup(key=key, name=name.default_with(lambda: bpy.path.display_name(key)), interface=interface, graph=graph))


def placed(group: NodeGroup, inputs: Mapping[Key, Term], output: Key) -> Expr:
    """Places a group node pointed at the group over operands by input item key, read at one output item.

    Args:
        group: Owned group the node points at.
        inputs: Term per input item key, unlisted inputs at their interface defaults.
        output: Output item key the expression reads.

    Returns:
        Group node expression in the output socket's shape.
    """
    socket = {socket.key: socket for socket in group.interface.outputs}[output]
    return Expr(kind=bpy.types.ShaderNodeGroup, inputs=tuple(inputs.items()), content=Some(Nested(group=group)), socket=output, shape=socket.kind)


def grouped[**P](*, kind: type[GraphTree], key: Option[Key] = Nothing) -> Callable[[Body[P]], Group[P]]:
    """Declares a body as an owned group of `kind`, built once at declaration and called like the body to place its group node.

    The group key is the declaring module's name joined to the body's name past any leading underscore, unless a factory building
    one body per row states it, and the group name its display name. The group reads each parameter as an input socket of the socket
    class its `Expr` annotation carries, float by default; a scalar return is the `result` output, a mapping returns outputs by key.

    Args:
        kind: Group tree class.
        key: Group ownership key a factory states per row.

    Returns:
        Decorator building the group from its body, raising `LookupError` with the lowering faults of a body the host refuses.
    """

    def declared(body: Body[P]) -> Group[P]:
        name = body.__name__.lstrip("_")
        owned = key.default_with(lambda: item_key(body.__module__.rpartition(".")[2], name))
        hints = typing.get_type_hints(body, include_extras=True)

        def annotated(name: str) -> Option[Shape]:
            carried = inspect_annotation(hints[name], annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata if name in hints else ()
            return Option.of_optional(next((held for held in carried if isinstance(held, type) and issubclass(held, bpy.types.NodeSocket)), None))

        inputs = tuple(Socket(key=parameter, kind=annotated(parameter).default_value(bpy.types.NodeSocketFloat)) for parameter in inspect.signature(body).parameters)
        bound = inspect.signature(body).bind(**{entry.key: group_input(entry.key, entry.kind) for entry in inputs})
        returned = body(*bound.args, **bound.kwargs)
        outputs: Mapping[Key, Term] = returned if isinstance(returned, Mapping) else {RESULT: returned}
        result = annotated("return").filter(lambda _: not isinstance(returned, Mapping))
        face = Interface(inputs=inputs, outputs=tuple(Socket(key=item, kind=result.default_value(widest(term))) for item, term in outputs.items()))
        match built(owned, face, outputs, kind, name=Some(bpy.path.display_name(key.default_value(name)))):
            case Result(tag="ok", ok=group):
                return Group(body=body, group=group)
            case Result(error=faults):
                raise LookupError(owned, faults)

    return declared


# --- [EVALUATION]


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

    def evaluated(scratch: ExitStack) -> Outcome[tuple[npt.NDArray[np.float32], ...]]:
        scene = bpy.data.scenes.new(".sampled")
        cloud = bpy.data.pointclouds.new(".sampled")
        holder = bpy.data.objects.new(".sampled", cloud)
        made: bpy.types.NodeTree = bpy.data.node_groups.new(".sampled", "GeometryNodeTree")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        scratch.callback(bpy.data.batch_remove, ids=(holder, cloud, made, scene))
        scene.collection.objects.link(holder)
        cloud.resize(count)
        attributes = cloud.attributes

        def attributed(name: str, values: npt.NDArray[np.float32]) -> Outcome[None]:
            kind, member, _ = _STORED[_COLUMNS[1 if values.ndim == 1 else values.shape[1]]]
            attribute = Option.of_optional(attributes.get(name)).default_with(lambda: attributes.new(name, kind, "POINT"))
            data: _Values = attribute.data  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return catch(data.foreach_set, exception=TypeError)(member, values.ravel()).map_error(partial(Fault.of, Reason.REFUSED, repr(attribute)))

        def column_read(points: bpy.types.PointCloud, name: str, shape: Shape) -> Outcome[npt.NDArray[np.float32]]:
            _, member, width = _STORED[shape]
            values = np.empty(count * width, dtype=np.float32)

            def read(attribute: bpy.types.Attribute) -> Outcome[npt.NDArray[np.float32]]:
                data: _Values = attribute.data  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                return (
                    catch(data.foreach_get, exception=TypeError)(member, values)
                    .map_error(partial(Fault.of, Reason.REFUSED, repr(attribute)))
                    .map(lambda _: values if width == 1 else values.reshape(count, width))
                )

            return Option.of_optional(points.attributes.get(name)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(points.attributes), member=name)).bind(read)

        def read(_: object) -> Outcome[tuple[npt.NDArray[np.float32], ...]]:
            modifier: bpy.types.NodesModifier = holder.modifiers.new("Field", "NODES")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            modifier.node_group = made
            layer: bpy.types.ViewLayer = scene.view_layers[0]
            layer.update()
            depsgraph: bpy.types.Depsgraph = layer.depsgraph  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return (
                catch(holder.evaluated_get(depsgraph).evaluated_geometry, exception=TypeError)()
                .map_error(partial(Fault.of, Reason.REFUSED, repr(holder)))
                .bind(
                    lambda geometry: (
                        Option
                        .of_optional(try_downcast(bpy.types.PointCloud, geometry.pointcloud))
                        .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(geometry), member="pointcloud"))
                        .bind(lambda points: accumulated(column_read(points, name, root.shape) for name, root in zip(names, roots)))
                    )
                )
            )

        face = Interface(inputs=(_GEOMETRY,), outputs=(msgspec.structs.replace(_GEOMETRY, key=RESULT),))
        return (
            accumulated(starmap(attributed, held.items()))
            .bind(lambda _: built(".sampled", face, {RESULT: stored}, bpy.types.GeometryNodeTree, name=Some(".sampled")))
            .bind(lambda group: reconciled(group, made, identity))
            .bind(read)
        )

    with ExitStack() as scratch:
        return evaluated(scratch)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EXPRESSIONS", "Body", "ColorExpr", "ExpressionNamespace", "Group", "VectorExpr", "built", "column", "grouped", "lowered", "placed", "sampled"]
