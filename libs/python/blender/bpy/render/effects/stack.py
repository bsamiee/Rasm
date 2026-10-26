"""Effect stack: offered rows over scene compositor entries, placement, recipes, refresh, and the commands and handlers keeping entries current."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack, nullcontext
from enum import auto, Enum
from functools import reduce
import inspect
from operator import or_
from types import MappingProxyType
from typing import Annotated, Final, Generic, TypeVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from typing_extensions import TypeForm

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Empty, Open, Operator, Outcome, Unchanged
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle, named, vacant
from blender.bpy.data.rna.animation import Animation, Keys, sampled
from blender.bpy.data.rna.conditions import check, Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import FrameHandler, LOAD_POST, PathHandler, SceneHandler, UpdateHandler
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderCollisionFault, BlenderEditFault, BlenderResult
from blender.bpy.files.interchange import Importing, Interchange
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.graphs import Expr, group_input, NodeGroup
from blender.bpy.nodes.interface import Interface, item_key, Socket
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.effects.formation import followed, formation, opening, viewed
from blender.bpy.render.effects.records import entry_input, HELD, path
from blender.bpy.render.effects.rows import CompositorEffect, Context, Derived, editable, EffectName, Enabled, entries, framing, Marks, Named, ownerships, Shell
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.results import produced, PRODUCTIONS
from blender.bpy.scene import cameras
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.surfaces.menus import Menu, Submenus
from blender.imaging.chain import Effect, Stage
from blender.imaging.color.processors import bracketed, DisplayView, Spaced
from blender.imaging.compositing.blending import Blend, Entry
from blender.imaging.effects.tone.exposure import Exposure
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Choice
from blender.imaging.sampling import Precision

# --- [TYPES] ----------------------------------------------------------------------------

_E_co = TypeVar("_E_co", bound=Effect, default=Effect, covariant=True)


class Edge(Enum):
    """Stack end holding unranked pinned group."""

    HEAD = auto()
    TAIL = auto()


class Toward(Choice):
    """Move target, neighbor or processing rank."""

    UP = 1, "Up", "Move effect up the stack"
    DOWN = 2, "Down", "Move effect down the stack"
    RANK = 3, "Processing Order", "Move effect into processing order"


# --- [MODELS] ---------------------------------------------------------------------------


class Shift(Named, frozen=True, kw_only=True):
    """Named entry and requested move."""

    toward: Toward = Toward.RANK


class Pinned(msgspec.Struct, frozen=True, kw_only=True):
    """Unranked owned group with entry title, stack end, enables, and conditions."""

    ownership: Ownership[bpy.types.CompositorNodeTree]
    title: str
    edge: Edge
    enables: frozenset[Enabled] = frozenset(Enabled)
    conditions: Option[Callable[[Identity, bpy.types.SceneCompositorEffect], BlenderResult[dict[Key, Condition]]]] = Nothing


class Owned(msgspec.Struct, Generic[_E_co], frozen=True, kw_only=True):
    """Entry whose group carries the ownership mark of an offered row."""

    entry: bpy.types.SceneCompositorEffect
    row: CompositorEffect[_E_co]


class Configured(msgspec.Struct, Generic[_E_co], frozen=True, kw_only=True):
    """Effect record with render and preview enables."""

    record: Entry[_E_co]
    render: Annotated[bool, Enabled.RENDER]
    preview: Annotated[bool, Enabled.PREVIEW]


class EffectOperators(msgspec.Struct, frozen=True, kw_only=True):
    """Stack edit commands menus and shortcuts call."""

    moving: Operator[Shift]
    removing: Operator[Named]
    resetting: Operator[Named]
    adding: tuple[Operator[Empty], ...]


# --- [SERVICES] -------------------------------------------------------------------------


class EffectStack[E: Effect = Effect]:
    """Offered effect rows, pinned groups, and the exposure row the formation folds scene exposure into."""

    effects: tuple[CompositorEffect[E], ...]
    pinned: tuple[Pinned, ...]
    gain: Option[CompositorEffect[Exposure]]
    operators: EffectOperators
    precision: Callable[[Identity], BlenderResult[Precision]]
    keyed: Mapping[Key, CompositorEffect[E | DisplayView]]

    # --- [OFFERED_ROWS]

    @property
    def rows(self) -> tuple[CompositorEffect[E | DisplayView], ...]:
        """Formation row followed by offered rows."""
        return (formation, *self.effects)

    @property
    def offered(self) -> TypeForm[E]:
        """Union of offered row record types."""
        return reduce(or_, (row.record for row in self.effects))

    @property
    def form(self) -> TypeForm[E | DisplayView]:
        """Union of row record types."""
        return reduce(or_, (row.record for row in self.rows))

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Frame size attachment camera frame derivation binds, and the cell of scenes under execution."""
        return (FrameSize.attachment, EXECUTING)

    @property
    def standalone(self) -> tuple[CompositorEffect[E], ...]:
        """Rows needing neither render pass request nor file."""
        return tuple(row for row in self.effects if not row.requests and row.opened.is_none())

    def config(self) -> BlenderResult[PyOpenColorIO.Config]:
        """Returns live OCIO config resolving row default process spaces, declared as class values or field defaults."""
        name = Spaced.space.__name__
        return config_of(
            tuple(
                dict.fromkeys(
                    value
                    for row in self.rows
                    if isinstance(row.record, Spaced)
                    for value in ({field.name: field.default for field in msgspec.structs.fields(row.record)}.get(name, inspect.getattr_static(row.record, name)),)
                    if isinstance(value, str)
                )
            )
        )

    def offering(self, kind: type[Effect]) -> BlenderResult[CompositorEffect[E | DisplayView]]:
        """Finds row of exact effect type."""
        return Option.of_optional(next((row for row in self.rows if row.record is kind), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(tuple(row.kind for row in self.rows)), key=item_key(kind)),)
        )

    def menu(self, key: Key, title: str, description: str) -> Menu:
        """Builds add menu of every row, one submenu per imaging phase, rows in stage order."""
        return Menu(
            key=key,
            title=title,
            description=description,
            body=Submenus(
                entries=tuple(
                    (row.record.stage.phase, OperatorButton(target=operator, text=row.record.title))
                    for row, operator in sorted(zip(self.rows, self.operators.adding), key=lambda pair: pair[0].record.stage)
                )
            ),
        )

    # --- [ENTRIES]

    def owned(self, identity: Identity, scene: bpy.types.Scene) -> tuple[Owned[E | DisplayView], ...]:
        """Returns entries offered rows own, with their rows, in stack order."""
        return tuple(Owned(entry=entry, row=self.keyed[mark.key]) for entry, mark in Marks.of(identity, scene).held if mark.key in self.keyed)

    def row(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Option[CompositorEffect[E | DisplayView]]:
        """Returns the offered row owning entry's group."""
        return Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).bind(lambda mark: Option.of_optional(self.keyed.get(mark.key)))

    def entries(self, identity: Identity, scene: bpy.types.Scene, selection: Stage | type[Effect]) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns owned entries of stage or exact record type."""
        return tuple(held.entry for held in self.owned(identity, scene) if held.row.record.stage is selection or held.row.record is selection)

    def stages(self, identity: Identity, scene: bpy.types.Scene, enabled: Enabled = Enabled.RENDER) -> tuple[Owned[E | DisplayView], ...]:
        """Returns enabled staged entries with their rows in stack order."""
        return tuple(held for held in self.owned(identity, scene) if held.row.staged and enabled.descriptor.read(held.entry) is True)

    def found(self, identity: Identity, scene: bpy.types.Scene, name: str) -> BlenderResult[Owned[E | DisplayView]]:
        """Finds named owned entry, skipping user groups."""
        return Option.of_optional(next((held for held in self.owned(identity, scene) if held.entry.name == name), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=name),)
        )

    def held(self, identity: Identity, scene: bpy.types.Scene) -> tuple[CompositorEffect[E | DisplayView], ...]:
        """Returns rows holding an entry in the scene, in row order."""
        keys = frozenset(mark.key for _, mark in Marks.of(identity, scene).held)
        return tuple(row for row in self.rows if any(owner.key in keys for owner in ownerships(row.record)))

    # --- [RECIPES]

    def captured(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[dict[EffectName, Configured[E | DisplayView]]]:
        """Reads owned entry records and enables by name in stack order."""
        return accumulated(
            held.row.read(identity, held.entry).map(lambda record: (held.entry.name, Configured(record=record, render=held.entry.enable_for_render, preview=held.entry.enable_for_preview)))
            for held in self.owned(identity, scene)
        ).map(dict)

    def applied(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        recipe: Mapping[EffectName, Configured[Effect]],
        *,
        locked: frozenset[EffectName] = frozenset(),
        fields: Mapping[EffectName, frozenset[tuple[Key | int, ...]]] = {},
        initial_view: Option[DisplayView] = Nothing,
        context: Context = Nothing,
    ) -> BlenderResult[None]:
        """Applies sparse named records, keeping locked, unlisted, and user entries."""

        def applied(current: Mapping[EffectName, Configured[Effect]]) -> BlenderResult[None]:
            desired = projected(current, recipe, locked=locked, fields=fields)
            if not (requested := ({name: desired[name] for name in recipe if name not in locked})) and initial_view.is_none():
                return Ok(None)
            native = {entry.name: entry for entry in scene.compositor_effects}
            retired = tuple(dict.fromkeys(type(value.record.effect) for value in current.values() if not any(type(record.record.effect) is type(value.record.effect) for record in desired.values())))

            def checked(rows: tuple[CompositorEffect[E | DisplayView], ...]) -> BlenderResult[None]:
                def write(initial: Option[DisplayView], restores: tuple[tuple[CompositorEffect[E | DisplayView], tuple[Callable[[], BlenderResult[None]], ...]], ...]) -> BlenderResult[None]:
                    order = tuple(native.values())
                    prior = next((entry for entry in order if entry.is_active), None)

                    def written_entry(
                        name: EffectName, value: Configured[Effect], row: CompositorEffect[E | DisplayView]
                    ) -> BlenderResult[tuple[bpy.types.SceneCompositorEffect, CompositorEffect[E | DisplayView]]]:
                        placing = Ok(None) if any(isinstance(held.record.effect, row.record) for held in current.values()) else row.placing(identity, scene, self)
                        return placing.bind(
                            lambda _: (
                                Option
                                .of_optional(native.get(name))
                                .map(lambda entry: row.write(identity, scene, entry, value.record, context=context).map(lambda _: entry))
                                .default_with(lambda: row.appended(identity, scene, value.record, context=context, name=Some(name)))
                            )
                        ).bind(lambda entry: paired(written(entry, "enable_for_render", value.render), written(entry, "enable_for_preview", value.preview)).map(lambda _: (entry, row)))

                    def arranged(placed: tuple[tuple[bpy.types.SceneCompositorEffect, CompositorEffect[E | DisplayView]], ...]) -> BlenderResult[None]:
                        live = dict(zip(requested, (entry for entry, _ in placed)))
                        selected = iter(native.get(name, live[name]) if name in live else native[name] for name in desired)
                        target = tuple(next(selected) if entry.name in current else entry for entry in order) + tuple(selected)
                        kept = frozenset(native[name] for name in current) | frozenset(live.values())
                        return (
                            accumulated(self.release(identity, scene, row).bind(lambda _: accumulated(step() for step in steps)) for row, steps in restores)
                            .bind(lambda _: accumulated(hold(identity, scene, row) for _, row in placed))
                            .bind(lambda _: accumulated(relocated(scene, entry, index) for index, entry in enumerate(target) if entry in kept))
                            .bind(lambda _: initial.map(lambda view: self.placed(identity, scene, formation, Entry(effect=view), context=context).map(lambda _: None)).default_value(Ok(None)))
                            .map(lambda _: None)
                        )

                    with ExitStack() as scope, bpy.context.temp_override(scene=scene):
                        if prior is not None:
                            scope.callback(setattr, prior, "is_active", True)
                        return accumulated(written_entry(name, value, row) for (name, value), row in zip(requested.items(), rows)).bind(arranged)

                seeded = next((seed for row in rows for seed in opening(row.record).to_list()), None)
                initial = (
                    initial_view.map(lambda view: Ok(Some(view))).default_with(lambda: self.config().bind(lambda config: seeded(config, scene)).map(Some) if seeded is not None else Ok(Nothing))
                    if not formation.entries(identity, scene) and not any(isinstance(value.record.effect, DisplayView) for value in desired.values())
                    else Ok(Nothing)
                )
                restores = accumulated(
                    self.offering(kind).bind(
                        lambda row: accumulated(row.removing(identity, scene, native[name]) for name, value in current.items() if type(value.record.effect) is kind).map(lambda steps: (row, steps))
                    )
                    for kind in retired
                )
                return paired(initial, restores).bind(lambda parts: write(*parts))

            names = accumulated(
                Descriptor.validated(NAME.prop, name).bind(
                    lambda _: Error((BlenderCollisionFault(request=name, error=repr(scene.compositor_effects[name])),)) if name not in current and name in scene.compositor_effects else Ok(None)
                )
                for name in requested
            )
            return paired(names, accumulated(self.offering(type(value.record.effect)) for value in requested.values())).bind(lambda pair: checked(pair[1]))

        return editable(scene).bind(lambda _: self.captured(identity, scene)).bind(applied)

    # --- [PLACEMENT]

    def placed(self, identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[Effect], record: Entry[Effect], *, context: Context = Nothing) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Places entry at rank, applying first-entry settings, restoring active entry."""
        prior = Option.of_optional(next((entry for entry in scene.compositor_effects if entry.is_active), None))
        existing = Option.of_optional(next(iter(self.entries(identity, scene, row.record.stage)), None)) if row.record.stage.single else Nothing

        def added() -> BlenderResult[bpy.types.SceneCompositorEffect]:
            seeding = (
                opening(row.record)
                .map(
                    lambda seed: (
                        self.config().bind(lambda config: seed(config, scene)).bind(lambda view: self.placed(identity, scene, formation, Entry(effect=view), context=context)).map(lambda _: None)
                    )
                )
                .default_value(Ok(None))
                if not formation.entries(identity, scene)
                else Ok(None)
            )
            return (
                seeding
                .bind(lambda _: row.placing(identity, scene, self) if not row.entries(identity, scene) else Ok(None))
                .bind(lambda _: row.appended(identity, scene, record, context=context))
                .bind(
                    lambda entry: (
                        hold(identity, scene, row)
                        .bind(lambda _: self.moved(identity, scene, Shift(name=entry.name)))
                        .bind(lambda _: prior.map(lambda active: written(active, "is_active", True).map(lambda _: None)).default_value(Ok(None)))
                        .map(lambda _: entry)
                    )
                )
            )

        return editable(scene).bind(lambda _: existing.map(Ok).default_with(added))

    def assigned(self, identity: Identity, scene: bpy.types.Scene, effect: Effect, *, context: Context = Nothing) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Writes earliest entry of effect type or places new one."""

        def assigned(row: CompositorEffect[Effect]) -> BlenderResult[bpy.types.SceneCompositorEffect]:
            return (
                Option
                .of_optional(next(iter(row.entries(identity, scene)), None))
                .map(lambda entry: row.read(identity, entry).bind(lambda prior: row.write(identity, scene, entry, msgspec.structs.replace(prior, effect=effect), context=context)).map(lambda _: entry))
                .default_with(lambda: self.placed(identity, scene, row, Entry(effect=effect), context=context))
            )

        return self.offering(type(effect)).bind(assigned)

    def moved(self, identity: Identity, scene: bpy.types.Scene, shift: Shift) -> BlenderResult[bool]:
        """Moves named owned entry alone, answering whether it moved."""

        def moved(held: Owned[Effect]) -> BlenderResult[bool]:
            order = tuple(scene.compositor_effects)
            position = order.index(held.entry)
            tail = frozenset(pinned.ownership.key for pinned in self.pinned if pinned.edge is Edge.TAIL)
            above = frozenset(entry for entry, mark in Marks.of(identity, scene).held if mark.key in tail) | frozenset(
                owned.entry for owned in self.owned(identity, scene) if owned.row.record.stage > held.row.record.stage
            )
            match shift.toward:
                case Toward.UP:
                    index = max(0, position - 1)
                case Toward.DOWN:
                    index = min(len(order) - 1, position + 1)
                case Toward.RANK:
                    others = tuple(entry for entry in order if entry != held.entry)
                    index = next((index for index, entry in enumerate(others) if entry in above), len(others))
            return relocated(scene, held.entry, index)

        return editable(scene).bind(lambda _: self.found(identity, scene, shift.name)).bind(moved)

    def removed(self, identity: Identity, scene: bpy.types.Scene, name: str) -> BlenderResult[None]:
        """Removes owned entry, unused groups, and last-entry row holds, then runs the row's restoring step."""

        def removed(held: Owned[Effect]) -> BlenderResult[None]:
            row = held.row
            return (
                row
                .removing(identity, scene, held.entry)
                .bind(
                    lambda restoring: (
                        BlenderEditFault
                        .attempted(repr(scene), lambda: scene.compositor_effects.remove(held.entry))
                        .bind(lambda _: self.release(identity, scene, row) if not row.entries(identity, scene) else Ok(None))
                        .bind(lambda _: restoring())
                    )
                )
                .bind(lambda _: accumulated(owner.removed(identity) for owner in ownerships(row.record)))
                .map(lambda _: None)
            )

        return editable(scene).bind(lambda _: self.found(identity, scene, name)).bind(removed)

    # --- [SCENE_SETTINGS]

    def release(self, identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[Effect]) -> BlenderResult[None]:
        """Restores held scene settings, reapplying camera requirements of remaining rows."""

        def released(held: dict[Key, Snapshot]) -> BlenderResult[None]:
            return (
                Option
                .of_optional(held.get(row.kind))
                .map(lambda snapshot: snapshot.apply(scene).outcome)
                .default_value(Ok(None))
                .bind(lambda _: HELD.written(identity, scene, {key: value for key, value in held.items() if key != row.kind}))
                .bind(lambda _: cameras.released(identity))
                .bind(
                    lambda _: accumulated(
                        cameras.required(Snapshot.of(bpy.types.Camera, dict(offered.cameras)), target, identity) for target in bpy.data.scenes for offered in self.held(identity, target)
                    ).map(lambda _: None)
                )
            )

        return HELD.read(identity, scene).bind(released)

    # --- [REFRESH]

    def refreshed(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[bool]:
        """Separates per-entry groups, places missing pinned entries, and releases stale holds, answering whether an entry changed."""

        def separated(entry: bpy.types.SceneCompositorEffect, row: CompositorEffect[Effect], owner: Ownership[bpy.types.NodeTree], group: bpy.types.NodeTree) -> BlenderResult[bool]:
            label = vacant(row.record.title, bpy.data.node_groups.keys())
            copied: bpy.types.NodeTree = group.copy()  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return named(copied, label).bind(lambda labelled: owner.owned(identity, labelled, of=Some(scene))).bind(lambda owned: written(entry, "node_group", owned)).map(lambda _: True)

        def placed(pinned: Pinned, group: bpy.types.CompositorNodeTree) -> BlenderResult[bool]:
            with ExitStack() as rollback:
                entry = scene.compositor_effects.new(vacant(pinned.title, (held.name for held in scene.compositor_effects)))
                rollback.callback(scene.compositor_effects.remove, entry)
                return (
                    accumulated((written(entry, "node_group", group), *(enabled.descriptor.write(entry, enabled in pinned.enables) for enabled in Enabled)))
                    .bind(lambda _: relocated(scene, entry, 0 if pinned.edge is Edge.HEAD else len(scene.compositor_effects) - 1))
                    .map(lambda _: rollback.pop_all())
                    .map(lambda _: True)
                )

        def refreshed(_: bpy.types.Scene) -> BlenderResult[bool]:
            personal = tuple(
                (held.entry, held.row, owner, group)
                for held in self.owned(identity, scene)
                for owner in (ownerships(held.row.record)[1],)
                for group in Option.of_optional(held.entry.node_group).to_list()
                if Mark.read(identity, group).filter(lambda mark: mark.key == owner.key).is_some()
            )
            keeper = {group.session_uid: entry for entry, _, _, group in reversed(personal)}
            absent = (
                (pinned, group)
                for pinned in self.pinned
                for group in Option.of_optional(next((group for group, mark in pinned.ownership.held(identity) if mark.of == Some(scene)), None)).to_list()
                if not any(entry.node_group == group for entry in scene.compositor_effects)
            )
            return (
                reduce(
                    lambda done, shared: done.bind(lambda _: separated(*shared)), ((entry, row, owner, group) for entry, row, owner, group in personal if keeper[group.session_uid] != entry), Ok(False)
                )
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

        return editable(scene).bind(refreshed)

    def pointwise(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Returns owned entry when its effect is pointwise."""
        return self.found(identity, scene, entry.name).bind(lambda held: held.row.read(identity, entry)).bind(lambda record: local_entry(entry, record))

    def forming(self, identity: Identity, scene: bpy.types.Scene, config: PyOpenColorIO.Config, enabled: Enabled) -> BlenderResult[Option[PyOpenColorIO.Transform]]:
        """Groups enabled entry transforms, closed by scene view absent Display View, Nothing when an entry has none."""
        owned = {held.entry: held.row for held in self.owned(identity, scene)}

        def transformed(entry: bpy.types.SceneCompositorEffect, record: Entry[Effect]) -> BlenderResult[Option[PyOpenColorIO.Transform]]:
            blend: BlenderResult[None] = Ok(None) if record.blend == Blend() else Error((BlenderChoiceFault(path=entry.name, identifier=repr(record.blend), accepted=(repr(Blend()),)),))
            return paired(local_entry(entry, record), blend).bind(lambda _: bracketed(record.effect, config) if isinstance(record.effect, Spaced) else record.effect.transform(config))

        operations = accumulated(
            Option
            .of_optional(owned.get(entry))
            .map(lambda row: row.read(identity, entry).bind(lambda record: transformed(entry, record)))
            .default_value(Error((BlenderAbsentFault(owner=repr(scene.compositor_effects), key=entry.name),)))
            for entry in scene.compositor_effects
            if enabled.descriptor.read(entry) is True
        )
        closing = viewed(scene).transform(config).map(lambda operation: (operation,)) if not formation.entries(identity, scene) else Ok(())
        return paired(operations, closing).map(
            lambda pair: (
                Some(PyOpenColorIO.GroupTransform([operation for optional in (*pair[0], *pair[1]) for operation in optional.to_list()]))
                if all(operation.is_some() for operation in (*pair[0], *pair[1]))
                else Nothing
            )
        )

    # --- [COMMANDS_AND_LIFETIME]

    def __init__(
        self,
        *,
        effects: tuple[CompositorEffect[E], ...],
        precision: Callable[[Identity], BlenderResult[Precision]] = lambda _identity: Ok(Precision()),
        pinned: tuple[Pinned, ...] = (),
        gain: Option[CompositorEffect[Exposure]] = Nothing,
    ) -> None:
        """Declares stack edit commands over offered rows."""
        self.effects, self.precision, self.pinned, self.gain = effects, precision, pinned, gain
        self.keyed = MappingProxyType({owner.key: row for row in self.rows for owner in ownerships(row.record)})

        def added(row: CompositorEffect[E | DisplayView]) -> Operator[Empty]:
            def run(call: Call[Empty]) -> BlenderResult[Outcome]:
                return (
                    call
                    .root(bpy.types.Scene)
                    .bind(
                        lambda scene: accumulated(
                            record.bind(lambda effect: self.placed(call.identity, scene, row, Entry(effect=effect)))
                            for record in row.opened.map(lambda opened: tuple(Ok(opened.of(str(path))) for path in call.paths)).default_with(lambda: (row.seeded(scene),))
                        )
                    )
                    .map(lambda _: Done())
                )

            return Operator(
                key=f"effect_add_{row.kind}",
                title=f"Add {row.record.title}",
                description=row.description,
                form=Empty,
                run=run,
                invoke=row.opened.map(lambda opened: Open(extensions=lambda _record: opened.extensions())),
            )

        def reset(call: Call[Named]) -> BlenderResult[Outcome]:
            return (
                call
                .root(bpy.types.Scene)
                .bind(
                    lambda scene: self.found(call.identity, scene, call.record.name).bind(
                        lambda held: held.row.seeded(scene).bind(lambda effect: held.row.write(call.identity, scene, held.entry, Entry(effect=effect)))
                    )
                )
                .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.record.name} holds defaults"))
            )

        self.operators = EffectOperators(
            moving=Operator(
                key="effect_move",
                title="Move Effect",
                description="Move effect up, down, or to processing order",
                form=Shift,
                run=lambda call: (
                    call
                    .root(bpy.types.Scene)
                    .bind(lambda scene: self.moved(call.identity, scene, call.record))
                    .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.record.name} holds requested position"))
                ),
            ),
            removing=Operator(
                key="effect_remove",
                title="Remove Effect",
                description="Remove compositor effect",
                form=Named,
                run=lambda call: call.root(bpy.types.Scene).bind(lambda scene: self.removed(call.identity, scene, call.record.name)).map(lambda _: Done()),
            ),
            resetting=Operator(key="effect_reset", title="Reset Effect", description="Reset effect to defaults", form=Named, run=reset),
            adding=tuple(map(added, self.rows)),
        )

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Yields stack registrables, handlers, and subscriptions with deferred refresh."""

        def refreshed(handle: Handle[bpy.types.Scene], groups: tuple[int, ...]) -> BlenderResult[Option[str]]:
            def update(scene: bpy.types.Scene) -> BlenderResult[Option[str]]:
                if scene.session_uid in EXECUTING.of(identity):
                    return Ok(Nothing)

                def reconciled(row: CompositorEffect[Effect], entry: bpy.types.SceneCompositorEffect) -> BlenderResult[bool]:
                    match row.read(identity, entry):
                        case Result(tag="ok", ok=record):
                            return row.write(identity, scene, entry, record)
                        case Result(error=faults):
                            restored = row.read(identity, entry, edited=False).bind(lambda stored: row.write(identity, scene, entry, stored))
                            return Error((*faults, *restored.swap().default_value(())))

                return self.refreshed(identity, scene).bind(
                    lambda changed: accumulated(
                        reconciled(held.row, held.entry)
                        if held.row.imaged or any(group.session_uid in groups for group in Option.of_optional(held.entry.node_group).to_list())
                        else held.row.refreshed(identity, scene, held.entry)
                        for held in self.owned(identity, scene)
                    ).map(lambda outcomes: Some("Refresh Effects") if changed or any(outcomes) else Nothing)
                )

            return handle.resolve().bind(update)

        with identity.reports.deferred(refreshed) as defer:

            def loaded(_path: object) -> BlenderResult[None]:
                return accumulated(
                    self.refreshed(identity, scene).bind(lambda _: rendered(scene)).bind(lambda _: accumulated(hold(identity, scene, row) for row in self.held(identity, scene)))
                    for source in bpy.data.scenes
                    for scene in editable(source).to_option().to_list()
                ).map(lambda _: None)

            def updated(scene: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> BlenderResult[None]:
                for held in editable(scene).to_option().to_list():
                    defer(Handle.of(held), tuple(update.id.session_uid for update in updates if isinstance(update.id, bpy.types.CompositorNodeTree)))
                return Ok(None)

            def rendered(scene: bpy.types.Scene, source: Option[bpy.types.Scene] = Nothing) -> BlenderResult[None]:
                if scene.session_uid in EXECUTING.of(identity):
                    return Ok(None)
                return accumulated(
                    source
                    .map(lambda evaluated: held.row.named(identity, evaluated, held.entry.name))
                    .default_value(Ok(held.entry))
                    .bind(lambda reading: held.row.read(identity, reading))
                    .bind(lambda record: held.row.write(identity, target, held.entry, record, source=source))
                    if source.is_some() or held.row.imaged
                    else held.row.refreshed(identity, target, held.entry)
                    for target in editable(scene).to_option().to_list()
                    for held in self.owned(identity, target)
                ).map(lambda _: None)

            def invalidated(_scene: bpy.types.Scene) -> BlenderResult[None]:
                for image in bpy.data.images:
                    if image.type in {"RENDER_RESULT", "COMPOSITING"}:
                        produced(identity, image, Nothing)
                return Ok(None)

            def built(frame: Derived[Effect, msgspec.Struct]) -> Acquisitions:
                operators = self.operators
                return (
                    PRODUCTIONS,
                    *self.rows,
                    operators.moving,
                    operators.removing,
                    operators.resetting,
                    *operators.adding,
                    *(
                        Interchange(key=row.kind, title=row.record.title, rows=Importing(operator=operator, record=Empty(), drop=frozenset({"NODE_EDITOR", "IMAGE_EDITOR"})))
                        for row, operator in zip(self.rows, operators.adding)
                        if row.opened.is_some()
                    ),
                    PathHandler(handlers=LOAD_POST, body=loaded),
                    SceneHandler(handlers=bpy.app.handlers.render_init, body=invalidated),
                    UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene, bpy.types.Action, bpy.types.CompositorNodeTree), body=updated),
                    FrameHandler(handlers=bpy.app.handlers.frame_change_post, body=lambda scene, graph: rendered(scene, Some(graph.scene_eval))),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    *(
                        Subscription(owner=owner, setting=setting, body=lambda: accumulated(rendered(scene) for scene in bpy.data.scenes).map(lambda _: None))
                        for owner, setting in dict.fromkeys((*frame.reads, *(read for row in self.rows for read in row.reads)))
                    ),
                    Subscription(owner=bpy.types.ColorManagedDisplaySettings, setting=bpy.types.ColorManagedDisplaySettings.bl_rna.properties["display_device"], body=lambda: followed(identity)),
                )

            yield framing(identity).map(built)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ENTRY_EDITS]


def hold(identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[Effect]) -> BlenderResult[None]:
    """Captures and applies row holds once, then camera requirements and the row's own held state."""

    def held(values: dict[Key, Snapshot], descriptors: tuple[Descriptor, ...]) -> BlenderResult[None]:
        prior = Option.of_optional(values.get(row.kind))

        def captured(snapshot: Snapshot) -> BlenderResult[None]:
            first = HELD.written(identity, scene, values | {row.kind: snapshot}).bind(lambda _: Snapshot.of(bpy.types.Scene, dict(row.holds)).apply(scene).outcome)
            return (
                (first if prior.is_none() else Ok(None))
                .bind(lambda _: cameras.required(Snapshot.of(bpy.types.Camera, dict(row.cameras)), scene, identity))
                .bind(lambda _: row.holding(identity, scene))
                .map(lambda _: None)
            )

        return prior.map(Ok).default_with(lambda: Snapshot.capture(scene, Some(descriptors))).bind(captured)

    return paired(HELD.read(identity, scene), accumulated(Descriptor.resolve(bpy.types.Scene, path, bpy.types.Property) for path in row.holds)).bind(lambda pair: held(*pair))


def local_entry(entry: bpy.types.SceneCompositorEffect, record: Entry[Effect]) -> BlenderResult[bpy.types.SceneCompositorEffect]:
    """Returns entry when its effect is pointwise."""
    return Ok(entry) if record.effect.pointwise else Error((BlenderChoiceFault(path=entry.name, identifier="False", accepted=("True",)),))


def relocated(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, index: int) -> BlenderResult[bool]:
    """Moves entry to stack index unless it sits there, answering whether it moved."""
    return (
        Ok(False)
        if scene.compositor_effects.find(entry.name) == index
        else called(bpy.ops.scene.move_compositor_effect_to_index, {"name": entry.name, "index": index}, override=Some(bpy.context.temp_override(scene=scene))).map(lambda _: True)
    )


# --- [RECIPES]


def projected[E: Effect](
    current: Mapping[EffectName, Configured[E]],
    recipe: Mapping[EffectName, Configured[E]],
    *,
    locked: frozenset[EffectName] = frozenset(),
    fields: Mapping[EffectName, frozenset[tuple[Key | int, ...]]] = {},
) -> dict[EffectName, Configured[E]]:
    """Merges sparse recipe values and order into current records, keeping locks."""

    def kept(prior: object, value: object, selected: frozenset[tuple[Key | int, ...]]) -> object:
        if not selected:
            return value
        if () in selected or type(prior) is not type(value):
            return prior
        match value, prior:
            case msgspec.Struct(), msgspec.Struct():
                return msgspec.structs.replace(
                    value,
                    **{
                        field.name: kept(getattr(prior, field.name), getattr(value, field.name), frozenset(path[1:] for path in selected if path and path[0] == field.name))
                        for field in msgspec.structs.fields(value)
                    },
                )
            case tuple(), tuple():
                return (
                    prior
                    if len(value) != len(prior)
                    else tuple(kept(before, after, frozenset(path[1:] for path in selected if path and path[0] == index)) for index, (before, after) in enumerate(zip(prior, value), 1))
                )
            case _:
                return value

    changed = {
        name: msgspec.structs.replace(
            value,
            **{
                field.name: kept(getattr(current[name], field.name), getattr(value, field.name), frozenset(path[1:] for path in fields.get(name, frozenset()) if path and path[0] == field.name))
                for field in msgspec.structs.fields(value)
            },
        )
        if name in current
        else value
        for name, value in recipe.items()
        if name not in locked
    }
    ordered = iter(name for name in changed if name in current)
    return {selected: changed[selected] if selected in changed else current[selected] for name in current for selected in (next(ordered) if name in changed else name,)} | {
        name: value for name, value in changed.items() if name not in current
    }


# --- [EXECUTION]


@contextmanager
def capturing(identity: Identity, before: bpy.types.SceneCompositorEffect, output: tuple[Expr, ...], *, stop: bool = False) -> Generator[BlenderResult[bpy.types.SceneCompositorEffect]]:
    """Places scoped pass-through capture entry before entry, holding the output roots beside its pass-through."""
    scene: bpy.types.Scene = before.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    active = next((entry for entry in scene.compositor_effects if entry.is_active), None)
    at = tuple(scene.compositor_effects).index(before)
    face = Interface(inputs=(Socket(key=Shell.image, name="Image", kind=bpy.types.NodeSocketColor),), outputs=(Socket(key=Shell.result, name="Image", kind=bpy.types.NodeSocketColor),))
    with ExitStack() as scope:
        if active is not None:
            scope.callback(setattr, active, "is_active", True)

        def placed(tree: bpy.types.NodeTree) -> BlenderResult[bpy.types.SceneCompositorEffect]:
            scope.callback(bpy.data.node_groups.remove, tree)
            entry = scene.compositor_effects.new("Input Capture")
            scope.callback(scene.compositor_effects.remove, entry)
            return written(entry, "node_group", tree).bind(lambda _: relocated(scene, entry, at)).map(lambda _: entry)

        suffix = tuple(scene.compositor_effects)[at:] if stop else ()
        for entry in suffix:
            scope.callback(setattr, entry, "enable_for_render", entry.enable_for_render)
        yield (
            editable(scene)
            .bind(lambda _: check(composites, bpy.context, Some(scene)))
            .bind(lambda _: accumulated(written(entry, "enable_for_render", False) for entry in suffix))
            .bind(lambda _: NodeGroup.built("input_capture", "Input Capture", face, {Shell.result: group_input(Shell.image, "RGBA").keeping(*output)}))
            .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
            .bind(lambda group: made(group, identity, of=Some(scene)))
            .bind(placed)
        )


def keyed_product(
    identity: Identity,
    scene: bpy.types.Scene,
    entry: bpy.types.SceneCompositorEffect,
    source: type[Effect],
    source_key: Key,
    target_key: Key,
    transform: Callable[[npt.NDArray[np.float64]], npt.NDArray[np.float64]],
) -> BlenderResult[tuple[Animation, ...]]:
    """Keys product of later source gains weighted by opacity and render enable."""
    frames = range(scene.frame_start, scene.frame_end + 1)
    ordered = tuple(scene.compositor_effects)
    following = ordered[ordered.index(entry) + 1 :]

    def samples(held: bpy.types.SceneCompositorEffect) -> BlenderResult[tuple[tuple[float, ...], ...]]:
        _, opacity, _ = Blend.__struct_fields__
        return paired(path(identity, held, source_key), path(identity, held, item_key(Shell.blend, opacity))).bind(
            lambda paths: accumulated(sampled(scene, address, frames) for address in (*paths, held.path_from_id(Enabled.RENDER.descriptor.prop.identifier)))
        )

    def formed(parts: tuple[str, tuple[tuple[tuple[float, ...], ...], ...]]) -> tuple[Animation, ...]:
        address, sources = parts
        values, opacity, enabled = np.asarray(sources, np.float64).reshape(-1, 3, len(frames)).transpose(1, 0, 2)
        gains = np.prod(1.0 + opacity * enabled * (transform(values) - 1.0), axis=0)
        return (Animation(struct=address, member="value", source=Keys(points=tuple(zip(map(float, frames), map(float, gains))))),)

    return paired(entry_input(identity, entry, target_key).map(lambda held: held.path_from_id()), accumulated(samples(held) for held in entries(identity, scene, source) if held in following)).map(
        formed
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

NAME: Final = Descriptor.declared(bpy.types.SceneCompositorEffect, "name", bpy.types.StringProperty)
composites: Final = Setting(descriptor=Descriptor.declared(bpy.types.Scene, "render.use_compositing", bpy.types.BoolProperty))
EXECUTING: Final = Scoped[set[int]](made=lambda _identity: nullcontext(set()))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "EXECUTING",
    "NAME",
    "Configured",
    "Edge",
    "EffectOperators",
    "EffectStack",
    "Owned",
    "Pinned",
    "Shift",
    "Toward",
    "capturing",
    "composites",
    "hold",
    "keyed_product",
    "local_entry",
    "projected",
    "relocated",
]
