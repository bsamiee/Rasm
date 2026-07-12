# [RASM_BIM_API_MESHOPTIMIZER]

`Alimer.Bindings.MeshOptimizer` exposes the full `meshoptimizer` C library through a
single static class `Meshopt` plus supporting value types. The first-class surface is
the MANAGED generic tier — `Span<TVertex>`/`ReadOnlySpan<T>` overloads constrained
`where TVertex: unmanaged`, with `out float error` and `Bounds`/`*Statistics` value
returns — that internally pins and forwards to the raw `unsafe static extern` P/Invoke
twins. A dense rail composes the managed tier (no manual pinning); the extern tier is
the fallback for caller-owned `stackalloc`/`NativeMemory` buffers. Covers index and
vertex buffer optimization, encode/decode compression for GPU streaming, LOD
simplification, meshlet generation, spatial clustering and partitioning, quantization
helpers, and cache/overdraw/fetch/coverage analysis for Bim and Compute mesh rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Alimer.Bindings.MeshOptimizer`
- package: `Alimer.Bindings.MeshOptimizer`
- assembly: `Alimer.Bindings.MeshOptimizer`
- namespace: `MeshOptimizer`
- asset: net10.0, net9.0 — `net10.0` consumer binds `lib/net10.0`
- asset: native `runtimes/<rid>/native/` for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx` (universal arm64+x64 fat dylib), `android-arm`, `android-arm64`, `android-x64`; `net10.0`-only TFM firebreaks it out of any `net48` in-Rhino plugin ALC
- license: MIT (`LICENSE.md` file, `requireLicenseAcceptance=true`); admitted for the Compute `EXPORT_RAIL`, outside-Rhino only
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: facade and stream input
- rail: geometry

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [ROLE]                                                                                                                                                                                                  |
| :-----: | :-------- | :------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Meshopt` | static class  | entire operation surface: managed `Span<T>` tier + raw extern tier; public consts `MESHOPTIMIZER_VERSION = 1000u`, `LibraryName = "meshoptimizer"`; static event `ResolveLibrary` (`DllImportResolver`) |
|  [02]   | `Stream`  | unsafe struct | multi-stream vertex remap input (primary-ctor `(void* data, nuint size, nuint stride)`); the managed `GenerateVertexRemapMulti<TVertex>` overloads take `ReadOnlySpan<Stream>`                          |

[PUBLIC_TYPE_SCOPE]: meshlet geometry
- rail: geometry

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [FIELDS]                                                                                                                         |
| :-----: | :-------- | :------------ | :------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Meshlet` | struct        | `vertex_offset`, `triangle_offset`, `vertex_count`, `triangle_count` (all `uint`)                                                |
|  [02]   | `Bounds`  | unsafe struct | center (float[3]), radius (float), cone_apex/axis (float[3]), cone_cutoff (float), cone_axis_s8 (byte[3]), cone_cutoff_s8 (byte) |

[PUBLIC_TYPE_SCOPE]: simplification and encoding options
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CASES]                                                                                                   |
| :-----: | :---------------------- | :------------ | :-------------------------------------------------------------------------------------------------------- |
|  [01]   | `SimplificationOptions` | [Flags] enum  | `None=0`, `SimplifyLockBorder=1`, `meshopt_SimplifySparse=2`, `meshopt_SimplifyErrorAbsolute=4`           |
|  [02]   | `EncodeExpMode`         | enum          | `None/EncodeExpSeparate=0`, `EncodeExpSharedVector=1`, `EncodeExpSharedComponent=2`, `EncodeExpClamped=3` |

[PUBLIC_TYPE_SCOPE]: analysis result structs
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [FIELDS]                                                                |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `VertexCacheStatistics` | struct        | `vertices_transformed`, `warps_executed` (uint); `acmr`, `atvr` (float) |
|  [02]   | `VertexFetchStatistics` | struct        | `bytes_fetched` (uint); `overfetch` (float)                             |
|  [03]   | `OverdrawStatistics`    | struct        | `pixels_covered`, `pixels_shaded` (uint); `overdraw` (float)            |
|  [04]   | `CoverageStatistics`    | unsafe struct | `coverage` (float[3]); `extent` (float)                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: managed `Span<T>` tier — the first-class rail surface
- rail: geometry

