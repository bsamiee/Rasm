"""Owned native workspaces, owner filters, and deferred activation and tab ordering."""

from contextlib import ExitStack
from enum import auto, Enum
from typing import Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from rasm.blender.commands.calls import called
from rasm.blender.data.ids.ownership import Ownership, Saved
from rasm.blender.data.ids.store import Handle, named
from rasm.blender.data.rna.descriptors import written
from rasm.blender.extension.events.handlers import PathHandler
from rasm.blender.extension.events.timers import Timer
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.extension.registration import Acquisitions, Registrable
from rasm.blender.faults import BlenderAbsentFault, BlenderResult
from rasm.imaging.faults import accumulated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class TabOrder(Enum):
    """Ends of the native workspace tab order."""

    FRONT = auto()
    BACK = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Activation(msgspec.Struct, frozen=True, kw_only=True):
    """Deferred native workspace switch with its prior workspace and requested tab end."""

    window: bpy.types.Window
    target: Handle[bpy.types.WorkSpace]
    prior: Handle[bpy.types.WorkSpace]
    order: Option[TabOrder]

    def advanced(self) -> BlenderResult[Option[Self]]:
        """Returns this switch while pending, or Nothing after activation or a user's switch."""

        def reordered(order: TabOrder) -> BlenderResult[None]:
            """Orders the shown workspace through its native window-only operator context."""
            return called(bpy.ops.workspace.reorder_to_front if order is TabOrder.FRONT else bpy.ops.workspace.reorder_to_back, {}, override=Some(bpy.context.temp_override(window=self.window))).map(
                lambda _: None
            )

        shown = self.window.workspace.session_uid
        return self.order.map(reordered).default_value(Ok(None)).map(lambda _: Nothing) if shown == self.target.uid else Ok(Some(self) if shown == self.prior.uid else Nothing)

    def reverted(self) -> BlenderResult[None]:
        """Requests the prior workspace, or returns its expired session identity."""

        def assigned(workspace: bpy.types.WorkSpace) -> None:
            """Passes the resolved prior workspace to Blender's deferred setter."""
            self.window.workspace = workspace

        return self.prior.resolve().map(assigned)


class WorkspaceSetup(msgspec.Struct, frozen=True, kw_only=True):
    """Saved workspace copied from a native source with optional mode and extension filter."""

    key: Key
    name: str
    source: str
    mode: "Option[rna_enums.WorkspaceObjectModeItems]" = Nothing
    owners: Option[tuple[str, ...]] = Nothing
    order: Option[TabOrder] = Nothing

    # --- [OWNERSHIP]

    @property
    def ownership(self) -> Ownership[bpy.types.WorkSpace]:
        """Workspace ownership whose fake user retains its screens and marks in the file."""
        return Ownership(kind=bpy.types.WorkSpace, key=self.key, saved=Saved.ALWAYS)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Declares ownership and file-load and first-pass owner filters for registration."""
        listed: tuple[Registrable, ...] = (
            (
                PathHandler(handlers=bpy.app.handlers.load_post, body=lambda _: self.filtered(identity)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                Timer(body=lambda: self.filtered(identity).map(lambda _: Nothing)),
            )
            if self.owners.is_some()
            else ()
        )
        return Ok((self.ownership, *listed))

    # --- [FILTERING]

    def _configured(self, identity: Identity, workspace: bpy.types.WorkSpace) -> bpy.types.WorkSpace:
        """Adds the configured owners without removing owners the user already selected."""
        for extra in self.owners.to_list():
            listed = dict.fromkeys((identity.package, *extra))
            present = {entry.name for entry in workspace.owner_ids}
            for owner in (owner for owner in listed if owner not in present):
                workspace.owner_ids.new(owner)
            workspace.use_filter_by_owner = True
        return workspace

    def filtered(self, identity: Identity) -> BlenderResult[None]:
        """Refreshes this row's saved workspaces after registration or file loading."""
        for workspace, _ in self.ownership.held(identity):
            self._configured(identity, workspace)
        return Ok(None)

    # --- [CREATION]

    def made(self, identity: Identity) -> BlenderResult[bpy.types.WorkSpace]:
        """Returns the owned workspace, or an absent source or native name collision."""

        def moded(workspace: bpy.types.WorkSpace) -> BlenderResult[bpy.types.WorkSpace]:
            """Sets the copied workspace's declared native activation mode."""
            return accumulated(written(workspace, "object_mode", mode) for mode in self.mode.to_list()).map(lambda _: workspace)

        def built() -> BlenderResult[bpy.types.WorkSpace]:
            """Copies native screens and tools without requesting a pending layout switch."""

            def copied(workspace: bpy.types.WorkSpace) -> BlenderResult[bpy.types.WorkSpace]:
                """Owns copied screens through the complete marked workspace acquisition."""
                screens.callback(bpy.data.batch_remove, ids=tuple(workspace.screens))
                with ExitStack() as pending:
                    pending.callback(bpy.data.batch_remove, ids=(workspace,))
                    if (answer := named(workspace, self.name).bind(moded)).is_ok():
                        pending.pop_all()
                    return answer

            return (
                Option
                .of_optional(bpy.data.workspaces.get(self.source))
                .bind(lambda source: Option.of_optional(source.copy()))
                .map(copied)
                .default_with(lambda: Error((BlenderAbsentFault(owner=repr(bpy.data.workspaces), key=self.source),)))
            )

        with ExitStack() as screens:
            if (answer := self.ownership.made(identity, built).map(lambda workspace: self._configured(identity, workspace))).is_ok():
                screens.pop_all()
            return answer

    def activated(self, identity: Identity, window: bpy.types.Window) -> BlenderResult[Activation]:
        """Requests the owned workspace and returns its deferred activation lifetime."""

        def switched(workspace: bpy.types.WorkSpace) -> Activation:
            """Captures session handles before requesting the native workspace switch."""
            activation = Activation(window=window, target=Handle.of(workspace), prior=Handle.of(window.workspace), order=self.order)
            window.workspace = workspace
            return activation

        return self.made(identity).map(switched)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Activation", "TabOrder", "WorkspaceSetup"]
