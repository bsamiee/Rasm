"""Light rig data and placement: the setup attachment, owned rows, rig state, the rig a command resolves, and lights placed about a target with a background world."""

from collections.abc import Mapping
from contextlib import ExitStack
from functools import partial
from typing import Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
from mathutils import Matrix, Vector
import msgspec
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.data.ids.evaluation import evaluated
from blender.bpy.data.ids.images import lighting
from blender.bpy.data.ids.ownership import Mark, Ownership, released, retained, Saved
from blender.bpy.data.ids.store import Handle, IdName
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.mirrors import choices, reconciling
from blender.bpy.data.rna.paths import RnaPath, rooted
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.bpy.scene.collections import founded
from blender.bpy.scene.lights.emitters import TEMPERATURE
from blender.bpy.scene.lights.linking import LINK_STATE, linked, RECEIVERS
from blender.bpy.scene.objects import descendants, extent, focused, ObjectState, reconciled, spawned, StackEntry, WorldBounds
from blender.bpy.scene.worlds import reverted, swapped
from blender.bpy.settings.attachments import Attached, Attachment, Stored
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.imaging.analysis.lighting import LightingReading
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason, typed
from blender.records.fields import Positive
from blender.rendering.lighting.references import ReferenceImage
from blender.rendering.lighting.rigs import Arranged, Disk, PlacedLight, PlacedRig, Point, Rig, Spot, Sun

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

_TYPE: Final = Descriptor.declared(bpy.types.Light, ("type",), bpy.types.EnumProperty)

_FOCI: Final = choices((
    ("BOUNDS", "Bounds", "Midpoint of evaluated bounds of target and descendants"),
    ("ORIGIN", "Origin", "Target origin"),
    ("BONE", "Bone", "Pose bone head of armature target"),
    ("CURSOR", "3D Cursor", "3D cursor location"),
))
_AXES: Final = choices((
    ("CAMERA", "Camera", "Scene camera axes"),
    ("VIEW", "View", "3D view axes"),
    ("TARGET", "Target", "Target object axes"),
    ("WORLD", "World", "World +X right, +Z up, and -Y back"),
))
EMPTY: Final = Ownership(kind=bpy.types.Object, key="rig", saved=Saved.USED)
LIGHTS: Final = Ownership(kind=bpy.types.Object, key="rig_light", saved=Saved.USED)
DATA: Final = Ownership(kind=bpy.types.Light, key="rig", saved=Saved.USED, single=False)
COLLECTION: Final = Ownership(kind=bpy.types.Collection, key="rig", saved=Saved.USED)
WORLD: Final = Ownership(kind=bpy.types.World, key="rig", saved=Saved.USED)
REFERENCE: Final = Ownership(kind=bpy.types.Image, key="rig_reference", saved=Saved.USED)

# --- [MODELS] ---------------------------------------------------------------------------


class RigSetup(bpy.types.PropertyGroup):
    """Focus, axes, linking, and reference of one rig empty."""

    focus: bpy.props.EnumProperty(name="Focus", description="Rig center", items=_FOCI, default="BOUNDS")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    bone: bpy.props.StringProperty(name="Bone", description="Pose bone centering a bone focus")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    axes: bpy.props.EnumProperty(name="Axes", description="Azimuth and elevation axes", items=_AXES, default="CAMERA")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    linked: bpy.props.BoolProperty(name="Target Alone", description="Lights reach target and descendants alone")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    reference: bpy.props.PointerProperty(type=bpy.types.Image, name="Reference", description="Image a matched rig reads")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    size: bpy.props.IntProperty(name="Reference Size", description="Side of the rendered reference image", min=1, default=256)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class RigState(msgspec.Struct, frozen=True, kw_only=True):
    """Rig and reference recipe one rig empty stores beside its setup."""

    rig: Rig = Rig(layout=Arranged())
    recipe: ReferenceImage = ReferenceImage()


