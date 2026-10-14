"""Desired native node graphs: keyed nodes and links, graph content, owned groups, and their node constructors."""

from collections.abc import Callable, Iterable, Iterator, Mapping
import functools
import itertools
from typing import Final, Literal, overload

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.ids.images import ImageFrames, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderEditFault, BlenderResult
from blender.bpy.nodes.interface import Interface, item_key, MenuItems
from blender.bpy.nodes.trees import GraphTree, group_node
from blender.imaging.faults import accumulated, paired
from blender.imaging.hashing import ContentHash
from blender.imaging.parameters import Ramp

# --- [TYPES] ----------------------------------------------------------------------------

type DataType = Literal["FLOAT", "VECTOR", "RGBA"]
type Operand = End | Value | tuple[Fragment, End]
type Part = BlenderResult[Fragment] | BlenderResult[tuple[Fragment, End]]
type Points = tuple[tuple[float, float], ...]
type Content = Ramp | CurvePoints | Generated | Loaded | Nested | MenuItems | FileItems

# --- [CONSTANTS] ------------------------------------------------------------------------

INPUTS: Final[Key] = "inputs"
OUTPUTS: Final[Key] = "outputs"

# --- [MODELS] ---------------------------------------------------------------------------


class End(msgspec.Struct, frozen=True, kw_only=True):
    """Socket end of a link: a node key and a socket address, `Value` unless stated."""

    node: Key
    socket: str = "Value"


class Link(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """One declared connection occurrence from a source end to a target end."""

    source: End
    target: End


class CurvePoints(msgspec.Struct, frozen=True, kw_only=True):
    """Curve map points with optional viewport bounds and independent evaluation clipping."""

    maps: tuple[Points] | tuple[Points, Points, Points, Points]
    clip: Option[tuple[float, float, float, float]] = Nothing
    handle: Literal["AUTO", "AUTO_CLAMPED", "VECTOR"] = "VECTOR"
    clipped: bool = False


class Generated(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Packed generated image a node shows, built once per content."""

    ownership: Ownership[bpy.types.Image]
    content: ContentHash
    name: str
    plane: Callable[[], BlenderResult[Option[npt.NDArray[np.float32]]]]
    tag: Tag = Tag()


class Loaded(msgspec.Struct, frozen=True, kw_only=True):
    """Image file or sequence a node shows."""

    path: str
    tag: Tag = Tag()
    frames: Option[ImageFrames] = Nothing


class Nested(msgspec.Struct, frozen=True, kw_only=True):
    """Owned group a group node points at."""

    group: "NodeGroup"


class FileItem(msgspec.Struct, frozen=True, kw_only=True):
    """File Output item with its socket type and format members."""

    name: str
    kind: DataType
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)


class FileItems(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered items of a File Output node."""

    items: tuple[FileItem, ...]


class Node(msgspec.Struct, frozen=True, kw_only=True):
    """Desired node: its class, members, socket values by address, and graph content."""

    kind: type[bpy.types.Node]
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)
    inputs: dict[str, Value] = msgspec.field(default_factory=dict)
    outputs: dict[str, Value] = msgspec.field(default_factory=dict)
    content: Option[Content] = Nothing


class FrameNode(msgspec.Struct, frozen=True, kw_only=True):
    """Frame node holding its members and the nodes it frames."""

    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)
    nodes: dict[Key, "Node | FrameNode"] = msgspec.field(default_factory=dict)


