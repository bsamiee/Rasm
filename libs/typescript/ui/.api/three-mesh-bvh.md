# [TS_UI_API_THREE_MESH_BVH]

`three-mesh-bvh` is the spatial-acceleration structure the `ui/viewer/mark` pick pipes and the `viewer/scene` section/measure queries build over `three` `BufferGeometry`: a bounding-volume hierarchy turning brute-force per-triangle raycast, distance, and overlap scans on merged CAD meshes and dense LiDAR clouds into logarithmic tree descents. One `BVH` base owns the traversal spine (`refit`/`traverse`/`shapecast`/`bvhcast`) and a `GeometryBVH` subtree specializes it per topology — `MeshBVH` (triangles), `SkinnedMeshBVH` (deformable), `PointsBVH`/`LineSegmentsBVH`/`LineBVH`/`LineLoopBVH` (point and wire), `ObjectBVH` (whole-scene, composite-id addressed). Querying is one polymorphic `shapecast` descent over a bounds test and a per-primitive callback, never a per-shape method family; `raycast`/`closestPointToPoint`/`intersectsGeometry` are its named specializations. Members below verify against the shipped `src/index.d.ts`, `src/workers/index.d.ts`, and `src/webgpu/index.d.ts`. `scope:viewer` project-local: admitted only by the `ui/viewer` Nx project, compile-time excluded from the `ui` core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `three-mesh-bvh`
- package: `three-mesh-bvh`
- license: `MIT`
- abi: pure-JS over `three`'s WebGL2/WebGPU scene graph; the BVH is a typed-array node buffer (`Float32Array` bounds + `Int32Array`/`Uint32Array` index) transferable across worker boundaries and shareable through `SharedArrayBuffer`
- peer: `three` (`~catalog`, the `BufferGeometry`/`Raycaster`/`Mesh`/`Box3`/`Triangle`/`Object3D` surface every BVH indexes and every query returns `Intersection` against, `.api/three.md`)
- export roots: `.` (core BVH family, extension functions, shader + math utilities), `./worker` (`GenerateMeshBVHWorker`/`ParallelMeshBVHWorker` off-thread builders), `./webgpu` (`BVHComputeData` node-shader compute descriptor)
- ships first-party `.d.ts` — the `types: src/index.d.ts` declaration is member truth, no `@types/*` companion admitted
- catalog-verdict: KEEP — the BVH-accelerated raycast/distance/overlap engine `viewer/mark` pick pipes and `viewer/scene` section/measure name explicitly; no admitted package owns triangle-level spatial acceleration over `three` geometry, and deck's GPU picking resolves layer marks, never mesh-interior triangle hits or closest-surface distance
- runtime: `scope:viewer` project-local; the one process-global surface is the optional `three` prototype patch (see `[03]`) — the collision-free path constructs `MeshBVH` explicitly and queries its own methods, so two viewer apps in one process never contend over a shared prototype
- modules: `BVH`, `GeometryBVH`, `MeshBVH`, `SkinnedMeshBVH`, `PointsBVH`, `LineSegmentsBVH`, `LineBVH`, `LineLoopBVH`, `ObjectBVH`, `SerializedBVH`, `BVHHelper`, `ExtendedTriangle`, `OrientedBox`, `StaticGeometryGenerator`, `MeshBVHUniformStruct`, `FloatVertexAttributeTexture`, `UIntVertexAttributeTexture`, `IntVertexAttributeTexture`, `BVHShaderGLSL`, the `computeBoundsTree`/`disposeBoundsTree`/`acceleratedRaycast` extension trio, the batched `computeBatchedBoundsTree`/`disposeBatchedBoundsTree` pair, and the `getTriangleHitPointInfo`/`estimateMemoryInBytes`/`getBVHExtremes`/`validateBounds`/`getJSONStructure` utilities
- constants: `SplitStrategy` — `CENTER` (fastest build), `AVERAGE` (balanced), `SAH` (surface-area heuristic, tightest tree, slowest build); `ShapecastIntersection` — `NOT_INTERSECTED`, `INTERSECTED`, `CONTAINED` (the `intersectsBounds` return trichotomy driving subtree prune vs descend vs whole-contained fast path)

