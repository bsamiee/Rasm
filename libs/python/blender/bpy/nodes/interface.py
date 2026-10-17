"""Group interfaces: desired sockets with their menu items and panels, record interfaces, stored item keys, and their native diff and writes."""

from collections.abc import Callable, Iterable, Iterator, Mapping
from enum import Enum
import itertools
from typing import Literal, Self

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import idprop
import msgspec
import msgspec.structs

from blender.bpy.data.rna.descriptors import assigned, Descriptor, differing, holds, Value, written
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.settings import shapes
from blender.bpy.settings.properties import RecordGroup, Unit
from blender.imaging.faults import accumulated
from blender.imaging.parameters import Case, Choice, Gated, heading, markers, Presentation, Quantity

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
    """Ordered items of a Menu Switch."""

    items: tuple[MenuItem, ...]

    @classmethod
    def of(cls, choices: type[Enum] | tuple[type[Case], ...], description: str = "") -> Self:
        """Constructs native node-menu items from enum members sharing a description or tagged-union cases described by their docstrings."""
        return (
            cls(items=tuple(MenuItem(name=case.title, description=Option.of_optional(case.__doc__).default_value("")) for case in choices))
            if isinstance(choices, tuple)
            else cls(items=tuple(MenuItem(name=member.label if isinstance(member, Choice) else member.name, description=description) for member in choices))
        )


class Socket(msgspec.Struct, frozen=True, kw_only=True):
    """Interface socket keyed by its parameter key, a menu socket with the items a group input holds only while linked to a Menu Switch."""

    key: Key
    name: str
    kind: type[bpy.types.NodeSocket]
    description: str = ""
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)
    items: Option[MenuItems] = Nothing