class RigFrame(msgspec.Struct, frozen=True, kw_only=True):
    """Rig world matrix with right, up, back, and center columns, and bounding radius."""

    matrix: Matrix
    radius: Positive


class Placing(msgspec.Struct, frozen=True, kw_only=True):
    """Rig frame, lights with powers, and background environment members by path."""

    frame: RigFrame
    lights: tuple[tuple[PlacedLight, float], ...]
    background: Option[Mapping[RnaPath, Value]]


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [RIGS]


def served(identity: Identity, item: bpy.types.Object) -> Outcome[bpy.types.Object]:
    """Returns rig empty target, or item when it is no rig empty."""
    return (
        EMPTY
        .marked(identity, item)
        .map(lambda mark: mark.of.map(partial(typed, bpy.types.Object, f"{item!r}.target")).default_with(lambda: Error(Fault.of(BlenderReason.ABSENT, repr(item), member="target"))))
        .default_value(Ok(item))
    )


def resolved(view_layer: bpy.types.ViewLayer, identity: Identity) -> Outcome[bpy.types.Object]:
    """Returns the object holding the rig state every rig command reads: the active rig empty, else the rig empty serving the active object, else the active object."""
    return focused(view_layer).map(lambda item: EMPTY.marked(identity, item).map(lambda _: item).or_else_with(lambda: EMPTY.found(identity, of=Some(item))).default_value(item))


def replaced(rig: Handle[bpy.types.Object], identity: Identity) -> Outcome[Option[str]]:
    """Places rig again from stored setup.

    Args:
        rig: Handle of rig empty.
        identity: Identity binding attachment and marking owned data.

    Returns:
        `Nothing` naming no undo step, or `absent`, `register`, `read`, and `rigged` faults.
    """
    context = bpy.context
    return rig.resolve().bind(
        lambda empty: (
            EMPTY.marked(identity, empty).map(lambda _: served(identity, empty).bind(lambda holder: rigged(holder, Some(empty), context, identity)).map(lambda _: Nothing)).default_value(Ok(Nothing))
        )
    )


def states(identity: Identity, holder: bpy.types.Object) -> Outcome[tuple[Stored[RigState], RigState]]:
    """Returns the rig state store and the state holder keeps, defaults while it holds none."""
    return Stored.keyed(identity, "rig_state", msgspec.msgpack.Decoder(RigState)).bind(lambda store: store.read(holder).map(lambda found: (store, found.default_with(RigState))))


# --- [PLACEMENT]


