# [PY_ARTIFACTS_NOTICE]

`TransmittalNotice` seals the settled ISO 19650 issue close as one trace-continuous CloudEvents envelope — a wire-ready announcement a downstream system ingests without opening the archive. `of` projects the landed `ArtifactReceipt.Transmittal` case and the issued register into one checked `cloudevents.v1.http.CloudEvent`: the event `type` reuses the `ArtifactHook.TRANSMITTAL_ISSUED` id so the wire grammar and the hook grammar stay one vocabulary, the `subject` carries the transmittal `ContentKey` hex, the issued-register row references ride the JSON data payload, evidence scalars ride spec-grammar extension attributes, and `propagate.inject` writes the active W3C trace and baggage fields into the attribute map — production context stays continuous into every notice consumer.

Envelope bytes are the terminal product: `sealed` lowers the checked event through the `cloudevents.v1.conversion` structured or binary row and returns `NoticeWire` headers-plus-body, and transport belongs to the composing app by the ruled python asymmetry — no broker client crosses this page. `issued` is the fold seam `delivery/transmittal#TRANSMITTAL` composes at `_emit`: mint, seal under the selected binding, fire the `NOTICE_ISSUED` observe row under the supplied runtime scope, and land a mint fault as receipt-stream evidence — the notice is the soft terminal member of the issue closure, never a break in the legal close.

## [01]-[INDEX]

- [01]-[NOTICE]: the `TransmittalNotice` envelope owner — receipt-and-register projection, extension-attribute vocabulary, W3C trace injection, the structured/binary binding rows, and the `issued` fold seam firing the hook registry.

## [02]-[NOTICE]

- Owner: `TransmittalNotice` holds the checked `CloudEvent` beside its transmittal `ContentKey` and event id — `CloudEvent.create(attributes, data)` is the one admission gate, and `GenericException` converts at that seam, so a held notice is well-formed by construction and `sealed` never fails. `NoticeWire` is the frozen wire value — `headers: frozendict[str, str]`, `body: bytes` — projected from the conversion's header/body tuple, and each `NoticeBinding` member carries its own `to_structured` or `to_binary` lowering callable.
- Attributes: the vocabulary is derivation, never invention — `id` mints through `uuid7().hex` (time-ordered event identity, distinct from the content key so replay identity and event identity never collide), `source` is the caller's issuing-app URI, `type` is `ArtifactHook.TRANSMITTAL_ISSUED.value`, `specversion` is `SPECVERSION_V1_0`, `subject` is the receipt `slot` hex, `time` is the aware emission instant, and `datacontenttype` is `application/json`. Extension attributes obey the spec's lowercase-alphanumeric grammar: the receipt case scalars land as `transmittalid`/`sheets`/`suitability`/`containerkey`/`validationstate`, the transmittal evidence scalars arrive as the caller's `extensions` mapping (`purpose`/`revision`/`issuedat`/`recordstate`/`padeslevel`/`lineage` from the transmittal fold), and the carrier lands by spreading `propagate.inject` output — an active trace contributes `traceparent` and optional `tracestate`, active tenant baggage contributes `baggage`, and absent context contributes no invented field; binary conversion lowers present fields to `ce-` headers and structured conversion carries them inline.
- Data: the payload answers what the archive holds — `register` the issued-register pre-run key hex and `rows` one entry per `Register.latest()` container (`reference`, `key`, `suitability` code, `revision` render, `title`) — so an ingesting system routes on rows without the workbook; heavy material never rides the notice, because the envelope references content-keyed artifacts rather than carrying them.
- Entry: `of(receipt, register)` is the one mint — it matches the `transmittal` receipt case, refuses any other kind through `BoundaryFault.config`, builds the attribute map, and converts the `GenericException` construction raise at the seam. `sealed(binding)` is total over a held notice; `issued(receipt, register, binding, scope)` composes both under the fire seam — the Ok arm runs `Production.fired(ArtifactHook.NOTICE_ISSUED, NoticeIssued(...), scope=scope)` carrying the stored event id, content type, bounded body length, and the `scoped` issue-baggage correlation id before returning the wire; either the mint or fire error arm emits the original fault through `Signals.emit` and returns that same failure, so every refused notice is stream evidence without a raise into the close.
- Packages: `cloudevents` (`cloudevents.v1.http.CloudEvent.create` the checked envelope, `cloudevents.v1.exceptions.GenericException` the boundary fault, `cloudevents.v1.conversion.to_structured`/`to_binary` the header/body tuple rows, `SPECVERSION_V1_0` the admitted protocol value); `opentelemetry-api` (`propagate.inject` the W3C carrier write, `context.get_current` the live context); `msgspec` (`Struct` the wire value); `expression` (the rail); the builtin `frozendict` (headers and extension rows); `uuid.uuid7` (event identity); core hooks (`ArtifactHook`/`NoticeIssued`/`Production`/`scoped`); `delivery/register#REGISTER` (`Register.latest`/`Register.key` the row projection); `core/receipt#RECEIPT` (`ArtifactReceipt` the projected case); runtime (`faults.BoundaryFault`/`RuntimeRail`, `identity.ContentKey`, `receipts.Signals`/`Receipt`/`OPEN` the fault evidence row).
- Growth: a new evidence scalar on the wire is one `extensions` entry at the transmittal fold; a new payload row field is one entry in the `rows` projection; a new content mode is one behavior-bearing `NoticeBinding` member; a new announced production fact rides the hooks page's point-row growth, never a second envelope owner.
- Boundary: this page holds no broker client, opens no span, and re-implements no spec algebra — attribute validation, JSON format, and binding header maps stay the SDK's; the trace context stays the runtime telemetry owner's, reached only through `propagate.inject`; the envelope announces the close and never gains authority over it — `ArtifactReceipt.Transmittal` remains the evidence truth, and the notice projects it. `delivery/transmittal#TRANSMITTAL` composes `issued` downward at `_emit`; nothing here imports the transmittal owner, so the delivery plane stays acyclic.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping
from datetime import UTC, datetime
from enum import StrEnum
from typing import Final, Self, assert_never
from uuid import uuid7