class Interface(msgspec.Struct, frozen=True, kw_only=True):
    """Group interface of output sockets, input sockets, and panels in Blender's order."""

    outputs: tuple[Socket, ...] = ()
    inputs: tuple[Socket, ...] = ()
    panels: tuple["InterfacePanel", ...] = ()

    def walked(self, parent: Option[Key] = Nothing) -> Iterator[tuple[Key, DesiredItem, Option[Key], int]]:
        """Walks the interface items preorder.

        Args:
            parent: Key of the panel holding these items, `Nothing` at the root.

        Yields:
            Each item's key, the item (a socket paired with its side, or a panel), its parent key, and its native position under its parent, after the toggle socket Blender keeps first in a toggled panel.
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
        """Constructs a record's input interface from native fields and their inspected types."""
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
                    return socket(
                        bpy.types.NodeSocketMenu,
                        bpy.types.NodeTreeInterfaceSocketMenu,
                        given(lambda value: Ok(choice.label if isinstance(choice := choices(value), Choice) else choice.name)),
                        items=Some(MenuItems.of(choices, described(field.type))),
                    )
                case shapes.Flags(enum=choices):
                    selected = default()
                    return accumulated(
                        socket(
                            bpy.types.NodeSocketBool,
                            bpy.types.NodeTreeInterfaceSocketBool,
                            Ok(selected.map(lambda values: {"default_value": member in msgspec.convert(values, frozenset[object])}).default_value({})),
                            item_key(*path, member.name),
                            member.label if isinstance(member, Choice) else member.name,
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
                        items=Some(MenuItems.of(tuple(record for record, _ in cases))),
                    )

                    def branch(record: type[Case]) -> BlenderResult[tuple[Socket | InterfacePanel, ...]]:
                        chosen = selected.filter(lambda value: isinstance(value, record))
                        return panel((*path, record), record.title, Option.of_optional(record.__doc__).default_value(""), Nothing, walk((*path, record), record, chosen))

                    return accumulated((menu, *(branch(record) for record, _ in cases))).map(lambda parts: tuple(itertools.chain.from_iterable(parts)))
                case shapes.Records():
                    return panel(path, title, description, Nothing, Ok(cls()))
                case shapes.OptionalText() | shapes.Literals() | shapes.Entries() | shapes.Pointer():
                    return shapes.refused(field.type, declared.shape)

        return walk((), kind, Nothing)


class InterfacePanel(Interface, frozen=True, kw_only=True):
    """Interface panel holding its own sockets and panels, with an optional enable toggle."""

    key: Key
    name: str
    description: str = ""
    closed: bool = False
    toggle: Option[bool] = Nothing


class Withdrawn(msgspec.Struct, frozen=True, kw_only=True):
    """Stored interface item the interface no longer declares as it stands."""

    key: Key


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Interface item to create from its desired entry."""

    key: Key
    entry: DesiredItem


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


# --- [OPERATIONS] -----------------------------------------------------------------------


def described(annotation: object) -> str:
    """Returns the outermost `msgspec.Meta` description of an annotation, empty for none as native item text holds absence."""
    return next((meta.description for meta in reversed(markers(annotation, msgspec.Meta)) if meta.description is not None), "")


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


def slot(identity: Identity) -> BlenderResult[str]:
    """Returns the ID property name a group stores its item keys under, or the refused attachment name."""
    return identity.name(Name.ATTACHMENT, "interface")


def addresses(tree: bpy.types.NodeTree, slot: str) -> dict[Key, str]:
    """Reads a group's item key to live socket identifier map, a panel key answering its toggle socket."""
    return {key: socket.identifier for key, item in _resolved(tree, slot).items() for socket in (Some(item) if isinstance(item, bpy.types.NodeTreeInterfaceSocket) else _toggle(item)).to_list()}


def kept(tree: bpy.types.NodeTree, interface: Interface, slot: str) -> dict[Key, InterfaceItem]:
    """Live items of a group that keep an interface item's key, kind, side, and toggle presence."""
    resolved = _resolved(tree, slot)
    return {key: item for key, entry, _, _ in interface.walked() for item in (resolved.get(key),) if item is not None and _matched(entry, item)}


def interfaced(tree: bpy.types.NodeTree, interface: Interface, slot: str) -> tuple[tuple[Withdrawn, ...], tuple[Declared, ...], tuple[Restated, ...]]:
    """Reads a group's live interface against a desired one, writing nothing.

    Args:
        tree: Group whose interface the rows read.
        interface: Interface the group declares.
        slot: ID property name of the stored item keys.

    Returns:
        Items to withdraw (last first), items to declare, and differing parts of every item, each item of a panel holding a
        fresh or misplaced item placed again in preorder, since a move shifts its siblings.
    """
    entries = tuple(interface.walked())
    inverse = {identifier: key for key, identifier in _items(tree, slot).items()}
    held = kept(tree, interface, slot)
    wanted = frozenset(key for key, *_ in entries)
    fresh = wanted - held.keys()
    placed = {key: (Option.of_optional(item.parent).bind(lambda panel: Option.of_optional(inverse.get(panel.identifier))), item.position) for key, item in held.items()}
    unsettled = frozenset(parent for key, _, parent, position in entries if key in fresh or placed.get(key) != (parent, position) or any(frame in fresh for frame in parent.to_list()))
    return (
        tuple(Withdrawn(key=key) for identifier in reversed(_live(tree)) for key in Option.of_optional(inverse.get(identifier)).to_list() if key not in wanted or key in fresh),
        tuple(Declared(key=key, entry=entry) for key, entry, _, _ in entries if key in fresh),
        tuple(
            row for key, entry, parent, position in entries for row in _restated(key, entry, Some((parent, position)) if parent in unsettled else Nothing, Option.of_optional(held.get(key))).to_list()
        ),
    )


def revised(row: Withdrawn | Declared | Restated, tree: bpy.types.NodeTree, slot: str) -> BlenderResult[None]:
    """Writes one interface row onto a group.

    Args:
        row: Interface row to write.
        tree: Group the row names.
        slot: ID property name of the stored item keys.

    Returns:
        `Ok` once written; `Error` of `absent` for an unresolved item, of `edit` for a socket type the tree refuses, or of each refused write.
    """
    face: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    def item[T: bpy.types.NodeTreeInterfaceItem](key: Key, kind: type[T]) -> BlenderResult[T]:
        return (
            Option
            .of_optional(_resolved(tree, slot).get(key))
            .bind(lambda each: Some(each) if isinstance(each, kind) else Nothing)
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(face), key=key),))
        )

    def withdrawn(held: bpy.types.NodeTreeInterfaceItem, key: Key) -> None:
        for toggle in _toggle(held).to_list():
            face.remove(toggle)
        face.remove(held)
        tree[slot] = {name: identifier for name, identifier in _items(tree, slot).items() if name != key}

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

        def stored(declared: InterfaceItem) -> None:
            tree[slot] = _items(tree, slot) | {key: declared.identifier}

        return created().map(stored)

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
                assigned(change.members, held),
            )).map(lambda _: None)

        return item(change.key, bpy.types.NodeTreeInterfaceItem).bind(retyped).bind(rest)

    match row:
        case Withdrawn(key=key):
            return item(key, bpy.types.NodeTreeInterfaceItem).map(lambda held: withdrawn(held, key))
        case Declared(key=key, entry=entry):
            return made(key, entry)
        case Restated():
            return restated(row)


