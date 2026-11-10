"""Emitter list with solo, bulk power edits, and emissive selection."""

from functools import partial
from typing import Annotated, ClassVar

from bl_ui.space_view3d import VIEW3D_MT_select_object
import bpy
from expression import Option, Result, Some, try_downcast
import msgspec

from rasm_bpy.commands.operators import Answer, answered, Call, Done, Empty, Operator, Unchanged
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import acquired_rows, Acquisitions
from rasm_bpy.data._rna.conditions import chosen, Scenes, scope_choice, SCOPE_COLLECTION
from rasm_bpy.data._rna.descriptors import Value
from rasm_bpy.data._rna.paths import RnaPath, root_in, rooted
from rasm_bpy.data._rna.snapshots import Applied, assigned, Snapshot
from rasm_bpy.scene._lights.emitters import emitters, emitting, picked, strengths, TEMPERATURE
from rasm_bpy.scene._objects import active_object
from rasm_bpy.settings._attachments import Attached, Attachment
from rasm_bpy.ui._elements.menus import Append
from rasm_bpy.ui._layout.lists import ItemList
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import OperatorButton, Property
from rasm_bpy.ui._layout.sections import Section
from rasm_records import accumulated, Error, Ok, Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------

type _LightData = bpy.types.PointLight | bpy.types.SpotLight | bpy.types.AreaLight | bpy.types.SunLight

# --- [MODELS] ---------------------------------------------------------------------------


class Solo(bpy.types.PropertyGroup):
    """Emitter visibility before solo, held while solo hides the emitter."""

    hidden: Annotated[bool, bpy.props.BoolProperty(name="Hidden", description="Solo hides the emitter")]
    render: Annotated[bool, bpy.props.BoolProperty(name="Render", description="Shown in renders before solo", default=True)]
    viewport: Annotated[bool, bpy.props.BoolProperty(name="Viewport", description="Shown in viewports before solo", default=True)]


class Bulk(bpy.types.PropertyGroup):
    """Multiplier on power and emission of emitters in scope, with a blackbody color for every light."""

    scope: Annotated[str, scope_choice(Scenes)]
    collection: Annotated[str, SCOPE_COLLECTION]
    factor: Annotated[float, bpy.props.FloatProperty(name="Factor", description="Power and emission strength multiplier", min=0.0, soft_min=0.1, soft_max=10.0, precision=3, default=1.0)]
    use_temperature: Annotated[bool, bpy.props.BoolProperty(name="Write Temperature", description="Write a blackbody color to every light")]
    temperature: Annotated[float, TEMPERATURE]


class Emissives(bpy.types.PropertyGroup):
    """Scope of emissive objects to select."""

    scope: Annotated[str, scope_choice(Scenes)]
    collection: Annotated[str, SCOPE_COLLECTION]


