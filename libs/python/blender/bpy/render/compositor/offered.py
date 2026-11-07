"""Effects an extension offers together over scene compositor entries: phases ranking them, rivals sharing a rank, placement, moves, removal, holds, refresh, the pointwise read, entry commands, the add menu the stock one opens, and the handlers keeping entries current."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from enum import Enum
from functools import cached_property, partial, reduce
from itertools import chain, starmap
from operator import or_
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

from bl_ui.properties_compositor import SCENE_MT_add_compositor_effect
import bpy
from expression import Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from blender.bpy.commands.invocations import Open
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Placement, PopupKind, PopupTarget, Unchanged
from blender.bpy.data.ids.ownership import Mark, Ownership, retained
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.data.rna.paths import definition, rooted
from blender.bpy.data.rna.properties import Callbacks, choices, rebound
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import updated
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Declaration, Handler, Subscription
from blender.bpy.files.interchange import dropped_in, Interchange
from blender.bpy.nodes.desired import item_key
from blender.bpy.render.compositor.effects import CompositorEffect, CompositorRecord, Entry, FromFile, Mix, Named
from blender.bpy.render.compositor.entries import added_entry, EXECUTING, marked, Pinned, relocated, StackEnd
from blender.bpy.render.compositor.inputs import captured, defaulted, FRAMING, hold, holding, PreviewFrame, valued, WORKING_SPACE
from blender.bpy.render.compositor.masks import BLEND, MODE, OPACITY
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.results import RENDER_ORIGINS
from blender.bpy.scene import cameras
from blender.bpy.settings.attachments import Stored
from blender.bpy.ui.elements.menus import Append, Menu, Submenus
from blender.bpy.ui.layout.rows import Opens, OperatorButton
from blender.imaging.geometry.frames import Raster
from blender.records.faults import accumulated, Error, Fault, Faults, Nothing, Ok, Outcome, paired, Reason
from blender.records.trees import Toward

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Move(Enum):
    """Move of a named entry: one step toward an end of the stack, or into processing order."""

    UP = (Toward.UP, "Move effect one place up the stack")
    DOWN = (Toward.DOWN, "Move effect one place down the stack")
    PROCESSING_ORDER = (None, "Move effect into processing order")

    def __init__(self, toward: Toward | None, description: str) -> None:
        """Holds the step, Nothing into processing order, and the item description."""
        self.toward: Option[Toward] = Option.of_optional(toward)
        self.description = description


# --- [CONSTANTS] ------------------------------------------------------------------------

_TREE_TYPE: Final[bpy.types.EnumProperty] = bpy.types.SpaceNodeEditor.bl_rna.properties["tree_type"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_TREE_ICON: Final = _TREE_TYPE.enum_items[definition(bpy.types.CompositorNodeTree).identifier].icon

# --- [MODELS] ---------------------------------------------------------------------------


class Choice(bpy.types.PropertyGroup):
    """Offered effect a command adds, its items the offering rows."""

    kind: bpy.props.EnumProperty(name="Effect", description="Offered effect", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Shift(Named):
    """Named entry and requested move."""

    toward: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Toward", items=choices((move.name, bpy.path.display_name(move.name.lower()), move.description) for move in Move), default=Move.PROCESSING_ORDER.name
    )


class Phase(msgspec.Struct, frozen=True, kw_only=True):
    """Run of ranked effects in processing order under its key, each member one rank, the add menu filing them under the key's display name."""

    key: Key
    members: tuple[CompositorEffect | tuple[CompositorEffect, ...], ...]

    @property
    def title(self) -> str:
        """Display name of the key."""
        return bpy.path.display_name(self.key)

    @property
    def ranked(self) -> tuple[tuple[CompositorEffect, ...], ...]:
        """Effects of each rank in order."""
        return tuple((member,) if isinstance(member, CompositorEffect) else member for member in self.members)


class Owned(msgspec.Struct, frozen=True, kw_only=True):
    """Entry whose group holds the ownership mark of an offered effect."""

    entry: bpy.types.SceneCompositorEffect
    row: CompositorEffect


# --- [SERVICES] -------------------------------------------------------------------------


