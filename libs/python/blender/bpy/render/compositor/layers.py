"""Layer stacks: layers of a source through compositor effects, placement, blend, and track matte, their pure structure edits and stored parts, the host forms of stack commands, layer rows, and the screen selection, and the node group a stack plans over the effects an extension offers."""

from collections.abc import Callable, Iterator, Mapping
from enum import Enum
import functools
from functools import partial
import graphlib
import itertools
from typing import Final, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
import msgspec
import PyOpenColorIO

from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.mirrors import case_choices, choices, enum_choices, held_choices
from blender.bpy.data.rna.paths import Struct, subclass
from blender.bpy.extension.identity import Key
from blender.bpy.nodes import functions
from blender.bpy.nodes.desired import End, FrameNode, INPUTS, Interface, InterfacePanel, item_key, Members, NodeGroup, OUTPUTS, Socket, under
from blender.bpy.nodes.functions import lowered
from blender.bpy.nodes.graphs import Expr, group_input, math, switched
from blender.bpy.nodes.trees import DATA_TYPES
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, with_alpha
from blender.bpy.render.compositor.coordinates import FrameSpace
from blender.bpy.render.compositor.effects import CompositorEffect, Entry
from blender.bpy.render.compositor.entries import Edge, Pinned
from blender.bpy.render.compositor.inputs import FRAME, FRAME_ASPECT, FRAME_SCALE
from blender.bpy.render.compositor.masks import Blend, mixed_back, MODE, OPACITY
from blender.bpy.render.compositor.recombination import Recombination, Recombined, TermFilter
from blender.bpy.render.outputs.formats import FileItem, FileItems, OutputFormat
from blender.bpy.render.outputs.paths import Pathed
from blender.bpy.render.view_layers.passes import PassRequest, used
from blender.bpy.render.view_layers.sources import lobe
from blender.bpy.settings.attachments import Attachment
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import amended, Case, focused, Lens, member
from blender.records.trees import Toward
from blender.rendering.outputs.naming import Output

if TYPE_CHECKING:
    from blender.bpy.render.compositor.stacking import LayerStacking

# --- [TYPES] ----------------------------------------------------------------------------

type Source = Adjustment | RenderPass | NodeSource | Recombined
type Target = LayerAt | EffectAt | LayerMaskAt | EffectMaskAt
type Order = tuple[Option[int], ...]
type _Unit = tuple[Expr, Option[Expr], tuple[tuple[str, Expr], ...], InterfacePanel]


class Edit(Enum):
    """Edits of layers, effects, and masks, each valued by its button icon."""

    REMOVE = "REMOVE"
    DUPLICATE = "DUPLICATE"
    UP = "TRIA_UP"
    DOWN = "TRIA_DOWN"
    RESET = "LOOP_BACK"

    def reordered(self, index: int, count: int, *, drawn_reversed: bool = False) -> Order:
        """Returns the prior position each position holds after the edit of `index`, `Nothing` where a reset item takes its defaults, a reversed list moving against its indices."""
        match self:
            case Edit.REMOVE:
                return tuple(Some(position) for position in range(count) if position != index)
            case Edit.DUPLICATE:
                return tuple(Some(position) for position in (*range(index + 1), index, *range(index + 1, count)))
            case Edit.RESET:
                return tuple(Nothing if position == index else Some(position) for position in range(count))
            case Edit.UP | Edit.DOWN:
                step = Toward.UP if self is Edit.UP else Toward.DOWN
                other = count - 1 - step.stepped(count - 1 - index, count) if drawn_reversed else step.stepped(index, count)
                return tuple(Some(other if position == index else index if position == other else position) for position in range(count))


# --- [CONSTANTS] ------------------------------------------------------------------------

