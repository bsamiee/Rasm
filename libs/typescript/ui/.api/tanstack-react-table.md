# [TS_UI_API_TANSTACK_REACT_TABLE]

`@tanstack/react-table` owns headless data-grid state and derivation — the `ColumnDef` model, the statically-registered feature set, and every derived row model — and renders no markup; `ui` supplies the grid through the react-aria spine.

`tableFeatures` stitches features, row-model factories, and function registries into one static object `useTable` instantiates as a `Table` of plain data and derivation functions; every state slice rides a `@tanstack/store` atom, and `options.atoms` hands a slice's ownership to an outside atom of that same vocabulary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tanstack/react-table`
- package: `@tanstack/react-table` (MIT)
- module: ESM-only, `sideEffects: false`; subpaths `.`, `./flex-render`, `./static-functions`, `./experimental-worker-plugin`; re-exports `@tanstack/table-core` whole
- runtime: React render tree over a DOM-free, framework-agnostic core; state rides `@tanstack/react-store` atoms; peer `react >=18`, node `>=20`
- rail: view table plane — the headless collection-derivation half of the `view/table` rows

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the feature registry — one static object stitching every capability a table owns

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [CAPABILITY]                                      |
| :-----: | :--------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `TableFeatures`        | registry shape | features, row-model slots, fn registries, meta    |
|  [02]   | `TableFeature`         | interface      | the lifecycle hooks one feature contributes       |
|  [03]   | `StockFeatures`        | interface      | the whole built-in feature set as one type        |
|  [04]   | `Plugins`              | augmentable    | declaration-merged custom feature keys            |
|  [05]   | `FeatureSlotPrereqs`   | augmentable    | the feature each slot demands beside it           |
|  [06]   | `ValidateFeatureSlots` | type guard     | fails a slot whose prerequisite feature is absent |
|  [07]   | `CoreFeatures`         | interface      | the always-present cell/column/header/row/table   |

- `TableFeatures`: types the `features` value every other generic threads as `TFeatures`; infer it as `typeof features`.
- `FeatureSlotPrereqs`: `sortedRowModel` demands `rowSortingFeature`, `filterFns` demands `columnFilteringFeature`, `columnResizingFeature` demands `columnSizingFeature`.

[PUBLIC_TYPE_SCOPE]: the column model — one discriminated `ColumnDef` union over the feature set

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY]     | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------------- | :---------------- | :------------------------------- |
|  [01]   | `ColumnDef<TFeatures, TData, TValue>`                                  | union             | the one column-def type          |
|  [02]   | `AccessorKeyColumnDef` / `AccessorFnColumnDef` / `IdentifiedColumnDef` | value column      | reads a row key or accessor fn   |
|  [03]   | `DisplayColumnDef` / `GroupColumnDef`                                  | structural column | display column, grouping parent  |
|  [04]   | `ColumnHelper<TFeatures, TData>`                                       | typed builder     | `createColumnHelper` columns     |
|  [05]   | `CellContext` / `HeaderContext`                                        | render ctx        | feeds `flexRender`               |
|  [06]   | `ColumnMeta<TFeatures, TData, TValue>` / `TableMeta<TFeatures, TData>` | typed metadata    | per-table or merged app metadata |
|  [07]   | `ColumnDefTemplate` / `StringOrTemplateHeader`                         | render slot       | the header/cell renderable slot  |

- `ColumnDef`: accessor, display, and group variants discriminate on which keys the def carries.
- `CellContext`/`HeaderContext`: carry `{ table, column, row, cell, getValue }` into the `flexRender` cell or header.
- `ColumnMeta`/`TableMeta`: the `columnMeta`/`tableMeta` registry slots type them per table; the global interfaces apply where no slot is registered.

