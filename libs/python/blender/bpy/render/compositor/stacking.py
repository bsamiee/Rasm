"""Layer stacks held in scene compositor entries: reading, rebuilding, and persisting each stack, its commands, its section of layer rows with presets, and its layered document."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from enum import Enum
import functools
from functools import cached_property, partial
import itertools
from pathlib import Path
import re
from types import GenericAlias, MappingProxyType
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Answer, Call, Command, Done, Empty, InvokePopup, Open, Operator, Properties, Unchanged
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.ownership import committed, Mark, Ownership, Saved
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import assigned, Descriptor, root_in, rooted, Struct, subclass, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import Changes, owned, reconciled
from blender.bpy.nodes.graphs import Graph, lowered, NodeGroup
from blender.bpy.nodes.interface import addresses, item_key, slot
from blender.bpy.nodes.trees import created, grouped
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.blending import Entry, photoshop
from blender.bpy.render.compositor.effects import camera_frame, CompositorEffect, Derives, entered, framing, PRECISION
from blender.bpy.render.compositor.entries import Edge, EffectRows, Pinned, relocated
from blender.bpy.render.compositor.inputs import captured, defaulted, entry_input, KeyPath, rebased, restored, updated
from blender.bpy.render.compositor.layers import (
    Adjustment,
    collection,
    Edit,
    EffectAt,
    EffectMaskAt,
    indexed,
    Layer,
    LayerAt,
    LayerMaskAt,
    LayerStack,
    located,
    node_types,
    NodeSource,
    planned,
    Planning,
    RenderPass,
    Source,
    sourced,
    Target,
    TermFiltering,
)
from blender.bpy.render.compositor.recombination import LightGroups, Lobes, Recombined, views
from blender.bpy.render.outputs.formats import extension
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.view_layers.passes import lobe, PassFlags, PassRequest, required
from blender.bpy.settings.attachments import Attached, Attachment, Changed, Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import alerted, Divider, Member, OperatorButton, Popover, pressed, Property, Pulldown, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.templates import NodeEditors
from blender.bpy.ui.layout.values import Listing
from blender.bpy.ui.surfaces.menus import Dropdown, Menu, Submenus
from blender.imaging.color.processors import DisplayView
from blender.imaging.files import documents
from blender.imaging.files.documents import Document
from blender.imaging.files.images import Depth, Rounded, unassociated
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Coded = np.uint8 | np.uint16 | np.float32
type Rows = tuple[tuple[str, str, str], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

OWNERSHIP: Final = Ownership(kind=bpy.types.CompositorNodeTree, key="layer_stack", saved=Saved.ALWAYS, single=False)
TITLE: Final = bpy.path.display_name(OWNERSHIP.key)

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


class Switched(bpy.types.PropertyGroup):
    """Owned stack selected by node group name, its rows bound at registration."""

    stack: bpy.props.EnumProperty(name="Stack", description="Owned layer stack", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Copied(bpy.types.PropertyGroup):
    """One layer of another stack, its rows bound at registration."""

    stack: bpy.props.EnumProperty(name="Stack", description="Owned layer stack", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    layer: bpy.props.EnumProperty(name="Layer", description="Layer of the chosen stack", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class NodeKind(bpy.types.PropertyGroup):
    """Source node class to create."""

    node: bpy.props.EnumProperty(name="Node Type", description="Compositor node class of the source", items=node_types)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Addressed(bpy.types.PropertyGroup):
    """Edit of the layer, effect, or mask it targets, an effect or mask index below zero naming none."""

    action: bpy.props.EnumProperty(name="Edit", items=tuple((edit.name, bpy.path.display_name(edit.name.lower()), "", edit.value, index) for index, edit in enumerate(Edit)))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    layer: bpy.props.IntProperty(name="Layer", description="Bottom-first layer index", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    entry: bpy.props.IntProperty(name="Effect", description="Effect index within the layer", min=-1, default=-1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    mask: bpy.props.IntProperty(name="Mask", description="Mask index within the blend", min=-1, default=-1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class OutputChoice(bpy.types.PropertyGroup):
    """Enabled source output chosen for one layer."""

    layer: bpy.props.IntProperty(name="Layer", description="Bottom-first layer index", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.StringProperty(name="Output", description="Source output name")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class PassSourced(bpy.types.PropertyGroup):
    """View layer output a render pass layer reads."""

    layer: bpy.props.EnumProperty(name="View Layer", description="Source view layer", items=views)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.StringProperty(name="Pass", description="Render Layers output name", default="Image")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class RecombinedSourced(bpy.types.PropertyGroup):
    """View layer and the terms a recombined layer sums."""

    layer: bpy.props.EnumProperty(name="View Layer", description="Source view layer", items=views)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    recombination: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Sum", description="Light passes or light groups summed into beauty", items=(("LOBES", "Light Passes", Lobes.__doc__ or ""), ("LIGHT_GROUPS", "Light Groups", LightGroups.__doc__ or ""))
    )


class Valued[T](msgspec.Struct, frozen=True, kw_only=True):
    """Stack, layer, or effect structure with the group input values captured at the item keys under its prefix, a stock preset holding no values."""

    structure: T
    values: Snapshot | None = None
    prefix: Key = ""

    def rebased(self, prefix: Key) -> Option[Snapshot]:
        """Returns the captured values at the item keys under `prefix` in place of their own."""
        return Option.of_optional(self.values).map(lambda values: rebased(values, self.prefix, prefix))


class Selection[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Head entry, its owned stack and record, and the screen's active layer index."""

    entry: bpy.types.SceneCompositorEffect
    tree: bpy.types.NodeTree
    record: LayerStack[Source, M]
    active: int

    @property
    def layer(self) -> Outcome[Layer[Source, M]]:
        """Active layer."""
        return indexed(self.record.layers, self.active, TITLE)


# --- [SERVICES] -------------------------------------------------------------------------


