# [UI_TABLE]

TanStack Table owns the one data grid — rows, headers, facets, grouping, aggregation, pinning, the spreadsheet cell band, merged runs, and resize interaction — TanStack Virtual windows them, react-aria supplies the interaction behavior, and ONE atom holds the whole `TableState`. Columns type against the wire-decoded row Schema through `createColumnHelper`, or fold from a `Feed.Document` column band so a producer-opaque artifact renders with zero static row Schema. This folder owns the grid markup and its aria roles outright: RAC `Table` keeps the collections faceting, grouping, and virtual scale never earn, because its collection would re-derive what the feature object here already owns. Module: `ui/src/view/table.ts`.

## [01]-[INDEX]

- [02]-[STATE_FOLD]: one-atom `TableState` slice, `Updater` application, the store-edge binding, the replayable parcel; `Grid`.
- [03]-[COLUMN_PLANE]: static helper columns and the `Feed.Document` band-driven fold with its span and identity slots; `Grid`.
- [04]-[DERIVE_MODELS]: one feature object — row models, fn registries, meta slots; `Grid`.
- [05]-[GRID_SEMANTICS]: aria grid roles, logical-count law, roving focus, the range and resize seams, selection identity; `Grid`.
- [06]-[WINDOWING]: virtualizer fold — measurement, pinned-range union, subscription boundary, selection echo scroll; `Grid`.

## [02]-[STATE_FOLD]

[STATE_FOLD]:
- Owner: `Grid.seed`, `Grid.apply`, and `Grid.edge` — the whole `TableState` is ONE `@effect-atom` cell; `apply` resolves an `Updater` against one slice key, and `edge` projects a single slice onto the `@tanstack/store` atom the table's own ownership rail speaks.
- Law: slice ownership resolves in one order — `options.atoms[key]` beats `options.state[key]`, which beats the internal base atom. A slice with an estate owner (persisted layout, viewer-bridged selection, server-driven pagination) binds through `options.atoms` over `Grid.edge`; a grid-local transient slice stays on the table's own base atoms and never enters the fold.
- Law: the effect-atom cell stays the ONE owner and the store atom is its EDGE — the edge holds no copy, reads through the registry at call time, and writes fold back through `apply`. `makeStateUpdater` reads `options.atoms` itself, so a `state` + `on<Slice>Change` pair beside a bound slice is the fork: two writers over one cell, and the callback rail plus the live base-atom twins are what that fork costs.
- Law: `Grid.Persisted` is the replayable parcel — the logical column layout plus the cell band. `columnResizing` is transient by construction and never enters it, so of the two sizing slices only `columnSizing` carries a width across a reload.
- Law: the cell band persists WHOLE because the drag session lives outside the slice as non-reactive instance data; a restored band survives the first data arrival only under `autoResetCellSelection: false`, which defaults true and would otherwise clear the replay on the first row load.
- Growth: a new slice is one `Grid.Slice` field, one seed row, and — where an estate owns it — one `Grid.edge` binding; never a second fold and never a parallel controlled pair.
- Boundary: registry lifecycle, write modality, and the undo fold are `system/atom`'s; this owner holds only the grid's slice vocabulary and its crossing into the table's ownership rail.

