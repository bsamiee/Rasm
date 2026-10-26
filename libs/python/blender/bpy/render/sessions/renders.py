"""Scene renders: the render call every render path takes, Blender's native render, and slot renders under temporary scene overrides."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import datetime
from functools import cached_property
from itertools import starmap
from pathlib import Path
from typing import Annotated, ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.calls import called, Stock
from blender.bpy.commands.operators import Call, Done, Operator, Properties
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.snapshots import Endpoints, Scenes, Snapshot
from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.trees import nested
from blender.bpy.render.color.spaces import config_of, viewed
from blender.bpy.render.outputs.exports import COMPOSITE
from blender.bpy.render.outputs.formats import pointer
from blender.bpy.render.outputs.framing import Framed, FrameSize
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.outputs.results import produced, Production
from blender.bpy.render.view_layers.passes import PassRouting, PassSet
from blender.bpy.settings.attachments import Attached, Attachment, Recovery
from blender.bpy.settings.history import Retention
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import Member, OperatorButton, Property, Pulldown
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.templates import ImageFormat
from blender.bpy.ui.layout.values import Hosted, Listing, Store
from blender.bpy.ui.surfaces.menus import Captured, PresetMenu
from blender.records.faults import accumulated, paired, scoped
from blender.records.fields import Choice, Gated
from blender.records.identity import content_hash, ContentHash

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Render = Callable[[Identity, bpy.types.Scene, RenderCall], BlenderResult[frozenset[rna_enums.OperatorReturnItems]]]


class Into(Choice):
    """Render Result slot receiving next render."""

    DISPLAYED = 1, "Displayed Slot"
    NEW = 2, "New Slot"


class Isolation(Choice):
    """Render state assigned to unselected scene objects."""

    HOLDOUT = 1, "Holdout"
    HIDDEN = 2, "Hidden"


# --- [CONSTANTS] ------------------------------------------------------------------------

HOLDOUT: Final = Descriptor.declared(bpy.types.Object, "is_holdout", bpy.types.BoolProperty)
VISIBLE_CAMERA: Final = Descriptor.declared(bpy.types.Object, "visible_camera", bpy.types.BoolProperty)
SHADOW_CATCHER: Final = Descriptor.declared(bpy.types.Object, "is_shadow_catcher", bpy.types.BoolProperty)
ENGINE: Final = Descriptor.declared(bpy.types.RenderSettings, "engine", bpy.types.EnumProperty)
value_sets: Final[Preset[Snapshot]] = Preset(key="render_override", form=Snapshot)
value_set_names: Final = Choices(value_sets.entries)
layers: Final = Choices(
    lambda _identity, _owner, context: (
        (layer.name, layer.name, "") for held in Option.of_optional(context).to_list() for scene in Option.of_optional(held.scene).to_list() for layer in scene.view_layers
    )
)

# --- [MODELS] ---------------------------------------------------------------------------


class RenderCall(msgspec.Struct, frozen=True, kw_only=True):
    """`render.render` options a render passes through, an absent layer rendering every layer."""

    animation: bool = False
    write_still: bool = False
    layer: Option[str] = Nothing


class Launch(msgspec.Struct, frozen=True, kw_only=True):
    """Render options and destination slot."""

    animation: Annotated[bool, msgspec.Meta(title="Animation")] = False
    write: Annotated[bool, msgspec.Meta(title="Write Still")] = False
    into: Annotated[Into, msgspec.Meta(title="Slot")] = Into.DISPLAYED
    layer: Annotated[Gated[Annotated[str, layers]], msgspec.Meta(title="View Layer")] = Gated(enabled=False, value="")


class ValueSet(msgspec.Struct, frozen=True, kw_only=True):
    """Scene value preset for one render."""

    name: Annotated[str, msgspec.Meta(title="Preset"), value_set_names]


class Isolated(msgspec.Struct, frozen=True, kw_only=True):
    """Isolation of unselected objects, shadow catching on selected."""

    mode: Annotated[Isolation, msgspec.Meta(title="Others")] = Isolation.HOLDOUT
    catcher: Annotated[bool, msgspec.Meta(title="Shadow Catcher")] = False


class RenderOverride(msgspec.Struct, frozen=True, kw_only=True):
    """Overrides applied for one render."""

    values: Annotated[Gated[ValueSet], msgspec.Meta(title="Value Set", description="Scene value preset applied for the render")] = Gated(enabled=False, value=ValueSet(name=""))
    isolated: Gated[Isolated] = Gated(enabled=False, value=Isolated())


class Overriding(msgspec.Struct, frozen=True):
    """Override store, frame size, pass routing, and recovery one render binds."""

    held: Attached[RenderOverride]
    framed: Framed
    routing: PassRouting
    recovery: Recovery[Endpoints]


# --- [SERVICES] -------------------------------------------------------------------------


class RenderSlots(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render operators with temporary scene overrides, each render running through `render`."""

    render: Render = msgspec.field(default_factory=lambda: native)
    override: ClassVar[Attachment[RenderOverride]] = Attachment(key="render_override", form=RenderOverride, owner=bpy.types.Scene)
    bakes: ClassVar[Preset[Snapshot]] = Preset(key="bake", form=Snapshot)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (override, FrameSize.attachment, PassSet.attachment)

    # --- [COMMANDS]

    @cached_property
    def rendering(self) -> Operator[Launch]:
        """Slot render command binding overrides on call."""
        return Operator(
            key="render",
            title="Render",
            description="Render into chosen slot with temporary scene overrides",
            form=Launch,
            invoke=Some(Properties()),
            undo=False,
            run=lambda call: self.launched(call, viewport=False),
        )

    @cached_property
    def viewporting(self) -> Operator[Launch]:
        """Viewport render command binding overrides on call."""
        return Operator(
            key="render_viewport",
            title="Render Viewport",
            description="Render 3D view with temporary scene overrides",
            form=Launch,
            invoke=Some(Properties()),
            undo=False,
            condition=Some(Setting(descriptor=Descriptor.declared(bpy.types.SpaceView3D, "type", bpy.types.EnumProperty), accepted=("VIEW_3D",))),
            run=lambda call: self.launched(call, viewport=True),
        )

    # --- [OVERRIDES]

    def bound(self, identity: Identity) -> BlenderResult[Overriding]:
        """Binds override store, frame size, pass routing, and recovery."""
        return paired(self.override.bound(identity), FrameSize().bound(identity), PassSet().bound(identity), identity.name(Name.ATTACHMENT, "render_restore").map(Recovery.endpoints)).map(
            lambda parts: Overriding(*parts)
        )

    @staticmethod
    def restored(bound: Overriding, scene: bpy.types.Scene) -> BlenderResult[Option[str]]:
        """Reverts scene overrides once its render finishes, then reconciles pass routing to the restored settings."""
        routing = bound.routing

        def rerouted() -> BlenderResult[None]:
            return routing.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date()).bind(lambda path: routing.reconciled(scene, bpy.context, path)).map(lambda _: None)

        return bound.framed.restored(scene).bind(lambda _: bound.recovery.reverted(scene)).bind(lambda held: rerouted() if held else Ok(None)).map(lambda _: Nothing)

    def launched(self, call: Call[Launch], *, viewport: bool) -> BlenderResult[Done]:
        """Renders into chosen slot under temporary overrides, their prior values recorded first."""
        identity = call.identity

        def overridden_by(bound: Overriding) -> BlenderResult[Done]:
            held, framed, routing, recovery = bound.held, bound.framed, bound.routing, bound.recovery

            def scene_render(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> BlenderResult[Done]:
                record = call.record
                images = tuple(image for image in bpy.data.images if image.type == "RENDER_RESULT")
                prior = tuple((image, image.render_slots.active_index, Option.of_optional(image.render_slots.active).map(lambda slot: (slot, slot.name))) for image in images)
                created: list[tuple[bpy.types.Image, bpy.types.RenderSlot]] = []
                chosen = Some(record.layer.value) if record.layer.enabled and not viewport else Nothing
                member = chosen.default_value(scene.name)

                def prepared(values: tuple[tuple[RenderOverride, Snapshot], tuple[int, OutputPath]]) -> BlenderResult[None]:
                    (override, captured), (version, path) = values
                    presets = value_sets.found(identity, override.values.value.name).bind(value_sets.read).map(lambda snapshot: (snapshot,)) if override.values.enabled else Ok(())
                    isolated = Ok(isolation(scene, layer, override.isolated.value) if override.isolated.enabled else ())
                    stamp = noted(member, version, content_hash(Snapshot, captured))

                    def overridden(owned: tuple[tuple[Snapshot, ...], tuple[tuple[bpy.types.Object, Snapshot], ...]]) -> BlenderResult[None]:
                        writes = (*((scene, snapshot) for snapshot in (*owned[0], stamp)), *owned[1])
                        return recovery.recorded(scene, writes, lambda ends: ends).bind(lambda _: routing.reconciled(scene, call.context, path)).map(lambda _: None)

                    return (
                        paired(presets, isolated)
                        .bind(lambda owned: routing.reconciled(scene, call.context, path).bind(lambda _: overridden(owned)))
                        .bind(lambda _: framed.grown(scene).map(lambda _: None))
                    )

                def slotted(version: int) -> BlenderResult[None]:
                    def changed(image: bpy.types.Image) -> BlenderResult[None]:
                        title = f"{member} v{version}"

                        def retained(slot: bpy.types.RenderSlot) -> BlenderResult[None]:
                            created.append((image, slot))
                            return written(image.render_slots, "active", slot).map(lambda _: None)

                        destination: BlenderResult[bpy.types.RenderSlot] = (
                            Option.of_optional(image.render_slots.active).to_result_with(lambda: (BlenderAbsentFault(owner=repr(image), key="render_slots.active"),))
                            if record.into is Into.DISPLAYED
                            else catch(lambda: image.render_slots.new(name=title), exception=RuntimeError)().map_error(
                                lambda error: (BlenderEditFault(request=f"{image!r}.render_slots.new", error=error),)
                            )
                        )
                        return Result[bpy.types.RenderSlot, BlenderFaults].bind(
                            destination, lambda slot: written(slot, "name", title).map(lambda _: None) if record.into is Into.DISPLAYED else retained(slot)
                        )

                    return accumulated(map(changed, images)).map(lambda _: None)

                def undone(faults: BlenderFaults) -> BlenderResult[Done]:
                    def removed(image: bpy.types.Image, slot: bpy.types.RenderSlot) -> BlenderResult[None]:
                        return (
                            written(image.render_slots, "active", slot)
                            .bind(lambda _: called(bpy.ops.image.remove_render_slot, {}, override=Some(call.context.temp_override(edit_image=image))))
                            .map(lambda _: None)
                        )

                    return accumulated((
                        Error(faults),
                        self.restored(bound, scene),
                        *starmap(removed, created),
                        *(written(image.render_slots, "active_index", index) for image, index, _ in prior),
                        *(written(slot, "name", name) for _, _, held in prior for slot, name in held.to_list()),
                    )).map(lambda _: Done())

                def called_render(_: None) -> BlenderResult[Done]:
                    options = RenderCall(animation=record.animation, write_still=record.write and not record.animation, layer=chosen)
                    outcome = (
                        called(bpy.ops.render.opengl, msgspec.structs.asdict(options) | {"view_context": True}, context="INVOKE_DEFAULT") if viewport else self.render(call.identity, scene, options)
                    )
                    return outcome.bind(lambda status: self.restored(bound, scene).map(lambda _: Done()) if "FINISHED" in status else Ok(Done()))

                day = datetime.datetime.now(datetime.UTC).astimezone().date()
                return (
                    paired(paired(held.read(scene), Snapshot.capture(scene)), paired(routing.pathed.attached.read(scene).map(lambda outputs: outputs.version), routing.pathed.path(scene, day)))
                    .bind(lambda values: prepared(values).bind(lambda _: slotted(values[1][0])).bind(called_render))
                    .or_else_with(undone)
                )

            def checked(roots: tuple[bpy.types.Scene, bpy.types.ViewLayer]) -> BlenderResult[Done]:
                scene = roots[0]
                return recovery.stored.read(scene).bind(lambda held: Error((BlenderCollisionFault(request=repr(scene), error=recovery.stored.name),)) if held.is_some() else scene_render(*roots))

            return paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(checked)

        return self.bound(identity).bind(overridden_by)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds declarations and override recovery until running render finishes."""
        scope = ExitStack()

        def built(bound: Overriding) -> BlenderResult[Acquisitions]:
            finishing = scope.enter_context(identity.reports.deferred(lambda scene: self.restored(bound, scene)))

            def held() -> tuple[bpy.types.Scene, ...]:
                stored = identity.reports.reported(
                    lambda scene: paired(bound.recovery.stored.read(scene), bound.framed.recovery.stored.read(scene)).map(lambda parts: parts[0].is_some() or parts[1].is_some())
                )
                return tuple(scene for scene in bpy.data.scenes if stored(scene).default_value(False))

            def ticked() -> BlenderResult[Option[float]]:
                if not bpy.app.is_job_running("RENDER"):
                    for scene in held():
                        finishing(scene)
                return Ok(Some(0.25))

            def loaded(_: Option[Path]) -> BlenderResult[None]:
                return accumulated(self.restored(bound, scene) for scene in bpy.data.scenes).map(lambda _: None)

            scope.callback(lambda: identity.reports.reported(lambda: accumulated(self.restored(bound, scene) for scene in held()).map(lambda _: None))().to_optional())
            return Ok((
                PresetMenu(source=Captured(preset=value_sets, scope=Scenes(), selection=("render", "cycles", "eevee"))),
                PresetMenu(source=Captured(preset=self.bakes, scope=Scenes(), selection=("render.bake",))),
                self.rendering,
                self.viewporting,
                PathHandler(handlers=LOAD_POST, body=loaded),
                Timer(body=ticked),
            ))

        try:
            yield self.bound(identity).bind(built)
        finally:

            def retired() -> float | None:
                if bpy.app.is_job_running("RENDER"):
                    return 0.25
                scope.close()
                return None

            if bpy.app.is_job_running("RENDER"):
                bpy.app.timers.register(retired, persistent=True)
            else:
                retired()

    # --- [SECTIONS]

    def section(self, retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing) -> Single[RenderOverride]:
        """Returns section of render buttons and override settings, with the bake button, settings, image format, and presets under the slots it bakes."""
        return Single(
            key="render",
            title="Render",
            rows=(
                OperatorButton(target=self.rendering),
                OperatorButton(target=self.viewporting),
                Single(
                    key="bake",
                    title="Bake",
                    rows=(
                        OperatorButton(target=Stock(operator=bpy.ops.object.bake)),
                        *(
                            Property(target=Member(root=bpy.types.Scene, path=f"render.bake.{prop.identifier}"))
                            for prop in bpy.types.BakeSettings.bl_rna.properties
                            if not (prop.is_readonly or prop.is_hidden)
                        ),
                        ImageFormat(target=Member(root=bpy.types.Scene, path=f"render.bake.{pointer(bpy.types.BakeSettings)}")),
                        Pulldown(menu=Listing.keyed(RenderSlots.bakes)),
                    ),
                ),
            ),
            store=Some(Hosted(store=Store(attachment=RenderSlots.override, retention=retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RENDERS]


def native(identity: Identity, scene: bpy.types.Scene, call: RenderCall) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
    """Runs `render.render`, recording Render Result and Viewer images as scene linear under the scene view, the form Blender displays them in."""
    composited = scene.render.use_compositing and any(entry.enable_for_render for entry in scene.compositor_effects)
    layer = COMPOSITE if composited else call.layer.default_with(lambda: scene.view_layers[0].name)

    def rendered(config: PyOpenColorIO.Config) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
        origin = Production(scene=Handle.of(scene), space=config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(), view=Some(viewed(scene)))
        productions = {"RENDER_RESULT": msgspec.structs.replace(origin, layer=Some(layer)), "COMPOSITING": origin}
        for image in (image for image in bpy.data.images if image.type in productions):
            produced(identity, image, Nothing)

        def published(status: frozenset["rna_enums.OperatorReturnItems"]) -> frozenset["rna_enums.OperatorReturnItems"]:
            for image in (image for image in bpy.data.images if image.type in productions):
                produced(identity, image, Some(productions[image.type]))
            return status

        return called(bpy.ops.render.render, msgspec.structs.asdict(call) | {"scene": scene.name}).map(published)

    return config_of().bind(rendered)


def sourced(scenes: tuple[bpy.types.Scene, ...]) -> tuple[bpy.types.Scene, ...]:
    """Returns scenes and every scene their render-enabled groups read through unmuted Render Layers or render Cryptomatte nodes, each once."""
    reached = tuple(
        dict.fromkeys((
            *scenes,
            *(
                live.scene
                for scene in scenes
                for tree in nested((entry.node_group for entry in scene.compositor_effects if entry.enable_for_render and entry.node_group is not None), muted=False)
                for live in tree.nodes
                if isinstance(live, bpy.types.CompositorNodeRLayers | bpy.types.CompositorNodeCryptomatteV2)
                and not live.mute
                and live.scene is not None
                and (not isinstance(live, bpy.types.CompositorNodeCryptomatteV2) or live.source == "RENDER")
            ),
        ))
    )
    return reached if reached == scenes else sourced(reached)


def engined[T](sources: tuple[bpy.types.Scene, ...], engine: str, body: Callable[[], BlenderResult[T]]) -> BlenderResult[T]:
    """Runs body with every source rendering through `engine`, each source's own engine restored after on every path."""
    return scoped(
        Ok(tuple((source.render, source.render.engine) for source in sources)),
        lambda held: accumulated(ENGINE.write(render, engine) for render, _ in held).bind(lambda _: body()),
        lambda held: accumulated(starmap(ENGINE.write, held)),
    )


def bounds(scene: bpy.types.Scene) -> tuple[int, int, int, int, int, int]:
    """Returns scaled frame size and border pixel edges in Blender's float32 arithmetic."""
    render = scene.render
    width, height = (side * render.resolution_percentage // 100 for side in (render.resolution_x, render.resolution_y))
    edges = (
        (
            int(np.float32(width) * np.float32(render.border_min_x)),
            int(np.float32(height) * np.float32(render.border_min_y)),
            int(np.float32(width) * np.float32(render.border_max_x)),
            int(np.float32(height) * np.float32(render.border_max_y)),
        )
        if render.use_border
        else (0, 0, width, height)
    )
    return width, height, *edges


# --- [OVERRIDES]


def isolation(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, isolated: Isolated) -> tuple[tuple[bpy.types.Object, Snapshot], ...]:
    """Builds per-object holdout, camera visibility, or shadow catcher snapshots."""

    def values(item: bpy.types.Object) -> dict[str, bool]:
        return (
            ({SHADOW_CATCHER.path: True} if isolated.catcher else {})
            if item.select_get(view_layer=layer)
            else {HOLDOUT.path: True}
            if isolated.mode is Isolation.HOLDOUT
            else {VISIBLE_CAMERA.path: False}
        )

    return tuple((item, Snapshot.of(bpy.types.Object, values(item))) for item in scene.objects)


def noted(member: str, version: int, key: ContentHash) -> Snapshot:
    """Builds stamp note of member, version, and pre-override settings digest."""
    return Snapshot.of(bpy.types.Scene, {"render.use_stamp_note": True, "render.stamp_note_text": f"{member} v{version} {key.hex()}"})


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ENGINE",
    "HOLDOUT",
    "SHADOW_CATCHER",
    "VISIBLE_CAMERA",
    "Into",
    "Isolated",
    "Isolation",
    "Launch",
    "Render",
    "RenderCall",
    "RenderOverride",
    "RenderSlots",
    "ValueSet",
    "bounds",
    "engined",
    "isolation",
    "native",
    "noted",
    "sourced",
    "value_sets",
]
