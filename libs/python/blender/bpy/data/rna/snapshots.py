"""RNA value trees, ordered writes, restoration, and owner scopes."""

from collections.abc import Callable, Collection, Iterable, Iterator, Mapping
from functools import partial, reduce
import inspect
from types import MappingProxyType
from typing import ClassVar, Final, Protocol, Self, TypeIs

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np

from blender.bpy.data.rna.descriptors import choices, definition, Descriptor, effective, id_path, LEADING, Native, parsed, rna, RnaPath, rooted, sequenced, Struct, struct_at, Value
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Case
from blender.records.trees import Owner, revert, transfer, Tree

# --- [TYPES] ----------------------------------------------------------------------------


type Scope = Scenes | ViewLayers | Selection | Objects
type NativeOwner = tuple[bpy.types.ID, RnaPath] | Native
type NativeValue = tuple[NativeOwner, Value | Struct | Tree[NativeValue, RnaPath]]
type _Persisted = Tree[tuple[tuple[object, RnaPath], object], RnaPath]
type _Enclosing = frozenset[tuple[int, str]]


class Scoped[O: Struct = Struct](Protocol):
    """Context-selected owners of one struct type."""

    @property
    def owner_type(self) -> type[O]:
        """Owner struct type."""
        ...

    def owners(self, context: bpy.types.Context, /) -> Outcome[tuple[O, ...]]:
        """Returns selected owners in stable order."""
        ...


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

    def then(self, step: Callable[[], Self]) -> Self:
        """Joins the outcome of a step that runs once this one holds."""
        return type(self).join((self, step())) if self.outcome.is_ok() else self