```typescript
import type { Atom, Registry } from "@effect-atom/atom-react"
import type {
  CellSelectionState,
  ColumnFiltersState,
  ColumnOrderState,
  ColumnPinningState,
  ColumnSizingState,
  ColumnVisibilityState,
  ExpandedState,
  GroupingState,
  PaginationState,
  RowPinningState,
  RowSelectionState,
  SortingState,
  Updater,
  columnResizingState,
} from "@tanstack/react-table"
import { functionalUpdate } from "@tanstack/react-table"
import type { Atom as StoreAtom, Observer, Subscription } from "@tanstack/store"
import { Predicate, Record, Schema } from "effect"

declare namespace Grid {
  type Slice = {
    readonly sorting: SortingState
    readonly columnFilters: ColumnFiltersState
    readonly globalFilter: string
    readonly rowSelection: RowSelectionState
    readonly cellSelection: CellSelectionState
    readonly rowPinning: RowPinningState
    readonly grouping: GroupingState
    readonly expanded: ExpandedState
    readonly pagination: PaginationState
    readonly columnOrder: ColumnOrderState
    readonly columnPinning: ColumnPinningState
    readonly columnSizing: ColumnSizingState
    readonly columnResizing: columnResizingState
    readonly columnVisibility: ColumnVisibilityState
  }
  type Banded = Record.ReadonlyRecord<string, unknown>
  type Persisted = typeof _Persisted.Type
}

const _SLICE: Grid.Slice = {
  sorting: [],
  columnFilters: [],
  globalFilter: "",
  rowSelection: {},
  cellSelection: [],
  rowPinning: { top: [], bottom: [] },
  grouping: [],
  expanded: {},
  pagination: { pageIndex: 0, pageSize: 50 },
  columnOrder: [],
  columnPinning: { start: [], end: [] },
  columnSizing: {},
  columnResizing: {
    columnSizingStart: [],
    deltaOffset: null,
    deltaPercentage: null,
    isResizingColumn: false,
    startOffset: null,
    startSize: null,
  },
  columnVisibility: {},
}

const _Persisted = Schema.Struct({
  columnOrder: Schema.mutable(Schema.Array(Schema.String)),
  columnSizing: Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.Number })),
  columnVisibility: Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.Boolean })),
  columnPinning: Schema.Struct({
    start: Schema.optionalWith(Schema.mutable(Schema.Array(Schema.String)), { default: () => [] }),
    end: Schema.optionalWith(Schema.mutable(Schema.Array(Schema.String)), { default: () => [] }),
  }),
  cellSelection: Schema.mutable(Schema.Array(Schema.Struct({
    anchorColumnId: Schema.String,
    anchorRowId: Schema.String,
    focusColumnId: Schema.String,
    focusRowId: Schema.String,
    operation: Schema.optionalWith(Schema.Literal("include", "exclude"), { default: () => "include" as const }),
  }))),
})

const _apply = <K extends keyof Grid.Slice>(key: K) =>
  (state: Grid.Slice, updater: Updater<Grid.Slice[K]>): Grid.Slice =>
    ({ ...state, ...Record.singleton(key, functionalUpdate(updater, state[key])) })

const _edge = <K extends keyof Grid.Slice>(
  registry: Registry.Registry,
  fold: Atom.Writable<Grid.Slice, Grid.Slice>,
  key: K,
): StoreAtom<Grid.Slice[K]> => {
  const read = (): Grid.Slice[K] => registry.get(fold)[key]
  return {
    get: read,
    set: (next: Updater<Grid.Slice[K]>) => registry.update(fold, (state) => _apply(key)(state, next)),
    subscribe: (observer: Observer<Grid.Slice[K]> | ((value: Grid.Slice[K]) => void)): Subscription => ({
      unsubscribe: registry.subscribe(fold, () =>
        Predicate.isFunction(observer) ? observer(read()) : observer.next?.(read())),
    }),
  }
}
```

## [03]-[COLUMN_PLANE]

[COLUMN_PLANE]:
- Owner: `Grid.banded` folds a `Feed.Document` band into column defs; `Grid.Cell` is the per-kind behavior roster and `Grid.ColumnMeta` the one metadata record every column on every grid carries.
- Law: identity rides a DATA-FREE marker — `identity` is a boolean and nothing else, so the single registered `columnMeta` type serves a band row and a wire row alike; the identity VALUE resolves at `[05]` through the marked column's own accessor, never through a row-typed callback, which the shared meta would pin to one `TData` and break at every other consumer.
- Law: exactly one column per grid marks identity. A band may carry several `key` columns, so the mark lands on the first in the band's own order; a grid with no marked column has no branded anchor and its rows fall back to the table's own row index.
- Law: `Grid.keyed` and the marker read the same anchor — `getRowId` resolves before any instance exists and must answer from the document alone, while `Grid.anchor` reads the mounted row, so both routes name one column and neither can drift.
- Law: spanning is role-driven — a `category` column merges adjacent equal values into one anchored run, a `key` column never merges because identity is per row, and a `measure` column never merges because one magnitude read across many rows is a false total.
- Growth: a new wire kind is one `_CELL` row; a new column fact is one `Grid.ColumnMeta` field every producer of columns then states.

