"""Layer stacks held in scene compositor entries: the stack service reading, editing, rebuilding, and persisting each stack with its input values."""

from collections.abc import Callable
from contextlib import ExitStack
from functools import partial
import itertools
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.data.ids.ownership import Mark, Ownership, retained, Saved
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.changes import Changes, reconciled
from blender.bpy.nodes.desired import Graph
from blender.bpy.nodes.functions import lowered
from blender.bpy.nodes.interface import item_key
from blender.bpy.nodes.trees import created
from blender.bpy.render.compositor.blending import Blend, Entry, OPACITY
from blender.bpy.render.compositor.effects import PRECISION
from blender.bpy.render.compositor.entries import Edge, EffectRows, Pinned, relocated
from blender.bpy.render.compositor.inputs import CameraFrame, captured, defaulted, entered, rebased, restored
from blender.bpy.render.compositor.layers import (
    Adjustment,
    Edit,
    EffectAt,
    EffectMaskAt,
    EFFECTS,
    ENTRY_BLEND,
    LAYER_BLEND,
    LayerAt,
    LayerMaskAt,
    LAYERS,
    LayerStack,
    MASKS,
    NodeSource,
    Order,
    planned,
    RenderPass,
    SOURCE,
    Source,
    sourced,
    StackLayer,
    Target,
    TermFiltering,
)
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.render.compositor.recombination import LightGroups, Recombined
from blender.bpy.render.view_layers.passes import PassFlags, PassRequest, required
from blender.bpy.settings.attachments import Attached, Attachment, Stored
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.fields import amended, Case, Lens

# --- [CONSTANTS] ------------------------------------------------------------------------

OWNERSHIP: Final = Ownership(kind=bpy.types.CompositorNodeTree, key="layer_stack", saved=Saved.ALWAYS, single=False)
TITLE: Final = bpy.path.display_name(OWNERSHIP.key)
PINNED: Final = Pinned(ownership=OWNERSHIP, edge=Edge.HEAD)

# --- [MODELS] ---------------------------------------------------------------------------


