"""Object state, stacks, owned objects, mesh geometry, bounds, and scoped writes."""

from collections.abc import Callable, Iterable, Mapping
from contextlib import ExitStack
from functools import partial, reduce
from itertools import chain, repeat, starmap
from typing import Final

import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
from mathutils import Matrix, Vector
import msgspec
import numpy as np
import numpy.typing as npt

from rasm_bpy.commands.operators import Answer, answered, Call, called, Operator
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.ownership import _kept, _retained, Ownership
from rasm_bpy.data._ids.store import Handle
from rasm_bpy.data._rna.conditions import chosen, Objects, Scenes, Scope, scope_choice, SCOPE_COLLECTION, Selection, ViewLayers
from rasm_bpy.data._rna.descriptors import Descriptor, Value, written
from rasm_bpy.data._rna.paths import _definition, RnaPath, rooted, Struct
from rasm_bpy.data._rna.properties import _redeclared
from rasm_bpy.data._rna.snapshots import Applied, Snapshot
from rasm_bpy.scene._collections import _relinked
from rasm_records import accumulated, Error, Fault, Faults, Nothing, Ok, Outcome, paired, Reason, typed

# --- [TYPES] ----------------------------------------------------------------------------

type _Entries = Mapping[str, Option[Snapshot] | _StackEntry]
type _Stack = bpy.types.VertexGroups | bpy.types.ObjectModifiers | bpy.types.ObjectConstraints | bpy.types.ObjectShaderFx
type _Mover = Callable[[str, int], Outcome[None]]
type _Step = Callable[[], Outcome[None]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_LEVELS: Final = Descriptor[bpy.types.IntProperty](root=bpy.types.SubsurfModifier, prop=bpy.types.SubsurfModifier.bl_rna.properties["levels"])  # ty: ignore[invalid-argument-type]
_RENDER_LEVELS: Final = Descriptor[bpy.types.IntProperty](root=bpy.types.SubsurfModifier, prop=bpy.types.SubsurfModifier.bl_rna.properties["render_levels"])  # ty: ignore[invalid-argument-type]
_VERTEX: Final = Descriptor[bpy.types.FloatProperty](root=bpy.types.MeshVertex, prop=bpy.types.MeshVertex.bl_rna.properties["co"])  # ty: ignore[invalid-argument-type]

# --- [MODELS] ---------------------------------------------------------------------------


class _StackEntry(msgspec.Struct, frozen=True, kw_only=True):
    """Desired modifier, constraint, or shader effect."""

    kind: str
    values: Option[Snapshot] = Nothing


class _ObjectState(msgspec.Struct, frozen=True, kw_only=True):
    """Desired object state, each field unchanged when `Nothing`."""

    data: Option[Handle[bpy.types.ID]] = Nothing
    parent: Option[Handle[bpy.types.Object]] = Nothing
    material_slots: Option[tuple[Snapshot, ...]] = Nothing
    values: Option[Snapshot] = Nothing
    placement: Option[Matrix] = Nothing
    hidden: Option[bool] = Nothing
    selected: Option[bool] = Nothing
    vertex_groups: Option[Mapping[str, Option[Snapshot]]] = Nothing
    modifiers: Option[Mapping[str, _StackEntry]] = Nothing
    constraints: Option[Mapping[str, _StackEntry]] = Nothing
    shader_effects: Option[Mapping[str, _StackEntry]] = Nothing

    def stacks(self, item: bpy.types.Object) -> tuple[tuple[_Stack, Outcome[_Mover], Option[_Entries]], ...]:
        """Returns object stacks with their reorder call and desired entries."""
        return (
            (
                item.vertex_groups,
                Ok(partial(_vertex_group_moved, item)).filter_with(
                    lambda _: not (isinstance(item.data, bpy.types.Lattice) and item.mode == "EDIT"),
                    lambda _: Fault.of(Reason.OUTSIDE, f"{item!r}.mode", item.mode, member="Object Mode for lattice vertex-group reordering"),
                ),
                self.vertex_groups,
            ),
            (item.modifiers, Ok(partial(_modifier_moved, item.modifiers)), self.modifiers),
            (
                item.constraints,
                Ok(lambda name, index: catch(item.constraints.move, exception=RuntimeError)(item.constraints.find(name), index).map_error(partial(Fault.of, Refusal.REFUSED, repr(item.constraints)))),
                self.constraints,
            ),
            (item.shader_effects, Ok(partial(_shader_effect_moved, item)), self.shader_effects),
        )


class LevelEdit(bpy.types.PropertyGroup):
    """Viewport and render subdivision levels within Subsurf bounds and the objects they write."""

    levels: _redeclared(_LEVELS.prop)  # ty: ignore[invalid-type-form]
    render_levels: _redeclared(_RENDER_LEVELS.prop)  # ty: ignore[invalid-type-form]
    scope: scope_choice(Selection)  # ty: ignore[invalid-type-form]
    collection: SCOPE_COLLECTION  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [READS]


def _active_object(view_layer: bpy.types.ViewLayer) -> Outcome[bpy.types.Object]:
    """Returns active object of view layer, `absent` when none is active."""
    objects = view_layer.objects
    return Option.of_optional(objects.active).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(objects), member="active"))


