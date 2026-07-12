# [RASM_APPUI_API_AVALONIA]

`Avalonia` supplies the retained UI object model for the AppUi shell — the property/element/control model, binding, styles, resources, input, routed events, drag-drop, the data-transfer clipboard surface, notifications, and the dispatcher. It is the substrate every `SurfaceHost` mounts onto: `AppBuilder` boots the `Application`, `AvaloniaObject`/`StyledProperty` carry typed state, `AvaloniaObjectExtensions.GetObservable` projects property state into the `System.Reactive` rail, `DragDrop`/`IClipboard` carry the transfer boundary the `Shell/input` page composes, and `Dispatcher.UIThread` is the render-thread marshal that pairs with the reactive `SynchronizationContextScheduler`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia`
- package: `Avalonia` (version `12.0.5`, MIT)
- assembly: `Avalonia.Base` (object model, input, data-transfer, threading, styling)
- assembly: `Avalonia.Controls` (controls, notifications, name scope)
- assembly: `Avalonia.Markup.Xaml` (XAML loader, markup extensions)
- assembly: `Avalonia.Dialogs` (managed file dialogs)
- bound asset: `ref/net10.0` reference assemblies (the workspace binds `net10.0` directly)
- namespace: `Avalonia`, `Avalonia.Controls`, `Avalonia.Controls.Notifications`, `Avalonia.Data`, `Avalonia.Input`, `Avalonia.Input.Platform`, `Avalonia.Interactivity`, `Avalonia.Threading`, `Avalonia.Markup.Xaml`, `Avalonia.Styling`, `Avalonia.Platform`
- asset: runtime libraries
- rail: retained-ui

## [02]-[PUBLIC_TYPES]

[BASE_OBJECTS]: retained property and element model
- rail: retained-ui

| [INDEX] | [SYMBOL]                           | [RAIL]                |
| :-----: | :--------------------------------- | :-------------------- |
|  [01]   | `AvaloniaObject`                   | property owner        |
|  [02]   | `AvaloniaProperty`                 | property identity     |
|  [03]   | `StyledProperty<TValue>`           | inherited property    |
|  [04]   | `DirectProperty<TOwner,TValue>`    | direct property       |
|  [05]   | `AttachedProperty<TValue>`         | attached property     |
|  [06]   | `AvaloniaPropertyMetadata`         | property metadata     |
|  [07]   | `AvaloniaPropertyRegistry`         | property registry     |
|  [08]   | `AvaloniaPropertyChangedEventArgs` | change event          |
|  [09]   | `BindingValue<T>`                  | binding-value carrier |

[ELEMENT_TREE]: styled, logical, visual, and layout participation
- rail: retained-ui

| [INDEX] | [SYMBOL]        | [RAIL]                   |
| :-----: | :-------------- | :----------------------- |
|  [01]   | `StyledElement` | style participant        |
|  [02]   | `Visual`        | visual tree node         |
|  [03]   | `Interactive`   | routed-event node        |
|  [04]   | `InputElement`  | focus + key-binding node |
|  [05]   | `Layoutable`    | measure/arrange node     |
|  [06]   | `ILogical`      | logical tree node        |
|  [07]   | `IResourceHost` | resource owner           |

[CONTROL_SURFACES]: product surface and shell controls
- rail: retained-ui

| [INDEX] | [SYMBOL]         | [RAIL]                                  |
| :-----: | :--------------- | :-------------------------------------- |
|  [01]   | `Application`    | application root                        |
|  [02]   | `AppBuilder`     | application builder                     |
|  [03]   | `TopLevel`       | host root (`Clipboard`, `FocusManager`) |
|  [04]   | `Window`         | window shell                            |
|  [05]   | `UserControl`    | screen surface                          |
|  [06]   | `Panel`          | layout surface                          |
|  [07]   | `ContentControl` | content host                            |
|  [08]   | `ItemsControl`   | item host                               |
|  [09]   | `Button`         | command surface                         |
|  [10]   | `TextBox`        | text entry surface                      |
|  [11]   | `TreeView`       | hierarchy surface                       |

[STATE_AND_STYLE]: binding, resources, styles, and templates
- rail: retained-ui

| [INDEX] | [SYMBOL]                   | [RAIL]             |
| :-----: | :------------------------- | :----------------- |
|  [01]   | `BindingBase`              | binding root       |
|  [02]   | `Binding`                  | reflection binding |
|  [03]   | `CompiledBindingExtension` | compiled binding   |
|  [04]   | `MultiBinding`             | composite binding  |
|  [05]   | `TemplateBinding`          | template binding   |
|  [06]   | `BindingNotification`      | binding result     |
|  [07]   | `ResourceDictionary`       | resource scope     |
|  [08]   | `Styles`                   | style collection   |
|  [09]   | `Style`                    | selector style     |
|  [10]   | `Setter`                   | styled assignment  |
|  [11]   | `ControlTheme`             | theme record       |
|  [12]   | `DataTemplate`             | data presentation  |

[THEME_VARIANT_TYPES]: the variant key that scopes resource resolution
- rail: retained-ui

| [INDEX] | [SYMBOL]                                 | [RAIL]            |
| :-----: | :--------------------------------------- | :---------------- |
|  [01]   | `Avalonia.Styling.ThemeVariant`          | variant key       |
|  [02]   | `Avalonia.Platform.PlatformThemeVariant` | OS probe value    |
|  [03]   | `StyledElement.RequestedThemeVariant`    | requested variant |
|  [04]   | `StyledElement.ActualThemeVariant`       | resolved variant  |
|  [05]   | `Avalonia.Controls.ThemeVariantScope`    | subtree override  |

[THEME_VARIANT_DETAILS]:
- `Avalonia.Styling.ThemeVariant`: sealed record carrying `Key`, `InheritVariant`, the `Light`/`Dark`/`Default` statics, the `(object key, ThemeVariant? inheritVariant)` inheritance constructor, and explicit conversions to and from `PlatformThemeVariant`.
- `Avalonia.Platform.PlatformThemeVariant`: OS light/dark value projected through `IPlatformSettings` and `PlatformColorValues`.
- `StyledElement.RequestedThemeVariant`: settable `StyledProperty<ThemeVariant?>` also carried by `Application` and `TopLevel`.
- `StyledElement.ActualThemeVariant`: resolved inherited variant paired with `ActualThemeVariantChanged`.
- `Avalonia.Controls.ThemeVariantScope`: `Decorator` subtree boundary carrying its own `RequestedThemeVariant`.

[INPUT_AND_FOCUS_TYPES]: key gestures, bindings, focus, and modifiers
- rail: retained-ui

| [INDEX] | [SYMBOL]                                   | [RAIL]                                         |
| :-----: | :----------------------------------------- | :--------------------------------------------- |
|  [01]   | `KeyGesture`                               | value-equal `(Key, KeyModifiers)` chord        |
|  [02]   | `KeyBinding`                               | `Gesture`/`Command` binding row                |
|  [03]   | `KeyModifiers`                             | logical modifier flags                         |
|  [04]   | `RawInputModifiers`                        | raw modifier + mouse-button flags              |
|  [05]   | `FocusManager`                             | `GetFocusedElement` / `Focus` / `TryMoveFocus` |
|  [06]   | `NavigationMethod`                         | focus-move cause                               |
|  [07]   | `KeyEventArgs` / `PointerPressedEventArgs` | input event payloads                           |
|  [08]   | `Dispatcher`                               | `UIThread` render-thread marshal               |

[METADATA_ATTRIBUTES]: XAML and template metadata
- rail: retained-ui

| [INDEX] | [SYMBOL]                        | [RAIL]            |
| :-----: | :------------------------------ | :---------------- |
|  [01]   | `PseudoClassesAttribute`        | style metadata    |
|  [02]   | `TemplatePartAttribute`         | template metadata |
|  [03]   | `ContentAttribute`              | XAML content      |
|  [04]   | `TemplateContentAttribute`      | template content  |
|  [05]   | `ControlTemplateScopeAttribute` | template scope    |

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

| [INDEX] | [SYMBOL]                                      | [RAIL]                                                  |
| :-----: | :-------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `Avalonia.Input.Platform.IClipboard`          | clipboard contract (`SetDataAsync`/`TryGetDataAsync`)   |
|  [02]   | `Avalonia.Input.Platform.ClipboardExtensions` | typed clip ops (extension methods)                      |
|  [03]   | `Avalonia.Input.IDataTransfer`                | sync transfer contract (`IDisposable`)                  |
|  [04]   | `Avalonia.Input.IAsyncDataTransfer`           | async transfer contract                                 |
|  [05]   | `Avalonia.Input.DataTransfer`                 | transfer payload (`IDataTransfer`+`IAsyncDataTransfer`) |
|  [06]   | `Avalonia.Input.DataTransferItem`             | per-format item                                         |
|  [07]   | `Avalonia.Input.IDataTransferItem`            | item contract (`TryGetRaw`)                             |
|  [08]   | `Avalonia.Input.DataFormat`                   | format identity (`Identifier`/`Kind`)                   |
|  [09]   | `Avalonia.Input.DataFormat<T>`                | typed format                                            |
|  [10]   | `Avalonia.Input.DataFormatKind`               | format-kind enum                                        |
|  [11]   | `Avalonia.Input.DragDrop`                     | drop-target + drag-initiation static surface            |
|  [12]   | `Avalonia.Input.DragDropEffects`              | `None`/`Copy`/`Move`/`Link` flags                       |
|  [13]   | `Avalonia.Input.DragEventArgs`                | drop payload (`DataTransfer`/`DragEffects`)             |

## [03]-[ENTRYPOINTS]

[PROPERTY_OPERATIONS]: retained property registration and observation
- rail: retained-ui

| [INDEX] | [SURFACE]                                | [SURFACE_ROOT]             | [RAIL]                      |
| :-----: | :--------------------------------------- | :------------------------- | :-------------------------- |
|  [01]   | `Register<TOwner,TValue>`                | `AvaloniaProperty`         | styled property             |
|  [02]   | `RegisterDirect<TOwner,TValue>`          | `AvaloniaProperty`         | direct property             |
|  [03]   | `RegisterAttached<THost,TValue>`         | `AvaloniaProperty`         | attached property           |
|  [04]   | `Bind(property, IObservable)`            | `AvaloniaObject`           | observable -> state binding |
|  [05]   | `GetObservable(property)`                | `AvaloniaObjectExtensions` | typed state stream          |
|  [06]   | `GetBindingObservable(property)`         | `AvaloniaObjectExtensions` | `BindingValue<T>` stream    |
|  [07]   | `GetPropertyChangedObservable(property)` | `AvaloniaObjectExtensions` | change-args stream          |

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

[INPUT_AND_ROUTE_OPERATIONS]: focus, key binding, routed events, and dispatch
- rail: retained-ui

| [INDEX] | [SURFACE]                                                      | [SURFACE_ROOT]          | [RAIL]                     |
| :-----: | :------------------------------------------------------------- | :---------------------- | :------------------------- |
|  [01]   | `Focus(NavigationMethod, KeyModifiers)`                        | `InputElement`          | focus movement             |
|  [02]   | `GetFocusedElement()` / `TryMoveFocus(NavigationDirection)`    | `FocusManager`          | focus ownership / move     |
|  [03]   | `KeyBindings` (`List<KeyBinding>`)                             | `InputElement`          | gesture-binding collection |
|  [04]   | `KeyGesture(Key, KeyModifiers)` / `Parse` / `Matches`          | `KeyGesture`            | value-equal chord          |
|  [05]   | `AddHandler` / `RemoveHandler(RoutedEvent, handler, strategy)` | `Interactive`           | routed-event handling      |
|  [06]   | `GetObservable(RoutedEvent)`                                   | `InteractiveExtensions` | routed-event stream        |
|  [07]   | `UIThread.Invoke` / `InvokeAsync` / `Post`                     | `Dispatcher`            | render-thread marshal      |
|  [08]   | `CheckAccess()` / `VerifyAccess()`                             | `Dispatcher`            | thread-affinity guard      |

[XAML_AND_RENDER_OPERATIONS]: XAML load and visual invalidation
- rail: retained-ui

| [INDEX] | [SURFACE]                       | [SURFACE_ROOT]       | [RAIL]           |
| :-----: | :------------------------------ | :------------------- | :--------------- |
|  [01]   | `Configure<TApp>` / `Configure` | `AppBuilder`         | application root |
|  [02]   | `Load` / `Parse`                | `AvaloniaXamlLoader` | XAML materialize |
|  [03]   | `InvalidateVisual`              | `Visual`             | render refresh   |
|  [04]   | `InvalidateMeasure`             | `Layoutable`         | layout refresh   |
|  [05]   | `InvalidateArrange`             | `Layoutable`         | arrange refresh  |

[HOST_BUILD_OPERATIONS]: application-builder option admission and native host handle
- rail: retained-ui

| [INDEX] | [SURFACE]                                    | [SURFACE_ROOT] | [RAIL]               |
| :-----: | :------------------------------------------- | :------------- | :------------------- |
|  [01]   | `With<T>(T options)` / `With<T>(Func<T>)`    | `AppBuilder`   | option registration  |
|  [02]   | `SetupWithoutStarting()`                     | `AppBuilder`   | run-loop-free setup  |
|  [03]   | `TryGetPlatformHandle() -> IPlatformHandle?` | `TopLevel`     | native window handle |

[HOST_BUILD_DETAILS]:
- `AppBuilder.With<T>` registers platform and options values, including `UseSkia().With(SkiaOptions)`.
- `AppBuilder.SetupWithoutStarting()` builds and configures the application without entering a run loop.
- `TopLevel.TryGetPlatformHandle()` returns `IPlatformHandle?`; `IPlatformHandle.Handle` is `nint`, and `HandleDescriptor` is `string?`.

[THEME_VARIANT_OPERATIONS]: variant request, resolution, and OS-probe read
- rail: retained-ui
- `RequestedThemeVariant` accepts `ThemeVariant?` on each owning surface.

| [INDEX] | [SURFACE]                                          | [SURFACE_ROOT]      | [RAIL]              |
| :-----: | :------------------------------------------------- | :------------------ | :------------------ |
|  [01]   | `RequestedThemeVariant`                            | `Application`       | variant request     |
|  [02]   | `RequestedThemeVariant`                            | `StyledElement`     | variant request     |
|  [03]   | `RequestedThemeVariant`                            | `ThemeVariantScope` | variant request     |
|  [04]   | `ActualThemeVariant` / `ActualThemeVariantChanged` | `StyledElement`     | resolution and flip |
|  [05]   | `new ThemeVariant(key, inheritVariant)`            | `ThemeVariant`      | inherited key       |
|  [06]   | `(ThemeVariant)platformThemeVariant`               | `ThemeVariant`      | OS-probe cast       |
|  [07]   | `Palettes[ThemeVariant]` (`ColorPaletteResources`) | `FluentTheme`       | palette key         |

[NOTIFICATION_OPERATIONS]: toast presentation surfaces
- rail: retained-ui

| [INDEX] | [SURFACE]            | [SURFACE_ROOT]              | [RAIL]            |
| :-----: | :------------------- | :-------------------------- | :---------------- |
|  [01]   | `Show`               | `WindowNotificationManager` | toast present     |
|  [02]   | `Close` / `CloseAll` | `WindowNotificationManager` | toast close/clear |
|  [03]   | `Position`           | `WindowNotificationManager` | placement knob    |
|  [04]   | `MaxItems`           | `WindowNotificationManager` | queue cap         |

[DATA_TRANSFER_OPERATIONS]: clipboard and drag data-transfer composition
- rail: retained-ui
- Avalonia owns and disposes the transfer handed to `IClipboard.SetDataAsync`.

| [INDEX] | [SURFACE]                                                             | [SURFACE_ROOT]        | [RAIL]                              |
| :-----: | :-------------------------------------------------------------------- | :-------------------- | :---------------------------------- |
|  [01]   | `SetDataAsync(IAsyncDataTransfer?)`                                   | `IClipboard`          | clipboard write                     |
|  [02]   | `TryGetDataAsync()` / `TryGetInProcessDataAsync()`                    | `IClipboard`          | clipboard read                      |
|  [03]   | `ClearAsync()` / `FlushAsync()`                                       | `IClipboard`          | clear / flush                       |
|  [04]   | `GetDataFormatsAsync()`                                               | `ClipboardExtensions` | present-format probe                |
|  [05]   | `TryGetValueAsync<T>(DataFormat<T>)` / `TryGetValuesAsync<T>`         | `ClipboardExtensions` | typed clip read                     |
|  [06]   | `SetValueAsync<T>(DataFormat<T>, T?)` / `SetValuesAsync<T>`           | `ClipboardExtensions` | typed clip write                    |
|  [07]   | `TryGetTextAsync()` / `SetTextAsync(string?)`                         | `ClipboardExtensions` | text clip read/write                |
|  [08]   | `TryGetFilesAsync()` / `TryGetBitmapAsync()`                          | `ClipboardExtensions` | file / bitmap clip read             |
|  [09]   | `Add(DataTransferItem)` / `Formats` / `Items`                         | `DataTransfer`        | item compose / inventory            |
|  [10]   | `Create<T>(DataFormat<T>, T?)` / `Create<T>(DataFormat<T>, Func<T?>)` | `DataTransferItem`    | per-format item make                |
|  [11]   | `CreateText(string?)` / `SetText` / `Set<T>(DataFormat<T>, T?)`       | `DataTransferItem`    | text/typed item set                 |
|  [12]   | `TryGetRaw(DataFormat)`                                               | `DataTransferItem`    | untyped per-format read (`object?`) |
|  [13]   | `CreateBytesApplicationFormat` / `CreateStringApplicationFormat`      | `DataFormat`          | byte / string app format            |
|  [14]   | `CreateInProcessFormat<T>` / `Text` / `Bitmap` / `File`               | `DataFormat`          | in-process + universal formats      |
|  [15]   | `SetAllowDrop(Interactive, bool)` / `GetAllowDrop`                    | `DragDrop`            | enable drop target                  |
|  [16]   | `DoDragDropAsync`                                                     | `DragDrop`            | drag start                          |
|  [17]   | `DragEnterEvent` / `DragOverEvent` / `DragLeaveEvent` / `DropEvent`   | `DragDrop`            | drop routed events                  |

[DRAG_START_SIGNATURE]: `DoDragDropAsync(PointerPressedEventArgs, IDataTransfer, DragDropEffects)` returns `DragDropEffects`.

## [04]-[IMPLEMENTATION_LAW]

[OBJECT_MODEL_LAW]:
- Package: `Avalonia`
- Owns: retained object, property, style, resource, input, routed-event, drag-drop, data-transfer, and render contracts.
- Accept: product UI concepts enter through typed retained surfaces; property state is observed through `GetObservable`, never a manual `PropertyChanged` handler chain.
- Reject: untyped wrapper layers over controls, properties, resources, or events; the legacy `DataObject`/`DataFormats`/`IDataObject` clipboard surface (obsolete in Avalonia 12 — `IDataTransfer`/`DataTransferItem`/`DataFormat` replace it).

[STACKING]:
- Property state feeds the reactive rail: `AvaloniaObjectExtensions.GetObservable(property)` / `GetPropertyChangedObservable` emit `IObservable<T>` consumed by `System.Reactive` operators (`api-reactive.md`) and ReactiveUI `WhenAnyValue`; a control-state reaction is `GetObservable(prop).Throttle(...).DistinctUntilChanged().Subscribe(...)` under a `CompositeDisposable`, and `AvaloniaObject.Bind(property, observable)` pushes a stream back into a property.
- The render-thread hop is one boundary shared by two owners: Avalonia's `Dispatcher.UIThread` (imperative marshal) and `System.Reactive`'s `SynchronizationContextScheduler` (stream marshal). A live-data bind composes `ObserveOn(SynchronizationContextScheduler)` once at the bind edge; an imperative cross-thread UI write uses `Dispatcher.UIThread.Post`/`InvokeAsync`. `CheckAccess`/`VerifyAccess` guard the affinity.
- Hotkeys derive from the command table onto Avalonia primitives: a value-equal `KeyGesture` carries `(Key, KeyModifiers)` with `Parse`/`Matches`, and `KeyBinding` rows carry `Gesture`/`Command` through the surface root's `InputElement.KeyBindings` collection (`Shell/input` `HOTKEY_DERIVATION`).
- `RawInputModifiers` carries mouse buttons for the headless harness, so headless input simulation crosses it instead of `KeyModifiers`.
- The transfer boundary stacks `DragDrop` and the data-transfer surface under the validation rail (`Shell/input` `DRAG_CLIPBOARD`). A drop target binds through `DragDrop.SetAllowDrop(control, true)` with routed `DragDrop.DragOverEvent` and `DropEvent` handlers.
- Dropped payloads read `DragEventArgs.DataTransfer`, and `DragOver` writes the effect to `DragEventArgs.DragEffects`. Drags start through `DragDrop.DoDragDropAsync(pointerArgs, dataTransfer, allowedEffects)`.
- Structured copy crosses one `IClipboard.SetDataAsync(IAsyncDataTransfer)` carrying a `DataTransfer` with one `DataTransferItem` per format. `DataFormat.CreateBytesApplicationFormat` and `CreateStringApplicationFormat` key the formats, and `DataTransferItem.Create<T>(DataFormat<T>, T?)` or `CreateText` builds each item.
- Reads ride `IClipboard.TryGetDataAsync()` with `ClipboardExtensions.GetDataFormatsAsync` as the present-format gate. Extraction uses `ClipboardExtensions.TryGetTextAsync`, `TryGetValueAsync<T>(DataFormat<T>)`, or `DataTransferItem.TryGetRaw`.
- Avalonia takes ownership of the `IAsyncDataTransfer` passed to `SetDataAsync` and disposes it once the transfer leaves the clipboard.
- Settled-vocabulary value types from the substrate (`Thinktecture` `[SmartEnum]`/`[ValueObject]`, `NodaTime` instants, `UnitsNet` quantities) bind into properties through compiled `{Binding}` or `Bind(property, observable)`; the AppUi owners never re-model those as Avalonia types.
- `Avalonia.Styling.ThemeVariant` is the cross-catalog sealed-record key requested by `Application.RequestedThemeVariant`, resolved by `StyledElement.ActualThemeVariant` with `ActualThemeVariantChanged`, overridden per subtree by `ThemeVariantScope`, and indexed by `FluentTheme.Palettes` (`api-avalonia-fluent.md`).
- The `Theme/tokens.md` token rail owns the AppUi variant vocabulary as `[SmartEnum<string>]` `ThemeVariantRow`, whose `Variant` member carries one `ThemeVariant` per row. Settled rows carry `ThemeVariant.Light`, `Dark`, or `Default`; the high-contrast row carries `new ThemeVariant("high-contrast", ThemeVariant.Dark)` so missing resources fall through the dark inheritance chain.
- The OS probe crosses `IPlatformSettings.GetColorValues()` to `PlatformColorValues.ThemeVariant` (`PlatformThemeVariant`) and casts through the explicit `ThemeVariant` operator.
- `Mount` and `ApplyTo` write `application.RequestedThemeVariant = row.Variant` and key `FluentTheme.Palettes[ThemeVariant.Light]` or `[ThemeVariant.Dark]` from the same resolution.
- `ThemeVariant` remains the wire key, and `ThemeVariantRow` remains the domain vocabulary that projects onto it.

[SHELL_LAW]:
- Package: `Avalonia`
- Owns: application roots, top levels, windows, screens, and panels behind the one `SurfaceHost` axis.
- Accept: host, sidecar, companion, diagnostics, and downstream shells share one UI rail; `TopLevel.GetTopLevel(control)` resolves the per-surface `Clipboard`/`FocusManager`/`StorageProvider`.
- Reject: separate UI families per host modality.

[XAML_LAW]:
- Package: `Avalonia`
- Owns: XAML load, style include, resource include, template metadata, compiled-binding generation, and namescope identity.
- Accept: generated and handwritten surfaces share the same namescope and resource rail; `AvaloniaXamlLoader.Load`/`Parse` materialize markup into the retained tree.
- Reject: generated XAML that routes outside the retained UI object model; reflection `{Binding}` where a compiled binding is admissible.
