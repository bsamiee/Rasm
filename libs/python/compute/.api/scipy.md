# [PY_COMPUTE_API_SCIPY]

`scipy` supplies the dense/sparse linear algebra, optimization, integration, interpolation, and signal/statistics surfaces for the compute numeric-intent solver rail. The package owner routes each `NumericIntent` case onto a scipy submodule callable — `scipy.linalg.solve`, `scipy.sparse.linalg.spsolve`, `scipy.optimize.minimize`, `scipy.integrate.solve_ivp`, `scipy.interpolate.CubicSpline` — and captures tolerances and residuals as study evidence; it never re-implements a numeric routine scipy owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scipy`
- package: `scipy`
- import: `scipy` (lint alias `sp`); submodules `scipy.fft`, `scipy.linalg`, `scipy.sparse`, `scipy.sparse.linalg`, `scipy.optimize`, `scipy.integrate`, `scipy.interpolate`, `scipy.signal`, `scipy.stats` (`scipy.stats.qmc`), `scipy.spatial` (`scipy.spatial.distance`, `scipy.spatial.transform`)
- owner: `compute`
- rail: solvers
- capability: scientific solver suite — fast Fourier transforms, dense and sparse linear algebra, nonlinear optimization, numerical integration, interpolation, statistics (distributions, hypothesis tests, QMC sampling), signal processing, and spatial neighbour/tessellation/rotation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sparse containers and solver result types
- rail: solvers
- `ArpackNoConvergence` is raised by `eigsh`/`eigs`/`svds(solver='arpack')` on non-convergence within `maxiter`, carrying partial `.eigenvalues`/`.eigenvectors`.

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
|  [21]   | `scipy.sparse.linalg.ArpackNoConvergence` | ARPACK fault       | ARPACK non-convergence fault (partial eigenpairs; see lead)  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `scipy.fft` discrete Fourier and trigonometric transforms
- rail: transform

The pocketfft-backed transform family; `rfft`/`irfft` halve storage for real input, `fftn`/`ifftn` transform over the `axes` tuple, and `fftfreq`/`fftshift` build and centre the frequency grid for the analytic-signal and spectral routes. 1-D transforms share `(x, n, axis, norm)`, n-D forms take `(x, s, axes, norm)`, `dct`/`dst` add `type`, `fftfreq`/`rfftfreq` take `(n, d)` sample spacing, and `fftshift`/`ifftshift` take `(x, axes)`.

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
- rail: solvers
- routines take matrix `a`/`b` arguments; `solve` dispatches on `assume_a='pos'/'sym'/'her'/'gen'`, `qr` takes `mode`/`pivoting`, `svd` takes `full_matrices`/`compute_uv`, `eigh` takes `subset_by_index`, `pinv` takes `atol`/`rtol`, and `lstsq` takes `lapack_driver`. The control matrix equations `solve_sylvester`/`solve_continuous_are`/`solve_discrete_lyapunov` solve Sylvester / algebraic-Riccati / Lyapunov systems.

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

[ENTRYPOINT_SCOPE]: `scipy.sparse` construction and `scipy.sparse.linalg` solve
- rail: solvers
- construction surfaces are `sparse` members, solve/eigen surfaces `sparse.linalg` members. Krylov solvers share `(A, b, x0=None, *, rtol=1e-5, atol=0.0, maxiter=None, M=None, callback=None)`; `gmres` adds `restart`/`callback_type='pr_norm'/'x'`. `eigsh`/`eigs` take `(A, k=6, M=None, sigma=None, which='LM', ncv=None, maxiter=None, tol=0, OPinv=None, mode=...)` where `sigma`/`OPinv` drive shift-invert about the interior spectrum (`mode='normal'/'buckling'/'cayley'`) and `eigs` adds `OPpart='r'/'i'`; `lobpcg` takes `(A, X, B=None, M=None, Y=None, largest=True)` (`M` preconditioner, `B` mass matrix, `Y` constraint block); `svds` takes `(A, k=6, solver='arpack'/'lobpcg'/'propack', return_singular_vectors='u'/'vh'/True/False)`.

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
- rail: solvers
- local/least-squares/root routines take `(fun, x0, ...)` with `method`/`jac`/`bounds`/`constraints` where applicable; constraints route through `Bounds`/`LinearConstraint`/`NonlinearConstraint` and results carry an `OptimizeResult`. Global optimizers take `(func, bounds)`; the stochastic `differential_evolution`/`dual_annealing` carry the SPEC-007 `rng` keyword (`seed` the deprecated alias), and `differential_evolution` adds `workers` (process-parallel population), `polish` (L-BFGS-B refinement), and `strategy` (mutation policy); `shgo`/`direct`/`brute` are deterministic and carry no `rng`.

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
- rail: solvers
- quadrature/ODE surfaces are `integrate` members, interpolants `interpolate` members; adaptive quadrature takes `(func, a, b, ...)`, sampled rules take `(y, x, dx)`, interpolants take `(x, y, ...)`, and `solve_ivp` dispatches `method='RK45'/'DOP853'/'Radau'/'BDF'/'LSODA'`.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY]   | [RESULT]                                        |
| :-----: | :---------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `quad(epsabs, epsrel)` \| `quad_vec(workers)`               | quadrature       | adaptive scalar/vector integral + error         |
|  [02]   | `dblquad(gfun, hfun)` \| `tplquad` \| `nquad(ranges, opts)` | quadrature       | double / triple / n-dimensional integral        |
|  [03]   | `qmc_quad(n_estimates, qrng)` \| `tanhsinh` \| `nsum`       | quadrature       | QMC / tanh-sinh singular / series sum           |
|  [04]   | `solve_ivp(t_span, y0, method, events, jac)`                | ODE integrator   | initial-value ODE                               |
|  [05]   | `RK45` \| `DOP853` \| `Radau` \| `BDF` \| `LSODA`           | ODE solver class | `OdeSolver` classes for `solve_ivp(method=...)` |
|  [06]   | `solve_bvp(fun, bc, x, y)`                                  | BVP solver       | two-point boundary-value ODE solution           |
|  [07]   | `odeint(func, y0, t)`                                       | ODE integrator   | legacy LSODA ODE solution                       |
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
- rail: signal
- IIR designers take `(N, Wn, btype, output='sos')` (`iirfilter` adds `ftype`, `iirnotch` takes `(w0, Q)`); filter-apply takes `(b, a, x)`/`(sos, x)`; spectral estimators take `(x, fs, nperseg, ...)`; `windows.<name>(M)` builds `hann`/`hamming`/`blackman`/`kaiser`/`tukey`/`dpss` tapers.

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
- rail: statistics

Continuous (`norm`/`lognorm`/`gamma`/`beta`/`t`/`chi2`/`expon`/`uniform`/`weibull_min`/`triang(c, loc, scale)`/`truncnorm(a, b, loc, scale)`) and discrete (`binom`/`poisson`/`geom`/`nbinom`) distributions are frozen `rv_continuous`/`rv_discrete` instances exposing `.pdf`/`.pmf`/`.cdf`/`.ppf`/`.rvs`/`.fit`/`.stats`; the hypothesis tests return a named result carrying `statistic` and `pvalue`. For DOE marginals, `triang` shape `c` in [0, 1] is the mode as a fraction of `(loc, loc+scale)` and `truncnorm` shapes `a`/`b` are the standardized (pre-`loc`/`scale`) truncation bounds, so `ppf(c=peak, loc=start, scale=end-start)` / `ppf((lo-mean)/std, (hi-mean)/std, loc=mean, scale=std)`.

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
- rail: experiments

`Sobol`/`Halton`/`LatinHypercube` are `QMCEngine` subclasses whose `random(n)` draws a low-discrepancy sample on the unit hypercube; `scale` affinely maps the sample to the bounds box and `discrepancy` scores the sample uniformity. Symbols omit the `qmc` prefix; engines take `(d, scramble, rng, optimization)` and expose `.random(n)`, with `rng` the SPEC-007 randomness keyword (`seed` a deprecated interim alias).

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
- rail: spatial

`cKDTree` is the compiled KD-tree and `KDTree` is the Python subclass over it; `ConvexHull`/`Delaunay`/`Voronoi` are Qhull-backed tessellation carriers whose attributes expose simplices, vertices, and adjacency; `distance.cdist`/`pdist` are the pairwise distance matrix and condensed vector that `squareform` interconverts; `transform.Rotation` is the quaternion/matrix/Euler rotation carrier; `procrustes` is the optimal similarity alignment of two point sets. `HalfspaceIntersection.intersections` are the `(n_vertices, dim)` vertices of the intersection polytope and `ip` a strictly-feasible interior point (e.g. the `linprog` Chebyshev centre); `Rotation` constructs via `from_quat`/`from_matrix`/`from_euler`/`from_rotvec` and applies via `.apply`/`.as_matrix`/`.inv`/`.mean`.

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

[SOLVER_TOPOLOGY]:
- transform: `scipy.fft` owns the pocketfft DFT family (`fft`/`ifft`, `fftn`/`ifftn`, `rfft`/`irfft`), the trigonometric transforms (`dct`, `dst`), and the frequency-grid helpers (`fftfreq`, `fftshift`); the `analysis/transform.md#TRANSFORM` owner routes spectral cases here, and `scipy.signal.hilbert` composes `fft`/`ifft` to build the analytic signal. `scipy.fft` is Array-API-aware (dispatches on the backend `xp`), but `scipy.signal.hilbert`'s Array-API support is active (gated behind `SCIPY_ARRAY_API=1`, off by default) and jax-skipped (item assignment), so the `analysis/transform.md#TRANSFORM` analytic arm is numpy-resident — aligning with the `analysis/signal.md#DSP` numpy-floor stance for `scipy.signal`.
- dense linear: `scipy.linalg` owns factorizations (`lu_factor`, `cholesky`, `ldl`, `qr`, `svd`, `polar`, `schur`), direct/structured solve (`solve`, `solve_triangular`, `solve_banded`, `lstsq`, `pinv`), eigensolvers (`eig`, `eigh` with `subset_by_index`), matrix functions (`expm`, `logm`, `sqrtm`, `funm`), and control matrix equations (`solve_sylvester`, `solve_continuous_are`, `solve_discrete_lyapunov`).
- sparse storage: `scipy.sparse` owns the CSR/CSC/COO/DIA/BSR/LIL containers; construct from `diags_array`, `eye_array`, `kron`, or block stacks before solving.
- sparse solve: `scipy.sparse.linalg` owns direct solve (`spsolve`, `splu`, `spilu`, `factorized`), the full Krylov family (`cg`/`gmres`/`bicgstab`/`minres`/`qmr`/`tfqmr`/`lgmres`/`gcrotmk`, all sharing the `x0`, `rtol`, `atol`, `maxiter`, `M`, `callback` signature; `gmres` adds `restart`/`callback_type`), least squares (`lsqr`, `lsmr`), sparse eigen/SVD (`eigsh`/`eigs` ARPACK with `sigma`/`OPinv` shift-invert for interior spectra, `lobpcg` block-preconditioned for large SPD systems, `svds` truncated SVD over the `'arpack'`/`'lobpcg'`/`'propack'` solvers, non-convergence railing as `ArpackNoConvergence`), and matrix-free operator algebra (`LinearOperator`, `aslinearoperator`, `expm_multiply`, `onenormest`) — the matrix-free path feeds the FEM `solver_iter_krylov(M=...)` route in `.api/scikit-fem.md`, and the shift-invert `OPinv` operator is itself a `LinearOperator` wrapping a `factorized([A - sigma·M])` closure.
- optimization: `scipy.optimize` owns local minimization (`minimize` with a `BFGS`/`SR1` `HessianUpdateStrategy`), global search (`differential_evolution`, `dual_annealing`, `shgo`, `basinhopping`, `direct`), least squares (`least_squares`, `curve_fit`, `nnls`), root finding (`root`, `brentq`, `newton`, `fsolve`, `fixed_point`), and linear/integer programming (`linprog`, `milp`); constraints route through `Bounds`, `LinearConstraint`, `NonlinearConstraint`; gradients verify through `approx_fprime`/`check_grad`; results carry an `OptimizeResult`.
- integration: `scipy.integrate` owns adaptive quadrature (`quad`, `dblquad`, `tplquad`, `nquad`, `quad_vec`, `tanhsinh`, `qmc_quad`, `nsum`), initial-value ODE (`solve_ivp` dispatching `RK45`/`DOP853`/`Radau`/`BDF`/`LSODA` `OdeSolver` classes, `odeint` legacy), boundary-value ODE (`solve_bvp`), and sampled/cumulative rules (`simpson`, `trapezoid`, `romb`, `cumulative_trapezoid`, `cumulative_simpson`); `qmc_quad` stacks a `scipy.stats.qmc` engine into the quadrature node set.
- interpolation: `scipy.interpolate` owns 1-D interpolants (`interp1d`, `CubicSpline`, `Akima1DInterpolator`, `PchipInterpolator`, `make_interp_spline`, `UnivariateSpline`, `make_smoothing_spline`) and scattered/grid interpolation (`RBFInterpolator`, `NearestNDInterpolator`, `LinearNDInterpolator`, `CloughTocher2DInterpolator`, `griddata`, `RegularGridInterpolator`, `NdBSpline`).
- signal: `scipy.signal` owns IIR/FIR design (`butter`, `cheby1`, `cheby2`, `ellip`, `bessel`, `iirfilter`, `firwin`), zero-phase/causal filtering (`sosfiltfilt`, `filtfilt`, `sosfilt`, `lfilter`, `savgol_filter`), spectral estimation (`welch`, `csd`, `coherence`, `periodogram`), the modern `ShortTimeFFT`/`stft`/`istft` time-frequency surface, convolution (`fftconvolve`, `oaconvolve`), resampling (`resample_poly`, `decimate`), and peak analysis (`find_peaks`, `peak_widths`, `peak_prominences`); the `signal/dsp.md#DSP` owner routes the stationary cases here beside the `pywt` wavelet cases.
- statistics: `scipy.stats` owns the continuous/discrete distribution objects (`norm`, `lognorm`, `gamma`, `beta`, `t`, `chi2`, `binom`, `poisson`,...) with frozen `pdf`/`pmf`/`cdf`/`ppf`/`rvs`/`fit` (plus the array-API `make_distribution` object and bounded `fit`), parametric/rank hypothesis tests (`ttest_ind`, `f_oneway`, `mannwhitneyu`, `kruskal`, `wilcoxon`), goodness-of-fit (`ks_2samp`, `anderson`, `shapiro`), correlations (`pearsonr`, `spearmanr`, `kendalltau`, `linregress`), resampling inference (`bootstrap`, `permutation_test`, `monte_carlo_test`, all taking `rng`), density/divergence (`gaussian_kde`, `ecdf`, `wasserstein_distance`, `entropy`, `false_discovery_control`); the `numerics/statistics.md#STATISTICS` owner routes distribution and test cases here.
- quasi-Monte-Carlo: `scipy.stats.qmc` owns the low-discrepancy engines (`Sobol`, `Halton`, `LatinHypercube`) with `scale` to a bounds box and `discrepancy` scoring; the `experiments/study.md#STUDY` DOE sampler routes here.
- spatial: `scipy.spatial` owns KD-tree neighbour/radius search (`KDTree`/`cKDTree.query`/`query_ball_point`/`query_pairs`, `workers`-parallel), Qhull hull/tessellation (`ConvexHull`, `Delaunay`, `Voronoi`, `SphericalVoronoi`, `HalfspaceIntersection`), pairwise/condensed distance (`distance.cdist`, `distance.pdist`/`squareform`, `distance_matrix`), rotation algebra and interpolation (`transform.Rotation` with `align_vectors`/`mean`, `Slerp`, `RotationSpline`, `RigidTransform`), and point-set alignment (`procrustes`, `geometric_slerp`); the `spatial/query.md#SPATIAL` owner routes every geometry query here.

