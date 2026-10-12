"""Owned native layer stacks, source graphs, edits, and layered documents."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import datetime
from enum import auto, Enum
import functools
import itertools
from pathlib import Path
from types import GenericAlias
from typing import Annotated, ClassVar, get_args

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
from msgspec import structs
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm.blender.commands.modal import Gesture, Modal, Travel
from rasm.blender.commands.operators import Call, Command, Done, Empty, InvokePopup, Open, Operator, Outcome, Own, Placed, Properties, Unchanged
from rasm.blender.data.ids.images import loaded, Tag
from rasm.blender.data.ids.ownership import Mark, Ownership, Saved
from rasm.blender.data.rna.conditions import Condition, Setting
from rasm.blender.data.rna.descriptors import Descriptor, IdName, root_in, written
from rasm.blender.data.rna.snapshots import Snapshot
from rasm.blender.extension.events.handlers import PathHandler
from rasm.blender.extension.events.subscriptions import Subscription
from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderImagingFault, BlenderResult
from rasm.blender.nodes.changes import Changes, owned, reconciled
from rasm.blender.nodes.coordinates import framed
from rasm.blender.nodes.graphs import End, FileItem, FileItems, Fragment, FrameNode, Graph, INPUTS, Link, nested, node, NodeGroup, OUTPUTS, switched
from rasm.blender.nodes.interface import addresses, Interface, InterfacePanel, item_key, slot, Socket, under
from rasm.blender.nodes.trees import created
from rasm.blender.render.effects.compositing import masks
from rasm.blender.render.effects.compositing.blending import mixed_back, product
from rasm.blender.render.effects.compositing.recombination import Denoising, RecombinationFlags, Recombined, views
from rasm.blender.render.effects.detail.denoise import denoiser, Filters, guided
from rasm.blender.render.effects.stack import (
    CameraFrame,
    Derived,
    Edge,
    EffectStack,
    entered,
    entry_input,
    expanded,
    formation,
    framing,
    graph_values,
    inputs,
    KeyPath,
    Pinned,
    Placeable,
    relocated,
    Shell,
    viewed,
)
from rasm.blender.render.effects.warp.transforms import placed, placement
from rasm.blender.render.outputs.exports import image_type
from rasm.blender.render.outputs.paths import OutputPath
from rasm.blender.render.view_layers.passes import filed, Filing, PassRequest, PassRouting, PassSet, required, synced
from rasm.blender.scene.lights.groups import used
from rasm.blender.settings.attachments import Attached, Attachment
from rasm.blender.settings.presets import Preset
from rasm.blender.settings.properties import Changed, Choices
from rasm.blender.ui.layout.lists import ItemList
from rasm.blender.ui.layout.rows import alerted, Member, operated, Popover, Property, RowContent, RowDraw
from rasm.blender.ui.layout.sections import Listing, Single
from rasm.blender.ui.resources.theme import Metrics
from rasm.imaging.chain import Domain, Effect, Stage
from rasm.imaging.color.processors import DisplayView, unassociated
from rasm.imaging.compositing.blending import Blend, Entry
from rasm.imaging.compositing.layers import Adjustment, Affine, Layer, LayerStack
from rasm.imaging.compositing.masks import Mask, TrackMatte
from rasm.imaging.compositing.recombination import Lobes
from rasm.imaging.effects.detail.denoise import Frame, Groups, Passes, SpatialDenoise
from rasm.imaging.effects.quantization import codes
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.files import documents
from rasm.imaging.files.documents import Document, Group, Raster
from rasm.imaging.files.images import Depth
from rasm.imaging.files.multilayer import Layer as FileLayer, Multilayer
from rasm.imaging.parameters import Case, Gated

# --- [TYPES] ----------------------------------------------------------------------------

type Source = Adjustment | RenderPass | NodeSource | Recombined


class Edit(Enum):
    """Native stack, layer, effect, and mask editing intentions."""

    REMOVE = auto()
    DUPLICATE = auto()
    UP = auto()
    DOWN = auto()
    RESET = auto()


# --- [ROSTERS] --------------------------------------------------------------------------


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
    """One native output of a scene view layer."""

    title = "Render Pass"
    layer: Annotated[str, msgspec.Meta(title="View Layer"), views]
    output: Annotated[str, msgspec.Meta(title="Pass")] = "Image"


class NodeSource(Case, frozen=True, kw_only=True, tag=3):
    """Owned compositor source node whose native settings remain editable."""

    title = "Node"
    node: Annotated[str, msgspec.Meta(title="Node Type")]
    output: Annotated[str, msgspec.Meta(title="Output")]


class StackView(msgspec.Struct, frozen=True, kw_only=True):
    """Workspace selection in bottom-first layer order."""

    active: Annotated[int, msgspec.Meta(ge=0)] = 0


class Switched(msgspec.Struct, frozen=True, kw_only=True):
    """Owned stack selected by its native data-block name."""

    stack: Annotated[str, stack_names]


class Copied(msgspec.Struct, frozen=True, kw_only=True):
    """One explicitly selected layer of another native stack."""

    stack: Annotated[str, stack_names]
    layer: Annotated[str, layer_names]


class NodeKind(msgspec.Struct, frozen=True, kw_only=True):
    """Native source node to create before choosing its enabled output."""

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
    """Native operator's algebraic layer, effect, or mask location."""

    target: LayerAt | EffectAt | LayerMaskAt | EffectMaskAt


class OutputChoice(msgspec.Struct, frozen=True, kw_only=True):
    """Enabled native source output chosen for one layer."""

    layer: Annotated[int, msgspec.Meta(ge=0)]
    output: str


