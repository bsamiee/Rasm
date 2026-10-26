"""Light rigs aimed at a target, with background world."""

from collections.abc import Callable, Generator, Hashable
from contextlib import contextmanager, ExitStack
from functools import partial
import itertools
from typing import Annotated, ClassVar, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from mathutils import Matrix, Vector
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, InvokePopup, Operator, Own
from blender.bpy.data.ids.evaluation import evaluated
from blender.bpy.data.ids.images import generated, read, Tag
from blender.bpy.data.ids.ownership import committed, Mark, Ownership, released, Saved
from blender.bpy.data.ids.store import Handle, IdName, vacant
from blender.bpy.data.rna.descriptors import root_in, rooted, typed, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import UpdateHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.graphs import Graph
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.previews.queue import Job, PreviewQueue, Rank
from blender.bpy.scene.collections import founded
from blender.bpy.scene.lights.linking import LINK_STATE, linked, RECEIVERS
from blender.bpy.scene.objects import descendants, extent, focused, ObjectState, reconciled, spawned, StackEntry, WorldBounds
from blender.bpy.scene.worlds import Backdrop, Environment, FlatColor, reverted, swapped, world_graph
from blender.bpy.settings.attachments import Attached, Attachment, reconciling
from blender.bpy.settings.history import Retention
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import DataBlocks, Member, OperatorButton, PreviewGrid, RowDraw, Selector
from blender.bpy.ui.layout.sections import Section, Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.resources.previews import PREVIEWS
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.imaging.analysis.lighting import LightingReading
from blender.imaging.color.processors import DisplayView
from blender.imaging.faults import Access, accumulated, ImagingFileFault, paired
from blender.imaging.hashing import content_hash
from blender.imaging.parameters import Case, Gated, Positive, Seed, Side
from blender.rendering.lighting.references import DistantLight, gray_ball, ReferenceDistribution, ReferenceImage, Roll, sphere
from blender.rendering.lighting.rigs import Arranged, Disk, Placement, Placements, Point, Rig, rig_catalog, Spot, Sun

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Focus = Bounds | Origin | BoneHead | Cursor
type RigAxes = CameraAxes | ViewAxes | TargetAxes | WorldAxes


# --- [CHOICES] --------------------------------------------------------------------------

rigs = Choices.named(lambda _identity, _owner, _context: rig_catalog())
references = Choices(lambda _identity, _owner, _context: ((name, name, family.label) for family, recipes in ReferenceImage.catalog().items() for name in recipes))
gels = Choices.named(lambda _identity, _owner, _context: ReferenceImage.gels())


# --- [MODELS] ---------------------------------------------------------------------------


class Bounds(Case, frozen=True, kw_only=True, tag=1):
    """Midpoint of evaluated bounds of target and descendants."""


class Origin(Case, frozen=True, kw_only=True, tag=2):
    """Target origin."""


class BoneHead(Case, frozen=True, kw_only=True, tag=3):
    """Pose bone head of armature target."""

    title = "Bone"
    bone: Annotated[str, msgspec.Meta(description="Pose bone centering rig")] = ""


class Cursor(Case, frozen=True, kw_only=True, tag=4):
    """3D cursor location."""

    title = "3D Cursor"


class CameraAxes(Case, frozen=True, kw_only=True, tag=1):
    """Scene camera axes."""

    title = "Camera"


class ViewAxes(Case, frozen=True, kw_only=True, tag=2):
    """3D view axes."""

    title = "View"


class TargetAxes(Case, frozen=True, kw_only=True, tag=3):
    """Target object axes."""

    title = "Target"


class WorldAxes(Case, frozen=True, kw_only=True, tag=4):
    """World +X right, +Z up, and -Y back."""

    title = "World"


