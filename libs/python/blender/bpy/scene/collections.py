"""Collection membership, placement, instancing, and native data-block lifetimes."""

from collections.abc import Iterable
from contextlib import ExitStack
from functools import reduce
from itertools import chain, starmap

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
from idprop.types import IDPropertyGroup
import msgspec

from blender.bpy.data.ids.store import Handle, named, resolved
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderResult
from blender.imaging.faults import accumulated

# --- [TYPES] ----------------------------------------------------------------------------

type Member = bpy.types.Object | bpy.types.Collection
type MembershipChange = Joined | Parted | Sorted
type LayerPath = tuple[bpy.types.LayerCollection, tuple[bpy.types.LayerCollection, ...]]

# --- [MODELS] ---------------------------------------------------------------------------


class Joined(msgspec.Struct, frozen=True, kw_only=True):
    """Member linked into a collection."""

    member: Member


class Parted(msgspec.Struct, frozen=True, kw_only=True):
    """Member unlinked from a collection."""

    member: Member


class Sorted(msgspec.Struct, frozen=True, kw_only=True):
    """Member entry assigned a position in the custom Outliner order."""

    member: Member
    index: int


class CollectionTree(msgspec.Struct, frozen=True, kw_only=True):
    """Desired membership, order, and flags; absent concerns remain unchanged."""

    members: Option[tuple[Handle[Member], ...]] = Nothing
    kinds: frozenset[type[bpy.types.Object] | type[bpy.types.Collection]] = frozenset((bpy.types.Object, bpy.types.Collection))
    values: Option[Snapshot] = Nothing


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [MEMBERSHIP]


def entry(collection: bpy.types.Collection, member: Member) -> BlenderResult[bpy.types.CollectionObject | bpy.types.CollectionChild]:
    """Returns a member's native entry, or absence when it is outside the collection."""

    def found[E, M](entries: Iterable[E], items: Iterable[M]) -> BlenderResult[E]:
        return Option.of_optional(next((held for held, item in zip(entries, items) if item == member), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(collection), key=member.name),))

    match member:
        case bpy.types.Object():
            return found(collection.collection_objects, collection.objects)
        case bpy.types.Collection():
            return found(collection.collection_children, collection.children)


def parents(collection: bpy.types.Collection) -> tuple[bpy.types.Collection, ...]:
    """Returns direct parents in native data order, including embedded scene masters."""
    users = bpy.data.user_map(subset=(collection,))[collection]
    return tuple(
        holder for holder in (*(held for held in bpy.data.collections if held in users), *(scene.collection for scene in bpy.data.scenes if scene in users)) if collection in holder.children.values()
    )


def membership(tree: CollectionTree, collection: bpy.types.Collection) -> BlenderResult[tuple[MembershipChange, ...]]:
    """Returns ordered membership changes, accumulating every expired member handle."""

    def listed(wanted: tuple[Handle[Member], ...]) -> BlenderResult[tuple[MembershipChange, ...]]:
        handles = tuple(dict.fromkeys(wanted))
        held = dict(zip((*collection.objects, *collection.children), (item.sort_index for item in (*collection.collection_objects, *collection.collection_children))))
        return resolved(handles).map(
            lambda items: (
                *(Parted(member=item) for item in held if type(item) in tree.kinds and item not in items),
                *(Joined(member=item) for item in items if item not in held),
                *(Sorted(member=item, index=index) for index, item in enumerate(items) if held.get(item) != index),
            )
        )

    return tree.members.map(listed).default_with(lambda: Ok(()))


def applied(collection: bpy.types.Collection, change: MembershipChange) -> BlenderResult[None]:
    """Applies one change, returning a native membership or write refusal."""

    def linked(item: Member, row: Joined | Parted) -> None:
        match item:
            case bpy.types.Object():
                match row:
                    case Joined():
                        collection.objects.link(item)
                    case Parted():
                        collection.objects.unlink(item)
            case bpy.types.Collection():
                match row:
                    case Joined():
                        collection.children.link(item)
                    case Parted():
                        collection.children.unlink(item)

    match change:
        case Joined(member=member) | Parted(member=member):
            return catch(lambda: linked(member, change), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(collection), error=error),))
        case Sorted(member=member, index=index):
            return entry(collection, member).bind(lambda held: written(held, "sort_index", index)).map(lambda _: None)


