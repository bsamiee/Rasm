"""Root composition owner for tests/python: SUT registration derives from disk shape."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from pathlib import Path

from tests.python._testkit.laws import register_tree
from tests.python._testkit.runtime import REPO_ROOT


# --- [COMPOSITION] ----------------------------------------------------------------------

# A libs/python package registers the moment it carries source; per-suite conftests compose
# fixtures and seams only, never registration. Tool suites register in their own conftest.
register_tree(REPO_ROOT / "libs" / "python", Path(__file__).resolve().parent / "libs")