AFFINE: Final = Interface(
    inputs=(
        Socket(key="position", name="Position", kind=bpy.types.NodeSocketVector2D, description="Offset in frame fractions", default_value=(0.0, 0.0)),
        Socket(key="rotation", name="Rotation", kind=bpy.types.NodeSocketFloatAngle, description="Turn about pivot"),
        Socket(key="scale", name="Scale", kind=bpy.types.NodeSocketVector2D, description="Signed factor per frame axis", default_value=(1.0, 1.0)),
        Socket(key="skew", name="Skew", kind=bpy.types.NodeSocketVector2D, description="Horizontal and vertical shear angles", default_value=(0.0, 0.0)),
        Socket(key="pivot", name="Pivot", kind=bpy.types.NodeSocketVector2D, description="Fixed point of scale, shear, and rotation", default_value=(0.5, 0.5)),
    )
)
IMAGE: Final = "Image"
OWNERSHIP: Final = Ownership(kind=bpy.types.CompositorNodeTree, key="layer_stack", saved=Saved.ALWAYS, single=False)
TITLE: Final = bpy.path.display_name(OWNERSHIP.key)
PINNED: Final = Pinned(ownership=OWNERSHIP, edge=Edge.HEAD)
_AFFINE: Final[Key] = "affine"
_PADDING: Final = 4

# --- [CHOICES] --------------------------------------------------------------------------


def views(_owner: Struct, context: bpy.types.Context | None) -> tuple[tuple[str, str, str, str, int], ...]:
    """Used view layer items of the context scene."""
    return held_choices(tuple((layer.name, layer.name, "") for scene in Option.of_optional(context).bind(lambda held: Option.of_optional(held.scene)).to_list() for layer in used(scene)))


# --- [MODELS] ---------------------------------------------------------------------------


class Adjustment(Case, frozen=True):
    """Composite beneath through layer effects."""


class RenderPass(Case, frozen=True, kw_only=True):
    """One output of a scene view layer."""

    layer: str
    output: str = IMAGE


class NodeSource(Case, frozen=True, kw_only=True):
    """Owned compositor source node with editable settings."""

    node: str
    output: str


class LayerAt(Case, frozen=True, kw_only=True):
    """Edit of one layer."""

    layer: int


class EffectAt(Case, frozen=True, kw_only=True):
    """Edit of one effect within a layer."""

    layer: int
    entry: int


class LayerMaskAt(Case, frozen=True, kw_only=True):
    """Edit of one layer mask."""

    layer: int
    mask: int


class EffectMaskAt(Case, frozen=True, kw_only=True):
    """Edit of one effect's blend mask."""

    layer: int
    entry: int
    mask: int