class Snapshot(msgspec.Struct, frozen=True, kw_only=True):
    """Assignable values keyed by owner path under struct type."""

    struct: str
    tree: Tree[Value, RnaPath]

    # --- [CAPTURE]

    @classmethod
    def of[K: str | RnaPath](cls, struct: type[Struct], values: Mapping[K, Value]) -> Self:
        """Returns snapshot of member values, each path placed under the owner its last identifier names.

        Args:
            struct: Struct type the paths start from.
            values: Value per path, host path text read through `parsed`.

        Returns:
            Snapshot holding one owner row per owner path, a path no RNA path reads raising `LookupError`.
        """

        def placed(key: str | RnaPath) -> tuple[RnaPath, str]:
            read: Outcome[RnaPath] = parsed(struct, key) if isinstance(key, str) else Ok(key)
            match read:
                case Result(tag="ok", ok=steps):
                    last = max(index for index, step in enumerate(steps) if isinstance(step, str))
                    return steps[:last], rna(steps[last:])
                case Result(error=faults):
                    raise LookupError(key, faults)

        rows = tuple((*placed(key), value) for key, value in values.items())
        owners = tuple(dict.fromkeys(owner for owner, _, _ in rows))
        return cls(
            struct=definition(struct).identifier,
            tree=Tree(roots=owners, rows=tuple((owner, Owner[Value, RnaPath](values={name: value for at, name, value in rows if at == owner})) for owner in owners)),
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

        def stored(descriptor: Descriptor, struct: Native, _: _Enclosing) -> Option[Value]:
            return Some(descriptor.read(struct)).filter(
                lambda value: not (isinstance(descriptor.prop, bpy.types.EnumProperty) and not descriptor.prop.is_enum_flag and isinstance(value, str) and not value)
            )

        return (
            catch(_walked, exception=msgspec.ValidationError)(owner, owner, _Walk(read=stored), selection)
            .map(lambda tree: cls(struct=definition(owner).identifier, tree=tree))
            .map_error(partial(Fault.of, RecordReason.OUTSIDE, repr(owner), member=Value.__name__))
        )

    @staticmethod
    def settings(setting: Descriptor, compound: tuple[type[Struct], ...] = ()) -> tuple[Descriptor, ...]:
        """Expands setting into leaf descriptors, `compound` pointers kept whole."""

        def walked(struct: type[Struct], owner: RnaPath, entered: frozenset[str]) -> tuple[Descriptor, ...]:
            def member(prop: bpy.types.Property) -> tuple[Descriptor, ...]:
                match prop:
                    case _ if _valued(prop) or (isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, compound)):
                        return (Descriptor(root=setting.root, owner=owner, prop=prop),)
                    case bpy.types.PointerProperty(is_readonly=True, fixed_type=bpy.types.bpy_struct() as pointed) if not isinstance(pointed, bpy.types.ID):  # type: ignore[unreachable]
                        return walked(type(pointed), (*owner, prop.identifier), entered | {pointed.identifier}) if pointed.identifier not in entered else ()
                    case _:
                        return ()

            return tuple(descriptor for found in Descriptor.saved(struct) for descriptor in member(found.prop))

        match setting.prop:
            case bpy.types.PointerProperty(is_readonly=True, fixed_type=bpy.types.bpy_struct() as pointed) if not isinstance(pointed, (bpy.types.ID, *compound)):
                return walked(type(pointed), setting.path, frozenset({pointed.identifier}))
            case _:
                return (setting,)

    # --- [APPLICATION]

    def apply(self, owner: Native) -> Applied:
        """Writes snapshot to owner in host order."""
        match definition(owner).identifier:
            case self.struct:
                return self._applied(owner, self.tree, lambda _, value: value)
            case identifier:
                return Applied.refused(Fault.of(RecordReason.OUTSIDE, repr(owner), self.struct, member=identifier))

    def applied[O: Native](self, owners: tuple[O, ...]) -> tuple[Applied, ...]:
        """Writes snapshot to every owner, one outcome each."""
        return tuple(map(self.apply, owners))

    def selection(self, owner: Native) -> tuple[Descriptor, ...]:
        """Resolves members and compound roots assigned on owner."""
        return (
            *(
                descriptor
                for key in self.tree.owners
                if key
                for descriptor in Descriptor.resolve(owner, key, bpy.types.Property).to_option().to_list()
                if isinstance(descriptor.prop, bpy.types.CollectionProperty) or _curve(descriptor.prop)
            ),
            *(
                descriptor
                for key, held in self.tree.owners.items()
                for struct in struct_at(owner, key).to_list()
                for name in held.values
                for descriptor in parsed(struct, name).bind(lambda steps: Descriptor.resolve(owner, (*key, *steps), bpy.types.Property)).to_option().to_list()
            ),
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

        def named(key: RnaPath, name: str) -> RnaPath:
            return struct_at(owner, key).bind(lambda struct: parsed(struct, name).to_option()).map(lambda steps: (*key, *steps)).default_value((*key, name))

        def split() -> tuple[Snapshot, Snapshot]:
            free = {
                key: Owner[Value, RnaPath](
                    values={name: value for name, value in held.values.items() if not locked or not within(named(key, name), locked)},
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
                if isinstance(descriptor.prop, bpy.types.CollectionProperty) or _curve(descriptor.prop)
            )
            base = {key: held for key, held in current.tree.owners.items() if not within(key, compounds)}
            values = base | {
                key: held if within(key, compounds) else Owner[Value, RnaPath](values={**previous.values, **held.values}, children=tuple(dict.fromkeys((*previous.children, *held.children))))
                for key, held in free.items()
                for previous in (base.get(key, Owner[Value, RnaPath](values={})),)
            }
            nested = frozenset(child for held in values.values() for child in held.children)
            sparse = msgspec.structs.replace(self, tree=Tree(roots=tuple(key for key in self.tree.roots if key in free), rows=tuple(free.items())))
            matching = msgspec.structs.replace(self, tree=Tree(roots=tuple(dict.fromkeys(key for key in (*current.tree.roots, *sparse.tree.roots) if key not in nested)), rows=tuple(values.items())))
            return sparse, matching

        return accumulated(
            Ok(snapshot).filter_with(lambda held: held.struct == identifier, lambda held: Fault.of(RecordReason.OUTSIDE, repr(owner), held.struct, member=identifier)) for snapshot in (self, current)
        ).map(lambda _: split())

    @classmethod
    def _applied[W](cls, owner: Native, tree: Tree[W, RnaPath], projected: Callable[[Descriptor, W], Value | Struct | Tree[NativeValue, RnaPath]]) -> Applied:
        """Writes stored or native values in host order, every write, resize, and skip reported once.

        `LEADING` members go first across every owner, each resetting members it governs, then each root writes its members, resizes its collections, and descends.

        Args:
            owner: Struct or collection the tree's owner paths start from.
            tree: Values keyed by owner path.
            projected: Host value of one held value.

        Returns:
            Writes, unchanged and skipped paths, and accumulated faults.
        """
        leads = tuple(
            (struct, key, Descriptor(root=type(struct), prop=prop), held.values[prop.identifier])
            for prop in LEADING
            for key, held in tree.owners.items()
            if prop.identifier in held.values
            for struct in struct_at(owner, key).bind(lambda found: Option.of_optional(try_downcast(bpy.types.bpy_struct, found))).to_list()
            if definition(struct).properties.get(prop.identifier) == prop
        )
        completed = frozenset((key, descriptor.prop.identifier) for _, key, descriptor, _ in leads)

        def member(struct: Struct, key: RnaPath, descriptor: Descriptor, value: W) -> Applied:
            desired = projected(descriptor, value)
            return cls._applied(struct, desired, _projected) if isinstance(desired, Tree) else _reported((*key, *descriptor.path), descriptor.write(struct, desired))

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
            rows = tuple({name: projected(Descriptor(root=type(fixed), prop=held[name]), value) for name, value in tree.owners[at].values.items() if name in held} for at in tree.owners[path].children)
            items: _Elements = getattr(struct, prop.identifier)
            subject = f"{struct!r}.{prop.identifier}"
            match Option.of_optional(_ADDED.get(type(fixed))), fixed:
                case Option(tag="some", some=adding), _:
                    return changed(items, path, rows, adding, indexed=False).map_error(partial(Fault.of, BlenderReason.REFUSED, subject)).default_with(Applied.refused)
                case _, bpy.types.PropertyGroup() if prop.is_runtime:
                    return changed(items, path, rows, lambda grown, _: grown.add(), indexed=True).map_error(partial(Fault.of, BlenderReason.REFUSED, subject)).default_with(Applied.refused)
                case _:
                    present = sum(item is not None for item in items)
                    return Applied() if present == len(rows) else Applied.refused(Fault.of(RecordReason.OUTSIDE, subject, str(len(rows)), member=str(present)))

        def written(struct: Struct, key: RnaPath) -> Applied:
            members = tuple((name, value, Descriptor.resolve(struct, name, bpy.types.Property).to_option()) for name, value in tree.owners[key].values.items() if (key, name) not in completed)
            ordered = sequenced(
                (descriptor.owner, holder, descriptor, value)
                for _, value, found in members
                for descriptor in found.to_list()
                for holder in struct_at(struct, descriptor.owner).to_list()
                if isinstance(holder, bpy.types.bpy_struct)
            )
            result = (
                Applied(skipped=tuple((*key, name) for name, _, found in members if found.is_none()))
                .then(lambda: Applied.join(member(holder, key, descriptor, value) for _, holder, descriptor, value in ordered))
                .then(
                    lambda: Applied.join(
                        resized(struct, (*key, prop.identifier), prop, fixed)
                        for prop in definition(struct).properties
                        if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID)
                        for fixed in Option.of_optional(prop.fixed_type).to_list()
                    )
                )
                .then(lambda: Applied.join(located(child) for child in tree.owners[key].children))
            )
            if isinstance(struct, bpy.types.CurveMapping | bpy.types.CurveProfile):
                struct.update()
            return result

        def located(key: RnaPath) -> Applied:
            match struct_at(owner, key).to_optional():
                case bpy.types.bpy_struct() as struct:
                    return written(struct, key)
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

    @staticmethod
    def persisted(tree: Tree[NativeValue, RnaPath]) -> Outcome[_Persisted]:
        """Converts native restoration tree to ID-keyed stored form."""

        def member(row: NativeValue) -> Outcome[tuple[tuple[object, RnaPath], object]]:
            target, value = row
            if not isinstance(target, tuple):
                return Error(Fault.of(RecordReason.OUTSIDE, repr(target), type(target).__name__, member=bpy.types.ID.__name__))
            if isinstance(value, Tree):
                return Snapshot.persisted(value).map(lambda held: (target, held))
            if isinstance(value, bpy.types.bpy_struct) and not isinstance(value, bpy.types.ID):
                return Error(Fault.of(RecordReason.OUTSIDE, repr(target), type(value).__name__, member=bpy.types.ID.__name__))
            return Ok((target, value))

        return accumulated(
            accumulated(member(value).map(lambda row: (name, row)) for name, value in held.values.items()).map(lambda values: (key, Owner(values=dict(values), children=held.children)))
            for key, held in tree.owners.items()
        ).map(lambda rows: Tree(roots=tree.roots, rows=rows))

    @staticmethod
    def decoded(tree: _Persisted, *, compound: bool = False) -> Outcome[Tree[NativeValue, RnaPath]]:
        """Validates stored ID references and rebuilds keys from current IDs, members of removed IDs left out.

        Args:
            tree: Stored tree.
            compound: Whether tree holds one compound member's items, its keys kept and its members resolved by type.

        Returns:
            Native tree, domain fault for a stored value no member accepts.
        """

        def value_of(descriptor: Descriptor, value: object) -> Outcome[Value | Struct | Tree[NativeValue, RnaPath]]:
            subject = rna(descriptor.path)
            match descriptor.prop:
                case bpy.types.CollectionProperty() | bpy.types.PointerProperty(is_readonly=True):
                    nested: Outcome[_Persisted] = catch(msgspec.convert, exception=msgspec.ValidationError)(value, type=_Persisted).map_error(
                        partial(Fault.of, RecordReason.OUTSIDE, subject, member=Value.__name__)
                    )
                    return nested.bind(partial(Snapshot.decoded, compound=True))
                case bpy.types.PointerProperty() if value is None or isinstance(value, bpy.types.ID):
                    return Ok(value)
                case bpy.types.PointerProperty():
                    return Error(Fault.of(RecordReason.OUTSIDE, subject, type(value).__name__, member=bpy.types.ID.__name__))
                case _:
                    held: Outcome[Value] = catch(msgspec.convert, exception=msgspec.ValidationError)(value, Value).map_error(partial(Fault.of, RecordReason.OUTSIDE, subject, member=Value.__name__))
                    return held

        def member(key: RnaPath, name: str, row: tuple[tuple[object, RnaPath], object]) -> Outcome[Option[tuple[RnaPath, str, NativeValue]]]:
            (block, path), value = row
            match block:
                case None:
                    return Ok(Nothing)
                case bpy.types.ID():
                    found = (
                        Descriptor.resolve(type(block), (*path, name), bpy.types.Property).to_option()
                        if compound
                        else struct_at(block, path).bind(lambda struct: Descriptor.resolve(struct, name, bpy.types.Property).to_option())
                    )
                    return found.map(
                        lambda descriptor: value_of(descriptor, value).map(lambda held: Some((key if compound else (block.session_uid, *path), name, ((block, path), held))))
                    ).default_value(Ok(Nothing))
                case _:
                    return Error(Fault.of(RecordReason.OUTSIDE, rna(path), type(block).__name__, member=bpy.types.ID.__name__))

        def rebuilt(rows: tuple[tuple[RnaPath, str, NativeValue], ...]) -> Tree[NativeValue, RnaPath]:
            keys = tuple(tree.owners) if compound else tuple(dict.fromkeys(at for at, _, _ in rows))
            return Tree(
                roots=tree.roots if compound else keys,
                rows=tuple((key, Owner[NativeValue, RnaPath](values={name: value for at, name, value in rows if at == key}, children=tree.owners[key].children if compound else ())) for key in keys),
            )

        return accumulated(member(key, name, row) for key, held in tree.owners.items() for name, row in held.values.items()).map(
            lambda found: rebuilt(tuple(row for each in found for row in each.to_list()))
        )

    @staticmethod
    def reverted(before: _Persisted, after: _Persisted) -> Outcome[None]:
        """Decodes stored trees and restores values still at after state."""
        return paired(Snapshot.decoded(before), Snapshot.decoded(after)).bind(lambda pair: Snapshot.restore(*pair).outcome)

    @staticmethod
    def restore(before: Tree[NativeValue, RnaPath], after: Tree[NativeValue, RnaPath]) -> Applied:
        """Restores live values still at after state."""

        def resolved(target: NativeOwner, name: str) -> tuple[tuple[Native, Descriptor], ...]:
            located = struct_at(*target) if isinstance(target, tuple) else Some(target)
            return tuple((struct, descriptor) for struct in located.to_list() for descriptor in Descriptor.resolve(struct, name, bpy.types.Property).to_option().to_list())

        def actual(key: RnaPath, name: str, target: NativeOwner) -> tuple[Result[NativeValue, Applied], ...]:
            return tuple(
                native(struct, Some((descriptor,)))
                .map_error(Applied.refused)
                .bind(lambda tree: Block.of_seq(row for held in tree.owners.values() for row in held.values.values()).try_head().to_result_with(lambda: Applied(skipped=((*key, name),))))
                for struct, descriptor in resolved(target, name)
            )

        def assigned(path: RnaPath, struct: Struct, descriptor: Descriptor, value: Value | Struct | Tree[NativeValue, RnaPath]) -> Applied:
            return (
                Snapshot._applied(struct, value, _projected)
                if isinstance(value, Tree)
                else _reported(path, Descriptor.resolve(struct, descriptor.path, bpy.types.Property).bind(lambda found: found.write(struct, value)))
            )

        missing = tuple(
            Applied(outcome=Error(Fault.of(BlenderReason.ABSENT, repr(struct), member=rna(descriptor.path))))
            for key, held in before.owners.items()
            for name, (target, _) in held.values.items()
            if key not in after.owners or name not in after.owners[key].values
            for struct, descriptor in resolved(target, name)
        )
        captures = {key: tuple((name, actual(key, name, target)) for name, (target, _) in held.values.items()) for key, held in after.owners.items()}
        current = msgspec.structs.replace(
            after,
            rows=tuple(
                (key, msgspec.structs.replace(held, values={name: value for name, results in captures[key] for result in results for value in result.to_option().to_list()}))
                for key, held in after.rows
            ),
        )
        ordered = sequenced(
            (edit.key, struct, descriptor, ((*edit.key, name), value))
            for edit in revert(current, transfer(before, after))
            for name, (target, value) in edit.after.items()
            for struct, descriptor in resolved(target, name)
            if isinstance(struct, bpy.types.bpy_struct)
        )
        writes = tuple((struct, assigned(path, struct, descriptor, value)) for _, struct, descriptor, (path, value) in ordered)
        for curve in {struct.as_pointer(): struct for struct, _ in writes if isinstance(struct, bpy.types.CurveMapping | bpy.types.CurveProfile)}.values():
            curve.update()
        return Applied.join((
            *missing,
            *(fault for rows in captures.values() for _, results in rows for result in results for fault in result.swap().to_option().to_list()),
            *(report for _, report in writes),
        ))


class Endpoints(msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    """Stored before and after values of one operation, after unset until write finishes."""

    before: _Persisted
    after: _Persisted | None = None

    def completed(self, owner: object, key: str) -> Outcome[_Persisted]:
        """Returns stored after values, absent fault before write finishes."""
        return Option.of_optional(self.after).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(owner), member=f"{key}.after"))

    def decoded(self, owner: object, key: str) -> Outcome[tuple[Tree[NativeValue, RnaPath], Tree[NativeValue, RnaPath]]]:
        """Decodes before and after trees, accumulating faults of both."""
        return paired(Snapshot.decoded(self.before), self.completed(owner, key).bind(Snapshot.decoded))