class Fragment(msgspec.Struct, frozen=True, kw_only=True):
    """Keyed nodes and links of a partial graph."""

    nodes: dict[Key, Node | FrameNode] = msgspec.field(default_factory=dict)
    links: tuple[Link, ...] = ()

    @staticmethod
    def merged(fragments: Iterable["Fragment"]) -> BlenderResult["Fragment"]:
        """Joins fragments in order, each key holding its node once and each link occurrence kept once.

        Args:
            fragments: Fragments joined, earlier keys first.

        Returns:
            `Ok` of the joined fragment; `Error` of `edit` for each key two fragments hold with different nodes.
        """
        held = tuple(fragments)
        claims: dict[Key, list[Node | FrameNode]] = {}
        for key, item in ((key, item) for fragment in held for key, item in fragment.nodes.items()):
            if item not in claims.setdefault(key, []):
                claims[key].append(item)
        return accumulated(
            Ok((key, first)) if not rest else Error((BlenderEditFault(request=f"Fragment.nodes[{key!r}]", error=ValueError(f"{len(rest) + 1} different nodes share key {key!r}")),))
            for key, (first, *rest) in claims.items()
        ).map(lambda nodes: Fragment(nodes=dict(nodes), links=tuple(dict.fromkeys(link for fragment in held for link in fragment.links))))

    @staticmethod
    def joined(*parts: Part) -> BlenderResult["Fragment"]:
        """Joins independent constructions into one fragment.

        Args:
            *parts: Built fragments, or built pairs whose end the join drops.

        Returns:
            `Ok` of every part's fragment joined; `Error` of every part's faults and every key collision `merged` answers, accumulated.
        """
        return accumulated(part.map(lambda built: built if isinstance(built, Fragment) else built[0]) for part in parts).bind(Fragment.merged)


class Clock(msgspec.Struct, frozen=True, kw_only=True):
    """Held clock of one `Motion` panel."""

    key: Key
    speed: Operand
    hold: Operand

    @property
    def value(self) -> BlenderResult[tuple[Fragment, End]]:
        """Clock in clock frames, the scene frame held `hold` frames at a time and scaled by `speed`."""
        return (
            node(f"{self.key}_frame", bpy.types.CompositorNodeSceneTime, {}, {})
            .bind(lambda frame: math(f"{self.key}_steps", "DIVIDE", ended(f"{self.key}_frame", "Frame")(frame), self.hold))
            .bind(lambda steps: math(f"{self.key}_held", "FLOOR", steps))
            .bind(lambda held: math(f"{self.key}_frames", "MULTIPLY", held, self.hold))
            .bind(lambda frames: math(self.key, "MULTIPLY", frames, self.speed))
        )

    @property
    def pattern(self) -> BlenderResult[tuple[Fragment, End]]:
        """Whole clock frames, the pattern index a pattern redrawn per clock frame reads."""
        return self.value.bind(lambda clock: math(f"{self.key}_pattern", "FLOOR", clock))


