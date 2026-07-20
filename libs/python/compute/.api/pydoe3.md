# [PY_COMPUTE_API_PYDOE3]

`pyDOE3` supplies classical design-of-experiments matrix generators — full and fractional factorial, Box-Behnken, central-composite, Plackett-Burman, generalized subset, Taguchi orthogonal-array, and Doehlert designs — beside low-discrepancy and quasi-random samplers, filling the response-surface DOE gap the SALib variance-based samplers and the `scipy.stats.qmc` floors leave open for the compute `Study` design rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyDOE3`
- package: `pyDOE3` (dist name `pydoe3`, import name `pyDOE3`)
- owner: `compute`
- module: `pyDOE3`; submodules `pyDOE3.doe_factorial`, `pyDOE3.doe_box_behnken`, `pyDOE3.doe_composite`, `pyDOE3.doe_plackett_burman`, `pyDOE3.doe_gsd`, `pyDOE3.doe_taguchi`, `pyDOE3.doe_doehlert`, `pyDOE3.doe_lhs`, `pyDOE3.doe_sparse_grid`, `pyDOE3.utils`
- asset: runtime library
- rail: experiment-design
- namespace: `pyDOE3` (every generator is re-exported at the top level; each returns a `numpy.ndarray` in its design family's coordinate system)
- requires: `numpy`, `scipy`
- capability: coded response-surface and screening design matrices (full/fractional factorial, Box-Behnken, central-composite, star, Plackett-Burman, generalized subset, Taguchi orthogonal array, Doehlert shell/simplex), design folding, aliasing analysis, regression-variance evaluation, and low-discrepancy/quasi-random samplers (Latin hypercube, Halton, Sobol', Korobov, rank-1 lattice, sparse grid, Morris/Saltelli screening) over a factor count with per-generator level and center controls

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: top-level types
- rail: experiment-design

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [ROLE]                                                 |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `TaguchiObjective`       | `enum.Enum`   | Taguchi loss goal — larger / smaller / nominal is best |
|  [02]   | `ORTHOGONAL_ARRAY_NAMES` | `Literal`     | admitted Taguchi array ids (`L4(2^3)`, `L9(3^4)`, ...) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: coded factorial designs
- rail: experiment-design
- `fullfact` returns per-factor integer level indices from `0` through `levels[i] - 1`; `ff2n` and the fractional family return two-level columns over `{-1, +1}`. Rows are runs and columns are factors.

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
- rail: experiment-design
- `bbdesign` uses `{-1, 0, +1}` factor levels. `ccdesign` and `star` add axial `+/- alpha` levels to factorial and center points; `pbdesign` uses `{-1, +1}`, `gsd` uses per-factor level indices, `taguchi_design` emits the supplied factor settings, and `doehlert_*` emits its coded shell coordinates.

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
- rail: experiment-design
- `lhs`, `halton_sequence`, `sobol_sequence`, `korobov_sequence`, `rank1_lattice`, `sukharev_grid`, `random_uniform`, and `doe_sparse_grid` sample the unit hypercube; `morris_sampling` and `saltelli_sampling` emit normalized screening coordinates. Full signatures: `lhs(n, samples, criterion, iterations, random_state, correlation_matrix, seed)` — `criterion` ∈ `center`/`maximin`/`centermaximin`/`correlation`; `random_k_means(num_points, dimension, num_steps, initial_points, callback, random_state, seed)`; `doe_sparse_grid` `grid_type` ∈ `clenshaw_curtis`/`chebyshev`/`gauss_patterson`.

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
- rail: experiment-design

- return types: `list_orthogonal_arrays() -> list[str]`, `get_orthogonal_array(oa_name) -> np.ndarray`, `compute_snr(responses, objective=TaguchiObjective.LARGER_IS_BETTER) -> float`; `var_regression_matrix` model tokens read like `"1 x1 x2 x1*x2"`.

| [INDEX] | [SURFACE]                                     | [RAIL]                                         |
| :-----: | :-------------------------------------------- | :--------------------------------------------- |
|  [01]   | `list_orthogonal_arrays()`                    | list admitted Taguchi array ids                |
|  [02]   | `get_orthogonal_array(oa_name)`               | coded orthogonal array for `oa_name`           |
|  [03]   | `compute_snr(responses, objective)`           | Taguchi signal-to-noise over a trial's repeats |
|  [04]   | `var_regression_matrix(H, x, model, sigma=1)` | prediction variance at `x` for `model` tokens  |
|  [05]   | `build_regression_matrix(H, model, build)`    | expand a design into the regression matrix     |
|  [06]   | `scale_samples(samples, bounds)`              | affine-scale unit samples into `bounds`        |

## [04]-[IMPLEMENTATION_LAW]

[PYDOE3_TOPOLOGY]:
- design coordinates: `fullfact`/`gsd` use integer level indices, `ff2n`/`fracfact*`/`pbdesign` use `{-1, +1}`, `bbdesign` adds `0`, and `ccdesign`/`star` add `+/- alpha`; `fold` preserves the input levels and appends their sign-reversed mirror. Per-axis policy maps each family coordinate system onto physical factor bounds.
- `ccdesign` composes a two-level factorial (or resolution-V fraction) with `star` axial points and `center` block replicates; `face="faced"` fixes `alpha = 1` (in-cube), `circumscribed`/`inscribed` place the star points at the variance-driven `alpha` distance; `alpha="rotatable"` targets uniform prediction variance on a sphere.
- `fracfact` selects columns by a generator string (`"a b ab"`); `fracfact_by_res` builds a minimum-aberration design of a requested resolution; `fracfact_opt` searches generators minimizing the aliasing vector; `fracfact_aliasing` and `alias_vector_indices` expose the confounding structure a downstream screening decision reads.
- `taguchi_design(oa_name, levels_per_factor)` maps a named orthogonal array (`list_orthogonal_arrays`/`get_orthogonal_array`) onto actual factor settings; `compute_snr` reduces a trial's repeated responses to the Taguchi objective's signal-to-noise scalar.
- samplers split by input contract — `lhs`/`sobol_sequence`/`halton_sequence`/`korobov_sequence`/`rank1_lattice`/`sukharev_grid` take `(points, dimension)` unit-hypercube shape, `morris_sampling`/`saltelli_sampling` take `(num_vars, N)` screening shape, and `doe_sparse_grid` emits Smolyak quadrature nodes sized by `sparse_grid_dimension`.

[PYDOE3_STACKING]:
- study design rail: response-surface generators are `StudyMethod` design arms beside the hand-rolled `factorial` meshgrid and the `scipy.stats.qmc` engines — a Box-Behnken, central-composite, Plackett-Burman, or fractional-factorial row maps through the same per-`ParamAxis` family-coordinate fold, so the design matrix and the SALib/surrogate response contract stay identical to the sampling methods.
- sensitivity rail: `saltelli_sampling`/`morris_sampling` emit the same screening matrices SALib's `sobol`/`morris` samplers own; the SALib `ProblemSpec` pipeline remains the analyzer owner, and pyDOE3 samplers feed it only where a coded classical design is the required cohort rather than a variance-based sample.
- numpy rail: every design and sample is a plain `numpy.ndarray`, crossing into the study spine and the `data/tabular` DOE-frame gate as float64 columns without a private container.
- regression rail: `build_regression_matrix`/`var_regression_matrix` lower a coded design into the model regression matrix and its prediction-variance field, feeding the polynomial-surrogate screening diagnostic without a hand-built Vandermonde.

[LOCAL_ADMISSION]:
- Use the coded factorial/response-surface generators for classical DOE cohorts; use `scipy.stats.qmc` for pure space-filling QMC and SALib samplers for variance-based sensitivity — pyDOE3 owns the response-surface and screening design matrices, not the sensitivity indices.
- Map each design family's coordinates onto physical bounds through the study's per-axis fold; unit-hypercube samples alone use `scale_samples` directly.
- Capture the design method, factor count, center/star controls, and `seed` in the study receipt so a resumed run rebuilds the identical coded cohort.

[RAIL_LAW]:
- Package: `pyDOE3`
- Owns: classical design-of-experiments matrix generation — full/fractional factorial, Box-Behnken, central-composite, star, Plackett-Burman, generalized subset, Taguchi orthogonal-array, and Doehlert designs — with design folding, aliasing analysis, regression-variance evaluation, and low-discrepancy/quasi-random samplers
- Accept: design generators returning family-coded `numpy.ndarray` matrices as `StudyMethod` arms; per-axis mapping of coded matrices and `scale_samples` for unit-hypercube samples; `fracfact*`/`fracfact_aliasing` for resolution and confounding control; `saltelli_sampling`/`morris_sampling` where a normalized screening cohort feeds the SALib analyzers
- Reject: hand-rolled Box-Behnken/central-composite/Plackett-Burman/fractional-factorial matrix construction where the admitted generator owns it; pyDOE3 sensitivity-index reimplementation where SALib owns the analyzer; a second coded-to-physical scaling surface beside the study `rescale` fold