class Scenes(Case, frozen=True, kw_only=True):
    """Named or every non-runtime scene in file order."""

    owner_type: ClassVar[type[bpy.types.Scene]] = bpy.types.Scene
    names: tuple[str, ...] | None = None

    def owners(self, _context: bpy.types.Context) -> Outcome[tuple[bpy.types.Scene, ...]]:
        """Returns scenes, absent fault per missing name."""
        return (
            Option
            .of_optional(self.names)
            .map(lambda names: accumulated(Option.of_optional(bpy.data.scenes.get(name)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(bpy.data.scenes), member=name)) for name in names))
            .default_with(lambda: Ok(tuple(scene for scene in bpy.data.scenes if not scene.is_runtime_data)))
        )


class ViewLayers(Case, frozen=True, kw_only=True):
    """View layers of selected scenes."""

    owner_type: ClassVar[type[bpy.types.ViewLayer]] = bpy.types.ViewLayer
    scenes: tuple[str, ...] | None = None

    def owners(self, context: bpy.types.Context) -> Outcome[tuple[bpy.types.ViewLayer, ...]]:
        """Returns view layers in scene and layer order."""
        return Scenes(names=self.scenes).owners(context).map(lambda owners: tuple(layer for scene in owners for layer in scene.view_layers))


class Selection(Case, frozen=True, kw_only=True):
    """Objects selected in context view layer."""

    owner_type: ClassVar[type[bpy.types.Object]] = bpy.types.Object

    @staticmethod
    def owners(context: bpy.types.Context) -> Outcome[tuple[bpy.types.Object, ...]]:
        """Returns selected objects in view layer order."""
        return rooted(bpy.types.ViewLayer, context).map(lambda layer: tuple(layer.objects.selected))


