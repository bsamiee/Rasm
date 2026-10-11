# mypy: disable-error-code="type-arg"
# ty: ignore[missing-type-argument]
"""Desired native node graphs, owned groups, reconciliation, and compositor coordinates."""

from collections import deque
from collections.abc import Callable, Generator, Iterator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import Enum
import functools
import itertools
from operator import or_
import typing
from typing import Final, Literal, overload, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import idprop
import msgspec
import msgspec.inspect
import msgspec.structs
import numpy as np
import numpy.typing as npt

from rasm.blender.data.ids import images
from rasm.blender.data.ids.images import generated, ImageFrames, loaded, Tag
from rasm.blender.data.ids.ownership import earliest, Mark, Ownership, Saved
from rasm.blender.data.ids.store import Handle
from rasm.blender.data.rna.descriptors import Descriptor, IdName, struct_at, Value, written
from rasm.blender.data.rna.snapshots import Snapshot
from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from rasm.blender.settings.properties import Changed, RecordGroup, Unit
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.hashing import content_hash, ContentHash
from rasm.imaging.parameters import Case, Choice, Gated, GraphForm, Knot, markers, Presentation, Quantity, Ramp, Rgb
from rasm.rendering.edits import Owner, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type SocketSide = Literal["INPUT", "OUTPUT"]
type GraphTree = bpy.types.CompositorNodeTree | bpy.types.GeometryNodeTree | bpy.types.ShaderNodeTree
type Operand = End | Value | tuple[Fragment, End]
type Part = BlenderResult[Fragment] | BlenderResult[tuple[Fragment, End]]
type Points = tuple[tuple[float, float], ...]
type Content = Ramp | CurvePoints | Generated | Loaded | Nested | MenuItems | FileItems
type Structural = Configured | Withdrawn | Declared | Removed | Added
type Alteration = Altered | Disconnected
type TreeChange = Structural | Alteration | Connected | Reordered | Valued | Restated
type InterfaceItem = bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel


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


class Socket(msgspec.Struct, frozen=True, kw_only=True):
    """Interface socket keyed by its parameter key."""

    key: Key
    name: str
    kind: type[bpy.types.NodeSocket]
    description: str = ""
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)


class Interface(msgspec.Struct, frozen=True, kw_only=True):
    """Group interface of output sockets, input sockets, and panels in Blender's order."""

    outputs: tuple[Socket, ...] = ()
    inputs: tuple[Socket, ...] = ()
    panels: tuple["InterfacePanel", ...] = ()

    def walked(self, parent: Option[Key] = Nothing) -> Iterator[tuple[Key, "tuple[Socket, SocketSide] | InterfacePanel", Option[Key], int]]:
        """Walks the interface items preorder.

        Args:
            parent: Key of the panel holding these items, `Nothing` at the root.

        Yields:
            Each item's key, the item (a socket paired with its side, or a panel), its parent key, and its position under its parent.
        """
        items: tuple[tuple[Socket, SocketSide] | InterfacePanel, ...] = (*((socket, "OUTPUT") for socket in self.outputs), *((socket, "INPUT") for socket in self.inputs), *self.panels)
        for position, item in enumerate(items):
            match item:
                case InterfacePanel(key=key):
                    yield key, item, parent, position
                    yield from item.walked(Some(key))
                case (socket, _):
                    yield socket.key, item, parent, position

    @classmethod
    def of(cls, kind: type[msgspec.Struct]) -> BlenderResult["Interface"]:
        """Constructs a record's input interface from native fields and their inspected types."""
        refused = {"DISTANCE_CAMERA": "NONE", "POWER": "NONE", "XYZ_LENGTH": "TRANSLATION"}

        def faced(items: tuple[Socket | InterfacePanel, ...]) -> Interface:
            return cls(inputs=tuple(item for item in items if isinstance(item, Socket)), panels=tuple(item for item in items if isinstance(item, InterfacePanel)))

        def walk(path: tuple[Key | type[Case], ...], record: msgspec.inspect.StructType, held: Option[object]) -> BlenderResult[Interface]:
            annotations = {field.name: field for field in msgspec.structs.fields(record.cls)}
            return accumulated(
                entry(
                    (*path, field.name),
                    annotations[field.name],
                    field.type,
                    lambda: held.map(lambda value: getattr(value, field.name)).or_else_with(
                        lambda: Some(field.default) if field.default is not msgspec.NODEFAULT else Some(field.default_factory()) if field.default_factory is not msgspec.NODEFAULT else Nothing
                    ),
                    field.name.replace("_", " ").title(),
                    "",
                )
                for field in record.fields
            ).map(lambda parts: faced(tuple(itertools.chain.from_iterable(parts))))

        def panel(path: tuple[Key | type[Case], ...], name: str, description: str, toggle: Option[bool], inner: BlenderResult[Interface]) -> BlenderResult[tuple[Socket | InterfacePanel, ...]]:
            return inner.map(lambda face: (InterfacePanel(key=item_key(*path), name=name, description=description, toggle=toggle, inputs=face.inputs, panels=face.panels),))

        def entry(
            path: tuple[Key | type[Case], ...],
            field: msgspec.structs.FieldInfo,
            shape: msgspec.inspect.Type,
            default: Callable[[], Option[object]],
            title: str,
            description: str,
            presented: Option[object] = Nothing,
        ) -> BlenderResult[tuple[Socket | InterfacePanel, ...]]:
            def socket(
                kind: type[bpy.types.NodeSocket], owner: type[bpy.types.NodeTreeInterfaceSocket], members: BlenderResult[Mapping[str, Value]], address: str = item_key(*path), name: str = title
            ) -> BlenderResult[tuple[Socket | InterfacePanel, ...]]:
                return members.bind(
                    lambda values: accumulated(Descriptor.resolve(owner, identifier, bpy.types.Property) for identifier in values).map(
                        lambda resolved: (Socket(key=address, name=name, kind=kind, description=description, members=dict(zip(resolved, values.values()))),)
                    )
                )

            def ranged(low: float | None, high: float | None, soft: Option[tuple[float, float]], scale: float) -> dict[str, float]:
                bounds = soft.map(lambda span: (Some(span[0] * scale), Some(span[1] * scale))).default_value((Option.of_optional(low), Option.of_optional(high)))
                return {name: limit for name, side in zip(("min_value", "max_value"), bounds) for limit in side.to_list()}

            def given(members: Mapping[str, Value] = {}) -> BlenderResult[dict[str, Value]]:
                def lowered(value: object) -> BlenderResult[Value]:
                    match shape, value:  # type: ignore[exhaustive-match]
                        case msgspec.inspect.FloatType(), _:
                            return Unit.of(annotation, shape).stored(value, shape)
                        case msgspec.inspect.TupleType(), _:
                            return Unit.of(annotation, shape).stored(value, shape)
                        case msgspec.inspect.StructType(cls=record, fields=fields), Rgb():
                            declared = {channel.name: channel for channel in msgspec.structs.fields(record)}
                            channels: list[BlenderResult[float]] = []
                            for channel in fields:
                                numeric = channel.type
                                while isinstance(numeric, msgspec.inspect.Metadata):
                                    numeric = numeric.type
                                channels.append(Unit.of(declared[channel.name].type, numeric).stored(getattr(value, channel.name), numeric))  # type: ignore[call-overload]  # ty: ignore[no-matching-overload]
                            return accumulated(channels).map(lambda values: (*values, 1.0))
                        case _, Enum() as member:
                            return Ok(member.label if isinstance(member, Choice) else member.name)
                        case _, bool() | str() | int() as scalar:
                            return Ok(scalar)
                    raise TypeError(f"{value!r} is not a socket default for {shape!r}")

                return default().map(lambda value: lowered(value).map(lambda result: {**members, "default_value": result})).default_value(Ok(dict(members)))

            if markers(field.type, GraphForm):
                return Ok(())
            annotation = presented.default_value(field.type)
            shown = next(iter(markers(annotation, Presentation)), Presentation())
            match shape:  # type: ignore[exhaustive-match]
                case msgspec.inspect.Metadata() as metadata:
                    described = Option.of_optional(metadata.extra_json_schema).default_value({})
                    return entry(path, field, metadata.type, default, described.get("title", title), described.get("description", description), presented)
                case msgspec.inspect.FloatType():
                    unit = Unit.of(annotation, shape)
                    return socket(
                        bpy.types.NodeSocketFloat,
                        bpy.types.NodeTreeInterfaceSocketFloat,
                        unit.bounds(shape).bind(lambda bounds: given({"subtype": refused.get(unit.number, unit.number), **ranged(*bounds, shown.soft, unit.scale)})),
                    )
                case msgspec.inspect.IntType(ge=low, le=high):
                    return socket(
                        bpy.types.NodeSocketInt,
                        bpy.types.NodeTreeInterfaceSocketInt,
                        given({"subtype": "FACTOR" if low is not None and high is not None else "NONE", **{name: round(limit) for name, limit in ranged(low, high, shown.soft, 1.0).items()}}),
                    )
                case msgspec.inspect.TupleType() if RecordGroup.vector(shape):
                    unit = Unit.of(annotation, shape)
                    if len(shape.item_types) == 3 and shown.quantity == Some(Quantity.ANGLE):
                        return socket(bpy.types.NodeSocketRotation, bpy.types.NodeTreeInterfaceSocketRotation, given())
                    return socket(
                        bpy.types.NodeSocketVector,
                        bpy.types.NodeTreeInterfaceSocketVector,
                        unit.bounds(shape).bind(
                            lambda bounds: given({"dimensions": len(shape.item_types), "subtype": refused.get(unit.vector, unit.vector), **ranged(*bounds, shown.soft, unit.scale)})
                        ),
                    )
                case msgspec.inspect.TupleType():
                    return panel(
                        path,
                        title,
                        description,
                        Nothing,
                        accumulated(
                            entry(
                                (*path, str(index)),
                                component,
                                msgspec.inspect.type_info(component.type),
                                lambda: Some(component.default) if component.default is not msgspec.NODEFAULT else Nothing,
                                f"{title} {index}",
                                description,
                            )
                            for index, component in enumerate(RecordGroup.components(msgspec.structs.replace(field, default=default().default_value(msgspec.NODEFAULT))), 1)
                        ).map(lambda parts: faced(tuple(itertools.chain.from_iterable(parts)))),
                    )
                case msgspec.inspect.BoolType():
                    return socket(bpy.types.NodeSocketBool, bpy.types.NodeTreeInterfaceSocketBool, given())
                case msgspec.inspect.StructType(cls=record) if (typing.get_origin(record) or record) is Rgb:
                    return socket(bpy.types.NodeSocketColor, bpy.types.NodeTreeInterfaceSocketColor, given())
                case msgspec.inspect.StrType():
                    return socket(bpy.types.NodeSocketString, bpy.types.NodeTreeInterfaceSocketString, given())
                case msgspec.inspect.EnumType():
                    return socket(bpy.types.NodeSocketMenu, bpy.types.NodeTreeInterfaceSocketMenu, given())
                case msgspec.inspect.FrozenSetType(item_type=msgspec.inspect.EnumType(cls=choices)) | msgspec.inspect.SetType(item_type=msgspec.inspect.EnumType(cls=choices)):
                    selected = default()
                    return accumulated(
                        socket(
                            bpy.types.NodeSocketBool,
                            bpy.types.NodeTreeInterfaceSocketBool,
                            Ok(selected.map(lambda values: {"default_value": member in values}).default_value({})),  # type: ignore[operator]  # ty: ignore[unsupported-operator]
                            item_key(*path, member.name),
                            member.label if isinstance(member, Choice) else member.name,
                        )
                        for member in choices
                    ).map(lambda parts: tuple(itertools.chain.from_iterable(parts)))
                case msgspec.inspect.StructType(cls=record) if record is Ramp:
                    return Ok(())
                case msgspec.inspect.StructType(cls=record, fields=fields) if issubclass(typing.get_origin(record) or record, Gated):
                    inner = next(item for item in fields if item.name == Gated.value.__name__)  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
                    value_field = next(item for item in msgspec.structs.fields(record) if item.name == inner.name)
                    selected_annotation = field.type if markers(field.type, Presentation) else value_field.type

                    gate = default()
                    held = gate.map(lambda value: value.value)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    content = inner.type
                    while isinstance(content, msgspec.inspect.Metadata):
                        content = content.type
                    match content:
                        case msgspec.inspect.StructType(cls=inner_record) as nested if (typing.get_origin(inner_record) or inner_record) not in {Rgb, Ramp, Gated}:
                            face = walk((*path, inner.name), nested, held)
                        case other:
                            face = entry((*path, inner.name), value_field, other, lambda: held, title, description, Some(selected_annotation)).map(faced)
                    return panel(path, title, description, Some(gate.map(lambda value: value.enabled).default_value(False)), face)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                case msgspec.inspect.StructType(cls=record) as nested:
                    return panel(path, title, Option.of_optional(record.__doc__).default_value(""), Nothing, walk(path, nested, default()))
                case msgspec.inspect.UnionType(types=cases):
                    selected = default()

                    menu = socket(bpy.types.NodeSocketMenu, bpy.types.NodeTreeInterfaceSocketMenu, Ok(selected.map(lambda value: {"default_value": type(value).title}).default_value({})))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

                    def branch(case: msgspec.inspect.Type) -> BlenderResult[tuple[Socket | InterfacePanel, ...]]:
                        match case:
                            case msgspec.inspect.StructType(cls=record) as nested if issubclass(record, Case):
                                chosen = selected.filter(lambda value: isinstance(value, record))
                                return panel((*path, record), record.title, Option.of_optional(record.__doc__).default_value(""), Nothing, walk((*path, record), nested, chosen))
                        raise TypeError(f"{case!r} is not a tagged-union case")

                    return accumulated((menu, *map(branch, cases))).map(lambda parts: tuple(itertools.chain.from_iterable(parts)))
                case msgspec.inspect.VarTupleType(item_type=msgspec.inspect.StructType(cls=record)) if (typing.get_origin(record) or record) is Knot:
                    return Ok(())
                case msgspec.inspect.VarTupleType(item_type=msgspec.inspect.StructType()):
                    return panel(path, title, description, Nothing, Ok(cls()))
            raise TypeError(f"{shape!r} has no native interface socket")

        match msgspec.inspect.type_info(kind):  # type: ignore[exhaustive-match]
            case msgspec.inspect.Metadata(type=msgspec.inspect.StructType() as record):
                return walk((), record, Nothing)
            case msgspec.inspect.StructType() as record:
                return walk((), record, Nothing)
        raise TypeError(f"{kind!r} is not a record declaration")


