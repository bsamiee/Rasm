"""py_analyzer suite wiring: SUT registration for the law-coverage gate (second-package pilot) plus the module-tree kit.

The two-line integration recipe: import ``register_sut``, call it with the package and its exempt set.
The generic gate (``tests.python._testkit.test_policy``) walks this package's public surface on every
full-suite run with no further configuration. The ``kit`` fixture binds the shared ``TmpRoot`` writer through
the testkit ``KitFactory`` seam (this suite carries no settings payload).
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import Final, TYPE_CHECKING

import pytest

from tests.python._testkit.laws import register_sut
from tests.python._testkit.seams import tmp_root


if TYPE_CHECKING:
    from pathlib import Path

    from tests.python._testkit.seams import TmpRoot


# --- [CONSTANTS] ------------------------------------------------------------------------

# PILOT DEBT LEDGER, not an endorsement: every entry is a currently-uncovered public symbol awaiting
# laws. Shrink this set by authoring laws, never by deleting symbols from it without one. cli.py now
# declares __all__, so its import-noise symbols (Path/TYPE_CHECKING/annotations/assert_never) leave the
# walked surface — only the 15 real public symbols remain on the ledger.
_EXEMPT: Final = frozenset({
    "analyze_paths", "classify_scope", "PY_ANALYZER_ROOT",                                  # analyzer: walk + scope engine
    "Diagnostic", "JsonValue", "OutputFormat", "Rule", "RULES",                             # rules: catalog + wire shape
    "RuleCategory", "RuleId", "Scope", "Severity", "diagnostic",                            # rules: vocabularies + constructor
    "emit", "main",                                                                  # cli: output contract + entrypoint
})  # fmt: skip

register_sut("tools.py_analyzer", exempt=_EXEMPT)

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def kit(tmp_path: Path) -> TmpRoot[None]:
    """Isolated module-tree kit over the shared ``TmpRoot`` primitive via the ``KitFactory[None]`` seam.

    Returns:
        TmpRoot whose ``write`` materializes analyzer sample modules under an isolated root.
    """
    return tmp_root(tmp_path, lambda _root: None)
