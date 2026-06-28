# [PY_DATA_EGRESS]

The native object-store egress owner: one `ObjectEgress` façade over `obstore` for the Arrow/Parquet/GeoParquet/zarr bundles the `columnar`, `geospatial`, and `tensor` owners emit, keyed by runtime `ContentIdentity` over the operation's real bytes. `ObjectEgress` discriminates the `StoreOp` tagged-union axis — put/get/get_range/get_ranges/list/head/delete/copy/rename plus the handle-bearing reader/writer/sign cases — over the highest-throughput native `object_store` path against one `ObjectStore` store handle constructed once at `of` through `from_url`, and routes every case through one `_ROUTE` data table pairing each tag with its sync member, its `_async` sibling, one argument plan both legs read, one `read` projecting the obstore return to a byte length plus the `ObjectMeta`/`PutResult` carried plus the digest `Source` the bytes derive, so one `_apply` fold serves the synchronous `run` and one `_apply_async` fold serves the awaitable `run_async`. The `_async` sibling is the same row read under a second entrypoint over the identical kwargs plan because the obstore sync and async members carry identical keyword signatures (catalogue `_async`-identity law), never a parallel `AsyncStoreOp` family, never a per-leg keyword allowlist, and never two enumerated matches; dispatch is keyed by `op.tag` into the row rather than an `expression` structural `match`, so the case projection, identity `Source`, and the handle/sign args all fold onto the one row table and no tag escapes to a hand-written arm. The `list` arm folds into the same row through a `read` that counts the `return_arrow=True` listing rather than a payload-byte count — summing `arro3.core.RecordBatch.num_rows` over the recursive `ListStream` chunks, or reading the flat `arro3.core.Table.num_rows` of the `list_with_delimiter` `objects` table directly (iterating that `Table` yields its `ChunkedArray` columns, not row batches, so the count is the table property, never a per-element sum) — and the handle-bearing `reader`/`writer`/`sign` arms fold through a `read` that returns the obstore handle or the presigned-URL batch as the receipt's carried value, so the byte-receipt, row-count, and handle cases all stay in the one table. The content key derives from the operation's real bytes through exactly one `ContentIdentity.of` in `_receipt` — the `put` payload `bytes` and `get` body `Bytes` keying the `whole`-buffer modality through `IdentitySource.lift`'s `Buffer()` arm, the `get_ranges` `tuple[Bytes, ...]` windows keying the `stream` modality through the `Iterable()` arm (a tuple of buffer-protocol `Bytes` is neither a single `Buffer` nor a `tuple[ContentKey, ...]`, so it lands on `stream`, never the `merkle` arm the egress emits no `ContentKey` spine for) — fed the zero-copy `Bytes` buffer with no `bytes()` copy, scheme-scoped through `ResourceRef.scheme`, never from the path or filename the identity owner forbids as a key source, so an unchanged content-key is a put no-op by reference confirmed against the prior `Head` `e_tag` rather than a path-string hash. `ContentIdentity.of` returns a `RuntimeRail[ContentKey]` the `_receipt` `.map` threads onto the railed `EgressReceipt`, so `run`/`run_async` flatten `.bind(lambda rail: rail)` the identity-failure path through the same carrier the obstore-failure path returns. Each `run`/`run_async` opens one `_TRACER.start_as_current_span("egress.{tag}")` parent span carrying `rasm.egress.scheme`, then delegates the whole retried-traced-railed acquisition to the matching `reliability/resilience#RESILIENCE` fused envelope over the one `RetryClass.OBJECT_STORE` row — the awaitable leg to `guarded(RetryClass.OBJECT_STORE, _apply_async, op, target, subject="egress.{tag}")` and the synchronous leg to `guarded_sync(RetryClass.OBJECT_STORE, _apply, op, target, subject="egress.{tag}")`, the two runtime arms of the same row (`AsyncRetryingCaller`/`RetryingCaller`). Each fused entrypoint drives the member-cached `stamina` caller, opens the child `resilience.guarded` retry span beneath the egress parent, and lifts the terminal raise through the `reliability/faults#FAULT` `async_boundary`/`boundary` exactly once — exactly as the sibling `transport/roots#RESOURCE` object-store reader delegates its async leg and the `data:tabular/lakehouse#LAKEHOUSE` `Lakehouse.run` its synchronous commit legs, never a fetch-shaped leg re-spelling a bare `guard(cls)`/`guard_sync(cls)` caller inside its own hand-opened `async_boundary`/`boundary` and span (the doubled-span/doubled-lift form the resilience owner names a deleted shape, distinct from the lanes `retried` admission row that alone binds bare `guard`), and never a sync leg dropping the `OBJECT_STORE` outer envelope to lean on the `obstore` Rust-core `RetryConfig` alone where the async leg carries it — both legs ride the same stamina row over the store's inner `RetryConfig`. `BoundaryFault.of(subject, cause)` lands the raised `obstore.exceptions.*` leaf on the `boundary` catch-all carrying `(subject, str(cause) or type(cause).__name__)` from the `NotFoundError`/`AlreadyExistsError`/`PreconditionError`/`NotModifiedError`/`PermissionDeniedError`/`UnauthenticatedError`/`NotSupportedError`/`InvalidPathError`/`UnknownConfigurationKeyError`/`GenericError`/`JoinError` leaf set — the `CLASSIFY` fold catches `Exception` at the one seam so a row-projection `KeyError`/`AttributeError` also converts to `boundary` rather than escaping the egress — so recovery keys on `fault.recoverable({"boundary"})`, never a hand-rolled per-arm `try`/`except` and never a per-package classification table.

## [01]-[INDEX]

