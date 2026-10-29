"""Functions over graph expressions: lowering them into desired graphs, groups built around them, bodies declared as groups, and the Array API namespace of elementwise functions."""

from collections.abc import Callable, Mapping
import functools
import inspect
from math import e
import typing
from typing import Final, override

import bpy
from expression import Nothing, Option, Result, Some
import msgspec
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.changes import Nested
from blender.bpy.nodes.desired import End, Graph, INPUTS, Link, member_paths, Node, NodeGroup, OUTPUTS, RESULT
from blender.bpy.nodes.graphs import Expr, math, menu, Scalar, Shape, Term, vector, where, widest, xyz
from blender.bpy.nodes.interface import Interface, item_key, Socket
from blender.bpy.nodes.trees import GraphTree, native
from blender.imaging.geometry.arithmetic import Namespace
from blender.records.faults import accumulated, Outcome
from blender.records.identity import content_hash

# --- [TYPES] ----------------------------------------------------------------------------

type ColorExpr = typing.Annotated[Expr, bpy.types.NodeSocketColor]
type VectorExpr = typing.Annotated[Expr, bpy.types.NodeSocketVector]


class Body[**P](typing.Protocol):
    """Named function `grouped` declares as a group, read by its name and declaring module."""

    __name__: str
    __module__: str

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> Scalar | Mapping[Key, Expr]:
        """Builds the group outputs over its input expressions."""


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
    order = tuple(done)
    keys: dict[Expr, Key] = {}
    for expr in order:
        keys[expr] = expr.label.default_with(functools.partial(_digest, expr, keys))

    def claimed(expr: Expr) -> Outcome[Graph]:
        literals = {address: term for address, term in expr.inputs if not isinstance(term, Expr | End)}
        return native(tree, expr.kind).bind(
            lambda kind: accumulated(Descriptor.resolve(kind, path, bpy.types.Property) for path in member_paths(expr.members)).map(
                lambda _: Graph(kind=tree, nodes={keys[expr]: Node(kind=kind, members=expr.members, inputs=literals, content=expr.content)})
            )
        )

    links = tuple(
        Link(source=End(node=keys[term], socket=term.socket) if isinstance(term, Expr) else term, target=End(node=keys[expr], socket=address))
        for expr in order
        for address, term in expr.inputs
        if isinstance(term, Expr | End)
    )
    return accumulated(map(claimed, order)).bind(lambda claims: Graph.merged(tree, (*claims, Graph(kind=tree, links=links))))


def _digest(expr: Expr, keys: Mapping[Expr, Key]) -> Key:
    """Content key of one expression over its kind, members, input keys or literals, and content."""

    def entry(term: Term) -> tuple[object, ...]:
        match term:
            case Expr():
                return ("key", keys[term], term.socket)
            case End(node=node, socket=socket):
                return ("end", node, socket)
            case _:
                return ("value", term)

    payload = (expr.kind.__name__, tuple(expr.members.items()), tuple((address, entry(term)) for address, term in expr.inputs), expr.content.map(lambda content: content.keyed).to_optional())
    return f"n{content_hash(Expr, msgspec.msgpack.encode(payload, order='deterministic')).hex()}"


# --- [GROUPS]


