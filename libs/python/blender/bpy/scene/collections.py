"""Collection membership, placement, instancing, and lifetimes."""

from collections.abc import Iterable
from contextlib import ExitStack
from functools import reduce
from itertools import chain, starmap
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
from idprop.types import IDPropertyGroup
import msgspec

from blender.bpy.data.ids.ownership import committed, kept, released
from blender.bpy.data.ids.store import Handle, resolved
from blender.bpy.data.rna.descriptors import Descriptor, Struct, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome
from blender.records.trees import preorder

# --- [TYPES] ----------------------------------------------------------------------------

type Member = bpy.types.Object | bpy.types.Collection
type MembershipChange = Joined | Parted | Sorted
type LayerPath = tuple[bpy.types.LayerCollection, tuple[bpy.types.LayerCollection, ...]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_INVERSE: Final = Descriptor.declared(bpy.types.Object, "matrix_parent_inverse", bpy.types.FloatProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Joined(msgspec.Struct, frozen=True, kw_only=True):
    """Member link."""

    member: Member


class Parted(msgspec.Struct, frozen=True, kw_only=True):
    """Member unlink."""

    member: Member


class Sorted(msgspec.Struct, frozen=True, kw_only=True):
    """Member moved to index in custom Outliner order."""

    member: Member
    index: int


class CollectionTree(msgspec.Struct, frozen=True, kw_only=True):
    """Desired collection state, absent fields unchanged."""

    members: Option[tuple[Handle[Member], ...]] = Nothing
    kinds: frozenset[type[bpy.types.Object] | type[bpy.types.Collection]] = frozenset((bpy.types.Object, bpy.types.Collection))
    values: Option[Snapshot] = Nothing


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [WALKS]


def walked(root: bpy.types.LayerCollection) -> Block[LayerPath]:
    """Returns layer tree in preorder, each node with ancestors root first."""

    def children(path: LayerPath) -> tuple[LayerPath, ...]:
        node, ancestors = path
        return tuple((child, (*ancestors, node)) for child in node.children)

    return preorder((root, ()), children)


# --- [MEMBERSHIP]


def entry(collection: bpy.types.Collection, member: Member) -> Outcome[bpy.types.CollectionObject | bpy.types.CollectionChild]:
    """Returns member entry, failing when member is outside collection."""

    def found[E, M](entries: Iterable[E], items: Iterable[M]) -> Outcome[E]:
        return Option.of_optional(next((held for held, item in zip(entries, items) if item == member), None)).to_result_with(
            lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(collection), member=member.name),)
        )

    match member:
        case bpy.types.Object():
            return found(collection.collection_objects, collection.objects)
        case bpy.types.Collection():
            return found(collection.collection_children, collection.children)


def parents(collection: bpy.types.Collection) -> tuple[bpy.types.Collection, ...]:
    """Returns direct parents, scene collections included."""
    users = bpy.data.user_map(subset=(collection,))[collection]
    return tuple(
        holder for holder in (*(held for held in bpy.data.collections if held in users), *(scene.collection for scene in bpy.data.scenes if scene in users)) if collection in holder.children.values()
    )


def membership(tree: CollectionTree, collection: bpy.types.Collection) -> Outcome[tuple[MembershipChange, ...]]:
    """Returns ordered membership changes, accumulating every expired member handle."""

    def listed(wanted: tuple[Handle[Member], ...]) -> Outcome[tuple[MembershipChange, ...]]:
        held = dict(zip((*collection.objects, *collection.children), (item.sort_index for item in (*collection.collection_objects, *collection.collection_children))))
        return resolved(tuple(dict.fromkeys(wanted))).map(
            lambda items: (
                *(Parted(member=item) for item in held if type(item) in tree.kinds and item not in items),
                *(Joined(member=item) for item in items if item not in held),
                *(Sorted(member=item, index=index) for index, item in enumerate(items) if held.get(item) != index),
            )
        )

    return tree.members.map(listed).default_value(Ok(()))


