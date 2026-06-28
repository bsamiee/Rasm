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
|  [12]   | `scipy.optimize.HessianUpdateStrategy` | hessian carrier  | `BFGS`/`SR1` quasi-Newton Hessian strategy |
|  [13]   | `scipy.integrate.OdeSolver`          | ODE solver base    | `RK45`/`DOP853`/`Radau`/`BDF`/`LSODA` step-controllable base |
|  [14]   | `scipy.interpolate.BSpline`          | spline carrier     | knot/coefficient B-spline value         |
|  [15]   | `scipy.spatial.KDTree`               | neighbour search   | KD-tree; subclasses compiled `cKDTree`  |
|  [16]   | `scipy.spatial.transform.Rotation`   | rotation carrier   | quaternion/matrix/Euler 3-D rotation    |
|  [17]   | `scipy.stats.rv_continuous`          | distribution base  | continuous-distribution base class      |
|  [18]   | `scipy.stats.rv_discrete`            | distribution base  | discrete-distribution base class        |
|  [19]   | `scipy.stats.Covariance`             | covariance carrier | structured covariance for multivariate distributions |
|  [20]   | `scipy.stats.qmc.QMCEngine`          | sampler base       | low-discrepancy engine base class       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `scipy.fft` discrete Fourier and trigonometric transforms
- rail: transform

The pocketfft-backed transform family; `rfft`/`irfft` halve storage for real input, `fftn`/`ifftn` transform over the `axes` tuple, and `fftfreq`/`fftshift` build and centre the frequency grid for the analytic-signal and spectral routes.

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY]   | [RESULT]                               |
| :-----: | :---------------------------- | :--------------- | :------------------------------------- |
|  [01]   | `fft(x, n, axis, norm)`       | forward DFT      | complex 1-D discrete Fourier transform |
|  [02]   | `ifft(x, n, axis, norm)`      | inverse DFT      | complex inverse transform              |
|  [03]   | `fftn(x, s, axes, norm)`      | forward DFT      | complex n-D transform over `axes`      |
|  [04]   | `ifftn(x, s, axes, norm)`     | inverse DFT      | complex n-D inverse transform          |
|  [05]   | `rfft(x, n, axis, norm)`      | real forward DFT | half-spectrum transform of real input  |
|  [06]   | `irfft(x, n, axis, norm)`     | real inverse DFT | real signal from half-spectrum         |
|  [07]   | `dct(x, type, n, axis, norm)` | cosine transform | discrete cosine transform              |
|  [08]   | `dst(x, type, n, axis, norm)` | sine transform   | discrete sine transform                |
|  [09]   | `hfft(x, n, axis)` \| `ihfft(x, n, axis)` | Hermitian DFT | transform of a Hermitian-symmetric spectrum |
|  [10]   | `dctn(x, type, axes)` \| `dstn(x, type, axes)` \| `fht(a, dln, mu)` \| `ifht(...)` | n-D / log transform | n-D cosine/sine transform, fast Hankel transform |
|  [11]   | `fftfreq(n, d)` \| `rfftfreq(n, d)` \| `fftshift(x, axes)` \| `ifftshift(x, axes)` | frequency grid | sample frequencies and zero-frequency centring |
|  [12]   | `next_fast_len(n, real)` \| `set_workers(workers)` / `get_workers()` | tuning | optimal padding length and worker-thread pool for the pocketfft backend |

