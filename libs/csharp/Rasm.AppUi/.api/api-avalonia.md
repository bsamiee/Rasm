# [RASM_APPUI_API_AVALONIA]

`Avalonia` supplies the retained UI object model for shell, screen, command, theme, resource, input, and visual state.

## [01]-[PACKAGE_SURFACE]

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
- namespace: `Avalonia.Input.Platform`
- namespace: `Avalonia.Controls.Notifications`
- namespace: `Avalonia.Interactivity`
- namespace: `Avalonia.Markup.Xaml`
- asset: runtime libraries
- rail: retained-ui

## [02]-[PUBLIC_TYPES]

[BASE_OBJECTS]: retained property and element model
- rail: retained-ui

| [INDEX] | [SYMBOL]                           | [RAIL]             |
| :-----: | :--------------------------------- | :----------------- |
|  [01]   | `AvaloniaObject`                   | property owner     |
|  [02]   | `AvaloniaProperty`                 | property identity  |
|  [03]   | `AvaloniaProperty<TValue>`         | typed property     |
|  [04]   | `StyledProperty<TValue>`           | inherited property |
|  [05]   | `DirectPropertyBase<TValue>`       | direct property    |
|  [06]   | `AvaloniaPropertyMetadata`         | property metadata  |
|  [07]   | `AvaloniaPropertyRegistry`         | property registry  |
|  [08]   | `AvaloniaPropertyChangedEventArgs` | change event       |

[ELEMENT_TREE]: styled, logical, visual, and layout participation
- rail: retained-ui

| [INDEX] | [SYMBOL]        | [RAIL]              |
| :-----: | :-------------- | :------------------ |
|  [01]   | `StyledElement` | style participant   |
|  [02]   | `Visual`        | visual tree node    |
|  [03]   | `Interactive`   | routed input node   |
|  [04]   | `InputElement`  | focus input node    |
|  [05]   | `Layoutable`    | measure arrangement |
|  [06]   | `ILogical`      | logical tree node   |
|  [07]   | `IResourceHost` | resource owner      |

[CONTROL_SURFACES]: product surface and shell controls
- rail: retained-ui

| [INDEX] | [SYMBOL]         | [RAIL]              |
| :-----: | :--------------- | :------------------ |
|  [01]   | `Application`    | application root    |
|  [02]   | `AppBuilder`     | application builder |
|  [03]   | `TopLevel`       | host root           |
|  [04]   | `Window`         | window shell        |
|  [05]   | `UserControl`    | screen surface      |
|  [06]   | `Panel`          | layout surface      |
|  [07]   | `ContentControl` | content host        |
|  [08]   | `ItemsControl`   | item host           |
|  [09]   | `Button`         | command surface     |
|  [10]   | `TextBox`        | text entry surface  |
|  [11]   | `TreeView`       | hierarchy surface   |

[STATE_AND_STYLE]: binding, resources, styles, and templates
- rail: retained-ui

| [INDEX] | [SYMBOL]              | [RAIL]             |
| :-----: | :-------------------- | :----------------- |
|  [01]   | `BindingBase`         | binding root       |
|  [02]   | `ReflectionBinding`   | reflection binding |
|  [03]   | `CompiledBinding`     | compiled binding   |
|  [04]   | `MultiBinding`        | composite binding  |
|  [05]   | `TemplateBinding`     | template binding   |
|  [06]   | `BindingNotification` | binding result     |
|  [07]   | `ResourceDictionary`  | resource scope     |
|  [08]   | `Styles`              | style collection   |
|  [09]   | `Style`               | selector style     |
|  [10]   | `Setter`              | styled assignment  |
|  [11]   | `ControlTheme`        | theme record       |
|  [12]   | `DataTemplate`        | data presentation  |

[METADATA_ATTRIBUTES]: XAML and template metadata
- rail: retained-ui

| [INDEX] | [SYMBOL]                        | [RAIL]            |
| :-----: | :------------------------------ | :---------------- |
|  [01]   | `PseudoClassesAttribute`        | style metadata    |
|  [02]   | `TemplatePartAttribute`         | template metadata |
|  [03]   | `AssignBindingAttribute`        | binding metadata  |
|  [04]   | `ContentAttribute`              | XAML content      |
|  [05]   | `TemplateContentAttribute`      | template content  |
|  [06]   | `ControlTemplateScopeAttribute` | template scope    |

[NOTIFICATION_TYPES]: transient notification surfaces
- rail: retained-ui

| [INDEX] | [SYMBOL]                      | [RAIL]           |
| :-----: | :---------------------------- | :--------------- |
|  [01]   | `WindowNotificationManager`   | toast manager    |
|  [02]   | `INotificationManager`        | manager contract |
|  [03]   | `IManagedNotificationManager` | content manager  |
|  [04]   | `NotificationType`            | severity enum    |
|  [05]   | `NotificationPosition`        | placement enum   |

[DATA_TRANSFER_TYPES]: Avalonia 12 clipboard and drag data-transfer surfaces
- rail: retained-ui

| [INDEX] | [SYMBOL]                                      | [RAIL]             |
| :-----: | :-------------------------------------------- | :----------------- |
|  [01]   | `Avalonia.Input.Platform.IClipboard`          | clipboard contract |
|  [02]   | `Avalonia.Input.Platform.ClipboardExtensions` | typed clip ops     |
|  [03]   | `IDataTransfer`                               | transfer contract  |
|  [04]   | `IAsyncDataTransfer`                          | async transfer     |
|  [05]   | `DataTransfer`                                | transfer payload   |
|  [06]   | `DataTransferItem`                            | per-format item    |
|  [07]   | `IDataTransferItem`                           | item contract      |
|  [08]   | `DataFormat`                                  | format identity    |
|  [09]   | `DataFormat<T>`                               | typed format       |
|  [10]   | `DataFormatKind`                              | format kind enum   |

