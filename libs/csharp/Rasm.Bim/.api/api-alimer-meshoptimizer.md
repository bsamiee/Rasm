# [RASM_BIM_API_MESHOPTIMIZER]

`Alimer.Bindings.MeshOptimizer` exposes the full `meshoptimizer` C library surface as
a single static P/Invoke class `Meshopt` plus supporting value types, covering index
and vertex buffer optimization, encode/decode compression for GPU streaming, LOD
simplification, meshlet generation, spatial clustering, quantization helpers, and
cache/overdraw/fetch analysis for Bim and Compute mesh processing rails.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Alimer.Bindings.MeshOptimizer`
- package: `Alimer.Bindings.MeshOptimizer`
- assembly: `Alimer.Bindings.MeshOptimizer`
- namespace: `MeshOptimizer`
- asset: net10.0, net9.0 (native: win-x64, win-arm64, linux-x64, linux-arm64, osx, android-arm, android-arm64, android-x64)
- rail: geometry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: facade and stream input
- rail: geometry

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [ROLE]                                                                              |
| :-----: | :-------- | :------------ | :---------------------------------------------------------------------------------- |
|   [1]   | `Meshopt` | static class  | entire meshoptimizer operation surface via P/Invoke; `MESHOPTIMIZER_VERSION = 1000` |
|   [2]   | `Stream`  | unsafe struct | multi-stream vertex remap input: `data` (void*), `size` (nuint), `stride` (nuint)   |

[PUBLIC_TYPE_SCOPE]: meshlet geometry
- rail: geometry

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [FIELDS]                                                                                                                         |
| :-----: | :-------- | :------------ | :------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `Meshlet` | struct        | `vertex_offset`, `triangle_offset`, `vertex_count`, `triangle_count` (all `uint`)                                                |
|   [2]   | `Bounds`  | unsafe struct | center (float[3]), radius (float), cone_apex/axis (float[3]), cone_cutoff (float), cone_axis_s8 (byte[3]), cone_cutoff_s8 (byte) |

[PUBLIC_TYPE_SCOPE]: simplification and encoding options
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CASES]                                                                                                   |
| :-----: | :---------------------- | :------------ | :-------------------------------------------------------------------------------------------------------- |
|   [1]   | `SimplificationOptions` | [Flags] enum  | `None=0`, `SimplifyLockBorder=1`, `meshopt_SimplifySparse=2`, `meshopt_SimplifyErrorAbsolute=4`           |
|   [2]   | `EncodeExpMode`         | enum          | `None/EncodeExpSeparate=0`, `EncodeExpSharedVector=1`, `EncodeExpSharedComponent=2`, `EncodeExpClamped=3` |

[PUBLIC_TYPE_SCOPE]: analysis result structs
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [FIELDS]                                                                |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------------------- |
|   [1]   | `VertexCacheStatistics` | struct        | `vertices_transformed`, `warps_executed` (uint); `acmr`, `atvr` (float) |
|   [2]   | `VertexFetchStatistics` | struct        | `bytes_fetched` (uint); `overfetch` (float)                             |
|   [3]   | `OverdrawStatistics`    | struct        | `pixels_covered`, `pixels_shaded` (uint); `overdraw` (float)            |
|   [4]   | `CoverageStatistics`    | unsafe struct | `coverage` (float[3]); `extent` (float)                                 |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: vertex remap and index generation
- rail: geometry

| [INDEX] | [SURFACE]                         | [SIGNATURE_SHAPE]                                                                                    | [ROLE]                                    |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------------------------- | :---------------------------------------- |
|   [1]   | `GenerateVertexRemap`             | `(uint* dst, uint* idx, nuint idx_cnt, void* verts, nuint vert_cnt, nuint vert_sz)` → `nuint`        | builds remap table eliminating duplicates |
|   [2]   | `GenerateVertexRemapMulti`        | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt, Stream* streams, nuint stream_cnt)` → `nuint` | multi-stream remap                        |
|   [3]   | `GeneratePositionRemap`           | `(uint* dst, float* positions, nuint vert_cnt, nuint stride)`                                        | position-only dedup remap                 |
|   [4]   | `GenerateShadowIndexBuffer`       | `(uint* dst, uint* idx, nuint idx_cnt, void* verts, nuint vert_cnt, nuint vert_sz, nuint stride)`    | shadow index buffer                       |
|   [5]   | `GenerateShadowIndexBufferMulti`  | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt, Stream* streams, nuint cnt)`                  | multi-stream shadow buffer                |
|   [6]   | `GenerateAdjacencyIndexBuffer`    | `(uint* dst, uint* idx, nuint idx_cnt, float* pos, nuint vert_cnt, nuint stride)`                    | adjacency index buffer                    |
|   [7]   | `GenerateTessellationIndexBuffer` | `(uint* dst, uint* idx, nuint idx_cnt, float* pos, nuint vert_cnt, nuint stride)`                    | tessellation index buffer                 |
|   [8]   | `GenerateProvokingIndexBuffer`    | `(uint* dst, uint* reorder, uint* idx, nuint idx_cnt, nuint vert_cnt)` → `nuint`                     | provoking vertex index buffer             |
|   [9]   | `RemapVertexBuffer`               | `(void* dst, void* verts, nuint vert_cnt, nuint vert_sz, uint* remap)`                               | applies remap to vertex data              |
|  [10]   | `RemapIndexBuffer`                | `(uint* dst, uint* idx, nuint idx_cnt, uint* remap)`                                                 | applies remap to index buffer             |

[ENTRYPOINT_SCOPE]: cache and overdraw optimization
- rail: geometry

| [INDEX] | [SURFACE]                  | [SIGNATURE_SHAPE]                                                                                  | [ROLE]                         |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------- | :----------------------------- |
|   [1]   | `OptimizeVertexCache`      | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt)`                                            | tipsify vertex cache reorder   |
|   [2]   | `OptimizeVertexCacheStrip` | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt)`                                            | strip-order cache optimization |
|   [3]   | `OptimizeVertexCacheFifo`  | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt, uint cache_sz)`                             | FIFO cache optimization        |
|   [4]   | `OptimizeOverdraw`         | `(uint* dst, uint* idx, nuint idx_cnt, float* pos, nuint vert_cnt, nuint stride, float threshold)` | overdraw optimization          |
|   [5]   | `OptimizeVertexFetch`      | `(void* dst, uint* idx, nuint idx_cnt, void* verts, nuint vert_cnt, nuint vert_sz)` → `nuint`      | vertex fetch reorder           |
|   [6]   | `OptimizeVertexFetchRemap` | `(uint* dst, uint* idx, nuint idx_cnt, nuint vert_cnt)` → `nuint`                                  | fetch-optimal remap table      |

