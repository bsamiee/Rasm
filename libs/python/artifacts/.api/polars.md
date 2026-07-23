# [PY_ARTIFACTS_API_POLARS]

`polars` inside `artifacts` is the publication-boundary overlay onto the `data`-owned columnar engine, owning only the members that cross into the visualization plane: the `DataFrame.style → great_tables.GT` construction edge, the `pl.Expr` summary/predicate language great-tables folds, the `pl.Series → vals.fmt_*` formatter edge, and the `pl.DataFrame` adjacency/attribute frame that lowers into a graph. Artifacts styles, aggregates, and lowers a settled frame; it never authors data, re-opens the lazy engine, or re-implements a `data`-owned transform.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `polars` (artifacts overlay)
- package: `polars` (MIT)
- module: `polars`
- namespaces: `polars.selectors`, `polars.exceptions`
- engine: full eager/lazy/`Expr`/plugin surface owned by `data/.api/polars.md`
- rail: tabular boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: boundary types the artifacts pages carry

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [CAPABILITY]                                                                        |
| :-----: | :----------------------- | :-------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `DataFrame`              | eager frame     | settled frame; `.style` is the GT seam, `.to_dicts`/`.to_numpy` the egress          |
|  [02]   | `Series`                 | typed column    | single column into the standalone `vals.fmt_*` and `.to_numpy`/`.to_list` egress    |
|  [03]   | `Expr`                   | expression node | the aggregate/predicate the table owner builds for `summary_rows`/`loc.body(mask=)` |
|  [04]   | `DataType` family        | value object    | per-column dtype selecting the `FmtKind` and `cs.by_dtype(...)` set                 |
|  [05]   | `Config` / `StringCache` | render context  | deterministic display/precision and a shared categorical string-cache scope         |

- `Expr`: unevaluated until great-tables or polars runs it over the frame, never a Python lambda over rows.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the great-tables construction seam (`visualization/table`)
- [GT_SEAM]: `DataFrame.style -> GT` `GT(frame, rowname_col=, groupname_col=, locale=, id=)` — in-process styled-table construction from the frame with no frame→table marshalling; `.style` folds the `TableOp` sequence, the explicit ctor is the fall-through when knobs diverge from default.
- [SUMMARY_EXPR]: `pl.col(name).mean()` `.sum()` `.median()` `.std()` `.quantile()` `.min()` `.max()` `.n_unique()` `.alias(name)` — each `summary_rows`/`grand_summary_rows` `fns` value names its own column, great-tables rejecting a non-`None` `columns`.
- [MASK_PREDICATE]: `pl.col(name) > v` `.is_between(lo, hi)` `.is_in(set)` `.is_null()` `& | ~` — the boolean `Expr` for `loc.body(mask=)` cell targeting, mutually exclusive with `columns`/`rows`.
- [DERIVED_CELL]: `pl.when(pred).then(x).otherwise(y)` `pl.lit(v)` `pl.format(tmpl, *exprs)` `pl.concat_str(exprs, separator=)` `pl.coalesce(*exprs)` `pl.struct(*exprs)` — derive a display column inside `select`/`with_columns` before `.style`.
- [COLUMN_SELECTOR]: `cs.numeric()` `cs.by_dtype(dt)` `cs.matches(re)` `cs.starts_with(p)` `cs.exclude(...)` `& | - ~` — the declarative column set where a `TableOp` takes `Cols`.
- [FRAME_SHAPING]: `select` `with_columns` `filter` `sort(nulls_last=)` `rename` `cast` `head` `slice` `pivot` `unpivot` `group_by().agg` `top_k(by=)` `drop` — last-mile display-shape of the settled frame.
- [STRING_EXPR]: `Expr.str.extract(pat, group_index=)` `.str.extract_all(pat)` `.str.replace(...)` `.str.strip_chars()` `.str.to_uppercase()` — derive a sort or display token in-engine.

