# [PY_COMPUTE_API_SCIKIT_FEM]

`scikit-fem` (module `skfem`) supplies finite-element mesh management, element spaces, threaded form assembly, system conditioning, and linear/eigen solve pipelines for the compute FEM structural and field-analysis rail. The package owner builds a `Mesh -> Element -> Basis` pipeline, assembles a `BilinearForm`/`LinearForm` into a `scipy.sparse` matrix, conditions it with `condense`/`enforce`/`penalize`/`mpc`, and solves with a `solve`/`solve_eigen` dispatcher fed by a scipy-backed solver factory; it never hand-rolls element quadrature, DOF bookkeeping, or assembly loops the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scikit-fem`
- package: `scikit-fem`
- import: `skfem`; all public types and functions live at the top level (no submodule import path required)
- owner: `compute`
- rail: FEM assembly and solve
- capability: simplex/tensor mesh management, H1/Hdiv/Hcurl/DG/global element spaces, threaded bilinear/linear/trilinear/functional assembly, Dirichlet/penalty/multi-point conditioning, sparse direct/Krylov/eigen solve dispatch, L2 projection, adaptive refinement, and physical-point interpolation/probing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh types
- rail: FEM assembly

`MeshX1` is affine (linear geometry); `MeshX2` is the isoparametric quadratic geometry of the same cell; `MeshXxDG` is the discontinuous (node-duplicated) variant used for per-element discontinuous spaces. `Mesh2D`/`Mesh3D`/`MeshTri`/`MeshTet`/`MeshQuad`/`MeshHex`/`MeshLine` are the abstract bases; concrete construction uses the numbered subclasses below.

| [INDEX] | [SYMBOL]                                                                       | [TYPE_FAMILY]      | [CAPABILITY]                                                                                        |
| :-----: | :----------------------------------------------------------------------------- | :----------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `MeshLine1`                                                                    | 1-D simplex mesh   | line segment mesh; `init_tensor`                                                                    |
|  [02]   | `MeshTri1`                                                                     | 2-D simplex mesh   | triangular mesh; `init_circle`, `init_lshaped`, `init_symmetric`, `init_sqsymmetric`, `init_tensor` |
|  [03]   | `MeshTri2`                                                                     | 2-D quadratic mesh | isoparametric quadratic triangular mesh                                                             |
|  [04]   | `MeshQuad1`                                                                    | 2-D quad mesh      | quadrilateral mesh; `init_tensor`                                                                   |
|  [05]   | `MeshQuad2`                                                                    | 2-D quad mesh      | quadratic quadrilateral mesh                                                                        |
|  [06]   | `MeshTet1`                                                                     | 3-D simplex mesh   | tetrahedral mesh; `init_ball`, `init_tensor`                                                        |
|  [07]   | `MeshTet2`                                                                     | 3-D quadratic mesh | quadratic tetrahedral mesh                                                                          |
|  [08]   | `MeshHex1`                                                                     | 3-D hex mesh       | hexahedral mesh; `init_tensor`                                                                      |
|  [09]   | `MeshHex2`                                                                     | 3-D quad hex mesh  | quadratic hexahedral mesh                                                                           |
|  [10]   | `MeshWedge1`                                                                   | 3-D wedge mesh     | wedge/prism mesh                                                                                    |
|  [11]   | `MeshTri1DG` \| `MeshTet1DG` \| `MeshQuad1DG` \| `MeshHex1DG` \| `MeshLine1DG` | DG geometry        | node-duplicated discontinuous-geometry meshes for DG/periodic spaces                                |

[PUBLIC_TYPE_SCOPE]: basis and form types
- rail: FEM assembly

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CAPABILITY]                                                                                         |
| :-----: | :------------------- | :------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `AbstractBasis`      | basis base     | shared DOF/quadrature/interpolation surface for all bases                                            |
|  [02]   | `CellBasis`          | interior basis | volume integration basis (alias: `Basis`, `InteriorBasis`)                                           |
|  [03]   | `FacetBasis`         | facet basis    | interior and boundary facet integration                                                              |
|  [04]   | `BoundaryFacetBasis` | boundary basis | integration on boundary facets (Neumann/Robin loads, boundary functionals)                           |
|  [05]   | `InteriorFacetBasis` | interior facet | DG interior-facet basis (jump/average traces)                                                        |
|  [06]   | `ExteriorFacetBasis` | exterior facet | exterior boundary facet basis                                                                        |
|  [07]   | `BilinearForm`       | form           | `(u, v, w)` callback -> stiffness/mass `spmatrix`; `dtype=complex64`, `nthreads>0` threaded assembly |
|  [08]   | `LinearForm`         | form           | `(v, w)` callback -> load `ndarray`; same `dtype`/`nthreads` knobs                                   |
|  [09]   | `TrilinearForm`      | form           | `(u, v, w, p)` callback -> trilinear system tensor (advection/Navier-Stokes terms)                   |
|  [10]   | `Functional`         | form           | `(w,)` callback -> scalar functional (energy, error norm, flux integral)                             |

[PUBLIC_TYPE_SCOPE]: element types (simplex/tensor H1 Lagrange)
- rail: FEM assembly

The Lagrange ladder runs `P0` (DG-only piecewise-constant) -> `P1` -> `P2` -> `P3` -> `P4` per cell; `Pp` is the arbitrary-order simplex element. `Mini`/`P1B`/`P2B`/`CR`/`CCR` are bubble/Crouzeix-Raviart enrichments for inf-sup-stable Stokes pairs.

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY]         | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------------- | :-------------------- | :--------------------------------------- |
|  [01]   | `ElementTriP1` \| `ElementTriP2` \| `ElementTriP3` \| `ElementTriP4`   | H1 tri Lagrange       | linear/quadratic/cubic/quartic triangle  |
|  [02]   | `ElementTriP0` \| `ElementTetP0` \| `ElementQuad0` \| `ElementHex0`    | DG piecewise-constant | cell-constant element (DG/pressure)      |
|  [03]   | `ElementTriPp` \| `ElementLinePp` \| `ElementQuadP`                    | H1 arbitrary-order    | order-`p` simplex/line/quad Lagrange     |
|  [04]   | `ElementTriMini` \| `ElementTetMini` \| `ElementLineMini`              | H1 Mini bubble        | MINI enriched element for Stokes         |
|  [05]   | `ElementTriP1B` \| `ElementTriP2B`                                     | H1 bubble             | bubble-enriched triangle                 |
|  [06]   | `ElementTriCR` \| `ElementTriCCR` \| `ElementTetCR` \| `ElementTetCCR` | nonconforming         | Crouzeix-Raviart / conforming-CR element |
|  [07]   | `ElementTetP1` \| `ElementTetP2`                                       | H1 tet Lagrange       | linear/quadratic tetrahedron             |
|  [08]   | `ElementQuad1` \| `ElementQuad2` \| `ElementQuadS2`                    | H1 quad               | bilinear/biquadratic/serendipity quad    |
|  [09]   | `ElementHex1` \| `ElementHex2` \| `ElementHexS2`                       | H1 hex                | trilinear/triquadratic/serendipity hex   |
|  [10]   | `ElementLineP1` \| `ElementLineP2`                                     | H1 line               | linear/quadratic line element            |
|  [11]   | `ElementWedge1`                                                        | H1 wedge              | linear wedge/prism element               |

[PUBLIC_TYPE_SCOPE]: element types (mixed/DG/vector/global)
- rail: FEM assembly

| [INDEX] | [SYMBOL]                                                                                                                                                   | [TYPE_FAMILY]         | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------- | :------------------------------------------------------------- |
|  [01]   | `ElementTriRT0` \| `ElementTriRT1` \| `ElementTriRT2` \| `ElementTetRT0` \| `ElementTetRT1` \| `ElementQuadRT0` \| `ElementHexRT1`                         | H(div) Raviart-Thomas | flux/mixed-Poisson H(div) element ladder                       |
|  [02]   | `ElementTriN1` \| `ElementTriN2` \| `ElementTriN3` \| `ElementTetN0` \| `ElementTetN1` \| `ElementQuadN1`                                                  | H(curl) Nédélec       | edge-element ladder for Maxwell/curl-curl                      |
|  [03]   | `ElementTriBDM1`                                                                                                                                           | H(div) BDM            | Brezzi-Douglas-Marini H(div) element                           |
|  [04]   | `ElementTriArgyris` \| `ElementTriMorley` \| `ElementTriHermite` \| `ElementLineHermite` \| `ElementQuadBFS` \| `ElementHexC1` \| `ElementTri15ParamPlate` | C1/plate              | fourth-order (Kirchhoff plate, biharmonic) C1 elements         |
|  [05]   | `ElementTriHHJ0` \| `ElementTriHHJ1`                                                                                                                       | symmetric-tensor      | Hellan-Herrmann-Johnson plate stress element                   |
|  [06]   | `ElementTriP1DG` \| `ElementTriP2G` \| `ElementTriP1G` \| `ElementQuad1DG` \| `ElementQuad2G` \| `ElementHex1DG`                                           | DG/global-numbering   | discontinuous-Galerkin and global-DOF Lagrange                 |
|  [07]   | `ElementVector` \| `ElementVectorH1`                                                                                                                       | vector wrapper        | vectorizes any scalar element to `dim` components (elasticity) |
|  [08]   | `ElementComposite`                                                                                                                                         | composite             | product/mixed element (e.g. Taylor-Hood velocity+pressure)     |
|  [09]   | `ElementDG`                                                                                                                                                | DG wrapper            | makes any element discontinuous per cell                       |
|  [10]   | `ElementH1` \| `ElementHdiv` \| `ElementHcurl` \| `ElementGlobal`                                                                                          | element base          | space-family bases for custom element definition               |

[PUBLIC_TYPE_SCOPE]: DOF, field, mapping, and system carriers
- rail: FEM assembly

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]   | [CAPABILITY]                                                                                                        |
| :-----: | :----------------------------------------------------- | :-------------- | :------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Dofs`                                                 | DOF table       | degree-of-freedom index mapping; `.N` is the global DOF count                                                       |
|  [02]   | `DofsView`                                             | DOF filter      | boundary/interior DOF subset with `.nodal`/`.facet`/`.edge`/`.interior`/`.all`/`.keep`/`.drop`/`.flatten` selectors |
|  [03]   | `DofsCollection`                                       | DOF group       | named DOF-view collection from `get_dofs` over multiple tags                                                        |
|  [04]   | `DiscreteField`                                        | field container | quadrature-point field data (`value`, `grad`, `hess`) passed as `w` to forms                                        |
|  [05]   | `Mapping` \| `MappingAffine` \| `MappingIsoparametric` | mapping         | reference-to-physical mapping (constant-Jacobian affine vs curved isoparametric)                                    |
|  [06]   | `LinearSystem` \| `CondensedSystem` \| `Solution`      | system carrier  | assembled/conditioned system bundle and solve result                                                                |
|  [07]   | `LinearSolver` \| `EigenSolver`                        | solver alias    | callable protocol the `solver=` keyword accepts                                                                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: mesh construction, labelling, and IO
- rail: FEM assembly