class InterfacePanel(Interface, frozen=True, kw_only=True):
    """Interface panel holding its own sockets and panels, with an optional enable toggle."""

    key: Key
    name: str
    description: str = ""
    closed: bool = False
    toggle: Option[bool] = Nothing


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


class MenuItem(msgspec.Struct, frozen=True, kw_only=True):
    """Menu Switch item."""

    name: str
    description: str = ""


class MenuItems(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered items of a Menu Switch."""

    items: tuple[MenuItem, ...]

    @classmethod
    def of(cls, shape: msgspec.inspect.Type, description: str = "") -> Self:
        """Constructs native node-menu items from inspected choice or tagged-union metadata."""
        match shape:
            case msgspec.inspect.Metadata(type=inner, extra_json_schema=schema):
                return cls.of(inner, Option.of_optional(schema).default_value({}).get("description", description))
            case msgspec.inspect.EnumType(cls=choices):
                return cls(items=tuple(MenuItem(name=member.label if isinstance(member, Choice) else member.name, description=description) for member in choices))
            case msgspec.inspect.UnionType(types=cases):

                def item(case: msgspec.inspect.Type) -> MenuItem:
                    match case:
                        case msgspec.inspect.StructType(cls=record) if issubclass(record, Case):
                            return MenuItem(name=record.title, description=Option.of_optional(record.__doc__).default_value(""))
                    raise TypeError(f"{case!r} is not a tagged-union case")

                return cls(items=tuple(map(item, cases)))
            case unsupported:
                raise TypeError(f"{unsupported!r} holds no choice menu items")


class FileItem(msgspec.Struct, frozen=True, kw_only=True):
    """File Output item with its socket type and format members."""

    name: str
    kind: Literal["FLOAT", "VECTOR", "RGBA"]
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

    def __or__(self, other: "Fragment") -> "Fragment":
        """Joins two fragments, a key both hold keeping one node."""
        return Fragment(nodes=self.nodes | other.nodes, links=tuple(dict.fromkeys((*self.links, *other.links))))

    @staticmethod
    def joined(*parts: Part) -> BlenderResult["Fragment"]:
        """Joins independent constructions into one fragment.

        Args:
            *parts: Built fragments, or built pairs whose end the join drops.

        Returns:
            `Ok` of every part's fragment joined; `Error` of every part's faults, accumulated.
        """
        return accumulated(part.map(lambda built: built if isinstance(built, Fragment) else built[0]) for part in parts).map(lambda fragments: functools.reduce(or_, fragments, Fragment()))


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


class FrameSpace(msgspec.Struct, frozen=True):
    """Camera frame positions of a source: shares of the displayed greater side, frame fractions, that side in pixels, per-axis pixels, and the Map UV placement, each the built pair of its node."""

    point: tuple[Fragment, End]
    extent: tuple[Fragment, End]
    greater: tuple[Fragment, End]
    fraction: tuple[Fragment, End]
    scale: tuple[Fragment, End]
    placing: tuple[Fragment, End]
    margin: tuple[Fragment, End]


class PixelGrid(msgspec.Struct, frozen=True):
    """Data-window UV centres and increments, displayed pixel centres, and the single-value window offset, each with its native graph."""

    base: tuple[Fragment, End]
    inverse: tuple[Fragment, End]
    point: tuple[Fragment, End]
    offset: tuple[Fragment, End]

    def at(self, key: Key, offset: Operand, scale: Operand) -> BlenderResult[tuple[Fragment, End]]:
        """Builds the Map UV coordinate `offset ⊙ scale + base` of a pixel offset.

        Args:
            key: Key of the result node.
            offset: Operand of the offset in pixels.
            scale: Operand of the grid's `inverse` or a signed multiple of it.

        Returns:
            `Ok` of the fragment and its `Vector` end; `Error` of the faults `node` answers.
        """
        return vector(key, "MULTIPLY_ADD", offset, scale, self.base)


class Configured(msgspec.Struct, frozen=True, kw_only=True):
    """Tree members differing from the graph."""

    members: dict[Descriptor, Value]


class Withdrawn(msgspec.Struct, frozen=True, kw_only=True):
    """Stored interface item the interface no longer declares as it stands."""

    key: Key


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Interface item to create."""

    key: Key


class Restated(msgspec.Struct, frozen=True, kw_only=True):
    """Interface item's differing parts and place."""

    key: Key
    kind: Option[type[bpy.types.NodeSocket]] = Nothing
    name: Option[str] = Nothing
    description: Option[str] = Nothing
    closed: Option[bool] = Nothing
    toggle: Option[bool] = Nothing
    place: Option[tuple[Option[Key], int]] = Nothing
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)


class Removed(msgspec.Struct, frozen=True, kw_only=True):
    """Marked node no key holds, named by its node name."""

    name: str


class Added(msgspec.Struct, frozen=True, kw_only=True):
    """Node to create for a key."""

    key: Key
    kind: type[bpy.types.Node]


class Altered(msgspec.Struct, frozen=True, kw_only=True):
    """Node's differing desired parts."""

    key: Key
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)
    content: Option[Content] = Nothing
    parent: Option[Option[Key]] = Nothing


class Valued(msgspec.Struct, frozen=True, kw_only=True):
    """Node's differing socket values by address."""

    key: Key
    inputs: dict[str, Value] = msgspec.field(default_factory=dict)
    outputs: dict[str, Value] = msgspec.field(default_factory=dict)


class Disconnected(msgspec.Struct, frozen=True, kw_only=True):
    """Owned link the graph lacks."""

    link: bpy.types.NodeLink


class Connected(msgspec.Struct, frozen=True, kw_only=True):
    """Desired link the tree lacks."""

    link: Link


class Reordered(msgspec.Struct, frozen=True, kw_only=True):
    """Desired top-to-bottom order of owned connections at one input."""

    target: End
    sources: tuple[End, ...]


