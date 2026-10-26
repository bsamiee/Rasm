"""Owned layer stacks, source graphs, edits, and documents."""

from collections.abc import Callable, Generator, Iterator, Mapping
from contextlib import contextmanager, ExitStack
from enum import Enum
import functools
import graphlib
import itertools
from pathlib import Path
from types import GenericAlias, MappingProxyType
from typing import Annotated, ClassVar, Final, get_args

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
from msgspec import structs
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.modal import Gesture, Modal, Travel
from blender.bpy.commands.operators import Call, Command, Done, Empty, InvokePopup, Open, Operator, Outcome, Properties, Unchanged
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.ownership import committed, Mark, Ownership, Saved
from blender.bpy.data.ids.store import IdName, named, vacant
from blender.bpy.data.rna.conditions import Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor, root_in, rooted, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.changes import Changes, owned, reconciled
from blender.bpy.nodes.graphs import End, Expr, FileItem, FileItems, FrameNode, Graph, group_input, INPUTS, lowered, math, NodeGroup, OUTPUTS, switched, Term
from blender.bpy.nodes.interface import addresses, Interface, InterfacePanel, item_key, slot, Socket, under
from blender.bpy.nodes.trees import created, grouped
from blender.bpy.render.effects.compositing import masks
from blender.bpy.render.effects.compositing.blending import mixed_back
from blender.bpy.render.effects.compositing.recombination import Beauty, Denoising, RecombinationFlags, Recombined, views
from blender.bpy.render.effects.coordinates import framed
from blender.bpy.render.effects.detail.denoise import denoiser, Filters, guided
from blender.bpy.render.effects.formation import formation, viewed
from blender.bpy.render.effects.records import entered, entry_input, graph_values, inputs, KeyPath, reached, updated
from blender.bpy.render.effects.rows import camera_frame, CameraFrame, CompositorEffect, Derived, expanded, framing, hidden, Shell
from blender.bpy.render.effects.sections import graph_widgets, GraphEditors, GraphWidget
from blender.bpy.render.effects.stack import Edge, EffectStack, Pinned, relocated
from blender.bpy.render.effects.warp.transforms import placed, placement
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.view_layers.passes import filed, Filing, PassRequest, PassRouting, PassSet, required, synced
from blender.bpy.scene.lights.groups import used
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.properties import Changed, Choices, RecordGroup
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import alerted, Divider, Member, OperatorButton, Popover, pressed, Property, Pulldown, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.values import Listing
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.surfaces.menus import Dropdown, Menu, Submenus
from blender.imaging.chain import Domain, Effect, Stage
from blender.imaging.color.processors import DisplayView, unassociated
from blender.imaging.compositing.blending import Blend, Entry
from blender.imaging.compositing.layers import Adjustment, Affine, Layer, LayerStack
from blender.imaging.compositing.masks import Mask, TrackMatte
from blender.imaging.compositing.recombination import Lobes
from blender.imaging.effects.detail.denoise import Frame, Groups, Passes, SpatialDenoise
from blender.imaging.faults import accumulated, paired
from blender.imaging.files import documents
from blender.imaging.files.documents import Document
from blender.imaging.files.images import Depth
from blender.imaging.files.multilayer import Multilayer
from blender.imaging.parameters import Case, Gated, heading

# --- [TYPES] ----------------------------------------------------------------------------

type Source = Adjustment | RenderPass | NodeSource | Recombined
type Target = LayerAt | EffectAt | LayerMaskAt | EffectMaskAt
type Grouped[E: Effect] = tuple[int, Gated[Entry[E]], CompositorEffect[Effect], NodeGroup]


class Edit(Enum):
    """Edit actions on layers, effects, and masks, valued by button icon."""

    REMOVE = "REMOVE"
    DUPLICATE = "DUPLICATE"
    UP = "TRIA_UP"
    DOWN = "TRIA_DOWN"
    RESET = "LOOP_BACK"


# --- [CONSTANTS] ------------------------------------------------------------------------

_FAKE_USER: Final = Descriptor.declared(bpy.types.NodeTree, "use_fake_user", bpy.types.BoolProperty)

# --- [CHOICES] --------------------------------------------------------------------------

stack_names = Choices(lambda identity, _owner, _context: ((tree.name, tree.name, "") for tree, _ in LayerStacks.ownership.held(identity)))
layer_names = Choices(
    lambda identity, owner, _context: (
        (layer.key, layer.name, "")
        for name in identity.reports.reported(identity.name)(Name.ATTACHMENT, LayerStacks.ownership.key).to_list()
        for tree, _ in LayerStacks.ownership.held(identity)
        if tree.name == owner.stack  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        for layer in getattr(tree, name).layers
    )
)
node_types = Choices(
    lambda _identity, _owner, _context: (
        (kind.__name__, kind.bl_rna.name, kind.bl_rna.description)  # type: ignore[attr-defined]
        for name in dir(bpy.types)
        for kind in (getattr(bpy.types, name),)
        if isinstance(kind, type) and issubclass(kind, bpy.types.CompositorNode) and kind.is_registered_node_type()
    )
)

# --- [MODELS] ---------------------------------------------------------------------------


class RenderPass(Case, frozen=True, kw_only=True, tag=2):
    """One output of scene view layer."""

    title = "Render Pass"
    layer: Annotated[str, msgspec.Meta(title="View Layer"), views]
    output: Annotated[str, msgspec.Meta(title="Pass")] = "Image"


class NodeSource(Case, frozen=True, kw_only=True, tag=3):
    """Owned compositor source node with editable settings."""

    title = "Node"
    node: Annotated[str, msgspec.Meta(title="Node Type")]
    output: Annotated[str, msgspec.Meta(title="Output")]


class StackView(msgspec.Struct, frozen=True, kw_only=True):
    """Screen selection, bottom-first layer order."""

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


class LayerAt(Case, frozen=True, kw_only=True, tag=1):
    """Edit of one layer."""

    title = "Layer"
    layer: Annotated[int, msgspec.Meta(ge=0)]


class EffectAt(Case, frozen=True, kw_only=True, tag=2):
    """Edit of one effect within a layer."""

    title = "Effect"
    layer: Annotated[int, msgspec.Meta(ge=0)]
    entry: Annotated[int, msgspec.Meta(ge=0)]


class LayerMaskAt(Case, frozen=True, kw_only=True, tag=3):
    """Edit of one layer mask."""

    title = "Layer Mask"
    layer: Annotated[int, msgspec.Meta(ge=0)]
    mask: Annotated[int, msgspec.Meta(ge=0)]


class EffectMaskAt(Case, frozen=True, kw_only=True, tag=4):
    """Edit of one effect's blend mask."""

    title = "Effect Mask"
    layer: Annotated[int, msgspec.Meta(ge=0)]
    entry: Annotated[int, msgspec.Meta(ge=0)]
    mask: Annotated[int, msgspec.Meta(ge=0)]


class Addressed(msgspec.Struct, frozen=True, kw_only=True):
    """Layer, effect, or mask an edit operator targets."""

    target: Target


class OutputChoice(msgspec.Struct, frozen=True, kw_only=True):
    """Enabled source output chosen for one layer."""

    layer: Annotated[int, msgspec.Meta(ge=0)]
    output: str


class Sourced(msgspec.Struct, frozen=True, kw_only=True):
    """Key and source of one stored layer, read without its effects."""

    key: str
    source: Source


class Valued(msgspec.Struct, frozen=True, kw_only=True):
    """One record the entry's inputs hold: its entry key path, stack record path, value, and the domain its graph values take."""

    keys: KeyPath
    at: KeyPath
    value: msgspec.Struct
    domain: Domain = Domain.SCENE_LINEAR


