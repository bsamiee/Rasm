# [RASM_APPUI_API_AVALONIA_GRID]

`Avalonia.Controls.DataGrid` supplies the virtualized table control plus its own `DataGridCollectionView` projection engine (filter/sort/group/page/current-row/add-edit) over `Avalonia.Collections`. The retained product-UI tables (schedule, cost, diff, property inspectors, diagnostics rosters) bind one DynamicData-projected `ReadOnlyObservableCollection<TRow>` into `ItemsSource` and drive selection, edit, and sort state through a ReactiveUI view model — the grid is the single tabular boundary; no parallel hand-rolled list control exists.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Controls.DataGrid`
- package: `Avalonia.Controls.DataGrid`
- version: `12.0.0`
- license: `MIT`
- assembly: `Avalonia.Controls.DataGrid`
- consumer-tfm: `net10.0` (multi-target `net10.0`/`net8.0`; `net10.0` is the bound asset)
- build-floor: Avalonia `12.0.x` core (peer `Avalonia` `12.0.5`)
- namespace: `Avalonia.Controls`
- namespace: `Avalonia.Controls.Primitives`
- namespace: `Avalonia.Collections`
- asset: runtime library (no native payload)
- rail: tables

## [02]-[PUBLIC_TYPES]

[GRID_CONTROLS]: table controls and containers
- rail: tables

| [INDEX] | [SYMBOL]                   | [RAIL]           |
| :-----: | :------------------------- | :--------------- |
|  [01]   | `DataGrid`                 | table root       |
|  [02]   | `DataGridRow`              | row container    |
|  [03]   | `DataGridCell`             | cell container   |
|  [04]   | `DataGridColumnHeader`     | header container |
|  [05]   | `DataGridRowHeader`        | row header       |
|  [06]   | `DataGridCellsPresenter`   | cell presenter   |
|  [07]   | `DataGridRowsPresenter`    | row presenter    |
|  [08]   | `DataGridDetailsPresenter` | detail presenter |

[COLUMN_TYPES]: column and edit model
- rail: tables

| [INDEX] | [SYMBOL]                  | [RAIL]                              |
| :-----: | :------------------------ | :---------------------------------- |
|  [01]   | `DataGridColumn`          | column base (`IsReadOnly`, sorting) |
|  [02]   | `DataGridBoundColumn`     | `Binding`/`ClipboardContentBinding` |
|  [03]   | `DataGridTextColumn`      | text column                         |
|  [04]   | `DataGridCheckBoxColumn`  | boolean column                      |
|  [05]   | `DataGridTemplateColumn`  | template column                     |
|  [06]   | `DataGridLength`          | sizing value                        |
|  [07]   | `DataGridLengthConverter` | sizing converter                    |

[GRID_ENUMS]: bounded table vocabulary
- rail: tables

| [INDEX] | [SYMBOL]                            | [RAIL]                                   |
| :-----: | :---------------------------------- | :--------------------------------------- |
|  [01]   | `DataGridSelectionMode`             | `Single`/`Extended`                      |
|  [02]   | `DataGridEditingUnit`               | `Cell`/`Row` edit scope                  |
|  [03]   | `DataGridEditAction`                | `Commit`/`Cancel` end-edit intent        |
|  [04]   | `DataGridClipboardCopyMode`         | `None`/`ExcludeHeader`/`IncludeHeader`   |
|  [05]   | `DataGridHeadersVisibility`         | `All`/`Column`/`Row`/`None`              |
|  [06]   | `DataGridGridLinesVisibility`       | gridline rendering mode                  |
|  [07]   | `DataGridRowDetailsVisibilityMode`  | `Collapsed`/`Visible`/`VisibleWhenSel.`  |
|  [08]   | `DataGridLengthUnitType`            | `Auto`/`Pixel`/`SizeToCells`/`SizeToHdr` |

[GRID_EVENTS]: event argument surfaces
- rail: tables

| [INDEX] | [SYMBOL]                                | [RAIL]            |
| :-----: | :-------------------------------------- | :---------------- |
|  [01]   | `DataGridAutoGeneratingColumnEventArgs` | column generation |
|  [02]   | `DataGridBeginningEditEventArgs`        | edit start        |
|  [03]   | `DataGridCellEditEndingEventArgs`       | cell edit close   |
|  [04]   | `DataGridCellEditEndedEventArgs`        | cell edit result  |
|  [05]   | `DataGridColumnEventArgs`               | column/sort event |
|  [06]   | `DataGridRowEventArgs`                  | row event         |
|  [07]   | `DataGridRowEditEndingEventArgs`        | row edit close    |
|  [08]   | `DataGridRowDetailsEventArgs`           | row details       |
|  [09]   | `DataGridPreparingCellForEditEventArgs` | editor mount      |
|  [10]   | `DataGridRowClipboardEventArgs`         | clipboard row     |

[COLLECTION_VIEW_TYPES]: tabular collection view
- rail: tables

| [INDEX] | [SYMBOL]                           | [RAIL]                              |
| :-----: | :--------------------------------- | :---------------------------------- |
|  [01]   | `IDataGridCollectionView`          | view contract                       |
|  [02]   | `IDataGridCollectionViewFactory`   | view factory                        |
|  [03]   | `DataGridCollectionView`           | filter/sort/group/page engine       |
|  [04]   | `DataGridCollectionViewGroup`      | materialized group node             |
|  [05]   | `DataGridSortDescription`          | sort descriptor (`FromPath`/custom) |
|  [06]   | `DataGridGroupDescription`         | group descriptor base               |
|  [07]   | `DataGridPathGroupDescription`     | property-path group descriptor      |
|  [08]   | `DataGridCurrentChangingEventArgs` | current-row guard event             |
|  [09]   | `PageChangingEventArgs`            | page-change guard event             |

## [03]-[ENTRYPOINTS]

[GRID_STATE_ENTRYPOINTS]: row source, selection, and policy
- rail: tables

| [INDEX] | [SURFACE]                                       | [SURFACE_ROOT] | [RAIL]               |
| :-----: | :---------------------------------------------- | :------------- | :------------------- |
|  [01]   | `ItemsSource` (`IEnumerable`)                   | `DataGrid`     | row source           |
|  [02]   | `Columns` (`ObservableCollection<DataGridColumn>`) | `DataGrid`  | column model         |
|  [03]   | `CollectionView` (`IDataGridCollectionView`)    | `DataGrid`     | live view projection |
|  [04]   | `SelectionMode` (`DataGridSelectionMode`)       | `DataGrid`     | selection policy     |
|  [05]   | `SelectedItem` / `SelectedIndex`                | `DataGrid`     | single selection     |
|  [06]   | `SelectedItems` (`IList`)                       | `DataGrid`     | multi selection      |
|  [07]   | `CurrentColumn` (`DataGridColumn`)              | `DataGrid`     | focus cell column    |
|  [08]   | `IsReadOnly`                                    | `DataGrid`     | edit gate            |
|  [09]   | `AutoGenerateColumns`                           | `DataGrid`     | column generation    |
|  [10]   | `CanUserSortColumns` / `CanUserResizeColumns` / `CanUserReorderColumns` | `DataGrid` | user-interaction gates |
|  [11]   | `FrozenColumnCount` (`int`)                     | `DataGrid`     | pinned left columns  |
|  [12]   | `ClipboardCopyMode` (`DataGridClipboardCopyMode`) | `DataGrid`   | copy policy          |
|  [13]   | `HeadersVisibility` / `GridLinesVisibility`     | `DataGrid`     | chrome policy        |

[GRID_EDIT_ENTRYPOINTS]: edit, scroll, and grouping operations
- rail: tables

| [INDEX] | [SURFACE]                                              | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :---------------------------------------------------- | :------------- | :------------------ |
|  [01]   | `BeginEdit()` / `BeginEdit(RoutedEventArgs)`          | `DataGrid`     | edit start          |
|  [02]   | `CommitEdit()` / `CommitEdit(DataGridEditingUnit, bool exit)` | `DataGrid` | edit commit (scoped) |
|  [03]   | `CancelEdit()` / `CancelEdit(DataGridEditingUnit)`    | `DataGrid`     | edit cancel (scoped) |
|  [04]   | `ScrollIntoView(object item, DataGridColumn column)`  | `DataGrid`     | row/cell reveal     |
|  [05]   | `RowDetailsVisibilityMode` / `RowDetailsTemplate`     | `DataGrid`     | inline detail panel |
|  [06]   | `AreRowDetailsFrozen` / `AreRowGroupHeadersFrozen`    | `DataGrid`     | scroll pinning      |
|  [07]   | `ExpandRowGroup(group, all)` / `CollapseRowGroup(group, all)` | `DataGrid` | group toggle        |
|  [08]   | `GetGroupFromItem(item, level)`                       | `DataGrid`     | group lookup        |

[GRID_EVENT_ENTRYPOINTS]: table event operations
- rail: tables

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :----------------------------------------- | :------------- | :------------------ |
|  [01]   | `AutoGeneratingColumn`                     | `DataGrid`     | column generation   |
|  [02]   | `BeginningEdit` / `PreparingCellForEdit`   | `DataGrid`     | edit-mount hooks    |
|  [03]   | `CellEditEnding` / `CellEditEnded`         | `DataGrid`     | cell edit lifecycle |
|  [04]   | `RowEditEnding` / `RowEditEnded`           | `DataGrid`     | row edit lifecycle  |
|  [05]   | `LoadingRow` / `UnloadingRow`              | `DataGrid`     | row recycle         |
|  [06]   | `LoadingRowDetails` / `UnloadingRowDetails` / `RowDetailsVisibilityChanged` | `DataGrid` | details lifecycle |
|  [07]   | `Sorting` (`DataGridColumnEventArgs`)      | `DataGrid`     | sort intercept      |
|  [08]   | `SelectionChanged`                         | `DataGrid`     | selection signal    |

[COLUMN_ENTRYPOINTS]: per-column model
- rail: tables

| [INDEX] | [SURFACE]                                     | [SURFACE_ROOT]         | [RAIL]               |
| :-----: | :-------------------------------------------- | :--------------------- | :------------------- |
|  [01]   | `Binding` (`BindingBase`)                     | `DataGridBoundColumn`  | value path           |
|  [02]   | `ClipboardContentBinding`                     | `DataGridBoundColumn`  | copy path override   |
|  [03]   | `SortMemberPath` / `CustomSortComparer` (`IComparer`) | `DataGridColumn` | sort key/comparer    |
|  [04]   | `Header` / `HeaderTemplate`                   | `DataGridColumn`       | header content       |
|  [05]   | `Width` (`DataGridLength`) / `MinWidth` / `MaxWidth` | `DataGridColumn` | sizing               |
|  [06]   | `IsReadOnly` / `IsVisible` / `DisplayIndex`   | `DataGridColumn`       | per-column policy    |
|  [07]   | `CanUserSort` / `CanUserResize` / `CanUserReorder` | `DataGridColumn`  | per-column gates     |
|  [08]   | `CellStyleClasses`                            | `DataGridColumn`       | conditional styling  |
|  [09]   | `CellTemplate` / `CellEditingTemplate` (`IDataTemplate`) | `DataGridTemplateColumn` | display + edit cell template |

[COLLECTION_VIEW_ENTRYPOINTS]: view projection operations
- rail: tables

| [INDEX] | [SURFACE]                                     | [SURFACE_ROOT]           | [RAIL]            |
| :-----: | :-------------------------------------------- | :----------------------- | :---------------- |
|  [01]   | `Filter` (`Func<object,bool>`) / `CanFilter`  | `DataGridCollectionView` | predicate filter  |
|  [02]   | `SortDescriptions` (`DataGridSortDescriptionCollection`) | `DataGridCollectionView` | multi-key sort |
|  [03]   | `GroupDescriptions`                           | `DataGridCollectionView` | grouping state    |
|  [04]   | `PageSize` / `PageIndex` / `MoveToPage(n)`    | `DataGridCollectionView` | paging            |
|  [05]   | `MoveCurrentTo` / `MoveCurrentToFirst` / `MoveCurrentToNext` | `DataGridCollectionView` | current row |
|  [06]   | `DeferRefresh()` (`IDisposable`)              | `DataGridCollectionView` | batched refresh   |
|  [07]   | `AddNew()` / `CommitNew()` / `CancelNew()`    | `DataGridCollectionView` | row creation      |
|  [08]   | `EditItem(item)` / `CommitEdit()` / `CancelEdit()` | `DataGridCollectionView` | row edit txn  |
|  [09]   | `Refresh()`                                   | `DataGridCollectionView` | full re-project   |
|  [10]   | `CollectionChanged` / `CurrentChanged`        | `DataGridCollectionView` | view signals      |

## [04]-[IMPLEMENTATION_LAW]

[GRID_TOPOLOGY]:
- `DataGrid` is a virtualized control: `ItemsSource` (`IEnumerable`) is realized lazily through `DataGridRowsPresenter`/`DataGridCellsPresenter`, and `LoadingRow`/`UnloadingRow` recycle row containers — never assume a `DataGridRow` exists for an off-screen item.
- `DataGrid.CollectionView` (`DirectProperty`) exposes the live `IDataGridCollectionView`; when `ItemsSource` is a plain `IEnumerable`, the grid wraps it in an internal `DataGridCollectionView`. Filter/sort/group/page state belongs on that view, not on the source collection.
- Editing is two-level: `DataGridEditingUnit.Cell` vs `.Row`. `CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true)` validates and persists the whole row; `BeginningEdit`/`CellEditEnding`/`RowEditEnding` are the cancellable guard hooks (set `e.Cancel = true` to veto), and `CellEditEnded`/`RowEditEnded` are the post-commit observation points.
- Sorting routes through `DataGridColumn.SortMemberPath` plus optional `CustomSortComparer` (`IComparer`); the `Sorting` event intercepts to substitute a domain comparer or push the order into the backing query. `CanUserSortColumns` gates header-click sort.
- Selection is `DataGridSelectionMode.Single` or `.Extended`; `SelectedItems` (`IList`) is live under `Extended`. `FrozenColumnCount` pins the leading N columns during horizontal scroll; `ClipboardCopyMode` controls header inclusion for `Ctrl+C`, and `DataGridBoundColumn.ClipboardContentBinding` overrides the copied projection per column.
- Grouping materializes `DataGridCollectionViewGroup` nodes from `GroupDescriptions`; `ExpandRowGroup`/`CollapseRowGroup` and `AreRowGroupHeadersFrozen` drive the group-header UX.
- `DataGridCollectionView.DeferRefresh()` returns an `IDisposable` scope that suppresses re-projection across a batch of `SortDescriptions`/`GroupDescriptions`/`Filter` mutations — wrap multi-axis state changes in it to collapse N refreshes into one.

[STACKING_LAW]:
- DynamicData feed (`api-dynamicdata`): the canonical row source is a `SourceCache<TRow,TKey>` or `SourceList<TRow>` whose `.Connect()` pipeline (`Filter`/`Sort`/`Transform`/`Bind`) materializes a `ReadOnlyObservableCollection<TRow>` bound directly to `ItemsSource`. Coarse server-side or domain filtering lives in the DynamicData pipeline; fine interactive filter/sort/group/page lives in `DataGridCollectionView`. The two layers compose — DynamicData owns the reactive set algebra, the grid view owns the presentation projection.
- ReactiveUI binding (`api-reactiveui-avalonia`, `api-reactive`): `SelectedItem`/`SelectedItems`/`CurrentColumn` and the edit-lifecycle events bind to a `ReactiveObject` view model via `WhenAnyValue`/`Bind`; sort and filter intents are `ReactiveCommand`s that mutate the `DataGridCollectionView` (or the upstream DynamicData operator) rather than mutating UI state imperatively.
- Thinktecture rows (`libs/csharp/.api/api-thinktecture-json.md`, shared tier): table rows are `[ValueObject]`/record DTOs with Thinktecture `SmartEnum`/`ValueObject` cells; `DataGridColumn.Binding` targets the strongly-typed members and `CustomSortComparer` consumes the value-object ordering, so the grid never sees stringly-typed cells.
- The retained panels (schedule/cost/diff rosters, property inspectors, diagnostics tables) are all DataGrid instances over this one rail; `Dock.Avalonia` (`api-dock`) hosts them as dockable documents/tools, and `api-avalonia-fluent` supplies the column-header/cell theme.

[TABLE_LAW]:
- Package: `Avalonia.Controls.DataGrid`
- Owns: virtualized table projection, two-level editable rows, sortable/groupable/pageable columns, frozen columns, clipboard, and the `DataGridCollectionView` state engine
- Accept: typed rows feed one grid through a DynamicData-bound `ReadOnlyObservableCollection`; filter/sort/group/page state lives on `DataGridCollectionView`; edit/selection bind to a ReactiveUI view model
- Reject: an ad hoc table control beside the grid; filter/sort logic hidden in source-collection mutation instead of the view; stringly-typed cells in place of value-object rows
