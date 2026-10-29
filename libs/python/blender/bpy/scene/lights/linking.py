"""Light and shadow linking sets of emitters, set members, and member link states."""

from collections.abc import Mapping
from functools import partial
from types import MappingProxyType
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.calls import called, Operation, Placement
from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import root_in, rooted, Struct, struct_at
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.scene.collections import entry, founded, Member, relinked
from blender.bpy.scene.objects import focused
from blender.bpy.ui.layout import members, rows
from blender.records.faults import accumulated, Outcome, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

LINK_STATE: Final = Descriptor.declared(bpy.types.CollectionLightLinking, ("link_state",), bpy.types.EnumProperty)
RECEIVERS: Final = Descriptor.declared(bpy.types.ObjectLightLinking, ("receiver_collection",), bpy.types.PointerProperty)
BLOCKERS: Final = Descriptor.declared(bpy.types.ObjectLightLinking, ("blocker_collection",), bpy.types.PointerProperty)
_LINKERS: Final[Mapping[Descriptor[bpy.types.PointerProperty], Operation]] = MappingProxyType({
    RECEIVERS: bpy.ops.object.light_linking_receivers_link,
    BLOCKERS: bpy.ops.object.light_linking_blockers_link,
})
_SETS: Final = MappingProxyType({link: Ownership(kind=bpy.types.Collection, key=link.prop.identifier, saved=Saved.USED) for link in _LINKERS})
_NAMED: Final = MappingProxyType({link.prop.identifier: link for link in _LINKERS})

# --- [MODELS] ---------------------------------------------------------------------------


class Unlinking(bpy.types.PropertyGroup):
    """Linking set the selected objects leave."""

    collection: bpy.props.EnumProperty(name="Set", description="Light or shadow linking set", items=choices((link.prop.identifier, link.prop.name, link.prop.description) for link in _LINKERS))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LightLinks(msgspec.Struct, frozen=True, kw_only=True):
    """Receiver and blocker sets with the stock link operators and one unlink operator."""

    sets: Mapping[Descriptor[bpy.types.PointerProperty], Ownership[bpy.types.Collection]] = _SETS

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns set ownerships and the unlink operator."""
        return Ok((*self.sets.values(), light_linking_unlink_command))

    def drawn(self, identity: Identity, /) -> Outcome[rows.RowDraw]:
        """Returns each set's linking template with its stock buttons, then the selection unlink button of each set apart from them."""

        def reach(context: bpy.types.Context) -> Option[Struct]:
            return root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)).map(lambda item: item.light_linking)

        def linking(link: Descriptor[bpy.types.PointerProperty]) -> Outcome[rows.RowDraw]:
            stock = (rows.OperatorButton(target=Placement(_LINKERS[link])), rows.OperatorButton(target=Placement(bpy.ops.object.light_linking_unlink_from_collection)))
            view = members.Member(root=bpy.types.ObjectLightLinking, path=(link.prop.identifier,), reach=Some(reach))
            return rows.bound(((view, ()),), lambda held: partial(_collections, held[0])).bind(lambda shown: rows.realized((shown, *stock), identity))

        unlinks = tuple(rows.OperatorButton(target=Placement(light_linking_unlink_command, {"collection": link.prop.identifier})) for link in self.sets)
        return accumulated(map(linking, self.sets)).bind(lambda draws: rows.realized((*draws, *unlinks), identity))


# --- [OPERATIONS] -----------------------------------------------------------------------


def link_set(emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty], owner: bpy.types.Object, identity: Identity) -> Outcome[bpy.types.Collection]:
    """Returns collection in emitter link slot, creating an owned one for an empty slot.

    Args:
        emitter: Object with slot to read.
        link: Receiver or blocker member of `ObjectLightLinking`.
        owner: Object the owned collection serves, emitter or rig target.
        identity: Identity marking owned collection.

    Returns:
        Slot collection, or `collision` and `edit` faults.
    """
    return (
        _held(emitter, link)
        .map(Ok)
        .default_with(
            lambda: _SETS[link].made(
                identity, lambda: founded(Nothing, f"{owner.name} {link.prop.name}"), of=Some(owner), filled=lambda held: link.write(emitter.light_linking, held).map(lambda _: held)
            )
        )
    )


def linked(
    emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty], members: tuple[Member, ...], state: str, owner: bpy.types.Object, context: bpy.types.Context, identity: Identity
) -> Outcome[bpy.types.Collection]:
    """Links members into emitter set at state, objects through the set's stock link operator and collections through the set's entries.

    Args:
        emitter: Object with set receiving members.
        link: Receiver or blocker member of `ObjectLightLinking`.
        members: Objects and collections to link.
        state: `link_state` identifier.
        owner: Object an owned collection serves.
        context: Context the stock operator runs under with the emitter active and the objects selected.
        identity: Identity marking an owned collection.

    Returns:
        Set collection, or faults of `link_set`, the operator call, and the collection entry writes.
    """
    objects = tuple(member for member in members if isinstance(member, bpy.types.Object))
    collections = tuple(member for member in members if isinstance(member, bpy.types.Collection))

    def operated() -> Outcome[None]:
        override = Some(context.temp_override(active_object=emitter, selected_objects=list(objects)))
        return called(_LINKERS[link], {LINK_STATE.prop.identifier: state}, override=override).map(lambda _: None) if objects else Ok(None)

    def entered(held: bpy.types.Collection) -> Outcome[None]:
        return relinked(held, collections, linked=True).bind(
            lambda _: accumulated(entry(held, member).bind(lambda found: LINK_STATE.write(found.light_linking, state)) for member in collections).map(lambda _: None)
        )

    return link_set(emitter, link, owner, identity).bind(lambda held: paired(operated(), entered(held)).map(lambda _: held))


def unlinked(layer: bpy.types.ViewLayer, emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty]) -> Outcome[Answer]:
    """Unlinks selected objects other than the emitter from its set, `Unchanged` when the set stays."""
    if not (selected := tuple(item for item in layer.objects.selected if item != emitter)):
        return Ok(Unchanged(reason="No object besides emitter is selected"))
    return (
        _held(emitter, link)
        .map(lambda collection: relinked(collection, selected, linked=False).map(lambda changes: Done() if changes else Unchanged(reason=f"No selected object is in {collection.name}")))
        .default_value(Ok(Unchanged(reason=f"{emitter.name} holds no {link.prop.name}")))
    )


def _held(emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty]) -> Option[bpy.types.Collection]:
    """Returns the collection in the emitter's link slot, read through the slot's path."""
    return struct_at(emitter.light_linking, link.path, bpy.types.Collection)


def _collections(held: members.ResolvedMember, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the linking collection view at the located set, the drawing layout taking its active element as context."""
    for owner, key in held.at(holder, context).to_list():
        layout.template_light_linking_collection(layout, owner, key)


# --- [COMPOSITION] ----------------------------------------------------------------------


def light_linking_unlink(call: Call[Unlinking]) -> Outcome[Answer]:
    """Unlinks selected objects other than the active emitter from its light or shadow linking set."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: focused(layer).bind(lambda emitter: unlinked(layer, emitter, _NAMED[call.properties.collection])))


light_linking_unlink_command: Operator[Unlinking] = Operator(form=Unlinking, run=light_linking_unlink)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BLOCKERS", "LINK_STATE", "RECEIVERS", "LightLinks", "Unlinking", "light_linking_unlink_command", "link_set", "linked", "unlinked"]