- [01]-[EGRESS]: the native object-store egress owner over one `StoreOp` axis, table-routed sync and async through one `_ROUTE` fold over one `_Row` shape, composing the runtime transport, the `RetryClass.OBJECT_STORE` `guarded`/`guarded_sync` resilience envelopes (the `BoundaryFault` lift their `async_boundary`/`boundary` cores own), the one `ContentIdentity` identity rail, and the one `_TRACER` egress span.

## [02]-[EGRESS]

- Owner: `ObjectEgress` — the one object-store egress façade over `obstore`; `StoreOp` the tagged-union operation axis (put/get/get_range/get_ranges/list/head/delete/copy/rename/reader/writer/sign), routed through one `_ROUTE` data table over one `_Row` shape so a new store operation is one `StoreOp` case plus one `_ROUTE` row, never a `put_object`/`get_object`/`list_objects` method family, never an async method twin, and never a second `_HandleRow` shape beside the byte-receipt rows. `EgressReceipt` is the typed egress receipt — operation, path, byte length, e-tag, version, content-key, and the carried handle/URL value — its `contribute` yielding the `Iterable[Receipt]` the runtime `ReceiptContributor` Protocol declares, never a single bare `Receipt`. The store backend is the one `ObjectStore` handle `obstore.store.from_url(ref.root, ...)` constructs from the `ResourceRef` root URL (`s3://`/`gs://`/`az://`/`file:///`/`memory:///`/`http(s)://`) — `from_url(...) -> ObjectStore` returns the canonical store-handle Union the sibling `transport/roots#RESOURCE` `_object_store(root) -> ObjectStore` cache also carries, the one store-handle name both obstore-composing owners share — the `config` `S3Config | GCSConfig | AzureConfig` `TypedDict` union riding the credential-bearing `TransportResource`, so a new cloud backend is one URL scheme, never a parallel store class.
- Cases: `StoreOp` rows `Put(payload, mode, attributes, tags, chunk_size)` (`obstore.put`/`put_async` carrying `attributes`/`tags`/`mode`/`use_multipart`/`chunk_size`/`max_concurrency` identically, `mode` ∈ `create|overwrite` or an `UpdateVersion` `PutMode` for the conditional write-once put, multipart auto-selected when the payload exceeds `chunk_size`, the threshold an explicit request value defaulting to the 5-MiB `CHUNK` axis rather than the provider default, `_receipt` keying the payload `bytes` through `ContentIdentity.of` so the same payload to the same path is a `Head`-`e_tag`-confirmed no-op) · `Get(path, options)` (`obstore.get`/`get_async` with the optional `GetOptions` conditional-get/version/range `TypedDict` keying `if_match`/`if_none_match`/`if_modified_since`/`if_unmodified_since`/`range`/`version`/`head`, then `GetResult.bytes()` zero-copy `Bytes` for the eager read keyed through `ContentIdentity.of` directly as the whole-buffer `Source`, the receipt reading `GetResult.meta` for the `e_tag`/`version`/`size` evidence and carrying `GetResult.range`/`GetResult.attributes` on its `payload` slot for the served byte window and object attributes; the large-object streaming get rides the `Reader` case's `open_reader` handle while `GetResult.stream` `BytesStream` is the same-row growth value when a chunked-iterator get folds onto the existing `Get` payload slot rather than an eager `bytes()`) · `GetRange(path, start, end, length)` (`obstore.get_range`/`get_range_async` fetching one byte window, addressed as start+end or start+length — `end` and `length` are mutually exclusive window descriptors on the one case, both legs carrying the same `start`/`end`/`length` keyword shape) · `GetRanges(path, starts, ends, lengths, coalesce)` (`obstore.get_ranges`/`get_ranges_async` carrying `starts`/`ends`/`lengths`/`coalesce` identically, returning `list[Bytes]`, the coalesced multi-chunk fast-path the `gridded/virtual` VirtualiZarr cube reads against archival HDF5/NetCDF/GeoTIFF chunk byte ranges, the window batch addressed by `ends` or `lengths`, `coalesce` merging adjacent windows below the 1-MiB gap default into one request, never a per-chunk round-trip; the windows fold onto `ContentIdentity.of` as the `tuple[Bytes, ...]` `Source` the `IdentitySource.lift` discriminator keys as a chunk stream, never a hand-rolled per-window digest) · `List(prefix, offset, delimiter)` (`obstore.list` `return_arrow=True` streaming the `ObjectMeta` rows as `arro3.core.RecordBatch` with `offset` the last-seen object-path cursor resuming a prior listing lexicographically after that key — a `str | None`, never an integer page index obstore's `offset` is not — or `obstore.list_with_delimiter`/`list_with_delimiter_async` returning the `ListResult` `common_prefixes`/`objects` flat listing when `delimiter` is set) · `Head(path)` (`obstore.head`/`head_async` returning one `ObjectMeta` carrying `e_tag`/`size`/`version`/`last_modified`/`path`) · `Delete(paths)` (`obstore.delete`/`delete_async` over `str | Sequence[str]`, the bulk delete absorbing the singular and plural call on the one case) · `Copy(source, target, overwrite)` (`obstore.copy`/`copy_async` server-side over `from_`/`to`, `overwrite=False` the conditional copy-once precondition) · `Rename(source, target, overwrite)` (`obstore.rename`/`rename_async` atomic over `from_`/`to`, `overwrite=False` the conditional rename-once precondition) · `Reader(path, buffer_size, size)` (`obstore.open_reader`/`open_reader_async` returning the buffered `ReadableFile`/`AsyncReadableFile` carried as the receipt value, `buffer_size` the prefetch window and `size` the known object length) · `Writer(path, attributes, tags, buffer_size, max_concurrency)` (`obstore.open_writer`/`open_writer_async` returning the buffered `WritableFile`/`AsyncWritableFile`, `max_concurrency` the multipart fan-out) · `Sign(method, paths, expires_in)` (`obstore.sign`/`sign_async(store, method, paths, expires_in)` minting the presigned-URL batch over a `paths` `Sequence`, `method` the full `HTTP_METHOD` literal and `expires_in` a `datetime.timedelta` window, valid only on the `SignCapableStore` union — `S3Store | GCSStore | AzureStore` — so a sign on a `LocalStore`/`MemoryStore`/`HTTPStore` is the `NotSupportedError` the `boundary` converts, carried as the receipt value), each binding the exact `obstore` surface that owns it through one `_ROUTE` row pairing the sync member, its `_async` sibling, the one argument plan both legs read, the per-row `read`, and the per-row path projector. The `list` arm folds into the table like every other row, its `read` counting the `return_arrow=True` listing into the byte-length slot rather than reading a payload — summing `arro3.core.RecordBatch.num_rows` over the recursive `ListStream` chunks, or reading the flat `arro3.core.Table.num_rows` of the `list_with_delimiter` `objects` table directly since iterating that `Table` yields its `ChunkedArray` columns rather than row batches — and the `reader`/`writer`/`sign` arms fold through `read`s that carry the obstore handle or the URL batch into the receipt `payload` slot, so no tag escapes the `_ROUTE` fold to a `_list_rows` or a parallel `_HANDLE` branch the `_apply` checks before the table.
- Entry: `ObjectEgress.of` admits a `ResourceRef` and constructs the `ObjectStore` handle exactly once through `obstore.store.from_url(ref.root, config=..., client_options=..., retry_config=..., credential_provider=...)` — `from_url(...) -> ObjectStore` dispatches the backend off the `ref.root` URL scheme internally and returns the canonical store-handle Union, so no inline scheme split or per-call `parse_scheme` is needed — freezing the handle on the owner so no per-operation `from_url` re-mints it; the `config` is the `S3Config | GCSConfig | AzureConfig` `TypedDict` union (or `None`) `from_url` accepts directly, `client_options` the `ClientConfig` `TypedDict`, `retry_config` the `RetryConfig` `TypedDict`, and the `credential_provider` callable riding the runtime `TransportResource` for token refresh. `ObjectEgress.run` opens `_TRACER.start_as_current_span("egress.{tag}")` then delegates `_apply` wholesale to `guarded_sync(RetryClass.OBJECT_STORE, _apply, op, target, subject="egress.{tag}").bind(lambda rail: rail)`, while `ObjectEgress.run_async` opens the same parent span then delegates `_apply_async` wholesale to `guarded(RetryClass.OBJECT_STORE, _apply_async, op, target, subject="egress.{tag}")` — the two runtime arms of the one `reliability/resilience#RESILIENCE` fused envelope over the same `RetryClass.OBJECT_STORE` row, each owning the per-class `stamina` retry (`RetryingCaller`/`AsyncRetryingCaller`), the child `resilience.guarded` span nested beneath the egress parent, and the one-shot `reliability/faults#FAULT` `boundary`/`async_boundary` terminal-fault lift — self-flattened the same `.bind(lambda rail: rail)` way, the sibling `transport/roots#RESOURCE` object-store-reader async delegation and the `data:tabular/lakehouse#LAKEHOUSE` synchronous-commit delegation rather than a re-spelled bare `guard(cls)`/`guard_sync(cls)` caller inside a hand-opened span and `boundary`/`async_boundary`. Both legs read the one `_ROUTE` row for `op.tag`, project the case payload through the one row `plan` both legs share, call the row `sync` member (under `guarded_sync`) or `aio` member (under `guarded`) with the identical kwargs against the frozen store, and fold the obstore return through `_receipt` so the byte-receipt, listing, and handle-bearing cases reuse one row set; the `.bind(lambda rail: rail)` flatten lets `_receipt`'s `RuntimeRail[EgressReceipt]` (the `ContentIdentity.of` identity-failure path) share the one carrier the obstore-failure path returns. No keyword allowlist diverges the async plan from the sync plan because the obstore sync and async members carry identical keyword signatures (`put_async`/`get_ranges_async`/`open_writer_async` reflection-confirmed to carry `attributes`/`tags`/`lengths` identically to their sync siblings on the 0.10.1 distribution). The put short-circuits to a by-reference no-op when the payload's `ContentKey` — derived from the payload `bytes` through `ContentIdentity.of`, never the path — matches the prior egress reconciled against the remote `Head` `e_tag`, so an unchanged bundle never re-uploads. One entrypoint pair `run`/`run_async` owns every case, the streaming-handle and signing cases carrying their non-byte value on the receipt `payload` slot rather than forcing a second `handle`/`handle_async` surface.
- Receipt: `EgressReceipt.contribute` yields one emitted-phase row through `Receipt.of("object-egress", ("emitted", path, facts))` — the runtime `Receipt.of(owner, evidence)` two-argument form over the `(phase, subject, facts)` evidence tuple, never a four-positional `Receipt.of(phase, owner, subject, facts)` — satisfying the `ReceiptContributor.contribute -> Iterable[Receipt]` Protocol, the `byte_length` riding as a native `int` since the receipts `Encoder(enc_hook=repr)` serializes scalars without a `str()` coerce. A byte-bearing op's receipt is keyed by the operation-bytes `ContentKey`; a metadata-/control-plane op (`list`/`head`/`delete`/`copy`/`rename`/`reader`/`writer`/`sign`) carries `content_key=None` because it moves no operation bytes, so the receipt never digests a path string or a server-opaque `e_tag` to manufacture a key, the `content_key: ContentKey | None` slot making the absence explicit rather than the identity owner's no-path / operation-bytes law broken. The `PutResult`/`ObjectMeta` `e_tag`/`version` `TypedDict` subscripts fold once through a single coalescing read over the meta dict; the `Head` arm carries its remote `e_tag`/`version`/`size` evidence on the receipt's typed slots and emits a `None` `Source` (a metadata-only op moves no operation bytes), so the no-op put is confirmed by reading that typed `e_tag` slot against the payload `ContentKey` rather than by a redundant content key over the server-opaque `str(e_tag)`, and the `reader`/`writer`/`sign` arms carry the obstore handle or URL batch on the `payload` slot so a streaming or signing call leaves through the same receipt rail.
- Faults: the async leg lifts the terminal raise through the `guarded` envelope's `reliability/faults#FAULT` `async_boundary` and the sync leg through the `guarded_sync` envelope's synchronous `boundary`, both at the `Exception` catch-all the `CLASSIFY` fold owns — never a per-arm `try`/`except`, never a narrowed `catch=obstore.exceptions.BaseError` that would let a `KeyError`/`AttributeError` from the row projection escape the egress rather than convert. Every raised `obstore.exceptions.BaseError` — `NotFoundError`/`AlreadyExistsError`/`PreconditionError`/`NotModifiedError`/`PermissionDeniedError`/`UnauthenticatedError`/`NotSupportedError`/`InvalidPathError`/`UnknownConfigurationKeyError`/`GenericError`/`JoinError` — is not a `BoundaryFault.CLASSIFY` row, so every leaf lands on the `boundary` catch-all carrying `(subject, str(cause) or type(cause).__name__)` — the obstore message first, the class name only on a message-less raise — and recovery keys on `fault.recoverable({"boundary"})`: `AlreadyExistsError`/`PreconditionError` the conditional write-once and copy-once collisions, `NotModifiedError` the `if_none_match` match, `NotFoundError` the absent get/head, `NotSupportedError` the sign-on-unsupported-backend reject. A `NotFoundError`/`AlreadyExistsError`/`PreconditionError` retried by the `OBJECT_STORE` row's `BaseError`-rooted `target` exhausts the policy attempts and surfaces as the terminal `boundary` fault, the conditional collision read off the lifted message rather than a retry-suppressing per-arm catch the egress is forbidden to grow. The precondition vocabulary stays the one `BoundaryFault` family the whole branch returns through, never a per-package classification table and never an invented HTTP-status integer the `.api` EXCEPTIONS rows model no field for.
- Packages: `obstore` (`store.from_url(url, *, config, client_options, retry_config, credential_provider)`/`put`/`put_async`/`get`/`get_async`/`get_range`/`get_range_async`/`get_ranges`/`get_ranges_async`/`list`/`list_with_delimiter`/`list_with_delimiter_async`/`head`/`head_async`/`delete`/`delete_async`/`copy`/`copy_async`/`rename`/`rename_async`/`open_reader`/`open_reader_async`/`open_writer`/`open_writer_async`/`sign`/`sign_async(store, method, paths, expires_in)`/`GetResult.bytes`/`GetResult.meta`/`GetResult.range`/`GetResult.attributes`/`Bytes`/`PutResult`/`ObjectMeta`/`GetOptions`/`PutMode`/`Attributes`/`UpdateVersion`/`ListResult`/`HTTP_METHOD`/`SignCapableStore`/`store.ObjectStore`/`exceptions.BaseError` plus the leaf set — `GetResult.stream` returns the typing-only `BytesStream` chunk iterator the same `Get` row carries as a growth value, never a runtime-imported obstore symbol; `parse_scheme` is the top-level scheme classifier the egress does not bind because `from_url` dispatches the scheme), `arro3.core` (`RecordBatch.num_rows` over the recursive `return_arrow=True` `ListStream` chunks and `Table.num_rows` over the `list_with_delimiter(return_arrow=True)["objects"]` flat table), `pyarrow` (the bundle payloads), `opentelemetry` (`trace.get_tracer`/`Tracer.start_as_current_span` the per-op egress parent span), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `of` factory so a `ResourceRef`/config-union argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the caller's enclosing fence rather than an untyped admission, the shared `FAULT_CONF` the sibling `interop`/`store`/`ragged` admission seams bind), runtime (`TransportResource`/`ResourceRef`/`RuntimeRail`/`BoundaryFault`/`FAULT_CONF` the shared beartype violation-redirect config/`RetryClass`/`guarded` the async retried-traced-railed envelope every awaitable op delegates to/`guarded_sync` the synchronous mirror every sync op delegates to (both fusing the `OBJECT_STORE` row's retry, the child `resilience.guarded` span, and the `reliability/faults#FAULT` `async_boundary`/`boundary` terminal lift)/`ContentIdentity`/`ContentKey`/`Receipt`/`ReceiptContributor`).
- Growth: a new store operation is one `StoreOp` case plus one `_ROUTE` row pairing its sync member, `_async` sibling, argument plan, `read`, and path projector; a new cloud backend is one `from_url` URL scheme `from_url` already dispatches; a new precondition is one `PutMode` payload on the existing `Put` case or one `overwrite` value on the existing `Copy`/`Rename` case; a new conditional-get axis is one `GetOptions` key on the existing `Get` case; a new window-addressing mode is the existing `ends`/`lengths` pair on the `GetRange`/`GetRanges` cases; a new get-response evidence field is one more `GetResult` member on the existing `Get` `read`'s `payload` tuple, never a second receipt; a new streaming or signing surface is one `StoreOp` case whose `read` carries its non-byte value on the receipt `payload` slot and emits a `None` `Source` so it mints no content key, never a parallel `_HANDLE` table; the async fast-path is the existing `run_async` over the same row set; the content-key modality is the existing `read`-emitted `Source` — a `None` for a control-plane op, else the buffer/stream the one `ContentIdentity.of` classifies as whole buffer or chunk stream — never a new identity surface; zero new receipt surface.
- Boundary: this is the data-tier bundle-I/O owner — it owns the full write/mutation direction (`put`/`copy`/`rename`/`delete`/`writer`) plus the bundle-byte reads its `columnar`/`geospatial`/`tensor`/`gridded/virtual`/`spatial/catalog` consumers need (`get`/`get_range`/`get_ranges`/`head`/`reader`/`sign`, the archival-chunk window fast-path the `gridded/virtual` cube and `catalog` `AssetFold` read against HDF5/NetCDF/GeoTIFF byte ranges) over Arrow/Parquet/GeoParquet/zarr bundles keyed by `ContentIdentity`, while the runtime `transport/roots#RESOURCE` `ResourceRoot` owns the orthogonal runtime-transport lane — the concurrent-small-object/large-artifact-stream `read` (`ReadModality.WHOLE`/`STREAM`) and the generic `survey(StoreView.LIST/SIGN)` inventory/presign for companion artifact pulls; the two share the `obstore` provider and the `RetryClass.OBJECT_STORE` envelope but split by tier (data-bundle versus runtime-artifact), so egress composes `TransportResource`/`ResourceRef` only for credentials and endpoint and never re-derives roots' generic read/survey lane, and roots never moves a data-tier bundle. It composes the runtime `reliability/resilience#RESILIENCE` `guarded`/`guarded_sync` envelopes for the async and synchronous retried-traced-railed acquisition over the one `RetryClass.OBJECT_STORE` row (and the `reliability/faults#FAULT` `BoundaryFault` family those envelopes lift through), the runtime `ContentIdentity` for keying, and the `opentelemetry` `trace` API for the egress parent span, never a second transport, resilience, fault, identity, or tracer owner; consumes the egress bundles from `columnar`, `geospatial`, and `tensor` rather than re-minting them; no durable product store, no fsspec re-derivation, no parallel `S3Egress`/`GcsEgress` family, no parallel `AsyncObjectEgress`; a hand-rolled multi-cloud HTTP client, a thin rename wrapper over the `obstore` operations, a per-leg keyword allowlist diverging the async plan from the sync plan, a per-package `_classify` exception table, a bare `guard(RetryClass.OBJECT_STORE)`/`guard_sync(RetryClass.OBJECT_STORE)` caller composed inside the egress's own hand-opened `async_boundary`/`boundary` and span (the doubled-span/doubled-lift form the `transport/roots#RESOURCE`/`data:tabular/lakehouse#LAKEHOUSE`/resilience owners reject, where `guarded`/`guarded_sync` fuse the retry/child-span/terminal-lift triplet once and the lanes `retried` admission row alone binds bare `guard`), a sync `run` fencing through a bare `boundary` that drops the `OBJECT_STORE` outer stamina envelope to lean on the `obstore` Rust-core `RetryConfig` alone where the async leg carries it (the asymmetric-retry deleted form), a narrowed `catch=obstore.exceptions.BaseError` letting a row-projection `KeyError`/`AttributeError` escape the egress, a `list`-only branch escaping the `_ROUTE` fold, a parallel `_HandleRow`/`_HANDLE` shape beside the one `_Row`/`_ROUTE` owner, a per-arm sync/async method twin, a path-string `ContentIdentity.of` key contradicting the identity owner's no-path law, a hand-rolled `_key` merkle fold re-implementing the `IdentitySource.lift` discriminator, a `bytes()` copy of the zero-copy `Bytes` buffer before the digest, a `parse_scheme(path)` call mis-classifying a relative object path as a URL, a per-operation `from_url` re-mint of the store handle, a four-positional `Receipt.of(phase, owner, subject, facts)` against the two-argument evidence-tuple form, a `contribute` returning a single `Receipt` against the `Iterable[Receipt]` Protocol, an unspanned egress op, an `expires_in: int` against the `timedelta` sign window, a runtime `from obstore import SignCapableStore`/`HTTP_METHOD` import against their stub-only (`obstore._sign.pyi` `TypeAlias`) status where the `Method` literal inlines the nine `HTTP_METHOD` members and `SignCapableStore` stays a typing reference, an `Any`-typed store or a divergent `ObjectStoreMethods`-mixin annotation against the canonical `ObjectStore` Union `from_url` returns and the sibling `transport/roots#RESOURCE` cache carries, and an undecorated `of` admitting a caller `ResourceRef`/config-union argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling `interop`/`store`/`ragged` admission factories share are the deleted forms.

