"""Scene compositor entries as stack items: the enable flags an evaluation reads, entries created over a group at a stack index, moves to an index, the entries extension marks own, groups pinned at a stack end, and the scenes a render executes."""

from collections.abc import Generator, Iterator
from contextlib import contextmanager, ExitStack, nullcontext
from enum import auto, Enum, StrEnum
from functools import partial
from typing import Final

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands.operators import called
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Resource
from rasm_bpy.data._ids.ownership import _tracked, Mark, Ownership, Saved
from rasm_bpy.data._rna.conditions import Condition, Setting
from rasm_bpy.data._rna.descriptors import Descriptor, written
from rasm_bpy.nodes import functions
from rasm_bpy.nodes._graphs import Expr, group_input
from rasm_bpy.nodes._interfaces import Interface
from rasm_bpy.nodes.changes import made
from rasm_bpy.render._compositor.colors import COLOR_INPUT, COLOR_RESULT
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------


class Enabled(StrEnum):
    """Compositor effect enable flag an evaluation reads, valued by its RNA identifier."""

    RENDER = "enable_for_render"
    PREVIEW = "enable_for_preview"

    @property
    def descriptor(self) -> Descriptor:
        """Owning RNA property for reads and writes."""
        return Descriptor(root=bpy.types.SceneCompositorEffect, prop=bpy.types.SceneCompositorEffect.bl_rna.properties[self.value])


class StackEnd(Enum):
    """Stack end holding an unranked pinned group."""

    HEAD = auto()
    TAIL = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_EXECUTING: Final = Resource[set[int]](made=lambda _identity: nullcontext(set()))
_composites: Final = Setting(descriptor=Descriptor(root=bpy.types.Scene, owner=("render",), prop=bpy.types.RenderSettings.bl_rna.properties["use_compositing"]))

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


def _added_entry(
    scope: ExitStack, scene: bpy.types.Scene, group: bpy.types.NodeTree, /, *, name: Option[str] = Nothing, at: Option[int] = Nothing, enables: frozenset[Enabled] = frozenset(Enabled)
) -> Outcome[bpy.types.SceneCompositorEffect]:
    """Creates a scene compositor entry running a group under `name`, the group's name unless stated, removed when `scope` closes, its enable flags written and moved to `at` when given.

    Caller keeping the entry past success passes `scope` to `retained`.

    Returns:
        `Ok` of the entry, `refused` when the host refuses the entry, and faults of its group, enable, and move writes.
    """
    created = catch(scene.compositor_effects.new, exception=RuntimeError)(name.default_value(group.name)).map_error(partial(Fault.of, Reason.REFUSED, repr(scene.compositor_effects)))
    return _tracked(scope, scene.compositor_effects.remove, created).bind(
        lambda entry: (
            accumulated((written(entry, ("node_group",), group), *(enabled.descriptor.write(entry, enabled in enables) for enabled in Enabled)))
            .bind(lambda _: at.map(lambda index: _relocated(scene, entry, index)).default_value(Ok(value=False)))
            .map(lambda _: entry)
        )
    )


def _relocated(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, index: int) -> Outcome[bool]:
    """Moves entry to stack index unless it sits there, answering whether it moved."""
    return (
        Ok(value=False)
        if scene.compositor_effects.find(entry.name) == index
        else called(bpy.ops.scene.move_compositor_effect_to_index, {"name": entry.name, "index": index}, override=Some(bpy.context.temp_override(scene=scene))).map(lambda _: True)
    )


def _marked(identity: Identity, scene: bpy.types.Scene) -> Iterator[tuple[bpy.types.SceneCompositorEffect, Mark]]:
    """Returns the scene entries whose groups hold an extension ownership mark, with that mark, in stack order."""
    return ((entry, mark) for entry in scene.compositor_effects for group in Option.of_optional(entry.node_group).to_list() for mark in Mark.read(identity, group).to_list())


@contextmanager
def capturing(
    identity: Identity, scene: bpy.types.Scene, before: bpy.types.SceneCompositorEffect, output: tuple[Expr, ...], *, stop: bool = False
) -> Generator[Outcome[bpy.types.SceneCompositorEffect]]:
    """Places scoped pass-through outputs before an entry and restores enabled and active entries on exit."""
    at = tuple(scene.compositor_effects).index(before)
    interface = Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,))
    with ExitStack() as scope:
        for active in Option.of_optional(scene.compositor_effects.active).to_list():
            scope.callback(setattr, active, "is_active", True)  # ruff: ignore[boolean-positional-value-in-call]
        suffix = tuple(scene.compositor_effects)[at:] if stop else ()
        for entry in suffix:
            scope.callback(setattr, entry, "enable_for_render", entry.enable_for_render)

        def placed(tree: bpy.types.NodeTree) -> Outcome[bpy.types.SceneCompositorEffect]:
            scope.callback(bpy.data.node_groups.remove, tree)
            return _added_entry(scope, scene, tree, at=Some(at))

        yield (
            _composites
            .check(bpy.context, Some(scene))
            .bind(lambda _: accumulated(written(entry, ("enable_for_render",), value=False) for entry in suffix))
            .bind(lambda _: functions.built("input_capture", interface, {COLOR_RESULT.key: group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor).keeping(*output)}, bpy.types.CompositorNodeTree))
            .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
            .bind(lambda group: made(group, identity, of=Some(scene)))
            .bind(placed)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Enabled", "Pinned", "StackEnd", "capturing"]
