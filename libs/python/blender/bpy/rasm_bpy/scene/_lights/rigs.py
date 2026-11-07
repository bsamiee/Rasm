"""Light rig feature: setup and state, lights placed about a target with a background world, the reference image a matched rig reads, sections, and operators."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from enum import Enum
from functools import partial
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
from mathutils import Matrix, Vector
import msgspec
import numpy as np
import PyOpenColorIO

from rasm_bpy.commands.invocations import Popup
from rasm_bpy.commands.operators import Call, called, Done, Empty, Operator
from rasm_bpy.data._extension.events import _updated
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions, Handler
from rasm_bpy.data._ids.evaluation import _evaluated
from rasm_bpy.data._ids.images import _lighting, Tag
from rasm_bpy.data._ids.ownership import _released, _retained, Mark, Ownership, Saved
from rasm_bpy.data._ids.store import Handle, IdName
from rasm_bpy.data._rna.descriptors import Descriptor, Value, written
from rasm_bpy.data._rna.paths import RnaPath, rooted
from rasm_bpy.data._rna.properties import _reconciling, choices, enum_choices
from rasm_bpy.data._rna.snapshots import assigned, Snapshot
from rasm_bpy.nodes._content import Generated
from rasm_bpy.scene._collections import created
from rasm_bpy.scene._lights.emitters import _TEMPERATURE
from rasm_bpy.scene._lights.groups import _light_group
from rasm_bpy.scene._lights.linking import _LINK_STATE, _linked, _RECEIVERS
from rasm_bpy.scene._objects import _active_object, _descendants, _extent, _ObjectState, _owned_object, _reconciled, _spawned, _StackEntry
from rasm_bpy.scene._worlds import _BackdropSource, _reverted, _swapped
from rasm_bpy.settings._attachments import Attached, Attachment, Stored
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import _FUNCTIONS, OperatorButton, Property
from rasm_bpy.ui._layout.sections import Section
from rasm_bpy.ui._layout.values import Store
from rasm_bpy.ui._screens.areas import AreaQuery, Located
from rasm_imaging import LightingReading
from rasm_records import accumulated, content_hash, Error, Fault, Faults, Nothing, Ok, Outcome, paired, Positive, Reason, Retention, SEED_BOUND, typed
from rasm_rendering import Arranged, Disk, Gel, PlacedLight, PlacedRig, Point, ReferenceDistribution, ReferenceImage, REFERENCES, Rig, RIGS, Spot, Sun

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class _RigFocus(Enum):
    """Rig center, valued by its label and description."""

    BOUNDS = ("Bounds", "Midpoint of evaluated bounds of target and descendants")
    ORIGIN = ("Origin", "Target origin")
    BONE = ("Bone", "Pose bone head of armature target")
    CURSOR = ("3D Cursor", "3D cursor location")


class _RigAxes(Enum):
    """Azimuth and elevation axes, valued by their label and description."""

    CAMERA = ("Camera", "Scene camera axes")
    VIEW = ("View", "3D view axes")
    TARGET = ("Target", "Target object axes")
    WORLD = ("World", "World +X right, +Z up, and -Y back")


# --- [CONSTANTS] ------------------------------------------------------------------------

_TYPE: Final = Descriptor(root=bpy.types.Light, prop=bpy.types.Light.bl_rna.properties["type"])

# --- [MODELS] ---------------------------------------------------------------------------


class RigSetup(bpy.types.PropertyGroup):
    """Focus, axes, linking, and reference of one rig empty."""

    focus: bpy.props.EnumProperty(name="Focus", description="Rig center", items=choices((focus.name, *focus.value) for focus in _RigFocus), default=_RigFocus.BOUNDS.name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    bone: bpy.props.StringProperty(name="Bone", description="Pose bone centering a bone focus")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    axes: bpy.props.EnumProperty(name="Axes", description="Azimuth and elevation axes", items=choices((axes.name, *axes.value) for axes in _RigAxes), default=_RigAxes.CAMERA.name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    linked: bpy.props.BoolProperty(name="Target Alone", description="Lights reach target and descendants alone")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    reference: bpy.props.PointerProperty(type=bpy.types.Image, name="Reference", description="Image a matched rig reads")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    size: bpy.props.IntProperty(name="Reference Size", description="Side of the rendered reference image", min=1, default=256)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class _RigState(msgspec.Struct, frozen=True, kw_only=True):
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
    seed: bpy.props.IntProperty(name="Seed", min=0, max=SEED_BOUND)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class GelPick(bpy.types.PropertyGroup):
    """Gel name for reference tint."""

    name: bpy.props.EnumProperty(name="Gel", items=enum_choices(Gel))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class _LightRig(msgspec.Struct, frozen=True, kw_only=True):
    """Rig setup attachment with owned empty, lights, collection, world, and reference image, and the rig and reference sections."""

    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing

    @staticmethod
    def replaced(rig: Handle[bpy.types.Object], identity: Identity) -> Outcome[Option[str]]:
        """Places rig again from stored setup.

        Args:
            rig: Handle of rig empty.
            identity: Identity binding attachment and marking owned data.

        Returns:
            `Nothing` naming no undo step, or `absent`, `unregistered`, `unreadable`, and `rigged` faults.
        """
        context = bpy.context
        return rig.resolve().bind(
            lambda empty: (
                _LightRig.empty
                .marked(identity, empty)
                .map(lambda _: _served(identity, empty).bind(lambda target: _rigged(target, Some(empty), context, identity)).map(lambda _: Nothing))
                .default_value(Ok(Nothing))
            )
        )

    setup: ClassVar[Attachment[RigSetup]] = Attachment(key="rig", form=RigSetup, owner=bpy.types.Object, changed=Some(partial(_reconciling, bpy.types.Object, replaced)))
    empty: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="rig", saved=Saved.USED)
    lights: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="rig_light", saved=Saved.USED)
    data: ClassVar[Ownership[bpy.types.Light]] = Ownership(kind=bpy.types.Light, key="rig", saved=Saved.USED, single=False)
    collection: ClassVar[Ownership[bpy.types.Collection]] = Ownership(kind=bpy.types.Collection, key="rig", saved=Saved.USED)
    world: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key="rig", saved=Saved.USED)
    reference: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="rig_reference", saved=Saved.USED)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Acquisitions]:
        """Yields attachment, ownerships, the handler placing rigs again when their reference image changes, and both sections.

        Args:
            identity: Identity of extension registering row.

        Yields:
            Declarations held until extension unregisters.
        """
        with identity.reports.deferred(_LightRig.replaced) as deferred:

            def referenced(_: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                touched = frozenset(block.original for update in updates if (block := update.id) is not None)

                def placed(attached: Attached[RigSetup]) -> None:
                    for rig, _ in _LightRig.empty.held(identity):
                        if attached.struct(rig).reference in touched:
                            deferred(Handle.of(rig), identity)

                return _LightRig.setup.bound(identity).map(placed)

            yield (
                _LightRig.setup,
                _LightRig.empty,
                _LightRig.lights,
                _LightRig.data,
                _LightRig.collection,
                _LightRig.world,
                _LightRig.reference,
                Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(_updated, (bpy.types.Image,), referenced)),
                self.section(),
                self.references(),
            )

    def section(self) -> Section[RigSetup]:
        """Returns active rig setup with the place, pick, arrange, and remove buttons in its header."""
        return Section(
            key="light_rig",
            header=tuple(OperatorButton(target=command) for command in (rig_place, rig_pick, rig_arrange, rig_remove)),
            store=Some(
                Store(
                    attachment=_LightRig.setup,
                    host=Some(Member(root=bpy.types.LayerObjects, path=("active",))),
                    retention=self.retention,
                    fields={"reference": lambda member: Property(target=member, function=_FUNCTIONS["template_ID_preview"], keywords={"open": "image.open"})},
                )
            ),
        )

    @staticmethod
    def references() -> Section:
        """Returns the reference pick, draw, gel, and apply buttons."""
        commands = (rig_reference_pick, rig_reference_draw, rig_reference_gel, rig_reference_apply)
        return Section(key="rig_reference", rows=tuple(OperatorButton(target=command) for command in commands))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RIGS]


def _served(identity: Identity, item: bpy.types.Object) -> Outcome[bpy.types.Object]:
    """Returns rig empty target, or item when it is no rig empty."""
    return (
        _LightRig.empty
        .marked(identity, item)
        .map(lambda mark: mark.of.map(partial(typed, bpy.types.Object, f"{item!r}.target")).default_with(lambda: Error(Fault.of(Reason.ABSENT, repr(item), member="target"))))
        .default_value(Ok(item))
    )


def _resolved(view_layer: bpy.types.ViewLayer, identity: Identity) -> Outcome[bpy.types.Object]:
    """Returns the object holding the rig state every rig command reads: the active rig empty, else the rig empty serving the active object, else the active object."""
    return _active_object(view_layer).map(
        lambda item: _LightRig.empty.marked(identity, item).map(lambda _: item).or_else_with(lambda: _LightRig.empty.found(identity, of=Some(item))).default_value(item)
    )


def _states(identity: Identity, holder: bpy.types.Object) -> Outcome[tuple[Stored[_RigState], _RigState]]:
    """Returns the rig state store and the state holder keeps, defaults while it holds none."""
    store = Stored.keyed(identity, "rig_state", msgspec.msgpack.Decoder(_RigState))
    return store.read(holder).map(lambda found: (store, found.default_with(_RigState)))


def _restated[T: bpy.types.PropertyGroup](call: Call[T], change: Callable[[bpy.types.Object, RigSetup, _RigState], Outcome[_RigState]]) -> Outcome[Done]:
    """Writes the state a change derives from the resolved rig's setup and state."""
    return (
        rooted(bpy.types.ViewLayer, call.context)
        .bind(lambda layer: _resolved(layer, call.identity))
        .bind(
            lambda rig: paired(_LightRig.setup.read(call.identity, rig), _states(call.identity, rig)).bind(
                lambda held: change(rig, held[0], held[1][1]).bind(lambda state: held[1][0].write(rig, state))
            )
        )
        .map(lambda _: Done())
    )


