# [TS_DATA_API_MESHOPTIMIZER]

`meshoptimizer` is the wasm mesh-processing kernel: five independent module objects — encoder, decoder, simplifier, clusterizer, tangents — each a frozen record of functions over `TypedArray` views, each gated on its own `ready` promise. It owns vertex/index codec encode and decode, cache-aware reordering, error-bounded decimation, meshlet clustering, and tangent generation.

Each JS module base64-inlines its own wasm, so NO sidecar binary ships: one file carries the whole capability, and the module picks a SIMD or baseline build at load through `WebAssembly.validate`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `meshoptimizer`
- package: `meshoptimizer` (MIT)
- module: subpath `exports` — `.` re-exports all five, `./encoder`, `./simplifier`, `./clusterizer`, `./tangents` resolve `.js`, `./decoder` resolves `meshopt_decoder.mjs`, and `./decoder.cjs` is the CommonJS decoder
- runtime: both lanes — pure wasm with no native binding, no fs, and no fetch; the same module serves the browser viewer and the server pipeline
- native: none — every module inlines its wasm as base64, so no `.wasm` file ships and no loader path is configured
- rail: geometry codec beneath the `object` container plane; the decoder also serves the browser viewer as a static asset
- boundary: buffers in, buffers out; the package holds no glTF vocabulary and no container knowledge

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the five module records and their shared gate

| [INDEX] | [SYMBOL]             | [SUBPATH]       | [CAPABILITY]                                      |
| :-----: | :------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `MeshoptEncoder`     | `./encoder`     | vertex/index encode, reorder, attribute filters   |
|  [02]   | `MeshoptDecoder`     | `./decoder`     | vertex/index decode, optional worker pool         |
|  [03]   | `MeshoptSimplifier`  | `./simplifier`  | error-bounded decimation and point simplification |
|  [04]   | `MeshoptClusterizer` | `./clusterizer` | meshlet build and cluster/sphere bounds           |
|  [05]   | `MeshoptTangents`    | `./tangents`    | tangent generation from position, normal, and UV  |

Each record carries `supported: boolean` and `ready: Promise<void>`; every member throws before `ready` resolves.

[PUBLIC_TYPE_SCOPE]: bounded vocabularies and clusterizer value types

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :---------------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `SimplifierFlags` | flag set      | `LockBorder` `Sparse` `ErrorAbsolute` `Prune` `Regularize` `Permissive` `RegularizeLight` |
|  [02]   | `TangentsFlags`   | flag set      | `Compatible` `ZeroFallback`                                                               |
|  [03]   | `ExpMode`         | filter mode   | `Separate` `SharedVector` `SharedComponent` `Clamped`                                     |
|  [04]   | `MeshletBuffers`  | cluster batch | `meshlets` `vertices` `triangles` `meshletCount`                                          |
|  [05]   | `Meshlet`         | cluster slice | one extracted `{ vertices, triangles }`                                                   |
|  [06]   | `Bounds`          | culling bound | sphere center and radius plus the normal-cone apex, axis, and cutoff                      |

`Flags` is a deprecated alias of `SimplifierFlags`; the flag arrays are optional trailing parameters, never an options record.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `MeshoptEncoder` — codec and ordering

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------ | :------------------------------------- |
|  [01]   | `encodeVertexBuffer(source, count, size) -> Uint8Array`             | vertex stream codec                    |
|  [02]   | `encodeVertexBufferLevel(source, count, size, level, version?)`     | the same with an explicit effort level |
|  [03]   | `encodeIndexBuffer(source, count, size) -> Uint8Array`              | triangle index codec                   |
|  [04]   | `encodeIndexSequence(source, count, size) -> Uint8Array`            | non-triangle index codec               |
|  [05]   | `encodeGltfBuffer(source, count, size, mode, version?)`             | the glTF buffer-view form              |
|  [06]   | `reorderMesh(indices, triangles, optsize) -> [Uint32Array, number]` | vertex-cache/overdraw order + remap    |
|  [07]   | `reorderPoints(positions, positions_stride) -> Uint32Array`         | spatial order for point clouds         |

