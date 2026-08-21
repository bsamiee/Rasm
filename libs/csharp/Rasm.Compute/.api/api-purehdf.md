# [RASM_COMPUTE_API_PUREHDF]

`PureHDF` is the fully managed HDF5 read and write engine behind the chunked simulation-field lane: one entry class opens a file, stream, or memory-mapped accessor, traversal walks the object tree, and a dataset read projects a file-space hyperslab onto a caller-owned buffer with no native HDF5 library. Writing inverts that — an `H5File` graph encodes to a new file and `BeginWrite` defers per-dataset payloads to a live writer. `PureHDF.Filters.Lzf` and `PureHDF.Filters.BZip2.SharpZipLib` extend the filter roster managed, so the admitted set holds on every RID.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PureHDF`
- package: `PureHDF` (MIT)
- assembly: `PureHDF`
- namespace: `PureHDF`, `PureHDF.Selections`, `PureHDF.Filters`, `PureHDF.VOL.Native`
- abi: writes the HDF5 1.10 format — h5py, ParaView, and HDFView read the output, and netCDF-4 corpora read back as raw HDF5 objects
- asset: single managed AnyCPU IL assembly; the `net8.0` build binds on `net10.0`, no native payload and no RID-specific asset
- rail: scientific-array interchange

[PACKAGE_SURFACE]: `PureHDF.Filters.Lzf`
- package: `PureHDF.Filters.Lzf` (MIT)
- assembly: `PureHDF.Filters.Lzf`
- namespace: `PureHDF.Filters`
- asset: managed LZF codec, compress and decompress; depends on `PureHDF` alone
- rail: filter-pipeline row

[PACKAGE_SURFACE]: `PureHDF.Filters.BZip2.SharpZipLib`
- package: `PureHDF.Filters.BZip2.SharpZipLib` (MIT)
- assembly: `PureHDF.Filters.BZip2.SharpZipLib`
- namespace: `PureHDF.Filters`
- asset: managed BZip2 codec, compress and decompress; pulls `SharpZipLib` transitively, no manifest row of its own
- rail: filter-pipeline row

- verification: every member, default, and throw site here decompiles from the installed `net8.0` assemblies on the assay `--key purehdf` rail, cross-read against the shipped XML documentation; the accelerated-filter RID claim reads the published `Blosc2.PInvoke`, `Bitshuffle.PInvoke`, and `Intrinsics.ISA-L.PInvoke` payload layouts, which carry `linux-x64`, `win-x64`, and `win-x86` alone.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: read model — interface face and its native realization

`IH5*` interfaces carry the portable read vocabulary; `VOL.Native` classes realize them and add overloads the interface never declares.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :--------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `NativeFile`                       | class         | opened file root; `IDisposable`, `Path`, and a `NativeGroup` face |
|  [02]   | `IH5Group` / `NativeGroup`         | iface / class | link traversal — `Get`, `Children`, `LinkExists`                  |
|  [03]   | `IH5Dataset` / `NativeDataset`     | iface / class | dataset read surface with layout, space, and fill-value metadata  |
|  [04]   | `IH5Attribute` / `NativeAttribute` | iface / class | attribute read surface                                            |
|  [05]   | `IH5Object` / `NativeObject`       | iface / class | named-object floor carrying the attribute roster                  |
|  [06]   | `IH5Dataspace`                     | interface     | `Rank`, `Dimensions`, `MaxDimensions`, `Type`                     |
|  [07]   | `IH5DataType`                      | interface     | class discriminant plus the per-class detail interfaces           |
|  [08]   | `IH5DataLayout`                    | interface     | `Class` and `Chunks` — the chunk grid a read aligns to            |
|  [09]   | `IH5FillValue`                     | interface     | raw fill bytes and `GetValue<T>()`                                |
|  [10]   | `H5ReadOptions`                    | record        | field/property inclusion and name mapping for struct decode       |
|  [11]   | `H5DatasetAccess`                  | record struct | per-read chunk cache, external-file prefix, virtual prefix        |
|  [12]   | `H5LinkAccess`                     | record struct | external-link prefix                                              |

[PUBLIC_TYPE_SCOPE]: write model — the object graph an encode consumes

`H5File` is an `H5Group`, which is an `IDictionary<string, object>`: the tree is built by dictionary assignment and any CLR value admits as a dataset or attribute.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :------------------ | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `H5File`            | class         | write-graph root and the static open surface                            |
|  [02]   | `H5Group`           | class         | `IDictionary<string, object>` child map with an `Attributes` dictionary |
|  [03]   | `H5Dataset`         | class         | explicit dataset — chunk grid, selections, file dims, creation policy   |
|  [04]   | `H5Dataset<T>`      | class         | typed dataset; the `fileDims` ctor is the deferred-write placeholder    |
|  [05]   | `H5Attribute`       | class         | explicit attribute with dimensions and opaque tagging                   |
|  [06]   | `H5NativeWriter`    | class         | live writer from `BeginWrite`; `IDisposable`, finalizes the superblock  |
|  [07]   | `H5WriteOptions`    | record        | string policy, heap thresholds, user block, and the default filter list |
|  [08]   | `H5DatasetCreation` | record struct | per-dataset write chunk cache and filter list                           |
|  [09]   | `H5SoftLink`        | class         | soft link by target path                                                |
|  [10]   | `H5ObjectReference` | class         | object reference; implicit from any `H5Object`                          |
|  [11]   | `H5OpaqueInfo`      | class         | tags a `byte[]` payload opaque with a type size                         |
|  [12]   | `H5Constants`       | static class  | `Unlimited` — the `ulong.MaxValue` unlimited-dimension marker           |

[PUBLIC_TYPE_SCOPE]: selection algebra and filter pipeline

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]  | [CAPABILITY]                                                      |
| :-----: | :----------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `Selection`                                      | abstract class | walk contract — `Walk(ulong[] limits)` yielding `Step` runs       |
|  [02]   | `HyperslabSelection`                             | class          | start/stride/count/block hyperslab; the load-bearing slice form   |
|  [03]   | `PointSelection`                                 | class          | scattered coordinates from a rank-major `ulong[,]`                |
|  [04]   | `DelegateSelection`                              | class          | caller-computed step stream against a declared element count      |
|  [05]   | `AllSelection` / `NoneSelection`                 | class          | whole-space and empty-space terminals                             |
|  [06]   | `Step`                                           | record struct  | one contiguous run — coordinates plus element count               |
|  [07]   | `H5Filter`                                       | record         | filter id plus an options dictionary; implicit from a `ushort` id |
|  [08]   | `IH5Filter`                                      | interface      | `FilterId`, `Name`, `Filter`, `GetParameters` — registration face |
|  [09]   | `FilterInfo`                                     | record         | flags, parameters, chunk size, and the buffer handed to a filter  |
|  [10]   | `H5FilterFlags`                                  | enum           | `None`, `Decompress`, `SkipEdc`                                   |
|  [11]   | `IReadingChunkCache` / `SimpleReadingChunkCache` | iface / class  | bounded read-side chunk cache                                     |
|  [12]   | `IWritingChunkCache` / `SimpleWritingChunkCache` | iface / class  | write-side chunk staging                                          |
|  [13]   | `ChunkCache`                                     | static class   | the two default cache factories, both settable                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `H5File` open and traverse

Every open returns a `NativeFile`; dispose it to release the driver. File-handle and memory-mapped drivers hold their read position in a `ThreadLocal<long>`, while the stream driver reads the shared `Stream.Position` — that difference is the whole parallel-read law.

| [INDEX] | [SIGNATURE]                                                                        | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `static NativeFile OpenRead(string, H5ReadOptions? = null)`                        | read-only file-handle open, the default path        |
|  [02]   | `static NativeFile Open(string, FileMode, FileAccess, FileShare, H5ReadOptions?)`  | explicit share and access mode                      |
|  [03]   | `static NativeFile Open(Stream, bool leaveOpen = false, H5ReadOptions? = null)`    | readable and seekable stream; single-thread         |
|  [04]   | `static NativeFile Open(MemoryMappedViewAccessor, H5ReadOptions? = null)`          | memory-mapped open; parallel-read safe              |
|  [05]   | `IH5Object Get(string path)` / `GetAsync(string, CancellationToken)`               | resolve one link by absolute or relative path       |
|  [06]   | `IEnumerable<IH5Object> Children()` / `ChildrenAsync(CancellationToken)`           | enumerate direct children                           |
|  [07]   | `bool LinkExists(string path)` / `LinkExistsAsync(string, CancellationToken)`      | probe without faulting                              |
|  [08]   | `IH5Group.Group(string)` / `Dataset(string)` / `Get<T>(string)`                    | typed resolve extensions and their async peers      |
|  [09]   | `IEnumerable<IH5Attribute> Attributes()` / `Attribute(string)` / `AttributeExists` | attribute roster, fetch, and probe on any object    |
|  [10]   | `string IH5Object.Name`                                                            | the link name alone, never the path that reached it |

- `[10]` is the LINK name, so a child enumerated from a group answers its own segment and not the group-qualified path a second `Dataset(...)` resolve must rebuild — the roster-as-manifest reads at `Model/identity#MODEL_IDENTITY` `GraduationEnvelope.Admit(HdfHandle)` and `Model/sessions#SESSION_CAPSULE` `SessionPolicy.Pack` both re-qualify it against their own group prefix for that reason.

