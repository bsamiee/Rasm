"""Capture walks: the saved or selected members of a struct read into value tree rows, each struct read once."""

from collections.abc import Callable, Collection
from typing import TypeIs

import bpy
from expression import Nothing, Option, Some
from expression.collections import Block
import msgspec
import numpy as np

from blender.bpy.data.rna.descriptors import Descriptor, effective
from blender.bpy.data.rna.paths import definition, id_path, Native, RnaPath
from blender.records.trees import Owner, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type _Enclosing = frozenset[tuple[int, str]]
type _Read[W] = Callable[[Descriptor, Native, _Enclosing], Option[W]]

# --- [SERVICES] -------------------------------------------------------------------------


class _Capture[W]:
    """One walk of a source into value tree rows under a value reader, a native walk holding compounds whole and keying top-level rows by ID session."""

    __slots__ = ("enclosing", "expanded", "indexed", "native", "owner", "owners", "read", "seen", "sessioned", "source")

    def __init__(self, owner: Native, source: Native, read: _Read[W], prior: Option[Tree[W, RnaPath, RnaPath]], *, native: bool, expanded: Option[Descriptor], enclosing: _Enclosing) -> None:
        """Holds the prior rows, the reader, and the structs enclosing the walk."""
        self.owner, self.source, self.read, self.native, self.expanded, self.enclosing = owner, source, read, native, expanded, enclosing
        self.owners: dict[RnaPath, Owner[W, RnaPath, RnaPath]] = prior.map(lambda tree: dict(tree.owners)).default_with(dict)
        self.seen: set[tuple[int, str]] = set(enclosing)
        self.sessioned, self.indexed = native and expanded.is_none(), expanded.is_some()

    # --- [KEYS]

    @staticmethod
    def beneath(path: RnaPath, root: RnaPath) -> bool:
        """Whether path sits strictly under root."""
        return len(path) > len(root) and path[: len(root)] == root

    @staticmethod
    def address(struct: Native) -> tuple[int, str]:
        """Returns the walk identity of a struct or collection."""
        return (struct.as_pointer() if isinstance(struct, bpy.types.bpy_struct) else id(struct), definition(struct).identifier)

    @staticmethod
    def arrayed(prop: bpy.types.Property, names: Collection[RnaPath]) -> TypeIs[bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty]:
        """Whether names hold an element of the numeric array member."""
        return (
            isinstance(prop, bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty)
            and prop.is_array
            and any((prop.identifier, *at) in names for at in np.ndindex(*Descriptor.dimensions(prop)))
        )

    def keyed(self, struct: Native, path: RnaPath) -> RnaPath:
        """Returns the row key of a struct, a native top-level walk keying by session and path or by pointer."""
        if not self.sessioned:
            return path
        target = referenced(self.owner, struct, path)
        return (target[0].session_uid, *target[1]) if isinstance(target, tuple) else (self.address(struct)[0],)

    # --- [WALKS]

    def branched(self, struct: Native, key: RnaPath, prop: bpy.types.Property, inside: _Enclosing) -> tuple[RnaPath, ...]:
        """Walks the structs a pointer or struct collection member holds, returning their keys."""
        path = (*key, prop.identifier)
        match prop:
            case bpy.types.PointerProperty(fixed_type=bpy.types.ID()) | bpy.types.CollectionProperty(fixed_type=bpy.types.ID()):
                return ()  # type: ignore[unreachable]
            case bpy.types.CollectionProperty() if path in self.owners:
                return self.owners[path].children
            case bpy.types.PointerProperty(is_readonly=True):
                return tuple(at for child in Option.of_optional(getattr(struct, prop.identifier)).filter(effective).to_list() for at in self.walked(child, path, inside).to_list())
            case bpy.types.CollectionProperty():
                items = tuple((index, item) for index, item in enumerate(getattr(struct, prop.identifier)) if item is not None)
                named: Option[tuple[RnaPath, ...]] = (
                    Option
                    .of_optional(prop.fixed_type)
                    .bind(lambda fixed: Option.of_optional(fixed.name_property))
                    .map(lambda held: tuple(getattr(item, held.identifier) for _, item in items))
                    .filter(lambda found: not self.indexed and len(set(found)) == len(found))
                    .map(lambda found: tuple(((name,),) for name in found))
                )
                steps = named.default_with(lambda: tuple((index,) for index, _ in items))
                return tuple(at for step, (_, item) in zip(steps, items) for at in self.walked(item, (*path, *step), inside).to_list())
            case _:
                return ()

    def grouped(self, struct: Native, path: RnaPath, prop: bpy.types.Property, keys: tuple[RnaPath, ...]) -> tuple[RnaPath, ...]:
        """Returns the row of a struct collection holding keys, or keys themselves for any other member."""
        if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID) and (keys or not tuple(getattr(struct, prop.identifier))):
            self.owners.setdefault(row := (*path, prop.identifier), Owner(children=keys))
            return (row,)
        return keys

    def walked(self, struct: Native, path: RnaPath, above: _Enclosing) -> Option[RnaPath]:
        """Reads every saved member of struct into its row and descends, Nothing for a struct already read."""
        if (identity := self.address(struct)) in self.seen:
            return Nothing
        self.seen.add(identity)
        inside, key = above | {identity}, self.keyed(struct, path)
        members = (
            *Descriptor.saved(struct),
            *(Descriptor(root=type(struct), prop=prop) for prop in Option.of_optional(definition(struct).name_property).filter(lambda prop: self.indexed and not prop.is_readonly).to_list()),
        )
        held = self.owners.setdefault(key, Owner())
        values: dict[RnaPath, W] = {
            (descriptor.prop.identifier,): value
            for descriptor in members
            if descriptor.valued(native=self.native) and (descriptor.prop.identifier,) not in held.values and not self.arrayed(descriptor.prop, held.values)
            for value in self.read(descriptor, struct, self.enclosing | inside).to_list()
        }
        branches = tuple(
            (descriptor.prop, self.branched(struct, path, descriptor.prop, inside))
            for descriptor in members
            if not (self.native and (isinstance(descriptor.prop, bpy.types.CollectionProperty) or descriptor.curve))
        )
        nested = tuple(at for prop, keys in branches for at in self.grouped(struct, path, prop, keys))
        merged: dict[RnaPath, W] = {**held.values, **values}
        self.owners[key] = Owner(rows=tuple(merged.items()), children=tuple(dict.fromkeys((*held.children, *nested))))
        for descriptor in members:
            if self.arrayed(descriptor.prop, held.values):
                for at in np.ndindex(*Descriptor.dimensions(descriptor.prop)):
                    self.picked(Descriptor(root=type(self.source), owner=path, prop=descriptor.prop, coordinates=at), inside)
        return Some(key)

    def picked(self, descriptor: Descriptor, above: _Enclosing = frozenset()) -> tuple[RnaPath, ...]:
        """Reads one selected member into its row, a member inside a native compound reading that compound whole."""

        def member(struct: Native) -> tuple[RnaPath, ...]:
            inside = above | {self.address(struct)}
            if self.sessioned and isinstance(self.source, bpy.types.bpy_struct) and isinstance(struct, bpy.types.bpy_struct):
                enclosing = (
                    Descriptor(root=type(self.source), owner=at[len(base) :], prop=found.prop)
                    for base, path in id_path(self.source).bind(lambda base: id_path(struct).map(lambda own: (base, (*own, descriptor.prop.identifier)))).to_list()
                    for ancestor in struct.rna_ancestors()
                    for at in id_path(ancestor).to_list()
                    for found in Descriptor.saved(ancestor)
                    if (isinstance(found.prop, bpy.types.CollectionProperty) and not isinstance(found.prop.fixed_type, bpy.types.ID)) or (found.curve and found.prop.is_readonly)
                    for prefix in ((*at, found.prop.identifier),)
                    if self.beneath(prefix, base) and self.beneath(path, prefix)
                )
                for found in Block.of_seq(enclosing).try_head().to_list():
                    return self.picked(found, above)
            if self.expanded != Some(descriptor) and descriptor.valued(native=self.native):
                key = self.keyed(struct, descriptor.owner)
                name: RnaPath = (descriptor.prop.identifier, *descriptor.coordinates)
                if self.native and any(self.beneath(key, (*at, *held_name)) for at, held in self.owners.items() for held_name in held.values):
                    return ()
                if key in self.owners and (name in self.owners[key].values or (descriptor.prop.identifier,) in self.owners[key].values):
                    return (key,)
                if not descriptor.coordinates and key in self.owners and self.arrayed(descriptor.prop, self.owners[key].values):
                    return tuple(at for coordinates in np.ndindex(*Descriptor.dimensions(descriptor.prop)) for at in self.picked(msgspec.structs.replace(descriptor, coordinates=coordinates), above))
                for value in self.read(descriptor, struct, self.enclosing | inside).to_list():
                    held = self.owners.get(key, Owner())
                    self.owners[key] = Owner(rows=(*held.rows, (name, value)), children=held.children)
                    return (key,)
                return ()
            nested = self.branched(struct, descriptor.owner, descriptor.prop, inside)
            if self.expanded == Some(descriptor) or isinstance(descriptor.prop, bpy.types.CollectionProperty):
                if isinstance(descriptor.prop, bpy.types.CollectionProperty):
                    self.owners.setdefault(descriptor.path, Owner(children=nested))
                    nested = (descriptor.path,)
                held = self.owners.setdefault(descriptor.owner, Owner())
                self.owners[descriptor.owner] = msgspec.structs.replace(held, children=tuple(dict.fromkeys((*held.children, *nested))))
                return (descriptor.owner,)
            return nested

        return descriptor.owner_of(self.source).map(member).default_value(())

    def tree(self, chosen: Option[tuple[Descriptor, ...]], prior: Option[Tree[W, RnaPath, RnaPath]]) -> Tree[W, RnaPath, RnaPath]:
        """Walks the chosen members, or the whole source, into a tree whose roots extend prior's."""
        roots = (
            *prior.map(lambda tree: tree.roots).default_value(()),
            *chosen.map(lambda selected: tuple(key for descriptor in selected for key in self.picked(descriptor))).default_with(lambda: tuple(self.walked(self.source, (), frozenset()).to_list())),
        )
        nested = frozenset(child for held in self.owners.values() for child in held.children)
        return Tree(roots=tuple(dict.fromkeys(key for key in roots if key not in nested)), rows=tuple(self.owners.items()))


