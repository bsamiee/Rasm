# [PY_RUNTIME_OBSERVABILITY]

Local evidence production. `Receipt` is the one tagged-union evidence family (admitted/planned/emitted/rejected/drained rows); `ReceiptContributor` is the Protocol port every other package's typed receipt (data `QueryReceipt`, compute `GraduationReceipt`, geometry/artifacts `ArtifactReceipt`) wires through — not four parallel receipt rails. `Redaction` classifies fields before emission; structlog/OTel/psutil supply the signals. The package contributes receipts and structured facts; it never owns product telemetry export or health.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                      |
| :-----: | :-------- | :---------------------------------------------------------- |
|   [1]   | RECEIPT   | the receipt union, the contributor port, redaction, signals |

## [2]-[RECEIPT]

- Owner: `Receipt` — the one evidence union with slot/kind metadata; `ReceiptContributor` the Protocol port the sibling typed receipts implement; `Redaction` the field-classification policy; `Signals` the static surface emitting structlog events, OTel spans, and psutil process/system telemetry facts.
- Cases: `Receipt` cases `Admitted` · `Planned` · `Emitted` · `Rejected` · `Drained` — each a frozen `case()` carrying owner, subject, the fact map, and the correlation; three-or-more per-bucket constructions collapse into this one fact stream with kind metadata.
- Entry: `Receipt.of` constructs a row from owner/subject/facts/correlation; `ReceiptContributor.contribute` is the one method a sibling implements to feed its typed receipt into the stream; `Signals.emit` redacts then writes the structlog event and the OTel span.
- Auto: `Redaction.apply` classifies each fact before emission so a classified field never reaches a log or a span; `psutil.Process` facts (rss, cpu) attach to a drained receipt; the structlog logger and the OTel tracer are dependency-backed handles, never module constants.
- Packages: `msgspec`, `structlog` (`get_logger`/`BoundLogger`), `opentelemetry-api` (`trace.get_tracer`), `opentelemetry-sdk`, `opentelemetry-exporter-otlp-proto-http`, `psutil` (`Process`/`virtual_memory`).
- Growth: a new evidence kind is one `Receipt` case; a new signal target is one branch in `Signals.emit`; a new classified field is one `Redaction` row; zero new surface.
- Boundary: no AppHost envelope, health status, support-bundle capture, exporter ownership, or C# receipt minting; the suite classification taxonomy stays AppHost-owned; a per-package parallel receipt rail and a stdlib `logging` call outside the structlog bridge are the deleted forms.

```python signature
from typing import Literal, Protocol, runtime_checkable

import psutil
import structlog
from expression import case, tag, tagged_union
from msgspec import Struct
from opentelemetry import trace

from rasm.runtime.rails_resilience import BoundaryFault
from rasm.runtime.resources_lanes import DrainReceipt


@tagged_union(frozen=True)
class Receipt:
    tag: Literal["admitted", "planned", "emitted", "rejected", "drained"] = tag()
    admitted: tuple[str, str, dict[str, str]] = case()
    planned: tuple[str, str, dict[str, str]] = case()
    emitted: tuple[str, str, dict[str, str]] = case()
    rejected: tuple[str, str, BoundaryFault] = case()
    drained: tuple[str, DrainReceipt] = case()

    @staticmethod
    def Admitted(owner: str, subject: str, facts: dict[str, str]) -> "Receipt":
        return Receipt(admitted=(owner, subject, facts))

    @staticmethod
    def Planned(owner: str, subject: str, facts: dict[str, str]) -> "Receipt":
        return Receipt(planned=(owner, subject, facts))

    @staticmethod
    def Emitted(owner: str, subject: str, facts: dict[str, str]) -> "Receipt":
        return Receipt(emitted=(owner, subject, facts))

    @staticmethod
    def Rejected(owner: str, subject: str, fault: BoundaryFault) -> "Receipt":
        return Receipt(rejected=(owner, subject, fault))

    @staticmethod
    def Drained(owner: str, drain: DrainReceipt) -> "Receipt":
        return Receipt(drained=(owner, drain))


@runtime_checkable
class ReceiptContributor(Protocol):
    def contribute(self) -> Receipt: ...


class Redaction(Struct, frozen=True):
    classified: frozenset[str]

    def apply(self, facts: dict[str, str]) -> dict[str, str]:
        return {k: ("***" if k in self.classified else v) for k, v in facts.items()}


class Signals:
    _log: structlog.stdlib.BoundLogger = structlog.get_logger()
    _tracer = trace.get_tracer("rasm.python")

    @classmethod
    def emit(cls, receipt: Receipt, redaction: Redaction) -> None:
        match receipt:
            case Receipt(tag="drained", drained=(owner, drain)):
                proc = psutil.Process()
                cls._log.info("drained", owner=owner, completed=drain.completed, rss=proc.memory_info().rss)
            case _:
                cls._log.info("receipt", kind=receipt.tag)
```

## [3]-[RESEARCH]

- [OTEL_SPAN_BRIDGE]: the `opentelemetry` tracer-to-receipt span bridge (`tracer.start_as_current_span` attribute mapping) is verified against `opentelemetry-api>=1.41.1`; the structlog/OTel processor chain confirms against `.api/api-structlog.md` and `.api/api-opentelemetry-sdk.md` at fence transcription.
