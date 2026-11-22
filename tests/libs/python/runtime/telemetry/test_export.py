"""Spans held by `telemetry.export.ManualSpanProcessor` until a flush exports them, and the resource of `telemetry.export.provider`."""

from collections.abc import Sequence
import os
from typing import override

from opentelemetry.sdk.trace import ReadableSpan
from opentelemetry.sdk.trace.export import SpanExportResult
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
from opentelemetry.trace import SpanContext, TraceFlags
import pytest

from rasm.telemetry.export import ManualSpanProcessor, provider

# --- [SERVICES] -------------------------------------------------------------------------


class _Refusing(InMemorySpanExporter):
    """Exporter keeping each batch and answering every export with failure."""

    @override
    def export(self, spans: Sequence[ReadableSpan]) -> SpanExportResult:
        super().export(spans)
        return SpanExportResult.FAILURE


# --- [OPERATIONS] -----------------------------------------------------------------------


def _ended(name: str, /, *, sampled: bool = True) -> ReadableSpan:
    return ReadableSpan(name, context=SpanContext(trace_id=7, span_id=len(name), is_remote=False, trace_flags=TraceFlags(TraceFlags.SAMPLED if sampled else TraceFlags.DEFAULT)))


def _names(exporter: InMemorySpanExporter, /) -> list[str]:
    return [span.name for span in exporter.get_finished_spans()]


def test_spans_wait_for_a_flush_and_export_in_one_batch() -> None:
    """Spans a provider ends reach no exporter until `force_flush`, which exports every sampled one and reports success."""
    exporter = InMemorySpanExporter()
    held = provider("suite-flush", "1.2.3", ManualSpanProcessor(exporter))
    with held.get_tracer("suite").start_as_current_span("render"), held.get_tracer("suite").start_as_current_span("frame"):
        pass
    assert not _names(exporter)
    assert held.force_flush()
    assert _names(exporter) == ["frame", "render"]


def test_an_unsampled_span_is_never_exported() -> None:
    """A span its sampler did not record is dropped at its end."""
    exporter = InMemorySpanExporter()
    processor = ManualSpanProcessor(exporter)
    processor.on_end(_ended("kept"))
    processor.on_end(_ended("dropped", sampled=False))
    assert processor.force_flush()
    assert _names(exporter) == ["kept"]


def test_past_its_bound_the_oldest_held_span_is_dropped() -> None:
    """A processor holding two spans keeps the newest two of three."""
    exporter = InMemorySpanExporter()
    processor = ManualSpanProcessor(exporter, held=2)
    for name in ("a", "bb", "ccc"):
        processor.on_end(_ended(name))
    processor.force_flush()
    assert _names(exporter) == ["bb", "ccc"]


def test_a_refused_send_keeps_the_spans_for_the_next_flush() -> None:
    """While the host refuses a send, a flush exports nothing and reports failure, and the next allowed flush exports the held spans."""
    exporter, allowed = InMemorySpanExporter(), [False]
    processor = ManualSpanProcessor(exporter, allowed=lambda: allowed[0])
    processor.on_end(_ended("held"))
    assert (processor.force_flush(), _names(exporter)) == (False, [])
    allowed[0] = True
    assert (processor.force_flush(), _names(exporter)) == (True, ["held"])


def test_a_flush_reports_the_exporter_refusing_its_batch() -> None:
    """A flush whose exporter answers failure reports failure, and an empty flush reports success without exporting."""
    exporter = _Refusing()
    processor = ManualSpanProcessor(exporter)
    assert processor.force_flush()
    processor.on_end(_ended("refused"))
    assert not processor.force_flush()
    assert _names(exporter) == ["refused"]


@pytest.mark.parametrize("allowed,exported", [(True, ["before"]), (False, [])])
def test_shutdown_exports_where_allowed_and_drops_every_later_span(*, allowed: bool, exported: list[str]) -> None:
    """Shutdown exports the held spans where a send is allowed, else drops them, drops spans ended after it, and shuts the exporter down."""
    exporter = InMemorySpanExporter()
    processor = ManualSpanProcessor(exporter, allowed=lambda: allowed)
    processor.on_end(_ended("before"))
    processor.shutdown()
    processor.on_end(_ended("after"))
    processor.force_flush()
    assert _names(exporter) == exported
    exporter.export([_ended("closed")])
    assert _names(exporter) == exported


def test_the_service_given_wins_over_its_variables_and_detectors_fill_the_rest(monkeypatch: pytest.MonkeyPatch) -> None:
    """The resource names the service and version given over `OTEL_SERVICE_NAME` and `OTEL_RESOURCE_ATTRIBUTES`, keeps the other attributes those name, and holds the process detector's id."""
    monkeypatch.setenv("OTEL_SERVICE_NAME", "from-variable")
    monkeypatch.setenv("OTEL_RESOURCE_ATTRIBUTES", "service.name=from-attributes,service.version=0.0.0,deployment.environment.name=staging")
    attributes = provider("suite-resource", "1.2.3").resource.attributes
    assert (attributes["service.name"], attributes["service.version"], attributes["deployment.environment.name"], attributes["process.pid"]) == ("suite-resource", "1.2.3", "staging", os.getpid())
