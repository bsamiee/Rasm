# [RASM_APPUI_API_PROPERTYGRID]

`bodong.Avalonia.PropertyGrid` supplies the Avalonia property inspector control, editor factories, events, and localization services. `bodong.PropertyModels` supplies the platform-neutral domain model layer: reactive base objects, command/undo infrastructure, checked/selectable collections, localization contracts, data annotation attributes, and validation extensions consumed by the grid and by application view models.

## [01]-[PACKAGE_SURFACE]

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

## [02]-[PUBLIC_TYPES]

[REACTIVE_TYPES]: reactive base objects — `PropertyModels.ComponentModel`
- rail: inspectors

| [INDEX] | [SYMBOL]                   | [KIND]            | [RAIL]                                                   |
| :-----: | :------------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `IReactiveObject`          | contract          | `PropertyChanged` + batch + raise                        |
|  [02]   | `MiniReactiveObject`       | base class        | `IReactiveObject` without dependency tracking            |
|  [03]   | `ReactiveObject`           | base class        | `MiniReactiveObject` + `[DependsOnProperty]` propagation |
|  [04]   | `ReactiveObjectExtensions` | static extensions | `RaisePropertyChanged` helpers                           |

[COMMAND_TYPES]: command and undo infrastructure — `PropertyModels.ComponentModel`
- rail: inspectors

| [INDEX] | [SYMBOL]                    | [KIND]           | [RAIL]                            |
| :-----: | :-------------------------- | :--------------- | :-------------------------------- |
|  [01]   | `IBaseCommand`              | contract         | `Name`, `CanExecute`, `Execute`   |
|  [02]   | `ICancelableCommand`        | contract         | cancelable command contract       |
|  [03]   | `AbstractBaseCommand`       | abstract base    | `IBaseCommand` base               |
|  [04]   | `AbstractCancelableCommand` | abstract base    | `ICancelableCommand` base         |
|  [05]   | `GenericCommand`            | concrete command | delegate-backed command           |
|  [06]   | `GenericCancelableCommand`  | concrete command | delegate-backed cancelable        |
|  [07]   | `ReactiveCommand`           | reactive command | `INotifyCommandExecuting` command |
|  [08]   | `CancelableCommandRecorder` | undo recorder    | undo/redo queue, `MaxCommand=20`  |
|  [09]   | `CommandHistoryViewModel`   | undo view model  | `CanUndo`, `CanRedo`, queue views |

[COLLECTION_TYPES]: checked and selectable list collections — `PropertyModels.Collections`
- rail: inspectors

| [INDEX] | [SYMBOL]          | [KIND]        | [RAIL]                               |
| :-----: | :---------------- | :------------ | :----------------------------------- |
|  [01]   | `ICheckedList`    | contract      | checked selection over `ICollection` |
|  [02]   | `CheckedList`     | concrete list | typed checked multi-select list      |
|  [03]   | `ISelectableList` | contract      | single/multi selectable list         |
|  [04]   | `SelectableList`  | concrete list | typed selectable list                |

[ANNOTATION_TYPES]: data annotation attributes — `PropertyModels.ComponentModel.DataAnnotations`
- rail: inspectors

| [INDEX] | [SYMBOL]                               | [RAIL]                             |
| :-----: | :------------------------------------- | :--------------------------------- |
|  [01]   | `ConditionTargetAttribute`             | marks property as condition target |
|  [02]   | `PropertyVisibilityConditionAttribute` | conditional property visibility    |
|  [03]   | `DependsOnPropertyAttribute`           | reactive dependency declaration    |
|  [04]   | `PathBrowsableAttribute`               | path browse mode                   |
|  [05]   | `EnumPermitNamesAttribute`             | enum filter by name                |
|  [06]   | `EnumPermitValuesAttribute`            | enum filter by value               |
|  [07]   | `EnumProhibitNamesAttribute`           | enum exclusion by name             |
|  [08]   | `EnumProhibitValuesAttribute`          | enum exclusion by value            |
|  [09]   | `FloatPrecisionAttribute`              | float display precision            |
|  [10]   | `IntegerIncrementAttribute`            | integer editor increment           |
|  [11]   | `ProgressAttribute`                    | progress bar editor hint           |
|  [12]   | `WatermarkAttribute`                   | editor watermark text              |
|  [13]   | `TrackableAttribute`                   | change tracking marker             |
|  [14]   | `MultilineTextAttribute`               | multiline string editor            |
|  [15]   | `UnitAttribute`                        | unit label display                 |

