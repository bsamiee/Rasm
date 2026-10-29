"""Owned workspaces copied from the open file or appended from a blend file, with add-on filters, declared mode, scene pin and tool, and deferred activation."""

from contextlib import ExitStack
from functools import partial
from pathlib import Path
from typing import Self, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called, Operation
from blender.bpy.data.ids.ownership import kept, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import written
from blender.bpy.extension.events import Startup
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.files.libraries import blocks_loaded
from blender.records.faults import accumulated, Fault, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Activation(msgspec.Struct, frozen=True, kw_only=True):
    """Deferred workspace switch with prior workspace, running the setup's tab reorder operator and tool once it applies."""

    window: bpy.types.Window
    target: Handle[bpy.types.WorkSpace]
    prior: Handle[bpy.types.WorkSpace]
    setup: "WorkspaceSetup"

    def advanced(self) -> Outcome[Option[Self]]:
        """Returns switch while pending, Nothing after a user switch or once the switch applied and its reorder and tool ran."""
        if (shown := self.window.workspace.session_uid) != self.target.uid:
            return Ok(Some(self) if shown == self.prior.uid else Nothing)
        calls = (*((order, {}) for order in self.setup.order.to_list()), *((bpy.ops.wm.tool_set_by_id, {"name": name, "space_type": space}) for name, space in self.setup.tool.to_list()))
        return accumulated(called(operator, values, override=Some(bpy.context.temp_override(window=self.window))) for operator, values in calls).map(lambda _: Nothing)

    def reverted(self) -> Outcome[None]:
        """Requests prior workspace, applied next pass, fault once it no longer resolves."""
        return self.prior.resolve().bind(lambda workspace: written(self.window, ("workspace",), workspace)).map(lambda _: None)


class WorkspaceSetup(msgspec.Struct, frozen=True, kw_only=True):
    """Saved copy of a source workspace, from the open file or appended from `library`, with mode, scene pin, add-on filter, tab reorder operator (`workspace.reorder_to_front` or `reorder_to_back`), and the tool its mode starts with, by the idname and toolbar space `wm.tool_set_by_id` takes."""

    key: Key
    name: str
    source: str
    library: Option[Path] = Nothing
    mode: "Option[rna_enums.WorkspaceObjectModeItems]" = Nothing
    use_pin_scene: Option[bool] = Nothing
    owners: Option[tuple[str, ...]] = Nothing
    order: Option[Operation] = Nothing
    tool: "Option[tuple[str, rna_enums.SpaceTypeItems]]" = Nothing

    # --- [OWNERSHIP]

    @property
    def ownership(self) -> Ownership[bpy.types.WorkSpace]:
        """Workspace ownership, fake user keeping screens and marks in saved file."""
        return Ownership(kind=bpy.types.WorkSpace, key=self.key, saved=Saved.ALWAYS)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Declares ownership and add-on filters applied after registration and every file load."""
        return Ok((self.ownership, *(Startup(body=partial(self.filtered, identity)) for _ in self.owners.to_list())))

    # --- [FILTERING]

    def _configured(self, identity: Identity, workspace: bpy.types.WorkSpace) -> bpy.types.WorkSpace:
        """Enables add-on filter with declared owners, keeping user owners."""
        for extra in self.owners.to_list():
            present = {entry.name for entry in workspace.owner_ids}
            for owner in (owner for owner in dict.fromkeys((identity.package, *extra)) if owner not in present):
                workspace.owner_ids.new(owner)
            workspace.use_filter_by_owner = True
        return workspace

    def filtered(self, identity: Identity) -> Outcome[None]:
        """Applies add-on filter to owned workspaces."""
        for workspace, _ in self.ownership.held(identity):
            self._configured(identity, workspace)
        return Ok(None)

    # --- [CREATION]

    def made(self, identity: Identity) -> Outcome[bpy.types.WorkSpace]:
        """Returns owned workspace, a refused copy removed with its screens."""

        def declared(workspace: bpy.types.WorkSpace) -> Outcome[bpy.types.WorkSpace]:
            """Names the copy and writes declared object mode and scene pin."""
            return (
                catch(workspace.rename, exception=RuntimeError)(self.name, mode="NEVER")
                .map_error(partial(Fault.of, Reason.REFUSED, repr(workspace)))
                .bind(
                    lambda _: accumulated((
                        *(written(workspace, ("object_mode",), mode) for mode in self.mode.to_list()),
                        *(written(workspace, ("use_pin_scene",), pinned) for pinned in self.use_pin_scene.to_list()),
                    ))
                )
                .map(lambda _: workspace)
            )

        with ExitStack() as screens:

            def copied(workspace: bpy.types.WorkSpace) -> Outcome[bpy.types.WorkSpace]:
                """Holds removal of the copy's screens, removing the copy itself on a naming or declared-member refusal."""
                screens.callback(bpy.data.batch_remove, ids=tuple(workspace.screens))
                return kept(Ok(workspace), declared)

            def built() -> Outcome[bpy.types.WorkSpace]:
                """Appends the source workspace from the library, or copies it from the open file, with no layout switch."""
                return (
                    self.library
                    .map(lambda path: blocks_loaded(path, bpy.types.WorkSpace, (self.source,)).map(lambda blocks: Block.of_seq(blocks).try_head()))
                    .default_with(lambda: Ok(Option.of_optional(bpy.data.workspaces.get(self.source)).bind(lambda source: Option.of_optional(source.copy()))))
                    .bind(lambda found: found.to_result_with(partial(Fault.of, Reason.ABSENT, repr(bpy.data.workspaces), member=self.source)))
                    .bind(copied)
                )

            if (answer := self.ownership.made(identity, built).map(partial(self._configured, identity))).is_ok():
                screens.pop_all()
            return answer

    def activated(self, identity: Identity, window: bpy.types.Window) -> Outcome[Activation]:
        """Requests switch to owned workspace."""

        def switched(workspace: bpy.types.WorkSpace) -> Activation:
            """Takes handles before requesting switch."""
            activation = Activation(window=window, target=Handle.of(workspace), prior=Handle.of(window.workspace), setup=self)
            window.workspace = workspace
            return activation

        return self.made(identity).map(switched)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Activation", "WorkspaceSetup"]
