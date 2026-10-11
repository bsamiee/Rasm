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
from rasm.blender.data.rna.descriptors import Descriptor
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
    interface_input,
    KeyPath,
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

        def shown(layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
            if isinstance(holder, bpy.types.SceneCompositorEffect) and isinstance(scene := holder.id_data, bpy.types.Scene):
                row = next((held.row for held in self.stack.held(identity, scene) if held.entry == holder), Nothing)
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

        def toggled(header: bpy.types.UILayout, body: Option[bpy.types.UILayout], name: str, value: "bpy.types.bpy_struct[object]") -> None:
            header.prop(value, "value", text=name)
            for inner in body.to_list():
                inner.active = value.value  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

        def inputs(
            layout: bpy.types.UILayout, entry: bpy.types.SceneCompositorEffect, face: Interface, hidden: frozenset[Key], paths: Mapping[Key, tuple[Key | int, ...]], context: bpy.types.Context
        ) -> BlenderResult[None]:
            def lock(line: bpy.types.UILayout, key: Key) -> tuple[BlenderResult[None], ...]:
                return tuple(locked(line, entry, path, context) for path in Option.of_optional(paths.get(key)).to_list())

            def panel(owner: bpy.types.UILayout, key: Key, name: str, closed: bool, toggle: Option[bool]) -> tuple[Option[bpy.types.UILayout], tuple[BlenderResult[None], ...]]:
                header, body = owner.panel(f"{entry.name}_{key}", default_closed=closed)
                if toggle.is_none():
                    header.label(text=name)
                return Option.of_optional(body), (
                    *(entry_input(identity, entry, key).map(lambda value: toggled(header, Option.of_optional(body), name, value)) for _ in toggle.to_list()),
                    *lock(header, key),
                )

            def socket(owner: bpy.types.UILayout, key: Key, name: str) -> tuple[BlenderResult[None], ...]:
                line = owner.row(align=True)
                return (entry_input(identity, entry, key).map(lambda value: line.prop(value, "value", text=name)).map(lambda _: None), *lock(line, key))

            bodies: dict[Option[Key], Option[bpy.types.UILayout]] = {Nothing: Some(layout)}
            answers: list[BlenderResult[None]] = []
            for key, item, parent, _ in face.walked():
                host = bodies[parent] if key not in hidden else Nothing
                match item:
                    case InterfacePanel(name=name, closed=closed, toggle=toggle):
                        bodies[Some(key)], drawn = host.map(lambda owner: panel(owner, key, name, closed, toggle)).default_value((Nothing, ()))
                        answers.extend(drawn)
                    case (Socket(name=name, members=members), "INPUT") if key != Shell.image and not any(
                        descriptor.prop.identifier == "hide_in_modifier" and value for descriptor, value in members.items()
                    ):
                        answers.extend(answer for owner in host.to_list() for answer in socket(owner, key, name))
            return accumulated(answers).map(lambda _: None)

        def native(layout: bpy.types.UILayout, entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree, gates: dict[str, Condition], context: bpy.types.Context) -> None:
            def panel(owner: bpy.types.UILayout, item: bpy.types.NodeTreeInterfacePanel) -> Option[bpy.types.UILayout]:
                header, body = owner.panel(f"{entry.name}_{item.identifier}", default_closed=item.default_closed)
                if (toggle := next((child for child in item.interface_items if isinstance(child, bpy.types.NodeTreeInterfaceSocket) and child.is_panel_toggle), None)) is None:
                    header.label(text=item.name)
                else:
                    alerted(header, interface_input(entry, toggle.identifier).map(lambda value: toggled(header, Option.of_optional(body), item.name, value)))
                return Option.of_optional(body)

            def socket(owner: bpy.types.UILayout, item: bpy.types.NodeTreeInterfaceSocket) -> None:
                row = owner.row()
                if (condition := gates.get(item.identifier)) is not None:
                    held = check(condition, context, Some(entry))
                    row.active = held.is_ok()
                    alerted(row, held)
                alerted(
                    row,
                    interface_input(entry, item.identifier).map(
                        lambda value: row.prop(value, "value", text=item.name, expand=isinstance(item, bpy.types.NodeTreeInterfaceSocketMenu) and item.menu_expanded)
                    ),
                )

            face: bpy.types.NodeTreeInterface = group.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            bodies = {root.as_pointer(): Some(layout) for root in Option.of_optional(face.root_panel).to_list()}
            first = next((item for item in face.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket) and item.in_out == "INPUT"), None)
            for item in face.items_tree:
                host = Option.of_optional(item.parent).bind(lambda parent: bodies[parent.as_pointer()])
                match item:
                    case bpy.types.NodeTreeInterfacePanel():
                        bodies[item.as_pointer()] = host.bind(lambda owner: panel(owner, item))
                    case bpy.types.NodeTreeInterfaceSocket(in_out="INPUT", hide_in_modifier=False, is_panel_toggle=False) if item != first:
                        for owner in host.to_list():
                            socket(owner, item)

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
                    case str() as space if GraphForm.COLOR_SPACE in markers(field.type, GraphForm):
                        line = layout.row(align=True)
                        button(line, self.colorspace, SpaceField(name=entry.name, field=key, space=space), title)
                        return (locked(line, entry, paths[key], context),)
                    case _:
                        return tuple(widget(node, key, title, shape) for node in group.nodes if Mark.read(identity, node).filter(lambda mark: mark.key == key).is_some())

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
                if (
                    isinstance(record.effect, DisplayView)
                    and any(DynamicRange.of(space) is DynamicRange.HIGH for space in record.effect.colorspace(config).to_list())
                    and not any(window.support_hdr_color for window in Option.of_optional(context.window).to_list())
                ):
                    split(layout)[1].label(text="HDR not supported with OpenGL backend" if gpu.platform.backend_type_get() == "OPENGL" else "HDR display not supported", icon="STATUS_INFO_FILLED")
                return Ok(None)

            return paired(stack.config(), formation.current(identity, scene)).bind(lambda parts: shown(parts[0], parts[1].default_value(viewed(scene))))

        def shown(layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
            match holder:
                case bpy.types.SceneCompositorEffect(node_group=None):
                    layout.label(text="Requires an effect node group", icon="STATUS_ERROR_FILLED")
                case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, node_group=bpy.types.NodeTree() as group) as entry:
                    row = next((held.row for held in stack.held(identity, scene) if held.entry == entry), Nothing)
                    for offered in row.to_list():
                        alerted(
                            layout,
                            offered.read(identity, entry).bind(
                                lambda record: owned(layout, scene, entry, group, offered, record, context).bind(lambda _: notices(layout, scene, entry, offered, record, context))
                            ),
                        )
                    if row.is_none():
                        pinned = next((pinned for pinned in stack.pinned if Mark.read(identity, group).filter(lambda mark: mark.key == pinned.ownership.key).is_some()), None)
                        gates: BlenderResult[dict[Key, Condition]] = pinned.conditions.map(lambda conditions: conditions(identity, entry)).default_value(Ok({})) if pinned is not None else Ok({})
                        alerted(
                            layout,
                            paired(addresses(group, identity), gates).map(lambda parts: native(layout, entry, group, {parts[0][key]: condition for key, condition in parts[1].items()}, context)),
                        )
                    buttons = layout.row(align=True)
                    button(buttons, self.viewer.viewing, ViewTarget(name=entry.name, node=EffectViewer.entering.node, socket=EffectViewer.entering.socket), "Input")
                    button(buttons, self.viewer.viewing, ViewTarget(name=entry.name, node=EffectViewer.leaving.node, socket=EffectViewer.leaving.socket), "Result")
                    button(buttons, self.viewer.clearing, Named(name=entry.name), "Clear")
                case _:
                    pass

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

    def empty(layout: bpy.types.UILayout, _holder: object, _context: bpy.types.Context) -> None:
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
