"""Renders with temporary scene overrides."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import datetime
from itertools import starmap
from pathlib import Path
from typing import Annotated, ClassVar, Final

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called, Stock
from blender.bpy.commands.operators import Call, Done, Operator, Properties
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.snapshots import Endpoints, Scenes, Snapshot
from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.render.effects.stack import EffectStack
from blender.bpy.render.outputs.formats import pointer
from blender.bpy.render.outputs.framing import Framed, FrameSize
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.sessions.execution import RenderCall, rendered
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
from blender.imaging.faults import accumulated, paired
from blender.imaging.hashing import content_hash, ContentHash
from blender.imaging.parameters import Choice, Gated

# --- [TYPES] ----------------------------------------------------------------------------


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

# --- [CHOICES] --------------------------------------------------------------------------

value_sets: Preset[Snapshot] = Preset(key="render_override", form=Snapshot)
value_set_names = Choices(value_sets.entries)
layers = Choices(
    lambda _identity, _owner, context: (
        (layer.name, layer.name, "") for held in Option.of_optional(context).to_list() for scene in Option.of_optional(held.scene).to_list() for layer in scene.view_layers
    )
)

# --- [MODELS] ---------------------------------------------------------------------------


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


class RenderSlots:
    """Render operators with temporary scene overrides."""

    stack: EffectStack
    rendering: Operator[Launch]
    viewporting: Operator[Launch]
    override: ClassVar[Attachment[RenderOverride]] = Attachment(key="render_override", form=RenderOverride, owner=bpy.types.Scene)
    bakes: ClassVar[Preset[Snapshot]] = Preset(key="bake", form=Snapshot)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (override, FrameSize.attachment, PassSet.attachment)

    # --- [COMMANDS]

    def __init__(self, *, stack: EffectStack) -> None:
        """Declares slot and viewport render commands, each binding overrides on call."""
        self.stack = stack
        self.rendering = Operator(
            key="render",
            title="Render",
            description="Render into chosen slot with temporary scene overrides",
            form=Launch,
            invoke=Some(Properties()),
            undo=False,
            run=lambda call: self.launched(call, viewport=False),
        )
        self.viewporting = Operator(
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

    def restored(self, bound: Overriding, scene: bpy.types.Scene) -> BlenderResult[Option[str]]:
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
                        called(bpy.ops.render.opengl, msgspec.structs.asdict(options) | {"view_context": True}, context="INVOKE_DEFAULT")
                        if viewport
                        else self.stack.precision(call.identity).bind(lambda precision: rendered(call.identity, self.stack, scene, precision, options))
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
        """Returns section of render buttons and override settings."""
        return Single(
            key="render",
            title="Render",
            rows=(OperatorButton(target=self.rendering), OperatorButton(target=self.viewporting)),
            store=Some(Hosted(store=Store(attachment=RenderSlots.override, retention=retention))),
        )

    @staticmethod
    def baking() -> Single:
        """Returns section of bake button, settings, image format, and presets."""
        return Single(
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
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


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

__all__ = ["HOLDOUT", "SHADOW_CATCHER", "VISIBLE_CAMERA", "Into", "Isolated", "Isolation", "Launch", "RenderOverride", "RenderSlots", "ValueSet", "isolation", "noted", "value_sets"]