```typescript
import { Feed } from "@rasm/core"
import type { ColumnDef } from "@tanstack/react-table"
import { createColumnHelper } from "@tanstack/react-table"
import { Array, Match, Option, Order } from "effect"

declare namespace Grid {
  type SortKey = "alphanumeric" | "basic" | "datetime" | "rank" | "text"
  type FilterKey = "equals" | "inDateRange" | "inNumberRange" | "includesString"
  type Render = "instant" | "number" | "text" | "toggle" | "token"
  type Cell = {
    readonly render: Grid.Render
    readonly align: "start" | "center" | "end"
    readonly measured: boolean
    readonly editable: boolean
    readonly sort: Grid.SortKey
    readonly filter: Grid.FilterKey
  }
  type ColumnMeta = {
    readonly cell: Grid.Cell
    readonly dimension: Feed.Column["dimension"]
    readonly precision: Feed.Column["precision"]
    readonly role: Feed.Column["role"]
    readonly nullable: boolean
    readonly identity: boolean
  }
  type TableMeta = {
    readonly commit?: (row: Grid.Banded, column: string, value: unknown) => void
  }
}

const _helper = createColumnHelper<Grid.Features, Grid.Banded>()

const _CELL = {
  bool: { render: "toggle", align: "center", measured: false, editable: true, sort: "basic", filter: "equals" },
  int: { render: "number", align: "end", measured: true, editable: true, sort: "alphanumeric", filter: "inNumberRange" },
  real: { render: "number", align: "end", measured: true, editable: true, sort: "alphanumeric", filter: "inNumberRange" },
  text: { render: "text", align: "start", measured: false, editable: true, sort: "text", filter: "includesString" },
  stamp: { render: "instant", align: "start", measured: false, editable: false, sort: "datetime", filter: "inDateRange" },
} as const satisfies Record.ReadonlyRecord<Feed.Column["kind"], Grid.Cell>

const _byColumn = Order.combine(
  Order.mapInput(Order.number, (column: Feed.Column) => Option.getOrElse(column.rank, () => Number.MAX_SAFE_INTEGER)),
  Order.mapInput(Order.string, (column: Feed.Column) => column.name),
)

const _named = (document: Feed.Document): Option.Option<string> =>
  Match.value(document).pipe(
    Match.when({ media: "tabular" }, (ref) =>
      Option.map(
        Array.findFirst(Array.sort(ref.columns, _byColumn), (column) => column.role === "key"),
        (column) => column.name,
      )),
    Match.orElse(() => Option.none<string>()),
  )

const _keyed = (document: Feed.Document): Option.Option<(row: Grid.Banded) => string> =>
  Option.map(_named(document), (name) => (row: Grid.Banded) => String(row[name]))

const _banded = (document: Feed.Document): ReadonlyArray<ColumnDef<Grid.Features, Grid.Banded, unknown>> =>
  Match.value(document).pipe(
    Match.when({ media: "tabular" }, (ref) => {
      const anchor = _named(document)
      return Array.map(Array.sort(ref.columns, _byColumn), (column) =>
        _helper.accessor((row) => row[column.name], {
          id: column.name,
          header: column.name,
          sortFn: _CELL[column.kind].sort,
          filterFn: _CELL[column.kind].filter,
          spanRows: column.role === "category",
          meta: {
            cell: _CELL[column.kind],
            dimension: column.dimension,
            precision: column.precision,
            role: column.role,
            nullable: column.nullable,
            identity: Option.exists(anchor, (name) => name === column.name),
          },
        }))
    }),
    Match.when({ media: "text" }, () => []),
    Match.when({ media: "image" }, () => []),
    Match.when({ media: "model" }, () => []),
    Match.when({ media: "binary" }, () => []),
    Match.exhaustive,
  )
```

## [04]-[DERIVE_MODELS]

