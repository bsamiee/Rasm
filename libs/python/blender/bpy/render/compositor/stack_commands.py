"""Layer stack commands: each body with the form and invoke its operator takes, the forms they read, and the per-extension stack pickers."""

from collections.abc import Callable
import functools
from functools import partial, update_wrapper
from typing import Final, Protocol

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Command
from blender.bpy.commands.invocations import Open, Popup
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Invoke, Operator, Unchanged
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.mirrors import case_choices, choices, held_choices
from blender.bpy.data.rna.paths import rooted, Struct, subclass
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.interface import item_key
from blender.bpy.nodes.trees import grouped
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.edits import Choice
from blender.bpy.render.compositor.layers import Adjustment, Edit, IMAGE, LAYERS, LayerStack, NodeSource, RenderPass, SOURCE, Source, StackLayer, Target
from blender.bpy.render.compositor.recombination import Recombination, Recombined, views
from blender.bpy.render.compositor.stacking import LayerStacking, narrowed, nested_tree, OWNERSHIP, Selection, TITLE
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Items = Callable[..., tuple[tuple[str, str, str, str, int], ...]]
type Binding[M: Case] = Callable[[Identity], Outcome[LayerStacking[M]]]


class Body[T: bpy.types.PropertyGroup](Protocol):
    """Stack command body over the stacking of whichever extension calls its operator."""

    def __call__[M: Case](self, stacking: LayerStacking[M], call: Call[T], /) -> Outcome[Answer]:
        """Runs the command over the calling extension's stacking."""
        ...