`init_*` factories build canonical domains; `with_boundaries`/`with_subdomains` attach named predicate regions (boundary-condition tags / material regions) that `basis.get_dofs(name)` and subdomain-restricted bases later resolve. `meshio`-backed `load`/`save` interoperate with Gmsh/VTK/XDMF.

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] | [RESULT]                                                |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `MeshTri1.init_circle(nrefs)` \| `init_lshaped()` \| `init_symmetric()` \| `MeshTet1.init_ball()` | factory        | canonical-domain mesh                                   |
|  [02]   | `MeshTri1.init_tensor(x, y)` \| `MeshHex1.init_tensor(x, y, z)`                                   | factory        | structured tensor-product mesh                          |
|  [03]   | `MeshTri1.load(filename, out=None)` \| `mesh.save(filename, point_data, cell_data)`               | IO             | meshio load/save (Gmsh, VTK, XDMF)                      |
|  [04]   | `mesh.with_boundaries({name: lambda x: ...})`                                                     | labelling      | mesh with named boundary facet sets                     |
|  [05]   | `mesh.with_subdomains({name: lambda x: ...})`                                                     | labelling      | mesh with named element (material) regions              |
|  [06]   | `mesh.refined(times_or_ix=1)`                                                                     | refinement     | uniform (int) or marked (index array) refined mesh      |
|  [07]   | `mesh.boundary_facets()` \| `boundary_nodes()` \| `boundary_edges()`                              | query          | boundary facet/node/edge index arrays                   |
|  [08]   | `mesh.element_finder()` \| `mesh.p` \| `mesh.t` \| `mesh.facets`                                  | query/data     | point-location callable; node/connectivity/facet arrays |

