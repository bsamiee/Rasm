# [RASM_APPUI_API_AVALONIA_GRID]

`Avalonia.Controls.DataGrid` owns the AppUi tabular rail: a virtualized `TemplatedControl` over `ItemsSource` with two-level editable rows and sortable, groupable, pageable, frozen columns, paired with the `DataGridCollectionView` engine that folds filter, sort, group, page, and current-row state over `Avalonia.Collections`. Typed rows reach it as one DynamicData-projected `ReadOnlyObservableCollection` bound into `ItemsSource`, and selection and edit state bind through a ReactiveUI view model. This is the single tabular boundary; no parallel hand-rolled list control exists.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Controls.DataGrid`
- package: `Avalonia.Controls.DataGrid` (MIT)
- assembly: `Avalonia.Controls.DataGrid`
- consumer-tfm: `net10.0` (multi-target `net10.0`/`net8.0`; `net10.0` is the bound asset)
- namespace: `Avalonia.Controls`, `Avalonia.Controls.Primitives`, `Avalonia.Collections`
- asset: runtime library (no native payload)
- rail: tables

## [02]-[PUBLIC_TYPES]

[GRID_CONTROLS]: table controls and containers

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]     |
| :-----: | :------------------------- | :------------ | :--------------- |
|  [01]   | `DataGrid`                 | class         | table root       |
|  [02]   | `DataGridRow`              | class         | row container    |
|  [03]   | `DataGridCell`             | class         | cell container   |
|  [04]   | `DataGridColumnHeader`     | class         | header container |
|  [05]   | `DataGridRowHeader`        | class         | row header       |
|  [06]   | `DataGridCellsPresenter`   | class         | cell presenter   |
|  [07]   | `DataGridRowsPresenter`    | class         | row presenter    |
|  [08]   | `DataGridDetailsPresenter` | class         | detail presenter |

[COLUMN_TYPES]: column and edit model

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]      |
| :-----: | :------------------------ | :------------ | :---------------- |
|  [01]   | `DataGridColumn`          | class         | column base       |
|  [02]   | `DataGridBoundColumn`     | class         | bound column base |
|  [03]   | `DataGridTextColumn`      | class         | text column       |
|  [04]   | `DataGridCheckBoxColumn`  | class         | boolean column    |
|  [05]   | `DataGridTemplateColumn`  | class         | template column   |
|  [06]   | `DataGridLength`          | struct        | sizing value      |
|  [07]   | `DataGridLengthConverter` | class         | sizing converter  |

[GRID_ENUMS]: bounded table vocabulary

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :--------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `DataGridSelectionMode`            | enum          | `Single`/`Extended`                         |
|  [02]   | `DataGridEditingUnit`              | enum          | `Cell`/`Row` edit scope                     |
|  [03]   | `DataGridEditAction`               | enum          | `Commit`/`Cancel` end-edit intent           |
|  [04]   | `DataGridClipboardCopyMode`        | enum          | `None`/`ExcludeHeader`/`IncludeHeader`      |
|  [05]   | `DataGridHeadersVisibility`        | enum          | `All`/`Column`/`Row`/`None`                 |
|  [06]   | `DataGridGridLinesVisibility`      | enum          | gridline rendering mode                     |
|  [07]   | `DataGridRowDetailsVisibilityMode` | enum          | `Collapsed`/`Visible`/`VisibleWhenSelected` |
|  [08]   | `DataGridLengthUnitType`           | enum          | `Auto`/`Pixel`/`SizeToCells`/`SizeToHeader` |

[GRID_EVENTS]: event argument surfaces

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]      |
| :-----: | :-------------------------------------- | :------------ | :---------------- |
|  [01]   | `DataGridAutoGeneratingColumnEventArgs` | class         | column generation |
|  [02]   | `DataGridBeginningEditEventArgs`        | class         | edit start        |
|  [03]   | `DataGridCellEditEndingEventArgs`       | class         | cell edit close   |
|  [04]   | `DataGridCellEditEndedEventArgs`        | class         | cell edit result  |
|  [05]   | `DataGridColumnEventArgs`               | class         | column/sort event |
|  [06]   | `DataGridRowEventArgs`                  | class         | row event         |
|  [07]   | `DataGridRowEditEndingEventArgs`        | class         | row edit close    |
|  [08]   | `DataGridRowDetailsEventArgs`           | class         | row details       |
|  [09]   | `DataGridPreparingCellForEditEventArgs` | class         | editor mount      |
|  [10]   | `DataGridRowClipboardEventArgs`         | class         | clipboard row     |

[COLLECTION_VIEW_TYPES]: tabular collection view

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :--------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `IDataGridCollectionView`          | interface     | view contract                               |
|  [02]   | `IDataGridCollectionViewFactory`   | interface     | view factory                                |
|  [03]   | `DataGridCollectionView`           | class         | filter/sort/group/page engine               |
|  [04]   | `DataGridCollectionViewGroup`      | class         | materialized group node                     |
|  [05]   | `DataGridSortDescription`          | class         | sort descriptor (`FromPath`/`FromComparer`) |
|  [06]   | `DataGridGroupDescription`         | class         | group descriptor base                       |
|  [07]   | `DataGridPathGroupDescription`     | class         | property-path group descriptor              |
|  [08]   | `DataGridCurrentChangingEventArgs` | class         | current-row guard event                     |
|  [09]   | `PageChangingEventArgs`            | class         | page-change guard event                     |

## [03]-[ENTRYPOINTS]

[GRID_STATE_ENTRYPOINTS]: row source, selection, and policy on `DataGrid`

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]           |
| :-----: | :---------------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `ItemsSource` (`IEnumerable`)                                           | property | row source             |
|  [02]   | `Columns` (`ObservableCollection<DataGridColumn>`)                      | property | column model           |
|  [03]   | `CollectionView` (`IDataGridCollectionView`)                            | property | live view projection   |
|  [04]   | `SelectionMode` (`DataGridSelectionMode`)                               | property | selection policy       |
|  [05]   | `SelectedItem` / `SelectedIndex`                                        | property | single selection       |
|  [06]   | `SelectedItems` (`IList`)                                               | property | multi selection        |
|  [07]   | `CurrentColumn` (`DataGridColumn`)                                      | property | focus cell column      |
|  [08]   | `IsReadOnly`                                                            | property | edit gate              |
|  [09]   | `AutoGenerateColumns`                                                   | property | column generation      |
|  [10]   | `CanUserSortColumns` / `CanUserResizeColumns` / `CanUserReorderColumns` | property | user-interaction gates |
|  [11]   | `FrozenColumnCount` (`int`)                                             | property | pinned left columns    |
|  [12]   | `ClipboardCopyMode` (`DataGridClipboardCopyMode`)                       | property | copy policy            |
|  [13]   | `HeadersVisibility` / `GridLinesVisibility`                             | property | chrome policy          |

[GRID_EDIT_ENTRYPOINTS]: edit, scroll, and grouping operations on `DataGrid`

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]         |
| :-----: | :------------------------------------------------------- | :------- | :------------------- |
|  [01]   | `BeginEdit()` / `BeginEdit(RoutedEventArgs)`             | instance | edit start           |
|  [02]   | `CommitEdit()` / `CommitEdit(DataGridEditingUnit, bool)` | instance | edit commit (scoped) |
|  [03]   | `CancelEdit()` / `CancelEdit(DataGridEditingUnit)`       | instance | edit cancel (scoped) |
|  [04]   | `ScrollIntoView(object, DataGridColumn)`                 | instance | row/cell reveal      |
|  [05]   | `RowDetailsVisibilityMode` / `RowDetailsTemplate`        | property | inline detail panel  |
|  [06]   | `AreRowDetailsFrozen` / `AreRowGroupHeadersFrozen`       | property | scroll pinning       |
|  [07]   | `ExpandRowGroup(DataGridCollectionViewGroup, bool)`      | instance | group expand         |
|  [08]   | `CollapseRowGroup(DataGridCollectionViewGroup, bool)`    | instance | group collapse       |
|  [09]   | `GetGroupFromItem(object, int)`                          | instance | group lookup         |

[GRID_EVENT_ENTRYPOINTS]: table event surfaces on `DataGrid`

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]        |
| :-----: | :-------------------------------------------------------------------------- | :------ | :------------------ |
|  [01]   | `AutoGeneratingColumn`                                                      | event   | column generation   |
|  [02]   | `BeginningEdit` / `PreparingCellForEdit`                                    | event   | edit-mount hooks    |
|  [03]   | `CellEditEnding` / `CellEditEnded`                                          | event   | cell edit lifecycle |
|  [04]   | `RowEditEnding` / `RowEditEnded`                                            | event   | row edit lifecycle  |
|  [05]   | `LoadingRow` / `UnloadingRow`                                               | event   | row recycle         |
|  [06]   | `LoadingRowDetails` / `UnloadingRowDetails` / `RowDetailsVisibilityChanged` | event   | details lifecycle   |
|  [07]   | `Sorting` (`DataGridColumnEventArgs`)                                       | event   | sort intercept      |
|  [08]   | `SelectionChanged`                                                          | event   | selection signal    |

[COLUMN_ENTRYPOINTS]: per-column model, owner qualified

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------ | :------- | :---------------------- |
|  [01]   | `DataGridBoundColumn.Binding` (`BindingBase`)                       | property | value path              |
|  [02]   | `DataGridBoundColumn.ClipboardContentBinding`                       | property | copy path override      |
|  [03]   | `DataGridColumn.SortMemberPath`                                     | property | sort key path           |
|  [04]   | `DataGridColumn.CustomSortComparer` (`IComparer`)                   | property | sort comparer           |
|  [05]   | `DataGridColumn.Header` / `HeaderTemplate`                          | property | header content          |
|  [06]   | `DataGridColumn.Width` (`DataGridLength`) / `MinWidth` / `MaxWidth` | property | sizing                  |
|  [07]   | `DataGridColumn.IsReadOnly` / `IsVisible` / `DisplayIndex`          | property | per-column policy       |
|  [08]   | `DataGridColumn.CanUserSort` / `CanUserResize` / `CanUserReorder`   | property | per-column gates        |
|  [09]   | `DataGridColumn.CellStyleClasses`                                   | property | conditional styling     |
|  [10]   | `DataGridTemplateColumn.CellTemplate` / `CellEditingTemplate`       | property | display + edit template |

[COLLECTION_VIEW_ENTRYPOINTS]: view projection operations on `DataGridCollectionView`

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]     |
| :-----: | :----------------------------------------------------------------------- | :------- | :--------------- |
|  [01]   | `Filter` (`Func<object,bool>`) / `CanFilter`                             | property | predicate filter |
|  [02]   | `SortDescriptions` (`DataGridSortDescriptionCollection`)                 | property | multi-key sort   |
|  [03]   | `GroupDescriptions`                                                      | property | grouping state   |
|  [04]   | `PageSize` / `PageIndex`                                                 | property | paging window    |
|  [05]   | `MoveToPage(int)`                                                        | instance | page move        |
|  [06]   | `MoveCurrentTo(object)` / `MoveCurrentToFirst()` / `MoveCurrentToNext()` | instance | current row      |
|  [07]   | `DeferRefresh()` (`IDisposable`)                                         | instance | batched refresh  |
|  [08]   | `AddNew()` / `CommitNew()` / `CancelNew()`                               | instance | row creation     |
|  [09]   | `EditItem(object)` / `CommitEdit()` / `CancelEdit()`                     | instance | row edit txn     |
|  [10]   | `Refresh()`                                                              | instance | full re-project  |
|  [11]   | `CollectionChanged` / `CurrentChanged`                                   | event    | view signals     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DataGrid` realizes `ItemsSource` lazily through `DataGridRowsPresenter`/`DataGridCellsPresenter`, recycling containers on `LoadingRow`/`UnloadingRow`; no `DataGridRow` exists for an off-screen item.
- Filter, sort, group, and page state lives on `DataGrid.CollectionView` — the internal `DataGridCollectionView` wrapping a plain `IEnumerable` source — never on the source collection.
- `CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true)` validates and persists a whole row; `BeginningEdit`/`CellEditEnding`/`RowEditEnding` veto through `e.Cancel`, and the `*Ended` events observe post-commit.
- Sorting routes through `DataGridColumn.SortMemberPath` and `CustomSortComparer`; the `Sorting` event intercepts to substitute a domain comparer or push the order into the backing query.
- `DataGridCollectionView.DeferRefresh()` returns an `IDisposable` scope collapsing a batch of `SortDescriptions`/`GroupDescriptions`/`Filter` mutations into one re-projection.