[PUBLIC_TYPE_SCOPE]: table instance, rows, and the derived row model

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]   | [CAPABILITY]                    |
| :-----: | :-------------------------------------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `Table<TFeatures, TData>` / `ReactTable<TFeatures, TData>`      | table instance  | the returned derivation engine  |
|  [02]   | `Row<TFeatures, TData>` / `Cell<TFeatures, TData, TValue>`      | row / cell      | one data row and its cells      |
|  [03]   | `Header<TFeatures, TData, TValue>` / `HeaderGroup` / `Column`   | header / column | header and column derivation    |
|  [04]   | `RowModel<TFeatures, TData>` / `TableOptions<TFeatures, TData>` | model / options | derived row model, options bag  |
|  [05]   | `CachedRowModels` / `RowModelFns`                               | slot map        | the registered model and fn set |

- `Table`: `getRowModel`, `getHeaderGroups`, `state`, `atoms`, and `set*` hang every derivation off the instance.
- `Row`: `row.getVisibleCells()`, `row.getIsSelected()`, and `row.subRows` — react-virtual windows the row set.
- `Header`/`Column`: `header.getContext()` feeds `flexRender`; `column.getToggleSortingHandler()`/`getCanSort()` drive the header controls.
- `RowModel`: derivation output is `{ rows, flatRows, rowsById }`.
- `ReactTable`: widens the core `Table` with `state`, `Subscribe`, and `FlexRender`.

[PUBLIC_TYPE_SCOPE]: controlled state slices and their atom-backed update rail

[STATE_SLICES]: `SortingState` `ColumnFiltersState` `GroupingState` `PaginationState` `ExpandedState` `RowSelectionState` `CellSelectionState` `ColumnVisibilityState` `ColumnOrderState` `ColumnPinningState` `ColumnSizingState` `columnResizingState` `RowPinningState`

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `Updater<T>` / `OnChangeFn<T>`                 | update rail    | the `on*Change` callback shape     |
|  [02]   | `Atoms` / `BaseAtoms` / `ExternalAtoms`        | atom map       | derived, internal, and owned atoms |
|  [03]   | `TableState<TFeatures>`                        | mapped state   | only the registered slices         |
|  [04]   | `FilterFns` / `SortFns` / `AggregationFns`     | fn registry    | the per-table registered fn ids    |
|  [05]   | `FilterFn` / `SortFn` / `AggregationFnDef`     | fn shape       | one registered function's contract |
|  [06]   | `FilterMeta`                                   | filter meta    | per-row metadata a `FilterFn` sets |
|  [07]   | `ColumnPinningPosition` / `RowPinningPosition` | position union | `start`/`end`, `top`/`bottom`      |

- `ColumnPinningState` is `{ start, end }` and `RowPinningState` is `{ top, bottom }`; column pinning is logical positioning, so CSS logical inset properties own the sticky layout.
- `columnResizingState` exports with a lowercase `c` — the one slice type breaking the roster's PascalCase, and an upstream quirk to spell exactly, not to correct on import.
- `TableState` carries a slice only where its feature is registered — an unregistered slice is absent from `state`, `atoms`, and `initialState` alike.
- Every slice is controllable and atom-persistable; `RowSelectionState` keyed by the `GlobalId` brand bridges selection to `viewer/mark/selection`.
- `ExternalAtoms` maps each slice to a `@tanstack/store` `Atom`, and upstream names it the preferred v9 ownership model for app-managed state: `makeStateUpdater` reads `options.atoms?.[key] ?? baseAtoms[key]` itself, so the table's own setters write the outside atom with no callback in the loop. `Atom` is a structural interface, so a foreign fold is admitted as an adapter atom minted over it, never through a parallel `state` pair.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing a table and typing its columns

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `useTable(TableOptions, (TableState) -> TSelected)`               | hook     | builds the `ReactTable`, selector-narrowed |
|  [02]   | `tableFeatures(TFeatures) -> TFeatures`                           | fn       | stitches the static feature object         |
|  [03]   | `createColumnHelper<TFeatures, TData>()`                          | factory  | typed columns; variant inferred            |
|  [04]   | `helper.accessor(key\|fn, def)` / `.display(def)` / `.group(def)` | instance | one accessor, display, or parent column    |
|  [05]   | `helper.columns([def, def])`                                      | instance | preserves each column's own `TValue`       |
|  [06]   | `tableOptions(options)`                                           | fn       | types a reusable partial option object     |
|  [07]   | `metaHelper<TMeta>()`                                             | fn       | declares a type-only meta slot             |
|  [08]   | `createTableHook(options)`                                        | factory  | app-wide table hook, helper, and contexts  |
|  [09]   | `createTableHookContexts()`                                       | factory  | the table/cell/header context set          |