## [02]-[BVH_FAMILY]

[TYPE_SCOPE]: the acceleration-structure class tree — `BVH` is the topology-agnostic traversal base, `GeometryBVH` binds it to one `BufferGeometry`, and each leaf class specializes the per-primitive `shapecast` callback. Topology is the discriminant (triangle vs point vs line vs object), never a knob; one tree per geometry, queried by descent.
- `BVH` carries `shiftPrimitiveOffsets(offset)`, `refit(nodeIndices?)` (re-fit bounds after vertex mutation without a full rebuild), `traverse(callback, rootIndex?)` (raw node walk exposing `boundingData`/`offsetOrSplit`/`count`), `getBoundingBox(target)`, `shapecast(callbacks)`, and `bvhcast(otherBVH, matrixToLocal, callbacks)` (dual-tree descent for tree-vs-tree overlap).
- `GeometryBVH extends BVH` adds `readonly geometry`, `readonly indirect` (the indirect-buffer build mode leaving the source index untouched), and `raycastObject3D(object, raycaster, intersects)`.

Every constructor takes `(source, options?: BVHOptions)`; `[ROOT]` names the source. `ObjectBVH` adds `getObjectFromId`/`getInstanceFromId` and `raycast(raycaster, intersects?)`.

| [INDEX] | [SYMBOL]                | [ROOT]           | [CONSUMER_BOUNDARY]                                       |
| :-----: | :---------------------- | :--------------- | :-------------------------------------------------------- |
|  [01]   | `MeshBVH`               | `BufferGeometry` | triangle raycast/distance/overlap — `mark` pick workhorse |
|  [02]   | `SkinnedMeshBVH`        | `SkinnedMesh`    | deformable skinned-mesh triangle queries                  |
|  [03]   | `PointsBVH`             | `BufferGeometry` | point-cloud `shapecast` (`intersectsPoint`)               |
|  [04]   | `LineSegmentsBVH`       | `BufferGeometry` | wire `shapecast` (`intersectsLine`)                       |
|  [05]   | `LineBVH`/`LineLoopBVH` | `BufferGeometry` | polyline / closed-loop line topologies                    |
|  [06]   | `ObjectBVH`             | `Object3D[]`     | whole-scene tree, composite-id addressed                  |

[MESH_BVH_QUERIES]: `MeshBVH` specializes the descent into the named queries `mark`/`scene` consume — every one is a `shapecast` under the hood, exposed as a typed method so callers never author the traversal by hand.
- `raycast(ray, materialOrSide?, near?, far?): Intersection[]` and `raycastFirst(ray, materialOrSide?, near?, far?): Intersection | null` — accelerated ray queries returning all hits or the nearest; `raycastFirst` is the pick-pipe hot path.
- `intersectsSphere(sphere)`, `intersectsBox(box, boxToMesh)`, `intersectsGeometry(geometry, geometryToBvh)` — boolean overlap tests for brush/lasso selection and clash detection.
- `closestPointToPoint(point, target?, minThreshold?, maxThreshold?): HitPointInfo | null` and `closestPointToGeometry(geometry, geometryToBvh, target1?, target2?, minThreshold?, maxThreshold?): HitPointInfo | null` — nearest-surface distance for snapping and measure, `HitPointInfo` carrying `point`/`distance`/`faceIndex`.
- `shapecast(callbacks)` — descends generally with an `intersectsTriangle(triangle: ExtendedTriangle, triangleIndex, contained, depth)` per-primitive callback; `bvhcast(otherBVH, matrixToLocal, callbacks)` accepts either `intersectsRanges` or `intersectsTriangles` for tree-vs-tree scans.
- `resolveTriangleIndex(i)` maps an `indirect`-build hit back to the source triangle; `shiftTriangleOffsets(offset)` re-bases indices after a merged-geometry graft.

## [03]-[EXTENSION_AND_OPTIONS]