Every algorithm ships a managed overload that pins the spans internally and forwards to the extern twin. These are the surface a Rasm rail composes — no caller pinning, generic over the vertex struct (`where TVertex: unmanaged`), error/`Bounds`/`*Statistics` returned by value. Pass canonical interleaved vertex structs directly as `ReadOnlySpan<TVertex>`; the index pipeline stays `uint`.

| [INDEX] | [SURFACE]                                                            | [SIGNATURE_SHAPE]                                                                                                                                                                                                                     | [ROLE]                                           |
| :-----: | :------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :----------------------------------------------- |
|  [01]   | `GenerateVertexRemap<TVertex>`                                       | `(Span<uint> dst, ReadOnlySpan<uint> idx, ReadOnlySpan<TVertex> verts)` → `nuint` (also `(dst, nuint idxCount, verts)` for unindexed)                                                                                                 | typed remap table eliminating duplicate verts    |
|  [02]   | `GenerateVertexRemapMulti<TVertex>`                                  | `(Span<uint> dst, ReadOnlySpan<uint> idx, nuint vertCnt, ReadOnlySpan<Stream> streams)` → `nuint`                                                                                                                                     | multi-stream typed remap                         |
|  [03]   | `RemapVertexBuffer<TVertex>`                                         | `(Span<TVertex> dst, ReadOnlySpan<TVertex> verts, ReadOnlySpan<uint> remap)`                                                                                                                                                          | applies remap to typed vertex data               |
|  [04]   | `RemapIndexBuffer`                                                   | `(Span<uint> dst, ReadOnlySpan<uint> idx, ReadOnlySpan<uint> remap)` (also `(dst, nuint idxCount, remap)`)                                                                                                                            | applies remap to index buffer                    |
|  [05]   | `OptimizeVertexCache`                                                | `(Span<uint> dst, ReadOnlySpan<uint> idx, nuint vertCnt)`                                                                                                                                                                             | tipsify cache reorder (also `…Strip`, `…Fifo`)   |
|  [06]   | `OptimizeOverdraw`                                                   | `(Span<uint> dst, ReadOnlySpan<uint> idx, ReadOnlySpan<float> pos, nuint stride, float threshold)`                                                                                                                                    | overdraw reorder                                 |
|  [07]   | `OptimizeVertexFetch<TVertex>`                                       | `(Span<uint> dst, Span<uint> idx, ReadOnlySpan<TVertex> verts)` → `nuint`                                                                                                                                                             | fetch reorder, rewrites idx in place             |
|  [08]   | `EncodeVertexBuffer<TVertex>`                                        | `(Span<byte> buf, ReadOnlySpan<TVertex> verts)` → `nuint`; `EncodeVertexBufferLevel<TVertex>(buf, verts, int level, int version = -1)`                                                                                                | typed vertex encode (`EXT_meshopt_compression`)  |
|  [09]   | `DecodeVertexBuffer<TVertex>`                                        | `(Span<TVertex> dst, ReadOnlySpan<byte> buf)` → `int`                                                                                                                                                                                 | typed vertex decode                              |
|  [10]   | `EncodeIndexBuffer`                                                  | `(Span<byte> buf, ReadOnlySpan<uint> idx)` → `nuint`; `DecodeIndexBuffer<TIndex>(Span<TIndex> dst, ReadOnlySpan<byte> buf)` → `int`                                                                                                   | index encode/decode (typed index out)            |
|  [11]   | `DecodeIndexVersion` / `DecodeVertexVersion`                         | `(ReadOnlySpan<byte> buf)` → `int`                                                                                                                                                                                                    | reads bitstream version from a buffer            |
|  [12]   | `EncodeFilterOct<T>` / `…Quat` / `…Exp` / `…Color`                   | `(Span<T> dst, int bits, ReadOnlySpan<float> data[, EncodeExpMode mode])`; `DecodeFilter*<T>(Span<T> buf)`                                                                                                                            | typed vertex-attribute filters                   |
|  [13]   | `Simplify`                                                           | `(Span<uint> dst, ReadOnlySpan<uint> idx, ReadOnlySpan<float> pos, nuint stride, nuint targetIdxCount, float targetError, SimplificationOptions opts, out float error)` → `nuint`                                                     | LOD simplify with error readback                 |
|  [14]   | `SimplifyWithAttributes`                                             | `(…, ReadOnlySpan<float> attrs, nuint attrStride, ReadOnlySpan<float> attrWeights, nuint attrCount[, ReadOnlySpan<byte> vertexLock], nuint targetIdxCount, float targetError, SimplificationOptions opts, out float error)` → `nuint` | attribute-weighted simplify (optional lock mask) |
|  [15]   | `SimplifySloppy` / `SimplifyPrune` / `SimplifyPoints`                | `(Span<uint> dst, …, out float error)` / `(…, float targetError)` → `nuint`; `SimplifyScale(ReadOnlySpan<float> pos, nuint stride)` → `float`                                                                                         | aggressive/prune/point-cloud simplify + scale    |
|  [16]   | `BuildMeshlets`                                                      | `(Span<Meshlet> meshlets, Span<uint> meshletVerts, Span<byte> meshletTris, ReadOnlySpan<uint> idx, ReadOnlySpan<float> pos, nuint stride, nuint maxVerts, nuint maxTris, float coneWeight)` → `nuint`                                 | meshlet build with cone weight                   |
|  [17]   | `BuildMeshletsFlex` / `BuildMeshletsSpatial`                         | `(…, nuint maxVerts, nuint minTris, nuint maxTris, float coneWeight, float splitFactor)` / `(…, float fillWeight)` → `nuint`                                                                                                          | flexible / spatially-coherent meshlet build      |
|  [18]   | `ComputeClusterBounds` / `ComputeMeshletBounds`                      | `(ReadOnlySpan<uint> idx\| meshletVerts[, ReadOnlySpan<byte> meshletTris], ReadOnlySpan<float> pos, nuint stride)` → `Bounds`                                                                                                         | sphere + cone `Bounds` by value                  |
|  [19]   | `ComputeSphereBounds`                                                | `(ReadOnlySpan<float> pos, nuint stride[, ReadOnlySpan<float> radii, nuint radiiStride])` → `Bounds`                                                                                                                                  | bounding sphere (optional per-point radii)       |
|  [20]   | `PartitionClusters`                                                  | `(Span<uint> dst, ReadOnlySpan<uint> clusterIdx, ReadOnlySpan<uint> clusterIdxCounts, ReadOnlySpan<float> pos, nuint stride, nuint targetPartitionSize)` → `nuint`                                                                    | two-level cluster partition                      |
|  [21]   | `SpatialSortRemap` / `SpatialSortTriangles` / `SpatialClusterPoints` | `(Span<uint> dst, ReadOnlySpan<float> pos, nuint stride[, nuint clusterSize])`                                                                                                                                                        | spatial locality remap/reorder/cluster           |
|  [22]   | `AnalyzeVertexCache`                                                 | `(ReadOnlySpan<uint> idx, nuint vertCnt, uint cacheSize, uint warpSize, uint primGroupSize)` → `VertexCacheStatistics`                                                                                                                | ACMR/ATVR by value                               |
|  [23]   | `AnalyzeOverdraw` / `AnalyzeVertexFetch` / `AnalyzeCoverage`         | `(ReadOnlySpan<uint> idx, …)` → `OverdrawStatistics` / `VertexFetchStatistics` / `CoverageStatistics`                                                                                                                                 | overdraw/fetch/coverage stats by value           |
|  [24]   | `Stripify` / `Unstripify`                                            | `(Span<uint> dst, ReadOnlySpan<uint> idx, nuint vertCnt, uint restartIndex)` / `(dst, idx, uint restartIndex)` → `nuint`                                                                                                              | strip ↔ list with explicit restart index         |
|  [25]   | `GenerateVertexRemapCustom`                                          | `(Span<uint> dst, ReadOnlySpan<uint> idx, ReadOnlySpan<float> pos, nuint stride, delegate*<nint,uint,uint,int> callback, nint context)` → `nuint`                                                                                     | remap with a custom unmanaged equality callback  |

