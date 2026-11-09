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
import msgspec

from rasm_bpy.data._ids.ownership import _kept, _released, _retained
from rasm_bpy.data._ids.store import _resolved, Handle
from rasm_bpy.data._rna.descriptors import Descriptor, written
from rasm_bpy.data._rna.paths import Struct
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_records import accumulated, Fault, Faults, Nothing, Ok, Outcome, reached, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type _Member = bpy.types.Object | bpy.types.Collection
type _LayerPath = tuple[bpy.types.LayerCollection, tuple[bpy.types.LayerCollection, ...]]

# --- [MODELS] ---------------------------------------------------------------------------


class _CollectionState(msgspec.Struct, frozen=True, kw_only=True):
    """Desired collection state, absent fields unchanged."""

    members: Option[tuple[Handle[_Member], ...]] = Nothing
    kinds: frozenset[type[bpy.types.Object] | type[bpy.types.Collection]] = frozenset((bpy.types.Object, bpy.types.Collection))
    values: Option[Snapshot] = Nothing


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WALKS]


def _walked(root: bpy.types.LayerCollection) -> Block[_LayerPath]:
    """Returns layer tree in preorder, each node with ancestors root first."""

    def children(path: _LayerPath) -> tuple[_LayerPath, ...]:
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


def _parents(collection: bpy.types.Collection) -> tuple[bpy.types.Collection, ...]:
    """Returns direct parents in name order, scene collections last."""
    users = sorted(bpy.data.user_map(subset=(collection,), value_types={"COLLECTION", "SCENE"})[collection], key=lambda held: held.name_full)
    return (
        *(user for user in users if isinstance(user, bpy.types.Collection) and collection in user.children.values()),
        *(root for user in users if isinstance(user, bpy.types.Scene) and collection in (root := user.collection).children.values()),
    )


def _reconciled(tree: _CollectionState, collection: bpy.types.Collection) -> Outcome[tuple[_Member, ...]]:
    """Unlinks undesired members, links missing ones, writes Outliner order and values, returning members changed, every expired handle accumulated."""

    def listed(wanted: tuple[Handle[_Member], ...]) -> Outcome[tuple[_Member, ...]]:
        held = dict(zip((*collection.objects, *collection.children), (item.sort_index for item in (*collection.collection_objects, *collection.collection_children))))

        def written_all(items: tuple[_Member, ...]) -> Outcome[tuple[_Member, ...]]:
            included = frozenset(items)
            dropped = tuple(item for item in held if type(item) in tree.kinds and item not in included)
            added = tuple(item for item in items if item not in held)
            moved = tuple((index, item) for index, item in enumerate(items) if held.get(item) != index)
            links = (*((item, False) for item in dropped), *((item, True) for item in added))
            return (
                reduce(Result[None, Faults].bind, (lambda _: _linked(collection, link[0], linked=link[1]) for link in links), Result[None, Faults](ok=None))
                .map(
                    lambda _: (
                        dict[_Member, bpy.types.CollectionObject | bpy.types.CollectionChild](
                            chain(zip(collection.objects, collection.collection_objects), zip(collection.children, collection.collection_children))
                        )
                        if moved
                        else dict[_Member, bpy.types.CollectionObject | bpy.types.CollectionChild]()
                    )
                )
                .bind(
                    lambda entries: reduce(
                        Result[None, Faults].bind, (lambda _: written(entries[order[1]], ("sort_index",), order[0]).map(lambda _: None) for order in moved), Result[None, Faults](ok=None)
                    )
                )
                .map(lambda _: (*dropped, *added, *(item for _, item in moved)))
            )

        return _resolved(tuple(dict.fromkeys(wanted))).bind(written_all)

    changes: Outcome[tuple[_Member, ...]] = tree.members.map(listed).default_value(Ok(()))
    return changes.bind(lambda rows: tree.values.map(lambda values: values.apply(collection).outcome.map(lambda _: rows)).default_value(Ok(rows)))


def _relinked(collection: bpy.types.Collection, members: tuple[_Member, ...], *, linked: bool) -> Outcome[tuple[_Member, ...]]:
    """Links absent members, or unlinks present ones, once each, accumulating faults in input order, returning the members changed."""
    held = frozenset((*collection.objects, *collection.children))
    changed = tuple(item for item in dict.fromkeys(members) if (item in held) is not linked)
    return accumulated(_linked(collection, item, linked=linked) for item in changed).map(lambda _: changed)


