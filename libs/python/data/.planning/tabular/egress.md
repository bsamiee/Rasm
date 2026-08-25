# [PY_DATA_EGRESS]

`ObjectEgress` is the data-tier object-store egress owner: one receipt-and-governance surface over the runtime `runtime/transport/roots#STORE` `ObjectStoreLane` for the Arrow/Parquet/GeoParquet/zarr bundles `columnar`, `geospatial`, and `tensor` emit, keyed by runtime `ContentIdentity` over the operation's real bytes. It composes the whole `StoreOp` axis the lane dispatches — put/get/get_range/get_ranges/list/head/delete/copy/rename and the handle-bearing reader/writer/sign cases — and adds exactly what the transport tier has no business holding: the `EgressUnit` quantity vocabulary, the four mutation veto points, the by-reference put short-circuit, and the content-keyed `EgressReceipt`. Transport lives once at the runtime lane — the store handles, the reach matrix, the retry disposition, the client span — so no second provider composer, route table, or backend roster forms at this tier.

Content keys derive from operation bytes through one `ContentIdentity.of`, scheme-scoped through `ResourceRef.scheme` and never the path, so an unchanged content-key is a put no-op confirmed against the `e_tag` on the caller's prior egress receipt. That whole pre-flight semantics — veto then short-circuit — rides the lane's `gate` as ONE policy value this owner builds per call, so the ordering the boundary states (reach, then veto, then reuse, then provider) is structural rather than re-spelled at two entrypoints. Every `obstore.exceptions` leaf lands on the lane's `boundary` catch-all (none is a `CLASSIFY` row), so recovery keys on `fault.recoverable({"boundary"})` and a conditional write-once or copy-once collision surfaces as a terminal `boundary` fault read off the lifted message.

## [01]-[INDEX]

- [02]-[EGRESS]: the object-store egress owner composing the runtime lane — the gate policy, the quantity vocabulary, and the content-keyed `EgressReceipt`.

## [02]-[EGRESS]