[ENTRYPOINT_SCOPE]: basis construction, DOF access, and field transfer
- rail: FEM assembly

`get_dofs` is the single polymorphic DOF selector — it discriminates on `facets` (boundary tag name or facet array), `elements` (subdomain), `nodes`, or `skip` (component filter) and returns a `DofsView`. `interpolate` lifts a DOF vector to a quadrature-point `DiscreteField` for nonlinear/coupled forms; `probes`/`global_coordinates`/`doflocs` give the physical-point transfer surface the callable-source projection residual needs.

| [INDEX] | [SURFACE]                                                                                            | [ENTRY_FAMILY]    | [RESULT]                                                           |
| :-----: | :--------------------------------------------------------------------------------------------------- | :---------------- | :----------------------------------------------------------------- |
|  [01]   | `Basis(mesh, elem, mapping=None, intorder=None, quadrature=None, elements=None)`                     | construction      | interior volume `CellBasis`                                        |
|  [02]   | `FacetBasis(mesh, elem, facets=None, ...)` \| `BoundaryFacetBasis(...)` \| `InteriorFacetBasis(...)` | construction      | facet integration basis                                            |
|  [03]   | `basis.get_dofs(facets=None, elements=None, nodes=None, skip=None)` -> `DofsView`                    | DOF query         | DOF indices for a boundary tag / region / component                |
|  [04]   | `basis.N` \| `basis.dofs.N` \| `basis.Nbfun`                                                         | DOF count         | global DOF count / per-element basis-fn count                      |
|  [05]   | `basis.zeros()` \| `basis.ones()`                                                                    | vector init       | zero/one DOF vectors                                               |
|  [06]   | `basis.interpolate(w)` -> `DiscreteField` \| `basis.split(w)`                                        | postprocess       | lift DOF vector to quadrature field / split mixed solution         |
|  [07]   | `basis.project(interp, elements=None)`                                                               | projection        | L2-project a callable/`DiscreteField` onto the basis               |
|  [08]   | `basis.probes(x)` \| `basis.interpolator(w)` \| `basis.point_source(x)`                              | physical sampling | interpolation matrix at points `x` / evaluator / point-load vector |
|  [09]   | `basis.doflocs` \| `basis.global_coordinates()`                                                      | DOF geometry      | physical DOF coordinates (callable-source transfer fidelity)       |
|  [10]   | `basis.with_element(elem)` \| `basis.with_elements(elements)`                                        | restriction       | rebind to new element / subdomain element subset                   |