class Objects(Case, frozen=True, kw_only=True):
    """Objects of named collection and its descendants."""

    owner_type: ClassVar[type[bpy.types.Object]] = bpy.types.Object
    collection: str

    def owners(self, _context: bpy.types.Context) -> Outcome[tuple[bpy.types.Object, ...]]:
        """Returns collection objects."""
        return (
            Option
            .of_optional(bpy.data.collections.get(self.collection))
            .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(bpy.data.collections), member=self.collection))
            .map(lambda collection: tuple(collection.all_objects))
        )


class _Walk[W](msgspec.Struct, frozen=True, kw_only=True):
    """Capture policy: value reader, native compounds keyed by session, the compound a nested walk expands, and the structs enclosing it."""

    read: Callable[[Descriptor, Native, _Enclosing], Option[W]]
    native: bool = False
    expanded: Option[Descriptor] = Nothing
    enclosing: _Enclosing = frozenset()


# --- [TABLES] ---------------------------------------------------------------------------

_SCOPED: Final[Mapping[str, type[Scope]]] = MappingProxyType({case.__name__: case for case in (Scenes, ViewLayers, Selection, Objects)})
SCOPES: Final = choices((name, bpy.path.display_name(name), inspect.getdoc(case) or "") for name, case in _SCOPED.items())

