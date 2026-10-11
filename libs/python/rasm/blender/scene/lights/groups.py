"""Light group membership by rule, member selection, and lobe split children."""

from contextlib import ExitStack
from typing import Annotated, ClassVar

import bpy
from expression import Error, Ok, Option, Result, Some
from expression.extra.result import catch
from mathutils import Matrix
import msgspec

from rasm.blender.commands.operators import Done, Empty, Operator
from rasm.blender.data.ids.ownership import earliest, Mark, Ownership, Saved
from rasm.blender.data.ids.store import Handle
from rasm.blender.data.rna.conditions import Setting
from rasm.blender.data.rna.descriptors import Descriptor, root_in, Value, written
from rasm.blender.data.rna.snapshots import Scenes, Scope, Snapshot
from rasm.blender.extension.identity import Identity
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from rasm.blender.scene.collections import joined
from rasm.blender.scene.lights.emitters import emitters, focused, picked
from rasm.blender.scene.objects import ObjectState, reconciled, spawned
from rasm.blender.ui.layout.rows import Member, OperatorButton, SearchField
from rasm.blender.ui.layout.sections import Section, Single
from rasm.imaging.faults import accumulated
from rasm.imaging.parameters import Case, Choice, Gated

# --- [TYPES] ----------------------------------------------------------------------------

type Grouping = PerEmitter | Shared | Ungrouped


class Lobe(Choice):
    """Ray kind a split child reaches."""

    VISIBLE_DIFFUSE = 1, "Diffuse"
    VISIBLE_GLOSSY = 2, "Glossy"
    VISIBLE_TRANSMISSION = 3, "Transmission"
    VISIBLE_VOLUME_SCATTER = 4, "Volume"


# --- [MODELS] ---------------------------------------------------------------------------


class PerEmitter(Case, frozen=True, kw_only=True, tag=1):
    """One group per holder, named by the holder."""

    title = "Per Emitter"


class Shared(Case, frozen=True, kw_only=True, tag=2):
    """One named group every holder of the kind joins."""

    title = "Shared"
    name: Annotated[str, msgspec.Meta(min_length=1, title="Group", description="Light group every holder of the kind joins")] = "Emission"


class Ungrouped(Case, frozen=True, kw_only=True, tag=3):
    """No light group, the holder's light reaching Combined alone."""

    title = "None"


class Membership(msgspec.Struct, frozen=True, kw_only=True):
    """Light group rule per emitter kind and for the world."""

    lights: Annotated[Gated[Grouping], msgspec.Meta(title="Lights", description="Light group of each light object")] = Gated(enabled=True, value=PerEmitter())
    emissive: Annotated[Gated[Grouping], msgspec.Meta(title="Emissive", description="Light group of each emissive object")] = Gated(enabled=True, value=Shared())
    world: Annotated[Gated[Grouping], msgspec.Meta(title="World", description="Light group of the world")] = Gated(enabled=True, value=PerEmitter())


class Assignment(msgspec.Struct, frozen=True, kw_only=True):
    """Membership rule over the owners an assignment writes to."""

    membership: Annotated[Membership, msgspec.Meta(title="Membership", description="Light group rule per kind")] = Membership()
    scope: Annotated[Scope, msgspec.Meta(title="Scope", description="Owners whose emitters and worlds take groups")] = Scenes()


class Split(msgspec.Struct, frozen=True, kw_only=True):
    """Ray kinds a split light reaches through its own children."""

    lobes: Annotated[frozenset[Lobe], msgspec.Meta(title="Lobes", description="Ray kinds each split light reaches through its own child")] = frozenset(Lobe)