from builtins import frozendict
from cloudevents.core.spec import SPECVERSION_V1_0
from cloudevents.v1.conversion import to_binary, to_structured
from cloudevents.v1.exceptions import GenericException
from cloudevents.v1.http import CloudEvent
from expression import Error, Ok, Result
from msgspec import Struct
from opentelemetry import context as otel_context
from opentelemetry import propagate

from rasm.artifacts.core.hooks import ArtifactHook, NoticeIssued, Production, scoped
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.delivery.register import Register
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, Receipt, ScopeKey, Signals

# --- [TYPES] ----------------------------------------------------------------------------


class NoticeBinding(StrEnum):
    STRUCTURED = ("structured", to_structured)  # whole event as one application/cloudevents+json body
    BINARY = ("binary", to_binary)  # attributes as ce- headers, data as the raw body

    def __new__(
        cls,
        value: str,
        lower: Callable[[CloudEvent], tuple[Mapping[str, str], bytes]],
    ) -> Self:
        member = str.__new__(cls, value)
        member._value_ = value
        member._lower = lower
        return member

    def lower(self, event: CloudEvent, /) -> tuple[Mapping[str, str], bytes]:
        return self._lower(event)


# --- [CONSTANTS] ------------------------------------------------------------------------

_SOURCE: Final[str] = "//rasm/artifacts/delivery"

# --- [MODELS] ---------------------------------------------------------------------------


class NoticeWire(Struct, frozen=True, gc=False):
    headers: frozendict[str, str]
    body: bytes


class TransmittalNotice(Struct, frozen=True):
    event: CloudEvent
    key: ContentKey
    event_id: str

    @classmethod
    def of(
        cls,
        receipt: ArtifactReceipt,
        register: Register,
        /,
        *,
        source: str = _SOURCE,
        extensions: frozendict[str, str | int] = frozendict(),
    ) -> RuntimeRail[Self]:
        match receipt:
            case ArtifactReceipt(tag="transmittal", transmittal=(key, transmittal_id, sheets, suitability, container, validation_state)):
                carrier: dict[str, str] = {}
                propagate.inject(carrier, otel_context.get_current())  # active W3C trace + baggage fields
                event_id = uuid7().hex
                attributes: dict[str, object] = {
                    **extensions,
                    **carrier,
                    "id": event_id,
                    "source": source,
                    "type": ArtifactHook.TRANSMITTAL_ISSUED.value,
                    "specversion": SPECVERSION_V1_0,
                    "subject": key.hex,
                    "time": datetime.now(UTC),
                    "datacontenttype": "application/json",
                    "transmittalid": transmittal_id,
                    "sheets": sheets,
                    "suitability": suitability,
                    "containerkey": container,
                    "validationstate": validation_state,
                }
                rows = [
                    {
                        "reference": row.reference,
                        "key": row.asset_key,
                        "suitability": row.suitability.code,
                        "revision": row.revision.render(),
                        "title": row.title,
                    }
                    for row in register.latest()
                ]
                try:  # Exemption: the CloudEvent checked factory is the SDK's admission kernel — the one statement seam.
                    event = CloudEvent.create(attributes, {"register": register.key.hex, "rows": rows})
                    return Ok(cls(event=event, key=key, event_id=event_id))
                except GenericException as fault:
                    return Error(BoundaryFault(config=("artifacts.delivery.notice", str(fault))))
            case ArtifactReceipt(tag=kind):
                return Error(BoundaryFault(config=("artifacts.delivery.notice", f"non-transmittal-receipt:{kind}")))
            case _ as unreachable:
                assert_never(unreachable)

    def sealed(self, binding: NoticeBinding = NoticeBinding.STRUCTURED, /) -> NoticeWire:
        headers, body = binding.lower(self.event)
        return NoticeWire(headers=frozendict(headers), body=body)

    @classmethod
    def issued(
        cls,
        receipt: ArtifactReceipt,
        register: Register,
        /,
        *,
        source: str = _SOURCE,
        extensions: frozendict[str, str | int] = frozendict(),
        binding: NoticeBinding = NoticeBinding.STRUCTURED,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> RuntimeRail[NoticeWire]:
        match cls.of(receipt, register, source=source, extensions=extensions):
            case Result(tag="error", error=fault) as refused:
                Signals.emit(Receipt.of("artifacts.delivery.notice", fault), OPEN)  # mint fault is stream evidence, never a raise
                return refused
            case Result(tag="ok", ok=notice):
                wire = notice.sealed(binding)
                delivered = Production.fired(
                    ArtifactHook.NOTICE_ISSUED,
                    NoticeIssued(
                        key=notice.key.hex,
                        event_id=notice.event_id,
                        content_type=wire.headers["content-type"],
                        body_bytes=len(wire.body),
                        scope=scoped(otel_context.get_current()),
                    ),
                    scope=scope,
                )
                match delivered:
                    case Result(tag="error", error=fault) as failed:
                        Signals.emit(Receipt.of("artifacts.delivery.notice", fault), OPEN)
                        return failed
                    case Result(tag="ok"):
                        return Ok(wire)
                    case _ as unreachable:
                        assert_never(unreachable)
            case _ as unreachable:
                assert_never(unreachable)

# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ("NoticeBinding", "NoticeWire", "TransmittalNotice")
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
