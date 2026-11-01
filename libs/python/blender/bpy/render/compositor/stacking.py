"""Layer stacks held in scene compositor entries: the stack service selecting, reading, editing, and persisting each stack with its input values, and the node group it plans with every layer and composite filed to one multilayer data frame."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
import functools
from functools import partial
import graphlib
import itertools
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.data.ids.ownership import retained
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.data.rna.paths import rooted, subclass
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes import functions
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.desired import End, FrameNode, Graph, INPUTS, item_key, Members, OUTPUTS
from blender.bpy.nodes.functions import lowered
from blender.bpy.nodes.graphs import Expr, group_input, math, switched
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, NodeGroup
from blender.bpy.nodes.trees import DATA_TYPES
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, with_alpha
from blender.bpy.render.compositor.coordinates import FrameSpace
from blender.bpy.render.compositor.effects import CompositorEffect, Entry
from blender.bpy.render.compositor.entries import EffectRows
from blender.bpy.render.compositor.inputs import CameraFrame, entered, FRAME, FRAME_ASPECT, FRAME_SCALE, restored
from blender.bpy.render.compositor.layers import (
    Adjustment,
    AFFINE,
    EFFECTS,
    ENTRY_BLEND,
    LAYER_BLEND,
    LayerRows,
    LAYERS,
    LayerStack,
    MASKS,
    MATTE,
    NodeSource,
    OWNERSHIP,
    RenderPass,
    SOURCE,
    stack_file,
    StackLayer,
    StackView,
    StoredLayer,
    TermFiltering,
    TITLE,
)
from blender.bpy.render.compositor.masks import mixed_back, MODE, OPACITY
from blender.bpy.render.compositor.placement import added_entry, relocated
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.render.compositor.recombination import LightGroups, Recombined
from blender.bpy.render.outputs.formats import FileItem, FileItems, OutputFormat
from blender.bpy.render.outputs.paths import Pathed
from blender.bpy.render.view_layers.passes import PassFlags, PassRequest, required
from blender.bpy.render.view_layers.sources import lobe
from blender.bpy.settings.attachments import Attached, Stored
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason, typed
from blender.records.fields import amended, Case, Lens
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type _Unit = tuple[Expr, Option[Expr], tuple[tuple[str, Expr], ...], InterfacePanel]

# --- [CONSTANTS] ------------------------------------------------------------------------

_AFFINE: Final[Key] = "affine"

# --- [MODELS] ---------------------------------------------------------------------------


class Selection[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Head entry, its owned stack and record, and the screen's active layer index."""

    entry: bpy.types.SceneCompositorEffect
    tree: bpy.types.NodeTree
    record: LayerStack[M]
    active: int

    @property
    def layer(self) -> Outcome[StackLayer[M]]:
        """Active layer."""
        return self.record.layer(self.active)


# --- [SERVICES] -------------------------------------------------------------------------


