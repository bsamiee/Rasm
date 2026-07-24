# [TS_UI_API_THREE_MESH_BVH]

`three-mesh-bvh` accelerates raycast, distance, and overlap queries over `three` `BufferGeometry`: a bounding-volume hierarchy collapsing brute-force per-triangle scans on merged CAD meshes and dense point clouds into logarithmic tree descents. One `shapecast` descent — a bounds test and a per-primitive callback — owns every query, and `raycast`/`closestPointToPoint`/`intersectsGeometry` are its named specializations, never a per-shape method family.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `three-mesh-bvh`
- package: `three-mesh-bvh` (MIT)
- module: ESM subpath exports — `.` (BVH family, prototype extensions, GLSL + math utilities), `./worker` (off-thread builders), `./webgpu` (`BVHComputeData` compute descriptor); ships first-party `src/index.d.ts` as member truth, no `@types/*` companion
- runtime: browser render thread, peer `three` (`.api/three.md`); worker builders run on a `Worker`, and `useSharedArrayBuffer` node buffers cross to the render thread zero-copy
- abi: typed-array node buffer — `Float32Array` bounds with `Int32Array`/`Uint32Array` index — `Worker`-transferable, `SharedArrayBuffer`-shareable
- rail: BVH acceleration over `three` geometry — `viewer/mark` pick pipes and `viewer/scene` section/measure compose; `scope:viewer` project-local

## [02]-[BVH_FAMILY]

[TYPE_SCOPE]: the acceleration-structure class tree — `BVH` is the topology-agnostic traversal base, `GeometryBVH` binds it to one `BufferGeometry`, and each leaf specializes the per-primitive `shapecast` callback. Topology discriminates the leaf class; one tree per geometry, queried by descent, and every constructor takes `(source, options?: BVHOptions)`.

| [INDEX] | [SYMBOL]                | [ROOT]           | [CONSUMER]                                      |
| :-----: | :---------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `MeshBVH`               | `BufferGeometry` | triangle raycast/distance/overlap — `mark` pick |
|  [02]   | `SkinnedMeshBVH`        | `SkinnedMesh`    | deformable skinned-mesh triangle queries        |
|  [03]   | `PointsBVH`             | `BufferGeometry` | point-cloud `shapecast` (`intersectsPoint`)     |
|  [04]   | `LineSegmentsBVH`       | `BufferGeometry` | wire `shapecast` (`intersectsLine`)             |
|  [05]   | `LineBVH`/`LineLoopBVH` | `BufferGeometry` | polyline / closed-loop line topologies          |
|  [06]   | `ObjectBVH`             | `Object3D[]`     | whole-scene tree, composite-id addressed        |

[ENTRY_SCOPE]: the traversal spine every subclass inherits — `BVH` walks nodes, `GeometryBVH` binds geometry, `ObjectBVH` resolves composite ids.

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `BVH.shapecast(ShapecastCallbacks)`                                | instance | general descent — bounds test + leaf callback   |
|  [02]   | `BVH.bvhcast(MeshBVH, Matrix4, BVHCastCallbacks)`                  | instance | dual-tree descent for tree-vs-tree overlap      |
|  [03]   | `BVH.refit(Array\|Set?)`                                           | instance | re-fit bounds after vertex mutation, no rebuild |
|  [04]   | `BVH.traverse(callback, number?)`                                  | instance | raw node walk — `boundingData`/`offsetOrSplit`  |
|  [05]   | `BVH.getBoundingBox(Box3) -> Box3`                                 | instance | root bounds into target                         |
|  [06]   | `BVH.shiftPrimitiveOffsets(number)`                                | instance | re-base primitive offsets after a graft         |
|  [07]   | `GeometryBVH.raycastObject3D(Object3D, Raycaster, Intersection[])` | instance | raycast one `Object3D` into the hit array       |
|  [08]   | `GeometryBVH.geometry` / `.indirect`                               | property | source geometry; indirect-buffer build mode     |
|  [09]   | `ObjectBVH.getObjectFromId(number) -> Object3D`                    | instance | composite id resolves to its object             |
|  [10]   | `ObjectBVH.getInstanceFromId(number) -> number`                    | instance | composite id resolves to its instance           |
|  [11]   | `ObjectBVH.raycast(Raycaster, Intersection[]?) -> Intersection[]`  | instance | whole-scene ray query                           |

