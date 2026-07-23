# [PY_GEOMETRY_API_PYTHON_FCL]

`python-fcl` owns the geometry branch's narrow-phase collision, signed-distance, and continuous-collision engine: a `CollisionObject` pairs a primitive or `BVHModel` mesh with a rigid `Transform`, `collide`/`distance`/`continuousCollide` resolve one request-response query per call, and `DynamicAABBTreeCollisionManager` folds group queries onto a broadphase AABB tree. Its `mesh/spatial` CORE-clearance arm composes signed `distance` with the group manager for exact mesh-mesh separation, falling through to the `manifold3d` `min_gap` scalar when the `fcl` import is unavailable.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-fcl`
- package: `python-fcl` (BSD-3-Clause)
- import: `import fcl`
- owner: `geometry`
- rail: mesh/spatial / clearance-enrichment
- capability: narrow-phase collision detection, minimum-distance computation with nearest points and signed distance, continuous-collision time-of-contact; primitive and triangular-mesh geometries; rigid `Transform` poses; broadphase AABB-tree collision/distance managers for one-to-many and many-to-many group queries

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry and object family

`CollisionGeometry` is the shape base every primitive and the `BVHModel`/`Convex` wrapper derives from; `CollisionObject` binds a geometry to a `Transform` pose and is the query unit, carrying `get`/`set` for `Transform`/`Translation`/`Rotation`/`QuatRotation` and `isOccupied`/`isFree`/`isUncertain`. `BVHModel` builds through `beginModel`/`addSubModel`/`addVertex`/`addTriangle`/`endModel`.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `CollisionObject`                   | posed object  | geometry + `Transform` pose (accessors in lead)            |
|  [02]   | `Transform`                         | rigid pose    | rotation (3x3 or 4-quaternion) + 3-vec translation         |
|  [03]   | `Box` / `Sphere` / `Ellipsoid`      | primitive     | origin-centered analytic solids (side/radius/height)       |
|  [04]   | `Capsule` / `Cone` / `Cylinder`     | primitive     | origin-centered analytic solids (side/radius/height)       |
|  [05]   | `TriangleP` / `Halfspace` / `Plane` | primitive     | point triangle, half-space `<n,x> < d`, plane `<n,x> = d`  |
|  [06]   | `Convex`                            | mesh (convex) | vertex/face polygon hull for GJK-fast narrow phase         |
|  [07]   | `BVHModel`                          | mesh (BVH)    | triangle-soup bounding-volume hierarchy (builders in lead) |
|  [08]   | `OcTree`                            | mesh (octree) | octomap binary-stream occupancy geometry                   |

[PUBLIC_TYPE_SCOPE]: request, result, and broadphase family

Each query kind is one request-and-result pair; the manager wraps that pair in a `CollisionData`/`DistanceData` carrier with a `done` flag for the recursive broadphase walk.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]    | [CAPABILITY]                                                              |
| :-----: | :-------------------------------- | :--------------- | :------------------------------------------------------------------------ |
|  [01]   | `CollisionRequest`                | collision query  | `num_max_contacts`, `enable_contact`, `enable_cost`, `gjk_solver_type`    |
|  [02]   | `CollisionResult`                 | collision query  | `is_collision`, `contacts`, `cost_sources`                                |
|  [03]   | `DistanceRequest`                 | distance query   | `enable_nearest_points`, `enable_signed_distance`, `gjk_solver_type`      |
|  [04]   | `DistanceResult`                  | distance query   | `min_distance`, `nearest_points`, `o1`/`o2`/`b1`/`b2`                     |
|  [05]   | `ContinuousCollisionRequest`      | CCD query        | `num_max_iterations`, `toc_err`, `ccd_motion_type`, `ccd_solver_type`     |
|  [06]   | `ContinuousCollisionResult`       | CCD query        | `is_collide`, `time_of_contact`                                           |
|  [07]   | `Contact` / `CostSource`          | collision detail | `o1`/`o2`/`b1`/`b2`/`normal`/`pos`/`penetration_depth`; AABB cost density |
|  [08]   | `DynamicAABBTreeCollisionManager` | broadphase       | broadphase AABB tree over the managed `CollisionObject` set               |
|  [09]   | `CollisionData` / `DistanceData`  | manager carrier  | request-response pair + `done` flag for the recursive callback            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pairwise queries

