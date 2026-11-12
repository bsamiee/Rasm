"""OTLP over HTTP for a host plug-in: the trace collector a host setting names and its span exporter."""

from collections.abc import Mapping
from dataclasses import dataclass

from opentelemetry.exporter.otlp.common import http
from opentelemetry.exporter.otlp.proto.http import Compression, trace_exporter

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class Collector:
    """OTLP HTTP trace collector, each option the `OTLPSpanExporter` argument of its name.

    An option left out takes the exporter's own value: its `OTEL_EXPORTER_OTLP_TRACES_*` variable, else its `OTEL_EXPORTER_OTLP_*` variable, else the exporter's default.
    `endpoint` is the whole URL spans post to, signal path included, and `timeout` the seconds one export and its retries take at most.
    """

    endpoint: str | None = None
    headers: Mapping[str, str] | None = None
    timeout: float | None = None
    compression: Compression | http.Compression | None = None

    def exporter(self) -> trace_exporter.OTLPSpanExporter:
        """Returns a new span exporter posting to the collector."""
        return trace_exporter.OTLPSpanExporter(endpoint=self.endpoint, headers=self.headers, timeout=self.timeout, compression=self.compression)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Collector"]
