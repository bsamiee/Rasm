# [PY_DATA_EGRESS]

`ObjectEgress` is the native object-store egress owner: one façade over `obstore` for the Arrow/Parquet/GeoParquet/zarr bundles `columnar`, `geospatial`, and `tensor` emit, keyed by runtime `ContentIdentity` over the operation's real bytes. It discriminates the `StoreOp` tagged-union axis — put/get/get_range/get_ranges/list/head/delete/copy/rename and the handle-bearing reader/writer/sign cases — routing every case through one `_ROUTE` data table so the synchronous `run` and awaitable `run_async` fold over one row set rather than a structural `match`. It owns the full write/mutation direction and the bundle-byte reads its consumers need; the runtime `runtime/transport/roots#RESOURCE` owner holds the orthogonal generic-transport lane over the same provider.

Content keys derive from operation bytes through one `ContentIdentity.of`, scheme-scoped through `ResourceRef.scheme` and never the path, so an unchanged content-key is a put no-op confirmed against the `e_tag` on the caller's prior egress receipt. Retry is a per-row mutation-class disposition on `_ROUTE`: reads and idempotent mutations delegate the retried-traced-railed acquisition to the `runtime/reliability/resilience#RESILIENCE` `guarded`/`guarded_sync` fused envelopes over the `RetryClass.OBJECT_STORE` row (the `runtime/reliability/faults#FAULT` `BoundaryFault` those lift, the child `resilience.guarded` span beneath one `_TRACER` egress parent), while the non-idempotent `copy`/`rename` cross once through the bare `boundary`/`async_boundary` fence — both legs self-flattened `.bind(lambda rail: rail)` so the identity-failure rail shares the obstore-failure carrier. Its store backend is the one `ObjectStore` handle `from_url` constructs off the `ref.root` scheme — the canonical store-handle Union the sibling `runtime/transport/roots#RESOURCE` cache also carries — frozen at `of` over the credential-bearing `TransportResource`. Because the `obstore` sync and async members carry identical keyword signatures, `_async` is the same row read under a second entrypoint (catalogue `_async`-identity law), never a parallel `AsyncStoreOp` family.

## [01]-[INDEX]

- [02]-[EGRESS]: the object-store egress owner routing one `StoreOp` axis through one `_ROUTE` fold for the synchronous `run` and awaitable `run_async`.

## [02]-[EGRESS]

- Owner: `ObjectEgress` — the one object-store egress façade over `obstore`, discriminating `StoreOp` through one `_ROUTE` data table over one `_Row` shape, never a `put_object`/`get_object`/`list_objects` method family, never an async method twin, never a second `_HandleRow` beside the byte-receipt rows. Its store backend is the one `ObjectStore` handle `from_url` constructs off the `ref.root` scheme, the canonical store-handle Union the sibling `runtime/transport/roots#RESOURCE` `_object_store` cache also carries, so a new cloud backend is one URL scheme, never a parallel store class.
- Cases: the fence's factories and `_ROUTE` rows carry each obstore member; the decisions the fence cannot show —
  - `Put` `mode` ∈ `create|overwrite` or an `UpdateVersion` `PutMode` for the conditional write-once; multipart auto-selected when the payload exceeds `chunk_size`, the threshold an explicit request value defaulting to the 5-MiB `CHUNK` axis, never the provider default; `prior` is this path's last egress receipt, the caller-supplied by-reference evidence `_reuse` reads.
  - `GetRange` addresses one window as start+end or start+length — `end` and `length` are mutually exclusive descriptors on the one case.
  - `GetRanges` is the coalesced multi-chunk fast-path the `gridded/virtual` VirtualiZarr cube and `catalog` `AssetFold` read against archival HDF5/NetCDF/GeoTIFF byte ranges, `coalesce` merging adjacent windows below the 1-MiB gap default into one request, never a per-chunk round-trip.
  - `List` `offset` is the last-seen object-path cursor resuming lexicographically after that key — a `str | None`, never an integer page index; `delimiter` switches to `list_with_delimiter`'s flat `ListResult`.
  - `Delete` absorbs the singular and plural call over `str | Sequence[str]` on the one case.
  - `Sign` is valid only on the `SignCapableStore` union (`S3Store | GCSStore | AzureStore`), so a sign on a `local`/`memory`/`http` backend refuses at the `_REFUSAL` matrix ahead of the provider rather than riding out as its `NotSupportedError`; `expires_in` is a `timedelta`, never an `int`.
  - `list`'s `read` counts the `return_arrow=True` listing under `EgressUnit.OBJECTS`; `head` reports `OBJECT_SIZE`, `sign` reports `PATHS`, and only payload-bearing rows report `BYTES`; the `reader`/`writer`/`sign` `read`s carry the obstore handle or URL batch on the receipt `payload` slot, so no tag escapes the `_ROUTE` fold.
