"""Group interfaces declared directly, from records, or from stock node inputs, and owned groups of a graph and an interface."""

from collections.abc import Iterator, Mapping
from contextlib import ExitStack
from enum import Enum
import functools
import inspect
from types import MappingProxyType
from typing import Final, Literal, override, Protocol, runtime_checkable, Self

import bpy
from expression import Nothing, Ok, Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import msgspec.inspect
import msgspec.structs
import numpy as np
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.desired import Content, Graph, item_key, MenuItem, MenuItems
from blender.bpy.nodes.trees import base_socket, socket_at
from blender.records.faults import accumulated, Fault, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type DesiredItem = tuple[Socket, Literal["INPUT", "OUTPUT"]] | InterfacePanel


@runtime_checkable
class Socketed(Protocol):
    """Record stating the group sockets its fields take, such as a stock node's settings, in place of the sockets its field types give."""

    @property
    def sockets(self) -> "tuple[Socket, ...]":
        """Group sockets keyed by field name."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_TREES: Final[Mapping[type[bpy.types.Node], type[bpy.types.NodeTree]]] = MappingProxyType({
    bpy.types.CompositorNode: bpy.types.CompositorNodeTree,
    bpy.types.GeometryNode: bpy.types.GeometryNodeTree,
    bpy.types.FunctionNode: bpy.types.GeometryNodeTree,
    bpy.types.ShaderNode: bpy.types.ShaderNodeTree,
    bpy.types.TextureNode: bpy.types.TextureNodeTree,
})
_SINGLE: Final = float(np.finfo(np.float32).max)

# --- [MODELS] ---------------------------------------------------------------------------


class StockInput(msgspec.Struct, frozen=True, kw_only=True):
    """Input of a stock node class by identifier, whose host declaration a socket takes in place of a stated class."""

    node: type[bpy.types.Node]
    input: str


class Socket(msgspec.Struct, frozen=True, kw_only=True):
    """Interface socket by parameter key, its class a refined socket class whose default value states subtype, dimensions, and bounds, or a stock node input the host declares.

    A stock input's class, description, bounds, and default come from the host at reconcile or graph build, stated bounds and default
    winning, and a menu linked to a stock menu socket takes that socket's definition. Group input menu items are kept only while linked to a Menu Switch.
    """

    key: Key
    kind: type[bpy.types.NodeSocket] | StockInput
    name: str | None = None
    description: str = ""
    min_value: float | None = None
    max_value: float | None = None
    default_value: Value = None
    hide_in_modifier: bool | None = None
    items: Option[MenuItems] = Nothing

    @property
    def title(self) -> str:
        """Stated name, else the key's display name."""
        return bpy.path.display_name(self.key) if self.name is None else self.name

    @property
    def stock(self) -> Option[StockInput]:
        """Stock node input the socket takes its declaration from, a graph feeding that input with the socket's value, Nothing for a stated class."""
        return Option.of_optional(try_downcast(StockInput, self.kind))

    @property
    def declared(self) -> "Socket":
        """The socket as declared: a stock input's host class, description, bounds, and default under its stated bounds and default, any other socket itself."""
        return self.stock.map(lambda stock: _declared(self, stock)).default_value(self)

    @property
    def refined(self) -> type[bpy.types.NodeSocket]:
        """Refined socket class, a stock input's as the host declares it."""
        match self.kind:
            case StockInput() as stock:
                return _declared(self, stock).refined
            case kind:
                return kind

    @property
    def base(self) -> type[bpy.types.NodeSocket]:
        """Base socket class an interface item takes as its socket type."""
        return base_socket(self.refined)

    @property
    def members(self) -> dict[RnaPath, Value]:
        """Interface members by path in write order: subtype and dimensions of a refined class, then bounds, the class's own finite bounds where none is stated, then the default."""
        held, kind = self.declared, self.refined
        properties = kind.bl_rna.properties
        match properties["default_value"] if kind is not self.base and "default_value" in properties else None:
            case bpy.types.FloatProperty() | bpy.types.IntProperty() as prop:
                finite = abs(prop.hard_max) < _SINGLE
                subtype = None if prop.subtype == "NONE" else prop.subtype
                dimensions = prop.array_length if self.base is bpy.types.NodeSocketVector else None
                low, high = (prop.hard_min, prop.hard_max) if finite else (None, None)
            case _:
                subtype, dimensions, low, high = None, None, None, None
        return {
            path: value
            for path, value in (
                (("subtype",), subtype),
                (("dimensions",), dimensions),
                (("min_value",), low if held.min_value is None else held.min_value),
                (("max_value",), high if held.max_value is None else held.max_value),
                (("default_value",), held.default_value),
                (("hide_in_modifier",), held.hide_in_modifier),
            )
            if value is not None
        }

    @classmethod
    def of(cls, key: Key, annotation: object, default: Value | Enum = None, /) -> Option[Self]:
        """Builds the socket a typed value declares, bounds and description from its `msgspec.Meta`, its class from a socket class or `StockInput` its `Annotated` metadata carries.

        Booleans take a Bool socket, integers an Int socket, floats bounded to [0, 1] a factor and other floats a Float socket, text a String
        socket, pairs and triples of floats a 2D or 3D vector, and enums and text literals a menu of their members, a bound past float32 left to the socket class.
        A carried socket class or stock input replaces the class the type gives, such as `NodeSocketColor` over four floats.

        Args:
            key: Socket key.
            annotation: Value type, an alias over `Annotated` included.
            default: Default value, an enum member as its item name.

        Returns:
            Socket of the value type, `Nothing` for a type no socket holds, such as a record.
        """

        def bound(inclusive: float | None, exclusive: float | None) -> float | None:
            held = exclusive if inclusive is None else inclusive
            return held if held is None or abs(held) < _SINGLE else None

        value: Value = MenuItem.of(default).name if isinstance(default, Enum) else default
        match msgspec.inspect.type_info(annotation):
            case msgspec.inspect.Metadata(type=info, extra_json_schema={"description": str(stated)}):
                described = stated
            case msgspec.inspect.Metadata(type=info) | info:
                described = ""
        made = functools.partial(cls, key=key, description=described, default_value=value)
        carried = inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata
        match next((held for held in carried if isinstance(held, StockInput) or (isinstance(held, type) and issubclass(held, bpy.types.NodeSocket))), None), info:
            case None, _:
                pass
            case form, msgspec.inspect.FloatType(ge=ge, gt=gt, le=le, lt=lt) | msgspec.inspect.IntType(ge=ge, gt=gt, le=le, lt=lt):
                return Some(made(kind=form, min_value=bound(ge, gt), max_value=bound(le, lt)))
            case form, _:
                return Some(made(kind=form))
        match info:
            case msgspec.inspect.BoolType():
                return Some(made(kind=bpy.types.NodeSocketBool))
            case msgspec.inspect.IntType(ge=ge, gt=gt, le=le, lt=lt):
                return Some(made(kind=bpy.types.NodeSocketInt, min_value=bound(ge, gt), max_value=bound(le, lt)))
            case msgspec.inspect.FloatType(ge=0.0, le=1.0):
                return Some(made(kind=bpy.types.NodeSocketFloatFactor))
            case msgspec.inspect.FloatType(ge=ge, gt=gt, le=le, lt=lt):
                return Some(made(kind=bpy.types.NodeSocketFloat, min_value=bound(ge, gt), max_value=bound(le, lt)))
            case msgspec.inspect.StrType():
                return Some(made(kind=bpy.types.NodeSocketString))
            case msgspec.inspect.TupleType(item_types=(msgspec.inspect.FloatType(), msgspec.inspect.FloatType())):
                return Some(made(kind=bpy.types.NodeSocketVector2D))
            case msgspec.inspect.TupleType(item_types=(msgspec.inspect.FloatType(), msgspec.inspect.FloatType(), msgspec.inspect.FloatType())):
                return Some(made(kind=bpy.types.NodeSocketVector))
            case msgspec.inspect.EnumType(cls=kind):
                return Some(made(kind=bpy.types.NodeSocketMenu, items=Some(MenuItems.of(kind))))
            case msgspec.inspect.LiteralType(values=values) if all(isinstance(held, str) for held in values):
                return Some(made(kind=bpy.types.NodeSocketMenu, items=Some(MenuItems(items=tuple(MenuItem(name=str(held)) for held in values)))))
            case _:
                return Nothing


