# [PY_GEOMETRY_API_COMPAS_TNA]

`compas_tna` supplies thrust-network analysis (TNA) for masonry-vault form-finding on the COMPAS spine: the reciprocal `FormDiagram`/`ForceDiagram` dual datastructures with a rich `create_*` pattern-generator family, the numpy/scipy horizontal and vertical equilibrium solvers, the `parallelisation_numpy` sparse primitives, a callable `LoadUpdater` for selfweight, the `Envelope` masonry-bounds family (mesh/Brep/parametric-vault subclasses), and the `FormDiagramObject`/`ForceDiagramObject` scene nodes. It rides `compas` as its spine — `FormDiagram` extends `compas.datastructures.Mesh`, diagrams round-trip through the `json_dumps`/`json_loads` `Data` handle — and depends on `compas_fd` for the boundary-relaxation force-density solve. The `graph/algebra.md#ALGEBRA` owner selects TNA through the `FormEngine` sub-enum and offloads the numpy solvers out of process through `compas.rpc.Proxy.function("compas_tna.equilibrium.horizontal_numpy")`; it never re-implements the reciprocal-diagram algebra, the parallelisation step, or the vertical scale search.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas_tna`
- package: `compas_tna`
- import: `import compas_tna`
- owner: `geometry`
- rail: structural-form-finding
- installed: `0.7.0` reflected via `python -c "import compas_tna"` on the cp313 companion (`assay api resolve compas-tna` resolves no source on the cp315 `3.15.0b2` core — the documented companion-band gap, not a catalog fault)
- license: MIT
- wheel-floor: pure-Python `py3-none-any` wheel; the `_numpy` equilibrium solvers, `parallelisation_numpy`, and `LoadUpdater` pull `numpy`/`scipy` from the `compas` spine (no cp315 wheel) — ABI: none for compas_tna itself, scipy-family native ABI for the numpy rails
- pin: `compas_tna; python_version<'3.15'` (gated by the scipy-family CPython 3.15 lag, on the same marker as `compas`/`compas_dr`)
- depends: `compas` (spine) and `compas_fd` (force-density solver behind `relax_boundary_openings`); both ride the same gated band
- entry points: none (library only; `compas_tna.notebook.scene` is the COMPAS plugin hook, not a console script)
- capability: reciprocal form/force diagram construction with a parametric pattern-generator family, numpy horizontal/vertical equilibrium, sparse parallelisation primitives, tributary-area selfweight, masonry envelope bounds (mesh/Brep/parametric vaults), and Rhino/notebook scene rendering

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: diagram family — `compas_tna.diagrams`
- rail: form-force dual graph

`FormDiagram` extends the COMPAS `Mesh` and carries per-vertex load/reaction/residual/support and per-edge force-density (`q`) attributes. `ForceDiagram` is the reciprocal dual built by `ForceDiagram.from_formdiagram(form)`; the inherited `FormDiagram.dual_diagram(cls)` returns a generic dual mesh of the requested class, so the force diagram is constructed by the dedicated `from_formdiagram` factory, not by `dual_diagram` directly.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [CAPABILITY]                                                            |
| :-----: | :------------- | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `Diagram`      | base mesh graph | shared vertex/edge/face topology over the COMPAS `Mesh`                |
|  [02]   | `FormDiagram`  | form graph      | `Mesh` subclass; loads, supports, residuals, per-edge `q` force-density |
|  [03]   | `ForceDiagram` | force polygon   | reciprocal dual; `from_formdiagram(form)`, `ordered_edges(form)`, `uv_index(form=None)` |

[PUBLIC_TYPE_SCOPE]: envelope family — `compas_tna.envelope`
- rail: structural envelope geometry
- type-family: `compas.data.Data` subclass

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [CAPABILITY]                              |
| :-----: | :----------------------- | :--------------- | :---------------------------------------- |
|  [01]   | `Envelope`               | base envelope    | `rho`/`rho_fill` density bounds; `apply_*_to_formdiagram` and `compute_*` abstract surface |
|  [02]   | `MeshEnvelope`           | mesh-driven      | mesh-based target surface                 |
|  [03]   | `BrepEnvelope`           | Brep-driven      | Brep-based target surface                 |
|  [04]   | `ParametricEnvelope`     | parametric base  | generic parametric envelope               |
|  [05]   | `CrossVaultEnvelope`     | parametric vault | cross-vault intrados/extrados             |
|  [06]   | `DomeEnvelope`           | parametric vault | dome intrados/extrados                    |
|  [07]   | `PavillionVaultEnvelope` | parametric vault | pavillion-vault intrados/extrados         |
|  [08]   | `PointedVaultEnvelope`   | parametric vault | pointed-vault intrados/extrados           |

[PUBLIC_TYPE_SCOPE]: load, numdata, and scene family
- rail: load computation, numerical cache, scene rendering

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]       | [CAPABILITY]                                                              |
| :-----: | :-------------------- | :------------------ | :----------------------------------------------------------------------- |
|  [01]   | `LoadUpdater`         | callable load computer | `compas_tna.loads`; constructed against the form mesh, called as `updater(p, xyz)` to refresh selfweight in place |
|  [02]   | `FormDiagramNumData`  | numerical cache     | `compas_tna.numdata`; wraps a `FormDiagram` into cached numpy arrays (`xyz`/`p`/`q`/`C`/`Q`/`fixed`/`free`) plus `update_formdiagram()` writeback |
|  [03]   | `FormDiagramObject`   | scene node          | `compas_tna.scene`; Rhino/notebook scene object for a `FormDiagram`      |
|  [04]   | `ForceDiagramObject`  | scene node          | `compas_tna.scene`; Rhino/notebook scene object for a `ForceDiagram`     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: FormDiagram construction — `compas_tna.diagrams`
- rail: form-finding setup

The `create_*` classmethods build the pattern mesh through the `diagram_rectangular`/`diagram_circular`/`diagram_arch` mesh generators, then wrap it as a `FormDiagram`. `supports="corners"` is the default support strategy on the rectangular family.

| [INDEX] | [SURFACE]                                                                                                                                                  | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------- |
|  [01]   | `FormDiagram.create_cross(x_span=(0.0,10.0), y_span=(0.0,10.0), n=10, supports="corners")`                                                                  | factory        | cross-vault pattern diagram |
|  [02]   | `FormDiagram.create_cross_with_diagonal(x_span=(0.0,10.0), y_span=(0.0,10.0), n=10, supports="corners")`                                                    | factory        | cross + diagonal diagram    |
|  [03]   | `FormDiagram.create_fan(x_span=(0.0,10.0), y_span=(0.0,10.0), n_fans=10, n_hoops=10, supports="corners")`                                                   | factory        | fan pattern diagram         |
|  [04]   | `FormDiagram.create_parametric_fan(x_span=(0.0,10.0), y_span=(0.0,10.0), n=10, lambd=0.5, supports="corners")`                                              | factory        | parametric fan diagram      |
|  [05]   | `FormDiagram.create_ortho(x_span=(0.0,10.0), y_span=(0.0,10.0), nx=10, ny=10, supports="corners")`                                                          | factory        | orthogonal grid diagram     |
|  [06]   | `FormDiagram.create_circular_radial(center=(5.0,5.0), radius=5.0, n_hoops=8, n_parallels=20, r_oculus=0.0, diagonal=False, diagonal_type="split")`          | factory        | circular radial diagram     |
|  [07]   | `FormDiagram.create_circular_radial_spaced(center=(5.0,5.0), radius=5.0, n_hoops=8, n_parallels=20, r_oculus=0.0, diagonal=False, diagonal_type="split")`   | factory        | non-uniform radial diagram  |
|  [08]   | `FormDiagram.create_circular_spiral(center=(5.0,5.0), radius=5.0, n_hoops=8, n_parallels=20, r_oculus=0.0)`                                                 | factory        | spiral radial diagram       |
|  [09]   | `FormDiagram.create_arch(H=1.0, L=2.0, x0=0.0, n=100)`                                                                                                      | factory        | arch diagram                |
|  [10]   | `FormDiagram.create_arch_equally_spaced(L=2.0, x0=0.0, n=100)`                                                                                              | factory        | equally spaced arch diagram |
|  [11]   | `FormDiagram.from_mesh(mesh: compas.datastructures.Mesh) -> FormDiagram` / `FormDiagram.from_lines(...)`                                                    | converter      | from a COMPAS `Mesh` or line set |
|  [12]   | `ForceDiagram.from_formdiagram(form: FormDiagram) -> ForceDiagram`                                                                                          | factory        | build the reciprocal force diagram |

[ENTRYPOINT_SCOPE]: equilibrium solvers — `compas_tna.equilibrium`
- rail: horizontal + vertical equilibrium

`compas_tna.equilibrium.__init__` exports the pure `horizontal_nodal` always; under `not compas.IPY` it adds `horizontal_numpy`, `horizontal_nodal_numpy`, `relax_boundary_openings`, `vertical_from_q`, `vertical_from_zmax`. The `_numpy` variants are the production path. `alpha` is a 0..100 weighting (clamped to 0..1 internally): 100 fixes the form diagram, 0 fixes the force diagram. The solvers return their inputs `(form, force)` for RPC compatibility. `relax_boundary_openings` delegates to `compas_fd.fd_numpy`.

| [INDEX] | [SURFACE]                                                                                            | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `horizontal_numpy(form, force, alpha=100.0, kmax=100) -> tuple[FormDiagram, ForceDiagram]`            | solver         | numpy horizontal equilibrium |
|  [02]   | `horizontal_nodal_numpy(form, force, alpha=100, kmax=100) -> tuple[FormDiagram, ForceDiagram]`        | solver         | nodal horizontal equilibrium |
|  [03]   | `horizontal_nodal(form, force, ...)`                                                                  | solver         | pure-Python nodal fallback (always exported) |
|  [04]   | `vertical_from_q(form, scale=1.0, density=1.0, kmax=100, tol=1e-3, display=False)`                     | solver         | vertical from force density  |
|  [05]   | `vertical_from_zmax(form, zmax, kmax=100, xtol=1e-2, rtol=1e-3, density=1.0, display=False) -> tuple[FormDiagram, float]` | solver | vertical scale search for a target crown height |
|  [06]   | `relax_boundary_openings(form, fixed: list[int]) -> FormDiagram`                                      | relaxation     | inward-curving boundary relaxation via `compas_fd.fd_numpy` |

[ENTRYPOINT_SCOPE]: parallelisation primitives — `compas_tna.equilibrium.parallelisation_numpy`
- rail: sparse linear algebra

These are the sparse primitives the horizontal solvers call; they are not re-exported from `compas_tna.equilibrium`, so import them from the `parallelisation_numpy` module directly.

| [INDEX] | [SURFACE]                                                                                            | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `parallelise(A, x, b, known: list[int]) -> ndarray`                                                   | numeric        | least-squares parallelisation step with known DOFs |
|  [02]   | `parallelise_nodal(xy, C, targets, i_nbrs, ij_e, fixed=None, kmax=100, lmin=None, lmax=None)`         | numeric        | node-per-node parallelisation step |
|  [03]   | `parallelise_sparse(A, B, X, known, k=1, key=None)`                                                   | numeric        | sparse-solve parallelisation step |
|  [04]   | `apply_bounds(x, xmin, xmax)`                                                                          | numeric        | clamp coordinates to length bounds |

[ENTRYPOINT_SCOPE]: load computation — `compas_tna.loads`
- rail: load application

`LoadUpdater` constructs a face matrix once, then its `__call__(p, xyz)` updates `p[:, 2]` in place from `tributary_area * thickness * density + tributary_area * live`. `thickness` is a per-vertex array or a constant.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `LoadUpdater(mesh, p0, thickness=1.0, density=1.0, live=0.0)`        | constructor    | tributary load computer bound to the form mesh and fixed loads `p0` (Nx3) |
|  [02]   | `LoadUpdater.__call__(p, xyz) -> None`                              | callable       | refresh `p[:,2]` selfweight in place for the current `xyz` |
|  [03]   | `LoadUpdater.tributary_areas(xyz) -> ndarray (N,1)`                  | method         | per-vertex tributary area for the current coordinates |
|  [04]   | `LoadUpdater.face_matrix() -> scipy.sparse.csr_matrix`              | method         | normalized face-vertex centroid matrix |

[ENTRYPOINT_SCOPE]: envelope operations — `compas_tna.envelope`
- rail: structural bounds application

The `apply_*_to_formdiagram(formdiagram)` methods push the envelope's geometry onto the form diagram's vertex attributes; the `compute_bounds(x, y)`-family take coordinate arrays and return intrados/extrados arrays. Base methods raise `NotImplementedError`; subclasses override per vault geometry.

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :--------------------------------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `Envelope(rho=20.0, rho_fill=14.0, is_parametric=False)`                            | constructor    | material density bounds                 |
|  [02]   | `Envelope.apply_selfweight_to_formdiagram(formdiagram)`                             | method         | apply selfweight load to vertices       |
|  [03]   | `Envelope.apply_fill_weight_to_formdiagram(formdiagram)`                            | method         | apply fill-weight load                  |
|  [04]   | `Envelope.apply_bounds_to_formdiagram(formdiagram)` / `apply_target_heights_to_formdiagram(formdiagram)` / `apply_reaction_bounds_to_formdiagram(formdiagram)` | method | apply height/target/reaction bounds |
|  [05]   | `Envelope.compute_bounds(x, y) -> tuple[ndarray, ndarray]`                          | method         | upper/lower bound surfaces at `(x,y)`   |
|  [06]   | `Envelope.compute_middle(x, y) -> ndarray` / `compute_bounds_derivatives(x, y)`     | method         | mid-surface and bound gradients         |
|  [07]   | `Envelope.compute_area() -> float` / `compute_volume() -> float` / `compute_selfweight() -> float` | method | total area/volume/selfweight (cached as the `area`/`volume`/`selfweight` properties) |

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
- rpc-bridge integration: `graph/algebra.md#bridged` resolves `compas.rpc.Proxy.function("compas_tna.equilibrium.horizontal_numpy")` to run the scipy-backed solvers out of process; the `_numpy` solvers return `(form, force)` precisely for this RPC round-trip. The proxy is supplied by the `graph/algebra.md#ALGEBRA` `solver_proxy` async-resource scope, not constructed per call: the blocking surface crossing `anyio.to_thread.run_sync` is the whole proxy SCOPE — the eager `Proxy(...)` construction (which `_try_reconnect`s to the running localhost server or spawns one through a blocking `start_server()`) on scope entry plus each per-`function` solve — both bounded by the algebra owner's `_SOLVER_LIMITER`, so a fan of equilibrium solves shares one reconnected worker process under one limiter. The cold-start bring-up transient (`compas.rpc.RPCServerError`/`RPCClientError`) retries under the runtime `reliability/resilience#RESILIENCE` `RetryClass.RPC` row on the scope-entry offload leg, sharing the one form-finding fault rail with `compas_dr`.