- `useTable`: omitting the selector subscribes the component to every registered slice; a selector narrows the re-render and lands on `table.state`.
- `tableFeatures`: declare each prerequisite feature before the slot demanding it, in one call.
- `metaHelper`: a phantom — it returns `{}` and the table strips it at runtime, so only its type reaches `columnMeta`/`tableMeta`. `ExtractColumnMeta` hands the registered type straight back UNAPPLIED to `TData`/`TValue`, which is why one shared `columnMeta` must stay data-free: a row-typed member pins the slot to a single `TData` and breaks every other grid registering it.
- `createTableHook`: earns its slot only where several tables share features, row models, defaults, and registered components.

[ENTRYPOINT_SCOPE]: rendering a cell, header, or footer

| [INDEX] | [SURFACE]                                       | [SHAPE]   | [CAPABILITY]                            |
| :-----: | :---------------------------------------------- | :-------- | :-------------------------------------- |
|  [01]   | `table.FlexRender({ cell })`                    | property  | resolves a cell against its own context |
|  [02]   | `FlexRender({ cell \| header \| footer })`      | component | the standalone one-prop render bridge   |
|  [03]   | `flexRender(Renderable, TProps)`                | fn        | resolves a renderable against raw props |
|  [04]   | `table.Subscribe({ source, selector })`         | property  | subscribes one subtree to one slice     |
|  [05]   | `Subscribe({ source, selector })`               | component | the standalone subscription boundary    |
|  [06]   | `table.getHeaderGroups()` / `getFooterGroups()` | instance  | header and footer derivation            |
|  [07]   | `table.getRowModel().rows`                      | instance  | the final derived row array             |

- `FlexRender` admits exactly one of `cell`, `header`, or `footer`, deriving the def and context the bare `flexRender` call spells by hand.
- `table.Subscribe` is three overloads, deliberately not a union: source alone (identity, children receive the source value), source with `selector`, and store mode where `source` is omitted and `selector` is required. A union of two `selector` signatures degrades the callback parameter to implicit `any`, and the source arms are listed first so `TSourceValue` infers from `source`.
- `SubscribeSource<TValue>` is `Atom | ReadonlyAtom | Store | ReadonlyStore` of that value, so `table.atoms.<slice>`, `table.baseAtoms.<slice>`, `table.optionsStore`, and an `options.atoms` entry are all valid sources.
- `getRowModel().rows` is the array `@tanstack/react-virtual` windows.

[ENTRYPOINT_SCOPE]: feature registration — the tree-shakeable capability roster

[FEATURES]: `cellSelectionFeature` `cellSpanningFeature` `columnFacetingFeature` `columnFilteringFeature` `columnGroupingFeature` `columnOrderingFeature` `columnPinningFeature` `columnResizingFeature` `columnSizingFeature` `columnVisibilityFeature` `globalFilteringFeature` `rowAggregationFeature` `rowExpandingFeature` `rowPaginationFeature` `rowPinningFeature` `rowSelectionFeature` `rowSortingFeature`

