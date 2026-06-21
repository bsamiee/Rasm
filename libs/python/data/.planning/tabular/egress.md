# [PY_DATA_EGRESS]

The native object-store egress owner: one `ObjectEgress` façade over `obstore` for the Arrow/Parquet/GeoParquet/zarr bundles the `columnar`, `geospatial`, and `tensor` owners emit, keyed by runtime `ContentIdentity` over the operation's real bytes. `ObjectEgress` discriminates the `StoreOp` tagged-union axis — put/get/get_range/get_ranges/list/head/delete/copy/rename plus the handle-bearing reader/writer/sign cases — over the highest-throughput native `object_store` path against one `ObjectStoreMethods` store handle constructed once at `of` through `from_url`, and routes every case through one `_ROUTE` data table pairing each tag with its sync member, its `_async` sibling, one argument plan both legs read, one reader projecting the obstore return to a byte length plus the `ObjectMeta`/`PutResult` carried plus the canonical `ContentKey` the bytes derive, so one `_apply` fold serves the synchronous `run` and one `_apply_async` fold serves the awaitable `run_async`. The `_async` sibling is the same row read under a second entrypoint over the identical kwargs plan because the obstore sync and async members carry identical keyword signatures (catalogue `_async`-identity law), never a parallel `AsyncStoreOp` family, never a per-leg keyword allowlist, and never two enumerated matches; dispatch is keyed by `op.tag` into the row rather than an `expression` structural `match`, so the case projection, receipt key, and the handle/sign args all fold onto the one row table and no tag escapes to a hand-written arm. The `list` arm folds into the same row through a reader that sums `arro3.core.RecordBatch.num_rows` over the `return_arrow=True` stream rather than a payload-byte count, and the handle-bearing `reader`/`writer`/`sign` arms fold through a reader that returns the obstore handle or the presigned-URL batch as the receipt's carried value, so the byte-receipt, row-sum, and handle cases all stay in the one table. The content key derives from the operation's real bytes through exactly one `ContentIdentity.of` — the `put` payload `bytes`, the `get` body `Bytes`, and the `get_ranges` windows folded as a child-`ContentKey` tuple under the Merkle modality — never from the path or filename the identity owner forbids as a key source, so an unchanged content-key is a put no-op by reference confirmed against the prior `Head` `e_tag` rather than a path-string hash. The raised `obstore.exceptions.*` is lifted exactly once through the runtime `boundary`/`async_boundary` owner — `catch=BaseError` narrows the funnel to the obstore taxonomy and `BoundaryFault.of` carries `type(cause).__name__` from the `NotFoundError`/`AlreadyExistsError`/`PreconditionError`/`NotModifiedError`/`PermissionDeniedError`/`UnauthenticatedError`/`NotSupportedError`/`InvalidPathError`/`UnknownConfigurationKeyError` leaf set so recovery keys on the tag, never a hand-rolled per-arm `try`/`except` and never a reconstructed message.

## [01]-[INDEX]

- [01]-[EGRESS]: the native object-store egress owner over one `StoreOp` axis, table-routed sync and async through one `_ROUTE` fold over one `_Row` shape, composing the runtime transport, resilience, and the one `boundary` fault conversion.

## [02]-[EGRESS]

