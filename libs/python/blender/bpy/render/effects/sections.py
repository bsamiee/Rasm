"""Active compositor effect section beside the native scene effect stack."""

from collections.abc import Callable, Mapping
from typing import Annotated, get_origin

import bpy
from expression import Nothing, Ok, Option, Result, Some
import gpu
import msgspec
from msgspec import inspect
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, InvokePopup, Operator, Outcome, Own, Unchanged
from blender.bpy.data.rna.conditions import AllOf, check, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderResult
from blender.bpy.nodes.changes import owned
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket
from blender.bpy.render.color.luts import Enabled
from blender.bpy.render.effects.stack import (
    active,
    composites,
    Configured,
    EffectStack,
    entry_interface,
    expanded,
    formation,
    graphed,
    KeyPath,
    Named,
    Placeable,
    reached,
    rows,
    Shell,
    StackEntry,
    viewed,
)
from blender.bpy.render.effects.viewers import EffectViewer, ViewTarget
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import acquired, alerted, joined, Line, Member, operated, OperatorButton, placed, realized, Row, RowContent, RowDraw, split
from blender.bpy.ui.layout.sections import fields, Single
from blender.imaging.chain import Domain, Effect, Stage
from blender.imaging.color.authoring import dynamic_range_of, DynamicRange
from blender.imaging.color.processors import DisplayView
from blender.imaging.compositing.blending import Blend, Entry
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import GraphForm, Knot, markers, Ramp

# --- [CHOICES] --------------------------------------------------------------------------

spaces = Choices(lambda _identity, _owner, _context: ((space.getName(), space.getName(), space.getDescription()) for space in PyOpenColorIO.GetCurrentConfig().getColorSpaces()))


# --- [MODELS] ---------------------------------------------------------------------------


class SpaceField(Named, frozen=True, kw_only=True):
    """Entry graph field and OCIO colorspace."""

    field: str
    space: Annotated[str, spaces] = ""


