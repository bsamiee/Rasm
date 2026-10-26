"""Active compositor effect section beside the native scene effect stack, and the native editors drawing graphed record fields."""

from collections.abc import Callable, Mapping
import functools
from types import MappingProxyType
import typing
from typing import Annotated, ClassVar, get_origin, Self

import bpy
from expression import Nothing, Ok, Option, Some
import gpu
import msgspec
import msgspec.inspect as native
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, InvokePopup, Operator, Outcome, Own, Unchanged
from blender.bpy.data.rna.conditions import check
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.changes import owned
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket
from blender.bpy.render.effects.formation import formation, viewed
from blender.bpy.render.effects.records import active, CurveEditor, entry_input, Field, KeyPath, reached
from blender.bpy.render.effects.rows import CompositorEffect, Enabled, entry_interface, expanded, HIDDEN, Named, Shell
from blender.bpy.render.effects.stack import composites, Configured
from blender.bpy.render.effects.viewers import EffectViewer, EntryPick, ViewTarget
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.properties import Choices, SPACES
from blender.bpy.ui.layout.rows import acquired, alerted, invoking, joined, Line, Member, OperatorButton, placed, pressed, realized, Row, RowContent, RowDraw, split
from blender.bpy.ui.layout.sections import fields, Single
from blender.bpy.ui.layout.templates import ColorRamp, Curve, Wheel
from blender.imaging.chain import Domain, Effect, Stage
from blender.imaging.color.authoring import DynamicRange
from blender.imaging.color.processors import DisplayView
from blender.imaging.compositing.blending import Blend, Entry
from blender.imaging.effects.grade import primary
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import GraphForm, Knot, markers, Ramp

# --- [TYPES] ----------------------------------------------------------------------------

type Locks = Callable[[Option[tuple[Key | int, ...]]], RowContent]

# --- [MODELS] ---------------------------------------------------------------------------


class SpaceField(Named, frozen=True, kw_only=True):
    """Entry graph field and OCIO colorspace."""

    field: str
    space: Annotated[str, SPACES] = ""


class GraphWidget(msgspec.Struct, frozen=True, kw_only=True):
    """Owned node editing one active graphed path under its heading, a shared editor standing once for its fields."""

    node: Key
    heading: str
    field: Field
    value: msgspec.Struct


class GraphEditors(msgspec.Struct, frozen=True, kw_only=True):
    """Native editors drawing graphed fields: the curve editor per curve node kind, channel tabs by kind, and the color ramp editor."""

    curves: Mapping[type[bpy.types.Node], RowDraw]
    ramp: RowDraw
    tabs: ClassVar[Mapping[type[bpy.types.Node], str]] = MappingProxyType({
        bpy.types.CompositorNodeCurveRGB: "COLOR",
        bpy.types.CompositorNodeHueCorrect: "HUE",
        bpy.types.ShaderNodeFloatCurve: "NONE",
    })

    @classmethod
    def realized(cls, identity: Identity) -> BlenderResult[Self]:
        """Realizes every curve editor and the ramp editor once."""
        return paired(
            accumulated(realized(Curve(target=Member(root=kind, path="mapping"), type=tab), identity).map(lambda draw: (kind, draw)) for kind, tab in cls.tabs.items()).map(dict),
            realized(ColorRamp(target=Member(root=bpy.types.ShaderNodeValToRGB, path="color_ramp")), identity),
        ).map(lambda parts: cls(curves=parts[0], ramp=parts[1]))

    def drawn(self, layout: bpy.types.UILayout, node: bpy.types.Node, widget: GraphWidget, context: bpy.types.Context) -> BlenderResult[None]:
        """Draws widget heading over its node's native editor: curve, ramp, or node inputs."""
        inner = layout.column()
        inner.use_property_split = False
        inner.label(text=widget.heading)
        match widget.field.shape:
            case native.VarTupleType(item_type=native.StructType(cls=kind)) if (typing.get_origin(kind) or kind) is Knot:
                return Option.of_optional(self.curves.get(type(node))).map(lambda draw: draw(inner, node, context)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(node), key="mapping"),))
            case native.StructType(cls=kind) if kind is Ramp:
                self.ramp(inner, node, context)
                return Ok(None)
            case native.VarTupleType():
                return Ok(None)
            case _:
                inner.template_node_inputs(node)
                return Ok(None)


