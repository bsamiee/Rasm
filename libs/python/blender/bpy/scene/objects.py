"""Object state, stacks, mesh geometry, bounds, and scoped writes."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial, reduce
from itertools import chain, starmap
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
from mathutils import Matrix, Vector
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import Answer, answered, Call, Operator
from blender.bpy.data.ids.ownership import kept
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, mirrored, rooted, Struct, Value, written
from blender.bpy.data.rna.snapshots import Applied, Objects, Scenes, Scope, scoped, SCOPES, Selection, Snapshot, ViewLayers
from blender.bpy.faults import BlenderReason
from blender.bpy.scene.collections import joined
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.trees import Deleted, diff, Edit, Owner, Reparametrized, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type Entries = Mapping[str, Option[Snapshot] | StackEntry]
type Stack = bpy.types.VertexGroups | bpy.types.ObjectModifiers | bpy.types.ObjectConstraints | bpy.types.ObjectShaderFx
type Mover = Callable[[int, int], None]
type Step = Callable[[], Outcome[None]]
type ObjectChange = Swapped | Parented | Resized | Layered | Restacked

# --- [CONSTANTS] ------------------------------------------------------------------------

_EDITING: Final = Setting(descriptor=Descriptor.declared(bpy.types.Object, "mode", bpy.types.EnumProperty), accepted=("EDIT",))
_LEVELS: Final = Descriptor.declared(bpy.types.SubsurfModifier, "levels", bpy.types.IntProperty)
_RENDER_LEVELS: Final = Descriptor.declared(bpy.types.SubsurfModifier, "render_levels", bpy.types.IntProperty)
VERTEX: Final = Descriptor.declared(bpy.types.MeshVertex, "co", bpy.types.FloatProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class StackEntry(msgspec.Struct, frozen=True, kw_only=True):
    """Desired modifier, constraint, or shader effect."""

    kind: str
    values: Option[Snapshot] = Nothing


class ObjectState(msgspec.Struct, frozen=True, kw_only=True):
    """Desired object state, each field unchanged when `Nothing`."""

    data: Option[Handle[bpy.types.ID]] = Nothing
    parent: Option[Handle[bpy.types.Object]] = Nothing
    material_slots: Option[tuple[Snapshot, ...]] = Nothing
    values: Option[Snapshot] = Nothing
    placement: Option[Matrix] = Nothing
    hidden: Option[bool] = Nothing
    selected: Option[bool] = Nothing
    vertex_groups: Option[Mapping[str, Option[Snapshot]]] = Nothing
    modifiers: Option[Mapping[str, StackEntry]] = Nothing
    constraints: Option[Mapping[str, StackEntry]] = Nothing
    shader_effects: Option[Mapping[str, StackEntry]] = Nothing

    def stacks(self, item: bpy.types.Object) -> tuple[tuple[Stack, Option[Mover], Option[Entries]], ...]:
        """Returns object stacks with their reorder call and desired entries."""
        return (
            (item.vertex_groups, Nothing, self.vertex_groups),
            (item.modifiers, Some(item.modifiers.move), self.modifiers),
            (item.constraints, Some(item.constraints.move), self.constraints),
            (item.shader_effects, Nothing, self.shader_effects),
        )


class WorldBounds(msgspec.Struct, frozen=True, kw_only=True):
    """Frozen world-space axis-aligned bounds."""

    low: Vector
    high: Vector


class Restacked(msgspec.Struct, frozen=True, kw_only=True):
    """Structural edits, reorder call, and desired creation parameters of stack."""

    stack: Stack
    move: Option[Mover]
    edits: tuple[Edit[str], ...]
    wanted: tuple[tuple[str, dict[str, str]], ...]


class Swapped(msgspec.Struct, frozen=True, kw_only=True):
    """Replacement object data."""

    data: bpy.types.ID


class Parented(msgspec.Struct, frozen=True, kw_only=True):
    """Replacement object parent."""

    parent: bpy.types.Object


class Resized(msgspec.Struct, frozen=True, kw_only=True):
    """Slot count change."""

    materials: bpy.types.IDMaterials
    before: int
    after: int


class Layered(msgspec.Struct, frozen=True, kw_only=True):
    """Hide and select changes in one view layer."""

    view_layer: bpy.types.ViewLayer
    hidden: Option[bool]
    selected: Option[bool]


class Reconciled(msgspec.Struct, frozen=True, kw_only=True):
    """Applied object changes and snapshot outcomes."""

    changes: tuple[ObjectChange, ...]
    applied: tuple[Applied, ...]


class LevelEdit(bpy.types.PropertyGroup):
    """Viewport and render subdivision levels within Subsurf bounds and the objects they write."""

    levels: bpy.props.IntProperty(**mirrored(_LEVELS.prop))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    render_levels: bpy.props.IntProperty(**mirrored(_RENDER_LEVELS.prop))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scope: bpy.props.EnumProperty(name="Scope", description="Objects to write", items=SCOPES, default=Selection.__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    collection: bpy.props.StringProperty(name="Collection", description="Collection whose objects a collection scope writes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [READS]


def focused(view_layer: bpy.types.ViewLayer) -> Outcome[bpy.types.Object]:
    """Returns active object of view layer, `absent` when none is active."""
    objects = view_layer.objects
    return Option.of_optional(objects.active).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(objects), member="active"))


def descendants(item: bpy.types.Object, view_layer: bpy.types.ViewLayer) -> tuple[bpy.types.Object, ...]:
    """Returns descendants of object the view layer holds, at any depth."""
    held = frozenset(view_layer.objects)
    return tuple(child for child in item.children_recursive if child in held)


def extent(items: Iterable[bpy.types.Object], depsgraph: bpy.types.Depsgraph) -> Option[WorldBounds]:
    """Returns evaluated world bounds, `Nothing` without bounds."""
    corners = np.array(
        [
            (evaluated.matrix_world @ Vector(corner))[:]
            for evaluated in (item.evaluated_get(depsgraph) for item in items)
            if any(any(corner) for corner in evaluated.bound_box)
            for corner in evaluated.bound_box
        ],
        dtype=np.float64,
    ).reshape(-1, 3)
    return Some(WorldBounds(low=Vector(corners.min(axis=0).tolist()).freeze(), high=Vector(corners.max(axis=0).tolist()).freeze())) if corners.size else Nothing  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


# --- [CREATION]


def spawned(name: str, data: Option[bpy.types.ID], collection: bpy.types.Collection) -> Outcome[bpy.types.Object]:
    """Creates object linked into collection, removing it on fault."""
    return kept(
        catch(bpy.data.objects.new, exception=RuntimeError)(name, data.to_optional()).map_error(partial(Fault.of, BlenderReason.REFUSED, name)),
        lambda item: joined(collection, (item,)).map(lambda _: item),
    )


# --- [STACKS]


def restacked(stack: Stack, move: Option[Mover], entries: Entries) -> Outcome[tuple[Restacked, ...]]:
    """Returns stack edits toward entries, recreating out-of-order entries of a stack without reorder call."""
    inputs = tuple(parameter for parameter in stack.bl_rna.functions["new"].parameters if not parameter.is_output)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    choices = {parameter.identifier: tuple(item.identifier for item in parameter.enum_items) for parameter in inputs if isinstance(parameter, bpy.types.EnumProperty)}
    live = tuple((entry.name, {key: getattr(entry, key) for key in choices}) for entry in stack)
    wanted = tuple((name, {"type": entry.kind} if isinstance(entry, StackEntry) else {}) for name, entry in entries.items())
    prefix = next((index for index, (was, now) in enumerate(zip(live, wanted)) if was != now), min(len(live), len(wanted)))
    trees = tuple(Tree(roots=tuple(key for key, _ in pairs), rows=tuple((key, Owner(values=values)) for key, values in pairs)) for pairs in (live, live if move.is_some() else live[:prefix], wanted))
    edits = (*diff(trees[0], trees[1]), *diff(trees[1], trees[2]))
    return accumulated(
        Ok(value).filter_with(choices[key].__contains__, partial(Fault.of, RecordReason.OUTSIDE, repr(stack), member=", ".join(choices[key])))
        for _, parameters in wanted
        for key, value in parameters.items()
    ).map(lambda _: (Restacked(stack=stack, move=move, edits=edits, wanted=wanted),) if edits else ())


def restack(row: Restacked) -> tuple[Step, ...]:
    """Returns removal, insertion, and reorder steps in target order."""
    stack, parameters = row.stack, dict(row.wanted)
    named = "name" in stack.bl_rna.functions["new"].parameters  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def removed(key: str) -> Outcome[None]:
        return catch(stack.remove, exception=RuntimeError)(stack[key]).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(stack)))

    def added(key: str) -> Outcome[None]:
        entry: Outcome[Struct | None] = catch(stack.new, exception=RuntimeError)(**({"name": key} if named else {}), **parameters[key]).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(stack)))

        def renamed(held: Struct | None) -> Outcome[None]:
            match held:
                case None:
                    return Error(Fault.of(BlenderReason.ABSENT, repr(stack), member=key))
                case made:
                    return Ok(None) if named else written(made, "name", key).map(lambda _: None)

        return Result[Struct | None, Faults].bind(entry, renamed)

    def moved(move: Mover, before: int, index: int) -> Outcome[None]:
        return catch(move, exception=RuntimeError)(before, index).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(stack)))

    leaving = tuple(edit.key for edit in row.edits if isinstance(edit, (Deleted, Reparametrized)))
    order = [entry.name for entry in stack if entry.name not in leaving]
    planned: list[Step] = [partial(removed, key) for key in leaving]
    for index, (key, _) in enumerate(row.wanted):
        if key not in order:
            planned.append(partial(added, key))
            order.append(key)
        if (before := order.index(key)) != index:
            planned.extend(partial(moved, move, before, index) for move in row.move.to_list())
            order.insert(index, order.pop(before))
    return tuple(planned)


# --- [CHANGES]


def diverged(state: ObjectState, item: bpy.types.Object, layers: tuple[bpy.types.ViewLayer, ...], data: Option[bpy.types.ID], parent: Option[bpy.types.Object]) -> Outcome[tuple[ObjectChange, ...]]:
    """Returns changes from object to state, accumulating independent faults."""

    def swapped(block: bpy.types.ID) -> Outcome[tuple[ObjectChange, ...]]:
        return Ok(()) if item.data == block else (~_EDITING).check(bpy.context, Some(item)).map(lambda _: (Swapped(data=block),))

    def parented(block: bpy.types.Object) -> Outcome[tuple[ObjectChange, ...]]:
        return (
            Ok(())
            if item.parent == block
            else Error(Fault.of(RecordReason.OUTSIDE, f"{item!r}.parent", block.name, member="objects outside its own hierarchy"))
            if block == item or block in item.children_recursive
            else Ok((Parented(parent=block),))
        )

    def resized(slots: tuple[Snapshot, ...]) -> Outcome[tuple[ObjectChange, ...]]:
        return (
            Result[bpy.types.ID, Faults]
            .bind(
                data.or_else(Option.of_optional(item.data)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(item), member="data.materials")),
                lambda block: Descriptor.resolve(block, "materials", bpy.types.CollectionProperty).map(lambda member: getattr(block, member.prop.identifier)),
            )
            .map(lambda materials: (Resized(materials=materials, before=before, after=after),) if (before := len(materials)) != (after := len(slots)) else ())
        )

    def layered(layer: bpy.types.ViewLayer) -> tuple[ObjectChange, ...]:
        hidden = state.hidden.filter(lambda hide: item.hide_get(view_layer=layer) != hide)
        selected = state.selected.filter(lambda select: item.select_get(view_layer=layer) != select)
        return (Layered(view_layer=layer, hidden=hidden, selected=selected),) if hidden.is_some() or selected.is_some() else ()

    return accumulated((
        *data.map(swapped).to_list(),
        *state.material_slots.map(resized).to_list(),
        *parent.map(parented).to_list(),
        Ok(tuple(chain.from_iterable(layered(layer) for layer in layers))),
        *(restacked(stack, move, entries) for stack, move, wanted in state.stacks(item) for entries in wanted.to_list()),
    )).map(lambda parts: tuple(chain.from_iterable(parts)))


def steps(item: bpy.types.Object, change: ObjectChange) -> tuple[Step, ...]:
    """Returns dependent writes of change."""
    refused = partial(Fault.of, BlenderReason.REFUSED, repr(item))

    match change:
        case Swapped(data=data):
            return (lambda: written(item, "data", data).map(lambda _: None),)
        case Parented(parent=parent):
            return (lambda: written(item, "parent", parent).map(lambda _: None),)
        case Resized(materials=materials, before=before, after=after):

            def resized() -> None:
                for _ in range(after - before):
                    materials.append(None)
                for _ in range(before - after):
                    materials.pop()

            return (lambda: catch(resized, exception=RuntimeError)().map_error(refused),)
        case Layered(view_layer=layer, hidden=hidden, selected=selected):
            return (
                lambda: accumulated(
                    result.map_error(refused)
                    for result in (
                        *(catch(lambda: item.hide_set(hide, view_layer=layer), exception=RuntimeError)() for hide in hidden.to_list()),
                        *(catch(lambda: item.select_set(select, view_layer=layer), exception=RuntimeError)() for select in selected.to_list()),
                    )
                ).map(lambda _: None),
            )
        case Restacked():
            return restack(change)


def reconciling(state: ObjectState) -> Outcome[Callable[[bpy.types.Object, tuple[bpy.types.ViewLayer, ...]], Outcome[Reconciled]]]:
    """Resolves state handles once, returning per-object reconcile writing placement in parent frame."""
    framed = state.placement.map(
        lambda matrix: Snapshot.of(
            bpy.types.Object,
            {"matrix_parent_inverse": tuple(np.eye(4).ravel().tolist()), "location": matrix.to_translation()[:], "rotation_euler": matrix.to_euler()[:], "scale": matrix.to_scale()[:]},
        )
    )

    def live[T: bpy.types.ID](handle: Option[Handle[T]]) -> Outcome[Option[T]]:
        return handle.map(lambda held: held.resolve().map(Some)).default_value(Ok(Nothing))

    def each(found: tuple[Option[bpy.types.ID], Option[bpy.types.Object]], item: bpy.types.Object, layers: tuple[bpy.types.ViewLayer, ...]) -> Outcome[Reconciled]:
        def applied(rows: tuple[ObjectChange, ...]) -> Outcome[Reconciled]:
            pairs = chain(
                ((item, values) for values in (*state.values.to_list(), *framed.to_list())),
                (pair for slots in state.material_slots.to_list() for pair in zip(item.material_slots, slots)),
                (
                    (stack[name], values)
                    for stack, _, wanted in state.stacks(item)
                    for entries in wanted.to_list()
                    for name, entry in entries.items()
                    for values in (entry.values if isinstance(entry, StackEntry) else entry).to_list()
                ),
            )
            return accumulated(applied.outcome.map(lambda _: applied) for owner, values in pairs for applied in (values.apply(owner),)).map(lambda counts: Reconciled(changes=rows, applied=counts))

        return (
            diverged(state, item, layers, *found)
            .bind(lambda rows: reduce(lambda done, step: done.bind(lambda _: step()), chain.from_iterable(steps(item, row) for row in rows), Ok(None)).map(lambda _: rows))
            .bind(applied)
        )

    return paired(live(state.data), live(state.parent)).map(lambda found: partial(each, found))


def reconciled(state: ObjectState, item: bpy.types.Object, layers: tuple[bpy.types.ViewLayer, ...]) -> Outcome[Reconciled]:
    """Reconciles object to state."""
    return reconciling(state).bind(lambda each: each(item, layers))


# --- [GEOMETRY]


def shaped(mesh: bpy.types.Mesh, vertices: npt.NDArray[np.float32], faces: npt.NDArray[np.int64]) -> Outcome[None]:
    """Writes mesh vertices and faces when they differ from live geometry.

    Args:
        mesh: Mesh to write.
        vertices: Vertex positions as float32, one row per vertex.
        faces: Corner vertex indices, one row per face of equal corner count.

    Returns:
        `Ok` once geometry matches, or `edit` fault.
    """
    live, corners, starts = np.empty(len(mesh.vertices) * 3, np.float32), np.empty(len(mesh.loops), np.int32), np.empty(len(mesh.polygons), np.int32)
    for name, member, buffer in (("position", "vector", live), (".corner_vert", "value", corners)):
        if name in mesh.attributes and isinstance(attribute := mesh.attributes[name], bpy.types.FloatVectorAttribute | bpy.types.IntAttribute):
            attribute.data.foreach_get(member, buffer)
    mesh.polygons.foreach_get("loop_start", starts)
    if np.array_equal(live.view(np.uint32), vertices.ravel().view(np.uint32)) and np.array_equal(corners, faces.ravel()) and np.array_equal(starts, np.arange(0, faces.size, faces.shape[1])):
        return Ok(None)
    try:
        mesh.clear_geometry()
        mesh.from_pydata(vertices, (), faces)
    except (ReferenceError, RuntimeError, TypeError, ValueError) as error:
        return Error(Fault.of(BlenderReason.REFUSED, repr(mesh), error))
    return Ok(None)


# --- [SCOPES]


def targets(scope: Scope, context: bpy.types.Context) -> Outcome[tuple[tuple[bpy.types.Object, tuple[bpy.types.ViewLayer, ...]], ...]]:
    """Returns scope objects once with their view layers."""

    def reaching(layers: tuple[bpy.types.ViewLayer, ...]) -> tuple[tuple[bpy.types.Object, tuple[bpy.types.ViewLayer, ...]], ...]:
        held = tuple((layer, frozenset(layer.objects)) for layer in layers)
        return tuple((item, tuple(layer for layer, members in held if item in members)) for item in dict.fromkeys(item for layer in layers for item in layer.objects))

    match scope:
        case Scenes(names=names) | ViewLayers(scenes=names):
            return ViewLayers(scenes=names).owners(context).map(reaching)
        case Selection() | Objects():
            return paired(scope.owners(context), rooted(bpy.types.ViewLayer, context)).map(lambda pair: tuple((item, (pair[1],)) for item in pair[0]))


def fanned(state: ObjectState, scope: Scope, context: bpy.types.Context) -> Outcome[tuple[Reconciled, ...]]:
    """Reconciles every scope object to state, accumulating faults."""
    return paired(reconciling(state), targets(scope, context)).bind(lambda pair: accumulated(starmap(*pair)))


def levelled(levels: Mapping[str, Value], scope: Scope, context: bpy.types.Context) -> Outcome[Answer]:
    """Writes subdivision levels to each Subsurf and Multires modifier in scope.

    Args:
        levels: Viewport and render levels by Subsurf member name.
        scope: Objects to write.
        context: Context `targets` reads.

    Returns:
        `Done` with skipped faults when a modifier changed, `Unchanged` when no level changes, or scope and modifier faults when none changed.
    """
    snapshots = {kind: Snapshot.of(kind, levels) for kind in (bpy.types.SubsurfModifier, bpy.types.MultiresModifier)}
    return targets(scope, context).bind(
        lambda reached: answered(
            Applied.join(tuple(snapshot.apply(entry) for item, _ in reached for entry in item.modifiers for kind, snapshot in snapshots.items() if isinstance(entry, kind))),
            "No Subsurf or Multires modifier in scope takes new levels",
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def subdivision_levels(call: Call[LevelEdit]) -> Outcome[Answer]:
    """Writes subdivision levels to the modifiers in scope."""
    return levelled({"levels": call.properties.levels, "render_levels": call.properties.render_levels}, scoped(call.properties.scope, call.properties.collection), call.context)


subdivision_levels_command: Operator[LevelEdit] = Operator(form=LevelEdit, run=subdivision_levels, presets=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "VERTEX",
    "Entries",
    "Layered",
    "LevelEdit",
    "Mover",
    "ObjectChange",
    "ObjectState",
    "Parented",
    "Reconciled",
    "Resized",
    "Restacked",
    "Stack",
    "StackEntry",
    "Step",
    "Swapped",
    "WorldBounds",
    "descendants",
    "diverged",
    "extent",
    "fanned",
    "focused",
    "levelled",
    "reconciled",
    "restack",
    "restacked",
    "shaped",
    "spawned",
    "steps",
    "subdivision_levels_command",
    "targets",
]