# --- [OPERATIONS] -----------------------------------------------------------------------


def native(owner: Native, selection: Option[tuple[Descriptor, ...]] = Nothing, prior: Option[Tree[NativeValue, RnaPath]] = Nothing) -> Outcome[Tree[NativeValue, RnaPath]]:
    """Captures live values for restoration, owners keyed by ID session and path, collections and curves held whole.

    Args:
        owner: Struct whose members the tree holds.
        selection: Members to capture, every saved member when Nothing.
        prior: Tree whose values the capture keeps, adding paths it lacks.

    Returns:
        Native tree, domain fault for an undecodable value.
    """
    return catch(_walked, exception=msgspec.ValidationError)(owner, owner, _Walk(read=partial(_native_read, owner), native=True), selection, prior).map_error(
        partial(Fault.of, RecordReason.OUTSIDE, repr(owner), member=Value.__name__)
    )


def scoped(kind: str, collection: str) -> Scope:
    """Returns the scope a `SCOPES` choice names, a collection's objects by its name."""
    return Objects(collection=collection) if issubclass(case := _SCOPED[kind], Objects) else case()


def _native_read(owner: Native, descriptor: Descriptor, struct: Native, enclosing: _Enclosing) -> Option[NativeValue]:
    """Reads one member for restoration under owner: compounds walked whole, pointers held, values as read."""
    prop, target = descriptor.prop, _reference(owner, struct, descriptor.owner)
    if isinstance(prop, bpy.types.CollectionProperty) or (_curve(prop) and prop.is_readonly):
        selected = Descriptor(root=type(struct), prop=prop)
        return Some((target, _walked(owner, struct, _Walk(read=partial(_native_read, owner), native=True, expanded=Some(selected), enclosing=enclosing), Some((selected,)))))
    return Some((target, getattr(struct, prop.identifier) if isinstance(prop, bpy.types.PointerProperty) else descriptor.read(struct)))


