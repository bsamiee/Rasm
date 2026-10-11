"""Desired object state, native stack edits, evaluated bounds, and scoped writes."""

from collections.abc import Callable, Iterable, Mapping
from contextlib import ExitStack
from functools import partial, reduce
from itertools import chain
from typing import Annotated, ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
from mathutils import Matrix, Vector
import msgspec

from rasm.blender.commands.operators import Done, Operator
from rasm.blender.data.ids.store import Handle
from rasm.blender.data.rna.conditions import Not, Setting
from rasm.blender.data.rna.descriptors import Descriptor, written
from rasm.blender.data.rna.snapshots import Applied, Objects, Scenes, Scope, Selection, Snapshot, ViewLayers
from rasm.blender.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderResult, BlenderUnmetFault
from rasm.blender.scene.collections import joined
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Presentation
from rasm.rendering.edits import Deleted, diff, Edit, Owner, Reparametrized, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type Entries = Mapping[str, Option[Snapshot] | StackEntry]
type Stack = bpy.types.VertexGroups | bpy.types.ObjectModifiers | bpy.types.ObjectConstraints | bpy.types.ObjectShaderFx
type Step = Callable[[], BlenderResult[None]]
type ObjectChange = Swapped | Parented | Resized | Layered | Restacked


# --- [MODELS] ---------------------------------------------------------------------------


class StackEntry(msgspec.Struct, frozen=True, kw_only=True):
    """Desired modifier, constraint, or shader effect."""

    kind: str
    values: Option[Snapshot] = Nothing


class ObjectState(msgspec.Struct, frozen=True, kw_only=True):
    """Desired concerns of an object, with Nothing leaving each concern unchanged."""

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

    def stacks(self, item: bpy.types.Object) -> tuple[tuple[Stack, Option[Entries]], ...]:
        """Pairs native stacks with their desired entries in declaration order."""
        return ((item.vertex_groups, self.vertex_groups), (item.modifiers, self.modifiers), (item.constraints, self.constraints), (item.shader_effects, self.shader_effects))


class WorldBounds(msgspec.Struct, frozen=True, kw_only=True):
    """Frozen world-space axis-aligned bounds."""

    low: Vector
    high: Vector


class Restacked(msgspec.Struct, frozen=True, kw_only=True):
    """Structural edits and desired native creation parameters of a stack."""

    stack: Stack
    edits: tuple[Edit[str], ...]
    wanted: tuple[tuple[str, dict[str, str]], ...]


class Swapped(msgspec.Struct, frozen=True, kw_only=True):
    """Replacement data of an object."""

    data: bpy.types.ID


class Parented(msgspec.Struct, frozen=True, kw_only=True):
    """Replacement parent of an object."""

    parent: bpy.types.Object


class Resized(msgspec.Struct, frozen=True, kw_only=True):
    """Change in material-slot count."""

    materials: bpy.types.IDMaterials
    before: int
    after: int


class Layered(msgspec.Struct, frozen=True, kw_only=True):
    """Differing base flags of one view layer."""

    view_layer: bpy.types.ViewLayer
    hidden: Option[bool]
    selected: Option[bool]


class Reconciled(msgspec.Struct, frozen=True, kw_only=True):
    """Applied object changes and native snapshot outcomes."""

    changes: tuple[ObjectChange, ...]
    applied: tuple[Applied, ...]


class SubdivisionLevels(msgspec.Struct, frozen=True, kw_only=True):
    """Viewport and render levels supplied by native subdivision metadata."""

    native: ClassVar[dict[str, bpy.types.IntProperty]] = {prop.identifier: prop for prop in bpy.types.SubsurfModifier.bl_rna.properties if isinstance(prop, bpy.types.IntProperty)}
    viewport: ClassVar[bpy.types.IntProperty] = native["levels"]
    render: ClassVar[bpy.types.IntProperty] = native["render_levels"]
    levels: Annotated[
        int,
        msgspec.Meta(ge=viewport.hard_min, le=viewport.hard_max, title=viewport.name, description=viewport.description),
        Presentation(soft=Some((float(viewport.soft_min), float(viewport.soft_max)))),
    ] = viewport.default
    render_levels: Annotated[
        int, msgspec.Meta(ge=render.hard_min, le=render.hard_max, title=render.name, description=render.description), Presentation(soft=Some((float(render.soft_min), float(render.soft_max))))
    ] = render.default


class LevelEdit(msgspec.Struct, frozen=True, kw_only=True):
    """Subdivision levels and the objects receiving them."""

    levels: Annotated[SubdivisionLevels, msgspec.Meta(title="Levels", description="Viewport and render subdivision levels")] = SubdivisionLevels()
    scope: Annotated[Scope, msgspec.Meta(title="Scope", description="Objects whose modifiers take the levels")] = Selection()


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [READS]


