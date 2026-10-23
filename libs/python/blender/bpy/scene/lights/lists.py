"""Emitter list with solo, bulk power edits, and emissive selection."""

from collections.abc import Callable
from typing import Annotated, ClassVar

from bl_ui.space_view3d import VIEW3D_MT_select_object
import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.operators import answered, Done, Empty, Operator, Outcome, Unchanged
from blender.bpy.data.rna.descriptors import root_in
from blender.bpy.data.rna.snapshots import Applied, Scenes, Scope, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderResult
from blender.bpy.scene.lights.emitters import emitters, emitting, focused, picked, strengths
from blender.bpy.scene.objects import ObjectState, reconciled
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import acquired, Member, OperatorButton, Property, SearchField
from blender.bpy.ui.layout.sections import Section, Single
from blender.bpy.ui.surfaces.appends import Append
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Gated, NonNegative, Presentation, Quantity

# --- [MODELS] ---------------------------------------------------------------------------


class Visibility(msgspec.Struct, frozen=True, kw_only=True):
    """Emitter visibility before solo."""

    render: Annotated[bool, msgspec.Meta(description="Shown in renders")] = True
    viewport: Annotated[bool, msgspec.Meta(description="Shown in viewports")] = True


class Bulk(msgspec.Struct, frozen=True, kw_only=True):
    """Multiplier on power and emission of emitters in scope."""

    scope: Annotated[Scope, msgspec.Meta(description="Owners to change")] = Scenes()
    factor: Annotated[NonNegative, msgspec.Meta(description="Power and emission strength multiplier"), Presentation(soft=Some((0.1, 10.0)), decimals=Some(3))] = 1.0
    temperature: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=800.0, le=20000.0)]],
        msgspec.Meta(description="Blackbody color of every light"),
        Presentation(step=Some(1.0), decimals=Some(0), quantity=Some(Quantity.COLOR_TEMPERATURE)),
    ] = Gated(enabled=False, value=6500.0)


class Emissives(msgspec.Struct, frozen=True, kw_only=True):
    """Scope of emissive objects to select."""

    scope: Annotated[Scope, msgspec.Meta(description="Owners to select from")] = Scenes()


