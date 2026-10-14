"""Light rigs placed as owned lights about a target, aimed, with their background world."""

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
from blender.bpy.data.ids.ownership import earliest, Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import IdName, root_in, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import UpdateHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderImagingFault, BlenderReadFault, BlenderRenderingFault, BlenderResult
from blender.bpy.nodes.graphs import Graph
from blender.bpy.render.previews.queue import Job, PreviewQueue, Rank
from blender.bpy.scene.collections import founded
from blender.bpy.scene.lights.emitters import focused
from blender.bpy.scene.lights.linking import LightLinks, link_state, linked
from blender.bpy.scene.objects import descendants, extent, ObjectState, reconciled, spawned, StackEntry, WorldBounds
from blender.bpy.scene.worlds import Backdrop, Environment, FlatColor, reverted, swapped, world_graph
from blender.bpy.settings.attachments import Attached, Attachment, reconciling
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import DataBlocks, Member, OperatorButton, PreviewGrid, RowDraw, Selector
from blender.bpy.ui.layout.sections import Hosted, Section, Single, Store
from blender.bpy.ui.resources.previews import Previews
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.imaging.analysis.lighting import LightingReading
from blender.imaging.color.processors import DisplayView
from blender.imaging.faults import accumulated, paired
from blender.imaging.hashing import content_hash
from blender.imaging.parameters import Case, Gated, Seed
from blender.rendering.history import Retention
from blender.rendering.lighting.references import DistantLight, gray_ball, ReferenceDistribution, ReferenceImage, Roll, sphere
from blender.rendering.lighting.rigs import Arranged, Disk, Placement, Placements, Point, Radius, Rig, rig_catalog, Spot, Sun
from blender.rendering.outputs.framing import Side

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Focus = Bounds | Origin | BoneHead | Cursor
type RigAxes = CameraAxes | ViewAxes | TargetAxes | WorldAxes


# --- [CHOICES] --------------------------------------------------------------------------

rigs = Choices(lambda identity, owner, context: ((name, name, "") for name in rig_catalog()))
references = Choices(lambda identity, owner, context: ((name, name, family.label) for family, recipes in ReferenceImage.catalog().items() for name in recipes))
gels = Choices(lambda identity, owner, context: ((name, name, "") for name in ReferenceImage.gels()))


# --- [MODELS] ---------------------------------------------------------------------------


class Bounds(Case, frozen=True, kw_only=True, tag=1):
    """Midpoint of the evaluated bounds of the target and its descendants."""

    title = "Bounds"


class Origin(Case, frozen=True, kw_only=True, tag=2):
    """World origin of the target."""

    title = "Origin"


class BoneHead(Case, frozen=True, kw_only=True, tag=3):
    """Head of one pose bone of an armature target."""

    title = "Bone"
    bone: Annotated[str, msgspec.Meta(title="Bone", description="Pose bone whose head the rig centers on")] = ""


class Cursor(Case, frozen=True, kw_only=True, tag=4):
    """Location of the 3D cursor."""

    title = "3D Cursor"


class CameraAxes(Case, frozen=True, kw_only=True, tag=1):
    """Right, up, and back axes of the scene camera."""

    title = "Camera"


class ViewAxes(Case, frozen=True, kw_only=True, tag=2):
    """Right, up, and back axes of the 3D view."""

    title = "View"


class TargetAxes(Case, frozen=True, kw_only=True, tag=3):
    """Right, up, and back axes of the target object."""

    title = "Target"


class WorldAxes(Case, frozen=True, kw_only=True, tag=4):
    """World +X right, +Z up, and -Y back."""

    title = "World"


class RigSetup(msgspec.Struct, frozen=True, kw_only=True):
    """Rig, focus, axes, linking, and reference of one rig empty."""

    rig: Annotated[Rig, msgspec.Meta(title="Rig", description="Lights placed about the target")] = Rig(layout=Arranged())
    focus: Annotated[Focus, msgspec.Meta(title="Focus", description="Point the rig centers on")] = Bounds()
    axes: Annotated[RigAxes, msgspec.Meta(title="Axes", description="Frame the rig's azimuth and elevation turn in")] = CameraAxes()
    linked: Annotated[bool, msgspec.Meta(title="Target Alone", description="Rig lights reach the target and its descendants alone")] = False
    reference: Annotated[bpy.types.Image | None, msgspec.Meta(title="Reference", description="Image a matched rig reads")] = None
    recipe: Annotated[ReferenceImage, msgspec.Meta(title="Reference Recipe", description="Editable scene-linear lighting reference")] = ReferenceImage()
    size: Annotated[Side, msgspec.Meta(title="Reference Size")] = 256


