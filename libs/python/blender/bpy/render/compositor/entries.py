"""Effects an extension offers together over scene compositor entries: phases ranking them, placement by rank, moves, removal, holds, refresh, the commands editing their entries, the add menu, and the handlers keeping entries current."""

from collections.abc import Generator, Mapping
from contextlib import contextmanager, ExitStack, nullcontext
from enum import auto, Enum
from functools import cached_property, partial, reduce
from operator import or_
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import PyOpenColorIO
from typing_extensions import TypeForm

from blender.bpy.commands.calls import called, Placement
from blender.bpy.commands.invocations import Open
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.ownership import Mark, Ownership, retained, tracked
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.mirrors import choices, itemized
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler, Subscription, updated
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.files.interchange import dropped_in, Interchange
from blender.bpy.nodes.desired import item_key
from blender.bpy.render.compositor.effects import CompositorEffect, Enabled, Entry, Named, Opened
from blender.bpy.render.compositor.inputs import defaulted, FRAMING, hold, holding, PreviewFrame, WORKING_SPACE
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.results import Production, PRODUCTIONS
from blender.bpy.scene import cameras
from blender.bpy.settings.attachments import Stored
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.surfaces.menus import Dropdown, Menu, Submenus
from blender.records.faults import accumulated, Fault, Faults, Outcome, Reason
from blender.records.fields import Case, Name
from blender.records.trees import Toward

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Edge(Enum):
    """Stack end holding an unranked pinned group."""

    HEAD = auto()
    TAIL = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

EXECUTING: Final = Resource[set[int]](made=lambda _identity: nullcontext(set()))
_RANK: Final[Key] = "RANK"

# --- [MODELS] ---------------------------------------------------------------------------


class Choice(bpy.types.PropertyGroup):
    """Offered effect a command adds, its items the offering rows."""

    kind: bpy.props.EnumProperty(name="Effect", description="Offered effect", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Shift(Named):
    """Named entry and requested move."""

    toward: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Toward",
        items=choices((
            *((step.name, bpy.path.display_name(step.name.lower()), f"Move effect one place {step.name.lower()} the stack") for step in Toward),
            (_RANK, "Processing Order", "Move effect into processing order"),
        )),
        default=_RANK,
    )


