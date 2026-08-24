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

- `ICheckedMaskModel` and `CheckedMaskModel` both live in `PropertyModels.ComponentModel`, not `.Collections`: `CheckedMaskModel(IEnumerable<string> masks, string all)` with `Masks : string[]` (ordinal-sorted), `All`, `IsAllChecked`, `IsChecked(string)`, `Check(string)`, `UnCheck(string)`, the `CheckChanged` event, and a `BeginUpdate`/`EndUpdate` pair that suppresses the event across a batch — the shipped backing for a flags-enum editor.

[ANNOTATION_TYPES]: editor-hint attributes — `PropertyModels.ComponentModel`; each payload member is a readonly field unless the row marks it settable, and each targets `Property` and `Field` unless the row says otherwise.

| [INDEX] | [SYMBOL]                          | [PAYLOAD]                                                         | [CAPABILITY]                 |
| :-----: | :-------------------------------- | :---------------------------------------------------------------- | :--------------------------- |
|  [01]   | `FloatPrecisionAttribute`         | `Increment : decimal`, `FormatString`                             | float display precision      |
|  [02]   | `IntegerIncrementAttribute`       | `Increment : int`                                                 | integer editor increment     |
|  [03]   | `ProgressAttribute`               | `Minimum`, `Maximum`, `FormatString`, settable `ShowProgressText` | progress-bar editor hint     |
|  [04]   | `WatermarkAttribute`              | `Watermark : string`                                              | editor watermark text        |
|  [05]   | `TrackableAttribute`              | `Minimum`, `Maximum` + four settable knobs                        | drag-numeric editor marker   |
|  [06]   | `MultilineTextAttribute`          | `IsMultiline : bool`, ctor default true                           | multiline string editor      |
|  [07]   | `UnitAttribute`                   | `Unit : string`                                                   | unit label display           |
|  [08]   | `EnumDisplayNameAttribute`        | `DisplayName : string`, targets `Enum`/`Field`                    | per-enum-member display name |
|  [09]   | `EnumExcludeAttribute`            | marker, targets `Enum`/`Field`                                    | exclude an enum member       |
|  [10]   | `CustomPropertyOrderAttribute`    | `ICustomPropertyOrderHandler`                                     | explicit property order      |
|  [11]   | `AutoCollapseCategoriesAttribute` | `CategoryNames`, `ShouldAutoCollapse`, targets `Class`            | collapse categories on load  |
|  [12]   | `ControlClassesAttribute`         | `Classes : string[]`, `AllowMultiple`                             | Avalonia style classes       |

- `FloatPrecisionAttribute` mints from three constructors — `(int precision = 2)` derives both members from a digit count, while `(decimal, string)` and `(double, string)` take the increment and format directly.
- `TrackableAttribute`'s settable knobs are `Increment` (default `0.01`), `FormatString`, `AllowSpin`, and `ShowButtonSpinner`; its two bounds arrive through the `(double min = 0, double max = 100)` constructor.

[ANNOTATION_TYPES]: validation, condition, path, and enum-filter attributes — `PropertyModels.ComponentModel.DataAnnotations`

| [INDEX] | [SYMBOL]                                                     | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `DependsOnPropertyAttribute`                                 | reactive dependency declaration                                 |
|  [02]   | `ConditionTargetAttribute`                                   | marks a property as a visibility-condition target               |
|  [03]   | `PropertyVisibilityConditionAttribute`                       | conditional visibility (`AbstractVisibilityConditionAttribute`) |
|  [04]   | `PathBrowsableAttribute`                                     | path-browse editor (`PathBrowsableType`)                        |
|  [05]   | `FileNameValidationAttribute`                                | file-name validation                                            |
|  [06]   | `FloatingNumberEqualToleranceAttribute`                      | float equality tolerance                                        |
|  [07]   | `IEnumValueAuthorizeAttribute`                               | `AllowValue(Type, string, object) : bool`                       |
|  [08]   | `ImagePreviewModeAttribute`                                  | settable `Stretch`, `StretchDirection`                          |
|  [09]   | `EnumPermitNamesAttribute` / `EnumPermitValuesAttribute`     | enum allow-list by name / value, `AllowMultiple`                |
|  [10]   | `EnumProhibitNamesAttribute` / `EnumProhibitValuesAttribute` | enum deny-list by name / value, `AllowMultiple`                 |

