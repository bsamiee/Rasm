"""Install package-wide runtime hooks before downstream Assay imports.

The beartype claw hook must run before ordinary submodules load. Settings faults are
captured for dispatch-time reporting so import stays usable for CLI fault envelopes.
"""
# ruff:file-ignore[non-empty-init-module]  # claw hook installs before ordinary package imports.

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
# aspect uses PEP 695 aliases beartype_this_package cannot decorate.
import os
import sys

from beartype import BeartypeConf
from beartype.claw import beartype_this_package
from opentelemetry.sdk.resources import Resource
from pydantic import ValidationError

import tools.assay.core.aspect


# AssaySettings loads after claw; the import-time gate must read the environment directly.
match os.environ.get("ASSAY_CLAW", ""):  # ruff:ignore[banned-api]
    case "1":
        beartype_this_package(conf=BeartypeConf(is_pep484_tower=True, warning_cls_on_decorator_exception=None))
    case _:
        pass

from tools.assay._logging import configure_logging  # ruff:ignore[import-private-name]  # package re-export keeps the private logging owner internal
from tools.assay.composition.settings import AssaySettings


# --- [CONSTANTS] ------------------------------------------------------------------------

DRAIN_MS: int = 1500  # shared trace flush budget; keep in sync with exit drain
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
            # OTLP imports stay on the endpoint-set path to keep tracing-disabled startup lean.
            from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter  # ruff:ignore[import-outside-top-level]
            from opentelemetry.sdk.trace import TracerProvider  # ruff:ignore[import-outside-top-level]
            from opentelemetry.sdk.trace.export import BatchSpanProcessor  # ruff:ignore[import-outside-top-level]
            from opentelemetry.trace import set_tracer_provider  # ruff:ignore[import-outside-top-level]

            # Exporter connects on force_flush, not construction; a stale endpoint fails silently until flush time.
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
    # Settings faults surface at dispatch; logging falls back to field defaults.
    _BOOTSTRAP_ERROR = exc
    _SETTINGS = AssaySettings.model_construct()

configure_logging(_SETTINGS.log_format)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DRAIN_MS", "bootstrap_error", "configure_logging", "install_tracing"]