def target(holder: bpy.types.Object, setup: RigSetup, context: bpy.types.Context, scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[RigFrame]:
    """Returns rig world frame and bounding radius.

    Args:
        holder: Target object.
        setup: Focus and axes choices.
        context: Context supplying window manager for view axes.
        scene: Scene supplying camera and cursor.
        layer: View layer evaluating target and descendants.

    Returns:
        Frame, or `absent` for missing bounds, bone, camera, or view, `choice` for a non-3D view, `read` for zero radius.
    """

    def centered(bounds: WorldBounds) -> Outcome[Vector]:
        match setup.focus:
            case "ORIGIN":
                return Ok(holder.matrix_world.translation.copy())
            case "BONE":
                pose: Option[bpy.types.PoseBone] = Option.of_optional(holder.pose).bind(lambda pose: Option.of_optional(pose.bones.get(setup.bone)))
                return pose.map(lambda pose: holder.matrix_world @ pose.head).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(holder), member=setup.bone))
            case "CURSOR":
                return Ok(scene.cursor.location.copy())
            case _:
                return Ok((bounds.low + bounds.high) / 2.0)

    def framed() -> Outcome[Matrix]:
        world = Matrix(((1.0, 0.0, 0.0), (0.0, 0.0, -1.0), (0.0, 1.0, 0.0)))

        def oriented(found: Located) -> Outcome[Matrix]:
            match found.area.spaces.active:
                case bpy.types.SpaceView3D(region_3d=bpy.types.RegionView3D(view_rotation=rotation)):
                    return Ok(rotation.to_matrix())
                case space:
                    return Error(Fault.of(RecordReason.OUTSIDE, f"{found.area!r}.spaces.active.region_3d", type(space).__name__, member=bpy.types.SpaceView3D.__name__))

        match setup.axes:
            case "CAMERA":
                return (
                    Option.of_optional(scene.camera).map(lambda camera: camera.matrix_world.to_3x3().normalized()).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(scene), member="camera"))
                )
            case "VIEW":
                return rooted(bpy.types.WindowManager, context).bind(lambda manager: AreaQuery(space=bpy.types.SpaceView3D).first(manager.windows)).bind(oriented)
            case "TARGET":
                return Ok(holder.matrix_world.to_3x3().normalized() @ world)
            case _:
                return Ok(world)

    def measured(carried: tuple[WorldBounds, Vector, Matrix]) -> Outcome[RigFrame]:
        bounds, center, frame = carried
        reach = Vector(tuple(max(abs(low), abs(high)) for low, high in zip((bounds.low - center)[:], (bounds.high - center)[:], strict=True)))
        radius: Outcome[float] = catch(msgspec.convert, exception=msgspec.ValidationError)(reach.length, type=Positive).map_error(
            lambda error: Fault.of(RecordReason.OUTSIDE, f"{holder!r}.bound_box", str(bounds), member=str(error))
        )
        return radius.map(lambda held: RigFrame(matrix=Matrix.LocRotScale(center, frame, None).freeze(), radius=held))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    return (
        evaluated(layer)
        .bind(lambda depsgraph: extent((holder, *descendants(holder, layer)), depsgraph).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(holder), member="bound_box")))
        .bind(lambda bounds: paired(centered(bounds), framed()).map(lambda pair: (bounds, *pair)))
        .bind(measured)
    )


def fitted(holder: bpy.types.Object, rig: bpy.types.Object, frame: RigFrame, context: bpy.types.Context) -> Outcome[None]:
    """Writes rig empty placement, display, and target constraint, its inverse set by `constraint.childof_set_inverse`.

    Args:
        holder: Target object.
        rig: Owned rig empty.
        frame: World frame and radius from `target`.
        context: Context the inverse operator runs under with the rig as object.

    Returns:
        `Ok` once written, or write and operator faults.
    """
    display = Snapshot.of(bpy.types.Object, {("empty_display_type",): "SPHERE", ("empty_display_size",): frame.radius})
    follow = Snapshot.of(bpy.types.ChildOfConstraint, {("target",): IdName.of(holder)})
    return reconciled(ObjectState(values=Some(display), placement=Some(frame.matrix), constraints=Some({"Follow": StackEntry(kind="CHILD_OF", values=Some(follow))})), rig, ()).bind(
        lambda _: called(bpy.ops.constraint.childof_set_inverse, {"constraint": "Follow", "owner": "OBJECT"}, override=Some(context.temp_override(object=rig))).map(lambda _: None)
    )