[DERIVE_MODELS]:
- Owner: `Grid.features` is the one registration object every column helper, column def, and table instance types against.
- Law: a capability absent from this object is absent from the state, the API, and the bundle alike, so the roster is the grid's whole vocabulary; each registry key is the exact string a column's `sortFn`, `filterFn`, or `aggregationFn` may name.
- Law: each built-in arrives by its own import, and the per-table `columnMeta`/`tableMeta` slots type the metadata one grid owns. `metaHelper` is phantom — it returns an empty object and `ExtractColumnMeta` hands the registered type back UNAPPLIED to `TData`, which is why the meta type must stay data-free.
- Law: interaction layers over derivation, one feature per layer — `columnResizingFeature` sits beside `columnSizingFeature` and owns only the transient drag slice, while `cellSelectionFeature` and `cellSpanningFeature` carry the spreadsheet band and the merged run. Each demands its prerequisite in the same call, and `ValidateFeatureSlots` names the missing feature rather than failing at the slot.
- Law: the sort registry stays DOMAIN-BLIND — `rank` is the one consumer-ordered entry, and a consumer orders its own closed vocabulary by handing that column the rank its own registry row carries. Domain order crosses as DATA; a key named for a consumer's vocabulary would seat viewer words in the view tier and invert the strata.
- Growth: a new derivation is one feature beside its row-model slot, never a branch at a call site; a new sort or filter behavior is one registry key.

```typescript
import {
  aggregationFn_count,
  aggregationFn_extent,
  aggregationFn_max,
  aggregationFn_mean,
  aggregationFn_min,
  aggregationFn_sum,
  aggregationFn_unique,
  cellSelectionFeature,
  cellSpanningFeature,
  columnFacetingFeature,
  columnFilteringFeature,
  columnGroupingFeature,
  columnOrderingFeature,
  columnPinningFeature,
  columnResizingFeature,
  columnSizingFeature,
  columnVisibilityFeature,
  constructSortFn,
  createExpandedRowModel,
  createFacetedMinMaxValues,
  createFacetedRowModel,
  createFacetedUniqueValues,
  createFilteredRowModel,
  createGroupedRowModel,
  createPaginatedRowModel,
  createSortedRowModel,
  filterFn_equals,
  filterFn_inDateRange,
  filterFn_inNumberRange,
  filterFn_includesString,
  globalFilteringFeature,
  metaHelper,
  rowAggregationFeature,
  rowExpandingFeature,
  rowPaginationFeature,
  rowPinningFeature,
  rowSelectionFeature,
  rowSortingFeature,
  sortFn_alphanumeric,
  sortFn_basic,
  sortFn_datetime,
  sortFn_text,
  tableFeatures,
} from "@tanstack/react-table"
import { Order, Predicate } from "effect"

const _rank = constructSortFn({
  sort: (left: number, right: number) => Order.number(left, right),
  resolveDataValue: (value) => (Predicate.isNumber(value) ? value : Number.MAX_SAFE_INTEGER),
})

const _FEATURES = tableFeatures({
  cellSelectionFeature,
  cellSpanningFeature,
  columnFacetingFeature,
  columnFilteringFeature,
  columnGroupingFeature,
  columnOrderingFeature,
  columnPinningFeature,
  columnResizingFeature,
  columnSizingFeature,
  columnVisibilityFeature,
  globalFilteringFeature,
  rowAggregationFeature,
  rowExpandingFeature,
  rowPaginationFeature,
  rowPinningFeature,
  rowSelectionFeature,
  rowSortingFeature,
  expandedRowModel: createExpandedRowModel(),
  facetedMinMaxValues: createFacetedMinMaxValues(),
  facetedRowModel: createFacetedRowModel(),
  facetedUniqueValues: createFacetedUniqueValues(),
  filteredRowModel: createFilteredRowModel(),
  groupedRowModel: createGroupedRowModel(),
  paginatedRowModel: createPaginatedRowModel(),
  sortedRowModel: createSortedRowModel(),
  aggregationFns: {
    count: aggregationFn_count,
    extent: aggregationFn_extent,
    max: aggregationFn_max,
    mean: aggregationFn_mean,
    min: aggregationFn_min,
    sum: aggregationFn_sum,
    unique: aggregationFn_unique,
  },
  filterFns: {
    equals: filterFn_equals,
    inDateRange: filterFn_inDateRange,
    inNumberRange: filterFn_inNumberRange,
    includesString: filterFn_includesString,
  } satisfies Record.ReadonlyRecord<Grid.FilterKey, unknown>,
  sortFns: {
    alphanumeric: sortFn_alphanumeric,
    basic: sortFn_basic,
    datetime: sortFn_datetime,
    rank: _rank,
    text: sortFn_text,
  } satisfies Record.ReadonlyRecord<Grid.SortKey, unknown>,
  columnMeta: metaHelper<Grid.ColumnMeta>(),
  tableMeta: metaHelper<Grid.TableMeta>(),
})

declare namespace Grid {
  type Features = typeof _FEATURES
}
```

