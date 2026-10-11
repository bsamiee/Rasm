"""Emitter list with solo by stored prior, bulk power and emission edits, and emissive selection."""

from collections.abc import Callable
import sys
from typing import Annotated, ClassVar

from bl_ui.space_view3d import VIEW3D_MT_select_object
import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from rasm.blender.commands.operators import Done, Empty, Operator
from rasm.blender.data.rna.descriptors import root_in, written
from rasm.blender.data.rna.snapshots import Scenes, Scope, Snapshot
from rasm.blender.extension.identity import Identity
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderResult
from rasm.blender.scene.lights.emitters import emitters, emitting, focused, picked, strengths
from rasm.blender.scene.objects import ObjectState, reconciled
from rasm.blender.settings.attachments import Attached, Attachment
from rasm.blender.ui.layout.lists import ItemList
from rasm.blender.ui.layout.rows import acquired, Member, OperatorButton, Property, SearchField
from rasm.blender.ui.layout.sections import Section, Single
from rasm.blender.ui.surfaces.appends import Append
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Gated, Presentation, Quantity

# --- [MODELS] ---------------------------------------------------------------------------


class Visibility(msgspec.Struct, frozen=True, kw_only=True):
    """Render and viewport visibility of an emitter before a solo."""

    render: Annotated[bool, msgspec.Meta(title="Render", description="Shown in renders before the solo")] = True
    viewport: Annotated[bool, msgspec.Meta(title="Viewport", description="Shown in viewports before the solo")] = True


class Bulk(msgspec.Struct, frozen=True, kw_only=True):
    """One multiplier on the power and emission of a scope's emitters."""

    scope: Annotated[Scope, msgspec.Meta(title="Scope", description="Owners whose emitters change")] = Scenes()
    factor: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Factor", description="Multiplier on each light's power and each emission strength"),
        Presentation(soft=Some((0.1, 10.0)), decimals=Some(3)),
    ] = 1.0
    temperature: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=800.0, le=20000.0)]],
        msgspec.Meta(title="Temperature", description="Blackbody color every light of the scope takes"),
        Presentation(step=Some(1.0), decimals=Some(0), quantity=Some(Quantity.COLOR_TEMPERATURE)),
    ] = Gated(enabled=False, value=6500.0)


class Emissives(msgspec.Struct, frozen=True, kw_only=True):
    """Scope whose emissive objects select."""

    scope: Annotated[Scope, msgspec.Meta(title="Scope", description="Owners whose emissive objects select")] = Scenes()


class LightList(msgspec.Struct, frozen=True, kw_only=True):
    """Row of the emitter list with its solo, bulk, and select operators."""

    solo: ClassVar[Attachment[Gated[Visibility]]] = Attachment(key="solo", form=Gated[Visibility], owner=bpy.types.Object)

    commands: ClassVar[tuple[Operator[Empty], Operator[Empty], Operator[Bulk], Operator[Emissives]]] = (
        Operator(
            key="light_solo",
            title="Solo Light",
            description="Hide every other emitter of the scene, keeping each one's visibility to restore",
            form=Empty,
            run=lambda call: (
                paired(paired(call.root(bpy.types.ViewLayer).bind(focused), call.root(bpy.types.Scene)), LightList.solo.bound(call.identity))
                .bind(lambda pair: soloed(*pair[0], pair[1]))
                .map(lambda _: Done())
            ),
        ),
        Operator(
            key="light_unsolo",
            title="Unsolo Lights",
            description="Show every emitter a solo hid as it was",
            form=Empty,
            run=lambda call: paired(call.root(bpy.types.Scene), LightList.solo.bound(call.identity)).bind(lambda pair: unsoloed(*pair)).map(lambda _: Done()),
        ),
        Operator(
            key="light_bulk",
            title="Multiply Lights",
            description="Multiply the power and emission of a scope's emitters",
            form=Bulk,
            run=lambda call: multiplied(call.record, call.context).map(lambda _: Done()),
        ),
        Operator(
            key="emissives_select",
            title="Select Emissive Objects",
            description="Select every object whose material emits",
            form=Emissives,
            run=lambda call: call.root(bpy.types.ViewLayer).bind(lambda layer: emissives_selected(call.record.scope, call.context, layer)).map(lambda _: Done()),
        ),
    )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the solo attachment, the four operators, and the emissive select menu entry.

        Args:
            identity: Extension placing the row.

        Returns:
            `Ok` of the attachment, operators, append, and section declarations.
        """
        *_, select = self.commands
        return Ok((
            LightList.solo,
            Append(target=VIEW3D_MT_select_object, rows=(OperatorButton(target=select),)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            *acquired((self.section(),)),
        ))

    def section(self) -> Section:
        """Section drawing the emitter list with its solo buttons and the active light's own rows."""
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
                    where=Some(lambda context, item: emitting(item)),
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


