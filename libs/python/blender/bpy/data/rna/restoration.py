"""Restoration: native value captures, operation before and after values stored by ID, decoded against current IDs, and restored where live values still match."""

from functools import partial
from itertools import islice

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor, sequenced, Value, written
from blender.bpy.data.rna.paths import Native, rna, RnaPath, Struct, struct_at
from blender.bpy.data.rna.snapshots import Applied, applied, Capture, referenced
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
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
                native(root, Some(tuple(descriptor for name in held.values for descriptor in Descriptor.resolve(root, (*path, *name), bpy.types.Property).to_option().to_list())))
                for held in before.owners.values()
                for (root, path), _ in islice(held.values.values(), 1)
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
        Native tree, outside fault for an undecodable value.
    """
    return catch(Capture.tree, exception=msgspec.ValidationError)(Capture(owner, partial(_native_read, owner), selection, prior=prior, native=True)).map_error(
        partial(Fault.of, Reason.OUTSIDE, repr(owner), member=Value.__name__)
    )


def _native_read(owner: Native, descriptor: Descriptor, struct: Native, enclosing: frozenset[tuple[int, str]]) -> Option[NativeValue]:
    """Reads one member for restoration under owner: compounds walked whole, pointers held, values as read."""
    prop, target = descriptor.prop, referenced(owner, struct, descriptor.owner)
    if descriptor.compound:
        return Some((target, Capture(owner, partial(_native_read, owner), nested=Some((struct, Descriptor(root=type(struct), prop=prop), enclosing))).tree()))
    return Some((target, getattr(struct, prop.identifier) if isinstance(prop, bpy.types.PointerProperty) else descriptor.read(struct)))


# --- [STORAGE]


def persisted(tree: NativeTree) -> Outcome[_Persisted]:
    """Converts native restoration tree to ID-keyed stored form."""

    def member(row: NativeValue) -> Outcome[tuple[tuple[object, RnaPath], object]]:
        target, value = row
        if not isinstance(target[0], bpy.types.ID):
            return Error(Fault.of(Reason.OUTSIDE, repr(target[0]), type(target[0]).__name__, member=bpy.types.ID.__name__))
        if isinstance(value, Tree):
            return persisted(value).map(lambda held: (target, held))
        if isinstance(value, bpy.types.bpy_struct) and not isinstance(value, bpy.types.ID):
            return Error(Fault.of(Reason.OUTSIDE, repr(target), type(value).__name__, member=bpy.types.ID.__name__))
        return Ok((target, value))

    return accumulated(
        accumulated(member(value).map(lambda row: (name, row)) for name, value in held.rows).map(lambda rows: (key, Owner(rows=rows, children=held.children))) for key, held in tree.rows
    ).map(lambda rows: Tree(roots=tree.roots, rows=rows))


def decoded(tree: _Persisted, *, compound: bool = False) -> Outcome[NativeTree]:
    """Validates stored ID references and rebuilds keys from current IDs, members of removed IDs left out.

    Args:
        tree: Stored tree.
        compound: Whether tree holds one compound member's items, its keys kept and its members resolved by type.

    Returns:
        Native tree, outside fault for a stored value no member accepts.
    """

    def value_of(descriptor: Descriptor, value: object) -> Outcome[Value | Struct | NativeTree]:
        subject = rna(descriptor.path)
        match descriptor.prop:
            case _ if descriptor.compound:
                nested: Outcome[_Persisted] = catch(msgspec.convert, exception=msgspec.ValidationError)(value, type=_Persisted).map_error(
                    partial(Fault.of, Reason.OUTSIDE, subject, member=Value.__name__)
                )
                return nested.bind(partial(decoded, compound=True))
            case bpy.types.PointerProperty() if value is None or isinstance(value, bpy.types.ID):
                return Ok(value)
            case bpy.types.PointerProperty():
                return Error(Fault.of(Reason.OUTSIDE, subject, type(value).__name__, member=bpy.types.ID.__name__))
            case _:
                held: Outcome[Value] = catch(msgspec.convert, exception=msgspec.ValidationError)(value, Value).map_error(partial(Fault.of, Reason.OUTSIDE, subject, member=Value.__name__))
                return held

    def member(key: RnaPath, name: RnaPath, row: tuple[tuple[object, RnaPath], object]) -> Outcome[Option[tuple[RnaPath, RnaPath, NativeValue]]]:
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
                return found.map(lambda descriptor: value_of(descriptor, value).map(lambda held: Some((key if compound else (block.session_uid, *path), name, ((block, path), held))))).default_value(
                    Ok(Nothing)
                )
            case _:
                return Error(Fault.of(Reason.OUTSIDE, rna(path), type(block).__name__, member=bpy.types.ID.__name__))

    def rebuilt(rows: tuple[tuple[RnaPath, RnaPath, NativeValue], ...]) -> NativeTree:
        keys = tuple(tree.owners) if compound else tuple(dict.fromkeys(at for at, _, _ in rows))
        return Tree(
            roots=tree.roots if compound else keys,
            rows=tuple(
                (key, Owner[NativeValue, RnaPath, RnaPath](rows=tuple((name, value) for at, name, value in rows if at == key), children=tree.owners[key].children if compound else ())) for key in keys
            ),
        )

    return accumulated(member(key, name, row) for key, held in tree.rows for name, row in held.rows).map(lambda found: rebuilt(tuple(row for each in found for row in each.to_list())))


# --- [RESTORE]


def restore(before: NativeTree, after: NativeTree) -> Applied:
    """Restores live values still at after state."""

    def resolved(target: NativeOwner, name: RnaPath) -> tuple[tuple[Native, Descriptor], ...]:
        return tuple(found for struct in struct_at(*target).to_list() for found in Descriptor.located(struct, name).to_option().to_list())

    def assigned(path: RnaPath, struct: Native, descriptor: Descriptor, value: Value | Struct | NativeTree) -> Applied:
        return applied(struct, value, lambda _, row: row[1]) if isinstance(value, Tree) else Applied.reported(path, written(struct, descriptor.path, value))

    missing = tuple(
        Applied(outcome=Error(Fault.of(Reason.ABSENT, repr(struct), member=rna(descriptor.path))))
        for key, held in before.owners.items()
        for name, (target, _) in held.values.items()
        if key not in after.owners or name not in after.owners[key].values
        for struct, descriptor in resolved(target, name)
    )
    captures = {
        (key, name): native(root, Some((descriptor,))).map(lambda tree: Block.of_seq(row for held in tree.owners.values() for row in held.values.values()).try_head())
        for key, held in after.owners.items()
        for name, ((root, path), _) in held.values.items()
        for descriptor in Descriptor.resolve(root, (*path, *name), bpy.types.Property).to_option().to_list()
    }
    current = msgspec.structs.replace(
        after,
        rows=tuple(
            (key, msgspec.structs.replace(held, rows=tuple((name, value) for name in held.values if (key, name) in captures for value in captures[key, name].default_value(Nothing).to_list())))
            for key, held in after.rows
        ),
    )
    ordered = sequenced(
        (edit.key, struct, descriptor, ((*edit.key, *name), value))
        for edit in revert(current, transfer(before, after))
        for name, (target, value) in edit.after.items()
        for struct, descriptor in resolved(target, name)
    )
    writes = tuple((struct, assigned(path, struct, descriptor, value)) for _, struct, descriptor, (path, value) in ordered)
    for curve in {struct.as_pointer(): struct for struct, _ in writes if isinstance(struct, bpy.types.CurveMapping | bpy.types.CurveProfile)}.values():
        curve.update()
    return Applied.join((
        *missing,
        *(Applied(outcome=result.map(lambda _: None), skipped=((*key, *name),) if result.map(Option.is_none).default_value(False) else ()) for (key, name), result in captures.items()),
        *(report for _, report in writes),
    ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BeforeAfter", "NativeOwner", "NativeTree", "NativeValue", "decoded", "native", "persisted", "restore"]