def _descendants(item: bpy.types.Object, view_layer: bpy.types.ViewLayer) -> tuple[bpy.types.Object, ...]:
    """Returns descendants of object the view layer holds, at any depth."""
    held = frozenset(view_layer.objects)
    return tuple(child for child in item.children_recursive if child in held)


def _corners(items: Iterable[bpy.types.Object], depsgraph: bpy.types.Depsgraph) -> npt.NDArray[np.float64]:
    """Returns native evaluated bounding-box corners in world space, shaped (corners, 3), including zero corners at an object's origin."""
    evaluated = tuple(item.evaluated_get(depsgraph) for item in items)
    boxes = np.asarray([held.bound_box for held in evaluated], dtype=np.float64).reshape(-1, 8, 3)
    matrices = np.asarray([held.matrix_world for held in evaluated], dtype=np.float64).reshape(-1, 4, 4)
    return (boxes @ matrices[:, :3, :3].transpose(0, 2, 1) + matrices[:, None, :3, 3]).reshape(-1, 3)


def _extent(items: Iterable[bpy.types.Object], depsgraph: bpy.types.Depsgraph) -> Option[tuple[Vector, Vector]]:
    """Returns frozen low and high world corners of native evaluated bounds, `Nothing` for no objects."""
    points = _corners(items, depsgraph)
    return Some((Vector(points.min(axis=0).tolist()).freeze(), Vector(points.max(axis=0).tolist()).freeze())) if points.size else Nothing


# --- [CREATION]


def _spawned(name: str, data: Option[bpy.types.ID], collection: bpy.types.Collection) -> Outcome[bpy.types.Object]:
    """Creates object linked into collection, removing it on fault."""
    return _kept(
        catch(bpy.data.objects.new, exception=RuntimeError)(name, data.to_optional()).map_error(partial(Fault.of, Refusal.REFUSED, name)),
        lambda item: _relinked(collection, (item,), linked=True).map(lambda _: item),
    )


