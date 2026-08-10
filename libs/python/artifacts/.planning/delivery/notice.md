# [PY_ARTIFACTS_NOTICE]

`TransmittalNotice` announces the settled ISO 19650 issue close on the wire. It is the plane's one projection row: an `observe` subscriber over the `TRANSMITTAL_ISSUED` hook fact that answers a `runtime/transport/event#MESSAGE` `MessageEnvelope`, so the issue fold fires its fact once and never learns a transport exists.

Message-envelope algebra, format contract, protocol lowering, and delivery all seat at the runtime transport owner; this page ends at the message-envelope value and holds no attribute map, header spelling, content mode, or byte body.

## [01]-[INDEX]

- [02]-[NOTICE]: `TransmittalNotice` — the fact-to-message-envelope projection, its attribute derivations off the fired identities, the typed extension roster it fills, and the DSSE signer port.

## [02]-[NOTICE]

- Owner: `TransmittalNotice` is one `Project` row keyed by the `TRANSMITTAL_ISSUED` point id, handed to `transport/binding#BINDING`'s `Emitter` at the composition root beside the bindings that composition dials. Direction runs OUTWARD from the fact: the transmittal fold fires, the registry fans, and this projection answers a message envelope — never a call the producing fold makes and discards. Fault isolation is the registry's `OBSERVE` contract, so a refused projection lands on the receipt stream while the issue close stays whole.
- Cases: every attribute derives from an identity the fired fact already carries. `type` is `EventType.of(_DOMAIN, _SUBJECT, _FACT, _VERSION)` under the `observability/metrics#METRIC` `artifact` capability segment the receipt fold already records against, so a board and a subscription name one capability. `source` derives from that same value, naming the producing capability and never a host or a caller override. `id` is the transmittal's PRE-RUN aggregate key, which is operation identity exactly: the reuse fabric elides two runs over identical inputs onto one, and a retry replays the value every dedup window reads through `(source, id)`.
- Law: `subject` carries the PAYLOAD's content key, minted here over the encoded fact bytes through `evidence/identity#IDENTITY` and rendered at this mint through that owner's own `project("wire")` — the estate's ONE lowering site — into the `WireKey` slot the wire carries. Seating the pre-run aggregate key there instead collapses operation identity onto content identity, which is the one distinction `(source, id)` exists to hold.
- Law: the payload IS the fired fact. One `msgspec` encode answers the `Raw` band the message envelope signs and every format lowers, so no second projection re-spells the evidence and the announcement cannot disagree with the receipt it projects. `datacontenttype` stays absent because the format row that encodes names it, and the issued register rides as its own content key rather than a re-spelled copy of its rows — an unbounded row set defeats every frame budget `transport/binding#BINDING` declares, and a consumer resolves the authoritative register from the key.
- Law: the extension roster is `transport/event#MESSAGE`'s typed `Extensions` and this owner fills the slots the issue proves. `partitionkey` is the transmittal id, so one transmittal's whole revision stream stays ordered inside one partition; `sequence` is `RevisionCode.ordinal` under `Sequencing.INTEGER`, so a consumer reads a gap as a missed revision; `authcontext` is the issuing party, the same named actor the durable leg records; `correlation` is the issue-scope baggage id every payload carries. Scalars the roster does not name ride the payload, never an invented attribute name.
- Law: `dataclassification` resolves AT this boundary and nowhere inside. ISO 19650 confidentiality is free header text, so `_CLASSIFIED` keys the estate grade vocabulary off the folded value and an unspelled or absent one reads `INTERNAL` — an unclassified issue is not a publishable one, and the resolved grade is what makes `transport/binding#BINDING`'s `CLASSIFICATION_ROWS` refuse a broker the issue may not cross.
- Law: the creation-time W3C trace injects here and the transport hop's carrier stays the binding's. Artifacts taps run synchronously inside the fire, so `context.get_current()` at this projection is still the producing fold's context and the injected carrier fills the roster's own W3C slots through `TRACE_SLOTS` rather than three spelled names.
- Entry: `projections()` answers the point-keyed `Map` the `Emitter` composes, and `announce(fact)` is the one arm. `signer` is an `Option` port bound at the composition root: bound, the message envelope's own `signed` lands the DSSE material over the published attribute digest; unbound, no attribute appears at all.
- Packages: `msgspec` (`Struct` the projection value, `Raw` the payload band, `json.encode` the fact bytes); `expression` (`Option`/`Map`/`Block` and the rail); `opentelemetry-api` (`propagate.inject` the creation-trace write, `context.get_current` the live context); core hooks (`ArtifactHook`/`TransmittalIssued`); runtime (`event.MessageEnvelope`/`EventType`/`Source`/`OperationId`/`Extensions`/`Severity`/`Sequencing`/`TRACE_SLOTS` the message-envelope algebra, `binding.Project` the emitter row type, `admission.Classification` the grade vocabulary, `identity.ContentIdentity` the payload key, `faults.RuntimeRail`). No `cloudevents` member crosses this page — the message-envelope owner composes the distribution.
- Growth: a new announced fact is one projection row keyed by its own point id; a new routed scalar is one `TransmittalIssued` field the encoded payload carries; a new confidentiality spelling is one `_CLASSIFIED` row; a new evidence state is one `_GRADE_LADDER` row; a new binding, format, or content mode reaches this announcement untouched, because each is a row at the transport owner.
- Boundary: fact-to-message-envelope projection only. This page mints no message-envelope algebra, format, header map, wire value, content mode, or broker client, imports no artifacts sibling above the floor, and fires no hook of its own — the runtime `Delivery` receipt carries what the fan answered. Rejected: a lowering callable on an enum member; a frozen struct holding a mutable event; a `frozendict[str, str]` extension passthrough minting spec-invalid names; a literal `datacontenttype` beside the format row that owns it; a caller-supplied `source`; a register row projection beside the content-keyed artifact it copies.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Final

