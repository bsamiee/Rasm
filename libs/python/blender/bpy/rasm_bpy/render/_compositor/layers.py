"""Layer stacks: layers of a source through compositor effects, placement, blend, and track matte, their pure structure edits and stored parts, the term filter a recombined layer reads, and the host forms of stack commands, layer rows, the screen selection, and the scene look pick."""

from collections.abc import Callable, Iterator
from enum import Enum
from functools import partial
import itertools
from typing import Final, Generic, Self, TypeVar

import bpy
from expression import Option, Some
from expression.collections import Block
import msgspec

from rasm_bpy.data._extension.identity import Key
from rasm_bpy.data._ids.ownership import Ownership, Saved
from rasm_bpy.data._rna.paths import Struct
from rasm_bpy.data._rna.properties import case_choices, choices, enum_choices, EnumItem
from rasm_bpy.nodes._desired import item_key
from rasm_bpy.nodes._graphs import Expr, group_input
from rasm_bpy.nodes._interfaces import Interface, Socket
from rasm_bpy.nodes.changes import NodeGroup
from rasm_bpy.render._compositor.effects import CompositorRecord, Entry
from rasm_bpy.render._compositor.entries import Pinned, StackEnd
from rasm_bpy.render._compositor.masks import Blend
from rasm_bpy.render._compositor.recombination import Recombination, Recombined, TermFilter
from rasm_bpy.render._view_layers.passes import _used
from rasm_bpy.settings._attachments import Attachment
from rasm_records import accumulated, amended, Case, Fault, focused, Lens, Nothing, Ok, Outcome, Reason, Toward

# --- [TYPES] ----------------------------------------------------------------------------

_M_co = TypeVar("_M_co", bound=CompositorRecord, default=CompositorRecord, covariant=True)

type Source = Adjustment | RenderPass | NodeSource | Recombined
type StackTarget = LayerAt | EffectAt | LayerMaskAt | EffectMaskAt
type Order = tuple[Option[int], ...]


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
        Socket(key="position", kind=bpy.types.NodeSocketVector2D, description="Offset in frame fractions", default_value=(0.0, 0.0)),
        Socket(key="rotation", kind=bpy.types.NodeSocketFloatAngle, description="Turn about pivot"),
        Socket(key="scale", kind=bpy.types.NodeSocketVector2D, description="Signed factor per frame axis", default_value=(1.0, 1.0)),
        Socket(key="skew", kind=bpy.types.NodeSocketVector2D, description="Horizontal and vertical shear angles", default_value=(0.0, 0.0)),
        Socket(key="pivot", kind=bpy.types.NodeSocketVector2D, description="Fixed point of scale, shear, and rotation", default_value=(0.5, 0.5)),
    )
)
_IMAGE: Final = "Image"
_COMPOSITE_PART: Final = "Composite"
_OWNERSHIP: Final = Ownership(kind=bpy.types.CompositorNodeTree, key="layer_stack", saved=Saved.ALWAYS, single=False)
_TITLE: Final = bpy.path.display_name(_OWNERSHIP.key)
PINNED: Final = Pinned(ownership=_OWNERSHIP, edge=StackEnd.HEAD)
_PADDING: Final = 4

# --- [CHOICES] --------------------------------------------------------------------------