- Owner: `ObjectEgress` — the one object-store egress façade over `obstore`; `StoreOp` the tagged-union operation axis (put/get/get_range/get_ranges/list/head/delete/copy/rename/reader/writer/sign), routed through one `_ROUTE` data table over one `_Row` shape so a new store operation is one `StoreOp` case plus one `_ROUTE` row, never a `put_object`/`get_object`/`list_objects` method family, never an async method twin, and never a second `_HandleRow` shape beside the byte-receipt rows. `EgressReceipt` is the typed egress receipt — operation, path, byte length, e-tag, version, content-key, and the carried handle/URL value — satisfying the runtime `ReceiptContributor` Protocol. The store backend is the one `obstore.store.from_url` constructs from the `ResourceRef` scheme (`s3://`/`gs://`/`az://`/`file:///`/`memory:///`), the `config` riding the credential-bearing `TransportResource`, so a new cloud backend is one URL scheme, never a parallel store class.
- Cases: `StoreOp` rows `Put(payload, mode, attributes, tags, chunk_size)` (`obstore.put`/`put_async` carrying `attributes`/`tags`/`mode`/`use_multipart`/`chunk_size`/`max_concurrency` identically, `mode` ∈ `create|overwrite` or an `UpdateVersion` `PutMode` for the conditional write-once put, multipart auto-selected when the payload exceeds `chunk_size`, the threshold an explicit request value defaulting to the 5-MiB `CHUNK` axis rather than the provider default, the receipt keying the payload `bytes` through `ContentIdentity.of` so the same payload to the same path is a `Head`-`e_tag`-confirmed no-op) · `Get(path, options)` (`obstore.get`/`get_async` with the optional `GetOptions` conditional-get/version/range `TypedDict`, then `GetResult.bytes()` zero-copy `Bytes` for the eager read keying the body through `ContentIdentity.of`, the receipt reading `GetResult.meta` for the `e_tag`/`version`/`size` evidence and carrying `GetResult.range`/`GetResult.attributes` on its `payload` slot for the served byte window and object attributes; the large-object streaming get rides the `Reader` case's `open_reader` handle while `GetResult.stream` `BytesStream` is the same-row growth value when a chunked-iterator get folds onto the existing `Get` payload slot rather than an eager `bytes()`) · `GetRange(path, start, end, length)` (`obstore.get_range`/`get_range_async` fetching one byte window, addressed as start+end or start+length — `end` and `length` are mutually exclusive window descriptors on the one case, both legs carrying the same `start`/`end`/`length` keyword shape) · `GetRanges(path, starts, ends, lengths, coalesce)` (`obstore.get_ranges`/`get_ranges_async` carrying `starts`/`ends`/`lengths`/`coalesce` identically, returning `list[Bytes]`, the coalesced multi-chunk fast-path the `gridded/virtual` VirtualiZarr cube reads against archival HDF5/NetCDF/GeoTIFF chunk byte ranges, the window batch addressed by `ends` or `lengths`, `coalesce` merging adjacent windows below the 1-MiB gap default into one request, never a per-chunk round-trip) · `List(prefix, offset, delimiter)` (`obstore.list` `return_arrow=True` streaming the `ObjectMeta` rows as `arro3.core.RecordBatch` with `offset` resuming a prior listing, or `obstore.list_with_delimiter`/`list_with_delimiter_async` returning the `ListResult` `common_prefixes`/`objects` flat listing when `delimiter` is set) · `Head(path)` (`obstore.head`/`head_async` returning one `ObjectMeta` carrying `e_tag`/`size`/`version`/`last_modified`/`path`) · `Delete(paths)` (`obstore.delete`/`delete_async` over `str | Sequence[str]`, the bulk delete absorbing the singular and plural call on the one case) · `Copy(source, target, overwrite)` (`obstore.copy`/`copy_async` server-side over `from_`/`to`, `overwrite=False` the conditional copy-once precondition) · `Rename(source, target, overwrite)` (`obstore.rename`/`rename_async` atomic over `from_`/`to`, `overwrite=False` the conditional rename-once precondition) · `Reader(path, buffer_size, size)` (`obstore.open_reader`/`open_reader_async` returning the buffered `ReadableFile`/`AsyncReadableFile` carried as the receipt value, `buffer_size` the prefetch window and `size` the known object length) · `Writer(path, attributes, tags, buffer_size, max_concurrency)` (`obstore.open_writer`/`open_writer_async` returning the buffered `WritableFile`/`AsyncWritableFile`, `max_concurrency` the multipart fan-out) · `Sign(method, paths, expires_in)` (`obstore.sign`/`sign_async` minting the presigned-URL batch over a `paths` `Sequence`, carried as the receipt value), each binding the exact `obstore` surface that owns it through one `_ROUTE` row pairing the sync member, its `_async` sibling, the one argument plan both legs read, the per-row reader, and the per-row path projector. The `list` arm folds into the table like every other row, its reader summing `arro3.core.RecordBatch.num_rows` over the `return_arrow=True` stream into the byte-length slot rather than reading a payload, and the `reader`/`writer`/`sign` arms fold through readers that carry the obstore handle or the URL batch into the receipt `payload` slot, so no tag escapes the `_ROUTE` fold to a `_list_rows` or a parallel `_HANDLE` branch the `_apply` checks before the table.
- Entry: `ObjectEgress.of` admits a `ResourceRef`, derives the backend from `obstore.store.parse_scheme(ref.root)` rather than re-deriving the scheme inline, and constructs the `ObjectStoreMethods` store exactly once through `obstore.store.from_url(ref.root, config=..., client_options=..., retry_config=..., credential_provider=...)`, freezing the handle on the owner so no per-operation `from_url` re-mints it — the `config` `dict[str, str]` projecting the `S3Config`/`GCSConfig`/`AzureConfig` `TypedDict` keys, `client_options` the `ClientConfig` `TypedDict`, `retry_config` the `RetryConfig` `TypedDict`, and the `credential_provider` callable riding the runtime `TransportResource` for token refresh; `ObjectEgress.run` wraps `_apply` in `boundary("egress.{tag}", ..., catch=BaseError)` and `ObjectEgress.run_async` wraps `_apply_async` in `async_boundary("egress.{tag}", ..., catch=BaseError)` — one typed boundary kernel each, no per-arm `try`/`except` — where `_apply`/`_apply_async` read the one `_ROUTE` row for `op.tag`, project the case payload through the one row `plan` both legs share, call the row `sync` member (or the `aio` member under `await guard(RetryClass.OBJECT_STORE)`) with the identical kwargs against the frozen store, and fold the obstore return through the row `read` so the byte-receipt, listing, and handle-bearing cases reuse one row set under retry through one awaitable fault funnel; no keyword allowlist diverges the async plan from the sync plan because the obstore sync and async members carry identical keyword signatures. The put short-circuits to a by-reference no-op when the payload's `ContentKey` — derived from the payload `bytes` through `ContentIdentity.of`, never the path — matches the prior egress reconciled against the remote `Head` `e_tag`, so an unchanged bundle never re-uploads. One entrypoint pair `run`/`run_async` owns every case, the streaming-handle and signing cases carrying their non-byte value on the receipt `payload` slot rather than forcing a second `handle`/`handle_async` surface.
- Auto: the bundle bytes cross as the zero-copy `obstore.Bytes` buffer protocol, so the Arrow/Parquet/GeoParquet/zarr payload the upstream owner already minted uploads without a Python-side copy; `mode="create"` preconditions a write-once put, `mode="overwrite"` replaces, and an `UpdateVersion` `PutMode` carrying the prior `e_tag`/`version` makes the put conditional, all surfaced as the `Put` mode rather than three methods; the `Put` arm passes `chunk_size` explicitly so the multipart threshold is the request value defaulting to the 5-MiB `CHUNK` axis, not the provider default, and the `GetRanges` arm passes `coalesce` explicitly so the merge distance is the request value, not the 1-MiB provider default, and the window batch addresses by `ends` or `lengths` so an offset-plus-length manifest reads without pre-computing end offsets; `Copy`/`Rename` pass `overwrite` explicitly so the conditional copy-once and rename-once precondition is the request value; `Delete` passes `str | Sequence[str]` so the singular and bulk delete share one obstore call; the list path requests `return_arrow=True` so the listing streams as Arrow `RecordBatch` and the egress sums `num_rows` over the stream rather than materializing a `list[ObjectMeta]`, with `offset` resuming a prior page and the `list_with_delimiter` variant returning the `ListResult` flat common-prefix listing; the content key derives from the operation's real bytes through exactly one canonical `ContentIdentity.of` — `put` payload `bytes`, `get` body `Bytes`, `get_ranges` windows as a child-`ContentKey` Merkle tuple — never re-minted and never path-derived, the `Bytes` zero-copy buffer feeding the `bytes`/`Iterable[bytes]` modality of `ContentIdentity.of` directly. `obstore` is the abi3 cp315-ready wheel and imports module-top, but its `PutResult`/`ObjectMeta`/`GetOptions`/`PutMode`/`GetResult`/`Attributes`/`ObjectStoreMethods` types are typing-only — its own stubs mark them not importable at runtime — so they import under a `TYPE_CHECKING` block while only the operation members, `parse_scheme`, and `Bytes` import at module top.
- Receipt: the egress contributes an emitted-phase `Receipt.of` row through `ReceiptContributor.contribute` and produces an `EgressReceipt` keyed by the operation-bytes `ContentKey` plus the `PutResult`/`ObjectMeta` `e_tag`/`version` `TypedDict` subscripts folded once through a single coalescing read over the meta dict, never a generic receipt and never a path-string key; the `Head` arm reconciles the prior `ObjectMeta` `e_tag` so the no-op put is content-key plus remote-etag confirmed, and the `reader`/`writer`/`sign` arms carry the obstore handle or URL batch on the `payload` slot so a streaming or signing call leaves through the same receipt rail.
- Faults: every raised `obstore.exceptions.BaseError` — `NotFoundError`/`AlreadyExistsError`/`PreconditionError`/`NotModifiedError`/`PermissionDeniedError`/`UnauthenticatedError`/`NotSupportedError`/`InvalidPathError`/`UnknownConfigurationKeyError` — converts exactly once at the `boundary`/`async_boundary` seam through `BoundaryFault.of(subject, cause)` carrying `(subject, type(cause).__name__)`, narrowed by `catch=obstore.exceptions.BaseError` so a non-obstore exception escapes rather than masquerading as an egress fault; recovery keys on `fault.recoverable({"boundary"})` and `type(cause).__name__` membership — `AlreadyExistsError`/`PreconditionError` the conditional write-once and copy-once collisions, `NotModifiedError` the `if_none_match` match, `NotFoundError` the absent get/head, `NotSupportedError` the sign-on-unsupported-backend reject — never a reconstructed message, and the precondition vocabulary stays the one `BoundaryFault` family the whole branch returns through, never a per-package classification table and never an invented HTTP-status integer the `.api` EXCEPTIONS rows model no field for.
- Packages: `obstore` (`store.from_url(url, *, config, client_options, retry_config, credential_provider)`/`store.parse_scheme`/`put`/`put_async`/`get`/`get_async`/`get_range`/`get_range_async`/`get_ranges`/`get_ranges_async`/`list`/`list_with_delimiter`/`list_with_delimiter_async`/`head`/`head_async`/`delete`/`delete_async`/`copy`/`copy_async`/`rename`/`rename_async`/`open_reader`/`open_reader_async`/`open_writer`/`open_writer_async`/`sign`/`sign_async`/`GetResult.bytes`/`GetResult.stream`/`GetResult.meta`/`GetResult.range`/`GetResult.attributes`/`Bytes`/`Bytes.to_bytes`/`BytesStream`/`PutResult`/`ObjectMeta`/`GetOptions`/`PutMode`/`Attributes`/`UpdateVersion`/`ListResult`/`store.ObjectStoreMethods`/`exceptions.BaseError` plus the leaf set), `arro3.core` (`RecordBatch.num_rows` over the `return_arrow=True` list stream), `pyarrow` (the bundle payloads), runtime (`TransportResource`/`ResourceRef`/`RuntimeRail`/`BoundaryFault`/`boundary`/`async_boundary`/`RetryClass`/`guard`/`ContentIdentity`/`ContentKey`/`Receipt`/`ReceiptContributor`).
- Growth: a new store operation is one `StoreOp` case plus one `_ROUTE` row pairing its sync member, `_async` sibling, argument plan, reader, and path projector; a new cloud backend is one `from_url` URL scheme the `parse_scheme` derivation already routes; a new precondition is one `PutMode` payload on the existing `Put` case or one `overwrite` value on the existing `Copy`/`Rename` case; a new conditional-get axis is one `GetOptions` key on the existing `Get` case; a new window-addressing mode is the existing `ends`/`lengths` pair on the `GetRange`/`GetRanges` cases; a new get-response evidence field is one more `GetResult` member on the existing `Get` reader's `payload` tuple, never a second receipt; a new streaming or signing surface is one `StoreOp` case whose reader carries its non-byte value on the receipt `payload` slot, never a parallel `_HANDLE` table; the async fast-path is the existing `run_async` over the same row set; the content-key modality is the existing `_key` fold over `bytes`/`Bytes` or a child-key tuple, never a new identity surface; zero new receipt surface.
- Boundary: composes the runtime `TransportResource`/`ResourceRef` for credentials and endpoint and the runtime `boundary`/`async_boundary`/`BoundaryFault` for fault conversion, never a second transport or fault owner; consumes the egress bundles from `columnar`, `geospatial`, and `tensor` rather than re-minting them; no durable product store, no fsspec re-derivation, no parallel `S3Egress`/`GcsEgress` family, no parallel `AsyncObjectEgress`; a hand-rolled multi-cloud HTTP client, a thin rename wrapper over the `obstore` operations, a per-leg keyword allowlist diverging the async plan from the sync plan, a per-package `_classify` exception table, a bare-boundary `try`/`except` discarding the `obstore` exception class, a `list`-only branch escaping the `_ROUTE` fold, a parallel `_HandleRow`/`_HANDLE` shape beside the one `_Row`/`_ROUTE` owner, a per-arm sync/async method twin, a path-string `ContentIdentity.of` key contradicting the identity owner's no-path law, a per-operation `from_url` re-mint of the store handle, and an `Any`-typed store ignoring the `ObjectStoreMethods` Protocol are the deleted forms.

```python signature
from __future__ import annotations

from collections.abc import Callable, Sequence
from typing import TYPE_CHECKING, Any, Final, Literal

import obstore
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from obstore import Bytes
from obstore.exceptions import BaseError
from obstore.store import from_url, parse_scheme

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guard
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from obstore import Attributes, GetOptions, ObjectMeta, PutMode, PutResult
    from obstore.store import AzureConfig, ClientConfig, GCSConfig, ObjectStoreMethods, RetryConfig, S3Config

type Config = "S3Config | GCSConfig | AzureConfig | dict[str, str]"
type Provider = Callable[[], Any] | None
type Method = Literal["GET", "PUT", "DELETE"]
type Meta = "ObjectMeta | PutResult | None"
type Call = tuple[tuple[Any, ...], dict[str, Any]]
type Read = tuple[int, Meta, ContentKey, Any]

COALESCE: Final[int] = 1 << 20
CHUNK: Final[int] = 5 << 20
FANOUT: Final[int] = 12


@tagged_union(frozen=True)
class StoreOp:
    tag: Literal["put", "get", "get_range", "get_ranges", "list", "head", "delete", "copy", "rename", "reader", "writer", "sign"] = tag()
    put: tuple[bytes, "PutMode", "Attributes", dict[str, str], int] = case()
    get: tuple[str, "GetOptions | None"] = case()
    get_range: tuple[str, int, int | None, int | None] = case()
    get_ranges: tuple[str, tuple[int, ...], tuple[int, ...] | None, tuple[int, ...] | None, int] = case()
    list: tuple[str, int, bool] = case()
    head: str = case()
    delete: str | Sequence[str] = case()
    copy: tuple[str, str, bool] = case()
    rename: tuple[str, str, bool] = case()
    reader: tuple[str, int | None, int | None] = case()
    writer: tuple[str, "Attributes", dict[str, str], int | None, int] = case()
    sign: tuple[Method, Sequence[str], int] = case()

    @staticmethod
    def Put(payload: bytes, mode: "PutMode" = "overwrite", attributes: "Attributes | None" = None, tags: dict[str, str] | None = None, chunk_size: int = CHUNK) -> StoreOp:
        return StoreOp(put=(payload, mode, attributes or {}, tags or {}, chunk_size))

    @staticmethod
    def Get(path: str, options: "GetOptions | None" = None) -> StoreOp:
        return StoreOp(get=(path, options))

    @staticmethod
    def GetRange(path: str, start: int, end: int | None = None, length: int | None = None) -> StoreOp:
        return StoreOp(get_range=(path, start, end, length))

    @staticmethod
    def GetRanges(path: str, starts: tuple[int, ...], ends: tuple[int, ...] | None = None, lengths: tuple[int, ...] | None = None, coalesce: int = COALESCE) -> StoreOp:
        return StoreOp(get_ranges=(path, starts, ends, lengths, coalesce))

    @staticmethod
    def List(prefix: str, offset: int = 0, delimiter: bool = False) -> StoreOp:
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
    def Writer(path: str, attributes: "Attributes | None" = None, tags: dict[str, str] | None = None, buffer_size: int | None = None, max_concurrency: int = FANOUT) -> StoreOp:
        return StoreOp(writer=(path, attributes or {}, tags or {}, buffer_size, max_concurrency))

    @staticmethod
    def Sign(method: Method, paths: Sequence[str], expires_in: int) -> StoreOp:
        return StoreOp(sign=(method, paths, expires_in))


class _Row(Struct, frozen=True):
    sync: Callable[..., Any]
    aio: Callable[..., Any]
    plan: Callable[[StoreOp, str], Call]
    read: Callable[[StoreOp, str, Any], Read]
    path: Callable[[StoreOp, str], str]


def _key(fmt: str, source: bytes | Sequence[Bytes]) -> ContentKey:
    return ContentIdentity.of(fmt, source if isinstance(source, bytes) else tuple(_key(fmt, bytes(w)) for w in source))


def _rows(stream: Any) -> int:
    return sum(batch.num_rows for batch in stream)


def _listing(store: ObjectStoreMethods, prefix: str, *, offset: int | None, delimiter: bool) -> int:
    return _rows(obstore.list_with_delimiter(store, prefix, return_arrow=True)["objects"] if delimiter
                 else obstore.list(store, prefix, offset=offset, return_arrow=True))


async def _listing_async(store: ObjectStoreMethods, prefix: str, *, offset: int | None, delimiter: bool) -> int:
    if delimiter:
        return _rows((await obstore.list_with_delimiter_async(store, prefix, return_arrow=True))["objects"])
    return sum([batch.num_rows async for batch in obstore.list(store, prefix, offset=offset, return_arrow=True)])


_ROUTE: Final[Map[str, _Row]] = Map.of_seq([
    ("put", _Row(obstore.put, obstore.put_async,
        lambda op, t: ((t, op.put[0]), {"mode": op.put[1], "attributes": op.put[2], "tags": op.put[3], "chunk_size": op.put[4]}),
        lambda op, t, result: (len(op.put[0]), result, _key(f"egress.put.{parse_scheme(t)}", op.put[0]), None),
        lambda op, t: t)),
    ("get", _Row(obstore.get, obstore.get_async,
        lambda op, t: ((op.get[0],), {"options": op.get[1]}),
        lambda op, t, result: (len(body := result.bytes()), result.meta, _key("egress.get", bytes(body)), (result.range, result.attributes)),
        lambda op, t: op.get[0])),
    ("get_range", _Row(obstore.get_range, obstore.get_range_async,
        lambda op, t: ((op.get_range[0],), {"start": op.get_range[1], "end": op.get_range[2], "length": op.get_range[3]}),
        lambda op, t, window: (len(window), None, _key("egress.get_range", bytes(window)), None),
        lambda op, t: op.get_range[0])),
    ("get_ranges", _Row(obstore.get_ranges, obstore.get_ranges_async,
        lambda op, t: ((op.get_ranges[0],), {"starts": op.get_ranges[1], "ends": op.get_ranges[2], "lengths": op.get_ranges[3], "coalesce": op.get_ranges[4]}),
        lambda op, t, windows: (sum(len(w) for w in windows), None, _key("egress.get_ranges", windows), None),
        lambda op, t: op.get_ranges[0])),
    ("list", _Row(_listing, _listing_async,
        lambda op, t: ((op.list[0],), {"offset": op.list[1] or None, "delimiter": op.list[2]}),
        lambda op, t, rows: (rows, None, _key("egress.list", str(rows).encode()), None),
        lambda op, t: op.list[0])),
    ("head", _Row(obstore.head, obstore.head_async,
        lambda op, t: ((op.head,), {}),
        lambda op, t, meta: (meta["size"], meta, _key("egress.head", str(meta.get("e_tag") or "").encode()), None),
        lambda op, t: op.head)),
    ("delete", _Row(obstore.delete, obstore.delete_async,
        lambda op, t: ((op.delete,), {}),
        lambda op, t, _: (0, None, _key("egress.delete", b""), None),
        lambda op, t: op.delete if isinstance(op.delete, str) else ",".join(op.delete))),
    ("copy", _Row(obstore.copy, obstore.copy_async,
        lambda op, t: ((op.copy[0], op.copy[1]), {"overwrite": op.copy[2]}),
        lambda op, t, _: (0, None, _key("egress.copy", op.copy[1].encode()), None),
        lambda op, t: op.copy[1])),
    ("rename", _Row(obstore.rename, obstore.rename_async,
        lambda op, t: ((op.rename[0], op.rename[1]), {"overwrite": op.rename[2]}),
        lambda op, t, _: (0, None, _key("egress.rename", op.rename[1].encode()), None),
        lambda op, t: op.rename[1])),
    ("reader", _Row(obstore.open_reader, obstore.open_reader_async,
        lambda op, t: ((op.reader[0],), {k: v for k, v in (("buffer_size", op.reader[1]), ("size", op.reader[2])) if v is not None}),
        lambda op, t, file: (0, None, _key("egress.reader", op.reader[0].encode()), file),
        lambda op, t: op.reader[0])),
    ("writer", _Row(obstore.open_writer, obstore.open_writer_async,
        lambda op, t: ((op.writer[0],), {k: v for k, v in (("attributes", op.writer[1]), ("tags", op.writer[2]), ("buffer_size", op.writer[3]), ("max_concurrency", op.writer[4])) if v is not None}),
        lambda op, t, file: (0, None, _key("egress.writer", op.writer[0].encode()), file),
        lambda op, t: op.writer[0])),
    ("sign", _Row(obstore.sign, obstore.sign_async,
        lambda op, t: ((op.sign[0], op.sign[1], op.sign[2]), {}),
        lambda op, t, urls: (len(op.sign[1]), None, _key("egress.sign", "".join(op.sign[1]).encode()), urls),
        lambda op, t: op.sign[1][0] if op.sign[1] else "")),
])


class EgressReceipt(Struct, frozen=True):
    operation: str
    path: str
    byte_length: int
    e_tag: str
    version: str
    content_key: ContentKey
    payload: Any = None

    @classmethod
    def of(cls, operation: str, path: str, length: int, meta: Meta, content_key: ContentKey, payload: Any) -> EgressReceipt:
        read = lambda field: str((meta or {}).get(field) or "")
        return cls(operation=operation, path=path, byte_length=length, e_tag=read("e_tag"), version=read("version"), content_key=content_key, payload=payload)

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "object-egress", self.path,
            {"op": self.operation, "bytes": str(self.byte_length), "etag": self.e_tag, "version": self.version, "key": self.content_key.hex})


class ObjectEgress(Struct, frozen=True):
    ref: ResourceRef
    store: ObjectStoreMethods

    @classmethod
    def of(cls, ref: ResourceRef, config: Config | None = None, client_options: "ClientConfig | None" = None, retry_config: "RetryConfig | None" = None, credential_provider: Provider = None) -> ObjectEgress:
        return cls(ref=ref, store=from_url(ref.root, config=config or {}, client_options=client_options, retry_config=retry_config, credential_provider=credential_provider))

    def run(self, op: StoreOp, path: str = "") -> RuntimeRail[EgressReceipt]:
        return boundary(f"egress.{op.tag}", lambda: self._apply(op, path or self.ref.relative), catch=BaseError)

    async def run_async(self, op: StoreOp, path: str = "") -> RuntimeRail[EgressReceipt]:
        return await async_boundary(f"egress.{op.tag}", lambda: self._apply_async(op, path or self.ref.relative), catch=BaseError)

    def _receipt(self, op: StoreOp, target: str, returned: Any) -> EgressReceipt:
        row = _ROUTE[op.tag]
        length, meta, content_key, payload = row.read(op, target, returned)
        return EgressReceipt.of(op.tag, row.path(op, target), length, meta, content_key, payload)

    def _apply(self, op: StoreOp, target: str) -> EgressReceipt:
        args, kwargs = _ROUTE[op.tag].plan(op, target)
        return self._receipt(op, target, _ROUTE[op.tag].sync(self.store, *args, **kwargs))

    async def _apply_async(self, op: StoreOp, target: str) -> EgressReceipt:
        args, kwargs = _ROUTE[op.tag].plan(op, target)
        return self._receipt(op, target, await guard(RetryClass.OBJECT_STORE)(_ROUTE[op.tag].aio, self.store, *args, **kwargs))
```

## [03]-[RESEARCH]

- [OBSTORE_ROUTE_SURFACE]: every `_ROUTE` member — `store.from_url`/`store.parse_scheme`/`put`/`put_async`/`get`/`get_async`/`get_range`/`get_range_async`/`get_ranges`/`get_ranges_async`/`list`/`list_with_delimiter`/`list_with_delimiter_async`/`head`/`head_async`/`delete`/`delete_async`/`copy`/`copy_async`/`rename`/`rename_async`/`open_reader`/`open_reader_async`/`open_writer`/`open_writer_async`/`sign`/`sign_async` — is catalogue-confirmed against the substrate `runtime/.api/obstore.md` (object operations [01]-[09], listing [01]-[02], streaming IO [01]-[02], store construction [01], [06] `parse_scheme`) reflection-confirmed on the cp315 abi3 core, the same catalogue `transport/roots#RESOURCE` settles its `obstore` arms against. The store handle types as the `store.ObjectStoreMethods` structural Protocol every store class satisfies (PUBLIC_TYPES store classes [07]), constructed once at `of` and frozen on the owner so no per-operation `from_url` re-mint exists, the backend derived from `store.parse_scheme(ref.root)` rather than an inline scheme split. The `Get` reader reads `GetResult.bytes()` for the body, `GetResult.meta` for the `e_tag`/`version`/`size` evidence, and `GetResult.range`/`GetResult.attributes` for the served byte window and object attributes onto the receipt `payload` slot, while `GetResult.stream` `BytesStream` is the large-object streaming value the same `Get` row carries (PUBLIC_TYPES result types [02] enumerates `.meta`/`.range`/`.attributes`/`.bytes()`/`.stream`/`.buffer`); the content key derives from the operation's real bytes through `ContentIdentity.of` over the `Bytes`/`bytes` whole-payload modality (`put`/`get`/`get_range`) or the child-`ContentKey` Merkle tuple over the `get_ranges` `list[Bytes]` windows, the `Bytes.to_bytes()` zero-copy wrapper feeding the digest, never the path the `evidence/identity#IDENTITY` owner forbids as a key source. The catalogue `_async`-identity law (object-operations scope: each operation has a matching `_async` variant with identical signature) settles every `_async`/`*_async` sibling — `put_async` carries `attributes`/`tags` identically to `put`, `get_ranges_async` carries `lengths` identically to `get_ranges`, `sign_async`/`open_reader_async`/`open_writer_async`/`list_with_delimiter_async` mirror their sync rows — so one kwargs plan serves both legs and no per-leg keyword allowlist exists: `put`/`put_async` carry `attributes`/`tags`/`mode`/`use_multipart`/`chunk_size`/`max_concurrency` (`chunk_size` 5-MiB default, `max_concurrency` 12 default); `get_range`/`get_range_async` carry `start`/`end`/`length` with `end` and `length` mutually exclusive window descriptors; `get_ranges`/`get_ranges_async` carry `starts`/`ends`/`lengths`/`coalesce` (1-MiB default) so the window batch addresses by `ends` or `lengths`; `copy`/`copy_async`/`rename`/`rename_async` carry `overwrite=True` (object operations [05]-[06]) as the conditional precondition; `delete`/`delete_async` accept `paths` as `str | Sequence[str]` so the bulk delete absorbs the singular and plural call; `open_reader`/`open_reader_async` carry `buffer_size`/`size` and `open_writer`/`open_writer_async` carry `attributes`/`tags`/`buffer_size`/`max_concurrency` (streaming IO [01]-[02]); `sign`/`sign_async(store, method, paths, expires_in)` mints the presigned-URL batch (object operations [09]); `store.from_url(url, *, config, client_options, retry_config, credential_provider, **kwargs)` carries `credential_provider` (store construction [01]). The `list` member works under both `for` and `async for` over one `ListStream` (no `list_async`), so the row pairs `_listing`/`_listing_async` summing `arro3.core.RecordBatch.num_rows` over the `return_arrow=True` stream or the `list_with_delimiter` `ListResult` `objects` row-count, folding the listing case into the table rather than a `_list_rows` branch. `PutResult` `e_tag`/`version`, `ObjectMeta` `path`/`last_modified`/`size`/`e_tag`/`version`, and `GetResult.meta` are `TypedDict`/result subscripts the receipt reads directly (`meta["size"]`/`meta.get("e_tag")`/`result.meta`); `PutResult`/`ObjectMeta`/`GetOptions`/`PutMode`/`GetResult`/`Attributes`/`ObjectStoreMethods` are typing-only names the `obstore` stubs mark not importable at runtime, so they import under `TYPE_CHECKING` while only the operation members, `store.parse_scheme`, and `Bytes` import at module top. `GetResult.bytes()` returns a `len`-able `Bytes` and `GetResult.stream` returns a `BytesStream` sync/async chunk iterator (PUBLIC_TYPES [02]); dispatch over a tagged-union `case` is `expression` structural `match` over the `tag` field, not a generated `Switch` method, so the receipt path and the case projections fold onto the one `_ROUTE` row keyed by `op.tag` rather than an enumerated `match`. [RESEARCH]: the catalogue states `GetOptions` is the conditional-get/range/version `TypedDict` but enumerates no field names; the `if_match`/`if_none_match`/`if_modified_since`/`range`/`version` keys the `Get` card names stay marked RESEARCH until the catalogue PUBLIC_TYPES row enumerates the `GetOptions` field set.
- [OBSTORE_FAULT_CONVERSION]: the `obstore.exceptions` taxonomy — `BaseError` root with `NotFoundError`/`AlreadyExistsError`/`PermissionDeniedError`/`UnauthenticatedError`/`PreconditionError`/`NotModifiedError`/`NotSupportedError`/`InvalidPathError`/`UnknownConfigurationKeyError`/`GenericError`/`JoinError` leaves — is catalogue-confirmed (`runtime/.api/obstore.md` PUBLIC_TYPES exceptions [01]-[12]) and `from obstore.exceptions import BaseError` is import-confirmed against the live distribution; the egress lifts the live cause through the runtime `reliability/faults#FAULT` `boundary`/`async_boundary` exactly once with `catch=BaseError` narrowing the funnel to the obstore root, the conversion landing on `BoundaryFault.of(subject, cause)` (the `boundary` case carrying `(subject, type(cause).__name__)`), so a precondition collision is recoverable through `fault.recoverable({"boundary"})` and `type(cause).__name__` membership rather than a reconstructed message — no per-package `_classify` table and no invented HTTP-status integer, since the EXCEPTIONS rows model no numeric status. `NotFoundError` (absent get/head) and `NotSupportedError` (sign on a backend lacking presign support, raised per IMPLEMENTATION_LAW) are catalogued leaves and ride the same funnel, so the recovery vocabulary keys on their `type(cause).__name__` like every other leaf.
- [OBSTORE_CREDENTIAL_CONFIG]: the `config` value `ObjectEgress.of` admits is the union of the `S3Config`/`GCSConfig`/`AzureConfig` `TypedDict` shapes in `obstore.store` with a plain `dict[str, str]` projection, `client_options` the `ClientConfig` `TypedDict` and `retry_config` the `RetryConfig` `TypedDict` (`runtime/.api/obstore.md` IMPLEMENTATION_LAW config types); the `credential_provider` callable rides the runtime `TransportResource` for token refresh and threads through `from_url(url, *, config, client_options, retry_config, credential_provider, **kwargs)` (store construction [01]) to the per-scheme store, catalogue-confirmed on the abi3 core.
- [OBSTORE_STREAM_AND_SIGN]: the buffered large-bundle streaming handles — `obstore.open_reader(store, path, *, buffer_size, size)`/`open_reader_async` returning `ReadableFile`/`AsyncReadableFile` and `obstore.open_writer(store, path, *, attributes, buffer_size, tags, max_concurrency)`/`open_writer_async` returning `WritableFile`/`AsyncWritableFile` (streaming IO [01]-[02]) — and the presigned-URL mint `obstore.sign(store, method, paths, expires_in)`/`sign_async` (object operations [09]) fold into the one `StoreOp` axis as the `reader`/`writer`/`sign` cases, their `_ROUTE` readers carrying the obstore handle or URL batch on the `EgressReceipt.payload` slot rather than a parallel `StoreHandle` union or a second `handle`/`handle_async` surface; `open_reader`/`open_writer` own large-object streaming and `sign` requires a backend supporting presign (else `NotSupportedError`), both catalogue-confirmed on the abi3 core.
