# [RASM_APPUI_API_AVALONIA_GRID]

`Avalonia.Controls.DataGrid` owns the AppUi tabular surface: a virtualized `TemplatedControl` over `ItemsSource` with two-level editable rows and sortable, groupable, pageable, frozen columns, paired with the `DataGridCollectionView` engine that folds filter, sort, group, page, and current-row state over `Avalonia.Collections`. Typed rows reach it as one DynamicData-projected `ReadOnlyObservableCollection` bound into `ItemsSource`, and selection and edit state bind through a ReactiveUI view model. This is the single tabular boundary; no parallel hand-rolled list control exists.

## [01]-[PUBLIC_TYPES]

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

## [02]-[ENTRYPOINTS]

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
|  [08]   | `SelectionChanged` / `CurrentCellChanged` / `CellPointerPressed`            | event   | selection signal    |
|  [09]   | `LoadingRowGroup` / `UnloadingRowGroup` (`DataGridRowGroupHeaderEventArgs`) | event   | group-header mount  |
|  [10]   | `ColumnDisplayIndexChanged` / `ColumnReordering` / `ColumnReordered`        | event   | column-order signal |
|  [11]   | `CopyingRowClipboardContent` (`DataGridRowClipboardEventArgs`)              | event   | copy-row intercept  |

[COLUMN_ENTRYPOINTS]: per-column model, owner qualified

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]           |
| :-----: | :------------------------------------------------------------------ | :------- | :--------------------- |
|  [01]   | `DataGridBoundColumn.Binding` (`BindingBase`)                       | property | value path             |
|  [02]   | `DataGridBoundColumn.ClipboardContentBinding`                       | property | copy path override     |
|  [03]   | `DataGridColumn.SortMemberPath`                                     | property | sort key path          |
|  [04]   | `DataGridColumn.CustomSortComparer` (`IComparer`)                   | property | sort comparer          |
|  [05]   | `DataGridColumn.Header` / `HeaderTemplate`                          | property | header content         |
|  [06]   | `DataGridColumn.Width` (`DataGridLength`) / `MinWidth` / `MaxWidth` | property | sizing                 |
|  [07]   | `DataGridColumn.ActualWidth` (`double`)                             | property | measured extent        |
|  [08]   | `DataGridColumn.IsReadOnly` / `IsVisible` / `DisplayIndex`          | property | per-column policy      |
|  [09]   | `DataGridColumn.CanUserSort` / `CanUserResize` / `CanUserReorder`   | property | per-column gates       |
|  [10]   | `DataGridColumn.CellStyleClasses` (`Classes`)                       | property | per-column style class |
|  [11]   | `DataGridColumn.CellTheme` (`ControlTheme`)                         | property | per-column cell theme  |
|  [12]   | `DataGridColumn.Sort()` / `Sort(ListSortDirection)` / `ClearSort()` | instance | programmatic sort      |
|  [13]   | `DataGridTemplateColumn.CellTemplate` / `CellEditingTemplate`       | property | `IDataTemplate` pair   |

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
|  [11]   | `Culture` (`CultureInfo`)                                                | property | sort culture     |
|  [12]   | `CollectionChanged` / `CurrentChanged`                                   | event    | view signals     |

[SORT_DESCRIPTION_ENTRYPOINTS]: `DataGridSortDescription` construction and read-back, owner qualified

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]           |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `FromPath(string, ListSortDirection, CultureInfo?)`            | static   | path description       |
|  [02]   | `FromPath(string, ListSortDirection, IComparer)`               | static   | path plus comparer     |
|  [03]   | `FromComparer(IComparer, ListSortDirection)`                   | static   | comparer description   |
|  [04]   | `PropertyPath` / `HasPropertyPath` / `Direction`               | property | description read-back  |
|  [05]   | `SwitchSortDirection()` / `OrderBy` / `ThenBy`                 | instance | toggle and application |
|  [06]   | `DataGridComparerSortDescription.SourceComparer` (`IComparer`) | property | comparer identity      |
|  [07]   | `DataGridPathSortDescription.Comparer` (`IComparer<object>`)   | property | resolved comparer      |

