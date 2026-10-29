"""Light rig feature: setup and state, lights placed about a target with a background world, the reference image a matched rig reads, sections, and operators."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import partial
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
from mathutils import Matrix, Vector
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.ids.evaluation import evaluated
from blender.bpy.data.ids.images import lighting, Tag
from blender.bpy.data.ids.ownership import Mark, Ownership, released, retained, Saved
from blender.bpy.data.ids.store import Handle, IdName
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.mirrors import choices, enum_choices, reconciling
from blender.bpy.data.rna.paths import RnaPath, rooted
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.events import Handler, updated
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.content import Generated
from blender.bpy.scene.collections import founded
from blender.bpy.scene.lights.emitters import TEMPERATURE
from blender.bpy.scene.lights.linking import LINK_STATE, linked, RECEIVERS
from blender.bpy.scene.objects import descendants, extent, focused, ObjectState, owned_object, reconciled, spawned, StackEntry
from blender.bpy.scene.worlds import reverted, swapped
from blender.bpy.settings.attachments import Attached, Attachment, Stored
from blender.bpy.ui.layout.rows import FUNCTIONS, Member, OperatorButton, Property
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.imaging.analysis.lighting import LightingReading
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason, typed
from blender.records.fields import Positive, Retention
from blender.records.stores import content_hash
from blender.rendering.lighting.references import Gel, ReferenceDistribution, ReferenceImage, REFERENCES
from blender.rendering.lighting.rigs import Arranged, Disk, PlacedLight, PlacedRig, Point, Rig, RIGS, Spot, Sun

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


class RigPick(bpy.types.PropertyGroup):
    """Catalog rig name."""

    name: bpy.props.EnumProperty(name="Rig", description="Catalog rig", items=choices((name, name, "") for name in RIGS))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class ReferencePick(bpy.types.PropertyGroup):
    """Reference recipe name from catalog."""

    name: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Reference", items=choices((name, name, bpy.path.display_name(family.name.lower())) for name, (family, _) in REFERENCES.items())
    )


class ReferenceDraw(bpy.types.PropertyGroup):
    """Reference seed and color distribution."""

    distribution: bpy.props.EnumProperty(name="Distribution", items=enum_choices(ReferenceDistribution), default=ReferenceDistribution.BALANCED.name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    seed: bpy.props.IntProperty(name="Seed", min=0, max=2**24)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class GelPick(bpy.types.PropertyGroup):
    """Gel name for reference tint."""

    name: bpy.props.EnumProperty(name="Gel", items=enum_choices(Gel))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LightRig(msgspec.Struct, frozen=True, kw_only=True):
    """Rig setup attachment with owned empty, lights, collection, world, and reference image, and the rig and reference sections."""

    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing

    @staticmethod
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
                LightRig.empty
                .marked(identity, empty)
                .map(lambda _: served(identity, empty).bind(lambda holder: rigged(holder, Some(empty), context, identity)).map(lambda _: Nothing))
                .default_value(Ok(Nothing))
            )
        )

    setup: ClassVar[Attachment[RigSetup]] = Attachment(key="rig", form=RigSetup, owner=bpy.types.Object, changed=Some(partial(reconciling, bpy.types.Object, replaced)))
    empty: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="rig", saved=Saved.USED)
    lights: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="rig_light", saved=Saved.USED)
    data: ClassVar[Ownership[bpy.types.Light]] = Ownership(kind=bpy.types.Light, key="rig", saved=Saved.USED, single=False)
    collection: ClassVar[Ownership[bpy.types.Collection]] = Ownership(kind=bpy.types.Collection, key="rig", saved=Saved.USED)
    world: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key="rig", saved=Saved.USED)
    reference: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="rig_reference", saved=Saved.USED)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields attachment, ownerships, the handler placing rigs again when their reference image changes, and both sections.

        Args:
            identity: Identity of extension registering row.

        Yields:
            Acquisitions with `Band` pairs.
        """
        with identity.reports.deferred(LightRig.replaced) as deferred:

            def referenced(_: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                touched = frozenset(block.original for update in updates if (block := update.id) is not None)

                def placed(attached: Attached[RigSetup]) -> None:
                    for rig, _ in LightRig.empty.held(identity):
                        if attached.struct(rig).reference in touched:
                            deferred(Handle.of(rig), identity)

                return LightRig.setup.bound(identity).map(placed)

            yield Ok((
                LightRig.setup,
                LightRig.empty,
                LightRig.lights,
                LightRig.data,
                LightRig.collection,
                LightRig.world,
                LightRig.reference,
                Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Image,), referenced)),
                self.section(),
                self.references(),
            ))

    def section(self) -> Section[RigSetup]:
        """Returns active rig setup with the place, pick, arrange, and remove buttons in its header."""
        return Section(
            key="light_rig",
            title="Light Rig",
            header=tuple(OperatorButton(target=command) for command in (rig_place, rig_pick, rig_arrange, rig_remove)),
            store=Some(
                Store(
                    attachment=LightRig.setup,
                    host=Some(Member(root=bpy.types.LayerObjects, path=("active",))),
                    retention=self.retention,
                    fields={"reference": lambda member: Property(target=member, function=FUNCTIONS["template_ID_preview"], keywords={"open": "image.open"})},
                )
            ),
        )

    @staticmethod
    def references() -> Section:
        """Returns the reference pick, draw, gel, and apply buttons."""
        commands = (rig_reference_pick, rig_reference_draw, rig_reference_gel, rig_reference_apply)
        return Section(key="rig_reference", title="Rig Reference", rows=tuple(OperatorButton(target=command) for command in commands))


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [RIGS]