[STACKING]:
- `api-dynamicdata`(`.api/api-dynamicdata.md`): a `SourceCache<TRow,TKey>.Connect()` pipeline applies `Filter`/`SortAndBind` into a `ReadOnlyObservableCollection<TRow>` bound to `DataGrid.ItemsSource`; coarse set algebra stays in the DynamicData pipeline, interactive filter/sort/group/page on `DataGridCollectionView`.
- `api-reactiveui-avalonia`(`.api/api-reactiveui-avalonia.md`), `api-reactive`(`.api/api-reactive.md`): `SelectedItem`/`SelectedItems`/`CurrentColumn` and the edit-lifecycle events bind to a `ReactiveObject` through `WhenAnyValue`/`Bind`; sort and filter intents are `ReactiveCommand`s mutating `DataGridCollectionView`, never imperative UI state.
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`, shared tier): rows are `[ValueObject]`/`[SmartEnum]` records; `DataGridColumn.Binding` targets the typed members and `CustomSortComparer` consumes the value-object ordering, so no stringly-typed cell reaches the grid.
- `api-dock`(`.api/api-dock.md`), `api-avalonia-fluent`(`.api/api-avalonia-fluent.md`): `Dock.Avalonia` hosts each `DataGrid` as a dockable document or tool, and `api-avalonia-fluent` themes the column-header and cell chrome.
- within-lib: one `DataGridCollectionView.DeferRefresh()` scope wraps a multi-axis `SortDescriptions`/`GroupDescriptions`/`Filter` mutation, collapsing N re-projections into one.
- within-lib: every product tabular surface is one `DataGrid` over this rail; a new table is a new DynamicData-bound `ReadOnlyObservableCollection`, never a new control.

[LOCAL_ADMISSION]:
- A tabular surface in the AppUi shell is admitted only as a `DataGrid` bound to a DynamicData-projected `ReadOnlyObservableCollection`, its filter/sort/group/page state on `DataGridCollectionView` and its rows typed value objects.

[RAIL_LAW]:
- Package: `Avalonia.Controls.DataGrid`
- Owns: virtualized table projection, two-level editable rows, sortable/groupable/pageable/frozen columns, clipboard, and the `DataGridCollectionView` filter/sort/group/page/current-row engine.
- Accept: typed rows feed one grid through a DynamicData-bound `ReadOnlyObservableCollection`; interactive filter/sort/group/page on `DataGridCollectionView`; edit and selection through a ReactiveUI view model.
- Reject: an ad hoc table control beside the grid; filter/sort hidden in source-collection mutation; stringly-typed cells in place of value-object rows.