[ENTRYPOINT_SCOPE]: dataset and attribute read

`T` is the destination shape — an array, a `Memory<T>`, a scalar, a struct, or a class the options map. `memoryDims` is required whenever `memorySelection` is set. `Span<T>` and `H5DatasetAccess` overloads live on `NativeDataset` alone, so a caller-owned buffer or a per-read cache demands the concrete type, never the `IH5Dataset` face.

| [INDEX] | [SIGNATURE]                                                                               | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `T Read<T>(Selection? file = null, Selection? memory = null, ulong[]? memoryDims = null)` | allocating read of a file selection   |
|  [02]   | `void Read<T>(T buffer, Selection?, Selection?, ulong[]?)`                                | read into an existing array or memory |
|  [03]   | `void Read<T>(Span<T> buffer, Selection?, Selection?, ulong[]?)`                          | caller-owned span; zero allocation    |
|  [04]   | `void Read<T>(H5DatasetAccess, Span<T>, Selection?, Selection?, ulong[]?)`                | span read under a per-read cache      |
|  [05]   | `Task<T> ReadAsync<T>(Selection?, Selection?, ulong[]?, CancellationToken)`               | async allocating read                 |
|  [06]   | `Task ReadAsync<T>(T buffer, Selection?, Selection?, ulong[]?, CancellationToken)`        | async read into a buffer              |
|  [07]   | `T Read<T>(ulong[]? memoryDims = null)` / `void Read<T>(T buffer, ulong[]?)`              | attribute read, allocating or into    |
|  [08]   | `void Read<T>(Span<T> buffer, ulong[]? memoryDims = null)`                                | attribute read into a caller span     |
|  [09]   | `Layout.Class` / `Layout.Chunks` / `Space.Dimensions` / `Type.Class`                      | layout and shape metadata pre-read    |