class Dragged[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Native entry and complete initial values of a layer drag."""

    entry: bpy.types.SceneCompositorEffect
    record: LayerStack[Source, E]
    index: int
    pitch: float


class Planning(msgspec.Struct, frozen=True, kw_only=True):
    """Actual source outputs and native dependencies of one graph construction."""

    identity: Identity
    scene: bpy.types.Scene
    config: PyOpenColorIO.Config
    luminance: NodeGroup
    denoiser: NodeGroup
    stack: EffectStack
    routed: PassRouting
    offered: dict[Key, dict[str, str]]
    path: OutputPath
    uid: int


class LayerStacking[E: Effect]:
    """Registered attachment, scene requirements, and editor of one stack row."""

    identity: Identity
    form: type[LayerStack[Source, E]]
    stack: EffectStack
    attached: Attached[LayerStack[Source, E]]
    view: Attached[StackView]
    flags: RecombinationFlags
    routed: PassRouting
    framing: Derived[Effect, CameraFrame]
    commands: tuple[Command, ...]

    def __init__(
        self,
        *,
        identity: Identity,
        form: type[LayerStack[Source, E]],
        stack: EffectStack,
        attached: Attached[LayerStack[Source, E]],
        view: Attached[StackView],
        flags: RecombinationFlags,
        routed: PassRouting,
        framing: Derived[Effect, CameraFrame],
    ) -> None:
        """Retains one native declaration set for this registered binding."""
        self.identity, self.form, self.stack = identity, form, stack
        self.attached, self.view, self.flags = attached, view, flags
        self.routed, self.framing = routed, framing
        self._commands()
        self._listings()
        self.listing = self._layer_list()

    # --- [GRAPH]

    def head(self, scene: bpy.types.Scene) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Returns the first owned native stack entry or its actual absence."""
        return Option.of_optional(next((entry for entry in scene.compositor_effects if stacked_by(self.identity, entry).is_some()), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=LayerStacks.ownership.key),)
        )

    def sources(self, scene: bpy.types.Scene, layers: LayerStack[Source, E], tree: Option[bpy.types.NodeTree] = Nothing) -> BlenderResult[dict[Key, dict[str, str]]]:
        """Reads enabled outputs on an isolated native source graph with copied settings."""
        with ExitStack() as release:
            scratch: bpy.types.CompositorNodeTree = bpy.data.node_groups.new("Layer Sources", "CompositorNodeTree")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            release.callback(bpy.data.node_groups.remove, scratch)

            def copied(_: Changes) -> BlenderResult[dict[Key, dict[str, str]]]:
                found = owned(scratch, self.identity)
                settings = accumulated(
                    Snapshot.capture(source).bind(lambda snapshot: snapshot.apply(found[key]).outcome)
                    for prior in tree.to_list()
                    for key, source in owned(prior, self.identity).items()
                    if key in found
                )
                return settings.map(lambda _: {key: {output.name: output.identifier for output in source.outputs if output.enabled} for key, source in found.items()})

            return (
                accumulated(sourced(layer, scene) for layer in layers.layers)
                .bind(Fragment.merged)
                .bind(lambda joined: reconciled(Graph(kind=bpy.types.CompositorNodeTree, nodes=dict(joined.nodes)), scratch, self.identity))
                .bind(copied)
            )

    def requests(self, layers: LayerStack[Source, E]) -> BlenderResult[tuple[PassRequest, ...]]:
        """Accumulates source, effect, and actual denoising guide requests."""

        def layer_requests(layer: Layer[Source, E]) -> BlenderResult[tuple[PassRequest, ...]]:
            source: tuple[PassRequest, ...]
            match layer.source:
                case RenderPass(layer=name):
                    source = (PassRequest(layer=name),)
                case Recombined() as source_case:
                    source = (source_case.requested(self.flags),)
                case Adjustment() | NodeSource():
                    source = ()
            guides = (
                guided(layer.source.layer).map(lambda request: (request,))
                if isinstance(layer.source, Recombined) and any(isinstance(entry.value.effect, SpatialDenoise) for entry in layer.effects.value)
                else Ok(())
            )
            return guides.map(lambda requests: (*source, *requests))

        return accumulated(layer_requests(layer) for layer in layers.layers).map(lambda groups: tuple(request for group in groups for request in group))

    def reconciled(self, scene: bpy.types.Scene, tree: bpy.types.NodeTree, context: bpy.types.Context, *, record: Option[LayerStack[Source, E]] = Nothing) -> BlenderResult[bool]:
        """Enables requests, constructs the complete graph, and writes its frame inputs, answering whether the graph or a frame input changed."""

        def built(layers: LayerStack[Source, E], config: PyOpenColorIO.Config, path: OutputPath) -> BlenderResult[bool]:
            names = tuple(sorted(used(scene)))
            layers = structs.replace(layers, layers=tuple(structs.replace(layer, source=layer.source.synced(names)) if isinstance(layer.source, Recombined) else layer for layer in layers.layers))
            groups = masks.luminance(config).bind(lambda working: denoiser(working).map(lambda filtered: (working, filtered)))
            return (
                synced(scene, self.identity)
                .bind(lambda _: self.attached.write(tree, layers))
                .bind(lambda _: self.requests(layers))
                .bind(lambda requests: required(scene, context, requests, self.routed.precision))
                .bind(lambda _: paired(self.sources(scene, layers, Some(tree)), groups))
                .bind(
                    lambda parts: planned(
                        layers,
                        Planning(
                            identity=self.identity,
                            scene=scene,
                            config=config,
                            luminance=parts[1][0],
                            denoiser=parts[1][1],
                            stack=self.stack,
                            routed=self.routed,
                            offered=parts[0],
                            path=path,
                            uid=tree.session_uid,
                        ),
                    )
                )
                .bind(lambda group: reconciled(group, tree, self.identity))
                .bind(
                    lambda changes: (
                        self.framing.of
                        .map(
                            lambda read: (
                                read(self.identity, scene, viewed(scene))
                                .bind(lambda frame: accumulated(entered(self.identity, entry, (Shell.frame,), frame) for entry in scene.compositor_effects if entry.node_group == tree))
                                .map(any)
                            )
                        )
                        .default_value(Ok(False))
                        .map(lambda framed: framed or changes != Changes())
                    )
                )
            )

        desired = record.map(Ok).default_with(
            lambda: self.attached.read(tree).bind(
                lambda record: Result[bpy.types.SceneCompositorEffect, BlenderFaults].bind(
                    Option.of_optional(next((entry for entry in scene.compositor_effects if entry.node_group == tree), None)).to_result_with(
                        lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=tree.name),)
                    ),
                    lambda entry: self.read(entry, baseline=Some(record)),
                )
            )
        )
        path = self.routed.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date())
        return paired(paired(desired, self.stack.config()), path).bind(lambda parts: built(*parts[0], parts[1]))

    def refreshed(self) -> BlenderResult[Option[str]]:
        """Reconciles each distinct scene stack while retaining all independent refusals, naming an undo step where a stack changed."""
        return accumulated(
            self.reconciled(scene, tree, bpy.context)
            for scene in bpy.data.scenes
            for tree in dict.fromkeys(tree for entry in scene.compositor_effects for tree in stacked_by(self.identity, entry).to_list())
        ).map(lambda changed: Some(f"Refresh {LayerStacks.title}s") if any(changed) else Nothing)

    # --- [VALUES]

    def read(self, entry: bpy.types.SceneCompositorEffect, *, baseline: Option[LayerStack[Source, E]] = Nothing) -> BlenderResult[LayerStack[Source, E]]:
        """Reads current inputs or overlays graph edits on supplied structural changes."""

        def toggle(path: KeyPath) -> BlenderResult[bool]:
            return entry_input(self.identity, entry, item_key(*path)).map(lambda owner: bool(owner.path_resolve("value")))

        def effect(key: Key, index: int, value: Gated[Entry[E]], graphs: dict[Key, object]) -> BlenderResult[Gated[Entry[E]]]:
            at = (key, "effects", index)
            return paired(
                Ok(value.enabled) if baseline.is_some() else toggle(at),
                paired(
                    inputs(self.identity, entry, at, type(value.value.effect), graphs, baseline=baseline.map(lambda _: value.value.effect)),
                    inputs(self.identity, entry, (*at, Shell.blend), Blend, graphs, baseline=baseline.map(lambda _: value.value.blend)),
                ),
            ).map(lambda pair: Gated(enabled=pair[0], value=Entry(effect=pair[1][0], blend=pair[1][1])))

        def layer(held: Layer[Source, E], graphs: dict[Key, object]) -> BlenderResult[Layer[Source, E]]:
            key = held.key
            records = paired(
                paired(
                    inputs(self.identity, entry, (key, "source"), type(held.source), graphs, baseline=baseline.map(lambda _: held.source)),
                    inputs(self.identity, entry, (key, "affine"), Affine, graphs, baseline=baseline.map(lambda _: held.affine)),
                ),
                paired(
                    inputs(self.identity, entry, (key, "blend"), Blend, graphs, baseline=baseline.map(lambda _: held.blend)),
                    inputs(self.identity, entry, (key, "matte"), TrackMatte, graphs, baseline=baseline.map(lambda _: held.matte.value)),
                ),
            )
            effects = paired(
                Ok(held.effects.enabled) if baseline.is_some() else toggle((key, "effects")), accumulated(effect(key, index, value, graphs) for index, value in enumerate(held.effects.value, 1))
            )
            return paired(records, effects).map(
                lambda values: structs.replace(
                    held,
                    source=values[0][0][0],
                    affine=values[0][0][1],
                    blend=values[0][1][0],
                    matte=Gated(enabled=held.matte.enabled, value=values[0][1][1]),
                    effects=Gated(enabled=values[1][0], value=values[1][1]),
                )
            )

        def read(record: LayerStack[Source, E]) -> BlenderResult[LayerStack[Source, E]]:
            records = tuple(
                item
                for held in record.layers
                for item in (
                    *(
                        ((held.key, name), type(value), Some(value), Domain.SCENE_LINEAR)
                        for name, value in (("source", held.source), ("affine", held.affine), ("blend", held.blend), ("matte", held.matte.value))
                    ),
                    *(
                        item
                        for index, effect in enumerate(held.effects.value, 1)
                        for item in (
                            ((held.key, "effects", index), type(effect.value.effect), Some(effect.value.effect), effect.value.effect.stage.domain),
                            ((held.key, "effects", index, Shell.blend), Blend, Some(effect.value.blend), effect.value.effect.stage.domain),
                        )
                    ),
                )
            )
            return graph_values(self.identity, entry, records).bind(
                lambda graphs: accumulated(layer(held, graphs) for held in record.layers).map(lambda layers: structs.replace(record, layers=layers))
            )

        tree: BlenderResult[bpy.types.NodeTree] = stacked_by(self.identity, entry).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=LayerStacks.ownership.key),))
        return baseline.map(Ok).default_with(lambda: Result[bpy.types.NodeTree, BlenderFaults].bind(tree, self.attached.read)).bind(read)

    def written(self, entry: bpy.types.SceneCompositorEffect, record: LayerStack[Source, E], context: bpy.types.Context) -> BlenderResult[None]:
        """Rebuilds structure before writing every desired value into its native input."""

        def toggle(at: KeyPath, value: bool) -> BlenderResult[None]:
            return entry_input(self.identity, entry, item_key(*at)).bind(lambda owner: written(owner, "value", value)).map(lambda _: None)

        def layer(held: Layer[Source, E]) -> BlenderResult[None]:
            fields = (("source", held.source), ("affine", held.affine), ("blend", held.blend), ("matte", held.matte.value))
            return accumulated((
                *(entered(self.identity, entry, (held.key, name), value) for name, value in fields),
                toggle((held.key, "effects"), held.effects.enabled),
                *(
                    answer
                    for index, effect in enumerate(held.effects.value, 1)
                    for at in ((held.key, "effects", index),)
                    for answer in (toggle(at, effect.enabled), entered(self.identity, entry, at, effect.value.effect), entered(self.identity, entry, (*at, Shell.blend), effect.value.blend))
                ),
            )).map(lambda _: None)

        tree: BlenderResult[bpy.types.NodeTree] = stacked_by(self.identity, entry).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=LayerStacks.ownership.key),))
        return (
            Result[bpy.types.Scene, BlenderFaults]
            .bind(
                Option.of_optional(context.scene).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="scene"),)),
                lambda scene: Result[bpy.types.NodeTree, BlenderFaults].bind(tree, lambda tree: self.reconciled(scene, tree, context, record=Some(record))),
            )
            .bind(lambda _: accumulated(layer(held) for held in record.layers))
            .map(lambda _: None)
        )

    def selected(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.SceneCompositorEffect, LayerStack[Source, E], StackView, bpy.types.NodeTree]]:
        """Returns the editor entry, current values, workspace selection, and owned tree."""
        screen: BlenderResult[bpy.types.Screen] = Option.of_optional(context.screen).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="screen"),))
        scene: BlenderResult[bpy.types.Scene] = Option.of_optional(context.scene).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="scene"),))
        return (
            Result[bpy.types.Scene, BlenderFaults]
            .bind(scene, self.head)
            .bind(
                lambda entry: paired(
                    paired(self.read(entry), Result[bpy.types.Screen, BlenderFaults].bind(screen, self.view.read)),
                    stacked_by(self.identity, entry).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=LayerStacks.ownership.key),)),
                ).map(lambda pair: (entry, *pair[0], pair[1]))
            )
        )

    def written_selection(self, context: bpy.types.Context, entry: bpy.types.SceneCompositorEffect, record: LayerStack[Source, E], active: int) -> BlenderResult[None]:
        """Writes an edited stack and its construction-selected active index."""
        screen: BlenderResult[bpy.types.Screen] = Option.of_optional(context.screen).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="screen"),))
        return paired(self.written(entry, record, context), Result[bpy.types.Screen, BlenderFaults].bind(screen, lambda screen: self.view.write(screen, StackView(active=active)))).map(lambda _: None)

    def replaced(self, context: bpy.types.Context, entry: bpy.types.SceneCompositorEffect, current: LayerStack[Source, E], desired: LayerStack[Source, E], active: int) -> BlenderResult[bool]:
        """Writes a desired stack differing from the current one with its selected index, answering whether the layers differed."""
        return Ok(False) if desired == current else self.written_selection(context, entry, desired, active).map(lambda _: True)

    # --- [STACKS]

    def switched(self, context: bpy.types.Context, tree: bpy.types.NodeTree) -> BlenderResult[None]:
        """Places or replaces the pinned native stack entry at the head."""

        def switched(scene: bpy.types.Scene) -> BlenderResult[None]:
            held = Option.of_optional(next((entry for entry in scene.compositor_effects if stacked_by(self.identity, entry).is_some()), None))
            with ExitStack() as rollback:
                entry = held.default_with(lambda: scene.compositor_effects.new(tree.name))
                if held.is_none():
                    rollback.callback(scene.compositor_effects.remove, entry)
                return (
                    written(entry, "node_group", tree)
                    .bind(lambda _: relocated(scene, entry, 0))
                    .bind(lambda _: self.reconciled(scene, tree, context))
                    .map(lambda _: rollback.pop_all())
                    .map(lambda _: None)
                )

        return Result[bpy.types.Scene, BlenderFaults].bind(Option.of_optional(context.scene).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="scene"),)), switched)

    def inserted(self, context: bpy.types.Context, layers: tuple[Layer[Source, E], ...], source_tree: Option[bpy.types.NodeTree] = Nothing) -> BlenderResult[bool]:
        """Copies complete source-node settings while giving inserted layers fresh keys, answering whether any layer was given."""

        def insert(entry: bpy.types.SceneCompositorEffect, record: LayerStack[Source, E], view: StackView, target_tree: bpy.types.NodeTree) -> BlenderResult[bool]:
            held = frozenset(layer.key for layer in record.layers)
            keys = (f"layer_{index}" for index in itertools.count(1) if f"layer_{index}" not in held)
            added = tuple(structs.replace(layer, key=next(keys)) for layer in layers)
            at = min(view.active + 1, len(record.layers))
            desired = structs.replace(record, layers=(*record.layers[:at], *added, *record.layers[at:]))
            with ExitStack() as rollback:

                def copied(source: bpy.types.Node, after: Layer[Source, E]) -> BlenderResult[None]:
                    def filled(snapshot: Snapshot, target: bpy.types.Node) -> BlenderResult[None]:
                        rollback.callback(target_tree.nodes.remove, target)
                        return snapshot.apply(target).outcome.bind(lambda _: Mark(key=f"{after.key}_source").write(self.identity, target))

                    return Snapshot.capture(source).bind(lambda snapshot: created(target_tree, type(source)).bind(lambda target: filled(snapshot, target)))

                inserting = accumulated(
                    copied(source, after)
                    for tree in source_tree.to_list()
                    for before, after in zip(layers, added)
                    for source in Option.of_optional(owned(tree, self.identity).get(f"{before.key}_source")).to_list()
                ).bind(lambda _: self.replaced(context, entry, record, desired, at + len(added) - 1 if added else view.active))
                for _ in inserting.to_option().to_list():
                    rollback.pop_all()
                return inserting

        return self.selected(context).bind(lambda selected: insert(*selected))

    def add_node[T](self, context: bpy.types.Context, kind: type[bpy.types.Node], setup: Callable[[bpy.types.Node], BlenderResult[T]]) -> BlenderResult[None]:
        """Creates the actual source once and selects its first enabled native output."""
        identifier = kind.__name__

        def added(entry: bpy.types.SceneCompositorEffect, record: LayerStack[Source, E], view: StackView, tree: bpy.types.NodeTree) -> BlenderResult[None]:
            key = next(f"layer_{index}" for index in itertools.count(1) if all(layer.key != f"layer_{index}" for layer in record.layers))
            with ExitStack() as rollback:

                def placed(source: bpy.types.Node) -> BlenderResult[None]:
                    return Result[str, BlenderFaults].bind(
                        Option.of_optional(next((output.name for output in source.outputs if output.enabled), None)).to_result_with(lambda: (BlenderAbsentFault(owner=identifier, key="outputs"),)),
                        lambda output: self.written_selection(
                            context,
                            entry,
                            structs.replace(
                                record,
                                layers=(*record.layers[: view.active + 1], Layer(key=key, name=source.bl_label, source=NodeSource(node=identifier, output=output)), *record.layers[view.active + 1 :]),
                            ),
                            min(view.active + 1, len(record.layers)),
                        ),
                    )

                def claimed(source: bpy.types.Node) -> BlenderResult[None]:
                    rollback.callback(tree.nodes.remove, source)
                    return Mark(key=f"{key}_source").write(self.identity, source).bind(lambda _: setup(source)).bind(lambda _: placed(source))

                answer = created(tree, kind).bind(claimed)
                for _ in answer.to_option().to_list():
                    rollback.pop_all()
                return answer

        return self.selected(context).bind(lambda selected: added(*selected))

    def edited(self, context: bpy.types.Context, action: Edit, target: LayerAt | EffectAt | LayerMaskAt | EffectMaskAt) -> BlenderResult[bool]:
        """Applies one tuple edit to the selected layer, effect, or mask, answering whether the edit changed the stack."""

        def changed[T](items: tuple[T, ...], index: int, fresh: Callable[[T], BlenderResult[T]]) -> BlenderResult[tuple[tuple[T, ...], int]]:
            selected: BlenderResult[T] = Option.of_optional(next(iter(items[index : index + 1]), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(items), key=str(index)),))

            def edit(value: T) -> BlenderResult[tuple[tuple[T, ...], int]]:
                match action:
                    case Edit.REMOVE:
                        return Ok(((*items[:index], *items[index + 1 :]), max(index - 1, 0)))
                    case Edit.DUPLICATE:
                        return fresh(value).map(lambda replacement: ((*items[: index + 1], replacement, *items[index + 1 :]), index + 1))
                    case Edit.UP | Edit.DOWN:
                        target = min(max(index + (1 if action is Edit.UP else -1), 0), len(items) - 1)
                        order = list(items)
                        order[index], order[target] = order[target], order[index]
                        return Ok((tuple(order), target))
                    case Edit.RESET:
                        return fresh(value).map(lambda replacement: ((*items[:index], replacement, *items[index + 1 :]), index))

            return Result[T, BlenderFaults].bind(selected, edit)

        def edited(entry: bpy.types.SceneCompositorEffect, record: LayerStack[Source, E], _view: StackView, tree: bpy.types.NodeTree) -> BlenderResult[bool]:
            def layer_selected(layer: Layer[Source, E]) -> BlenderResult[bool]:
                def saved(layer: Layer[Source, E]) -> BlenderResult[bool]:
                    return self.replaced(context, entry, record, structs.replace(record, layers=(*record.layers[: target.layer], layer, *record.layers[target.layer + 1 :])), target.layer)

                def effects(index: int) -> BlenderResult[bool]:
                    return changed(
                        layer.effects.value,
                        index,
                        lambda held: (
                            self.stack.offering(type(held.value.effect)).bind(lambda row: row.seeded(entry.id_data).map(lambda effect: structs.replace(held, value=Entry(effect=effect))))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                            if action is Edit.RESET
                            else Ok(held)
                        ),
                    ).bind(lambda pair: saved(structs.replace(layer, effects=structs.replace(layer.effects, value=pair[0]))))

                def mask(index: int) -> BlenderResult[bool]:
                    def blended(blend: Blend) -> BlenderResult[Blend]:
                        return changed(blend.masks, index, lambda value: Ok(Mask() if action is Edit.RESET else value)).map(lambda pair: structs.replace(blend, masks=pair[0]))

                    def effect_mask(at: int) -> BlenderResult[bool]:
                        return Result[Gated[Entry[E]], BlenderFaults].bind(
                            Option.of_optional(next(iter(layer.effects.value[at : at + 1]), None)).to_result_with(lambda: (BlenderAbsentFault(owner=layer.name, key=str(at)),)),
                            lambda value: blended(value.value.blend).bind(
                                lambda blend: saved(
                                    structs.replace(
                                        layer,
                                        effects=structs.replace(
                                            layer.effects, value=(*layer.effects.value[:at], structs.replace(value, value=structs.replace(value.value, blend=blend)), *layer.effects.value[at + 1 :])
                                        ),
                                    )
                                )
                            ),
                        )

                    return effect_mask(target.entry) if isinstance(target, EffectMaskAt) else blended(layer.blend).bind(lambda blend: saved(structs.replace(layer, blend=blend)))

                def fresh(value: Layer[Source, E]) -> BlenderResult[Layer[Source, E]]:
                    return Ok(structs.replace(value, affine=Affine(), blend=Blend(), matte=Gated(enabled=False, value=TrackMatte())))

                match target:
                    case LayerMaskAt(mask=index) | EffectMaskAt(mask=index):
                        return mask(index)
                    case EffectAt(entry=index):
                        return effects(index)
                    case LayerAt() if action is Edit.DUPLICATE:
                        return self.inserted(context, (layer,), Some(tree))
                    case LayerAt(layer=index):
                        return changed(record.layers, index, fresh).bind(lambda pair: self.replaced(context, entry, record, structs.replace(record, layers=pair[0]), pair[1]))

            return Result[Layer[Source, E], BlenderFaults].bind(
                Option.of_optional(next(iter(record.layers[target.layer : target.layer + 1]), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(record), key=str(target.layer)),)),
                layer_selected,
            )

        return self.selected(context).bind(lambda selected: edited(*selected))

    # --- [EDITOR]

    def _commands(self) -> None:
        """Builds native stack, source, layer, effect, mask, and drag operations."""

        def tree(name: str) -> BlenderResult[bpy.types.NodeTree]:
            return Option.of_optional(next((tree for tree, _ in LayerStacks.ownership.held(self.identity) if tree.name == name), None)).to_result_with(
                lambda: (BlenderAbsentFault(owner=repr(bpy.data.node_groups), key=name),)
            )

        def created(call: Call[Empty]) -> BlenderResult[Done]:
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: LayerStacks.ownership.made(self.identity, lambda: Ok(bpy.data.node_groups.new(LayerStacks.title, "CompositorNodeTree")), of=Some(scene)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                .bind(lambda group: self.attached.write(group, self.form()).bind(lambda _: self.switched(call.context, group)))
                .map(lambda _: Done())
            )

        def duplicated(call: Call[Empty]) -> BlenderResult[Done]:
            return (
                call
                .root(bpy.types.Scene)
                .bind(
                    lambda scene: self.head(scene).bind(
                        lambda entry: Result[bpy.types.NodeTree, BlenderFaults].bind(
                            stacked_by(self.identity, entry).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=LayerStacks.ownership.key),)),
                            lambda tree: LayerStacks.ownership.made(self.identity, lambda: Ok(tree.copy()), of=Some(scene)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        )
                    )
                )
                .bind(lambda group: self.switched(call.context, group))
                .map(lambda _: Done())
            )

        def removed(call: Call[Empty]) -> BlenderResult[Done]:
            def remove(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree) -> Done:
                bpy.data.node_groups.remove(tree, do_unlink=True)
                scene.compositor_effects.remove(entry)
                return Done()

            return call.root(bpy.types.Scene).bind(
                lambda scene: self.head(scene).bind(
                    lambda entry: (
                        stacked_by(self.identity, entry).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=LayerStacks.ownership.key),)).map(lambda tree: remove(scene, entry, tree))
                    )
                )
            )

        def added[S: Source](call: Call[S]) -> BlenderResult[Done]:
            return self.inserted(call.context, (Layer(key="inserted", name=type(call.record).title, source=call.record),)).map(lambda _: Done())

        def copied(context: bpy.types.Context, name: str, selected: Option[str]) -> BlenderResult[Outcome]:
            def valued(group: bpy.types.NodeTree) -> BlenderResult[LayerStack[Source, E]]:
                return (
                    Option
                    .of_optional(next((entry for scene in bpy.data.scenes for entry in scene.compositor_effects if entry.node_group == group), None))
                    .map(self.read)
                    .default_with(lambda: self.attached.read(group))
                )

            def chosen(group: bpy.types.NodeTree, record: LayerStack[Source, E]) -> BlenderResult[Outcome]:
                return (
                    Result[tuple[Layer[Source, E], ...], BlenderFaults]
                    .bind(
                        selected.map(
                            lambda key: (
                                Option
                                .of_optional(next((layer for layer in record.layers if layer.key == key), None))
                                .to_result_with(lambda: (BlenderAbsentFault(owner=group.name, key=key),))
                                .map(lambda layer: (layer,))
                            )
                        ).default_value(Ok(record.layers)),
                        lambda layers: self.inserted(context, layers, Some(group)),
                    )
                    .map(lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers"))
                )

            return tree(name).bind(lambda group: valued(group).bind(lambda record: chosen(group, record)))

        def clear(call: Call[Empty]) -> BlenderResult[Outcome]:
            return (
                self
                .selected(call.context)
                .bind(lambda held: self.replaced(call.context, held[0], held[1], structs.replace(held[1], layers=()), 0))
                .map(lambda wrote: Done() if wrote else Unchanged(reason="The stack holds no layers"))
            )

        def nested(call: Call[Empty]) -> BlenderResult[Done]:
            return (
                self
                .selected(call.context)
                .bind(
                    lambda held: (
                        Option
                        .of_optional(next(iter(held[1].layers[held[2].active : held[2].active + 1]), None))
                        .bind(lambda layer: nested_tree(self.identity, held[3], layer))
                        .to_result_with(lambda: (BlenderAbsentFault(owner=LayerStacks.title, key="nested stack"),))
                    )
                )
                .bind(lambda group: self.switched(call.context, group))
                .map(lambda _: Done())
            )

        def media(call: Call[Empty]) -> BlenderResult[Done]:
            return (
                accumulated(loaded(str(path), Tag()) for path in call.paths)
                .bind(lambda images: accumulated(self.add_node(call.context, bpy.types.CompositorNodeImage, functools.partial(written, identifier="image", value=image)) for image in images))
                .map(lambda _: Done())
            )

        def group(call: Call[Switched]) -> BlenderResult[Done]:
            return (
                tree(call.record.stack)
                .bind(lambda group: self.add_node(call.context, bpy.types.CompositorNodeGroup, functools.partial(written, identifier="node_tree", value=group)))
                .map(lambda _: Done())
            )

        def edit(action: Edit, call: Call[Addressed]) -> BlenderResult[Outcome]:
            return self.edited(call.context, action, call.record.target).map(lambda wrote: Done() if wrote else Unchanged(reason=f"{action.name.title()} leaves the stack as it is"))

        def changed(context: bpy.types.Context, transform: Callable[[Layer[Source, E]], BlenderResult[Layer[Source, E]]], at: Option[int] = Nothing) -> BlenderResult[bool]:
            def applied(entry: bpy.types.SceneCompositorEffect, record: LayerStack[Source, E], view: StackView, _tree: bpy.types.NodeTree) -> BlenderResult[bool]:
                index = at.default_value(view.active)
                selected: BlenderResult[Layer[Source, E]] = Option.of_optional(next(iter(record.layers[index : index + 1]), None)).to_result_with(
                    lambda: (BlenderAbsentFault(owner=repr(record), key=str(index)),)
                )
                return (
                    Result[Layer[Source, E], BlenderFaults]
                    .bind(selected, transform)
                    .bind(lambda layer: self.replaced(context, entry, record, structs.replace(record, layers=(*record.layers[:index], layer, *record.layers[index + 1 :])), index))
                )

            return self.selected(context).bind(lambda held: applied(*held))

        def mask(call: Call[Addressed]) -> BlenderResult[Done]:
            def masked(layer: Layer[Source, E]) -> BlenderResult[Layer[Source, E]]:
                match call.record.target:
                    case EffectAt(entry=index) | EffectMaskAt(entry=index):
                        effect: BlenderResult[Gated[Entry[E]]] = Option.of_optional(next(iter(layer.effects.value[index : index + 1]), None)).to_result_with(
                            lambda: (BlenderAbsentFault(owner=layer.name, key=str(index)),)
                        )
                        return effect.map(
                            lambda effect: structs.replace(
                                layer,
                                effects=structs.replace(
                                    layer.effects,
                                    value=(
                                        *layer.effects.value[:index],
                                        structs.replace(effect, value=structs.replace(effect.value, blend=structs.replace(effect.value.blend, masks=(*effect.value.blend.masks, Mask())))),
                                        *layer.effects.value[index + 1 :],
                                    ),
                                ),
                            )
                        )
                    case LayerAt() | LayerMaskAt():
                        return Ok(structs.replace(layer, blend=structs.replace(layer.blend, masks=(*layer.blend.masks, Mask()))))

            return changed(call.context, masked, Some(call.record.target.layer)).map(lambda _: Done())

        def effect(row: Placeable, call: Call[Empty]) -> BlenderResult[Done]:
            return (
                call
                .root(bpy.types.Scene)
                .bind(
                    lambda scene: changed(
                        call.context,
                        lambda layer: row.seeded(scene).map(
                            lambda effect: structs.replace(layer, effects=structs.replace(layer.effects, value=(*layer.effects.value, Gated(enabled=True, value=Entry(effect=effect)))))
                        ),
                    )
                )
                .map(lambda _: Done())
            )

        def output(call: Call[OutputChoice]) -> BlenderResult[Outcome]:
            def chosen(layer: Layer[Source, E]) -> BlenderResult[Layer[Source, E]]:
                _, output_choice = OutputChoice.__struct_fields__
                match layer.source:
                    case RenderPass() | NodeSource():
                        return Ok(structs.replace(layer, source=structs.replace(layer.source, output=call.record.output)))
                    case Adjustment() | Recombined():
                        return Error((BlenderAbsentFault(owner=layer.name, key=output_choice),))

            return changed(call.context, chosen, Some(call.record.layer)).map(lambda wrote: Done() if wrote else Unchanged(reason=f"The layer already reads {call.record.output}"))

        def dragged(context: bpy.types.Context, _travel: Travel) -> BlenderResult[Dragged[E]]:
            return self.selected(context).map(lambda held: Dragged(entry=held[0], record=held[1], index=held[2].active, pitch=Metrics.read().unit))

        def moved(context: bpy.types.Context, state: Dragged[E], travel: Travel) -> BlenderResult[Option[str]]:
            layers, index = state.record.layers, state.index
            target = min(max(index + round(travel.dy / state.pitch), 0), max(len(layers) - 1, 0))
            rest = (*layers[:index], *layers[index + 1 :])
            return self.written_selection(context, state.entry, structs.replace(state.record, layers=(*rest[:target], *layers[index : index + 1], *rest[target:])), target).map(lambda _: Nothing)

        (node_field,) = structs.fields(NodeKind)
        self.output = Operator(key="layer_output", title="Select Output", description="Select an enabled native source output", form=OutputChoice, run=output)
        self.masking = Operator(key="layer_mask_add", title="Add Mask", description="Append a mask to the chosen layer or effect blend", form=Addressed, run=mask)
        self.editing = {
            action: Operator(
                key=f"layer_edit_{action.name.lower()}",
                title=action.name.title(),
                description=f"{action.name.title()} the chosen layer, effect, or mask",
                form=Addressed,
                run=functools.partial(edit, action),
            )
            for action in Edit
        }
        self.commands = (
            Operator(key="layer_stack_new", title="New Stack", description="Create a named native layer stack", form=Empty, run=created),
            Operator(key="layer_stack_duplicate", title="Duplicate Stack", description="Copy the complete stack and native source settings", form=Empty, run=duplicated),
            Operator(key="layer_stack_remove", title="Remove Stack", description="Remove this owned stack and its native users", form=Empty, run=removed),
            Operator(
                key="layer_stack_switch",
                title="Switch Stack",
                description="Edit another named stack",
                form=Switched,
                run=lambda call: tree(call.record.stack).bind(lambda group: self.switched(call.context, group)).map(lambda _: Done()),
                invoke=Some(InvokePopup.SEARCH),
            ),
            Operator(key="layer_stack_nested", title="Edit Nested Stack", description="Open the selected layer's nested stack", form=Empty, run=nested),
            Operator(key="layer_stack_clear", title="Clear Layers", description="Remove every layer of the selected stack", form=Empty, run=clear),
            Operator(
                key="layer_copy_all",
                title="Copy All Layers",
                description="Copy every layer of another stack with its native settings",
                form=Switched,
                run=lambda call: copied(call.context, call.record.stack, Nothing),
                invoke=Some(InvokePopup.SEARCH),
            ),
            Operator(
                key="layer_copy",
                title="Copy Layer",
                description="Copy a selected layer of another stack",
                form=Copied,
                run=lambda call: copied(call.context, call.record.stack, Some(call.record.layer)),
                invoke=Some(Properties()),
            ),
            Operator(key="layer_add_adjustment", title=Adjustment.title, description="Process the composite beneath", form=Adjustment, run=added),
            Operator(key="layer_add_render_pass", title=RenderPass.title, description="Add a view layer output", form=RenderPass, run=added, invoke=Some(Properties())),
            Operator(key="layer_add_recombined", title=Recombined.title, description="Recombine a view layer's native passes", form=Recombined, run=added, invoke=Some(Properties())),
            Operator(
                key="layer_add_node",
                title="Node",
                description="Create a native source and select its enabled output",
                form=NodeKind,
                run=lambda call: node_kind(call.record.node).bind(lambda kind: self.add_node(call.context, kind, lambda _: Ok(None))).map(lambda _: Done()),
                invoke=Some(InvokePopup.SEARCH),
                primary=Some(node_field),
            ),
            Operator(key="layer_add_nested", title="Nested Stack", description="Use another owned stack as a source", form=Switched, run=group, invoke=Some(InvokePopup.SEARCH)),
            Operator(
                key="layer_add_media",
                title="Media",
                description="Load image and movie footage",
                form=Empty,
                run=media,
                invoke=Some(Open(extensions=lambda _: (*sorted(bpy.path.extensions_image), *sorted(bpy.path.extensions_movie)))),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            ),
            self.output,
            self.masking,
            Operator[Empty, Dragged[E]](
                key="layer_drag",
                title="Drag Layer",
                description="Move the selected layer by native row heights",
                form=Empty,
                run=lambda _: Ok(Done()),
                invoke=Some(
                    Modal[Dragged[E]](gesture=Gesture.DRAG_Y, begin=dragged, preview=moved, restore=lambda context, state: self.written_selection(context, state.entry, state.record, state.index))
                ),
            ),
            *self.editing.values(),
            *(
                Operator(key=f"layer_effect_add_{row.kind}", title=row.record.title, description=f"Add {row.record.title} to the selected layer", form=Empty, run=functools.partial(effect, row))
                for row in self.stack.effects
                if row.record.stage < Stage.FORMATION
            ),
        )

    def _layer_list(self) -> ItemList:
        """Binds the native collection and screen selection, with visibility columns."""
        layer = self.attached.group.struct.bl_rna.properties["layers"].fixed_type  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return ItemList(
            key="layer_stack_layers",
            item=type(layer),
            collection=Member(
                root=bpy.types.NodeTree,
                path=f"{self.attached.name}.layers",
                reach=Some(lambda context: root_in(bpy.types.Scene, context).bind(lambda scene: self.head(scene).to_option()).bind(lambda entry: stacked_by(self.identity, entry))),
            ),
            active=Member(root=bpy.types.Screen, path=f"{self.view.name}.active", reach=Some(lambda context: Option.of_optional(context.screen))),
            columns=tuple(Property(target=Member(root=type(layer), path=name), text=Some("")) for name in ("visible", "solo", "matte.enabled")),
            enabled=Some(lambda context, item: self.selected(context).map(lambda selected: any(held.key == item.key for held, _ in selected[1].composited()))),  # ty: ignore[unresolved-attribute]
            sort_reverse=True,
        )

    def _listings(self) -> None:
        """Publishes complete stack, layer, and entry preset recipes."""
        source, effect = get_args(self.form)

        def current(context: bpy.types.Context) -> BlenderResult[LayerStack[Source, E]]:
            return (
                Result[bpy.types.Scene, BlenderFaults]
                .bind(root_in(bpy.types.Scene, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="scene"),)), self.head)
                .bind(self.read)
            )

        def chosen(context: bpy.types.Context) -> BlenderResult[Layer[Source, E]]:
            return self.selected(context).bind(
                lambda held: Option.of_optional(next(iter(held[1].layers[held[2].active : held[2].active + 1]), None)).to_result_with(
                    lambda: (BlenderAbsentFault(owner=LayerStacks.title, key=str(held[2].active)),)
                )
            )

        def last(context: bpy.types.Context) -> BlenderResult[Entry[E]]:
            return chosen(context).bind(
                lambda layer: Option.of_optional(next((value.value for value in layer.effects.value[-1:]), None)).to_result_with(lambda: (BlenderAbsentFault(owner=layer.name, key="effects"),))
            )

        def applied(context: bpy.types.Context, value: LayerStack[Source, E] | Layer[Source, E] | Entry[E]) -> BlenderResult[Outcome]:
            def edited(held: tuple[bpy.types.SceneCompositorEffect, LayerStack[Source, E], StackView, bpy.types.NodeTree]) -> BlenderResult[bool]:
                native, record, view, _tree = held

                def changed(recipe: Layer[Source, E] | Entry[E], prior: Layer[Source, E]) -> BlenderResult[bool]:
                    match recipe:
                        case Layer():
                            layer = structs.replace(recipe, key=prior.key)
                        case Entry():
                            layer = structs.replace(prior, effects=structs.replace(prior.effects, value=(*prior.effects.value, Gated(enabled=True, value=recipe))))
                    return self.replaced(context, native, record, structs.replace(record, layers=(*record.layers[: view.active], layer, *record.layers[view.active + 1 :])), view.active)

                match value:
                    case LayerStack():
                        return self.replaced(context, native, record, value, 0)
                    case Layer() | Entry():
                        prior: BlenderResult[Layer[Source, E]] = Option.of_optional(next(iter(record.layers[view.active : view.active + 1]), None)).to_result_with(
                            lambda: (BlenderAbsentFault(owner=LayerStacks.title, key=str(view.active)),)
                        )
                        return Result[Layer[Source, E], BlenderFaults].bind(prior, functools.partial(changed, value))

            return self.selected(context).bind(edited).map(lambda wrote: Done() if wrote else Unchanged(reason="The stack already holds that preset"))

        def listing[T: msgspec.Struct](read: Callable[[bpy.types.Context], BlenderResult[T]], preset: Preset[T], apply: Callable[[bpy.types.Context, T], BlenderResult[Outcome]]) -> Listing[T]:
            def matches(context: bpy.types.Context, values: tuple[T, ...]) -> BlenderResult[tuple[bool, ...]]:
                return read(context).map(lambda held: tuple(held == value for value in values))

            return Listing(preset=preset, current=read, matches=matches, applied=apply)

        self.listings = (
            listing(current, Preset(key="layer_stack", form=self.form), applied),
            listing(chosen, Preset(key="layer_stack_layer", form=GenericAlias(Layer, (source, effect))), applied),  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]
            listing(last, Preset(key="layer_stack_entry", form=GenericAlias(Entry, (effect,))), applied),  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]
        )

    def drawn(self) -> BlenderResult[RowDraw]:
        """Draws native collection selection, source settings, inputs, and graph widgets."""
        commands = {command.key: command for command in self.commands}

        def draw(listing: RowDraw) -> RowDraw:
            def button(layout: bpy.types.UILayout, command: Placed, text: Option[str] = Nothing) -> None:
                def pressed(idname: str) -> BlenderResult[bpy.types.OperatorProperties]:
                    return text.map(lambda label: operated(layout.operator(idname, text=label), idname)).default_with(lambda: operated(layout.operator(idname), idname))

                alerted(layout, command.invocation(self.identity).bind(lambda placement: pressed(placement[0]).bind(placement[1])))

            def edits(layout: bpy.types.UILayout, target: LayerAt | EffectAt | LayerMaskAt | EffectMaskAt) -> None:
                line = layout.row(align=True)
                for action in Edit:
                    button(line, Own(command=self.editing[action], values=Addressed(target=target)))

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                line = layout.row(align=True)
                for key in ("layer_stack_new", "layer_stack_duplicate", "layer_stack_remove", "layer_stack_switch"):
                    button(line, commands[key])
                listing(layout, holder, context)
                for key in ("layer_stack_clear", "layer_copy", "layer_copy_all", "layer_drag", "layer_stack_nested"):
                    button(layout, commands[key])
                for key in ("layer_add_adjustment", "layer_add_render_pass", "layer_add_recombined", "layer_add_node", "layer_add_nested", "layer_add_media"):
                    button(layout, commands[key])
                selected = self.selected(context)
                alerted(layout, selected)
                for entry, record, view, tree in selected.to_option().to_list():
                    if view.active >= len(record.layers):
                        continue
                    layer = record.layers[view.active]
                    native = self.attached.struct(tree).layers[view.active]  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    for name in ("name", "visible", "solo"):
                        layout.prop(native, name)
                    edits(layout, LayerAt(layer=view.active))
                    layout.prop(native.matte, "enabled")
                    source_values = getattr(native.source, type(layer.source).__name__)
                    for field in structs.fields(type(layer.source)):
                        if isinstance(getattr(layer.source, field.name), str):
                            layout.prop(source_values, field.name)
                    button(layout, Own(command=self.masking, values=Addressed(target=LayerAt(layer=view.active))))
                    nodes = owned(tree, self.identity)
                    for source in Option.of_optional(nodes.get(f"{layer.key}_source")).to_list():
                        source.draw_buttons_ext(context, layout)
                        layout.template_node_inputs(source)
                        for output in (output for output in source.outputs if output.enabled):
                            button(layout, Own(command=self.output, values=OutputChoice(layer=view.active, output=output.name)), Some(output.name))
                    for key, current in nodes.items():
                        match current:
                            case bpy.types.ShaderNodeValToRGB() if key.startswith(f"{layer.key}_"):
                                layout.template_color_ramp(current, "color_ramp", expand=True)
                            case bpy.types.ShaderNodeFloatCurve() if key.startswith(f"{layer.key}_"):
                                layout.template_curve_mapping(current, "mapping")
                    for key in (key for keys in slot(self.identity).map(lambda name: addresses(tree, name)).to_option().to_list() for key in keys if key.startswith(f"{layer.key}_")):
                        alerted(layout, entry_input(self.identity, entry, key).map(lambda owner: layout.prop(owner, "value")))
                    for index, _ in enumerate(layer.blend.masks):
                        edits(layout, LayerMaskAt(layer=view.active, mask=index))
                    for index, effect in enumerate(layer.effects.value):
                        layout.label(text=type(effect.value.effect).title)
                        edits(layout, EffectAt(layer=view.active, entry=index))
                        button(layout, Own(command=self.masking, values=Addressed(target=EffectAt(layer=view.active, entry=index))))
                        for mask, _ in enumerate(effect.value.blend.masks):
                            edits(layout, EffectMaskAt(layer=view.active, entry=index, mask=mask))
                    for row in self.stack.effects:
                        if row.record.stage < Stage.FORMATION:
                            button(layout, commands[f"layer_effect_add_{row.kind}"])

            return shown

        return self.listing.drawn(self.identity).map(draw)

    # --- [DOCUMENTS]

    def layered(self, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[np.uint8 | np.uint16 | np.float32]]:
        """Exports formed color and raw scalar coverage, retaining nested stack groups."""
        number = Result[str, BlenderFaults].bind(
            Option.of_optional(next((part.header.get("Frame") for part in frame.parts[:1]), None)).to_result_with(lambda: (BlenderAbsentFault(owner=frame.path, key="Frame"),)),
            lambda text: catch(lambda: int(text), exception=ValueError)().map_error(lambda error: (BlenderEditFault(request=frame.path, error=error),)),
        )

        def built[D: np.uint8 | np.uint16 | np.float32](coded: Callable[[npt.NDArray[np.float32]], npt.NDArray[D]]) -> BlenderResult[Document[D]]:
            def composed(tree: bpy.types.NodeTree, record: LayerStack[Source, E], at: int, config: PyOpenColorIO.Config, view: Option[DisplayView]) -> BlenderResult[Document[D]]:
                def read_tree(tree: bpy.types.NodeTree, record: LayerStack[Source, E]) -> BlenderResult[tuple[documents.Layers[D], npt.NDArray[np.float32]]]:
                    path = str(Path(bpy.app.tempdir) / f"layer_stack_{tree.session_uid}_{at:0{LayerStacks.digits}d}.exr")

                    def planes(file: Multilayer) -> BlenderResult[tuple[documents.Layers[D], npt.NDArray[np.float32]]]:
                        shown = frozenset(layer.key for layer, _ in record.composited())

                        def raw(name: str) -> BlenderResult[npt.NDArray[np.float32]]:
                            return Result[FileLayer, BlenderFaults].bind(
                                Option.of_optional(next((layer for layer in file.layers if layer.name == name), None)).to_result_with(lambda: (BlenderAbsentFault(owner=path, key=name),)),
                                lambda layer: file.plane(layer, layer.channels).map_error(lambda faults: (BlenderImagingFault(faults=faults),)),
                            )

                        def color(name: str) -> BlenderResult[npt.NDArray[np.float32]]:
                            return (
                                raw(name)
                                .bind(
                                    lambda pixels: (
                                        view
                                        .map(lambda shown: shown.form(config, pixels))
                                        .default_with(lambda: viewed(scene).form(config, pixels, exposure=scene.view_settings.exposure, gamma=scene.view_settings.gamma))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                                        .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
                                    )
                                )
                                .map(unassociated)
                            )

                        def each(layer: Layer[Source, E]) -> BlenderResult[documents.Raster[D] | documents.Adjustment[D] | documents.Group[D]]:
                            visible = layer.key in shown
                            match layer.source:
                                case Adjustment():
                                    return color(f"{layer.key} Composite").map(
                                        lambda pixels: documents.Adjustment(name=layer.name, color=coded(pixels[..., :3]), alpha=coded(pixels[..., 3]), visible=visible)
                                    )
                                case _:
                                    inner = nested_tree(self.identity, tree, layer).filter(
                                        lambda _: (
                                            layer.affine == Affine()
                                            and not layer.blend.masks
                                            and not layer.matte.enabled
                                            and not (layer.effects.enabled and any(effect.enabled for effect in layer.effects.value))
                                        )
                                    )
                                    mask = raw(f"{layer.key} Mask").map(lambda pixels: Some(coded(pixels[..., 0]))) if layer.blend.masks or layer.matte.enabled else Ok(Nothing)
                                    return inner.map(
                                        lambda tree: (
                                            self.attached
                                            .read(tree)
                                            .bind(lambda record: read_tree(tree, record))
                                            .map(lambda parts: Group(name=layer.name, layers=parts[0], mode=Some(layer.blend.mode), opacity=layer.blend.opacity, visible=visible))
                                        )
                                    ).default_with(
                                        lambda: paired(color(layer.key), mask).map(
                                            lambda parts: Raster(
                                                name=layer.name,
                                                color=coded(parts[0][..., :3]),
                                                alpha=coded(parts[0][..., 3]),
                                                mask=parts[1],
                                                mode=layer.blend.mode,
                                                opacity=layer.blend.opacity,
                                                visible=visible,
                                            )
                                        )
                                    )

                        top = color("Composite")
                        return paired(accumulated(each(layer) for layer in record.layers), top)

                    return Result[Multilayer, BlenderFaults].bind(Multilayer.read(path).map_error(lambda faults: (BlenderImagingFault(faults=faults),)), planes)

                shown = view.default_with(lambda: viewed(scene))
                return read_tree(tree, record).bind(
                    lambda parts: (
                        shown
                        .colorspace(config)
                        .to_result_with(lambda: (BlenderAbsentFault(owner=repr(config), key=f"{shown.display}/{shown.view}"),))
                        .map(lambda space: Document(layers=parts[0], composite=coded(parts[1]), colorspace=space.getName(), density=scene.render.ppm_factor / scene.render.ppm_base))
                    )
                )

            return paired(paired(number, self.stack.config()), formation.current(self.identity, scene)).bind(
                lambda parts: self.head(scene).bind(
                    lambda entry: self.read(entry).bind(
                        lambda record: Result[bpy.types.NodeTree, BlenderFaults].bind(
                            stacked_by(self.identity, entry).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=LayerStacks.ownership.key),)),
                            lambda tree: composed(tree, record, *parts[0], parts[1]),
                        )
                    )
                )
            )

        match depth:
            case Depth.UINT8:
                return built(lambda pixels: codes(pixels, 8))
            case Depth.UINT16:
                return built(lambda pixels: codes(pixels, 16))
            case Depth.HALF | Depth.FLOAT:
                return built(lambda pixels: pixels)


