"""Native sidebar sections for compositor entries, graph fields, and inspection actions."""

from collections.abc import Callable, Mapping
from typing import Annotated, get_origin

import bpy
from expression import Nothing, Ok, Option, Result, Some
import gpu
import msgspec
from msgspec import inspect
import PyOpenColorIO

from rasm.blender.commands.operators import Call, Done, InvokePopup, Operator, Own
from rasm.blender.data.ids.ownership import Mark
from rasm.blender.data.rna.conditions import AllOf, check, Condition, Setting
from rasm.blender.data.rna.descriptors import Descriptor, struct_at
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderResult
from rasm.blender.nodes.graphs import addresses, Interface, InterfacePanel, item_key, Socket
from rasm.blender.render.color.luts import Enabled
from rasm.blender.render.effects.stack import (
    composites,
    Configured,
    EffectStack,
    entry_input,
    entry_interface,
    expanded,
    formation,
    graphed,
    Named,
    Placeable,
    reached,
    rows,
    Shell,
    Shift,
    StackEntry,
    Toward,
    viewed,
)
from rasm.blender.render.effects.viewers import EffectViewer, ViewTarget
from rasm.blender.settings.attachments import Attached, Attachment
from rasm.blender.settings.properties import Choices
from rasm.blender.ui.layout.rows import acquired, alerted, joined, Line, Member, operated, OperatorButton, placed, Property, realized, Row, RowContent, RowDraw, split
from rasm.blender.ui.layout.sections import Each, fields
from rasm.imaging.chain import Domain, Effect, Stage
from rasm.imaging.color.authoring import DynamicRange
from rasm.imaging.color.processors import DisplayView
from rasm.imaging.compositing.blending import Blend, Entry
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import GraphForm, Knot, markers, Ramp

# --- [CHOICES] --------------------------------------------------------------------------

spaces = Choices(lambda _identity, _owner, _context: ((space.getName(), space.getName(), space.getDescription()) for space in PyOpenColorIO.GetCurrentConfig().getColorSpaces()))


# --- [MODELS] ---------------------------------------------------------------------------


class SpaceField(Named, frozen=True, kw_only=True):
    """Entry graph field and its selected OCIO colorspace."""

    field: str
    space: Annotated[str, spaces] = ""