def _walked[W](owner: Native, source: Native, walk: _Walk[W], chosen: Option[tuple[Descriptor, ...]], prior: Option[Tree[W, RnaPath]] = Nothing) -> Tree[W, RnaPath]:
    """Walks source into a value tree, the chosen members alone when given, extending prior.

    Each struct is walked once, and a struct enclosing a nested walk ends that walk's branch.

    Args:
        owner: Struct the capture started from, anchoring native references.
        source: Struct or collection this walk reads.
        walk: Capture policy.
        chosen: Members to capture, every saved member when Nothing.
        prior: Tree whose values the walk keeps.

    Returns:
        Tree keyed by owner path, a native top-level walk keying by session and path or by pointer.
    """
    owners: dict[RnaPath, Owner[W, RnaPath]] = prior.map(lambda tree: dict(tree.owners)).default_with(dict)
    seen: set[tuple[int, str]] = set(walk.enclosing)
    sessioned, indexed = walk.native and walk.expanded.is_none(), walk.expanded.is_some()

    def beneath(path: RnaPath, root: RnaPath) -> bool:
        return len(path) > len(root) and path[: len(root)] == root

    def keyed(struct: Native, path: RnaPath) -> RnaPath:
        if not sessioned:
            return path
        target = _reference(owner, struct, path)
        return (target[0].session_uid, *target[1]) if isinstance(target, tuple) else (struct.as_pointer() if isinstance(struct, bpy.types.bpy_struct) else id(struct),)

    def arrayed(prop: bpy.types.Property, names: Collection[str]) -> TypeIs[bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty]:
        return (
            isinstance(prop, bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty)
            and prop.is_array
            and any(rna((prop.identifier, *at)) in names for at in np.ndindex(*Descriptor.dimensions(prop)))
        )

    def address(struct: Native) -> tuple[int, str]:
        return (struct.as_pointer() if isinstance(struct, bpy.types.bpy_struct) else id(struct), definition(struct).identifier)

    def branched(struct: Native, key: RnaPath, prop: bpy.types.Property, inside: _Enclosing) -> tuple[RnaPath, ...]:
        path = (*key, prop.identifier)
        match prop:
            case bpy.types.PointerProperty(fixed_type=bpy.types.ID()) | bpy.types.CollectionProperty(fixed_type=bpy.types.ID()):
                return ()  # type: ignore[unreachable]
            case bpy.types.CollectionProperty() if path in owners:
                return owners[path].children
            case bpy.types.PointerProperty(is_readonly=True):
                return tuple(at for child in Option.of_optional(getattr(struct, prop.identifier)).filter(effective).to_list() for at in walked(child, path, inside).to_list())
            case bpy.types.CollectionProperty():
                items = tuple((index, item) for index, item in enumerate(getattr(struct, prop.identifier)) if item is not None)
                named: Option[tuple[RnaPath, ...]] = (
                    Option
                    .of_optional(prop.fixed_type)
                    .bind(lambda fixed: Option.of_optional(fixed.name_property))
                    .map(lambda held: tuple(getattr(item, held.identifier) for _, item in items))
                    .filter(lambda found: not indexed and len(set(found)) == len(found))
                    .map(lambda found: tuple(((name,),) for name in found))
                )
                steps = named.default_with(lambda: tuple((index,) for index, _ in items))
                return tuple(at for step, (_, item) in zip(steps, items) for at in walked(item, (*path, *step), inside).to_list())
            case _:
                return ()

    def grouped(struct: Native, path: RnaPath, prop: bpy.types.Property, keys: tuple[RnaPath, ...]) -> tuple[RnaPath, ...]:
        if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID) and (keys or not tuple(getattr(struct, prop.identifier))):
            owners.setdefault(row := (*path, prop.identifier), Owner(values={}, children=keys))
            return (row,)
        return keys

    def walked(struct: Native, path: RnaPath, above: _Enclosing) -> Option[RnaPath]:
        if (identity := address(struct)) in seen:
            return Nothing
        seen.add(identity)
        inside, key = above | {identity}, keyed(struct, path)
        held_definition = definition(struct)
        members = (
            *Descriptor.saved(struct),
            *(Descriptor(root=type(struct), prop=prop) for prop in Option.of_optional(held_definition.name_property).filter(lambda prop: indexed and not prop.is_readonly).to_list()),
        )
        held = owners.setdefault(key, Owner(values={}))
        values = {
            descriptor.prop.identifier: value
            for descriptor in members
            if _valued(descriptor.prop, native=walk.native) and descriptor.prop.identifier not in held.values and not arrayed(descriptor.prop, held.values)
            for value in walk.read(descriptor, struct, walk.enclosing | inside).to_list()
        }
        branches = tuple(
            (descriptor.prop, branched(struct, path, descriptor.prop, inside))
            for descriptor in members
            if not (walk.native and (isinstance(descriptor.prop, bpy.types.CollectionProperty) or _curve(descriptor.prop)))
        )
        nested = tuple(at for prop, keys in branches for at in grouped(struct, path, prop, keys))
        owners[key] = Owner(values={**held.values, **values}, children=tuple(dict.fromkeys((*held.children, *nested))))
        for descriptor in members:
            if arrayed(descriptor.prop, held.values):
                for at in np.ndindex(*Descriptor.dimensions(descriptor.prop)):
                    picked(Descriptor(root=type(source), owner=path, prop=descriptor.prop, coordinates=at), inside)
        return Some(key)

    def picked(descriptor: Descriptor, above: _Enclosing = frozenset()) -> tuple[RnaPath, ...]:
        def member(struct: Native) -> tuple[RnaPath, ...]:
            inside = above | {address(struct)}
            if sessioned and isinstance(source, bpy.types.bpy_struct) and isinstance(struct, bpy.types.bpy_struct):
                enclosing = (
                    Descriptor(root=type(source), owner=at[len(base) :], prop=found.prop)
                    for base, path in id_path(source).bind(lambda base: id_path(struct).map(lambda own: (base, (*own, descriptor.prop.identifier)))).to_list()
                    for ancestor in struct.rna_ancestors()
                    for at in id_path(ancestor).to_list()
                    for found in Descriptor.saved(ancestor)
                    if (isinstance(found.prop, bpy.types.CollectionProperty) and not isinstance(found.prop.fixed_type, bpy.types.ID)) or (_curve(found.prop) and found.prop.is_readonly)
                    for prefix in ((*at, found.prop.identifier),)
                    if beneath(prefix, base) and beneath(path, prefix)
                )
                for found in Block.of_seq(enclosing).try_head().to_list():
                    return picked(found, above)
            if walk.expanded != Some(descriptor) and _valued(descriptor.prop, native=walk.native):
                key, name = keyed(struct, descriptor.owner), rna((descriptor.prop.identifier, *descriptor.coordinates))
                if walk.native and any(beneath(key, (*at, held_name)) for at, held in owners.items() for held_name in held.values):
                    return ()
                if key in owners and (name in owners[key].values or descriptor.prop.identifier in owners[key].values):
                    return (key,)
                if not descriptor.coordinates and key in owners and arrayed(descriptor.prop, owners[key].values):
                    return tuple(at for coordinates in np.ndindex(*Descriptor.dimensions(descriptor.prop)) for at in picked(msgspec.structs.replace(descriptor, coordinates=coordinates), above))
                for value in walk.read(descriptor, struct, walk.enclosing | inside).to_list():
                    owners[key] = msgspec.structs.replace(held := owners.get(key, Owner(values={})), values={**held.values, name: value})
                    return (key,)
                return ()
            nested = branched(struct, descriptor.owner, descriptor.prop, inside)
            if walk.expanded == Some(descriptor) or isinstance(descriptor.prop, bpy.types.CollectionProperty):
                if isinstance(descriptor.prop, bpy.types.CollectionProperty):
                    owners.setdefault(descriptor.path, Owner(values={}, children=nested))
                    nested = (descriptor.path,)
                held = owners.setdefault(descriptor.owner, Owner(values={}))
                owners[descriptor.owner] = msgspec.structs.replace(held, children=tuple(dict.fromkeys((*held.children, *nested))))
                return (descriptor.owner,)
            return nested

        return descriptor.owner_of(source).map(member).default_value(())

    roots = (
        *prior.map(lambda tree: tree.roots).default_value(()),
        *chosen.map(lambda selected: tuple(key for descriptor in selected for key in picked(descriptor))).default_with(lambda: tuple(walked(source, (), frozenset()).to_list())),
    )
    nested = frozenset(child for held in owners.values() for child in held.children)
    return Tree(roots=tuple(dict.fromkeys(key for key in roots if key not in nested)), rows=tuple(owners.items()))