class _LightList(msgspec.Struct, frozen=True, kw_only=True):
    """Emitter list with solo, bulk, and select operators."""

    solo: ClassVar[Attachment[Solo]] = Attachment(key="solo", form=Solo, owner=bpy.types.Object)

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns solo attachment, emissive select menu entry, and section."""
        return _LightList.solo, Append(target=VIEW3D_MT_select_object, rows=(OperatorButton(target=emissives_select),)), *acquired_rows((self.section(),))

    @staticmethod
    def section() -> Section:
        """Returns emitter list, active light rows, and operator buttons."""

        def light[K: bpy.types.Light](kind: type[K], context: bpy.types.Context) -> Option[K]:
            return root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)).bind(lambda item: Option.of_optional(try_downcast(kind, item.data)))

        return Section(
            key="light_list",
            title=Some("Lights"),
            rows=(
                ItemList[bpy.types.PropertyGroup, bpy.types.Object](
                    key="emitters",
                    collection=Member(root=bpy.types.Scene, path=("objects",)),
                    active=Some(Member(root=bpy.types.LayerObjects, path=("active",))),
                    cells=(
                        Property(target=Member(root=bpy.types.Object, path=("hide_viewport",)), keywords={"text": "", "icon_only": True}),
                        Property(target=Member(root=bpy.types.Object, path=("hide_render",)), keywords={"text": "", "icon_only": True}),
                        Property(target=Member(root=bpy.types.Object, path=("lightgroup",)), keywords={"text": ""}, search=Some(Member(root=bpy.types.ViewLayer, path=("lightgroups",)))),
                    ),
                    where=Some(lambda _context, _ui, item: Ok(emitting(item))),
                    order=Some("type"),
                    side=(OperatorButton(target=light_solo), OperatorButton(target=light_unsolo)),
                ),
                *(
                    Property(target=Member(root=kind, path=(member,), reach=Some(partial(light, kind))))
                    for kind, size in ((bpy.types.PointLight, "shadow_soft_size"), (bpy.types.SpotLight, "shadow_soft_size"), (bpy.types.AreaLight, "size"), (bpy.types.SunLight, "angle"))
                    for member in ("energy", size)
                ),
                *(Property(target=Member(root=bpy.types.Light, path=(member,), reach=Some(partial(light, bpy.types.Light)))) for member in ("use_temperature", "temperature")),
                OperatorButton(target=light_bulk),
                OperatorButton(target=emissives_select),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _shown(*, render: bool, viewport: bool) -> Snapshot:
    """Returns `Object` snapshot writing render and viewport visibility."""
    return Snapshot.of(bpy.types.Object, {("hide_render",): not render, ("hide_viewport",): not viewport})


def _restored(item: bpy.types.Object, solo: Attached[Solo]) -> Outcome[None]:
    """Restores emitter visibility solo held, clearing its solo mark."""
    held = solo.struct(item)
    return _shown(render=held.render, viewport=held.viewport).apply(item).outcome.bind(lambda _: assigned(held, {("hidden",): False}))


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Empty)
def light_solo(call: Call[Empty]) -> Outcome[Answer]:
    """Hides every other scene emitter, storing visibility for unsolo."""
    match paired(paired(rooted(bpy.types.ViewLayer, call.context).bind(active_object), rooted(bpy.types.Scene, call.context)), _LightList.solo.bound(call.identity)):
        case Result(tag="ok", ok=((chosen, scene), solo)):
            reached = frozenset(emitters(scene))
            rows = tuple((item, solo.struct(item)) for item in scene.objects if item in reached or solo.stored(item))
            restoring = tuple(item for item, held in rows if held.hidden and (item == chosen or item not in reached))
            hiding = tuple(item for item, held in rows if item in reached and item != chosen and not held.hidden)
            hidden = _shown(render=False, viewport=False)
            return (
                accumulated((
                    *(_restored(item, solo) for item in restoring),
                    *(
                        assigned(solo.struct(item), {("hidden",): True, ("render",): not item.hide_render, ("viewport",): not item.hide_viewport}).bind(lambda _: hidden.apply(item).outcome)
                        for item in hiding
                    ),
                )).map(lambda _: Done())
                if restoring or hiding
                else Ok(Unchanged(reason="Solo hides every other emitter"))
            )
        case Result(error=faults):
            return Error(faults)


@Operator.of(Empty)
def light_unsolo(call: Call[Empty]) -> Outcome[Answer]:
    """Restores the emitters solo hid."""
    return paired(rooted(bpy.types.Scene, call.context), _LightList.solo.bound(call.identity)).bind(
        lambda pair: (
            accumulated(_restored(item, pair[1]) for item in hidden).map(lambda _: Done())
            if (hidden := tuple(item for item in pair[0].objects if pair[1].stored(item) and pair[1].struct(item).hidden))
            else Ok(Unchanged(reason="Solo hides no emitter"))
        )
    )


@Operator.of(Bulk)
def light_bulk(call: Call[Bulk]) -> Outcome[Answer]:
    """Multiplies power and emission in scope."""
    factor = call.properties.factor
    colored = dict[RnaPath, Value]({("use_temperature",): True, ("temperature",): call.properties.temperature} if call.properties.use_temperature else {})
    return (
        chosen(call.properties)
        .bind(lambda scope: scope.owners(call.context))
        .map(lambda reached: tuple(dict.fromkeys(item for owner in reached for item in emitters(owner))))
        .bind(
            lambda items: answered(
                Applied.join((
                    *(
                        Snapshot.of(type(light), {("energy",): light.energy * factor, **colored}).apply(light)
                        for light in dict.fromkeys(item.data for item in items if isinstance(item.data, _LightData.__value__))
                    ),
                    *(
                        Snapshot.of(type(socket), {("default_value",): socket.default_value * factor}).apply(socket)
                        for socket in dict.fromkeys(
                            socket
                            for material in dict.fromkeys(slot.material for item in items for slot in item.material_slots if slot.material is not None)
                            for socket, linked in strengths(material)
                            if not linked
                        )
                    ),
                )),
                "No light or unlinked emission strength in scope takes a new value",
            )
        )
    )


@Operator.of(Emissives)
def emissives_select(call: Call[Emissives]) -> Outcome[Answer]:
    """Selects emissive objects in scope."""
    return (
        paired(rooted(bpy.types.ViewLayer, call.context), chosen(call.properties))
        .bind(
            lambda pair: (
                pair[1].owners(call.context).bind(lambda reached: picked(frozenset(item for owner in reached for item in emitters(owner) if not isinstance(item.data, bpy.types.Light)), pair[0]))
            )
        )
        .map(lambda _: Done())
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bulk", "Emissives", "Solo", "emissives_select", "light_bulk", "light_solo", "light_unsolo"]
