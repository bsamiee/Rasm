# [RASM_BIM_API_MESHOPTIMIZER]

`Alimer.Bindings.MeshOptimizer` exposes the full `meshoptimizer` C library surface as
a single static P/Invoke class `Meshopt` plus supporting value types, covering index
and vertex buffer optimization, encode/decode compression for GPU streaming, LOD
simplification, meshlet generation, spatial clustering, quantization helpers, and
cache/overdraw/fetch analysis for Bim and Compute mesh processing rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Alimer.Bindings.MeshOptimizer`
- package: `Alimer.Bindings.MeshOptimizer`
- assembly: `Alimer.Bindings.MeshOptimizer`
- namespace: `MeshOptimizer`
- asset: net10.0, net9.0 (native: win-x64, win-arm64, linux-x64, linux-arm64, osx, android-arm, android-arm64, android-x64)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: facade and stream input
- rail: geometry

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [ROLE]                                                                              |
| :-----: | :-------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `Meshopt` | static class  | entire meshoptimizer operation surface via P/Invoke; `MESHOPTIMIZER_VERSION = 1000` |
|  [02]   | `Stream`  | unsafe struct | multi-stream vertex remap input: `data` (void*), `size` (nuint), `stride` (nuint)   |

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

[ENTRYPOINT_SCOPE]: vertex remap and index generation
- rail: geometry

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
- native library: `meshoptimizer` (`.dll` on Windows, `.dylib` on macOS, `.so` on Linux/Android); loaded via `NativeLibrary.SetDllImportResolver` at static init
- `Meshopt.ResolveLibrary` is a `DllImportResolver` event for custom native load paths before the built-in resolver runs
- all methods are `unsafe static extern`; all pointer arguments require pinned or stack-allocated buffers from the caller
- `SimplificationOptions` is a `[Flags]` enum; flags compose with bitwise OR
- `EncodeIndexVersion` and `EncodeVertexVersion` must be called before encoding when targeting a specific bitstream format version

[LOCAL_ADMISSION]:
- Index path: `GenerateVertexRemap` → `RemapVertexBuffer` + `RemapIndexBuffer` → `OptimizeVertexCache` → `OptimizeOverdraw` → `OptimizeVertexFetch` → `EncodeIndexBuffer` / `EncodeVertexBuffer`
- Meshlet path: `BuildMeshlets` (allocate via `BuildMeshletsBound`) → `OptimizeMeshlet` per meshlet → `ComputeMeshletBounds` for culling
- Simplification path: `Simplify` / `SimplifyWithAttributes` with `SimplificationOptions` flags → optional `SimplifyPrune` for cleanup
- Custom native resolution: subscribe `Meshopt.ResolveLibrary` before first P/Invoke call when deploying outside standard NuGet layout

[RAIL_LAW]:
- Package: `Alimer.Bindings.MeshOptimizer`
- Owns: index/vertex optimization, encode/decode compression, LOD simplification, meshlet generation, spatial clustering, cache analysis
- Accept: unsafe pointer inputs from pinned managed arrays, `NativeMemory`, or `stackalloc` buffers
- Reject: hand-rolled vertex cache optimization, meshlet partition algorithms, or custom half-float encode/decode
