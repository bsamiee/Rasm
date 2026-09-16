"""Pytest runtime plugin for the Hypothesis default profile, the network marker, property records, and the telemetry and log capture fixtures."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Generator

from hypothesis import HealthCheck, settings as hyp_settings
from opentelemetry import trace as otel_trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
import pytest
from structlog.testing import capture_logs
from structlog.types import EventDict, Processor

from tests.python.support.properties import PROPERTY_RECORDS, PropertyRecord, record_coverage_declarations

# --- [COMPOSITION] ----------------------------------------------------------------------

hyp_settings.register_profile("default", deadline=None, suppress_health_check=(HealthCheck.too_slow, HealthCheck.data_too_large, HealthCheck.filter_too_much))


def pytest_collection_modifyitems(config: pytest.Config, items: list[pytest.Item]) -> None:
    """Apply the network marker and record the property tests and each module's ``COVERS`` declarations once."""
    for item in items:
        if "socket_enabled" in getattr(item, "fixturenames", ()):
            item.add_marker(pytest.mark.network, append=False)
    config.stash[PROPERTY_RECORDS] = (
        *(record for item in items for mark in item.iter_markers("property") if isinstance(record := mark.kwargs.get("record"), PropertyRecord)),
        *(record for module in dict.fromkeys(getattr(item, "module", None) for item in items) for record in record_coverage_declarations(module)),
    )


@pytest.fixture(scope="session")
def _otel_exporter() -> InMemorySpanExporter:
    """Attach a fresh in-memory exporter to the set-once process-level ``TracerProvider``."""
    match otel_trace.get_tracer_provider():
        case TracerProvider() as tracer_provider:
            pass
        case _:
            tracer_provider = TracerProvider()
            otel_trace.set_tracer_provider(tracer_provider)
    exporter = InMemorySpanExporter()
    tracer_provider.add_span_processor(SimpleSpanProcessor(exporter))
    return exporter


@pytest.fixture
def otel_spans(_otel_exporter: InMemorySpanExporter) -> InMemorySpanExporter:
    """Clear and return the session exporter for a test."""
    _otel_exporter.clear()
    return _otel_exporter


@pytest.fixture
def log_processors() -> tuple[Processor, ...]:
    """Processors inserted before the ``log_events`` capture sink, empty unless a package conftest overrides it."""
    return ()


@pytest.fixture
def log_events(log_processors: tuple[Processor, ...]) -> Generator[list[EventDict]]:
    """Capture structlog events as plain dictionaries for a test."""
    with capture_logs(processors=log_processors) as events:
        yield events


# --- [EXPORTS] --------------------------------------------------------------------------

__all__: list[str] = []
