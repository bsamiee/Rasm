# [UI_TABLE]

The one data-grid owner: TanStack Table models — rows, headers, facets, grouping, aggregation, pinning — TanStack Virtual windows, react-aria supplies the grid semantics, and ONE atom holds the whole `TableState` so every slice persists, derives, and echoes through the store. Columns type against the wire-decoded row Schema through `createColumnHelper`, or fold dynamically from a `Feed.Document` column band so a producer-opaque tabular artifact renders with zero static row Schema and zero producer branching. RAC `Table` remains the owner for interactive accessible collections WITHOUT heavy derivation — the TanStack fold is earned by faceting/grouping/virtual scale, and one collection never runs both engines. The module is `ui/src/view/table.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                        | [PUBLIC] |
| :-----: | :--------------- | :------------------------------------------------------------------------------ | :------- |
|  [01]   | `STATE_FOLD`     | the one-atom `TableState` slice, the `Updater` application, persistence          | `Grid`   |
|  [02]   | `COLUMN_PLANE`   | static helper columns and the `Feed.Document` band-driven dynamic column fold    | `Grid`   |
|  [03]   | `DERIVE_MODELS`  | the row-model roster — core, sort, filter, facet, group, expand, paginate        | —        |
|  [04]   | `GRID_SEMANTICS` | aria grid roles, logical-count law, selection identity                           | —        |
|  [05]   | `WINDOWING`      | the virtualizer fold — measurement, pinned-range union, selection echo scroll    | `Grid`   |

## [2]-[STATE_FOLD]

[STATE_FOLD]:
- Owner: `Grid` — the state fold: ONE atom holds the whole TanStack `TableState` slice (sorting, filters, selection, grouping, expanded, pagination, sizing, order, pinning, visibility), `useReactTable` reads it as controlled `state` with `onStateChange` writing through `useAtomSet` (the exact `Updater` shape `makeStateUpdater` builds), and `Grid.apply(key)` folds one slice key through `functionalUpdate` so every `on<Slice>Change` handler is one row over one fold — never a per-slice `useState`.
- Packages: `@tanstack/react-table` (`useReactTable`, `functionalUpdate`, `makeStateUpdater`, the state types — `SortingState`, `ColumnFiltersState`, `RowSelectionState`, `GroupingState`, `ExpandedState`, `PaginationState`, `ColumnPinningState`, `ColumnOrderState`, `ColumnSizingState`, `VisibilityState`); `@effect-atom/atom-react` (the one store, `system/atom` law).
- Law: a state slice persists by backing the atom with `Atom.kvs` and its owning schema — column order, sizing, and visibility survive reload as decoded values; a raw `localStorage` read beside the store is the named defect.
- Law: the slice is one product — a second atom holding a parallel copy of any slice, or a component mirroring `sorting` into local state, restates the fold; projections read through `useAtomValue(atom, selector)`.
- Growth: a new managed slice is one field on the slice product plus one `Grid.apply` row — never a sibling state cell.

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
  RowSelectionState,
  SortingState,
  Updater,
  VisibilityState,
} from "@tanstack/react-table"
import { createColumnHelper, functionalUpdate } from "@tanstack/react-table"
import { Feed } from "@rasm/ts/core"
import { Array, Option } from "effect"

declare namespace Grid {
  type Slice = {
    readonly sorting: SortingState
    readonly columnFilters: ColumnFiltersState
    readonly globalFilter: string
    readonly rowSelection: RowSelectionState
    readonly grouping: GroupingState
    readonly expanded: ExpandedState
    readonly pagination: PaginationState
    readonly columnOrder: ColumnOrderState
    readonly columnPinning: ColumnPinningState
    readonly columnSizing: ColumnSizingState
    readonly columnVisibility: VisibilityState
  }
  type Banded = Readonly<Record<string, unknown>>
}

const _SLICE: Grid.Slice = {
  sorting: [],
  columnFilters: [],
  globalFilter: "",
  rowSelection: {},
  grouping: [],
  expanded: {},
  pagination: { pageIndex: 0, pageSize: 50 },
  columnOrder: [],
  columnPinning: { left: [], right: [] },
  columnSizing: {},
  columnVisibility: {},
}

const _apply = <K extends keyof Grid.Slice>(key: K) =>
  (state: Grid.Slice, updater: Updater<Grid.Slice[K]>): Grid.Slice =>
    ({ ...state, [key]: functionalUpdate(updater, state[key]) })
```

## [3]-[COLUMN_PLANE]

[COLUMN_PLANE]:
- Owner: the column fold riding `Grid` — two ingress modalities on one plane: STATIC columns type against the wire-decoded row Schema via `createColumnHelper<Row>()` (accessor rows carrying `sortingFn`/`filterFn`/`aggregationFn` references by registry name); BANDED columns fold from a `Feed.Document` column band — `name`/`kind`/`dimension`/`nullable` rows become dynamic `accessor((row) => row[column.name], { id: column.name })` definitions, so a self-described result artifact renders with the band — never the payload — as the binding contract.
- Law: a `dimension`-carrying column formats through `system/intl` `Format.number` rows as a projection over the SI magnitude; a `stamp` column renders through `Format.instant` + `useDateFormatter`; the band `kind` axis selects the cell projection from one closed row table, and a producer discriminant never appears.
- Law: `flexRender` is the only bridge from column definition to markup — header, cell, footer, and aggregated presentations all pass through it; a cell component reading table internals directly is the named defect.
- Growth: a new column is one `columnHelper` row — or, for banded sources, one band row the fold picks up; a new cell presentation is one `kind` arm on the closed projection table.

```typescript
const _helper = createColumnHelper<Grid.Banded>()

const _CELL = {
  bool: "toggle",
  int: "number",
  real: "number",
  text: "text",
  stamp: "instant",
} as const

const _banded = (document: Feed.Document): ReadonlyArray<ColumnDef<Grid.Banded, unknown>> =>
  Option.match(document.columns, {
    onNone: () => [],
    onSome: (band) =>
      Array.map(band, (column) =>
        _helper.accessor((row) => row[column.name], {
          id: column.name,
          header: column.name,
          meta: { cell: _CELL[column.kind], dimension: column.dimension, nullable: column.nullable },
        })),
  })
```

## [4]-[DERIVE_MODELS]

[DERIVE_MODELS]:
- Law: only the used row models import — `getCoreRowModel` always; `getSortedRowModel`, `getFilteredRowModel`, `getFacetedRowModel` + `getFacetedUniqueValues`, `getGroupedRowModel` + `getExpandedRowModel`, and `getPaginationRowModel` join per surface; an imported-but-unwired model is dead mass, and a hand-derived sort/filter over `rows` restates the engine.
- Law: grouping is model composition — `getGroupedRowModel` groups by the `grouping` slice, `getExpandedRowModel` opens the group rows, and aggregate cells resolve through the `aggregationFns` registry (`sum`/`min`/`max`/`extent`/`mean`/`median`/`unique`/`uniqueCount`/`count`) referenced by name on the column row; a bespoke reduce over group leaves is the named defect.
- Law: TanStack derives, react-aria presents — rows, headers, facets, and aggregates come from the row models; semantics and keyboard come from `[5]`'s grid roles over the rendered markup; the two never swap duties.
- Law: faceted values feed filter affordances — `getFacetedUniqueValues` supplies the option sets a filter overlay renders; the facet read is a model output, never a second pass over data.
- Boundary: filter input debounce is a `system/atom` `Atom.debounce` row on the query atom; collation-correct column sorting lifts `Format.collate` into a named `sortingFns` registry row.

## [5]-[GRID_SEMANTICS]

[GRID_SEMANTICS]:
- Law: semantics and modeling split by owner — react-aria supplies the `grid`/`row`/`columnheader`/`gridcell` roles and roving keyboard over the rendered markup; `aria-rowcount`/`aria-rowindex` stay on the FULL logical count while windowing mounts the visible span, so assistive tech sees the whole collection.
- Law: `RowSelectionState` keys are `GlobalId` strings where the grid fronts model elements — the table's selection slice and the viewer's selection set are ONE atom projected two ways, never two stores reconciled; the selection echo in `[6]` closes the loop.
- Law: header interactions are discrete rows — sort toggles and column menus bind through `system/act` `Gesture.useDiscrete`; column resize drag is the table's own sizing handler bound to the `columnSizing` slice, never a second gesture engine on the header.

## [6]-[WINDOWING]

[WINDOWING]:
- Owner: `Grid.range` — the module's virtualizer member: the pinned-range union `rangeExtractor` the `useVirtualizer` call site consumes; the hook itself binds at the consuming row — `count` from the row model, `getScrollElement` on the scroll container, `estimateSize` + `measureElement` for variable rows, `overscan` as a policy value, `getTotalSize()` sizing the spacer — with `Grid.range(pinned)` unioning pinned/sticky indices over `defaultRangeExtractor` so pinned rows stay mounted outside the visible span.
- Packages: `@tanstack/react-virtual` (`useVirtualizer`, `defaultRangeExtractor`, `Range`, `VirtualItem`).
- Law: a selection echo scrolls through `scrollToIndex(index, { align: "center" })` when the selection atom changes and the selected row sits outside the window — the echo is a subscription consequence, never a render-time side effect.
- Law: window virtualization is this owner for the grid; a RAC collection with native `Virtualizer` support never mounts this hook beside it — one windowing engine per collection.
- Growth: a new sticky class (a pinned footer, a group header) is one index-set argument to the range union — never a second extractor.

```typescript
import { defaultRangeExtractor, type Range } from "@tanstack/react-virtual"
import { Array, Order } from "effect"

const _range = (pinned: ReadonlyArray<number>) =>
  (range: Range): number[] =>
    Array.sort(Array.dedupe([...pinned, ...defaultRangeExtractor(range)]), Order.number)

const Grid: {
  readonly seed: Grid.Slice
  readonly apply: typeof _apply
  readonly banded: typeof _banded
  readonly range: typeof _range
} = {
  seed: _SLICE,
  apply: _apply,
  banded: _banded,
  range: _range,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Grid }
```
