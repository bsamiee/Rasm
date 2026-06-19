# [RASM_COMPUTE_API_ALIMER_MESHOPTIMIZER]

`Alimer.Bindings.MeshOptimizer` exposes the native `meshoptimizer` library through P/Invoke
bindings in `MeshOptimizer.Meshopt`, supplying vertex remap, cache and overdraw optimization,
index and vertex buffer codec, meshlet building, spatial sorting, LOD simplification, and
quantization for mesh interchange and GPU-ready geometry preparation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Alimer.Bindings.MeshOptimizer`
- package: `Alimer.Bindings.MeshOptimizer`
- assembly: `Alimer.Bindings.MeshOptimizer`
- namespace: `MeshOptimizer`
- asset: managed bindings + native `meshoptimizer` runtime (platform-specific shared library)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: entry-point and options family
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                |
| :-----: | :---------------------- | :-------------- | :------------------------------------ |
|  [01]   | `Meshopt`               | static bindings | all meshoptimizer P/Invoke operations |
|  [02]   | `SimplificationOptions` | flags enum      | LOD simplification control flags      |
|  [03]   | `EncodeExpMode`         | enum            | vertex buffer exponent encode mode    |

[PUBLIC_TYPE_SCOPE]: data carrier family
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                          |
| :-----: | :---------------------- | :------------- | :------------------------------ |
|  [01]   | `Stream`                | vertex stream  | multi-stream vertex remap input |
|  [02]   | `Meshlet`               | meshlet struct | meshlet index/vertex counts     |
|  [03]   | `Bounds`                | bounds struct  | cluster and meshlet bounds      |
|  [04]   | `VertexCacheStatistics` | stat struct    | vertex cache hit statistics     |
|  [05]   | `VertexFetchStatistics` | stat struct    | vertex fetch efficiency stats   |
|  [06]   | `OverdrawStatistics`    | stat struct    | overdraw ratio statistics       |
|  [07]   | `CoverageStatistics`    | stat struct    | coverage efficiency statistics  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: remap and index generation operations
- rail: geometry

| [INDEX] | [SURFACE]                                                                                                   | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `GenerateVertexRemap(destination, indices, index_count, vertices, vertex_count, vertex_size)`               | remap          | build vertex remap table     |
|  [02]   | `GenerateVertexRemapMulti(destination, indices, index_count, vertex_count, streams, stream_count)`          | remap          | multi-stream remap table     |
|  [03]   | `RemapVertexBuffer(destination, vertices, vertex_count, vertex_size, remap)`                                | apply          | apply remap to vertex buffer |
|  [04]   | `RemapIndexBuffer(destination, indices, index_count, remap)`                                                | apply          | apply remap to index buffer  |
|  [05]   | `GenerateShadowIndexBuffer(destination, indices, index_count, vertices, vertex_count, vertex_size, stride)` | shadow         | shadow index generation      |
|  [06]   | `GenerateAdjacencyIndexBuffer(destination, indices, index_count, positions, vertex_count, stride)`          | topology       | adjacency index generation   |
|  [07]   | `GenerateProvokingIndexBuffer(destination, reorder, indices, index_count, vertex_count)`                    | provoking      | provoking index generation   |

[ENTRYPOINT_SCOPE]: cache and overdraw optimization
- rail: geometry

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- | :-------------------------------- |
|  [01]   | `OptimizeVertexCache(destination, indices, index_count, vertex_count)`                            | cache optimize | tipsify cache optimization        |
|  [02]   | `OptimizeVertexCacheStrip(destination, indices, index_count, vertex_count)`                       | cache optimize | strip-friendly cache optimization |
|  [03]   | `OptimizeVertexCacheFifo(destination, indices, index_count, vertex_count, cache_size)`            | cache optimize | FIFO cache optimization           |
|  [04]   | `OptimizeOverdraw(destination, indices, index_count, positions, vertex_count, stride, threshold)` | overdraw       | overdraw minimization             |
|  [05]   | `OptimizeVertexFetch(destination, indices, index_count, vertices, vertex_count, vertex_size)`     | fetch optimize | vertex fetch locality             |
|  [06]   | `OptimizeVertexFetchRemap(destination, indices, index_count, vertex_count)`                       | fetch remap    | vertex fetch remap generation     |

[ENTRYPOINT_SCOPE]: codec operations
- rail: geometry

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `EncodeIndexBuffer(buffer, buffer_size, indices, index_count)`                             | encode         | compress index buffer              |
|  [02]   | `EncodeIndexBufferBound(index_count, vertex_count)`                                        | bound          | maximum encoded index buffer size  |
|  [03]   | `DecodeIndexBuffer(destination, index_count, index_size, buffer, buffer_size)`             | decode         | decompress index buffer            |
|  [04]   | `EncodeIndexSequence(buffer, buffer_size, indices, index_count)`                           | encode         | compress index sequence            |
|  [05]   | `EncodeIndexSequenceBound(index_count, vertex_count)`                                      | bound          | maximum encoded sequence size      |
|  [06]   | `EncodeVertexBuffer(buffer, buffer_size, vertices, vertex_count, vertex_size)`             | encode         | compress vertex buffer             |
|  [07]   | `EncodeVertexBufferBound(vertex_count, vertex_size)`                                       | bound          | maximum encoded vertex buffer size |
|  [08]   | `EncodeVertexBufferLevel(buffer, buffer_size, vertices, vertex_count, vertex_size, level)` | encode         | level-compressed vertex buffer     |
|  [09]   | `DecodeVertexBuffer(destination, vertex_count, vertex_size, buffer, buffer_size)`          | decode         | decompress vertex buffer           |

[ENTRYPOINT_SCOPE]: filter, simplification, and quantization operations
- rail: geometry

| [INDEX] | [SURFACE]                                                                                                             | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `DecodeFilterOct(buffer, count, stride)`                                                                              | filter decode  | decode octahedral normal filter  |
|  [02]   | `DecodeFilterQuat(buffer, count, stride)`                                                                             | filter decode  | decode quaternion filter         |
|  [03]   | `DecodeFilterExp(buffer, count, stride)`                                                                              | filter decode  | decode exponent filter           |
|  [04]   | `EncodeFilterOct(destination, count, stride, bits, data)`                                                             | filter encode  | encode octahedral normals        |
|  [05]   | `EncodeFilterQuat(destination, count, stride, bits, data)`                                                            | filter encode  | encode quaternions               |
|  [06]   | `EncodeFilterExp(destination, count, stride, bits, data, mode)`                                                       | filter encode  | encode floats with exponent mode |
|  [07]   | `Simplify(destination, indices, index_count, positions, vertex_count, stride, target, error, options, out error_out)` | simplify       | LOD simplification               |
|  [08]   | `SimplifySloppy(destination, indices, index_count, positions, vertex_count, stride, target, error, out error_out)`    | simplify       | aggressive LOD simplification    |
|  [09]   | `SimplifyPoints(destination, positions, vertex_count, stride, colors, colors_stride, colors_weight, target)`          | simplify       | point cloud simplification       |
|  [10]   | `SimplifyScale(positions, vertex_count, stride)`                                                                      | scale          | mesh unit scale for error metric |
|  [11]   | `QuantizeHalf(v)`                                                                                                     | quantize       | float to half-float              |
|  [12]   | `QuantizeFloat(v, N)`                                                                                                 | quantize       | float to N-bit quantized float   |
|  [13]   | `DequantizeHalf(h)`                                                                                                   | dequantize     | half-float to float              |

[ENTRYPOINT_SCOPE]: meshlet and spatial operations
- rail: geometry

| [INDEX] | [SURFACE]                                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `BuildMeshlets(meshlets, vertices, triangles, indices, index_count, positions, vertex_count, stride, max_vertices, max_triangles, cone_weight)` | meshlet        | build meshlets                 |
|  [02]   | `BuildMeshletsBound(index_count, max_vertices, max_triangles)`                                                                                  | bound          | maximum meshlet count          |
|  [03]   | `BuildMeshletsScan(meshlets, vertices, triangles, indices, index_count, vertex_count, max_vertices, max_triangles)`                             | meshlet        | scan-based meshlet build       |
|  [04]   | `OptimizeMeshlet(meshlet_vertices, triangles, triangle_count, vertex_count)`                                                                    | meshlet        | optimize single meshlet        |
|  [05]   | `ComputeClusterBounds(indices, index_count, positions, vertex_count, stride)`                                                                   | bounds         | cluster cone and sphere bounds |
|  [06]   | `ComputeMeshletBounds(meshlet_vertices, triangles, triangle_count, positions, vertex_count, stride)`                                            | bounds         | meshlet cone/sphere bounds     |
|  [07]   | `SpatialSortRemap(destination, positions, vertex_count, stride)`                                                                                | sort           | spatially sort vertex remap    |
|  [08]   | `SpatialSortTriangles(destination, indices, index_count, positions, vertex_count, stride)`                                                      | sort           | spatially sort triangles       |
|  [09]   | `PartitionClusters(destination, cluster_indices, total_index_count, cluster_index_counts, cluster_count, vertex_count, max_partitions)`         | partition      | cluster partitioning           |
|  [10]   | `AnalyzeVertexCache(indices, index_count, vertex_count, cache_size, warp_size, primgroup_size)`                                                 | analyze        | vertex cache statistics        |
|  [11]   | `AnalyzeVertexFetch(indices, index_count, vertex_count, vertex_size)`                                                                           | analyze        | vertex fetch statistics        |
|  [12]   | `AnalyzeOverdraw(indices, index_count, positions, vertex_count, stride)`                                                                        | analyze        | overdraw statistics            |

## [04]-[IMPLEMENTATION_LAW]

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
