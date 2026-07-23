# [PY_COMPUTE_API_GMSH]

`gmsh` mints unstructured 1/2/3-D finite-element meshes for the compute `[MESH_GENERATION_ROUTE]`: a built-in `geo` or OpenCASCADE `occ` geometry enters, `addPhysicalGroup` tags boundary and material regions, size fields drive element density, and `generate` meshes the model. Node/element/physical-group extraction and the `.msh` write feed the `meshio` read and `scikit-fem` assembly; this owner drives the process-global kernel through `initialize`/`finalize` and hand-rolls no Delaunay/frontal meshing, CAD boolean geometry, or `.msh` codec the kernel owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `gmsh`
- package: `gmsh`
- module: `gmsh`; module-level functions under namespace objects, never instantiable classes
- owner: `compute`
- rail: mesh generation
- state: one process-global kernel bracketed by `initialize()`/`finalize()`, with `model.add`/`setCurrent` switching named models in a live session
- alias: every function carries a snake_case alias beside its camelCase canonical (`add_physical_group` == `addPhysicalGroup`); this catalog documents the camelCase form

## [02]-[NAMESPACE_SURFACE]

[NAMESPACE_SCOPE]: kernel namespaces

| [INDEX] | [NAMESPACE]              | [FAMILY]         | [CAPABILITY]                                                             |
| :-----: | :----------------------- | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `gmsh`                   | session          | `initialize`/`finalize`/`isInitialized`/`clear`/`open`/`merge`/`write`   |
|  [02]   | `gmsh.model`             | model            | physical groups, entity/boundary queries, naming, visibility, geometry   |
|  [03]   | `gmsh.model.geo`         | built-in kernel  | point/curve/surface/volume primitives, extrude/revolve, `synchronize`    |
|  [04]   | `gmsh.model.occ`         | OpenCASCADE      | B-Rep primitives, boolean cut/fuse/fragment, fillet/chamfer, STEP import |
|  [05]   | `gmsh.model.mesh`        | meshing          | `generate`, size/order/transfinite/recombine control, node/element query |
|  [06]   | `gmsh.model.mesh.field`  | size fields      | `add`/`setNumber`/`setNumbers` fields, `setAsBackgroundMesh`             |
|  [07]   | `gmsh.option`            | options          | `setNumber`/`setString`/`setColor` global mesh/geometry option knobs     |
|  [08]   | `gmsh.view` / `plugin`   | post-processing  | model/list data views, aliases, probe, plugin execution                  |
|  [09]   | `gmsh.logger`            | telemetry        | `get`/`start`/`stop`, CPU/wall time, memory, last-error retrieval        |
|  [10]   | `gmsh.onelab` / `parser` | parameter/script | ONELAB parameter exchange and `.geo` script parse/evaluate               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: session lifecycle and model selection

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RESULT]                                    |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `initialize(argv=[], readConfigFiles=True)`   | lifecycle      | opens the kernel; pair with `finalize()`    |
|  [02]   | `finalize()` \| `isInitialized()`             | lifecycle      | closes the kernel / kernel-open probe       |
|  [03]   | `model.add(name)` \| `model.setCurrent(name)` | model select   | new/active named model in the session       |
|  [04]   | `model.getCurrent()` \| `clear()`             | model select   | active model name / empty the current model |

[ENTRYPOINT_SCOPE]: constructive geometry over the built-in and OpenCASCADE kernels

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [RESULT]                                        |
| :-----: | :-------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `model.geo.addPoint(x, y, z, meshSize=0.0, tag=-1)`                   | built-in       | point entity tag with a local target size       |
|  [02]   | `model.geo.addLine` \| `addCircleArc` \| `addSpline` \| `addBSpline`  | built-in       | curve entity tag                                |
|  [03]   | `model.geo.addCurveLoop` \| `addPlaneSurface` \| `addSurfaceFilling`  | built-in       | surface entity tag from bounding loops          |
|  [04]   | `model.geo.addSurfaceLoop` \| `addVolume`                             | built-in       | volume entity tag from bounding surfaces        |
|  [05]   | `model.geo.extrude` \| `revolve` \| `twist` \| `extrudeBoundaryLayer` | built-in       | swept entities from a `(dim, tag)` source list  |
|  [06]   | `model.occ.addBox` \| `addSphere` \| `addCylinder` \| `addTorus`      | OpenCASCADE    | primitive solid `(dim, tag)`                    |
|  [07]   | `model.occ.cut` \| `fuse` \| `intersect` \| `fragment`                | OpenCASCADE    | boolean result `(outDimTags, outDimTagsMap)`    |
|  [08]   | `model.occ.fillet` \| `chamfer` \| `importShapes(fileName)`           | OpenCASCADE    | filleted/chamfered solids / imported STEP-IGES  |
|  [09]   | `model.geo.synchronize()` \| `model.occ.synchronize()`                | commit         | promote kernel geometry into the meshable model |