def _views(_owner: Struct, context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
    """Used view layer items of the context scene."""
    return choices((layer.name, layer.name, "") for scene in Option.of_optional(context).bind(lambda held: Option.of_optional(held.scene)).to_list() for layer in _used(scene))


# --- [MODELS] ---------------------------------------------------------------------------


class Adjustment(Case, frozen=True):
    """Composite beneath through layer effects."""


class RenderPass(Case, frozen=True, kw_only=True):
    """One output of a scene view layer."""

    layer: str
    output: str = _IMAGE


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


class StackLayer(msgspec.Struct, Generic[_M_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Source through compositor effects and placement, blended over the layers beneath, its values held by the stack group's inputs."""

    key: str
    name: str
    source: Source
    effects: tuple[Entry[_M_co], ...] = ()
    blend: Blend = Blend()
    matte: bool = False
    visible: bool = True
    solo: bool = False

    @property
    def node(self) -> Key:
        """Key of the layer's owned source node."""
        return item_key(self.key, SOURCE)

    @property
    def places(self) -> tuple[Key, ...]:
        """Item key prefix of each effect's inputs in order, effects counted from one."""
        return tuple(item_key(self.key, EFFECTS, index) for index in range(1, len(self.effects) + 1))

    @property
    def mask_part(self) -> str:
        """Multilayer frame part holding the layer's mask coverage."""
        return f"{self.key} Mask"

    @property
    def composite_part(self) -> str:
        """Multilayer frame part holding the composite through the layer."""
        return f"{self.key} {_COMPOSITE_PART}"

    @property
    def plain(self) -> bool:
        """Whether the layer composites its source unmasked, unmatted, and through no effect."""
        return not self.blend.masks and not self.matte and not self.effects


class LayerStack(msgspec.Struct, Generic[_M_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Layers ordered bottom first over a transparent frame, the list drawing them top first."""

    layers: tuple[StackLayer[_M_co], ...] = ()

    def composited(self) -> tuple[tuple[StackLayer[_M_co], Option[StackLayer[_M_co]]], ...]:
        """Selects visible layers under solo, leaving out layers a matte above consumes.

        Returns:
            Layers in stack order, each with the layer beneath, absent at the bottom.
        """
        soloed = any(layer.solo for layer in self.layers)
        beneath: tuple[Option[StackLayer[_M_co]], ...] = (Nothing, *map(Some, self.layers))
        consumed = (*(layer.matte for layer in self.layers[1:]), False)
        return tuple((layer, below) for layer, below, matte in zip(self.layers, beneath, consumed) if layer.visible and not matte and (layer.solo or not soloed))

    def fresh(self) -> Iterator[Key]:
        """Layer keys the stack holds no layer under, in order."""
        taken = frozenset(layer.key for layer in self.layers)
        return (key for index in itertools.count(1) for key in (f"layer_{index}",) if key not in taken)

    def layer(self, index: int) -> Outcome[StackLayer[_M_co]]:
        """Returns the layer at a bottom-first index, absent past the stack."""
        return Option.of_optional(self.layers[index] if 0 <= index < len(self.layers) else None).to_result_with(partial(Fault.of, Reason.ABSENT, type(self).__name__, member=str(index)))

    def edited(self, edit: Edit, target: StackTarget, seeded: Callable[[CompositorRecord], Outcome[CompositorRecord]]) -> Outcome[tuple["LayerStack", int, Order]]:
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
                lens: Lens = (_LAYERS,)
            case EffectAt(layer=layer, entry=index):
                lens = (_LAYERS, layer, EFFECTS)

        def fresh(item: object) -> Outcome[object]:
            match item:
                case StackLayer():
                    return Ok(msgspec.structs.replace(item, blend=Blend(), matte=False))
                case Entry(record=record):
                    return seeded(record).map(lambda value: Entry(record=value))
                case _:
                    return Ok(item)

        def written(items: tuple[object, ...]) -> Outcome[tuple[LayerStack, int, Order]]:
            order = edit.reordered(index, len(items), drawn_reversed=isinstance(target, LayerAt))
            kept = accumulated(prior.map(lambda at: Ok(items[at])).default_with(lambda: fresh(items[index])) for prior in order)
            moved = max((position for position, prior in enumerate(order) if prior == Some(index)), default=index if edit is Edit.RESET else max(index - 1, 0))
            active = moved if isinstance(target, LayerAt) else target.layer
            return kept.bind(lambda changed: amended(tuple, self, lens, lambda _: changed).to_result_with(absent).map(lambda stack: (stack, active, order)))

        found = focused(tuple, self, lens).filter(lambda items: 0 <= index < len(items))
        return found.to_result_with(absent).bind(written)

    def blended(self, target: StackTarget) -> Option[Blend]:
        """Returns the blend a target edits, the layer's or its effect's."""
        return focused(Blend, self, _blend_lens(target))

    def masked(self, target: StackTarget, change: int) -> Outcome[Self]:
        """Changes the mask count of the targeted blend by `change`, never below zero, the last mask leaving first."""
        changed = amended(Blend, self, _blend_lens(target), lambda blend: msgspec.structs.replace(blend, masks=max(blend.masks + change, 0)))
        return changed.to_result_with(partial(Fault.of, Reason.ABSENT, type(self).__name__, member=repr(target)))


class StoredLayer(msgspec.Struct, Generic[_M_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Structure of one layer the stack group stores under the layer key, its name, flags, and order held by its layer row."""

    source: Source
    effects: tuple[Entry[_M_co], ...] = ()
    blend: Blend = Blend()


class TermFiltering(msgspec.Struct, frozen=True, kw_only=True):
    """Term filter a recombined layer builds from its first effect of `record`, reading that effect's inputs and requested passes."""

    record: type[CompositorRecord]
    filter: Callable[[Recombined, Callable[[Key], Expr], NodeGroup], Outcome[TermFilter]]

    def matched(self, layer: StackLayer) -> Option[int]:
        """One-based place of the first effect of the configured record type."""
        return Block.of_seq(enumerate(layer.effects, 1)).filter(lambda pair: isinstance(pair[1].record, self.record)).try_head().map(lambda pair: pair[0])

    def built(self, layer: StackLayer, source: Recombined, luminance: NodeGroup) -> Outcome[Option[TermFilter]]:
        """Builds the filter over the layer's matched entry, reading its inputs under the entry's place, `Nothing` while the layer holds none."""
        return self.matched(layer).map(lambda index: self.filter(source, lambda key: group_input(item_key(layer.key, EFFECTS, index, key)), luminance).map(Some)).default_value(Ok(Nothing))


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
            for struct in (kind.bl_rna,)
        ),
    )


class LayerEdit(bpy.types.PropertyGroup):
    """Edit of the layer, effect, or mask a target case names by its indices."""

    action: bpy.props.EnumProperty(name="Edit", items=enum_choices(Edit))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    kind: bpy.props.EnumProperty(name="Target", description="Layer, effect, or mask edited", items=case_choices(StackTarget))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    layer: bpy.props.IntProperty(name="Layer", description="Bottom-first layer index", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    entry: bpy.props.IntProperty(name="Effect", description="Effect index within the layer", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    mask: bpy.props.IntProperty(name="Mask", description="Mask index within the blend", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class OutputChoice(bpy.types.PropertyGroup):
    """Enabled source output chosen for one layer."""

    layer: bpy.props.IntProperty(name="Layer", description="Bottom-first layer index", min=0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.StringProperty(name="Output", description="Source output name")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class PassSourced(bpy.types.PropertyGroup):
    """View layer output a render pass layer reads."""

    layer: bpy.props.EnumProperty(name="View Layer", description="Source view layer", items=_views)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.StringProperty(name="Pass", description="Render Layers output name", default=_IMAGE)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class RecombinedSourced(bpy.types.PropertyGroup):
    """View layer and the terms a recombined layer sums."""

    layer: bpy.props.EnumProperty(name="View Layer", description="Source view layer", items=_views)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
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


class StackLook(bpy.types.PropertyGroup):
    """Stack preset the scene's head stack takes, its effects mixed by Strength, its items the stack presets."""

    look: bpy.props.EnumProperty(name="Stack Look", description="Stack preset the head stack takes", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    strength: bpy.props.IntProperty(name="Strength", description="Share each preset effect mixes over its input, in percent", subtype="PERCENTAGE", min=0, max=200, default=100)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [LENSES] ---------------------------------------------------------------------------

_LAYERS: Final = LayerStack.layers  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
SOURCE: Final = StackLayer.source
EFFECTS: Final = StackLayer.effects  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
LAYER_BLEND: Final = StackLayer.blend
MATTE: Final = StackLayer.matte
ENTRY_BLEND: Final = Entry.blend
MASKS: Final = Blend.masks

# --- [OPERATIONS] -----------------------------------------------------------------------


def _blend_lens(target: StackTarget) -> Lens:
    """Lens reaching the blend a target edits, the layer's or its effect's."""
    match target:
        case LayerAt(layer=layer) | LayerMaskAt(layer=layer):
            return (_LAYERS, layer, LAYER_BLEND)
        case EffectAt(layer=layer, entry=entry) | EffectMaskAt(layer=layer, entry=entry):
            return (_LAYERS, layer, EFFECTS, entry, ENTRY_BLEND)


def _stack_file(uid: int, frame: Option[int] = Nothing) -> str:
    """Returns the File Output name of a stack group's multilayer frame, `#` padding the host fills with the frame number unless a frame is given."""
    return f"{_OWNERSHIP.key}_{uid}_{frame.map(lambda number: str(number).zfill(_PADDING)).default_value('#' * _PADDING)}"


# --- [COMPOSITION] ----------------------------------------------------------------------

_VIEW: Final = Attachment(key=f"{_OWNERSHIP.key}_view", form=StackView, owner=bpy.types.Screen)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AFFINE",
    "EFFECTS",
    "ENTRY_BLEND",
    "LAYER_BLEND",
    "MASKS",
    "MATTE",
    "PINNED",
    "SOURCE",
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
    "StackLook",
    "StackTarget",
    "StackView",
    "StoredLayer",
    "Switched",
    "TermFiltering",
]
