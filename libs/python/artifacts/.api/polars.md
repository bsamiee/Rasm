# [PY_ARTIFACTS_API_POLARS]

`polars` is the Rust-backed columnar dataframe engine; inside `artifacts` it is an **artifact-local overlay, not the engine owner** — the full eager/lazy/`Expr`/plugin engine surface (lazy optimization, `scan_*`/`sink_*` streaming IO, the native expression-plugin host, the complete dtype vocabulary) is owned by `libs/python/data/.api/polars.md` and is NOT restated here. This catalog documents only the **publication-boundary seam** the artifacts visualization plane composes: the `DataFrame.style → great_tables.GT` in-process table-construction edge, the `pl.Expr` summary/predicate language `visualization/table` folds into `summary_rows`/`grand_summary_rows`/`loc.body(mask=...)`, the standalone `pl.Series → great_tables.vals.fmt_*` Series-formatter edge, and the `pl.DataFrame` adjacency/attribute frame `visualization/diagram/layout` lowers into a graph. The frame is always a **settled input arriving over the `data/tabular` wire** (Arrow C-stream / interchange protocol, never a Python row roundtrip); artifacts styles, aggregates, and lowers it but never authors the data, re-opens the lazy engine, or re-implements a transform the `data` owner already owns. Stacks onto the `great-tables` folder catalogue (`.api/great-tables.md`) for the GT seam and the universal substrate rails (`expression` Result/Option around the boundary, `msgspec` Structs that carry a `pl.DataFrame` as a field, `beartype` at the ingress edge, `numpy` for the coordinate/value egress).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `polars`
- package: `polars`
- import: `polars` (alias `pl`); `polars.selectors` (alias `cs`)
- owner: `data` (this is the artifacts-local overlay; the canonical engine catalogue is `libs/python/data/.api/polars.md`)
- rail: tabular boundary (publication-table + diagram-graph input edge)
- license: MIT
- installed: `1.42.0`
- build: abi3 wheel (`cp310-abi3`, covers 3.15 — no cp315 gate); the engine ships as the `polars` Python façade over the `polars-runtime-32` native runtime (the optional `polars-runtime-64` large-index and `GPUEngine` cuDF runtimes are unused by the artifacts overlay)
- entry points: none (library only)
- capability (artifacts overlay scope): the `DataFrame.style` accessor returning a real `great_tables.GT` for in-process publication-table construction; `pl.Expr` as the `great-tables` `summary_rows`/`grand_summary_rows` aggregation language and `loc.body(mask=...)` cell-targeting predicate; standalone `pl.Series → vals.fmt_*` formatting; `pl.DataFrame` carried as a settled `msgspec.Struct` field and lowered to graph rows via `to_dicts()`; Arrow-C-stream / interchange ingress (`from_dataframe`/`from_arrow`/`__arrow_c_stream__`) as the `data/tabular` wire; `to_numpy`/`Series.to_numpy` egress to the coordinate/value plane

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: boundary types the artifacts pages carry
- rail: tabular boundary

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
|  [01]   | `DataFrame` | eager frame | the settled in-memory frame carried as a `msgspec.Struct` field (`TablePlan.frame`, `DiagramLayout.adjacency`/`.attributes`); the `.style` accessor is the GT seam, `.to_dicts()`/`.to_numpy()` the egress |
|  [02]   | `Series` | typed column | the single typed column threaded into the standalone `vals.fmt_*` Series formatters (`TablePlan.Series`) and into `.to_numpy()`/`.to_list()` value egress |
|  [03]   | `Expr` | expression node | the aggregation/predicate value the table owner builds — `pl.col(...).mean().alias(...)` for `summary_rows` `fns`, `pl.col(...) > x` as the `loc.body(mask=...)` cell predicate; an `Expr` until evaluated by great-tables/polars, never a Python lambda over rows |
|  [04]   | `DataType` family | dtype value object | `Int64`/`Float64`/`String`/`Boolean`/`Date`/`Datetime`/`Decimal`/`Categorical`/`Enum`/`Struct`/`List` — the dtype carried on each column; relevant here only for selecting the right `FmtKind` per column and the `cs.numeric()`/`cs.by_dtype(...)` selector sets. The full dtype vocabulary is owned by `data/.api/polars.md` |
|  [05]   | `Config` / `StringCache` | runtime context | `Config` for deterministic display/precision context around a render; `StringCache` to scope a shared categorical string-cache when a categorical-keyed frame is styled or joined at the seam |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the great-tables construction seam (`visualization/table`)
- rail: tabular boundary

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
| :-----: | :-------- | :------------- | :----------- |
|  [01]   | `DataFrame.style -> great_tables.GT` | GT seam | the load-bearing edge: the accessor returns a **real `great_tables.gt.GT`** (verified `type(df.style) is great_tables.gt.GT`), so `TablePlan._build` constructs the styled table in-process from a polars frame with no DataFrame→table marshalling — `df.style` then folds the `TableOp` sequence directly. The default-construction path; the explicit `GT(frame, rowname_col=..., groupname_col=..., locale=..., id=...)` constructor is the fall-through only when those knobs diverge from default |
|  [02]   | `pl.col(name)` / `pl.col(name).mean()/.sum()/.median()/.std()/.quantile()/.min()/.max()/.n_unique()` / `.alias(name)` | summary expr | the `summary_rows(fns=...)` / `grand_summary_rows(fns=...)` aggregation language: each `dict[str, pl.Expr]` value is a polars aggregate whose `.alias`/own name targets its column (great-tables rejects a non-`None` `columns`, so the `Expr` must name its own column). Built once, evaluated by great-tables over the frame, never a Python reduction |
|  [03]   | `pl.col(name) > v` / `.is_between(lo, hi)` / `.is_in(set)` / `.is_null()` / `& \| ~` | mask predicate | the boolean `Expr` passed as `loc.body(mask=...)` (and the masked `loc.grand_summary`) for value-targeted cell styling/text-transform — `TablePlan`'s `Mask = pl.Expr \| None`; mutually exclusive with `columns`/`rows`, so a predicate-targeted `Place` carries the `mask` alone |
|  [04]   | `pl.when(pred).then(x).otherwise(y)` / `pl.lit(v)` / `pl.format(tmpl, *exprs)` / `pl.concat_str(exprs, separator=)` / `pl.coalesce(*exprs)` / `pl.struct(*exprs)` | derived-cell expr | the expression vocabulary for deriving a display column the frame did not carry (a formatted label, a merged string, a first-non-null) inside a `select`/`with_columns` BEFORE the `.style` seam, so the styled table reads a settled column rather than a per-cell Python transform |
|  [05]   | `cs.numeric()` / `cs.by_dtype(dt)` / `cs.matches(re)` / `cs.starts_with(p)` / `cs.exclude(...)` / set algebra `& \| - ~` | column selector | the declarative dtype/name column set passed where a `TableOp` takes `Cols` (e.g. `Fmt(FmtKind.NUMBER, columns=cs.numeric())`) so a format/align/color op addresses a column class without a hardcoded name list |
|  [06]   | `DataFrame.select(*exprs)` / `.with_columns(*exprs)` / `.filter(*pred)` / `.sort(by)` / `.rename(map)` / `.cast(dtypes)` / `.head(n)` / `.slice(off, len)` / `.pivot(...)` / `.unpivot(...)` / `.group_by(keys).agg(*exprs)` / `.top_k(k, by=)` | frame shaping | last-mile shaping of the settled frame at the boundary — re-projecting, re-ordering, casting, or rolling-up the QTO/schedule frame to the exact display shape before `.style`. The transforms are the `data`-owned API; artifacts uses them only to land the display frame, never to author the source |

