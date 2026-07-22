# [PY_DATA_EGRESS]

`ObjectEgress` is the native object-store egress owner: one façade over `obstore` for the Arrow/Parquet/GeoParquet/zarr bundles `columnar`, `geospatial`, and `tensor` emit, keyed by runtime `ContentIdentity` over the operation's real bytes. It discriminates the `StoreOp` tagged-union axis — put/get/get_range/get_ranges/list/head/delete/copy/rename and the handle-bearing reader/writer/sign cases — routing every case through one `_ROUTE` data table so the synchronous `run` and awaitable `run_async` fold over one row set rather than a structural `match`. It owns the full write/mutation direction and the bundle-byte reads its consumers need; the runtime `transport/roots#RESOURCE` owner holds the orthogonal generic-transport lane over the same provider.

Content keys derive from operation bytes through one `ContentIdentity.of`, scheme-scoped through `ResourceRef.scheme` and never the path, so an unchanged content-key is a put no-op confirmed against the prior `Head` `e_tag`. Retry is a per-row mutation-class disposition on `_ROUTE`: reads and idempotent mutations delegate the retried-traced-railed acquisition to the `reliability/resilience#RESILIENCE` `guarded`/`guarded_sync` fused envelopes over the `RetryClass.OBJECT_STORE` row (the `reliability/faults#FAULT` `BoundaryFault` those lift, the child `resilience.guarded` span beneath one `_TRACER` egress parent), while the non-idempotent `copy`/`rename` cross once through the bare `boundary`/`async_boundary` fence — both legs self-flattened `.bind(lambda rail: rail)` so the identity-failure rail shares the obstore-failure carrier. Its store backend is the one `ObjectStore` handle `from_url` constructs off the `ref.root` scheme — the canonical store-handle Union the sibling `transport/roots#RESOURCE` cache also carries — frozen at `of` over the credential-bearing `TransportResource`. Because the `obstore` sync and async members carry identical keyword signatures, `_async` is the same row read under a second entrypoint (catalogue `_async`-identity law), never a parallel `AsyncStoreOp` family.

## [01]-[INDEX]

- [01]-[EGRESS]: the object-store egress owner routing one `StoreOp` axis through one `_ROUTE` fold for the synchronous `run` and awaitable `run_async`.

## [02]-[EGRESS]

- Owner: `ObjectEgress` — the one object-store egress façade over `obstore`, discriminating `StoreOp` through one `_ROUTE` data table over one `_Row` shape, never a `put_object`/`get_object`/`list_objects` method family, never an async method twin, never a second `_HandleRow` beside the byte-receipt rows. Its store backend is the one `ObjectStore` handle `from_url` constructs off the `ref.root` scheme, the canonical store-handle Union the sibling `transport/roots#RESOURCE` `_object_store` cache also carries, so a new cloud backend is one URL scheme, never a parallel store class.
- Cases: the fence's factories and `_ROUTE` rows carry each obstore member; the decisions the fence cannot show —
  - `Put` `mode` ∈ `create|overwrite` or an `UpdateVersion` `PutMode` for the conditional write-once; multipart auto-selected when the payload exceeds `chunk_size`, the threshold an explicit request value defaulting to the 5-MiB `CHUNK` axis, never the provider default.
  - `GetRange` addresses one window as start+end or start+length — `end` and `length` are mutually exclusive descriptors on the one case.
  - `GetRanges` is the coalesced multi-chunk fast-path the `gridded/virtual` VirtualiZarr cube and `catalog` `AssetFold` read against archival HDF5/NetCDF/GeoTIFF byte ranges, `coalesce` merging adjacent windows below the 1-MiB gap default into one request, never a per-chunk round-trip.
  - `List` `offset` is the last-seen object-path cursor resuming lexicographically after that key — a `str | None`, never an integer page index; `delimiter` switches to `list_with_delimiter`'s flat `ListResult`.
  - `Delete` absorbs the singular and plural call over `str | Sequence[str]` on the one case.
  - `Sign` is valid only on the `SignCapableStore` union (`S3Store | GCSStore | AzureStore`), so a sign on a `LocalStore`/`MemoryStore`/`HTTPStore` is the `NotSupportedError` the `boundary` converts; `expires_in` is a `timedelta`, never an `int`.
  - `list`'s `read` counts the `return_arrow=True` listing under `EgressUnit.OBJECTS`; `head` reports `OBJECT_SIZE`, `sign` reports `PATHS`, and only payload-bearing rows report `BYTES`; the `reader`/`writer`/`sign` `read`s carry the obstore handle or URL batch on the receipt `payload` slot, so no tag escapes the `_ROUTE` fold.
