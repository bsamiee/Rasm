"""py_analyzer suite wiring: SUT registration for the law-coverage gate plus the module-tree kit.

The two-line integration recipe: import ``register_sut``, call it with the package. The generic gate
(``tests.python._testkit.test_policy``) walks this package's public surface on every full-suite run;
StrEnum vocabularies, method-free frozen structs, and value-only symbols auto-exempt by predicate,
and every remaining public symbol carries census credit through the suite's ``COVERS`` tuple.
The ``kit`` fixture binds the shared ``TmpRoot`` writer through the testkit ``KitFactory`` seam
(this suite carries no settings payload).
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

import pytest

from tests.python._testkit.laws import register_sut
from tests.python._testkit.seams import tmp_root


if TYPE_CHECKING:
    from pathlib import Path

    from tests.python._testkit.seams import TmpRoot


# --- [CONSTANTS] ------------------------------------------------------------------------

register_sut("tools.py_analyzer")

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def kit(tmp_path: Path) -> TmpRoot[None]:
    """Isolated module-tree kit over the shared ``TmpRoot`` primitive via the ``KitFactory[None]`` seam.

    Returns:
        TmpRoot whose ``write`` materializes analyzer sample modules under an isolated root.
    """
    return tmp_root(tmp_path, lambda _root: None)