def _reference(owner: Native, struct: Native, path: RnaPath) -> NativeOwner:
    """Returns struct as its ID and path, a collection through owner's ID path, else struct itself."""
    match struct, owner:
        case bpy.types.bpy_struct(id_data=bpy.types.ID() as block), _:
            return id_path(struct).map(lambda found: (block, found)).default_value(struct)
        case bpy.types.bpy_prop_collection(), bpy.types.bpy_struct(id_data=bpy.types.ID() as block):
            return id_path(owner).map(lambda found: (block, (*found, *path))).default_value(struct)
        case _:
            return struct


def _valued(prop: bpy.types.Property, *, native: bool = False) -> bool:
    """Whether property captures as value, native adding struct collections and curve or writable pointers."""
    match prop:
        case bpy.types.CollectionProperty():
            return native and not isinstance(prop.fixed_type, bpy.types.ID)
        case bpy.types.PointerProperty(is_readonly=True):
            return native and _curve(prop)
        case bpy.types.PointerProperty():
            return native or isinstance(prop.fixed_type, bpy.types.ID)
        case _:
            return not prop.is_readonly


def _curve(prop: bpy.types.Property) -> bool:
    """Whether property points at a curve mapping or profile."""
    return isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.CurveMapping | bpy.types.CurveProfile)


def _projected(_: Descriptor, row: NativeValue) -> Value | Struct | Tree[NativeValue, RnaPath]:
    """Returns the value of a native row."""
    return row[1]


def _reported(path: RnaPath, answer: Outcome[Option[Value | Struct]]) -> Applied:
    """Classifies write as written, unchanged, or refused."""
    return answer.map(lambda prior: Applied(written=(path,)) if prior.is_some() else Applied(unchanged=(path,))).default_with(Applied.refused)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SCOPES", "Applied", "Endpoints", "NativeValue", "Objects", "Scenes", "Scope", "Scoped", "Selection", "Snapshot", "ViewLayers", "native", "scoped"]