[EXTENSION_TRIO]: these functions patch host-wide `three` prototypes, so every `three` consumer in the process observes the mutation. Multi-owner hosts construct and query `MeshBVH` explicitly. A patched host requires one patch owner or a ref-counted lease that snapshots each prototype member, installs once, and restores only after the last lease closes.
- `computeBoundsTree(options?: ComputeBVHOptions): GeometryBVH` — bound as `BufferGeometry.prototype.computeBoundsTree`; builds the tree and parks it on `geometry.boundsTree`. `ComputeBVHOptions` adds `type?: typeof GeometryBVH` selecting the leaf class.
- `disposeBoundsTree(): void` — bound as `BufferGeometry.prototype.disposeBoundsTree`; frees the tree and clears `geometry.boundsTree`.
- `acceleratedRaycast(raycaster, intersects): void` — assigned to `Mesh.prototype.raycast`; when `geometry.boundsTree` is present it descends the BVH, honoring `raycaster.firstHitOnly` to stop at the nearest hit.
- batched twins: `computeBatchedBoundsTree(index?, options?): GeometryBVH | GeometryBVH[]` and `disposeBatchedBoundsTree(index?)` bind onto `BatchedMesh.prototype`, parking per-instance trees on `batchedMesh.boundsTrees`.
- augments (the `declare module 'three'` interface merges): `BufferGeometry.boundsTree`/`computeBoundsTree`/`disposeBoundsTree`, `BatchedMesh.boundsTrees`/`computeBoundsTree`/`disposeBoundsTree`, `Raycaster.firstHitOnly`.

[BVH_OPTIONS]: the single build-tuning bag threaded through every constructor and `computeBoundsTree` — split policy, depth, and buffer sharing are rows on one options object, never a builder-method family.
- `strategy?: SplitStrategy` — `CENTER`/`AVERAGE`/`SAH`, trading build time against query tightness.
- `maxDepth?`, `targetLeafSize?` — depth cap and target primitives-per-leaf.
- `setBoundingBox?` — also set `geometry.boundingBox` during build.
- `useSharedArrayBuffer?` — allocate node buffers in a `SharedArrayBuffer` so a worker-built tree transfers to the render thread zero-copy.
- `indirect?` — build against an indirect index buffer, leaving the source geometry index untouched (`resolveTriangleIndex` maps hits back).
- `verbose?`, `onProgress?(progress)`, `range?: { start; count }` — logging, build-progress callback, and a sub-range build over part of the index.

[SHAPECAST_CALLBACKS]: the descent contract — `intersectsBounds` decides prune/descend/contained per node, the optional `boundsTraverseOrder` scores near-first traversal, and the leaf callback tests primitives.
- `intersectsBounds(box, isLeaf, score, depth, nodeIndex): ShapecastIntersection | boolean` — return `NOT_INTERSECTED` to prune, `INTERSECTED` to descend, `CONTAINED` to accept the whole subtree without further bounds tests.
- `boundsTraverseOrder?(box): number` — orders sibling descent, driving nearest-first raycast early-out.
- `intersectsRange?(offset, count, contained, depth, nodeIndex, box): boolean` — batch-accept a leaf's primitive range without per-primitive dispatch.
- `MeshBVH`/`SkinnedMeshBVH.intersectsTriangle`: `(triangle: ExtendedTriangle, triangleIndex: number, contained: boolean, depth: number) => boolean | void`.
- `PointsBVH.intersectsPoint`: `(pointIndex: number, contained: boolean, depth: number) => boolean | void`.
- `LineSegmentsBVH`/`LineBVH`/`LineLoopBVH.intersectsLine`: `(lineIndex: number, contained: boolean, depth: number) => boolean | void`.
- `ObjectBVH.intersectsObject`: `(object: Object3D, instanceId: number, contained: boolean, depth: number) => boolean | void`.

## [04]-[WORKER_GPU_SERIALIZE]

