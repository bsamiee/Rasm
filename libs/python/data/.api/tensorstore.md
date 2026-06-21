# [PY_DATA_API_TENSORSTORE]

`tensorstore` supplies the asynchronous multi-dimensional array backend for the data rail: a single `open` factory that resolves a JSON/`Spec` driver configuration into a `Future[TensorStore]`, a `TensorStore` handle whose `read`/`write` return awaitable `Future`/`WriteFutures`, and a `zarr3` driver bound to any `KvStore` backend (`file`, `gcs`, `s3`, `memory`, `ocdbt`). The data owner composes `open`, `TensorStore`, `KvStore`, and one shared `Context` into the TENSORSTORE_ADMIT async zarr-v3 read/write path; every I/O entrypoint is `await`-native, so the owner consumes the Future rail directly rather than re-implementing chunked codec, sharding, or cache-coherence machinery tensorstore already owns. tensorstore is the high-throughput async writer/reader over the **same on-disk zarr-v3 layout** that virtualizarr references and zarr writes: a `zarr3` store opened over an `ocdbt` `KvStore` is the durable substrate a virtualizarr `ManifestStore` can later reference without copy, so the two libraries meet at the zarr-v3 wire rather than through a bridging adapter.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tensorstore`
- package: `tensorstore`
- import: `tensorstore`
- owner: `data`
- rail: array-store
- installed: `0.1.84` docs-derived; native (C++/pybind11) wheel, manifest-markered `python_version<'3.15'`, so it resolves on the companion interpreter only — the cp315 project venv carries no native wheel and `assay api resolve` finds no source there
- entry points: library use is import-only; no console script
- submodules: `tensorstore.experimental` (metrics: `experimental_collect_matching_metrics`, `experimental_collect_prometheus_format_metrics`, `experimental_push_metrics_to_prometheus`, `experimental_update_verbose_logging`, `parse_tensorstore_flags`); `tensorstore.ocdbt` (`DistributedCoordinatorServer`, `dump`, `undump_manifest`, `undump_version_tree_node`) for the optimistically-concurrent distributed B+tree kvstore
- capability: asynchronous N-dimensional array open/read/write over the `zarr3` (and `zarr`, `n5`, `neuroglancer_precomputed`) drivers, JSON/`Spec`-driven schema/codec/chunk-layout control, `KvStore`-abstracted storage (`file`/`gcs`/`s3`/`memory`/`ocdbt`), transactional staging, virtual `stack`/`concat`/`overlay`/`downsample`/`virtual_chunked` views, NumPy-style advanced indexing, and a `Future`/`WriteFutures` async rail consumable by `await`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: array handle, spec, index space, and async rail
- rail: array-store

`open` returns a `Future[TensorStore]`; `TensorStore.read` returns a `Future[numpy.ndarray]` and `TensorStore.write` returns a `WriteFutures` bundling a `copy` and a `commit` future. `Spec` carries the JSON driver configuration; `KvStore` abstracts the byte-keyed storage backend the `zarr3` driver writes through.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :--------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `TensorStore`    | array handle  | asynchronous multi-dimensional array view with `read`/`write` |
|  [02]   | `Spec`           | specification | JSON-backed driver/schema configuration for `open`            |
|  [03]   | `Schema`         | specification | driver-independent rank/dtype/domain/codec/chunk constraints  |
|  [04]   | `CodecSpec`      | specification | driver-specific encode/decode (zarr3 codec chain) parameters  |
|  [05]   | `ChunkLayout`    | specification | read/write/grid chunk-grid storage layout                     |
|  [06]   | `OpenMode`       | specification | open/create/delete_existing/assume_metadata mode set          |
|  [07]   | `KvStore`        | storage       | ordered byte-key/byte-value store under the array driver      |
|  [08]   | `KvStore.Spec`   | storage       | parsed JSON key-value store specification                     |
|  [09]   | `Context`        | resource      | shared cache/credential/concurrency resource pool             |
|  [10]   | `Transaction`    | resource      | staged-modification group with atomic commit/abort            |
|  [11]   | `Batch`          | resource      | grouped-read coalescing handle                                |
|  [12]   | `IndexDomain`    | index space   | N-dimensional bounds-and-labels domain                        |
|  [13]   | `IndexTransform` | index space   | input-to-output index-space transform                         |
|  [14]   | `Dim`            | index space   | 1-d index interval with optional label and implicit bounds    |
|  [15]   | `DimExpression`  | index space   | advanced indexing/transform operation chain                   |
|  [16]   | `d`              | index space   | `DimExpression` builder: `ts.d[...]`/`ts.d['x','y']` selects dims |
|  [17]   | `newaxis`        | index space   | alias for `None` adding a singleton dimension in indexing     |
|  [18]   | `Unit`           | metadata      | physical quantity/unit for a dimension                        |
|  [19]   | `dtype`          | data type     | TensorStore data type bridging the NumPy dtype                |
|  [20]   | `Future`         | async rail    | awaitable handle for an asynchronous result                   |
|  [21]   | `WriteFutures`   | async rail    | write handle bundling `copy` and `commit` futures             |
|  [22]   | `Promise`        | async rail    | producer side of a `Future`                                   |
|  [23]   | `FutureLike`     | async rail    | generic possibly-asynchronous-result type for callbacks       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module factories
- rail: array-store

`open` is the single array-store entrypoint; `spec` carries the `zarr3` driver plus `kvstore`, while `read`/`write`/`create`/`open_mode` and the inline schema rows (`dtype`/`domain`/`shape`/`chunk_layout`/`codec`/`fill_value`/`dimension_units`/`schema`) refine the open without a per-mode factory. Virtual views (`stack`/`concat`/`overlay`/`downsample`/`virtual_chunked`) compose existing handles rather than re-opening storage.

| [INDEX] | [SURFACE]         | [CALL_SHAPE]                                                                                                                                                                                                                                                                                                                                                                                                                                               | [CAPABILITY]                                         |
| :-----: | :---------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `open`            | `open(spec, *, read=None, write=None, open_mode=None, open=None, create=None, delete_existing=None, assume_metadata=None, assume_cached_metadata=None, context=None, transaction=None, batch=None, kvstore=None, recheck_cached_metadata=None, recheck_cached_data=None, recheck_cached=None, rank=None, dtype=None, domain=None, shape=None, chunk_layout=None, codec=None, fill_value=None, dimension_units=None, schema=None)` -> `Future[TensorStore]` | open/create a `TensorStore` from a `Spec` or JSON    |
|  [02]   | `array`           | `array(array, dtype=None, *, context=None, transaction=None)` -> `TensorStore`                                                                                                                                                                                                                                                                                                                                                                             | wrap an in-memory array as a `TensorStore`           |
|  [03]   | `cast`            | `cast(base, dtype)` -> `TensorStore` \| `Spec`                                                                                                                                                                                                                                                                                                                                                                                                             | dtype-conversion view over a store or spec           |
|  [04]   | `stack`           | `stack(layers, axis=0, *, read=None, write=None, context=None, transaction=None)` -> `TensorStore`                                                                                                                                                                                                                                                                                                                                                         | virtually stack layers along a new dimension         |
|  [05]   | `concat`          | `concat(layers, axis, *, read=None, write=None, context=None, transaction=None)` -> `TensorStore`                                                                                                                                                                                                                                                                                                                                                          | virtually concatenate layers along an existing axis  |
|  [06]   | `overlay`         | `overlay(layers, *, read=None, write=None, context=None, transaction=None)` -> `TensorStore`                                                                                                                                                                                                                                                                                                                                                               | virtually overlay layers within a shared domain      |
|  [07]   | `downsample`      | `downsample(base, downsample_factors, method)` -> `TensorStore` \| `Spec`                                                                                                                                                                                                                                                                                                                                                                                  | virtual reduced-resolution view                      |
|  [08]   | `virtual_chunked` | `virtual_chunked(read_function=None, write_function=None, *, context=None, transaction=None, ...)` -> `TensorStore`                                                                                                                                                                                                                                                                                                                                        | chunk-wise store backed by a Python callback         |
|  [09]   | `KvStore.open`    | `KvStore.open(spec, *, context=None, transaction=None)` -> `Future[KvStore]`                                                                                                                                                                                                                                                                                                                                                                               | open a key-value store from a `KvStore.Spec` or JSON |

[ENTRYPOINT_SCOPE]: `TensorStore` async I/O and views
- rail: array-store

`read` and `write` are the async I/O rail; their results are `await`-native (`data = await store.read()`; `await store.write(array)`). `resolve`/`resize`/`storage_statistics` return a `Future`; the indexing, label, transpose, translate, and transaction-binding surfaces return synchronous virtual `TensorStore` views. Read properties carry the schema metadata the data receipt records.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                                                                                                                          | [CAPABILITY]                                         |
| :-----: | :--------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `TensorStore.read`                 | `read(order='C', *, batch=None)` -> `Future[numpy.ndarray]`                                                                                                           | read the current domain into a NumPy array           |
|  [02]   | `TensorStore.write`                | `write(source)` -> `WriteFutures`                                                                                                                                     | write source into the current domain                 |
|  [03]   | `TensorStore.resolve`              | `resolve(*, fix_resizable_bounds=False)` -> `Future[TensorStore]`                                                                                                     | obtain updated bounds under the cache policy         |
|  [04]   | `TensorStore.resize`               | `resize(inclusive_min=None, exclusive_max=None, resize_metadata_only=False, resize_tied_bounds=False, expand_only=False, shrink_only=False)` -> `Future[TensorStore]` | persistently resize the stored domain                |
|  [05]   | `TensorStore.storage_statistics`   | `storage_statistics(*, query_not_stored=False, query_fully_stored=False)` -> `Future[StorageStatistics]`                                                              | query stored/unstored chunk extent                   |
|  [06]   | `TensorStore.astype`               | `astype(dtype)` -> `TensorStore`                                                                                                                                      | dtype-conversion read/write view                     |
|  [07]   | `TensorStore.spec`                 | `spec(*, minimal_spec=False, retain_context=False, unbind_context=False)` -> `Spec`                                                                                   | spec to re-open or re-create the store               |
|  [08]   | `TensorStore.with_transaction`     | `with_transaction(transaction)` -> `TensorStore`                                                                                                                      | bind a `Transaction` to a view                       |
|  [09]   | `TensorStore.__getitem__`          | `store[transform \| domain \| expr \| indices]` -> `TensorStore`                                                                                                      | virtual view by transform/domain/expr/NumPy indexing |
|  [10]   | `TensorStore.__setitem__`          | `store[transform \| domain \| expr \| indices] = source`                                                                                                              | synchronous write into the indexed sub-view          |
|  [11]   | `TensorStore.vindex`               | `vindex[indices]` -> `TensorStore`                                                                                                                                    | vectorized-indexing virtual view                     |
|  [12]   | `TensorStore.oindex`               | `oindex[indices]` -> `TensorStore`                                                                                                                                    | outer-indexing virtual view                          |
|  [13]   | `TensorStore.label`                | `label(*labels)` -> `TensorStore`                                                                                                                                     | relabel domain dimensions                            |
|  [14]   | `TensorStore.transpose`            | `transpose(axes=None)` -> `TensorStore` (`store.T` reverses dims)                                                                                                     | transposed-domain view                               |
|  [15]   | `TensorStore.translate_to`         | `translate_to(origin)` -> `TensorStore`                                                                                                                               | reposition domain origin                             |
|  [16]   | `TensorStore.translate_by`         | `translate_by(offsets)` / `translate_backward_by(offsets)` -> `TensorStore`                                                                                           | shift domain origin                                  |
|  [17]   | `TensorStore.mark_bounds_implicit` | `mark_bounds_implicit(...)` -> `TensorStore`                                                                                                                          | toggle implicit (resizable) bound flags              |
|  [18]   | `TensorStore.__array__`            | `__array__(dtype=None)` -> `numpy.ndarray`                                                                                                                            | synchronous NumPy interop conversion                 |
|  [19]   | `TensorStore.domain`               | property -> `IndexDomain`                                                                                                                                             | bounds-and-labels domain of the array                |
|  [20]   | `TensorStore.origin` / `shape` / `size` | property -> `tuple[int, ...]` / `tuple[int, ...]` / `int`                                                                                                        | inclusive lower bound, extent, element count         |
|  [21]   | `TensorStore.dtype`                | property -> `dtype`                                                                                                                                                   | element data type                                    |
|  [22]   | `TensorStore.rank` / `ndim`        | property -> `int`                                                                                                                                                     | dimension count                                      |
|  [23]   | `TensorStore.schema`               | property -> `Schema`                                                                                                                                                  | resolved schema                                      |
|  [24]   | `TensorStore.chunk_layout`         | property -> `ChunkLayout`                                                                                                                                             | resolved chunk-grid layout                           |
|  [25]   | `TensorStore.codec`                | property -> `CodecSpec \| None`                                                                                                                                       | resolved codec spec                                  |
|  [26]   | `TensorStore.fill_value`           | property -> `numpy.ndarray \| None`                                                                                                                                   | fill value for unwritten positions                   |
|  [27]   | `TensorStore.dimension_units`      | property -> `tuple[Unit \| None, ...]`                                                                                                                                | per-dimension physical units                         |
|  [28]   | `TensorStore.kvstore` / `base`     | property -> `KvStore \| None` / `TensorStore \| None`                                                                                                                 | underlying key-value store / adapter base            |
|  [29]   | `TensorStore.url`                  | property -> `str`                                                                                                                                                    | URL form of the store spec                           |
|  [30]   | `TensorStore.transaction`          | property -> `Transaction \| None`                                                                                                                                     | bound transaction                                    |
|  [31]   | `TensorStore.readable` / `writable` / `mode` | property -> `bool` / `bool` / `str`                                                                                                                         | read/write capability flags and mode string          |

[ENTRYPOINT_SCOPE]: async rail and storage
- rail: array-store

`Future` and `WriteFutures` carry the awaitable result; `await future` returns the value or raises the captured error, and `WriteFutures` forwards the `Future` interface to its `commit` future. `KvStore` is the byte-keyed storage surface the `zarr3` driver reads and writes through; `Transaction` stages modifications for an atomic commit.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                  | [CAPABILITY]                                |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `Future.result`            | `result()` -> `T`                                                                             | block until complete, return result         |
|  [02]   | `Future.exception`         | `exception()` -> `Exception \| None`                                                          | block until complete, return error          |
|  [03]   | `Future.__await__`         | `await future` -> `T`                                                                         | async-native consumption of the result      |
|  [04]   | `Future.add_done_callback` | `add_done_callback(callback)` -> `None`                                                       | register a completion callback              |
|  [05]   | `Future.force`             | `force()` -> `None`                                                                           | begin the asynchronous operation eagerly    |
|  [06]   | `Future.cancel`            | `cancel()` -> `bool`                                                                          | request cancellation                        |
|  [07]   | `WriteFutures.copy`        | property -> `Future`                                                                          | completes when the source read finishes     |
|  [08]   | `WriteFutures.commit`      | property -> `Future`                                                                          | completes when the write is durably visible |
|  [09]   | `KvStore.read`             | `read(key, *, byte_range=None, transaction=None, batch=None)` -> `Future[KvStore.ReadResult]` | read one key's value                        |
|  [10]   | `KvStore.write`            | `write(key, value, *, transaction=None)` -> `Future[KvStore.TimestampedStorageGeneration]`    | write or delete one key                     |
|  [11]   | `KvStore.list`             | `list(*, transaction=None)` -> `Future[list[bytes]]`                                          | list keys in the store                      |
|  [12]   | `KvStore.delete_range`     | `delete_range(range)` -> `Future[None]`                                                       | delete a key range                          |
|  [13]   | `Transaction.commit_async` | `commit_async()` -> `Future[None]`                                                            | asynchronously commit staged modifications  |
|  [14]   | `Transaction.abort`        | `abort()` -> `None`                                                                           | abort the transaction                       |

## [04]-[IMPLEMENTATION_LAW]

[ARRAY_STORE_ZARR3]:
- import: `import tensorstore as ts` at boundary scope only; module-level import is banned by the manifest import policy.
- open axis: one `open` owns array-store acquisition; `zarr3` driver, `kvstore`, `open`/`create`/`delete_existing`/`assume_metadata` modes, and the inline schema rows (`dtype`/`domain`/`shape`/`chunk_layout`/`codec`/`fill_value`/`dimension_units`/`schema`) are call rows on the JSON `Spec`, never a per-mode or per-driver factory type; `array`/`cast`/`stack`/`concat`/`overlay`/`downsample`/`virtual_chunked` compose existing handles into virtual views.
- async axis: `read` returns `Future[numpy.ndarray]` and `write` returns `WriteFutures`; the data owner consumes them with `await` (`data = await store.read()`; `await store.write(block)`), never `result()` blocking inside the async rail; `WriteFutures.copy` gates source release and `WriteFutures.commit` gates durable visibility.
- storage axis: `KvStore` is the single byte-keyed backend surface (`file`/`gcs`/`s3`/`memory`/`ocdbt`); the `zarr3` driver writes through it, so backend selection is a `kvstore` row on the `Spec`, never a parallel store type per backend.
- codec axis: `CodecSpec` owns the zarr3 codec chain — `transpose`, `bytes` (endianness), `sharding_indexed` (sub-chunk sharding into one object), `gzip`/`blosc`/`zstd` (compression), `crc32c` (checksum); the codec is a `Spec` row, never a hand-rolled chunk encoder. `sharding_indexed` is the row that packs many logical chunks into one `KvStore` object, the layout virtualizarr later references.
- index axis: `ts.d[...]` builds a `DimExpression` (label/range/diagonal/translate chains) applied as `store[ts.d['x','y'].translate_by[...]]`; advanced indexing, transposition, and labelling are `DimExpression`/`IndexTransform` rows, never positional NumPy-only slicing where a labelled domain exists.
- transaction axis: `Transaction(atomic=...)` stages grouped modifications and exposes one `commit_async`/`abort`; bind it via `open(..., transaction=txn)` or `store.with_transaction(txn)`, never a manual multi-write fence. `Batch` coalesces concurrent reads into one backend round-trip.
- context axis: one `Context` (from a JSON resource spec: `cache_pool`, `data_copy_concurrency`, `file_io_concurrency`, credential resources) is shared across opens so cache pools, concurrency limits, and credentials are pooled, never re-minted per store.
- metrics axis: `tensorstore.experimental` collects internal metrics in Prometheus format for the observability owner; never a hand-instrumented counter around `read`/`write`.
- evidence: each open captures driver id, kvstore url, resolved dtype, domain shape, chunk-grid layout, codec chain, and committed generation as an array-store receipt.
- boundary: tensorstore owns chunked zarr-v3 read/write, sharding, cache coherence, and the `KvStore` backend abstraction; NumPy arrays cross the wire at `read`/`write`; raster/mesh post-processing and on-disk archive packaging route to their own owners; identity/path minting stays with the runtime owner.

[RAIL_LAW]:
- Package: `tensorstore`
- Owns: asynchronous zarr-v3 (`zarr3`) array open/read/write over `KvStore` backends, JSON/`Spec`-driven schema/codec/chunk-layout control, transactional staging, virtual stack/concat/overlay/downsample/virtual_chunked views, and the `Future`/`WriteFutures` async rail
- Accept: async chunked array read/write feeding the data owner over `file`/`gcs`/`s3`/`memory`/`ocdbt` `KvStore` backends
- Reject: wrapper-renames of `open`/`read`/`write`; blocking `result()` calls inside the async rail; a hand-rolled chunk codec, sharding, or cache layer tensorstore already owns; a parallel store type per backend or per open mode; a per-driver factory family where one `Spec` row discriminates the driver