Each pairwise query populates a request, allocates an empty result, calls the free function over two `CollisionObject` items, and reads the scalar return with the mutated result; a `None` request or result defaults to the base shape.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `collide(o1, o2, request=None, result=None)`                   | collision      | contact count; `is_collision`/`contacts`              |
|  [02]   | `distance(o1, o2, request=None, result=None)`                  | distance       | `min_distance` (neg on penetration); `nearest_points` |
|  [03]   | `continuousCollide(o1, tf1_end, o2, tf2_end, request, result)` | CCD            | time-of-contact in `(0, 1)`; `time_of_contact`        |

[ENTRYPOINT_SCOPE]: broadphase group queries

`collide`/`distance` are polymorphic on argument shape — a callback pair for internal many-to-many, a `CollisionObject` and callback for one-to-many, or a second manager and callback for group many-to-many; every object registers with a manager before a group query.

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------ | :------------- | :------------------------------------------------------------ |
|  [01]   | `manager.registerObjects(objs)` / `registerObject(obj)` | register       | admit objects into the broadphase tree                        |
|  [02]   | `manager.setup()` / `update(arg=None)`                  | build          | build or refresh the AABB tree after pose changes             |
|  [03]   | `manager.collide(cdata, callback)`                      | internal n²    | pairwise collision across all managed objects                 |
|  [04]   | `manager.collide(obj, cdata, callback)`                 | one-to-many    | collision between one object and the managed set              |
|  [05]   | `manager.collide(other_manager, cdata, callback)`       | group          | collision between two managed sets                            |
|  [06]   | `manager.distance(ddata, callback)`                     | internal n²    | closest distance across the managed set                       |
|  [07]   | `defaultCollisionCallback` / `defaultDistanceCallback`  | callback       | stock recursion-terminating callbacks over the `Data` carrier |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `import fcl` resolves function-local at boundary scope under `# ruff:ignore[import-outside-top-level]`; an unavailable import is a probed capability the fall-through handles, never an unconditional one.
- Each query kind is one request-and-result shape — `collide`/`distance`/`continuousCollide` discriminate by function, never a query-per-geometry family; the CORE-clearance arm calls `distance` with `DistanceRequest(enable_nearest_points=True, enable_signed_distance=True)` and reads `min_distance` (negative under penetration) with `nearest_points` as signed clearance evidence.
- A triangulated element lifts through `BVHModel.beginModel`/`addSubModel`/`endModel`, analytic primitives are the fast path for a known solid, and `Convex` is the GJK-direct hull path; `CollisionObject.setTransform`/`setTranslation`/`setQuatRotation` re-pose in place and recompute the AABB.
- Group clearance registers every `CollisionObject` into a `DynamicAABBTreeCollisionManager`, calls `setup`, then `distance(DistanceData(), defaultDistanceCallback)` over the set, `update` refreshing the tree after pose mutation; the clearance receipt keys the minimum separation, the `nearest_points` pair, the `b1`/`b2` handles, and the penetrating-pair count.

[STACKING]:
- `manifold3d`(`.api/manifold3d.md`): signed `distance` `min_distance`/`nearest_points` is the exact narrow-phase separation; an unavailable `fcl` import falls the arm through to `Manifold.min_gap(other, search_length)`, the unsigned minimum-gap scalar without signed penetration or a nearest-point pair.
- `rtree`(`.api/rtree.md`): the `intersection`/`intersection_v` box pre-filter narrows candidate element pairs over a static set, and the surviving pairs enter fcl narrow-phase `distance` or the `DynamicAABBTreeCollisionManager` group walk.
- `trimesh`(`.api/trimesh.md`): a `Trimesh` `vertices`/`faces` pair feeds `BVHModel.addSubModel` to build the mesh geometry; nearest-surface proximity and ray stay on trimesh.

[LOCAL_ADMISSION]:
- `python-fcl` is the admitted narrow-phase collision and signed-distance backend for the geometry branch; the `mesh/spatial` clearance owner composes it rather than a parallel GJK/EPA or BVH-traversal surface.

[RAIL_LAW]:
- Package: `python-fcl`
- Owns: narrow-phase collision detection, minimum-distance computation with nearest points and signed distance, continuous-collision time-of-contact, primitive and BVH/Convex/OcTree geometries, rigid poses, and broadphase AABB-tree collision/distance managers
- Accept: the `mesh/spatial` CORE-clearance arm's exact signed separation and group broadphase clearance
- Reject: wrapper-renames of `collide`/`distance`; a hand-rolled GJK/EPA or BVH traversal where fcl is admitted; a query-per-geometry function family over the request-result pipeline; treating an unavailable `fcl` import as a hard failure rather than the `manifold3d` fall-through
