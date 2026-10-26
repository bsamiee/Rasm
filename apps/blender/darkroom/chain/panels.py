"""Active effect section of the chain beside the native scene effect stack: entry commands, graphed field editors, grading wheels, notices, setup locks, and Viewer buttons."""

from collections.abc import Callable, Mapping
import functools
from typing import Annotated, get_origin

import bpy
from expression import Nothing, Ok, Option, Some
import gpu
import msgspec
import msgspec.inspect
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, InvokePopup, Operator, Outcome, Own, Unchanged
from blender.bpy.data.rna.conditions import check
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.changes import owned
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.blending import Blend, Entry
from blender.bpy.render.compositor.effects import Blended, CompositorEffect, Enabled, entry_interface, expanded, HIDDEN, Measures, Named, Registers, Shell
from blender.bpy.render.compositor.entries import composites, Configured
from blender.bpy.render.compositor.inputs import active, entry_input, Field, node_fields, reached, titled
from blender.bpy.render.compositor.viewers import EntryPick, Viewers, ViewTarget
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.properties import Choices, SPACES
from blender.bpy.settings.shapes import GraphForm
from blender.bpy.ui.layout.rows import acquired, alerted, invoking, joined, Line, Member, OperatorButton, placed, pressed, realized, Row, RowContent, RowDraw, split
from blender.bpy.ui.layout.sections import fields, Single
from blender.bpy.ui.layout.templates import NodeEditors, Wheel
from blender.imaging.color.authoring import DynamicRange
from blender.imaging.color.processors import DisplayView, Domain
from blender.records.faults import accumulated, paired
from blender.records.fields import markers, Rgb

from .stack import Chain
from .stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type Locks = Callable[[Option[tuple[Key | int, ...]]], RowContent]

# --- [MODELS] ---------------------------------------------------------------------------


class SpaceField(Named, frozen=True, kw_only=True):
    """Entry graph field and the OCIO colorspace it reads."""

    field: str
    space: Annotated[str, SPACES] = ""