[ENTRYPOINT_SCOPE]: raw extern P/Invoke tier — caller-pinned fallback
- rail: geometry

The `unsafe static extern` twins each managed overload forwards to. Compose these only when the caller already owns a pinned `stackalloc`/`NativeMemory`/`fixed` pointer and wants to skip the managed pin; otherwise prefer the `Span<T>` tier above.

| [INDEX] | [SURFACE]                         | [SIGNATURE_SHAPE]                                                                                    | [ROLE]                                    |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `GenerateVertexRemap`             | `(uint* dst, uint* idx, nuint idx_cnt, void* verts, nuint vert_cnt, nuint vert_sz)` → `nuint`        | builds remap table eliminating duplicates |
|  [02]   | `GenerateVertexRemapMulti`        | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt, Stream* streams, nuint stream_cnt)` → `nuint` | multi-stream remap                        |
|  [03]   | `GeneratePositionRemap`           | `(uint* dst, float* positions, nuint vert_cnt, nuint stride)`                                        | position-only dedup remap                 |
|  [04]   | `GenerateShadowIndexBuffer`       | `(uint* dst, uint* idx, nuint idx_cnt, void* verts, nuint vert_cnt, nuint vert_sz, nuint stride)`    | shadow index buffer                       |
|  [05]   | `GenerateShadowIndexBufferMulti`  | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt, Stream* streams, nuint cnt)`                  | multi-stream shadow buffer                |
|  [06]   | `GenerateAdjacencyIndexBuffer`    | `(uint* dst, uint* idx, nuint idx_cnt, float* pos, nuint vert_cnt, nuint stride)`                    | adjacency index buffer                    |
|  [07]   | `GenerateTessellationIndexBuffer` | `(uint* dst, uint* idx, nuint idx_cnt, float* pos, nuint vert_cnt, nuint stride)`                    | tessellation index buffer                 |
|  [08]   | `GenerateProvokingIndexBuffer`    | `(uint* dst, uint* reorder, uint* idx, nuint idx_cnt, nuint vert_cnt)` → `nuint`                     | provoking vertex index buffer             |
|  [09]   | `RemapVertexBuffer`               | `(void* dst, void* verts, nuint vert_cnt, nuint vert_sz, uint* remap)`                               | applies remap to vertex data              |
|  [10]   | `RemapIndexBuffer`                | `(uint* dst, uint* idx, nuint idx_cnt, uint* remap)`                                                 | applies remap to index buffer             |

