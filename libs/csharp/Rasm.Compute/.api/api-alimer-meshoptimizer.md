# [RASM_COMPUTE_API_ALIMER_MESHOPTIMIZER]

`Meshopt` binds Arseny Kapoulkine's native `meshoptimizer` through one source-generated P/Invoke surface, owning GPU-ready mesh optimization over raw index and vertex pointers. It constructs no mesh topology and interpolates no attributes — geometry arrives welded and leaves cache-optimized, simplified, meshletized, and wire-encoded — and feeds the Compute geometry rail and the `EXT_meshopt_compression` glTF interchange path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Alimer.Bindings.MeshOptimizer`
- package: `Alimer.Bindings.MeshOptimizer` (MIT)
- assembly: `Alimer.Bindings.MeshOptimizer`
- namespace: `MeshOptimizer`
- asset: managed binding over native `meshoptimizer`; `runtimes/<rid>/native/` ships one native per RID, the `osx-arm64` consumer binding the fat `libmeshoptimizer.dylib` (arm64+x64)
- abi: `Meshopt.MESHOPTIMIZER_VERSION = 1000u`; `Meshopt.LibraryName = "meshoptimizer"`
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: binding, options, resolver event, and the by-value data-carrier structs

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :----------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `Meshopt`                | static class  | every meshoptimizer P/Invoke operation          |
|  [02]   | `Meshopt.ResolveLibrary` | event         | `DllImportResolver?` for embedded native load   |
|  [03]   | `SimplificationOptions`  | enum          | `[Flags]` LOD simplification control            |
|  [04]   | `EncodeExpMode`          | enum          | exponent-filter sharing mode                    |
|  [05]   | `Stream`                 | struct        | remap source `(void* data, nuint size, stride)` |
|  [06]   | `Meshlet`                | struct        | `vertex_offset`/`triangle_offset` + counts      |
|  [07]   | `Bounds`                 | struct        | `center[3]`, `radius`, cone apex/axis/cutoff+s8 |
|  [08]   | `VertexCacheStatistics`  | struct        | ACMR / ATVR cache ratios                        |
|  [09]   | `VertexFetchStatistics`  | struct        | overfetch ratio + bytes fetched                 |
|  [10]   | `OverdrawStatistics`     | struct        | pixels-shaded / pixels-covered ratio            |
|  [11]   | `CoverageStatistics`     | struct        | per-axis coverage from `AnalyzeCoverage`        |

## [03]-[ENTRYPOINTS]

Pointer parameters carry `uint*` (indices, remap), `void*` (interleaved vertices), `float*` (positions, attributes), or `byte*` (triangle, codec, and lock buffers); `nuint` carries every size and count in bytes. An `nuint` return is a written element count, an `int` return a status code (`0` = ok).

[ENTRYPOINT_SCOPE]: remap, indexing, and topology generation
- call: index ops take the head `(dst, indices, index_count)`—`…` in a row—then a vertex source: `void* vertices`, `float* positions`, or `Stream* streams`.

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

| [INDEX] | [SURFACE]                                                                                      | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `void OptimizeVertexCache(dst, indices, index_count, vertex_count)`                            | tipsify cache optimization          |
|  [02]   | `void OptimizeVertexCacheStrip(dst, indices, index_count, vertex_count)`                       | strip-friendly cache optimization   |
|  [03]   | `void OptimizeVertexCacheFifo(dst, indices, index_count, vertex_count, cache_size)`            | FIFO cache optimization             |
|  [04]   | `void OptimizeOverdraw(dst, indices, index_count, positions, vertex_count, stride, threshold)` | overdraw minimization (post-cache)  |
|  [05]   | `nuint OptimizeVertexFetch(dst, indices, index_count, vertices, vertex_count, vertex_size)`    | reorder vertices for fetch locality |
|  [06]   | `nuint OptimizeVertexFetchRemap(dst, indices, index_count, vertex_count)`                      | fetch-remap table (apply via remap) |
|  [07]   | `void OptimizeMeshlet(meshlet_vertices, meshlet_triangles, triangle_count, vertex_count)`      | per-meshlet local cache optimize    |
|  [08]   | `nuint Stripify(dst, indices, index_count, vertex_count, restart_index)`                       | triangle strip with restart index   |
|  [09]   | `nuint StripifyBound(index_count)`                                                             | max stripified index count          |
|  [10]   | `nuint Unstripify(dst, indices, index_count, restart_index)`                                   | strip → triangle list expansion     |
|  [11]   | `nuint UnstripifyBound(index_count)`                                                           | max unstripified index count        |

[ENTRYPOINT_SCOPE]: index/vertex/sequence codec with format-version pinning
- call: encode ops write into `(byte* buffer, nuint buffer_size)`—`…`—returning the byte count; decode ops read that pair into `dst` returning an `int` status; `*Bound` ops size the destination.

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

`EncodeExpMode` selects exponent sharing: `EncodeExpSeparate` (`0`, per-value, aliased `None`), `EncodeExpSharedVector` (`1`), `EncodeExpSharedComponent` (`2`, per-column), `EncodeExpClamped` (`3`).

[ENTRYPOINT_SCOPE]: LOD simplification and quantization
- call: `Simplify*` ops take the head `(dst, indices, index_count, positions, vertex_count, stride)`—`…`—return the written index count, write `result_error`, and gate on `(target_index_count, target_error, options)`; `vertex_lock` is an optional `byte*` pin mask (`1` = locked) shared across the attribute-aware simplifiers.

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