[RAIL_LAW]:
- Package: `compas_tna`
- Owns: TNA form-finding, reciprocal form/force diagram construction, horizontal/vertical equilibrium, sparse parallelisation, masonry envelope bounds
- Accept: `compas` `Mesh`/`FormDiagram`/`ForceDiagram` inputs; numpy arrays for the numeric rails
- Reject: hand-rolled TNA solvers or duplicate equilibrium implementations, hand-rolled reciprocal-diagram construction that bypasses `ForceDiagram.from_formdiagram`, direct sparse assembly outside `parallelisation_numpy`

[CAPTURE_GAP]:
- floor: `compas_tna 0.7.0` ships a pure-Python `py3-none-any` wheel under MIT; it rides the `compas` companion band on the `python_version<'3.15'` marker (scipy-family CPython 3.15 lag) and pulls `compas_fd` for the `relax_boundary_openings` force-density solve. Reflection ran by reading the installed cp313 companion distribution; `assay api resolve compas-tna` resolves no source on the cp315 core.
- members: verified against the installed cp313 sources — the `create_*` factory signatures, the equilibrium solver signatures and export gating, the `parallelisation_numpy` primitives, the callable `LoadUpdater(p, xyz)` shape, the `Envelope.compute_bounds(x, y)` arity, and the `FormDiagramObject`/`ForceDiagramObject` scene-class spellings resolve against the live modules. The scene classes are `FormDiagramObject`/`ForceDiagramObject` (not `FormObject`/`ForceObject`) — no phantom.