# --- [OPERATIONS] -----------------------------------------------------------------------


def stacked_by(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Option[bpy.types.NodeTree]:
    """Returns an entry's group only when it carries the stack ownership mark."""
    return Option.of_optional(entry.node_group).filter(lambda group: Mark.read(identity, group).filter(lambda mark: mark.key == LayerStacks.ownership.key).is_some())


def nested_tree[E: Effect](identity: Identity, tree: bpy.types.NodeTree, layer: Layer[Source, E]) -> Option[bpy.types.NodeTree]:
    """Returns an owned nested stack from the layer's actual group node."""
    match owned(tree, identity).get(f"{layer.key}_source"):
        case bpy.types.CompositorNodeGroup(node_tree=inner) if inner is not None:
            return Some(inner).filter(lambda tree: Mark.read(identity, tree).filter(lambda mark: mark.key == LayerStacks.ownership.key).is_some())
        case _:
            return Nothing


def layer_conditions(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[dict[Key, Condition]]:
    """Restricts denoise targets to terms actually owned by each layer source."""

    def found(tree: bpy.types.NodeTree, name: str) -> BlenderResult[dict[Key, Condition]]:
        def accepted(source: bpy.types.PropertyGroup) -> tuple[str, ...]:
            return (
                (Frame.title, Passes.title if getattr(getattr(source, Recombined.__name__).recombination, Case.__struct_config__.tag_field) == Lobes.__name__ else Groups.title)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                if getattr(source, Case.__struct_config__.tag_field) == Recombined.__name__  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                else (Frame.title,)
            )

        def setting(key: Key, titles: tuple[str, ...]) -> BlenderResult[tuple[Key, Condition]]:
            return entry_input(identity, entry, key).map(
                lambda item: (key, Setting(descriptor=Descriptor(root=bpy.types.Scene, owner=item.path_from_id(), prop=item.bl_rna.properties["value"]), accepted=titles))
            )

        return accumulated(
            setting(item_key(layer.key, "effects", index, *Filters.of().target), accepted(layer.source))
            for layer in getattr(tree, name).layers
            for index, gated in enumerate(layer.effects.value, 1)
            if getattr(gated.value.effect, Case.__struct_config__.tag_field) == SpatialDenoise.__name__  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        ).map(dict)

    return paired(
        stacked_by(identity, entry).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=LayerStacks.ownership.key),)), identity.name(Name.ATTACHMENT, LayerStacks.ownership.key)
    ).bind(lambda pair: found(*pair))


