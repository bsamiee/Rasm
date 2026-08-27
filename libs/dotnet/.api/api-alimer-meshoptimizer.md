# [RASM_API_ALIMER_MESHOPTIMIZER]

`Meshopt` binds Arseny Kapoulkine's native `meshoptimizer` as one in-process algorithmic surface across two tiers — a first-class managed generic `Span<T>` surface that pins internally, over a raw `unsafe static extern` P/Invoke twin for caller-pinned buffers. It constructs no mesh topology and interpolates no attributes: geometry arrives welded and leaves cache-optimized, simplified, meshletized, and wire-encoded. Two folders compose it against one shared carrier algebra — `Rasm.Bim` drives the glTF `EXT_meshopt_compression` interchange leg and the per-element BIM LOD pyramid, `Rasm.Compute` the runtime tile/payload codec streams and the cluster-LOD residency pyramid — and the codec's process-global format-version state makes a per-folder partition of this assembly structurally impossible, so the whole surface homes here.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Meshopt` facade and its value types — stream input, meshlet geometry, options, and by-value analysis results

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `Meshopt`                | static class  | algorithmic surface; const `MESHOPTIMIZER_VERSION=1000u`, `LibraryName`; `ResolveLibrary` |
|  [02]   | `Meshopt.ResolveLibrary` | event         | `DllImportResolver?` overriding the native load path for embedded/self-contained deploys  |
|  [03]   | `Stream`                 | unsafe struct | multi-stream remap input; ctor `(void* data, nuint size, nuint stride)`                   |
|  [04]   | `Meshlet`                | struct        | `vertex_offset`, `triangle_offset`, `vertex_count`, `triangle_count` (uint)               |
|  [05]   | `Bounds`                 | unsafe struct | sphere + cone-cull bounds; fields below                                                   |
|  [06]   | `SimplificationOptions`  | [Flags] enum  | bitwise-OR simplify flags; cases below                                                    |
|  [07]   | `EncodeExpMode`          | enum          | exponent-sharing encode mode; cases below                                                 |
|  [08]   | `VertexCacheStatistics`  | struct        | `vertices_transformed`, `warps_executed` (uint); `acmr`, `atvr` (float)                   |
|  [09]   | `VertexFetchStatistics`  | struct        | `bytes_fetched` (uint); `overfetch` (float)                                               |
|  [10]   | `OverdrawStatistics`     | struct        | `pixels_covered`, `pixels_shaded` (uint); `overdraw` (float)                              |
|  [11]   | `CoverageStatistics`     | unsafe struct | `coverage[3]` fixed float; `extent` (float)                                               |

- `Bounds`: `center[3]`/`cone_apex[3]`/`cone_axis[3]` fixed float; `radius`/`cone_cutoff` float; `cone_axis_s8[3]`/`cone_cutoff_s8` byte.
- `SimplificationOptions`: `None=0`, `SimplifyLockBorder=1` (freeze open edges), `meshopt_SimplifySparse=2` (sparse attribute-discontinuity reduction), `meshopt_SimplifyErrorAbsolute=4` (`target_error` in world units via `SimplifyScale`).
- `EncodeExpMode`: `None`/`EncodeExpSeparate`=0 (per-value), `EncodeExpSharedVector`=1, `EncodeExpSharedComponent`=2 (per-column), `EncodeExpClamped`=3.

## [02]-[ENTRYPOINTS]

Every surface is a static method on `Meshopt` in two forms. The MANAGED form pins internally and derives counts from span length — generic over `TVertex`/`TIndex`/`T: unmanaged`, returning `out float error`/`Bounds`/`*Statistics` by value — so a `[SURFACE]` cell carries only the member and the args that vary from its scope-lead convention. The EXTERN twin takes `uint*` (indices, remap), `void*` (interleaved vertices), `float*` (positions, attributes), or `byte*` (triangle, codec, and lock buffers) with `nuint` sizes/counts and serves only an already-pinned caller buffer (`stackalloc`/`NativeMemory`/`fixed`); every `nuint` return is a written element count and every `int` return a status code (`0` = ok).

[ENTRYPOINT_SCOPE]: vertex remap, dedup, and derived index buffers — remap ops write `Span<uint> dst` and return the `nuint` count; `verts` is `ReadOnlySpan<TVertex>` (`TVertex: unmanaged`), `pos` is `ReadOnlySpan<float>` + `nuint stride`, `cb` is `delegate*<nint,uint,uint,int>`, an unindexed overload swaps `idx` for `nuint idxCount`

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `GenerateVertexRemap<TVertex>(dst, idx, verts)`                                      | typed remap, dedups verts           |
|  [02]   | `GenerateVertexRemapMulti<TVertex>(dst, idx, vertCount, ReadOnlySpan<Stream>)`       | multi-stream typed remap            |
|  [03]   | `GenerateVertexRemapCustom(dst, idx, pos, stride, cb, nint ctx)`                     | remap with custom equality callback |
|  [04]   | `GeneratePositionRemap(dst, pos, stride)`                                            | position-only dedup remap           |
|  [05]   | `RemapVertexBuffer<TVertex>(Span<TVertex> dst, verts, remap)`                        | applies remap to typed vertex data  |
|  [06]   | `RemapIndexBuffer(dst, idx, remap)`                                                  | applies remap to index buffer       |
|  [07]   | `GenerateShadowIndexBuffer<TVertex>(dst, idx, verts, nuint vertSize)`                | position-equivalence shadow buffer  |
|  [08]   | `GenerateShadowIndexBufferMulti<TVertex>(dst, idx, vertCount, ReadOnlySpan<Stream>)` | multi-stream shadow buffer          |
|  [09]   | `GenerateAdjacencyIndexBuffer(dst, idx, pos, stride)`                                | triangle-adjacency IB for GS        |
|  [10]   | `GenerateTessellationIndexBuffer(dst, idx, pos, stride)`                             | PN-tessellation IB                  |
|  [11]   | `GenerateProvokingIndexBuffer(dst, Span<uint> reorder, idx, nuint vertCount)`        | flat-shading provoking-vertex IB    |

[ENTRYPOINT_SCOPE]: cache, overdraw, fetch reorder and strip conversion — reorder ops take `(Span<uint> dst, ReadOnlySpan<uint> idx, nuint vertCount)` and rewrite `dst` in index order

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------ | :----------------------------------------------- |
|  [01]   | `OptimizeVertexCache(dst, idx, vertCount)`                          | tipsify vertex-cache reorder                     |
|  [02]   | `OptimizeVertexCacheStrip(dst, idx, vertCount)`                     | strip-order cache reorder                        |
|  [03]   | `OptimizeVertexCacheFifo(dst, idx, vertCount, uint cacheSize)`      | FIFO cache reorder for a fixed cache size        |
|  [04]   | `OptimizeOverdraw(dst, idx, pos, stride, float threshold)`          | overdraw reorder within a cache-efficiency bound |
|  [05]   | `OptimizeVertexFetch<TVertex>(dst, Span<uint> idx, verts) -> nuint` | fetch reorder, rewrites `idx` in place           |
|  [06]   | `OptimizeVertexFetchRemap(dst, idx, vertCount) -> nuint`            | fetch-optimal remap table                        |
|  [07]   | `Stripify(dst, idx, vertCount, uint restartIndex) -> nuint`         | triangle list → strip with explicit restart      |
|  [08]   | `Unstripify(dst, idx, uint restartIndex) -> nuint`                  | strip → triangle list                            |

[ENTRYPOINT_SCOPE]: encode/decode compression for GPU streaming (`EXT_meshopt_compression`) and vertex-attribute filters — encode writes `Span<byte> buf` returning the `nuint` byte count, decode reads `ReadOnlySpan<byte> buf` into a typed span returning `int` status, version reads return `int`; a filter generic over `<T: unmanaged>` rewrites `Span<T> buf` in place on decode and takes `(Span<T> dst, int bits, ReadOnlySpan<float> data)` on encode

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `EncodeIndexBuffer(buf, idx) -> nuint`                                               | index buffer encode                          |
|  [02]   | `EncodeIndexSequence(buf, Span<uint> idx) -> nuint`                                  | non-triangle index sequence encode           |
|  [03]   | `DecodeIndexBuffer<TIndex>(Span<TIndex> dst, buf) -> int`                            | typed index decode                           |
|  [04]   | `DecodeIndexSequence<TIndex>(Span<TIndex> dst, buf) -> int`                          | typed index sequence decode                  |
|  [05]   | `EncodeVertexBuffer<TVertex>(buf, verts) -> nuint`                                   | typed vertex encode                          |
|  [06]   | `EncodeVertexBufferLevel<TVertex>(buf, verts, int level, int version = -1) -> nuint` | leveled/versioned vertex encode              |
|  [07]   | `DecodeVertexBuffer<TVertex>(Span<TVertex> dst, buf) -> int`                         | typed vertex decode                          |
|  [08]   | `DecodeIndexVersion(buf)` / `DecodeVertexVersion(buf) -> int`                        | reads bitstream format version from a buffer |
|  [09]   | `DecodeFilterOct` / `…Quat` / `…Exp` / `…Color <T>(buf)`                             | decode oct/quat/exp/color filter in place    |
|  [10]   | `EncodeFilterOct` / `…Quat` / `…Color <T>(dst, bits, data)`                          | encode normals/quaternions/color             |
|  [11]   | `EncodeFilterExp<T>(dst, bits, data, EncodeExpMode mode)`                            | encode floats exponent-separated by mode     |

[ENTRYPOINT_SCOPE]: LOD simplification — forms lead with `(Span<uint> dst, ReadOnlySpan<uint> idx, ReadOnlySpan<float> pos, nuint stride, …)`, mostly end in `(nuint targetIdxCount, float targetError, SimplificationOptions opts, out float error)` returning the reduced `nuint` count; attribute forms insert `(attrs, attrStride, attrWeights, attrCount)` + optional `ReadOnlySpan<byte> vertexLock` (`1` = locked, shared across the attribute-aware simplifiers)

| [INDEX] | [SURFACE]                                                                          | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `Simplify`                                                                         | error-bounded quadric simplify        |
|  [02]   | `SimplifyWithAttributes` (+ attrs, opt `vertexLock`)                               | attribute-weighted quadric simplify   |
|  [03]   | `SimplifyWithUpdate(Span<uint> idx, …)` (+ attrs, `vertexLock`)                    | in-place simplify (mutates `idx`)     |
|  [04]   | `SimplifySloppy(…[, byte* vertexLock])` (no opts)                                  | topology-ignoring aggressive simplify |
|  [05]   | `SimplifyPrune(…, float targetError)`                                              | drops components under threshold      |
|  [06]   | `SimplifyPoints(…, colors, colorStride, float colorWeight, nuint targetVertCount)` | color-weighted point-cloud decimation |
|  [07]   | `SimplifyScale(pos, stride) -> float`                                              | world-space error-scale factor        |

[ENTRYPOINT_SCOPE]: meshlets, bounds and spatial clustering — meshlet builds fill `(Span<Meshlet>, Span<uint> meshletVerts, Span<byte> meshletTris, …)` and return the `nuint` meshlet count; bounds return `Bounds` by value

| [INDEX] | [SURFACE]                                                                                | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `BuildMeshlets(…, idx, pos, stride, nuint maxVerts, nuint maxTris, float coneWeight)`    | cone-cull meshlet build                   |
|  [02]   | `BuildMeshletsScan(…, idx, nuint vertCount, maxVerts, maxTris)`                          | scan-based build, no cone culling         |
|  [03]   | `BuildMeshletsFlex(…, maxVerts, minTris, maxTris, coneWeight, float splitFactor)`        | flexible min/max-tri build                |
|  [04]   | `BuildMeshletsSpatial(…, maxVerts, minTris, maxTris, float fillWeight)`                  | spatially-coherent build                  |
|  [05]   | `OptimizeMeshlet(meshletVerts, meshletTris, nuint triCount, nuint vertCount)`            | per-meshlet cache reorder                 |
|  [06]   | `ComputeClusterBounds(idx, pos, stride) -> Bounds`                                       | sphere + cone cluster bounds              |
|  [07]   | `ComputeMeshletBounds(meshletVerts, meshletTris, pos, stride) -> Bounds`                 | bounds for a meshlet vert/tri set         |
|  [08]   | `ComputeSphereBounds(pos, stride[, radii, radiiStride]) -> Bounds`                       | bounding sphere, optional per-point radii |
|  [09]   | `PartitionClusters(dst, clusterIdx, clusterIdxCounts, pos, stride, targetSize) -> nuint` | two-level cluster→group partition         |
|  [10]   | `SpatialSortRemap(dst, pos, stride)`                                                     | Morton-order vertex remap                 |
|  [11]   | `SpatialSortTriangles(dst, idx, pos, stride)`                                            | triangle reorder by centroid locality     |
|  [12]   | `SpatialClusterPoints(dst, pos, stride, nuint clusterSize)`                              | spatially-coherent point clustering       |

[ENTRYPOINT_SCOPE]: analysis results and scalar helpers — analysis returns its `*Statistics` struct by value; a bound helper takes `nuint` counts, returns the `nuint` output ceiling, and sizes a buffer before the op fills it; cache params are `uint`

| [INDEX] | [SURFACE]                                                                | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `AnalyzeVertexCache(idx, vertCount, cacheSize, warpSize, primGroupSize)` | ACMR/ATVR cache stats                       |
|  [02]   | `AnalyzeOverdraw(idx, pos, stride)`                                      | overdraw stats (pixels covered/shaded)      |
|  [03]   | `AnalyzeVertexFetch(idx, vertCount, nuint vertSize)`                     | fetch stats (bytes fetched, overfetch)      |
|  [04]   | `AnalyzeCoverage(idx, pos, stride)`                                      | per-axis coverage extents                   |
|  [05]   | `EncodeIndexBufferBound` / `…SequenceBound(idxCount, vertCount)`         | max index-encode byte size                  |
|  [06]   | `EncodeVertexBufferBound(vertCount, vertSize)`                           | max vertex-encode byte size                 |
|  [07]   | `BuildMeshletsBound(idxCount, maxVerts, maxTris)`                        | upper meshlet count for allocation          |
|  [08]   | `StripifyBound` / `UnstripifyBound(idxCount)`                            | max strip/unstrip index count               |
|  [09]   | `EncodeIndexVersion` / `EncodeVertexVersion(int version)`                | sets the process-wide encode format version |
|  [10]   | `QuantizeHalf(float v) -> ushort` / `DequantizeHalf(ushort h) -> float`  | float ↔ fp16 half                           |
|  [11]   | `QuantizeFloat(float v, int N) -> float`                                 | quantize float to N mantissa bits           |
|  [12]   | `SetAllocator(allocate, deallocate)`                                     | route native scratch through a pool         |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- every algorithm ships a managed `Span<T>`/`ReadOnlySpan<T>` overload — the first-class surface, pinning internally, generic over `TVertex`/`TIndex`/`T: unmanaged`, returning `out float error`/`Bounds`/`*Statistics` by value — over the raw `unsafe static extern` twin (`meshopt_*` entry points, dual `[LibraryImport]`/`[DllImport]`) it forwards to.
- managed tier owns pinning, span-length→`nuint` count derivation, and function-pointer marshalling; the extern twin serves only a caller-owned `stackalloc`/`NativeMemory`/`fixed` buffer — a GC move mid-call corrupts the native read, so pinning via `fixed`, a pinned `Span<T>`, or `MemoryHandle` holds every managed array across an extern call.
- native library `meshoptimizer` (`.dll` Windows, `.dylib` macOS, `.so` Linux/Android) resolves through `NativeLibrary.SetDllImportResolver` at static init; `Meshopt.ResolveLibrary` is a `DllImportResolver` event overriding the load path ahead of the built-in resolver, and `Meshopt.SetAllocator` installs a custom native allocator ONCE at startup before any concurrent use.
- each `delegate* unmanaged<...>` parameter (`GenerateVertexRemapCustom`, `SetAllocator`) binds a `[UnmanagedCallersOnly]` static address, so no closure crosses the boundary.
- codec versioning is PROCESS-GLOBAL: `EncodeIndexVersion`/`EncodeVertexVersion` set the format the next encode emits, `EncodeVertexBufferLevel` overrides it per call, and `Decode*Version` probes an unknown blob before its decode target is allocated — this shared mutable state is why the two consuming folders compose one catalogue, never a partition; `SimplificationOptions` flags compose by bitwise OR.
- `Stream` `(void* data, nuint size, nuint stride)` points `data` at the first attribute of a de-interleaved stream, `size` the per-vertex byte span the remap hashes, `stride` the advance; multi-stream remap builds one table covering every stream.
- canonical GPU-ready order, each stage feeding the next: dedup (`GenerateVertexRemap{,Multi,Custom}` → `RemapVertexBuffer` + `RemapIndexBuffer`) → locality (`OptimizeVertexCache` → `OptimizeOverdraw` → `OptimizeVertexFetch{,Remap}`) → LOD (`SimplifyWithAttributes`/`Simplify`/`SimplifySloppy` per level, `SimplifyScale`-normalized under `meshopt_SimplifyErrorAbsolute`; `SimplifyPrune` drops islands) → mesh-shader (`BuildMeshlets{,Flex,Spatial}` sized by `BuildMeshletsBound` → `OptimizeMeshlet` → `ComputeMeshletBounds`; `PartitionClusters` groups for Nanite-style DAGs) → wire (`EncodeFilter{Oct,Quat,Exp,Color}` → `EncodeVertexBufferLevel` + `EncodeIndexBuffer`; `Analyze*` results gate the output).

[STACKING]:
- `SharpGLTF.Core`(`api-sharpgltf.md`): SharpGLTF carries no meshopt or Draco encoder, so this surface owns the `EXT_meshopt_compression` encode path — `EncodeFilterOct`/`…Quat`/`…Exp` pre-quantize into the filter-coded layout the extension declares, then `EncodeVertexBufferLevel`/`EncodeIndexBuffer` produce the `byte[]` payload attached under the extension's bufferView metadata; `SharpGLTF.Toolkit` mesh building feeds raw positions and indices into the `Generate*`→`Optimize*` head.
- `Openize.Drako`(`Rasm.Bim/.api/api-openize-drako.md`): sibling compression leg — `Meshopt` is the `EXT_meshopt_compression` leg, `Drako` the `KHR_draco_mesh_compression` leg; one export-codec dispatch row selects by extension policy, both feeding the same glTF buffer writer.
- `Microsoft.IO.RecyclableMemoryStream`(`Rasm.Compute/.api/api-recyclable-stream.md`): a `*Bound`-sized rented stream whose pinned `GetSpan()` is the codec target, so encode and decode scratch takes no per-call LOH churn.
- `System.IO.Hashing`(`api-hashing.md`): `XxHash3`/`XxHash128` fingerprints an encoded blob into the `Microsoft.Extensions.Caching.Hybrid` LOD cache key, so identical source meshes reuse encoded output.
- `CommunityToolkit.HighPerformance`(`api-highperformance.md`): `Span2D<T>`/`MemoryOwner<T>` with `System.Numerics.Tensors` back the managed position, attribute, and remap arrays the spans project from.
- Bim consumer anchor: the canonical interleaved vertex struct threads the index-optimize/encode path as one `ReadOnlySpan<TVertex>` shared with Drako intake; every `Bounds`/`*Statistics` value return folds into the typed domain result under the codec `Fin`/`Eff` carrier, while `ResolveLibrary`/`SetAllocator` native-load throws enter `Try.lift` and remain the original exceptional `Error` unless a documented native verdict maps to a caused owner case.
- Compute consumer anchor: the cluster-LOD residency chain — `LodChain`/`ClusterLevel` builds each coarser level through error-bounded `SimplifyScale`-normalized `Simplify` then re-meshlets it with `BuildMeshlets`, threading `Level`/`Parent`/`Error`/`ParentError` onto `ResidencyMeshlet` rows so the viewer's screen-space-error cut stays monotonic — a parent error never below its child's.

[LOCAL_ADMISSION]:
- index path: `GenerateVertexRemap<TVertex>` → `RemapVertexBuffer<TVertex>` + `RemapIndexBuffer` → `OptimizeVertexCache` → `OptimizeOverdraw` → `OptimizeVertexFetch<TVertex>` → `EncodeIndexBuffer` / `EncodeVertexBuffer<TVertex>`.
- meshlet path: `BuildMeshlets` sized by `BuildMeshletsBound` → `OptimizeMeshlet` per meshlet → `ComputeMeshletBounds` for culling.
- simplify path: `Simplify` / `SimplifyWithAttributes` (`SimplificationOptions` + `out float error`) → optional `SimplifyPrune`, normalizing the budget with `SimplifyScale`.
- size every encode and meshlet destination through its `*Bound` op first; drop to the extern twin solely for an already-pinned caller buffer; subscribe `Meshopt.ResolveLibrary` or `SetAllocator` before first call when deploying outside the standard NuGet native layout.