- Entry: `ObjectEgress.of` constructs both store handles and captures the composition scope. `run` and `run_async` open the egress client span, bind `_gated` before provider dispatch, and flatten the selected retry or boundary rail exactly once. Each `_Row.veto` value selects its mutation point, and `Hooks.fire(..., scope=self.scope)` returns subscriber rejection before provider mutation.
- Auto: the put short-circuits to a by-reference no-op when the payload's `ContentKey` — derived from the payload `bytes`, never the path — matches the prior egress reconciled against the remote `Head` `e_tag`, so an unchanged bundle never re-uploads. Both legs read the same `_ROUTE` row for `op.tag` under the identical kwargs plan, never a per-leg keyword allowlist. Every `obstore.exceptions` leaf lands on the `boundary` catch-all (none is a `CLASSIFY` row), so recovery keys on `fault.recoverable({"boundary"})` and a conditional write-once/copy-once collision surfaces as a terminal `boundary` fault read off the lifted message, never a retry-suppressing per-arm catch.
- Receipt: `EgressReceipt.contribute` yields one emitted-phase `Receipt.of("object-egress", ("emitted", path, facts))` — the two-argument `(owner, evidence)` form over the `(phase, subject, facts)` tuple, never a four-positional call — satisfying the `ReceiptContributor` `Iterable[Receipt]` Protocol. `quantity` and `unit` preserve byte volume, object count, signed-path count, and metadata size without overloading; `byte_length` projects only the `BYTES` rows consumed by cost and range folds. A byte-bearing op is keyed by its operation-bytes `ContentKey`; a control-plane op (`list`/`head`/`delete`/`copy`/`rename`/`reader`/`writer`/`sign`) carries `content_key=None` because it moves no operation bytes, so the receipt never digests a path string or a server-opaque `e_tag` to manufacture a key. `Head` reads its `e_tag`/`version`/`size` off the typed slots, so the no-op put confirms against that typed `e_tag` rather than a redundant key over `str(e_tag)`. A byte-bearing `contribute` projects `rasm.egress.byte_volume` onto the runtime `Metrics.record` arm under `domain="egress"` keyed by operation — the canonical object-store throughput instrument, data-owned beside the runtime's artifact byte-volume row; the egress spans open `kind=SpanKind.CLIENT` per the store-transport law.
- Packages: `obstore` is the sole store provider; `SignCapableStore`/`HTTP_METHOD` are stub-only typing references (`obstore._sign.pyi` `TypeAlias`), so `Method` inlines the nine members rather than a runtime import and `parse_scheme` stays unbound because `from_url` dispatches the scheme, while `GetResult.stream`'s `BytesStream` is a typing-only chunk-iterator growth value the same `Get` row carries. `obstore.exceptions`' leaf taxonomy (`BaseError` root and the eleven leaves) and the `config`/`client_options`/`retry_config` `TypedDict` shapes are `data/.api/obstore.md` catalog facts. `arro3.core` counts the listing rows; `beartype(conf=FAULT_CONF)` is the public admission contract on `of` the sibling `interop`/`store`/`ragged` factories share.
- Growth: a new store operation is one `StoreOp` case and one `_ROUTE` row carrying dispatch, argument planning, result projection, quantity unit, retry class, mutation point, and path; a new cloud backend one `from_url` URL scheme; a new precondition one `PutMode` on `Put` or one `overwrite` value on `Copy`/`Rename`; a new conditional-get axis one `GetOptions` key on `Get`; a new get-response evidence field one more `GetResult` member on the `Get` `read`'s `payload` tuple, never a second receipt; a new streaming or signing surface one `StoreOp` case whose `read` carries its non-byte value on the `payload` slot and emits a `None` `Source`, never a parallel `_HANDLE` table; a new governance concern is one subscriber the app root attaches.
- Boundary: this is the data-tier bundle-I/O owner — the full write/mutation direction (`put`/`copy`/`rename`/`delete`/`writer`) and the bundle-byte reads (`get`/`get_range`/`get_ranges`/`head`/`reader`/`sign`) its `columnar`/`geospatial`/`tensor`/`gridded/virtual`/`spatial/catalog` consumers need over Arrow/Parquet/GeoParquet/zarr bundles keyed by `ContentIdentity`. Runtime's `transport/roots#RESOURCE` owner holds the orthogonal generic-transport lane (the concurrent-small-object/large-artifact-stream read and generic survey/presign); the two share the `obstore` provider and `RetryClass.OBJECT_STORE` envelope but split by tier, so egress consumes `TransportResource`/`ResourceRef` only for credentials and never re-derives roots' read/survey lane. Composes — never re-mints — the `reliability/resilience#RESILIENCE` `guarded`/`guarded_sync` envelopes, the `reliability/faults#FAULT` `BoundaryFault` those lift, the `ContentIdentity` keyer, and the `opentelemetry` tracer; consumes the bundles rather than re-minting them. Rejected: a bare `guard`/`guard_sync` caller inside a hand-opened `boundary` and span (the doubled-span/doubled-lift form, where `guarded`/`guarded_sync` fuse the retry/child-span/terminal-lift once); a sync leg dropping a retried row's `OBJECT_STORE` outer envelope to lean on the `obstore` Rust-core `RetryConfig` where the async leg carries it; a blanket retry envelope re-driving the non-idempotent `copy`/`rename` whose ambiguously-succeeded first attempt a blind replay corrupts or mis-reports; a narrowed `catch=obstore.exceptions.BaseError` letting a row-projection `KeyError`/`AttributeError` escape rather than convert at the `CLASSIFY` `Exception` seam; a path-string `ContentIdentity.of` key against the identity owner's no-path law; a per-operation `from_url` re-mint; a parallel `S3Egress`/`AsyncObjectEgress` family.