from expression import Nothing, Option, Some
from expression.collections import Block, Map
from msgspec import Raw, Struct, json
from opentelemetry import context as otel_context
from opentelemetry import propagate

from rasm.artifacts.core.hooks import ArtifactHook, TransmittalIssued
from rasm.runtime.admission import Classification
from rasm.runtime.binding import Project
from rasm.runtime.event import (
    TRACE_SLOTS,
    EventType,
    Extensions,
    MessageEnvelope,
    OperationId,
    Sequencing,
    Severity,
    Source,
)
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity

# --- [CONSTANTS] ------------------------------------------------------------------------

# `_DOMAIN` names the METRIC roster's own `artifact` subject the receipt fold already records under, so a
# board and a subscription join ONE vocabulary; `delivery` is the plane's noun and `issued` reads past tense. The
# hook id spells `rasm.artifacts.delivery.issued` under the registry's own grammar and carries no version segment,
# so the event type derives from the roster rather than from that id — one segment apart, and both spelled once.
_DOMAIN: Final[str] = "artifact"
_SUBJECT: Final[str] = "delivery"
_FACT: Final[str] = "issued"
_VERSION: Final[int] = 1

# `_PAYLOAD_FMT` tags the payload key's own namespace, distinct from every transmittal-stage tag so a notice key
# and an issue key never collide inside one content-keyed store.
_PAYLOAD_FMT: Final[str] = "transmittal-notice"

# `dataschema` binds the registry SUBJECT and its version together, and the type major moves with that version
# rather than beside it; the reference is relative for the same reason `source` is — an absolute form pins a host a
# redeployment re-authors.
_REGISTRY: Final[str] = "//rasm/registry"

# ISO 19650 confidentiality is free header text, so the grade resolves at THIS boundary: the folded value keys the
# estate vocabulary and an unspelled or absent one reads INTERNAL, never PUBLIC — an unclassified issue is not a
# publishable one, and the grade is what every broker row reads to refuse a crossing.
_CLASSIFIED: Final[Map[str, Classification]] = Map.of_seq([
    ("public", Classification.PUBLIC),
    ("internal", Classification.INTERNAL),
    ("confidential", Classification.RESTRICTED),
    ("restricted", Classification.RESTRICTED),
    ("secret", Classification.SECRET),
])

# Grade reads the WEAKEST evidence state off one DESCENDING ladder: a failed proof degrades and an unproven one is
# notable, while a state no row names contributes no grade at all rather than asserting one over evidence nobody
# took. Neither the signature nor the record proof carries a branch of its own, and a third proof is one ladder row.
_GRADE_LADDER: Final[Block[tuple[frozenset[str], Severity]]] = Block.of_seq([
    (frozenset({"invalid"}), Severity.DEGRADED),
    (frozenset({"unsigned", "unverified"}), Severity.NOTABLE),
])

# --- [MODELS] ---------------------------------------------------------------------------