[OFF_THREAD_BUILD]: `three-mesh-bvh/worker` — building a tree over a dense cloud or a merged CAD assembly blocks the main thread for hundreds of ms; the worker builders move the build off-thread and hand back a `SharedArrayBuffer`-backed `MeshBVH`.
- `GenerateMeshBVHWorker` — `readonly running`, `generate(geometry, options?: BVHOptions): Promise<MeshBVH>`, `dispose()`; one worker, one build in flight.
- `ParallelMeshBVHWorker extends GenerateMeshBVHWorker` — adds `maxWorkerCount`, fanning the build across a worker pool for the largest geometries.

[GPU_SHADER]: the surfaces that push BVH traversal into a fragment/compute shader — GPU picking, ray-marched SDF, and per-pixel closest-surface over the same tree the CPU queries.
- `MeshBVHUniformStruct` — `updateFrom(bvh: MeshBVH)`, `dispose()`; packs the tree into GPU textures.
- `FloatVertexAttributeTexture`/`UIntVertexAttributeTexture`/`IntVertexAttributeTexture` — `DataTexture` subclasses with `updateFrom(attribute)` uploading a `BufferAttribute` as a sampleable texture.
- `BVHShaderGLSL` — `{ bvh_distance_functions, bvh_ray_functions, bvh_struct_definitions, common_functions }` GLSL chunk strings injected into a custom `ShaderMaterial`.
- `three-mesh-bvh/webgpu` `BVHComputeData` — the WebGPU/TSL node-shader compute descriptor over an `ObjectBVH`: `storage`/`structs`/`fns` (`raycastFirstHit`/`sampleTrianglePoint`/`closestPointToPoint`), `update()`, and `getShapecastFn(options)` composing a custom compute pass.

[SERIALIZE]: `SerializedBVH` (`roots: ArrayBuffer[]`, `index`, `indirectBuffer`) is the transferable tree snapshot — `MeshBVH.serialize(bvh, { cloneBuffers? })` and `MeshBVH.deserialize(data, geometry, { setIndex? })` move a built tree across the worker boundary or cache it beside the geometry so a re-open skips the rebuild.

## [05]-[MATH_DEBUG_MERGE]

[MATH_PRIMITIVES]: the exact-intersection value objects the leaf callbacks and clash queries return — extended `three` math types carrying the tests the base library omits.
- `ExtendedTriangle extends Triangle` — `needsUpdate`, `intersectsTriangle(other, target?: Line3)` (with the shared intersection segment), `intersectsSphere(sphere)`, `closestPointToSegment(segment, target1?, target2?)`, `distanceToPoint(point)`, `distanceToTriangle(tri)`.
- `OrientedBox` — `set(min, max, matrix)`, `intersectsBox(box)`, `intersectsTriangle(tri)`, `closestPointToPoint(point, target?)`, `distanceToPoint(point)`, `distanceToBox(box, threshold?, target1?, target2?)`; the arbitrarily-rotated box a section plane or brush volume tests against.
- `getTriangleHitPointInfo(point, geometry, triangleIndex, target?): HitTriangleInfo` — resolves a hit into `face` (vertex indices, `materialIndex`, `normal`), interpolated `uv`, and `barycoord` for surface-attribute sampling at the hit.

[MERGE_AND_DEBUG]: the geometry-consolidation and tree-inspection surfaces `scene` section/measure and BVH validation consume.
- `StaticGeometryGenerator(objects: Object3D | Object3D[])` — `meshes`, `useGroups`, `attributes`, `applyWorldTransforms`, `getMaterials()`, `generate(target?): BufferGeometry`; bakes a whole `Object3D` subtree into one world-space `BufferGeometry` — the merged mesh a single `MeshBVH` then accelerates for section/measure over an entire assembly.
- `BVHHelper extends Group` — `opacity`/`depth`/`displayParents`/`displayEdges`/`edgeMaterial`/`meshMaterial`/`color`, `update()`, `dispose()`; the debug scene node visualizing tree bounds.
- inspection: `estimateMemoryInBytes(bvh)`, `getBVHExtremes(bvh): ExtremeInfo[]` (`nodeCount`/`leafNodeCount`/`surfaceAreaScore`/`depth`/`primitives`/`splits`), `validateBounds(bvh: MeshBVH): boolean`, `getJSONStructure(bvh): TreeNode`.

