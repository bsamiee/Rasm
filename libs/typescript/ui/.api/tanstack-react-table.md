# [TS_UI_API_TANSTACK_REACT_TABLE]

`@tanstack/react-table` owns headless data-grid state and derivation — the `ColumnDef` model and every feature row-model — and renders no markup; `ui` supplies the grid through the react-aria spine.

`useReactTable` + `flexRender` adapt the framework-agnostic `@tanstack/table-core` into React, returning a `Table<T>` of plain data and derivation functions; `state` + `on*Change` `Updater` callbacks control table state — the seam the `@effect-atom` one-binding law drives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tanstack/react-table`
- package: `@tanstack/react-table` (MIT)
- module: dual ESM+CJS, `sideEffects: false`; bundled `.d.ts` re-exporting `@tanstack/table-core`
- runtime: React render tree over a DOM-free, framework-agnostic core; the adapter folds core derivation into a React refresh; peer `react`/`react-dom` via the folder React spine
- rail: view composition plane — the headless collection-derivation half of the `view/compose` table/virtual rows

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the column model — one discriminated `ColumnDef` union

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY]     | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------------------------- | :---------------- | :-------------------------------- |
|  [01]   | `ColumnDef<TData, TValue>`                                             | union             | the one column-def type           |
|  [02]   | `AccessorKeyColumnDef` / `AccessorFnColumnDef` / `IdentifiedColumnDef` | value column      | reads a row key or accessor fn    |
|  [03]   | `DisplayColumnDef` / `GroupColumnDef`                                  | structural column | display column, grouping parent   |
|  [04]   | `ColumnHelper<TData>`                                                  | typed builder     | `createColumnHelper<T>()` columns |
|  [05]   | `CellContext` / `HeaderContext`                                        | render ctx        | feeds `flexRender`                |
|  [06]   | `ColumnMeta<TData, TValue>` / `TableMeta<TData>`                       | typed metadata    | declaration-merged app metadata   |

- `ColumnDef`: accessor, display, and group variants discriminate on which keys the def carries.
- `CellContext`/`HeaderContext`: carry `{ table, column, row, cell, getValue }` into the `flexRender` cell or header.

[PUBLIC_TYPE_SCOPE]: table instance, rows, and the derived row model

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY]   | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------------------- | :-------------- | :----------------------------- |
|  [01]   | `Table<TData>`                                                     | table instance  | the returned derivation engine |
|  [02]   | `Row<TData>` / `Cell<TData, TValue>`                               | row / cell      | one data row and its cells     |
|  [03]   | `Header<TData, TValue>` / `HeaderGroup<TData>` / `Column<TData>`   | header / column | header and column derivation   |
|  [04]   | `RowModel<TData>` / `TableOptions<TData>` / `TableOptionsResolved` | model / options | derived row model, options bag |

- `Table`: `getRowModel`, `getHeaderGroups`, `getState`, and `set*` hang every derivation off the instance.
- `Row`: `row.getVisibleCells()`, `row.getIsSelected()`, and `row.subRows` — react-virtual windows the row set.
- `Header`/`Column`: `header.getContext()` feeds `flexRender`; `column.getToggleSortingHandler()`/`getCanSort()` drive the header controls.
- `RowModel`: derivation output is `{ rows, flatRows, rowsById }`.

[PUBLIC_TYPE_SCOPE]: controlled state slices and their update rail

[STATE_SLICES]: `SortingState` `ColumnFiltersState` `GlobalFilterTableState` `GroupingState` `PaginationState` `ExpandedState` `RowSelectionState` `VisibilityState` `ColumnOrderState` `ColumnPinningState` `ColumnSizingState` `RowPinningState`

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]            | [CAPABILITY]                       |
| :-----: | :-------------------------------------------- | :----------------------- | :--------------------------------- |
|  [01]   | `Updater<T>` / `OnChangeFn<T>`                | update rail              | the `on*Change` callback shape     |
|  [02]   | `FilterFns` / `SortingFns` / `AggregationFns` | fn registry, augmentable | declaration-merged custom fn ids   |
|  [03]   | `FilterMeta`                                  | filter meta              | per-row metadata a `FilterFn` sets |

- Every state slice is controllable and atom-persistable; `RowSelectionState` keyed by the `GlobalId` brand bridges selection to `viewer/mark/selection`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing and rendering a table

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `useReactTable<T>({ data, columns, getCoreRowModel, state, onStateChange })` | hook     | builds the `Table<T>`, rebuilt on change |
|  [02]   | `createColumnHelper<T>() → .accessor(key\|fn) / .display / .group`           | factory  | typed columns; variant inferred          |
|  [03]   | `flexRender(columnDef.cell, cell.getContext())`                              | fn       | resolves a cell/header against its ctx   |
|  [04]   | `table.getHeaderGroups()` / `getRowModel().rows` / `getFooterGroups()`       | accessor | header/body/footer derivation            |

- `flexRender` is the only bridge from a headless `cell`/`header` def to react-aria markup, resolving a component, render fn, or primitive.
- `getRowModel().rows` is the array `@tanstack/react-virtual` windows.

[ENTRYPOINT_SCOPE]: opt-in feature composition — tree-shakeable row models

| [INDEX] | [SURFACE]                                                                    | [SHAPE]           | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------------------------------- | :---------------- | :-------------------------------- |
|  [01]   | `getCoreRowModel()`                                                          | factory, required | the mandatory base row model      |
|  [02]   | `getFilteredRowModel()` / `getSortedRowModel()` / `getGroupedRowModel()`     | factory           | filter, sort, group derivation    |
|  [03]   | `getExpandedRowModel()` / `getPaginationRowModel()` / `getFacetedRowModel()` | factory           | sub-row expand, paginate, facet   |
|  [04]   | `getFacetedUniqueValues()` / `getFacetedMinMaxValues()`                      | factory           | distinct-value map, numeric range |
|  [05]   | `_features: [RowSelection, ColumnPinning, RowExpanding, …]`                  | option            | composes a bespoke feature subset |

- Each imported builder activates its feature and tree-shakes out unused, so a plain table ships no sort/filter code; faceting feeds a filter UI's option list and slider bounds.
- `_features` module roster: `ColumnFiltering` `RowSorting` `GlobalFiltering` `ColumnGrouping` `ColumnOrdering` `ColumnSizing` `ColumnVisibility` `GlobalFaceting` `RowPagination` `RowPinning`.

[ENTRYPOINT_SCOPE]: the pluggable function registries

| [INDEX] | [SURFACE]                                 | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `sortingFns` / `column.sortingFn`         | registry | built-in or custom `SortingFn` by id       |
|  [02]   | `filterFns` / `column.filterFn`           | registry | built-in or custom `FilterFn` by id        |
|  [03]   | `aggregationFns` / `column.aggregationFn` | registry | grouped-row aggregation by id or custom fn |

[STATE_UTIL]: `functionalUpdate(updater, old)` `makeStateUpdater(key, instance)` `memo(deps, fn, opts)`

- Reference a built-in by id or register a custom fn by declaration-merging `SortingFns`/`FilterFns`/`AggregationFns` — new behavior is a registry row, never a per-column branch; a fuzzy-rank `filterFn` powers command-palette-style column filtering.
- `sortingFns` built-ins: `alphanumeric` `text` `datetime` `basic`.
- `filterFns` built-ins: `includesString` `equals` `inNumberRange` `arrIncludes`.
- `aggregationFns` built-ins: `sum` `mean` `min` `max` `count` `extent` `unique`.
- `functionalUpdate` resolves an `Updater`, `makeStateUpdater` builds the keyed atom setter, and `memo` memoizes a derivation.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Headless by construction: the package returns a `Table<T>` instance — plain data and derivation functions — and never touches the DOM. `ui` owns all markup through the react-aria grid spine (`grid`/`row`/`columnheader`/`gridcell` roles and keyboard nav), and `flexRender` is the single bridge resolving a column's `cell`/`header` against its typed context. No styled component exists to override; the token vocabulary lives entirely in `ui`.
- Core and adapter split: every type, model, feature, and function registry is `@tanstack/table-core`; `@tanstack/react-table` adds only `useReactTable` (wire the core into a React refresh), `flexRender`, and the React-typed `createColumnHelper`. `@tanstack/react-virtual` over `virtual-core` shares this split — one headless core, one thin framework adapter.
- Features are opt-in row-model builders, never a monolith: `getCoreRowModel` is mandatory and every other model is imported only when used and tree-shakes out otherwise; a bespoke table composes the raw feature modules through `_features`. This is one parameterized composition surface, never a table subclass per capability.
- State is controlled through one `Updater` rail: `state` + `onStateChange` (or per-feature `state.sorting` + `onSortingChange`) delivers every mutation as `Updater<T> = T | ((old: T) => T)`. `functionalUpdate` resolves it and `makeStateUpdater` builds the setter — the exact shape an `@effect-atom` writer consumes, so the table state fold is one atom and the table is a pure function of it.
- Function registries are named tables, never switches: `sortingFns`/`filterFns`/`aggregationFns` are string-keyed rosters a column references by id, and a custom function registers by declaration-merging `SortingFns`/`FilterFns`/`AggregationFns`. New sort, filter, or aggregation behavior is a registry row, never a new code path.

[STACKING]:
- `@tanstack/react-virtual` (`.api/tanstack-react-virtual.md`): the sibling half of the `view/compose` collection rows — `useVirtualizer({ count: table.getRowModel().rows.length, ... })` windows the derived rows so a 100k-row table renders only the visible span; the two share the headless-core and React-adapter architecture and compose into one virtualized data grid.
- `react-aria` / `react-aria-components` (`.api/react-aria-components.md`): supply the grid interaction and ARIA semantics the headless table lacks — `columnheader` sort announcement, `gridcell` focus management, roving tabindex, multi-select keyboard model — so the table is accessible without `ui` hand-rolling the a11y layer.
- `@effect-atom/atom` + `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): the `ONE_FOLD_ONE_BINDING` law binds `Table` state — the sorting/filter/selection/pagination fold is one atom, `on*Change` writes it via `makeStateUpdater`, `state` reads it; a server-driven table binds an `AtomHttpApi` row to feed `data` and manual pagination.
- `effect` `Schema` (`libs/typescript/.api/effect.md`): the decoded `wire` row type is the `TData` generic; `createColumnHelper<Schema.Type>()` types every accessor against it, and `RowSelectionState` keyed by the `GlobalId` brand carries the selection to `viewer/mark/selection` without re-deriving identity.
- `class-variance-authority` + `tailwind-merge` (`.api/class-variance-authority.md`, `.api/tailwind-merge.md`): the cell/header/row class dispatch — density, alignment, sort-direction, and selection variants resolve to tailwind classes through `cva`, conflict-merged by `twMerge`, styling the headless output.
- `cmdk` (`.api/cmdk.md`): a fuzzy `filterFn` registered in `filterFns` powers a command-palette-style column filter, sharing the rank algorithm the command palette uses.

