"""Prints what the running Blender's interpreter accepts and bundles, read by the extension build."""

import importlib.metadata
import json
import pkgutil
import sys
import sysconfig

from bl_pkg.bl_extension_utils import platform_from_this_system
from packaging.tags import platform_tags

# --- [COMPOSITION] ----------------------------------------------------------------------

bundled = sysconfig.get_paths()["purelib"]
sys.stdout.write(
    json.dumps({
        "python": f"{sys.version_info.major}.{sys.version_info.minor}",
        "platform": platform_from_this_system(),
        "tags": list(platform_tags()),
        "distributions": sorted(distribution.metadata["Name"] for distribution in importlib.metadata.distributions(path=[bundled])),
        "modules": sorted(module.name for module in pkgutil.iter_modules([bundled])),
    })
    + "\n"
)
