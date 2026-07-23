# [PY_DATA_API_BW2CALC]

`bw2calc` is the Brightway LCA calculation engine: from a functional-unit demand dict and `bw_processing` datapackages it assembles the technosphere, biosphere, and characterization `scipy` sparse matrices, solves `A·supply = demand`, then folds `inventory = B·supply` and `characterized_inventory = C·inventory` to `score`. It owns the solver hierarchy and `stats_arrays` uncertainty propagation as the pure compute layer that never opens the graph `bw2data` owns, authors the datapackage format `bw_processing` owns, or re-implements the factorization `scipy` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bw2calc`
- package: `bw2calc` (`BSD-3-Clause`, Brightway Developers)
- module: `bw2calc` (`import bw2calc as bc`)
- rail: LCA linear-system solve engine for the EPD/LCA cluster
- asset: pure-Python `py3-none-any` purelib, zero compiled extensions; numerics ride `scipy` sparse + `numpy`, matrix mapping `matrix_utils.MappedMatrix`, uncertainty `stats_arrays`, graph traversal `bw_graph_tools`
- accel: import-time probe selects a fast factorizer arch-keyed — `pypardiso` (x64 MKL PARDISO) then `scikit-umfpack` (ARM UMFPACK) — else `scipy` SuperLU `factorized`/`spsolve`; `presamples.PackagesDataLoader` sources pre-sampled arrays

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver-engine hierarchy (`LCABase`/`Iterator` subclasses) and the `pydantic` LCIA config model

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `LCA`                      | class         | sparse SuperLU factorize-and-solve engine    |
|  [02]   | `DenseLCA`                 | class         | dense `numpy.linalg` solve for small systems |
|  [03]   | `IterativeLCA`             | class         | iterative `Ax=b` for large sparse            |
|  [04]   | `JacobiGMRESLCA`           | class         | GMRES with Jacobi preconditioner             |
|  [05]   | `LeastSquaresLCA`          | class         | non-square technosphere via `lsqr`           |
|  [06]   | `CachingLCA`               | class         | supply vectors cached by demand              |
|  [07]   | `PartitionedMonteCarloLCA` | class         | Monte Carlo over fixed background            |
|  [08]   | `MultiLCA`                 | class         | many-demand many-method batch build          |
|  [09]   | `FastScoresOnlyMultiLCA`   | class         | chunked scores-only fast path                |
|  [10]   | `MethodConfig`             | model         | `pydantic` LCIA tree validated pre-solve     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: staged LCA lifecycle, factorization-reuse rail, and result projection
- `LCA` and `MultiLCA` carry: `data_objs`, `remapping_dicts`, `log_config`, `seed_override`, `use_arrays`, `use_distributions`, `selective_use`

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `LCA(demand, method, weighting, normalization)`                                 | ctor     | base engine over one functional unit    |
|  [02]   | `MultiLCA(demands, method_config, data_objs)`                                   | ctor     | batch over many demands and methods     |
|  [03]   | `lci(demand, factorize)`                                                        | instance | matrix build and inventory solve        |
|  [04]   | `lcia(demand)`                                                                  | instance | characterization build to `score`       |
|  [05]   | `to_dataframe(matrix_label, row_dict, col_dict, annotate, cutoff, cutoff_mode)` | instance | top-contributor table over any matrix   |
|  [06]   | `MultiLCA.filter_package_by_identifier(data_objs, identifier)`                  | instance | select datapackages for one matrix list |

- Factorization reuse: `redo_lci(demand)` / `redo_lcia(demand)` recompute a new demand and `switch_method(method)` / `switch_normalization(normalization)` / `switch_weighting(weighting)` swap an LCIA stage, both on the kept factorization; `normalize()` / `weight()` apply the loaded stages after `lcia`.

[RAW_SOLVE]: `invert_technosphere_matrix()` `decompose_technosphere()` `solve_linear_system(demand)` `build_demand_array(demand)` `keep_first_iteration()`
[MATRICES]: `technosphere_matrix` `biosphere_matrix` `characterization_matrix` `normalization_matrix` `weighting_matrix` `technosphere_mm` `biosphere_mm`
[VECTORS]: `supply_array` `inventory` `characterized_inventory` `demand_array`
[PROPERTIES]: `score` `activity_dict` `product_dict` `biosphere_dict` `matrix_labels` `MultiLCA.scores` `MultiLCA.matrix_list_labels`
- `LCA`/`MultiLCA` are iterators: `next(instance)` yields a Monte Carlo sample under `use_distributions`/`use_arrays`, `selective_use` scoping stochasticity per matrix.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `bw2calc` is stateless over storage — it consumes `bw_processing` datapackages and never opens a database; the staged `lci`->`lcia`->`normalize`/`weight` calls are cumulative, a parameter sweep running `lci`/`lcia` once then looping `redo_lci`/`redo_lcia` on the kept factorization rather than reconstructing `LCA`.
- Solver subclass follows matrix shape, never a default; the `[02]` capability column is the selection key.
- Monte Carlo is iteration, not a method: the same object yields a new sample per `next()`, uncertainty living in the datapackage `UNCERTAINTY_DTYPE` arrays sampled by `stats_arrays`, `seed_override` making a run reproducible.
- `MethodConfig` is the typed multi-LCIA contract: its `pydantic` validators check impact-category, normalization, and weighting cross-references before any solve.

[STACKING]:
- `bw2data`(`.api/bw2data.md`): `prepare_lca_inputs` / `get_multilca_data_objs` feed `LCA(data_objs=..., remapping_dicts=[...])` — the canonical input bridge minting `data_objs` and `remapping_dicts` in the integer-id matrix space.
- `bw-processing`(`.api/bw-processing.md`): `data_objs` are its `Datapackage` objects, whose `INDICES_DTYPE` groups `matrix_utils.MappedMatrix` maps into the sparse matrices.
- `openepd`(`.api/openepd.md`) / `epdx`(`.api/epdx.md`): characterized `score` recomputes and reconciles the impacts an openEPD/EPDx payload declares.
- `olca-ipc`(`.api/olca-ipc.md`): remote peer running the equivalent solve on an openLCA server; `bw2calc` stays the in-process engine when the background lives in Brightway.
- `premise`(`.api/premise.md`): emits prospective `bw2data` databases the identical `LCA`/`MultiLCA` surface solves for future-scenario impacts.
- `numpy`(`.api/numpy.md`): matrices are `scipy` sparse and vectors `numpy`; read `supply_array`/`characterized_inventory` as arrays into the numerics rail, densifying only inside `DenseLCA`.
- `pydantic`(`.api/pydantic.md`): `MethodConfig` is a `pydantic` v2 `BaseModel`; validate the LCIA tree and `model_dump()` it into the calculation receipt.
- `anyio`(`.api/anyio.md`): `lci()`/`lcia()` are CPU-bound synchronous `scipy`; run them under `anyio.to_thread.run_sync`, or a process pool for a Monte Carlo fan-out.
- `structlog`(`.api/structlog.md`) / `opentelemetry`(`.api/opentelemetry-api.md`): `log_config` wires the logger; emit `score`/`method`/functional-unit as a structured event and span the solve.
- `msgspec`(`.api/msgspec.md`): fold `(functional_unit, method, score, n_iterations)` into a `msgspec.Struct` receipt as the typed egress shape.
- data folder: obtain `data_objs` through the `bw2data` bridge, run the staged `lci`->`lcia` pipeline once, then reuse the factorization across alternatives via `redo_lci`/`switch_*` with the solver subclass matched to matrix shape.

[LOCAL_ADMISSION]:
- `LCA`/`MultiLCA` own every in-repo LCA solve; inputs arrive as `bw_processing` datapackages through the `bw2data` bridges, the solver subclass chosen by matrix shape.

[RAIL_LAW]:
- Package: `bw2calc`
- Owns: LCA linear-system solve (`supply = A⁻¹·demand`), inventory and characterized-inventory computation, the four-strategy solver hierarchy, Monte Carlo and array uncertainty propagation, multi-demand multi-method batch scoring, technosphere inversion, and top-contributor dataframes
- Accept: `LCA`/`MultiLCA` owners; `data_objs` from `bw2data.prepare_lca_inputs` / `get_multilca_data_objs`; the staged `lci`->`lcia`->`normalize`/`weight` pipeline with `redo_lci`/`redo_lcia`/`switch_*` reuse; solver subclass by matrix shape; `use_distributions`/`use_arrays` iteration with `seed_override`; `MethodConfig` validation; `to_dataframe` contributions
- Reject: re-implementing the sparse solve `scipy` owns; reading SQLite instead of the `bw2data` bridges; reconstructing `LCA` per alternative where `redo_lci`/`CachingLCA` reuse the factorization; per-iteration re-solve where `PartitionedMonteCarloLCA`/`keep_first_iteration` exist; hand-assembling matrices `bw_processing`/`matrix_utils` own; a parallel config object where `MethodConfig` is the typed owner