[ENTRYPOINT_SCOPE]: assembly
- rail: FEM assembly

A form callback returns an `np`-broadcast expression over quadrature; `BilinearForm(fn)(basis)` and `LinearForm(fn)(basis)` are the explicit routes, `asm(form, *bases, to=...)` the unified dispatcher that also folds multi-basis blocks. Complex assembly sets `dtype=np.complex64`; threaded assembly sets `nthreads>0`.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RESULT]                                             |
| :-----: | :-------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `asm(form, *bases, to=_sum)`            | assembly       | dispatch to bilinear/linear/trilinear/functional asm |
|  [02]   | `BilinearForm(fn)(basis)` -> `spmatrix` | bilinear asm   | stiffness/mass matrix                                |
|  [03]   | `LinearForm(fn)(basis)` -> `ndarray`    | linear asm     | load vector                                          |
|  [04]   | `TrilinearForm(fn)(basis)` -> tensor    | trilinear asm  | trilinear system tensor (advection)                  |
|  [05]   | `Functional(fn)(basis)` -> scalar       | functional asm | scalar integral (energy, error norm)                 |
|  [06]   | `bmat(blocks)` -> `spmatrix`            | block assembly | block-sparse matrix from a 2-D block list            |

[ENTRYPOINT_SCOPE]: boundary conditions and system conditioning
- rail: FEM assembly

