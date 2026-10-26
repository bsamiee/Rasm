"""Light rigs aimed at a target, with background world."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
import itertools
from types import MappingProxyType
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
from mathutils import Matrix, Vector
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Empty, InvokePopup, Operator
from blender.bpy.data.ids.evaluation import evaluated
from blender.bpy.data.ids.images import generated, read, Tag
from blender.bpy.data.ids.ownership import committed, Mark, Ownership, released, Saved
from blender.bpy.data.ids.store import Handle, IdName
from blender.bpy.data.rna.descriptors import assigned, Descriptor, rooted, typed, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import UpdateHandler
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.scene.collections import founded
from blender.bpy.scene.lights.emitters import TEMPERATURE
from blender.bpy.scene.lights.linking import LINK_STATE, linked, RECEIVERS
from blender.bpy.scene.objects import descendants, extent, focused, ObjectState, reconciled, spawned, StackEntry, WorldBounds
from blender.bpy.scene.worlds import reverted, swapped
from blender.bpy.settings.attachments import Attached, Attachment, reconciling, Stored
from blender.bpy.ui.layout.rows import DataBlocks, Member, OperatorButton, PreviewGrid, Selector
from blender.bpy.ui.layout.sections import Section, Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.imaging.analysis.lighting import Lighting, LightingReading
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Positive
from blender.records.identity import content_hash
from blender.records.trees import Retention
from blender.rendering.lighting.references import ReferenceDistribution, ReferenceImage
from blender.rendering.lighting.rigs import Arranged, Disk, Placement, Placements, Point, Rig, rig_catalog, Spot, Sun

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

_TYPE: Final = Descriptor.declared(bpy.types.Light, "type", bpy.types.EnumProperty)
_COLOR: Final = Descriptor.declared(bpy.types.Light, "color", bpy.types.FloatProperty)
_BLACKBODY: Final = Descriptor.declared(bpy.types.Light, "use_temperature", bpy.types.BoolProperty)
_ENERGY: Final = MappingProxyType({kind: Descriptor.declared(kind, "energy", bpy.types.FloatProperty) for kind in (bpy.types.PointLight, bpy.types.SpotLight, bpy.types.AreaLight, bpy.types.SunLight)})
_RADIUS: Final = MappingProxyType({kind: Descriptor.declared(kind, "shadow_soft_size", bpy.types.FloatProperty) for kind in (bpy.types.PointLight, bpy.types.SpotLight)})
_CONE: Final = Descriptor.declared(bpy.types.SpotLight, "spot_size", bpy.types.FloatProperty)
_BLEND: Final = Descriptor.declared(bpy.types.SpotLight, "spot_blend", bpy.types.FloatProperty)
_SHAPE: Final = Descriptor.declared(bpy.types.AreaLight, "shape", bpy.types.EnumProperty)
_SIZE: Final = Descriptor.declared(bpy.types.AreaLight, "size", bpy.types.FloatProperty)
_ANGLE: Final = Descriptor.declared(bpy.types.SunLight, "angle", bpy.types.FloatProperty)

_FOCI: Final = (
    ("BOUNDS", "Bounds", "Midpoint of evaluated bounds of target and descendants"),
    ("ORIGIN", "Origin", "Target origin"),
    ("BONE", "Bone", "Pose bone head of armature target"),
    ("CURSOR", "3D Cursor", "3D cursor location"),
)
_AXES: Final = (("CAMERA", "Camera", "Scene camera axes"), ("VIEW", "View", "3D view axes"), ("TARGET", "Target", "Target object axes"), ("WORLD", "World", "World +X right, +Z up, and -Y back"))

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

    name: bpy.props.EnumProperty(name="Rig", description="Catalog rig", items=tuple((name, name, "") for name in rig_catalog()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class ReferencePick(bpy.types.PropertyGroup):
    """Reference recipe name from catalog."""

    name: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Reference", items=tuple((name, name, bpy.path.display_name(family.name.lower())) for family, recipes in ReferenceImage.catalog().items() for name in recipes)
    )


class ReferenceDraw(bpy.types.PropertyGroup):
    """Reference seed and color distribution."""

    distribution: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Distribution", items=tuple((member.name, bpy.path.display_name(member.name.lower()), "") for member in ReferenceDistribution), default=ReferenceDistribution.BALANCED.name
    )
    seed: bpy.props.IntProperty(name="Seed", min=0, max=2**24)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class GelPick(bpy.types.PropertyGroup):
    """Gel name for reference tint."""

    name: bpy.props.EnumProperty(name="Gel", items=tuple((name, name, "") for name in ReferenceImage.gels()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class RigFrame(msgspec.Struct, frozen=True, kw_only=True):
    """Rig world matrix with right, up, back, and center columns, and bounding radius."""

    matrix: Matrix
    radius: Positive


class Placing(msgspec.Struct, frozen=True, kw_only=True):
    """Rig frame, lights with powers, and background environment members by dotted path."""

    frame: RigFrame
    lights: tuple[tuple[Placement, float], ...]
    background: Option[Mapping[str, Value]]


class LightRig(msgspec.Struct, frozen=True, kw_only=True):
    """Rig setup attachment with owned lights, collection, and world."""

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
                .map(lambda _: LightRig.served(identity, empty).bind(lambda holder: rigged(holder, Some(empty), context, identity)).map(lambda _: Nothing))
                .default_value(Ok(Nothing))
            )
        )

    attachment: ClassVar[Attachment[RigSetup]] = Attachment(key="rig", form=RigSetup, owner=bpy.types.Object, changed=Some(reconciling(bpy.types.Object, replaced)))
    empty: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key=attachment.key, saved=Saved.USED)
    data: ClassVar[Ownership[bpy.types.Light]] = Ownership(kind=bpy.types.Light, key=attachment.key, saved=Saved.USED, single=False)
    collection: ClassVar[Ownership[bpy.types.Collection]] = Ownership(kind=bpy.types.Collection, key=attachment.key, saved=Saved.USED)
    world: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key=attachment.key, saved=Saved.USED)
    reference: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="rig_reference", saved=Saved.USED)

    @staticmethod
    def referenced[T: bpy.types.PropertyGroup](call: Call[T], recipe: Callable[[RigState], ReferenceImage]) -> Outcome[Done]:
        """Renders recipe into owned scene-linear setup image, removing unused reference images."""
        config = PyOpenColorIO.GetCurrentConfig()

        def stored(holder: bpy.types.Object) -> Outcome[int]:

            def imaged(setup: RigSetup, store: Stored[RigState], state: RigState) -> Outcome[None]:
                chosen = recipe(state)
                return generated(
                    LightRig.reference,
                    call.identity,
                    f"{holder.name} Reference",
                    content_hash(LightRig, chosen, arrays=(np.asarray(setup.size, dtype=np.int64),), config=Some(config.getCacheID())),
                    lambda: chosen.plane(setup.size).map(Some),
                    Tag(space=Some(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()), view_as_render=Some(True)),
                    of=Some(holder),
                ).bind(lambda image: paired(store.write(holder, msgspec.structs.replace(state, recipe=chosen)), written(setup, "reference", image.to_optional())).map(lambda _: None))

            return (
                paired(LightRig.attachment.read(call.identity, holder), states(call.identity, holder))
                .bind(lambda pair: imaged(pair[0], *pair[1]))
                .bind(lambda _: LightRig.reference.removed(call.identity))
            )

        return call.root(bpy.types.ViewLayer).bind(focused).bind(stored).map(lambda _: Done())

    @staticmethod
    def served(identity: Identity, item: bpy.types.Object) -> Outcome[bpy.types.Object]:
        """Returns rig empty target, or item when it is no rig empty."""
        return (
            LightRig.empty
            .marked(identity, item)
            .map(
                lambda mark: mark.of.map(lambda target: typed(target, bpy.types.Object, f"{item!r}.target")).default_with(
                    lambda: Error((Fault(reason=BlenderReason.ABSENT, subject=repr(item), member="target"),))
                )
            )
            .default_value(Ok(item))
        )

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields attachment, ownerships, reference handler, and section.

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

                return LightRig.attachment.bound(identity).map(placed)

            yield Ok((
                LightRig.attachment,
                LightRig.empty,
                LightRig.data,
                LightRig.collection,
                LightRig.world,
                LightRig.reference,
                UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Image,), body=referenced),
                self.section(),
            ))

    def section(self) -> Section:
        """Returns active rig setup with command buttons in header."""
        return Single(
            key="light_rig",
            title="Light Rig",
            header=tuple(
                OperatorButton(target=command)
                for command in (
                    rig_place_command,
                    rig_pick_command,
                    rig_arrange_command,
                    rig_remove_command,
                    rig_reference_pick_command,
                    rig_reference_draw_command,
                    rig_reference_gel_command,
                    rig_reference_apply_command,
                )
            ),
            store=Some(
                Hosted(
                    host=Some(Member(root=bpy.types.LayerObjects, path="active")),
                    store=Store(
                        attachment=LightRig.attachment,
                        retention=self.retention,
                        fields={"reference": lambda member: Selector(target=member, browse=DataBlocks(open=Some(bpy.ops.image.open)), preview=Some(PreviewGrid()))},
                    ),
                )
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


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
                return pose.map(lambda pose: holder.matrix_world @ pose.head).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(holder), member=setup.bone),))
            case "CURSOR":
                return Ok(scene.cursor.location.copy())
            case _:
                return Ok((bounds.low + bounds.high) / 2.0)

    def framed() -> Outcome[Matrix]:
        world = Matrix(((1.0, 0.0, 0.0), (0.0, 0.0, -1.0), (0.0, 1.0, 0.0)))

        def oriented(found: Located) -> Outcome[Matrix]:
            match found.area.spaces.active:
                case bpy.types.SpaceView3D(region_3d=bpy.types.RegionView3D(view_matrix=matrix)):
                    return Ok(matrix.inverted().to_3x3().normalized())
                case space:
                    return Error((Fault(reason=RecordReason.OUTSIDE, subject=f"{found.area!r}.spaces.active.region_3d", member=bpy.types.SpaceView3D.__name__, cause=type(space).__name__),))

        match setup.axes:
            case "CAMERA":
                return (
                    Option
                    .of_optional(scene.camera)
                    .map(lambda camera: camera.matrix_world.to_3x3().normalized())
                    .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(scene), member="camera"),))
                )
            case "VIEW":
                return rooted(bpy.types.WindowManager, context).bind(lambda manager: AreaQuery(space="VIEW_3D").first(manager.windows)).bind(oriented)
            case "TARGET":
                return Ok(holder.matrix_world.to_3x3().normalized() @ world)
            case _:
                return Ok(world)

    def measured(carried: tuple[WorldBounds, Vector, Matrix]) -> Outcome[RigFrame]:
        bounds, center, frame = carried
        radius: Outcome[float] = catch(msgspec.convert, exception=msgspec.ValidationError)(
            max((Vector(corner) - center).length for corner in itertools.product(*zip(bounds.low, bounds.high))),  # type: ignore[call-overload]  # ty: ignore[invalid-argument-type]
            type=Positive,
        ).map_error(lambda error: (Fault(reason=RecordReason.OUTSIDE, subject=f"{holder!r}.bound_box", member=str(error), cause=str(bounds)),))
        return radius.map(lambda held: RigFrame(matrix=Matrix.LocRotScale(center, frame, None).freeze(), radius=held))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    return (
        evaluated(layer)
        .bind(lambda depsgraph: extent((holder, *descendants(holder, layer)), depsgraph).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(holder), member="bound_box"),)))
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
    display = Snapshot.of(bpy.types.Object, {"empty_display_type": "SPHERE", "empty_display_size": frame.radius})
    follow = Snapshot.of(bpy.types.ChildOfConstraint, {"target": IdName.of(holder)})
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
                frame, reading = found
                return (
                    Result[Option[Placements], Faults]
                    .bind(
                        state.rig.placements(config, frame.radius, reading),
                        lambda placements: placements.to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(setup), member="reference"),)),
                    )
                    .bind(
                        lambda placements: accumulated(placement.power for placement in placements.lights).map(
                            lambda powers: Placing(
                                frame=frame,
                                lights=tuple(zip(placements.lights, powers)),
                                background=placements.background.map(lambda background: {"lighting.mode": "COLOR", "lighting.color": background.color, "lighting.strength": background.strength}),
                            )
                        )
                    )
                )

            return paired(target(holder, setup, context, scene, layer), _reading(setup.reference, config)).bind(prepared)

        def lights(rig: bpy.types.Object, home: bpy.types.Collection, placing: Placing, *, alone: bool) -> Outcome[tuple[bpy.types.Object, ...]]:
            aim = Snapshot.of(bpy.types.TrackToConstraint, {"target": IdName.of(rig), "track_axis": "TRACK_NEGATIVE_Z", "up_axis": "UP_Y"})
            rows = tuple(Ownership(kind=bpy.types.Object, key=f"light_{index}", saved=Saved.USED) for index in range(len(placing.lights)))
            keys = frozenset(row.key for row in rows)
            stale = tuple(block for block, mark in _serving(rig, identity) if mark.key not in keys)

            def emitted(placement: Placement, power: float) -> tuple["rna_enums.LightTypeItems", dict[Descriptor[bpy.types.Property], Value]]:
                light = placement.light
                identifier: rna_enums.LightTypeItems
                kind: type[bpy.types.Light]
                own: tuple[tuple[Descriptor[bpy.types.Property], Value], ...]
                match light.emitter:
                    case Point():
                        identifier, kind, own = "POINT", bpy.types.PointLight, ((_RADIUS[bpy.types.PointLight], placement.radius),)
                    case Spot(cone=cone, blend=blend):
                        identifier, kind, own = "SPOT", bpy.types.SpotLight, ((_RADIUS[bpy.types.SpotLight], placement.radius), (_CONE, cone), (_BLEND, blend))
                    case Disk():
                        identifier, kind, own = "AREA", bpy.types.AreaLight, ((_SHAPE, "DISK"), (_SIZE, 2.0 * placement.radius))
                    case Sun():
                        identifier, kind, own = "SUN", bpy.types.SunLight, ((_ANGLE, light.size),)
                return identifier, dict[Descriptor[bpy.types.Property], Value]((
                    *own,
                    (_ENERGY[kind], power),
                    (_COLOR, light.color),
                    (_BLACKBODY, light.temperature is not None),
                    *((TEMPERATURE, kelvin) for kelvin in Option.of_optional(light.temperature).to_list()),
                ))

            def spawned_light(row: Ownership[bpy.types.Object], placement: Placement, power: float) -> Outcome[bpy.types.Object]:
                kind, values = emitted(placement, power)
                label = f"{holder.name} {bpy.path.display_name(placement.light.role.name.lower())}"

                def stated(item: bpy.types.Object) -> ObjectState:
                    return ObjectState(
                        parent=Some(Handle.of(rig)),
                        values=Some(Snapshot.of(bpy.types.Object, {"lightgroup": item.name})),
                        placement=Some(Matrix.Translation(placement.location)),
                        constraints=Some({"Aim": StackEntry(kind="TRACK_TO", values=Some(aim))}),
                    )

                def built() -> Outcome[bpy.types.Object]:
                    return (
                        Result[bpy.types.Light, Faults]
                        .bind(
                            catch(bpy.data.lights.new, exception=RuntimeError)(label, kind).map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=label, cause=error),)),
                            lambda light: LightRig.data.owned(identity, light, of=Some(rig), scope=Some(rollback)),
                        )
                        .bind(lambda light: spawned(label, Some(light), home))
                    )

                def configured(item: bpy.types.Object) -> Outcome[bpy.types.Object]:
                    return typed(item.data, bpy.types.Light, f"{item!r}.data").bind(
                        lambda light: _TYPE.write(light, kind).bind(lambda _: reconciled(stated(item), item, ())).bind(lambda _: assigned(light.type_recast(), values)).map(lambda _: item)
                    )

                return row.made(identity, built, of=Some(rig), scope=Some(rollback)).bind(configured)

            def joined_to(prepared: tuple[tuple[bpy.types.Object, ...], None]) -> Outcome[tuple[bpy.types.Object, ...]]:
                lights, _ = prepared
                return (
                    accumulated(linked(item, RECEIVERS, (holder, *descendants(holder, layer)), LINK_STATE.prop.default, holder, identity) for item in lights).map(lambda _: None)
                    if alone
                    else accumulated(RECEIVERS.write(item.light_linking, None) for item in lights).map(lambda _: None)
                ).map(lambda _: lights)

            backed = placing.background.map(lambda prepared: swapped(LightRig.world, prepared, scene, identity, Some(rollback)).map(lambda _: None)).default_with(
                lambda: reverted(LightRig.world, scene, identity).map(lambda _: None)
            )
            return (
                paired(accumulated(spawned_light(row, placement, power) for row, (placement, power) in zip(rows, placing.lights)), backed)
                .bind(joined_to)
                .bind(lambda lit: released(stale).bind(lambda _: LightRig.data.removed(identity)).map(lambda _: lit))
            )

        def placed(attached: Attached[RigSetup], store: Stored[RigState], state: RigState, placing: Placing) -> Outcome[tuple[bpy.types.Object, ...]]:
            setup = attached.struct(source)

            def copied(rig: bpy.types.Object) -> Outcome[bpy.types.Object]:
                return Snapshot.capture(setup).bind(lambda held: held.apply(attached.struct(rig)).outcome).bind(lambda _: store.write(rig, state)).map(lambda _: rig)

            def empty(home: bpy.types.Collection) -> Outcome[bpy.types.Object]:
                return existing.map(Ok).default_with(
                    lambda: spawned(f"{holder.name} Rig", Nothing, home).bind(lambda rig: LightRig.empty.owned(identity, rig, of=Some(holder), filled=copied, scope=Some(rollback)))
                )

            return LightRig.collection.made(identity, lambda: founded(Some(scene.collection), f"{holder.name} Rig"), of=Some(holder), scope=Some(rollback)).bind(
                lambda home: empty(home).bind(lambda rig: fitted(holder, rig, placing.frame, context).bind(lambda _: lights(rig, home, placing, alone=setup.linked)))
            )

        source = existing.default_value(holder)
        return paired(LightRig.attachment.bound(identity), states(identity, source)).bind(
            lambda found: measured(found[0].struct(source), found[1][1]).bind(lambda planned: placed(found[0], *found[1], planned))
        )

    with ExitStack() as rollback:
        return committed(paired(rooted(bpy.types.Scene, context), rooted(bpy.types.ViewLayer, context)).bind(lambda found: placing(*found, PyOpenColorIO.GetCurrentConfig(), rollback)), rollback)


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
        *(block for block, _ in _serving(rig, identity)),
        rig,
        *(held for holder in Mark.read(identity, rig).bind(lambda mark: mark.of).to_list() for held in LightRig.collection.serving(identity, holder)),
    )
    return (
        reverted(LightRig.world, scene, identity)
        .bind(lambda _: released(blocks))
        .bind(lambda count: accumulated((LightRig.data.removed(identity), LightRig.reference.removed(identity))).map(lambda counts: count + sum(counts)))
    )


def _rig(view_layer: bpy.types.ViewLayer, identity: Identity) -> Outcome[bpy.types.Object]:
    """Returns active object marked as rig empty."""
    return focused(view_layer).bind(
        lambda item: Ok(item) if LightRig.empty.marked(identity, item).is_some() else Error((Fault(reason=BlenderReason.ABSENT, subject=repr(item), member=LightRig.empty.key),))
    )


def _serving(rig: bpy.types.Object, identity: Identity) -> tuple[tuple[bpy.types.Object, Mark], ...]:
    """Returns objects with marks naming rig empty."""
    return tuple((block, mark) for block in bpy.data.objects for mark in Mark.read(identity, block).to_list() if mark.of == Some(rig))


def states(identity: Identity, holder: bpy.types.Object) -> Outcome[tuple[Stored[RigState], RigState]]:
    """Returns the rig state store and the state holder keeps, defaults while it holds none."""
    return identity.name(Name.ATTACHMENT, "rig_state").bind(
        lambda name: Ok(Stored(name=name, decoder=msgspec.msgpack.Decoder(RigState))).bind(lambda store: store.read(holder).map(lambda found: (store, found.default_with(RigState))))
    )


def _reading(reference: bpy.types.Image | None, config: PyOpenColorIO.Config) -> Outcome[Option[LightingReading]]:
    """Returns reference lighting reading, `Nothing` without reference or measurable reading."""
    return Option.of_optional(reference).map(lambda image: read(image, config).bind(lambda file: Lighting().read(config, file.pixels))).default_value(Ok(Nothing))


def rig_place(call: Call[Empty]) -> Outcome[Done]:
    """Places the rig about the active object, or about the target of the active rig empty."""
    return (
        call
        .root(bpy.types.ViewLayer)
        .bind(focused)
        .bind(lambda item: LightRig.served(call.identity, item))
        .bind(lambda holder: rigged(holder, LightRig.empty.found(call.identity, of=Some(holder)), call.context, call.identity))
        .map(lambda lights: Done(notes=(("INFO", f"Placed {len(lights)} lights"),)))
    )


def rig_pick(call: Call[RigPick]) -> Outcome[Done]:
    """Sets the active rig from the catalog."""
    return (
        call
        .root(bpy.types.ViewLayer)
        .bind(focused)
        .bind(lambda rig: states(call.identity, rig).bind(lambda pair: pair[0].write(rig, msgspec.structs.replace(pair[1], rig=rig_catalog()[call.properties.name]))))
        .map(lambda _: Done())
    )


def rig_arrange(call: Call[Empty]) -> Outcome[Done]:
    """Converts the active rig's matched lights to arranged lights."""
    config = PyOpenColorIO.GetCurrentConfig()

    def converted(rig: bpy.types.Object, setup: RigSetup, store: Stored[RigState], state: RigState) -> Outcome[None]:
        return (
            _reading(setup.reference, config)
            .bind(state.rig.arranged)
            .bind(lambda arranged: arranged.to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(rig), member="reference"),)))
            .bind(lambda arranged: store.write(rig, msgspec.structs.replace(state, rig=msgspec.structs.replace(state.rig, layout=arranged))))
        )

    return (
        call
        .root(bpy.types.ViewLayer)
        .bind(lambda layer: _rig(layer, call.identity))
        .bind(lambda rig: paired(LightRig.attachment.read(call.identity, rig), states(call.identity, rig)).bind(lambda held: converted(rig, held[0], *held[1])))
        .map(lambda _: Done())
    )


