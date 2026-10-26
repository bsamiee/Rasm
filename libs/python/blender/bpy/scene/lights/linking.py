"""Light and shadow linking sets of emitters, set members, and member link states."""

from collections.abc import Mapping
from types import MappingProxyType
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor, root_in
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.scene.collections import entry, founded, joined, Member as Linkable, parted
from blender.bpy.scene.objects import focused
from blender.bpy.ui.layout import rows
from blender.bpy.ui.layout.rows import Member, OperatorButton
from blender.bpy.ui.layout.templates import LightLinking
from blender.records.faults import accumulated, Outcome, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

LINK_STATE: Final = Descriptor.declared(bpy.types.CollectionLightLinking, "link_state", bpy.types.EnumProperty)
_SETS: Final = MappingProxyType({
    Descriptor.declared(bpy.types.ObjectLightLinking, prop.identifier, bpy.types.PointerProperty): Ownership(kind=bpy.types.Collection, key=prop.identifier, saved=Saved.USED)
    for prop in bpy.types.ObjectLightLinking.bl_rna.properties
    if isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.Collection)
})
RECEIVERS: Final = Descriptor.declared(bpy.types.ObjectLightLinking, "receiver_collection", bpy.types.PointerProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Linking(bpy.types.PropertyGroup):
    """Link state of each joining member."""

    state: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name=LINK_STATE.prop.name,
        description="Include or exclude each member",
        items=tuple((item.identifier, item.name, item.description) for item in LINK_STATE.prop.enum_items),
        default=LINK_STATE.prop.default,
    )


class LightLinks(msgspec.Struct, frozen=True, kw_only=True):
    """Receiver and blocker sets with link and unlink operators."""

    sets: Mapping[Descriptor[bpy.types.PointerProperty], Ownership[bpy.types.Collection]] = _SETS

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns set ownerships and link and unlink operators in RNA order."""
        return Ok((*self.sets.values(), *(command for link in self.sets for command in commands[link])))

    def drawn(self, identity: Identity, /) -> Outcome[rows.RowDraw]:
        """Returns linking templates with link and unlink buttons."""
        reach = Some(lambda context: root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)).map(lambda item: item.light_linking))
        return accumulated(
            paired(
                LightLinking(target=Member(root=bpy.types.ObjectLightLinking, path=descriptor.prop.identifier, reach=reach)).drawn(identity),
                rows.placed((OperatorButton(target=link), OperatorButton(target=unlink)), identity),
            ).map(lambda pair: rows.joined((pair[0], *pair[1])))
            for descriptor in self.sets
            for link, unlink in (commands[descriptor],)
        ).map(rows.joined)


# --- [OPERATIONS] -----------------------------------------------------------------------


def edited[T: bpy.types.PropertyGroup](link: Descriptor[bpy.types.PointerProperty], state: Option[str], call: Call[T]) -> Outcome[Answer]:
    """Links selected objects other than the active emitter into its set at a state, else unlinks them, `Unchanged` when set stays."""

    def applied(layer: bpy.types.ViewLayer, emitter: bpy.types.Object) -> Outcome[Answer]:
        members = tuple(item for item in layer.objects.selected if item != emitter)
        held = Option.of_optional(getattr(emitter.light_linking, link.prop.identifier))
        match state:
            case Option(tag="some", some=chosen) if members or held.is_none():
                return linked(emitter, link, members, chosen, emitter, call.identity).map(lambda made: Done(notes=(("INFO", f"Linked into {made.name}"),)))
            case _ if not members:
                return Ok(Unchanged(reason="No object besides emitter is selected"))
            case _:
                return held.map(
                    lambda collection: parted(collection, members).map(lambda changes: Done() if changes else Unchanged(reason=f"No selected object is in {collection.name}"))
                ).default_value(Ok(Unchanged(reason=f"{emitter.name} holds no {link.prop.name}")))

    return call.root(bpy.types.ViewLayer).bind(lambda layer: focused(layer).bind(lambda emitter: applied(layer, emitter)))


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
        Option
        .of_optional(getattr(emitter.light_linking, link.prop.identifier))
        .map(Ok)
        .default_with(
            lambda: _SETS[link].made(
                identity, lambda: founded(Nothing, f"{owner.name} {link.prop.name}"), of=Some(owner), filled=lambda held: link.write(emitter.light_linking, held).map(lambda _: held)
            )
        )
    )


def linked(
    emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty], members: tuple[Linkable, ...], state: str, owner: bpy.types.Object, identity: Identity
) -> Outcome[bpy.types.Collection]:
    """Links members into emitter set at state.

    Args:
        emitter: Object with set receiving members.
        link: Receiver or blocker member of `ObjectLightLinking`.
        members: Objects and collections to link.
        state: Validated `link_state` identifier.
        owner: Object an owned collection serves.
        identity: Identity marking an owned collection.

    Returns:
        Set collection, or faults of `link_set`, `joined`, and state writes.
    """
    return (
        link_set(emitter, link, owner, identity)
        .bind(lambda held: joined(held, members).map(lambda _: held))
        .bind(lambda held: accumulated(entry(held, member).bind(lambda found: LINK_STATE.write(found.light_linking, state)) for member in members).map(lambda _: held))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def link_commands(link: Descriptor[bpy.types.PointerProperty]) -> tuple[Operator[Linking], Operator[Empty]]:
    """Returns the link and unlink operators of one set, each body named `<verb>_<set member>`."""

    def linked_into(call: Call[Linking]) -> Outcome[Answer]:
        """Links selected objects other than the active emitter into its set at the chosen state."""
        return edited(link, Some(call.properties.state), call)

    def unlinked_from(call: Call[Empty]) -> Outcome[Answer]:
        """Unlinks selected objects other than the active emitter from its set."""
        return edited(link, Nothing, call)

    linked_into.__name__ = linked_into.__qualname__ = f"link_{link.prop.identifier}"
    unlinked_from.__name__ = unlinked_from.__qualname__ = f"unlink_{link.prop.identifier}"
    return Operator(form=Linking, run=linked_into), Operator(form=Empty, run=unlinked_from)


commands: Final = MappingProxyType({link: link_commands(link) for link in _SETS})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LINK_STATE", "RECEIVERS", "LightLinks", "Linking", "commands", "edited", "link_commands", "link_set", "linked"]
