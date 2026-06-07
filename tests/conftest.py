"""Shared pytest configuration for repo-local Python tests."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

import os
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
_DEFAULT_HYPOTHESIS_HOME = ROOT / ".cache" / "hypothesis"
_hypothesis_storage_directory = os.environ.get("HYPOTHESIS_STORAGE_DIRECTORY")  # noqa: TID251  # pytest boundary: anchor Hypothesis before import
HYPOTHESIS_HOME = Path(_hypothesis_storage_directory) if _hypothesis_storage_directory else _DEFAULT_HYPOTHESIS_HOME
os.environ.setdefault("HYPOTHESIS_STORAGE_DIRECTORY", str(HYPOTHESIS_HOME))  # noqa: TID251  # pytest boundary: seed subprocess home before import

# --- [IMPORTS] ------------------------------------------------------------------------

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


# --- [CONSTANTS] -----------------------------------------------------------------------
# Repo-local Hypothesis/cache anchors W3 reads directly (no fixture indirection over importable constants).
# Optional profiles: ``--hypothesis-profile=rasm-ci|rasm-stress|rasm-debug|rasm-adversarial|rasm-stateful|rasm-parity``;
# mutmut uses ``rasm-mutation`` via ``[tool.mutmut]`` pytest args.


HYPOTHESIS_EXAMPLES = HYPOTHESIS_HOME / "examples"
_EXAMPLE_DB = DirectoryBasedExampleDatabase(HYPOTHESIS_EXAMPLES)
_SUPPRESSIONS = (HealthCheck.too_slow, HealthCheck.data_too_large)


# --- [COMPOSITION] ---------------------------------------------------------------------


set_hypothesis_home_dir(HYPOTHESIS_HOME)
hyp_settings.register_profile("rasm", database=_EXAMPLE_DB, deadline=None, suppress_health_check=_SUPPRESSIONS)
hyp_settings.register_profile("rasm-ci", database=_EXAMPLE_DB, deadline=None, max_examples=200, suppress_health_check=_SUPPRESSIONS)
hyp_settings.register_profile(
    "rasm-stress",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=1000,
    phases=(Phase.explicit, Phase.reuse, Phase.generate, Phase.target, Phase.shrink, Phase.explain),
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    "rasm-debug",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=25,
    phases=(Phase.explicit, Phase.reuse, Phase.generate, Phase.explain),
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
hyp_settings.register_profile(
    # Adversarial corpus: deep example budget to drive the hostile-input degradation laws into their corners.
    "rasm-adversarial",
    database=_EXAMPLE_DB,
    deadline=None,
    max_examples=2000,
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # Stateful RBSMs (lease / tick / history): long step traces so interleavings shrink to a minimal counterexample.
    "rasm-stateful",
    database=_EXAMPLE_DB,
    deadline=None,
    stateful_step_count=200,
    suppress_health_check=_SUPPRESSIONS,
)
hyp_settings.register_profile(
    # A/B parity: derandomized (database=None is mandatory under derandomize) for byte-stable cross-tool comparison.
    "rasm-parity",
    database=None,
    deadline=None,
    derandomize=True,
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
