"""Native and viewport renders, color origins, temporary overrides, and scene dependencies."""

from collections import deque
from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import datetime
from enum import auto, Enum
from functools import cached_property, partial
from typing import ClassVar, Final, TYPE_CHECKING, TypeGuard

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import BoundBody, Call, called, Done, Operator, PopupKind, PopupTarget
from blender.bpy.data.rna.conditions import Scenes, Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import RnaPath, rooted
from blender.bpy.data.rna.properties import enum_choices
from blender.bpy.data.rna.restoration import BeforeAfter
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Declaration, Handler
from blender.bpy.nodes.trees import nested
from blender.bpy.render.outputs.framing import Framed, FrameSize
from blender.bpy.render.outputs.paths import OutputPath, today
from blender.bpy.render.outputs.results import animation_files, Receipt, RENDER_ORIGINS, still_files
from blender.bpy.render.view_layers.passes import layer_of, soloed
from blender.bpy.render.view_layers.routing import PassRouting, RoutingRow
from blender.bpy.settings.attachments import Attached, Attachment, Recovery, RetentionSettings
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.elements.menus import PresetMenu
from blender.bpy.ui.elements.panels import Preference
from blender.bpy.ui.layout.rows import Layout, Opens, OperatorButton
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Captured, menu_key, Store
from blender.records.faults import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason
from blender.records.fields import Retention
from blender.records.stores import content_hash

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Render = Callable[[Identity, bpy.types.Scene, RenderCall], Outcome[frozenset[rna_enums.OperatorReturnItems]]]


class TargetSlot(Enum):
    """Render Result slot receiving next render."""

    DISPLAYED = auto()
    NEW = auto()


class Isolation(Enum):
    """Render state assigned to unselected scene objects."""

    HOLDOUT = auto()
    HIDDEN = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

RENDER_HISTORY: Final = Preference(key="render_history", form=RetentionSettings)

# --- [MODELS] ---------------------------------------------------------------------------


class RenderCall(msgspec.Struct, frozen=True, kw_only=True):
    """Native render options, an explicit layer rendered alone and absent frame bounds keeping the scene range."""

    animation: bool = False
    write_still: bool = False
    layer: Option[str] = Nothing
    frame_start: Option[int] = Nothing
    frame_end: Option[int] = Nothing