[ENTRYPOINT_SCOPE]: cache and overdraw optimization
- rail: geometry

| [INDEX] | [SURFACE]                  | [SIGNATURE_SHAPE]                                                                                  | [ROLE]                         |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------- | :----------------------------- |
|  [01]   | `OptimizeVertexCache`      | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt)`                                            | tipsify vertex cache reorder   |
|  [02]   | `OptimizeVertexCacheStrip` | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt)`                                            | strip-order cache optimization |
|  [03]   | `OptimizeVertexCacheFifo`  | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt, uint cache_sz)`                             | FIFO cache optimization        |
|  [04]   | `OptimizeOverdraw`         | `(uint* dst, uint* idx, nuint idx_cnt, float* pos, nuint vert_cnt, nuint stride, float threshold)` | overdraw optimization          |
|  [05]   | `OptimizeVertexFetch`      | `(void* dst, uint* idx, nuint idx_cnt, void* verts, nuint vert_cnt, nuint vert_sz)` → `nuint`      | vertex fetch reorder           |
|  [06]   | `OptimizeVertexFetchRemap` | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt)` → `nuint`                                  | fetch-optimal remap table      |

[ENTRYPOINT_SCOPE]: index buffer encode/decode
- rail: geometry

| [INDEX] | [SURFACE]                  | [SIGNATURE_SHAPE]                                                           | [ROLE]                                 |
| :-----: | :------------------------- | :-------------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `EncodeIndexBuffer`        | `(byte* buf, nuint buf_sz, uint* idx, nuint idx_cnt)` → `nuint`             | encodes index buffer for GPU streaming |
|  [02]   | `EncodeIndexBufferBound`   | `(nuint idx_cnt, nuint vert_cnt)` → `nuint`                                 | max output byte size for encode buffer |
|  [03]   | `EncodeIndexVersion`       | `(int version)`                                                             | selects encode format version          |
|  [04]   | `DecodeIndexBuffer`        | `(void* dst, nuint idx_cnt, nuint idx_sz, byte* buf, nuint buf_sz)` → `int` | decodes index buffer                   |
|  [05]   | `DecodeIndexVersion`       | `(byte* buf, nuint buf_sz)` → `int`                                         | reads encode version from buffer       |
|  [06]   | `EncodeIndexSequence`      | `(byte* buf, nuint buf_sz, uint* idx, nuint idx_cnt)` → `nuint`             | encodes index sequence (non-triangle)  |
|  [07]   | `EncodeIndexSequenceBound` | `(nuint idx_cnt, nuint vert_cnt)` → `nuint`                                 | bound for sequence encode              |
|  [08]   | `DecodeIndexSequence`      | `(void* dst, nuint idx_cnt, nuint idx_sz, byte* buf, nuint buf_sz)` → `int` | decodes index sequence                 |