class Interface(msgspec.Struct, frozen=True, kw_only=True):
    """Group interface items in Blender order."""

    outputs: tuple[Socket, ...] = ()
    inputs: tuple[Socket, ...] = ()
    panels: tuple["InterfacePanel", ...] = ()

    @property
    def first(self) -> int:
        """Position of the first item, after nothing at the root."""
        return 0

    @property
    def menu(self) -> MenuItems:
        """Menu items selecting one of the interface's panels, each under the panel's title and description."""
        return MenuItems(items=tuple(panel.item for panel in self.panels))

    def walked(self, parent: Option[Key] = Nothing) -> Iterator[tuple[Key, DesiredItem, Option[Key], int]]:
        """Walks interface items in preorder.

        Args:
            parent: Key of panel holding items, `Nothing` at root.

        Yields:
            Item key, socket with side or panel, parent key, and position after the toggle socket a toggled panel holds first.
        """
        items: tuple[DesiredItem, ...] = (*((socket, "OUTPUT") for socket in self.outputs), *((socket, "INPUT") for socket in self.inputs), *self.panels)
        for position, item in enumerate(items, self.first):
            match item:
                case InterfacePanel(key=key):
                    yield key, item, parent, position
                    yield from item.walked(Some(key))
                case (socket, _):
                    yield socket.key, item, parent, position

    def defaulted(self, values: Mapping[Key, Value], /) -> Self:
        """Returns the interface with the default of each socket at any depth `values` keys replaced."""

        def socket(held: Socket) -> Socket:
            return msgspec.structs.replace(held, default_value=values[held.key]) if held.key in values else held

        return msgspec.structs.replace(self, outputs=tuple(map(socket, self.outputs)), inputs=tuple(map(socket, self.inputs)), panels=tuple(panel.defaulted(values) for panel in self.panels))