def reconciled(tree: CollectionTree, collection: bpy.types.Collection) -> BlenderResult[tuple[MembershipChange, ...]]:
    """Reconciles dependent membership changes and then flags, retaining native faults."""
    initial: BlenderResult[None] = Ok(None)
    return membership(tree, collection).bind(
        lambda rows: reduce(lambda done, row: done.bind(lambda _: applied(collection, row)), rows, initial).bind(
            lambda _: tree.values.map(lambda values: values.apply(collection).outcome.map(lambda _: rows)).default_with(lambda: Ok(rows))
        )
    )


def joined(collection: bpy.types.Collection, members: tuple[Member, ...]) -> BlenderResult[tuple[MembershipChange, ...]]:
    """Links absent members once, accumulating native refusals in input order."""
    held = frozenset((*collection.objects, *collection.children))
    rows = tuple(Joined(member=item) for item in dict.fromkeys(members) if item not in held)
    return accumulated(applied(collection, row) for row in rows).map(lambda _: rows)


def parted(collection: bpy.types.Collection, members: tuple[Member, ...]) -> BlenderResult[tuple[MembershipChange, ...]]:
    """Unlinks present members once, accumulating native refusals in input order."""
    held = frozenset((*collection.objects, *collection.children))
    rows = tuple(Parted(member=item) for item in dict.fromkeys(members) if item in held)
    return accumulated(applied(collection, row) for row in rows).map(lambda _: rows)


# --- [PLACEMENT]


def walked(root: bpy.types.LayerCollection) -> Block[LayerPath]:
    """Returns the layer tree in preorder, each node beside its root-first ancestors."""

    def expanded(frontier: tuple[LayerPath, ...]) -> Option[tuple[LayerPath, tuple[LayerPath, ...]]]:
        match frontier:
            case ((node, ancestors) as head, *rest):
                return Some((head, (*((child, (*ancestors, node)) for child in node.children), *rest)))
            case _:
                return Nothing

    seed: tuple[LayerPath, ...] = ((root, ()),)
    return Block.unfold(expanded, seed)


def landing(view_layer: bpy.types.ViewLayer) -> BlenderResult[bpy.types.Collection]:
    """Returns the active collection's first editable ancestor, or native placement absence."""
    layers: dict[bpy.types.Collection, bpy.types.LayerCollection] = {}
    for node, _ in walked(view_layer.layer_collection):
        layers.setdefault(node.collection, node)

    def climbed(frontier: tuple[bpy.types.Collection, ...]) -> Option[tuple[bpy.types.Collection, tuple[bpy.types.Collection, ...]]]:
        match frontier:
            case (held, *rest):
                return Some((held, (*(parents(held) if held in layers else ()), *rest)))
            case _:
                return Nothing

    seed: tuple[bpy.types.Collection, ...] = (view_layer.active_layer_collection.collection,)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    return (
        Block
        .unfold(climbed, seed)
        .filter(lambda held: held in layers and _editable(held) and not layers[held].exclude)
        .try_head()
        .to_result_with(lambda: (BlenderAbsentFault(owner=repr(view_layer), key="active_layer_collection"),))
    )


def moved(item: bpy.types.Object, target: bpy.types.Collection, scene: bpy.types.Scene) -> BlenderResult[tuple[MembershipChange, ...]]:
    """Joins before leaving other editable scene collections, preserving the object's base."""
    held = tuple(collection for collection in (scene.collection, *scene.collection.children_recursive) if collection != target and item in collection.objects.values() and _editable(collection))
    return joined(target, (item,)).bind(lambda first: accumulated(parted(collection, (item,)) for collection in held).map(lambda rest: (*first, *chain.from_iterable(rest))))