class RigPick(msgspec.Struct, frozen=True, kw_only=True):
    """Named catalog rig the catalog operator writes."""

    name: Annotated[str, msgspec.Meta(title="Rig", description="Named rig of the catalog"), rigs] = ""


class ReferencePick(msgspec.Struct, frozen=True, kw_only=True):
    """Named reference recipe from its native catalog."""

    name: Annotated[str, msgspec.Meta(title="Reference"), references] = ""


class ReferenceDraw(msgspec.Struct, frozen=True, kw_only=True):
    """Seed and color distribution for an editable reference."""

    distribution: ReferenceDistribution = ReferenceDistribution.BALANCED
    seed: Seed = 0


class GelPick(msgspec.Struct, frozen=True, kw_only=True):
    """Native gel applied to a reference recipe's tint."""

    name: Annotated[str, msgspec.Meta(title="Gel"), gels] = ""


class Anchor(msgspec.Struct, frozen=True, kw_only=True):
    """World frame a rig places about: center and right, up, and back columns, with the bounding radius."""

    matrix: Matrix
    radius: Radius


class LightRig(msgspec.Struct, frozen=True, kw_only=True):
    """Row attaching `RigSetup` to rig empties with the owned lights, collection, and world."""

    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing
    scale: float

    @staticmethod
    def replaced(rig: Handle[bpy.types.Object], identity: Identity) -> BlenderResult[Option[str]]:
        """Places a rig again from the setup its empty stores, the write a setup edit or a reference change defers.

        Args:
            rig: Handle of the rig empty.
            identity: Identity binding the attachment and marking owned data.

        Returns:
            `Ok` of `Nothing`, a placement pushing no undo step; `absent` for an empty the file no longer holds, `register`, `read`, and the faults of `rig_placed`.
        """
        context = bpy.context

        def retained(block: bpy.types.Collection) -> bpy.types.Collection:
            rollback.callback(bpy.data.batch_remove, ids=(block,))
            return block

        def restored(carried: tuple[tuple[bpy.types.Object, Attached[RigSetup]], tuple[bpy.types.Scene, bpy.types.ViewLayer]]) -> BlenderResult[Option[str]]:
            (empty, attached), (scene, layer) = carried
            config = PyOpenColorIO.GetCurrentConfig()

            def collected(holder: bpy.types.Object) -> BlenderResult[bpy.types.Collection]:
                mark = Mark(key=LightRig.collection.key, of=Some(holder))
                return (
                    Option
                    .of_optional(earliest(LightRig.collection.held(identity)).get(mark))
                    .map(Ok)
                    .default_with(lambda: founded(Some(scene.collection), f"{holder.name} Rig").bind(lambda block: LightRig.collection.owned(identity, block, mark)).map(retained))
                )

            return (
                paired(
                    attached.read(empty),
                    Result[bpy.types.ID, BlenderFaults].bind(
                        Mark.read(identity, empty).bind(lambda mark: mark.of).to_result_with(lambda: (BlenderAbsentFault(owner=repr(empty), key=LightRig.empty.key),)),
                        lambda block: (
                            Ok(block)
                            if isinstance(block, bpy.types.Object)
                            else Error((BlenderChoiceFault(path=f"{empty!r}.target", identifier=type(block).__name__, accepted=(bpy.types.Object.__name__,)),))
                        ),
                    ),
                )
                .bind(lambda pair: paired(target(pair[1], pair[0], context, scene, layer), _reading(pair[0], config)).map(lambda measured: (*pair, *measured)))
                .bind(lambda measured: LightRig.prepared(measured[0], config, measured[2].radius, measured[3]).map(lambda prepared: (*measured[:3], prepared)))
                .bind(
                    lambda prepared: collected(prepared[1]).bind(
                        lambda home: anchored(prepared[1], empty, prepared[2]).bind(
                            lambda _: rig_placed(empty, prepared[1], home, prepared[3], scene, layer, identity, target_alone=prepared[0].linked)
                        )
                    )
                )
                .map(lambda _: Nothing)
            )

        def published(result: Option[str]) -> Option[str]:
            rollback.pop_all()
            return result

        with ExitStack() as rollback:
            return (
                rig
                .resolve()
                .bind(
                    lambda empty: (
                        paired(
                            LightRig.attachment.bound(identity).map(lambda attached: (empty, attached)),
                            paired(
                                root_in(bpy.types.Scene, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key=bpy.types.Scene.__name__),)),
                                root_in(bpy.types.ViewLayer, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key=bpy.types.ViewLayer.__name__),)),
                            ),
                        ).bind(restored)
                        if Mark.read(identity, empty).filter(lambda mark: mark.key == LightRig.empty.key).is_some()
                        else Ok(Nothing)
                    )
                )
                .map(published)
            )

    attachment: ClassVar[Attachment[RigSetup]] = Attachment(key="rig", form=RigSetup, owner=bpy.types.Object, changed=Some(reconciling(bpy.types.Object, replaced)))
    empty: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key=attachment.key, saved=Saved.USED)
    data: ClassVar[Ownership[bpy.types.Light]] = Ownership(kind=bpy.types.Light, key=attachment.key, saved=Saved.USED, single=False)
    collection: ClassVar[Ownership[bpy.types.Collection]] = Ownership(kind=bpy.types.Collection, key=attachment.key, saved=Saved.USED)
    world: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key=attachment.key, saved=Saved.USED)
    reference: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="rig_reference", saved=Saved.USED)

    @staticmethod
    def prepared(
        setup: RigSetup, config: PyOpenColorIO.Config, radius: Radius, reading: Option[LightingReading]
    ) -> BlenderResult[tuple[Placements, tuple[float, ...], Option[tuple[Environment, Graph]]]]:
        """Prepares placements, independent powers, and the native world graph before host publication."""
        return (
            Result[Option[Placements], BlenderFaults]
            .bind(
                setup.rig.placements(config, radius, reading).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)),
                lambda placements: placements.to_result_with(lambda: (BlenderAbsentFault(owner=repr(setup), key="reference"),)),
            )
            .bind(
                lambda placements: paired(
                    accumulated(placement.power for placement in placements.lights).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)),
                    placements.background
                    .map(lambda background: Environment(lighting=Backdrop(mode=FlatColor(color=background.color), strength=background.strength)))
                    .map(lambda environment: world_graph(environment).map(lambda graph: Some((environment, graph))))
                    .default_value(Ok(Nothing)),
                ).map(lambda pair: (placements, pair[0], pair[1]))
            )
        )

    @staticmethod
    def referenced[T: msgspec.Struct](call: Call[T], recipe: Callable[[RigSetup], ReferenceImage]) -> BlenderResult[Done]:
        """Publishes a recipe's owned linear image and retains editable inputs."""
        config = PyOpenColorIO.GetCurrentConfig()

        def stored(pair: tuple[bpy.types.Object, Attached[RigSetup]]) -> BlenderResult[None]:
            holder, attached = pair

            def imaged(setup: RigSetup) -> BlenderResult[None]:
                chosen = recipe(setup)
                with ExitStack() as rollback:
                    rollback.callback(LightRig.reference.removed, call.identity)

                    def published(_: None) -> None:
                        LightRig.reference.removed(call.identity)
                        rollback.pop_all()

                    return (
                        generated(
                            LightRig.reference,
                            call.identity,
                            f"{holder.name} Reference",
                            content_hash(LightRig, chosen, arrays=(np.asarray(setup.size, dtype=np.int64),), config=Some(config)),
                            lambda: chosen.plane(setup.size).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)).map(Some),
                            Tag(space=Some(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()), view_as_render=Some(True)),
                            of=Some(holder),
                        )
                        .bind(lambda image: attached.write(holder, msgspec.structs.replace(setup, recipe=chosen, reference=image.to_optional())))
                        .map(published)
                    )

            return attached.read(holder).bind(imaged)

        return paired(call.root(bpy.types.ViewLayer).bind(focused), LightRig.attachment.bound(call.identity)).bind(stored).map(lambda _: Done())

    @staticmethod
    def placed(call: Call[Empty], scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> BlenderResult[Done]:
        """Admits the target, reference, placements, and powers before publishing the rig."""
        identity, context = call.identity, call.context
        config = PyOpenColorIO.GetCurrentConfig()

        def retained[T: bpy.types.ID](block: T) -> T:
            rollback.callback(bpy.data.batch_remove, ids=(block,))
            return block

        def collected(holder: bpy.types.Object) -> BlenderResult[bpy.types.Collection]:
            mark = Mark(key=LightRig.collection.key, of=Some(holder))
            return (
                Option
                .of_optional(earliest(LightRig.collection.held(identity)).get(mark))
                .map(Ok)
                .default_with(lambda: founded(Some(scene.collection), f"{holder.name} Rig").bind(lambda block: LightRig.collection.owned(identity, block, mark)).map(retained))
            )

        def placed(pair: tuple[bpy.types.Object, Attached[RigSetup]]) -> BlenderResult[tuple[bpy.types.Object, ...]]:
            holder, attached = pair
            mark = Mark(key=LightRig.empty.key, of=Some(holder))
            existing = Option.of_optional(earliest(LightRig.empty.held(identity)).get(mark))
            return (
                existing
                .map(attached.read)
                .default_with(lambda: attached.read(holder))
                .bind(lambda setup: paired(target(holder, setup, context, scene, layer), _reading(setup, config)).map(lambda pair: (setup, *pair)))
                .bind(lambda carried: LightRig.prepared(carried[0], config, carried[1].radius, carried[2]).map(lambda prepared: (carried[0], carried[1], prepared)))
                .bind(
                    lambda prepared: collected(holder).bind(
                        lambda home: (
                            existing
                            .map(Ok)
                            .default_with(
                                lambda: (
                                    spawned(f"{holder.name} Rig", Nothing, home)
                                    .bind(lambda rig: LightRig.empty.owned(identity, rig, mark))
                                    .map(retained)
                                    .bind(lambda rig: attached.write(rig, prepared[0]).map(lambda _: rig))
                                )
                            )
                            .bind(lambda rig: anchored(holder, rig, prepared[1]).map(lambda _: rig))
                            .bind(lambda rig: rig_placed(rig, holder, home, prepared[2], scene, layer, identity, target_alone=prepared[0].linked))
                        )
                    )
                )
            )

        def published(lights: tuple[bpy.types.Object, ...]) -> Done:
            rollback.pop_all()
            return Done(notes=(("INFO", f"Placed {len(lights)} lights"),))

        with ExitStack() as rollback:
            holder = (
                focused(layer)
                .bind(
                    lambda item: (
                        Mark
                        .read(identity, item)
                        .filter(lambda mark: mark.key == LightRig.empty.key)
                        .map(lambda mark: mark.of.to_result_with(lambda: (BlenderAbsentFault(owner=repr(item), key="target"),)))
                        .default_value(Ok(item))
                    )
                )
                .bind(
                    lambda item: (
                        Ok(item) if isinstance(item, bpy.types.Object) else Error((BlenderChoiceFault(path="rig.target", identifier=type(item).__name__, accepted=(bpy.types.Object.__name__,)),))
                    )
                )
            )
            return paired(holder, LightRig.attachment.bound(identity)).bind(placed).map(published)

    rig_pick: ClassVar[Operator[RigPick]] = Operator(
        key="rig_catalog",
        title="Choose Rig",
        description="Write a named rig into the active rig",
        form=RigPick,
        invoke=Some(InvokePopup.SEARCH),
        run=lambda call: (
            call
            .root(bpy.types.ViewLayer)
            .bind(focused)
            .bind(
                lambda rig: LightRig.attachment.bound(call.identity).bind(
                    lambda attached: attached.read(rig).bind(lambda setup: attached.write(rig, msgspec.structs.replace(setup, rig=rig_catalog()[call.record.name])))
                )
            )
            .map(lambda _: Done())
        ),
    )
    reference_pick: ClassVar[Operator[ReferencePick]] = Operator(
        key="rig_reference_catalog",
        title="Choose Reference",
        description="Choose a native reference recipe",
        form=ReferencePick,
        invoke=Some(InvokePopup.SEARCH),
        run=lambda call: LightRig.referenced(call, lambda _: next(recipes[call.record.name] for recipes in ReferenceImage.catalog().values() if call.record.name in recipes)),
    )
    gel_pick: ClassVar[Operator[GelPick]] = Operator(
        key="rig_reference_gel",
        title="Choose Gel",
        description="Apply a named native gel to the reference tint",
        form=GelPick,
        invoke=Some(InvokePopup.SEARCH),
        run=lambda call: LightRig.referenced(call, lambda setup: msgspec.structs.replace(setup.recipe, tint=ReferenceImage.gels()[call.record.name])),
    )
    commands: ClassVar[
        tuple[Operator[Empty], Operator[RigPick], Operator[Empty], Operator[Empty], Operator[ReferencePick], Operator[ReferenceDraw], Operator[Roll], Operator[GelPick], Operator[Empty]]
    ] = (
        Operator(
            key="rig_place",
            title="Place Rig",
            description="Place a light rig about the active object",
            form=Empty,
            run=lambda call: paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(lambda roots: LightRig.placed(call, *roots)),
        ),
        rig_pick,
        Operator(
            key="rig_arrange",
            title="Arrange Rig",
            description="Write the active rig's matched lights as stated lights",
            form=Empty,
            run=lambda call: (
                call
                .root(bpy.types.ViewLayer)
                .bind(lambda layer: _rig(layer, call.identity))
                .bind(
                    lambda rig: LightRig.attachment.bound(call.identity).bind(
                        lambda attached: attached.read(rig).bind(
                            lambda setup: (
                                _reading(setup, PyOpenColorIO.GetCurrentConfig())
                                .bind(lambda reading: setup.rig.arranged(reading).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)))
                                .bind(lambda arranged: arranged.to_result_with(lambda: (BlenderAbsentFault(owner=repr(rig), key="reference"),)))
                                .bind(lambda arranged: attached.write(rig, msgspec.structs.replace(setup, rig=msgspec.structs.replace(setup.rig, layout=arranged))))
                            )
                        )
                    )
                )
                .map(lambda _: Done())
            ),
        ),
        Operator(
            key="rig_remove",
            title="Remove Rig",
            description="Remove the active rig's lights, empty, and collection",
            form=Empty,
            run=lambda call: (
                paired(call.root(bpy.types.ViewLayer).bind(lambda layer: _rig(layer, call.identity)), call.root(bpy.types.Scene))
                .bind(lambda pair: rig_removed(pair[0], call.identity, pair[1]))
                .map(lambda count: Done(notes=(("INFO", f"Removed {count} rig data-blocks"),)))
            ),
        ),
        reference_pick,
        Operator(
            key="rig_reference_draw",
            title="Draw Reference",
            description="Draw a seeded balanced, vivid, or soft reference",
            form=ReferenceDraw,
            invoke=Some(InvokePopup.REDO),
            run=lambda call: LightRig.referenced(call, lambda _: ReferenceImage.drawn(call.record.distribution, call.record.seed)),
        ),
        Operator(
            key="rig_reference_roll",
            title="Randomize Reference",
            description="Draw reference fields from their native metadata",
            form=Roll,
            invoke=Some(InvokePopup.REDO),
            run=lambda call: LightRig.referenced(call, lambda setup: setup.recipe.rolled(call.record)),
        ),
        gel_pick,
        Operator(
            key="rig_reference_apply",
            title="Render Reference",
            description="Publish the edited reference recipe as the matched rig's image",
            form=Empty,
            run=lambda call: LightRig.referenced(call, lambda setup: setup.recipe),
        ),
    )

    def jobs(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Requests native catalog images and gray balls under current scene display settings."""

        def requested(
            carried: tuple[bpy.types.Scene, bpy.types.ColorManagedViewSettings, bpy.types.ColorManagedDisplaySettings, bpy.types.ViewLayer, bpy.types.Object, Attached[RigSetup]],
        ) -> BlenderResult[tuple[Job, ...]]:
            scene, view, shown, layer, holder, attached = carried
            config, previews = PyOpenColorIO.GetCurrentConfig(), Previews(identity=identity)
            display = DisplayView(display=shown.display_device, view=view.view_transform, look=Gated(enabled=Option.of_optional(config.getLook(view.look)).is_some(), value=view.look))
            exposure, gamma = view.exposure, view.gamma
            display_key = (display, exposure, gamma, config.getCacheID())

            def rendered(source: ReferenceImage | tuple[DistantLight, ...], side: int) -> BlenderResult[Option[npt.NDArray[np.float32]]]:
                result = source.plane(side) if isinstance(source, ReferenceImage) else sphere(source, (side, side))
                return result.map_error(lambda faults: (BlenderRenderingFault(faults=faults),)).map(Some)

            def job(slot: str, key: Hashable, rendered: Callable[[int], BlenderResult[Option[npt.NDArray[np.float32]]]], rank: Rank = Rank.REST) -> tuple[Job, ...]:
                def formed(side: int) -> BlenderResult[Option[npt.NDArray[np.float32]]]:
                    return rendered(side).bind(
                        lambda plane: plane.map(
                            lambda values: (
                                display
                                .form(config, np.pad(values, ((0, 0), (0, 0), (0, 4 - values.shape[2])), constant_values=1.0), exposure=exposure, gamma=gamma)
                                .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
                                .map(Some)
                            )
                        ).default_value(Ok(Nothing))
                    )

                return (
                    ()
                    if previews.current(slot, key, self.scale)
                    else (Job(place=slot, key=key, rank=rank, run=lambda: previews.tile(slot, key, lambda side: formed(side).map(Some), self.scale).map(lambda _: None)),)
                )

            def prepared(setup: RigSetup) -> tuple[Job, ...]:
                origin = Mark.read(identity, holder).filter(lambda mark: mark.key == LightRig.empty.key).bind(lambda mark: mark.of).default_value(holder)
                anchor = (
                    target(origin, setup, context, scene, layer)
                    if isinstance(origin, bpy.types.Object)
                    else Error((BlenderChoiceFault(path="rig.target", identifier=type(origin).__name__, accepted=(bpy.types.Object.__name__,)),))
                )
                reading = _reading(setup, config)
                measured = paired(anchor, reading)
                inputs = (setup.focus, setup.axes, Handle.of(holder), Option.of_optional(setup.reference).map(Handle.of), measured.to_option())

                def ball(rig: Rig, side: int) -> BlenderResult[Option[npt.NDArray[np.float32]]]:
                    return measured.bind(lambda pair: rig.placements(config, pair[0].radius, pair[1]).map_error(lambda faults: (BlenderRenderingFault(faults=faults),))).bind(
                        lambda placements: placements.map(
                            lambda placement: gray_ball(placement, config, side).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)).map(Some)
                        ).default_value(Ok(Nothing))
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
            .bind(lambda scene: Option.of_optional(scene.view_settings).bind(lambda view: Option.of_optional(scene.display_settings).map(lambda shown: (scene, view, shown))))
            .bind(lambda held: root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active).map(lambda holder: (*held, layer, holder))))
            .map(lambda held: LightRig.attachment.bound(identity).bind(lambda attached: requested((*held, attached))))
            .default_value(Ok(()))
        )

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Draws catalog choices and queued native previews with retained operator declarations."""
        previews = Previews(identity=identity)
        catalogs = (
            *(
                (family.label, tuple((f"reference:{name}", OperatorButton(target=Own(command=self.reference_pick, values=ReferencePick(name=name)), text=Some(name))) for name in recipes))
                for family, recipes in ReferenceImage.catalog().items()
            ),
            ("Rigs", tuple((f"rig:{name}", OperatorButton(target=Own(command=self.rig_pick, values=RigPick(name=name)), text=Some(name))) for name in rig_catalog())),
            ("Gels", tuple((f"gel:{name}", OperatorButton(target=Own(command=self.gel_pick, values=GelPick(name=name)), text=Some(name))) for name in ReferenceImage.gels())),
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
        """Acquires settings, ownership, catalog previews, commands, and reference updates.

        Args:
            identity: Extension placing the row.

        Yields:
            `Ok` of every band and acquisition pair, or every construction fault.
        """
        with identity.reports.deferred(LightRig.replaced) as deferred:

            def referenced(_: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> BlenderResult[None]:
                touched = frozenset(original for update in updates if (block := update.id) is not None and (original := block.original) is not None)

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
                (Band.RESOURCES, Previews(identity=identity)),
                (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
                UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Image,), body=referenced),
                self.section(),
            ))

    def section(self) -> Section:
        """Section drawing the active rig's setup with its place, catalog, arrange, and remove buttons."""
        reference = vars(RigSetup)["reference"]
        return Single(
            key="light_rig",
            title="Light Rig",
            rows=(self,),
            header=tuple(OperatorButton(target=command) for command in self.commands),
            store=Some(
                Hosted(
                    host=Member(root=bpy.types.LayerObjects, path="active"),
                    store=Store(
                        attachment=LightRig.attachment,
                        retention=self.retention,
                        fields={
                            reference: Selector(
                                target=next(field for field in msgspec.structs.fields(RigSetup) if field.name == reference.__name__),
                                browse=DataBlocks(open=Some(bpy.ops.image.open)),
                                preview=Some(PreviewGrid()),
                            )
                        },
                    ),
                )
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def target(holder: bpy.types.Object, setup: RigSetup, context: bpy.types.Context, scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> BlenderResult[Anchor]:
    """Answers the world frame and bounding radius a rig places about.

    Args:
        holder: Target object.
        setup: Focus and axes cases.
        context: Context whose view layer, camera, cursor, and windows the cases read.
        scene: Admitted scene supplying the camera and cursor.
        layer: Admitted view layer whose target descendants are evaluated.

    Returns:
        `Ok` of the center, positive radius, and frame; `absent` for missing host data, or `read` for a native radius conversion failure.
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
                return (
                    Result[bpy.types.WindowManager, BlenderFaults]
                    .bind(
                        root_in(bpy.types.WindowManager, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key=bpy.types.WindowManager.__name__),)),
                        lambda manager: AreaQuery(space="VIEW_3D").first(manager.windows),
                    )
                    .bind(viewed)
                )
            case TargetAxes():
                return Ok(holder.matrix_world.to_3x3().normalized() @ world)
            case WorldAxes():
                return Ok(world)

    def measured(carried: tuple[WorldBounds, Vector, Matrix]) -> BlenderResult[Anchor]:
        bounds, center, frame = carried
        try:
            radius = msgspec.convert(max((Vector(corner) - center).length for corner in itertools.product(*zip(bounds.low, bounds.high))), type=Radius)  # type: ignore[call-overload]  # ty: ignore[invalid-argument-type]
        except msgspec.ValidationError as error:
            return Error((BlenderReadFault(request=f"{holder!r}.bound_box", error=error),))
        return Ok(Anchor(matrix=Matrix.LocRotScale(center, frame, None).freeze(), radius=radius))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    return (
        evaluated(layer)
        .bind(lambda depsgraph: extent((holder, *descendants(holder, layer)), depsgraph).to_result_with(lambda: (BlenderAbsentFault(owner=repr(holder), key="bound_box"),)))
        .bind(lambda bounds: paired(centered(bounds), framed()).map(lambda pair: (bounds, *pair)))
        .bind(measured)
    )


def anchored(holder: bpy.types.Object, rig: bpy.types.Object, anchor: Anchor) -> BlenderResult[None]:
    """Writes the rig empty's placement, sphere display, and target constraint.

    Args:
        holder: Target object.
        rig: Owned rig empty.
        anchor: World frame and radius `target` answered.

    Returns:
        `Ok` after reconciliation, or the native write faults.
    """
    display = Snapshot.of(bpy.types.Object, {"empty_display_type": "SPHERE", "empty_display_size": anchor.radius})
    follow = Snapshot.of(bpy.types.ChildOfConstraint, {"target": IdName.of(holder), "inverse_matrix": tuple(value for column in holder.matrix_world.inverted().transposed() for value in column)})  # type: ignore[union-attr]

    return reconciled(ObjectState(values=Some(display), placement=Some(anchor.matrix), constraints=Some({"Follow": StackEntry(kind="CHILD_OF", values=Some(follow))})), rig, ()).map(lambda _: None)


def rig_placed(
    rig: bpy.types.Object,
    target: bpy.types.Object,
    home: bpy.types.Collection,
    prepared: tuple[Placements, tuple[float, ...], Option[tuple[Environment, Graph]]],
    scene: bpy.types.Scene,
    view_layer: bpy.types.ViewLayer,
    identity: Identity,
    *,
    target_alone: bool,
) -> BlenderResult[tuple[bpy.types.Object, ...]]:
    """Places a rig's lights in its empty's frame and shows or restores its background.

    Args:
        rig: Owned rig empty.
        target: Actual target admitted by the caller.
        home: Owned collection acquired once by the caller after native preparation.
        prepared: Native placements, ordered powers, and optional world graph accepted together before publication.
        scene: Scene showing the background.
        view_layer: View layer of the scene whose target descendants a linked rig reaches.
        identity: Identity marking lights, collection, and world.
        target_alone: Whether the lights reach the target and its descendants alone.

    Returns:
        `Ok` of the lights in placement order, or the faults of each write.
    """
    placements, powers, background = prepared
    receivers = next(link for link in LightLinks.sets if link.prop.identifier == "receiver_collection")
    aim = Snapshot.of(bpy.types.TrackToConstraint, {"target": IdName.of(rig), "track_axis": "TRACK_NEGATIVE_Z", "up_axis": "UP_Y"})

    def retained[T: bpy.types.ID](block: T) -> T:
        rollback.callback(bpy.data.batch_remove, ids=(block,))
        return block

    def emitted(placement: Placement, power: float) -> tuple["rna_enums.LightTypeItems", dict[str, Value]]:
        light = placement.light
        common: dict[str, Value] = {
            "energy": power,
            "color": (light.color.red, light.color.green, light.color.blue),
            "use_temperature": light.temperature.enabled,
            "temperature": light.temperature.value,
        }
        match light.emitter:
            case Point():
                return "POINT", {**common, "shadow_soft_size": placement.radius}
            case Spot(cone=cone, blend=blend):
                return "SPOT", {**common, "shadow_soft_size": placement.radius, "spot_size": cone, "spot_blend": blend}
            case Disk():
                return "AREA", {**common, "shape": "DISK", "size": 2.0 * placement.radius}
            case Sun():
                return "SUN", {**common, "angle": light.size}

    def lamp(index: int, placement: Placement, power: float) -> BlenderResult[bpy.types.Object]:
        kind, values = emitted(placement, power)
        label = placement.light.role.label
        row = Ownership(kind=bpy.types.Object, key=f"light_{index}", saved=Saved.USED)
        mark = Mark(key=row.key, of=Some(rig))

        def stated(item: bpy.types.Object) -> ObjectState:
            return ObjectState(
                parent=Some(Handle.of(rig)),
                values=Some(Snapshot.of(bpy.types.Object, {"lightgroup": item.name})),
                placement=Some(Matrix.Translation(placement.location)),
                constraints=Some({"Aim": StackEntry(kind="TRACK_TO", values=Some(aim))}),
            )

        def built() -> BlenderResult[bpy.types.Object]:
            try:
                light = bpy.data.lights.new(label, kind)
            except (RuntimeError, TypeError, ValueError) as error:
                return Error((BlenderEditFault(request=label, error=error),))
            return (
                LightRig.data
                .owned(identity, light, Mark(key=LightRig.data.key, of=Some(rig)))
                .map(retained)
                .bind(lambda light: spawned(label, Some(light), home))
                .bind(lambda item: row.owned(identity, item, mark))
                .map(retained)
            )

        def configured(item: bpy.types.Object) -> BlenderResult[bpy.types.Object]:
            light = item.data
            return (
                (
                    written(light, "type", kind)
                    .bind(lambda _: reconciled(stated(item), item, ()))
                    .bind(lambda _: Snapshot.of(type(data := light.type_recast()), values).apply(data).outcome)
                    .map(lambda _: item)
                )
                if isinstance(light, bpy.types.Light)
                else Error((BlenderChoiceFault(path=f"{item!r}.data", identifier=type(light).__name__, accepted=(bpy.types.Light.__name__,)),))
            )

        return Option.of_optional(earliest(row.held(identity)).get(mark)).map(Ok).default_with(built).bind(configured)

    def lit() -> BlenderResult[tuple[bpy.types.Object, ...]]:
        keys = frozenset(f"light_{index}" for index in range(len(placements.lights)))
        stale = tuple(block for block in bpy.data.objects for mark in Mark.read(identity, block).to_list() if mark.of == Some(rig) and mark.key not in keys)
        try:
            if stale:
                bpy.data.batch_remove(ids=stale)
            LightRig.data.removed(identity)
        except (ReferenceError, RuntimeError, TypeError, ValueError) as error:
            return Error((BlenderEditFault(request=repr(rig), error=error),))
        return accumulated(lamp(index, placement, power) for index, (placement, power) in enumerate(zip(placements.lights, powers)))

    def joined_to(prepared: tuple[tuple[bpy.types.Object, ...], None]) -> BlenderResult[tuple[bpy.types.Object, ...]]:
        lights, _ = prepared
        return (
            accumulated(linked(item, receivers, (target, *descendants(target, view_layer)), link_state.default, target, identity) for item in lights).map(lambda _: None)
            if target_alone
            else accumulated(receivers.write(item.light_linking, None) for item in lights).map(lambda _: None)
        ).map(lambda _: lights)

    def backed() -> BlenderResult[None]:
        return background.map(lambda prepared: swapped(LightRig.world, prepared, scene, identity).map(lambda _: None)).default_with(
            lambda: reverted(LightRig.world, scene, identity).map(lambda _: None)
        )

    def published(lights: tuple[bpy.types.Object, ...]) -> tuple[bpy.types.Object, ...]:
        rollback.pop_all()
        return lights

    with ExitStack() as rollback:
        return paired(lit(), backed()).bind(joined_to).map(published)


def rig_removed(rig: bpy.types.Object, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[int]:
    """Removes a rig's lights, empty, and collection and restores the world it replaced.

    Args:
        rig: Owned rig empty.
        identity: Identity reading the marks.
        scene: Scene whose world returns.

    Returns:
        `Ok` of the objects and collections removed; `edit` for a refused world write.
    """
    holder = Mark.read(identity, rig).bind(lambda mark: mark.of)
    blocks = (
        *(block for block in bpy.data.objects for mark in Mark.read(identity, block).to_list() if mark.of == Some(rig)),
        rig,
        *(held for held, mark in LightRig.collection.held(identity) if holder.is_some() and mark.of == holder),
    )

    def gone(_: Option[int]) -> BlenderResult[int]:
        try:
            bpy.data.batch_remove(ids=blocks)
            return Ok(len(blocks) + LightRig.data.removed(identity) + LightRig.reference.removed(identity))
        except (ReferenceError, RuntimeError, TypeError, ValueError) as error:
            return Error((BlenderEditFault(request=repr(rig), error=error),))

    return reverted(LightRig.world, scene, identity).bind(gone)


def _rig(view_layer: bpy.types.ViewLayer, identity: Identity) -> BlenderResult[bpy.types.Object]:
    """View layer's active object with a rig empty's mark, or an absent fault."""
    return focused(view_layer).bind(
        lambda item: Ok(item) if Mark.read(identity, item).filter(lambda mark: mark.key == LightRig.empty.key).is_some() else Error((BlenderAbsentFault(owner=repr(item), key=LightRig.empty.key),))
    )


def _reading(setup: RigSetup, config: PyOpenColorIO.Config) -> BlenderResult[Option[LightingReading]]:
    """Reading of a rig's reference image, or Nothing for no reference or measure."""
    return (
        Option
        .of_optional(setup.reference)
        .map(lambda image: read(image, config).bind(lambda file: LightingReading.of(config, file.pixels).map_error(lambda faults: (BlenderImagingFault(faults=faults),))))
        .default_value(Ok(Nothing))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Anchor",
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
    "RigPick",
    "RigSetup",
    "TargetAxes",
    "ViewAxes",
    "WorldAxes",
    "anchored",
    "gels",
    "references",
    "rig_placed",
    "rig_removed",
    "rigs",
    "target",
]