class RigSetup(msgspec.Struct, frozen=True, kw_only=True):
    """Rig, focus, axes, linking, and reference of one rig empty."""

    rig: Annotated[Rig, msgspec.Meta(description="Lights about target")] = Rig(layout=Arranged())
    focus: Annotated[Focus, msgspec.Meta(description="Rig center")] = Bounds()
    axes: Annotated[RigAxes, msgspec.Meta(description="Azimuth and elevation axes")] = CameraAxes()
    linked: Annotated[bool, msgspec.Meta(title="Target Alone", description="Lights reach target and descendants alone")] = False
    reference: Annotated[bpy.types.Image | None, msgspec.Meta(description="Image a matched rig reads")] = None
    recipe: Annotated[ReferenceImage, msgspec.Meta(title="Reference Recipe", description="Editable scene-linear lighting reference")] = ReferenceImage()
    size: Annotated[Side, msgspec.Meta(title="Reference Size")] = 256


class RigPick(msgspec.Struct, frozen=True, kw_only=True):
    """Catalog rig name."""

    name: Annotated[str, msgspec.Meta(title="Rig", description="Catalog rig"), rigs] = ""


class ReferencePick(msgspec.Struct, frozen=True, kw_only=True):
    """Reference recipe name from catalog."""

    name: Annotated[str, msgspec.Meta(title="Reference"), references] = ""


class ReferenceDraw(msgspec.Struct, frozen=True, kw_only=True):
    """Reference seed and color distribution."""

    distribution: ReferenceDistribution = ReferenceDistribution.BALANCED
    seed: Seed = 0


class GelPick(msgspec.Struct, frozen=True, kw_only=True):
    """Gel name for reference tint."""

    name: Annotated[str, msgspec.Meta(title="Gel"), gels] = ""


class RigFrame(msgspec.Struct, frozen=True, kw_only=True):
    """Rig world matrix with right, up, back, and center columns, and bounding radius."""

    matrix: Matrix
    radius: Positive


class Placing(msgspec.Struct, frozen=True, kw_only=True):
    """Rig frame, lights with powers, and background environment."""

    frame: RigFrame
    lights: tuple[tuple[Placement, float], ...]
    background: Option[tuple[Environment, Graph]]


