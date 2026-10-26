"""Pytest plugin with Hypothesis default profile, network marking, Trio autojump backend, and telemetry and log capture fixtures."""

lazy from collections.abc import Generator

from hypothesis import settings
import pytest
lazy from opentelemetry import trace
lazy from opentelemetry.sdk.trace import TracerProvider
lazy from opentelemetry.sdk.trace.export import SimpleSpanProcessor
lazy from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
lazy import pytest_socket
lazy from structlog.testing import capture_logs
lazy from structlog.types import EventDict, Processor
lazy import trio.testing

# --- [COMPOSITION] ----------------------------------------------------------------------
# --- [SESSION]

settings.register_profile("default", deadline=None)


def pytest_collection_modifyitems(items: list[pytest.Item]) -> None:
    """Mark tests requesting ``socket_enabled`` as ``network``."""
    for item in items:
        if isinstance(item, pytest.Function) and pytest_socket.socket_enabled.name in item.fixturenames:
            item.add_marker(pytest.mark.network, append=False)


# --- [BACKENDS]


@pytest.fixture
def anyio_backend() -> tuple[str, dict[str, object]]:
    """Run ``anyio`` tests on Trio under a clock that jumps to the next deadline once every task blocks."""
    return ("trio", {"clock": trio.testing.MockClock(autojump_threshold=0)})


# --- [CAPTURE]


@pytest.fixture(scope="session")
def _otel_exporter() -> InMemorySpanExporter:
    """Set process ``TracerProvider`` with in-memory exporter and return exporter."""
    exporter = InMemorySpanExporter()
    tracer_provider = TracerProvider()
    tracer_provider.add_span_processor(SimpleSpanProcessor(exporter))
    trace.set_tracer_provider(tracer_provider)
    return exporter


@pytest.fixture
def otel_spans(_otel_exporter: InMemorySpanExporter) -> InMemorySpanExporter:
    """Clear and return session exporter."""
    _otel_exporter.clear()
    return _otel_exporter


@pytest.fixture
def log_processors() -> tuple[Processor, ...]:
    """Return processors before ``log_events`` capture sink, empty unless a package conftest overrides it."""
    return ()


@pytest.fixture
def log_events(log_processors: tuple[Processor, ...]) -> Generator[list[EventDict]]:
    """Capture structlog events as plain dictionaries."""
    with capture_logs(processors=log_processors) as events:
        yield events
