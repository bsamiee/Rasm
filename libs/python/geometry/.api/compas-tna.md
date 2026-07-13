# [PY_GEOMETRY_API_COMPAS_TNA]

`compas_tna` supplies thrust-network analysis (TNA) for masonry-vault form-finding on the COMPAS spine: the reciprocal `FormDiagram`/`ForceDiagram` dual datastructures with a rich `create_*` pattern-generator family, the numpy/scipy horizontal and vertical equilibrium solvers, the `parallelisation_numpy` sparse primitives, a callable `LoadUpdater` for selfweight, the `Envelope` masonry-bounds family (mesh/Brep/parametric-vault subclasses), and the `FormDiagramObject`/`ForceDiagramObject` scene nodes. It rides `compas` as its spine — `FormDiagram` extends `compas.datastructures.Mesh`, diagrams round-trip through the `json_dumps`/`json_loads` `Data` handle — and depends on `compas_fd` for the boundary-relaxation force-density solve. The `graph/algebra.md#ALGEBRA` owner selects TNA through the `FormEngine` sub-enum and offloads the numpy solvers out of process through `compas.rpc.Proxy.function("compas_tna.equilibrium.horizontal_numpy")`; it never re-implements the reciprocal-diagram algebra, the parallelisation step, or the vertical scale search.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas_tna`
- package: `compas_tna`
- import: `import compas_tna`
- owner: `geometry`
- rail: structural-form-finding
- installed: `0.7.0`
- license: MIT
- entry points: none (library only; `compas_tna.notebook.scene` is the COMPAS plugin hook, not a console script)
- capability: reciprocal form/force diagram construction with a parametric pattern-generator family, numpy horizontal/vertical equilibrium, sparse parallelisation primitives, tributary-area selfweight, masonry envelope bounds (mesh/Brep/parametric vaults), and Rhino/notebook scene rendering

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: diagram family — `compas_tna.diagrams`
- rail: form-force dual graph

`FormDiagram` extends the COMPAS `Mesh` and carries per-vertex load/reaction/residual/support and per-edge force-density (`q`) attributes. `ForceDiagram` is the reciprocal dual built by `ForceDiagram.from_formdiagram(form)`; the inherited `FormDiagram.dual_diagram(cls)` returns a generic dual mesh of the requested class, so the force diagram is constructed by the dedicated `from_formdiagram` factory, not by `dual_diagram` directly.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [CAPABILITY]                                                                            |
| :-----: | :------------- | :-------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Diagram`      | base mesh graph | shared vertex/edge/face topology over the COMPAS `Mesh`                                 |
|  [02]   | `FormDiagram`  | form graph      | `Mesh` subclass; loads, supports, residuals, per-edge `q` force-density                 |
|  [03]   | `ForceDiagram` | force polygon   | reciprocal dual; `from_formdiagram(form)`, `ordered_edges(form)`, `uv_index(form=None)` |

[PUBLIC_TYPE_SCOPE]: envelope family — `compas_tna.envelope`
- rail: structural envelope geometry
- type-family: `compas.data.Data` subclass

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [CAPABILITY]                                                                               |
| :-----: | :----------------------- | :--------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `Envelope`               | base envelope    | `rho`/`rho_fill` density bounds; `apply_*_to_formdiagram` and `compute_*` abstract surface |
|  [02]   | `MeshEnvelope`           | mesh-driven      | mesh-based target surface                                                                  |
|  [03]   | `BrepEnvelope`           | Brep-driven      | Brep-based target surface                                                                  |
|  [04]   | `ParametricEnvelope`     | parametric base  | generic parametric envelope                                                                |
|  [05]   | `CrossVaultEnvelope`     | parametric vault | cross-vault intrados/extrados                                                              |
|  [06]   | `DomeEnvelope`           | parametric vault | dome intrados/extrados                                                                     |
|  [07]   | `PavillionVaultEnvelope` | parametric vault | pavillion-vault intrados/extrados                                                          |
|  [08]   | `PointedVaultEnvelope`   | parametric vault | pointed-vault intrados/extrados                                                            |

[PUBLIC_TYPE_SCOPE]: load, numdata, and scene family
- rail: load computation, numerical cache, scene rendering

