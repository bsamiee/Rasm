"""Collection membership, Outliner order, placement, instancing, and lifetimes."""

from collections.abc import Generator, Iterable
from contextlib import ExitStack
from functools import partial, reduce
from itertools import chain, takewhile

import bpy
from expression import Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
from idprop.types import IDPropertyGroup

from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.ownership import kept_block, released, retained
from rasm_bpy.data._rna.descriptors import Descriptor, written
from rasm_bpy.data._rna.paths import Struct
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_records import accumulated, Fault, Faults, Nothing, Ok, Outcome, reached, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type _Member = bpy.types.Object | bpy.types.Collection

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WALKS]


def walked(root: bpy.types.LayerCollection) -> Block[tuple[bpy.types.LayerCollection, tuple[bpy.types.LayerCollection, ...]]]:
    """Returns layer tree in preorder, each node with ancestors root first."""

    def children(path: tuple[bpy.types.LayerCollection, tuple[bpy.types.LayerCollection, ...]]) -> tuple[tuple[bpy.types.LayerCollection, tuple[bpy.types.LayerCollection, ...]], ...]:
        node, ancestors = path
        return tuple((child, (*ancestors, node)) for child in node.children)

    return reached(((root, ()),), children)


# --- [MEMBERSHIP]


def _entry(collection: bpy.types.Collection, member: _Member) -> Outcome[bpy.types.CollectionObject | bpy.types.CollectionChild]:
    """Returns member entry, failing when member is outside collection."""

    def found[E](pairs: Iterable[tuple[E, _Member]]) -> Outcome[E]:
        return Option.of_optional(next((entry for entry, item in pairs if item == member), None)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(collection), member=member.name))

    match member:
        case bpy.types.Object():
            return found(zip(collection.collection_objects, collection.objects))
        case bpy.types.Collection():
            return found(zip(collection.collection_children, collection.children))


def relinked(collection: bpy.types.Collection, members: tuple[_Member, ...], *, linked: bool) -> Outcome[tuple[_Member, ...]]:
    """Links absent members, or unlinks present ones, once each through the host collection each belongs to, accumulating faults in input order, returning the members changed."""
    held = frozenset((*collection.objects, *collection.children))
    changed = tuple(item for item in dict.fromkeys(members) if (item in held) is not linked)
    return accumulated(
        catch(entries.link if linked else entries.unlink, exception=RuntimeError)(item).map_error(partial(Fault.of, Refusal.REFUSED, repr(collection)))
        for item in changed
        for entries in (collection.objects if isinstance(item, bpy.types.Object) else collection.children,)
    ).map(lambda _: changed)


# --- [PLACEMENT]


def active_editable(view_layer: bpy.types.ViewLayer) -> Outcome[bpy.types.Collection]:
    """Returns active collection or nearest editable included ancestor."""
    active = view_layer.active_layer_collection
    path = next((*ancestors, node) for node, ancestors in walked(view_layer.layer_collection) if node == active)
    return Option.of_optional(next((layer.collection for layer in reversed(tuple(takewhile(lambda layer: not layer.exclude, path))) if _editable(layer.collection)), None)).to_result_with(
        lambda: Fault.of(Reason.ABSENT, repr(view_layer), member="active_layer_collection")
    )


def instanced(collection: bpy.types.Collection, target: bpy.types.Collection, name: str) -> Outcome[bpy.types.Object]:
    """Creates empty instancing collection in target under name, removing it on fault."""
    return kept_block(
        catch(bpy.data.objects.new, exception=RuntimeError)(name, None).map_error(partial(Fault.of, Refusal.REFUSED, name)),
        lambda made: (
            accumulated((written(made, ("instance_type",), "COLLECTION"), written(made, ("instance_collection",), collection)))
            .bind(lambda _: relinked(target, (made,), linked=True))
            .map(lambda _: made)
        ),
    )


def _editable(collection: bpy.types.Collection) -> bool:
    return collection.is_editable and collection.override_library is None and collection.importer is None


# --- [LIFECYCLE]


def created(parent: Option[bpy.types.Collection], name: str) -> Outcome[bpy.types.Collection]:
    """Creates collection under parent named name, removing it on fault."""
    return kept_block(catch(bpy.data.collections.new, exception=RuntimeError)(name).map_error(partial(Fault.of, Refusal.REFUSED, name)), lambda made: _parented(made, parent))


def cloned(collection: bpy.types.Collection, parent: Option[bpy.types.Collection]) -> Outcome[bpy.types.Collection]:
    """Returns shallow copy linked into parent, removing it on fault."""
    return kept_block(_copied(collection), lambda made: _parented(made, parent))


