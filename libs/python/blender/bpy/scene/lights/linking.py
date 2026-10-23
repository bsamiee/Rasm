"""Light and shadow linking sets of emitters, set members, and member link states."""

from functools import partial
from itertools import product
from typing import Annotated, ClassVar

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.operators import Call, Done, Empty, Operator, Outcome, Unchanged
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor, root_in, written
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderResult
from blender.bpy.scene.collections import entry, founded, joined, Member as Linkable, parted
from blender.bpy.scene.lights.emitters import focused
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout import rows
from blender.bpy.ui.layout.rows import Member, OperatorButton
from blender.bpy.ui.layout.templates import templated
from blender.bpy.ui.surfaces.panels import enumerated
from blender.imaging.faults import accumulated, paired

# --- [CHOICES] --------------------------------------------------------------------------

link_state = next(prop for prop in bpy.types.CollectionLightLinking.bl_rna.properties if isinstance(prop, bpy.types.EnumProperty) and prop.identifier == "link_state")
states = Choices(lambda *_: enumerated(link_state))


# --- [MODELS] ---------------------------------------------------------------------------


class Linking(msgspec.Struct, frozen=True, kw_only=True):
    """Link state of each joining member."""

    state: Annotated[str, msgspec.Meta(description="Include or exclude each member"), states] = link_state.default


class LightLinks(msgspec.Struct, frozen=True, kw_only=True):
    """Receiver and blocker sets with link and unlink operators."""

    sets: ClassVar[dict[Descriptor[bpy.types.PointerProperty], Ownership[bpy.types.Collection]]] = {
        Descriptor(root=bpy.types.ObjectLightLinking, owner="", prop=prop): Ownership(kind=bpy.types.Collection, key=prop.identifier, saved=Saved.USED)
        for prop in bpy.types.ObjectLightLinking.bl_rna.properties
        if isinstance(prop, bpy.types.PointerProperty)
        if isinstance(prop.fixed_type, bpy.types.Collection)
    }

    @staticmethod
    def members(layer: bpy.types.ViewLayer, emitter: bpy.types.Object) -> tuple[bpy.types.Object, ...]:
        """Returns selected objects except emitter."""
        return tuple(item for item in layer.objects.selected if item != emitter)

    @staticmethod
    def edited[T: Linking | Empty](link: Descriptor[bpy.types.PointerProperty], call: Call[T]) -> BlenderResult[Outcome]:
        """Links or unlinks selected objects in active emitter set, `Unchanged` when set stays."""

        def applied(layer: bpy.types.ViewLayer, emitter: bpy.types.Object) -> BlenderResult[Outcome]:
            members = LightLinks.members(layer, emitter)
            held = Option.of_optional(getattr(emitter.light_linking, link.prop.identifier))
            match call.record:
                case Linking(state=state) if members or held.is_none():
                    return linked(emitter, link, members, state, emitter, call.identity).map(lambda made: Done(notes=(("INFO", f"Linked into {made.name}"),)))
                case _ if not members:
                    return Ok(Unchanged(reason="No object besides emitter is selected"))
                case _:
                    return held.map(
                        lambda collection: parted(collection, members).map(lambda changes: Done() if changes else Unchanged(reason=f"No selected object is in {collection.name}"))
                    ).default_value(Ok(Unchanged(reason=f"{emitter.name} holds no {link.prop.name}")))

        return call.root(bpy.types.ViewLayer).bind(lambda layer: focused(layer).bind(lambda emitter: applied(layer, emitter)))

    commands: ClassVar[tuple[tuple[Operator[Linking], Operator[Empty]], ...]] = tuple(
        (
            Operator(key=f"link_{slot}", title=f"Link to {link.prop.name}", description=f"Link selected objects into {link.prop.name} of active emitter", form=Linking, run=partial(edit, link)),
            Operator(key=f"unlink_{slot}", title=f"Unlink from {link.prop.name}", description=f"Unlink selected objects from {link.prop.name} of active emitter", form=Empty, run=partial(edit, link)),
        )
        for link, edit in product(sets, (edited,))
        for slot in (link.prop.identifier,)
    )

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns set ownerships and link and unlink operators in RNA order."""
        return Ok((*LightLinks.sets.values(), *(command for pair in self.commands for command in pair)))

    def drawn(self, identity: Identity, /) -> BlenderResult[rows.RowDraw]:
        """Returns linking templates with link and unlink buttons."""
        reach = Some(lambda context: root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)).map(lambda item: item.light_linking))
        return accumulated(
            paired(
                templated(
                    Member(root=bpy.types.ObjectLightLinking, path=descriptor.prop.identifier, reach=reach),
                    (bpy.types.Collection,),
                    (),
                    lambda layout, owner, key, _context: layout.template_light_linking_collection(layout, owner, key),
                ),
                rows.placed((OperatorButton(target=link), OperatorButton(target=unlink)), identity),
            ).map(lambda pair: rows.joined((pair[0], *pair[1])))
            for descriptor, (link, unlink) in zip(LightLinks.sets, self.commands)
        ).map(rows.joined)


# --- [OPERATIONS] -----------------------------------------------------------------------


def link_set(emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty], owner: bpy.types.Object, identity: Identity) -> BlenderResult[bpy.types.Collection]:
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
            lambda: LightLinks.sets[link].made(
                identity, lambda: founded(Nothing, f"{owner.name} {link.prop.name}"), of=Some(owner), filled=lambda held: link.write(emitter.light_linking, held).map(lambda _: held)
            )
        )
    )


def linked(
    emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty], members: tuple[Linkable, ...], state: str, owner: bpy.types.Object, identity: Identity
) -> BlenderResult[bpy.types.Collection]:
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
        .bind(lambda held: accumulated(entry(held, member).bind(lambda found: written(found.light_linking, "link_state", state)) for member in members).map(lambda _: held))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LightLinks", "Linking", "link_set", "link_state", "linked", "states"]