[ENTRYPOINT_SCOPE]: vertex buffer encode/decode and filters
- rail: geometry

| [INDEX] | [SURFACE]                 | [SIGNATURE_SHAPE]                                                                                         | [ROLE]                                  |
| :-----: | :------------------------ | :-------------------------------------------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `EncodeVertexBuffer`      | `(byte* buf, nuint buf_sz, void* verts, nuint vert_cnt, nuint vert_sz)` → `nuint`                         | encodes vertex buffer for GPU streaming |
|  [02]   | `EncodeVertexBufferBound` | `(nuint vert_cnt, nuint vert_sz)` → `nuint`                                                               | max output byte size                    |
|  [03]   | `EncodeVertexBufferLevel` | `(byte* buf, nuint buf_sz, void* verts, nuint vert_cnt, nuint vert_sz, int level, int version)` → `nuint` | versioned encode with level             |
|  [04]   | `EncodeVertexVersion`     | `(int version)`                                                                                           | selects vertex encode format version    |
|  [05]   | `DecodeVertexBuffer`      | `(void* dst, nuint vert_cnt, nuint vert_sz, byte* buf, nuint buf_sz)` → `int`                             | decodes vertex buffer                   |
|  [06]   | `DecodeVertexVersion`     | `(byte* buf, nuint buf_sz)` → `int`                                                                       | reads vertex encode version from buffer |
|  [07]   | `DecodeFilterOct`         | `(void* buf, nuint count, nuint stride)`                                                                  | decodes octahedral normal filter        |
|  [08]   | `DecodeFilterQuat`        | `(void* buf, nuint count, nuint stride)`                                                                  | decodes quaternion filter               |
|  [09]   | `DecodeFilterExp`         | `(void* buf, nuint count, nuint stride)`                                                                  | decodes exponent filter                 |
|  [10]   | `DecodeFilterColor`       | `(void* buf, nuint count, nuint stride)`                                                                  | decodes color filter                    |
|  [11]   | `EncodeFilterOct`         | `(void* dst, nuint count, nuint stride, int bits, float* data)`                                           | encodes normals to octahedral format    |
|  [12]   | `EncodeFilterQuat`        | `(void* dst, nuint count, nuint stride, int bits, float* data)`                                           | encodes quaternions                     |
|  [13]   | `EncodeFilterExp`         | `(void* dst, nuint count, nuint stride, int bits, float* data, EncodeExpMode mode)`                       | encodes floats as exponent-separated    |
|  [14]   | `EncodeFilterColor`       | `(void* dst, nuint count, nuint stride, int bits, float* data)`                                           | encodes color data                      |

[ENTRYPOINT_SCOPE]: LOD simplification
- rail: geometry

| [INDEX] | [SURFACE]                | [ROLE]                                                                        |
| :-----: | :----------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `Simplify`               | surface simplification with error threshold and `SimplificationOptions` flags |
|  [02]   | `SimplifyWithAttributes` | attribute-weighted simplification with per-vertex lock mask                   |
|  [03]   | `SimplifyWithUpdate`     | in-place simplification updating existing index buffer                        |
|  [04]   | `SimplifySloppy`         | fast aggressive simplification with per-vertex lock mask                      |
|  [05]   | `SimplifyPrune`          | removes disconnected triangles below error threshold                          |
|  [06]   | `SimplifyPoints`         | point cloud simplification with optional color weighting                      |
|  [07]   | `SimplifyScale`          | returns world-space scale factor for error normalization                      |

[ENTRYPOINT_SCOPE]: meshlets and spatial clustering
- rail: geometry