def descendants(item: bpy.types.Object, view_layer: bpy.types.ViewLayer) -> tuple[bpy.types.Object, ...]:
    """Returns descendants in preorder from one parent map over the view layer."""
    children: dict[bpy.types.Object, list[bpy.types.Object]] = {}
    for child in view_layer.objects:
        if (parent := child.parent) is not None:
            children.setdefault(parent, []).append(child)
    return tuple(Block.unfold(lambda stack: Some((stack[0], (*children.get(stack[0], ()), *stack[1:]))) if stack else Nothing, children.get(item, ())))


def extent(items: Iterable[bpy.types.Object], depsgraph: bpy.types.Depsgraph) -> Option[WorldBounds]:
    """Returns evaluated world bounds, or Nothing when every object lacks bounds."""
    corners = tuple(
        evaluated.matrix_world @ Vector(corner)
        for evaluated in (item.evaluated_get(depsgraph) for item in items)
        if any(any(corner) for corner in evaluated.bound_box)
        for corner in evaluated.bound_box
    )
    return Some(WorldBounds(low=Vector(tuple(map(min, zip(*corners)))).freeze(), high=Vector(tuple(map(max, zip(*corners)))).freeze())) if corners else Nothing  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


# --- [CREATION]


def spawned(name: str, data: Option[bpy.types.ID], collection: bpy.types.Collection) -> BlenderResult[bpy.types.Object]:
    """Creates and joins an object, releasing it when native membership is refused."""

    def published(_: object) -> bpy.types.Object:
        scope.pop_all()
        return item

    with ExitStack() as scope:
        try:
            item = bpy.data.objects.new(name, data.to_optional())
        except (ReferenceError, RuntimeError, TypeError, ValueError) as error:
            return Error((BlenderEditFault(request=name, error=error),))
        scope.callback(bpy.data.objects.remove, item, do_unlink=True)
        return joined(collection, (item,)).map(published)


# --- [STACKS]


def restacked(stack: Stack, entries: Entries) -> BlenderResult[tuple[Restacked, ...]]:
    """Plans a desired stack through its native creation choices and move capability."""
    functions = stack.bl_rna.functions  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    inputs = tuple(parameter for parameter in functions["new"].parameters if not parameter.is_output)
    choices = {parameter.identifier: tuple(item.identifier for item in parameter.enum_items) for parameter in inputs if isinstance(parameter, bpy.types.EnumProperty)}
    live = tuple((entry.name, {key: getattr(entry, key) for key in choices}) for entry in stack)
    wanted = tuple((name, {"type": entry.kind} if isinstance(entry, StackEntry) else {}) for name, entry in entries.items())
    prefix = next((index for index, (was, now) in enumerate(zip(live, wanted)) if was != now), min(len(live), len(wanted)))
    kept = live if "move" in functions else live[:prefix]
    trees = tuple(Tree(roots=tuple(key for key, _ in pairs), owners={key: Owner(values=values) for key, values in pairs}) for pairs in (live, kept, wanted))
    edits = (*diff(trees[0], trees[1]), *diff(trees[1], trees[2]))
    return accumulated(
        Ok(value) if value in choices[key] else Error((BlenderChoiceFault(path=repr(stack), identifier=value, accepted=choices[key]),)) for _, parameters in wanted for key, value in parameters.items()
    ).map(lambda _: (Restacked(stack=stack, edits=edits, wanted=wanted),) if edits else ())


def restack(row: Restacked) -> tuple[Step, ...]:
    """Constructs removals and target-ordered native insertions and moves."""
    stack, functions, parameters = row.stack, row.stack.bl_rna.functions, dict(row.wanted)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    named = any(parameter.identifier == "name" for parameter in functions["new"].parameters)

    def removed(key: str) -> Step:
        return lambda: catch(lambda: stack.remove(stack[key]), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(stack), error=error),))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    def added(key: str) -> Step:
        def inserted() -> BlenderResult[None]:
            try:
                entry = stack.new(**({"name": key} if named else {}), **parameters[key])  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            except RuntimeError as error:
                return Error((BlenderEditFault(request=repr(stack), error=error),))
            return Result.bind(
                Option.of_optional(entry).to_result_with(lambda: (BlenderAbsentFault(owner=repr(stack), key=key),)), lambda made: Ok(None) if named else written(made, "name", key).map(lambda _: None)
            )

        return inserted

    def moved(stack: bpy.types.ObjectModifiers | bpy.types.ObjectConstraints, before: int, index: int) -> BlenderResult[None]:
        return catch(lambda: stack.move(before, index), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(stack), error=error),))

    leaving = tuple(edit.key for edit in row.edits if isinstance(edit, (Deleted, Reparametrized)))
    order = [entry.name for entry in stack if entry.name not in leaving]
    planned = [removed(key) for key in leaving]
    for index, (key, _) in enumerate(row.wanted):
        if key not in order:
            planned.append(added(key))
            order.append(key)
        if (before := order.index(key)) != index:
            if isinstance(stack, bpy.types.ObjectModifiers | bpy.types.ObjectConstraints):
                planned.append(partial(moved, stack, before, index))
            order.insert(index, order.pop(before))
    return tuple(planned)