[LOCAL_ADMISSION]:
- Use `useReactTable` + `flexRender` + `createColumnHelper` as the sole table entry; the headless engine and the react-aria grid spine form the contract, never a styled table component owning markup.
- Import only the row-model builders a table uses (`getCoreRowModel` always); the tree-shake is the point, never a barrel import of every feature.
- Reference built-in sort/filter/aggregation behavior by registry id and register custom functions by declaration-merging the `*Fns` interfaces, never an inline per-column comparator where a registry row fits.
- Drive table state through `state` + `on*Change` bound to `@effect-atom` whenever the fold must survive remount, feed selection to the viewer, or round-trip to the server.
- Type columns against the decoded `wire` Schema type via `createColumnHelper<T>()`; the `wire` `#vocab` owns the row shape, never a re-declared or `any`-typed row.

[RAIL_LAW]:
- Package: `@tanstack/react-table` (over `@tanstack/table-core`)
- Owns: the `ColumnDef` model, the `Table`/`Row`/`Cell`/`Header`/`Column` instance surface, the tree-shakeable feature row-models (sort/filter/group/expand/paginate/facet/select/pin/order/size/visibility), the controlled-state `Updater` rail, and the `sortingFns`/`filterFns`/`aggregationFns` registries
- Accept: headless usage with `ui`-owned react-aria markup, opt-in row-model imports, `createColumnHelper` typed against the `wire` Schema, `@effect-atom`-bound controlled state through `makeStateUpdater`, registry-id function references with declaration-merged custom fns, react-virtual windowing over `getRowModel().rows`
- Reject: a styled table component owning markup or a11y, barrel-importing unused features, inline per-column comparators where a registry row fits, uncontrolled state where a fold must persist or bridge, `any`-typed rows, a re-declared row shape duplicating the `wire` `#vocab`