def applied(collection: bpy.types.Collection, change: MembershipChange) -> Outcome[None]:
    """Applies membership change."""

    def linked(row: Joined | Parted) -> None:
        joining = isinstance(row, Joined)
        match row.member:
            case bpy.types.Object() as item:
                (collection.objects.link if joining else collection.objects.unlink)(item)
            case bpy.types.Collection() as item:
                (collection.children.link if joining else collection.children.unlink)(item)

    match change:
        case Joined() | Parted():
            return catch(linked, exception=RuntimeError)(change).map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(collection), cause=error),))
        case Sorted(member=member, index=index):
            return entry(collection, member).bind(lambda held: written(held, "sort_index", index)).map(lambda _: None)


def reconciled(tree: CollectionTree, collection: bpy.types.Collection) -> Outcome[tuple[MembershipChange, ...]]:
    """Applies membership changes, then values, stopping at first fault."""
    initial: Outcome[None] = Ok(None)
    return membership(tree, collection).bind(
        lambda rows: reduce(lambda done, row: done.bind(lambda _: applied(collection, row)), rows, initial).bind(
            lambda _: tree.values.map(lambda values: values.apply(collection).outcome.map(lambda _: rows)).default_value(Ok(rows))
        )
    )


def changed(collection: bpy.types.Collection, members: tuple[Member, ...], kind: type[Joined | Parted]) -> Outcome[tuple[MembershipChange, ...]]:
    """Links absent members or unlinks present ones once each, accumulating faults in input order."""
    held = frozenset((*collection.objects, *collection.children))
    rows = tuple(kind(member=item) for item in dict.fromkeys(members) if (item in held) is (kind is Parted))
    return accumulated(applied(collection, row) for row in rows).map(lambda _: rows)


def joined(collection: bpy.types.Collection, members: tuple[Member, ...]) -> Outcome[tuple[MembershipChange, ...]]:
    """Links absent members once, accumulating faults in input order."""
    return changed(collection, members, Joined)


def parted(collection: bpy.types.Collection, members: tuple[Member, ...]) -> Outcome[tuple[MembershipChange, ...]]:
    """Unlinks present members once, accumulating faults in input order."""
    return changed(collection, members, Parted)


# --- [PLACEMENT]


def landing(view_layer: bpy.types.ViewLayer) -> Outcome[bpy.types.Collection]:
    """Returns active collection or nearest editable included ancestor."""
    layers = {node.collection: node for node, _ in reversed(tuple(walked(view_layer.layer_collection)))}
    return (
        preorder(view_layer.active_layer_collection.collection, lambda held: parents(held) if held in layers else ())  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        .filter(lambda held: held in layers and _editable(held) and not layers[held].exclude)
        .try_head()
        .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(view_layer), member="active_layer_collection"),))
    )


def moved(item: bpy.types.Object, target: bpy.types.Collection, scene: bpy.types.Scene) -> Outcome[tuple[MembershipChange, ...]]:
    """Moves object to target from other editable scene collections, keeping its base."""
    held = tuple(collection for collection in (scene.collection, *scene.collection.children_recursive) if collection != target and item in collection.objects.values() and _editable(collection))
    return joined(target, (item,)).bind(lambda first: accumulated(parted(collection, (item,)) for collection in held).map(lambda rest: (*first, *chain.from_iterable(rest))))


def instanced(collection: bpy.types.Collection, target: bpy.types.Collection, name: str) -> Outcome[bpy.types.Object]:
    """Creates empty instancing collection in target under name, removing it on fault."""
    return kept(
        catch(bpy.data.objects.new, exception=RuntimeError)(name, None).map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=name, cause=error),)),
        lambda made: accumulated((written(made, "instance_type", "COLLECTION"), written(made, "instance_collection", collection))).bind(lambda _: joined(target, (made,))).map(lambda _: made),
    )


def _editable(collection: bpy.types.Collection) -> bool:
    return collection.is_editable and collection.override_library is None and collection.importer is None


# --- [LIFETIME]


def founded(parent: Option[bpy.types.Collection], name: str) -> Outcome[bpy.types.Collection]:
    """Creates collection under parent named name, removing it on fault."""
    return kept(
        catch(bpy.data.collections.new, exception=RuntimeError)(name).map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=name, cause=error),)), lambda made: _parented(made, parent)
    )


