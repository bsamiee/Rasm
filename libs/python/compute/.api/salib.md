# [PY_COMPUTE_API_SALIB]

`SALib` supplies global sensitivity analysis via Sobol, Morris, FAST, RBD-FAST, delta, PAWN, DGSM, and RSA methods — structured around a `problem` dict and the `ProblemSpec` fluent API — for the compute uncertainty-quantification and sensitivity rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SALib`
- package: `SALib`
- module: `SALib`; submodules `SALib.sample`, `SALib.analyze`, `SALib.util`
- asset: runtime library
- rail: sensitivity-analysis

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: top-level and utility types
- rail: sensitivity-analysis

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]         | [ROLE]                                          |
| :-----: | :------------ | :-------------------- | :---------------------------------------------- |
|   [1]   | `ProblemSpec` | class (dict subclass) | fluent problem definition and pipeline runner   |
|   [2]   | `ResultDict`  | class                 | analysis result container with `to_df()` method |

[PUBLIC_TYPE_SCOPE]: sample submodules
- rail: sensitivity-analysis
- type: module

| [INDEX] | [SYMBOL]                    | [SAMPLING_METHOD]                      |
| :-----: | :-------------------------- | :------------------------------------- |
|   [1]   | `SALib.sample.saltelli`     | Saltelli quasi-random Sobol sampling   |
|   [2]   | `SALib.sample.sobol`        | Sobol sequence sampling                |
|   [3]   | `SALib.sample.morris`       | Morris one-at-a-time trajectory sample |
|   [4]   | `SALib.sample.latin`        | Latin hypercube sampling               |
|   [5]   | `SALib.sample.fast_sampler` | FAST frequency sampling                |
|   [6]   | `SALib.sample.finite_diff`  | finite-difference gradient sample      |
|   [7]   | `SALib.sample.ff`           | fractional factorial design            |

[PUBLIC_TYPE_SCOPE]: analyze submodules
- rail: sensitivity-analysis
- type: module

| [INDEX] | [SYMBOL]                 | [ANALYSIS_METHOD]                        |
| :-----: | :----------------------- | :--------------------------------------- |
|   [1]   | `SALib.analyze.sobol`    | Sobol variance-based indices (S1/S2/ST)  |
|   [2]   | `SALib.analyze.morris`   | Morris elementary effects (mu/mu*/sigma) |
|   [3]   | `SALib.analyze.fast`     | FAST first-order indices                 |
|   [4]   | `SALib.analyze.rbd_fast` | RBD-FAST random-balance design           |
|   [5]   | `SALib.analyze.delta`    | delta moment-independent indices         |
|   [6]   | `SALib.analyze.pawn`     | PAWN KS-based indices                    |
|   [7]   | `SALib.analyze.dgsm`     | derivative-based global sensitivity      |
|   [8]   | `SALib.analyze.rsa`      | regional sensitivity analysis            |
|   [9]   | `SALib.analyze.hdmr`     | HDMR high-dimensional model repr.        |
|  [10]   | `SALib.analyze.ff`       | fractional factorial analysis            |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ProblemSpec fluent pipeline
- rail: sensitivity-analysis

| [INDEX] | [SURFACE]                                                                                        | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :----------------------------------------------------------------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `ProblemSpec.sample(func, *args, **kwargs)`                                                      | pipeline       | call a sample function and store  |
|   [2]   | `ProblemSpec.evaluate(func, *args, **kwargs)`                                                    | pipeline       | evaluate model on stored samples  |
|   [3]   | `ProblemSpec.analyze(func, *args, **kwargs)`                                                     | pipeline       | run analysis on stored results    |
|   [4]   | `ProblemSpec.evaluate_parallel(func, *args, nprocs=None, **kwargs)`                              | parallel eval  | parallel model evaluation         |
|   [5]   | `ProblemSpec.analyze_parallel(func, *args, nprocs=None, **kwargs)`                               | parallel       | parallel analysis                 |
|   [6]   | `ProblemSpec.evaluate_distributed(func, *args, nprocs=1, servers=None, verbose=False, **kwargs)` | distributed    | distributed evaluation            |
|   [7]   | `ProblemSpec.set_samples(samples: np.ndarray)`                                                   | setter         | inject pre-computed sample matrix |
|   [8]   | `ProblemSpec.set_results(results: np.ndarray)`                                                   | setter         | inject pre-computed model results |
|   [9]   | `ProblemSpec.to_df()`                                                                            | export         | convert analysis to DataFrame     |
|  [10]   | `ProblemSpec.plot(**kwargs)`                                                                     | visualization  | plot sensitivity indices          |
|  [11]   | `ProblemSpec.heatmap(metric=None, index=None, title=None, ax=None)`                              | visualization  | heatmap of indices                |
|  [12]   | `ProblemSpec.samples` (property)                                                                 | accessor       | stored sample matrix              |
|  [13]   | `ProblemSpec.results` (property)                                                                 | accessor       | stored result array               |
|  [14]   | `ProblemSpec.analysis` (property)                                                                | accessor       | stored analysis result            |

[ENTRYPOINT_SCOPE]: sampling functions
- rail: sensitivity-analysis

| [INDEX] | [SURFACE]                                                                                                           | [METHOD] | [RAIL]                    |
| :-----: | :------------------------------------------------------------------------------------------------------------------ | :------- | :------------------------ |
|   [1]   | `saltelli.sample(problem, N, calc_second_order=True, skip_values=None) -> ndarray`                                  | Saltelli | Sobol quasi-random sample |
|   [2]   | `sobol.sample(problem, N, *, calc_second_order=True, scramble=True, skip_values=0, seed=None)`                      | Sobol    | Sobol sequence sample     |
|   [3]   | `morris.sample(problem, N, num_levels=4, optimal_trajectories=None, local_optimization=True, seed=None) -> ndarray` | Morris   | trajectory sample         |
|   [4]   | `latin.sample(problem, N, seed=None)`                                                                               | LHS      | Latin hypercube sample    |

[ENTRYPOINT_SCOPE]: analyze functions
- rail: sensitivity-analysis

| [INDEX] | [SURFACE]                                                                                                                                                                           | [METHOD] | [INDICES]          |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------- | :----------------- |
|   [1]   | `sobol.analyze(problem, Y, calc_second_order=True, num_resamples=100, conf_level=0.95, print_to_console=False, parallel=False, n_processors=None, keep_resamples=False, seed=None)` | Sobol    | S1/S2/ST           |
|   [2]   | `morris.analyze(problem, X, Y, num_resamples=100, conf_level=0.95, scaled=False, print_to_console=False, num_levels=4, seed=None) -> Dict`                                          | Morris   | mu/mu*/sigma       |
|   [3]   | `fast.analyze(problem, Y, M=4, num_resamples=100, conf_level=0.95, print_to_console=False, seed=None)`                                                                              | FAST     | first-order        |
|   [4]   | `rbd_fast.analyze(problem, X, Y, M=10, num_resamples=100, conf_level=0.95, print_to_console=False, seed=None)`                                                                      | RBD-FAST | first-order        |
|   [5]   | `delta.analyze(problem, X, Y, num_resamples=100, conf_level=0.95, print_to_console=False, seed=None, y_resamples=None, method='all') -> Dict`                                       | delta    | moment-independent |
|   [6]   | `pawn.analyze(problem, X, Y, S=10, print_to_console=False, seed=None)`                                                                                                              | PAWN     | KS-based           |

[ENTRYPOINT_SCOPE]: utility functions
- rail: sensitivity-analysis

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :----------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `util.read_param_file(filename, delimiter=None)` | I/O            | read problem dict from CSV         |
|   [2]   | `util.scale_samples(params, problem)`            | transform      | scale unit-hypercube to bounds     |
|   [3]   | `util.compute_groups_matrix(groups)`             | utility        | build groups incidence matrix      |
|   [4]   | `util.extract_group_names(p) -> Tuple`           | utility        | extract group names from problem   |
|   [5]   | `util.handle_seed(seed) -> Generator`            | utility        | normalize seed to numpy Generator  |
|   [6]   | `util.avail_approaches(pkg)`                     | introspection  | list available methods in a subpkg |

## [4]-[IMPLEMENTATION_LAW]

[SALIB_TOPOLOGY]:
- `problem` dict requires keys `num_vars: int`, `names: list[str]`, `bounds: list[[lo, hi]]`; optional `groups`, `dists`, `outputs`
- `ProblemSpec` is a `dict` subclass — construct with same keys as `problem`; provides the fluent pipeline
- `ResultDict` is returned by all `analyze()` calls; keys vary by method (`S1`/`S2`/`ST` for Sobol, `mu`/`mu_star`/`sigma` for Morris)
- Sobol requires sample size `N = 2^k`; Saltelli produces `N * (2*D + 2)` rows for `calc_second_order=True`

[LOCAL_ADMISSION]:
- Use `ProblemSpec` for new analysis pipelines; use module-level `sample`/`analyze` functions only for one-shot scripting.
- Capture `problem`, `N`, `num_resamples`, `conf_level`, and `seed` in the study receipt alongside the `ResultDict`.
- `scale_samples` applies bounds transformation only when sampling produces unit-hypercube output (Saltelli, LHS); Sobol sampler scales automatically when bounds are provided in `problem`.

[RAIL_LAW]:
- Package: `SALib`
- Owns: global sensitivity analysis — sampling design and variance/moment/KS index computation
- Accept: `ProblemSpec`-driven pipelines with structured receipts; module-level `analyze(problem, Y, ...)` for direct use
- Reject: hand-rolled Sobol index estimators; use of `print_to_console=True` in production compute paths