[ENTRYPOINT_SCOPE]: `HyperslabSelection` construction

Rank-wise construction is the canonical form: `starts` is the origin, `strides` the inter-block pitch, `counts` the block repeat, `blocks` the per-block extent. Three-array construction derives a stride equal to the block (a contiguous slab) and a unit count.

| [INDEX] | [SIGNATURE]                                                                                | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `HyperslabSelection(int rank, ulong[] starts, ulong[] blocks)`                             | contiguous n-dim slab                      |
|  [02]   | `HyperslabSelection(int, ulong[] starts, ulong[] strides, ulong[] counts, ulong[] blocks)` | strided repeating slab                     |
|  [03]   | `HyperslabSelection(ulong start, ulong block)`                                             | rank-1 contiguous run                      |
|  [04]   | `HyperslabSelection(ulong start, ulong stride, ulong count, ulong block)`                  | rank-1 strided run                         |
|  [05]   | `Rank` / `TotalElementCount`                                                               | shape probes the read buffer sizes against |
|  [06]   | `PointSelection(ulong[,] points)`                                                          | scattered coordinates, rank-major          |
|  [07]   | `DelegateSelection(ulong totalElementCount, Func<ulong[], IEnumerable<Step>>)`             | computed step stream                       |

[ENTRYPOINT_SCOPE]: write

