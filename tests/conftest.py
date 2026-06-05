"""Shared pytest configuration for repo-local Python tests."""

# --- [IMPORTS] ------------------------------------------------------------------------

from dataclasses import dataclass
from pathlib import Path
from typing import TYPE_CHECKING

from hypothesis import HealthCheck, Phase, settings as hyp_settings
from hypothesis.configuration import set_hypothesis_home_dir
from hypothesis.database import DirectoryBasedExampleDatabase
from opentelemetry import trace as otel_trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
import pytest
from structlog.testing import capture_logs


if TYPE_CHECKING:
    from collections.abc import Generator

    from structlog.types import EventDict


# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class RasmPytestPaths:
    """Shared cache and repository paths for Python test fixtures."""

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
hyp_settings.register_profile(
    # Mutation runs: no shared example DB (cached examples must not mask mutants), derandomized for stable kill verdicts,
    # explicit+generate only (shrinking a survivor wastes the mutation budget; a single killing example suffices).
    "rasm-mutation",
    database=None,
    deadline=None,
    derandomize=True,
    max_examples=50,
    phases=(Phase.explicit, Phase.generate),
    suppress_health_check=_SUPPRESSIONS,
)


def pytest_collection_modifyitems(items: list[pytest.Item]) -> None:
    """Auto-apply ``@pytest.mark.network`` to any test requesting the ``socket_enabled`` fixture.

    Generic to every suite under this root (assay, quality, future py projects): ``pytest-socket``'s
    ``socket_enabled`` fixture lifts ``--disable-socket`` per test, but without this hook those tests
    are invisible to ``-m network`` selection. Marking by fixture-name keeps the network contract in one
    place instead of a per-test decorator that drifts from the fixture it gates.
    """
    network = pytest.mark.network
    for item in items:
        if "socket_enabled" in getattr(item, "fixturenames", ()):
            item.add_marker(network, append=False)


# --- [EXPORTS] -------------------------------------------------------------------------


@pytest.fixture(scope="session")
def rasm_pytest_paths() -> RasmPytestPaths:
    """Return repo-local cache and Hypothesis paths.

    Returns:
        Shared pytest path bundle for repo-local Python tests.
    """
    return RasmPytestPaths(root=_ROOT, pytest_cache=_PYTEST_CACHE, hypothesis_home=_HYPOTHESIS_HOME, hypothesis_examples=_HYPOTHESIS_EXAMPLES)


@pytest.fixture(scope="session")
def hypothesis_examples_dir() -> Path:
    """Return the persistent Hypothesis example database directory.

    Returns:
        Directory that stores reusable Hypothesis examples.
    """
    return _HYPOTHESIS_EXAMPLES


@pytest.fixture(scope="session")
def hypothesis_home_dir() -> Path:
    """Return the repo-local Hypothesis home directory.

    Returns:
        Directory that owns Hypothesis profile state.
    """
    return _HYPOTHESIS_HOME


@pytest.fixture(scope="session", autouse=True)
def _otel_provider() -> Generator[InMemorySpanExporter]:
    """Install a real TracerProvider once per session; ``SimpleSpanProcessor`` ensures synchronous export.

    ``autouse=True``: every test records real spans without opt-in. Placed in root conftest so it runs
    before engine/aspect module-scope imports that bind the ProxyTracer — pytest guarantees root conftest
    fixtures execute before sub-conftest fixtures.

    Yields:
        The ``InMemorySpanExporter`` backed by the session-scoped ``TracerProvider``.
    """
    exp = InMemorySpanExporter()
    tp = TracerProvider()
    tp.add_span_processor(SimpleSpanProcessor(exp))
    otel_trace.set_tracer_provider(tp)
    yield exp
    tp.shutdown()


@pytest.fixture
def otel_spans(_otel_provider: InMemorySpanExporter) -> InMemorySpanExporter:
    """Clear and return the session exporter for one test.

    Returns:
        Exporter containing only spans recorded after this fixture starts.
    """
    _otel_provider.clear()
    return _otel_provider


@pytest.fixture
def log_events() -> Generator[list[EventDict]]:
    """Capture structlog events as plain dicts for one test via ``capture_logs()``.

    INCOMPATIBLE with ``ring_processor`` assertions in the same test: ``capture_logs`` replaces the
    full processor chain. Use ``_RING.set(deque(...))`` for ring-content assertions instead.

    Yields:
        Mutable list of captured log event dicts; each entry has ``{'event': str, 'log_level': str, ...}``.
    """
    with capture_logs() as events:
        yield events
