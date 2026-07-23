# [TS_UI_API_TANSTACK_REACT_TABLE]

`@tanstack/react-table` is the headless data-grid engine the `view/compose` collection rows compose: it owns table *state and derivation* — column model, sorting, filtering, grouping, expansion, pagination, pinning, faceting, selection — and renders *nothing*. The React package is a thin adapter (`useReactTable` + `flexRender`) over the framework-agnostic `@tanstack/table-core`; it returns a `Table<T>` instance of plain data plus derivation functions, so `ui` supplies the markup through the react-aria grid spine and styles it with the tailwind/cva token vocabulary. Feature cost is opt-in and tree-shakeable: a table pulls exactly the row-model builders it uses (`getCoreRowModel` is the only required one), and the sorting/filtering/aggregation function *registries* are named-table extension points, never a fixed switch. Table state is fully controllable through `state` + `on*Change` `Updater` callbacks, which is the precise seam the `@effect-atom` one-binding law drives — the table state fold lives in an atom, `onStateChange` writes it, `state` reads it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tanstack/react-table`
- package: `@tanstack/react-table` (MIT)
- module format: dual ESM+CJS (`build/lib/index.esm.js` + `index.js`), `sideEffects: false`; first-party bundled `.d.ts` (`build/lib/index.d.ts`) re-exporting `table-core`
- runtime target: React render tree; the core is DOM-free and framework-agnostic, the adapter binds core derivation into a React `useState`/`useReducer` refresh
- peer: `react catalog`, `react-dom catalog` — satisfied by the folder React 19 spine
- asset: pure-TypeScript library, fully declaration-shaped; `useReactTable`/`flexRender`/`createColumnHelper` verified present via `assay api query --key @tanstack/react-table`
- rail: view composition plane — the headless collection-derivation half of the `view/compose` table/virtual rows

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the column model — one discriminated `ColumnDef` union
- rail: view

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY]              |
| :-----: | :--------------------------------------------------------------------- | :------------------------- |
|  [01]   | `ColumnDef<TData, TValue>`                                             | column def union           |
|  [02]   | `AccessorKeyColumnDef` / `AccessorFnColumnDef` / `IdentifiedColumnDef` | value column               |
|  [03]   | `DisplayColumnDef` / `GroupColumnDef`                                  | structural column          |
|  [04]   | `ColumnHelper<TData>` / `CellContext` / `HeaderContext`                | typed builder / render ctx |
|  [05]   | `ColumnMeta<TData, TValue>` / `TableMeta<TData>` (augmentable)         | typed metadata             |

- [01]-[COLUMN_DEF_UNION]: the one column-definition type; the accessor/display/group variants below are its members, discriminated by which keys are present.
- [02]-[VALUE_COLUMN]: a column reading a row key or accessor fn; `createColumnHelper` picks the variant from the accessor shape.
- [03]-[STRUCTURAL_COLUMN]: a value-less display column (selection checkbox, row actions) and a header-grouping parent column.
- [04]-[TYPED_BUILDER_CTX]: `createColumnHelper<Schema.Type>()` yields fully-typed columns; the contexts carry `{ table, column, row, cell, getValue }` into a `flexRender` cell/header.
- [05]-[TYPED_METADATA]: declaration-merged interfaces carrying app-typed column/table metadata (alignment, `GlobalId` accessor, edit policy) end-to-end.

[PUBLIC_TYPE_SCOPE]: table instance, rows, and the derived row model
- rail: view

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY]   |
| :-----: | :----------------------------------------------------------------- | :-------------- |
|  [01]   | `Table<TData>`                                                     | table instance  |
|  [02]   | `Row<TData>` / `Cell<TData, TValue>`                               | row / cell      |
|  [03]   | `Header<TData, TValue>` / `HeaderGroup<TData>` / `Column<TData>`   | header / column |
|  [04]   | `RowModel<TData>` / `TableOptions<TData>` / `TableOptionsResolved` | model / options |

- [01]-[TABLE_INSTANCE]: the returned engine; every derivation (`getRowModel`, `getHeaderGroups`, `getState`, `set*`) hangs off it.
- [02]-[ROW_CELL]: `row.getVisibleCells()`, `row.getIsSelected()`, `row.subRows`; the unit react-virtual windows.
- [03]-[HEADER_COLUMN]: `header.getContext()` feeds `flexRender`; `column.getToggleSortingHandler()`/`getCanSort()` drive the header controls.
- [04]-[MODEL_OPTIONS]: the `{ rows, flatRows, rowsById }` derivation output and the options bag `useReactTable` consumes.

[PUBLIC_TYPE_SCOPE]: controlled state slices and their update rail
- rail: view

| [INDEX] | [SYMBOL]                                                                                            | [TYPE_FAMILY]       |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------------------ |
|  [01]   | `SortingState` / `ColumnFiltersState` / `GlobalFilterTableState` (`globalFilter`) / `GroupingState` | filter/sort state   |
|  [02]   | `PaginationState` / `ExpandedState` / `RowSelectionState` / `VisibilityState`                       | nav/selection state |
|  [03]   | `ColumnOrderState` / `ColumnPinningState` / `ColumnSizingState` / `RowPinningState`                 | layout state        |
|  [04]   | `Updater<T>` / `OnChangeFn<T>`                                                                      | update rail         |
|  [05]   | `FilterFns` / `SortingFns` / `AggregationFns` (augmentable) / `FilterMeta`                          | fn registry types   |

- [01]-[FILTER_SORT_STATE]: the per-feature state slices lifted into `@effect-atom` folds when the row order/filter must survive remount.
- [02]-[NAV_SELECTION_STATE]: `RowSelectionState` keyed by `GlobalId` bridges the table selection to `viewer/mark/selection`.
- [03]-[LAYOUT_STATE]: column reorder/pin/resize and sticky-row state, all controllable and atom-persistable.
- [04]-[UPDATE_RAIL]: every `on*Change` is `(updater: T \| ((old: T) => T)) => void`; `functionalUpdate` resolves it, `makeStateUpdater` builds the atom writer.
- [05]-[FN_REGISTRY_TYPES]: declaration-merged registries naming custom fuzzy/rank/domain functions referenced by string id in a column def.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing and rendering a table
- rail: view

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------- |
|  [01]   | `useReactTable<T>({ data, columns, getCoreRowModel, state, onStateChange, ... })`                 | build table    |
|  [02]   | `createColumnHelper<Schema.Type>()` → `.accessor(key\|fn, def)` / `.display(def)` / `.group(def)` | typed columns  |
|  [03]   | `flexRender(cellOrHeader.column.columnDef.cell, cell.getContext())`                               | render slot    |
|  [04]   | `table.getHeaderGroups()` / `table.getRowModel().rows` / `table.getFooterGroups()`                | derive markup  |

- [01]-[BUILD_TABLE]: the one entry hook; returns the `Table<T>` instance rebuilt on state/data change.
- [02]-[TYPED_COLUMNS]: columns typed against the decoded `wire` Schema type; the accessor variant is inferred from the argument.
- [03]-[RENDER_SLOT]: resolves a column's cell/header (component, fn, or primitive) against its context; the only bridge from headless def to react-aria markup.
- [04]-[DERIVE_MARKUP]: the header/body/footer derivation; `getRowModel().rows` is the array react-virtual windows.

[ENTRYPOINT_SCOPE]: opt-in feature composition — tree-shakeable row models
- rail: view

| [INDEX] | [SURFACE]                                                                                                 | [ENTRY_FAMILY]     |
| :-----: | :-------------------------------------------------------------------------------------------------------- | :----------------- |
|  [01]   | `getCoreRowModel()` (required) — `getFilteredRowModel()` / `getSortedRowModel()` / `getGroupedRowModel()` | row-model opt-in   |
|  [02]   | `getExpandedRowModel()` / `getPaginationRowModel()` / `getFacetedRowModel()`                              | row-model opt-in   |
|  [03]   | `getFacetedUniqueValues()` / `getFacetedMinMaxValues()`                                                   | facet derivation   |
|  [04]   | `_features: [RowSelection, ColumnPinning, RowExpanding, ...]`                                             | custom feature set |

- [01]-[CORE_MODELS]: each imported builder enables its feature; unused builders tree-shake out, so a plain table ships no sort/filter code.
- [02]-[EXTRA_MODELS]: sub-row expansion, client pagination, and the faceted (distinct-value) pipeline.
- [03]-[FACET_DERIVATION]: the distinct-value map and numeric range feeding a filter UI's option list / slider bounds.
- [04]-[CUSTOM_FEATURE_SET]: the feature modules composed explicitly when a table needs a bespoke subset or a custom feature beside the built-ins — the roster is `ColumnFiltering`/`RowSorting`/`GlobalFiltering`/`ColumnGrouping`/`ColumnOrdering`/`ColumnSizing`/`ColumnVisibility`/`GlobalFaceting`/`RowPagination`/`RowPinning`.

[ENTRYPOINT_SCOPE]: the pluggable function registries
- rail: view

| [INDEX] | [SURFACE]                                                                                                    | [ENTRY_FAMILY]  |
| :-----: | :----------------------------------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `sortingFns` (`alphanumeric`/`text`/`datetime`/`basic`/...) — `column.sortingFn: 'alphanumeric' \| MySortFn` | sort registry   |
|  [02]   | `filterFns` (`includesString`/`equals`/`inNumberRange`/`arrIncludes`/...) — `column.filterFn`                | filter registry |
|  [03]   | `aggregationFns` (`sum`/`mean`/`min`/`max`/`count`/`extent`/`unique`/...) — `column.aggregationFn`           | agg registry    |
|  [04]   | `functionalUpdate(updater, old)` / `makeStateUpdater(key, instance)` / `memo(deps, fn, opts)`                | state util      |

- [01]-[SORT_REGISTRY]: reference a built-in by id or supply a custom `SortingFn`; the registry is the parameterization, not a per-column branch.
- [02]-[FILTER_REGISTRY]: built-in or custom `FilterFn`; a fuzzy-rank fn registered here powers the command-palette-style column filter.
- [03]-[AGG_REGISTRY]: grouped-row aggregation by id or custom `AggregationFn`; the roster is seed data feeding one lookup, extended by declaration-merging `AggregationFns`.
- [04]-[STATE_UTIL]: resolve an `Updater` against prior state, build a keyed state-setter for the atom binding, and memoize a derivation.

## [04]-[IMPLEMENTATION_LAW]

[TABLE_TOPOLOGY]:
- Headless by construction: the package returns a `Table<T>` instance — plain data + derivation functions — and never touches the DOM. `ui` owns all markup through the react-aria grid spine (`grid`/`row`/`columnheader`/`gridcell` roles + keyboard nav), and `flexRender` is the single bridge that resolves a column's `cell`/`header` (a component, a render fn, or a primitive) against its typed context. There is no styled component to override; the token vocabulary lives entirely in `ui`.
- Core + adapter split: every type, model, feature, and function registry is `@tanstack/table-core`; `@tanstack/react-table` adds only `useReactTable` (wire the core `createTable` into a React refresh), `flexRender`, and the React-typed `createColumnHelper`. The same split governs `@tanstack/react-virtual` over `virtual-core` — one headless core, one thin framework adapter.
- Features are opt-in row-model builders, not a monolith: `getCoreRowModel` is mandatory; every other model (`getSortedRowModel`, `getFilteredRowModel`, `getGroupedRowModel`, `getExpandedRowModel`, `getPaginationRowModel`, `getFacetedRowModel`) is imported only when used and tree-shakes out otherwise. A bespoke table composes the raw feature modules through `_features`. This is one parameterized composition surface, never a table subclass per capability.
- State is controlled through one `Updater` rail: pass `state` + `onStateChange` (or per-feature `state.sorting` + `onSortingChange`, …) and every mutation arrives as `Updater<T> = T | ((old: T) => T)`. `functionalUpdate` resolves it and `makeStateUpdater` builds the setter — the exact shape an `@effect-atom` writer consumes, so the table state fold is one atom and the table is a pure function of it.
- Function registries are named tables, not switches: `sortingFns`/`filterFns`/`aggregationFns` are string-keyed rosters a column references by id; a custom function is registered by declaration-merging `SortingFns`/`FilterFns`/`AggregationFns` and referenced the same way. New sort/filter/aggregation behavior is a registry row, never a new code path in the table.

[STACKS_WITH]:
- `@tanstack/react-virtual` (`.api/tanstack-react-virtual.md`): the sibling half of the `view/compose` collection rows — `useVirtualizer({ count: table.getRowModel().rows.length, ... })` windows the derived rows so a 100k-row table renders only the visible span; the two share the headless-core + React-adapter architecture and compose into one virtualized data grid.
- `react-aria` / `react-aria-components` (`.api/react-aria-components.md`): supply the grid interaction + ARIA semantics the headless table lacks — `columnheader` sort announcement, `gridcell` focus management, roving tabindex, multi-select keyboard model — so the table is accessible without `ui` hand-rolling the a11y layer.
- `@effect-atom/atom` + `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): the `ONE_FOLD_ONE_BINDING` law binds `Table` state — the sorting/filter/selection/pagination fold is one atom, `on*Change` writes it via `makeStateUpdater`, `state` reads it; server-driven tables bind an `AtomHttpApi` row to feed `data` + manual pagination.
- `effect` `Schema` (`.api/effect.md`): the decoded `wire` row type is the `TData` generic; `createColumnHelper<Schema.Type>()` types every accessor against it, and `RowSelectionState` keyed by the `GlobalId` brand carries the selection to `viewer/mark/selection` without re-deriving identity.
- `class-variance-authority` + `tailwind-merge` (`.api/class-variance-authority.md`, `.api/tailwind-merge.md`): the cell/header/row class dispatch — density, alignment, sort-direction, and selection variants resolve to tailwind classes through `cva`, conflict-merged by `twMerge`, styling the headless output.
- `cmdk` (`.api/cmdk.md`): a fuzzy `filterFn` registered in `filterFns` powers a command-palette-style column filter, sharing the same rank algorithm the command palette uses.