def rigged(holder: bpy.types.Object, existing: Option[bpy.types.Object], context: bpy.types.Context, identity: Identity) -> Outcome[tuple[bpy.types.Object, ...]]:
    """Places rig about target, rolling back created data and world swap on fault.

    Args:
        holder: Target object.
        existing: Rig empty to place again from stored setup, `Nothing` to build one from target setup.
        context: Context supplying scene, view layer, and window manager for view axes.
        identity: Identity binding attachment and marking owned data.

    Returns:
        Lights in placement order, or `absent`, setup read, `target`, reference, placement, and write faults.
    """

    def placing(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, config: PyOpenColorIO.Config, rollback: ExitStack) -> Outcome[tuple[bpy.types.Object, ...]]:
        def measured(setup: RigSetup, state: RigState) -> Outcome[Placing]:
            def prepared(found: tuple[RigFrame, Option[LightingReading]]) -> Outcome[Placing]:
                frame, measurement = found
                return (
                    Result[Option[PlacedRig], Faults]
                    .bind(
                        state.rig.placements(config, frame.radius, measurement), lambda placements: placements.to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(setup), member="reference"))
                    )
                    .bind(
                        lambda placements: accumulated(placement.power for placement in placements.lights).map(
                            lambda powers: Placing(
                                frame=frame,
                                lights=tuple(zip(placements.lights, powers)),
                                background=placements.background.map(
                                    lambda background: {("lighting", "mode"): "COLOR", ("lighting", "color"): background.color, ("lighting", "strength"): background.strength}
                                ),
                            )
                        )
                    )
                )

            return paired(target(holder, setup, context, scene, layer), lighting(setup.reference, config)).bind(prepared)

        def lights(rig: bpy.types.Object, home: bpy.types.Collection, placing: Placing, *, alone: bool) -> Outcome[tuple[bpy.types.Object, ...]]:
            aim = Snapshot.of(bpy.types.TrackToConstraint, {("target",): IdName.of(rig), ("track_axis",): "TRACK_NEGATIVE_Z", ("up_axis",): "UP_Y"})
            count = len(placing.lights)
            stale = tuple(block for block, mark in LIGHTS.held(identity) if mark.of == Some(rig) and mark.slot.filter(lambda slot: slot < count).is_none())

            def emitted(placement: PlacedLight, power: float) -> tuple["rna_enums.LightTypeItems", dict[RnaPath, Value]]:
                light = placement.light
                temperature: dict[RnaPath, Value] = {TEMPERATURE.path: light.temperature} if light.temperature is not None else {}
                shared: dict[RnaPath, Value] = {("energy",): power, ("color",): light.color, ("use_temperature",): light.temperature is not None, **temperature}
                match light.emitter:
                    case Point():
                        return "POINT", {("shadow_soft_size",): placement.radius, **shared}
                    case Spot(cone=cone, blend=blend):
                        return "SPOT", {("shadow_soft_size",): placement.radius, ("spot_size",): cone, ("spot_blend",): blend, **shared}
                    case Disk():
                        return "AREA", {("shape",): "DISK", ("size",): 2.0 * placement.radius, **shared}
                    case Sun():
                        return "SUN", {("angle",): light.size, **shared}

            def spawned_light(slot: int, placement: PlacedLight, power: float) -> Outcome[bpy.types.Object]:
                kind, values = emitted(placement, power)
                label = f"{holder.name} {bpy.path.display_name(placement.light.role.name.lower())}"

                def stated(item: bpy.types.Object) -> ObjectState:
                    return ObjectState(
                        parent=Some(Handle.of(rig)),
                        values=Some(Snapshot.of(bpy.types.Object, {("lightgroup",): item.name})),
                        placement=Some(Matrix.Translation(placement.location)),
                        constraints=Some({"Aim": StackEntry(kind="TRACK_TO", values=Some(aim))}),
                    )

                def built() -> Outcome[bpy.types.Object]:
                    return (
                        Result[bpy.types.Light, Faults]
                        .bind(
                            catch(bpy.data.lights.new, exception=RuntimeError)(label, kind).map_error(partial(Fault.of, BlenderReason.REFUSED, label)),
                            lambda light: DATA.owned(identity, light, of=Some(rig), scope=Some(rollback)),
                        )
                        .bind(lambda light: spawned(label, Some(light), home))
                    )

                def configured(item: bpy.types.Object) -> Outcome[bpy.types.Object]:
                    return typed(bpy.types.Light, f"{item!r}.data", item.data).bind(
                        lambda light: _TYPE.write(light, kind).bind(lambda _: reconciled(stated(item), item, ())).bind(lambda _: assigned(light.type_recast(), values)).map(lambda _: item)
                    )

                return LIGHTS.made(identity, built, of=Some(rig), slot=Some(slot), scope=Some(rollback)).bind(configured)

            def joined_to(prepared: tuple[tuple[bpy.types.Object, ...], None]) -> Outcome[tuple[bpy.types.Object, ...]]:
                lights, _ = prepared
                return (
                    accumulated(linked(item, RECEIVERS, (holder, *descendants(holder, layer)), LINK_STATE.prop.default, holder, context, identity) for item in lights).map(lambda _: None)
                    if alone
                    else accumulated(RECEIVERS.write(item.light_linking, None) for item in lights).map(lambda _: None)
                ).map(lambda _: lights)

            backed = placing.background.map(lambda prepared: swapped(WORLD, prepared, scene, identity, Some(rollback)).map(lambda _: None)).default_with(
                lambda: reverted(WORLD, scene, identity).map(lambda _: None)
            )
            return (
                paired(accumulated(spawned_light(slot, placement, power) for slot, (placement, power) in enumerate(placing.lights)), backed)
                .bind(joined_to)
                .bind(lambda lit: released(stale).bind(lambda _: DATA.removed(identity)).map(lambda _: lit))
            )

        def placed(attached: Attached[RigSetup], store: Stored[RigState], state: RigState, placing: Placing) -> Outcome[tuple[bpy.types.Object, ...]]:
            setup = attached.struct(source)

            def copied(rig: bpy.types.Object) -> Outcome[bpy.types.Object]:
                return Snapshot.capture(setup).bind(lambda held: held.apply(attached.struct(rig)).outcome).bind(lambda _: store.write(rig, state)).map(lambda _: rig)

            def empty(home: bpy.types.Collection) -> Outcome[bpy.types.Object]:
                return existing.map(Ok).default_with(
                    lambda: spawned(f"{holder.name} Rig", Nothing, home).bind(lambda rig: EMPTY.owned(identity, rig, of=Some(holder), filled=copied, scope=Some(rollback)))
                )

            return COLLECTION.made(identity, lambda: founded(Some(scene.collection), f"{holder.name} Rig"), of=Some(holder), scope=Some(rollback)).bind(
                lambda home: empty(home).bind(lambda rig: fitted(holder, rig, placing.frame, context).bind(lambda _: lights(rig, home, placing, alone=setup.linked)))
            )

        source = existing.default_value(holder)
        return paired(SETUP.bound(identity), states(identity, source)).bind(lambda found: measured(found[0].struct(source), found[1][1]).bind(lambda planned: placed(found[0], *found[1], planned)))

    with ExitStack() as rollback:
        return retained(paired(rooted(bpy.types.Scene, context), rooted(bpy.types.ViewLayer, context)).bind(lambda found: placing(*found, PyOpenColorIO.GetCurrentConfig(), rollback)), rollback)


