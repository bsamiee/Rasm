# [PY_DATA_EGRESS]

`ObjectEgress` is the data-tier object-store egress owner: one receipt-and-governance surface over the runtime `runtime/transport/roots#STORE` `ObjectStoreLane` for the Arrow/Parquet/GeoParquet/zarr bundles `columnar`, `geospatial`, and `tensor` emit, keyed by runtime `ContentIdentity` over the operation's real bytes. It composes the whole `StoreOp` axis the lane dispatches — put/get/get_range/get_ranges/list/head/delete/copy/rename and the handle-bearing reader/writer/sign cases — and adds exactly what the transport tier has no business holding: the `EgressUnit` quantity vocabulary, the four mutation veto points, the by-reference put short-circuit, and the content-keyed `EgressReceipt`. Transport lives once at the runtime lane — the store handles, the reach matrix, the retry disposition, the client span — so no second provider composer, route table, or backend roster forms at this tier.

Content keys derive from operation bytes through one `ContentIdentity.of`, scheme-scoped through `ResourceRef.scheme` and never the path, so an unchanged content-key is a put no-op confirmed against the `e_tag` on the caller's prior egress receipt. That whole pre-flight semantics — veto then short-circuit — rides the lane's `gate` as ONE policy value this owner builds per call, so the ordering the boundary states (reach, then veto, then reuse, then provider) is structural rather than re-spelled at two entrypoints. Every `obstore.exceptions` leaf lands on the lane's `boundary` catch-all (none is a `CLASSIFY` row), so recovery keys on `fault.recoverable({"boundary"})` and a conditional write-once or copy-once collision surfaces as a terminal `boundary` fault read off the lifted message.

## [01]-[INDEX]

- [02]-[EGRESS]: the object-store egress owner composing the runtime lane — the gate policy, the quantity vocabulary, and the content-keyed `EgressReceipt`.

## [02]-[EGRESS]