class EffectButtons(Line, frozen=True, kw_only=True):
    """Entry header actions the native effect header lacks: setup locks, the row's entry commands, and reset."""

    viewer: EffectViewer
    locks: Option[Locks] = Nothing

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Returns header draw with command records from each entry."""
        stack = self.viewer.stack

        def shown(layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
            if isinstance(holder, bpy.types.SceneCompositorEffect):
                row = stack.row(identity, holder)
                actions = (
                    *(locks(Nothing) for locks in self.locks.to_list() if row.is_some()),
                    *(
                        OperatorButton(target=Own(command=command, values=command.form(name=holder.name)), text="")
                        for offered in row.to_list()
                        for command in (*offered.registrables, *self.viewer.commands.get(offered.kind, ()))
                        if isinstance(command, Operator) and issubclass(command.form, Named)
                    ),
                    OperatorButton(target=Own(command=stack.operators.resetting, values=Named(name=holder.name)), text="", icon=Some("LOOP_BACK")),
                )
                alerted(layout, placed(actions, identity).map(lambda draws: joined(draws)(layout, holder, context)))

        return Ok(shown)


class EffectBody(Line, frozen=True, kw_only=True):
    """Entry graph templates, attachments, notices, setup locks, and Viewer buttons the native effect body lacks."""

    viewer: EffectViewer
    colorspace: Operator[SpaceField]
    locks: Option[Locks] = Nothing

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns viewer and colorspace commands, and lock rows."""
        return Ok((self.viewer, self.colorspace, *acquired(tuple(locks(Nothing) for locks in self.locks.to_list()))))

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Returns entry body draw over the record and owned nodes, buttons invoking their commands."""
        stack = self.viewer.stack
        record_key = next(field.name for field in msgspec.structs.fields(Configured) if get_origin(field.type) is Entry)
        entry_fields = msgspec.structs.fields(Entry)
        enables = tuple((enabled, field) for field in msgspec.structs.fields(Configured) for enabled in markers(field.type, Enabled))

        def locked(layout: bpy.types.UILayout, entry: bpy.types.SceneCompositorEffect, path: tuple[Key | int, ...], context: bpy.types.Context) -> BlenderResult[None]:
            return accumulated(realized(locks(Some(path)), identity).map(lambda draw: draw(layout, entry, context)) for locks in self.locks.to_list()).map(lambda _: None)

        def listed(
            layout: bpy.types.UILayout,
            entry: bpy.types.SceneCompositorEffect,
            face: Interface,
            hidden: frozenset[Key],
            paths: Mapping[Key, tuple[Key | int, ...]],
            graphs: tuple[tuple[str, tuple[Key | int, ...]], ...],
            context: bpy.types.Context,
        ) -> BlenderResult[None]:
            concealed, titles = set(hidden), dict[Key, str]()
            lockable: list[tuple[str, tuple[Key | int, ...]]] = [*((enabled.descriptor.prop.name, (field.name,)) for enabled, field in enables), *graphs]
            for key, item, parent, _ in face.walked():
                if key in concealed or parent.filter(concealed.__contains__).is_some():
                    concealed.add(key)
                    continue
                outer = parent.bind(lambda held: Option.of_optional(titles.get(held)))
                match item:
                    case InterfacePanel(name=name):
                        titles[key] = outer.map(lambda above: f"{above} > {name}").default_value(name)
                        if key in paths:
                            lockable.append((titles[key], paths[key]))
                    case (Socket(name=name, members=members), "INPUT") if key in paths and key != Shell.image and members.get(HIDDEN) is not True:
                        lockable.append((outer.map(lambda above: f"{above} > {name}").default_value(name), paths[key]))
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
            layout: bpy.types.UILayout,
            scene: bpy.types.Scene,
            entry: bpy.types.SceneCompositorEffect,
            group: bpy.types.NodeTree,
            row: CompositorEffect[Effect],
            record: Entry[Effect],
            context: bpy.types.Context,
            editors: GraphEditors,
        ) -> BlenderResult[None]:
            values = tuple((value, (Shell.blend,) if isinstance(value, Blend) else (), field.name) for field in entry_fields for value in (getattr(record, field.name),))
            parameters = tuple((value, prefix, name, member) for value, prefix, name in values for member in Field.of(type(value), value=Some(value)))
            paths = {
                item_key(*prefix, *member.path[:depth]): (record_key, name, *(part for part in member.path[:depth] if not isinstance(part, type)))
                for _, prefix, name, member in parameters
                for depth in range(1, len(member.path) + 1)
            }
            paths[Shell.blend] = (record_key, next(name for value, _, name in values if isinstance(value, Blend)))
            hidden = frozenset(item_key(*prefix, *member.path) for value, prefix, _, member in parameters if not active(value, member.path))
            nodes = owned(group, identity)
            widgets = tuple(widget for value, prefix, _ in values for widget in graph_widgets(value, prefix, () if prefix else row.editors))

            def graph_field(widget: GraphWidget) -> tuple[BlenderResult[None], ...]:
                match reached(widget.value, widget.field.path):
                    case str(space) if GraphForm.COLOR_SPACE in widget.field.forms:
                        return (pressed(layout.row(align=True), identity, self.colorspace, SpaceField(name=entry.name, field=widget.node, space=space), text=widget.heading, icon="NONE"),)
                    case _:
                        return tuple(editors.drawn(layout, node, widget, context) for node in Option.of_optional(nodes.get(widget.node)).to_list())

            def wheeled() -> BlenderResult[None]:
                flow = layout.grid_flow(row_major=True, columns=2, even_columns=True)

                def bound(key: Key) -> BlenderResult[Member]:
                    return entry_input(identity, entry, key).map(lambda held: Member(root=type(held), path="value", reach=Some(lambda _context: Some(held))))

                def wheel(title: str, key: Key) -> BlenderResult[None]:
                    color, master = primary.Wheel.__struct_fields__
                    inner = flow.column()
                    inner.label(text=title)
                    return (
                        paired(bound(item_key(key, color)), bound(item_key(key, master)))
                        .bind(lambda pair: realized(Wheel(color=pair[0], luma=Some(pair[1])), identity))
                        .map(lambda draw: draw(inner, entry, context))
                    )

                return accumulated(
                    wheel(titled(member), item_key(*prefix, *member.path))
                    for value, prefix, _, member in parameters
                    if item_key(*prefix, *member.path) not in hidden and isinstance(reached(value, member.path), primary.Wheel)
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

            def locks_listed() -> BlenderResult[None]:
                graphed = tuple((titled(member), paths[key]) for value, prefix, _, member in parameters if member.graphed for key in (item_key(*prefix, *member.path),) if key not in hidden)
                return accumulated(
                    entry_interface(row.record, len(record.blend.masks), blended=row.blended)
                    .bind(lambda face: expanded(face, record.effect))
                    .bind(lambda face: listed(layout, entry, face, hidden, paths, graphed, context))
                    for _ in self.locks.to_list()
                ).map(lambda _: None)

            return wheeled().bind(lambda _: accumulated(answer for widget in widgets for answer in graph_field(widget))).bind(lambda _: attached()).bind(lambda _: locks_listed())

        def notices(
            layout: bpy.types.UILayout, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, row: CompositorEffect[Effect], record: Entry[Effect], context: bpy.types.Context
        ) -> BlenderResult[None]:
            order = tuple(scene.compositor_effects)
            formed = formation.entries(identity, scene)
            if row.record.stage.domain is Domain.DISPLAY and (not formed or order.index(entry) < order.index(formed[0])):
                layout.label(text="Needs earlier Display View entry", icon="STATUS_ERROR_FILLED")

            def shown(config: PyOpenColorIO.Config, view: DisplayView) -> BlenderResult[None]:
                if row.record.stage is Stage.TONE:
                    return DisplayView.for_encoding(config, view.display, Some(view.view)).map(
                        lambda encoded: layout.label(text="Needs encoding-only Display View", icon="STATUS_ERROR_FILLED") if encoded.is_none() else None
                    )
                if (
                    isinstance(record.effect, DisplayView)
                    and any(DynamicRange.of(space) is DynamicRange.HIGH for space in record.effect.colorspace(config).to_list())
                    and not any(window.support_hdr_color for window in Option.of_optional(context.window).to_list())
                ):
                    split(layout)[1].label(text="OpenGL backend cannot show HDR" if gpu.platform.backend_type_get() == "OPENGL" else "Window cannot show HDR", icon="STATUS_INFO_FILLED")
                return Ok(None)

            return paired(stack.config(), formation.current(identity, scene)).bind(lambda parts: shown(parts[0], parts[1].default_value(viewed(scene))))

        def shown(editors: GraphEditors, layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
            match holder:
                case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, node_group=bpy.types.NodeTree() as group) as entry:
                    row = stack.row(identity, entry)
                    for offered in row.to_list():
                        alerted(
                            layout,
                            offered.read(identity, entry).bind(
                                lambda record: offered_drawn(layout, scene, entry, group, offered, record, context, editors).bind(lambda _: notices(layout, scene, entry, offered, record, context))
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
                    alerted(
                        buttons,
                        accumulated((
                            pressed(
                                buttons, identity, self.viewer.viewing, ViewTarget(name=entry.name, node=EffectViewer.entering.node, socket=EffectViewer.entering.socket), text="Input", icon="NONE"
                            ),
                            pressed(
                                buttons, identity, self.viewer.viewing, ViewTarget(name=entry.name, node=EffectViewer.leaving.node, socket=EffectViewer.leaving.socket), text="Result", icon="NONE"
                            ),
                            pressed(buttons, identity, self.viewer.clearing, Named(name=entry.name), text="Clear", icon="NONE"),
                            *(pressed(buttons, identity, self.viewer.picking, EntryPick(name=entry.name), text="", icon="EYEDROPPER") for offered in row.to_list() if offered.measured.is_some()),
                        )),
                    )
                case _:
                    pass

        return GraphEditors.realized(identity).map(lambda editors: invoking(functools.partial(shown, editors)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def titled(field: Field) -> str:
    """Returns field's declared title, else its display name."""
    return next((meta.title for meta in markers(field.info.type, msgspec.Meta) if meta.title is not None), bpy.path.display_name(field.info.name))