def served(identity: Identity, item: bpy.types.Object) -> Outcome[bpy.types.Object]:
    """Returns rig empty target, or item when it is no rig empty."""
    return (
        LightRig.empty
        .marked(identity, item)
        .map(lambda mark: mark.of.map(partial(typed, bpy.types.Object, f"{item!r}.target")).default_with(lambda: Error(Fault.of(Reason.ABSENT, repr(item), member="target"))))
        .default_value(Ok(item))
    )


def resolved(view_layer: bpy.types.ViewLayer, identity: Identity) -> Outcome[bpy.types.Object]:
    """Returns the object holding the rig state every rig command reads: the active rig empty, else the rig empty serving the active object, else the active object."""
    return focused(view_layer).map(lambda item: LightRig.empty.marked(identity, item).map(lambda _: item).or_else_with(lambda: LightRig.empty.found(identity, of=Some(item))).default_value(item))


def states(identity: Identity, holder: bpy.types.Object) -> Outcome[tuple[Stored[RigState], RigState]]:
    """Returns the rig state store and the state holder keeps, defaults while it holds none."""
    return Stored.keyed(identity, "rig_state", msgspec.msgpack.Decoder(RigState)).bind(lambda store: store.read(holder).map(lambda found: (store, found.default_with(RigState))))


def _restated[T: bpy.types.PropertyGroup](call: Call[T], change: Callable[[bpy.types.Object, RigSetup, RigState], Outcome[RigState]]) -> Outcome[Done]:
    """Writes the state a change derives from the resolved rig's setup and state."""
    return (
        rooted(bpy.types.ViewLayer, call.context)
        .bind(lambda layer: resolved(layer, call.identity))
        .bind(
            lambda rig: paired(LightRig.setup.read(call.identity, rig), states(call.identity, rig)).bind(lambda held: change(rig, held[0], held[1][1]).bind(lambda state: held[1][0].write(rig, state)))
        )
        .map(lambda _: Done())
    )


def _referenced(recipe: Callable[[RigState], ReferenceImage], holder: bpy.types.Object, setup: RigSetup, state: RigState, identity: Identity) -> Outcome[RigState]:
    """Renders the recipe into the rig's owned scene-linear reference image, removing unused reference images, returning the state holding the recipe."""
    chosen = recipe(state)
    config = PyOpenColorIO.GetCurrentConfig()
    return (
        Generated(
            ownership=LightRig.reference,
            content=content_hash(RigState, chosen, config.getCacheID().encode(), arrays=(np.asarray(setup.size, dtype=np.int64),)),
            name=f"{holder.name} Reference",
            plane=lambda: chosen.plane(setup.size).map(Some),
            tag=Tag(space=Some(PyOpenColorIO.ROLE_SCENE_LINEAR), view_as_render=Some(True)),
        )
        .made(identity, of=Some(holder))
        .bind(lambda image: written(setup, ("reference",), image.to_optional()))
        .bind(lambda _: LightRig.reference.removed(identity))
        .map(lambda _: msgspec.structs.replace(state, recipe=chosen))
    )


# --- [PLACEMENT]