[ENTRYPOINT_SCOPE]: standalone Series formatting and the diagram-graph + interop egress
- rail: tabular boundary

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
| :-----: | :-------- | :------------- | :----------- |
|  [01]   | `DataFrame.get_column(name)` / `DataFrame[name]` -> `Series`; `Series.to_list()` / `.cast(dt)` / `.round(n)` / `.fill_null(v)` / `.rename(name)` / `.to_frame()` | Series projection | extract the single column great-tables' standalone `vals.fmt_number(series, ...)` / `vals.fmt_*` formatters consume (`TablePlan.Series` indexes `VALS_TABLE[kind](series, opts)`); the `Series` is the out-of-table value source, formatted to `list[str]` for rich-text rendering outside a `GT` chain |
|  [02]   | `DataFrame.to_dicts()` / `.iter_rows(named=)` / `.rows()` | row lowering | the `visualization/diagram/layout` lowering: `DiagramLayout._as_graph` reads `self.attributes.to_dicts()` for node payloads and `self.adjacency.to_dicts()` for `(source, target)` edges, feeding `rustworkx` `add_nodes_from`/`add_edges_from`. A bounded, deliberate frame→row crossing at the graph boundary, never inside a transform |
|  [03]   | `DataFrame.to_numpy(...)` / `Series.to_numpy(...)` | numeric egress | the value/coordinate egress to the numpy substrate (`.api/numpy.md`) when a diagram projection or a nanoplot value vector must leave the frame as a dense array — the zero-copy-where-possible exit to the universal numeric rail |
|  [04]   | `pl.from_dataframe(obj, allow_copy=)` / `pl.from_arrow(tbl)` / `pl.from_dicts(rows)` / `DataFrame.__arrow_c_stream__()` / `DataFrame.__dataframe__()` | interchange ingress/egress | the `data/tabular` wire edge: a QTO/schedule frame arriving from the C# `Rasm.Bim` graph (or any `data` producer) crosses as an Arrow C-stream / interchange-protocol object zero-copy via `from_dataframe`/`from_arrow`; `__arrow_c_stream__`/`to_arrow()` is the matching outbound capsule. Arrow `Table`/`RecordBatch` is the wire, never a Python row roundtrip — the seam that keeps `visualization/table` a consumer of the `data` plane, not a re-author of it |
|  [05]   | `pl.Config(...)` (context manager / `set_*`) · `pl.StringCache()` | render context | deterministic display/precision context and a shared categorical string-cache scope around a styled render so a categorical-keyed table or a precision-sensitive figure renders reproducibly |