```python signature
from collections.abc import Buffer, Callable, Iterable, Sequence
from datetime import timedelta
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal

import obstore
from beartype import beartype
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from obstore.store import ObjectStore, from_url
from opentelemetry import trace
from opentelemetry.trace import SpanKind

from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, async_boundary, boundary
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.resilience import RetryClass, guarded, guarded_sync
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from obstore import Attributes, GetOptions, ObjectMeta, PutMode, PutResult
    from obstore.store import AzureConfig, ClientConfig, GCSConfig, RetryConfig, S3Config

# `Method` mirrors the obstore `HTTP_METHOD` presign literal (nine members). `Source` is the runtime
# `evidence/identity#IDENTITY` payload axis widened with `None`: a byte-bearing op emits a zero-copy
# `Bytes`/`Buffer` whole (`Buffer()` arm) or a `get_ranges` `tuple[Bytes, ...]` keyed `stream`
# (`Iterable()` arm); a control-plane op emits `None` so `_receipt` mints NO key — the no-path law.
# `Source`'s `tuple[ContentKey, ...]` merkle arm is a modality the egress never mints.
type Config = "S3Config | GCSConfig | AzureConfig"
type Provider = Callable[[], Any] | None
type Method = Literal["GET", "PUT", "POST", "HEAD", "PATCH", "TRACE", "DELETE", "OPTIONS", "CONNECT"]
type Meta = "ObjectMeta | PutResult | None"
type Source = Buffer | Iterable[bytes] | tuple[ContentKey, ...] | None
type Call = tuple[tuple[Any, ...], dict[str, Any]]
type Read = tuple[int, Meta, Source, Any]


class EgressUnit(StrEnum):
    BYTES = "bytes"
    OBJECTS = "objects"
    PATHS = "paths"
    OBJECT_SIZE = "object_size"
    NONE = "none"

COALESCE: Final[int] = 1 << 20
CHUNK: Final[int] = 5 << 20
FANOUT: Final[int] = 12

_TRACER: Final = trace.get_tracer("rasm.data.tabular.egress")


