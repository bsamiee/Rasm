"""Native sequencer groups around the offered compositor effects."""

from contextlib import ExitStack

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.graphs import NodeGroup
from blender.bpy.render.effects.stack import Context, EffectStack, ownerships, Placeable
from blender.imaging.compositing.blending import Entry
from blender.imaging.faults import paired
from blender.imaging.parameters import Choice

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
    """Native strip placement for the stack's standalone rows."""

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
                                        name=row.record.title, type="COMPOSITOR", channel=strip.channel + 1, frame_start=strip.left_handle, length=strip.duration, input1=strip
                                    )
                                    rollback.callback(editor.strips.remove, holder)
                            return holder

                        return (
                            striped(call.identity, scene, row, config)
                            .bind(lambda group: made(group, call.identity, of=Some(scene)))
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

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Retains strip ownership and placement commands."""
        return Ok((*(msgspec.structs.replace(owner, key=f"{owner.key}_strip") for row in self.stack.standalone for owner in ownerships(row.record)), *self.operators))


# --- [OPERATIONS] -----------------------------------------------------------------------


def striped(identity: Identity, scene: bpy.types.Scene, row: Placeable, config: PyOpenColorIO.Config, *, context: Context = Nothing) -> BlenderResult[NodeGroup]:
    """Builds the effect group as a strip group, sequencer groups taking and returning scene-linear values whatever the sequencer space."""
    return (
        row
        .seeded(scene)
        .bind(lambda effect: row.grouped(identity, scene, Entry(effect=effect), config, context=context))
        .bind(
            lambda group: Descriptor.resolve(bpy.types.CompositorNodeTree, "is_strip_modifier", bpy.types.BoolProperty).map(
                lambda trait: msgspec.structs.replace(group, key=f"{group.key}_strip", graph=msgspec.structs.replace(group.graph, members=group.graph.members | {trait: True}))
            )
        )
    )


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
