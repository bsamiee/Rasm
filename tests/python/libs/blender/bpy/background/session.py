"""Background Blender session installing the fixture extension, observing each area while it is enabled and after it is disabled, and writing the observations as JSON to the path after `--`."""

from collections.abc import Callable, Mapping
from enum import Enum
import importlib
from pathlib import Path
import shutil
import sys
import traceback
from types import ModuleType
from typing import Final

import bpy
from expression import Option, Result
from expression.extra.result import catch
import msgspec
import numpy as np

from blender.bpy.extension.identity import Identity
from blender.records.faults import Fault

# --- [CONSTANTS] ------------------------------------------------------------------------

ID: Final = "fixture"
PACKAGE: Final = f"bl_ext.user_default.{ID}"
MANIFEST: Final = f"""schema_version = "1.0.0"
id = "{ID}"
version = "1.0.0"
name = "Fixture"
tagline = "Rows of every kind the bpy library declares"
maintainer = "Rasm"
type = "add-on"
blender_version_min = "5.4.0"
license = ["SPDX:MIT"]
website = "https://docs.example.org/fixture/"
"""

# --- [OPERATIONS] -----------------------------------------------------------------------


def plain(value: object) -> object:
    """Returns a JSON value: results and options as one-key objects, faults as their reason names, sets sorted, arrays as lists."""
    match value:
        case Result(tag="ok", ok=held):
            return {"ok": plain(held)}
        case Result(error=held):
            return {"error": plain(held)}
        case Option(tag="some", some=held):
            return {"some": plain(held)}
        case Option():
            return {"nothing": None}
        case Fault(reason=reason):
            return reason.name
        case msgspec.Struct():
            return plain(msgspec.structs.asdict(value))
        case Enum():
            return value.name
        case Path():
            return value.as_posix()
        case np.ndarray() | np.generic():
            return value.tolist()
        case frozenset() | set():
            return sorted(map(plain, value), key=repr)
        case tuple() | list():
            return [plain(item) for item in value]
        case Mapping():
            return {str(key): plain(item) for key, item in value.items()}
        case _:
            return value


def observed(area: ModuleType, probes: Mapping[str, Callable[..., object]], *arguments: object) -> dict[str, object]:
    """Runs each probe of an area, keeping a raised exception's traceback as its observation."""

    def ran(probe: Callable[..., object]) -> object:
        return catch(lambda: plain(probe(*arguments)), exception=Exception)().default_with(lambda error: {"raised": "".join(traceback.format_exception(error))})

    name = area.__name__.rpartition(".")[2]
    return {f"{name}.{key}": ran(probe) for key, probe in probes.items()}


def installed() -> None:
    """Copies the fixture package into the user extension repository beside its manifest."""
    folder = Path(bpy.utils.user_resource("EXTENSIONS", path="user_default", create=True)) / ID
    shutil.copytree(Path(__file__).parent / "fixture", folder, ignore=shutil.ignore_patterns("__pycache__"))
    (folder / "blender_manifest.toml").write_text(MANIFEST, encoding="utf-8")


# --- [COMPOSITION] ----------------------------------------------------------------------


def main(target: Path) -> None:
    """Enables the fixture, observes every area, disables it, observes the releases, and writes the observations."""
    installed()
    bpy.ops.preferences.addon_enable(module=PACKAGE)
    fixture = importlib.import_module(PACKAGE)
    identity: Identity = fixture.extension.WITNESS.identity.value
    enabled = {key: value for area in fixture.AREAS for key, value in observed(area, area.PROBES, identity).items()}
    bpy.ops.preferences.addon_disable(module=PACKAGE)
    released = {key: value for area in fixture.AREAS for key, value in observed(area, area.RELEASED).items()}
    target.write_bytes(msgspec.json.encode({"enabled": enabled, "released": released}))


main(Path(sys.argv[sys.argv.index("--") + 1]))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__: list[str] = []
