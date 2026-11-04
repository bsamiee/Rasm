"""Restoration: native value captures, operation before and after values stored by ID, decoded against current IDs, and restored where live values still match."""

# ruff: file-ignore[return-in-generator]

from collections import defaultdict, deque
from collections.abc import Callable, Generator
from functools import partial
from itertools import islice

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor, sequenced, Value, written
from blender.bpy.data.rna.paths import Native, rna, RnaPath, Struct, struct_at
from blender.bpy.data.rna.snapshots import Applied, applied, Capture, referenced
from blender.records.faults import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason
from blender.records.trees import Owner, revert, transfer, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type NativeOwner = tuple[Native, RnaPath]
type NativeValue = tuple[NativeOwner, Value | Struct | Tree[NativeValue, RnaPath, RnaPath]]
type NativeTree = Tree[NativeValue, RnaPath, RnaPath]
type _Persisted = Tree[tuple[tuple[object, RnaPath], object], RnaPath, RnaPath]

# --- [MODELS] ---------------------------------------------------------------------------


class BeforeAfter(msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    """Stored before and after values of one operation, after unset until write finishes."""

    before: _Persisted
    after: _Persisted | None = None

    def decoded(self, owner: object, key: str) -> Outcome[tuple[NativeTree, NativeTree]]:
        """Decodes before and after trees, accumulating faults of both, absent fault before write finishes."""
        return paired(decoded(self.before), Option.of_optional(self.after).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(owner), member=f"{key}.after")).bind(decoded))

    def restored(self) -> Outcome[None]:
        """Restores members still at their after values, every before value of an operation that never finished."""

        def live(before: NativeTree) -> Outcome[NativeTree]:
            return accumulated(
                native(root, Some(tuple(descriptor for name in values for descriptor in Descriptor.resolve(root, (*path, *name), bpy.types.Property).to_option().to_list())))
                for held in before.owners.values()
                for values in (held.values,)
                for (root, path), _ in islice(values.values(), 1)
            ).map(Tree[NativeValue, RnaPath, RnaPath].joined)

        return decoded(self.before).bind(lambda before: Option.of_optional(self.after).map(decoded).default_with(lambda: live(before)).bind(lambda after: restore(before, after).outcome))


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [CAPTURE]


def native(owner: Native, selection: Option[tuple[Descriptor, ...]] = Nothing, prior: Option[NativeTree] = Nothing) -> Outcome[NativeTree]:
    """Captures live values for restoration, owners keyed by ID session and path, collections and curves held whole.

    Args:
        owner: Struct whose members the tree holds.
        selection: Members to capture, every saved member when Nothing.
        prior: Tree whose values the capture keeps, adding paths it lacks.

    Returns:
        Native tree, refused fault for a removed struct.
    """
    return catch(Capture[NativeValue](owner, partial(_native_read, owner), selection, prior=prior, native=True, inactive=True).tree, exception=ReferenceError)().map_error(
        partial(Fault.of, Reason.REFUSED, repr(owner))
    )


def _native_read(owner: Native, descriptor: Descriptor, struct: Native, _enclosing: frozenset[tuple[int, str]], compound: Option[NativeTree]) -> Option[NativeValue]:
    """Reads one member for restoration under owner: compounds walked whole, pointers held, values as read."""
    prop, target = descriptor.prop, referenced(owner, struct, descriptor.owner)
    match compound:
        case Option(tag="some", some=tree):
            return Some((target, tree))
        case _:
            value: Value | Struct = getattr(struct, prop.identifier) if isinstance(prop, bpy.types.PointerProperty) else descriptor.read(struct)  # ty: ignore[unsound-assignment]
            return Some((target, value))


# --- [STORAGE]


def _fold[K, V](steps: Generator[K, Outcome[V], Outcome[V]], descend: Callable[[K], Generator[K, Outcome[V], Outcome[V]]]) -> Outcome[V]:
    """Returns a completed outcome after depth-first child folds."""
    pending = [steps]
    answer: Option[Outcome[V]] = Nothing
    while True:
        try:
            child = answer.map(pending[-1].send).default_with(lambda: next(pending[-1]))
        except StopIteration as finished:
            pending.pop()
            result: Outcome[V] = finished.value  # ty: ignore[unsound-assignment]
            if not pending:
                return result
            answer = Some(result)
        else:
            pending.append(descend(child))
            answer = Nothing


