"""Wheel build hook generating the store service client from its contract with protoc-gen-py and protoc-gen-connectrpc."""

from pathlib import Path
import subprocess
import sys
from tempfile import TemporaryDirectory
from typing import override

from hatchling.builders.config import BuilderConfig
from hatchling.builders.hooks.plugin.interface import BuildHookInterface

# --- [COMPOSITION] ----------------------------------------------------------------------


class ContractHook(BuildHookInterface[BuilderConfig]):
    """Generates the client into a folder the build holds, then places each generated package in the wheel as a typed package, editable installs included."""

    @override
    def initialize(self, version: str, build_data: dict[str, object]) -> None:
        self.generated = TemporaryDirectory()
        contract = next(folder for row in self.metadata.hatch.build_targets["sdist"]["force-include"].items() for part in row if (folder := Path(self.root, part)).is_dir())
        subprocess.run(
            (sys.executable, "-m", "grpc_tools.protoc", f"-I{contract}", f"--py_out={self.generated.name}", f"--connectrpc_out={self.generated.name}", *map(str, contract.rglob("*.proto"))), check=True
        )
        placed = {str(package): package.name for package in Path(self.generated.name).iterdir() if package.is_dir()}
        for package in placed:
            Path(package, "py.typed").touch()
        build_data.update(force_include=placed, force_include_editable=placed)

    @override
    def finalize(self, version: str, build_data: dict[str, object], artifact_path: str) -> None:
        self.generated.cleanup()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ContractHook"]