class Selection[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Head entry, its owned stack and record, and the screen's active layer."""

    entry: bpy.types.SceneCompositorEffect
    tree: bpy.types.NodeTree
    record: LayerStack[Source, E]
    view: StackView

    @property
    def layer(self) -> BlenderResult[Layer[Source, E]]:
        """Active layer."""
        return _at(self.record.layers, self.view.active, LayerStacks.title)


class Dragged[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Selection and row pitch of a layer drag."""

    held: Selection[E]
    pitch: float


class Planning[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Source outputs and dependencies of one graph build."""

    identity: Identity
    scene: bpy.types.Scene
    config: PyOpenColorIO.Config
    luminance: NodeGroup
    denoiser: NodeGroup
    stack: EffectStack[E]
    routed: PassRouting
    offered: dict[Key, dict[str, str]]
    path: OutputPath
    uid: int


class Built(msgspec.Struct, frozen=True, kw_only=True):
    """Root expressions of one layer, its mix-back when composited, the node keys it owns, its interface panel, and its file items."""

    roots: tuple[Expr, ...]
    mixed: Option[Expr]
    nodes: frozenset[Key]
    panel: InterfacePanel
    items: tuple[tuple[FileItem, Expr], ...]


# --- [SERVICES] -------------------------------------------------------------------------


class LayerStacking[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Stack row bound to one extension: attachments, pass flags and routing, framing reads, and the rows built per extension."""

    identity: Identity
    stacks: "LayerStacks[E]"
    attached: Attached[LayerStack[Source, E]]
    view: Attached[StackView]
    flags: RecombinationFlags
    routed: PassRouting
    framing: Derived[Effect, CameraFrame]

    # --- [SELECTION]

    def head(self, scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree]]:
        """Returns first entry holding an owned stack, with that stack."""
        held = next(((entry, tree) for entry in scene.compositor_effects for tree in stacked_by(self.identity, entry).to_list()), None)
        return _found(held, repr(scene.compositor_effects), LayerStacks.ownership.key)

    def named(self, name: str) -> BlenderResult[bpy.types.NodeTree]:
        """Returns the owned stack of a node group name."""
        return _found(next((tree for tree, _ in LayerStacks.ownership.held(self.identity) if tree.name == name), None), repr(bpy.data.node_groups), name)

    def selected(self, context: bpy.types.Context) -> BlenderResult[Selection[E]]:
        """Returns head entry, owned stack, record, and screen selection."""
        held = rooted(bpy.types.Scene, context).bind(self.head).bind(lambda found: self.read(*found).map(lambda record: (*found, record)))
        return paired(held, rooted(bpy.types.Screen, context).bind(self.view.read)).map(lambda parts: Selection(entry=parts[0][0], tree=parts[0][1], record=parts[0][2], view=parts[1]))

    # --- [VALUES]

    def read(self, entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree, *, baseline: Option[LayerStack[Source, E]] = Nothing) -> BlenderResult[LayerStack[Source, E]]:
        """Splices entry inputs into the stored stack, or graph edits into given baseline."""

        def recorded(record: LayerStack[Source, E]) -> BlenderResult[LayerStack[Source, E]]:
            parts, gates = _parts(record), _gates(record)
            editors = accumulated(self.stacks.stack.offering(type(part.value)).map(lambda row: row.editors) if isinstance(part.value, Effect) else Ok(()) for part in parts)
            values = editors.bind(lambda found: graph_values(self.identity, entry, tuple((part.keys, type(part.value), Some(part.value), part.domain, held) for part, held in zip(parts, found)))).bind(
                lambda graphs: accumulated(inputs(self.identity, entry, part.keys, type(part.value), graphs, baseline=baseline.map(lambda _: part.value)) for part in parts)
            )
            toggles = accumulated(
                Ok(held) if baseline.is_some() else entry_input(self.identity, entry, item_key(*keys)).map(lambda owner: bool(owner.path_resolve("value"))) for keys, _, held in gates
            )
            return paired(values, toggles).map(
                lambda read: functools.reduce(
                    lambda held, change: updated(held, change[0], lambda _: change[1]), (*zip((part.at for part in parts), read[0]), *zip((at for _, at, _ in gates), read[1])), record
                )
            )

        return baseline.map(Ok).default_with(lambda: self.attached.read(tree)).bind(recorded)

    def rebuilt(self, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree, record: LayerStack[Source, E], context: bpy.types.Context) -> BlenderResult[None]:
        """Rebuilds graph, then writes record values into entry inputs."""

        def toggled(keys: KeyPath, *, enabled: bool) -> BlenderResult[bool]:
            return entry_input(self.identity, entry, item_key(*keys)).bind(lambda owner: written(owner, "value", enabled)).map(Option.is_some)

        def values() -> BlenderResult[tuple[bool, ...]]:
            return accumulated((*(entered(self.identity, entry, part.keys, part.value) for part in _parts(record)), *(toggled(keys, enabled=enabled) for keys, _, enabled in _gates(record))))

        return self.reconciled(scene, tree, context, record=Some(record)).bind(lambda _: values()).map(lambda _: None)

    def persisted(self, context: bpy.types.Context, held: Selection[E], record: LayerStack[Source, E], active: int) -> BlenderResult[None]:
        """Writes stack and active layer index."""
        rebuilt = rooted(bpy.types.Scene, context).bind(lambda scene: self.rebuilt(scene, held.entry, held.tree, record, context))
        return paired(rebuilt, rooted(bpy.types.Screen, context).bind(lambda screen: self.view.write(screen, StackView(active=active)))).map(lambda _: None)

    def looked(self, scene: bpy.types.Scene, preset: LayerStack[Source, E], strength: int, context: bpy.types.Context) -> BlenderResult[bool]:
        """Writes a preset over the head stack at `strength` percent of each effect's change from its record defaults, creating the stack when absent, answering whether it changed."""

        def held() -> BlenderResult[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree]]:
            return self.head(scene).to_option().map(Ok).default_with(lambda: self.made(scene, context).bind(lambda _: self.head(scene)))

        def placed(record: LayerStack[Source, E], entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree) -> BlenderResult[bool]:
            return self.read(entry, tree).bind(lambda current: Ok(False) if current == record else self.rebuilt(scene, entry, tree, record, context).map(lambda _: True))

        return self.stacks.stack.config().bind(lambda config: strengthened(preset, strength / 100, config)).bind(lambda record: held().bind(lambda found: placed(record, *found)))

    def replaced(self, context: bpy.types.Context, held: Selection[E], record: LayerStack[Source, E], active: int) -> BlenderResult[bool]:
        """Writes stack and active index when the stack changed, answering whether it did."""
        return Ok(False) if record == held.record else self.persisted(context, held, record, active).map(lambda _: True)

    def appended(self, context: bpy.types.Context, held: Selection[E], at: KeyPath, item: object, active: int) -> BlenderResult[bool]:
        """Appends an item to the collection at a record path, answering whether the stack changed."""
        return _reached(held.record, at).bind(lambda found: self.replaced(context, held, updated(held.record, at, lambda _: (*found, item)), active))

    # --- [GRAPH]

    def sources(self, scene: bpy.types.Scene, layers: LayerStack[Source, E], tree: bpy.types.NodeTree) -> BlenderResult[dict[Key, dict[str, str]]]:
        """Reads enabled source outputs from scratch graph with settings copied from the stack's owned source nodes."""
        with ExitStack() as release:
            scratch: bpy.types.CompositorNodeTree = bpy.data.node_groups.new("Layer Sources", "CompositorNodeTree")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            release.callback(bpy.data.node_groups.remove, scratch)

            def copied(_: Changes) -> BlenderResult[dict[Key, dict[str, str]]]:
                found = owned(scratch, self.identity)
                return accumulated(Snapshot.capture(source).bind(lambda snapshot: snapshot.apply(found[key]).outcome) for key, source in owned(tree, self.identity).items() if key in found).map(
                    lambda _: {key: {output.name: output.identifier for output in source.outputs if output.enabled} for key, source in found.items()}
                )

            return (
                accumulated(sourced(layer.key, source, scene) for layer in layers.layers for source in (layer.source,) if not isinstance(source, Adjustment))
                .bind(lambda found: lowered(*found))
                .bind(lambda body: reconciled(Graph(kind=bpy.types.CompositorNodeTree, nodes=body.nodes), scratch, self.identity))
                .bind(copied)
            )

    def reconciled(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, context: bpy.types.Context, *, record: Option[LayerStack[Source, E]] = Nothing) -> BlenderResult[bool]:
        """Enables source pass and denoising guide requests, builds graph, and writes frame inputs, answering whether any changed."""

        def requested(layer: Layer[Source, E]) -> BlenderResult[tuple[PassRequest, ...]]:
            match layer.source:
                case RenderPass(layer=name):
                    return Ok((PassRequest(layer=name),))
                case Recombined(layer=name) as source if any(isinstance(gated.value.effect, SpatialDenoise) for gated in layer.effects.value):
                    return guided(name).map(lambda guide: (source.requested(self.flags), guide))
                case Recombined() as source:
                    return Ok((source.requested(self.flags),))
                case Adjustment() | NodeSource():
                    return Ok(())

        def built(layers: LayerStack[Source, E], config: PyOpenColorIO.Config, path: OutputPath) -> BlenderResult[bool]:
            names = tuple(sorted(used(scene)))
            layers = structs.replace(layers, layers=tuple(structs.replace(layer, source=layer.source.synced(names)) if isinstance(layer.source, Recombined) else layer for layer in layers.layers))
            groups = masks.luminance(config).bind(lambda working: denoiser(working).map(lambda filtered: (working, filtered)))
            frames = FrameSize().bound(self.identity).bind(lambda framed: camera_frame(framed, scene))

            def planning(parts: tuple[dict[Key, dict[str, str]], tuple[NodeGroup, NodeGroup]]) -> Planning[E]:
                (working, filtered), offered = parts[1], parts[0]
                return Planning(
                    identity=self.identity,
                    scene=scene,
                    config=config,
                    luminance=working,
                    denoiser=filtered,
                    stack=self.stacks.stack,
                    routed=self.routed,
                    offered=offered,
                    path=path,
                    uid=tree.session_uid,
                )

            return (
                synced(scene, self.identity)
                .bind(lambda _: self.attached.write(tree, layers))
                .bind(lambda _: accumulated(map(requested, layers.layers)))
                .bind(lambda wanted: required(scene, context, tuple(itertools.chain.from_iterable(wanted)), self.routed.precision))
                .bind(lambda _: paired(self.sources(scene, layers, tree), groups))
                .bind(lambda parts: planned(layers, planning(parts)))
                .bind(lambda group: reconciled(group, tree, self.identity))
                .bind(
                    lambda changes: frames.bind(lambda frame: accumulated(entered(self.identity, entry, (Shell.frame,), frame) for entry in scene.compositor_effects if entry.node_group == tree)).map(
                        lambda wrote: any(wrote) or changes != Changes()
                    )
                )
            )

        desired = record.map(Ok).default_with(
            lambda: self.attached.read(tree).bind(
                lambda stored: _found(next((entry for entry in scene.compositor_effects if entry.node_group == tree), None), repr(scene.compositor_effects), tree.name).bind(
                    lambda entry: self.read(entry, tree, baseline=Some(stored))
                )
            )
        )
        return paired(desired, self.stacks.stack.config(), self.routed.path(scene)).bind(lambda parts: built(*parts))

    def refreshed(self) -> BlenderResult[Option[str]]:
        """Reconciles each scene stack, naming undo step when any changed."""
        return accumulated(
            self.reconciled(scene, tree, bpy.context)
            for scene in bpy.data.scenes
            for tree in dict.fromkeys(tree for entry in scene.compositor_effects for tree in stacked_by(self.identity, entry).to_list())
        ).map(lambda changed: Some(f"Refresh {LayerStacks.title}s") if any(changed) else Nothing)

    # --- [STACKS]

    def switched(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, context: bpy.types.Context) -> BlenderResult[None]:
        """Places stack in head entry, creating it when absent."""
        held = self.head(scene).to_option()
        with ExitStack() as rollback:
            entry = held.map(lambda found: found[0]).default_with(lambda: scene.compositor_effects.new(vacant(LayerStacks.title, (item.name for item in scene.compositor_effects))))
            for _ in () if held.is_some() else (entry,):
                rollback.callback(scene.compositor_effects.remove, entry)
            return committed(written(entry, "node_group", tree).bind(lambda _: relocated(scene, entry, 0)).bind(lambda _: self.reconciled(scene, tree, context)).map(lambda _: None), rollback)

    def made(self, scene: bpy.types.Scene, context: bpy.types.Context) -> BlenderResult[bpy.types.CompositorNodeTree]:
        """Creates an empty stack and places it in the head entry."""

        def fresh() -> BlenderResult[bpy.types.CompositorNodeTree]:
            group: bpy.types.CompositorNodeTree = bpy.data.node_groups.new(vacant(LayerStacks.title, bpy.data.node_groups.keys()), "CompositorNodeTree")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return Ok(group)

        return LayerStacks.ownership.made(self.identity, fresh, of=Some(scene)).bind(
            lambda group: self.attached.write(group, self.stacks.form()).bind(lambda _: self.switched(scene, group, context)).map(lambda _: group)
        )

    def inserted(self, context: bpy.types.Context, layers: tuple[Layer[Source, E], ...], source: Option[bpy.types.NodeTree] = Nothing) -> BlenderResult[bool]:
        """Inserts layers after the active layer under fresh keys with source node settings copied, answering whether any was given."""

        def insert(held: Selection[E]) -> BlenderResult[bool]:
            added = tuple(structs.replace(layer, key=key) for layer, key in zip(layers, _fresh(held.record)))
            at = min(held.view.active + 1, len(held.record.layers))
            nodes = tuple(
                (node, f"{after.key}_source")
                for tree in source.to_list()
                for before, after in zip(layers, added)
                for node in Option.of_optional(owned(tree, self.identity).get(f"{before.key}_source")).to_list()
            )

            def copy(node: bpy.types.Node, key: Key) -> BlenderResult[None]:
                return Snapshot.capture(node).bind(
                    lambda snapshot: created(held.tree, type(node)).bind(lambda made: snapshot.apply(made).outcome.bind(lambda _: Mark(key=key).write(self.identity, made)))
                )

            with ExitStack() as rollback:
                for _, key in nodes:
                    rollback.callback(_dropped, held.tree, self.identity, key)
                record = structs.replace(held.record, layers=(*held.record.layers[:at], *added, *held.record.layers[at:]))
                return committed(accumulated(itertools.starmap(copy, nodes)).bind(lambda _: self.replaced(context, held, record, at + len(added) - 1 if added else held.view.active)), rollback)

        return self.selected(context).bind(insert)

    def added_node[T](self, context: bpy.types.Context, kind: type[bpy.types.Node], setup: Callable[[bpy.types.Node], BlenderResult[T]]) -> BlenderResult[None]:
        """Adds source node layer after the active layer at the node's first enabled output."""

        def add(held: Selection[E]) -> BlenderResult[None]:
            key, at = next(_fresh(held.record)), min(held.view.active + 1, len(held.record.layers))

            def placed(node: bpy.types.Node) -> BlenderResult[None]:
                return _found(next((output.name for output in node.outputs if output.enabled), None), kind.__name__, "outputs").bind(
                    lambda output: self.persisted(
                        context,
                        held,
                        structs.replace(
                            held.record, layers=(*held.record.layers[:at], Layer(key=key, name=node.bl_label, source=NodeSource(node=kind.__name__, output=output)), *held.record.layers[at:])
                        ),
                        at,
                    )
                )

            with ExitStack() as rollback:
                rollback.callback(_dropped, held.tree, self.identity, f"{key}_source")
                return committed(created(held.tree, kind).bind(lambda node: Mark(key=f"{key}_source").write(self.identity, node).bind(lambda _: setup(node)).bind(lambda _: placed(node))), rollback)

        return self.selected(context).bind(add)

    # --- [COMMANDS]

    def created(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Creates an empty stack and places it in the head entry."""
        return call.root(bpy.types.Scene).bind(lambda scene: self.made(scene, call.context)).map(lambda _: Done())

    def duplicated(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Copies the head stack with its source settings and places the copy."""

        def cloned(scene: bpy.types.Scene, tree: bpy.types.NodeTree) -> BlenderResult[None]:
            def copied() -> BlenderResult[bpy.types.CompositorNodeTree]:
                group: bpy.types.CompositorNodeTree = tree.copy()  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                return named(group, vacant(LayerStacks.title, bpy.data.node_groups.keys()))

            return LayerStacks.ownership.made(self.identity, copied, of=Some(scene)).bind(lambda copy: self.switched(scene, copy, call.context))

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
        inner = self.selected(call.context).bind(lambda held: held.layer.bind(lambda layer: _found(nested_tree(self.identity, held.tree, layer).to_optional(), LayerStacks.title, "nested stack")))
        return paired(call.root(bpy.types.Scene), inner).bind(lambda parts: self.switched(*parts, call.context)).map(lambda _: Done())

    def cleared(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Removes every layer."""
        wrote = self.selected(call.context).bind(lambda held: self.replaced(call.context, held, structs.replace(held.record, layers=()), 0))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason="Stack holds no layers"))

    def copied(self, call: Call[Copied | Switched]) -> BlenderResult[Outcome]:
        """Inserts one or every layer of another stack with its source settings."""
        wanted = Some(call.record.layer) if isinstance(call.record, Copied) else Nothing

        def chosen(group: bpy.types.NodeTree, record: LayerStack[Source, E]) -> BlenderResult[Outcome]:
            layers = wanted.map(lambda key: _found(next((held for held in record.layers if held.key == key), None), group.name, key).map(lambda held: (held,))).default_value(Ok(record.layers))
            return layers.bind(lambda picked: self.inserted(call.context, picked, Some(group))).map(lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers"))

        def valued(group: bpy.types.NodeTree) -> BlenderResult[LayerStack[Source, E]]:
            entry = next((entry for scene in bpy.data.scenes for entry in scene.compositor_effects if entry.node_group == group), None)
            return Option.of_optional(entry).map(lambda held: self.read(held, group)).default_with(lambda: self.attached.read(group))

        return self.named(call.record.stack).bind(lambda group: valued(group).bind(lambda record: chosen(group, record)))

    def added[S: Source](self, call: Call[S]) -> BlenderResult[Outcome]:
        """Inserts a layer of the called source."""
        return self.inserted(call.context, (Layer(key="inserted", name=type(call.record).title, source=call.record),)).map(lambda _: Done())

    def noded(self, call: Call[NodeKind]) -> BlenderResult[Outcome]:
        """Adds a source node layer of the called class."""
        return node_kind(call.record.node).bind(lambda kind: self.added_node(call.context, kind, Ok)).map(lambda _: Done())

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

        def chosen(held: Selection[E], layer: Layer[Source, E]) -> BlenderResult[bool]:
            match layer.source:
                case RenderPass() | NodeSource():
                    return self.replaced(call.context, held, updated(held.record, at, lambda _: call.record.output), index)
                case Adjustment() | Recombined():
                    return Error((BlenderAbsentFault(owner=layer.name, key=at[-1]),))

        wrote = self.selected(call.context).bind(lambda held: _at(held.record.layers, index, LayerStacks.title).bind(lambda layer: chosen(held, layer)))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason=f"Layer reads {call.record.output}"))

    def masked(self, call: Call[Addressed]) -> BlenderResult[Outcome]:
        """Adds a mask to the blend of the targeted layer or effect."""
        return self.selected(call.context).bind(lambda held: self.appended(call.context, held, _masking(call.record.target), Mask(), call.record.target.layer)).map(lambda _: Done())

    def effected(self, row: CompositorEffect[E], call: Call[Empty]) -> BlenderResult[Outcome]:
        """Appends the row's seeded effect to the active layer."""

        def appended(held: Selection[E], effect: E) -> BlenderResult[bool]:
            return self.appended(call.context, held, ("layers", held.view.active + 1, "effects", "value"), Gated(enabled=True, value=Entry(effect=effect)), held.view.active)

        return paired(call.root(bpy.types.Scene).bind(row.seeded), self.selected(call.context)).bind(lambda parts: appended(parts[1], parts[0])).map(lambda _: Done())

    def edit(self, action: Edit, call: Call[Addressed]) -> BlenderResult[Outcome]:
        """Applies an edit to the target, a layer duplicate copying its source node settings."""
        target = call.record.target

        def seeded(scene: bpy.types.Scene) -> Callable[[E], BlenderResult[E]]:
            return lambda effect: _found(self.stacks.rows.get(type(effect)), LayerStacks.title, item_key(type(effect))).bind(lambda row: row.seeded(scene))

        def applied(scene: bpy.types.Scene, held: Selection[E]) -> BlenderResult[bool]:
            match target:
                case LayerAt(layer=index) if action is Edit.DUPLICATE:
                    return _at(held.record.layers, index, LayerStacks.title).bind(lambda layer: self.inserted(call.context, (layer,), Some(held.tree)))
                case _:
                    return edited(held.record, action, target, seeded(scene)).bind(lambda edit: self.replaced(call.context, held, *edit))

        wrote = paired(call.root(bpy.types.Scene), self.selected(call.context)).bind(lambda parts: applied(*parts))
        return wrote.map(lambda changed: Done() if changed else Unchanged(reason=f"{action.name.title()} changes nothing"))

    def applied(self, context: bpy.types.Context, value: LayerStack[Source, E] | Layer[Source, E] | Entry[E]) -> BlenderResult[Outcome]:
        """Applies a stack, layer, or effect preset over the selection."""

        def edited(held: Selection[E]) -> BlenderResult[bool]:
            active = ("layers", held.view.active + 1)
            match value:
                case LayerStack():
                    return self.replaced(context, held, value, 0)
                case Layer():
                    return held.layer.bind(lambda prior: self.replaced(context, held, updated(held.record, active, lambda _: structs.replace(value, key=prior.key)), held.view.active))
                case Entry():
                    return self.appended(context, held, (*active, "effects", "value"), Gated(enabled=True, value=value), held.view.active)

        return self.selected(context).bind(edited).map(lambda wrote: Done() if wrote else Unchanged(reason="Stack matches preset"))

    # --- [ROWS]

    @property
    def specials(self) -> Menu:
        """Stack and layer commands beside the layer list, with the drag holding this binding."""

        def moved(context: bpy.types.Context, state: Dragged[E], travel: Travel) -> BlenderResult[Option[str]]:
            layers, index = state.held.record.layers, state.held.view.active
            target = min(max(index + round(travel.dy / state.pitch), 0), max(len(layers) - 1, 0))
            rest = (*layers[:index], *layers[index + 1 :])
            return self.persisted(context, state.held, structs.replace(state.held.record, layers=(*rest[:target], *layers[index : index + 1], *rest[target:])), target).map(lambda _: Nothing)

        drag = Modal[Dragged[E]](
            gesture=Gesture.DRAG_Y,
            begin=lambda context, _travel: self.selected(context).map(lambda held: Dragged(held=held, pitch=Metrics.read().unit)),
            preview=moved,
            restore=lambda context, state: self.persisted(context, state.held, state.held.record, state.held.view.active),
        )
        dragging = Operator[Empty, Dragged[E]](key="layer_drag", title="Drag Layer", description="Drag selected layer by list rows", form=Empty, run=lambda _: Ok(Done()), invoke=Some(drag))
        stacks = self.stacks
        entries = (stacks.duplicating, stacks.switching, stacks.nesting, *stacks.copying, dragging, stacks.clearing, stacks.removing)
        return Menu(
            key=f"{LayerStacks.ownership.key}_specials",
            title=f"{LayerStacks.title} Specials",
            description="Stack and layer commands beside the layer list",
            body=Dropdown(entries=tuple(OperatorButton(target=command) for command in entries)),
        )

    @property
    def listings(self) -> tuple[Listing[LayerStack[Source, E]], Listing[Layer[Source, E]], Listing[Entry[E]]]:
        """Stack, active layer, and last effect preset listings."""

        def listing[T: msgspec.Struct](read: Callable[[bpy.types.Context], BlenderResult[T]], preset: Preset[T], applied: Callable[[bpy.types.Context, T], BlenderResult[Outcome]]) -> Listing[T]:
            return Listing(preset=preset, current=read, matches=lambda context, values: read(context).map(lambda held: tuple(held == value for value in values)), applied=applied)

        def last(held: Selection[E]) -> BlenderResult[Entry[E]]:
            return held.layer.bind(lambda layer: _found(next((gated.value for gated in layer.effects.value[-1:]), None), layer.name, "effects"))

        form, key = self.stacks.form, LayerStacks.ownership.key
        return (
            listing(lambda context: self.selected(context).map(lambda held: held.record), self.stacks.preset, self.applied),
            listing(lambda context: self.selected(context).bind(lambda held: held.layer), Preset(key=f"{key}_layer", form=GenericAlias(Layer, get_args(form))), self.applied),  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]
            listing(lambda context: self.selected(context).bind(last), Preset(key=f"{key}_entry", form=GenericAlias(Entry, get_args(form)[1:])), self.applied),  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]
        )

    @property
    def listing(self) -> ItemList:
        """Layer list with visibility columns and edit side buttons."""
        layer = self.attached.group.struct.bl_rna.properties["layers"].fixed_type  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return ItemList(
            key=f"{LayerStacks.ownership.key}_layers",
            item=type(layer),
            collection=Member(
                root=bpy.types.NodeTree,
                path=f"{self.attached.name}.layers",
                reach=Some(lambda context: root_in(bpy.types.Scene, context).bind(lambda scene: self.head(scene).to_option()).map(lambda held: held[1])),
            ),
            active=Member(root=bpy.types.Screen, path=f"{self.view.name}.active", reach=Some(lambda context: Option.of_optional(context.screen))),
            columns=tuple(Property(target=Member(root=type(layer), path=name), text="") for name in ("visible", "solo", "matte.enabled")),
            enabled=Some(lambda context, item: self.selected(context).map(lambda held: any(shown.key == item.path_resolve("key") for shown, _ in held.record.composited()))),
            sort_reverse=True,
            side=(
                Pulldown(menu=self.stacks.adding.key, text="", icon=Some("ADD")),
                self.button(Edit.REMOVE),
                Divider(type="SPACE"),
                Pulldown(menu=self.specials.key, text="", icon=Some("DOWNARROW_HLT")),
                Divider(type="SPACE"),
                *map(self.button, (Edit.DUPLICATE, Edit.UP, Edit.DOWN)),
            ),
            absent=(OperatorButton(target=self.stacks.creating),),
        )

    # --- [EDITOR]

    def button(self, action: Edit) -> RowDraw:
        """Returns icon button of one edit over the active layer."""

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            for held in self.selected(context).to_option().to_list():
                alerted(layout, pressed(layout, self.identity, self.stacks.editing[action], Addressed(target=LayerAt(layer=held.view.active)), text="", icon=action.value))

        return shown

    def drawn(self) -> BlenderResult[RowDraw]:
        """Returns draw of layer list, then selected layer source, inputs, and widgets, and one section per layer effect."""
        stacks = self.stacks

        def edits(layout: bpy.types.UILayout, target: Target) -> None:
            line = layout.row(align=True)
            for action in Edit:
                alerted(line, pressed(line, self.identity, stacks.editing[action], Addressed(target=target), text="", icon=action.value))

        def mask(layout: bpy.types.UILayout, target: LayerAt | EffectAt) -> None:
            alerted(layout, pressed(layout, self.identity, stacks.masking, Addressed(target=target), text=stacks.masking.title, icon="MOD_MASK"))

        def draw(listing: RowDraw, adding: RowDraw, editors: GraphEditors) -> RowDraw:
            def widgets(layout: bpy.types.UILayout, held: Selection[E], prefix: Key, inner: tuple[Key, ...], graphs: tuple[GraphWidget, ...], context: bpy.types.Context) -> None:
                nodes = owned(held.tree, self.identity)
                for widget, live in ((shown, live) for shown in graphs for live in Option.of_optional(nodes.get(shown.node)).to_list()):
                    alerted(layout, editors.drawn(layout, live, widget, context))
                for key in (key for keys in slot(self.identity).map(lambda name: addresses(held.tree, name)).to_option().to_list() for key in keys):
                    if key.startswith(f"{prefix}_") and not any(key.startswith(f"{nested}_") for nested in inner):
                        alerted(layout, entry_input(self.identity, held.entry, key).map(lambda owner: layout.prop(owner, "value")))

            def effected(layout: bpy.types.UILayout, held: Selection[E], at: EffectAt, place: KeyPath, gated: Gated[Entry[E]], context: bpy.types.Context) -> None:
                header, body = layout.panel(f"{item_key(*place)}_{held.tree.session_uid}", default_closed=False)
                header.label(text=type(gated.value.effect).title)
                edits(header, at)
                for inner in Option.of_optional(body).to_list():
                    shown = stacks.stack.offering(type(gated.value.effect)).map(
                        lambda row: (*graph_widgets(gated.value.effect, place, row.editors), *graph_widgets(gated.value.blend, (*place, Shell.blend)))
                    )
                    alerted(inner, shown.map(lambda graphs: widgets(inner, held, item_key(*place), (), graphs, context)))
                    mask(inner, at)
                    for index in range(len(gated.value.blend.masks)):
                        edits(inner, EffectMaskAt(layer=at.layer, entry=at.entry, mask=index))

            def detail(layout: bpy.types.UILayout, held: Selection[E], layer: Layer[Source, E], context: bpy.types.Context) -> None:
                at = LayerAt(layer=held.view.active)
                native = self.attached.struct(held.tree).layers[at.layer]  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                places = tuple((layer.key, "effects", index) for index in range(1, len(layer.effects.value) + 1))
                for field in (field for field in structs.fields(type(layer.source)) if isinstance(getattr(layer.source, field.name), str)):
                    layout.prop(getattr(native.source, type(layer.source).__name__), field.name)
                for source in Option.of_optional(owned(held.tree, self.identity).get(f"{layer.key}_source")).to_list():
                    source.draw_buttons_ext(context, layout)
                    layout.template_node_inputs(source)
                    outputs = layout.row(align=True)
                    for output in (output for output in source.outputs if output.enabled):
                        alerted(outputs, pressed(outputs, self.identity, stacks.output, OutputChoice(layer=at.layer, output=output.name), text=output.name, icon="NONE"))
                graphs = tuple(widget for name, value in layer.parts for widget in graph_widgets(value, (layer.key, name)))
                widgets(layout, held, layer.key, tuple(itertools.starmap(item_key, places)), graphs, context)
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
        return paired(self.listing.drawn(self.identity), adding, GraphEditors.realized(self.identity)).map(lambda parts: draw(*parts))

    # --- [DOCUMENTS]

    def layered(self, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[np.uint8 | np.uint16 | np.float32]]:
        """Builds document of formed color and raw mask coverage, nested stacks as groups."""
        number = _found(next((part.header.get("Frame") for part in frame.parts[:1]), None), frame.path, "Frame").bind(
            lambda text: catch(lambda: int(text), exception=ValueError)().map_error(lambda error: (BlenderEditFault(request=frame.path, error=error),))
        )

        def built[D: np.uint8 | np.uint16 | np.float32](coded: Callable[[npt.NDArray[np.float32]], npt.NDArray[D]]) -> BlenderResult[Document[D]]:
            def composed(tree: bpy.types.NodeTree, record: LayerStack[Source, E], at: int, config: PyOpenColorIO.Config, view: Option[DisplayView]) -> BlenderResult[Document[D]]:
                def read_tree(tree: bpy.types.NodeTree, record: LayerStack[Source, E]) -> BlenderResult[tuple[documents.Layers[D], npt.NDArray[np.float32]]]:
                    path = str(Path(bpy.app.tempdir) / f"layer_stack_{tree.session_uid}_{at:0{LayerStacks.digits}d}.exr")
                    shown = frozenset(layer.key for layer, _ in record.composited())

                    def planes(file: Multilayer) -> BlenderResult[tuple[documents.Layers[D], npt.NDArray[np.float32]]]:
                        def raw(name: str) -> BlenderResult[npt.NDArray[np.float32]]:
                            return _found(next((layer for layer in file.layers if layer.name == name), None), path, name).bind(lambda layer: file.plane(layer, layer.channels))

                        def color(name: str) -> BlenderResult[npt.NDArray[np.float32]]:
                            def formed(pixels: npt.NDArray[np.float32]) -> BlenderResult[npt.NDArray[np.float32]]:
                                settings = scene.view_settings
                                return view.map(lambda held: held.form(config, pixels)).default_with(lambda: viewed(scene).form(config, pixels, exposure=settings.exposure, gamma=settings.gamma))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

                            return raw(name).bind(formed).map(unassociated)

                        def each(layer: Layer[Source, E]) -> BlenderResult[documents.Raster[D] | documents.Adjustment[D] | documents.Group[D]]:
                            visible, blend = layer.key in shown, layer.blend
                            if isinstance(layer.source, Adjustment):
                                return color(f"{layer.key} Composite").map(
                                    lambda pixels: documents.Adjustment(name=layer.name, color=coded(pixels[..., :3]), alpha=coded(pixels[..., 3]), visible=visible)
                                )
                            plain = layer.affine == Affine() and not blend.masks and not layer.matte.enabled and not (layer.effects.enabled and any(gated.enabled for gated in layer.effects.value))
                            mask = raw(f"{layer.key} Mask").map(lambda pixels: Some(coded(pixels[..., 0]))) if blend.masks or layer.matte.enabled else Ok(Nothing)
                            grouped = (
                                nested_tree(self.identity, tree, layer)
                                .filter(lambda _: plain)
                                .map(
                                    lambda inner: (
                                        self.attached
                                        .read(inner)
                                        .bind(lambda held: read_tree(inner, held))
                                        .map(lambda parts: documents.Group(name=layer.name, layers=parts[0], mode=Some(blend.mode), opacity=blend.opacity, visible=visible))
                                    )
                                )
                            )
                            return grouped.default_with(
                                lambda: paired(color(layer.key), mask).map(
                                    lambda parts: documents.Raster(
                                        name=layer.name, color=coded(parts[0][..., :3]), alpha=coded(parts[0][..., 3]), mask=parts[1], mode=blend.mode, opacity=blend.opacity, visible=visible
                                    )
                                )
                            )

                        return paired(accumulated(each(layer) for layer in record.layers), color("Composite"))

                    return Result[Multilayer, BlenderFaults].bind(Multilayer.read(path), planes)

                shown = view.default_with(lambda: viewed(scene))
                density = scene.render.ppm_factor / scene.render.ppm_base
                return read_tree(tree, record).bind(
                    lambda parts: _found(shown.colorspace(config).to_optional(), repr(config), f"{shown.display}/{shown.view}").map(
                        lambda space: Document(layers=parts[0], composite=coded(parts[1]), colorspace=space.getName(), density=density)
                    )
                )

            return paired(number, self.stacks.stack.config(), formation.current(self.identity, scene)).bind(
                lambda parts: self.head(scene).bind(lambda held: self.read(*held).bind(lambda record: composed(held[1], record, *parts)))
            )

        return built(depth.coded)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOOKUPS]


def _found[T](value: T | None, owner: str, key: str) -> BlenderResult[T]:
    """Answers a lookup, absent when it found nothing."""
    return Option.of_optional(value).to_result_with(lambda: (BlenderAbsentFault(owner=owner, key=key),))


def _at[T](items: tuple[T, ...], index: int, owner: str) -> BlenderResult[T]:
    """Answers the item at a zero-based index, absent past the collection."""
    return _found(items[index] if index < len(items) else None, owner, str(index))


def _reached(record: msgspec.Struct, path: KeyPath) -> BlenderResult[tuple[object, ...]]:
    """Answers the collection at a record path, absent where an index on the way reaches past its collection."""
    match catch(lambda: reached(record, path), exception=IndexError)():
        case Result(tag="ok", ok=tuple(items)):
            return Ok(items)
        case _:
            return Error((BlenderAbsentFault(owner=type(record).__name__, key=item_key(*path)),))


def _fresh[E: Effect](record: LayerStack[Source, E]) -> Iterator[Key]:
    """Layer keys the stack holds no layer under, in order."""
    taken = frozenset(layer.key for layer in record.layers)
    return (key for index in itertools.count(1) for key in (f"layer_{index}",) if key not in taken)


def _dropped(tree: bpy.types.NodeTree, identity: Identity, key: Key) -> None:
    """Removes the owned node of a key when the tree still holds it."""
    for node in Option.of_optional(owned(tree, identity).get(key)).to_list():
        tree.nodes.remove(node)


def _parts[E: Effect](record: LayerStack[Source, E]) -> tuple[Valued, ...]:
    """Layer source, placement, blend, and matte, then each effect and its blend, the records entry inputs hold in layer order."""
    return tuple(
        part
        for index, layer in enumerate(record.layers, 1)
        for part in (
            *(Valued(keys=(layer.key, name), at=("layers", index, name, *(("value",) if isinstance(getattr(layer, name), Gated) else ())), value=value) for name, value in layer.parts),
            *(
                held
                for number, gated in enumerate(layer.effects.value, 1)
                for at, domain in ((("layers", index, "effects", "value", number, "value"), gated.value.effect.stage.domain),)
                for held in (
                    Valued(keys=(layer.key, "effects", number), at=(*at, "effect"), value=gated.value.effect, domain=domain),
                    Valued(keys=(layer.key, "effects", number, Shell.blend), at=(*at, Shell.blend), value=gated.value.blend, domain=domain),
                )
            ),
        )
    )


def _gates[E: Effect](record: LayerStack[Source, E]) -> tuple[tuple[KeyPath, KeyPath, bool], ...]:
    """Entry key path, record path, and value of each layer's effects gate and each effect's gate."""
    return tuple(
        gate
        for index, layer in enumerate(record.layers, 1)
        for gate in (
            ((layer.key, "effects"), ("layers", index, "effects", "enabled"), layer.effects.enabled),
            *(((layer.key, "effects", number), ("layers", index, "effects", "value", number, "enabled"), gated.enabled) for number, gated in enumerate(layer.effects.value, 1)),
        )
    )


def stacked_by(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Option[bpy.types.NodeTree]:
    """Returns entry node group when it holds stack ownership mark."""
    return Option.of_optional(entry.node_group).filter(lambda group: LayerStacks.ownership.marked(identity, group).is_some())


def nested_tree[E: Effect](identity: Identity, tree: bpy.types.NodeTree, layer: Layer[Source, E]) -> Option[bpy.types.NodeTree]:
    """Returns owned stack of layer source group node."""
    match owned(tree, identity).get(f"{layer.key}_source"):
        case bpy.types.CompositorNodeGroup(node_tree=inner) if inner is not None:
            return Some(inner).filter(lambda tree: LayerStacks.ownership.marked(identity, tree).is_some())
        case _:
            return Nothing


def node_kind(identifier: str) -> BlenderResult[type[bpy.types.Node]]:
    """Returns the node class `bpy.types` names by identifier."""
    match getattr(bpy.types, identifier, None):
        case type() as kind if issubclass(kind, bpy.types.Node):
            return Ok(kind)
        case _:
            return Error((BlenderAbsentFault(owner="bpy.types", key=identifier),))


# --- [EDITS]


def _masking(target: Target) -> KeyPath:
    """Record path of the blend masks of the layer or effect a target names."""
    match target:
        case LayerAt(layer=layer) | LayerMaskAt(layer=layer):
            return ("layers", layer + 1, "blend", "masks")
        case EffectAt(layer=layer, entry=entry) | EffectMaskAt(layer=layer, entry=entry):
            return ("layers", layer + 1, "effects", "value", entry + 1, "value", "blend", "masks")


def edited[E: Effect](record: LayerStack[Source, E], action: Edit, target: Target, seeded: Callable[[E], BlenderResult[E]]) -> BlenderResult[tuple[LayerStack[Source, E], int]]:
    """Applies an edit to a target's item, answering the edited stack and the layer it leaves active.

    Args:
        record: Stack holding the target.
        action: Edit applied.
        target: Layer, effect, or mask the edit applies to.
        seeded: Defaults of an effect a reset restores.

    Returns:
        `Ok` of edited stack and active layer index. `Error` of `absent` for a target past its collection and of seed faults.
    """
    match target:
        case LayerAt(layer=index):
            path: KeyPath = ("layers",)
        case EffectAt(layer=layer, entry=index):
            path = ("layers", layer + 1, "effects", "value")
        case LayerMaskAt(mask=index) | EffectMaskAt(mask=index):
            path = _masking(target)

    def fresh(item: object) -> BlenderResult[object]:
        match item:
            case Layer():
                return Ok(structs.replace(item, affine=Affine(), blend=Blend(), matte=Gated(enabled=False, value=TrackMatte())))
            case Gated(value=Entry(effect=effect)):
                return seeded(effect).map(lambda value: structs.replace(item, value=Entry(effect=value)))
            case _:
                return Ok(Mask())

    def moved(items: tuple[object, ...], item: object) -> BlenderResult[tuple[tuple[object, ...], int]]:
        match action:
            case Edit.REMOVE:
                return Ok(((*items[:index], *items[index + 1 :]), max(index - 1, 0)))
            case Edit.DUPLICATE:
                return Ok(((*items[: index + 1], item, *items[index + 1 :]), index + 1))
            case Edit.UP | Edit.DOWN:
                other = min(max(index + (1 if action is Edit.UP else -1), 0), len(items) - 1)
                return Ok((tuple(items[other] if at == index else item if at == other else held for at, held in enumerate(items)), other))
            case Edit.RESET:
                return fresh(item).map(lambda replaced: ((*items[:index], replaced, *items[index + 1 :]), index))

    edit = _reached(record, path).bind(lambda items: _at(items, index, item_key(*path)).bind(lambda item: moved(items, item)))
    return edit.map(lambda held: (updated(record, path, lambda _: held[0]), held[1] if isinstance(target, LayerAt) else target.layer))


def strengthened[E: Effect](record: LayerStack[Source, E], amount: float, config: PyOpenColorIO.Config) -> BlenderResult[LayerStack[Source, E]]:
    """Moves every layer effect from its record defaults toward its stored values by `amount`, 1 keeping the stack."""

    def entry(gated: Gated[Entry[E]]) -> BlenderResult[Gated[Entry[E]]]:
        effect = gated.value.effect
        return Preset.blended(type(effect)(), effect, amount, config).map(lambda moved: structs.replace(gated, value=structs.replace(gated.value, effect=moved)))

    def layer(held: Layer[Source, E]) -> BlenderResult[Layer[Source, E]]:
        return accumulated(map(entry, held.effects.value)).map(lambda entries: structs.replace(held, effects=structs.replace(held.effects, value=entries)))

    return accumulated(map(layer, record.layers)).map(lambda layers: structs.replace(record, layers=layers))


# --- [CONDITIONS]


def layer_conditions(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[dict[Key, Condition]]:
    """Restricts denoise targets to terms each layer source offers."""
    tag = Case.__struct_config__.tag_field

    def accepted(source: Source) -> tuple[str, ...]:
        match source:
            case Recombined(recombination=Lobes()):
                return Frame.title, Passes.title
            case Recombined():
                return Frame.title, Groups.title
            case Adjustment() | RenderPass() | NodeSource():
                return (Frame.title,)

    def setting(key: Key, titles: tuple[str, ...]) -> BlenderResult[tuple[Key, Condition]]:
        return entry_input(identity, entry, key).map(
            lambda item: (key, Setting(descriptor=Descriptor(root=bpy.types.Scene, owner=item.path_from_id(), prop=item.bl_rna.properties["value"]), accepted=titles))
        )

    def found(tree: bpy.types.NodeTree, name: str) -> BlenderResult[dict[Key, Condition]]:
        stored = tuple(getattr(tree, name).layers)
        return (
            accumulated(RecordGroup(record=Sourced, struct=type(layer)).read(layer) for layer in stored)
            .bind(
                lambda layers: accumulated(
                    setting(item_key(layer.key, "effects", index, *Filters.of().target), accepted(layer.source))
                    for layer, native in zip(layers, stored)
                    for index, gated in enumerate(native.effects.value, 1)
                    if getattr(gated.value.effect, tag) == SpatialDenoise.__name__  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                )
            )
            .map(dict)
        )

    return paired(_found(stacked_by(identity, entry).to_optional(), repr(entry), LayerStacks.ownership.key), identity.name(Name.ATTACHMENT, LayerStacks.ownership.key)).bind(lambda pair: found(*pair))


# --- [GRAPHS]


def sourced(key: Key, source: RenderPass | NodeSource | Recombined, scene: bpy.types.Scene) -> BlenderResult[Expr]:
    """Builds the layer's source node at default settings under its stored key."""
    match source:
        case RenderPass(layer=name):
            return Ok(Expr(kind=bpy.types.CompositorNodeRLayers, members=(("scene", IdName.of(scene)), ("layer", name)), label=Some(f"{key}_source")))
        case Recombined(layer=name):
            return Ok(Expr(kind=bpy.types.CompositorNodeRLayers, members=(("scene", IdName.of(scene)), ("layer", name)), label=Some(f"{key}_render")))
        case NodeSource(node=identifier):
            return node_kind(identifier).map(lambda kind: Expr(kind=kind, label=Some(f"{key}_source")))


def unit[E: Effect](layer: Layer[Source, E], below: End, base: End, planning: Planning[E], *, mixes: bool) -> BlenderResult[Built]:
    """Builds layer source, gated effects, affine sampling, masks, and coverage, a recombined layer's denoised beauty chosen after its denoise effect."""
    key = layer.key
    frame = {name: group_input(name) for field in structs.fields(CameraFrame) for name in (item_key(Shell.frame, field.name),)}
    aspect, scale = frame[item_key(Shell.frame, "aspect")], frame[item_key(Shell.frame, "scale")]
    denoised = Option.of_optional(next((index for index, gated in enumerate(layer.effects.value, 1) if isinstance(gated.value.effect, SpatialDenoise)), None))

    def outer(*parts: Key | int) -> Expr:
        return group_input(item_key(key, *parts))

    def entering() -> BlenderResult[Beauty]:
        match layer.source:
            case Adjustment():
                return Ok(Beauty(plain=Expr(kind=bpy.types.NodeReroute, inputs=(("Input", base),), socket="Output", shape="RGBA")))
            case RenderPass(output=name) | NodeSource(output=name) as source:
                found = _found(planning.offered[f"{key}_source"].get(name), f"{key}_source", name)
                return paired(found, sourced(key, source, planning.scene)).map(lambda parts: Beauty(plain=structs.replace(parts[1], socket=parts[0], shape="RGBA")))
            case Recombined(layer=view) as source:
                denoising = denoised.map(lambda index: Denoising(group=planning.denoiser, entry=item_key(key, "effects", index), switch=item_key(key, "effects", index, *Filters.of().target)))
                routed = planning.routed

                def filing(request: Filing, at: Key) -> BlenderResult[Expr]:
                    return (
                        _found(planning.scene.view_layers.get(view), repr(planning.scene.view_layers), view)
                        .bind(routed.routing.read)
                        .bind(lambda routing: filed(routed.pathed, planning.path, view, request, at, routing.convention, routed.dimensions))
                    )

                return sourced(key, source, planning.scene).bind(
                    lambda render: source.recombined(key, lambda inner: outer("source", inner), render, planning.scene, planning.offered[f"{key}_render"], routed.lobe, denoising, filing)
                )

    def grouped(index: int, gated: Gated[Entry[E]]) -> BlenderResult[Grouped[E]]:
        return planning.stack.offering(type(gated.value.effect)).bind(
            lambda row: row.grouped(planning.identity, planning.scene, gated.value, planning.config).map(lambda group: (index, gated, row, group))
        )

    def chained(beauty: Beauty, entries: tuple[Grouped[E], ...]) -> BlenderResult[Expr]:
        def step(done: BlenderResult[Expr], held: Grouped[E]) -> BlenderResult[Expr]:
            index, _, _, group = held
            prefix = item_key(key, "effects", index)
            placed = {node: value for node, (value, _) in group.graph.placed.items() if node not in {INPUTS, OUTPUTS}}
            feeds = {node: tuple(link for link in group.graph.links if link.target.node == node) for node in (*placed, OUTPUTS)}
            order = tuple(graphlib.TopologicalSorter({node: {link.source.node for link in feeds[node] if link.source.node != INPUTS} for node in placed}).static_order())

            def inlined(source: Expr) -> BlenderResult[Expr]:
                def read(exprs: Mapping[Key, Expr], end: End) -> Expr:
                    match end:
                        case End(node=node, socket=socket) if node != INPUTS:
                            return structs.replace(exprs[node], socket=socket)
                        case End(socket=Shell.image):
                            return source
                        case End(socket=socket) if socket in frame:
                            return frame[socket]
                        case End(socket=socket):
                            return group_input(item_key(prefix, socket))

                def expressed(exprs: Mapping[Key, Expr], node: Key) -> Mapping[Key, Expr]:
                    value = placed[node]
                    inputs = (*value.inputs.items(), *((link.target.socket, read(exprs, link.source)) for link in feeds[node]))
                    members = tuple((member.path, held) for member, held in value.members.items())
                    return {**exprs, node: Expr(kind=value.kind, inputs=inputs, members=members, content=value.content, label=Some(f"{prefix}_{node}"))}

                seed: Mapping[Key, Expr] = {}
                exprs = functools.reduce(expressed, order, seed)
                result = next((link.source for link in feeds[OUTPUTS] if link.target.socket == Shell.result), None)
                return _found(result, group.name, Shell.result).map(lambda end: read(exprs, end).keeping(*exprs.values()))

            def chosen(result: Expr) -> Expr:
                return next((switched(selected, result, cleaned) for cleaned, selected in beauty.denoised.to_list() if denoised == Some(index)), result)

            return done.bind(lambda source: inlined(source).map(lambda result: switched(group_input(prefix), source, chosen(result))))

        ordered = sorted(entries, key=lambda held: (held[1].value.effect.stage, held[0]))
        return functools.reduce(step, ordered, Ok(beauty.plain)).map(lambda chain: switched(outer("effects"), beauty.plain, chain))

    def transformed(content: Expr) -> Expr:
        space = framed(content, aspect, scale)
        return placed(content, space, placement(space, *(outer("affine", field.name) for field in structs.fields(Affine))))

    def covered(original: Expr, content: Expr) -> BlenderResult[tuple[Expr, Option[Expr], tuple[tuple[FileItem, Expr], ...]]]:
        keyed = masks.coverage(len(layer.blend.masks), original, aspect, scale, lambda inner: outer("blend", "masks", inner))
        matted = (
            masks.matte(planning.luminance).map(lambda group: Some(group.placed(masks.relayed(lambda inner: below if inner == Shell.image else outer("matte", inner), group.interface), masks.KEY.key)))
            if layer.matte.enabled
            else Ok(Nothing)
        )

        def blended(factors: tuple[Expr, ...]) -> BlenderResult[tuple[Expr, Option[Expr], tuple[tuple[FileItem, Expr], ...]]]:
            mask = Option.of_optional(functools.reduce(lambda held, factor: held * factor, factors) if factors else None)
            weight = next((math("MULTIPLY", outer("blend", "opacity"), held) for held in mask.to_list()), outer("blend", "opacity"))
            coverage = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", content), ("Alpha", weight), ("Type", "Apply Mask")), socket="Image", shape="RGBA", label=Some(f"{key}_covered"))
            items = ((FileItem(name=key, kind="RGBA"), content), *((FileItem(name=f"{key} Mask", kind="FLOAT"), held) for held in mask.to_list()))
            return (mixed_back(key, base, content, outer("blend", "mode"), weight).map(Some) if mixes else Ok(Nothing)).map(lambda mixed: (coverage, mixed, items))

        return paired(keyed, matted).bind(lambda parts: blended((*parts[0].to_list(), *parts[1].to_list())))

    def faced(entries: tuple[Grouped[E], ...]) -> BlenderResult[InterfacePanel]:
        def panel(name: Key, face: Interface, toggle: Option[bool] = Nothing) -> InterfacePanel:
            inner = under(item_key(key, name), face)
            return InterfacePanel(key=item_key(key, name), name=heading(name), toggle=toggle, inputs=inner.inputs, panels=inner.panels)

        def entry_panel(index: int, gated: Gated[Entry[E]], row: CompositorEffect[Effect], group: NodeGroup) -> InterfacePanel:
            prefix = item_key(key, "effects", index)
            inner = under(prefix, Interface(inputs=tuple(socket for socket in group.interface.inputs if socket.key not in {Shell.image, *frame}), panels=group.interface.panels))
            return InterfacePanel(key=prefix, name=row.record.title, toggle=Some(gated.enabled), inputs=inner.inputs, panels=inner.panels)

        def assembled(source: Interface, affine: Interface, blend: Interface, matte: Interface) -> InterfacePanel:
            effects = structs.replace(panel("effects", Interface(), Some(layer.effects.enabled)), panels=tuple(itertools.starmap(entry_panel, entries)))
            return InterfacePanel(key=key, name=layer.name, panels=(panel("source", source), effects, panel("affine", affine), panel("blend", blend), panel("matte", matte)))

        source = Interface.of(type(layer.source)).bind(lambda face: expanded(face, layer.source))
        return paired(source, Interface.of(Affine), Interface.of(Blend).bind(lambda face: expanded(face, layer.blend)), Interface.of(TrackMatte)).map(lambda faces: assembled(*faces))

    def built(beauty: Beauty, entries: tuple[Grouped[E], ...]) -> BlenderResult[Built]:
        def assembled(coverage: Expr, mixed: Option[Expr], items: tuple[tuple[FileItem, Expr], ...], panel: InterfacePanel) -> BlenderResult[Built]:
            roots = (coverage, *mixed.to_list(), *beauty.filing.to_list())
            return lowered(*roots, *(expr for _, expr in items)).map(lambda body: Built(roots=roots, mixed=mixed, nodes=frozenset(body.nodes).difference((INPUTS,)), panel=panel, items=items))

        return paired(chained(beauty, entries).map(transformed).bind(lambda content: covered(beauty.plain, content)), faced(entries)).bind(lambda parts: assembled(*parts[0], parts[1]))

    return paired(entering(), accumulated(itertools.starmap(grouped, enumerate(layer.effects.value, 1)))).bind(lambda parts: built(*parts))


def planned[E: Effect](record: LayerStack[Source, E], planning: Planning[E]) -> BlenderResult[NodeGroup]:
    """Builds every layer and its matte even when its composite is hidden, each layer's nodes in its own frame."""
    shown = frozenset(layer.key for layer, _ in record.composited())
    canvas = End(node="canvas", socket="Image")
    covered = (canvas, *(End(node=f"{layer.key}_covered", socket="Image") for layer in record.layers))
    running = tuple(itertools.accumulate(record.layers, lambda prior, layer: End(node=f"{layer.key}_mixback", socket="image") if layer.key in shown else prior, initial=canvas))
    ramp = Expr(kind=bpy.types.ShaderNodeTexGradient, members=(("gradient_type", "LINEAR"),), socket="Color", shape="RGBA")
    background = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", ramp), ("Alpha", 0.0), ("Type", "Apply Mask")), socket="Image", shape="RGBA", label=Some(canvas.node))

    def built(units: tuple[Built, ...], face: Interface) -> BlenderResult[NodeGroup]:
        items: tuple[tuple[FileItem, Term], ...] = (
            (FileItem(name="Composite", kind="RGBA"), running[-1]),
            *(item for held in units for item in held.items),
            *((FileItem(name=f"{layer.key} Composite", kind="RGBA"), end) for layer, end in zip(record.layers, running[1:])),
        )
        members = (
            ("directory", bpy.app.tempdir),
            ("file_name", f"layer_stack_{planning.uid}_{'#' * LayerStacks.digits}"),
            ("format.media_type", "MULTI_LAYER_IMAGE"),
            ("format.file_format", "OPEN_EXR_MULTILAYER"),
            ("format.color_depth", "32"),
        )
        files = Expr(kind=bpy.types.CompositorNodeOutputFile, inputs=tuple((item.name, term) for item, term in items), members=members, content=Some(FileItems(items=tuple(item for item, _ in items))))
        owner = {node: layer.key for layer, held in reversed(tuple(zip(record.layers, units))) for node in held.nodes}
        result = next((mixed for held in reversed(units) for mixed in held.mixed.to_list()), background)
        interface = structs.replace(
            face,
            inputs=(Socket(key=Shell.image, name="Image", kind=bpy.types.NodeSocketColor), *face.inputs),
            outputs=(Socket(key=Shell.result, name="Image", kind=bpy.types.NodeSocketColor),),
            panels=tuple(held.panel for held in units),
        )

        def enframed(group: NodeGroup) -> NodeGroup:
            joined = group.graph.nodes
            frames = {
                f"{layer.key}_frame": FrameNode(members={PassRouting.label: layer.name}, nodes={node: value for node, value in joined.items() if owner.get(node) == layer.key})
                for layer in record.layers
            }
            nodes = {**{node: value for node, value in joined.items() if node not in owner}, **frames}
            return structs.replace(group, graph=structs.replace(group.graph, members={_FAKE_USER: True}, nodes=nodes))

        kept = result.keeping(background, files, *(root for held in units for root in held.roots))
        return NodeGroup.built(key=LayerStacks.ownership.key, name=LayerStacks.title, interface=interface, outputs={Shell.result: kept}).map(enframed)

    units = accumulated(unit(layer, below, base, planning, mixes=layer.key in shown) for layer, below, base in zip(record.layers, covered, running))
    return paired(units, Interface.of(CameraFrame).map(lambda face: hidden(under(Shell.frame, face)))).bind(lambda parts: built(*parts))


# --- [COMPOSITION] ----------------------------------------------------------------------


class LayerStacks[E: Effect]:
    """Stack ownership, commands, and editor row an application places, bound per extension on each use."""

    stack: EffectStack[E]
    form: type[LayerStack[Source, E]]
    attachment: Attachment[LayerStack[Source, E]]
    preset: Preset[LayerStack[Source, E]]
    rows: Mapping[type[Effect], CompositorEffect[E]]
    title: ClassVar[str] = "Layer Stack"
    digits: ClassVar[int] = 4
    ownership: ClassVar[Ownership[bpy.types.CompositorNodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key="layer_stack", saved=Saved.ALWAYS, single=False)
    view: ClassVar[Attachment[StackView]] = Attachment(key="layer_stack_view", form=StackView, owner=bpy.types.Screen)
    pinned: ClassVar[Pinned] = Pinned(ownership=ownership, title=title, edge=Edge.HEAD, conditions=Some(layer_conditions))

    def __init__(self, *, stack: EffectStack[E], looks: Mapping[str, LayerStack[Source, E]] = MappingProxyType({})) -> None:
        """Declares the stack form, attachment, and preset catalog over stock looks, and every command whose body resolves the calling extension's binding."""
        self.stack = stack
        self.form = LayerStack[Source, stack.offered]  # type: ignore[name-defined]  # ty: ignore[invalid-type-form]
        self.attachment = Attachment(key=self.ownership.key, form=self.form, owner=bpy.types.NodeTree, changed=Some(self.changed))
        self.preset = Preset(key=self.ownership.key, form=self.form, stock=looks)
        self.rows = MappingProxyType({row.record: row for row in stack.effects})

        def bound[T: msgspec.Struct](body: Callable[[LayerStacking[E], Call[T]], BlenderResult[Outcome]]) -> Callable[[Call[T]], BlenderResult[Outcome]]:
            return lambda call: self.bound(call.identity).bind(lambda stacking: body(stacking, call))

        def edit(action: Edit) -> Operator[Addressed]:
            title = action.name.title()
            return Operator(key=f"layer_edit_{action.name.lower()}", title=title, description=f"{title} layer, effect, or mask", form=Addressed, run=bound(lambda held, call: held.edit(action, call)))

        def effect(row: CompositorEffect[E]) -> Operator[Empty]:
            title = row.record.title
            return Operator(key=f"layer_effect_add_{row.kind}", title=title, description=f"Add {title} to layer", form=Empty, run=bound(lambda held, call: held.effected(row, call)))

        (node_field,) = structs.fields(NodeKind)
        extensions = (*sorted(bpy.path.extensions_image), *sorted(bpy.path.extensions_movie))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        self.creating = Operator(key="layer_stack_new", title="New Stack", description="Create layer stack", form=Empty, run=bound(LayerStacking.created))
        self.duplicating = Operator(key="layer_stack_duplicate", title="Duplicate Stack", description="Copy stack with source settings", form=Empty, run=bound(LayerStacking.duplicated))
        self.removing = Operator(key="layer_stack_remove", title="Remove Stack", description="Remove stack and its effect entry", form=Empty, run=bound(LayerStacking.removed))
        self.switching = Operator(key="layer_stack_switch", title="Switch Stack", description="Edit another stack", form=Switched, run=bound(LayerStacking.switching), invoke=Some(InvokePopup.SEARCH))
        self.nesting = Operator(key="layer_stack_nested", title="Edit Nested Stack", description="Edit selected layer's stack", form=Empty, run=bound(LayerStacking.nested))
        self.clearing = Operator(key="layer_stack_clear", title="Clear Layers", description="Remove every layer", form=Empty, run=bound(LayerStacking.cleared))
        self.copying = (
            Operator(key="layer_copy", title="Copy Layer", description="Copy one layer of another stack", form=Copied, run=bound(LayerStacking.copied), invoke=Some(Properties())),
            Operator(
                key="layer_copy_all",
                title="Copy All Layers",
                description="Copy layers of another stack with source settings",
                form=Switched,
                run=bound(LayerStacking.copied),
                invoke=Some(InvokePopup.SEARCH),
            ),
        )
        self.sourcing: tuple[Command, ...] = (
            Operator(key="layer_add_adjustment", title=Adjustment.title, description="Add layer processing composite beneath", form=Adjustment, run=bound(LayerStacking.added)),
            Operator(key="layer_add_render_pass", title=RenderPass.title, description="Add view layer output", form=RenderPass, run=bound(LayerStacking.added), invoke=Some(Properties())),
            Operator(key="layer_add_recombined", title=Recombined.title, description="Add beauty from view layer light", form=Recombined, run=bound(LayerStacking.added), invoke=Some(Properties())),
            Operator(
                key="layer_add_node",
                title=NodeSource.title,
                description="Add source node at first enabled output",
                form=NodeKind,
                run=bound(LayerStacking.noded),
                invoke=Some(InvokePopup.SEARCH),
                primary=Some(node_field),
            ),
            Operator(key="layer_add_nested", title="Nested Stack", description="Add stack as source", form=Switched, run=bound(LayerStacking.nesting), invoke=Some(InvokePopup.SEARCH)),
            Operator(key="layer_add_media", title="Media", description="Add image or movie files", form=Empty, run=bound(LayerStacking.media), invoke=Some(Open(extensions=lambda _: extensions))),
        )
        self.output = Operator(key="layer_output", title="Select Output", description="Set layer source output", form=OutputChoice, run=bound(LayerStacking.output))
        self.masking = Operator(key="layer_mask_add", title="Add Mask", description="Add mask to layer or effect blend", form=Addressed, run=bound(LayerStacking.masked))
        self.editing = MappingProxyType({action: edit(action) for action in Edit})
        self.effects = tuple((row, effect(row)) for row in stack.effects if row.record.stage < Stage.FORMATION)
        self.adding = Menu(key="layer_add", title="Add Layer", description="Add layer source to stack", body=Dropdown(entries=tuple(OperatorButton(target=command) for command in self.sourcing)))
        staged = sorted(self.effects, key=lambda pair: pair[0].record.stage)
        self.effecting = Menu(
            key="layer_effect_add",
            title="Add Effect",
            description="Add effect to selected layer by imaging phase",
            body=Submenus(entries=tuple((row.record.stage.phase, OperatorButton(target=command)) for row, command in staged)),
        )

    @property
    def commands(self) -> tuple[Command, ...]:
        """Commands every binding shares."""
        held: tuple[Command, ...] = (self.creating, self.duplicating, self.removing, self.switching, self.nesting, self.clearing, *self.copying, *self.sourcing, self.output, self.masking)
        return (*held, *self.editing.values(), *(command for _, command in self.effects))

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Stack and view attachments with routing and frame size bindings read."""
        return self.attachment, self.view, PassSet.attachment, FrameSize.attachment

    @contextmanager
    def changed(self, identity: Identity) -> Generator[Changed]:
        """Defers stack refresh for attachment lifetime."""
        with identity.reports.deferred(lambda: self.bound(identity).bind(LayerStacking.refreshed)) as refresh:
            yield lambda _owner, _context, _property: refresh()

    def bound(self, identity: Identity) -> BlenderResult[LayerStacking[E]]:
        """Binds stores, pass descriptors, routing, and framing of an extension."""
        return paired(self.attachment.bound(identity), self.view.bound(identity), RecombinationFlags.of(), PassSet().bound(identity), framing(identity)).map(
            lambda parts: LayerStacking(identity=identity, stacks=self, attached=parts[0], view=parts[1], flags=parts[2], routed=parts[3], framing=parts[4])
        )

    def document(self, identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[np.uint8 | np.uint16 | np.float32]]:
        """Returns layered document of scene stack at frame."""
        return self.bound(identity).bind(lambda stacking: stacking.layered(scene, frame, depth))

    def section(self, header: tuple[RowContent, ...] = ()) -> Single:
        """Returns editor section under application header rows."""
        return Single(key=self.ownership.key, title=self.title, header=(Popover(panel=self.presets().key, text="", icon=Some("PRESET")), *header), rows=(self,), opened=True)

    def presets(self) -> Single:
        """Returns section of stack, layer, and effect preset listings."""
        description = f"Apply, add, or import {self.title.lower()}, layer, and effect presets"
        return Single(key=f"{self.ownership.key}_presets", title=f"{self.title} Presets", description=description, rows=((self, self.presets_drawn),))

    def presets_drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns preset listing draws of each held stack, layer, or effect, the New Stack button while no stack exists."""

        def gated[T: msgspec.Struct](listing: Listing[T], draw: RowDraw) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                if listing.current(context).is_ok():
                    draw(layout, holder, context)

            return shown

        def draw(stacking: LayerStacking[E], draws: tuple[RowDraw, ...]) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                if rooted(bpy.types.Scene, context).bind(stacking.head).is_error():
                    alerted(layout, pressed(layout, identity, self.creating, Empty(), text=self.creating.title, icon="NONE"))
                for each in draws:
                    each(layout, holder, context)

            return shown

        def listed(stacking: LayerStacking[E]) -> BlenderResult[RowDraw]:
            stack, layer, entry = stacking.listings
            draws = (
                stack.drawn(identity).map(lambda shown: gated(stack, shown)),
                layer.drawn(identity).map(lambda shown: gated(layer, shown)),
                entry.drawn(identity).map(lambda shown: gated(entry, shown)),
            )
            return accumulated(draws).map(lambda held: draw(stacking, held))

        return self.bound(identity).bind(listed)

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns editor draw of the extension's binding."""
        return self.bound(identity).bind(LayerStacking.drawn)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Yields commands, listings, handlers, and subscriptions with deferred refresh."""
        watched = ((bpy.types.RenderSettings, "film_transparent"), (bpy.types.ViewLayer, "name"), (bpy.types.ViewLayer, "use"), (bpy.types.Object, "lightgroup"), (bpy.types.World, "lightgroup"))
        with identity.reports.deferred(lambda: self.bound(identity).bind(LayerStacking.refreshed)) as refresh:

            def updated() -> BlenderResult[None]:
                refresh()
                return Ok(None)

            def rows(stacking: LayerStacking[E]) -> Acquisitions:
                subscriptions = (*stacking.framing.reads, *((owner, owner.bl_rna.properties[name]) for owner, name in watched))
                handlers: tuple[Registrable, ...] = (
                    PathHandler(handlers=LOAD_POST, body=lambda _: updated()),
                    *(Subscription(owner=owner, setting=prop, body=updated) for owner, prop in subscriptions),
                )
                return (self.ownership, stacking.listing, self.adding, self.effecting, stacking.specials, *self.commands, *stacking.listings, *handlers)

            yield self.bound(identity).map(rows)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Addressed",
    "Copied",
    "Edit",
    "EffectAt",
    "EffectMaskAt",
    "LayerAt",
    "LayerMaskAt",
    "LayerStacking",
    "LayerStacks",
    "NodeKind",
    "NodeSource",
    "OutputChoice",
    "RenderPass",
    "Selection",
    "Source",
    "StackView",
    "Switched",
    "Target",
    "edited",
    "layer_conditions",
    "nested_tree",
    "node_kind",
    "planned",
    "sourced",
    "stacked_by",
    "strengthened",
    "unit",
]
