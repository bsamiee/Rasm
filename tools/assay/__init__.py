"""Package marker: a stderr-bound structlog pipeline and an endpoint-gated OTel provider, installed at import.

The sole package-marker file (every other directory is a PEP 420 namespace package), running three
import-time boundary actions in order: the optional ``ASSAY_CLAW`` beartype claw gate, the once-only
structlog configure, and the endpoint-gated OTel ``TracerProvider`` install. Install only — the
``BatchSpanProcessor`` drain is owned by ``__main__``.
"""

# ruff: noqa: RUF067, E402  # executable package boundary: the claw gate is the FIRST statement so every import legitimately follows it (E402), and the module runs import-time side effects rather than re-exporting (RUF067)

# --- [COMPOSITION] ----------------------------------------------------------------------
# ASSAY_CLAW beartype gate is the FIRST statement: the claw rewrites submodules at import, so it must
# precede the first transitive `import tools.assay.core.model` (msgspec resolves field annotations
# lazily at first codec build under PEP 649/749).
import os
import sys
from typing import TYPE_CHECKING

from beartype import BeartypeConf
from beartype.claw import beartype_this_package


{"1": lambda: beartype_this_package(conf=BeartypeConf(is_pep484_tower=True, warning_cls_on_decorator_exception=None))}.get(
    os.environ.get("ASSAY_CLAW", ""),  # noqa: TID251  # claw gate runs PRE-import: it cannot route through AssaySettings, which itself imports core.model
    lambda: None,
)()

import msgspec
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.trace import set_tracer_provider
import structlog
from structlog import make_filtering_bound_logger, WriteLoggerFactory
from structlog.contextvars import bind_contextvars, merge_contextvars
from structlog.dev import ConsoleRenderer
from structlog.processors import add_log_level, dict_tracebacks, JSONRenderer, TimeStamper

from tools.assay.composition.settings import AssaySettings, LogFormat  # intra-package import; tools.assay is the package root
from tools.assay.core.aspect import ring_processor  # intra-package import; recent-events ring seam


if TYPE_CHECKING:
    from structlog.typing import Processor


# --- [CONSTANTS] ------------------------------------------------------------------------

_INFO: int = 20  # make_filtering_bound_logger floor; disabled levels compile to `return None`
_DRAIN_MS: int = 5000  # BatchSpanProcessor schedule delay; __main__ owns the matching force_flush
_SERVICE: dict[str, str] = {"service.name": "assay"}  # base span-identity mapping; agent_context extends it in _install_tracing
_ENDPOINT_ENV: str = "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"  # the only network gate this package opens


# --- [OPERATIONS] -----------------------------------------------------------------------


def _configure(log_format: LogFormat, agent_context: dict[str, str]) -> None:
    match log_format:
        # CI renderer routes through msgspec.json.encode: JSONRenderer's default json.dumps cannot encode structs/datetime.
        case LogFormat.CI:
            renderer: Processor = JSONRenderer(serializer=lambda v, **_k: msgspec.json.encode(v).decode())
        case LogFormat.HUMAN:
            renderer = ConsoleRenderer(colors=True)
    structlog.configure(
        processors=[
            merge_contextvars,  # FIRST: ContextVar isolation for @logged bind + the agent-context seed below
            ring_processor,  # AFTER merge_contextvars: captures every event WITH context into the live recent-events ring (pure pass-through)
            add_log_level,
            dict_tracebacks,  # bug channel, NOT the domain Fault rail
            TimeStamper(fmt="iso", utc=True),
            renderer,
        ],
        wrapper_class=make_filtering_bound_logger(_INFO),
        logger_factory=WriteLoggerFactory(file=sys.stderr),  # MANDATORY: default sink is stdout -> Envelope corruption
        cache_logger_on_first_use=True,
    )
    bind_contextvars(**agent_context)  # process-global {run.id, agent.task.id}: every log correlates, zero flags


def _install_tracing(endpoint: str, agent_context: dict[str, str]) -> None:
    # Constructing the exporter does NOT connect: the first POST is __main__'s force_flush drain, so a
    # stale endpoint surfaces as a flush timeout, never an import-time failure. One Resource.create over
    # {_SERVICE | agent_context} — a merge would collapse service.name to unknown_service.
    provider = TracerProvider(resource=Resource.create(_SERVICE | agent_context))
    provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter(endpoint=endpoint), schedule_delay_millis=_DRAIN_MS))
    set_tracer_provider(provider)


# --- [COMPOSITION] ----------------------------------------------------------------------
# First gate makes re-import a no-op (configure-once); the second is a no-op when the endpoint is
# empty, so the default NoOpTracer stands.
_SETTINGS = AssaySettings()  # validate once at the boundary; agent_context is pydantic-read, never os.environ

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