class Commanding(Protocol):
    """Stack command of any form, building its operator over an extension's binding and picker forms."""

    def operator[M: Case](self, binding: Binding[M], forms: "Forms", /) -> Command:
        """Returns the operator running the command over the binding."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_TARGETS: Final = Target.__value__
_RECOMBINATIONS: Final = Recombination.__value__
_MEDIA: Final = (*sorted(bpy.path.extensions_image), *sorted(bpy.path.extensions_movie))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

# --- [CHOICES] --------------------------------------------------------------------------


@functools.cache
def _node_types() -> tuple[tuple[str, str, str], ...]:
    """Rows of registered compositor node types."""
    return tuple(
        (struct.identifier, struct.name, struct.description)
        for kind in (getattr(bpy.types, name) for name in dir(bpy.types))
        if isinstance(kind, type) and issubclass(kind, bpy.types.CompositorNode) and kind.is_registered_node_type()
        for struct in (bpy.types.Node.bl_rna_get_subclass(kind.__name__),)
    )


def node_types(_owner: Struct, _context: bpy.types.Context | None) -> tuple[tuple[str, str, str, str, int], ...]:
    """Enum items of registered compositor node types."""
    return held_choices(_node_types())


# --- [MODELS] ---------------------------------------------------------------------------


class Switched(bpy.types.PropertyGroup):
    """Owned stack selected by node group name, its items the extension's `switching` form reads."""

    stack: bpy.props.EnumProperty(name="Stack", description="Owned layer stack", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Copied(bpy.types.PropertyGroup):
    """One layer of another stack, its items the extension's `copying` form reads."""

    stack: bpy.props.EnumProperty(name="Stack", description="Owned layer stack", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    layer: bpy.props.EnumProperty(name="Layer", description="Layer of the chosen stack", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class NodeKind(bpy.types.PropertyGroup):
    """Source node class to create."""

    node: bpy.props.EnumProperty(name="Node Type", description="Compositor node class of the source", items=node_types)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LayerEdit(bpy.types.PropertyGroup):
    """Edit of the layer, effect, or mask a target case names by its indices."""

    action: bpy.props.EnumProperty(name="Edit", items=choices((edit.name, bpy.path.display_name(edit.name.lower()), "") for edit in Edit))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
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


class Forms(msgspec.Struct, frozen=True, kw_only=True):
    """Forms an extension declares over its own stacks, layer rows, and accepted effects."""

    switched: type[Switched]
    copied: type[Copied]
    choice: type[Choice]


class StackCommand[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Stack command body, keyed and described by the body, with the form its operator takes, the form's invoke, and the extension's form it takes in place of `form`."""

    run: Body[T]
    form: type[T]
    invoke: Option[Invoke[T, None]] = Nothing
    picked: Option[Callable[[Forms], type[T]]] = Nothing

    def operator[M: Case](self, binding: Binding[M], forms: Forms) -> Operator[T]:
        """Returns the operator running the body over the calling extension's binding, under the extension's form where the command picks one."""
        form: type[T] = self.picked.map(lambda pick: pick(forms)).default_value(self.form)
        return Operator[T](form=form, run=update_wrapper(partial(self.ran, binding), self.run), invoke=self.invoke)

    def ran[M: Case](self, binding: Binding[M], call: Call[T]) -> Outcome[Answer]:
        """Runs the body over the binding of the calling extension."""
        return binding(call.identity).bind(lambda stacking: self.run(stacking, call))


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [DECLARATIONS]


def command[T: bpy.types.PropertyGroup](form: type[T], invoke: Option[Invoke[T, None]] = Nothing, picked: Option[Callable[[Forms], type[T]]] = Nothing) -> Callable[[Body[T]], StackCommand[T]]:
    """Declares a stack command body with the form its operator takes, the form's invoke, and the extension's form it takes in place of `form`."""
    return lambda run: StackCommand(run=run, form=form, invoke=invoke, picked=picked)


def switching(stacks: Items) -> type[Switched]:
    """Returns the stack switch form of an extension, its stack items the extension's stacks."""

    class Switching(Switched):
        """Owned stack of the extension selected by node group name."""

        stack: bpy.props.EnumProperty(name="Stack", description="Owned layer stack", items=stacks)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    return Switching


def copying(stacks: Items, layers: Items) -> type[Copied]:
    """Returns the layer copy form of an extension, its stack items the extension's stacks and its layer items the chosen stack's layer rows."""

    class Copying(Copied):
        """One layer of another stack of the extension."""

        stack: bpy.props.EnumProperty(name="Stack", description="Owned layer stack", items=stacks)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        layer: bpy.props.EnumProperty(name="Layer", description="Layer of the chosen stack", items=layers)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    return Copying


def targeted(properties: LayerEdit) -> Outcome[Target]:
    """Decodes the target case an edit's properties name."""
    return catch(msgspec.convert, exception=msgspec.ValidationError)(properties, _TARGETS, from_attributes=True).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(properties)))


# --- [STACKS]


@command(Empty)
def new_stack[M: Case](stacking: LayerStacking[M], call: Call[Empty]) -> Outcome[Answer]:
    """Create layer stack."""
    return rooted(bpy.types.Scene, call.context).bind(lambda scene: stacking.made(scene, call.context)).map(lambda _: Done())


@command(Empty)
def duplicate_stack[M: Case](stacking: LayerStacking[M], call: Call[Empty]) -> Outcome[Answer]:
    """Copy stack with source settings."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(
            lambda scene: stacking.head(scene).bind(
                lambda held: OWNERSHIP.made(stacking.identity, lambda: narrowed(held[1].copy()), of=Some(scene)).bind(lambda copy: stacking.switched(scene, copy, call.context))
            )
        )
        .map(lambda _: Done())
    )


@command(Empty)
def remove_stack[M: Case](stacking: LayerStacking[M], call: Call[Empty]) -> Outcome[Answer]:
    """Remove stack and its effect entry."""

    def remove(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree) -> Done:
        bpy.data.node_groups.remove(tree, do_unlink=True)
        scene.compositor_effects.remove(entry)
        return Done()

    return rooted(bpy.types.Scene, call.context).bind(lambda scene: stacking.head(scene).map(lambda held: remove(scene, *held)))


@command(Switched, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), Some(lambda forms: forms.switched))
def switch_stack[M: Case](stacking: LayerStacking[M], call: Call[Switched]) -> Outcome[Answer]:
    """Edit another stack."""
    return paired(rooted(bpy.types.Scene, call.context), stacking.named(call.properties.stack)).bind(lambda parts: stacking.switched(*parts, call.context)).map(lambda _: Done())


@command(Empty)
def edit_nested_stack[M: Case](stacking: LayerStacking[M], call: Call[Empty]) -> Outcome[Answer]:
    """Edit selected layer's stack."""
    inner = stacking.selected(call.context).bind(
        lambda held: held.layer.bind(lambda layer: nested_tree(stacking.identity, held.tree, layer).to_result_with(partial(Fault.of, BlenderReason.ABSENT, TITLE, member=layer.node)))
    )
    return paired(rooted(bpy.types.Scene, call.context), inner).bind(lambda parts: stacking.switched(*parts, call.context)).map(lambda _: Done())


@command(Empty)
def clear_layers[M: Case](stacking: LayerStacking[M], call: Call[Empty]) -> Outcome[Answer]:
    """Remove every layer."""
    wrote = stacking.selected(call.context).bind(lambda held: stacking.replaced(call.context, held, msgspec.structs.replace(held.record, layers=()), 0))
    return wrote.map(lambda changed: Done() if changed else Unchanged(reason="Stack holds no layers"))


@command(Copied, Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), Some(lambda forms: forms.copied))
def copy_layer[M: Case](stacking: LayerStacking[M], call: Call[Copied]) -> Outcome[Answer]:
    """Copy one layer of another stack."""

    def chosen(group: bpy.types.NodeTree, record: LayerStack[Source, M]) -> Outcome[Answer]:
        layer = (
            Block
            .of_seq(record.layers)
            .filter(lambda held: held.key == call.properties.layer)
            .try_head()
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, group.name, member=call.properties.layer))
        )
        return layer.bind(lambda picked: stacking.inserted(call.context, (picked,), Some(group))).map(lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers"))

    return stacking.named(call.properties.stack).bind(lambda group: stacking.read(group).bind(lambda record: chosen(group, record)))


@command(Switched, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), Some(lambda forms: forms.switched))
def copy_all_layers[M: Case](stacking: LayerStacking[M], call: Call[Switched]) -> Outcome[Answer]:
    """Copy layers of another stack with source settings."""
    return stacking.named(call.properties.stack).bind(
        lambda group: stacking.read(group).bind(
            lambda record: stacking.inserted(call.context, record.layers, Some(group)).map(lambda wrote: Done() if wrote else Unchanged(reason=f"{group.name} holds no layers"))
        )
    )


