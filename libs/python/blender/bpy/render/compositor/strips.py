"""Sequencer strip modifiers and compositor strips running standalone effects."""

from contextlib import ExitStack
from functools import cached_property, partial
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from blender.bpy.commands.calls import Placement
from blender.bpy.commands.operators import Call, Done, Operator
from blender.bpy.data.rna.conditions import AllOf, Present, Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import rna, RnaPath, rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.desired import NodeGroup
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.edits import bound, Choice
from blender.bpy.render.compositor.effects import CompositorEffect, ownerships
from blender.bpy.render.compositor.entries import EffectRows
from blender.bpy.render.compositor.shells import PreviewFrame
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.surfaces.menus import Dropdown, Menu
from blender.records.faults import Fault, Faults, Outcome, paired
from blender.records.fields import Case

# --- [CONSTANTS] ------------------------------------------------------------------------

_ACTIVE: Final[RnaPath] = ("workspace", "sequencer_scene", "sequence_editor", "active_strip")
_STRIP: Final[Key] = "strip"

# --- [MODELS] ---------------------------------------------------------------------------


class Striping(Choice):
    """Effect placement on active sequencer strip."""

    placement: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Placement", items=choices((("MODIFIER", "Modifier", "Compositor strip modifier on the active strip"), ("STRIP", "Strip", "Compositor strip over the active strip")))
    )


# --- [SERVICES] -------------------------------------------------------------------------


class Strips[R: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Strip placement command for the standalone effects of a set of rows and the menu listing them."""

    placeable: ClassVar[AllOf] = Present(descriptor=Descriptor.declared(bpy.types.Context, _ACTIVE, bpy.types.PointerProperty)) & ~Setting(
        descriptor=Descriptor.declared(bpy.types.Context, (*_ACTIVE, "type"), bpy.types.EnumProperty), accepted=("SOUND",)
    )
    rows: EffectRows[R]

    @cached_property
    def placing(self) -> Operator[Striping]:
        """Command running the chosen standalone effect on the active strip, an item per standalone row under its key, title, and record documentation."""
        items = choices((row.kind, row.title, row.description) for row in self.rows.standalone)

        class Placing(Striping):
            kind: bpy.props.EnumProperty(name="Effect", description="Offered effect", items=items)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

        return Operator(condition=Some(Strips.placeable), form=Placing, run=bound(add_strip_effect, self.rows))

    @cached_property
    def menu(self) -> Menu:
        """Menu of every standalone effect, each placing it on the active strip."""
        return Menu(
            key=self.placing.key,
            title=bpy.path.display_name(self.placing.key),
            description=self.placing.description,
            condition=Some(Strips.placeable),
            body=Dropdown(entries=tuple(OperatorButton(target=Placement(self.placing, {"kind": row.kind}), keywords={"text": row.title}) for row in self.rows.standalone), search=True),
        )

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns strip group ownerships, the placement command, and its menu."""
        return Ok((*(msgspec.structs.replace(owner, key=f"{owner.key}_{_STRIP}") for row in self.rows.standalone for owner in ownerships(row.record)), self.placing, self.menu))


# --- [OPERATIONS] -----------------------------------------------------------------------


def add_strip_effect[R: Case](rows: EffectRows[R], call: Call[Striping]) -> Outcome[Done]:
    """Run the chosen effect on the active strip as a strip modifier or a strip over it."""
    target = active(call.context).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(call.context.workspace), member=rna(_ACTIVE[1:])))
    offered = Option.of_optional(rows.keyed.get(call.properties.kind)).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(tuple(rows.keyed)), member=call.properties.kind))

    def built(target: tuple[bpy.types.Scene, bpy.types.SequenceEditor, bpy.types.Strip], row: CompositorEffect[R], record: R) -> Outcome[Done]:
        scene, editor, strip = target

        with ExitStack() as rollback:

            def held() -> bpy.types.Strip | bpy.types.StripModifier:
                holder: bpy.types.Strip | bpy.types.StripModifier
                match call.properties.placement:
                    case "MODIFIER":
                        holder = strip.modifiers.new(name=row.title, type="COMPOSITOR")
                        rollback.callback(strip.modifiers.remove, holder)
                    case _:
                        holder = editor.strips.new_effect(name=row.title, type="COMPOSITOR", channel=strip.channel + 1, frame_start=strip.left_handle, length=strip.duration, input1=strip)
                        rollback.callback(editor.strips.remove, holder)
                return holder

            return (
                striped(call.identity, scene, row, record, PyOpenColorIO.GetCurrentConfig())
                .bind(lambda group: made(group, call.identity, of=Some(scene)))
                .bind(
                    lambda group: Result[bpy.types.Strip | bpy.types.StripModifier, Faults].bind(
                        catch(held, exception=RuntimeError)().map_error(partial(Fault.of, BlenderReason.REFUSED, repr(strip))), lambda holder: written(holder, ("node_group",), group)
                    )
                )
                .map(lambda _: rollback.pop_all())
                .map(lambda _: Done(notes=(("INFO", f"Added {row.title} to {strip.name}"),)))
            )

    return paired(target, offered).bind(lambda pair: rooted(bpy.types.Scene, call.context).bind(pair[1].seeded).bind(lambda record: built(pair[0], pair[1], record)))


def striped[R: Case](identity: Identity, scene: bpy.types.Scene, row: CompositorEffect[R], record: R, config: PyOpenColorIO.Config, *, preview: Option[PreviewFrame] = Nothing) -> Outcome[NodeGroup]:
    """Builds the record's effect group as strip modifier group, scene-linear in any sequencer space."""
    return row.grouped(identity, scene, Entry(record=record), config, preview=preview).map(lambda group: msgspec.structs.replace(group, key=f"{group.key}_{_STRIP}"))


def active(context: bpy.types.Context) -> Option[tuple[bpy.types.Scene, bpy.types.SequenceEditor, bpy.types.Strip]]:
    """Returns the workspace sequencer scene, its editor, and active strip."""
    return (
        Option
        .of_optional(context.workspace)
        .bind(lambda workspace: Option.of_optional(workspace.sequencer_scene))
        .bind(lambda scene: Option.of_optional(scene.sequence_editor).bind(lambda editor: Option.of_optional(editor.active_strip).map(lambda strip: (scene, editor, strip))))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Striping", "Strips", "active", "add_strip_effect", "striped"]
