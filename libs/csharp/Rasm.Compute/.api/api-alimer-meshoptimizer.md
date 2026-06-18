# [RASM_COMPUTE_API_ALIMER_MESHOPTIMIZER]

`Alimer.Bindings.MeshOptimizer` exposes the native `meshoptimizer` library through P/Invoke
bindings in `MeshOptimizer.Meshopt`, supplying vertex remap, cache and overdraw optimization,
index and vertex buffer codec, meshlet building, spatial sorting, LOD simplification, and
quantization for mesh interchange and GPU-ready geometry preparation.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Alimer.Bindings.MeshOptimizer`
- package: `Alimer.Bindings.MeshOptimizer`
- assembly: `Alimer.Bindings.MeshOptimizer`
- namespace: `MeshOptimizer`
- asset: managed bindings + native `meshoptimizer` runtime (platform-specific shared library)
- rail: geometry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: entry-point and options family
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                |
| :-----: | :---------------------- | :-------------- | :------------------------------------ |
|   [1]   | `Meshopt`               | static bindings | all meshoptimizer P/Invoke operations |
|   [2]   | `SimplificationOptions` | flags enum      | LOD simplification control flags      |
|   [3]   | `EncodeExpMode`         | enum            | vertex buffer exponent encode mode    |

[PUBLIC_TYPE_SCOPE]: data carrier family
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                          |
| :-----: | :---------------------- | :------------- | :------------------------------ |
|   [1]   | `Stream`                | vertex stream  | multi-stream vertex remap input |
|   [2]   | `Meshlet`               | meshlet struct | meshlet index/vertex counts     |
|   [3]   | `Bounds`                | bounds struct  | cluster and meshlet bounds      |
|   [4]   | `VertexCacheStatistics` | stat struct    | vertex cache hit statistics     |
|   [5]   | `VertexFetchStatistics` | stat struct    | vertex fetch efficiency stats   |
|   [6]   | `OverdrawStatistics`    | stat struct    | overdraw ratio statistics       |
|   [7]   | `CoverageStatistics`    | stat struct    | coverage efficiency statistics  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: remap and index generation operations
- rail: geometry

| [INDEX] | [SURFACE]                                                                                                   | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------- |
|   [1]   | `GenerateVertexRemap(destination, indices, index_count, vertices, vertex_count, vertex_size)`               | remap          | build vertex remap table     |
|   [2]   | `GenerateVertexRemapMulti(destination, indices, index_count, vertex_count, streams, stream_count)`          | remap          | multi-stream remap table     |
|   [3]   | `RemapVertexBuffer(destination, vertices, vertex_count, vertex_size, remap)`                                | apply          | apply remap to vertex buffer |
|   [4]   | `RemapIndexBuffer(destination, indices, index_count, remap)`                                                | apply          | apply remap to index buffer  |
|   [5]   | `GenerateShadowIndexBuffer(destination, indices, index_count, vertices, vertex_count, vertex_size, stride)` | shadow         | shadow index generation      |
|   [6]   | `GenerateAdjacencyIndexBuffer(destination, indices, index_count, positions, vertex_count, stride)`          | topology       | adjacency index generation   |
|   [7]   | `GenerateProvokingIndexBuffer(destination, reorder, indices, index_count, vertex_count)`                    | provoking      | provoking index generation   |

[ENTRYPOINT_SCOPE]: cache and overdraw optimization
- rail: geometry

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- | :-------------------------------- |
|   [1]   | `OptimizeVertexCache(destination, indices, index_count, vertex_count)`                            | cache optimize | tipsify cache optimization        |
|   [2]   | `OptimizeVertexCacheStrip(destination, indices, index_count, vertex_count)`                       | cache optimize | strip-friendly cache optimization |
|   [3]   | `OptimizeVertexCacheFifo(destination, indices, index_count, vertex_count, cache_size)`            | cache optimize | FIFO cache optimization           |
|   [4]   | `OptimizeOverdraw(destination, indices, index_count, positions, vertex_count, stride, threshold)` | overdraw       | overdraw minimization             |
|   [5]   | `OptimizeVertexFetch(destination, indices, index_count, vertices, vertex_count, vertex_size)`     | fetch optimize | vertex fetch locality             |
|   [6]   | `OptimizeVertexFetchRemap(destination, indices, index_count, vertex_count)`                       | fetch remap    | vertex fetch remap generation     |

[ENTRYPOINT_SCOPE]: codec operations
- rail: geometry

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `EncodeIndexBuffer(buffer, buffer_size, indices, index_count)`                             | encode         | compress index buffer              |
|   [2]   | `EncodeIndexBufferBound(index_count, vertex_count)`                                        | bound          | maximum encoded index buffer size  |
|   [3]   | `DecodeIndexBuffer(destination, index_count, index_size, buffer, buffer_size)`             | decode         | decompress index buffer            |
|   [4]   | `EncodeIndexSequence(buffer, buffer_size, indices, index_count)`                           | encode         | compress index sequence            |
|   [5]   | `EncodeIndexSequenceBound(index_count, vertex_count)`                                      | bound          | maximum encoded sequence size      |
|   [6]   | `EncodeVertexBuffer(buffer, buffer_size, vertices, vertex_count, vertex_size)`             | encode         | compress vertex buffer             |
|   [7]   | `EncodeVertexBufferBound(vertex_count, vertex_size)`                                       | bound          | maximum encoded vertex buffer size |
|   [8]   | `EncodeVertexBufferLevel(buffer, buffer_size, vertices, vertex_count, vertex_size, level)` | encode         | level-compressed vertex buffer     |
|   [9]   | `DecodeVertexBuffer(destination, vertex_count, vertex_size, buffer, buffer_size)`          | decode         | decompress vertex buffer           |

[ENTRYPOINT_SCOPE]: filter, simplification, and quantization operations
- rail: geometry

| [INDEX] | [SURFACE]                                                                                                             | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `DecodeFilterOct(buffer, count, stride)`                                                                              | filter decode  | decode octahedral normal filter  |
|   [2]   | `DecodeFilterQuat(buffer, count, stride)`                                                                             | filter decode  | decode quaternion filter         |
|   [3]   | `DecodeFilterExp(buffer, count, stride)`                                                                              | filter decode  | decode exponent filter           |
|   [4]   | `EncodeFilterOct(destination, count, stride, bits, data)`                                                             | filter encode  | encode octahedral normals        |
|   [5]   | `EncodeFilterQuat(destination, count, stride, bits, data)`                                                            | filter encode  | encode quaternions               |
|   [6]   | `EncodeFilterExp(destination, count, stride, bits, data, mode)`                                                       | filter encode  | encode floats with exponent mode |
|   [7]   | `Simplify(destination, indices, index_count, positions, vertex_count, stride, target, error, options, out error_out)` | simplify       | LOD simplification               |
|   [8]   | `SimplifySloppy(destination, indices, index_count, positions, vertex_count, stride, target, error, out error_out)`    | simplify       | aggressive LOD simplification    |
|   [9]   | `SimplifyPoints(destination, positions, vertex_count, stride, colors, colors_stride, colors_weight, target)`          | simplify       | point cloud simplification       |
|  [10]   | `SimplifyScale(positions, vertex_count, stride)`                                                                      | scale          | mesh unit scale for error metric |
|  [11]   | `QuantizeHalf(v)`                                                                                                     | quantize       | float to half-float              |
|  [12]   | `QuantizeFloat(v, N)`                                                                                                 | quantize       | float to N-bit quantized float   |
|  [13]   | `DequantizeHalf(h)`                                                                                                   | dequantize     | half-float to float              |

[ENTRYPOINT_SCOPE]: meshlet and spatial operations
- rail: geometry

| [INDEX] | [SURFACE]                                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `BuildMeshlets(meshlets, vertices, triangles, indices, index_count, positions, vertex_count, stride, max_vertices, max_triangles, cone_weight)` | meshlet        | build meshlets                 |
|   [2]   | `BuildMeshletsBound(index_count, max_vertices, max_triangles)`                                                                                  | bound          | maximum meshlet count          |
|   [3]   | `BuildMeshletsScan(meshlets, vertices, triangles, indices, index_count, vertex_count, max_vertices, max_triangles)`                             | meshlet        | scan-based meshlet build       |
|   [4]   | `OptimizeMeshlet(meshlet_vertices, triangles, triangle_count, vertex_count)`                                                                    | meshlet        | optimize single meshlet        |
|   [5]   | `ComputeClusterBounds(indices, index_count, positions, vertex_count, stride)`                                                                   | bounds         | cluster cone and sphere bounds |
|   [6]   | `ComputeMeshletBounds(meshlet_vertices, triangles, triangle_count, positions, vertex_count, stride)`                                            | bounds         | meshlet cone/sphere bounds     |
|   [7]   | `SpatialSortRemap(destination, positions, vertex_count, stride)`                                                                                | sort           | spatially sort vertex remap    |
|   [8]   | `SpatialSortTriangles(destination, indices, index_count, positions, vertex_count, stride)`                                                      | sort           | spatially sort triangles       |
|   [9]   | `PartitionClusters(destination, cluster_indices, total_index_count, cluster_index_counts, cluster_count, vertex_count, max_partitions)`         | partition      | cluster partitioning           |
|  [10]   | `AnalyzeVertexCache(indices, index_count, vertex_count, cache_size, warp_size, primgroup_size)`                                                 | analyze        | vertex cache statistics        |
|  [11]   | `AnalyzeVertexFetch(indices, index_count, vertex_count, vertex_size)`                                                                           | analyze        | vertex fetch statistics        |
|  [12]   | `AnalyzeOverdraw(indices, index_count, positions, vertex_count, stride)`                                                                        | analyze        | overdraw statistics            |

## [4]-[IMPLEMENTATION_LAW]

[NATIVE_TOPOLOGY]:
- namespace: `MeshOptimizer`
- all operations live in the `Meshopt` static class as `extern` P/Invoke methods over the native `meshoptimizer` shared library
- native library names: `meshoptimizer` (Windows), `libmeshoptimizer` (macOS/Linux)
- `Meshopt.ResolveLibrary` event allows custom `DllImportResolver` registration for embedded native deployment
- all pointer parameters are unsafe `uint*`, `void*`, or `float*` — callers must operate in unsafe context or pin managed arrays

[SIMPLIFICATION_FLAGS]:
- `SimplificationOptions.None` (0): default quadric simplification
- `SimplificationOptions.SimplifyLockBorder` (1): prevents border edge collapse
- `SimplificationOptions.meshopt_SimplifySparse` (2): reduces attribute discontinuities
- `SimplificationOptions.meshopt_SimplifyErrorAbsolute` (4): interprets target error as absolute world-space value

[LOCAL_ADMISSION]:
- All operations are `unsafe extern`; callers must pin managed arrays or use `fixed` blocks before passing pointers.
- Encode bound operations (`EncodeIndexBufferBound`, `EncodeVertexBufferBound`) return `nuint` — allocate at least this many bytes before calling the matching encode.
- Statistics structs (`VertexCacheStatistics`, `VertexFetchStatistics`, `OverdrawStatistics`, `CoverageStatistics`) carry floating-point ratios and are returned by value.

[RAIL_LAW]:
- Package: `Alimer.Bindings.MeshOptimizer`
- Owns: native meshoptimizer bindings for GPU-ready mesh processing
- Accept: vertex and index buffer preparation, meshlet construction, LOD generation, codec for wire transmission
- Reject: mesh topology construction, polygon Boolean operations, and attribute interpolation