## [05]-[GRID_SEMANTICS]

[GRID_SEMANTICS]:
- Owner: `Grid.seat` and `Grid.perch` — one record per rendered cell and per rendered header carrying every aria attribute, the roving tab index, the span pair, and the logical selection edges, so the markup spreads ONE value and assembles no attribute at a call site.
- Owner: `Grid.counts`, `Grid.marked`, and `Grid.anchor` — the grid's logical size and the branded identity of one row.
- Packages: `@tanstack/react-table` (the `cellSelectionFeature` and `cellSpanningFeature` cell members, `header.getResizeHandler`); `viewer/mark` (`Selection.Id` — the one `GlobalId` brand); `effect`.
- Law: this folder owns the grid markup — `role="grid"` on the scroll host, `role="row"`, `role="columnheader"`, and `role="gridcell"` on its own elements, with react-aria supplying press, focus, and keyboard behavior only. RAC `Table` is refused at this scale because its collection re-derives the faceting, grouping, and windowing `[04]` already owns, and a second derivation of the same rows is a second answer.
- Law: aria counts and indexes are LOGICAL — `aria-rowcount`/`aria-colcount` from `Grid.counts` and `aria-rowindex`/`aria-colindex` from the seat, so a window mounting thirty rows of a hundred thousand still announces each row's true position; a count read off the mounted span is the defect. Spanning changes no count: a merged run is one cell across its rows and the covered positions keep their indexes.
- Law: roving focus is `getTabIndex()`'s `0`/`-1` and nothing else — exactly one cell in the grid is tabbable, arrows move it through `table.moveCellSelection(direction)` and shift-arrows extend through `table.extendCellSelection(direction)`. A focus index kept beside the feature is a second focus authority, and its drift is what the reader hears.
- Law: the range seam is two handlers on the cell — `getSelectionStartHandler(contextDocument?)` binds `mousedown` and installs its OWN document-level `mouseup`, so a popout or iframe host passes its document and no teardown is authored here, while `getSelectionExtendHandler()` binds `mouseenter`. A drag emits one change per cell crossed, so a fine read subscribes `table.atoms.cellSelection` instead of riding the instance holder's projection.
- Law: a covered cell RENDERS NOTHING — `getIsCovered()` is the skip test and a `0` span never reaches the DOM, because HTML reads `rowspan="0"` as span-to-the-end-of-the-section and would merge the run through the whole table body. Runs recompute from the rendered rows and never cross a page boundary, a pinned-section boundary, a change of position in the row tree, or a grouped row; a column that is itself grouped ignores its span slot entirely.
- Law: physical edges fold ONCE — the feature answers `top`/`right`/`bottom`/`left` computed from neighbouring display indexes, `_EDGE` maps them to the logical set, and no focus ring, outline, or sticky inset downstream ever sees a physical side. `table.getCellSelectionMergeBounds()` expands a rectangle over merged cells, so a band is never half a merged run and the outline never cuts one.
- Law: the resize handle binds on this folder's own header element through `header.getResizeHandler(host)`, which installs and cleans its own drag listeners. TRAP: v9 defaults `columnResizeMode` to `'onEnd'` where v8 defaulted to `'onChange'`, so a grid that must paint live widths states the mode at its options; the commit writes `columnSizing` through `makeStateUpdater` under the same atoms precedence every other slice obeys, and the moves arrive rAF-coalesced and batch-wrapped, so no throttle is authored here.
- Law: selection identity is the marked column's own value decoded to the `GlobalId` brand — `RowSelectionState` keys on that brand from a band row and a wire row alike, so the grid's selection IS `viewer/mark`'s set with no second identity fold; an undecodable value answers `None` and the row leaves the set rather than keying it on a raw string.
- Boundary: press, hover, focus-ring, and keyboard primitives are `system/act`'s and `system/primitive`'s; the class dispatch a seat drives is the folder's token plane; the selection SET and its op vocabulary are `viewer/mark#SELECTION_FOLD`'s.

