"""Light and shadow linking sets of emitters, set members, and member link states."""

from collections.abc import Mapping
from types import MappingProxyType
from typing import ClassVar, Final

import bpy
from expression import Option, Some
import msgspec

from rasm_bpy.commands.operators import Answer, Call, called, Done, Operation, Operator, Placement, Unchanged
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._ids.ownership import Ownership, Saved
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.data._rna.paths import _struct_at, root_in, rooted, Struct
from rasm_bpy.data._rna.properties import choices
from rasm_bpy.scene._collections import _entry, _Member, _relinked, created
from rasm_bpy.scene._objects import _active_object
from rasm_bpy.ui._layout import members, rows
from rasm_bpy.ui._layout.members import _bound
from rasm_bpy.ui._layout.rows import _realized
from rasm_records import accumulated, Nothing, Ok, Outcome, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

_LINK_STATE: Final = Descriptor[bpy.types.EnumProperty](root=bpy.types.CollectionLightLinking, prop=bpy.types.CollectionLightLinking.bl_rna.properties["link_state"])  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
_RECEIVERS: Final = Descriptor[bpy.types.PointerProperty](root=bpy.types.ObjectLightLinking, prop=bpy.types.ObjectLightLinking.bl_rna.properties["receiver_collection"])  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
_BLOCKERS: Final = Descriptor[bpy.types.PointerProperty](root=bpy.types.ObjectLightLinking, prop=bpy.types.ObjectLightLinking.bl_rna.properties["blocker_collection"])  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
_LINKERS: Final[Mapping[Descriptor[bpy.types.PointerProperty], Operation]] = MappingProxyType({
    _RECEIVERS: bpy.ops.object.light_linking_receivers_link,
    _BLOCKERS: bpy.ops.object.light_linking_blockers_link,
})
_NAMED: Final = MappingProxyType({link.prop.identifier: link for link in _LINKERS})

# --- [MODELS] ---------------------------------------------------------------------------


class Unlinking(bpy.types.PropertyGroup):
    """Linking set the selected objects leave."""

    collection: bpy.props.EnumProperty(name="Set", description="Light or shadow linking set", items=choices((link.prop.identifier, link.prop.name, link.prop.description) for link in _LINKERS))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class _LightLinks(msgspec.Struct, frozen=True, kw_only=True):
    """Receiver and blocker sets with the stock link operators and one unlink operator."""

    sets: ClassVar[Mapping[Descriptor[bpy.types.PointerProperty], Ownership[bpy.types.Collection]]] = MappingProxyType({
        link: Ownership(kind=bpy.types.Collection, key=link.prop.identifier, saved=Saved.USED) for link in _LINKERS
    })

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns set ownerships and the unlink operator."""
        return *self.sets.values(), light_linking_unlink

    def drawn(self, identity: Identity, /) -> Outcome[rows.RowDraw]:
        """Returns each set's linking template with its stock buttons, then the selection unlink button of each set apart from them."""

        def reach(context: bpy.types.Context) -> Option[Struct]:
            return root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)).map(lambda item: item.light_linking)  # ty: ignore[unsound-return-statement]

        def linking(link: Descriptor[bpy.types.PointerProperty]) -> Outcome[rows.RowDraw]:
            stock = (rows.OperatorButton(target=Placement(_LINKERS[link])), rows.OperatorButton(target=Placement(bpy.ops.object.light_linking_unlink_from_collection)))
            view = members.Member(root=bpy.types.ObjectLightLinking, path=(link.prop.identifier,), reach=Some(reach))
            return _bound(identity, (view,), lambda located, layout, _holder, _context: layout.template_light_linking_collection(layout, *located[0][1])).bind(
                lambda shown: _realized((shown, *stock), identity)
            )

        unlinks = tuple(rows.OperatorButton(target=Placement(light_linking_unlink, {"collection": link.prop.identifier})) for link in self.sets)
        return accumulated(map(linking, self.sets)).bind(lambda draws: _realized((*draws, *unlinks), identity))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _linked(
    emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty], members: tuple[_Member, ...], state: str, owner: bpy.types.Object, context: bpy.types.Context, identity: Identity
) -> Outcome[bpy.types.Collection]:
    """Links members into emitter set at state, objects through the set's stock link operator and collections through the set's entries.

    The set is the collection in the emitter's link slot, an owned one created for an empty slot.

    Args:
        emitter: Object with set receiving members.
        link: Receiver or blocker member of `ObjectLightLinking`.
        members: Objects and collections to link.
        state: `link_state` identifier.
        owner: Object an owned collection serves, emitter or rig target.
        context: Context the stock operator runs under with the emitter active and the objects selected.
        identity: Identity marking an owned collection.

    Returns:
        Set collection, or faults of the slot collection, the operator call, and the collection entry writes.
    """
    objects = tuple(member for member in members if isinstance(member, bpy.types.Object))
    collections = tuple(member for member in members if isinstance(member, bpy.types.Collection))

    def operated() -> Outcome[None]:
        override = Some(context.temp_override(active_object=emitter, selected_objects=list(objects)))
        return called(_LINKERS[link], {_LINK_STATE.prop.identifier: state}, override=override).map(lambda _: None) if objects else Ok(None)

    def entered(held: bpy.types.Collection) -> Outcome[None]:
        return _relinked(held, collections, linked=True).bind(
            lambda _: accumulated(_entry(held, member).bind(lambda found: _LINK_STATE.write(found.light_linking, state)) for member in collections).map(lambda _: None)
        )

    return (
        _struct_at(emitter.light_linking, link.path, bpy.types.Collection)
        .map(Ok)
        .default_with(
            lambda: _LightLinks.sets[link].made(
                identity, lambda: created(Nothing, f"{owner.name} {link.prop.name}"), of=Some(owner), filled=lambda held: link.write(emitter.light_linking, held).map(lambda _: held)
            )
        )
        .bind(lambda held: paired(operated(), entered(held)).map(lambda _: held))
    )


def _unlinked(layer: bpy.types.ViewLayer, emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty]) -> Outcome[Answer]:
    """Unlinks selected objects other than the emitter from its set, `Unchanged` when the set stays."""
    if not (selected := tuple(item for item in layer.objects.selected if item != emitter)):
        return Ok(Unchanged(reason="No object besides emitter is selected"))
    return (
        _struct_at(emitter.light_linking, link.path, bpy.types.Collection)
        .map(lambda collection: _relinked(collection, selected, linked=False).map(lambda changes: Done() if changes else Unchanged(reason=f"No selected object is in {collection.name}")))
        .default_value(Ok(Unchanged(reason=f"{emitter.name} holds no {link.prop.name}")))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Unlinking)
def light_linking_unlink(call: Call[Unlinking]) -> Outcome[Answer]:
    """Unlinks selected objects other than the active emitter from its light or shadow linking set."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: _active_object(layer).bind(lambda emitter: _unlinked(layer, emitter, _NAMED[call.properties.collection])))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Unlinking", "light_linking_unlink"]