def target(holder: bpy.types.Object, setup: RigSetup, context: bpy.types.Context, scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[tuple[Matrix, float]]:
    """Returns rig world matrix with right, up, back, and center columns, and bounding radius.

    Args:
        holder: Target object.
        setup: Focus and axes choices.
        context: Context supplying window manager for view axes.
        scene: Scene supplying camera and cursor.
        layer: View layer evaluating target and descendants.

    Returns:
        Matrix and radius, or `absent` for missing bounds, bone, camera, or view, `choice` for a non-3D view, `read` for zero radius.
    """

    def centered(low: Vector, high: Vector) -> Outcome[Vector]:
        match setup.focus:
            case "ORIGIN":
                return Ok(holder.matrix_world.translation.copy())
            case "BONE":
                pose: Option[bpy.types.PoseBone] = Option.of_optional(holder.pose).bind(lambda pose: Option.of_optional(pose.bones.get(setup.bone)))
                return pose.map(lambda pose: holder.matrix_world @ pose.head).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(holder), member=setup.bone))
            case "CURSOR":
                return Ok(scene.cursor.location.copy())
            case _:
                return Ok((low + high) / 2.0)

    def framed() -> Outcome[Matrix]:
        world = Matrix(((1.0, 0.0, 0.0), (0.0, 0.0, -1.0), (0.0, 1.0, 0.0)))

        def oriented(found: Located) -> Outcome[Matrix]:
            match found.area.spaces.active:
                case bpy.types.SpaceView3D(region_3d=bpy.types.RegionView3D(view_rotation=rotation)):
                    return Ok(rotation.to_matrix())
                case space:
                    return Error(Fault.of(Reason.OUTSIDE, f"{found.area!r}.spaces.active.region_3d", type(space).__name__, member=bpy.types.SpaceView3D.__name__))

        match setup.axes:
            case "CAMERA":
                return Option.of_optional(scene.camera).map(lambda camera: camera.matrix_world.to_3x3().normalized()).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(scene), member="camera"))
            case "VIEW":
                return rooted(bpy.types.WindowManager, context).bind(lambda manager: AreaQuery(space=bpy.types.SpaceView3D).first(manager.windows)).bind(oriented)
            case "TARGET":
                return Ok(holder.matrix_world.to_3x3().normalized() @ world)
            case _:
                return Ok(world)

    def measured(carried: tuple[tuple[Vector, Vector], Vector, Matrix]) -> Outcome[tuple[Matrix, float]]:
        (low, high), center, frame = carried
        reach = Vector(tuple(max(abs(lower), abs(upper)) for lower, upper in zip((low - center)[:], (high - center)[:], strict=True)))
        radius: Outcome[float] = catch(msgspec.convert, exception=msgspec.ValidationError)(reach.length, type=Positive).map_error(
            lambda error: Fault.of(Reason.OUTSIDE, f"{holder!r}.bound_box", f"{low[:]} to {high[:]}", member=str(error))
        )
        return radius.map(lambda held: (Matrix.LocRotScale(center, frame, None).freeze(), held))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    return (
        evaluated(layer)
        .bind(lambda depsgraph: extent((holder, *descendants(holder, layer)), depsgraph).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(holder), member="bound_box")))
        .bind(lambda bounds: paired(centered(*bounds), framed()).map(lambda pair: (bounds, *pair)))
        .bind(measured)
    )