`[ATTRIBUTE_FILTER]: `encodeFilterOct(source, count, stride, bits)` `encodeFilterQuat(source, count, stride, bits)` `encodeFilterExp(source, count, stride, bits, mode?)` `encodeFilterColor(source, count, stride, bits)`` — octahedral normals, quaternion rotations, shared-exponent floats under an `ExpMode`, and quantized color.

[ENTRYPOINT_SCOPE]: `MeshoptDecoder` — the consumer half

| [INDEX] | [SURFACE]                                                                          | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `decodeVertexBuffer(target, count, size, source, filter?)`                         | decode into a caller-owned target |
|  [02]   | `decodeIndexBuffer(target, count, size, source)`                                   | triangle index decode             |
|  [03]   | `decodeIndexSequence(target, count, size, source)`                                 | non-triangle index decode         |
|  [04]   | `decodeGltfBuffer(target, count, size, source, mode, filter?)`                     | the glTF buffer-view form         |
|  [05]   | `decodeGltfBufferAsync(count, size, source, mode, filter?) -> Promise<Uint8Array>` | the worker-pool form              |
|  [06]   | `useWorkers(count)`                                                                | size the decode worker pool       |

Every synchronous decoder writes into a target the CALLER allocates and returns `void`; only the async form allocates and answers its own buffer.

[ENTRYPOINT_SCOPE]: `MeshoptSimplifier` — decimation

Every decimating member takes `(indices, positions, stride, …, target_index_count, target_error)` and answers `[Uint32Array, number]` — the surviving indices and the achieved error.

| [INDEX] | [SURFACE]                | [EXTRA_PARAMETERS]                                   | [CAPABILITY]                      |
| :-----: | :----------------------- | :--------------------------------------------------- | :-------------------------------- |
|  [01]   | `simplify`               | `flags?`                                             | decimate under an error bound     |
|  [02]   | `simplifyWithAttributes` | `attributes` `attr_stride` `weights` `lock` `flags?` | attribute-aware decimation        |
|  [03]   | `simplifyWithUpdate`     | the `simplifyWithAttributes` set                     | in-place form; answers two counts |
|  [04]   | `simplifySloppy`         | `lock`                                               | topology-ignoring fast path       |
|  [05]   | `simplifyPoints`         | `colors?` `colors_stride?` `color_weight?`           | point-cloud decimation            |
|  [06]   | `simplifyPrune`          | none                                                 | drop components under the error   |
|  [07]   | `getScale`               | `(positions, stride) -> number`                      | the scale `target_error` is in    |
|  [08]   | `compactMesh`            | `(indices)`                                          | drop unreferenced vertices        |
|  [09]   | `generatePositionRemap`  | `(positions, stride) -> Uint32Array`                 | weld-equivalent position remap    |

`simplifyPoints` answers a `Uint32Array` of kept indices and `simplifyPrune` a `Uint32Array` of surviving indices; both stand outside the pair-returning shape.

`target_error` is RELATIVE to `getScale(positions, stride)` unless `ErrorAbsolute` is in the flag array, and the returned second element is the achieved error in the same scale.

[ENTRYPOINT_SCOPE]: `MeshoptClusterizer` and `MeshoptTangents`

Every clusterizer builder takes `(indices, positions, stride, max_vertices, …)` and answers `MeshletBuffers`.

| [INDEX] | [SURFACE]              | [EXTRA_PARAMETERS]                                                        | [CAPABILITY]                 |
| :-----: | :--------------------- | :------------------------------------------------------------------------ | :--------------------------- |
|  [01]   | `buildMeshlets`        | `max_triangles` `cone_weight?`                                            | fixed-bound meshlet build    |
|  [02]   | `buildMeshletsFlex`    | `min_triangles` `max_triangles` `cone_weight?` `split_factor?`            | variable-size meshlets       |
|  [03]   | `buildMeshletsSpatial` | `min_triangles` `max_triangles` `fill_weight?`                            | spatially coherent meshlets  |
|  [04]   | `extractMeshlet`       | `(buffers, index) -> Meshlet`                                             | one meshlet out of the batch |
|  [05]   | `computeMeshletBounds` | `(buffers, positions, stride) -> Bounds[]`                                | per-meshlet sphere and cone  |
|  [06]   | `computeClusterBounds` | `(indices, positions, stride) -> Bounds`                                  | one cluster's bound          |
|  [07]   | `computeSphereBounds`  | `(positions, stride, radii?, radii_stride?) -> Bounds`                    | bounding sphere over points  |
|  [08]   | `generateTangents`     | `(indices, positions, stride, normals, n_stride, uvs, uv_stride, flags?)` | tangent basis                |

