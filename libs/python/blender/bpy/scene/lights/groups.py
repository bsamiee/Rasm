"""Light group membership by rule, member selection, and lobe split children."""

from collections.abc import Mapping
from contextlib import ExitStack
from itertools import chain
from types import MappingProxyType
from typing import Annotated, ClassVar

import bpy
from expression import Ok, Option, Result, Some
from mathutils import Matrix
import msgspec

from blender.bpy.commands.operators import Done, Empty, Operator, Outcome, Unchanged
from blender.bpy.data.ids.ownership import committed, Ownership, released, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, root_in, Value, written
from blender.bpy.data.rna.snapshots import Scenes, Scope, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.scene.collections import joined
from blender.bpy.scene.lights.emitters import emitters, picked
from blender.bpy.scene.objects import focused, ObjectState, reconciled, spawned
from blender.bpy.ui.layout.rows import Member, OperatorButton, SearchField
from blender.bpy.ui.layout.sections import Section, Single
from blender.records.faults import accumulated
from blender.records.fields import Case, Choice, Gated

# --- [TYPES] ----------------------------------------------------------------------------

type Grouping = PerEmitter | Shared | Ungrouped


class Lobe(Choice):
    """Ray kind a split child reaches."""

    VISIBLE_DIFFUSE = 1, "Diffuse"
    VISIBLE_GLOSSY = 2, "Glossy"
    VISIBLE_TRANSMISSION = 3, "Transmission"
    VISIBLE_VOLUME_SCATTER = 4, "Volume"


# --- [MODELS] ---------------------------------------------------------------------------


class PerEmitter(Case, frozen=True, kw_only=True):
    """Group per holder, named after holder."""


class Shared(Case, frozen=True, kw_only=True):
    """Named group every holder of kind joins."""

    name: Annotated[str, msgspec.Meta(min_length=1, title="Group", description="Group every holder joins")] = "Emission"


class Ungrouped(Case, frozen=True, kw_only=True):
    """No group, holder reaching Combined alone."""

    title = "None"


class Membership(msgspec.Struct, frozen=True, kw_only=True):
    """Light group rule per emitter kind and world."""

    lights: Annotated[Gated[Grouping], msgspec.Meta(description="Group of each light object")] = Gated(enabled=True, value=PerEmitter())
    emissive: Annotated[Gated[Grouping], msgspec.Meta(description="Group of each emissive object")] = Gated(enabled=True, value=Shared())
    world: Annotated[Gated[Grouping], msgspec.Meta(description="World light group")] = Gated(enabled=True, value=PerEmitter())


class Assignment(msgspec.Struct, frozen=True, kw_only=True):
    """Membership rule with owners it writes to."""

    membership: Annotated[Membership, msgspec.Meta(description="Light group rule per kind")] = Membership()
    scope: Annotated[Scope, msgspec.Meta(description="Owners to group")] = Scenes()


class Split(msgspec.Struct, frozen=True, kw_only=True):
    """Ray kinds split light reaches through children."""

    lobes: Annotated[frozenset[Lobe], msgspec.Meta(description="Ray kinds with one split child each")] = frozenset(Lobe)


class LightGroupEdits(msgspec.Struct, frozen=True, kw_only=True):
    """Light group operators and section."""

    children: ClassVar[Mapping[Lobe, Ownership[bpy.types.Object]]] = MappingProxyType({lobe: Ownership(kind=bpy.types.Object, key=lobe.name.lower(), saved=Saved.USED) for lobe in Lobe})
    cycles: ClassVar[Option[Setting]] = Some(Setting(descriptor=Descriptor.declared(bpy.types.Context, "scene.render.engine", bpy.types.EnumProperty), accepted=("CYCLES",)))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns lobe child ownerships and section."""
        return Ok((*LightGroupEdits.children.values(), self.section()))

    def section(self) -> Section:
        """Returns active object light group field and operators."""
        active = Some(lambda context: root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)))
        return Single(
            key="light_groups",
            title="Light Groups",
            rows=(
                SearchField(target=Member(root=bpy.types.Object, path="lightgroup", reach=active), collection=Member(root=bpy.types.ViewLayer, path="lightgroups")),
                *(OperatorButton(target=command) for command in (light_groups_assign, light_group_select, light_split, light_split_toggle, light_unsplit)),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def assigned(membership: Membership, scope: Scope, context: bpy.types.Context) -> BlenderResult[Option[frozenset[str]]]:
    """Writes rule light groups to emitters and worlds in scope.

    Args:
        membership: Rule per kind, kinds with gate off unchanged.
        scope: Owners to group.
        context: Context `scope.owners` reads.

    Returns:
        Group names written, `Nothing` when every holder held its group, or `absent` and `edit` faults.
    """

    def named(grouping: Grouping, holder: bpy.types.Object | bpy.types.World) -> str:
        match grouping:
            case PerEmitter():
                return holder.name
            case Shared(name=name):
                return name
            case Ungrouped():
                return ""

    def wrote(grouping: Grouping, holder: bpy.types.Object | bpy.types.World) -> BlenderResult[Option[str]]:
        name = named(grouping, holder)
        return written(holder, "lightgroup", name).map(lambda prior: prior.map(lambda _: name))

    def worlds(reached: tuple[bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object, ...]) -> tuple[bpy.types.World, ...]:
        return tuple(dict.fromkeys(world for owner in reached if isinstance(scene := owner.id_data, bpy.types.Scene) for world in Option.of_optional(scene.world).to_list()))

    return scope.owners(context).bind(
        lambda reached: accumulated((
            *(
                wrote(rule.value, item)
                for item in dict.fromkeys(item for owner in reached for item in emitters(owner))
                for rule in (membership.lights if isinstance(item.data, bpy.types.Light) else membership.emissive,)
                if rule.enabled
            ),
            *(wrote(membership.world.value, world) for world in worlds(reached) if membership.world.enabled),
        )).map(lambda changes: Some(tuple(name for change in changes for name in change.to_list())).filter(bool).map(lambda names: frozenset(names) - {""}))
    )


def used(scene: bpy.types.Scene) -> frozenset[str]:
    """Returns light group names of scene objects and world."""
    return frozenset(name for name in (*(item.lightgroup for item in scene.objects), *Option.of_optional(scene.world).map(lambda world: world.lightgroup).to_list()) if name)


def members(item: bpy.types.Object, view_layer: bpy.types.ViewLayer) -> frozenset[bpy.types.Object]:
    """Returns view layer objects in light group of object, none for ungrouped object."""
    return frozenset(other for other in view_layer.objects if item.lightgroup and other.lightgroup == item.lightgroup)


def split(master: bpy.types.Object, light: bpy.types.Light, record: Split, view_layer: bpy.types.ViewLayer, identity: Identity) -> BlenderResult[tuple[bpy.types.Object, ...]]:
    """Creates one child per lobe sharing light data and hides master in renders.

    Args:
        master: Light object to split.
        light: Light data of master.
        record: Lobes to create children for.
        view_layer: View layer hiding split children.
        identity: Identity marking each child.

    Returns:
        Children in lobe order, or `absent` and `edit` faults.
    """
    group, holders = master.lightgroup or master.name, tuple(master.users_collection)
    stale = tuple(item for lobe, row in LightGroupEdits.children.items() if lobe not in record.lobes for item in row.serving(identity, master))

    def child(lobe: Lobe, links: Snapshot, home: bpy.types.Collection) -> BlenderResult[bpy.types.Object]:
        values: dict[str, Value] = {
            **{other.name.lower(): other is lobe for other in Lobe},
            "visible_camera": master.visible_camera and lobe is Lobe.VISIBLE_DIFFUSE,
            "visible_shadow": master.visible_shadow,
            "hide_render": False,
            "lightgroup": f"{group}_{lobe.label.lower()}",
        }
        state = ObjectState(parent=Some(Handle.of(master)), values=Some(Snapshot.of(bpy.types.Object, values)), placement=Some(Matrix.Identity(4)), hidden=Some(True))
        return (
            LightGroupEdits
            .children[lobe]
            .made(identity, lambda: spawned(f"{master.name} {lobe.label}", Some(light), home), of=Some(master), scope=Some(rollback))
            .bind(lambda item: accumulated(joined(collection, (item,)) for collection in holders).map(lambda _: item))
            .bind(lambda item: accumulated((reconciled(state, item, (view_layer,)).map(lambda _: None), links.apply(item.light_linking).outcome)).map(lambda _: item))
        )

    hidden = Snapshot.of(bpy.types.Object, {"hide_render": True})

    with ExitStack() as rollback:
        return committed(
            Result[bpy.types.Collection, BlenderFaults]
            .bind(
                Option.of_optional(next(iter(holders), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(master), key="users_collection"),)),
                lambda home: Snapshot.capture(master.light_linking).bind(lambda links: released(stale).bind(lambda _: accumulated(child(lobe, links, home) for lobe in Lobe if lobe in record.lobes))),
            )
            .bind(lambda children: reconciled(ObjectState(values=Some(hidden)), master, ()).map(lambda _: children)),
            rollback,
        )


def lights(view_layer: bpy.types.ViewLayer) -> dict[bpy.types.Object, bpy.types.Light]:
    """Returns selected objects with light data."""
    return {item: light for item in view_layer.objects.selected if isinstance(light := item.data, bpy.types.Light)}


def splits(view_layer: bpy.types.ViewLayer, identity: Identity) -> dict[bpy.types.Object, tuple[bpy.types.Object, ...]]:
    """Returns selected split lights in selection order with children."""
    return {master: held for master in lights(view_layer) if (held := tuple(item for row in LightGroupEdits.children.values() for item in row.serving(identity, master)))}


def lobed(view_layer: bpy.types.ViewLayer, record: Split, identity: Identity) -> BlenderResult[Outcome]:
    """Splits each selected light into lobe children.

    Args:
        view_layer: Layer with selected lights.
        record: Lobes to create children for.
        identity: Identity marking each child.

    Returns:
        `Done`, `Unchanged` when no light is selected, or split faults.
    """
    if not (selected := lights(view_layer)):
        return Ok(Unchanged(reason="No light selected"))
    return accumulated(split(master, light, record, view_layer, identity) for master, light in selected.items()).map(lambda _: Done())


def toggled(view_layer: bpy.types.ViewLayer, identity: Identity) -> BlenderResult[Outcome]:
    """Swaps visibility of selected split lights and children.

    Args:
        view_layer: Layer with selected lights.
        identity: Identity reading child marks.

    Returns:
        `Done`, `Unchanged` when no split light is selected, or `edit` faults.
    """
    if not (held := splits(view_layer, identity)):
        return Ok(Unchanged(reason="No split light selected"))
    return accumulated(reconciled(ObjectState(hidden=Some(not item.hide_get(view_layer=view_layer))), item, (view_layer,)) for master, children in held.items() for item in (master, *children)).map(
        lambda _: Done()
    )


def unsplit(view_layer: bpy.types.ViewLayer, identity: Identity) -> BlenderResult[Outcome]:
    """Removes split children of selected lights and renders lights again.

    Args:
        view_layer: Layer with selected lights.
        identity: Identity reading child marks.

    Returns:
        `Done` noting removed child count, `Unchanged` when no split light is selected, or `edit` faults.
    """
    if not (held := splits(view_layer, identity)):
        return Ok(Unchanged(reason="No split light selected"))
    shown = Snapshot.of(bpy.types.Object, {"hide_render": False})
    return released(tuple(chain.from_iterable(held.values()))).bind(
        lambda count: accumulated(reconciled(ObjectState(values=Some(shown)), master, ()) for master in held).map(lambda _: Done(notes=(("INFO", f"Removed {count} children"),)))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

light_groups_assign: Operator[Assignment] = Operator(
    key="light_groups_assign",
    title="Assign Light Groups",
    description="Write rule light groups to emitters and worlds in scope",
    form=Assignment,
    condition=LightGroupEdits.cycles,
    run=lambda call: assigned(call.record.membership, call.record.scope, call.context).map(
        lambda change: change.map(lambda names: Done(notes=(("INFO", f"Light groups {', '.join(sorted(names))}"),))).default_value(
            Unchanged(reason="Every emitter and world in scope holds its light group")
        )
    ),
)
light_group_select: Operator[Empty] = Operator(
    key="light_group_select",
    title="Select Group Members",
    description="Select objects in light group of active emitter",
    form=Empty,
    condition=LightGroupEdits.cycles,
    run=lambda call: call.root(bpy.types.ViewLayer).bind(lambda layer: focused(layer).bind(lambda item: picked(members(item, layer), layer))).map(lambda _: Done()),
)
light_split: Operator[Split] = Operator(
    key="light_split",
    title="Split Lobes",
    description="Split selected lights into one light group child per ray kind",
    form=Split,
    condition=LightGroupEdits.cycles,
    run=lambda call: call.root(bpy.types.ViewLayer).bind(lambda layer: lobed(layer, call.record, call.identity)),
)
light_split_toggle: Operator[Empty] = Operator(
    key="light_split_toggle",
    title="Toggle Split",
    description="Swap visibility of selected split lights and their children",
    form=Empty,
    condition=LightGroupEdits.cycles,
    run=lambda call: call.root(bpy.types.ViewLayer).bind(lambda layer: toggled(layer, call.identity)),
)
light_unsplit: Operator[Empty] = Operator(
    key="light_unsplit",
    title="Remove Split",
    description="Remove split children and render selected lights again",
    form=Empty,
    condition=LightGroupEdits.cycles,
    run=lambda call: call.root(bpy.types.ViewLayer).bind(lambda layer: unsplit(layer, call.identity)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Assignment",
    "Grouping",
    "LightGroupEdits",
    "Lobe",
    "Membership",
    "PerEmitter",
    "Shared",
    "Split",
    "Ungrouped",
    "assigned",
    "light_group_select",
    "light_groups_assign",
    "light_split",
    "light_split_toggle",
    "light_unsplit",
    "lights",
    "lobed",
    "members",
    "split",
    "splits",
    "toggled",
    "unsplit",
    "used",
]