class LightGroupEdits(msgspec.Struct, frozen=True, kw_only=True):
    """Row of membership, member selection, and lobe split operators."""

    children: ClassVar[dict[Lobe, Ownership[bpy.types.Object]]] = {lobe: Ownership(kind=bpy.types.Object, key=lobe.name.lower(), saved=Saved.USED) for lobe in Lobe}
    cycles: ClassVar[Option[Setting]] = Some(
        Setting(descriptor=Descriptor(root=bpy.types.Context, owner="scene.render", prop=bpy.types.RenderSettings.bl_rna.properties["engine"]), accepted=("CYCLES",))
    )

    @staticmethod
    def lights(layer: bpy.types.ViewLayer) -> dict[bpy.types.Object, bpy.types.Light]:
        """Returns selected objects paired with their native light data."""
        return {item: light for item in layer.objects.selected for light in (item.data,) if isinstance(light, bpy.types.Light)}

    commands: ClassVar[tuple[Operator[Assignment], Operator[Empty], Operator[Split], Operator[Empty], Operator[Empty]]] = (
        Operator(
            key="light_groups_assign",
            title="Assign Light Groups",
            description="Write the light groups a rule names onto a scope's emitters and worlds",
            form=Assignment,
            condition=cycles,
            run=lambda call: assigned(call.record.membership, call.record.scope, call.context).map(lambda names: Done(notes=(("INFO", f"Light groups {', '.join(sorted(names))}"),))),
        ),
        Operator(
            key="light_group_select",
            title="Select Group Members",
            description="Select the objects sharing the active emitter's light group",
            form=Empty,
            condition=cycles,
            run=lambda call: call.root(bpy.types.ViewLayer).bind(lambda layer: focused(layer).bind(lambda item: picked(members(item, layer), layer))).map(lambda _: Done()),
        ),
        Operator(
            key="light_split",
            title="Split Lobes",
            description="Give each selected light one child per ray kind, each in its own light group",
            form=Split,
            condition=cycles,
            run=lambda call: (
                call
                .root(bpy.types.ViewLayer)
                .bind(lambda layer: accumulated(split(master, light, call.record, layer, call.identity) for master, light in LightGroupEdits.lights(layer).items()))
                .map(lambda _: Done())
            ),
        ),
        Operator(
            key="light_split_toggle",
            title="Toggle Split",
            description="Swap view layer visibility between split lights and their children",
            form=Empty,
            condition=cycles,
            run=lambda call: call.root(bpy.types.ViewLayer).bind(lambda layer: toggled(tuple(LightGroupEdits.lights(layer)), layer, call.identity)).map(lambda _: Done()),
        ),
        Operator(
            key="light_unsplit",
            title="Remove Split",
            description="Remove the selected lights' split children and render the lights again",
            form=Empty,
            condition=cycles,
            run=lambda call: (
                call
                .root(bpy.types.ViewLayer)
                .bind(lambda layer: accumulated(unsplit(master, call.identity) for master in LightGroupEdits.lights(layer)))
                .map(lambda counts: Done(notes=(("INFO", f"Removed {sum(counts)} children"),)))
            ),
        ),
    )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers each lobe row's ownership merge and the five operators.

        Args:
            identity: Extension placing the row.

        Returns:
            `Ok` of every band and acquisition pair, or every construction fault.
        """
        return Ok((*LightGroupEdits.children.values(), self.section()))

    def section(self) -> Section:
        """Section drawing the active object's light group and the group operators."""
        active = Some(lambda context: root_in(bpy.types.ViewLayer, context).bind(lambda layer: Option.of_optional(layer.objects.active)))
        return Single(
            key="light_groups",
            title="Light Groups",
            rows=(
                SearchField(target=Member(root=bpy.types.Object, path="lightgroup", reach=active), collection=Member(root=bpy.types.ViewLayer, path="lightgroups")),
                *(OperatorButton(target=command) for command in self.commands),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def assigned(membership: Membership, scope: Scope, context: bpy.types.Context) -> BlenderResult[frozenset[str]]:
    """Writes the light groups a rule names onto a scope's emitters and worlds.

    Args:
        membership: Rule per kind, a gate off leaving that kind.
        scope: Owners whose emitters and scene worlds take groups.
        context: Context the scope's `owners` reads.

    Returns:
        `Ok` of the group names written; `absent` for a missing scope name and `edit` per refused write.
    """

    def named(grouping: Grouping, holder: bpy.types.Object | bpy.types.World) -> str:
        match grouping:
            case PerEmitter():
                return holder.name
            case Shared(name=name):
                return name
            case Ungrouped():
                return ""

    def wrote(rule: Gated[Grouping], holder: bpy.types.Object | bpy.types.World) -> BlenderResult[frozenset[str]]:
        name = named(rule.value, holder)
        return written(holder, "lightgroup", name).map(lambda _: frozenset({name} - {""})) if rule.enabled else Ok(frozenset())

    def worlds(reached: tuple[bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object, ...]) -> tuple[bpy.types.World, ...]:
        scenes: tuple[bpy.types.Scene, ...] = tuple(owner if isinstance(owner, bpy.types.Scene) else owner.id_data for owner in reached if isinstance(owner, bpy.types.Scene | bpy.types.ViewLayer))  # type: ignore[misc]  # ty: ignore[invalid-assignment]
        return tuple(dict.fromkeys(world for scene in scenes for world in Option.of_optional(scene.world).to_list()))

    return scope.owners(context).bind(
        lambda reached: accumulated((
            *(wrote(membership.lights if isinstance(item.data, bpy.types.Light) else membership.emissive, item) for item in dict.fromkeys(item for owner in reached for item in emitters(owner))),
            *(wrote(membership.world, world) for world in worlds(reached)),
        )).map(lambda parts: frozenset[str]().union(*parts))
    )


def used(scene: bpy.types.Scene) -> frozenset[str]:
    """Answers every light group name a scene's objects and world hold."""
    return frozenset(name for name in (*(item.lightgroup for item in scene.objects), *Option.of_optional(scene.world).map(lambda world: world.lightgroup).to_list()) if name)


def members(item: bpy.types.Object, view_layer: bpy.types.ViewLayer) -> frozenset[bpy.types.Object]:
    """Answers the objects of a view layer sharing an object's light group, none for an ungrouped object."""
    return frozenset(other for other in view_layer.objects if item.lightgroup and other.lightgroup == item.lightgroup)


def split(master: bpy.types.Object, light: bpy.types.Light, record: Split, view_layer: bpy.types.ViewLayer, identity: Identity) -> BlenderResult[tuple[bpy.types.Object, ...]]:
    """Makes one child per lobe sharing a light's data, each reaching one ray kind.

    Args:
        master: Light object split.
        light: Native data admitted with the master.
        record: Lobes the children take.
        view_layer: View layer hiding the split children while the master is edited.
        identity: Identity marking each child.

    Returns:
        `Ok` of the children in lobe order; `edit` and the reconcile's faults.
    """
    group, holders = master.lightgroup or master.name, tuple(master.users_collection)
    inverse = tuple(value for row in Matrix.Identity(4) for value in row)  # type: ignore[attr-defined]
    stale = tuple(item for lobe, row in LightGroupEdits.children.items() if lobe not in record.lobes for item, mark in row.held(identity) if mark.of == Some(master))

    def retained(item: bpy.types.Object) -> bpy.types.Object:
        rollback.callback(bpy.data.batch_remove, ids=(item,))
        return item

    def child(lobe: Lobe, links: Snapshot, home: bpy.types.Collection) -> BlenderResult[bpy.types.Object]:
        values: dict[str, Value] = {
            **{other.name.lower(): other is lobe for other in Lobe},
            "visible_camera": master.visible_camera and lobe is Lobe.VISIBLE_DIFFUSE,
            "visible_shadow": master.visible_shadow,
            "hide_render": False,
            "lightgroup": f"{group}_{lobe.label.lower()}",
            "matrix_parent_inverse": inverse,
        }
        state = ObjectState(parent=Some(Handle.of(master)), values=Some(Snapshot.of(bpy.types.Object, values)), placement=Some(master.matrix_world.copy().freeze()), hidden=Some(True))
        row = LightGroupEdits.children[lobe]
        mark = Mark(key=row.key, of=Some(master))
        return (
            Option
            .of_optional(earliest(row.held(identity)).get(mark))
            .map(Ok)
            .default_with(lambda: spawned(f"{master.name} {lobe.label}", Some(light), home).bind(lambda item: row.owned(identity, item, mark)).map(retained))
            .bind(lambda item: accumulated(joined(collection, (item,)) for collection in holders).map(lambda _: item))
            .bind(lambda item: accumulated((reconciled(state, item, (view_layer,)).map(lambda _: None), links.apply(item.light_linking).outcome)).map(lambda _: item))
        )

    hidden = Snapshot.of(bpy.types.Object, {"hide_render": True})

    def published(children: tuple[bpy.types.Object, ...]) -> tuple[bpy.types.Object, ...]:
        rollback.pop_all()
        return children

    with ExitStack() as rollback:
        return (
            Result[bpy.types.Collection, BlenderFaults]
            .bind(
                Option.of_optional(next(iter(holders), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(master), key="users_collection"),)),
                lambda home: Snapshot.capture(master.light_linking).bind(
                    lambda links: Result[None, BlenderFaults].bind(
                        catch(lambda: bpy.data.batch_remove(ids=stale), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(master), error=error),)),
                        lambda _: accumulated(child(lobe, links, home) for lobe in Lobe if lobe in record.lobes),
                    )
                ),
            )
            .bind(lambda children: reconciled(ObjectState(values=Some(hidden)), master, ()).map(lambda _: children))
            .map(published)
        )


