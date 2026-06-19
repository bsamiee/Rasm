# [PY_RUNTIME_RECEIPTS]

Local evidence production. `Receipt` is the one tagged-union evidence family (admitted/planned/emitted/rejected/drained rows); `ReceiptContributor` is the Protocol port every other package's typed receipt — data `QueryReceipt`, compute `GraduationReceipt`, geometry/artifacts `ArtifactReceipt` — wires through, never four parallel receipt rails. `Redaction` classifies fields before emission; the `structlog` processor chain injects OTel trace context and `psutil` supplies process facts. `Signals` carries both directions of the W3C trace-context seam: the outbound `trace_context` processor injects the active span's `trace_id`/`span_id` into every event, and the inbound `continue_inbound`/`attach` pair extracts the C#-minted parent off the gRPC carrier so the next measured span seeds from the host trace rather than minting a fresh root (the named cross-language drift defect). OTLP log egress rides the `observability/telemetry#TELEMETRY`-installed `LoggerProvider`; the package contributes receipts and structured facts, never the provider install, product telemetry export, or health.

## [1]-[INDEX]

One cluster: `[2]-[RECEIPT]` — the receipt union, the contributor port, redaction, signals, the inbound trace-context extract-and-continue.

## [2]-[RECEIPT]

- Owner: `Receipt` — the one evidence union with slot/kind metadata; `ReceiptContributor` the Protocol port the sibling typed receipts implement; `Redaction` the field-classification policy; `Signals` the static surface owning the `structlog` processor chain, the `psutil` process telemetry facts, and the inbound trace-context extract-and-continue, the trace context riding the processor chain rather than a cached tracer.
- Cases: the three lifecycle phases share one `fact` case carrying `(Phase, owner, subject, facts)` — `admitted`/`planned`/`emitted` are a `Phase` literal value the case routes, not three identical-payload sibling cases; `rejected` carries `(owner, subject, BoundaryFault)` and `drained` carries `(owner, DrainReceipt)` because their payloads differ; correlation flows through `merge_contextvars`, never a per-case field.
- Entry: `Receipt.of(phase, owner, subject, facts)` is the one phase-keyed factory; `ReceiptContributor.contribute` is the one method a sibling implements to feed its typed receipt into the stream; `Signals.emit` dispatches the three cases by `match` and never probes the union by `getattr`, redacting each fact map through `Redaction.apply` then writing the structlog event under the active span's `trace_id`/`span_id` from the `trace_context` processor; `Signals.continue_inbound(carrier)` runs `propagate.extract(carrier)` over the `dict[str, str]` carrier (resolved through `propagate.get_global_textmap`, the composite `observability/telemetry#TELEMETRY` installs) and returns the extracted `Context`, and `Signals.attach(context)` is the context-manager that activates it so the next measured span seeds from the C# parent.
- Auto: the `structlog` processor chain binds `merge_contextvars` (carrying the `Correlation`/`RuntimeContext` bound context), a custom `trace_context` processor reading `opentelemetry.trace.get_current_span()` to inject `trace_id`/`span_id` into every event, and a JSON renderer; span creation belongs to the measured operation, not to receipt emission, so `emit` writes the event under whatever span is active; OTLP log egress rides the `observability/telemetry#TELEMETRY`-installed `LoggerProvider`/`OTLPLogExporter` (the `structlog`-to-OTel bridge, since `structlog` mints no native OTLP log export) — this owner wires no private `LogRecordProcessor`/`OTLPLogExporter`; `continue_inbound` resolves the parent through the installed composite propagator, so before the install the extract reads the default no-op propagator and the C# parent is dropped (the mechanical reason the extract sequences after the install); `Redaction.apply` classifies each fact so a classified field never reaches a log line; `psutil.Process` rss attaches to a drained receipt; the logger is fetched per emit through `structlog.get_logger`, never cached as a module constant.
- Packages: `expression` (`tagged_union`/`case`/`tag`), `msgspec`, `structlog` (`get_logger`/`configure`/`contextvars.merge_contextvars`/`processors`), `opentelemetry-api` (`trace.get_current_span`, `propagate.extract` ENTRYPOINTS [6], `propagate.get_global_textmap` ENTRYPOINTS [8], `Context` PUBLIC_TYPES [5], `context.attach`/`context.detach` ENTRYPOINTS [2]/[3], `TextMapPropagator` PUBLIC_TYPES [7], `DefaultGetter` PUBLIC_TYPES [8]), `psutil` (`Process.memory_info`). The SDK `LoggerProvider`/`OTLPLogExporter` are consumed by `observability/telemetry#TELEMETRY`, never imported here.
- Growth: a new lifecycle phase is one `Phase` literal absorbed by the existing `fact` case; a distinct-payload evidence kind is one `Receipt` case with its own match arm; a new classified field is one `Redaction` row; a new processor is one entry in the `structlog` chain; zero new surface.
- Boundary: no AppHost envelope, health status, support-bundle capture, exporter ownership, provider install, or C# receipt minting; the suite classification taxonomy stays AppHost-owned; a per-package parallel receipt rail, a stdlib `logging` call outside the structlog bridge, a second tracer minted for the inbound extract, and a fresh-root span where the C# parent is on the inbound carrier are the deleted forms.

