"""Assay package initializer: installs beartype claw hook, bootstraps settings, configures logging, and exposes the tracing installer.

The beartype claw hook must be installed before any other Assay submodule is imported; the
``ASSAY_CLAW`` environment variable gates activation so integration tests can opt out.
Settings bootstrap errors are captured and deferred to dispatch time rather than raising at import.
"""
# ruff: noqa: RUF067  # claw hook must install before any Assay module is imported.

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
# core.aspect uses PEP 695 ParamSpec aliases incompatible with beartype_this_package; must be imported before the claw hook installs.
import os
import sys

from beartype import BeartypeConf
from beartype.claw import beartype_this_package
from opentelemetry.sdk.resources import Resource
from pydantic import ValidationError

import tools.assay.core.aspect


match os.environ.get("ASSAY_CLAW", ""):  # noqa: TID251  # import-time claw gate cannot route through AssaySettings, which is loaded after the hook
    case "1":
        beartype_this_package(conf=BeartypeConf(is_pep484_tower=True, warning_cls_on_decorator_exception=None))
    case _:
        pass

from tools.assay._logging import configure_logging  # noqa: PLC2701  # re-exported; callers must not import from the private submodule directly
from tools.assay.composition.settings import AssaySettings


# --- [CONSTANTS] ------------------------------------------------------------------------

_DRAIN_MS: int = 1500  # BatchSpanProcessor schedule cadence; must match the exit-time force_flush budget in __main__.main.
_SERVICE: dict[str, str | int] = {
    "service.name": "assay",
    "service.namespace": "rasm",
    "process.pid": os.getpid(),
    "process.runtime.name": "python",
    "process.runtime.version": sys.version.split()[0],
}

# --- [COMPOSITION] ----------------------------------------------------------------------


def _install_tracing(endpoint: str) -> None:
    # OTLP import chain (~50 ms) deferred to the endpoint-set path to avoid startup cost when tracing is disabled.
    from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter  # noqa: PLC0415
    from opentelemetry.sdk.trace import TracerProvider  # noqa: PLC0415
    from opentelemetry.sdk.trace.export import BatchSpanProcessor  # noqa: PLC0415
    from opentelemetry.trace import set_tracer_provider  # noqa: PLC0415

    # Exporter connects on force_flush, not construction; a stale endpoint fails silently until flush time.
    provider = TracerProvider(resource=Resource.create(_SERVICE))
    provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter(endpoint=endpoint), schedule_delay_millis=_DRAIN_MS))
    set_tracer_provider(provider)


def install_tracing(endpoint: str) -> None:
    """Install the OTLP trace provider and register it as the global tracer provider.

    Defers the OTLP import chain to avoid startup cost when tracing is disabled.
    When endpoint is empty no provider is installed and the call is a no-op.

    Args:
        endpoint: OTLP HTTP endpoint URL; empty string disables tracing.
    """
    match endpoint:
        case "":
            pass
        case target:
            _install_tracing(target)


def bootstrap_error() -> ValidationError | None:
    """Return the import-time settings validation fault, or None when settings bootstrapped cleanly."""
    return _BOOTSTRAP_ERROR


try:
    _SETTINGS = AssaySettings()
    _BOOTSTRAP_ERROR: ValidationError | None = None
except ValidationError as exc:
    # Logging falls back to field defaults; the fault surfaces as FAULTED at dispatch (registry._dispatch / parse_fault).
    _BOOTSTRAP_ERROR = exc
    _SETTINGS = AssaySettings.model_construct()

configure_logging(_SETTINGS.log_format)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["bootstrap_error", "configure_logging", "install_tracing"]