def _restated(key: Key, entry: DesiredItem, place: Option[tuple[Option[Key], int]], held: Option[InterfaceItem]) -> Option[Restated]:
    """Differing parts of a kept interface item or the members of a fresh one, with the place its panel's order requires."""

    def changed[T](now: T, wanted: T) -> Option[T]:
        return Nothing if now == wanted else Some(wanted)

    def parts() -> Restated:
        match entry, held.to_optional():
            case (Socket() as socket, _), bpy.types.NodeTreeInterfaceSocket() as item:
                kind = Nothing if item.socket_type == socket.kind.__name__ else Some(socket.kind)
                return Restated(
                    key=key,
                    kind=kind,
                    name=changed(item.name, socket.name),
                    description=changed(item.description, socket.description),
                    place=place,
                    members=kind.map(lambda _: socket.members).default_with(lambda: differing(socket.members, item)),
                )
            case InterfacePanel() as panel, bpy.types.NodeTreeInterfacePanel() as item:
                return Restated(
                    key=key,
                    name=changed(item.name, panel.name),
                    description=changed(item.description, panel.description),
                    closed=changed(item.default_closed, panel.closed),
                    toggle=panel.toggle.filter(lambda value: not any(holds(toggle, "default_value", value) for toggle in _toggle(item).to_list())),
                    place=place,
                )
            case (Socket() as socket, _), _:
                return Restated(key=key, place=place, members=socket.members)
            case _:
                return Restated(key=key, place=place)

    return Some(parts()).filter(lambda row: row != Restated(key=key))


def _matched(entry: DesiredItem, item: InterfaceItem) -> bool:
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


def _items(tree: bpy.types.NodeTree, slot: str) -> dict[str, str | int]:
    """Stored item key to item identifier, empty where the slot holds no group."""
    return Option.of_optional(tree.get(slot)).filter(lambda value: isinstance(value, idprop.types.IDPropertyGroup)).map(lambda group: dict(group.to_dict())).default_value({})


def _live(tree: bpy.types.NodeTree) -> dict[str | int, InterfaceItem]:
    """Live interface sockets and panels by identifier."""
    return {item.identifier: item for item in tree.interface.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel)}  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


def _resolved(tree: bpy.types.NodeTree, slot: str) -> dict[Key, InterfaceItem]:
    """Stored item key to its live socket or panel, omitting keys whose item the interface no longer holds."""
    live = _live(tree)
    return {key: live[identifier] for key, identifier in _items(tree, slot).items() if identifier in live}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
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
    "described",
    "interfaced",
    "item_key",
    "kept",
    "revised",
    "slot",
    "under",
]
