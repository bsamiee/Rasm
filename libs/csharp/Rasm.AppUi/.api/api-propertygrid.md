# [RASM_APPUI_API_PROPERTYGRID]

`bodong.Avalonia.PropertyGrid` owns the Avalonia property-inspector control: the editor-factory registry, routed inspector events, and the cell and filter contracts that project a live object bound through `DataContext` as typed editor rows. `bodong.PropertyModels` owns the host-neutral model substrate the grid and every inspected view-model bind against — reactive bases, the cancelable command/undo recorder, the selection collections, the editor-hint and data-annotation attribute vocabulary, and the localization contracts. Both packages serve the inspector rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bodong.PropertyModels`
- package: `bodong.PropertyModels` (MIT)
- assembly: `PropertyModels`
- namespace: `PropertyModels.Collections`, `.ComponentModel`, `.ComponentModel.DataAnnotations`, `.Extensions`, `.Localization`, `.Utils`
- target: `lib/net10.0`
- asset: runtime library
- rail: inspectors

[PACKAGE_SURFACE]: `bodong.Avalonia.PropertyGrid`
- package: `bodong.Avalonia.PropertyGrid` (MIT)
- assembly: `Avalonia.PropertyGrid`
- namespace: `Avalonia.PropertyGrid.Controls`, `.Controls.Factories`, `.Services`, `.ViewModels`
- target: `lib/net10.0`
- asset: runtime library
- rail: inspectors

## [02]-[PUBLIC_TYPES]

[REACTIVE_TYPES]: reactive base objects — `PropertyModels.ComponentModel`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `IReactiveObject`    | interface     | marker over `INotifyPropertyChanged`; batch and raise on the base   |
|  [02]   | `MiniReactiveObject` | class         | `IReactiveObject` + `SetProperty`/batch, no dependency tracking     |
|  [03]   | `ReactiveObject`     | class         | `MiniReactiveObject` + `[DependsOnProperty]` dependency propagation |

[COMMAND_TYPES]: cancelable command and undo recorder — `PropertyModels.ComponentModel`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [CAPABILITY]                                                  |
| :-----: | :-------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `IBaseCommand`              | interface      | `Name`, `CanExecute()`, `Execute()` (all `bool`)              |
|  [02]   | `ICancelableCommand`        | interface      | `IBaseCommand` + `CanCancel()`, `Cancel()`                    |
|  [03]   | `AbstractBaseCommand`       | abstract class | `IBaseCommand` base                                           |
|  [04]   | `AbstractCancelableCommand` | abstract class | `AbstractBaseCommand` + `ICancelableCommand` base             |
|  [05]   | `GenericCommand`            | class          | delegate-backed `AbstractBaseCommand`                         |
|  [06]   | `GenericCancelableCommand`  | class          | two-delegate cancelable `AbstractCancelableCommand`           |
|  [07]   | `ReactiveCommand`           | class          | `ICommand` with `ExecuteDelegate`/`CanExecuteDelegate` fields |
|  [08]   | `CancelableCommandRecorder` | class          | undo/redo queue (`MaxCommand=20`) + lifecycle events          |
|  [09]   | `CommandHistoryViewModel`   | class          | `CanUndo`, `CanRedo`, queue views, bindable commands          |

[COLLECTION_TYPES]: checked, selectable, and checked-mask collections — `PropertyModels.Collections` / `.ComponentModel`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `ICheckedList`     | interface     | checked selection over `ICollection`                                 |
|  [02]   | `CheckedList`      | class         | typed checked multi-select list                                      |
|  [03]   | `ISelectableList`  | interface     | single/multi selectable list                                         |
|  [04]   | `SelectableList`   | class         | typed selectable list                                                |
|  [05]   | `CheckedMaskModel` | class         | `ICheckedMaskModel` over `MiniReactiveObject`; bit-mask multi-select |

[ANNOTATION_TYPES]: editor-hint attributes — `PropertyModels.ComponentModel`

| [INDEX] | [SYMBOL]                          | [CAPABILITY]                                            |
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

| [INDEX] | [SYMBOL]                                                     | [CAPABILITY]                                                    |
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

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :--------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `ILocalizationService` | interface      | indexer lookup, culture roster + switch, extras |
|  [02]   | `ICultureData`         | interface      | culture metadata                                |
|  [03]   | `AbstractCultureData`  | abstract class | `ICultureData` base                             |

[INSPECTOR_TYPES]: grid, context, and filter surfaces — `Avalonia.PropertyGrid.Controls` / `.ViewModels`

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                                               |
| :-----: | :--------------------------- | :--------------------------------------------------------- |
|  [01]   | `PropertyGrid`               | inspector control                                          |
|  [02]   | `IPropertyGrid`              | inspector contract (`INotifyPropertyChanged, IDisposable`) |
|  [03]   | `PropertyCellContext`        | cell context                                               |
|  [04]   | `IPropertyGridCellInfo`      | cell info (`IPropertyGridCellInfoContainer`)               |
|  [05]   | `IPropertyGridFilterContext` | filter context                                             |
|  [06]   | `FilterCategory`             | filter-category enum                                       |
|  [07]   | `ReferencePath`              | property path                                              |
|  [08]   | `PropertyGridLayoutStyle`    | layout enum                                                |
|  [09]   | `PropertyGridOrderStyle`     | order enum                                                 |
|  [10]   | `PropertyVisibility`         | visibility enum                                            |
|  [11]   | `CellEditAlignmentType`      | alignment enum                                             |
|  [12]   | `FilterChangedEventArgs`     | filter-change args                                         |

[EDITOR_TYPES]: editor controls and list view models — `Avalonia.PropertyGrid.Controls` / `.ViewModels`

| [INDEX] | [SYMBOL]                                                      | [CAPABILITY]                               |
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
|  [10]   | `SingleSelectListViewModel` / `SingleSelectListItemViewModel` | single-select list models                  |

[FACTORY_AND_SERVICE_TYPES]: editor-factory contract + service registries — `Avalonia.PropertyGrid.Controls.Factories` / `.Services`

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                                                                     |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `ICellEditFactory`           | editor-factory contract                                                          |
|  [02]   | `ICellEditFactoryCollection` | factory-set contract — `Factories`/`AddFactory`/`RemoveFactory`/`CloneFactories` |
|  [03]   | `AbstractCellEditFactory`    | editor-factory base; subclass per custom editor                                  |
|  [04]   | `CellEditFactoryService`     | static registry exposing `Default : ICellEditFactoryCollection`                  |
|  [05]   | `LocalizationService`        | static localization registry                                                     |

A custom editor extends the public `AbstractCellEditFactory` registered through `CellEditFactoryService`; the built-in factories (`Boolean`/`Color`/`Collection`/`Enum`/`Numeric`/`String`/`Path`/`Expandable`), `CellEditFactoryCollection`, `PropertyGridViewModel`, and `AssemblyJsonAssetLocalizationService` are assembly-internal and never referenced.

[EVENT_TYPES]: routed inspector event args — `Avalonia.PropertyGrid.Controls`

| [INDEX] | [SYMBOL]                                                   | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------- | :------------------------------------ |
|  [01]   | `CustomPropertyDescriptorFilterEventArgs`                  | descriptor filter (`RoutedEventArgs`) |
|  [02]   | `PropertyGotFocusEventArgs` / `PropertyLostFocusEventArgs` | focus events (`RoutedEventArgs`)      |
|  [03]   | `CellPropertyChangedEventArgs`                             | cell-value change (`EventArgs`)       |

## [03]-[ENTRYPOINTS]

[REACTIVE_ENTRYPOINTS]: reactive base, command, and recorder surfaces — `PropertyModels.ComponentModel`

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------------------- | :------------------------------ |
|  [01]   | `MiniReactiveObject.RaisePropertyChanged(string)`                      | property change notify          |
|  [02]   | `MiniReactiveObject.SetProperty<T>(ref T, T, string?)`                 | set + notify helper             |
|  [03]   | `MiniReactiveObject.BeginBatchUpdate()` / `EndBatchUpdate()`           | notification suppression        |
|  [04]   | `IBaseCommand.Name` / `CanExecute()` / `Execute()`                     | command identity, gate, forward |
|  [05]   | `ICancelableCommand.Cancel()` / `CanCancel()`                          | inverse gate and apply          |
|  [06]   | `GenericCancelableCommand(...)`                                        | cancelable delegate binding     |
|  [07]   | `CancelableCommandRecorder.PushCommand` / `ExecuteCommand`             | enqueue / execute-and-enqueue   |
|  [08]   | `CancelableCommandRecorder.Undo()` / `Redo()` / `Clear()`              | pop-and-apply inverse / forward |
|  [09]   | `CancelableCommandRecorder.CanUndo` / `CanRedo` / `MaxCommand`         | undo/redo state + window bound  |
|  [10]   | `CancelableCommandRecorder.GetUndoQueue() : ReadOnlyCollection<…>`     | undo queue snapshot             |
|  [11]   | `CancelableCommandRecorder.GetRedoQueue() : ReadOnlyCollection<…>`     | redo queue snapshot             |
|  [12]   | `CancelableCommandRecorder.OnNewCommandAdded`                          | enqueue event                   |
|  [13]   | `CancelableCommandRecorder.OnCommandRedo`                              | redo event                      |
|  [14]   | `CancelableCommandRecorder.OnCommandCanceled`                          | cancellation event              |
|  [15]   | `CancelableCommandRecorder.OnCommandCleared`                           | clear event                     |
|  [16]   | `CommandHistoryViewModel.UndoCommand` / `RedoCommand` / `ClearCommand` | bindable undo/redo/clear        |

`GenericCancelableCommand(name, executeFunc, cancelFunc, canExecuteFunc?, canCancelFunc?)` binds forward and inverse as `Func<bool>?`.

[COLLECTION_ENTRYPOINTS]: `ICheckedList` operations — `PropertyModels.Collections`

| [INDEX] | [SURFACE]                                    | [CAPABILITY]         |
| :-----: | :------------------------------------------- | :------------------- |
|  [01]   | `IsChecked(object)`                          | check state query    |
|  [02]   | `SetChecked(object, bool)`                   | check state write    |
|  [03]   | `SetRangeChecked(IEnumerable<object>, bool)` | batch check          |
|  [04]   | `Select(object)` / `SelectRange(...)`        | selection set        |
|  [05]   | `SelectionChanged`                           | change event         |
|  [06]   | `Items` / `SourceItems`                      | selected / all items |

[LOCALIZATION_ENTRYPOINTS]: `ILocalizationService` culture management

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]             |
| :-----: | :-------------------------------------------------------------- | :----------------------- |
|  [01]   | `this[string key]`                                              | key translation          |
|  [02]   | `SelectCulture(string cultureName)`                             | culture switch           |
|  [03]   | `GetCultures() : ICultureData[]`                                | available cultures       |
|  [04]   | `CultureData`                                                   | current culture metadata |
|  [05]   | `AddExtraService` / `RemoveExtraService` / `GetExtraServices()` | composite localization   |
|  [06]   | `OnCultureChanged`                                              | culture change event     |

[GRID_ENTRYPOINTS]: property-grid state and layout — `Avalonia.PropertyGrid.Controls.PropertyGrid`

The inspected object binds through `DataContext`; no public `ViewModel` property exists.

| [INDEX] | [SURFACE]                                            | [CAPABILITY]          |
| :-----: | :--------------------------------------------------- | :-------------------- |
|  [01]   | `DataContext`                                        | inspected object      |
|  [02]   | `IsReadOnly`                                         | read-only state       |
|  [03]   | `LayoutStyle` (`PropertyGridLayoutStyle`)            | layout mode           |
|  [04]   | `CategoryOrderStyle` (`PropertyGridOrderStyle`)      | category ordering     |
|  [05]   | `PropertyOrderStyle` (`PropertyGridOrderStyle`)      | property ordering     |
|  [06]   | `IsCategoryVisible`                                  | category display      |
|  [07]   | `IsTitleVisible`                                     | title display         |
|  [08]   | `IsHeaderVisible`                                    | header display        |
|  [09]   | `IsQuickFilterVisible`                               | quick filter          |
|  [10]   | `PropertyOperationVisibility` (`PropertyVisibility`) | operation display     |
|  [11]   | `CellEditAlignment` (`CellEditAlignmentType`)        | editor alignment      |
|  [12]   | `IsAutoNameWidth`                                    | automatic name width  |
|  [13]   | `NameWidth`                                          | explicit name width   |
|  [14]   | `AllCategoriesExpanded`                              | expansion state       |
|  [15]   | `TopHeaderContent`                                   | top chrome content    |
|  [16]   | `MiddleContent`                                      | middle chrome content |
|  [17]   | `BottomContent`                                      | bottom chrome content |

[FACTORY_ENTRYPOINTS]: `ICellEditFactory` contract — match, create, refresh, and read/write one cell editor.

| [INDEX] | [SURFACE]                                                                                                   | [CAPABILITY]             |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :----------------------- |
|  [01]   | `Accept(object accessToken) : bool`                                                                         | editor match             |
|  [02]   | `ImportPriority : int`                                                                                      | match priority           |
|  [03]   | `HandleNewProperty(PropertyCellContext) : Control?`                                                         | editor creation          |
|  [04]   | `HandlePropertyChanged(PropertyCellContext) : bool`                                                         | editor refresh           |
|  [05]   | `HandleReadOnlyStateChanged(Control, bool)`                                                                 | read-only refresh        |
|  [06]   | `HandlePropagateVisibility(object?, PropertyCellContext, IPropertyGridFilterContext) : PropertyVisibility?` | filter-driven visibility |
|  [07]   | `SetPropertyValue(PropertyCellContext, object?)`                                                            | command-routed write     |
|  [08]   | `GetPropertyValue(PropertyCellContext) : object?`                                                           | value read               |
|  [09]   | `Clone() : ICellEditFactory?`                                                                               | per-cell factory clone   |

[FACTORY_REGISTRY]: `ICellEditFactoryCollection` operations, resolved through `CellEditFactoryService.Default`.

| [INDEX] | [SURFACE]                         | [CAPABILITY]         |
| :-----: | :-------------------------------- | :------------------- |
|  [01]   | `Factories`                       | registered factories |
|  [02]   | `AddFactory(ICellEditFactory)`    | factory registration |
|  [03]   | `RemoveFactory(ICellEditFactory)` | factory removal      |
|  [04]   | `CloneFactories(object)`          | factory snapshot     |

[EVENT_ENTRYPOINTS]: routed inspector event surfaces — `PropertyGrid`

| [INDEX] | [SURFACE]                                | [CAPABILITY]                          |
| :-----: | :--------------------------------------- | :------------------------------------ |
|  [01]   | `CustomPropertyDescriptorFilter`         | descriptor filter (`RoutedEventArgs`) |
|  [02]   | `CustomNameBlock`                        | name rendering                        |
|  [03]   | `CustomPropertyOperationControl`         | operation surface                     |
|  [04]   | `CustomPropertyOperationMenuOpening`     | operation menu                        |
|  [05]   | `CommandExecuting` / `CommandExecuted`   | command gate / receipt                |
|  [06]   | `PropertyGotFocus` / `PropertyLostFocus` | focus receipts                        |
|  [07]   | `NameWidthChanged`                       | name-column width change              |

[EDITOR_ENTRYPOINTS]: list, color, and slider editor operations

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------ | :------------------------------- |
|  [01]   | `ListEdit.NewElementCommand` / `ClearElementsCommand`         | list add / clear                 |
|  [02]   | `ListViewModel.InsertCommand` / `RemoveCommand`               | list insert / remove             |
|  [03]   | `PreviewableColorPicker.ColorChanged` / `PreviewColorChanged` | committed / preview color        |
|  [04]   | `PreviewableSlider.RealValueChanged` / `PreviewValueChanged`  | committed / preview slider value |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `PropertyModels` is the host-neutral model substrate; `Avalonia.PropertyGrid` projects the object bound through `DataContext` as typed editor rows, selecting each editor by `ICellEditFactory.Accept(object accessToken)` match ranked by `ImportPriority` (higher first) through `CellEditFactoryService.Default : ICellEditFactoryCollection`.
- Visibility, ordering, and editor presentation drive off data-annotation attributes on the model, never reflection-UI-as-model: `[ConditionTarget]` + `[PropertyVisibilityCondition]` + `[DependsOnProperty]` gate visibility and dependency without code-behind, `ReactiveObject` propagating `[DependsOnProperty]` automatically; `Accept` takes the `object` access token, `PropertyCellContext` arriving on `HandleNewProperty`/`HandlePropertyChanged`.

[STACKING]:
- `ReactiveUI`(`.api/api-reactiveui.md`): `PropertyModels.ReactiveObject`/`MiniReactiveObject` is the inspected-model base, distinct from ReactiveUI's `ReactiveObject` screen base — an inspected view-model derives from the PropertyModels base for `[DependsOnProperty]`/`SetProperty`, its hosting screen from the ReactiveUI base; `CancelableCommandRecorder` records undo/redo beside ReactiveUI `ReactiveCommand` execution, never replacing it.
- `Dock.Avalonia`(`.api/api-dock.md`): `PropertyGrid` mounts as a `Tool`/`Document` `Content` in the `IDock` graph, its `DataContext` the inspected object marshaled onto the UI thread.
- within-lib: application view-models inherit `ReactiveObject`/`MiniReactiveObject`; command pipelines compose `CancelableCommandRecorder` + `CommandHistoryViewModel`; multi-select editors back onto `CheckedList`/`CheckedMaskModel`.

[LOCAL_ADMISSION]:
- A custom editor is a public `AbstractCellEditFactory` subclass registered through `CellEditFactoryService.Default.AddFactory(factory)` with an `ImportPriority` above the built-in it overrides.
- Editor-hint attributes live in `PropertyModels.ComponentModel`, validation/condition/enum-filter attributes in `PropertyModels.ComponentModel.DataAnnotations`; each attribute routes to its real namespace and is never re-declared locally.

[RAIL_LAW]:
- Package: `bodong.PropertyModels`
- Owns: reactive base objects, the cancelable command/undo recorder, checked/selectable/checked-mask collections, editor-hint and data-annotation attributes, localization contracts, and descriptor extensions.
- Accept: view-models inherit `ReactiveObject`/`MiniReactiveObject`; command pipelines route through `CancelableCommandRecorder`, which owns pop-and-apply — a revert resolving an op without driving `Undo()`/`Redo()` leaves the inverse unapplied.
- Reject: hand-rolled `INotifyPropertyChanged`, per-screen undo stacks, string-keyed property registries.
- Package: `bodong.Avalonia.PropertyGrid`
- Owns: typed property inspection, the editor-factory registry, property operations, list editing, localization services, and routed inspector events.
- Accept: every inspecting surface binds its object through the control's `DataContext` and projects state through typed rows, factories, filters, commands, and routed events; layout, order, and visibility drive off the `PropertyGridLayoutStyle`/`PropertyGridOrderStyle`/`PropertyVisibility`/`CellEditAlignmentType` enums.
- Reject: reflection UI as public model; referencing the internal `PropertyGridViewModel` or the built-in concrete factories instead of subclassing `AbstractCellEditFactory`; assuming a public `ViewModel` property where the binding is `DataContext`.