| [INDEX] | [SURFACE]                                          | [SHAPE] | [CAPABILITY]                       |
| :-----: | :------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `sortedRowModel: createSortedRowModel()`           | slot    | client sort derivation             |
|  [02]   | `filteredRowModel: createFilteredRowModel()`       | slot    | client column-filter derivation    |
|  [03]   | `groupedRowModel: createGroupedRowModel()`         | slot    | client grouping derivation         |
|  [04]   | `expandedRowModel: createExpandedRowModel()`       | slot    | sub-row expansion derivation       |
|  [05]   | `paginatedRowModel: createPaginatedRowModel()`     | slot    | client pagination derivation       |
|  [06]   | `facetedRowModel: createFacetedRowModel()`         | slot    | per-column facet derivation        |
|  [07]   | `facetedUniqueValues: createFacetedUniqueValues()` | slot    | distinct-value map per column      |
|  [08]   | `facetedMinMaxValues: createFacetedMinMaxValues()` | slot    | numeric range per column           |
|  [09]   | `coreRowModel: createCoreRowModel()`               | slot    | overrides the automatic base model |
|  [10]   | `stockFeatures`                                    | object  | every built-in feature at once     |

- Registering a feature is what creates its state and its APIs; a missing method names a missing feature, never a typing fault.
- `useTable` builds the core row model automatically; the `coreRowModel` slot only replaces it.
- `rowAggregationFeature` stands independent of `columnGroupingFeature`: register aggregation for the aggregate APIs and add grouping only for grouped rows.
- `columnResizingFeature` layers interaction over `columnSizingFeature`, carrying the transient `columnResizing` slice, `setColumnResizing`/`onColumnResizingChange`, and `table.resetHeaderSizeInfo(defaultState?)`; `header.getResizeHandler(context?)` builds the pointer/touch handler and takes an owning `Document` for an iframe or popout, while `column.getCanResize()`/`getIsResizing()` gate and mark the active target.
- `columnResizeMode` flipped its default to `'onEnd'` in v9 — v8 committed `'onChange'` — so committed `columnSizing` lands once the drag releases unless a table opts back into live commits; `columnResizeDirection` (`'ltr'`/`'rtl'`) picks the sign the drag offset carries.
- `stockFeatures` bundles every built-in, so an explicit feature list is the tree-shaking end state.

[ENTRYPOINT_SCOPE]: `cellSelectionFeature` — the spreadsheet band, stored as corner pairs

| [INDEX] | [SURFACE]                                                                                         | [SHAPE] | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------------------------------ | :------ | :------------------------------------- |
|  [01]   | `CellSelectionRange` (`anchorColumnId`/`anchorRowId`/`focusColumnId`/`focusRowId`/`operation?`)   | type    | one rectangle as its two corners       |
|  [02]   | `CellSelectionState` = `Array<CellSelectionRange>`                                                | state   | the ordered operation list             |
|  [03]   | `cell.getTabIndex()` / `getIsFocused()` / `getCanSelect()`                                        | cell    | roving focus and the per-cell gate     |
|  [04]   | `cell.getSelectionStartHandler(contextDocument?)` / `getSelectionExtendHandler()`                 | cell    | `mousedown` open, `mouseenter` extend  |
|  [05]   | `cell.getSelectionEdges()`                                                                        | cell    | which sides sit on the outline         |
|  [06]   | `table.setCellSelection(updater)` / `resetCellSelection(defaultState?)`                           | table   | the slice writer and its reset         |
|  [07]   | `table.selectCellRange(range, { mode })` / `selectAllCells()` / `setFocusedCell(rowId, columnId)` | table   | write a rectangle, all cells, one cell |
|  [08]   | `table.moveCellSelection(direction)` / `extendCellSelection(direction)`                           | table   | keyboard collapse-move and anchor-hold |
|  [09]   | `table.getCellSelectionBounds()` / `getCellSelectionMergeBounds()`                                | table   | resolved index rectangles, merged runs |
|  [10]   | `table.getSelectedCellIds()` / `getSelectedCellRangesData()`                                      | table   | expanded ids, region-major value grids |