[MESH_BVH_QUERIES]: `MeshBVH` specializes the descent into the named queries `mark`/`scene` consume; each wraps a `shapecast` as a typed method, `raycastFirst`/`closestPointTo*` returning `null` on a miss and `HitPointInfo` carrying `point`/`distance`/`faceIndex`. Both `closestPointTo*` take optional `minThreshold`/`maxThreshold` bounds.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `MeshBVH.raycast(Ray, Side\|Material[]?, number?, number?) -> Intersection[]` | instance | all ray hits                          |
|  [02]   | `MeshBVH.raycastFirst(Ray, Side\|Material[]?, number?, number?)`              | instance | nearest hit — pick-pipe hot path      |
|  [03]   | `MeshBVH.intersectsSphere(Sphere) -> boolean`                                 | instance | sphere overlap                        |
|  [04]   | `MeshBVH.intersectsBox(Box3, Matrix4) -> boolean`                             | instance | box overlap — brush/lasso selection   |
|  [05]   | `MeshBVH.intersectsGeometry(BufferGeometry, Matrix4) -> boolean`              | instance | geometry clash detection              |
|  [06]   | `MeshBVH.closestPointToPoint(Vector3, HitPointInfo?)`                         | instance | nearest surface — snapping, measure   |
|  [07]   | `MeshBVH.closestPointToGeometry(BufferGeometry, Matrix4)`                     | instance | nearest surface, geometry-to-geometry |
|  [08]   | `MeshBVH.resolveTriangleIndex(number) -> number`                              | property | indirect-build hit → source triangle  |
|  [09]   | `MeshBVH.shiftTriangleOffsets(number)`                                        | instance | re-base indices after a merged graft  |

## [03]-[EXTENSION_AND_OPTIONS]

[ENTRY_SCOPE]: the prototype-extension functions patch host-wide `three` prototypes — every `three` consumer in the process observes the mutation, so a multi-owner host constructs and queries `MeshBVH` explicitly or leases the patch (see `[06]`).

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `computeBoundsTree(ComputeBVHOptions?) -> GeometryBVH` | function | `BufferGeometry.prototype` — build, park `boundsTree` |
|  [02]   | `disposeBoundsTree()`                                  | function | `BufferGeometry.prototype` — free `boundsTree`        |
|  [03]   | `acceleratedRaycast(Raycaster, Intersection[])`        | function | `Mesh.prototype.raycast` — honor `firstHitOnly`       |
|  [04]   | `computeBatchedBoundsTree(number?, BVHOptions?)`       | function | `BatchedMesh.prototype` — per-instance `boundsTrees`  |
|  [05]   | `disposeBatchedBoundsTree(number?)`                    | function | `BatchedMesh.prototype` — free per-instance trees     |

- `computeBatchedBoundsTree` returns one `GeometryBVH` for a given index, or the full `GeometryBVH[]` when called without one.
- `ComputeBVHOptions` extends `BVHOptions` with `type?: typeof GeometryBVH`, selecting the leaf class the extension builds.
- `declare module 'three'` merges add `BufferGeometry.boundsTree`/`computeBoundsTree`/`disposeBoundsTree`, `BatchedMesh.boundsTrees`/`computeBoundsTree`/`disposeBoundsTree`, and `Raycaster.firstHitOnly`.

[BVH_OPTIONS]: the single build-tuning bag threaded through every constructor and `computeBoundsTree` — split policy, depth, and buffer sharing are rows on one object, never a builder-method family.

