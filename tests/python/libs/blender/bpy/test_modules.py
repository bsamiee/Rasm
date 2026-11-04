"""Library imports under Blender's interpreter."""

import importlib
import os
from pathlib import Path
import site
import subprocess
import sys

import pytest

import blender
from blender.records import faults

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "name", sorted(".".join((blender.__name__, *path.relative_to(root).with_suffix("").parts)) for root in map(Path, blender.__path__) for path in root.rglob("*.py") if path.name != "__init__.py")
)
def test_modules(name: str) -> None:
    """Library modules import under Blender's native interpreter."""
    importlib.import_module(name)


def test_editable_user_site_reaches_child_interpreter(tmp_path: Path) -> None:
    """Native startup exposes the same editable library to a child of Blender's Python outside the checkout."""
    assert site.ENABLE_USER_SITE is True
    child = subprocess.run(
        (sys.executable, "-c", f"import {faults.__name__} as held; print(held.__file__)"),
        cwd=tmp_path,
        env={key: value for key, value in os.environ.items() if key != "PYTHONPATH"},
        stdout=subprocess.PIPE,
        text=True,
        check=True,
    )
    assert Path(child.stdout.strip()).samefile(faults.__file__)