def cloned(collection: bpy.types.Collection, parent: Option[bpy.types.Collection]) -> Outcome[bpy.types.Collection]:
    """Returns shallow copy linked into parent, removing it on fault."""
    return kept(_copied(collection), lambda made: _parented(made, parent))


def duplicated(collection: bpy.types.Collection, parent: bpy.types.Collection, copies: frozenset[type[bpy.types.ID]]) -> Outcome[bpy.types.Collection]:
    """Duplicates local hierarchy with object data of `copies` types, removing every copy on fault."""
    tree = tuple(dict.fromkeys(preorder(collection, lambda held: (child for child in held.children if child.library is None))))
    with ExitStack() as rollback:

        def copied[T: bpy.types.ID](blocks: Iterable[T]) -> Outcome[dict[T, T]]:
            return accumulated(kept(_copied(block), Ok, Some(rollback)).map(lambda held: (block, held)) for block in blocks).map(dict)

        def finished(
            collections: dict[bpy.types.Collection, bpy.types.Collection], objects: dict[bpy.types.Object, bpy.types.Object], data: dict[bpy.types.ID, bpy.types.ID]
        ) -> Outcome[bpy.types.Collection]:
            mapping: dict[bpy.types.ID, bpy.types.ID] = dict(chain(collections.items(), objects.items(), data.items()))
            seen: set[tuple[int, type[object]]] = set()
            owned: set[bpy.types.ID] = set(mapping.values())

            def custom(group: IDPropertyGroup) -> Outcome[None]:
                def rewired(name: str, value: object) -> Outcome[None]:
                    match value:
                        case bpy.types.ID() if value in mapping:
                            return (
                                catch(group.update, exception=ReferenceError)({name: mapping[value]})
                                .map(lambda _: None)
                                .map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=f"{group!r}[{name!r}]", cause=error),))
                            )
                        case IDPropertyGroup():
                            return custom(value)
                        case list():
                            return accumulated(custom(item) for item in value).map(lambda _: None)
                        case _:
                            return Ok(None)

                return accumulated(starmap(rewired, group.items())).map(lambda _: None)

            def repointed(owner: Struct) -> Outcome[None]:
                if (identity := ((owner.as_pointer(), type(owner)))) in seen:
                    return Ok(None)
                seen.add(identity)

                def member(prop: bpy.types.Property) -> Outcome[None]:
                    match prop:
                        case bpy.types.PointerProperty():
                            match getattr(owner, prop.identifier):
                                case bpy.types.ID() as value if not prop.is_readonly and value in mapping:
                                    return Descriptor(root=type(owner), owner="", prop=prop).write(owner, mapping[value]).map(lambda _: None)
                                case bpy.types.ID(is_embedded_data=True) as value if prop.is_readonly:
                                    owned.add(value)
                                    return repointed(value)
                                case bpy.types.ID() | None:
                                    return Ok(None)
                                case bpy.types.bpy_struct(id_data=held) as value if held in owned:
                                    return repointed(value)
                                case _:
                                    return Ok(None)
                        case bpy.types.CollectionProperty():
                            return accumulated(repointed(item) for item in getattr(owner, prop.identifier) if item.id_data in owned).map(lambda _: None)
                        case _:
                            return Ok(None)

                return accumulated((
                    *(member(prop) for prop in owner.bl_rna.properties),
                    *(custom(owner.id_properties_ensure()) for _ in (owner,) if hasattr(owner, bpy.types.bpy_struct.__getitem__.__name__)),
                )).map(lambda _: None)

            def mirrored(original: bpy.types.Collection) -> Outcome[None]:
                made = collections[original]
                members = (*original.objects, *original.children)
                wanted: tuple[Member, ...] = (*(objects[item] for item in original.objects), *(collections.get(child, child) for child in original.children))
                return (
                    accumulated(Snapshot.capture(held) for held in (*original.collection_objects, *original.collection_children))
                    .bind(
                        lambda values: (
                            parted(made, members)
                            .bind(lambda _: joined(made, wanted))
                            .bind(lambda _: accumulated(entry(made, item).bind(lambda held: snapshot.apply(held).outcome) for item, snapshot in zip(wanted, values)))
                        )
                    )
                    .map(lambda _: None)
                )

            def placed(_: tuple[None, ...]) -> Outcome[bpy.types.Collection]:
                made = collections[collection]
                rows = tuple(zip((*parent.children, *parent.objects), (*parent.collection_children, *parent.collection_objects)))
                ordered = (*(row[0] for row in sorted((row for row in rows if row[1].sort_index >= 0), key=lambda row: row[1].sort_index)), *(item for item, held in rows if held.sort_index < 0))
                members = tuple(Handle.of(member) for item in ordered for member in ((item, made) if item == collection else (item,)))
                return (
                    entry(parent, collection)
                    .bind(Snapshot.capture)
                    .bind(
                        lambda values: (
                            joined(parent, (made,))
                            .bind(lambda _: entry(parent, made))
                            .bind(lambda held: values.apply(held).outcome)
                            .bind(lambda _: reconciled(CollectionTree(members=Some(members)), parent))
                        )
                    )
                    .map(lambda _: made)
                )

            return (
                accumulated(repointed(made) for made in mapping.values())
                .bind(lambda _: accumulated(_INVERSE.write(made, _INVERSE.read(original)).map(lambda _: None) for original, made in objects.items()))
                .bind(lambda _: accumulated(mirrored(held) for held in tree))
                .bind(placed)
            )

        return committed(
            copied(tree).bind(
                lambda collections: copied(dict.fromkeys(item for held in tree for item in held.objects)).bind(
                    lambda objects: copied(dict.fromkeys(block for item in objects for block in Option.of_optional(item.data).to_list() if type(block) in copies)).bind(
                        lambda data: finished(collections, objects, data)
                    )
                )
            ),
            rollback,
        )


