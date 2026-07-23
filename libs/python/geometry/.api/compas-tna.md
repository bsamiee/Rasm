# [PY_GEOMETRY_API_COMPAS_TNA]

`compas_tna` owns thrust-network analysis for masonry-vault form-finding on the COMPAS spine: reciprocal form/force equilibrium and masonry envelope bounds over `compas` datastructures. `FormDiagram` extends `compas.datastructures.Mesh` and every diagram and envelope is a `compas.data.Data` subclass, so the algebra owner graduates evidence through one `compas.json_dumps` handle and offloads the `_numpy` solvers through `compas.rpc.Proxy`. Boundary relaxation delegates to `compas_fd.fd_numpy`; the reciprocal-diagram algebra, parallelisation, and vertical scale search live here alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas_tna`
- package: `compas_tna` (`MIT`)
- import: `import compas_tna`
- owner: `geometry`
- rail: structural-form-finding
- entry points: none (library only; `compas_tna.notebook.scene` is the COMPAS plugin hook, not a console script)
- capability: reciprocal form/force diagram construction, numpy horizontal/vertical equilibrium, sparse parallelisation, tributary-area selfweight, masonry envelope bounds, and Rhino/notebook scene rendering

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: diagram family — `compas_tna.diagrams`

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [CAPABILITY]                                                                            |
| :-----: | :------------- | :-------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Diagram`      | base mesh graph | shared vertex/edge/face topology over the COMPAS `Mesh`                                 |
|  [02]   | `FormDiagram`  | form graph      | `Mesh` subclass; loads, supports, residuals, per-edge `q` force-density                 |
|  [03]   | `ForceDiagram` | force polygon   | reciprocal dual; `from_formdiagram(form)`, `ordered_edges(form)`, `uv_index(form=None)` |

[PUBLIC_TYPE_SCOPE]: envelope family — `compas_tna.envelope`

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

[PUBLIC_TYPE_SCOPE]: load, numerical cache, scene — `compas_tna.{loads,numdata,scene}`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]          | [CAPABILITY]                                                                      |
| :-----: | :------------------- | :--------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `LoadUpdater`        | callable load computer | bound to the form mesh; `updater(p, xyz)` refreshes selfweight in place           |
|  [02]   | `FormDiagramNumData` | numerical cache        | caches `xyz`/`p`/`q`/`C`/`Q`/`fixed`/`free` numpy arrays + `update_formdiagram()` |
|  [03]   | `FormDiagramObject`  | scene node             | Rhino/notebook scene object for a `FormDiagram`                                   |
|  [04]   | `ForceDiagramObject` | scene node             | Rhino/notebook scene object for a `ForceDiagram`                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: FormDiagram construction — `compas_tna.diagrams`

`create_*` classmethods (the `FormDiagram.` prefix elided) build the pattern mesh through the `diagram_rectangular`/`diagram_circular`/`diagram_arch` generators, then wrap it as a `FormDiagram`; the rectangular family carries `x_span=(0.0,10.0), y_span=(0.0,10.0), supports="corners"` and the circular family `center=(5.0,5.0), radius=5.0, n_hoops=8, n_parallels=20, r_oculus=0.0`. Every row is a factory.

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]             |
| :-----: | :--------------------------------------------------------------------- | :----------------------- |
|  [01]   | `create_cross(n=10)`                                                   | cross-vault              |
|  [02]   | `create_cross_with_diagonal(n=10)`                                     | cross + diagonal         |
|  [03]   | `create_fan(n_fans=10, n_hoops=10)`                                    | fan                      |
|  [04]   | `create_parametric_fan(n=10, lambd=0.5)`                               | parametric fan           |
|  [05]   | `create_ortho(nx=10, ny=10)`                                           | orthogonal grid          |
|  [06]   | `create_circular_radial(diagonal=False, diagonal_type="split")`        | circular radial          |
|  [07]   | `create_circular_radial_spaced(diagonal=False, diagonal_type="split")` | non-uniform radial       |
|  [08]   | `create_circular_spiral()`                                             | spiral radial            |
|  [09]   | `create_arch(H=1.0, L=2.0, x0=0.0, n=100)`                             | arch                     |
|  [10]   | `create_arch_equally_spaced(L=2.0, x0=0.0, n=100)`                     | equally spaced arch      |
|  [11]   | `from_mesh(mesh) -> FormDiagram`                                       | converter from `Mesh`    |
|  [12]   | `from_lines(lines, *, delete_boundary_face=True, precision=None)`      | converter from lines     |
|  [13]   | `ForceDiagram.from_formdiagram(form) -> ForceDiagram`                  | reciprocal force diagram |

[ENTRYPOINT_SCOPE]: equilibrium solvers — `compas_tna.equilibrium`

Horizontal solvers carry `alpha, kmax=100` and return `tuple[FormDiagram, ForceDiagram]` (returning their inputs for the RPC round-trip); vertical solvers carry `kmax=100, density=1.0, display=False`. `alpha` is a 0..100 weighting clamped to 0..1 internally: 100 fixes the form diagram, 0 the force diagram. `_numpy` variants are the production path; `horizontal_nodal` is the always-available pure-Python fallback and `relax_boundary_openings` delegates to `compas_fd.fd_numpy`.

| [INDEX] | [SURFACE]                                                                           | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `horizontal_numpy(form, force, alpha=100.0, kmax=100)`                              | numpy horizontal equilibrium                  |
|  [02]   | `horizontal_nodal_numpy(form, force, alpha=100, kmax=100)`                          | nodal horizontal equilibrium                  |
|  [03]   | `horizontal_nodal(form, force, *, callback=None) -> None`                           | pure-Python nodal fallback, always exported   |
|  [04]   | `vertical_from_q(form, scale=1.0, tol=1e-3)`                                        | vertical from force density                   |
|  [05]   | `vertical_from_zmax(form, zmax, xtol=1e-2, rtol=1e-3) -> tuple[FormDiagram, float]` | vertical scale search for target crown height |
|  [06]   | `relax_boundary_openings(form, fixed: list[int]) -> FormDiagram`                    | boundary relaxation via `compas_fd.fd_numpy`  |