def _referenced(recipe: Callable[[_RigState], ReferenceImage], holder: bpy.types.Object, setup: RigSetup, state: _RigState, identity: Identity) -> Outcome[_RigState]:
    """Renders the recipe into the rig's owned scene-linear reference image, removing unused reference images, returning the state holding the recipe."""
    chosen = recipe(state)
    config = PyOpenColorIO.GetCurrentConfig()
    return (
        Generated(
            ownership=_LightRig.reference,
            content=content_hash(_RigState, chosen, config.getCacheID().encode(), arrays=(np.asarray(setup.size, dtype=np.int64),)),
            name=f"{holder.name} Reference",
            plane=lambda: chosen.plane(setup.size).map(Some),
            tag=Tag(space=Some(PyOpenColorIO.ROLE_SCENE_LINEAR), view_as_render=Some(True)),
        )
        .made(identity, of=Some(holder))
        .bind(lambda image: written(setup, ("reference",), image.to_optional()))
        .bind(lambda _: _LightRig.reference.removed(identity))
        .map(lambda _: msgspec.structs.replace(state, recipe=chosen))
    )


# --- [PLACEMENT]


def _rig_frame(target: bpy.types.Object, setup: RigSetup, context: bpy.types.Context, scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[tuple[Matrix, float]]:
    """Returns rig world matrix with right, up, back, and center columns, and bounding radius.

    Args:
        target: Target object.
        setup: Focus and axes choices.
        context: Context supplying window manager for view axes.
        scene: Scene supplying camera and cursor.
        layer: View layer evaluating target and descendants.

    Returns:
        Matrix and radius, or `absent` for missing bounds, bone, camera, or view, `outside` for a non-3D view or zero radius.
    """

    def centered(low: Vector, high: Vector) -> Outcome[Vector]:
        match _RigFocus[setup.focus]:
            case _RigFocus.ORIGIN:
                return Ok(target.matrix_world.translation.copy())
            case _RigFocus.BONE:
                bone: str = setup.bone  # ty: ignore[unsound-assignment]
                pose = Option.of_optional(target.pose).bind(lambda pose: catch(lambda: pose.bones[bone], exception=KeyError)().to_option())
                return pose.map(lambda pose: target.matrix_world @ pose.head).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(target), member=bone))
            case _RigFocus.CURSOR:
                return Ok(scene.cursor.location.copy())
            case _RigFocus.BOUNDS:
                return Ok((low + high) / 2.0)

    def framed() -> Outcome[Matrix]:
        world = Matrix(((1.0, 0.0, 0.0), (0.0, 0.0, -1.0), (0.0, 1.0, 0.0)))

        def oriented(found: Located) -> Outcome[Matrix]:
            match found.area.spaces.active:
                case bpy.types.SpaceView3D(region_3d=bpy.types.RegionView3D(view_rotation=rotation)):
                    return Ok(rotation.to_matrix())
                case space:
                    return Error(Fault.of(Reason.OUTSIDE, f"{found.area!r}.spaces.active.region_3d", type(space).__name__, member=bpy.types.SpaceView3D.__name__))

        match _RigAxes[setup.axes]:
            case _RigAxes.CAMERA:
                return Option.of_optional(scene.camera).map(lambda camera: camera.matrix_world.to_3x3().normalized()).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(scene), member="camera"))
            case _RigAxes.VIEW:
                return rooted(bpy.types.WindowManager, context).bind(lambda manager: AreaQuery(space=bpy.types.SpaceView3D).first(manager.windows)).bind(oriented)
            case _RigAxes.TARGET:
                return Ok(target.matrix_world.to_3x3().normalized() @ world)
            case _RigAxes.WORLD:
                return Ok(world)

    def measured(found: tuple[tuple[Vector, Vector], Vector, Matrix]) -> Outcome[tuple[Matrix, float]]:
        (low, high), center, frame = found
        reach = Vector(tuple(max(abs(lower), abs(upper)) for lower, upper in zip((low - center)[:], (high - center)[:], strict=True)))
        try:
            radius: float = msgspec.convert(reach.length, type=Positive)  # ty: ignore[unsound-assignment]
        except msgspec.ValidationError as error:
            return Error(Fault.of(Reason.OUTSIDE, f"{target!r}.bound_box {low[:]} to {high[:]}", error, member="a positive radius"))
        return Ok((Matrix.LocRotScale(center, frame, None).freeze(), radius))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    return (
        _evaluated(layer)
        .bind(lambda depsgraph: _extent((target, *_descendants(target, layer)), depsgraph).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(target), member="bound_box")))
        .bind(lambda bounds: paired(centered(*bounds), framed()).map(lambda pair: (bounds, *pair)))
        .bind(measured)
    )