class Exclusive[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Effects of one rank a stack holds one entry of."""

    members: tuple[CompositorEffect[R], ...]


class Phase[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Run of ranked effects in processing order, each member one rank, the add menu filing them under its group."""

    members: tuple[CompositorEffect[R] | Exclusive[R], ...]
    group: Option[Enum] = Nothing

    @property
    def ranked(self) -> tuple[tuple[CompositorEffect[R], ...], ...]:
        """Effects of each rank in order."""
        return tuple(member.members if isinstance(member, Exclusive) else (member,) for member in self.members)


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
    """Effects an extension offers in phases, their pinned groups, the commands editing their entries, the add menu, and the handlers keeping entries current."""

    phases: tuple[Phase[R], ...]
    pinned: tuple[Pinned, ...] = ()

    # --- [OFFERED_ROWS]

    @cached_property
    def rows(self) -> tuple[CompositorEffect[R], ...]:
        """Offered effects in rank order."""
        return tuple(row for phase in self.phases for rank in phase.ranked for row in rank)

    @cached_property
    def keyed(self) -> Mapping[Key, CompositorEffect[R]]:
        """Offered effects by each ownership key of their groups."""
        return MappingProxyType({owner.key: row for row in self.rows for owner in row.ownerships})

    @cached_property
    def ranks(self) -> Mapping[Key, int]:
        """Rank of each offered effect by record key, across every phase."""
        return MappingProxyType({row.kind: index for index, rank in enumerate(rank for phase in self.phases for rank in phase.ranked) for row in rank})

    @cached_property
    def exclusive(self) -> frozenset[Key]:
        """Record keys of effects sharing an exclusive rank, a stack holding one entry of that rank."""
        return frozenset(row.kind for phase in self.phases for member in phase.members if isinstance(member, Exclusive) for row in member.members)

    def rank(self, row: CompositorEffect[R]) -> int:
        """Position of the phase member holding the effect, across every phase."""
        return self.ranks[row.kind]

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

    def offering(self, kind: Key | type[Case]) -> Outcome[CompositorEffect[R]]:
        """Finds the effect of a record key or exact record type."""
        key = kind if isinstance(kind, str) else item_key(kind)
        return Option.of_optional(self.keyed.get(key)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(tuple(row.kind for row in self.rows)), member=key))

    # --- [ENTRIES]

    def owned(self, identity: Identity, scene: bpy.types.Scene) -> tuple[Owned[R], ...]:
        """Returns entries offered effects own, with their effects, in stack order."""
        return tuple(Owned(entry=entry, row=self.keyed[mark.key]) for entry, mark in _marked(identity, scene) if mark.key in self.keyed)

    def row(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Option[CompositorEffect[R]]:
        """Returns the offered effect owning entry's group."""
        return Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).bind(lambda mark: Option.of_optional(self.keyed.get(mark.key)))

    def found(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[Owned[R]]:
        """Finds named owned entry, skipping user groups."""
        return (
            Option
            .of_optional(scene.compositor_effects.get(name))
            .bind(lambda entry: self.row(identity, entry).map(lambda row: Owned(entry=entry, row=row)))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    def held(self, identity: Identity, scene: bpy.types.Scene) -> tuple[CompositorEffect[R], ...]:
        """Returns effects holding an entry in the scene, in declaration order."""
        keys = frozenset(mark.key for _, mark in _marked(identity, scene))
        return tuple(row for row in self.rows if any(owner.key in keys for owner in row.ownerships))

    # --- [PLACEMENT]

    def placed(self, identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[R], record: Entry[R], *, preview: Option[PreviewFrame] = Nothing) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Places entry at rank under the preview frame, the held entry of a single rank kept, applying holds and restoring the active entry."""
        prior = Option.of_optional(scene.compositor_effects.active)
        existing = (
            Block.of_seq(self.owned(identity, scene)).filter(lambda held: self.rank(held.row) == self.rank(row)).try_head().map(lambda held: held.entry) if row.kind in self.exclusive else Nothing
        )

        def added() -> Outcome[bpy.types.SceneCompositorEffect]:
            return appended(identity, scene, row, record, preview=preview).bind(
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
                .map(lambda entry: row.revised(identity, scene, entry, lambda _: Ok(record)).map(lambda _: entry))
                .default_with(lambda: self.placed(identity, scene, row, Entry(record=record)))
            )

        return self.offering(type(record)).bind(assigned)

    def moved(self, identity: Identity, scene: bpy.types.Scene, name: str, toward: Option[Toward]) -> Outcome[bool]:
        """Moves named owned entry alone one step, else into its rank, answering whether it moved."""

        def moved(held: Owned[R]) -> Outcome[bool]:
            order = tuple(scene.compositor_effects)
            tail = frozenset(pinned.ownership.key for pinned in self.pinned if pinned.edge is Edge.TAIL)
            above = frozenset(entry for entry, mark in _marked(identity, scene) if mark.key in tail) | frozenset(
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
                    catch(scene.compositor_effects.remove, exception=RuntimeError)(held.entry).map_error(partial(Fault.of, Reason.REFUSED, repr(scene))),
                    lambda _: self.release(identity, scene, row) if not row.entries(identity, scene) else Ok(None),
                )
                .bind(lambda _: accumulated(owner.removed(identity) for owner in row.ownerships))
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
                .bind(lambda _: accumulated(cameras.required(offered.holds.cameras, target, identity) for target in bpy.data.scenes for offered in self.held(identity, target)).map(lambda _: None))
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
                at = Some(0) if pinned.edge is Edge.HEAD else Nothing
                return retained(added_entry(rollback, scene, group, name=Some(pinned.named), at=at, enables=pinned.enables).map(lambda _: True), rollback)

        personal = tuple(
            (owned.entry, owner, group)
            for owned in self.owned(identity, scene)
            for owner in (owned.row.ownerships[1],)
            for group in Option.of_optional(owned.entry.node_group).to_list()
            if Mark.read(identity, group).filter(lambda mark: mark.key == owner.key).is_some()
        )
        keeper = {group.session_uid: entry for entry, _, group in reversed(personal)}
        absent = (
            (pinned, groups[0])
            for pinned in self.pinned
            for groups in (tuple(group for group, mark in pinned.ownership.held(identity) if mark.of == Some(scene)),)
            if groups and not any(entry.node_group in groups for entry in scene.compositor_effects)
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

    # --- [COMMANDS]

    @cached_property
    def choice(self) -> type[Choice]:
        """Form choosing one offered effect, an item per row under its key, title, and record documentation."""
        return itemized(Choice, kind=choices((row.kind, row.title, row.description) for row in self.rows))

    @cached_property
    def adding(self) -> Operator[Choice]:
        """Command adding the chosen effect at its rank."""
        return Operator.of(self.choice)(self.add_effect)

    @cached_property
    def opening(self) -> tuple[tuple[CompositorEffect[R], Operator[Empty]], ...]:
        """Per-row commands of effects opened from files, the drop handler's operators."""

        def command(row: CompositorEffect[R], opened: Opened[R]) -> Operator[Empty]:
            return Operator.of(Empty, named=Some(f"add_{row.kind}"), invoke=Some(Open(extensions=lambda _: opened.extensions())))(partial(self.open_effect, row))

        return tuple((row, command(row, opened)) for row in self.rows for opened in (row.source,) if isinstance(opened, Opened))

    @cached_property
    def moving(self) -> Operator[Shift]:
        """Command moving an entry one step or into its rank."""
        return Operator.of(Shift)(self.move_effect)

    @cached_property
    def removing(self) -> Operator[Named]:
        """Command removing an owned entry."""
        return Operator.of(Named)(self.remove_effect)

    @cached_property
    def resetting(self) -> Operator[Named]:
        """Command resetting an entry to its source record and interface defaults."""
        return Operator.of(Named)(self.reset_effect)

    @cached_property
    def menu(self) -> Menu:
        """Add menu of every effect in rank order, one submenu per phase group where every phase holds one."""
        openers = {row.kind: operator for row, operator in self.opening}
        buttons = tuple(
            (phase.group, OperatorButton(target=openers.get(row.kind, Placement(self.adding, {"kind": row.kind})), keywords={"text": row.title}))
            for phase in self.phases
            for rank in phase.ranked
            for row in rank
        )
        filed = tuple((group, button) for held, button in buttons for group in held.to_list())
        body = Submenus(entries=filed) if len(filed) == len(buttons) else Dropdown(entries=tuple(button for _, button in buttons), search=True)
        return Menu(key=self.adding.key, description=self.adding.description, body=body)

    def add_effect(self, call: Call[Choice]) -> Outcome[Answer]:
        """Add the chosen effect at its rank."""
        return (
            rooted(bpy.types.Scene, call.context)
            .bind(lambda scene: self.offering(call.properties.kind).bind(lambda row: row.seeded(scene).bind(lambda record: self.placed(call.identity, scene, row, Entry(record=record)))))
            .map(lambda _: Done())
        )

    def open_effect(self, row: CompositorEffect[R], call: Call[Empty]) -> Outcome[Answer]:
        """Add the effect at its rank, one entry per chosen file."""
        opened = row.source
        records = tuple(opened.of(str(path)) for path in call.paths) if isinstance(opened, Opened) else ()
        return rooted(bpy.types.Scene, call.context).bind(lambda scene: accumulated(self.placed(call.identity, scene, row, Entry(record=record)) for record in records)).map(lambda _: Done())

    def reset_effect(self, call: Call[Named]) -> Outcome[Answer]:
        """Reset effect to defaults."""
        return (
            rooted(bpy.types.Scene, call.context)
            .bind(
                lambda scene: self.found(call.identity, scene, call.properties.name).bind(
                    lambda held: defaulted(call.identity, held.entry).bind(
                        lambda reset: held.row.seeded(scene).bind(lambda effect: held.row.write(call.identity, scene, held.entry, Entry(record=effect))).map(lambda changed: changed or reset)
                    )
                )
            )
            .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.properties.name} holds defaults"))
        )

    def move_effect(self, call: Call[Shift]) -> Outcome[Answer]:
        """Move effect up, down, or to processing order."""
        return (
            rooted(bpy.types.Scene, call.context)
            .bind(lambda scene: self.moved(call.identity, scene, call.properties.name, Option.of_optional(Toward.__members__.get(call.properties.toward))))
            .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.properties.name} holds requested position"))
        )

    def remove_effect(self, call: Call[Named]) -> Outcome[Answer]:
        """Remove compositor effect."""
        return rooted(bpy.types.Scene, call.context).bind(lambda scene: self.removed(call.identity, scene, call.properties.name)).map(lambda _: Done())

    # --- [LIFETIME]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields offered effects, commands, handlers, and subscriptions with deferred refresh, and the working-space follow-through of entry colors."""

        def kept(scene: bpy.types.Scene, source: Option[bpy.types.Scene], groups: frozenset[int]) -> Outcome[bool]:
            def rewritten(held: Owned[R]) -> Outcome[bool]:
                reading = source.map(lambda evaluated: held.row.named(identity, evaluated, held.entry.name)).default_value(Ok(held.entry))
                return reading.bind(lambda entry: held.row.read(identity, entry)).bind(lambda record: held.row.write(identity, scene, held.entry, record, source=source))

            return (
                Ok(False)
                if scene.session_uid in identity.held[EXECUTING]
                else accumulated(
                    rewritten(held)
                    if source.is_some() or held.row.request.is_some() or any(group.session_uid in groups for group in Option.of_optional(held.entry.node_group).to_list())
                    else held.row.refreshed(identity, scene, held.entry)
                    for held in self.owned(identity, scene)
                ).map(any)
            )

        def refreshed(handle: Handle[bpy.types.Scene], groups: tuple[int, ...]) -> Outcome[Option[str]]:
            def update(scene: bpy.types.Scene) -> Outcome[Option[str]]:
                return (
                    Ok(Nothing)
                    if scene.session_uid in identity.held[EXECUTING]
                    else self.refreshed(identity, scene).bind(lambda changed: kept(scene, Nothing, frozenset(groups)).map(lambda wrote: Some("Refresh Effects") if changed or wrote else Nothing))
                )

            return handle.resolve().bind(update)

        with identity.reports.deferred(refreshed) as defer:

            def loaded() -> Outcome[None]:
                return accumulated(
                    self
                    .refreshed(identity, scene)
                    .bind(lambda _: kept(scene, Nothing, frozenset()))
                    .bind(lambda _: accumulated(hold(identity, scene, row.kind, row.holds) for row in self.held(identity, scene)))
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
                        Production.recorded(identity, image, Nothing)
                return Ok(None)

            spaces: frozenset[rna_enums.SpaceTypeItems] = frozenset({"NODE_EDITOR", "IMAGE_EDITOR"})
            yield Ok((
                PRODUCTIONS,
                WORKING_SPACE,
                *self.rows,
                self.moving,
                self.removing,
                self.resetting,
                self.adding,
                *(operator for _, operator in self.opening),
                *(
                    Interchange(key=row.kind, title=Some(row.title), extensions=opens.extensions(), importing=Some(operator), drop=partial(dropped_in, spaces))
                    for row, operator in self.opening
                    for opens in (row.source,)
                    if isinstance(opens, Opened)
                ),
                Handler(handlers=bpy.app.handlers.load_post, body=loaded),
                Handler(handlers=bpy.app.handlers.render_init, body=invalidated),
                Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Scene, bpy.types.Action, bpy.types.CompositorNodeTree, bpy.types.Image), edited)),
                Handler(handlers=bpy.app.handlers.frame_change_post, body=lambda scene, graph: kept(scene, Some(graph.scene_eval), frozenset()).map(lambda _: None)),
                *(
                    Subscription(owner=owner, setting=setting, body=lambda: accumulated(kept(scene, Nothing, frozenset()) for scene in bpy.data.scenes).map(lambda _: None))
                    for owner, setting in dict.fromkeys((*FRAMING.reads, *(read for row in self.rows for read in row.reads)))
                ),
                *(Subscription(owner=bpy.types.Image, setting=bpy.types.Image.bl_rna.properties[name], body=retagged) for name in ("alpha_mode", "use_half_precision")),
            ))


