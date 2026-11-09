"""Owned workspaces and their deferred activation."""

from contextlib import ExitStack
from functools import partial
from pathlib import Path
from typing import Self, TYPE_CHECKING

import bpy
from expression import Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands.operators import called, Operation
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import _Startup, Acquisitions
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.ownership import _kept, Ownership, Saved
from rasm_bpy.data._ids.store import Handle
from rasm_bpy.data._rna.descriptors import written
from rasm_bpy.files._libraries import _blocks_loaded
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class _Activation(msgspec.Struct, frozen=True, kw_only=True):
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
    """Saved copy of a source workspace, from the open file or appended from `library`, with mode, scene pin, add-on filter, tab reorder operator, and the tool its mode starts with as the idname and toolbar space `wm.tool_set_by_id` takes."""

    # --- [STATE]

    key: Key
    name: str
    source: str
    library: Option[Path] = Nothing
    mode: "Option[rna_enums.WorkspaceObjectModeItems]" = Nothing
    use_pin_scene: Option[bool] = Nothing
    owners: Option[tuple[str, ...]] = Nothing
    order: Option[Operation] = Nothing
    tool: "Option[tuple[str, rna_enums.SpaceTypeItems]]" = Nothing

    # --- [REGISTRATION]

    @property
    def ownership(self) -> Ownership[bpy.types.WorkSpace]:
        """Workspace ownership, fake user keeping screens and marks in saved file."""
        return Ownership(kind=bpy.types.WorkSpace, key=self.key, saved=Saved.ALWAYS)

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Declares ownership and add-on filters applied after registration and every file load."""
        return (self.ownership, *(_Startup(body=partial(self.filtered, identity)) for _ in self.owners.to_list()))

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
                .map_error(partial(Fault.of, Refusal.REFUSED, repr(workspace)))
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
                return _kept(Ok(workspace), declared)

            def built() -> Outcome[bpy.types.WorkSpace]:
                """Appends the source workspace from the library, or copies it from the open file, with no layout switch."""
                return (
                    self.library
                    .map(lambda path: _blocks_loaded(path, bpy.types.WorkSpace, (self.source,)).map(lambda blocks: Block.of_seq(blocks).try_head()))
                    .default_with(lambda: Ok(Option.of_optional(bpy.data.workspaces.get(self.source)).bind(lambda source: Option.of_optional(source.copy()))))
                    .bind(lambda found: found.to_result_with(partial(Fault.of, Reason.ABSENT, repr(bpy.data.workspaces), member=self.source)))
                    .bind(copied)
                )

            if (answer := self.ownership.made(identity, built).map(partial(self._configured, identity))).is_ok():
                screens.pop_all()
            return answer

    def activated(self, identity: Identity, window: bpy.types.Window) -> Outcome[_Activation]:
        """Requests switch to owned workspace."""

        def switched(workspace: bpy.types.WorkSpace) -> _Activation:
            """Takes handles before requesting switch."""
            activation = _Activation(window=window, target=Handle.of(workspace), prior=Handle.of(window.workspace), setup=self)
            window.workspace = workspace
            return activation

        return self.made(identity).map(switched)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["WorkspaceSetup"]
