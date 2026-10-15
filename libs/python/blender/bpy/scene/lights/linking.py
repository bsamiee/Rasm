"""Light and shadow linking sets of an emitter, their members, and each member's link state."""

from contextlib import ExitStack
from functools import partial
from itertools import product
from typing import Annotated, ClassVar

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.operators import Call, Done, Empty, Operator, Outcome, Unchanged
from blender.bpy.data.ids.ownership import earliest, Mark, Ownership, Saved
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
    """Link state a link operator writes on each joining member."""

    state: Annotated[str, msgspec.Meta(title="State", description="Whether each member is lit and shadowed by the emitter or left out"), states] = link_state.default


class LightLinks(msgspec.Struct, frozen=True, kw_only=True):
    """Row of the receiver and blocker sets with their link and unlink operators."""

    sets: ClassVar[dict[Descriptor[bpy.types.PointerProperty], Ownership[bpy.types.Collection]]] = {
        Descriptor(root=bpy.types.ObjectLightLinking, owner="", prop=prop): Ownership(kind=bpy.types.Collection, key=prop.identifier, saved=Saved.USED)
        for prop in bpy.types.ObjectLightLinking.bl_rna.properties
        if isinstance(prop, bpy.types.PointerProperty)
        if isinstance(prop.fixed_type, bpy.types.Collection)
    }

    @staticmethod
    def members(layer: bpy.types.ViewLayer, emitter: bpy.types.Object) -> tuple[bpy.types.Object, ...]:
        """Returns selected receivers excluding the emitter."""
        return tuple(item for item in layer.objects.selected if item != emitter)

    @staticmethod
    def edited[T: Linking | Empty](link: Descriptor[bpy.types.PointerProperty], call: Call[T]) -> BlenderResult[Outcome]:
        """Applies a link or unlink command to the active emitter and selected receivers, `Unchanged` where it would leave the slot as it is."""

        def applied(layer: bpy.types.ViewLayer, emitter: bpy.types.Object) -> BlenderResult[Outcome]:
            members = LightLinks.members(layer, emitter)
            held = Option.of_optional(getattr(emitter.light_linking, link.prop.identifier))
            match call.record:
                case Linking(state=state) if members or held.is_none():
                    return linked(emitter, link, members, state, emitter, call.identity).map(lambda made: Done(notes=(("INFO", f"Linked into {made.name}"),)))
                case _ if not members:
                    return Ok(Unchanged(reason="No object besides the emitter is selected"))
                case _:
                    return held.map(
                        lambda collection: parted(collection, members).map(lambda changes: Done() if changes else Unchanged(reason=f"No selected object is in {collection.name}"))
                    ).default_value(Ok(Unchanged(reason=f"{emitter.name} holds no {link.prop.name}")))

        return call.root(bpy.types.ViewLayer).bind(lambda layer: focused(layer).bind(lambda emitter: applied(layer, emitter)))

    commands: ClassVar[tuple[tuple[Operator[Linking], Operator[Empty]], ...]] = tuple(
        (
            Operator(key=f"link_{slot}", title=f"Link to {link.prop.name}", description=f"Join the selected objects to the active emitter's {link.prop.name}", form=Linking, run=partial(edit, link)),
            Operator(
                key=f"unlink_{slot}", title=f"Unlink from {link.prop.name}", description=f"Remove the selected objects from the active emitter's {link.prop.name}", form=Empty, run=partial(edit, link)
            ),
        )
        for link, edit in product(sets, (edited,))
        for slot in (link.prop.identifier,)
    )

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Retains each set's ownership and operators for central acquisition.

        Args:
            _identity: Extension placing the row.

        Returns:
            `Ok` of the ownership and command declarations in native order.
        """
        return Ok((*LightLinks.sets.values(), *(command for pair in self.commands for command in pair)))

    def drawn(self, identity: Identity, /) -> BlenderResult[rows.RowDraw]:
        """Acquires native linking templates and their link and unlink buttons."""
        reach = Some(lambda context: root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)).map(lambda item: item.light_linking))
        return accumulated(
            paired(
                templated(
                    Member(root=bpy.types.ObjectLightLinking, path=descriptor.prop.identifier, reach=reach),
                    (bpy.types.Collection,),
                    (bpy.types.Collection,),
                    lambda layout, owner, key, _context: layout.template_light_linking_collection(layout, owner, key),
                ),
                rows.placed((OperatorButton(target=link), OperatorButton(target=unlink)), identity),
            ).map(lambda pair: rows.joined((pair[0], *pair[1])))
            for descriptor, (link, unlink) in zip(LightLinks.sets, self.commands)
        ).map(rows.joined)


# --- [OPERATIONS] -----------------------------------------------------------------------


def link_set(emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty], owner: bpy.types.Object, identity: Identity) -> BlenderResult[bpy.types.Collection]:
    """Answers the collection an emitter's link slot holds, making an owned one for an empty slot.

    Args:
        emitter: Object whose slot is read.
        link: Receiver or blocker member of `ObjectLightLinking`.
        owner: Object the owned collection serves, the emitter or a rig's target.
        identity: Identity marking the owned collection.

    Returns:
        `Ok` of the slot's collection; `collision` for a taken name and `edit` for a refused slot write.
    """
    row = LightLinks.sets[link]
    mark = Mark(key=row.key, of=Some(owner))

    def retained(held: bpy.types.Collection) -> bpy.types.Collection:
        rollback.callback(bpy.data.batch_remove, ids=(held,))
        return held

    def published(held: bpy.types.Collection) -> bpy.types.Collection:
        rollback.pop_all()
        return held

    with ExitStack() as rollback:
        return (
            Option
            .of_optional(getattr(emitter.light_linking, link.prop.identifier))
            .map(Ok)
            .default_with(
                lambda: (
                    Option
                    .of_optional(earliest(row.held(identity)).get(mark))
                    .map(Ok)
                    .default_with(lambda: founded(Nothing, f"{owner.name} {link.prop.name}").bind(lambda held: row.owned(identity, held, mark)).map(retained))
                    .bind(lambda held: link.write(emitter.light_linking, held).map(lambda _: held))
                )
            )
            .map(published)
        )


def linked(
    emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty], members: tuple[Linkable, ...], state: str, owner: bpy.types.Object, identity: Identity
) -> BlenderResult[bpy.types.Collection]:
    """Joins members into an emitter's link set at one state.

    Args:
        emitter: Object whose set takes the members.
        link: Receiver or blocker member of `ObjectLightLinking`.
        members: Objects and collections joining.
        state: `link_state` identifier the operator admitted.
        owner: Object an owned collection serves.
        identity: Identity marking an owned collection.

    Returns:
        `Ok` of the set's collection; the faults of `link_set`, `joined`, `entry`, and each state write.
    """
    return (
        link_set(emitter, link, owner, identity)
        .bind(lambda held: joined(held, members).map(lambda _: held))
        .bind(lambda held: accumulated(entry(held, member).bind(lambda found: written(found.light_linking, "link_state", state)) for member in members).map(lambda _: held))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LightLinks", "Linking", "link_set", "link_state", "linked", "states"]
