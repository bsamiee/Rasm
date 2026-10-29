"""Layer stacks held in scene compositor entries: the stack service reading, editing, rebuilding, and persisting each stack with its input values, and the stack commands running over it."""

from collections.abc import Callable
from contextlib import ExitStack
from functools import partial
import itertools

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Unchanged
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.ownership import Mark, retained
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.data.rna.paths import rooted, subclass
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.desired import Graph, item_key
from blender.bpy.nodes.functions import lowered
from blender.bpy.nodes.trees import created, grouped
from blender.bpy.render.compositor.effects import Entry
from blender.bpy.render.compositor.entries import added_entry, Choice, EffectRows, relocated
from blender.bpy.render.compositor.inputs import CameraFrame, captured, defaulted, entered, rebased, restored
from blender.bpy.render.compositor.layers import (
    Adjustment,
    Copied,
    Edit,
    EffectAt,
    EffectMaskAt,
    EFFECTS,
    ENTRY_BLEND,
    LAYER_BLEND,
    LayerAt,
    LayerEdit,
    LayerMaskAt,
    LayerRows,
    LAYERS,
    LayerStack,
    MASKS,
    NodeKind,
    NodeSource,
    Order,
    OutputChoice,
    OWNERSHIP,
    PassSourced,
    planned,
    RecombinedSourced,
    RenderPass,
    SOURCE,
    Source,
    sourced,
    StackLayer,
    StackView,
    StoredLayer,
    Switched,
    Target,
    TermFiltering,
    TITLE,
)
from blender.bpy.render.compositor.masks import OPACITY
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.render.compositor.recombination import LightGroups, Recombination, Recombined
from blender.bpy.render.view_layers.passes import PassFlags, PassRequest, required
from blender.bpy.settings.attachments import Attached, Stored
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason, typed
from blender.records.fields import amended, Case, Lens