| [INDEX] | [FIELD]                | [TYPE]           | [CAPABILITY]                                    |
| :-----: | :--------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `strategy`             | `SplitStrategy`  | split policy                                    |
|  [02]   | `maxDepth`             | `number`         | tree depth cap                                  |
|  [03]   | `targetLeafSize`       | `number`         | target primitives per leaf                      |
|  [04]   | `setBoundingBox`       | `boolean`        | also set `geometry.boundingBox` during build    |
|  [05]   | `useSharedArrayBuffer` | `boolean`        | `SharedArrayBuffer` node buffers, zero-copy     |
|  [06]   | `indirect`             | `boolean`        | indirect index build, source geometry untouched |
|  [07]   | `range`                | `{start,count}`  | sub-range build over part of the index          |
|  [08]   | `verbose`              | `boolean`        | build logging                                   |
|  [09]   | `onProgress`           | `(number)=>void` | build-progress callback                         |

- `SplitStrategy`: `CENTER` fastest build / `AVERAGE` balanced / `SAH` surface-area heuristic, tightest tree, slowest build.

[SHAPECAST_CALLBACKS]: the descent contract — `intersectsBounds` decides prune/descend/contain per node, `boundsTraverseOrder` scores near-first traversal, and one leaf callback per topology tests primitives, each returning `boolean`/`void`.

| [INDEX] | [CALLBACK]                                                       | [OWNER]                    | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------- | :------------------------- | :------------------------------------ |
|  [01]   | `intersectsBounds(Box3, boolean, number?, number, number)`       | all                        | per-node prune/descend/contain        |
|  [02]   | `boundsTraverseOrder(Box3) -> number`                            | all                        | sibling descent order — nearest-first |
|  [03]   | `intersectsRange(number, number, boolean, number, number, Box3)` | all                        | batch-accept a leaf's primitive range |
|  [04]   | `intersectsTriangle(ExtendedTriangle, number, boolean, number)`  | `MeshBVH`/`SkinnedMeshBVH` | per-triangle leaf test                |
|  [05]   | `intersectsPoint(number, boolean, number)`                       | `PointsBVH`                | per-point leaf test                   |
|  [06]   | `intersectsLine(number, boolean, number)`                        | `Line*BVH`                 | per-line leaf test                    |
|  [07]   | `intersectsObject(Object3D, number, boolean, number)`            | `ObjectBVH`                | per-object leaf test                  |

- `ShapecastIntersection`: `NOT_INTERSECTED` prunes the subtree / `INTERSECTED` descends / `CONTAINED` accepts the whole subtree without further bounds tests.

## [04]-[WORKER_GPU_SERIALIZE]

[OFF_THREAD_BUILD]: `three-mesh-bvh/worker` moves the build over a dense cloud or merged CAD assembly off the main thread, handing back a `SharedArrayBuffer`-backed `MeshBVH`.

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `GenerateMeshBVHWorker.generate(BufferGeometry, BVHOptions?) -> Promise<MeshBVH>` | instance | one worker, one build in flight     |
|  [02]   | `GenerateMeshBVHWorker.running` / `.dispose()`                                    | property | in-flight flag; teardown            |
|  [03]   | `ParallelMeshBVHWorker.maxWorkerCount`                                            | property | fans the build across a worker pool |

[GPU_SHADER]: the surfaces pushing BVH traversal into a fragment or compute shader — GPU picking, ray-marched SDF, and per-pixel closest-surface over the same tree the CPU queries.

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `MeshBVHUniformStruct.updateFrom(MeshBVH)` / `.dispose()` | instance | packs the tree into GPU textures                      |
|  [02]   | `FloatVertexAttributeTexture` / `UInt…` / `Int…`          | class    | `DataTexture` — `updateFrom(BufferAttribute)`         |
|  [03]   | `BVHShaderGLSL`                                           | const    | GLSL chunk strings for a `ShaderMaterial`             |
|  [04]   | `BVHComputeData` (`./webgpu`)                             | class    | TSL compute over `ObjectBVH` — `getShapecastFn(opts)` |