[ENTRYPOINT_SCOPE]: `scipy.linalg` dense factorization and solve
- rail: solvers

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]  | [RESULT]                        |
| :-----: | :--------------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `solve(a, b, assume_a)`                  | dense solve     | solution of `a x = b` (`assume_a='pos'/'sym'/'her'/'gen'` dispatch) |
|  [02]   | `solve_triangular(a, b, lower, trans)` \| `solve_banded(l_and_u, ab, b)` \| `solveh_banded(ab, b)` | structured solve | triangular / banded / Hermitian-banded back-substitution |
|  [03]   | `inv(a)` \| `det(a)` \| `pinv(a, atol, rtol)` | dense solve | inverse, determinant, Moore-Penrose pseudoinverse |
|  [04]   | `lu_factor(a)` \| `lu_solve(lu_piv, b)`  | factorization   | LU factor and back-substitution |
|  [05]   | `cholesky(a, lower)` \| `cho_factor(a)` \| `cho_solve(c_low, b)` \| `ldl(a)` | factorization | Cholesky / LDL^T factor and solve |
|  [06]   | `qr(a, mode, pivoting)` \| `rq(a)` \| `polar(a)` \| `schur(a)` \| `cossin(x, p, q)` | factorization | QR / RQ / polar / Schur / CS decomposition |
|  [07]   | `svd(a, full_matrices, compute_uv)` \| `null_space(A)` \| `orth(A)` | factorization | SVD and SVD-based null-space / orthonormal range |
|  [08]   | `eig(a, b, left, right)` \| `eigh(a, b, eigvals_only, subset_by_index)` | eigensolver | general / symmetric (subset) eigenvalues+vectors |
|  [09]   | `lstsq(a, b, lapack_driver)`             | least squares   | minimum-norm least-squares fit  |
|  [10]   | `expm(A)` \| `logm(A)` \| `sqrtm(A)` \| `funm(A, func)` | matrix function | matrix exponential / log / sqrt / arbitrary analytic function |
|  [11]   | `norm(a, ord, axis)` \| `block_diag(*arrs)` \| `toeplitz(c, r)` \| `circulant(c)` \| `khatri_rao(a, b)` | structured construct | matrix/vector norm and structured-matrix builders |
|  [12]   | `solve_sylvester(a, b, q)` \| `solve_continuous_are(a, b, q, r)` \| `solve_discrete_lyapunov(a, q)` | control eqn | Sylvester / algebraic-Riccati / Lyapunov matrix-equation solve |

[ENTRYPOINT_SCOPE]: `scipy.sparse` construction and `scipy.sparse.linalg` solve
- rail: solvers

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]  | [RESULT]                      |
| :-----: | :------------------------------------------------- | :-------------- | :---------------------------- |
|  [01]   | `sparse.diags_array(diagonals, offsets, shape)`    | construct       | banded sparse array           |
|  [02]   | `sparse.eye_array(m, k, format)`                   | construct       | sparse identity               |
|  [03]   | `sparse.kron(A, B, format)`                        | construct       | sparse Kronecker product      |
|  [04]   | `sparse.hstack(blocks)` \| `sparse.vstack(blocks)` | construct       | block-stacked sparse array    |
|  [05]   | `sparse.random_array(shape, density)`              | construct       | random sparse array           |
|  [06]   | `sparse.linalg.spsolve(A, b)`                      | direct solve    | sparse `A x = b` solution     |
|  [07]   | `sparse.linalg.splu(A)` \| `spilu(A, drop_tol, fill_factor)` \| `factorized(A)` | factorization | sparse LU / incomplete-LU / reusable solve closure |
|  [08]   | `sparse.linalg.cg(A, b, x0=None, *, rtol=1e-5, atol=0.0, maxiter=None, M=None, callback=None)` | iterative solve | symmetric-positive-definite conjugate gradient |
|  [09]   | `sparse.linalg.gmres(A, b, x0=None, *, rtol=1e-5, atol=0.0, restart=None, maxiter=None, M=None, callback=None, callback_type=None)` | iterative solve | restarted GMRES; `callback_type='pr_norm'/'x'` selects residual vs iterate callback |
|  [10]   | `sparse.linalg.bicgstab(A, b, x0=None, *, rtol, atol, maxiter, M, callback)` \| `minres(A, b, ..., shift)` \| `qmr` \| `tfqmr` | iterative solve | nonsymmetric/indefinite Krylov mirrors (same `rtol`/`atol`/`maxiter`/`M`/`callback` signature) |
|  [11]   | `sparse.linalg.lgmres(A, b, ...)` \| `gcrotmk(A, b, ...)`         | iterative solve | augmented/recycling GMRES variants for slow-converging systems |
|  [12]   | `sparse.linalg.lsqr(A, b, atol, btol)` \| `lsmr(A, b, atol, btol)` | least squares  | sparse least-squares (LSQR / LSMR) solution |
|  [13]   | `sparse.linalg.eigsh(A, k, which, sigma)` \| `eigs(A, k, which, sigma)` \| `svds(A, k, which)` | eigensolver | symmetric / general sparse eigenpairs and truncated SVD (ARPACK) |
|  [14]   | `sparse.linalg.aslinearoperator(A)` \| `LinearOperator(shape, matvec)` \| `expm_multiply(A, B)` \| `onenormest(A)` \| `matrix_power(A, p)` | operator algebra | matrix-free operator construction, action-only `exp(A)·B`, 1-norm estimate |

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
|  [12]   | `differential_evolution(func, bounds, *, rng, workers, polish, strategy, ...)` \| `dual_annealing(func, bounds, *, rng, ...)` \| `shgo(func, bounds)` \| `basinhopping(func, x0)` \| `direct(func, bounds)` \| `brute(func, ranges)` | global optimize | stochastic / deterministic global minimum; the stochastic `differential_evolution`/`dual_annealing` carry the SPEC-007 `rng` reproducibility keyword (`seed` the deprecated interim alias, per the QMC/resampling convention), and `differential_evolution` additionally exposes `workers` (process-parallel population evaluation), `polish` (L-BFGS-B incumbent refinement), and `strategy` (the mutation policy altering the search walk); `shgo`/`direct`/`brute` are deterministic and carry no `rng` |
|  [13]   | `nnls(A, b, maxiter)`                                 | least squares   | nonnegative least-squares solution |
|  [14]   | `linear_sum_assignment(cost_matrix)` \| `fixed_point(func, x0)` \| `approx_fprime(xk, f)` \| `check_grad(func, grad, x0)` | assignment / fixed point / gradient | optimal assignment, fixed-point iterate, finite-difference Jacobian and gradient check |
|  [15]   | `BFGS()` \| `SR1()` (`HessianUpdateStrategy`)         | hessian strategy | quasi-Newton Hessian approximation passed to `minimize(hess=...)` |