class InterfacePanel(Interface, frozen=True, kw_only=True):
    """Interface panel with optional toggle, titled by its key unless named."""

    key: Key
    name: str | None = None
    description: str = ""
    closed: bool = False
    toggle: Option[bool] = Nothing

    @property
    @override
    def first(self) -> int:
        """Position of the first item, after the toggle a toggled panel holds."""
        return int(self.toggle.is_some())

    @property
    def title(self) -> str:
        """Stated name, else the key's display name."""
        return bpy.path.display_name(self.key) if self.name is None else self.name

    @property
    def item(self) -> MenuItem:
        """Menu item selecting the panel, under its title and description."""
        return MenuItem(name=self.title, description=self.description)

    def socket(self, *path: object) -> Socket:
        """Returns the socket at a key path under the panel, record fields by member descriptor and case classes by class, such as `panel.socket(Clock.speed)`.

        Raises:
            LookupError: For a path the panel declares no socket at, at graph build.
        """
        match _found(self, item_key(self.key, *path)):
            case Option(tag="some", some=(Socket() as found, _)):
                return found
            case _:
                raise LookupError(f"{item_key(self.key, *path)} names no socket under {self.key}")

    def panel(self, *path: object) -> "InterfacePanel":
        """Returns the panel at a key path under the panel as `socket` reads one, such as a record panel a section panel nests, its keys as the interface holds them.

        Raises:
            LookupError: For a path the panel declares no panel at, at graph build.
        """
        match _found(self, item_key(self.key, *path)):
            case Option(tag="some", some=InterfacePanel() as found):
                return found
            case _:
                raise LookupError(f"{item_key(self.key, *path)} names no panel under {self.key}")

    @classmethod
    def of(cls, key: Key, face: Interface | msgspec.Struct, /, *, name: str | None = None, description: str | None = None, closed: bool = False, toggle: Option[bool] = Nothing) -> Self:
        """Builds the panel at `key` holding an interface's items, or the interface `fielded` derives from a record, every item key at any depth prefixed by `key` through `item_key` and every title kept.

        Args:
            key: Panel key and the prefix of every key it holds.
            face: Interface whose items the panel holds, or record whose fields it holds.
            name: Panel name, its key's display name by default.
            description: Panel description, a record's docstring by default.
            closed: Whether the panel starts closed.
            toggle: Default of the panel's toggle, `Nothing` for none.

        Returns:
            Panel of the prefixed items.
        """
        held, stated = (face, "") if isinstance(face, Interface) else (fielded(face), Option.of_optional(type(face).__doc__).map(inspect.cleandoc).default_value(""))
        prefixed = _under(key, held)
        return cls(
            key=key, name=name, description=stated if description is None else description, closed=closed, toggle=toggle, outputs=prefixed.outputs, inputs=prefixed.inputs, panels=prefixed.panels
        )


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

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns group ownership row, then every nested group's row."""
        return Ok((self.ownership, *self.graph.groups))


