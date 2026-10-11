"""Native sequencer groups around the offered compositor effects."""

from contextlib import ExitStack

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from rasm.blender.commands.operators import Call, Done, Operator
from rasm.blender.data.rna.descriptors import Descriptor, written
from rasm.blender.extension.events.subscriptions import Subscription
from rasm.blender.extension.identity import Identity
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from rasm.blender.nodes.graphs import End, INPUTS, InterfacePanel, nested, NodeGroup, Operand, Socket, SocketSide
from rasm.blender.render.color.spaces import converted, pointwise
from rasm.blender.render.effects.stack import Context, EffectStack, ownerships, Placeable, Shell
from rasm.imaging.compositing.blending import Entry
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Choice

# --- [TYPES] ----------------------------------------------------------------------------


class Placing(Choice):
    """Native sequencer operation receiving a compositor group."""

    MODIFIER = 1, "Modifier"
    STRIP = 2, "Compositor Strip"


# --- [MODELS] ---------------------------------------------------------------------------


class Striping(msgspec.Struct, frozen=True, kw_only=True):
    """Placement of an offered effect on the active sequencer strip."""

    placement: Placing = Placing.MODIFIER


# --- [SERVICES] -------------------------------------------------------------------------


class StripEffect:
    """Native strip placement and space refresh for the stack's standalone rows."""

    stack: EffectStack
    operators: tuple[Operator[Striping], ...]

    def __init__(self, *, stack: EffectStack) -> None:
        """Declares one native placement command per offered standalone row."""
        self.stack = stack

        def offered(row: Placeable) -> Operator[Striping]:
            def run(call: Call[Striping]) -> BlenderResult[Done]:
                target: BlenderResult[tuple[bpy.types.Scene, bpy.types.SequenceEditor, bpy.types.Strip]] = (
                    Option
                    .of_optional(call.context.workspace)
                    .bind(lambda workspace: Option.of_optional(workspace.sequencer_scene))
                    .bind(lambda scene: Option.of_optional(scene.sequence_editor).bind(lambda editor: Option.of_optional(editor.active_strip).map(lambda strip: (scene, editor, strip))))
                    .to_result_with(lambda: (BlenderAbsentFault(owner=repr(call.context.workspace), key="sequencer_scene.sequence_editor.active_strip"),))
                )

                def built(target: tuple[bpy.types.Scene, bpy.types.SequenceEditor, bpy.types.Strip], config: PyOpenColorIO.Config) -> BlenderResult[Done]:
                    scene, editor, strip = target

                    with ExitStack() as rollback:

                        def held() -> bpy.types.Strip | bpy.types.StripModifier:
                            holder: bpy.types.Strip | bpy.types.StripModifier
                            match call.record.placement:
                                case Placing.MODIFIER:
                                    holder = strip.modifiers.new(name=row.record.title, type="COMPOSITOR")
                                    rollback.callback(strip.modifiers.remove, holder)
                                case Placing.STRIP:
                                    holder = editor.strips.new_effect(
                                        name=row.record.title, type="COMPOSITOR", channel=strip.channel + 1, frame_start=strip.frame_final_start, length=strip.frame_final_duration, input1=strip
                                    )
                                    rollback.callback(editor.strips.remove, holder)
                            return holder

                        return (
                            striped(call.identity, scene, row, config)
                            .bind(lambda group: group.made(call.identity, of=Some(scene)))
                            .bind(
                                lambda group: Result[bpy.types.Strip | bpy.types.StripModifier, BlenderFaults].bind(
                                    catch(held, exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(strip), error=error),)),
                                    lambda holder: written(holder, "node_group", group),
                                )
                            )
                            .map(lambda _: rollback.pop_all())
                            .map(lambda _: Done(notes=(("INFO", f"Added {row.record.title} to {strip.name}"),)))
                        )

                return paired(target, self.stack.config()).bind(lambda pair: built(*pair))

            return Operator(key=f"strip_{row.kind}", title=f"Add {row.record.title}", description=f"Runs {row.record.title} on the active sequencer strip", form=Striping, run=run)

        self.operators = tuple(map(offered, stack.standalone))

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Retains strip ownership, placement commands, and native space-change refresh."""

        def refreshed() -> BlenderResult[None]:
            def scene_groups(scene: bpy.types.Scene, row: Placeable, config: PyOpenColorIO.Config) -> BlenderResult[None]:
                held = tuple(tree for owner in ownerships(row.record) for tree, mark in msgspec.structs.replace(owner, key=f"{owner.key}_strip").held(identity) if mark.of == Some(scene))
                return (
                    striped(identity, scene, row, config).bind(lambda group: accumulated(group.graph.reconciled(tree, identity, group.interface) for tree in held)).map(lambda _: None)
                    if held
                    else Ok(None)
                )

            return self.stack.config().bind(lambda config: accumulated(scene_groups(scene, row, config) for scene in bpy.data.scenes for row in self.stack.standalone)).map(lambda _: None)

        return Ok((
            *(msgspec.structs.replace(owner, key=f"{owner.key}_strip") for row in self.stack.standalone for owner in ownerships(row.record)),
            *self.operators,
            Subscription(owner=bpy.types.ColorManagedSequencerColorspaceSettings, setting=bpy.types.ColorManagedSequencerColorspaceSettings.bl_rna.properties["name"], body=refreshed),
        ))


# --- [OPERATIONS] -----------------------------------------------------------------------


def striped(identity: Identity, scene: bpy.types.Scene, row: Placeable, config: PyOpenColorIO.Config, *, context: Context = Nothing) -> BlenderResult[NodeGroup]:
    """Builds the full effect interface between sequencer-space conversions."""
    decode, effect, encode = "decode", "effect", "encode"
    space = scene.sequencer_colorspace_settings.name  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    def wrapped(group: NodeGroup) -> BlenderResult[NodeGroup]:
        def fed(item: tuple[Socket, SocketSide] | InterfacePanel) -> Option[End]:
            match item:
                case (Socket() as socket, "INPUT"):
                    return Some(End(node=INPUTS, socket=socket.key))
                case InterfacePanel(key=key, toggle=toggle):
                    return toggle.map(lambda _: End(node=INPUTS, socket=key))
                case _:
                    return Nothing

        forwarded: dict[str, Operand] = {key: value for key, item, _, _ in group.interface.walked() for value in fed(item).to_list()}
        return (
            pointwise(decode, End(node=INPUTS, socket=Shell.image), lambda source: converted(decode, config, space, PyOpenColorIO.ROLE_SCENE_LINEAR, source))
            .bind(lambda entering: nested(effect, group, {**forwarded, Shell.image: entering}, Shell.result))
            .bind(lambda result: pointwise(encode, result, lambda source: converted(encode, config, PyOpenColorIO.ROLE_SCENE_LINEAR, space, source)))
            .bind(lambda result: NodeGroup.built(f"{group.key}_strip", group.name, group.interface, {Shell.result: result}))
            .bind(
                lambda result: Descriptor.resolve(bpy.types.CompositorNodeTree, "is_strip_modifier", bpy.types.BoolProperty).map(
                    lambda trait: msgspec.structs.replace(result, single=group.single, graph=msgspec.structs.replace(result.graph, members=result.graph.members | {trait: True}))
                )
            )
        )

    return row.grouped(identity, scene, Entry(effect=row.seeded(scene)), config, context=context).bind(wrapped)


def active(context: bpy.types.Context) -> Option[bpy.types.CompositorStrip]:
    """Returns the sequencer scene's active native compositor strip."""
    scene = Option.of_optional(context.workspace).bind(lambda workspace: Option.of_optional(workspace.sequencer_scene))
    return (
        scene
        .bind(lambda scene: Option.of_optional(scene.sequence_editor))
        .bind(lambda editor: Option.of_optional(editor.active_strip))
        .bind(lambda strip: Some(strip) if isinstance(strip, bpy.types.CompositorStrip) else Nothing)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Placing", "StripEffect", "Striping", "active", "striped"]