[LOCALIZATION_TYPES]: localization contracts — `PropertyModels.Localization`
- rail: inspectors

| [INDEX] | [SYMBOL]               | [KIND]        | [RAIL]                             |
| :-----: | :--------------------- | :------------ | :--------------------------------- |
|  [01]   | `ILocalizationService` | contract      | key lookup, culture change, extras |
|  [02]   | `ICultureData`         | contract      | culture metadata                   |
|  [03]   | `AbstractCultureData`  | abstract base | `ICultureData` base                |

[INSPECTOR_TYPES]: grid, context, and view model surfaces — `bodong.Avalonia.PropertyGrid` — rail: inspectors

| [INDEX] | [SYMBOL]                     | [KIND]             |
| :-----: | :--------------------------- | :----------------- |
|  [01]   | `PropertyGrid`               | inspector control  |
|  [02]   | `IPropertyGrid`              | inspector contract |
|  [03]   | `PropertyGridViewModel`      | inspector model    |
|  [04]   | `PropertyCellContext`        | cell context       |
|  [05]   | `IPropertyGridCellInfo`      | cell info          |
|  [06]   | `IPropertyGridFilterContext` | filter context     |
|  [07]   | `FilterCategory`             | filter value       |
|  [08]   | `ReferencePath`              | property path      |

[EDITOR_TYPES]: editor controls and list models — rail: inspectors

| [INDEX] | [SYMBOL]                    | [KIND]         |
| :-----: | :-------------------------- | :------------- |
|  [01]   | `ButtonEdit`                | button editor  |
|  [02]   | `ListEdit`                  | list editor    |
|  [03]   | `CheckedListEdit`           | checked list   |
|  [04]   | `RadioButtonListEdit`       | radio list     |
|  [05]   | `ToggleButtonGroupListEdit` | toggle list    |
|  [06]   | `PreviewableColorPicker`    | color editor   |
|  [07]   | `PreviewableSlider`         | numeric editor |
|  [08]   | `TrackableEdit`             | tracked editor |
|  [09]   | `ListViewModel`             | list model     |
|  [10]   | `SingleSelectListViewModel` | select model   |

[FACTORY_TYPES]: editor factory families — rail: inspectors

| [INDEX] | [SYMBOL]                    | [KIND]            |
| :-----: | :-------------------------- | :---------------- |
|  [01]   | `ICellEditFactory`          | factory contract  |
|  [02]   | `AbstractCellEditFactory`   | factory base      |
|  [03]   | `CellEditFactoryCollection` | factory set       |
|  [04]   | `BooleanCellEditFactory`    | boolean editor    |
|  [05]   | `ColorCellEditFactory`      | color editor      |
|  [06]   | `CollectionCellEditFactory` | collection editor |
|  [07]   | `EnumCellEditFactory`       | enum editor       |
|  [08]   | `NumericCellEditFactory`    | numeric editor    |
|  [09]   | `StringCellEditFactory`     | string editor     |
|  [10]   | `PathCellEditFactory`       | path editor       |
|  [11]   | `ExpandableCellEditFactory` | nested object     |

[EVENT_AND_SERVICE_TYPES]: events, localization, and services — rail: inspectors

| [INDEX] | [SYMBOL]                                  | [KIND]               |
| :-----: | :---------------------------------------- | :------------------- |
|  [01]   | `CellPropertyChangedEventArgs`            | cell change          |
|  [02]   | `CustomPropertyDescriptorFilterEventArgs` | descriptor filter    |
|  [03]   | `CustomPropertyOperationControlEventArgs` | operation control    |
|  [04]   | `CustomPropertyDefaultOperationEventArgs` | operation default    |
|  [05]   | `PropertyGotFocusEventArgs`               | focus event          |
|  [06]   | `PropertyLostFocusEventArgs`              | focus event          |
|  [07]   | `CellEditFactoryService`                  | factory service      |
|  [08]   | `LocalizationService`                     | localization service |
|  [09]   | `AssemblyJsonAssetLocalizationService`    | asset localization   |

## [03]-[ENTRYPOINTS]

[REACTIVE_ENTRYPOINTS]: `PropertyModels.ComponentModel` reactive surfaces
- rail: inspectors

