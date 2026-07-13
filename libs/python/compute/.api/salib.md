# [PY_COMPUTE_API_SALIB]

`SALib` supplies global sensitivity analysis via Sobol, Morris, FAST, RBD-FAST, delta, PAWN, DGSM, and RSA methods — structured around a `problem` dict and the `ProblemSpec` fluent API — for the compute uncertainty-quantification and sensitivity rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SALib`
- package: `SALib` (dist name `salib`, import name `SALib`)
- owner: `compute`
- module: `SALib`; submodules `SALib.sample`, `SALib.analyze`, `SALib.util`, `SALib.plotting`
- asset: runtime library
- rail: sensitivity-analysis
- namespace: `SALib` (top-level `ProblemSpec`; method functions under `SALib.sample.<method>.sample` and `SALib.analyze.<method>.analyze`)
- installed: `1.5.2`
- requires: `numpy`, `scipy`, `pandas`, `matplotlib` (plotting), `multiprocess` (parallel/distributed evaluation)
- capability: global sensitivity analysis via Sobol, Morris, FAST, RBD-FAST, delta moment-independent, PAWN, DGSM, RSA, HDMR, and fractional-factorial methods; sampling designs (Saltelli, Sobol sequence, Morris trajectories, Latin hypercube, FAST frequency, finite-difference, fractional factorial); a `problem` dict plus the `ProblemSpec` fluent sample→evaluate→analyze pipeline with serial/parallel/distributed evaluation and DataFrame/plot export

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: top-level and utility types
- rail: sensitivity-analysis

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]         | [ROLE]                                          |
| :-----: | :---------------------- | :-------------------- | :---------------------------------------------- |
|  [01]   | `ProblemSpec`           | class (dict subclass) | fluent problem definition and pipeline runner   |
|  [02]   | `SALib.util.ResultDict` | class                 | analysis result container with `to_df()` method |

[PUBLIC_TYPE_SCOPE]: sample submodules
- rail: sensitivity-analysis
- type: module

| [INDEX] | [SYMBOL]                    | [SAMPLING_METHOD]                      |
| :-----: | :-------------------------- | :------------------------------------- |
|  [01]   | `SALib.sample.saltelli`     | Saltelli quasi-random Sobol sampling   |
|  [02]   | `SALib.sample.sobol`        | Sobol sequence sampling                |
|  [03]   | `SALib.sample.morris`       | Morris one-at-a-time trajectory sample |
|  [04]   | `SALib.sample.latin`        | Latin hypercube sampling               |
|  [05]   | `SALib.sample.fast_sampler` | FAST frequency sampling                |
|  [06]   | `SALib.sample.finite_diff`  | finite-difference gradient sample      |
|  [07]   | `SALib.sample.ff`           | fractional factorial design            |

[PUBLIC_TYPE_SCOPE]: analyze submodules
- rail: sensitivity-analysis
- type: module

| [INDEX] | [SYMBOL]                 | [ANALYSIS_METHOD]                        |
| :-----: | :----------------------- | :--------------------------------------- |
|  [01]   | `SALib.analyze.sobol`    | Sobol variance-based indices (S1/S2/ST)  |
|  [02]   | `SALib.analyze.morris`   | Morris elementary effects (mu/mu*/sigma) |
|  [03]   | `SALib.analyze.fast`     | FAST first-order indices                 |
|  [04]   | `SALib.analyze.rbd_fast` | RBD-FAST random-balance design           |
|  [05]   | `SALib.analyze.delta`    | delta moment-independent indices         |
|  [06]   | `SALib.analyze.pawn`     | PAWN KS-based indices                    |
|  [07]   | `SALib.analyze.dgsm`     | derivative-based global sensitivity      |
|  [08]   | `SALib.analyze.rsa`      | regional sensitivity analysis            |
|  [09]   | `SALib.analyze.hdmr`     | HDMR high-dimensional model repr.        |
|  [10]   | `SALib.analyze.ff`       | fractional factorial analysis            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ProblemSpec fluent pipeline
- rail: sensitivity-analysis
- every surface is a `ProblemSpec` method; `.sample`/`.evaluate`/`.analyze` and the parallel/distributed variants take `(func, *args, **kwargs)`, adding the keywords shown; setters take `np.ndarray`.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `.sample`                                                      | pipeline       | call a sample function and store  |
|  [02]   | `.evaluate`                                                    | pipeline       | evaluate model on stored samples  |
|  [03]   | `.analyze`                                                     | pipeline       | run analysis on stored results    |
|  [04]   | `.evaluate_parallel(nprocs=None)`                              | parallel eval  | parallel model evaluation         |
|  [05]   | `.analyze_parallel(nprocs=None)`                               | parallel       | parallel analysis                 |
|  [06]   | `.evaluate_distributed(nprocs=1, servers=None, verbose=False)` | distributed    | distributed evaluation            |
|  [07]   | `.set_samples(samples)`                                        | setter         | inject pre-computed sample matrix |
|  [08]   | `.set_results(results)`                                        | setter         | inject pre-computed model results |
|  [09]   | `.to_df()`                                                     | export         | convert analysis to DataFrame     |
|  [10]   | `.plot(**kwargs)`                                              | visualization  | plot sensitivity indices          |
|  [11]   | `.heatmap(metric=None, index=None, title=None, ax=None)`       | visualization  | heatmap of indices                |
|  [12]   | `.samples` (property)                                          | accessor       | stored sample matrix              |
|  [13]   | `.results` (property)                                          | accessor       | stored result array               |
|  [14]   | `.analysis` (property)                                         | accessor       | stored analysis result            |

[ENTRYPOINT_SCOPE]: sampling functions
- rail: sensitivity-analysis
- every sampler takes `(problem, N, ..., seed=None)` and returns an `ndarray`; `saltelli.sample` is the deprecated alias, `sobol.sample` the maintained Saltelli/Sobol design; `fast_sampler` `M` is the interference factor, `finite_diff` `delta` the perturbation step, and `ff` fixes size by `num_vars` (no `N`).

| [INDEX] | [SURFACE]                                                                         | [METHOD] | [SAMPLE]                              |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `saltelli.sample(calc_second_order=True, skip_values=None)`                       | Saltelli | Sobol quasi-random sample             |
|  [02]   | `sobol.sample(*, calc_second_order=True, scramble=True, skip_values=0)`           | Sobol    | Sobol sequence (bounds auto-scaled)   |
|  [03]   | `morris.sample(num_levels=4, optimal_trajectories=None, local_optimization=True)` | Morris   | trajectory sample                     |
|  [04]   | `latin.sample()`                                                                  | LHS      | Latin hypercube sample                |
|  [05]   | `fast_sampler.sample(M=4)`                                                        | FAST     | FAST frequency search-curve sample    |
|  [06]   | `finite_diff.sample(delta=0.01, skip_values=1024)`                                | DGSM     | finite-difference gradient sample     |
|  [07]   | `ff.sample()`                                                                     | FF       | fractional-factorial two-level design |

[ENTRYPOINT_SCOPE]: analyze functions
- rail: sensitivity-analysis
- every analyzer takes `(problem, [X,] Y, ..., seed=None)` (methods needing input samples take `X` before `Y`); `num_resamples=100`, `conf_level=0.95`, `print_to_console=False` are shared resampling/reporting keywords, and `sobol.analyze` adds `parallel=`/`n_processors=`/`keep_resamples=` for resample parallelism — the row shows the method-specific keywords.

| [INDEX] | [SURFACE]                                                                            | [METHOD] | [INDICES]                             |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `sobol.analyze(calc_second_order=True)`                                              | Sobol    | S1/S2/ST                              |
|  [02]   | `morris.analyze(scaled=False, num_levels=4)`                                         | Morris   | mu/mu*/sigma                          |
|  [03]   | `fast.analyze(M=4)`                                                                  | FAST     | first-order                           |
|  [04]   | `rbd_fast.analyze(M=10)`                                                             | RBD-FAST | first-order                           |
|  [05]   | `delta.analyze(y_resamples=None, method='all')`                                      | delta    | moment-independent                    |
|  [06]   | `pawn.analyze(S=10)`                                                                 | PAWN     | KS-based                              |
|  [07]   | `dgsm.analyze()`                                                                     | DGSM     | `vi`/`vi_std`/`dgsm` derivative-based |
|  [08]   | `rsa.analyze(bins=20, target='Y')`                                                   | RSA      | regional (binned) sensitivity         |
|  [09]   | `hdmr.analyze(maxorder=2, maxiter=100, m=2, K=20, R=None, alpha=0.95, lambdax=0.01)` | HDMR     | `Sa`/`Sb`/`ST` component expansion    |
|  [10]   | `ff.analyze(second_order=False)`                                                     | FF       | main + optional 2nd-order effects     |

[ENTRYPOINT_SCOPE]: utility functions
- rail: sensitivity-analysis

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :----------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `util.read_param_file(filename, delimiter=None)` | I/O            | read problem dict from CSV         |
|  [02]   | `util.scale_samples(params, problem)`            | transform      | scale unit-hypercube to bounds     |
|  [03]   | `util.compute_groups_matrix(groups)`             | utility        | build groups incidence matrix      |
|  [04]   | `util.extract_group_names(p) -> Tuple`           | utility        | extract group names from problem   |
|  [05]   | `util.handle_seed(seed) -> Generator`            | utility        | normalize seed to numpy Generator  |
|  [06]   | `util.avail_approaches(pkg)`                     | introspection  | list available methods in a subpkg |

## [04]-[IMPLEMENTATION_LAW]

[SALIB_TOPOLOGY]:
- `problem` dict requires keys `num_vars: int`, `names: list[str]`, `bounds: list[[lo, hi]]`; optional `groups`, `dists` (per-variable distribution: `unif`/`norm`/`lognorm`/`triang`/`truncnorm`), `outputs`
- `ProblemSpec` is a `dict` subclass — construct with the same keys as `problem`; the fluent pipeline is `.sample(sampler.sample, N, ...)` → `.evaluate(model)` (or `.evaluate_parallel`/`.evaluate_distributed`) → `.analyze(analyzer.analyze, ...)`, with `.samples`/`.results`/`.analysis` properties and `.set_samples`/`.set_results` injectors; `.to_df()`/`.plot()`/`.heatmap()` export the analysis
- `ResultDict` (`SALib.util.ResultDict`) is returned by all `analyze()` calls; it is a `dict` subclass with `.to_df()` and `.plot()`; keys vary by method (`S1`/`S2`/`ST`/`*_conf` for Sobol, `mu`/`mu_star`/`mu_star_conf`/`sigma` for Morris, `delta`/`S1` for delta, `vi`/`dgsm` for DGSM)
- `seed` is accepted on every sampler (`sobol`/`morris`/`latin`/`fast_sampler`/`finite_diff`/`ff`) and every analyzer for deterministic reproduction; `SALib.util.handle_seed(seed)` normalizes an `int`/`Generator`/`None` to a numpy `Generator`
- Sobol requires sample size `N = 2^k`; the Saltelli/Sobol design produces `N * (2*D + 2)` rows for `calc_second_order=True`, `N * (D + 2)` otherwise; DGSM consumes the `finite_diff` sample, RSA/PAWN/delta/Morris consume a paired `(X, Y)`, and FAST/Sobol consume `Y` alone

[SALIB_STACKING]:
- model evaluation rail: `ProblemSpec.evaluate(model)` calls a vectorized `model(X) -> Y` over the full sample matrix; `evaluate_parallel(model, nprocs=)` and `evaluate_distributed(model, nprocs=, servers=)` fan the rows out over `multiprocess`/distributed workers — when the model is an expensive geometry/FE/quadrature evaluation, the parallel evaluator owns the fan-out rather than a hand-built pool.
- numpy/pandas rail: sample matrices and `Y` are plain `numpy.ndarray`; `ResultDict.to_df()` / `ProblemSpec.to_df()` lower the indices to a `pandas.DataFrame` for the receipt and downstream tabulation, so the study result crosses the wire as a frame, not a bare dict.
- arviz/posterior rail: when the model output is a posterior summary, the sensitivity indices and their `*_conf` confidence bounds are captured beside the `arviz` diagnostics under one study receipt keyed by `problem`/`N`/`seed`.
- distribution rail: `problem['dists']` selects per-variable input distributions resolved through `scipy.stats`; non-uniform inputs are declared in the `problem` dict, never pre-transformed by the caller before sampling.

[LOCAL_ADMISSION]:
- Use `ProblemSpec` for new analysis pipelines; use module-level `sample`/`analyze` functions only for one-shot scripting.
- Capture `problem`, `N`, `num_resamples`, `conf_level`, and `seed` in the study receipt alongside the `ResultDict`.
- `scale_samples` applies bounds transformation only when sampling produces unit-hypercube output (Saltelli, LHS); Sobol sampler scales automatically when bounds are provided in `problem`.

[RAIL_LAW]:
- Package: `SALib`
- Owns: global sensitivity analysis — sampling design (Saltelli/Sobol, Morris trajectories, Latin hypercube, FAST frequency, finite-difference, fractional factorial) and index computation (Sobol S1/S2/ST, Morris mu/mu*/sigma, FAST, RBD-FAST, delta moment-independent, PAWN KS, DGSM derivative-based, RSA regional, HDMR component-function, fractional-factorial), with serial/parallel/distributed model evaluation and DataFrame/plot/heatmap export
- Accept: `ProblemSpec`-driven `.sample`→`.evaluate`→`.analyze` pipelines with structured receipts; module-level `<method>.sample(problem, N, ..., seed=)` / `<method>.analyze(problem, [X,] Y, ..., seed=)` for direct use; `seed` for deterministic reproduction; `problem['dists']` for non-uniform inputs; `evaluate_parallel`/`evaluate_distributed` for expensive models
- Reject: hand-rolled Sobol/Morris/FAST index estimators where the admitted analyzer owns the method; `print_to_console=True` in production compute paths; caller pre-transforming input distributions where `problem['dists']` declares them; a hand-built worker pool where `evaluate_parallel` owns the fan-out