def persisted(tree: NativeTree) -> Outcome[_Persisted]:
    """Converts native restoration tree to ID-keyed stored form."""

    def member(row: NativeValue) -> Generator[NativeTree, Outcome[_Persisted], Outcome[tuple[tuple[object, RnaPath], object]]]:
        target, value = row
        if not isinstance(target[0], bpy.types.ID):
            return Error(Fault.of(Reason.OUTSIDE, repr(target[0]), type(target[0]).__name__, member=bpy.types.ID.__name__))
        if isinstance(value, Tree):
            return (yield value).map(lambda held: (target, held))  # ty: ignore[unsound-yield]
        if isinstance(value, bpy.types.bpy_struct) and not isinstance(value, bpy.types.ID):
            return Error(Fault.of(Reason.OUTSIDE, repr(target), type(value).__name__, member=bpy.types.ID.__name__))
        return Ok((target, value))

    def folded(source: NativeTree) -> Generator[NativeTree, Outcome[_Persisted], Outcome[_Persisted]]:
        owners: list[Outcome[tuple[RnaPath, Owner[tuple[tuple[object, RnaPath], object], RnaPath, RnaPath]]]] = []
        for key, held in source.rows:
            values: list[Outcome[tuple[RnaPath, tuple[tuple[object, RnaPath], object]]]] = []
            for name, value in held.rows:
                result = yield from member(value)
                values.append(result.map(lambda row: (name, row)))
            owners.append(accumulated(values).map(lambda rows: (key, Owner(rows=rows, children=held.children))))
        return accumulated(owners).map(lambda rows: Tree(roots=source.roots, rows=rows))

    return _fold(folded(tree), folded)


def decoded(tree: _Persisted, *, compound: bool = False) -> Outcome[NativeTree]:
    """Validates stored ID references and rebuilds keys from current IDs, members of removed IDs left out.

    Args:
        tree: Stored tree.
        compound: Whether tree holds one compound member's items, its keys kept and its members resolved by type.

    Returns:
        Native tree, outside fault for a stored value no member accepts.
    """

    def value_of(descriptor: Descriptor, value: object) -> Generator[_Persisted, Outcome[NativeTree], Outcome[Value | Struct | NativeTree]]:
        subject = rna(descriptor.path)
        try:
            match descriptor.prop:
                case _ if descriptor.compound:
                    source: _Persisted = msgspec.convert(value, type=_Persisted)  # ty: ignore[unsound-assignment]
                    return (yield source)
                case bpy.types.PointerProperty() if value is None or isinstance(value, bpy.types.ID):
                    return Ok(value)  # ty: ignore[unsound-return-statement]
                case bpy.types.PointerProperty():
                    return Error(Fault.of(Reason.OUTSIDE, subject, type(value).__name__, member=bpy.types.ID.__name__))
                case _:
                    held: Value = msgspec.convert(value, Value)  # ty: ignore[unsound-assignment]
                    return Ok(held)
        except msgspec.ValidationError as error:
            return Error(Fault.of(Reason.OUTSIDE, subject, error, member=Value.__name__))

    def member(
        key: RnaPath, name: RnaPath, row: tuple[tuple[object, RnaPath], object], *, compound: bool
    ) -> Generator[_Persisted, Outcome[NativeTree], Outcome[Option[tuple[RnaPath, RnaPath, NativeValue]]]]:
        (block, path), value = row
        match block:
            case None:
                return Ok(Nothing)
            case bpy.types.ID():
                found = (
                    Descriptor.resolve(type(block), (*path, *name), bpy.types.Property).to_option()
                    if compound
                    else struct_at(block, path).bind(lambda struct: Descriptor.resolve(struct, name, bpy.types.Property).to_option())
                )
            case _:
                return Error(Fault.of(Reason.OUTSIDE, rna(path), type(block).__name__, member=bpy.types.ID.__name__))
        for descriptor in found.to_list():
            result = yield from value_of(descriptor, value)
            return result.map(lambda held: Some((key if compound else (block.session_uid, *path), name, ((block, path), held))))  # ty: ignore[unsound-return-statement]
        return Ok(Nothing)

    def rebuilt(source: _Persisted, rows: tuple[tuple[RnaPath, RnaPath, NativeValue], ...], *, compound: bool) -> NativeTree:
        grouped: defaultdict[RnaPath, list[tuple[RnaPath, NativeValue]]] = defaultdict(list)
        for key, name, value in rows:
            grouped[key].append((name, value))
        owners = source.owners
        keys = tuple(owners) if compound else tuple(grouped)
        return Tree(
            roots=source.roots if compound else keys,
            rows=tuple((key, Owner[NativeValue, RnaPath, RnaPath](rows=tuple(grouped[key]), children=owners[key].children if compound else ())) for key in keys),
        )

    def folded(source: _Persisted, *, compound: bool) -> Generator[_Persisted, Outcome[NativeTree], Outcome[NativeTree]]:
        rows: list[Outcome[Option[tuple[RnaPath, RnaPath, NativeValue]]]] = []
        for key, held in source.rows:
            for name, row in held.rows:
                rows.append((yield from member(key, name, row, compound=compound)))
        return accumulated(rows).map(lambda found: rebuilt(source, tuple(row for each in found for row in each.to_list()), compound=compound))

    return _fold(folded(tree, compound=compound), partial(folded, compound=True))