def node_kind(identifier: str) -> BlenderResult[type[bpy.types.Node]]:
    """Returns the registered node class an identifier names, or its absence."""
    return Option.of_optional(bpy.types.Node.bl_rna_get_subclass_py(identifier)).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.types.Node", key=identifier),))


def sourced[E: Effect](layer: Layer[Source, E], scene: bpy.types.Scene) -> BlenderResult[Fragment]:
    """Constructs one original native source without mirroring its settings."""
    match layer.source:
        case RenderPass(layer=name):
            return node(f"{layer.key}_source", bpy.types.CompositorNodeRLayers, {}, {"scene": IdName.of(scene), "layer": name})
        case Recombined(layer=name):
            return node(f"{layer.key}_render", bpy.types.CompositorNodeRLayers, {}, {"scene": IdName.of(scene), "layer": name})
        case NodeSource(node=identifier):
            return node_kind(identifier).bind(lambda kind: node(f"{layer.key}_source", kind, {}, {}))
        case Adjustment():
            return Ok(Fragment())


def inlined(prefix: Key, group: NodeGroup, outer: Callable[[Key], End]) -> BlenderResult[tuple[Fragment, End]]:
    """Relays native group inputs and preserves original ordered link multiplicity."""

    def mapped(end: End) -> End:
        return outer(end.socket) if end.node == INPUTS else End(node=f"{prefix}_{end.node}", socket=end.socket)

    return (
        Option
        .of_optional(next((link.source for link in group.graph.links if link.target == End(node=OUTPUTS, socket=Shell.result)), None))
        .to_result_with(lambda: (BlenderAbsentFault(owner=group.name, key=Shell.result),))
        .map(
            lambda result: (
                Fragment(
                    nodes={f"{prefix}_{key}": value for key, (value, _) in group.graph.placed.items() if key not in {INPUTS, OUTPUTS}},
                    links=tuple(Link(source=mapped(link.source), target=mapped(link.target)) for link in group.graph.links if link.target.node != OUTPUTS),
                ),
                mapped(result),
            )
        )
    )