[ENTRYPOINT_SCOPE]: index buffer encode/decode
- rail: geometry

| [INDEX] | [SURFACE]                  | [SIGNATURE_SHAPE]                                                           | [ROLE]                                 |
| :-----: | :------------------------- | :-------------------------------------------------------------------------- | :------------------------------------- |
|   [1]   | `EncodeIndexBuffer`        | `(byte* buf, nuint buf_sz, uint* idx, nuint idx_cnt)` → `nuint`             | encodes index buffer for GPU streaming |
|   [2]   | `EncodeIndexBufferBound`   | `(nuint idx_cnt, nuint vert_cnt)` → `nuint`                                 | max output byte size for encode buffer |
|   [3]   | `EncodeIndexVersion`       | `(int version)`                                                             | selects encode format version          |
|   [4]   | `DecodeIndexBuffer`        | `(void* dst, nuint idx_cnt, nuint idx_sz, byte* buf, nuint buf_sz)` → `int` | decodes index buffer                   |
|   [5]   | `DecodeIndexVersion`       | `(byte* buf, nuint buf_sz)` → `int`                                         | reads encode version from buffer       |
|   [6]   | `EncodeIndexSequence`      | `(byte* buf, nuint buf_sz, uint* idx, nuint idx_cnt)` → `nuint`             | encodes index sequence (non-triangle)  |
|   [7]   | `EncodeIndexSequenceBound` | `(nuint idx_cnt, nuint vert_cnt)` → `nuint`                                 | bound for sequence encode              |
|   [8]   | `DecodeIndexSequence`      | `(void* dst, nuint idx_cnt, nuint idx_sz, byte* buf, nuint buf_sz)` → `int` | decodes index sequence                 |