```python signature
from collections.abc import Buffer, Callable, Iterable, Sequence
from datetime import timedelta
from typing import TYPE_CHECKING, Any, Final, Literal

import obstore
from beartype import beartype
from expression import Ok, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from obstore.store import ObjectStore, from_url
from opentelemetry import trace

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded, guarded_sync
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from obstore import Attributes, GetOptions, ObjectMeta, PutMode, PutResult
    from obstore.store import AzureConfig, ClientConfig, GCSConfig, RetryConfig, S3Config

# `Method` mirrors the obstore `HTTP_METHOD` presign literal (the full nine-member set).
# `Source` is the runtime `evidence/identity#IDENTITY` payload axis widened with `None`: a
# byte-bearing op emits a zero-copy `Bytes`/`Buffer` whole (`put`/`get`/`get_range`, the `Buffer()`
# arm) or a `get_ranges` `tuple[Bytes, ...]` keyed `stream` (the `Iterable()` arm), and a metadata-
# /control-plane op that moves NO operation bytes (`list`/`head`/`delete`/`copy`/`rename`/`reader`/
# `writer`/`sign`) emits `None` so `_receipt` mints NO content key rather than digesting a path string
# or a server-opaque `e_tag` string — the identity owner's no-path / operation-bytes law the egress
# would otherwise break (`head`'s `e_tag`/`version`/`size` ride the receipt's own typed slots, which
# already carry that evidence; a key over `str(e_tag).encode()` would add nothing those slots lack
# while digesting a non-operation-bytes string). The `tuple[ContentKey, ...]` merkle arm is the
# identity owner's modality the egress mints no spine for.
type Config = "S3Config | GCSConfig | AzureConfig"
type Provider = Callable[[], Any] | None
type Method = Literal["GET", "PUT", "POST", "HEAD", "PATCH", "TRACE", "DELETE", "OPTIONS", "CONNECT"]
type Meta = "ObjectMeta | PutResult | None"
type Source = Buffer | Iterable[bytes] | tuple[ContentKey, ...] | None
type Call = tuple[tuple[Any, ...], dict[str, Any]]
type Read = tuple[int, Meta, Source, Any]

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
    def Writer(path: str, attributes: "Attributes | None" = None, tags: dict[str, str] | None = None, buffer_size: int | None = None, max_concurrency: int = FANOUT) -> StoreOp:
        return StoreOp(writer=(path, attributes or {}, tags or {}, buffer_size, max_concurrency))

    @staticmethod
    def Sign(method: Method = "GET", paths: Sequence[str] = (), expires_in: timedelta = timedelta(hours=1)) -> StoreOp:
        return StoreOp(sign=(method, paths, expires_in))


