"""RNA value snapshots: capture walks reading member values into value trees, member values by owner path, host-ordered writes, projection, and clipboard transfer."""

from collections.abc import Callable, Collection, Iterable, Iterator, Mapping
from functools import partial, reduce
from types import MappingProxyType
from typing import Final, Protocol, Self, TypeIs

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np

from blender.bpy.data.rna.descriptors import Descriptor, effective, LEADING, sequenced, Value
from blender.bpy.data.rna.paths import defined, definition, id_path, Native, rna, RnaPath, Struct, struct_at
from blender.records.faults import accumulated, Fault, Faults, Outcome, Reason
from blender.records.trees import merge, Owner, transfer, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type _Enclosing = frozenset[tuple[int, str]]
type _Read[W] = Callable[[Descriptor, Native, _Enclosing], Option[W]]


class _Elements(Protocol):
    """Struct collection the host grows and shrinks through its own calls."""

    def __len__(self) -> int: ...

    def __getitem__(self, index: int, /) -> object: ...

    def __iter__(self) -> Iterator[object]: ...

    def add(self, *values: object) -> object: ...

    def new(self, *values: object) -> object: ...

    def remove(self, item: object, /) -> object: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_ADDED: Final[Mapping[type[Struct], Callable[[_Elements, Mapping[str, object]], object]]] = MappingProxyType({
    bpy.types.CurveMapPoint: lambda items, values: items.new(*np.asarray(values["location"], dtype=np.float64)),
    bpy.types.CurveProfilePoint: lambda items, values: items.add(*np.asarray(values["location"], dtype=np.float64)),
    bpy.types.ColorRampElement: lambda items, values: items.new(values["position"]),
})

# --- [MODELS] ---------------------------------------------------------------------------


class Applied(msgspec.Struct, frozen=True, kw_only=True):
    """Snapshot write outcome per path."""

    written: tuple[RnaPath, ...] = ()
    unchanged: tuple[RnaPath, ...] = ()
    skipped: tuple[RnaPath, ...] = ()
    outcome: Outcome[None] = Ok(None)

    @classmethod
    def join(cls, outcomes: Iterable[Self]) -> Self:
        """Concatenates each path category in operation order."""
        held = tuple(outcomes)
        return cls(
            written=tuple(path for outcome in held for path in outcome.written),
            unchanged=tuple(path for outcome in held for path in outcome.unchanged),
            skipped=tuple(path for outcome in held for path in outcome.skipped),
            outcome=accumulated(outcome.outcome for outcome in held).map(lambda _: None),
        )

    @classmethod
    def refused(cls, faults: Faults) -> Self:
        """Returns outcome failing with faults."""
        return cls(outcome=Error(faults))

    @classmethod
    def reported(cls, path: RnaPath, answer: Outcome[Option[Value | Struct]]) -> Self:
        """Classifies one member write as written, unchanged, or refused."""
        return answer.map(lambda prior: cls(written=(path,)) if prior.is_some() else cls(unchanged=(path,))).default_with(cls.refused)

    def then(self, step: Callable[[], Self]) -> Self:
        """Joins the outcome of a step that runs once this one holds."""
        return type(self).join((self, step())) if self.outcome.is_ok() else self