def _fitted(target: bpy.types.Object, rig: bpy.types.Object, matrix: Matrix, radius: float, context: bpy.types.Context) -> Outcome[None]:
    """Writes rig empty placement, display, and target constraint, its inverse set by `constraint.childof_set_inverse`.

    Args:
        target: Target object.
        rig: Owned rig empty.
        matrix: World matrix from `rig_frame`.
        radius: Bounding radius from `rig_frame`.
        context: Context the inverse operator runs under with the rig as object.

    Returns:
        `Ok` once written, or write and operator faults.
    """
    display = Snapshot.of(bpy.types.Object, {("empty_display_type",): "SPHERE", ("empty_display_size",): radius})
    follow = Snapshot.of(bpy.types.ChildOfConstraint, {("target",): IdName.of(target)})
    return _reconciled(_ObjectState(values=Some(display), placement=Some(matrix), constraints=Some({"Follow": _StackEntry(kind="CHILD_OF", values=Some(follow))})), rig, ()).bind(
        lambda _: called(bpy.ops.constraint.childof_set_inverse, {"constraint": "Follow", "owner": "OBJECT"}, override=Some(context.temp_override(object=rig))).map(lambda _: None)
    )


def _rigged(target: bpy.types.Object, existing: Option[bpy.types.Object], context: bpy.types.Context, identity: Identity) -> Outcome[tuple[bpy.types.Object, ...]]:
    """Places rig about target, rolling back created data and world swap on fault.

    Args:
        target: Target object.
        existing: Rig empty to place again from stored setup, `Nothing` to build one from target setup.
        context: Context supplying scene, view layer, and window manager for view axes.
        identity: Identity binding attachment and marking owned data.

    Returns:
        Lights in placement order, or `absent`, setup read, `rig_frame`, reference, placement, and write faults.
    """

    def placing(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, config: PyOpenColorIO.Config, rollback: ExitStack) -> Outcome[tuple[bpy.types.Object, ...]]:
        def measured(setup: RigSetup, state: _RigState) -> Outcome[tuple[tuple[Matrix, float], PlacedRig]]:
            def prepared(found: tuple[tuple[Matrix, float], Option[LightingReading]]) -> Outcome[tuple[tuple[Matrix, float], PlacedRig]]:
                frame, measurement = found
                return (
                    Result[Option[PlacedRig], Faults]
                    .bind(state.rig.placements(config, frame[1], measurement), lambda placed: placed.to_result_with(lambda: Fault.of(Reason.ABSENT, repr(setup), member="reference")))
                    .map(lambda placed: (frame, placed))
                )

            return paired(_rig_frame(target, setup, context, scene, layer), _lighting(setup.reference, config)).bind(prepared)

        def lights(rig: bpy.types.Object, home: bpy.types.Collection, placed: PlacedRig, *, alone: bool) -> Outcome[tuple[bpy.types.Object, ...]]:
            aim = Snapshot.of(bpy.types.TrackToConstraint, {("target",): IdName.of(rig), ("track_axis",): "TRACK_NEGATIVE_Z", ("up_axis",): "UP_Y"})
            count = len(placed.lights)
            stale = tuple(block for block, mark in _LightRig.lights.held(identity) if mark.of == Some(rig) and mark.slot.filter(lambda slot: slot < count).is_none())

            def emitted(placement: PlacedLight, power: float) -> tuple["rna_enums.LightTypeItems", dict[RnaPath, Value]]:
                light = placement.light
                temperature: dict[RnaPath, Value] = {_TEMPERATURE.path: light.temperature} if light.temperature is not None else {}
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
                label = f"{target.name} {bpy.path.display_name(placement.light.role.name.lower())}"

                def configured(item: bpy.types.Object, light: bpy.types.Light) -> Outcome[bpy.types.Object]:
                    state = _ObjectState(
                        parent=Some(Handle.of(rig)),
                        values=Some(Snapshot.of(bpy.types.Object, {("lightgroup",): _light_group(item.name)})),
                        placement=Some(Matrix.Translation(placement.location)),
                        constraints=Some({"Aim": _StackEntry(kind="TRACK_TO", values=Some(aim))}),
                    )
                    return _TYPE.write(light, kind).bind(lambda _: _reconciled(state, item, ())).bind(lambda _: assigned(light.type_recast(), values)).map(lambda _: item)

                return _owned_object((_LightRig.lights, _LightRig.data), label, partial(bpy.data.lights.new, label, kind), home, identity, of=Some(rig), slot=Some(slot), scope=Some(rollback)).bind(
                    lambda pair: configured(*pair)
                )

            def joined_to(lit: tuple[bpy.types.Object, ...]) -> Outcome[tuple[bpy.types.Object, ...]]:
                members = (target, *_descendants(target, layer)) if alone else ()
                return (
                    accumulated(_linked(item, _RECEIVERS, members, _LINK_STATE.prop.default, target, context, identity) for item in lit).map(lambda _: None)
                    if alone
                    else accumulated(_RECEIVERS.write(item.light_linking, None) for item in lit).map(lambda _: None)
                ).map(lambda _: lit)

            def backed() -> Outcome[None]:
                return placed.background.map(
                    lambda background: _swapped(
                        _LightRig.world,
                        {("lighting", "source"): _BackdropSource.COLOR.name, ("lighting", "color"): background.color, ("lighting", "strength"): background.strength},
                        scene,
                        identity,
                        Some(rollback),
                    ).map(lambda _: None)
                ).default_with(lambda: _reverted(_LightRig.world, scene, identity).map(lambda _: None))

            return (
                accumulated(placement.power for placement in placed.lights)
                .bind(lambda powers: paired(accumulated(spawned_light(slot, placement, power) for slot, (placement, power) in enumerate(zip(placed.lights, powers))), backed()))
                .bind(lambda pair: joined_to(pair[0]))
                .bind(lambda lit: _released(stale).bind(lambda _: _LightRig.data.removed(identity)).map(lambda _: lit))
            )

        def built(attached: Attached[RigSetup], store: Stored[_RigState], state: _RigState, placement: tuple[tuple[Matrix, float], PlacedRig]) -> Outcome[tuple[bpy.types.Object, ...]]:
            setup = attached.struct(source)
            (matrix, radius), placed = placement

            def copied(rig: bpy.types.Object) -> Outcome[bpy.types.Object]:
                return Snapshot.capture(setup).bind(lambda held: held.apply(attached.struct(rig)).outcome).bind(lambda _: store.write(rig, state)).map(lambda _: rig)

            def empty(home: bpy.types.Collection) -> Outcome[bpy.types.Object]:
                return existing.map(Ok).default_with(
                    lambda: _spawned(f"{target.name} Rig", Nothing, home).bind(lambda rig: _LightRig.empty.owned(identity, rig, of=Some(target), filled=copied, scope=Some(rollback)))
                )

            return _LightRig.collection.made(identity, lambda: created(Some(scene.collection), f"{target.name} Rig"), of=Some(target), scope=Some(rollback)).bind(
                lambda home: empty(home).bind(lambda rig: _fitted(target, rig, matrix, radius, context).bind(lambda _: lights(rig, home, placed, alone=setup.linked)))
            )

        source = existing.default_value(target)
        return paired(_LightRig.setup.bound(identity), _states(identity, source)).bind(
            lambda found: measured(found[0].struct(source), found[1][1]).bind(lambda planned: built(found[0], *found[1], planned))
        )

    with ExitStack() as rollback:
        return _retained(paired(rooted(bpy.types.Scene, context), rooted(bpy.types.ViewLayer, context)).bind(lambda found: placing(*found, PyOpenColorIO.GetCurrentConfig(), rollback)), rollback)