class EffectButtons(Line, frozen=True, kw_only=True):
    """Entry movement, reset, binding actions, and removal in native header order."""

    stack: EffectStack
    locks: Option[Callable[[Option[tuple[Key | int, ...]]], RowContent]] = Nothing

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Binds commands once and fills their real records from each supplied entry."""
        operators = self.stack.operators
        admissions = tuple((enabled, field) for field in msgspec.structs.fields(Configured) for enabled in markers(field.type, Enabled))

        def shown(layout: bpy.types.UILayout, holder: bpy.types.bpy_struct, context: bpy.types.Context) -> None:  # type: ignore[type-arg]  # ty: ignore[missing-type-argument]
            if isinstance(holder, bpy.types.SceneCompositorEffect):
                row = next((held.row for held in self.stack.held(identity, holder.id_data) if held.entry == holder), Nothing)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                actions = (
                    *(
                        Property(
                            target=Member(root=bpy.types.SceneCompositorEffect, path=enabled.descriptor.prop.identifier),
                            text=Some(""),
                            icon=Some("RESTRICT_VIEW_OFF" if enabled is Enabled.PREVIEW else "RESTRICT_RENDER_OFF"),
                            icon_only=True,
                            action=self.locks.filter(lambda _: row.is_some()).map(lambda action: action(Some((field.name,)))),
                        )
                        for enabled, field in admissions
                    ),
                    *(locks(Nothing) for locks in self.locks.to_list() if row.is_some()),
                    OperatorButton(target=Own(command=operators.moving, values=Shift(name=holder.name, toward=Toward.UP)), text=Some(""), icon=Some("TRIA_UP")),
                    OperatorButton(target=Own(command=operators.moving, values=Shift(name=holder.name, toward=Toward.DOWN)), text=Some(""), icon=Some("TRIA_DOWN")),
                    OperatorButton(target=Own(command=operators.resetting, values=Named(name=holder.name)), text=Some(""), icon=Some("LOOP_BACK")),
                    *(
                        OperatorButton(target=Own(command=command, values=command.form(name=holder.name)), text=Some(""))
                        for owned in row.to_list()
                        for command in owned.registrables
                        if isinstance(command, Operator) and issubclass(command.form, Named)
                    ),
                    OperatorButton(target=Own(command=operators.removing, values=Named(name=holder.name)), text=Some(""), icon=Some("X")),
                )
                alerted(layout, placed(actions, identity).map(lambda draws: joined(draws)(layout, holder, context)))

        return Ok(shown)


class EffectBody(Line, frozen=True, kw_only=True):
    """Native inputs, attached records, graph templates, and viewer actions of an entry."""

    viewer: EffectViewer
    colorspace: Operator[SpaceField]
    locks: Option[Callable[[Option[tuple[Key | int, ...]]], RowContent]] = Nothing

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Retains the actual shared viewer and colorspace declarations."""
        return Ok((self.viewer, self.colorspace, *acquired(tuple(locks(Nothing) for locks in self.locks.to_list()))))

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Realizes entry drawing against native input and marked-node owners."""
        stack = self.viewer.stack
        record_key = next(field.name for field in msgspec.structs.fields(Configured) if get_origin(field.type) is Entry)
        entry_fields = msgspec.structs.fields(Entry)

        def button[T: msgspec.Struct](layout: bpy.types.UILayout, command: Operator[T], record: T, text: str) -> None:
            context = layout.operator_context
            layout.operator_context = "INVOKE_REGION_WIN" if command.invoked else "EXEC_REGION_WIN"
            try:
                alerted(layout, command.bound(identity).bind(lambda pair: operated(layout.operator(pair[0], text=text), pair[0]).bind(lambda properties: pair[1].write(properties, record))))
            finally:
                layout.operator_context = context

        def locked(layout: bpy.types.UILayout, entry: bpy.types.SceneCompositorEffect, path: tuple[Key | int, ...], context: bpy.types.Context) -> BlenderResult[None]:
            return accumulated(realized(locks(Some(path)), identity).map(lambda draw: draw(layout, entry, context)) for locks in self.locks.to_list()).map(lambda _: None)

        def inputs(
            layout: bpy.types.UILayout, entry: bpy.types.SceneCompositorEffect, face: Interface, hidden: frozenset[Key], paths: Mapping[Key, tuple[Key | int, ...]], context: bpy.types.Context
        ) -> BlenderResult[None]:
            bodies: dict[Option[Key], Option[bpy.types.UILayout]] = {Nothing: Some(layout)}
            answers: list[BlenderResult[None]] = []
            for key, item, parent, _ in face.walked():
                host = bodies[parent] if key not in hidden else Nothing
                match item:
                    case InterfacePanel(name=name, closed=closed, toggle=toggle):
                        bodies[Some(key)] = Nothing
                        for owner in host.to_list():
                            header, body = owner.panel(f"{entry.name}_{key}", default_closed=closed)
                            bodies[Some(key)] = Option.of_optional(body)
                            if toggle.is_none():
                                header.label(text=name)
                            else:

                                def toggled(value: bpy.types.bpy_struct) -> None:  # type: ignore[type-arg]  # ty: ignore[missing-type-argument]
                                    header.prop(value, "value", text=name)
                                    for inner in Option.of_optional(body).to_list():
                                        inner.active = value.value  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

                                answers.append(entry_input(identity, entry, key).map(toggled))
                            if key in paths:
                                answers.append(locked(header, entry, paths[key], context))
                    case (Socket(name=name, members=members), "INPUT") if key != Shell.image and not any(
                        descriptor.prop.identifier == "hide_in_modifier" and value for descriptor, value in members.items()
                    ):
                        for owner in host.to_list():
                            line = owner.row(align=True)
                            answers.append(entry_input(identity, entry, key).map(lambda value: line.prop(value, "value", text=name)).map(lambda _: None))
                            if key in paths:
                                answers.append(locked(line, entry, paths[key], context))
            return accumulated(answers).map(lambda _: None)

        def native(layout: bpy.types.UILayout, entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree, gates: dict[str, Condition], context: bpy.types.Context) -> None:
            bodies: dict[int, Option[bpy.types.UILayout]] = {}
            first = next((item for item in group.interface.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket) and item.in_out == "INPUT"), None)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            for item in group.interface.items_tree:  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                host = Option.of_optional(item.parent).map(lambda panel: bodies[panel.as_pointer()]).default_value(Some(layout))
                match item:
                    case bpy.types.NodeTreeInterfacePanel():
                        bodies[item.as_pointer()] = Nothing
                        for owner in host.to_list():
                            header, body = owner.panel(f"{entry.name}_{item.identifier}", default_closed=item.default_closed)
                            bodies[item.as_pointer()] = Option.of_optional(body)
                            if (toggle := (next((child for child in item.interface_items if isinstance(child, bpy.types.NodeTreeInterfaceSocket) and child.is_panel_toggle), None))) is None:
                                header.label(text=item.name)
                            else:

                                def toggled(value: bpy.types.bpy_struct | bpy.types.bpy_prop_collection) -> None:  # type: ignore[type-arg]  # ty: ignore[missing-type-argument]
                                    header.prop(value, "value", text=item.name)
                                    for inner in Option.of_optional(body).to_list():
                                        inner.active = value.value  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

                                alerted(
                                    header,
                                    struct_at(entry, f"properties.inputs.{toggle.identifier}").to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=toggle.identifier),)).map(toggled),
                                )
                    case bpy.types.NodeTreeInterfaceSocket(in_out="INPUT", hide_in_modifier=False, is_panel_toggle=False) if item != first:
                        for owner in host.to_list():
                            row = owner.row()
                            selected: Option[Condition] = Option.of_optional(gates.get(item.identifier))
                            for condition in selected.to_list():
                                held = check(condition, context, Some(entry))
                                row.active = held.is_ok()
                                alerted(row, held)
                            alerted(
                                row,
                                struct_at(entry, f"properties.inputs.{item.identifier}")
                                .to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=item.identifier),))
                                .map(lambda value: row.prop(value, "value", text=item.name, expand=isinstance(item, bpy.types.NodeTreeInterfaceSocketMenu) and item.menu_expanded)),
                            )

        def owned(
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
            hidden = frozenset(
                item_key(*prefix, *path)
                for value, prefix, _, path in parameters
                if any(isinstance(part, type) and not isinstance(reached(value, path[:index]), part) for index, part in enumerate(path))
            )

            def graph_fields() -> BlenderResult[None]:
                answers: list[BlenderResult[None]] = []
                for value, prefix, _ in values:
                    for path, (field, shape) in graphed(type(value), value=Some(value)).items():
                        if (key := (item_key(*prefix, *path))) in hidden:
                            continue
                        title = next((meta.title for meta in markers(field.type, msgspec.Meta) if meta.title is not None), field.name.replace("_", " ").title())
                        if GraphForm.COLOR_SPACE in markers(field.type, GraphForm):
                            line = layout.row(align=True)
                            button(line, self.colorspace, SpaceField(name=entry.name, field=key, space=reached(value, path)), title)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                            answers.append(locked(line, entry, paths[key], context))
                        else:
                            for node in group.nodes:
                                if Mark.read(identity, node).filter(lambda mark: mark.key == key).is_none():
                                    continue
                                inner = layout.column()
                                inner.use_property_split = False
                                header = inner.row(align=True)
                                header.label(text=title)
                                answers.append(locked(header, entry, paths[key], context))
                                match shape:
                                    case inspect.VarTupleType(item_type=inspect.StructType(cls=kind)) if (get_origin(kind) or kind) is Knot:
                                        inner.template_curve_mapping(node, "mapping", type="NONE")
                                    case inspect.StructType(cls=kind) if kind is Ramp:
                                        inner.template_color_ramp(node, "color_ramp", expand=True)
                                    case inspect.VarTupleType():
                                        pass
                                    case _:
                                        inner.template_node_inputs(node)
                return accumulated(answers).map(lambda _: None)

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
                entry_interface(row.record, len(record.blend.masks))
                .bind(lambda face: expanded(face, record.effect))
                .bind(lambda face: inputs(layout, entry, face, hidden, paths, context))
                .bind(lambda _: attached())
                .bind(lambda _: graph_fields())
            )

        def notices(
            layout: bpy.types.UILayout, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, row: Placeable, record: Entry[Effect], context: bpy.types.Context
        ) -> BlenderResult[None]:
            native = tuple(scene.compositor_effects)
            formed = tuple(stack.entries(identity, scene, DisplayView))
            if row.record.stage.domain is Domain.DISPLAY and (not formed or native.index(entry) < native.index(formed[0])):
                layout.label(text="Requires a formation before display effects", icon="STATUS_ERROR_FILLED")

            def shown(config: PyOpenColorIO.Config, view: DisplayView) -> BlenderResult[None]:
                if row.record.stage is Stage.TONE:
                    return (
                        DisplayView
                        .for_encoding(config, view.display, Some(view.view))
                        .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
                        .map(lambda encoded: layout.label(text="Requires an encoding-only formation", icon="STATUS_ERROR_FILLED") if encoded.is_none() else None)
                    )
                if isinstance(record.effect, DisplayView):
                    for space in record.effect.colorspace(config).to_list():
                        if DynamicRange.of(space) is DynamicRange.HIGH and not any(window.support_hdr_color for window in Option.of_optional(context.window).to_list()):
                            split(layout)[1].label(
                                text="HDR not supported with OpenGL backend" if gpu.platform.backend_type_get() == "OPENGL" else "HDR display not supported", icon="STATUS_INFO_FILLED"
                            )
                return Ok(None)

            return paired(stack.config(), formation.current(identity, scene)).bind(lambda parts: shown(parts[0], parts[1].default_value(viewed(scene))))

        def shown(layout: bpy.types.UILayout, holder: bpy.types.bpy_struct, context: bpy.types.Context) -> None:  # type: ignore[type-arg]  # ty: ignore[missing-type-argument]
            if isinstance(holder, bpy.types.SceneCompositorEffect):
                entry, scene, group = holder, holder.id_data, holder.node_group
                if group is None:
                    layout.label(text="Requires an effect node group", icon="STATUS_ERROR_FILLED")
                    return
                row = next((held.row for held in stack.held(identity, scene) if held.entry == entry), Nothing)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                for offered in row.to_list():
                    alerted(
                        layout,
                        offered.read(identity, entry).bind(
                            lambda record: owned(layout, scene, entry, group, offered, record, context).bind(lambda _: notices(layout, scene, entry, offered, record, context))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        ),
                    )
                if row.is_none():
                    pinned = next((pinned for pinned in stack.pinned if Mark.read(identity, group).filter(lambda mark: mark.key == pinned.ownership.key).is_some()), None)
                    gates: BlenderResult[dict[Key, Condition]] = pinned.conditions.map(lambda conditions: conditions(identity, entry)).default_value(Ok({})) if pinned is not None else Ok({})
                    alerted(
                        layout, paired(addresses(group, identity), gates).map(lambda parts: native(layout, entry, group, {parts[0][key]: condition for key, condition in parts[1].items()}, context))
                    )
                buttons = layout.row(align=True)
                button(buttons, self.viewer.viewing, ViewTarget(name=entry.name, node=EffectViewer.entering.node, socket=EffectViewer.entering.socket), "Input")
                button(buttons, self.viewer.viewing, ViewTarget(name=entry.name, node=EffectViewer.leaving.node, socket=EffectViewer.leaving.socket), "Result")
                button(buttons, self.viewer.clearing, Named(name=entry.name), "Clear")

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------


def effect_section(viewer: EffectViewer, title: str = "Effects", *, thumbnail: Row | RowDraw, locks: Option[Callable[[Option[tuple[Key | int, ...]]], RowContent]] = Nothing) -> Each:
    """Constructs the native section and its stable colorspace and viewer declarations."""

    def changed(call: Call[SpaceField]) -> BlenderResult[Done]:
        def wrote(scene: bpy.types.Scene, held: StackEntry) -> BlenderResult[Done]:
            return (
                Result[Placeable, BlenderFaults]
                .bind(
                    held.row.to_result_with(lambda: (BlenderAbsentFault(owner=repr(held.entry), key=call.record.field),)),
                    lambda row: row.read(call.identity, held.entry, graphs={call.record.field: call.record.space}).bind(lambda record: row.write(call.identity, scene, held.entry, record)),
                )
                .map(lambda _: Done())
            )

        return call.root(bpy.types.Scene).bind(lambda scene: viewer.stack.found(call.identity, scene, call.record.name).bind(lambda held: wrote(scene, held)))

    colorspace = Operator(
        key="effect_colorspace",
        title="Choose Color Space",
        description="Changes the effect's graph colorspace",
        form=SpaceField,
        run=changed,
        invoke=Some(InvokePopup.SEARCH),
        primary=Some(next(field for field in msgspec.structs.fields(SpaceField) if spaces in markers(field.type, Choices))),
        undo=True,
    )
    body = EffectBody(viewer=viewer, colorspace=colorspace, locks=locks)

    def empty(layout: bpy.types.UILayout, _holder: bpy.types.bpy_struct, _context: bpy.types.Context) -> None:  # type: ignore[type-arg]  # ty: ignore[missing-type-argument]
        layout.menu("SCENE_MT_add_compositor_effect", text="Add Effect")

    return Each(
        key="effects",
        title=title,
        collection=Member(root=bpy.types.Scene, path="compositor_effects"),
        open=Some("show_expanded"),
        condition=Some(composites),
        rows=(thumbnail, body),
        header=(EffectButtons(stack=viewer.stack, locks=locks),),
        empty=(empty,),
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