# --- [OPERATIONS] -----------------------------------------------------------------------


def fielded(record: msgspec.Struct, /) -> Interface:
    """Builds the interface of a record's fields keyed by field name: a `Socketed` record's stated sockets, else a socket at its value for each field `Socket.of` takes and a panel of its fields for each field holding a record other than node content."""
    match record:
        case Socketed():
            return Interface(inputs=record.sockets)
        case _:
            fields = tuple((field.name, field.type, getattr(record, field.name)) for field in msgspec.structs.fields(record))
            return Interface(
                inputs=tuple(socket for field, kind, value in fields for socket in Socket.of(field, kind, value).to_list()),
                panels=tuple(InterfacePanel.of(field, value) for field, _, value in fields if _nested(value)),
            )


def decoded[T: msgspec.Struct](record: T, values: Mapping[Key, Value], /, *path: object) -> Outcome[T]:
    """Decodes the record whose fields `InterfacePanel.of(key, record)` or `fielded(record)` declares from the values held at the keys of the fields a socket takes, a nested record from the values under its field's key, fields without a value keeping the record's.

    Args:
        record: Record whose values fields without a held value keep.
        values: Values by socket key, such as entry input values, a menu value as its item name.
        *path: Key path of the panel holding the record's sockets, empty for sockets `fielded` keys at the interface root.

    Returns:
        `Ok` of record of the held values. `Error` of `invalid` for a value outside its field's bounds.
    """

    def held(info: msgspec.inspect.Type, value: Value) -> object:
        match info, value:
            case msgspec.inspect.Metadata(type=inner), _:
                return held(inner, value)
            case msgspec.inspect.EnumType(cls=kind), str(name):
                return MenuItem.member(kind, name).map(lambda member: member.value).default_value(value)
            case _:
                return value

    def nested(field: msgspec.structs.FieldInfo) -> Outcome[tuple[str, object]]:
        return decoded(getattr(record, field.name), values, *path, field.name).map(lambda found: (field.encode_name, msgspec.to_builtins(found)))

    fields = msgspec.structs.fields(record)
    changes = {
        field.encode_name: held(msgspec.inspect.type_info(field.type), values[at])
        for field in fields
        for at in (item_key(*path, field.name),)
        if at in values and Socket.of(field.name, field.type).is_some()
    }
    return accumulated(nested(field) for field in fields if _nested(getattr(record, field.name))).bind(
        lambda inner: _converted({**msgspec.to_builtins(record), **changes, **dict(inner)}, type(record)).map_error(functools.partial(Fault.of, Reason.INVALID, type(record).__name__))
    )


