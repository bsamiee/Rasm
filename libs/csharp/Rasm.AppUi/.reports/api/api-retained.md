# [RASM_APPUI_API_RETAINED]

Retained UI APIs supply AppUi shell, platform, controls, theme, font, and retained surface primitives.

## [1]-[SURFACES]

This table is a lookup by retained UI package.

| [INDEX] | [PACKAGE]                         | [ASSEMBLY]                         | [LOCAL_RAIL] |
| :-----: | :-------------------------------- | :--------------------------------- | :----------- |
|   [1]   | `Avalonia`                        | `Avalonia.Base`                    | shell        |
|   [2]   | `Avalonia.Desktop`                | `Avalonia.Desktop`                 | platform     |
|   [3]   | `Avalonia.Themes.Fluent`          | `Avalonia.Themes.Fluent`           | theme        |
|   [4]   | `Avalonia.Fonts.Inter`            | `Avalonia.Fonts.Inter`             | typography   |
|   [5]   | `Avalonia.Controls.DataGrid`      | `Avalonia.Controls.DataGrid`       | table        |
|   [6]   | `Avalonia.Controls.ColorPicker`   | `Avalonia.Controls.ColorPicker`    | editor       |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                    | [NAMESPACE]                  | [USING]                       | [API_LOCATOR] |
| :-----: | :---------------------------- | :--------------------------- | :---------------------------- | :------------ |
|   [1]   | `Avalonia.Base`               | `Avalonia`                   | `Avalonia`                    | `.cache/nuget/packages/avalonia/` |
|   [2]   | `Avalonia.Controls`           | `Avalonia.Controls`          | `Avalonia.Controls`           | `.cache/nuget/packages/avalonia/` |
|   [3]   | `Avalonia.Desktop`            | `Avalonia`                   | `Avalonia`                    | `.cache/nuget/packages/avalonia.desktop/` |
|   [4]   | `Avalonia.Themes.Fluent`      | `Avalonia.Themes.Fluent`     | `Avalonia.Themes.Fluent`      | `.cache/nuget/packages/avalonia.themes.fluent/` |
|   [5]   | `Avalonia.Fonts.Inter`        | `Avalonia.Media`             | `Avalonia.Media`              | `.cache/nuget/packages/avalonia.fonts.inter/` |
|   [6]   | `Avalonia.Controls.DataGrid`  | `Avalonia.Controls`          | `Avalonia.Controls`           | `.cache/nuget/packages/avalonia.controls.datagrid/` |
|   [7]   | `Avalonia.Controls.ColorPicker` | `Avalonia.Controls`        | `Avalonia.Controls`           | `.cache/nuget/packages/avalonia.controls.colorpicker/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]                | [ENTRY_SURFACE]              | [LOCAL_RAIL] |
| :-----: | :--------------------------- | :--------------------------- | :----------- |
|   [1]   | `Application`                | platform lifetime            | platform     |
|   [2]   | `TopLevel`                   | retained surface root        | shell        |
|   [3]   | `MacOSPlatformOptions`       | app delegate policy          | platform     |
|   [4]   | `Dispatcher.UIThread`        | scheduler capture            | scheduler    |
|   [5]   | `ResourceDictionary`         | theme and asset resources    | theme        |
|   [6]   | `DataGrid`                   | flat table adapter           | table        |
|   [7]   | color picker controls        | palette editing              | editor       |
|   [8]   | Fluent theme resources       | base control templates       | theme        |
|   [9]   | Inter font resources         | default UI font collection   | typography   |

## [4]-[REJECTED]

This table is a lookup by rejected package.

| [INDEX] | [REJECT]                       | [LOCAL_RAIL] | [REASON]                  |
| :-----: | :----------------------------- | :----------- | :------------------------ |
|   [1]   | `FluentAvaloniaUI`             | theme        | parallel control suite    |
|   [2]   | `Material.Avalonia`            | theme        | theme takeover            |
|   [3]   | `Avalonia.Controls.TreeDataGrid` | table       | paid/pro path             |
|   [4]   | `AvaloniaUI.Licensing`         | table        | license-key package       |