`Write` encodes the whole graph and closes; `BeginWrite` encodes it and hands back the live writer so a `H5Dataset<T>` declared with `fileDims` alone takes its payload later, chunk by chunk. Both overwrite the target unconditionally.

| [INDEX] | [SIGNATURE]                                                             | [CAPABILITY]                             |
| :-----: | :---------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `void Write(string filePath, H5WriteOptions? = null)`                   | encode and close; creates or truncates   |
|  [02]   | `void Write(Stream, H5WriteOptions? = null)`                            | encode into a read-write seekable stream |
|  [03]   | `H5NativeWriter BeginWrite(string filePath, H5WriteOptions?)`           | encode, return the live writer           |
|  [04]   | `H5NativeWriter BeginWrite(Stream, H5WriteOptions?)`                    | same over a stream                       |
|  [05]   | `void H5NativeWriter.Write<T>(H5Dataset<T>, T, Selection?, Selection?)` | deferred per-dataset payload write       |
|  [06]   | `group[name] = value` / `group.Attributes[name] = value`                | implicit dataset and attribute admission |

[H5Dataset]: `H5Dataset(object data, uint[]? chunks = null, Selection? memorySelection = null, Selection? fileSelection = null, ulong[]? fileDims = null, H5DatasetCreation datasetCreation = default, H5OpaqueInfo? opaqueInfo = null)` mints the explicit dataset; `H5Dataset<T>(ulong[] fileDims, uint[]? chunks = null, H5DatasetCreation = default, H5OpaqueInfo? = null)` declares shape alone and takes its payload through the writer, and `H5Dataset<T>(T data, uint[]? chunks = null, ulong[]? fileDims = null, …)` preserves a nullable value type the `object` ctor erases.

[H5Attribute]: `H5Attribute(object data, ulong[]? dimensions = null, H5OpaqueInfo? opaqueInfo = null)`, with `H5Attribute<T>(T data, ulong[]? dimensions = null, H5OpaqueInfo? = null)` preserving the same nullable-value-type detail.

[ENTRYPOINT_SCOPE]: filter pipeline and chunk caches

`H5Filter` registration is a process-static `ConcurrentDictionary` seeded with five built-ins; the Lzf and BZip2 rows register explicitly. Any chunk reaching an unregistered filter faults the read by id.