[ENTRYPOINT_SCOPE]: `scipy.integrate` and `scipy.interpolate`
- rail: solvers

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RESULT]                           |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------- |
|  [01]   | `integrate.quad(func, a, b, epsabs, epsrel)` \| `quad_vec(f, a, b, workers)` | quadrature | adaptive scalar / vector-valued integral plus error |
|  [02]   | `integrate.dblquad(func, a, b, gfun, hfun)` \| `tplquad(...)` \| `nquad(func, ranges, opts)` | quadrature | double / triple / n-dimensional integral |
|  [03]   | `integrate.qmc_quad(func, a, b, n_estimates, qrng)` \| `tanhsinh(f, a, b)` \| `nsum(f, a, b)` | quadrature | QMC integral, tanh-sinh singular-endpoint quadrature, infinite series sum |
|  [04]   | `integrate.solve_ivp(fun, t_span, y0, method, dense_output, events, jac)` | ODE integrator | initial-value ODE; `method='RK45'/'DOP853'/'Radau'/'BDF'/'LSODA'` |
|  [05]   | `integrate.RK45` \| `DOP853` \| `Radau` \| `BDF` \| `LSODA` (subclasses of `OdeSolver`) | ODE solver class | step-controllable OOP integrator for `solve_ivp(method=...)` |
|  [06]   | `integrate.solve_bvp(fun, bc, x, y)`                          | BVP solver     | two-point boundary-value ODE solution |
|  [07]   | `integrate.odeint(func, y0, t)`                               | ODE integrator | legacy LSODA ODE solution          |
|  [08]   | `integrate.simpson(y, x, dx)` \| `trapezoid(y, x, dx)` \| `romb(y, dx)` | sampled rule | Simpson / trapezoidal / Romberg integral |
|  [09]   | `integrate.cumulative_trapezoid(y, x)` \| `cumulative_simpson(y, x)`     | cumulative   | running antiderivative array       |
|  [10]   | `interpolate.interp1d(x, y, kind)` \| `CubicSpline(x, y, bc_type)` \| `Akima1DInterpolator(x, y)` \| `PchipInterpolator(x, y)` | 1-D interpolant | callable / C2-spline / Akima / shape-preserving cubic |
|  [11]   | `interpolate.make_interp_spline(x, y, k)` \| `make_smoothing_spline(x, y, lam)` \| `UnivariateSpline(x, y, s)` \| `splrep`/`splev` | spline fit | `BSpline` interpolant / smoothing spline / FITPACK B-spline |
|  [12]   | `interpolate.RBFInterpolator(y, d, kernel)` \| `NearestNDInterpolator` \| `LinearNDInterpolator` \| `CloughTocher2DInterpolator` | scattered N-D | radial-basis / nearest / linear / C1 scattered interpolation |
|  [13]   | `interpolate.griddata(points, values, xi, method)`            | scattered      | unstructured-grid interpolation (wraps the ND interpolators) |
|  [14]   | `interpolate.RegularGridInterpolator(points, values, method)` \| `NdBSpline(t, c, k)` | grid | regular-grid interpolant / tensor-product N-D B-spline |

