# [PY_COMPUTE_API_SCIPY]

`scipy` supplies the dense/sparse linear algebra, optimization, integration, interpolation, and signal/statistics surfaces for the compute numeric-intent solver rail. The package owner routes each `NumericIntent` case onto a scipy submodule callable — `scipy.linalg.solve`, `scipy.sparse.linalg.spsolve`, `scipy.optimize.minimize`, `scipy.integrate.solve_ivp`, `scipy.interpolate.CubicSpline` — and captures tolerances and residuals as study evidence; it never re-implements a numeric routine scipy owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scipy`
- package: `scipy`
- import: `scipy` (lint alias `sp`); submodules `scipy.linalg`, `scipy.sparse`, `scipy.sparse.linalg`, `scipy.optimize`, `scipy.integrate`, `scipy.interpolate`
- owner: `compute`
- rail: solvers
- capability: scientific solver suite — dense and sparse linear algebra, nonlinear optimization, numerical integration, interpolation, statistics, and signal processing

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sparse containers and solver result types
- rail: solvers

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]      | [CAPABILITY]                            |
| :-----: | :----------------------------------- | :----------------- | :-------------------------------------- |
|   [1]   | `scipy.sparse.csr_array`             | sparse format      | compressed-sparse-row container         |
|   [2]   | `scipy.sparse.csc_array`             | sparse format      | compressed-sparse-column container      |
|   [3]   | `scipy.sparse.coo_array`             | sparse format      | coordinate-list container               |
|   [4]   | `scipy.sparse.dia_array`             | sparse format      | diagonal-storage container              |
|   [5]   | `scipy.sparse.bsr_array`             | sparse format      | block-sparse-row container              |
|   [6]   | `scipy.sparse.lil_array`             | sparse format      | row-based incremental-build container   |
|   [7]   | `scipy.sparse.linalg.LinearOperator` | matvec abstraction | matrix-free linear operator             |
|   [8]   | `scipy.optimize.OptimizeResult`      | result carrier     | solution, success flag, and diagnostics |
|   [9]   | `scipy.optimize.Bounds`              | constraint         | variable lower/upper bounds             |
|  [10]   | `scipy.optimize.LinearConstraint`    | constraint         | `lb <= A x <= ub` linear constraint     |
|  [11]   | `scipy.optimize.NonlinearConstraint` | constraint         | nonlinear `lb <= f(x) <= ub` constraint |
|  [12]   | `scipy.interpolate.BSpline`          | spline carrier     | knot/coefficient B-spline value         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `scipy.linalg` dense factorization and solve
- rail: solvers

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]  | [RESULT]                        |
| :-----: | :--------------------------------------- | :-------------- | :------------------------------ |
|   [1]   | `solve(a, b, assume_a)`                  | dense solve     | solution of `a x = b`           |
|   [2]   | `inv(a)`                                 | dense solve     | matrix inverse                  |
|   [3]   | `det(a)`                                 | scalar          | determinant                     |
|   [4]   | `lu_factor(a)` \| `lu_solve(lu_piv, b)`  | factorization   | LU factor and back-substitution |
|   [5]   | `cholesky(a, lower)`                     | factorization   | Cholesky factor                 |
|   [6]   | `cho_factor(a)` \| `cho_solve(c_low, b)` | factorization   | Cholesky factor and solve       |
|   [7]   | `qr(a, mode, pivoting)`                  | factorization   | QR factor                       |
|   [8]   | `svd(a, full_matrices, compute_uv)`      | factorization   | singular value decomposition    |
|   [9]   | `eig(a, b, left, right)`                 | eigensolver     | general eigenvalues/vectors     |
|  [10]   | `eigh(a, b, eigvals_only)`               | eigensolver     | symmetric eigenvalues/vectors   |
|  [11]   | `lstsq(a, b, lapack_driver)`             | least squares   | minimum-norm least-squares fit  |
|  [12]   | `pinv(a, atol, rtol)`                    | dense solve     | Moore-Penrose pseudoinverse     |
|  [13]   | `expm(A)`                                | matrix function | matrix exponential              |
|  [14]   | `norm(a, ord, axis)`                     | scalar          | matrix or vector norm           |

[ENTRYPOINT_SCOPE]: `scipy.sparse` construction and `scipy.sparse.linalg` solve
- rail: solvers

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]  | [RESULT]                      |
| :-----: | :------------------------------------------------- | :-------------- | :---------------------------- |
|   [1]   | `sparse.diags(diagonals, offsets, shape)`          | construct       | banded sparse array           |
|   [2]   | `sparse.eye_array(m, k, format)`                   | construct       | sparse identity               |
|   [3]   | `sparse.kron(A, B, format)`                        | construct       | sparse Kronecker product      |
|   [4]   | `sparse.hstack(blocks)` \| `sparse.vstack(blocks)` | construct       | block-stacked sparse array    |
|   [5]   | `sparse.random_array(shape, density)`              | construct       | random sparse array           |
|   [6]   | `sparse.linalg.spsolve(A, b)`                      | direct solve    | sparse `A x = b` solution     |
|   [7]   | `sparse.linalg.splu(A)`                            | factorization   | sparse LU factor object       |
|   [8]   | `sparse.linalg.factorized(A)`                      | factorization   | reusable solve closure        |
|   [9]   | `sparse.linalg.cg(A, b, rtol, M)`                  | iterative solve | conjugate-gradient solution   |
|  [10]   | `sparse.linalg.gmres(A, b, restart)`               | iterative solve | GMRES solution                |
|  [11]   | `sparse.linalg.bicgstab(A, b, rtol)`               | iterative solve | BiCGSTAB solution             |
|  [12]   | `sparse.linalg.lsqr(A, b, atol, btol)`             | least squares   | sparse least-squares solution |
|  [13]   | `sparse.linalg.eigsh(A, k, which, sigma)`          | eigensolver     | symmetric sparse eigenpairs   |
|  [14]   | `sparse.linalg.svds(A, k, which)`                  | factorization   | sparse truncated SVD          |

[ENTRYPOINT_SCOPE]: `scipy.optimize` root-find and minimize
- rail: solvers

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]  | [RESULT]                           |
| :-----: | :---------------------------------------------------- | :-------------- | :--------------------------------- |
|   [1]   | `minimize(fun, x0, method, jac, bounds, constraints)` | minimize        | `OptimizeResult` local minimum     |
|   [2]   | `minimize_scalar(fun, bracket, bounds, method)`       | minimize        | scalar-function minimum            |
|   [3]   | `least_squares(fun, x0, jac, bounds, method)`         | least squares   | nonlinear least-squares solution   |
|   [4]   | `curve_fit(f, xdata, ydata, p0, bounds)`              | least squares   | fitted parameters and covariance   |
|   [5]   | `root(fun, x0, method, jac)`                          | root find       | vector-root `OptimizeResult`       |
|   [6]   | `root_scalar(f, method, bracket, x0)`                 | root find       | scalar-root result                 |
|   [7]   | `brentq(f, a, b, xtol, rtol)`                         | root find       | bracketed scalar root              |
|   [8]   | `newton(func, x0, fprime, tol)`                       | root find       | Newton/secant scalar root          |
|   [9]   | `fsolve(func, x0, fprime)`                            | root find       | hybrd vector root                  |
|  [10]   | `linprog(c, A_ub, b_ub, A_eq, b_eq, bounds)`          | linear program  | HiGHS LP solution                  |
|  [11]   | `milp(c, integrality, bounds, constraints)`           | integer program | mixed-integer LP solution          |
|  [12]   | `differential_evolution(func, bounds, strategy)`      | global optimize | stochastic global minimum          |
|  [13]   | `nnls(A, b, maxiter)`                                 | least squares   | nonnegative least-squares solution |
|  [14]   | `linear_sum_assignment(cost_matrix)`                  | assignment      | optimal row/column assignment      |

[ENTRYPOINT_SCOPE]: `scipy.integrate` and `scipy.interpolate`
- rail: solvers

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RESULT]                           |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------- |
|   [1]   | `integrate.quad(func, a, b, epsabs, epsrel)`                  | quadrature     | scalar integral plus error         |
|   [2]   | `integrate.dblquad(func, a, b, gfun, hfun)`                   | quadrature     | double integral                    |
|   [3]   | `integrate.nquad(func, ranges, opts)`                         | quadrature     | n-dimensional integral             |
|   [4]   | `integrate.quad_vec(f, a, b, epsabs, workers)`                | quadrature     | vector-valued integral             |
|   [5]   | `integrate.solve_ivp(fun, t_span, y0, method)`                | ODE integrator | initial-value ODE solution         |
|   [6]   | `integrate.odeint(func, y0, t)`                               | ODE integrator | LSODA ODE solution                 |
|   [7]   | `integrate.simpson(y, x, dx)`                                 | sampled rule   | Simpson-rule integral              |
|   [8]   | `integrate.trapezoid(y, x, dx)`                               | sampled rule   | trapezoidal integral               |
|   [9]   | `interpolate.interp1d(x, y, kind)`                            | interpolant    | callable 1-D interpolant           |
|  [10]   | `interpolate.CubicSpline(x, y, bc_type)`                      | interpolant    | C2 cubic-spline interpolant        |
|  [11]   | `interpolate.PchipInterpolator(x, y)`                         | interpolant    | shape-preserving cubic interpolant |
|  [12]   | `interpolate.make_interp_spline(x, y, k)`                     | interpolant    | `BSpline` interpolant              |
|  [13]   | `interpolate.griddata(points, values, xi)`                    | scattered      | unstructured-grid interpolation    |
|  [14]   | `interpolate.RegularGridInterpolator(points, values, method)` | grid           | regular-grid interpolant           |

## [4]-[IMPLEMENTATION_LAW]

[SOLVER_TOPOLOGY]:
- dense linear: `scipy.linalg` owns factorizations (`lu_factor`, `cholesky`, `qr`, `svd`), direct solve (`solve`, `inv`, `lstsq`, `pinv`), eigensolvers (`eig`, `eigh`), and matrix functions (`expm`).
- sparse storage: `scipy.sparse` owns the CSR/CSC/COO/DIA/BSR/LIL containers; construct from `diags`, `eye_array`, `kron`, or block stacks before solving.
- sparse solve: `scipy.sparse.linalg` owns direct solve (`spsolve`, `splu`, `factorized`), Krylov iterative solvers (`cg`, `gmres`, `bicgstab`, `lsqr`), and sparse eigen/SVD (`eigsh`, `svds`) over a matrix or `LinearOperator`.
- optimization: `scipy.optimize` owns minimization (`minimize`, `differential_evolution`), least squares (`least_squares`, `curve_fit`, `nnls`), root finding (`root`, `brentq`, `newton`, `fsolve`), and linear/integer programming (`linprog`, `milp`); constraints route through `Bounds`, `LinearConstraint`, and `NonlinearConstraint`; results carry an `OptimizeResult`.
- integration: `scipy.integrate` owns adaptive quadrature (`quad`, `dblquad`, `nquad`, `quad_vec`), ODE integrators (`solve_ivp`, `odeint`), and sampled rules (`simpson`, `trapezoid`).
- interpolation: `scipy.interpolate` owns 1-D interpolants (`interp1d`, `CubicSpline`, `PchipInterpolator`, `make_interp_spline`) and scattered/grid interpolation (`griddata`, `RegularGridInterpolator`).

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