class StackLayer[S: Case, M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Source through compositor effects and placement, blended over the layers beneath, its values held by the stack group's inputs."""

    key: str
    name: str
    source: S
    effects: tuple[Entry[M], ...] = ()
    blend: Blend = Blend()
    matte: bool = False
    visible: bool = True
    solo: bool = False

    @property
    def node(self) -> Key:
        """Key of the layer's owned source node."""
        return item_key(self.key, SOURCE)

    @property
    def plain(self) -> bool:
        """Whether the layer composites its source unmasked, unmatted, and through no effect."""
        return not self.blend.masks and not self.matte and not self.effects


class LayerStack[S: Case, M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Layers ordered bottom first over a transparent frame, the list drawing them top first."""

    layers: tuple[StackLayer[S, M], ...] = ()

    def composited(self) -> tuple[tuple[StackLayer[S, M], Option[StackLayer[S, M]]], ...]:
        """Selects visible layers under solo, leaving out layers a matte above consumes.

        Returns:
            Layers in stack order, each with the layer beneath, absent at the bottom.
        """
        soloed = any(layer.solo for layer in self.layers)
        beneath: tuple[Option[StackLayer[S, M]], ...] = (Nothing, *map(Some, self.layers))
        consumed = (*(layer.matte for layer in self.layers[1:]), False)
        return tuple((layer, below) for layer, below, matte in zip(self.layers, beneath, consumed) if layer.visible and not matte and (layer.solo or not soloed))

    def fresh(self) -> Iterator[Key]:
        """Layer keys the stack holds no layer under, in order."""
        taken = frozenset(layer.key for layer in self.layers)
        return (key for index in itertools.count(1) for key in (f"layer_{index}",) if key not in taken)

    def layer(self, index: int) -> Outcome[StackLayer[S, M]]:
        """Returns the layer at a bottom-first index, absent past the stack."""
        return Option.of_optional(self.layers[index] if 0 <= index < len(self.layers) else None).to_result_with(partial(Fault.of, Reason.ABSENT, type(self).__name__, member=str(index)))

    def edited(self, edit: Edit, target: Target, seeded: Callable[[M], Outcome[M]]) -> Outcome[tuple["LayerStack[S, M]", int, Order]]:
        """Applies an edit to a target's item, answering the edited stack, the layer it leaves active, and the prior position of each item's values.

        Args:
            edit: Edit applied.
            target: Layer, effect, or mask the edit applies to.
            seeded: Defaults of an effect record a reset restores.

        Returns:
            `Ok` of the edited stack, active layer index, and value order. `Error` of `absent` for a target past its collection and of seed faults.
        """
        absent = partial(Fault.of, Reason.ABSENT, type(self).__name__, member=repr(target))
        match target:
            case LayerMaskAt(layer=layer, mask=index) | EffectMaskAt(layer=layer, mask=index):
                return (
                    self
                    .blended(target)
                    .filter(lambda blend: 0 <= index < blend.masks)
                    .to_result_with(absent)
                    .bind(lambda blend: self.masked(target, {Edit.REMOVE: -1, Edit.DUPLICATE: 1}.get(edit, 0)).map(lambda stack: (stack, layer, edit.reordered(index, blend.masks))))
                )
            case LayerAt(layer=index):
                lens: Lens = (LAYERS,)
            case EffectAt(layer=layer, entry=index):
                lens = (LAYERS, layer, EFFECTS)

        def fresh(item: object) -> Outcome[object]:
            match item:
                case StackLayer():
                    return Ok(msgspec.structs.replace(item, blend=Blend(), matte=False))
                case Entry(record=record):
                    return seeded(record).map(lambda value: Entry(record=value))
                case _:
                    return Ok(item)

        def written(items: tuple[object, ...]) -> Outcome[tuple["LayerStack[S, M]", int, Order]]:
            order = edit.reordered(index, len(items), drawn_reversed=isinstance(target, LayerAt))
            kept = accumulated(prior.map(lambda at: Ok(items[at])).default_with(lambda: fresh(items[index])) for prior in order)
            settled = max((position for position, prior in enumerate(order) if prior == Some(index)), default=index if edit is Edit.RESET else max(index - 1, 0))
            active = settled if isinstance(target, LayerAt) else target.layer
            return kept.bind(lambda changed: amended(self, lens, lambda _: changed).to_result_with(absent).map(lambda stack: (stack, active, order)))

        found = focused(self, lens).bind(lambda held: Option.of_optional(try_downcast(tuple, held))).filter(lambda items: 0 <= index < len(items))
        return found.to_result_with(absent).bind(written)

    def blended(self, target: Target) -> Option[Blend]:
        """Returns the blend a target edits, the layer's or its effect's."""
        return focused(self, _blend_lens(target)).bind(lambda held: Option.of_optional(try_downcast(Blend, held)))

    def masked(self, target: Target, change: int) -> Outcome["LayerStack[S, M]"]:
        """Changes the mask count of the targeted blend by `change`, never below zero, the last mask leaving first."""
        changed = amended(
            self,
            _blend_lens(target),
            lambda held: Option.of_optional(try_downcast(Blend, held)).map(lambda blend: msgspec.structs.replace(blend, masks=max(blend.masks + change, 0))).default_value(held),
        )
        return changed.to_result_with(partial(Fault.of, Reason.ABSENT, type(self).__name__, member=repr(target)))


class StoredLayer[S: Case, M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Structure of one layer the stack group stores under the layer key, its name, flags, and order held by its layer row."""

    source: S
    effects: tuple[Entry[M], ...] = ()
    blend: Blend = Blend()


class TermFiltering[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Filter of a recombined layer's terms before they sum, built over the layer, its outer input reader, and the working luminance group, and the passes the filter reads."""

    filter: Callable[[StackLayer[Source, M], Callable[[Key], Expr], NodeGroup], Outcome[Option[TermFilter]]] = lambda _layer, _outer, _luminance: Ok(Nothing)
    requests: Callable[[StackLayer[Source, M]], Outcome[tuple[PassRequest, ...]]] = lambda _layer: Ok(())


class Switched(bpy.types.PropertyGroup):
    """Owned stack selected by node group name, its items the extension's stacks."""

    stack: bpy.props.EnumProperty(name="Stack", description="Owned layer stack", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Copied(bpy.types.PropertyGroup):
    """One layer of another stack, its items the extension's stacks and the chosen stack's layer rows."""

    stack: bpy.props.EnumProperty(name="Stack", description="Owned layer stack", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    layer: bpy.props.EnumProperty(name="Layer", description="Layer of the chosen stack", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class NodeKind(bpy.types.PropertyGroup):
    """Source node class to create."""

    node: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Node Type",
        description="Compositor node class of the source",
        items=choices(
            (struct.identifier, struct.name, struct.description)
            for kind in (getattr(bpy.types, name) for name in dir(bpy.types))
            if isinstance(kind, type) and issubclass(kind, bpy.types.CompositorNode) and kind.is_registered_node_type()
            for struct in (bpy.types.Node.bl_rna_get_subclass(kind.__name__),)
        ),
    )


class LayerEdit(bpy.types.PropertyGroup):
    """Edit of the layer, effect, or mask a target case names by its indices."""

    action: bpy.props.EnumProperty(name="Edit", items=enum_choices(Edit))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    kind: bpy.props.EnumProperty(name="Target", description="Layer, effect, or mask edited", items=case_choices(Target))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    layer: bpy.props.IntProperty(name="Layer", description="Bottom-first layer index", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    entry: bpy.props.IntProperty(name="Effect", description="Effect index within the layer", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    mask: bpy.props.IntProperty(name="Mask", description="Mask index within the blend", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class OutputChoice(bpy.types.PropertyGroup):
    """Enabled source output chosen for one layer."""

    layer: bpy.props.IntProperty(name="Layer", description="Bottom-first layer index", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.StringProperty(name="Output", description="Source output name")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class PassSourced(bpy.types.PropertyGroup):
    """View layer output a render pass layer reads."""

    layer: bpy.props.EnumProperty(name="View Layer", description="Source view layer", items=views)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.StringProperty(name="Pass", description="Render Layers output name", default=IMAGE)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class RecombinedSourced(bpy.types.PropertyGroup):
    """View layer and the terms a recombined layer sums."""

    layer: bpy.props.EnumProperty(name="View Layer", description="Source view layer", items=views)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    recombination: bpy.props.EnumProperty(name="Sum", description="Light passes or light groups summed into beauty", items=case_choices(Recombination))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LayerRow(bpy.types.PropertyGroup):
    """List row of one layer under the layer's name, its order and flags the stack composites by."""

    key: bpy.props.StringProperty(name="Key", description="Layer key its inputs and nodes sit under", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    visible: bpy.props.BoolProperty(name="Visible", description="Composite layer", default=True, options=set())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    solo: bpy.props.BoolProperty(name="Solo", description="Composite soloed layers alone", options=set())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    matte: bpy.props.BoolProperty(name="Matte", description="Track the layer beneath as this layer's matte", options=set())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LayerRows(bpy.types.PropertyGroup):
    """Layer rows of one stack, bottom first."""

    layers: bpy.props.CollectionProperty(type=LayerRow, name="Layers", description="Layers bottom first")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class StackView(bpy.types.PropertyGroup):
    """Screen's active layer index, bottom-first layer order."""

    active: bpy.props.IntProperty(name="Active Layer", description="Bottom-first index of the edited layer", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [LENSES] ---------------------------------------------------------------------------

LAYERS: Final = member(LayerStack, lambda kind: kind.layers)
SOURCE: Final = member(StackLayer, lambda kind: kind.source)
EFFECTS: Final = member(StackLayer, lambda kind: kind.effects)
LAYER_BLEND: Final = member(StackLayer, lambda kind: kind.blend)
MATTE: Final = member(StackLayer, lambda kind: kind.matte)
ENTRY_BLEND: Final = member(Entry, lambda kind: kind.blend)
MASKS: Final = member(Blend, lambda kind: kind.masks)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _blend_lens(target: Target) -> Lens:
    """Lens reaching the blend a target edits, the layer's or its effect's."""
    match target:
        case LayerAt(layer=layer) | LayerMaskAt(layer=layer):
            return (LAYERS, layer, LAYER_BLEND)
        case EffectAt(layer=layer, entry=entry) | EffectMaskAt(layer=layer, entry=entry):
            return (LAYERS, layer, EFFECTS, entry, ENTRY_BLEND)


def stack_file(uid: int, frame: Option[int] = Nothing) -> str:
    """Returns the File Output name of a stack group's multilayer frame, `#` padding the host fills with the frame number unless a frame is given."""
    return f"{OWNERSHIP.key}_{uid}_{frame.map(lambda number: str(number).zfill(_PADDING)).default_value('#' * _PADDING)}"


def sourced(key: Key, source: RenderPass | NodeSource | Recombined, scene: bpy.types.Scene) -> Outcome[Expr]:
    """Builds the source node of the layer under `key` at default settings, keyed as the layer's source node."""
    node = item_key(key, SOURCE)
    match source:
        case RenderPass(layer=name) | Recombined(layer=name):
            return Ok(Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene), "layer": name}, label=Some(node)))
        case NodeSource(node=identifier):
            return subclass(bpy.types.Node, identifier).map(lambda kind: Expr(kind=kind, label=Some(node)))


def planned[M: Case](record: LayerStack[Source, M], binding: "LayerStacking[M]", scene: bpy.types.Scene, offered: Mapping[Key, Mapping[str, str]], uid: int) -> Outcome[NodeGroup]:
    """Builds every layer and its matte even when its composite is hidden, each layer reading the composite and coverage of the layers beneath, its nodes in its own frame, every layer and composite filed to one multilayer data frame."""
    config, shown = PyOpenColorIO.GetCurrentConfig(), frozenset(layer.key for layer, _ in record.composited())
    frame = {socket.key: group_input(socket.key) for socket in FRAME}
    aspect, scale = frame[FRAME_ASPECT.key], frame[FRAME_SCALE.key]
    ramp = Expr(kind=bpy.types.ShaderNodeTexGradient, members={"gradient_type": "LINEAR"}, socket="Color", shape=bpy.types.NodeSocketColor)
    canvas = with_alpha(ramp, 0.0, masked=True).named("canvas")

    def unit(layer: StackLayer[Source, M], below: Expr, base: Expr, luminance: NodeGroup, lobe: NodeGroup) -> Outcome[_Unit]:
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
                    return paired(found, sourced(layer_key, source, scene)).map(lambda parts: msgspec.structs.replace(parts[1], socket=parts[0], shape=bpy.types.NodeSocketColor))
                case Recombined() as source:

                    def inner(field: Key) -> Expr:
                        return group_input(item_key(layer_key, SOURCE, field))

                    return paired(binding.filtering.filter(layer, inner, luminance), sourced(layer_key, source, scene)).bind(
                        lambda parts: source.recombined(inner, parts[1], scene, sockets, binding.flags, lobe, parts[0])
                    )

        def grouped(index: int, entry: Entry[M]) -> Outcome[tuple[int, CompositorEffect[M], NodeGroup]]:
            return binding.rows.offering(type(entry.record)).bind(lambda row: row.grouped(binding.identity, scene, entry, config).map(lambda group: (index, row, group)))

        def chained(source: Expr, entries: tuple[tuple[int, CompositorEffect[M], NodeGroup], ...]) -> Outcome[Expr]:
            def step(done: Outcome[Expr], held: tuple[int, CompositorEffect[M], NodeGroup]) -> Outcome[Expr]:
                index, _, group = held
                prefix = item_key(layer_key, EFFECTS, index)
                nodes = {node: value for node, (value, _) in group.graph.placed.items() if node not in {INPUTS, OUTPUTS}}
                feeds = {node: tuple(link for link in group.graph.links if link.target.node == node) for node in (*nodes, OUTPUTS)}
                order = tuple(graphlib.TopologicalSorter({node: {link.source.node for link in feeds[node] if link.source.node != INPUTS} for node in nodes}).static_order())

                def inlined(entering: Expr) -> Outcome[Expr]:
                    def read(exprs: Mapping[Key, Expr], end: End) -> Expr:
                        match end:
                            case End(node=node, socket=socket) if node != INPUTS:
                                return msgspec.structs.replace(exprs[node], socket=socket)
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

            ordered = sorted(entries, key=lambda held: (binding.rows.rank(held[1]), held[0]))
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

        def faced(entries: tuple[tuple[int, CompositorEffect[M], NodeGroup], ...]) -> InterfacePanel:
            def panel(part: Key, face: Interface, toggle: Option[bool] = Nothing) -> InterfacePanel:
                inner = under(item_key(layer_key, part), face)
                return InterfacePanel(key=item_key(layer_key, part), name=bpy.path.display_name(part), toggle=toggle, inputs=inner.inputs, panels=inner.panels)

            def entry_panel(index: int, row: CompositorEffect[M], group: NodeGroup) -> InterfacePanel:
                prefix = item_key(layer_key, EFFECTS, index)
                inner = under(prefix, Interface(inputs=tuple(socket for socket in group.interface.inputs if socket.key not in {COLOR_INPUT.key, *frame}), panels=group.interface.panels))
                return InterfacePanel(key=prefix, name=row.title, toggle=Some(True), inputs=inner.inputs, panels=inner.panels)

            sourcing = (panel(item_key(SOURCE), layer.source.interface),) if isinstance(layer.source, Recombined) else ()
            effects = msgspec.structs.replace(panel(item_key(EFFECTS), Interface(), Some(True)), panels=tuple(itertools.starmap(entry_panel, entries)))
            matting = (panel(item_key(MATTE), masks.TRACK_MATTE),) if layer.matte else ()
            return InterfacePanel(key=layer_key, name=layer.name, panels=(*sourcing, effects, panel(_AFFINE, AFFINE), panel(item_key(LAYER_BLEND), layer.blend.interface), *matting))

        def built(source: Expr, entries: tuple[tuple[int, CompositorEffect[M], NodeGroup], ...]) -> Outcome[_Unit]:
            return chained(source, entries).map(transformed).bind(lambda content: covered(source, content)).map(lambda parts: (*parts, faced(entries)))

        return paired(entering(), accumulated(itertools.starmap(grouped, enumerate(layer.effects, 1)))).bind(lambda parts: built(*parts))

    def folded(luminance: NodeGroup, lobe: NodeGroup) -> Outcome[tuple[tuple[_Unit, ...], Expr]]:
        def step(done: Outcome[tuple[tuple[_Unit, ...], Expr, Expr]], layer: StackLayer[Source, M]) -> Outcome[tuple[tuple[_Unit, ...], Expr, Expr]]:
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

        def enframed(group: NodeGroup, owned: tuple[frozenset[Key], ...]) -> NodeGroup:
            owner = {node: layer.key for layer, nodes in reversed(tuple(zip(record.layers, owned))) for node in nodes}
            joined = group.graph.nodes
            frames = {f"{layer.key}_frame": FrameNode(members={"label": layer.name}, nodes={node: value for node, value in joined.items() if owner.get(node) == layer.key}) for layer in record.layers}
            return msgspec.structs.replace(group, graph=msgspec.structs.replace(group.graph, nodes={**{node: value for node, value in joined.items() if node not in owner}, **frames}))

        backdrop = lowered(bpy.types.CompositorNodeTree, canvas).map(lambda body: frozenset(body.nodes))
        owned = backdrop.bind(lambda shared: accumulated(lowered(bpy.types.CompositorNodeTree, *held).map(lambda body: frozenset(body.nodes) - shared - {INPUTS}) for held in roots))
        interface = Interface(inputs=(COLOR_INPUT, *FRAME), outputs=(COLOR_RESULT,), panels=tuple(held[3] for held in units))
        outputs = {COLOR_RESULT.key: composite.keeping(canvas, files, *(root for held in roots for root in held))}
        return paired(functions.built(OWNERSHIP.key, interface, outputs, bpy.types.CompositorNodeTree, name=Some(TITLE)), owned).map(lambda parts: enframed(*parts))

    return paired(masks.working(config), lobe()).bind(lambda groups: paired(folded(*groups), OutputFormat.exr(Output.DATA, layered=True)).bind(lambda parts: built(*parts[0], parts[1])))


# --- [COMPOSITION] ----------------------------------------------------------------------

VIEW: Final = Attachment(key=f"{OWNERSHIP.key}_view", form=StackView, owner=bpy.types.Screen)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AFFINE",
    "EFFECTS",
    "ENTRY_BLEND",
    "IMAGE",
    "LAYERS",
    "LAYER_BLEND",
    "MASKS",
    "MATTE",
    "OWNERSHIP",
    "PINNED",
    "SOURCE",
    "TITLE",
    "VIEW",
    "Adjustment",
    "Copied",
    "Edit",
    "EffectAt",
    "EffectMaskAt",
    "LayerAt",
    "LayerEdit",
    "LayerMaskAt",
    "LayerRow",
    "LayerRows",
    "LayerStack",
    "NodeKind",
    "NodeSource",
    "Order",
    "OutputChoice",
    "PassSourced",
    "RecombinedSourced",
    "RenderPass",
    "Source",
    "StackLayer",
    "StackView",
    "StoredLayer",
    "Switched",
    "Target",
    "TermFiltering",
    "planned",
    "sourced",
    "stack_file",
    "views",
]
