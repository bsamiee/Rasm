"""Layer stacks: layers of a source through compositor effects, placement, blend, and track matte, their pure edits, and the node group a stack plans over the effects an extension offers."""

from collections.abc import Callable, Iterator, Mapping
from enum import Enum
import functools
import graphlib
import itertools
from typing import Annotated, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import subclass
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import End, Expr, FileItem, FileItems, FrameNode, group_input, INPUTS, lowered, math, NodeGroup, OUTPUTS, switched, Term
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket, under
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.blending import Blend, Entry, mixed_back
from blender.bpy.render.compositor.coordinates import framed, placed, placement
from blender.bpy.render.compositor.effects import CameraFrame, CompositorEffect, expanded, hidden, Shell
from blender.bpy.render.compositor.entries import EffectRows
from blender.bpy.render.compositor.inputs import KeyPath, reached, updated
from blender.bpy.render.compositor.recombination import Recombined, TermFilter, views
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.view_layers.passes import filed, Filing, PassRequest, PassRouting
from blender.bpy.settings.properties import Choices
from blender.imaging.color.processors import basis, Domain
from blender.imaging.geometry.regions import Position
from blender.records.faults import accumulated, paired
from blender.records.fields import blended, Case, Finite, Gated, heading, Presentation, Quantity

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


# --- [CHOICES] --------------------------------------------------------------------------

node_types = Choices(
    lambda _identity, _owner, _context: (
        (struct.identifier, struct.name, struct.description)
        for kind in (getattr(bpy.types, name) for name in dir(bpy.types))
        if isinstance(kind, type) and issubclass(kind, bpy.types.CompositorNode) and kind.is_registered_node_type()
        for struct in (bpy.types.Node.bl_rna_get_subclass(kind.__name__),)
    )
)

# --- [MODELS] ---------------------------------------------------------------------------


class Adjustment(Case, frozen=True):
    """Composite beneath through layer effects."""


class RenderPass(Case, frozen=True, kw_only=True):
    """One output of a scene view layer."""

    title = "Render Pass"
    layer: Annotated[str, msgspec.Meta(title="View Layer"), views]
    output: Annotated[str, msgspec.Meta(title="Pass")] = "Image"


class NodeSource(Case, frozen=True, kw_only=True):
    """Owned compositor source node with editable settings."""

    title = "Node"
    node: Annotated[str, msgspec.Meta(title="Node Type"), node_types]
    output: Annotated[str, msgspec.Meta(title="Output")]