## [04]-[IMPLEMENTATION_LAW]

[OVERLAY_SCOPE]:
- This catalogue is the **artifacts boundary view** of polars. The engine itself — `LazyFrame`, the optimizer (`predicate_pushdown`/`projection_pushdown`/`comm_subexpr_elim`/...), `scan_*`/`sink_*` streaming and out-of-core IO, `collect(engine='streaming'|GPUEngine)`, `register_plugin_function` and the native Rust expression-plugin host, the full `Int8..Int128`/`Decimal`/`Struct`/`List`/`Array` dtype vocabulary, the `.str`/`.dt`/`.list`/`.arr`/`.struct`/`.cat`/`.bin` namespace method families, `group_by_dynamic`/`rolling`/`join_asof`/`join_where`/`over`, and the SQL surface — is owned by `libs/python/data/.api/polars.md`. An artifacts design page reaching for a lazy scan, a streaming sink, a custom plugin kernel, or a deep namespace transform is reaching past its boundary: that work belongs upstream in the `data` plane, and the frame arrives here already settled.
- The single correctness invariant of the overlay is the **seam**: `DataFrame.style` is a real `GT`, so the publication-table plane never marshals a frame into a foreign table builder. Everything else artifacts touches (`Expr` summary/predicate, `Series → vals.fmt_*`, `to_dicts()` graph lowering, Arrow-stream ingress) is the thin set of polars members that cross that boundary.

