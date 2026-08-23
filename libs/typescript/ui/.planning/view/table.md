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

```typescript signature
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
  // the effect `Record` namespace shadows the global utility type across this whole module, so every record shape
  // here spells `Record.ReadonlyRecord` — a bare `Record<K, V>` resolves to a non-generic namespace and fails
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
  // the transient drag slice seeds at rest: `isResizingColumn` is `false | string`, so the no-drag value is the
  // literal `false` and never a null, an empty string, or an absent column id a handler would then resolve
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
  // the table's slice types are MUTABLE arrays and records, so each row decodes mutable: a readonly parcel forces a
  // defensive copy at every rehydrate, and that copy is exactly where a restored layout silently forks from the fold
  columnOrder: Schema.mutable(Schema.Array(Schema.String)),
  columnSizing: Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.Number })),
  columnVisibility: Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.Boolean })),
  // pinning is logical, so a persisted layout replays under either writing direction and the sticky
  // offset rides a CSS logical inset the reader's direction resolves
  columnPinning: Schema.Struct({
    start: Schema.optionalWith(Schema.mutable(Schema.Array(Schema.String)), { default: () => [] }),
    end: Schema.optionalWith(Schema.mutable(Schema.Array(Schema.String)), { default: () => [] }),
  }),
  // corners, never a normalized min/max rectangle: the anchor stays put while the focus corner moves, so a replayed
  // band resumes a shift-extend exactly where it stopped and an exclusion keeps its place in the operation order
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
    // a generic computed key in an object literal widens to a string index and loses the slice's shape, so the
    // package construction carries the single-key record the checker cannot type from the literal
    ({ ...state, ...Record.singleton(key, functionalUpdate(updater, state[key])) })

const _edge = <K extends keyof Grid.Slice>(
  registry: Registry.Registry,
  fold: Atom.Writable<Grid.Slice, Grid.Slice>,
  key: K,
): StoreAtom<Grid.Slice[K]> => {
  const read = (): Grid.Slice[K] => registry.get(fold)[key]
  return {
    get: read,
    // the table hands either a bare value or its own updater, and `Updater` is the union of both, so one parameter
    // satisfies both arms of the store atom's intersected setter without a second overload
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

```typescript signature
import { Feed } from "@rasm/ts/core"
import type { ColumnDef } from "@tanstack/react-table"
import { createColumnHelper } from "@tanstack/react-table"
import { Array, Match, Option, Order } from "effect"

declare namespace Grid {
  // declare the registry vocabulary here rather than reading it off the features object: that object holds the meta
  // slots, so keying the roster off it closes a type cycle through ColumnMeta
  type SortKey = "alphanumeric" | "basic" | "datetime" | "rank" | "text"
  type FilterKey = "equals" | "inDateRange" | "inNumberRange" | "includesString"
  // the closed presentation roster — a new presentation is one member here plus its renderer at the consuming
  // surface, so a free-spelled render string never reaches a column ("token" is review's change-column arm)
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

// each row carries the cell's BEHAVIOR, never a bare projection name — `render` keys the roster's presentation,
// `align` and `measured` decide the column's layout and whether Format.number folds its SI magnitude, `editable`
// states which kinds a TableMeta commit port may write, and `sort`/`filter` name registry keys the features object
// registers by construction, so a comparator is never spelled at a column
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

// `getRowId` resolves before any table instance exists, so the band names its own anchor column: the composing view
// hands this straight to the option and row selection keys on the brand from the first render, never the row index
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
          // a category repeats down the band, so equal adjacent values merge into one anchored run; the run is
          // recomputed from rendered rows alone, so sorting and filtering only change which rows are adjacent
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

```typescript signature
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

// the comparator sees NUMBERS and nothing else: a consumer orders its own vocabulary by making the rank its column's
// value, so the registry never learns a domain word. `resolveDataValue` normalizes before the comparison, so a column
// with no rank to give sorts last in ascending order instead of throwing the whole model into NaN comparisons
const _rank = constructSortFn({
  sort: (left: number, right: number) => Order.number(left, right),
  resolveDataValue: (value) => (Predicate.isNumber(value) ? value : Number.MAX_SAFE_INTEGER),
})

// stitched once outside every component: the object is the type parameter each column helper, column
// def, and table instance narrows through, so a remount can never hand the grid a different vocabulary
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
  // satisfies closes the loop the cell roster opens: a key the roster names and this slot drops fails here
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
  // the alias homes at its value: the roster and the type every helper, def, and instance narrows through are one
  // declaration apart, so neither can be widened without the other
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

```typescript signature
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

// the feature answers four physical box sides, each computed from the DISPLAY INDEX of the neighbouring cell, so the
// lowest index is inline-start under either writing direction; folding them here is what keeps a physical side out of
// every ring, outline, and sticky inset the grid then paints
const _EDGE = {
  top: "blockStart",
  right: "inlineEnd",
  bottom: "blockEnd",
  left: "inlineStart",
} as const satisfies Record.ReadonlyRecord<keyof CellSelectionEdges, Grid.Edge>

const _edges = (edges: CellSelectionEdges): ReadonlyArray<Grid.Edge> =>
  Array.filterMap(Record.toEntries(_EDGE), ([side, edge]) => (edges[side] ? Option.some(edge) : Option.none()))

// the counts answer the LOGICAL grid — the derived row model plus the one header row, and the visible leaf columns —
// so a reader hears what the current filter admits, never the pre-filter source and never the mounted window
const _counts = <TData>(table: Table<Grid.Features, TData>): Grid.Counts => ({
  rows: table.getRowModel().rows.length + 1,
  columns: table.getVisibleLeafColumns().length,
})

// indexes are 1-based over the logical grid with the header row at 1, so a windowed row still announces its true
// position; the caller passes the row's model index, not its position in the mounted span
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
  // a placeholder header in a merged chain reports rowSpan 0, which HTML reads as span-to-the-end of the header
  // group, so `covered` carries the same skip signal the seat does and the placeholder never reaches the DOM
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

// the marked column's OWN accessor answers identity, which is what lets one data-free marker serve a band row and a
// wire row at once; the decode is the gate, so a value the brand refuses keeps the row out of the selection
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

```typescript signature
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

// the `use` prefix is load-bearing: the rules-of-hooks check reads the NAME, so a `_window` spelling would leave every
// call site of this hook unchecked while it still holds the virtualizer's subscription
const _useWindow = <TData>(shape: Grid.Window<TData>): ReactVirtualizer<HTMLElement, HTMLElement> =>
  useVirtualizer({
    count: shape.rows.length,
    getScrollElement: shape.scroll,
    // an index the model no longer carries keys as itself, so a shrinking model re-keys nothing that survived it
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
    // an anchor the current filter drops reveals nothing, rather than scrolling to whatever now sits at its index
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

// --- [EXPORTS] --------------------------------------------------------------------------

export { Grid }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