`condense` is the canonical eliminate-and-restore route; `enforce`/`penalize` are penalty alternatives; `mpc` imposes multi-point (periodic/tied) constraints; `rcm` reorders for bandwidth. `D`/`I` accept a `DofsView`, an index `ndarray`, or a `{name: DofsView}` dict.

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [RESULT]                                                      |
| :-----: | :---------------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `condense(A, b=None, x=None, I=None, D=None, expand=True)`              | Dirichlet      | condensed system; `expand=True` restores full-length solution |
|  [02]   | `enforce(A, b=None, x=None, I=None, D=None, diag=1.0, overwrite=False)` | penalty row    | identity-row enforcement of Dirichlet DOFs                    |
|  [03]   | `penalize(A, b=None, x=None, I=None, D=None, epsilon=None)`             | penalty        | epsilon-penalty Dirichlet                                     |
|  [04]   | `mpc(A, b, S=None, M=None, T=None, g=None)`                             | constraint     | multi-point (periodic/tied) constrained system                |
|  [05]   | `rcm(A, b)` -> `(A_perm, b_perm, perm)`                                 | reordering     | reverse Cuthill-McKee bandwidth reduction                     |

[ENTRYPOINT_SCOPE]: solve dispatch and solver/preconditioner factories
- rail: FEM assembly

`solve` discriminates on the `solver` callable's return arity (`ndarray` linear vs `(w, x)` eigenpair); `solve_linear`/`solve_eigen` are the explicit routes. Solver factories return the callable passed as `solver=`; preconditioner builders feed `solver_iter_krylov(M=...)`.

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RESULT]                                                              |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `solve(A, b, x=None, I=None, solver=None, **kw)`                                           | solve dispatch | linear or eigen solve discriminated by solver arity                   |
|  [02]   | `solve_linear(A, b, x=None, I=None, solver=None, **kw)`                                    | linear solve   | `A x = b` solution                                                    |
|  [03]   | `solve_eigen(A, M, x=None, I=None, solver=None, **kw)`                                     | eigen solve    | generalized `A x = λ M x` eigenpairs                                  |
|  [04]   | `solver_direct_scipy(**kw)` -> callable                                                    | factory        | SciPy sparse direct (`splu`-backed) solver                            |
|  [05]   | `solver_iter_krylov(krylov=cg, verbose=False, **kw)` -> callable                           | factory        | Krylov iterative solver (CG default; pass `gmres`/`bicgstab`)         |
|  [06]   | `solver_iter_cg(**kw)` \| `solver_iter_pcg(**kw)` -> callable                              | factory        | conjugate-gradient / preconditioned-CG solver                         |
|  [07]   | `solver_eigen_scipy(**kw)` \| `solver_eigen_scipy_sym(**kw)` -> callable                   | factory        | SciPy ARPACK eigensolver (general/symmetric)                          |
|  [08]   | `build_pc_diag(A)` \| `build_pc_ilu(A, drop_tol=1e-4, fill_factor=20)` -> `LinearOperator` | preconditioner | Jacobi / ILU preconditioner for `solver_iter_krylov(M=...)`           |
|  [09]   | `adaptive_theta(est, theta=0.5, max=None)`                                                 | refinement     | element index set for Dörfler-marked `mesh.refined(ix)` adaptive loop |