class TransmittalNotice(Struct, frozen=True, gc=False):
    # ONE projection row handed to the runtime `Emitter` at the composition root. Lowering, binding selection,
    # format, and delivery are the emitter's, so this owner ends at the message-envelope VALUE and holds no wire bytes,
    # header map, or content mode. `signer` is a PORT: bound, the DSSE material lands over the published attribute
    # digest; unbound, no attribute appears — never a hand-rolled signature beside the message envelope's own.
    signer: Option[Callable[[bytes], bytes]] = Nothing

    def projections(self) -> Map[str, Project[Struct]]:
        return Map.of_seq([(ArtifactHook.TRANSMITTAL_ISSUED.value, self.announce)])

    def announce(self, fact: TransmittalIssued, /) -> RuntimeRail[MessageEnvelope]:
        # Payload IS the fired fact: one encode answers the `Raw` band the digest signs and every format lowers,
        # so no second projection re-spells the evidence and the announcement cannot disagree with the receipt it
        # projects. `datacontenttype` stays absent because the encoding format row names it.
        body = json.encode(fact)
        # CREATION-time trace. Artifacts taps run synchronously inside the fire, so this context is still the
        # producing fold's; the hop's own carrier stays the binding's and never folds onto these slots.
        carrier: dict[str, str] = {}
        propagate.inject(carrier, otel_context.get_current())
        return (
            EventType.of(_DOMAIN, _SUBJECT, _FACT, _VERSION)
            .bind(
                lambda event_type: Source.of(f"//rasm/{event_type.domain}/{event_type.subject}").map(
                    lambda source: MessageEnvelope(
                        event_type=event_type,
                        source=source,
                        # PRE-RUN aggregate key: the reuse fabric elides two runs over identical inputs onto one,
                        # so this value IS operation identity and a retry replays what every dedup window reads.
                        operation=OperationId(value=fact.key),
                        occurred=fact.occurred,
                        payload=Raw(body),
                        # PAYLOAD content key, minted over the encoded bytes and lowered HERE through the key
                        # owner's own `project("wire")` into the WireKey slot — the slot holds what crosses, so
                        # `decoded` rebuilds exactly, and seating the pre-run key here would collapse the two
                        # identities `(source, id)` exists to hold apart.
                        subject=Some(ContentIdentity.key(_PAYLOAD_FMT, body).project("wire")),
                        data_schema=Some(_schema(event_type)),
                        extensions=Extensions(
                            # `TRACE_SLOTS` folds the W3C subset off the roster's own slot names, never three keys.
                            **{slot.value: Some(carrier[slot.value]) for slot in TRACE_SLOTS if slot.value in carrier},
                            authcontext=_held(fact.issuing_party),
                            correlation=_held(fact.scope),
                            dataclassification=Some(
                                _CLASSIFIED.try_find(fact.confidentiality.casefold()).default_value(Classification.INTERNAL)
                            ),
                            # `partitionkey` keeps one transmittal's whole revision stream ordered inside one
                            # partition, and the revision ordinal is the position a consumer reads a gap against.
                            partitionkey=Some(fact.transmittal_id),
                            sequence=Some(fact.revision_ordinal),
                            sequencetype=Some(Sequencing.INTEGER),
                            severity=_graded(Block.of_seq([fact.validation_state, fact.record_state])),
                        ),
                    )
                )
            )
            .map(lambda envelope: self.signer.map(envelope.signed).default_value(envelope))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _held(value: str, /) -> Option[str]:
    # Empty scalars contribute NO attribute: an empty-string value names an extension a filter matches and nobody
    # fills, which reads on a subscription exactly like a producer that meant to send one.
    return Some(value) if value else Nothing


def _schema(event_type: EventType, /) -> str:
    # registry SUBJECT and version in one reference, derived from the type whose major moves with it — a literal
    # here forks the two the moment a breaking payload change increments one of them.
    return f"{_REGISTRY}/subjects/{event_type.domain}.{event_type.subject}.{event_type.fact}/versions/{event_type.version}"


def _graded(states: Block[str], /) -> Option[Severity]:
    # descending ladder, first match wins: the weakest evidence state decides the whole fact's grade, and a state
    # no row names contributes nothing rather than asserting ROUTINE over evidence nobody graded.
    matched = _GRADE_LADDER.filter(lambda row: not states.filter(lambda state: state in row[0]).is_empty())
    return Nothing if matched.is_empty() else Some(matched.head()[1])


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ("TransmittalNotice",)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
