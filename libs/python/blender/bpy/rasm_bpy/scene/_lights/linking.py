"""Light and shadow linking sets of emitters, set members, and member link states."""

from collections.abc import Mapping
from types import MappingProxyType
from typing import Annotated, ClassVar, Final

import bpy
from expression import Option, Some
import msgspec

from rasm_bpy.commands.operators import Answer, Call, called, Done, Operator, Placement, Unchanged
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._ids.ownership import Ownership, Saved
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.data._rna.paths import root_in, rooted, struct_at
from rasm_bpy.data._rna.properties import choices
from rasm_bpy.scene._collections import created, relinked
from rasm_bpy.scene._objects import active_object
from rasm_bpy.ui._layout import members, rows
from rasm_bpy.ui._layout.members import bound
from rasm_bpy.ui._layout.rows import realized
from rasm_records import accumulated, Nothing, Ok, Outcome

# --- [CONSTANTS] ------------------------------------------------------------------------

LINK_STATE: Final = Descriptor.of(bpy.types.CollectionLightLinking, ("link_state",), bpy.types.EnumProperty)
RECEIVERS: Final = Descriptor(root=bpy.types.ObjectLightLinking, prop=bpy.types.ObjectLightLinking.bl_rna.properties["receiver_collection"])
_BLOCKERS: Final = Descriptor(root=bpy.types.ObjectLightLinking, prop=bpy.types.ObjectLightLinking.bl_rna.properties["blocker_collection"])
_LINKERS: Final = MappingProxyType({RECEIVERS: bpy.ops.object.light_linking_receivers_link, _BLOCKERS: bpy.ops.object.light_linking_blockers_link})
_NAMED: Final = MappingProxyType({link.prop.identifier: link for link in _LINKERS})

# --- [MODELS] ---------------------------------------------------------------------------


class Unlinking(bpy.types.PropertyGroup):
    """Linking set the selected objects leave."""

    collection: Annotated[
        str, bpy.props.EnumProperty(name="Set", description="Light or shadow linking set", items=choices((link.prop.identifier, link.prop.name, link.prop.description) for link in _LINKERS))
    ]


class _LightLinks(msgspec.Struct, frozen=True, kw_only=True):
    """Receiver and blocker sets with the stock link operators and one unlink operator."""

    sets: ClassVar[Mapping[Descriptor, Ownership[bpy.types.Collection]]] = MappingProxyType({
        link: Ownership(kind=bpy.types.Collection, key=link.prop.identifier, saved=Saved.USED) for link in _LINKERS
    })

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns set ownerships and the unlink operator."""
        return *self.sets.values(), light_linking_unlink

    def drawn(self, identity: Identity, /) -> Outcome[rows.RowDraw]:
        """Returns each set's linking template with its stock buttons, then the selection unlink button of each set apart from them."""

        def reach(context: bpy.types.Context) -> Option[bpy.types.ObjectLightLinking]:
            return root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)).map(lambda item: item.light_linking)

        def linking(link: Descriptor) -> Outcome[rows.RowDraw]:
            stock = (rows.OperatorButton(target=Placement(_LINKERS[link])), rows.OperatorButton(target=Placement(bpy.ops.object.light_linking_unlink_from_collection)))
            view = members.Member(root=bpy.types.ObjectLightLinking, path=(link.prop.identifier,), reach=Some(reach))
            return bound(identity, (view,), lambda located, layout, _holder, _context: layout.template_light_linking_collection(layout, *located[0][1])).bind(
                lambda shown: realized((shown, *stock), identity)
            )

        unlinks = tuple(rows.OperatorButton(target=Placement(light_linking_unlink, {"collection": link.prop.identifier})) for link in self.sets)
        return accumulated(map(linking, self.sets)).bind(lambda draws: realized((*draws, *unlinks), identity))


# --- [OPERATIONS] -----------------------------------------------------------------------


def linked(
    emitter: bpy.types.Object, link: Descriptor, members: tuple[bpy.types.Object, ...], state: str, owner: bpy.types.Object, context: bpy.types.Context, identity: Identity
) -> Outcome[bpy.types.Collection]:
    """Links objects into an emitter set at a state through the set's stock link operator.

    The set is the collection in the emitter's link slot, an owned one created for an empty slot.

    Args:
        emitter: Object with set receiving members.
        link: Receiver or blocker member of `ObjectLightLinking`.
        members: Objects to link.
        state: `link_state` identifier.
        owner: Object an owned collection serves, emitter or rig target.
        context: Context the stock operator runs under with the emitter active and the objects selected.
        identity: Identity marking an owned collection.

    Returns:
        Set collection, or faults of the slot collection and the operator call.
    """
    return (
        struct_at(emitter.light_linking, link.path, bpy.types.Collection)
        .map(Ok)
        .default_with(
            lambda: _LightLinks.sets[link].made(
                identity, lambda: created(Nothing, f"{owner.name} {link.prop.name}"), of=Some(owner), filled=lambda held: link.write(emitter.light_linking, held).map(lambda _: held)
            )
        )
        .bind(lambda held: called(_LINKERS[link], {LINK_STATE.prop.identifier: state}, override=Some(context.temp_override(active_object=emitter, selected_objects=list(members)))).map(lambda _: held))
    )


def _unlinked(layer: bpy.types.ViewLayer, emitter: bpy.types.Object, link: Descriptor) -> Outcome[Answer]:
    """Unlinks selected objects other than the emitter from its set, `Unchanged` when the set stays."""
    if not (selected := tuple(item for item in layer.objects.selected if item != emitter)):
        return Ok(Unchanged(reason="No object besides emitter is selected"))
    return (
        struct_at(emitter.light_linking, link.path, bpy.types.Collection)
        .map(lambda collection: relinked(collection, selected, linked=False).map(lambda changes: Done() if changes else Unchanged(reason=f"No selected object is in {collection.name}")))
        .default_value(Ok(Unchanged(reason=f"{emitter.name} holds no {link.prop.name}")))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Unlinking)
def light_linking_unlink(call: Call[Unlinking]) -> Outcome[Answer]:
    """Unlinks selected objects other than the active emitter from its light or shadow linking set."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: active_object(layer).bind(lambda emitter: _unlinked(layer, emitter, _NAMED[call.properties.collection])))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LINK_STATE", "RECEIVERS", "Unlinking", "light_linking_unlink", "linked"]