- Owner: `ObjectEgress` — the one object-store egress façade, holding one `ObjectStoreLane` and one composition scope and nothing else. It mints no store handle, no route table, no reach matrix, and no span: those are the runtime lane's, so a new cloud backend is one `StoreBackend` row at the runtime owner and zero edits here. Never a `put_object`/`get_object`/`list_objects` method family, never an async method twin, never a parallel `S3Egress`/`AsyncObjectEgress`.
- Cases: the operation axis and its provider decisions are the lane's `StoreOp`; what this tier decides per op is the quantity it reports — `list`'s `read` counts the Arrow listing under `EgressUnit.OBJECTS`, `head` reports `OBJECT_SIZE`, `sign` reports `PATHS`, `delete`/`copy`/`rename`/`reader`/`writer` report `NONE`, and only payload-bearing rows report `BYTES`. One `_UNIT` row per op tag carries that judgment, so a new operation lands its unit beside the lane's own row and no receipt reads a quantity whose meaning it cannot name.
- Entry: `ObjectEgress.of` builds the lane once over the three transport carries, the credential riding `ref.credentials` rather than a fourth parameter this owner would only forward; `run` and `run_async` are one expression each — hand the op and the per-call gate to the lane, then fold the returned `StoreOutcome` into the receipt. `prior` is the caller's own by-reference evidence supplied per call, never an owner-held ledger, because a stateless owner cannot know which of many compositions last wrote this path; it rides the call rather than the op so the transport tier carries no data-tier receipt type.
- Auto: `_gate` is the whole pre-flight policy as one value the lane's prologue fires after reach — the registered mutation point vetoes first and a subscriber rejection returns on that rail, then an admitted put alone consults the short-circuit, so a settled skip never claims a write a governance tap refused. `_reuse` settles a put to a by-reference no-op only against an IDENTICAL write contract: the prior receipt names the `put` operation and THIS destination, its `contract` renders the same attributes and tags this put carries, its `e_tag` is non-empty so that upload was acknowledged, and the payload's `ContentKey` — derived from the payload `bytes`, never the path — equals its key; a conditional `mode` refuses the skip outright, because `create` and every `UpdateVersion` carry a remote precondition only the provider answers. The settled outcome keeps the payload on its `source` slot, so the receipt it folds mints exactly the key the upload would have, and a digest fault rides the same rail rather than falling through into an upload. Remote drift SINCE that egress is the caller's to detect by handing a fresher prior — a `Head` round-trip per put spends exactly the traffic the short-circuit removes.
- Law: object mutations land durable evidence on the `python:runtime/observability/journal#LEDGER` plane and reads land none — one `AuditFact` per `_AUDIT` row carrying the disposal class its verb earns, plus a `STORAGE` `MeterFact` over the bytes the operation actually stored. The seat is the AWAITABLE leg alone, since recording suspends and `contribute` is a synchronous projection; the facts mint off the settled receipt so a fact never names a write the store refused, and a by-reference settle records its audit line with no charge, because pricing a skip bills a residence for an upload nobody made. Evidence truth and series truth stay two folds: the receipt fan keeps the metric and the journal keeps the fact, and neither re-mints the other's number.
- Law: transport-band compression arrives AS the payload — a scan-scale frame crossing to object storage rides the `tabular/interop#CARRIER` `wire_bytes` fold and its `WireCodec`, so the put moves compressed bytes while the frame's canonical identity stays the caller's uncompressed `arrow_bytes` key on its bundle; this owner keys the operation bytes it actually moved, and no codec knob lands here because the codec is the interop transport parameter, never an egress column.
- Receipt: `EgressReceipt.contribute` yields one emitted-phase `Receipt.of("object-egress", ("emitted", path, facts))` — the two-argument `(owner, evidence)` form over the `(phase, subject, facts)` tuple, never a four-positional call — satisfying the `ReceiptContributor` `Iterable[Receipt]` Protocol. `quantity` and `unit` preserve byte volume, object count, signed-path count, and metadata size without overloading; `byte_length` projects only the `BYTES` rows consumed by cost and range folds. Byte-bearing ops key by their operation-bytes `ContentKey`; a control-plane op (`list`/`head`/`delete`/`copy`/`rename`/`reader`/`writer`/`sign`) carries `content_key=None` because it moves no operation bytes, so the receipt never digests a path string or a server-opaque `e_tag` to manufacture a key, and its fact map OMITS `key` rather than writing a null into the rendered-value contract the `tabular/lakehouse#LAKEHOUSE` residence reads. `Head` reads its `e_tag`/`version`/`size` off the typed slots, so the no-op put confirms against that typed `e_tag` rather than a redundant key over `str(e_tag)`, and a settled no-op carries `reused=True` with zero bytes — the receipt is where a skipped upload is visible, because an operation moving no bytes emits no throughput point to carry it. Byte-bearing `contribute` projects `rasm.egress.byte_volume` onto the runtime `Metrics.record` arm under `domain="egress"` keyed by operation — the canonical object-store throughput instrument, data-owned beside the runtime's artifact byte-volume row.
- Packages: none directly — `obstore` is the runtime lane's sole provider and this tier names no provider member, no provider exception, and no provider config beyond forwarding the three typed carries into `ObjectStoreLane.of`. `beartype(conf=FAULT_CONF)` is the public admission contract on `of` the sibling `interop`/`store`/`ragged` factories share.
- Growth: a new store operation is one `StoreOp` case and one `_ROUTE` row at the runtime lane plus one `_UNIT` row here; a new cloud backend is one `StoreBackend` row at the runtime lane and nothing here; a second by-reference short-circuit is one `_reuse` arm over that op's own prior evidence, the `reused` and `contract` receipt slots already carrying it; a newly output-affecting write knob is one `_contract` term beside its `StoreOp` slot; a new governance concern is one subscriber the app root attaches; a new mutation point is one `_VETO` row; a newly audited operation is one `_AUDIT` row naming its retention class, the verb deriving off the tag.
- Boundary: this is the data-tier bundle-I/O owner — the receipt, quantity, veto, and reuse semantics over the full write/mutation direction (`put`/`copy`/`rename`/`delete`/`writer`) and the bundle-byte reads (`get`/`get_range`/`get_ranges`/`head`/`reader`/`sign`) its `columnar`/`geospatial`/`tensor`/`gridded/virtual`/`spatial/catalog` consumers need. `runtime/transport/roots#STORE` owns every transport concern beneath it and `runtime/transport/roots#RESOURCE` the orthogonal generic-artifact acquisition lane; this tier consumes `ResourceRef` and the credential carries alone and re-derives neither. Composes — never re-mints — the `ContentIdentity` keyer, the `Hooks` registry, and the runtime metric spine. Rejected: a second `obstore` composer, route table, backend literal, refusal matrix, retry disposition, or client span beside the runtime lane; a capability bound answered by a provider exception where the lane's matrix states it as data; a mutation row that fires no veto point while this boundary names the mutation; a path-string `ContentIdentity.of` key against the identity owner's no-path law; an owner-held prior-egress ledger, or a `Head` probe opened per put to refresh one, where a stateless owner cannot know which composition last wrote the path and the probe costs the round-trip the short-circuit exists to remove.