- Entry: `ObjectEgress.of` constructs both store handles, classifies the backend once through `obstore.parse_scheme`, and captures the composition scope. `run` and `run_async` open the egress client span off the faults-owned `scoped` stamp, bind `_gated` before provider dispatch, and flatten the selected retry or boundary rail exactly once; the awaited leg reads the gate rail through a `match` closed by `assert_never` rather than a `bind`, so both entrypoints stay total over the carrier. `_gated` reads the `(backend, tag)` `_REFUSAL` matrix first so a cell no backend serves answers its typed row ahead of every provider call and every hook fire, then each `_Row.veto` value selects its mutation point and `Hooks.fire(..., scope=self.scope)` returns subscriber rejection before provider mutation.
- Auto: `_reuse` short-circuits a put to a by-reference no-op only against an IDENTICAL write contract — the `Put.prior` receipt names the `put` operation and this same destination, its `contract` renders the same attributes and tags this put carries, its `e_tag` is non-empty so that upload was acknowledged, and the payload's `ContentKey` — derived from the payload `bytes`, never the path — equals its key; a conditional `mode` refuses the skip outright, because `create` and every `UpdateVersion` carry a remote precondition only the provider answers. So an unchanged bundle never re-uploads, the skipped receipt keys identically to the upload it stands in for, and a prior read off another path or another metadata contract can never settle a write nobody made. This owner opens no probe of its own: freshness of `prior` stays the caller's, because a `Head` round-trip per put spends exactly the traffic the short-circuit removes. Both legs read the same `_ROUTE` row for `op.tag` under the identical kwargs plan and the identical `_reuse`/`_handle` prologue, never a per-leg keyword allowlist. Every `obstore.exceptions` leaf lands on the `boundary` catch-all (none is a `CLASSIFY` row), so recovery keys on `fault.recoverable({"boundary"})` and a conditional write-once/copy-once collision surfaces as a terminal `boundary` fault read off the lifted message, never a retry-suppressing per-arm catch.
- Receipt: `EgressReceipt.contribute` yields one emitted-phase `Receipt.of("object-egress", ("emitted", path, facts))` — the two-argument `(owner, evidence)` form over the `(phase, subject, facts)` tuple, never a four-positional call — satisfying the `ReceiptContributor` `Iterable[Receipt]` Protocol. `quantity` and `unit` preserve byte volume, object count, signed-path count, and metadata size without overloading; `byte_length` projects only the `BYTES` rows consumed by cost and range folds. Byte-bearing ops key by their operation-bytes `ContentKey`; a control-plane op (`list`/`head`/`delete`/`copy`/`rename`/`reader`/`writer`/`sign`) carries `content_key=None` because it moves no operation bytes, so the receipt never digests a path string or a server-opaque `e_tag` to manufacture a key, and its fact map OMITS `key` rather than writing a null into the rendered-value contract the `tabular/lakehouse#LAKEHOUSE` residence reads. `Head` reads its `e_tag`/`version`/`size` off the typed slots, so the no-op put confirms against that typed `e_tag` rather than a redundant key over `str(e_tag)`, and a settled no-op carries `reused=True` with zero bytes — the receipt is where a skipped upload is visible, because an operation moving no bytes emits no throughput point to carry it. Byte-bearing `contribute` projects `rasm.egress.byte_volume` onto the runtime `Metrics.record` arm under `domain="egress"` keyed by operation — the canonical object-store throughput instrument, data-owned beside the runtime's artifact byte-volume row; the egress spans open `kind=SpanKind.CLIENT` per the store span-kind law.
- Packages: `obstore` is the sole store provider; `SignCapableStore`/`HTTP_METHOD` are stub-only typing references (`obstore._sign.pyi` `TypeAlias`), so `Method` inlines the nine members rather than a runtime import, while the module-level `obstore.parse_scheme` IS bound — it answers the closed six-member backend classification the reach matrix keys on, which `from_url`'s own dispatch settles internally and never exposes — and `GetResult.stream`'s `BytesStream` is a typing-only chunk-iterator growth value the same `Get` row carries. `obstore.exceptions`' leaf taxonomy (`BaseError` root and the eleven leaves) and the `config`/`client_options`/`retry_config` `TypedDict` shapes are `data/.api/obstore.md` catalog facts. `arro3.core` counts the listing rows; `beartype(conf=FAULT_CONF)` is the public admission contract on `of` the sibling `interop`/`store`/`ragged` factories share.
- Growth: a new store operation is one `StoreOp` case and one `_ROUTE` row carrying dispatch, argument planning, result projection, quantity unit, retry class, mutation point, and path; a new cloud backend one `from_url` URL scheme with its `Backend` member and any `_REFUSAL` cells it cannot serve; a newly unreachable cell one `_REFUSAL` row carrying its `EgressRefusal` reason; a new precondition one `PutMode` on `Put` or one `overwrite` value on `Copy`/`Rename`; a second by-reference short-circuit one `_reuse` arm over that op's own prior evidence, the `reused` and `contract` receipt slots already carrying it; a newly output-affecting write knob one `_contract` term beside its `StoreOp` slot; a new conditional-get axis one `GetOptions` key on `Get`; a new get-response evidence field one more `GetResult` member on the `Get` `read`'s `payload` tuple, never a second receipt; a new streaming or signing surface one `StoreOp` case whose `read` carries its non-byte value on the `payload` slot and emits a `None` `Source`, never a parallel `_HANDLE` table; a new governance concern is one subscriber the app root attaches.
- Boundary: this is the data-tier bundle-I/O owner — the full write/mutation direction (`put`/`copy`/`rename`/`delete`/`writer`) and the bundle-byte reads (`get`/`get_range`/`get_ranges`/`head`/`reader`/`sign`) its `columnar`/`geospatial`/`tensor`/`gridded/virtual`/`spatial/catalog` consumers need over Arrow/Parquet/GeoParquet/zarr bundles keyed by `ContentIdentity`. Runtime's `runtime/transport/roots#RESOURCE` owner holds the orthogonal generic-transport lane (the concurrent-small-object/large-artifact-stream read and generic survey/presign); the two share the `obstore` provider and `RetryClass.OBJECT_STORE` envelope but split by tier, so egress consumes `TransportResource`/`ResourceRef` only for credentials and never re-derives roots' read/survey lane. Composes — never re-mints — the `runtime/reliability/resilience#RESILIENCE` `guarded`/`guarded_sync` envelopes, the `runtime/reliability/faults#FAULT` `BoundaryFault` those lift, the `ContentIdentity` keyer, and the `opentelemetry` tracer; consumes the bundles rather than re-minting them. Rejected: a capability bound answered by a provider exception where the sibling owners answer it as a matrix row; a mutation row that fires no veto point while the boundary above it names the mutation; a bare `guard`/`guard_sync` caller inside a hand-opened `boundary` and span (the doubled-span/doubled-lift form, where `guarded`/`guarded_sync` fuse the retry/child-span/terminal-lift once); a sync leg dropping a retried row's `OBJECT_STORE` outer envelope to lean on the `obstore` Rust-core `RetryConfig` where the async leg carries it; a blanket retry envelope re-driving the non-idempotent `copy`/`rename` whose ambiguously-succeeded first attempt a blind replay corrupts or mis-reports; a narrowed `catch=obstore.exceptions.BaseError` letting a row-projection `KeyError`/`AttributeError` escape rather than convert at the `CLASSIFY` `Exception` seam; a path-string `ContentIdentity.of` key against the identity owner's no-path law; a per-operation `from_url` re-mint; an owner-held prior-egress ledger, or a `Head` probe opened per put to refresh one, where a stateless owner cannot know which composition last wrote the path and the probe costs the round-trip the short-circuit exists to remove; a parallel `S3Egress`/`AsyncObjectEgress` family.