[ENTRYPOINT_SCOPE]: vertex buffer encode/decode and filters
- rail: geometry

| [INDEX] | [SURFACE]                 | [SIGNATURE_SHAPE]                                                                                         | [ROLE]                                  |
| :-----: | :------------------------ | :-------------------------------------------------------------------------------------------------------- | :-------------------------------------- |
|   [1]   | `EncodeVertexBuffer`      | `(byte* buf, nuint buf_sz, void* verts, nuint vert_cnt, nuint vert_sz)` → `nuint`                         | encodes vertex buffer for GPU streaming |
|   [2]   | `EncodeVertexBufferBound` | `(nuint vert_cnt, nuint vert_sz)` → `nuint`                                                               | max output byte size                    |
|   [3]   | `EncodeVertexBufferLevel` | `(byte* buf, nuint buf_sz, void* verts, nuint vert_cnt, nuint vert_sz, int level, int version)` → `nuint` | versioned encode with level             |
|   [4]   | `EncodeVertexVersion`     | `(int version)`                                                                                           | selects vertex encode format version    |
|   [5]   | `DecodeVertexBuffer`      | `(void* dst, nuint vert_cnt, nuint vert_sz, byte* buf, nuint buf_sz)` → `int`                             | decodes vertex buffer                   |
|   [6]   | `DecodeVertexVersion`     | `(byte* buf, nuint buf_sz)` → `int`                                                                       | reads vertex encode version from buffer |
|   [7]   | `DecodeFilterOct`         | `(void* buf, nuint count, nuint stride)`                                                                  | decodes octahedral normal filter        |
|   [8]   | `DecodeFilterQuat`        | `(void* buf, nuint count, nuint stride)`                                                                  | decodes quaternion filter               |
|   [9]   | `DecodeFilterExp`         | `(void* buf, nuint count, nuint stride)`                                                                  | decodes exponent filter                 |
|  [10]   | `DecodeFilterColor`       | `(void* buf, nuint count, nuint stride)`                                                                  | decodes color filter                    |
|  [11]   | `EncodeFilterOct`         | `(void* dst, nuint count, nuint stride, int bits, float* data)`                                           | encodes normals to octahedral format    |
|  [12]   | `EncodeFilterQuat`        | `(void* dst, nuint count, nuint stride, int bits, float* data)`                                           | encodes quaternions                     |
|  [13]   | `EncodeFilterExp`         | `(void* dst, nuint count, nuint stride, int bits, float* data, EncodeExpMode mode)`                       | encodes floats as exponent-separated    |
|  [14]   | `EncodeFilterColor`       | `(void* dst, nuint count, nuint stride, int bits, float* data)`                                           | encodes color data                      |

[ENTRYPOINT_SCOPE]: LOD simplification
- rail: geometry

