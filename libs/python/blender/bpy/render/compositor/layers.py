"""Layer stacks: layers of a source through compositor effects, placement, blend, and track matte, their pure structure edits, and the node group a stack plans over the effects an extension offers."""

from collections.abc import Callable, Iterator, Mapping
from enum import Enum
import functools
from functools import partial
import graphlib
import itertools
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
import msgspec
import PyOpenColorIO

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Struct, subclass
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.graphs import End, Expr, FileItem, FileItems, FrameNode, group_input, INPUTS, lowered, math, NodeGroup, OUTPUTS, switched, Term
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket, under
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.blending import Blend, Entry, mixed_back
from blender.bpy.render.compositor.coordinates import framed, placed, placement
from blender.bpy.render.compositor.effects import CompositorEffect, FRAME, Shell
from blender.bpy.render.compositor.entries import EffectRows
from blender.bpy.render.compositor.recombination import Recombined, TermFilter
from blender.bpy.render.view_layers.passes import filed, Filing, PassFlags, PassRequest, PassRouting, PassSet, reading
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.fields import Case
from blender.records.trees import amended, focused, Lens

# --- [TYPES] ----------------------------------------------------------------------------

type Source = Adjustment | RenderPass | NodeSource | Recombined
type Target = LayerAt | EffectAt | LayerMaskAt | EffectMaskAt


class Edit(Enum):
    """Edits of layers, effects, and masks, valued by their button icon."""

    REMOVE = "REMOVE"
    DUPLICATE = "DUPLICATE"
    UP = "TRIA_UP"
    DOWN = "TRIA_DOWN"
    RESET = "LOOP_BACK"


# --- [CONSTANTS] ------------------------------------------------------------------------

AFFINE: Final = Interface(
    inputs=(
        Socket(key="position", name="Position", kind=bpy.types.NodeSocketVector, description="Offset in frame fractions", members={"dimensions": 2, "default_value": (0.0, 0.0)}),
        Socket(key="rotation", name="Rotation", kind=bpy.types.NodeSocketFloat, description="Turn about pivot", members={"subtype": "ANGLE"}),
        Socket(key="scale", name="Scale", kind=bpy.types.NodeSocketVector, description="Signed factor per frame axis", members={"dimensions": 2, "default_value": (1.0, 1.0)}),
        Socket(key="skew", name="Skew", kind=bpy.types.NodeSocketVector, description="Horizontal and vertical shear angles", members={"dimensions": 2, "default_value": (0.0, 0.0)}),
        Socket(key="pivot", name="Pivot", kind=bpy.types.NodeSocketVector, description="Fixed point of scale, shear, and rotation", members={"dimensions": 2, "default_value": (0.5, 0.5)}),
    )
)

# --- [CHOICES] --------------------------------------------------------------------------


@functools.cache
def _node_types() -> tuple[tuple[str, str, str], ...]:
    """Enum rows of registered compositor node types, cached so the strings Blender reads stay referenced."""
    return tuple(
        (struct.identifier, struct.name, struct.description)
        for kind in (getattr(bpy.types, name) for name in dir(bpy.types))
        if isinstance(kind, type) and issubclass(kind, bpy.types.CompositorNode) and kind.is_registered_node_type()
        for struct in (bpy.types.Node.bl_rna_get_subclass(kind.__name__),)
    )


def node_types(_owner: Struct, _context: bpy.types.Context | None) -> tuple[tuple[str, str, str], ...]:
    """Enum rows of registered compositor node types."""
    return _node_types()


# --- [MODELS] ---------------------------------------------------------------------------


class Adjustment(Case, frozen=True):
    """Composite beneath through layer effects."""


class RenderPass(Case, frozen=True, kw_only=True):
    """One output of a scene view layer."""

    layer: str
    output: str = "Image"


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
        return item_key(self.key, "source")

    @property
    def plain(self) -> bool:
        """Whether the layer composites its source unmasked, unmatted, and through no effect."""
        return not self.blend.masks and not self.matte and not self.effects