```python signature
from collections.abc import Buffer, Callable, Iterable, Sequence
from datetime import timedelta
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import obstore
from beartype import beartype
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from obstore.store import ObjectStore, from_url
from opentelemetry import trace
from opentelemetry.trace import SpanKind

from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, async_boundary, boundary, scoped
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.resilience import RetryClass, guarded, guarded_sync
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from obstore import Attributes, GetOptions, ObjectMeta, PutMode, PutResult
    from obstore.store import AzureConfig, ClientConfig, GCSConfig, RetryConfig, S3Config

# `Method` mirrors the obstore `HTTP_METHOD` presign literal (nine members). `Source` is the runtime
# `runtime/evidence/identity#IDENTITY` payload axis widened with `None`: a byte-bearing op emits a zero-copy
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
# `obstore.parse_scheme` answers this closed backend classification off a store URL — the reach matrix's key half,
# read once at `of` so no cell probes the handle's runtime class or the raw scheme string.
type Backend = Literal["s3", "gcs", "azure", "http", "local", "memory"]


class EgressRefusal(StrEnum):
    # every refused `(backend, op)` cell names its own reason and the member value IS the operator-facing evidence
    # `BoundaryFault` carries, exactly as the sibling `tabular/lakehouse#LAKEHOUSE` `_REFUSAL` and
    # `tabular/query#QUERY` `_REMOTE_REFUSAL` matrices state theirs — reject law is data on every owner, so an
    # unreachable cell never rides out as a provider exception a caller cannot tell from a transport fault.
    SIGN_UNSUPPORTED = "presigning reaches the cloud backends alone; this scheme mints no signed url"