[STACKING]:
- **great-tables (`.api/great-tables.md`):** the BYO-DataFrame contract is polars-first — `GT(frame)` and the `frame.style` accessor accept a `pl.DataFrame` directly, `summary_rows`/`grand_summary_rows` take `dict[str, pl.Expr]` (the polars aggregate names its own column), and `loc.body(mask=<pl.Expr>)` takes a polars boolean predicate. `vals.fmt_*` accept a `pl.Series` and return `list[str]`. The two libraries compose into one rail: a settled polars frame `.style`s into a `GT`, folds the `TableOp` sequence, and emits HTML/LaTeX/PDF — no intermediate interchange (see `visualization/table#TABLE`).
- **expression (`.api/expression.md`):** every seam crossing is wrapped at the boundary so a malformed frame, a schema mismatch, or a render fault returns a typed `Result`/`Error` rather than raising — `TablePlan.render`/`DiagramLayout.assign` thread the polars call through `RuntimeRail`/`boundary`, and the polars `pl.exceptions` family (e.g. `ColumnNotFoundError`, `SchemaError`, `ComputeError`) is the error vocabulary the boundary maps into the rail.
- **msgspec (`.api/msgspec.md`):** a `pl.DataFrame` is carried as a frozen `Struct` field (`TablePlan.frame: pl.DataFrame`, `DiagramLayout.adjacency: pl.DataFrame`) — the frame is an opaque settled payload on the artifact spec, not a serialized structure; only its content key (`ContentIdentity.of`) and its rendered bytes ride the `ArtifactReceipt`.
- **beartype (`.api/beartype.md`):** the ingress edge of an artifacts owner that accepts a frame validates `pl.DataFrame`/`pl.Series`/`pl.Expr` as the declared annotation, so a non-frame or a wrong-shape input fails at the boundary rather than deep inside a render.
- **numpy (`.api/numpy.md`):** `to_numpy()`/`Series.to_numpy()` is the exit to the dense-array substrate for diagram coordinates and nanoplot value vectors; the frame is the structured upstream, numpy the numeric downstream.

[LOCAL_ADMISSION]:
- Accept a polars frame **only as a settled input** over the `data/tabular` wire (`from_dataframe`/`from_arrow` from a `data` producer or the C# `Rasm.Bim` QTO/schedule egress); never `read_*`/`scan_*` a source from inside an artifacts page — source IO is the `data` plane's concern.
- Drive the publication table through the `frame.style → GT` seam; express any aggregate or cell predicate as a `pl.Expr` (`pl.col(...).mean().alias(...)`, `pl.col(...) > v`) and any derived display column as a `select`/`with_columns` expression BEFORE `.style`, never a Python loop over rows or a per-cell callable where an `Expr` composes.
- Address column classes with `polars.selectors` (`cs.numeric()`, `cs.by_dtype(...)`) where a `TableOp` takes `Cols`, not a hardcoded name list.
- Cross frame→rows (`to_dicts()`) only at a deliberate graph/structure boundary (`DiagramLayout._as_graph`); cross frame→array (`to_numpy()`) only at the numeric egress; everything between stays columnar.
- Defer every lazy/streaming/plugin/deep-namespace need to `data/.api/polars.md` and the `data` plane — this overlay owns only the boundary.

[RAIL_LAW]:
- Package: `polars` (artifacts overlay)
- Owns (here): the publication-table construction seam (`DataFrame.style → GT`), the `pl.Expr` summary/predicate language for great-tables, the standalone `pl.Series → vals.fmt_*` edge, the `pl.DataFrame` adjacency/attribute frame and its `to_dicts()` graph lowering, and the Arrow-C-stream / interchange ingress that is the `data/tabular` wire
- Accept: a settled `pl.DataFrame`/`pl.Series`/`pl.Expr` arriving over the interchange wire from a `data` producer; `polars.selectors` column sets; a `pl.Config`/`pl.StringCache` render context
- Reject: source IO from inside artifacts (`read_*`/`scan_*`/`sink_*` belong to `data`); the lazy engine, optimizer flags, streaming/GPU collect, and `register_plugin_function` (owned by `data/.api/polars.md`); a Python loop or per-cell callable where a `pl.Expr` or a `data`-side transform expresses the logic; re-authoring a QTO/schedule frame the C# `Rasm.Bim` graph already owns (artifacts renders the frame, it never re-implements the IFC model)