class Changes(msgspec.Struct, frozen=True, kw_only=True):
    """Reconcile rows in apply phases, `Changes()` for a current tree."""

    structure: tuple[Structural, ...] = ()
    alterations: tuple[Alteration, ...] = ()
    links: tuple[Connected | Reordered, ...] = ()
    values: tuple[Valued, ...] = ()
    restatements: tuple[Restated, ...] = ()


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

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the ownership pairs of every nested group.

        Args:
            identity: Extension whose marks the groups hold.

        Returns:
            `Ok` of the pairs in nesting order, else `Error` of every fault their builds answer.
        """
        return Ok(tuple(group.ownership for group in self.nested))

    def changes(self, tree: bpy.types.NodeTree, identity: Identity, interface: Interface = Interface()) -> BlenderResult[Changes]:  # ruff: ignore[function-call-in-default-argument]
        """Reads a tree against the graph and an interface, writing nothing.

        Args:
            tree: Owned tree to read.
            identity: Extension whose marks find owned nodes.
            interface: Interface the tree declares, empty for an embedded tree.

        Returns:
            `Ok` of the rows an apply writes, `Changes()` for a current tree; `Error` of `absent` or `choice` for a content member the node's class lacks, or of `register` for a refused stored-key name.
        """
        placed, entries = self.placed, tuple(interface.walked())
        marked = tuple((node, mark.key) for node in tree.nodes for mark in Mark.read(identity, node).to_list())
        held: dict[Key, bpy.types.Node] = {}
        for node, key in marked:
            if key in placed and isinstance(node, placed[key][0].kind):
                held.setdefault(key, node)
        owners = {node: key for key, node in held.items()}

        def altered(key: Key, node: Node, parent: Option[Key]) -> BlenderResult[Option[Altered]]:
            def drawn(live: bpy.types.Node) -> BlenderResult[Altered]:
                framed = parent.bind(lambda frame: Option.of_optional(held.get(frame))).to_optional() == live.parent
                return (
                    node.content
                    .map(lambda wanted: _current(wanted, live, identity).map(lambda now: Nothing if now else Some(wanted)))
                    .default_value(Ok(Nothing))
                    .map(lambda content: Altered(key=key, members=_differing(node.members, live), content=content, parent=Nothing if framed else Some(parent)))
                )

            fresh = Altered(key=key, members=node.members, content=node.content, parent=parent.map(Some))
            return (drawn(held[key]) if key in held else Ok(fresh)).map(lambda row: Some(row).filter(lambda row: row != Altered(key=key)))

        def interfaced(slot: str) -> tuple[tuple[Withdrawn, ...], tuple[Declared, ...], tuple[Restated, ...]]:
            stored = _items(tree, slot)
            live = {item.identifier: item for item in tree.interface.items_tree if isinstance(item, (bpy.types.NodeTreeInterfaceSocket, bpy.types.NodeTreeInterfacePanel))}  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            inverse = {identifier: key for key, identifier in stored.items()}
            kept = {
                key: item
                for key, entry, _, _ in entries
                for identifier in (stored.get(key),)
                if identifier is not None
                for item in Option.of_optional(live.get(identifier)).to_list()
                if _matched(entry, item)
            }
            fresh = frozenset(key for key, *_ in entries if key not in kept)
            wanted = frozenset(key for key, *_ in entries)
            leaving = tuple(key for identifier in reversed(live) for key in Option.of_optional(inverse.get(identifier)).to_list() if key not in wanted or key in fresh)
            return (
                tuple(Withdrawn(key=key) for key in leaving),
                tuple(Declared(key=key) for key, *_ in entries if key in fresh),
                tuple(
                    row
                    for key, entry, parent, position in entries
                    for row in _restated(key, entry, (parent, position), Option.of_optional(kept.get(key)), inverse, moved=any(frame in fresh for frame in parent.to_list())).to_list()
                ),
            )

        def linked(slot: str, rows: tuple[Option[Altered], ...]) -> tuple[tuple[Disconnected, ...], tuple[Connected | Reordered, ...], dict[Key, dict[str, tuple[str, type[bpy.types.NodeSocket]]]]]:
            def addressed(endpoint: End) -> End:
                return msgspec.structs.replace(endpoint, socket=_keyed(held[endpoint.node], slot).get(endpoint.socket, endpoint.socket)) if endpoint.node in held else endpoint

            def surviving(group: bpy.types.NodeTree, interface: Interface) -> dict[str, tuple[str, type[bpy.types.NodeSocket]]]:
                live = {item.identifier: item for item in group.interface.items_tree if isinstance(item, (bpy.types.NodeTreeInterfaceSocket, bpy.types.NodeTreeInterfacePanel))}  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                stored = _items(group, slot)
                return {
                    key: (address, entry[0].kind if isinstance(entry, tuple) else bpy.types.NodeSocketBool)
                    for key, address in _addresses(group, slot).items()
                    for desired, entry, _, _ in interface.walked()
                    if key == desired
                    for identifier in (stored.get(key),)
                    if identifier is not None
                    for item in Option.of_optional(live.get(identifier)).to_list()
                    if _matched(entry, item)
                }

            interfaces: dict[Key, dict[str, tuple[str, type[bpy.types.NodeSocket]]]] = {
                key: surviving(tree, interface) for key, live in held.items() if isinstance(live, (bpy.types.NodeGroupInput, bpy.types.NodeGroupOutput))
            }
            for change in (change for option in rows for change in option.to_list()):
                match change.content:
                    case Option(tag="some", some=Nested(group=group)) if change.key in held:
                        target = Option.of_optional(earliest(group.ownership.held(identity)).get(Mark(key=group.key))) if group.single else Nothing
                        interfaces[change.key] = target.map(lambda tree: surviving(tree, group.interface)).default_value({})

            def retained(end: End) -> bool:
                if end.node not in interfaces:
                    return True
                future = interfaces[end.node]
                address = {identifier: key for key, identifier in _keyed(held[end.node], slot).items()}.get(end.socket, end.socket)
                return any(identifier == end.socket and address in {key, identifier} for key, (identifier, _) in future.items())

            sockets = {socket: End(node=key, socket=socket.identifier) for key, live in held.items() for socket in (*live.inputs, *live.outputs)}
            endpoints = {
                link: (sockets[link.from_socket], sockets[link.to_socket])  # ty: ignore[invalid-argument-type]
                for link in tree.links
                if link.from_node in owners and link.to_node in owners
            }
            targets: dict[End, bpy.types.NodeSocket] = {target: link.to_socket for link, (_, target) in endpoints.items()}  # type: ignore[misc]  # ty: ignore[invalid-assignment]
            owned = tuple(link for socket in targets.values() for link in socket.links if link in endpoints)  # type: ignore[union-attr]  # ty: ignore[not-iterable]
            available: dict[tuple[End, End], deque[bpy.types.NodeLink]] = {}
            for link in owned:
                if all(map(retained, endpoints[link])):
                    available.setdefault(endpoints[link], deque()).append(link)
            kept: set[bpy.types.NodeLink] = set()
            missing: deque[Link] = deque()
            desired: dict[End, list[End]] = {}
            for link in self.links:
                desired.setdefault(link.target, []).append(link.source)
                if (candidates := available.get((addressed(link.source), addressed(link.target)))) is not None and candidates:
                    kept.add(candidates.popleft())
                else:
                    missing.appendleft(link)
            disconnected = tuple(Disconnected(link=link) for link in owned if link not in kept)
            connected = tuple(Connected(link=link) for link in missing)
            reordered = tuple(
                Reordered(target=target, sources=tuple(sources))
                for target, sources in desired.items()
                if tuple(endpoints[link][0] for socket in Option.of_optional(targets.get(addressed(target))).to_list() for link in socket.links if link in endpoints)  # type: ignore[union-attr]  # ty: ignore[not-iterable]
                != tuple(map(addressed, sources))
            )
            return disconnected, (*connected, *reordered), interfaces

        def assembled(slot: str, rows: tuple[Option[Altered], ...]) -> Changes:
            (withdrawn, declared, restated), (disconnected, connected, interfaces) = interfaced(slot), linked(slot, rows)
            values = tuple(
                Valued(
                    key=key,
                    inputs=_stale(held[key], held[key].inputs, node.inputs, slot, future := Option.of_optional(interfaces.get(key))),
                    outputs=_stale(held[key], held[key].outputs, node.outputs, slot, future),
                )
                if key in held
                else Valued(key=key, inputs=node.inputs, outputs=node.outputs)
                for key, (node, _) in placed.items()
            )
            return Changes(
                structure=(
                    *(Configured(members=members) for members in (_differing(self.members, tree),) if members),
                    *withdrawn,
                    *declared,
                    *(Removed(name=node.name) for node, _ in marked if node not in owners),
                    *(Added(key=key, kind=node.kind) for key, (node, _) in placed.items() if key not in held),
                ),
                alterations=(*(row for option in rows for row in option.to_list()), *disconnected),
                links=connected,
                values=tuple(row for row in values if row.inputs or row.outputs),
                restatements=restated,
            )

        return identity.name(Name.ATTACHMENT, "interface").bind(lambda slot: accumulated(altered(key, node, parent) for key, (node, parent) in placed.items()).map(lambda rows: assembled(slot, rows)))

    def reconciled(self, tree: bpy.types.NodeTree, identity: Identity, interface: Interface = Interface()) -> BlenderResult[Changes]:  # ruff: ignore[function-call-in-default-argument]
        """Writes the rows `changes` answers, phase by phase.

        Args:
            tree: Owned tree to write.
            identity: Extension whose marks find and mark owned nodes.
            interface: Interface the tree declares, empty for an embedded tree.

        Returns:
            `Ok` of the rows written; `Error` of the faults `changes` answers, or of every `edit`, `absent`, or `choice` fault a phase's writes answer.
        """
        return (
            identity
            .name(Name.ATTACHMENT, "interface")
            .bind(lambda slot: self.changes(tree, identity, interface).map(lambda rows: (slot, rows)))
            .bind(
                lambda carried: functools.reduce(
                    lambda done, phase: done.bind(lambda _: accumulated(self.applied(row, tree, identity, interface, carried[0]) for row in phase).map(lambda _: None)),
                    (
                        tuple(row for row in carried[1].alterations if isinstance(row, Disconnected)),
                        carried[1].structure,
                        tuple(Restated(key=row.key, kind=row.kind) for row in carried[1].restatements if row.kind.is_some()),
                        tuple(row for row in carried[1].alterations if isinstance(row, Altered)),
                        tuple(row for row in carried[1].links if isinstance(row, Connected)),
                        tuple(row for row in carried[1].links if isinstance(row, Reordered)),
                        carried[1].values,
                        tuple(msgspec.structs.replace(row, kind=Nothing) for row in carried[1].restatements),
                    ),
                    Result[None, BlenderFaults].Ok(None),
                ).map(lambda _: carried[1])
            )
        )

    def applied(self, row: TreeChange, tree: bpy.types.NodeTree, identity: Identity, interface: Interface, slot: str) -> BlenderResult[None]:
        """Writes one change row onto a tree.

        Args:
            row: Change row to write.
            tree: Owned tree the row names.
            identity: Extension whose marks find and mark owned nodes.
            interface: Interface the tree declares.
            slot: ID property name of the stored item keys.

        Returns:
            `Ok` once written; `Error` of `edit` for a refused node type or write, of `absent` for an unresolved node, item, or socket, or of `choice` for a refused enum identifier.
        """
        entries = {key: entry for key, entry, _, _ in interface.walked()}
        face: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

        def node(key: Key) -> BlenderResult[bpy.types.Node]:
            return Option.of_optional(next((each for each in tree.nodes if Mark.read(identity, each).map(lambda mark: mark.key == key).default_value(False)), None)).to_result_with(
                lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=key),)
            )

        def item[T: bpy.types.NodeTreeInterfaceItem](key: Key, kind: type[T]) -> BlenderResult[T]:
            found: Option[T] = Option.of_optional(_items(tree, slot).get(key)).bind(
                lambda identifier: Option.of_optional(
                    next(  # type: ignore[arg-type]
                        (
                            each
                            for each in face.items_tree
                            if isinstance(each, (bpy.types.NodeTreeInterfaceSocket, bpy.types.NodeTreeInterfacePanel)) and each.identifier == identifier and isinstance(each, kind)
                        ),
                        None,
                    )
                )
            )
            return found.to_result_with(lambda: (BlenderAbsentFault(owner=repr(tree.interface), key=key),))

        def connected(link: Link) -> BlenderResult[None]:
            def created(sockets: tuple[bpy.types.NodeSocket, bpy.types.NodeSocket]) -> BlenderResult[None]:
                return (
                    catch(lambda: tree.links.new(*sockets, verify_limits=False), exception=RuntimeError)()
                    .map_error(lambda error: (BlenderEditFault(request=repr(tree.links), error=error),))
                    .map(lambda _: None)
                )

            return paired(
                node(link.source.node).bind(lambda live: _socket(live.outputs, _keyed(live, slot).get(link.source.socket, link.source.socket))),
                node(link.target.node).bind(lambda live: _socket(live.inputs, _keyed(live, slot).get(link.target.socket, link.target.socket))),
            ).bind(created)

        def withdrawn(held: bpy.types.NodeTreeInterfaceItem, key: Key) -> None:
            for toggle in _toggle(held).to_list():
                face.remove(toggle)
            face.remove(held)
            tree[slot] = {name: identifier for name, identifier in _items(tree, slot).items() if name != key}

        def declared(key: Key) -> BlenderResult[None]:
            def toggled(panel: bpy.types.NodeTreeInterfacePanel, *, value: bool) -> BlenderResult[None]:
                socket = face.new_socket(panel.name, in_out="INPUT", socket_type=bpy.types.NodeSocketBool.bl_rna.identifier)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                face.move_to_parent(socket, panel, 0)
                return accumulated((written(socket, "is_panel_toggle", True), written(socket, "default_value", value))).map(lambda _: None)

            def made(entry: tuple[Socket, SocketSide] | InterfacePanel) -> BlenderResult[InterfaceItem]:
                match entry:
                    case tuple() as pair:
                        socket, side = pair
                        return Ok(face.new_socket(socket.name, description=socket.description, in_out=side, socket_type=socket.kind.bl_rna.identifier))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    case InterfacePanel(name=name, description=description, closed=closed, toggle=toggle):
                        panel = face.new_panel(name, description=description, default_closed=closed)
                        return accumulated(toggled(panel, value=value) for value in toggle.to_list()).map(lambda _: panel)

            def stored(created: InterfaceItem) -> None:
                tree[slot] = {**_items(tree, slot), key: created.identifier}

            return made(entries[key]).map(stored)

        def restated(change: Restated) -> BlenderResult[None]:
            def retyped(held: bpy.types.NodeTreeInterfaceItem) -> BlenderResult[bpy.types.NodeTreeInterfaceItem]:
                return change.kind.map(lambda kind: written(held, "socket_type", kind.bl_rna.identifier).bind(lambda _: item(change.key, bpy.types.NodeTreeInterfaceItem))).default_value(Ok(held))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

            def moved(held: bpy.types.NodeTreeInterfaceItem, parent: Option[Key], position: int) -> BlenderResult[None]:
                return (
                    parent.map(lambda key: item(key, bpy.types.NodeTreeInterfacePanel).map(Some)).default_value(Ok(Nothing)).map(lambda panel: face.move_to_parent(held, panel.to_optional(), position))
                )

            def rest(held: bpy.types.NodeTreeInterfaceItem) -> BlenderResult[None]:
                return accumulated((
                    *(written(held, "name", name) for name in change.name.to_list()),
                    *(written(held, "description", text) for text in change.description.to_list()),
                    *(written(held, "default_closed", closed) for closed in change.closed.to_list()),
                    *(written(toggle, "default_value", value) for value in change.toggle.to_list() for toggle in _toggle(held).to_list()),
                    *(moved(held, *place) for place in change.place.to_list()),
                    _assigned(change.members, held),
                )).map(lambda _: None)

            return item(change.key, bpy.types.NodeTreeInterfaceItem).bind(retyped).bind(rest)

        def altered(change: Altered) -> BlenderResult[None]:
            def framed(live: bpy.types.Node, frame: Option[Key]) -> BlenderResult[None]:
                def parented(found: Option[bpy.types.Node]) -> None:
                    live.parent = found.to_optional()

                return frame.map(lambda key: node(key).map(Some)).default_value(Ok(Nothing)).map(parented)

            return node(change.key).bind(
                lambda live: (
                    change.content
                    .map(lambda content: stored(content, live, identity))
                    .default_value(Ok(None))
                    .bind(lambda _: _assigned(change.members, live))
                    .bind(lambda _: change.parent.map(lambda frame: framed(live, frame)).default_value(Ok(None)))
                )
            )

        def reordered(change: Reordered) -> BlenderResult[None]:
            def ordered(target: bpy.types.NodeSocket, sources: tuple[bpy.types.NodeSocket, ...]) -> BlenderResult[None]:
                marked = {live for live in tree.nodes if Mark.read(identity, live).is_some()}
                current = [link for link in target.links if link.from_node in marked]  # type: ignore[union-attr]  # ty: ignore[not-iterable]
                available: dict[bpy.types.NodeSocket, deque[bpy.types.NodeLink]] = {}
                for link in current:
                    available.setdefault(link.from_socket, deque()).append(link)  # type: ignore[arg-type]  # ty: ignore[no-matching-overload]

                def swapped(desired: tuple[bpy.types.NodeLink, ...]) -> None:
                    positions = {link: index for index, link in enumerate(current)}
                    for index, wanted in enumerate(desired):
                        position = positions[wanted]
                        if (held := current[index]) != wanted:
                            held.swap_multi_input_sort_id(wanted)
                            current[index], current[position] = wanted, held
                            positions[wanted], positions[held] = index, position

                return accumulated(
                    Option.of_optional(available.get(source)).filter(bool).map(lambda links: links.popleft()).to_result_with(lambda: (BlenderAbsentFault(owner=repr(target), key=repr(source)),))
                    for source in sources
                ).map(swapped)

            return paired(
                node(change.target.node).bind(lambda live: _socket(live.inputs, _keyed(live, slot).get(change.target.socket, change.target.socket))),
                accumulated(node(source.node).bind(lambda live: _socket(live.outputs, _keyed(live, slot).get(source.socket, source.socket))) for source in change.sources),
            ).bind(lambda pair: ordered(*pair))

        match row:
            case Configured(members=members):
                return _assigned(members, tree)
            case Withdrawn(key=key):
                return item(key, bpy.types.NodeTreeInterfaceItem).map(lambda held: withdrawn(held, key))
            case Declared(key=key):
                return declared(key)
            case Removed(name=name):
                return Option.of_optional(tree.nodes.get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=name),)).map(tree.nodes.remove)
            case Added(key=key, kind=kind):
                match catch(lambda: tree.nodes.new(kind.bl_rna.identifier), exception=RuntimeError)():  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    case Result(tag="error", error=error):
                        return Error((BlenderEditFault(request=repr(tree.nodes), error=error),))
                    case Result(ok=made):
                        return Mark(key=key).write(identity, made)
            case Altered():
                return altered(row)
            case Disconnected(link=link):
                tree.links.remove(link)
                return Ok(None)
            case Connected(link=link):
                return connected(link)
            case Reordered():
                return reordered(row)
            case Valued(key=key, inputs=inputs, outputs=outputs):
                return node(key).bind(lambda live: paired(valued(live, live.inputs, inputs, slot), valued(live, live.outputs, outputs, slot))).map(lambda _: None)
            case Restated():
                return restated(row)


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
        """Builds a group around its output operands, adding its output node and, for an interface declaring inputs, its input node.

        Args:
            key: Ownership key of the group.
            name: Name the group takes at creation.
            interface: Interface the group declares.
            outputs: Operand per output item key, each fragment joining the graph.
            kind: Tree class of the group.

        Returns:
            `Ok` of the group; `Error` of the faults `node` answers for the output node.
        """
        entered = {INPUTS: Node(kind=bpy.types.NodeGroupInput)} if interface.inputs or interface.panels else {}
        return node(OUTPUTS, bpy.types.NodeGroupOutput, outputs, {"is_active_output": True}).map(
            lambda body: cls(key=key, name=name, interface=interface, graph=Graph(kind=kind, nodes={**entered, **body.nodes}, links=body.links))
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the group's ownership pairs and every nested group's.

        Args:
            identity: Extension whose marks the groups hold.

        Returns:
            `Ok` of its own pairs, then its graph's, else `Error` of every fault either answers.
        """
        return Ok((self.ownership, self.graph))

    def made(self, identity: Identity, *, of: Option[bpy.types.ID] = Nothing) -> BlenderResult[bpy.types.NodeTree]:
        """Reuses or builds the group's tree and reconciles it.

        Args:
            identity: Extension whose marks find and mark the group and its nodes.
            of: Data-block the group serves, so a single group of one scene stays that scene's.

        Returns:
            `Ok` of the group's tree; `Error` of the faults its reconcile answers.
        """
        return self.ownership.made(identity, lambda: Ok(bpy.data.node_groups.new(self.name, self.graph.kind.bl_rna.identifier)), of=of).bind(  # type: ignore[arg-type,attr-defined]  # ty: ignore[invalid-argument-type,unresolved-attribute]
            lambda tree: self.graph.reconciled(tree, identity, self.interface).map(lambda _: tree)
        )


