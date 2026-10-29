"""Scene renders: the render call every render path takes, Blender's native render, and slot renders under temporary scene overrides."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import datetime
from enum import auto, Enum
from functools import cached_property, partial
from typing import ClassVar, TYPE_CHECKING, TypeGuard

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Called, called, Opened
from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Call, Done, Operator, Over
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Scenes, Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.mirrors import enum_choices
from blender.bpy.data.rna.paths import RnaPath, rooted
from blender.bpy.data.rna.restoration import Endpoints
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.nodes.trees import nested
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.framing import Framed, FrameSize
from blender.bpy.render.outputs.paths import OutputPath, today
from blender.bpy.render.outputs.results import COMPOSITE, Production, PRODUCTIONS
from blender.bpy.render.view_layers.routing import PassRouting, PassSet
from blender.bpy.settings.attachments import Attached, Attachment, Recovery
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import Opens, OperatorButton
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Captured, menu_key, Store
from blender.bpy.ui.surfaces.menus import PresetMenu
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Retention
from blender.records.stores import content_hash

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


# --- [MODELS] ---------------------------------------------------------------------------


class RenderCall(msgspec.Struct, frozen=True, kw_only=True):
    """`render.render` options a render passes through, an absent layer rendering every layer and an absent frame bound keeping the scene range."""

    animation: bool = False
    write_still: bool = False
    layer: Option[str] = Nothing
    frame_start: Option[int] = Nothing
    frame_end: Option[int] = Nothing


class Launch(bpy.types.PropertyGroup):
    """Render options and destination slot."""

    animation: bpy.props.BoolProperty(name="Animation")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    write: bpy.props.BoolProperty(name="Write Still")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    into: bpy.props.EnumProperty(name="Slot", items=enum_choices(Into))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_layer: bpy.props.BoolProperty(name="View Layer", description="Render one view layer alone")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    layer: bpy.props.StringProperty(name="Layer", search=lambda _owner, context, _edit: tuple(layer.name for scene in Option.of_optional(context.scene).to_list() for layer in scene.view_layers))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class RenderOverride(bpy.types.PropertyGroup):
    """Overrides applied for one render."""

    use_values: bpy.props.BoolProperty(name="Value Set", description="Apply a scene value preset for the render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    values: bpy.props.StringProperty(name="Preset", description="Scene value preset applied for the render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_isolation: bpy.props.BoolProperty(name="Isolate Selection", description="Isolate selected objects for the render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    isolation: bpy.props.EnumProperty(name="Others", items=enum_choices(Isolation))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    catcher: bpy.props.BoolProperty(name="Shadow Catcher")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Overriding(msgspec.Struct, frozen=True):
    """Override store, frame size, pass routing, and recovery one render binds."""

    held: Attached[RenderOverride]
    framed: Framed
    routing: PassRouting
    recovery: Recovery[Endpoints]

    def restored(self, day: datetime.date, scene: bpy.types.Scene) -> Outcome[Option[str]]:
        """Reverts scene overrides once its render finishes, then reconciles pass routing to the restored settings on the output path of that day."""

        def rerouted() -> Outcome[None]:
            return self.routing.pathed.path(scene, day).bind(lambda path: self.routing.reconciled(scene, bpy.context, path)).map(lambda _: None)

        return self.framed.recovery.reverted(scene).bind(lambda _: self.recovery.reverted(scene)).bind(lambda held: rerouted() if held else Ok(None)).map(lambda _: Nothing)


# --- [SERVICES] -------------------------------------------------------------------------

# --- [RENDERS]


def native(identity: Identity, scene: bpy.types.Scene, call: RenderCall) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
    """Runs `render.render`, recording Render Result and Viewer images as scene linear under the scene view, the form Blender displays them in."""
    composited = scene.render.use_compositing and any(entry.enable_for_render for entry in scene.compositor_effects)
    active: bpy.types.ViewLayer = bpy.context.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    layer = COMPOSITE if composited else call.layer.default_value(active.name)

    origin = Production(scene=Handle.of(scene), space=bpy.data.colorspace.working_space, view=Some(viewed(scene)))
    productions = {"RENDER_RESULT": msgspec.structs.replace(origin, layer=Some(layer)), "COMPOSITING": origin}
    for image in (image for image in bpy.data.images if image.type in productions):
        Production.recorded(identity, image, Nothing)

    def published(status: frozenset["rna_enums.OperatorReturnItems"]) -> frozenset["rna_enums.OperatorReturnItems"]:
        for image in (image for image in bpy.data.images if image.type in productions):
            Production.recorded(identity, image, Some(productions[image.type]))
        return status

    return called(bpy.ops.render.render, msgspec.structs.asdict(call) | {"scene": scene.name}).map(published)


# --- [SLOTS]


class RenderSlots(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render operators with temporary scene overrides, each render running through `render`."""

    render: Render = native
    override: ClassVar[Attachment[RenderOverride]] = Attachment(key="render_override", form=RenderOverride, owner=bpy.types.Scene)
    values: ClassVar[Preset[Snapshot]] = Preset(key="render_override", form=Snapshot)
    bakes: ClassVar[Preset[Snapshot]] = Preset(key="bake", form=Snapshot)
    bake_presets: ClassVar[Opens] = Opens(target=Opened(menu_key(bakes), Called.MENU))
    prerequisites: ClassVar[tuple[Declaration, ...]] = (override, PRODUCTIONS, FrameSize.attachment, *PassSet.prerequisites)

    # --- [COMMANDS]

    @cached_property
    def rendering(self) -> Operator[Launch]:
        """Slot render command binding overrides on call."""
        return Operator.of(Launch, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), undo=Nothing)(Over(self.bound, self.render_slot))

    @cached_property
    def viewporting(self) -> Operator[Launch]:
        """Viewport render command binding overrides on call."""
        return Operator.of(
            Launch,
            invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))),
            undo=Nothing,
            condition=Some(Setting(descriptor=Descriptor.declared(bpy.types.SpaceView3D, ("type",), bpy.types.EnumProperty), accepted=("VIEW_3D",))),
        )(Over(self.bound, self.render_viewport))

    # --- [OVERRIDES]

    def bound(self, identity: Identity) -> Outcome[Overriding]:
        """Binds override store, frame size, pass routing, and recovery."""
        return paired(self.override.bound(identity), FrameSize().bound(identity), PassSet.bound(identity), Recovery.endpoints(identity, "render_restore")).map(lambda parts: Overriding(*parts))

    def render_slot(self, bound: Overriding, call: Call[Launch]) -> Outcome[Done]:
        """Renders into the chosen slot under temporary scene overrides."""
        return self.launched(bound, call, viewport=False)

    def render_viewport(self, bound: Overriding, call: Call[Launch]) -> Outcome[Done]:
        """Renders the 3D view under temporary scene overrides."""
        return self.launched(bound, call, viewport=True)

    def launched(self, bound: Overriding, call: Call[Launch], *, viewport: bool) -> Outcome[Done]:
        """Renders into chosen slot under temporary overrides, their prior values recorded first, restoring them once the render ends, refusing a scene still holding the overrides of an unfinished render."""
        identity, record, day = call.identity, call.properties, today()

        def scene_render(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[Done]:
            images = tuple(image for image in bpy.data.images if image.type == "RENDER_RESULT")
            chosen = Some(record.layer) if record.use_layer and not viewport else Nothing
            member = chosen.default_value(scene.name)

            def prepared(values: tuple[Snapshot, tuple[int, OutputPath]]) -> Outcome[None]:
                captured, (version, path) = values
                override = bound.held.struct(scene)
                presets = self.values.loaded(identity, override.values).map(lambda snapshot: (snapshot,)) if override.use_values else Ok(())
                writes = (
                    *((scene, snapshot) for snapshot in presets.default_value(())),
                    (scene, Snapshot.of(bpy.types.Scene, {("render", "use_stamp_note"): True, ("render", "stamp_note_text"): f"{member} v{version} {content_hash(Snapshot, captured).hex()}"})),
                    *(isolation(scene, layer, override) if override.use_isolation else ()),
                )
                return (
                    presets
                    .bind(lambda _: bound.recovery.recorded(scene, writes, lambda ends: ends))
                    .bind(lambda _: bound.routing.reconciled(scene, call.context, path))
                    .bind(lambda _: bound.framed.grown(scene))
                    .map(lambda _: None)
                )

            def slotted(rollback: ExitStack, version: int) -> Outcome[None]:
                title = f"{member} v{version}"

                def changed(image: bpy.types.Image) -> Outcome[None]:
                    slots = image.render_slots
                    rollback.callback(written, slots, ("active_index",), slots.active_index)
                    match Into[record.into]:
                        case Into.DISPLAYED:
                            return Option.of_optional(slots.active).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(image), member="render_slots.active")).bind(renamed)
                        case Into.NEW:
                            return catch(slots.new, exception=RuntimeError)(name=title).map_error(partial(Fault.of, Reason.REFUSED, f"{image!r}.render_slots.new")).bind(partial(added, image))

                def renamed(slot: bpy.types.RenderSlot) -> Outcome[None]:
                    rollback.callback(written, slot, ("name",), slot.name)
                    return written(slot, ("name",), title).map(lambda _: None)

                def added(image: bpy.types.Image, slot: bpy.types.RenderSlot) -> Outcome[None]:
                    rollback.callback(dropped, image, slot)
                    return written(image.render_slots, ("active",), slot).map(lambda _: None)

                return accumulated(map(changed, images)).map(lambda _: None)

            def dropped(image: bpy.types.Image, slot: bpy.types.RenderSlot) -> Outcome[None]:
                return (
                    written(image.render_slots, ("active",), slot)
                    .bind(lambda _: called(bpy.ops.image.remove_render_slot, {}, override=Some(call.context.temp_override(edit_image=image))))
                    .map(lambda _: None)
                )

            def rendered(_: None) -> Outcome[Done]:
                options = RenderCall(animation=record.animation, write_still=record.write and not record.animation, layer=chosen)
                outcome = called(bpy.ops.render.opengl, msgspec.structs.asdict(options) | {"view_context": True}, context="EXEC_DEFAULT") if viewport else self.render(identity, scene, options)
                return outcome.bind(lambda _: bound.restored(day, scene)).map(lambda _: Done())

            with ExitStack() as rollback:
                outcome = (
                    paired(Snapshot.capture(scene), bound.routing.pathed.path(scene, day).map(lambda path: (bound.routing.pathed.attached.struct(scene).version, path)))
                    .bind(lambda values: prepared(values).bind(lambda _: slotted(rollback, values[1][0])))
                    .bind(rendered)
                )
                if outcome.is_ok():
                    rollback.pop_all()
            return outcome.or_else_with(lambda faults: paired(Error(faults), bound.restored(day, scene)).map(lambda _: Done()))

        def checked(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[Done]:
            return bound.recovery.stored.read(scene).bind(lambda held: Error(Fault.of(Reason.TAKEN, repr(scene), member=bound.recovery.stored.name)) if held.is_some() else scene_render(scene, layer))

        return paired(rooted(bpy.types.Scene, call.context), rooted(bpy.types.ViewLayer, call.context)).bind(lambda parts: checked(*parts))

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds declarations and override recovery, restoring held scenes at file load, after a crash left them, and at scope exit."""
        with ExitStack() as scope:

            def built(bound: Overriding) -> Outcome[Acquisitions]:
                def loaded() -> Outcome[None]:
                    return accumulated(map(partial(bound.restored, today()), bpy.data.scenes)).map(lambda _: None)

                scope.callback(lambda: identity.reports.reported(loaded)().to_optional())
                return Ok((
                    PresetMenu(source=Captured(preset=self.values, scope=Scenes(), selection=(("render",), ("cycles",), ("eevee",)))),
                    PresetMenu(source=Captured(preset=self.bakes, scope=Scenes(), selection=(("render", "bake"),))),
                    self.rendering,
                    self.viewporting,
                    Handler(handlers=bpy.app.handlers.load_post, body=loaded),
                ))

            yield self.bound(identity).bind(built)

    # --- [SECTIONS]

    def section(self, retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing) -> Section[RenderOverride]:
        """Returns section of render buttons and override settings."""
        return Section(
            key="render", title="Render", rows=(OperatorButton(target=self.rendering), OperatorButton(target=self.viewporting)), store=Some(Store(attachment=RenderSlots.override, retention=retention))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SOURCES]


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
                if not live.mute and reads_render(live) and live.scene is not None
            ),
        ))
    )
    return reached if reached == scenes else sourced(reached)


def reads_render(node: bpy.types.Node) -> TypeGuard[bpy.types.CompositorNodeRLayers | bpy.types.CompositorNodeCryptomatteV2]:
    """Answers whether a compositor node reads a scene render: a Render Layers node, or a Cryptomatte node sourcing the render."""
    match node:
        case bpy.types.CompositorNodeRLayers():
            return True
        case bpy.types.CompositorNodeCryptomatteV2(source=source):
            return source == "RENDER"
        case _:
            return False


def engined[T](sources: tuple[bpy.types.Scene, ...], engine: str, body: Callable[[], Outcome[T]]) -> Outcome[T]:
    """Runs body with every source rendering through `engine`, each source's own engine restored after on every path."""
    held = tuple((source.render, source.render.engine) for source in sources)
    return paired(accumulated(assigned(render, {("engine",): engine}) for render, _ in held).bind(lambda _: body()), accumulated(assigned(render, {("engine",): prior}) for render, prior in held)).map(
        lambda pair: pair[0]
    )


# --- [OVERRIDES]


def isolation(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, override: RenderOverride) -> tuple[tuple[bpy.types.Object, Snapshot], ...]:
    """Builds per-object holdout, camera visibility, or shadow catcher snapshots of an override's isolation."""

    def values(item: bpy.types.Object) -> dict[RnaPath, bool]:
        return (
            ({("is_shadow_catcher",): True} if override.catcher else {})
            if item.select_get(view_layer=layer)
            else {("is_holdout",): True}
            if Isolation[override.isolation] is Isolation.HOLDOUT
            else {("visible_camera",): False}
        )

    return tuple((item, Snapshot.of(bpy.types.Object, values(item))) for item in scene.objects)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Into", "Isolation", "Launch", "Overriding", "Render", "RenderCall", "RenderOverride", "RenderSlots", "engined", "isolation", "native", "reads_render", "sourced"]
