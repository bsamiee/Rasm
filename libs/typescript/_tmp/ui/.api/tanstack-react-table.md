# [API_CATALOGUE] @tanstack/react-table

`@tanstack/react-table` v8 binds the headless `@tanstack/table-core` engine to React through `useReactTable` (which wraps `createTable` and forces a re-render on every `onStateChange`) and `flexRender` (which renders a cell/header def that is either a `ReactNode` or a `ComponentType`). It re-exports the full core: the `Table`/`Row`/`Column`/`Cell`/`Header`/`HeaderGroup` instance graph, the `ColumnDef` discriminated union, `createColumnHelper`, and every feature — sorting, filtering, grouping, expanding, pagination, row/column pinning, selection, sizing, visibility, ordering, and faceting — each opted in by passing its row-model factory. In `ui` this is the model half of the one binding-studio health table: it folds the decoded `BindingStatusWire`/`CoercedValueWire`/`WriteReceiptWire` rows into a column model whose accessors are total `effect` `Match`/`Option` folds, virtualized through `@tanstack/react-virtual` (`render/dashboard.md#LIVE_BINDING_DASHBOARD`). Note the pin is v8 (implicit feature registration via row-model factories), NOT v9's explicit `columnFilteringFeature` imports.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tanstack/react-table`
- package / version: `@tanstack/react-table` @ `8.21.3`
- license: `MIT`
- module: dual ESM `build/lib/index.mjs` + CJS `build/lib/index.js`; `types` `build/lib/index.d.ts`; single `.` export
- peer: `react` `>=16.8`, `react-dom` `>=16.8`
- dependency: `@tanstack/table-core` @ `8.21.3` (exact pin) — the framework-agnostic engine, re-exported verbatim; every non-React symbol below is a core re-export
- rail: data-table

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: react-specific + core instance graph
- rail: data-table

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [NOTE]                                                                        |
| :-----: | :---------------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `Renderable<TProps>`          | type alias    | `ReactNode \| ComponentType<TProps>` — the union `flexRender` discriminates    |
|  [02]   | `Table<TData>`                | interface     | the top-level instance; carries every enabled feature's methods (`getState`, `setOptions`, the row-model + feature getters) |
|  [03]   | `Row<TData>`                  | interface     | one data row; `getValue`, `getAllCells`, `getVisibleCells`, `subRows`, plus the row-feature mixins (selection/expanding/pinning/grouping) |
|  [04]   | `Column<TData, TValue>`       | interface     | one column; `columnDef`, `getFlatColumns`, plus the column-feature mixins (sorting/filtering/sizing/pinning/visibility/grouping/faceting/ordering) |
|  [05]   | `Cell<TData, TValue>`         | interface     | `getValue`, `renderValue`, `getContext`, plus `getIsGrouped`/`getIsAggregated`/`getIsPlaceholder` grouping state |
|  [06]   | `Header<TData, TValue>` / `HeaderGroup<TData>` | interface | a header (with the sizing mixin `getSize`/`getStart`/`getResizeHandler` and `getContext`/`getLeafHeaders`) and a row of them |
|  [07]   | `RowModel<TData>`             | interface     | `{ rows, flatRows, rowsById }` — the output of every row-model factory          |
|  [08]   | `TableFeature<TData>`         | interface     | custom-feature contract: `createCell`/`createColumn`/`createHeader`/`createRow`/`createTable`/`getDefaultColumnDef`/`getDefaultOptions`/`getInitialState`, injected via the `_features` option |

[PUBLIC_TYPE_SCOPE]: column definition family
- rail: data-table

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [NOTE]                                                                        |
| :-----: | :------------------------------------ | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `ColumnDef<TData, TValue>`            | union         | `AccessorFnColumnDef \| AccessorKeyColumnDef \| DisplayColumnDef \| GroupColumnDef` |
|  [02]   | `AccessorFnColumnDef` / `AccessorKeyColumnDef` / `DisplayColumnDef` / `GroupColumnDef` | variant | function accessor / key-path accessor / display-only (id or header required) / nested group |
|  [03]   | `ColumnHelper<TData>`                 | factory type  | `{ accessor, display, group }` — produces the correct discriminated `ColumnDef` member with inferred `TValue` |
|  [04]   | `CellContext<TData, TValue>` / `HeaderContext<TData, TValue>` | interface | `{ cell, column, getValue, renderValue, row, table }` passed to a cell/header render fn |
|  [05]   | `AccessorFn<TData, TValue>`           | type alias    | `(row: TData, index: number) => TValue`                                        |
|  [06]   | `Updater<T>` / `OnChangeFn<T>`        | type alias    | `T \| ((old: T) => T)` and `(updater: Updater<T>) => void` — the state-lift contract every `on*Change` uses |
|  [07]   | `RowData`                             | type alias    | `unknown \| object \| any[]` — the `TData` constraint                          |

[PUBLIC_TYPE_SCOPE]: options and feature state
- rail: data-table

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [NOTE]                                                                        |
| :-----: | :-------------------------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `TableOptions<TData>` / `TableOptionsResolved<TData>` | interface | the public options (partial keys) and the fully-resolved shape `setOptions` receives |
|  [02]   | `TableState` / `InitialTableState`      | interface     | the merged state from all enabled features / the partial one-time seed         |
|  [03]   | `SortingState` / `ColumnFiltersState`   | state | `ColumnSort[]` (`{ id, desc }`) / `ColumnFilter[]` (`{ id, value }`); the global-filter slice is the loosely-typed `TableState.globalFilter` |
|  [04]   | `RowSelectionState` / `ExpandedState` / `GroupingState` | state | `Record<string, boolean>` / `true \| Record<string, boolean>` / `string[]`     |
|  [05]   | `ColumnPinningState` / `RowPinningState` / `ColumnOrderState` | state | `{ left?, right? }` position records / `string[]` order                        |
|  [06]   | `ColumnSizingState` / `ColumnSizingInfoState` / `VisibilityState` | state | `Record<id, number>` widths / the active-resize info / `Record<id, boolean>`   |
|  [07]   | `PaginationState`                       | state         | `{ pageIndex: number; pageSize: number }`                                      |
|  [08]   | `SortDirection` / `SortingFn` / `FilterFn` / `AggregationFn` | fn/union | `'asc' \| 'desc'`; the pluggable sort/filter/aggregation strategies (`sortingFns`/`filterFns`/`aggregationFns` built-in tables) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: React hooks, renderer, and factories
- rail: data-table

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [NOTE]                                                                        |
| :-----: | :-------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `useReactTable<TData>(options)`   | table hook     | wraps `createTable`; merges external `state` over internal defaults and re-renders on `onStateChange` |
|  [02]   | `flexRender<TProps>(Comp, props)` | render helper  | renders a `ReactNode` or `ComponentType` cell/header def — pass `cell.column.columnDef.cell` + `cell.getContext()` |
|  [03]   | `createColumnHelper<TData>()`     | column factory | `{ accessor(accessor, colDef), display(colDef), group(colDef) }` — typed `ColumnDef` builders |
|  [04]   | `createTable<TData>(options)`     | low-level      | the headless `Table<TData>` constructor `useReactTable` wraps                   |

[ENTRYPOINT_SCOPE]: row-model factories (feature opt-in — pass as the matching option)
- rail: data-table

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [NOTE]                                                                        |
| :-----: | :------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `getCoreRowModel()`        | required       | the base row model; every other factory is opt-in via its option key           |
|  [02]   | `getFilteredRowModel()` / `getSortedRowModel()` / `getPaginationRowModel()` | pipeline | column+global filter → sort → paginate; passing the factory enables the feature |
|  [03]   | `getGroupedRowModel()` / `getExpandedRowModel()` | tree     | grouping/aggregation and sub-row expansion                                     |
|  [04]   | `getFacetedRowModel()` / `getFacetedUniqueValues()` / `getFacetedMinMaxValues()` | facet | the faceting models a filter UI reads: `() => Map<TValue, number>` and `() => [min, max] \| undefined` |

