"""Shared pytest configuration for repo-local Python tests."""

# --- [IMPORTS] ------------------------------------------------------------------------

from dataclasses import dataclass
from pathlib import Path

from hypothesis import HealthCheck, Phase, settings as hyp_settings
from hypothesis.configuration import set_hypothesis_home_dir
from hypothesis.database import DirectoryBasedExampleDatabase
import pytest


# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class RasmPytestPaths:
    root: Path
    pytest_cache: Path
    hypothesis_home: Path
    hypothesis_examples: Path


# --- [CONSTANTS] -----------------------------------------------------------------------


_ROOT = Path(__file__).resolve().parents[1]
_PYTEST_CACHE = _ROOT / ".cache" / "pytest"
_HYPOTHESIS_HOME = _ROOT / ".cache" / "hypothesis"
_HYPOTHESIS_EXAMPLES = _HYPOTHESIS_HOME / "examples"
_EXAMPLE_DB = DirectoryBasedExampleDatabase(_HYPOTHESIS_EXAMPLES)
_SUPPRESSIONS = (HealthCheck.too_slow, HealthCheck.data_too_large)


# --- [COMPOSITION] ---------------------------------------------------------------------


set_hypothesis_home_dir(_HYPOTHESIS_HOME)
hyp_settings.register_profile("rasm", database=_EXAMPLE_DB, deadline=None, suppress_health_check=_SUPPRESSIONS)
hyp_settings.register_profile("rasm-ci", database=_EXAMPLE_DB, deadline=None, max_examples=200, suppress_health_check=_SUPPRESSIONS)
hyp_settings.register_profile(
    "rasm-stress",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=1000,
    phases=(Phase.explicit, Phase.reuse, Phase.generate, Phase.target, Phase.shrink),
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    "rasm-debug",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=25,
    phases=(Phase.explicit, Phase.reuse, Phase.generate),
    suppress_health_check=_SUPPRESSIONS,
)


# --- [EXPORTS] -------------------------------------------------------------------------


@pytest.fixture(scope="session")
def rasm_pytest_paths() -> RasmPytestPaths:
    return RasmPytestPaths(root=_ROOT, pytest_cache=_PYTEST_CACHE, hypothesis_home=_HYPOTHESIS_HOME, hypothesis_examples=_HYPOTHESIS_EXAMPLES)


@pytest.fixture(scope="session")
def hypothesis_examples_dir() -> Path:
    return _HYPOTHESIS_EXAMPLES


@pytest.fixture(scope="session")
def hypothesis_home_dir() -> Path:
    return _HYPOTHESIS_HOME
