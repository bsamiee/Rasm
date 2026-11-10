"""Light group membership by rule, member selection, and lobe split children."""

from contextlib import ExitStack
from enum import auto, StrEnum
from itertools import chain
from typing import Annotated, ClassVar, Final

import bpy
from expression import Option, Result, Some
from expression.collections import Block
from mathutils import Matrix
import msgspec

from rasm_bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._ids.ownership import Ownership, released, retained, Saved
from rasm_bpy.data._ids.store import Handle
from rasm_bpy.data._rna.conditions import chosen, Scenes, scope_choice, SCOPE_COLLECTION, Setting
from rasm_bpy.data._rna.descriptors import converted_value, Descriptor, Value, written
from rasm_bpy.data._rna.paths import RnaPath, root_in, rooted
from rasm_bpy.data._rna.properties import case_choices
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_bpy.scene._collections import relinked
from rasm_bpy.scene._lights.emitters import emitters, picked
from rasm_bpy.scene._objects import active_object, ObjectState, reconciled, spawned
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import OperatorButton, Property
from rasm_bpy.ui._layout.sections import Section
from rasm_records import accumulated, Case, Fault, Faults, Name, Nothing, Ok, Outcome, Reason

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


class PerEmitter(Case, frozen=True, kw_only=True):
    """Group per holder, named after holder."""


class Shared(Case, frozen=True, kw_only=True):
    """Named group every holder of kind joins."""

    name: Name = "Emission"


class Ungrouped(Case, frozen=True, kw_only=True):
    """No group, holder reaching Combined alone."""


class Split(bpy.types.PropertyGroup):
    """Ray kinds split light reaches through children."""

    lobes: Annotated[
        set[str],
        bpy.props.EnumProperty(
            name="Lobes",
            description="Ray kinds with one split child each",
            items=tuple((lobe.value, lobe.label, bpy.types.Object.bl_rna.properties[lobe.value].description) for lobe in Lobe),
            options={"ENUM_FLAG"},
            default={lobe.value for lobe in Lobe},
        ),
    ]