| [INDEX] | [SURFACE]                                                                     | [SURFACE_ROOT]              | [RAIL]                              |
| :-----: | :---------------------------------------------------------------------------- | :-------------------------- | :---------------------------------- |
|  [01]   | `RaisePropertyChanged(string)`                                                | `MiniReactiveObject`        | property change notify              |
|  [02]   | `SetProperty<T>(ref T, T, string?)`                                           | `MiniReactiveObject`        | set + notify helper                 |
|  [03]   | `BeginBatchUpdate()` / `EndBatchUpdate()`                                     | `MiniReactiveObject`        | suppress intermediate notifications |
|  [04]   | `CanExecute()` / `Execute()`                                                  | `IBaseCommand`              | command gate and execute            |
|  [05]   | `Cancel()` / `CanCancel()`                                                    | `ICancelableCommand`        | inverse gate and apply              |
|  [06]   | `new GenericCancelableCommand(name, Func<bool>? exec, Func<bool>? cancel, …)` | `GenericCancelableCommand`  | two-delegate cancelable command     |
|  [07]   | `PushCommand(ICancelableCommand)` / `ExecuteCommand(ICancelableCommand)`      | `CancelableCommandRecorder` | enqueue / execute-and-enqueue       |
|  [08]   | `Undo()` / `Redo()` / `Clear()`                                               | `CancelableCommandRecorder` | pop-and-apply inverse / forward     |
|  [09]   | `CanUndo` / `CanRedo`                                                         | `CancelableCommandRecorder` | undo/redo state                     |
|  [10]   | `GetUndoQueue()` / `GetRedoQueue()`                                           | `CancelableCommandRecorder` | queue snapshots                     |
|  [11]   | `UndoCommand` / `RedoCommand` / `ClearCommand`                                | `CommandHistoryViewModel`   | bindable commands                   |

[COLLECTION_ENTRYPOINTS]: checked list operations — `PropertyModels.Collections`
- rail: inspectors

| [INDEX] | [SURFACE]                                    | [SURFACE_ROOT] | [RAIL]               |
| :-----: | :------------------------------------------- | :------------- | :------------------- |
|  [01]   | `IsChecked(object)`                          | `ICheckedList` | check state query    |
|  [02]   | `SetChecked(object, bool)`                   | `ICheckedList` | check state write    |
|  [03]   | `SetRangeChecked(IEnumerable<object>, bool)` | `ICheckedList` | batch check          |
|  [04]   | `Select(object)` / `SelectRange(...)`        | `ICheckedList` | selection set        |
|  [05]   | `SelectionChanged`                           | `ICheckedList` | change event         |
|  [06]   | `Items` / `SourceItems`                      | `ICheckedList` | selected / all items |

[LOCALIZATION_ENTRYPOINTS]: `ILocalizationService` culture management
- rail: inspectors

| [INDEX] | [SURFACE]                                | [SURFACE_ROOT]         | [RAIL]                 |
| :-----: | :--------------------------------------- | :--------------------- | :--------------------- |
|  [01]   | `this[string]`                           | `ILocalizationService` | key translation        |
|  [02]   | `SelectCulture(string)`                  | `ILocalizationService` | culture switch         |
|  [03]   | `GetCultures()`                          | `ILocalizationService` | available cultures     |
|  [04]   | `AddExtraService` / `RemoveExtraService` | `ILocalizationService` | composite localization |
|  [05]   | `OnCultureChanged`                       | `ILocalizationService` | culture change event   |

[GRID_ENTRYPOINTS]: property grid state and layout — `bodong.Avalonia.PropertyGrid`
- rail: inspectors

| [INDEX] | [SURFACE]                     | [SURFACE_ROOT] | [RAIL]             |
| :-----: | :---------------------------- | :------------- | :----------------- |
|  [01]   | `ViewModel`                   | `PropertyGrid` | inspected object   |
|  [02]   | `IsReadOnly`                  | `PropertyGrid` | read-only state    |
|  [03]   | `LayoutStyle`                 | `PropertyGrid` | layout mode        |
|  [04]   | `IsCategoryVisible`           | `PropertyGrid` | category display   |
|  [05]   | `IsQuickFilterVisible`        | `PropertyGrid` | quick filter       |
|  [06]   | `PropertyOperationVisibility` | `PropertyGrid` | operation controls |
|  [07]   | `CellEditAlignment`           | `PropertyGrid` | editor alignment   |
|  [08]   | `AllCategoriesExpanded`       | `PropertyGrid` | expansion state    |

[FACTORY_ENTRYPOINTS]: editor factory operations
- rail: inspectors

