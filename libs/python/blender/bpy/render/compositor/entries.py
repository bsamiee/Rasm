"""Scene compositor entries as stack items: the enable flags an evaluation reads, entries created over a group at a stack index, moves to an index, the entries extension marks own, groups pinned at a stack end, and the scenes a render executes."""

from contextlib import ExitStack, nullcontext
from enum import auto, Enum, StrEnum
from functools import partial
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import called
from blender.bpy.data.ids.ownership import Mark, Ownership, tracked
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.extension.identity import Identity, Resource
from blender.records.faults import accumulated, Fault, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------


class Enabled(StrEnum):
    """Compositor effect enable flag an evaluation reads, valued by its RNA identifier."""

    RENDER = "enable_for_render"
    PREVIEW = "enable_for_preview"

    @property
    def descriptor(self) -> Descriptor[bpy.types.BoolProperty]:
        """Owning RNA property for reads and writes."""
        return Descriptor.declared(bpy.types.SceneCompositorEffect, (self.value,), bpy.types.BoolProperty)


class StackEnd(Enum):
    """Stack end holding an unranked pinned group."""

    HEAD = auto()
    TAIL = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

EXECUTING: Final = Resource[set[int]](made=lambda _identity: nullcontext(set()))

# --- [MODELS] ---------------------------------------------------------------------------


class Pinned(msgspec.Struct, frozen=True, kw_only=True):
    """Unranked owned group with stack end, enables, and the conditions its entry needs, its entry named after the ownership key unless titled."""

    ownership: Ownership[bpy.types.CompositorNodeTree]
    edge: StackEnd
    title: Option[str] = Nothing
    enables: frozenset[Enabled] = frozenset(Enabled)
    conditions: tuple[Condition, ...] = ()

    @property
    def named(self) -> str:
        """Entry title, the ownership key's display name unless stated."""
        return self.title.default_with(lambda: bpy.path.display_name(self.ownership.key))

    def checked(self, context: bpy.types.Context, entry: bpy.types.SceneCompositorEffect) -> Outcome[None]:
        """Checks every condition the pinned entry needs against the entry."""
        return accumulated(condition.check(context, Some(entry)) for condition in self.conditions).map(lambda _: None)


# --- [OPERATIONS] -----------------------------------------------------------------------


def added_entry(
    scope: ExitStack, scene: bpy.types.Scene, group: bpy.types.NodeTree, /, *, name: Option[str] = Nothing, at: Option[int] = Nothing, enables: frozenset[Enabled] = frozenset(Enabled)
) -> Outcome[bpy.types.SceneCompositorEffect]:
    """Creates a scene compositor entry running a group under `name`, the group's name unless stated, removed when `scope` closes, its enable flags written and moved to `at` when given.

    Caller keeping the entry past success passes `scope` to `retained`.

    Returns:
        `Ok` of the entry, `refused` when the host refuses the entry, and faults of its group, enable, and move writes.
    """
    created = catch(scene.compositor_effects.new, exception=RuntimeError)(name.default_value(group.name)).map_error(partial(Fault.of, Reason.REFUSED, repr(scene.compositor_effects)))
    return tracked(scope, scene.compositor_effects.remove, created).bind(
        lambda entry: (
            accumulated((written(entry, ("node_group",), group), *(enabled.descriptor.write(entry, enabled in enables) for enabled in Enabled)))
            .bind(lambda _: at.map(lambda index: relocated(scene, entry, index)).default_value(Ok(False)))
            .map(lambda _: entry)
        )
    )


def relocated(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, index: int) -> Outcome[bool]:
    """Moves entry to stack index unless it sits there, answering whether it moved."""
    return (
        Ok(False)
        if scene.compositor_effects.find(entry.name) == index
        else called(bpy.ops.scene.move_compositor_effect_to_index, {"name": entry.name, "index": index}, override=Some(bpy.context.temp_override(scene=scene))).map(lambda _: True)
    )


def marked(identity: Identity, scene: bpy.types.Scene) -> tuple[tuple[bpy.types.SceneCompositorEffect, Mark], ...]:
    """Returns the scene entries whose groups hold an extension ownership mark, with that mark, in stack order."""
    return tuple((entry, mark) for entry in scene.compositor_effects for group in Option.of_optional(entry.node_group).to_list() for mark in Mark.read(identity, group).to_list())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EXECUTING", "Enabled", "Pinned", "StackEnd", "added_entry", "marked", "relocated"]