| [INDEX] | [SURFACE]                                             |  [ID]   | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------------- | :-----: | :------------------------------------------------ |
|  [01]   | `H5Filter.Register(IH5Filter)`                        |   n/a   | registers or replaces one filter by its id        |
|  [02]   | `H5Filter.ResetRegistrations()`                       |   n/a   | restores the five built-in registrations          |
|  [03]   | `DeflateFilter` (`COMPRESSION_LEVEL`)                 |   `1`   | zlib via `ZLibStream`; round-trip                 |
|  [04]   | `ShuffleFilter`                                       |   `2`   | byte shuffle, vectorized; round-trip              |
|  [05]   | `Fletcher32Filter`                                    |   `3`   | trailing checksum; round-trip, `SkipEdc` bypasses |
|  [06]   | `NbitFilter`                                          |   `5`   | registered but throws on both directions          |
|  [07]   | `ScaleOffsetFilter`                                   |   `6`   | decompress only; compress throws                  |
|  [08]   | `LzfFilter`                                           | `32000` | round-trip; register explicitly                   |
|  [09]   | `BZip2SharpZipLibFilter` (`BLOCK_SIZE`)               |  `307`  | round-trip; register explicitly                   |
|  [10]   | `SimpleReadingChunkCache(int = 521, ulong = 1048576)` |   n/a   | slot- and byte-bounded LRU read cache             |
|  [11]   | `SimpleWritingChunkCache()`                           |   n/a   | unbounded write staging until `Flush`             |
|  [12]   | `ChunkCache.DefaultReadingChunkCacheFactory`          |   n/a   | settable factory, defaults to 521 slots and 1 MiB |
|  [13]   | `ChunkCache.DefaultWritingChunkCacheFactory`          |   n/a   | settable factory for the write-side cache         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Byte order gates a read, never converts it: a dataset or attribute whose element byte order differs from the host refuses with `Byte order conversion is not (yet) support by PureHDF.`, a VAX-endian type refuses outright, and opening on a big-endian host refuses before the superblock parses. Big-endian corpora therefore re-encode upstream and never decode here — an endianness converter ships in the assembly that no read path calls.
- Element width gates a read the same way — probe-proven: `Read<T>` never width-converts, a float32 destination over a float64 dataset and the reverse both refuse with `Unable to decode values types of different type size.`, so narrowing and widening are caller-side after a same-width read.
- Virtual datasets read — probe-proven: an h5py-authored VDS surfaces as a `NativeDataset` with `Layout.Class == VirtualStorage` and resolves whole and hyperslab reads across source boundaries; relative source paths resolve beside the containing file independent of process cwd, `H5DatasetAccess.VirtualPrefix` supplements that resolution, and an unresolvable source region yields the dataset's declared fill value, never a fault. The write model carries no virtual layout, so VDS is read-side only.
- `H5Constants.Unlimited` in a `H5Dataset<T>` `fileDims` faults `BeginWrite` at encode — probe-proven, the dataspace byte sizing overflows before any chunk write — so deferred writes declare fixed extents; a fixed oversized extent is lawful, unwritten chunks reading back as the fill value (0 default) here and under h5py.
- Writing creates only: `Write` and `BeginWrite` truncate the target, and the surface carries no append, no in-place edit, and no re-open-for-write. Incremental result sets accumulate inside one `BeginWrite` session or re-encode whole.
- Chunks write once. `Chunks can only be written once.` throws from the v4 chunk index when a chunk already flushed to disk is touched again, and the earlier B-tree index rejects chunked writes entirely. Revisiting chunks out of order therefore faults mid-encode, so every write walks chunk-aligned in index order.
- Write-side chunk staging is unbounded — `SimpleWritingChunkCache` holds every touched chunk in a plain dictionary until `Flush`, so peak write memory equals distinct chunks touched before disposal times the chunk size. Bounding it through a custom `IWritingChunkCache` trades that ceiling for the write-once fault above, which makes chunk-aligned writing the law rather than a smaller cache.
- Read-side chunk caching is bounded and settable: 521 slots and 1 MiB by default, per-read through `H5DatasetAccess.ChunkCache` and process-wide through `ChunkCache.DefaultReadingChunkCacheFactory`. Working sets exceeding the cache re-decompress on every miss, so cache size follows the slab shape a read walks.
- Filter coverage carries holes: SZIP (id `4`) is absent entirely, N-Bit registers yet throws in both directions, and Scale-Offset decompresses only. Corpora carrying any of the three block on exactly that filter, and no managed substitute ships in the package family.
- Deflate levels split across two gates: `GetParameters` admits `-1..9` and stores `6` for the unset default, while the compress path maps only `{-1, 0, 1, 9}` onto `CompressionLevel` and throws on the rest. Levels `2`-`5`, `7`, and `8` therefore pass dataset construction and fault at the first chunk compress, so policy admits the four-value set alone.
- Parallel reads hold from the file-path and memory-mapped entry points, whose drivers keep position in a `ThreadLocal<long>`; `Open(Stream, …)` shares one `Stream.Position` across threads and reads single-reader by construction. Concurrent fan-out therefore binds `OpenRead(string)` or the memory-mapped accessor, never a caller-supplied stream.
- Shuffle with Deflate is the h5py-compatible pipeline: filter ids `2` and `1` are the registered HDF5 identifiers, so a dataset written with both reads back under `h5py` as `compression='gzip', shuffle=True` and the reverse holds. Output lands in HDF5 1.10 format, which ParaView and HDFView open directly.
- netCDF-4 is HDF5 underneath and opens on the same path, yet PureHDF surfaces raw HDF5 groups, datasets, and attributes — dimension scales, coordinate variables, and the netCDF type model stay uninterpreted, so netCDF semantics resolve above this rail.
- Accelerated filter packages — Blosc2, Bitshuffle, and Deflate ISA-L — carry native payloads published for `linux-x64`, `win-x64`, and `win-x86` alone, so none arms on the branch osx target. `Lzf` and `BZip2.SharpZipLib` are therefore the complete admitted filter extension, identical on every RID.