[VALIDATION_ENTRYPOINTS]: cell/row validity, owner qualified

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------ | :------- | :---------------------- |
|  [01]   | `DataGrid.IsValid` / `DataGridRow.IsValid` / `DataGridCell.IsValid` | property | read-only; internal set |
|  [02]   | `DataGridCellEditEndingEventArgs.EditingElement` (`Control`)        | property | validation write target |
|  [03]   | `DataGridPreparingCellForEditEventArgs.EditingElement`              | property | editor mount target     |
|  [04]   | `DataGridCollectionViewGroup.Key` / `ItemCount` / `Items`           | property | group identity          |
|  [05]   | `DataGridRowGroupHeader.PropertyName` / `IsItemCountVisible`        | property | header chrome           |
|  [06]   | `DataGridRowClipboardEventArgs.ClipboardRowContent` / `Item`        | property | copy-row payload        |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DataGrid` realizes `ItemsSource` lazily through `DataGridRowsPresenter`/`DataGridCellsPresenter`, recycling containers on `LoadingRow`/`UnloadingRow`; no `DataGridRow` exists for an off-screen item.
- Filter, sort, group, and page state lives on `DataGrid.CollectionView` — the internal `DataGridCollectionView` wrapping a plain `IEnumerable` source — never on the source collection.
- `CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true)` validates and persists a whole row; `BeginningEdit`/`CellEditEnding`/`RowEditEnding` veto through `e.Cancel`, and the `*Ended` events observe post-commit.
- Sorting routes through `DataGridColumn.SortMemberPath` and `CustomSortComparer`; the `Sorting` event intercepts through `e.Handled`, which vetoes the whole built-in gesture and is the hook for pushing the order into a backing query.
- The header gesture owns MULTI-SORT: a plain click clears `SortDescriptions` and toggles that column alone, `Shift`-click appends or toggles in place (click order becomes key order), `Ctrl`/`Cmd`-click clears every description, `Shift`+`Ctrl`/`Cmd` is a no-op, and the whole gesture is refused while `EditingRow` is non-null. `DataGridColumn.GetSortDescription` matches a comparer-bearing column by `SourceComparer` alone and a path-bearing column by `PropertyPath`, so a column carrying BOTH `CustomSortComparer` and a path-bearing description gains a SECOND sort entry on the next click.
- Cell and row validity are read-only from outside: `DataGridCell.IsValid`/`DataGridRow.IsValid` carry internal setters and only `EndCellEdit` writes them, reading `DataValidationErrors.GetHasErrors(editingElement)` when `CurrentColumn.CellEditBinding` is non-null — so `:invalid` is reachable on BOUND columns alone, and `DataValidationErrors.SetErrors` written inside `CellEditEnding` (which raises before that read) both refuses the cell commit and stamps the pseudo-class. A `DataGridTemplateColumn` generates no `CellEditBinding` and therefore cannot reach the cell-level gate.
- The built-in `Ctrl+C` copy walks `ColumnsInternal.GetVisibleColumns()`, so an `IsVisible=false` column contributes nothing to the clipboard payload; there is no paste path on the control at all.
- `DataGrid` recycles ROW containers and never columns: `Columns` is a model collection re-materialized wholesale, so each column costs one header plus one realized cell control per visible row for the surface lifetime.
- `LoadingRowGroup` hands a `DataGridRowGroupHeader` whose `DataContext` the control has already set to the header's `DataGridCollectionViewGroup`.
- `DataGridCollectionView.DeferRefresh()` returns an `IDisposable` scope collapsing a batch of `SortDescriptions`/`GroupDescriptions`/`Filter` mutations into one re-projection.

[STACKING]:
- `api-dynamicdata`(`.api/api-dynamicdata.md`): a `SourceCache<TRow,TKey>.Connect()` pipeline applies `Filter`/`SortAndBind` into a `ReadOnlyObservableCollection<TRow>` bound to `DataGrid.ItemsSource`; coarse set algebra stays in the DynamicData pipeline, interactive filter/sort/group/page on `DataGridCollectionView`.
- `api-reactiveui-avalonia`(`.api/api-reactiveui-avalonia.md`), `api-reactive`(`.api/api-reactive.md`): `SelectedItem`/`SelectedItems`/`CurrentColumn` and the edit-lifecycle events bind to a `ReactiveObject` through `WhenAnyValue`/`Bind`; sort and filter intents are `ReactiveCommand`s mutating `DataGridCollectionView`, never imperative UI state.
- `api-thinktecture-runtime-extensions`(`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`, shared tier): rows are `[ValueObject]`/`[SmartEnum]` records; `DataGridColumn.Binding` targets the typed members and `CustomSortComparer` consumes the value-object ordering, so no stringly-typed cell reaches the grid.
- `api-dock`(`.api/api-dock.md`), `api-avalonia-fluent`(`.api/api-avalonia-fluent.md`): `Dock.Avalonia` hosts each `DataGrid` as a dockable document or tool, and `api-avalonia-fluent` themes the column-header and cell chrome.
- within-lib: one `DataGridCollectionView.DeferRefresh()` scope wraps a multi-axis `SortDescriptions`/`GroupDescriptions`/`Filter` mutation, collapsing N re-projections into one.
- within-lib: every product tabular surface is one `DataGrid` over this control; a new table is a new DynamicData-bound `ReadOnlyObservableCollection`, never a new control.

[LOCAL_ADMISSION]:
- A tabular surface in the AppUi shell is admitted only as a `DataGrid` bound to a DynamicData-projected `ReadOnlyObservableCollection`, its filter/sort/group/page state on `DataGridCollectionView` and its rows typed value objects.