- `BVHShaderGLSL`: `{bvh_distance_functions, bvh_ray_functions, bvh_struct_definitions, common_functions}` — the GLSL chunks injected into a custom `ShaderMaterial`.
- `BVHComputeData`: `storage`/`structs`/`fns` (`raycastFirstHit`/`sampleTrianglePoint`/`closestPointToPoint`) and `update()` — the compute-pass buffers, structs, and entry points `getShapecastFn` composes.

[SERIALIZE]: `SerializedBVH` (`roots: ArrayBuffer[]`, `index`, `indirectBuffer`) is the transferable tree snapshot moving a built tree across the worker boundary or caching it beside the geometry so a re-open skips the rebuild.

| [INDEX] | [SURFACE]                                                                    | [SHAPE] | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `MeshBVH.serialize(MeshBVH, {cloneBuffers?}) -> SerializedBVH`               | static  | snapshot a built tree                |
|  [02]   | `MeshBVH.deserialize(SerializedBVH, BufferGeometry, {setIndex?}) -> MeshBVH` | static  | rebuild-free re-open from a snapshot |

## [05]-[MATH_DEBUG_MERGE]

[MATH_PRIMITIVES]: the exact-intersection value objects leaf callbacks and clash queries return — extended `three` math types carrying the tests the base library omits, `OrientedBox` the arbitrarily-rotated box a section plane or brush volume tests against.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `ExtendedTriangle.intersectsTriangle(Triangle, Line3?) -> boolean`            | instance | overlap; shared segment into `Line3` |
|  [02]   | `ExtendedTriangle.intersectsSphere(Sphere) -> boolean`                        | instance | triangle-sphere overlap              |
|  [03]   | `ExtendedTriangle.closestPointToSegment(Line3, Vector3?, Vector3?) -> number` | instance | nearest point to a segment           |
|  [04]   | `ExtendedTriangle.distanceToPoint(Vector3)` / `.distanceToTriangle(Triangle)` | instance | point / triangle distance            |
|  [05]   | `OrientedBox.set(Vector3, Vector3, Matrix4) -> OrientedBox`                   | instance | rebuild the rotated box              |
|  [06]   | `OrientedBox.intersectsBox(Box3)` / `.intersectsTriangle(Triangle)`           | instance | box / triangle overlap               |
|  [07]   | `OrientedBox.closestPointToPoint(Vector3, Vector3?) -> number`                | instance | nearest point                        |
|  [08]   | `OrientedBox.distanceToBox(Box3, number?, Vector3?, Vector3?) -> number`      | instance | box distance with threshold          |
|  [09]   | `getTriangleHitPointInfo(Vector3, BufferGeometry, number, HitTriangleInfo?)`  | function | hit → `face`/`uv`/`barycoord`        |

[MERGE_AND_DEBUG]: the geometry-consolidation and tree-inspection surfaces `scene` section/measure and BVH validation consume.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `StaticGeometryGenerator(Object3D\|Object3D[])`                       | ctor     | bakes a subtree into one world-space geometry |
|  [02]   | `StaticGeometryGenerator.generate(BufferGeometry?) -> BufferGeometry` | instance | the merged mesh one `MeshBVH` accelerates     |
|  [03]   | `BVHHelper` (`extends Group`)                                         | class    | debug scene node — `update()`/`dispose()`     |
|  [04]   | `estimateMemoryInBytes(BVH) -> number`                                | function | tree byte estimate                            |
|  [05]   | `getBVHExtremes(BVH) -> ExtremeInfo[]`                                | function | per-tree quality stats                        |
|  [06]   | `validateBounds(MeshBVH) -> boolean`                                  | function | bounds-integrity check                        |
|  [07]   | `getJSONStructure(BVH) -> TreeNode`                                   | function | tree as a nested `TreeNode`                   |

