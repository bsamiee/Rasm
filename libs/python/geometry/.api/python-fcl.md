# [PY_GEOMETRY_API_PYTHON_FCL]

`python-fcl` supplies the Flexible Collision Library (FCL 0.7.0) narrow-phase collision, distance, and continuous-collision engine for the geometry `mesh/spatial` CORE-clearance worker-lane arm: a `CollisionObject` pairing a `CollisionGeometry` primitive or `BVHModel` mesh with a `Transform` pose, the `collide`/`distance`/`continuousCollide` request-response pipeline, and the `DynamicAABBTreeCollisionManager` broadphase avoiding n-squared group queries. The package owner composes `BVHModel.addSubModel`, `CollisionObject`, and `distance(o1, o2, DistanceRequest(enable_nearest_points=True, enable_signed_distance=True), DistanceResult())` into the spatial owner, reading `DistanceResult.min_distance`/`nearest_points` for signed separation; it never re-implements GJK/EPA, the BVH traversal, or the AABB broadphase tree. It is a `python_version<'3.15'` manifest-row GATED ENRICHMENT backend inside the `mesh/spatial` query rail — it owns the exact narrow-phase mesh-mesh separation the `manifold3d` `min_gap` fall-through and the `rtree` box pre-filter cannot deliver; when the `<3.15` wheel band is unmet the spatial owner falls through to `manifold3d`, and the fall-through is realized selection law per the `[V9]` clearance arm, never a spine dependency.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-fcl`
- package: `python-fcl`
- import: `import fcl`
- owner: `geometry`
- rail: mesh/spatial / clearance-enrichment
- license: `BSD-3-Clause` (own) over FCL `BSD-3-Clause`
- entry points: none (library only)
- admission band: `python_version<'3.15'` — cp314 wheel ceiling; the manifest-recorded blocker condition on the `[V9]` clearance arm
- capability: narrow-phase collision detection, minimum-distance computation with nearest points and signed distance, continuous-collision time-of-contact; primitive and triangular-mesh geometries; rigid `Transform` poses; broadphase AABB-tree collision/distance managers for one-to-many and many-to-many group queries

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry and object family
- rail: mesh/spatial

`CollisionGeometry` is the shape base; every primitive and the `BVHModel`/`Convex` mesh wrappers derive from it. `CollisionObject` binds a geometry to a `Transform` pose and is the unit every query consumes.

| [INDEX] | [SYMBOL]                                                                 | [TYPE_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :---------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `CollisionObject`                                                        | posed object   | geometry + `Transform`; `get`/`setTransform`, `get`/`setTranslation`, `get`/`setRotation`, `get`/`setQuatRotation`, `isOccupied`/`isFree`/`isUncertain` |
|  [02]   | `Transform`                                                             | rigid pose     | 3x3-matrix or 4-quaternion rotation plus 3-vector translation; `getRotation`/`getTranslation`/`getQuatRotation` |
|  [03]   | `Box` / `Sphere` / `Ellipsoid` / `Capsule` / `Cone` / `Cylinder`        | primitive      | origin-centered analytic solids sized by side/radius/height        |
|  [04]   | `TriangleP` / `Halfspace` / `Plane`                                     | primitive      | point triangle, half-space `<n,x> < d`, and plane `<n,x> = d`       |
|  [05]   | `Convex`                                                                | mesh (convex)  | vertex/face polygon hull for GJK-fast narrow phase                  |
|  [06]   | `BVHModel`                                                              | mesh (BVH)     | `beginModel`/`addSubModel`/`addVertex`/`addTriangle`/`endModel` triangle-soup bounding-volume hierarchy |
|  [07]   | `OcTree`                                                                | mesh (octree)  | octomap binary-stream occupancy geometry                           |

[PUBLIC_TYPE_SCOPE]: request, result, and broadphase family
- rail: mesh/spatial

Each query kind is one request-plus-result pair; the manager wraps a request-response pair in a `CollisionData`/`DistanceData` carrier with a `done` flag for the recursive broadphase walk.

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY]     | [CAPABILITY]                                                        |
| :-----: | :--------------------------------------------------------- | :---------------- | :----------------------------------------------------------------- |
|  [01]   | `CollisionRequest` / `CollisionResult`                     | collision query   | `num_max_contacts`, `enable_contact`, `enable_cost`, `gjk_solver_type`; result `is_collision`, `contacts`, `cost_sources` |
|  [02]   | `DistanceRequest` / `DistanceResult`                       | distance query    | `enable_nearest_points`, `enable_signed_distance`, `gjk_solver_type`; result `min_distance`, `nearest_points`, `o1`/`o2`/`b1`/`b2` |
|  [03]   | `ContinuousCollisionRequest` / `ContinuousCollisionResult` | CCD query         | `num_max_iterations`, `toc_err`, `ccd_motion_type`, `ccd_solver_type`; result `is_collide`, `time_of_contact` |
|  [04]   | `Contact` / `CostSource`                                   | collision detail  | `o1`/`o2`/`b1`/`b2`/`normal`/`pos`/`penetration_depth`; AABB cost density |
|  [05]   | `DynamicAABBTreeCollisionManager`                          | broadphase        | `registerObjects`/`registerObject`/`unregisterObject`/`setup`/`update`/`collide`/`distance`/`getObjects`/`clear`/`empty`/`size` |
|  [06]   | `CollisionData` / `DistanceData`                           | manager carrier   | request-response pair plus `done` flag for the recursive callback  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pairwise queries
- rail: mesh/spatial

Each pairwise query follows one pipeline: populate a request, allocate an empty result, call the free function with the two `CollisionObject` items, read the scalar return and the mutated result. A `None` request or result defaults to the base shape.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `collide(o1, o2, request=None, result=None)`                    | collision      | returns contact count; result carries `is_collision`/`contacts` |
|  [02]   | `distance(o1, o2, request=None, result=None)`                   | distance       | returns `min_distance` (negative under penetration); result carries `nearest_points` |
|  [03]   | `continuousCollide(o1, tf1_end, o2, tf2_end, request, result)`  | CCD            | returns time-of-contact in `(0, 1)`; result carries `time_of_contact` |

[ENTRYPOINT_SCOPE]: broadphase group queries
- rail: mesh/spatial

`CollisionObject` items register with a manager before group queries; `collide`/`distance` are polymorphic on the argument shape — a callback pair for internal many-to-many, a `CollisionObject` plus callback for one-to-many, or a second manager plus callback for group many-to-many.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `manager.registerObjects(objs)` / `registerObject(obj)`     | register       | admit objects into the broadphase tree                   |
|  [02]   | `manager.setup()` / `update(arg=None)`                      | build          | build or refresh the AABB tree after pose changes        |
|  [03]   | `manager.collide(cdata, callback)`                          | internal n²    | pairwise collision across all managed objects            |
|  [04]   | `manager.collide(obj, cdata, callback)`                     | one-to-many    | collision between one object and the managed set         |
|  [05]   | `manager.collide(other_manager, cdata, callback)`           | group          | collision between two managed sets                       |
|  [06]   | `manager.distance(ddata, callback)`                         | internal n²    | closest distance across the managed set                  |
|  [07]   | `defaultCollisionCallback` / `defaultDistanceCallback`      | callback       | stock recursion-terminating callbacks over the `Data` carrier |

## [04]-[IMPLEMENTATION_LAW]

[MESH_SPATIAL_CLEARANCE]:
- import: `import fcl` function-local at boundary scope under `# noqa: PLC0415`; module-level import is banned by the manifest import policy, and the `<3.15` band makes the import a probed capability, never an unconditional one.
- geometry axis: a triangulated element lifts through `BVHModel.beginModel`/`addSubModel`/`endModel`; analytic primitives (`Box`/`Sphere`/`Cylinder`/...) are the fast narrow-phase path when the element is a known solid; `Convex` is the convex-hull path with GJK-direct separation.
- query axis: each query kind is one request-plus-result shape — `collide`/`distance`/`continuousCollide` discriminate by function, never a query-per-shape family; the CORE-clearance arm calls `distance` with `DistanceRequest(enable_nearest_points=True, enable_signed_distance=True)` and reads `min_distance` (negative under penetration) plus `nearest_points` as the signed clearance evidence.
- broadphase axis: group clearance registers every `CollisionObject` into a `DynamicAABBTreeCollisionManager`, calls `setup`, then `distance(DistanceData(), defaultDistanceCallback)` — the sub-n² path the arm uses over an element set, `update` refreshing the tree after pose mutation.
- pose axis: `CollisionObject.setTransform`/`setTranslation`/`setQuatRotation` re-pose in place and recompute the AABB; the `Transform` rotation admits a 3x3 matrix or a 4-vector quaternion.
- fall-through: the `<3.15` wheel ceiling makes `find_spec("fcl")` the gate; an unmet band routes the clearance arm to `manifold3d` `min_gap`, the realized selection law the `[V9]` clearance arm records, never a raised error.
- evidence: each clearance query captures `min_distance`, the `nearest_points` pair, and the object handles `b1`/`b2`; the clearance receipt keys the minimum separation and the penetrating-pair count the spatial owner folds against the caller ceiling.
- boundary: fcl owns narrow-phase collision, signed distance, and continuous-collision; the broadphase box pre-filter over a static set stays `rtree`; exact mesh boolean and `min_gap` stay `manifold3d`; nearest-surface proximity and ray stay `trimesh`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `python-fcl`
- Owns: narrow-phase collision detection, minimum-distance computation with nearest points and signed distance, continuous-collision time-of-contact, primitive and BVH/Convex/OcTree geometries, rigid poses, and broadphase AABB-tree collision/distance managers
- Accept: the `mesh/spatial` CORE-clearance arm's exact signed separation and group broadphase clearance on the `python_version<'3.15'` band
- Reject: wrapper-renames of `collide`/`distance`; a hand-rolled GJK/EPA or BVH traversal where fcl is admitted; a query-per-geometry function family over the request-result pipeline; treating the `<3.15` band as a hard failure rather than the `manifold3d` fall-through

[CAPTURE_GAP]:
