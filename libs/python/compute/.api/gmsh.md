# [PY_COMPUTE_API_GMSH]

`gmsh` supplies unstructured 1/2/3-D finite-element mesh generation for the compute `[MESH_GENERATION_ROUTE]`: a boundary or CAD description enters the built-in `geo` or OpenCASCADE `occ` kernel, `addPhysicalGroup` tags mark boundary and material regions, size fields drive element density, and `generate` meshes the model. Node/element/physical-group extraction with the `.msh` write emit the same content `meshio` reads and `scikit-fem` assembles; this owner drives the process-global kernel through `initialize`/`finalize`, maps physical groups onto named sets and gmsh element-type integers onto `ElementKind`, and hand-rolls no Delaunay/frontal meshing, CAD boolean geometry, or `.msh` codec the kernel owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `gmsh`
- package: `gmsh`
- import: `gmsh`; the whole API is module-level functions under namespace objects (`gmsh.model`, `gmsh.model.mesh`, `gmsh.option`), never classes to instantiate
- owner: `compute`
- rail: mesh generation
- state: one process-global kernel — `initialize()` opens it, `finalize()` closes it, and `model.add(name)`/`model.setCurrent(name)` switch between named models inside a live session
- alias: every function carries a snake_case alias beside its camelCase canonical (`add_physical_group` == `addPhysicalGroup`); the catalog documents the camelCase SDK canonical
- capability: built-in (`geo`) and OpenCASCADE (`occ`) constructive geometry, boolean/fillet/chamfer CAD operations, named physical groups, threshold/distance/box/min mesh size fields, 1/2/3-D Delaunay/frontal/HXT meshing, transfinite/recombine structured meshing, high-order element promotion, mesh optimization, node/element/Jacobian/basis extraction, and `.msh`/VTK/STL/MED codec IO

## [02]-[NAMESPACE_SURFACE]

[NAMESPACE_SCOPE]: kernel namespaces
- rail: mesh generation
- `model` is the active-model surface; `geo` and `occ` are the two geometry kernels under it; `mesh` owns generation and extraction; `field` under `mesh` owns size fields. A geometry kernel mutation is invisible to meshing until its `synchronize()` promotes it into the model.

| [INDEX] | [NAMESPACE]              | [FAMILY]         | [CAPABILITY]                                                              |
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
- rail: mesh generation

`initialize` opens the process-global kernel (`readConfigFiles`/`run`/`interruptible` knobs); every session pairs it with `finalize`. `model.add` names a fresh empty model, `model.setCurrent`/`getCurrent` switch the active one, and `clear` empties the current model without closing the kernel.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RESULT]                                        |
| :-----: | :--------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `initialize(argv=[], readConfigFiles=True)`          | lifecycle      | opens the kernel; pair with `finalize()`        |
|  [02]   | `finalize()` \| `isInitialized()`                    | lifecycle      | closes the kernel / kernel-open probe           |
|  [03]   | `model.add(name)` \| `model.setCurrent(name)`        | model select   | new/active named model in the session           |
|  [04]   | `model.getCurrent()` \| `clear()`                    | model select   | active model name / empty the current model     |

[ENTRYPOINT_SCOPE]: constructive geometry (built-in and OpenCASCADE kernels)
- rail: mesh generation

`geo` builds bottom-up (points -> curves -> curve loops -> surfaces -> surface loops -> volumes); `occ` adds top-down B-Rep primitives (`addBox`/`addSphere`/`addCylinder`) and boolean composition. Both return integer entity tags and both require `synchronize()` before meshing. `occ` boolean and feature ops (`cut`/`fuse`/`fragment`/`fillet`/`chamfer`) take and return `(dim, tag)` pairs.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [RESULT]                                          |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `model.geo.addPoint(x, y, z, meshSize=0.0, tag=-1)`            | built-in       | point entity tag with a local target size         |
|  [02]   | `model.geo.addLine` \| `addCircleArc` \| `addSpline` \| `addBSpline` | built-in  | curve entity tag                                  |
|  [03]   | `model.geo.addCurveLoop` \| `addPlaneSurface` \| `addSurfaceFilling` | built-in  | surface entity tag from bounding loops            |
|  [04]   | `model.geo.addSurfaceLoop` \| `addVolume`                      | built-in       | volume entity tag from bounding surfaces          |
|  [05]   | `model.geo.extrude` \| `revolve` \| `twist` \| `extrudeBoundaryLayer` | built-in | swept entities from a `(dim, tag)` source list    |
|  [06]   | `model.occ.addBox` \| `addSphere` \| `addCylinder` \| `addTorus` | OpenCASCADE  | primitive solid `(dim, tag)`                       |
|  [07]   | `model.occ.cut` \| `fuse` \| `intersect` \| `fragment`         | OpenCASCADE    | boolean result `(outDimTags, outDimTagsMap)`      |
|  [08]   | `model.occ.fillet` \| `chamfer` \| `importShapes(fileName)`    | OpenCASCADE    | filleted/chamfered solids / imported STEP-IGES    |
|  [09]   | `model.geo.synchronize()` \| `model.occ.synchronize()`         | commit         | promote kernel geometry into the meshable model   |

