# [PY_COMPUTE_API_SCIKIT_FEM]

`scikit-fem` (module `skfem`) supplies finite-element mesh management, element spaces, basis assembly, system conditioning, and linear/eigenvalue solve pipelines for the compute FEM structural and field-analysis rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scikit-fem`
- package: `scikit-fem`
- module: `skfem`
- asset: runtime library
- rail: FEM assembly and solve

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh types
- rail: FEM assembly

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]      | [CAPABILITY]                                                     |
| :-----: | :----------- | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `MeshLine1`  | 1-D simplex mesh   | line segment mesh                                                |
|  [02]   | `MeshTri1`   | 2-D simplex mesh   | triangular mesh; `init_circle`, `init_lshaped`, `init_symmetric` |
|  [03]   | `MeshTri2`   | 2-D quadratic mesh | quadratic triangular mesh                                        |
|  [04]   | `MeshQuad1`  | 2-D quad mesh      | quadrilateral mesh; `init_tensor`                                |
|  [05]   | `MeshQuad2`  | 2-D quad mesh      | quadratic quadrilateral mesh                                     |
|  [06]   | `MeshTet1`   | 3-D simplex mesh   | tetrahedral mesh; `init_ball`, `init_tensor`                     |
|  [07]   | `MeshTet2`   | 3-D quadratic mesh | quadratic tetrahedral mesh                                       |
|  [08]   | `MeshHex1`   | 3-D hex mesh       | hexahedral mesh; `init_tensor`                                   |
|  [09]   | `MeshHex2`   | 3-D quad hex mesh  | quadratic hexahedral mesh                                        |
|  [10]   | `MeshWedge1` | 3-D wedge mesh     | wedge/prism mesh                                                 |

[PUBLIC_TYPE_SCOPE]: basis and form types
- rail: FEM assembly

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CAPABILITY]                                               |
| :-----: | :------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `CellBasis`          | interior basis | volume integration basis (alias: `Basis`, `InteriorBasis`) |
|  [02]   | `BoundaryFacetBasis` | boundary basis | integration on boundary facets                             |
|  [03]   | `FacetBasis`         | facet basis    | interior and boundary facet integration                    |
|  [04]   | `InteriorFacetBasis` | interior facet | DG interior facet basis                                    |
|  [05]   | `ExteriorFacetBasis` | exterior facet | exterior boundary facet basis                              |
|  [06]   | `BilinearForm`       | form           | assembles stiffness/mass matrix; `(u, v, w)` callback      |
|  [07]   | `LinearForm`         | form           | assembles load vector; `(v, w)` callback                   |
|  [08]   | `TrilinearForm`      | form           | assembles trilinear system tensor                          |
|  [09]   | `Functional`         | form           | assembles scalar functional                                |

[PUBLIC_TYPE_SCOPE]: element types (simplex H1)
- rail: FEM assembly

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]              |
| :-----: | :--------------- | :--------------- | :------------------------ |
|  [01]   | `ElementTriP1`   | H1 tri P1        | linear triangle           |
|  [02]   | `ElementTriP2`   | H1 tri P2        | quadratic triangle        |
|  [03]   | `ElementTriP3`   | H1 tri P3        | cubic triangle            |
|  [04]   | `ElementTriMini` | H1 Mini tri      | MINI enriched triangle    |
|  [05]   | `ElementTetP1`   | H1 tet P1        | linear tetrahedron        |
|  [06]   | `ElementTetP2`   | H1 tet P2        | quadratic tetrahedron     |
|  [07]   | `ElementTetMini` | H1 Mini tet      | MINI enriched tetrahedron |
|  [08]   | `ElementQuad1`   | H1 quad bilinear | bilinear quadrilateral    |
|  [09]   | `ElementHex1`    | H1 hex trilinear | trilinear hexahedron      |
|  [10]   | `ElementLineP1`  | H1 line P1       | linear line element       |
|  [11]   | `ElementLineP2`  | H1 line P2       | quadratic line element    |

[PUBLIC_TYPE_SCOPE]: element types (mixed/DG/vector)
- rail: FEM assembly

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [CAPABILITY]                           |
| :-----: | :----------------- | :------------- | :------------------------------------- |
|  [01]   | `ElementTriP1DG`   | DG P1 tri      | discontinuous Galerkin triangle        |
|  [02]   | `ElementTriRT0`    | H(div) tri RT0 | Raviart-Thomas lowest order triangle   |
|  [03]   | `ElementTriN1`     | H(curl) tri N1 | Nédélec first-kind triangle            |
|  [04]   | `ElementTetN0`     | H(curl) tet N0 | Nédélec lowest order tetrahedron       |
|  [05]   | `ElementTetRT0`    | H(div) tet RT0 | Raviart-Thomas lowest order tet        |
|  [06]   | `ElementVector`    | vector wrapper | vectorized scalar element              |
|  [07]   | `ElementVectorH1`  | vector H1      | H1 vector element                      |
|  [08]   | `ElementComposite` | composite      | product element for mixed formulations |
|  [09]   | `ElementDG`        | DG wrapper     | DG wrapper for any element             |

[PUBLIC_TYPE_SCOPE]: DOF and system types
- rail: FEM assembly

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                              |
| :-----: | :--------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `Dofs`                 | DOF table       | degree-of-freedom index mapping           |
|  [02]   | `DofsView`             | DOF filter      | boundary/interior DOF subsets             |
|  [03]   | `DiscreteField`        | field container | discrete field data with element metadata |
|  [04]   | `Mapping`              | mapping base    | reference-to-physical mapping base        |
|  [05]   | `MappingAffine`        | affine mapping  | constant Jacobian affine mapping          |
|  [06]   | `MappingIsoparametric` | isoparametric   | isoparametric reference-physical mapping  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Basis` construction and DOF access
- rail: FEM assembly

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :-------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `CellBasis(mesh, elem, mapping=None, intorder=None, ...)` | construction   | interior volume basis             |
|  [02]   | `BoundaryFacetBasis(mesh, elem, facets=None, ...)`        | construction   | boundary facet basis              |
|  [03]   | `basis.get_dofs(boundary_tag)`                            | DOF query      | DOF indices for a boundary tag    |
|  [04]   | `basis.zeros()` / `basis.ones()`                          | vector init    | zero/one DOF vectors              |
|  [05]   | `basis.interpolate(field)` / `basis.split(field)`         | postprocess    | field interpolation and splitting |

