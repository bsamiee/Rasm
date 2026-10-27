"""Scene renders: the render call every render path takes, Blender's native render, and slot renders under temporary scene overrides."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import datetime
from enum import auto, Enum
from functools import cached_property, partial
from itertools import starmap
from typing import ClassVar, Final, TYPE_CHECKING

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
from blender.bpy.data.rna.descriptors import choices, Descriptor, RnaPath, written
from blender.bpy.data.rna.snapshots import Endpoints, Scenes, Snapshot
from blender.bpy.extension.events import Handler, Timer
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.trees import nested
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.exports import COMPOSITE
from blender.bpy.render.outputs.formats import pointer
from blender.bpy.render.outputs.framing import Framed, FrameSize
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.outputs.results import produced, Production
from blender.bpy.render.view_layers.passes import PassRouting, PassSet
from blender.bpy.settings.attachments import Attached, Attachment, Recovery
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import Member, OperatorButton, Property, Pulldown
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.templates import ImageFormat
from blender.bpy.ui.layout.values import Hosted, Listing, Store
from blender.bpy.ui.surfaces.menus import Captured, PresetMenu
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.identity import content_hash, ContentHash
from blender.records.trees import Retention

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Render = Callable[[Identity, bpy.types.Scene, RenderCall], Outcome[frozenset[rna_enums.OperatorReturnItems]]]


class Into(Enum):
    """Render Result slot receiving next render."""

    DISPLAYED = auto()
    NEW = auto()


class Isolation(Enum):
    """Render state assigned to unselected scene objects."""

    HOLDOUT = auto()
    HIDDEN = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

HOLDOUT: Final = Descriptor.declared(bpy.types.Object, "is_holdout", bpy.types.BoolProperty)
VISIBLE_CAMERA: Final = Descriptor.declared(bpy.types.Object, "visible_camera", bpy.types.BoolProperty)
SHADOW_CATCHER: Final = Descriptor.declared(bpy.types.Object, "is_shadow_catcher", bpy.types.BoolProperty)
ENGINE: Final = Descriptor.declared(bpy.types.RenderSettings, "engine", bpy.types.EnumProperty)
value_sets: Final[Preset[Snapshot]] = Preset(key="render_override", form=Snapshot)

# --- [MODELS] ---------------------------------------------------------------------------


class RenderCall(msgspec.Struct, frozen=True, kw_only=True):
    """`render.render` options a render passes through, an absent layer rendering every layer."""

    animation: bool = False
    write_still: bool = False
    layer: Option[str] = Nothing


class Launch(bpy.types.PropertyGroup):
    """Render options and destination slot."""

    animation: bpy.props.BoolProperty(name="Animation")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    write: bpy.props.BoolProperty(name="Write Still")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    into: bpy.props.EnumProperty(name="Slot", items=choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in Into))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_layer: bpy.props.BoolProperty(name="View Layer", description="Render one view layer alone")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    layer: bpy.props.StringProperty(name="Layer", search=lambda _owner, context, _edit: tuple(layer.name for scene in Option.of_optional(context.scene).to_list() for layer in scene.view_layers))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Isolated(msgspec.Struct, frozen=True, kw_only=True):
    """Isolation of unselected objects, shadow catching on selected."""

    mode: Isolation = Isolation.HOLDOUT
    catcher: bool = False


class RenderOverride(bpy.types.PropertyGroup):
    """Overrides applied for one render."""

    use_values: bpy.props.BoolProperty(name="Value Set", description="Apply a scene value preset for the render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    values: bpy.props.StringProperty(name="Preset", description="Scene value preset applied for the render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_isolation: bpy.props.BoolProperty(name="Isolate Selection", description="Isolate selected objects for the render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    isolation: bpy.props.EnumProperty(name="Others", items=choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in Isolation))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    catcher: bpy.props.BoolProperty(name="Shadow Catcher")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


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
        return Operator(form=Launch, invoke=Some(Properties()), undo=False, run=self.render_slot)

    @cached_property
    def viewporting(self) -> Operator[Launch]:
        """Viewport render command binding overrides on call."""
        return Operator(
            form=Launch,
            invoke=Some(Properties()),
            undo=False,
            condition=Some(Setting(descriptor=Descriptor.declared(bpy.types.SpaceView3D, "type", bpy.types.EnumProperty), accepted=("VIEW_3D",))),
            run=self.render_viewport,
        )

    # --- [OVERRIDES]

    def bound(self, identity: Identity) -> Outcome[Overriding]:
        """Binds override store, frame size, pass routing, and recovery."""
        return paired(self.override.bound(identity), FrameSize().bound(identity), PassSet().bound(identity), identity.identifier("render_restore").map(Recovery.endpoints)).map(
            lambda parts: Overriding(*parts)
        )

    @staticmethod
    def restored(bound: Overriding, scene: bpy.types.Scene) -> Outcome[Option[str]]:
        """Reverts scene overrides once its render finishes, then reconciles pass routing to the restored settings."""
        routing = bound.routing

        def rerouted() -> Outcome[None]:
            return routing.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date()).bind(lambda path: routing.reconciled(scene, bpy.context, path)).map(lambda _: None)

        return bound.framed.restored(scene).bind(lambda _: bound.recovery.reverted(scene)).bind(lambda held: rerouted() if held else Ok(None)).map(lambda _: Nothing)

    def render_slot(self, call: Call[Launch]) -> Outcome[Done]:
        """Renders into the chosen slot under temporary scene overrides."""
        return self.launched(call, viewport=False)

    def render_viewport(self, call: Call[Launch]) -> Outcome[Done]:
        """Renders the 3D view under temporary scene overrides."""
        return self.launched(call, viewport=True)

    def launched(self, call: Call[Launch], *, viewport: bool) -> Outcome[Done]:
        """Renders into chosen slot under temporary overrides, their prior values recorded first."""
        identity = call.identity

        def overridden_by(bound: Overriding) -> Outcome[Done]:
            held, framed, routing, recovery = bound.held, bound.framed, bound.routing, bound.recovery

            def scene_render(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[Done]:
                record = call.properties
                images = tuple(image for image in bpy.data.images if image.type == "RENDER_RESULT")
                prior = tuple((image, image.render_slots.active_index, Option.of_optional(image.render_slots.active).map(lambda slot: (slot, slot.name))) for image in images)
                created: list[tuple[bpy.types.Image, bpy.types.RenderSlot]] = []
                chosen = Some(record.layer) if record.use_layer and not viewport else Nothing
                member = chosen.default_value(scene.name)

                def prepared(values: tuple[Snapshot, tuple[int, OutputPath]]) -> Outcome[None]:
                    captured, (version, path) = values
                    override = held.struct(scene)
                    presets = value_sets.found(identity, override.values).bind(value_sets.read).map(lambda snapshot: (snapshot,)) if override.use_values else Ok(())
                    isolated = Ok(isolation(scene, layer, Isolated(mode=Isolation[override.isolation], catcher=override.catcher)) if override.use_isolation else ())
                    stamp = noted(member, version, content_hash(Snapshot, captured))

                    def overridden(owned: tuple[tuple[Snapshot, ...], tuple[tuple[bpy.types.Object, Snapshot], ...]]) -> Outcome[None]:
                        writes = (*((scene, snapshot) for snapshot in (*owned[0], stamp)), *owned[1])
                        return recovery.recorded(scene, writes, lambda ends: ends).bind(lambda _: routing.reconciled(scene, call.context, path)).map(lambda _: None)

                    return (
                        paired(presets, isolated)
                        .bind(lambda owned: routing.reconciled(scene, call.context, path).bind(lambda _: overridden(owned)))
                        .bind(lambda _: framed.grown(scene).map(lambda _: None))
                    )

                def slotted(version: int) -> Outcome[None]:
                    def changed(image: bpy.types.Image) -> Outcome[None]:
                        title = f"{member} v{version}"

                        def retained(slot: bpy.types.RenderSlot) -> Outcome[None]:
                            created.append((image, slot))
                            return written(image.render_slots, "active", slot).map(lambda _: None)

                        destination: Outcome[bpy.types.RenderSlot] = (
                            Option.of_optional(image.render_slots.active).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(image), member="render_slots.active"))
                            if Into[record.into] is Into.DISPLAYED
                            else catch(lambda: image.render_slots.new(name=title), exception=RuntimeError)().map_error(partial(Fault.of, BlenderReason.REFUSED, f"{image!r}.render_slots.new"))
                        )
                        return Result[bpy.types.RenderSlot, Faults].bind(
                            destination, lambda slot: written(slot, "name", title).map(lambda _: None) if Into[record.into] is Into.DISPLAYED else retained(slot)
                        )

                    return accumulated(map(changed, images)).map(lambda _: None)

                def undone(faults: Faults) -> Outcome[Done]:
                    def removed(image: bpy.types.Image, slot: bpy.types.RenderSlot) -> Outcome[None]:
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

                def called_render(_: None) -> Outcome[Done]:
                    options = RenderCall(animation=record.animation, write_still=record.write and not record.animation, layer=chosen)
                    outcome = (
                        called(bpy.ops.render.opengl, msgspec.structs.asdict(options) | {"view_context": True}, context="INVOKE_DEFAULT") if viewport else self.render(call.identity, scene, options)
                    )
                    return outcome.bind(lambda status: self.restored(bound, scene).map(lambda _: Done()) if "FINISHED" in status else Ok(Done()))

                day = datetime.datetime.now(datetime.UTC).astimezone().date()
                return (
                    paired(Snapshot.capture(scene), routing.pathed.path(scene, day).map(lambda path: (routing.pathed.attached.struct(scene).version, path)))
                    .bind(lambda values: prepared(values).bind(lambda _: slotted(values[1][0])).bind(called_render))
                    .or_else_with(undone)
                )

            def checked(roots: tuple[bpy.types.Scene, bpy.types.ViewLayer]) -> Outcome[Done]:
                scene = roots[0]
                return recovery.stored.read(scene).bind(lambda held: Error(Fault.of(BlenderReason.TAKEN, repr(scene), member=recovery.stored.name)) if held.is_some() else scene_render(*roots))

            return paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(checked)

        return self.bound(identity).bind(overridden_by)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds declarations and override recovery until running render finishes."""
        scope = ExitStack()

        def built(bound: Overriding) -> Outcome[Acquisitions]:
            finishing = scope.enter_context(identity.reports.deferred(lambda scene: self.restored(bound, scene)))

            def held() -> tuple[bpy.types.Scene, ...]:
                stored = identity.reports.reported(
                    lambda scene: paired(bound.recovery.stored.read(scene), bound.framed.recovery.stored.read(scene)).map(lambda parts: parts[0].is_some() or parts[1].is_some())
                )
                return tuple(scene for scene in bpy.data.scenes if stored(scene).default_value(False))

            def ticked() -> Outcome[Option[float]]:
                if not bpy.app.is_job_running("RENDER"):
                    for scene in held():
                        finishing(scene)
                return Ok(Some(0.25))

            def loaded() -> Outcome[None]:
                return accumulated(self.restored(bound, scene) for scene in bpy.data.scenes).map(lambda _: None)

            scope.callback(lambda: identity.reports.reported(lambda: accumulated(self.restored(bound, scene) for scene in held()).map(lambda _: None))().to_optional())
            return Ok((
                PresetMenu(source=Captured(preset=value_sets, scope=Scenes(), selection=(("render",), ("cycles",), ("eevee",)))),
                PresetMenu(source=Captured(preset=self.bakes, scope=Scenes(), selection=(("render", "bake"),))),
                self.rendering,
                self.viewporting,
                Handler(handlers=bpy.app.handlers.load_post, body=loaded),
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

    def section(self, retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing) -> Single[RenderOverride]:
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
                            Property(target=Member(root=bpy.types.Scene, path=("render", "bake", prop.identifier)))
                            for prop in bpy.types.BakeSettings.bl_rna.properties
                            if not (prop.is_readonly or prop.is_hidden)
                        ),
                        ImageFormat(target=Member(root=bpy.types.Scene, path=("render", "bake", pointer(bpy.types.BakeSettings)))),
                        Pulldown(menu=Listing.keyed(RenderSlots.bakes)),
                    ),
                ),
            ),
            store=Some(Hosted(store=Store(attachment=RenderSlots.override, retention=retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RENDERS]


def native(identity: Identity, scene: bpy.types.Scene, call: RenderCall) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
    """Runs `render.render`, recording Render Result and Viewer images as scene linear under the scene view, the form Blender displays them in."""
    composited = scene.render.use_compositing and any(entry.enable_for_render for entry in scene.compositor_effects)
    active: bpy.types.ViewLayer = bpy.context.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    layer = COMPOSITE if composited else call.layer.default_value(active.name)

    origin = Production(scene=Handle.of(scene), space=PyOpenColorIO.GetCurrentConfig().getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(), view=Some(viewed(scene)))
    productions = {"RENDER_RESULT": msgspec.structs.replace(origin, layer=Some(layer)), "COMPOSITING": origin}
    for image in (image for image in bpy.data.images if image.type in productions):
        produced(identity, image, Nothing)

    def published(status: frozenset["rna_enums.OperatorReturnItems"]) -> frozenset["rna_enums.OperatorReturnItems"]:
        for image in (image for image in bpy.data.images if image.type in productions):
            produced(identity, image, Some(productions[image.type]))
        return status

    return called(bpy.ops.render.render, msgspec.structs.asdict(call) | {"scene": scene.name}).map(published)


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


def engined[T](sources: tuple[bpy.types.Scene, ...], engine: str, body: Callable[[], Outcome[T]]) -> Outcome[T]:
    """Runs body with every source rendering through `engine`, each source's own engine restored after on every path."""
    held = tuple((source.render, source.render.engine) for source in sources)
    return paired(accumulated(ENGINE.write(render, engine) for render, _ in held).bind(lambda _: body()), accumulated(starmap(ENGINE.write, held))).map(lambda pair: pair[0])


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

    def values(item: bpy.types.Object) -> dict[RnaPath, bool]:
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
    return Snapshot.of(bpy.types.Scene, {("render", "use_stamp_note"): True, ("render", "stamp_note_text"): f"{member} v{version} {key.hex()}"})


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
    "bounds",
    "engined",
    "isolation",
    "native",
    "noted",
    "sourced",
    "value_sets",
]