[ENTRYPOINT_SCOPE]: physical groups and entity queries
- rail: mesh generation

`addPhysicalGroup(dim, tags, tag=-1, name='')` is the one route that names a boundary or material region; the round-trip reads it back through `getPhysicalGroups`/`getEntitiesForPhysicalGroup` and the by-name `getEntitiesForPhysicalName`. Physical groups are the named-set bridge — every group becomes a `meshio` cell set / integer cell data and a `scikit-fem` boundary/subdomain tag.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RESULT]                                         |
| :-----: | :----------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `model.addPhysicalGroup(dim, tags, tag=-1, name='')`        | naming         | physical-group tag over the given entity tags    |
|  [02]   | `model.getPhysicalGroups(dim=-1)`                           | query          | `(dim, tag)` list of all physical groups         |
|  [03]   | `model.getEntitiesForPhysicalGroup(dim, tag)`              | query          | entity tags belonging to a physical group        |
|  [04]   | `model.getEntitiesForPhysicalName(name)`                   | query          | `(dim, tag)` entities for a named group          |
|  [05]   | `model.getEntities(dim=-1)` \| `model.getBoundary(dimTags)` | query          | model entities / boundary of an entity set       |
|  [06]   | `model.setPhysicalName(dim, tag, name)` \| `getPhysicalName` | naming        | attach/read a physical-group name                |

[ENTRYPOINT_SCOPE]: mesh generation, size control, and structured meshing
- rail: mesh generation

`generate(dim=3)` runs the 1/2/3-D pipeline over the synchronized model. Element density comes from three composable sources: per-point `meshSize`, `setSize(dimTags, size)` on entities, and background size fields. Structured meshing uses `setTransfiniteCurve`/`setTransfiniteSurface`/`setTransfiniteVolume` with `setRecombine` for quad/hex; `setOrder(order)` promotes to high-order (P2+) elements; `optimize(method)` improves quality.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RESULT]                                         |
| :-----: | :---------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `model.mesh.generate(dim=3)`                               | generation     | 1/2/3-D mesh over the synchronized model         |
|  [02]   | `model.mesh.setSize(dimTags, size)`                        | size control   | target element size at the given entities        |
|  [03]   | `model.mesh.setSizeCallback(fn)` \| `removeSizeCallback()` | size control   | `(dim, tag, x, y, z, lc) -> size` per-point rule |
|  [04]   | `model.mesh.setTransfiniteCurve(tag, numNodes, ...)`       | structured     | prescribed node count/distribution along a curve |
|  [05]   | `model.mesh.setTransfiniteSurface` \| `setTransfiniteVolume` | structured   | mapped structured surface/volume mesh            |
|  [06]   | `model.mesh.setRecombine(dim, tag)` \| `recombine()`       | structured     | recombine triangles/tets into quads/hexes        |
|  [07]   | `model.mesh.setOrder(order)`                               | high-order     | promote elements to the given interpolation order |
|  [08]   | `model.mesh.optimize(method='', niter=1)` \| `refine()`    | quality        | Netgen/Laplace optimization / uniform refinement |
|  [09]   | `model.mesh.setAlgorithm(dim, tag, val)` \| `embed(...)`   | control        | per-surface 2-D algorithm / embed lower entities |

[ENTRYPOINT_SCOPE]: size fields (background mesh and boundary layers)
- rail: mesh generation

`field.add(fieldType, tag=-1)` mints a field by string type (`Distance`, `Threshold`, `Box`, `Min`, `MathEval`, `BoundaryLayer`); `setNumber`/`setNumbers`/`setString` set its parameters; `setAsBackgroundMesh(tag)` installs it as the global size driver and `setAsBoundaryLayer(tag)` as a boundary-layer field. A `Threshold` over a `Distance` field is the canonical distance-graded refinement.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RESULT]                                        |
| :-----: | :--------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `model.mesh.field.add(fieldType, tag=-1)`           | field mint     | size field of the named type                    |
|  [02]   | `model.mesh.field.setNumber(tag, option, value)`    | parameter      | scalar field parameter                          |
|  [03]   | `model.mesh.field.setNumbers(tag, option, values)`  | parameter      | list field parameter (entity/field tag lists)   |
|  [04]   | `model.mesh.field.setAsBackgroundMesh(tag)`         | install        | field drives global element size                |
|  [05]   | `model.mesh.field.setAsBoundaryLayer(tag)`          | install        | field drives a boundary-layer mesh              |

[ENTRYPOINT_SCOPE]: mesh extraction and codec IO
- rail: mesh generation

Extraction returns flat NumPy-compatible tuples: `getNodes` yields `(nodeTags, coord, parametricCoord)` with `coord` a flat `3*N` array, and `getElements` yields `(elementTypes, elementTags, nodeTags)` per element type. `getElementProperties(elementType)` resolves a gmsh type integer to `(name, dim, order, numNodes, ...)`; `write(fileName)` dispatches on extension across `.msh`/`.vtk`/`.stl`/`.med`, feeding the `meshio` round-trip.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RESULT]                                            |
| :-----: | :------------------------------------------------------------ | :------------- | :-------------------------------------------------- |
|  [01]   | `model.mesh.getNodes(dim=-1, tag=-1, includeBoundary=False)` | extraction     | `(nodeTags, coord, parametricCoord)`                |
|  [02]   | `model.mesh.getElements(dim=-1, tag=-1)`                     | extraction     | `(elementTypes, elementTags, nodeTags)`             |
|  [03]   | `model.mesh.getElementsByType(elementType, tag=-1)`         | extraction     | element/node tags for one gmsh type integer         |
|  [04]   | `model.mesh.getNodesForPhysicalGroup(dim, tag)`             | extraction     | node tags and coords for a named group              |
|  [05]   | `model.mesh.getElementProperties(elementType)`             | metadata       | `(name, dim, order, numNodes, ...)` for a type int  |
|  [06]   | `model.mesh.getJacobians(...)` \| `getBasisFunctions(...)` | metadata       | element Jacobians / reference basis at quad points  |
|  [07]   | `write(fileName)` \| `merge(fileName)` \| `open(fileName)` | codec IO       | extension-dispatched mesh write / merge / open      |

## [04]-[IMPLEMENTATION_LAW]

[SESSION_TOPOLOGY]:
- lifecycle: the kernel and current-model selector are process-global — one `initialize()`/`finalize()` pair brackets the generation arm, and the arm runs on the compute worker off the event loop. Concurrent callers use separate processes; callers sharing a process enter through one session-owner lock because `model.setCurrent(name)` races across named models.
- kernel choice: `geo` is the built-in bottom-up kernel for parametric primitives; `occ` is OpenCASCADE for B-Rep booleans, fillets, and STEP/IGES import. A model mixes at most one geometry kernel; both require `synchronize()` before `mesh.generate` sees the geometry.
- size sources: element density composes from per-point `meshSize`, entity `setSize`, and background `field` — a `Threshold` over a `Distance` field is the canonical graded-refinement source, installed via `field.setAsBackgroundMesh`.

[ROUND_TRIP]:
- physical groups: `addPhysicalGroup(dim, tags, name=...)` names every boundary and material region; the generation arm reads them back through `getPhysicalGroups`/`getEntitiesForPhysicalGroup` and maps each onto the same named `MeshField` set the read arm mints, so a generated mesh and a read mesh carry identical group semantics.
- element mapping: gmsh element-type integers map onto `ElementKind` — `1`=Line(2), `2`=Triangle(3), `3`=Quadrilateral(4), `4`=Tetrahedron(4), `5`=Hexahedron(8), `6`=Prism(6), `7`=Pyramid(5), high-order `8`=Line3, `9`=Triangle6, `11`=Tetrahedron10, and `15`=Point(1); `getElementProperties` resolves any integer to its `(name, dim, order, numNodes)` record.
- exchange: the arm either extracts nodes/elements directly through `getNodes`/`getElements` into the content-keyed `MeshField`, or writes a `.msh` that `meshio` reads (`.api/meshio.md` — gmsh physical groups arrive as `cell_sets`/integer `cell_data`), and the mesh then assembles through `scikit-fem` (`.api/scikit-fem.md`) with the physical groups feeding `get_dofs` boundary/subdomain selection.

[LOCAL_ADMISSION]:
- import: top-level `gmsh` module; the namespaced functions carry no return-object identity — extraction tuples cross as data into the `MeshField`/`MeshExchange` owner, and no geometry-branch kernel is imported (boundary input arrives as data per the compute charter).
- boundary: gmsh generation is offline study evidence; the generated `MeshField` crosses the `HandoffAxis` as receipt data, and production mesh substrate authority stays in the C# managed owner.

[RAIL_LAW]:
- Package: `gmsh`
- Owns: 1/2/3-D unstructured mesh generation, built-in and OpenCASCADE constructive geometry with boolean/fillet/STEP-import operations, named physical groups, background/boundary-layer size fields, transfinite/recombine structured meshing, high-order promotion, mesh optimization, node/element/Jacobian extraction, and `.msh`/VTK/STL/MED codec IO
- Accept: a synchronized `geo`/`occ` geometry with `addPhysicalGroup` regions, meshed via `generate` under composed size sources, extracted through `getNodes`/`getElements` or the `.msh`/`meshio` round-trip into a content-keyed `MeshField`
- Reject: hand-rolled Delaunay/frontal meshing, CAD boolean geometry, `.msh` codec logic, or physical-group bookkeeping the kernel owns; concurrent access without process isolation or the session-owner lock; a geometry-branch kernel import when boundary input crosses as data