@catch(exception=msgspec.ValidationError)
def _converted[T: msgspec.Struct](builtins: Mapping[str, object], kind: type[T], /) -> T:
    """Record of builtin values, validated against every bound."""
    converted: T = msgspec.convert(builtins, kind)
    return converted


def _under[I: Interface](prefix: Key, face: I) -> I:
    """Interface with every key at any depth prefixed through `item_key` and every title kept."""
    return msgspec.structs.replace(
        face,
        outputs=tuple(msgspec.structs.replace(socket, key=item_key(prefix, socket.key), name=socket.title) for socket in face.outputs),
        inputs=tuple(msgspec.structs.replace(socket, key=item_key(prefix, socket.key), name=socket.title) for socket in face.inputs),
        panels=tuple(msgspec.structs.replace(_under(prefix, panel), key=item_key(prefix, panel.key), name=panel.title) for panel in face.panels),
    )


@functools.cache
def _declared(socket: Socket, stock: StockInput, /) -> Socket:
    """Socket over a stock input as the host declares the input, read from the interface item a scratch group's Group Input grows when linked to it, the group removed after.

    Returns:
        Socket of the host class, description, bounds, and default under the bounds and default the socket states.

    Raises:
        LookupError: For a node class no tree kind holds, or an input the node lacks or no interface item takes.
    """
    if not (kinds := tuple(tree for base, tree in _TREES.items() if issubclass(stock.node, base))):
        raise LookupError(f"{stock.node.__name__} belongs to no node tree kind")
    with ExitStack() as scratch:
        tree: bpy.types.NodeTree = bpy.data.node_groups.new(".stock", kinds[0].__name__)  # type: ignore[arg-type, assignment]  # ty: ignore[invalid-argument-type, invalid-assignment]
        scratch.callback(bpy.data.node_groups.remove, tree)
        entered, node = tree.nodes.new(bpy.types.NodeGroupInput.__name__), tree.nodes.new(stock.node.__name__)
        match socket_at(node.inputs, stock.input):
            case Result(tag="ok", ok=target):
                tree.links.new(entered.outputs[-1], target, handle_dynamic_sockets=True)
            case Result(error=faults):
                raise LookupError(stock, faults)
        face: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        match tuple(item for item in face.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket)):
            case (item,):
                refined: type[bpy.types.NodeSocket] = getattr(bpy.types, item.bl_socket_idname)

                def read(name: str) -> Value:
                    return Descriptor.resolve(item, (name,), bpy.types.Property).map(lambda descriptor: descriptor.read(item)).default_value(None)

                low, high = read("min_value"), read("max_value")
                return msgspec.structs.replace(
                    socket,
                    kind=refined,
                    description=item.description,
                    min_value=low if socket.min_value is None and isinstance(low, int | float) else socket.min_value,
                    max_value=high if socket.max_value is None and isinstance(high, int | float) else socket.max_value,
                    default_value=read("default_value") if socket.default_value is None else socket.default_value,
                )
            case _:
                raise LookupError(f"{stock.node.__name__}.{stock.input} grows no interface socket")


def _found(panel: InterfacePanel, key: Key, /) -> Option[DesiredItem]:
    """Item the panel holds at `key` at any depth."""
    return Block.of_seq(item for held, item, _, _ in panel.walked() if held == key).try_head()


def _nested(value: object, /) -> bool:
    """Whether a record field's value is a record whose fields a nested panel holds, node content a node holds in place of sockets excluded."""
    return isinstance(value, msgspec.Struct) and not isinstance(value, Content)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DesiredItem", "Interface", "InterfacePanel", "NodeGroup", "Socket", "Socketed", "StockInput", "decoded", "fielded"]