# --- [SOURCES]


@command(Empty)
def add_adjustment_layer[M: Case](stacking: LayerStacking[M], call: Call[Empty]) -> Outcome[Answer]:
    """Add layer processing composite beneath."""
    return stacking.inserted(call.context, (StackLayer(key="", name=bpy.path.display_name(item_key(Adjustment)), source=Adjustment()),)).map(lambda _: Done())


@command(PassSourced, Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))))
def add_render_pass_layer[M: Case](stacking: LayerStacking[M], call: Call[PassSourced]) -> Outcome[Answer]:
    """Add view layer output."""
    source = RenderPass(layer=call.properties.layer, output=call.properties.output)
    return stacking.inserted(call.context, (StackLayer(key="", name=f"{source.layer} {source.output}", source=source),)).map(lambda _: Done())


@command(RecombinedSourced, Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))))
def add_recombined_layer[M: Case](stacking: LayerStacking[M], call: Call[RecombinedSourced]) -> Outcome[Answer]:
    """Add beauty from view layer light."""
    layer, picked = call.properties.layer, call.properties.recombination
    recombination = catch(msgspec.convert, exception=msgspec.ValidationError)({"kind": picked}, _RECOMBINATIONS).map_error(partial(Fault.of, BlenderReason.REFUSED, picked))
    return recombination.bind(lambda chosen: stacking.inserted(call.context, (StackLayer(key="", name=layer, source=Recombined(layer=layer, recombination=chosen)),))).map(lambda _: Done())


@command(NodeKind, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))))
def add_node_layer[M: Case](stacking: LayerStacking[M], call: Call[NodeKind]) -> Outcome[Answer]:
    """Add source node at first enabled output."""
    return subclass(bpy.types.Node, call.properties.node).bind(lambda kind: stacking.added_node(call.context, kind, Ok)).map(lambda _: Done())


@command(Switched, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), Some(lambda forms: forms.switched))
def add_nested_stack[M: Case](stacking: LayerStacking[M], call: Call[Switched]) -> Outcome[Answer]:
    """Add stack as source."""
    return stacking.named(call.properties.stack).bind(lambda group: stacking.added_node(call.context, bpy.types.CompositorNodeGroup, lambda node: grouped(node, group))).map(lambda _: Done())


