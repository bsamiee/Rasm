"""Desired node graphs, graph content, owned groups, the graph expression algebra lowering onto them, and its Array API namespace."""

from collections.abc import Callable, Iterable, Iterator, Mapping, Sequence
import functools
import inspect
import itertools
from math import e
from types import MappingProxyType
import typing
from typing import Final, Literal, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from blender.bpy.data.ids.images import ImageFrames, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor, RnaPath, Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.interface import Interface, item_key, MenuItems, Socket
from blender.bpy.nodes.ramps import Ramp
from blender.bpy.nodes.trees import GraphTree, GROUP_NODES
from blender.records.faults import accumulated, Fault, Outcome
from blender.records.identity import content_hash, ContentHash

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type DataType = Literal["FLOAT", "VECTOR", "RGBA"]
type Term = Expr | End | Value
type Scalar = Expr | End | float
type Points = tuple[tuple[float, float], ...]
type Content = Ramp | CurvePoints | Generated | Loaded | Nested | MenuItems | FileItems
type Component = Literal["X", "Y", "Z", "Red", "Green", "Blue", "Alpha"]
type Arithmetic = Literal["ADD", "SUBTRACT", "MULTIPLY", "DIVIDE"]

# --- [CONSTANTS] ------------------------------------------------------------------------

INPUTS: Final[Key] = "inputs"
OUTPUTS: Final[Key] = "outputs"
RESULT: Final[Key] = "result"
SOCKETS: Final[Mapping[DataType, type[bpy.types.NodeSocket]]] = MappingProxyType({"FLOAT": bpy.types.NodeSocketFloat, "VECTOR": bpy.types.NodeSocketVector, "RGBA": bpy.types.NodeSocketColor})
_SHAPES: Final[Mapping[type[bpy.types.NodeSocket], DataType]] = MappingProxyType({socket: shape for shape, socket in SOCKETS.items()})
COLORS: Final[Mapping[type[GraphTree], tuple[type[bpy.types.Node], type[bpy.types.Node]]]] = MappingProxyType({
    bpy.types.ShaderNodeTree: (bpy.types.ShaderNodeSeparateColor, bpy.types.ShaderNodeCombineColor),
    bpy.types.CompositorNodeTree: (bpy.types.CompositorNodeSeparateColor, bpy.types.CompositorNodeCombineColor),
    bpy.types.GeometryNodeTree: (bpy.types.FunctionNodeSeparateColor, bpy.types.FunctionNodeCombineColor),
})

# --- [MODELS] ---------------------------------------------------------------------------


class End(msgspec.Struct, frozen=True, kw_only=True):
    """Link end at node key and socket address: identifier, item key, item name, or position among the enabled sockets once node members are written."""

    node: Key
    socket: str | int = "Value"


class Link(msgspec.Struct, frozen=True, kw_only=True):
    """Declared link between two ends, equal links one link."""

    source: End
    target: End


class CurvePoints(msgspec.Struct, frozen=True, kw_only=True):
    """Curve map points with view bounds and evaluation clipping."""

    maps: tuple[Points, *tuple[Points, ...]]
    clip: Option[tuple[float, float, float, float]] = Nothing
    handle: Literal["AUTO", "AUTO_CLAMPED", "VECTOR"] = "VECTOR"
    clipped: bool = False


