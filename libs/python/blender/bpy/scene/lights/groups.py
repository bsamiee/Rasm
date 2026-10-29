"""Light group membership by rule, member selection, and lobe split children."""

from contextlib import ExitStack
from enum import auto, StrEnum
from itertools import chain
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
from mathutils import Matrix
import msgspec

from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.ownership import Ownership, released, retained, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import chosen, Scenes, Scope, scope_choice, SCOPE_COLLECTION, Setting
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.mirrors import case_choices
from blender.bpy.data.rna.paths import RnaPath, root_in, rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.scene.collections import relinked
from blender.bpy.scene.lights.emitters import emitters, picked
from blender.bpy.scene.objects import focused, ObjectState, reconciled, spawned
from blender.bpy.ui.layout.rows import Member, OperatorButton, Property
from blender.bpy.ui.layout.sections import Section
from blender.records.faults import accumulated, Fault, Faults, Outcome, Reason
from blender.records.fields import Case, Name

# --- [TYPES] ----------------------------------------------------------------------------

type Grouping = Kept | PerEmitter | Shared | Ungrouped


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

    @property
    def slot(self) -> int:
        """Position among lobes, telling apart the split children of one light."""
        return tuple(Lobe).index(self)


# --- [MODELS] ---------------------------------------------------------------------------


class Kept(Case, frozen=True, kw_only=True):
    """Holders keep their groups."""

    @staticmethod
    def named(_holder: bpy.types.Object | bpy.types.World) -> Option[str]:
        """Returns no group name, the holder's group left as it is."""
        return Nothing


class PerEmitter(Case, frozen=True, kw_only=True):
    """Group per holder, named after holder."""

    @staticmethod
    def named(holder: bpy.types.Object | bpy.types.World) -> Option[str]:
        """Returns the holder's name."""
        return Some(holder.name)


class Shared(Case, frozen=True, kw_only=True):
    """Named group every holder of kind joins."""

    name: Name = "Emission"

    def named(self, _holder: bpy.types.Object | bpy.types.World) -> Option[str]:
        """Returns the shared group name."""
        return Some(self.name)