def rig_remove(call: Call[Empty]) -> Outcome[Done]:
    """Removes the active rig's lights, empty, and collection."""
    return (
        paired(call.root(bpy.types.ViewLayer).bind(lambda layer: _rig(layer, call.identity)), call.root(bpy.types.Scene))
        .bind(lambda pair: rig_removed(pair[0], call.identity, pair[1]))
        .map(lambda count: Done(notes=(("INFO", f"Removed {count} rig data-blocks"),)))
    )


def rig_reference_pick(call: Call[ReferencePick]) -> Outcome[Done]:
    """Renders a catalog reference as the matched rig image."""
    return LightRig.referenced(call, lambda _: next(recipes[call.properties.name] for recipes in ReferenceImage.catalog().values() if call.properties.name in recipes))


def rig_reference_draw(call: Call[ReferenceDraw]) -> Outcome[Done]:
    """Draws a seeded reference as the matched rig image."""
    return LightRig.referenced(call, lambda _: ReferenceImage.drawn(ReferenceDistribution[call.properties.distribution], call.properties.seed))


def rig_reference_gel(call: Call[GelPick]) -> Outcome[Done]:
    """Tints the reference with a catalog gel."""
    return LightRig.referenced(call, lambda state: msgspec.structs.replace(state.recipe, tint=ReferenceImage.gels()[call.properties.name]))