| [INDEX] | [SURFACE]              | [ROLE]                                                                                 |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `BuildMeshlets`        | builds meshlets with cone culling; returns count, fills `Meshlet[]`/vertex/tri buffers |
|  [02]   | `BuildMeshletsScan`    | sequential scan-based meshlet build without cone culling                               |
|  [03]   | `BuildMeshletsBound`   | upper bound on meshlet count for buffer allocation                                     |
|  [04]   | `BuildMeshletsFlex`    | flexible meshlet build with min/max triangles and split factor                         |
|  [05]   | `BuildMeshletsSpatial` | spatially-coherent meshlet build with fill weight                                      |
|  [06]   | `OptimizeMeshlet`      | reorders meshlet vertices and triangles for cache efficiency                           |
|  [07]   | `ComputeClusterBounds` | computes `Bounds` (sphere + cone) for a cluster of triangles                           |
|  [08]   | `ComputeMeshletBounds` | computes `Bounds` for a meshlet vertex/triangle set                                    |
|  [09]   | `ComputeSphereBounds`  | computes bounding sphere from positions and per-point radii                            |
|  [10]   | `PartitionClusters`    | partitions clusters into target-size groups for two-level culling                      |
|  [11]   | `SpatialSortRemap`     | generates spatially coherent vertex remap for improved locality                        |
|  [12]   | `SpatialSortTriangles` | reorders triangles by centroid for spatial locality                                    |
|  [13]   | `SpatialClusterPoints` | clusters points into spatially coherent groups                                         |

[ENTRYPOINT_SCOPE]: strip encoding and analysis
- rail: geometry

| [INDEX] | [SURFACE]            | [ROLE]                                                     |
| :-----: | :------------------- | :--------------------------------------------------------- |
|  [01]   | `Stripify`           | converts indexed triangle list to triangle strip           |
|  [02]   | `StripifyBound`      | max output index count for strip buffer allocation         |
|  [03]   | `Unstripify`         | converts triangle strip back to indexed triangle list      |
|  [04]   | `UnstripifyBound`    | max output index count for unstrip buffer allocation       |
|  [05]   | `AnalyzeVertexCache` | returns `VertexCacheStatistics` (ACMR, ATVR)               |
|  [06]   | `AnalyzeVertexFetch` | returns `VertexFetchStatistics` (bytes fetched, overfetch) |
|  [07]   | `AnalyzeOverdraw`    | returns `OverdrawStatistics` (pixels covered/shaded)       |
|  [08]   | `AnalyzeCoverage`    | returns `CoverageStatistics` (coverage extents)            |

[ENTRYPOINT_SCOPE]: quantization helpers
- rail: geometry

| [INDEX] | [SURFACE]        | [SIGNATURE_SHAPE]            | [ROLE]                             |
| :-----: | :--------------- | :--------------------------- | :--------------------------------- |
|  [01]   | `QuantizeHalf`   | `(float v)` → `ushort`       | float → fp16 half-precision        |
|  [02]   | `QuantizeFloat`  | `(float v, int N)` → `float` | quantizes float to N mantissa bits |
|  [03]   | `DequantizeHalf` | `(ushort h)` → `float`       | fp16 half-precision → float        |

## [04]-[IMPLEMENTATION_LAW]

[MESHOPT_TOPOLOGY]:
- namespace: `MeshOptimizer`; 10 public types in 1 namespace; entire algorithmic surface is on `Meshopt`
- `Meshopt` exposes TWO tiers of every algorithm: a managed `Span<T>`/`ReadOnlySpan<T>` overload (the first-class rail surface — pins internally, generic over `TVertex: unmanaged`, returns `out float error`/`Bounds`/`*Statistics` by value) and the raw `unsafe static extern` twin it forwards to (`meshopt_*` entry points, `[LibraryImport]`/`[DllImport]` dual-attributed). The managed tier is NOT a thin rename — it owns pinning, span-length→`nuint` count derivation, and the function-pointer marshalling for `GenerateVertexRemapCustom`.
- native library: entry-point `meshoptimizer` (`LibraryName` const; `.dll` Windows, `.dylib` macOS, `.so` Linux/Android); resolved via `NativeLibrary.SetDllImportResolver` at static init, with the `osx` RID shipping a universal arm64+x64 fat `libmeshoptimizer.dylib`
- `Meshopt.ResolveLibrary` is a static `DllImportResolver` event for custom native load paths ahead of the built-in resolver; `Meshopt.SetAllocator` installs a custom native allocator
- `SimplificationOptions` is a `[Flags]` enum; flags compose with bitwise OR (`SimplifyLockBorder \| meshopt_SimplifyErrorAbsolute`)
- `EncodeIndexVersion(int)` / `EncodeVertexVersion(int)` set the target bitstream format version process-wide before encoding; `EncodeVertexBufferLevel<TVertex>(…, int level, int version = -1)` takes the version per-call; `DecodeIndexVersion`/`DecodeVertexVersion` read it back from an encoded buffer