class Ungrouped(Case, frozen=True, kw_only=True):
    """No group, holder reaching Combined alone."""

    @staticmethod
    def named(_holder: bpy.types.Object | bpy.types.World) -> Option[str]:
        """Returns the empty group name."""
        return Some("")


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

    children: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="split_child", saved=Saved.USED)
    cycles: ClassVar[Option[Setting]] = Some(Setting(descriptor=Descriptor.declared(bpy.types.Context, ("scene", "render", "engine"), bpy.types.EnumProperty), accepted=("CYCLES",)))

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns lobe child ownerships and section."""
        return Ok((LightGroupEdits.children, self.section()))

    @staticmethod
    def section() -> Section:
        """Returns active object light group field and operators."""
        active = Some(lambda context: root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)))
        return Section(
            key="light_groups",
            title="Light Groups",
            rows=(
                Property(target=Member(root=bpy.types.Object, path=("lightgroup",), reach=active), search=Some(Member(root=bpy.types.ViewLayer, path=("lightgroups",)))),
                *(OperatorButton(target=command) for command in (light_groups_assign, light_group_select, light_split, light_split_toggle, light_unsplit)),
            ),
        )


# --- [TABLES] ---------------------------------------------------------------------------

GROUPINGS: Final = case_choices(Grouping)

# --- [PROPERTIES] -----------------------------------------------------------------------


class Assignment(bpy.types.PropertyGroup):
    """Light group rule per emitter kind and world with the owners it writes to."""

    lights: bpy.props.EnumProperty(name="Lights", description="Group of each light object", items=GROUPINGS, default=PerEmitter.__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    emissive: bpy.props.EnumProperty(name="Emissive", description="Group of each emissive object", items=GROUPINGS, default=Shared.__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    world: bpy.props.EnumProperty(name="World", description="World light group", items=GROUPINGS, default=PerEmitter.__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    shared: bpy.props.StringProperty(name="Shared Group", description="Group every holder of a shared rule joins", default="Emission")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scope: scope_choice(Scenes)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    collection: SCOPE_COLLECTION  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------


def grouping(choice: str, shared: str) -> Outcome[Grouping]:
    """Returns the rule a `GROUPINGS` choice names, a shared rule under its group name."""
    return catch(msgspec.convert, exception=msgspec.ValidationError)({"kind": choice, "name": shared}, Grouping).map_error(
        lambda error: Fault.of(Reason.OUTSIDE, "Shared Group", repr(shared), member=f"a group name ({error})")
    )


def assigned(scope: Scope, context: bpy.types.Context, *, lights: Grouping, emissive: Grouping, world: Grouping) -> Outcome[Option[frozenset[str]]]:
    """Writes rule light groups to emitters and worlds in scope.

    Args:
        scope: Owners to group.
        context: Context `scope.owners` reads.
        lights: Rule of each light object, `Kept` leaving them.
        emissive: Rule of each emissive object, `Kept` leaving them.
        world: Rule of each world, `Kept` leaving them.

    Returns:
        Group names written, `Nothing` when every holder held its group, or `absent` and `edit` faults.
    """

    def wrote(grouping: Grouping, holder: bpy.types.Object | bpy.types.World) -> Outcome[Option[str]]:
        return grouping.named(holder).map(lambda name: written(holder, ("lightgroup",), name).map(lambda prior: prior.map(lambda _: name))).default_value(Ok(Nothing))

    def worlds(reached: tuple[bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object, ...]) -> tuple[bpy.types.World, ...]:
        return tuple(dict.fromkeys(world for owner in reached if isinstance(scene := owner.id_data, bpy.types.Scene) for world in Option.of_optional(scene.world).to_list()))

    return scope.owners(context).bind(
        lambda reached: accumulated((
            *(wrote(lights if isinstance(item.data, bpy.types.Light) else emissive, item) for item in dict.fromkeys(item for owner in reached for item in emitters(owner))),
            *(wrote(world, held) for held in worlds(reached)),
        )).map(lambda changes: Some(tuple(name for change in changes for name in change.to_list())).filter(bool).map(lambda names: frozenset(names) - {""}))
    )


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
    kept = frozenset(lobe.slot for lobe in lobes)
    stale = tuple(item for item, mark in LightGroupEdits.children.held(identity) if mark.of == Some(master) and mark.slot.filter(kept.__contains__).is_none())

    def child(lobe: Lobe, links: Snapshot, home: bpy.types.Collection) -> Outcome[bpy.types.Object]:
        values: dict[RnaPath, Value] = {
            **{(other.value,): other is lobe for other in Lobe},
            ("visible_camera",): master.visible_camera and lobe is Lobe.VISIBLE_DIFFUSE,
            ("visible_shadow",): master.visible_shadow,
            ("hide_render",): False,
            ("lightgroup",): f"{group}_{lobe.label.lower()}",
        }
        state = ObjectState(parent=Some(Handle.of(master)), values=Some(Snapshot.of(bpy.types.Object, values)), placement=Some(Matrix.Identity(4)), hidden=Some(True))
        return (
            LightGroupEdits.children
            .made(identity, lambda: spawned(f"{master.name} {lobe.label}", Some(light), home), of=Some(master), slot=Some(lobe.slot), scope=Some(rollback))
            .bind(lambda item: accumulated(relinked(collection, (item,), linked=True) for collection in holders).map(lambda _: item))
            .bind(lambda item: accumulated((reconciled(state, item, (view_layer,)).map(lambda _: None), links.apply(item.light_linking).outcome)).map(lambda _: item))
        )

    hidden = Snapshot.of(bpy.types.Object, {("hide_render",): True})

    with ExitStack() as rollback:
        return retained(
            Result[bpy.types.Collection, Faults]
            .bind(
                Block.of_seq(holders).try_head().to_result_with(lambda: Fault.of(Reason.ABSENT, repr(master), member="users_collection")),
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
    return {master: held for master in lights(view_layer) if (held := LightGroupEdits.children.serving(identity, master))}


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
    shown = Snapshot.of(bpy.types.Object, {("hide_render",): False})
    return released(tuple(chain.from_iterable(held.values()))).bind(
        lambda count: accumulated(reconciled(ObjectState(values=Some(shown)), master, ()) for master in held).map(lambda _: Done(notes=(("INFO", f"Removed {count} children"),)))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Assignment, condition=LightGroupEdits.cycles)
def light_groups_assign(call: Call[Assignment]) -> Outcome[Answer]:
    """Writes rule light groups to the emitters and worlds in scope."""
    rules = (grouping(choice, call.properties.shared) for choice in (call.properties.lights, call.properties.emissive, call.properties.world))
    return (
        accumulated(rules)
        .bind(lambda found: assigned(chosen(call.properties), call.context, lights=found[0], emissive=found[1], world=found[2]))
        .map(
            lambda change: change.map(lambda names: Done(notes=(("INFO", f"Light groups {', '.join(sorted(names))}"),))).default_value(
                Unchanged(reason="Every emitter and world in scope holds its light group")
            )
        )
    )


@Operator.of(Empty, condition=LightGroupEdits.cycles)
def light_group_select(call: Call[Empty]) -> Outcome[Answer]:
    """Selects the objects in the active emitter's light group."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: focused(layer).bind(lambda item: picked(members(item, layer), layer))).map(lambda _: Done())


@Operator.of(Split, condition=LightGroupEdits.cycles)
def light_split(call: Call[Split]) -> Outcome[Answer]:
    """Splits selected lights into one light group child per ray kind."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: lobed(layer, frozenset(map(Lobe, call.properties.lobes)), call.identity))


@Operator.of(Empty, condition=LightGroupEdits.cycles)
def light_split_toggle(call: Call[Empty]) -> Outcome[Answer]:
    """Swaps visibility of selected split lights and their children."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: toggled(layer, call.identity))


@Operator.of(Empty, condition=LightGroupEdits.cycles)
def light_unsplit(call: Call[Empty]) -> Outcome[Answer]:
    """Removes split children and renders selected lights again."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: unsplit(layer, call.identity))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "GROUPINGS",
    "Assignment",
    "Grouping",
    "Kept",
    "LightGroupEdits",
    "Lobe",
    "PerEmitter",
    "Shared",
    "Split",
    "Ungrouped",
    "assigned",
    "grouping",
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
]
