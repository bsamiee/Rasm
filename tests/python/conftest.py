"""Root composition owner for tests/python: SUT registration derives from disk shape."""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path

from tests.python.testkit.laws import register_tree
from tests.python.testkit.runtime import REPO_ROOT

# --- [COMPOSITION] ----------------------------------------------------------------------

register_tree(REPO_ROOT / "libs" / "python", Path(__file__).resolve().parent / "libs")
