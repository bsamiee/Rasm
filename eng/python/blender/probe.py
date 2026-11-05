"""Writes the facts the installed Blender states as JSON to the file `--` names, each wheel platform after it mapped to its extension platforms."""

import importlib.metadata
import json
import os
from pathlib import Path
import platform
import sys
import sysconfig

from bl_pkg.cli.blender_ext import blender_platforms_from_wheel_platform, platform_from_this_system  # type: ignore[import-not-found]  # ty: ignore[unresolved-import]
import bpy
import OpenImageIO
import PyOpenColorIO

# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Writes the facts of this Blender to the answer file."""
    answer, *wheel_platforms = sys.argv[sys.argv.index("--") + 1 :]
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    text = preferences.themes[0].user_interface.wcol_tool.text
    facts = {
        "executable": sys.executable,
        "version": bpy.app.version_string,
        "python": platform.python_version(),
        "user_site": os.path.relpath(sysconfig.get_path("purelib", scheme=sysconfig.get_preferred_scheme("user"), vars={"userbase": sys.prefix}), sys.prefix),
        "repository": next(repository.module for repository in preferences.extensions.repos if repository.source == "USER" and not repository.use_remote_url),
        "bundled": {distribution.name: distribution.version for distribution in importlib.metadata.distributions(path=[sysconfig.get_path("purelib")]) if distribution.name}
        | {"openimageio": OpenImageIO.VERSION_STRING, "opencolorio": PyOpenColorIO.__version__},
        "text": (text.r, text.g, text.b),
        "platform": platform_from_this_system(),
        "platforms": {tag: blender_platforms_from_wheel_platform(tag) for tag in wheel_platforms},
    }
    Path(answer).write_text(json.dumps(facts), encoding="utf-8")  # ruff: ignore[banned-api]


if __name__ == "__main__":
    main()
