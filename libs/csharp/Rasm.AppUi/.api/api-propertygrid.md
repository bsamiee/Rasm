# [RASM_APPUI_API_PROPERTYGRID]

`bodong.Avalonia.PropertyGrid` supplies the Avalonia property inspector control, editor factories, events, and localization services. `bodong.PropertyModels` supplies the platform-neutral domain model layer: reactive base objects, command/undo infrastructure, checked/selectable collections, localization contracts, data annotation attributes, and validation extensions consumed by the grid and by application view models.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bodong.PropertyModels`
- package: `bodong.PropertyModels`
- assembly: `PropertyModels`
- namespace: `PropertyModels.Collections`
- namespace: `PropertyModels.ComponentModel`
- namespace: `PropertyModels.ComponentModel.DataAnnotations`
- namespace: `PropertyModels.Extensions`
- namespace: `PropertyModels.Localization`
- namespace: `PropertyModels.Utils`
- asset: runtime library (net8.0 / net9.0 / net10.0)
- rail: inspectors

[PACKAGE_SURFACE]: `bodong.Avalonia.PropertyGrid`
- package: `bodong.Avalonia.PropertyGrid`
- assembly: `Avalonia.PropertyGrid`
- namespace: `Avalonia.PropertyGrid`
- namespace: `Avalonia.PropertyGrid.Controls`
- namespace: `Avalonia.PropertyGrid.Controls.Factories`
- namespace: `Avalonia.PropertyGrid.Services`
- namespace: `Avalonia.PropertyGrid.ViewModels`
- asset: runtime library
- rail: inspectors

## [2]-[PUBLIC_TYPES]

[REACTIVE_TYPES]: reactive base objects — `PropertyModels.ComponentModel`
- rail: inspectors

| [INDEX] | [SYMBOL]                  | [KIND]               | [RAIL]                             |
| :-----: | :------------------------ | :------------------- | :--------------------------------- |
|   [1]   | `IReactiveObject`         | contract             | `PropertyChanged` + batch + raise  |
|   [2]   | `MiniReactiveObject`      | base class           | `IReactiveObject` without dependency tracking |
|   [3]   | `ReactiveObject`          | base class           | `MiniReactiveObject` + `[DependsOnProperty]` propagation |
|   [4]   | `ReactiveObjectExtensions`| static extensions    | `RaisePropertyChanged` helpers     |

[COMMAND_TYPES]: command and undo infrastructure — `PropertyModels.ComponentModel`
- rail: inspectors

| [INDEX] | [SYMBOL]                     | [KIND]            | [RAIL]                            |
| :-----: | :--------------------------- | :---------------- | :-------------------------------- |
|   [1]   | `IBaseCommand`               | contract          | `Name`, `CanExecute`, `Execute`   |
|   [2]   | `ICancelableCommand`         | contract          | cancelable command contract       |
|   [3]   | `AbstractBaseCommand`        | abstract base     | `IBaseCommand` base               |
|   [4]   | `AbstractCancelableCommand`  | abstract base     | `ICancelableCommand` base         |
|   [5]   | `GenericCommand`             | concrete command  | delegate-backed command           |
|   [6]   | `GenericCancelableCommand`   | concrete command  | delegate-backed cancelable        |
|   [7]   | `ReactiveCommand`            | reactive command  | `INotifyCommandExecuting` command |
|   [8]   | `CancelableCommandRecorder`  | undo recorder     | undo/redo queue, `MaxCommand=20`  |
|   [9]   | `CommandHistoryViewModel`    | undo view model   | `CanUndo`, `CanRedo`, queue views |

[COLLECTION_TYPES]: checked and selectable list collections — `PropertyModels.Collections`
- rail: inspectors

| [INDEX] | [SYMBOL]           | [KIND]            | [RAIL]                               |
| :-----: | :----------------- | :---------------- | :----------------------------------- |
|   [1]   | `ICheckedList`     | contract          | checked selection over `ICollection` |
|   [2]   | `CheckedList`      | concrete list     | typed checked multi-select list      |
|   [3]   | `ISelectableList`  | contract          | single/multi selectable list         |
|   [4]   | `SelectableList`   | concrete list     | typed selectable list                |

[ANNOTATION_TYPES]: data annotation attributes — `PropertyModels.ComponentModel.DataAnnotations`
- rail: inspectors

| [INDEX] | [SYMBOL]                              | [RAIL]                              |
| :-----: | :------------------------------------ | :---------------------------------- |
|   [1]   | `ConditionTargetAttribute`            | marks property as condition target  |
|   [2]   | `PropertyVisibilityConditionAttribute`| conditional property visibility     |
|   [3]   | `DependsOnPropertyAttribute`          | reactive dependency declaration     |
|   [4]   | `PathBrowsableAttribute`              | path browse mode                    |
|   [5]   | `EnumPermitNamesAttribute`            | enum filter by name                 |
|   [6]   | `EnumPermitValuesAttribute`           | enum filter by value                |
|   [7]   | `EnumProhibitNamesAttribute`          | enum exclusion by name              |
|   [8]   | `EnumProhibitValuesAttribute`         | enum exclusion by value             |
|   [9]   | `FloatPrecisionAttribute`             | float display precision             |
|  [10]   | `IntegerIncrementAttribute`           | integer editor increment            |
|  [11]   | `ProgressAttribute`                   | progress bar editor hint            |
|  [12]   | `WatermarkAttribute`                  | editor watermark text               |
|  [13]   | `TrackableAttribute`                  | change tracking marker              |
|  [14]   | `MultilineTextAttribute`              | multiline string editor             |
|  [15]   | `UnitAttribute`                       | unit label display                  |

[LOCALIZATION_TYPES]: localization contracts — `PropertyModels.Localization`
- rail: inspectors

| [INDEX] | [SYMBOL]                  | [KIND]           | [RAIL]                               |
| :-----: | :------------------------ | :--------------- | :----------------------------------- |
|   [1]   | `ILocalizationService`    | contract         | key lookup, culture change, extras   |
|   [2]   | `ICultureData`            | contract         | culture metadata                     |
|   [3]   | `AbstractCultureData`     | abstract base    | `ICultureData` base                  |

[INSPECTOR_TYPES]: grid, context, and view model surfaces — `bodong.Avalonia.PropertyGrid` — rail: inspectors

| [INDEX] | [SYMBOL]                     | [KIND]             |
| :-----: | :--------------------------- | :----------------- |
|   [1]   | `PropertyGrid`               | inspector control  |
|   [2]   | `IPropertyGrid`              | inspector contract |
|   [3]   | `PropertyGridViewModel`      | inspector model    |
|   [4]   | `PropertyCellContext`        | cell context       |
|   [5]   | `IPropertyGridCellInfo`      | cell info          |
|   [6]   | `IPropertyGridFilterContext` | filter context     |
|   [7]   | `FilterCategory`             | filter value       |
|   [8]   | `ReferencePath`              | property path      |

[EDITOR_TYPES]: editor controls and list models — rail: inspectors

| [INDEX] | [SYMBOL]                    | [KIND]         |
| :-----: | :-------------------------- | :------------- |
|   [1]   | `ButtonEdit`                | button editor  |
|   [2]   | `ListEdit`                  | list editor    |
|   [3]   | `CheckedListEdit`           | checked list   |
|   [4]   | `RadioButtonListEdit`       | radio list     |
|   [5]   | `ToggleButtonGroupListEdit` | toggle list    |
|   [6]   | `PreviewableColorPicker`    | color editor   |
|   [7]   | `PreviewableSlider`         | numeric editor |
|   [8]   | `TrackableEdit`             | tracked editor |
|   [9]   | `ListViewModel`             | list model     |
|  [10]   | `SingleSelectListViewModel` | select model   |

[FACTORY_TYPES]: editor factory families — rail: inspectors

| [INDEX] | [SYMBOL]                    | [KIND]            |
| :-----: | :-------------------------- | :---------------- |
|   [1]   | `ICellEditFactory`          | factory contract  |
|   [2]   | `AbstractCellEditFactory`   | factory base      |
|   [3]   | `CellEditFactoryCollection` | factory set       |
|   [4]   | `BooleanCellEditFactory`    | boolean editor    |
|   [5]   | `ColorCellEditFactory`      | color editor      |
|   [6]   | `CollectionCellEditFactory` | collection editor |
|   [7]   | `EnumCellEditFactory`       | enum editor       |
|   [8]   | `NumericCellEditFactory`    | numeric editor    |
|   [9]   | `StringCellEditFactory`     | string editor     |
|  [10]   | `PathCellEditFactory`       | path editor       |
|  [11]   | `ExpandableCellEditFactory` | nested object     |

[EVENT_AND_SERVICE_TYPES]: events, localization, and services — rail: inspectors

| [INDEX] | [SYMBOL]                                  | [KIND]               |
| :-----: | :---------------------------------------- | :------------------- |
|   [1]   | `CellPropertyChangedEventArgs`            | cell change          |
|   [2]   | `CustomPropertyDescriptorFilterEventArgs` | descriptor filter    |
|   [3]   | `CustomPropertyOperationControlEventArgs` | operation control    |
|   [4]   | `CustomPropertyDefaultOperationEventArgs` | operation default    |
|   [5]   | `PropertyGotFocusEventArgs`               | focus event          |
|   [6]   | `PropertyLostFocusEventArgs`              | focus event          |
|   [7]   | `CellEditFactoryService`                  | factory service      |
|   [8]   | `LocalizationService`                     | localization service |
|   [9]   | `AssemblyJsonAssetLocalizationService`    | asset localization   |

## [3]-[ENTRYPOINTS]

[REACTIVE_ENTRYPOINTS]: `PropertyModels.ComponentModel` reactive surfaces
- rail: inspectors

| [INDEX] | [SURFACE]                                   | [SURFACE_ROOT]       | [RAIL]                   |
| :-----: | :------------------------------------------ | :------------------- | :----------------------- |
|   [1]   | `RaisePropertyChanged(string)`              | `MiniReactiveObject` | property change notify   |
|   [2]   | `SetProperty<T>(ref T, T, string?)`         | `MiniReactiveObject` | set + notify helper      |
|   [3]   | `BeginBatchUpdate()` / `EndBatchUpdate()`   | `MiniReactiveObject` | suppress intermediate notifications |
|   [4]   | `CanExecute()` / `Execute()`                | `IBaseCommand`       | command gate and execute |
|   [5]   | `CanUndo` / `CanRedo`                       | `CancelableCommandRecorder` | undo/redo state   |
|   [6]   | `GetUndoQueue()` / `GetRedoQueue()`         | `CancelableCommandRecorder` | queue snapshots   |
|   [7]   | `UndoCommand` / `RedoCommand` / `ClearCommand` | `CommandHistoryViewModel` | bindable commands |

[COLLECTION_ENTRYPOINTS]: checked list operations — `PropertyModels.Collections`
- rail: inspectors

| [INDEX] | [SURFACE]                               | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :-------------------------------------- | :------------- | :------------------ |
|   [1]   | `IsChecked(object)`                     | `ICheckedList` | check state query   |
|   [2]   | `SetChecked(object, bool)`              | `ICheckedList` | check state write   |
|   [3]   | `SetRangeChecked(IEnumerable<object>, bool)` | `ICheckedList` | batch check    |
|   [4]   | `Select(object)` / `SelectRange(...)`   | `ICheckedList` | selection set       |
|   [5]   | `SelectionChanged`                      | `ICheckedList` | change event        |
|   [6]   | `Items` / `SourceItems`                 | `ICheckedList` | selected / all items |

[LOCALIZATION_ENTRYPOINTS]: `ILocalizationService` culture management
- rail: inspectors

| [INDEX] | [SURFACE]                           | [SURFACE_ROOT]         | [RAIL]               |
| :-----: | :---------------------------------- | :--------------------- | :------------------- |
|   [1]   | `this[string]`                      | `ILocalizationService` | key translation      |
|   [2]   | `SelectCulture(string)`             | `ILocalizationService` | culture switch       |
|   [3]   | `GetCultures()`                     | `ILocalizationService` | available cultures   |
|   [4]   | `AddExtraService` / `RemoveExtraService` | `ILocalizationService` | composite localization |
|   [5]   | `OnCultureChanged`                  | `ILocalizationService` | culture change event |

[GRID_ENTRYPOINTS]: property grid state and layout — `bodong.Avalonia.PropertyGrid`
- rail: inspectors

| [INDEX] | [SURFACE]                     | [SURFACE_ROOT] | [RAIL]             |
| :-----: | :---------------------------- | :------------- | :----------------- |
|   [1]   | `ViewModel`                   | `PropertyGrid` | inspected object   |
|   [2]   | `IsReadOnly`                  | `PropertyGrid` | read-only state    |
|   [3]   | `LayoutStyle`                 | `PropertyGrid` | layout mode        |
|   [4]   | `IsCategoryVisible`           | `PropertyGrid` | category display   |
|   [5]   | `IsQuickFilterVisible`        | `PropertyGrid` | quick filter       |
|   [6]   | `PropertyOperationVisibility` | `PropertyGrid` | operation controls |
|   [7]   | `CellEditAlignment`           | `PropertyGrid` | editor alignment   |
|   [8]   | `AllCategoriesExpanded`       | `PropertyGrid` | expansion state    |

[FACTORY_ENTRYPOINTS]: editor factory operations
- rail: inspectors

| [INDEX] | [SURFACE]                    | [SURFACE_ROOT]              | [RAIL]            |
| :-----: | :--------------------------- | :-------------------------- | :---------------- |
|   [1]   | `Accept`                     | `ICellEditFactory`          | editor match      |
|   [2]   | `HandleNewProperty`          | `ICellEditFactory`          | editor creation   |
|   [3]   | `HandlePropertyChanged`      | `ICellEditFactory`          | editor refresh    |
|   [4]   | `HandleReadOnlyStateChanged` | `ICellEditFactory`          | read-only refresh |
|   [5]   | `SetPropertyValue`           | `AbstractCellEditFactory`   | value write       |
|   [6]   | `GetPropertyValue`           | `AbstractCellEditFactory`   | value read        |
|   [7]   | `ValidateProperty`           | `AbstractCellEditFactory`   | value validation  |
|   [8]   | `Factories`                  | `CellEditFactoryCollection` | factory set       |

[EVENT_ENTRYPOINTS]: inspector event surfaces
- rail: inspectors

| [INDEX] | [SURFACE]                            | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :----------------------------------- | :------------- | :---------------- |
|   [1]   | `CustomPropertyDescriptorFilter`     | `PropertyGrid` | descriptor filter |
|   [2]   | `CustomNameBlock`                    | `PropertyGrid` | name rendering    |
|   [3]   | `CustomPropertyOperationControl`     | `PropertyGrid` | operation surface |
|   [4]   | `CustomPropertyOperationMenuOpening` | `PropertyGrid` | operation menu    |
|   [5]   | `CommandExecuting`                   | `PropertyGrid` | command gate      |
|   [6]   | `CommandExecuted`                    | `PropertyGrid` | command receipt   |
|   [7]   | `PropertyGotFocus`                   | `PropertyGrid` | focus receipt     |
|   [8]   | `PropertyLostFocus`                  | `PropertyGrid` | focus receipt     |

[EDITOR_ENTRYPOINTS]: list, color, and slider editor operations
- rail: inspectors

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]           | [RAIL]         |
| :-----: | :--------------------- | :----------------------- | :------------- |
|   [1]   | `NewElementCommand`    | `ListEdit`               | list add       |
|   [2]   | `ClearElementsCommand` | `ListEdit`               | list clear     |
|   [3]   | `InsertCommand`        | `ListViewModel`          | list insert    |
|   [4]   | `RemoveCommand`        | `ListViewModel`          | list remove    |
|   [5]   | `ColorChanged`         | `PreviewableColorPicker` | color change   |
|   [6]   | `PreviewColorChanged`  | `PreviewableColorPicker` | preview color  |
|   [7]   | `RealValueChanged`     | `PreviewableSlider`      | slider commit  |
|   [8]   | `PreviewValueChanged`  | `PreviewableSlider`      | slider preview |

## [4]-[IMPLEMENTATION_LAW]

[MODELS_LAW]:
- Package: `bodong.PropertyModels`
- Owns: reactive base objects, command/undo pipeline, checked/selectable collections, data annotation attributes, localization contracts, and descriptor extensions
- Accept: application view models inherit `ReactiveObject` or `MiniReactiveObject`; command pipelines use `CancelableCommandRecorder` + `CommandHistoryViewModel`
- Reject: hand-rolling `INotifyPropertyChanged`, per-screen undo stacks, or string-keyed property registries

[INSPECTOR_LAW]:
- Package: `bodong.Avalonia.PropertyGrid`
- Owns: typed property inspection, editor factories, property operations, list editing, localization services, and inspector events
- Accept: inspectors project product state through typed rows, factories, filters, commands, and receipts
- Reject: reflection UI as public model

[EDITOR_LAW]:
- Package: `bodong.Avalonia.PropertyGrid`
- Owns: editor selection by property context and factory priority
- Accept: panels, companion windows, sidecars, diagnostics, and support views share one inspector rail
- Reject: per-screen reflection editors

[ANNOTATION_LAW]:
- `[ConditionTarget]` + `[PropertyVisibilityCondition]` + `[DependsOnProperty]` on model properties drive visibility and dependency without code-behind
- `[DependsOnProperty]` is propagated automatically by `ReactiveObject`; no manual wiring required
- Attribute classes live in `PropertyModels.ComponentModel.DataAnnotations`; do not re-declare annotation contracts locally
