"""Light and shadow linking sets of an emitter, their members, and each member's link state."""

from contextlib import ExitStack
from functools import partial
from itertools import product
from typing import Annotated, ClassVar

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from rasm.blender.commands.operators import Call, Done, Empty, Operator
from rasm.blender.data.ids.ownership import earliest, Mark, Ownership, Saved
from rasm.blender.data.rna.descriptors import Descriptor, root_in, written
from rasm.blender.extension.identity import Identity
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderResult
from rasm.blender.scene.collections import entry, founded, joined, parted
from rasm.blender.scene.lights.emitters import focused
from rasm.blender.settings.properties import Choices
from rasm.blender.ui.layout import rows
from rasm.blender.ui.layout.rows import Member, OperatorButton
from rasm.blender.ui.layout.templates import templated
from rasm.imaging.faults import accumulated, paired

# --- [CHOICES] --------------------------------------------------------------------------

link_state: bpy.types.EnumProperty = bpy.types.CollectionLightLinking.bl_rna.properties["link_state"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
states = Choices(lambda identity, owner, context: ((item.identifier, item.name, item.description) for item in link_state.enum_items))


# --- [MODELS] ---------------------------------------------------------------------------


class Linking(msgspec.Struct, frozen=True, kw_only=True):
    """Link state a link operator writes on each joining member."""

    state: Annotated[str, msgspec.Meta(title="State", description="Whether each member is lit and shadowed by the emitter or left out"), states] = link_state.default


class LightLinks(msgspec.Struct, frozen=True, kw_only=True):
    """Row of the receiver and blocker sets with their link and unlink operators."""

    sets: ClassVar[dict[Descriptor[bpy.types.PointerProperty], Ownership[bpy.types.Collection]]] = {
        Descriptor(root=bpy.types.ObjectLightLinking, owner="", prop=prop): Ownership(kind=bpy.types.Collection, key=prop.identifier, saved=Saved.USED)  # type: ignore[arg-type]
        for prop in bpy.types.ObjectLightLinking.bl_rna.properties
        if isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.Collection)
    }

    @staticmethod
    def members(layer: bpy.types.ViewLayer, emitter: bpy.types.Object) -> tuple[bpy.types.Object, ...]:
        """Returns selected receivers excluding the emitter."""
        return tuple(item for item in layer.objects.selected if item != emitter)

    @staticmethod
    def edited[T: Linking | Empty](link: Descriptor[bpy.types.PointerProperty], call: Call[T]) -> BlenderResult[Done]:
        """Applies a link or unlink command to the active emitter and selected receivers."""
        return call.root(bpy.types.ViewLayer).bind(
            lambda layer: focused(layer).bind(
                lambda emitter: (
                    linked(emitter, link, LightLinks.members(layer, emitter), call.record.state, emitter, call.identity).map(lambda held: Done(notes=(("INFO", f"Linked into {held.name}"),)))
                    if isinstance(call.record, Linking)
                    else unlinked(emitter, link, LightLinks.members(layer, emitter)).map(lambda _: Done())
                )
            )
        )

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
    emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty], members: tuple[bpy.types.Object | bpy.types.Collection, ...], state: str, owner: bpy.types.Object, identity: Identity
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


def unlinked(emitter: bpy.types.Object, link: Descriptor[bpy.types.PointerProperty], members: tuple[bpy.types.Object | bpy.types.Collection, ...]) -> BlenderResult[None]:
    """Parts members from an emitter's link set.

    Args:
        emitter: Object whose set loses the members.
        link: Receiver or blocker member of `ObjectLightLinking`.
        members: Objects and collections leaving.

    Returns:
        `Ok` once parted, also for an empty slot; `edit` per refused unlink.
    """
    return Option.of_optional(getattr(emitter.light_linking, link.prop.identifier)).map(lambda held: parted(held, members).map(lambda _: None)).default_value(Ok(None))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LightLinks", "Linking", "link_set", "link_state", "linked", "states", "unlinked"]