class LayerStacking[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Layer stacks bound to one extension: the offered effects and term filter while one is declared, the store of each layer's structure by key, the layer rows owning each layer's name, flags, and order, the view attachment, and pass flags."""

    identity: Identity
    rows: EffectRows[M]
    filtering: Option[TermFiltering[M]]
    store: Stored[dict[Key, StoredLayer[M]]]
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

    def read(self, tree: bpy.types.NodeTree) -> Outcome[LayerStack[M]]:
        """Joins the layer rows, which own each layer's name, flags, and order, with the structure the group stores under each row's key, an empty stack before the first store."""
        rows = tuple(self.layer_rows.struct(tree).layers)

        def joined(stored: dict[Key, StoredLayer[M]]) -> LayerStack[M]:
            return LayerStack(
                layers=tuple(
                    StackLayer(key=row.key, name=row.name, visible=row.visible, solo=row.solo, matte=row.matte, source=part.source, effects=part.effects, blend=part.blend)
                    for row in rows
                    for part in Option.of_optional(stored.get(row.key)).to_list()
                )
            )

        return self.store.read(tree).map(lambda held: joined(held.default_value({})))

    def written(self, tree: bpy.types.NodeTree, record: LayerStack[M]) -> Outcome[None]:
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

    def rebuilt(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, record: LayerStack[M], context: bpy.types.Context) -> Outcome[None]:
        """Writes the stack, then rebuilds its graph."""
        return self.written(tree, record).bind(lambda _: self.reconciled(scene, tree, context, record=Some(record))).map(lambda _: None)

    def replaced(self, context: bpy.types.Context, held: Selection[M], record: LayerStack[M], active: int) -> Outcome[bool]:
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

    def looked(self, scene: bpy.types.Scene, preset: Valued[LayerStack[M]], strength: int, context: bpy.types.Context) -> Outcome[bool]:
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

    def applied(self, context: bpy.types.Context, value: Valued[LayerStack[M]] | Valued[StackLayer[M]] | Valued[Entry[M]]) -> Outcome[bool]:
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

    def sources(self, scene: bpy.types.Scene, record: LayerStack[M], tree: bpy.types.NodeTree) -> Outcome[dict[Key, dict[str, str]]]:
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

    def reconciled(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, context: bpy.types.Context, *, record: Option[LayerStack[M]] = Nothing) -> Outcome[bool]:
        """Enables the passes each source and the effect a term filter matches read, builds the graph, and writes frame inputs, answering whether any changed."""

        def requests(row: CompositorEffect[M], record: M, name: str) -> Outcome[tuple[PassRequest, ...]]:
            return row.request.map(lambda request: request(record, name).map(lambda held: (held,))).default_value(Ok(()))

        def requested(layer: StackLayer[M]) -> Outcome[tuple[PassRequest, ...]]:
            match layer.source:
                case RenderPass(layer=name):
                    return Ok((PassRequest(layer=name),))
                case Recombined(layer=name) as source:
                    filtered = self.filtering.bind(lambda filtering: filtering.matched(layer)).map(lambda found: self.rows.offering(type(found[1])).bind(lambda row: requests(row, found[1], name)))
                    return filtered.default_value(Ok(())).map(lambda extra: (source.requested(self.flags), *extra))
                case Adjustment() | NodeSource():
                    return Ok(())

        def grouped_lights(layer: StackLayer[M]) -> Outcome[StackLayer[M]]:
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

        def built(desired: LayerStack[M], stacked: LayerStack[M]) -> Outcome[bool]:
            return (
                (Ok(None) if stacked == desired else self.written(tree, stacked))
                .bind(lambda _: accumulated(map(requested, stacked.layers)))
                .bind(lambda wanted: required(scene, context, tuple(itertools.chain.from_iterable(wanted))))
                .bind(lambda _: self.sources(scene, stacked, tree))
                .bind(lambda offered: self.planned(scene, stacked, offered, tree.session_uid))
                .bind(lambda group: paired(reconciled(group, tree, self.identity), CameraFrame.of(self.identity, scene)))
                .bind(
                    lambda parts: accumulated(entered(self.identity, entry, parts[1].values) for entry in scene.compositor_effects if entry.node_group == tree).map(
                        lambda wrote: any(wrote) or any(parts[0])
                    )
                )
            )

        desired = record.map(Ok).default_with(lambda: self.read(tree))
        return desired.bind(lambda held: accumulated(map(grouped_lights, held.layers)).bind(lambda layers: built(held, msgspec.structs.replace(held, layers=layers))))

    def planned(self, scene: bpy.types.Scene, record: LayerStack[M], offered: Mapping[Key, Mapping[str, str]], uid: int) -> Outcome[NodeGroup]:
        """Builds every layer and its matte even when its composite is hidden, each layer reading the composite and coverage of the layers beneath, its nodes in its own frame, every layer and composite filed to one multilayer data frame."""
        config, shown = PyOpenColorIO.GetCurrentConfig(), frozenset(layer.key for layer, _ in record.composited())
        frame = {socket.key: group_input(socket.key) for socket in FRAME}
        aspect, scale = frame[FRAME_ASPECT.key], frame[FRAME_SCALE.key]
        ramp = Expr(kind=bpy.types.ShaderNodeTexGradient, members={"gradient_type": "LINEAR"}, socket="Color", shape=bpy.types.NodeSocketColor)
        canvas = with_alpha(ramp, 0.0, masked=True).named("canvas")

        def unit(layer: StackLayer[M], below: Expr, base: Expr, luminance: NodeGroup, lobe: NodeGroup) -> Outcome[_Unit]:
            layer_key = layer.key

            def outer(*parts: object) -> Expr:
                return group_input(item_key(layer_key, *parts))

            def entering() -> Outcome[Expr]:
                sockets = offered.get(layer.node, {})
                match layer.source:
                    case Adjustment():
                        return Ok(Expr(kind=bpy.types.NodeReroute, inputs=(("Input", base),), socket="Output", shape=bpy.types.NodeSocketColor))
                    case RenderPass(output=output) | NodeSource(output=output) as source:
                        found = Option.of_optional(sockets.get(output)).to_result_with(partial(Fault.of, Reason.ABSENT, layer.node, member=output))
                        return paired(found, sourced(layer_key, source, scene)).map(lambda parts: parts[1].at(parts[0], bpy.types.NodeSocketColor))
                    case Recombined() as source:

                        def inner(field: Key) -> Expr:
                            return group_input(item_key(layer_key, SOURCE, field))

                        filtered = self.filtering.map(lambda filtering: filtering.built(layer, source, luminance)).default_value(Ok(Nothing))
                        return paired(filtered, sourced(layer_key, source, scene)).bind(lambda parts: source.recombined(inner, parts[1], scene, sockets, self.flags, lobe, parts[0]))

            def grouped(prefix: Key, entry: Entry[M]) -> Outcome[tuple[Key, CompositorEffect[M], NodeGroup]]:
                return self.rows.offering(type(entry.record)).bind(lambda row: row.grouped(self.identity, scene, entry, config).map(lambda group: (prefix, row, group)))

            def chained(source: Expr, entries: tuple[tuple[Key, CompositorEffect[M], NodeGroup], ...]) -> Outcome[Expr]:
                def step(done: Outcome[Expr], held: tuple[Key, CompositorEffect[M], NodeGroup]) -> Outcome[Expr]:
                    prefix, _, group = held
                    nodes = {node: value for node, (value, _) in group.graph.placed.items() if node not in {INPUTS, OUTPUTS}}
                    feeds = {node: tuple(link for link in group.graph.links if link.target.node == node) for node in (*nodes, OUTPUTS)}
                    order = tuple(graphlib.TopologicalSorter({node: {link.source.node for link in feeds[node] if link.source.node != INPUTS} for node in nodes}).static_order())

                    def inlined(entering: Expr) -> Outcome[Expr]:
                        def read(exprs: Mapping[Key, Expr], end: End) -> Expr:
                            match end:
                                case End(node=node, socket=socket) if node != INPUTS:
                                    return exprs[node].at(socket, exprs[node].shape)
                                case End(socket=COLOR_INPUT.key):
                                    return entering
                                case End(socket=str(socket)) if socket in frame:
                                    return frame[socket]
                                case End(socket=socket):
                                    return group_input(item_key(prefix, str(socket)))

                        def expressed(exprs: Mapping[Key, Expr], node: Key) -> Mapping[Key, Expr]:
                            value = nodes[node]
                            inputs = (*value.inputs.items(), *((link.target.socket, read(exprs, link.source)) for link in feeds[node]))
                            return {**exprs, node: Expr(kind=value.kind, inputs=inputs, members=value.members, content=value.content, label=Some(f"{prefix}_{node}"))}

                        seed: Mapping[Key, Expr] = {}
                        exprs = functools.reduce(expressed, order, seed)
                        result = Block.of_seq(feeds[OUTPUTS]).filter(lambda link: link.target.socket == COLOR_RESULT.key).try_head().map(lambda link: link.source)
                        return result.to_result_with(partial(Fault.of, Reason.ABSENT, group.name, member=COLOR_RESULT.key)).map(lambda end: read(exprs, end).keeping(*exprs.values()))

                    return done.bind(lambda entering: inlined(entering).map(lambda result: switched(group_input(prefix), entering, result)))

                ordered = sorted(entries, key=lambda held: (self.rows.rank(held[1]), held[0]))
                return functools.reduce(step, ordered, Ok(source)).map(lambda chain: switched(outer(EFFECTS), source, chain))

            def covered(original: Expr, content: Expr) -> Outcome[tuple[Expr, Option[Expr], tuple[tuple[str, Expr], ...]]]:
                keyed = masks.coverage(layer.blend.masks, original, aspect, scale, lambda inner: outer(LAYER_BLEND, MASKS, inner), config)
                matted = (
                    masks.matte(luminance).map(
                        lambda group: Some(functions.placed(group, masks.relayed(lambda inner: below if inner == COLOR_INPUT.key else outer(MATTE, inner), group.interface), masks.KEY.key))
                    )
                    if layer.matte
                    else Ok(Nothing)
                )

                def weighed(factors: tuple[Expr, ...]) -> Outcome[tuple[Expr, Option[Expr], tuple[tuple[str, Expr], ...]]]:
                    mask = Option.of_optional(functools.reduce(lambda held, factor: held * factor, factors) if factors else None)
                    opacity = outer(LAYER_BLEND, OPACITY.key)
                    weight = mask.map(lambda held: math("MULTIPLY", opacity, held)).default_value(opacity)
                    items = ((layer_key, content), *((f"{layer_key} Mask", held) for held in mask.to_list()))
                    mixing = mixed_back(layer_key, base, content, outer(LAYER_BLEND, MODE.key), weight).map(Some) if layer_key in shown else Ok(Nothing)
                    return mixing.map(lambda mixed: (with_alpha(content, weight, masked=True), mixed, items))

                return paired(keyed, matted).bind(lambda parts: weighed((*parts[0].to_list(), *parts[1].to_list())))

            def transformed(content: Expr) -> Expr:
                space = FrameSpace.of(content, aspect, scale)
                return space.placed(content, space.placement(*(outer(_AFFINE, socket.key) for socket in AFFINE.inputs)))

            def faced(entries: tuple[tuple[Key, CompositorEffect[M], NodeGroup], ...]) -> InterfacePanel:
                def panel(part: Key, face: Interface, toggle: Option[bool] = Nothing) -> InterfacePanel:
                    return InterfacePanel.of(item_key(layer_key, part), face, name=bpy.path.display_name(part), toggle=toggle)

                def entry_panel(prefix: Key, row: CompositorEffect[M], group: NodeGroup) -> InterfacePanel:
                    face = Interface(inputs=tuple(socket for socket in group.interface.inputs if socket.key not in {COLOR_INPUT.key, *frame}), panels=group.interface.panels)
                    return InterfacePanel.of(prefix, face, name=row.title, toggle=Some(True))

                sourcing = (panel(item_key(SOURCE), layer.source.interface),) if isinstance(layer.source, Recombined) else ()
                effects = msgspec.structs.replace(panel(item_key(EFFECTS), Interface(), Some(True)), panels=tuple(itertools.starmap(entry_panel, entries)))
                matting = (panel(item_key(MATTE), masks.TRACK_MATTE),) if layer.matte else ()
                return InterfacePanel(key=layer_key, name=layer.name, panels=(*sourcing, effects, panel(_AFFINE, AFFINE), panel(item_key(LAYER_BLEND), layer.blend.interface), *matting))

            def built(source: Expr, entries: tuple[tuple[Key, CompositorEffect[M], NodeGroup], ...]) -> Outcome[_Unit]:
                return chained(source, entries).map(transformed).bind(lambda content: covered(source, content)).map(lambda parts: (*parts, faced(entries)))

            return paired(entering(), accumulated(itertools.starmap(grouped, zip(layer.places, layer.effects, strict=True)))).bind(lambda parts: built(*parts))

        def folded(luminance: NodeGroup, lobe: NodeGroup) -> Outcome[tuple[tuple[_Unit, ...], Expr]]:
            def step(done: Outcome[tuple[tuple[_Unit, ...], Expr, Expr]], layer: StackLayer[M]) -> Outcome[tuple[tuple[_Unit, ...], Expr, Expr]]:
                return done.bind(lambda state: unit(layer, state[2], state[1], luminance, lobe).map(lambda made: ((*state[0], made), made[1].default_value(state[1]), made[0])))

            start: Outcome[tuple[tuple[_Unit, ...], Expr, Expr]] = Ok(((), canvas, canvas))
            return functools.reduce(step, record.layers, start).map(lambda state: (state[0], state[1]))

        def built(units: tuple[_Unit, ...], composite: Expr, output: OutputFormat) -> Outcome[NodeGroup]:
            filed: tuple[tuple[str, Expr], ...] = (
                ("Composite", composite),
                *(item for held in units for item in held[2]),
                *((f"{layer.key} Composite", held[1].default_value(composite)) for layer, held in zip(record.layers, units)),
            )
            content = FileItems(items=tuple(FileItem(name=label, socket_type=DATA_TYPES[term.shape]) for label, term in filed))
            members: Members = {Pathed.directory.path: bpy.app.tempdir, Pathed.file_name.path: stack_file(uid), **output.members(("format",))}
            files = Expr(kind=bpy.types.CompositorNodeOutputFile, inputs=tuple((stored.name, term) for stored, (_, term) in zip(content.items, filed)), members=members, content=Some(content))
            roots = tuple((held[0], *held[1].to_list(), *(term for _, term in held[2])) for held in units)

            def enframed(group: NodeGroup, claimed: tuple[frozenset[Key], ...]) -> NodeGroup:
                owner = {node: layer.key for layer, nodes in reversed(tuple(zip(record.layers, claimed))) for node in nodes}
                joined = group.graph.nodes
                frames = {
                    f"{layer.key}_frame": FrameNode(members={"label": layer.name}, nodes={node: value for node, value in joined.items() if owner.get(node) == layer.key}) for layer in record.layers
                }
                return msgspec.structs.replace(group, graph=msgspec.structs.replace(group.graph, nodes={**{node: value for node, value in joined.items() if node not in owner}, **frames}))

            backdrop = lowered(bpy.types.CompositorNodeTree, canvas).map(lambda body: frozenset(body.nodes))
            claimed = backdrop.bind(lambda shared: accumulated(lowered(bpy.types.CompositorNodeTree, *held).map(lambda body: frozenset(body.nodes) - shared - {INPUTS}) for held in roots))
            interface = Interface(inputs=(COLOR_INPUT, *FRAME), outputs=(COLOR_RESULT,), panels=tuple(held[3] for held in units))
            outputs = {COLOR_RESULT.key: composite.keeping(canvas, files, *(root for held in roots for root in held))}
            return paired(functions.built(OWNERSHIP.key, interface, outputs, bpy.types.CompositorNodeTree, name=Some(TITLE)), claimed).map(lambda parts: enframed(*parts))

        return paired(masks.working(config), lobe()).bind(lambda groups: paired(folded(*groups), OutputFormat.exr(Output.DATA, layered=True)).bind(lambda parts: built(*parts[0], parts[1])))

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


# --- [OPERATIONS] -----------------------------------------------------------------------


def sourced(key: Key, source: RenderPass | NodeSource | Recombined, scene: bpy.types.Scene) -> Outcome[Expr]:
    """Builds the source node of the layer under `key` at default settings, keyed as the layer's source node."""
    node = item_key(key, SOURCE)
    match source:
        case RenderPass(layer=name) | Recombined(layer=name):
            return Ok(Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene), "layer": name}, label=Some(node)))
        case NodeSource(node=identifier):
            return subclass(bpy.types.Node, identifier).map(lambda kind: Expr(kind=kind, label=Some(node)))


def stacked_by(identity: Identity, tree: bpy.types.NodeTree | None, /) -> Option[bpy.types.NodeTree]:
    """Returns a node group when it holds the stack ownership mark."""
    return Option.of_optional(tree).filter(lambda group: OWNERSHIP.marked(identity, group).is_some())


def nested_tree[M: Case](identity: Identity, tree: bpy.types.NodeTree, layer: StackLayer[M]) -> Option[bpy.types.NodeTree]:
    """Returns the owned stack of a layer's source group node."""
    match owned(tree, identity).get(layer.node):
        case bpy.types.CompositorNodeGroup(node_tree=inner):
            return stacked_by(identity, inner)
        case _:
            return Nothing


def _tree(name: str) -> Outcome[bpy.types.CompositorNodeTree]:
    """Creates an empty compositor node group."""
    return typed(bpy.types.CompositorNodeTree, name, bpy.data.node_groups.new(name, "CompositorNodeTree"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LayerStacking", "Selection", "nested_tree", "sourced", "stacked_by"]