- `PathBrowsableAttribute` carries `Type : PathBrowsableType` (`File`, `MultipleFiles`, `Directory`, `MultipleDirectories`) beside settable `SaveMode`, `InitialFileName`, `Filters`, `Title`, and the derived `IsFileSelection`.
- All four permit and prohibit attributes implement `IEnumValueAuthorizeAttribute`, so a consumer reads that ONE contract and folds the admitted set as a conjunction; a per-attribute ladder re-derives polymorphism the declaration already carries, and an absent filter set admits everything with no null branch.

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

[FACTORY_AND_SERVICE_TYPES]: editor-factory contract + service registries — `Avalonia.PropertyGrid.Controls` / `.Controls.Factories` / `.Services`

Namespaces below drop the shared `Avalonia.PropertyGrid.` prefix.

| [INDEX] | [SYMBOL]                               | [NAMESPACE]          | [CAPABILITY]                                         |
| :-----: | :------------------------------------- | :------------------- | :--------------------------------------------------- |
|  [01]   | `ICellEditFactory`                     | `Controls`           | editor-factory contract, ten members                 |
|  [02]   | `ICellEditFactoryCollection`           | `Controls`           | factory-set and per-cell build contract              |
|  [03]   | `AbstractCellEditFactory`              | `Controls.Factories` | editor-factory base; subclass per custom editor      |
|  [04]   | `CellEditFactoryService`               | `Services`           | static `Default : ICellEditFactoryCollection`        |
|  [05]   | `LocalizationService`                  | `Services`           | static `Default : ILocalizationService`              |
|  [06]   | `AssemblyJsonAssetLocalizationService` | `Services`           | JSON-asset service over an `Assembly` or asset `Uri` |

- The two interfaces sit in `Avalonia.PropertyGrid.Controls` while the base class sits one namespace deeper in `.Controls.Factories`, so a `using` for the base does not bring the contract into scope.
- Built-in factories are public and subclassable under `Avalonia.PropertyGrid.Controls.Factories.Builtins`, each an `AbstractCellEditFactory` unless noted: `Common`, `Boolean`, `Color`, `Collection`, `CheckedList`, `SelectableList`, `Enum`, `Expandable`, `Numeric`, `String`, `Path`, `DateTime`, `Time`, `FontFamily`, `Image`, `Progress` (extends `Numeric`), `Trackable` (extends `Numeric`) — a narrowed editor derives the nearest built-in and raises `ImportPriority`, never re-implements the base.
- `CellEditFactoryCollection` (`Avalonia.PropertyGrid.Controls.Implements`) and `PropertyGridViewModel` are the assembly-internal pair: the collection reaches consumers only as `ICellEditFactoryCollection`, and the view-model only as `IPropertyGridFilterContext` through `PropertyCellContext`.

[EVENT_TYPES]: routed inspector event args — `Avalonia.PropertyGrid.Controls`

| [INDEX] | [SYMBOL]                                                   | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `CustomPropertyDescriptorFilterEventArgs`                  | descriptor filter (`RoutedEventArgs`)     |
|  [02]   | `CustomNameBlockEventArgs`                                 | name-column replacement                   |
|  [03]   | `CustomPropertyOperationControlEventArgs`                  | operation-column replacement              |
|  [04]   | `CustomPropertyDefaultOperationEventArgs`                  | default operation button and menu         |
|  [05]   | `PropertyDefaultOperationStageType`                        | `Init` / `MenuOpening` stage discriminant |
|  [06]   | `PropertyGotFocusEventArgs` / `PropertyLostFocusEventArgs` | focus events (`RoutedEventArgs`)          |
|  [07]   | `CellPropertyChangedEventArgs`                             | cell-value change (`EventArgs`)           |
|  [08]   | `RoutedCommandExecutedEventArgs`                           | command receipt (`RoutedEventArgs`)       |
|  [09]   | `RoutedCommandExecutingEventArgs`                          | command veto (row [08] subtype)           |