@command(Empty, Some(Open(extensions=lambda _: _MEDIA)))
def add_media[M: Case](stacking: LayerStacking[M], call: Call[Empty]) -> Outcome[Answer]:
    """Add image or movie files."""
    images = accumulated(loaded(str(path), Tag()) for path in call.paths)
    return images.bind(lambda found: accumulated(stacking.added_node(call.context, bpy.types.CompositorNodeImage, functools.partial(written, path=("image",), value=image)) for image in found)).map(
        lambda _: Done()
    )


@command(OutputChoice)
def select_output[M: Case](stacking: LayerStacking[M], call: Call[OutputChoice]) -> Outcome[Answer]:
    """Set layer source output."""
    index, output = call.properties.layer, call.properties.output

    def chosen(held: Selection[M], layer: StackLayer[Source, M]) -> Outcome[bool]:
        match layer.source:
            case RenderPass() | NodeSource() as source:
                return stacking.rewritten(call.context, held, (LAYERS, index, SOURCE), lambda _: msgspec.structs.replace(source, output=output), index)
            case _:
                return Ok(False)

    wrote = stacking.selected(call.context).bind(lambda held: held.record.layer(index).bind(lambda layer: chosen(held, layer)))
    return wrote.map(lambda changed: Done() if changed else Unchanged(reason=f"Layer reads {output}"))


# --- [EDITS]


@command(LayerEdit)
def add_mask[M: Case](stacking: LayerStacking[M], call: Call[LayerEdit]) -> Outcome[Answer]:
    """Add mask to layer or effect blend."""
    return (
        paired(targeted(call.properties), stacking.selected(call.context))
        .bind(lambda parts: parts[1].record.masked(parts[0], 1).bind(lambda record: stacking.replaced(call.context, parts[1], record, parts[0].layer)))
        .map(lambda _: Done())
    )


@command(LayerEdit)
def edit_layer[M: Case](stacking: LayerStacking[M], call: Call[LayerEdit]) -> Outcome[Answer]:
    """Remove, duplicate, move, or reset a layer, effect, or mask, its input values following it."""
    edit = Edit[call.properties.action]
    return (
        targeted(call.properties)
        .bind(lambda target: stacking.edited(call.context, target, edit))
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{bpy.path.display_name(edit.name.lower())} changes nothing"))
    )


@command(Choice, picked=Some(lambda forms: forms.choice))
def add_effect_to_layer[M: Case](stacking: LayerStacking[M], call: Call[Choice]) -> Outcome[Answer]:
    """Add the chosen effect to the selected layer."""
    row = Option.of_optional(stacking.rows.keyed.get(call.properties.kind)).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(tuple(stacking.rows.keyed)), member=call.properties.kind))
    return (
        paired(row.bind(lambda found: rooted(bpy.types.Scene, call.context).bind(found.seeded)), stacking.selected(call.context))
        .bind(lambda parts: stacking.appended(call.context, parts[1], Entry(record=parts[0])))
        .map(lambda _: Done())
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

STACKING: Final[tuple[Commanding, ...]] = (duplicate_stack, switch_stack, edit_nested_stack, copy_layer, copy_all_layers, clear_layers, remove_stack)
SOURCING: Final[tuple[Commanding, ...]] = (add_adjustment_layer, add_render_pass_layer, add_recombined_layer, add_node_layer, add_nested_stack, add_media)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "SOURCING",
    "STACKING",
    "Binding",
    "Body",
    "Commanding",
    "Copied",
    "Forms",
    "Items",
    "LayerEdit",
    "NodeKind",
    "OutputChoice",
    "PassSourced",
    "RecombinedSourced",
    "StackCommand",
    "Switched",
    "add_adjustment_layer",
    "add_effect_to_layer",
    "add_mask",
    "add_media",
    "add_nested_stack",
    "add_node_layer",
    "add_recombined_layer",
    "add_render_pass_layer",
    "clear_layers",
    "command",
    "copy_all_layers",
    "copy_layer",
    "copying",
    "duplicate_stack",
    "edit_layer",
    "edit_nested_stack",
    "new_stack",
    "node_types",
    "remove_stack",
    "select_output",
    "switch_stack",
    "switching",
    "targeted",
]
