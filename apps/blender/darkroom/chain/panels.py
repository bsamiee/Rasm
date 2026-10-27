"""Active effect section of the chain beside the native scene effect stack: entry commands, grading wheels, node editors, colorspace picks, notices, setup locks, and Viewer buttons."""

from collections.abc import Callable, Mapping
import functools

import bpy
from expression import Nothing, Ok, Option, Some
import gpu
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, InvokePopup, Operator, Own, Unchanged
from blender.bpy.data.rna.conditions import check
from blender.bpy.data.rna.descriptors import Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.changes import owned
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import CompositorEffect, Enabled, Named, Registers, revised, Shell
from blender.bpy.render.compositor.entries import composites
from blender.bpy.render.compositor.inputs import interface_input, keyed
from blender.bpy.render.compositor.viewers import Viewers
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.layout.rows import acquired, alerted, invoking, joined, Line, Member, OperatorButton, placed, pressed, realized, Row, RowContent, RowDraw, split
from blender.bpy.ui.layout.sections import fields, Single
from blender.bpy.ui.layout.templates import NodeEditors, Wheel
from blender.imaging.color.authoring import DynamicRange
from blender.imaging.color.processors import ColorSpace, DisplayView, Domain
from blender.records.faults import accumulated, Outcome, paired

from .stack import Chain
from .stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type Locks = Callable[[Option[Key]], RowContent]

# --- [MODELS] ---------------------------------------------------------------------------


class SpaceField(bpy.types.PropertyGroup):
    """Entry record field and the OCIO colorspace it reads."""

    name: bpy.props.StringProperty(name="Entry", description="Compositor entry name")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    field: bpy.props.StringProperty(name="Field", description="Record field holding the colorspace")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    space: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Color Space",
        description="Colorspace the field reads",
        items=lambda _owner, _context: _spaces(tuple((item.identifier, item.name, item.description) for item in bpy.types.ColorManagedInputColorspaceSettings.bl_rna.properties["name"].enum_items)),  # ty: ignore[unresolved-attribute]
    )