# --- [MODELS] ---------------------------------------------------------------------------


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
        held = Block.of_seq(scene.compositor_effects).choose(lambda entry: stacked_by(self.identity, entry.node_group).map(lambda tree: (entry, tree))).try_head()
        return held.to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=OWNERSHIP.key))

    def named(self, name: str) -> Outcome[bpy.types.NodeTree]:
        """Returns the owned stack of a node group name."""
        return stacked_by(self.identity, bpy.data.node_groups.get(name)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(bpy.data.node_groups), member=name))

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

    def replaced(self, context: bpy.types.Context, held: Selection[M], record: LayerStack[Source, M], active: int) -> Outcome[bool]:
        """Writes the stack and the active layer index when either changed, answering whether one did."""
        if record == held.record and active == held.active:
            return Ok(False)
        rebuilt = rooted(bpy.types.Scene, context).bind(lambda scene: self.rebuilt(scene, held.tree, record, context))
        return paired(rebuilt, rooted(bpy.types.Screen, context).bind(lambda screen: written(self.view.struct(screen), ("active",), active))).map(lambda _: True)

    def rewritten(self, context: bpy.types.Context, held: Selection[M], at: Lens, change: Callable[[object], object], active: int) -> Outcome[bool]:
        """Writes the stack with the value a lens reaches changed, answering whether the stack changed."""
        return amended(held.record, at, change).to_result_with(partial(Fault.of, Reason.ABSENT, TITLE, member=str(active))).bind(lambda record: self.replaced(context, held, record, active))

    def appended(self, context: bpy.types.Context, held: Selection[M], entry: Entry[M]) -> Outcome[bool]:
        """Appends an effect entry to the active layer, answering whether the stack changed."""
        return self.rewritten(context, held, (LAYERS, held.active, EFFECTS), lambda found: (*found, entry) if isinstance(found, tuple) else found, held.active)

    def moved(self, context: bpy.types.Context, origin: int, target: int) -> Outcome[None]:
        """Moves the layer row at one list row to another, then rebuilds the stack, its inputs following their layer keys."""

        def move(held: Selection[M]) -> Outcome[None]:
            self.layer_rows.struct(held.tree).layers.move(origin, target)
            return rooted(bpy.types.Scene, context).bind(lambda scene: self.reconciled(scene, held.tree, context)).map(lambda _: None)

        return self.selected(context).bind(move)

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

                def copied(_: object) -> Outcome[dict[Key, dict[str, str]]]:
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

        return _tree("Layer Sources").bind(read)

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

        def grouped_lights(layer: StackLayer[Source, M]) -> Outcome[StackLayer[Source, M]]:
            match layer.source:
                case Recombined(layer=name, recombination=LightGroups()) as source:
                    return (
                        Option
                        .of_optional(scene.view_layers.get(name))
                        .to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.view_layers), member=name))
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
                .bind(lambda wanted: required(scene, context, tuple(itertools.chain.from_iterable(wanted))))
                .bind(lambda _: self.sources(scene, stacked, tree))
                .bind(lambda offered: planned(stacked, self, scene, offered, tree.session_uid))
                .bind(lambda group: paired(reconciled(group, tree, self.identity), CameraFrame.of(self.identity, scene)))
                .bind(
                    lambda parts: accumulated(entered(self.identity, entry, parts[1].values) for entry in scene.compositor_effects if entry.node_group == tree).map(
                        lambda wrote: any(wrote) or any(parts[0])
                    )
                )
            )

        desired = record.map(Ok).default_with(lambda: self.read(tree))
        return desired.bind(lambda held: accumulated(map(grouped_lights, held.layers)).bind(lambda layers: built(held, msgspec.structs.replace(held, layers=layers))))

    def refreshed(self) -> Outcome[Option[str]]:
        """Reconciles each scene stack, naming the undo step when any changed."""
        return accumulated(
            self.reconciled(scene, tree, bpy.context)
            for scene in bpy.data.scenes
            for tree in dict.fromkeys(tree for entry in scene.compositor_effects for tree in stacked_by(self.identity, entry.node_group).to_list())
        ).map(lambda changed: Some(f"Refresh {TITLE}s") if any(changed) else Nothing)

    # --- [STACKS]

    def switched(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, context: bpy.types.Context) -> Outcome[None]:
        """Places a stack in the head entry, creating the entry when absent."""
        with ExitStack() as rollback:
            placed = (
                self
                .head(scene)
                .to_option()
                .map(lambda found: written(found[0], ("node_group",), tree).bind(lambda _: relocated(scene, found[0], 0)).map(lambda _: None))
                .default_with(lambda: added_entry(rollback, scene, tree, name=Some(TITLE), at=Some(0)).map(lambda _: None))
            )
            return retained(placed.bind(lambda _: self.reconciled(scene, tree, context)).map(lambda _: None), rollback)

    def made(self, scene: bpy.types.Scene, context: bpy.types.Context) -> Outcome[bpy.types.CompositorNodeTree]:
        """Creates an empty stack and places it in the head entry."""
        return OWNERSHIP.made(self.identity, lambda: _tree(TITLE), of=Some(scene)).bind(
            lambda group: self.store.write(group, {}).bind(lambda _: self.switched(scene, group, context)).map(lambda _: group)
        )

    def inserted(self, context: bpy.types.Context, layers: tuple[Callable[[bpy.types.NodeTree, Key], Outcome[StackLayer[Source, M]]], ...]) -> Outcome[bool]:
        """Inserts layers after the active layer under fresh keys, each built under its key with any source node it places, answering whether any was given."""

        def insert(held: Selection[M]) -> Outcome[bool]:
            keys, at = tuple(itertools.islice(held.record.fresh(), len(layers))), min(held.active + 1, len(held.record.layers))

            def dropped(key: Key) -> None:
                for node in Option.of_optional(owned(held.tree, self.identity).get(item_key(key, SOURCE))).to_list():
                    held.tree.nodes.remove(node)

            with ExitStack() as rollback:
                for key in keys:
                    rollback.callback(dropped, key)
                added = accumulated(layer(held.tree, key) for layer, key in zip(layers, keys))
                return retained(
                    added.bind(
                        lambda made: self.replaced(
                            context, held, msgspec.structs.replace(held.record, layers=(*held.record.layers[:at], *made, *held.record.layers[at:])), at + len(made) - 1 if made else held.active
                        )
                    ),
                    rollback,
                )

        return self.selected(context).bind(insert)

    def _copied(self, source: Option[bpy.types.NodeTree], layer: StackLayer[Source, M], tree: bpy.types.NodeTree, key: Key) -> Outcome[StackLayer[Source, M]]:
        """Copy of a layer under key with the settings of its source node in `source`, a layer whose source no node holds where no tree is given."""
        copy = msgspec.structs.replace(layer, key=key)
        return accumulated(
            Snapshot.capture(node).bind(lambda snapshot: created(tree, type(node)).bind(lambda made: snapshot.apply(made).outcome.bind(lambda _: Mark(key=copy.node).write(self.identity, made))))
            for held in source.to_list()
            for node in Option.of_optional(owned(held, self.identity).get(layer.node)).to_list()
        ).map(lambda _: copy)

    def _noded(self, kind: type[bpy.types.Node], setup: Callable[[bpy.types.Node], Outcome[object]], tree: bpy.types.NodeTree, key: Key) -> Outcome[StackLayer[Source, M]]:
        """Layer of a new owned source node set up by `setup`, reading the node's first enabled output."""

        def first(node: bpy.types.Node) -> Outcome[StackLayer[Source, M]]:
            output = Block.of_seq(node.outputs).filter(lambda socket: socket.enabled).try_head().to_result_with(partial(Fault.of, Reason.ABSENT, kind.__name__, member="outputs"))
            return output.map(lambda socket: StackLayer(key=key, name=node.bl_label, source=NodeSource(node=kind.__name__, output=socket.name)))

        return created(tree, kind).bind(lambda node: Mark(key=item_key(key, SOURCE)).write(self.identity, node).bind(lambda _: setup(node)).bind(lambda _: first(node)))

    # --- [COMMANDS]

    def new_stack(self, call: Call[Empty]) -> Outcome[Answer]:
        """Create layer stack."""
        return rooted(bpy.types.Scene, call.context).bind(lambda scene: self.made(scene, call.context)).map(lambda _: Done())

    def duplicate_stack(self, call: Call[Empty]) -> Outcome[Answer]:
        """Copy stack with source settings."""
        return (
            rooted(bpy.types.Scene, call.context)
            .bind(
                lambda scene: self.head(scene).bind(
                    lambda held: OWNERSHIP.made(self.identity, lambda: typed(bpy.types.CompositorNodeTree, held[1].name, held[1].copy()), of=Some(scene)).bind(
                        lambda copy: self.switched(scene, copy, call.context)
                    )
                )
            )
            .map(lambda _: Done())
        )

    def remove_stack(self, call: Call[Empty]) -> Outcome[Answer]:
        """Remove stack and its effect entry."""

        def remove(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree) -> Done:
            bpy.data.node_groups.remove(tree, do_unlink=True)
            scene.compositor_effects.remove(entry)
            return Done()

        return rooted(bpy.types.Scene, call.context).bind(lambda scene: self.head(scene).map(lambda held: remove(scene, *held)))

    def switch_stack(self, call: Call[Switched]) -> Outcome[Answer]:
        """Edit another stack."""
        return paired(rooted(bpy.types.Scene, call.context), self.named(call.properties.stack)).bind(lambda parts: self.switched(*parts, call.context)).map(lambda _: Done())

    def edit_nested_stack(self, call: Call[Empty]) -> Outcome[Answer]:
        """Edit selected layer's stack."""
        inner = self.selected(call.context).bind(
            lambda held: held.layer.bind(lambda layer: nested_tree(self.identity, held.tree, layer).to_result_with(partial(Fault.of, Reason.ABSENT, TITLE, member=layer.node)))
        )
        return paired(rooted(bpy.types.Scene, call.context), inner).bind(lambda parts: self.switched(*parts, call.context)).map(lambda _: Done())

    def clear_layers(self, call: Call[Empty]) -> Outcome[Answer]:
        """Remove every layer."""
        wrote = self.selected(call.context).bind(lambda held: self.replaced(call.context, held, msgspec.structs.replace(held.record, layers=()), 0))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason="Stack holds no layers"))

    def copy_layer(self, call: Call[Copied]) -> Outcome[Answer]:
        """Copy one layer of another stack."""

        def chosen(group: bpy.types.NodeTree, record: LayerStack[Source, M]) -> Outcome[bool]:
            layer = Block.of_seq(record.layers).filter(lambda held: held.key == call.properties.layer).try_head()
            return layer.to_result_with(partial(Fault.of, Reason.ABSENT, group.name, member=call.properties.layer)).bind(
                lambda picked: self.inserted(call.context, (partial(self._copied, Some(group), picked),))
            )

        return self.named(call.properties.stack).bind(
            lambda group: self.read(group).bind(lambda record: chosen(group, record)).map(lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers"))
        )

    def copy_all_layers(self, call: Call[Switched]) -> Outcome[Answer]:
        """Copy layers of another stack with source settings."""
        return self.named(call.properties.stack).bind(
            lambda group: self.read(group).bind(
                lambda record: self.inserted(call.context, tuple(partial(self._copied, Some(group), layer) for layer in record.layers)).map(
                    lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers")
                )
            )
        )

    def add_adjustment_layer(self, call: Call[Empty]) -> Outcome[Answer]:
        """Add layer processing composite beneath."""
        return self.inserted(call.context, (partial(self._copied, Nothing, StackLayer[Source, M](key="", name=bpy.path.display_name(item_key(Adjustment)), source=Adjustment())),)).map(
            lambda _: Done()
        )

    def add_render_pass_layer(self, call: Call[PassSourced]) -> Outcome[Answer]:
        """Add view layer output."""
        source = RenderPass(layer=call.properties.layer, output=call.properties.output)
        return self.inserted(call.context, (partial(self._copied, Nothing, StackLayer[Source, M](key="", name=f"{source.layer} {source.output}", source=source)),)).map(lambda _: Done())

    def add_recombined_layer(self, call: Call[RecombinedSourced]) -> Outcome[Answer]:
        """Add beauty from view layer light."""
        layer, picked = call.properties.layer, call.properties.recombination
        recombination = catch(msgspec.convert, exception=msgspec.ValidationError)({"kind": picked}, Recombination.__value__).map_error(partial(Fault.of, Reason.REFUSED, picked))
        return recombination.bind(
            lambda chosen: self.inserted(call.context, (partial(self._copied, Nothing, StackLayer[Source, M](key="", name=layer, source=Recombined(layer=layer, recombination=chosen))),))
        ).map(lambda _: Done())

    def add_node_layer(self, call: Call[NodeKind]) -> Outcome[Answer]:
        """Add source node at first enabled output."""
        return subclass(bpy.types.Node, call.properties.node).bind(lambda kind: self.inserted(call.context, (partial(self._noded, kind, Ok),))).map(lambda _: Done())

    def add_nested_stack(self, call: Call[Switched]) -> Outcome[Answer]:
        """Add stack as source."""
        return (
            self
            .named(call.properties.stack)
            .bind(lambda group: self.inserted(call.context, (partial(self._noded, bpy.types.CompositorNodeGroup, lambda node: grouped(node, group)),)))
            .map(lambda _: Done())
        )

    def add_media(self, call: Call[Empty]) -> Outcome[Answer]:
        """Add image or movie files."""
        images = accumulated(loaded(str(path), Tag()) for path in call.paths)
        return images.bind(
            lambda found: self.inserted(call.context, tuple(partial(self._noded, bpy.types.CompositorNodeImage, partial(written, path=("image",), value=image)) for image in found))
        ).map(lambda _: Done())

    def select_output(self, call: Call[OutputChoice]) -> Outcome[Answer]:
        """Set layer source output."""
        index, output = call.properties.layer, call.properties.output

        def chosen(held: Selection[M], layer: StackLayer[Source, M]) -> Outcome[bool]:
            match layer.source:
                case RenderPass() | NodeSource() as source:
                    return self.rewritten(call.context, held, (LAYERS, index, SOURCE), lambda _: msgspec.structs.replace(source, output=output), index)
                case _:
                    return Ok(False)

        wrote = self.selected(call.context).bind(lambda held: held.record.layer(index).bind(lambda layer: chosen(held, layer)))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason=f"Layer reads {output}"))

    def add_mask(self, call: Call[LayerEdit]) -> Outcome[Answer]:
        """Add mask to layer or effect blend."""
        return (
            paired(_targeted(call.properties), self.selected(call.context))
            .bind(lambda parts: parts[1].record.masked(parts[0], 1).bind(lambda record: self.replaced(call.context, parts[1], record, parts[0].layer)))
            .map(lambda _: Done())
        )

    def edit_layer(self, call: Call[LayerEdit]) -> Outcome[Answer]:
        """Remove, duplicate, move, or reset a layer, effect, or mask, its input values following it."""
        identity, context, edit = self.identity, call.context, Edit[call.properties.action]

        def structural(scene: bpy.types.Scene, held: Selection[M], target: Target) -> Outcome[tuple[bool, Order]]:
            done = held.record.edited(edit, target, lambda record: self.rows.offering(type(record)).bind(lambda row: row.seeded(scene)))
            return done.bind(lambda parts: self.replaced(context, held, parts[0], parts[1]).map(lambda changed: (changed, parts[2])))

        def carried(scene: bpy.types.Scene, held: Selection[M], target: Target, base: Key, count: int) -> Outcome[bool]:
            def placed(captures: tuple[Snapshot, ...], new: int, old: Option[int]) -> Outcome[bool]:
                at = item_key(base, new + 1)
                return old.map(lambda prior: restored(identity, held.entry, rebased(captures[prior], item_key(base, prior + 1), at))).default_with(lambda: defaulted(identity, held.entry, at))

            def moved(captures: tuple[Snapshot, ...], done: tuple[bool, Order]) -> Outcome[bool]:
                return accumulated(placed(captures, new, old) for new, old in enumerate(done[1]) if old != Some(new)).map(lambda wrote: done[0] or any(wrote))

            captures = accumulated(captured(identity, held.entry, item_key(base, position + 1)) for position in range(count))
            return captures.bind(lambda found: structural(scene, held, target).bind(lambda done: moved(found, done)))

        def duplicated(held: Selection[M], layer: StackLayer[Source, M]) -> Outcome[bool]:
            def copied(values: Snapshot, after: Selection[M]) -> Outcome[bool]:
                return after.layer.bind(lambda made: restored(identity, after.entry, rebased(values, layer.key, made.key)))

            return captured(identity, held.entry, layer.key).bind(
                lambda values: self.inserted(context, (partial(self._copied, Some(held.tree), layer),)).bind(
                    lambda changed: self.selected(context).bind(lambda after: copied(values, after)).map(lambda _: changed)
                )
            )

        def applied(scene: bpy.types.Scene, held: Selection[M], target: Target) -> Outcome[bool]:
            within = held.record.layer
            match target:
                case LayerAt(layer=index) if edit is Edit.DUPLICATE:
                    return within(index).bind(lambda layer: duplicated(held, layer))
                case LayerAt(layer=index) if edit is Edit.RESET:
                    return within(index).bind(lambda layer: structural(scene, held, target).bind(lambda done: defaulted(identity, held.entry, layer.key).map(lambda wrote: done[0] or wrote)))
                case LayerAt():
                    return structural(scene, held, target).map(lambda done: done[0])
                case EffectAt(layer=index):
                    return within(index).bind(lambda layer: carried(scene, held, target, item_key(layer.key, EFFECTS), len(layer.effects)))
                case LayerMaskAt(layer=index):
                    return within(index).bind(lambda layer: carried(scene, held, target, item_key(layer.key, LAYER_BLEND, MASKS), layer.blend.masks))
                case EffectMaskAt(layer=index, entry=entry):
                    blend = held.record.blended(target).to_result_with(partial(Fault.of, Reason.ABSENT, TITLE, member=str(entry)))
                    return paired(within(index), blend).bind(lambda parts: carried(scene, held, target, item_key(parts[0].key, EFFECTS, entry + 1, ENTRY_BLEND, MASKS), parts[1].masks))

        return (
            paired(rooted(bpy.types.Scene, context), self.selected(context), _targeted(call.properties))
            .bind(lambda parts: applied(*parts))
            .map(lambda changed: Done() if changed else Unchanged(reason=f"{bpy.path.display_name(edit.name.lower())} changes nothing"))
        )

    def add_effect_to_layer(self, call: Call[Choice]) -> Outcome[Answer]:
        """Add the chosen effect to the selected layer."""
        row = Option.of_optional(self.rows.keyed.get(call.properties.kind)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(tuple(self.rows.keyed)), member=call.properties.kind))
        return (
            paired(row.bind(lambda found: rooted(bpy.types.Scene, call.context).bind(found.seeded)), self.selected(call.context))
            .bind(lambda parts: self.appended(call.context, parts[1], Entry(record=parts[0])))
            .map(lambda _: Done())
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _tree(name: str) -> Outcome[bpy.types.CompositorNodeTree]:
    """Creates an empty compositor node group."""
    return typed(bpy.types.CompositorNodeTree, name, bpy.data.node_groups.new(name, "CompositorNodeTree"))


def _targeted(properties: LayerEdit) -> Outcome[Target]:
    """Decodes the target case an edit's properties name."""
    return catch(msgspec.convert, exception=msgspec.ValidationError)(properties, Target.__value__, from_attributes=True).map_error(partial(Fault.of, Reason.REFUSED, repr(properties)))


def stacked_by(identity: Identity, tree: bpy.types.NodeTree | None, /) -> Option[bpy.types.NodeTree]:
    """Returns a node group when it holds the stack ownership mark."""
    return Option.of_optional(tree).filter(lambda group: OWNERSHIP.marked(identity, group).is_some())


def nested_tree[M: Case](identity: Identity, tree: bpy.types.NodeTree, layer: StackLayer[Source, M]) -> Option[bpy.types.NodeTree]:
    """Returns the owned stack of a layer's source group node."""
    match owned(tree, identity).get(layer.node):
        case bpy.types.CompositorNodeGroup(node_tree=inner):
            return stacked_by(identity, inner)
        case _:
            return Nothing


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LayerStacking", "Selection", "nested_tree", "stacked_by"]
