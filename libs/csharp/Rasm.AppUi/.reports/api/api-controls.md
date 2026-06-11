# [RASM_APPUI_API_CONTROLS]

Controls APIs supply AppUi inspectors, property editors, dialogs, prompts, toasts, event triggers, key bindings, drag-drop, and behavior composition.

## [1]-[SURFACES]

This table is a lookup by controls package.

| [INDEX] | [PACKAGE]                    | [ASSEMBLY]                    | [LOCAL_RAIL] |
| :-----: | :--------------------------- | :---------------------------- | :----------- |
|   [1]   | `bodong.Avalonia.PropertyGrid` | `Avalonia.PropertyGrid`     | inspector    |
|   [2]   | `DialogHost.Avalonia`        | `DialogHost.Avalonia`         | dialog       |
|   [3]   | `Xaml.Behaviors.Avalonia`    | `Xaml.Behaviors.Avalonia`     | behavior     |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                | [NAMESPACE]                 | [USING]                    | [API_LOCATOR] |
| :-----: | :------------------------ | :-------------------------- | :------------------------- | :------------ |
|   [1]   | `Avalonia.PropertyGrid`   | `Avalonia.Controls`         | `Avalonia.Controls`        | `.cache/nuget/packages/bodong.avalonia.propertygrid/` |
|   [2]   | `DialogHost.Avalonia`     | `DialogHostAvalonia`        | `DialogHostAvalonia`       | `.cache/nuget/packages/dialoghost.avalonia/` |
|   [3]   | `Xaml.Behaviors.Avalonia` | `Avalonia.Xaml.Interactivity` | `Avalonia.Xaml.Interactivity` | `.cache/nuget/packages/xaml.behaviors.avalonia/` |
|   [4]   | `Xaml.Behaviors.Avalonia` | `Avalonia.Xaml.Interactions.Core` | `Avalonia.Xaml.Interactions.Core` | `.cache/nuget/packages/xaml.behaviors.avalonia/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]       | [ENTRY_SURFACE]             | [LOCAL_RAIL] |
| :-----: | :------------------ | :-------------------------- | :----------- |
|   [1]   | property grid controls | typed object inspection  | inspector    |
|   [2]   | editor slot APIs    | property editor integration | inspector    |
|   [3]   | dialog host APIs    | in-panel modal surface      | dialog       |
|   [4]   | toast/prompt APIs   | transient notification      | dialog       |
|   [5]   | behavior base types | event trigger composition   | behavior     |
|   [6]   | command triggers    | key binding and drag-drop   | behavior     |

## [4]-[REJECTED]

This table is a lookup by rejected package.

| [INDEX] | [REJECT]                   | [LOCAL_RAIL] | [REASON]                    |
| :-----: | :------------------------- | :----------- | :-------------------------- |
|   [1]   | `MessageBox.Avalonia`      | dialog       | independent window dialogs  |
|   [2]   | `Avalonia.Xaml.Interactions` | behavior    | replaced by behavior package |
|   [3]   | ad hoc property editors    | inspector    | inspector rail owns editors |