def _owned_object[D: bpy.types.ID](
    rows: tuple[Ownership[bpy.types.Object], Ownership[D]],
    name: str,
    created: Callable[[], D],
    collection: bpy.types.Collection,
    identity: Identity,
    /,
    *,
    of: Option[bpy.types.ID] = Nothing,
    slot: Option[int] = Nothing,
    scope: Option[ExitStack] = Nothing,
    seeded: Callable[[D], Outcome[D]] = Ok,
) -> Outcome[tuple[bpy.types.Object, D]]:
    """Reuses or builds the owned object of a row, a built object holding new data owned under the data row.

    Args:
        rows: Object row, then the row owning its data.
        name: Name of a built object and its data.
        created: Host call creating the data of a built object.
        collection: Collection a built object links into.
        identity: Identity marking object and data.
        of: Data-block both serve.
        slot: Slot telling apart the objects one source keeps.
        scope: Stack removing the built object and data on unwind.
        seeded: Fill of newly created data alone.

    Returns:
        Object with its data, or refusal, ownership, and data type faults.
    """
    held, owner = rows

    with ExitStack() as rollback:

        def built() -> Outcome[bpy.types.Object]:
            return (
                Result[D, Faults]
                .bind(
                    catch(created, exception=RuntimeError)().map_error(partial(Fault.of, Refusal.REFUSED, name)), lambda block: owner.owned(identity, block, of=of, filled=seeded, scope=Some(rollback))
                )
                .bind(lambda block: _spawned(name, Some(block), collection))
            )

        return _retained(
            held.made(identity, built, of=of, slot=slot, scope=Some(rollback)).bind(lambda item: typed(owner.kind, f"{item!r}.data", item.data).map(lambda block: (item, block))), rollback, scope
        )


# --- [CHANGES]


def _vertex_group_moved(item: bpy.types.Object, name: str, index: int) -> Outcome[None]:
    """Moves a vertex group through the stock operator, preserving weights and active group."""
    groups = item.vertex_groups
    before = groups.find(name)
    if (active := groups.active) is None:
        return Error(Fault.of(Reason.ABSENT, repr(groups), member="active"))

    def shifted(_: None) -> Outcome[None]:
        return called(bpy.ops.object.vertex_group_move, {"direction": "UP" if index < before else "DOWN"}, override=Some(bpy.context.temp_override(object=item, active_object=item))).map(
            lambda _: None
        )

    with ExitStack() as scope:
        return (
            written(groups, ("active_index",), before)
            .map(lambda _: scope.callback(setattr, groups, "active", active))
            .bind(lambda _: reduce(Result[None, Faults].bind, repeat(shifted, abs(index - before)), Ok(None)))
        )


def _shader_effect_moved(item: bpy.types.Object, name: str, index: int) -> Outcome[None]:
    """Moves a shader effect through the stock operator, preserving effect data."""
    return called(bpy.ops.object.shaderfx_move_to_index, {"shaderfx": name, "index": index}, override=Some(bpy.context.temp_override(object=item, active_object=item))).map(lambda _: None)


def _modifier_moved(stack: bpy.types.ObjectModifiers, name: str, index: int) -> Outcome[None]:
    """Moves a modifier within its live pinned segment, reporting the boundary Blender refuses without an error."""
    before = stack.find(name)
    modifier = stack[before]
    pinned = modifier.use_pin_to_last
    blocked = tuple(held for held in stack[min(before, index) : max(before, index) + 1] if held.use_pin_to_last != pinned)
    return (
        Error(Fault.of(Reason.OUTSIDE, repr(modifier), blocked, member="its pinned modifier segment"))
        if blocked
        else catch(stack.move, exception=RuntimeError)(before, index).map_error(partial(Fault.of, Refusal.REFUSED, repr(stack)))
    )


