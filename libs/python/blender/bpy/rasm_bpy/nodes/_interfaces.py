"""Group interfaces declared directly, from records, or from stock node inputs."""

from collections.abc import Callable, Iterator, Mapping
from contextlib import ExitStack
from enum import Enum
import functools
import inspect
from itertools import chain, starmap
from types import MappingProxyType
from typing import Final, Literal, Protocol, runtime_checkable, Self, TypeGuard

import bpy
from expression import Option, Result, Some, try_downcast
from expression.extra.result import catch
import msgspec
import msgspec.inspect
import msgspec.structs
import numpy as np
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from rasm_bpy.data._extension.identity import Key
from rasm_bpy.data._rna.descriptors import Descriptor, Value
from rasm_bpy.data._rna.paths import _definition, RnaPath
from rasm_bpy.nodes._desired import Content, item_key, MenuItem, MenuItems
from rasm_bpy.nodes._trees import _base_socket, _interface_of, _new_group, _socket_at
from rasm_records import accumulated, Error, Fault, Nothing, Ok, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type DesiredItem = tuple[Socket, Literal["INPUT", "OUTPUT"]] | InterfacePanel


@runtime_checkable
class _Socketed(Protocol):
    """Record stating the group sockets its fields take (a stock node's settings) in place of the sockets its field types give."""

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

    Stock input's class, description, bounds, and default come from the host at reconcile or graph build, stated bounds and default
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
    items: tuple[MenuItem, ...] = ()

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
        """Socket as declared: a stock input's host class, description, bounds, and default under its stated bounds and default, any other socket itself."""
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
        return _base_socket(self.refined)

    @property
    def members(self) -> dict[RnaPath, Value]:
        """Interface members by path in write order: subtype and dimensions of a refined class, then bounds, the class's own finite bounds where none is stated, then the default."""
        held = self.declared
        kind = held.refined
        base = _base_socket(kind)
        properties = kind.bl_rna.properties
        match properties["default_value"] if kind is not base and "default_value" in properties else None:
            case bpy.types.FloatProperty() | bpy.types.IntProperty() as prop:
                finite = abs(prop.hard_max) < _SINGLE
                subtype = None if prop.subtype == "NONE" else prop.subtype
                dimensions = prop.array_length if prop.is_array else None
                low, high = (prop.hard_min, prop.hard_max) if finite else (None, None)
            case bpy.types.StringProperty(subtype=value):
                subtype, dimensions, low, high = None if value == "NONE" else value, None, None, None
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
        """Builds the socket a typed value declares, bounds and description from its `msgspec.Meta`, its class from a socket class or `StockInput` its `Annotated` metadata holds.

        Booleans take a Bool socket, integers an Int socket, floats bounded to [0, 1] a factor and other floats a Float socket, text a String
        socket, pairs and triples of floats a 2D or 3D vector, and enums and text literals a menu of their members, a bound past float32 left to the socket class.
        Socket classes or stock inputs in metadata replace the class the type gives (`NodeSocketColor` over four floats).

        Args:
            key: Socket key.
            annotation: Value type, an alias over `Annotated` included.
            default: Default value, an enum member as its item name.

        Returns:
            Socket of the value type, `Nothing` for a type no socket holds (a record).
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
        metadata = inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata
        low, high = None, None
        items: tuple[MenuItem, ...] = ()
        match next((held for held in metadata if isinstance(held, StockInput) or (isinstance(held, type) and issubclass(held, bpy.types.NodeSocket))), None), info:
            case (StockInput() | type()) as kind, msgspec.inspect.FloatType(ge=ge, gt=gt, le=le, lt=lt) | msgspec.inspect.IntType(ge=ge, gt=gt, le=le, lt=lt):
                low, high = bound(ge, gt), bound(le, lt)
            case (StockInput() | type()) as kind, _:
                pass
            case None, msgspec.inspect.BoolType():
                kind = bpy.types.NodeSocketBool
            case None, msgspec.inspect.IntType(ge=ge, gt=gt, le=le, lt=lt):
                kind, low, high = bpy.types.NodeSocketInt, bound(ge, gt), bound(le, lt)
            case None, msgspec.inspect.FloatType(ge=0.0, le=1.0):
                kind = bpy.types.NodeSocketFloatFactor
            case None, msgspec.inspect.FloatType(ge=ge, gt=gt, le=le, lt=lt):
                kind, low, high = bpy.types.NodeSocketFloat, bound(ge, gt), bound(le, lt)
            case None, msgspec.inspect.StrType():
                kind = bpy.types.NodeSocketString
            case None, msgspec.inspect.TupleType(item_types=(msgspec.inspect.FloatType(), msgspec.inspect.FloatType())):
                kind = bpy.types.NodeSocketVector2D
            case None, msgspec.inspect.TupleType(item_types=(msgspec.inspect.FloatType(), msgspec.inspect.FloatType(), msgspec.inspect.FloatType())):
                kind = bpy.types.NodeSocketVector
            case None, msgspec.inspect.EnumType(cls=enum):
                kind, items = bpy.types.NodeSocketMenu, MenuItems.of(enum).items
            case None, msgspec.inspect.LiteralType(values=values) if all(isinstance(held, str) for held in values):
                kind, items = bpy.types.NodeSocketMenu, tuple(MenuItem(name=str(held)) for held in values)
            case _:
                return Nothing
        return Some(cls(key=key, kind=kind, description=described, default_value=value, min_value=low, max_value=high, items=items))


class Interface(msgspec.Struct, frozen=True, kw_only=True):
    """Group interface items in Blender order."""

    outputs: tuple[Socket, ...] = ()
    inputs: tuple[Socket, ...] = ()
    panels: tuple["InterfacePanel", ...] = ()

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

        def entries(held: Interface, parent: Option[Key]) -> Iterator[tuple[Key, DesiredItem, Option[Key], int]]:
            items: tuple[DesiredItem, ...] = (*((socket, "OUTPUT") for socket in held.outputs), *((socket, "INPUT") for socket in held.inputs), *held.panels)
            first = int(held.toggle.is_some()) if isinstance(held, InterfacePanel) else 0
            return ((item.key if isinstance(item, InterfacePanel) else item[0].key, item, parent, position) for position, item in enumerate(items, first))

        pending = [entries(self, parent)]
        while pending:
            match next(pending[-1], None):
                case (key, InterfacePanel() as panel, parent, position):
                    yield key, panel, parent, position
                    pending.append(entries(panel, Some(key)))
                case tuple(entry):
                    yield entry
                case None:
                    pending.pop()

    def defaulted(self, values: Mapping[Key, Value], /) -> Self:
        """Returns the interface with the default of each socket at any depth `values` keys replaced."""
        return _mapped(self, lambda held: msgspec.structs.replace(held, default_value=values[held.key]) if held.key in values else held, lambda held: held)


class InterfacePanel(Interface, frozen=True, kw_only=True):
    """Interface panel with optional toggle, titled by its key unless named."""

    key: Key
    name: str | None = None
    description: str = ""
    closed: bool = False
    toggle: Option[bool] = Nothing

    @property
    def title(self) -> str:
        """Stated name, else the key's display name."""
        return bpy.path.display_name(self.key) if self.name is None else self.name

    @property
    def item(self) -> MenuItem:
        """Menu item selecting the panel, under its title and description."""
        return MenuItem(name=self.title, description=self.description)

    def socket(self, *path: object) -> Socket:
        """Returns the socket at a key path under the panel, record fields by member descriptor and case classes by class (`panel.socket(Clock.speed)`).

        Raises:
            LookupError: For a path the panel declares no socket at, at graph build.
        """
        match _found(self, item_key(self.key, *path)):
            case Option(tag="some", some=(Socket() as found, _)):
                return found
            case _:
                raise LookupError(f"{item_key(self.key, *path)} names no socket under {self.key}")

    def panel(self, *path: object) -> "InterfacePanel":
        """Returns the panel at a key path under the panel as `socket` reads one (a record panel a section panel nests), its keys as the interface holds them.

        Raises:
            LookupError: For a path the panel declares no panel at, at graph build.
        """
        match _found(self, item_key(self.key, *path)):
            case Option(tag="some", some=InterfacePanel() as found):
                return found
            case _:
                raise LookupError(f"{item_key(self.key, *path)} names no panel under {self.key}")

    @classmethod
    def of(cls, key: Key, source: Interface | msgspec.Struct, /, *, name: str | None = None, description: str | None = None, closed: bool = False, toggle: Option[bool] = Nothing) -> Self:
        """Builds the panel at `key` holding an interface's items, or the interface `fielded` derives from a record, every item key at any depth prefixed by `key` through `item_key` and every title kept.

        Args:
            key: Panel key and the prefix of every key it holds.
            source: Interface whose items the panel holds, or record whose fields it holds.
            name: Panel name, its key's display name by default.
            description: Panel description, a record's docstring by default.
            closed: Whether the panel starts closed.
            toggle: Default of the panel's toggle, `Nothing` for none.

        Returns:
            Panel of the prefixed items.
        """
        held, stated = (source, "") if isinstance(source, Interface) else (fielded(source), Option.of_optional(type(source).__doc__).map(inspect.cleandoc).default_value(""))
        prefixed = _mapped(
            held,
            lambda socket: msgspec.structs.replace(socket, key=item_key(key, socket.key), name=socket.title),
            lambda panel: msgspec.structs.replace(panel, key=item_key(key, panel.key), name=panel.title),
        )
        return cls(
            key=key, name=name, description=stated if description is None else description, closed=closed, toggle=toggle, outputs=prefixed.outputs, inputs=prefixed.inputs, panels=prefixed.panels
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def fielded(record: msgspec.Struct, /) -> Interface:
    """Builds the interface of a record's fields keyed by field name: a `Socketed` record's stated sockets, else a socket at its value for each field `Socket.of` takes and a panel of its fields for each field holding a record other than node content."""
    interfaces: dict[tuple[str, ...], Interface] = {}
    for owner, path, fields in _records(record, lambda held: not isinstance(held, _Socketed)):
        sockets = owner.sockets if isinstance(owner, _Socketed) else tuple(socket for field, value in fields for socket in Socket.of(field.name, field.type, value).to_list())  # ty: ignore[invalid-argument-type]
        interfaces[path] = Interface(
            inputs=tuple(msgspec.structs.replace(socket, key=item_key(*path, socket.key), name=socket.title) for socket in sockets) if path else sockets,
            panels=tuple(
                InterfacePanel(
                    key=item_key(*path, field.name),
                    name=bpy.path.display_name(field.name) if path else None,
                    description=inspect.cleandoc(type(value).__doc__ or ""),
                    **msgspec.structs.asdict(interfaces.pop((*path, field.name))),
                )
                for field, value in fields
                if _nested(value)
            ),
        )
    return interfaces[()]


def decoded[T: msgspec.Struct](record: T, values: Mapping[Key, Value], /, *path: object) -> Outcome[T]:
    """Decodes a record from socket values by field key, a nested record under its field's key, other fields keeping the record's.

    Args:
        record: Record whose values fields without a held value keep.
        values: Values by socket key (entry input values), a menu value as its item name.
        *path: Key path of the panel holding the record's sockets, empty for sockets `fielded` keys at the interface root.

    Returns:
        `Ok` of record of the held values. `Error` of every `invalid` value by field key, or a refused record construction.
    """
    if not values:
        return Ok(record)

    def held(info: msgspec.inspect.Type, value: Value) -> object:
        while isinstance(info, msgspec.inspect.Metadata):
            info = info.type
        match info, value:
            case msgspec.inspect.EnumType(cls=kind), str(name):
                return MenuItem.member(kind, name).map(lambda member: member.value).default_value(value)
            case _:
                return value

    def rebuilt[S: msgspec.Struct](owner: S, changes: Mapping[str, object]) -> Outcome[S]:
        if not changes:
            return Ok(owner)
        try:
            return Ok(msgspec.structs.replace(owner, **changes))
        except (TypeError, ValueError) as error:
            return Error(Fault.of(Reason.INVALID, type(owner).__name__, error))

    changes: dict[tuple[str, ...], Outcome[dict[str, object]]] = {}

    def changed(owner: msgspec.Struct, names: tuple[str, ...], field: msgspec.structs.FieldInfo, value: object) -> Outcome[tuple[tuple[str, object], ...]]:
        at = item_key(*path, *names, field.name)
        match value:
            case _ if _nested(value):
                return changes.pop((*names, field.name)).bind(lambda updates: rebuilt(value, updates).map(lambda found: ((field.name, found),)) if updates else Ok(()))
            case _ if at in values and Socket.of(field.name, field.type).is_some():
                return (
                    catch(msgspec.convert, exception=msgspec.ValidationError)(held(msgspec.inspect.type_info(field.type), values[at]), field.type)
                    .map_error(functools.partial(Fault.of, Reason.INVALID, type(owner).__name__, member=at))
                    .map(lambda found: ((field.name, found),))
                )
            case _:
                return Ok(())

    for owner, names, fields in _records(record, lambda _: True):
        changes[names] = accumulated(starmap(functools.partial(changed, owner, names), fields)).map(lambda rows: dict(chain.from_iterable(rows)))
    return changes[()].bind(functools.partial(rebuilt, record))


def _records(record: msgspec.Struct, descend: Callable[[msgspec.Struct], bool], /) -> Iterator[tuple[msgspec.Struct, tuple[str, ...], tuple[tuple[msgspec.structs.FieldInfo, object], ...]]]:
    """Reads actual record occurrences bottom up, snapshotting each field and value once."""

    def entered(owner: msgspec.Struct, path: tuple[str, ...]) -> tuple[msgspec.Struct, tuple[str, ...], tuple[tuple[msgspec.structs.FieldInfo, object], ...], Iterator[tuple[str, msgspec.Struct]]]:
        fields = tuple((field, getattr(owner, field.name)) for field in msgspec.structs.fields(owner)) if descend(owner) else ()
        return owner, path, fields, iter((field.name, value) for field, value in fields if _nested(value))

    pending = [entered(record, ())]
    while pending:
        owner, path, fields, children = pending[-1]
        match next(children, None):
            case (name, child):
                pending.append(entered(child, (*path, name)))
            case None:
                pending.pop()
                yield owner, path, fields


def _mapped[I: Interface](interface: I, socket: Callable[[Socket], Socket], panel: Callable[[InterfacePanel], InterfacePanel], /) -> I:
    """Transforms sockets and panels bottom up, preserving root type and every other field."""

    def rebuilt[S: Interface](owner: S, children: list[InterfacePanel]) -> S:
        return msgspec.structs.replace(owner, outputs=tuple(map(socket, owner.outputs)), inputs=tuple(map(socket, owner.inputs)), panels=tuple(children))

    panels: list[InterfacePanel] = []
    pending: list[tuple[Option[InterfacePanel], Iterator[InterfacePanel], list[InterfacePanel]]] = [(Nothing, iter(interface.panels), panels)]
    while pending:
        parent, children, ready = pending[-1]
        match next(children, None):
            case InterfacePanel() as child:
                pending.append((Some(child), iter(child.panels), []))
            case None:
                pending.pop()
                for child in parent.to_list():
                    pending[-1][2].append(panel(rebuilt(child, ready)))
    return rebuilt(interface, panels)


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

        def linked(tree: bpy.types.NodeTree) -> Outcome[bpy.types.NodeTree]:
            scratch.callback(bpy.data.node_groups.remove, tree)
            entered, node = tree.nodes.new(_definition(bpy.types.NodeGroupInput).identifier), tree.nodes.new(_definition(stock.node).identifier)
            return _socket_at(node.inputs, stock.input).map(lambda target: tree.links.new(entered.outputs[-1], target, handle_dynamic_sockets=True)).map(lambda _: tree)

        match _new_group(kinds[0], ".stock").bind(linked):
            case Result(tag="ok", ok=held):
                items = tuple(item for item in _interface_of(held).items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket))
            case Result(error=faults):
                raise LookupError(stock, faults)
        match items:
            case (item,):
                refined: type[bpy.types.NodeSocket] = getattr(bpy.types, item.bl_socket_idname)  # ty: ignore[unsound-assignment]

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
    return Option.of_optional(next((item for held, item, _, _ in panel.walked() if held == key), None))


def _nested(value: object, /) -> TypeGuard[msgspec.Struct]:
    """Whether a record field's value is a record whose fields a nested panel holds, node content a node holds in place of sockets excluded."""
    return isinstance(value, msgspec.Struct) and not isinstance(value, Content)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DesiredItem", "Interface", "InterfacePanel", "Socket", "StockInput", "decoded", "fielded"]