[ENTRYPOINT_SCOPE]: parallelisation primitives — `compas_tna.equilibrium.parallelisation_numpy`

`compas_tna.equilibrium` does not re-export these sparse primitives, so import them from the `parallelisation_numpy` module directly; the horizontal solvers call them.

| [INDEX] | [SURFACE]                                                                                     | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `parallelise(A, x, b, known: list[int]) -> ndarray`                                           | least-squares step with known DOFs |
|  [02]   | `parallelise_nodal(xy, C, targets, i_nbrs, ij_e, fixed=None, kmax=100, lmin=None, lmax=None)` | node-per-node step                 |
|  [03]   | `parallelise_sparse(A, B, X, known, k=1, key=None)`                                           | sparse-solve step                  |
|  [04]   | `apply_bounds(x, xmin, xmax)`                                                                 | clamp coordinates to length bounds |

[ENTRYPOINT_SCOPE]: load computation — `compas_tna.loads`

`LoadUpdater` builds its face matrix once at construction, then `__call__(p, xyz)` writes `p[:, 2]` in place from `tributary_area * thickness * density + tributary_area * live`; `thickness` is a per-vertex array or a constant.

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | `LoadUpdater(mesh, p0, thickness=1.0, density=1.0, live=0.0)` | tributary load computer bound to the form mesh, loads `p0` (Nx3) |
|  [02]   | `LoadUpdater.__call__(p, xyz) -> None`                        | refresh `p[:,2]` selfweight in place for the current `xyz`       |
|  [03]   | `LoadUpdater.tributary_areas(xyz) -> ndarray (N,1)`           | per-vertex tributary area for the current coordinates            |
|  [04]   | `LoadUpdater.face_matrix() -> scipy.sparse.csr_matrix`        | normalized face-vertex centroid matrix                           |

[ENTRYPOINT_SCOPE]: envelope operations — `compas_tna.envelope`

`apply_*_to_formdiagram(formdiagram)` methods push the envelope geometry onto the form-diagram vertex attributes; the `compute_*(x, y)` family take coordinate arrays and return intrados/extrados arrays. Base methods raise `NotImplementedError`; each vault subclass overrides per geometry.

| [INDEX] | [SURFACE]                                                    | [CAPABILITY]                                    |
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

[TOPOLOGY]:
- `FormDiagram` extends the COMPAS `Mesh`; per-vertex `px`/`py`/`pz` loads, `_rx`/`_ry`/`_rz` reactions, support flags, and per-edge `q` force-density with `_is_edge`/`lmin`/`lmax`/`hmin`/`hmax` bounds drive every solver.
- `ForceDiagram.from_formdiagram(form)` rides the lower-level `FormDiagram.dual_diagram(cls)` (a generic dual mesh of the passed class); construct the reciprocal dual through the factory, never `dual_diagram` directly.
- `_numpy` solvers and `relax_boundary_openings` load only outside IronPython (`not compas.IPY`); `horizontal_nodal` is the always-available pure path, and `parallelisation_numpy` owns the sparse primitives the horizontal solvers call.
- `vertical_from_q` applies the force-density matrix at a fixed `scale`; `vertical_from_zmax` binary-searches `scale` (`xtol`/`rtol`) so the crown reaches `zmax`, internally driving a `LoadUpdater`.

[STACKING]:
- `compas`(`.api/compas.md`): diagrams and envelopes are `compas.data.Data`/`Mesh` subclasses graduating through `compas.json_dumps`, and the `_numpy` solvers cross the wire through `compas.rpc.Proxy.function("compas_tna.equilibrium.horizontal_numpy")` offloaded across the algebra owner's runtime THREAD band, returning `(form, force)` for the round-trip; `compas.md` owns the Proxy reconnect-or-spawn scope lifecycle and its `RetryClass.RPC` cold-start rail, and cold-start faults share one form-finding fault rail with `compas_dr`.
- `graph/algebra.md` selects TNA through the `FormEngine` sub-enum on the one form-finding case, reads `vertices_attributes("xy"/"xyz")` for the numeric rails, and drives `LoadUpdater` for selfweight through one per-vertex loop.
- `compas_fd.fd_numpy` takes the boundary-relaxation force-density solve that `relax_boundary_openings` delegates.

[LOCAL_ADMISSION]:
- TNA pipeline: `FormDiagram.create_*` or `from_mesh` -> `relax_boundary_openings(form, fixed)` -> `Envelope.apply_selfweight_to_formdiagram(form)` or `LoadUpdater` -> `ForceDiagram.from_formdiagram(form)` -> `horizontal_numpy(form, force)` -> `vertical_from_zmax(form, zmax)`.
- `LoadUpdater` binds a `Mesh` and a numpy `(n,3)` fixed-load array `p0`, mutating the passed `p` in place; diagrams and envelopes serialize through `compas.json_dumps` for graduation.

[RAIL_LAW]:
- Package: `compas_tna`
- Owns: TNA form-finding, reciprocal form/force diagram construction, horizontal/vertical equilibrium, sparse parallelisation, masonry envelope bounds
- Accept: `compas` `Mesh`/`FormDiagram`/`ForceDiagram` inputs and numpy arrays for the numeric rails
- Reject: a hand-rolled TNA or equilibrium solver, reciprocal-diagram construction bypassing `ForceDiagram.from_formdiagram`, direct sparse assembly outside `parallelisation_numpy`
