# [PY_COMPUTE_API_PYDOE3]

`pyDOE3` mints classical design-of-experiments matrices — factorial, response-surface, screening, orthogonal-array, and Doehlert families — beside low-discrepancy and quasi-random samplers, filling the coded response-surface DOE gap left open between the SALib variance-based samplers and the `scipy.stats.qmc` space-filling engines for the compute `Study` design rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyDOE3`
- package: `pyDOE3` (dist name `pydoe3`, import name `pyDOE3`)
- owner: `compute`
- module: `pyDOE3`; submodules `pyDOE3.doe_factorial`, `pyDOE3.doe_box_behnken`, `pyDOE3.doe_composite`, `pyDOE3.doe_plackett_burman`, `pyDOE3.doe_gsd`, `pyDOE3.doe_taguchi`, `pyDOE3.doe_doehlert`, `pyDOE3.doe_lhs`, `pyDOE3.doe_sparse_grid`, `pyDOE3.utils`
- asset: runtime library
- rail: experiment-design
- namespace: `pyDOE3` — generators, samplers, and `TaguchiObjective` re-export at the top level, each generator returning a `numpy.ndarray` in its family coordinate system; `ORTHOGONAL_ARRAY_NAMES` and `build_regression_matrix` resolve under `pyDOE3.doe_taguchi` / `pyDOE3.build_regression_matrix`
- requires: `numpy`, `scipy`
- capability: coded design matrices over a factor count with per-generator level, center, and axial controls, design folding, aliasing analysis, regression-variance evaluation, and unit-hypercube samplers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: design vocabulary

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `TaguchiObjective`       | enum          | Taguchi loss goal — larger / smaller / nominal is best |
|  [02]   | `ORTHOGONAL_ARRAY_NAMES` | literal       | admitted Taguchi array ids (`L4(2^3)`, `L9(3^4)`, ...) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: coded factorial designs

| [INDEX] | [SURFACE]                                           | [DESIGN]           | [RETURN]                                              |
| :-----: | :-------------------------------------------------- | :----------------- | :---------------------------------------------------- |
|  [01]   | `fullfact(levels)`                                  | general full       | every level combination; rows `= prod(levels)`        |
|  [02]   | `ff2n(n_factors)`                                   | two-level full     | `2**n_factors` runs over `{-1, +1}`                   |
|  [03]   | `fracfact(gen)`                                     | fractional by gen  | columns selected by a generator string `"a b ab"`     |
|  [04]   | `fracfact_by_res(n, res)`                           | fractional by res  | minimum-aberration design of resolution `res`         |
|  [05]   | `fracfact_opt(n_factors, n_erased, max_attempts=0)` | fractional optimal | aliasing-optimal generator search over erased columns |
|  [06]   | `fracfact_aliasing(design)`                         | aliasing analysis  | `(aliases, alias_vector)` confounding structure       |
|  [07]   | `alias_vector_indices(n_factors)`                   | aliasing analysis  | column-pair index list for the alias vector           |
|  [08]   | `fold(H, columns=None)`                             | design fold        | design stacked with its sign-reversed mirror          |

[ENTRYPOINT_SCOPE]: response-surface and screening designs

| [INDEX] | [SURFACE]                                               | [DESIGN]           | [CONTROLS]                           |
| :-----: | :------------------------------------------------------ | :----------------- | :----------------------------------- |
|  [01]   | `bbdesign(n, center)`                                   | Box-Behnken        | `center` center-point count          |
|  [02]   | `ccdesign(n, center, alpha, face)`                      | central-composite  | axial `alpha`, block `face`          |
|  [03]   | `star(n, alpha, center)`                                | star points        | axial block at `+/- alpha`           |
|  [04]   | `pbdesign(n)`                                           | Plackett-Burman    | main-effect screening                |
|  [05]   | `gsd(levels, reduction, n=1)`                           | generalized subset | per-factor `levels`, `reduction` > 1 |
|  [06]   | `taguchi_design(oa_name, levels_per_factor)`            | Taguchi            | orthogonal array to factor settings  |
|  [07]   | `doehlert_shell_design(num_factors, num_center_points)` | Doehlert shell     | uniform-shell second-order           |
|  [08]   | `doehlert_simplex_design(num_factors)`                  | Doehlert simplex   | simplex-lattice second-order         |

[ENTRYPOINT_SCOPE]: samplers and low-discrepancy sequences
- `lhs` `criterion` ∈ `center`/`maximin`/`centermaximin`/`correlation`; `doe_sparse_grid` `grid_type` ∈ `clenshaw_curtis`/`chebyshev`/`gauss_patterson`.

| [INDEX] | [SURFACE]                                                          | [SAMPLER]                        |
| :-----: | :----------------------------------------------------------------- | :------------------------------- |
|  [01]   | `lhs(n, samples, criterion, iterations, ...)`                      | Latin hypercube (space-filling)  |
|  [02]   | `halton_sequence(num_points, dimension, skip=0)`                   | Halton low-discrepancy           |
|  [03]   | `sobol_sequence(n, d, scramble, seed, bounds, skip, use_pow_of_2)` | Sobol' scrambled/bounded         |
|  [04]   | `korobov_sequence(num_points, dimension, generator_param)`         | Korobov rank-1 lattice           |
|  [05]   | `rank1_lattice(num_points, dimension, generator_vector)`           | rank-1 integration lattice       |
|  [06]   | `sukharev_grid(num_points, dimension)`                             | centered regular grid            |
|  [07]   | `doe_sparse_grid(n_level, n_factors, grid_type)`                   | Smolyak sparse-grid nodes        |
|  [08]   | `sparse_grid_dimension(n_level, n_factors)`                        | sparse-grid node count           |
|  [09]   | `morris_sampling(num_vars, N, num_levels, seed)`                   | Morris screening trajectories    |
|  [10]   | `saltelli_sampling(num_vars, N, calc_second_order, ...)`           | Saltelli Sobol'-index sample     |
|  [11]   | `random_uniform(num_points, dimension, seed)`                      | i.i.d. uniform                   |
|  [12]   | `random_k_means(num_points, dimension, num_steps, ...)`            | centroidal-Voronoi space-filling |
|  [13]   | `cranley_patterson_shift(points, seed)`                            | randomized-QMC toroidal shift    |

[ENTRYPOINT_SCOPE]: Taguchi and regression utilities
- `list_orthogonal_arrays() -> list[str]`, `get_orthogonal_array() -> np.ndarray`, `compute_snr() -> float`; `var_regression_matrix` model tokens read `"1 x1 x2 x1*x2"`.

| [INDEX] | [SURFACE]                                     | [RAIL]                                         |
| :-----: | :-------------------------------------------- | :--------------------------------------------- |
|  [01]   | `list_orthogonal_arrays()`                    | list admitted Taguchi array ids                |
|  [02]   | `get_orthogonal_array(oa_name)`               | coded orthogonal array for `oa_name`           |
|  [03]   | `compute_snr(responses, objective)`           | Taguchi signal-to-noise over a trial's repeats |
|  [04]   | `var_regression_matrix(H, x, model, sigma=1)` | prediction variance at `x` for `model` tokens  |
|  [05]   | `build_regression_matrix(H, model, build)`    | expand a design into the regression matrix     |
|  [06]   | `scale_samples(samples, bounds)`              | affine-scale unit samples into `bounds`        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Rows are runs, columns are factors: `fullfact`/`gsd` carry integer level indices, `ff2n`/`fracfact*`/`pbdesign` carry `{-1, +1}`, `bbdesign` adds `0`, `ccdesign`/`star` add `+/- alpha`, and `fold` appends the sign-reversed mirror of its input levels — each family coordinate system maps onto physical factor bounds through the study per-axis fold.
- `ccdesign` composes a two-level factorial (or resolution-V fraction) with `star` axial points and `center` replicates: `face="faced"` fixes `alpha = 1` in-cube, `circumscribed`/`inscribed` place the star at the variance-driven `alpha`, and `alpha="rotatable"` targets uniform prediction variance on a sphere.
- Samplers split by input contract — `lhs`/`sobol_sequence`/`halton_sequence`/`korobov_sequence`/`rank1_lattice`/`sukharev_grid` take `(points, dimension)` unit-hypercube shape, `morris_sampling`/`saltelli_sampling` take `(num_vars, N)` normalized screening shape, and `doe_sparse_grid` emits Smolyak quadrature nodes sized by `sparse_grid_dimension`.

[STACKING]:
- `salib`(`.api/salib.md`): `saltelli_sampling`/`morris_sampling` emit the screening matrices SALib's `sobol`/`morris` samplers own; the SALib `ProblemSpec` pipeline stays the analyzer owner and consumes a pyDOE3 sample only where a coded classical cohort is required over a variance-based one.
- `scipy`(`.api/scipy.md`): `scipy.stats.qmc` owns pure space-filling QMC; pyDOE3 owns the response-surface and screening design matrices, never the QMC engines or the sensitivity indices.
- study design rail: response-surface generators are `StudyMethod` design arms beside the `factorial` meshgrid and the QMC engines, each family-coded matrix folding through the per-`ParamAxis` mapping so the design matrix and the response contract stay identical across design and sampling methods.
- regression + numpy rail: `build_regression_matrix`/`var_regression_matrix` lower a coded design into its regression matrix and prediction-variance field for the polynomial-surrogate diagnostic without a hand-built Vandermonde; every design and sample crosses into the study spine and the `data/tabular` DOE-frame gate as a plain `numpy.ndarray` of float64 columns.

[LOCAL_ADMISSION]:
- Generate classical DOE cohorts through the coded factorial/response-surface generators; route pure space-filling QMC to `scipy.stats.qmc` and variance-based sensitivity to the SALib samplers.
- Map each family's coordinates onto physical bounds through the study per-axis fold; a bare unit-hypercube sample uses `scale_samples` directly.
- Capture the design method, factor count, center/star controls, and `seed` in the study receipt so a resumed run rebuilds the identical coded cohort.

[RAIL_LAW]:
- Package: `pyDOE3`
- Owns: classical design-of-experiments matrix generation across factorial, response-surface, screening, orthogonal-array, and Doehlert families, with design folding, aliasing analysis, regression-variance evaluation, and low-discrepancy/quasi-random sampling.
- Accept: design generators returning family-coded `numpy.ndarray` matrices as `StudyMethod` arms; per-axis mapping of coded matrices with `scale_samples` for unit-hypercube samples; `fracfact*`/`fracfact_aliasing` for resolution and confounding control; `saltelli_sampling`/`morris_sampling` where a normalized screening cohort feeds the SALib analyzers.
- Reject: hand-rolled Box-Behnken/central-composite/Plackett-Burman/fractional-factorial construction where the admitted generator owns it; a pyDOE3 sensitivity-index reimplementation where SALib owns the analyzer; a second coded-to-physical scaling surface beside the study `rescale` fold.