def _linked(collection: bpy.types.Collection, member: _Member, *, linked: bool) -> Outcome[None]:
    """Links or unlinks one member through the host collection it belongs to."""
    members: bpy.types.CollectionObjects | bpy.types.CollectionChildren = collection.objects if isinstance(member, bpy.types.Object) else collection.children
    return catch(members.link if linked else members.unlink, exception=RuntimeError)(member).map_error(partial(Fault.of, Reason.REFUSED, repr(collection)))


# --- [PLACEMENT]


def active_editable(view_layer: bpy.types.ViewLayer) -> Outcome[bpy.types.Collection]:
    """Returns active collection or nearest editable included ancestor."""
    active: bpy.types.LayerCollection = view_layer.active_layer_collection  # ty: ignore[invalid-assignment]
    path = next((*ancestors, node) for node, ancestors in _walked(view_layer.layer_collection) if node == active)
    return Option.of_optional(next((layer.collection for layer in reversed(tuple(takewhile(lambda layer: not layer.exclude, path))) if _editable(layer.collection)), None)).to_result_with(
        lambda: Fault.of(Reason.ABSENT, repr(view_layer), member="active_layer_collection")
    )


def _moved(item: bpy.types.Object, target: bpy.types.Collection, scene: bpy.types.Scene) -> Outcome[tuple[bpy.types.Collection, ...]]:
    """Moves object to target from other editable scene collections, keeping its base, returning the collections whose membership changed."""
    held = tuple(collection for collection in _holders(item, scene) if collection != target)
    return _relinked(target, (item,), linked=True).bind(
        lambda first: accumulated(_relinked(collection, (item,), linked=False).map(lambda changed: (collection,) if changed else ()) for collection in held).map(
            lambda rest: (*((target,) if first else ()), *chain.from_iterable(rest))
        )
    )


def instanced(collection: bpy.types.Collection, target: bpy.types.Collection, name: str) -> Outcome[bpy.types.Object]:
    """Creates empty instancing collection in target under name, removing it on fault."""
    return _kept(
        catch(bpy.data.objects.new, exception=RuntimeError)(name, None).map_error(partial(Fault.of, Reason.REFUSED, name)),
        lambda made: (
            accumulated((written(made, ("instance_type",), "COLLECTION"), written(made, ("instance_collection",), collection)))
            .bind(lambda _: _relinked(target, (made,), linked=True))
            .map(lambda _: made)
        ),
    )


def _editable(collection: bpy.types.Collection) -> bool:
    return collection.is_editable and collection.override_library is None and collection.importer is None


def _holders(item: bpy.types.Object, scene: bpy.types.Scene) -> tuple[bpy.types.Collection, ...]:
    """Editable collections of the scene that link the object directly."""
    holders = frozenset(item.users_collection)
    return tuple(held for held in (scene.collection, *scene.collection.children_recursive) if held in holders and _editable(held))


# --- [LIFECYCLE]


def created(parent: Option[bpy.types.Collection], name: str) -> Outcome[bpy.types.Collection]:
    """Creates collection under parent named name, removing it on fault."""
    return _kept(catch(bpy.data.collections.new, exception=RuntimeError)(name).map_error(partial(Fault.of, Reason.REFUSED, name)), lambda made: _parented(made, parent))


def cloned(collection: bpy.types.Collection, parent: Option[bpy.types.Collection]) -> Outcome[bpy.types.Collection]:
    """Returns shallow copy linked into parent, removing it on fault."""
    return _kept(_copied(collection), lambda made: _parented(made, parent))