[INTEGRATION_STACK]:
- `Alimer.Bindings.MeshOptimizer` is the `EXT_meshopt_compression` leg of the Compute glTF `EXPORT_RAIL`, sibling to `Openize.Drako` (the `KHR_draco_mesh_compression` leg) and stacking ONTO `SharpGLTF.Core` (the glTF wire owner). One export-codec dispatch row chooses meshopt vs Draco by extension policy; `EncodeVertexBuffer<TVertex>`/`EncodeIndexBuffer` produce the `byte[]` payload SharpGLTF references under the `EXT_meshopt_compression` extension, with `EncodeFilterOct`/`…Quat`/`…Exp` pre-quantizing normals/quaternions/exponents into the filter-coded layout the extension declares.
- The pipeline composes on the canonical interleaved vertex struct — pass it straight as `ReadOnlySpan<TVertex>` to `GenerateVertexRemap<TVertex>` → `RemapVertexBuffer<TVertex>` → `OptimizeVertexCache` → `OptimizeOverdraw` → `OptimizeVertexFetch<TVertex>` → `EncodeVertexBuffer<TVertex>`, so the SAME canonical buffer that feeds Draco intake (`PointAttribute.Wrap`) feeds meshopt without a second projection.
- Meshlet/cluster output stacks onto the Compute LOD owner: `BuildMeshlets`/`BuildMeshletsSpatial` (sizes via `BuildMeshletsBound`) → `OptimizeMeshlet` per meshlet → `ComputeMeshletBounds` → `PartitionClusters` for two-level GPU culling; the `Bounds`/`*Statistics` value returns become typed receipt fields (ACMR/ATVR, overdraw, coverage extent, compression ratio), never a generic ledger.
- A Compute codec rail runs the managed tier inside the project `Fin`/`Eff` rail: span allocation, the `out float error` LOD readback, and the `nuint` written-byte count fold into one typed receipt under a per-codec telemetry span; native-load failures from `ResolveLibrary`/`SetAllocator` map to a typed boundary fault rather than an unhandled `DllNotFoundException`.

[LOCAL_ADMISSION]:
- Index path (managed tier): `GenerateVertexRemap<TVertex>` → `RemapVertexBuffer<TVertex>` + `RemapIndexBuffer` → `OptimizeVertexCache` → `OptimizeOverdraw` → `OptimizeVertexFetch<TVertex>` → `EncodeIndexBuffer` / `EncodeVertexBuffer<TVertex>`
- Meshlet path: `BuildMeshlets` (allocate via `BuildMeshletsBound`) → `OptimizeMeshlet` per meshlet → `ComputeMeshletBounds` for culling
- Simplification path: `Simplify` / `SimplifyWithAttributes` (`SimplificationOptions` flags + `out float error`) → optional `SimplifyPrune` for cleanup; normalize the error budget with `SimplifyScale`
- Pin only when caller-owned: drop to the extern twin solely for an existing `stackalloc`/`NativeMemory`/`fixed` pointer; otherwise the managed `Span<T>` overload pins internally
- Custom native resolution: subscribe `Meshopt.ResolveLibrary` (or `SetAllocator`) before first call when deploying outside standard NuGet layout

[RAIL_LAW]:
- Package: `Alimer.Bindings.MeshOptimizer`
- Owns: index/vertex optimization, encode/decode compression (`EXT_meshopt_compression`), LOD simplification, meshlet generation, spatial clustering/partitioning, cache/overdraw/fetch/coverage analysis
- Accept: canonical interleaved vertex structs as `Span<TVertex>`/`ReadOnlySpan<TVertex>` (`where TVertex: unmanaged`) through the managed tier; caller-pinned pointers through the extern twin only when the buffer is already pinned
- Reject: hand-rolled vertex cache optimization, meshlet partition algorithms, custom half-float/octahedral/quaternion filter encode, or manual pinning around the managed overloads when the span tier already owns it