class StackView(bpy.types.PropertyGroup):
    """Screen's active layer index, bottom-first layer order."""

    active: bpy.props.IntProperty(name="Active Layer", description="Bottom-first index of the edited layer", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LayerRow(bpy.types.PropertyGroup):
    """List row of one layer under the layer's name, its order and flags the stack composites by."""

    key: bpy.props.StringProperty(name="Key", description="Layer key its inputs and nodes sit under", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    visible: bpy.props.BoolProperty(name="Visible", description="Composite layer", default=True, options=set())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    solo: bpy.props.BoolProperty(name="Solo", description="Composite soloed layers alone", options=set())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    matte: bpy.props.BoolProperty(name="Matte", description="Track the layer beneath as this layer's matte", options=set())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LayerRows(bpy.types.PropertyGroup):
    """Layer rows of one stack, bottom first."""

    layers: bpy.props.CollectionProperty(type=LayerRow, name="Layers", description="Layers bottom first")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class StoredLayer[S: Case, M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Structure of one layer the stack group stores under the layer key, its name, flags, and order held by its layer row."""

    source: S
    effects: tuple[Entry[M], ...] = ()
    blend: Blend = Blend()


class Selection[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Head entry, its owned stack and record, and the screen's active layer index."""

    entry: bpy.types.SceneCompositorEffect
    tree: bpy.types.NodeTree
    record: LayerStack[Source, M]
    active: int

    @property
    def layer(self) -> Outcome[StackLayer[Source, M]]:
        """Active layer."""
        return self.record.layer(self.active)


# --- [SERVICES] -------------------------------------------------------------------------


class LayerStacking[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Layer stacks bound to one extension: the offered effects and term filter, the store of each layer's structure by key, the layer rows owning each layer's name, flags, and order, the view attachment, and pass flags."""

    identity: Identity
    rows: EffectRows[M]
    filtering: TermFiltering[M]
    store: Stored[dict[Key, StoredLayer[Source, M]]]
    layer_rows: Attached[LayerRows]
    view: Attached[StackView]
    flags: PassFlags

    # --- [SELECTION]

    def head(self, scene: bpy.types.Scene) -> Outcome[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree]]:
        """Returns the first entry holding an owned stack, with that stack."""
        held = Block.of_seq(scene.compositor_effects).choose(lambda entry: stacked_by(self.identity, entry).map(lambda tree: (entry, tree))).try_head()
        return held.to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(scene.compositor_effects), member=OWNERSHIP.key))

    def named(self, name: str) -> Outcome[bpy.types.NodeTree]:
        """Returns the owned stack of a node group name."""
        held = Block.of_seq(OWNERSHIP.held(self.identity)).choose(lambda pair: Some(pair[0]) if pair[0].name == name else Nothing).try_head()
        return held.to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(bpy.data.node_groups), member=name))

    def selected(self, context: bpy.types.Context) -> Outcome[Selection[M]]:
        """Returns head entry, owned stack, record, and screen selection."""
        held = rooted(bpy.types.Scene, context).bind(self.head).bind(lambda found: self.read(found[1]).map(lambda record: (*found, record)))
        return paired(held, rooted(bpy.types.Screen, context).map(self.view.struct)).map(lambda parts: Selection(entry=parts[0][0], tree=parts[0][1], record=parts[0][2], active=parts[1].active))

    # --- [VALUES]

    def read(self, tree: bpy.types.NodeTree) -> Outcome[LayerStack[Source, M]]:
        """Joins the layer rows, which own each layer's name, flags, and order, with the structure the group stores under each row's key, an empty stack before the first store."""
        rows = tuple(self.layer_rows.struct(tree).layers)

        def joined(stored: dict[Key, StoredLayer[Source, M]]) -> LayerStack[Source, M]:
            return LayerStack(
                layers=tuple(
                    StackLayer(key=row.key, name=row.name, visible=row.visible, solo=row.solo, matte=row.matte, source=part.source, effects=part.effects, blend=part.blend)
                    for row in rows
                    for part in Option.of_optional(stored.get(row.key)).to_list()
                )
            )

        return self.store.read(tree).map(lambda held: joined(held.default_value({})))

    def written(self, tree: bpy.types.NodeTree, record: LayerStack[Source, M]) -> Outcome[None]:
        """Splits a stack on write: each layer's structure to the store under its key, and its name, flags, and order to one layer row in stack order."""
        layers = self.layer_rows.struct(tree).layers
        for _ in range(len(layers), len(record.layers)):
            layers.add()
        for index in range(len(layers) - 1, len(record.layers) - 1, -1):
            layers.remove(index)
        rows = accumulated(
            assigned(row, {("key",): layer.key, ("name",): layer.name, ("visible",): layer.visible, ("solo",): layer.solo, ("matte",): layer.matte}) for row, layer in zip(layers, record.layers)
        )
        stored = self.store.write(tree, {layer.key: StoredLayer(source=layer.source, effects=layer.effects, blend=layer.blend) for layer in record.layers})
        return paired(stored, rows).map(lambda _: None)

    def rebuilt(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, record: LayerStack[Source, M], context: bpy.types.Context) -> Outcome[None]:
        """Writes the stack, then rebuilds its graph."""
        return self.written(tree, record).bind(lambda _: self.reconciled(scene, tree, context, record=Some(record))).map(lambda _: None)

    def moved(self, context: bpy.types.Context, origin: int, target: int) -> Outcome[None]:
        """Moves the layer row at one list row to another, then rebuilds the stack, its inputs following their layer keys."""

        def move(held: Selection[M]) -> Outcome[None]:
            self.layer_rows.struct(held.tree).layers.move(origin, target)
            return rooted(bpy.types.Scene, context).bind(lambda scene: self.reconciled(scene, held.tree, context)).map(lambda _: None)

        return self.selected(context).bind(move)

    def persisted(self, context: bpy.types.Context, held: Selection[M], record: LayerStack[Source, M], active: int) -> Outcome[None]:
        """Writes the stack and the active layer index."""
        rebuilt = rooted(bpy.types.Scene, context).bind(lambda scene: self.rebuilt(scene, held.tree, record, context))
        return paired(rebuilt, rooted(bpy.types.Screen, context).bind(lambda screen: written(self.view.struct(screen), ("active",), active))).map(lambda _: None)

    def looked(self, scene: bpy.types.Scene, preset: Valued[LayerStack[Source, M]], strength: int, context: bpy.types.Context) -> Outcome[bool]:
        """Writes a preset's structure and input values over the head stack, every effect mixing at `strength` percent through its blend opacity, creating the stack when absent, answering whether it changed."""

        def held() -> Outcome[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree]]:
            return self.head(scene).to_option().map(Ok).default_with(lambda: self.made(scene, context).bind(lambda _: self.head(scene)))

        def placed(entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree) -> Outcome[bool]:
            mixes: dict[Key, Value] = {item_key(layer.key, EFFECTS, index, ENTRY_BLEND, OPACITY.key): strength / 100 for layer in preset.structure.layers for index in range(1, len(layer.effects) + 1)}
            return self.read(tree).bind(
                lambda current: (
                    (Ok(False) if current == preset.structure else self.rebuilt(scene, tree, preset.structure, context).map(lambda _: True))
                    .bind(lambda changed: preset.rebased("").map(lambda values: restored(self.identity, entry, values)).default_value(Ok(False)).map(lambda wrote: changed or wrote))
                    .bind(lambda changed: entered(self.identity, entry, mixes).map(lambda mixed: changed or mixed))
                )
            )

        return held().bind(lambda found: placed(*found))

    def replaced(self, context: bpy.types.Context, held: Selection[M], record: LayerStack[Source, M], active: int) -> Outcome[bool]:
        """Writes the stack and active index when either changed, answering whether one did."""
        return Ok(False) if record == held.record and active == held.active else self.persisted(context, held, record, active).map(lambda _: True)

    def rewritten(self, context: bpy.types.Context, held: Selection[M], at: Lens, change: Callable[[object], object], active: int) -> Outcome[bool]:
        """Writes the stack with the value a lens reaches changed, answering whether the stack changed."""
        return amended(held.record, at, change).to_result_with(partial(Fault.of, BlenderReason.ABSENT, TITLE, member=str(active))).bind(lambda record: self.replaced(context, held, record, active))

    def appended(self, context: bpy.types.Context, held: Selection[M], entry: Entry[M]) -> Outcome[bool]:
        """Appends an effect entry to the active layer, answering whether the stack changed."""
        return self.rewritten(context, held, (LAYERS, held.active, EFFECTS), lambda found: (*found, entry) if isinstance(found, tuple) else found, held.active)

    def edited(self, context: bpy.types.Context, target: Target, edit: Edit) -> Outcome[bool]:
        """Removes, duplicates, moves, or resets a layer, effect, or mask, its input values following the items they belong to, answering whether anything changed."""
        identity = self.identity

        def structural(scene: bpy.types.Scene, held: Selection[M]) -> Outcome[tuple[bool, Order]]:
            done = held.record.edited(edit, target, lambda record: self.rows.offering(type(record)).bind(lambda row: row.seeded(scene)))
            return done.bind(lambda parts: self.replaced(context, held, parts[0], parts[1]).map(lambda changed: (changed, parts[2])))

        def carried(scene: bpy.types.Scene, held: Selection[M], base: Key, count: int) -> Outcome[bool]:
            def placed(captures: tuple[Snapshot, ...], new: int, old: Option[int]) -> Outcome[bool]:
                at = item_key(base, new + 1)
                return old.map(lambda prior: restored(identity, held.entry, rebased(captures[prior], item_key(base, prior + 1), at))).default_with(lambda: defaulted(identity, held.entry, at))

            def moved(captures: tuple[Snapshot, ...], done: tuple[bool, Order]) -> Outcome[bool]:
                return accumulated(placed(captures, new, old) for new, old in enumerate(done[1]) if old != Some(new)).map(lambda wrote: done[0] or any(wrote))

            captures = accumulated(captured(identity, held.entry, item_key(base, position + 1)) for position in range(count))
            return captures.bind(lambda found: structural(scene, held).bind(lambda done: moved(found, done)))

        def duplicated(held: Selection[M], layer: StackLayer[Source, M]) -> Outcome[bool]:
            def copied(values: Snapshot, after: Selection[M]) -> Outcome[bool]:
                return after.layer.bind(lambda made: restored(identity, after.entry, rebased(values, layer.key, made.key)))

            return captured(identity, held.entry, layer.key).bind(
                lambda values: self.inserted(context, (layer,), Some(held.tree)).bind(lambda changed: self.selected(context).bind(lambda after: copied(values, after)).map(lambda _: changed))
            )

        def applied(scene: bpy.types.Scene, held: Selection[M]) -> Outcome[bool]:
            within = held.record.layer
            match target:
                case LayerAt(layer=index) if edit is Edit.DUPLICATE:
                    return within(index).bind(lambda layer: duplicated(held, layer))
                case LayerAt(layer=index) if edit is Edit.RESET:
                    return within(index).bind(lambda layer: structural(scene, held).bind(lambda done: defaulted(identity, held.entry, layer.key).map(lambda wrote: done[0] or wrote)))
                case LayerAt():
                    return structural(scene, held).map(lambda done: done[0])
                case EffectAt(layer=index):
                    return within(index).bind(lambda layer: carried(scene, held, item_key(layer.key, EFFECTS), len(layer.effects)))
                case LayerMaskAt(layer=index):
                    return within(index).bind(lambda layer: carried(scene, held, item_key(layer.key, LAYER_BLEND, MASKS), layer.blend.masks))
                case EffectMaskAt(layer=index, entry=entry):
                    blend = held.record.blended(target).to_result_with(partial(Fault.of, BlenderReason.ABSENT, TITLE, member=str(entry)))
                    return paired(within(index), blend).bind(lambda parts: carried(scene, held, item_key(parts[0].key, EFFECTS, entry + 1, ENTRY_BLEND, MASKS), parts[1].masks))

        return paired(rooted(bpy.types.Scene, context), self.selected(context)).bind(lambda parts: applied(*parts))

    def applied(self, context: bpy.types.Context, value: Valued[LayerStack[Source, M]] | Valued[StackLayer[Source, M]] | Valued[Entry[M]]) -> Outcome[bool]:
        """Applies a stack, layer, or effect preset over the selection, then the input values it captured at the keys the applied item takes, answering whether anything changed."""

        def valued(prefix: Key, *, changed: bool) -> Outcome[bool]:
            return self.selected(context).bind(
                lambda after: value.rebased(prefix).map(lambda values: restored(self.identity, after.entry, values)).default_value(Ok(False)).map(lambda wrote: changed or wrote)
            )

        def edited(held: Selection[M]) -> Outcome[bool]:
            match value.structure:
                case LayerStack() as stack:
                    return self.replaced(context, held, stack, 0).bind(lambda changed: valued("", changed=changed))
                case StackLayer() as layer:
                    return held.layer.bind(
                        lambda prior: self.rewritten(context, held, (LAYERS, held.active), lambda _: msgspec.structs.replace(layer, key=prior.key), held.active).bind(
                            lambda changed: valued(prior.key, changed=changed)
                        )
                    )
                case Entry() as entry:
                    return held.layer.bind(lambda prior: self.appended(context, held, entry).bind(lambda changed: valued(item_key(prior.key, EFFECTS, len(prior.effects) + 1), changed=changed)))

        return self.selected(context).bind(edited)

    # --- [GRAPH]

    def sources(self, scene: bpy.types.Scene, record: LayerStack[Source, M], tree: bpy.types.NodeTree) -> Outcome[dict[Key, dict[str, str]]]:
        """Reads enabled source outputs from a scratch graph with settings copied from the stack's owned source nodes."""

        def read(scratch: bpy.types.CompositorNodeTree) -> Outcome[dict[Key, dict[str, str]]]:
            with ExitStack() as release:
                release.callback(bpy.data.node_groups.remove, scratch)

                def copied(_: Changes) -> Outcome[dict[Key, dict[str, str]]]:
                    found = owned(scratch, self.identity)
                    return accumulated(Snapshot.capture(source).bind(lambda snapshot: snapshot.apply(found[key]).outcome) for key, source in owned(tree, self.identity).items() if key in found).map(
                        lambda _: {key: {output.name: output.identifier for output in source.outputs if output.enabled} for key, source in found.items()}
                    )

                return (
                    accumulated(sourced(layer.key, source, scene) for layer in record.layers for source in (layer.source,) if not isinstance(source, Adjustment))
                    .bind(lambda found: lowered(bpy.types.CompositorNodeTree, *found))
                    .bind(lambda body: reconciled(Graph(kind=bpy.types.CompositorNodeTree, nodes=body.nodes), scratch, self.identity))
                    .bind(copied)
                )

        return created_tree("Layer Sources").bind(read)

    def reconciled(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, context: bpy.types.Context, *, record: Option[LayerStack[Source, M]] = Nothing) -> Outcome[bool]:
        """Enables the passes each source and term filter reads, builds the graph, and writes frame inputs, answering whether any changed."""

        def requested(layer: StackLayer[Source, M]) -> Outcome[tuple[PassRequest, ...]]:
            match layer.source:
                case RenderPass(layer=name):
                    return Ok((PassRequest(layer=name),))
                case Recombined() as source:
                    return self.filtering.requests(layer).map(lambda extra: (source.requested(self.flags), *extra))
                case Adjustment() | NodeSource():
                    return Ok(())

        def grouped(layer: StackLayer[Source, M]) -> Outcome[StackLayer[Source, M]]:
            match layer.source:
                case Recombined(layer=name, recombination=LightGroups()) as source:
                    return (
                        Option
                        .of_optional(scene.view_layers.get(name))
                        .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(scene.view_layers), member=name))
                        .bind(
                            lambda held: called(bpy.ops.scene.view_layer_add_used_lightgroups, {}, override=Some(context.temp_override(scene=scene, view_layer=held))).map(
                                lambda _: msgspec.structs.replace(layer, source=source.synced(tuple(held.lightgroups.keys())))
                            )
                        )
                    )
                case _:
                    return Ok(layer)

        def built(desired: LayerStack[Source, M], stacked: LayerStack[Source, M]) -> Outcome[bool]:
            return (
                (Ok(None) if stacked == desired else self.written(tree, stacked))
                .bind(lambda _: accumulated(map(requested, stacked.layers)))
                .bind(lambda wanted: required(scene, context, tuple(itertools.chain.from_iterable(wanted)), PRECISION))
                .bind(lambda _: self.sources(scene, stacked, tree))
                .bind(lambda offered: planned(stacked, self, scene, offered, tree.session_uid, OWNERSHIP.key, TITLE))
                .bind(lambda group: paired(reconciled(group, tree, self.identity), CameraFrame.of(self.identity, scene)))
                .bind(
                    lambda parts: accumulated(entered(self.identity, entry, parts[1].values) for entry in scene.compositor_effects if entry.node_group == tree).map(
                        lambda wrote: any(wrote) or parts[0] != Changes()
                    )
                )
            )

        desired = record.map(Ok).default_with(lambda: self.read(tree))
        return desired.bind(lambda held: accumulated(map(grouped, held.layers)).bind(lambda layers: built(held, msgspec.structs.replace(held, layers=layers))))

    def refreshed(self) -> Outcome[Option[str]]:
        """Reconciles each scene stack, naming the undo step when any changed."""
        return accumulated(
            self.reconciled(scene, tree, bpy.context)
            for scene in bpy.data.scenes
            for tree in dict.fromkeys(tree for entry in scene.compositor_effects for tree in stacked_by(self.identity, entry).to_list())
        ).map(lambda changed: Some(f"Refresh {TITLE}s") if any(changed) else Nothing)

    # --- [STACKS]

    def switched(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, context: bpy.types.Context) -> Outcome[None]:
        """Places a stack in the head entry, creating the entry when absent."""
        held = self.head(scene).to_option()
        with ExitStack() as rollback:
            entry = held.map(lambda found: found[0]).default_with(lambda: scene.compositor_effects.new(name=TITLE))
            for _ in () if held.is_some() else (entry,):
                rollback.callback(scene.compositor_effects.remove, entry)
            return retained(written(entry, ("node_group",), tree).bind(lambda _: relocated(scene, entry, 0)).bind(lambda _: self.reconciled(scene, tree, context)).map(lambda _: None), rollback)

    def made(self, scene: bpy.types.Scene, context: bpy.types.Context) -> Outcome[bpy.types.CompositorNodeTree]:
        """Creates an empty stack and places it in the head entry."""
        return OWNERSHIP.made(self.identity, lambda: created_tree(TITLE), of=Some(scene)).bind(
            lambda group: self.store.write(group, {}).bind(lambda _: self.switched(scene, group, context)).map(lambda _: group)
        )

    def inserted(self, context: bpy.types.Context, layers: tuple[StackLayer[Source, M], ...], source: Option[bpy.types.NodeTree] = Nothing) -> Outcome[bool]:
        """Inserts layers after the active layer under fresh keys with source node settings copied, answering whether any was given."""

        def insert(held: Selection[M]) -> Outcome[bool]:
            added = tuple(msgspec.structs.replace(layer, key=key) for layer, key in zip(layers, held.record.fresh()))
            at = min(held.active + 1, len(held.record.layers))
            nodes = tuple((node, after.node) for tree in source.to_list() for before, after in zip(layers, added) for node in Option.of_optional(owned(tree, self.identity).get(before.node)).to_list())

            def copy(node: bpy.types.Node, key: Key) -> Outcome[None]:
                return Snapshot.capture(node).bind(
                    lambda snapshot: created(held.tree, type(node)).bind(lambda made: snapshot.apply(made).outcome.bind(lambda _: Mark(key=key).write(self.identity, made)))
                )

            with ExitStack() as rollback:
                for _, key in nodes:
                    rollback.callback(dropped, held.tree, self.identity, key)
                record = msgspec.structs.replace(held.record, layers=(*held.record.layers[:at], *added, *held.record.layers[at:]))
                return retained(accumulated(itertools.starmap(copy, nodes)).bind(lambda _: self.replaced(context, held, record, at + len(added) - 1 if added else held.active)), rollback)

        return self.selected(context).bind(insert)

    def added_node[T](self, context: bpy.types.Context, kind: type[bpy.types.Node], setup: Callable[[bpy.types.Node], Outcome[T]]) -> Outcome[None]:
        """Adds a source node layer after the active layer at the node's first enabled output."""

        def add(held: Selection[M]) -> Outcome[None]:
            key, at = next(held.record.fresh()), min(held.active + 1, len(held.record.layers))
            layer_node = item_key(key, SOURCE)

            def placed(node: bpy.types.Node) -> Outcome[None]:
                layer: Option[StackLayer[Source, M]] = (
                    Block
                    .of_seq(node.outputs)
                    .filter(lambda output: output.enabled)
                    .try_head()
                    .map(lambda output: StackLayer(key=key, name=node.bl_label, source=NodeSource(node=kind.__name__, output=output.name)))
                )
                return layer.to_result_with(partial(Fault.of, BlenderReason.ABSENT, kind.__name__, member="outputs")).bind(
                    lambda made: self.persisted(context, held, msgspec.structs.replace(held.record, layers=(*held.record.layers[:at], made, *held.record.layers[at:])), at)
                )

            with ExitStack() as rollback:
                rollback.callback(dropped, held.tree, self.identity, layer_node)
                return retained(created(held.tree, kind).bind(lambda node: Mark(key=layer_node).write(self.identity, node).bind(lambda _: setup(node)).bind(lambda _: placed(node))), rollback)

        return self.selected(context).bind(add)


# --- [OPERATIONS] -----------------------------------------------------------------------


def created_tree(name: str) -> Outcome[bpy.types.CompositorNodeTree]:
    """Creates an empty compositor node group."""
    return narrowed(bpy.data.node_groups.new(name, "CompositorNodeTree"))


def narrowed(tree: bpy.types.ID | None) -> Outcome[bpy.types.CompositorNodeTree]:
    """Narrows a node group to a compositor node tree."""
    match tree:
        case bpy.types.CompositorNodeTree():
            return Ok(tree)
        case _:
            return Error(Fault.of(BlenderReason.ABSENT, repr(tree), member=bpy.types.CompositorNodeTree.__name__))


def dropped(tree: bpy.types.NodeTree, identity: Identity, key: Key) -> None:
    """Removes the owned node of a key when the tree still holds it."""
    for node in Option.of_optional(owned(tree, identity).get(key)).to_list():
        tree.nodes.remove(node)


def stacked_by(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Option[bpy.types.NodeTree]:
    """Returns an entry's node group when it holds the stack ownership mark."""
    return Option.of_optional(entry.node_group).filter(lambda group: OWNERSHIP.marked(identity, group).is_some())


def nested_tree[M: Case](identity: Identity, tree: bpy.types.NodeTree, layer: StackLayer[Source, M]) -> Option[bpy.types.NodeTree]:
    """Returns the owned stack of a layer's source group node."""
    match owned(tree, identity).get(layer.node):
        case bpy.types.CompositorNodeGroup(node_tree=inner) if inner is not None:
            return Some(inner).filter(lambda held: OWNERSHIP.marked(identity, held).is_some())
        case _:
            return Nothing


# --- [COMPOSITION] ----------------------------------------------------------------------

VIEW: Final = Attachment(key=f"{OWNERSHIP.key}_view", form=StackView, owner=bpy.types.Screen)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["OWNERSHIP", "PINNED", "TITLE", "VIEW", "LayerRow", "LayerRows", "LayerStacking", "Selection", "StackView", "StoredLayer", "created_tree", "narrowed", "nested_tree", "stacked_by"]
