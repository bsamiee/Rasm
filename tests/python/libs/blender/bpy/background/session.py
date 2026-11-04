"""Installs the fixture extension and runs native pytest with its registration scopes."""

from collections.abc import Callable, Generator
from contextlib import AbstractContextManager, contextmanager, ExitStack
from pathlib import Path
import shutil
import sys
from tempfile import TemporaryDirectory

import bpy
from expression import Result
import pytest

from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Declarations, Registration

# --- [TYPES] ----------------------------------------------------------------------------

type Acquired = Callable[[Declarations], AbstractContextManager[Identity]]

# --- [COMPOSITION] ----------------------------------------------------------------------


@contextmanager
def fixture() -> Generator[Acquired]:
    """Holds an isolated native extension repository and provides declaration scopes."""

    @contextmanager
    def opened(rows: Declarations, /) -> Generator[Identity]:
        with Registration(package, rows).opened() as outcome:
            match outcome:
                case Result(tag="ok", ok=identity):
                    yield identity
                case Result(error=_):
                    pytest.fail(repr(outcome))

    source = Path(__file__).with_name("fixture")
    manifest = f"""schema_version = "1.0.0"
id = "{source.name}"
version = "1.0.0"
name = "Fixture"
tagline = "Rows of every kind the bpy library declares"
maintainer = "Rasm"
type = "add-on"
blender_version_min = "{".".join(map(str, bpy.app.version))}"
license = ["SPDX:MIT"]
website = "https://docs.example.org/fixture/"
"""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    with TemporaryDirectory() as directory, ExitStack() as release:
        repos = preferences.extensions.repos
        repository = repos.new(name=source.name, module=source.name, custom_directory=directory)
        release.callback(repos.remove, repository)
        folder = shutil.copytree(source, Path(repository.directory) / source.name, ignore=shutil.ignore_patterns("__pycache__"))
        (folder / "blender_manifest.toml").write_text(manifest, encoding="utf-8")
        package = f"bl_ext.{repository.module}.{source.name}"
        bpy.ops.preferences.addon_enable(module=package)
        yield opened


@pytest.fixture(scope="session")
def acquired() -> Generator[Acquired]:
    """Supplies declaration scopes from one extension for the native test session."""
    with fixture() as opened:
        yield opened


def main(arguments: list[str], /) -> int:
    """Runs pytest with Blender's subnormal warning exception until bundled NumPy reaches 2.4."""
    return pytest.main(["-W", "ignore:The value of the smallest subnormal for:UserWarning:numpy._core.getlimits", *arguments], plugins=(sys.modules[__name__],))


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[sys.argv.index("--") + 1 :]))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Acquired", "acquired", "fixture", "main"]