[LOCAL_ADMISSION]:
- Use `useReactTable` + `flexRender` + `createColumnHelper` as the sole table entry; never render table markup from a styled table component — the headless engine plus the react-aria grid spine is the contract.
- Import only the row-model builders a table uses (`getCoreRowModel` always); never barrel-import every feature — the tree-shake is the point.
- Reference built-in sort/filter/aggregation behavior by registry id and register custom functions by declaration-merging the `*Fns` interfaces; never branch per column with an inline comparator where a registry row fits.
- Drive table state through `state` + `on*Change` bound to `@effect-atom`; never let the table own uncontrolled internal state when the fold must survive remount, feed selection to the viewer, or round-trip to the server.
- Type columns against the decoded `wire` Schema type via `createColumnHelper<T>()`; never pass `any` rows or re-declare the row shape the `wire` `#vocab` already owns.

[RAIL_LAW]:
- Package: `@tanstack/react-table` (over `@tanstack/table-core`)
- Owns: the `ColumnDef` model, the `Table`/`Row`/`Cell`/`Header`/`Column` instance surface, the tree-shakeable feature row-models (sort/filter/group/expand/paginate/facet/select/pin/order/size/visibility), the controlled-state `Updater` rail, and the `sortingFns`/`filterFns`/`aggregationFns` registries
- Accept: headless usage with `ui`-owned react-aria markup, opt-in row-model imports, `createColumnHelper` typed against the `wire` Schema, `@effect-atom`-bound controlled state through `makeStateUpdater`, registry-id function references with declaration-merged custom fns, react-virtual windowing over `getRowModel().rows`
- Reject: a styled table component owning markup or a11y, barrel-importing unused features, inline per-column comparators where a registry row fits, uncontrolled state where a fold must persist or bridge, `any`-typed rows, a re-declared row shape duplicating the `wire` `#vocab`
