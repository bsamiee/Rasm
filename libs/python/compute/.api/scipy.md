# [PY_COMPUTE_API_SCIPY]

`scipy` supplies the dense/sparse linear algebra, optimization, integration, interpolation, and signal/statistics surfaces for the compute numeric-intent solver rail. The package owner routes each `NumericIntent` case onto a scipy submodule callable — `scipy.linalg.solve`, `scipy.sparse.linalg.spsolve`, `scipy.optimize.minimize`, `scipy.integrate.solve_ivp`, `scipy.interpolate.CubicSpline` — and captures tolerances and residuals as study evidence; it never re-implements a numeric routine scipy owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scipy`
- package: `scipy`
- import: `scipy` (lint alias `sp`); submodules `scipy.linalg`, `scipy.sparse`, `scipy.sparse.linalg`, `scipy.optimize`, `scipy.integrate`, `scipy.interpolate`, `scipy.signal`, `scipy.stats` (`scipy.stats.qmc`), `scipy.spatial`
- owner: `compute`
- rail: solvers
- capability: scientific solver suite — dense and sparse linear algebra, nonlinear optimization, numerical integration, interpolation, statistics, and signal processing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sparse containers and solver result types
- rail: solvers

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]      | [CAPABILITY]                            |
| :-----: | :----------------------------------- | :----------------- | :-------------------------------------- |
|  [01]   | `scipy.sparse.csr_array`             | sparse format      | compressed-sparse-row container         |
|  [02]   | `scipy.sparse.csc_array`             | sparse format      | compressed-sparse-column container      |
|  [03]   | `scipy.sparse.coo_array`             | sparse format      | coordinate-list container               |
|  [04]   | `scipy.sparse.dia_array`             | sparse format      | diagonal-storage container              |
|  [05]   | `scipy.sparse.bsr_array`             | sparse format      | block-sparse-row container              |
|  [06]   | `scipy.sparse.lil_array`             | sparse format      | row-based incremental-build container   |
|  [07]   | `scipy.sparse.linalg.LinearOperator` | matvec abstraction | matrix-free linear operator             |
|  [08]   | `scipy.optimize.OptimizeResult`      | result carrier     | solution, success flag, and diagnostics |
|  [09]   | `scipy.optimize.Bounds`              | constraint         | variable lower/upper bounds             |
|  [10]   | `scipy.optimize.LinearConstraint`    | constraint         | `lb <= A x <= ub` linear constraint     |
|  [11]   | `scipy.optimize.NonlinearConstraint` | constraint         | nonlinear `lb <= f(x) <= ub` constraint |
|  [12]   | `scipy.interpolate.BSpline`          | spline carrier     | knot/coefficient B-spline value         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `scipy.linalg` dense factorization and solve
- rail: solvers

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]  | [RESULT]                        |
| :-----: | :--------------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `solve(a, b, assume_a)`                  | dense solve     | solution of `a x = b`           |
|  [02]   | `inv(a)`                                 | dense solve     | matrix inverse                  |
|  [03]   | `det(a)`                                 | scalar          | determinant                     |
|  [04]   | `lu_factor(a)` \| `lu_solve(lu_piv, b)`  | factorization   | LU factor and back-substitution |
|  [05]   | `cholesky(a, lower)`                     | factorization   | Cholesky factor                 |
|  [06]   | `cho_factor(a)` \| `cho_solve(c_low, b)` | factorization   | Cholesky factor and solve       |
|  [07]   | `qr(a, mode, pivoting)`                  | factorization   | QR factor                       |
|  [08]   | `svd(a, full_matrices, compute_uv)`      | factorization   | singular value decomposition    |
|  [09]   | `eig(a, b, left, right)`                 | eigensolver     | general eigenvalues/vectors     |
|  [10]   | `eigh(a, b, eigvals_only)`               | eigensolver     | symmetric eigenvalues/vectors   |
|  [11]   | `lstsq(a, b, lapack_driver)`             | least squares   | minimum-norm least-squares fit  |
|  [12]   | `pinv(a, atol, rtol)`                    | dense solve     | Moore-Penrose pseudoinverse     |
|  [13]   | `expm(A)`                                | matrix function | matrix exponential              |
|  [14]   | `norm(a, ord, axis)`                     | scalar          | matrix or vector norm           |

[ENTRYPOINT_SCOPE]: `scipy.sparse` construction and `scipy.sparse.linalg` solve
- rail: solvers

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]  | [RESULT]                      |
| :-----: | :------------------------------------------------- | :-------------- | :---------------------------- |
|  [01]   | `sparse.diags(diagonals, offsets, shape)`          | construct       | banded sparse array           |
|  [02]   | `sparse.eye_array(m, k, format)`                   | construct       | sparse identity               |
|  [03]   | `sparse.kron(A, B, format)`                        | construct       | sparse Kronecker product      |
|  [04]   | `sparse.hstack(blocks)` \| `sparse.vstack(blocks)` | construct       | block-stacked sparse array    |
|  [05]   | `sparse.random_array(shape, density)`              | construct       | random sparse array           |
|  [06]   | `sparse.linalg.spsolve(A, b)`                      | direct solve    | sparse `A x = b` solution     |
|  [07]   | `sparse.linalg.splu(A)`                            | factorization   | sparse LU factor object       |
|  [08]   | `sparse.linalg.factorized(A)`                      | factorization   | reusable solve closure        |
|  [09]   | `sparse.linalg.cg(A, b, rtol, M)`                  | iterative solve | conjugate-gradient solution   |
|  [10]   | `sparse.linalg.gmres(A, b, restart)`               | iterative solve | GMRES solution                |
|  [11]   | `sparse.linalg.bicgstab(A, b, rtol)`               | iterative solve | BiCGSTAB solution             |
|  [12]   | `sparse.linalg.lsqr(A, b, atol, btol)`             | least squares   | sparse least-squares solution |
|  [13]   | `sparse.linalg.eigsh(A, k, which, sigma)`          | eigensolver     | symmetric sparse eigenpairs   |
|  [14]   | `sparse.linalg.svds(A, k, which)`                  | factorization   | sparse truncated SVD          |

[ENTRYPOINT_SCOPE]: `scipy.optimize` root-find and minimize
- rail: solvers

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]  | [RESULT]                           |
| :-----: | :---------------------------------------------------- | :-------------- | :--------------------------------- |
|  [01]   | `minimize(fun, x0, method, jac, bounds, constraints)` | minimize        | `OptimizeResult` local minimum     |
|  [02]   | `minimize_scalar(fun, bracket, bounds, method)`       | minimize        | scalar-function minimum            |
|  [03]   | `least_squares(fun, x0, jac, bounds, method)`         | least squares   | nonlinear least-squares solution   |
|  [04]   | `curve_fit(f, xdata, ydata, p0, bounds)`              | least squares   | fitted parameters and covariance   |
|  [05]   | `root(fun, x0, method, jac)`                          | root find       | vector-root `OptimizeResult`       |
|  [06]   | `root_scalar(f, method, bracket, x0)`                 | root find       | scalar-root result                 |
|  [07]   | `brentq(f, a, b, xtol, rtol)`                         | root find       | bracketed scalar root              |
|  [08]   | `newton(func, x0, fprime, tol)`                       | root find       | Newton/secant scalar root          |
|  [09]   | `fsolve(func, x0, fprime)`                            | root find       | hybrd vector root                  |
|  [10]   | `linprog(c, A_ub, b_ub, A_eq, b_eq, bounds)`          | linear program  | HiGHS LP solution                  |
|  [11]   | `milp(c, integrality, bounds, constraints)`           | integer program | mixed-integer LP solution          |
|  [12]   | `differential_evolution(func, bounds, strategy)`      | global optimize | stochastic global minimum          |
|  [13]   | `nnls(A, b, maxiter)`                                 | least squares   | nonnegative least-squares solution |
|  [14]   | `linear_sum_assignment(cost_matrix)`                  | assignment      | optimal row/column assignment      |

[ENTRYPOINT_SCOPE]: `scipy.integrate` and `scipy.interpolate`
- rail: solvers

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RESULT]                           |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------- |
|  [01]   | `integrate.quad(func, a, b, epsabs, epsrel)`                  | quadrature     | scalar integral plus error         |
|  [02]   | `integrate.dblquad(func, a, b, gfun, hfun)`                   | quadrature     | double integral                    |
|  [03]   | `integrate.nquad(func, ranges, opts)`                         | quadrature     | n-dimensional integral             |
|  [04]   | `integrate.quad_vec(f, a, b, epsabs, workers)`                | quadrature     | vector-valued integral             |
|  [05]   | `integrate.solve_ivp(fun, t_span, y0, method)`                | ODE integrator | initial-value ODE solution         |
|  [06]   | `integrate.odeint(func, y0, t)`                               | ODE integrator | LSODA ODE solution                 |
|  [07]   | `integrate.simpson(y, x, dx)`                                 | sampled rule   | Simpson-rule integral              |
|  [08]   | `integrate.trapezoid(y, x, dx)`                               | sampled rule   | trapezoidal integral               |
|  [09]   | `interpolate.interp1d(x, y, kind)`                            | interpolant    | callable 1-D interpolant           |
|  [10]   | `interpolate.CubicSpline(x, y, bc_type)`                      | interpolant    | C2 cubic-spline interpolant        |
|  [11]   | `interpolate.PchipInterpolator(x, y)`                         | interpolant    | shape-preserving cubic interpolant |
|  [12]   | `interpolate.make_interp_spline(x, y, k)`                     | interpolant    | `BSpline` interpolant              |
|  [13]   | `interpolate.griddata(points, values, xi)`                    | scattered      | unstructured-grid interpolation    |
|  [14]   | `interpolate.RegularGridInterpolator(points, values, method)` | grid           | regular-grid interpolant           |

[ENTRYPOINT_SCOPE]: `scipy.signal` filter design, spectral estimation, and resampling
- rail: signal

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]    | [RESULT]                             |
| :-----: | :-------------------------------------------- | :---------------- | :----------------------------------- |
|  [01]   | `butter(N, Wn, btype, output='sos')`          | IIR design        | Butterworth SOS / b,a coefficients   |
|  [02]   | `firwin(numtaps, cutoff, window, pass_zero)`  | FIR design        | windowed-FIR tap coefficients        |
|  [03]   | `sosfiltfilt(sos, x, axis)`                   | zero-phase filter | forward-backward SOS-filtered signal |
|  [04]   | `filtfilt(b, a, x, axis)`                     | zero-phase filter | forward-backward b,a-filtered signal |
|  [05]   | `welch(x, fs, nperseg, noverlap)`             | spectral estimate | Welch PSD `(f, Pxx)`                 |
|  [06]   | `spectrogram(x, fs, nperseg, noverlap)`       | spectral estimate | time-frequency `(f, t, Sxx)`         |
|  [07]   | `resample_poly(x, up, down, axis)`            | resample          | polyphase rational resample          |
|  [08]   | `find_peaks(x, height, distance, prominence)` | peak detect       | peak indices and properties          |

[ENTRYPOINT_SCOPE]: `scipy.stats.qmc` quasi-Monte-Carlo sampling
- rail: experiments

`Sobol`/`Halton`/`LatinHypercube` are `QMCEngine` subclasses whose `random(n)` draws a low-discrepancy sample on the unit hypercube; `scale` affinely maps the sample to the bounds box and `discrepancy` scores the sample uniformity.

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]   | [RESULT]                               |
| :-----: | :------------------------------------------------------ | :--------------- | :------------------------------------- |
|  [01]   | `qmc.Sobol(d, scramble, seed)` -> `.random(n)`          | QMC engine       | scrambled Sobol low-discrepancy sample |
|  [02]   | `qmc.Halton(d, scramble, seed)` -> `.random(n)`         | QMC engine       | Halton low-discrepancy sample          |
|  [03]   | `qmc.LatinHypercube(d, scramble, seed)` -> `.random(n)` | QMC engine       | Latin-hypercube stratified sample      |
|  [04]   | `qmc.scale(sample, l_bounds, u_bounds)`                 | affine map       | sample scaled to the bounds box        |
|  [05]   | `qmc.discrepancy(sample, method)`                       | uniformity score | low-discrepancy quality metric         |

[ENTRYPOINT_SCOPE]: `scipy.spatial` neighbour search, hull, and tessellation
- rail: spatial

`cKDTree` is the compiled KD-tree; `ConvexHull`/`Delaunay`/`Voronoi` are Qhull-backed tessellation carriers whose attributes expose simplices, vertices, and adjacency; `distance.cdist` is the pairwise distance matrix.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]   | [RESULT]                               |
| :-----: | :-------------------------------------------------------------- | :--------------- | :------------------------------------- |
|  [01]   | `cKDTree(data)` -> `.query(x, k)`                               | neighbour search | k-nearest indices and distances        |
|  [02]   | `cKDTree.query_ball_point(x, r)`                                | radius search    | indices within radius `r`              |
|  [03]   | `ConvexHull(points)` -> `.simplices`/`.volume`/`.area`          | hull             | facet simplices, hull volume and area  |
|  [04]   | `Delaunay(points)` -> `.simplices`/`.points`/`.find_simplex(p)` | triangulation    | simplex table, points, point-location  |
|  [05]   | `Voronoi(points)` -> `.vertices`/`.regions`/`.ridge_points`     | tessellation     | Voronoi vertices, regions, ridge graph |
|  [06]   | `distance.cdist(XA, XB, metric)`                                | distance matrix  | pairwise distance matrix               |

## [04]-[IMPLEMENTATION_LAW]

[SOLVER_TOPOLOGY]:
- dense linear: `scipy.linalg` owns factorizations (`lu_factor`, `cholesky`, `qr`, `svd`), direct solve (`solve`, `inv`, `lstsq`, `pinv`), eigensolvers (`eig`, `eigh`), and matrix functions (`expm`).
- sparse storage: `scipy.sparse` owns the CSR/CSC/COO/DIA/BSR/LIL containers; construct from `diags`, `eye_array`, `kron`, or block stacks before solving.
- sparse solve: `scipy.sparse.linalg` owns direct solve (`spsolve`, `splu`, `factorized`), Krylov iterative solvers (`cg`, `gmres`, `bicgstab`, `lsqr`), and sparse eigen/SVD (`eigsh`, `svds`) over a matrix or `LinearOperator`.
- optimization: `scipy.optimize` owns minimization (`minimize`, `differential_evolution`), least squares (`least_squares`, `curve_fit`, `nnls`), root finding (`root`, `brentq`, `newton`, `fsolve`), and linear/integer programming (`linprog`, `milp`); constraints route through `Bounds`, `LinearConstraint`, and `NonlinearConstraint`; results carry an `OptimizeResult`.
- integration: `scipy.integrate` owns adaptive quadrature (`quad`, `dblquad`, `nquad`, `quad_vec`), ODE integrators (`solve_ivp`, `odeint`), and sampled rules (`simpson`, `trapezoid`).
- interpolation: `scipy.interpolate` owns 1-D interpolants (`interp1d`, `CubicSpline`, `PchipInterpolator`, `make_interp_spline`) and scattered/grid interpolation (`griddata`, `RegularGridInterpolator`).
- signal: `scipy.signal` owns IIR/FIR design (`butter`, `firwin`), zero-phase filtering (`sosfiltfilt`, `filtfilt`), spectral estimation (`welch`, `spectrogram`), polyphase resampling (`resample_poly`), and peak detection (`find_peaks`); the `signal/dsp.md#DSP` owner routes the stationary cases here beside the `pywt` wavelet cases.
- quasi-Monte-Carlo: `scipy.stats.qmc` owns the low-discrepancy engines (`Sobol`, `Halton`, `LatinHypercube`) with `scale` to a bounds box and `discrepancy` scoring; the `experiments/study.md#STUDY` DOE sampler routes here.
- spatial: `scipy.spatial` owns KD-tree neighbour/radius search (`cKDTree.query`/`query_ball_point`), Qhull hull/tessellation (`ConvexHull`, `Delaunay`, `Voronoi`), and pairwise distance (`distance.cdist`); the `spatial/query.md#SPATIAL` owner routes every geometry query here.

[LOCAL_ADMISSION]:
- import: submodule imports at boundary scope only; module-level import is banned by the manifest import policy.
- routing: `NumericIntent` dense-linear -> `scipy.linalg`; sparse-solve -> `scipy.sparse.linalg`; nonlinear-optimize -> `scipy.optimize`; integrate -> `scipy.integrate`; interpolate -> `scipy.interpolate`.
- evidence: each solve captures the route callable, the tolerance inputs, and the convergence/residual (the `OptimizeResult` flags or solver residual) as a study receipt.
- boundary: scipy results are offline study evidence; production substrate selection and benchmark claims stay in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `scipy`
- Owns: dense/sparse linear algebra, nonlinear optimization, numerical integration, and interpolation for the numeric-intent rail
- Accept: a `NumericIntent` case routed to a scipy submodule callable with captured tolerances and residuals
- Reject: hand-rolled numeric kernels scipy owns; wrapper-renames of solver callables; product benchmark claims