class Route(msgspec.Struct, frozen=True, kw_only=True):
    """Path to a node output, or an output node's linked input, through a holder's groups."""

    key: Key
    groups: tuple[str, ...]
    node: str
    socket: str


class Routed[H: bpy.types.NodeTree | bpy.types.Material | bpy.types.World | bpy.types.Light](msgspec.Struct, frozen=True, kw_only=True):
    """Copied holder and its carried output or retained shader output node and input."""

    holder: H
    node: str
    socket: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def fingerprinted(owner: bpy.types.NodeTree | bpy.types.Node) -> BlenderResult[Option[ContentHash]]:
    """Hashes observable native content of a tree or one node's upstream graph.

    Args:
        owner: Complete tree, or target node whose incoming connections select its upstream closure.

    Returns:
        The digest of persistent settings, socket defaults, ordered links, nested
        groups, and image content when fully observable. Unobservable image
        dependencies return `Nothing`; native capture faults remain errors.
    """
    tree: bpy.types.NodeTree = owner if isinstance(owner, bpy.types.NodeTree) else owner.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    pending = list(owner.nodes) if isinstance(owner, bpy.types.NodeTree) else [owner]
    seen: set[bpy.types.Node] = set()
    while pending:
        if (current := (pending.pop())) not in seen:
            seen.add(current)
            pending.extend(link.from_node for socket in current.inputs for link in socket.links)  # type: ignore[misc,union-attr]  # ty: ignore[invalid-argument-type,not-iterable]
    nodes = tuple(sorted(seen, key=lambda item: item.name))
    settings = tuple(Descriptor(root=type(tree), owner="", prop=prop) for prop in Snapshot.members(tree.bl_rna) if prop.identifier not in {"nodes", "links"})  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    def captured(struct: bpy.types.bpy_struct, selection: Option[tuple[Descriptor, ...]] = Nothing) -> BlenderResult[tuple[Snapshot, tuple[Option[ContentHash], ...]]]:
        def referenced(snapshot: Snapshot) -> BlenderResult[tuple[Snapshot, tuple[Option[ContentHash], ...]]]:
            blocks = dict.fromkeys(
                block
                for at, held in snapshot.tree.owners.items()
                for name, value in held.values.items()
                if isinstance(value, IdName)
                for block in (struct.path_resolve(f"{at}.{name}".removeprefix(".")),)
                if isinstance(block, bpy.types.NodeTree | bpy.types.Image)
            )
            return accumulated(fingerprinted(block) if isinstance(block, bpy.types.NodeTree) else images.fingerprinted(block) for block in blocks).map(lambda hashes: (snapshot, hashes))

        return Snapshot.capture(struct, selection).bind(referenced)

    def linked(link: bpy.types.NodeLink) -> BlenderResult[ContentHash]:
        endpoints = Link(source=End(node=link.from_node.name, socket=link.from_socket.identifier), target=End(node=link.to_node.name, socket=link.to_socket.identifier))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        members = tuple(Descriptor(root=type(link), owner="", prop=prop) for prop in link.bl_rna.properties if not prop.is_readonly)
        return Snapshot.capture(link, Some(members)).map(lambda snapshot: content_hash(Link, endpoints, snapshot))

    return paired(
        accumulated(itertools.starmap(captured, ((tree, Some(settings)), *((item, Nothing) for item in nodes)))),
        accumulated(linked(link) for item in nodes for socket in item.inputs for link in socket.links),  # type: ignore[union-attr]  # ty: ignore[not-iterable]
    ).map(
        lambda parts: functools.reduce(
            lambda held, part: held.map2(lambda prior, current: content_hash(type(owner), prior, current), part),
            (key for _, hashes in parts[0] for key in hashes),
            Some(content_hash(type(owner), *(snapshot for snapshot, _ in parts[0]), *parts[1])),
        )
    )


