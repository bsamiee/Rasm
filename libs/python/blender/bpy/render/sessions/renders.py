"""Session renders and scoped scene overrides."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import datetime
from functools import reduce
from itertools import starmap
from pathlib import Path
from typing import Annotated, ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Acquired, Call, Done, Operator, Properties, Stock
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.snapshots import Endpoints, Persisted, Scenes, Snapshot
from blender.bpy.extension.events.handlers import PathHandler
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.render.effects.stack import EffectStack
from blender.bpy.render.outputs.formats import Holder
from blender.bpy.render.outputs.framing import Framed, FrameSize
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.sessions import execution
from blender.bpy.render.view_layers.passes import PassRouting, PassSet
from blender.bpy.settings.attachments import Attached, Attachment, endpoints, Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import Member, OperatorButton, Property, Pulldown
from blender.bpy.ui.layout.sections import Hosted, Listing, Single, Store
from blender.bpy.ui.layout.templates import ImageFormat
from blender.bpy.ui.surfaces.menus import Captured, PresetMenu
from blender.imaging.faults import accumulated, paired
from blender.imaging.hashing import content_hash, ContentHash
from blender.imaging.parameters import Choice, Gated
from blender.rendering.edits import Tree
from blender.rendering.history import Retention

# --- [TYPES] ----------------------------------------------------------------------------


class Into(Choice):
    """Native Render Result slot receiving the next render."""

    DISPLAYED = 1, "Displayed Slot"
    NEW = 2, "New Slot"


class Isolation(Choice):
    """Render state assigned to unselected scene objects."""

    HOLDOUT = 1, "Holdout"
    HIDDEN = 2, "Hidden"


# --- [ROSTERS] --------------------------------------------------------------------------

value_sets: Preset[Snapshot] = Preset(key="render_override", form=Snapshot)
value_set_names = Choices(value_sets.entries)
layers = Choices(
    lambda _identity, _owner, context: (
        (layer.name, layer.name, "") for held in Option.of_optional(context).to_list() for scene in Option.of_optional(held.scene).to_list() for layer in scene.view_layers
    )
)

# --- [MODELS] ---------------------------------------------------------------------------


class Launch(msgspec.Struct, frozen=True, kw_only=True):
    """Final or viewport render and its native destination."""

    animation: Annotated[bool, msgspec.Meta(title="Animation")] = False
    write: Annotated[bool, msgspec.Meta(title="Write Still")] = False
    into: Annotated[Into, msgspec.Meta(title="Slot")] = Into.DISPLAYED
    layer: Annotated[Gated[Annotated[str, layers]], msgspec.Meta(title="View Layer")] = Gated(enabled=False, value="")


class ValueSet(msgspec.Struct, frozen=True, kw_only=True):
    """Stored native scene values applied for one render."""

    name: Annotated[str, msgspec.Meta(title="Value Set"), value_set_names]


class Isolated(msgspec.Struct, frozen=True, kw_only=True):
    """Selection isolation and optional selected-object shadow catching."""

    mode: Annotated[Isolation, msgspec.Meta(title="Others")] = Isolation.HOLDOUT
    catcher: Annotated[bool, msgspec.Meta(title="Shadow Catcher")] = False


class RenderOverride(msgspec.Struct, frozen=True, kw_only=True):
    """Scene values and selection isolation held for a native render."""

    values: Gated[ValueSet] = Gated(enabled=False, value=ValueSet(name=""))
    isolated: Gated[Isolated] = Gated(enabled=False, value=Isolated())


# --- [SERVICES] -------------------------------------------------------------------------


class RenderSlots(msgspec.Struct, frozen=True, kw_only=True):
    """Native final and viewport renders with scoped settings."""

    stack: EffectStack

    render: ClassVar[Key] = "render"
    viewport: ClassVar[Key] = "render_viewport"
    override: ClassVar[Attachment[RenderOverride]] = Attachment(key="render_override", form=RenderOverride, owner=bpy.types.Scene)
    bakes: ClassVar[Preset[Snapshot]] = Preset(key="bake", form=Snapshot)
    page_rows: ClassVar[tuple[Registrable, ...]] = (override,)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Retains native declarations and recovery state through render retirement."""
        scope = ExitStack()
        pending: set[bpy.types.Scene] = set()

        def built(parts: tuple[tuple[tuple[Attached[RenderOverride], Framed], PassRouting], Stored[Endpoints]]) -> BlenderResult[Acquisitions]:
            ((held, framed), routing), recovery = parts

            def restored(scene: bpy.types.Scene) -> BlenderResult[Option[str]]:
                def reverted(state: Endpoints) -> BlenderResult[None]:
                    before = state.before

                    def restored(after: Persisted) -> BlenderResult[None]:
                        missing = Tree(roots=tuple(key for key in before.roots if key not in after.owners), owners={key: held for key, held in before.owners.items() if key not in after.owners})
                        return (
                            Snapshot
                            .reverted(before, after)
                            .bind(lambda _: routing.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date()))
                            .bind(lambda path: routing.reconciled(scene, bpy.context, path))
                            .bind(
                                lambda _: (
                                    recovery.write(scene, Endpoints(before=missing)).bind(lambda _: Error((BlenderAbsentFault(owner=repr(scene), key=f"{recovery.name}.after"),)))
                                    if missing.owners
                                    else recovery.removed(scene).map(lambda _: None)
                                )
                            )
                        )

                    return state.completed(scene, recovery.name).bind(restored)

                def finished(_: None) -> BlenderResult[Option[str]]:
                    pending.discard(scene)
                    return Ok(Nothing)

                return recovery.read(scene).bind(lambda state: framed.restored(scene).bind(lambda _: state.map(reverted).default_value(Ok(None))).bind(finished))

            finishing = scope.enter_context(identity.reports.deferred(restored))

            def seeded() -> BlenderResult[Option[float]]:
                pending.update(bpy.data.scenes)
                return Ok(Nothing)

            def ticked() -> BlenderResult[Option[float]]:
                if not bpy.app.is_job_running("RENDER"):
                    for scene in tuple(pending):
                        finishing(scene)
                return Ok(Some(0.25))

            def loaded(_: Option[Path]) -> BlenderResult[None]:
                pending.clear()
                return accumulated(restored(scene) for scene in bpy.data.scenes).map(lambda _: None)

            scope.callback(lambda: identity.reports.reported(lambda: accumulated(restored(scene) for scene in tuple(pending)).map(lambda _: None))().to_optional())

            def launched(call: Call[Launch], viewport: bool) -> BlenderResult[Done]:
                def scene_render(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> BlenderResult[Done]:
                    record = call.record
                    images = tuple(image for image in bpy.data.images if image.type == "RENDER_RESULT")
                    prior = tuple((image, image.render_slots.active_index, Option.of_optional(image.render_slots.active).map(lambda slot: (slot, slot.name))) for image in images)
                    created: list[tuple[bpy.types.Image, bpy.types.RenderSlot]] = []
                    chosen = Some(record.layer.value) if record.layer.enabled and not viewport else Nothing
                    member = chosen.default_value(scene.name)

                    def prepared(values: tuple[tuple[RenderOverride, Snapshot], tuple[int, OutputPath]]) -> BlenderResult[None]:
                        (override, captured), (version, path) = values
                        presets = (
                            value_sets
                            .found(identity, override.values.value.name)
                            .bind(lambda source: value_sets.read(source) if isinstance(source, Path) else Ok(source))
                            .map(lambda snapshot: (snapshot,))
                            if override.values.enabled
                            else Ok(())
                        )
                        isolated = isolation(scene, layer, override.isolated.value) if override.isolated.enabled else Ok(())
                        stamp = noted(member, version, content_hash(Snapshot, captured))

                        def overridden(owned: tuple[tuple[Snapshot, ...], tuple[tuple[bpy.types.Object, Snapshot], ...]]) -> BlenderResult[None]:
                            owners = (scene, *(owner for owner, _ in owned[1]))

                            def changed(before: Persisted) -> BlenderResult[None]:
                                pending.add(scene)
                                applied = paired(
                                    reduce(lambda outcome, snapshot: outcome.bind(lambda _: snapshot.apply(scene).outcome), (*owned[0], stamp), Result[None, BlenderFaults](ok=None)),
                                    accumulated(snapshot.apply(owner).outcome for owner, snapshot in owned[1]),
                                ).bind(lambda _: routing.reconciled(scene, call.context, path))
                                final = tuple(Snapshot.capture(owner, native=True) for owner in owners)
                                trees = tuple(tree for answer in final for tree in answer.to_option().to_list())
                                after = Tree.joined(trees)
                                endpoint = Snapshot.persisted(after).bind(lambda projected: recovery.write(scene, Endpoints(before=before, after=projected)))
                                return paired(applied, paired(accumulated(final), endpoint)).map(lambda _: None)

                            return (
                                accumulated(Snapshot.capture(owner, native=True) for owner in owners)
                                .map(Tree.joined)
                                .bind(Snapshot.persisted)
                                .bind(lambda before: recovery.write(scene, Endpoints(before=before)).bind(lambda _: changed(before)))
                            )

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
                            restored(scene),
                            *starmap(removed, created),
                            *(written(image.render_slots, "active_index", index) for image, index, _ in prior),
                            *(written(slot, "name", name) for _, _, held in prior for slot, name in held.to_list()),
                        )).map(lambda _: Done())

                    def called_render(_: None) -> BlenderResult[Done]:
                        properties = {"animation": record.animation, "write_still": record.write and not record.animation}
                        outcome = (
                            called(bpy.ops.render.opengl, properties | {"view_context": True}, context="INVOKE_DEFAULT")
                            if viewport
                            else self.stack.precision(call.identity).bind(lambda precision: execution.rendered(call.identity, self.stack, scene, precision, properties | {"layer": chosen}))
                        )
                        return outcome.bind(lambda status: restored(scene).map(lambda _: Done()) if "FINISHED" in status else Ok(Done()))

                    day = datetime.datetime.now(datetime.UTC).astimezone().date()
                    return (
                        paired(paired(held.read(scene), Snapshot.capture(scene)), paired(routing.pathed.attached.read(scene).map(lambda outputs: outputs.version), routing.pathed.path(scene, day)))
                        .bind(lambda values: prepared(values).bind(lambda _: slotted(values[1][0])).bind(called_render))
                        .or_else_with(undone)
                    )

                def admitted(roots: tuple[bpy.types.Scene, bpy.types.ViewLayer]) -> BlenderResult[Done]:
                    scene = roots[0]
                    return recovery.read(scene).bind(lambda held: Error((BlenderCollisionFault(request=repr(scene), error=recovery.name),)) if held.is_some() else scene_render(*roots))

                return paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(admitted)

            viewport = Setting(descriptor=Descriptor(root=bpy.types.SpaceView3D, owner="", prop=bpy.types.SpaceView3D.bl_rna.properties["type"]), accepted=("VIEW_3D",))
            return Ok((
                self.override,
                PresetMenu(source=Captured(preset=value_sets, scope=Scenes(), selection=("render", "cycles", "eevee"))),
                PresetMenu(source=Captured(preset=self.bakes, scope=Scenes(), selection=("render.bake",))),
                Operator(
                    key=self.render,
                    title="Render",
                    description="Render into the selected Render Result slot with scoped scene values",
                    form=Launch,
                    invoke=Some(Properties()),
                    undo=False,
                    run=lambda call: launched(call, False),
                ),
                Operator(
                    key=self.viewport,
                    title="Render Viewport",
                    description="Render the 3D view with scoped scene values",
                    form=Launch,
                    invoke=Some(Properties()),
                    undo=False,
                    condition=Some(viewport),
                    run=lambda call: launched(call, True),
                ),
                PathHandler(handlers=bpy.app.handlers.load_post, body=loaded),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                Timer(body=seeded),
                Timer(body=ticked),
            ))

        try:
            recovery = identity.name(Name.ATTACHMENT, "render_restore").map(endpoints)
            yield paired(paired(paired(self.override.bound(identity), FrameSize().bound(identity)), PassSet().bound(identity)), recovery).bind(built)
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

    @staticmethod
    def section(retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing) -> Single[RenderOverride]:
        """Places native render actions beside the context scene's scoped override."""
        return Single(
            key="render",
            title="Render",
            rows=(OperatorButton(target=Acquired(key=RenderSlots.render)), OperatorButton(target=Acquired(key=RenderSlots.viewport))),
            store=Some(Hosted(host=Member(root=bpy.types.Context, path="scene"), store=Store(attachment=RenderSlots.override, retention=retention))),
        )

    @staticmethod
    def baking() -> Single:
        """Places the native bake action, writable settings, format, and preset menu."""
        return Single(
            key="bake",
            title="Bake",
            rows=(
                OperatorButton(target=Stock(operator=bpy.ops.object.bake)),
                *(Property(target=Member(root=bpy.types.Scene, path=f"render.bake.{prop.identifier}")) for prop in Holder.BAKE.struct.bl_rna.properties if not (prop.is_readonly or prop.is_hidden)),
                ImageFormat(settings=Member(root=bpy.types.Scene, path=f"render.bake.{Holder.BAKE.pointer}")),
                Pulldown(menu=Listing.keyed(RenderSlots.bakes)),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def isolation(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, isolated: Isolated) -> BlenderResult[tuple[tuple[bpy.types.Object, Snapshot], ...]]:
    """Constructs native object settings for selection isolation."""

    def values(item: bpy.types.Object, paths: tuple[str, ...]) -> dict[str, bool]:
        holdout, visible, catcher = paths
        return ({catcher: True} if isolated.catcher else {}) if item.select_get(view_layer=layer) else {holdout: True} if isolated.mode is Isolation.HOLDOUT else {visible: False}

    return accumulated(Descriptor.resolve(bpy.types.Object, name, bpy.types.BoolProperty) for name in ("is_holdout", "visible_camera", "is_shadow_catcher")).map(
        lambda descriptors: tuple((item, Snapshot.of(bpy.types.Object, values(item, tuple(descriptor.path for descriptor in descriptors)))) for item in scene.objects)
    )


def noted(member: str, version: int, key: ContentHash) -> Snapshot:
    """Constructs the render stamp's member, version, and original settings digest."""
    return Snapshot.of(bpy.types.Scene, {"render.use_stamp_note": True, "render.stamp_note_text": f"{member} v{version} {key.hex()}"})


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Into", "Isolated", "Isolation", "Launch", "RenderOverride", "RenderSlots", "ValueSet", "isolation", "noted", "value_sets"]