class Graph(msgspec.Struct, frozen=True, kw_only=True):
    """Desired contents of one node tree."""

    kind: type[GraphTree]
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)
    nodes: dict[Key, Node | FrameNode] = msgspec.field(default_factory=dict)
    links: tuple[Link, ...] = ()

    @property
    def placed(self) -> dict[Key, tuple[Node, Option[Key]]]:
        """Each key's node and parent frame key, preorder, a frame read as a `NodeFrame` node of its members."""

        def walked(items: dict[Key, Node | FrameNode], parent: Option[Key]) -> Iterator[tuple[Key, tuple[Node, Option[Key]]]]:
            for key, item in items.items():
                match item:
                    case FrameNode(members=members, nodes=inner):
                        yield key, (Node(kind=bpy.types.NodeFrame, members=members), parent)
                        yield from walked(inner, Some(key))
                    case Node():
                        yield key, (item, parent)

        return dict(walked(self.nodes, Nothing))

    @property
    def nested(self) -> tuple["NodeGroup", ...]:
        """Groups the graph nests at any depth, one per key."""
        inner = (content.group for node, _ in self.placed.values() for content in node.content.to_list() if isinstance(content, Nested))
        return tuple({group.key: group for child in inner for group in (child, *child.graph.nested)}.values())

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the ownership rows of every nested group in nesting order."""
        return Ok(tuple(group.ownership for group in self.nested))


class NodeGroup(msgspec.Struct, frozen=True, kw_only=True):
    """Owned node group around a graph, with its interface and saved state."""

    key: Key
    name: str
    graph: Graph
    interface: Interface = Interface()
    saved: Saved = Saved.USED
    single: bool = True

    @property
    def ownership(self) -> Ownership[bpy.types.NodeTree]:
        """Ownership row of the group's tree kind, key, and saved state."""
        return Ownership(kind=self.graph.kind, key=self.key, saved=self.saved, single=self.single)

    @classmethod
    def built(cls, key: Key, name: str, interface: Interface, outputs: Mapping[Key, Operand], kind: type[GraphTree] = bpy.types.CompositorNodeTree) -> BlenderResult["NodeGroup"]:
        """Builds a group around its output operands, adding its output node, an input node for an interface declaring inputs, and a Menu Switch per menu input no operand reads.

        Args:
            key: Ownership key of the group.
            name: Name the group takes at creation.
            interface: Interface the group declares.
            outputs: Operand per output item key, each fragment joining the graph.
            kind: Tree class of the group.

        Returns:
            `Ok` of the group, each unread menu input linked into a Menu Switch keyed `inputs_<item key>` holding its items, since
            a group menu input holds items only while linked to one; `Error` of the faults `node` answers.
        """
        entered = {INPUTS: Node(kind=bpy.types.NodeGroupInput)} if interface.inputs or interface.panels else {}

        def held(body: Fragment) -> BlenderResult[Fragment]:
            read = frozenset(link.source.socket for link in body.links if link.source.node == INPUTS)
            return Fragment.joined(
                Ok(body),
                *(
                    menu(item_key(INPUTS, key), "FLOAT", End(node=INPUTS, socket=key), items, {})
                    for key, entry, _, _ in interface.walked()
                    if isinstance(entry, tuple) and entry[1] == "INPUT" and key not in read
                    for items in entry[0].items.to_list()
                ),
            )

        return (
            node(OUTPUTS, bpy.types.NodeGroupOutput, outputs, {"is_active_output": True})
            .bind(held)
            .map(lambda body: cls(key=key, name=name, interface=interface, graph=Graph(kind=kind, nodes={**entered, **body.nodes}, links=body.links)))
        )

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the group's ownership row, then its graph's nested rows."""
        return Ok((self.ownership, self.graph))


# --- [OPERATIONS] -----------------------------------------------------------------------


def ended(key: Key, socket: str) -> Callable[[Fragment], tuple[Fragment, End]]:
    """Pairs a fragment with one output of its node, the operand a later constructor nests."""
    return lambda built: (built, End(node=key, socket=socket))


def node(key: Key, kind: type[bpy.types.Node], inputs: Mapping[str, Operand], members: Mapping[str, Value], content: Option[Content] = Nothing) -> BlenderResult[Fragment]:
    """Builds one keyed node over its operands, a value written into its input and an end linked into it.

    Args:
        key: Key of the node.
        kind: Node class.
        inputs: Operand per input address. Literals have the native socket default's shape; a menu input takes its item name.
        members: Value per RNA member identifier of the class.
        content: Graph content the node holds in its storage.

    Returns:
        `Ok` of the node joined with each operand's fragment and link; `Error` of `absent` for a member the class lacks.
    """

    def placed(address: str, operand: Operand) -> tuple[Fragment, dict[str, Value]]:
        target = End(node=key, socket=address)
        match operand:
            case (Fragment() as held, End() as source):
                return msgspec.structs.replace(held, links=(*held.links, Link(source=source, target=target))), {}
            case End():
                return Fragment(links=(Link(source=operand, target=target),)), {}
            case _:
                return Fragment(), {address: operand}

    placements = tuple(itertools.starmap(placed, inputs.items()))
    return accumulated(Descriptor.resolve(kind, identifier, bpy.types.Property) for identifier in members).bind(
        lambda resolved: Fragment.merged((
            Fragment(
                nodes={key: Node(kind=kind, members=dict(zip(resolved, members.values())), inputs={address: value for _, values in placements for address, value in values.items()}, content=content)}
            ),
            *(held for held, _ in placements),
        ))
    )


def menu(key: Key, kind: str, selector: Operand, items: MenuItems, cases: Mapping[str, Operand]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds a Menu Switch over its items, each case operand at its item name.

    Args:
        key: Key of the node.
        kind: `GeometryNodeMenuSwitch.data_type` identifier of the cases.
        selector: Operand of `Menu`, the group menu socket a menu row links.
        items: Items the switch holds.
        cases: Operand per item name.

    Returns:
        `Ok` of the fragment and its `Output` end; `Error` of the faults `node` answers.
    """
    return node(key, bpy.types.GeometryNodeMenuSwitch, {"Menu": selector, **cases}, {"data_type": kind}, Some(items)).map(ended(key, "Output"))