`generateTangents` is `MeshoptTangents`' only member and answers a `Float32Array`; every other row above belongs to `MeshoptClusterizer`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Five independent modules, five `ready` gates, one import each — a consumer that only decodes imports `meshoptimizer/decoder` and never loads the encoder, simplifier, clusterizer, or tangents wasm.
- Buffers in, buffers out over `TypedArray` views, and no glTF vocabulary lives here — `mode` and `filter` are plain strings the container layer supplies, and the codec never reads a document.
- Inlined wasm and load-time SIMD selection through `WebAssembly.validate` leave no decoder path to configure, no `.wasm` sibling to serve, and no fetch on the module's critical path.
- Synchronous decode writes into a caller-owned target; the worker-pool form is the ONE allocating entry, so buffer ownership is explicit at every call.

[STACKING]:
- `@gltf-transform/extensions`(`.api/gltf-transform-extensions.md`): `EXTMeshoptCompression` declares `readDependencies: ["meshopt.decoder"]` and `writeDependencies: ["meshopt.encoder"]`, so `MeshoptDecoder` and `MeshoptEncoder` install through `PlatformIO.registerDependencies` and the extension calls `decodeGltfBuffer`/`encodeGltfBuffer` itself.
- `@gltf-transform/functions`(`.api/gltf-transform-functions.md`): `meshopt({ encoder })` and `reorder({ encoder })` take `MeshoptEncoder`; `simplify({ simplifier })` takes `MeshoptSimplifier`. Each row takes the SAME instance registered on the IO, so encode and extension write share one wasm instance.
- `object/store.md`: encoded buffers are ordinary bytes admitted under the one `ContentKey` conditional put; the codec is deterministic for a given input and version, so a re-encode of unchanged geometry is idempotent by digest.
- `object/file.md` `[03]-[CODEC_GATE]`: `supported` is the same boot-time capability read the sharp gate performs — a false value refuses the row rather than failing mid-fold.

[LOCAL_ADMISSION]:
- `await ready` is proven at LAYER construction, never inside a fold; a member called before it resolves throws, and a pipeline must not fail halfway through a document mutation.
- `MeshoptEncoder` stays on the server lane and `MeshoptDecoder` is the browser half — an encoder shipped to a browser bundle loads wasm the viewer never runs.
- `useWorkers(count)` is stated per host from its concurrency budget, never left at a default, because each worker holds its own wasm instance.
- `getScale` is read before any `target_error`, so decimation is stated in the mesh's own scale; an absolute error passed as relative silently decimates by orders of magnitude.
- `encodeVertexBufferLevel` and `encodeGltfBuffer` take a codec `version` — it is stated explicitly wherever the emitted bytes are content-addressed, because a default that moves with the package re-keys every previously encoded buffer.
- `meshopt_decoder_reference.js` ships in the package and is absent from `exports`; the decode path is the exported subpath alone.

[RAIL_LAW]:
- Package: `meshoptimizer`
- Owns: the wasm mesh kernel — vertex and index encode/decode with attribute filters, vertex-cache and spatial reordering, error-bounded and attribute-aware simplification, point decimation, meshlet clustering with sphere and normal-cone bounds, and tangent generation
- Accept: per-module subpath imports, `ready` proven at layer construction, `supported` read as a capability gate, caller-owned decode targets, explicit `useWorkers` sizing, `getScale`-relative error bounds, an explicit codec `version` on content-addressed output, one shared instance across the IO dependency and the transform row
- Reject: a member called before `ready`, an encoder in a browser bundle, a decoder path or `.wasm` sidecar configured for it, a default codec version on addressed bytes, the unexported reference decoder, glTF semantics read inside the codec