# --- [RESTORE]


def equal(left: NativeValue, right: NativeValue) -> bool:
    """Compares native targets and ordered compound values without recursive tree equality."""
    pending = deque(((left, right),))
    while pending:
        (left_target, left_value), (right_target, right_value) = pending.popleft()
        if left_target != right_target:
            return False
        if left_value is right_value:
            continue
        match left_value, right_value:
            case Tree() as first, Tree() as second:
                rows = tuple(zip(first.rows, second.rows))
                if (
                    first.roots != second.roots
                    or len(first.rows) != len(second.rows)
                    or any(
                        (first_key, first_owner.children, tuple(name for name, _ in first_owner.rows)) != (second_key, second_owner.children, tuple(name for name, _ in second_owner.rows))
                        for (first_key, first_owner), (second_key, second_owner) in rows
                    )
                ):
                    return False
                pending.extend((before, after) for (_, first_owner), (_, second_owner) in rows for (_, before), (_, after) in zip(first_owner.rows, second_owner.rows))
            case _:
                if left_value != right_value:
                    return False
    return True


def restore(before: NativeTree, after: NativeTree) -> Applied:
    """Restores live values still at after state."""
    before_values = {key: held.values for key, held in before.owners.items()}
    after_values = {key: held.values for key, held in after.owners.items()}

    def resolved(target: NativeOwner, name: RnaPath) -> tuple[tuple[Native, Descriptor], ...]:
        return tuple(found for struct in struct_at(*target).to_list() for found in Descriptor.located(struct, name).to_option().to_list())

    def assigned(path: RnaPath, struct: Native, descriptor: Descriptor, value: Value | Struct | NativeTree) -> Applied:
        return applied(struct, value, lambda _, row: row[1]) if isinstance(value, Tree) else Applied.reported(path, written(struct, descriptor.path, value))

    missing = tuple(
        Applied(outcome=Error(Fault.of(Reason.ABSENT, repr(struct), member=rna(descriptor.path))))
        for key, values in before_values.items()
        for name, (target, _) in values.items()
        if key not in after_values or name not in after_values[key]
        for struct, descriptor in resolved(target, name)
    )
    captures = {
        (key, name): native(root, Some((descriptor,))).map(lambda tree: Option.of_optional(next((row for held in tree.owners.values() for row in held.values.values()), None)))
        for key, values in after_values.items()
        for name, ((root, path), _) in values.items()
        for descriptor in Descriptor.resolve(root, (*path, *name), bpy.types.Property).to_option().to_list()
    }
    current = msgspec.structs.replace(
        after,
        rows=tuple(
            (key, msgspec.structs.replace(held, rows=tuple((name, value) for name in values if (key, name) in captures for value in captures[key, name].default_value(Nothing).to_list())))
            for key, held in after.rows
            for values in (held.values,)
        ),
    )
    ordered = sequenced(
        (edit.key, struct, descriptor, ((*edit.key, *name), value))
        for edit in revert(current, transfer(before, after, equal=equal), equal=equal)
        for name, (target, value) in edit.after.items()
        for struct, descriptor in resolved(target, name)
    )
    writes = tuple((struct, assigned(path, struct, descriptor, value)) for _, struct, descriptor, (path, value) in ordered)
    for curve in {struct.as_pointer(): struct for struct, _ in writes if isinstance(struct, bpy.types.CurveMapping | bpy.types.CurveProfile)}.values():
        curve.update()
    return Applied.join((
        *missing,
        *(Applied(outcome=result.map(lambda _: None), skipped=((*key, *name),) if result.map(Option.is_none).default_value(value=False) else ()) for (key, name), result in captures.items()),
        *(report for _, report in writes),
    ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BeforeAfter", "NativeOwner", "NativeTree", "NativeValue", "decoded", "equal", "native", "persisted", "restore"]
