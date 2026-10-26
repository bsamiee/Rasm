"""RNA value trees, ordered writes, restoration, and owner scopes."""

from collections.abc import Callable, Iterable, Mapping
from functools import reduce
from itertools import count, takewhile
from typing import Annotated, ClassVar, Protocol, Self, TypeGuard

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np

from blender.bpy.data.rna.descriptors import beneath, Descriptor, effective, id_path, LEADING, member_path, Native, rooted, sequenced, struct_at, Value
from blender.bpy.data.rna.edits import Owner, revert, transfer, Tree
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.imaging.faults import Access, accumulated, ImagingFileFault, paired
from blender.imaging.parameters import Case

# --- [TYPES] ----------------------------------------------------------------------------


type Scope = Scenes | ViewLayers | Selection | Objects
type NativeOwner = tuple[bpy.types.ID, str] | Native
type NativeValue = tuple[NativeOwner, Value | bytes | bpy.types.bpy_struct[object] | Tree[NativeValue]]
type Persisted = Tree[tuple[tuple[object, str], object]]


class Scoped[O: "bpy.types.bpy_struct[object]" = "bpy.types.bpy_struct[object]"](Protocol):
    """Context-selected owners of one struct type."""

    @property
    def owner_type(self) -> type[O]:
        """Owner struct type."""
        ...

    def owners(self, context: bpy.types.Context, /) -> BlenderResult[tuple[O, ...]]:
        """Returns selected owners in stable order."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_FLOAT_STORAGE = np.dtype(np.float64).newbyteorder(">")

# --- [MODELS] ---------------------------------------------------------------------------


class Applied(msgspec.Struct, frozen=True, kw_only=True):
    """Snapshot write outcome per path."""

    written: tuple[str, ...] = ()
    unchanged: tuple[str, ...] = ()
    skipped: tuple[str, ...] = ()
    outcome: BlenderResult[None] = Ok(None)

    @classmethod
    def join(cls, outcomes: tuple[Self, ...]) -> Self:
        """Concatenates each path category in operation order."""
        return cls(
            written=tuple(path for outcome in outcomes for path in outcome.written),
            unchanged=tuple(path for outcome in outcomes for path in outcome.unchanged),
            skipped=tuple(path for outcome in outcomes for path in outcome.skipped),
            outcome=accumulated(outcome.outcome for outcome in outcomes).map(lambda _: None),
        )

    @classmethod
    def refused(cls, faults: BlenderFaults) -> Self:
        """Returns outcome failing with faults."""
        return cls(outcome=Error(faults))


class Snapshot(msgspec.Struct, frozen=True, kw_only=True):
    """Assignable values keyed by path under struct type."""

    struct: str
    tree: Tree[Value]

    # --- [CAPTURE]

    @classmethod
    def of(cls, struct: "type[bpy.types.bpy_struct[object]]", values: Mapping[str, Value]) -> Self:
        """Returns snapshot of struct member values."""
        return cls(struct=struct.bl_rna.identifier, tree=Tree(roots=("",), owners={"": Owner(values=dict(values))}))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    @classmethod
    def capture(cls, owner: Native, selection: Option[tuple[Descriptor, ...]] = Nothing) -> BlenderResult[Self]:
        """Captures assignable values keyed by path, the selected members alone when given.

        Args:
            owner: Struct or collection whose members the snapshot holds.
            selection: Members to capture, every saved member when Nothing.

        Returns:
            Snapshot, read fault for an undecodable value, edit fault for a stale owner.
        """

        def stored(descriptor: Descriptor, struct: Native, _: frozenset[tuple[int, str]]) -> Option[Value]:
            value = descriptor.read(struct)
            return Nothing if isinstance(descriptor.prop, bpy.types.EnumProperty) and not descriptor.prop.is_enum_flag and not bool(value) else Some(value)

        return _attempted(owner, lambda: cls(struct=owner.bl_rna.identifier, tree=_walked(owner, owner, _Walk(read=stored), selection)))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    @staticmethod
    def settings(setting: Descriptor, compound: "tuple[type[bpy.types.bpy_struct[object]], ...]" = ()) -> tuple[Descriptor, ...]:
        """Expands setting into leaf descriptors, `compound` pointers kept whole."""

        def walked(struct: "type[bpy.types.bpy_struct[object]]", owner: str, entered: frozenset[str]) -> tuple[Descriptor, ...]:
            def member(prop: bpy.types.Property) -> tuple[Descriptor, ...]:
                match prop:
                    case _ if _valued(prop) or (isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, compound)):
                        return (Descriptor(root=setting.root, owner=owner, prop=prop),)
                    case bpy.types.PointerProperty(is_readonly=True, fixed_type=bpy.types.bpy_struct() as pointed) if not isinstance(pointed, bpy.types.ID):  # type: ignore[unreachable]
                        return walked(type(pointed), member_path(owner, prop.identifier), entered | {pointed.identifier}) if pointed.identifier not in entered else ()
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
        match _attempted(owner, lambda: owner.bl_rna.identifier):  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            case Result(tag="ok", ok=identifier) if identifier == self.struct:
                return self._applied(owner, self.tree, lambda _, value: value)
            case Result(tag="ok", ok=identifier):
                return Applied.refused((BlenderChoiceFault(path=repr(owner), identifier=self.struct, accepted=(identifier,)),))
            case Result(error=faults):
                return Applied.refused(faults)

    def applied[O: "bpy.types.bpy_struct[object]"](self, owners: tuple[O, ...]) -> tuple[Applied, ...]:
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
                if isinstance(descriptor.prop, bpy.types.PointerProperty | bpy.types.CollectionProperty)
            ),
            *(
                descriptor
                for key, held in self.tree.owners.items()
                for name in held.values
                for descriptor in Descriptor.resolve(owner, member_path(key, name), bpy.types.Property).to_option().to_list()
            ),
        )

    def projected(self, owner: Native, current: "Snapshot", locked: frozenset[str] = frozenset()) -> "BlenderResult[tuple[Snapshot, Snapshot]]":
        """Returns unlocked assignments with resulting current state."""

        def within(path: str, roots: frozenset[str]) -> bool:
            return any(path == root or beneath(path, root) for root in roots)

        def split(identifier: str) -> BlenderResult[tuple[Snapshot, Snapshot]]:
            if incompatible := tuple(BlenderChoiceFault(path=repr(owner), identifier=snapshot.struct, accepted=(identifier,)) for snapshot in (self, current) if snapshot.struct != identifier):
                return Error((incompatible[0], *incompatible[1:]))
            free = {
                key: Owner(
                    values={name: value for name, value in held.values.items() if not within(member_path(key, name), locked)},
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
            values = {key: held for key, held in current.tree.owners.items() if not within(key, compounds)}
            for key, held in free.items():
                previous = values.get(key, Owner(values={}))
                values[key] = held if within(key, compounds) else Owner(values=previous.values | held.values, children=tuple(dict.fromkeys((*previous.children, *held.children))))
            nested = frozenset(child for held in values.values() for child in held.children)
            sparse = msgspec.structs.replace(self, tree=Tree(roots=tuple(key for key in self.tree.roots if key in free), owners=free))
            matching = msgspec.structs.replace(self, tree=Tree(roots=tuple(dict.fromkeys(key for key in (*current.tree.roots, *sparse.tree.roots) if key not in nested)), owners=values))
            return Ok((sparse, matching))

        return _attempted(owner, lambda: owner.bl_rna.identifier).bind(split)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    @classmethod
    def _applied[W](cls, owner: Native, tree: Tree[W], projected: "Callable[[Descriptor, W], Value | bytes | bpy.types.bpy_struct[object] | Tree[NativeValue]]") -> Applied:
        """Writes stored or native values in host order, every write, resize, and skip reported once."""
        reports: list[Applied] = []
        completed: set[tuple[str, str]] = set()

        def every(results: Iterable[BlenderResult[None]]) -> BlenderResult[None]:
            return accumulated(results).map(lambda _: None)

        def member(struct: Native, key: str, descriptor: Descriptor, value: W) -> BlenderResult[None]:
            desired = projected(descriptor, value)
            reports.append(
                cls._applied(struct, desired, lambda found, row: cls._value(found, row[1]))
                if isinstance(desired, Tree)
                else cls._reported(member_path(key, descriptor.path), descriptor.write(struct, desired))
            )
            return reports[-1].outcome

        def lead(prop: bpy.types.Property) -> BlenderResult[None]:
            steps = tuple(
                (struct, key, held.values[prop.identifier])
                for key, held in tree.owners.items()
                if prop.identifier in held.values
                for struct in struct_at(owner, key).to_list()
                if struct.bl_rna.properties.get(prop.identifier) == prop  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            )
            completed.update((key, prop.identifier) for _, key, _ in steps)
            return every(member(struct, key, Descriptor(root=type(struct), owner="", prop=prop), value) for struct, key, value in steps)

        def resized(struct: Native, key: str, prop: bpy.types.CollectionProperty) -> BlenderResult[None]:
            if (path := member_path(key, prop.identifier)) not in tree.owners and f"{path}[0]" not in tree.owners:
                return Ok(None)
            keys = tree.owners[path].children if path in tree.owners else tuple(takewhile(tree.owners.__contains__, (f"{path}[{index}]" for index in count())))
            items = getattr(struct, prop.identifier)
            fixed: bpy.types.bpy_struct[object] = prop.fixed_type  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            added: Callable[[dict[str, Value | bytes | bpy.types.bpy_struct[object] | Tree[NativeValue]]], object]
            match fixed:
                case bpy.types.CurveMapPoint():
                    added = lambda values: items.new(*np.asarray(values["location"], dtype=np.float64))
                case bpy.types.CurveProfilePoint():
                    added = lambda values: items.add(*np.asarray(values["location"], dtype=np.float64))
                case bpy.types.ColorRampElement():
                    added = lambda values: items.new(values["position"])
                case bpy.types.PropertyGroup() if prop.is_runtime:
                    added = lambda _: items.add()
                case bpy.types.bpy_struct():
                    present = sum(item is not None for item in items)
                    return Ok(None) if present == len(keys) else Error((BlenderChoiceFault(path=f"{struct!r}.{prop.identifier}", identifier=str(len(keys)), accepted=(str(present),)),))

            def changed() -> None:
                properties = fixed.bl_rna.properties
                reports.append(Applied(written=(path,) if len(items) != len(keys) else ()))
                for at in keys[len(items) :]:
                    added({name: projected(Descriptor(root=type(fixed), owner="", prop=properties[name]), value) for name, value in tree.owners[at].values.items() if name in properties})
                for _ in range(len(items) - len(keys)):
                    items.remove(len(items) - 1 if isinstance(fixed, bpy.types.PropertyGroup) and prop.is_runtime else items[-1])

            return BlenderEditFault.attempted(f"{struct!r}.{prop.identifier}", changed)

        def located(key: str) -> BlenderResult[None]:
            def written(struct: Native) -> BlenderResult[None]:
                definitions = tuple(struct.bl_rna.properties)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                members = tuple((name, value, Descriptor.resolve(struct, name, bpy.types.Property).to_option()) for name, value in tree.owners[key].values.items() if (key, name) not in completed)
                reports.append(Applied(skipped=tuple(member_path(key, name) for name, _, descriptor in members if descriptor.is_none())))
                ordered = sequenced(
                    (descriptor.owner, holder, descriptor, value) for _, value, found in members for descriptor in found.to_list() for holder in struct_at(struct, descriptor.owner).to_list()
                )
                result = (
                    every(member(holder, key, descriptor, value) for _, holder, descriptor, value in ordered)
                    .bind(lambda _: every(resized(struct, key, prop) for prop in definitions if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID)))
                    .bind(lambda _: every(located(child) for child in tree.owners[key].children))
                )
                if isinstance(struct, bpy.types.CurveMapping | bpy.types.CurveProfile):
                    struct.update()
                return result

            match struct_at(owner, key):
                case Option(tag="some", some=bpy.types.bpy_prop_collection()):
                    return every(located(child) for child in tree.owners[key].children)
                case Option(tag="some", some=struct):
                    return written(struct)
                case Option():
                    reports.append(Applied(skipped=(key,)))
                    return Ok(None)

        try:
            outcome = reduce(lambda state, prop: state.bind(lambda _: lead(prop)), LEADING, Result[None, BlenderFaults](ok=None)).bind(lambda _: every(located(key) for key in tree.roots))
        except ReferenceError as error:
            outcome = Error((BlenderEditFault(request=repr(owner), error=error),))
        return msgspec.structs.replace(Applied.join(tuple(reports)), outcome=outcome)

    @staticmethod
    def persisted(tree: Tree[NativeValue]) -> BlenderResult[Persisted]:
        """Converts native restoration tree to ID-keyed stored form."""

        def member(row: NativeValue) -> BlenderResult[tuple[tuple[object, str], object]]:
            target, value = row
            if not isinstance(target, tuple):
                return Error((BlenderChoiceFault(path=repr(target), identifier=type(target).__name__, accepted=(bpy.types.ID.__name__,)),))
            if isinstance(value, Tree):
                return Snapshot.persisted(value).map(lambda held: (target, held))
            if isinstance(value, bpy.types.bpy_struct) and not isinstance(value, bpy.types.ID):
                return Error((BlenderChoiceFault(path=repr(target), identifier=type(value).__name__, accepted=(bpy.types.ID.__name__,)),))
            return Ok((target, value))

        return accumulated(
            accumulated(member(value).map(lambda row: (name, row)) for name, value in held.values.items()).map(lambda values: (key, Owner(values=dict(values), children=held.children)))
            for key, held in tree.owners.items()
        ).map(lambda owners: Tree(roots=tree.roots, owners=dict(owners)))

    @staticmethod
    def decoded(tree: Persisted, *, compound: bool = False) -> BlenderResult[Tree[NativeValue]]:
        """Validates stored ID references and rebuilds keys from current IDs."""
        owners: dict[str, Owner[NativeValue]] = {key: Owner(values={}, children=held.children) for key, held in tree.owners.items()} if compound else {}

        def member(key: str, name: str, row: tuple[tuple[object, str], object]) -> BlenderResult[None]:
            (block, path), value = row
            if block is None:
                return Ok(None)
            if not isinstance(block, bpy.types.ID):
                return Error((BlenderChoiceFault(path=path, identifier=type(block).__name__, accepted=(bpy.types.ID.__name__,)),))
            descriptors = (
                Descriptor.resolve(type(block), member_path(path, name), bpy.types.Property).to_option()
                if compound
                else struct_at(block, path).bind(lambda struct: Descriptor.resolve(struct, name, bpy.types.Property).to_option())
            )
            for descriptor in descriptors.to_list():
                answer: BlenderResult[Value | bytes | bpy.types.bpy_struct[object] | Tree[NativeValue]]
                match descriptor.prop:
                    case bpy.types.CollectionProperty() | bpy.types.PointerProperty(is_readonly=True):
                        answer = Snapshot.decoded(msgspec.convert(value, type=Persisted), compound=True)
                    case bpy.types.PointerProperty() if value is None or isinstance(value, bpy.types.ID):
                        answer = Ok(value)
                    case bpy.types.PointerProperty():
                        return Error((BlenderChoiceFault(path=member_path(path, name), identifier=type(value).__name__, accepted=(bpy.types.ID.__name__,)),))
                    case bpy.types.FloatProperty() as prop:
                        size = (prop.array_length if prop.is_array and descriptor.index.is_none() else 1) * _FLOAT_STORAGE.itemsize
                        answer = Ok(msgspec.convert(value, Annotated[bytes, msgspec.Meta(min_length=size, max_length=size)]))
                    case _:
                        answer = Ok(msgspec.convert(value, Value))
                match answer:
                    case Result(tag="ok", ok=held):
                        owners.setdefault(key if compound else _session_key(block, path), Owner(values={}, children=tree.owners[key].children if compound else ())).values[name] = ((block, path), held)
                    case Result(error=faults):
                        return Error(faults)
            return Ok(None)

        try:
            return accumulated(member(key, name, row) for key, held in tree.owners.items() for name, row in held.values.items()).map(
                lambda _: Tree(roots=tree.roots if compound else tuple(owners), owners=owners)
            )
        except (msgspec.ValidationError, TypeError) as error:
            return Error((ImagingFileFault(access=Access.READ, request=repr(tree), error=error),))
        except ReferenceError as error:
            return Error((BlenderEditFault(request=repr(tree), error=error),))

    @staticmethod
    def reverted(before: Persisted, after: Persisted) -> BlenderResult[None]:
        """Decodes stored trees and restores values still at after state."""
        return paired(Snapshot.decoded(before), Snapshot.decoded(after)).bind(lambda pair: Snapshot.restore(*pair).outcome)

    @staticmethod
    def restore(before: Tree[NativeValue], after: Tree[NativeValue]) -> Applied:
        """Restores live values still at after state."""

        def located(target: NativeOwner) -> Option[Native]:
            return struct_at(*target) if isinstance(target, tuple) else Some(target)

        def resolved(target: NativeOwner, name: str) -> tuple[tuple[Native, Descriptor], ...]:
            return tuple((struct, descriptor) for struct in located(target).to_list() for descriptor in Descriptor.resolve(struct, name, bpy.types.Property).to_option().to_list())

        def absent(key: str, name: str, target: NativeOwner) -> tuple[Applied, ...]:
            try:
                return tuple(Applied(outcome=Error((BlenderAbsentFault(owner=repr(struct), key=descriptor.path),))) for struct, descriptor in resolved(target, name))
            except ReferenceError:
                return (Applied(skipped=(member_path(key, name),)),)

        def actual(key: str, name: str, target: NativeOwner) -> tuple[Result[NativeValue, Applied], ...]:
            try:
                return tuple(
                    native(struct, Some((descriptor,))).map(lambda tree: next(row for held in tree.owners.values() for row in held.values.values())).map_error(Applied.refused)
                    for struct, descriptor in resolved(target, name)
                )
            except ReferenceError:
                return (Error(Applied(skipped=(member_path(key, name),))),)

        def assigned(path: str, struct: Native, descriptor: Descriptor, value: "Value | bytes | bpy.types.bpy_struct[object] | Tree[NativeValue]") -> Applied:
            return (
                Snapshot._applied(struct, value, lambda found, row: Snapshot._value(found, row[1]))
                if isinstance(value, Tree)
                else Snapshot._reported(path, Descriptor.resolve(struct, descriptor.path, bpy.types.Property).bind(lambda found: found.write(struct, Snapshot._value(found, value))))
            )

        def refreshed(struct: bpy.types.CurveMapping | bpy.types.CurveProfile) -> tuple[Applied, ...]:
            return BlenderEditFault.attempted(repr(struct), struct.update).map(lambda _: ()).default_with(lambda faults: (Applied.refused(faults),))

        missing = tuple(
            report
            for key, held in before.owners.items()
            for name, (target, _) in held.values.items()
            if key not in after.owners or name not in after.owners[key].values
            for report in absent(key, name, target)
        )
        captures = {key: tuple((name, actual(key, name, target)) for name, (target, _) in held.values.items()) for key, held in after.owners.items()}
        current = msgspec.structs.replace(
            after,
            owners={
                key: msgspec.structs.replace(held, values={name: value for name, results in captures[key] for result in results for value in result.to_option().to_list()})
                for key, held in after.owners.items()
            },
        )
        ordered = sequenced(
            (edit.key, struct, descriptor, (f"{edit.key}.{name}", value))
            for edit in revert(current, transfer(before, after))
            for name, (target, value) in edit.after.items()
            for struct, descriptor in resolved(target, name)
        )
        writes = tuple((struct, assigned(path, struct, descriptor, value)) for _, struct, descriptor, (path, value) in ordered)
        updated = {struct.as_pointer(): struct for struct, _ in writes if isinstance(struct, bpy.types.CurveMapping | bpy.types.CurveProfile)}
        return Applied.join((
            *missing,
            *(fault for rows in captures.values() for _, results in rows for result in results for fault in result.swap().to_option().to_list()),
            *(report for _, report in writes),
            *(report for struct in updated.values() for report in refreshed(struct)),
        ))

    @staticmethod
    def _reported(path: str, answer: "BlenderResult[Option[Value | bpy.types.bpy_struct[object]]]") -> Applied:
        """Classifies write as written, unchanged, or refused."""
        return answer.map(lambda prior: Applied(written=(path,)) if prior.is_some() else Applied(unchanged=(path,))).default_with(Applied.refused)

    @staticmethod
    def _value[V](descriptor: Descriptor, value: V) -> float | tuple[float, ...] | V:
        """Decodes captured float bits as scalar or array per property."""
        match descriptor.prop, value:
            case bpy.types.FloatProperty() as prop, bytes(bits):
                values = np.frombuffer(bits, dtype=_FLOAT_STORAGE)
                return tuple(values.tolist()) if prop.is_array and descriptor.index.is_none() else float(values[0])
            case _:
                return value


class Endpoints(msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    """Stored before and after values of one operation, after unset until write finishes."""

    before: Persisted
    after: Persisted | None = None

    def completed(self, owner: object, key: str) -> BlenderResult[Persisted]:
        """Returns stored after values, absent fault before write finishes."""
        return Option.of_optional(self.after).to_result_with(lambda: (BlenderAbsentFault(owner=repr(owner), key=f"{key}.after"),))

    def decoded(self, owner: object, key: str) -> BlenderResult[tuple[Tree[NativeValue], Tree[NativeValue]]]:
        """Decodes before and after trees, accumulating faults of both."""
        return paired(Snapshot.decoded(self.before), self.completed(owner, key).bind(Snapshot.decoded))


class Scenes(Case, frozen=True, kw_only=True, tag=1):
    """Named or every non-runtime scene in file order."""

    owner_type: ClassVar[type[bpy.types.Scene]] = bpy.types.Scene
    names: tuple[str, ...] | None = None

    def owners(self, _context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene, ...]]:
        """Returns scenes, absent fault per missing name."""
        return (
            Option
            .of_optional(self.names)
            .map(lambda names: accumulated(Option.of_optional(bpy.data.scenes.get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.data.scenes), key=name),)) for name in names))
            .default_with(lambda: Ok(tuple(scene for scene in bpy.data.scenes if not scene.is_runtime_data)))
        )


class ViewLayers(Case, frozen=True, kw_only=True, tag=2):
    """View layers of selected scenes."""

    owner_type: ClassVar[type[bpy.types.ViewLayer]] = bpy.types.ViewLayer
    scenes: tuple[str, ...] | None = None

    def owners(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.ViewLayer, ...]]:
        """Returns view layers in scene and layer order."""
        return Scenes(names=self.scenes).owners(context).map(lambda owners: tuple(layer for scene in owners for layer in scene.view_layers))


class Selection(Case, frozen=True, kw_only=True, tag=3):
    """Objects selected in context view layer."""

    owner_type: ClassVar[type[bpy.types.Object]] = bpy.types.Object

    @staticmethod
    def owners(context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Object, ...]]:
        """Returns selected objects in view layer order."""
        return rooted(bpy.types.ViewLayer, context).map(lambda layer: tuple(layer.objects.selected))


class Objects(Case, frozen=True, kw_only=True, tag=4):
    """Objects of named collection and its descendants."""

    title = "Collection Objects"
    owner_type: ClassVar[type[bpy.types.Object]] = bpy.types.Object
    collection: str

    def owners(self, _context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Object, ...]]:
        """Returns collection objects."""
        return (
            Option
            .of_optional(bpy.data.collections.get(self.collection))
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.data.collections), key=self.collection),))
            .map(lambda collection: tuple(collection.all_objects))
        )


class _Walk[W](msgspec.Struct, frozen=True, kw_only=True):
    """Capture policy: value reader, native compounds keyed by session, the compound a nested walk expands, and the structs enclosing it."""

    read: Callable[[Descriptor, Native, frozenset[tuple[int, str]]], Option[W]]
    native: bool = False
    expanded: Option[Descriptor] = Nothing
    enclosing: frozenset[tuple[int, str]] = frozenset()


# --- [OPERATIONS] -----------------------------------------------------------------------


def native(owner: Native, selection: Option[tuple[Descriptor, ...]] = Nothing, prior: Option[Tree[NativeValue]] = Nothing) -> BlenderResult[Tree[NativeValue]]:
    """Captures live values for restoration, owners keyed by ID session, collections and curves held whole.

    Args:
        owner: Struct whose members the tree holds.
        selection: Members to capture, every saved member when Nothing.
        prior: Tree whose values the capture keeps, adding paths it lacks.

    Returns:
        Native tree, read fault for an undecodable value, edit fault for a stale owner.
    """
    return _attempted(owner, lambda: _walked(owner, owner, _Walk(read=_actual(owner), native=True), selection, prior))


def _attempted[T](owner: Native, call: Callable[[], T]) -> BlenderResult[T]:
    """Runs one walk or definition read, an undecodable value as read fault and a stale owner as edit fault."""
    try:
        return Ok(call())
    except msgspec.ValidationError as error:
        return Error((ImagingFileFault(access=Access.READ, request=repr(owner), error=error),))
    except ReferenceError as error:
        return Error((BlenderEditFault(request=repr(owner), error=error),))


def _actual(owner: Native) -> Callable[[Descriptor, Native, frozenset[tuple[int, str]]], Option[NativeValue]]:
    """Returns native reader under owner: compounds walked whole, pointers held, floats as big-endian float64 bytes."""

    def read(descriptor: Descriptor, struct: Native, enclosing: frozenset[tuple[int, str]]) -> Option[NativeValue]:
        prop, target = descriptor.prop, _reference(owner, struct, descriptor.owner)
        if isinstance(prop, bpy.types.CollectionProperty) or (_curve(prop) and prop.is_readonly):
            selected = Descriptor(root=type(struct), owner="", prop=prop)
            return Some((target, _walked(owner, struct, _Walk(read=read, native=True, expanded=Some(selected), enclosing=enclosing), Some((selected,)))))
        value = getattr(struct, prop.identifier) if isinstance(prop, bpy.types.PointerProperty) else descriptor.read(struct)
        return Some((target, np.asarray(value, dtype=_FLOAT_STORAGE).tobytes() if isinstance(prop, bpy.types.FloatProperty) else value))

    return read


def _walked[W](owner: Native, source: Native, walk: _Walk[W], chosen: Option[tuple[Descriptor, ...]], prior: Option[Tree[W]] = Nothing) -> Tree[W]:
    """Walks source into a value tree, the chosen members alone when given, extending prior.

    Each struct is walked once, and a struct enclosing a nested walk ends that walk's branch.

    Args:
        owner: Struct the capture started from, anchoring native references.
        source: Struct or collection this walk reads.
        walk: Capture policy.
        chosen: Members to capture, every saved member when Nothing.
        prior: Tree whose values the walk keeps.

    Returns:
        Tree keyed by path, or by session key and pointer for a native top-level walk.
    """
    owners: dict[str, Owner[W]] = prior.map(lambda tree: {key: msgspec.structs.replace(row, values=row.values.copy()) for key, row in tree.owners.items()}).default_with(dict)
    seen: set[tuple[int, str]] = set(walk.enclosing)
    sessioned, indexed = walk.native and walk.expanded.is_none(), walk.expanded.is_some()

    def keyed(struct: Native, path: str) -> str:
        if not sessioned:
            return path
        target = _reference(owner, struct, path)
        return _session_key(*target) if isinstance(target, tuple) else str(struct.as_pointer() if isinstance(struct, bpy.types.bpy_struct) else id(struct))

    def arrayed(prop: bpy.types.Property, names: Iterable[str]) -> TypeGuard[bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty]:
        return isinstance(prop, bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty) and prop.is_array and any(name.startswith(f"{prop.identifier}[") for name in names)

    def address(struct: Native) -> tuple[int, str]:
        return (struct.as_pointer() if isinstance(struct, bpy.types.bpy_struct) else id(struct), struct.bl_rna.identifier)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    def branched(struct: Native, key: str, prop: bpy.types.Property, inside: frozenset[tuple[int, str]]) -> tuple[str, ...]:
        path = member_path(key, prop.identifier)
        match prop:
            case bpy.types.PointerProperty(fixed_type=bpy.types.ID()) | bpy.types.CollectionProperty(fixed_type=bpy.types.ID()):
                return ()  # type: ignore[unreachable]
            case bpy.types.CollectionProperty() if path in owners:
                return owners[path].children
            case bpy.types.PointerProperty(is_readonly=True):
                return tuple(at for child in Option.of_optional(getattr(struct, prop.identifier)).filter(effective).to_list() for at in walked(child, path, inside).to_list())
            case bpy.types.CollectionProperty():
                items = tuple((index, item) for index, item in enumerate(getattr(struct, prop.identifier)) if item is not None)
                keys = (
                    Option
                    .of_optional(prop.fixed_type)
                    .bind(lambda definition: Option.of_optional(definition.name_property))
                    .map(lambda named: tuple(getattr(item, named.identifier) for _, item in items))
                    .filter(lambda found: not indexed and len(set(found)) == len(found))
                    .map(lambda found: tuple(f'["{bpy.utils.escape_identifier(name)}"]' for name in found))
                    .default_with(lambda: tuple(f"[{index}]" for index, _ in items))
                )
                return tuple(at for suffix, (_, item) in zip(keys, items) for at in walked(item, f"{path}{suffix}", inside).to_list())
            case _:
                return ()

    def grouped(path: str, prop: bpy.types.Property, keys: tuple[str, ...]) -> tuple[str, ...]:
        if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID):
            owners.setdefault(row := member_path(path, prop.identifier), Owner(values={}, children=keys))
            return (row,)
        return keys

    def walked(struct: Native, path: str, above: frozenset[tuple[int, str]]) -> Option[str]:
        if (identity := address(struct)) in seen:
            return Nothing
        seen.add(identity)
        inside, key = above | {identity}, keyed(struct, path)
        definition: bpy.types.Struct = struct.bl_rna  # type: ignore[assignment, union-attr]  # ty: ignore[invalid-assignment, unresolved-attribute]
        members = (
            *Descriptor.saved(struct),
            *(Descriptor(root=type(struct), owner="", prop=prop) for prop in Option.of_optional(definition.name_property).filter(lambda prop: indexed and not prop.is_readonly).to_list()),
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
        nested = tuple(at for prop, keys in branches for at in grouped(path, prop, keys))
        owners[key] = Owner(values=held.values | values, children=tuple(dict.fromkeys((*held.children, *nested))))
        for descriptor in members:
            if arrayed(descriptor.prop, held.values):
                for index in range(descriptor.prop.array_length):
                    picked(Descriptor(root=type(source), owner=path, prop=descriptor.prop, index=Some(index)), inside)
        return Some(key)

    def picked(descriptor: Descriptor, above: frozenset[tuple[int, str]] = frozenset()) -> tuple[str, ...]:
        def member(struct: Native) -> tuple[str, ...]:
            inside = above | {address(struct)}
            if sessioned and isinstance(source, bpy.types.bpy_struct) and isinstance(struct, bpy.types.bpy_struct):
                enclosing = (
                    Descriptor(root=type(source), owner=at.removeprefix(base).removeprefix("."), prop=found.prop)
                    for base, path in id_path(source).bind(lambda base: id_path(struct).map(lambda own: (base, member_path(own, descriptor.prop.identifier)))).to_list()
                    for ancestor in struct.rna_ancestors()
                    for at in id_path(ancestor).to_list()
                    for found in Descriptor.saved(ancestor)
                    if (isinstance(found.prop, bpy.types.CollectionProperty) and not isinstance(found.prop.fixed_type, bpy.types.ID)) or (_curve(found.prop) and found.prop.is_readonly)
                    for prefix in (member_path(at, found.prop.identifier),)
                    if (not base or beneath(prefix, base)) and beneath(path, prefix)
                )
                for found in Option.of_optional(next(enclosing, None)).to_list():
                    return picked(found, above)
            if walk.expanded != Some(descriptor) and _valued(descriptor.prop, native=walk.native):
                key, name = keyed(struct, descriptor.owner), descriptor.member
                if walk.native and any(beneath(key, member_path(at, held_name)) for at, held in owners.items() for held_name in held.values):
                    return ()
                if key in owners and (name in owners[key].values or descriptor.prop.identifier in owners[key].values):
                    return (key,)
                if descriptor.index.is_none() and key in owners and arrayed(descriptor.prop, owners[key].values):
                    return tuple(at for index in range(descriptor.prop.array_length) for at in picked(msgspec.structs.replace(descriptor, index=Some(index)), above))
                for value in walk.read(descriptor, struct, walk.enclosing | inside).to_list():
                    owners.setdefault(key, Owner(values={})).values[name] = value
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
        *chosen.map(lambda selected: tuple(key for descriptor in selected for key in picked(descriptor))).default_with(lambda: tuple(walked(source, "", frozenset()).to_list())),
    )
    nested = frozenset(child for held in owners.values() for child in held.children)
    return Tree(roots=tuple(dict.fromkeys(key for key in roots if key not in nested)), owners=owners)


def _reference(owner: Native, struct: Native, path: str) -> NativeOwner:
    """Returns struct as its ID and path, a collection through owner's ID path, else struct itself."""
    match struct, owner:
        case bpy.types.bpy_struct(id_data=bpy.types.ID() as block), _:
            return id_path(struct).map(lambda found: (block, found)).default_value(struct)
        case bpy.types.bpy_prop_collection(), bpy.types.bpy_struct(id_data=bpy.types.ID() as block):
            return id_path(owner).map(lambda found: (block, member_path(found, path))).default_value(struct)
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


def _session_key(block: bpy.types.ID, path: str) -> str:
    """Returns `<session_uid>:<path>` owner key of struct path in ID."""
    return f"{block.session_uid}:{path}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Applied", "Endpoints", "NativeValue", "Objects", "Persisted", "Scenes", "Scope", "Scoped", "Selection", "Snapshot", "ViewLayers", "native"]