- A range is its `anchor` and `focus` corners, never a normalized min/max rectangle: the anchor stays put while the focus corner moves under shift-extend or drag, so the pair carries where an interaction resumes. Corners are flat row and column ids because a user-supplied `getRowId` may return any separator.
- `operation` is `'include'` or `'exclude'`, defaulting to include, and the array is ordered — later entries carve or add against what came before, and the last entry is what extend and drag operate on.
- The state is a bare `Array`, matching `SortingState` and `ColumnFiltersState`, and holds nothing transient: the open-drag flag lives outside the slice as non-reactive instance data, so the whole slice persists and rehydrates without resuming a phantom drag.
- `getSelectionStartHandler` takes an owning `contextDocument` because it attaches its own document-level `mouseup`, so a drag released outside the table still closes; pass it when the grid renders into an iframe or popout.
- `getSelectionEdges` answers per cell which sides bound the selection, so the spreadsheet outline renders without a cell inspecting its neighbours; `getCellSelectionBounds` is the memoized cache every per-cell read goes through, and ranges whose corners no longer resolve are omitted rather than clamped.
- `getSelectedCellRangesData` indexes `[region][row][column]`; the delimiter, null spelling, and quoting of a clipboard payload stay in userland.

[ENTRYPOINT_SCOPE]: `cellSpanningFeature` — merged runs over the rendered rows

| [INDEX] | [SURFACE]                                                        | [SHAPE]    | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------------- | :--------- | :------------------------------------- |
|  [01]   | `columnDef.spanRows` (`boolean \| (RowSpanContext) => boolean`)  | column def | merge adjacent equal rows into one run |
|  [02]   | `columnDef.spanColumns` (`number \| (ColSpanContext) => number`) | column def | widen a cell across later columns      |
|  [03]   | `columnDef.enableCellSpanning` / `options.enableCellSpanning`    | gate       | per column, then table-wide            |
|  [04]   | `cell.getRowSpan()` / `getColSpan()` / `getIsCovered()`          | cell       | the two span counts and the skip test  |
|  [05]   | `table.getCellSpanIndex()`                                       | table      | the memoized span index                |

- `spanRows: true` merges adjacent rows whose value for that column is the same under `Object.is`, and nullish never merges; a predicate replaces the comparison entirely and decides whether a candidate row joins the run anchored at `anchorRow`, which keeps runs transitive by construction.
- `spanColumns` counts in render order and clamps to the end of the cell's pinned region, so a span never crosses the start-pinned, center, or end-pinned boundary; `Infinity` means the rest of the region and hidden columns are not counted.
- The index is built from the rows actually rendered, in render order — top-pinned, then the paginated center, then bottom-pinned — so sorting, filtering, and paging only change which rows are adjacent, and a run never crosses a page boundary, a pinned section, a change of tree position, or a grouped row.
- A covered cell reports `0` and renders NOTHING: `rowspan="0"` means "span to the end of the row group" in HTML and would merge the cell down the whole table section, so the render loop tests `getIsCovered()` and skips rather than emitting the attribute.
- `getCellSpanIndex()` exposes `colSpans`, `rowSpans`, `columnIndexes`, and the exact ordered `rows` it was built from — cell reads compare against that array to reject stale positions, and a virtualizer reads it to place a run's anchor against the mounted window.

[ENTRYPOINT_SCOPE]: the per-table function registries and their built-ins

| [INDEX] | [SURFACE]                                      | [SHAPE] | [CAPABILITY]                             |
| :-----: | :--------------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `sortFns: { }` / `column.sortFn`               | slot    | registered `SortFn` by id                |
|  [02]   | `filterFns: { }` / `column.filterFn`           | slot    | registered `FilterFn` by id              |
|  [03]   | `aggregationFns: { }` / `column.aggregationFn` | slot    | grouped-row aggregation by id            |
|  [04]   | `columnMeta` / `tableMeta` / `filterMeta`      | slot    | the per-table meta types, phantom-valued |

[SORT_FNS]: `sortFn_alphanumeric` `sortFn_alphanumericCaseSensitive` `sortFn_basic` `sortFn_datetime` `sortFn_text` `sortFn_textCaseSensitive`