[STACKING]:
- `Runtime/archive#HDF_ARCHIVE`: THE branch archive owner — `HdfArchive.Mount/Session/Open/Fan/Begin`, `HdfSource`, `HdfHandle` (concrete `NativeDataset`/`NativeGroup` resolve, both on `Fin`), `HdfArchivePolicy` (`DeflateGrade` four-value closed vocabulary beside the rank-ordered `FilterStage` set) — every other row on this list composes it and none opens an `H5File` of its own.
- `Runtime/archive#CHUNK_CURSOR`: `ChunkGrid` is the one station-outermost `(FileDims, Chunks)` derivation every producer composes, and the same value answers the ordinal↔hyperslab projection a write takes and the byte slice a random-access read takes; `ArchiveSession.Open`/`Write` is THE declared-write capsule — `H5Dataset<T[]>` mint off `H5DatasetCreation`, `H5File` graph seat, `Attributes[name]` stamp through the closed `ArchiveAttribute` vocabulary, `BeginWrite`, and release fold into one act, so no artifact class re-spells the five steps; `session.Cursor(ArchiveSlot<T>)` mints the per-slot `ChunkCursor<T>` that OWNS the monotone ordinal, so an out-of-order chunk is unrepresentable rather than refused and the library's mid-encode `Chunks can only be written once.` fault is unreachable.
- `Runtime/field#FIELD_RESULT_CODEC`: `FieldCodec.Hdf5Decode`/`Hdf5Encode` read and emit the station×component chunk model as an HDF5 1.10 container — ONE dataset with the trailing COMPONENT axis (a dataset per component is the refuted sibling layout, forking the chunk address a consumer computes) — seating the container's own chunk grid through `ChunkGrid.Seat` and deriving one through `ChunkGrid.Derive` for a contiguous corpus; `FieldElement` is the one `(H5DataTypeClass, Size)` element gate both float ingests read.
- `Runtime/field#SCIENTIFIC_INGEST`: `InterchangeIo.ImportWaveforms` reads `[samples, channels]` SHM and reference-bank corpora into `WaveformCorpus` under the frame/hop hyperslab law the admitted `WaveformWindow` carries (hop at or above frame rides one strided selection, overlap walks per-frame slabs), `Stats/signal` the storing-nothing consumer; `InterchangeIo.ImportField` brackets its payload-source container through `HdfArchive.Session`.
- `Runtime/scheduling#JOB_GRAPH`: `ShardPartition.ArchiveBlocks` is the corpus-backed shard block provider — one bracketed `HdfArchive.Session` per call (the archive node-key law: one NativeFile per job, so parallel shards never share a driver), the row block reading as ONE rank-2 `HyperslabSelection(2, [start, 0], [height, cols])` through `NativeDataset.Read<double>(H5DatasetAccess, Span<T>, …)` so the partition never stages the full operator, and the dataset resolving on `Fin` rather than dereferencing.
- `Solver/route#SOLVE_ARCHIVE`: `SolveArchive` and its three container sessions — `SolveHistory` `[steps, dofs]` one chunk per accepted step, `SolveModes` `[pairs, dofs]` mode-outermost with eigenvalue/participation/condensation attributes, and one create-only `SolveCheckpoint` container per committed arc step.
- `Solver/field#DISCRETE_FIELD`: `DiscreteMesh.Archive` writes the mesh container through `Begin` under the same chunk-aligned law.
- `Solver/uncertainty#FORWARD_UQ`: `EnsembleSeal` lands the sampled design matrix beside its responses as one ensemble container per campaign.
- `Solver/clash#CLASH_AND_TWIN`: `TwinLoop` segments its accumulating twin evidence at the producer's own cadence edge — one create-only session per segment, the no-append law's sanctioned accumulation form.
- `Model/identity#MODEL_IDENTITY`: `GraduationEnvelope.Admit(HdfHandle)` ingests the h5py-written `/bands/<feature>` reference roster under declared selections — the forward graduation seam.
- `Model/sessions#SESSION_CAPSULE`: `SessionPolicy.Pack` loads the `/initializers` group (children = roster) through `TensorVocabulary.Admit(IH5DataType)` and `Space.Dimensions` gates, content-keying the staging span before `TensorBridge.Ingress`.
- `Model/tiling#TILE_FOLD` + `Model/run#RUN_MODES`: the tiled mosaic accumulates every field in a pooled host arena and SPILLS NOTHING, so this library reaches it only through the `PlaneFill`/`WindowFill` delegate seams — an archive-resident plane or chunk window fills a caller-owned span through a filler the composition root binds over `ArchiveSession.Open`/`Cursor`, and PureHDF lands on no Compute signature at either.
- `Tensor/vocabulary#TENSOR_VOCABULARY`: `TensorVocabulary.Admit(IH5DataType)` maps `H5DataTypeClass`+`Size`(+`IFixedPointType.IsSigned`) onto the dtype rows; the interface face carries no byte order, so endian refusal stays at the archive read.
- `Tensor/factor#SPARSE_SOLVE`: `ReadArchive`/`WriteArchive` — the scipy sparse group convention beside `.mtx`, the write carrying the kind/ordering/fill/frobenius/symmetric attributes MatrixMarket cannot.
- `Tensor/sampling#OWNED_BUILDS`: the `JoeKuo` embedded HDF5 resource read per Sobol construction (Payload source, rank-2 `/seeds` hyperslab) and the `Replicates` response corpus (one chunk per replicate, regenerating-state attributes).
- `Tensor/quadrature#TRAJECTORY_DRIVER`: the `TraceSpill` station stream — `[Stations.Count, width]`, fileDims known at Admit, monotone station cursor as the chunk-once law.
- `Tensor/dispatch#EQUIVALENCE_INTEROP`: `TapeSpill` — the `[steps, width]` step-chunked reverse-mode primal spill: one declared slot on the `ArchiveSession` capsule, forward `Push` under the seated `ChunkCursor` ordinal, reverse per-step hyperslab `Replay` with one chunk resident, fileDims from the declared step count (the unlimited-dimension write faults at encode), undeclarable counts segmenting one create-only session per segment.
- `Tensor/blas#BASIS_ARTIFACT`: `BasisArtifact` — the sketch/modal/rbf row axis, one column-axis-chunked writer, rank-truncated hyperslab read-back.
- `Analysis/daylight#SKY_AND_SHADOW`: the `WeatherSource.Gridded` netCDF-4 row (rank-3 single-cell annual hyperslabs, CF `units`/`calendar` gated above the rail) and the annual `[targets, hours]` irradiance matrix artifact.
- `Analysis/capacity#DESIGN_CHECK`: the shared demands/modal artifact — `/demands` member rows for both routes (one `Span2D` block, the column layout the `MemberResponse` record declares), the seismic `/modes` `[modes, dofs]` chunked mode-outermost beside `/periods`.
- `Runtime/scheduling#JOB_GRAPH`: `ShardPartition.ArchiveBlocks` — one rank-2 hyperslab per shard row block, one Path handle per call under the archive node-key law.
- `Microsoft.IO.RecyclableMemoryStream`(`.api/api-recyclable-stream.md`): reads target a rented buffer's `Span<T>` through the `NativeDataset.Read<T>(Span<T>, …)` overload, so a decoded chunk lands in the pooled staging block the frame law already owns and never mints a second array.
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing.md`): `XxHash128` keys each decoded chunk on the Persistence blob lane, computed over the read span before it leaves the staging buffer.
- within-lib: archive reads and writes run as `Runtime/scheduling` job-graph nodes keyed by the corpus content key and the declared selection, one `NativeFile` per job disposed at completion; filter faults, byte-order refusals, and chunk write-once faults each project to a typed `ComputeFault` wearing the `<hdf5-…>` slug grammar on the `Runtime/receipts` rail, never a raw exception.

[LOCAL_ADMISSION]:
- `Runtime/archive#HDF_ARCHIVE` is the ONE consumption owner — every open, selection read, deferred chunked write, and filter registration in the Compute closure crosses `HdfArchive`/`HdfHandle`/`HdfWriter`, and the composing clusters above reach the library only through them; a second `H5File` open surface anywhere in the package is the rejected form.
- One `NativeFile` per read job, disposed at the job boundary; a long-lived open handle across jobs is the rejected form because driver, chunk cache, and global-heap map all hang off it.
- Filter registration runs once at `HdfArchive.Mount` — never per read, because the registry is process-static and a per-call register writes a shared `ConcurrentDictionary` on the hot path.
- Every read declares its selection: an unqualified whole-dataset read of a screening-scale corpus refuses at admission, since the file space is the corpus and the memory buffer bounds to the lane staging policy.
- A folder needing HDF5 outside this package admits the library for its own seam and carries its own catalog; archive artifacts leave content-addressed through `ArtifactIndexRow.Admit` on the Persistence blob lane, never a Compute-side file catalog.

[RAIL_LAW]:
- Package: `PureHDF`, `PureHDF.Filters.Lzf`, `PureHDF.Filters.BZip2.SharpZipLib`
- Owns: managed HDF5 file read and write, group and attribute traversal, hyperslab and point selections, the chunked filter pipeline, and the read/write chunk caches
- Accept: little-endian HDF5 and netCDF-4 corpora read through bounded selections into caller-owned buffers, h5py-authored virtual datasets under whole or hyperslab selections with missing-source regions yielding the declared fill value, and create-only chunk-aligned writes under the Shuffle/Deflate/Fletcher32/Lzf/BZip2 filter set
- Reject: big-endian corpora, cross-width element decode (a float32 read of a float64 dataset and the reverse refuse by type size), append or in-place edit of an existing file, `H5Constants.Unlimited` file dimensions on the write path (`BeginWrite` faults at encode), out-of-order chunk writes, SZIP/N-Bit payloads and Scale-Offset compression, deflate levels outside `{-1, 0, 1, 9}`, concurrent reads over a caller-supplied `Stream`, and the accelerated filter packages whose natives never reach the branch RID