[LOCAL_ADMISSION]:
- import: submodule imports at boundary scope only; module-level import is banned by the manifest import policy.
- routing: `NumericIntent` dense-linear -> `scipy.linalg`; sparse-solve -> `scipy.sparse.linalg`; nonlinear-optimize -> `scipy.optimize`; integrate -> `scipy.integrate`; interpolate -> `scipy.interpolate`.
- evidence: each solve captures the route callable, the tolerance inputs, and the convergence/residual (the `OptimizeResult` flags or solver residual) as a study receipt.
- boundary: scipy results are offline study evidence; production substrate selection and benchmark claims stay in `Rasm.Compute`.

[RAIL_LAW]:
- Package: `scipy`
- Owns: discrete Fourier transforms, dense/sparse linear algebra, nonlinear optimization, numerical integration, interpolation, statistics (distributions, hypothesis tests, QMC), signal processing, and spatial query/tessellation/rotation for the numeric-intent rail
- Accept: a `NumericIntent` case routed to a scipy submodule callable (`scipy.fft`, `scipy.linalg`, `scipy.sparse.linalg`, `scipy.optimize`, `scipy.integrate`, `scipy.interpolate`, `scipy.signal`, `scipy.stats`, `scipy.spatial`) with captured tolerances and residuals
- Reject: hand-rolled numeric kernels scipy owns (DFT, distance, distribution sampling); wrapper-renames of solver callables; product benchmark claims