def instanced(collection: bpy.types.Collection, target: bpy.types.Collection, name: str) -> BlenderResult[bpy.types.Object]:
    """Creates and links a named empty instance, removing it on naming, write, or link refusal."""
    with ExitStack() as rollback:
        item = bpy.data.objects.new(name, None)
        rollback.callback(bpy.data.batch_remove, ids=(item,))
        result = named(item, name).bind(
            lambda made: accumulated((written(made, "instance_type", "COLLECTION"), written(made, "instance_collection", collection))).bind(lambda _: joined(target, (made,))).map(lambda _: made)
        )
        if result.is_ok():
            rollback.pop_all()
        return result


def _editable(collection: bpy.types.Collection) -> bool:
    return collection.is_editable and collection.override_library is None and collection.importer is None


# --- [LIFETIME]


def founded(parent: Option[bpy.types.Collection], name: str) -> BlenderResult[bpy.types.Collection]:
    """Creates a named collection, removing it on a name collision or refused parent link."""
    with ExitStack() as rollback:
        made = bpy.data.collections.new(name)
        rollback.callback(bpy.data.batch_remove, ids=(made,))
        if (result := (named(made, name).bind(lambda block: parent.map(lambda held: joined(held, (block,)).map(lambda _: block)).default_with(lambda: Ok(block))))).is_ok():
            rollback.pop_all()
        return result


def cloned(collection: bpy.types.Collection, parent: Option[bpy.types.Collection]) -> BlenderResult[bpy.types.Collection]:
    """Returns a shallow native copy, or copy absence or a parent-link refusal."""
    with ExitStack() as rollback:
        copied = Option.of_optional(collection.copy())
        for made in copied.to_list():
            rollback.callback(bpy.data.batch_remove, ids=(made,))
        result = copied.map(lambda made: parent.map(lambda held: joined(held, (made,)).map(lambda _: made)).default_with(lambda: Ok(made))).default_with(
            lambda: Error((BlenderAbsentFault(owner=repr(collection), key="copy"),))
        )
        if result.is_ok():
            rollback.pop_all()
        return result


def duplicated(collection: bpy.types.Collection, parent: bpy.types.Collection, copies: frozenset[type[bpy.types.ID]]) -> BlenderResult[bpy.types.Collection]:
    """Duplicates a local hierarchy and selected object data, rolling back every acquired ID."""

    def expanded(frontier: tuple[bpy.types.Collection, ...]) -> Option[tuple[bpy.types.Collection, tuple[bpy.types.Collection, ...]]]:
        match frontier:
            case (held, *rest):
                return Some((held, (*(child for child in held.children if child.library is None), *rest)))
            case _:
                return Nothing

    seed: tuple[bpy.types.Collection, ...] = (collection,)
    tree = tuple(dict.fromkeys(Block.unfold(expanded, seed)))
    with ExitStack() as rollback:

        def copied[T: bpy.types.ID](blocks: Iterable[T]) -> BlenderResult[dict[T, T]]:
            def made(block: T) -> BlenderResult[tuple[T, T]]:
                copy = Option.of_optional(block.copy())
                for held in copy.to_list():
                    rollback.callback(bpy.data.batch_remove, ids=(held,))
                return copy.to_result_with(lambda: (BlenderAbsentFault(owner=repr(block), key="copy"),)).map(lambda held: (block, held))

            return accumulated(map(made, blocks)).map(dict)

        def finished(
            collections: dict[bpy.types.Collection, bpy.types.Collection], objects: dict[bpy.types.Object, bpy.types.Object], data: dict[bpy.types.ID, bpy.types.ID]
        ) -> BlenderResult[bpy.types.Collection]:
            mapping: dict[bpy.types.ID, bpy.types.ID] = dict(chain(collections.items(), objects.items(), data.items()))
            seen: set[tuple[int, type[object]]] = set()
            owned: set[bpy.types.ID] = set(mapping.values())

            def custom(group: IDPropertyGroup) -> BlenderResult[None]:
                def changed(name: str, value: object) -> BlenderResult[None]:
                    match value:
                        case bpy.types.ID() if value in mapping:
                            try:
                                group.update({name: mapping[value]})
                                return Ok(None)
                            except ReferenceError as error:
                                return Error((BlenderEditFault(request=f"{group!r}[{name!r}]", error=error),))
                        case IDPropertyGroup():
                            return custom(value)
                        case list():
                            return accumulated(custom(item) for item in value).map(lambda _: None)
                        case _:
                            return Ok(None)

                return accumulated(starmap(changed, group.items())).map(lambda _: None)

            def repointed(owner: "bpy.types.bpy_struct[object]") -> BlenderResult[None]:
                if (identity := ((owner.as_pointer(), type(owner)))) in seen:
                    return Ok(None)
                seen.add(identity)

                def member(prop: bpy.types.Property) -> BlenderResult[None]:
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

            def mirrored(original: bpy.types.Collection) -> BlenderResult[None]:
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

            def placed(_: tuple[None, ...]) -> BlenderResult[bpy.types.Collection]:
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

            inverse = Descriptor.resolve(bpy.types.Object, "matrix_parent_inverse", bpy.types.FloatProperty)
            return (
                accumulated(repointed(made) for made in mapping.values())
                .bind(lambda _: inverse.bind(lambda prop: accumulated(prop.write(made, prop.read(original)).map(lambda _: None) for original, made in objects.items())))
                .bind(lambda _: accumulated(mirrored(held) for held in tree))
                .bind(placed)
            )

        result = copied(tree).bind(
            lambda collections: copied(dict.fromkeys(item for held in tree for item in held.objects)).bind(
                lambda objects: copied(dict.fromkeys(block for item in objects for block in Option.of_optional(item.data).to_list() if type(block) in copies)).bind(
                    lambda data: finished(collections, objects, data)
                )
            )
        )
        if result.is_ok():
            rollback.pop_all()
        return result