[ENTRYPOINT_SCOPE]: physical groups and entity queries

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RESULT]                                      |
| :-----: | :----------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `model.addPhysicalGroup(dim, tags, tag=-1, name='')`         | naming         | physical-group tag over the given entity tags |
|  [02]   | `model.getPhysicalGroups(dim=-1)`                            | query          | `(dim, tag)` list of all physical groups      |
|  [03]   | `model.getEntitiesForPhysicalGroup(dim, tag)`                | query          | entity tags belonging to a physical group     |
|  [04]   | `model.getEntitiesForPhysicalName(name)`                     | query          | `(dim, tag)` entities for a named group       |
|  [05]   | `model.getEntities(dim=-1)` \| `model.getBoundary(dimTags)`  | query          | model entities / boundary of an entity set    |
|  [06]   | `model.setPhysicalName(dim, tag, name)` \| `getPhysicalName` | naming         | attach/read a physical-group name             |

[ENTRYPOINT_SCOPE]: mesh generation, size control, and structured meshing

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RESULT]                                          |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `model.mesh.generate(dim=3)`                                 | generation     | 1/2/3-D mesh over the synchronized model          |
|  [02]   | `model.mesh.setSize(dimTags, size)`                          | size control   | target element size at the given entities         |
|  [03]   | `model.mesh.setSizeCallback(fn)` \| `removeSizeCallback()`   | size control   | `(dim, tag, x, y, z, lc) -> size` per-point rule  |
|  [04]   | `model.mesh.setTransfiniteCurve(tag, numNodes, ...)`         | structured     | prescribed node count/distribution along a curve  |
|  [05]   | `model.mesh.setTransfiniteSurface` \| `setTransfiniteVolume` | structured     | mapped structured surface/volume mesh             |
|  [06]   | `model.mesh.setRecombine(dim, tag)` \| `recombine()`         | structured     | recombine triangles/tets into quads/hexes         |
|  [07]   | `model.mesh.setOrder(order)`                                 | high-order     | promote elements to the given interpolation order |
|  [08]   | `model.mesh.optimize(method='', niter=1)` \| `refine()`      | quality        | Netgen/Laplace optimization / uniform refinement  |
|  [09]   | `model.mesh.setAlgorithm(dim, tag, val)` \| `embed(...)`     | control        | per-surface 2-D algorithm / embed lower entities  |

[ENTRYPOINT_SCOPE]: size fields for background mesh and boundary layers
- `model.mesh.field.add`: mints a field by string type (`Distance`, `Threshold`, `Box`, `Min`, `MathEval`, `BoundaryLayer`); a `Threshold` over a `Distance` field is the canonical distance-graded refinement.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RESULT]                                      |
| :-----: | :------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `model.mesh.field.add(fieldType, tag=-1)`          | field mint     | size field of the named type                  |
|  [02]   | `model.mesh.field.setNumber(tag, option, value)`   | parameter      | scalar field parameter                        |
|  [03]   | `model.mesh.field.setNumbers(tag, option, values)` | parameter      | list field parameter (entity/field tag lists) |
|  [04]   | `model.mesh.field.setAsBackgroundMesh(tag)`        | install        | field drives global element size              |
|  [05]   | `model.mesh.field.setAsBoundaryLayer(tag)`         | install        | field drives a boundary-layer mesh            |

