"""Layer stacks held in scene compositor entries: reading, rebuilding, and persisting each stack, its commands, its section of a layer list with presets, and its layered document."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from enum import Enum
import functools
from functools import cached_property
import itertools
from pathlib import Path
from types import GenericAlias, MappingProxyType
from typing import Annotated, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Command, Done, Empty, InvokePopup, Open, Operator, Outcome, Properties, Unchanged
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.ownership import committed, Mark, Ownership, Saved
from blender.bpy.data.ids.store import named, vacant
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor, root_in, rooted, subclass, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.changes import Changes, owned, reconciled
from blender.bpy.nodes.graphs import Graph, lowered, NodeGroup
from blender.bpy.nodes.interface import addresses, item_key, slot
from blender.bpy.nodes.trees import created, grouped
from blender.bpy.render.color.spaces import config_of, viewed
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import camera_frame, CameraFrame, CompositorEffect, Derives, framing, Shell, Spaces
from blender.bpy.render.compositor.entries import Edge, EffectRows, Pinned, relocated
from blender.bpy.render.compositor.inputs import entered, entry_input, graph_values, inputs, KeyPath, node_fields, updated
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
    masking,
    node_types,
    NodeSource,
    planned,
    Planning,
    RenderPass,
    Source,
    sourced,
    Target,
    TermFiltering,
    Valued,
)
from blender.bpy.render.compositor.recombination import Recombined
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.view_layers.passes import PassRequest, PassRouting, PassSet, required, synced
from blender.bpy.scene.lights.groups import used
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.properties import Changed, Choices
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import alerted, Divider, Member, OperatorButton, Popover, pressed, Property, Pulldown, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.templates import NodeEditors, NodeField
from blender.bpy.ui.layout.values import Listing
from blender.bpy.ui.surfaces.menus import Dropdown, Menu, Submenus
from blender.imaging.color.processors import DisplayView
from blender.imaging.files import documents
from blender.imaging.files.documents import Document
from blender.imaging.files.images import Depth, Rounded, unassociated
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import accumulated, paired
from blender.records.fields import Case, Gated, heading

# --- [TYPES] ----------------------------------------------------------------------------

type Coded = np.uint8 | np.uint16 | np.float32

# --- [CONSTANTS] ------------------------------------------------------------------------

OWNERSHIP: Final = Ownership(kind=bpy.types.CompositorNodeTree, key="layer_stack", saved=Saved.ALWAYS, single=False)
TITLE: Final = heading(OWNERSHIP.key)
_DIGITS: Final = 4

# --- [CHOICES] --------------------------------------------------------------------------

stack_names = Choices.named(lambda identity, _owner, _context: (tree.name for tree, _ in OWNERSHIP.held(identity)))
layer_names = Choices(
    lambda identity, owner, _context: (
        (layer.key, layer.name, "")
        for name in identity.name(Name.ATTACHMENT, OWNERSHIP.key).to_option().to_list()
        for tree, _ in OWNERSHIP.held(identity)
        if tree.name == owner.stack  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        for layer in getattr(tree, name).layers
    )
)

# --- [MODELS] ---------------------------------------------------------------------------


class StackView(msgspec.Struct, frozen=True, kw_only=True):
    """Screen's active layer index, bottom-first layer order."""

    active: Annotated[int, msgspec.Meta(ge=0)] = 0


class Switched(msgspec.Struct, frozen=True, kw_only=True):
    """Owned stack selected by node group name."""

    stack: Annotated[str, stack_names]


class Copied(msgspec.Struct, frozen=True, kw_only=True):
    """One layer of another stack."""

    stack: Annotated[str, stack_names]
    layer: Annotated[str, layer_names]


class NodeKind(msgspec.Struct, frozen=True, kw_only=True):
    """Source node class to create."""

    node: Annotated[str, node_types]


class Addressed(msgspec.Struct, frozen=True, kw_only=True):
    """Layer, effect, or mask an edit targets."""

    target: Target


class OutputChoice(msgspec.Struct, frozen=True, kw_only=True):
    """Enabled source output chosen for one layer."""

    layer: Annotated[int, msgspec.Meta(ge=0)]
    output: str