def toggled(masters: tuple[bpy.types.Object, ...], view_layer: bpy.types.ViewLayer, identity: Identity) -> BlenderResult[None]:
    """Swaps layer visibility between split masters and their children.

    Args:
        masters: Split light objects.
        view_layer: View layer whose base flags change.
        identity: Identity reading the children's marks.

    Returns:
        `Ok` once swapped; the `edit` faults of each refused write.
    """
    held = tuple(item for master in masters for row in LightGroupEdits.children.values() for item, mark in row.held(identity) if mark.of == Some(master))
    return accumulated(reconciled(ObjectState(hidden=Some(not item.hide_get(view_layer=view_layer))), item, (view_layer,)) for item in (*masters, *held)).map(lambda _: None)


def unsplit(master: bpy.types.Object, identity: Identity) -> BlenderResult[int]:
    """Removes a master's split children and shows the master in renders.

    Args:
        master: Split light object.
        identity: Identity reading the children's marks.

    Returns:
        `Ok` of the children removed; `edit` for a refused write.
    """
    held = tuple(item for row in LightGroupEdits.children.values() for item, mark in row.held(identity) if mark.of == Some(master))
    shown = Snapshot.of(bpy.types.Object, {"hide_render": False})
    try:
        bpy.data.batch_remove(ids=held)
    except (ReferenceError, RuntimeError, TypeError, ValueError) as error:
        return Error((BlenderEditFault(request=repr(master), error=error),))
    return reconciled(ObjectState(values=Some(shown)), master, ()).map(lambda _: len(held))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Assignment", "Grouping", "LightGroupEdits", "Lobe", "Membership", "PerEmitter", "Shared", "Split", "Ungrouped", "assigned", "members", "split", "toggled", "unsplit", "used"]