class LayerStacking[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Layer stacks bound to one extension: stack store, view attachment, pass flags, and camera framing reads."""

    identity: Identity
    stacks: "LayerStacks[M]"
    store: Stored[LayerStack[Source, M]]
    layer_rows: Attached[LayerRows]
    view: Attached[StackView]
    flags: PassFlags
    framing: Derives[Case]

    # --- [SELECTION]

    def head(self, scene: bpy.types.Scene) -> Outcome[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree]]:
        """Returns the first entry holding an owned stack, with that stack."""
        return located(next(((entry, tree) for entry in scene.compositor_effects for tree in stacked_by(self.identity, entry).to_list()), None), repr(scene.compositor_effects), OWNERSHIP.key)

    def named(self, name: str) -> Outcome[bpy.types.NodeTree]:
        """Returns the owned stack of a node group name."""
        return located(next((tree for tree, _ in OWNERSHIP.held(self.identity) if tree.name == name), None), repr(bpy.data.node_groups), name)

    def selected(self, context: bpy.types.Context) -> Outcome[Selection[M]]:
        """Returns head entry, owned stack, record, and screen selection."""
        held = rooted(bpy.types.Scene, context).bind(self.head).bind(lambda found: self.read(found[1]).map(lambda record: (*found, record)))
        return paired(held, rooted(bpy.types.Screen, context).map(self.view.struct)).map(lambda parts: Selection(entry=parts[0][0], tree=parts[0][1], record=parts[0][2], active=parts[1].active))

    # --- [VALUES]

    def read(self, tree: bpy.types.NodeTree) -> Outcome[LayerStack[Source, M]]:
        """Reads the stack its group stores in the order of its layer rows under their names and flags, an empty stack before the first store."""

        def ordered(stored: LayerStack[Source, M]) -> LayerStack[Source, M]:
            held = {layer.key: layer for layer in stored.layers}
            return msgspec.structs.replace(
                stored,
                layers=tuple(
                    msgspec.structs.replace(held[row.key], name=row.name, visible=row.visible, solo=row.solo, matte=row.matte) for row in self.layer_rows.struct(tree).layers if row.key in held
                ),
            )

        return self.store.read(tree).map(lambda held: ordered(held.default_with(self.stacks.form)))

    def listed(self, tree: bpy.types.NodeTree, record: LayerStack[Source, M]) -> Outcome[None]:
        """Writes one layer row per layer of the stack in its order, under the layer's name and flags."""
        layers = self.layer_rows.struct(tree).layers
        for _ in range(len(layers), len(record.layers)):
            layers.add()
        for index in range(len(layers) - 1, len(record.layers) - 1, -1):
            layers.remove(index)
        return accumulated(assigned(row, {"key": layer.key, "name": layer.name, "visible": layer.visible, "solo": layer.solo, "matte": layer.matte}) for row, layer in zip(layers, record.layers)).map(
            lambda _: None
        )

    def rebuilt(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, record: LayerStack[Source, M], context: bpy.types.Context) -> Outcome[None]:
        """Stores the stack and its layer rows, then rebuilds its graph."""
        return self.store.write(tree, record).bind(lambda _: self.listed(tree, record)).bind(lambda _: self.reconciled(scene, tree, context, record=Some(record))).map(lambda _: None)

    def moved(self, context: bpy.types.Context, origin: int, target: int) -> Outcome[None]:
        """Moves the layer row at one list row to another, then rebuilds the stack, its inputs following their layer keys."""

        def move(held: Selection[M]) -> Outcome[None]:
            self.layer_rows.struct(held.tree).layers.move(origin, target)
            return rooted(bpy.types.Scene, context).bind(lambda scene: self.reconciled(scene, held.tree, context)).map(lambda _: None)

        return self.selected(context).bind(move)

    def persisted(self, context: bpy.types.Context, held: Selection[M], record: LayerStack[Source, M], active: int) -> Outcome[None]:
        """Writes the stack and the active layer index."""
        rebuilt = rooted(bpy.types.Scene, context).bind(lambda scene: self.rebuilt(scene, held.tree, record, context))
        return paired(rebuilt, rooted(bpy.types.Screen, context).bind(lambda screen: written(self.view.struct(screen), "active", active))).map(lambda _: None)

    def looked(self, scene: bpy.types.Scene, preset: Valued[LayerStack[Source, M]], strength: int, context: bpy.types.Context) -> Outcome[bool]:
        """Writes a preset's structure and input values over the head stack, every effect mixing at `strength` percent through its blend opacity, creating the stack when absent, answering whether it changed."""

        def held() -> Outcome[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree]]:
            return self.head(scene).to_option().map(Ok).default_with(lambda: self.made(scene, context).bind(lambda _: self.head(scene)))

        def placed(entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree) -> Outcome[bool]:
            layers = preset.structure.layers
            mixes: dict[Key, Value] = {item_key(layer.key, "effects", index, "blend", "opacity"): strength / 100 for layer in layers for index in range(1, len(layer.effects) + 1)}
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

    def appended(self, context: bpy.types.Context, held: Selection[M], at: KeyPath, item: object, active: int) -> Outcome[bool]:
        """Appends an item to the collection at a record path, answering whether the stack changed."""
        return collection(held.record, at).bind(lambda found: self.replaced(context, held, updated(held.record, at, lambda _: (*found, item)), active))

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

        def requested(layer: Layer[Source, M]) -> Outcome[tuple[PassRequest, ...]]:
            match layer.source:
                case RenderPass(layer=name):
                    return Ok((PassRequest(layer=name),))
                case Recombined() as source:
                    return self.stacks.filtering.requests(layer).map(lambda extra: (source.requested(self.flags), *extra))
                case Adjustment() | NodeSource():
                    return Ok(())

        def grouped(layer: Layer[Source, M]) -> Outcome[Layer[Source, M]]:
            match layer.source:
                case Recombined(layer=name, recombination=LightGroups()) as source:
                    return located(scene.view_layers.get(name), repr(scene.view_layers), name).bind(
                        lambda held: called(bpy.ops.scene.view_layer_add_used_lightgroups, {}, override=Some(context.temp_override(scene=scene, view_layer=held))).map(
                            lambda _: msgspec.structs.replace(layer, source=source.synced(tuple(held.lightgroups.keys())))
                        )
                    )
                case _:
                    return Ok(layer)

        def built(desired: LayerStack[Source, M], stacked: LayerStack[Source, M], config: PyOpenColorIO.Config) -> Outcome[bool]:
            frames = FrameSize().bound(self.identity).bind(lambda framed: camera_frame(framed, scene))

            def planning(parts: tuple[dict[Key, dict[str, str]], NodeGroup, NodeGroup]) -> Planning[M]:
                return Planning(
                    identity=self.identity,
                    scene=scene,
                    config=config,
                    luminance=parts[1],
                    rows=self.stacks.rows,
                    flags=self.flags,
                    lobe=parts[2],
                    offered=parts[0],
                    uid=tree.session_uid,
                    filtering=self.stacks.filtering,
                )

            return (
                (Ok(None) if stacked == desired else self.store.write(tree, stacked))
                .bind(lambda _: accumulated(map(requested, stacked.layers)))
                .bind(lambda wanted: required(scene, context, tuple(itertools.chain.from_iterable(wanted)), PRECISION))
                .bind(lambda _: paired(self.sources(scene, stacked, tree), masks.luminance(config), lobe()))
                .bind(lambda parts: planned(stacked, planning(parts), OWNERSHIP.key, TITLE))
                .bind(lambda group: reconciled(group, tree, self.identity))
                .bind(
                    lambda changes: frames.bind(lambda frame: accumulated(entered(self.identity, entry, frame.values) for entry in scene.compositor_effects if entry.node_group == tree)).map(
                        lambda wrote: any(wrote) or changes != Changes()
                    )
                )
            )

        desired = record.map(Ok).default_with(lambda: self.read(tree))
        return desired.bind(lambda held: accumulated(map(grouped, held.layers)).bind(lambda layers: built(held, msgspec.structs.replace(held, layers=layers), PyOpenColorIO.GetCurrentConfig())))

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
            entry = held.map(lambda found: found[0]).default_with(lambda: scene.compositor_effects.new(TITLE))
            for _ in () if held.is_some() else (entry,):
                rollback.callback(scene.compositor_effects.remove, entry)
            return committed(written(entry, "node_group", tree).bind(lambda _: relocated(scene, entry, 0)).bind(lambda _: self.reconciled(scene, tree, context)).map(lambda _: None), rollback)

    def made(self, scene: bpy.types.Scene, context: bpy.types.Context) -> Outcome[bpy.types.CompositorNodeTree]:
        """Creates an empty stack and places it in the head entry."""
        return OWNERSHIP.made(self.identity, lambda: created_tree(TITLE), of=Some(scene)).bind(
            lambda group: self.store.write(group, self.stacks.form()).bind(lambda _: self.switched(scene, group, context)).map(lambda _: group)
        )

    def inserted(self, context: bpy.types.Context, layers: tuple[Layer[Source, M], ...], source: Option[bpy.types.NodeTree] = Nothing) -> Outcome[bool]:
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
                return committed(accumulated(itertools.starmap(copy, nodes)).bind(lambda _: self.replaced(context, held, record, at + len(added) - 1 if added else held.active)), rollback)

        return self.selected(context).bind(insert)

    def added_node[T](self, context: bpy.types.Context, kind: type[bpy.types.Node], setup: Callable[[bpy.types.Node], Outcome[T]]) -> Outcome[None]:
        """Adds a source node layer after the active layer at the node's first enabled output."""

        def add(held: Selection[M]) -> Outcome[None]:
            key, at = next(held.record.fresh()), min(held.active + 1, len(held.record.layers))
            layer_node = item_key(key, "source")

            def placed(node: bpy.types.Node) -> Outcome[None]:
                return located(next((output.name for output in node.outputs if output.enabled), None), kind.__name__, "outputs").bind(
                    lambda output: self.persisted(
                        context,
                        held,
                        msgspec.structs.replace(
                            held.record, layers=(*held.record.layers[:at], Layer(key=key, name=node.bl_label, source=NodeSource(node=kind.__name__, output=output)), *held.record.layers[at:])
                        ),
                        at,
                    )
                )

            with ExitStack() as rollback:
                rollback.callback(dropped, held.tree, self.identity, layer_node)
                return committed(created(held.tree, kind).bind(lambda node: Mark(key=layer_node).write(self.identity, node).bind(lambda _: setup(node)).bind(lambda _: placed(node))), rollback)

        return self.selected(context).bind(add)

    # --- [COMMANDS]

    def new_stack(self, call: Call[Empty]) -> Outcome[Answer]:
        """Create layer stack."""
        return call.root(bpy.types.Scene).bind(lambda scene: self.made(scene, call.context)).map(lambda _: Done())

    def duplicate_stack(self, call: Call[Empty]) -> Outcome[Answer]:
        """Copy stack with source settings."""
        return (
            call
            .root(bpy.types.Scene)
            .bind(
                lambda scene: self.head(scene).bind(
                    lambda held: OWNERSHIP.made(self.identity, lambda: narrowed(held[1].copy()), of=Some(scene)).bind(lambda copy: self.switched(scene, copy, call.context))
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

        return call.root(bpy.types.Scene).bind(lambda scene: self.head(scene).map(lambda held: remove(scene, *held)))

    def switch_stack(self, call: Call[Switched]) -> Outcome[Answer]:
        """Edit another stack."""
        return paired(call.root(bpy.types.Scene), self.named(call.properties.stack)).bind(lambda parts: self.switched(*parts, call.context)).map(lambda _: Done())

    def edit_nested_stack(self, call: Call[Empty]) -> Outcome[Answer]:
        """Edit selected layer's stack."""
        inner = self.selected(call.context).bind(lambda held: held.layer.bind(lambda layer: located(nested_tree(self.identity, held.tree, layer).to_optional(), TITLE, "nested stack")))
        return paired(call.root(bpy.types.Scene), inner).bind(lambda parts: self.switched(*parts, call.context)).map(lambda _: Done())

    def clear_layers(self, call: Call[Empty]) -> Outcome[Answer]:
        """Remove every layer."""
        wrote = self.selected(call.context).bind(lambda held: self.replaced(call.context, held, msgspec.structs.replace(held.record, layers=()), 0))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason="Stack holds no layers"))

    def copy_layer(self, call: Call[Copied]) -> Outcome[Answer]:
        """Copy one layer of another stack."""

        def chosen(group: bpy.types.NodeTree, record: LayerStack[Source, M]) -> Outcome[Answer]:
            layer = located(next((held for held in record.layers if held.key == call.properties.layer), None), group.name, call.properties.layer)
            return layer.bind(lambda picked: self.inserted(call.context, (picked,), Some(group))).map(lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers"))

        return self.named(call.properties.stack).bind(lambda group: self.read(group).bind(lambda record: chosen(group, record)))

    def copy_all_layers(self, call: Call[Switched]) -> Outcome[Answer]:
        """Copy layers of another stack with source settings."""
        return self.named(call.properties.stack).bind(
            lambda group: self.read(group).bind(
                lambda record: self.inserted(call.context, record.layers, Some(group)).map(lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers"))
            )
        )

    def add_adjustment_layer(self, call: Call[Empty]) -> Outcome[Answer]:
        """Add layer processing composite beneath."""
        return self.inserted(call.context, (Layer(key="inserted", name=bpy.path.display_name(item_key(Adjustment)), source=Adjustment()),)).map(lambda _: Done())

    def add_render_pass_layer(self, call: Call[PassSourced]) -> Outcome[Answer]:
        """Add view layer output."""
        source = RenderPass(layer=call.properties.layer, output=call.properties.output)
        return self.inserted(call.context, (Layer(key="inserted", name=f"{source.layer} {source.output}", source=source),)).map(lambda _: Done())

    def add_recombined_layer(self, call: Call[RecombinedSourced]) -> Outcome[Answer]:
        """Add beauty from view layer light."""
        source = Recombined(layer=call.properties.layer, recombination=LightGroups() if call.properties.recombination == "LIGHT_GROUPS" else Lobes())
        return self.inserted(call.context, (Layer(key="inserted", name=source.layer, source=source),)).map(lambda _: Done())

    def add_node_layer(self, call: Call[NodeKind]) -> Outcome[Answer]:
        """Add source node at first enabled output."""
        return subclass(bpy.types.Node, call.properties.node).bind(lambda kind: self.added_node(call.context, kind, Ok)).map(lambda _: Done())

    def add_nested_stack(self, call: Call[Switched]) -> Outcome[Answer]:
        """Add stack as source."""
        return self.named(call.properties.stack).bind(lambda group: self.added_node(call.context, bpy.types.CompositorNodeGroup, lambda node: grouped(node, group))).map(lambda _: Done())

    def add_media(self, call: Call[Empty]) -> Outcome[Answer]:
        """Add image or movie files."""
        images = accumulated(loaded(str(path), Tag()) for path in call.paths)
        return images.bind(lambda found: accumulated(self.added_node(call.context, bpy.types.CompositorNodeImage, functools.partial(written, identifier="image", value=image)) for image in found)).map(
            lambda _: Done()
        )

    def select_output(self, call: Call[OutputChoice]) -> Outcome[Answer]:
        """Set layer source output."""
        index, at = call.properties.layer, ("layers", call.properties.layer + 1, "source", "output")

        def chosen(held: Selection[M], layer: Layer[Source, M]) -> Outcome[bool]:
            match layer.source:
                case RenderPass() | NodeSource():
                    return self.replaced(call.context, held, updated(held.record, at, lambda _: call.properties.output), index)
                case Adjustment() | Recombined():
                    return located(None, layer.name, at[-1])

        wrote = self.selected(call.context).bind(lambda held: indexed(held.record.layers, index, TITLE).bind(lambda layer: chosen(held, layer)))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason=f"Layer reads {call.properties.output}"))

    def add_mask(self, call: Call[Addressed]) -> Outcome[Answer]:
        """Add mask to layer or effect blend."""
        target = targeted(call.properties)
        return self.selected(call.context).bind(lambda held: held.record.masked(target, 1).bind(lambda record: self.replaced(call.context, held, record, target.layer))).map(lambda _: Done())

    def effected(self, row: CompositorEffect[M], call: Call[Empty]) -> Outcome[Answer]:
        """Appends the effect's seeded record to the active layer."""

        def appended(held: Selection[M], record: M) -> Outcome[bool]:
            return self.appended(call.context, held, ("layers", held.active + 1, "effects"), Entry(record=record), held.active)

        return paired(call.root(bpy.types.Scene).bind(row.seeded), self.selected(call.context)).bind(lambda parts: appended(parts[1], parts[0])).map(lambda _: Done())

    def edit_layer(self, call: Call[Addressed]) -> Outcome[Answer]:
        """Remove, duplicate, move, or reset a layer, effect, or mask, its input values following it."""
        target, identity, action = targeted(call.properties), self.identity, Edit[call.properties.action]

        def structural(scene: bpy.types.Scene, held: Selection[M]) -> Outcome[bool]:
            return held.record.edited(action, target, lambda record: self.stacks.rows.offering(type(record)).bind(lambda row: row.seeded(scene))).bind(
                lambda edit: self.replaced(call.context, held, *edit)
            )

        def order(count: int, index: int) -> tuple[Option[int], ...]:
            match action:
                case Edit.REMOVE:
                    return tuple(Some(position) for position in range(count) if position != index)
                case Edit.DUPLICATE:
                    return tuple(Some(position) for position in (*range(index + 1), index, *range(index + 1, count)))
                case Edit.UP | Edit.DOWN:
                    other = min(max(index + (1 if action is Edit.UP else -1), 0), count - 1)
                    return tuple(Some(other if position == index else index if position == other else position) for position in range(count))
                case Edit.RESET:
                    return tuple(Nothing if position == index else Some(position) for position in range(count))

        def carried(scene: bpy.types.Scene, held: Selection[M], base: Key, count: int, index: int) -> Outcome[bool]:
            def placed(captures: tuple[Snapshot, ...], new: int, old: Option[int]) -> Outcome[bool]:
                at = item_key(base, new + 1)
                return old.map(lambda prior: restored(identity, held.entry, rebased(captures[prior], item_key(base, prior + 1), at))).default_with(lambda: defaulted(identity, held.entry, at))

            def moved(captures: tuple[Snapshot, ...]) -> Outcome[bool]:
                return accumulated(placed(captures, new, old) for new, old in enumerate(order(count, index)) if old != Some(new)).map(any)

            captures = accumulated(captured(identity, held.entry, item_key(base, position + 1)) for position in range(count))
            return captures.bind(lambda found: structural(scene, held).bind(lambda changed: moved(found).map(lambda wrote: changed or wrote)))

        def duplicated(held: Selection[M], layer: Layer[Source, M]) -> Outcome[bool]:
            def copied(values: Snapshot, after: Selection[M]) -> Outcome[bool]:
                return indexed(after.record.layers, after.active, TITLE).bind(lambda made: restored(identity, after.entry, rebased(values, layer.key, made.key)))

            return captured(identity, held.entry, layer.key).bind(
                lambda values: self.inserted(call.context, (layer,), Some(held.tree)).bind(lambda changed: self.selected(call.context).bind(lambda after: copied(values, after)).map(lambda _: changed))
            )

        def applied(scene: bpy.types.Scene, held: Selection[M]) -> Outcome[bool]:
            def within(index: int) -> Outcome[Layer[Source, M]]:
                return indexed(held.record.layers, index, TITLE)

            match target:
                case LayerAt(layer=index) if action is Edit.DUPLICATE:
                    return within(index).bind(lambda layer: duplicated(held, layer))
                case LayerAt(layer=index) if action is Edit.RESET:
                    return within(index).bind(lambda layer: structural(scene, held).bind(lambda changed: defaulted(identity, held.entry, layer.key).map(lambda wrote: changed or wrote)))
                case LayerAt():
                    return structural(scene, held)
                case EffectAt(layer=index, entry=entry):
                    return within(index).bind(lambda layer: carried(scene, held, item_key(layer.key, "effects"), len(layer.effects), entry))
                case LayerMaskAt(layer=index, mask=mask):
                    return within(index).bind(lambda layer: carried(scene, held, item_key(layer.key, "blend", "masks"), layer.blend.masks, mask))
                case EffectMaskAt(layer=index, entry=entry, mask=mask):
                    return within(index).bind(
                        lambda layer: indexed(layer.effects, entry, layer.name).bind(
                            lambda effect: carried(scene, held, item_key(layer.key, "effects", entry + 1, "blend", "masks"), effect.blend.masks, mask)
                        )
                    )

        wrote = paired(call.root(bpy.types.Scene), self.selected(call.context)).bind(lambda parts: applied(*parts))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason=f"{bpy.path.display_name(action.name.lower())} changes nothing"))

    def applied(self, context: bpy.types.Context, value: Valued[LayerStack[Source, M]] | Valued[Layer[Source, M]] | Valued[Entry[M]]) -> Outcome[Answer]:
        """Applies a stack, layer, or effect preset over the selection, then the input values it captured at the keys the applied item takes."""

        def valued(prefix: Key, *, changed: bool) -> Outcome[bool]:
            return self.selected(context).bind(
                lambda after: value.rebased(prefix).map(lambda values: restored(self.identity, after.entry, values)).default_value(Ok(False)).map(lambda wrote: changed or wrote)
            )

        def edited(held: Selection[M]) -> Outcome[bool]:
            at = ("layers", held.active + 1)
            match value.structure:
                case LayerStack() as stack:
                    return self.replaced(context, held, stack, 0).bind(lambda changed: valued("", changed=changed))
                case Layer() as layer:
                    return held.layer.bind(
                        lambda prior: self.replaced(context, held, updated(held.record, at, lambda _: msgspec.structs.replace(layer, key=prior.key)), held.active).bind(
                            lambda changed: valued(prior.key, changed=changed)
                        )
                    )
                case Entry() as entry:
                    return held.layer.bind(
                        lambda prior: self.appended(context, held, (*at, "effects"), entry, held.active).bind(
                            lambda changed: valued(item_key(prior.key, "effects", len(prior.effects) + 1), changed=changed)
                        )
                    )

        return self.selected(context).bind(edited).map(lambda wrote: Done() if wrote else Unchanged(reason="Stack matches preset"))

    # --- [ROWS]

    @property
    def listings(self) -> tuple[Listing[Valued[LayerStack[Source, M]]], Listing[Valued[Layer[Source, M]]], Listing[Valued[Entry[M]]]]:
        """Stack, active layer, and last effect preset listings, each preset holding the input values at its item keys."""

        def matched[T](read: Callable[[bpy.types.Context], Outcome[Valued[T]]], context: bpy.types.Context, values: tuple[Valued[T], ...]) -> Outcome[tuple[bool, ...]]:
            def equal(held: Valued[T], value: Valued[T]) -> bool:
                return held.structure == value.structure and value.rebased(held.prefix).map(lambda values: values == held.values).default_value(True)

            return read(context).map(lambda held: tuple(equal(held, value) for value in values))

        def valued[T](held: Selection[M], structure: T, prefix: Key) -> Outcome[Valued[T]]:
            return captured(self.identity, held.entry, prefix).map(lambda values: Valued(structure=structure, values=values, prefix=prefix))

        def stack(context: bpy.types.Context) -> Outcome[Valued[LayerStack[Source, M]]]:
            return self.selected(context).bind(lambda held: valued(held, held.record, ""))

        def layer(context: bpy.types.Context) -> Outcome[Valued[Layer[Source, M]]]:
            return self.selected(context).bind(lambda held: held.layer.bind(lambda found: valued(held, found, found.key)))

        def last(context: bpy.types.Context) -> Outcome[Valued[Entry[M]]]:
            return self.selected(context).bind(
                lambda held: held.layer.bind(
                    lambda found: located(next(iter(found.effects[-1:]), None), found.name, "effects").bind(lambda entry: valued(held, entry, item_key(found.key, "effects", len(found.effects))))
                )
            )

        stacks = self.stacks
        return (
            Listing(preset=stacks.preset, current=stack, matches=partial(matched, stack), applied=self.applied),
            Listing(preset=stacks.layer_preset, current=layer, matches=partial(matched, layer), applied=self.applied),
            Listing(preset=stacks.entry_preset, current=last, matches=partial(matched, last), applied=self.applied),
        )

    # --- [EDITOR]

    @property
    def listing(self) -> ItemList[bpy.types.PropertyGroup, LayerRow]:
        """Layer list top first with visibility, solo, and matte columns and edit side buttons, dragged to reorder."""
        stacks = self.stacks
        return ItemList(
            key=f"{OWNERSHIP.key}_layers",
            item=LayerRow,
            collection=Member(
                root=bpy.types.NodeTree,
                path=f"{self.layer_rows.name}.layers",
                reach=Some(lambda context: root_in(bpy.types.Scene, context).bind(lambda scene: self.head(scene).to_option()).map(lambda held: held[1])),
            ),
            active=Member(root=bpy.types.Screen, path=f"{self.view.name}.active", reach=Some(lambda context: Option.of_optional(context.screen))),
            columns=tuple(Property(target=Member(root=LayerRow, path=path), text="") for path in ("visible", "solo", "matte")),
            enabled=Some(lambda context, row: self.selected(context).map(lambda found: any(shown.key == row.key for shown, _ in found.record.composited()))),
            sort_reverse=True,
            side=(
                Pulldown(menu=stacks.adding.key, text="", icon=Some("ADD")),
                partial(self.pressing, Edit.REMOVE),
                Divider(type="SPACE"),
                Pulldown(menu=stacks.specials.key, text="", icon=Some("DOWNARROW_HLT")),
                Divider(type="SPACE"),
                *(partial(self.pressing, action) for action in (Edit.DUPLICATE, Edit.UP, Edit.DOWN, Edit.RESET)),
            ),
            absent=(OperatorButton(target=stacks.creating),),
            move=Some(self.moved),
        )

    def pressing(self, action: Edit, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the icon button of one edit over the active layer."""
        alerted(
            layout,
            self.selected(context).bind(lambda held: pressed(layout, self.identity, self.stacks.editing, {**addressed(LayerAt(layer=held.active)), "action": action.name}, text="", icon=action.value)),
        )

    def drawn(self) -> Outcome[RowDraw]:
        """Returns the draw of the layer list, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
        return paired(self.listing.drawn(self.identity), realized(Pulldown(menu=self.stacks.effecting.key, icon=Some("ADD")), self.identity), NodeEditors.realized(self.identity)).map(
            lambda parts: _editor(self, *parts)
        )

    # --- [DOCUMENTS]

    def layered(self, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document[Coded]]:
        """Builds the document of formed color and raw mask coverage of the head stack at a saved frame, nested stacks as groups."""
        number = located(next((part.header.get("Frame") for part in frame.parts[:1]), None), frame.path, "Frame").bind(
            lambda text: catch(lambda: int(text), exception=ValueError)().map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=frame.path, cause=error),))
        )

        coded = functools.partial(depth.coded, coding=Rounded())

        def composed(
            entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree, record: LayerStack[Source, M], at: int, config: PyOpenColorIO.Config, view: DisplayView
        ) -> Outcome[Document[Coded]]:
            def read_tree(tree: bpy.types.NodeTree, record: LayerStack[Source, M], valued: Callable[[Key], Outcome[Value]]) -> Outcome[tuple[documents.Layers[Coded], npt.NDArray[np.float32]]]:
                shown = frozenset(layer.key for layer, _ in record.composited())

                def written(node: bpy.types.CompositorNodeOutputFile) -> Outcome[str]:
                    named = re.sub(r"#+", lambda run: str(at).zfill(len(run[0])), node.file_name)
                    return located(Option.of_optional(node.format).bind(extension).to_optional(), repr(node), "format").map(
                        lambda suffix: str(Path(bpy.path.abspath(node.directory)) / f"{named}{suffix}")
                    )

                path = located(
                    next((node for node in owned(tree, self.identity).values() if isinstance(node, bpy.types.CompositorNodeOutputFile)), None),
                    repr(tree.nodes),
                    bpy.types.CompositorNodeOutputFile.__name__,
                ).bind(written)

                def planes(file: Multilayer) -> Outcome[tuple[documents.Layers[Coded], npt.NDArray[np.float32]]]:
                    def raw(name: str) -> Outcome[npt.NDArray[np.float32]]:
                        return located(next((layer for layer in file.layers if layer.name == name), None), file.path, name).bind(lambda layer: file.plane(layer, layer.channels))

                    def color(name: str) -> Outcome[npt.NDArray[np.float32]]:
                        return raw(name).bind(lambda pixels: view.form(config, pixels)).map(unassociated)

                    def each(layer: Layer[Source, M]) -> Outcome[documents.Raster[Coded] | documents.Adjustment[Coded] | documents.Group[Coded]]:
                        visible = layer.key in shown
                        if isinstance(layer.source, Adjustment):
                            return color(f"{layer.key} Composite").map(lambda pixels: documents.Adjustment(name=layer.name, color=coded(pixels[..., :3]), alpha=coded(pixels[..., 3]), visible=visible))
                        mask = raw(f"{layer.key} Mask").map(lambda pixels: Some(coded(pixels[..., 0]))) if layer.blend.masks or layer.matte else Ok(Nothing)
                        blend = paired(valued(item_key(layer.key, "blend", "mode")), valued(item_key(layer.key, "blend", "opacity"))).bind(
                            lambda pair: (
                                catch(msgspec.convert, exception=msgspec.ValidationError)(pair[1], float)
                                .map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=layer.key, cause=error),))
                                .map(lambda opacity: (photoshop(str(pair[0])), opacity))
                            )
                        )
                        nested = (
                            nested_tree(self.identity, tree, layer)
                            .filter(lambda _: layer.plain)
                            .map(
                                lambda inner: paired(self.read(inner), blend).bind(
                                    lambda parts: read_tree(inner, parts[0], functools.partial(grouped_value, self.identity, inner, owned(tree, self.identity)[layer.node])).map(
                                        lambda held: documents.Group(name=layer.name, layers=held[0], mode=Some(parts[1][0]), opacity=parts[1][1], visible=visible)
                                    )
                                )
                            )
                        )
                        return nested.default_with(
                            lambda: paired(color(layer.key), mask, blend).map(
                                lambda parts: documents.Raster(
                                    name=layer.name, color=coded(parts[0][..., :3]), alpha=coded(parts[0][..., 3]), mask=parts[1], mode=parts[2][0], opacity=parts[2][1], visible=visible
                                )
                            )
                        )

                    return paired(accumulated(each(layer) for layer in record.layers), color("Composite"))

                return path.bind(Multilayer.read).bind(planes)

            density = scene.render.ppm_factor / scene.render.ppm_base
            return read_tree(tree, record, functools.partial(entry_value, self.identity, entry)).bind(
                lambda parts: located(view.colorspace(config).to_optional(), repr(config), f"{view.display}/{view.view}").map(
                    lambda space: Document(layers=parts[0], composite=coded(parts[1]), colorspace=space.getName(), density=density)
                )
            )

        def stack_view() -> Outcome[DisplayView]:
            return self.stacks.view(self.identity, scene).map(lambda held: held.default_with(lambda: viewed(scene)))

        return self.head(scene).bind(
            lambda held: self.read(held[1]).bind(
                lambda record: paired(number, stack_view()).bind(lambda parts: composed(held[0], held[1], record, parts[0], PyOpenColorIO.GetCurrentConfig(), parts[1]))
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _editor[M: Case](stacking: LayerStacking[M], listing: RowDraw, adding: RowDraw, editors: NodeEditors) -> RowDraw:
    """Layer list top first, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
    identity, stacks = stacking.identity, stacking.stacks

    def edit(layout: bpy.types.UILayout, action: Edit, target: Target) -> None:
        alerted(layout, pressed(layout, identity, stacks.editing, {**addressed(target), "action": action.name}, text="", icon=action.value))

    def edits(layout: bpy.types.UILayout, target: Target) -> None:
        line = layout.row(align=True)
        for action in Edit:
            edit(line, action, target)

    def mask(layout: bpy.types.UILayout, target: LayerAt | EffectAt) -> None:
        alerted(layout, pressed(layout, identity, stacks.masking, addressed(target), text=stacks.masking.title, icon="MOD_MASK"))

    def widgets(layout: bpy.types.UILayout, held: Selection[M], prefix: Key, inner: tuple[Key, ...], context: bpy.types.Context) -> None:
        def mine(key: Key) -> bool:
            return key.startswith(f"{prefix}_") and not any(key.startswith(f"{nested}_") for nested in inner)

        for node in (node for key, node in owned(held.tree, identity).items() if mine(key)):
            editors.drawn(layout, node, context)
        for key in (key for keys in slot(identity).map(lambda name: addresses(held.tree, name)).to_option().to_list() for key in keys if mine(key)):
            alerted(layout, entry_input(identity, held.entry, key).map(lambda owner: layout.prop(owner, "value")))

    def effected(layout: bpy.types.UILayout, held: Selection[M], at: EffectAt, place: Key, entry: Entry[M], context: bpy.types.Context) -> None:
        header, body = layout.panel(f"{place}_{held.tree.session_uid}", default_closed=False)
        header.label(text=stacks.rows.offering(type(entry.record)).map(lambda row: row.title).default_value(bpy.path.display_name(item_key(type(entry.record)))))
        edits(header, at)
        for inner in Option.of_optional(body).to_list():
            widgets(inner, held, place, (), context)
            mask(inner, at)
            for index in range(entry.blend.masks):
                edits(inner, EffectMaskAt(layer=at.layer, entry=at.entry, mask=index))

    def detail(layout: bpy.types.UILayout, held: Selection[M], layer: Layer[Source, M], context: bpy.types.Context) -> None:
        at = LayerAt(layer=held.active)
        places = tuple(item_key(layer.key, "effects", index) for index in range(1, len(layer.effects) + 1))
        for source in Option.of_optional(owned(held.tree, identity).get(layer.node)).to_list():
            source.draw_buttons_ext(context, layout)
            layout.template_node_inputs(source)
            outputs = layout.row(align=True)
            for output in (output for output in source.outputs if output.enabled):
                alerted(outputs, pressed(outputs, identity, stacks.output, {"layer": at.layer, "output": output.name}, text=output.name, icon="NONE"))
        widgets(layout, held, layer.key, places, context)
        buttons = layout.row(align=True)
        mask(buttons, at)
        edit(buttons, Edit.RESET, at)
        for index in range(layer.blend.masks):
            edits(layout, LayerMaskAt(layer=at.layer, mask=index))
        for index, entry in enumerate(layer.effects):
            effected(layout, held, EffectAt(layer=at.layer, entry=index), places[index], entry, context)

    def drawn(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        listing(layout, holder, context)
        for held in stacking.selected(context).to_option().to_list():
            for layer in held.layer.to_option().to_list():
                detail(layout, held, layer, context)
                adding(layout, holder, context)

    return drawn


def _presets[M: Case](
    stacking: LayerStacking[M], creating: Operator[Empty], listings: tuple[tuple[Listing[Valued[LayerStack[Source, M]]] | Listing[Valued[Layer[Source, M]]] | Listing[Valued[Entry[M]]], RowDraw], ...]
) -> RowDraw:
    """Preset listing draws of each held stack, layer, or effect, the New Stack button while no stack exists."""

    def drawn(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        if rooted(bpy.types.Scene, context).bind(stacking.head).is_error():
            alerted(layout, pressed(layout, stacking.identity, creating, {}, text=creating.title, icon="NONE"))
        for listing, draw in listings:
            if listing.current(context).is_ok():
                draw(layout, holder, context)

    return drawn


@functools.cache
def _rows(held: tuple[tuple[str, str], ...]) -> Rows:
    """Enum rows of identifier and name pairs, cached so the strings Blender reads stay referenced."""
    return tuple((identifier, name, "") for identifier, name in held)


def targeted(properties: Addressed) -> Target:
    """Target an edit's properties name, effect and mask indices below zero naming none."""
    match properties.entry, properties.mask:
        case int(entry), int(mask) if entry >= 0 and mask >= 0:
            return EffectMaskAt(layer=properties.layer, entry=entry, mask=mask)
        case int(entry), _ if entry >= 0:
            return EffectAt(layer=properties.layer, entry=entry)
        case _, int(mask) if mask >= 0:
            return LayerMaskAt(layer=properties.layer, mask=mask)
        case _:
            return LayerAt(layer=properties.layer)


def addressed(target: Target) -> dict[str, Value]:
    """Edit property values naming a target."""
    match target:
        case LayerAt(layer=layer):
            return {"layer": layer}
        case EffectAt(layer=layer, entry=entry):
            return {"layer": layer, "entry": entry}
        case LayerMaskAt(layer=layer, mask=mask):
            return {"layer": layer, "mask": mask}
        case EffectMaskAt(layer=layer, entry=entry, mask=mask):
            return {"layer": layer, "entry": entry, "mask": mask}


def entry_value(identity: Identity, entry: bpy.types.SceneCompositorEffect, key: Key) -> Outcome[Value]:
    """Reads an entry input value by interface key."""
    return entry_input(identity, entry, key).bind(lambda held: Descriptor.resolve(held, "value", bpy.types.Property).map(lambda descriptor: descriptor.read(held)))


def grouped_value(identity: Identity, tree: bpy.types.NodeTree, node: bpy.types.Node, key: Key) -> Outcome[Value]:
    """Reads a group node input value by its group's interface key."""
    return (
        slot(identity)
        .bind(lambda name: located(addresses(tree, name).get(key), repr(tree), key))
        .bind(lambda identifier: located(next((socket for socket in node.inputs if socket.identifier == identifier), None), repr(node), identifier))
        .bind(lambda socket: Descriptor.resolve(socket, "default_value", bpy.types.Property).map(lambda descriptor: descriptor.read(socket)))
    )


def created_tree(name: str) -> Outcome[bpy.types.CompositorNodeTree]:
    """Creates an empty compositor node group."""
    return narrowed(bpy.data.node_groups.new(name, "CompositorNodeTree"))


def narrowed(tree: bpy.types.ID | None) -> Outcome[bpy.types.CompositorNodeTree]:
    """Narrows a node group to a compositor node tree."""
    match tree:
        case bpy.types.CompositorNodeTree():
            return Ok(tree)
        case _:
            return located(None, repr(tree), bpy.types.CompositorNodeTree.__name__)


def dropped(tree: bpy.types.NodeTree, identity: Identity, key: Key) -> None:
    """Removes the owned node of a key when the tree still holds it."""
    for node in Option.of_optional(owned(tree, identity).get(key)).to_list():
        tree.nodes.remove(node)


def stacked_by(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Option[bpy.types.NodeTree]:
    """Returns an entry's node group when it holds the stack ownership mark."""
    return Option.of_optional(entry.node_group).filter(lambda group: OWNERSHIP.marked(identity, group).is_some())


def nested_tree[M: Case](identity: Identity, tree: bpy.types.NodeTree, layer: Layer[Source, M]) -> Option[bpy.types.NodeTree]:
    """Returns the owned stack of a layer's source group node."""
    match owned(tree, identity).get(layer.node):
        case bpy.types.CompositorNodeGroup(node_tree=inner) if inner is not None:
            return Some(inner).filter(lambda held: OWNERSHIP.marked(identity, held).is_some())
        case _:
            return Nothing


# --- [COMPOSITION] ----------------------------------------------------------------------

VIEW: Final = Attachment(key=f"{OWNERSHIP.key}_view", form=StackView, owner=bpy.types.Screen)


class LayerStacks[M: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Layer stacks over the effects an extension offers: the rows a layer accepts and how its add menu groups them, stock looks, term filters, the document view, and conditions of the pinned head entry."""

    rows: EffectRows[M]
    accepts: Callable[[CompositorEffect[M]], bool] = lambda _row: True
    groups: Option[Callable[[CompositorEffect[M]], Enum]] = Nothing
    looks: Mapping[str, Valued[LayerStack[Source, M]]] = MappingProxyType({})
    filtering: TermFiltering[M] = TermFiltering()
    view: Callable[[Identity, bpy.types.Scene], Outcome[Option[DisplayView]]] = lambda _identity, _scene: Ok(Nothing)
    conditions: Option[Callable[[Identity, bpy.types.SceneCompositorEffect], Outcome[dict[Key, Condition]]]] = Nothing

    # --- [DECLARATIONS]

    @cached_property
    def form(self) -> type[LayerStack[Source, M]]:
        """Stack record over the offered effect records."""
        return GenericAlias(LayerStack, (Source, self.rows.form))  # type: ignore[return-value]  # ty: ignore[invalid-return-type]

    @cached_property
    def layered(self) -> Attachment[LayerRows]:
        """Layer rows on the stack group, a row edit deferring refresh of every stack."""
        return Attachment(key=f"{OWNERSHIP.key}_layers", form=LayerRows, owner=bpy.types.NodeTree, changed=Some(self.changed))

    @contextmanager
    def changed(self, identity: Identity) -> Generator[Changed]:
        """Holds the deferred stack refresh a layer row edit calls."""
        with identity.reports.deferred(lambda: self.bound(identity).bind(LayerStacking.refreshed)) as refresh:
            yield lambda _owner, _context: refresh()

    @cached_property
    def preset(self) -> Preset[Valued[LayerStack[Source, M]]]:
        """Stack presets with their input values over the stock looks."""
        return Preset(key=OWNERSHIP.key, form=GenericAlias(Valued, (self.form,)), stock=self.looks)  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    @cached_property
    def layer_preset(self) -> Preset[Valued[Layer[Source, M]]]:
        """Presets of one layer with its input values."""
        return Preset(key=f"{OWNERSHIP.key}_layer", form=GenericAlias(Valued, (GenericAlias(Layer, (Source, self.rows.form)),)))  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    @cached_property
    def entry_preset(self) -> Preset[Valued[Entry[M]]]:
        """Presets of one effect with its blend and input values."""
        return Preset(key=f"{OWNERSHIP.key}_entry", form=GenericAlias(Valued, (GenericAlias(Entry, (self.rows.form,)),)))  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    @cached_property
    def pinned(self) -> Pinned:
        """Head entry of the stack, pinned with its conditions."""
        return Pinned(ownership=OWNERSHIP, title=TITLE, edge=Edge.HEAD, conditions=self.conditions)

    # --- [COMMANDS]

    def bound_run[T: bpy.types.PropertyGroup](self, body: Callable[[LayerStacking[M], Call[T]], Outcome[Answer]]) -> Callable[[Call[T]], Outcome[Answer]]:
        """Runs a body over the calling extension's binding, described by the body."""
        return functools.update_wrapper(lambda call: self.bound(call.identity).bind(lambda stacking: body(stacking, call)), body)

    @cached_property
    def creating(self) -> Operator[Empty]:
        """Creates an empty stack."""
        return Operator(form=Empty, run=self.bound_run(LayerStacking.new_stack))

    @cached_property
    def stacking(self) -> tuple[Command, ...]:
        """Stack commands beside the layer rows."""
        return (
            Operator(form=Empty, run=self.bound_run(LayerStacking.duplicate_stack)),
            Operator(form=Switched, run=self.bound_run(LayerStacking.switch_stack), invoke=Some(InvokePopup.SEARCH), primary=Some("stack")),
            Operator(form=Empty, run=self.bound_run(LayerStacking.edit_nested_stack)),
            Operator(form=Copied, run=self.bound_run(LayerStacking.copy_layer), invoke=Some(Properties())),
            Operator(form=Switched, run=self.bound_run(LayerStacking.copy_all_layers), invoke=Some(InvokePopup.SEARCH), primary=Some("stack")),
            Operator(form=Empty, run=self.bound_run(LayerStacking.clear_layers)),
            Operator(form=Empty, run=self.bound_run(LayerStacking.remove_stack)),
        )

    @cached_property
    def sourcing(self) -> tuple[Command, ...]:
        """Commands adding a layer of each source."""
        extensions = (*sorted(bpy.path.extensions_image), *sorted(bpy.path.extensions_movie))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return (
            Operator(form=Empty, run=self.bound_run(LayerStacking.add_adjustment_layer)),
            Operator(form=PassSourced, run=self.bound_run(LayerStacking.add_render_pass_layer), invoke=Some(Properties())),
            Operator(form=RecombinedSourced, run=self.bound_run(LayerStacking.add_recombined_layer), invoke=Some(Properties())),
            Operator(form=NodeKind, run=self.bound_run(LayerStacking.add_node_layer), invoke=Some(InvokePopup.SEARCH), primary=Some("node")),
            Operator(form=Switched, run=self.bound_run(LayerStacking.add_nested_stack), invoke=Some(InvokePopup.SEARCH), primary=Some("stack")),
            Operator(form=Empty, run=self.bound_run(LayerStacking.add_media), invoke=Some(Open(extensions=lambda _: extensions))),
        )

    @cached_property
    def output(self) -> Operator[OutputChoice]:
        """Sets a layer's source output."""
        return Operator(form=OutputChoice, run=self.bound_run(LayerStacking.select_output))

    @cached_property
    def masking(self) -> Operator[Addressed]:
        """Adds a mask to a layer or effect blend."""
        return Operator(form=Addressed, run=self.bound_run(LayerStacking.add_mask))

    @cached_property
    def editing(self) -> Operator[Addressed]:
        """Edits a layer, effect, or mask by the edit its properties name."""
        return Operator(form=Addressed, run=self.bound_run(LayerStacking.edit_layer))

    @cached_property
    def effects(self) -> tuple[tuple[CompositorEffect[M], Operator[Empty]], ...]:
        """Accepted effects in rank order, each with the command appending it to the active layer."""

        def effect(row: CompositorEffect[M]) -> Operator[Empty]:
            def run(call: Call[Empty]) -> Outcome[Answer]:
                """Add the effect to the selected layer."""
                return self.bound(call.identity).bind(lambda stacking: stacking.effected(row, call))

            run.__name__ = run.__qualname__ = f"add_{row.kind}_to_layer"
            return Operator(form=Empty, run=run)

        return tuple((row, effect(row)) for row in sorted(filter(self.accepts, self.rows.rows), key=self.rows.rank))

    @property
    def commands(self) -> tuple[Command, ...]:
        """Commands every binding shares."""
        return (self.creating, *self.stacking, *self.sourcing, self.output, self.masking, self.editing, *(command for _, command in self.effects))

    # --- [MENUS]

    @cached_property
    def adding(self) -> Menu:
        """Menu adding a layer of each source."""
        return Menu(key="layer_add", title="Add Layer", description="Add layer source to stack", body=Dropdown(entries=tuple(OperatorButton(target=command) for command in self.sourcing)))

    @cached_property
    def effecting(self) -> Menu:
        """Menu appending an accepted effect to the active layer, one submenu per group where groups are given."""
        buttons = tuple((row, OperatorButton(target=command)) for row, command in self.effects)
        return Menu(
            key="layer_effect_add",
            title="Add Effect",
            description="Add effect to selected layer",
            body=self.groups.map(lambda group: Submenus(entries=tuple((group(row), button) for row, button in buttons))).default_with(
                lambda: Dropdown(entries=tuple(button for _, button in buttons), search=True)
            ),
        )

    @cached_property
    def specials(self) -> Menu:
        """Stack commands beside the layer rows."""
        return Menu(
            key=f"{OWNERSHIP.key}_specials",
            title=f"{TITLE} Specials",
            description="Stack and layer commands beside the layer rows",
            body=Dropdown(entries=tuple(OperatorButton(target=command) for command in self.stacking)),
        )

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """View attachment and layer rows with the frame size binding stacks read."""
        return VIEW, self.layered, FrameSize.attachment

    def bound(self, identity: Identity) -> Outcome[LayerStacking[M]]:
        """Binds the stack store, view attachment, pass flags, and framing of an extension."""
        store = identity.name(Name.ATTACHMENT, OWNERSHIP.key).map(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(self.form)))
        return paired(store, self.layered.bound(identity), VIEW.bound(identity), PassFlags.of(), framing(identity)).map(
            lambda parts: LayerStacking(identity=identity, stacks=self, store=parts[0], layer_rows=parts[1], view=parts[2], flags=parts[3], framing=parts[4])
        )

    def document(self, identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document[Coded]]:
        """Returns the layered document of the scene's head stack at a saved frame."""
        return self.bound(identity).bind(lambda stacking: stacking.layered(scene, frame, depth))

    # --- [SECTIONS]

    def section(self, header: tuple[RowContent, ...] = ()) -> Single:
        """Returns the editor section under header rows, presets first."""
        return Single(key=OWNERSHIP.key, title=TITLE, header=(Popover(panel=self.presets().key, text="", icon=Some("PRESET")), *header), rows=(self,), opened=True)

    def presets(self) -> Single:
        """Returns the section of stack, layer, and effect preset listings."""
        return Single(key=f"{OWNERSHIP.key}_presets", title=f"{TITLE} Presets", description=f"Apply, add, or import {TITLE.lower()}, layer, and effect presets", rows=((self, self.presets_drawn),))

    def presets_drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns the preset listing draws of each held stack, layer, or effect, the New Stack button while no stack exists."""

        def listed(stacking: LayerStacking[M]) -> Outcome[RowDraw]:
            stack, layer, entry = stacking.listings
            return accumulated((stack.drawn(identity), layer.drawn(identity), entry.drawn(identity))).map(lambda draws: _presets(stacking, self.creating, tuple(zip((stack, layer, entry), draws))))

        return self.bound(identity).bind(listed)

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns the editor draw of the extension's binding."""
        return self.bound(identity).bind(LayerStacking.drawn)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields commands, menus, listings, handlers, and subscriptions with deferred refresh, binding the stack and layer rows of the stack forms."""
        watched = ((bpy.types.RenderSettings, "film_transparent"), (bpy.types.ViewLayer, "name"), (bpy.types.ViewLayer, "use"), (bpy.types.Object, "lightgroup"), (bpy.types.World, "lightgroup"))
        with identity.reports.deferred(lambda: self.bound(identity).bind(LayerStacking.refreshed)) as refresh:

            def updated_scenes() -> Outcome[None]:
                refresh()
                return Ok(None)

            def rows(stacking: LayerStacking[M]) -> Acquisitions:
                def stack_rows(_owner: Struct, _context: bpy.types.Context | None) -> Rows:
                    return _rows(tuple((tree.name, tree.name) for tree, _ in OWNERSHIP.held(identity)))

                def layer_rows(owner: Copied, _context: bpy.types.Context | None) -> Rows:
                    return _rows(
                        tuple((layer.key, layer.name) for tree in stacking.named(owner.stack).to_option().to_list() for record in stacking.read(tree).to_option().to_list() for layer in record.layers)
                    )

                for definition in (Switched.__annotations__["stack"], Copied.__annotations__["stack"]):
                    definition.keywords["items"] = stack_rows
                Copied.__annotations__["layer"].keywords["items"] = layer_rows
                subscriptions = (*stacking.framing.reads, *((owner, owner.bl_rna.properties[name]) for owner, name in watched))
                handlers: tuple[Registrable, ...] = (
                    PathHandler(handlers=LOAD_POST, body=lambda _: updated_scenes()),
                    *(Subscription(owner=owner, setting=prop, body=updated_scenes) for owner, prop in subscriptions),
                )
                return (OWNERSHIP, self.adding, self.effecting, self.specials, *self.commands, *stacking.listings, stacking.listing, *handlers)

            yield self.bound(identity).map(rows)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "OWNERSHIP",
    "TITLE",
    "VIEW",
    "Addressed",
    "Copied",
    "LayerRow",
    "LayerRows",
    "LayerStacking",
    "LayerStacks",
    "NodeKind",
    "OutputChoice",
    "PassSourced",
    "RecombinedSourced",
    "Selection",
    "StackView",
    "Switched",
    "Valued",
    "addressed",
    "entry_value",
    "grouped_value",
    "nested_tree",
    "stacked_by",
    "targeted",
]
