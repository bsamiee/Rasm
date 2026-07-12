# [RASM_APPUI_API_PROPERTYGRID]

`bodong.Avalonia.PropertyGrid` supplies the Avalonia property inspector control, editor controls, the editor-factory registry, routed inspector events, and localization services. `bodong.PropertyModels` supplies the platform-neutral domain model layer: reactive base objects, the cancelable command/undo recorder, checked/selectable/checked-mask collections, localization contracts, editor-hint and data-annotation attributes, and descriptor extensions consumed by the grid and by application view models.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bodong.PropertyModels`
- package: `bodong.PropertyModels`
- license: MIT
- floor: `net10.0` consumer (`lib/net10.0/PropertyModels.dll`); the package multi-targets net8.0 / net9.0 / net10.0
- assembly: `PropertyModels`
- namespace: `PropertyModels.Collections`, `PropertyModels.ComponentModel` (reactive base, command/undo, editor-hint attributes), `PropertyModels.ComponentModel.DataAnnotations` (validation/condition/enum-filter attributes), `PropertyModels.Extensions`, `PropertyModels.Localization`, `PropertyModels.Utils`
- rail: inspectors

[PACKAGE_SURFACE]: `bodong.Avalonia.PropertyGrid`
- package: `bodong.Avalonia.PropertyGrid` (floor-pinned over its open `>= 12.0.0` range)
- license: MIT
- floor: `net10.0` consumer (`lib/net10.0/Avalonia.PropertyGrid.dll`)
- assembly: `Avalonia.PropertyGrid`
- namespace: `Avalonia.PropertyGrid.Controls` (`PropertyGrid`, `IPropertyGrid`, editor controls, cell context, factory base), `Avalonia.PropertyGrid.Controls.Factories` (`ICellEditFactory`, `AbstractCellEditFactory`), `Avalonia.PropertyGrid.Services` (factory + localization service registries), `Avalonia.PropertyGrid.ViewModels` (layout/order/visibility enums, list view models, converters)
- rail: inspectors

## [02]-[PUBLIC_TYPES]

[REACTIVE_TYPES]: reactive base objects — `PropertyModels.ComponentModel`
- rail: inspectors

| [INDEX] | [SYMBOL]             | [KIND]     | [RAIL]                                                              |
| :-----: | :------------------- | :--------- | :------------------------------------------------------------------ |
|  [01]   | `IReactiveObject`    | contract   | marker over `INotifyPropertyChanged` (batch/raise live on the base) |
|  [02]   | `MiniReactiveObject` | base class | `IReactiveObject` + `SetProperty`/batch, no dependency tracking     |
|  [03]   | `ReactiveObject`     | base class | `MiniReactiveObject` + `[DependsOnProperty]` dependency propagation |

[COMMAND_TYPES]: cancelable command and undo recorder — `PropertyModels.ComponentModel`
- rail: inspectors

| [INDEX] | [SYMBOL]                    | [KIND]           | [RAIL]                                                        |
| :-----: | :-------------------------- | :--------------- | :------------------------------------------------------------ |
|  [01]   | `IBaseCommand`              | contract         | `Name`, `CanExecute()`, `Execute()` (all `bool`)              |
|  [02]   | `ICancelableCommand`        | contract         | `IBaseCommand` + `CanCancel()`, `Cancel()`                    |
|  [03]   | `AbstractBaseCommand`       | abstract base    | `IBaseCommand` base                                           |
|  [04]   | `AbstractCancelableCommand` | abstract base    | `AbstractBaseCommand` + `ICancelableCommand` base             |
|  [05]   | `GenericCommand`            | concrete command | delegate-backed `AbstractBaseCommand`                         |
|  [06]   | `GenericCancelableCommand`  | concrete command | two-delegate cancelable `AbstractCancelableCommand`           |
|  [07]   | `ReactiveCommand`           | concrete command | `ICommand` with `ExecuteDelegate`/`CanExecuteDelegate` fields |
|  [08]   | `CancelableCommandRecorder` | undo recorder    | undo/redo queue (`MaxCommand=20`) + lifecycle events          |
|  [09]   | `CommandHistoryViewModel`   | undo view model  | `CanUndo`, `CanRedo`, queue views, bindable commands          |

[COLLECTION_TYPES]: checked, selectable, and checked-mask collections — `PropertyModels.Collections` / `PropertyModels.ComponentModel`
- rail: inspectors

| [INDEX] | [SYMBOL]           | [KIND]         | [RAIL]                                                                               |
| :-----: | :----------------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `ICheckedList`     | contract       | checked selection over `ICollection`                                                 |
|  [02]   | `CheckedList`      | concrete list  | typed checked multi-select list                                                      |
|  [03]   | `ISelectableList`  | contract       | single/multi selectable list                                                         |
|  [04]   | `SelectableList`   | concrete list  | typed selectable list                                                                |
|  [05]   | `CheckedMaskModel` | reactive model | `ICheckedMaskModel` over `MiniReactiveObject` (bit-mask multi-select editor backing) |

[ANNOTATION_TYPES]: editor-hint attributes — `PropertyModels.ComponentModel`
- rail: inspectors

| [INDEX] | [SYMBOL]                          | [RAIL]                                                  |
| :-----: | :-------------------------------- | :------------------------------------------------------ |
|  [01]   | `FloatPrecisionAttribute`         | float display precision                                 |
|  [02]   | `IntegerIncrementAttribute`       | integer editor increment                                |
|  [03]   | `ProgressAttribute`               | progress-bar editor hint                                |
|  [04]   | `WatermarkAttribute`              | editor watermark text                                   |
|  [05]   | `TrackableAttribute`              | trackable (drag) numeric editor marker                  |
|  [06]   | `MultilineTextAttribute`          | multiline string editor                                 |
|  [07]   | `UnitAttribute`                   | unit label display                                      |
|  [08]   | `EnumDisplayNameAttribute`        | per-enum-member display name                            |
|  [09]   | `EnumExcludeAttribute`            | exclude an enum member from the editor                  |
|  [10]   | `CustomPropertyOrderAttribute`    | explicit property order (`ICustomPropertyOrderHandler`) |
|  [11]   | `AutoCollapseCategoriesAttribute` | collapse categories on load                             |
|  [12]   | `ControlClassesAttribute`         | apply Avalonia style classes to the editor              |

[ANNOTATION_TYPES]: validation, condition, path, and enum-filter attributes — `PropertyModels.ComponentModel.DataAnnotations`
- rail: inspectors

| [INDEX] | [SYMBOL]                                                     | [RAIL]                                                          |
| :-----: | :----------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `DependsOnPropertyAttribute`                                 | reactive dependency declaration                                 |
|  [02]   | `ConditionTargetAttribute`                                   | marks a property as a visibility-condition target               |
|  [03]   | `PropertyVisibilityConditionAttribute`                       | conditional visibility (`AbstractVisibilityConditionAttribute`) |
|  [04]   | `PathBrowsableAttribute`                                     | path-browse editor (`PathBrowsableType`)                        |
|  [05]   | `FileNameValidationAttribute`                                | file-name validation                                            |
|  [06]   | `FloatingNumberEqualToleranceAttribute`                      | float equality tolerance                                        |
|  [07]   | `IEnumValueAuthorizeAttribute`                               | runtime enum-value authorization contract                       |
|  [08]   | `ImagePreviewModeAttribute`                                  | image-preview mode hint                                         |
|  [09]   | `EnumPermitNamesAttribute` / `EnumPermitValuesAttribute`     | enum allow-list by name / value                                 |
|  [10]   | `EnumProhibitNamesAttribute` / `EnumProhibitValuesAttribute` | enum deny-list by name / value                                  |

[LOCALIZATION_TYPES]: localization contracts — `PropertyModels.Localization`
- rail: inspectors

| [INDEX] | [SYMBOL]               | [KIND]        | [RAIL]                                          |
| :-----: | :--------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `ILocalizationService` | contract      | indexer lookup, culture roster + switch, extras |
|  [02]   | `ICultureData`         | contract      | culture metadata                                |
|  [03]   | `AbstractCultureData`  | abstract base | `ICultureData` base                             |

[INSPECTOR_TYPES]: grid, context, and filter surfaces — `Avalonia.PropertyGrid.Controls` / `…ViewModels`
- rail: inspectors

| [INDEX] | [SYMBOL]                     | [KIND]                                                     |
| :-----: | :--------------------------- | :--------------------------------------------------------- |
|  [01]   | `PropertyGrid`               | inspector control (`Controls`)                             |
|  [02]   | `IPropertyGrid`              | inspector contract (`INotifyPropertyChanged, IDisposable`) |
|  [03]   | `PropertyCellContext`        | cell context (`Controls`)                                  |
|  [04]   | `IPropertyGridCellInfo`      | cell info (`IPropertyGridCellInfoContainer`)               |
|  [05]   | `IPropertyGridFilterContext` | filter context (`Controls`)                                |
|  [06]   | `FilterCategory`             | filter-category enum (`Controls`)                          |
|  [07]   | `ReferencePath`              | property path (`ViewModels`)                               |
|  [08]   | `PropertyGridLayoutStyle`    | layout enum (`ViewModels`)                                 |
|  [09]   | `PropertyGridOrderStyle`     | order enum (`ViewModels`)                                  |
|  [10]   | `PropertyVisibility`         | visibility enum (`ViewModels`)                             |
|  [11]   | `CellEditAlignmentType`      | alignment enum (`ViewModels`)                              |
|  [12]   | `FilterChangedEventArgs`     | filter-change args (`ViewModels`)                          |

[EDITOR_TYPES]: editor controls and list view models — `Avalonia.PropertyGrid.Controls` / `…ViewModels`
- rail: inspectors

| [INDEX] | [SYMBOL]                                                      | [KIND]                                     |
| :-----: | :------------------------------------------------------------ | :----------------------------------------- |
|  [01]   | `ButtonEdit`                                                  | button editor (`TemplatedControl`)         |
|  [02]   | `ListEdit`                                                    | list editor (`TemplatedControl`)           |
|  [03]   | `CheckedListEdit`                                             | checked-list editor (`TemplatedControl`)   |
|  [04]   | `RadioButtonListEdit`                                         | radio-list editor (`ICheckableListEdit`)   |
|  [05]   | `ToggleButtonGroupListEdit`                                   | toggle-group editor (`ICheckableListEdit`) |
|  [06]   | `PreviewableColorPicker`                                      | color editor (`TemplatedControl`)          |
|  [07]   | `PreviewableSlider`                                           | numeric editor (`TemplatedControl`)        |
|  [08]   | `TrackableEdit`                                               | trackable numeric editor (`RangeBase`)     |
|  [09]   | `ListViewModel`                                               | list editor model (`ReactiveObject`)       |
|  [10]   | `SingleSelectListViewModel` / `SingleSelectListItemViewModel` | single-select list models (`ViewModels`)   |

[FACTORY_AND_SERVICE_TYPES]: editor factory contract + service registries — `Avalonia.PropertyGrid.Controls.Factories` / `…Services`
- rail: inspectors

| [INDEX] | [SYMBOL]                     | [KIND]                                                                                        |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `ICellEditFactory`           | editor-factory contract (`Controls`)                                                          |
|  [02]   | `ICellEditFactoryCollection` | factory-set contract — `Factories`/`AddFactory`/`RemoveFactory`/`CloneFactories` (`Controls`) |
|  [03]   | `AbstractCellEditFactory`    | editor-factory base (`Factories`); subclass per custom editor                                 |
|  [04]   | `CellEditFactoryService`     | static registry exposing `Default : ICellEditFactoryCollection` (Services)                    |
|  [05]   | `LocalizationService`        | static localization registry (Services)                                                       |

The concrete built-in factories (`Boolean`/`Color`/`Collection`/`Enum`/`Numeric`/`String`/`Path`/`Expandable` cell-edit factories), `CellEditFactoryCollection`, `PropertyGridViewModel`, and `AssemblyJsonAssetLocalizationService` are INTERNAL to the assembly — extend through a public `AbstractCellEditFactory` subclass registered with `CellEditFactoryService`, never by referencing an internal factory type.

[EVENT_TYPES]: routed inspector event args — `Avalonia.PropertyGrid.Controls`
- rail: inspectors

| [INDEX] | [SYMBOL]                                                   | [KIND]                                |
| :-----: | :--------------------------------------------------------- | :------------------------------------ |
|  [01]   | `CustomPropertyDescriptorFilterEventArgs`                  | descriptor filter (`RoutedEventArgs`) |
|  [02]   | `PropertyGotFocusEventArgs` / `PropertyLostFocusEventArgs` | focus events (`RoutedEventArgs`)      |
|  [03]   | `CellPropertyChangedEventArgs`                             | cell-value change (`EventArgs`)       |

## [03]-[ENTRYPOINTS]

[REACTIVE_ENTRYPOINTS]: `PropertyModels.ComponentModel` reactive + command surfaces
- rail: inspectors

| [INDEX] | [SURFACE]                                                                | [SURFACE_ROOT]              | [RAIL]                          |
| :-----: | :----------------------------------------------------------------------- | :-------------------------- | :------------------------------ |
|  [01]   | `RaisePropertyChanged(string)`                                           | `MiniReactiveObject`        | property change notify          |
|  [02]   | `SetProperty<T>(ref T, T, string?)`                                      | `MiniReactiveObject`        | set + notify helper             |
|  [03]   | `BeginBatchUpdate()` / `EndBatchUpdate()`                                | `MiniReactiveObject`        | notification suppression        |
|  [04]   | `Name` / `CanExecute()` / `Execute()`                                    | `IBaseCommand`              | command identity, gate, forward |
|  [05]   | `Cancel()` / `CanCancel()`                                               | `ICancelableCommand`        | inverse gate and apply          |
|  [06]   | `GenericCancelableCommand(...)`                                          | `GenericCancelableCommand`  | cancelable delegate binding     |
|  [07]   | `PushCommand(ICancelableCommand)` / `ExecuteCommand(ICancelableCommand)` | `CancelableCommandRecorder` | enqueue / execute-and-enqueue   |
|  [08]   | `Undo()` / `Redo()` / `Clear()`                                          | `CancelableCommandRecorder` | pop-and-apply inverse / forward |
|  [09]   | `CanUndo` / `CanRedo` / `MaxCommand`                                     | `CancelableCommandRecorder` | undo/redo state + window bound  |
|  [10]   | `GetUndoQueue() : ReadOnlyCollection<ICancelableCommand>`                | `CancelableCommandRecorder` | undo queue snapshot             |
|  [11]   | `GetRedoQueue() : ReadOnlyCollection<ICancelableCommand>`                | `CancelableCommandRecorder` | redo queue snapshot             |
|  [12]   | `OnNewCommandAdded`                                                      | `CancelableCommandRecorder` | enqueue event                   |
|  [13]   | `OnCommandRedo`                                                          | `CancelableCommandRecorder` | redo event                      |
|  [14]   | `OnCommandCanceled`                                                      | `CancelableCommandRecorder` | cancellation event              |
|  [15]   | `OnCommandCleared`                                                       | `CancelableCommandRecorder` | clear event                     |
|  [16]   | `UndoCommand`                                                            | `CommandHistoryViewModel`   | bindable undo                   |
|  [17]   | `RedoCommand`                                                            | `CommandHistoryViewModel`   | bindable redo                   |
|  [18]   | `ClearCommand`                                                           | `CommandHistoryViewModel`   | bindable clear                  |

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

| [INDEX] | [SURFACE]                                                       | [SURFACE_ROOT]         | [RAIL]                   |
| :-----: | :-------------------------------------------------------------- | :--------------------- | :----------------------- |
|  [01]   | `this[string key]`                                              | `ILocalizationService` | key translation          |
|  [02]   | `SelectCulture(string cultureName)`                             | `ILocalizationService` | culture switch           |
|  [03]   | `GetCultures() : ICultureData[]`                                | `ILocalizationService` | available cultures       |
|  [04]   | `CultureData`                                                   | `ILocalizationService` | current culture metadata |
|  [05]   | `AddExtraService` / `RemoveExtraService` / `GetExtraServices()` | `ILocalizationService` | composite localization   |
|  [06]   | `OnCultureChanged`                                              | `ILocalizationService` | culture change event     |

[GRID_ENTRYPOINTS]: property grid state and layout — `Avalonia.PropertyGrid.Controls.PropertyGrid`
- rail: inspectors

`PropertyGrid.DataContext` binds the inspected object; the control exposes no public `ViewModel` property.

| [INDEX] | [SURFACE]                                     | [RAIL]                |
| :-----: | :-------------------------------------------- | :-------------------- |
|  [01]   | `DataContext`                                 | inspected object      |
|  [02]   | `IsReadOnly`                                  | read-only state       |
|  [03]   | `LayoutStyle` (`PropertyGridLayoutStyle`)     | layout mode           |
|  [04]   | `CategoryOrderStyle`                          | category ordering     |
|  [05]   | `PropertyOrderStyle`                          | property ordering     |
|  [06]   | `IsCategoryVisible`                           | category display      |
|  [07]   | `IsTitleVisible`                              | title display         |
|  [08]   | `IsHeaderVisible`                             | header display        |
|  [09]   | `IsQuickFilterVisible`                        | quick filter          |
|  [10]   | `PropertyOperationVisibility`                 | operation display     |
|  [11]   | `CellEditAlignment` (`CellEditAlignmentType`) | editor alignment      |
|  [12]   | `IsAutoNameWidth`                             | automatic name width  |
|  [13]   | `NameWidth`                                   | explicit name width   |
|  [14]   | `AllCategoriesExpanded`                       | expansion state       |
|  [15]   | `TopHeaderContent`                            | top chrome content    |
|  [16]   | `MiddleContent`                               | middle chrome content |
|  [17]   | `BottomContent`                               | bottom chrome content |

`CategoryOrderStyle` and `PropertyOrderStyle` carry `PropertyGridOrderStyle`; `PropertyOperationVisibility` carries the identically named enum.

[FACTORY_ENTRYPOINTS]: editor factory contract + registry operations
- rail: inspectors

`ICellEditFactoryCollection` registry operations resolve through `CellEditFactoryService.Default`.

| [INDEX] | [SURFACE]                                           | [SURFACE_ROOT]               | [RAIL]                   |
| :-----: | :-------------------------------------------------- | :--------------------------- | :----------------------- |
|  [01]   | `Accept(object accessToken) : bool`                 | `ICellEditFactory`           | editor match             |
|  [02]   | `ImportPriority : int`                              | `ICellEditFactory`           | match priority           |
|  [03]   | `HandleNewProperty(PropertyCellContext) : Control?` | `ICellEditFactory`           | editor creation          |
|  [04]   | `HandlePropertyChanged(PropertyCellContext) : bool` | `ICellEditFactory`           | editor refresh           |
|  [05]   | `HandleReadOnlyStateChanged(Control, bool)`         | `ICellEditFactory`           | read-only refresh        |
|  [06]   | `HandlePropagateVisibility(...)`                    | `ICellEditFactory`           | filter-driven visibility |
|  [07]   | `SetPropertyValue(PropertyCellContext, object?)`    | `ICellEditFactory`           | command-routed write     |
|  [08]   | `GetPropertyValue(PropertyCellContext) : object?`   | `ICellEditFactory`           | value read               |
|  [09]   | `Clone() : ICellEditFactory?`                       | `ICellEditFactory`           | per-cell factory clone   |
|  [10]   | `Factories`                                         | `ICellEditFactoryCollection` | registered factories     |
|  [11]   | `AddFactory(ICellEditFactory)`                      | `ICellEditFactoryCollection` | factory registration     |
|  [12]   | `RemoveFactory(ICellEditFactory)`                   | `ICellEditFactoryCollection` | factory removal          |
|  [13]   | `CloneFactories(object)`                            | `ICellEditFactoryCollection` | factory snapshot         |

[HandlePropagateVisibility]:

- Signature: `HandlePropagateVisibility(object?, PropertyCellContext, IPropertyGridFilterContext, …) : PropertyVisibility?`

[EVENT_ENTRYPOINTS]: routed inspector event surfaces — `PropertyGrid`
- rail: inspectors

| [INDEX] | [SURFACE]                                | [SURFACE_ROOT] | [RAIL]                                |
| :-----: | :--------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `CustomPropertyDescriptorFilter`         | `PropertyGrid` | descriptor filter (`RoutedEventArgs`) |
|  [02]   | `CustomNameBlock`                        | `PropertyGrid` | name rendering                        |
|  [03]   | `CustomPropertyOperationControl`         | `PropertyGrid` | operation surface                     |
|  [04]   | `CustomPropertyOperationMenuOpening`     | `PropertyGrid` | operation menu                        |
|  [05]   | `CommandExecuting` / `CommandExecuted`   | `PropertyGrid` | command gate / receipt                |
|  [06]   | `PropertyGotFocus` / `PropertyLostFocus` | `PropertyGrid` | focus receipts                        |
|  [07]   | `NameWidthChanged`                       | `PropertyGrid` | name-column width change              |

[EDITOR_ENTRYPOINTS]: list, color, and slider editor operations
- rail: inspectors

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT]           | [RAIL]                           |
| :-----: | :----------------------------------------- | :----------------------- | :------------------------------- |
|  [01]   | `NewElementCommand`                        | `ListEdit`               | list add                         |
|  [02]   | `ClearElementsCommand`                     | `ListEdit`               | list clear                       |
|  [03]   | `InsertCommand`                            | `ListViewModel`          | list insert                      |
|  [04]   | `RemoveCommand`                            | `ListViewModel`          | list remove                      |
|  [05]   | `ColorChanged` / `PreviewColorChanged`     | `PreviewableColorPicker` | committed / preview color        |
|  [06]   | `RealValueChanged` / `PreviewValueChanged` | `PreviewableSlider`      | committed / preview slider value |

## [04]-[IMPLEMENTATION_LAW]

[MODELS_LAW]:
- Package: `bodong.PropertyModels`
- Owns: reactive base objects, the cancelable command/undo recorder, checked/selectable/checked-mask collections, editor-hint attributes (`PropertyModels.ComponentModel`), validation/condition/enum-filter attributes (`PropertyModels.ComponentModel.DataAnnotations`), localization contracts, and descriptor extensions.
- Accept: application view models inherit `ReactiveObject` or `MiniReactiveObject`; command pipelines use `CancelableCommandRecorder` + `CommandHistoryViewModel`; multi-select editors back onto `CheckedList`/`CheckedMaskModel`.
- Reject: hand-rolling `INotifyPropertyChanged`, per-screen undo stacks, or string-keyed property registries; declaring editor-hint attributes under `DataAnnotations` (they live in `PropertyModels.ComponentModel`).

[RECORDER_LAW]:
- `ICancelableCommand` is a two-delegate command: `Execute()` runs the forward and `Cancel()` runs the inverse, both `bool`, gated by `CanExecute()`/`CanCancel()`; `GenericCancelableCommand(name, executeFunc, cancelFunc, canExecuteFunc?, canCancelFunc?)` binds them as `Func<bool>?`.
- `CancelableCommandRecorder` owns the queue lifecycle: `PushCommand` enqueues a command whose forward already applied (clearing the redo queue), `ExecuteCommand` runs the forward then enqueues, `Undo()` pops the head and runs its `Cancel`, `Redo()` re-runs its `Execute`, `Clear()` empties both queues; `CanUndo`/`CanRedo` read the head command's `CanCancel`/`CanExecute`, `MaxCommand` (default 20) bounds the window, and the `OnNewCommandAdded`/`OnCommandRedo`/`OnCommandCanceled`/`OnCommandCleared` events drive an `INotifyPropertyChanged`-free undo HUD (`CommandHistoryViewModel` wraps the recorder as bindable `UndoCommand`/`RedoCommand`/`ClearCommand`).
- A revert that resolves an op without driving `Undo()`/`Redo()` leaves the inverse unapplied — the recorder, not a hand-rolled stack, owns pop-and-apply.

[INSPECTOR_LAW]:
- Package: `bodong.Avalonia.PropertyGrid`
- Owns: typed property inspection (`Avalonia.PropertyGrid.Controls.PropertyGrid`), the editor-factory registry, property operations, list editing, localization services, and routed inspector events.
- Accept: the inspected object binds through the control's `DataContext`; inspectors project product state through typed rows, factories, filters, commands, and routed events; layout/order/visibility drive off the `PropertyGridLayoutStyle`/`PropertyGridOrderStyle`/`PropertyVisibility`/`CellEditAlignmentType` enums.
- Reject: reflection UI as public model; referencing the internal `PropertyGridViewModel` or internal concrete factories; assuming a public `ViewModel` property where the binding is `DataContext`.

[EDITOR_LAW]:
- Package: `bodong.Avalonia.PropertyGrid`
- Owns: editor selection by `ICellEditFactory.Accept(object accessToken)` match ranked by `ImportPriority` (higher processed first) through the `CellEditFactoryService.Default : ICellEditFactoryCollection` registry.
- Accept: a custom editor is a public `AbstractCellEditFactory` subclass registered through `CellEditFactoryService.Default.AddFactory(factory)` with an `ImportPriority` above the built-ins it overrides; panels, companion windows, sidecars, diagnostics, and support views share one inspector rail.
- Reject: per-screen reflection editors; subclassing or referencing an internal built-in factory instead of `AbstractCellEditFactory`; assuming `Accept` takes a typed `PropertyCellContext` (it takes the `object` access token; the `PropertyCellContext` arrives on `HandleNewProperty`/`HandlePropertyChanged`).

[ANNOTATION_LAW]:
- `[ConditionTarget]` + `[PropertyVisibilityCondition]` + `[DependsOnProperty]` on model properties drive visibility and dependency without code-behind; `[DependsOnProperty]` is propagated automatically by `ReactiveObject`, no manual wiring required.
- Editor presentation is attribute-driven: `[Watermark]`, `[Unit]`, `[Progress]`, `[Trackable]`, `[FloatPrecision]`, `[IntegerIncrement]`, `[MultilineText]`, `[ControlClasses]`, `[CustomPropertyOrder]`, `[EnumDisplayName]`/`[EnumExclude]` (in `PropertyModels.ComponentModel`) and `[PathBrowsable]`, `[EnumPermit*]`/`[EnumProhibit*]`, `[FileNameValidation]`, `[ImagePreviewMode]` (in `PropertyModels.ComponentModel.DataAnnotations`) select and shape the editor.
- Attribute classes live in `PropertyModels`; do not re-declare annotation contracts locally, and route each attribute to its real namespace (editor-hint -> `ComponentModel`, validation/condition/enum-filter -> `ComponentModel.DataAnnotations`).