[ENTRYPOINT_SCOPE]: assembly functions
- rail: FEM assembly

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :-------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `asm(form, *bases)` → `spmatrix` or `ndarray` | assembly       | dispatch to bilinear/linear/functional asm |
|  [02]   | `BilinearForm(fn)(basis, ...)` → `spmatrix`   | bilinear asm   | stiffness/mass matrix                      |
|  [03]   | `LinearForm(fn)(basis, ...)` → `ndarray`      | linear asm     | load vector                                |
|  [04]   | `Functional(fn)(basis, ...)` → scalar         | functional asm | scalar quantity integration                |
|  [05]   | `bmat(blocks)` → `spmatrix`                   | block assembly | block-sparse matrix construction           |

[ENTRYPOINT_SCOPE]: boundary condition and system conditioning
- rail: FEM assembly

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `condense(A, b=None, x=None, I=None, D=None, expand=True)`       | BC application | Dirichlet condensation via DOF index or view |
|  [02]   | `enforce(A, b=None, x=None, I=None, D=None, diag=1.0, ...)`      | BC application | penalty-row enforcement                      |
|  [03]   | `penalize(A, b=None, x=None, I=None, D=None, epsilon=None, ...)` | BC application | epsilon-penalty Dirichlet                    |
|  [04]   | `mpc(A, b, S=None, M=None, T=None, g=None)`                      | BC application | multi-point constraint system                |
|  [05]   | `project(fun, basis_from=None, basis_to=None, diff=None, ...)`   | projection     | L2 projection of callable field              |
|  [06]   | `rcm(A, b)` → `(A_reordered, b_reordered, perm)`                 | reordering     | reverse Cuthill-McKee permutation            |