def duplicated(collection: bpy.types.Collection, parent: bpy.types.Collection, copies: frozenset[type[bpy.types.ID]]) -> Outcome[bpy.types.Collection]:
    """Duplicates local hierarchy with object data of `copies` types, removing every copy on fault."""
    tree = tuple(reached((collection,), lambda held: (child for child in held.children if child.library is None)))
    with ExitStack() as rollback:

        def copied[T: bpy.types.ID](blocks: Iterable[T]) -> Outcome[dict[T, T]]:
            return accumulated(_kept(_copied(block), Ok, Some(rollback)).map(lambda held: (block, held)) for block in blocks).map(dict)

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
                type Changed = Outcome[Option[object]]
                completed: list[Changed] = []

                def remapped(value: object) -> Generator[object, Changed]:
                    match value:
                        case bpy.types.ID() if value in mapping:
                            completed.append(Ok(Some(mapping[value])))
                        case list():
                            answers: list[Changed] = []
                            for item in value:
                                answers.append((yield item))  # ruff: ignore[manual-list-comprehension]
                            completed.append(
                                accumulated(answers).map(
                                    lambda changes: Some([change.default_value(item) for item, change in zip(value, changes)]) if any(change.is_some() for change in changes) else Nothing
                                )
                            )
                        case _:
                            completed.append(Ok(Nothing))

                def members(held: Struct | IDPropertyGroup) -> Generator[object, Changed]:
                    answers: list[Outcome[None]] = []
                    for name, value in held.items():
                        changed = yield value
                        answers.append(
                            changed.bind(
                                lambda change: accumulated(
                                    catch(held.__setitem__, exception=ReferenceError)(name, replacement).map_error(partial(Fault.of, Reason.REFUSED, f"{held!r}[{name!r}]"))  # ty: ignore[unresolved-attribute]
                                    for replacement in change.to_list()
                                ).map(lambda _: None)
                            )
                        )
                    completed.append(accumulated(answers).map(lambda _: Nothing))

                pending = [members(group)]
                answer: Option[Changed] = Nothing
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
                            _relinked(made, members, linked=False)
                            .bind(lambda _: _relinked(made, wanted, linked=True))
                            .bind(lambda _: accumulated(_entry(made, item).bind(lambda held: snapshot.apply(held).outcome) for item, snapshot in zip(wanted, values)))
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
                    _entry(parent, collection)
                    .bind(Snapshot.capture)
                    .bind(
                        lambda values: (
                            _relinked(parent, (made,), linked=True)
                            .bind(lambda _: _entry(parent, made))
                            .bind(lambda held: values.apply(held).outcome)
                            .bind(lambda _: _reconciled(_CollectionState(members=Some(members)), parent))
                        )
                    )
                    .map(lambda _: made)
                )

            for original, made in objects.items():
                made.matrix_parent_inverse = original.matrix_parent_inverse.copy()
            seeds: Iterable[tuple[type[Struct], Struct]] = ((type(made), made) for made in mapping.values())  # ty: ignore[unsound-assignment]
            reached(seeds, followed)
            return accumulated(writes).bind(lambda _: accumulated(rebuilt(held) for held in tree)).bind(placed)

        return _retained(
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
    holders = _holders(item, scene) or (scene.collection,)
    return _kept(created(Nothing, name), lambda made: _relinked(made, (item,), linked=True).bind(lambda _: accumulated(_relinked(held, (made,), linked=True) for held in holders)).map(lambda _: made))


def ungrouped(collection: bpy.types.Collection) -> Outcome[int]:
    """Dissolves collection into its parents, returning removed count."""
    members = (*collection.objects, *collection.children)
    return accumulated(_relinked(parent, members, linked=True) for parent in _parents(collection)).bind(lambda _: _released((collection,)))


def deleted(collection: bpy.types.Collection) -> Outcome[int]:
    """Removes hierarchy and objects it orphans, returning removed count."""
    tree, items = tuple(dict.fromkeys((collection, *collection.children_recursive))), tuple(collection.all_objects)
    return _released(tree).bind(lambda count: _released(tuple(item for item in items if item.users == 0)).map(lambda orphans: count + orphans))


def _parented(collection: bpy.types.Collection, parent: Option[bpy.types.Collection]) -> Outcome[bpy.types.Collection]:
    return parent.map(lambda held: _relinked(held, (collection,), linked=True).map(lambda _: collection)).default_value(Ok(collection))


def _copied[T: bpy.types.ID](block: T) -> Outcome[T]:
    return Option.of_optional(block.copy()).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(block), member="copy"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["active_editable", "cloned", "created", "deleted", "duplicated", "grouped", "instanced", "ungrouped"]