[ENTRYPOINT_SCOPE]: `scipy.signal` filter design, spectral estimation, and resampling
- rail: signal

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]    | [RESULT]                             |
| :-----: | :-------------------------------------------- | :---------------- | :----------------------------------- |
|  [01]   | `butter(N, Wn, btype, output='sos')` \| `cheby1` \| `cheby2` \| `ellip` \| `bessel` \| `iirfilter(N, Wn, ftype)` \| `iirnotch(w0, Q)` | IIR design | Butterworth/Chebyshev/elliptic/Bessel/notch SOS or b,a coefficients |
|  [02]   | `firwin(numtaps, cutoff, window, pass_zero)` | FIR design        | windowed-FIR tap coefficients        |
|  [03]   | `sosfiltfilt(sos, x, axis)` \| `filtfilt(b, a, x)` \| `sosfilt(sos, x)` \| `lfilter(b, a, x)` \| `savgol_filter(x, w, p)` | filter apply | zero-phase / causal / Savitzky-Golay filtering |
|  [04]   | `welch(x, fs, nperseg, noverlap)` \| `csd(x, y, fs)` \| `coherence(x, y, fs)` \| `periodogram(x, fs)` | spectral estimate | PSD / cross-PSD / coherence / periodogram |
|  [05]   | `ShortTimeFFT(win, hop, fs)` \| `stft(x, fs)` \| `istft(Zxx, fs)` \| `spectrogram(x, fs)` | time-frequency | STFT object / forward-inverse STFT / spectrogram `(f, t, Sxx)` |
|  [06]   | `convolve(in1, in2, mode)` \| `correlate(in1, in2)` \| `fftconvolve(in1, in2)` \| `oaconvolve(in1, in2)` | convolution | direct / FFT / overlap-add convolution and correlation |
|  [07]   | `resample_poly(x, up, down, axis)` \| `decimate(x, q)` \| `detrend(x, type)` | resample/condition | polyphase resample / decimation / detrend |
|  [08]   | `find_peaks(x, height, distance, prominence)` \| `peak_widths(x, peaks)` \| `peak_prominences(x, peaks)` | peak detect | peak indices, widths, and prominences |
|  [09]   | `hilbert(x, N, axis)` \| `hilbert2(x)`        | analytic signal   | complex analytic signal (FFT-backed; `scipy.fft.fft`/`ifft` composed; Array-API experimental, jax-skipped) |
|  [10]   | `windows.<name>(M)` (`hann`, `hamming`, `blackman`, `kaiser`, `tukey`, `dpss`) | window | tapering windows for FIR design and STFT |

[ENTRYPOINT_SCOPE]: `scipy.stats` distributions and hypothesis tests
- rail: statistics

