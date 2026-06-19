# [PY_GEOMETRY_API_COMPAS_DR]

`compas_dr` supplies dynamic-relaxation form-finding solvers through `dr` (pure-Python), `dr_numpy` (numpy/scipy vectorized), and `dr_constrained_numpy` (constrained vectorized) entry points, `InputData` / `ResultData` typed carriers, `Constraint` subclasses (`CircleConstraint`, `LineConstraint`, `PlaneConstraint`, `CurveConstraint`, `SurfaceConstraint`), and `SelfweightCalculator` for tributary-area load distribution in the geometry form-finding rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas_dr`
- package: `compas-dr`
- module: `compas_dr`
- asset: pure-Python wheel, cp313 available
- rail: form-finding

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver data carriers
- rail: form-finding
- type-family: COMPAS data record (serializable via `from_json`/`to_json`)

| [INDEX] | [SYMBOL]     | [CAPABILITY]                                                           |
| :-----: | :----------- | :--------------------------------------------------------------------- |
|  [01]   | `InputData`  | vertices, edges, fixed, loads, qpre/fpre/lpre, E, radius; serializable |
|  [02]   | `ResultData` | solver output fields; `update_mesh(mesh)` writes back to compas Mesh   |

[PUBLIC_TYPE_SCOPE]: constraint family
- rail: form-finding

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                          |
| :-----: | :------------------ | :----------------- | :-------------------------------------------------------------------- |
|  [01]   | `Constraint`        | base constraint    | `project()`, `update()`, `get_constraint_cls(geometry)`, serializable |
|  [02]   | `CircleConstraint`  | circle constraint  | project node onto circle; `update(damping=0.1)`                       |
|  [03]   | `LineConstraint`    | line constraint    | project node onto line; `update(damping=0.1)`                         |
|  [04]   | `PlaneConstraint`   | plane constraint   | project node onto plane; `update(damping=0.1)`                        |
|  [05]   | `CurveConstraint`   | curve constraint   | project node onto curve; `update(damping=0.1)`                        |
|  [06]   | `SurfaceConstraint` | surface constraint | project node onto surface; `update(damping=0.1)`                      |

[PUBLIC_TYPE_SCOPE]: load utilities
- rail: form-finding

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                                            |
| :-----: | :--------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `SelfweightCalculator` | load calculator | `compute_tributary_areas(xyz)`, `compute_face_matrix()` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solvers
- rail: form-finding

| [INDEX] | [SURFACE]                                                                                                                                                                                                      | [ENTRY_FAMILY]           | [RAIL]                                   |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------------- | :--------------------------------------- |
|  [01]   | `dr(vertices, edges, fixed, loads, qpre, fpre=None, lpre=None, linit=None, E=None, radius=None, kmax=100, dt=1.0, tol1=0.001, tol2=1e-6, c=0.1, callback=None, callback_args=None)`                            | pure-Python solver       | list-based dynamic relaxation            |
|  [02]   | `dr_numpy(indata: InputData, kmax=10000, dt=1.0, tol1=0.001, tol2=1e-6, c=0.1, rk_steps: Literal[1,2,4]=2, callback=None, callback_args=None) -> ResultData`                                                   | numpy solver             | vectorized dynamic relaxation            |
|  [03]   | `dr_constrained_numpy(*, indata: InputData, constraints: Sequence[Constraint], kmax=10000, dt=1.0, tol1=0.001, tol2=1e-6, c=0.1, rk_steps: Literal[1,2,4]=2, callback=None, callback_args=None) -> ResultData` | constrained numpy solver | vectorized DR with geometric constraints |

[ENTRYPOINT_SCOPE]: InputData construction and serialization
- rail: form-finding

| [INDEX] | [SURFACE]                                                                                                           | [ENTRY_FAMILY]  | [RAIL]                       |
| :-----: | :------------------------------------------------------------------------------------------------------------------ | :-------------- | :--------------------------- |
|  [01]   | `InputData.from_mesh(mesh, fixed, loads, qpre, fpre=None, lpre=None, linit=None, E=None, radius=None) -> InputData` | construction    | build from compas Mesh       |
|  [02]   | `InputData.from_json(filepath) -> InputData`                                                                        | deserialization | load from COMPAS JSON file   |
|  [03]   | `InputData.from_jsonstring(string) -> InputData`                                                                    | deserialization | load from COMPAS JSON string |
|  [04]   | `InputData.to_json(filepath, pretty=False, compact=False, minimal=False)`                                           | serialization   | write to COMPAS JSON file    |
|  [05]   | `InputData.copy(cls=None, copy_guid=False) -> InputData`                                                            | copy            | deep copy                    |

[ENTRYPOINT_SCOPE]: ResultData fields and mesh writeback
- rail: form-finding

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY]  | [RAIL]                                     |
| :-----: | :--------------------------------------------------------- | :-------------- | :----------------------------------------- |
|  [01]   | `ResultData.update_mesh(mesh, vertex_index=None)`          | writeback       | apply solved positions back to compas Mesh |
|  [02]   | `ResultData.from_json(filepath) -> ResultData`             | deserialization | load result from COMPAS JSON file          |
|  [03]   | `ResultData.copy(cls=None, copy_guid=False) -> ResultData` | copy            | deep copy of result data                   |

[ENTRYPOINT_SCOPE]: Constraint operations
- rail: form-finding

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY]  | [RAIL]                                         |
| :-----: | :---------------------------------------------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | `Constraint.get_constraint_cls(geometry, **kwargs) -> type` | factory         | resolve Constraint subclass from geometry type |
|  [02]   | `constraint.project()`                                      | constraint      | project constrained node to geometry           |
|  [03]   | `constraint.update(damping=0.1)`                            | constraint      | apply damped constraint update                 |
|  [04]   | `constraint.compute_param()`                                | geometry        | compute parameter on constraint geometry       |
|  [05]   | `constraint.compute_normal()`                               | geometry        | compute normal at constraint location          |
|  [06]   | `constraint.compute_tangent()`                              | geometry        | compute tangent at constraint location         |
|  [07]   | `constraint.from_json(filepath) -> Constraint`              | deserialization | load from COMPAS JSON                          |
|  [08]   | `constraint.to_json(filepath, ...)`                         | serialization   | write to COMPAS JSON                           |

[ENTRYPOINT_SCOPE]: SelfweightCalculator
- rail: form-finding
- entry-family: load computation

| [INDEX] | [SURFACE]                                                       | [RAIL]                                       |
| :-----: | :-------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `SelfweightCalculator.compute_tributary_areas(xyz) -> FloatNx1` | tributary area per vertex from positions     |
|  [02]   | `SelfweightCalculator.compute_face_matrix() -> csr_matrix`      | sparse face-vertex matrix for area weighting |

## [04]-[IMPLEMENTATION_LAW]

[DR_TOPOLOGY]:
- `dr` is a list-based reference implementation; `dr_numpy` and `dr_constrained_numpy` are the production vectorized paths accepting `InputData`.
- `rk_steps` in `dr_numpy`/`dr_constrained_numpy` selects Runge-Kutta order: `1` = explicit Euler, `2` = RK2, `4` = RK4.
- `InputData` fields: `vertices` (Nx3 float positions), `edges` (Ex2 int pairs), `fixed` (set of fixed node indices), `loads` (Nx3 load vectors), `qpre`/`fpre`/`lpre`/`linit` (prestress/force/length controls), `E`/`radius` (material properties for axial stiffness).
- `ResultData` carries the converged vertex positions, reaction forces, and residuals; `update_mesh` writes solved XYZ back to a `compas.datastructures.Mesh`.
- `Constraint.get_constraint_cls(geometry)` dispatches on the `compas.geometry` type of `geometry` to return the matching `*Constraint` subclass.
- All constraint and data types use COMPAS JSON serialization (`from_json`/`to_json`/`from_jsonstring`).

[LOCAL_ADMISSION]:
- Form-finding pipelines build `InputData` from a `compas.datastructures.Mesh` via `InputData.from_mesh`, then call `dr_numpy` or `dr_constrained_numpy`, then write results back via `ResultData.update_mesh`.
- Constraints attach to nodes by index; `dr_constrained_numpy` applies them after each relaxation step with the given damping.
- `SelfweightCalculator` computes tributary vertex areas from the current mesh geometry; the result is a load vector for gravity loads.

[RAIL_LAW]:
- Package: `compas-dr`
- Owns: dynamic-relaxation form-finding, geometric constraint projection, tributary-area load distribution
- Accept: `InputData` from `Mesh`, constraint sequences, numpy position arrays
- Reject: hand-rolled dynamic-relaxation loops, direct scipy sparse assembly outside this package