[ENTRYPOINT_SCOPE]: `Table` instance — state, query, and per-feature setters
- rail: data-table

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [NOTE]                                                             |
| :-----: | :-------------------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `getState()` / `setState(updater)` / `setOptions(updater)` / `reset()` | state    | read/replace the merged `TableState`, re-apply resolved options, reset to initial |
|  [02]   | `getRowModel()` / `getCoreRowModel()` / `getSortedRowModel()` / `getFilteredRowModel()` / `getPaginationRowModel()` / `getExpandedRowModel()` / `getGroupedRowModel()` | row model | the final processed model and each stage's pre/post model |
|  [03]   | `getAllColumns()` / `getAllFlatColumns()` / `getAllLeafColumns()` / `getVisibleFlatColumns()` / `getVisibleLeafColumns()` / `getColumn(id)` | column query | the column hierarchy, its flattenings, the visible subset, and a keyed lookup |
|  [04]   | `getHeaderGroups()` / `getFooterGroups()` / `getFlatHeaders()` / `getLeafHeaders()` / `getLeft…`/`getCenter…`/`getRight…HeaderGroups()` | header query | header/footer rows and the pinned-section header groups |
|  [05]   | `setSorting` / `setColumnFilters` / `setGlobalFilter` / `setGrouping` / `setExpanded` / `setRowSelection` / `setColumnPinning` / `setRowPinning` / `setColumnSizing` / `setColumnVisibility` / `setColumnOrder` / `setPagination` | feature setter | one `Updater`-taking setter per feature slice; each lifts through its `on*Change` |
|  [06]   | `getSelectedRowModel()` / `getFilteredSelectedRowModel()` / `toggleAllRowsSelected()` / `getIsAllRowsSelected()` / `getToggleAllRowsSelectedHandler()` | selection | the selected models and the all-rows selection surface |
|  [07]   | `getCanNextPage()` / `getCanPreviousPage()` / `nextPage()` / `previousPage()` / `firstPage()` / `lastPage()` / `setPageIndex()` / `setPageSize()` / `getPageCount()` / `getRowCount()` | pagination | the full page-navigation surface off `PaginationState` |
|  [08]   | `toggleAllRowsExpanded()` / `getIsAllRowsExpanded()` / `getToggleAllColumnsVisibilityHandler()` / `resetSorting()` / `resetColumnFilters()` / `resetRowSelection()` / `resetExpanded()` / `reset*()` | bulk / reset | all-toggle handlers and the per-feature reset methods |

[ENTRYPOINT_SCOPE]: `Column` / `Header` instance — per-feature operations
- rail: data-table

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [NOTE]                                                             |
| :-----: | :------------------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `getCanSort()` / `getIsSorted()` / `toggleSorting(desc?, multi?)` / `getToggleSortingHandler()` / `getSortIndex()` / `getNextSortingOrder()` / `clearSorting()` | sorting | the header-click sort surface; `getToggleSortingHandler` wires directly to `onClick` |
|  [02]   | `getFilterValue()` / `setFilterValue(updater)` / `getCanFilter()` / `getIsFiltered()` / `getFacetedUniqueValues()` / `getFacetedMinMaxValues()` | filtering | per-column filter value + the faceted values a filter widget renders |
|  [03]   | `getCanPin()` / `getIsPinned()` / `getPinnedIndex()` / `pin('left'\|'right'\|false)` | pinning | column pin state and mutation                                     |
|  [04]   | `getSize()` / `getStart()` / `getCanResize()` / `getIsResizing()` / `resetSize()` / `getResizeHandler()` (Column + Header) | sizing | the width/offset getters and the drag-resize handler bound to a pointer/mouse-down |
|  [05]   | `getIsVisible()` / `getCanHide()` / `toggleVisibility(value?)` / `getToggleVisibilityHandler()` | visibility | per-column show/hide surface                                      |
|  [06]   | `getCanGroup()` / `getIsGrouped()` / `getGroupedIndex()` / `toggleGrouping()` / `getToggleGroupingHandler()` / `getIndex()` / `getAggregationFn()` | grouping/order | grouping toggles, the column's ordered index, and its aggregation strategy |

[ENTRYPOINT_SCOPE]: `Row` / `Cell` instance — per-feature operations
- rail: data-table

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [NOTE]                                                             |
| :-----: | :------------------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `getIsSelected()` / `getIsSomeSelected()` / `getCanSelect()` / `toggleSelected(value?)` / `getToggleSelectedHandler()` | selection | the row-level selection surface; `getToggleSelectedHandler` wires to a checkbox `onChange` |
|  [02]   | `getIsExpanded()` / `getCanExpand()` / `toggleExpanded(value?)` / `getToggleExpandedHandler()` / `getIsAllParentsExpanded()` | expanding | the expand/collapse surface for a tree/detail row |
|  [03]   | `getIsGrouped()` / `getGroupingValue(columnId)` (Row) / `getIsGrouped()` / `getIsAggregated()` / `getIsPlaceholder()` (Cell) | grouping | row grouping state and the cell's grouped/aggregated/placeholder render discriminant |
|  [04]   | `getVisibleCells()` / `getLeftVisibleCells()` / `getCenterVisibleCells()` / `getRightVisibleCells()` (Row) | visibility/pin | the rendered cell list, split by pinned section                    |
|  [05]   | `pin('top'\|'bottom'\|false)` / `getCanPin()` / `getIsPinned()` / `getPinnedIndex()` (Row) | pinning | row pin state and mutation                                        |

## [04]-[IMPLEMENTATION_LAW]

[TABLE_TOPOLOGY]:
- state is external: consumers lift a `TableState` slice into React state via `state` + `onStateChange` (or a per-feature `state.sorting` + `onSortingChange`); `useReactTable` merges external state over internal defaults, so a controlled slice overrides continuously while `initialState` seeds once
- `getCoreRowModel` is required; every other feature is enabled by passing its row-model factory, and its instance methods (`getIsSorted`, `getFilterValue`, …) exist only when the feature is registered — v8 registers implicitly by factory presence, unlike v9's explicit feature imports
- column defs discriminate three data variants (`AccessorFn`, `AccessorKey`, `Display`) plus `Group`; `createColumnHelper().accessor(...)` produces the correct member with inferred `TValue`
- `getToggle*Handler()` methods return event handlers to bind directly (`onClick`/`onChange`) — the sanctioned interaction wiring, never a hand-rolled state mutation
- `TableFeature` + `_features` injects custom `createCell`/`createColumn`/`createRow`/`createTable` callbacks to extend the instance graph

[STACKING]:
- virtualization (the health-table rail): the row model is the virtualizer's `count` — `useVirtualizer({ count: table.getRowModel().rows.length, … })`, and only `getVirtualItems()`-indexed rows call `row.getVisibleCells()` (`render/dashboard.md#LIVE_BINDING_DASHBOARD`, `tanstack-react-virtual.md`); the table owns the model, the virtualizer owns the window
- effect-tier decode → column model: row data is decoded ONCE at the interchange seam via `effect` `Schema.Class`/`Schema.Literal` (the C# wire vocabularies verbatim; `../.api/effect.md` `Schema`), and a `createColumnHelper<Row>().accessor((row) => …, { id, header })` cell folds a `kind`-discriminated union under `Match.discriminatorsExhaustive("kind")` / `Match.value(...).pipe(Match.exhaustive)` with `Option.match` for absent joins — total and compile-checked; the table reads the decoded interior and never re-decodes
- vocabulary source: a status/health column keys off a closed `Schema.Literal` union and an `as const satisfies Record<Key, …>` lookup (`HEALTH_OF`), not a re-minted enum beside the C# `BindingStatusWire`; a new field is one `ColumnDef` row, a new state a new literal breaking the fold at compile time (`render/dashboard.md`)
- a11y grid semantics: the render leaf carries `role="grid"`/`role="row"`/`role="gridcell"` and projects the table's own state to ARIA (`getIsSorted` → `aria-sort`, `getIsSelected` → `aria-selected`); richer grid interaction (roving focus, selection announcement) composes the `react-aria`/`react-stately` a11y tier the folder owns (`react-aria.md`, `react-stately.md`), not a re-implemented keyboard-grid controller
- cell styling: `flexRender` output styles through the one `cn = twMerge(cx(...))` recipe, keying `cva` rows off cell/row `data-*` state (`data-selected`, `data-health`) the feature methods drive (`class-variance-authority.md`, `tailwind-merge.md`)

[LOCAL_ADMISSION]:
- lift the exact `TableState` slices the surface uses (`sorting`, `rowSelection`, `pagination`) into `Atom` cells via `binding/atom.md`, not scattered `useState`; `initialState` seeds one-time defaults
- enable only the row-model factories a surface uses; an unused feature adds no methods and no cost
- give every `Row` a stable `getRowId` when a server key exists — the default index path (`"0"`, `"0.1"`) breaks selection/expansion identity across re-sorts
- bind interactions through the `getToggle*Handler()` methods; never mutate `setSorting`/`setRowSelection` by hand where a handler exists

[RAIL_LAW]:
- package: `@tanstack/react-table` (core: `@tanstack/table-core` @ `8.21.3`)
- owns: the headless data-table model — sorting, filtering, global filtering, grouping/aggregation, pagination, row/column pinning, selection, sizing/resizing, visibility, ordering, expanding, and faceting
- accept: `useReactTable` + `flexRender` for all table UI, `createColumnHelper` for typed column defs, the row-model factories for feature opt-in, the `getToggle*Handler` interaction wiring, `_features`/`TableFeature` for a genuinely new feature
- reject: a custom table state machine duplicating feature row-model behavior; a wrapper component hiding the `Table` instance; a re-minted status/enum vocabulary beside the decoded wire; v9's `columnFilteringFeature`-style explicit imports (this pin is v8)