def soloed(chosen: bpy.types.Object, scene: bpy.types.Scene, solo: Attached[Gated[Visibility]]) -> BlenderResult[None]:
    """Hides every other emitter of a scene, storing each one's prior visibility.

    Args:
        chosen: Emitter left shown.
        scene: Scene whose emitters hide.
        solo: Bound solo attachment.

    Returns:
        `Ok` once every write landed; `read` for a stored gate the record refuses and the `edit` faults of each refused write.
    """

    def hid(item: bpy.types.Object) -> BlenderResult[None]:
        return (
            solo
            .write(item, Gated(enabled=True, value=Visibility(render=not item.hide_render, viewport=not item.hide_viewport)))
            .bind(lambda _: reconciled(ObjectState(values=Some(_shown(Visibility(render=False, viewport=False)))), item, ()))
            .map(lambda _: None)
        )

    reached = frozenset(emitters(scene))
    return (
        accumulated(solo.read(item).map(lambda held: (item, held)) for item in scene.objects if item in reached or solo.stored(item))
        .bind(
            lambda rows: accumulated((
                *(_restored(item, held.value, solo) for item, held in rows if held.enabled and (item == chosen or item not in reached)),
                *(hid(item) for item, held in rows if item in reached and item != chosen and not held.enabled),
            ))
        )
        .map(lambda _: None)
    )


def unsoloed(scene: bpy.types.Scene, solo: Attached[Gated[Visibility]]) -> BlenderResult[None]:
    """Writes back the prior visibility of every emitter a solo hid.

    Args:
        scene: Scene whose emitters return.
        solo: Bound solo attachment.

    Returns:
        `Ok` once every write landed; `read` for a stored gate the record refuses and the `edit` faults of each refused write.
    """
    return accumulated(solo.read(item).bind(lambda held: _restored(item, held.value, solo) if held.enabled else Ok(None)) for item in scene.objects if solo.stored(item)).map(lambda _: None)


def multiplied(bulk: Bulk, context: bpy.types.Context) -> BlenderResult[None]:
    """Multiplies the power and emission strength of a scope's emitters.

    Args:
        bulk: Scope, factor, and gated temperature.
        context: Context the scope's `owners` reads.

    Returns:
        `Ok` once every write landed; `absent` for a missing scope name and `edit` or `choice` per refused write.
    """

    def powered(data: bpy.types.Light) -> BlenderResult[None]:
        energy: float = data.path_resolve("energy")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        values = {"energy": energy * bulk.factor, **({"use_temperature": True, "temperature": bulk.temperature.value} if bulk.temperature.enabled else {})}
        return Snapshot.of(type(data), values).apply(data).outcome

    def edited(reached: tuple[bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object, ...]) -> BlenderResult[None]:
        items = tuple(dict.fromkeys(item for owner in reached for item in emitters(owner)))
        lights = dict.fromkeys(data for item in items if isinstance(data := item.data, bpy.types.Light))
        materials = dict.fromkeys(material for item in items for slot in item.material_slots if (material := slot.material) is not None)
        sockets = dict.fromkeys(socket for material in materials for socket in strengths(material))
        return accumulated((
            *(powered(data) for data in lights),
            *(written(socket, "default_value", socket.default_value * bulk.factor) for socket in sockets if not any(link.is_valid and not link.is_muted for link in socket.links)),  # type: ignore[union-attr]  # ty: ignore[not-iterable]
        )).map(lambda _: None)

    return bulk.scope.owners(context).bind(edited)


def emissives_selected(scope: Scope, context: bpy.types.Context, layer: bpy.types.ViewLayer) -> BlenderResult[None]:
    """Selects every emissive object a scope reaches.

    Args:
        scope: Owners whose emissive objects select.
        context: Context supplying the scope's owners.
        layer: View layer receiving the selection.

    Returns:
        `Ok` once written; the faults of the scope's `owners` and of `picked`.
    """
    return scope.owners(context).bind(lambda reached: picked(frozenset(item for owner in reached for item in emitters(owner) if not isinstance(item.data, bpy.types.Light)), layer))


def _shown(visibility: Visibility) -> Snapshot:
    """`Object` values snapshot showing an emitter in renders and viewports as a visibility states."""
    return Snapshot.of(bpy.types.Object, {"hide_render": not visibility.render, "hide_viewport": not visibility.viewport})


def _restored(item: bpy.types.Object, prior: Visibility, solo: Attached[Gated[Visibility]]) -> BlenderResult[None]:
    """Writes an emitter's prior visibility back and stores its gate off with the prior kept."""
    return reconciled(ObjectState(values=Some(_shown(prior))), item, ()).bind(lambda _: solo.write(item, Gated(enabled=False, value=prior)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bulk", "Emissives", "LightList", "Visibility", "emissives_selected", "multiplied", "soloed", "unsoloed"]
