"""Pytest plugin with Hypothesis default profile, network marking, property records, and telemetry and log capture fixtures."""

from collections.abc import Generator

from hypothesis import HealthCheck, settings
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
import pytest
from structlog.testing import capture_logs
from structlog.types import EventDict, Processor

from tests.python.support.properties import PROPERTY_RECORDS

# --- [COMPOSITION] ----------------------------------------------------------------------

settings.register_profile("default", deadline=None, suppress_health_check=(HealthCheck.too_slow, HealthCheck.data_too_large, HealthCheck.filter_too_much))


def pytest_collection_modifyitems(config: pytest.Config, items: list[pytest.Item]) -> None:
    """Mark socket tests ``network``, reject repeated property marks, and record each property mark once."""
    for function in (item for item in items if isinstance(item, pytest.Function)):
        if "socket_enabled" in function.fixturenames:
            function.add_marker(pytest.mark.network, append=False)
        if sum(mark.name == "property" for mark in function.own_markers) > 1:
            raise pytest.UsageError(f"@property_test applied twice to {function.nodeid}, remove one")
    config.stash[PROPERTY_RECORDS] = tuple(dict.fromkeys(mark.kwargs["record"] for item in items for mark in item.iter_markers("property")))


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