```typescript
import type { Cell, CellSelectionEdges, Header, Row, Table } from "@tanstack/react-table"
import { Selection } from "../viewer/mark.ts"

type GlobalId = Selection.Id

declare namespace Grid {
  type Edge = "blockStart" | "blockEnd" | "inlineStart" | "inlineEnd"
  type Counts = { readonly rows: number; readonly columns: number }
  type Seat = {
    readonly rowIndex: number
    readonly colIndex: number
    readonly rowSpan: number
    readonly colSpan: number
    readonly covered: boolean
    readonly selected: boolean
    readonly focused: boolean
    readonly tabIndex: number
    readonly edges: ReadonlyArray<Grid.Edge>
  }
  type Perch = {
    readonly colIndex: number
    readonly colSpan: number
    readonly rowSpan: number
    readonly covered: boolean
    readonly sort: "ascending" | "descending" | "none"
    readonly size: number
    readonly resizable: boolean
    readonly resizing: boolean
  }
}

const _EDGE = {
  top: "blockStart",
  right: "inlineEnd",
  bottom: "blockEnd",
  left: "inlineStart",
} as const satisfies Record.ReadonlyRecord<keyof CellSelectionEdges, Grid.Edge>

const _edges = (edges: CellSelectionEdges): ReadonlyArray<Grid.Edge> =>
  Array.filterMap(Record.toEntries(_EDGE), ([side, edge]) => (edges[side] ? Option.some(edge) : Option.none()))

const _counts = <TData>(table: Table<Grid.Features, TData>): Grid.Counts => ({
  rows: table.getRowModel().rows.length + 1,
  columns: table.getVisibleLeafColumns().length,
})

const _seat = <TData>(cell: Cell<Grid.Features, TData, unknown>, row: number, column: number): Grid.Seat => ({
  rowIndex: row + 2,
  colIndex: column + 1,
  rowSpan: cell.getRowSpan(),
  colSpan: cell.getColSpan(),
  covered: cell.getIsCovered(),
  selected: cell.getIsSelected(),
  focused: cell.getIsFocused(),
  tabIndex: cell.getTabIndex(),
  edges: _edges(cell.getSelectionEdges()),
})

const _perch = <TData>(header: Header<Grid.Features, TData, unknown>, column: number): Grid.Perch => ({
  colIndex: column + 1,
  colSpan: header.colSpan,
  rowSpan: header.rowSpan,
  covered: header.isPlaceholder,
  sort: Match.value(header.column.getIsSorted()).pipe(
    Match.when("asc", () => "ascending" as const),
    Match.when("desc", () => "descending" as const),
    Match.orElse(() => "none" as const),
  ),
  size: header.getSize(),
  resizable: header.column.getCanResize(),
  resizing: header.column.getIsResizing(),
})

const _decode: (raw: unknown) => Option.Option<GlobalId> = Selection.decode

const _marked = <TData>(table: Table<Grid.Features, TData>): Option.Option<string> =>
  Option.map(
    Array.findFirst(table.getAllLeafColumns(), (column) =>
      Option.match(Option.fromNullable(column.columnDef.meta), {
        onNone: () => false,
        onSome: (meta) => meta.identity,
      })),
    (column) => column.id,
  )

const _anchor = <TData>(
  table: Table<Grid.Features, TData>,
  row: Row<Grid.Features, TData>,
): Option.Option<GlobalId> => Option.flatMap(_marked(table), (column) => _decode(row.getValue(column)))
```

## [06]-[WINDOWING]