class LightList(msgspec.Struct, frozen=True, kw_only=True):
    """Emitter list with solo, bulk, and select operators."""

    solo: ClassVar[Attachment[Gated[Visibility]]] = Attachment(key="solo", form=Gated[Visibility], owner=bpy.types.Object)

    commands: ClassVar[tuple[Operator[Empty], Operator[Empty], Operator[Bulk], Operator[Emissives]]] = (
        Operator(
            key="light_solo",
            title="Solo Light",
            description="Hide every other scene emitter, storing visibility for Unsolo Lights",
            form=Empty,
            run=lambda call: paired(paired(call.root(bpy.types.ViewLayer).bind(focused), call.root(bpy.types.Scene)), LightList.solo.bound(call.identity)).bind(lambda pair: soloed(*pair[0], pair[1])),
        ),
        Operator(
            key="light_unsolo",
            title="Unsolo Lights",
            description="Restore emitters solo hid",
            form=Empty,
            run=lambda call: paired(call.root(bpy.types.Scene), LightList.solo.bound(call.identity)).bind(lambda pair: unsoloed(*pair)),
        ),
        Operator(key="light_bulk", title="Multiply Lights", description="Multiply power and emission in scope", form=Bulk, run=lambda call: multiplied(call.record, call.context)),
        Operator(
            key="emissives_select",
            title="Select Emissive Objects",
            description="Select emissive objects in scope",
            form=Emissives,
            run=lambda call: call.root(bpy.types.ViewLayer).bind(lambda layer: emissives_selected(call.record.scope, call.context, layer)).map(lambda _: Done()),
        ),
    )

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns solo attachment, emissive select menu entry, and section."""
        *_, select = self.commands
        return Ok((
            LightList.solo,
            Append(target=VIEW3D_MT_select_object, rows=(OperatorButton(target=select),)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            *acquired((self.section(),)),
        ))

    def section(self) -> Section:
        """Returns emitter list, active light rows, and operator buttons."""
        solo, unsolo, bulk, select = self.commands

        def light[K: bpy.types.Light](kind: type[K]) -> Callable[[bpy.types.Context], Option[K]]:
            return lambda context: (
                root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)).bind(lambda item: Some(data) if isinstance(data := item.data, kind) else Nothing)
            )

        return Single(
            key="light_list",
            title="Lights",
            rows=(
                ItemList[msgspec.Struct, bpy.types.Object](
                    key="emitters",
                    item=bpy.types.Object,
                    collection=Member(root=bpy.types.Scene, path="objects"),
                    active=Member(root=bpy.types.LayerObjects, path="active"),
                    columns=(
                        Property(target=Member(root=bpy.types.Object, path="hide_viewport"), text=Some(""), icon_only=True),
                        Property(target=Member(root=bpy.types.Object, path="hide_render"), text=Some(""), icon_only=True),
                        SearchField(target=Member(root=bpy.types.Object, path="lightgroup"), collection=Member(root=bpy.types.ViewLayer, path="lightgroups"), text=Some("")),
                    ),
                    where=Some(lambda _context, item: emitting(item)),
                    order=Some("type"),
                    side=(OperatorButton(target=solo), OperatorButton(target=unsolo)),
                ),
                *(Property(target=Member(root=bpy.types.Light, path=member, reach=Some(light(bpy.types.Light)))) for member in ("energy", "use_temperature", "temperature")),
                *(
                    Property(target=Member(root=kind, path=member, reach=Some(light(kind))))
                    for kind, member in ((bpy.types.PointLight, "shadow_soft_size"), (bpy.types.SpotLight, "shadow_soft_size"), (bpy.types.AreaLight, "size"), (bpy.types.SunLight, "angle"))
                ),
                OperatorButton(target=bulk),
                OperatorButton(target=select),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def soloed(chosen: bpy.types.Object, scene: bpy.types.Scene, solo: Attached[Gated[Visibility]]) -> BlenderResult[Outcome]:
    """Hides every other scene emitter, storing prior visibility.

    Args:
        chosen: Emitter left shown.
        scene: Scene with emitters to hide.
        solo: Bound solo attachment.

    Returns:
        `Done`, `Unchanged` when solo hides every other emitter, or `read` and `edit` faults.
    """

    def hid(item: bpy.types.Object) -> BlenderResult[None]:
        return (
            solo
            .write(item, Gated(enabled=True, value=Visibility(render=not item.hide_render, viewport=not item.hide_viewport)))
            .bind(lambda _: reconciled(ObjectState(values=Some(_shown(Visibility(render=False, viewport=False)))), item, ()))
            .map(lambda _: None)
        )

    def edited(rows: tuple[tuple[bpy.types.Object, Gated[Visibility]], ...]) -> BlenderResult[Outcome]:
        restoring = tuple((item, held.value) for item, held in rows if held.enabled and (item == chosen or item not in reached))
        hiding = tuple(item for item, held in rows if item in reached and item != chosen and not held.enabled)
        if not (restoring or hiding):
            return Ok(Unchanged(reason="Solo hides every other emitter"))
        return accumulated((*(_restored(item, prior, solo) for item, prior in restoring), *(hid(item) for item in hiding))).map(lambda _: Done())

    reached = frozenset(emitters(scene))
    return accumulated(solo.read(item).map(lambda held: (item, held)) for item in scene.objects if item in reached or solo.stored(item)).bind(edited)


def unsoloed(scene: bpy.types.Scene, solo: Attached[Gated[Visibility]]) -> BlenderResult[Outcome]:
    """Restores prior visibility of every emitter solo hid.

    Args:
        scene: Scene with emitters to restore.
        solo: Bound solo attachment.

    Returns:
        `Done`, `Unchanged` when solo hides no emitter, or `read` and `edit` faults.
    """

    def edited(rows: tuple[tuple[bpy.types.Object, Gated[Visibility]], ...]) -> BlenderResult[Outcome]:
        if not (hidden := tuple((item, held.value) for item, held in rows if held.enabled)):
            return Ok(Unchanged(reason="Solo hides no emitter"))
        return accumulated(_restored(item, prior, solo) for item, prior in hidden).map(lambda _: Done())

    return accumulated(solo.read(item).map(lambda held: (item, held)) for item in scene.objects if solo.stored(item)).bind(edited)


def multiplied(bulk: Bulk, context: bpy.types.Context) -> BlenderResult[Outcome]:
    """Multiplies emitter power and emission strength in scope.

    Args:
        bulk: Scope, factor, and gated temperature.
        context: Context `scope.owners` reads.

    Returns:
        `Done` with skipped faults when a write changed, `Unchanged` when no value changes, or `absent`, `edit`, and `choice` faults when none changed.
    """

    def powered(data: bpy.types.Light) -> Applied:
        energy: float = data.path_resolve("energy")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        values = {"energy": energy * bulk.factor, **({"use_temperature": True, "temperature": bulk.temperature.value} if bulk.temperature.enabled else {})}
        return Snapshot.of(type(data), values).apply(data)

    def edited(reached: tuple[bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object, ...]) -> BlenderResult[Outcome]:
        items = tuple(dict.fromkeys(item for owner in reached for item in emitters(owner)))
        lights = dict.fromkeys(data for item in items if isinstance(data := item.data, bpy.types.Light))
        materials = dict.fromkeys(material for item in items for slot in item.material_slots if (material := slot.material) is not None)
        sockets = dict.fromkeys(socket for material in materials for socket in strengths(material) if not any(link.is_valid and not link.is_muted for link in socket.links))  # type: ignore[union-attr]  # ty: ignore[not-iterable]
        return answered(
            Applied.join((*(powered(data) for data in lights), *(Snapshot.of(type(socket), {"default_value": socket.default_value * bulk.factor}).apply(socket) for socket in sockets))),
            "No light or unlinked emission strength in scope takes a new value",
        )

    return bulk.scope.owners(context).bind(edited)


def emissives_selected(scope: Scope, context: bpy.types.Context, layer: bpy.types.ViewLayer) -> BlenderResult[None]:
    """Selects every emissive object in scope on layer."""
    return scope.owners(context).bind(lambda reached: picked(frozenset(item for owner in reached for item in emitters(owner) if not isinstance(item.data, bpy.types.Light)), layer))


def _shown(visibility: Visibility) -> Snapshot:
    """Returns `Object` snapshot writing render and viewport visibility."""
    return Snapshot.of(bpy.types.Object, {"hide_render": not visibility.render, "hide_viewport": not visibility.viewport})


def _restored(item: bpy.types.Object, prior: Visibility, solo: Attached[Gated[Visibility]]) -> BlenderResult[None]:
    """Restores emitter prior visibility, storing it with gate off."""
    return reconciled(ObjectState(values=Some(_shown(prior))), item, ()).bind(lambda _: solo.write(item, Gated(enabled=False, value=prior)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bulk", "Emissives", "LightList", "Visibility", "emissives_selected", "multiplied", "soloed", "unsoloed"]