- `PropertyGotFocusEventArgs` / `PropertyLostFocusEventArgs` carry one readonly `Context : PropertyCellContext`; `CustomPropertyDescriptorFilterEventArgs` carries readonly `TargetObject : object` and `PropertyDescriptor : PropertyDescriptor` beside a settable `IsVisible : bool`.
- `CustomNameBlockEventArgs` carries readonly `Context`, the get-only `DefaultNameBlock : Control`, and a settable `CustomNameBlock : Control?` pre-seeded to that default, so a handler substitutes a whole name cell by assignment and a handler that reads without writing keeps the stock block.
- `CustomPropertyOperationControlEventArgs` carries readonly `Context` and a settable `CustomControl : Control?` defaulting to null: assigning replaces the operation column outright, leaving it null keeps the built-in button, which then raises `CustomPropertyDefaultOperationEventArgs` (readonly `Context` + `StageType`, get-only `DefaultButton : Button` and `Menu : ContextMenu`) twice — once at `Init` and once per `MenuOpening` — for in-place button and menu-item edits.
- `RoutedCommandExecutedEventArgs` carries readonly `Command : ICancelableCommand`, `Target : object`, `Property : PropertyDescriptor`, and `OldValue`/`NewValue`/`Context : object?`; `RoutedCommandExecutingEventArgs` EXTENDS it and adds the settable `Canceled : bool`, so a handler narrowing to the base type accepts the veto edge and never reaches `Canceled`.

[MODEL_SYNTHESIS_TYPES]: descriptor synthesis over a bound instance — `PropertyModels.Utils` / `.ComponentModel` / `.ComponentModel.DataAnnotations`

| [INDEX] | [SYMBOL]                        | [NAMESPACE]                       | [CAPABILITY]                                            |
| :-----: | :------------------------------ | :-------------------------------- | :------------------------------------------------------ |
|  [01]   | `PropertyDescriptorBuilder`     | `PropertyModels.Utils`            | descriptor set over one target or an `IEnumerable` set  |
|  [02]   | `MultiObjectPropertyDescriptor` | `PropertyModels.ComponentModel`   | one descriptor fanning writes across a merged selection |
|  [03]   | `ListElementPropertyDescriptor` | `PropertyModels.ComponentModel`   | per-element descriptor inside a collection editor       |
|  [04]   | `DependsOnPropertyAttribute`    | `…ComponentModel.DataAnnotations` | derived-member dependency, `AllowMultiple`              |

- `MultiObjectPropertyDescriptor : PropertyDescriptor, IEnumerable<PropertyDescriptor>`: `this[int]` indexer and `GetEnumerator()` expose the merged descriptor set (`Descriptors` itself is internal); `PropertyType`, `ComponentType`, `Converter`, `DisplayName`, and `Category` all read descriptor zero, and `IsReadOnly` is true when ANY child is.
- `GetValue(object? component)` folds the per-target values and returns NULL the moment two disagree, which is the same answer a uniformly null value gives — a mixed-state read therefore takes `GetValues(object[] components) : object?[]` and folds distinctness itself.
- `SetValue(object?, object?)` fans one write across every child descriptor (cloning the value first when it is `ICloneable`), so an apply-to-all is ONE call through the factory write path rather than a per-target loop; `ResetValue` fans the same way and `SetValues(object components, object values)` pairs two equal-length arrays or throws `ArgumentOutOfRangeException`.
- The component argument's shape is asymmetric: `GetValue` accepts any `IEnumerable` while `SetValue`, `ResetValue`, and `SetValues` hard-cast to `object[]`, and `GetValues` allocates over `components.Length` while looping `Descriptors.Length` — so the bound multi-target instance crosses as an `object[]` whose length equals the descriptor count.

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
|  [17]   | `PropertyDescriptorBuilder(object target)`                             | synthesis over one bound target |
|  [18]   | `PropertyDescriptorBuilder.GetProperties()`                            | `PropertyDescriptorCollection`  |
|  [19]   | `PropertyDescriptorBuilder.IsMultipleObjects` / `Target`               | readonly fields, not properties |
|  [20]   | `PropertyDescriptorBuilder.AllowMerge(PropertyDescriptor) : bool`      | STATIC merge admission          |

`GenericCancelableCommand(name, executeFunc, cancelFunc, canExecuteFunc?, canCancelFunc?)` binds forward and inverse as `Func<bool>?`.