def selected(key: Key, selector: Operand, items: MenuItems, outputs: Mapping[Key, tuple[str, Mapping[str, Operand]]]) -> BlenderResult[dict[Key, tuple[Fragment, End]]]:
    """Selects named outputs through one Menu Switch and its case boolean outputs.

    Args:
        key: Key of the sole Menu Switch; secondary switches append the output key and the case's menu item index.
        selector: Operand of the menu input.
        items: Ordered native menu items.
        outputs: Native socket kind and nonempty case operands per output key, in construction order.

    Returns:
        Selected outputs sharing their complete fragment, or every independent construction fault.
    """
    position = {item.name: index for index, item in enumerate(items.items)}

    def output(index: int, name: Key, kind: str, cases: Mapping[str, Operand]) -> BlenderResult[tuple[Fragment, End]]:
        if index == 0:
            return menu(key, kind, selector, items, cases)
        choices = iter(cases.items())
        first_name, first = next(choices)

        def chosen(held: BlenderResult[tuple[Fragment, End]], case: tuple[str, Operand]) -> BlenderResult[tuple[Fragment, End]]:
            case_name, operand = case
            return held.bind(lambda off: switched(item_key(key, name, position[case_name]), kind, End(node=key, socket=case_name), off, operand))

        return functools.reduce(chosen, choices, switched(item_key(key, name, position[first_name]), kind, End(node=key, socket=first_name), first, first))

    return accumulated(output(index, name, kind, cases) for index, (name, (kind, cases)) in enumerate(outputs.items())).bind(
        lambda built: Fragment.merged(fragment for fragment, _ in built).map(lambda joined: {name: (joined, end) for name, (_, end) in zip(outputs, built)})
    )


def nested(key: Key, group: NodeGroup, inputs: Mapping[Key, Operand], output: Key) -> BlenderResult[tuple[Fragment, End]]:
    """Builds a group node over a nested group, its inputs and output addressed by item key.

    Args:
        key: Key of the group node.
        group: Owned group the node points at.
        inputs: Operand per input item key of the group's interface.
        output: Output item key the answer ends at.

    Returns:
        `Ok` of the fragment and its output end; `Error` of the faults `node` answers.
    """
    return node(key, group_node(group.graph.kind), inputs, {}, Some(Nested(group=group))).map(ended(key, output))