## [06]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Stack under `three` `BufferGeometry` (`.api/three.md`): the BVH indexes a `three` geometry and returns `three` `Intersection`/`Box3`/`Triangle` values — `viewer/scene` builds one `MeshBVH` per residency-loaded GLB at graft, parks it on `geometry.boundsTree`, and `refit`s rather than rebuilds when a vertex buffer mutates.
- Stack under `viewer/mark` pick pipes: `raycastFirst` is the pointer-pick hot path returning the nearest triangle `Intersection`; the same tree serves brush/lasso selection through `intersectsBox`/`intersectsSphere` and snapping through `closestPointToPoint` — one tree built once at graft, shared across pick, section, and measure, the `[POINT_CLOUD]` IDEAS card's law.
- Stack with `StaticGeometryGenerator` for section/measure over merged meshes: `generate` bakes a multi-mesh assembly into one world-space `BufferGeometry`, and a single `MeshBVH` over it answers section-plane overlap (`shapecast` + `OrientedBox`) and point-to-surface measure (`closestPointToPoint`) across the whole assembly without per-mesh iteration.
- Stack with `PointsBVH` for dense clouds: the `@loaders.gl/las`-decoded `PointCloudLayer` scans acquire a `PointsBVH` for accelerated point pick and radius query, closing the pick gap on clouds too dense for per-point scans.
- Stack with the worker builders: `ParallelMeshBVHWorker.generate` with `useSharedArrayBuffer` builds the tree off the render thread and transfers it zero-copy, so a large-assembly graft never stalls the frame loop.

[LOCAL_ADMISSION]:
- imported only inside the `ui/viewer` Nx project (`scope:viewer`); the `ui` core never resolves it — the BVH engine and its worker/GPU deps stay compile-time excluded from non-spatial apps.
- `three` prototype patching is host-wide. Multi-owner hosts construct `MeshBVH` explicitly; patched hosts admit one owner or a ref-counted patch lease that restores the captured prototype members after its final release.
- pick the leaf class by geometry topology (`MeshBVH`/`PointsBVH`/`LineSegmentsBVH`/`ObjectBVH`) and query through the one polymorphic `shapecast` descent — never fork a per-shape query method the library already specializes.

[RAIL_LAW]:
- Package: `three-mesh-bvh`
- Owns: the BVH acceleration family over `three` geometry — `MeshBVH`/`SkinnedMeshBVH`/`PointsBVH`/`LineSegmentsBVH`/`LineBVH`/`LineLoopBVH`/`ObjectBVH` on the `BVH`→`GeometryBVH` spine, the `shapecast`/`bvhcast`/`refit`/`traverse` traversal core, the `raycast`/`raycastFirst`/`intersects*`/`closestPointTo*` `MeshBVH` queries, the `computeBoundsTree`/`disposeBoundsTree`/`acceleratedRaycast` prototype extension trio and batched twins, the `BVHOptions` build bag (`strategy`/`targetLeafSize`/`useSharedArrayBuffer`/`indirect`/`range`), the `GenerateMeshBVHWorker`/`ParallelMeshBVHWorker` off-thread builders, the `MeshBVHUniformStruct`/`BVHShaderGLSL`/`*VertexAttributeTexture` GPU-shader surface and the `webgpu` `BVHComputeData`, the `ExtendedTriangle`/`OrientedBox`/`StaticGeometryGenerator` math and merge primitives, `SerializedBVH` serialization, and the debug/inspection utilities
- Accept: one tree per residency-loaded geometry built at graft and `refit` on vertex mutation, `raycastFirst` as the pick hot path, merged-assembly and point-cloud trees, worker-built `SharedArrayBuffer` trees, explicit construction or one ref-counted host patch owner
- Reject: brute-force per-triangle raycast or distance scans where a tree serves, a module-load global prototype patch two apps contend over, a per-shape query method family instead of the polymorphic `shapecast`, rebuilding a tree where `refit` re-fits mutated bounds, blocking the render thread on a large-geometry build the worker builders move off-thread