class EffectButtons(Line, frozen=True, kw_only=True):
    """Entry header actions the native effect header lacks: setup locks, the effect's own commands, and reset."""

    viewers: Viewers[Effect]
    locks: Option[Locks] = Nothing

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Returns the header draw placing each owned entry's commands over its name."""
        rows = self.viewers.rows

        def shown(layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
            if isinstance(holder, bpy.types.SceneCompositorEffect):
                row = rows.row(identity, holder)
                actions = (
                    *(locks(Nothing) for locks in self.locks.to_list() if row.is_some()),
                    *(
                        OperatorButton(target=Own(command=command, values={"name": holder.name}), text="")
                        for offered in row.to_list()
                        for part in offered.found(Registers)
                        for command in part.rows
                        if isinstance(command, Operator) and issubclass(command.form, Named)
                    ),
                    OperatorButton(target=Own(command=rows.operators.resetting, values={"name": holder.name}), text="", icon=Some("LOOP_BACK")),
                )
                alerted(layout, placed(actions, identity).map(lambda draws: joined(draws)(layout, holder, context)))

        return Ok(self.conditioned(shown))


class EffectBody(Line, frozen=True, kw_only=True):
    """Grading wheels, curve and ramp editors, colorspace picks, attachments, notices, setup locks, and Viewer buttons the native effect body lacks."""

    chain: Chain
    viewers: Viewers[Effect]
    colorspace: Operator[SpaceField]
    locks: Option[Locks] = Nothing

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the Viewer and colorspace commands and lock rows."""
        return Ok((self.viewers, self.colorspace, *acquired(tuple(locks(Nothing) for locks in self.locks.to_list()))))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Returns the entry body draw over its group interface, record, and owned nodes."""
        chain, rows = self.chain, self.viewers.rows

        def bound(held: Struct) -> Member:
            return Member(root=type(held), path="value", reach=Some(lambda _context: Some(held)))

        def listed(layout: bpy.types.UILayout, entry: bpy.types.SceneCompositorEffect, interface: bpy.types.NodeTreeInterface, context: bpy.types.Context) -> Outcome[None]:
            def named(socket: bpy.types.NodeTreeInterfaceSocket) -> str:
                parent: bpy.types.NodeTreeInterfacePanel = socket.parent  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                return f"{parent.name} > {socket.name}" if parent.name else socket.name

            def lockable(addresses: Mapping[Key, str]) -> tuple[tuple[str, Key], ...]:
                live = {item.identifier: item for item in interface.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket) and item.in_out == "INPUT"}
                return (
                    *((enabled.value.name, enabled.value.identifier) for enabled in Enabled),
                    *(
                        (named(socket), key)
                        for key, identifier in addresses.items()
                        if key != Shell.image
                        for socket in Option.of_optional(live.get(identifier)).to_list()
                        if not socket.hide_in_modifier
                    ),
                )

            def lined(inner: bpy.types.UILayout, locks: Locks, name: str, key: Key) -> Outcome[None]:
                line = inner.row(align=True)
                line.label(text=name)
                return realized(locks(Some(key)), identity).map(lambda draw: draw(line, entry, context))

            def drawn(names: tuple[tuple[str, Key], ...]) -> Outcome[None]:
                header, body = layout.panel(f"{entry.name}_locks", default_closed=True)
                header.label(text="Setup Locks")
                return accumulated(lined(inner, locks, *pair) for inner in Option.of_optional(body).to_list() for locks in self.locks.to_list() for pair in names).map(lambda _: None)

            return keyed(identity, entry).map(lambda pair: lockable(pair[1])).bind(drawn) if self.locks.is_some() else Ok(None)

        def offered_drawn(
            layout: bpy.types.UILayout,
            scene: bpy.types.Scene,
            entry: bpy.types.SceneCompositorEffect,
            group: bpy.types.NodeTree,
            row: CompositorEffect[Effect],
            record: Entry[Effect],
            context: bpy.types.Context,
            editors: NodeEditors,
        ) -> Outcome[None]:
            def wheeled() -> Outcome[None]:
                flow = layout.grid_flow(row_major=True, columns=2, even_columns=True)

                def wheel(panel: bpy.types.NodeTreeInterfacePanel, color: str, luma: str) -> Outcome[None]:
                    inner = flow.column()
                    inner.label(text=panel.name)
                    return (
                        paired(interface_input(entry, color), interface_input(entry, luma))
                        .bind(lambda pair: realized(Wheel(color=bound(pair[0]), luma=Some(bound(pair[1]))), identity))
                        .map(lambda draw: draw(inner, entry, context))
                    )

                return accumulated(
                    wheel(panel, color.identifier, luma.identifier)
                    for panel in interface.items_tree
                    if isinstance(panel, bpy.types.NodeTreeInterfacePanel)
                    for color, luma in (tuple(panel.interface_items),)
                    if isinstance(color, bpy.types.NodeTreeInterfaceSocketColor) and isinstance(luma, bpy.types.NodeTreeInterfaceSocketFloat)
                ).map(lambda _: None)

            def edited() -> Outcome[None]:
                spaced = (field for field in msgspec.structs.fields(type(record.record)) if field.type is ColorSpace)
                for node in owned(group, identity).values():
                    if type(node) in editors.curves or isinstance(node, bpy.types.ShaderNodeValToRGB):
                        editors.drawn(layout, node, context)
                return accumulated(
                    pressed(
                        layout.row(align=True),
                        identity,
                        self.colorspace,
                        {"name": entry.name, "field": field.name, "space": getattr(record.record, field.name)},
                        text=bpy.path.display_name(field.name),
                        icon="NONE",
                    )
                    for field in spaced
                ).map(lambda _: None)

            def attached() -> Outcome[None]:
                def drawn[G: bpy.types.PropertyGroup](attachment: Attachment[G]) -> Outcome[None]:
                    def shown(held: Attached[G]) -> Outcome[None]:
                        holder = held.struct(scene)
                        return (
                            fields(held.form, identity)
                            .bind(lambda built: placed(built[0], identity).map(lambda draws: (tuple(draw(layout, holder, context) for draw in draws), built[1](layout, holder, context, entry.name))))
                            .map(lambda _: None)
                        )

                    return attachment.bound(identity).bind(shown)

                return accumulated(drawn(attachment) for part in row.found(Registers) for attachment in part.rows if isinstance(attachment, Attachment) and attachment.owner is bpy.types.Scene).map(
                    lambda _: None
                )

            interface: bpy.types.NodeTreeInterface = group.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return wheeled().bind(lambda _: edited()).bind(lambda _: attached()).bind(lambda _: listed(layout, entry, interface, context))

        def notices(layout: bpy.types.UILayout, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, row: CompositorEffect[Effect], context: bpy.types.Context) -> Outcome[None]:
            order = tuple(scene.compositor_effects)
            formed = chain.formation.entries(identity, scene)
            if row.domain is Domain.DISPLAY and (not formed or order.index(entry) < order.index(formed[0])):
                layout.label(text="Needs earlier Display View entry", icon="STATUS_ERROR_FILLED")

            def shown(config: PyOpenColorIO.Config, view: DisplayView) -> Outcome[None]:
                if row.record.stage is Stage.TONE:
                    return DisplayView.for_encoding(config, view.display, Some(view.view)).map(
                        lambda encoded: layout.label(text="Needs encoding-only Display View", icon="STATUS_ERROR_FILLED") if encoded.is_none() else None
                    )
                if (
                    row is chain.formation
                    and any(DynamicRange.of(space) is DynamicRange.HIGH for space in view.colorspace(config).to_list())
                    and not any(window.support_hdr_color for window in Option.of_optional(context.window).to_list())
                ):
                    split(layout)[1].label(text="OpenGL backend cannot show HDR" if gpu.platform.backend_type_get() == "OPENGL" else "Window cannot show HDR", icon="STATUS_INFO_FILLED")
                return Ok(None)

            return chain.view(identity, scene).bind(lambda view: shown(PyOpenColorIO.GetCurrentConfig(), view.default_with(lambda: viewed(scene))))

        def shown(editors: NodeEditors, layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
            match holder:
                case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, node_group=bpy.types.NodeTree() as group) as entry:
                    row = rows.row(identity, entry)
                    for offered in row.to_list():
                        alerted(
                            layout,
                            offered.read(identity, entry).bind(
                                lambda record: offered_drawn(layout, scene, entry, group, offered, record, context, editors).bind(lambda _: notices(layout, scene, entry, offered, context))
                            ),
                        )
                    alerted(
                        layout,
                        accumulated(
                            gates.bind(lambda held: accumulated(check(condition, context, Some(entry)) for condition in held.values()))
                            for pinned in rows.pinned
                            if row.is_none() and pinned.ownership.marked(identity, group).is_some()
                            for gates in pinned.conditions.map(lambda conditions: conditions(identity, entry)).to_list()
                        ),
                    )
                    buttons = layout.row(align=True)
                    alerted(
                        buttons,
                        accumulated((
                            pressed(buttons, identity, self.viewers.viewing, {"name": entry.name, "node": Viewers.entering.node, "socket": Viewers.entering.socket}, text="Input", icon="NONE"),
                            pressed(buttons, identity, self.viewers.viewing, {"name": entry.name, "node": Viewers.leaving.node, "socket": Viewers.leaving.socket}, text="Result", icon="NONE"),
                            pressed(buttons, identity, self.viewers.clearing, {"name": entry.name}, text="Clear", icon="NONE"),
                        )),
                    )
                case _:
                    pass

        return NodeEditors.realized(identity).map(lambda editors: self.conditioned(invoking(functools.partial(shown, editors))))


# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def _spaces(rows: tuple[tuple[str, str, str], ...]) -> tuple[tuple[str, str, str], ...]:
    """Holds colorspace enum rows so the strings Blender reads stay referenced."""
    return rows


def effect_section(chain: Chain, viewers: Viewers[Effect], *, thumbnail: Row | RowDraw, locks: Option[Locks] = Nothing) -> Single:
    """Builds the active effect section, open by default, with its colorspace command."""

    def choose_color_space(call: Call[SpaceField]) -> Outcome[Answer]:
        """Set the colorspace a record field reads."""
        properties = call.properties
        return call.root(bpy.types.Scene).bind(
            lambda scene: chain.rows.found(call.identity, scene, properties.name).bind(
                lambda held: revised(held.row, call.identity, scene, held.entry, lambda record: Ok(msgspec.structs.replace(record, **{properties.field: properties.space}))).map(
                    lambda wrote: Done() if wrote else Unchanged(reason=f"{held.entry.name} reads {properties.space}")
                )
            )
        )

    colorspace = Operator(form=SpaceField, run=choose_color_space, invoke=Some(InvokePopup.SEARCH), primary=Some("space"))
    return Single(
        key="active_effect",
        title="Active Effect",
        holder=Some(Member(root=bpy.types.Scene, path="compositor_effects.active")),
        condition=Some(composites),
        rows=(thumbnail, EffectBody(chain=chain, viewers=viewers, colorspace=colorspace, locks=locks)),
        header=(EffectButtons(viewers=viewers, locks=locks),),
        opened=True,
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EffectBody", "EffectButtons", "Locks", "SpaceField", "effect_section"]