```python signature
from collections.abc import Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final

from beartype import beartype
from expression import Error, Ok, Result
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.journal import Actor, Assigned, AuditFact, Cleared, Fact, Journal, MeterFact, Party, Resource, Retain
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.roots import (
    Config,
    Meta,
    ObjectStoreLane,
    ResourceRef,
    StoreAdmission,
    StoreGate,
    StoreOp,
    StoreOutcome,
    store_path,
)

if TYPE_CHECKING:
    from obstore.store import ClientConfig, RetryConfig

# --- [TYPES] ----------------------------------------------------------------------------


class EgressUnit(StrEnum):
    BYTES = "bytes"
    OBJECTS = "objects"
    PATHS = "paths"
    OBJECT_SIZE = "object_size"
    NONE = "none"


# --- [CONSTANTS] ------------------------------------------------------------------------

# this owner's two names spelled once — the receipt owner label the audit actor also identifies as, and the metric
# segment its key subjects, its facts, and its audit verbs all derive from — so a rename cannot strand a series, a
# lifted evidence column, and a verb under three spellings of one owner.
OWNER: Final[str] = "object-egress"
DOMAIN: Final[str] = "egress"

# the quantity vocabulary is THIS tier's judgment over the lane's op axis: the transport reports a number, and the
# unit row is what makes that number legible to a cost fold. A settled by-reference no-op overrides to NONE at the
# receipt, because it moved no bytes whatever its op would otherwise report.
_UNIT: Final[Map[str, EgressUnit]] = Map.of_seq([
    ("put", EgressUnit.BYTES),
    ("get", EgressUnit.BYTES),
    ("get_range", EgressUnit.BYTES),
    ("get_ranges", EgressUnit.BYTES),
    ("list", EgressUnit.OBJECTS),
    ("head", EgressUnit.OBJECT_SIZE),
    ("delete", EgressUnit.NONE),
    ("copy", EgressUnit.NONE),
    ("rename", EgressUnit.NONE),
    ("reader", EgressUnit.NONE),
    ("writer", EgressUnit.NONE),
    ("sign", EgressUnit.PATHS),
])

# retention class per MUTATION tag, and the roster of ops recording durable evidence at all: a put or copy is the
# routine operational trail, a delete or rename the disposal evidence a regulator reads back years later, and
# `writer` shares `put`'s class exactly as it shares `put`'s veto point. Every read tag is absent by design — an
# audit plane recording each `get` prices the read path for rows no incident reconstruction ever reads.
_AUDIT: Final[Map[str, Retain]] = Map.of_seq([
    ("put", Retain.OPERATIONAL),
    ("writer", Retain.OPERATIONAL),
    ("copy", Retain.OPERATIONAL),
    ("delete", Retain.REGULATORY),
    ("rename", Retain.REGULATORY),
])


# --- [MODELS] ---------------------------------------------------------------------------


class EgressMutation(Struct, frozen=True):
    # pre-flight mutation fact the veto edges fire — a receipt exists only after the provider call.
    operation: str
    path: str
    byte_length: int


PUT_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id="rasm.data.egress.put", payload=EgressMutation, modality=Modality.VETO)
DELETE_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id="rasm.data.egress.delete", payload=EgressMutation, modality=Modality.VETO)
COPY_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id="rasm.data.egress.copy", payload=EgressMutation, modality=Modality.VETO)
RENAME_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id="rasm.data.egress.rename", payload=EgressMutation, modality=Modality.VETO)

# `writer` opens a multipart UPLOAD — the fifth mutation this owner's boundary names — so it fires the same
# pre-flight point `put` does; a governance tap that vetoes a put and silently admits a streamed write of the same
# bytes gates nothing. Every other tag passes untouched through the absent-row default.
_VETO: Final[Map[str, HookPoint[EgressMutation]]] = Map.of_seq([
    ("put", PUT_POINT),
    ("writer", PUT_POINT),
    ("delete", DELETE_POINT),
    ("copy", COPY_POINT),
    ("rename", RENAME_POINT),
])


class EgressReceipt(Struct, frozen=True):
    operation: str
    path: str
    byte_length: int
    quantity: int
    unit: EgressUnit
    e_tag: str
    version: str
    # `None` for a control-plane op: no content key minted — `head`'s `e_tag`/`version`/`size` ride the typed slots.
    content_key: ContentKey | None
    payload: Any = None
    # by-reference evidence: the operation settled against a prior receipt and moved no bytes. It rides the
    # receipt rather than an instrument because a skipped upload emits no throughput point to carry it.
    reused: bool = False
    # the write's OWN output-affecting contract beyond its bytes, so a later by-reference put proves the object it
    # stands in for carries the metadata this caller asks for. Empty on every op whose row writes no metadata.
    contract: str = ""

    @classmethod
    def of(
        cls,
        operation: str,
        path: str,
        quantity: int,
        unit: EgressUnit,
        meta: Meta,
        content_key: ContentKey | None,
        payload: Any,
        reused: bool = False,
        contract: str = "",
    ) -> EgressReceipt:
        slot = meta or {}
        return cls(
            operation=operation,
            path=path,
            byte_length=quantity if unit is EgressUnit.BYTES else 0,
            quantity=quantity,
            unit=unit,
            e_tag=str(slot.get("e_tag") or ""),
            version=str(slot.get("version") or ""),
            content_key=content_key,
            payload=payload,
            reused=reused,
            contract=contract,
        )

    def contribute(self) -> Iterable[Receipt]:
        # `byte_length` rides as a native `int`: the receipts `Encoder(enc_hook=repr)` serializes
        # scalars without a `str()` coerce. Byte-bearing ops project throughput onto the metric spine
        # under domain="egress" keyed by operation; control-plane ops (byte_length 0) record nothing.
        # `domain`/`kind`/`key` are the lifted evidence contract the `tabular/lakehouse#LAKEHOUSE` residence
        # reads — the SAME pair handed `Metrics.record` — so a stored row rejoins the series its live twin emitted.
        # Control-plane ops contribute NO `key` entry: every fact this map carries renders, so a null lands at the
        # residence as the literal `"None"` unless every reader coerces it back. Absence spells "this op minted no
        # content key" honestly, and the emitter owes that spelling rather than the readers downstream of it.
        if self.byte_length:
            Metrics.record({"rasm.egress.byte_volume": float(self.byte_length)}, domain=DOMAIN, kind=self.operation)
        yield Receipt.of(
            OWNER,
            (
                "emitted",
                self.path,
                {
                    "domain": DOMAIN,
                    "kind": self.operation,
                    "bytes": self.byte_length,
                    "quantity": self.quantity,
                    "unit": self.unit,
                    "etag": self.e_tag,
                    "version": self.version,
                    "reused": self.reused,
                }
                | ({} if self.content_key is None else {"key": self.content_key.hex}),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _contract(op: StoreOp) -> str:
    # the OUTPUT-affecting half of a write beyond its bytes: the metadata the stored object carries afterwards.
    # Rendered from sorted pairs, so two writes declaring one contract render one token whatever order a caller
    # built its maps in, and a later put reads it back off the prior receipt as the by-reference proof. `chunk_size`
    # stays OUT — it selects single-part against multipart transfer and changes no stored byte or header, so folding
    # it in would refuse a sound skip over a re-tuned threshold. Every other tag writes no metadata and renders empty.
    attributes, tags = (op.put[2], op.put[3]) if op.tag == "put" else (op.writer[1], op.writer[2]) if op.tag == "writer" else ({}, {})
    return repr((sorted((str(key), str(value)) for key, value in dict(attributes).items()), sorted(tags.items())))


def _vetoed(op: StoreOp, target: str, scope: ScopeKey) -> "RuntimeRail[StoreOp]":
    # the mutation rows fire their registered point before any provider mutation, reading the destination off the
    # lane's own `store_path` fold so the path a tap sees and the path the receipt names cannot diverge; the
    # streamed `writer` payload has no length yet, so its fact reports zero.
    return (
        _VETO.try_find(op.tag)
        .map(
            lambda point: Hooks.fire(
                point.id,
                EgressMutation(operation=op.tag, path=store_path(op, target), byte_length=len(op.put[0]) if op.tag == "put" else 0),
                scope=scope,
            ).map(lambda _fact: op)
        )
        .default_value(Ok(op))
    )


def _reuse(op: StoreOp, target: str, scheme: str, prior: "EgressReceipt | None") -> "RuntimeRail[StoreAdmission]":
    # By-reference put no-op, and every half is load-bearing because a settled skip CLAIMS a write nobody made:
    # the prior receipt names this operation and THIS destination, its `contract` renders the same object metadata
    # this put carries, its non-empty `e_tag` proves that upload was acknowledged rather than merely attempted, and
    # the fresh payload key equals its key so the BYTES are unchanged. A prior read off another path, another
    # metadata contract, or another operation settles a write that never happened at all. Mode is the fifth half and
    # refuses rather than compares: `create` and every `UpdateVersion` carry a REMOTE precondition only the provider
    # can answer, so a conditional put dispatches and lets it. The settled outcome keeps the payload on its `source`
    # slot and its prior `(e_tag, version)` on `meta`, so the receipt it folds carries the same content key AND the
    # same remote identity a fresh `PutResult` would; a digest fault rides this one rail rather than falling through
    # into an upload.
    if (
        prior is None
        or prior.content_key is None
        or not prior.e_tag
        or prior.operation != "put"
        or prior.path != target
        or prior.contract != _contract(op)
        or op.tag != "put"
        or op.put[1] != "overwrite"
    ):
        return Ok(StoreAdmission(dispatch=op))
    settled = StoreOutcome(
        operation=op.tag,
        path=target,
        quantity=0,
        meta={"e_tag": prior.e_tag or None, "version": prior.version or None},
        source=op.put[0],
        settled=True,
    )
    # the comparison subject is the RECEIPT's own subject verbatim, so the prior key and this probe address one
    # identity namespace — a subject spelled short here would never match a key the scheme-scoped receipt minted.
    return ContentIdentity.of(f"{DOMAIN}.{op.tag}.{scheme}", op.put[0]).map(
        lambda key: StoreAdmission(settled=settled) if key == prior.content_key else StoreAdmission(dispatch=op)
    )


def _evidence(receipt: EgressReceipt) -> Block[Fact]:
    # the durable half of a mutation, minted off the SETTLED receipt so the disposition and the remote identity are
    # both in hand — a fact built before the provider answered names a write the store may still refuse. The audit
    # verb derives from the op tag under the runtime `<domain>.<operation>` grammar, so a new mutation reaches the
    # journal with one `_AUDIT` row and no verb table. The meter rides the bytes this operation actually stored:
    # a by-reference settle reports zero and lands its audit line alone, since a skip that prices storage charges a
    # residence for an upload nobody made, and a control-plane tag never reaches this fold at all.
    def minted(retention: Retain) -> Block[Fact]:
        audited = AuditFact(
            action=f"{DOMAIN}.{receipt.operation}",
            actor=Party(kind=Actor.SERVICE, key=OWNER),
            target=Party(kind="object", key=receipt.path),
            retention=retention,
            change=(
                (Cleared(path="/e_tag", prior=receipt.e_tag),)
                if receipt.operation == "delete"
                else (Assigned(path="/e_tag", next=receipt.e_tag),)
            ),
        )
        metered = MeterFact(resource=Resource.STORAGE, quantity=receipt.byte_length, surface=receipt.path)
        return Block.of_seq((audited, metered) if receipt.byte_length else (audited,))

    return _AUDIT.try_find(receipt.operation).map(minted).default_value(Block.empty())


def _gate(scope: ScopeKey, scheme: str, prior: "EgressReceipt | None") -> StoreGate:
    # ONE pre-flight policy value the lane's prologue fires after reach: veto first — a subscriber rejection returns
    # on this rail and no provider call runs — then the admitted op consults the by-reference short-circuit, so a
    # settled skip never stands in for a mutation a governance tap refused. Ordering is structural here rather than
    # re-spelled at two entrypoints, because the lane reads exactly one gate.
    return lambda op, target: _vetoed(op, target, scope).bind(lambda admitted: _reuse(admitted, target, scheme, prior))


# --- [SERVICES] -------------------------------------------------------------------------


class ObjectEgress(Struct, frozen=True):
    lane: ObjectStoreLane
    scope: ScopeKey = DEFAULT_SCOPE

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(
        cls,
        ref: ResourceRef,
        config: Config | None = None,
        client_options: "ClientConfig | None" = None,
        retry_config: "RetryConfig | None" = None,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> ObjectEgress:
        # the lane mints both store handles once here and owns every transport carry; this owner forwards the three
        # typed values and holds nothing a provider would recognize. The credential is NOT among them — it rides
        # `ref.credentials`, so a caller egressing to a private residence hands the coordinate that already carries
        # its provider rather than a field this owner would only forward; a `credential_provider=` beside the ref
        # was a second credential resolution the lane's one store memo key cannot serve.
        return cls(
            lane=ObjectStoreLane.of(ref, config=config, client_options=client_options, retry_config=retry_config),
            scope=scope,
        )

    def run(self, op: StoreOp, path: str = "", prior: "EgressReceipt | None" = None) -> "RuntimeRail[EgressReceipt]":
        return self.lane.run(op, path, gate=self._gated(prior)).bind(lambda outcome: self._receipt(op, outcome))

    async def run_async(self, op: StoreOp, path: str = "", prior: "EgressReceipt | None" = None) -> "RuntimeRail[EgressReceipt]":
        # the AWAITABLE leg is where durable evidence lands, under the runtime producer-seam law: recording suspends
        # on a full intake, so the synchronous `run` carries no producer leg and `contribute` stays the pure receipt
        # projection its metric half already is. The record rail BINDS into this verdict — an armed evidence plane
        # refusing a mutation fact is a governance failure this caller owns — while a composition that installed no
        # plane folds to the lawful no-op, so this leg costs one empty block and never learns which it ran under.
        settled = (await self.lane.run_async(op, path, gate=self._gated(prior))).bind(lambda outcome: self._receipt(op, outcome))
        match settled:
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(_evidence(receipt), scope=self.scope)).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    def _gated(self, prior: "EgressReceipt | None") -> StoreGate:
        return _gate(self.scope, self.lane.ref.scheme, prior)

    def _receipt(self, op: StoreOp, outcome: StoreOutcome) -> "RuntimeRail[EgressReceipt]":
        # a settled outcome reports NONE whatever its op's row would say, because a by-reference skip moved no bytes;
        # `source is None` (control-plane) mints NO key, else the one `ContentIdentity.of` classifies the operation
        # bytes and rails the key, scheme-scoped through the lane's own ref.
        unit = EgressUnit.NONE if outcome.settled else _UNIT[outcome.operation]

        def receipt(key: ContentKey | None) -> EgressReceipt:
            return EgressReceipt.of(
                outcome.operation, outcome.path, outcome.quantity, unit, outcome.meta, key, outcome.payload,
                reused=outcome.settled, contract=_contract(op),
            )

        if outcome.source is None:
            return Ok(receipt(None))
        return ContentIdentity.of(f"{DOMAIN}.{outcome.operation}.{self.lane.ref.scheme}", outcome.source).map(receipt)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