def fitted(holder: bpy.types.Object, rig: bpy.types.Object, matrix: Matrix, radius: float, context: bpy.types.Context) -> Outcome[None]:
    """Writes rig empty placement, display, and target constraint, its inverse set by `constraint.childof_set_inverse`.

    Args:
        holder: Target object.
        rig: Owned rig empty.
        matrix: World matrix from `target`.
        radius: Bounding radius from `target`.
        context: Context the inverse operator runs under with the rig as object.

    Returns:
        `Ok` once written, or write and operator faults.
    """
    display = Snapshot.of(bpy.types.Object, {("empty_display_type",): "SPHERE", ("empty_display_size",): radius})
    follow = Snapshot.of(bpy.types.ChildOfConstraint, {("target",): IdName.of(holder)})
    return reconciled(ObjectState(values=Some(display), placement=Some(matrix), constraints=Some({"Follow": StackEntry(kind="CHILD_OF", values=Some(follow))})), rig, ()).bind(
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
        def measured(setup: RigSetup, state: RigState) -> Outcome[tuple[tuple[Matrix, float], PlacedRig]]:
            def prepared(found: tuple[tuple[Matrix, float], Option[LightingReading]]) -> Outcome[tuple[tuple[Matrix, float], PlacedRig]]:
                frame, measurement = found
                return (
                    Result[Option[PlacedRig], Faults]
                    .bind(state.rig.placements(config, frame[1], measurement), lambda placed: placed.to_result_with(lambda: Fault.of(Reason.ABSENT, repr(setup), member="reference")))
                    .map(lambda placed: (frame, placed))
                )

            return paired(target(holder, setup, context, scene, layer), lighting(setup.reference, config)).bind(prepared)

        def lights(rig: bpy.types.Object, home: bpy.types.Collection, placed: PlacedRig, *, alone: bool) -> Outcome[tuple[bpy.types.Object, ...]]:
            aim = Snapshot.of(bpy.types.TrackToConstraint, {("target",): IdName.of(rig), ("track_axis",): "TRACK_NEGATIVE_Z", ("up_axis",): "UP_Y"})
            count = len(placed.lights)
            stale = tuple(block for block, mark in LightRig.lights.held(identity) if mark.of == Some(rig) and mark.slot.filter(lambda slot: slot < count).is_none())

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

                def configured(item: bpy.types.Object, light: bpy.types.Light) -> Outcome[bpy.types.Object]:
                    state = ObjectState(
                        parent=Some(Handle.of(rig)),
                        values=Some(Snapshot.of(bpy.types.Object, {("lightgroup",): item.name})),
                        placement=Some(Matrix.Translation(placement.location)),
                        constraints=Some({"Aim": StackEntry(kind="TRACK_TO", values=Some(aim))}),
                    )
                    return _TYPE.write(light, kind).bind(lambda _: reconciled(state, item, ())).bind(lambda _: assigned(light.type_recast(), values)).map(lambda _: item)

                return owned_object((LightRig.lights, LightRig.data), label, partial(bpy.data.lights.new, label, kind), home, identity, of=Some(rig), slot=Some(slot), scope=Some(rollback)).bind(
                    lambda pair: configured(*pair)
                )

            def joined_to(lit: tuple[bpy.types.Object, ...]) -> Outcome[tuple[bpy.types.Object, ...]]:
                return (
                    accumulated(linked(item, RECEIVERS, (holder, *descendants(holder, layer)), LINK_STATE.prop.default, holder, context, identity) for item in lit).map(lambda _: None)
                    if alone
                    else accumulated(RECEIVERS.write(item.light_linking, None) for item in lit).map(lambda _: None)
                ).map(lambda _: lit)

            def backed() -> Outcome[None]:
                return placed.background.map(
                    lambda background: swapped(
                        LightRig.world, {("lighting", "mode"): "COLOR", ("lighting", "color"): background.color, ("lighting", "strength"): background.strength}, scene, identity, Some(rollback)
                    ).map(lambda _: None)
                ).default_with(lambda: reverted(LightRig.world, scene, identity).map(lambda _: None))

            return (
                accumulated(placement.power for placement in placed.lights)
                .bind(lambda powers: paired(accumulated(spawned_light(slot, placement, power) for slot, (placement, power) in enumerate(zip(placed.lights, powers))), backed()))
                .bind(lambda pair: joined_to(pair[0]))
                .bind(lambda lit: released(stale).bind(lambda _: LightRig.data.removed(identity)).map(lambda _: lit))
            )

        def built(attached: Attached[RigSetup], store: Stored[RigState], state: RigState, placement: tuple[tuple[Matrix, float], PlacedRig]) -> Outcome[tuple[bpy.types.Object, ...]]:
            setup = attached.struct(source)
            (matrix, radius), placed = placement

            def copied(rig: bpy.types.Object) -> Outcome[bpy.types.Object]:
                return Snapshot.capture(setup).bind(lambda held: held.apply(attached.struct(rig)).outcome).bind(lambda _: store.write(rig, state)).map(lambda _: rig)

            def empty(home: bpy.types.Collection) -> Outcome[bpy.types.Object]:
                return existing.map(Ok).default_with(
                    lambda: spawned(f"{holder.name} Rig", Nothing, home).bind(lambda rig: LightRig.empty.owned(identity, rig, of=Some(holder), filled=copied, scope=Some(rollback)))
                )

            return LightRig.collection.made(identity, lambda: founded(Some(scene.collection), f"{holder.name} Rig"), of=Some(holder), scope=Some(rollback)).bind(
                lambda home: empty(home).bind(lambda rig: fitted(holder, rig, matrix, radius, context).bind(lambda _: lights(rig, home, placed, alone=setup.linked)))
            )

        source = existing.default_value(holder)
        return paired(LightRig.setup.bound(identity), states(identity, source)).bind(
            lambda found: measured(found[0].struct(source), found[1][1]).bind(lambda planned: built(found[0], *found[1], planned))
        )

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
    blocks = (
        *LightRig.lights.serving(identity, rig),
        rig,
        *(held for holder in Mark.read(identity, rig).bind(lambda mark: mark.of).to_list() for held in LightRig.collection.serving(identity, holder)),
    )
    return (
        reverted(LightRig.world, scene, identity)
        .bind(lambda _: released(blocks))
        .bind(lambda count: accumulated((LightRig.data.removed(identity), LightRig.reference.removed(identity))).map(lambda counts: count + sum(counts)))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Empty)
def rig_place(call: Call[Empty]) -> Outcome[Done]:
    """Places the rig about the active object, or about the target of the active rig empty."""
    return (
        rooted(bpy.types.ViewLayer, call.context)
        .bind(focused)
        .bind(lambda item: served(call.identity, item))
        .bind(lambda holder: rigged(holder, LightRig.empty.found(call.identity, of=Some(holder)), call.context, call.identity))
        .map(lambda lights: Done(notes=(("INFO", f"Placed {len(lights)} lights"),)))
    )


@Operator.of(RigPick, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), primary=Some("name"))
def rig_pick(call: Call[RigPick]) -> Outcome[Done]:
    """Sets the resolved rig's layout from the catalog."""
    return _restated(call, lambda _rig, _setup, state: Ok(msgspec.structs.replace(state, rig=RIGS[call.properties.name])))


