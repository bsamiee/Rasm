"""Shared pytest configuration for repo-local Python tests."""

# --- [IMPORTS] ------------------------------------------------------------------------

from pathlib import Path

from hypothesis import settings as hyp_settings
from hypothesis.configuration import set_hypothesis_home_dir
from hypothesis.database import DirectoryBasedExampleDatabase
import pytest


# --- [CONSTANTS] -----------------------------------------------------------------------

_ROOT = Path(__file__).resolve().parents[1]
_HYPOTHESIS_HOME = _ROOT / ".cache" / "hypothesis"
_HYPOTHESIS_EXAMPLES = _HYPOTHESIS_HOME / "examples"


# --- [COMPOSITION] ---------------------------------------------------------------------

set_hypothesis_home_dir(_HYPOTHESIS_HOME)
hyp_settings.register_profile("rasm", database=DirectoryBasedExampleDatabase(_HYPOTHESIS_EXAMPLES))
hyp_settings.load_profile("rasm")


# --- [EXPORTS] -------------------------------------------------------------------------


@pytest.fixture(scope="session")
def hypothesis_examples_dir() -> Path:
    return _HYPOTHESIS_EXAMPLES


@pytest.fixture(scope="session")
def hypothesis_home_dir() -> Path:
    return _HYPOTHESIS_HOME