def rig_reference_apply(call: Call[Empty]) -> Outcome[Done]:
    """Renders the edited reference as the matched rig image."""
    return LightRig.referenced(call, lambda state: state.recipe)


# --- [COMPOSITION] ----------------------------------------------------------------------

rig_place_command: Operator[Empty] = Operator(form=Empty, run=rig_place)
rig_pick_command: Operator[RigPick] = Operator(form=RigPick, invoke=Some(InvokePopup.SEARCH), primary=Some("name"), run=rig_pick)
rig_arrange_command: Operator[Empty] = Operator(form=Empty, run=rig_arrange)
rig_remove_command: Operator[Empty] = Operator(form=Empty, run=rig_remove)
rig_reference_pick_command: Operator[ReferencePick] = Operator(form=ReferencePick, invoke=Some(InvokePopup.SEARCH), primary=Some("name"), run=rig_reference_pick)
rig_reference_draw_command: Operator[ReferenceDraw] = Operator(form=ReferenceDraw, invoke=Some(InvokePopup.REDO), run=rig_reference_draw)
rig_reference_gel_command: Operator[GelPick] = Operator(form=GelPick, invoke=Some(InvokePopup.SEARCH), primary=Some("name"), run=rig_reference_gel)
rig_reference_apply_command: Operator[Empty] = Operator(form=Empty, run=rig_reference_apply)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "GelPick",
    "LightRig",
    "ReferenceDraw",
    "ReferencePick",
    "RigFrame",
    "RigPick",
    "RigSetup",
    "RigState",
    "fitted",
    "rig_arrange_command",
    "rig_pick_command",
    "rig_place_command",
    "rig_reference_apply_command",
    "rig_reference_draw_command",
    "rig_reference_gel_command",
    "rig_reference_pick_command",
    "rig_remove_command",
    "rig_removed",
    "rigged",
    "states",
    "target",
]