class _Row(Struct, frozen=True):
    sync: Callable[..., Any]
    aio: Callable[..., Any]
    plan: Callable[[StoreOp, str], Call]
    # `read` projects the obstore return to (byte length, meta dict, identity `Source`, carried
    # value); a byte-bearing row emits real operation bytes as the `Source` `_receipt` folds through
    # `ContentIdentity.of` exactly once, a control-plane row emits `None` so `_receipt` mints no key.
    read: Callable[[StoreOp, str, Any], Read]
    path: Callable[[StoreOp, str], str]


def _listing(store: ObjectStore, prefix: str, *, offset: str | None, delimiter: bool) -> int:
    # `list_with_delimiter(return_arrow=True)["objects"]` is one arro3 `Table` carrying the flat
    # listing — its `num_rows` IS the count, read directly (iterating a `Table` yields its
    # `ChunkedArray` COLUMNS, which carry no `num_rows`); the recursive `list` yields `RecordBatch`
    # chunks summed by `num_rows`.
    if delimiter:
        return obstore.list_with_delimiter(store, prefix, return_arrow=True)["objects"].num_rows
    return sum(batch.num_rows for batch in obstore.list(store, prefix, offset=offset, return_arrow=True))


async def _listing_async(store: ObjectStore, prefix: str, *, offset: str | None, delimiter: bool) -> int:
    if delimiter:
        return (await obstore.list_with_delimiter_async(store, prefix, return_arrow=True))["objects"].num_rows
    return sum([batch.num_rows async for batch in obstore.list(store, prefix, offset=offset, return_arrow=True)])


