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

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [RAIL]                                        |
| :-----: | :----------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `Meshopt`                      | static bindings | every meshoptimizer P/Invoke operation        |
|  [02]   | `Meshopt.ResolveLibrary`       | event           | `DllImportResolver?` for embedded native load |
|  [03]   | `SimplificationOptions`        | `[Flags]` enum  | LOD simplification control flags              |
|  [04]   | `EncodeExpMode`                | enum            | exponent filter sharing mode                  |

[PUBLIC_TYPE_SCOPE]: data carrier family (all `struct`, returned/passed by value)
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                                                       |
| :-----: | :---------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Stream`                | vertex stream  | primary-ctor `(void* data, nuint size, nuint stride)` remap  |
|  [02]   | `Meshlet`               | meshlet struct | `uint vertex_offset/triangle_offset/vertex_count/triangle_count` |
|  [03]   | `Bounds`                | bounds struct  | `fixed float center[3]`, `radius`, cone apex/axis/cutoff + s8 |
|  [04]   | `VertexCacheStatistics` | stat struct    | ACMR / ATVR vertex-cache ratios                              |
|  [05]   | `VertexFetchStatistics` | stat struct    | overfetch ratio + bytes fetched                             |
|  [06]   | `OverdrawStatistics`    | stat struct    | pixels-shaded / pixels-covered overdraw ratio               |
|  [07]   | `CoverageStatistics`    | stat struct    | per-axis coverage from `AnalyzeCoverage`                    |

## [03]-[ENTRYPOINTS]

All pointer parameters are `uint*` (indices/remap), `void*` (interleaved vertices), `float*`
(positions/attributes), or `byte*` (triangle buffers/codec/lock); `nuint` carries sizes and
counts. `nuint`-returning ops return a written element count; `int`-returning decoders return a
status code (0 = ok). Stride/size are byte quantities. Pin managed arrays (`fixed` / pinned
`Span<T>` / `MemoryHandle`) before every call.

[ENTRYPOINT_SCOPE]: remap, indexing, and topology generation
- rail: geometry

| [INDEX] | [SURFACE]                                                                                                                             | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :-------------------------------------- |
|  [01]   | `nuint GenerateVertexRemap(uint* dst, uint* indices, nuint index_count, void* vertices, nuint vertex_count, nuint vertex_size)`        | remap          | dedup remap over interleaved vertices   |
|  [02]   | `nuint GenerateVertexRemapMulti(uint* dst, uint* indices, nuint index_count, nuint vertex_count, Stream* streams, nuint stream_count)` | remap          | multi-stream (de-interleaved) remap     |
|  [03]   | `nuint GenerateVertexRemapCustom(uint* dst, uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride, delegate* unmanaged<nint,uint,uint,int> cb, nint ctx)` | remap | predicate-driven weld via callback |
|  [04]   | `void GeneratePositionRemap(uint* dst, float* positions, nuint vertex_count, nuint stride)`                                            | remap          | position-only welding remap             |
|  [05]   | `void RemapVertexBuffer(void* dst, void* vertices, nuint vertex_count, nuint vertex_size, uint* remap)`                               | apply          | apply remap to vertex buffer            |
|  [06]   | `void RemapIndexBuffer(uint* dst, uint* indices, nuint index_count, uint* remap)`                                                     | apply          | apply remap to index buffer             |
|  [07]   | `void GenerateShadowIndexBuffer(uint* dst, uint* indices, nuint index_count, void* vertices, nuint vertex_count, nuint vertex_size, nuint vertex_stride)` | shadow | position-equal shadow IB |
|  [08]   | `void GenerateShadowIndexBufferMulti(uint* dst, uint* indices, nuint index_count, nuint vertex_count, Stream* streams, nuint stream_count)` | shadow | multi-stream shadow IB |
|  [09]   | `void GenerateAdjacencyIndexBuffer(uint* dst, uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride)`  | topology       | triangle-adjacency IB for GS            |
|  [10]   | `void GenerateTessellationIndexBuffer(uint* dst, uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride)` | topology     | PN-tessellation IB                      |
|  [11]   | `nuint GenerateProvokingIndexBuffer(uint* dst, uint* reorder, uint* indices, nuint index_count, nuint vertex_count)`                  | provoking      | flat-shading provoking-vertex reindex   |

[ENTRYPOINT_SCOPE]: cache, overdraw, fetch optimization, and strip generation
- rail: geometry

| [INDEX] | [SURFACE]                                                                                                            | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `void OptimizeVertexCache(uint* dst, uint* indices, nuint index_count, nuint vertex_count)`                          | cache optimize | tipsify cache optimization        |
|  [02]   | `void OptimizeVertexCacheStrip(uint* dst, uint* indices, nuint index_count, nuint vertex_count)`                     | cache optimize | strip-friendly cache optimization |
|  [03]   | `void OptimizeVertexCacheFifo(uint* dst, uint* indices, nuint index_count, nuint vertex_count, uint cache_size)`     | cache optimize | FIFO cache (legacy HW) optimization |
|  [04]   | `void OptimizeOverdraw(uint* dst, uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride, float threshold)` | overdraw | overdraw minimization (post-cache) |
|  [05]   | `nuint OptimizeVertexFetch(void* dst, uint* indices, nuint index_count, void* vertices, nuint vertex_count, nuint vertex_size)` | fetch optimize | reorder vertices for fetch locality |
|  [06]   | `nuint OptimizeVertexFetchRemap(uint* dst, uint* indices, nuint index_count, nuint vertex_count)`                    | fetch remap    | fetch-remap table (apply via remap) |
|  [07]   | `void OptimizeMeshlet(uint* meshlet_vertices, byte* meshlet_triangles, nuint triangle_count, nuint vertex_count)`    | cache optimize | per-meshlet local cache optimize  |
|  [08]   | `nuint Stripify(uint* dst, uint* indices, nuint index_count, nuint vertex_count, uint restart_index)`                | strip          | triangle strip with restart index |
|  [09]   | `nuint StripifyBound(nuint index_count)`                                                                             | bound          | max stripified index count        |
|  [10]   | `nuint Unstripify(uint* dst, uint* indices, nuint index_count, uint restart_index)`                                  | strip          | strip → triangle list expansion   |
|  [11]   | `nuint UnstripifyBound(nuint index_count)`                                                                           | bound          | max unstripified index count      |

[ENTRYPOINT_SCOPE]: index/vertex/sequence codec with format-version pinning
- rail: geometry

| [INDEX] | [SURFACE]                                                                                              | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :----------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `nuint EncodeIndexBuffer(byte* buffer, nuint buffer_size, uint* indices, nuint index_count)`            | encode         | compress index buffer (returns bytes)   |
|  [02]   | `nuint EncodeIndexBufferBound(nuint index_count, nuint vertex_count)`                                   | bound          | encode dst sizing (allocate ≥ this)     |
|  [03]   | `int DecodeIndexBuffer(void* dst, nuint index_count, nuint index_size, byte* buffer, nuint buffer_size)` | decode       | decompress index buffer (status)        |
|  [04]   | `void EncodeIndexVersion(int version)`                                                                  | codec config   | pin index codec format version (global) |
|  [05]   | `int DecodeIndexVersion(byte* buffer, nuint buffer_size)`                                               | codec probe    | read encoded index format version       |
|  [06]   | `nuint EncodeIndexSequence(byte* buffer, nuint buffer_size, uint* indices, nuint index_count)`          | encode         | compress arbitrary index sequence       |
|  [07]   | `nuint EncodeIndexSequenceBound(nuint index_count, nuint vertex_count)`                                 | bound          | sequence encode dst sizing              |
|  [08]   | `int DecodeIndexSequence(void* dst, nuint index_count, nuint index_size, byte* buffer, nuint buffer_size)` | decode    | decompress index sequence (status)      |
|  [09]   | `nuint EncodeVertexBuffer(byte* buffer, nuint buffer_size, void* vertices, nuint vertex_count, nuint vertex_size)` | encode | compress vertex buffer                  |
|  [10]   | `nuint EncodeVertexBufferLevel(byte* buffer, nuint buffer_size, void* vertices, nuint vertex_count, nuint vertex_size, int level, int version)` | encode | level+version-pinned vertex encode |
|  [11]   | `nuint EncodeVertexBufferBound(nuint vertex_count, nuint vertex_size)`                                  | bound          | vertex encode dst sizing                |
|  [12]   | `int DecodeVertexBuffer(void* dst, nuint vertex_count, nuint vertex_size, byte* buffer, nuint buffer_size)` | decode     | decompress vertex buffer (status)       |
|  [13]   | `void EncodeVertexVersion(int version)`                                                                 | codec config   | pin vertex codec format version (global) |
|  [14]   | `int DecodeVertexVersion(byte* buffer, nuint buffer_size)`                                              | codec probe    | read encoded vertex format version      |

[ENTRYPOINT_SCOPE]: attribute filters (octahedral / quaternion / exponent / color)
- rail: geometry

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `void DecodeFilterOct(void* buffer, nuint count, nuint stride)`                                | filter decode  | unpack octahedral normals/tangents     |
|  [02]   | `void DecodeFilterQuat(void* buffer, nuint count, nuint stride)`                               | filter decode  | unpack quaternion rotations            |
|  [03]   | `void DecodeFilterExp(void* buffer, nuint count, nuint stride)`                                | filter decode  | unpack shared-exponent floats          |
|  [04]   | `void DecodeFilterColor(void* buffer, nuint count, nuint stride)`                              | filter decode  | unpack quantized colors                |
|  [05]   | `void EncodeFilterOct(void* dst, nuint count, nuint stride, int bits, float* data)`            | filter encode  | pack octahedral normals (4/8-byte)     |
|  [06]   | `void EncodeFilterQuat(void* dst, nuint count, nuint stride, int bits, float* data)`           | filter encode  | pack quaternions                       |
|  [07]   | `void EncodeFilterExp(void* dst, nuint count, nuint stride, int bits, float* data, EncodeExpMode mode)` | filter encode | pack floats with `EncodeExpMode` sharing |
|  [08]   | `void EncodeFilterColor(void* dst, nuint count, nuint stride, int bits, float* data)`          | filter encode  | pack colors                            |

`EncodeExpMode`: `None`/`EncodeExpSeparate` (0, per-value), `EncodeExpSharedVector` (1, per-vector),
`EncodeExpSharedComponent` (2, per-component column), `EncodeExpClamped` (3).

[ENTRYPOINT_SCOPE]: LOD simplification and quantization
- rail: geometry

| [INDEX] | [SURFACE]                                                                                                                  | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :------------------------------------------------------------------------------------------------------------------------ | :------------- | :----------------------------------------- |
|  [01]   | `nuint Simplify(uint* dst, uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride, nuint target_index_count, float target_error, SimplificationOptions options, float* result_error)` | simplify | quadric LOD; writes achieved error |
|  [02]   | `nuint SimplifyWithAttributes(uint* dst, uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride, float* attributes, nuint attr_stride, float* attr_weights, nuint attr_count, byte* vertex_lock, nuint target_index_count, float target_error, SimplificationOptions options, float* result_error)` | simplify | attribute-aware quadric (the production simplifier) |
|  [03]   | `nuint SimplifyWithUpdate(uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride, float* attributes, nuint attr_stride, float* attr_weights, nuint attr_count, byte* vertex_lock, nuint target_index_count, float target_error, uint options, float* result_error)` | simplify | in-place simplify (mutates `indices`) |
|  [04]   | `nuint SimplifySloppy(uint* dst, uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride, byte* vertex_lock, nuint target_index_count, float target_error, float* result_error)` | simplify | topology-ignoring aggressive LOD |
|  [05]   | `nuint SimplifyPoints(uint* dst, float* positions, nuint vertex_count, nuint stride, float* colors, nuint colors_stride, float color_weight, nuint target_vertex_count)` | simplify | point-cloud decimation |
|  [06]   | `nuint SimplifyPrune(uint* dst, uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride, float target_error)` | simplify | drop components under error threshold |
|  [07]   | `float SimplifyScale(float* positions, nuint vertex_count, nuint stride)`                                                  | scale          | absolute-error scale factor for the mesh   |
|  [08]   | `ushort QuantizeHalf(float v)` / `float DequantizeHalf(ushort h)`                                                          | quantize       | IEEE half round-trip                       |
|  [09]   | `float QuantizeFloat(float v, int N)`                                                                                      | quantize       | reduce float to N-bit mantissa             |

`SimplificationOptions` `[Flags]`: `None` (0), `SimplifyLockBorder` (1, freeze open edges),
`meshopt_SimplifySparse` (2, sparse attribute discontinuity reduction), `meshopt_SimplifyErrorAbsolute`
(4, interpret `target_error` in world units scaled by `SimplifyScale`). `vertex_lock` is an optional
`byte*` mask (1 = pinned) shared across the attribute-aware simplifiers.

[ENTRYPOINT_SCOPE]: meshlet, cluster, spatial, and analysis
- rail: geometry

| [INDEX] | [SURFACE]                                                                                                                                   | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :---------------------------------- |
|  [01]   | `nuint BuildMeshlets(Meshlet* meshlets, uint* meshlet_vertices, byte* meshlet_triangles, uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride, nuint max_vertices, nuint max_triangles, float cone_weight)` | meshlet | cone-weighted meshlet build |
|  [02]   | `nuint BuildMeshletsFlex(… nuint max_vertices, nuint min_triangles, nuint max_triangles, float cone_weight, float split_factor)`             | meshlet        | variable-size meshlets (min..max)   |
|  [03]   | `nuint BuildMeshletsSpatial(… nuint max_vertices, nuint min_triangles, nuint max_triangles, float fill_weight)`                              | meshlet        | spatial-locality meshlet build      |
|  [04]   | `nuint BuildMeshletsScan(Meshlet* meshlets, uint* meshlet_vertices, byte* meshlet_triangles, uint* indices, nuint index_count, nuint vertex_count, nuint max_vertices, nuint max_triangles)` | meshlet | fast scan build (no positions) |
|  [05]   | `nuint BuildMeshletsBound(nuint index_count, nuint max_vertices, nuint max_triangles)`                                                       | bound          | meshlet/vertex/triangle buffer sizing |
|  [06]   | `Bounds ComputeClusterBounds(uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride)`                          | bounds         | cone + sphere bounds for index range |
|  [07]   | `Bounds ComputeMeshletBounds(uint* meshlet_vertices, byte* meshlet_triangles, nuint triangle_count, float* positions, nuint vertex_count, nuint stride)` | bounds | per-meshlet cone/sphere bounds |
|  [08]   | `Bounds ComputeSphereBounds(float* positions, nuint count, nuint stride, float* radii, nuint radii_stride)`                                  | bounds         | bounding sphere over (optional) radii |
|  [09]   | `nuint PartitionClusters(uint* dst, uint* cluster_indices, nuint total_index_count, uint* cluster_index_counts, nuint cluster_count, float* positions, nuint vertex_count, nuint stride, nuint target_partition_size)` | partition | spatial cluster→group partitioning |
|  [10]   | `void SpatialSortRemap(uint* dst, float* positions, nuint vertex_count, nuint stride)`                                                       | sort           | Morton-order vertex remap            |
|  [11]   | `void SpatialSortTriangles(uint* dst, uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride)`                 | sort           | spatially coherent triangle order    |
|  [12]   | `void SpatialClusterPoints(uint* dst, float* positions, nuint vertex_count, nuint stride, nuint cluster_size)`                               | cluster        | spatial point clustering             |
|  [13]   | `VertexCacheStatistics AnalyzeVertexCache(uint* indices, nuint index_count, nuint vertex_count, uint cache_size, uint warp_size, uint primgroup_size)` | analyze | ACMR/ATVR cache stats |
|  [14]   | `VertexFetchStatistics AnalyzeVertexFetch(uint* indices, nuint index_count, nuint vertex_count, nuint vertex_size)`                          | analyze        | overfetch stats                      |
|  [15]   | `OverdrawStatistics AnalyzeOverdraw(uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride)`                   | analyze        | overdraw ratio stats                 |
|  [16]   | `CoverageStatistics AnalyzeCoverage(uint* indices, nuint index_count, float* positions, nuint vertex_count, nuint stride)`                   | analyze        | per-axis coverage stats              |
|  [17]   | `void SetAllocator(delegate* unmanaged<nuint,void*> allocate, delegate* unmanaged<void*,void> deallocate)`                                   | allocator      | route native scratch through a pool  |

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