class Snapshot(msgspec.Struct, frozen=True, kw_only=True):
    """Assignable values keyed by owner path under struct type, each value named by its member path under that owner."""

    struct: str
    tree: Tree[Value, RnaPath, RnaPath]

    # --- [CAPTURE]

    @classmethod
    def of(cls, struct: type[Struct], values: Mapping[RnaPath, Value]) -> Self:
        """Returns snapshot of member values, each path placed under the owner its last identifier names.

        Args:
            struct: Struct type the paths start from.
            values: Value per member path.

        Returns:
            Snapshot holding one owner row per owner path, an owner written before the owners beneath it.
        """

        def placed(path: RnaPath) -> tuple[RnaPath, RnaPath]:
            last = max(index for index, step in enumerate(path) if isinstance(step, str))
            return path[:last], path[last:]

        rows = tuple((*placed(path), value) for path, value in values.items())
        owners = tuple(sorted(dict.fromkeys(owner for owner, _, _ in rows), key=len))
        return cls(
            struct=definition(struct).identifier,
            tree=Tree(roots=owners, rows=tuple((owner, Owner[Value, RnaPath, RnaPath](rows=tuple((name, value) for at, name, value in rows if at == owner))) for owner in owners)),
        )

    @classmethod
    def capture(cls, owner: Native, selection: Option[tuple[Descriptor, ...]] = Nothing) -> Outcome[Self]:
        """Captures assignable values keyed by owner path, the selected members alone when given.

        Args:
            owner: Struct whose members the snapshot holds.
            selection: Members to capture, every saved member when Nothing.

        Returns:
            Snapshot, domain fault for an undecodable value.
        """

        def stored(descriptor: Descriptor, struct: Native, _: frozenset[tuple[int, str]]) -> Option[Value]:
            return Some(descriptor.read(struct)).filter(
                lambda value: not (isinstance(descriptor.prop, bpy.types.EnumProperty) and not descriptor.prop.is_enum_flag and isinstance(value, str) and not value)
            )

        return (
            catch(captured, exception=msgspec.ValidationError)(owner, stored, selection)
            .map_error(partial(Fault.of, Reason.OUTSIDE, repr(owner), member=Value.__name__))
            .map(lambda tree: cls(struct=definition(owner).identifier, tree=tree))
        )

    # --- [APPLICATION]

    def apply(self, owner: Native) -> Applied:
        """Writes snapshot to owner in host order."""
        match definition(owner).identifier:
            case self.struct:
                return applied(owner, self.tree, lambda _, value: value)
            case identifier:
                return Applied.refused(Fault.of(Reason.OUTSIDE, repr(owner), self.struct, member=identifier))

    def selection(self, owner: Native) -> tuple[Descriptor, ...]:
        """Resolves members and compound roots assigned on owner."""
        return (
            *(descriptor for key in self.tree.owners if key for descriptor in Descriptor.resolve(owner, key, bpy.types.Property).to_option().to_list() if descriptor.compound),
            *(descriptor for key, held in self.tree.owners.items() for name in held.values for descriptor in Descriptor.resolve(owner, (*key, *name), bpy.types.Property).to_option().to_list()),
        )

    def projected(self, owner: Native, current: "Snapshot", locked: frozenset[RnaPath] = frozenset()) -> Outcome[tuple["Snapshot", "Snapshot"]]:
        """Returns unlocked assignments with resulting current state.

        Args:
            owner: Struct both snapshots describe.
            current: Snapshot of owner's present values.
            locked: Paths whose members and owners stay as current holds them.

        Returns:
            Sparse assignments and the state they leave, domain fault for a snapshot of another struct.
        """
        identifier = definition(owner).identifier

        def within(path: RnaPath, roots: Collection[RnaPath]) -> bool:
            return any(path[: len(root)] == root for root in roots)

        def split() -> tuple[Snapshot, Snapshot]:
            free = {
                key: Owner[Value, RnaPath, RnaPath](
                    rows=tuple((name, value) for name, value in held.rows if not locked or not within((*key, *name), locked)),
                    children=tuple(child for child in held.children if not within(child, locked)),
                )
                for key, held in self.tree.owners.items()
                if not within(key, locked)
            }
            compounds = frozenset(key for key in free if key for descriptor in Descriptor.resolve(owner, key, bpy.types.Property).to_option().to_list() if descriptor.compound)
            base = {key: held for key, held in current.tree.owners.items() if not within(key, compounds)}
            values = base | {
                key: held
                if within(key, compounds)
                else Owner[Value, RnaPath, RnaPath](rows=tuple({**previous.values, **held.values}.items()), children=tuple(dict.fromkeys((*previous.children, *held.children))))
                for key, held in free.items()
                for previous in (base.get(key, Owner[Value, RnaPath, RnaPath]()),)
            }
            nested = frozenset(child for held in values.values() for child in held.children)
            sparse = msgspec.structs.replace(self, tree=Tree(roots=tuple(key for key in self.tree.roots if key in free), rows=tuple(free.items())))
            matching = msgspec.structs.replace(self, tree=Tree(roots=tuple(dict.fromkeys(key for key in (*current.tree.roots, *sparse.tree.roots) if key not in nested)), rows=tuple(values.items())))
            return sparse, matching

        return accumulated(
            Ok(snapshot).filter_with(lambda held: held.struct == identifier, lambda held: Fault.of(Reason.OUTSIDE, repr(owner), held.struct, member=identifier)) for snapshot in (self, current)
        ).map(lambda _: split())

    # --- [TEXT]

    @property
    def text(self) -> str:
        """JSON text of the snapshot in deterministic key order, the form a clipboard or text block holds."""
        return msgspec.json.encode(self, order="deterministic").decode()

    @classmethod
    def parsed(cls, subject: str, text: str) -> Outcome[Self]:
        """Returns the snapshot JSON text holds, unreadable fault over subject for text no snapshot wrote."""
        held: Outcome[Self] = catch(msgspec.json.decode, exception=msgspec.DecodeError)(text, type=cls).map_error(partial(Fault.of, Reason.UNREADABLE, subject))
        return held

    def copied(self, manager: bpy.types.WindowManager) -> None:
        """Copies snapshot to the system clipboard as JSON."""
        manager.clipboard = self.text

    @staticmethod
    def pasted(manager: bpy.types.WindowManager, owners: tuple[Struct, ...]) -> Outcome[tuple[Applied, ...]]:
        """Applies the clipboard snapshot to each owner of its struct over the values it holds now, outside fault when no owner is of its struct."""
        subject = f"{manager!r}.clipboard"
        return Snapshot.parsed(subject, manager.clipboard).bind(
            lambda clip: (
                Result[tuple[Struct, ...], Faults]
                .Ok(tuple(owner for owner in owners if definition(owner).identifier == clip.struct))
                .filter_with(bool, lambda _: Fault.of(Reason.OUTSIDE, subject, clip.struct, member=", ".join(dict.fromkeys(definition(owner).identifier for owner in owners))))
                .bind(
                    lambda matching: accumulated(
                        Snapshot
                        .capture(owner)
                        .map(lambda target: msgspec.structs.replace(clip, tree=merge(target.tree, transfer(target.tree, clip.tree))).apply(owner))
                        .bind(lambda applied: applied.outcome.map(lambda _: applied))
                        for owner in matching
                    )
                )
            )
        )


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
        branches = tuple((descriptor.prop, self.branched(struct, path, descriptor.prop, inside)) for descriptor in members if not (self.native and descriptor.compound))
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
                    if found.compound
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
# --- [CAPTURE]


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


