# [PY_COMPUTE_API_SALIB]

`SALib` owns global sensitivity analysis for the compute uncertainty-quantification rail: a `problem` dict and the `ProblemSpec` fluent pipeline fold a design's inputs through one sample→evaluate→analyze rail into variance-based, elementary-effect, moment-independent, and derivative-based sensitivity indices.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SALib`
- package: `SALib` (MIT, SALib project)
- module: `SALib` (dist `salib`); entrypoints under `SALib.sample.<method>.sample`, `SALib.analyze.<method>.analyze`, utilities in `SALib.util`
- rail: sensitivity analysis — sampling design and index computation over serial, parallel, and distributed model evaluation with DataFrame/plot/heatmap export

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: top-level and result types

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]         | [CAPABILITY]                                                       |
| :-----: | :---------------------- | :-------------------- | :----------------------------------------------------------------- |
|  [01]   | `ProblemSpec`           | class (dict subclass) | fluent problem definition and `sample`/`evaluate`/`analyze` runner |
|  [02]   | `SALib.util.ResultDict` | class (dict subclass) | analysis result container with `to_df()` / `plot()`                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ProblemSpec` fluent pipeline
- `.sample`/`.evaluate`/`.analyze` and the parallel/distributed variants take `(func, *args, **kwargs)` and store their output; the rows show the added keywords, setters take an `np.ndarray`.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `.sample`                                                         | instance | call a sampler and store the matrix        |
|  [02]   | `.evaluate`                                                       | instance | evaluate model over stored samples         |
|  [03]   | `.analyze`                                                        | instance | run analysis over stored results           |
|  [04]   | `.evaluate_parallel(*, nprocs=None)`                              | instance | fan evaluation over `multiprocess` workers |
|  [05]   | `.analyze_parallel(*, nprocs=None)`                               | instance | parallel analysis                          |
|  [06]   | `.evaluate_distributed(*, nprocs=1, servers=None, verbose=False)` | instance | distribute evaluation across a cluster     |
|  [07]   | `.set_samples(samples)`                                           | instance | inject a precomputed sample matrix         |
|  [08]   | `.set_results(results)`                                           | instance | inject precomputed model results           |
|  [09]   | `.to_df()`                                                        | instance | lower analysis to a `pandas.DataFrame`     |
|  [10]   | `.plot(**kwargs)`                                                 | instance | plot sensitivity indices                   |
|  [11]   | `.heatmap(metric=None, index=None, title=None, ax=None)`          | instance | heatmap of indices                         |
|  [12]   | `.samples`                                                        | property | stored sample matrix                       |
|  [13]   | `.results`                                                        | property | stored result array                        |
|  [14]   | `.analysis`                                                       | property | stored analysis result                     |

[ENTRYPOINT_SCOPE]: sampling functions
- Each sampler is a module-level `<method>.sample(problem, N, ..., seed=None) -> ndarray`; rows show the method-specific keywords. `sobol.sample` implements Saltelli's Sobol'-sequence extension with bounds auto-scaled, and `ff.sample` fixes size by `num_vars` with no `N`.

| [INDEX] | [SURFACE]                                                                         | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `sobol.sample(*, calc_second_order=True, scramble=True, skip_values=0)`           | Sobol' quasi-random (Saltelli) sample          |
|  [02]   | `morris.sample(num_levels=4, optimal_trajectories=None, local_optimization=True)` | one-at-a-time trajectory sample                |
|  [03]   | `latin.sample()`                                                                  | Latin hypercube sample                         |
|  [04]   | `fast_sampler.sample(M=4)`                                                        | FAST frequency search-curve sample             |
|  [05]   | `finite_diff.sample(delta=0.01, skip_values=1024)`                                | finite-difference gradient sample (DGSM input) |
|  [06]   | `ff.sample()`                                                                     | fractional-factorial two-level design          |

[ENTRYPOINT_SCOPE]: analyze functions
- Each analyzer is `<method>.analyze(problem, [X,] Y, ..., seed=None) -> ResultDict`; methods reading input samples take `X` before `Y`. Shared keywords `num_resamples=100`, `conf_level=0.95`, `print_to_console=False`; `sobol.analyze` adds `parallel=`/`n_processors=`/`keep_resamples=` for resample parallelism. Rows show the method-specific keywords.

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `sobol.analyze(calc_second_order=True)`                                              | S1/S2/ST variance indices                      |
|  [02]   | `morris.analyze(scaled=False, num_levels=4)`                                         | mu/mu*/sigma elementary effects                |
|  [03]   | `fast.analyze(M=4)`                                                                  | first-order indices                            |
|  [04]   | `rbd_fast.analyze(M=10)`                                                             | first-order (random-balance design)            |
|  [05]   | `delta.analyze(y_resamples=None, method='all')`                                      | moment-independent delta + S1                  |
|  [06]   | `pawn.analyze(S=10)`                                                                 | KS-based indices                               |
|  [07]   | `dgsm.analyze()`                                                                     | `vi`/`vi_std`/`dgsm` derivative-based          |
|  [08]   | `rsa.analyze(bins=20, target='Y')`                                                   | regional (binned) sensitivity                  |
|  [09]   | `hdmr.analyze(maxorder=2, maxiter=100, m=2, K=20, R=None, alpha=0.95, lambdax=0.01)` | `Sa`/`Sb`/`ST` component expansion             |
|  [10]   | `discrepancy.analyze(method='WD')`                                                   | discrepancy indices (`WD`/`CD`/`MD`/`L2-star`) |
|  [11]   | `ff.analyze(second_order=False)`                                                     | main + optional 2nd-order effects              |

[ENTRYPOINT_SCOPE]: utility functions (module-level, `SALib.util`)

| [INDEX] | [SURFACE]                                   | [CAPABILITY]                                        |
| :-----: | :------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `read_param_file(filename, delimiter=None)` | read a `problem` dict from CSV                      |
|  [02]   | `scale_samples(params, problem)`            | scale a unit-hypercube matrix to bounds             |
|  [03]   | `compute_groups_matrix(groups)`             | build the groups incidence matrix                   |
|  [04]   | `extract_group_names(problem) -> tuple`     | extract group names from a `problem`                |
|  [05]   | `handle_seed(seed) -> Generator`            | normalize int/Generator/None to a numpy `Generator` |
|  [06]   | `avail_approaches(pkg)`                     | list available methods in a subpackage              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `problem` dict requires `num_vars: int`, `names: list[str]`, `bounds: list[[lo, hi]]`; optional `groups`, `dists` (per-variable `unif`/`norm`/`lognorm`/`triang`/`truncnorm`), `outputs`. `ProblemSpec` is a `dict` subclass constructed from the same keys.
- Sobol requires `N = 2^k`; the Saltelli/Sobol' design emits `N*(2D+2)` rows under `calc_second_order=True`, `N*(D+2)` otherwise. FAST and Sobol consume `Y` alone; Morris, RSA, PAWN, delta, and DGSM consume a paired `(X, Y)`, DGSM over the `finite_diff` sample.
- Every `analyze()` returns a `ResultDict` (`dict` subclass, `.to_df()`/`.plot()`); keys vary by method — `S1`/`S2`/`ST`/`*_conf` for Sobol, `mu`/`mu_star`/`mu_star_conf`/`sigma` for Morris, `delta`/`S1` for delta, `vi`/`dgsm` for DGSM.
- `seed` on every sampler and analyzer gives deterministic reproduction; `util.handle_seed` normalizes an int/`Generator`/`None` to a numpy `Generator`.

[STACKING]:
- `numpy`(`.api/numpy.md`): sample matrices and `Y` cross as plain `ndarray`; a vectorized `model(X) -> Y` closes over the full sample matrix.
- `scipy`(`.api/scipy.md`): `problem['dists']` resolves per-variable input distributions through `scipy.stats`; non-uniform inputs are declared in the dict, never pre-transformed by the caller.
- `arviz`(`.api/arviz.md`): a posterior-summary model output lands its indices and their `*_conf` bounds beside the `arviz` diagnostics under one study receipt keyed by `problem`/`N`/`seed`.
- within-lib: `ResultDict.to_df()`/`ProblemSpec.to_df()` lower indices to a `pandas.DataFrame` for the receipt; `evaluate_parallel`/`evaluate_distributed` fan an expensive `model(X)` over `multiprocess`/cluster workers, owning the fan-out rather than a hand-built pool.

[LOCAL_ADMISSION]:
- `ProblemSpec` drives every new pipeline; module-level `<method>.sample`/`.analyze` serve one-shot scripting only. Every study receipt captures `problem`, `N`, `num_resamples`, `conf_level`, `seed`, and the `ResultDict`.
- `scale_samples` transforms bounds only for unit-hypercube samplers (LHS); the Sobol sampler auto-scales when `problem` carries bounds.

[RAIL_LAW]:
- Package: `SALib`
- Owns: global sensitivity analysis — sampling design and index computation over serial, parallel, and distributed model evaluation with DataFrame/plot/heatmap export
- Accept: `ProblemSpec` `.sample`→`.evaluate`→`.analyze` pipelines with structured receipts; module-level `<method>.sample`/`.analyze` for direct use; `seed` for reproduction; `problem['dists']` for non-uniform inputs; `evaluate_parallel`/`evaluate_distributed` for expensive models
- Reject: a hand-rolled Sobol/Morris/FAST index estimator where the admitted analyzer owns the method; `print_to_console=True` in production compute paths; a caller pre-transforming input distributions `problem['dists']` declares; a hand-built worker pool where `evaluate_parallel` owns the fan-out
