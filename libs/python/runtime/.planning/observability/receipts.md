# [PY_RUNTIME_RECEIPTS]

Local evidence production. `Receipt` is the one tagged-union evidence family (admitted/planned/emitted/rejected/drained rows); `ReceiptContributor` is the Protocol port every other package's typed receipt — data `QueryReceipt`, compute `GraduationReceipt`, geometry/artifacts `ArtifactReceipt` — wires through, never four parallel receipt rails. `Redaction` classifies fields before emission; the `structlog` processor chain injects OTel trace context and `psutil` supplies process facts. The package contributes receipts and structured facts; it never owns product telemetry export or health.

## [1]-[INDEX]

One cluster: `[2]-[RECEIPT]` — the receipt union, the contributor port, redaction, signals.

## [2]-[RECEIPT]

- Owner: `Receipt` — the one evidence union with slot/kind metadata; `ReceiptContributor` the Protocol port the sibling typed receipts implement; `Redaction` the field-classification policy; `Signals` the static surface owning the `structlog` processor chain and the `psutil` process telemetry facts, the trace context riding the processor chain rather than a cached tracer.
- Cases: the three lifecycle phases share one `fact` case carrying `(Phase, owner, subject, facts)` — `admitted`/`planned`/`emitted` are a `Phase` literal value the case routes, not three identical-payload sibling cases; `rejected` carries `(owner, subject, BoundaryFault)` and `drained` carries `(owner, DrainReceipt)` because their payloads differ; correlation flows through `merge_contextvars`, never a per-case field.
- Entry: `Receipt.of(phase, owner, subject, facts)` is the one phase-keyed factory; `ReceiptContributor.contribute` is the one method a sibling implements to feed its typed receipt into the stream; `Signals.emit` dispatches the three cases by `match` and never probes the union by `getattr`, redacting each fact map through `Redaction.apply` then writing the structlog event under the active span's `trace_id`/`span_id` from the `trace_context` processor.
- Auto: the `structlog` processor chain binds `merge_contextvars` (carrying the `Correlation`/`RuntimeContext` bound context), a custom `trace_context` processor reading `opentelemetry.trace.get_current_span()` to inject `trace_id`/`span_id` into every event, and a JSON renderer; span creation belongs to the measured operation, not to receipt emission, so `emit` writes the event under whatever span is active; OTLP log egress rides a `LogRecordProcessor`/`OTLPLogExporter` since `structlog` mints no native OTLP log export; `Redaction.apply` classifies each fact so a classified field never reaches a log line; `psutil.Process` rss attaches to a drained receipt; the logger is fetched per emit through `structlog.get_logger`, never cached as a module constant.
- Packages: `expression` (`tagged_union`/`case`/`tag`), `msgspec`, `structlog` (`get_logger`/`configure`/`contextvars.merge_contextvars`/`processors`), `opentelemetry-api` (`trace.get_current_span`), `opentelemetry-sdk` (`LogRecordProcessor`), `opentelemetry-exporter-otlp-proto-http` (`OTLPLogExporter`), `psutil` (`Process.memory_info`).
- Growth: a new lifecycle phase is one `Phase` literal absorbed by the existing `fact` case; a distinct-payload evidence kind is one `Receipt` case with its own match arm; a new classified field is one `Redaction` row; a new processor is one entry in the `structlog` chain; zero new surface.
- Boundary: no AppHost envelope, health status, support-bundle capture, exporter ownership, or C# receipt minting; the suite classification taxonomy stays AppHost-owned; a per-package parallel receipt rail and a stdlib `logging` call outside the structlog bridge are the deleted forms.

```python signature
from typing import Literal, Protocol, assert_never, runtime_checkable

import psutil
import structlog
from expression import case, tag, tagged_union
from msgspec import Struct
from opentelemetry import trace

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

- [OTLP_LOG_EGRESS]: the `LogRecordProcessor`/`OTLPLogExporter` log-egress wiring (no native `structlog` OTLP export) and the `structlog.contextvars.merge_contextvars` processor spelling are verified against the `structlog` and `opentelemetry-sdk` catalogues; the `OTLPLogExporter` constructor arity confirms at fence transcription.