_ROUTE: Final[Map[str, _Row]] = Map.of_seq([
    ("put", _Row(obstore.put, obstore.put_async,
        lambda op, t: ((t, op.put[0]), {"mode": op.put[1], "attributes": op.put[2], "tags": op.put[3], "chunk_size": op.put[4]}),
        lambda op, t, result: (len(op.put[0]), result, op.put[0], None),
        lambda op, t: t)),
    ("get", _Row(obstore.get, obstore.get_async,
        lambda op, t: ((op.get[0],), {"options": op.get[1]}),
        lambda op, t, result: (len(body := result.bytes()), result.meta, body, (result.range, result.attributes)),
        lambda op, t: op.get[0])),
    ("get_range", _Row(obstore.get_range, obstore.get_range_async,
        lambda op, t: ((op.get_range[0],), {"start": op.get_range[1], "end": op.get_range[2], "length": op.get_range[3]}),
        lambda op, t, window: (len(window), None, window, None),
        lambda op, t: op.get_range[0])),
    ("get_ranges", _Row(obstore.get_ranges, obstore.get_ranges_async,
        lambda op, t: ((op.get_ranges[0],), {"starts": op.get_ranges[1], "ends": op.get_ranges[2], "lengths": op.get_ranges[3], "coalesce": op.get_ranges[4]}),
        lambda op, t, windows: (sum(len(w) for w in windows), None, tuple(windows), None),
        lambda op, t: op.get_ranges[0])),
    ("list", _Row(_listing, _listing_async,
        lambda op, t: ((op.list[0],), {"offset": op.list[1], "delimiter": op.list[2]}),
        lambda op, t, rows: (rows, None, None, None),
        lambda op, t: op.list[0])),
    ("head", _Row(obstore.head, obstore.head_async,
        lambda op, t: ((op.head,), {}),
        lambda op, t, meta: (meta["size"], meta, None, None),
        lambda op, t: op.head)),
    ("delete", _Row(obstore.delete, obstore.delete_async,
        lambda op, t: ((op.delete,), {}),
        lambda op, t, _: (0, None, None, None),
        lambda op, t: op.delete if isinstance(op.delete, str) else ",".join(op.delete))),
    ("copy", _Row(obstore.copy, obstore.copy_async,
        lambda op, t: ((op.copy[0], op.copy[1]), {"overwrite": op.copy[2]}),
        lambda op, t, _: (0, None, None, None),
        lambda op, t: op.copy[1])),
    ("rename", _Row(obstore.rename, obstore.rename_async,
        lambda op, t: ((op.rename[0], op.rename[1]), {"overwrite": op.rename[2]}),
        lambda op, t, _: (0, None, None, None),
        lambda op, t: op.rename[1])),
    ("reader", _Row(obstore.open_reader, obstore.open_reader_async,
        lambda op, t: ((op.reader[0],), {k: v for k, v in (("buffer_size", op.reader[1]), ("size", op.reader[2])) if v is not None}),
        lambda op, t, file: (0, None, None, file),
        lambda op, t: op.reader[0])),
    ("writer", _Row(obstore.open_writer, obstore.open_writer_async,
        lambda op, t: ((op.writer[0],), {k: v for k, v in (("attributes", op.writer[1]), ("tags", op.writer[2]), ("buffer_size", op.writer[3]), ("max_concurrency", op.writer[4])) if v is not None}),
        lambda op, t, file: (0, None, None, file),
        lambda op, t: op.writer[0])),
    ("sign", _Row(obstore.sign, obstore.sign_async,
        lambda op, t: ((op.sign[0], op.sign[1], op.sign[2]), {}),
        lambda op, t, urls: (len(op.sign[1]), None, None, urls),
        lambda op, t: op.sign[1][0] if op.sign[1] else "")),
])


