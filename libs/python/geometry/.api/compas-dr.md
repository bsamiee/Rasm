# [PY_GEOMETRY_API_COMPAS_DR]

`compas_dr` supplies dynamic-relaxation form-finding for axial-force networks on the COMPAS spine: a pure-Python `dr` reference solver, the numpy/scipy-accelerated `dr_numpy` and `dr_constrained_numpy` production solvers, the `InputData`/`ResultData` numerical carriers in `compas_dr.numdata`, the polymorphic `Constraint` registry (`Line`/`Plane`/`Circle`/`NurbsCurve`/`NurbsSurface` -> `*Constraint`), and a callable `SelfweightCalculator` for tributary-area gravity loads. It rides `compas` as its spine — `InputData`/`Constraint` are `compas.data.Data` subclasses that round-trip through the same `json_dumps`/`json_loads` graduation handle, and `InputData.from_mesh` consumes a `compas.datastructures.Mesh`. The `graph/algebra.md#ALGEBRA` owner selects this solver through the `FormEngine` sub-enum on the one form-finding case and offloads the numpy band out of process through `compas.rpc.Proxy.function("compas_dr.solvers.dr_numpy")`; it never re-implements the relaxation loop, the RK integrator, or the constraint-projection algebra.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas_dr`
- package: `compas_dr`
- import: `import compas_dr`
- owner: `geometry`
- rail: form-finding
- installed: `0.3.1`
- license: MIT
- depends: `compas>=2.1` (the only runtime dependency; `numba` is an optional `hpc` extra, not admitted)
- entry points: none (library only; `compas_dr.install` is the COMPAS plugin hook, not a console script)
- capability: pure-Python and numpy/scipy dynamic-relaxation form-finding with selectable RK order, geometric constraint projection by registered geometry type, tributary-area selfweight loads, and COMPAS `Data` serialization of the input/constraint carriers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver data carriers — `compas_dr.numdata`
- rail: form-finding
- type-family: `compas.data.Data` subclass

`InputData` and `ResultData` live in `compas_dr.numdata`, not the package root (the `compas_dr.__init__` exports only `HOME`/`DATA`/`DOCS`/`TEMP` path anchors). `InputData` lazily materializes numpy arrays from its list inputs and exposes the derived `free`/`q0`/`l0`/`v0`/`r0` properties plus the `C` connectivity matrix, round-tripping through COMPAS JSON; `ResultData.__data__` returns `{}`, so it is an in-memory result carrier whose `from_json`/`to_json` round-trip is lossy — persist results by writing the solved `Mesh`, not the `ResultData`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                                                                         |
| :-----: | :----------------------------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `compas_dr.numdata.InputData`  | `Data` record  | inputs `vertices`/`edges`/`fixed`/`loads`/`qpre`/`fpre`/`lpre`/`linit`/`E`/`radius`  |
|  [02]   | `compas_dr.numdata.ResultData` | result carrier | output fields `xyz`/`q`/`forces`/`lengths`/`residuals`; `update_mesh` mesh writeback |

[PUBLIC_TYPE_SCOPE]: constraint registry — `compas_dr.constraints`
- rail: form-finding
- type-family: `compas.data.Data` subclass, geometry-keyed registry

`Constraint(geometry)` is itself the polymorphic factory: `Constraint.__new__` calls `get_constraint_cls(geometry)`, which walks the geometry's MRO against the `GEOMETRY_CONSTRAINT` registry (`Line->LineConstraint`, `Plane->PlaneConstraint`, `Circle->CircleConstraint`, `NurbsCurve->CurveConstraint`, `NurbsSurface->SurfaceConstraint`) and returns the concrete subclass — there is no per-verb constructor proliferation. A node binds by setting `constraint.location` (the setter triggers `project()` on first assignment) and `constraint.residual`; `tangent`/`normal`/`param` are lazily computed managed properties.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                                            |
| :-----: | :------------------ | :----------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Constraint`        | base + factory     | managed `geometry`/`location`/`residual`/`tangent`/`normal`/`param`                     |
|  [02]   | `LineConstraint`    | line constraint    | project node onto a segment; `update(damping=0.1)` steps along tangent then re-projects |
|  [03]   | `PlaneConstraint`   | plane constraint   | project node onto a plane                                                               |
|  [04]   | `CircleConstraint`  | circle constraint  | project node onto a circle                                                              |
|  [05]   | `CurveConstraint`   | curve constraint   | project node onto a `NurbsCurve`                                                        |
|  [06]   | `SurfaceConstraint` | surface constraint | project node onto a `NurbsSurface`                                                      |

