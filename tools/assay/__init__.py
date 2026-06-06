"""Configure Assay import-time logging, tracing, and beartype checks."""
# ruff: noqa: RUF067, E402  # executable boundary: the claw gate runs before Assay imports.

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
# ASSAY_CLAW must run before the first Assay import because beartype rewrites submodules while msgspec resolves annotations lazily.
import os
import sys
from typing import TYPE_CHECKING

from beartype import BeartypeConf
from beartype.claw import beartype_this_package


{"1": lambda: beartype_this_package(conf=BeartypeConf(is_pep484_tower=True, warning_cls_on_decorator_exception=None))}.get(
    os.environ.get("ASSAY_CLAW", ""),  # noqa: TID251  # pre-import claw gate cannot route through AssaySettings, which imports core.model
    lambda: None,
)()

import msgspec
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.trace import set_tracer_provider
from pydantic import ValidationError
import structlog
from structlog import make_filtering_bound_logger, WriteLogger
from structlog.contextvars import clear_contextvars, merge_contextvars
from structlog.dev import ConsoleRenderer
from structlog.processors import add_log_level, dict_tracebacks, JSONRenderer, TimeStamper

from tools.assay.composition.settings import AssaySettings, LogFormat
from tools.assay.core.aspect import ring_processor  # intra-package import; recent-events ring seam


if TYPE_CHECKING:
    from structlog.typing import Processor

# --- [CONSTANTS] ------------------------------------------------------------------------

_INFO: int = 20
_DRAIN_MS: int = 1500  # single ~1.5s bound: BatchSpanProcessor schedule cadence here and the exit-time force_flush budget in __main__.main
_SERVICE: dict[str, str] = {"service.name": "assay"}


# --- [COMPOSITION] ----------------------------------------------------------------------


def _configure(log_format: LogFormat) -> None:
    match log_format:
        # msgspec keeps CI logs compatible with Assay structs and datetimes.
        case LogFormat.CI:
            renderer: Processor = JSONRenderer(serializer=lambda v, **_k: msgspec.json.encode(v).decode())
        case LogFormat.HUMAN:
            renderer = ConsoleRenderer(colors=True)
    structlog.configure(
        processors=[
            merge_contextvars,  # first so @logged binds and agent context stay isolated per ContextVar
            ring_processor,  # captures contextualized events into the recent-events ring
            add_log_level,
            dict_tracebacks,
            TimeStamper(fmt="iso", utc=True),
            renderer,
        ],
        wrapper_class=make_filtering_bound_logger(_INFO),
        logger_factory=lambda *_args: WriteLogger(sys.stderr),  # stdout belongs to the Envelope wire
        cache_logger_on_first_use=False,
    )
    clear_contextvars()


def _install_tracing(endpoint: str) -> None:
    # Deferred: the OTLP exporter import chain (requests/urllib3/charset_normalizer) costs ~50ms and is reached only on the endpoint-set path.
    from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter  # noqa: PLC0415
    from opentelemetry.sdk.trace.export import BatchSpanProcessor  # noqa: PLC0415

    # The exporter connects on force_flush, not construction, so stale endpoints fail after CLI work has emitted its Envelope.
    provider = TracerProvider(resource=Resource.create(_SERVICE))
    provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter(endpoint=endpoint), schedule_delay_millis=_DRAIN_MS))
    set_tracer_provider(provider)


# Configure logging once, and leave the default NoOpTracer when no endpoint is configured.
try:
    _SETTINGS = AssaySettings()
except ValidationError:
    # A malformed ASSAY_* env must not crash package import; logging falls back to field defaults and the
    # config fault surfaces as one FAULTED Envelope at dispatch (registry._dispatch / parse_fault).
    _SETTINGS = AssaySettings.model_construct()

_configure(_SETTINGS.log_format)

match _SETTINGS.otel_endpoint:
    case "":
        pass
    case endpoint:
        _install_tracing(endpoint)