class EffectButtons(Line, frozen=True, kw_only=True):
    """Entry header actions the native effect header lacks: setup locks, the effect's own and Viewer commands, and reset."""

    viewers: Viewers[Effect]
    locks: Option[Locks] = Nothing

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Returns the header draw placing each owned entry's commands over its name."""
        rows = self.viewers.rows

        def shown(layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
            if isinstance(holder, bpy.types.SceneCompositorEffect):
                row = rows.row(identity, holder)
                actions = (
                    *(locks(Nothing) for locks in self.locks.to_list() if row.is_some()),
                    *(
                        OperatorButton(target=Own(command=command, values=command.form(name=holder.name)), text="")
                        for offered in row.to_list()
                        for command in (*(held for part in offered.found(Registers) for held in part.rows), *self.viewers.commands.get(offered.kind, ()))
                        if isinstance(command, Operator) and issubclass(command.form, Named)
                    ),
                    OperatorButton(target=Own(command=rows.operators.resetting, values=Named(name=holder.name)), text="", icon=Some("LOOP_BACK")),
                )
                alerted(layout, placed(actions, identity).map(lambda draws: joined(draws)(layout, holder, context)))

        return Ok(shown)


class EffectBody(Line, frozen=True, kw_only=True):
    """Entry editors, grading wheels, attachments, notices, setup locks, and Viewer buttons the native effect body lacks."""

    chain: Chain
    viewers: Viewers[Effect]
    colorspace: Operator[SpaceField]
    locks: Option[Locks] = Nothing

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the Viewer and colorspace commands and lock rows."""
        return Ok((self.viewers, self.colorspace, *acquired(tuple(locks(Nothing) for locks in self.locks.to_list()))))

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Returns the entry body draw over its record and owned nodes."""
        chain, rows = self.chain, self.viewers.rows
        record_key = next(field.name for field in msgspec.structs.fields(Configured) if get_origin(field.type) is Entry)
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
            editors: NodeEditors,
        ) -> BlenderResult[None]:
            values = tuple((value, (Shell.blend,) if isinstance(value, Blend) else (), field.name) for field in msgspec.structs.fields(Entry) for value in (getattr(record, field.name),))
            parameters = tuple((value, prefix, name, member) for value, prefix, name in values for member in Field.of(type(value), value=Some(value)))
            paths = {
                item_key(*prefix, *member.path[:depth]): (record_key, name, *(part for part in member.path[:depth] if not isinstance(part, type)))
                for _, prefix, name, member in parameters
                for depth in range(1, len(member.path) + 1)
            }
            paths[Shell.blend] = (record_key, next(name for value, _, name in values if isinstance(value, Blend)))
            hidden = frozenset(item_key(*prefix, *member.path) for value, prefix, _, member in parameters if not active(value, member.path))
            spaced = tuple(
                (key, titled(member.info), space)
                for value, prefix, _, member in parameters
                if GraphForm.COLOR_SPACE in member.forms
                for key in (item_key(*prefix, *member.path),)
                if key not in hidden
                for space in (reached(value, member.path),)
                if isinstance(space, str)
            )
            nodes = owned(group, identity)

            def graphed() -> BlenderResult[None]:
                keys = frozenset(key for key, _, _ in spaced)
                return accumulated((
                    *(pressed(layout.row(align=True), identity, self.colorspace, SpaceField(name=entry.name, field=key, space=space), text=title, icon="NONE") for key, title, space in spaced),
                    *(
                        editors.drawn(layout, node, edited, context)
                        for value, prefix, _ in values
                        for edited in node_fields(value, prefix)
                        if edited.node not in keys
                        for node in Option.of_optional(nodes.get(edited.node)).to_list()
                    ),
                )).map(lambda _: None)

            def wheeled() -> BlenderResult[None]:
                flow = layout.grid_flow(row_major=True, columns=2, even_columns=True)

                def bound(key: Key) -> BlenderResult[Member]:
                    return entry_input(identity, entry, key).map(lambda held: Member(root=type(held), path="value", reach=Some(lambda _context: Some(held))))

                def wheel(title: str, key: Key, shape: type[msgspec.Struct]) -> BlenderResult[None]:
                    color, luma = shape.__struct_fields__
                    inner = flow.column()
                    inner.label(text=title)
                    return (
                        paired(bound(item_key(key, color)), bound(item_key(key, luma)))
                        .bind(lambda pair: realized(Wheel(color=pair[0], luma=Some(pair[1])), identity))
                        .map(lambda draw: draw(inner, entry, context))
                    )

                return accumulated(
                    wheel(titled(member.info), key, type(held))
                    for value, prefix, _, member in parameters
                    for key in (item_key(*prefix, *member.path),)
                    if key not in hidden
                    for held in (reached(value, member.path),)
                    if isinstance(held, msgspec.Struct) and graded(held)
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

                return accumulated(drawn(attachment) for part in row.found(Registers) for attachment in part.rows if isinstance(attachment, Attachment) and attachment.owner is bpy.types.Scene).map(
                    lambda _: None
                )

            def locks_listed() -> BlenderResult[None]:
                graphs = tuple((titled(member.info), paths[key]) for value, prefix, _, member in parameters if member.graphed for key in (item_key(*prefix, *member.path),) if key not in hidden)
                return accumulated(
                    entry_interface(row.record, len(record.blend.masks), blended=isinstance(row.mix, Blended))
                    .bind(lambda face: expanded(face, record.record))
                    .bind(lambda face: listed(layout, entry, face, hidden, paths, graphs, context))
                    for _ in self.locks.to_list()
                ).map(lambda _: None)

            return wheeled().bind(lambda _: graphed()).bind(lambda _: attached()).bind(lambda _: locks_listed())

        def notices(
            layout: bpy.types.UILayout, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, row: CompositorEffect[Effect], record: Entry[Effect], context: bpy.types.Context
        ) -> BlenderResult[None]:
            order = tuple(scene.compositor_effects)
            formed = chain.formation.entries(identity, scene)
            if row.domain is Domain.DISPLAY and (not formed or order.index(entry) < order.index(formed[0])):
                layout.label(text="Needs earlier Display View entry", icon="STATUS_ERROR_FILLED")

            def shown(config: PyOpenColorIO.Config, view: DisplayView) -> BlenderResult[None]:
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

            return paired(row.config(record.record), chain.view(identity, scene)).bind(lambda parts: shown(parts[0], parts[1].default_with(lambda: viewed(scene))))

        def shown(editors: NodeEditors, layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
            match holder:
                case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, node_group=bpy.types.NodeTree() as group) as entry:
                    row = rows.row(identity, entry)
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
                            for pinned in rows.pinned
                            if row.is_none() and pinned.ownership.marked(identity, group).is_some()
                            for gates in pinned.conditions.map(lambda conditions: conditions(identity, entry)).to_list()
                        ),
                    )
                    buttons = layout.row(align=True)
                    alerted(
                        buttons,
                        accumulated((
                            pressed(buttons, identity, self.viewers.viewing, ViewTarget(name=entry.name, node=Viewers.entering.node, socket=Viewers.entering.socket), text="Input", icon="NONE"),
                            pressed(buttons, identity, self.viewers.viewing, ViewTarget(name=entry.name, node=Viewers.leaving.node, socket=Viewers.leaving.socket), text="Result", icon="NONE"),
                            pressed(buttons, identity, self.viewers.clearing, Named(name=entry.name), text="Clear", icon="NONE"),
                            *(pressed(buttons, identity, self.viewers.picking, EntryPick(name=entry.name), text="", icon="EYEDROPPER") for offered in row.to_list() if offered.found(Measures)),
                        )),
                    )
                case _:
                    pass

        return NodeEditors.realized(identity).map(lambda editors: invoking(functools.partial(shown, editors)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def graded(value: msgspec.Struct) -> bool:
    """Whether a record is a grading wheel: one RGB color with the luma component beside it."""
    match msgspec.inspect.type_info(type(value)):
        case msgspec.inspect.StructType(fields=(color, luma)):
            return isinstance(getattr(value, color.name), Rgb) and isinstance(getattr(value, luma.name), float)
        case _:
            return False


def effect_section(chain: Chain, viewers: Viewers[Effect], *, thumbnail: Row | RowDraw, locks: Option[Locks] = Nothing) -> Single:
    """Builds the active effect section, open by default, with its colorspace command."""

    def changed(call: Call[SpaceField]) -> BlenderResult[Outcome]:
        return call.root(bpy.types.Scene).bind(
            lambda scene: chain.rows.found(call.identity, scene, call.record.name).bind(
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
        rows=(thumbnail, EffectBody(chain=chain, viewers=viewers, colorspace=colorspace, locks=locks)),
        header=(EffectButtons(viewers=viewers, locks=locks),),
        opened=True,
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EffectBody", "EffectButtons", "Locks", "SpaceField", "effect_section", "graded"]