- Owner: `ObjectEgress` — the one object-store egress façade, holding one `ObjectStoreLane` and one composition scope and nothing else. It mints no store handle, no route table, no reach matrix, and no span: those are the runtime lane's, so a new cloud backend is one `StoreBackend` row at the runtime owner and zero edits here. Never a `put_object`/`get_object`/`list_objects` method family, never an async method twin, never a parallel `S3Egress`/`AsyncObjectEgress`.
- Cases: the operation axis and its provider decisions are the lane's `StoreOp`; what this tier decides per op is the quantity it reports — `list`'s `read` counts the Arrow listing under `EgressUnit.OBJECTS`, `head` reports `OBJECT_SIZE`, `sign` reports `PATHS`, `delete`/`copy`/`rename`/`reader`/`writer` report `NONE`, and only payload-bearing rows report `BYTES`. One `_UNIT` row per op tag carries that judgment, so a new operation lands its unit beside the lane's own row and no receipt reads a quantity whose meaning it cannot name.
- Entry: `ObjectEgress.of` builds the lane once over the three transport carries, the credential riding `ref.credentials` rather than a fourth parameter this owner would only forward; `run` and `run_async` are one expression each — hand the op and the per-call gate to the lane, then fold the returned `StoreOutcome` into the receipt. `prior` is the caller's own by-reference evidence supplied per call, never an owner-held ledger, because a stateless owner cannot know which of many compositions last wrote this path; it rides the call rather than the op so the transport tier carries no data-tier receipt type.
- Auto: `_gate` is the whole pre-flight policy as one value the lane's prologue fires after reach — the registered mutation point vetoes first and a subscriber rejection returns on that rail, then an admitted put alone consults the short-circuit, so a settled skip never claims a write a governance tap refused. `_reuse` settles a put to a by-reference no-op only against an IDENTICAL write contract: the prior receipt names the `put` operation and THIS destination, its `contract` renders the same attributes and tags this put carries, its `RemoteIdentity` is the `acknowledged` case so that upload really landed, and the payload's `ContentKey` — derived from the payload `bytes`, never the path — equals its key; a conditional `mode` refuses the skip outright, because `create` and every `UpdateVersion` carry a remote precondition only the provider answers. The settled outcome keeps the payload on its `source` slot, so the receipt it folds mints exactly the key the upload would have, and a digest fault rides the same rail rather than falling through into an upload. Remote drift SINCE that egress is the caller's to detect by handing a fresher prior — a `Head` round-trip per put spends exactly the traffic the short-circuit removes.
- Law: object mutations land durable evidence on the `python:runtime/observability/journal#LEDGER` plane and reads land none — one `AuditFact` per `_AUDIT` row carrying the disposal class its verb earns, plus a `STORAGE` `MeterFact` over the bytes the operation actually stored. The seat is the AWAITABLE leg alone, since recording suspends and `contribute` is a synchronous projection; the facts mint off the settled receipt so a fact never names a write the store refused, and a by-reference settle records its audit line with no charge, because pricing a skip bills a residence for an upload nobody made. Evidence truth and series truth stay two folds: the receipt fan keeps the metric and the journal keeps the fact, and neither re-mints the other's number.
- Law: transport-band compression arrives AS the payload — a scan-scale frame crossing to object storage rides the `tabular/interop#CARRIER` `wire_bytes` fold and its `WireCodec`, so the put moves compressed bytes while the frame's canonical identity stays the caller's uncompressed `arrow_bytes` key on its bundle; this owner keys the operation bytes it actually moved, and no codec knob lands here because the codec is the interop transport parameter, never an egress column.
- Receipt: `EgressReceipt.contribute` yields one emitted-phase `Receipt.of("object-egress", ("emitted", path, facts))` — the two-argument `(owner, evidence)` form over the `(phase, subject, facts)` tuple, never a four-positional call — satisfying the `ReceiptContributor` `Iterable[Receipt]` Protocol. `quantity` and `unit` preserve byte volume, object count, signed-path count, and metadata size without overloading; `byte_length` projects only the `BYTES` rows consumed by cost and range folds. Byte-bearing ops key by their operation-bytes `ContentKey`; a control-plane op (`list`/`head`/`delete`/`copy`/`rename`/`reader`/`writer`/`sign`) carries `content_key=None` because it moves no operation bytes, so the receipt never digests a path string or a server-opaque `e_tag` to manufacture a key, and its fact map OMITS `key` rather than writing a null into the rendered-value contract the `tabular/lakehouse#LAKEHOUSE` residence reads. `Head` reads its `e_tag`/`version`/`size` off the typed slots and `RemoteIdentity.of` admits them ONCE into a closed case, so the no-op put confirms against an ACKNOWLEDGED identity rather than the truthiness of a string that also spells "this op reported no metadata at all", and a settled no-op carries `reused=True` with zero bytes — the receipt is where a skipped upload is visible, because an operation moving no bytes emits no throughput point to carry it. Byte-bearing `contribute` projects `rasm.egress.byte_volume` onto the runtime `Metrics.record` arm under `domain="egress"` keyed by operation — the canonical object-store throughput instrument, data-owned beside the runtime's artifact byte-volume row.
- Packages: none directly — `obstore` is the runtime lane's sole provider and this tier names no provider member, no provider exception, and no provider config beyond forwarding the three typed carries into `ObjectStoreLane.of`. `beartype(conf=FAULT_CONF)` is the public admission contract on `of` the sibling `interop`/`store`/`ragged` factories share.
- Growth: a new store operation is one `StoreOp` case and one `_ROUTE` row at the runtime lane plus one `_UNIT` row here; a new remote-identity posture is one `RemoteIdentity` case owning its `facts` arm; a new cloud backend is one `StoreBackend` row at the runtime lane and nothing here; a second by-reference short-circuit is one `_reuse` arm over that op's own prior evidence, the `reused` and `contract` receipt slots already carrying it; a newly output-affecting write knob is one `_contract` term beside its `StoreOp` slot; a new governance concern is one subscriber the app root attaches; a new mutation point is one `_VETO` row; a newly audited operation is one `_AUDIT` row naming its retention class, the verb deriving off the tag.
- Boundary: this is the data-tier bundle-I/O owner — the receipt, quantity, veto, and reuse semantics over the full write/mutation direction (`put`/`copy`/`rename`/`delete`/`writer`) and the bundle-byte reads (`get`/`get_range`/`get_ranges`/`head`/`reader`/`sign`) its `columnar`/`geospatial`/`tensor`/`gridded/virtual`/`spatial/catalog` consumers need. `runtime/transport/roots#STORE` owns every transport concern beneath it and `runtime/transport/roots#RESOURCE` the orthogonal generic-artifact acquisition lane; this tier consumes `ResourceRef` and the credential carries alone and re-derives neither. Composes — never re-mints — the `ContentIdentity` keyer, the `Hooks` registry, and the runtime metric spine. Rejected: a second `obstore` composer, route table, backend literal, refusal matrix, retry disposition, or client span beside the runtime lane; a capability bound answered by a provider exception where the lane's matrix states it as data; a mutation row that fires no veto point while this boundary names the mutation; a path-string `ContentIdentity.of` key against the identity owner's no-path law; an owner-held prior-egress ledger, or a `Head` probe opened per put to refresh one, where a stateless owner cannot know which composition last wrote the path and the probe costs the round-trip the short-circuit exists to remove.