def _restacked(item: bpy.types.Object, stack: _Stack, move: Outcome[_Mover], entries: _Entries) -> Outcome[tuple[_Step, ...]]:
    """Returns removal, insertion, and native reorder steps toward entries, every refused type before any write."""
    new = _definition(stack).functions["new"]
    choices = {
        parameter.identifier: tuple(item.identifier for item in parameter.enum_items) for parameter in new.parameters if not parameter.is_output and isinstance(parameter, bpy.types.EnumProperty)
    }
    named = "name" in new.parameters
    live = tuple((entry.name, {key: getattr(entry, key) for key in choices}) for entry in stack)
    wanted = tuple((name, {"type": entry.kind} if isinstance(entry, _StackEntry) else {}) for name, entry in entries.items())
    parameters = dict(wanted)
    leaving = tuple(name for name, values in live if parameters.get(name) != values)
    refused = partial(Fault.of, Refusal.REFUSED, repr(stack))

    def removed(key: str) -> Outcome[None]:
        return catch(stack.remove, exception=RuntimeError)(stack[key]).map_error(refused)

    def added(key: str) -> Outcome[None]:
        def renamed(held: Struct | None) -> Outcome[None]:
            match held:
                case None:
                    return Error(Fault.of(Reason.ABSENT, repr(stack), member=key))
                case made:
                    return Ok(None) if named else written(made, ("name",), key).map(lambda _: None)

        entry: Outcome[Struct | None] = catch(stack.new, exception=RuntimeError)(**({"name": key} if named else {}), **parameters[key]).map_error(refused)  # ty: ignore[unsound-assignment]
        return Result[Struct | None, Faults].bind(entry, renamed)

    order = [name for name, _ in live if name not in leaving]
    adding = tuple(key for key, _ in wanted if key not in order)
    planned = (*(partial(removed, key) for key in leaving), *(partial(added, key) for key in adding))
    moves: list[tuple[str, int]]
    if adding and stack == item.modifiers:
        moves = [(key, index) for index, (key, _) in enumerate(wanted)]
    else:
        moves = []
        order.extend(adding)
        for index, (key, _) in enumerate(wanted):
            if (before := order.index(key)) != index:
                moves.append((key, index))
                order.insert(index, order.pop(before))
    return paired(
        accumulated(
            Ok(value).filter_with(choices[key].__contains__, partial(Fault.of, Reason.OUTSIDE, repr(stack), member=", ".join(choices[key]))) for _, held in wanted for key, value in held.items()
        ),
        move.map(lambda mover: (*planned, *(partial(mover, *indices) for indices in moves))) if moves else Ok(planned),
    ).map(lambda pair: pair[1])


def _diverged(state: _ObjectState, item: bpy.types.Object, layers: tuple[bpy.types.ViewLayer, ...], data: Option[bpy.types.ID], parent: Option[bpy.types.Object]) -> Outcome[tuple[_Step, ...]]:
    """Returns writes from object to state, accumulating independent faults before any write, the data and parent writes the host drops refused."""
    refused = partial(Fault.of, Refusal.REFUSED, repr(item))
    unchanged: Outcome[tuple[_Step, ...]] = Ok(())

    def resized(slots: tuple[Snapshot, ...]) -> Outcome[tuple[_Step, ...]]:
        def sized(materials: bpy.types.IDMaterials) -> None:
            for _ in range(len(slots) - len(materials)):
                materials.append(None)
            for _ in range(len(materials) - len(slots)):
                materials.pop()

        return (
            Result[bpy.types.ID, Faults]
            .bind(
                data.or_else(Option.of_optional(item.data)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(item), member="data.materials")),
                lambda block: Descriptor.resolve(block, ("materials",), bpy.types.CollectionProperty).map(lambda member: getattr(block, member.prop.identifier)),
            )
            .map(lambda materials: (lambda: catch(sized, exception=RuntimeError)(materials).map_error(refused),) if len(materials) != len(slots) else ())
        )

    def layered(layer: bpy.types.ViewLayer) -> tuple[_Step, ...]:
        def wrote(write: Callable[[bool], None], /, *, value: bool) -> Outcome[None]:
            return catch(write, exception=RuntimeError)(value).map_error(refused)

        writes = (
            (partial(item.hide_set, view_layer=layer), state.hidden.filter(lambda hide: item.hide_get(view_layer=layer) != hide)),
            (partial(item.select_set, view_layer=layer), state.selected.filter(lambda select: item.select_get(view_layer=layer) != select)),
        )
        return tuple(partial(wrote, write, value=value) for write, wanted in writes for value in wanted.to_list())

    return accumulated((
        *data.map(
            lambda block: unchanged.filter_with(
                lambda _: item.data == block or item.mode != "EDIT", lambda _: Fault.of(Reason.OUTSIDE, f"{item!r}.mode", item.mode, member="a mode other than Edit Mode, where data writes drop")
            )
        ).to_list(),
        *state.material_slots.map(resized).to_list(),
        *parent.map(
            lambda block: unchanged.filter_with(
                lambda _: item.parent == block or not (block == item or block in item.children_recursive),
                lambda _: Fault.of(Reason.OUTSIDE, f"{item!r}.parent", block.name, member="objects outside its own hierarchy"),
            )
        ).to_list(),
        Ok(tuple(chain.from_iterable(map(layered, layers)))),
        *(_restacked(item, stack, move, entries) for stack, move, wanted in state.stacks(item) for entries in wanted.to_list()),
    )).map(lambda parts: tuple(chain.from_iterable(parts)))