class EgressUnit(StrEnum):
    BYTES = "bytes"
    OBJECTS = "objects"
    PATHS = "paths"
    OBJECT_SIZE = "object_size"
    NONE = "none"

COALESCE: Final[int] = 1 << 20
CHUNK: Final[int] = 5 << 20
FANOUT: Final[int] = 12

# faults-owned scope stamp: `scoped` binds the version and semconv triple, so no page re-spells the pin.
_TRACER: Final = scoped(trace.get_tracer, "rasm.data.tabular.egress")


@tagged_union(frozen=True)
class StoreOp:
    tag: Literal["put", "get", "get_range", "get_ranges", "list", "head", "delete", "copy", "rename", "reader", "writer", "sign"] = tag()
    put: tuple[bytes, "PutMode", "Attributes", dict[str, str], int, "EgressReceipt | None"] = case()
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
        prior: "EgressReceipt | None" = None,
    ) -> StoreOp:
        # `prior` is this path's last egress receipt — the caller's own by-reference evidence, never an owner-held
        # ledger, because a stateless owner cannot know which of many compositions last wrote this path.
        return StoreOp(put=(payload, mode, attributes or {}, tags or {}, chunk_size, prior))

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


def _contract(op: StoreOp) -> str:
    # the OUTPUT-affecting half of a write beyond its bytes: the metadata the stored object carries afterwards.
    # Rendered from sorted pairs, so two writes declaring one contract render one token whatever order a caller
    # built its maps in, and a later put reads it back off the prior receipt as the by-reference proof. `chunk_size`
    # stays OUT — it selects single-part against multipart transfer and changes no stored byte or header, so folding
    # it in would refuse a sound skip over a re-tuned threshold. Every other tag writes no metadata and renders empty.
    attributes, tags = (op.put[2], op.put[3]) if op.tag == "put" else (op.writer[1], op.writer[2]) if op.tag == "writer" else ({}, {})
    return repr((sorted((str(key), str(value)) for key, value in dict(attributes).items()), sorted(tags.items())))


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
            # `writer` opens a multipart UPLOAD — the fifth mutation this owner's boundary names — so it fires the
            # same pre-flight point `put` does; a governance tap that vetoes a put and silently admits a streamed
            # write of the same bytes gates nothing. The streamed payload has no length yet, so the fact reports zero.
            veto=PUT_POINT,
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


