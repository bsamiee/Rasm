"""Install package-wide runtime hooks before downstream Assay imports.

The beartype claw hook must run before ordinary submodules load. Settings faults are
captured for dispatch-time reporting so import stays usable for CLI fault envelopes.
"""
# ruff:file-ignore[non-empty-init-module]

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
import sys

from beartype import BeartypeConf
from beartype.claw import beartype_this_package
from opentelemetry.sdk.resources import Resource
from pydantic import ValidationError

lazy from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
lazy from opentelemetry.sdk.trace import TracerProvider
lazy from opentelemetry.sdk.trace.export import BatchSpanProcessor
lazy from opentelemetry.trace import set_tracer_provider

import assay.core.aspect


match os.environ.get("ASSAY_CLAW", ""):  # ruff:ignore[banned-api]
    case "1":
        beartype_this_package(conf=BeartypeConf(is_pep484_tower=True, warning_cls_on_decorator_exception=None))
    case _:
        pass

from assay._logging import configure_logging
from assay.composition.settings import AssaySettings


# --- [CONSTANTS] ------------------------------------------------------------------------

DRAIN_MS: int = 1500
_SERVICE: dict[str, str | int] = {
    "service.name": "assay",
    "service.namespace": "rasm",
    "process.pid": os.getpid(),
    "process.runtime.name": "python",
    "process.runtime.version": sys.version.split()[0],
}

# --- [COMPOSITION] ----------------------------------------------------------------------


def install_tracing(endpoint: str) -> None:
    """Install the global OTLP tracer when an endpoint is configured.

    Args:
        endpoint: OTLP HTTP endpoint; empty string disables tracing.
    """
    match endpoint:
        case "":
            pass
        case target:
            provider = TracerProvider(resource=Resource.create(_SERVICE))
            provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter(endpoint=target), schedule_delay_millis=DRAIN_MS))
            set_tracer_provider(provider)


def bootstrap_error() -> ValidationError | None:
    """Return the deferred settings fault captured during package bootstrap."""
    return _BOOTSTRAP_ERROR


try:
    _SETTINGS = AssaySettings()
    _BOOTSTRAP_ERROR: ValidationError | None = None
except ValidationError as exc:
    _BOOTSTRAP_ERROR = exc
    _SETTINGS = AssaySettings.model_construct()

configure_logging(_SETTINGS.log_format)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DRAIN_MS", "bootstrap_error", "configure_logging", "install_tracing"]
