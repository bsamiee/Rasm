"""Desired group interfaces, record interfaces, curve node markers, stored item keys, and live interface writes."""

from collections.abc import Callable, Iterable, Iterator, Mapping
import itertools
from types import MemberDescriptorType
from typing import Literal, Self

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import msgspec.structs

from blender.bpy.data.rna.descriptors import assigned, Descriptor, differing, Value, written
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.settings import shapes
from blender.bpy.settings.properties import RecordGroup, Unit
from blender.records.faults import accumulated
from blender.records.fields import Case, Choice, Gated, heading, markers, Presentation, Quantity

# --- [TYPES] ----------------------------------------------------------------------------

type SocketSide = Literal["INPUT", "OUTPUT"]
type InterfaceItem = bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel
type DesiredItem = tuple[Socket, SocketSide] | InterfacePanel

# --- [MODELS] ---------------------------------------------------------------------------


class MenuItem(msgspec.Struct, frozen=True, kw_only=True):
    """Menu Switch item."""

    name: str
    description: str = ""


class MenuItems(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered Menu Switch items."""

    items: tuple[MenuItem, ...]

    @classmethod
    def of(cls, choices: type[Choice] | tuple[type[Case], ...]) -> Self:
        """Builds items from choice members described by their choice text, or from tagged cases described by docstrings."""
        return (
            cls(items=tuple(MenuItem(name=case.title, description=Option.of_optional(case.__doc__).default_value("")) for case in choices))
            if isinstance(choices, tuple)
            else cls(items=tuple(MenuItem(name=member.label, description=member.description.default_value("")) for member in choices))
        )


class Socket(msgspec.Struct, frozen=True, kw_only=True):
    """Interface socket by parameter key, group input menu items kept only while linked to a Menu Switch."""

    key: Key
    name: str
    kind: type[bpy.types.NodeSocket]
    description: str = ""
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)
    items: Option[MenuItems] = Nothing


class Interface(msgspec.Struct, frozen=True, kw_only=True):
    """Group interface items in Blender order."""

    outputs: tuple[Socket, ...] = ()
    inputs: tuple[Socket, ...] = ()
    panels: tuple["InterfacePanel", ...] = ()

    def walked(self, parent: Option[Key] = Nothing) -> Iterator[tuple[Key, DesiredItem, Option[Key], int]]:
        """Walks interface items in preorder.

        Args:
            parent: Key of panel holding items, `Nothing` at root.

        Yields:
            Item key, socket with side or panel, parent key, and position after toggle socket a toggled panel holds first.
        """
        items: tuple[DesiredItem, ...] = (*((socket, "OUTPUT") for socket in self.outputs), *((socket, "INPUT") for socket in self.inputs), *self.panels)
        for position, item in enumerate(items, int(isinstance(self, InterfacePanel) and self.toggle.is_some())):
            match item:
                case InterfacePanel(key=key):
                    yield key, item, parent, position
                    yield from item.walked(Some(key))
                case (socket, _):
                    yield socket.key, item, parent, position

    @classmethod
    def of(cls, kind: type[msgspec.Struct]) -> BlenderResult["Interface"]:
        """Builds record input interface from field types."""
        refused = {"DISTANCE_CAMERA": "NONE", "POWER": "NONE", "XYZ_LENGTH": "TRANSLATION"}

        def faced(parts: Iterable[tuple[Socket | InterfacePanel, ...]]) -> Interface:
            items = tuple(itertools.chain.from_iterable(parts))
            return cls(inputs=tuple(item for item in items if isinstance(item, Socket)), panels=tuple(item for item in items if isinstance(item, InterfacePanel)))

        def walk(path: tuple[Key | type[Case], ...], record: shapes.Record, held: Option[object]) -> BlenderResult[Interface]:
            return shapes.field_shapes(record).bind(
                lambda rows: accumulated(
                    entry(
                        (*path, field.name),
                        field,
                        shape,
                        lambda: held.map(lambda value: getattr(value, field.name)).or_else_with(
                            lambda: Some(field.default) if field.default is not msgspec.NODEFAULT else Some(field.default_factory()) if field.default_factory is not msgspec.NODEFAULT else Nothing
                        ),
                        heading(field.name),
                        "",
                    )
                    for field, shape in rows
                ).map(faced)
            )

        def panel(path: tuple[Key | type[Case], ...], name: str, description: str, toggle: Option[bool], inner: BlenderResult[Interface]) -> BlenderResult[tuple[Socket | InterfacePanel, ...]]:
            return inner.map(lambda face: (InterfacePanel(key=item_key(*path), name=name, description=description, toggle=toggle, inputs=face.inputs, panels=face.panels),))

        def entry(
            path: tuple[Key | type[Case], ...], field: msgspec.structs.FieldInfo, declared: shapes.Declared, default: Callable[[], Option[object]], fallback: str, summary: str
        ) -> BlenderResult[tuple[Socket | InterfacePanel, ...]]:
            title = declared.title.default_value(fallback)
            description = declared.description.default_value(summary)

            def socket(
                kind: type[bpy.types.NodeSocket],
                owner: type[bpy.types.NodeTreeInterfaceSocket],
                members: BlenderResult[Mapping[str, Value]],
                address: str = item_key(*path),
                name: str = title,
                items: Option[MenuItems] = Nothing,
                description: str = description,
            ) -> BlenderResult[tuple[Socket | InterfacePanel, ...]]:
                return members.bind(
                    lambda values: accumulated(Descriptor.resolve(owner, identifier, bpy.types.Property) for identifier in values).map(
                        lambda resolved: (Socket(key=address, name=name, kind=kind, description=description, members=dict(zip(resolved, values.values())), items=items),)
                    )
                )

            def given(lowered: Callable[[object], BlenderResult[Value]], members: Mapping[str, Value] = {}) -> BlenderResult[dict[str, Value]]:
                return default().map(lambda value: lowered(value).map(lambda result: {**members, "default_value": result})).default_value(Ok(dict(members)))

            shown = Presentation.of(field.type)
            match declared.shape:
                case shapes.Number(kind=number):
                    unit = Unit.of(field.type, number)

                    def numbered(bounds: tuple[float, float]) -> BlenderResult[dict[str, Value]]:
                        low, high, spanned = unit.spanned(shown, bounds)
                        return given(lambda value: unit.stored(value, number), {"subtype": refused.get(spanned.number, spanned.number), "min_value": low, "max_value": high})

                    return socket(bpy.types.NodeSocketFloat, bpy.types.NodeTreeInterfaceSocketFloat, unit.bounds(number).bind(numbered))
                case shapes.Integer(kind=whole):
                    low, high, counted = Unit.of(field.type, whole).counted(shown, whole)
                    return socket(
                        bpy.types.NodeSocketInt,
                        bpy.types.NodeTreeInterfaceSocketInt,
                        given(lambda value: Ok(msgspec.convert(value, int)), {"subtype": counted.number, "min_value": low, "max_value": high}),
                    )
                case shapes.Vector(kind=tupled):
                    unit = Unit.of(field.type, tupled)

                    def stored(value: object) -> BlenderResult[Value]:
                        return unit.stored(value, tupled)

                    def directed(bounds: tuple[float, float]) -> BlenderResult[dict[str, Value]]:
                        low, high, spanned = unit.spanned(shown, bounds)
                        return given(stored, {"dimensions": len(tupled.item_types), "subtype": refused.get(spanned.vector, spanned.vector), "min_value": low, "max_value": high})

                    if len(tupled.item_types) == 3 and shown.quantity == Some(Quantity.ANGLE):
                        return socket(bpy.types.NodeSocketRotation, bpy.types.NodeTreeInterfaceSocketRotation, given(stored))
                    return socket(bpy.types.NodeSocketVector, bpy.types.NodeTreeInterfaceSocketVector, unit.bounds(tupled).bind(directed))
                case shapes.Components(items=items):
                    return panel(
                        path,
                        title,
                        description,
                        Nothing,
                        accumulated(
                            entry((*path, str(index)), component, item, lambda: Some(component.default) if component.default is not msgspec.NODEFAULT else Nothing, f"{title} {index}", description)
                            for index, (component, item) in enumerate(zip(RecordGroup.components(msgspec.structs.replace(field, default=default().default_value(msgspec.NODEFAULT))), items), 1)
                        ).map(faced),
                    )
                case shapes.Flag():
                    return socket(bpy.types.NodeSocketBool, bpy.types.NodeTreeInterfaceSocketBool, given(lambda value: Ok(msgspec.convert(value, bool))))
                case shapes.Color(kind=color):
                    return socket(
                        bpy.types.NodeSocketColor,
                        bpy.types.NodeTreeInterfaceSocketColor,
                        given(lambda value: Unit.of(field.type, color).stored(msgspec.to_builtins(value), color).map(lambda channels: (*channels, 1.0))),
                    )
                case shapes.Text(space=True):
                    return Ok(())
                case shapes.Text():
                    return socket(bpy.types.NodeSocketString, bpy.types.NodeTreeInterfaceSocketString, given(lambda value: Ok(msgspec.convert(value, str))))
                case shapes.Enumerated(enum=choices):
                    return socket(bpy.types.NodeSocketMenu, bpy.types.NodeTreeInterfaceSocketMenu, given(lambda value: Ok(msgspec.convert(value, choices).label)), items=Some(MenuItems.of(choices)))
                case shapes.Literals() as literal:
                    return socket(
                        bpy.types.NodeSocketMenu,
                        bpy.types.NodeTreeInterfaceSocketMenu,
                        given(lambda value: Ok(literal.labels[msgspec.convert(value, int | str)])),
                        items=Some(MenuItems(items=tuple(MenuItem(name=label) for label in literal.labels.values()))),
                    )
                case shapes.Flags(enum=choices):
                    selected = default()
                    return accumulated(
                        socket(
                            bpy.types.NodeSocketBool,
                            bpy.types.NodeTreeInterfaceSocketBool,
                            Ok(selected.map(lambda values: {"default_value": member in msgspec.convert(values, frozenset[object])}).default_value({})),
                            item_key(*path, member.name),
                            member.label,
                            description=member.description.default_value(description),
                        )
                        for member in choices
                    ).map(lambda parts: tuple(itertools.chain.from_iterable(parts)))
                case shapes.Gate(value=value):
                    gate = default().bind(lambda chosen: Some(chosen) if isinstance(chosen, Gated) else Nothing)
                    held = gate.map(lambda chosen: chosen.value)

                    def valued(row: msgspec.structs.FieldInfo, inner: shapes.Declared) -> BlenderResult[Interface]:
                        match inner.shape:
                            case shapes.Nested(record=record):
                                return walk((*path, row.name), record, held)
                            case _:
                                return entry((*path, row.name), row, shapes.Declared(shape=inner.shape), lambda: held, title, description).map(lambda items: faced((items,)))

                    return panel(path, title, description, Some(gate.map(lambda chosen: chosen.enabled).default_value(False)), value.map(lambda pair: valued(*pair)).default_value(Ok(cls())))
                case shapes.Nested(record=record):
                    return panel(path, title, Option.of_optional(record.__doc__).default_value(""), Nothing, walk(path, record, default()))
                case shapes.Tagged(cases=cases):
                    selected = default()
                    tagged = selected.bind(lambda value: Some(value) if isinstance(value, Case) else Nothing)
                    menu = socket(
                        bpy.types.NodeSocketMenu,
                        bpy.types.NodeTreeInterfaceSocketMenu,
                        Ok(tagged.map(lambda value: {"default_value": type(value).title}).default_value({})),
                        items=Some(MenuItems.of(cases)),
                    )

                    def branch(record: type[Case]) -> BlenderResult[tuple[Socket | InterfacePanel, ...]]:
                        chosen = selected.filter(lambda value: isinstance(value, record))
                        return panel((*path, record), record.title, Option.of_optional(record.__doc__).default_value(""), Nothing, walk((*path, record), record, chosen))

                    return accumulated((menu, *(branch(record) for record in cases))).map(lambda parts: tuple(itertools.chain.from_iterable(parts)))
                case shapes.Records():
                    return panel(path, title, description, Nothing, Ok(cls()))
                case shapes.OptionalText() | shapes.Entries() | shapes.Pointer():
                    return shapes.refused(field.type, declared.shape)

        return walk((), kind, Nothing)


class InterfacePanel(Interface, frozen=True, kw_only=True):
    """Interface panel with optional toggle."""

    key: Key
    name: str
    description: str = ""
    closed: bool = False
    toggle: Option[bool] = Nothing


class Withdrawn(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item interface drops or redeclares."""

    key: Key


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Interface item to create."""

    key: Key
    entry: DesiredItem


class Restated(msgspec.Struct, frozen=True, kw_only=True):
    """Item parts and place to write."""

    key: Key
    kind: Option[type[bpy.types.NodeSocket]] = Nothing
    name: Option[str] = Nothing
    description: Option[str] = Nothing
    closed: Option[bool] = Nothing
    toggle: Option[bool] = Nothing
    place: Option[tuple[Option[Key], int]] = Nothing
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)


class CurveAxes(msgspec.Struct, frozen=True, kw_only=True):
    """Record curve placed in its node's own axes, the shown height `scale * y + slope * x + offset`."""

    scale: float = 1.0
    slope: float = 0.0
    offset: float = 0.0

    def shown(self, x: float, y: float) -> float:
        """Returns node height of record point."""
        return self.scale * y + self.slope * x + self.offset

    def held(self, x: float, y: float) -> float:
        """Returns record height of node point."""
        return (y - self.slope * x - self.offset) / self.scale


class CurveNode(msgspec.Struct, frozen=True, kw_only=True):
    """Annotation marker of a knot field the native curve node of `node` holds, fields marking one node class its curves in declaration order.

    `view` is the node's own view in place of the record bounds.
    """

    node: type[bpy.types.Node]
    axes: CurveAxes = CurveAxes()
    view: Option[tuple[float, float, float, float]] = Nothing

    @staticmethod
    def marked(record: type[msgspec.Struct]) -> tuple[tuple[msgspec.structs.FieldInfo, "CurveNode", int], ...]:
        """Returns each marked field with its marker and its curve index among the fields marking the same node class."""
        rows = tuple((field, marker) for field in msgspec.structs.fields(record) for marker in markers(field.type, CurveNode)[:1])
        return tuple((field, marker, sum(prior.node is marker.node for _, prior in rows[:index])) for index, (field, marker) in enumerate(rows))


# --- [SERVICES] -------------------------------------------------------------------------


class Addressed(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item keys of one group read once against its live interface items."""

    stored: dict[Key, str | int]
    live: dict[str | int, InterfaceItem]

    @classmethod
    def of(cls, tree: bpy.types.NodeTree, slot: str) -> Self:
        """Reads item identifiers by stored key, list entries holding keys past 63-byte ID property name limit, and live sockets and panels."""
        live = {item.identifier: item for item in tree.interface.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel)}  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        return cls(stored={row["key"]: row["identifier"] for row in Option.of_optional(tree.get(slot)).filter(lambda value: isinstance(value, list)).default_value([])}, live=live)

    @property
    def resolved(self) -> dict[Key, InterfaceItem]:
        """Live items by stored key, removed items left out."""
        return {key: self.live[identifier] for key, identifier in self.stored.items() if identifier in self.live}

    @property
    def keys(self) -> dict[str | int, Key]:
        """Stored keys by item identifier."""
        return {identifier: key for key, identifier in self.stored.items()}

    @property
    def sockets(self) -> dict[Key, str]:
        """Live socket identifiers by stored key, panel keys mapping to their toggle sockets."""
        return {key: socket.identifier for key, item in self.resolved.items() for socket in (Some(item) if isinstance(item, bpy.types.NodeTreeInterfaceSocket) else _toggle(item)).to_list()}

    def kept(self, interface: Interface) -> dict[Key, InterfaceItem]:
        """Live items matching item key, kind, side, and toggle presence of `interface`."""
        resolved = self.resolved

        def matched(entry: DesiredItem, item: InterfaceItem) -> bool:
            match entry, item:
                case (Socket(), side), bpy.types.NodeTreeInterfaceSocket():
                    return item.in_out == side
                case InterfacePanel(toggle=toggle), bpy.types.NodeTreeInterfacePanel():
                    return toggle.is_some() == _toggle(item).is_some()
                case _:
                    return False

        return {key: item for key, entry, _, _ in interface.walked() for item in (resolved.get(key),) if item is not None and matched(entry, item)}


# --- [OPERATIONS] -----------------------------------------------------------------------


def item_key(*parts: Key | int | type[Case] | MemberDescriptorType) -> Key:
    """Joins key path into item key, record fields by their member descriptor's name and case classes as lowercase names."""

    def part(value: Key | int | type[Case] | MemberDescriptorType) -> str:
        match value:
            case MemberDescriptorType():
                return value.__name__
            case type():
                return value.__name__.lower()
            case str() | int():
                return str(value)

    return "_".join(map(part, parts))


def under[I: Interface](prefix: Key, face: I) -> I:
    """Returns interface with every key prefixed through `item_key`, for a group nested in another group's panel."""
    return msgspec.structs.replace(
        face,
        outputs=tuple(msgspec.structs.replace(socket, key=item_key(prefix, socket.key)) for socket in face.outputs),
        inputs=tuple(msgspec.structs.replace(socket, key=item_key(prefix, socket.key)) for socket in face.inputs),
        panels=tuple(msgspec.structs.replace(under(prefix, panel), key=item_key(prefix, panel.key)) for panel in face.panels),
    )


def slot(identity: Identity) -> BlenderResult[str]:
    """Returns ID property name holding group item keys, or refused attachment name fault."""
    return identity.name(Name.ATTACHMENT, "interface")


def addresses(tree: bpy.types.NodeTree, slot: str) -> dict[Key, str]:
    """Reads live socket identifiers by item key, panel keys mapping to toggle sockets."""
    return Addressed.of(tree, slot).sockets


def interfaced(tree: bpy.types.NodeTree, interface: Interface, slot: str) -> tuple[tuple[Withdrawn, ...], tuple[Declared, ...], tuple[Restated, ...]]:
    """Reads live interface against desired one, writing nothing.

    Args:
        tree: Group to read.
        interface: Interface group declares.
        slot: ID property name holding stored item keys.

    Returns:
        Items to withdraw last first, items to declare, and differing parts per item, siblings of each new or misplaced item placed again in preorder.
    """
    entries = tuple(interface.walked())
    addressed = Addressed.of(tree, slot)
    inverse = addressed.keys
    held = addressed.kept(interface)
    wanted = frozenset(key for key, *_ in entries)
    fresh = wanted - held.keys()
    placed = {key: (Option.of_optional(item.parent).bind(lambda panel: Option.of_optional(inverse.get(panel.identifier))), item.position) for key, item in held.items()}
    shifted = frozenset(parent for key, _, parent, position in entries if key in fresh or placed.get(key) != (parent, position) or any(frame in fresh for frame in parent.to_list()))

    def restated(key: Key, entry: DesiredItem, place: Option[tuple[Option[Key], int]]) -> Option[Restated]:
        def changed[T](now: T, wanted: T) -> Option[T]:
            return Nothing if now == wanted else Some(wanted)

        match entry, held.get(key):
            case (Socket() as socket, _), bpy.types.NodeTreeInterfaceSocket() as item:
                kind = Nothing if item.socket_type == socket.kind.__name__ else Some(socket.kind)
                row = Restated(
                    key=key,
                    kind=kind,
                    name=changed(item.name, socket.name),
                    description=changed(item.description, socket.description),
                    place=place,
                    members=kind.map(lambda _: socket.members).default_with(lambda: differing(item, socket.members)),
                )
            case InterfacePanel() as panel, bpy.types.NodeTreeInterfacePanel() as item:
                row = Restated(
                    key=key,
                    name=changed(item.name, panel.name),
                    description=changed(item.description, panel.description),
                    closed=changed(item.default_closed, panel.closed),
                    toggle=panel.toggle.filter(lambda value: all(differing(toggle, {"default_value": value}) for toggle in _toggle(item).to_list())),
                    place=place,
                )
            case (Socket() as socket, _), _:
                row = Restated(key=key, place=place, members=socket.members)
            case _:
                row = Restated(key=key, place=place)
        return Some(row).filter(lambda differs: differs != Restated(key=key))

    return (
        tuple(Withdrawn(key=key) for identifier in reversed(addressed.live) for key in Option.of_optional(inverse.get(identifier)).to_list() if key not in wanted or key in fresh),
        tuple(Declared(key=key, entry=entry) for key, entry, _, _ in entries if key in fresh),
        tuple(row for key, entry, parent, position in entries for row in restated(key, entry, Some((parent, position)) if parent in shifted else Nothing).to_list()),
    )


def revised(row: Withdrawn | Declared | Restated, tree: bpy.types.NodeTree, slot: str) -> BlenderResult[None]:
    """Writes one interface row onto a group, reading live items again after each write that replaces them.

    Args:
        row: Interface row to write.
        tree: Group row names.
        slot: ID property name holding stored item keys.

    Returns:
        `Ok` once written. `Error` of `absent` for unresolved item, `edit` per refused socket type or write.
    """
    face: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    def stored(items: dict[Key, str | int]) -> None:
        tree[slot] = [{"key": key, "identifier": identifier} for key, identifier in items.items()]

    def item[T: bpy.types.NodeTreeInterfaceItem](key: Key, kind: type[T]) -> BlenderResult[T]:
        return (
            Option
            .of_optional(Addressed.of(tree, slot).resolved.get(key))
            .bind(lambda each: Some(each) if isinstance(each, kind) else Nothing)
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(face), key=key),))
        )

    def withdrawn(held: bpy.types.NodeTreeInterfaceItem, key: Key) -> None:
        for toggle in _toggle(held).to_list():
            face.remove(toggle)
        face.remove(held)
        stored({name: identifier for name, identifier in Addressed.of(tree, slot).stored.items() if name != key})

    def refused(error: TypeError) -> BlenderFaults:
        return (BlenderEditFault(request=repr(face), error=error),)

    def made(key: Key, entry: DesiredItem) -> BlenderResult[None]:
        def toggled(panel: bpy.types.NodeTreeInterfacePanel, *, value: bool) -> BlenderResult[None]:
            return (
                catch(lambda: face.new_socket(panel.name, in_out="INPUT", socket_type=bpy.types.NodeSocketBool.__name__), exception=TypeError)()  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                .bind(lambda socket: catch(face.move_to_parent, exception=TypeError)(socket, panel, 0).map(lambda _: socket))
                .map_error(refused)
                .bind(lambda socket: accumulated((written(socket, "is_panel_toggle", True), written(socket, "default_value", value))).map(lambda _: None))
            )

        def created() -> BlenderResult[InterfaceItem]:
            match entry:
                case tuple(pair):
                    socket, side = pair
                    return catch(lambda: face.new_socket(socket.name, description=socket.description, in_out=side, socket_type=socket.kind.__name__), exception=TypeError)().map_error(refused)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                case InterfacePanel(name=name, description=description, closed=closed, toggle=toggle):
                    return (
                        catch(lambda: face.new_panel(name, description=description, default_closed=closed), exception=TypeError)()
                        .map_error(refused)
                        .bind(lambda panel: accumulated(toggled(panel, value=value) for value in toggle.to_list()).map(lambda _: panel))
                    )

        return created().map(lambda declared: stored(Addressed.of(tree, slot).stored | {key: declared.identifier}))

    def restated(change: Restated) -> BlenderResult[None]:
        def retyped(held: bpy.types.NodeTreeInterfaceItem) -> BlenderResult[bpy.types.NodeTreeInterfaceItem]:
            return change.kind.map(lambda kind: written(held, "socket_type", kind.__name__).bind(lambda _: item(change.key, bpy.types.NodeTreeInterfaceItem))).default_value(Ok(held))

        def moved(held: bpy.types.NodeTreeInterfaceItem, parent: Option[Key], position: int) -> BlenderResult[None]:
            return (
                parent
                .map(lambda key: item(key, bpy.types.NodeTreeInterfacePanel).map(Some))
                .default_value(Ok(Nothing))
                .bind(lambda panel: catch(face.move_to_parent, exception=TypeError)(held, panel.to_optional(), position).map_error(refused))
            )

        def rest(held: bpy.types.NodeTreeInterfaceItem) -> BlenderResult[None]:
            return accumulated((
                *(written(held, "name", name) for name in change.name.to_list()),
                *(written(held, "description", text) for text in change.description.to_list()),
                *(written(held, "default_closed", closed) for closed in change.closed.to_list()),
                *(written(toggle, "default_value", value) for value in change.toggle.to_list() for toggle in _toggle(held).to_list()),
                *(moved(held, *place) for place in change.place.to_list()),
                assigned(held, change.members),
            )).map(lambda _: None)

        return item(change.key, bpy.types.NodeTreeInterfaceItem).bind(retyped).bind(rest)

    match row:
        case Withdrawn(key=key):
            return item(key, bpy.types.NodeTreeInterfaceItem).map(lambda held: withdrawn(held, key))
        case Declared(key=key, entry=entry):
            return made(key, entry)
        case Restated():
            return restated(row)


def _toggle(item: bpy.types.NodeTreeInterfaceItem) -> Option[bpy.types.NodeTreeInterfaceSocket]:
    """Panel toggle socket, `Nothing` for items without one."""
    match item:
        case bpy.types.NodeTreeInterfacePanel():
            return Option.of_optional(next((child for child in item.interface_items if isinstance(child, bpy.types.NodeTreeInterfaceSocket) and child.is_panel_toggle), None))
        case _:
            return Nothing


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Addressed",
    "CurveAxes",
    "CurveNode",
    "Declared",
    "DesiredItem",
    "Interface",
    "InterfaceItem",
    "InterfacePanel",
    "MenuItem",
    "MenuItems",
    "Restated",
    "Socket",
    "SocketSide",
    "Withdrawn",
    "addresses",
    "interfaced",
    "item_key",
    "revised",
    "slot",
    "under",
]