class EgressReceipt(Struct, frozen=True):
    operation: str
    path: str
    byte_length: int
    e_tag: str
    version: str
    # `None` for a metadata-/control-plane op moving no operation bytes (`list`/`head`/`delete`/
    # `copy`/`rename`/`reader`/`writer`/`sign`): the receipt mints no content key rather than digesting
    # a path string or a server-opaque `e_tag` — `head`'s `e_tag`/`version`/`size` ride the typed slots.
    content_key: ContentKey | None
    payload: Any = None

    @classmethod
    def of(cls, operation: str, path: str, length: int, meta: Meta, content_key: ContentKey | None, payload: Any) -> EgressReceipt:
        slot = meta or {}
        return cls(operation=operation, path=path, byte_length=length, e_tag=str(slot.get("e_tag") or ""), version=str(slot.get("version") or ""), content_key=content_key, payload=payload)

    def contribute(self) -> Iterable[Receipt]:
        # `byte_length` rides as a native `int`: the receipts `Encoder(enc_hook=repr)` serializes
        # scalars without a `str()` coerce, exactly the deleted form the receipts owner rejects.
        yield Receipt.of("object-egress", ("emitted", self.path, {
            "op": self.operation, "bytes": self.byte_length, "etag": self.e_tag, "version": self.version,
            "key": self.content_key.hex if self.content_key is not None else None,
        }))