def ended(key: Key, socket: str) -> Callable[[Fragment], tuple[Fragment, End]]:
    """Pairs a fragment with one output of its node, the operand a later constructor nests."""
    return lambda built: (built, End(node=key, socket=socket))


def item_key(*parts: Key | int | type[Case]) -> Key:
    """Joins a row key path into one interface item key, a case class entering as its lowered name and an item index as its number."""
    return "_".join(part.__name__.lower() if isinstance(part, type) else str(part) for part in parts)


def under[I: Interface](prefix: Key, face: I) -> I:
    """Answers an interface with every socket and panel key joined under a prefix through `item_key`, the form a group nested in another's panel takes."""
    return msgspec.structs.replace(
        face,
        outputs=tuple(msgspec.structs.replace(socket, key=item_key(prefix, socket.key)) for socket in face.outputs),
        inputs=tuple(msgspec.structs.replace(socket, key=item_key(prefix, socket.key)) for socket in face.inputs),
        panels=tuple(msgspec.structs.replace(under(prefix, panel), key=item_key(prefix, panel.key)) for panel in face.panels),
    )


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
                return held | Fragment(links=(Link(source=source, target=target),)), {}
            case End():
                return Fragment(links=(Link(source=operand, target=target),)), {}
            case _:
                return Fragment(), {address: operand}

    placements = tuple(itertools.starmap(placed, inputs.items()))
    return accumulated(Descriptor.resolve(kind, identifier, bpy.types.Property) for identifier in members).map(
        lambda resolved: functools.reduce(
            or_,
            (held for held, _ in placements),
            Fragment(
                nodes={key: Node(kind=kind, members=dict(zip(resolved, members.values())), inputs={address: value for _, values in placements for address, value in values.items()}, content=content)}
            ),
        )
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
        key: Key of the sole Menu Switch; secondary switches append output and case keys.
        selector: Operand of the menu input.
        items: Ordered native menu items.
        outputs: Native socket kind and nonempty case operands per output key, in construction order.

    Returns:
        Selected outputs sharing their complete fragment, or every independent construction fault.
    """

    def output(index: int, name: Key, kind: str, cases: Mapping[str, Operand]) -> BlenderResult[tuple[Fragment, End]]:
        if index == 0:
            return menu(key, kind, selector, items, cases)
        choices = iter(cases.items())
        first_name, first = next(choices)

        def chosen(held: BlenderResult[tuple[Fragment, End]], case: tuple[str, Operand]) -> BlenderResult[tuple[Fragment, End]]:
            case_name, operand = case
            return held.bind(lambda off: switched(item_key(key, name, case_name), kind, End(node=key, socket=case_name), off, operand))

        return functools.reduce(chosen, choices, switched(item_key(key, name, first_name), kind, End(node=key, socket=first_name), first, first))

    return (
        accumulated(output(index, name, kind, cases) for index, (name, (kind, cases)) in enumerate(outputs.items()))
        .map(lambda built: (functools.reduce(or_, (fragment for fragment, _ in built), Fragment()), built))
        .map(lambda joined: {name: (joined[0], end) for name, (_, end) in zip(outputs, joined[1])})
    )


def nested(key: Key, group: "NodeGroup", inputs: Mapping[Key, Operand], output: Key) -> BlenderResult[tuple[Fragment, End]]:
    """Builds a group node over a nested group, its inputs and output addressed by item key.

    Args:
        key: Key of the group node.
        group: Owned group the node points at.
        inputs: Operand per input item key of the group's interface.
        output: Output item key the answer ends at.

    Returns:
        `Ok` of the fragment and its output end; `Error` of the faults `node` answers.

    """

    def holder(tree: type[GraphTree]) -> type[bpy.types.Node]:
        return bpy.types.CompositorNodeGroup if tree is bpy.types.CompositorNodeTree else bpy.types.ShaderNodeGroup if tree is bpy.types.ShaderNodeTree else bpy.types.GeometryNodeGroup

    return node(key, holder(group.graph.kind), inputs, {}, Some(Nested(group=group))).map(ended(key, output))


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


def masked(key: Key, kind: Literal["FLOAT", "VECTOR", "RGBA"], gate: Operand, off: Operand, on: Operand) -> BlenderResult[tuple[Fragment, End]]:
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
            .bind(lambda branches: accumulated(chosen(item_key(key, channel), inverse, ended(low_key, channel)(branches[0]), ended(high_key, channel)(branches[1])) for channel in channels))
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


def mixed(key: Key, kind: Literal["FLOAT", "VECTOR", "RGBA"], factor: Operand, a: Operand, b: Operand, blend: str = "MIX") -> BlenderResult[tuple[Fragment, End]]:
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
def summed(prefix: Key, terms: tuple[tuple[Fragment, End], *tuple[tuple[Fragment, End], ...]], *, kind: Literal["FLOAT", "VECTOR", "RGBA"] = "RGBA") -> BlenderResult[tuple[Fragment, End]]: ...
@overload
def summed(prefix: Key, terms: tuple[Operand, ...], *, kind: Literal["FLOAT", "VECTOR", "RGBA"] = "RGBA") -> BlenderResult[Operand]: ...
def summed(prefix: Key, terms: tuple[Operand, ...], *, kind: Literal["FLOAT", "VECTOR", "RGBA"] = "RGBA") -> BlenderResult[Operand]:
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


def origin(key: Key, seed: Operand, noise: int, pattern: Operand = 0, instance: Operand = 0) -> BlenderResult[tuple[Fragment, End]]:
    """Builds the decorrelation origin of one noise.

    Args:
        key: Key of the origin's scale node, inner nodes keyed from it.
        seed: Operand of the record's `Seed`.
        noise: Member of the effect's closed noise set.
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


def framed(key: Key, source: Operand, aspect: Operand, scale: Operand = 1.0) -> BlenderResult[FrameSpace]:
    """Builds the frame positions of the camera frame centred in a source, over its displayed frame.

    Args:
        key: Prefix of the build's node keys.
        source: Operand of the image whose pixels the build reads.
        aspect: Operand of the pixel aspect `pixel_aspect_x / pixel_aspect_y`.
        scale: Operand of the camera frame's share of each side of the source, 1 for a source holding the camera frame alone.

    Returns:
        `Ok` of the source's `FrameSpace`, each field carrying the nodes upstream of it; `Error` of the faults `node` answers.
    """

    def placed(
        axes: tuple[Fragment, End], grid: PixelGrid, camera: tuple[tuple[Fragment, End], tuple[Fragment, End]], sizes: tuple[tuple[Fragment, End], tuple[Fragment, End]]
    ) -> BlenderResult[FrameSpace]:
        pixels, offset = camera
        sized, displayed = sizes
        return (
            node(f"{key}_sides", bpy.types.ShaderNodeSeparateXYZ, {"Vector": sized}, {})
            .bind(lambda sides: math(f"{key}_greater", "MAXIMUM", ended(f"{key}_sides", "X")(sides), ended(f"{key}_sides", "Y")(sides)))
            .bind(
                lambda greater: paired(
                    paired(vector(f"{key}_point", "DIVIDE", displayed, greater), vector(f"{key}_extent", "DIVIDE", sized, greater)),
                    paired(
                        vector(f"{key}_scale", "DIVIDE", greater, axes),
                        paired(
                            vector(f"{key}_placing", "MULTIPLY", pixels, grid.inverse),
                            vector(f"{key}_data_offset", "SUBTRACT", offset, grid.offset)
                            .bind(lambda shifted: vector(f"{key}_uv_offset", "MULTIPLY", shifted, grid.inverse))
                            .bind(lambda normalized: vector(f"{key}_uv_margin", "ADD", normalized, (0.0, 0.0, 1.0))),
                        ),
                    ),
                ).bind(lambda parts: vector(f"{key}_fraction", "DIVIDE", *parts[0]).map(lambda fraction: FrameSpace(*parts[0], greater, fraction, parts[1][0], *parts[1][1])))
            )
        )

    def measured(style: tuple[tuple[Fragment, End], tuple[Fragment, End]], coordinates: tuple[PixelGrid, tuple[Fragment, End]]) -> BlenderResult[FrameSpace]:
        axes, margin = style
        grid, dimensions = coordinates
        return paired(vector(f"{key}_camera_pixels", "SCALE", dimensions, scale), vector(f"{key}_camera_offset", "SCALE", dimensions, margin)).bind(
            lambda camera: paired(
                vector(f"{key}_sized", "MULTIPLY", camera[0], axes),
                vector(f"{key}_shifted", "SUBTRACT", grid.point, camera[1]).bind(lambda shifted: vector(f"{key}_displayed", "MULTIPLY", shifted, axes)),
            ).bind(lambda sizes: placed(axes, grid, camera, sizes))
        )

    return paired(
        paired(node(f"{key}_aspect", bpy.types.ShaderNodeCombineXYZ, {"X": aspect, "Y": 1.0}, {}).map(ended(f"{key}_aspect", "Vector")), math(f"{key}_margin", "MULTIPLY_ADD", scale, -0.5, 0.5)),
        _coordinates(key, source),
    ).bind(lambda parts: measured(*parts))


def gridded(key: Key, image: Operand) -> BlenderResult[PixelGrid]:
    """Builds data-window UVs, pixel centres, and the single-value window offset.

    Args:
        key: Prefix of the grid's node keys.
        image: Operand of the image whose pixels the grid samples.

    Returns:
        `Ok` of the grid, each field carrying the nodes upstream of it; `Error` of the faults `node` answers.
    """
    return _coordinates(key, image).map(lambda parts: parts[0])


def sampled(key: Key, frame: FrameSpace, fraction: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Maps a camera frame fraction onto the Map UV coordinate of the source holding that frame, with coverage 1 in Z.

    Args:
        key: Key of the result node.
        frame: Frame space of the source the coordinate samples.
        fraction: Operand of a position as fractions of the camera frame from its bottom-left corner.

    Returns:
        `Ok` of the fragment and its `Vector` end; `Error` of the faults `node` answers.
    """
    return vector(key, "MULTIPLY_ADD", fraction, frame.placing, frame.margin)


def pixels(key: Key, frame: FrameSpace, length: Operand, kind: Literal["FLOAT", "VECTOR"] = "FLOAT") -> BlenderResult[tuple[Fragment, End]]:
    """Converts a `FRAME_GREATER` length or offset into stored pixels of the frame space `framed` built.

    Args:
        key: Key of the result node, its axis split keyed `<key>_axes`.
        frame: Frame space of the source the length measures.
        length: Operand of a length, or of an offset or 2D size in displayed units.
        kind: `FLOAT` for a length in stored X pixels, `VECTOR` for per-axis stored pixels.

    Returns:
        `Ok` of the fragment and its pixel end; `Error` of the faults `node` answers.
    """
    match kind:
        case "VECTOR":
            return vector(key, "MULTIPLY", length, frame.scale)
        case "FLOAT":
            return node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.scale}, {}).bind(lambda axes: math(key, "MULTIPLY", length, ended(f"{key}_axes", "X")(axes)))


def reconciling[T: bpy.types.ID](kind: type[T], write: Callable[[Handle[T], Identity], BlenderResult[Option[str]]]) -> Callable[[Identity], AbstractContextManager[Changed]]:
    """Builds the attachment update body deferring one reconcile of the holder its data-block names.

    Args:
        kind: Holder class the write resolves: a material, world, light, scene, or node group.
        write: Module-level write that resolves the handle, reads the owner's record, derives its graph, and reconciles inside the deferred call.

    Returns:
        The callback resource builder Attachment enters before compiling its native group.
    """

    @contextmanager
    def built(identity: Identity) -> Generator[Changed]:
        with identity.reports.deferred(write) as deferred:

            def changed(owner: bpy.types.bpy_struct, _context: bpy.types.Context, _property: bpy.types.Property) -> None:
                for holder in [holder for holder in _holder(owner.id_data).to_list() if isinstance(holder, kind)]:  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    deferred(Handle.of(holder), identity)

            yield changed

    return built


@contextmanager
def routed[H: bpy.types.NodeTree | bpy.types.Material | bpy.types.World | bpy.types.Light](identity: Identity, holder: H, route: Route) -> Generator[BlenderResult[Routed[H]]]:
    """Scopes copied preview resources, exposing an output or retaining a shader output node whole.

    Args:
        identity: Extension whose marks the copies hold.
        holder: Group, material, world, or light whose tree nests the node.
        route: Copies' key, group node names, target node name, and socket identifier; an output node names its linked input.

    Yields:
        The copied holder and output, or missing-node, socket, link, or native-edit faults. Every copy releases when the context exits.
    """

    def found(tree: bpy.types.NodeTree, name: str) -> BlenderResult[bpy.types.Node]:
        return Option.of_optional(tree.nodes.get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=name),))

    def entered(copy: bpy.types.NodeTree | bpy.types.Material | bpy.types.World | bpy.types.Light) -> BlenderResult[bpy.types.NodeTree]:
        match copy:
            case bpy.types.NodeTree():
                return Ok(copy)
            case bpy.types.Material() | bpy.types.World() | bpy.types.Light():
                return Option.of_optional(copy.node_tree).to_result_with(lambda: (BlenderAbsentFault(owner=repr(copy), key="node_tree"),))

    def endpoint(node: bpy.types.Node) -> BlenderResult[tuple[str, str]]:
        match node:
            case bpy.types.ShaderNodeOutputMaterial() | bpy.types.ShaderNodeOutputWorld() | bpy.types.ShaderNodeOutputLight():
                return _socket(node.inputs, route.socket).map(lambda socket: (node.name, socket.identifier))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            case bpy.types.NodeGroupOutput():
                return _socket(node.inputs, route.socket).bind(
                    lambda socket: (
                        Option
                        .of_optional(next(iter(socket.links), None))  # type: ignore[arg-type]  # ty: ignore[no-matching-overload]
                        .to_result_with(lambda: (BlenderAbsentFault(owner=repr(socket.links), key=route.socket),))
                        .map(lambda link: (link.from_node.name, link.from_socket.identifier))  # type: ignore[union-attr]
                    )
                )
            case _:
                return _socket(node.outputs, route.socket).map(lambda socket: (node.name, socket.identifier))

    with ExitStack() as rollback:

        def copied[T: bpy.types.NodeTree | bpy.types.Material | bpy.types.World | bpy.types.Light](block: T) -> BlenderResult[T]:
            def acquired(copy: T) -> T:
                rollback.callback(bpy.data.batch_remove, ids=(copy,))
                return copy

            return Ownership(kind=type(block), key=route.key, saved=Saved.NEVER, single=False).made(identity, lambda: Ok(block.copy())).map(acquired)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

        def exposed(inner: bpy.types.NodeTree, name: str, socket: str) -> BlenderResult[str]:
            def connected(pair: tuple[bpy.types.Node, bpy.types.NodeSocket]) -> BlenderResult[str]:
                output, source = pair
                return (
                    catch(lambda: inner.links.new(source, output.inputs["__extend__"], handle_dynamic_sockets=True), exception=RuntimeError)()
                    .map_error(lambda error: (BlenderEditFault(request=repr(inner.links), error=error),))
                    .map(lambda link: link.to_socket.identifier)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                )

            return paired(
                Option.of_optional(next((node for node in inner.nodes if isinstance(node, bpy.types.NodeGroupOutput) and node.is_active_output), None)).to_result_with(
                    lambda: (BlenderAbsentFault(owner=repr(inner.nodes), key=bpy.types.NodeGroupOutput.bl_rna.identifier),)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                ),
                found(inner, name).bind(lambda node: _socket(node.outputs, socket)),
            ).bind(connected)

        def outward(tree: bpy.types.NodeTree, groups: tuple[str, ...]) -> BlenderResult[tuple[str, str]]:
            match groups:
                case (head, *rest):
                    return (
                        found(tree, head)
                        .bind(
                            lambda node: (
                                _pointed(node, "node_tree", bpy.types.NodeTree)
                                .map(lambda group: copied(group).bind(lambda inner: written(node, "node_tree", inner).map(lambda _: inner)))
                                .default_value(Error((BlenderAbsentFault(owner=repr(node), key="node_tree"),)))
                            )
                        )
                        .bind(lambda inner: outward(inner, tuple(rest)).map(lambda carried: (inner, *carried)))
                        .bind(lambda carried: exposed(*carried))
                        .map(lambda socket: (head, socket))
                    )
                case _:
                    return found(tree, route.node).bind(endpoint)

        yield copied(holder).bind(lambda copy: entered(copy).bind(lambda tree: outward(tree, route.groups)).map(lambda carried: Routed(holder=copy, node=carried[0], socket=carried[1])))


def addresses(tree: bpy.types.NodeTree, identity: Identity) -> BlenderResult[dict[Key, str]]:
    """Reads a group's item key to live socket identifier map.

    Args:
        tree: Group tree whose stored item keys the map reads.
        identity: Extension whose attachment name holds the stored keys.

    Returns:
        `Ok` of each item key's socket identifier, a panel key answering its toggle socket; `Error` of `register` for a refused attachment name.
    """
    return identity.name(Name.ATTACHMENT, "interface").map(lambda slot: _addresses(tree, slot))


def _tree(content: Ramp | CurvePoints) -> Tree[Value]:
    """Lowers a ramp or curve points into the settings tree its node stores, keyed by RNA path from the node."""
    match content:
        case Ramp(stops=stops, interpolation=interpolation, mode=mode, hue_path=hue_path):
            elements = tuple(f"color_ramp.elements[{index}]" for index in range(len(stops)))
            return Tree(
                roots=("color_ramp",),
                owners={
                    "color_ramp": Owner(values={"interpolation": interpolation.name, "color_mode": mode.name, "hue_interpolation": hue_path.name}, children=elements),
                    **{
                        key: Owner(values={"position": stop.position, "color": (stop.color.red, stop.color.green, stop.color.blue, 1.0)})
                        for key, stop in zip(elements, sorted(stops, key=lambda stop: stop.position))
                    },
                },
            )
        case CurvePoints(clip=clip, maps=maps, handle=handle, clipped=clipped):
            curves = {f"mapping.curves[{index}]": points for index, points in enumerate(maps)}
            keys = {curve: tuple(f"{curve}.points[{index}]" for index in range(len(points))) for curve, points in curves.items()}
            bounds = clip.map(lambda span: dict(zip(("clip_min_x", "clip_min_y", "clip_max_x", "clip_max_y"), span))).default_value({})
            return Tree(
                roots=("mapping",),
                owners={
                    "mapping": Owner(values={"use_clip": clipped, "extend": "EXTRAPOLATED", **bounds}, children=tuple(curves)),
                    **{curve: Owner(values={}, children=keys[curve]) for curve in curves},
                    **{key: Owner(values={"location": point, "handle_type": handle}) for curve, points in curves.items() for key, point in zip(keys[curve], points)},
                },
            )


def _current(content: Content, live: bpy.types.Node, identity: Identity) -> BlenderResult[bool]:
    """Whether a node holds a content, one arm per content case."""
    match content:
        case Ramp() | CurvePoints():
            desired = _tree(content)
            return (
                accumulated(Descriptor.resolve(type(live), root, bpy.types.PointerProperty) for root in desired.roots)
                .bind(lambda roots: Snapshot.capture(live, Some(roots)))
                .map(
                    lambda held: all(
                        key in held.tree.owners
                        and held.tree.owners[key].children == owner.children
                        and all(_holds(struct, name, value) for struct in struct_at(live, key).to_list() for name, value in owner.values.items())
                        for key, owner in desired.owners.items()
                    )
                )
            )
        case Generated(ownership=ownership, content=hashed):
            return Ok(
                _pointed(live, "image", bpy.types.Image)
                .map(lambda image: Mark.read(identity, image) == Some(Mark(key=ownership.key, content=Some(hashed))))
                .default_with(lambda: Mark.read(identity, live).map(lambda mark: mark.content == Some(hashed)).default_value(False))
            )
        case Loaded(path=path, tag=tag, frames=frames):
            return (
                frames
                .map(lambda want: _framed(live).map(lambda holder: all(_holds(holder, name, value) for name, value in msgspec.structs.asdict(want).items())))
                .default_value(Ok(True))
                .map(
                    lambda framed: (
                        framed
                        and _pointed(live, "image", bpy.types.Image)
                        .map(lambda image: bpy.path.abspath(image.filepath, library=image.library) == path and (image.source == "SEQUENCE") == tag.sequence)
                        .default_value(False)
                    )
                )
            )
        case Nested(group=group):
            return (
                _pointed(live, "node_tree", bpy.types.NodeTree)
                .filter(lambda tree: Mark.read(identity, tree) == Some(Mark(key=group.key)))
                .map(lambda tree: group.graph.changes(tree, identity, group.interface).map(lambda rows: rows == Changes()))
                .default_value(Ok(False))
            )
        case MenuItems() as menu:
            return _narrowed(live, bpy.types.GeometryNodeMenuSwitch, "enum_items").map(
                lambda switch: tuple((each.name, each.description) for each in switch.enum_items) == tuple((each.name, each.description) for each in menu.items)
            )
        case FileItems() as files:
            return _narrowed(live, bpy.types.CompositorNodeOutputFile, "file_output_items").map(
                lambda output: (
                    len(output.file_output_items) == len(files.items)
                    and all(held.name == item.name and held.socket_type == item.kind and not _differing(item.members, held) for held, item in zip(output.file_output_items, files.items))
                )
            )


def stored(content: Content, live: bpy.types.Node, identity: Identity) -> BlenderResult[None]:
    """Writes declared content onto an acquired native node.

    Args:
        content: Ramp, curves, image, group, menu, or file-output storage to write.
        live: Native node retained by its caller's resource scope.
        identity: Extension owning referenced groups, images, and content marks.

    Returns:
        `Ok` after writing; native snapshot, image, group, and RNA faults remain typed errors.
    """
    match content:
        case Ramp() | CurvePoints():
            return Snapshot(struct=live.bl_rna.identifier, tree=_tree(content)).apply(live).outcome  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        case Generated(ownership=ownership, content=hashed, name=name, plane=plane, tag=tag):
            return generated(ownership, identity, name, hashed, plane, tag).bind(
                lambda image: written(live, "image", image.to_optional()).bind(
                    lambda _: (
                        Mark
                        .read(identity, live)
                        .map(lambda mark: msgspec.structs.replace(mark, content=Some(hashed) if image.is_none() else Nothing).write(identity, live))
                        .default_value(Error((BlenderAbsentFault(owner=repr(live), key=identity.manifest.id),)))
                    )
                )
            )
        case Loaded(path=path, tag=tag, frames=frames):
            return loaded(path, tag).bind(lambda image: written(live, "image", image)).bind(lambda _: frames.map(lambda want: _framed(live).bind(want.applied)).default_value(Ok(None)))
        case Nested(group=group):
            return msgspec.structs.replace(group, name=f".{group.name}").made(identity).bind(lambda tree: written(live, "node_tree", tree)).map(lambda _: None)
        case MenuItems() as menu:
            return (
                _narrowed(live, bpy.types.GeometryNodeMenuSwitch, "enum_items")
                .map(lambda switch: _ordered(switch.enum_items, tuple(each.name for each in menu.items), switch.enum_items.new))
                .bind(lambda held: accumulated(written(each, "description", item.description) for each, item in zip(held, menu.items)).map(lambda _: None))
            )
        case FileItems() as files:
            kinds = {item.name: item.kind for item in files.items}
            return (
                _narrowed(live, bpy.types.CompositorNodeOutputFile, "file_output_items")
                .map(lambda output: _ordered(output.file_output_items, tuple(kinds), lambda name: output.file_output_items.new(kinds[name], name)))
                .bind(lambda held: accumulated(paired(written(each, "socket_type", item.kind), _assigned(item.members, each)) for each, item in zip(held, files.items)).map(lambda _: None))
            )


def _ordered(
    items: bpy.types.NodeMenuSwitchItems | bpy.types.NodeCompositorFileOutputItems, names: tuple[str, ...], made: Callable[[str], object]
) -> tuple[bpy.types.NodeEnumItem | bpy.types.NodeCompositorFileOutputItem, ...]:
    """Keeps a named item collection to the given names in order, removing the rest and adding the missing, so a held item keeps its name and stored values."""
    for name in tuple(item.name for item in items if item.name not in names):
        match items:
            case bpy.types.NodeMenuSwitchItems():
                items.remove(items[name])
            case bpy.types.NodeCompositorFileOutputItems():
                items.remove(items[name])
    for name in [name for name in names if items.get(name) is None]:
        made(name)
    for index, name in enumerate(names):
        items.move(tuple(items).index(items[name]), index)
    return tuple(items)


def _restated(
    key: Key, entry: "tuple[Socket, SocketSide] | InterfacePanel", place: tuple[Option[Key], int], held: Option[bpy.types.NodeTreeInterfaceItem], inverse: dict[str | int, Key], *, moved: bool
) -> Option[Restated]:
    """Differing parts of a kept interface item or the members of a fresh one, with its place where it, its panel, or its position moved."""

    def placed(item: bpy.types.NodeTreeInterfaceItem) -> Option[tuple[Option[Key], int]]:
        parent = Option.of_optional(item.parent).bind(lambda panel: Option.of_optional(inverse.get(panel.identifier)))
        return Nothing if not moved and (parent, item.position) == place else Some(place)

    def changed[T](now: T, wanted: T) -> Option[T]:
        return Nothing if now == wanted else Some(wanted)

    def parts() -> Restated:
        match entry, held.to_optional():
            case (Socket() as socket, _), bpy.types.NodeTreeInterfaceSocket() as item:
                kind = Nothing if item.socket_type == socket.kind.bl_rna.identifier else Some(socket.kind)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                return Restated(
                    key=key,
                    kind=kind,
                    name=changed(item.name, socket.name),
                    description=changed(item.description, socket.description),
                    place=placed(item),
                    members=kind.map(lambda _: socket.members).default_with(lambda: _differing(socket.members, item)),
                )
            case InterfacePanel() as panel, bpy.types.NodeTreeInterfacePanel() as item:
                return Restated(
                    key=key,
                    name=changed(item.name, panel.name),
                    description=changed(item.description, panel.description),
                    closed=changed(item.default_closed, panel.closed),
                    toggle=panel.toggle.filter(lambda value: not any(_holds(toggle, "default_value", value) for toggle in _toggle(item).to_list())),
                    place=placed(item),
                )
            case (Socket() as socket, _), _:
                return Restated(key=key, place=Some(place), members=socket.members)
            case _:
                return Restated(key=key, place=Some(place))

    return Some(parts()).filter(lambda row: row != Restated(key=key))


def _matched(entry: "tuple[Socket, SocketSide] | InterfacePanel", item: bpy.types.NodeTreeInterfaceItem) -> bool:
    """Whether a live interface item keeps a desired item's kind, side, and toggle presence."""
    match entry, item:
        case (Socket(), side), bpy.types.NodeTreeInterfaceSocket():
            return item.in_out == side
        case InterfacePanel(toggle=toggle), bpy.types.NodeTreeInterfacePanel():
            return toggle.is_some() == _toggle(item).is_some()
        case _:
            return False


def _toggle(item: bpy.types.NodeTreeInterfaceItem) -> Option[bpy.types.NodeTreeInterfaceSocket]:
    """Panel's toggle socket, `Nothing` for a socket or a panel holding none."""
    match item:
        case bpy.types.NodeTreeInterfacePanel():
            return Option.of_optional(next((child for child in item.interface_items if isinstance(child, bpy.types.NodeTreeInterfaceSocket) and child.is_panel_toggle), None))
        case _:
            return Nothing


def _holds(struct: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection", identifier: str, value: Value) -> bool:
    """Whether a live struct's member, resolved on its own RNA, holds a value under `Descriptor.equal`."""
    return Descriptor.resolve(struct, identifier, bpy.types.Property).map(lambda descriptor: descriptor.equal(descriptor.read(struct), value)).to_option().default_value(False)


def _differing(members: dict[Descriptor, Value], root: bpy.types.bpy_struct) -> dict[Descriptor, Value]:
    """Members whose owner under a root is absent or reads a value other than the desired one."""
    return {descriptor: value for descriptor, value in members.items() if not any(descriptor.equal(descriptor.read(owner), value) for owner in struct_at(root, descriptor.owner).to_list())}


def _assigned(members: dict[Descriptor, Value], root: bpy.types.bpy_struct) -> BlenderResult[None]:
    """Writes members under a root, ID pointers first, then enum members, each group in declaration order."""

    def assigned(descriptor: Descriptor, value: Value) -> BlenderResult[None]:
        return (
            struct_at(root, descriptor.owner).map(lambda owner: descriptor.write(owner, value)).default_value(Error((BlenderAbsentFault(owner=repr(root), key=descriptor.path),))).map(lambda _: None)
        )

    pointers: list[tuple[Descriptor, Value]] = []
    choices: list[tuple[Descriptor, Value]] = []
    values: list[tuple[Descriptor, Value]] = []
    for descriptor, value in members.items():
        match descriptor.prop:
            case bpy.types.PointerProperty():
                pointers.append((descriptor, value))
            case bpy.types.EnumProperty():
                choices.append((descriptor, value))
            case _:
                values.append((descriptor, value))
    return functools.reduce(
        lambda done, phase: done.bind(lambda _: accumulated(itertools.starmap(assigned, phase)).map(lambda _: None)), (pointers, choices, values), Result[None, BlenderFaults].Ok(None)
    ).map(lambda _: None)


def _stale(
    live: bpy.types.Node, sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: dict[str, Value], slot: str, interface: Option[dict[str, tuple[str, type[bpy.types.NodeSocket]]]]
) -> dict[str, Value]:
    """Socket values by address whose live socket is absent or holds another value."""
    keyed = _keyed(live, slot)
    return {
        address: value
        for address, value in values.items()
        if not any(
            _holds(socket, "default_value", value)
            and interface.map(
                lambda future: any(identifier == socket.identifier and address in {key, identifier} and isinstance(socket, kind) for key, (identifier, kind) in future.items())
            ).default_value(True)
            for socket in _socket(sockets, keyed.get(address, address)).to_option().to_list()
        )
    }


def valued(live: bpy.types.Node, sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: Mapping[str, Value], slot: str) -> BlenderResult[None]:
    """Writes socket defaults by native identifier or declared interface key."""
    keyed = _keyed(live, slot)

    def valued(address: str, value: Value) -> BlenderResult[None]:
        return _socket(sockets, keyed.get(address, address)).bind(lambda socket: written(socket, "default_value", value)).map(lambda _: None)

    return accumulated(itertools.starmap(valued, values.items())).map(lambda _: None)


def _pointed[T: bpy.types.ID](live: bpy.types.Node, identifier: str, kind: type[T]) -> Option[T]:
    """Data-block a node's pointer member names, `Nothing` for an empty pointer or a member the node lacks."""
    return struct_at(live, identifier).bind(lambda found: Some(found) if isinstance(found, kind) else Nothing)


def _framed(live: bpy.types.Node) -> BlenderResult[bpy.types.ImageUser | bpy.types.CompositorNodeImage]:
    """Struct holding a node's frame window: a compositor image node itself, a shader image node's `image_user`."""
    match live:
        case bpy.types.CompositorNodeImage():
            return Ok(live)
        case bpy.types.ShaderNodeTexImage() | bpy.types.ShaderNodeTexEnvironment():
            return Ok(live.image_user)
        case _:
            return Error((BlenderAbsentFault(owner=repr(live), key="image_user"),))


def _narrowed[N: bpy.types.Node](live: bpy.types.Node, kind: type[N], member: str) -> BlenderResult[N]:
    """Node as the class holding a member, `absent` holding the member for any other class."""
    return Ok(live) if isinstance(live, kind) else Error((BlenderAbsentFault(owner=repr(live), key=member),))


def _holder(block: bpy.types.ID) -> Option[bpy.types.ID]:
    """`bpy.data` holder of a data-block: itself, or the material, world, or light embedding a node tree, `Nothing` for a tree none embeds."""
    return Option.of_optional(next((holder for holder in (*bpy.data.materials, *bpy.data.worlds, *bpy.data.lights) if holder.node_tree == block), None)) if block.is_embedded_data else Some(block)


def _items(tree: bpy.types.NodeTree, slot: str) -> dict[str, str | int]:
    """Stored item key to item identifier, empty where the slot holds no group."""
    return Option.of_optional(tree.get(slot)).filter(lambda value: isinstance(value, idprop.types.IDPropertyGroup)).map(lambda group: dict(group.to_dict())).default_value({})


def _addresses(tree: bpy.types.NodeTree, slot: str) -> dict[str, str]:
    """Item key to live socket identifier, a panel key answering its toggle socket."""
    live = {item.identifier: item for item in tree.interface.items_tree if isinstance(item, (bpy.types.NodeTreeInterfaceSocket, bpy.types.NodeTreeInterfacePanel))}  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    return {
        key: socket.identifier
        for key, identifier in _items(tree, slot).items()
        for item in Option.of_optional(live.get(identifier)).to_list()
        for socket in (Some(item) if isinstance(item, bpy.types.NodeTreeInterfaceSocket) else _toggle(item)).to_list()
    }


def _keyed(node: bpy.types.Node, slot: str) -> dict[str, str]:
    """Address to socket identifier: interface item keys on a group boundary or group node, item names on a Menu Switch, empty for any other node."""
    match node:
        case bpy.types.NodeGroupInput() | bpy.types.NodeGroupOutput():
            return _addresses(node.id_data, slot)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        case bpy.types.CompositorNodeGroup() | bpy.types.ShaderNodeGroup() | bpy.types.GeometryNodeGroup():
            return Option.of_optional(node.node_tree).map(lambda tree: _addresses(tree, slot)).default_value({})
        case bpy.types.GeometryNodeMenuSwitch():
            fixed = {socket.identifier for socket in (*node.inputs, *node.outputs)}  # type: ignore[misc]  # ty: ignore[not-iterable]
            return {item.name: socket.identifier for item, socket in zip(node.enum_items, tuple(node.outputs)[1:]) if item.name not in fixed}  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        case _:
            return {}


def _coordinates(key: Key, image: Operand) -> BlenderResult[tuple[PixelGrid, tuple[Fragment, End]]]:
    """Builds native data and display coordinates over the realized source domain."""
    realized, coordinates, dimensions = item_key(key, "realized"), item_key(key, "coordinates"), item_key(key, "dimensions")
    information, offset = item_key(key, "information"), item_key(key, "window_offset")

    def built(pixel: tuple[Fragment, End], sizes: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[PixelGrid, tuple[Fragment, End]]]:
        data, display = sizes
        return paired(
            paired(
                vector(item_key(key, "centre"), "ADD", pixel, (0.5, 0.5, 0.0)),
                paired(
                    node(item_key(offset, "x"), bpy.types.CompositorNodeLevels, {"Image": pixel, "Channel": "Red"}, {}).map(ended(item_key(offset, "x"), "Minimum")),
                    node(item_key(offset, "y"), bpy.types.CompositorNodeLevels, {"Image": pixel, "Channel": "Green"}, {}).map(ended(item_key(offset, "y"), "Minimum")),
                ).bind(lambda minima: node(offset, bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y"), minima)), {}).map(ended(offset, "Vector"))),
            ),
            vector(item_key(key, "inverse"), "DIVIDE", (1.0, 1.0, 0.0), data),
        ).bind(
            lambda parts: (
                vector(item_key(key, "local"), "SUBTRACT", *parts[0])
                .bind(lambda local: vector(item_key(key, "normalized"), "MULTIPLY", local, parts[1]))
                .bind(lambda normalized: vector(item_key(key, "base"), "ADD", normalized, (0.0, 0.0, 1.0)))
                .map(lambda base: (PixelGrid(base=base, inverse=parts[1], point=parts[0][0], offset=parts[0][1]), display))
            )
        )

    return paired(
        node(realized, bpy.types.CompositorNodeBlur, {"Image": image}, {})
        .map(ended(realized, "Image"))
        .bind(lambda source: node(coordinates, bpy.types.CompositorNodeImageCoordinates, {"Image": source}, {}).map(ended(coordinates, "Pixel"))),
        paired(
            node(information, bpy.types.CompositorNodeImageInfo, {"Image": image}, {}).map(ended(information, "Dimensions")),
            node(dimensions, bpy.types.CompositorNodeRelativeToPixel, {"Image": image, "Float Value": 1.0}, {"data_type": "FLOAT", "reference_dimension": "PER_DIMENSION"}).map(
                ended(dimensions, "Vector Value")
            ),
        ),
    ).bind(lambda parts: built(*parts))


def _socket(sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, identifier: str) -> BlenderResult[bpy.types.NodeSocket]:
    """Finds a socket by identifier alone, never by name."""
    return Option.of_optional(next((socket for socket in sockets if socket.identifier == identifier), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(sockets), key=identifier),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "INPUTS",
    "OUTPUTS",
    "Added",
    "Alteration",
    "Altered",
    "Changes",
    "Clock",
    "Configured",
    "Connected",
    "Content",
    "CurvePoints",
    "Declared",
    "Disconnected",
    "End",
    "FileItem",
    "FileItems",
    "Fragment",
    "FrameNode",
    "FrameSpace",
    "Generated",
    "Graph",
    "GraphTree",
    "Interface",
    "InterfacePanel",
    "Link",
    "Loaded",
    "MenuItem",
    "MenuItems",
    "Nested",
    "Node",
    "NodeGroup",
    "Operand",
    "Part",
    "PixelGrid",
    "Points",
    "Removed",
    "Reordered",
    "Restated",
    "Route",
    "Routed",
    "Socket",
    "SocketSide",
    "Structural",
    "TreeChange",
    "Valued",
    "Withdrawn",
    "addresses",
    "compared",
    "ended",
    "fingerprinted",
    "framed",
    "gridded",
    "item_key",
    "masked",
    "math",
    "menu",
    "mixed",
    "nested",
    "node",
    "origin",
    "pixels",
    "reconciling",
    "routed",
    "sampled",
    "selected",
    "stored",
    "summed",
    "switched",
    "under",
    "valued",
    "vector",
]
