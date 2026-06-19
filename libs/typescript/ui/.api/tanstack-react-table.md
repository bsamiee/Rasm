# [API_CATALOGUE] @tanstack/react-table

`@tanstack/react-table` supplies `useReactTable` and `flexRender` over `@tanstack/table-core`, binding the headless table engine to React. The package re-exports the full `table-core` surface: `Table`, `Row`, `Column`, `Cell`, `Header`, `HeaderGroup`, `ColumnDef`, `TableOptions`, `TableState`, `ColumnHelper`, `createColumnHelper`, and all feature row-model factories.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tanstack/react-table`
- package: `@tanstack/react-table`
- module: `@tanstack/react-table`
- asset: `build/lib/index.d.ts` — re-exports `@tanstack/table-core`
- rail: data-table

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: react-specific types
- rail: data-table

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                               |
| :-----: | :------------------- | :------------ | :----------------------------------- |
|  [01]   | `Renderable<TProps>` | type alias    | `ReactNode \| ComponentType<TProps>` |

[PUBLIC_TYPE_SCOPE]: core table model
- rail: data-table

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                |
| :-----: | :---------------------------- | :------------ | :------------------------------------ |
|  [01]   | `Table<TData>`                | interface     | top-level table instance              |
|  [02]   | `Row<TData>`                  | interface     | data row with all feature mixins      |
|  [03]   | `Column<TData, TValue>`       | interface     | column with all feature mixins        |
|  [04]   | `Cell<TData, TValue>`         | interface     | cell value + context                  |
|  [05]   | `Header<TData, TValue>`       | interface     | header with sizing mixin              |
|  [06]   | `HeaderGroup<TData>`          | interface     | row of headers                        |
|  [07]   | `RowModel<TData>`             | interface     | `{ rows, flatRows, rowsById }`        |
|  [08]   | `TableState`                  | interface     | merged state from all features        |
|  [09]   | `InitialTableState`           | interface     | partial initial state shape           |
|  [10]   | `TableOptions<TData>`         | interface     | public options (partial keys allowed) |
|  [11]   | `TableOptionsResolved<TData>` | interface     | fully-resolved options                |
|  [12]   | `TableFeature<TData>`         | interface     | custom feature extension contract     |

[PUBLIC_TYPE_SCOPE]: column definition family
- rail: data-table

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------ | :------------ | :------------------------------------------ |
|  [01]   | `ColumnDef<TData, TValue>`            | union         | display \| group \| accessor column def     |
|  [02]   | `AccessorFnColumnDef<TData, TValue>`  | type          | function accessor column                    |
|  [03]   | `AccessorKeyColumnDef<TData, TValue>` | type          | key-path accessor column                    |
|  [04]   | `DisplayColumnDef<TData, TValue>`     | type          | display-only column                         |
|  [05]   | `GroupColumnDef<TData, TValue>`       | type          | nested column group                         |
|  [06]   | `IdentifiedColumnDef<TData, TValue>`  | interface     | base with optional `id`/`header`            |
|  [07]   | `ColumnHelper<TData>`                 | type          | `{ accessor, display, group }` factory type |

[PUBLIC_TYPE_SCOPE]: cell and header contexts
- rail: data-table

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [RAIL]                                                |
| :-----: | :----------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `CellContext<TData, TValue>`   | interface     | `{ cell, column, getValue, renderValue, row, table }` |
|  [02]   | `HeaderContext<TData, TValue>` | interface     | header render context                                 |
|  [03]   | `AccessorFn<TData, TValue>`    | type alias    | `(row, index) => TValue`                              |
|  [04]   | `Updater<T>`                   | type alias    | `T \| ((old: T) => T)`                                |
|  [05]   | `OnChangeFn<T>`                | type alias    | `(updaterOrValue: Updater<T>) => void`                |
|  [06]   | `RowData`                      | type alias    | `unknown \| object \| any[]`                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: React hooks and renderers
- rail: data-table

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :-------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `useReactTable<TData>(options)`   | table hook     | returns `Table<TData>` with React state integration |
|  [02]   | `flexRender<TProps>(Comp, props)` | render helper  | `ReactNode \| ReactJSX.Element`                     |

[ENTRYPOINT_SCOPE]: column factory and table constructor
- rail: data-table

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY]    | [RAIL]                  |
| :-----: | :---------------------------- | :---------------- | :---------------------- |
|  [01]   | `createColumnHelper<TData>()` | column factory    | `ColumnHelper<TData>`   |
|  [02]   | `createTable<TData>(options)` | low-level factory | headless `Table<TData>` |

[ENTRYPOINT_SCOPE]: row model factories (all row-model builders require passing as table option)
- rail: data-table

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :------------------------- | :------------- | :------------------------------------ |
|  [01]   | `getCoreRowModel()`        | core model     | required base row model               |
|  [02]   | `getFilteredRowModel()`    | filter model   | column-filtered rows                  |
|  [03]   | `getGroupedRowModel()`     | group model    | grouped/aggregated rows               |
|  [04]   | `getExpandedRowModel()`    | expand model   | expanded sub-rows                     |
|  [05]   | `getSortedRowModel()`      | sort model     | sorted rows                           |
|  [06]   | `getPaginationRowModel()`  | page model     | paginated rows                        |
|  [07]   | `getFacetedRowModel()`     | facet model    | faceting for filter UI                |
|  [08]   | `getFacetedUniqueValues()` | facet values   | `() => Map<TValue, number>`           |
|  [09]   | `getFacetedMinMaxValues()` | facet range    | `() => [TValue, TValue] \| undefined` |

[ENTRYPOINT_SCOPE]: Table instance operations
- rail: data-table

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :----------------------- | :------------- | :-------------------------------------- |
|  [01]   | `getAllColumns()`        | column query   | all columns in nested hierarchy         |
|  [02]   | `getAllFlatColumns()`    | column query   | all columns flattened                   |
|  [03]   | `getAllLeafColumns()`    | column query   | leaf columns only                       |
|  [04]   | `getColumn(columnId)`    | column lookup  | `Column<TData, unknown> \| undefined`   |
|  [05]   | `getCoreRowModel()`      | row model      | base row model                          |
|  [06]   | `getRowModel()`          | row model      | final processed row model               |
|  [07]   | `getRow(id, searchAll?)` | row lookup     | `Row<TData>`                            |
|  [08]   | `getState()`             | state query    | current `TableState`                    |
|  [09]   | `setState(updater)`      | state mutation | applies `Updater<TableState>`           |
|  [10]   | `setOptions(newOptions)` | options update | applies `Updater<TableOptionsResolved>` |
|  [11]   | `reset()`                | state reset    | resets to initial state                 |
|  [12]   | `getHeaderGroups()`      | header query   | header rows for column headers          |
|  [13]   | `getFooterGroups()`      | header query   | footer rows                             |
|  [14]   | `getFlatHeaders()`       | header query   | all headers flattened                   |
|  [15]   | `getLeafHeaders()`       | header query   | leaf-level headers                      |

## [04]-[IMPLEMENTATION_LAW]

[TABLE_TOPOLOGY]:
- `useReactTable` wraps `createTable` and forces a React re-render on every `onStateChange` call
- `getCoreRowModel` is required; all other row-model factories are opt-in via their corresponding option keys (`getFilteredRowModel`, `getSortedRowModel`, etc.)
- Column defs distinguish three variants: `AccessorFnColumnDef` (function accessor), `AccessorKeyColumnDef` (key-path string), and `DisplayColumnDef` (no data accessor, id or header required)
- `ColumnHelper` factories — `.accessor(accessor, colDef)`, `.display(colDef)`, `.group(colDef)` — produce the correct discriminated `ColumnDef` union member with inferred types
- `flexRender(Comp, props)` handles both `ReactNode` (string/element) and `ComponentType<P>` — pass cell/header render functions directly here
- `TableFeature` allows injecting `createCell`/`createColumn`/`createRow`/`createTable`/`getDefaultColumnDef`/`getDefaultOptions`/`getInitialState` callbacks via `_features` option

[LOCAL_ADMISSION]:
- State is managed externally: consumers lift `TableState` into React state via `state` + `onStateChange`; `useReactTable` merges external state over internal defaults.
- `initialState` seeds one-time initial values; `state` overrides continuously.
- Row IDs default to row index path (`"0"`, `"0.1"`, …); pass `getRowId` for server-keyed row identity.

[RAIL_LAW]:
- package: `@tanstack/react-table` (core: `@tanstack/table-core`)
- Owns: headless data-table model with sorting, filtering, grouping, pagination, pinning, selection, sizing, and faceting
- Accept: `useReactTable` + `flexRender` for all table UI; `createColumnHelper` for typed column definitions
- Reject: custom table state machines that duplicate feature row-model behavior; wrapper components that hide table instance access