- `PropertyDescriptorBuilder` synthesizes over ONE live instance: an `IEnumerable` target sets `IsMultipleObjects` and merges only the descriptors `AllowMerge` admits (the BCL `MergablePropertyAttribute`, default allowing) into `MultiObjectPropertyDescriptor` rows, dropping the rest. There is NO immutable-value facility anywhere in the package — every write is `PropertyDescriptor.SetValue(component, value)` in place, so an inspected record edits through a mutable draft over `MiniReactiveObject`/`ReactiveObject` and the record rebuilds from that draft at commit.
- `ReactiveObject` reads `[DependsOnProperty(nameof(...))]` at construction and raises each dependent member off its dependency's change, so a derived property notifies with nothing hand-raised; `MiniReactiveObject` carries no dependency propagation.

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

`DataContext` binds the inspected object; no public `ViewModel` property exists.

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

- `PropertyGrid` derives `UserControl` and loads `avares://Avalonia.PropertyGrid/Controls/PropertyGrid.axaml`, keeping its named parts as internal fields: `TopHeaderArea`, `InternalHeaderGrid`, `SearchTextBox`, `OptionsButton`, `FastFilterBox`, `MiddleArea`, `SplitterGrid`, `ColumnName`, `ColumnProperties`, `PropertiesGrid`, `BottomArea`, `OptionsContextMenu`.
- Descendant-plus-name selectors reach every one of those parts (`PropertyGrid TextBox#SearchTextBox`), matching the selectors the package's own styles use; code and a `ControlTheme` on `PropertyGrid` reach none of them.
- `PropertyGrid.axaml` injects `/Themes/Fluent/Fluent.xaml` and `/Themes/Simple/Simple.xaml` into `PropertyGrid.Styles` unconditionally and declares two resource keys, `ExpanderContentPadding` and `EnumToBooleanConverter` — zero brushes.
- Color resolves at runtime from the ambient app theme with hardcoded fallbacks: `SystemAccentColor` and `SystemControlPageTextBaseMediumBrush` off `Application.Current.ActualThemeVariant`, `ColorControlDarkSelectorBrush` off the app resource tree. `PropertyGrid` tracks a theme swap unaided and exposes no brush key to recolor it, while the row-label foreground resolves once in a static ctor and holds through a runtime variant change.

[FACTORY_ENTRYPOINTS]: `ICellEditFactory` contract — match, create, refresh, and read/write one cell editor. Rows [05], [06], and [09] carry a base implementation a custom factory usually must displace.

| [INDEX] | [SURFACE]                                            | [CAPABILITY]             | [BASE_BODY]                                  |
| :-----: | :--------------------------------------------------- | :----------------------- | :------------------------------------------- |
|  [01]   | `Accept(object accessToken) : bool`                  | clone admission          | `accessToken is PropertyGrid`                |
|  [02]   | `ImportPriority : int`                               | match priority           | `100`                                        |
|  [03]   | `HandleNewProperty(PropertyCellContext) : Control?`  | editor creation          | abstract                                     |
|  [04]   | `HandlePropertyChanged(PropertyCellContext) : bool`  | editor refresh           | abstract                                     |
|  [05]   | `HandleReadOnlyStateChanged(Control, bool)`          | read-only refresh        | `control.IsEnabled = !readOnly` on the root  |
|  [06]   | `HandlePropagateVisibility(…) : PropertyVisibility?` | filter-driven visibility | `null`, deferring to the default match       |
|  [07]   | `SetPropertyValue(PropertyCellContext, object?)`     | command-routed write     | `SetAndRaise(context, CellEdit, value, old)` |
|  [08]   | `GetPropertyValue(PropertyCellContext) : object?`    | value read               | `context.GetValue()`                         |
|  [09]   | `Clone() : ICellEditFactory?`                        | per-cell factory clone   | `Activator.CreateInstance(GetType())`        |
|  [10]   | `Collection : ICellEditFactoryCollection?`           | owning set               | `internal set`, the collection assigns it    |

- Row [06] spells in full as `HandlePropagateVisibility(object? target, PropertyCellContext context, IPropertyGridFilterContext filterContext, string? filterText = null, bool filterMatchesParentCategory = false)`.

