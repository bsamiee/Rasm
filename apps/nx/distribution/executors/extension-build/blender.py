"""Prints what the running Blender's interpreter accepts and bundles, with the release of each module it bundles without distribution metadata."""

import importlib.metadata
import json
import pkgutil
import sys
import sysconfig

from bl_pkg.bl_extension_utils import platform_from_this_system
import OpenImageIO
from packaging.tags import platform_tags
import PyOpenColorIO

# --- [COMPOSITION] ----------------------------------------------------------------------

bundled = sysconfig.get_paths()["purelib"]
sys.stdout.write(
    json.dumps({
        "python": f"{sys.version_info.major}.{sys.version_info.minor}",
        "platform": platform_from_this_system(),
        "tags": list(platform_tags()),
        "distributions": dict(sorted((distribution.metadata["Name"], distribution.version) for distribution in importlib.metadata.distributions(path=[bundled]))),
        "undeclared": {"opencolorio": PyOpenColorIO.__version__, "openimageio": OpenImageIO.VERSION_STRING},
        "modules": sorted(module.name for module in pkgutil.iter_modules([bundled])),
    })
    + "\n"
)

# --- [EXPORTS] -------------------------------------------------------------------------

__all__: list[str] = []
