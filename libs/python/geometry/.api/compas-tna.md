# [PY_GEOMETRY_API_COMPAS_TNA]

`compas_tna` supplies thrust network analysis (TNA) for masonry vault form-finding: dual form/force diagram datastructures, horizontal equilibrium solvers, vertical equilibrium solvers, self-weight and load updaters, and envelope geometry families for AEC structural geometry workflows.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas_tna`
- package: `compas_tna`
- module: `compas_tna`
- asset: runtime library
- rail: geometry-algebra / structural-form-finding

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: diagram family
- rail: form-force dual graph

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [RAIL]                              |
| :-----: | :------------- | :-------------- | :---------------------------------- |
|   [1]   | `Diagram`      | base mesh graph | shared vertex/edge/face topology    |
|   [2]   | `FormDiagram`  | form graph      | loads, supports, force-density data |
|   [3]   | `ForceDiagram` | force polygon   | reciprocal dual force polygon       |

[PUBLIC_TYPE_SCOPE]: envelope family
- rail: structural envelope geometry

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [RAIL]                       |
| :-----: | :----------------------- | :--------------- | :--------------------------- |
|   [1]   | `Envelope`               | base envelope    | thickness/fill weight bounds |
|   [2]   | `MeshEnvelope`           | mesh-driven      | mesh-based target surface    |
|   [3]   | `BrepEnvelope`           | Brep-driven      | Brep-based target surface    |
|   [4]   | `CrossVaultEnvelope`     | parametric vault | cross-vault geometry         |
|   [5]   | `DomeEnvelope`           | parametric vault | dome geometry                |
|   [6]   | `PavillionVaultEnvelope` | parametric vault | pavillion vault geometry     |
|   [7]   | `PointedVaultEnvelope`   | parametric vault | pointed vault geometry       |
|   [8]   | `ParametricEnvelope`     | parametric base  | generic parametric envelope  |

[PUBLIC_TYPE_SCOPE]: load and scene family
- rail: load computation, scene rendering

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                                       |
| :-----: | :------------ | :------------ | :------------------------------------------- |
|   [1]   | `LoadUpdater` | load computer | tributary area selfweight + live loading     |
|   [2]   | `FormObject`  | scene node    | Rhino/notebook scene object for FormDiagram  |
|   [3]   | `ForceObject` | scene node    | Rhino/notebook scene object for ForceDiagram |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: FormDiagram construction
- rail: form-finding setup

| [INDEX] | [SURFACE]                                                                                                     | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :------------------------------------------------------------------------------------------------------------ | :------------- | :-------------------------- |
|   [1]   | `FormDiagram.create_ortho(x_span, y_span, nx, ny, supports)`                                                  | factory        | orthogonal grid diagram     |
|   [2]   | `FormDiagram.create_fan(x_span, y_span, n_fans, n_hoops, supports)`                                           | factory        | fan pattern diagram         |
|   [3]   | `FormDiagram.create_circular_radial(center, radius, n_hoops, n_parallels, r_oculus, diagonal, diagonal_type)` | factory        | circular radial diagram     |
|   [4]   | `FormDiagram.create_circular_radial_spaced(...)`                                                              | factory        | non-uniform radial diagram  |
|   [5]   | `FormDiagram.create_circular_spiral(...)`                                                                     | factory        | spiral radial diagram       |
|   [6]   | `FormDiagram.create_cross(...)`                                                                               | factory        | cross-vault pattern diagram |
|   [7]   | `FormDiagram.create_cross_with_diagonal(...)`                                                                 | factory        | cross + diagonal diagram    |
|   [8]   | `FormDiagram.create_arch(...)`                                                                                | factory        | arch diagram                |
|   [9]   | `FormDiagram.create_arch_equally_spaced(...)`                                                                 | factory        | equally spaced arch diagram |
|  [10]   | `FormDiagram.create_parametric_fan(...)`                                                                      | factory        | parametric fan diagram      |
|  [11]   | `FormDiagram.from_mesh(mesh: compas.Mesh) -> FormDiagram`                                                     | converter      | from COMPAS Mesh            |

[ENTRYPOINT_SCOPE]: equilibrium solvers
- rail: horizontal + vertical equilibrium

| [INDEX] | [SURFACE]                                                                                                                  | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------- |
|   [1]   | `horizontal_numpy(form, force, alpha=100.0, kmax=100) -> tuple[FormDiagram, ForceDiagram]`                                 | solver         | numpy horizontal equilibrium |
|   [2]   | `horizontal_nodal_numpy(form, force, alpha=100, kmax=100) -> tuple[FormDiagram, ForceDiagram]`                             | solver         | nodal horizontal equilibrium |
|   [3]   | `vertical_from_q(form, scale=1.0, density=1.0, kmax=100, tol=0.001, display=False)`                                        | solver         | vertical from force density  |
|   [4]   | `vertical_from_zmax(form, zmax, kmax=100, xtol=0.01, rtol=0.001, density=1.0, display=False) -> tuple[FormDiagram, float]` | solver         | vertical from max height     |
|   [5]   | `relax_boundary_openings(form, fixed: list[int]) -> FormDiagram`                                                           | relaxation     | boundary opening relaxation  |
|   [6]   | `parallelise(A, x, b, known) -> ndarray`                                                                                   | numeric        | force parallelisation step   |
|   [7]   | `parallelise_nodal(xy, C, targets, i_nbrs, ij_e, fixed, kmax, lmin, lmax)`                                                 | numeric        | nodal parallelisation step   |

[ENTRYPOINT_SCOPE]: load computation
- rail: load application

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------------------------------------ | :------------- | :---------------------------- |
|   [1]   | `LoadUpdater(mesh, p0, thickness=1.0, density=1.0, live=0.0)` | constructor    | tributary load computer setup |
|   [2]   | `LoadUpdater.tributary_areas() -> ...`                        | method         | per-vertex tributary area     |
|   [3]   | `LoadUpdater.face_matrix() -> ...`                            | method         | face area matrix              |

[ENTRYPOINT_SCOPE]: envelope operations
- rail: structural bounds application

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `Envelope(rho=20.0, rho_fill=14.0, is_parametric)`  | constructor    | material density bounds            |
|   [2]   | `Envelope.apply_selfweight_to_formdiagram(...)`     | method         | apply selfweight load              |
|   [3]   | `Envelope.apply_fill_weight_to_formdiagram(...)`    | method         | apply fill weight load             |
|   [4]   | `Envelope.apply_bounds_to_formdiagram(...)`         | method         | apply height bounds                |
|   [5]   | `Envelope.apply_target_heights_to_formdiagram(...)` | method         | apply target height targets        |
|   [6]   | `Envelope.compute_bounds()`                         | method         | compute extrados/intrados surfaces |
|   [7]   | `Envelope.compute_selfweight()`                     | method         | compute distributed selfweight     |
|   [8]   | `Envelope.compute_volume()`                         | method         | compute enclosed volume            |

## [4]-[IMPLEMENTATION_LAW]

[TNA_TOPOLOGY]:
- subpackages: `diagrams`, `equilibrium`, `envelope`, `loads`, `scene`, `rhino`, `notebook`, `numdata`
- `FormDiagram` extends COMPAS `Mesh`; carries per-vertex load, reaction, residual, support type, and force-density data
- `ForceDiagram` is the reciprocal dual; `dual_diagram()` on `FormDiagram` constructs it
- equilibrium module exposes both Python and numpy variants; `_numpy` variants are the production path
- `parallelisation_numpy` owns the sparse linear algebra primitives used by horizontal solvers
- `vertical_from_q` uses force-density matrix directly; `vertical_from_zmax` binary-searches scale for a target crown height

[LOCAL_ADMISSION]:
- Import diagrams from `compas_tna.diagrams`; equilibrium functions from `compas_tna.equilibrium`
- `horizontal_numpy` and `vertical_from_q`/`vertical_from_zmax` are the primary production entrypoints
- `LoadUpdater` requires a COMPAS `Mesh` and a numpy `(n, 3)` load array `p0`
- Envelope subclasses override `compute_bounds` to define geometry-specific intrados/extrados surfaces

[RAIL_LAW]:
- Package: `compas_tna`
- Owns: TNA form-finding, horizontal/vertical equilibrium, structural envelope bounds
- Accept: COMPAS Mesh/FormDiagram inputs; numpy arrays for numeric rails
- Reject: hand-rolled TNA solvers or duplicate equilibrium implementations