[ENTRYPOINT_SCOPE]: solvers
- rail: FEM assembly

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `solve(A, b, x=None, I=None, solver=None, ...)`        | solve dispatch | linear or eigenvalue solve dispatcher |
|  [02]   | `solve_linear(A, b, x=None, I=None, solver=None, ...)` | linear solve   | direct/iterative linear system solve  |
|  [03]   | `solve_eigen(A, M, x=None, I=None, solver=None, ...)`  | eigen solve    | generalized eigenproblem solve        |
|  [04]   | `solver_direct_scipy(**kwargs)` → callable             | solver factory | SciPy sparse direct solver            |
|  [05]   | `solver_iter_krylov(krylov=cg, ...)` → callable        | solver factory | Krylov iterative solver (CG default)  |
|  [06]   | `solver_iter_cg(**kwargs)` → callable                  | solver factory | conjugate gradient solver             |
|  [07]   | `solver_iter_pcg(**kwargs)` → callable                 | solver factory | preconditioned CG solver              |
|  [08]   | `solver_eigen_scipy(**kwargs)` → callable              | solver factory | SciPy eigensolver (ARPACK)            |
|  [09]   | `solver_eigen_scipy_sym(**kwargs)` → callable          | solver factory | SciPy symmetric eigensolver           |

[ENTRYPOINT_SCOPE]: mesh operations and IO
- rail: FEM assembly

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :-------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `MeshTri1.load(path)` / `MeshTet1.load(path)` | IO             | load mesh from file (Gmsh, VTK)   |
|  [02]   | `mesh.refined(times_or_ix=1)`                 | refinement     | uniform or marked mesh refinement |
|  [03]   | `mesh.boundary_facets()`                      | query          | boundary facet index array        |
|  [04]   | `mesh.with_boundaries({name: fn})`            | boundary label | label boundary by predicate       |
|  [05]   | `mesh.p` / `mesh.t` / `mesh.facets`           | data access    | nodes, connectivity, facet arrays |
|  [06]   | `mesh.element_finder()`                       | query          | element-index lookup callable     |

## [04]-[IMPLEMENTATION_LAW]

[FEM_TOPOLOGY]:
- namespace: `skfem`; all public types and functions at top level
- `CellBasis` (also exported as `Basis` and `InteriorBasis`) is the primary assembly object; all form assembly routes through it
- form callbacks receive `(u, v, w)` (bilinear), `(v, w)` (linear), or `(w,)` (functional) where `w` is the `DiscreteField` test/trial context
- `asm(form, basis)` is the unified assembly entry point; `BilinearForm(fn)(basis)` and `LinearForm(fn)(basis)` are the explicit routes
- `condense` returns a condensed system or expanded-shape tuple depending on `expand`; DOF sets pass as `DofsView`, `ndarray`, or dict
- solver factories return callables compatible with `solve` / `solve_linear` / `solve_eigen`; they are not called directly

[LOCAL_ADMISSION]:
- Mesh construction uses static factories (`MeshTri1.init_circle()`, `MeshTet1.init_ball()`, `MeshTri1.load(path)`) before basis construction.
- Boundary DOF sets pass as `basis.get_dofs(tag)` results (`DofsView`); boundary tag names are defined on the mesh via `define_boundary`.
- `condense` with `expand=True` returns the full-length solution vector; `expand=False` returns the condensed-size vector.
- Solver factories are passed as the `solver=` keyword to `solve` / `solve_linear` / `solve_eigen`; default is `solver_direct_scipy`.

[RAIL_LAW]:
- Package: `scikit-fem`
- Owns: FEM mesh management, element basis construction, bilinear/linear form assembly, boundary condition application, and sparse linear/eigenvalue solve dispatch
- Accept: `Mesh` + `Element` + `Basis` as the canonical FEM pipeline; `spmatrix` from `scipy.sparse` as the assembled system
- Reject: hand-rolled FEM assembly loops when `BilinearForm`/`LinearForm`/`asm` own the concern