Continuous (`norm`, `lognorm`, `gamma`, `beta`, `t`, `chi2`, `expon`, `uniform`, `weibull_min`) and discrete (`binom`, `poisson`, `geom`, `nbinom`) distributions are frozen `rv_continuous`/`rv_discrete` instances exposing `.pdf`/`.pmf`, `.cdf`, `.ppf`, `.rvs`, `.fit`, and `.stats`; the hypothesis tests return a named result carrying `statistic` and `pvalue`.

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY]  | [RESULT]                                         |
| :-----: | :----------------------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `norm` / `lognorm` / `gamma` / `beta` / `t` / `chi2` / `expon` / `weibull_min` / `triang(c, loc, scale)` / `truncnorm(a, b, loc, scale)` | continuous dist | frozen distribution with `pdf`/`cdf`/`ppf`/`rvs`; `triang` shape `c` in [0, 1] is the mode as a fraction of `(loc, loc+scale)`, `truncnorm` shapes `a`/`b` are the standardized (pre-`loc`/`scale`) truncation bounds, so a DOE marginal maps `ppf(c=peak, loc=start, scale=end-start)` / `ppf((lo-mean)/std, (hi-mean)/std, loc=mean, scale=std)` |
|  [02]   | `binom` / `poisson` / `geom` / `nbinom`                                        | discrete dist   | frozen distribution with `pmf`/`cdf`/`rvs`       |
|  [03]   | `<dist>.fit(data)` \| `fit(dist, data, bounds)` \| `make_distribution(dist)`    | MLE fit / array-API | MLE shape/loc/scale; bounded optimisation fit; modern array-API distribution object |
|  [04]   | `ks_2samp(d1, d2)` \| `anderson(x, dist)` \| `shapiro(x)`                       | goodness-of-fit | Kolmogorov-Smirnov / Anderson-Darling / Shapiro-Wilk normality |
|  [05]   | `ttest_ind(a, b)` \| `ttest_rel(a, b)` \| `f_oneway(*samples)` \| `wilcoxon(x, y)` \| `mannwhitneyu(x, y)` \| `kruskal(*samples)` | parametric / rank test | t / ANOVA / Wilcoxon / Mann-Whitney / Kruskal-Wallis, each returning `statistic`/`pvalue` |
|  [06]   | `pearsonr(x, y)` \| `spearmanr(x, y)` \| `kendalltau(x, y)` \| `linregress(x, y)` | correlation | linear / rank correlation and OLS regression result |
|  [07]   | `bootstrap(data, statistic)` \| `permutation_test(data, statistic)` \| `monte_carlo_test(sample, rvs, statistic)` | resampling inference | confidence-interval / exchangeability / parametric MC test (`rng` keyword) |
|  [08]   | `gaussian_kde(dataset)` \| `multivariate_normal(mean, cov)` \| `ecdf(sample)` \| `Covariance.from_*(...)` | density / multivariate | KDE, multivariate-normal, empirical CDF, structured covariance carrier |
|  [09]   | `wasserstein_distance(u, v)` \| `entropy(pk, qk)` \| `differential_entropy(x)` \| `false_discovery_control(ps)` | divergence / multiple-test | optimal-transport distance, (relative) entropy, Benjamini-Hochberg FDR control |

[ENTRYPOINT_SCOPE]: `scipy.stats.qmc` quasi-Monte-Carlo sampling
- rail: experiments

`Sobol`/`Halton`/`LatinHypercube` are `QMCEngine` subclasses whose `random(n)` draws a low-discrepancy sample on the unit hypercube; `scale` affinely maps the sample to the bounds box and `discrepancy` scores the sample uniformity. The constructor randomness keyword is `rng` (SPEC-007; `seed` is a deprecated interim alias).

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY]   | [RESULT]                               |
| :-----: | :----------------------------------------------------- | :--------------- | :------------------------------------- |
|  [01]   | `qmc.Sobol(d, scramble, bits, rng, optimization)` -> `.random(n)` / `.random_base2(m)` | QMC engine | scrambled Sobol low-discrepancy sample |
|  [02]   | `qmc.Halton(d, scramble, rng)` -> `.random(n)`         | QMC engine       | Halton low-discrepancy sample          |
|  [03]   | `qmc.LatinHypercube(d, scramble, strength, rng, optimization)` -> `.random(n)` | QMC engine | Latin-hypercube (orthogonal-array `strength=2`) stratified sample |
|  [04]   | `qmc.PoissonDisk(d, radius, rng)` -> `.fill_space()`   | QMC engine       | blue-noise minimum-distance sample     |
|  [05]   | `qmc.MultivariateNormalQMC(mean, cov)` \| `MultinomialQMC(pvals, n_trials)` | QMC engine | low-discrepancy multivariate-normal / multinomial draw |
|  [06]   | `qmc.scale(sample, l_bounds, u_bounds)`                | affine map       | sample scaled to the bounds box        |
|  [07]   | `qmc.discrepancy(sample, method)`                      | uniformity score | low-discrepancy quality metric (`'CD'`/`'WD'`/`'MD'`/`'L2-star'`) |

[ENTRYPOINT_SCOPE]: `scipy.spatial` neighbour search, hull, and tessellation
- rail: spatial