class OfferedEffects(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Effects an extension offers in phases, their pinned groups, the commands editing their entries, the add menu the stock add menu opens, and the handlers keeping entries current."""

    # --- [STATE]

    phases: tuple[Phase, ...]
    pinned: tuple[Pinned, ...] = ()

    # --- [OFFERED_ROWS]

    @cached_property
    def rows(self) -> tuple[CompositorEffect, ...]:
        """Offered effects in rank order."""
        return tuple(row for phase in self.phases for rank in phase.ranked for row in rank)

    @cached_property
    def keyed(self) -> Mapping[Key, CompositorEffect]:
        """Offered effects by each ownership key of their groups."""
        return MappingProxyType({owner.key: row for row in self.rows for owner in row.ownerships})

    @cached_property
    def _placed(self) -> Mapping[Key, tuple[int, Phase, bool]]:
        """Rank across every phase, phase, and rank exclusivity of each offered effect by record key."""
        members = tuple((phase, member) for phase in self.phases for member in phase.members)
        return MappingProxyType({
            row.kind: (index, phase, isinstance(member, tuple)) for index, (phase, member) in enumerate(members) for row in ((member,) if isinstance(member, CompositorEffect) else member)
        })

    def rank(self, row: CompositorEffect) -> int:
        """Position of the phase member holding the effect, across every phase."""
        return self._placed[row.kind][0]

    def phase(self, row: CompositorEffect) -> Phase:
        """Phase holding the effect."""
        return self._placed[row.kind][1]

    def rivals(self, row: CompositorEffect, other: CompositorEffect) -> bool:
        """Whether two effects share an exclusive rank, a stack holding one entry of either."""
        return self._placed[row.kind][2] and self.rank(row) == self.rank(other)

    @cached_property
    def form(self) -> TypeForm[CompositorRecord]:
        """Union of offered record types."""
        return reduce(or_, (row.record for row in self.rows))  # ty: ignore[unsound-return-statement]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Frame size attachment camera frame derivation binds, and the cell of scenes under execution."""
        return (FrameSize.attachment, EXECUTING)

    def standalone(self, scene: bpy.types.Scene) -> Outcome[tuple[CompositorEffect, ...]]:
        """Effects whose scene-seeded records need neither render passes nor a chosen file."""
        return accumulated(row.seeded(scene).map(lambda record: (row, record.requested)) for row in self.rows if not isinstance(row.source, FromFile)).map(
            lambda found: tuple(row for row, request in found if request is None)
        )

    def offering(self, kind: Key | type[CompositorRecord]) -> Outcome[CompositorEffect]:
        """Finds the effect of a record key or exact record type."""
        key = kind if isinstance(kind, str) else item_key(kind)
        return Option.of_optional(self.keyed.get(key)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(tuple(row.kind for row in self.rows)), member=key))

    # --- [ENTRIES]

    def owned(self, identity: Identity, scene: bpy.types.Scene) -> tuple[Owned, ...]:
        """Returns entries offered effects own, with their effects, in stack order."""
        return tuple(Owned(entry=entry, row=self.keyed[mark.key]) for entry, mark in marked(identity, scene) if mark.key in self.keyed)

    def row(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Option[CompositorEffect]:
        """Returns the offered effect owning entry's group."""
        return Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).bind(lambda mark: Option.of_optional(self.keyed.get(mark.key)))

    def found(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[Owned]:
        """Finds named owned entry, skipping user groups."""
        return (
            Option
            .of_optional(scene.compositor_effects.get(name))
            .bind(lambda entry: self.row(identity, entry).map(lambda row: Owned(entry=entry, row=row)))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    def held(self, identity: Identity, scene: bpy.types.Scene) -> tuple[CompositorEffect, ...]:
        """Returns effects holding an entry in the scene, in declaration order."""
        keys = frozenset(mark.key for _, mark in marked(identity, scene))
        return tuple(row for row in self.rows if any(owner.key in keys for owner in row.ownerships))

    def pointwise(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[tuple[CompositorEffect, Entry, Mapping[Key, Value]]]:
        """Reads an owned entry of a pointwise effect replacing its input or blending under no mask at the plain mode and full opacity, with its effect and input values by item key.

        Returns:
            `Ok` of the effect, entry record, and values. `Error` of `absent` for an entry no offered effect owns, and of `outside` for an effect reading neighbors or a blend that mixes.
        """

        def plain(row: CompositorEffect, record: Entry, values: Mapping[Key, Value]) -> Outcome[tuple[CompositorEffect, Entry, Mapping[Key, Value]]]:
            unmixed = row.mix is Mix.REPLACE or (not record.blend.masks and all(values.get(item_key(BLEND, socket.key)) == socket.default_value for socket in (MODE, OPACITY)))
            return paired(
                Ok(None) if row.pointwise else Error(Fault.of(Reason.OUTSIDE, entry.name, row.kind, member="pointwise effects")),
                Ok(None) if unmixed else Error(Fault.of(Reason.OUTSIDE, entry.name, repr(record.blend), member=f"plain {BLEND}")),
            ).map(lambda _: (row, record, values))

        return (
            self
            .row(identity, entry)
            .to_result_with(partial(Fault.of, Reason.ABSENT, entry.name, member="offered effect"))
            .bind(lambda row: paired(row.read(identity, entry), captured(identity, entry)).bind(lambda pair: plain(row, pair[0], valued(pair[1]))))
        )

    # --- [PLACEMENT]

    def placed(
        self, identity: Identity, scene: bpy.types.Scene, row: CompositorEffect, record: Entry, *, preview: Option[PreviewFrame] = Nothing, rasters: Option[tuple[Raster, Raster]] = Nothing
    ) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Places entry at rank under the preview frame, the held entry of a rival kept, applying holds and restoring the active entry."""
        prior = Option.of_optional(scene.compositor_effects.active)
        existing = Block.of_seq(self.owned(identity, scene)).filter(lambda held: self.rivals(row, held.row)).try_head().map(lambda held: held.entry)

        def added() -> Outcome[bpy.types.SceneCompositorEffect]:
            return row.appended(identity, scene, record, preview=preview, rasters=rasters).bind(
                lambda entry: (
                    hold(identity, scene, row.kind, row.holds)
                    .bind(lambda _: self.moved(identity, scene, entry.name, Nothing))
                    .bind(lambda _: prior.map(lambda active: written(active, ("is_active",), value=True).map(lambda _: None)).default_value(Ok(None)))
                    .map(lambda _: entry)
                )
            )

        return existing.map(Ok).default_with(added)

    def assigned(self, identity: Identity, scene: bpy.types.Scene, record: CompositorRecord) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Writes the earliest entry of the record's type or places a new one."""

        def assigned(row: CompositorEffect) -> Outcome[bpy.types.SceneCompositorEffect]:
            return (
                row
                .earliest(identity, scene)
                .map(lambda entry: row.revised(identity, scene, entry, lambda _: Ok(record), form=row.record).map(lambda _: entry))
                .default_with(lambda: self.placed(identity, scene, row, Entry(record=record)))
            )

        return self.offering(type(record)).bind(assigned)

    def moved(self, identity: Identity, scene: bpy.types.Scene, name: str, toward: Option[Toward]) -> Outcome[bool]:
        """Moves named owned entry alone one step, else into its rank, answering whether it moved."""

        def moved(held: Owned) -> Outcome[bool]:
            order = tuple(scene.compositor_effects)
            tail = frozenset(pinned.ownership.key for pinned in self.pinned if pinned.edge is StackEnd.TAIL)
            rank = self.rank(held.row)
            above = frozenset(entry for entry, mark in marked(identity, scene) for row in (self.keyed.get(mark.key),) if mark.key in tail or (row is not None and self.rank(row) > rank))
            others = tuple(entry for entry in order if entry != held.entry)
            ranked = next((index for index, entry in enumerate(others) if entry in above), len(others))
            return relocated(scene, held.entry, toward.map(lambda step: step.stepped(order.index(held.entry), len(order))).default_value(ranked))

        return self.found(identity, scene, name).bind(moved)

    def removed(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[None]:
        """Removes owned entry, unused groups, and last-entry holds."""

        def removed(held: Owned) -> Outcome[None]:
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

    def release(self, identity: Identity, scene: bpy.types.Scene, row: CompositorEffect) -> Outcome[None]:
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

    # --- [REFRESHING]

    def refreshed(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[bool]:
        """Separates per-entry groups, places missing pinned entries, and releases stale holds, answering whether an entry changed."""

        def separated(entry: bpy.types.SceneCompositorEffect, owner: Ownership[bpy.types.NodeTree], group: bpy.types.NodeTree) -> Outcome[bool]:
            with ExitStack() as rollback:
                copied: bpy.types.NodeTree = group.copy()  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                return retained(owner.owned(identity, copied, of=Some(scene), scope=Some(rollback)).bind(lambda owned: written(entry, ("node_group",), owned)).map(lambda _: True), rollback)

        def placed(pinned: Pinned, group: bpy.types.CompositorNodeTree) -> Outcome[bool]:
            with ExitStack() as rollback:
                at = Some(0) if pinned.edge is StackEnd.HEAD else Nothing
                return retained(added_entry(rollback, scene, group, name=Some(pinned.named), at=at, enables=pinned.enables).map(lambda _: True), rollback)

        personal = tuple(
            (owned.entry, owner, group)
            for owned in self.owned(identity, scene)
            for owner in (owned.row.ownerships[1],)
            for group in Option.of_optional(owned.entry.node_group).to_list()
            if Mark.read(identity, group).filter(lambda mark: mark.key == owner.key).is_some()
        )
        keeper: dict[int, bpy.types.SceneCompositorEffect] = {}
        for entry, _, group in personal:
            keeper.setdefault(group.session_uid, entry)
        absent = (
            (pinned, groups[0])
            for pinned in self.pinned
            for groups in (tuple(group for group, mark in pinned.ownership.held(identity) if mark.of == Some(scene)),)
            if groups and not any(entry.node_group in groups for entry in scene.compositor_effects)
        )
        changed = False
        for result in chain((separated(entry, owner, group) for entry, owner, group in personal if keeper[group.session_uid] != entry), starmap(placed, absent)):
            if result.is_error():
                return Error(result.error)
            changed = result.ok or changed
        return (
            holding(identity, scene)
            .bind(lambda pair: accumulated(self.release(identity, scene, row) for row in self.rows if row.kind in pair[1] and not row.entries(identity, scene)))
            .map(lambda _: changed)
        )

    # --- [COMMANDS]

    @cached_property
    def choice(self) -> type[Choice]:
        """Form choosing one offered effect, an item per row under its key, title, and record documentation."""
        return rebound(Choice, kind=Callbacks(items=choices((row.kind, row.title, row.description) for row in self.rows)))

    @cached_property
    def adding(self) -> Operator[Choice]:
        """Command adding the chosen effect at its rank."""
        return Operator.of(self.choice)(self.add_effect)

    @cached_property
    def opening(self) -> tuple[tuple[CompositorEffect, Operator[Empty]], ...]:
        """Per-row commands of effects opened from files, the drop handler's operators."""

        def command(row: CompositorEffect, opened: FromFile) -> Operator[Empty]:
            return Operator(form=Empty, named=Some(f"add_{row.kind}"), invoke=Some(Open[Empty](extensions=lambda _: opened.extensions())), run=partial(self.open_effect, row, opened.of))

        return tuple((row, command(row, opened)) for row in self.rows for opened in (row.source,) if isinstance(opened, FromFile))

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
        return Operator.of(Named, icon=Some("LOOP_BACK"))(self.reset_effect)

    @cached_property
    def menu(self) -> Menu:
        """Add menu of every effect in rank order, one submenu per phase, one flat list for a single phase."""
        openers = {row.kind: operator for row, operator in self.opening}
        buttons = tuple(
            (phase.key, OperatorButton(target=openers.get(row.kind, Placement(self.adding, {"kind": row.kind})), keywords={"text": row.title}))
            for phase in self.phases
            for rank in phase.ranked
            for row in rank
        )
        return Menu(key=self.adding.key, description=self.adding.description, body=Submenus(entries=buttons))

    def add_effect(self, call: Call[Choice]) -> Outcome[Answer]:
        """Add the chosen effect at its rank."""
        return (
            rooted(bpy.types.Scene, call.context)
            .bind(lambda scene: self.offering(call.properties.kind).bind(lambda row: row.seeded(scene).bind(lambda record: self.placed(call.identity, scene, row, Entry(record=record)))))
            .map(lambda _: Done())
        )

    def open_effect(self, row: CompositorEffect, of: Callable[[str], CompositorRecord], call: Call[Empty]) -> Outcome[Answer]:
        """Add the effect at its rank, one entry per chosen file."""
        records = tuple(of(str(path)) for path in call.paths)
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
            .bind(lambda scene: self.moved(call.identity, scene, call.properties.name, Move[call.properties.toward].toward))
            .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.properties.name} holds requested position"))
        )

    def remove_effect(self, call: Call[Named]) -> Outcome[Answer]:
        """Remove compositor effect."""
        return rooted(bpy.types.Scene, call.context).bind(lambda scene: self.removed(call.identity, scene, call.properties.name)).map(lambda _: Done())

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Acquisitions]:
        """Yields offered effects, commands, the add menu the stock add menu opens, handlers, and subscriptions with deferred refresh, and working-space conversion of entry colors."""

        def kept(scene: bpy.types.Scene, source: Option[bpy.types.Scene], groups: frozenset[int]) -> Outcome[bool]:
            def rewritten(held: Owned) -> Outcome[bool]:
                reading = source.map(lambda evaluated: held.row.named(identity, evaluated, held.entry.name)).default_value(Ok(held.entry))
                return reading.bind(lambda entry: held.row.read(identity, entry)).bind(
                    lambda record: (
                        held.row.write(identity, scene, held.entry, record, source=source)
                        if source.is_some() or record.record.requested is not None or any(group.session_uid in groups for group in Option.of_optional(held.entry.node_group).to_list())
                        else held.row.refreshed(identity, scene, held.entry)
                    )
                )

            return Ok(value=False) if scene.session_uid in EXECUTING[identity] else accumulated(rewritten(held) for held in self.owned(identity, scene)).map(any)

        def refreshed(handle: Handle[bpy.types.Scene], groups: tuple[int, ...]) -> Outcome[Option[str]]:
            def update(scene: bpy.types.Scene) -> Outcome[Option[str]]:
                return (
                    Ok(Nothing)
                    if scene.session_uid in EXECUTING[identity]
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

            spaces: frozenset[rna_enums.SpaceTypeItems] = frozenset({"NODE_EDITOR", "IMAGE_EDITOR"})
            yield (
                RENDER_ORIGINS,
                WORKING_SPACE,
                *self.rows,
                self.moving,
                self.removing,
                self.resetting,
                self.adding,
                self.menu,
                Append(target=SCENE_MT_add_compositor_effect, rows=(Opens(target=PopupTarget(self.menu.key, PopupKind.MENU), keywords={"icon": _TREE_ICON}),)),
                *(operator for _, operator in self.opening),
                *(
                    Interchange(key=row.kind, title=Some(row.title), extensions=opens.extensions(), importing=Some(operator), drop=partial(dropped_in, spaces))
                    for row, operator in self.opening
                    for opens in (row.source,)
                    if isinstance(opens, FromFile)
                ),
                Handler(handlers=bpy.app.handlers.load_post, body=loaded),
                Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Scene, bpy.types.Action, bpy.types.CompositorNodeTree, bpy.types.Image), edited)),
                Handler(handlers=bpy.app.handlers.frame_change_post, body=lambda scene, graph: kept(scene, Some(graph.scene_eval), frozenset()).map(lambda _: None)),
                *(
                    Subscription(owner=owner, setting=setting, body=lambda: accumulated(kept(scene, Nothing, frozenset()) for scene in bpy.data.scenes).map(lambda _: None))
                    for owner, setting in dict.fromkeys((*FRAMING.reads, *(read for row in self.rows for read in row.reads)))
                ),
                *(Subscription(owner=bpy.types.Image, setting=bpy.types.Image.bl_rna.properties[name], body=retagged) for name in ("alpha_mode", "use_half_precision")),
            )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Choice", "Move", "OfferedEffects", "Owned", "Phase", "Shift"]
