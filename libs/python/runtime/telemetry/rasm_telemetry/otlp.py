"""OTLP over HTTP for a host plug-in: the collector a host setting names and its span exporter."""

from dataclasses import dataclass
from datetime import timedelta
from urllib.parse import urljoin

from opentelemetry.exporter.otlp.proto.http import trace_exporter

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class Collector:
    """OTLP HTTP collector a plug-in exports spans to, and the time one export holds the exporting thread at most."""

    endpoint: str = trace_exporter.DEFAULT_ENDPOINT
    timeout: timedelta = timedelta(seconds=1)

    def exporter(self) -> trace_exporter.OTLPSpanExporter:
        """Returns the span exporter posting to the collector's traces path, each export and its retries ending within the timeout."""
        return trace_exporter.OTLPSpanExporter(endpoint=urljoin(self.endpoint, trace_exporter.DEFAULT_TRACES_EXPORT_PATH), timeout=self.timeout.total_seconds())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Collector"]
