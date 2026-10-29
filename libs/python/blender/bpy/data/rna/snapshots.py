"""RNA value snapshots: member values by owner path, their capture, host-ordered writes, projection, and clipboard transfer."""

from collections.abc import Callable, Collection, Iterable, Iterator, Mapping
from functools import partial, reduce
from types import MappingProxyType
from typing import Final, Protocol, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np

from blender.bpy.data.rna.captures import captured
from blender.bpy.data.rna.descriptors import Descriptor, LEADING, sequenced, Value
from blender.bpy.data.rna.paths import definition, id_path, Native, rna, RnaPath, Struct, struct_at
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Faults, Outcome, RecordReason
from blender.records.trees import merge, Owner, transfer, Tree

# --- [TYPES] ----------------------------------------------------------------------------


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
            .map_error(partial(Fault.of, RecordReason.OUTSIDE, repr(owner), member=Value.__name__))
            .map(lambda tree: cls(struct=definition(owner).identifier, tree=tree))
        )

    # --- [APPLICATION]

    def apply(self, owner: Native) -> Applied:
        """Writes snapshot to owner in host order."""
        match definition(owner).identifier:
            case self.struct:
                return applied(owner, self.tree, lambda _, value: value)
            case identifier:
                return Applied.refused(Fault.of(RecordReason.OUTSIDE, repr(owner), self.struct, member=identifier))

    def selection(self, owner: Native) -> tuple[Descriptor, ...]:
        """Resolves members and compound roots assigned on owner."""
        return (
            *(
                descriptor
                for key in self.tree.owners
                if key
                for descriptor in Descriptor.resolve(owner, key, bpy.types.Property).to_option().to_list()
                if isinstance(descriptor.prop, bpy.types.CollectionProperty) or descriptor.curve
            ),
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
            compounds = frozenset(
                key
                for key in free
                if key
                for descriptor in Descriptor.resolve(owner, key, bpy.types.Property).to_option().to_list()
                if isinstance(descriptor.prop, bpy.types.CollectionProperty) or descriptor.curve
            )
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
            Ok(snapshot).filter_with(lambda held: held.struct == identifier, lambda held: Fault.of(RecordReason.OUTSIDE, repr(owner), held.struct, member=identifier)) for snapshot in (self, current)
        ).map(lambda _: split())

    # --- [CLIPBOARD]

    def copied(self, manager: bpy.types.WindowManager) -> None:
        """Copies snapshot to the system clipboard as JSON."""
        manager.clipboard = msgspec.json.encode(self, order="deterministic").decode()

    @staticmethod
    def pasted(manager: bpy.types.WindowManager, owners: tuple[Struct, ...]) -> Outcome[tuple[Applied, ...]]:
        """Applies the clipboard snapshot to each owner over the values it holds now."""
        return Result[Snapshot, Faults].bind(
            catch(msgspec.json.Decoder(Snapshot).decode, exception=msgspec.DecodeError)(manager.clipboard).map_error(partial(Fault.of, RecordReason.UNREADABLE, f"{manager!r}.clipboard")),
            lambda clip: accumulated(
                Snapshot
                .capture(owner)
                .map(lambda target: msgspec.structs.replace(clip, tree=merge(target.tree, transfer(target.tree, clip.tree))).apply(owner))
                .bind(lambda applied: applied.outcome.map(lambda _: applied))
                for owner in owners
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
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

    def current(struct: Struct) -> Struct:
        match struct.id_data:
            case bpy.types.ID() as block:
                return id_path(struct).bind(lambda path: struct_at(block, path, bpy.types.bpy_struct)).default_value(struct)
            case _:
                return struct

    def member(struct: Struct, key: RnaPath, descriptor: Descriptor, value: W) -> Applied:
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

    def resized(struct: Struct, path: RnaPath, prop: bpy.types.CollectionProperty, fixed: Struct) -> Applied:
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
                return changed(items, path, rows, lambda grown, _: grown.add(), indexed=True).map_error(partial(Fault.of, BlenderReason.REFUSED, subject)).default_with(Applied.refused)
            case None, _:
                present = sum(item is not None for item in items)
                return Applied() if present == len(rows) else Applied.refused(Fault.of(RecordReason.OUTSIDE, subject, str(len(rows)), member=str(present)))
            case adding, _:
                return changed(items, path, rows, adding, indexed=False).map_error(partial(Fault.of, BlenderReason.REFUSED, subject)).default_with(Applied.refused)

    def members(struct: Struct, key: RnaPath, names: tuple[tuple[RnaPath, W], ...]) -> Applied:
        found = tuple((name, value, Descriptor.resolve(struct, name, bpy.types.Property).to_option()) for name, value in names)
        ordered = sequenced(
            (descriptor.owner, holder, descriptor, value) for _, value, held in found for descriptor in held.to_list() for holder in struct_at(struct, descriptor.owner, bpy.types.bpy_struct).to_list()
        )
        rest = tuple((name, value) for name, value, held in found if held.is_none())
        return Applied.join(member(holder, key, descriptor, value) for _, holder, descriptor, value in ordered).then(
            lambda: members(current(struct), key, rest) if rest and len(rest) < len(names) else Applied(skipped=tuple((*key, *name) for name, _ in rest))
        )

    def wrote(struct: Struct, key: RnaPath) -> Applied:
        written = members(struct, key, tuple((name, value) for name, value in tree.owners[key].values.items() if (key, name) not in completed))
        refined = current(struct)
        result = written.then(
            lambda: Applied.join(
                resized(refined, (*key, prop.identifier), prop, fixed)
                for prop in definition(refined).properties
                if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID)
                for fixed in Option.of_optional(prop.fixed_type).to_list()
            )
        ).then(lambda: Applied.join(located(child) for child in tree.owners[key].children))
        if isinstance(refined, bpy.types.CurveMapping | bpy.types.CurveProfile):
            refined.update()
        return result

    def located(key: RnaPath) -> Applied:
        match struct_at(owner, key).to_optional():
            case bpy.types.bpy_struct() as struct:
                return wrote(struct, key)
            case None:
                return Applied(skipped=(key,))
            case _:
                return Applied.join(located(child) for child in tree.owners[key].children)

    @catch(exception=ReferenceError)
    def settled() -> Applied:
        return reduce(
            lambda done, prop: done.then(lambda: Applied.join(member(struct, key, descriptor, value) for struct, key, descriptor, value in leads if descriptor.prop == prop)), LEADING, Applied()
        ).then(lambda: Applied.join(located(key) for key in tree.roots))

    return settled().map_error(partial(Fault.of, BlenderReason.REFUSED, repr(owner))).default_with(Applied.refused)


def assigned(root: Struct, members: Mapping[RnaPath, Value]) -> Outcome[None]:
    """Writes members under root through the snapshot writer, in host order.

    Args:
        root: Struct the member paths start from.
        members: Value per member path.

    Returns:
        Faults of every refused write and of every path naming no member, accumulated.
    """
    written = Snapshot.of(type(root), members).apply(root)
    return accumulated((written.outcome, *(Error(Fault.of(BlenderReason.ABSENT, repr(root), member=rna(path))) for path in written.skipped))).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Applied", "Snapshot", "applied", "assigned"]