[FILTER_FNS]: `filterFn_includesString` `filterFn_includesStringSensitive` `filterFn_equals` `filterFn_equalsString` `filterFn_equalsStringSensitive` `filterFn_weakEquals` `filterFn_startsWith` `filterFn_endsWith` `filterFn_arrHas` `filterFn_arrIncludes` `filterFn_arrIncludesAll` `filterFn_arrIncludesSome` `filterFn_between` `filterFn_betweenInclusive` `filterFn_inNumberRange` `filterFn_inDateRange` `filterFn_greaterThan` `filterFn_greaterThanOrEqualTo` `filterFn_lessThan` `filterFn_lessThanOrEqualTo` `filterFn_empty` `filterFn_notEmpty`

[AGGREGATION_FNS]: `aggregationFn_sum` `aggregationFn_mean` `aggregationFn_median` `aggregationFn_min` `aggregationFn_max` `aggregationFn_extent` `aggregationFn_count` `aggregationFn_uniqueCount` `aggregationFn_unique` `aggregationFn_first` `aggregationFn_last`

- Import each built-in individually and register it under its conventional key; the aggregate `sortFns`/`filterFns`/`aggregationFns` objects are `@deprecated` upstream — each pulls every built-in into the bundle, and `'auto'` name resolution only finds what a table registers itself.
- Registered keys become the inferred string union `column.sortFn`, `column.filterFn`, `globalFilterFn`, and `column.aggregationFn` accept — `SortFnOption` is `'auto' | ExtractSortFnKeys<TFeatures> | SortFn`, so an inline comparator remains legal where no id fits.
- `constructSortFn` / `constructFilterFn` / `constructAggregationFn` build a custom entry carrying the same descriptor the built-ins carry.
- `SortFnDef` is value-level, not row-level: `sort(dataValueA, dataValueB, rowA, rowB, columnId)` receives the two cell values already extracted, with the rows behind them for a tiebreak, and an optional `resolveDataValue` normalizer (lowercase, strip diacritics, coerce) runs first. `constructSortFn` returns a `CreatedSortFn` carrying that def back, so a variant spreads the original into a second call.

[ENTRYPOINT_SCOPE]: state ownership, reads, and subscription

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :---------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `options.atoms: { sorting: Atom }`  | option   | hands a slice's ownership to an outside atom |
|  [02]   | `options.state` + `on<Slice>Change` | option   | the controlled per-slice pair                |
|  [03]   | `table.state`                       | property | the selector's projection, read in render    |
|  [04]   | `table.atoms.<slice>.get()`         | property | one slice's readonly derived snapshot        |
|  [05]   | `table.baseAtoms.<slice>`           | property | the internal writable atom per slice         |
|  [06]   | `table.store`                       | property | readonly flat store; `@deprecated` in render |
|  [07]   | `table.optionsStore`                | property | the resolved options as one atom             |
|  [08]   | `table.reset()`                     | instance | restores internal atoms to `initialState`    |

[STATE_UTIL]: `functionalUpdate(updater, input)` `makeStateUpdater(key, instance)` `memo(deps, fn, opts)` `tableMemo(options)`

