# [PY_COMPUTE_API_SCIPY]

`scipy` owns the scientific numeric solver surface the compute numeric-intent rail routes onto. Each `NumericIntent` case binds one submodule callable and captures its tolerances and residuals as study evidence; scipy results are offline study evidence, and production substrate selection stays in `Rasm.Compute`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scipy`
- package: `scipy`
- module: `scipy` (lint alias `sp`)
- namespaces: `scipy.fft`, `scipy.linalg`, `scipy.sparse`, `scipy.sparse.linalg`, `scipy.optimize`, `scipy.integrate`, `scipy.interpolate`, `scipy.signal`, `scipy.stats` (`scipy.stats.qmc`), `scipy.spatial` (`scipy.spatial.distance`, `scipy.spatial.transform`)
- rail: numeric-intent solver
- capability: scientific solver suite — fast Fourier transforms, dense and sparse linear algebra, nonlinear optimization, numerical integration, interpolation, statistics (distributions, hypothesis tests, QMC sampling), signal processing, and spatial neighbour/tessellation/rotation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sparse containers and solver result types

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]      | [CAPABILITY]                                                 |
| :-----: | :---------------------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `scipy.sparse.csr_array`                  | sparse format      | compressed-sparse-row container                              |
|  [02]   | `scipy.sparse.csc_array`                  | sparse format      | compressed-sparse-column container                           |
|  [03]   | `scipy.sparse.coo_array`                  | sparse format      | coordinate-list container                                    |
|  [04]   | `scipy.sparse.dia_array`                  | sparse format      | diagonal-storage container                                   |
|  [05]   | `scipy.sparse.bsr_array`                  | sparse format      | block-sparse-row container                                   |
|  [06]   | `scipy.sparse.lil_array`                  | sparse format      | row-based incremental-build container                        |
|  [07]   | `scipy.sparse.linalg.LinearOperator`      | matvec abstraction | matrix-free linear operator                                  |
|  [08]   | `scipy.optimize.OptimizeResult`           | result carrier     | solution, success flag, and diagnostics                      |
|  [09]   | `scipy.optimize.Bounds`                   | constraint         | variable lower/upper bounds                                  |
|  [10]   | `scipy.optimize.LinearConstraint`         | constraint         | `lb <= A x <= ub` linear constraint                          |
|  [11]   | `scipy.optimize.NonlinearConstraint`      | constraint         | nonlinear `lb <= f(x) <= ub` constraint                      |
|  [12]   | `scipy.optimize.HessianUpdateStrategy`    | hessian carrier    | `BFGS`/`SR1` quasi-Newton Hessian strategy                   |
|  [13]   | `scipy.integrate.OdeSolver`               | ODE solver base    | `RK45`/`DOP853`/`Radau`/`BDF`/`LSODA` step-controllable base |
|  [14]   | `scipy.interpolate.BSpline`               | spline carrier     | knot/coefficient B-spline value                              |
|  [15]   | `scipy.spatial.KDTree`                    | neighbour search   | KD-tree; subclasses compiled `cKDTree`                       |
|  [16]   | `scipy.spatial.transform.Rotation`        | rotation carrier   | quaternion/matrix/Euler 3-D rotation                         |
|  [17]   | `scipy.stats.rv_continuous`               | distribution base  | continuous-distribution base class                           |
|  [18]   | `scipy.stats.rv_discrete`                 | distribution base  | discrete-distribution base class                             |
|  [19]   | `scipy.stats.Covariance`                  | covariance carrier | structured covariance for multivariate distributions         |
|  [20]   | `scipy.stats.qmc.QMCEngine`               | sampler base       | low-discrepancy engine base class                            |
|  [21]   | `scipy.sparse.linalg.ArpackNoConvergence` | ARPACK fault       | ARPACK non-convergence, partial eigenpairs                   |

- `ArpackNoConvergence`: raised by `eigsh`/`eigs`/`svds(solver='arpack')` past `maxiter`, carrying partial `.eigenvalues`/`.eigenvectors`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `scipy.fft` pocketfft discrete Fourier and trigonometric transforms
- carry: 1-D `(x, n, axis, norm)`; n-D `(x, s, axes, norm)`; `dct`/`dst` add `type`; `fftfreq`/`rfftfreq` `(n, d)`; `fftshift`/`ifftshift` `(x, axes)`.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY]      | [RESULT]                                       |
| :-----: | :-------------------------------------------------------- | :------------------ | :--------------------------------------------- |
|  [01]   | `fft`                                                     | forward DFT         | complex 1-D discrete Fourier transform         |
|  [02]   | `ifft`                                                    | inverse DFT         | complex inverse transform                      |
|  [03]   | `fftn`                                                    | forward DFT         | complex n-D transform over `axes`              |
|  [04]   | `ifftn`                                                   | inverse DFT         | complex n-D inverse transform                  |
|  [05]   | `rfft`                                                    | real forward DFT    | half-spectrum transform of real input          |
|  [06]   | `irfft`                                                   | real inverse DFT    | real signal from half-spectrum                 |
|  [07]   | `dct`                                                     | cosine transform    | discrete cosine transform                      |
|  [08]   | `dst`                                                     | sine transform      | discrete sine transform                        |
|  [09]   | `hfft` \| `ihfft`                                         | Hermitian DFT       | transform of a Hermitian-symmetric spectrum    |
|  [10]   | `dctn` \| `dstn` \| `fht(a, dln, mu)` \| `ifht`           | n-D / log transform | n-D cosine/sine transform, fast Hankel         |
|  [11]   | `fftfreq` \| `rfftfreq` \| `fftshift` \| `ifftshift`      | frequency grid      | sample frequencies and zero-frequency centring |
|  [12]   | `next_fast_len(n, real)` \| `set_workers` / `get_workers` | tuning              | optimal padding length + pocketfft worker pool |

[ENTRYPOINT_SCOPE]: `scipy.linalg` dense factorization and solve
- carry: `a`/`b` matrix args; `solve(assume_a='pos'/'sym'/'her'/'gen')`, `qr(mode, pivoting)`, `svd(full_matrices, compute_uv)`, `eigh(subset_by_index)`, `pinv(atol, rtol)`, `lstsq(lapack_driver)`.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]       | [RESULT]                                    |
| :-----: | :------------------------------------------------------------------ | :------------------- | :------------------------------------------ |
|  [01]   | `solve`                                                             | dense solve          | solution of `a x = b`                       |
|  [02]   | `solve_triangular` \| `solve_banded` \| `solveh_banded`             | structured solve     | triangular / banded / Hermitian solve       |
|  [03]   | `inv` \| `det` \| `pinv`                                            | dense solve          | inverse / determinant / pseudoinverse       |
|  [04]   | `lu_factor` \| `lu_solve`                                           | factorization        | LU factor and back-substitution             |
|  [05]   | `cholesky` \| `cho_factor` \| `cho_solve` \| `ldl`                  | factorization        | Cholesky / LDL^T factor and solve           |
|  [06]   | `qr` \| `rq` \| `polar` \| `schur` \| `cossin`                      | factorization        | QR / RQ / polar / Schur / CS decomposition  |
|  [07]   | `svd` \| `null_space` \| `orth`                                     | factorization        | SVD null-space / orthonormal range          |
|  [08]   | `eig` \| `eigh`                                                     | eigensolver          | general / symmetric (subset) eigenpairs     |
|  [09]   | `lstsq`                                                             | least squares        | minimum-norm least-squares fit              |
|  [10]   | `expm` \| `logm` \| `sqrtm` \| `funm`                               | matrix function      | matrix exp / log / sqrt / analytic function |
|  [11]   | `norm` \| `block_diag` \| `toeplitz` \| `circulant` \| `khatri_rao` | structured construct | matrix/vector norm + structured builders    |

- `solve_sylvester` \| `solve_continuous_are` \| `solve_discrete_lyapunov`: Sylvester / algebraic-Riccati / Lyapunov matrix-equation solve.

[ENTRYPOINT_SCOPE]: `scipy.sparse` construction and `scipy.sparse.linalg` solve
- Krylov carry: `(A, b, x0, *, rtol, atol, maxiter, M, callback)`; `gmres` adds `restart`, `callback_type='pr_norm'/'x'`.
- `eigsh`/`eigs(A, k, M, sigma, which, ncv, maxiter, tol, OPinv, mode)`: `sigma`/`OPinv` shift-invert the interior spectrum (`mode='normal'/'buckling'/'cayley'`), `eigs` adds `OPpart`; `lobpcg(A, X, B, M, Y, largest)` carries `M` preconditioner, `B` mass matrix, `Y` constraint block; `svds(A, k, solver='arpack'/'lobpcg'/'propack', return_singular_vectors)`.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY]        | [RESULT]                                 |
| :-----: | :---------------------------------------------------------------- | :-------------------- | :--------------------------------------- |
|  [01]   | `diags_array(diagonals, offsets, shape)`                          | construct             | banded sparse array                      |
|  [02]   | `eye_array(m, k, format)`                                         | construct             | sparse identity                          |
|  [03]   | `kron(A, B, format)`                                              | construct             | sparse Kronecker product                 |
|  [04]   | `hstack(blocks)` \| `vstack(blocks)`                              | construct             | block-stacked sparse array               |
|  [05]   | `random_array(shape, density)`                                    | construct             | random sparse array                      |
|  [06]   | `spsolve(A, b)`                                                   | direct solve          | sparse `A x = b` solution                |
|  [07]   | `splu(A)` \| `spilu(A, drop_tol, fill_factor)` \| `factorized(A)` | factorization         | sparse LU / ILU / reusable solve closure |
|  [08]   | `cg`                                                              | iterative solve       | SPD conjugate gradient                   |
|  [09]   | `gmres`                                                           | iterative solve       | restarted GMRES                          |
|  [10]   | `bicgstab` \| `minres` \| `qmr` \| `tfqmr`                        | iterative solve       | nonsymmetric/indefinite Krylov mirrors   |
|  [11]   | `lgmres` \| `gcrotmk`                                             | iterative solve       | augmented/recycling GMRES variants       |
|  [12]   | `lsqr` \| `lsmr`                                                  | least squares         | sparse least-squares (LSQR / LSMR)       |
|  [13]   | `eigsh`                                                           | symmetric eigensolver | ARPACK symmetric eigenpairs (`sigma`)    |
|  [14]   | `eigs`                                                            | general eigensolver   | ARPACK nonsymmetric eigenpairs           |
|  [15]   | `lobpcg`                                                          | block eigensolver     | preconditioned LOBPCG (large SPD)        |
|  [16]   | `svds`                                                            | truncated SVD         | `k` singular triplets                    |
|  [17]   | `aslinearoperator` \| `LinearOperator(shape, matvec)`             | operator algebra      | matrix-free operator construction        |
|  [18]   | `expm_multiply` \| `onenormest` \| `matrix_power`                 | operator algebra      | action-only `exp(A)·B`, 1-norm estimate  |

[ENTRYPOINT_SCOPE]: `scipy.optimize` root-find and minimize
- carry: local/least-squares/root `(fun, x0, method, jac, bounds, constraints)`, constraints via `Bounds`/`LinearConstraint`/`NonlinearConstraint`, results carry `OptimizeResult`.
- global `(func, bounds)`; stochastic `differential_evolution`/`dual_annealing` take `rng` (`differential_evolution` adds `workers`, `polish`, `strategy`); `shgo`/`direct`/`brute` are deterministic.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY]         | [RESULT]                                      |
| :-----: | :------------------------------------------------------------- | :--------------------- | :-------------------------------------------- |
|  [01]   | `minimize(fun, x0, method, jac, bounds, constraints)`          | minimize               | `OptimizeResult` local minimum                |
|  [02]   | `minimize_scalar(fun, bracket, bounds, method)`                | minimize               | scalar-function minimum                       |
|  [03]   | `least_squares(fun, x0, jac, bounds, method)`                  | least squares          | nonlinear least-squares solution              |
|  [04]   | `curve_fit(f, xdata, ydata, p0, bounds)`                       | least squares          | fitted parameters and covariance              |
|  [05]   | `root(fun, x0, method, jac)`                                   | root find              | vector-root `OptimizeResult`                  |
|  [06]   | `root_scalar(f, method, bracket, x0)`                          | root find              | scalar-root result                            |
|  [07]   | `brentq(f, a, b, xtol, rtol)`                                  | root find              | bracketed scalar root                         |
|  [08]   | `newton(func, x0, fprime, tol)`                                | root find              | Newton/secant scalar root                     |
|  [09]   | `fsolve(func, x0, fprime)`                                     | root find              | hybrd vector root                             |
|  [10]   | `linprog(c, A_ub, b_ub, A_eq, b_eq, bounds)`                   | linear program         | HiGHS LP solution                             |
|  [11]   | `milp(c, integrality, bounds, constraints)`                    | integer program        | mixed-integer LP solution                     |
|  [12]   | `differential_evolution` \| `dual_annealing` \| `basinhopping` | global (stochastic)    | stochastic global minimum (`rng`)             |
|  [13]   | `shgo` \| `direct` \| `brute`                                  | global (deterministic) | deterministic global minimum                  |
|  [14]   | `nnls(A, b, maxiter)`                                          | least squares          | nonnegative least-squares solution            |
|  [15]   | `linear_sum_assignment` \| `fixed_point`                       | assignment / fixed pt  | optimal assignment, fixed-point iterate       |
|  [16]   | `approx_fprime` \| `check_grad`                                | gradient util          | finite-diff Jacobian + gradient check         |
|  [17]   | `BFGS()` \| `SR1()` (`HessianUpdateStrategy`)                  | hessian strategy       | quasi-Newton Hessian for `minimize(hess=...)` |

[ENTRYPOINT_SCOPE]: `scipy.integrate` and `scipy.interpolate`
- carry: adaptive quadrature `(func, a, b)`, sampled rules `(y, x, dx)`, interpolants `(x, y)`; `solve_ivp(method='RK45'/'DOP853'/'Radau'/'BDF'/'LSODA')`.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY]   | [RESULT]                                        |
| :-----: | :---------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `quad(epsabs, epsrel)` \| `quad_vec(workers)`               | quadrature       | adaptive scalar/vector integral + error         |
|  [02]   | `dblquad(gfun, hfun)` \| `tplquad` \| `nquad(ranges, opts)` | quadrature       | double / triple / n-dimensional integral        |
|  [03]   | `qmc_quad(n_estimates, qrng)` \| `tanhsinh` \| `nsum`       | quadrature       | QMC / tanh-sinh singular / series sum           |
|  [04]   | `solve_ivp(t_span, y0, method, events, jac)`                | ODE integrator   | initial-value ODE                               |
|  [05]   | `RK45` \| `DOP853` \| `Radau` \| `BDF` \| `LSODA`           | ODE solver class | `OdeSolver` classes for `solve_ivp(method=...)` |
|  [06]   | `solve_bvp(fun, bc, x, y)`                                  | BVP solver       | two-point boundary-value ODE solution           |
|  [07]   | `odeint(func, y0, t)`                                       | ODE integrator   | LSODA ODE solution (functional interface)       |
|  [08]   | `simpson` \| `trapezoid` \| `romb`                          | sampled rule     | Simpson / trapezoidal / Romberg integral        |
|  [09]   | `cumulative_trapezoid` \| `cumulative_simpson`              | cumulative       | running antiderivative array                    |
|  [10]   | `interp1d`                                                  | 1-D interpolant  | callable interpolant                            |
|  [11]   | `CubicSpline`                                               | 1-D interpolant  | C2 cubic spline                                 |
|  [12]   | `Akima1DInterpolator`                                       | 1-D interpolant  | Akima interpolant                               |
|  [13]   | `PchipInterpolator`                                         | 1-D interpolant  | shape-preserving cubic                          |
|  [14]   | `make_interp_spline` \| `make_smoothing_spline`             | spline fit       | `BSpline` interpolant / smoothing spline        |
|  [15]   | `UnivariateSpline` \| `splrep`/`splev`                      | spline fit       | FITPACK smoothing / B-spline rep+eval           |
|  [16]   | `RBFInterpolator`                                           | scattered N-D    | radial-basis interpolation                      |
|  [17]   | `NearestNDInterpolator`                                     | scattered N-D    | nearest interpolation                           |
|  [18]   | `LinearNDInterpolator`                                      | scattered N-D    | linear interpolation                            |
|  [19]   | `CloughTocher2DInterpolator`                                | scattered N-D    | C1 scattered interpolation                      |
|  [20]   | `griddata(points, values, xi, method)`                      | scattered        | unstructured-grid interp (wraps ND)             |
|  [21]   | `RegularGridInterpolator` \| `NdBSpline(t, c, k)`           | grid             | regular-grid / tensor-product N-D B-spline      |

[ENTRYPOINT_SCOPE]: `scipy.signal` filter design, spectral estimation, and resampling
- carry: IIR design `(N, Wn, btype, output='sos')` (`iirfilter` adds `ftype`, `iirnotch(w0, Q)`); filter-apply `(b, a, x)`/`(sos, x)`; spectral `(x, fs, nperseg)`; `windows.<name>(M)` builds `hann`/`hamming`/`kaiser`/`tukey`/`dpss` tapers.

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY]     | [RESULT]                             |
| :-----: | :----------------------------------------------------------------------- | :----------------- | :----------------------------------- |
|  [01]   | `butter` \| `cheby1` \| `cheby2` \| `ellip` \| `bessel`                  | IIR design         | IIR SOS or b,a coefficients          |
|  [02]   | `iirfilter(ftype)` \| `iirnotch(w0, Q)`                                  | IIR design         | generic IIR / notch design           |
|  [03]   | `firwin(numtaps, cutoff, window, pass_zero)`                             | FIR design         | windowed-FIR tap coefficients        |
|  [04]   | `sosfiltfilt` \| `filtfilt` \| `sosfilt` \| `lfilter` \| `savgol_filter` | filter apply       | zero-phase / causal / Savitzky-Golay |
|  [05]   | `welch` \| `csd` \| `coherence` \| `periodogram`                         | spectral estimate  | PSD / CSD / coherence / periodogram  |
|  [06]   | `ShortTimeFFT` \| `stft` \| `istft` \| `spectrogram`                     | time-frequency     | STFT / spectrogram `(f, t, Sxx)`     |
|  [07]   | `convolve` \| `correlate` \| `fftconvolve` \| `oaconvolve`               | convolution        | direct / FFT / overlap-add convolve  |
|  [08]   | `resample_poly` \| `decimate` \| `detrend`                               | resample/condition | resample / decimate / detrend        |
|  [09]   | `find_peaks` \| `peak_widths` \| `peak_prominences`                      | peak detect        | peak indices, widths, prominences    |
|  [10]   | `hilbert(x, N, axis)` \| `hilbert2(x)`                                   | analytic signal    | complex analytic signal (FFT-backed) |
|  [11]   | `windows.<name>(M)`                                                      | window             | tapering windows for FIR/STFT        |

[ENTRYPOINT_SCOPE]: `scipy.stats` distributions and hypothesis tests
- distributions: continuous (`norm`/`lognorm`/`gamma`/`beta`/`t`/`chi2`/`expon`/`uniform`/`weibull_min`/`triang(c, loc, scale)`/`truncnorm(a, b, loc, scale)`) and discrete (`binom`/`poisson`/`geom`/`nbinom`) are frozen `rv_continuous`/`rv_discrete` instances exposing `.pdf`/`.pmf`/`.cdf`/`.ppf`/`.rvs`/`.fit`/`.stats`; tests return `statistic` and `pvalue`.
- DOE marginals: `triang` shape `c` in [0, 1] is the mode fraction of `(loc, loc+scale)`, `truncnorm` `a`/`b` are standardized (pre-`loc`/`scale`) truncation bounds — `ppf(c=peak, loc=start, scale=end-start)` / `ppf((lo-mean)/std, (hi-mean)/std, loc=mean, scale=std)`.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]         | [RESULT]                                       |
| :-----: | :------------------------------------------------------------ | :--------------------- | :--------------------------------------------- |
|  [01]   | `.fit(data)` \| `fit(dist, ...)` \| `make_distribution`       | MLE fit / array-API    | MLE fit, bounded fit, array-API dist object    |
|  [02]   | `ks_2samp` \| `anderson` \| `shapiro`                         | goodness-of-fit        | KS / Anderson-Darling / Shapiro-Wilk           |
|  [03]   | `ttest_ind` \| `ttest_rel` \| `f_oneway`                      | parametric test        | t-test / paired-t / ANOVA                      |
|  [04]   | `wilcoxon` \| `mannwhitneyu` \| `kruskal`                     | rank test              | Wilcoxon / Mann-Whitney / Kruskal-Wallis       |
|  [05]   | `pearsonr` \| `spearmanr` \| `kendalltau` \| `linregress`     | correlation            | linear / rank correlation, OLS regression      |
|  [06]   | `bootstrap` \| `permutation_test` \| `monte_carlo_test`       | resampling inference   | CI / exchangeability / parametric MC (`rng`)   |
|  [07]   | `gaussian_kde` \| `multivariate_normal`                       | density / multivariate | KDE, multivariate-normal                       |
|  [08]   | `ecdf` \| `Covariance.from_*`                                 | empirical / covariance | empirical CDF, structured covariance carrier   |
|  [09]   | `wasserstein_distance` \| `entropy` \| `differential_entropy` | divergence             | optimal-transport distance, (relative) entropy |
|  [10]   | `false_discovery_control`                                     | multiple-test          | Benjamini-Hochberg FDR control                 |

[ENTRYPOINT_SCOPE]: `scipy.stats.qmc` quasi-Monte-Carlo sampling

`Sobol`/`Halton`/`LatinHypercube` are `QMCEngine` subclasses; engines take `(d, scramble, rng, optimization)` and expose `.random(n)` on the unit hypercube. Symbols omit the `qmc` prefix.

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]   | [RESULT]                                          |
| :-----: | :------------------------------------------ | :--------------- | :------------------------------------------------ |
|  [01]   | `Sobol(bits)` -> `.random_base2(m)`         | QMC engine       | scrambled Sobol low-discrepancy sample            |
|  [02]   | `Halton`                                    | QMC engine       | Halton low-discrepancy sample                     |
|  [03]   | `LatinHypercube(strength)`                  | QMC engine       | Latin-hypercube (orthogonal-array `strength=2`)   |
|  [04]   | `PoissonDisk(radius)` -> `.fill_space()`    | QMC engine       | blue-noise minimum-distance sample                |
|  [05]   | `MultivariateNormalQMC` \| `MultinomialQMC` | QMC engine       | low-discrepancy MV-normal / multinomial draw      |
|  [06]   | `scale(sample, l_bounds, u_bounds)`         | affine map       | sample scaled to the bounds box                   |
|  [07]   | `discrepancy(sample, method)`               | uniformity score | quality metric (`'CD'`/`'WD'`/`'MD'`/`'L2-star'`) |

[ENTRYPOINT_SCOPE]: `scipy.spatial` neighbour search, hull, and tessellation

`cKDTree` is the compiled KD-tree, `KDTree` the Python subclass; `ConvexHull`/`Delaunay`/`Voronoi` are Qhull-backed. `distance.cdist`/`pdist` yield the pairwise matrix / condensed vector `squareform` interconverts. `Rotation` constructs via `from_quat`/`from_matrix`/`from_euler`/`from_rotvec` and applies via `.apply`/`.as_matrix`/`.inv`/`.mean`. `HalfspaceIntersection(halfspaces, ip)` needs `ip`, a strictly-feasible interior point such as the `linprog` Chebyshev centre; `.intersections` are the `(n_vertices, dim)` polytope vertices.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]     | [RESULT]                                       |
| :-----: | :--------------------------------------------------------------- | :----------------- | :--------------------------------------------- |
|  [01]   | `KDTree(data)` \| `cKDTree(data)` -> `.query(x, k, workers)`     | neighbour search   | k-nearest indices and distances                |
|  [02]   | `cKDTree.query_ball_point` \| `query_pairs` \| `query_ball_tree` | radius search      | within-radius / self pairs / cross-tree pairs  |
|  [03]   | `ConvexHull(points)` -> `.simplices`/`.volume`/`.area`           | hull               | facet simplices, hull volume/area              |
|  [04]   | `HalfspaceIntersection(halfspaces, ip)` -> `.intersections`      | hull               | intersection-polytope vertices                 |
|  [05]   | `Delaunay(points)` -> `.simplices`/`.find_simplex(p)`            | triangulation      | simplex table + barycentric location           |
|  [06]   | `Voronoi(points)` \| `SphericalVoronoi(points, radius)`          | tessellation       | planar / spherical Voronoi vertices+regions    |
|  [07]   | `distance.cdist(XA, XB, metric)` \| `distance_matrix(x, y, p)`   | distance matrix    | pairwise / Minkowski distance matrix           |
|  [08]   | `distance.pdist(X, metric)` \| `distance.squareform(Y)`          | distance vector    | condensed distance vector / matrix form        |
|  [09]   | `Rotation`                                                       | rotation carrier   | 3-D rotation algebra (mean, inversion)         |
|  [10]   | `Slerp` \| `RotationSpline`                                      | rotation interp    | slerp / spline rotation interpolation          |
|  [11]   | `RigidTransform` \| `Rotation.align_vectors`                     | rigid / fit        | rigid transform, Kabsch alignment              |
|  [12]   | `procrustes(data1, data2)` \| `geometric_slerp(start, end, t)`   | alignment / interp | similarity alignment + disparity, sphere slerp |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each `scipy` submodule owns one numeric domain, and the numeric-intent owner routes a `NumericIntent` case to exactly one submodule callable; the `[02]`/`[03]` tables own the member rosters.
- `scipy.fft` is Array-API-aware (dispatches on the backend `xp`); `scipy.signal.hilbert` gates Array-API behind `SCIPY_ARRAY_API=1` (off by default) and skips jax under item assignment, so the analytic-signal arm is numpy-resident beside the `scipy.signal` numpy floor.

[STACKING]:
- `scikit-fem`(`.api/scikit-fem.md`): FEM assembly emits a `scipy.sparse` matrix and its solver factories return `scipy.sparse.linalg` callables — `solver_iter_krylov(M=...)` consumes a `LinearOperator` preconditioner, and scipy owns the sparse direct/Krylov/eigen solve under the FEM dispatcher.
- numeric-intent owner: `integrate.qmc_quad` folds a `scipy.stats.qmc` engine into its quadrature nodes; the shift-invert `OPinv` is a `LinearOperator` wrapping a `factorized([A - sigma·M])` closure; `signal.hilbert` composes `fft`/`ifft`; `expm_multiply` takes the action-only `exp(A)·B` path over a `LinearOperator`.

[LOCAL_ADMISSION]:
- import: submodule imports at boundary scope only.
- routing: `NumericIntent` dense-linear -> `scipy.linalg`; sparse-solve -> `scipy.sparse.linalg`; nonlinear-optimize -> `scipy.optimize`; integrate -> `scipy.integrate`; interpolate -> `scipy.interpolate`.
- evidence: each solve captures the route callable, the tolerance inputs, and the convergence/residual (`OptimizeResult` flags or solver residual) as a study receipt.
- boundary: scipy results are offline study evidence; production substrate selection and benchmark claims stay in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `scipy`
- Owns: discrete Fourier transforms, dense/sparse linear algebra, nonlinear optimization, numerical integration, interpolation, statistics (distributions, hypothesis tests, QMC), signal processing, and spatial query/tessellation/rotation for the numeric-intent rail
- Accept: a `NumericIntent` case routed to a scipy submodule callable with captured tolerances and residuals
- Reject: hand-rolled numeric kernels scipy owns (DFT, distance, distribution sampling); wrapper-renames of solver callables; product benchmark claims