`cKDTree` is the compiled KD-tree and `KDTree` is the Python subclass over it; `ConvexHull`/`Delaunay`/`Voronoi` are Qhull-backed tessellation carriers whose attributes expose simplices, vertices, and adjacency; `distance.cdist`/`pdist` are the pairwise distance matrix and condensed vector that `squareform` interconverts; `transform.Rotation` is the quaternion/matrix/Euler rotation carrier; `procrustes` is the optimal similarity alignment of two point sets.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]   | [RESULT]                                |
| :-----: | :-------------------------------------------------------------- | :--------------- | :-------------------------------------- |
|  [01]   | `KDTree(data)` \| `cKDTree(data)` -> `.query(x, k, workers)`    | neighbour search | k-nearest indices and distances (`KDTree` subclasses compiled `cKDTree`) |
|  [02]   | `cKDTree.query_ball_point(x, r)` \| `query_pairs(r)` \| `query_ball_tree(other, r)` | radius search | indices within radius / self pairs / cross-tree pairs |
|  [03]   | `ConvexHull(points)` -> `.simplices`/`.volume`/`.area` \| `HalfspaceIntersection(halfspaces, ip)` -> `.intersections`/`.dual_points` | hull | facet simplices, hull volume/area; `HalfspaceIntersection.intersections` the `(n_vertices, dim)` vertices of the intersection polytope, `ip` a strictly-feasible interior point (e.g. the `linprog` Chebyshev centre) |
|  [04]   | `Delaunay(points)` -> `.simplices`/`.find_simplex(p)`           | triangulation    | simplex table and barycentric point-location |
|  [05]   | `Voronoi(points)` -> `.vertices`/`.regions`/`.ridge_points` \| `SphericalVoronoi(points, radius)` | tessellation | planar / spherical Voronoi vertices, regions, ridge graph |
|  [06]   | `distance.cdist(XA, XB, metric)` \| `distance_matrix(x, y, p)`  | distance matrix  | pairwise / Minkowski distance matrix    |
|  [07]   | `distance.pdist(X, metric)` \| `distance.squareform(Y)`         | distance vector  | condensed distance vector / matrix form |
|  [08]   | `transform.Rotation.from_quat/from_matrix/from_euler/from_rotvec(...)` -> `.apply`/`.as_matrix`/`.inv`/`.mean` | rotation carrier | 3-D rotation algebra with mean and inversion |
|  [09]   | `transform.Slerp(times, rotations)` \| `RotationSpline(times, rotations)` \| `RigidTransform(...)` \| `Rotation.align_vectors(a, b)` | rotation interp / fit | spherical-linear / spline rotation interpolation, rigid transform, Kabsch alignment |
|  [10]   | `procrustes(data1, data2)` \| `geometric_slerp(start, end, t)`  | alignment / interp | optimal similarity alignment + disparity, sphere-surface slerp |

## [04]-[IMPLEMENTATION_LAW]