def unit[E: Effect](layer: Layer[Source, E], below: End, base: End, planning: Planning, *, mixes: bool) -> BlenderResult[tuple[Fragment, InterfacePanel, tuple[tuple[FileItem, End], ...]]]:
    """Constructs a source, gated effects, affine sampling, masks, and covered content."""
    key = layer.key
    context = {key: End(node=INPUTS, socket=key) for field in structs.fields(CameraFrame) for key in (item_key(Shell.frame, field.name),)}
    frame = context[item_key(Shell.frame, "aspect")], context[item_key(Shell.frame, "scale")]

    def outer(*parts: Key | int) -> End:
        return End(node=INPUTS, socket=item_key(key, *parts))

    def entering() -> BlenderResult[tuple[Fragment, End]]:
        match layer.source:
            case Adjustment():
                return Ok((Fragment(), base))
            case RenderPass(output=name) | NodeSource(output=name):
                return Result[str, BlenderFaults].bind(
                    Option.of_optional(planning.offered[f"{key}_source"].get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=f"{key}_source", key=name),)),
                    lambda identifier: sourced(layer, planning.scene).map(lambda body: (body, End(node=f"{key}_source", socket=identifier))),
                )
            case Recombined(layer=view) as source:
                filter_entry = Option.of_optional(next((index for index, entry in enumerate(layer.effects.value, 1) if isinstance(entry.value.effect, SpatialDenoise)), None)).map(
                    lambda index: Denoising(group=planning.denoiser, entry=item_key(key, "effects", index), switch=f"{item_key(key, 'effects', index)}_{item_key(*Filters.of().target)}")
                )

                def filing(request: Filing, at: Key) -> BlenderResult[Fragment]:
                    return (
                        Result[bpy.types.ViewLayer, BlenderFaults]
                        .bind(
                            Option.of_optional(planning.scene.view_layers.get(view)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(planning.scene.view_layers), key=view),)),
                            planning.routed.routing.read,
                        )
                        .bind(lambda routing: filed(planning.routed.pathed, planning.path, view, request, at, routing.convention, planning.routed.dimensions))
                    )

                return source.recombined(key, lambda inner: outer("source", inner), planning.scene, planning.offered[f"{key}_render"], planning.routed.lobe, filter_entry, filing)

    def grouped(index: int, entry: Gated[Entry[E]], row: Placeable) -> BlenderResult[tuple[int, Gated[Entry[E]], Placeable, NodeGroup]]:
        return row.grouped(planning.identity, planning.scene, entry.value, planning.config).map(lambda group: (index, entry, row, group))

    groups = accumulated(planning.stack.offering(type(entry.value.effect)).bind(functools.partial(grouped, index, entry)) for index, entry in enumerate(layer.effects.value, 1))

    def chained(content: tuple[Fragment, End], entries: tuple[tuple[int, Gated[Entry[E]], Placeable, NodeGroup], ...]) -> BlenderResult[tuple[Fragment, End]]:
        def step(done: BlenderResult[tuple[Fragment, End]], held: tuple[int, Gated[Entry[E]], Placeable, NodeGroup]) -> BlenderResult[tuple[Fragment, End]]:
            index, _, _, group = held
            prefix = item_key(key, "effects", index)
            return done.bind(
                lambda source: inlined(
                    prefix, group, lambda inner: source[1] if inner == Shell.image else context[inner] if inner in context else End(node=INPUTS, socket=item_key(prefix, inner))
                ).bind(lambda result: switched(f"{prefix}_switch", "RGBA", End(node=INPUTS, socket=prefix), source, result))
            )

        ordered = tuple((index, entry, row, group) for _, index, entry, row, group in sorted((entry.value.effect.stage, index, entry, row, group) for index, entry, row, group in entries))
        return functools.reduce(step, ordered, Ok(content)).bind(lambda chain: switched(f"{key}_effects", "RGBA", outer("effects"), content, chain))

    def transformed(content: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        return framed(f"{key}_space", content, *frame).bind(
            lambda space: placement(f"{key}_affine", space, *(outer("affine", field.name) for field in structs.fields(Affine))).bind(lambda matrix: placed(f"{key}_placed", content, space, matrix))
        )

    def covered(original: tuple[Fragment, End], content: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, tuple[tuple[FileItem, End], ...]]]:
        _, content_end = content
        keyed = masks.masked(f"{key}_masks", len(layer.blend.masks), original, *frame, lambda inner: outer("blend", "masks", inner))
        matted = (
            masks.matte(planning.luminance).bind(
                lambda group: nested(f"{key}_matte", group, masks.relayed(lambda inner: below if inner == Shell.image else outer("matte", inner), group.interface), "key").map(Some)
            )
            if layer.matte.enabled
            else Ok(Nothing)
        )

        def combined(parts: tuple[Option[tuple[Fragment, End]], Option[tuple[Fragment, End]]]) -> BlenderResult[tuple[Fragment, tuple[tuple[FileItem, End], ...]]]:
            factors = tuple(value for part in parts for value in part.to_list())

            def mixed(mask: Option[tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, tuple[tuple[FileItem, End], ...]]]:
                return product(f"{key}_weight", (outer("blend", "opacity"), *mask.to_list())).bind(
                    lambda weight: paired(
                        mixed_back(key, base, content, outer("blend", "mode"), weight) if mixes else Ok((Fragment(), base)),
                        node(f"{key}_covered", bpy.types.CompositorNodeSetAlpha, {"Image": content, "Alpha": weight, "Type": "Apply Mask"}, {}),
                    ).bind(
                        lambda built: Fragment.merged((built[0][0], built[1])).map(
                            lambda joined: (joined, ((FileItem(name=layer.key, kind="RGBA"), content_end), *((FileItem(name=f"{layer.key} Mask", kind="FLOAT"), end) for _, end in mask.to_list())))
                        )
                    )
                )

            return product(f"{key}_mask", (factors[0], *factors[1:])).bind(lambda mask: mixed(Some(mask))) if factors else mixed(Nothing)

        return paired(keyed, matted).bind(combined)

    def faced(entries: tuple[tuple[int, Gated[Entry[E]], Placeable, NodeGroup], ...]) -> BlenderResult[InterfacePanel]:
        def panel(name: Key, face: Interface, toggle: Option[bool] = Nothing) -> InterfacePanel:
            inner = under(item_key(key, name), face)
            return InterfacePanel(key=item_key(key, name), name=name.title(), toggle=toggle, inputs=inner.inputs, panels=inner.panels)

        def entry_panel(index: int, value: Gated[Entry[E]], row: Placeable, group: NodeGroup) -> InterfacePanel:
            prefix = item_key(key, "effects", index)
            inner = under(prefix, Interface(inputs=tuple(socket for socket in group.interface.inputs if socket.key not in {Shell.image, *context}), panels=group.interface.panels))
            return InterfacePanel(key=prefix, name=row.record.title, toggle=Some(value.enabled), inputs=inner.inputs, panels=inner.panels)

        source = Interface.of(type(layer.source)).bind(lambda face: expanded(face, layer.source))
        blend = Interface.of(Blend).bind(lambda face: expanded(face, layer.blend))
        return paired(paired(source, Interface.of(Affine)), paired(blend, Interface.of(TrackMatte))).map(
            lambda parts: InterfacePanel(
                key=key,
                name=layer.name,
                panels=(
                    panel("source", parts[0][0]),
                    structs.replace(panel("effects", Interface(), Some(layer.effects.enabled)), panels=tuple(itertools.starmap(entry_panel, entries))),
                    panel("affine", parts[0][1]),
                    panel("blend", parts[1][0]),
                    panel("matte", parts[1][1]),
                ),
            )
        )

    return paired(entering(), groups).bind(
        lambda parts: (
            chained(*parts)
            .bind(transformed)
            .bind(
                lambda content: paired(covered(parts[0], content), faced(parts[1])).bind(
                    lambda built: Fragment.merged((parts[0][0], content[0], built[0][0])).map(lambda joined: (joined, built[1], built[0][1]))
                )
            )
        )
    )