## [03]-[ENTRYPOINTS]

[PROPERTY_OPERATIONS]: retained property operations
- rail: retained-ui

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]             | [RAIL]            |
| :-----: | :--------------------- | :------------------------- | :---------------- |
|  [01]   | `Register<T,U>`        | `AvaloniaProperty`         | styled property   |
|  [02]   | `RegisterDirect<T,U>`  | `AvaloniaProperty`         | direct property   |
|  [03]   | `Register`             | `AvaloniaPropertyRegistry` | property registry |
|  [04]   | `Bind`                 | `AvaloniaObject`           | state binding     |
|  [05]   | `GetObservable`        | `AvaloniaObjectExtensions` | state stream      |
|  [06]   | `GetBindingObservable` | `AvaloniaObjectExtensions` | binding stream    |

[ASSET_LOOKUP_OPERATIONS]: resource and name lookup
- rail: retained-ui

| [INDEX] | [SURFACE]         | [SURFACE_ROOT]           | [RAIL]             |
| :-----: | :---------------- | :----------------------- | :----------------- |
|  [01]   | `FindResource`    | `ResourceNodeExtensions` | resource lookup    |
|  [02]   | `TryFindResource` | `ResourceNodeExtensions` | guarded lookup     |
|  [03]   | `Register`        | `INameScope`             | name ownership     |
|  [04]   | `Find`            | `INameScope`             | named lookup       |
|  [05]   | `Add`             | `Styles`                 | style admission    |
|  [06]   | `Add`             | `ResourceDictionary`     | resource admission |

[INPUT_AND_ROUTE_OPERATIONS]: focus, routed event, and command surfaces
- rail: retained-ui

| [INDEX] | [SURFACE]       | [SURFACE_ROOT]          | [RAIL]          |
| :-----: | :-------------- | :---------------------- | :-------------- |
|  [01]   | `Focus`         | `IInputElement`         | focus movement  |
|  [02]   | `Focus`         | `FocusManager`          | focus ownership |
|  [03]   | `Register`      | `RoutedEventRegistry`   | event admission |
|  [04]   | `GetObservable` | `InteractiveExtensions` | event stream    |
|  [05]   | `AddHandler`    | `Interactive`           | event handling  |
|  [06]   | `RemoveHandler` | `Interactive`           | handler removal |

[XAML_AND_RENDER_OPERATIONS]: XAML load and visual invalidation
- rail: retained-ui

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]       | [RAIL]           |
| :-----: | :------------------ | :------------------- | :--------------- |
|  [01]   | `Configure<TApp>`   | `AppBuilder`         | application root |
|  [02]   | `Configure`         | `AppBuilder`         | application root |
|  [03]   | `Load`              | `AvaloniaXamlLoader` | XAML materialize |
|  [04]   | `InvalidateVisual`  | `Visual`             | render refresh   |
|  [05]   | `InvalidateMeasure` | `Layoutable`         | layout refresh   |
|  [06]   | `InvalidateArrange` | `Layoutable`         | arrange refresh  |

[NOTIFICATION_OPERATIONS]: toast presentation surfaces
- rail: retained-ui

| [INDEX] | [SURFACE]  | [SURFACE_ROOT]              | [RAIL]         |
| :-----: | :--------- | :-------------------------- | :------------- |
|  [01]   | `Show`     | `WindowNotificationManager` | toast present  |
|  [02]   | `Close`    | `WindowNotificationManager` | toast close    |
|  [03]   | `CloseAll` | `WindowNotificationManager` | toast clear    |
|  [04]   | `Position` | `WindowNotificationManager` | placement knob |
|  [05]   | `MaxItems` | `WindowNotificationManager` | queue cap      |

[DATA_TRANSFER_OPERATIONS]: clipboard and drag data-transfer composition
- rail: retained-ui

| [INDEX] | [SURFACE]                       | [SURFACE_ROOT]        | [RAIL]           |
| :-----: | :------------------------------ | :-------------------- | :--------------- |
|  [01]   | `SetDataAsync`                  | `IClipboard`          | clipboard write  |
|  [02]   | `TryGetDataAsync`               | `IClipboard`          | clipboard read   |
|  [03]   | `TryGetDataAsync<T>`            | `ClipboardExtensions` | typed clip read  |
|  [04]   | `TryGetTextAsync`               | `ClipboardExtensions` | text clip read   |
|  [05]   | `GetDataFormatsAsync`           | `ClipboardExtensions` | format probe     |
|  [06]   | `Add`                           | `DataTransfer`        | item compose     |
|  [07]   | `Create<T>`                     | `DataTransferItem`    | per-format item  |
|  [08]   | `CreateText`                    | `DataTransferItem`    | text item make   |
|  [09]   | `Set<T>`                        | `DataTransferItem`    | per-format set   |
|  [10]   | `TryGetRaw`                     | `DataTransferItem`    | per-format read  |
|  [11]   | `CreateBytesApplicationFormat`  | `DataFormat`          | byte format      |
|  [12]   | `CreateStringApplicationFormat` | `DataFormat`          | string format    |
|  [13]   | `Formats`                       | `IDataTransfer`       | format inventory |

## [04]-[IMPLEMENTATION_LAW]

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