def rig_removed(rig: bpy.types.Object, identity: Identity, scene: bpy.types.Scene) -> Outcome[int]:
    """Removes rig lights, empty, and collection, restoring world it replaced.

    Args:
        rig: Owned rig empty.
        identity: Identity reading marks.
        scene: Scene with world to restore.

    Returns:
        Removed data-block count, or `edit` faults.
    """
    blocks = (*LIGHTS.serving(identity, rig), rig, *(held for holder in Mark.read(identity, rig).bind(lambda mark: mark.of).to_list() for held in COLLECTION.serving(identity, holder)))
    return (
        reverted(WORLD, scene, identity).bind(lambda _: released(blocks)).bind(lambda count: accumulated((DATA.removed(identity), REFERENCE.removed(identity))).map(lambda counts: count + sum(counts)))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

SETUP: Final[Attachment[RigSetup]] = Attachment(key="rig", form=RigSetup, owner=bpy.types.Object, changed=Some(partial(reconciling, bpy.types.Object, replaced)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "COLLECTION",
    "DATA",
    "EMPTY",
    "LIGHTS",
    "REFERENCE",
    "SETUP",
    "WORLD",
    "RigFrame",
    "RigSetup",
    "RigState",
    "fitted",
    "replaced",
    "resolved",
    "rig_removed",
    "rigged",
    "served",
    "states",
    "target",
]