def built(key: Key, name: str, interface: Interface, outputs: Mapping[Key, Term], kind: type[GraphTree]) -> Outcome[NodeGroup]:
    """Builds group around output terms, adding Group Input node for interface inputs or panels.

    Args:
        key: Group ownership key.
        name: Group name at creation.
        interface: Interface group declares.
        outputs: Term per output item key, lowered once into the active Group Output node.
        kind: Group tree class.

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
    return lowered(kind, output).bind(held).map(lambda graph: NodeGroup(key=key, name=name, interface=interface, graph=graph))


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
        bound = inspect.signature(body).bind(**{entry.key: Expr(kind=bpy.types.NodeGroupInput, socket=entry.key, shape=entry.kind, label=Some(INPUTS)) for entry in inputs})
        returned = body(*bound.args, **bound.kwargs)
        outputs: Mapping[Key, Term] = returned if isinstance(returned, Mapping) else {RESULT: returned}
        result = annotated("return").filter(lambda _: not isinstance(returned, Mapping))
        face = Interface(inputs=inputs, outputs=tuple(Socket(key=item, kind=result.default_value(widest(term))) for item, term in outputs.items()))
        match built(owned, bpy.path.display_name(key.default_value(name)), face, outputs, kind):
            case Result(tag="ok", ok=group):
                return Group(body=body, group=group)
            case Result(error=faults):
                raise LookupError(owned, faults)

    return declared


# --- [NAMESPACE]


class ExpressionNamespace(Namespace[Expr, Expr]):
    """Elementwise functions over expressions under their Array API names, `EXPRESSIONS` the namespace a map written over numpy takes."""

    @override
    def negative(self, x: Expr, /) -> Expr:
        """Negation."""
        return -x

    @override
    def abs(self, x: Expr, /) -> Expr:
        """Magnitude."""
        return abs(x)

    @override
    def sqrt(self, x: Expr, /) -> Expr:
        """Square root."""
        return math("SQRT", x)

    @override
    def sin(self, x: Expr, /) -> Expr:
        """Sine of radians."""
        return math("SINE", x)

    @override
    def cos(self, x: Expr, /) -> Expr:
        """Cosine of radians."""
        return math("COSINE", x)

    @override
    def tan(self, x: Expr, /) -> Expr:
        """Tangent of radians."""
        return math("TANGENT", x)

    @override
    def asin(self, x: Expr, /) -> Expr:
        """Inverse sine in radians."""
        return math("ARCSINE", x)

    @override
    def atan(self, x: Expr, /) -> Expr:
        """Inverse tangent in radians."""
        return math("ARCTANGENT", x)

    @override
    def atan2(self, x1: Expr, x2: Expr, /) -> Expr:
        """Quadrant-aware inverse tangent of `x1 / x2`."""
        return math("ARCTAN2", x1, x2)

    @override
    def hypot(self, x1: Expr, x2: Expr, /) -> Expr:
        """Length of the vector of `x1` and `x2`."""
        return vector("LENGTH", xyz(x1, x2))

    @override
    def log(self, x: Expr, /) -> Expr:
        """Natural logarithm."""
        return math("LOGARITHM", x, e)

    @override
    def floor(self, x: Expr, /) -> Expr:
        """Greatest integer at or below."""
        return math("FLOOR", x)

    @override
    def remainder(self, x1: Expr, x2: Expr | float, /) -> Expr:
        """Floored modulo, taking the sign of `x2`."""
        return x1 % x2

    @override
    def minimum(self, x1: Expr, x2: Expr | float, /) -> Expr:
        """Lesser value."""
        return math("MINIMUM", x1, x2)

    @override
    def maximum(self, x1: Expr, x2: Expr | float, /) -> Expr:
        """Greater value."""
        return math("MAXIMUM", x1, x2)

    @override
    def less(self, x1: Expr, x2: Expr | float, /) -> Expr:
        """1 where `x1` lies below `x2`."""
        return x1 < x2

    @override
    def greater(self, x1: Expr, x2: Expr | float, /) -> Expr:
        """1 where `x1` lies above `x2`."""
        return x1 > x2

    @override
    def less_equal(self, x1: Expr, x2: Expr | float, /) -> Expr:
        """1 where `x1` lies at or below `x2`."""
        return 1.0 - (x1 > x2)

    @override
    def greater_equal(self, x1: Expr, x2: Expr | float, /) -> Expr:
        """1 where `x1` lies at or above `x2`."""
        return 1.0 - (x1 < x2)

    @override
    def logical_and(self, x1: Expr, x2: Expr, /) -> Expr:
        """1 where both conditions hold."""
        return x1 * x2

    @override
    def logical_or(self, x1: Expr, x2: Expr, /) -> Expr:
        """1 where either condition holds."""
        return math("MAXIMUM", x1, x2)

    @override
    def where(self, condition: Expr, x1: Expr | float, x2: Expr | float, /) -> Expr:
        """`x1` where `condition` holds, else `x2`, per pixel."""
        return where(condition, x1, x2)


EXPRESSIONS: Final = ExpressionNamespace()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EXPRESSIONS", "Body", "ColorExpr", "ExpressionNamespace", "Group", "VectorExpr", "built", "grouped", "lowered", "placed"]