def grouped(item: bpy.types.Object, scene: bpy.types.Scene, name: str) -> BlenderResult[bpy.types.Collection]:
    """Creates a group beside an object's scene memberships, rolling it back on refusal."""
    holders = tuple(held for held in (scene.collection, *scene.collection.children_recursive) if item in held.objects.values() and _editable(held)) or (scene.collection,)
    with ExitStack() as rollback:
        made = bpy.data.collections.new(name)
        rollback.callback(bpy.data.batch_remove, ids=(made,))
        if (result := (named(made, name).bind(lambda block: joined(block, (item,))).bind(lambda _: accumulated(joined(held, (made,)) for held in holders)).map(lambda _: made))).is_ok():
            rollback.pop_all()
        return result


def ungrouped(collection: bpy.types.Collection) -> BlenderResult[int]:
    """Reparents members before removing a collection, or returns every refused parent link."""
    members = (*collection.objects, *collection.children)

    return accumulated(
        joined(parent, tuple(member for member in members if not (isinstance(member, bpy.types.Collection) and parent in {member, *member.children_recursive}))) for parent in parents(collection)
    ).bind(
        lambda _: (
            catch(lambda: bpy.data.collections.remove(collection, do_unlink=True), exception=RuntimeError)()
            .map_error(lambda error: (BlenderEditFault(request=repr(collection), error=error),))
            .map(lambda _: 1)
        )
    )


def deleted(collection: bpy.types.Collection) -> BlenderResult[int]:
    """Removes a hierarchy and newly orphaned objects, returning the count or native refusal."""
    tree, items = tuple(dict.fromkeys((collection, *collection.children_recursive))), tuple(collection.all_objects)

    def removed(held: bpy.types.Collection) -> BlenderResult[None]:
        return catch(lambda: bpy.data.collections.remove(held, do_unlink=True), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(held), error=error),))

    return (
        removed(collection)
        .bind(lambda _: accumulated(removed(held) for held in tree[1:]))
        .bind(
            lambda _: accumulated(
                catch(lambda: bpy.data.objects.remove(item, do_unlink=True), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(item), error=error),))
                for item in items
                if item.users == 0
            ).map(lambda removed: len(tree) + len(removed))
        )
    )


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