# --- [CHANGES]


def diverged(state: ObjectState, item: bpy.types.Object, layers: tuple[bpy.types.ViewLayer, ...]) -> BlenderResult[tuple[ObjectChange, ...]]:
    """Returns differing concerns, accumulating independent planning refusals."""
    editing = Setting(descriptor=Descriptor(root=bpy.types.Object, owner="", prop=bpy.types.Object.bl_rna.properties["mode"]), accepted=("EDIT",))
    unchanged = Ok(())

    def swapped(block: bpy.types.ID) -> BlenderResult[tuple[ObjectChange, ...]]:
        return unchanged if item.data == block else Error((BlenderUnmetFault(requirement=Not(condition=editing)),)) if editing.descriptor.read(item) in editing.accepted else Ok((Swapped(data=block),))

    def parented(parent: Handle[bpy.types.Object]) -> BlenderResult[tuple[ObjectChange, ...]]:
        return parent.resolve().bind(
            lambda block: (
                Ok(())
                if item.parent == block
                else Error((BlenderChoiceFault(path=f"{item!r}.parent", identifier=block.name, accepted=()),))
                if item in {block, *Block.unfold(lambda above: Some((up, up)) if (up := above.parent) is not None else Nothing, block)}
                else Ok((Parented(parent=block),))
            )
        )

    def resized(data: Option[bpy.types.ID], slots: tuple[Snapshot, ...]) -> BlenderResult[tuple[ObjectChange, ...]]:
        return (
            Result[bpy.types.ID, BlenderFaults]
            .bind(
                data.to_result_with(lambda: (BlenderAbsentFault(owner=repr(item), key="data.materials"),)),
                lambda block: Descriptor.resolve(block, "materials", bpy.types.CollectionProperty).map(lambda member: getattr(block, member.prop.identifier)),
            )
            .map(lambda materials: (Resized(materials=materials, before=before, after=after),) if (before := len(materials)) != (after := len(slots)) else ())
        )

    def layered(layer: bpy.types.ViewLayer) -> tuple[ObjectChange, ...]:
        hidden = state.hidden.filter(lambda hide: item.hide_get(view_layer=layer) != hide)
        selected = state.selected.filter(lambda select: item.select_get(view_layer=layer) != select)
        return (Layered(view_layer=layer, hidden=hidden, selected=selected),) if hidden.is_some() or selected.is_some() else ()

    return accumulated((
        state.data
        .map(lambda handle: handle.resolve().map(Some))
        .default_with(lambda: Ok(Option.of_optional(item.data)))
        .bind(
            lambda data: paired(state.data.bind(lambda _: data).map(swapped).default_value(unchanged), state.material_slots.map(lambda slots: resized(data, slots)).default_value(unchanged)).map(
                lambda pair: (*pair[0], *pair[1])
            )
        ),
        *state.parent.map(parented).to_list(),
        Ok(tuple(chain.from_iterable(layered(layer) for layer in layers))),
        *(restacked(stack, entries) for stack, wanted in state.stacks(item) for entries in wanted.to_list()),
    )).map(lambda parts: tuple(chain.from_iterable(parts)))


def steps(item: bpy.types.Object, change: ObjectChange) -> tuple[Step, ...]:
    """Returns each change's dependent host writes."""
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

            return (lambda: catch(resized, exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(item), error=error),)),)
        case Layered(view_layer=layer, hidden=hidden, selected=selected):
            return (
                lambda: accumulated(
                    result.map_error(lambda error: (BlenderEditFault(request=repr(item), error=error),))
                    for result in (
                        *(catch(lambda: item.hide_set(hide, view_layer=layer), exception=RuntimeError)() for hide in hidden.to_list()),
                        *(catch(lambda: item.select_set(select, view_layer=layer), exception=RuntimeError)() for select in selected.to_list()),
                    )
                ).map(lambda _: None),
            )
        case Restacked():
            return restack(change)


