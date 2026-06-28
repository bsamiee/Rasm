# [PY_DATA_API_BW2CALC]

`bw2calc` is the LCA calculation engine of the Brightway cluster: given a functional unit (a `{id: amount}` demand dict) and one or more `bw_processing` datapackages, it assembles the technosphere, biosphere, and characterization `scipy` sparse matrices (via `matrix_utils.MappedMatrix`), solves the linear system `A·supply = demand`, then computes the life-cycle inventory `inventory = B·supply` and the characterized inventory `characterized_inventory = C·inventory`, whose sum is the impact `score`. It owns the full solver hierarchy (sparse SuperLU default, dense, iterative GMRES, least-squares for non-square systems, supply-vector caching, partitioned Monte Carlo) and stochastic iteration (`use_distributions`/`use_arrays` propagate `stats_arrays` uncertainty per matrix sample). It is the pure compute layer: it never touches the SQLite graph (`bw2data` owns that) nor authors the datapackage format (`bw_processing` owns that), and it never re-implements the sparse factorization `scipy` provides.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bw2calc`
- package: `bw2calc`
- import: `import bw2calc as bc`
- owner: `data`
- rail: lca-engine (EPD/LCA cluster)
- version: `2.5.0`
- license: `BSD-3-Clause` (declared in package metadata)
- asset: pure Python (zero compiled extensions); the numerics are `scipy` sparse + `numpy`, the matrix mapping is `matrix_utils`, and the uncertainty distributions are `stats_arrays`. `Requires-Python >=3.9`
- depends-on: `bw_processing>=1.0` (the datapackage input format), `matrix_utils>=0.6` (the datapackage->`scipy`-sparse mapper, surfaced as `technosphere_mm`/`biosphere_mm`), `scipy` (SuperLU/`spsolve`/iterative solvers), `numpy<3`, `stats_arrays` (the `UNCERTAINTY_DTYPE` distribution samplers for Monte Carlo), `pandas` (`to_dataframe`), `xarray` (labeled multi-result arrays), `pydantic` (`MethodConfig`), `fsspec`, `bw_graph_tools>=0.8`
- optional accel: `scikit-umfpack` swaps the sparse backend to UMFPACK (warns once on ARM at import if absent); it is a performance dependency, not required for correctness
- marker: COMPANION-GATED. Pinned `bw2calc; python_version<'3.15'`. Pure-Python package; the gate is TRANSITIVE — `numpy<3`, `scipy`, `pandas` lack `cp315` wheels at admission, so the cluster pins `<3.15`. `assay api resolve bw2calc` cannot reflect on the active `cp315` interpreter; this surface is verified against the real `bw2calc 2.5.0` wheel on an isolated `cp313` install (`matrix_utils 0.8`, `scipy 1.18`).
- entry points: library-only; no console script
- capability: deterministic single-demand LCA, multi-demand x multi-method batch scoring, four solver strategies for square/non-square/large-sparse/iterative systems, supply-vector caching, factorization reuse across redemands and method switches, Monte Carlo uncertainty propagation (sampling) and pre-sampled array iteration, technosphere inversion, and top-contributor dataframes

## [02]-[CAPTURE]

[PUBLIC_TYPES]: the solver hierarchy (all `LCABase` subclasses, all `Iterator`)
- `bc.LCA(demand, method=None, weighting=None, normalization=None, data_objs=None, remapping_dicts=None, log_config=None, seed_override=None, use_arrays=False, use_distributions=False, selective_use=False)` — the base engine: sparse SuperLU factorization + solve. `data_objs` is the list of `bw_processing` datapackages (or `fsspec` paths); `remapping_dicts` restores `(database, code)` keys onto the integer-indexed result dicts.
- `bc.DenseLCA` — converts the technosphere to a dense array and solves with `numpy.linalg` (small fully-connected systems).
- `bc.IterativeLCA` — solves `Ax=b` with iterative methods instead of a direct factorization (very large sparse systems where LU is too costly).
- `bc.JacobiGMRESLCA` — GMRES with a Jacobi preconditioner; overrides `solve_linear_system` and the Monte Carlo `__next__`.
- `bc.LeastSquaresLCA` — overdetermined/non-square technosphere (more products than activities) via `scipy` `lsqr`; overrides `decompose_technosphere`.
- `bc.CachingLCA` — caches supply vectors keyed by demand so repeated `redo_lci` over seen demands skip the solve.
- `bc.PartitionedMonteCarloLCA` — pre-solves the static background system once, then iterates only the foreground (fast uncertainty over a fixed background).
- `bc.MultiLCA(demands, method_config, data_objs, remapping_dicts=None, log_config=None, seed_override=None, use_arrays=False, use_distributions=False, selective_use=None)` — many functional units x many impact categories in one build; exposes `.scores` and `matrix_list_labels`.
- `bc.FastScoresOnlyMultiLCA` — chunked, scores-only fast path over a `MultiLCA` shape (skips retaining per-cell characterized inventories).
- `bc.MethodConfig` — a `pydantic` `BaseModel` with fields `impact_categories`, `normalizations`, `weightings`; validators (`normalizations_cover_all_impact_categories`, `normalizations_reference_impact_categories`, `weightings_*`) enforce a coherent LCIA tree. This is the typed config `MultiLCA` and `bw2data.get_multilca_data_objs` consume.

[CALCULATION]: the `LCA` lifecycle (one polymorphic surface, staged)
- `lci(demand=None, factorize=False)` — build the technosphere + biosphere matrices, solve `supply_array`, compute `inventory`; `factorize=True` caches the LU for reuse.
- `lcia(demand=None)` — build the `characterization_matrix`, compute `characterized_inventory`; `score` is its sum.
- `normalize()` / `weight()` — apply the loaded normalization / weighting stages (after `lcia`).
- `redo_lci(demand=None)` / `redo_lcia(demand=None)` — recompute for a new demand reusing the existing factorization/characterization (the per-alternative loop, far cheaper than a fresh `LCA`).
- `switch_method(method)` / `switch_weighting(weighting)` / `switch_normalization(normalization)` — swap the LCIA stage in place without rebuilding the inventory.
- `invert_technosphere_matrix()` — full `A⁻¹` (the supply matrix for all demands at once); `decompose_technosphere()` factorizes; `solve_linear_system(demand=None)` is the raw solve; `build_demand_array(demand=None)` materializes the RHS vector.
- `to_dataframe(matrix_label='characterized_inventory', row_dict=None, col_dict=None, annotate=True, cutoff=200, cutoff_mode='number') -> pandas.DataFrame` — the top-contributor table (by count or fraction) over any computed matrix.
- Monte Carlo: construct with `use_distributions=True` (sample `UNCERTAINTY_DTYPE` rows) and/or `use_arrays=True` (cycle pre-sampled array columns), then iterate the instance (`next(lca)` / `for _ in lca`); `keep_first_iteration()` holds the deterministic first sample. `selective_use={'technosphere matrix': {'use_distributions': True}}` scopes stochasticity per matrix.
- `MultiLCA`: `lci()` then `lcia()`, read `.scores`; `filter_package_by_identifier(data_objs, identifier)` selects the datapackages for one named matrix list.

[RESULT_STATE]: instance attributes set during calculation (not class-level)
- matrices: `technosphere_matrix`, `biosphere_matrix`, `characterization_matrix`, `normalization_matrix`, `weighting_matrix` (`scipy` sparse); the `matrix_utils.MappedMatrix` wrappers `technosphere_mm` / `biosphere_mm`.
- vectors: `supply_array`, `inventory`, `characterized_inventory`, `demand_array`.
- properties: `score` (the characterized-inventory sum), `activity_dict`, `product_dict`, `biosphere_dict` (id<->matrix-position maps, key-remapped when `remapping_dicts` is given), `matrix_labels`; `MultiLCA.scores`.

[IMPLEMENTATION_LAW]:
- `bw2calc` is stateless over storage: it consumes `data_objs` (`bw_processing` datapackages) and never opens a database. Always obtain `data_objs` from `bw2data.prepare_lca_inputs` (single) or `bw2data.get_multilca_data_objs` (multi) — those produce the datapackage list and the `remapping_dicts` in the integer-id space the matrices use.
- The staged calls are cumulative: `lci()` must precede `lcia()` must precede `normalize()`/`weight()`. For a parameter sweep over alternatives, call `lci()`/`lcia()` once then loop `redo_lci(new_demand)` / `redo_lcia(new_demand)` — never reconstruct `LCA` per alternative.
- Pick the solver subclass by matrix shape, not by default: `LCA` (sparse, the norm), `DenseLCA` (tiny dense), `LeastSquaresLCA` (non-square, more products than processes), `IterativeLCA`/`JacobiGMRESLCA` (very large sparse), `CachingLCA` (many repeated demands), `PartitionedMonteCarloLCA` (MC over a fixed background).
- Monte Carlo is iteration, not a method: the same `LCA` object yields a new sample per `next()`; the uncertainty lives in the datapackage `UNCERTAINTY_DTYPE` arrays, sampled by `stats_arrays`. `seed_override` makes a run reproducible.
- `MethodConfig` is the typed contract for multi-LCIA: build it as a `pydantic` model (or a dict it validates) so the impact-category / normalization / weighting cross-references are checked before any solve.

[SUBSTRATE_STACK]: stacking onto the universal Python rails (`libs/python/.api/`)
- `numpy` + `scipy`: the matrices are `scipy` sparse, the vectors `numpy`; never densify a sparse matrix outside `DenseLCA`, and read `supply_array`/`characterized_inventory` as `numpy` arrays into the numerics rail.
- `pydantic`: `MethodConfig` IS a `pydantic` v2 `BaseModel` — validate the LCIA tree through the same rail the rest of the system uses, and `model_dump()`/`model_dump_json()` it into the calculation receipt.
- `anyio`: the solve is CPU-bound synchronous `scipy`; run `lci()`/`lcia()` under `anyio.to_thread.run_sync` (or a process pool for a Monte Carlo fan-out) so the structured-concurrency event loop stays free.
- `stamina`: when the inputs come from a remote store (`fsspec` object backend, or an `olca-ipc` background), wrap the fetch in the retry rail — the solve itself is deterministic and never retried.
- `structlog` / `opentelemetry`: `log_config` wires `bw2calc`'s logger; emit `score`, `method`, and the functional unit as a structured event and wrap the solve in a span so a long Monte Carlo run is observable.
- `msgspec`: fold `(functional_unit, method, score, n_iterations)` into a `msgspec.Struct` receipt — the typed egress shape for the impact result.

[SIBLING_STACK]: stacking with the EPD/LCA folder cluster
- `bw2data`: `fu, data_objs, remapping = bw2data.prepare_lca_inputs(demand, method=...)` then `bc.LCA(demand=fu, data_objs=data_objs, remapping_dicts=[remapping])` — the canonical bridge. `bw2data.get_multilca_data_objs(functional_units, method_config)` produces the `MultiLCA` `data_objs`.
- `bw_processing`: the `data_objs` are its `Datapackage` objects; `matrix_utils.MappedMatrix` (a `bw2calc` dependency) maps their `INDICES_DTYPE` groups into the sparse matrices.
- `openepd` / `epdx`: the characterized `score` (GWP, etc.) reconciles against the declared impacts in an openEPD/EPDx payload — `bw2calc` is the engine that recomputes what an EPD asserts.
- `olca-ipc`: the remote alternative — `olca-ipc` runs the equivalent calculation on an openLCA server; `bw2calc` is the in-process engine for the same result, chosen when the background lives in Brightway rather than openLCA.
- `premise`: emits prospective `bw2data` databases; `bw2calc` then solves them for future-scenario impacts with the identical `LCA`/`MultiLCA` surface (install `premise[bw25]` so its `bw2calc>=2.0.1` pin binds this `2.5.0` line).

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `bw2calc`
- Owns: the LCA linear-system solve (`supply = A⁻¹·demand`), inventory and characterized-inventory computation, the four-strategy solver hierarchy, Monte Carlo / array uncertainty propagation, multi-demand x multi-method batch scoring, technosphere inversion, and top-contributor dataframes
- Accept: `LCA`/`MultiLCA` as the owners; `data_objs` from `bw2data.prepare_lca_inputs` / `get_multilca_data_objs`; the staged `lci`->`lcia`->`normalize`/`weight` pipeline with `redo_lci`/`redo_lcia` + `switch_*` for reuse; the solver subclass matched to matrix shape; `use_distributions`/`use_arrays` + iteration for Monte Carlo with `seed_override` for reproducibility; `MethodConfig` `pydantic` validation; `to_dataframe` for contributions
- Reject: re-implementing the sparse solve/factorization `scipy` owns; reading SQLite directly instead of the `bw2data` bridges; reconstructing `LCA` per alternative when `redo_lci`/`CachingLCA` reuse the factorization; full per-iteration re-solve in Monte Carlo when `PartitionedMonteCarloLCA`/`keep_first_iteration` exist; hand-assembling matrices that `bw_processing` + `matrix_utils` own; a parallel impact-category config object when `MethodConfig` is the typed owner