class LayerStack[S: Case, M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Layers ordered bottom first over a transparent frame."""

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

    def edited(self, action: Edit, target: Target, seeded: Callable[[M], Outcome[M]]) -> Outcome[tuple["LayerStack[S, M]", int]]:
        """Applies an edit to a target's item, answering the edited stack and the layer it leaves active.

        Args:
            action: Edit applied.
            target: Layer, effect, or mask the edit applies to.
            seeded: Defaults of an effect record a reset restores.

        Returns:
            `Ok` of the edited stack and active layer index. `Error` of `absent` for a target past its collection and of seed faults.
        """
        match target, action:
            case LayerMaskAt(layer=layer) | EffectMaskAt(layer=layer), _:
                return self.masked(target, -1 if action is Edit.REMOVE else 1 if action is Edit.DUPLICATE else 0).map(lambda edited: (edited, layer))
            case LayerAt(layer=index), _:
                lens: Lens = (LAYERS,)
            case EffectAt(layer=layer, entry=index), _:
                lens = (LAYERS, layer, EFFECTS)

        def fresh(item: object) -> Outcome[object]:
            match item:
                case StackLayer():
                    return Ok(msgspec.structs.replace(item, blend=Blend(), matte=False))
                case Entry(record=record):
                    return seeded(record).map(lambda value: Entry(record=value))
                case _:
                    return Ok(item)

        def moved(items: tuple[object, ...], item: object) -> Outcome[tuple[tuple[object, ...], int]]:
            match action:
                case Edit.REMOVE:
                    return Ok(((*items[:index], *items[index + 1 :]), max(index - 1, 0)))
                case Edit.DUPLICATE:
                    return Ok(((*items[: index + 1], item, *items[index + 1 :]), index + 1))
                case Edit.UP | Edit.DOWN:
                    other = min(max(index + (1 if action is Edit.UP else -1), 0), len(items) - 1)
                    return Ok((tuple(items[other] if at == index else item if at == other else held for at, held in enumerate(items)), other))
                case _:
                    return fresh(item).map(lambda replaced: ((*items[:index], replaced, *items[index + 1 :]), index))

        def written(changed: tuple[object, ...], moved_to: int) -> Outcome[tuple["LayerStack[S, M]", int]]:
            active = moved_to if isinstance(target, LayerAt) else target.layer
            return located(amended(self, lens, lambda _: changed).to_optional(), type(self).__name__, repr(target)).map(lambda edited: (edited, active))

        match focused(self, lens).to_optional():
            case tuple(items) if 0 <= index < len(items):
                return moved(items, items[index]).bind(lambda held: written(*held))
            case _:
                return located(None, type(self).__name__, repr(target))

    def masked(self, target: Target, change: int) -> Outcome["LayerStack[S, M]"]:
        """Changes the mask count of the targeted blend by `change`, never below zero, the last mask leaving first."""

        def blended(lens: Lens) -> Outcome["LayerStack[S, M]"]:
            changed = amended(
                self, lens, lambda held: Option.of_optional(try_downcast(Blend, held)).map(lambda blend: msgspec.structs.replace(blend, masks=max(blend.masks + change, 0))).default_value(held)
            )
            return located(changed.to_optional(), type(self).__name__, repr(target))

        match target:
            case LayerAt(layer=layer) | LayerMaskAt(layer=layer):
                return blended((LAYERS, layer, StackLayer.blend))
            case EffectAt(layer=layer, entry=entry) | EffectMaskAt(layer=layer, entry=entry):
                return blended((LAYERS, layer, EFFECTS, entry, Entry.blend))


class TermFiltering[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Filter of a recombined layer's terms before they sum, built over the layer, its outer input reader, and the working luminance group, and the passes the filter reads."""

    filter: Callable[[StackLayer[Source, M], Callable[[Key], Expr], NodeGroup], Outcome[Option[TermFilter]]] = lambda _layer, _outer, _luminance: Ok(Nothing)
    requests: Callable[[StackLayer[Source, M]], Outcome[tuple[PassRequest, ...]]] = lambda _layer: Ok(())


class Planning[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Scene, offered effects, source outputs, pass flags, and lobe group of one stack graph build."""

    identity: Identity
    scene: bpy.types.Scene
    config: PyOpenColorIO.Config
    luminance: NodeGroup
    rows: EffectRows[M]
    flags: PassFlags
    lobe: NodeGroup
    offered: Mapping[Key, Mapping[str, str]]
    uid: int
    filtering: TermFiltering[M]


class Built(msgspec.Struct, frozen=True, kw_only=True):
    """Root expressions of one layer, its mix-back when composited, the node keys it owns, its interface panel, and its file items."""

    roots: tuple[Expr, ...]
    mixed: Option[Expr]
    nodes: frozenset[Key]
    panel: InterfacePanel
    items: tuple[tuple[FileItem, Expr], ...]


# --- [LENSES] ---------------------------------------------------------------------------

LAYERS: Final = LayerStack.layers  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
SOURCE: Final = StackLayer.source  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
EFFECTS: Final = StackLayer.effects  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOOKUPS]


def located[T](value: T | None, owner: str, key: str) -> Outcome[T]:
    """Answers a lookup, absent when it found nothing."""
    return Option.of_optional(value).to_result_with(partial(Fault.of, BlenderReason.ABSENT, owner, member=key))


def indexed[T](items: tuple[T, ...], index: int, owner: str) -> Outcome[T]:
    """Answers the item at a zero-based index, absent past the collection."""
    return located(items[index] if 0 <= index < len(items) else None, owner, str(index))


# --- [GRAPHS]


def sourced(key: Key, source: RenderPass | NodeSource | Recombined, scene: bpy.types.Scene) -> Outcome[Expr]:
    """Builds the source node of the layer under `key` at default settings, keyed as the layer's source node."""
    node = item_key(key, "source")
    match source:
        case RenderPass(layer=name) | Recombined(layer=name):
            return Ok(Expr(kind=bpy.types.CompositorNodeRLayers, members=(("scene", IdName.of(scene)), ("layer", name)), label=Some(node)))
        case NodeSource(node=identifier):
            return subclass(bpy.types.Node, identifier).map(lambda kind: Expr(kind=kind, label=Some(node)))


def unit[M: Case](layer: StackLayer[Source, M], below: End, base: End, planning: Planning[M], *, mixes: bool) -> Outcome[Built]:
    """Builds layer source, gated effects in rank order, affine sampling, masks, and coverage, a recombined source's terms filtered before they sum."""
    key = layer.key
    frame = {socket.key: group_input(socket.key) for socket in FRAME}
    aspect, scale = frame[item_key(Shell.frame, "aspect")], frame[item_key(Shell.frame, "scale")]

    def outer(*parts: Key | int) -> Expr:
        return group_input(item_key(key, *parts))

    def entering() -> Outcome[Expr]:
        offered = planning.offered.get(layer.node, {})
        match layer.source:
            case Adjustment():
                return Ok(Expr(kind=bpy.types.NodeReroute, inputs=(("Input", base),), socket="Output", shape="RGBA"))
            case RenderPass(output=name) | NodeSource(output=name) as source:
                return paired(located(offered.get(name), layer.node, name), sourced(key, source, planning.scene)).map(lambda parts: msgspec.structs.replace(parts[1], socket=parts[0], shape="RGBA"))
            case Recombined() as source:

                def inner(field: Key) -> Expr:
                    return outer("source", field)

                beauty = paired(planning.filtering.filter(layer, inner, planning.luminance), sourced(key, source, planning.scene)).bind(
                    lambda parts: source.recombined(inner, parts[1], planning.scene, offered, planning.flags, planning.lobe, parts[0])
                )
                return beauty.map(lambda held: held.filtered.default_value(held.plain))

    def grouped(index: int, entry: Entry[M]) -> Outcome[tuple[int, Entry[M], CompositorEffect[M], NodeGroup]]:
        return planning.rows.offering(type(entry.record)).bind(lambda row: row.grouped(planning.identity, planning.scene, entry, planning.config).map(lambda group: (index, entry, row, group)))

    def chained(source: Expr, entries: tuple[tuple[int, Entry[M], CompositorEffect[M], NodeGroup], ...]) -> Outcome[Expr]:
        def step(done: Outcome[Expr], held: tuple[int, Entry[M], CompositorEffect[M], NodeGroup]) -> Outcome[Expr]:
            index, _, _, group = held
            prefix = item_key(key, "effects", index)
            nodes = {node: value for node, (value, _) in group.graph.placed.items() if node not in {INPUTS, OUTPUTS}}
            feeds = {node: tuple(link for link in group.graph.links if link.target.node == node) for node in (*nodes, OUTPUTS)}
            order = tuple(graphlib.TopologicalSorter({node: {link.source.node for link in feeds[node] if link.source.node != INPUTS} for node in nodes}).static_order())

            def inlined(entering: Expr) -> Outcome[Expr]:
                def read(exprs: Mapping[Key, Expr], end: End) -> Expr:
                    match end:
                        case End(node=node, socket=socket) if node != INPUTS:
                            return msgspec.structs.replace(exprs[node], socket=socket)
                        case End(socket=Shell.image):
                            return entering
                        case End(socket=str(socket)) if socket in frame:
                            return frame[socket]
                        case End(socket=socket):
                            return group_input(item_key(prefix, str(socket)))

                def expressed(exprs: Mapping[Key, Expr], node: Key) -> Mapping[Key, Expr]:
                    value = nodes[node]
                    inputs = (*value.inputs.items(), *((link.target.socket, read(exprs, link.source)) for link in feeds[node]))
                    members = tuple((member.path, held) for member, held in value.members.items())
                    return {**exprs, node: Expr(kind=value.kind, inputs=inputs, members=members, content=value.content, label=Some(f"{prefix}_{node}"))}

                seed: Mapping[Key, Expr] = {}
                exprs = functools.reduce(expressed, order, seed)
                result = next((link.source for link in feeds[OUTPUTS] if link.target.socket == Shell.result), None)
                return located(result, group.name, Shell.result).map(lambda end: read(exprs, end).keeping(*exprs.values()))

            return done.bind(lambda entering: inlined(entering).map(lambda result: switched(group_input(prefix), entering, result)))

        ordered = sorted(entries, key=lambda held: (planning.rows.rank(held[2]), held[0]))
        return functools.reduce(step, ordered, Ok(source)).map(lambda chain: switched(outer("effects"), source, chain))

    def transformed(content: Expr) -> Expr:
        space = framed(content, aspect, scale)
        return placed(content, space, placement(space, *(outer("affine", socket.key) for socket in AFFINE.inputs)))

    def covered(original: Expr, content: Expr) -> Outcome[tuple[Expr, Option[Expr], tuple[tuple[FileItem, Expr], ...]]]:
        keyed = masks.coverage(layer.blend.masks, original, aspect, scale, lambda inner: outer("blend", "masks", inner), planning.config)
        matted = (
            masks.matte(planning.luminance).map(lambda group: Some(group.placed(masks.relayed(lambda inner: below if inner == Shell.image else outer("matte", inner), group.interface), masks.KEY.key)))
            if layer.matte
            else Ok(Nothing)
        )

        def weighed(factors: tuple[Expr, ...]) -> Outcome[tuple[Expr, Option[Expr], tuple[tuple[FileItem, Expr], ...]]]:
            mask = Option.of_optional(functools.reduce(lambda held, factor: held * factor, factors) if factors else None)
            weight = mask.map(lambda held: math("MULTIPLY", outer("blend", "opacity"), held)).default_value(outer("blend", "opacity"))
            coverage = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", content), ("Alpha", weight), ("Type", "Apply Mask")), socket="Image", shape="RGBA", label=Some(f"{key}_covered"))
            items = ((FileItem(name=key, kind="RGBA"), content), *((FileItem(name=f"{key} Mask", kind="FLOAT"), held) for held in mask.to_list()))
            return (mixed_back(key, base, content, outer("blend", "mode"), weight).map(Some) if mixes else Ok(Nothing)).map(lambda mixed: (coverage, mixed, items))

        return paired(keyed, matted).bind(lambda parts: weighed((*parts[0].to_list(), *parts[1].to_list())))

    def faced(entries: tuple[tuple[int, Entry[M], CompositorEffect[M], NodeGroup], ...]) -> InterfacePanel:
        def panel(name: Key, face: Interface, toggle: Option[bool] = Nothing) -> InterfacePanel:
            inner = under(item_key(key, name), face)
            return InterfacePanel(key=item_key(key, name), name=bpy.path.display_name(name), toggle=toggle, inputs=inner.inputs, panels=inner.panels)

        def entry_panel(index: int, _entry: Entry[M], row: CompositorEffect[M], group: NodeGroup) -> InterfacePanel:
            prefix = item_key(key, "effects", index)
            inner = under(prefix, Interface(inputs=tuple(socket for socket in group.interface.inputs if socket.key not in {Shell.image, *frame}), panels=group.interface.panels))
            return InterfacePanel(key=prefix, name=row.title, toggle=Some(True), inputs=inner.inputs, panels=inner.panels)

        sourcing = (panel("source", layer.source.interface),) if isinstance(layer.source, Recombined) else ()
        effects = msgspec.structs.replace(panel("effects", Interface(), Some(True)), panels=tuple(itertools.starmap(entry_panel, entries)))
        matting = (panel("matte", masks.TRACK_MATTE),) if layer.matte else ()
        return InterfacePanel(key=key, name=layer.name, panels=(*sourcing, effects, panel("affine", AFFINE), panel("blend", layer.blend.interface), *matting))

    def built(source: Expr, entries: tuple[tuple[int, Entry[M], CompositorEffect[M], NodeGroup], ...]) -> Outcome[Built]:
        def assembled(coverage: Expr, mixed: Option[Expr], items: tuple[tuple[FileItem, Expr], ...]) -> Outcome[Built]:
            roots = (coverage, *mixed.to_list())
            return lowered(bpy.types.CompositorNodeTree, *roots, *(expr for _, expr in items)).map(
                lambda body: Built(roots=roots, mixed=mixed, nodes=frozenset(body.nodes).difference((INPUTS,)), panel=faced(entries), items=items)
            )

        return chained(source, entries).map(transformed).bind(lambda content: covered(source, content)).bind(lambda parts: assembled(*parts))

    return paired(entering(), accumulated(itertools.starmap(grouped, enumerate(layer.effects, 1)))).bind(lambda parts: built(*parts))


def planned[M: Case](record: LayerStack[Source, M], planning: Planning[M], key: Key, name: str) -> Outcome[NodeGroup]:
    """Builds every layer and its matte even when its composite is hidden, each layer's nodes in its own frame, every layer and composite filed to one multilayer frame."""
    shown = frozenset(layer.key for layer, _ in record.composited())
    canvas = End(node="canvas", socket="Image")
    beneath = (canvas, *(End(node=f"{layer.key}_covered", socket="Image") for layer in record.layers))
    running = tuple(itertools.accumulate(record.layers, lambda prior, layer: End(node=f"{layer.key}_mixback", socket="image") if layer.key in shown else prior, initial=canvas))
    ramp = Expr(kind=bpy.types.ShaderNodeTexGradient, members=(("gradient_type", "LINEAR"),), socket="Color", shape="RGBA")
    background = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", ramp), ("Alpha", 0.0), ("Type", "Apply Mask")), socket="Image", shape="RGBA", label=Some(canvas.node))

    def built(units: tuple[Built, ...]) -> Outcome[NodeGroup]:
        items: tuple[tuple[FileItem, Term], ...] = (
            (FileItem(name="Composite", kind="RGBA"), running[-1]),
            *(item for held in units for item in held.items),
            *((FileItem(name=f"{layer.key} Composite", kind="RGBA"), end) for layer, end in zip(record.layers, running[1:])),
        )
        members = (
            ("directory", bpy.app.tempdir),
            ("file_name", f"{key}_{planning.uid}_####"),
            ("format.media_type", "MULTI_LAYER_IMAGE"),
            ("format.file_format", "OPEN_EXR_MULTILAYER"),
            ("format.color_depth", "32"),
        )
        files = Expr(kind=bpy.types.CompositorNodeOutputFile, inputs=tuple((item.name, term) for item, term in items), members=members, content=Some(FileItems(items=tuple(item for item, _ in items))))
        owner = {node: layer.key for layer, held in reversed(tuple(zip(record.layers, units))) for node in held.nodes}
        result = next((mixed for held in reversed(units) for mixed in held.mixed.to_list()), background)
        interface = Interface(
            inputs=(Socket(key=Shell.image, name="Image", kind=bpy.types.NodeSocketColor), *FRAME),
            outputs=(Socket(key=Shell.result, name="Image", kind=bpy.types.NodeSocketColor),),
            panels=tuple(held.panel for held in units),
        )

        def enframed(group: NodeGroup) -> NodeGroup:
            joined = group.graph.nodes
            frames = {
                f"{layer.key}_frame": FrameNode(members={PassRouting.label: layer.name}, nodes={node: value for node, value in joined.items() if owner.get(node) == layer.key})
                for layer in record.layers
            }
            return msgspec.structs.replace(group, graph=msgspec.structs.replace(group.graph, nodes={**{node: value for node, value in joined.items() if node not in owner}, **frames}))

        kept = result.keeping(background, files, *(root for held in units for root in held.roots))
        return NodeGroup.built(key=key, name=name, interface=interface, outputs={Shell.result: kept}, kind=bpy.types.CompositorNodeTree).map(enframed)

    return accumulated(unit(layer, below, base, planning, mixes=layer.key in shown) for layer, below, base in zip(record.layers, beneath, running)).bind(built)


def filing[M: Case](planning: Planning[M], view: str, request: Filing, at: Key) -> Outcome[Expr]:
    """Builds the File Output node of a filing of a recombined layer's view layer under the scene's routing, for term filters filing what they read."""
    return paired(located(planning.scene.view_layers.get(view), repr(planning.scene.view_layers), view), PassSet().bound(planning.identity)).bind(
        lambda pair: pair[1].path(planning.scene).bind(lambda path: filed(pair[1].pathed, path, view, request, at, reading(pair[1].routing.struct(pair[0])), pair[1].dimensions))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AFFINE",
    "EFFECTS",
    "LAYERS",
    "SOURCE",
    "Adjustment",
    "Built",
    "Edit",
    "EffectAt",
    "EffectMaskAt",
    "LayerAt",
    "LayerMaskAt",
    "LayerStack",
    "NodeSource",
    "Planning",
    "RenderPass",
    "Source",
    "StackLayer",
    "Target",
    "TermFiltering",
    "filing",
    "indexed",
    "located",
    "node_types",
    "planned",
    "sourced",
    "unit",
]