# --- [WRITES]


def applied[W](owner: Native, tree: Tree[W, RnaPath, RnaPath], projected: Callable[[Descriptor, W], Value | Struct | Tree[W, RnaPath, RnaPath]]) -> Applied:
    """Writes stored or native values in host order, every write, resize, and skip reported once.

    `LEADING` members go first across every owner, each resetting members it governs, then each root writes its members, resizes its collections, and descends.
    Each member write re-reads its holder through its ID, an earlier write (`type`, `dimensions`) having refined the holder's class, and members the holder lacked before such a write resolve again on the refined holder.

    Args:
        owner: Struct or collection the tree's owner paths start from.
        tree: Values keyed by owner path and member path.
        projected: Host value of one held value, a tree for a compound written beneath it.

    Returns:
        Writes, unchanged and skipped paths, and accumulated faults.
    """
    leads = tuple(
        (struct, key, Descriptor(root=type(struct), prop=prop), held.values[prop.identifier,])
        for prop in LEADING
        for key, held in tree.owners.items()
        if (prop.identifier,) in held.values
        for struct in struct_at(owner, key, bpy.types.bpy_struct).to_list()
        if definition(struct).properties.get(prop.identifier) == prop
    )
    completed = frozenset((key, (descriptor.prop.identifier,)) for _, key, descriptor, _ in leads)

    def current(struct: Native) -> Native:
        match struct:
            case bpy.types.bpy_struct(id_data=bpy.types.ID() as block):
                return id_path(struct).bind(lambda path: struct_at(block, path, bpy.types.bpy_struct)).default_value(struct)
            case _:
                return struct

    def member(struct: Native, key: RnaPath, descriptor: Descriptor, value: W) -> Applied:
        desired, holder = projected(descriptor, value), current(struct)
        return applied(holder, desired, projected) if isinstance(desired, Tree) else Applied.reported((*key, *descriptor.path), descriptor.write(holder, desired))

    @catch(exception=RuntimeError)
    def changed(items: _Elements, path: RnaPath, rows: tuple[Mapping[str, object], ...], adding: Callable[[_Elements, Mapping[str, object]], object], *, indexed: bool) -> Applied:
        report = Applied(written=(path,) if len(items) != len(rows) else ())
        for values in rows[len(items) :]:
            adding(items, values)
        for _ in range(len(items) - len(rows)):
            items.remove(len(items) - 1 if indexed else items[-1])
        return report

    def resized(struct: Native, path: RnaPath, prop: bpy.types.CollectionProperty, fixed: Struct) -> Applied:
        if path not in tree.owners:
            return Applied()
        held = definition(fixed).properties
        rows = tuple(
            {
                identifier: projected(Descriptor(root=type(fixed), prop=held[identifier]), value)
                for name, value in tree.owners[at].values.items()
                for identifier in name[:1]
                if len(name) == 1 and isinstance(identifier, str) and identifier in held
            }
            for at in tree.owners[path].children
        )
        items: _Elements = getattr(struct, prop.identifier)
        subject = f"{struct!r}.{prop.identifier}"
        match _ADDED.get(type(fixed)), fixed:
            case None, bpy.types.PropertyGroup() if prop.is_runtime:
                return changed(items, path, rows, lambda grown, _: grown.add(), indexed=True).map_error(partial(Fault.of, Reason.REFUSED, subject)).default_with(Applied.refused)
            case None, _:
                present = sum(item is not None for item in items)
                return Applied() if present == len(rows) else Applied.refused(Fault.of(Reason.OUTSIDE, subject, str(len(rows)), member=str(present)))
            case adding, _:
                return changed(items, path, rows, adding, indexed=False).map_error(partial(Fault.of, Reason.REFUSED, subject)).default_with(Applied.refused)

    def members(struct: Native, key: RnaPath, names: tuple[tuple[RnaPath, W], ...]) -> Applied:
        found = tuple((name, value, Descriptor.resolve(struct, name, bpy.types.Property).to_option()) for name, value in names)
        ordered = sequenced((descriptor.owner, holder, descriptor, value) for _, value, held in found for descriptor in held.to_list() for holder in struct_at(struct, descriptor.owner).to_list())
        rest = tuple((name, value) for name, value, held in found if held.is_none())
        return Applied.join(member(holder, key, descriptor, value) for _, holder, descriptor, value in ordered).then(
            lambda: members(current(struct), key, rest) if rest and len(rest) < len(names) else Applied(skipped=tuple((*key, *name) for name, _ in rest))
        )

    def wrote(struct: Native, key: RnaPath) -> Applied:
        written = members(struct, key, tuple((name, value) for name, value in tree.owners[key].values.items() if (key, name) not in completed))
        refined = current(struct)
        result = written.then(
            lambda: Applied.join(
                resized(refined, (*key, prop.identifier), prop, fixed)
                for held in defined(refined).to_list()
                for prop in held.properties
                if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID)
                for fixed in Option.of_optional(prop.fixed_type).to_list()
            )
        ).then(lambda: Applied.join(located(child) for child in tree.owners[key].children))
        if isinstance(refined, bpy.types.CurveMapping | bpy.types.CurveProfile):
            refined.update()
        return result

    def located(key: RnaPath) -> Applied:
        return struct_at(owner, key).map(lambda found: wrote(found, key)).default_with(lambda: Applied(skipped=(key,)))

    @catch(exception=ReferenceError)
    def settled() -> Applied:
        return reduce(
            lambda done, prop: done.then(lambda: Applied.join(member(struct, key, descriptor, value) for struct, key, descriptor, value in leads if descriptor.prop == prop)), LEADING, Applied()
        ).then(lambda: Applied.join(located(key) for key in tree.roots))

    return settled().map_error(partial(Fault.of, Reason.REFUSED, repr(owner))).default_with(Applied.refused)


def assigned(root: Struct, members: Mapping[RnaPath, Value]) -> Outcome[None]:
    """Writes members under root through the snapshot writer, in host order.

    Args:
        root: Struct the member paths start from.
        members: Value per member path.

    Returns:
        Faults of every refused write and of every path naming no member, accumulated.
    """
    written = Snapshot.of(type(root), members).apply(root)
    return accumulated((written.outcome, *(Error(Fault.of(Reason.ABSENT, repr(root), member=rna(path))) for path in written.skipped))).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Applied", "Snapshot", "applied", "assigned", "captured", "referenced"]