`LoadUpdater` lives in `compas_tna.loads`, `FormDiagramNumData` in `compas_tna.numdata`, and the two scene objects in `compas_tna.scene`.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]          | [CAPABILITY]                                                                      |
| :-----: | :------------------- | :--------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `LoadUpdater`        | callable load computer | bound to the form mesh; `updater(p, xyz)` refreshes selfweight in place           |
|  [02]   | `FormDiagramNumData` | numerical cache        | caches `xyz`/`p`/`q`/`C`/`Q`/`fixed`/`free` numpy arrays + `update_formdiagram()` |
|  [03]   | `FormDiagramObject`  | scene node             | Rhino/notebook scene object for a `FormDiagram`                                   |
|  [04]   | `ForceDiagramObject` | scene node             | Rhino/notebook scene object for a `ForceDiagram`                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: FormDiagram construction — `compas_tna.diagrams`
- rail: form-finding setup

The `create_*` classmethods (the `FormDiagram.` prefix elided below) build the pattern mesh through the `diagram_rectangular`/`diagram_circular`/`diagram_arch` generators, then wrap it as a `FormDiagram`. The rectangular family shares `x_span=(0.0,10.0), y_span=(0.0,10.0), supports="corners"`; the circular family shares `center=(5.0,5.0), radius=5.0, n_hoops=8, n_parallels=20, r_oculus=0.0`.

| [INDEX] | [SURFACE]                                                              | [PATTERN]                      |
| :-----: | :--------------------------------------------------------------------- | :----------------------------- |
|  [01]   | `create_cross(n=10)`                                                   | cross-vault                    |
|  [02]   | `create_cross_with_diagonal(n=10)`                                     | cross + diagonal               |
|  [03]   | `create_fan(n_fans=10, n_hoops=10)`                                    | fan                            |
|  [04]   | `create_parametric_fan(n=10, lambd=0.5)`                               | parametric fan                 |
|  [05]   | `create_ortho(nx=10, ny=10)`                                           | orthogonal grid                |
|  [06]   | `create_circular_radial(diagonal=False, diagonal_type="split")`        | circular radial                |
|  [07]   | `create_circular_radial_spaced(diagonal=False, diagonal_type="split")` | non-uniform radial             |
|  [08]   | `create_circular_spiral()`                                             | spiral radial                  |
|  [09]   | `create_arch(H=1.0, L=2.0, x0=0.0, n=100)`                             | arch                           |
|  [10]   | `create_arch_equally_spaced(L=2.0, x0=0.0, n=100)`                     | equally spaced arch            |
|  [11]   | `from_mesh(mesh) -> FormDiagram` / `from_lines(...)`                   | converter from `Mesh` or lines |
|  [12]   | `ForceDiagram.from_formdiagram(form) -> ForceDiagram`                  | reciprocal force diagram       |

[ENTRYPOINT_SCOPE]: equilibrium solvers — `compas_tna.equilibrium`
- rail: horizontal + vertical equilibrium

`compas_tna.equilibrium.__init__` exports the pure `horizontal_nodal` always; under `not compas.IPY` it adds `horizontal_numpy`, `horizontal_nodal_numpy`, `relax_boundary_openings`, `vertical_from_q`, `vertical_from_zmax`. The `_numpy` variants are the production path. `alpha` is a 0..100 weighting (clamped to 0..1 internally): 100 fixes the form diagram, 0 fixes the force diagram. The horizontal pair returns `tuple[FormDiagram, ForceDiagram]` (returning its inputs for RPC compatibility) and shares `alpha, kmax=100`; the vertical solvers share `kmax=100, density=1.0, display=False`. `relax_boundary_openings` delegates to `compas_fd.fd_numpy`.

