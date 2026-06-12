# [RASM_APPUI_API_AVALONIA]

`Avalonia` supplies the retained UI object model for shell, screen, command, theme, resource, input, and visual state.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia`
- package: `Avalonia`
- assembly: `Avalonia.Base`
- assembly: `Avalonia.Controls`
- assembly: `Avalonia.Markup.Xaml`
- assembly: `Avalonia.Dialogs`
- namespace: `Avalonia`
- namespace: `Avalonia.Controls`
- namespace: `Avalonia.Data`
- namespace: `Avalonia.Input`
- namespace: `Avalonia.Interactivity`
- namespace: `Avalonia.Markup.Xaml`
- asset: runtime libraries
- rail: retained-ui

## [2]-[PUBLIC_TYPES]

[BASE_OBJECTS]: retained property and element model
- rail: retained-ui

| [INDEX] | [SYMBOL]                           | [RAIL]             |
| :-----: | :--------------------------------- | :----------------- |
|   [1]   | `AvaloniaObject`                   | property owner     |
|   [2]   | `AvaloniaProperty`                 | property identity  |
|   [3]   | `AvaloniaProperty<TValue>`         | typed property     |
|   [4]   | `StyledProperty<TValue>`           | inherited property |
|   [5]   | `DirectPropertyBase<TValue>`       | direct property    |
|   [6]   | `AvaloniaPropertyMetadata`         | property metadata  |
|   [7]   | `AvaloniaPropertyRegistry`         | property registry  |
|   [8]   | `AvaloniaPropertyChangedEventArgs` | change event       |

[ELEMENT_TREE]: styled, logical, visual, and layout participation
- rail: retained-ui

| [INDEX] | [SYMBOL]        | [RAIL]               |
| :-----: | :-------------- | :------------------- |
|   [1]   | `StyledElement` | style participant    |
|   [2]   | `Visual`        | visual tree node     |
|   [3]   | `Interactive`   | routed input node    |
|   [4]   | `InputElement`  | focus input node     |
|   [5]   | `Layoutable`    | measure arrangement  |
|   [6]   | `ILogical`      | logical tree node    |
|   [7]   | `IVisual`       | visual tree contract |
|   [8]   | `IResourceHost` | resource owner       |

[CONTROL_SURFACES]: product surface and shell controls
- rail: retained-ui

| [INDEX] | [SYMBOL]         | [RAIL]              |
| :-----: | :--------------- | :------------------ |
|   [1]   | `Application`    | application root    |
|   [2]   | `AppBuilder`     | application builder |
|   [3]   | `TopLevel`       | host root           |
|   [4]   | `Window`         | window shell        |
|   [5]   | `UserControl`    | screen surface      |
|   [6]   | `Panel`          | layout surface      |
|   [7]   | `ContentControl` | content host        |
|   [8]   | `ItemsControl`   | item host           |
|   [9]   | `Button`         | command surface     |
|  [10]   | `TextBox`        | text entry surface  |
|  [11]   | `TreeView`       | hierarchy surface   |
|  [12]   | `DataGrid`       | tabular surface     |

[STATE_AND_STYLE]: binding, resources, styles, and templates
- rail: retained-ui

| [INDEX] | [SYMBOL]              | [RAIL]             |
| :-----: | :-------------------- | :----------------- |
|   [1]   | `BindingBase`         | binding root       |
|   [2]   | `ReflectionBinding`   | reflection binding |
|   [3]   | `CompiledBinding`     | compiled binding   |
|   [4]   | `MultiBinding`        | composite binding  |
|   [5]   | `TemplateBinding`     | template binding   |
|   [6]   | `BindingNotification` | binding result     |
|   [7]   | `ResourceDictionary`  | resource scope     |
|   [8]   | `Styles`              | style collection   |
|   [9]   | `Style`               | selector style     |
|  [10]   | `Setter`              | styled assignment  |
|  [11]   | `ControlTheme`        | theme record       |
|  [12]   | `DataTemplate`        | data presentation  |

[METADATA_ATTRIBUTES]: XAML and template metadata
- rail: retained-ui

| [INDEX] | [SYMBOL]                        | [RAIL]            |
| :-----: | :------------------------------ | :---------------- |
|   [1]   | `PseudoClassesAttribute`        | style metadata    |
|   [2]   | `TemplatePartAttribute`         | template metadata |
|   [3]   | `AssignBindingAttribute`        | binding metadata  |
|   [4]   | `ContentAttribute`              | XAML content      |
|   [5]   | `TemplateContentAttribute`      | template content  |
|   [6]   | `ControlTemplateScopeAttribute` | template scope    |

## [3]-[ENTRYPOINTS]

[PROPERTY_OPERATIONS]: retained property operations
- rail: retained-ui

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]             | [RAIL]            |
| :-----: | :--------------------- | :------------------------- | :---------------- |
|   [1]   | `Register<T,U>`        | `AvaloniaProperty`         | styled property   |
|   [2]   | `RegisterDirect<T,U>`  | `AvaloniaProperty`         | direct property   |
|   [3]   | `Register`             | `AvaloniaPropertyRegistry` | property registry |
|   [4]   | `Bind`                 | `AvaloniaObject`           | state binding     |
|   [5]   | `GetObservable`        | `AvaloniaObjectExtensions` | state stream      |
|   [6]   | `GetBindingObservable` | `AvaloniaObjectExtensions` | binding stream    |

[ASSET_LOOKUP_OPERATIONS]: resource and name lookup
- rail: retained-ui

| [INDEX] | [SURFACE]         | [SURFACE_ROOT]           | [RAIL]             |
| :-----: | :---------------- | :----------------------- | :----------------- |
|   [1]   | `FindResource`    | `ResourceNodeExtensions` | resource lookup    |
|   [2]   | `TryFindResource` | `ResourceNodeExtensions` | guarded lookup     |
|   [3]   | `Register`        | `INameScope`             | name ownership     |
|   [4]   | `Find`            | `INameScope`             | named lookup       |
|   [5]   | `Add`             | `Styles`                 | style admission    |
|   [6]   | `Add`             | `ResourceDictionary`     | resource admission |

[INPUT_AND_ROUTE_OPERATIONS]: focus, routed event, and command surfaces
- rail: retained-ui

| [INDEX] | [SURFACE]       | [SURFACE_ROOT]          | [RAIL]          |
| :-----: | :-------------- | :---------------------- | :-------------- |
|   [1]   | `Focus`         | `IInputElement`         | focus movement  |
|   [2]   | `Focus`         | `FocusManager`          | focus ownership |
|   [3]   | `Register`      | `RoutedEventRegistry`   | event admission |
|   [4]   | `GetObservable` | `InteractiveExtensions` | event stream    |
|   [5]   | `AddHandler`    | `Interactive`           | event handling  |
|   [6]   | `RemoveHandler` | `Interactive`           | handler removal |

[XAML_AND_RENDER_OPERATIONS]: XAML load and visual invalidation
- rail: retained-ui

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]        | [RAIL]           |
| :-----: | :------------------ | :-------------------- | :--------------- |
|   [1]   | `Configure<TApp>`   | `AppBuilder`          | application root |
|   [2]   | `Configure`         | `AppBuilder`          | application root |
|   [3]   | `Load`              | `AvaloniaXamlLoader`  | XAML materialize |
|   [4]   | `InvalidateVisual`  | `Visual`              | render refresh   |
|   [5]   | `InvalidateMeasure` | `Layoutable`          | layout refresh   |
|   [6]   | `AttachDevTools`    | diagnostics extension | diagnostics      |

## [4]-[IMPLEMENTATION_LAW]

[OBJECT_MODEL_LAW]:
- Package: `Avalonia`
- Owns: retained object, property, style, resource, input, and render contracts
- Accept: product UI concepts enter through typed retained surfaces
- Reject: untyped wrapper layers over controls, properties, resources, or events

[SHELL_LAW]:
- Package: `Avalonia`
- Owns: application roots, top levels, windows, screens, and panels
- Accept: host, sidecar, companion, diagnostics, and downstream shells share one UI rail
- Reject: separate UI families per host modality

[XAML_LAW]:
- Package: `Avalonia`
- Owns: XAML load, style include, resource include, template metadata, and namescope identity
- Accept: generated and handwritten surfaces share the same namescope and resource rail
- Reject: generated XAML routes outside the retained UI object model