[ENTRYPOINT_SCOPE]: mesh extraction and codec IO
- `model.mesh.getNodes`: `coord` is a flat `3*N` NumPy-compatible array; `getElements` returns tuples per element type.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RESULT]                                           |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `model.mesh.getNodes(dim=-1, tag=-1, includeBoundary=False)` | extraction     | `(nodeTags, coord, parametricCoord)`               |
|  [02]   | `model.mesh.getElements(dim=-1, tag=-1)`                     | extraction     | `(elementTypes, elementTags, nodeTags)`            |
|  [03]   | `model.mesh.getElementsByType(elementType, tag=-1)`          | extraction     | element/node tags for one gmsh type integer        |
|  [04]   | `model.mesh.getNodesForPhysicalGroup(dim, tag)`              | extraction     | node tags and coords for a named group             |
|  [05]   | `model.mesh.getElementProperties(elementType)`               | metadata       | `(name, dim, order, numNodes, ...)` for a type int |
|  [06]   | `model.mesh.getJacobians(...)` \| `getBasisFunctions(...)`   | metadata       | element Jacobians / reference basis at quad points |
|  [07]   | `write(fileName)` \| `merge(fileName)` \| `open(fileName)`   | codec IO       | extension-dispatched mesh write / merge / open     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- lifecycle: the kernel and current-model selector are process-global — one `initialize()`/`finalize()` pair brackets the generation arm, run on the compute worker off the event loop. Concurrent callers isolate by process; callers sharing a process serialize through one session-owner lock, because `model.setCurrent(name)` races across named models.
- kernel: `geo` builds bottom-up from points to volumes; `occ` is the OpenCASCADE B-Rep kernel for booleans, fillets, and STEP/IGES import over `(dim, tag)` pairs. A model mixes at most one kernel, and both require `synchronize()` before `mesh.generate` sees the geometry.
- size: element density composes per-point `meshSize`, entity `setSize`, and a background `field` — a `Threshold` over a `Distance` field is the canonical graded-refinement source installed via `field.setAsBackgroundMesh`.
- naming: `addPhysicalGroup` is the sole route naming a boundary or material region, and `getElementProperties` resolves any gmsh element-type integer to its `(name, dim, order, numNodes)` record.

[STACKING]:
- `meshio`(`.api/meshio.md`): `write` emits a `.msh` that `meshio` reads, gmsh physical groups arriving as `cell_sets` and integer `cell_data`.
- `scikit-fem`(`.api/scikit-fem.md`): the read mesh assembles through scikit-fem, physical groups feeding `get_dofs` boundary/subdomain selection.
- `MeshField`/`MeshExchange`: extraction tuples cross as data into the content-keyed `MeshField`; every physical group maps onto the named `MeshField` set the read arm mints and gmsh element-type integers map onto `ElementKind`, so a generated mesh and a read mesh carry identical group and element semantics.

[LOCAL_ADMISSION]:
- import: top-level `gmsh` module; namespaced functions carry no return-object identity, so extraction tuples cross as data into the `MeshField`/`MeshExchange` owner, and no geometry-branch kernel is imported — boundary input arrives as data per the compute charter.
- boundary: gmsh generation is offline study evidence; the generated `MeshField` crosses the `HandoffAxis` as receipt data, and production mesh-substrate authority stays in the C# managed owner.

[RAIL_LAW]:
- Package: `gmsh`
- Owns: 1/2/3-D unstructured mesh generation, built-in and OpenCASCADE constructive geometry with boolean/fillet/STEP-import operations, named physical groups, background/boundary-layer size fields, transfinite/recombine structured meshing, high-order promotion, mesh optimization, node/element/Jacobian extraction, and `.msh`/VTK/STL/MED codec IO
- Accept: a synchronized `geo`/`occ` geometry with `addPhysicalGroup` regions, meshed via `generate` under composed size sources, extracted through `getNodes`/`getElements` or the `.msh`/`meshio` round-trip into a content-keyed `MeshField`
- Reject: hand-rolled Delaunay/frontal meshing, CAD boolean geometry, `.msh` codec logic, or physical-group bookkeeping the kernel owns; concurrent access without process isolation or the session-owner lock; a geometry-branch kernel import when boundary input crosses as data