@tagged_union(frozen=True)
class StoreOp:
    tag: Literal["put", "get", "get_range", "get_ranges", "list", "head", "delete", "copy", "rename", "reader", "writer", "sign"] = tag()
    put: tuple[bytes, "PutMode", "Attributes", dict[str, str], int] = case()
    get: tuple[str, "GetOptions | None"] = case()
    get_range: tuple[str, int, int | None, int | None] = case()
    get_ranges: tuple[str, tuple[int, ...], tuple[int, ...] | None, tuple[int, ...] | None, int] = case()
    list: tuple[str, str | None, bool] = case()
    head: str = case()
    delete: str | Sequence[str] = case()
    copy: tuple[str, str, bool] = case()
    rename: tuple[str, str, bool] = case()
    reader: tuple[str, int | None, int | None] = case()
    writer: tuple[str, "Attributes", dict[str, str], int | None, int] = case()
    sign: tuple[Method, Sequence[str], timedelta] = case()

    @staticmethod
    def Put(
        payload: bytes,
        mode: "PutMode" = "overwrite",
        attributes: "Attributes | None" = None,
        tags: dict[str, str] | None = None,
        chunk_size: int = CHUNK,
    ) -> StoreOp:
        return StoreOp(put=(payload, mode, attributes or {}, tags or {}, chunk_size))

    @staticmethod
    def Get(path: str, options: "GetOptions | None" = None) -> StoreOp:
        return StoreOp(get=(path, options))

    @staticmethod
    def GetRange(path: str, start: int, end: int | None = None, length: int | None = None) -> StoreOp:
        return StoreOp(get_range=(path, start, end, length))

    @staticmethod
    def GetRanges(
        path: str, starts: tuple[int, ...], ends: tuple[int, ...] | None = None, lengths: tuple[int, ...] | None = None, coalesce: int = COALESCE
    ) -> StoreOp:
        return StoreOp(get_ranges=(path, starts, ends, lengths, coalesce))

    @staticmethod
    def List(prefix: str, offset: str | None = None, delimiter: bool = False) -> StoreOp:
        return StoreOp(list=(prefix, offset, delimiter))

    @staticmethod
    def Head(path: str) -> StoreOp:
        return StoreOp(head=path)

    @staticmethod
    def Delete(paths: str | Sequence[str]) -> StoreOp:
        return StoreOp(delete=paths)

    @staticmethod
    def Copy(source: str, target: str, overwrite: bool = True) -> StoreOp:
        return StoreOp(copy=(source, target, overwrite))

    @staticmethod
    def Rename(source: str, target: str, overwrite: bool = True) -> StoreOp:
        return StoreOp(rename=(source, target, overwrite))

    @staticmethod
    def Reader(path: str, buffer_size: int | None = None, size: int | None = None) -> StoreOp:
        return StoreOp(reader=(path, buffer_size, size))

    @staticmethod
    def Writer(
        path: str,
        attributes: "Attributes | None" = None,
        tags: dict[str, str] | None = None,
        buffer_size: int | None = None,
        max_concurrency: int = FANOUT,
    ) -> StoreOp:
        return StoreOp(writer=(path, attributes or {}, tags or {}, buffer_size, max_concurrency))

    @staticmethod
    def Sign(method: Method = "GET", paths: Sequence[str] = (), expires_in: timedelta = timedelta(hours=1)) -> StoreOp:
        return StoreOp(sign=(method, paths, expires_in))


class _Row(Struct, frozen=True):
    sync: Callable[..., Any]
    aio: Callable[..., Any]
    plan: Callable[[StoreOp, str], Call]
    # `read`'s `Source` slot is operation bytes for a byte-bearing row, `None` for a control-plane row so `_receipt` mints no key.
    read: Callable[[StoreOp, str, Any], Read]
    path: Callable[[StoreOp, str], str]
    unit: EgressUnit = EgressUnit.BYTES
    veto: "HookPoint[EgressMutation] | None" = None
    # per-op mutation-class judgment: reads, same-bytes `put`, `delete`, `sign`, and the lazy `open_reader`/`open_writer` handle
    # mints replay safely under OBJECT_STORE (a conditional-write collision stays the terminal `boundary` fault); `copy` (a
    # provider-side multi-step rewrite chain) and `rename` (copy-then-delete) override to None and cross unretried — an
    # ambiguously-succeeded first attempt makes a blind replay corrupt state or mis-report `NotFound`/`AlreadyExists`.
    retry: RetryClass | None = RetryClass.OBJECT_STORE


