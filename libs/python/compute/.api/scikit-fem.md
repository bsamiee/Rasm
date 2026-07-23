# [PY_COMPUTE_API_SCIKIT_FEM]

`scikit-fem` owns finite-element mesh management, element spaces, threaded form assembly, conditioning, and linear/eigen solve for the compute FEM structural and field-analysis rail. Assembly emits a `scipy.sparse` matrix that crosses to scipy for sparse solve, and the assembled system is offline study evidence, never a production substrate.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scikit-fem`
- package: `scikit-fem`
- import: `skfem`; public types and functions at top level
- owner: `compute`
- rail: FEM assembly and solve
- capability: simplex/tensor mesh management, H1/Hdiv/Hcurl/DG/C1 element spaces, threaded bilinear/linear/trilinear/functional assembly, Dirichlet/penalty/multi-point conditioning, sparse direct/Krylov/eigen solve dispatch, L2 projection, adaptive refinement, and physical-point interpolation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh types

`MeshX1` is affine, `MeshX2` isoparametric-quadratic over the same cell, `MeshXxDG` the node-duplicated discontinuous variant for DG/periodic spaces. Unnumbered bases (`Mesh2D`/`MeshTri`) are abstract; the numbered subclasses construct, each carrying `init_tensor`.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]      | [CAPABILITY]                                                                      |
| :-----: | :----------- | :----------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `MeshLine1`  | 1-D simplex mesh   | line segment mesh                                                                 |
|  [02]   | `MeshTri1`   | 2-D simplex mesh   | triangular mesh; `init_circle`/`init_lshaped`/`init_symmetric`/`init_sqsymmetric` |
|  [03]   | `MeshTri2`   | 2-D quadratic mesh | isoparametric quadratic triangular mesh                                           |
|  [04]   | `MeshQuad1`  | 2-D quad mesh      | quadrilateral mesh                                                                |
|  [05]   | `MeshQuad2`  | 2-D quad mesh      | quadratic quadrilateral mesh                                                      |
|  [06]   | `MeshTet1`   | 3-D simplex mesh   | tetrahedral mesh; `init_ball`                                                     |
|  [07]   | `MeshTet2`   | 3-D quadratic mesh | quadratic tetrahedral mesh                                                        |
|  [08]   | `MeshHex1`   | 3-D hex mesh       | hexahedral mesh                                                                   |
|  [09]   | `MeshHex2`   | 3-D quad hex mesh  | quadratic hexahedral mesh                                                         |
|  [10]   | `MeshWedge1` | 3-D wedge mesh     | wedge/prism mesh                                                                  |

[PUBLIC_TYPE_SCOPE]: basis and form types

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CAPABILITY]                                                      |
| :-----: | :------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `AbstractBasis`      | basis base     | shared DOF/quadrature/interpolation surface for all bases         |
|  [02]   | `CellBasis`          | interior basis | volume integration basis (alias: `Basis`, `InteriorBasis`)        |
|  [03]   | `FacetBasis`         | facet basis    | interior and boundary facet integration                           |
|  [04]   | `BoundaryFacetBasis` | boundary basis | boundary facets (Neumann/Robin loads, boundary functionals)       |
|  [05]   | `InteriorFacetBasis` | interior facet | DG interior-facet basis (jump/average traces)                     |
|  [06]   | `ExteriorFacetBasis` | exterior facet | exterior boundary facet basis                                     |
|  [07]   | `BilinearForm`       | form           | `(u, v, w)` callback -> stiffness/mass `spmatrix`                 |
|  [08]   | `LinearForm`         | form           | `(v, w)` callback -> load `ndarray`                               |
|  [09]   | `TrilinearForm`      | form           | `(u, v, w, p)` callback -> trilinear tensor (advection/N-S terms) |
|  [10]   | `Functional`         | form           | `(w,)` callback -> scalar functional (energy, error norm, flux)   |

[PUBLIC_TYPE_SCOPE]: element types (simplex/tensor H1 Lagrange)

Triangle Lagrange runs `P0` (piecewise-constant) through `P4`; `ElementLinePp`/`ElementQuadP` are arbitrary-order line/quad. `Mini`/`P1B`/`P2B`/`CR`/`CCR` enrich for inf-sup-stable Stokes pairs.

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY]         | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------------------- | :-------------------- | :------------------------------------- |
|  [01]   | `ElementTriP1` \| `ElementTriP2` \| `ElementTriP3` \| `ElementTriP4`   | H1 tri Lagrange       | P1-P4 Lagrange triangle                |
|  [02]   | `ElementTriP0` \| `ElementTetP0` \| `ElementQuad0` \| `ElementHex0`    | DG piecewise-constant | cell-constant element (DG/pressure)    |
|  [03]   | `ElementLinePp` \| `ElementQuadP`                                      | H1 arbitrary-order    | order-`p` line/quad Lagrange           |
|  [04]   | `ElementTriMini` \| `ElementTetMini` \| `ElementLineMini`              | H1 Mini bubble        | MINI enriched element for Stokes       |
|  [05]   | `ElementTriP1B` \| `ElementTriP2B`                                     | H1 bubble             | bubble-enriched triangle               |
|  [06]   | `ElementTriCR` \| `ElementTriCCR` \| `ElementTetCR` \| `ElementTetCCR` | nonconforming         | Crouzeix-Raviart/conforming-CR         |
|  [07]   | `ElementTetP1` \| `ElementTetP2`                                       | H1 tet Lagrange       | linear/quadratic tetrahedron           |
|  [08]   | `ElementQuad1` \| `ElementQuad2` \| `ElementQuadS2`                    | H1 quad               | bilinear/biquadratic/serendipity quad  |
|  [09]   | `ElementHex1` \| `ElementHex2` \| `ElementHexS2`                       | H1 hex                | trilinear/triquadratic/serendipity hex |
|  [10]   | `ElementLineP1` \| `ElementLineP2`                                     | H1 line               | linear/quadratic line element          |
|  [11]   | `ElementWedge1`                                                        | H1 wedge              | linear wedge/prism element             |

[PUBLIC_TYPE_SCOPE]: element types (mixed/DG/vector/global)

Exact members per family in `[ELEMENT_ROSTER]` below.

| [INDEX] | [ELEMENT_FAMILY]      | [CAPABILITY]                                                   |
| :-----: | :-------------------- | :------------------------------------------------------------- |
|  [01]   | H(div) Raviart-Thomas | flux/mixed-Poisson H(div) element ladder                       |
|  [02]   | H(curl) Nédélec       | edge-element ladder for Maxwell/curl-curl                      |
|  [03]   | H(div) BDM            | Brezzi-Douglas-Marini H(div) element                           |
|  [04]   | C1/plate              | fourth-order (Kirchhoff plate, biharmonic) C1 elements         |
|  [05]   | symmetric-tensor      | Hellan-Herrmann-Johnson plate stress element                   |
|  [06]   | DG/global-numbering   | discontinuous-Galerkin and global-DOF Lagrange                 |
|  [07]   | vector wrapper        | vectorizes any scalar element to `dim` components (elasticity) |
|  [08]   | composite             | product/mixed element (e.g. Taylor-Hood velocity+pressure)     |
|  [09]   | DG wrapper            | makes any element discontinuous per cell                       |
|  [10]   | element base          | space-family bases for custom element definition               |

[ELEMENT_ROSTER]: exact member names per family
- [01]-[H_DIV_RT]: `ElementTriRT0`/`ElementTriRT1`/`ElementTriRT2`/`ElementTetRT0`/`ElementTetRT1`/`ElementQuadRT0`/`ElementHexRT1`
- [02]-[H_CURL_NEDELEC]: `ElementTriN1`/`ElementTriN2`/`ElementTriN3`/`ElementTetN0`/`ElementTetN1`/`ElementQuadN1`
- [03]-[H_DIV_BDM]: `ElementTriBDM1`
- [04]-[C1_PLATE]: `ElementTriArgyris`/`ElementTriMorley`/`ElementTriHermite`/`ElementLineHermite`/`ElementQuadBFS`/`ElementHexC1`/`ElementTri15ParamPlate`
- [05]-[SYMMETRIC_TENSOR]: `ElementTriHHJ0`/`ElementTriHHJ1`
- [06]-[DG_GLOBAL]: `ElementTriP1DG`/`ElementTriP2G`/`ElementTriP1G`/`ElementQuad1DG`/`ElementQuad2G`/`ElementHex1DG`
- [07]-[VECTOR]: `ElementVector`/`ElementVectorH1`
- [08]-[COMPOSITE]: `ElementComposite`
- [09]-[DG_WRAPPER]: `ElementDG`
- [10]-[ELEMENT_BASE]: `ElementH1`/`ElementHdiv`/`ElementHcurl`/`ElementGlobal`

[PUBLIC_TYPE_SCOPE]: DOF, field, mapping, and system carriers

`DofsView` carries the `.nodal`/`.facet`/`.edge`/`.interior`/`.all`/`.keep`/`.drop`/`.flatten` selectors; `MappingAffine` is constant-Jacobian, `MappingIsoparametric` curves.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]   | [CAPABILITY]                                                 |
| :-----: | :----------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `Dofs`                                                 | DOF table       | degree-of-freedom index mapping; `.N` global DOF count       |
|  [02]   | `DofsView`                                             | DOF filter      | boundary/interior DOF subset (selectors in lead)             |
|  [03]   | `DofsCollection`                                       | DOF group       | named DOF-view collection from `get_dofs` over tags          |
|  [04]   | `DiscreteField`                                        | field container | quadrature-point field (`value`/`grad`/`hess`) passed as `w` |
|  [05]   | `Mapping` \| `MappingAffine` \| `MappingIsoparametric` | mapping         | reference-to-physical mapping (affine vs isoparametric)      |
|  [06]   | `LinearSystem` \| `CondensedSystem` \| `Solution`      | system carrier  | assembled/conditioned system bundle and solve result         |
|  [07]   | `LinearSolver` \| `EigenSolver`                        | solver alias    | callable protocol the `solver=` keyword accepts              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: mesh construction, labelling, and IO

`init_circle`/`init_lshaped`/`init_symmetric`/`init_ball` build canonical domains; `with_boundaries`/`with_subdomains` attach named predicate regions that `get_dofs(name)` and subdomain bases resolve. `save`/`load` are `meshio`-backed (Gmsh/VTK/XDMF).

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [RESULT]                                     |
| :-----: | :------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `MeshTri1.init_tensor(x, y)` \| `MeshHex1.init_tensor(x, y, z)`      | factory        | structured tensor-product mesh               |
|  [02]   | `mesh.load(filename)` \| `mesh.save(...)`                            | IO             | meshio load/save (Gmsh, VTK, XDMF)           |
|  [03]   | `mesh.with_boundaries({name: lambda x: ...})`                        | labelling      | mesh with named boundary facet sets          |
|  [04]   | `mesh.with_subdomains({name: lambda x: ...})`                        | labelling      | mesh with named element (material) regions   |
|  [05]   | `mesh.refined(times_or_ix=1)`                                        | refinement     | uniform/marked (index-array) refined mesh    |
|  [06]   | `mesh.boundary_facets()` \| `boundary_nodes()` \| `boundary_edges()` | query          | boundary facet/node/edge index arrays        |
|  [07]   | `mesh.element_finder()` \| `mesh.p` \| `mesh.t` \| `mesh.facets`     | query/data     | point-location callable; connectivity arrays |

[ENTRYPOINT_SCOPE]: basis construction, DOF access, and field transfer

`Basis` takes `mapping`/`intorder`/`quadrature`/`elements`; `FacetBasis`/`BoundaryFacetBasis`/`InteriorFacetBasis` are facet constructors over `(mesh, elem, facets=None)`. `interpolate` lifts a DOF vector to a quadrature-point `DiscreteField`; `probes`/`global_coordinates`/`doflocs` are the physical-point transfer surface.

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY]    | [RESULT]                                             |
| :-----: | :------------------------------------------------------- | :---------------- | :--------------------------------------------------- |
|  [01]   | `Basis(mesh, elem, ...)`                                 | construction      | interior volume `CellBasis`                          |
|  [02]   | `FacetBasis(mesh, elem, facets=None)`                    | construction      | facet integration basis (boundary/interior variants) |
|  [03]   | `.get_dofs(...)` -> `DofsView`                           | DOF query         | DOF indices for a boundary tag / region / component  |
|  [04]   | `.N` \| `.dofs.N` \| `.Nbfun`                            | DOF count         | global DOF count / per-element basis-fn count        |
|  [05]   | `.zeros()` \| `.ones()`                                  | vector init       | zero/one DOF vectors                                 |
|  [06]   | `.interpolate(w)` -> `DiscreteField` \| `.split(w)`      | postprocess       | lift DOF vector to quad field / split mixed solution |
|  [07]   | `.project(interp, elements=None)`                        | projection        | L2-project a callable/`DiscreteField` onto the basis |
|  [08]   | `.probes(x)` \| `.interpolator(w)` \| `.point_source(x)` | physical sampling | interp matrix at `x` / evaluator / point-load vector |
|  [09]   | `.doflocs` \| `.global_coordinates()`                    | DOF geometry      | physical DOF coordinates (transfer fidelity)         |
|  [10]   | `.with_element(elem)` \| `.with_elements(elements)`      | restriction       | rebind to new element / subdomain element subset     |

[ENTRYPOINT_SCOPE]: assembly

A form callback returns a broadcast expression over quadrature points; `BilinearForm(fn)(basis)`/`LinearForm(fn)(basis)` are explicit, `asm(form, *bases, to=...)` the dispatcher folding multi-basis blocks.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RESULT]                                             |
| :-----: | :-------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `asm(form, *bases, to=_sum)`            | assembly       | dispatch to bilinear/linear/trilinear/functional asm |
|  [02]   | `BilinearForm(fn)(basis)` -> `spmatrix` | bilinear asm   | stiffness/mass matrix                                |
|  [03]   | `LinearForm(fn)(basis)` -> `ndarray`    | linear asm     | load vector                                          |
|  [04]   | `TrilinearForm(fn)(basis)` -> tensor    | trilinear asm  | trilinear system tensor (advection)                  |
|  [05]   | `Functional(fn)(basis)` -> scalar       | functional asm | scalar integral (energy, error norm)                 |
|  [06]   | `bmat(blocks)` -> `spmatrix`            | block assembly | block-sparse matrix from a 2-D block list            |

[ENTRYPOINT_SCOPE]: boundary conditions and system conditioning

Dirichlet conditioners share `(A, b=None, x=None, I=None, D=None)`; `D`/`I` accept a `DofsView`, an index `ndarray`, or a `{name: DofsView}` dict.

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [RESULT]                                                      |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------------ |
|  [01]   | `condense(..., expand=True)`                | Dirichlet      | condensed system; `expand=True` restores full-length solution |
|  [02]   | `enforce(..., diag=1.0, overwrite=False)`   | penalty row    | identity-row enforcement of Dirichlet DOFs                    |
|  [03]   | `penalize(..., epsilon=None)`               | penalty        | epsilon-penalty Dirichlet                                     |
|  [04]   | `mpc(A, b, S=None, M=None, T=None, g=None)` | constraint     | multi-point (periodic/tied) constrained system                |
|  [05]   | `rcm(A, b)` -> `(A_perm, b_perm, perm)`     | reordering     | reverse Cuthill-McKee bandwidth reduction                     |

[ENTRYPOINT_SCOPE]: solve dispatch and solver/preconditioner factories

`solve`/`solve_linear` take `(A, b, x=None, I=None, solver=None)`, `solve_eigen` takes `(A, M)`. A solver factory returns the callable passed as `solver=`; `build_pc_diag`/`build_pc_ilu` return a `LinearOperator` feeding `solver_iter_krylov(M=...)`.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RESULT]                                            |
| :-----: | :----------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `solve(A, b, ..., solver=None)`                              | solve dispatch | linear or eigen solve discriminated by solver arity |
|  [02]   | `solve_linear(A, b, ...)`                                    | linear solve   | `A x = b` solution                                  |
|  [03]   | `solve_eigen(A, M, ...)`                                     | eigen solve    | generalized `A x = λ M x` eigenpairs                |
|  [04]   | `solver_direct_scipy(**kw)`                                  | factory        | SciPy sparse direct (`spsolve`-backed) solver       |
|  [05]   | `solver_iter_krylov(krylov=cg, verbose=False, **kw)`         | factory        | Krylov iterative solver (CG; `gmres`/`bicgstab`)    |
|  [06]   | `solver_iter_cg(**kw)` \| `solver_iter_pcg(**kw)`            | factory        | conjugate-gradient / preconditioned-CG solver       |
|  [07]   | `solver_eigen_scipy(**kw)` \| `solver_eigen_scipy_sym(**kw)` | factory        | SciPy ARPACK eigensolver (general/symmetric)        |
|  [08]   | `build_pc_diag(A)` \| `build_pc_ilu(A, ...)`                 | preconditioner | Jacobi / ILU preconditioner                         |
|  [09]   | `adaptive_theta(est, theta=0.5, max=None)`                   | refinement     | Dörfler-marked element set for `mesh.refined(ix)`   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- pipeline: `Mesh -> Element -> Basis`; `CellBasis` (alias `Basis`/`InteriorBasis`) owns volume assembly, `FacetBasis`/`BoundaryFacetBasis`/`InteriorFacetBasis` own facet assembly, and element/geometry order matches the assembled physics (`MeshTri2` + `ElementTriP2` for curved isoparametric).
- forms: callbacks receive `DiscreteField` arguments (`u`/`v` trial/test, `w` the parameter context carrying mesh data, prior solutions, and coordinates); `BilinearForm`/`LinearForm`/`TrilinearForm`/`Functional` are the four assembly arities, with `dtype=np.complex64` and `nthreads>0` the complex and threaded knobs.
- selection: `basis.get_dofs(...)` is one polymorphic selector discriminating on the `facets`/`elements`/`nodes`/`skip` keyword and returning a `DofsView` whose selectors carve the Dirichlet/Neumann partition.
- conditioning: `condense` eliminates constrained DOFs and (with `expand=True`) restores full length; `enforce`/`penalize` hold system size via penalty rows; `mpc` ties DOFs for periodic/coupled meshes.
- solve: `solve(A, b, solver=...)` discriminates linear vs eigen by the factory's return arity; factories (`solver_direct_scipy`/`solver_iter_krylov`/`solver_eigen_scipy`) pass as `solver=`, and preconditioners (`build_pc_diag`/`build_pc_ilu`) feed `solver_iter_krylov(M=...)`.

[STACKING]:
- `scipy`(`.api/scipy.md`): assembly emits a `scipy.sparse` `spmatrix` crossing to `scipy.sparse.linalg` for the solve — `solver_direct_scipy` wraps `spsolve`, `solver_iter_krylov` wraps `cg`/`gmres`/`bicgstab`, `solver_eigen_scipy` wraps ARPACK `eigs` and `solver_eigen_scipy_sym` wraps `eigsh`.
- `gmsh`(`.api/gmsh.md`) / `meshio`(`.api/meshio.md`): a `mesh.load`-read mesh assembles directly, its physical groups arriving as boundary/subdomain tags that `with_boundaries`/`with_subdomains` name and `get_dofs` resolves.
- within-lib: the `QuadratureIntent` weak-form fold assembles through `Basis`/`BilinearForm`, `MeshField`/`MeshExchange` own mesh topology and IO, `FieldQuery` reads back through `basis.probes`/`interpolate`, and the solve folds its solver factory and DOF-partition onto `SolverReceipt`.

[LOCAL_ADMISSION]:
- import: top-level `skfem`; the assembled `spmatrix` is `scipy.sparse`.
- transfer: a callable source/Dirichlet field projects with `basis.project`; physical-point transfer fidelity uses `basis.probes`/`global_coordinates`/`doflocs`, feeding the `FieldQuery` transfer-residual receipt.
- adaptive: an error estimator drives `adaptive_theta(est, theta)` -> `mesh.refined(ix)`, the loop capturing the estimator and marked-element count as study evidence.
- boundary: the conditioned `spmatrix`/solution crosses to scipy for sparse solve, and production substrate/benchmark authority stays in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `scikit-fem`
- Owns: FEM mesh management, H1/Hdiv/Hcurl/DG/C1 element spaces, threaded bilinear/linear/trilinear/functional assembly, Dirichlet/penalty/multi-point conditioning, sparse direct/Krylov/eigen solve dispatch, L2 projection, adaptive refinement, and physical-point interpolation
- Accept: a `Mesh + Element + Basis` pipeline assembled via `BilinearForm`/`LinearForm`/`asm`, conditioned via `condense`/`enforce`/`mpc`, solved via `solve`/`solve_eigen` with a captured solver factory and DOF-partition receipt
- Reject: hand-rolled element quadrature, DOF bookkeeping, or assembly loops when `Basis`/`BilinearForm`/`LinearForm`/`asm` own the concern; parallel DOF-selection method families when `get_dofs` discriminates by keyword