[PUBLIC_TYPE_SCOPE]: load utilities — `compas_dr.loads`
- rail: form-finding
- type-family: callable load computer

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                                                                                  |
| :-----: | :--------------------- | :-------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `SelfweightCalculator` | load calculator | called as `calculator(xyz) -> FloatNx1` = `tributary_area * (thickness * density)` per vertex |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solvers — `compas_dr.solvers`
- rail: form-finding

`dr` is the list-based reference path; `dr_numpy` and `dr_constrained_numpy` are the numpy/scipy production paths taking an `InputData`. Every solver shares the relaxation tail `dt=1.0, tol1=1e-3, tol2=1e-6, c=0.1, callback=None, callback_args=None`; the numpy pair adds `rk_steps: Literal[1,2,4]=2` selecting the Runge-Kutta order and defaults `kmax=10000` (the pure `dr` defaults `kmax=100`), and keyword-only `dr_constrained_numpy` takes `constraints: Sequence[Constraint]` applied after each step.

| [INDEX] | [SURFACE]                                                                                                  | [SOLVER]             |
| :-----: | :--------------------------------------------------------------------------------------------------------- | :------------------- |
|  [01]   | `dr(vertices, edges, fixed, loads, qpre, fpre=None, lpre=None, linit=None, E=None, radius=None, kmax=100)` | list-based reference |
|  [02]   | `dr_numpy(indata: InputData, kmax=10000, rk_steps=2) -> ResultData`                                        | vectorized numpy     |
|  [03]   | `dr_constrained_numpy(*, indata, constraints, kmax=10000, rk_steps=2) -> ResultData`                       | constrained numpy    |

[ENTRYPOINT_SCOPE]: InputData construction and serialization
- rail: form-finding

`callback(k, x, crit1, crit2, callback_args)` is invoked each iteration; a non-callable `callback` raises `ValueError`. `from_mesh` takes the source `mesh` then the InputData field arguments `fixed, loads, qpre, fpre=None, lpre=None, linit=None, E=None, radius=None`; `from_json`/`to_json`/`from_jsonstring`/`copy` are inherited from `compas.data.Data`.

| [INDEX] | [SURFACE]                                                                  | [ENTRY]                                          |
| :-----: | :------------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `InputData.from_mesh(mesh, fixed, loads, qpre, …) -> InputData`            | build from `Mesh` (`vertices_attributes("xyz")`) |
|  [02]   | `InputData.from_json(filepath)` / `from_jsonstring(string)` -> `InputData` | COMPAS JSON decode                               |
|  [03]   | `InputData.to_json(filepath, pretty=False, compact=False, minimal=False)`  | COMPAS JSON encode                               |
|  [04]   | `InputData.copy(cls=None, copy_guid=False) -> InputData`                   | deep copy                                        |

[ENTRYPOINT_SCOPE]: ResultData fields and mesh writeback
- rail: form-finding

| [INDEX] | [SURFACE]                                                   | [ENTRY]                                                               |
| :-----: | :---------------------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `ResultData.update_mesh(mesh, vertex_index=None)`           | apply solved `xyz` to a `Mesh`; `vertex_index` defaults to enum       |
|  [02]   | `ResultData.xyz` / `q` / `forces` / `lengths` / `residuals` | positions, force densities, axial forces, lengths, per-node residuals |

[ENTRYPOINT_SCOPE]: Constraint operations
- rail: form-finding

`get_constraint_cls` returns the constraint class (not an instance) by MRO walk; an unregistered geometry raises `GeometryNotRegisteredAsConstraint`. `project`/`update`/`compute_param`/`compute_normal`/`compute_tangent`/`update_location_at_param` are subclass-implemented (the base raises `NotImplementedError`); `from_json`/`to_json` are inherited.

| [INDEX] | [SURFACE]                                                               | [ENTRY]                                                     |
| :-----: | :---------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `Constraint(geometry, name=None) -> *Constraint`                        | construct the registered subclass for the geometry type     |
|  [02]   | `Constraint.get_constraint_cls(geometry, **kwargs) -> type`             | resolve the `*Constraint` class by MRO registry walk        |
|  [03]   | `Constraint.register(gtype, ctype)`                                     | bind a geometry type to a constraint subclass               |
|  [04]   | `constraint.project()`                                                  | project the constrained node onto the geometry              |
|  [05]   | `constraint.update(damping=0.1)`                                        | step `location += tangent*damping`, re-project off-geometry |
|  [06]   | `constraint.compute_param()` / `compute_normal()` / `compute_tangent()` | populate the `param`/`normal`/`tangent` managed properties  |

[ENTRYPOINT_SCOPE]: SelfweightCalculator — `compas_dr.loads`
- rail: form-finding
- entry-family: load computation

