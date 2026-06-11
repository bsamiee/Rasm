# [RASM_APPUI_API_PROPERTYGRID]

`bodong.Avalonia.PropertyGrid` supplies object inspection, property rows, editors, and categorized property surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bodong.Avalonia.PropertyGrid`
- package: `bodong.Avalonia.PropertyGrid`
- assembly: `Avalonia.PropertyGrid`
- namespace: `Avalonia.PropertyGrid`
- asset: runtime library
- rail: inspectors

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: property grid family
- rail: inspectors

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]     | [CAPABILITY]                |
| :-----: | :-------------------------- | :----------------- | :-------------------------- |
|   [1]   | `PropertyGrid`              | inspector control  | renders property rows       |
|   [2]   | `IPropertyGrid`             | inspector contract | anchors inspectors contract |
|   [3]   | `PropertyGridViewModel`     | view model         | projects inspected object   |
|   [4]   | `PropertyCellContext`       | cell context       | carries editor state        |
|   [5]   | `ICellEditFactory`          | editor factory     | creates property editors    |
|   [6]   | `CellEditFactoryCollection` | editor collection  | registers editor factories  |
|   [7]   | `PropertyGridLayoutStyle`   | layout enum        | controls layout mode        |
|   [8]   | `PropertyVisibility`        | visibility enum    | controls visible rows       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: inspector operations
- rail: inspectors

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]    | [CAPABILITY]                  |
| :-----: | :---------------------------------------- | :-------------- | :---------------------------- |
|   [1]   | `CellEditFactoryService`                  | service surface | resolves editor factories     |
|   [2]   | `LocalizationService`                     | service surface | resolves localized labels     |
|   [3]   | `CustomPropertyDescriptorFilterEventArgs` | filter event    | filters property descriptors  |
|   [4]   | `CustomPropertyOperationControlEventArgs` | event args      | controls property operations  |
|   [5]   | `CellPropertyChangedEventArgs`            | event args      | carries property cell changes |
|   [6]   | `FilterCategory`                          | filter value    | scopes property filtering     |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `bodong.Avalonia.PropertyGrid`
- Owns: typed property inspection
- Accept: inspectors project product state
- Reject: reflection UI as public model
