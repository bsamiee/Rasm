# [PY_DATA_API_BW2ANALYZER]

`bw2analyzer` is the analysis layer of the Brightway cluster: contribution analysis over a computed `bw2calc.LCA` (top processes/emissions by direct impact, annotated against the `bw2data` graph), activity comparison across databases, recursive supply-chain and calculation walks, tagged-database traversal, and graph utilities (PageRank over the technosphere, database health checks). It is the DEPTH leg of the data EPD/LCA owner's Brightway arm — the `impact/impact.md` `_from_score` fold mines its contribution rows past the single-aggregate-cell floor — and it computes nothing itself: every entry consumes an already-solved `LCA` (matrices, `supply_array`, `characterized_inventory`) or the `bw2data` graph, so it never re-implements the solver, the store, or the datapackage substrate.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bw2analyzer`
- package: `bw2analyzer`
- import: `from bw2analyzer import ContributionAnalysis`
- owner: `data`
- rail: lca-analysis (EPD/LCA cluster)
- version: `0.11.8`
- license: `BSD-3-Clause`
- asset: pure Python, `py3-none-any` purelib; the numerics ride the solved `bw2calc` matrices (`numpy`/`scipy` arrive transitively)
- depends-on: `bw2calc` (the solved `LCA` every analysis consumes), `bw2data` (activity annotation via `get_activity`), `numpy`, `pandas`, `matplotlib` (the plotting extras stay unconsumed)
- entry points: library-only; no console script
- capability: sorted top-contributor extraction over any computed matrix, graph-annotated top processes/emissions, cross-database activity comparison, recursive calculation/supply-chain printing, tagged-database impact traversal, technosphere PageRank, and database health checks

## [02]-[CAPTURE]

[PUBLIC_TYPES]: the exported surface (`__all__`)
- `ContributionAnalysis` — the contribution owner: `sort_array(data, limit=25, limit_type="number", total=None)` (top-k by count or cumulative fraction), `top_processes(matrix, **kwargs)` / `top_emissions(matrix, **kwargs)` (`[value, index]` rows over a summed matrix axis), `annotated_top_processes(lca, names=True, **kwargs)` / `annotated_top_emissions(lca, names=True, **kwargs)` (`[(lca score, supply/inventory amount, activity)]` tuples — `names=True` resolves the `bw2data` activity, `False` keeps the key), `top_matrix(matrix, rows=5, cols=5)` / `hinton_matrix(lca, rows=5, cols=5)` (the dominant row×column sub-matrix), `annotate(sorted_data, rev_mapping)`, `get_name(key)`, `d3_treemap(...)`.
- `compare_activities_by_lcia_score(activities, lcia_method, band=0.125)` / `compare_activities_by_grouped_leaves(...)` / `find_differences_in_inputs(activity, ...)` — cross-database activity comparison and input-difference detection.
- `print_recursive_calculation(activity, lcia_method, amount=1, max_level=..., cutoff=...)` / `print_recursive_supply_chain(activity, ...)` — the recursive walk printers (diagnostic egress, not frame material).
- `traverse_tagged_databases(functional_unit, method, label=..., default_tag=...)` — impact aggregated by activity tag across databases.
- `PageRank(database)` — technosphere PageRank; `DatabaseHealthCheck(database)` — graph health diagnostics; `GTManipulator` — supply-chain graph-traversal post-processing.

[IMPLEMENTATION_LAW]:
- every contribution entry takes the SOLVED `bw2calc.LCA` — `lci()`/`lcia()` have already run, `characterized_inventory` and `supply_array` are populated — so the analysis is a read over the solve, never a re-solve; the `impact` owner calls `annotated_top_processes(lca, limit=n)` inside the same `_from_score` kernel that staged the solve.
- `sort_array`'s `limit_type` is the count-vs-fraction axis (`"number"` | `"percent"`); the owner passes `limit=` through the solve request's `contributions` depth, never a hardcoded 25.
- the printers (`print_recursive_*`) write to stdout — diagnostic surfaces, never composed into the normalization fold or the frame egress.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `bw2analyzer`
- Owns: contribution analysis over solved LCAs (top processes/emissions, annotated rows, dominant sub-matrices), cross-database activity comparison, recursive calculation/supply-chain walks, tagged traversal, technosphere PageRank, and database health checks
- Accept: `ContributionAnalysis().annotated_top_processes(lca, limit=n)` / `annotated_top_emissions(...)` as the mined contribution rows on the `impact` Brightway leg; `compare_activities_by_lcia_score` for cross-database reconciliation; `traverse_tagged_databases` for tag-aggregated impact when a consumer names tags
- Reject: re-implementing top-k extraction over `characterized_inventory` where `ContributionAnalysis` owns it; re-solving inside an analysis call (the solved `LCA` is the input); composing the stdout printers into the normalization fold; the matplotlib plotting extras (visualization is the artifacts plane's)