# `(backend, tag)` reach matrix: an absent cell is reachable and rides its `_ROUTE` row, a present cell refuses
# with its own reason. The matrix outranks the route, so a seventh backend lands its unreachable cells as rows.
_REFUSAL: Final[Map[tuple[Backend, str], EgressRefusal]] = Map.of_seq([
    (("http", "sign"), EgressRefusal.SIGN_UNSUPPORTED),
    (("local", "sign"), EgressRefusal.SIGN_UNSUPPORTED),
    (("memory", "sign"), EgressRefusal.SIGN_UNSUPPORTED),
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
            Metrics.record({"rasm.egress.byte_volume": float(self.byte_length)}, domain="egress", kind=self.operation)
        yield Receipt.of(
            "object-egress",
            (
                "emitted",
                self.path,
                {
                    "domain": "egress",
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


class ObjectEgress(Struct, frozen=True):
    ref: ResourceRef
    store: ObjectStore
    direct: ObjectStore  # zero-retry twin the mutation-ambiguous rows (`copy`/`rename`) cross — the provider's own HTTP replay is off too
    # `backend` holds the provider's OWN closed classification, resolved once beside the handles `from_url` builds off the
    # same root — the reach matrix keys on it, so no cell probes a store's runtime class or re-parses the URL.
    backend: Backend = "memory"
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
            backend=obstore.parse_scheme(ref.root),
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
            # veto crosses a `match` rather than a `bind` because the fenced leg is awaited, and `assert_never`
            # keeps the arms total over the carrier — an unclosed two-arm match falls through returning `None`
            # past the declared rail.
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
                case unreachable:
                    assert_never(unreachable)

    def _gated(self, op: StoreOp, target: str) -> RuntimeRail[StoreOp]:
        # ONE prologue both entrypoints read: reach answers a cell this backend cannot serve BEFORE the veto point
        # fires and before any provider call, then the mutation rows fire their point. A `sign` against a local,
        # memory, or http store otherwise reached the provider's own `NotSupportedError` and surfaced as an untyped
        # boundary fault a caller could not tell from a transport failure — a capability bound wearing a fault's
        # clothes, where every sibling owner answers the same class as a matrix row.
        return (
            _REFUSAL.try_find((self.backend, op.tag))
            .map(lambda refusal: Error(BoundaryFault(boundary=(f"egress.{op.tag}", refusal))))
            .default_value(Ok(op))
            .bind(lambda admitted: self._vetoed(admitted, target))
        )

    def _vetoed(self, op: StoreOp, target: str) -> RuntimeRail[StoreOp]:
        # Veto pre-flight: put/writer/delete/copy/rename fire their registered row before provider mutation — a
        # subscriber rejection returns on this rail — and every other tag passes untouched through the absent-row default.
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
            return EgressReceipt.of(op.tag, row.path(op, target), length, row.unit, meta, key, payload, contract=_contract(op))

        # `source is None` (control-plane) mints NO key; else the one `ContentIdentity.of` classifies the
        # `Source` and rails the key, scheme-scoped through `self.ref.scheme`.
        if source is None:
            return Ok(receipt(None))
        return ContentIdentity.of(f"egress.{op.tag}.{self.ref.scheme}", source).map(receipt)

    def _reuse(self, op: StoreOp, target: str) -> "Option[RuntimeRail[EgressReceipt]]":
        # By-reference put no-op, and every half is load-bearing because a settled skip CLAIMS a write nobody made:
        # the prior receipt names this operation and THIS destination, its `contract` renders the same object
        # metadata this put carries, its non-empty `e_tag` proves that upload was acknowledged rather than merely
        # attempted, and the fresh payload key equals its key so the BYTES are unchanged. A prior read off another
        # path, another metadata contract, or another operation settles a write that never happened at all. Mode is
        # the fifth half and refuses rather than compares: `create` and every `UpdateVersion` carry a REMOTE
        # precondition only the provider can answer, so a conditional put crosses and lets it. `Nothing` means no
        # short-circuit and the provider call runs; a digest fault rides out as `Some(Error(...))` so an unreadable
        # payload never falls through into an upload. Remote drift SINCE that egress is the caller's to detect by
        # handing a fresher prior — a `Head` probe per put spends exactly the round-trip this short-circuit removes —
        # and a settled skip keeps the same content key an upload would have minted, so the reuse ledger is unbroken.
        prior = op.put[5] if op.tag == "put" else None
        if (
            prior is None
            or prior.content_key is None
            or not prior.e_tag
            or prior.operation != "put"
            or prior.path != target
            or prior.contract != _contract(op)
            or op.put[1] != "overwrite"
        ):
            return Nothing
        match ContentIdentity.of(f"egress.{op.tag}.{self.ref.scheme}", op.put[0]):
            case Result(tag="error", error=fault):
                return Some(Error(fault))
            case Result(tag="ok", ok=key) if key == prior.content_key:
                # `(e_tag, version)` on the prior IS an `UpdateVersion`, so it re-enters `of` through the `Meta`
                # slot and the reused receipt carries the same remote identity a fresh `PutResult` would.
                carried = {"e_tag": prior.e_tag or None, "version": prior.version or None}
                return Some(Ok(EgressReceipt.of(op.tag, target, 0, EgressUnit.NONE, carried, key, None, reused=True, contract=prior.contract)))
            case _:
                return Nothing

    def _apply(self, op: StoreOp, target: str) -> RuntimeRail[EgressReceipt]:
        row = _ROUTE[op.tag]
        args, kwargs = row.plan(op, target)
        reused = self._reuse(op, target)
        return reused.value if reused.is_some() else self._receipt(op, target, row.sync(self._handle(row), *args, **kwargs))

    async def _apply_async(self, op: StoreOp, target: str) -> RuntimeRail[EgressReceipt]:
        row = _ROUTE[op.tag]
        args, kwargs = row.plan(op, target)
        reused = self._reuse(op, target)
        return reused.value if reused.is_some() else self._receipt(op, target, await row.aio(self._handle(row), *args, **kwargs))

    def _handle(self, row: _Row) -> ObjectStore:
        # one store selection both legs read off the row's own mutation-class judgment.
        return self.store if row.retry is not None else self.direct
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
