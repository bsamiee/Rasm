# [RASM_COMPUTE_API_ALIMER_MESHOPTIMIZER]

`Alimer.Bindings.MeshOptimizer` projects Arseny Kapoulkine's native `meshoptimizer` through a
single source-generated P/Invoke surface (`MeshOptimizer.Meshopt`): vertex remap, cache/overdraw/
fetch optimization, the index+vertex+sequence buffer codec with explicit format-version pinning,
octahedral/quaternion/exponent/color attribute filters, triangle-strip generation, attribute-aware
and in-place LOD simplification, meshlet/cluster construction with cone/sphere bounds, spatial
sorting and clustering, and quantization for GPU-ready geometry and `EXT_meshopt_compression`
glTF interchange.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Alimer.Bindings.MeshOptimizer`
- package: `Alimer.Bindings.MeshOptimizer` `1.2.1`
- assembly: `Alimer.Bindings.MeshOptimizer` (consumer-bound `lib/net10.0`; also ships `lib/net9.0`)
- namespace: `MeshOptimizer`
- license: MIT (managed binding by Amer Koleci; native `meshoptimizer` MIT by Arseny Kapoulkine)
- asset: managed bindings + native `meshoptimizer` runtime; `runtimes/<rid>/native/` ships `osx`
  (single fat dylib, arm64+x64), `linux-x64`, `linux-arm64`, `android-{arm,arm64,x64}`,
  `win-x64`, `win-arm64`. The Rasm consumer RID is `osx-arm64`; `libmeshoptimizer.dylib` is fat.
- abi: `Meshopt.MESHOPTIMIZER_VERSION = 1000u`; `Meshopt.LibraryName = "meshoptimizer"`
- build-floor: net9.0 (no netstandard fallback); `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` is
  mandatory — every entry point is `unsafe extern` over raw pointers
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: entry-point, options, and resolver family
- rail: geometry

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [RAIL]                                        |
| :-----: | :----------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `Meshopt`                | static bindings | every meshoptimizer P/Invoke operation        |
|  [02]   | `Meshopt.ResolveLibrary` | event           | `DllImportResolver?` for embedded native load |
|  [03]   | `SimplificationOptions`  | `[Flags]` enum  | LOD simplification control flags              |
|  [04]   | `EncodeExpMode`          | enum            | exponent filter sharing mode                  |

[PUBLIC_TYPE_SCOPE]: data carrier family (all `struct`, returned/passed by value)
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                                                           |
| :-----: | :---------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `Stream`                | vertex stream  | primary-ctor `(void* data, nuint size, nuint stride)` remap      |
|  [02]   | `Meshlet`               | meshlet struct | `uint vertex_offset/triangle_offset/vertex_count/triangle_count` |
|  [03]   | `Bounds`                | bounds struct  | `fixed float center[3]`, `radius`, cone apex/axis/cutoff + s8    |
|  [04]   | `VertexCacheStatistics` | stat struct    | ACMR / ATVR vertex-cache ratios                                  |
|  [05]   | `VertexFetchStatistics` | stat struct    | overfetch ratio + bytes fetched                                  |
|  [06]   | `OverdrawStatistics`    | stat struct    | pixels-shaded / pixels-covered overdraw ratio                    |
|  [07]   | `CoverageStatistics`    | stat struct    | per-axis coverage from `AnalyzeCoverage`                         |

## [03]-[ENTRYPOINTS]

All pointer parameters are `uint*` (indices/remap), `void*` (interleaved vertices), `float*`
(positions/attributes), or `byte*` (triangle buffers/codec/lock); `nuint` carries sizes and
counts. `nuint`-returning ops return a written element count; `int`-returning decoders return a
status code (0 = ok). Stride/size are byte quantities. Pin managed arrays (`fixed` / pinned
`Span<T>` / `MemoryHandle`) before every call.

[ENTRYPOINT_SCOPE]: remap, indexing, and topology generation
- rail: geometry
- call: index ops take the head `(dst, indices, index_count, …)` then a vertex source (`void* vertices`, `float* positions`, or `Stream* streams`); `…` in a row stands for that head. Delegate params take a `[UnmanagedCallersOnly]` static address.

| [INDEX] | [SURFACE]                                                                               | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `nuint GenerateVertexRemap(…, vertices, vertex_count, vertex_size)`                     | dedup remap over interleaved vertices |
|  [02]   | `nuint GenerateVertexRemapMulti(…, vertex_count, streams, stream_count)`                | multi-stream (de-interleaved) remap   |
|  [03]   | `nuint GenerateVertexRemapCustom(…, positions, vertex_count, stride, cb, ctx)`          | predicate-driven weld via callback    |
|  [04]   | `void GeneratePositionRemap(dst, positions, vertex_count, stride)`                      | position-only welding remap           |
|  [05]   | `void RemapVertexBuffer(dst, vertices, vertex_count, vertex_size, remap)`               | apply remap to vertex buffer          |
|  [06]   | `void RemapIndexBuffer(dst, indices, index_count, remap)`                               | apply remap to index buffer           |
|  [07]   | `void GenerateShadowIndexBuffer(…, vertices, vertex_count, vertex_size, vertex_stride)` | position-equal shadow IB              |
|  [08]   | `void GenerateShadowIndexBufferMulti(…, vertex_count, streams, stream_count)`           | multi-stream shadow IB                |
|  [09]   | `void GenerateAdjacencyIndexBuffer(…, positions, vertex_count, stride)`                 | triangle-adjacency IB for GS          |
|  [10]   | `void GenerateTessellationIndexBuffer(…, positions, vertex_count, stride)`              | PN-tessellation IB                    |
|  [11]   | `nuint GenerateProvokingIndexBuffer(dst, reorder, indices, index_count, vertex_count)`  | flat-shading provoking-vertex reindex |

[ENTRYPOINT_SCOPE]: cache, overdraw, fetch optimization, and strip generation
- rail: geometry
- call: cache/overdraw/fetch ops write reordered indices into `dst` from `(indices, index_count, …)`; strip ops take `(dst, indices, index_count, …)` with a `restart_index`.

| [INDEX] | [SURFACE]                                                                                      | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `void OptimizeVertexCache(dst, indices, index_count, vertex_count)`                            | tipsify cache optimization          |
|  [02]   | `void OptimizeVertexCacheStrip(dst, indices, index_count, vertex_count)`                       | strip-friendly cache optimization   |
|  [03]   | `void OptimizeVertexCacheFifo(dst, indices, index_count, vertex_count, cache_size)`            | FIFO cache (legacy HW) optimization |
|  [04]   | `void OptimizeOverdraw(dst, indices, index_count, positions, vertex_count, stride, threshold)` | overdraw minimization (post-cache)  |
|  [05]   | `nuint OptimizeVertexFetch(dst, indices, index_count, vertices, vertex_count, vertex_size)`    | reorder vertices for fetch locality |
|  [06]   | `nuint OptimizeVertexFetchRemap(dst, indices, index_count, vertex_count)`                      | fetch-remap table (apply via remap) |
|  [07]   | `void OptimizeMeshlet(meshlet_vertices, meshlet_triangles, triangle_count, vertex_count)`      | per-meshlet local cache optimize    |
|  [08]   | `nuint Stripify(dst, indices, index_count, vertex_count, restart_index)`                       | triangle strip with restart index   |
|  [09]   | `nuint StripifyBound(index_count)`                                                             | max stripified index count          |
|  [10]   | `nuint Unstripify(dst, indices, index_count, restart_index)`                                   | strip → triangle list expansion     |
|  [11]   | `nuint UnstripifyBound(index_count)`                                                           | max unstripified index count        |

[ENTRYPOINT_SCOPE]: index/vertex/sequence codec with format-version pinning
- rail: geometry
- call: encode ops write into `(byte* buffer, nuint buffer_size)` and return the written byte count; decode ops read that pair into `dst` and return an `int` status (0 = ok); `*Bound` ops size the destination; version ops are process-global.

| [INDEX] | [SURFACE]                                                                                       | [CAPABILITY]                        |
| :-----: | :---------------------------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `nuint EncodeIndexBuffer(…, indices, index_count)`                                              | compress index buffer               |
|  [02]   | `nuint EncodeIndexBufferBound(index_count, vertex_count)`                                       | encode dst sizing (allocate ≥ this) |
|  [03]   | `int DecodeIndexBuffer(dst, index_count, index_size, …)`                                        | decompress index buffer             |
|  [04]   | `void EncodeIndexVersion(int version)`                                                          | pin index codec format version      |
|  [05]   | `int DecodeIndexVersion(…)`                                                                     | read encoded index format version   |
|  [06]   | `nuint EncodeIndexSequence(…, indices, index_count)`                                            | compress arbitrary index sequence   |
|  [07]   | `nuint EncodeIndexSequenceBound(index_count, vertex_count)`                                     | sequence encode dst sizing          |
|  [08]   | `int DecodeIndexSequence(dst, index_count, index_size, …)`                                      | decompress index sequence           |
|  [09]   | `nuint EncodeVertexBuffer(…, vertices, vertex_count, vertex_size)`                              | compress vertex buffer              |
|  [10]   | `nuint EncodeVertexBufferLevel(…, vertices, vertex_count, vertex_size, int level, int version)` | level+version-pinned vertex encode  |
|  [11]   | `nuint EncodeVertexBufferBound(vertex_count, vertex_size)`                                      | vertex encode dst sizing            |
|  [12]   | `int DecodeVertexBuffer(dst, vertex_count, vertex_size, …)`                                     | decompress vertex buffer            |
|  [13]   | `void EncodeVertexVersion(int version)`                                                         | pin vertex codec format version     |
|  [14]   | `int DecodeVertexVersion(…)`                                                                    | read encoded vertex format version  |

[ENTRYPOINT_SCOPE]: attribute filters (octahedral / quaternion / exponent / color)
- rail: geometry

| [INDEX] | [SURFACE]                                                                             | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------------ | :--------------------------------------- |
|  [01]   | `void DecodeFilterOct(buffer, count, stride)`                                         | unpack octahedral normals/tangents       |
|  [02]   | `void DecodeFilterQuat(buffer, count, stride)`                                        | unpack quaternion rotations              |
|  [03]   | `void DecodeFilterExp(buffer, count, stride)`                                         | unpack shared-exponent floats            |
|  [04]   | `void DecodeFilterColor(buffer, count, stride)`                                       | unpack quantized colors                  |
|  [05]   | `void EncodeFilterOct(dst, count, stride, int bits, float* data)`                     | pack octahedral normals (4/8-byte)       |
|  [06]   | `void EncodeFilterQuat(dst, count, stride, int bits, float* data)`                    | pack quaternions                         |
|  [07]   | `void EncodeFilterExp(dst, count, stride, int bits, float* data, EncodeExpMode mode)` | pack floats with `EncodeExpMode` sharing |
|  [08]   | `void EncodeFilterColor(dst, count, stride, int bits, float* data)`                   | pack colors                              |

`EncodeExpMode`: `None`/`EncodeExpSeparate` (0, per-value), `EncodeExpSharedVector` (1, per-vector),
`EncodeExpSharedComponent` (2, per-component column), `EncodeExpClamped` (3).

[ENTRYPOINT_SCOPE]: LOD simplification and quantization
- rail: geometry
- call: the `Simplify*` ops take the head `(dst, indices, index_count, positions, vertex_count, stride, …)`, return the written index count, write `result_error`, and gate on `(target_index_count, target_error, options)`. The attribute simplifiers take `(attributes, attr_stride, attr_weights, attr_count, vertex_lock)`; point ops take `(dst, positions, vertex_count, stride, …)`. `vertex_lock` is an optional `byte*` mask (1 = pinned).

| [INDEX] | [SURFACE]                                                                              | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `nuint Simplify(…, target_index_count, target_error, options, result_error)`           | quadric LOD; writes achieved error    |
|  [02]   | `nuint SimplifyWithAttributes(…, attributes, attr_weights, vertex_lock, …)`            | attribute-aware quadric (production)  |
|  [03]   | `nuint SimplifyWithUpdate(indices, …, attributes, attr_weights, vertex_lock, …)`       | in-place simplify (mutates `indices`) |
|  [04]   | `nuint SimplifySloppy(…, vertex_lock, target_index_count, target_error, result_error)` | topology-ignoring aggressive LOD      |
|  [05]   | `nuint SimplifyPoints(…, colors, colors_stride, color_weight, target_vertex_count)`    | point-cloud decimation                |
|  [06]   | `nuint SimplifyPrune(…, target_error)`                                                 | drop components under error threshold |
|  [07]   | `float SimplifyScale(positions, vertex_count, stride)`                                 | absolute-error scale factor           |
|  [08]   | `ushort QuantizeHalf(float v)` / `float DequantizeHalf(ushort h)`                      | IEEE half round-trip                  |
|  [09]   | `float QuantizeFloat(float v, int N)`                                                  | reduce float to N-bit mantissa        |

`SimplificationOptions` `[Flags]`: `None` (0), `SimplifyLockBorder` (1, freeze open edges),
`meshopt_SimplifySparse` (2, sparse attribute discontinuity reduction), `meshopt_SimplifyErrorAbsolute`
(4, interpret `target_error` in world units scaled by `SimplifyScale`). `vertex_lock` is an optional
`byte*` mask (1 = pinned) shared across the attribute-aware simplifiers.

[ENTRYPOINT_SCOPE]: meshlet, cluster, spatial, and analysis
- rail: geometry
- call: meshlet builds take the output arrays `(meshlets, meshlet_vertices, meshlet_triangles)` then the source `(indices, index_count, positions, vertex_count, stride)` and `max_vertices`/`max_triangles` sizing; `Analyze*` take `(indices, index_count, …)` and return their stat struct by value. `PartitionClusters` takes `(dst, cluster_indices, cluster_index_counts, cluster_count, …, target_partition_size)`. The tail shown is the discriminator.

| [INDEX] | [SURFACE]                                                                               | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `nuint BuildMeshlets(…, cone_weight)`                                                   | cone-weighted meshlet build           |
|  [02]   | `nuint BuildMeshletsFlex(…, min_triangles, cone_weight, split_factor)`                  | variable-size meshlets (min..max)     |
|  [03]   | `nuint BuildMeshletsSpatial(…, min_triangles, fill_weight)`                             | spatial-locality meshlet build        |
|  [04]   | `nuint BuildMeshletsScan(…, vertex_count)`                                              | fast scan build (no positions)        |
|  [05]   | `nuint BuildMeshletsBound(index_count, max_vertices, max_triangles)`                    | meshlet/vertex/triangle buffer sizing |
|  [06]   | `Bounds ComputeClusterBounds(indices, index_count, positions, vertex_count, stride)`    | cone + sphere bounds for index range  |
|  [07]   | `Bounds ComputeMeshletBounds(meshlet_vertices, meshlet_triangles, triangle_count, …)`   | per-meshlet cone/sphere bounds        |
|  [08]   | `Bounds ComputeSphereBounds(positions, count, stride, radii, radii_stride)`             | bounding sphere over (optional) radii |
|  [09]   | `nuint PartitionClusters(…)`                                                            | spatial cluster→group partitioning    |
|  [10]   | `void SpatialSortRemap(dst, positions, vertex_count, stride)`                           | Morton-order vertex remap             |
|  [11]   | `void SpatialSortTriangles(dst, indices, index_count, positions, vertex_count, stride)` | spatially coherent triangle order     |
|  [12]   | `void SpatialClusterPoints(dst, positions, vertex_count, stride, cluster_size)`         | spatial point clustering              |
|  [13]   | `VertexCacheStatistics AnalyzeVertexCache(…, cache_size, warp_size, primgroup_size)`    | ACMR/ATVR cache stats                 |
|  [14]   | `VertexFetchStatistics AnalyzeVertexFetch(…, vertex_size)`                              | overfetch stats                       |
|  [15]   | `OverdrawStatistics AnalyzeOverdraw(…, positions, vertex_count, stride)`                | overdraw ratio stats                  |
|  [16]   | `CoverageStatistics AnalyzeCoverage(…, positions, vertex_count, stride)`                | per-axis coverage stats               |
|  [17]   | `void SetAllocator(allocate, deallocate)`                                               | route native scratch through a pool   |

## [04]-[IMPLEMENTATION_LAW]

[NATIVE_TOPOLOGY]:
- one `static class Meshopt`; each method carries both `[DllImport(...)]` and `[LibraryImport(...)]`
  source-generated marshalling over the native `meshoptimizer` library.
- native names by OS: `meshoptimizer.dll` (Windows), `libmeshoptimizer.so` (Linux/Android),
  `libmeshoptimizer.dylib` (macOS). For self-contained or embedded deployment register a handler on
  the `Meshopt.ResolveLibrary` event (`DllImportResolver`) before the first call.
- `Stream` is `(void* data, nuint size, nuint stride)`: `data` points at the first attribute of a
  de-interleaved vertex stream, `size` is the per-vertex byte span the remap hashes, `stride` is the
  advance. Multi-stream remap builds one table covering all streams.
- codec versioning is process-global: `EncodeIndexVersion`/`EncodeVertexVersion` set the format the
  next encode emits; pair encode/decode versions across the wire and probe an unknown blob with the
  `Decode*Version` readers before allocating decode targets.
- the `delegate* unmanaged<...>` parameters (`GenerateVertexRemapCustom`, `SetAllocator`) require a
  `[UnmanagedCallersOnly]` static method address; closures cannot cross the boundary.

[PIPELINE_LAW] — canonical GPU-ready order (each stage feeds the next):
1. `GenerateVertexRemap{,Multi,Custom}` → `RemapVertexBuffer` + `RemapIndexBuffer` (dedup).
2. `OptimizeVertexCache` → `OptimizeOverdraw` (positions) → `OptimizeVertexFetch{,Remap}` (locality).
3. Simplify per LOD with `SimplifyWithAttributes` (or `Simplify`/`SimplifySloppy`), guided by
   `SimplifyScale` when `meshopt_SimplifyErrorAbsolute` is set; `SimplifyPrune` removes islands.
4. Mesh-shader path: `BuildMeshlets{,Flex,Spatial}` (sized by `BuildMeshletsBound`) →
   `OptimizeMeshlet` → `ComputeMeshletBounds` per meshlet; `PartitionClusters` groups for Nanite-style DAGs.
5. Wire path: attribute filters (`EncodeFilterOct/Quat/Exp/Color`) → `EncodeVertexBufferLevel` +
   `EncodeIndexBuffer`; `Analyze*` returns receipts (ACMR/ATVR/overfetch/overdraw) gating the result.

[STACKING] — single dense interchange rail with sibling Compute libs:
- `SharpGLTF.Core` ships NO meshopt/Draco encoder (decompile-verified). The `EXT_meshopt_compression`
  glTF encode path IS this package: filter+`EncodeVertexBufferLevel`/`EncodeIndexBuffer` the
  `MemoryAccessor` byte spans SharpGLTF exposes, then attach the extension and bufferView metadata.
  `SharpGLTF.Toolkit` mesh building feeds raw positions/indices into the `Generate*`→`Optimize*` head.
- `Microsoft.IO.RecyclableMemoryStream` owns the encode/decode scratch buffers: size with the
  `*Bound` ops, rent a stream, pin its `GetSpan()` for the `byte*` codec target — no per-call LOH churn.
- `System.IO.Hashing` (`XxHash3`/`XxHash128`) fingerprints encoded blobs for the
  `Microsoft.Extensions.Caching.Hybrid` LOD cache key, so identical source meshes reuse encoded output.
- `CommunityToolkit.HighPerformance` `Span2D<T>`/`MemoryOwner<T>` and `System.Numerics.Tensors`
  back the managed position/attribute/remap arrays whose handles pin into these pointers.
- `Runtime/payload#RESIDENCY` cluster-LOD chain: `LodChain`/`ClusterLevel` builds each coarser level
  through `Simplify` (error-bounded, `SimplifyScale`-normalized) then re-meshlets it with
  `BuildMeshlets`, threading `Level`/`Parent`/`Error`/`ParentError` onto `ResidencyMeshlet` rows so
  the viewer's screen-space-error cut is monotonic — parent error never below the child's.

[LOCAL_ADMISSION]:
- every entry point is `unsafe extern`; pin via `fixed`, a pinned `Span<T>`, or `MemoryHandle` — a
  GC move mid-call corrupts the native read. `nuint`/`int` returns are written counts / status codes;
  size all encode and meshlet destinations through the matching `*Bound` op before the call.
- statistics structs (`VertexCacheStatistics`, `VertexFetchStatistics`, `OverdrawStatistics`,
  `CoverageStatistics`) return by value with `float` ratios; thread them into typed Compute receipts.
- `SetAllocator` is process-global and must be installed once at startup before concurrent use.

[RAIL_LAW]:
- Package: `Alimer.Bindings.MeshOptimizer`
- Owns: native meshoptimizer bindings for GPU-ready mesh processing and `EXT_meshopt_compression`
- Accept: vertex/index buffer preparation, strip + meshlet + cluster construction, LOD generation,
  attribute-filter codec for wire transmission, quantization
- Reject: mesh topology construction, polygon Boolean operations, attribute interpolation, and Draco
  compression (no Draco surface exists — that codec is out of scope for this binding)