| [INDEX] | [SURFACE]                                                                           | [RAIL]                                        |
| :-----: | :---------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `horizontal_numpy(form, force, alpha=100.0, kmax=100)`                              | numpy horizontal equilibrium                  |
|  [02]   | `horizontal_nodal_numpy(form, force, alpha=100, kmax=100)`                          | nodal horizontal equilibrium                  |
|  [03]   | `horizontal_nodal(form, force, ...)`                                                | pure-Python nodal fallback (always exported)  |
|  [04]   | `vertical_from_q(form, scale=1.0, tol=1e-3)`                                        | vertical from force density                   |
|  [05]   | `vertical_from_zmax(form, zmax, xtol=1e-2, rtol=1e-3) -> tuple[FormDiagram, float]` | vertical scale search for target crown height |
|  [06]   | `relax_boundary_openings(form, fixed: list[int]) -> FormDiagram`                    | boundary relaxation via `compas_fd.fd_numpy`  |

[ENTRYPOINT_SCOPE]: parallelisation primitives — `compas_tna.equilibrium.parallelisation_numpy`
- rail: sparse linear algebra

These are the sparse primitives the horizontal solvers call; they are not re-exported from `compas_tna.equilibrium`, so import them from the `parallelisation_numpy` module directly.

| [INDEX] | [SURFACE]                                                                                     | [RAIL]                             |
| :-----: | :-------------------------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `parallelise(A, x, b, known: list[int]) -> ndarray`                                           | least-squares step with known DOFs |
|  [02]   | `parallelise_nodal(xy, C, targets, i_nbrs, ij_e, fixed=None, kmax=100, lmin=None, lmax=None)` | node-per-node step                 |
|  [03]   | `parallelise_sparse(A, B, X, known, k=1, key=None)`                                           | sparse-solve step                  |
|  [04]   | `apply_bounds(x, xmin, xmax)`                                                                 | clamp coordinates to length bounds |

[ENTRYPOINT_SCOPE]: load computation — `compas_tna.loads`
- rail: load application

`LoadUpdater` constructs a face matrix once, then its `__call__(p, xyz)` updates `p[:, 2]` in place from `tributary_area * thickness * density + tributary_area * live`. `thickness` is a per-vertex array or a constant.

| [INDEX] | [SURFACE]                                                     | [RAIL]                                                           |
| :-----: | :------------------------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | `LoadUpdater(mesh, p0, thickness=1.0, density=1.0, live=0.0)` | tributary load computer bound to the form mesh, loads `p0` (Nx3) |
|  [02]   | `LoadUpdater.__call__(p, xyz) -> None`                        | refresh `p[:,2]` selfweight in place for the current `xyz`       |
|  [03]   | `LoadUpdater.tributary_areas(xyz) -> ndarray (N,1)`           | per-vertex tributary area for the current coordinates            |
|  [04]   | `LoadUpdater.face_matrix() -> scipy.sparse.csr_matrix`        | normalized face-vertex centroid matrix                           |

[ENTRYPOINT_SCOPE]: envelope operations — `compas_tna.envelope`
- rail: structural bounds application

The `apply_*_to_formdiagram(formdiagram)` methods push the envelope's geometry onto the form diagram's vertex attributes; the `compute_bounds(x, y)`-family take coordinate arrays and return intrados/extrados arrays. Base methods raise `NotImplementedError`; subclasses override per vault geometry.

| [INDEX] | [SURFACE]                                                    | [RAIL]                                          |
| :-----: | :----------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `Envelope(rho=20.0, rho_fill=14.0, is_parametric=False)`     | material density bounds                         |
|  [02]   | `Envelope.apply_selfweight_to_formdiagram(formdiagram)`      | apply selfweight load to vertices               |
|  [03]   | `Envelope.apply_fill_weight_to_formdiagram(formdiagram)`     | apply fill-weight load                          |
|  [04]   | `Envelope.apply_bounds_to_formdiagram(formdiagram)`          | apply height bounds                             |
|  [05]   | `Envelope.apply_target_heights_to_formdiagram(formdiagram)`  | apply target heights                            |
|  [06]   | `Envelope.apply_reaction_bounds_to_formdiagram(formdiagram)` | apply reaction bounds                           |
|  [07]   | `Envelope.compute_bounds(x, y) -> tuple[ndarray, ndarray]`   | upper/lower bound surfaces at `(x,y)`           |
|  [08]   | `Envelope.compute_middle(x, y) -> ndarray`                   | mid-surface at `(x,y)`                          |
|  [09]   | `Envelope.compute_bounds_derivatives(x, y)`                  | bound gradients                                 |
|  [10]   | `Envelope.compute_area() -> float`                           | total area (cached `area` property)             |
|  [11]   | `Envelope.compute_volume() -> float`                         | total volume (cached `volume` property)         |
|  [12]   | `Envelope.compute_selfweight() -> float`                     | total selfweight (cached `selfweight` property) |