| [INDEX] | [SURFACE]                                                                 | [RAIL]                                                      |
| :-----: | :------------------------------------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | `SelfweightCalculator(mesh, density=1.0, thickness_attr_name="t")`        | construct against a `Mesh`; per-vertex thickness into `rho` |
|  [02]   | `SelfweightCalculator.call(xyz: FloatNx3) -> FloatNx1`                    | tributary area per vertex × `rho` at current `xyz`          |
|  [03]   | `SelfweightCalculator.compute_tributary_areas(xyz: FloatNx3) -> FloatNx1` | per-vertex tributary area (halfedge fan over loaded faces)  |
|  [04]   | `SelfweightCalculator.compute_face_matrix() -> scipy.sparse.csr_matrix`   | normalized face-vertex matrix (face centroids)              |

## [04]-[IMPLEMENTATION_LAW]

[DR_TOPOLOGY]:
- import surface: solvers from `compas_dr.solvers`, carriers from `compas_dr.numdata`, constraints from `compas_dr.constraints`, the calculator from `compas_dr.loads` — at boundary scope only per the manifest import policy; the package root carries no API.
- `dr` is the list-based reference implementation; `dr_numpy`/`dr_constrained_numpy` are the production vectorized paths accepting `InputData`. `rk_steps` selects RK order: `1` explicit Euler, `2` RK2, `4` RK4.
- `InputData` properties lazily materialize numpy arrays (`vertices` reshaped Nx3 float64, `edges` Nx2 int32, `loads`/`qpre`/`fpre`/`lpre`/`linit`/`E`/`radius` reshaped float64) and cache the connectivity matrix `C` via `compas.matrices.connectivity_matrix`; `free` is `range(n) - fixed`. Unset `fpre`/`lpre`/`linit`/`E`/`radius` default to zero vectors over the edge count.
- `Constraint` is a geometry-keyed registry, not a type hierarchy the caller selects: `compas_dr.constraints.__init__` runs the five `Constraint.register(...)` calls at import, so `Constraint(line)` already returns a `LineConstraint`. The damped `update(damping=0.1)` is the per-step relaxation move; `dr_constrained_numpy` applies the constraint sequence after each relaxation step.
- `SelfweightCalculator` is constructed once against the mesh (caching the face matrix), then called with the current `xyz` each iteration so selfweight tracks geometry change.

[LOCAL_ADMISSION]:
- form-finding pipeline: build `InputData.from_mesh(mesh, fixed, loads, qpre, ...)`, run `dr_numpy(indata)` or `dr_constrained_numpy(indata=..., constraints=[...])`, then `ResultData.update_mesh(mesh)`. Constraints attach by node index and are constructed polymorphically via `Constraint(geometry)`.
- COMPAS spine integration: `InputData` and every `Constraint` are `compas.data.Data` subclasses; the algebra owner serializes inputs/constraints through `compas.json_dumps` for graduation and decodes constraint geometry through `compas.json_loads` so `Constraint(decoded_geometry)` dispatches on the real decoded type. `InputData.from_mesh` and `ResultData.update_mesh` are the only `Mesh` seams — the solver never touches mesh topology directly.
- rpc-bridge integration: `graph/algebra.md#bridged` resolves `compas.rpc.Proxy.function("compas_dr.solvers.dr_numpy")` to run the scipy-backed solver out of process on the gated companion interpreter. The proxy is supplied by the `graph/algebra.md#ALGEBRA` `solver_proxy` async-resource scope, not constructed per call: the blocking surface crossing the runtime lane THREAD band is the whole proxy SCOPE — the eager `Proxy(...)` construction (which `_try_reconnect`s to the running localhost server or spawns one through a blocking `start_server()`) on scope entry plus each per-`function` solve — both bounded by the algebra owner's the runtime THREAD band, so a fan of dynamic-relaxation solves shares one reconnected worker process under one limiter. The cold-start bring-up transient (`compas.rpc.RPCServerError`/`RPCClientError`) retries under the runtime `reliability/resilience#RESILIENCE` `RetryClass.RPC` row on the scope-entry offload leg, sharing the one form-finding fault rail with `compas_tna` rather than a parallel async surface.

[RAIL_LAW]:
- Package: `compas-dr`
- Owns: dynamic-relaxation form-finding, geometric constraint projection by registered geometry type, tributary-area selfweight loads
- Accept: `InputData` from a `compas` `Mesh`, constraint sequences built via `Constraint(geometry)`, numpy position arrays
- Reject: hand-rolled dynamic-relaxation loops, hand-rolled RK integration, direct scipy sparse assembly outside this package, parallel `*Constraint` selection that bypasses the `Constraint(geometry)` registry factory

[CAPTURE_GAP]:
