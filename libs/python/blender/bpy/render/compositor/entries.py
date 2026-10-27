"""Effects an extension offers together over scene compositor entries: placement by rank, recipes, holds, refresh, and the commands and handlers keeping entries current."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack, nullcontext
from enum import auto, Enum
from functools import partial, reduce
from operator import or_
from types import MappingProxyType
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np
import PyOpenColorIO
from typing_extensions import TypeForm

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Open, Operator, Unchanged
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Condition, Setting
from blender.bpy.data.rna.descriptors import choices, Descriptor, rna, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler, startup, Subscription, updated
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.faults import BlenderReason
from blender.bpy.files.interchange import Importing, Interchange
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.graphs import Expr, group_input, NodeGroup
from blender.bpy.nodes.interface import Interface, item_key, Socket
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import CompositorEffect, Context, Derives, EffectName, Enabled, framing, Holds, Marks, Named, Opens, ownerships, Shell
from blender.bpy.render.compositor.inputs import captured, defaulted, HELD, interface_input, regrouped, restored
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.results import produced, PRODUCTIONS
from blender.bpy.scene import cameras
from blender.bpy.settings.attachments import Stored
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.surfaces.menus import Dropdown, Menu, Submenus
from blender.imaging.color.processors import processor
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------


class Edge(Enum):
    """Stack end holding an unranked pinned group."""

    HEAD = auto()
    TAIL = auto()


class Toward(Enum):
    """Move target, neighbor or rank."""

    UP = auto()
    DOWN = auto()
    RANK = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Shift(bpy.types.PropertyGroup):
    """Named entry and requested move."""

    name: bpy.props.StringProperty(name="Entry", description="Compositor entry name")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    toward: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Toward",
        items=choices((
            (Toward.UP.name, "Up", "Move effect up the stack"),
            (Toward.DOWN.name, "Down", "Move effect down the stack"),
            (Toward.RANK.name, "Processing Order", "Move effect into processing order"),
        )),
        default="RANK",
    )


class Pinned(msgspec.Struct, frozen=True, kw_only=True):
    """Unranked owned group with entry title, stack end, enables, and conditions."""

    ownership: Ownership[bpy.types.CompositorNodeTree]
    title: str
    edge: Edge
    enables: frozenset[Enabled] = frozenset(Enabled)
    conditions: Option[Callable[[Identity, bpy.types.SceneCompositorEffect], Outcome[dict[Key, Condition]]]] = Nothing


class Owned[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Entry whose group carries the ownership mark of an offered effect."""

    entry: bpy.types.SceneCompositorEffect
    row: CompositorEffect[R]


class Configured[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Entry with its input values by item key and its render and preview enables."""

    entry: Entry[R]
    values: Snapshot
    render: bool
    preview: bool


class EffectOperators(msgspec.Struct, frozen=True, kw_only=True):
    """Entry edit commands menus and shortcuts call."""

    moving: Operator[Shift]
    removing: Operator[Named]
    resetting: Operator[Named]
    adding: tuple[Operator[Empty], ...]


# --- [SERVICES] -------------------------------------------------------------------------


class EffectRows[R: Case]:
    """Effects an extension offers, their pinned groups, placement rank, single-entry ranks, and add menu groups."""

    rows: tuple[CompositorEffect[R], ...]
    pinned: tuple[Pinned, ...]
    rank: Callable[[CompositorEffect[R]], int]
    single: Callable[[CompositorEffect[R]], bool]
    groups: Option[Callable[[CompositorEffect[R]], Enum]]
    operators: EffectOperators
    keyed: Mapping[Key, CompositorEffect[R]]

    # --- [OFFERED_ROWS]

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
        return tuple(row for row in self.rows if not row.requests and not row.found(Opens))

    @property
    def menu(self) -> Menu:
        """Add menu of every effect in rank order, one submenu per group where groups are given."""
        ordered = sorted(zip(self.rows, self.operators.adding), key=lambda pair: self.rank(pair[0]))
        buttons = tuple((row, OperatorButton(target=operator, text=row.title)) for row, operator in ordered)
        return Menu(
            key="effect_add",
            title="Add Effect",
            description="Add a scene compositor effect",
            body=self.groups.map(lambda group: Submenus(entries=tuple((group(row), button) for row, button in buttons))).default_with(
                lambda: Dropdown(entries=tuple(button for _, button in buttons), search=True)
            ),
        )

    def offering(self, kind: type[Case]) -> Outcome[CompositorEffect[R]]:
        """Finds the effect of exact record type."""
        return (
            Block
            .of_seq(self.rows)
            .filter(lambda row: row.record is kind)
            .try_head()
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(tuple(row.kind for row in self.rows)), member=item_key(kind)))
        )

    # --- [ENTRIES]

    def owned(self, identity: Identity, scene: bpy.types.Scene) -> tuple[Owned[R], ...]:
        """Returns entries offered effects own, with their effects, in stack order."""
        return tuple(Owned(entry=entry, row=self.keyed[mark.key]) for entry, mark in Marks.of(identity, scene).held if mark.key in self.keyed)

    def row(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Option[CompositorEffect[R]]:
        """Returns the offered effect owning entry's group."""
        return Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).bind(lambda mark: Option.of_optional(self.keyed.get(mark.key)))

    def found(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[Owned[R]]:
        """Finds named owned entry, skipping user groups."""
        return (
            Block
            .of_seq(self.owned(identity, scene))
            .filter(lambda held: held.entry.name == name)
            .try_head()
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    def held(self, identity: Identity, scene: bpy.types.Scene) -> tuple[CompositorEffect[R], ...]:
        """Returns effects holding an entry in the scene, in declaration order."""
        keys = frozenset(mark.key for _, mark in Marks.of(identity, scene).held)
        return tuple(row for row in self.rows if any(owner.key in keys for owner in ownerships(row.record)))

    # --- [RECIPES]

    def captured(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[dict[EffectName, Configured[R]]]:
        """Reads owned entry records, input values, and enables by name in stack order."""
        return accumulated(
            paired(held.row.read(identity, held.entry), captured(identity, held.entry)).map(
                lambda pair: (held.entry.name, Configured(entry=pair[0], values=pair[1], render=held.entry.enable_for_render, preview=held.entry.enable_for_preview))
            )
            for held in self.owned(identity, scene)
        ).map(dict)

    def applied(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        recipe: Mapping[EffectName, Configured[R]],
        *,
        locked: frozenset[EffectName] = frozenset(),
        fields: Mapping[EffectName, frozenset[Key]] = {},
        context: Context = Nothing,
    ) -> Outcome[None]:
        """Applies sparse named records, keeping locked, unlisted, and user entries."""

        def applied(current: Mapping[EffectName, Configured[R]]) -> Outcome[None]:
            desired = projected(current, recipe, locked=locked, fields=fields)
            requested = {name: desired[name] for name in recipe if name not in locked}
            native = {entry.name: entry for entry in scene.compositor_effects}
            retired = tuple(dict.fromkeys(type(value.entry.record) for value in current.values() if not any(type(record.entry.record) is type(value.entry.record) for record in desired.values())))

            def write(rows: tuple[CompositorEffect[R], ...], restores: tuple[CompositorEffect[R], ...]) -> Outcome[None]:
                order = tuple(native.values())
                prior = next((entry for entry in order if entry.is_active), None)

                def written_entry(name: EffectName, value: Configured[R], row: CompositorEffect[R]) -> Outcome[tuple[bpy.types.SceneCompositorEffect, CompositorEffect[R]]]:
                    return (
                        Option
                        .of_optional(native.get(name))
                        .map(lambda entry: row.write(identity, scene, entry, value.entry, context=context).map(lambda _: entry))
                        .default_with(lambda: row.appended(identity, scene, value.entry, context=context, name=Some(name)))
                    ).bind(
                        lambda entry: paired(restored(identity, entry, value.values), written(entry, "enable_for_render", value.render), written(entry, "enable_for_preview", value.preview)).map(
                            lambda _: (entry, row)
                        )
                    )

                def arranged(placed: tuple[tuple[bpy.types.SceneCompositorEffect, CompositorEffect[R]], ...]) -> Outcome[None]:
                    live = dict(zip(requested, (entry for entry, _ in placed)))
                    selected = iter(native.get(name, live[name]) if name in live else native[name] for name in desired)
                    target = tuple(next(selected) if entry.name in current else entry for entry in order) + tuple(selected)
                    kept = frozenset(native[name] for name in current) | frozenset(live.values())
                    return (
                        accumulated(self.release(identity, scene, row) for row in restores)
                        .bind(lambda _: accumulated(hold(identity, scene, row) for _, row in placed))
                        .bind(lambda _: accumulated(relocated(scene, entry, index) for index, entry in enumerate(target) if entry in kept))
                        .map(lambda _: None)
                    )

                with ExitStack() as scope, bpy.context.temp_override(scene=scene):
                    if prior is not None:
                        scope.callback(setattr, prior, "is_active", True)
                    return accumulated(written_entry(name, value, row) for (name, value), row in zip(requested.items(), rows)).bind(arranged)

            restores = accumulated(map(self.offering, retired))
            names = accumulated(
                Descriptor.validated(NAME.prop, name).bind(
                    lambda _: Error(Fault.of(BlenderReason.TAKEN, name, member=repr(scene.compositor_effects[name]))) if name not in current and name in scene.compositor_effects else Ok(None)
                )
                for name in requested
            )
            return (
                Ok(None) if not requested else paired(names, paired(accumulated(self.offering(type(value.entry.record)) for value in requested.values()), restores)).bind(lambda pair: write(*pair[1]))
            )

        return self.captured(identity, scene).bind(applied)

    # --- [PLACEMENT]

    def placed(self, identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[R], record: Entry[R], *, context: Context = Nothing) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Places entry at rank under the frame context, the held entry of a single rank kept, applying holds and restoring the active entry."""
        prior = Block.of_seq(scene.compositor_effects).filter(lambda entry: entry.is_active).try_head()
        existing = Block.of_seq(self.owned(identity, scene)).filter(lambda held: self.rank(held.row) == self.rank(row)).try_head().map(lambda held: held.entry) if self.single(row) else Nothing

        def added() -> Outcome[bpy.types.SceneCompositorEffect]:
            return row.appended(identity, scene, record, context=context).bind(
                lambda entry: (
                    hold(identity, scene, row)
                    .bind(lambda _: self.moved(identity, scene, entry.name, Toward.RANK))
                    .bind(lambda _: prior.map(lambda active: written(active, "is_active", True).map(lambda _: None)).default_value(Ok(None)))
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

    def moved(self, identity: Identity, scene: bpy.types.Scene, name: str, toward: Toward) -> Outcome[bool]:
        """Moves named owned entry alone, answering whether it moved."""

        def moved(held: Owned[R]) -> Outcome[bool]:
            order = tuple(scene.compositor_effects)
            position = order.index(held.entry)
            tail = frozenset(pinned.ownership.key for pinned in self.pinned if pinned.edge is Edge.TAIL)
            above = frozenset(entry for entry, mark in Marks.of(identity, scene).held if mark.key in tail) | frozenset(
                owned.entry for owned in self.owned(identity, scene) if self.rank(owned.row) > self.rank(held.row)
            )
            others = tuple(entry for entry in order if entry != held.entry)
            match toward:
                case Toward.UP:
                    index = max(0, position - 1)
                case Toward.DOWN:
                    index = min(len(order) - 1, position + 1)
                case Toward.RANK:
                    index = next((index for index, entry in enumerate(others) if entry in above), len(others))
            return relocated(scene, held.entry, index)

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

        def released(held: dict[Key, Snapshot]) -> Outcome[None]:
            return (
                Option
                .of_optional(held.get(row.kind))
                .map(lambda snapshot: snapshot.apply(scene).outcome)
                .default_value(Ok(None))
                .bind(lambda _: HELD.written(identity, scene, {key: value for key, value in held.items() if key != row.kind}))
                .bind(lambda _: cameras.released(identity))
                .bind(
                    lambda _: accumulated(
                        cameras.required(holds.cameras, target, identity) for target in bpy.data.scenes for offered in self.held(identity, target) for holds in offered.first(Holds).to_list()
                    ).map(lambda _: None)
                )
            )

        return HELD.read(identity, scene).bind(released)

    # --- [REFRESH]

    def refreshed(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[bool]:
        """Separates per-entry groups, places missing pinned entries, and releases stale holds, answering whether an entry changed."""

        def separated(entry: bpy.types.SceneCompositorEffect, owner: Ownership[bpy.types.NodeTree], group: bpy.types.NodeTree) -> Outcome[bool]:
            copied: bpy.types.NodeTree = group.copy()  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return owner.owned(identity, copied, of=Some(scene)).bind(lambda owned: written(entry, "node_group", owned)).map(lambda _: True)

        def placed(pinned: Pinned, group: bpy.types.CompositorNodeTree) -> Outcome[bool]:
            with ExitStack() as rollback:
                entry = scene.compositor_effects.new(pinned.title)
                rollback.callback(scene.compositor_effects.remove, entry)
                return (
                    accumulated((written(entry, "node_group", group), *(enabled.descriptor.write(entry, enabled in pinned.enables) for enabled in Enabled)))
                    .bind(lambda _: relocated(scene, entry, 0 if pinned.edge is Edge.HEAD else len(scene.compositor_effects) - 1))
                    .map(lambda _: rollback.pop_all())
                    .map(lambda _: True)
                )

        def refreshed(_: bpy.types.Scene) -> Outcome[bool]:
            personal = tuple(
                (held.entry, owner, group)
                for held in self.owned(identity, scene)
                for owner in (ownerships(held.row.record)[1],)
                for group in Option.of_optional(held.entry.node_group).to_list()
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
                        HELD
                        .read(identity, scene)
                        .bind(lambda snapshots: accumulated(self.release(identity, scene, row) for row in self.rows if row.kind in snapshots and not row.entries(identity, scene)))
                        .map(lambda _: changed)
                    )
                )
            )

        return refreshed(scene)

    # --- [COMMANDS_AND_LIFETIME]

    def __init__(
        self,
        *,
        rows: tuple[CompositorEffect[R], ...],
        pinned: tuple[Pinned, ...] = (),
        rank: Option[Callable[[CompositorEffect[R]], int]] = Nothing,
        single: Callable[[CompositorEffect[R]], bool] = lambda _row: False,
        groups: Option[Callable[[CompositorEffect[R]], Enum]] = Nothing,
    ) -> None:
        """Declares entry edit commands over offered effects, ranked in declaration order unless `rank` orders them."""
        self.rows, self.pinned, self.single, self.groups = rows, pinned, single, groups
        self.rank = rank.default_value(rows.index)
        self.keyed = MappingProxyType({owner.key: row for row in rows for owner in ownerships(row.record)})

        def added(row: CompositorEffect[R]) -> Operator[Empty]:
            def run(call: Call[Empty]) -> Outcome[Answer]:
                """Add the effect at its rank, one entry per chosen file of an opened effect."""
                return (
                    call
                    .root(bpy.types.Scene)
                    .bind(
                        lambda scene: accumulated(
                            record.bind(lambda effect: self.placed(call.identity, scene, row, Entry(record=effect)))
                            for record in row.first(Opens).map(lambda opens: tuple(Ok(opens.of(str(path))) for path in call.paths)).default_with(lambda: (row.seeded(scene),))
                        )
                    )
                    .map(lambda _: Done())
                )

            run.__name__ = run.__qualname__ = f"add_{row.kind}"
            return Operator(form=Empty, run=run, invoke=row.first(Opens).map(lambda opens: Open(extensions=lambda _properties: opens.extensions())))

        def reset_effect(call: Call[Named]) -> Outcome[Answer]:
            """Reset effect to defaults."""
            return (
                call
                .root(bpy.types.Scene)
                .bind(
                    lambda scene: self.found(call.identity, scene, call.properties.name).bind(
                        lambda held: defaulted(call.identity, held.entry).bind(
                            lambda reset: held.row.seeded(scene).bind(lambda effect: held.row.write(call.identity, scene, held.entry, Entry(record=effect))).map(lambda changed: changed or reset)
                        )
                    )
                )
                .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.properties.name} holds defaults"))
            )

        def move_effect(call: Call[Shift]) -> Outcome[Answer]:
            """Move effect up, down, or to processing order."""
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: self.moved(call.identity, scene, call.properties.name, Toward[call.properties.toward]))
                .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.properties.name} holds requested position"))
            )

        def remove_effect(call: Call[Named]) -> Outcome[Answer]:
            """Remove compositor effect."""
            return call.root(bpy.types.Scene).bind(lambda scene: self.removed(call.identity, scene, call.properties.name)).map(lambda _: Done())

        self.operators = EffectOperators(
            moving=Operator(form=Shift, run=move_effect), removing=Operator(form=Named, run=remove_effect), resetting=Operator(form=Named, run=reset_effect), adding=tuple(map(added, rows))
        )

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields offered effects, commands, handlers, and subscriptions with deferred refresh, and the working-space follow-through of entry colors."""

        def refreshed(handle: Handle[bpy.types.Scene], groups: tuple[int, ...]) -> Outcome[Option[str]]:
            def reconciled(row: CompositorEffect[R], scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
                return row.read(identity, entry).bind(lambda record: row.write(identity, scene, entry, record))

            def update(scene: bpy.types.Scene) -> Outcome[Option[str]]:
                return (
                    Ok(Nothing)
                    if scene.session_uid in identity.held[EXECUTING]
                    else self.refreshed(identity, scene).bind(
                        lambda changed: accumulated(
                            reconciled(held.row, scene, held.entry)
                            if held.row.imaged or any(group.session_uid in groups for group in Option.of_optional(held.entry.node_group).to_list())
                            else held.row.refreshed(identity, scene, held.entry)
                            for held in self.owned(identity, scene)
                        ).map(lambda outcomes: Some("Refresh Effects") if changed or any(outcomes) else Nothing)
                    )
                )

            return handle.resolve().bind(update)

        previous: Option[str] = Nothing

        def recolored(stamp: Stored[str]) -> Outcome[Option[str]]:
            nonlocal previous
            target = bpy.data.colorspace.working_space

            def scene_recolored(scene: bpy.types.Scene, start: str) -> Outcome[bool]:
                def converted(cpu: PyOpenColorIO.CPUProcessor) -> Outcome[bool]:
                    held = tuple(
                        (owner, np.asarray(descriptor.read(owner), dtype=np.float32))
                        for entry in scene.compositor_effects
                        for group in Option.of_optional(entry.node_group).to_list()
                        for item in group.interface.items_tree  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                        if isinstance(item, bpy.types.NodeTreeInterfaceSocket) and item.in_out == "INPUT"
                        for owner in interface_input(entry, item.identifier).to_option().to_list()
                        for descriptor in Descriptor.resolve(owner, "value", bpy.types.FloatProperty).to_option().to_list()
                        if descriptor.prop.subtype == "COLOR"
                    )
                    for _, values in held:
                        cpu.applyRGB(values[:3])
                    return accumulated(written(owner, "value", tuple(values.tolist())).map(Option.is_some) for owner, values in held).map(any)

                return (
                    processor(PyOpenColorIO.GetCurrentConfig(), PyOpenColorIO.ColorSpaceTransform(src=start, dst=target))
                    .map(PyOpenColorIO.Processor.getDefaultCPUProcessor)
                    .bind(converted)
                    .bind(lambda wrote: stamp.write(scene, target).map(lambda _: wrote))
                )

            def each(scene: bpy.types.Scene) -> Outcome[bool]:
                start = stamp.read(scene).map(lambda held: held.or_else(previous).default_value(target))
                return start.bind(lambda source: Ok(False) if source == target else scene_recolored(scene, source))

            counted = accumulated(each(scene) for scene in bpy.data.scenes)
            previous = Some(target)
            return counted.map(lambda wrote: Some("Convert Effect Colors") if any(wrote) else Nothing)

        with ExitStack() as scope:
            defer = scope.enter_context(identity.reports.deferred(refreshed))

            def rendered(scene: bpy.types.Scene, source: Option[bpy.types.Scene] = Nothing) -> Outcome[None]:
                return (
                    Ok(None)
                    if scene.session_uid in identity.held[EXECUTING]
                    else accumulated(
                        source
                        .map(lambda evaluated: held.row.named(identity, evaluated, held.entry.name))
                        .default_value(Ok(held.entry))
                        .bind(lambda reading: held.row.read(identity, reading))
                        .bind(lambda record: held.row.write(identity, target, held.entry, record, source=source))
                        if source.is_some() or held.row.imaged
                        else held.row.refreshed(identity, target, held.entry)
                        for target in (scene,)
                        for held in self.owned(identity, target)
                    ).map(lambda _: None)
                )

            def loaded() -> Outcome[None]:
                return accumulated(
                    self.refreshed(identity, scene).bind(lambda _: rendered(scene)).bind(lambda _: accumulated(hold(identity, scene, row) for row in self.held(identity, scene)))
                    for scene in bpy.data.scenes
                ).map(lambda _: None)

            def imaged(scene: bpy.types.Scene, images: frozenset[bpy.types.Image], trees: tuple[bpy.types.ID, ...] = ()) -> None:
                users = bpy.data.user_map(subset=images, value_types={"NODETREE"}) if images else {}
                defer(Handle.of(scene), tuple(dict.fromkeys(tree.session_uid for tree in (*trees, *(tree for held in users.values() for tree in held)))))

            def edited(scene: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                imaged(
                    scene,
                    frozenset(image for update in updates if isinstance(update.id, bpy.types.Image) for image in (update.id.original,) if isinstance(image, bpy.types.Image)),
                    tuple(update.id for update in updates if isinstance(update.id, bpy.types.CompositorNodeTree)),
                )
                return Ok(None)

            def retagged() -> Outcome[None]:
                for scene in bpy.data.scenes:
                    imaged(scene, frozenset(bpy.data.images))
                return Ok(None)

            def invalidated(_scene: bpy.types.Scene) -> Outcome[None]:
                for image in bpy.data.images:
                    if image.type in {"RENDER_RESULT", "COMPOSITING"}:
                        produced(identity, image, Nothing)
                return Ok(None)

            def followed(stamp: Stored[str]) -> Acquisitions:
                later = scope.enter_context(identity.reports.deferred(lambda: recolored(stamp)))

                def seeded() -> Outcome[None]:
                    nonlocal previous
                    previous = Some(bpy.data.colorspace.working_space)
                    return Ok(None)

                def checked(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                    target = bpy.data.colorspace.working_space
                    return stamp.read(scene).map(lambda held: None if held.or_else(previous).default_value(target) == target else later())

                return (*startup(seeded), Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.ID,), checked)))

            def built(frame: Derives[Case], stamp: Stored[str]) -> Acquisitions:
                operators = self.operators
                return (
                    PRODUCTIONS,
                    *self.rows,
                    operators.moving,
                    operators.removing,
                    operators.resetting,
                    *operators.adding,
                    *(
                        Interchange(key=row.kind, title=row.title, rows=Importing(operator=operator, extensions=opens.extensions(), drop=frozenset({"NODE_EDITOR", "IMAGE_EDITOR"})))
                        for row, operator in zip(self.rows, operators.adding)
                        for opens in row.first(Opens).to_list()
                    ),
                    Handler(handlers=bpy.app.handlers.load_post, body=loaded),
                    Handler(handlers=bpy.app.handlers.render_init, body=invalidated),
                    Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Scene, bpy.types.Action, bpy.types.CompositorNodeTree, bpy.types.Image), edited)),
                    Handler(handlers=bpy.app.handlers.frame_change_post, body=lambda scene, graph: rendered(scene, Some(graph.scene_eval))),
                    *(
                        Subscription(owner=owner, setting=setting, body=lambda: accumulated(rendered(scene) for scene in bpy.data.scenes).map(lambda _: None))
                        for owner, setting in dict.fromkeys((*frame.reads, *(read for row in self.rows for read in row.reads)))
                    ),
                    *(Subscription(owner=bpy.types.Image, setting=bpy.types.Image.bl_rna.properties[name], body=retagged) for name in ("alpha_mode", "use_half_precision")),
                    *followed(stamp),
                )

            yield paired(framing(identity), identity.identifier("working_space")).map(lambda pair: built(pair[0], Stored(name=pair[1], decoder=msgspec.msgpack.Decoder(str))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ENTRY_EDITS]


def hold[R: Case](identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[R]) -> Outcome[None]:
    """Captures the scene members the effect's holds assign and applies its scene holds once, then applies its camera holds."""

    def held(holds: Holds, values: dict[Key, Snapshot]) -> Outcome[None]:
        first = (
            Snapshot.capture(scene, Some(holds.scene.selection(scene))).bind(lambda prior: HELD.written(identity, scene, values | {row.kind: prior})).bind(lambda _: holds.scene.apply(scene).outcome)
        )
        return (Ok(None) if row.kind in values else first).bind(lambda _: cameras.required(holds.cameras, scene, identity))

    return row.first(Holds).map(lambda holds: HELD.read(identity, scene).bind(lambda values: held(holds, values))).default_value(Ok(None))


def relocated(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, index: int) -> Outcome[bool]:
    """Moves entry to stack index unless it sits there, answering whether it moved."""
    return (
        Ok(False)
        if scene.compositor_effects.find(entry.name) == index
        else called(bpy.ops.scene.move_compositor_effect_to_index, {"name": entry.name, "index": index}, override=Some(bpy.context.temp_override(scene=scene))).map(lambda _: True)
    )


# --- [RECIPES]


def projected[R: Case](
    current: Mapping[EffectName, Configured[R]], recipe: Mapping[EffectName, Configured[R]], *, locked: frozenset[EffectName] = frozenset(), fields: Mapping[EffectName, frozenset[Key]] = {}
) -> dict[EffectName, Configured[R]]:
    """Merges sparse recipe entries and order into current ones, keeping locked entries whole and, on the others, the input values and enables at their locked keys."""

    def kept(prior: Configured[R], value: Configured[R], keys: frozenset[Key]) -> Configured[R]:
        owners = {key: owner for key, owner in value.values.tree.rows if key and rna(key) not in keys} | {key: owner for key, owner in prior.values.tree.rows if key and rna(key) in keys}
        return msgspec.structs.replace(
            value,
            values=regrouped(value.values, owners),
            render=prior.render if Enabled.RENDER.value.identifier in keys else value.render,
            preview=prior.preview if Enabled.PREVIEW.value.identifier in keys else value.preview,
        )

    changed = {name: kept(current[name], value, fields.get(name, frozenset())) if name in current else value for name, value in recipe.items() if name not in locked}
    ordered = iter(name for name in changed if name in current)
    return {selected: changed[selected] if selected in changed else current[selected] for name in current for selected in (next(ordered) if name in changed else name,)} | {
        name: value for name, value in changed.items() if name not in current
    }


# --- [EXECUTION]


@contextmanager
def capturing(
    identity: Identity, scene: bpy.types.Scene, before: bpy.types.SceneCompositorEffect, output: tuple[Expr, ...], *, stop: bool = False
) -> Generator[Outcome[bpy.types.SceneCompositorEffect]]:
    """Places scoped pass-through capture entry before the scene's entry, holding the output roots beside its pass-through."""
    active = next((entry for entry in scene.compositor_effects if entry.is_active), None)
    at = tuple(scene.compositor_effects).index(before)
    face = Interface(inputs=(Socket(key=Shell.image, name="Image", kind=bpy.types.NodeSocketColor),), outputs=(Socket(key=Shell.result, name="Image", kind=bpy.types.NodeSocketColor),))
    with ExitStack() as scope:
        if active is not None:
            scope.callback(setattr, active, "is_active", True)

        def placed(tree: bpy.types.NodeTree) -> Outcome[bpy.types.SceneCompositorEffect]:
            scope.callback(bpy.data.node_groups.remove, tree)
            entry = scene.compositor_effects.new("Input Capture")
            scope.callback(scene.compositor_effects.remove, entry)
            return written(entry, "node_group", tree).bind(lambda _: relocated(scene, entry, at)).map(lambda _: entry)

        suffix = tuple(scene.compositor_effects)[at:] if stop else ()
        for entry in suffix:
            scope.callback(setattr, entry, "enable_for_render", entry.enable_for_render)
        yield (
            composites
            .check(bpy.context, Some(scene))
            .bind(lambda _: accumulated(written(entry, "enable_for_render", False) for entry in suffix))
            .bind(lambda _: NodeGroup.built("input_capture", "Input Capture", face, {Shell.result: group_input(Shell.image, "RGBA").keeping(*output)}, bpy.types.CompositorNodeTree))
            .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
            .bind(lambda group: made(group, identity, of=Some(scene)))
            .bind(placed)
        )


# --- [COMPOSITION] ----------------------------------------------------------------------

NAME: Final = Descriptor.declared(bpy.types.SceneCompositorEffect, "name", bpy.types.StringProperty)
composites: Final = Setting(descriptor=Descriptor.declared(bpy.types.Scene, ("render", "use_compositing"), bpy.types.BoolProperty))
EXECUTING: Final = Resource[set[int]](made=lambda _identity: nullcontext(set()))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EXECUTING", "NAME", "Configured", "Edge", "EffectOperators", "EffectRows", "Owned", "Pinned", "Shift", "Toward", "capturing", "composites", "hold", "projected", "relocated"]