@Operator.of(Empty)
def rig_arrange(call: Call[Empty]) -> Outcome[Done]:
    """Converts the resolved rig's matched lights to the arranged lights its reference reading derives."""
    return _restated(
        call,
        lambda rig, setup, state: (
            lighting(setup.reference, PyOpenColorIO.GetCurrentConfig())
            .bind(state.rig.arranged)
            .bind(lambda arranged: arranged.to_result_with(lambda: Fault.of(Reason.ABSENT, repr(rig), member="reference")))
            .map(lambda arranged: msgspec.structs.replace(state, rig=msgspec.structs.replace(state.rig, layout=arranged)))
        ),
    )


@Operator.of(Empty)
def rig_remove(call: Call[Empty]) -> Outcome[Done]:
    """Removes the resolved rig's lights, empty, and collection."""
    return (
        paired(
            rooted(bpy.types.ViewLayer, call.context)
            .bind(lambda layer: resolved(layer, call.identity))
            .filter_with(lambda rig: LightRig.empty.marked(call.identity, rig).is_some(), lambda rig: Fault.of(Reason.ABSENT, repr(rig), member=LightRig.empty.key)),
            rooted(bpy.types.Scene, call.context),
        )
        .bind(lambda pair: rig_removed(pair[0], call.identity, pair[1]))
        .map(lambda count: Done(notes=(("INFO", f"Removed {count} rig data-blocks"),)))
    )


@Operator.of(ReferencePick, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), primary=Some("name"))
def rig_reference_pick(call: Call[ReferencePick]) -> Outcome[Done]:
    """Renders a catalog reference as the matched rig image."""
    return _restated(call, partial(_referenced, lambda _: REFERENCES[call.properties.name][1], identity=call.identity))


@Operator.of(ReferenceDraw, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_popup))))
def rig_reference_draw(call: Call[ReferenceDraw]) -> Outcome[Done]:
    """Draws a seeded reference as the matched rig image."""
    return _restated(call, partial(_referenced, lambda _: ReferenceImage.drawn(ReferenceDistribution[call.properties.distribution], call.properties.seed), identity=call.identity))


@Operator.of(GelPick, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), primary=Some("name"))
def rig_reference_gel(call: Call[GelPick]) -> Outcome[Done]:
    """Tints the reference with a catalog gel."""
    return _restated(call, partial(_referenced, lambda state: msgspec.structs.replace(state.recipe, tint=Gel[call.properties.name].value), identity=call.identity))


@Operator.of(Empty)
def rig_reference_apply(call: Call[Empty]) -> Outcome[Done]:
    """Renders the edited reference as the matched rig image."""
    return _restated(call, partial(_referenced, lambda state: state.recipe, identity=call.identity))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "GelPick",
    "LightRig",
    "ReferenceDraw",
    "ReferencePick",
    "RigPick",
    "RigSetup",
    "RigState",
    "fitted",
    "resolved",
    "rig_arrange",
    "rig_pick",
    "rig_place",
    "rig_reference_apply",
    "rig_reference_draw",
    "rig_reference_gel",
    "rig_reference_pick",
    "rig_remove",
    "rig_removed",
    "rigged",
    "served",
    "states",
    "target",
]
