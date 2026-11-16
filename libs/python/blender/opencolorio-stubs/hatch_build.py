"""Wheel build hook generating the PyOpenColorIO stubs with pybind11-stubgen from the OpenColorIO the build environment holds."""

from itertools import accumulate, pairwise
from pathlib import Path
import subprocess
import sys
from tempfile import TemporaryDirectory
from typing import override

from hatchling.builders.config import BuilderConfig
from hatchling.builders.hooks.plugin.interface import BuildHookInterface

# --- [OPERATIONS] -----------------------------------------------------------------------


def _ordered(line: str, /) -> str:
    """Drops the default of each parameter a required one follows, the order pybind11 binds and Python refuses."""
    head, opened, rest = line.partition("(")
    depths = tuple(accumulate(1 if char in "([{" else -1 if char in ")]}" else 0 for char in rest))
    if not (head.lstrip().startswith("def ") and opened and -1 in depths):
        return line
    close = depths.index(-1)
    cuts = (-1, *(index for index, (char, depth) in enumerate(zip(rest[:close], depths, strict=False)) if char == "," and depth == 0), close)
    parameters = tuple(rest[start + 1 : end].strip() for start, end in pairwise(cuts))
    last = max((index for index, parameter in enumerate(parameters) if "=" not in parameter and not parameter.startswith(("*", "/"))), default=-1)
    required = tuple(parameter.partition("=")[0].rstrip() if index < last else parameter for index, parameter in enumerate(parameters))
    return line if required == parameters else f"{head}({', '.join(required)}{rest[close:]}"


# --- [COMPOSITION] ----------------------------------------------------------------------


class StubsHook(BuildHookInterface[BuilderConfig]):
    """Generates the stubs into a folder the build holds, then places it as the wheel's `PyOpenColorIO-stubs` package, editable installs included."""

    @override
    def initialize(self, version: str, build_data: dict[str, object]) -> None:
        self.generated = TemporaryDirectory()
        subprocess.run((sys.executable, "-m", "pybind11_stubgen", "PyOpenColorIO", "--root-suffix=-stubs", "--output-dir", self.generated.name), check=True)
        root = Path(self.generated.name, "PyOpenColorIO-stubs")
        for stub in root.rglob("*.pyi"):
            stub.write_text("".join(_ordered(line) for line in stub.read_text(encoding="utf-8").splitlines(keepends=True) if line.strip() != "from __future__ import annotations"), encoding="utf-8")
        placed = {str(root): "PyOpenColorIO-stubs"}
        build_data.update(force_include=placed, force_include_editable=placed)

    @override
    def finalize(self, version: str, build_data: dict[str, object], artifact_path: str) -> None:
        self.generated.cleanup()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["StubsHook"]
