# [PY_GEOMETRY_API_COMPAS_DR]

`compas_dr` owns dynamic-relaxation form-finding for axial-force networks on the COMPAS spine: pure-Python and numpy/scipy relaxation solvers, geometric constraint projection by registered geometry type, and tributary-area selfweight loads. Its `InputData` and `Constraint` carriers extend `compas.data.Data`, graduating through the shared `json_dumps`/`json_loads` handle, and `InputData.from_mesh` consumes a `compas.datastructures.Mesh`. Its numpy band offloads through `compas.rpc.Proxy`, and the relaxation loop, RK integrator, and constraint-projection algebra never re-implement here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas_dr`
- package: `compas_dr` (MIT)
- import: `import compas_dr`
- owner: `geometry`
- rail: form-finding
- entry points: none (library only)
- capability: pure-Python and numpy/scipy dynamic-relaxation form-finding with selectable RK order, geometric constraint projection by registered geometry type, tributary-area selfweight loads, and COMPAS `Data` serialization of the input and constraint carriers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver data carriers — `compas_dr.numdata`

`InputData` and `ResultData` live in `compas_dr.numdata`; `InputData` lazily materializes its numpy arrays and the `C` connectivity matrix from list inputs and round-trips through COMPAS JSON.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                                                                         |
| :-----: | :----------------------------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `compas_dr.numdata.InputData`  | `Data` record  | inputs `vertices`/`edges`/`fixed`/`loads`/`qpre`/`fpre`/`lpre`/`linit`/`E`/`radius`  |
|  [02]   | `compas_dr.numdata.ResultData` | result carrier | output fields `xyz`/`q`/`forces`/`lengths`/`residuals`; `update_mesh` mesh writeback |

[PUBLIC_TYPE_SCOPE]: constraint registry — `compas_dr.constraints`

`Constraint(geometry)` is itself the polymorphic factory: `__new__` routes through `get_constraint_cls`, an MRO walk over the `GEOMETRY_CONSTRAINT` registry that returns the concrete `*Constraint` subclass, so no per-type constructor exists. A node binds by setting `constraint.location` (the first assignment triggers `project()`) and `constraint.residual`; `tangent`/`normal`/`param` are lazily computed managed properties.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                                            |
| :-----: | :------------------ | :----------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Constraint`        | base + factory     | managed `geometry`/`location`/`residual`/`tangent`/`normal`/`param`                     |
|  [02]   | `LineConstraint`    | line constraint    | project node onto a segment; `update(damping=0.1)` steps along tangent then re-projects |
|  [03]   | `PlaneConstraint`   | plane constraint   | project node onto a plane                                                               |
|  [04]   | `CircleConstraint`  | circle constraint  | project node onto a circle                                                              |
|  [05]   | `CurveConstraint`   | curve constraint   | project node onto a `NurbsCurve`                                                        |
|  [06]   | `SurfaceConstraint` | surface constraint | project node onto a `NurbsSurface`                                                      |

[PUBLIC_TYPE_SCOPE]: load utilities — `compas_dr.loads`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                                                                                  |
| :-----: | :--------------------- | :-------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `SelfweightCalculator` | load calculator | called as `calculator(xyz) -> FloatNx1` = `tributary_area * (thickness * density)` per vertex |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solvers — `compas_dr.solvers`

Free functions sharing one relaxation tail. `dr` is the list-based reference (`kmax=100`); `dr_numpy`/`dr_constrained_numpy` take an `InputData`, default `kmax=10000`, and add `rk_steps: Literal[1,2,4]=2` selecting RK order (`1` explicit Euler, `2` RK2, `4` RK4).
- solver tail carry: `dt=1.0`, `tol1=1e-3`, `tol2=1e-6`, `c=0.1`, `callback`, `callback_args`

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY]                       |
| :-----: | :----------------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `dr(vertices, edges, fixed, loads, qpre, …, kmax=100)`                               | list-based pure-Python reference   |
|  [02]   | `dr_numpy(indata, kmax=10000, rk_steps=2) -> ResultData`                             | vectorized numpy production        |
|  [03]   | `dr_constrained_numpy(*, indata, constraints, kmax=10000, rk_steps=2) -> ResultData` | numpy, per-step constraint project |

`callback(k, x, crit1, crit2, callback_args)` fires each iteration; a non-callable `callback` raises `ValueError`.

[ENTRYPOINT_SCOPE]: InputData construction and serialization

`from_json`/`to_json`/`from_jsonstring`/`copy` inherit from `compas.data.Data`.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `InputData.from_mesh(mesh, fixed, loads, qpre, …) -> InputData`          | factory  | build from `Mesh` via `vertices_attributes("xyz")` |
|  [02]   | `InputData.from_json(path)` / `from_jsonstring(string) -> InputData`     | factory  | COMPAS JSON decode                                 |
|  [03]   | `InputData.to_json(path, *, pretty=False, compact=False, minimal=False)` | instance | COMPAS JSON encode                                 |
|  [04]   | `InputData.copy(cls=None, copy_guid=False) -> InputData`                 | instance | deep copy                                          |

[ENTRYPOINT_SCOPE]: ResultData fields and mesh writeback

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `ResultData.update_mesh(mesh, vertex_index=None)`           | instance | apply solved `xyz` to a `Mesh`; `vertex_index` defaults to enum |
|  [02]   | `ResultData.xyz` / `q` / `forces` / `lengths` / `residuals` | property | positions, densities, forces, lengths, residuals                |

`ResultData.__data__` returns `{}`, so its JSON round-trip is lossy — persist the solved `Mesh`, never the `ResultData`.

[ENTRYPOINT_SCOPE]: Constraint operations

Base `project`/`update`/`compute_*`/`update_location_at_param` raise `NotImplementedError`; subclasses implement per geometry. An unregistered geometry raises `GeometryNotRegisteredAsConstraint`.

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Constraint(geometry, name=None) -> *Constraint`                        | factory  | registered subclass for the geometry type      |
|  [02]   | `Constraint.get_constraint_cls(geometry) -> type`                       | static   | resolve the `*Constraint` class by MRO walk    |
|  [03]   | `Constraint.register(gtype, ctype)`                                     | static   | bind a geometry type to a constraint subclass  |
|  [04]   | `constraint.project()`                                                  | instance | project the node onto the geometry             |
|  [05]   | `constraint.update(damping=0.1)`                                        | instance | step `location += tangent*damping`, re-project |
|  [06]   | `constraint.compute_param()` / `compute_normal()` / `compute_tangent()` | instance | populate `param`/`normal`/`tangent`            |

[ENTRYPOINT_SCOPE]: SelfweightCalculator — `compas_dr.loads`

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `SelfweightCalculator(mesh, density=1.0, thickness_attr_name="t")`      | ctor     | bind to a `Mesh`; `thickness*density` into `rho` |
|  [02]   | `SelfweightCalculator.__call__(xyz) -> FloatNx1`                        | instance | `tributary_area * rho` at the current `xyz`      |
|  [03]   | `SelfweightCalculator.compute_tributary_areas(xyz) -> FloatNx1`         | instance | per-vertex tributary area over loaded faces      |
|  [04]   | `SelfweightCalculator.compute_face_matrix() -> scipy.sparse.csr_matrix` | instance | normalized face-vertex centroid matrix           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import surface: solvers from `compas_dr.solvers`, carriers from `compas_dr.numdata`, constraints from `compas_dr.constraints`, the calculator from `compas_dr.loads`; the package root exports only `HOME`/`DATA`/`DOCS`/`TEMP` path anchors, no API.
- `InputData` properties lazily materialize numpy arrays (`vertices` reshaped Nx3 float64, `edges` Nx2 int32, the rest reshaped float64) and cache the connectivity matrix `C`; `free` is `range(n) - fixed`, and unset `fpre`/`lpre`/`linit`/`E`/`radius` default to zero over the edge count.
- `Constraint` is a geometry-keyed registry, not a caller-selected hierarchy: `compas_dr.constraints` runs the `Constraint.register(...)` calls at import, so `Constraint(line)` already returns a `LineConstraint`; `dr_constrained_numpy` applies the constraint sequence after each relaxation step, `update(damping=0.1)` the per-step move.
- `SelfweightCalculator` binds once against the mesh (caching the face matrix), then recomputes selfweight from the current `xyz` each iteration so gravity tracks geometry change.

[STACKING]:
- `compas`(`.api/compas.md`): `InputData` and every `Constraint` extend `compas.data.Data`, graduating through `compas.json_dumps`/`json_loads`, and `InputData.from_mesh` consumes a `compas.datastructures.Mesh`; the numpy solvers offload out of process through `compas.rpc.Proxy.function("compas_dr.solvers.dr_numpy")` across the runtime THREAD band under `RetryClass.RPC`, the eager reconnect-or-spawn Proxy lifecycle owned by `compas.md`.
- `compas-tna`(`.api/compas-tna.md`): the two COMPAS solver companions share the one form-finding fault rail (`RetryClass.RPC`) and ride the same `compas` `Mesh`/`json_*` spine, selected apart by the algebra owner's `FormEngine` sub-enum.
- `graph/algebra.md#ALGEBRA`: selects this solver on the one form-finding case, threads the `solver_proxy` async-resource scope so a fan of solves shares one reconnected worker, and decodes constraint geometry through `json_loads` so `Constraint(decoded_geometry)` dispatches on the real decoded type.

[LOCAL_ADMISSION]:
- form-finding pipeline: build `InputData.from_mesh(mesh, fixed, loads, qpre, ...)`, run `dr_numpy(indata)` or `dr_constrained_numpy(indata=..., constraints=[Constraint(geometry), ...])`, then `ResultData.update_mesh(mesh)`; constraints attach by node index, built polymorphically via `Constraint(geometry)`.
- `Mesh` seam: `InputData.from_mesh` and `ResultData.update_mesh` are the only `Mesh` touch-points; the solver never reads mesh topology directly.

[RAIL_LAW]:
- Package: `compas-dr`
- Owns: dynamic-relaxation form-finding, geometric constraint projection by registered geometry type, tributary-area selfweight loads
- Accept: `InputData` from a `compas` `Mesh`, constraint sequences built via `Constraint(geometry)`, numpy position arrays
- Reject: hand-rolled dynamic-relaxation or RK-integration loops, direct scipy sparse assembly outside this package, parallel `*Constraint` selection that bypasses the `Constraint(geometry)` registry factory
