# [UI_TABLE]

The one data-grid owner: TanStack Table models rows, headers, facets, grouping, aggregation, and pinning, TanStack Virtual windows them, react-aria supplies the grid semantics, and ONE atom holds the whole `TableState`. Columns type against the wire-decoded row Schema through `createColumnHelper`, or fold from a `Feed.Document` column band so a producer-opaque artifact renders with zero static row Schema. RAC `Table` keeps the collections faceting, grouping, and virtual scale never earn. Module: `ui/src/view/table.ts`.

## [01]-[INDEX]

- [02]-[STATE_FOLD]: the one-atom `TableState` slice, the `Updater` application, persistence; `Grid`.
- [03]-[COLUMN_PLANE]: static helper columns and the `Feed.Document` band-driven dynamic column fold; `Grid`.
- [04]-[DERIVE_MODELS]: the row-model roster — core, sort, filter, facet, group, expand, paginate; —.
- [05]-[GRID_SEMANTICS]: aria grid roles, logical-count law, selection identity; —.
- [06]-[WINDOWING]: the virtualizer fold — measurement, pinned-range union, selection echo scroll; `Grid`.

## [02]-[STATE_FOLD]

[STATE_FOLD]:

```typescript
import type {
  ColumnDef,
  ColumnFiltersState,
  ColumnOrderState,
  ColumnPinningState,
  ColumnSizingState,
  ExpandedState,
  GroupingState,
  PaginationState,
  RowPinningState,
  RowSelectionState,
  SortingState,
  Updater,
  VisibilityState,
} from "@tanstack/react-table"
import { createColumnHelper, functionalUpdate } from "@tanstack/react-table"
import { Feed } from "@rasm/ts/core"
import { Array, Match, Option, Order, Record, Schema } from "effect"

declare namespace Grid {
  type Slice = {
    readonly sorting: SortingState
    readonly columnFilters: ColumnFiltersState
    readonly globalFilter: string
    readonly rowSelection: RowSelectionState
    readonly rowPinning: RowPinningState
    readonly grouping: GroupingState
    readonly expanded: ExpandedState
    readonly pagination: PaginationState
    readonly columnOrder: ColumnOrderState
    readonly columnPinning: ColumnPinningState
    readonly columnSizing: ColumnSizingState
    readonly columnVisibility: VisibilityState
  }
  type Banded = Readonly<Record<string, unknown>>
  type Persisted = typeof _Persisted.Type
}

const _SLICE: Grid.Slice = {
  sorting: [],
  columnFilters: [],
  globalFilter: "",
  rowSelection: {},
  rowPinning: { top: [], bottom: [] },
  grouping: [],
  expanded: {},
  pagination: { pageIndex: 0, pageSize: 50 },
  columnOrder: [],
  columnPinning: { left: [], right: [] },
  columnSizing: {},
  columnVisibility: {},
}

const _Persisted = Schema.Struct({
  columnOrder: Schema.Array(Schema.String),
  columnSizing: Schema.Record({ key: Schema.String, value: Schema.Number }),
  columnVisibility: Schema.Record({ key: Schema.String, value: Schema.Boolean }),
  columnPinning: Schema.Struct({
    left: Schema.optionalWith(Schema.Array(Schema.String), { default: () => [] }),
    right: Schema.optionalWith(Schema.Array(Schema.String), { default: () => [] }),
  }),
})

const _apply = <K extends keyof Grid.Slice>(key: K) =>
  (state: Grid.Slice, updater: Updater<Grid.Slice[K]>): Grid.Slice =>
    // a generic computed key in an object literal widens to a string index and loses the slice's shape, so the
    // package construction carries the single-key record the checker cannot type from the literal
    ({ ...state, ...Record.singleton(key, functionalUpdate(updater, state[key])) })
```

## [03]-[COLUMN_PLANE]

[COLUMN_PLANE]:

```typescript
declare module "@tanstack/react-table" {
  interface ColumnMeta<TData, TValue> {
    readonly cell: (typeof _CELL)[Feed.Column["kind"]]
    readonly dimension: Feed.Column["dimension"]
    readonly precision: Feed.Column["precision"]
    readonly role: Feed.Column["role"]
    readonly nullable: boolean
    readonly globalId?: (row: TData) => string
  }
  interface TableMeta<TData> {
    readonly commit?: (row: TData, column: string, value: unknown) => void
  }
}

const _helper = createColumnHelper<Grid.Banded>()

// each row carries the cell's BEHAVIOR, never a bare projection name — `render` keys the roster's presentation,
// `align` and `measured` decide the column's layout and whether Format.number folds its SI magnitude, and
// `editable` states which kinds a TableMeta commit port may write; a name alone forces a second table to read it
const _CELL = {
  bool: { render: "toggle", align: "center", measured: false, editable: true },
  int: { render: "number", align: "end", measured: true, editable: true },
  real: { render: "number", align: "end", measured: true, editable: true },
  text: { render: "text", align: "start", measured: false, editable: true },
  stamp: { render: "instant", align: "start", measured: false, editable: false },
} as const satisfies Record<
  Feed.Column["kind"],
  { readonly render: string; readonly align: "start" | "center" | "end"; readonly measured: boolean; readonly editable: boolean }
>

const _byColumn = Order.combine(
  Order.mapInput(Order.number, (column: Feed.Column) => Option.getOrElse(column.rank, () => Number.MAX_SAFE_INTEGER)),
  Order.mapInput(Order.string, (column: Feed.Column) => column.name),
)

const _banded = (document: Feed.Document): ReadonlyArray<ColumnDef<Grid.Banded, unknown>> =>
  Match.value(document).pipe(
    Match.when({ media: "tabular" }, (ref) => Array.map(Array.sort(ref.columns, _byColumn), (column) =>
      _helper.accessor((row) => row[column.name], {
        id: column.name,
        header: column.name,
        meta: {
          cell: _CELL[column.kind],
          dimension: column.dimension,
          precision: column.precision,
          role: column.role,
          nullable: column.nullable,
          ...(column.role === "key" ? { globalId: (row: Grid.Banded) => String(row[column.name]) } : {}),
        },
      }))),
    Match.when({ media: "text" }, () => []),
    Match.when({ media: "image" }, () => []),
    Match.when({ media: "model" }, () => []),
    Match.when({ media: "binary" }, () => []),
    Match.exhaustive,
  )
```

## [04]-[DERIVE_MODELS]

[DERIVE_MODELS]:

## [05]-[GRID_SEMANTICS]

[GRID_SEMANTICS]:

## [06]-[WINDOWING]

[WINDOWING]:
- Packages: `@tanstack/react-virtual` (`useVirtualizer`, `defaultRangeExtractor`, `Range`, `VirtualItem`).
- Growth: a new sticky class (a pinned footer, a group header) is one index-set argument to the range union — never a second extractor.

```typescript
import { defaultRangeExtractor, type Range } from "@tanstack/react-virtual"
import { Array, Order } from "effect"

const _range = (pinned: ReadonlyArray<number>) =>
  (range: Range): number[] =>
    Array.sort(Array.dedupe([...pinned, ...defaultRangeExtractor(range)]), Order.number)

declare namespace Grid {
  type Shape = {
    readonly Persisted: typeof _Persisted
    readonly seed: Grid.Slice
    readonly apply: typeof _apply
    readonly banded: typeof _banded
    readonly range: typeof _range
  }
}

const Grid: Grid.Shape = {
  Persisted: _Persisted,
  seed: _SLICE,
  apply: _apply,
  banded: _banded,
  range: _range,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Grid }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