[ENTRYPOINT_SCOPE]: standalone Series formatting, the diagram-graph lowering, and interop egress
- [SERIES_PROJECTION]: `DataFrame.get_column(name)` `df[name]` `Series.to_list()` `.cast(dt)` `.round(n)` `.fill_null(v)` `.rename(name)` `.to_frame()` — the out-of-table value source `vals.fmt_*` formats to `list[str]`.
- [ROW_LOWERING]: `DataFrame.to_dicts()` `.iter_rows(named=)` `.rows()` — the one deliberate frame→row crossing; `DiagramLayout._as_graph` reads `attributes.to_dicts()` for nodes and `adjacency.to_dicts()` for `(source, target)` edges into `rustworkx`.
- [NUMERIC_EGRESS]: `DataFrame.to_numpy()` `Series.to_numpy()` — dense-array exit for diagram coordinates and nanoplot vectors.
- [INTERCHANGE]: `pl.from_dataframe(obj, allow_copy=)` `pl.from_arrow(tbl)` `pl.from_dicts(rows)` `DataFrame.__arrow_c_stream__()` `DataFrame.__dataframe__()` — the `data/tabular` wire; a frame from the C# `Rasm.Bim` graph crosses zero-copy as Arrow, never a Python row roundtrip.
- [RENDER_CONTEXT]: `pl.Config(...)` `pl.StringCache()` — deterministic display/precision and a shared categorical string-cache scope.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DataFrame.style` is a real `great_tables.gt.GT`, so the publication-table plane never marshals a frame into a foreign table builder; every member this overlay touches is the thin set crossing that one boundary, and a lazy scan, streaming sink, plugin kernel, or deep-namespace transform is the `data` engine's concern past it.

[STACKING]:
- `great-tables`(`.api/great-tables.md`): the BYO-DataFrame contract is polars-first — `GT(frame)` and `frame.style` take a `pl.DataFrame`, `summary_rows`/`grand_summary_rows` take `dict[str, pl.Expr]`, `loc.body(mask=)` takes a boolean `Expr`, `vals.fmt_*` takes a `pl.Series`; the styled frame folds the `TableOp` sequence and emits HTML/LaTeX/PDF with no intermediate interchange.
- `expression`(`.api/expression.md`): `TablePlan.render`/`DiagramLayout.assign` thread every seam crossing through `RuntimeRail`/`boundary`, mapping the `pl.exceptions` family (`ColumnNotFoundError`, `SchemaError`, `ComputeError`) onto a typed `Result`/`Error` rather than raising.
- `msgspec`(`.api/msgspec.md`): a `pl.DataFrame` rides as a frozen `Struct` field (`TablePlan.frame`, `DiagramLayout.adjacency`) — an opaque settled payload whose content key (`ContentIdentity.of`) and rendered bytes alone ride the `ArtifactReceipt`.
- `beartype`(`.api/beartype.md`): an artifacts owner's ingress validates `pl.DataFrame`/`pl.Series`/`pl.Expr` as the declared annotation, faulting a wrong-shape input at the boundary.
- `numpy`(`.api/numpy.md`): `to_numpy()`/`Series.to_numpy()` exits to the dense-array substrate for diagram coordinates and nanoplot vectors.
- within-lib: `TablePlan` folds the `TableOp` sequence onto `frame.style` and indexes `VALS_TABLE[kind](series, opts)` for out-of-table Series formatting; `DiagramLayout` lowers `adjacency`/`attributes` through `to_dicts()` into a `rustworkx` graph.

[LOCAL_ADMISSION]:
- Accept a polars frame only as a settled input over the `data/tabular` wire (`from_dataframe`/`from_arrow` from a `data` producer or the C# `Rasm.Bim` QTO/schedule egress); an artifacts page never `read_*`/`scan_*`s a source.
- Cross frame→rows (`to_dicts`) only at the `DiagramLayout` graph boundary and frame→array (`to_numpy`) only at the numeric egress; everything between stays columnar `pl.Expr` composition addressed by `polars.selectors`.

[RAIL_LAW]:
- Package: `polars` (artifacts overlay)
- Owns: the `DataFrame.style → GT` construction seam, the `pl.Expr` summary/predicate language for great-tables, the standalone `pl.Series → vals.fmt_*` edge, the `pl.DataFrame` adjacency/attribute frame and its `to_dicts` graph lowering, and the Arrow-C-stream / interchange ingress that is the `data/tabular` wire
- Accept: a settled `pl.DataFrame`/`pl.Series`/`pl.Expr` over the interchange wire from a `data` producer; `polars.selectors` column sets; a `pl.Config`/`pl.StringCache` render context
- Reject: source IO inside artifacts (`read_*`/`scan_*`/`sink_*`), the lazy engine, optimizer flags, streaming/GPU collect, and `register_plugin_function` (all owned by `data/.api/polars.md`); a Python loop or per-cell callable where a `pl.Expr` expresses the logic; re-authoring a QTO/schedule frame the C# `Rasm.Bim` graph already owns
