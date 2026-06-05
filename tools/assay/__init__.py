"""Configure Assay import-time logging, tracing, and beartype checks."""

# ruff: noqa: RUF067, E402  # executable boundary: the claw gate runs before Assay imports.

# --- [COMPOSITION] ----------------------------------------------------------------------
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
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.trace import set_tracer_provider
import structlog
from structlog import make_filtering_bound_logger, WriteLogger
from structlog.contextvars import bind_contextvars, merge_contextvars
from structlog.dev import ConsoleRenderer
from structlog.processors import add_log_level, dict_tracebacks, JSONRenderer, TimeStamper

from tools.assay.composition.settings import AssaySettings, LogFormat
from tools.assay.core.aspect import ring_processor  # intra-package import; recent-events ring seam


if TYPE_CHECKING:
    from structlog.typing import Processor

# --- [CONSTANTS] ------------------------------------------------------------------------

_INFO: int = 20
_DRAIN_MS: int = 5000
_SERVICE: dict[str, str] = {"service.name": "assay"}
_ENDPOINT_ENV: str = "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"


def _configure(log_format: LogFormat, agent_context: dict[str, str]) -> None:
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
    bind_contextvars(**agent_context)


def _install_tracing(endpoint: str, agent_context: dict[str, str]) -> None:
    # The exporter connects on force_flush, not construction, so stale endpoints fail after CLI work has emitted its Envelope.
    # Resource.create must see `_SERVICE | agent_context`; a later merge would collapse service.name to unknown_service.
    provider = TracerProvider(resource=Resource.create(_SERVICE | agent_context))
    provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter(endpoint=endpoint), schedule_delay_millis=_DRAIN_MS))
    set_tracer_provider(provider)


# --- [COMPOSITION] ----------------------------------------------------------------------
# Configure logging once, and leave the default NoOpTracer when no endpoint is configured.
_SETTINGS = AssaySettings()

match structlog.is_configured():
    case False:
        _configure(_SETTINGS.log_format, _SETTINGS.agent_context)
    case True:
        pass

match os.environ.get(_ENDPOINT_ENV, ""):  # noqa: TID251  # OTel endpoint has no AssaySettings field (a 2nd BaseSettings is forbidden); the gate read is the import-time boundary, not domain logic
    case "":
        pass
    case endpoint:
        _install_tracing(endpoint, _SETTINGS.agent_context)