- `Accept` gates CLONING, never cell building: `BuildPropertyControl` walks the priority-ordered set calling `HandleNewProperty` and takes the first non-null answer without consulting `Accept` at all, while `CloneFactories(accessToken)` filters on `Accept` before cloning each factory into a grid's own set. A custom `Accept` narrowing to the inspected TYPE therefore returns false for the grid access token the package passes, and the factory never enters any grid's set — shape selection belongs in `HandleNewProperty` and the base predicate stands.
- `Clone`'s base mints through `Activator.CreateInstance(GetType())`, so a factory whose constructor takes a dependency MUST override it or throw `MissingMethodException` at the first clone.
- `HandlePropagateVisibility` carries two optional parameters beyond the three the contract leads with; an override omitting them does not match the virtual and silently shadows nothing.
- Protected extension points on the base: `CheckIsPropertyChanged`, `SetAndRaise`, `ExecuteCommand`, `HandleSetValue`, `HandleRaiseEvent`, `ValidateProperty`, `CheckEqual`/`CheckEquals`/`ArrayToString`.

[CELL_CONTEXT]: `PropertyCellContext` — the one argument every factory member above receives.

| [INDEX] | [SURFACE]                              | [CAPABILITY]                                          |
| :-----: | :------------------------------------- | :---------------------------------------------------- |
|  [01]   | `Property : PropertyDescriptor`        | descriptor channel; `PropertyType` selects the editor |
|  [02]   | `Target : object`                      | the bound instance every write lands on               |
|  [03]   | `GetValue() : object?`                 | value channel, reading `Property.GetValue(Target)`    |
|  [04]   | `CellEdit : Control?`                  | the materialized editor, settable                     |
|  [05]   | `Factory : ICellEditFactory?`          | the factory that materialized it, settable            |
|  [06]   | `IsReadOnly : bool`                    | per-cell write admission                              |
|  [07]   | `Root` / `Owner : IPropertyGrid`       | routed-event raiser and root grid                     |
|  [08]   | `ParentContext : PropertyCellContext?` | enclosing cell for nested descriptors                 |
|  [09]   | `GetCellEditFactoryCollection()`       | the resolving factory set                             |

- `SetPropertyValue` is the ONE write path a presented editor commits through: it mints a `GenericCancelableCommand` per changed cell, raises `PropertyGrid.CommandExecuting` (cancellable), executes, then raises `PropertyGrid.CommandExecuted` — all inside one synchronous frame — so a control writing `context.Property.SetValue(context.Target, value)` directly bypasses the veto edge, the receipt edge, and the undo recorder at once. `GetPropertyValue` is `context.GetValue()`.

[FACTORY_REGISTRY]: `ICellEditFactoryCollection` operations, resolved through `CellEditFactoryService.Default`.

| [INDEX] | [SURFACE]                                              | [CAPABILITY]         |
| :-----: | :----------------------------------------------------- | :------------------- |
|  [01]   | `Factories`                                            | registered factories |
|  [02]   | `AddFactory(ICellEditFactory)`                         | factory registration |
|  [03]   | `RemoveFactory(ICellEditFactory)`                      | factory removal      |
|  [04]   | `CloneFactories(object)`                               | factory snapshot     |
|  [05]   | `BuildPropertyControl(PropertyCellContext) : Control?` | one cell's editor    |

- `CellEditFactoryService.Default` self-populates in its static ctor, reflecting every non-abstract `ICellEditFactory` in the assembly, and `AddFactory` re-sorts the set by descending `ImportPriority` and back-links `ICellEditFactory.Collection` — an added factory needs no ordering call and reads its owning set off `Collection`.
- `BuildPropertyControl` walks that priority order to the first factory returning a control, then reads every `[ControlClasses(params string[])]` on the cell's `PropertyDescriptor`, distincts the union, and `Classes.AddRange`s it onto the returned editor before binding `CellEdit`/`Factory` — the one per-row style hook, so a per-property editor restyle carries one attribute with a `Style` on `.<class>`, never a factory subclass.

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
- `PropertyGrid` builds one category `Expander` in code per category and pins `Background` (null), `Margin`, `Padding`, and `HeaderTemplate` (a `FuncDataTemplate` minting a `TextBlock` at `FontWeight` 700) through CLR setters at `BindingPriority.LocalValue`, outranking every `Style`, `ControlTheme`, and theme-variant setter aimed at those four.
- `Template` stays unpinned, so a category restyle rides a `ControlTheme` or `Template` setter on `Expander`: the replacement template binds `Header` directly to escape the pinned header template and paints its own chrome to escape the null background, while `Margin` sits outside the template and holds.
- Each Expander subscribes `TemplateApplied` at construction and force-writes the left `Margin` of its `PART_ContentPresenter` to `8.0` on every application, so a replacement template naming its presenter `PART_ContentPresenter` re-inherits that indent and one under any other name escapes it.