| [INDEX] | [SURFACE]                | [ROLE]                                                                        |
| :-----: | :----------------------- | :---------------------------------------------------------------------------- |
|   [1]   | `Simplify`               | surface simplification with error threshold and `SimplificationOptions` flags |
|   [2]   | `SimplifyWithAttributes` | attribute-weighted simplification with per-vertex lock mask                   |
|   [3]   | `SimplifyWithUpdate`     | in-place simplification updating existing index buffer                        |
|   [4]   | `SimplifySloppy`         | fast aggressive simplification with per-vertex lock mask                      |
|   [5]   | `SimplifyPrune`          | removes disconnected triangles below error threshold                          |
|   [6]   | `SimplifyPoints`         | point cloud simplification with optional color weighting                      |
|   [7]   | `SimplifyScale`          | returns world-space scale factor for error normalization                      |

[ENTRYPOINT_SCOPE]: meshlets and spatial clustering
- rail: geometry

| [INDEX] | [SURFACE]              | [ROLE]                                                                                 |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------- |
|   [1]   | `BuildMeshlets`        | builds meshlets with cone culling; returns count, fills `Meshlet[]`/vertex/tri buffers |
|   [2]   | `BuildMeshletsScan`    | sequential scan-based meshlet build without cone culling                               |
|   [3]   | `BuildMeshletsBound`   | upper bound on meshlet count for buffer allocation                                     |
|   [4]   | `BuildMeshletsFlex`    | flexible meshlet build with min/max triangles and split factor                         |
|   [5]   | `BuildMeshletsSpatial` | spatially-coherent meshlet build with fill weight                                      |
|   [6]   | `OptimizeMeshlet`      | reorders meshlet vertices and triangles for cache efficiency                           |
|   [7]   | `ComputeClusterBounds` | computes `Bounds` (sphere + cone) for a cluster of triangles                           |
|   [8]   | `ComputeMeshletBounds` | computes `Bounds` for a meshlet vertex/triangle set                                    |
|   [9]   | `ComputeSphereBounds`  | computes bounding sphere from positions and per-point radii                            |
|  [10]   | `PartitionClusters`    | partitions clusters into target-size groups for two-level culling                      |
|  [11]   | `SpatialSortRemap`     | generates spatially coherent vertex remap for improved locality                        |
|  [12]   | `SpatialSortTriangles` | reorders triangles by centroid for spatial locality                                    |
|  [13]   | `SpatialClusterPoints` | clusters points into spatially coherent groups                                         |

[ENTRYPOINT_SCOPE]: strip encoding and analysis
- rail: geometry

| [INDEX] | [SURFACE]            | [ROLE]                                                     |
| :-----: | :------------------- | :--------------------------------------------------------- |
|   [1]   | `Stripify`           | converts indexed triangle list to triangle strip           |
|   [2]   | `StripifyBound`      | max output index count for strip buffer allocation         |
|   [3]   | `Unstripify`         | converts triangle strip back to indexed triangle list      |
|   [4]   | `UnstripifyBound`    | max output index count for unstrip buffer allocation       |
|   [5]   | `AnalyzeVertexCache` | returns `VertexCacheStatistics` (ACMR, ATVR)               |
|   [6]   | `AnalyzeVertexFetch` | returns `VertexFetchStatistics` (bytes fetched, overfetch) |
|   [7]   | `AnalyzeOverdraw`    | returns `OverdrawStatistics` (pixels covered/shaded)       |
|   [8]   | `AnalyzeCoverage`    | returns `CoverageStatistics` (coverage extents)            |

[ENTRYPOINT_SCOPE]: quantization helpers
- rail: geometry

| [INDEX] | [SURFACE]        | [SIGNATURE_SHAPE]            | [ROLE]                             |
| :-----: | :--------------- | :--------------------------- | :--------------------------------- |
|   [1]   | `QuantizeHalf`   | `(float v)` → `ushort`       | float → fp16 half-precision        |
|   [2]   | `QuantizeFloat`  | `(float v, int N)` → `float` | quantizes float to N mantissa bits |
|   [3]   | `DequantizeHalf` | `(ushort h)` → `float`       | fp16 half-precision → float        |

## [4]-[IMPLEMENTATION_LAW]

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