```python
from collections.abc import Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

from beartype import beartype
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.data.tabular.interop import DataHook
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


@tagged_union(frozen=True)
class RemoteIdentity:
    tag: Literal["acknowledged", "unacknowledged", "unreported"] = tag()
    acknowledged: tuple[str, Option[str]] = case()
    unacknowledged: None = case()
    unreported: None = case()

    @staticmethod
    def of(meta: Meta) -> "RemoteIdentity":
        match meta:
            case None:
                return RemoteIdentity(unreported=None)
            case slot if not slot.get("e_tag"):
                return RemoteIdentity(unacknowledged=None)
            case slot:
                version = slot.get("version")
                return RemoteIdentity(acknowledged=(str(slot["e_tag"]), Nothing if version is None else Some(str(version))))

    @property
    def e_tag(self) -> Option[str]:
        return Some(self.acknowledged[0]) if self.tag == "acknowledged" else Nothing

    @property
    def version(self) -> Option[str]:
        return self.acknowledged[1] if self.tag == "acknowledged" else Nothing

    def facts(self) -> dict[str, object]:
        match self:
            case RemoteIdentity(tag="acknowledged", acknowledged=(etag, version)):
                return {"identity": self.tag, "etag": etag} | version.map(lambda held: {"version": held}).default_value({})
            case RemoteIdentity(tag="unacknowledged") | RemoteIdentity(tag="unreported"):
                return {"identity": self.tag}
            case _ as unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] ------------------------------------------------------------------------

OWNER: Final[str] = "object-egress"
DOMAIN: Final[str] = "egress"

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

_AUDIT: Final[Map[str, Retain]] = Map.of_seq([
    ("put", Retain.OPERATIONAL),
    ("writer", Retain.OPERATIONAL),
    ("copy", Retain.OPERATIONAL),
    ("delete", Retain.REGULATORY),
    ("rename", Retain.REGULATORY),
])


# --- [MODELS] ---------------------------------------------------------------------------


class EgressMutation(Struct, frozen=True):
    operation: str
    path: str
    byte_length: int


PUT_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id=DataHook.EGRESS_PUT, payload=EgressMutation, modality=Modality(veto=None))
DELETE_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id=DataHook.EGRESS_DELETE, payload=EgressMutation, modality=Modality(veto=None))
COPY_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id=DataHook.EGRESS_COPY, payload=EgressMutation, modality=Modality(veto=None))
RENAME_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id=DataHook.EGRESS_RENAME, payload=EgressMutation, modality=Modality(veto=None))

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
    identity: RemoteIdentity
    content_key: ContentKey | None
    payload: Any = None
    reused: bool = False
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
        return cls(
            operation=operation,
            path=path,
            byte_length=quantity if unit is EgressUnit.BYTES else 0,
            quantity=quantity,
            unit=unit,
            identity=RemoteIdentity.of(meta),
            content_key=content_key,
            payload=payload,
            reused=reused,
            contract=contract,
        )

    def contribute(self) -> Iterable[Receipt]:
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
                    "reused": self.reused,
                }
                | self.identity.facts()
                | ({} if self.content_key is None else {"key": self.content_key.hex}),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _contract(op: StoreOp) -> str:
    attributes, tags = (op.put[2], op.put[3]) if op.tag == "put" else (op.writer[1], op.writer[2]) if op.tag == "writer" else ({}, {})
    return repr((sorted((str(key), str(value)) for key, value in dict(attributes).items()), sorted(tags.items())))


def _vetoed(op: StoreOp, target: str, scope: ScopeKey) -> "RuntimeRail[StoreOp]":
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
    if (
        prior is None
        or prior.content_key is None
        or prior.identity.tag != "acknowledged"
        or prior.operation != "put"
        or prior.path != target
        or prior.contract != _contract(op)
        or op.tag != "put"
        or op.put[1] != "overwrite"
    ):
        return Ok(StoreAdmission(dispatch=op))
    acknowledged, version = prior.identity.acknowledged
    settled = StoreOutcome(
        operation=op.tag,
        path=target,
        quantity=0,
        meta={"e_tag": acknowledged, "version": version.to_optional()},
        source=op.put[0],
        settled=True,
    )
    return ContentIdentity.of(f"{DOMAIN}.{op.tag}.{scheme}", op.put[0]).map(
        lambda key: StoreAdmission(settled=settled) if key == prior.content_key else StoreAdmission(dispatch=op)
    )


def _evidence(receipt: EgressReceipt) -> Block[Fact]:
    def minted(retention: Retain) -> Block[Fact]:
        audited = AuditFact(
            action=f"{DOMAIN}.{receipt.operation}",
            actor=Party(kind=Actor.SERVICE, key=OWNER),
            target=Party(kind="object", key=receipt.path),
            retention=retention,
            change=tuple(
                receipt.identity.e_tag.map(
                    lambda held: Cleared(path="/e_tag", prior=held) if receipt.operation == "delete" else Assigned(path="/e_tag", next=held)
                ).to_list()
            ),
        )
        metered = MeterFact(resource=Resource.STORAGE, quantity=receipt.byte_length, surface=receipt.path)
        return Block.of_seq((audited, metered) if receipt.byte_length else (audited,))

    return _AUDIT.try_find(receipt.operation).map(minted).default_value(Block.empty())


def _gate(scope: ScopeKey, scheme: str, prior: "EgressReceipt | None") -> StoreGate:
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
        return cls(
            lane=ObjectStoreLane.of(ref, config=config, client_options=client_options, retry_config=retry_config),
            scope=scope,
        )

    def run(self, op: StoreOp, path: str = "", prior: "EgressReceipt | None" = None) -> "RuntimeRail[EgressReceipt]":
        return self.lane.run(op, path, gate=self._gated(prior)).bind(lambda outcome: self._receipt(op, outcome))

    async def run_async(self, op: StoreOp, path: str = "", prior: "EgressReceipt | None" = None) -> "RuntimeRail[EgressReceipt]":
        settled = (await self.lane.run_async(op, path, gate=self._gated(prior))).bind(lambda outcome: self._receipt(op, outcome))
        match settled:
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(_evidence(receipt), scope=self.scope)).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    def _gated(self, prior: "EgressReceipt | None") -> StoreGate:
        return _gate(self.scope, self.lane.ref.scheme, prior)

    def _receipt(self, op: StoreOp, outcome: StoreOutcome) -> "RuntimeRail[EgressReceipt]":
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
