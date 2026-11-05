"""Loads an archive into the running Blender session whose MCP server receives this source and one `installed` call."""

import os

import bpy

# --- [OPERATIONS] -----------------------------------------------------------------------


def installed(archive: str, repository: str, sessions: tuple[int, ...]) -> None:
    """Installs archive into repository over any version it holds, a session outside `sessions` refused, and keeps the preferences the install changed.

    Args:
        archive: Extension archive path.
        repository: Local repository module.
        sessions: Process identifiers holding the profile being installed to.

    Raises:
        ProcessLookupError: If the session answering holds another profile.
    """
    if os.getpid() not in sessions:
        raise ProcessLookupError(os.getpid())
    bpy.ops.extensions.package_install_files(filepath=archive, repo=repository)
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    if preferences.is_dirty:
        bpy.ops.wm.save_userpref()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["installed"]
