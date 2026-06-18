# [RASM_APPUI_API_AVALONIA_GRID]

`Avalonia.Controls.DataGrid` supplies virtualized table, row, column, edit, selection, sort, grouping, and collection-view surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Controls.DataGrid`
- package: `Avalonia.Controls.DataGrid`
- assembly: `Avalonia.Controls.DataGrid`
- namespace: `Avalonia.Controls`
- namespace: `Avalonia.Controls.Primitives`
- namespace: `Avalonia.Collections`
- asset: runtime library
- rail: tables

## [2]-[PUBLIC_TYPES]

[GRID_CONTROLS]: table controls and containers
- rail: tables

| [INDEX] | [SYMBOL]                   | [RAIL]           |
| :-----: | :------------------------- | :--------------- |
|   [1]   | `DataGrid`                 | table root       |
|   [2]   | `DataGridRow`              | row container    |
|   [3]   | `DataGridCell`             | cell container   |
|   [4]   | `DataGridColumnHeader`     | header container |
|   [5]   | `DataGridRowHeader`        | row header       |
|   [6]   | `DataGridCellsPresenter`   | cell presenter   |
|   [7]   | `DataGridRowsPresenter`    | row presenter    |
|   [8]   | `DataGridDetailsPresenter` | detail presenter |

[COLUMN_TYPES]: column and edit model
- rail: tables

| [INDEX] | [SYMBOL]                  | [RAIL]           |
| :-----: | :------------------------ | :--------------- |
|   [1]   | `DataGridColumn`          | column base      |
|   [2]   | `DataGridBoundColumn`     | binding column   |
|   [3]   | `DataGridTextColumn`      | text column      |
|   [4]   | `DataGridCheckBoxColumn`  | boolean column   |
|   [5]   | `DataGridTemplateColumn`  | template column  |
|   [6]   | `DataGridLength`          | sizing value     |
|   [7]   | `DataGridLengthConverter` | sizing converter |

[GRID_EVENTS]: event argument surfaces
- rail: tables

| [INDEX] | [SYMBOL]                                | [RAIL]            |
| :-----: | :-------------------------------------- | :---------------- |
|   [1]   | `DataGridAutoGeneratingColumnEventArgs` | column generation |
|   [2]   | `DataGridBeginningEditEventArgs`        | edit start        |
|   [3]   | `DataGridCellEditEndingEventArgs`       | cell edit close   |
|   [4]   | `DataGridCellEditEndedEventArgs`        | cell edit result  |
|   [5]   | `DataGridColumnEventArgs`               | column event      |
|   [6]   | `DataGridRowEventArgs`                  | row event         |
|   [7]   | `DataGridRowEditEndingEventArgs`        | row edit close    |
|   [8]   | `DataGridRowDetailsEventArgs`           | row details       |

[COLLECTION_VIEW_TYPES]: tabular collection view
- rail: tables

| [INDEX] | [SYMBOL]                           | [RAIL]              |
| :-----: | :--------------------------------- | :------------------ |
|   [1]   | `IDataGridCollectionView`          | view contract       |
|   [2]   | `IDataGridCollectionViewFactory`   | view factory        |
|   [3]   | `DataGridCollectionView`           | view implementation |
|   [4]   | `DataGridSortDescription`          | sort descriptor     |
|   [5]   | `DataGridGroupDescription`         | group descriptor    |
|   [6]   | `DataGridCurrentChangingEventArgs` | current event       |
|   [7]   | `PageChangingEventArgs`            | page event          |

## [3]-[ENTRYPOINTS]

[GRID_ENTRYPOINTS]: table control operations
- rail: tables

| [INDEX] | [SURFACE]             | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :-------------------- | :------------- | :---------------- |
|   [1]   | `ItemsSource`         | `DataGrid`     | row source        |
|   [2]   | `Columns`             | `DataGrid`     | column model      |
|   [3]   | `SelectedItem`        | `DataGrid`     | single selection  |
|   [4]   | `SelectedItems`       | `DataGrid`     | multi selection   |
|   [5]   | `AutoGenerateColumns` | `DataGrid`     | column generation |
|   [6]   | `CollectionView`      | `DataGrid`     | view model        |
|   [7]   | `BeginEdit`           | `DataGrid`     | edit start        |
|   [8]   | `CommitEdit`          | `DataGrid`     | edit commit       |
|   [9]   | `CancelEdit`          | `DataGrid`     | edit cancel       |
|  [10]   | `ScrollIntoView`      | `DataGrid`     | row reveal        |

[GRID_EVENT_ENTRYPOINTS]: table event operations
- rail: tables

| [INDEX] | [SURFACE]              | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :--------------------- | :------------- | :------------------ |
|   [1]   | `AutoGeneratingColumn` | `DataGrid`     | column generation   |
|   [2]   | `LoadingRow`           | `DataGrid`     | row materialize     |
|   [3]   | `LoadingRowDetails`    | `DataGrid`     | details materialize |
|   [4]   | `LoadingRowGroup`      | `DataGrid`     | group materialize   |
|   [5]   | `Sorting`              | `DataGrid`     | sort request        |

[COLLECTION_VIEW_ENTRYPOINTS]: view operations
- rail: tables

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]           | [RAIL]        |
| :-----: | :------------------ | :----------------------- | :------------ |
|   [1]   | `Filter`            | `DataGridCollectionView` | filter state  |
|   [2]   | `SortDescriptions`  | `DataGridCollectionView` | sort state    |
|   [3]   | `GroupDescriptions` | `DataGridCollectionView` | group state   |
|   [4]   | `PageSize`          | `DataGridCollectionView` | page size     |
|   [5]   | `MoveCurrentTo`     | `DataGridCollectionView` | current row   |
|   [6]   | `DeferRefresh`      | `DataGridCollectionView` | refresh batch |
|   [7]   | `AddNew`            | `DataGridCollectionView` | row creation  |
|   [8]   | `EditItem`          | `DataGridCollectionView` | row edit      |
|   [9]   | `CommitEdit`        | `DataGridCollectionView` | edit commit   |
|  [10]   | `Refresh`           | `DataGridCollectionView` | view refresh  |

## [4]-[IMPLEMENTATION_LAW]

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