| [INDEX] | [SURFACE]                    | [SURFACE_ROOT]              | [RAIL]            |
| :-----: | :--------------------------- | :-------------------------- | :---------------- |
|  [01]   | `Accept`                     | `ICellEditFactory`          | editor match      |
|  [02]   | `HandleNewProperty`          | `ICellEditFactory`          | editor creation   |
|  [03]   | `HandlePropertyChanged`      | `ICellEditFactory`          | editor refresh    |
|  [04]   | `HandleReadOnlyStateChanged` | `ICellEditFactory`          | read-only refresh |
|  [05]   | `SetPropertyValue`           | `AbstractCellEditFactory`   | value write       |
|  [06]   | `GetPropertyValue`           | `AbstractCellEditFactory`   | value read        |
|  [07]   | `ValidateProperty`           | `AbstractCellEditFactory`   | value validation  |
|  [08]   | `Factories`                  | `CellEditFactoryCollection` | factory set       |

[EVENT_ENTRYPOINTS]: inspector event surfaces
- rail: inspectors

| [INDEX] | [SURFACE]                            | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :----------------------------------- | :------------- | :---------------- |
|  [01]   | `CustomPropertyDescriptorFilter`     | `PropertyGrid` | descriptor filter |
|  [02]   | `CustomNameBlock`                    | `PropertyGrid` | name rendering    |
|  [03]   | `CustomPropertyOperationControl`     | `PropertyGrid` | operation surface |
|  [04]   | `CustomPropertyOperationMenuOpening` | `PropertyGrid` | operation menu    |
|  [05]   | `CommandExecuting`                   | `PropertyGrid` | command gate      |
|  [06]   | `CommandExecuted`                    | `PropertyGrid` | command receipt   |
|  [07]   | `PropertyGotFocus`                   | `PropertyGrid` | focus receipt     |
|  [08]   | `PropertyLostFocus`                  | `PropertyGrid` | focus receipt     |

[EDITOR_ENTRYPOINTS]: list, color, and slider editor operations
- rail: inspectors

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]           | [RAIL]         |
| :-----: | :--------------------- | :----------------------- | :------------- |
|  [01]   | `NewElementCommand`    | `ListEdit`               | list add       |
|  [02]   | `ClearElementsCommand` | `ListEdit`               | list clear     |
|  [03]   | `InsertCommand`        | `ListViewModel`          | list insert    |
|  [04]   | `RemoveCommand`        | `ListViewModel`          | list remove    |
|  [05]   | `ColorChanged`         | `PreviewableColorPicker` | color change   |
|  [06]   | `PreviewColorChanged`  | `PreviewableColorPicker` | preview color  |
|  [07]   | `RealValueChanged`     | `PreviewableSlider`      | slider commit  |
|  [08]   | `PreviewValueChanged`  | `PreviewableSlider`      | slider preview |

## [04]-[IMPLEMENTATION_LAW]

[MODELS_LAW]:
- Package: `bodong.PropertyModels`
- Owns: reactive base objects, command/undo pipeline, checked/selectable collections, data annotation attributes, localization contracts, and descriptor extensions
- Accept: application view models inherit `ReactiveObject` or `MiniReactiveObject`; command pipelines use `CancelableCommandRecorder` + `CommandHistoryViewModel`
- Reject: hand-rolling `INotifyPropertyChanged`, per-screen undo stacks, or string-keyed property registries

[RECORDER_LAW]:
- `ICancelableCommand` is a two-delegate command: `Execute()` runs the forward and `Cancel()` runs the inverse, both `bool`, gated by `CanExecute()`/`CanCancel()`; `GenericCancelableCommand(name, executeFunc, cancelFunc, canExecuteFunc?, canCancelFunc?)` binds them as `Func<bool>?`.
- `CancelableCommandRecorder` owns the queue lifecycle: `PushCommand` enqueues a command whose forward already applied (clearing the redo queue), `ExecuteCommand` runs the forward then enqueues, `Undo()` pops the head and runs its `Cancel`, `Redo()` re-runs its `Execute`, `Clear()` empties both queues; `CanUndo`/`CanRedo` read the head command's `CanCancel`/`CanExecute`, and `MaxCommand` (default 20) bounds the window.
- A revert that resolves an op without driving `Undo()`/`Redo()` leaves the inverse unapplied — the recorder, not a hand-rolled stack, owns pop-and-apply.

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