class LightGroupEdits(msgspec.Struct, frozen=True, kw_only=True):
    """Light group operators and section."""

    children: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="split_child", saved=Saved.USED)
    cycles: ClassVar[Option[Setting]] = Some(
        Setting(descriptor=Descriptor(root=bpy.types.Context, owner=("scene", "render"), prop=bpy.types.RenderSettings.bl_rna.properties["engine"]), accepted=("CYCLES",))
    )

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns lobe child ownerships and section."""
        return LightGroupEdits.children, self.section()

    @staticmethod
    def section() -> Section:
        """Returns active object light group field and operators."""
        active = Some(lambda context: root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)))
        return Section(
            key="light_groups",
            rows=(
                Property(target=Member(root=bpy.types.Object, path=("lightgroup",), reach=active), search=Some(Member(root=bpy.types.ViewLayer, path=("lightgroups",)))),
                *(OperatorButton(target=command) for command in (light_groups_assign, light_group_select, light_split, light_split_toggle, light_unsplit)),
            ),
        )


# --- [TABLES] ---------------------------------------------------------------------------

_GROUPINGS: Final = case_choices(Grouping)

# --- [PROPERTIES] -----------------------------------------------------------------------


class Assignment(bpy.types.PropertyGroup):
    """Light group rule per emitter kind and world with the owners it writes to."""

    lights: Annotated[str, bpy.props.EnumProperty(name="Lights", description="Group of each light object", items=_GROUPINGS, default=PerEmitter.__name__)]
    emissive: Annotated[str, bpy.props.EnumProperty(name="Emissive", description="Group of each emissive object", items=_GROUPINGS, default=Shared.__name__)]
    world: Annotated[str, bpy.props.EnumProperty(name="World", description="World light group", items=_GROUPINGS, default=PerEmitter.__name__)]
    shared: Annotated[str, bpy.props.StringProperty(name="Shared Group", description="Group every holder of a shared rule joins", default=Shared().name)]
    scope: Annotated[str, scope_choice(Scenes)]
    collection: Annotated[str, SCOPE_COLLECTION]


# --- [OPERATIONS] -----------------------------------------------------------------------


def light_group(name: str, /) -> str:
    """Returns name as `ViewLayer.lightgroups` holds it, which replaces dots with underscores."""
    return name.replace(".", "_")


def _split(master: bpy.types.Object, light: bpy.types.Light, lobes: frozenset[Lobe], view_layer: bpy.types.ViewLayer, identity: Identity) -> Outcome[tuple[bpy.types.Object, ...]]:
    """Creates one child per lobe sharing light data and hides master in renders.

    Args:
        master: Light object to split.
        light: Light data of master.
        lobes: Lobes to create children for.
        view_layer: View layer hiding split children.
        identity: Identity marking each child.

    Returns:
        Children in lobe order, or `absent` and `refused` faults.
    """
    group, holders = light_group(master.lightgroup or master.name), tuple(master.users_collection)
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
            .bind(lambda children: hidden.apply(master).outcome.map(lambda _: children)),
            rollback,
        )


def _lights(view_layer: bpy.types.ViewLayer) -> dict[bpy.types.Object, bpy.types.Light]:
    """Returns selected objects with light data."""
    return {item: light for item in view_layer.objects.selected if isinstance(light := item.data, bpy.types.Light)}


def _splits(view_layer: bpy.types.ViewLayer, identity: Identity) -> dict[bpy.types.Object, tuple[bpy.types.Object, ...]]:
    """Returns selected split lights in selection order with children."""
    return {master: held for master in _lights(view_layer) if (held := LightGroupEdits.children.serving(identity, master))}


def _unsplit(view_layer: bpy.types.ViewLayer, identity: Identity) -> Outcome[Answer]:
    """Removes split children of selected lights and renders lights again.

    Args:
        view_layer: Layer with selected lights.
        identity: Identity reading child marks.

    Returns:
        `Done` noting removed child count, `Unchanged` when no split light is selected, or `refused` faults.
    """
    if not (held := _splits(view_layer, identity)):
        return Ok(Unchanged(reason="No split light selected"))
    shown = Snapshot.of(bpy.types.Object, {("hide_render",): False})
    return released(tuple(chain.from_iterable(held.values()))).bind(
        lambda count: accumulated(shown.apply(master).outcome for master in held).map(lambda _: Done(notes=(("INFO", f"Removed {count} children"),)))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Assignment, condition=LightGroupEdits.cycles)
def light_groups_assign(call: Call[Assignment]) -> Outcome[Answer]:
    """Writes rule light groups to the emitters and worlds in scope."""
    rules = (converted_value({"kind": choice, "name": call.properties.shared}, Grouping, "Shared Group") for choice in (call.properties.lights, call.properties.emissive, call.properties.world))

    def wrote(grouping: Grouping, holder: bpy.types.Object | bpy.types.World) -> Outcome[Option[str]]:
        match grouping:
            case Kept():
                return Ok(Nothing)
            case PerEmitter():
                name = light_group(holder.name)
            case Shared(name=shared):
                name = light_group(shared)
            case Ungrouped():
                name = ""
        return written(holder, ("lightgroup",), name).map(lambda prior: prior.map(lambda _: name))

    return (
        accumulated(rules)
        .bind(
            lambda found: (
                chosen(call.properties)
                .bind(lambda scope: scope.owners(call.context))
                .bind(
                    lambda reached: accumulated((
                        *(wrote(found[0] if isinstance(item.data, bpy.types.Light) else found[1], item) for item in dict.fromkeys(item for owner in reached for item in emitters(owner))),
                        *(
                            wrote(found[2], world)
                            for world in dict.fromkeys(
                                world for owner in reached for scene in (owner.id_data,) if isinstance(scene, bpy.types.Scene) for world in Option.of_optional(scene.world).to_list()
                            )
                        ),
                    ))
                )
            )
        )
        .map(
            lambda changes: (
                Some(tuple(name for change in changes for name in change.to_list()))
                .filter(bool)
                .map(lambda names: Done(notes=(("INFO", f"Light groups {', '.join(sorted(frozenset(names) - {''}))}"),)))
                .default_value(Unchanged(reason="Every emitter and world in scope holds its light group"))
            )
        )
    )


@Operator.of(Empty, condition=LightGroupEdits.cycles)
def light_group_select(call: Call[Empty]) -> Outcome[Answer]:
    """Selects the objects in the active emitter's light group."""
    return (
        rooted(bpy.types.ViewLayer, call.context)
        .bind(lambda layer: active_object(layer).bind(lambda item: picked(frozenset(other for other in layer.objects if item.lightgroup and other.lightgroup == item.lightgroup), layer)))
        .map(lambda _: Done())
    )


@Operator.of(Split, condition=LightGroupEdits.cycles)
def light_split(call: Call[Split]) -> Outcome[Answer]:
    """Splits selected lights into one light group child per ray kind."""
    lobes = frozenset(map(Lobe, call.properties.lobes))
    return rooted(bpy.types.ViewLayer, call.context).bind(
        lambda layer: (
            accumulated(_split(master, light, lobes, layer, call.identity) for master, light in selected.items()).map(lambda _: Done())
            if (selected := _lights(layer))
            else Ok(Unchanged(reason="No light selected"))
        )
    )


@Operator.of(Empty, condition=LightGroupEdits.cycles)
def light_split_toggle(call: Call[Empty]) -> Outcome[Answer]:
    """Swaps visibility of selected split lights and their children."""
    return rooted(bpy.types.ViewLayer, call.context).bind(
        lambda layer: (
            accumulated(reconciled(ObjectState(hidden=Some(not item.hide_get(view_layer=layer))), item, (layer,)) for master, children in held.items() for item in (master, *children)).map(
                lambda _: Done()
            )
            if (held := _splits(layer, call.identity))
            else Ok(Unchanged(reason="No split light selected"))
        )
    )


@Operator.of(Empty, condition=LightGroupEdits.cycles)
def light_unsplit(call: Call[Empty]) -> Outcome[Answer]:
    """Removes split children and renders selected lights again."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: _unsplit(layer, call.identity))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Assignment",
    "Grouping",
    "Kept",
    "LightGroupEdits",
    "Lobe",
    "PerEmitter",
    "Shared",
    "Split",
    "Ungrouped",
    "light_group",
    "light_group_select",
    "light_groups_assign",
    "light_split",
    "light_split_toggle",
    "light_unsplit",
]
