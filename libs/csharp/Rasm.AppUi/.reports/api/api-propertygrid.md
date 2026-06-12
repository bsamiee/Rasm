# [RASM_APPUI_API_PROPERTYGRID]

`bodong.Avalonia.PropertyGrid` supplies object inspection, property rows, editors, editor factories, filters, localization, commands, and categorized property surfaces.

## [1]-[PACKAGE_SURFACE]

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

[INSPECTOR_TYPES]: grid, context, and view model surfaces
- rail: inspectors

| [INDEX] | [SYMBOL]                     | [RAIL]             |
| :-----: | :--------------------------- | :----------------- |
|   [1]   | `PropertyGrid`               | inspector control  |
|   [2]   | `IPropertyGrid`              | inspector contract |
|   [3]   | `PropertyGridViewModel`      | inspector model    |
|   [4]   | `PropertyCellContext`        | cell context       |
|   [5]   | `IPropertyGridCellInfo`      | cell info          |
|   [6]   | `IPropertyGridFilterContext` | filter context     |
|   [7]   | `FilterCategory`             | filter value       |
|   [8]   | `ReferencePath`              | property path      |

[EDITOR_TYPES]: editor controls and list models
- rail: inspectors

| [INDEX] | [SYMBOL]                    | [RAIL]         |
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

[FACTORY_TYPES]: editor factory families
- rail: inspectors

| [INDEX] | [SYMBOL]                    | [RAIL]            |
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

[EVENT_AND_SERVICE_TYPES]: events, localization, and services
- rail: inspectors

| [INDEX] | [SYMBOL]                                  | [RAIL]               |
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

[GRID_ENTRYPOINTS]: property grid state and layout
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

[INSPECTOR_LAW]:
- Package: `bodong.Avalonia.PropertyGrid`
- Owns: typed property inspection, editor factories, property operations, list editing, localization, and inspector events
- Accept: inspectors project product state through typed rows, factories, filters, commands, and receipts
- Reject: reflection UI as public model

[EDITOR_LAW]:
- Package: `bodong.Avalonia.PropertyGrid`
- Owns: editor selection by property context and factory priority
- Accept: panels, companion windows, sidecars, diagnostics, and support views share one inspector rail
- Reject: per-screen reflection editors