class Generated(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Packed generated image node shows, built once per content hash."""

    ownership: Ownership[bpy.types.Image]
    content: ContentHash
    name: str
    plane: Callable[[], Outcome[Option[npt.NDArray[np.float32]]]]
    tag: Tag = Tag()


class Loaded(msgspec.Struct, frozen=True, kw_only=True):
    """Image file or sequence node shows."""

    path: str
    tag: Tag = Tag()
    frames: Option[ImageFrames] = Nothing


class Nested(msgspec.Struct, frozen=True, kw_only=True):
    """Owned group a group node points at."""

    group: "NodeGroup"


class FileItem(msgspec.Struct, frozen=True, kw_only=True):
    """File Output item with format members."""

    name: str
    kind: DataType
    members: Mapping[Descriptor, Value] = msgspec.field(default_factory=dict)


class FileItems(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered File Output node items."""

    items: tuple[FileItem, ...]


class Node(msgspec.Struct, frozen=True, kw_only=True):
    """Desired node with socket values by address."""

    kind: type[bpy.types.Node]
    members: Mapping[Descriptor, Value] = MappingProxyType({})
    inputs: Mapping[str | int, Value] = MappingProxyType({})
    outputs: Mapping[str | int, Value] = MappingProxyType({})
    content: Option[Content] = Nothing


class FrameNode(msgspec.Struct, frozen=True, kw_only=True):
    """Frame node members and framed nodes."""

    members: Mapping[Descriptor, Value] = MappingProxyType({})
    nodes: Mapping[Key, "Node | FrameNode"] = MappingProxyType({})


class Fragment(msgspec.Struct, frozen=True, kw_only=True):
    """Keyed nodes and links of a partial graph."""

    nodes: Mapping[Key, Node | FrameNode] = MappingProxyType({})
    links: tuple[Link, ...] = ()

    @staticmethod
    def merged(fragments: Iterable["Fragment"]) -> Outcome["Fragment"]:
        """Joins fragments, keeping one node per key and each link once.

        Args:
            fragments: Fragments to join, earlier keys first.

        Returns:
            `Ok` of joined fragment. `Error` of `edit` per key holding different nodes.
        """
        held = tuple(fragments)
        pairs = tuple((key, item) for fragment in held for key, item in fragment.nodes.items())
        first = {key: index for index, key in enumerate(dict.fromkeys(key for key, _ in pairs))}
        claims: dict[Key, tuple[Node | FrameNode, ...]] = {
            key: functools.reduce(lambda distinct, item: distinct if item in distinct else (*distinct, item), (item for _, item in run), ())
            for key, run in itertools.groupby(sorted(pairs, key=lambda pair: first[pair[0]]), key=lambda pair: pair[0])
        }
        return accumulated(
            Ok((key, first)) if not rest else Error(Fault.of(BlenderReason.REFUSED, f"Fragment.nodes[{key!r}]", ValueError(f"{len(rest) + 1} different nodes share key {key!r}")))
            for key, (first, *rest) in claims.items()
        ).map(lambda nodes: Fragment(nodes=dict(nodes), links=tuple(dict.fromkeys(link for fragment in held for link in fragment.links))))


class Expr(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Output socket of one desired node over term inputs, keyed at lowering by label or content digest.

    Arithmetic and comparison operators build Math, Vector Math, or Mix nodes by the wider operand shape, scalar literals spreading
    over vector and color channels; subscripts read a vector or color component. Kept expressions lower beside the node unlinked. The class is the Array API
    namespace of elementwise functions over expressions, conditions as 0 and 1 values.
    """

    kind: type[bpy.types.Node]
    inputs: tuple[tuple[str | int, Term], ...] = ()
    members: tuple[tuple[str | RnaPath, Value], ...] = ()
    content: Option[Content] = Nothing
    socket: str | int = "Value"
    shape: DataType = "FLOAT"
    label: Option[Key] = Nothing
    kept: tuple["Expr", ...] = ()

    def __add__(self, other: Scalar | tuple[float, ...]) -> "Expr":
        """Adds `other`."""
        return _arithmetic("ADD", self, other)

    def __radd__(self, other: float | tuple[float, ...]) -> "Expr":
        """Adds this term to `other`."""
        return _arithmetic("ADD", other, self)

    def __sub__(self, other: Scalar | tuple[float, ...]) -> "Expr":
        """Subtracts `other`."""
        return _arithmetic("SUBTRACT", self, other)

    def __rsub__(self, other: float | tuple[float, ...]) -> "Expr":
        """Subtracts this term from `other`."""
        return _arithmetic("SUBTRACT", other, self)

    def __mul__(self, other: Scalar | tuple[float, ...]) -> "Expr":
        """Multiplies by `other`."""
        return _arithmetic("MULTIPLY", self, other)

    def __rmul__(self, other: float | tuple[float, ...]) -> "Expr":
        """Multiplies `other` by this term."""
        return _arithmetic("MULTIPLY", other, self)

    def __truediv__(self, other: Scalar | tuple[float, ...]) -> "Expr":
        """Divides by `other` under safe division."""
        return _arithmetic("DIVIDE", self, other)

    def __rtruediv__(self, other: float | tuple[float, ...]) -> "Expr":
        """Divides `other` by this term under safe division."""
        return _arithmetic("DIVIDE", other, self)

    def __pow__(self, other: Scalar) -> "Expr":
        """Raises to `other`."""
        return _raised(self, other)

    def __rpow__(self, other: float) -> "Expr":
        """Raises `other` to this term."""
        return _raised(other, self)

    def __mod__(self, other: Scalar) -> "Expr":
        """Takes floored modulo by `other`."""
        return _math("FLOORED_MODULO", (self, other))

    def __rmod__(self, other: float) -> "Expr":
        """Takes floored modulo of `other` by this term."""
        return _math("FLOORED_MODULO", (other, self))

    def __lt__(self, other: Scalar) -> "Expr":
        """Compares as 1 where less than `other`, else 0."""
        return _math("LESS_THAN", (self, other))

    def __gt__(self, other: Scalar) -> "Expr":
        """Compares as 1 where greater than `other`, else 0."""
        return _math("GREATER_THAN", (self, other))

    def __neg__(self) -> "Expr":
        """Negates every component, color alpha kept."""
        return _arithmetic("MULTIPLY", self, -1.0)

    def __abs__(self) -> "Expr":
        """Takes the magnitude of every component.

        Returns:
            Absolute Math or Vector Math expression.

        Raises:
            TypeError: For a color, which no Math or Vector Math node takes.
        """
        match self.shape:
            case "FLOAT":
                return _math("ABSOLUTE", (self,))
            case "VECTOR":
                return _vector("ABSOLUTE", (self,))
            case "RGBA":
                raise TypeError(f"{self.kind.__name__}.{self.socket} is a color, which has no absolute value")

    def __getitem__(self, component: Component) -> "Expr":
        """Reads one vector or color component, a color through the lowered tree's `COLORS` row."""
        match component:
            case "X" | "Y" | "Z":
                return Expr(kind=bpy.types.ShaderNodeSeparateXYZ, inputs=(("Vector", self),), socket=component)
            case "Red" | "Green" | "Blue" | "Alpha":
                return Expr(kind=COLORS[bpy.types.ShaderNodeTree][0], inputs=((0, self),), members=(("mode", "RGB"),), socket=component)

    def named(self, key: Key) -> "Expr":
        """Returns the same node under a stable key, for nodes stored, edited, viewed, or inspected by key."""
        return msgspec.structs.replace(self, label=Some(key))

    def keeping(self, *roots: "Expr") -> "Expr":
        """Returns the same node holding `roots` in every graph it lowers into, though no input reads them."""
        return msgspec.structs.replace(self, kept=(*self.kept, *roots))

    # --- [ELEMENTWISE]

    @staticmethod
    def negative(x: "Expr", /) -> "Expr":
        """Negation."""
        return -x

    @staticmethod
    def abs(x: "Expr", /) -> "Expr":
        """Magnitude."""
        return abs(x)

    @staticmethod
    def sqrt(x: "Expr", /) -> "Expr":
        """Square root."""
        return math("SQRT", x)

    @staticmethod
    def sin(x: "Expr", /) -> "Expr":
        """Sine of radians."""
        return math("SINE", x)

    @staticmethod
    def cos(x: "Expr", /) -> "Expr":
        """Cosine of radians."""
        return math("COSINE", x)

    @staticmethod
    def tan(x: "Expr", /) -> "Expr":
        """Tangent of radians."""
        return math("TANGENT", x)

    @staticmethod
    def asin(x: "Expr", /) -> "Expr":
        """Inverse sine in radians."""
        return math("ARCSINE", x)

    @staticmethod
    def atan(x: "Expr", /) -> "Expr":
        """Inverse tangent in radians."""
        return math("ARCTANGENT", x)

    @staticmethod
    def atan2(x1: "Expr", x2: "Expr", /) -> "Expr":
        """Quadrant-aware inverse tangent of `x1 / x2`."""
        return math("ARCTAN2", x1, x2)

    @staticmethod
    def hypot(x1: "Expr", x2: "Expr", /) -> "Expr":
        """Length of the vector of `x1` and `x2`."""
        return vector("LENGTH", xyz(x1, x2))

    @staticmethod
    def log(x: "Expr", /) -> "Expr":
        """Natural logarithm."""
        return math("LOGARITHM", x, e)

    @staticmethod
    def floor(x: "Expr", /) -> "Expr":
        """Greatest integer at or below."""
        return math("FLOOR", x)

    @staticmethod
    def remainder(x1: "Expr", x2: "Expr | float", /) -> "Expr":
        """Floored modulo, taking the sign of `x2`."""
        return x1 % x2

    @staticmethod
    def minimum(x1: "Expr", x2: "Expr | float", /) -> "Expr":
        """Lesser value."""
        return math("MINIMUM", x1, x2)

    @staticmethod
    def maximum(x1: "Expr", x2: "Expr | float", /) -> "Expr":
        """Greater value."""
        return math("MAXIMUM", x1, x2)

    @staticmethod
    def less(x1: "Expr", x2: "Expr | float", /) -> "Expr":
        """1 where `x1` lies below `x2`."""
        return x1 < x2

    @staticmethod
    def greater(x1: "Expr", x2: "Expr | float", /) -> "Expr":
        """1 where `x1` lies above `x2`."""
        return x1 > x2

    @staticmethod
    def less_equal(x1: "Expr", x2: "Expr | float", /) -> "Expr":
        """1 where `x1` lies at or below `x2`."""
        return 1.0 - (x1 > x2)

    @staticmethod
    def greater_equal(x1: "Expr", x2: "Expr | float", /) -> "Expr":
        """1 where `x1` lies at or above `x2`."""
        return 1.0 - (x1 < x2)

    @staticmethod
    def logical_and(x1: "Expr", x2: "Expr", /) -> "Expr":
        """1 where both conditions hold."""
        return x1 * x2

    @staticmethod
    def logical_or(x1: "Expr", x2: "Expr", /) -> "Expr":
        """1 where either condition holds."""
        return math("MAXIMUM", x1, x2)

    @staticmethod
    def where(condition: "Expr", x1: "Expr | float", x2: "Expr | float", /) -> "Expr":
        """`x1` where `condition` holds, else `x2`, per pixel."""
        return where(condition, x1, x2)


class Graph(msgspec.Struct, frozen=True, kw_only=True):
    """Desired contents of one node tree."""

    kind: type[GraphTree]
    members: Mapping[Descriptor, Value] = MappingProxyType({})
    nodes: Mapping[Key, Node | FrameNode] = MappingProxyType({})
    links: tuple[Link, ...] = ()

    @property
    def placed(self) -> dict[Key, tuple[Node, Option[Key]]]:
        """Node and parent frame key per key in preorder, frames as `NodeFrame` nodes."""

        def walked(items: Mapping[Key, Node | FrameNode], parent: Option[Key]) -> Iterator[tuple[Key, tuple[Node, Option[Key]]]]:
            for key, item in items.items():
                match item:
                    case FrameNode(members=members, nodes=inner):
                        yield key, (Node(kind=bpy.types.NodeFrame, members=members), parent)
                        yield from walked(inner, Some(key))
                    case Node():
                        yield key, (item, parent)

        return dict(walked(self.nodes, Nothing))

    @classmethod
    def lowered(cls, kind: type[GraphTree], *roots: Expr) -> Outcome["Graph"]:
        """Lowers root expressions, such as a tree's output nodes, into the desired contents of a `kind` tree.

        Returns:
            `Ok` of graph holding every node the roots read. `Error` of member resolution faults and key collisions.
        """
        return lowered(kind, *roots).map(lambda body: cls(kind=kind, nodes=body.nodes, links=body.links))

    @property
    def nested(self) -> tuple["NodeGroup", ...]:
        """Groups nested at any depth, one per key."""
        inner = (content.group for node, _ in self.placed.values() for content in node.content.to_list() if isinstance(content, Nested))
        return tuple({group.key: group for child in inner for group in (child, *child.graph.nested)}.values())

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns nested group ownership rows in nesting order."""
        return Ok(tuple(group.ownership for group in self.nested))


class NodeGroup(msgspec.Struct, frozen=True, kw_only=True):
    """Owned node group of a graph, interface, and saved state."""

    key: Key
    name: str
    graph: Graph
    interface: Interface = Interface()
    saved: Saved = Saved.USED
    single: bool = True

    @property
    def ownership(self) -> Ownership[bpy.types.NodeTree]:
        """Group tree ownership row."""
        return Ownership(kind=self.graph.kind, key=self.key, saved=self.saved, single=self.single)

    @classmethod
    def built(cls, key: Key, name: str, interface: Interface, outputs: Mapping[Key, Term], kind: type[GraphTree]) -> Outcome["NodeGroup"]:
        """Builds group around output terms, adding Group Input node for interface inputs or panels.

        Args:
            key: Group ownership key.
            name: Group name at creation.
            interface: Interface group declares.
            outputs: Term per output item key, lowered once into the active Group Output node.
            kind: Group tree class.

        Returns:
            `Ok` of group, each unread menu input linked into Menu Switch `inputs_<item key>` holding its items. `Error` of lowering faults.
        """
        entered: dict[Key, Node | FrameNode] = {INPUTS: Node(kind=bpy.types.NodeGroupInput)} if interface.inputs or interface.panels else {}

        def held(body: Fragment) -> Outcome[Fragment]:
            read = frozenset(link.source.socket for link in body.links if link.source.node == INPUTS)
            menus = tuple(
                _menu("FLOAT", "FLOAT", End(node=INPUTS, socket=key), items, {}).named(item_key(INPUTS, key))
                for key, entry, _, _ in interface.walked()
                if isinstance(entry, tuple) and entry[1] == "INPUT" and key not in read
                for items in entry[0].items.to_list()
            )
            return lowered(kind, *menus).bind(lambda selected: Fragment.merged((body, selected)))

        output = Expr(kind=bpy.types.NodeGroupOutput, inputs=tuple(outputs.items()), members=(("is_active_output", True),), label=Some(OUTPUTS))
        return lowered(kind, output).bind(held).map(lambda body: cls(key=key, name=name, interface=interface, graph=Graph(kind=kind, nodes={**entered, **body.nodes}, links=body.links)))

    def placed(self, inputs: Mapping[Key, Term], output: Key) -> Expr:
        """Places a group node pointed at this group over operands by input item key, read at one output item.

        Args:
            inputs: Term per input item key, unlisted inputs at their interface defaults.
            output: Output item key the expression reads.

        Returns:
            Group node expression in the output socket's shape.
        """
        socket = {socket.key: socket for socket in self.interface.outputs}[output]
        return Expr(kind=GROUP_NODES[self.graph.kind], inputs=tuple(inputs.items()), content=Some(Nested(group=self)), socket=output, shape=_SHAPES[socket.kind])

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns group ownership row, then graph's nested rows."""
        return Ok((self.ownership, self.graph))


class Group[**P](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Owned group `grouped` declares from one body, placed by calling it like the body."""

    body: Callable[P, Scalar | Mapping[Key, Expr]]
    group: NodeGroup

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> Expr:
        """Places the group node over arguments bound like the body, read at its first output."""
        bound = inspect.signature(self.body).bind(*args, **kwargs)
        bound.apply_defaults()
        return self.group.placed(bound.arguments, self.group.interface.outputs[0].key)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ALGEBRA]


def math(operation: "rna_enums.NodeMathItems", /, *operands: Scalar, clamp: bool = False) -> Expr:
    """Builds Math node of one operation over its enabled inputs in order.

    Args:
        operation: `ShaderNodeMath.operation` identifier.
        *operands: Term per input in order.
        clamp: Whether node clamps result to [0, 1].

    Returns:
        `Value` socket expression.
    """
    return _math(operation, operands, clamp=clamp)


def vector(operation: "rna_enums.NodeVecMathItems", /, *operands: Scalar | tuple[float, ...]) -> Expr:
    """Builds Vector Math node of one operation over its enabled inputs in order, the scalar `Scale` among them where the operation reads one.

    Returns:
        `Value` socket expression for scalar results, else `Vector`.
    """
    return _vector(operation, operands)


def mixed(factor: Scalar, a: Term, b: Term, /, *, blend: "rna_enums.RampBlendItems" = "MIX") -> Expr:
    """Builds Mix node of the operands' shape, `a` at factor 0 and `b` at factor 1, blended by `ShaderNodeMix.blend_type` identifier."""
    return _mixed(_shape(a, b), factor, a, b, blend=blend)


def switched(gate: Scalar, off: Term, on: Term, /) -> Expr:
    """Builds lazy whole-image Switch node of the branches' shape over a single-value gate, `off` for false and `on` for true."""
    shape = _shape(off, on)
    return Expr(kind=bpy.types.GeometryNodeSwitch, inputs=(("Switch", gate), ("False", off), ("True", on)), members=(("input_type", shape),), socket="Output", shape=shape)


def menu(selector: Scalar | str, items: MenuItems, cases: Mapping[str, Term], /, *, kind: DataType | Literal["MENU", "BOOLEAN"]) -> Expr:
    """Builds Menu Switch of `kind` cases, each case operand at its item name, every item's boolean output beside `Output`.

    Args:
        selector: Menu input operand, or item name.
        items: Ordered menu items.
        cases: Term per item name.
        kind: `GeometryNodeMenuSwitch.data_type` identifier of the cases.

    Returns:
        `Output` socket expression.
    """
    match kind:
        case "VECTOR" | "RGBA" as shape:
            return _menu(kind, shape, selector, items, cases)
        case _:
            return _menu(kind, "FLOAT", selector, items, cases)


def where(gate: Term, on: Term, off: Term) -> Expr:
    """Selects each pixel by binary mask over both evaluated branches, dropping the unselected branch's nonfinite values.

    Args:
        gate: Boolean or comparison term, 1 for `on` and 0 for `off`.
        on: Term selected at 1.
        off: Term selected at 0.

    Returns:
        `Value`, `Vector`, or `Image` expression by the branches' shape, `off / (1 - gate) + on / gate` under safe division.
    """
    shape = _shape(on, off)
    inverse = _math("SUBTRACT", (1.0, gate))
    match shape:
        case "FLOAT":
            return _math("ADD", (_math("DIVIDE", (off, inverse)), _math("DIVIDE", (on, gate))))
        case "VECTOR":
            return _vector("ADD", (_vector("DIVIDE", (_spread(off, shape), inverse)), _vector("DIVIDE", (_spread(on, shape), gate))))
        case "RGBA":
            red, green, blue, alpha = (
                where(gate, *(Expr(kind=COLORS[bpy.types.ShaderNodeTree][0], inputs=((0, _spread(branch, shape)),), members=(("mode", "RGB"),), socket=channel) for branch in (on, off)))
                for channel in ("Red", "Green", "Blue", "Alpha")
            )
            return rgba(red, green, blue, alpha)


def group_input(key: Key, shape: DataType = "FLOAT") -> Expr:
    """Reads one Group Input output of the group being built, by interface item key."""
    return Expr(kind=bpy.types.NodeGroupInput, socket=key, shape=shape, label=Some(INPUTS))


def xyz(x: Scalar = 0.0, y: Scalar = 0.0, z: Scalar = 0.0) -> Expr:
    """Builds Combine XYZ vector of three scalars."""
    return Expr(kind=bpy.types.ShaderNodeCombineXYZ, inputs=(("X", x), ("Y", y), ("Z", z)), socket="Vector", shape="VECTOR")


def rgba(red: Scalar, green: Scalar, blue: Scalar, alpha: Scalar = 1.0) -> Expr:
    """Builds Combine Color of four RGB channels, its class the lowered tree's `COLORS` row."""
    return Expr(kind=COLORS[bpy.types.ShaderNodeTree][1], inputs=(("Red", red), ("Green", green), ("Blue", blue), ("Alpha", alpha)), members=(("mode", "RGB"),), socket=0, shape="RGBA")


def grouped[**P](key: Key, name: str, kind: type[GraphTree]) -> Callable[[Callable[P, Scalar | Mapping[Key, Expr]]], Group[P]]:
    """Declares a body as an owned group of `kind`, built once at declaration and called like the body to place its group node.

    The group reads each parameter as an input socket of the `NodeSocketFloat`, `NodeSocketVector`, or `NodeSocketColor` class its
    `Expr` annotation carries, float by default, titled by its display name; a scalar return is the `result`
    output, a mapping returns outputs by key.

    Args:
        key: Group ownership key.
        name: Group name at creation.
        kind: Group tree class.

    Returns:
        Decorator building the group from its body, raising `LookupError` with the lowering faults of a body the host refuses.
    """

    def declared(body: Callable[P, Scalar | Mapping[Key, Expr]]) -> Group[P]:
        hints = typing.get_type_hints(body, include_extras=True)
        inputs = tuple(
            Socket(
                key=parameter,
                name=bpy.path.display_name(parameter),
                kind=next(
                    (socket for socket in _SHAPES if socket in inspect_annotation(hints[parameter], annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata),
                    bpy.types.NodeSocketFloat,
                ),
            )
            for parameter in inspect.signature(body).parameters
        )
        bound = inspect.signature(body).bind(**{entry.key: group_input(entry.key, _SHAPES[entry.kind]) for entry in inputs})
        returned = body(*bound.args, **bound.kwargs)
        outputs: Mapping[Key, Term] = returned if isinstance(returned, Mapping) else {RESULT: returned}
        face = Interface(inputs=inputs, outputs=tuple(Socket(key=item, name=bpy.path.display_name(item), kind=SOCKETS[_shape(term)]) for item, term in outputs.items()))
        match NodeGroup.built(key, name, face, outputs, kind):
            case Result(tag="ok", ok=group):
                return Group(body=body, group=group)
            case Result(error=faults):
                raise LookupError(key, faults)

    return declared


def _arithmetic(operation: Arithmetic, a: Term, b: Term) -> Expr:
    shape = _shape(a, b)
    match shape:
        case "FLOAT":
            return _math(operation, (a, b))
        case "VECTOR":
            return _vector(operation, (_spread(a, shape), _spread(b, shape)))
        case "RGBA":
            return _mixed(shape, 1.0, _spread(a, shape), _spread(b, shape), blend=operation)


def _raised(base: Term, exponent: Term) -> Expr:
    shape = _shape(base, exponent)
    match shape:
        case "FLOAT":
            return _math("POWER", (base, exponent))
        case "VECTOR":
            return _vector("POWER", (_spread(base, shape), _spread(exponent, shape)))
        case "RGBA":
            raise TypeError("Mix nodes hold no power blend")


def _math(operation: str, operands: Sequence[Term], *, clamp: bool = False) -> Expr:
    return Expr(kind=bpy.types.ShaderNodeMath, inputs=tuple(enumerate(operands)), members=(("operation", operation), ("use_clamp", clamp)))


def _vector(operation: str, operands: Sequence[Term]) -> Expr:
    scalar, scaled = operation in {"DOT_PRODUCT", "DISTANCE", "LENGTH"}, operation in {"SCALE", "REFRACT"}
    spread = tuple(operand if scaled and index == len(operands) - 1 else _spread(operand, "VECTOR") for index, operand in enumerate(operands))
    return Expr(
        kind=bpy.types.ShaderNodeVectorMath, inputs=tuple(enumerate(spread)), members=(("operation", operation),), socket="Value" if scalar else "Vector", shape="FLOAT" if scalar else "VECTOR"
    )


def _mixed(shape: DataType, factor: Term, a: Term, b: Term, *, blend: str) -> Expr:
    return Expr(kind=bpy.types.ShaderNodeMix, inputs=tuple(enumerate((factor, a, b))), members=(("data_type", shape), ("blend_type", blend)), socket=0, shape=shape)


def _menu(kind: str, shape: DataType, selector: Term, items: MenuItems, cases: Mapping[str, Term]) -> Expr:
    return Expr(kind=bpy.types.GeometryNodeMenuSwitch, inputs=(("Menu", selector), *cases.items()), members=(("data_type", kind),), content=Some(items), socket="Output", shape=shape)


def _shape(*terms: Term) -> DataType:
    def of(term: Term) -> DataType:
        match term:
            case Expr(shape=shape):
                return shape
            case (_, _, _):
                return "VECTOR"
            case (_, _, _, _):
                return "RGBA"
            case _:
                return "FLOAT"

    return next((shape for shape in map(of, terms) if shape != "FLOAT"), "FLOAT")


def _spread(term: Term, shape: DataType) -> Term:
    match term, shape:
        case (int() | float()) as value, "VECTOR":
            return (value, value, value)
        case (int() | float()) as value, "RGBA":
            return (value, value, value, 1.0)
        case _:
            return term


# --- [LOWERING]


def lowered(tree: type[GraphTree], *roots: Expr) -> Outcome[Fragment]:
    """Lowers root expressions and every node they read into keyed nodes and links of a `tree` tree, each color node the tree's `COLORS` class.

    Each expression lowers once per call through one walk over the shared graph, with the expressions each keeps, keyed by label, else
    by `n` and a content digest of kind, members, input keys or literals, and content; member resolution faults and differing nodes
    under one key accumulate.

    Returns:
        `Ok` of fragment holding the roots and every node they read. `Error` of member resolution faults and key collisions.
    """
    native = dict(zip(COLORS[bpy.types.ShaderNodeTree], COLORS[tree], strict=True))
    keys: dict[Expr, Key] = {}
    held: dict[Key, Expr] = {}
    parts: list[Outcome[Fragment]] = []
    walk: list[Link | tuple[Expr | End, End]] = []

    def entry(term: Term) -> tuple[str | Value, ...]:
        match term:
            case Expr():
                return ("key", keys[term], term.socket)
            case End(node=node, socket=socket):
                return ("end", node, socket)
            case _:
                return ("value", term)

    def stored(content: Content) -> object:
        match content:
            case Generated(ownership=ownership, content=hashed, name=name, tag=tag):
                return (ownership.key, hashed, name, tag)
            case Nested(group=group):
                return group.key
            case FileItems(items=items):
                return tuple((item.name, item.kind, tuple((held.path, value) for held, value in item.members.items())) for item in items)
            case Ramp() | CurvePoints() | Loaded() | MenuItems():
                return content

    def digest(expr: Expr) -> Key:
        payload = (expr.kind.__name__, expr.members, tuple((address, entry(term)) for address, term in expr.inputs), expr.content.map(stored))
        return f"n{content_hash(Expr, msgspec.msgpack.encode(payload, order='deterministic')).hex()}"

    def claimed(expr: Expr) -> Outcome[Fragment]:
        literals = {address: term for address, term in expr.inputs if not isinstance(term, Expr | End)}
        kind = native.get(expr.kind, expr.kind)
        return accumulated(Descriptor.resolve(kind, identifier, bpy.types.Property) for identifier, _ in expr.members).map(
            lambda resolved: Fragment(nodes={keys[expr]: Node(kind=kind, members=dict(zip(resolved, (value for _, value in expr.members))), inputs=literals, content=expr.content)})
        )

    def entered(expr: Expr) -> None:
        match held.get(keys[expr]):
            case None:
                held[keys[expr]] = expr
                parts.append(claimed(expr))
                walk.extend((term, End(node=keys[expr], socket=address)) for address, term in reversed(expr.inputs) if isinstance(term, Expr | End))
            case first if first is not expr:
                parts.append(claimed(expr))
            case _:
                return
        for root in expr.kept:
            entered(root)

    def stepped(item: Link | tuple[Expr | End, End]) -> None:
        match item:
            case Link():
                parts.append(Ok(Fragment(links=(item,))))
            case (Expr() as term, target):
                walk.append(Link(source=End(node=keys[term], socket=term.socket), target=target))
                entered(term)
            case (End() as term, target):
                parts.append(Ok(Fragment(links=(Link(source=term, target=target),))))

    pending = [(root, False) for root in roots]
    while pending:
        expr, expanded = pending.pop()
        if expr in keys:
            continue
        children = tuple(term for term in (*(term for _, term in expr.inputs), *expr.kept) if isinstance(term, Expr) and term not in keys)
        if expanded or not children:
            keys[expr] = expr.label.default_with(lambda: digest(expr))
        else:
            pending.extend(((expr, True), *((child, False) for child in children)))
    for root in roots:
        entered(root)
        while walk:
            stepped(walk.pop())
    return accumulated(parts).bind(Fragment.merged)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "COLORS",
    "INPUTS",
    "OUTPUTS",
    "RESULT",
    "SOCKETS",
    "Content",
    "CurvePoints",
    "DataType",
    "End",
    "Expr",
    "FileItem",
    "FileItems",
    "Fragment",
    "FrameNode",
    "Generated",
    "Graph",
    "Group",
    "Link",
    "Loaded",
    "Nested",
    "Node",
    "NodeGroup",
    "Points",
    "Scalar",
    "Term",
    "group_input",
    "grouped",
    "lowered",
    "math",
    "menu",
    "mixed",
    "rgba",
    "switched",
    "vector",
    "where",
    "xyz",
]