```python signature
from collections.abc import Iterator
from contextlib import contextmanager
from typing import Literal, Protocol, assert_never, runtime_checkable

import psutil
import structlog
from expression import case, tag, tagged_union
from msgspec import Struct
from opentelemetry import context, propagate, trace
from opentelemetry.context import Context

from rasm.runtime.faults import BoundaryFault
from rasm.runtime.lanes import DrainReceipt


type Phase = Literal["admitted", "planned", "emitted"]


@tagged_union(frozen=True)
class Receipt:
    tag: Literal["fact", "rejected", "drained"] = tag()
    fact: tuple[Phase, str, str, dict[str, str]] = case()
    rejected: tuple[str, str, BoundaryFault] = case()
    drained: tuple[str, DrainReceipt] = case()

    @staticmethod
    def of(phase: Phase, owner: str, subject: str, facts: dict[str, str]) -> "Receipt":
        return Receipt(fact=(phase, owner, subject, facts))


@runtime_checkable
class ReceiptContributor(Protocol):
    def contribute(self) -> Receipt: ...


class Redaction(Struct, frozen=True):
    classified: frozenset[str]

    def apply(self, facts: dict[str, str]) -> dict[str, str]:
        return {k: ("***" if k in self.classified else v) for k, v in facts.items()}


def trace_context(_: object, __: str, event: dict[str, object]) -> dict[str, object]:
    span = trace.get_current_span()
    ctx = span.get_span_context()
    if ctx.is_valid:
        event["trace_id"] = format(ctx.trace_id, "032x")
        event["span_id"] = format(ctx.span_id, "016x")
    return event


class Signals:
    @staticmethod
    def configure() -> None:
        structlog.configure(
            processors=[
                structlog.contextvars.merge_contextvars,
                structlog.processors.add_log_level,
                trace_context,
                structlog.processors.TimeStamper(fmt="iso"),
                structlog.processors.JSONRenderer(),
            ]
        )

    @staticmethod
    def continue_inbound(carrier: dict[str, str]) -> Context:
        return propagate.extract(carrier)

    @staticmethod
    @contextmanager
    def attach(parent: Context) -> Iterator[None]:
        token = context.attach(parent)
        try:
            yield
        finally:
            context.detach(token)

    @staticmethod
    def emit(receipt: Receipt, redaction: Redaction) -> None:
        log = structlog.get_logger()
        match receipt:
            case Receipt(tag="drained", drained=(owner, drain)):
                rss = psutil.Process().memory_info().rss
                log.info("drained", **redaction.apply({"owner": owner, "completed": str(drain.completed), "rss": str(rss)}))
            case Receipt(tag="rejected", rejected=(owner, subject, fault)):
                log.warning("rejected", **redaction.apply({"owner": owner, "subject": subject, "fault": fault.tag}))
            case Receipt(tag="fact", fact=(phase, owner, subject, fields)):
                log.info(phase, owner=owner, subject=subject, **redaction.apply(fields))
            case _ as unreachable:
                assert_never(unreachable)
```

## [3]-[RESEARCH]

- [OTLP_LOG_EGRESS]: reflection-confirmed — the `opentelemetry-sdk` `_logs.LogRecordProcessor`/`_logs.export.BatchLogRecordProcessor` log-egress wiring (no native `structlog` OTLP export) pairs with the `opentelemetry-exporter-otlp-proto-http` `OTLPLogExporter(endpoint, headers, timeout, compression, ...)` constructor, and `structlog.contextvars.merge_contextvars` is the bound-context processor. The provider/processor/exporter NOW INSTALL once at `observability/telemetry#TELEMETRY` (the composition-root install owner this campaign lands); this owner reads the installed `LoggerProvider` and wires no private `LogRecordProcessor`/`OTLPLogExporter`. The metric egress is the sibling `observability/metrics#METRIC` reading the installed `MeterProvider` over the same shared OTLP exporter family and one `Resource`. The log-egress wiring is settled.
- [TRACE_INBOUND_EXTRACT]: reflection-confirmed against the branch `libs/python/.api/opentelemetry-api.md` — `propagate.extract(carrier, context)` (ENTRYPOINTS [6]) decodes the W3C `traceparent`/`tracestate` carrier into a `Context` (PUBLIC_TYPES [5]) resolved through `propagate.get_global_textmap()` (ENTRYPOINTS [8], the composite `observability/telemetry#TELEMETRY` installs via `set_global_textmap` ENTRYPOINTS [9]); the carrier is a plain `dict[str, str]` per the `DefaultGetter` (PUBLIC_TYPES [8]) contract, and `context.attach`/`context.detach` (ENTRYPOINTS [2]/[3]) are the token-paired scoped activation. The `server/serve#SERVE` interceptor reads the carrier off `grpc.aio.ServicerContext.invocation_metadata()` projected to `dict[str, str]` and calls `continue_inbound` before the servicer body opens its span. Producer is `csharp:Rasm.AppHost/observability/diagnostics-and-telemetry#CORRELATION_SPINE` (the `TraceContextPropagator`/`BaggagePropagator` composite registered as `Propagators.DefaultTextMapPropagator`); the extract resolving through the default no-op propagator before the install is the mechanical reason this sequences after `observability/telemetry#TELEMETRY`. Realizes the cross-`libs/` `ONE_DISTRIBUTED_TRACE` Python leg. Spellings settled.