def _rig_removed(rig: bpy.types.Object, identity: Identity, scene: bpy.types.Scene) -> Outcome[int]:
    """Removes rig lights, empty, and collection, restoring world it replaced.

    Args:
        rig: Owned rig empty.
        identity: Identity reading marks.
        scene: Scene with world to restore.

    Returns:
        Removed data-block count, or `refused` faults.
    """
    blocks = (
        *_LightRig.lights.serving(identity, rig),
        rig,
        *(held for target in Mark.read(identity, rig).bind(lambda mark: mark.of).to_list() for held in _LightRig.collection.serving(identity, target)),
    )
    return (
        _reverted(_LightRig.world, scene, identity)
        .bind(lambda _: _released(blocks))
        .bind(lambda count: accumulated((_LightRig.data.removed(identity), _LightRig.reference.removed(identity))).map(lambda counts: count + sum(counts)))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Empty)
def rig_place(call: Call[Empty]) -> Outcome[Done]:
    """Places the rig about the active object, or about the target of the active rig empty."""
    return (
        rooted(bpy.types.ViewLayer, call.context)
        .bind(_active_object)
        .bind(lambda item: _served(call.identity, item))
        .bind(lambda target: _rigged(target, _LightRig.empty.found(call.identity, of=Some(target)), call.context, call.identity))
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
            _lighting(setup.reference, PyOpenColorIO.GetCurrentConfig())
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
            .bind(lambda layer: _resolved(layer, call.identity))
            .filter_with(lambda rig: _LightRig.empty.marked(call.identity, rig).is_some(), lambda rig: Fault.of(Reason.ABSENT, repr(rig), member=_LightRig.empty.key)),
            rooted(bpy.types.Scene, call.context),
        )
        .bind(lambda pair: _rig_removed(pair[0], call.identity, pair[1]))
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
    "ReferenceDraw",
    "ReferencePick",
    "RigPick",
    "RigSetup",
    "rig_arrange",
    "rig_pick",
    "rig_place",
    "rig_reference_apply",
    "rig_reference_draw",
    "rig_reference_gel",
    "rig_reference_pick",
    "rig_remove",
]
