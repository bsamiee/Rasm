# [PY_ARTIFACTS_NOTICE]

`TransmittalNotice` announces the settled ISO 19650 issue close on the wire. It is the plane's one projection row: an `observe` subscriber over the `TRANSMITTAL_ISSUED` hook fact that answers a `runtime/transport/event#MESSAGE` `MessageEnvelope`, so the issue fold fires its fact once and never learns a transport exists.

Message-envelope algebra, format contract, protocol lowering, and delivery all seat at the runtime transport owner; this page ends at the message-envelope value and holds no attribute map, header spelling, content mode, or byte body.

## [01]-[INDEX]

- [02]-[NOTICE]: `TransmittalNotice` — the fact-to-message-envelope projection, its attribute derivations off the fired identities, and the generated extension roster it fills.

## [02]-[NOTICE]

- Owner: `TransmittalNotice` is one `Projection` row coupling the `TRANSMITTAL_ISSUED` point to its typed projector, handed to `transport/binding#BINDING`'s `Emitter` at the composition root beside the bindings that composition dials. Direction runs OUTWARD from the fact: the transmittal fold fires, the registry fans, and this projection answers a message envelope — never a call the producing fold makes and discards. Fault isolation is the registry's `OBSERVE` contract, so a refused projection lands on the receipt stream while the issue close stays whole.
- Cases: every attribute derives from an identity the fired fact already carries. `type` is `EventType.of(_DOMAIN, _SUBJECT, _FACT, _VERSION)` under the `observability/metrics#METRIC` `artifact` segment. `source` states the producing `_CAPABILITY` explicitly under the Rasm `rasm:<domain>/<capability>` URI-reference profile; it is not inferred from `type.subject`, and profile composition proves domain agreement only. `id` is `fact.scope`, the `core/issue#ISSUE` UUIDv7 minted once around the producing issue operation. The pre-run aggregate `fact.key` remains reuse/content scheduling identity and never crosses as event operation identity.
- Law: `subject` carries the PAYLOAD's content key, minted here over the encoded fact bytes through `evidence/identity#IDENTITY` and rendered at this mint through that owner's own `project("wire")` — the estate's ONE lowering site — into the `WireKey` slot the wire carries. Seating the pre-run aggregate key there instead collapses operation identity onto content identity, which is the one distinction `(source, id)` exists to hold.
- Law: the payload IS the fired fact. One `msgspec` JSON encode answers the `Raw` band every format lowers, so no second projection re-spells the evidence and the announcement cannot disagree with the receipt it projects. `datacontenttype` is `application/json`, because it describes the DATA independently of the enclosing JSON/protobuf/Avro event format; leaving it absent would let a binary payload cross formats under an implied media type no producer stated. The issued register rides as its own content key rather than a re-spelled copy of its rows — an unbounded row set defeats every frame budget `transport/binding#BINDING` declares, and a consumer resolves the authoritative register from the key.
- Law: `dataschema` stays absent because this producer publishes no absolute URI identifying a schema document for the msgspec body. A registry subject, package coordinate, generated type name, or event-type major is not substituted for that URI; registry and contract generations stay composition configuration outside the envelope.
- Law: `dataref` stays absent at this projection. `transport/binding#BINDING` alone externalizes an oversized payload through its bound residence and writes the answered `ResourceRef.path` into that generated URI-reference column, under the selected binding's `Retain` row. `expirytime` also stays absent: this legal announcement has no producer validity cutoff, and broker retention or resource aging is custody policy rather than event semantics.
- Law: the extension roster is the generated `rasm.contracts.event.Extensions` `transport/event#MESSAGE` derives its codecs off, and this owner fills only columns the issue proves by keyword — an unproved column is not passed, so wire presence is `has_field`. `partitionkey` is the transmittal id; `sequence` is `RevisionCode.ordinal` formatted directly as zero-padded D20 text, so lexical order equals unsigned ordinal order; `recordedtime` is the producer creation instant minted at this projection. Issuing party, scope, evidence states, and gate grade remain typed payload facts instead of private extension names.
- Law: `dataclassification` resolves AT this boundary and nowhere inside. ISO 19650 confidentiality is free header text, so `_CLASSIFIED` keys the estate grade vocabulary off the folded value. An absent or unknown spelling REFUSES on `NOTICE_CONFIDENTIALITY`; defaulting it to `INTERNAL` would silently weaken an unclassified issue. The extension carries the admitted canonical string value; `transport/binding#BINDING` admits it back through `Classification` before `CLASSIFICATION_ROWS` decides which broker the issue may cross.
- Law: the creation-time W3C trace injects here and the transport hop's carrier stays the binding's. Artifacts taps run synchronously inside the fire, so `context.get_current()` at this projection is still the producing fold's context and the injected carrier fills the roster's own W3C slots through `TRACE_SLOTS` rather than three spelled names.
- Entry: `projections()` answers the heterogeneous `Block[Projection]` the `Emitter` composes, and `announce(fact)` is the one typed arm.
- Packages: `msgspec` (`Struct` the projection value, `Raw` the payload band, `json.encode` the fact bytes); `expression` (`Block`, `Map`, and `Some`); `opentelemetry-api` (`propagate.inject` the creation-trace write, `context.get_current` the live context); `protobuf-py` (`Timestamp.now` the producer creation stamp); core hooks (`TRANSMITTAL_POINT`/`TransmittalIssued`); runtime (`event.MessageEnvelope`/`EventType`/`Source`/`OperationId`/`TRACE_SLOTS` the message-envelope algebra, `rasm.contracts` the generated `Extensions` roster, `binding.Projection` the point-coupled emitter adapter, `admission.Classification` the grade vocabulary, `identity.ContentIdentity` the payload key, `faults.RuntimeRail`). No `cloudevents` member crosses this page — the message-envelope owner composes the distribution.
- Growth: a new announced fact is one projection row keyed by its own point id; a new routed scalar is one `TransmittalIssued` field the encoded payload carries; a new confidentiality spelling is one `_CLASSIFIED` row; a new binding, format, or content mode reaches this announcement untouched, because each is a row at the transport owner.
- Boundary: fact-to-message-envelope projection only. This page mints no message-envelope algebra, format, header map, wire value, content mode, or broker client, imports no artifacts sibling above the floor, and fires no hook of its own — the runtime `Delivery` receipt carries what the fan answered. Rejected: a lowering callable on an enum member; a frozen struct holding a mutable event; a `frozendict[str, str]` extension passthrough minting spec-invalid names; an event-format media type substituted for the payload's `datacontenttype`; a caller-supplied `source`; a register row projection beside the content-keyed artifact it copies.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final

from expression import Some
from expression.collections import Block, Map
from msgspec import Raw, Struct, json
from opentelemetry import context as otel_context
from opentelemetry import propagate
from protobuf.wkt import Timestamp

from rasm.artifacts.core.hooks import TRANSMITTAL_POINT, ArtifactsLeg, TransmittalIssued
from rasm.contracts.rasm.contracts.event.event_pb import Extensions
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

_DOMAIN: Final[str] = "artifact"
_CAPABILITY: Final[str] = "delivery"
_SUBJECT: Final[str] = "delivery"
_FACT: Final[str] = "issued"
_VERSION: Final[int] = 1

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
            lambda classification: EventType.of(_DOMAIN, _SUBJECT, _FACT, _VERSION).bind(
                lambda event_type: Source.of(_DOMAIN, _CAPABILITY).map(
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