def _listing(store: ObjectStore, prefix: str, *, offset: str | None, delimiter: bool) -> int:
    # `list_with_delimiter(return_arrow=True)["objects"]` is one arro3 `Table`: its `num_rows` IS the
    # count, read directly (iterating a `Table` yields `ChunkedArray` COLUMNS, not row batches); the
    # recursive `list` yields `RecordBatch` chunks summed by `num_rows`.
    if delimiter:
        return obstore.list_with_delimiter(store, prefix, return_arrow=True)["objects"].num_rows
    return sum(batch.num_rows for batch in obstore.list(store, prefix, offset=offset, return_arrow=True))


async def _listing_async(store: ObjectStore, prefix: str, *, offset: str | None, delimiter: bool) -> int:
    if delimiter:
        return (await obstore.list_with_delimiter_async(store, prefix, return_arrow=True))["objects"].num_rows
    return sum([batch.num_rows async for batch in obstore.list(store, prefix, offset=offset, return_arrow=True)])


class EgressMutation(Struct, frozen=True):
    # pre-flight mutation fact the veto edges fire — a receipt exists only after the provider call.
    operation: str
    path: str
    byte_length: int


PUT_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id="rasm.data.egress.put", payload=EgressMutation, modality=Modality.VETO)
DELETE_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id="rasm.data.egress.delete", payload=EgressMutation, modality=Modality.VETO)
COPY_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id="rasm.data.egress.copy", payload=EgressMutation, modality=Modality.VETO)
RENAME_POINT: Final[HookPoint[EgressMutation]] = HookPoint(id="rasm.data.egress.rename", payload=EgressMutation, modality=Modality.VETO)