def graph_widgets(value: msgspec.Struct, prefix: KeyPath, editors: tuple[CurveEditor, ...] = ()) -> tuple[GraphWidget, ...]:
    """Returns active graphed paths of value in field order, each shared editor once at its first field."""
    shared = {field: editor for editor in editors for field in editor.fields}
    return tuple(
        GraphWidget(
            node=item_key(*prefix, editor.key) if editor is not None else item_key(*prefix, *field.path),
            heading=bpy.path.display_name(editor.key) if editor is not None else titled(field),
            field=field,
            value=value,
        )
        for field in Field.of(type(value), value=Some(value))
        if field.graphed and active(value, field.path)
        for editor in (shared.get(field.info.name),)
        if editor is None or field.info.name == editor.fields[0]
    )


def effect_section(viewer: EffectViewer, *, thumbnail: Row | RowDraw, locks: Option[Locks] = Nothing) -> Single:
    """Builds active effect section, open by default, with its colorspace command and viewer."""

    def changed(call: Call[SpaceField]) -> BlenderResult[Outcome]:
        return call.root(bpy.types.Scene).bind(
            lambda scene: viewer.stack.found(call.identity, scene, call.record.name).bind(
                lambda held: (
                    held.row
                    .read(call.identity, held.entry, graphs={call.record.field: call.record.space})
                    .bind(lambda record: held.row.write(call.identity, scene, held.entry, record))
                    .map(lambda wrote: Done() if wrote else Unchanged(reason=f"{held.entry.name} reads {call.record.space}"))
                )
            )
        )

    colorspace = Operator(
        key="effect_colorspace",
        title="Choose Color Space",
        description="Set graph field color space",
        form=SpaceField,
        run=changed,
        invoke=Some(InvokePopup.SEARCH),
        primary=Some(next(field for field in msgspec.structs.fields(SpaceField) if SPACES in markers(field.type, Choices))),
    )
    return Single(
        key="active_effect",
        title="Active Effect",
        holder=Some(Member(root=bpy.types.Scene, path="compositor_effects.active")),
        condition=Some(composites),
        rows=(thumbnail, EffectBody(viewer=viewer, colorspace=colorspace, locks=locks)),
        header=(EffectButtons(viewer=viewer, locks=locks),),
        opened=True,
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EffectBody", "EffectButtons", "GraphEditors", "GraphWidget", "Locks", "SpaceField", "effect_section", "graph_widgets", "titled"]
