"""Layer stack commands, each an operator body over an extension's bound stacking: stacks created, copied, removed, and switched, layers added from each source, copied, cleared, and edited, and effects and masks added to the selected layer."""

from collections.abc import Callable
from contextlib import ExitStack
from functools import partial
import itertools

import bpy
from expression import Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands.operators import Answer, Call, Done, Empty, Unchanged
from rasm_bpy.data._extension.identity import Key
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.images import loaded, Tag
from rasm_bpy.data._ids.ownership import Mark, retained, tracked
from rasm_bpy.data._rna.descriptors import converted_value, written
from rasm_bpy.data._rna.paths import rooted, subclass
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_bpy.nodes._addressing import owned
from rasm_bpy.nodes._desired import item_key
from rasm_bpy.nodes._trees import created, group_set
from rasm_bpy.render._compositor.effects import Entry
from rasm_bpy.render._compositor.inputs import captured, defaulted, rebased, restored
from rasm_bpy.render._compositor.layers import (
    Adjustment,
    Copied,
    Edit,
    EffectAt,
    EffectMaskAt,
    EFFECTS,
    LayerAt,
    LayerEdit,
    LayerMaskAt,
    LAYERS,
    LayerStack,
    NodeKind,
    NodeSource,
    Order,
    OutputChoice,
    OWNERSHIP,
    PassSourced,
    RecombinedSourced,
    RenderPass,
    Source,
    StackLayer,
    StackTarget,
    Switched,
    TITLE,
)
from rasm_bpy.render._compositor.masks import Blend
from rasm_bpy.render._compositor.offered import Choice
from rasm_bpy.render._compositor.recombination import Recombination, Recombined
from rasm_bpy.render._compositor.stack_service import LayerStackService, nested_tree, Selection
from rasm_records import accumulated, Fault, Ok, Outcome, paired, Reason, typed

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [STACKS]


def new_stack(stacking: LayerStackService, call: Call[Empty]) -> Outcome[Answer]:
    """Create layer stack."""
    return rooted(bpy.types.Scene, call.context).bind(lambda scene: stacking.made(scene, call.context)).map(lambda _: Done())


def duplicate_stack(stacking: LayerStackService, call: Call[Empty]) -> Outcome[Answer]:
    """Copy stack with source settings."""

    def copied(scene: bpy.types.Scene, held: tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree]) -> Outcome[Answer]:
        with ExitStack() as rollback:
            return retained(
                typed(bpy.types.CompositorNodeTree, repr(held[1]), held[1].copy())
                .bind(lambda copy: OWNERSHIP.owned(stacking.identity, copy, of=Some(scene), scope=Some(rollback)))
                .bind(lambda owned: stacking.switched(scene, owned, call.context))
                .map(lambda _: Done()),
                rollback,
            )

    return rooted(bpy.types.Scene, call.context).bind(lambda scene: stacking.head(scene).bind(partial(copied, scene)))


def remove_stack(stacking: LayerStackService, call: Call[Empty]) -> Outcome[Answer]:
    """Remove stack and its effect entry."""

    def remove(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree) -> Done:
        bpy.data.node_groups.remove(tree, do_unlink=True)
        scene.compositor_effects.remove(entry)
        return Done()

    return rooted(bpy.types.Scene, call.context).bind(lambda scene: stacking.head(scene).map(lambda held: remove(scene, *held)))


def switch_stack(stacking: LayerStackService, call: Call[Switched]) -> Outcome[Answer]:
    """Edit another stack."""
    return paired(rooted(bpy.types.Scene, call.context), stacking.named(call.properties.stack)).bind(lambda parts: stacking.switched(*parts, call.context)).map(lambda _: Done())


def edit_nested_stack(stacking: LayerStackService, call: Call[Empty]) -> Outcome[Answer]:
    """Edit selected layer's stack."""
    inner = stacking.selected(call.context).bind(
        lambda held: held.layer.bind(lambda layer: nested_tree(stacking.identity, held.tree, layer).to_result_with(partial(Fault.of, Reason.ABSENT, TITLE, member=layer.node)))
    )
    return paired(rooted(bpy.types.Scene, call.context), inner).bind(lambda parts: stacking.switched(*parts, call.context)).map(lambda _: Done())


# --- [LAYERS]