# --- [OPERATIONS] -----------------------------------------------------------------------


def appended[R: Case](
    identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[R], record: Entry[R], *, preview: Option[PreviewFrame] = Nothing, name: Option[Name] = Nothing
) -> Outcome[bpy.types.SceneCompositorEffect]:
    """Creates an entry of an offered effect once its requirements and group succeed, the entry named after the effect unless stated."""
    with ExitStack() as rollback:
        made = row.met(identity, scene, record.record, Nothing, preview).bind(lambda request: row.made(identity, scene, record, PyOpenColorIO.GetCurrentConfig(), preview, request))
        return retained(
            made.bind(
                lambda built: added_entry(rollback, scene, built[0], name=Some(name.default_value(row.title))).bind(
                    lambda entry: row.filled(identity, scene, entry, built[0], record).map(lambda _: entry)
                )
            ),
            rollback,
        )


def added_entry(
    scope: ExitStack, scene: bpy.types.Scene, group: bpy.types.NodeTree, /, *, name: Option[str] = Nothing, at: Option[int] = Nothing, enables: frozenset[Enabled] = frozenset(Enabled)
) -> Outcome[bpy.types.SceneCompositorEffect]:
    """Creates a scene compositor entry running a group under `name`, the group's name unless stated, removed when `scope` closes, its enable flags written and moved to `at` when given.

    A caller keeping the entry past success passes `scope` to `retained`.

    Returns:
        `Ok` of the entry, `refused` when the host refuses the entry, and faults of its group, enable, and move writes.
    """
    created = catch(scene.compositor_effects.new, exception=RuntimeError)(name.default_value(group.name)).map_error(partial(Fault.of, Reason.REFUSED, repr(scene.compositor_effects)))
    return tracked(scope, scene.compositor_effects.remove, created).bind(
        lambda entry: (
            accumulated((written(entry, ("node_group",), group), *(enabled.descriptor.write(entry, enabled in enables) for enabled in Enabled)))
            .bind(lambda _: at.map(lambda index: relocated(scene, entry, index)).default_value(Ok(False)))
            .map(lambda _: entry)
        )
    )


def relocated(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, index: int) -> Outcome[bool]:
    """Moves entry to stack index unless it sits there, answering whether it moved."""
    return (
        Ok(False)
        if scene.compositor_effects.find(entry.name) == index
        else called(bpy.ops.scene.move_compositor_effect_to_index, {"name": entry.name, "index": index}, override=Some(bpy.context.temp_override(scene=scene))).map(lambda _: True)
    )


def _marked(identity: Identity, scene: bpy.types.Scene) -> tuple[tuple[bpy.types.SceneCompositorEffect, Mark], ...]:
    """Returns the scene entries whose groups carry an extension ownership mark, with that mark, in stack order."""
    return tuple((entry, mark) for entry in scene.compositor_effects for group in Option.of_optional(entry.node_group).to_list() for mark in Mark.read(identity, group).to_list())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EXECUTING", "Choice", "Edge", "EffectRows", "Exclusive", "Owned", "Phase", "Pinned", "Shift", "added_entry", "appended", "relocated"]