class LightRig(msgspec.Struct, frozen=True, kw_only=True):
    """Rig setup attachment with owned lights, collection, and world."""

    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing
    scale: float

    @staticmethod
    def replaced(rig: Handle[bpy.types.Object], identity: Identity) -> BlenderResult[Option[str]]:
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
    def referenced[T: msgspec.Struct](call: Call[T], recipe: Callable[[RigSetup], ReferenceImage]) -> BlenderResult[Done]:
        """Renders recipe into owned scene-linear setup image, removing unused reference images."""

        def stored(found: tuple[bpy.types.Object, PyOpenColorIO.Config]) -> BlenderResult[int]:
            holder, config = found

            def imaged(setup: RigSetup) -> BlenderResult[RigSetup]:
                chosen = recipe(setup)
                return generated(
                    LightRig.reference,
                    call.identity,
                    f"{holder.name} Reference",
                    content_hash(LightRig, chosen, arrays=(np.asarray(setup.size, dtype=np.int64),), config=Some(config)),
                    lambda: chosen.plane(setup.size).map(Some),
                    Tag(space=Some(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()), view_as_render=Some(True)),
                    of=Some(holder),
                ).map(lambda image: msgspec.structs.replace(setup, recipe=chosen, reference=image.to_optional()))

            return LightRig.attachment.revised(call.identity, holder, imaged).bind(lambda _: LightRig.reference.removed(call.identity))

        return paired(call.root(bpy.types.ViewLayer).bind(focused), config_of()).bind(stored).map(lambda _: Done())

    @staticmethod
    def served(identity: Identity, item: bpy.types.Object) -> BlenderResult[bpy.types.Object]:
        """Returns rig empty target, or item when it is no rig empty."""
        return (
            LightRig.empty
            .marked(identity, item)
            .map(lambda mark: mark.of.map(lambda target: typed(target, bpy.types.Object, f"{item!r}.target")).default_with(lambda: Error((BlenderAbsentFault(owner=repr(item), key="target"),))))
            .default_value(Ok(item))
        )

    @staticmethod
    def placed(call: Call[Empty]) -> BlenderResult[Done]:
        """Places rig about active object, or about target of active rig empty."""
        return (
            call
            .root(bpy.types.ViewLayer)
            .bind(focused)
            .bind(lambda item: LightRig.served(call.identity, item))
            .bind(lambda holder: rigged(holder, LightRig.empty.found(call.identity, of=Some(holder)), call.context, call.identity))
            .map(lambda lights: Done(notes=(("INFO", f"Placed {len(lights)} lights"),)))
        )

    def jobs(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Returns preview jobs for catalog images and gray balls in scene display."""

        def requested(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, holder: bpy.types.Object, attached: Attached[RigSetup], config: PyOpenColorIO.Config) -> BlenderResult[tuple[Job, ...]]:
            view, shown = scene.view_settings, scene.display_settings
            previews = PREVIEWS.of(identity)
            display = DisplayView(display=shown.display_device, view=view.view_transform, look=Gated(enabled=Option.of_optional(config.getLook(view.look)).is_some(), value=view.look))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            exposure, gamma = view.exposure, view.gamma  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            display_key = (display, exposure, gamma, config.getCacheID())

            def rendered(source: ReferenceImage | tuple[DistantLight, ...], side: int) -> BlenderResult[Option[npt.NDArray[np.float32]]]:
                result = source.plane(side) if isinstance(source, ReferenceImage) else sphere(source, side)
                return result.map(Some)

            def job(slot: str, key: Hashable, rendered: Callable[[int], BlenderResult[Option[npt.NDArray[np.float32]]]], rank: Rank = Rank.REST) -> tuple[Job, ...]:
                def formed(side: int) -> BlenderResult[Option[npt.NDArray[np.float32]]]:
                    return rendered(side).bind(
                        lambda plane: plane.map(
                            lambda values: display.form(config, np.pad(values, ((0, 0), (0, 0), (0, 4 - values.shape[2])), constant_values=1.0), exposure=exposure, gamma=gamma).map(Some)
                        ).default_value(Ok(Nothing))
                    )

                return (
                    ()
                    if previews.current(slot, key, self.scale)
                    else (Job(place=slot, key=key, rank=rank, run=lambda: previews.tile(slot, key, lambda side: formed(side).map(Some), self.scale).map(lambda _: None)),)
                )

            def prepared(setup: RigSetup) -> tuple[Job, ...]:
                origin = LightRig.empty.marked(identity, holder).bind(lambda mark: mark.of).default_value(holder)
                measured = paired(typed(origin, bpy.types.Object, "rig.target").bind(lambda origin: target(origin, setup, context, scene, layer)), _reading(setup, config))
                inputs = (setup.focus, setup.axes, Handle.of(holder), Option.of_optional(setup.reference).map(Handle.of), measured.to_option())

                def ball(rig: Rig, side: int) -> BlenderResult[Option[npt.NDArray[np.float32]]]:
                    return measured.bind(lambda pair: rig.placements(config, pair[0].radius, pair[1])).bind(
                        lambda placements: placements.map(lambda placement: gray_ball(placement, config, side).map(Some)).default_value(Ok(Nothing))
                    )

                return (
                    *job("reference", (setup.recipe, *display_key), partial(rendered, setup.recipe), Rank.ACTIVE),
                    *job("rig", (setup.rig, *inputs, *display_key), partial(ball, setup.rig), Rank.ACTIVE),
                    *(row for recipes in ReferenceImage.catalog().values() for name, recipe in recipes.items() for row in job(f"reference:{name}", (recipe, *display_key), partial(rendered, recipe))),
                    *(row for name, rig in rig_catalog().items() for row in job(f"rig:{name}", (rig, *inputs, *display_key), partial(ball, rig))),
                    *(
                        row
                        for name, color in ReferenceImage.gels().items()
                        for row in job(f"gel:{name}", (color, *display_key), partial(rendered, (DistantLight(direction=(0.0, 0.0, 1.0), color=color),)))
                    ),
                )

            return attached.read(holder).map(prepared)

        return (
            root_in(bpy.types.Scene, context)
            .bind(lambda scene: root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active).map(lambda holder: (scene, layer, holder))))
            .map(lambda held: paired(LightRig.attachment.bound(identity), config_of()).bind(lambda found: requested(*held, *found)))
            .default_value(Ok(()))
        )

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Returns reference and rig previews with catalog button panels."""
        previews = PREVIEWS.of(identity)
        catalogs = (
            *(
                (family.label, tuple((f"reference:{name}", OperatorButton(target=Own(command=rig_reference_pick, values=ReferencePick(name=name)), text=name)) for name in recipes))
                for family, recipes in ReferenceImage.catalog().items()
            ),
            ("Rigs", tuple((f"rig:{name}", OperatorButton(target=Own(command=rig_pick, values=RigPick(name=name)), text=name)) for name in rig_catalog())),
            ("Gels", tuple((f"gel:{name}", OperatorButton(target=Own(command=rig_reference_gel, values=GelPick(name=name)), text=name)) for name in ReferenceImage.gels())),
        )

        def drawn(groups: tuple[tuple[str, tuple[tuple[str, RowDraw], ...]], ...]) -> RowDraw:
            def draw(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                active = layout.row()
                for slot in ("reference", "rig"):
                    for icon in previews.icon_id(slot).to_list():
                        active.template_icon(icon_value=icon, scale=self.scale)
                for title, rows in groups:
                    header, body = layout.panel(f"{identity.package}.lights.{title}", default_closed=True)
                    header.label(text=title)
                    if body is None:
                        continue
                    grid = body.grid_flow(row_major=True, columns=0, even_columns=True)
                    for slot, button in rows:
                        cell = grid.column()
                        for icon in previews.icon_id(slot).to_list():
                            cell.template_icon(icon_value=icon, scale=self.scale)
                        button(cell, holder, context)

            return draw

        return accumulated(accumulated(button.drawn(identity).map(lambda draw: (slot, draw)) for slot, button in rows).map(lambda rows: (title, rows)) for title, rows in catalogs).map(drawn)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Yields attachment, ownerships, previews, queue, reference handler, and section.

        Args:
            identity: Identity of extension registering row.

        Yields:
            Acquisitions with `Band` pairs.
        """
        with identity.reports.deferred(LightRig.replaced) as deferred:

            def referenced(_: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> BlenderResult[None]:
                touched = frozenset(block.original for update in updates if (block := update.id) is not None)

                def placed(attached: Attached[RigSetup]) -> BlenderResult[None]:
                    return accumulated(
                        attached.read(rig).map(lambda setup: deferred(Handle.of(rig), identity) if setup.reference in touched else None) for rig, _ in LightRig.empty.held(identity)
                    ).map(lambda _: None)

                return LightRig.attachment.bound(identity).bind(placed)

            yield Ok((
                LightRig.attachment,
                LightRig.empty,
                LightRig.data,
                LightRig.collection,
                LightRig.world,
                LightRig.reference,
                PREVIEWS,
                (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
                UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Image,), body=referenced),
                self.section(),
            ))

    def section(self) -> Section:
        """Returns active rig setup with command buttons in header."""
        reference = vars(RigSetup)["reference"]
        return Single(
            key="light_rig",
            title="Light Rig",
            rows=(self,),
            header=tuple(
                OperatorButton(target=command)
                for command in (rig_place, rig_pick, rig_arrange, rig_remove, rig_reference_pick, rig_reference_draw, rig_reference_roll, rig_reference_gel, rig_reference_apply)
            ),
            store=Some(
                Hosted(
                    host=Some(Member(root=bpy.types.LayerObjects, path="active")),
                    store=Store(
                        attachment=LightRig.attachment,
                        retention=self.retention,
                        fields={reference: lambda member: Selector(target=member, browse=DataBlocks(open=Some(bpy.ops.image.open)), preview=Some(PreviewGrid()))},
                    ),
                )
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def target(holder: bpy.types.Object, setup: RigSetup, context: bpy.types.Context, scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> BlenderResult[RigFrame]:
    """Returns rig world frame and bounding radius.

    Args:
        holder: Target object.
        setup: Focus and axes cases.
        context: Context supplying window manager for view axes.
        scene: Scene supplying camera and cursor.
        layer: View layer evaluating target and descendants.

    Returns:
        Frame, or `absent` for missing bounds, bone, camera, or view, `choice` for a non-3D view, `read` for zero radius.
    """

    def centered(bounds: WorldBounds) -> BlenderResult[Vector]:
        match setup.focus:
            case Bounds():
                return Ok((bounds.low + bounds.high) / 2.0)
            case Origin():
                return Ok(holder.matrix_world.translation.copy())
            case BoneHead(bone=bone):
                pose: Option[bpy.types.PoseBone] = Option.of_optional(holder.pose).bind(lambda pose: Option.of_optional(pose.bones.get(bone)))
                return pose.map(lambda pose: holder.matrix_world @ pose.head).to_result_with(lambda: (BlenderAbsentFault(owner=repr(holder), key=bone),))
            case Cursor():
                return Ok(scene.cursor.location.copy())

    def framed() -> BlenderResult[Matrix]:
        world = Matrix(((1.0, 0.0, 0.0), (0.0, 0.0, -1.0), (0.0, 1.0, 0.0)))

        def viewed(found: Located) -> BlenderResult[Matrix]:
            match found.area.spaces.active:
                case bpy.types.SpaceView3D(region_3d=bpy.types.RegionView3D(view_matrix=matrix)):
                    return Ok(matrix.inverted().to_3x3().normalized())
                case space:
                    return Error((BlenderChoiceFault(path=f"{found.area!r}.spaces.active.region_3d", identifier=type(space).__name__, accepted=(bpy.types.SpaceView3D.__name__,)),))

        match setup.axes:
            case CameraAxes():
                return Option.of_optional(scene.camera).map(lambda camera: camera.matrix_world.to_3x3().normalized()).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key="camera"),))
            case ViewAxes():
                return rooted(bpy.types.WindowManager, context).bind(lambda manager: AreaQuery(space="VIEW_3D").first(manager.windows)).bind(viewed)
            case TargetAxes():
                return Ok(holder.matrix_world.to_3x3().normalized() @ world)
            case WorldAxes():
                return Ok(world)

    def measured(carried: tuple[WorldBounds, Vector, Matrix]) -> BlenderResult[RigFrame]:
        bounds, center, frame = carried
        try:
            radius = msgspec.convert(max((Vector(corner) - center).length for corner in itertools.product(*zip(bounds.low, bounds.high))), type=Positive)  # type: ignore[call-overload]  # ty: ignore[invalid-argument-type]
        except msgspec.ValidationError as error:
            return Error((ImagingFileFault(access=Access.READ, request=f"{holder!r}.bound_box", error=error),))
        return Ok(RigFrame(matrix=Matrix.LocRotScale(center, frame, None).freeze(), radius=radius))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    return (
        evaluated(layer)
        .bind(lambda depsgraph: extent((holder, *descendants(holder, layer)), depsgraph).to_result_with(lambda: (BlenderAbsentFault(owner=repr(holder), key="bound_box"),)))
        .bind(lambda bounds: paired(centered(bounds), framed()).map(lambda pair: (bounds, *pair)))
        .bind(measured)
    )


def fitted(holder: bpy.types.Object, rig: bpy.types.Object, frame: RigFrame) -> BlenderResult[None]:
    """Writes rig empty placement, display, and target constraint.

    Args:
        holder: Target object.
        rig: Owned rig empty.
        frame: World frame and radius from `target`.

    Returns:
        `Ok` once written, or write faults.
    """
    display = Snapshot.of(bpy.types.Object, {"empty_display_type": "SPHERE", "empty_display_size": frame.radius})
    follow = Snapshot.of(bpy.types.ChildOfConstraint, {"target": IdName.of(holder), "inverse_matrix": tuple(itertools.chain.from_iterable(holder.matrix_world.inverted().transposed()))})  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    return reconciled(ObjectState(values=Some(display), placement=Some(frame.matrix), constraints=Some({"Follow": StackEntry(kind="CHILD_OF", values=Some(follow))})), rig, ()).map(lambda _: None)


def rigged(holder: bpy.types.Object, existing: Option[bpy.types.Object], context: bpy.types.Context, identity: Identity) -> BlenderResult[tuple[bpy.types.Object, ...]]:
    """Places rig about target, rolling back created data and world swap on fault.

    Args:
        holder: Target object.
        existing: Rig empty to place again from stored setup, `Nothing` to build one from target setup.
        context: Context supplying scene, view layer, and window manager for view axes.
        identity: Identity binding attachment and marking owned data.

    Returns:
        Lights in placement order, or `absent`, setup read, `target`, reference, placement, and write faults.
    """

    def placing(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, config: PyOpenColorIO.Config, rollback: ExitStack) -> BlenderResult[tuple[bpy.types.Object, ...]]:
        def measured(setup: RigSetup) -> BlenderResult[Placing]:
            def prepared(found: tuple[RigFrame, Option[LightingReading]]) -> BlenderResult[Placing]:
                frame, reading = found
                return (
                    Result[Option[Placements], BlenderFaults]
                    .bind(setup.rig.placements(config, frame.radius, reading), lambda placements: placements.to_result_with(lambda: (BlenderAbsentFault(owner=repr(setup), key="reference"),)))
                    .bind(
                        lambda placements: paired(
                            accumulated(placement.power for placement in placements.lights),
                            placements.background
                            .map(lambda background: Environment(lighting=Backdrop(mode=FlatColor(color=background.color), strength=background.strength)))
                            .map(lambda environment: world_graph(environment).map(lambda graph: Some((environment, graph))))
                            .default_value(Ok(Nothing)),
                        ).map(lambda pair: Placing(frame=frame, lights=tuple(zip(placements.lights, pair[0])), background=pair[1]))
                    )
                )

            return paired(target(holder, setup, context, scene, layer), _reading(setup, config)).bind(prepared)

        def lights(rig: bpy.types.Object, home: bpy.types.Collection, placing: Placing, *, alone: bool) -> BlenderResult[tuple[bpy.types.Object, ...]]:
            aim = Snapshot.of(bpy.types.TrackToConstraint, {"target": IdName.of(rig), "track_axis": "TRACK_NEGATIVE_Z", "up_axis": "UP_Y"})
            rows = tuple(Ownership(kind=bpy.types.Object, key=f"light_{index}", saved=Saved.USED) for index in range(len(placing.lights)))
            keys = frozenset(row.key for row in rows)
            stale = tuple(block for block, mark in _serving(rig, identity) if mark.key not in keys)

            def emitted(placement: Placement, power: float) -> tuple["rna_enums.LightTypeItems", dict[str, Value]]:
                light = placement.light
                common: dict[str, Value] = {"energy": power, "color": msgspec.structs.astuple(light.color), "use_temperature": light.temperature.enabled, "temperature": light.temperature.value}
                match light.emitter:
                    case Point():
                        return "POINT", common | {"shadow_soft_size": placement.radius}
                    case Spot(cone=cone, blend=blend):
                        return "SPOT", common | {"shadow_soft_size": placement.radius, "spot_size": cone, "spot_blend": blend}
                    case Disk():
                        return "AREA", common | {"shape": "DISK", "size": 2.0 * placement.radius}
                    case Sun():
                        return "SUN", common | {"angle": light.size}

            def spawned_light(row: Ownership[bpy.types.Object], placement: Placement, power: float) -> BlenderResult[bpy.types.Object]:
                kind, values = emitted(placement, power)
                label = f"{holder.name} {placement.light.role.label}"

                def stated(item: bpy.types.Object) -> ObjectState:
                    return ObjectState(
                        parent=Some(Handle.of(rig)),
                        values=Some(Snapshot.of(bpy.types.Object, {"lightgroup": item.name})),
                        placement=Some(Matrix.Translation(placement.location)),
                        constraints=Some({"Aim": StackEntry(kind="TRACK_TO", values=Some(aim))}),
                    )

                def built() -> BlenderResult[bpy.types.Object]:
                    return (
                        BlenderEditFault
                        .attempted(label, lambda: bpy.data.lights.new(vacant(label, bpy.data.lights.keys()), kind))
                        .bind(lambda light: LightRig.data.owned(identity, light, of=Some(rig), scope=Some(rollback)))
                        .bind(lambda light: spawned(label, Some(light), home))
                    )

                def configured(item: bpy.types.Object) -> BlenderResult[bpy.types.Object]:
                    return typed(item.data, bpy.types.Light, f"{item!r}.data").bind(
                        lambda light: (
                            written(light, "type", kind)
                            .bind(lambda _: reconciled(stated(item), item, ()))
                            .bind(lambda _: Snapshot.of(type(data := light.type_recast()), values).apply(data).outcome)
                            .map(lambda _: item)
                        )
                    )

                return row.made(identity, built, of=Some(rig), scope=Some(rollback)).bind(configured)

            def joined_to(prepared: tuple[tuple[bpy.types.Object, ...], None]) -> BlenderResult[tuple[bpy.types.Object, ...]]:
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

        def placed(attached: Attached[RigSetup], setup: RigSetup, placing: Placing) -> BlenderResult[tuple[bpy.types.Object, ...]]:
            def empty(home: bpy.types.Collection) -> BlenderResult[bpy.types.Object]:
                return existing.map(Ok).default_with(
                    lambda: spawned(f"{holder.name} Rig", Nothing, home).bind(
                        lambda rig: LightRig.empty.owned(identity, rig, of=Some(holder), filled=lambda rig: attached.write(rig, setup).map(lambda _: rig), scope=Some(rollback))
                    )
                )

            return LightRig.collection.made(identity, lambda: founded(Some(scene.collection), f"{holder.name} Rig"), of=Some(holder), scope=Some(rollback)).bind(
                lambda home: empty(home).bind(lambda rig: fitted(holder, rig, placing.frame).bind(lambda _: lights(rig, home, placing, alone=setup.linked)))
            )

        return LightRig.attachment.bound(identity).bind(
            lambda attached: attached.read(existing.default_value(holder)).bind(lambda setup: measured(setup).bind(lambda planned: placed(attached, setup, planned)))
        )

    with ExitStack() as rollback:
        return committed(paired(rooted(bpy.types.Scene, context), rooted(bpy.types.ViewLayer, context), config_of()).bind(lambda found: placing(*found, rollback)), rollback)


def rig_removed(rig: bpy.types.Object, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[int]:
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


def _rig(view_layer: bpy.types.ViewLayer, identity: Identity) -> BlenderResult[bpy.types.Object]:
    """Returns active object marked as rig empty."""
    return focused(view_layer).bind(lambda item: Ok(item) if LightRig.empty.marked(identity, item).is_some() else Error((BlenderAbsentFault(owner=repr(item), key=LightRig.empty.key),)))


def _serving(rig: bpy.types.Object, identity: Identity) -> tuple[tuple[bpy.types.Object, Mark], ...]:
    """Returns objects with marks naming rig empty."""
    return tuple((block, mark) for block in bpy.data.objects for mark in Mark.read(identity, block).to_list() if mark.of == Some(rig))


def _reading(setup: RigSetup, config: PyOpenColorIO.Config) -> BlenderResult[Option[LightingReading]]:
    """Returns reference lighting reading, `Nothing` without reference or measurable reading."""
    return Option.of_optional(setup.reference).map(lambda image: read(image, config).bind(lambda file: LightingReading.of(config, file.pixels))).default_value(Ok(Nothing))


# --- [COMPOSITION] ----------------------------------------------------------------------

rig_place: Operator[Empty] = Operator(key="rig_place", title="Place Rig", description="Place rig about active object", form=Empty, run=LightRig.placed)
rig_pick: Operator[RigPick] = Operator(
    key="rig_catalog",
    title="Choose Rig",
    description="Set active rig from catalog",
    form=RigPick,
    invoke=Some(InvokePopup.SEARCH),
    run=lambda call: (
        call
        .root(bpy.types.ViewLayer)
        .bind(focused)
        .bind(lambda rig: LightRig.attachment.revised(call.identity, rig, lambda setup: Ok(msgspec.structs.replace(setup, rig=rig_catalog()[call.record.name]))))
        .map(lambda _: Done())
    ),
)
rig_arrange: Operator[Empty] = Operator(
    key="rig_arrange",
    title="Arrange Rig",
    description="Convert active rig matched lights to arranged lights",
    form=Empty,
    run=lambda call: (
        paired(call.root(bpy.types.ViewLayer).bind(lambda layer: _rig(layer, call.identity)), config_of())
        .bind(
            lambda found: LightRig.attachment.revised(
                call.identity,
                found[0],
                lambda setup: (
                    _reading(setup, found[1])
                    .bind(setup.rig.arranged)
                    .bind(lambda arranged: arranged.to_result_with(lambda: (BlenderAbsentFault(owner=repr(found[0]), key="reference"),)))
                    .map(lambda arranged: msgspec.structs.replace(setup, rig=msgspec.structs.replace(setup.rig, layout=arranged)))
                ),
            )
        )
        .map(lambda _: Done())
    ),
)
rig_remove: Operator[Empty] = Operator(
    key="rig_remove",
    title="Remove Rig",
    description="Remove active rig lights, empty, and collection",
    form=Empty,
    run=lambda call: (
        paired(call.root(bpy.types.ViewLayer).bind(lambda layer: _rig(layer, call.identity)), call.root(bpy.types.Scene))
        .bind(lambda pair: rig_removed(pair[0], call.identity, pair[1]))
        .map(lambda count: Done(notes=(("INFO", f"Removed {count} rig data-blocks"),)))
    ),
)
rig_reference_pick: Operator[ReferencePick] = Operator(
    key="rig_reference_catalog",
    title="Choose Reference",
    description="Render catalog reference as matched rig image",
    form=ReferencePick,
    invoke=Some(InvokePopup.SEARCH),
    run=lambda call: LightRig.referenced(call, lambda _: next(recipes[call.record.name] for recipes in ReferenceImage.catalog().values() if call.record.name in recipes)),
)
rig_reference_draw: Operator[ReferenceDraw] = Operator(
    key="rig_reference_draw",
    title="Draw Reference",
    description="Draw seeded reference",
    form=ReferenceDraw,
    invoke=Some(InvokePopup.REDO),
    run=lambda call: LightRig.referenced(call, lambda _: ReferenceImage.drawn(call.record.distribution, call.record.seed)),
)
rig_reference_roll: Operator[Roll] = Operator(
    key="rig_reference_roll",
    title="Randomize Reference",
    description="Randomize reference fields",
    form=Roll,
    invoke=Some(InvokePopup.REDO),
    run=lambda call: LightRig.referenced(call, lambda setup: setup.recipe.rolled(call.record)),
)
rig_reference_gel: Operator[GelPick] = Operator(
    key="rig_reference_gel",
    title="Choose Gel",
    description="Apply catalog gel to reference tint",
    form=GelPick,
    invoke=Some(InvokePopup.SEARCH),
    run=lambda call: LightRig.referenced(call, lambda setup: msgspec.structs.replace(setup.recipe, tint=ReferenceImage.gels()[call.record.name])),
)
rig_reference_apply: Operator[Empty] = Operator(
    key="rig_reference_apply", title="Render Reference", description="Render edited reference as matched rig image", form=Empty, run=lambda call: LightRig.referenced(call, lambda setup: setup.recipe)
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "BoneHead",
    "Bounds",
    "CameraAxes",
    "Cursor",
    "Focus",
    "GelPick",
    "LightRig",
    "Origin",
    "ReferenceDraw",
    "ReferencePick",
    "RigAxes",
    "RigFrame",
    "RigPick",
    "RigSetup",
    "TargetAxes",
    "ViewAxes",
    "WorldAxes",
    "fitted",
    "gels",
    "references",
    "rig_arrange",
    "rig_pick",
    "rig_place",
    "rig_reference_apply",
    "rig_reference_draw",
    "rig_reference_gel",
    "rig_reference_pick",
    "rig_reference_roll",
    "rig_remove",
    "rig_removed",
    "rigged",
    "rigs",
    "target",
]