def duplicated(collection: bpy.types.Collection, parent: bpy.types.Collection, copies: frozenset[type[bpy.types.ID]]) -> Outcome[bpy.types.Collection]:
    """Duplicates local hierarchy with object data of `copies` types, removing every copy on fault."""
    tree = tuple(reached((collection,), lambda held: (child for child in held.children if child.library is None)))
    with ExitStack() as rollback:

        def copied[T: bpy.types.ID](blocks: Iterable[T]) -> Outcome[dict[T, T]]:
            return accumulated(kept_block(_copied(block), Ok, Some(rollback)).map(lambda held: (block, held)) for block in blocks).map(dict)

        def finished(
            collections: dict[bpy.types.Collection, bpy.types.Collection], objects: dict[bpy.types.Object, bpy.types.Object], data: dict[bpy.types.ID, bpy.types.ID]
        ) -> Outcome[bpy.types.Collection]:
            def copied_parts(pair: tuple[bpy.types.ID, bpy.types.ID]) -> tuple[tuple[bpy.types.ID, bpy.types.ID], ...]:
                original, made = pair
                return tuple(
                    (source, target)
                    for prop in original.bl_rna.properties
                    if isinstance(prop, bpy.types.PointerProperty) and prop.is_readonly
                    for source, target in ((getattr(original, prop.identifier), getattr(made, prop.identifier)),)
                    if isinstance(source, bpy.types.ID) and isinstance(target, bpy.types.ID) and source != target
                )

            mapping = dict(reached(chain(collections.items(), objects.items(), data.items()), copied_parts))
            owned = frozenset(mapping.values())

            def custom(group: Struct | IDPropertyGroup) -> Outcome[None]:
                completed: list[Outcome[Option[object]]] = []

                def remapped(value: object) -> Generator[object, Outcome[Option[object]]]:
                    match value:
                        case bpy.types.ID() if value in mapping:
                            completed.append(Ok(Some(mapping[value])))
                        case list():
                            answers: list[Outcome[Option[object]]] = []
                            for item in value:
                                answers.append((yield item))  # ruff: ignore[manual-list-comprehension]
                            completed.append(
                                accumulated(answers).map(
                                    lambda changes: Some([change.default_value(item) for item, change in zip(value, changes)]) if any(change.is_some() for change in changes) else Nothing
                                )
                            )
                        case _:
                            completed.append(Ok(Nothing))

                def members(held: Struct | IDPropertyGroup) -> Generator[object, Outcome[Option[object]]]:
                    answers: list[Outcome[None]] = []
                    for name, value in held.items():
                        changed = yield value
                        answers.append(
                            changed.bind(
                                lambda change: accumulated(
                                    catch((held.id_properties_ensure() if isinstance(held, bpy.types.bpy_struct) else held).update, exception=ReferenceError)({name: replacement}).map_error(
                                        partial(Fault.of, Refusal.REFUSED, f"{held!r}[{name!r}]")
                                    )
                                    for replacement in change.to_list()
                                ).map(lambda _: None)
                            )
                        )
                    completed.append(accumulated(answers).map(lambda _: Nothing))

                pending = [members(group)]
                answer: Option[Outcome[Option[object]]] = Nothing
                while True:
                    try:
                        child = answer.map(pending[-1].send).default_with(lambda: next(pending[-1]))
                    except StopIteration:
                        pending.pop()
                        result = completed.pop()
                        if not pending:
                            return result.map(lambda _: None)
                        answer = Some(result)
                    else:
                        pending.append(members(child) if isinstance(child, IDPropertyGroup) else remapped(child))
                        answer = Nothing

            writes: list[Outcome[None]] = []

            def followed(row: tuple[type[Struct], Struct]) -> Iterable[tuple[type[Struct], Struct]]:
                _, owner = row
                for prop in owner.bl_rna.properties:
                    match prop:
                        case bpy.types.PointerProperty():
                            match getattr(owner, prop.identifier):
                                case bpy.types.ID() as value if not prop.is_readonly and value in mapping:
                                    writes.append(Descriptor(root=type(owner), owner=(), prop=prop).write(owner, mapping[value]).map(lambda _: None))
                                case bpy.types.ID() as value if prop.is_readonly and value in owned:
                                    yield type(value), value
                                case bpy.types.ID() | None:
                                    pass
                                case bpy.types.bpy_struct(id_data=held) as value if held in owned:
                                    yield type(value), value
                        case bpy.types.CollectionProperty():
                            yield from ((type(item), item) for item in getattr(owner, prop.identifier) if item.id_data in owned)
                        case _:
                            pass
                if hasattr(owner, "__getitem__"):
                    writes.append(custom(owner))

            def rebuilt(original: bpy.types.Collection) -> Outcome[None]:
                made = collections[original]
                members = (*original.objects, *original.children)
                wanted: tuple[_Member, ...] = (*(objects[item] for item in original.objects), *(collections.get(child, child) for child in original.children))
                return (
                    accumulated(Snapshot.capture(held) for held in (*original.collection_objects, *original.collection_children))
                    .bind(
                        lambda values: (
                            relinked(made, members, linked=False)
                            .bind(lambda _: relinked(made, wanted, linked=True))
                            .bind(lambda _: accumulated(_entry(made, item).bind(lambda held: snapshot.apply(held).outcome) for item, snapshot in zip(wanted, values)))
                        )
                    )
                    .map(lambda _: None)
                )

            def placed(_: tuple[None, ...]) -> Outcome[bpy.types.Collection]:
                made = collections[collection]
                rows = tuple(zip((*parent.children, *parent.objects), (*parent.collection_children, *parent.collection_objects)))
                ordered = (*(row[0] for row in sorted((row for row in rows if row[1].sort_index >= 0), key=lambda row: row[1].sort_index)), *(item for item, held in rows if held.sort_index < 0))
                members = tuple(member for item in ordered for member in ((item, made) if item == collection else (item,)))
                return (
                    _entry(parent, collection)
                    .bind(Snapshot.capture)
                    .bind(lambda values: relinked(parent, (made,), linked=True).bind(lambda _: _entry(parent, made)).bind(lambda held: values.apply(held).outcome))
                    .map(
                        lambda _: dict[_Member, bpy.types.CollectionObject | bpy.types.CollectionChild](
                            chain(zip(parent.objects, parent.collection_objects), zip(parent.children, parent.collection_children))
                        )
                    )
                    .bind(
                        lambda entries: reduce(
                            Result[None, Faults].bind,
                            (
                                lambda _: written(entries[item], ("sort_index",), index).map(lambda _: None)
                                for index, item in tuple((index, item) for index, item in enumerate(members) if entries[item].sort_index != index)
                            ),
                            Result[None, Faults](ok=None),
                        )
                    )
                    .map(lambda _: made)
                )

            for original, made in objects.items():
                made.matrix_parent_inverse = original.matrix_parent_inverse.copy()
            reached(((type(made), made) for made in mapping.values()), followed)
            return accumulated(writes).bind(lambda _: accumulated(rebuilt(held) for held in tree)).bind(placed)

        return retained(
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
    """Adds object to a new collection under editable scene holders or scene root, preserving direct links and removing the new collection on fault."""
    linking = frozenset(item.users_collection)
    holders = tuple(held for held in (scene.collection, *scene.collection.children_recursive) if held in linking and _editable(held)) or (scene.collection,)
    return kept_block(
        created(Nothing, name), lambda made: relinked(made, (item,), linked=True).bind(lambda _: accumulated(relinked(held, (made,), linked=True) for held in holders)).map(lambda _: made)
    )


def ungrouped(collection: bpy.types.Collection) -> Outcome[int]:
    """Dissolves collection into its parents, returning removed count."""
    members, users = (*collection.objects, *collection.children), sorted(bpy.data.user_map(subset=(collection,), value_types={"COLLECTION", "SCENE"})[collection], key=lambda held: held.name_full)
    parents = (
        *(user for user in users if isinstance(user, bpy.types.Collection) and collection in user.children.values()),
        *(root for user in users if isinstance(user, bpy.types.Scene) and collection in (root := user.collection).children.values()),
    )
    return accumulated(relinked(parent, members, linked=True) for parent in parents).bind(lambda _: released((collection,)))


def deleted(collection: bpy.types.Collection) -> Outcome[int]:
    """Removes hierarchy and objects it orphans, returning removed count."""
    tree, items = tuple(dict.fromkeys((collection, *collection.children_recursive))), tuple(collection.all_objects)
    return released(tree).bind(lambda count: released(tuple(item for item in items if item.users == 0)).map(lambda orphans: count + orphans))


def _parented(collection: bpy.types.Collection, parent: Option[bpy.types.Collection]) -> Outcome[bpy.types.Collection]:
    return parent.map(lambda held: relinked(held, (collection,), linked=True).map(lambda _: collection)).default_value(Ok(collection))


def _copied[T: bpy.types.ID](block: T) -> Outcome[T]:
    return Option.of_optional(block.copy()).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(block), member="copy"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["active_editable", "cloned", "created", "deleted", "duplicated", "grouped", "instanced", "relinked", "ungrouped", "walked"]
