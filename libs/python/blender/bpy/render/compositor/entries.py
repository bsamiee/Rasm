"""Effects an extension offers together over scene compositor entries: phases ranking them, placement by rank, moves, removal, holds, and refresh."""

from collections.abc import Mapping
from contextlib import ExitStack, nullcontext
from enum import auto, Enum
from functools import cached_property, partial, reduce
from operator import or_
from types import MappingProxyType
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from blender.bpy.commands.calls import called
from blender.bpy.data.ids.ownership import Mark, Ownership
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Declaration
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import CompositorEffect, Enabled, marked, Opened, ownerships
from blender.bpy.render.compositor.inputs import hold, holding
from blender.bpy.render.compositor.shells import PreviewFrame
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.scene import cameras
from blender.bpy.settings.attachments import Stored
from blender.records.faults import accumulated, Fault, Faults, Outcome
from blender.records.fields import Case
from blender.records.trees import Toward

# --- [TYPES] ----------------------------------------------------------------------------


class Edge(Enum):
    """Stack end holding an unranked pinned group."""

    HEAD = auto()
    TAIL = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

EXECUTING: Final = Resource[set[int]](made=lambda _identity: nullcontext(set()))

# --- [MODELS] ---------------------------------------------------------------------------


class Exclusive[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Effects of one rank a stack holds one entry of."""

    members: tuple[CompositorEffect[R], ...]


class Phase[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Run of ranked effects in processing order, each member one rank, the add menu filing them under its group."""

    members: tuple[CompositorEffect[R] | Exclusive[R], ...]
    group: Option[Enum] = Nothing


class Pinned(msgspec.Struct, frozen=True, kw_only=True):
    """Unranked owned group with stack end, enables, and the conditions its entry needs, its entry named after the ownership key unless titled."""

    ownership: Ownership[bpy.types.CompositorNodeTree]
    edge: Edge
    title: Option[str] = Nothing
    enables: frozenset[Enabled] = frozenset(Enabled)
    conditions: tuple[Condition, ...] = ()

    @property
    def named(self) -> str:
        """Entry title, the ownership key's display name unless stated."""
        return self.title.default_with(lambda: bpy.path.display_name(self.ownership.key))

    def checked(self, context: bpy.types.Context, entry: bpy.types.SceneCompositorEffect) -> Outcome[None]:
        """Checks every condition the pinned entry needs against the entry."""
        return accumulated(condition.check(context, Some(entry)) for condition in self.conditions).map(lambda _: None)


class Owned[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Entry whose group carries the ownership mark of an offered effect."""

    entry: bpy.types.SceneCompositorEffect
    row: CompositorEffect[R]


# --- [SERVICES] -------------------------------------------------------------------------


class EffectRows[R: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Effects an extension offers in phases, their pinned groups, and the commands editing their entries."""

    phases: tuple[Phase[R], ...]
    pinned: tuple[Pinned, ...] = ()

    # --- [OFFERED_ROWS]

    @cached_property
    def rows(self) -> tuple[CompositorEffect[R], ...]:
        """Offered effects in rank order."""
        return tuple(row for phase in self.phases for member in phase.members for row in members(member))

    @cached_property
    def keyed(self) -> Mapping[Key, CompositorEffect[R]]:
        """Offered effects by each ownership key of their groups."""
        return MappingProxyType({owner.key: row for row in self.rows for owner in ownerships(row.record)})

    def rank(self, row: CompositorEffect[R]) -> int:
        """Position of the phase member holding the effect, across every phase."""
        return next(index for index, member in enumerate(member for phase in self.phases for member in phase.members) if row in members(member))

    def single(self, row: CompositorEffect[R]) -> bool:
        """Whether the effect shares an exclusive rank, a stack holding one entry of that rank."""
        return any(isinstance(member, Exclusive) and row in member.members for phase in self.phases for member in phase.members)

    @property
    def form(self) -> TypeForm[R]:
        """Union of offered record types."""
        return reduce(or_, (row.record for row in self.rows))

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Frame size attachment camera frame derivation binds, and the cell of scenes under execution."""
        return (FrameSize.attachment, EXECUTING)

    @property
    def standalone(self) -> tuple[CompositorEffect[R], ...]:
        """Effects needing neither render pass request nor file."""
        return tuple(row for row in self.rows if row.request.is_none() and not isinstance(row.source, Opened))

    def offering(self, kind: type[Case]) -> Outcome[CompositorEffect[R]]:
        """Finds the effect of exact record type."""
        return Option.of_optional(self.keyed.get(item_key(kind))).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(tuple(row.kind for row in self.rows)), member=kind.__name__))

    # --- [ENTRIES]

    def owned(self, identity: Identity, scene: bpy.types.Scene) -> tuple[Owned[R], ...]:
        """Returns entries offered effects own, with their effects, in stack order."""
        return tuple(Owned(entry=entry, row=self.keyed[mark.key]) for entry, mark in marked(identity, scene) if mark.key in self.keyed)

    def row(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Option[CompositorEffect[R]]:
        """Returns the offered effect owning entry's group."""
        return Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).bind(lambda mark: Option.of_optional(self.keyed.get(mark.key)))

    def found(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[Owned[R]]:
        """Finds named owned entry, skipping user groups."""
        return (
            Option
            .of_optional(scene.compositor_effects.get(name))
            .bind(lambda entry: self.row(identity, entry).map(lambda row: Owned(entry=entry, row=row)))
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    def held(self, identity: Identity, scene: bpy.types.Scene) -> tuple[CompositorEffect[R], ...]:
        """Returns effects holding an entry in the scene, in declaration order."""
        keys = frozenset(mark.key for _, mark in marked(identity, scene))
        return tuple(row for row in self.rows if any(owner.key in keys for owner in ownerships(row.record)))

    # --- [PLACEMENT]

    def placed(self, identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[R], record: Entry[R], *, preview: Option[PreviewFrame] = Nothing) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Places entry at rank under the preview frame, the held entry of a single rank kept, applying holds and restoring the active entry."""
        prior = Option.of_optional(scene.compositor_effects.active)
        existing = Block.of_seq(self.owned(identity, scene)).filter(lambda held: self.rank(held.row) == self.rank(row)).try_head().map(lambda held: held.entry) if self.single(row) else Nothing

        def added() -> Outcome[bpy.types.SceneCompositorEffect]:
            return row.appended(identity, scene, record, preview=preview).bind(
                lambda entry: (
                    hold(identity, scene, row.kind, row.holds)
                    .bind(lambda _: self.moved(identity, scene, entry.name, Nothing))
                    .bind(lambda _: prior.map(lambda active: written(active, ("is_active",), True).map(lambda _: None)).default_value(Ok(None)))
                    .map(lambda _: entry)
                )
            )

        return existing.map(Ok).default_with(added)

    def assigned(self, identity: Identity, scene: bpy.types.Scene, record: R) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Writes the earliest entry of the record's type or places a new one."""

        def assigned(row: CompositorEffect[R]) -> Outcome[bpy.types.SceneCompositorEffect]:
            return (
                Block
                .of_seq(row.entries(identity, scene))
                .try_head()
                .map(lambda entry: row.read(identity, entry).bind(lambda prior: row.write(identity, scene, entry, msgspec.structs.replace(prior, record=record))).map(lambda _: entry))
                .default_with(lambda: self.placed(identity, scene, row, Entry(record=record)))
            )

        return self.offering(type(record)).bind(assigned)

    def moved(self, identity: Identity, scene: bpy.types.Scene, name: str, toward: Option[Toward]) -> Outcome[bool]:
        """Moves named owned entry alone one step, else into its rank, answering whether it moved."""

        def moved(held: Owned[R]) -> Outcome[bool]:
            order = tuple(scene.compositor_effects)
            tail = frozenset(pinned.ownership.key for pinned in self.pinned if pinned.edge is Edge.TAIL)
            above = frozenset(entry for entry, mark in marked(identity, scene) if mark.key in tail) | frozenset(
                owned.entry for owned in self.owned(identity, scene) if self.rank(owned.row) > self.rank(held.row)
            )
            others = tuple(entry for entry in order if entry != held.entry)
            ranked = next((index for index, entry in enumerate(others) if entry in above), len(others))
            return relocated(scene, held.entry, toward.map(lambda step: step.stepped(order.index(held.entry), len(order))).default_value(ranked))

        return self.found(identity, scene, name).bind(moved)

    def removed(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[None]:
        """Removes owned entry, unused groups, and last-entry holds."""

        def removed(held: Owned[R]) -> Outcome[None]:
            row = held.row
            return (
                Result[None, Faults]
                .bind(
                    catch(scene.compositor_effects.remove, exception=RuntimeError)(held.entry).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(scene))),
                    lambda _: self.release(identity, scene, row) if not row.entries(identity, scene) else Ok(None),
                )
                .bind(lambda _: accumulated(owner.removed(identity) for owner in ownerships(row.record)))
                .map(lambda _: None)
            )

        return self.found(identity, scene, name).bind(removed)

    # --- [SCENE_SETTINGS]

    def release(self, identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[R]) -> Outcome[None]:
        """Restores held scene settings, reapplying camera holds of remaining effects."""

        def released(store: Stored[dict[Key, Snapshot]], values: dict[Key, Snapshot]) -> Outcome[None]:
            return (
                Option
                .of_optional(values.get(row.kind))
                .map(lambda snapshot: snapshot.apply(scene).outcome)
                .default_value(Ok(None))
                .bind(lambda _: store.write(scene, {key: value for key, value in values.items() if key != row.kind}))
                .bind(lambda _: cameras.released(identity))
                .bind(
                    lambda _: accumulated(cameras.required(holds.cameras, target, identity) for target in bpy.data.scenes for offered in self.held(identity, target) for holds in (offered.holds,)).map(
                        lambda _: None
                    )
                )
            )

        return holding(identity, scene).bind(lambda pair: released(*pair))

    # --- [REFRESH]

    def refreshed(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[bool]:
        """Separates per-entry groups, places missing pinned entries, and releases stale holds, answering whether an entry changed."""

        def separated(entry: bpy.types.SceneCompositorEffect, owner: Ownership[bpy.types.NodeTree], group: bpy.types.NodeTree) -> Outcome[bool]:
            copied: bpy.types.NodeTree = group.copy()  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return owner.owned(identity, copied, of=Some(scene)).bind(lambda owned: written(entry, ("node_group",), owned)).map(lambda _: True)

        def placed(pinned: Pinned, group: bpy.types.CompositorNodeTree) -> Outcome[bool]:
            with ExitStack() as rollback:
                entry = scene.compositor_effects.new(pinned.named)
                rollback.callback(scene.compositor_effects.remove, entry)
                return (
                    accumulated((written(entry, ("node_group",), group), *(enabled.descriptor.write(entry, enabled in pinned.enables) for enabled in Enabled)))
                    .bind(lambda _: relocated(scene, entry, 0 if pinned.edge is Edge.HEAD else len(scene.compositor_effects) - 1))
                    .map(lambda _: rollback.pop_all())
                    .map(lambda _: True)
                )

        personal = tuple(
            (owned.entry, owner, group)
            for owned in self.owned(identity, scene)
            for owner in (ownerships(owned.row.record)[1],)
            for group in Option.of_optional(owned.entry.node_group).to_list()
            if Mark.read(identity, group).filter(lambda mark: mark.key == owner.key).is_some()
        )
        keeper = {group.session_uid: entry for entry, _, group in reversed(personal)}
        absent = (
            (pinned, group)
            for pinned in self.pinned
            for group in Block.of_seq(pinned.ownership.held(identity)).filter(lambda pair: pair[1].of == Some(scene)).try_head().map(lambda pair: pair[0]).to_list()
            if not any(entry.node_group == group for entry in scene.compositor_effects)
        )
        return (
            reduce(lambda done, shared: done.bind(lambda _: separated(*shared)), ((entry, owner, group) for entry, owner, group in personal if keeper[group.session_uid] != entry), Ok(False))
            .bind(lambda changed: reduce(lambda done, missing: done.bind(lambda _: placed(*missing)), absent, Ok(changed)))
            .bind(
                lambda changed: (
                    holding(identity, scene)
                    .bind(lambda pair: accumulated(self.release(identity, scene, row) for row in self.rows if row.kind in pair[1] and not row.entries(identity, scene)))
                    .map(lambda _: changed)
                )
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def relocated(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, index: int) -> Outcome[bool]:
    """Moves entry to stack index unless it sits there, answering whether it moved."""
    return (
        Ok(False)
        if scene.compositor_effects.find(entry.name) == index
        else called(bpy.ops.scene.move_compositor_effect_to_index, {"name": entry.name, "index": index}, override=Some(bpy.context.temp_override(scene=scene))).map(lambda _: True)
    )


def members[R: Case](member: CompositorEffect[R] | Exclusive[R]) -> tuple[CompositorEffect[R], ...]:
    """Effects one phase member holds."""
    return member.members if isinstance(member, Exclusive) else (member,)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EXECUTING", "Edge", "EffectRows", "Exclusive", "Owned", "Phase", "Pinned", "members", "relocated"]