def _reconciler(state: _ObjectState) -> Outcome[Callable[[bpy.types.Object, tuple[bpy.types.ViewLayer, ...]], Outcome[Applied]]]:
    """Resolves state handles once, returning per-object reconcile writing placement in parent frame."""
    framed = state.placement.map(
        lambda matrix: Snapshot.of(bpy.types.Object, {("matrix_parent_inverse",): tuple(np.eye(4).ravel().tolist()), ("matrix_basis",): tuple(np.asarray(matrix, np.float64).T.ravel().tolist())})
    )

    def live[T: bpy.types.ID](handle: Option[Handle[T]]) -> Outcome[Option[T]]:
        return handle.map(lambda held: held.resolve().map(Some)).default_value(Ok(Nothing))

    def each(found: tuple[Option[bpy.types.ID], Option[bpy.types.Object]], item: bpy.types.Object, layers: tuple[bpy.types.ViewLayer, ...]) -> Outcome[Applied]:
        def applied(_: None) -> Outcome[Applied]:
            pairs = chain(
                ((item, values) for values in (*state.values.to_list(), *framed.to_list())),
                (pair for slots in state.material_slots.to_list() for pair in zip(item.material_slots, slots)),
                (
                    (stack[name], values)
                    for stack, _, wanted in state.stacks(item)
                    for entries in wanted.to_list()
                    for name, entry in entries.items()
                    for values in (entry.values if isinstance(entry, _StackEntry) else entry).to_list()
                ),
            )
            return accumulated(report.outcome.map(lambda _: report) for owner, values in pairs for report in (values.apply(owner),)).map(Applied.join)

        pointed = tuple((path, block) for path, held in ((("data",), found[0]), (("parent",), found[1])) for block in held.to_list())
        return (
            _diverged(state, item, layers, *found)
            .bind(lambda planned: accumulated(written(item, path, block) for path, block in pointed).map(lambda _: planned))
            .bind(lambda planned: reduce(lambda done, step: done.bind(lambda _: step()), planned, Ok(None)))
            .bind(applied)
        )

    return paired(live(state.data), live(state.parent)).map(lambda found: partial(each, found))


def _reconciled(state: _ObjectState, item: bpy.types.Object, layers: tuple[bpy.types.ViewLayer, ...]) -> Outcome[Applied]:
    """Reconciles object to state, returning its value writes."""
    return _reconciler(state).bind(lambda each: each(item, layers))


# --- [GEOMETRY]