def planned[E: Effect](record: LayerStack[Source, E], planning: Planning) -> BlenderResult[NodeGroup]:
    """Builds every layer and its matte even when its composite is hidden."""
    shown = frozenset(layer.key for layer, _ in record.composited())
    canvas = End(node="canvas", socket="Image")
    covered = (canvas, *(End(node=f"{layer.key}_covered", socket="Image") for layer in record.layers))
    running = tuple(itertools.accumulate(record.layers, lambda prior, layer: End(node=f"{layer.key}_mixback", socket="image") if layer.key in shown else prior, initial=canvas))
    background = node("canvas_ramp", bpy.types.ShaderNodeTexGradient, {}, {"gradient_type": "LINEAR"}).bind(
        lambda ramp: node("canvas", bpy.types.CompositorNodeSetAlpha, {"Image": (ramp, End(node="canvas_ramp", socket="Color")), "Alpha": 0.0, "Type": "Apply Mask"}, {})
    )

    def built(parts: tuple[tuple[Fragment, InterfacePanel, tuple[tuple[FileItem, End], ...]], ...], canvas_part: Fragment, face: Interface) -> BlenderResult[NodeGroup]:
        items = (
            (FileItem(name="Composite", kind="RGBA"), running[-1]),
            *(item for _, _, items in parts for item in items),
            *((FileItem(name=f"{layer.key} Composite", kind="RGBA"), end) for layer, end in zip(record.layers, running[1:])),
        )
        files = node(
            "layers",
            bpy.types.CompositorNodeOutputFile,
            {item.name: end for item, end in items},
            {
                "directory": bpy.app.tempdir,
                "file_name": f"layer_stack_{planning.uid}_{'#' * LayerStacks.digits}",
                "format.media_type": "MULTI_LAYER_IMAGE",
                "format.file_format": "OPEN_EXR_MULTILAYER",
                "format.color_depth": "32",
            },
            Some(FileItems(items=tuple(item for item, _ in items))),
        )

        def framed_group(group: NodeGroup) -> NodeGroup:
            label = Descriptor(root=bpy.types.NodeFrame, owner="", prop=bpy.types.NodeFrame.bl_rna.properties["label"])
            framed_keys = frozenset(key for body, _, _ in parts for key in body.nodes)
            frames = {f"{layer.key}_frame": FrameNode(members={label: layer.name}, nodes=dict(body.nodes)) for layer, (body, _, _) in zip(record.layers, parts)}
            return structs.replace(
                group,
                graph=structs.replace(
                    group.graph,
                    members={
                        Descriptor(root=bpy.types.NodeTree, owner="", prop=bpy.types.CompositorNodeTree.bl_rna.properties[name]): True
                        for name in ("allow_usage_in_scene_compositor_effect", "use_fake_user")
                    },
                    nodes={**{key: value for key, value in group.graph.nodes.items() if key not in framed_keys}, **frames},
                ),
            )

        return (
            files
            .bind(lambda filed: Fragment.merged((canvas_part, *(body for body, _, _ in parts), filed)))
            .bind(
                lambda joined: NodeGroup.built(
                    key=LayerStacks.ownership.key,
                    name=LayerStacks.title,
                    interface=Interface(outputs=(Socket(key=Shell.image, name="Image", kind=bpy.types.NodeSocketColor),), inputs=face.inputs, panels=tuple(panel for _, panel, _ in parts)),
                    outputs={Shell.image: (joined, running[-1])},
                )
            )
            .map(framed_group)
        )

    return paired(
        accumulated(unit(layer, below, base, planning, mixes=layer.key in shown) for layer, below, base in zip(record.layers, covered, running)),
        paired(background, Interface.of(CameraFrame).map(lambda face: under(Shell.frame, face))),
    ).bind(lambda parts: built(parts[0], *parts[1]))