def reconciled(state: ObjectState, item: bpy.types.Object, layers: tuple[bpy.types.ViewLayer, ...]) -> BlenderResult[Reconciled]:
    """Reconciles structure, then independent value sets, then world placement."""

    def applied(rows: tuple[ObjectChange, ...]) -> BlenderResult[Reconciled]:
        pairs = chain(
            ((item, values) for values in state.values.to_list()),
            (pair for slots in state.material_slots.to_list() for pair in zip(item.material_slots, slots)),
            (
                (stack[name], values)
                for stack, wanted in state.stacks(item)
                for entries in wanted.to_list()
                for name, entry in entries.items()
                for values in (entry.values if isinstance(entry, StackEntry) else entry).to_list()
            ),
        )
        return accumulated(applied.outcome.map(lambda _: applied) for owner, values in pairs for applied in (values.apply(owner),)).map(lambda counts: Reconciled(changes=rows, applied=counts))

    def placed(done: Reconciled) -> BlenderResult[Reconciled]:
        for matrix in state.placement.to_list():
            try:
                item.matrix_world = matrix
            except (ReferenceError, RuntimeError, TypeError, ValueError) as error:
                return Error((BlenderEditFault(request=f"{item!r}.matrix_world", error=error),))
        return Ok(done)

    return (
        diverged(state, item, layers)
        .bind(lambda rows: reduce(lambda done, step: done.bind(lambda _: step()), chain.from_iterable(steps(item, row) for row in rows), Ok(None)).map(lambda _: rows))
        .bind(applied)
        .bind(placed)
    )


# --- [SCOPES]


def targets(scope: Scope, context: bpy.types.Context) -> BlenderResult[tuple[tuple[bpy.types.Object, tuple[bpy.types.ViewLayer, ...]], ...]]:
    """Returns each reached object once and every view layer reaching it."""

    def reaching(layers: tuple[bpy.types.ViewLayer, ...]) -> tuple[tuple[bpy.types.Object, tuple[bpy.types.ViewLayer, ...]], ...]:
        owners: dict[bpy.types.Object, list[bpy.types.ViewLayer]] = {}
        for layer in layers:
            for item in layer.objects:
                owners.setdefault(item, []).append(layer)
        return tuple((item, tuple(held)) for item, held in owners.items())

    match scope:
        case Scenes(names=names) | ViewLayers(scenes=names):
            return ViewLayers(scenes=names).owners(context).map(reaching)
        case Selection() | Objects():
            return paired(scope.owners(context), Option.of_optional(context.view_layer).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="view_layer"),))).map(
                lambda pair: tuple((item, (pair[1],)) for item in pair[0])
            )


def fanned(state: ObjectState, scope: Scope, context: bpy.types.Context) -> BlenderResult[tuple[Reconciled, ...]]:
    """Reconciles a scope, accumulating every independent object's refusals."""
    return targets(scope, context).bind(lambda reached: accumulated(reconciled(state, item, layers) for item, layers in reached))


def adjusted(snapshots: tuple[Snapshot, ...], stack: Callable[[bpy.types.Object], Stack], scope: Scope, context: bpy.types.Context) -> BlenderResult[Done]:
    """Applies snapshots to matching native stack entries while retaining partial undo."""
    return targets(scope, context).map(
        lambda reached: Done(
            skipped=accumulated(
                snapshot.apply(entry).outcome
                for item, _ in reached
                for entry in stack(item)
                for snapshot in snapshots
                if entry.bl_rna.identifier == snapshot.struct  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            )
            .swap()
            .to_option()
        )
    )


def levelled(levels: SubdivisionLevels, scope: Scope, context: bpy.types.Context) -> BlenderResult[Done]:
    """Writes subdivision levels through each modifier's native range function."""
    return adjusted(tuple(Snapshot.of(kind, msgspec.structs.asdict(levels)) for kind in (bpy.types.SubsurfModifier, bpy.types.MultiresModifier)), lambda item: item.modifiers, scope, context)


# --- [COMPOSITION] ----------------------------------------------------------------------

subdivision: Operator[LevelEdit] = Operator(
    key="subdivision_levels",
    title="Set Subdivision Levels",
    description="Writes viewport and render subdivision levels across a scope",
    form=LevelEdit,
    run=lambda call: levelled(call.record.levels, call.record.scope, call.context),
    presets=True,
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Entries",
    "Layered",
    "LevelEdit",
    "ObjectChange",
    "ObjectState",
    "Parented",
    "Reconciled",
    "Resized",
    "Restacked",
    "Stack",
    "StackEntry",
    "Step",
    "SubdivisionLevels",
    "Swapped",
    "WorldBounds",
    "adjusted",
    "descendants",
    "diverged",
    "extent",
    "fanned",
    "levelled",
    "reconciled",
    "restack",
    "restacked",
    "spawned",
    "steps",
    "subdivision",
    "targets",
]
