"""Root pytest configuration for tests/python; SUT registration follows the filesystem layout."""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path

from tests.python.support.properties import register_package_tree
from tests.python.support.runtime import REPO_ROOT

# --- [COMPOSITION] ----------------------------------------------------------------------

register_package_tree(REPO_ROOT / "libs" / "python", Path(__file__).resolve().parent / "libs")
