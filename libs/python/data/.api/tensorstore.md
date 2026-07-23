# [PY_DATA_API_TENSORSTORE]

`tensorstore` owns the asynchronous zarr-v3 array backend for the data rail: one `open` factory resolves a JSON/`Spec` driver configuration into a `Future[TensorStore]`, and `read`/`write` return awaitable `Future`/`WriteFutures` the data owner consumes with `await`. It holds chunked codec, sharding, and cache-coherence behind the `KvStore` backend abstraction, so backend selection is a `Spec` row and the owner never re-implements the array-store rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tensorstore`
- package: `tensorstore` (Apache-2.0)
- module: `tensorstore`
- owner: data
- rail: array-store
- namespaces: `tensorstore.experimental` (Prometheus metrics collection and flush), `tensorstore.ocdbt` (`DistributedCoordinatorServer` and manifest `dump`/`undump` for the ocdbt B+tree kvstore)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: array handle, spec, index space, and async rail

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :--------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `TensorStore`    | array handle  | asynchronous multi-dimensional array view with `read`/`write`     |
|  [02]   | `Spec`           | specification | JSON-backed driver/schema configuration for `open`                |
|  [03]   | `Schema`         | specification | driver-independent rank/dtype/domain/codec/chunk constraints      |
|  [04]   | `CodecSpec`      | specification | driver-specific encode/decode (zarr3 codec chain) parameters      |
|  [05]   | `ChunkLayout`    | specification | read/write/grid chunk-grid storage layout                         |
|  [06]   | `OpenMode`       | specification | open/create/delete_existing/assume_metadata mode set              |
|  [07]   | `KvStore`        | storage       | ordered byte-key/byte-value store under the array driver          |
|  [08]   | `KvStore.Spec`   | storage       | parsed JSON key-value store specification                         |
|  [09]   | `Context`        | resource      | shared cache/credential/concurrency resource pool                 |
|  [10]   | `Transaction`    | resource      | staged-modification group with atomic commit/abort                |
|  [11]   | `Batch`          | resource      | grouped-read coalescing handle                                    |
|  [12]   | `IndexDomain`    | index space   | N-dimensional bounds-and-labels domain                            |
|  [13]   | `IndexTransform` | index space   | input-to-output index-space transform                             |
|  [14]   | `Dim`            | index space   | 1-d index interval with optional label and implicit bounds        |
|  [15]   | `DimExpression`  | index space   | advanced indexing/transform operation chain                       |
|  [16]   | `d`              | index space   | `DimExpression` builder: `ts.d[...]`/`ts.d['x','y']` selects dims |
|  [17]   | `newaxis`        | index space   | alias for `None` adding a singleton dimension in indexing         |
|  [18]   | `Unit`           | metadata      | physical quantity/unit for a dimension                            |
|  [19]   | `dtype`          | data type     | TensorStore data type bridging the NumPy dtype                    |
|  [20]   | `Future`         | async rail    | awaitable handle for an asynchronous result                       |
|  [21]   | `WriteFutures`   | async rail    | write handle bundling `copy` and `commit` futures                 |
|  [22]   | `Promise`        | async rail    | producer side of a `Future`                                       |
|  [23]   | `FutureLike`     | async rail    | generic possibly-asynchronous-result type for callbacks           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module factories

`open` is the sole array-store acquisition factory; the virtual-view family composes existing handles rather than re-opening storage.
- open carry: `read`, `write`, `open_mode`, `open`, `create`, `delete_existing`, `assume_metadata`, `context`, `transaction`, `batch`, `kvstore`, `dtype`, `domain`, `shape`, `chunk_layout`, `codec`, `fill_value`, `dimension_units`, `schema`

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `open(spec, *, ...) -> Future[TensorStore]`                      | open/create a `TensorStore` from a `Spec` or JSON   |
|  [02]   | `array(array, dtype, *, context, transaction)`                   | wrap an in-memory array as a `TensorStore`          |
|  [03]   | `cast(base, dtype) -> TensorStore \| Spec`                       | dtype-conversion view over a store or spec          |
|  [04]   | `stack(layers, axis=0)`                                          | virtually stack layers along a new dimension        |
|  [05]   | `concat(layers, axis)`                                           | virtually concatenate layers along an existing axis |
|  [06]   | `overlay(layers)`                                                | virtually overlay layers within a shared domain     |
|  [07]   | `downsample(base, factors, method) -> TensorStore \| Spec`       | virtual reduced-resolution view                     |
|  [08]   | `virtual_chunked(read_function, write_function, *, ...)`         | chunk-wise store backed by a Python callback        |
|  [09]   | `KvStore.open(spec, *, context, transaction) -> Future[KvStore]` | open a key-value store from a spec or JSON          |

[ENTRYPOINT_SCOPE]: `TensorStore` async I/O and views

`read`/`write` are the await-native async rail (`data = await store.read()`; `await store.write(array)`); `resolve`/`resize`/`storage_statistics` return a `Future`, and the indexing, label, transpose, translate, and transaction-binding methods return a synchronous virtual `TensorStore` view.

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------ | :------------------------------------------ |
|  [01]   | `read(order='C', *, batch) -> Future[numpy.ndarray]`                | read the current domain into a NumPy array  |
|  [02]   | `write(source) -> WriteFutures`                                     | write source into the current domain        |
|  [03]   | `resolve(*, fix_resizable_bounds=False)`                            | updated bounds under the cache policy       |
|  [04]   | `resize(*, inclusive_min, exclusive_max, expand_only, shrink_only)` | persistently resize the stored domain       |
|  [05]   | `storage_statistics(*, query_not_stored, query_fully_stored)`       | query stored/unstored chunk extent          |
|  [06]   | `astype(dtype)`                                                     | dtype-conversion read/write view            |
|  [07]   | `spec(*, minimal_spec, retain_context, unbind_context)`             | spec to re-open or re-create the store      |
|  [08]   | `with_transaction(transaction)`                                     | bind a `Transaction` to a view              |
|  [09]   | `store[transform \| domain \| expr \| indices]`                     | virtual view by transform/domain/expr/index |
|  [10]   | `store[transform \| domain \| expr \| indices] = source`            | synchronous write into the indexed sub-view |
|  [11]   | `vindex[indices]`                                                   | vectorized-indexing virtual view            |
|  [12]   | `oindex[indices]`                                                   | outer-indexing virtual view                 |
|  [13]   | `label(*labels)`                                                    | relabel domain dimensions                   |
|  [14]   | `transpose(axes=None)` / `.T`                                       | transposed-domain view                      |
|  [15]   | `translate_to(origin)`                                              | reposition domain origin                    |
|  [16]   | `translate_by(offsets)` / `translate_backward_by(offsets)`          | shift domain origin                         |
|  [17]   | `mark_bounds_implicit(...)`                                         | toggle implicit (resizable) bound flags     |
|  [18]   | `array(dtype=None) -> numpy.ndarray`                                | synchronous NumPy interop conversion        |

[ENTRYPOINT_SCOPE]: `TensorStore` resolved-metadata properties

Each property reads the resolved schema after `open`; `origin`/`shape`/`size`, `rank`/`ndim`, and `readable`/`writable`/`mode` share one row per metadata cluster.

| [INDEX] | [PROPERTY]                       | [TYPE]                                        | [CAPABILITY]                                 |
| :-----: | :------------------------------- | :-------------------------------------------- | :------------------------------------------- |
|  [01]   | `domain`                         | `IndexDomain`                                 | bounds-and-labels domain of the array        |
|  [02]   | `origin` / `shape` / `size`      | `tuple[int, ...]` / `tuple[int, ...]` / `int` | inclusive lower bound, extent, element count |
|  [03]   | `dtype`                          | `dtype`                                       | element data type                            |
|  [04]   | `rank` / `ndim`                  | `int`                                         | dimension count                              |
|  [05]   | `schema`                         | `Schema`                                      | resolved schema                              |
|  [06]   | `chunk_layout`                   | `ChunkLayout`                                 | resolved chunk-grid layout                   |
|  [07]   | `codec`                          | `CodecSpec \| None`                           | resolved codec spec                          |
|  [08]   | `fill_value`                     | `numpy.ndarray \| None`                       | fill value for unwritten positions           |
|  [09]   | `dimension_units`                | `tuple[Unit \| None, ...]`                    | per-dimension physical units                 |
|  [10]   | `kvstore` / `base`               | `KvStore \| None` / `TensorStore \| None`     | underlying key-value store / adapter base    |
|  [11]   | `url`                            | `str`                                         | URL form of the store spec                   |
|  [12]   | `transaction`                    | `Transaction \| None`                         | bound transaction                            |
|  [13]   | `readable` / `writable` / `mode` | `bool` / `bool` / `str`                       | read/write capability flags and mode string  |

[ENTRYPOINT_SCOPE]: async rail and storage

`await future` returns the value or raises the captured error; `WriteFutures` forwards the `Future` interface to its `commit` future. `KvStore` is the byte-keyed surface the `zarr3` driver reads and writes through; `Transaction` stages modifications for an atomic commit.

| [INDEX] | [SURFACE]                                                                           | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `Future.result() -> T`                                                              | block until complete, return result         |
|  [02]   | `Future.exception() -> Exception \| None`                                           | block until complete, return error          |
|  [03]   | `Future.__await__() -> T`                                                           | async-native consumption of the result      |
|  [04]   | `Future.add_done_callback(callback)`                                                | register a completion callback              |
|  [05]   | `Future.force()`                                                                    | begin the asynchronous operation eagerly    |
|  [06]   | `Future.cancel() -> bool`                                                           | request cancellation                        |
|  [07]   | `WriteFutures.copy`                                                                 | completes when the source read finishes     |
|  [08]   | `WriteFutures.commit`                                                               | completes when the write is durably visible |
|  [09]   | `KvStore.read(key, *, byte_range, transaction, batch) -> Future[ReadResult]`        | read one key's value                        |
|  [10]   | `KvStore.write(key, value, *, transaction) -> Future[TimestampedStorageGeneration]` | write or delete one key                     |
|  [11]   | `KvStore.list(*, transaction) -> Future[list[bytes]]`                               | list keys in the store                      |
|  [12]   | `KvStore.delete_range(range) -> Future[None]`                                       | delete a key range                          |
|  [13]   | `Transaction.commit_async() -> Future[None]`                                        | asynchronously commit staged modifications  |
|  [14]   | `Transaction.abort()`                                                               | abort the transaction                       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- open axis: one `open` owns array-store acquisition; the `zarr3` driver, `kvstore`, open/create/delete_existing/assume_metadata modes, and the inline schema rows are call rows on the JSON `Spec`, never a per-mode or per-driver factory type. `array`/`cast`/`stack`/`concat`/`overlay`/`downsample`/`virtual_chunked` compose existing handles into virtual views.
- async axis: `read` returns `Future[numpy.ndarray]` and `write` returns `WriteFutures`, consumed with `await`; `WriteFutures.copy` gates source release and `WriteFutures.commit` gates durable visibility.
- storage axis: `KvStore` is the single byte-keyed backend surface (`file`/`gcs`/`s3`/`memory`/`ocdbt`); the `zarr3` driver writes through it, so backend selection is a `kvstore` row on the `Spec`.
- codec axis: `CodecSpec` owns the zarr3 codec chain — `transpose`, `bytes` (endianness), `sharding_indexed` (sub-chunk sharding into one object), `gzip`/`blosc`/`zstd` compression, `crc32c` checksum; the codec is a `Spec` row. `sharding_indexed` packs many logical chunks into one `KvStore` object, the layout virtualizarr references.
- index axis: `ts.d[...]` builds a `DimExpression` (label/range/diagonal/translate chains) applied as `store[ts.d['x','y'].translate_by[...]]`; advanced indexing, transposition, and labelling ride `DimExpression`/`IndexTransform` over a labelled domain.
- transaction axis: `Transaction(atomic=...)` stages grouped modifications behind one `commit_async`/`abort`, bound via `open(..., transaction=txn)` or `store.with_transaction(txn)`; `Batch` coalesces concurrent reads into one backend round-trip.
- context axis: one `Context` (JSON resource spec: `cache_pool`, `data_copy_concurrency`, `file_io_concurrency`, credential resources) pools cache, concurrency limits, and credentials across every open.
- metrics axis: `tensorstore.experimental` collects internal metrics in Prometheus format for the observability owner.
- evidence: each open captures driver id, kvstore url, resolved dtype, domain shape, chunk-grid layout, codec chain, and committed generation as an array-store receipt.
- boundary: tensorstore owns chunked zarr-v3 read/write, sharding, cache coherence, and the `KvStore` backend abstraction; NumPy arrays cross the wire at `read`/`write`; raster/mesh post-processing and archive packaging route to their own owners; identity/path minting stays with the runtime owner.

[STACKING]:
- `zarr`(`.api/zarr.md`): the `zarr3` driver opens the identical on-disk chunk/codec layout `zarr` writes, so a zarr-written store is a tensorstore-readable store with no conversion.
- `icechunk`(`.api/icechunk.md`): the `zarr3` driver reads the same zarr-v3 format icechunk commits for high-throughput async access over a committed snapshot.
- `virtualizarr`(`.api/virtualizarr.md`): a `zarr3` store over an `ocdbt` `KvStore` is the durable substrate a `ManifestStore` references without copy — the two meet at the zarr-v3 wire, never a bridging adapter.
- within-lib: the data store plane composes tensorstore as one `TensorBackend`, threading one shared `Context` across opens and consuming the `Future`/`WriteFutures` rail with `await` rather than re-implementing chunked codec, sharding, or cache-coherence.

[LOCAL_ADMISSION]:
- Import `tensorstore as ts` at boundary scope only; the manifest bans the module-level import.
- Drive the driver, backend, mode, and schema as rows on one JSON `Spec`; never a parallel store type per backend or a per-driver factory family.
- Consume `read`/`write` with `await`; a blocking `result()` inside the async rail is rejected.
- Share one `Context` across opens so cache pools, concurrency, and credentials pool rather than re-mint per store.

[RAIL_LAW]:
- Package: `tensorstore`
- Owns: asynchronous zarr-v3 array open/read/write over `KvStore` backends, JSON/`Spec`-driven schema/codec/chunk-layout control, `DimExpression` advanced indexing, transactional staging, virtual stack/concat/overlay/downsample/virtual_chunked views, and the `Future`/`WriteFutures` async rail
- Accept: async chunked array read/write feeding the data owner over `file`/`gcs`/`s3`/`memory`/`ocdbt` `KvStore` backends
- Reject: wrapper-renames of `open`/`read`/`write`; a blocking `result()` inside the async rail; a hand-rolled chunk codec, sharding, or cache layer; a parallel store type per backend or open mode; a per-driver factory family where one `Spec` row discriminates the driver