class Launch(bpy.types.PropertyGroup):
    """Render options and destination slot."""

    animation: bpy.props.BoolProperty(name="Animation")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    write_still: bpy.props.BoolProperty(name="Write Still")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    slot: bpy.props.EnumProperty(name="Slot", items=enum_choices(TargetSlot))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_layer: bpy.props.BoolProperty(name="View Layer", description="Render one view layer alone")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    layer: bpy.props.StringProperty(name="Layer", search=lambda _owner, context, _edit: tuple(layer.name for scene in Option.of_optional(context.scene).to_list() for layer in scene.view_layers))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class RenderOverride(bpy.types.PropertyGroup):
    """Overrides applied for one render."""

    use_preset: bpy.props.BoolProperty(name="Value Set", description="Apply a scene value preset for the render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    preset: bpy.props.StringProperty(name="Preset", description="Scene value preset applied for the render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_isolation: bpy.props.BoolProperty(name="Isolate Selection", description="Isolate selected objects for the render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    isolation: bpy.props.EnumProperty(name="Others", items=enum_choices(Isolation))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    catcher: bpy.props.BoolProperty(name="Shadow Catcher")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class RenderBinding(msgspec.Struct, frozen=True):
    """Override store, frame size, pass routing, and recovery one render binds."""

    held: Attached[RenderOverride]
    framed: Framed
    routing: PassRouting
    recovery: Recovery[BeforeAfter]

    def restored(self, day: datetime.date, scene: bpy.types.Scene) -> Outcome[None]:
        """Reverts scene overrides once its render finishes, then reconciles pass routing to the restored settings on the output path of that day."""

        def rerouted() -> Outcome[None]:
            return self.routing.reconciled(scene, bpy.context, self.routing.pathed.path(scene, day)).map(lambda _: None)

        return self.framed.recovery.reverted(scene).bind(lambda _: self.recovery.reverted(scene)).bind(lambda held: rerouted() if held else Ok(None))


# --- [SERVICES] -------------------------------------------------------------------------

# --- [RENDERS]


@contextmanager
def _rendering(scene: bpy.types.Scene, layer: Option[bpy.types.ViewLayer]) -> Generator[None]:
    """Holds the producing scene, native interface lock, and layer policy."""
    settings = scene.render
    locked, single = settings.use_lock_interface, settings.use_single_layer
    uses = tuple((held, held.use) for held in scene.view_layers) if layer.is_some() else ()
    try:
        settings.use_lock_interface = True
        settings.use_single_layer = layer.is_some() or single
        for chosen in layer.to_list():
            soloed(chosen)
        with bpy.context.temp_override(scene=scene):
            yield
    finally:
        for held, use in uses:
            held.use = use
        settings.use_single_layer = single
        settings.use_lock_interface = locked


def native(identity: Identity, scene: bpy.types.Scene, call: RenderCall, *, files: Option[Receipt] = Nothing) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
    """Renders under the native lock with explicit receipts or paths of the native write mode."""
    receipts: Outcome[Option[Receipt]] = Ok(files.or_else(Some(animation_files) if call.animation else Some(still_files) if call.write_still else Nothing))

    def rendered(layer: Option[bpy.types.ViewLayer]) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
        with _rendering(scene, layer), RENDER_ORIGINS[identity].requested(scene, call.layer, receipts, animation=call.animation):
            return called(bpy.ops.render.render, msgspec.structs.asdict(call))

    return call.layer.map(lambda name: layer_of(scene, name).map(Some)).default_value(Ok(Nothing)).bind(rendered)


# --- [SLOTS]


class RenderSlots(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render operators with temporary scene overrides, each render running through `render`."""

    render: Render = native
    override: ClassVar[Attachment[RenderOverride]] = Attachment(key="render_override", form=RenderOverride, owner=bpy.types.Scene)
    presets: ClassVar[Preset[Snapshot]] = Preset(key="render_override", form=Snapshot)
    bakes: ClassVar[Preset[Snapshot]] = Preset(key="bake", form=Snapshot)
    bake_presets: ClassVar[Opens] = Opens(target=PopupTarget(menu_key(bakes), PopupKind.MENU))
    prerequisites: ClassVar[tuple[Declaration, ...]] = (override, RENDER_HISTORY, RENDER_ORIGINS, FrameSize.attachment, *RoutingRow.prerequisites)

    # --- [COMMANDS]

    @cached_property
    def rendering(self) -> Operator[Launch]:
        """Slot render command binding overrides on call."""
        return Operator.of(Launch, invoke=Some(Popup[Launch](partial(bpy.types.WindowManager.invoke_props_dialog))), options=frozenset())(
            BoundBody[RenderBinding, Launch](self.bound, self.render_slot)
        )

    @cached_property
    def viewporting(self) -> Operator[Launch]:
        """Viewport render command binding overrides on call."""
        return Operator.of(
            Launch,
            invoke=Some(Popup[Launch](partial(bpy.types.WindowManager.invoke_props_dialog))),
            options=frozenset(),
            condition=Some(Setting(descriptor=Descriptor(root=bpy.types.SpaceView3D, prop=bpy.types.Space.bl_rna.properties["type"]), accepted=("VIEW_3D",))),
        )(BoundBody[RenderBinding, Launch](self.bound, self.render_viewport))

    # --- [OVERRIDES]

    def bound(self, identity: Identity) -> Outcome[RenderBinding]:
        """Binds override store, frame size, pass routing, and recovery."""
        return paired(self.override.bound(identity), FrameSize().bound(identity), RoutingRow.bound(identity)).map(
            lambda parts: RenderBinding(*parts, Recovery.before_after(identity, "render_restore"))
        )

    def render_slot(self, bound: RenderBinding, call: Call[Launch]) -> Outcome[Done]:
        """Renders into the chosen slot under temporary scene overrides."""
        return self.launched(bound, call, self.render, Some(call.properties.layer) if call.properties.use_layer else Nothing)

    def render_viewport(self, bound: RenderBinding, call: Call[Launch]) -> Outcome[Done]:
        """Renders the 3D view under temporary scene overrides."""

        def viewport(_identity: Identity, scene: bpy.types.Scene, options: RenderCall) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
            with _rendering(scene, Nothing):
                return called(bpy.ops.render.opengl, msgspec.structs.asdict(options) | {"view_context": True})

        return self.launched(bound, call, viewport, Nothing)

    def launched(self, bound: RenderBinding, call: Call[Launch], render: Render, chosen: Option[str]) -> Outcome[Done]:
        """Renders into chosen slot under temporary overrides, their prior values recorded first, restoring them once the render ends, refusing a scene holding the overrides of an unfinished render."""
        identity, record, day = call.identity, call.properties, today()

        def scene_render(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[Done]:
            images = tuple(image for image in bpy.data.images if image.type == "RENDER_RESULT")
            member = chosen.default_value(scene.name)

            def prepared(values: tuple[Snapshot, tuple[int, OutputPath]]) -> Outcome[None]:
                captured, (version, path) = values
                override = bound.held.struct(scene)
                presets = self.presets.loaded(identity, override.preset).map(lambda snapshot: (snapshot,)) if override.use_preset else Ok(())
                writes = (
                    *((scene, snapshot) for snapshot in presets.default_value(())),
                    (scene, Snapshot.of(bpy.types.Scene, {("render", "use_stamp_note"): True, ("render", "stamp_note_text"): f"{member} v{version} {content_hash(Snapshot, captured).hex()}"})),
                    *(isolation(scene, layer, override) if override.use_isolation else ()),
                )
                return (
                    presets
                    .bind(lambda _: bound.recovery.recorded(scene, writes, lambda ends: ends))
                    .bind(lambda _: bound.routing.reconciled(scene, call.context, Ok(path)))
                    .bind(lambda _: bound.framed.grown(scene))
                    .map(lambda _: None)
                )

            def slotted(rollback: ExitStack, version: int) -> Outcome[None]:
                title = f"{member} v{version}"

                def changed(image: bpy.types.Image) -> Outcome[None]:
                    slots = image.render_slots
                    rollback.callback(written, slots, ("active_index",), slots.active_index)
                    match TargetSlot[record.slot]:
                        case TargetSlot.DISPLAYED:
                            return Option.of_optional(slots.active).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(image), member="render_slots.active")).bind(renamed)
                        case TargetSlot.NEW:
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
                options = RenderCall(animation=record.animation, write_still=record.write_still and not record.animation, layer=chosen)
                outcome = render(identity, scene, options)
                return outcome.bind(lambda _: bound.restored(day, scene)).map(lambda _: Done())

            with ExitStack() as rollback:
                if (
                    outcome := (
                        paired(Snapshot.capture(scene, owned=identity.owns), bound.routing.pathed.path(scene, day).map(lambda path: (bound.routing.pathed.attached.struct(scene).version, path)))
                        .bind(lambda values: prepared(values).bind(lambda _: slotted(rollback, values[1][0])))
                        .bind(rendered)
                    )
                ).is_ok():
                    rollback.pop_all()
            return outcome.or_else_with(lambda faults: paired(Error(faults), bound.restored(day, scene)).map(lambda _: Done()))

        def checked(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[Done]:
            return bound.recovery.stored.read(scene).bind(lambda held: Error(Fault.of(Reason.TAKEN, repr(scene), member=bound.recovery.stored.name)) if held.is_some() else scene_render(scene, layer))

        def selected(scene: bpy.types.Scene) -> Outcome[Done]:
            def launched(layer: bpy.types.ViewLayer) -> Outcome[Done]:
                with call.context.temp_override(scene=scene, view_layer=layer):
                    return checked(scene, layer)

            return chosen.map(lambda name: layer_of(scene, name)).default_with(lambda: rooted(bpy.types.ViewLayer, call.context)).bind(launched)

        return rooted(bpy.types.Scene, call.context).bind(selected)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds declarations and override recovery, restoring held scenes at file load, after a crash left them, and at scope exit."""
        with ExitStack() as scope:

            def built(bound: RenderBinding) -> Outcome[Acquisitions]:
                def loaded() -> Outcome[None]:
                    return accumulated(map(partial(bound.restored, today()), bpy.data.scenes)).map(lambda _: None)

                scope.callback(lambda: identity.reports.reported(loaded)().to_optional())
                return Ok((
                    PresetMenu(source=Captured(preset=self.presets, scope=Scenes(), selection=(("render",), ("cycles",), ("eevee",)))),
                    PresetMenu(source=Captured(preset=self.bakes, scope=Scenes(), selection=(("render", "bake"),))),
                    self.rendering,
                    self.viewporting,
                    Handler(handlers=bpy.app.handlers.load_post, body=loaded),
                ))

            yield self.bound(identity).bind(built)

    # --- [SECTIONS]

    @cached_property
    def section(self) -> Section[RenderOverride]:
        """Render buttons and override settings, their stored values kept under the render history retention, the slots registering what the buttons bind."""
        return Section(
            key="render",
            rows=(Layout(rows=(OperatorButton(target=self.rendering), OperatorButton(target=self.viewporting)), prerequisites=(self,)),),
            store=Some(Store(attachment=RenderSlots.override, retention=Some(self.retained))),
        )

    @staticmethod
    def retained(identity: Identity) -> Outcome[Retention]:
        """Reads the render history retention."""
        return RENDER_HISTORY.read(identity).map(lambda held: held.retention)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SOURCES]


def sourced(scenes: tuple[bpy.types.Scene, ...]) -> tuple[bpy.types.Scene, ...]:
    """Returns scenes and every scene their render-enabled groups read through unmuted Render Layers or render Cryptomatte nodes, each once."""
    reached = dict.fromkeys(scenes)
    pending = deque(reached)
    while pending:
        scene = pending.popleft()
        for source in (
            live.scene
            for tree in nested((entry.node_group for entry in scene.compositor_effects if entry.enable_for_render and entry.node_group is not None), muted=False)
            for live in tree.nodes
            if not live.mute and reads_render(live) and live.scene is not None
        ):
            if source not in reached:
                reached[source] = None
                pending.append(source)
    return tuple(reached)


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
    member = ("engine",)
    try:
        answer = accumulated(assigned(render, {member: engine}) for render, _ in held).bind(lambda _: body())
    finally:
        restored = accumulated(assigned(render, {member: prior}) for render, prior in held)
    return paired(answer, restored).map(lambda pair: pair[0])


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

__all__ = ["RENDER_HISTORY", "Isolation", "Launch", "Render", "RenderBinding", "RenderCall", "RenderOverride", "RenderSlots", "TargetSlot", "engined", "isolation", "native", "reads_render", "sourced"]
