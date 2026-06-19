# [RASM_APPUI_API_AVALONIA_GRID]

`Avalonia.Controls.DataGrid` supplies virtualized table, row, column, edit, selection, sort, grouping, and collection-view surfaces.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Controls.DataGrid`
- package: `Avalonia.Controls.DataGrid`
- assembly: `Avalonia.Controls.DataGrid`
- namespace: `Avalonia.Controls`
- namespace: `Avalonia.Controls.Primitives`
- namespace: `Avalonia.Collections`
- asset: runtime library
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

| [INDEX] | [SYMBOL]                  | [RAIL]           |
| :-----: | :------------------------ | :--------------- |
|  [01]   | `DataGridColumn`          | column base      |
|  [02]   | `DataGridBoundColumn`     | binding column   |
|  [03]   | `DataGridTextColumn`      | text column      |
|  [04]   | `DataGridCheckBoxColumn`  | boolean column   |
|  [05]   | `DataGridTemplateColumn`  | template column  |
|  [06]   | `DataGridLength`          | sizing value     |
|  [07]   | `DataGridLengthConverter` | sizing converter |

[GRID_EVENTS]: event argument surfaces
- rail: tables

| [INDEX] | [SYMBOL]                                | [RAIL]            |
| :-----: | :-------------------------------------- | :---------------- |
|  [01]   | `DataGridAutoGeneratingColumnEventArgs` | column generation |
|  [02]   | `DataGridBeginningEditEventArgs`        | edit start        |
|  [03]   | `DataGridCellEditEndingEventArgs`       | cell edit close   |
|  [04]   | `DataGridCellEditEndedEventArgs`        | cell edit result  |
|  [05]   | `DataGridColumnEventArgs`               | column event      |
|  [06]   | `DataGridRowEventArgs`                  | row event         |
|  [07]   | `DataGridRowEditEndingEventArgs`        | row edit close    |
|  [08]   | `DataGridRowDetailsEventArgs`           | row details       |

[COLLECTION_VIEW_TYPES]: tabular collection view
- rail: tables

| [INDEX] | [SYMBOL]                           | [RAIL]              |
| :-----: | :--------------------------------- | :------------------ |
|  [01]   | `IDataGridCollectionView`          | view contract       |
|  [02]   | `IDataGridCollectionViewFactory`   | view factory        |
|  [03]   | `DataGridCollectionView`           | view implementation |
|  [04]   | `DataGridSortDescription`          | sort descriptor     |
|  [05]   | `DataGridGroupDescription`         | group descriptor    |
|  [06]   | `DataGridCurrentChangingEventArgs` | current event       |
|  [07]   | `PageChangingEventArgs`            | page event          |

## [03]-[ENTRYPOINTS]

[GRID_ENTRYPOINTS]: table control operations
- rail: tables

| [INDEX] | [SURFACE]             | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :-------------------- | :------------- | :---------------- |
|  [01]   | `ItemsSource`         | `DataGrid`     | row source        |
|  [02]   | `Columns`             | `DataGrid`     | column model      |
|  [03]   | `SelectedItem`        | `DataGrid`     | single selection  |
|  [04]   | `SelectedItems`       | `DataGrid`     | multi selection   |
|  [05]   | `AutoGenerateColumns` | `DataGrid`     | column generation |
|  [06]   | `CollectionView`      | `DataGrid`     | view model        |
|  [07]   | `BeginEdit`           | `DataGrid`     | edit start        |
|  [08]   | `CommitEdit`          | `DataGrid`     | edit commit       |
|  [09]   | `CancelEdit`          | `DataGrid`     | edit cancel       |
|  [10]   | `ScrollIntoView`      | `DataGrid`     | row reveal        |

[GRID_EVENT_ENTRYPOINTS]: table event operations
- rail: tables

| [INDEX] | [SURFACE]              | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :--------------------- | :------------- | :------------------ |
|  [01]   | `AutoGeneratingColumn` | `DataGrid`     | column generation   |
|  [02]   | `LoadingRow`           | `DataGrid`     | row materialize     |
|  [03]   | `LoadingRowDetails`    | `DataGrid`     | details materialize |
|  [04]   | `LoadingRowGroup`      | `DataGrid`     | group materialize   |
|  [05]   | `Sorting`              | `DataGrid`     | sort request        |

[COLLECTION_VIEW_ENTRYPOINTS]: view operations
- rail: tables

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]           | [RAIL]        |
| :-----: | :------------------ | :----------------------- | :------------ |
|  [01]   | `Filter`            | `DataGridCollectionView` | filter state  |
|  [02]   | `SortDescriptions`  | `DataGridCollectionView` | sort state    |
|  [03]   | `GroupDescriptions` | `DataGridCollectionView` | group state   |
|  [04]   | `PageSize`          | `DataGridCollectionView` | page size     |
|  [05]   | `MoveCurrentTo`     | `DataGridCollectionView` | current row   |
|  [06]   | `DeferRefresh`      | `DataGridCollectionView` | refresh batch |
|  [07]   | `AddNew`            | `DataGridCollectionView` | row creation  |
|  [08]   | `EditItem`          | `DataGridCollectionView` | row edit      |
|  [09]   | `CommitEdit`        | `DataGridCollectionView` | edit commit   |
|  [10]   | `Refresh`           | `DataGridCollectionView` | view refresh  |

## [04]-[IMPLEMENTATION_LAW]

[TABLE_LAW]:
- Package: `Avalonia.Controls.DataGrid`
- Owns: virtualized table projection, editable rows, sortable columns, and collection-view state
- Accept: typed rows feed grids through one tabular projection rail
- Reject: ad hoc table controls

[VIEW_LAW]:
- Package: `Avalonia.Controls.DataGrid`
- Owns: tabular filter, sort, group, page, current-row, add, edit, and refresh mechanics
- Accept: table state remains explicit and observable across panels, sidecars, diagnostics, and support views
- Reject: hidden list mutation as a table model
