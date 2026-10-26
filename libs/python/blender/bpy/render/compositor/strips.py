"""Sequencer strip modifiers and compositor strips running standalone effects."""

from contextlib import ExitStack
from typing import ClassVar

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator
from blender.bpy.data.ids.store import vacant
from blender.bpy.data.rna.conditions import AllOf, Not, Present, Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.graphs import NodeGroup
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import CompositorEffect, Context, ownerships
from blender.bpy.render.compositor.entries import EffectRows
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.surfaces.menus import Dropdown, Menu
from blender.records.faults import paired
from blender.records.fields import Case, Choice

# --- [TYPES] ----------------------------------------------------------------------------


class Placing(Choice):
    """Sequencer holder receiving effect group."""

    MODIFIER = 1, "Modifier"
    STRIP = 2, "Compositor Strip"


# --- [MODELS] ---------------------------------------------------------------------------


class Striping(msgspec.Struct, frozen=True, kw_only=True):
    """Effect placement on active sequencer strip."""

    placement: Placing = Placing.MODIFIER


# --- [SERVICES] -------------------------------------------------------------------------


class Strips[R: Case]:
    """Strip placement commands for standalone effects and the menu listing them."""

    placeable: ClassVar[AllOf] = AllOf(
        conditions=(
            Present(descriptor=Descriptor.declared(bpy.types.Context, "workspace.sequencer_scene.sequence_editor.active_strip", bpy.types.PointerProperty)),
            Not(condition=Setting(descriptor=Descriptor.declared(bpy.types.Context, "workspace.sequencer_scene.sequence_editor.active_strip.type", bpy.types.EnumProperty), accepted=("SOUND",))),
        )
    )
    rows: EffectRows[R]
    operators: tuple[Operator[Striping], ...]
    menu: Menu

    def __init__(self, *, rows: EffectRows[R]) -> None:
        """Declares one placement command per standalone effect."""
        self.rows = rows

        def offered(row: CompositorEffect[R]) -> Operator[Striping]:
            def run(call: Call[Striping]) -> BlenderResult[Done]:
                target = active(call.context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(call.context.workspace), key="sequencer_scene.sequence_editor.active_strip"),))

                def built(target: tuple[bpy.types.Scene, bpy.types.SequenceEditor, bpy.types.Strip], record: R) -> BlenderResult[Done]:
                    scene, editor, strip = target

                    with ExitStack() as rollback:

                        def held() -> bpy.types.Strip | bpy.types.StripModifier:
                            holder: bpy.types.Strip | bpy.types.StripModifier
                            match call.record.placement:
                                case Placing.MODIFIER:
                                    holder = strip.modifiers.new(name=vacant(row.record.title, (modifier.name for modifier in strip.modifiers)), type="COMPOSITOR")
                                    rollback.callback(strip.modifiers.remove, holder)
                                case Placing.STRIP:
                                    holder = editor.strips.new_effect(
                                        name=vacant(row.record.title, (held.name for held in editor.strips_all)),
                                        type="COMPOSITOR",
                                        channel=strip.channel + 1,
                                        frame_start=strip.left_handle,
                                        length=strip.duration,
                                        input1=strip,
                                    )
                                    rollback.callback(editor.strips.remove, holder)
                            return holder

                        return (
                            row
                            .config(record)
                            .bind(lambda config: striped(call.identity, scene, row, record, config))
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

                return paired(target, call.root(bpy.types.Scene).bind(row.seeded)).bind(lambda pair: built(*pair))

            return Operator(key=f"strip_{row.kind}", title=f"Add {row.record.title}", description=f"Run {row.record.title} on active strip", condition=Some(Strips.placeable), form=Striping, run=run)

        self.operators = tuple(map(offered, rows.standalone))
        self.menu = Menu(
            key="strip_effects",
            title="Effects",
            description="Add an effect to the active strip",
            condition=Some(Strips.placeable),
            body=Dropdown(entries=tuple(OperatorButton(target=command, text=row.record.title) for row, command in zip(rows.standalone, self.operators)), search=True),
        )

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns group ownerships and the menu holding placement commands."""
        return Ok((*(msgspec.structs.replace(owner, key=f"{owner.key}_strip") for row in self.rows.standalone for owner in ownerships(row.record)), self.menu))


# --- [OPERATIONS] -----------------------------------------------------------------------


def striped[R: Case](identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[R], record: R, config: PyOpenColorIO.Config, *, context: Context = Nothing) -> BlenderResult[NodeGroup]:
    """Builds the record's effect group as strip modifier group, scene-linear in any sequencer space."""
    return row.grouped(identity, scene, Entry(record=record), config, context=context).map(lambda group: msgspec.structs.replace(group, key=f"{group.key}_strip"))


def active(context: bpy.types.Context) -> Option[tuple[bpy.types.Scene, bpy.types.SequenceEditor, bpy.types.Strip]]:
    """Returns the workspace sequencer scene, its editor, and active strip."""
    return (
        Option
        .of_optional(context.workspace)
        .bind(lambda workspace: Option.of_optional(workspace.sequencer_scene))
        .bind(lambda scene: Option.of_optional(scene.sequence_editor).bind(lambda editor: Option.of_optional(editor.active_strip).map(lambda strip: (scene, editor, strip))))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Placing", "Striping", "Strips", "active", "striped"]