[SOLVER_TOPOLOGY]:
- transform: `scipy.fft` owns the pocketfft DFT family (`fft`/`ifft`, `fftn`/`ifftn`, `rfft`/`irfft`), the trigonometric transforms (`dct`, `dst`), and the frequency-grid helpers (`fftfreq`, `fftshift`); the `analysis/transform.md#TRANSFORM` owner routes spectral cases here, and `scipy.signal.hilbert` composes `fft`/`ifft` to build the analytic signal. `scipy.fft` is Array-API-aware (dispatches on the backend `xp`), but `scipy.signal.hilbert`'s Array-API support is experimental (gated behind `SCIPY_ARRAY_API=1`, off by default) and jax-skipped (item assignment), so the `analysis/transform.md#TRANSFORM` analytic arm is numpy-resident — aligning with the `analysis/signal.md#DSP` numpy-floor stance for `scipy.signal`.
- dense linear: `scipy.linalg` owns factorizations (`lu_factor`, `cholesky`, `ldl`, `qr`, `svd`, `polar`, `schur`), direct/structured solve (`solve`, `solve_triangular`, `solve_banded`, `lstsq`, `pinv`), eigensolvers (`eig`, `eigh` with `subset_by_index`), matrix functions (`expm`, `logm`, `sqrtm`, `funm`), and control matrix equations (`solve_sylvester`, `solve_continuous_are`, `solve_discrete_lyapunov`).
- sparse storage: `scipy.sparse` owns the CSR/CSC/COO/DIA/BSR/LIL containers; construct from `diags_array`, `eye_array`, `kron`, or block stacks before solving.
- sparse solve: `scipy.sparse.linalg` owns direct solve (`spsolve`, `splu`, `spilu`, `factorized`), the full Krylov family (`cg`/`gmres`/`bicgstab`/`minres`/`qmr`/`tfqmr`/`lgmres`/`gcrotmk`, all sharing the `x0`, `rtol`, `atol`, `maxiter`, `M`, `callback` signature; `gmres` adds `restart`/`callback_type`), least squares (`lsqr`, `lsmr`), sparse eigen/SVD (`eigsh`, `eigs`, `svds`), and matrix-free operator algebra (`LinearOperator`, `aslinearoperator`, `expm_multiply`, `onenormest`) — the matrix-free path feeds the FEM `solver_iter_krylov(M=...)` route in `.api/scikit-fem.md`.
- optimization: `scipy.optimize` owns local minimization (`minimize` with a `BFGS`/`SR1` `HessianUpdateStrategy`), global search (`differential_evolution`, `dual_annealing`, `shgo`, `basinhopping`, `direct`), least squares (`least_squares`, `curve_fit`, `nnls`), root finding (`root`, `brentq`, `newton`, `fsolve`, `fixed_point`), and linear/integer programming (`linprog`, `milp`); constraints route through `Bounds`, `LinearConstraint`, `NonlinearConstraint`; gradients verify through `approx_fprime`/`check_grad`; results carry an `OptimizeResult`.
- integration: `scipy.integrate` owns adaptive quadrature (`quad`, `dblquad`, `tplquad`, `nquad`, `quad_vec`, `tanhsinh`, `qmc_quad`, `nsum`), initial-value ODE (`solve_ivp` dispatching `RK45`/`DOP853`/`Radau`/`BDF`/`LSODA` `OdeSolver` classes, `odeint` legacy), boundary-value ODE (`solve_bvp`), and sampled/cumulative rules (`simpson`, `trapezoid`, `romb`, `cumulative_trapezoid`, `cumulative_simpson`); `qmc_quad` stacks a `scipy.stats.qmc` engine into the quadrature node set.
- interpolation: `scipy.interpolate` owns 1-D interpolants (`interp1d`, `CubicSpline`, `Akima1DInterpolator`, `PchipInterpolator`, `make_interp_spline`, `UnivariateSpline`, `make_smoothing_spline`) and scattered/grid interpolation (`RBFInterpolator`, `NearestNDInterpolator`, `LinearNDInterpolator`, `CloughTocher2DInterpolator`, `griddata`, `RegularGridInterpolator`, `NdBSpline`).
- signal: `scipy.signal` owns IIR/FIR design (`butter`, `cheby1`, `cheby2`, `ellip`, `bessel`, `iirfilter`, `firwin`), zero-phase/causal filtering (`sosfiltfilt`, `filtfilt`, `sosfilt`, `lfilter`, `savgol_filter`), spectral estimation (`welch`, `csd`, `coherence`, `periodogram`), the modern `ShortTimeFFT`/`stft`/`istft` time-frequency surface, convolution (`fftconvolve`, `oaconvolve`), resampling (`resample_poly`, `decimate`), and peak analysis (`find_peaks`, `peak_widths`, `peak_prominences`); the `signal/dsp.md#DSP` owner routes the stationary cases here beside the `pywt` wavelet cases.
- statistics: `scipy.stats` owns the continuous/discrete distribution objects (`norm`, `lognorm`, `gamma`, `beta`, `t`, `chi2`, `binom`, `poisson`, ...) with frozen `pdf`/`pmf`/`cdf`/`ppf`/`rvs`/`fit` (plus the array-API `make_distribution` object and bounded `fit`), parametric/rank hypothesis tests (`ttest_ind`, `f_oneway`, `mannwhitneyu`, `kruskal`, `wilcoxon`), goodness-of-fit (`ks_2samp`, `anderson`, `shapiro`), correlations (`pearsonr`, `spearmanr`, `kendalltau`, `linregress`), resampling inference (`bootstrap`, `permutation_test`, `monte_carlo_test`, all taking `rng`), density/divergence (`gaussian_kde`, `ecdf`, `wasserstein_distance`, `entropy`, `false_discovery_control`); the `numerics/statistics.md#STATISTICS` owner routes distribution and test cases here.
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