`SimplificationOptions` `[Flags]`: `SimplifyLockBorder` (`1`, freeze open edges), `meshopt_SimplifySparse` (`2`, sparse attribute-discontinuity reduction), `meshopt_SimplifyErrorAbsolute` (`4`, `target_error` in world units via `SimplifyScale`); `None` is `0`.

[ENTRYPOINT_SCOPE]: meshlet, cluster, spatial, and analysis
- call: meshlet builds fill `(meshlets, meshlet_vertices, meshlet_triangles)` from the source `(indices, index_count, positions, vertex_count, stride)`—`…`—sized by `max_vertices`/`max_triangles`; `Analyze*` take `(indices, index_count)` and return their stat struct by value; the shown tail is the per-op discriminator.

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

[TOPOLOGY]:
- One `static class Meshopt`; each method carries both `[DllImport]` and `[LibraryImport]` source-generated marshalling, so the native name resolves per-OS and a `Meshopt.ResolveLibrary` handler registered before the first call routes embedded or self-contained loads.
- `Stream` `(void* data, nuint size, nuint stride)` points `data` at the first attribute of a de-interleaved stream, `size` the per-vertex byte span the remap hashes, `stride` the advance; multi-stream remap builds one table covering every stream.
- Codec versioning is process-global: `EncodeIndexVersion`/`EncodeVertexVersion` set the format the next encode emits, so encode and decode versions pair across the wire and `Decode*Version` probes an unknown blob before its decode target is allocated.
- A `delegate* unmanaged<...>` parameter (`GenerateVertexRemapCustom`, `SetAllocator`) binds a `[UnmanagedCallersOnly]` static address; a closure cannot cross the boundary.
- Canonical GPU-ready order, each stage feeding the next:
    1. Dedup: `GenerateVertexRemap{,Multi,Custom}` → `RemapVertexBuffer` + `RemapIndexBuffer`.
    2. Locality: `OptimizeVertexCache` → `OptimizeOverdraw` → `OptimizeVertexFetch{,Remap}`.
    3. LOD: `SimplifyWithAttributes` (or `Simplify`/`SimplifySloppy`) per level, `SimplifyScale`-normalized under `meshopt_SimplifyErrorAbsolute`; `SimplifyPrune` drops islands.
    4. Mesh-shader: `BuildMeshlets{,Flex,Spatial}` (sized by `BuildMeshletsBound`) → `OptimizeMeshlet` → `ComputeMeshletBounds`; `PartitionClusters` groups for Nanite-style DAGs.
    5. Wire: `EncodeFilter{Oct,Quat,Exp,Color}` → `EncodeVertexBufferLevel` + `EncodeIndexBuffer`; `Analyze*` returns the ACMR/ATVR/overfetch/overdraw receipts gating the result.

[STACKING]:
- `SharpGLTF.Core`(`.api/api-sharpgltf.md`): SharpGLTF carries no meshopt or Draco encoder, so this surface owns the `EXT_meshopt_compression` encode path — filter then `EncodeVertexBufferLevel`/`EncodeIndexBuffer` over the `MemoryAccessor` byte spans, then attach the extension and bufferView metadata; `SharpGLTF.Toolkit` mesh building feeds raw positions and indices into the `Generate*`→`Optimize*` head.
- `Microsoft.IO.RecyclableMemoryStream`(`.api/api-recyclable-stream.md`): a `*Bound`-sized rented stream whose pinned `GetSpan()` is the `byte*` codec target, so encode and decode scratch takes no per-call LOH churn.
- `System.IO.Hashing`: `XxHash3`/`XxHash128` fingerprints an encoded blob into the `Microsoft.Extensions.Caching.Hybrid` LOD cache key, so identical source meshes reuse encoded output.
- `CommunityToolkit.HighPerformance`: `Span2D<T>`/`MemoryOwner<T>` with `System.Numerics.Tensors` back the managed position, attribute, and remap arrays whose handles pin into these pointers.
- Cluster-LOD residency chain: `LodChain`/`ClusterLevel` builds each coarser level through error-bounded `SimplifyScale`-normalized `Simplify` then re-meshlets it with `BuildMeshlets`, threading `Level`/`Parent`/`Error`/`ParentError` onto `ResidencyMeshlet` rows so the viewer's screen-space-error cut stays monotonic — a parent error never below its child's.

[LOCAL_ADMISSION]:
- Every entry point is `unsafe extern`, so pinning via `fixed`, a pinned `Span<T>`, or `MemoryHandle` holds every managed array across the call — a GC move mid-call corrupts the native read; size each encode and meshlet destination through its `*Bound` op first.
- Statistics structs return by value with `float` ratios and thread into typed Compute receipts.
- `SetAllocator` installs once at startup before any concurrent use.

[RAIL_LAW]:
- Package: `Alimer.Bindings.MeshOptimizer`
- Owns: native meshoptimizer bindings for GPU-ready mesh processing and the `EXT_meshopt_compression` codec.
- Accept: vertex/index buffer preparation, strip, meshlet, and cluster construction, LOD generation, the attribute-filter wire codec, and quantization.
- Reject: hand-rolled simplification, cache/fetch reordering, and meshlet or strip construction; mesh topology construction, Boolean operations, attribute interpolation, and Draco encoding stand outside this binding.