class Selection[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Head entry, its owned stack and record, and the screen's active layer."""

    entry: bpy.types.SceneCompositorEffect
    tree: bpy.types.NodeTree
    record: LayerStack[Source, M]
    view: StackView

    @property
    def layer(self) -> BlenderResult[Layer[Source, M]]:
        """Active layer."""
        return indexed(self.record.layers, self.view.active, TITLE)


# --- [SERVICES] -------------------------------------------------------------------------


class LayerStacking[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Layer stacks bound to one extension: attachments, pass routing, and camera framing reads."""

    identity: Identity
    stacks: "LayerStacks[M]"
    attached: Attached[LayerStack[Source, M]]
    view: Attached[StackView]
    routed: PassRouting
    framing: Derives[Case, CameraFrame]

    # --- [SELECTION]

    def head(self, scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree]]:
        """Returns the first entry holding an owned stack, with that stack."""
        return located(next(((entry, tree) for entry in scene.compositor_effects for tree in stacked_by(self.identity, entry).to_list()), None), repr(scene.compositor_effects), OWNERSHIP.key)

    def named(self, name: str) -> BlenderResult[bpy.types.NodeTree]:
        """Returns the owned stack of a node group name."""
        return located(next((tree for tree, _ in OWNERSHIP.held(self.identity) if tree.name == name), None), repr(bpy.data.node_groups), name)

    def selected(self, context: bpy.types.Context) -> BlenderResult[Selection[M]]:
        """Returns head entry, owned stack, record, and screen selection."""
        held = rooted(bpy.types.Scene, context).bind(self.head).bind(lambda found: self.read(*found).map(lambda record: (*found, record)))
        return paired(held, rooted(bpy.types.Screen, context).bind(self.view.read)).map(lambda parts: Selection(entry=parts[0][0], tree=parts[0][1], record=parts[0][2], view=parts[1]))

    # --- [VALUES]

    def config(self, record: LayerStack[Source, M]) -> BlenderResult[PyOpenColorIO.Config]:
        """Returns the live OCIO config resolving every space the stack's effects run in."""
        spaces = accumulated(
            self.stacks.rows.offering(type(gated.value.record)).map(lambda row: tuple(part.space(gated.value.record) for part in row.found(Spaces)))
            for layer in record.layers
            for gated in layer.effects.value
        )
        return spaces.bind(lambda found: config_of(itertools.chain.from_iterable(found)))

    def read(self, entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree, *, baseline: Option[LayerStack[Source, M]] = Nothing) -> BlenderResult[LayerStack[Source, M]]:
        """Splices entry inputs into the stored stack, or graph edits into a given baseline."""

        def toggle(keys: KeyPath, *, held: bool) -> BlenderResult[bool]:
            return (
                Ok(held)
                if baseline.is_some()
                else entry_input(self.identity, entry, item_key(*keys)).bind(
                    lambda owner: Descriptor.resolve(owner, "value", bpy.types.BoolProperty).map(lambda descriptor: bool(descriptor.read(owner)))
                )
            )

        def recorded(record: LayerStack[Source, M]) -> BlenderResult[LayerStack[Source, M]]:
            gates = record.gates()

            def values(parts: tuple[Valued, ...]) -> BlenderResult[tuple[object, ...]]:
                return graph_values(self.identity, entry, tuple((part.keys, type(part.value), Some(part.value), part.domain) for part in parts)).bind(
                    lambda graphs: accumulated(inputs(self.identity, entry, part.keys, type(part.value), graphs, baseline=baseline.map(lambda _: part.value)) for part in parts)
                )

            return paired(
                record.parts(self.stacks.rows).bind(lambda parts: values(parts).map(lambda read: tuple(zip((part.at for part in parts), read)))),
                accumulated(toggle(keys, held=held) for keys, _, held in gates),
            ).map(lambda read: functools.reduce(lambda held, change: updated(held, change[0], lambda _: change[1]), (*read[0], *zip((at for _, at, _ in gates), read[1])), record))

        return baseline.map(Ok).default_with(lambda: self.attached.read(tree)).bind(recorded)

    def rebuilt(self, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree, record: LayerStack[Source, M], context: bpy.types.Context) -> BlenderResult[None]:
        """Rebuilds the graph, then writes record values into entry inputs."""

        def toggled(keys: KeyPath, *, enabled: bool) -> BlenderResult[bool]:
            return entry_input(self.identity, entry, item_key(*keys)).bind(lambda owner: written(owner, "value", enabled)).map(Option.is_some)

        def values() -> BlenderResult[tuple[bool, ...]]:
            return record.parts(self.stacks.rows).bind(
                lambda parts: accumulated((*(entered(self.identity, entry, part.keys, part.value) for part in parts), *(toggled(keys, enabled=enabled) for keys, _, enabled in record.gates())))
            )

        return self.reconciled(scene, tree, context, record=Some(record)).bind(lambda _: values()).map(lambda _: None)

    def persisted(self, context: bpy.types.Context, held: Selection[M], record: LayerStack[Source, M], active: int) -> BlenderResult[None]:
        """Writes the stack and the active layer index."""
        rebuilt = rooted(bpy.types.Scene, context).bind(lambda scene: self.rebuilt(scene, held.entry, held.tree, record, context))
        return paired(rebuilt, rooted(bpy.types.Screen, context).bind(lambda screen: self.view.write(screen, StackView(active=active)))).map(lambda _: None)

    def looked(self, scene: bpy.types.Scene, preset: LayerStack[Source, M], strength: int, context: bpy.types.Context) -> BlenderResult[bool]:
        """Writes a preset over the head stack at `strength` percent of each effect's change from its record defaults, creating the stack when absent, answering whether it changed."""

        def held() -> BlenderResult[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree]]:
            return self.head(scene).to_option().map(Ok).default_with(lambda: self.made(scene, context).bind(lambda _: self.head(scene)))

        def placed(record: LayerStack[Source, M], entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree) -> BlenderResult[bool]:
            return self.read(entry, tree).bind(lambda current: Ok(False) if current == record else self.rebuilt(scene, entry, tree, record, context).map(lambda _: True))

        return self.config(preset).bind(lambda config: preset.strengthened(strength / 100, config)).bind(lambda record: held().bind(lambda found: placed(record, *found)))

    def replaced(self, context: bpy.types.Context, held: Selection[M], record: LayerStack[Source, M], active: int) -> BlenderResult[bool]:
        """Writes the stack and active index when the stack changed, answering whether it did."""
        return Ok(False) if record == held.record else self.persisted(context, held, record, active).map(lambda _: True)

    def appended(self, context: bpy.types.Context, held: Selection[M], at: KeyPath, item: object, active: int) -> BlenderResult[bool]:
        """Appends an item to the collection at a record path, answering whether the stack changed."""
        return collection(held.record, at).bind(lambda found: self.replaced(context, held, updated(held.record, at, lambda _: (*found, item)), active))

    # --- [GRAPH]

    def sources(self, scene: bpy.types.Scene, record: LayerStack[Source, M], tree: bpy.types.NodeTree) -> BlenderResult[dict[Key, dict[str, str]]]:
        """Reads enabled source outputs from a scratch graph with settings copied from the stack's owned source nodes."""

        def read(scratch: bpy.types.CompositorNodeTree) -> BlenderResult[dict[Key, dict[str, str]]]:
            with ExitStack() as release:
                release.callback(bpy.data.node_groups.remove, scratch)

                def copied(_: Changes) -> BlenderResult[dict[Key, dict[str, str]]]:
                    found = owned(scratch, self.identity)
                    return accumulated(Snapshot.capture(source).bind(lambda snapshot: snapshot.apply(found[key]).outcome) for key, source in owned(tree, self.identity).items() if key in found).map(
                        lambda _: {key: {output.name: output.identifier for output in source.outputs if output.enabled} for key, source in found.items()}
                    )

                return (
                    accumulated(sourced(layer.key, source, scene) for layer in record.layers for source in (layer.source,) if not isinstance(source, Adjustment))
                    .bind(lambda found: lowered(*found))
                    .bind(lambda body: reconciled(Graph(kind=bpy.types.CompositorNodeTree, nodes=body.nodes), scratch, self.identity))
                    .bind(copied)
                )

        return created_tree("Layer Sources").bind(read)

    def reconciled(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, context: bpy.types.Context, *, record: Option[LayerStack[Source, M]] = Nothing) -> BlenderResult[bool]:
        """Enables the passes each source and term filter reads, builds the graph, and writes frame inputs, answering whether any changed."""

        def requested(layer: Layer[Source, M]) -> BlenderResult[tuple[PassRequest, ...]]:
            match layer.source:
                case RenderPass(layer=name):
                    return Ok((PassRequest(layer=name),))
                case Recombined() as source:
                    return self.stacks.filtering.requests(layer).map(lambda extra: (source.requested(self.routed.flags), *extra))
                case Adjustment() | NodeSource():
                    return Ok(())

        def built(desired: LayerStack[Source, M], config: PyOpenColorIO.Config, path: OutputPath) -> BlenderResult[bool]:
            names = tuple(sorted(used(scene)))
            synced_layers = tuple(msgspec.structs.replace(layer, source=layer.source.synced(names)) if isinstance(layer.source, Recombined) else layer for layer in desired.layers)
            stacked = msgspec.structs.replace(desired, layers=synced_layers)
            frames = FrameSize().bound(self.identity).bind(lambda framed: camera_frame(framed, scene))

            def planning(parts: tuple[dict[Key, dict[str, str]], NodeGroup]) -> Planning[M]:
                return Planning(
                    identity=self.identity,
                    scene=scene,
                    config=config,
                    luminance=parts[1],
                    rows=self.stacks.rows,
                    routed=self.routed,
                    offered=parts[0],
                    path=path,
                    uid=tree.session_uid,
                    filtering=self.stacks.filtering,
                )

            return (
                synced(scene, self.identity)
                .bind(lambda _: self.attached.write(tree, stacked))
                .bind(lambda _: accumulated(map(requested, stacked.layers)))
                .bind(lambda wanted: required(scene, context, tuple(itertools.chain.from_iterable(wanted)), self.routed.precision))
                .bind(lambda _: paired(self.sources(scene, stacked, tree), masks.luminance(config)))
                .bind(lambda parts: planned(stacked, planning(parts), OWNERSHIP.key, TITLE, _DIGITS))
                .bind(lambda group: reconciled(group, tree, self.identity))
                .bind(
                    lambda changes: frames.bind(lambda frame: accumulated(entered(self.identity, entry, (Shell.frame,), frame) for entry in scene.compositor_effects if entry.node_group == tree)).map(
                        lambda wrote: any(wrote) or changes != Changes()
                    )
                )
            )

        desired = record.map(Ok).default_with(
            lambda: self.attached.read(tree).bind(
                lambda stored: located(next((entry for entry in scene.compositor_effects if entry.node_group == tree), None), repr(scene.compositor_effects), tree.name).bind(
                    lambda entry: self.read(entry, tree, baseline=Some(stored))
                )
            )
        )
        return desired.bind(lambda held: paired(self.config(held), self.routed.path(scene)).bind(lambda parts: built(held, *parts)))

    def refreshed(self) -> BlenderResult[Option[str]]:
        """Reconciles each scene stack, naming the undo step when any changed."""
        return accumulated(
            self.reconciled(scene, tree, bpy.context)
            for scene in bpy.data.scenes
            for tree in dict.fromkeys(tree for entry in scene.compositor_effects for tree in stacked_by(self.identity, entry).to_list())
        ).map(lambda changed: Some(f"Refresh {TITLE}s") if any(changed) else Nothing)

    # --- [STACKS]

    def switched(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, context: bpy.types.Context) -> BlenderResult[None]:
        """Places a stack in the head entry, creating the entry when absent."""
        held = self.head(scene).to_option()
        with ExitStack() as rollback:
            entry = held.map(lambda found: found[0]).default_with(lambda: scene.compositor_effects.new(vacant(TITLE, (item.name for item in scene.compositor_effects))))
            for _ in () if held.is_some() else (entry,):
                rollback.callback(scene.compositor_effects.remove, entry)
            return committed(written(entry, "node_group", tree).bind(lambda _: relocated(scene, entry, 0)).bind(lambda _: self.reconciled(scene, tree, context)).map(lambda _: None), rollback)

    def made(self, scene: bpy.types.Scene, context: bpy.types.Context) -> BlenderResult[bpy.types.CompositorNodeTree]:
        """Creates an empty stack and places it in the head entry."""
        return OWNERSHIP.made(self.identity, lambda: created_tree(vacant(TITLE, bpy.data.node_groups.keys())), of=Some(scene)).bind(
            lambda group: self.attached.write(group, self.stacks.form()).bind(lambda _: self.switched(scene, group, context)).map(lambda _: group)
        )

    def inserted(self, context: bpy.types.Context, layers: tuple[Layer[Source, M], ...], source: Option[bpy.types.NodeTree] = Nothing) -> BlenderResult[bool]:
        """Inserts layers after the active layer under fresh keys with source node settings copied, answering whether any was given."""

        def insert(held: Selection[M]) -> BlenderResult[bool]:
            added = tuple(msgspec.structs.replace(layer, key=key) for layer, key in zip(layers, held.record.fresh()))
            at = min(held.view.active + 1, len(held.record.layers))
            nodes = tuple((node, after.node) for tree in source.to_list() for before, after in zip(layers, added) for node in Option.of_optional(owned(tree, self.identity).get(before.node)).to_list())

            def copy(node: bpy.types.Node, key: Key) -> BlenderResult[None]:
                return Snapshot.capture(node).bind(
                    lambda snapshot: created(held.tree, type(node)).bind(lambda made: snapshot.apply(made).outcome.bind(lambda _: Mark(key=key).write(self.identity, made)))
                )

            with ExitStack() as rollback:
                for _, key in nodes:
                    rollback.callback(dropped, held.tree, self.identity, key)
                record = msgspec.structs.replace(held.record, layers=(*held.record.layers[:at], *added, *held.record.layers[at:]))
                return committed(accumulated(itertools.starmap(copy, nodes)).bind(lambda _: self.replaced(context, held, record, at + len(added) - 1 if added else held.view.active)), rollback)

        return self.selected(context).bind(insert)

    def added_node[T](self, context: bpy.types.Context, kind: type[bpy.types.Node], setup: Callable[[bpy.types.Node], BlenderResult[T]]) -> BlenderResult[None]:
        """Adds a source node layer after the active layer at the node's first enabled output."""

        def add(held: Selection[M]) -> BlenderResult[None]:
            key, at = next(held.record.fresh()), min(held.view.active + 1, len(held.record.layers))
            layer_node = item_key(key, "source")

            def placed(node: bpy.types.Node) -> BlenderResult[None]:
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

    def moved(self, context: bpy.types.Context, origin: int, target: int) -> BlenderResult[None]:
        """Moves the layer at one list row to another."""

        def move(held: Selection[M]) -> BlenderResult[None]:
            layers = held.record.layers
            rest = (*layers[:origin], *layers[origin + 1 :])
            return self.persisted(context, held, msgspec.structs.replace(held.record, layers=(*rest[:target], *layers[origin : origin + 1], *rest[target:])), target)

        return self.selected(context).bind(move)

    # --- [COMMANDS]

    def created(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Creates an empty stack and places it in the head entry."""
        return call.root(bpy.types.Scene).bind(lambda scene: self.made(scene, call.context)).map(lambda _: Done())

    def duplicated(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Copies the head stack with its source settings and places the copy."""

        def cloned(scene: bpy.types.Scene, tree: bpy.types.NodeTree) -> BlenderResult[None]:
            def copied() -> BlenderResult[bpy.types.CompositorNodeTree]:
                return narrowed(tree.copy()).bind(lambda copy: named(copy, vacant(TITLE, bpy.data.node_groups.keys())))

            return OWNERSHIP.made(self.identity, copied, of=Some(scene)).bind(lambda copy: self.switched(scene, copy, call.context))

        return call.root(bpy.types.Scene).bind(lambda scene: self.head(scene).bind(lambda held: cloned(scene, held[1]))).map(lambda _: Done())

    def removed(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Removes the head stack and its entry."""

        def remove(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree) -> Done:
            bpy.data.node_groups.remove(tree, do_unlink=True)
            scene.compositor_effects.remove(entry)
            return Done()

        return call.root(bpy.types.Scene).bind(lambda scene: self.head(scene).map(lambda held: remove(scene, *held)))

    def switching(self, call: Call[Switched]) -> BlenderResult[Outcome]:
        """Places another owned stack."""
        return paired(call.root(bpy.types.Scene), self.named(call.record.stack)).bind(lambda parts: self.switched(*parts, call.context)).map(lambda _: Done())

    def nested(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Places the active layer's nested stack."""
        inner = self.selected(call.context).bind(lambda held: held.layer.bind(lambda layer: located(nested_tree(self.identity, held.tree, layer).to_optional(), TITLE, "nested stack")))
        return paired(call.root(bpy.types.Scene), inner).bind(lambda parts: self.switched(*parts, call.context)).map(lambda _: Done())

    def cleared(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Removes every layer."""
        wrote = self.selected(call.context).bind(lambda held: self.replaced(call.context, held, msgspec.structs.replace(held.record, layers=()), 0))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason="Stack holds no layers"))

    def copied(self, call: Call[Copied | Switched]) -> BlenderResult[Outcome]:
        """Inserts one or every layer of another stack with its source settings."""
        wanted = Some(call.record.layer) if isinstance(call.record, Copied) else Nothing

        def chosen(group: bpy.types.NodeTree, record: LayerStack[Source, M]) -> BlenderResult[Outcome]:
            layers = wanted.map(lambda key: located(next((held for held in record.layers if held.key == key), None), group.name, key).map(lambda held: (held,))).default_value(Ok(record.layers))
            return layers.bind(lambda picked: self.inserted(call.context, picked, Some(group))).map(lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers"))

        def valued(group: bpy.types.NodeTree) -> BlenderResult[LayerStack[Source, M]]:
            entry = next((entry for scene in bpy.data.scenes for entry in scene.compositor_effects if entry.node_group == group), None)
            return Option.of_optional(entry).map(lambda held: self.read(held, group)).default_with(lambda: self.attached.read(group))

        return self.named(call.record.stack).bind(lambda group: valued(group).bind(lambda record: chosen(group, record)))

    def added[S: Source](self, call: Call[S]) -> BlenderResult[Outcome]:
        """Inserts a layer of the called source."""
        return self.inserted(call.context, (Layer(key="inserted", name=type(call.record).title, source=call.record),)).map(lambda _: Done())

    def noded(self, call: Call[NodeKind]) -> BlenderResult[Outcome]:
        """Adds a source node layer of the called class."""
        return subclass(bpy.types.Node, call.record.node).bind(lambda kind: self.added_node(call.context, kind, Ok)).map(lambda _: Done())

    def nesting(self, call: Call[Switched]) -> BlenderResult[Outcome]:
        """Adds another stack as source."""
        return self.named(call.record.stack).bind(lambda group: self.added_node(call.context, bpy.types.CompositorNodeGroup, lambda node: grouped(node, group))).map(lambda _: Done())

    def media(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Adds image or movie file layers."""
        images = accumulated(loaded(str(path), Tag()) for path in call.paths)
        return images.bind(lambda found: accumulated(self.added_node(call.context, bpy.types.CompositorNodeImage, functools.partial(written, identifier="image", value=image)) for image in found)).map(
            lambda _: Done()
        )

    def output(self, call: Call[OutputChoice]) -> BlenderResult[Outcome]:
        """Sets a node or render pass layer's source output."""
        index, at = call.record.layer, ("layers", call.record.layer + 1, "source", "output")

        def chosen(held: Selection[M], layer: Layer[Source, M]) -> BlenderResult[bool]:
            match layer.source:
                case RenderPass() | NodeSource():
                    return self.replaced(call.context, held, updated(held.record, at, lambda _: call.record.output), index)
                case Adjustment() | Recombined():
                    return located(None, layer.name, at[-1])

        wrote = self.selected(call.context).bind(lambda held: indexed(held.record.layers, index, TITLE).bind(lambda layer: chosen(held, layer)))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason=f"Layer reads {call.record.output}"))

    def masked(self, call: Call[Addressed]) -> BlenderResult[Outcome]:
        """Adds a mask to the blend of the targeted layer or effect."""
        return self.selected(call.context).bind(lambda held: self.appended(call.context, held, masking(call.record.target), masks.Mask(), call.record.target.layer)).map(lambda _: Done())

    def effected(self, row: CompositorEffect[M], call: Call[Empty]) -> BlenderResult[Outcome]:
        """Appends the effect's seeded record to the active layer."""

        def appended(held: Selection[M], record: M) -> BlenderResult[bool]:
            return self.appended(call.context, held, ("layers", held.view.active + 1, "effects", "value"), Gated(enabled=True, value=Entry(record=record)), held.view.active)

        return paired(call.root(bpy.types.Scene).bind(row.seeded), self.selected(call.context)).bind(lambda parts: appended(parts[1], parts[0])).map(lambda _: Done())

    def edit(self, action: Edit, call: Call[Addressed]) -> BlenderResult[Outcome]:
        """Applies an edit to the target, a layer duplicate copying its source node settings."""
        target = call.record.target

        def seeded(scene: bpy.types.Scene) -> Callable[[M], BlenderResult[M]]:
            return lambda record: self.stacks.rows.offering(type(record)).bind(lambda row: row.seeded(scene))

        def applied(scene: bpy.types.Scene, held: Selection[M]) -> BlenderResult[bool]:
            match target:
                case LayerAt(layer=index) if action is Edit.DUPLICATE:
                    return indexed(held.record.layers, index, TITLE).bind(lambda layer: self.inserted(call.context, (layer,), Some(held.tree)))
                case _:
                    return held.record.edited(action, target, seeded(scene)).bind(lambda edit: self.replaced(call.context, held, *edit))

        wrote = paired(call.root(bpy.types.Scene), self.selected(call.context)).bind(lambda parts: applied(*parts))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason=f"{heading(action.name)} changes nothing"))

    def applied(self, context: bpy.types.Context, value: LayerStack[Source, M] | Layer[Source, M] | Entry[M]) -> BlenderResult[Outcome]:
        """Applies a stack, layer, or effect preset over the selection."""

        def edited(held: Selection[M]) -> BlenderResult[bool]:
            at = ("layers", held.view.active + 1)
            match value:
                case LayerStack():
                    return self.replaced(context, held, value, 0)
                case Layer():
                    return held.layer.bind(lambda prior: self.replaced(context, held, updated(held.record, at, lambda _: msgspec.structs.replace(value, key=prior.key)), held.view.active))
                case Entry():
                    return self.appended(context, held, (*at, "effects", "value"), Gated(enabled=True, value=value), held.view.active)

        return self.selected(context).bind(edited).map(lambda wrote: Done() if wrote else Unchanged(reason="Stack matches preset"))

    # --- [ROWS]

    @property
    def listings(self) -> tuple[Listing[LayerStack[Source, M]], Listing[Layer[Source, M]], Listing[Entry[M]]]:
        """Stack, active layer, and last effect preset listings."""

        def listing[T: msgspec.Struct](read: Callable[[bpy.types.Context], BlenderResult[T]], preset: Preset[T], applied: Callable[[bpy.types.Context, T], BlenderResult[Outcome]]) -> Listing[T]:
            return Listing(preset=preset, current=read, matches=lambda context, values: read(context).map(lambda held: tuple(held == value for value in values)), applied=applied)

        def last(held: Selection[M]) -> BlenderResult[Entry[M]]:
            return held.layer.bind(lambda layer: located(next((gated.value for gated in layer.effects.value[-1:]), None), layer.name, "effects"))

        stacks = self.stacks
        return (
            listing(lambda context: self.selected(context).map(lambda held: held.record), stacks.preset, self.applied),
            listing(lambda context: self.selected(context).bind(lambda held: held.layer), stacks.layer_preset, self.applied),
            listing(lambda context: self.selected(context).bind(last), stacks.entry_preset, self.applied),
        )

    @property
    def listing(self) -> ItemList:
        """Layer list with visibility columns and edit side buttons, dragged to reorder."""
        item = self.attached.group.struct.__annotations__["layers"].keywords["type"]
        stacks = self.stacks
        return ItemList(
            key=f"{OWNERSHIP.key}_layers",
            item=item,
            collection=Member(
                root=bpy.types.NodeTree,
                path=f"{self.attached.name}.layers",
                reach=Some(lambda context: root_in(bpy.types.Scene, context).bind(lambda scene: self.head(scene).to_option()).map(lambda held: held[1])),
            ),
            active=Member(root=bpy.types.Screen, path=f"{self.view.name}.active", reach=Some(lambda context: Option.of_optional(context.screen))),
            columns=tuple(Property(target=Member(root=item, path=path), text="") for path in ("visible", "solo", "matte.enabled")),
            enabled=Some(lambda context, held: self.selected(context).map(lambda found: any(shown.key == held.key for shown, _ in found.record.composited()))),  # ty: ignore[unresolved-attribute]
            sort_reverse=True,
            side=(
                Pulldown(menu=stacks.adding.key, text="", icon=Some("ADD")),
                self.button(Edit.REMOVE),
                Divider(type="SPACE"),
                Pulldown(menu=stacks.specials.key, text="", icon=Some("DOWNARROW_HLT")),
                Divider(type="SPACE"),
                *map(self.button, (Edit.DUPLICATE, Edit.UP, Edit.DOWN)),
            ),
            absent=(OperatorButton(target=stacks.creating),),
            move=Some(self.moved),
        )

    # --- [EDITOR]

    def button(self, action: Edit) -> RowDraw:
        """Returns the icon button of one edit over the active layer."""

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for held in self.selected(context).to_option().to_list():
                alerted(layout, pressed(layout, self.identity, self.stacks.editing[action], Addressed(target=LayerAt(layer=held.view.active)), text="", icon=action.value))

        return shown

    def drawn(self) -> BlenderResult[RowDraw]:
        """Returns the draw of the layer list, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
        stacks = self.stacks

        def edits(layout: bpy.types.UILayout, target: Target) -> None:
            line = layout.row(align=True)
            for action in Edit:
                alerted(line, pressed(line, self.identity, stacks.editing[action], Addressed(target=target), text="", icon=action.value))

        def mask(layout: bpy.types.UILayout, target: LayerAt | EffectAt) -> None:
            alerted(layout, pressed(layout, self.identity, stacks.masking, Addressed(target=target), text=stacks.masking.title, icon="MOD_MASK"))

        def draw(listing: RowDraw, adding: RowDraw, editors: NodeEditors) -> RowDraw:
            def widgets(layout: bpy.types.UILayout, held: Selection[M], prefix: Key, inner: tuple[Key, ...], fields: tuple[NodeField, ...], context: bpy.types.Context) -> None:
                nodes = owned(held.tree, self.identity)
                for edited in fields:
                    for live in Option.of_optional(nodes.get(edited.node)).to_list():
                        alerted(layout, editors.drawn(layout, live, edited, context))
                for key in (key for keys in slot(self.identity).map(lambda name: addresses(held.tree, name)).to_option().to_list() for key in keys):
                    if key.startswith(f"{prefix}_") and not any(key.startswith(f"{nested}_") for nested in inner):
                        alerted(layout, entry_input(self.identity, held.entry, key).map(lambda owner: layout.prop(owner, "value")))

            def effected(layout: bpy.types.UILayout, held: Selection[M], at: EffectAt, place: KeyPath, gated: Gated[Entry[M]], context: bpy.types.Context) -> None:
                header, body = layout.panel(f"{item_key(*place)}_{held.tree.session_uid}", default_closed=False)
                header.label(text=type(gated.value.record).title)
                edits(header, at)
                for inner in Option.of_optional(body).to_list():
                    widgets(inner, held, item_key(*place), (), (*node_fields(gated.value.record, place), *node_fields(gated.value.blend, (*place, Shell.blend))), context)
                    mask(inner, at)
                    for index in range(len(gated.value.blend.masks)):
                        edits(inner, EffectMaskAt(layer=at.layer, entry=at.entry, mask=index))

            def detail(layout: bpy.types.UILayout, held: Selection[M], layer: Layer[Source, M], context: bpy.types.Context) -> None:
                at = LayerAt(layer=held.view.active)
                stored = self.attached.struct(held.tree).layers[at.layer]  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                places = tuple((layer.key, "effects", index) for index in range(1, len(layer.effects.value) + 1))
                for field in (field for field in msgspec.structs.fields(type(layer.source)) if isinstance(getattr(layer.source, field.name), str)):
                    layout.prop(getattr(stored.source, type(layer.source).__name__), field.name)
                for source in Option.of_optional(owned(held.tree, self.identity).get(layer.node)).to_list():
                    source.draw_buttons_ext(context, layout)
                    layout.template_node_inputs(source)
                    outputs = layout.row(align=True)
                    for output in (output for output in source.outputs if output.enabled):
                        alerted(outputs, pressed(outputs, self.identity, stacks.output, OutputChoice(layer=at.layer, output=output.name), text=output.name, icon="NONE"))
                widgets(layout, held, layer.key, tuple(itertools.starmap(item_key, places)), tuple(field for name, value in layer.parts for field in node_fields(value, (layer.key, name))), context)
                buttons = layout.row(align=True)
                mask(buttons, at)
                alerted(buttons, pressed(buttons, self.identity, stacks.editing[Edit.RESET], Addressed(target=at), text="", icon=Edit.RESET.value))
                for index in range(len(layer.blend.masks)):
                    edits(layout, LayerMaskAt(layer=at.layer, mask=index))
                for index, gated in enumerate(layer.effects.value):
                    effected(layout, held, EffectAt(layer=at.layer, entry=index), places[index], gated, context)

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                listing(layout, holder, context)
                for _ in rooted(bpy.types.Scene, context).bind(self.head).to_option().to_list():
                    selected = self.selected(context)
                    alerted(layout, selected)
                    for held, layer in ((found, layer) for found in selected.to_option().to_list() for layer in found.layer.to_option().to_list()):
                        detail(layout, held, layer, context)
                        adding(layout, holder, context)

            return shown

        adding = realized(Pulldown(menu=stacks.effecting.key, icon=Some("ADD")), self.identity)
        return paired(self.listing.drawn(self.identity), adding, NodeEditors.realized(self.identity)).map(lambda parts: draw(*parts))

    # --- [DOCUMENTS]

    def layered(self, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[Coded]]:
        """Builds the document of formed color and raw mask coverage of the head stack at a saved frame, nested stacks as groups."""
        number = located(next((part.header.get("Frame") for part in frame.parts[:1]), None), frame.path, "Frame").bind(
            lambda text: catch(lambda: int(text), exception=ValueError)().map_error(lambda error: (BlenderEditFault(request=frame.path, error=error),))
        )

        coded = functools.partial(depth.coded, coding=Rounded())

        def composed(tree: bpy.types.NodeTree, record: LayerStack[Source, M], at: int, config: PyOpenColorIO.Config, view: DisplayView) -> BlenderResult[Document[Coded]]:
            def read_tree(tree: bpy.types.NodeTree, record: LayerStack[Source, M]) -> BlenderResult[tuple[documents.Layers[Coded], npt.NDArray[np.float32]]]:
                path = str(Path(bpy.app.tempdir) / f"{OWNERSHIP.key}_{tree.session_uid}_{at:0{_DIGITS}d}.exr")
                shown = frozenset(layer.key for layer, _ in record.composited())

                def planes(file: Multilayer) -> BlenderResult[tuple[documents.Layers[Coded], npt.NDArray[np.float32]]]:
                    def raw(name: str) -> BlenderResult[npt.NDArray[np.float32]]:
                        return located(next((layer for layer in file.layers if layer.name == name), None), path, name).bind(lambda layer: file.plane(layer, layer.channels))

                    def color(name: str) -> BlenderResult[npt.NDArray[np.float32]]:
                        return raw(name).bind(lambda pixels: view.form(config, pixels)).map(unassociated)

                    def each(layer: Layer[Source, M]) -> BlenderResult[documents.Raster[Coded] | documents.Adjustment[Coded] | documents.Group[Coded]]:
                        visible, blend = layer.key in shown, layer.blend
                        if isinstance(layer.source, Adjustment):
                            return color(f"{layer.key} Composite").map(lambda pixels: documents.Adjustment(name=layer.name, color=coded(pixels[..., :3]), alpha=coded(pixels[..., 3]), visible=visible))
                        mask = raw(f"{layer.key} Mask").map(lambda pixels: Some(coded(pixels[..., 0]))) if blend.masks or layer.matte.enabled else Ok(Nothing)
                        nested = (
                            nested_tree(self.identity, tree, layer)
                            .filter(lambda _: layer.plain)
                            .map(
                                lambda inner: (
                                    self.attached
                                    .read(inner)
                                    .bind(lambda held: read_tree(inner, held))
                                    .map(lambda parts: documents.Group(name=layer.name, layers=parts[0], mode=Some(blend.photoshop), opacity=blend.opacity, visible=visible))
                                )
                            )
                        )
                        return nested.default_with(
                            lambda: paired(color(layer.key), mask).map(
                                lambda parts: documents.Raster(
                                    name=layer.name, color=coded(parts[0][..., :3]), alpha=coded(parts[0][..., 3]), mask=parts[1], mode=blend.photoshop, opacity=blend.opacity, visible=visible
                                )
                            )
                        )

                    return paired(accumulated(each(layer) for layer in record.layers), color("Composite"))

                return Result[Multilayer, BlenderFaults].bind(Multilayer.read(path), planes)

            density = scene.render.ppm_factor / scene.render.ppm_base
            return read_tree(tree, record).bind(
                lambda parts: located(view.colorspace(config).to_optional(), repr(config), f"{view.display}/{view.view}").map(
                    lambda space: Document(layers=parts[0], composite=coded(parts[1]), colorspace=space.getName(), density=density)
                )
            )

        def stack_view() -> BlenderResult[DisplayView]:
            return self.stacks.view(self.identity, scene).map(lambda held: held.default_with(lambda: viewed(scene)))

        return self.head(scene).bind(
            lambda held: self.read(*held).bind(
                lambda record: self.config(record).bind(lambda config: paired(number, stack_view()).bind(lambda parts: composed(held[1], record, parts[0], config, parts[1])))
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def created_tree(name: str) -> BlenderResult[bpy.types.CompositorNodeTree]:
    """Creates an empty compositor node group."""
    return narrowed(bpy.data.node_groups.new(name, "CompositorNodeTree"))


def narrowed(tree: bpy.types.ID | None) -> BlenderResult[bpy.types.CompositorNodeTree]:
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
    looks: Mapping[str, LayerStack[Source, M]] = MappingProxyType({})
    filtering: TermFiltering[M] = TermFiltering()
    view: Callable[[Identity, bpy.types.Scene], BlenderResult[Option[DisplayView]]] = lambda _identity, _scene: Ok(Nothing)
    conditions: Option[Callable[[Identity, bpy.types.SceneCompositorEffect], BlenderResult[dict[Key, Condition]]]] = Nothing

    # --- [DECLARATIONS]

    @cached_property
    def form(self) -> type[LayerStack[Source, M]]:
        """Stack record over the offered effect records."""
        return GenericAlias(LayerStack, (Source, self.rows.form))  # type: ignore[return-value]  # ty: ignore[invalid-return-type]

    @cached_property
    def attachment(self) -> Attachment[LayerStack[Source, M]]:
        """Stack record attached to its node group, a change deferring refresh."""
        return Attachment(key=OWNERSHIP.key, form=self.form, owner=bpy.types.NodeTree, changed=Some(self.changed))

    @cached_property
    def preset(self) -> Preset[LayerStack[Source, M]]:
        """Stack presets over the stock looks."""
        return Preset(key=OWNERSHIP.key, form=self.form, stock=self.looks)

    @cached_property
    def layer_preset(self) -> Preset[Layer[Source, M]]:
        """Presets of one layer."""
        return Preset(key=f"{OWNERSHIP.key}_layer", form=GenericAlias(Layer, (Source, self.rows.form)))  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    @cached_property
    def entry_preset(self) -> Preset[Entry[M]]:
        """Presets of one effect with its blend."""
        return Preset(key=f"{OWNERSHIP.key}_entry", form=GenericAlias(Entry, (self.rows.form,)))  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    @cached_property
    def pinned(self) -> Pinned:
        """Head entry of the stack, pinned with its conditions."""
        return Pinned(ownership=OWNERSHIP, title=TITLE, edge=Edge.HEAD, conditions=self.conditions)

    # --- [COMMANDS]

    def bound_run[T: msgspec.Struct](self, body: Callable[[LayerStacking[M], Call[T]], BlenderResult[Outcome]]) -> Callable[[Call[T]], BlenderResult[Outcome]]:
        """Runs a body over the calling extension's binding."""
        return lambda call: self.bound(call.identity).bind(lambda stacking: body(stacking, call))

    @cached_property
    def creating(self) -> Operator[Empty]:
        """Creates an empty stack."""
        return Operator(key=f"{OWNERSHIP.key}_new", title="New Stack", description="Create layer stack", form=Empty, run=self.bound_run(LayerStacking.created))

    @cached_property
    def stacking(self) -> tuple[Command, ...]:
        """Stack commands beside the layer list."""
        return (
            Operator(key=f"{OWNERSHIP.key}_duplicate", title="Duplicate Stack", description="Copy stack with source settings", form=Empty, run=self.bound_run(LayerStacking.duplicated)),
            Operator(
                key=f"{OWNERSHIP.key}_switch", title="Switch Stack", description="Edit another stack", form=Switched, run=self.bound_run(LayerStacking.switching), invoke=Some(InvokePopup.SEARCH)
            ),
            Operator(key=f"{OWNERSHIP.key}_nested", title="Edit Nested Stack", description="Edit selected layer's stack", form=Empty, run=self.bound_run(LayerStacking.nested)),
            Operator(key="layer_copy", title="Copy Layer", description="Copy one layer of another stack", form=Copied, run=self.bound_run(LayerStacking.copied), invoke=Some(Properties())),
            Operator(
                key="layer_copy_all",
                title="Copy All Layers",
                description="Copy layers of another stack with source settings",
                form=Switched,
                run=self.bound_run(LayerStacking.copied),
                invoke=Some(InvokePopup.SEARCH),
            ),
            Operator(key=f"{OWNERSHIP.key}_clear", title="Clear Layers", description="Remove every layer", form=Empty, run=self.bound_run(LayerStacking.cleared)),
            Operator(key=f"{OWNERSHIP.key}_remove", title="Remove Stack", description="Remove stack and its effect entry", form=Empty, run=self.bound_run(LayerStacking.removed)),
        )

    @cached_property
    def sourcing(self) -> tuple[Command, ...]:
        """Commands adding a layer of each source."""
        extensions = (*sorted(bpy.path.extensions_image), *sorted(bpy.path.extensions_movie))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return (
            Operator(key="layer_add_adjustment", title=Adjustment.title, description="Add layer processing composite beneath", form=Adjustment, run=self.bound_run(LayerStacking.added)),
            Operator(key="layer_add_render_pass", title=RenderPass.title, description="Add view layer output", form=RenderPass, run=self.bound_run(LayerStacking.added), invoke=Some(Properties())),
            Operator(
                key="layer_add_recombined", title=Recombined.title, description="Add beauty from view layer light", form=Recombined, run=self.bound_run(LayerStacking.added), invoke=Some(Properties())
            ),
            Operator(
                key="layer_add_node",
                title=NodeSource.title,
                description="Add source node at first enabled output",
                form=NodeKind,
                run=self.bound_run(LayerStacking.noded),
                invoke=Some(InvokePopup.SEARCH),
                primary=Some(msgspec.structs.fields(NodeKind)[0]),
            ),
            Operator(key="layer_add_nested", title="Nested Stack", description="Add stack as source", form=Switched, run=self.bound_run(LayerStacking.nesting), invoke=Some(InvokePopup.SEARCH)),
            Operator(
                key="layer_add_media", title="Media", description="Add image or movie files", form=Empty, run=self.bound_run(LayerStacking.media), invoke=Some(Open(extensions=lambda _: extensions))
            ),
        )

    @cached_property
    def output(self) -> Operator[OutputChoice]:
        """Sets a layer's source output."""
        return Operator(key="layer_output", title="Select Output", description="Set layer source output", form=OutputChoice, run=self.bound_run(LayerStacking.output))

    @cached_property
    def masking(self) -> Operator[Addressed]:
        """Adds a mask to a layer or effect blend."""
        return Operator(key="layer_mask_add", title="Add Mask", description="Add mask to layer or effect blend", form=Addressed, run=self.bound_run(LayerStacking.masked))

    @cached_property
    def editing(self) -> Mapping[Edit, Operator[Addressed]]:
        """One command per edit of a layer, effect, or mask."""

        def edit(action: Edit) -> Operator[Addressed]:
            title = heading(action.name)
            return Operator(
                key=f"layer_edit_{action.name.lower()}", title=title, description=f"{title} layer, effect, or mask", form=Addressed, run=self.bound_run(lambda held, call: held.edit(action, call))
            )

        return MappingProxyType({action: edit(action) for action in Edit})

    @cached_property
    def effects(self) -> tuple[tuple[CompositorEffect[M], Operator[Empty]], ...]:
        """Accepted effects in rank order, each with the command appending it to the active layer."""

        def effect(row: CompositorEffect[M]) -> Operator[Empty]:
            return Operator(
                key=f"layer_effect_add_{row.kind}", title=row.record.title, description=f"Add {row.record.title} to layer", form=Empty, run=self.bound_run(lambda held, call: held.effected(row, call))
            )

        return tuple((row, effect(row)) for row in sorted(filter(self.accepts, self.rows.rows), key=self.rows.rank))

    @property
    def commands(self) -> tuple[Command, ...]:
        """Commands every binding shares."""
        return (self.creating, *self.stacking, *self.sourcing, self.output, self.masking, *self.editing.values(), *(command for _, command in self.effects))

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
        """Stack commands beside the layer list."""
        return Menu(
            key=f"{OWNERSHIP.key}_specials",
            title=f"{TITLE} Specials",
            description="Stack and layer commands beside the layer list",
            body=Dropdown(entries=tuple(OperatorButton(target=command) for command in self.stacking)),
        )

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Stack and view attachments with the routing and frame size bindings they read."""
        return self.attachment, VIEW, PassSet.attachment, FrameSize.attachment

    @contextmanager
    def changed(self, identity: Identity) -> Generator[Changed]:
        """Defers stack refresh for the attachment's lifetime."""
        with identity.reports.deferred(lambda: self.bound(identity).bind(LayerStacking.refreshed)) as refresh:
            yield lambda _owner, _context, _property: refresh()

    def bound(self, identity: Identity) -> BlenderResult[LayerStacking[M]]:
        """Binds the attachments, pass routing, and framing of an extension."""
        return paired(self.attachment.bound(identity), VIEW.bound(identity), PassSet().bound(identity), framing(identity)).map(
            lambda parts: LayerStacking(identity=identity, stacks=self, attached=parts[0], view=parts[1], routed=parts[2], framing=parts[3])
        )

    def document(self, identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[Coded]]:
        """Returns the layered document of the scene's head stack at a saved frame."""
        return self.bound(identity).bind(lambda stacking: stacking.layered(scene, frame, depth))

    # --- [SECTIONS]

    def section(self, header: tuple[RowContent, ...] = ()) -> Single:
        """Returns the editor section under header rows, presets first."""
        return Single(key=OWNERSHIP.key, title=TITLE, header=(Popover(panel=self.presets().key, text="", icon=Some("PRESET")), *header), rows=(self,), opened=True)

    def presets(self) -> Single:
        """Returns the section of stack, layer, and effect preset listings."""
        return Single(key=f"{OWNERSHIP.key}_presets", title=f"{TITLE} Presets", description=f"Apply, add, or import {TITLE.lower()}, layer, and effect presets", rows=((self, self.presets_drawn),))

    def presets_drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns the preset listing draws of each held stack, layer, or effect, the New Stack button while no stack exists."""

        def gated[T: msgspec.Struct](listing: Listing[T], draw: RowDraw) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                if listing.current(context).is_ok():
                    draw(layout, holder, context)

            return shown

        def draw(stacking: LayerStacking[M], draws: tuple[RowDraw, ...]) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                if rooted(bpy.types.Scene, context).bind(stacking.head).is_error():
                    alerted(layout, pressed(layout, identity, self.creating, Empty(), text=self.creating.title, icon="NONE"))
                for each in draws:
                    each(layout, holder, context)

            return shown

        def listed(stacking: LayerStacking[M]) -> BlenderResult[RowDraw]:
            stack, layer, entry = stacking.listings
            return accumulated((
                stack.drawn(identity).map(lambda shown: gated(stack, shown)),
                layer.drawn(identity).map(lambda shown: gated(layer, shown)),
                entry.drawn(identity).map(lambda shown: gated(entry, shown)),
            )).map(lambda held: draw(stacking, held))

        return self.bound(identity).bind(listed)

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns the editor draw of the extension's binding."""
        return self.bound(identity).bind(LayerStacking.drawn)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Yields commands, menus, listings, handlers, and subscriptions with deferred refresh."""
        watched = ((bpy.types.RenderSettings, "film_transparent"), (bpy.types.ViewLayer, "name"), (bpy.types.ViewLayer, "use"), (bpy.types.Object, "lightgroup"), (bpy.types.World, "lightgroup"))
        with identity.reports.deferred(lambda: self.bound(identity).bind(LayerStacking.refreshed)) as refresh:

            def updated_scenes() -> BlenderResult[None]:
                refresh()
                return Ok(None)

            def rows(stacking: LayerStacking[M]) -> Acquisitions:
                subscriptions = (*stacking.framing.reads, *((owner, owner.bl_rna.properties[name]) for owner, name in watched))
                handlers: tuple[Registrable, ...] = (
                    PathHandler(handlers=LOAD_POST, body=lambda _: updated_scenes()),
                    *(Subscription(owner=owner, setting=prop, body=updated_scenes) for owner, prop in subscriptions),
                )
                return (OWNERSHIP, stacking.listing, self.adding, self.effecting, self.specials, *self.commands, *stacking.listings, *handlers)

            yield self.bound(identity).map(rows)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["OWNERSHIP", "TITLE", "VIEW", "Addressed", "Copied", "LayerStacking", "LayerStacks", "NodeKind", "OutputChoice", "Selection", "StackView", "Switched", "nested_tree", "stacked_by"]
