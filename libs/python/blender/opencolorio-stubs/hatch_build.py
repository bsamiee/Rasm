"""Wheel build hook generating the PyOpenColorIO stubs with pybind11-stubgen from the OpenColorIO the build environment holds."""

from pathlib import Path
import subprocess
import sys
from tempfile import TemporaryDirectory
from typing import override

from hatchling.builders.config import BuilderConfig
from hatchling.builders.hooks.plugin.interface import BuildHookInterface

# --- [COMPOSITION] ----------------------------------------------------------------------


class StubsHook(BuildHookInterface[BuilderConfig]):
    """Generates the stubs into a folder the build holds, then places it as the wheel's `PyOpenColorIO-stubs` package, editable installs included."""

    @override
    def initialize(self, version: str, build_data: dict[str, object]) -> None:
        self.generated = TemporaryDirectory()
        subprocess.run((sys.executable, "-m", "pybind11_stubgen", "PyOpenColorIO", "--root-suffix=-stubs", "--output-dir", self.generated.name), check=True)
        placed = {str(Path(self.generated.name, "PyOpenColorIO-stubs")): "PyOpenColorIO-stubs"}
        build_data.update(force_include=placed, force_include_editable=placed)

    @override
    def finalize(self, version: str, build_data: dict[str, object], artifact_path: str) -> None:
        self.generated.cleanup()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["StubsHook"]
