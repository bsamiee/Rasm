"""Configure Assay import-time tracing and beartype checks; logging lives in ``tools.assay._logging``."""
# ruff: noqa: RUF067, E402  # executable boundary: the claw gate runs before Assay imports.

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
# core.aspect owns the explicit beartype layer and uses PEP 695 ParamSpec aliases newer than claw can reduce.
# Import it before installing the package hook; subsequent Assay modules still load under ASSAY_CLAW.
import os
import sys

from beartype import BeartypeConf
from beartype.claw import beartype_this_package
from opentelemetry.sdk.resources import Resource
from pydantic import ValidationError

import tools.assay.core.aspect  # pre-claw load: aspect's PEP 695 aliases must bypass beartype_this_package


{"1": lambda: beartype_this_package(conf=BeartypeConf(is_pep484_tower=True, warning_cls_on_decorator_exception=None))}.get(
    os.environ.get("ASSAY_CLAW", ""),  # noqa: TID251  # import-time claw gate cannot route through AssaySettings, which is loaded after the hook
    lambda: None,
)()

from tools.assay._logging import configure_logging  # noqa: PLC2701  # leaf logging owner; re-exported so importers need not drag this bootstrap
from tools.assay.composition.settings import AssaySettings


# --- [CONSTANTS] ------------------------------------------------------------------------

_DRAIN_MS: int = 1500  # single ~1.5s bound: BatchSpanProcessor schedule cadence here and the exit-time force_flush budget in __main__.main
_SERVICE: dict[str, str | int] = {
    "service.name": "assay",
    "service.namespace": "rasm",
    "process.pid": os.getpid(),
    "process.runtime.name": "python",
    "process.runtime.version": sys.version.split()[0],
}


# --- [COMPOSITION] ----------------------------------------------------------------------


def _install_tracing(endpoint: str) -> None:
    # Deferred: the OTLP exporter import chain (requests/urllib3/charset_normalizer) costs ~50ms and is reached only on the endpoint-set path.
    from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter  # noqa: PLC0415
    from opentelemetry.sdk.trace import TracerProvider  # noqa: PLC0415
    from opentelemetry.sdk.trace.export import BatchSpanProcessor  # noqa: PLC0415
    from opentelemetry.trace import set_tracer_provider  # noqa: PLC0415

    # The exporter connects on force_flush, not construction, so stale endpoints fail after CLI work has emitted its Envelope.
    provider = TracerProvider(resource=Resource.create(_SERVICE))
    provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter(endpoint=endpoint), schedule_delay_millis=_DRAIN_MS))
    set_tracer_provider(provider)


def install_tracing(endpoint: str) -> None:
    """Install the optional trace provider when an endpoint is configured."""
    match endpoint:
        case "":
            pass
        case target:
            _install_tracing(target)


def bootstrap_error() -> ValidationError | None:
    """Return the import-time settings validation fault, if one occurred.

    Returns:
        Import-time settings validation error, or None when bootstrap settings were valid.
    """
    return _BOOTSTRAP_ERROR


# Configure logging once, and leave the default NoOpTracer when no endpoint is configured.
try:
    _SETTINGS = AssaySettings()
    _BOOTSTRAP_ERROR: ValidationError | None = None
except ValidationError as exc:
    # A malformed ASSAY_* env must not crash package import; logging falls back to field defaults and the
    # config fault surfaces as one FAULTED Envelope at dispatch (registry._dispatch / parse_fault).
    _BOOTSTRAP_ERROR = exc
    _SETTINGS = AssaySettings.model_construct()

configure_logging(_SETTINGS.log_format)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["bootstrap_error", "configure_logging", "install_tracing"]
