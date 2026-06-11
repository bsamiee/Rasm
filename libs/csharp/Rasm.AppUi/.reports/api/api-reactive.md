# [RASM_APPUI_API_REACTIVE]

Reactive APIs supply AppUi activation, commands, validation, observable contracts, live collection folding, and scheduler-bound read-only projections.

## [1]-[SURFACES]

This table is a lookup by reactive package.

| [INDEX] | [PACKAGE]                 | [ASSEMBLY]                 | [LOCAL_RAIL] |
| :-----: | :------------------------ | :------------------------- | :----------- |
|   [1]   | `ReactiveUI`              | `ReactiveUI`               | screen       |
|   [2]   | `ReactiveUI.Avalonia`     | `ReactiveUI.Avalonia`      | screen       |
|   [3]   | `ReactiveUI.Validation`   | `ReactiveUI.Validation`    | validation   |
|   [4]   | `System.Reactive`         | `System.Reactive`          | live         |
|   [5]   | `DynamicData`             | `DynamicData`              | live         |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]              | [NAMESPACE]              | [USING]                   | [API_LOCATOR] |
| :-----: | :---------------------- | :----------------------- | :------------------------ | :------------ |
|   [1]   | `ReactiveUI`            | `ReactiveUI`             | `ReactiveUI`              | `.cache/nuget/packages/reactiveui/` |
|   [2]   | `ReactiveUI.Avalonia`   | `ReactiveUI`             | `ReactiveUI`              | `.cache/nuget/packages/reactiveui.avalonia/` |
|   [3]   | `ReactiveUI.Validation` | `ReactiveUI.Validation`  | `ReactiveUI.Validation`   | `.cache/nuget/packages/reactiveui.validation/` |
|   [4]   | `System.Reactive`       | `System.Reactive`        | `System.Reactive`         | `.cache/nuget/packages/system.reactive/` |
|   [5]   | `DynamicData`           | `DynamicData`            | `DynamicData`             | `.cache/nuget/packages/dynamicdata/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]           | [ENTRY_SURFACE]              | [LOCAL_RAIL] |
| :-----: | :---------------------- | :--------------------------- | :----------- |
|   [1]   | `ReactiveObject`        | observable screen state      | screen       |
|   [2]   | `ReactiveCommand`       | command execution rail       | command      |
|   [3]   | activation interfaces   | screen lifetime              | screen       |
|   [4]   | validation helpers      | screen validation            | validation   |
|   [5]   | `IObservable<T>`        | live projection contract     | live         |
|   [6]   | scheduler primitives    | UI observation boundary      | scheduler    |
|   [7]   | `SourceCache<T,K>`      | keyed collection folding     | live         |
|   [8]   | `ReadOnlyObservableCollection<T>` | read-only view model | live         |

## [4]-[REJECTED]

This table is a lookup by rejected API.

| [INDEX] | [REJECT]             | [LOCAL_RAIL] | [REASON]              |
| :-----: | :------------------- | :----------- | :-------------------- |
|   [1]   | alternate MVVM rail  | screen       | ReactiveUI owns rail  |
|   [2]   | public DynamicData exposure | live | AppUi exposes snapshots |