class ObjectEgress(Struct, frozen=True):
    ref: ResourceRef
    store: ObjectStore

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, ref: ResourceRef, config: Config | None = None, client_options: "ClientConfig | None" = None, retry_config: "RetryConfig | None" = None, credential_provider: Provider = None) -> ObjectEgress:
        return cls(ref=ref, store=from_url(ref.root, config=config, client_options=client_options, retry_config=retry_config, credential_provider=credential_provider))

    def run(self, op: StoreOp, path: str = "") -> RuntimeRail[EgressReceipt]:
        with _TRACER.start_as_current_span(f"egress.{op.tag}", attributes={"rasm.egress.scheme": self.ref.scheme}):
            return guarded_sync(RetryClass.OBJECT_STORE, self._apply, op, path or self.ref.relative, subject=f"egress.{op.tag}").bind(lambda rail: rail)

    async def run_async(self, op: StoreOp, path: str = "") -> RuntimeRail[EgressReceipt]:
        with _TRACER.start_as_current_span(f"egress.{op.tag}", attributes={"rasm.egress.scheme": self.ref.scheme}):
            return (await guarded(RetryClass.OBJECT_STORE, self._apply_async, op, path or self.ref.relative, subject=f"egress.{op.tag}")).bind(lambda rail: rail)

    def _receipt(self, op: StoreOp, target: str, returned: Any) -> RuntimeRail[EgressReceipt]:
        row = _ROUTE[op.tag]
        length, meta, source, payload = row.read(op, target, returned)

        def receipt(key: ContentKey | None) -> EgressReceipt:
            return EgressReceipt.of(op.tag, row.path(op, target), length, meta, key, payload)

        # a metadata-/control-plane op (`source is None`) moves no operation bytes, so it mints NO
        # content key rather than digesting a path string or a server-opaque `e_tag` — the identity
        # owner's no-path / operation-bytes law. A byte-bearing op runs the one `ContentIdentity.of`
        # call, which classifies the `Source` (a `Bytes` whole or a `get_ranges` `tuple[Bytes, ...]`
        # stream) and rails the key, scheme-scoped through `self.ref.scheme`.
        if source is None:
            return Ok(receipt(None))
        return ContentIdentity.of(f"egress.{op.tag}.{self.ref.scheme}", source).map(receipt)

    def _apply(self, op: StoreOp, target: str) -> RuntimeRail[EgressReceipt]:
        row = _ROUTE[op.tag]
        args, kwargs = row.plan(op, target)
        return self._receipt(op, target, row.sync(self.store, *args, **kwargs))

    async def _apply_async(self, op: StoreOp, target: str) -> RuntimeRail[EgressReceipt]:
        row = _ROUTE[op.tag]
        args, kwargs = row.plan(op, target)
        return self._receipt(op, target, await row.aio(self.store, *args, **kwargs))