## [04]-[IMPLEMENTATION_LAW]

[FEM_TOPOLOGY]:
- pipeline: `Mesh -> Element -> Basis` is canonical; `CellBasis` (alias `Basis`/`InteriorBasis`) owns volume assembly, `FacetBasis`/`BoundaryFacetBasis`/`InteriorFacetBasis` own facet assembly. Element/geometry order must match the assembled physics (`MeshTri2` + `ElementTriP2` for curved isoparametric).
- forms: callbacks receive `DiscreteField` arguments (`u`, `v` trial/test; `w` the parameter context carrying `w['mesh-data']`, prior solutions, and coordinates). `BilinearForm`/`LinearForm`/`TrilinearForm`/`Functional` are the four assembly arities; `dtype=np.complex64` and `nthreads>0` are the complex/threaded knobs.
- DOF selection: `basis.get_dofs(...)` is one polymorphic selector; it discriminates on the named `facets`/`elements`/`nodes`/`skip` keyword, returning a `DofsView` whose `.nodal`/`.facet`/`.edge`/`.interior`/`.all`/`.keep`/`.drop` filters carve the Dirichlet/Neumann partition. Never enumerate DOF families with parallel `get_boundary_dofs`/`get_interior_dofs` calls.
- conditioning: `condense` eliminates constrained DOFs and (with `expand=True`) restores the full-length solution; `enforce`/`penalize` keep system size via penalty rows; `mpc` ties DOFs for periodic/coupled meshes.
- solve: `solve(A, b, solver=...)` discriminates linear vs eigen by the factory's return arity. Solver factories (`solver_direct_scipy`, `solver_iter_krylov`, `solver_eigen_scipy`) are passed as `solver=`, never called directly; preconditioners (`build_pc_diag`, `build_pc_ilu`) feed `solver_iter_krylov(M=...)`.

[LOCAL_ADMISSION]:
- import: top-level `skfem` import (no submodule path); the assembled `spmatrix` is `scipy.sparse` and routes the sparse-solve evidence through `.api/scipy.md`.
- transfer: a callable source/Dirichlet field projects with `basis.project(fun)`; physical-point transfer fidelity uses `basis.probes(x)`/`global_coordinates()`/`doflocs` (not finiteness-only checks), feeding the `solvers/field.md#FIELD` transfer-residual receipt.
- adaptive: an error estimator drives `adaptive_theta(est, theta)` -> `mesh.refined(ix)`; the refinement loop captures the estimator and marked-element count as study evidence.
- boundary: scikit-fem assembly/solve is offline study evidence; the conditioned `spmatrix`/solution crosses to scipy for sparse solve, and production substrate/benchmark authority stays in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `scikit-fem`
- Owns: FEM mesh management, H1/Hdiv/Hcurl/DG/C1 element spaces, threaded bilinear/linear/trilinear/functional assembly, Dirichlet/penalty/multi-point conditioning, sparse direct/Krylov/eigen solve dispatch, L2 projection, adaptive refinement, and physical-point interpolation
- Accept: a `Mesh + Element + Basis` pipeline assembled via `BilinearForm`/`LinearForm`/`asm`, conditioned via `condense`/`enforce`/`mpc`, solved via `solve`/`solve_eigen` with a captured solver factory and DOF-partition receipt
- Reject: hand-rolled element quadrature, DOF bookkeeping, or assembly loops when `Basis`/`BilinearForm`/`LinearForm`/`asm` own the concern; parallel DOF-selection method families when `get_dofs` discriminates by keyword
