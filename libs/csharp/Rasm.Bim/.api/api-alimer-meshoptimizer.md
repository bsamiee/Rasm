# [RASM_BIM_API_MESHOPTIMIZER]

`Meshopt` owns the full `meshoptimizer` C surface across two tiers — a first-class managed generic `Span<T>` rail that pins internally over a raw `unsafe static extern` P/Invoke twin for caller-pinned buffers — folding index and vertex optimization, GPU-stream encode/decode compression, LOD simplification, meshlet generation, spatial clustering and partitioning, quantization, and cache/overdraw/fetch/coverage analysis onto the Bim and Compute mesh rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Alimer.Bindings.MeshOptimizer`
- package: `Alimer.Bindings.MeshOptimizer` (MIT)
- assembly: `Alimer.Bindings.MeshOptimizer`
- namespace: `MeshOptimizer`
- asset: `lib/net10.0`, `lib/net9.0` — the `net10.0`-only TFM firebreaks it out of any `net48` in-Rhino plugin ALC
- asset: native `runtimes/<rid>/native/` for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx` (universal arm64+x64 fat `libmeshoptimizer.dylib`), `android-arm`, `android-arm64`, `android-x64`
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Meshopt` facade and its value types — stream input, meshlet geometry, options, and by-value analysis receipts

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `Meshopt`               | static class  | algorithmic surface; const `MESHOPTIMIZER_VERSION=1000u`, `LibraryName`; `ResolveLibrary` |
|  [02]   | `Stream`                | unsafe struct | multi-stream remap input; ctor `(void* data, nuint size, nuint stride)`                   |
|  [03]   | `Meshlet`               | struct        | `vertex_offset`, `triangle_offset`, `vertex_count`, `triangle_count` (uint)               |
|  [04]   | `Bounds`                | unsafe struct | sphere + cone-cull bounds; fields below                                                   |
|  [05]   | `SimplificationOptions` | [Flags] enum  | bitwise-OR simplify flags; cases below                                                    |
|  [06]   | `EncodeExpMode`         | enum          | exponent-sharing encode mode; cases below                                                 |
|  [07]   | `VertexCacheStatistics` | struct        | `vertices_transformed`, `warps_executed` (uint); `acmr`, `atvr` (float)                   |
|  [08]   | `VertexFetchStatistics` | struct        | `bytes_fetched` (uint); `overfetch` (float)                                               |
|  [09]   | `OverdrawStatistics`    | struct        | `pixels_covered`, `pixels_shaded` (uint); `overdraw` (float)                              |
|  [10]   | `CoverageStatistics`    | unsafe struct | `coverage[3]` fixed float; `extent` (float)                                               |

- `Bounds`: `center[3]`/`cone_apex[3]`/`cone_axis[3]` fixed float; `radius`/`cone_cutoff` float; `cone_axis_s8[3]`/`cone_cutoff_s8` byte.
- `SimplificationOptions`: `None=0`, `SimplifyLockBorder=1`, `meshopt_SimplifySparse=2`, `meshopt_SimplifyErrorAbsolute=4`.
- `EncodeExpMode`: `None`/`EncodeExpSeparate`=0, `EncodeExpSharedVector`=1, `EncodeExpSharedComponent`=2, `EncodeExpClamped`=3.

## [03]-[ENTRYPOINTS]

Every surface is a static method on `Meshopt`; a managed `Span<T>` overload pins internally and derives counts from span length, so a `[SURFACE]` cell carries only the member and the args that vary from its scope-lead convention.

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
|  [09]   | `GenerateAdjacencyIndexBuffer(dst, idx, pos, stride)`                                | adjacency index buffer              |
|  [10]   | `GenerateTessellationIndexBuffer(dst, idx, pos, stride)`                             | tessellation index buffer           |
|  [11]   | `GenerateProvokingIndexBuffer(dst, Span<uint> reorder, idx, nuint vertCount)`        | provoking-vertex index buffer       |

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

[ENTRYPOINT_SCOPE]: encode/decode compression for GPU streaming (`EXT_meshopt_compression`) and vertex-attribute filters — encode writes `Span<byte> buf` returning the `nuint` byte count, decode reads `ReadOnlySpan<byte> buf` into a typed span, version reads return `int`; a filter generic over `<T: unmanaged>` rewrites `Span<T> buf` in place on decode and takes `(Span<T> dst, int bits, ReadOnlySpan<float> data)` on encode

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

[ENTRYPOINT_SCOPE]: LOD simplification — forms lead with `(Span<uint> dst, ReadOnlySpan<uint> idx, ReadOnlySpan<float> pos, nuint stride, …)`, mostly end in `(nuint targetIdxCount, float targetError, SimplificationOptions opts, out float error)` returning the reduced `nuint` count; attribute forms insert `(attrs, attrStride, attrWeights, attrCount)` + optional `ReadOnlySpan<byte> vertexLock`

| [INDEX] | [SURFACE]                                                                                         | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------------------------ | :----------------------------- |
|  [01]   | `Simplify`                                                                                        | error-bounded surface simplify |
|  [02]   | `SimplifyWithAttributes` (+ attrs, opt `vertexLock`)                                              | attribute-weighted simplify    |
|  [03]   | `SimplifyWithUpdate(Span<uint> idx, …)` (+ attrs, `vertexLock`)                                   | in-place attribute simplify    |
|  [04]   | `SimplifySloppy(…[, byte* vertexLock])` (no opts)                                                 | fast aggressive simplify       |
|  [05]   | `SimplifyPrune(…, float targetError)`                                                             | strips sub-threshold triangles |
|  [06]   | `SimplifyPoints(dst, pos, stride, colors, colorStride, float colorWeight, nuint targetVertCount)` | color-weighted point simplify  |
|  [07]   | `SimplifyScale(pos, stride) -> float`                                                             | world-space error-scale factor |

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
|  [09]   | `PartitionClusters(dst, clusterIdx, clusterIdxCounts, pos, stride, targetSize) -> nuint` | two-level cluster partition               |
|  [10]   | `SpatialSortRemap(dst, pos, stride)`                                                     | spatial-locality vertex remap             |
|  [11]   | `SpatialSortTriangles(dst, idx, pos, stride)`                                            | triangle reorder by centroid locality     |
|  [12]   | `SpatialClusterPoints(dst, pos, stride, nuint clusterSize)`                              | spatially-coherent point clustering       |

[ENTRYPOINT_SCOPE]: analysis receipts and scalar helpers — analysis returns its `*Statistics` struct by value; a bound helper takes `nuint` counts, returns the `nuint` output ceiling, and sizes a buffer before the op fills it; cache params are `uint`

| [INDEX] | [SURFACE]                                                                | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `AnalyzeVertexCache(idx, vertCount, cacheSize, warpSize, primGroupSize)` | ACMR/ATVR cache stats                       |
|  [02]   | `AnalyzeOverdraw(idx, pos, stride)`                                      | overdraw stats (pixels covered/shaded)      |
|  [03]   | `AnalyzeVertexFetch(idx, vertCount, nuint vertSize)`                     | fetch stats (bytes fetched, overfetch)      |
|  [04]   | `AnalyzeCoverage(idx, pos, stride)`                                      | coverage extents                            |
|  [05]   | `EncodeIndexBufferBound` / `…SequenceBound(idxCount, vertCount)`         | max index-encode byte size                  |
|  [06]   | `EncodeVertexBufferBound(vertCount, vertSize)`                           | max vertex-encode byte size                 |
|  [07]   | `BuildMeshletsBound(idxCount, maxVerts, maxTris)`                        | upper meshlet count for allocation          |
|  [08]   | `StripifyBound` / `UnstripifyBound(idxCount)`                            | max strip/unstrip index count               |
|  [09]   | `EncodeIndexVersion` / `EncodeVertexVersion(int version)`                | sets the process-wide encode format version |
|  [10]   | `QuantizeHalf(float v) -> ushort` / `DequantizeHalf(ushort h) -> float`  | float ↔ fp16 half                           |
|  [11]   | `QuantizeFloat(float v, int N) -> float`                                 | quantize float to N mantissa bits           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- every algorithm ships a managed `Span<T>`/`ReadOnlySpan<T>` overload — the first-class rail, pinning internally, generic over `TVertex`/`TIndex`/`T: unmanaged`, returning `out float error`/`Bounds`/`*Statistics` by value — over the raw `unsafe static extern` twin (`meshopt_*` entry points, dual `[LibraryImport]`/`[DllImport]`) it forwards to.
- managed tier owns pinning, span-length→`nuint` count derivation, and function-pointer marshalling for `GenerateVertexRemapCustom`; the extern twin serves only a caller-owned `stackalloc`/`NativeMemory`/`fixed` buffer.
- native library `meshoptimizer` (`.dll` Windows, `.dylib` macOS, `.so` Linux/Android) resolves through `NativeLibrary.SetDllImportResolver` at static init; `Meshopt.ResolveLibrary` is a `DllImportResolver` event overriding the load path ahead of the built-in resolver, and `Meshopt.SetAllocator` installs a custom native allocator.
- `EncodeIndexVersion`/`EncodeVertexVersion` set the target bitstream format version process-wide before encoding, `EncodeVertexBufferLevel` overrides it per call, and `DecodeIndexVersion`/`DecodeVertexVersion` read it back from an encoded buffer; `SimplificationOptions` flags compose by bitwise OR.

[STACKING]:
- `Openize.Drako`(`api-openize-drako`): sibling compression leg — `Meshopt` is the `EXT_meshopt_compression` leg, `Drako` the `KHR_draco_mesh_compression` leg; one export-codec dispatch row selects by extension policy, both feeding the same glTF buffer writer.
- `SharpGLTF.Core`(`api-sharpgltf`): `EncodeVertexBuffer<TVertex>`/`EncodeIndexBuffer` produce the `byte[]` payload referenced under `EXT_meshopt_compression`, with `EncodeFilterOct`/`…Quat`/`…Exp` pre-quantizing normals/quaternions/exponents into the filter-coded layout the extension declares.
- Compute owner: the canonical interleaved vertex struct threads the index-optimize/encode and meshlet/cluster paths (`[LOCAL_ADMISSION]`) as one `ReadOnlySpan<TVertex>` shared with Drako intake, and every `Bounds`/`*Statistics` value return folds into typed receipt fields under the codec `Fin`/`Eff` rail where `ResolveLibrary`/`SetAllocator` native-load faults map to a typed boundary fault.

[LOCAL_ADMISSION]:
- index path: `GenerateVertexRemap<TVertex>` → `RemapVertexBuffer<TVertex>` + `RemapIndexBuffer` → `OptimizeVertexCache` → `OptimizeOverdraw` → `OptimizeVertexFetch<TVertex>` → `EncodeIndexBuffer` / `EncodeVertexBuffer<TVertex>`.
- meshlet path: `BuildMeshlets` sized by `BuildMeshletsBound` → `OptimizeMeshlet` per meshlet → `ComputeMeshletBounds` for culling.
- simplify path: `Simplify` / `SimplifyWithAttributes` (`SimplificationOptions` + `out float error`) → optional `SimplifyPrune`, normalizing the budget with `SimplifyScale`.
- drop to the extern twin solely for an already-pinned caller buffer; subscribe `Meshopt.ResolveLibrary` or `SetAllocator` before first call when deploying outside the standard NuGet native layout.

[RAIL_LAW]:
- Package: `Alimer.Bindings.MeshOptimizer`
- Owns: index/vertex optimization, `EXT_meshopt_compression` encode/decode, LOD simplification, meshlet generation, spatial clustering/partitioning, and cache/overdraw/fetch/coverage analysis.
- Accept: canonical interleaved vertex structs as `Span<TVertex>`/`ReadOnlySpan<TVertex>` (`TVertex: unmanaged`) through the managed tier; caller-pinned pointers through the extern twin only when the buffer is already pinned.
- Reject: hand-rolled vertex-cache optimization, meshlet partition algorithms, custom half-float/octahedral/quaternion filter encode, and manual pinning around a managed overload that already owns it.