def switched(key: Key, kind: str, gate: Operand, off: Operand, on: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Builds a lazy whole-image switch from one single-value condition.

    Args:
        key: Key of the node.
        kind: `GeometryNodeSwitch.input_type` identifier of both branches.
        gate: Single-value operand of `Switch`, shared by every pixel.
        off: Operand the false gate answers.
        on: Operand the true gate answers.

    Returns:
        `Ok` of the fragment and its `Output` end; `Error` of the faults `node` answers.
    """
    return node(key, bpy.types.GeometryNodeSwitch, {"Switch": gate, "False": off, "True": on}, {"input_type": kind}).map(ended(key, "Output"))


def masked(key: Key, kind: DataType, gate: Operand, off: Operand, on: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Selects each pixel through a native binary mask without unused nonfinite contamination.

    Args:
        key: Key of the result node, with intermediate keys derived from it.
        kind: Native value shape of both branches.
        gate: Native boolean or comparison operand, zero for off and one for on.
        off: Operand selected at zero.
        on: Operand selected at one.

    Returns:
        Built pair ending at `Value`, `Vector`, or `Image` for float, vector, or RGBA.
        Both branches evaluate; native safe division excludes an unselected NaN or infinity.
        Arithmetic selection preserves values without promising signed-zero identity.
    """
    operation = vector if kind == "VECTOR" else math

    def chosen(prefix: Key, inverse: tuple[Fragment, End], low: Operand, high: Operand) -> BlenderResult[tuple[Fragment, End]]:
        return paired(operation(item_key(prefix, "off"), "DIVIDE", low, inverse), operation(item_key(prefix, "on"), "DIVIDE", high, gate)).bind(lambda branches: operation(prefix, "ADD", *branches))

    def colored(inverse: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        channels = ("Red", "Green", "Blue", "Alpha")
        low_key, high_key = item_key(key, "off_channels"), item_key(key, "on_channels")
        return (
            paired(node(low_key, bpy.types.CompositorNodeSeparateColor, {"Image": off}, {"mode": "RGB"}), node(high_key, bpy.types.CompositorNodeSeparateColor, {"Image": on}, {"mode": "RGB"}))
            .bind(
                lambda branches: accumulated(
                    chosen(item_key(key, "selected", channel.lower()), inverse, ended(low_key, channel)(branches[0]), ended(high_key, channel)(branches[1])) for channel in channels
                )
            )
            .bind(lambda values: node(key, bpy.types.CompositorNodeCombineColor, dict(zip(channels, values)), {"mode": "RGB"}).map(ended(key, "Image")))
        )

    return math(item_key(key, "inverse"), "SUBTRACT", 1.0, gate).bind(lambda inverse: colored(inverse) if kind == "RGBA" else chosen(key, inverse, off, on))


def math(key: Key, operation: str, *operands: Operand, clamp: bool = False) -> BlenderResult[tuple[Fragment, End]]:
    """Builds a shader math node of one operation.

    Args:
        key: Key of the node.
        operation: `ShaderNodeMath.operation` identifier.
        *operands: Operands of `Value`, `Value_001`, and `Value_002` in order.
        clamp: Whether the node clamps its answer to [0, 1].

    Returns:
        `Ok` of the fragment and its `Value` end; `Error` of the faults `node` answers.
    """
    return node(key, bpy.types.ShaderNodeMath, dict(zip(("Value", "Value_001", "Value_002"), operands)), {"operation": operation, "use_clamp": clamp}).map(ended(key, "Value"))


def vector(key: Key, operation: str, *operands: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Builds a vector math node of one operation.

    Args:
        key: Key of the node.
        operation: `ShaderNodeVectorMath.operation` identifier.
        *operands: Operands of the enabled inputs in order. Vector literals have three components; `Scale` takes a scalar. Linked ends use Blender's socket conversion.

    Returns:
        `Ok` of the fragment and its `Value` end for a scalar result, else its `Vector` end; `Error` of the faults `node` answers.
    """
    inputs = {"SCALE": ("Vector", "Scale"), "REFRACT": ("Vector", "Vector_001", "Scale")}.get(operation, ("Vector", "Vector_001", "Vector_002"))
    result = "Value" if operation in {"DOT_PRODUCT", "DISTANCE", "LENGTH"} else "Vector"
    return node(key, bpy.types.ShaderNodeVectorMath, dict(zip(inputs, operands)), {"operation": operation}).map(ended(key, result))


def mixed(key: Key, kind: DataType, factor: Operand, a: Operand, b: Operand, blend: str = "MIX") -> BlenderResult[tuple[Fragment, End]]:
    """Builds a mix node of one data type.

    Args:
        key: Key of the node.
        kind: `ShaderNodeMix.data_type` identifier, naming the node's socket identifiers.
        factor: Operand of `Factor_Float`.
        a: Operand the factor 0 answers.
        b: Operand the factor 1 answers.
        blend: `ShaderNodeMix.blend_type` identifier a color mix reads.

    Returns:
        `Ok` of the fragment and its result end; `Error` of the faults `node` answers.
    """
    first, second, result = {"FLOAT": ("A_Float", "B_Float", "Result_Float"), "VECTOR": ("A_Vector", "B_Vector", "Result_Vector"), "RGBA": ("A_Color", "B_Color", "Result_Color")}[kind]
    return node(key, bpy.types.ShaderNodeMix, {"Factor_Float": factor, first: a, second: b}, {"data_type": kind, "blend_type": blend}).map(ended(key, result))


def compared(key: Key, operation: str, a: Operand, b: Operand, *, epsilon: Option[Operand] = Nothing) -> BlenderResult[tuple[Fragment, End]]:
    """Builds a float compare node.

    Args:
        key: Key of the node.
        operation: `FunctionNodeCompare.operation` identifier.
        a: Operand of `A`.
        b: Operand of `B`.
        epsilon: Optional native `Epsilon` input for equality operations; absence keeps its native default.

    Returns:
        `Ok` of the fragment and its boolean `Result` end; `Error` of the faults `node` answers.
    """
    return node(key, bpy.types.FunctionNodeCompare, {"A": a, "B": b, **epsilon.map(lambda value: {"Epsilon": value}).default_value({})}, {"data_type": "FLOAT", "operation": operation}).map(
        ended(key, "Result")
    )


@overload
def summed(prefix: Key, terms: tuple[tuple[Fragment, End], *tuple[tuple[Fragment, End], ...]], *, kind: DataType = "RGBA") -> BlenderResult[tuple[Fragment, End]]: ...
@overload
def summed(prefix: Key, terms: tuple[Operand, ...], *, kind: DataType = "RGBA") -> BlenderResult[Operand]: ...
def summed(prefix: Key, terms: tuple[Operand, ...], *, kind: DataType = "RGBA") -> BlenderResult[Operand]:
    """Adds operands in order through their native ADD operation.

    Args:
        prefix: Key prefix of the nodes, each keyed `<prefix>_add_<n>` from 1.
        terms: Operands summed in order, the tuple `accumulated` answers among them.
        kind: Native float math, vector math, or RGBA mix operation.

    Returns:
        `Ok` of the last node's pair, the one term alone, or zero for no term (black at alpha 1 for RGBA); `Error` of the faults the nodes answer.
    """

    def added(done: BlenderResult[Operand], step: tuple[int, Operand]) -> BlenderResult[Operand]:
        index, term = step
        return done.bind(
            lambda held: mixed(f"{prefix}_add_{index}", "RGBA", 1.0, held, term, "ADD") if kind == "RGBA" else (vector if kind == "VECTOR" else math)(f"{prefix}_add_{index}", "ADD", held, term)
        )

    match terms:
        case (first, *rest):
            return functools.reduce(added, enumerate(rest, 1), Ok(first))
        case _:
            return Ok((0.0, 0.0, 0.0, 1.0) if kind == "RGBA" else (0.0, 0.0, 0.0) if kind == "VECTOR" else 0.0)


def origin(key: Key, seed: Operand, noise: Operand, pattern: Operand = 0, instance: Operand = 0) -> BlenderResult[tuple[Fragment, End]]:
    """Builds the decorrelation origin of one noise.

    Args:
        key: Key of the origin's scale node, inner nodes keyed from it.
        seed: Operand of the record's `Seed`.
        noise: Member of the effect's closed noise set, or the operand of a stream index.
        pattern: `Clock.pattern` for an origin redrawn per pattern, else 0.
        instance: Index of a repeated draw, else 0.

    Returns:
        `Ok` of the fragment and its 3D origin end; `Error` of the faults `node` answers.
    """
    return (
        node(f"{key}_cell", bpy.types.ShaderNodeCombineXYZ, {"X": seed, "Y": noise, "Z": pattern}, {})
        .bind(lambda cell: node(f"{key}_hash", bpy.types.ShaderNodeTexWhiteNoise, {"Vector": ended(f"{key}_cell", "Vector")(cell), "W": instance}, {"noise_dimensions": "4D"}))
        .bind(lambda hashed: vector(key, "MULTIPLY", ended(f"{key}_hash", "Color")(hashed), (256.0, 256.0, 256.0)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "INPUTS",
    "OUTPUTS",
    "Clock",
    "Content",
    "CurvePoints",
    "DataType",
    "End",
    "FileItem",
    "FileItems",
    "Fragment",
    "FrameNode",
    "Generated",
    "Graph",
    "Link",
    "Loaded",
    "Nested",
    "Node",
    "NodeGroup",
    "Operand",
    "Part",
    "Points",
    "compared",
    "ended",
    "masked",
    "math",
    "menu",
    "mixed",
    "nested",
    "node",
    "origin",
    "selected",
    "summed",
    "switched",
    "vector",
]