```

## [03]-[RESEARCH]

- [OBSTORE_FAULT_CONVERSION]: the `obstore.exceptions` taxonomy — `BaseError` root with `NotFoundError`/`AlreadyExistsError`/`PermissionDeniedError`/`UnauthenticatedError`/`PreconditionError`/`NotModifiedError`/`NotSupportedError`/`InvalidPathError`/`UnknownConfigurationKeyError`/`GenericError`/`JoinError` leaves — is catalogue-confirmed (`runtime/.api/obstore.md` exceptions [01]-[12]) and reflection-confirmed against the live 0.10.1 distribution (`dir(obstore.exceptions)` yields all eleven leaves plus the root). The async leg lifts the terminal raise through the `reliability/resilience#RESILIENCE` `guarded` envelope (its `reliability/faults#FAULT` `async_boundary`) and the sync leg through the `guarded_sync` envelope (its synchronous `boundary`), both at the `Exception` catch-all the `CLASSIFY` fold owns — the `guarded`/`guarded_sync` delegation is exactly the `transport/roots#RESOURCE` async object-store-reader and `data:tabular/lakehouse#LAKEHOUSE` synchronous-commit convention where the resilience owner's `async_boundary`/`boundary` catches `Exception` and `CLASSIFY` lands the obstore raise on its catch-all, so a narrowed `catch=obstore.exceptions.BaseError` is the deleted form because it would let a row-projection `KeyError`/`AttributeError` escape the egress rather than convert. The obstore taxonomy is NOT a `BoundaryFault.CLASSIFY` row (the `faults` owner's `CLASSIFY` `Block` rows only `TimeoutError`/`msgspec`/`beartype`/`ImportError`/`anyio`+`OSError`), so `BoundaryFault.of(subject, cause)` falls to the `try_head().default_with(...)` catch-all carrying the `boundary` case `(subject, str(cause) or type(cause).__name__)` — the obstore message first, the class name only on a message-less raise — and recovery keys on `fault.recoverable({"boundary"})`, never a reconstructed message, never a per-package `_classify` table, and never an invented HTTP-status integer since the EXCEPTIONS rows model no numeric status. `NotFoundError` (absent get/head) and `NotSupportedError` (sign on a non-`SignCapableStore` backend) ride the same catch-all, so the recovery vocabulary keys on `fault.recoverable({"boundary"})` like every other leaf. `SignCapableStore` (`= AzureStore | GCSStore | S3Store`) and the full nine-member `HTTP_METHOD` literal (`GET`/`PUT`/`POST`/`HEAD`/`PATCH`/`TRACE`/`DELETE`/`OPTIONS`/`CONNECT`) are reflection-confirmed type-stub-only `TypeAlias` names — declared in `obstore._sign.pyi` (no runtime `obstore._sign` module exists), absent from every `__all__`, and NOT re-exported from `obstore`/`obstore.store` at runtime — so they are typing-only like the `PutResult`/`GetOptions` result types: the egress inlines the nine `HTTP_METHOD` members into the local `Method` literal and references `SignCapableStore` only as a typing concept (the sign arm rejecting a non-sign backend at the obstore call as the `NotSupportedError` the `boundary` converts), never a runtime `from obstore import SignCapableStore`/`HTTP_METHOD`.
- [OBSTORE_CREDENTIAL_CONFIG]: the `config` value `ObjectEgress.of` admits is the `S3Config | GCSConfig | AzureConfig` `TypedDict` union in `obstore.store` (or `None`) — `from_url(url, *, config: S3Config | GCSConfig | AzureConfig | None, ...)` reflection-confirmed, so the prior `dict[str, str]` arm is dropped as a wider-than-the-signature type — `client_options` the `ClientConfig` `TypedDict` and `retry_config` the `RetryConfig` `TypedDict` (`runtime/.api/obstore.md` config types); the `credential_provider` callable rides the runtime `TransportResource` for token refresh and threads through `from_url(url, *, config, client_options, retry_config, credential_provider, **kwargs)` to the per-scheme store, reflection-confirmed on the native core.
- [OBSTORE_STREAM_AND_SIGN]: the buffered large-bundle streaming handles — `obstore.open_reader(store, path, *, buffer_size, size)`/`open_reader_async` returning `ReadableFile`/`AsyncReadableFile` and `obstore.open_writer(store, path, *, attributes, buffer_size, tags, max_concurrency)`/`open_writer_async` returning `WritableFile`/`AsyncWritableFile` (list/streaming [04]-[07]) — and the presigned-URL mint `obstore.sign(store, method, paths, expires_in)`/`sign_async` (presign [01]-[02]) fold into the one `StoreOp` axis as the `reader`/`writer`/`sign` cases, their `_ROUTE` `read`s carrying the obstore handle or URL batch on the `EgressReceipt.payload` slot rather than a parallel `StoreHandle` union or a second `handle`/`handle_async` surface; `open_reader`/`open_writer` own large-object streaming and `sign` requires a `SignCapableStore` (else `NotSupportedError`), both reflection-confirmed on the native core.
- [EGRESS_OBSERVABILITY]: each `run`/`run_async` opens one `_TRACER.start_as_current_span("egress.{tag}", attributes={"rasm.egress.scheme": ...})` parent span over `opentelemetry.trace.get_tracer` (`branch/.api/opentelemetry-api.md` trace API [10]/[03]), then delegates to the matching `reliability/resilience#RESILIENCE` envelope over the one `RetryClass.OBJECT_STORE` row: the awaitable leg to `guarded(RetryClass.OBJECT_STORE, _apply_async, op, target, subject="egress.{tag}")` and the synchronous leg to `guarded_sync(RetryClass.OBJECT_STORE, _apply, op, target, subject="egress.{tag}")`. Each envelope opens its own `resilience.guarded` derivation span and a per-retry `resilience.retry` child span beneath the active egress parent and lifts the terminal raise through one `async_boundary`/`boundary` — so the egress span is the parent the retry spans attach to on both legs without the egress re-implementing the retry/span/lift triplet, the `transport/roots#RESOURCE` async delegation and the `data:tabular/lakehouse#LAKEHOUSE` synchronous-commit delegation rather than a bare `guard(cls)`/`guard_sync(cls)` caller in a hand-opened fence and span, and never a sync leg dropping the `OBJECT_STORE` outer envelope where the async leg carries it — both legs ride the same stamina row over the store's inner `RetryConfig`, so the two span trees are symmetric. The `EgressReceipt.contribute` yields the emitted-phase `Receipt.of("object-egress", ("emitted", path, facts))` row the `observability/receipts#RECEIPT` `ReceiptContributor.contribute -> Iterable[Receipt]` Protocol drains, the facts dict carrying native scalars the `Encoder(enc_hook=repr)` serializes without a `str()` coerce.