# --- [OPERATIONS] -----------------------------------------------------------------------


def captured[W](
    owner: Native,
    read: _Read[W],
    selection: Option[tuple[Descriptor, ...]],
    prior: Option[Tree[W, RnaPath, RnaPath]] = Nothing,
    *,
    source: Option[Native] = Nothing,
    native: bool = False,
    expanded: Option[Descriptor] = Nothing,
    enclosing: _Enclosing = frozenset(),
) -> Tree[W, RnaPath, RnaPath]:
    """Walks a source into a value tree under a value reader, the selected members alone when given, extending prior.

    Args:
        owner: Struct the capture started from, anchoring native references.
        read: Value of one member, Nothing to leave it out.
        selection: Members to capture, every saved member when Nothing.
        prior: Tree whose values the walk keeps.
        source: Struct or collection this walk reads, owner when Nothing.
        native: Whether the walk holds compounds whole and keys top-level rows by ID session.
        expanded: Compound member a nested native walk expands.
        enclosing: Structs enclosing a nested walk, ending its branches.

    Returns:
        Tree keyed by owner path, each value named by its member path under that owner, a value no member decodes raising the reader's `msgspec.ValidationError` through the walk.
    """
    return _Capture(owner, source.default_value(owner), read, prior, native=native, expanded=expanded, enclosing=enclosing).tree(selection, prior)


def referenced(owner: Native, struct: Native, path: RnaPath) -> tuple[bpy.types.ID, RnaPath] | Native:
    """Returns struct as its ID and path, a collection through owner's ID path, else struct itself."""
    match struct, owner:
        case bpy.types.bpy_struct(id_data=bpy.types.ID() as block), _:
            return id_path(struct).map(lambda found: (block, found)).default_value(struct)
        case bpy.types.bpy_prop_collection(), bpy.types.bpy_struct(id_data=bpy.types.ID() as block):
            return id_path(owner).map(lambda found: (block, (*found, *path))).default_value(struct)
        case _:
            return struct


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["captured", "referenced"]