[STACKING]:
- `ReactiveUI`(`.api/api-reactiveui.md`): `PropertyModels.ReactiveObject`/`MiniReactiveObject` is the inspected-model base, distinct from ReactiveUI's `ReactiveObject` screen base — an inspected view-model derives from the PropertyModels base for `[DependsOnProperty]`/`SetProperty`, its hosting screen from the ReactiveUI base; `CancelableCommandRecorder` records undo/redo beside ReactiveUI `ReactiveCommand` execution, never replacing it.
- `Dock.Avalonia`(`.api/api-dock.md`): `PropertyGrid` mounts as a `Tool`/`Document` `Content` in the `IDock` graph, its `DataContext` the inspected object marshaled onto the UI thread.
- within-lib: application view-models inherit `ReactiveObject`/`MiniReactiveObject`; command pipelines compose `CancelableCommandRecorder` + `CommandHistoryViewModel`; multi-select editors back onto `CheckedList`/`CheckedMaskModel`.

[LOCAL_ADMISSION]:
- Custom editors subclass the public `AbstractCellEditFactory`, register through `CellEditFactoryService.Default.AddFactory(factory)`, and carry an `ImportPriority` above the built-in each one overrides.
- Editor-hint attributes live in `PropertyModels.ComponentModel`, validation/condition/enum-filter attributes in `PropertyModels.ComponentModel.DataAnnotations`; each attribute routes to its real namespace and is never re-declared locally.
- Chrome restyling routes by target: grid parts take a descendant-plus-name `Style`, a per-row editor takes `[ControlClasses]` with a class `Style`, a category header or body takes an `Expander` `ControlTheme`, and a whole name or operation cell takes the `CustomNameBlock`/`CustomPropertyOperationControl` handler returning a replacement `Control`.
- Recoloring the grid sets the ambient theme keys the host already owns — `SystemAccentColor`, `SystemControlPageTextBaseMediumBrush`, `ColorControlDarkSelectorBrush` — never a package-local brush override.

[RAIL_LAW]:
- Package: `bodong.PropertyModels`
- Owns: reactive base objects, the cancelable command/undo recorder, checked/selectable/checked-mask collections, editor-hint and data-annotation attributes, localization contracts, and descriptor extensions.
- Accept: view-models inherit `ReactiveObject`/`MiniReactiveObject`; command pipelines route through `CancelableCommandRecorder`, which owns pop-and-apply — a revert resolving an op without driving `Undo()`/`Redo()` leaves the inverse unapplied.
- Reject: hand-rolled `INotifyPropertyChanged`, per-screen undo stacks, string-keyed property registries.
- Package: `bodong.Avalonia.PropertyGrid`
- Owns: typed property inspection, the editor-factory registry, property operations, list editing, localization services, and routed inspector events.
- Accept: every inspecting surface binds its object through the control's `DataContext` and projects state through typed rows, factories, filters, commands, and routed events; layout, order, and visibility drive off the `PropertyGridLayoutStyle`/`PropertyGridOrderStyle`/`PropertyVisibility`/`CellEditAlignmentType` enums.
- Reject: reflection UI as public model; referencing the internal `PropertyGridViewModel` or `CellEditFactoryCollection` instead of `IPropertyGridFilterContext` and `ICellEditFactoryCollection`; assuming a public `ViewModel` property where the binding is `DataContext`; a `Style` or `ControlTheme` setter aimed at the category `Expander`'s pinned `Background`/`Margin`/`Padding`/`HeaderTemplate`; a package-local brush key where the grid reads ambient theme resources.
