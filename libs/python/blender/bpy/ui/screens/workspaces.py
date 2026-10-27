"""Owned workspaces with add-on filters and deferred activation."""

from contextlib import ExitStack
from functools import partial
from typing import Self, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called, Operation
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import written
from blender.bpy.extension.events import startup
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Faults, Outcome

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Activation(msgspec.Struct, frozen=True, kw_only=True):
    """Deferred workspace switch with prior workspace and the tab reorder operator run once it applies."""

    window: bpy.types.Window
    target: Handle[bpy.types.WorkSpace]
    prior: Handle[bpy.types.WorkSpace]
    order: Option[Operation]

    def advanced(self) -> Outcome[Option[Self]]:
        """Returns switch while pending, Nothing after activation or user switch."""
        shown = self.window.workspace.session_uid
        return (
            self.order.map(lambda order: called(order, {}, override=Some(bpy.context.temp_override(window=self.window))).map(lambda _: Nothing)).default_value(Ok(Nothing))
            if shown == self.target.uid
            else Ok(Some(self) if shown == self.prior.uid else Nothing)
        )

    def reverted(self) -> Outcome[None]:
        """Requests prior workspace, fault once it no longer resolves."""

        def assigned(workspace: bpy.types.WorkSpace) -> None:
            """Writes prior workspace, applied next pass."""
            self.window.workspace = workspace

        return self.prior.resolve().map(assigned)


class WorkspaceSetup(msgspec.Struct, frozen=True, kw_only=True):
    """Saved copy of source workspace with mode, add-on filter, and tab reorder operator (`workspace.reorder_to_front` or `reorder_to_back`)."""

    key: Key
    name: str
    source: str
    mode: "Option[rna_enums.WorkspaceObjectModeItems]" = Nothing
    owners: Option[tuple[str, ...]] = Nothing
    order: Option[Operation] = Nothing

    # --- [OWNERSHIP]

    @property
    def ownership(self) -> Ownership[bpy.types.WorkSpace]:
        """Workspace ownership, fake user keeping screens and marks in saved file."""
        return Ownership(kind=bpy.types.WorkSpace, key=self.key, saved=Saved.ALWAYS)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Declares ownership and add-on filters applied after registration and every file load."""
        return Ok((self.ownership, *(row for _ in self.owners.to_list() for row in startup(partial(self.filtered, identity)))))

    # --- [FILTERING]

    def _configured(self, identity: Identity, workspace: bpy.types.WorkSpace) -> bpy.types.WorkSpace:
        """Enables add-on filter with declared owners, keeping user owners."""
        for extra in self.owners.to_list():
            listed = dict.fromkeys((identity.package, *extra))
            present = {entry.name for entry in workspace.owner_ids}
            for owner in (owner for owner in listed if owner not in present):
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
        """Returns owned workspace."""

        def moded(workspace: bpy.types.WorkSpace) -> Outcome[bpy.types.WorkSpace]:
            """Writes declared object mode."""
            return accumulated(written(workspace, "object_mode", mode) for mode in self.mode.to_list()).map(lambda _: workspace)

        def built() -> Outcome[bpy.types.WorkSpace]:
            """Copies source workspace with no layout switch."""

            def copied(workspace: bpy.types.WorkSpace) -> Outcome[bpy.types.WorkSpace]:
                """Removes copy on naming or mode refusal, screens on any refusal."""
                screens.callback(bpy.data.batch_remove, ids=tuple(workspace.screens))
                with ExitStack() as pending:
                    pending.callback(bpy.data.batch_remove, ids=(workspace,))
                    named = catch(workspace.rename, exception=RuntimeError)(self.name, mode="NEVER").map_error(partial(Fault.of, BlenderReason.REFUSED, repr(workspace)))
                    if (answer := Result[object, Faults].bind(named, lambda _: moded(workspace))).is_ok():
                        pending.pop_all()
                    return answer

            return (
                Option
                .of_optional(bpy.data.workspaces.get(self.source))
                .bind(lambda source: Option.of_optional(source.copy()))
                .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(bpy.data.workspaces), member=self.source))
                .bind(copied)
            )

        with ExitStack() as screens:
            if (answer := self.ownership.made(identity, built).map(lambda workspace: self._configured(identity, workspace))).is_ok():
                screens.pop_all()
            return answer

    def activated(self, identity: Identity, window: bpy.types.Window) -> Outcome[Activation]:
        """Requests switch to owned workspace."""

        def switched(workspace: bpy.types.WorkSpace) -> Activation:
            """Takes handles before requesting switch."""
            activation = Activation(window=window, target=Handle.of(workspace), prior=Handle.of(window.workspace), order=self.order)
            window.workspace = workspace
            return activation

        return self.made(identity).map(switched)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Activation", "WorkspaceSetup"]