class EffectButtons(Line, frozen=True, kw_only=True):
    """Entry header actions the native effect header lacks."""

    stack: EffectStack
    locks: Option[Callable[[Option[tuple[Key | int, ...]]], RowContent]] = Nothing

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Returns header draw with command records from each entry."""
        operators = self.stack.operators

        def shown(layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
            if isinstance(holder, bpy.types.SceneCompositorEffect) and isinstance(scene := holder.id_data, bpy.types.Scene):
                row = next((held.row for held in self.stack.held(identity, scene) if held.entry == holder), Nothing)
                actions = (
                    *(locks(Nothing) for locks in self.locks.to_list() if row.is_some()),
                    *(
                        OperatorButton(target=Own(command=command, values=command.form(name=holder.name)), text=Some(""))
                        for owned in row.to_list()
                        for command in owned.registrables
                        if isinstance(command, Operator) and issubclass(command.form, Named)
                    ),
                    OperatorButton(target=Own(command=operators.resetting, values=Named(name=holder.name)), text=Some(""), icon=Some("LOOP_BACK")),
                )
                alerted(layout, placed(actions, identity).map(lambda draws: joined(draws)(layout, holder, context)))

        return Ok(shown)


class EffectBody(Line, frozen=True, kw_only=True):
    """Entry graph templates, attachments, notices, setup locks, and Viewer buttons the native effect body lacks."""

    viewer: EffectViewer
    colorspace: Operator[SpaceField]
    locks: Option[Callable[[Option[tuple[Key | int, ...]]], RowContent]] = Nothing

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns viewer, colorspace command, and lock rows."""
        return Ok((self.viewer, self.colorspace, *acquired(tuple(locks(Nothing) for locks in self.locks.to_list()))))

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Returns entry body draw over the record and owned nodes."""
        stack = self.viewer.stack
        record_key = next(field.name for field in msgspec.structs.fields(Configured) if get_origin(field.type) is Entry)
        entry_fields = msgspec.structs.fields(Entry)
        enables = tuple((enabled, field) for field in msgspec.structs.fields(Configured) for enabled in markers(field.type, Enabled))

        def button[T: msgspec.Struct](layout: bpy.types.UILayout, command: Operator[T], record: T, text: str) -> None:
            context = layout.operator_context
            layout.operator_context = "INVOKE_REGION_WIN" if command.invoked else "EXEC_REGION_WIN"
            try:
                alerted(layout, command.bound(identity).bind(lambda pair: operated(layout.operator(pair[0], text=text), pair[0]).bind(lambda properties: pair[1].write(properties, record))))
            finally:
                layout.operator_context = context

        def locked(layout: bpy.types.UILayout, entry: bpy.types.SceneCompositorEffect, path: tuple[Key | int, ...], context: bpy.types.Context) -> BlenderResult[None]:
            return accumulated(realized(locks(Some(path)), identity).map(lambda draw: draw(layout, entry, context)) for locks in self.locks.to_list()).map(lambda _: None)

        def listed(
            layout: bpy.types.UILayout, entry: bpy.types.SceneCompositorEffect, face: Interface, hidden: frozenset[Key], paths: Mapping[Key, tuple[Key | int, ...]], context: bpy.types.Context
        ) -> BlenderResult[None]:
            concealed = set(hidden)
            lockable: list[tuple[str, tuple[Key | int, ...]]] = [(enabled.descriptor.prop.name, (field.name,)) for enabled, field in enables]
            for key, item, parent, _ in face.walked():
                if key in concealed or parent.filter(concealed.__contains__).is_some():
                    concealed.add(key)
                    continue
                match item:
                    case InterfacePanel(name=name) if key in paths:
                        lockable.append((name, paths[key]))
                    case (Socket(name=name, members=members), "INPUT") if (
                        key in paths and key != Shell.image and not any(descriptor.prop.identifier == "hide_in_modifier" and value for descriptor, value in members.items())
                    ):
                        lockable.append((name, paths[key]))
            header, body = layout.panel(f"{entry.name}_locks", default_closed=True)
            header.label(text="Setup Locks")
            answers: list[BlenderResult[None]] = []
            for inner in Option.of_optional(body).to_list():
                for name, path in lockable:
                    line = inner.row(align=True)
                    line.label(text=name)
                    answers.append(locked(line, entry, path, context))
            return accumulated(answers).map(lambda _: None)

        def offered_drawn(
            layout: bpy.types.UILayout, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree, row: Placeable, record: Entry[Effect], context: bpy.types.Context
        ) -> BlenderResult[None]:
            values = tuple((value, (Shell.blend,) if isinstance(value, Blend) else (), field.name) for field in entry_fields for value in (getattr(record, field.name),))
            parameters = tuple((value, prefix, field, path) for value, prefix, field in values for path, _, _ in rows(type(value), value=Some(value)))
            paths = {
                item_key(*prefix, *path[:depth]): (record_key, field, *(part for part in path[:depth] if not isinstance(part, type)))
                for _, prefix, field, path in parameters
                for depth in range(1, len(path) + 1)
            }
            paths[Shell.blend] = (record_key, next(field for value, _, field in values if isinstance(value, Blend)))
            hidden = frozenset(item_key(*prefix, *path) for value, prefix, _, path in parameters if not active(value, path))
            nodes = owned(group, identity)

            def widget(node: bpy.types.Node, key: Key, title: str, shape: inspect.Type) -> BlenderResult[None]:
                inner = layout.column()
                inner.use_property_split = False
                header = inner.row(align=True)
                header.label(text=title)
                answer = locked(header, entry, paths[key], context)
                match shape:
                    case inspect.VarTupleType(item_type=inspect.StructType(cls=kind)) if (get_origin(kind) or kind) is Knot:
                        inner.template_curve_mapping(node, "mapping", type="NONE")
                    case inspect.StructType(cls=kind) if kind is Ramp:
                        inner.template_color_ramp(node, "color_ramp", expand=True)
                    case inspect.VarTupleType():
                        pass
                    case _:
                        inner.template_node_inputs(node)
                return answer

            def graph_field(value: msgspec.Struct, path: KeyPath, key: Key, field: msgspec.structs.FieldInfo, shape: inspect.Type) -> tuple[BlenderResult[None], ...]:
                title = next((meta.title for meta in markers(field.type, msgspec.Meta) if meta.title is not None), field.name.replace("_", " ").title())
                match reached(value, path):
                    case str(space) if GraphForm.COLOR_SPACE in markers(field.type, GraphForm):
                        line = layout.row(align=True)
                        button(line, self.colorspace, SpaceField(name=entry.name, field=key, space=space), title)
                        return (locked(line, entry, paths[key], context),)
                    case _:
                        return tuple(widget(node, key, title, shape) for node in Option.of_optional(nodes.get(key)).to_list())

            def graph_fields() -> BlenderResult[None]:
                return accumulated(
                    answer
                    for value, prefix, _ in values
                    for path, (field, shape) in graphed(type(value), value=Some(value)).items()
                    for key in (item_key(*prefix, *path),)
                    if key not in hidden
                    for answer in graph_field(value, path, key, field, shape)
                ).map(lambda _: None)

            def attached() -> BlenderResult[None]:
                def drawn[T: msgspec.Struct](attachment: Attachment[T]) -> BlenderResult[None]:
                    def shown(held: Attached[T]) -> BlenderResult[None]:
                        holder = held.struct(scene)
                        return (
                            fields(held.group, identity)
                            .bind(lambda built: placed(built[0], identity).map(lambda draws: (tuple(draw(layout, holder, context) for draw in draws), built[1](layout, holder, context, entry.name))))
                            .map(lambda _: None)
                        )

                    return attachment.bound(identity).bind(shown)

                return accumulated(drawn(attachment) for attachment in row.registrables if isinstance(attachment, Attachment) and attachment.owner is bpy.types.Scene).map(lambda _: None)

            return (
                graph_fields()
                .bind(lambda _: attached())
                .bind(
                    lambda _: accumulated(
                        entry_interface(row.record, len(record.blend.masks)).bind(lambda face: expanded(face, record.effect)).bind(lambda face: listed(layout, entry, face, hidden, paths, context))
                        for _ in self.locks.to_list()
                    )
                )
                .map(lambda _: None)
            )

        def notices(
            layout: bpy.types.UILayout, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, row: Placeable, record: Entry[Effect], context: bpy.types.Context
        ) -> BlenderResult[None]:
            native = tuple(scene.compositor_effects)
            formed = stack.entries(identity, scene, DisplayView)
            if row.record.stage.domain is Domain.DISPLAY and (not formed or native.index(entry) < native.index(formed[0])):
                layout.label(text="Needs earlier Display View entry", icon="STATUS_ERROR_FILLED")

            def shown(config: PyOpenColorIO.Config, view: DisplayView) -> BlenderResult[None]:
                if row.record.stage is Stage.TONE:
                    return (
                        DisplayView
                        .for_encoding(config, view.display, Some(view.view))
                        .map_error(BlenderImagingFault.lifted)
                        .map(lambda encoded: layout.label(text="Needs encoding-only Display View", icon="STATUS_ERROR_FILLED") if encoded.is_none() else None)
                    )
                if (
                    isinstance(record.effect, DisplayView)
                    and any(dynamic_range_of(space) is DynamicRange.HIGH for space in record.effect.colorspace(config).to_list())
                    and not any(window.support_hdr_color for window in Option.of_optional(context.window).to_list())
                ):
                    split(layout)[1].label(text="OpenGL backend cannot show HDR" if gpu.platform.backend_type_get() == "OPENGL" else "Window cannot show HDR", icon="STATUS_INFO_FILLED")
                return Ok(None)

            return paired(stack.config(), formation.current(identity, scene)).bind(lambda parts: shown(parts[0], parts[1].default_value(viewed(scene))))

        def shown(layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
            match holder:
                case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, node_group=bpy.types.NodeTree() as group) as entry:
                    row = next((held.row for held in stack.held(identity, scene) if held.entry == entry), Nothing)
                    for offered in row.to_list():
                        alerted(
                            layout,
                            offered.read(identity, entry).bind(
                                lambda record: offered_drawn(layout, scene, entry, group, offered, record, context).bind(lambda _: notices(layout, scene, entry, offered, record, context))
                            ),
                        )
                    alerted(
                        layout,
                        accumulated(
                            gates.bind(lambda held: accumulated(check(condition, context, Some(entry)) for condition in held.values()))
                            for pinned in stack.pinned
                            if row.is_none() and pinned.ownership.marked(identity, group).is_some()
                            for gates in pinned.conditions.map(lambda conditions: conditions(identity, entry)).to_list()
                        ),
                    )
                    buttons = layout.row(align=True)
                    button(buttons, self.viewer.viewing, ViewTarget(name=entry.name, node=EffectViewer.entering.node, socket=EffectViewer.entering.socket), "Input")
                    button(buttons, self.viewer.viewing, ViewTarget(name=entry.name, node=EffectViewer.leaving.node, socket=EffectViewer.leaving.socket), "Result")
                    button(buttons, self.viewer.clearing, Named(name=entry.name), "Clear")
                case _:
                    pass

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------


def effect_section(viewer: EffectViewer, *, thumbnail: Row | RowDraw, locks: Option[Callable[[Option[tuple[Key | int, ...]]], RowContent]] = Nothing) -> Single:
    """Builds active effect section with its colorspace command and viewer."""

    def changed(call: Call[SpaceField]) -> BlenderResult[Outcome]:
        def wrote(scene: bpy.types.Scene, held: StackEntry) -> BlenderResult[Outcome]:
            return (
                Result[Placeable, BlenderFaults]
                .bind(
                    held.row.to_result_with(lambda: (BlenderAbsentFault(owner=repr(held.entry), key=call.record.field),)),
                    lambda row: row.read(call.identity, held.entry, graphs={call.record.field: call.record.space}).bind(lambda record: row.write(call.identity, scene, held.entry, record)),
                )
                .map(lambda changed: Done() if changed else Unchanged(reason=f"{held.entry.name} reads {call.record.space}"))
            )

        return call.root(bpy.types.Scene).bind(lambda scene: viewer.stack.found(call.identity, scene, call.record.name).bind(lambda held: wrote(scene, held)))

    colorspace = Operator(
        key="effect_colorspace",
        title="Choose Color Space",
        description="Set graph field color space",
        form=SpaceField,
        run=changed,
        invoke=Some(InvokePopup.SEARCH),
        primary=Some(next(field for field in msgspec.structs.fields(SpaceField) if spaces in markers(field.type, Choices))),
        undo=True,
    )
    return Single(
        key="active_effect",
        title="Active Effect",
        holder=Some(Member(root=bpy.types.Scene, path="compositor_effects.active")),
        condition=Some(composites),
        rows=(thumbnail, EffectBody(viewer=viewer, colorspace=colorspace, locks=locks)),
        header=(EffectButtons(stack=viewer.stack, locks=locks),),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

COMPOSITING = AllOf(
    conditions=(
        Setting(descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, owner="", prop=bpy.types.SpaceNodeEditor.bl_rna.properties["tree_type"]), accepted=(bpy.types.CompositorNodeTree.__name__,)),
        Setting(descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, owner="", prop=bpy.types.SpaceNodeEditor.bl_rna.properties["node_tree_sub_type"]), accepted=("SCENE",)),
    )
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COMPOSITING", "EffectBody", "EffectButtons", "SpaceField", "effect_section", "spaces"]
