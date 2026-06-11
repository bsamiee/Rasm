# [RASM_APPUI_API_AVALONIA_GRID]

`Avalonia.Controls.DataGrid` supplies virtualized table, row, column, edit, selection, and sort surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Controls.DataGrid`
- package: `Avalonia.Controls.DataGrid`
- assembly: `Avalonia.Controls.DataGrid`
- namespace: `Avalonia.Controls`
- asset: runtime library
- rail: tables

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: grid family
- rail: tables

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]  | [CAPABILITY]          |
| :-----: | :----------------------- | :-------------- | :-------------------- |
|   [1]   | `DataGrid`               | table control   | renders tabular state |
|   [2]   | `DataGridColumn`         | column base     | owns column contract  |
|   [3]   | `DataGridTextColumn`     | text column     | edits text values     |
|   [4]   | `DataGridTemplateColumn` | template column | hosts custom cells    |
|   [5]   | `DataGridCheckBoxColumn` | boolean column  | edits boolean values  |
|   [6]   | `DataGridBoundColumn`    | bound column    | owns binding contract |
|   [7]   | `DataGridRow`            | row container   | owns row visuals      |
|   [8]   | `DataGridCell`           | cell container  | owns cell visuals     |
|   [9]   | `DataGridSelectionMode`  | selection mode  | controls selection    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grid operations
- rail: tables

| [INDEX] | [SURFACE]      | [CALL_SHAPE]     | [CAPABILITY]           |
| :-----: | :------------- | :--------------- | :--------------------- |
|   [1]   | `ItemsSource`  | property surface | binds surface state    |
|   [2]   | `Columns`      | property surface | binds surface state    |
|   [3]   | `SelectedItem` | property surface | binds surface state    |
|   [4]   | `BeginEdit`    | edit command     | opens cell edit        |
|   [5]   | `CommitEdit`   | commit command   | persists edit          |
|   [6]   | `CancelEdit`   | cancel command   | reverts edit           |
|   [7]   | `Sorting`      | sort event       | orders row view        |
|   [8]   | `LoadingRow`   | row event        | prepares row container |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Avalonia.Controls.DataGrid`
- Owns: table projection surface
- Accept: typed rows feed virtualized grids
- Reject: ad hoc table controls
