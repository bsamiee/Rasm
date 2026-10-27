"""Emitter list with solo, bulk power edits, and emissive selection."""

from functools import partial
from typing import ClassVar

from bl_ui.space_view3d import VIEW3D_MT_select_object
import bpy
from expression import Ok, Option, Some, try_downcast
import msgspec

from blender.bpy.commands.operators import Answer, answered, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.rna.descriptors import assigned, mirrored, root_in
from blender.bpy.data.rna.snapshots import Applied, Scenes, Scope, scoped, SCOPES, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.scene.lights.emitters import emitters, emitting, feeding, picked, strengths, TEMPERATURE
from blender.bpy.scene.objects import focused, ObjectState, reconciled
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import acquired, Member, OperatorButton, Property, SearchField
from blender.bpy.ui.layout.sections import Section, Single
from blender.bpy.ui.surfaces.appends import Append
from blender.records.faults import accumulated, Outcome, paired

# --- [MODELS] ---------------------------------------------------------------------------


class Solo(bpy.types.PropertyGroup):
    """Emitter visibility before solo, held while solo hides the emitter."""

    hidden: bpy.props.BoolProperty(name="Hidden", description="Solo hides the emitter")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    render: bpy.props.BoolProperty(name="Render", description="Shown in renders before solo", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    viewport: bpy.props.BoolProperty(name="Viewport", description="Shown in viewports before solo", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Bulk(bpy.types.PropertyGroup):
    """Multiplier on power and emission of emitters in scope, with a blackbody color for every light."""

    scope: bpy.props.EnumProperty(name="Scope", description="Owners to change", items=SCOPES, default=Scenes.__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    collection: bpy.props.StringProperty(name="Collection", description="Collection whose objects a collection scope changes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    factor: bpy.props.FloatProperty(name="Factor", description="Power and emission strength multiplier", min=0.0, soft_min=0.1, soft_max=10.0, precision=3, default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_temperature: bpy.props.BoolProperty(name="Write Temperature", description="Write a blackbody color to every light")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    temperature: bpy.props.FloatProperty(**mirrored(TEMPERATURE.prop))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Emissives(bpy.types.PropertyGroup):
    """Scope of emissive objects to select."""

    scope: bpy.props.EnumProperty(name="Scope", description="Owners to select from", items=SCOPES, default=Scenes.__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    collection: bpy.props.StringProperty(name="Collection", description="Collection whose objects a collection scope selects from")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LightList(msgspec.Struct, frozen=True, kw_only=True):
    """Emitter list with solo, bulk, and select operators."""

    solo: ClassVar[Attachment[Solo]] = Attachment(key="solo", form=Solo, owner=bpy.types.Object)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns solo attachment, emissive select menu entry, and section."""
        return Ok((LightList.solo, Append(target=VIEW3D_MT_select_object, rows=(OperatorButton(target=emissives_select_command),)), *acquired((self.section(),))))

    @staticmethod
    def section() -> Section:
        """Returns emitter list, active light rows, and operator buttons."""

        def light[K: bpy.types.Light](kind: type[K], context: bpy.types.Context) -> Option[K]:
            return root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)).bind(lambda item: Option.of_optional(try_downcast(kind, item.data)))

        return Single(
            key="light_list",
            title="Lights",
            rows=(
                ItemList[bpy.types.PropertyGroup, bpy.types.Object](
                    key="emitters",
                    item=bpy.types.Object,
                    collection=Member(root=bpy.types.Scene, path=("objects",)),
                    active=Member(root=bpy.types.LayerObjects, path=("active",)),
                    columns=(
                        Property(target=Member(root=bpy.types.Object, path=("hide_viewport",)), text="", icon_only=True),
                        Property(target=Member(root=bpy.types.Object, path=("hide_render",)), text="", icon_only=True),
                        SearchField(target=Member(root=bpy.types.Object, path=("lightgroup",)), collection=Member(root=bpy.types.ViewLayer, path=("lightgroups",)), text=""),
                    ),
                    where=Some(lambda _context, item: emitting(item)),
                    order=Some("type"),
                    side=(OperatorButton(target=light_solo_command), OperatorButton(target=light_unsolo_command)),
                ),
                *(Property(target=Member(root=bpy.types.Light, path=(member,), reach=Some(partial(light, bpy.types.Light)))) for member in ("energy", "use_temperature", "temperature")),
                *(
                    Property(target=Member(root=kind, path=(member,), reach=Some(partial(light, kind))))
                    for kind, member in ((bpy.types.PointLight, "shadow_soft_size"), (bpy.types.SpotLight, "shadow_soft_size"), (bpy.types.AreaLight, "size"), (bpy.types.SunLight, "angle"))
                ),
                OperatorButton(target=light_bulk_command),
                OperatorButton(target=emissives_select_command),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def soloed(chosen: bpy.types.Object, scene: bpy.types.Scene, solo: Attached[Solo]) -> Outcome[Answer]:
    """Hides every other scene emitter, storing prior visibility.

    Args:
        chosen: Emitter left shown.
        scene: Scene with emitters to hide.
        solo: Bound solo attachment.

    Returns:
        `Done`, `Unchanged` when solo hides every other emitter, or `edit` faults.
    """

    def hid(item: bpy.types.Object) -> Outcome[None]:
        return (
            assigned(solo.struct(item), {"hidden": True, "render": not item.hide_render, "viewport": not item.hide_viewport})
            .bind(lambda _: reconciled(ObjectState(values=Some(_shown(render=False, viewport=False))), item, ()))
            .map(lambda _: None)
        )

    reached = frozenset(emitters(scene))
    rows = tuple((item, solo.struct(item)) for item in scene.objects if item in reached or solo.stored(item))
    restoring = tuple(item for item, held in rows if held.hidden and (item == chosen or item not in reached))
    hiding = tuple(item for item, held in rows if item in reached and item != chosen and not held.hidden)
    if not (restoring or hiding):
        return Ok(Unchanged(reason="Solo hides every other emitter"))
    return accumulated((*(_restored(item, solo) for item in restoring), *(hid(item) for item in hiding))).map(lambda _: Done())


def unsoloed(scene: bpy.types.Scene, solo: Attached[Solo]) -> Outcome[Answer]:
    """Restores prior visibility of every emitter solo hid.

    Args:
        scene: Scene with emitters to restore.
        solo: Bound solo attachment.

    Returns:
        `Done`, `Unchanged` when solo hides no emitter, or `edit` faults.
    """
    if not (hidden := tuple(item for item in scene.objects if solo.stored(item) and solo.struct(item).hidden)):
        return Ok(Unchanged(reason="Solo hides no emitter"))
    return accumulated(_restored(item, solo) for item in hidden).map(lambda _: Done())


def multiplied(factor: float, temperature: Option[float], scope: Scope, context: bpy.types.Context) -> Outcome[Answer]:
    """Multiplies emitter power and emission strength in scope.

    Args:
        factor: Power and emission strength multiplier.
        temperature: Blackbody color written to every light, `Nothing` leaving light colors.
        scope: Owners to change.
        context: Context `scope.owners` reads.

    Returns:
        `Done` with skipped faults when a write changed, `Unchanged` when no value changes, or `absent`, `edit`, and `choice` faults when none changed.
    """

    def powered(data: bpy.types.PointLight | bpy.types.SpotLight | bpy.types.AreaLight | bpy.types.SunLight) -> Applied:
        values = {"energy": data.energy * factor, **temperature.map(lambda kelvin: {"use_temperature": True, "temperature": kelvin}).default_value({})}
        return Snapshot.of(type(data), values).apply(data)

    def edited(reached: tuple[bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object, ...]) -> Outcome[Answer]:
        items = tuple(dict.fromkeys(item for owner in reached for item in emitters(owner)))
        lights = dict.fromkeys(data for item in items if isinstance(data := item.data, bpy.types.PointLight | bpy.types.SpotLight | bpy.types.AreaLight | bpy.types.SunLight))
        materials = dict.fromkeys(material for item in items for slot in item.material_slots if (material := slot.material) is not None)
        sockets = dict.fromkeys(socket for material in materials for socket in strengths(material) if not feeding(socket))
        return answered(
            Applied.join((*(powered(data) for data in lights), *(Snapshot.of(type(socket), {"default_value": socket.default_value * factor}).apply(socket) for socket in sockets))),
            "No light or unlinked emission strength in scope takes a new value",
        )

    return scope.owners(context).bind(edited)


def emissives_selected(scope: Scope, context: bpy.types.Context, layer: bpy.types.ViewLayer) -> Outcome[None]:
    """Selects every emissive object in scope on layer."""
    return scope.owners(context).bind(lambda reached: picked(frozenset(item for owner in reached for item in emitters(owner) if not isinstance(item.data, bpy.types.Light)), layer))


def _shown(*, render: bool, viewport: bool) -> Snapshot:
    """Returns `Object` snapshot writing render and viewport visibility."""
    return Snapshot.of(bpy.types.Object, {"hide_render": not render, "hide_viewport": not viewport})


def _restored(item: bpy.types.Object, solo: Attached[Solo]) -> Outcome[None]:
    """Restores emitter visibility solo held, clearing its solo mark."""
    held = solo.struct(item)
    return reconciled(ObjectState(values=Some(_shown(render=held.render, viewport=held.viewport))), item, ()).bind(lambda _: assigned(held, {"hidden": False}))


# --- [COMPOSITION] ----------------------------------------------------------------------


def light_solo(call: Call[Empty]) -> Outcome[Answer]:
    """Hides every other scene emitter, storing visibility for unsolo."""
    return paired(paired(call.root(bpy.types.ViewLayer).bind(focused), call.root(bpy.types.Scene)), LightList.solo.bound(call.identity)).bind(lambda pair: soloed(*pair[0], pair[1]))


def light_unsolo(call: Call[Empty]) -> Outcome[Answer]:
    """Restores the emitters solo hid."""
    return paired(call.root(bpy.types.Scene), LightList.solo.bound(call.identity)).bind(lambda pair: unsoloed(*pair))


def light_bulk(call: Call[Bulk]) -> Outcome[Answer]:
    """Multiplies power and emission in scope."""
    return multiplied(
        call.properties.factor, Some(call.properties.temperature).filter(lambda _: call.properties.use_temperature), scoped(call.properties.scope, call.properties.collection), call.context
    )


def emissives_select(call: Call[Emissives]) -> Outcome[Answer]:
    """Selects emissive objects in scope."""
    return call.root(bpy.types.ViewLayer).bind(lambda layer: emissives_selected(scoped(call.properties.scope, call.properties.collection), call.context, layer)).map(lambda _: Done())


light_solo_command: Operator[Empty] = Operator(form=Empty, run=light_solo)
light_unsolo_command: Operator[Empty] = Operator(form=Empty, run=light_unsolo)
light_bulk_command: Operator[Bulk] = Operator(form=Bulk, run=light_bulk)
emissives_select_command: Operator[Emissives] = Operator(form=Emissives, run=emissives_select)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Bulk",
    "Emissives",
    "LightList",
    "Solo",
    "emissives_select_command",
    "emissives_selected",
    "light_bulk_command",
    "light_solo_command",
    "light_unsolo_command",
    "multiplied",
    "soloed",
    "unsoloed",
]