# --- [COMPOSITION] ----------------------------------------------------------------------


class LayerStacks[E: Effect]:
    """Native named stack ownership and the complete editor row an application places."""

    form: type[LayerStack[Source, E]]
    stack: EffectStack
    title: ClassVar[str] = "Layer Stack"
    digits: ClassVar[int] = 4
    ownership: ClassVar[Ownership[bpy.types.CompositorNodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key="layer_stack", saved=Saved.ALWAYS, single=False)
    view: ClassVar[Attachment[StackView]] = Attachment(key="layer_stack_view", form=StackView, owner=bpy.types.Screen)

    pinned: ClassVar[Pinned] = Pinned(ownership=ownership, edge=Edge.HEAD, conditions=Some(layer_conditions))

    def __init__(self, *, form: type[LayerStack[Source, E]], stack: EffectStack) -> None:
        """Retains original attachment identity and per-registration native resources."""
        self.form, self.stack = form, stack
        self.attachment = Attachment(key=self.ownership.key, form=form, owner=bpy.types.NodeTree, changed=Some(self.changed))
        self._bindings: dict[int, LayerStacking[E]] = {}

    @property
    def page_rows(self) -> tuple[Attachment[LayerStack[Source, E]], Attachment[StackView]]:
        """Exposes original attachment rows directly to the application's preferences."""
        return self.attachment, self.view

    @contextmanager
    def changed(self, identity: Identity) -> Generator[Changed]:
        """Retains deferred reconciliation for exactly the attachment's native lifetime."""
        with identity.reports.deferred(lambda: self.bound(identity).bind(lambda stacking: stacking.refreshed())) as refresh:
            yield lambda _owner, _context, _property: refresh()

    def bound(self, identity: Identity) -> BlenderResult[LayerStacking[E]]:
        """Binds independent native stores, pass descriptors, routing, and frame metadata."""

        def retained(values: tuple[tuple[Attached[LayerStack[Source, E]], Attached[StackView]], tuple[tuple[RecombinationFlags, PassRouting], Derived[Effect, CameraFrame]]]) -> LayerStacking[E]:
            (attached, view), ((flags, routed), frame) = values
            held = LayerStacking(identity=identity, form=self.form, stack=self.stack, attached=attached, view=view, flags=flags, routed=routed, framing=frame)
            self._bindings[id(identity)] = held
            return held

        return (
            Option
            .of_optional(self._bindings.get(id(identity)))
            .map(Ok)
            .default_with(
                lambda: paired(paired(self.attachment.bound(identity), self.view.bound(identity)), paired(paired(RecombinationFlags.of(), PassSet().bound(identity)), framing(identity))).map(retained)
            )
        )

    def document(self, identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[np.uint8 | np.uint16 | np.float32]]:
        """Implements the output owner's native layered-document protocol."""
        return self.bound(identity).bind(lambda stacking: stacking.layered(scene, frame, depth))

    def section(self, header: tuple[RowContent, ...] = ()) -> Single:
        """Places the native editor under a reusable application-supplied header."""
        return Single(key=self.ownership.key, title=self.title, header=(Popover(panel=self.presets().key, text=Some(""), icon=Some("PRESET")), *header), rows=(self,))

    def presets(self) -> Single:
        """Places native stack, selected-layer, and selected-effect recipe listings."""
        return Single(key=f"{self.ownership.key}_presets", title=f"{self.title} Presets", rows=((self, self.presets_drawn),))

    def presets_drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires and composes the complete native recipe listings."""

        def draw(rows: tuple[RowDraw, ...]) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for row in rows:
                    row(layout, holder, context)

            return shown

        return self.bound(identity).bind(lambda stacking: accumulated(listing.drawn(identity) for listing in stacking.listings)).map(draw)

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Acquires the native editor through its actual registered stack binding."""
        return self.bound(identity).bind(lambda stacking: stacking.drawn())

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds deferred native changes while publishing commands and subscriptions."""
        with identity.reports.deferred(lambda: self.bound(identity).bind(lambda stacking: stacking.refreshed())) as refresh:

            def updated() -> BlenderResult[None]:
                refresh()
                return Ok(None)

            try:
                yield self.bound(identity).map(
                    lambda stacking: (
                        self.ownership,
                        stacking.listing,
                        *stacking.commands,
                        *stacking.listings,
                        PathHandler(handlers=bpy.app.handlers.load_post, body=lambda _: updated()),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        *(Subscription(owner=owner, setting=prop, body=updated) for owner, prop in stacking.framing.reads),
                        Subscription(owner=bpy.types.RenderSettings, setting=bpy.types.RenderSettings.bl_rna.properties["film_transparent"], body=updated),
                        *(Subscription(owner=bpy.types.ViewLayer, setting=bpy.types.ViewLayer.bl_rna.properties[name], body=updated) for name in ("name", "use")),
                        Subscription(owner=bpy.types.Object, setting=bpy.types.Object.bl_rna.properties["lightgroup"], body=updated),
                        Subscription(owner=bpy.types.World, setting=bpy.types.World.bl_rna.properties["lightgroup"], body=updated),
                    )
                )
            finally:
                self._bindings.pop(id(identity), None)


COMPOSITED = image_type(("RENDER_RESULT", "COMPOSITING"))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "COMPOSITED",
    "Copied",
    "Dragged",
    "Edit",
    "LayerStacking",
    "LayerStacks",
    "NodeKind",
    "NodeSource",
    "Planning",
    "RenderPass",
    "Source",
    "StackView",
    "Switched",
    "inlined",
    "nested_tree",
    "planned",
    "sourced",
    "stacked_by",
    "unit",
]