def clear_layers(stacking: LayerStackService, call: Call[Empty]) -> Outcome[Answer]:
    """Remove every layer."""
    wrote = stacking.selected(call.context).bind(lambda held: stacking.replaced(call.context, held, msgspec.structs.replace(held.record, layers=()), 0))
    return wrote.map(lambda changed: Done() if changed else Unchanged(reason="Stack holds no layers"))


def copy_layer(stacking: LayerStackService, call: Call[Copied]) -> Outcome[Answer]:
    """Copy one layer of another stack."""

    def chosen(group: bpy.types.NodeTree, record: LayerStack) -> Outcome[bool]:
        layer = Block.of_seq(record.layers).filter(lambda held: held.key == call.properties.layer).try_head()
        return layer.to_result_with(partial(Fault.of, Reason.ABSENT, group.name, member=call.properties.layer)).bind(
            lambda picked: _inserted(stacking, call.context, (partial(_copied, stacking, group, picked),))
        )

    return stacking.named(call.properties.stack).bind(
        lambda group: stacking.read(group).bind(lambda record: chosen(group, record)).map(lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers"))
    )


def copy_all_layers(stacking: LayerStackService, call: Call[Switched]) -> Outcome[Answer]:
    """Copy layers of another stack with source settings."""
    return stacking.named(call.properties.stack).bind(
        lambda group: stacking.read(group).bind(
            lambda record: _inserted(stacking, call.context, tuple(partial(_copied, stacking, group, layer) for layer in record.layers)).map(
                lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers")
            )
        )
    )


def add_adjustment_layer(stacking: LayerStackService, call: Call[Empty]) -> Outcome[Answer]:
    """Add layer processing composite beneath."""
    return _inserted(stacking, call.context, (partial(_sourced, bpy.path.display_name(item_key(Adjustment)), Adjustment()),)).map(lambda _: Done())


def add_render_pass_layer(stacking: LayerStackService, call: Call[PassSourced]) -> Outcome[Answer]:
    """Add view layer output."""
    source = RenderPass(layer=call.properties.layer, output=call.properties.output)
    return _inserted(stacking, call.context, (partial(_sourced, f"{source.layer} {source.output}", source),)).map(lambda _: Done())


def add_recombined_layer(stacking: LayerStackService, call: Call[RecombinedSourced]) -> Outcome[Answer]:
    """Add beauty from view layer light."""
    layer, picked = call.properties.layer, call.properties.recombination
    recombination = catch(msgspec.convert, exception=msgspec.ValidationError)({"kind": picked}, Recombination.__value__).map_error(partial(Fault.of, Refusal.REFUSED, picked))
    return recombination.bind(lambda chosen: _inserted(stacking, call.context, (partial(_sourced, layer, Recombined(layer=layer, recombination=chosen)),))).map(lambda _: Done())


def add_node_layer(stacking: LayerStackService, call: Call[NodeKind]) -> Outcome[Answer]:
    """Add source node at first enabled output."""
    return subclass(bpy.types.Node, call.properties.node).bind(lambda kind: _inserted(stacking, call.context, (partial(_noded, stacking, kind, Ok),))).map(lambda _: Done())


def add_nested_stack(stacking: LayerStackService, call: Call[Switched]) -> Outcome[Answer]:
    """Add stack as source."""
    return (
        stacking
        .named(call.properties.stack)
        .bind(lambda group: _inserted(stacking, call.context, (partial(_noded, stacking, bpy.types.CompositorNodeGroup, lambda node: group_set(node, group)),)))
        .map(lambda _: Done())
    )


def add_media(stacking: LayerStackService, call: Call[Empty]) -> Outcome[Answer]:
    """Add image or movie files."""
    images = accumulated(loaded(str(path), Tag()) for path in call.paths)
    return images.bind(
        lambda found: _inserted(stacking, call.context, tuple(partial(_noded, stacking, bpy.types.CompositorNodeImage, partial(written, path=("image",), value=image)) for image in found))
    ).map(lambda _: Done())


def select_output(stacking: LayerStackService, call: Call[OutputChoice]) -> Outcome[Answer]:
    """Set layer source output."""
    index, output = call.properties.layer, call.properties.output

    def chosen(held: Selection, layer: StackLayer) -> Outcome[bool]:
        match layer.source:
            case RenderPass() | NodeSource() as source:
                return stacking.rewritten(call.context, held, object, (LAYERS, index, StackLayer.source), lambda _: msgspec.structs.replace(source, output=output), index)
            case _:
                return Ok(value=False)

    wrote = stacking.selected(call.context).bind(lambda held: held.record.layer(index).bind(lambda layer: chosen(held, layer)))
    return wrote.map(lambda changed: Done() if changed else Unchanged(reason=f"Layer reads {output}"))


def _inserted(stacking: LayerStackService, context: bpy.types.Context, layers: tuple[Callable[[ExitStack, bpy.types.NodeTree, Key], Outcome[StackLayer]], ...]) -> Outcome[bool]:
    """Inserts layers after the active layer under fresh keys, each built under its key with any source node it places, answering whether any was given."""

    def insert(held: Selection) -> Outcome[bool]:
        keys, at = tuple(itertools.islice(held.record.fresh(), len(layers))), min(held.active + 1, len(held.record.layers))

        with ExitStack() as rollback:
            added = accumulated(layer(rollback, held.tree, key) for layer, key in zip(layers, keys))
            return retained(
                added.bind(
                    lambda made: stacking.replaced(
                        context, held, msgspec.structs.replace(held.record, layers=(*held.record.layers[:at], *made, *held.record.layers[at:])), at + len(made) - 1 if made else held.active
                    )
                ),
                rollback,
            )

    return stacking.selected(context).bind(insert)


def _sourced(name: str, source: Source, _scope: ExitStack, _tree: bpy.types.NodeTree, key: Key) -> Outcome[StackLayer]:
    """Layer under key whose source no node holds."""
    return Ok(StackLayer(key=key, name=name, source=source))


def _copied(stacking: LayerStackService, source: bpy.types.NodeTree, layer: StackLayer, scope: ExitStack, tree: bpy.types.NodeTree, key: Key) -> Outcome[StackLayer]:
    """Copy of a layer of the `source` stack under key, with the settings of its source node there."""
    copy = msgspec.structs.replace(layer, key=key)
    return accumulated(
        Snapshot.capture(node).bind(
            lambda snapshot: tracked(scope, tree.nodes.remove, created(tree, type(node))).bind(
                lambda made: snapshot.apply(made).outcome.bind(lambda _: Mark(key=copy.node).write(stacking.identity, made))
            )
        )
        for node in Option.of_optional(owned(source, stacking.identity).get(layer.node)).to_list()
    ).map(lambda _: copy)


def _noded(stacking: LayerStackService, kind: type[bpy.types.Node], setup: Callable[[bpy.types.Node], Outcome[object]], scope: ExitStack, tree: bpy.types.NodeTree, key: Key) -> Outcome[StackLayer]:
    """Layer of a new owned source node set up by `setup`, reading the node's first enabled output."""

    def first(node: bpy.types.Node) -> Outcome[StackLayer]:
        output = Block.of_seq(node.outputs).filter(lambda socket: socket.enabled).try_head().to_result_with(partial(Fault.of, Reason.ABSENT, kind.__name__, member="outputs"))
        return output.map(lambda socket: StackLayer(key=key, name=node.bl_label, source=NodeSource(node=node.bl_idname, output=socket.name)))

    return tracked(scope, tree.nodes.remove, created(tree, kind)).bind(
        lambda node: Mark(key=item_key(key, StackLayer.source)).write(stacking.identity, node).bind(lambda _: setup(node)).bind(lambda _: first(node))
    )


# --- [EDITS]


def add_mask(stacking: LayerStackService, call: Call[LayerEdit]) -> Outcome[Answer]:
    """Add mask to layer or effect blend."""
    return (
        paired(_targeted(call.properties), stacking.selected(call.context))
        .bind(lambda parts: parts[1].record.masked(parts[0], 1).bind(lambda record: stacking.replaced(call.context, parts[1], record, parts[0].layer)))
        .map(lambda _: Done())
    )


def edit_layer(stacking: LayerStackService, call: Call[LayerEdit]) -> Outcome[Answer]:
    """Remove, duplicate, move, or reset a layer, effect, or mask, its input values following it."""
    identity, context, edit = stacking.identity, call.context, Edit[call.properties.action]

    def structural(scene: bpy.types.Scene, held: Selection, target: StackTarget) -> Outcome[tuple[bool, Order]]:
        done = held.record.edited(edit, target, lambda record: stacking.rows.offering(type(record)).bind(lambda row: row.seeded(scene)))
        return done.bind(lambda parts: stacking.replaced(context, held, parts[0], parts[1]).map(lambda changed: (changed, parts[2])))

    def reindexed(scene: bpy.types.Scene, held: Selection, target: StackTarget, base: Key, count: int) -> Outcome[bool]:
        def placed(captures: tuple[Snapshot, ...], new: int, old: Option[int]) -> Outcome[bool]:
            at = item_key(base, new + 1)
            return old.map(lambda prior: restored(identity, held.entry, rebased(captures[prior], item_key(base, prior + 1), at))).default_with(lambda: defaulted(identity, held.entry, at))

        def moved(captures: tuple[Snapshot, ...], done: tuple[bool, Order]) -> Outcome[bool]:
            return accumulated(placed(captures, new, old) for new, old in enumerate(done[1]) if old != Some(new)).map(lambda wrote: done[0] or any(wrote))

        captures = accumulated(captured(identity, held.entry, item_key(base, position + 1)) for position in range(count))
        return captures.bind(lambda found: structural(scene, held, target).bind(lambda done: moved(found, done)))

    def duplicated(held: Selection, layer: StackLayer) -> Outcome[bool]:
        def copied(values: Snapshot, after: Selection) -> Outcome[bool]:
            return after.layer.bind(lambda made: restored(identity, after.entry, rebased(values, layer.key, made.key)))

        return captured(identity, held.entry, layer.key).bind(
            lambda values: _inserted(stacking, context, (partial(_copied, stacking, held.tree, layer),)).bind(
                lambda changed: stacking.selected(context).bind(lambda after: copied(values, after)).map(lambda _: changed)
            )
        )

    def applied(scene: bpy.types.Scene, held: Selection, target: StackTarget) -> Outcome[bool]:
        within = held.record.layer
        match target:
            case LayerAt(layer=index) if edit is Edit.DUPLICATE:
                return within(index).bind(lambda layer: duplicated(held, layer))
            case LayerAt(layer=index) if edit is Edit.RESET:
                return within(index).bind(lambda layer: structural(scene, held, target).bind(lambda done: defaulted(identity, held.entry, layer.key).map(lambda wrote: done[0] or wrote)))
            case LayerAt():
                return structural(scene, held, target).map(lambda done: done[0])
            case EffectAt(layer=index):
                return within(index).bind(lambda layer: reindexed(scene, held, target, item_key(layer.key, EFFECTS), len(layer.effects)))
            case LayerMaskAt(layer=index):
                return within(index).bind(lambda layer: reindexed(scene, held, target, item_key(layer.key, StackLayer.blend, Blend.masks), layer.blend.masks))
            case EffectMaskAt(layer=index, entry=entry):
                blend = held.record.blended(target).to_result_with(partial(Fault.of, Reason.ABSENT, TITLE, member=str(entry)))
                return paired(within(index), blend).bind(lambda parts: reindexed(scene, held, target, item_key(parts[0].key, EFFECTS, entry + 1, Entry.blend, Blend.masks), parts[1].masks))

    return (
        paired(rooted(bpy.types.Scene, context), stacking.selected(context), _targeted(call.properties))
        .bind(lambda parts: applied(*parts))
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{bpy.path.display_name(edit.name.lower())} changes nothing"))
    )


def add_effect_to_layer(stacking: LayerStackService, call: Call[Choice]) -> Outcome[Answer]:
    """Add chosen effect to selected layer."""
    return (
        paired(stacking.rows.offering(call.properties.kind).bind(lambda found: rooted(bpy.types.Scene, call.context).bind(found.seeded)), stacking.selected(call.context))
        .bind(lambda parts: stacking.appended(call.context, parts[1], Entry(record=parts[0])))
        .map(lambda _: Done())
    )


def _targeted(properties: LayerEdit) -> Outcome[StackTarget]:
    """Decodes the target case an edit's properties name."""
    return converted_value({"kind": properties.kind, "layer": properties.layer, "entry": properties.entry, "mask": properties.mask}, StackTarget, repr(properties))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "add_adjustment_layer",
    "add_effect_to_layer",
    "add_mask",
    "add_media",
    "add_nested_stack",
    "add_node_layer",
    "add_recombined_layer",
    "add_render_pass_layer",
    "clear_layers",
    "copy_all_layers",
    "copy_layer",
    "duplicate_stack",
    "edit_layer",
    "edit_nested_stack",
    "new_stack",
    "remove_stack",
    "select_output",
    "switch_stack",
]