- Slice ownership resolves in one order — `options.atoms` beats `options.state`, which beats the internal base atom; declaring one slice twice makes precedence the application's logic.
- `table.reset()` restores internal atoms alone and leaves an externally-owned atom untouched.
- The React adapter marks `table.store` `@deprecated` — `store.state` is a current-value snapshot with no subscription, so a render read silently misses updates; `table.state` reads, `table.atoms.<slice>.get()` snapshots one slice, and `table.Subscribe` subscribes.
- `functionalUpdate` resolves an `Updater`, `makeStateUpdater` builds the keyed setter, and `tableMemo` memoizes a feature derivation against the table.
- `table.getIsSomeRowsSelected()`/`getIsSomePageRowsSelected()` read as at-least-one and stay true when all are selected; pair each with `!getIsAllRowsSelected()`/`!getIsAllPageRowsSelected()` for an indeterminate control.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Headless by construction: the package returns a `Table` instance — plain data and derivation functions — and never touches the DOM. `ui` owns all markup through the react-aria grid spine (`grid`/`row`/`columnheader`/`gridcell` roles and keyboard nav), and `FlexRender` is the single bridge resolving a column's `cell`/`header`/`footer` against its typed context. No styled component exists to override; the token vocabulary lives entirely in `ui`.
- `tableFeatures` decides the whole surface: it stitches feature modules, row-model factories, function registries, and type-only meta slots into the value `TFeatures` infers from, and `Table`, `Row`, `Cell`, `Column`, `ColumnDef`, `TableState`, and `TableOptions` narrow through it. Omitting a capability strips it from the state, the API, and the bundle alike, and `ValidateFeatureSlots` turns a slot missing its prerequisite into a compile error naming the feature to add.
- Core and adapter split: every type, feature, row model, and function registry is `@tanstack/table-core`; `@tanstack/react-table` adds `useTable` (wire the core store into a React refresh), the `FlexRender` and `Subscribe` components, and `createTableHook`. `@tanstack/react-virtual` over `virtual-core` shares this split — one headless core, one thin framework adapter.
- State is atoms under a three-tier precedence: every registered slice gets an internal writable base atom, `options.state` synchronizes into it, and `options.atoms[slice]` overrides both — table setters write that outside atom directly with no callback glue, and it survives `table.reset()`. Upstream names the external atom the preferred v9 ownership model, and `makeStateUpdater` enforces it at the source, resolving `options.atoms?.[key] ?? baseAtoms[key]` before every write. `Atom` is a structural interface, so an `@effect-atom` fold reaches the rail as an adapter atom over one slice rather than through a second binding.
- Subscription is explicit and per-slice: `useTable` without a selector re-renders on every registered slice, a selector narrows the component to its projection on `table.state`, and `table.Subscribe` pushes a still-narrower boundary down the tree against one atom. Render breadth is a chosen seam, never a default.
- Row, cell, column, and header methods live on shared prototypes: call each through its receiver, because destructuring, spread, `Object.keys`, and `JSON.stringify` drop them. Table-instance methods carry no such constraint.
- Function registries are per-table named tables: `sortFns`/`filterFns`/`aggregationFns` slots key a column's behavior by id and infer the accepted string union from the registered keys, so new sort, filter, or aggregation behavior is one registry entry, never a new code path or a global interface augmentation.