def _shaped(mesh: bpy.types.Mesh, vertices: npt.NDArray[np.float32], faces: npt.NDArray[np.int64]) -> Outcome[None]:
    """Writes mesh vertices and faces when they differ from live geometry.

    Args:
        mesh: Mesh to write.
        vertices: Vertex positions as float32, one row per vertex.
        faces: Corner vertex indices, one row per face of equal corner count.

    Returns:
        `Ok` once geometry matches, or `refused` fault.
    """
    live, loops, starts = np.empty(len(mesh.vertices) * 3, np.float32), np.empty(len(mesh.loops), np.int32), np.empty(len(mesh.polygons), np.int32)
    mesh.vertices.foreach_get("co", live)
    mesh.loops.foreach_get("vertex_index", loops)
    mesh.polygons.foreach_get("loop_start", starts)
    if np.array_equal(live.view(np.uint32), vertices.ravel().view(np.uint32)) and np.array_equal(loops, faces.ravel()) and np.array_equal(starts, np.arange(0, faces.size, faces.shape[1])):
        return Ok(None)

    @catch(exception=RuntimeError)
    def filled() -> None:
        mesh.clear_geometry()
        mesh.vertices.add(len(vertices))
        mesh.loops.add(faces.size)
        mesh.polygons.add(len(faces))
        mesh.vertices.foreach_set("co", vertices.ravel())
        mesh.polygons.foreach_set("loop_start", np.arange(0, faces.size, faces.shape[1], dtype=np.int32))
        mesh.polygons.foreach_set("vertices", faces.ravel().astype(np.int32))
        mesh.update(calc_edges=True)
        mesh.shade_flat()

    return filled().map_error(partial(Fault.of, Refusal.REFUSED, repr(mesh)))


# --- [SCOPES]


def _targets(scope: Scope, context: bpy.types.Context) -> Outcome[tuple[tuple[bpy.types.Object, tuple[bpy.types.ViewLayer, ...]], ...]]:
    """Returns scope objects once with their view layers."""

    def reaching(layers: tuple[bpy.types.ViewLayer, ...]) -> tuple[tuple[bpy.types.Object, tuple[bpy.types.ViewLayer, ...]], ...]:
        held = tuple((layer, frozenset(layer.objects)) for layer in layers)
        return tuple((item, tuple(layer for layer, members in held if item in members)) for item in dict.fromkeys(item for layer in layers for item in layer.objects))

    match scope:
        case Scenes(names=names) | ViewLayers(scenes=names):
            return ViewLayers(scenes=names).owners(context).map(reaching)
        case Selection() | Objects():
            return paired(scope.owners(context), rooted(bpy.types.ViewLayer, context)).map(lambda pair: tuple((item, (pair[1],)) for item in pair[0]))


def _fanned(state: _ObjectState, scope: Scope, context: bpy.types.Context) -> Outcome[tuple[Applied, ...]]:
    """Reconciles every scope object to state, accumulating faults."""
    return paired(_reconciler(state), _targets(scope, context)).bind(lambda pair: accumulated(starmap(*pair)))


def _leveled(levels: Mapping[RnaPath, Value], scope: Scope, context: bpy.types.Context) -> Outcome[Answer]:
    """Writes subdivision levels to each Subsurf and Multires modifier in scope.

    Args:
        levels: Viewport and render levels by Subsurf member name.
        scope: Objects to write.
        context: Context `targets` reads.

    Returns:
        `Done` with skipped faults when a modifier changed, `Unchanged` when no level changes, or scope and modifier faults when none changed.
    """
    snapshots = {kind: Snapshot.of(kind, levels) for kind in (bpy.types.SubsurfModifier, bpy.types.MultiresModifier)}
    return _targets(scope, context).bind(
        lambda reached: answered(
            Applied.join(tuple(snapshot.apply(entry) for item, _ in reached for entry in item.modifiers for kind, snapshot in snapshots.items() if isinstance(entry, kind))),
            "No Subsurf or Multires modifier in scope takes new levels",
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(LevelEdit, options=frozenset({"PRESET", "UNDO"}))
def subdivision_levels(call: Call[LevelEdit]) -> Outcome[Answer]:
    """Writes subdivision levels to the modifiers in scope."""
    return _leveled({("levels",): call.properties.levels, ("render_levels",): call.properties.render_levels}, chosen(call.properties), call.context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LevelEdit", "subdivision_levels"]