class Affine(msgspec.Struct, frozen=True, kw_only=True):
    """Scale, shear, and rotation about pivot, then translation."""

    position: Annotated[Position, msgspec.Meta(description="Offset in frame fractions")] = (0.0, 0.0)
    rotation: Annotated[Finite, msgspec.Meta(description="Turn about pivot"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    scale: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Signed factor per frame axis")] = (1.0, 1.0)
    skew: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Horizontal and vertical shear angles"), Presentation(quantity=Some(Quantity.ANGLE))] = (0.0, 0.0)
    pivot: Annotated[Position, msgspec.Meta(description="Fixed point of scale, shear, and rotation")] = (0.5, 0.5)


class LayerAt(Case, frozen=True, kw_only=True):
    """Edit of one layer."""

    title = "Layer"
    layer: Annotated[int, msgspec.Meta(ge=0)]


class EffectAt(Case, frozen=True, kw_only=True):
    """Edit of one effect within a layer."""

    title = "Effect"
    layer: Annotated[int, msgspec.Meta(ge=0)]
    entry: Annotated[int, msgspec.Meta(ge=0)]


class LayerMaskAt(Case, frozen=True, kw_only=True):
    """Edit of one layer mask."""

    title = "Layer Mask"
    layer: Annotated[int, msgspec.Meta(ge=0)]
    mask: Annotated[int, msgspec.Meta(ge=0)]


class EffectMaskAt(Case, frozen=True, kw_only=True):
    """Edit of one effect's blend mask."""

    title = "Effect Mask"
    layer: Annotated[int, msgspec.Meta(ge=0)]
    entry: Annotated[int, msgspec.Meta(ge=0)]
    mask: Annotated[int, msgspec.Meta(ge=0)]


class Layer[S: Case, M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Source through compositor effects and placement, blended over the layers beneath."""

    key: Annotated[str, msgspec.Meta(description="Identity within stack")]
    name: Annotated[str, msgspec.Meta(description="Name in lists and documents")]
    source: S
    effects: Annotated[Gated[tuple[Gated[Entry[M]], ...]], msgspec.Meta(description="Source effects")] = Gated(enabled=True, value=())
    affine: Annotated[Affine, msgspec.Meta(title="Transform", description="Content placement")] = Affine()
    blend: Annotated[Blend, msgspec.Meta(description="Mix over the layers beneath")] = Blend()
    matte: Annotated[Gated[masks.TrackMatte], msgspec.Meta(title="Track Matte", description="Coverage read from the layer beneath")] = Gated(enabled=False, value=masks.TrackMatte())
    visible: Annotated[bool, msgspec.Meta(description="Composite layer")] = True
    solo: Annotated[bool, msgspec.Meta(description="Composite soloed layers alone")] = False

    @property
    def parts(self) -> tuple[tuple[Key, msgspec.Struct], ...]:
        """Source, placement, blend, and matte records under their field names."""
        return (("source", self.source), ("affine", self.affine), ("blend", self.blend), ("matte", self.matte.value))

    @property
    def node(self) -> Key:
        """Key of the layer's owned source node."""
        return item_key(self.key, "source")

    @property
    def plain(self) -> bool:
        """Whether the layer composites its source unplaced, unmasked, unmatted, and through no enabled effect."""
        return self.affine == Affine() and not self.blend.masks and not self.matte.enabled and not (self.effects.enabled and any(gated.enabled for gated in self.effects.value))


class LayerStack[S: Case, M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Layers ordered bottom first over a transparent frame."""

    layers: Annotated[tuple[Layer[S, M], ...], msgspec.Meta(description="Layers bottom first")] = ()

    def composited(self) -> tuple[tuple[Layer[S, M], Option[Layer[S, M]]], ...]:
        """Selects visible layers under solo, leaving out layers a matte above consumes.

        Returns:
            Layers in stack order, each with the layer beneath, absent at the bottom.
        """
        soloed = any(layer.solo for layer in self.layers)
        beneath: tuple[Option[Layer[S, M]], ...] = (Nothing, *map(Some, self.layers))
        consumed = (*(layer.matte.enabled for layer in self.layers[1:]), False)
        return tuple((layer, below) for layer, below, matte in zip(self.layers, beneath, consumed) if layer.visible and not matte and (layer.solo or not soloed))

    def fresh(self) -> Iterator[Key]:
        """Layer keys the stack holds no layer under, in order."""
        taken = frozenset(layer.key for layer in self.layers)
        return (key for index in itertools.count(1) for key in (f"layer_{index}",) if key not in taken)

    def edited(self, action: Edit, target: Target, seeded: Callable[[M], BlenderResult[M]]) -> BlenderResult[tuple[Self, int]]:
        """Applies an edit to a target's item, answering the edited stack and the layer it leaves active.

        Args:
            action: Edit applied.
            target: Layer, effect, or mask the edit applies to.
            seeded: Defaults of an effect record a reset restores.

        Returns:
            `Ok` of the edited stack and active layer index. `Error` of `absent` for a target past its collection and of seed faults.
        """
        match target:
            case LayerAt(layer=index):
                path: KeyPath = ("layers",)
            case EffectAt(layer=layer, entry=index):
                path = ("layers", layer + 1, "effects", "value")
            case LayerMaskAt(mask=index) | EffectMaskAt(mask=index):
                path = masking(target)

        def fresh(item: object) -> BlenderResult[object]:
            match item:
                case Layer():
                    return Ok(msgspec.structs.replace(item, affine=Affine(), blend=Blend(), matte=Gated(enabled=False, value=masks.TrackMatte())))
                case Gated(value=Entry(record=record)):
                    return seeded(record).map(lambda value: msgspec.structs.replace(item, value=Entry(record=value)))
                case _:
                    return Ok(masks.Mask())

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

        edit = collection(self, path).bind(lambda items: indexed(items, index, item_key(*path)).bind(lambda item: moved(items, item)))
        return edit.map(lambda held: (updated(self, path, lambda _: held[0]), held[1] if isinstance(target, LayerAt) else target.layer))

    def strengthened(self, amount: float, config: PyOpenColorIO.Config) -> BlenderResult["LayerStack[S, M]"]:
        """Moves every layer effect from its record defaults toward its stored values by `amount`, 1 keeping the stack."""

        def layered(matrix: npt.NDArray[np.float64]) -> "LayerStack[S, M]":
            def entry(gated: Gated[Entry[M]]) -> Gated[Entry[M]]:
                record = gated.value.record
                return msgspec.structs.replace(gated, value=msgspec.structs.replace(gated.value, record=blended(type(record)(), record, amount, matrix)))

            def layer(held: Layer[S, M]) -> Layer[S, M]:
                return msgspec.structs.replace(held, effects=msgspec.structs.replace(held.effects, value=tuple(map(entry, held.effects.value))))

            return msgspec.structs.replace(self, layers=tuple(map(layer, self.layers)))

        return basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR).map(layered)

    def parts(self, rows: EffectRows[M]) -> BlenderResult[tuple["Valued", ...]]:
        """Layer source, placement, blend, and matte, then each effect and its blend in the domain of its row, the records entry inputs hold in layer order."""

        def effected(index: int, layer: Layer[S, M], number: int, gated: Gated[Entry[M]]) -> BlenderResult[tuple[Valued, Valued]]:
            at: KeyPath = ("layers", index, "effects", "value", number, "value")
            keys: KeyPath = (layer.key, "effects", number)
            return rows.offering(type(gated.value.record)).map(
                lambda row: (
                    Valued(keys=keys, at=(*at, "record"), value=gated.value.record, domain=row.domain),
                    Valued(keys=(*keys, Shell.blend), at=(*at, Shell.blend), value=gated.value.blend, domain=row.domain),
                )
            )

        placed = tuple(
            Valued(keys=(layer.key, name), at=("layers", index, name, *(("value",) if isinstance(getattr(layer, name), Gated) else ())), value=value)
            for index, layer in enumerate(self.layers, 1)
            for name, value in layer.parts
        )
        entries = accumulated(effected(index, layer, number, gated) for index, layer in enumerate(self.layers, 1) for number, gated in enumerate(layer.effects.value, 1))
        return entries.map(lambda held: (*placed, *(part for pair in held for part in pair)))

    def gates(self) -> tuple[tuple[KeyPath, KeyPath, bool], ...]:
        """Entry key path, record path, and value of each layer's effects gate and each effect's gate."""
        return tuple(
            gate
            for index, layer in enumerate(self.layers, 1)
            for gate in (
                ((layer.key, "effects"), ("layers", index, "effects", "enabled"), layer.effects.enabled),
                *(((layer.key, "effects", number), ("layers", index, "effects", "value", number, "enabled"), gated.enabled) for number, gated in enumerate(layer.effects.value, 1)),
            )
        )


class Valued(msgspec.Struct, frozen=True, kw_only=True):
    """One record the entry's inputs hold: its entry key path, stack record path, value, and the domain its graph values take."""

    keys: KeyPath
    at: KeyPath
    value: msgspec.Struct
    domain: Domain = Domain.SCENE_LINEAR


class TermFiltering[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Filter of a recombined layer's terms before they sum, built over the layer, its outer input reader, and the working luminance group, and the passes the filter reads."""

    filter: Callable[[Layer[Source, M], Callable[[Key], Expr], NodeGroup], BlenderResult[Option[TermFilter]]] = lambda _layer, _outer, _luminance: Ok(Nothing)
    requests: Callable[[Layer[Source, M]], BlenderResult[tuple[PassRequest, ...]]] = lambda _layer: Ok(())


class Planning[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Scene, offered effects, source outputs, and routing of one stack graph build."""

    identity: Identity
    scene: bpy.types.Scene
    config: PyOpenColorIO.Config
    luminance: NodeGroup
    rows: EffectRows[M]
    routed: PassRouting
    offered: Mapping[Key, Mapping[str, str]]
    path: OutputPath
    uid: int
    filtering: TermFiltering[M]


class Built(msgspec.Struct, frozen=True, kw_only=True):
    """Root expressions of one layer, its mix-back when composited, the node keys it owns, its interface panel, and its file items."""

    roots: tuple[Expr, ...]
    mixed: Option[Expr]
    nodes: frozenset[Key]
    panel: InterfacePanel
    items: tuple[tuple[FileItem, Expr], ...]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOOKUPS]


def located[T](value: T | None, owner: str, key: str) -> BlenderResult[T]:
    """Answers a lookup, absent when it found nothing."""
    return Option.of_optional(value).to_result_with(lambda: (BlenderAbsentFault(owner=owner, key=key),))


def indexed[T](items: tuple[T, ...], index: int, owner: str) -> BlenderResult[T]:
    """Answers the item at a zero-based index, absent past the collection."""
    return located(items[index] if index < len(items) else None, owner, str(index))


def collection(record: msgspec.Struct, path: KeyPath) -> BlenderResult[tuple[object, ...]]:
    """Answers the collection at a record path, absent where an index on the way reaches past its collection."""
    match catch(lambda: reached(record, path), exception=IndexError)():
        case Result(tag="ok", ok=tuple(items)):
            return Ok(items)
        case _:
            return Error((BlenderAbsentFault(owner=type(record).__name__, key=item_key(*path)),))


def masking(target: Target) -> KeyPath:
    """Record path of the blend masks of the layer or effect a target names."""
    match target:
        case LayerAt(layer=layer) | LayerMaskAt(layer=layer):
            return ("layers", layer + 1, "blend", "masks")
        case EffectAt(layer=layer, entry=entry) | EffectMaskAt(layer=layer, entry=entry):
            return ("layers", layer + 1, "effects", "value", entry + 1, "value", "blend", "masks")


# --- [GRAPHS]


def sourced(key: Key, source: RenderPass | NodeSource | Recombined, scene: bpy.types.Scene) -> BlenderResult[Expr]:
    """Builds the source node of the layer under `key` at default settings, keyed as the layer's source node."""
    node = item_key(key, "source")
    match source:
        case RenderPass(layer=name) | Recombined(layer=name):
            return Ok(Expr(kind=bpy.types.CompositorNodeRLayers, members=(("scene", IdName.of(scene)), ("layer", name)), label=Some(node)))
        case NodeSource(node=identifier):
            return subclass(bpy.types.Node, identifier).map(lambda kind: Expr(kind=kind, label=Some(node)))


def unit[M: Case](layer: Layer[Source, M], below: End, base: End, planning: Planning[M], *, mixes: bool) -> BlenderResult[Built]:
    """Builds layer source, gated effects in rank order, affine sampling, masks, and coverage, a recombined source's terms filtered before they sum."""
    key = layer.key
    frame = {name: group_input(name) for field in msgspec.structs.fields(CameraFrame) for name in (item_key(Shell.frame, field.name),)}
    aspect, scale = frame[item_key(Shell.frame, "aspect")], frame[item_key(Shell.frame, "scale")]

    def outer(*parts: Key | int) -> Expr:
        return group_input(item_key(key, *parts))

    def entering() -> BlenderResult[Expr]:
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
                    lambda parts: source.recombined(inner, parts[1], planning.scene, offered, planning.routed.lobe, parts[0])
                )
                return beauty.map(lambda held: held.filtered.default_value(held.plain))

    def grouped(index: int, gated: Gated[Entry[M]]) -> BlenderResult[tuple[int, Gated[Entry[M]], CompositorEffect[M], NodeGroup]]:
        return planning.rows.offering(type(gated.value.record)).bind(
            lambda row: row.grouped(planning.identity, planning.scene, gated.value, planning.config).map(lambda group: (index, gated, row, group))
        )

    def chained(source: Expr, entries: tuple[tuple[int, Gated[Entry[M]], CompositorEffect[M], NodeGroup], ...]) -> BlenderResult[Expr]:
        def step(done: BlenderResult[Expr], held: tuple[int, Gated[Entry[M]], CompositorEffect[M], NodeGroup]) -> BlenderResult[Expr]:
            index, _, _, group = held
            prefix = item_key(key, "effects", index)
            nodes = {node: value for node, (value, _) in group.graph.placed.items() if node not in {INPUTS, OUTPUTS}}
            feeds = {node: tuple(link for link in group.graph.links if link.target.node == node) for node in (*nodes, OUTPUTS)}
            order = tuple(graphlib.TopologicalSorter({node: {link.source.node for link in feeds[node] if link.source.node != INPUTS} for node in nodes}).static_order())

            def inlined(entering: Expr) -> BlenderResult[Expr]:
                def read(exprs: Mapping[Key, Expr], end: End) -> Expr:
                    match end:
                        case End(node=node, socket=socket) if node != INPUTS:
                            return msgspec.structs.replace(exprs[node], socket=socket)
                        case End(socket=Shell.image):
                            return entering
                        case End(socket=socket) if socket in frame:
                            return frame[socket]
                        case End(socket=socket):
                            return group_input(item_key(prefix, socket))

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
        return placed(content, space, placement(space, *(outer("affine", field.name) for field in msgspec.structs.fields(Affine))))

    def covered(original: Expr, content: Expr) -> BlenderResult[tuple[Expr, Option[Expr], tuple[tuple[FileItem, Expr], ...]]]:
        keyed = masks.coverage(len(layer.blend.masks), original, aspect, scale, lambda inner: outer("blend", "masks", inner), planning.config)
        matted = (
            masks.matte(planning.luminance).map(lambda group: Some(group.placed(masks.relayed(lambda inner: below if inner == Shell.image else outer("matte", inner), group.interface), masks.KEY.key)))
            if layer.matte.enabled
            else Ok(Nothing)
        )

        def weighed(factors: tuple[Expr, ...]) -> BlenderResult[tuple[Expr, Option[Expr], tuple[tuple[FileItem, Expr], ...]]]:
            mask = Option.of_optional(functools.reduce(lambda held, factor: held * factor, factors) if factors else None)
            weight = next((math("MULTIPLY", outer("blend", "opacity"), held) for held in mask.to_list()), outer("blend", "opacity"))
            coverage = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", content), ("Alpha", weight), ("Type", "Apply Mask")), socket="Image", shape="RGBA", label=Some(f"{key}_covered"))
            items = ((FileItem(name=key, kind="RGBA"), content), *((FileItem(name=f"{key} Mask", kind="FLOAT"), held) for held in mask.to_list()))
            return (mixed_back(key, base, content, outer("blend", "mode"), weight).map(Some) if mixes else Ok(Nothing)).map(lambda mixed: (coverage, mixed, items))

        return paired(keyed, matted).bind(lambda parts: weighed((*parts[0].to_list(), *parts[1].to_list())))

    def faced(entries: tuple[tuple[int, Gated[Entry[M]], CompositorEffect[M], NodeGroup], ...]) -> BlenderResult[InterfacePanel]:
        def panel(name: Key, face: Interface, toggle: Option[bool] = Nothing) -> InterfacePanel:
            inner = under(item_key(key, name), face)
            return InterfacePanel(key=item_key(key, name), name=heading(name), toggle=toggle, inputs=inner.inputs, panels=inner.panels)

        def entry_panel(index: int, gated: Gated[Entry[M]], row: CompositorEffect[M], group: NodeGroup) -> InterfacePanel:
            prefix = item_key(key, "effects", index)
            inner = under(prefix, Interface(inputs=tuple(socket for socket in group.interface.inputs if socket.key not in {Shell.image, *frame}), panels=group.interface.panels))
            return InterfacePanel(key=prefix, name=row.record.title, toggle=Some(gated.enabled), inputs=inner.inputs, panels=inner.panels)

        def assembled(source: Interface, affine: Interface, blend: Interface, matte: Interface) -> InterfacePanel:
            effects = msgspec.structs.replace(panel("effects", Interface(), Some(layer.effects.enabled)), panels=tuple(itertools.starmap(entry_panel, entries)))
            return InterfacePanel(key=key, name=layer.name, panels=(panel("source", source), effects, panel("affine", affine), panel("blend", blend), panel("matte", matte)))

        source = Interface.of(type(layer.source)).bind(lambda face: expanded(face, layer.source))
        return paired(source, Interface.of(Affine), Interface.of(Blend).bind(lambda face: expanded(face, layer.blend)), Interface.of(masks.TrackMatte)).map(lambda faces: assembled(*faces))

    def built(source: Expr, entries: tuple[tuple[int, Gated[Entry[M]], CompositorEffect[M], NodeGroup], ...]) -> BlenderResult[Built]:
        def assembled(coverage: Expr, mixed: Option[Expr], items: tuple[tuple[FileItem, Expr], ...], panel: InterfacePanel) -> BlenderResult[Built]:
            roots = (coverage, *mixed.to_list())
            return lowered(*roots, *(expr for _, expr in items)).map(lambda body: Built(roots=roots, mixed=mixed, nodes=frozenset(body.nodes).difference((INPUTS,)), panel=panel, items=items))

        return paired(chained(source, entries).map(transformed).bind(lambda content: covered(source, content)), faced(entries)).bind(lambda parts: assembled(*parts[0], parts[1]))

    return paired(entering(), accumulated(itertools.starmap(grouped, enumerate(layer.effects.value, 1)))).bind(lambda parts: built(*parts))


def planned[M: Case](record: LayerStack[Source, M], planning: Planning[M], key: Key, name: str, digits: int) -> BlenderResult[NodeGroup]:
    """Builds every layer and its matte even when its composite is hidden, each layer's nodes in its own frame, every layer and composite filed to one multilayer frame."""
    shown = frozenset(layer.key for layer, _ in record.composited())
    canvas = End(node="canvas", socket="Image")
    beneath = (canvas, *(End(node=f"{layer.key}_covered", socket="Image") for layer in record.layers))
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
            ("file_name", f"{key}_{planning.uid}_{'#' * digits}"),
            ("format.media_type", "MULTI_LAYER_IMAGE"),
            ("format.file_format", "OPEN_EXR_MULTILAYER"),
            ("format.color_depth", "32"),
        )
        files = Expr(kind=bpy.types.CompositorNodeOutputFile, inputs=tuple((item.name, term) for item, term in items), members=members, content=Some(FileItems(items=tuple(item for item, _ in items))))
        owner = {node: layer.key for layer, held in reversed(tuple(zip(record.layers, units))) for node in held.nodes}
        result = next((mixed for held in reversed(units) for mixed in held.mixed.to_list()), background)
        interface = msgspec.structs.replace(
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
            return msgspec.structs.replace(group, graph=msgspec.structs.replace(group.graph, nodes={**{node: value for node, value in joined.items() if node not in owner}, **frames}))

        kept = result.keeping(background, files, *(root for held in units for root in held.roots))
        return NodeGroup.built(key=key, name=name, interface=interface, outputs={Shell.result: kept}).map(enframed)

    units = accumulated(unit(layer, below, base, planning, mixes=layer.key in shown) for layer, below, base in zip(record.layers, beneath, running))
    return paired(units, Interface.of(CameraFrame).map(lambda face: hidden(under(Shell.frame, face)))).bind(lambda parts: built(*parts))


def filing[M: Case](planning: Planning[M], view: str, request: Filing, at: Key) -> BlenderResult[Expr]:
    """Builds the File Output node of a filing of a recombined layer's view layer under the scene's routing, for term filters filing what they read."""
    routed = planning.routed
    return (
        located(planning.scene.view_layers.get(view), repr(planning.scene.view_layers), view)
        .bind(routed.routing.read)
        .bind(lambda routing: filed(routed.pathed, planning.path, view, request, at, routing.convention, routed.dimensions))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Adjustment",
    "Affine",
    "Built",
    "Edit",
    "EffectAt",
    "EffectMaskAt",
    "Layer",
    "LayerAt",
    "LayerMaskAt",
    "LayerStack",
    "NodeSource",
    "Planning",
    "RenderPass",
    "Source",
    "Target",
    "TermFiltering",
    "Valued",
    "collection",
    "filing",
    "indexed",
    "located",
    "masking",
    "node_types",
    "planned",
    "sourced",
    "unit",
]