_ROUTE: Final[Map[str, _Row]] = Map.of_seq([
    (
        "put",
        _Row(
            obstore.put,
            obstore.put_async,
            lambda op, t: ((t, op.put[0]), {"mode": op.put[1], "attributes": op.put[2], "tags": op.put[3], "chunk_size": op.put[4]}),
            lambda op, t, result: (len(op.put[0]), result, op.put[0], None),
            lambda op, t: t,
            veto=PUT_POINT,
        ),
    ),
    (
        "get",
        _Row(
            obstore.get,
            obstore.get_async,
            lambda op, t: ((op.get[0],), {"options": op.get[1]}),
            lambda op, t, result: (len(body := result.bytes()), result.meta, body, (result.range, result.attributes)),
            lambda op, t: op.get[0],
        ),
    ),
    (
        "get_range",
        _Row(
            obstore.get_range,
            obstore.get_range_async,
            lambda op, t: ((op.get_range[0],), {"start": op.get_range[1], "end": op.get_range[2], "length": op.get_range[3]}),
            lambda op, t, window: (len(window), None, window, None),
            lambda op, t: op.get_range[0],
        ),
    ),
    (
        "get_ranges",
        _Row(
            obstore.get_ranges,
            obstore.get_ranges_async,
            lambda op, t: (
                (op.get_ranges[0],),
                {"starts": op.get_ranges[1], "ends": op.get_ranges[2], "lengths": op.get_ranges[3], "coalesce": op.get_ranges[4]},
            ),
            lambda op, t, windows: (sum(len(w) for w in windows), None, tuple(windows), None),
            lambda op, t: op.get_ranges[0],
        ),
    ),
    (
        "list",
        _Row(
            _listing,
            _listing_async,
            lambda op, t: ((op.list[0],), {"offset": op.list[1], "delimiter": op.list[2]}),
            lambda op, t, rows: (rows, None, None, None),
            lambda op, t: op.list[0],
            unit=EgressUnit.OBJECTS,
        ),
    ),
    (
        "head",
        _Row(
            obstore.head,
            obstore.head_async,
            lambda op, t: ((op.head,), {}),
            lambda op, t, meta: (meta["size"], meta, None, None),
            lambda op, t: op.head,
            unit=EgressUnit.OBJECT_SIZE,
        ),
    ),
    (
        "delete",
        _Row(
            obstore.delete,
            obstore.delete_async,
            lambda op, t: ((op.delete,), {}),
            lambda op, t, _: (0, None, None, None),
            lambda op, t: op.delete if isinstance(op.delete, str) else ",".join(op.delete),
            unit=EgressUnit.NONE,
            veto=DELETE_POINT,
        ),
    ),
    (
        "copy",
        _Row(
            obstore.copy,
            obstore.copy_async,
            lambda op, t: ((op.copy[0], op.copy[1]), {"overwrite": op.copy[2]}),
            lambda op, t, _: (0, None, None, None),
            lambda op, t: op.copy[1],
            unit=EgressUnit.NONE,
            veto=COPY_POINT,
            retry=None,
        ),
    ),
    (
        "rename",
        _Row(
            obstore.rename,
            obstore.rename_async,
            lambda op, t: ((op.rename[0], op.rename[1]), {"overwrite": op.rename[2]}),
            lambda op, t, _: (0, None, None, None),
            lambda op, t: op.rename[1],
            unit=EgressUnit.NONE,
            veto=RENAME_POINT,
            retry=None,
        ),
    ),
    (
        "reader",
        _Row(
            obstore.open_reader,
            obstore.open_reader_async,
            lambda op, t: ((op.reader[0],), {k: v for k, v in (("buffer_size", op.reader[1]), ("size", op.reader[2])) if v is not None}),
            lambda op, t, file: (0, None, None, file),
            lambda op, t: op.reader[0],
            unit=EgressUnit.NONE,
        ),
    ),
    (
        "writer",
        _Row(
            obstore.open_writer,
            obstore.open_writer_async,
            lambda op, t: (
                (op.writer[0],),
                {
                    k: v
                    for k, v in (
                        ("attributes", op.writer[1]),
                        ("tags", op.writer[2]),
                        ("buffer_size", op.writer[3]),
                        ("max_concurrency", op.writer[4]),
                    )
                    if v is not None
                },
            ),
            lambda op, t, file: (0, None, None, file),
            lambda op, t: op.writer[0],
            unit=EgressUnit.NONE,
        ),
    ),
    (
        "sign",
        _Row(
            obstore.sign,
            obstore.sign_async,
            lambda op, t: ((op.sign[0], op.sign[1], op.sign[2]), {}),
            lambda op, t, urls: (len(op.sign[1]), None, None, urls),
            lambda op, t: op.sign[1][0] if op.sign[1] else "",
            unit=EgressUnit.PATHS,
        ),
    ),
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
        )

    def contribute(self) -> Iterable[Receipt]:
        # `byte_length` rides as a native `int`: the receipts `Encoder(enc_hook=repr)` serializes
        # scalars without a `str()` coerce. Byte-bearing ops project throughput onto the metric spine
        # under domain="egress" keyed by operation; control-plane ops (byte_length 0) record nothing.
        if self.byte_length:
            Metrics.record({"rasm.egress.byte_volume": float(self.byte_length)}, domain="egress", kind=self.operation)
        yield Receipt.of(
            "object-egress",
            (
                "emitted",
                self.path,
                {
                    "op": self.operation,
                    "bytes": self.byte_length,
                    "quantity": self.quantity,
                    "unit": self.unit,
                    "etag": self.e_tag,
                    "version": self.version,
                    "key": self.content_key.hex if self.content_key is not None else None,
                },
            ),
        )


class ObjectEgress(Struct, frozen=True):
    ref: ResourceRef
    store: ObjectStore
    direct: ObjectStore  # zero-retry twin the mutation-ambiguous rows (`copy`/`rename`) cross — the provider's own HTTP replay is off too
    scope: ScopeKey = DEFAULT_SCOPE

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(
        cls,
        ref: ResourceRef,
        config: Config | None = None,
        client_options: "ClientConfig | None" = None,
        retry_config: "RetryConfig | None" = None,
        credential_provider: Provider = None,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> ObjectEgress:
        # both handles mint ONCE here: the retried store for the replay-safe rows, and the `max_retries: 0` twin so a
        # `retry=None` row is invoked exactly once end to end — the rail envelope AND the Rust-core replay both disabled,
        # because an ambiguously-succeeded copy/rename replayed inside the provider corrupts state the same as one
        # replayed by the rail.
        return cls(
            ref=ref,
            store=from_url(
                ref.root, config=config, client_options=client_options, retry_config=retry_config, credential_provider=credential_provider
            ),
            direct=from_url(
                ref.root, config=config, client_options=client_options, retry_config={"max_retries": 0}, credential_provider=credential_provider
            ),
            scope=scope,
        )

    def run(self, op: StoreOp, path: str = "") -> RuntimeRail[EgressReceipt]:
        # kind=CLIENT: the store transport is the outbound network leg, a local/memory scheme its degenerate client.
        retry, target = _ROUTE[op.tag].retry, path or self.ref.relative
        with _TRACER.start_as_current_span(f"egress.{op.tag}", kind=SpanKind.CLIENT, attributes={"rasm.egress.scheme": self.ref.scheme}):
            return self._gated(op, target).bind(
                lambda admitted: (
                    guarded_sync(retry, self._apply, admitted, target, subject=f"egress.{op.tag}")
                    if retry is not None
                    else boundary(f"egress.{op.tag}", lambda: self._apply(admitted, target))
                ).bind(lambda rail: rail)
            )

    async def run_async(self, op: StoreOp, path: str = "") -> RuntimeRail[EgressReceipt]:
        retry, target = _ROUTE[op.tag].retry, path or self.ref.relative
        with _TRACER.start_as_current_span(f"egress.{op.tag}", kind=SpanKind.CLIENT, attributes={"rasm.egress.scheme": self.ref.scheme}):
            match self._gated(op, target):
                case Result(tag="error", error=fault):
                    return Error(fault)
                case Result(tag="ok", ok=admitted):
                    fenced = (
                        await guarded(retry, self._apply_async, admitted, target, subject=f"egress.{op.tag}")
                        if retry is not None
                        else await async_boundary(f"egress.{op.tag}", lambda: self._apply_async(admitted, target))
                    )
                    return fenced.bind(lambda rail: rail)

    def _gated(self, op: StoreOp, target: str) -> RuntimeRail[StoreOp]:
        # Veto pre-flight: put/delete/copy/rename fire their registered row before provider mutation — a subscriber
        # rejection returns on this rail — and every other tag passes untouched through the absent-row default.
        row = _ROUTE[op.tag]
        if row.veto is None:
            return Ok(op)
        return Hooks.fire(
            row.veto.id,
            EgressMutation(
                operation=op.tag,
                path=row.path(op, target),
                byte_length=len(op.put[0]) if op.tag == "put" else 0,
            ),
            scope=self.scope,
        ).map(lambda _fact: op)

    def _receipt(self, op: StoreOp, target: str, returned: Any) -> RuntimeRail[EgressReceipt]:
        row = _ROUTE[op.tag]
        length, meta, source, payload = row.read(op, target, returned)

        def receipt(key: ContentKey | None) -> EgressReceipt:
            return EgressReceipt.of(op.tag, row.path(op, target), length, row.unit, meta, key, payload)

        # `source is None` (control-plane) mints NO key; else the one `ContentIdentity.of` classifies the
        # `Source` and rails the key, scheme-scoped through `self.ref.scheme`.
        if source is None:
            return Ok(receipt(None))
        return ContentIdentity.of(f"egress.{op.tag}.{self.ref.scheme}", source).map(receipt)

    def _apply(self, op: StoreOp, target: str) -> RuntimeRail[EgressReceipt]:
        row = _ROUTE[op.tag]
        args, kwargs = row.plan(op, target)
        return self._receipt(op, target, row.sync(self.store if row.retry is not None else self.direct, *args, **kwargs))

    async def _apply_async(self, op: StoreOp, target: str) -> RuntimeRail[EgressReceipt]:
        row = _ROUTE[op.tag]
        args, kwargs = row.plan(op, target)
        return self._receipt(op, target, await row.aio(self.store if row.retry is not None else self.direct, *args, **kwargs))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
