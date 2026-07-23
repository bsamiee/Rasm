# [PY_DATA_API_BW2ANALYZER]

`bw2analyzer` is the analysis leg of the Brightway rail: it reads an already-solved `bw2calc.LCA` or the `bw2data` graph and extracts what dominates a result — top contributing processes and emissions, cross-database activity comparison, tag-aggregated traversal, technosphere PageRank, and graph health. It computes nothing itself: every entry consumes a populated `characterized_inventory`/`supply_array` or the activity graph, so the solver, the store, and the datapackage substrate stay owned elsewhere.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bw2analyzer`
- package: `bw2analyzer` (BSD-3-Clause)
- module: `bw2analyzer` (`from bw2analyzer import ContributionAnalysis`)
- namespaces: `bw2analyzer` (library-only, no console script)
- asset: pure-Python `py3-none-any` purelib; numerics ride the solved `bw2calc` matrices, `numpy`/`scipy` arrive transitively
- depends: `bw2calc` (the solved `LCA` every analysis consumes), `bw2data` (activity annotation via `get_activity`)
- rail: lca-analysis (EPD/LCA cluster)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the exported analysis owners (`__all__`)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :--------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `ContributionAnalysis` | class         | top-contributor extraction and annotation over a solved LCA            |
|  [02]   | `PageRank`             | class         | technosphere PageRank; `.calculate()` returns the ranked vector        |
|  [03]   | `DatabaseHealthCheck`  | class         | graph diagnostics; `.check(graphs_dir=None)` reports the health record |
|  [04]   | `GTManipulator`        | class         | supply-chain graph post-processing over static folds                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: contribution extraction, cross-database comparison, tagged traversal, and the graph folds
- `ContributionAnalysis` `top_*`/`annotated_top_*` carry: `**kwargs` -> `sort_array` (`limit`, `limit_type`, `total`)

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `sort_array(data, limit=25, limit_type, total=None)`              | instance | top-k rows by count or fraction                    |
|  [02]   | `top_processes(matrix, **kwargs)`                                 | instance | `[value, index]` over the summed technosphere axis |
|  [03]   | `top_emissions(matrix, **kwargs)`                                 | instance | `[value, index]` over the summed biosphere axis    |
|  [04]   | `annotated_top_processes(lca, names=True, **kwargs)`              | instance | `(score, supply, activity)` tuples                 |
|  [05]   | `annotated_top_emissions(lca, names=True, **kwargs)`              | instance | `(score, inventory, flow)` tuples                  |
|  [06]   | `top_matrix(matrix, rows=5, cols=5)`                              | instance | dominant row×column sub-matrix                     |
|  [07]   | `hinton_matrix(lca, rows=5, cols=5)`                              | instance | Hinton sub-matrix over a solved LCA                |
|  [08]   | `annotate(sorted_data, rev_mapping)` / `get_name(key)`            | instance | reattach keys, resolve a key to its name           |
|  [09]   | `d3_treemap(matrix, rev_bio, rev_techno, limit=0.025)`            | instance | nested treemap structure for a result              |
|  [10]   | `compare_activities_by_lcia_score(activities, method, band=0.1)`  | static   | flag activities whose score diverges beyond `band` |
|  [11]   | `compare_activities_by_grouped_leaves(activities, method)`        | static   | compare by CPC-grouped supply-chain leaves         |
|  [12]   | `find_differences_in_inputs(activity, as_dataframe=False)`        | static   | input differences among same-name activities       |
|  [13]   | `traverse_tagged_databases(functional_unit, method, label="tag")` | static   | impact aggregated by activity tag across databases |
|  [14]   | `print_recursive_calculation(activity, method, amount=1)`         | static   | stdout supply-chain score walk (diagnostic)        |
|  [15]   | `print_recursive_supply_chain(activity, amount=1)`                | static   | stdout supply-chain input walk (diagnostic)        |
|  [16]   | `GTManipulator.unroll_graph(nodes, edges, score, cutoff=0.005)`   | static   | unroll a supply-chain graph to bounded links       |
|  [17]   | `GTManipulator.simplify(nodes, edges, score, limit=0.005)`        | static   | prune below-threshold nodes from the graph         |

- `annotated_top_*`: `names=False` keeps the integer matrix key; `names=True` calls `bw2data.get_activity`, the annotation seam.
- `print_recursive_*`: stdout diagnostic surfaces, never composed into a normalization fold or a frame egress.
- Comparison/traversal knobs: `mode`/`max_level`/`cutoff` tune leaf grouping depth, `rel_tol`/`abs_tol`/`locations` scope input diffs, `secondary_tags`/`fg_databases` scope the tag walk.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every contribution entry reads an already-solved `bw2calc.LCA` — `lci()`/`lcia()` have run and `characterized_inventory`/`supply_array` are populated — so analysis is a read over the solve, never a re-solve.
- `sort_array`'s `limit_type` is the count-vs-fraction axis (`"number"` | `"percent"`); the consumer threads its depth through the solve request, never a hardcoded 25.

[STACKING]:
- `bw2calc`(`.api/bw2calc.md`): every `ContributionAnalysis` entry consumes a solved `bc.LCA`; the analysis mines `characterized_inventory`/`supply_array` and the matrix dicts the solver populated, and re-solving inside an analysis call is rejected.
- `bw2data`(`.api/bw2data.md`): `annotated_top_*(names=True)` resolves each matrix key to an activity via `get_activity`; `PageRank(database)` and `DatabaseHealthCheck(database)` traverse the SQLite graph directly.
- `impact` owner: the `_from_score` fold calls `annotated_top_processes(lca, limit=n)` inside the same kernel that staged the solve, mining contribution rows past the single-aggregate-cell floor.

[LOCAL_ADMISSION]:
- `ContributionAnalysis` is the sole top-k owner admitted over a solved matrix; a consumer names tags to reach `traverse_tagged_databases`, and cross-database reconciliation reaches `compare_activities_by_lcia_score`.

[RAIL_LAW]:
- Package: `bw2analyzer`
- Owns: contribution analysis over solved LCAs (top processes/emissions, annotated rows, dominant sub-matrices), cross-database activity comparison, recursive calculation/supply-chain walks, tagged traversal, technosphere PageRank, and database health checks
- Accept: `ContributionAnalysis().annotated_top_processes(lca, limit=n)`/`annotated_top_emissions(...)` as the mined contribution rows; `compare_activities_by_lcia_score` for cross-database reconciliation; `traverse_tagged_databases` for tag-aggregated impact
- Reject: re-implementing top-k extraction over `characterized_inventory` where `ContributionAnalysis` owns it; re-solving inside an analysis call; composing the stdout printers into a normalization fold; the matplotlib plotting extras (visualization is the artifacts plane's)