- `StaticGeometryGenerator` config: `meshes`, `useGroups`, `attributes`, `applyWorldTransforms`, `getMaterials()` — the merge inputs and resolved material list.
- `BVHHelper` fields: `opacity`, `depth`, `color`, `displayParents`, `displayEdges`, `edgeMaterial`, `meshMaterial`.
- `getBVHExtremes` → `ExtremeInfo[]`: `nodeCount`, `leafNodeCount`, `surfaceAreaScore`, `depth`, `primitives`, `splits`.

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One tree per residency-loaded geometry, built once at graft and parked on `geometry.boundsTree`; a vertex-buffer mutation calls `refit`, never a rebuild.
- `shapecast` owns every query — `raycast`/`raycastFirst`/`intersects*`/`closestPointTo*` are typed specializations of the one descent, and the leaf class (`MeshBVH`/`PointsBVH`/`LineSegmentsBVH`/`ObjectBVH`) is picked by geometry topology, never a per-shape query method.

[STACKING]:
- `three` (`.api/three.md`): the BVH indexes a `three` `BufferGeometry` and returns `three` `Intersection`/`Box3`/`Triangle` values — `viewer/scene` builds one `MeshBVH` per residency-loaded GLB at graft, parks it on `geometry.boundsTree`, and `refit`s on vertex mutation; `acceleratedRaycast` on `Mesh.prototype.raycast` routes `Raycaster.firstHitOnly` picks through the tree.
- `viewer/mark` pick pipes: `raycastFirst` is the pointer-pick hot path returning the nearest triangle `Intersection`; the same tree serves brush/lasso through `intersectsBox`/`intersectsSphere` and snapping through `closestPointToPoint` — one tree built once at graft, shared across pick, section, and measure (the `[POINT_CLOUD]` IDEAS card law).
- `StaticGeometryGenerator` + `viewer/scene` section/measure: `generate` bakes a multi-mesh assembly into one world-space `BufferGeometry`, and a single `MeshBVH` answers section-plane overlap (`shapecast` + `OrientedBox`) and point-to-surface measure (`closestPointToPoint`) across the whole assembly without per-mesh iteration.
- `PointsBVH` + dense clouds: a `@loaders.gl/las`-decoded `PointCloudLayer` scan acquires a `PointsBVH` for accelerated point pick and radius query where per-point scans are too dense.
- `ParallelMeshBVHWorker` + `useSharedArrayBuffer`: builds the tree off the render thread and transfers it zero-copy, so a large-assembly graft never stalls the frame loop.

[LOCAL_ADMISSION]:
- imported only inside the `ui/viewer` Nx project (`scope:viewer`); the `ui` core never resolves the BVH engine or its worker/GPU deps.
- `three` prototype patching is host-wide: a multi-owner host constructs `MeshBVH` explicitly, or admits one patch owner or a ref-counted lease that snapshots each patched prototype member and restores it after the final release.
- pick the leaf class by geometry topology and query through the one `shapecast` descent — never fork a per-shape query method the library already specializes.

[RAIL_LAW]:
- Package: `three-mesh-bvh`
- Owns: BVH spatial acceleration over `three` geometry — the `BVH`→`GeometryBVH` class tree and its leaves, the `shapecast`/`bvhcast`/`refit`/`traverse` core, the `MeshBVH` raycast/distance/overlap queries, the `computeBoundsTree`/`acceleratedRaycast` prototype extensions and batched twins, the `BVHOptions` build bag, the worker builders, the GPU-shader surface (`MeshBVHUniformStruct`/`BVHShaderGLSL`/`BVHComputeData`), the `ExtendedTriangle`/`OrientedBox`/`StaticGeometryGenerator` math and merge primitives, `SerializedBVH` serialization, and the debug/inspection utilities
- Accept: one tree per residency-loaded geometry, `refit` on mutation, `raycastFirst` as the pick hot path, merged-assembly and point-cloud trees, worker-built `SharedArrayBuffer` trees, explicit construction or one ref-counted host patch owner
- Reject: brute-force per-triangle raycast or distance scans where a tree serves, a module-load global prototype patch two apps contend over, a per-shape query method family instead of `shapecast`, rebuilding where `refit` re-fits mutated bounds, blocking the render thread on a large-geometry build the worker builders move off-thread
