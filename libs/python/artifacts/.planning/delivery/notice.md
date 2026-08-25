# [PY_ARTIFACTS_NOTICE]

`TransmittalNotice` projects the settled ISO 19650 issue occurrence onto the runtime message boundary.

Message-envelope algebra, format contract, protocol lowering, and delivery all seat at the runtime transport owner; this page ends at the message-envelope value and holds no attribute map, header spelling, content mode, or byte body.

## [01]-[INDEX]

- [02]-[NOTICE]: `TransmittalNotice` — issued-transmittal projection and confidentiality admission.

## [02]-[NOTICE]

- Owner: `TransmittalNotice` couples `TRANSMITTAL_POINT` to the typed issue projector consumed by the runtime emitter.
- Cases: event type, source, operation identity, content identity, and extensions derive from `TransmittalIssued`.
- Law: `subject` carries the PAYLOAD's content key, minted here over the encoded fact bytes through `evidence/identity#IDENTITY` and rendered at this mint through that owner's own `project("wire")` — the estate's ONE lowering site — into the `WireKey` slot the wire carries. Seating the pre-run aggregate key there instead collapses operation identity onto content identity, which is the one distinction `(source, id)` exists to hold.
- Law: the encoded `TransmittalIssued` fact is the payload; the authoritative register remains content-keyed.
- Law: `dataschema` stays absent because this producer publishes no absolute URI identifying a schema document for the msgspec body. A registry subject, package coordinate, generated type name, or event-type major is not substituted for that URI; registry and contract generations stay composition configuration outside the envelope.
- Law: `dataref` stays absent at this projection. `transport/binding#BINDING` alone externalizes an oversized payload through its bound residence and writes the answered `ResourceRef.path` into that generated URI-reference column, under the selected binding's `Retain` row. `expirytime` also stays absent: this legal announcement has no producer validity cutoff, and broker retention or resource aging is custody policy rather than event semantics.
- Law: `_CLASSIFIED` admits the ISO 19650 confidentiality spelling before it enters the generated extension field.
- Law: the creation-time W3C trace injects here and the transport hop's carrier stays the binding's. Artifacts taps run synchronously inside the fire, so `context.get_current()` at this projection is still the producing fold's context and the injected carrier fills the roster's own W3C slots through `TRACE_SLOTS` rather than three spelled names.
- Entry: `projections()` exposes the issued point and `announce()` projects its typed fact.
- Packages: `msgspec` encodes the payload; OpenTelemetry injects trace context; runtime event and binding owners carry the envelope and projection.
- Growth: a new routed transmittal scalar is one `TransmittalIssued` field; a new confidentiality spelling is one `_CLASSIFIED` row.
- Boundary: fact-to-message-envelope projection only. This page mints no message-envelope algebra, format, header map, wire value, content mode, or broker client, imports no artifacts sibling above the floor, and fires no hook of its own — the runtime `Delivery` settlement carries what the fan answered. Rejected: a lowering callable on an enum member; a frozen struct holding a mutable event; a `frozendict[str, str]` extension passthrough minting spec-invalid names; an event-format media type substituted for the payload's `datacontenttype`; a caller-supplied `source`; a register row projection beside the content-keyed artifact it copies.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final

from expression import Some
from expression.collections import Block, Map
from msgspec import Raw, Struct, json
from opentelemetry import context as otel_context
from opentelemetry import propagate
from protobuf.wkt import Timestamp

from rasm.artifacts.core.hooks import DOMAIN, TRANSMITTAL_POINT, ArtifactsLeg, TransmittalIssued
# Contracts are retired from this logic.
from rasm.runtime.admission import Classification
from rasm.runtime.binding import Projection
from rasm.runtime.event import (
    TRACE_SLOTS,
    EventType,
    MessageEnvelope,
    OperationId,
    Source,
)
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, rostered
from rasm.runtime.identity import ContentIdentity

# --- [CONSTANTS] ------------------------------------------------------------------------

_CAPABILITY: Final[str] = "delivery"
_SUBJECT: Final[str] = "delivery"
_FACT: Final[str] = "issued"

_PAYLOAD_FMT: Final[str] = "transmittal-notice"

_CLASSIFIED: Final[Map[str, Classification]] = Map.of_seq([
    ("public", Classification.PUBLIC),
    ("internal", Classification.INTERNAL),
    ("confidential", Classification.RESTRICTED),
    ("restricted", Classification.RESTRICTED),
    ("secret", Classification.SECRET),
])

NOTICE_CONFIDENTIALITY: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.NOTICE,
    point="classification",
    arm="config",
    defect="unknown-confidentiality",
    retriability=TERMINAL,
    slots=("value",),
)
RAISES: Final = rostered(Block.singleton(NOTICE_CONFIDENTIALITY))

# --- [MODELS] ---------------------------------------------------------------------------


class TransmittalNotice(Struct, frozen=True, gc=False):

    def projections(self) -> Block[Projection]:
        return Block.singleton(Projection.of(TRANSMITTAL_POINT, self.announce))

    def announce(self, fact: TransmittalIssued, /) -> RuntimeRail[MessageEnvelope]:
        body = json.encode(fact)
        carrier: dict[str, str] = {}
        propagate.inject(carrier, otel_context.get_current())
        return _classification(fact.confidentiality).bind(
            lambda classification: EventType.of(DOMAIN, _SUBJECT, _FACT).bind(
                lambda event_type: Source.of(DOMAIN, _CAPABILITY).map(
                    lambda source: MessageEnvelope(
                        event_type=event_type,
                        source=source,
                        operation=OperationId(value=fact.scope),
                        occurred=fact.occurred,
                        payload=Raw(body),
                        content_type=Some("application/json"),
                        subject=Some(ContentIdentity.key(_PAYLOAD_FMT, body).project("wire")),
                        extensions=Extensions(
                            **{slot: carrier[slot] for slot in TRACE_SLOTS if slot in carrier},
                            dataclassification=classification.value,
                            partitionkey=fact.transmittal_id,
                            sequence=f"{fact.revision_ordinal:020d}",
                            recordedtime=Timestamp.now(),
                        ),
                    )
                )
            )
        )


def _classification(raw: str, /) -> RuntimeRail[Classification]:
    normalized = raw.casefold().strip()
    return _CLASSIFIED.try_find(normalized).to_result_with(
        lambda: NOTICE_CONFIDENTIALITY.raised(normalized or "<absent>")
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("TransmittalNotice",)
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