[STACKING]:
- `@tanstack/react-virtual` (`.api/tanstack-react-virtual.md`): the sibling half of the `view/table` collection rows — `useVirtualizer({ count: table.getRowModel().rows.length, getItemKey: (i) => rows[i].id, estimateSize })` windows the derived rows so a 100k-row table renders only the visible span, and `column.getSize()` feeds the cell width the virtual row lays out; the two share the headless-core and React-adapter architecture and compose into one virtualized data grid.
- `react-aria` / `react-aria-components` (`.api/react-aria-components.md`): supply the grid interaction and ARIA semantics the headless table lacks — `columnheader` sort announcement off `column.getToggleSortingHandler()`, `gridcell` focus management, roving tabindex, multi-select keyboard model — so the table is accessible without `ui` hand-rolling the a11y layer; `cellSelectionFeature`'s `cell.getTabIndex()` and `cell.getSelectionStartHandler()` hand the roving-focus and range-drag seam straight to the RAC spine.
- `@effect-atom/atom` + `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): the `ONE_FOLD_ONE_BINDING` law binds `Table` state — the sorting/filter/selection/pagination fold is one atom, and `view/table`'s `Grid.edge` projects one slice of it onto an `options.atoms` entry, `functionalUpdate` resolving the `Updater` inside the fold's own update; `table.state` reads the selector's projection back, and a server-driven table binds an `AtomHttpApi` row to feed `data` with `manualPagination`.
- `@tanstack/store` (`.api/tanstack-store.md`): the atom vocabulary this rail is typed in — `options.atoms`, `table.baseAtoms`, `table.atoms`, `table.store`, `table.optionsStore`, and `SubscribeSource` all speak it. It is admitted for that vocabulary alone; the fold stays with `@effect-atom` and the adapter atom is the only cell `ui` mints.
- `effect` `Schema` (`libs/typescript/.api/effect.md`): the decoded `wire` row type is the `TData` generic; `createColumnHelper<typeof features, Schema.Type>()` types every accessor against it, `metaHelper` declares the `columnMeta` slot carrying the column's decoded band facts, and `RowSelectionState` keyed by the `GlobalId` brand carries the selection to `viewer/mark/selection` without re-deriving identity.
- `class-variance-authority` + `tailwind-merge` (`.api/class-variance-authority.md`, `.api/tailwind-merge.md`): the cell/header/row class dispatch — density, alignment, sort-direction, and selection variants resolve to tailwind classes through `cva`, conflict-merged by `twMerge`, styling the headless output; `column.getIsPinned()` returning `start`/`end` picks the logical inset utility the sticky column rides.
- `cmdk` (`.api/cmdk.md`): a fuzzy filter registered in the table's `filterFns` slot powers a command-palette-style column filter, sharing the rank algorithm the command palette uses.

[LOCAL_ADMISSION]:
- Build one `tableFeatures` object per table shape outside the component, and enter through `useTable` + `createColumnHelper` + `FlexRender`; the headless engine and the react-aria grid spine form the contract, never a styled table component owning markup.
- Register only the features a table uses and import each built-in sort, filter, and aggregation function individually into its slot; the tree-shake is the point, never `stockFeatures` or a whole-registry spread in a shipped table.
- Type every helper and public column type against `typeof features`, and declare per-table meta through the `columnMeta`/`tableMeta` slots with `metaHelper`, never a global interface augmentation two tables then fight over.
- Bind every slice the `@effect-atom` fold owns, persists, feeds to the viewer, or round-trips to the server through `options.atoms`, handing it the adapter atom `Grid.edge` mints over that slice of the fold; the `state` + `on<Slice>Change` pair beside a fold is the two-writer defect, because the table already wrote the atom before the callback ran.
- Leave `state` and its callbacks to a slice no fold owns, and never declare one slice through both routes — `options.atoms` silently wins the precedence, so the controlled pair becomes dead code the next reader mistakes for the binding.
- Choose the render boundary deliberately — a `useTable` selector for the instance holder and `table.Subscribe` for a subtree reading one slice — rather than accepting the whole-state default under a virtualized grid.
- Call row, cell, column, and header methods through their receiver, and spell pinning as `start`/`end` in state, arguments, comparisons, and the CSS logical inset the sticky layout rides.
- Type columns against the decoded `wire` Schema type; the `wire` `#vocab` owns the row shape, never a re-declared or `any`-typed row.

[RAIL_LAW]:
- Package: `@tanstack/react-table` (over `@tanstack/table-core`)
- Owns: the `ColumnDef` model, the `Table`/`Row`/`Cell`/`Header`/`Column` instance surface, the static `tableFeatures` registry over every capability (sort/filter/group/expand/paginate/facet/select/span/pin/order/size/resize/visibility), the atom-backed state rail with its three-tier ownership precedence, and the per-table `sortFns`/`filterFns`/`aggregationFns` registries
- Accept: headless usage with `ui`-owned react-aria markup, an explicit feature object with individually-imported built-ins, `createColumnHelper` typed against `typeof features` and the `wire` Schema, `@effect-atom`-owned slices through `options.atoms` fed by the `Grid.edge` adapter atom, `state` with a per-slice controlled pair for a slice no fold owns, selector and `table.Subscribe` render boundaries, react-virtual windowing over `getRowModel().rows`
- Reject: a styled table component owning markup or a11y, `stockFeatures` or whole-registry spreads in a shipped table, inline per-column comparators where a registry entry fits, global meta augmentation where a per-table slot fits, a data-typed member on the shared `columnMeta` slot, destructured row/cell/column/header methods, physical `left`/`right` pinning vocabulary, a `state` + `on<Slice>Change` pair beside a fold that already owns the slice, `table.store` read in render, `any`-typed rows, a re-declared row shape duplicating the `wire` `#vocab`
