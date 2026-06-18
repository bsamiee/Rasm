# [PY_COMPUTE_IDEAS]

The forward pool of higher-order folder concepts grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or modern technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

## [1]-[OPEN]

[ARRAY_API_ADMISSION]: rebuild `ArrayPayload` admission on the Array API standard.
- Admit any conformant backend through `array_namespace(x)` namespace dispatch, so a numpy floor, a JAX array, a Dask graph, or a Sparse matrix admit through one path and every solver route dispatches through the resolved `xp`.
- Unlocks backend-portable solver dispatch and a clean accelerator story: JAX is a backend resolved at admission, not a JIT wrap, and numba stays a distinct loop-kernel compiler on the nonlinear route.
- Draws on `array-api-compat` and `array-api-extra` (both pure-Python and cp315-clean, resolving on the cp315 core), the write-once technique scipy 1.17 and scikit-learn now vendor, replacing the numpy-only admission plus a per-accelerator JIT wrap.

[JAX_SOLVER_FAMILY]: adopt the patrick-kidger JAX scientific family as the gated high-accuracy solver tier.
- Lineax unifies dense, sparse, and iterative linear solves and least-squares over a general linear operator as one autodifferentiable surface; Optimistix owns root-finding, minimisation, fixed-points, and nonlinear least-squares; Equinox is the PyTree foundation both compose.
- Unlocks autodifferentiable linear and nonlinear solves that close the loop with the differentiation owner through implicit-function-theorem adjoints, and one operator abstraction across dense, sparse, and iterative solves instead of four separate scipy entry points.
- Draws on the patrick-kidger JAX scientific family; `lineax`, `optimistix`, `diffrax`, and `equinox` all resolve on the gated jaxlib `python_version<'3.15'` floor.

[DIFFERENTIAL_EQUATIONS]: add a differential-equations solver sub-domain over Diffrax.
- A new solver route beside the 1-D quadrature: ODE, SDE, and CDE integration with adaptive step control, event handling, and adjoint-differentiable integration over Diffrax in JAX and Equinox.
- Unlocks offline ODE and dynamics evidence — relaxation, transient field decay, parametric trajectory studies — that graduates with adjoint sensitivities, feeding the study spine and the geometry form-finding handoff case.
- Draws on the genuine capability gap that the existing `Integrate` case is scalar quadrature only with no initial-value owner; Diffrax resolves on the gated jaxlib floor.

[SALIB_SENSITIVITY]: make SALib the canonical global-sensitivity owner on the study spine.
- Replace the hand-rolled numpy Morris elementary effects and the stubbed Saltelli Sobol indices with SALib's sampler-and-analyzer pair for Sobol, Morris, and FAST, keeping `scipy.stats.qmc` for low-discrepancy sampling and scikit-learn for surrogates.
- Unlocks full first-and-total-order Sobol indices, FAST, and grouped or optimal-trajectory Morris without local reinvention, a real sensitivity capability on the cp315 floor where SALib's numpy and scipy core runs.
- Draws on the ecosystem-first law: SALib owns sensitivity analysis and is pure-Python over numpy and scipy, resolving on the cp315 core.

[BAYESIAN_BACKEND_AXIS]: generalise the inference sampler into a backend discriminant.
- The one `Inference` owner selects the MCMC engine by case — PyMC-native NUTS or Metropolis, NumPyro JAX NUTS, Nutpie Rust or Numba NUTS — with arviz as the single cross-backend rhat-and-ess diagnostic owner.
- Unlocks GPU and JAX-accelerated NUTS and Rust/Numba NUTS as case rows on one owner with a single diagnostic gate, so a posterior graduates regardless of which engine sampled it.
- Draws on the documented PyMC multi-backend story: `numpyro` is already manifest-admitted on the jaxlib floor, and `nutpie` is the missing Numba-marker-floor backend.

[MPMATH_ENCLOSURE_FLOOR]: add an mpmath arbitrary-precision floor beneath the gated Arb path.
- An always-on cp315-clean interval floor in the validated-numerics owner beneath the python-flint Arb ball-arithmetic path, replacing the coarse `np.nextafter` double-precision outward-rounding band.
- Unlocks a tight certified-enclosure floor on the cp315 interpreter, so validated-numerics evidence graduates with a usable enclosure even before the Arb deploy asset lands.
- Draws on `mpmath` (pure-Python, cp315-clean, arbitrary-precision interval arithmetic, resolving on the cp315 core), sound where the Arb wheel is unavailable.

[SPATIAL_GEOMETRY_QUERIES]: open the `spatial` sub-domain as the array-native computational-geometry owner over `scipy.spatial`.
- One `SpatialQuery` owner discriminating nearest-neighbour and radius search over a `cKDTree`, convex hull and Delaunay triangulation over `Qhull`, Voronoi tessellation, and the alpha-shape boundary extraction folded from the Delaunay simplices, every query keyed by the `ArrayPayload` content key.
- Unlocks point-cloud and sample-set neighbourhood evidence — proximity graphs, mesh-free interpolation stencils, and boundary reconstruction — that the study spine reads and that aligns to the geometry-branch scan companion at the wire, distinct from the geometry branch's own `open3d` mesh owner.
- Draws on `scipy.spatial` (`cKDTree`, `ConvexHull`, `Delaunay`, `Voronoi`) which the manifest already admits through scipy, closing the visible `spatial` gap with no new package and the alpha-shape circumradius fold as the one local algorithm.

[SIMFRAME_MESH_FIELD]: open the `simframe` sub-domain as the simulation mesh-and-field interchange and weak-form assembly owner beside the FEM solver route.
- One `MeshField` interchange owner carrying the mesh topology, the per-node and per-cell field arrays, and the reusable `assemble` that lowers a weak form to the sparse stiffness/load pair the `solvers/quadrature.md#QUADRATURE` FEM route and a Diffrax field problem both consume, never re-owning the solve.
- Unlocks reusable field interchange across the FEM and differential-equations routes — a discretized field assembled once feeds the stiffness solve, the transient integration, and the study spine — and aligns the mesh shape to the geometry-branch tessellation at the wire rather than coupling to it.
- Draws on `scikit-fem` (`Mesh`, `Basis`, `asm`) for assembly and `meshio` for the mesh-and-field file interchange, separating the assembly-and-interchange concern the quadrature FEM route currently inlines from the solve it keeps.

[RUNTIME_BOUNDARY_PARITY]: pin every compute-to-runtime composition against the exact runtime owner spelling.
- Every page composing the runtime boundary names the real owner entry: `Receipt.of(phase, owner, subject, facts)` with `phase` in the `runtime/observability/receipts.md` `Phase` literal, `BoundaryFault(<case>=payload)` keyword case-construction from `runtime/reliability/faults.md`, and `ContentIdentity.of(fmt, source, policy)` with an `IdentityPolicy` from `runtime/identity/content-identity.md` — never a fabricated PascalCase factory (`Receipt.Emitted`, `BoundaryFault.Boundary`) or a non-existent shorthand (`ContentIdentity.key`).
- Unlocks zero-guess transcription of the consumer boundary: the four runtime-composing packages (`compute`, `data`, `geometry`, `artifacts`) read one authoritative owner spelling and re-mint nothing, so the boundary-fault, receipt, and content-identity owners stay single-minted across the branch.
- Draws on the topology law that the four consumers compose runtime shapes at the boundary and never re-mint; the drift was a missing runtime owner-spelling reference, the same gap a `runtime/.api`-equivalent owner card closes for every consumer.

## [2]-[CLOSED]

No idea has closed.
