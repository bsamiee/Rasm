# polars

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| DataFrame substrate | `polars.DataFrame`, `polars.LazyFrame` | `.planning/visualization/table.md`, `.planning/visualization/diagram/layout.md` |
| Expression columns | `polars.Expr`, `pl.col`, `pl.struct` | `.planning/visualization/table.md` |
| Row and grouped transforms | `DataFrame.select`, `DataFrame.with_columns`, `DataFrame.group_by` | `.planning/visualization/table.md` |

## Integration Rules

- `visualization/table` owns tabular artifact projection; data packages own durable columnar storage and quality profiles.
- `visualization/diagram/layout` may consume adjacency/attribute frames as graph input but emits local `LayoutMap`, not data-frame state.
- Polars frames stay provider substrate at the boundary; artifact receipts carry scalar evidence and content keys.