## [04]-[IMPLEMENTATION_LAW]

[TNA_TOPOLOGY]:
- subpackages: `diagrams`, `equilibrium`, `envelope`, `loads`, `scene`, `rhino`, `notebook`, plus the `numdata` module (`FormDiagramNumData`).
- `FormDiagram` extends the COMPAS `Mesh`; per-vertex `px`/`py`/`pz` loads, `_rx`/`_ry`/`_rz` reactions, support flags, and per-edge `q` force-density and `_is_edge`/`lmin`/`lmax`/`hmin`/`hmax` bounds drive the solvers.
- `ForceDiagram` is the reciprocal dual built by `ForceDiagram.from_formdiagram(form)`; the inherited `FormDiagram.dual_diagram(cls)` constructs a generic dual mesh of the passed class and is the lower-level primitive `from_formdiagram` rides.
- equilibrium splits Python and numpy variants: `horizontal_nodal` is the always-available pure path; the `_numpy` solvers and `relax_boundary_openings` load only outside IronPython (`not compas.IPY`). `parallelisation_numpy` owns the sparse `parallelise`/`parallelise_nodal`/`parallelise_sparse` primitives the horizontal solvers call.
- `vertical_from_q` applies the force-density matrix directly at a fixed `scale`; `vertical_from_zmax` binary-searches `scale` (via `xtol`/`rtol`) so the crown reaches `zmax`, internally driving a `LoadUpdater`.

[LOCAL_ADMISSION]:
- TNA pipeline: `FormDiagram.create_*` or `from_mesh` -> `relax_boundary_openings(form, fixed)` -> `Envelope.apply_selfweight_to_formdiagram(form)` (or `LoadUpdater`) -> `ForceDiagram.from_formdiagram(form)` -> `horizontal_numpy(form, force)` -> `vertical_from_zmax(form, zmax)`.
- COMPAS spine integration: diagrams and envelopes are `compas` `Data`/`Mesh` subclasses; the algebra owner serializes them through `compas.json_dumps` for graduation and reads `vertices_attributes("xy"/"xyz")` for the numeric rails. `LoadUpdater` requires a `Mesh` and a numpy `(n,3)` fixed-load array `p0`, mutating the passed `p` in place.
- rpc-bridge integration: `graph/algebra.md#bridged` resolves `compas.rpc.Proxy.function("compas_tna.equilibrium.horizontal_numpy")` to run the scipy-backed solvers out of process; the `_numpy` solvers return `(form, force)` precisely for this RPC round-trip. The proxy is supplied by the `graph/algebra.md#ALGEBRA` `solver_proxy` async-resource scope, not constructed per call: the blocking surface crossing the runtime lane THREAD band is the whole proxy SCOPE — the eager `Proxy(...)` construction (which `_try_reconnect`s to the running localhost server or spawns one through a blocking `start_server()`) on scope entry plus each per-`function` solve — both bounded by the algebra owner's the runtime THREAD band, so a fan of equilibrium solves shares one reconnected worker process under one limiter. The cold-start bring-up transient (`compas.rpc.RPCServerError`/`RPCClientError`) retries under the runtime `reliability/resilience#RESILIENCE` `RetryClass.RPC` row on the scope-entry offload leg, sharing the one form-finding fault rail with `compas_dr`.

[RAIL_LAW]:
- Package: `compas_tna`
- Owns: TNA form-finding, reciprocal form/force diagram construction, horizontal/vertical equilibrium, sparse parallelisation, masonry envelope bounds
- Accept: `compas` `Mesh`/`FormDiagram`/`ForceDiagram` inputs; numpy arrays for the numeric rails
- Reject: hand-rolled TNA solvers or duplicate equilibrium implementations, hand-rolled reciprocal-diagram construction that bypasses `ForceDiagram.from_formdiagram`, direct sparse assembly outside `parallelisation_numpy`

[CAPTURE_GAP]:
