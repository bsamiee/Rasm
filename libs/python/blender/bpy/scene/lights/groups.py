"""Light group membership by rule, member selection, and lobe split children."""

from collections.abc import Mapping
from contextlib import ExitStack
from enum import auto, StrEnum
from itertools import chain
from types import MappingProxyType
from typing import Annotated, ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from mathutils import Matrix
import msgspec

from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.ownership import committed, Ownership, released, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, root_in, Value, written
from blender.bpy.data.rna.snapshots import Scenes, Scope, scoped, SCOPES, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.scene.collections import joined
from blender.bpy.scene.lights.emitters import emitters, picked
from blender.bpy.scene.objects import focused, ObjectState, reconciled, spawned
from blender.bpy.ui.layout.rows import Member, OperatorButton, SearchField
from blender.bpy.ui.layout.sections import Section, Single
from blender.records.faults import accumulated, Fault, Faults, Outcome
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Grouping = PerEmitter | Shared | Ungrouped


class Lobe(StrEnum):
    """Ray kind a split child reaches, valued by the `Object` visibility member it holds."""

    VISIBLE_DIFFUSE = auto()
    VISIBLE_GLOSSY = auto()
    VISIBLE_TRANSMISSION = auto()
    VISIBLE_VOLUME_SCATTER = auto()

    @property
    def label(self) -> str:
        """Name of the visibility member, naming split children and their light groups."""
        return bpy.types.Object.bl_rna.properties[self.value].name


# --- [CONSTANTS] ------------------------------------------------------------------------

GROUPINGS: Final = (
    ("KEEP", "Keep", "Holders keep their groups"),
    ("PER_EMITTER", "Per Emitter", "Group per holder, named after holder"),
    ("SHARED", "Shared", "Named group every holder of kind joins"),
    ("NONE", "None", "No group, holder reaching Combined alone"),
)

# --- [MODELS] ---------------------------------------------------------------------------


class PerEmitter(Case, frozen=True, kw_only=True):
    """Group per holder, named after holder."""


class Shared(Case, frozen=True, kw_only=True):
    """Named group every holder of kind joins."""

    name: Annotated[str, msgspec.Meta(min_length=1)] = "Emission"


class Ungrouped(Case, frozen=True, kw_only=True):
    """No group, holder reaching Combined alone."""


class Membership(msgspec.Struct, frozen=True, kw_only=True):
    """Light group rule per emitter kind and world, `Nothing` leaving that kind's groups."""

    lights: Option[Grouping] = Some(PerEmitter())
    emissive: Option[Grouping] = Some(Shared())
    world: Option[Grouping] = Some(PerEmitter())


class Assignment(bpy.types.PropertyGroup):
    """Light group rule per emitter kind and world with the owners it writes to."""

    lights: bpy.props.EnumProperty(name="Lights", description="Group of each light object", items=GROUPINGS, default="PER_EMITTER")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    emissive: bpy.props.EnumProperty(name="Emissive", description="Group of each emissive object", items=GROUPINGS, default="SHARED")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    world: bpy.props.EnumProperty(name="World", description="World light group", items=GROUPINGS, default="PER_EMITTER")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    shared: bpy.props.StringProperty(name="Shared Group", description="Group every holder of a shared rule joins", default="Emission")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scope: bpy.props.EnumProperty(name="Scope", description="Owners to group", items=SCOPES, default=Scenes.__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    collection: bpy.props.StringProperty(name="Collection", description="Collection whose objects a collection scope groups")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Split(bpy.types.PropertyGroup):
    """Ray kinds split light reaches through children."""

    lobes: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Lobes",
        description="Ray kinds with one split child each",
        items=tuple((lobe.value, lobe.label, bpy.types.Object.bl_rna.properties[lobe.value].description) for lobe in Lobe),
        options={"ENUM_FLAG"},
        default={lobe.value for lobe in Lobe},
    )


class LightGroupEdits(msgspec.Struct, frozen=True, kw_only=True):
    """Light group operators and section."""

    children: ClassVar[Mapping[Lobe, Ownership[bpy.types.Object]]] = MappingProxyType({lobe: Ownership(kind=bpy.types.Object, key=lobe.name.lower(), saved=Saved.USED) for lobe in Lobe})
    cycles: ClassVar[Option[Setting]] = Some(Setting(descriptor=Descriptor.declared(bpy.types.Context, "scene.render.engine", bpy.types.EnumProperty), accepted=("CYCLES",)))

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns lobe child ownerships and section."""
        return Ok((*LightGroupEdits.children.values(), self.section()))

    @staticmethod
    def section() -> Section:
        """Returns active object light group field and operators."""
        active = Some(lambda context: root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)))
        return Single(
            key="light_groups",
            title="Light Groups",
            rows=(
                SearchField(target=Member(root=bpy.types.Object, path="lightgroup", reach=active), collection=Member(root=bpy.types.ViewLayer, path="lightgroups")),
                *(OperatorButton(target=command) for command in (light_groups_assign_command, light_group_select_command, light_split_command, light_split_toggle_command, light_unsplit_command)),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def grouping(choice: str, shared: str) -> Option[Grouping]:
    """Returns the rule a `GROUPINGS` choice names, `Nothing` to keep groups, a shared rule under its group name."""
    match choice:
        case "PER_EMITTER":
            return Some(PerEmitter())
        case "SHARED":
            return Some(Shared(name=shared))
        case "NONE":
            return Some(Ungrouped())
        case _:
            return Nothing


def assigned(membership: Membership, scope: Scope, context: bpy.types.Context) -> Outcome[Option[frozenset[str]]]:
    """Writes rule light groups to emitters and worlds in scope.

    Args:
        membership: Rule per kind, kinds without one unchanged.
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

    def wrote(grouping: Grouping, holder: bpy.types.Object | bpy.types.World) -> Outcome[Option[str]]:
        name = named(grouping, holder)
        return written(holder, "lightgroup", name).map(lambda prior: prior.map(lambda _: name))

    def worlds(reached: tuple[bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object, ...]) -> tuple[bpy.types.World, ...]:
        return tuple(dict.fromkeys(world for owner in reached if isinstance(scene := owner.id_data, bpy.types.Scene) for world in Option.of_optional(scene.world).to_list()))

    return scope.owners(context).bind(
        lambda reached: accumulated((
            *(
                wrote(rule, item)
                for item in dict.fromkeys(item for owner in reached for item in emitters(owner))
                for rule in (membership.lights if isinstance(item.data, bpy.types.Light) else membership.emissive).to_list()
            ),
            *(wrote(rule, world) for world in worlds(reached) for rule in membership.world.to_list()),
        )).map(lambda changes: Some(tuple(name for change in changes for name in change.to_list())).filter(bool).map(lambda names: frozenset(names) - {""}))
    )


def used(scene: bpy.types.Scene) -> frozenset[str]:
    """Returns light group names of scene objects and world."""
    return frozenset(name for name in (*(item.lightgroup for item in scene.objects), *Option.of_optional(scene.world).map(lambda world: world.lightgroup).to_list()) if name)


def members(item: bpy.types.Object, view_layer: bpy.types.ViewLayer) -> frozenset[bpy.types.Object]:
    """Returns view layer objects in light group of object, none for ungrouped object."""
    return frozenset(other for other in view_layer.objects if item.lightgroup and other.lightgroup == item.lightgroup)


def split(master: bpy.types.Object, light: bpy.types.Light, lobes: frozenset[Lobe], view_layer: bpy.types.ViewLayer, identity: Identity) -> Outcome[tuple[bpy.types.Object, ...]]:
    """Creates one child per lobe sharing light data and hides master in renders.

    Args:
        master: Light object to split.
        light: Light data of master.
        lobes: Lobes to create children for.
        view_layer: View layer hiding split children.
        identity: Identity marking each child.

    Returns:
        Children in lobe order, or `absent` and `edit` faults.
    """
    group, holders = master.lightgroup or master.name, tuple(master.users_collection)
    stale = tuple(item for lobe, row in LightGroupEdits.children.items() if lobe not in lobes for item in row.serving(identity, master))

    def child(lobe: Lobe, links: Snapshot, home: bpy.types.Collection) -> Outcome[bpy.types.Object]:
        values: dict[str, Value] = {
            **{other.value: other is lobe for other in Lobe},
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
            Result[bpy.types.Collection, Faults]
            .bind(
                Option.of_optional(next(iter(holders), None)).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(master), member="users_collection"),)),
                lambda home: Snapshot.capture(master.light_linking).bind(lambda links: released(stale).bind(lambda _: accumulated(child(lobe, links, home) for lobe in Lobe if lobe in lobes))),
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


def lobed(view_layer: bpy.types.ViewLayer, lobes: frozenset[Lobe], identity: Identity) -> Outcome[Answer]:
    """Splits each selected light into lobe children.

    Args:
        view_layer: Layer with selected lights.
        lobes: Lobes to create children for.
        identity: Identity marking each child.

    Returns:
        `Done`, `Unchanged` when no light is selected, or split faults.
    """
    if not (selected := lights(view_layer)):
        return Ok(Unchanged(reason="No light selected"))
    return accumulated(split(master, light, lobes, view_layer, identity) for master, light in selected.items()).map(lambda _: Done())


def toggled(view_layer: bpy.types.ViewLayer, identity: Identity) -> Outcome[Answer]:
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


def unsplit(view_layer: bpy.types.ViewLayer, identity: Identity) -> Outcome[Answer]:
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


def light_groups_assign(call: Call[Assignment]) -> Outcome[Answer]:
    """Writes rule light groups to the emitters and worlds in scope."""
    shared = call.properties.shared
    return assigned(
        Membership(lights=grouping(call.properties.lights, shared), emissive=grouping(call.properties.emissive, shared), world=grouping(call.properties.world, shared)),
        scoped(call.properties.scope, call.properties.collection),
        call.context,
    ).map(
        lambda change: change.map(lambda names: Done(notes=(("INFO", f"Light groups {', '.join(sorted(names))}"),))).default_value(
            Unchanged(reason="Every emitter and world in scope holds its light group")
        )
    )


def light_group_select(call: Call[Empty]) -> Outcome[Answer]:
    """Selects the objects in the active emitter's light group."""
    return call.root(bpy.types.ViewLayer).bind(lambda layer: focused(layer).bind(lambda item: picked(members(item, layer), layer))).map(lambda _: Done())


def light_split(call: Call[Split]) -> Outcome[Answer]:
    """Splits selected lights into one light group child per ray kind."""
    return call.root(bpy.types.ViewLayer).bind(lambda layer: lobed(layer, frozenset(map(Lobe, call.properties.lobes)), call.identity))


def light_split_toggle(call: Call[Empty]) -> Outcome[Answer]:
    """Swaps visibility of selected split lights and their children."""
    return call.root(bpy.types.ViewLayer).bind(lambda layer: toggled(layer, call.identity))


def light_unsplit(call: Call[Empty]) -> Outcome[Answer]:
    """Removes split children and renders selected lights again."""
    return call.root(bpy.types.ViewLayer).bind(lambda layer: unsplit(layer, call.identity))


light_groups_assign_command: Operator[Assignment] = Operator(form=Assignment, condition=LightGroupEdits.cycles, run=light_groups_assign)
light_group_select_command: Operator[Empty] = Operator(form=Empty, condition=LightGroupEdits.cycles, run=light_group_select)
light_split_command: Operator[Split] = Operator(form=Split, condition=LightGroupEdits.cycles, run=light_split)
light_split_toggle_command: Operator[Empty] = Operator(form=Empty, condition=LightGroupEdits.cycles, run=light_split_toggle)
light_unsplit_command: Operator[Empty] = Operator(form=Empty, condition=LightGroupEdits.cycles, run=light_unsplit)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "GROUPINGS",
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
    "grouping",
    "light_group_select_command",
    "light_groups_assign_command",
    "light_split_command",
    "light_split_toggle_command",
    "light_unsplit_command",
    "lights",
    "lobed",
    "members",
    "split",
    "splits",
    "toggled",
    "unsplit",
    "used",
]