def grouped(item: bpy.types.Object, scene: bpy.types.Scene, name: str) -> Outcome[bpy.types.Collection]:
    """Wraps object in collection under its editable holders or scene collection, removing it on fault."""
    holders = tuple(held for held in (scene.collection, *scene.collection.children_recursive) if item in held.objects.values() and _editable(held)) or (scene.collection,)
    return kept(founded(Nothing, name), lambda made: joined(made, (item,)).bind(lambda _: accumulated(joined(held, (made,)) for held in holders)).map(lambda _: made))


def ungrouped(collection: bpy.types.Collection) -> Outcome[int]:
    """Dissolves collection into its parents, returning removed count."""
    members = (*collection.objects, *collection.children)
    return accumulated(
        joined(parent, tuple(member for member in members if not (isinstance(member, bpy.types.Collection) and parent in {member, *member.children_recursive}))) for parent in parents(collection)
    ).bind(lambda _: released((collection,)))


def deleted(collection: bpy.types.Collection) -> Outcome[int]:
    """Removes hierarchy and objects it orphans, returning removed count."""
    tree, items = tuple(dict.fromkeys((collection, *collection.children_recursive))), tuple(collection.all_objects)
    return released(tree).bind(lambda count: released(tuple(item for item in items if item.users == 0)).map(lambda orphans: count + orphans))


def _parented(collection: bpy.types.Collection, parent: Option[bpy.types.Collection]) -> Outcome[bpy.types.Collection]:
    return parent.map(lambda held: joined(held, (collection,)).map(lambda _: collection)).default_value(Ok(collection))


def _copied[T: bpy.types.ID](block: T) -> Outcome[T]:
    return Option.of_optional(block.copy()).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(block), member="copy"),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CollectionTree",
    "Joined",
    "LayerPath",
    "Member",
    "MembershipChange",
    "Parted",
    "Sorted",
    "applied",
    "changed",
    "cloned",
    "deleted",
    "duplicated",
    "entry",
    "founded",
    "grouped",
    "instanced",
    "joined",
    "landing",
    "membership",
    "moved",
    "parents",
    "parted",
    "reconciled",
    "ungrouped",
    "walked",
]