[WINDOWING]:
- Owner: `Grid.useWindow` — the one virtualizer fold over the derived row model; `Grid.reveal` scrolls a branded anchor into view.
- Packages: `@tanstack/react-virtual` (`useVirtualizer`, `defaultRangeExtractor`, `measureElement`, `Range`, `ReactVirtualizer`).
- Law: render breadth is a CHOSEN seam in two rungs — `useTable(options, selector)` narrows the instance holder to its projection on `table.state` (shallow-compared), and `table.Subscribe({ source: table.atoms.<slice>, selector })` pushes a narrower boundary down the tree. Omitting the selector subscribes the holder to every registered slice, which under a windowed grid re-renders the whole mounted span on any slice write.
- Law: `table.Subscribe` is THREE OVERLOADS, never one union — source without selector, where children receive the source value; source with selector; and store mode, where the selector is required. The source overloads are declared first so the source infers, and a union of two selector signatures would degrade every callback parameter to implicit `any`.
- Law: a Subscribe boundary sits BESIDE the virtualized window, never around the measurement loop — the virtualizer already holds its own subscription to scroll and size, so a second boundary inside the window double-schedules measurement and the row settles twice per frame.
- Law: `getItemKey` is the row id, so a re-sort MOVES a measured row rather than re-measuring a new key; under `Grid.keyed` that id is the branded anchor, and the measurement cache survives a filter change instead of re-measuring every surviving row.
- Law: a row-spanning cell measures on its ANCHOR row — the run's covered rows render nothing, so the run's whole height belongs to the anchor and the range union keeps that anchor mounted whenever any row of its run is in view.
- Growth: a new sticky class (a pinned footer, a group header) is one index-set argument to the range union — never a second extractor.
- Boundary: the scroll host element and the density estimate are the composing view's; this owner takes both as parameters and mints no layout literal of its own.

```typescript
import type { Range, ReactVirtualizer } from "@tanstack/react-virtual"
import { defaultRangeExtractor, measureElement, useVirtualizer } from "@tanstack/react-virtual"
import { Function } from "effect"

const _range = (pinned: ReadonlyArray<number>) =>
  (range: Range): number[] =>
    Array.sort(Array.dedupe([...pinned, ...defaultRangeExtractor(range)]), Order.number)

declare namespace Grid {
  type Window<TData> = {
    readonly rows: ReadonlyArray<Row<Grid.Features, TData>>
    readonly scroll: () => HTMLElement | null
    readonly pinned: ReadonlyArray<number>
    readonly estimate: (index: number) => number
    readonly overscan: number
  }
}

const _useWindow = <TData>(shape: Grid.Window<TData>): ReactVirtualizer<HTMLElement, HTMLElement> =>
  useVirtualizer({
    count: shape.rows.length,
    getScrollElement: shape.scroll,
    getItemKey: (index) =>
      Option.match(Array.get(shape.rows, index), { onNone: () => index, onSome: (row) => row.id }),
    estimateSize: shape.estimate,
    overscan: shape.overscan,
    rangeExtractor: _range(shape.pinned),
    measureElement,
  })

const _reveal = <TData>(
  virtual: ReactVirtualizer<HTMLElement, HTMLElement>,
  rows: ReadonlyArray<Row<Grid.Features, TData>>,
  anchor: string,
): void =>
  Option.match(Array.findFirstIndex(rows, (row) => row.id === anchor), {
    onNone: Function.constVoid,
    onSome: (index) => virtual.scrollToIndex(index, { align: "center" }),
  })

declare namespace Grid {
  type Shape = {
    readonly Persisted: typeof _Persisted
    readonly features: Grid.Features
    readonly seed: Grid.Slice
    readonly apply: typeof _apply
    readonly edge: typeof _edge
    readonly banded: typeof _banded
    readonly keyed: typeof _keyed
    readonly counts: typeof _counts
    readonly seat: typeof _seat
    readonly perch: typeof _perch
    readonly marked: typeof _marked
    readonly anchor: typeof _anchor
    readonly range: typeof _range
    readonly useWindow: typeof _useWindow
    readonly reveal: typeof _reveal
  }
}

const Grid: Grid.Shape = {
  Persisted: _Persisted,
  features: _FEATURES,
  seed: _SLICE,
  apply: _apply,
  edge: _edge,
  banded: _banded,
  keyed: _keyed,
  counts: _counts,
  seat: _seat,
  perch: _perch,
  marked: _marked,
  anchor: _anchor,
  range: _range,
  useWindow: _useWindow,
  reveal: _reveal,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Grid }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
