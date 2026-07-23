# [RASM_APPUI_API_AVALONIA]

`Avalonia` owns the retained UI object model every `SurfaceHost` mounts onto: typed property and element trees, binding, styles, resources, input, routed events, and the render dispatcher. It holds the data-transfer boundary — clipboard and drag-drop — the shell input page composes, and marshals every cross-thread UI mutation through one render-thread hop. Every `SurfaceHost` binds the whole substrate through the retained-ui rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia`
- package: `Avalonia` (MIT)
- assembly: `Avalonia.Base` (object model, input, data-transfer, threading, styling)
- assembly: `Avalonia.Controls` (controls, notifications, name scope)
- assembly: `Avalonia.Markup.Xaml` (XAML loader, markup extensions)
- assembly: `Avalonia.Dialogs` (managed file dialogs)
- namespace: `Avalonia`, `Avalonia.Controls`, `Avalonia.Controls.Notifications`, `Avalonia.Data`, `Avalonia.Input`, `Avalonia.Input.Platform`, `Avalonia.Interactivity`, `Avalonia.Threading`, `Avalonia.Markup.Xaml`, `Avalonia.Styling`, `Avalonia.Platform`
- target: `net10.0` reference assemblies
- rail: retained-ui

## [02]-[PUBLIC_TYPES]

[BASE_OBJECTS]: retained property and element model

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :--------------------------------- | :------------ | :-------------------- |
|  [01]   | `AvaloniaObject`                   | class         | property owner        |
|  [02]   | `AvaloniaProperty`                 | class         | property identity     |
|  [03]   | `StyledProperty<TValue>`           | class         | inherited property    |
|  [04]   | `DirectProperty<TOwner,TValue>`    | class         | direct property       |
|  [05]   | `AttachedProperty<TValue>`         | class         | attached property     |
|  [06]   | `AvaloniaPropertyMetadata`         | class         | property metadata     |
|  [07]   | `AvaloniaPropertyRegistry`         | class         | property registry     |
|  [08]   | `AvaloniaPropertyChangedEventArgs` | class         | change event          |
|  [09]   | `BindingValue<T>`                  | struct        | binding-value carrier |

[ELEMENT_TREE]: styled, logical, visual, and layout participation

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]             |
| :-----: | :-------------- | :------------ | :----------------------- |
|  [01]   | `StyledElement` | class         | style participant        |
|  [02]   | `Visual`        | class         | visual tree node         |
|  [03]   | `Interactive`   | class         | routed-event node        |
|  [04]   | `InputElement`  | class         | focus + key-binding node |
|  [05]   | `Layoutable`    | class         | measure/arrange node     |
|  [06]   | `ILogical`      | interface     | logical tree node        |
|  [07]   | `IResourceHost` | interface     | resource owner           |

[CONTROL_SURFACES]: product surface and shell controls

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :--------------- | :------------ | :------------------ |
|  [01]   | `Application`    | class         | application root    |
|  [02]   | `AppBuilder`     | class         | application builder |
|  [03]   | `TopLevel`       | class         | host root           |
|  [04]   | `Window`         | class         | window shell        |
|  [05]   | `UserControl`    | class         | screen surface      |
|  [06]   | `Panel`          | class         | layout surface      |
|  [07]   | `ContentControl` | class         | content host        |
|  [08]   | `ItemsControl`   | class         | item host           |
|  [09]   | `Button`         | class         | command surface     |
|  [10]   | `TextBox`        | class         | text entry surface  |
|  [11]   | `TreeView`       | class         | hierarchy surface   |

[STATE_AND_STYLE]: binding, resources, styles, and templates

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]       |
| :-----: | :------------------------- | :------------ | :----------------- |
|  [01]   | `BindingBase`              | class         | binding root       |
|  [02]   | `Binding`                  | class         | reflection binding |
|  [03]   | `CompiledBindingExtension` | class         | compiled binding   |
|  [04]   | `MultiBinding`             | class         | composite binding  |
|  [05]   | `TemplateBinding`          | class         | template binding   |
|  [06]   | `BindingNotification`      | class         | binding result     |
|  [07]   | `ResourceDictionary`       | class         | resource scope     |
|  [08]   | `Styles`                   | class         | style collection   |
|  [09]   | `Style`                    | class         | selector style     |
|  [10]   | `Setter`                   | class         | styled assignment  |
|  [11]   | `ControlTheme`             | class         | theme record       |
|  [12]   | `DataTemplate`             | class         | data presentation  |

[THEME_VARIANT_TYPES]: the variant key that scopes resource resolution

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]     |
| :-----: | :--------------------- | :------------ | :--------------- |
|  [01]   | `ThemeVariant`         | record        | variant key      |
|  [02]   | `PlatformThemeVariant` | enum          | OS probe value   |
|  [03]   | `ThemeVariantScope`    | class         | subtree override |

[INPUT_AND_FOCUS_TYPES]: key gestures, bindings, focus, and modifiers

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :----------------------------------------- | :------------ | :-------------------- |
|  [01]   | `KeyGesture`                               | class         | value-equal chord     |
|  [02]   | `KeyBinding`                               | class         | gesture-binding row   |
|  [03]   | `KeyModifiers`                             | enum          | logical modifiers     |
|  [04]   | `RawInputModifiers`                        | enum          | raw modifier flags    |
|  [05]   | `FocusManager`                             | class         | focus ownership       |
|  [06]   | `NavigationMethod`                         | enum          | focus-move cause      |
|  [07]   | `KeyEventArgs` / `PointerPressedEventArgs` | class         | input event payloads  |
|  [08]   | `Dispatcher`                               | class         | render-thread marshal |

[METADATA_ATTRIBUTES]: XAML and template metadata

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]      |
| :-----: | :------------------------------ | :------------ | :---------------- |
|  [01]   | `PseudoClassesAttribute`        | attribute     | style metadata    |
|  [02]   | `TemplatePartAttribute`         | attribute     | template metadata |
|  [03]   | `ContentAttribute`              | attribute     | XAML content      |
|  [04]   | `TemplateContentAttribute`      | attribute     | template content  |
|  [05]   | `ControlTemplateScopeAttribute` | attribute     | template scope    |

[NOTIFICATION_TYPES]: transient notification surfaces

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]         |
| :-----: | :---------------------------- | :------------ | :------------------- |
|  [01]   | `WindowNotificationManager`   | class         | toast manager        |
|  [02]   | `INotificationManager`        | interface     | manager contract     |
|  [03]   | `IManagedNotificationManager` | interface     | content manager      |
|  [04]   | `NotificationType`            | enum          | severity vocabulary  |
|  [05]   | `NotificationPosition`        | enum          | placement vocabulary |

[DATA_TRANSFER_TYPES]: clipboard and drag data-transfer surfaces

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :-------------------- | :------------ | :--------------------------- |
|  [01]   | `IClipboard`          | interface     | clipboard contract           |
|  [02]   | `ClipboardExtensions` | class         | typed clip ops               |
|  [03]   | `IDataTransfer`       | interface     | sync transfer contract       |
|  [04]   | `IAsyncDataTransfer`  | interface     | async transfer contract      |
|  [05]   | `DataTransfer`        | class         | transfer payload             |
|  [06]   | `DataTransferItem`    | class         | per-format item              |
|  [07]   | `IDataTransferItem`   | interface     | item contract                |
|  [08]   | `DataFormat`          | class         | format identity              |
|  [09]   | `DataFormat<T>`       | class         | typed format                 |
|  [10]   | `DataFormatKind`      | enum          | format-kind vocabulary       |
|  [11]   | `DragDrop`            | class         | drop-target and drag surface |
|  [12]   | `DragDropEffects`     | enum          | drag-effect flags            |
|  [13]   | `DragEventArgs`       | class         | drop payload                 |

## [03]-[ENTRYPOINTS]

[PROPERTY_OPERATIONS]: retained property registration and observation

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `AvaloniaProperty.Register<TOwner,TValue>`                        | static   | styled property             |
|  [02]   | `AvaloniaProperty.RegisterDirect<TOwner,TValue>`                  | static   | direct property             |
|  [03]   | `AvaloniaProperty.RegisterAttached<THost,TValue>`                 | static   | attached property           |
|  [04]   | `AvaloniaObject.Bind(property, IObservable)`                      | instance | observable to state binding |
|  [05]   | `AvaloniaObjectExtensions.GetObservable(property)`                | static   | typed state stream          |
|  [06]   | `AvaloniaObjectExtensions.GetBindingObservable(property)`         | static   | `BindingValue<T>` stream    |
|  [07]   | `AvaloniaObjectExtensions.GetPropertyChangedObservable(property)` | static   | change-args stream          |

[ASSET_LOOKUP_OPERATIONS]: resource and name lookup

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]       |
| :-----: | :--------------------------------------- | :------- | :----------------- |
|  [01]   | `ResourceNodeExtensions.FindResource`    | static   | resource lookup    |
|  [02]   | `ResourceNodeExtensions.TryFindResource` | static   | guarded lookup     |
|  [03]   | `INameScope.Register`                    | instance | name ownership     |
|  [04]   | `INameScope.Find`                        | instance | named lookup       |
|  [05]   | `Styles.Add`                             | instance | style admission    |
|  [06]   | `ResourceDictionary.Add`                 | instance | resource admission |

[INPUT_AND_ROUTE_OPERATIONS]: focus, key binding, routed events, and dispatch

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]               |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `InputElement.Focus(NavigationMethod, KeyModifiers)`                     | instance | focus movement             |
|  [02]   | `FocusManager.GetFocusedElement() / TryMoveFocus(NavigationDirection)`   | instance | focus ownership and move   |
|  [03]   | `InputElement.KeyBindings` (`List<KeyBinding>`)                          | property | gesture-binding collection |
|  [04]   | `KeyGesture(Key, KeyModifiers) / Parse / Matches`                        | ctor     | value-equal chord          |
|  [05]   | `Interactive.AddHandler / RemoveHandler(RoutedEvent, handler, strategy)` | instance | routed-event handling      |
|  [06]   | `InteractiveExtensions.GetObservable(RoutedEvent)`                       | static   | routed-event stream        |
|  [07]   | `Dispatcher.UIThread.Invoke / InvokeAsync / Post`                        | static   | render-thread marshal      |
|  [08]   | `Dispatcher.CheckAccess() / VerifyAccess()`                              | instance | thread-affinity guard      |
|  [09]   | `Dispatcher.ToTaskScheduler() / ToTaskScheduler(DispatcherPriority)`     | instance | TaskScheduler for TPL      |

- `Dispatcher.ToTaskScheduler`: yields a `TaskScheduler` that runs continuations on this dispatcher; the no-arg form captures the current `AvaloniaSynchronizationContext` priority, else `DispatcherPriority.Default`.

[XAML_AND_RENDER_OPERATIONS]: XAML load and visual invalidation

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]     |
| :-----: | :----------------------------------------- | :------- | :--------------- |
|  [01]   | `AppBuilder.Configure<TApp>() / Configure` | static   | application root |
|  [02]   | `AvaloniaXamlLoader.Load / Parse`          | static   | XAML materialize |
|  [03]   | `Visual.InvalidateVisual`                  | instance | render refresh   |
|  [04]   | `Layoutable.InvalidateMeasure`             | instance | layout refresh   |
|  [05]   | `Layoutable.InvalidateArrange`             | instance | arrange refresh  |

[HOST_BUILD_OPERATIONS]: application-builder option admission and native host handle

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]         |
| :-----: | :---------------------------------------------------- | :------- | :------------------- |
|  [01]   | `AppBuilder.With<T>(T) / With<T>(Func<T>)`            | instance | option registration  |
|  [02]   | `AppBuilder.SetupWithoutStarting()`                   | instance | run-loop-free setup  |
|  [03]   | `TopLevel.TryGetPlatformHandle() -> IPlatformHandle?` | instance | native window handle |

- `AppBuilder.SetupWithoutStarting`: builds and configures without entering the run loop.
- `TopLevel.TryGetPlatformHandle`: returns `IPlatformHandle?` whose `Handle` is `nint`.

[THEME_VARIANT_OPERATIONS]: variant request, resolution, and OS-probe read

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]        |
| :-----: | :------------------------------------------------------------- | :------- | :------------------ |
|  [01]   | `Application.RequestedThemeVariant`                            | property | variant request     |
|  [02]   | `StyledElement.RequestedThemeVariant`                          | property | variant request     |
|  [03]   | `ThemeVariantScope.RequestedThemeVariant`                      | property | variant request     |
|  [04]   | `StyledElement.ActualThemeVariant / ActualThemeVariantChanged` | property | resolution and flip |
|  [05]   | `new ThemeVariant(key, inheritVariant)`                        | ctor     | inherited key       |
|  [06]   | `(ThemeVariant)platformThemeVariant`                           | operator | OS-probe cast       |
|  [07]   | `FluentTheme.Palettes[ThemeVariant]`                           | property | palette key         |

[NOTIFICATION_OPERATIONS]: toast presentation surfaces

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]          |
| :-----: | :------------------------------------------- | :------- | :-------------------- |
|  [01]   | `WindowNotificationManager.Show`             | instance | toast present         |
|  [02]   | `WindowNotificationManager.Close / CloseAll` | instance | toast close and clear |
|  [03]   | `WindowNotificationManager.Position`         | property | placement knob        |
|  [04]   | `WindowNotificationManager.MaxItems`         | property | queue cap             |

[DATA_TRANSFER_OPERATIONS]: clipboard and drag data-transfer composition

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `IClipboard.SetDataAsync(IAsyncDataTransfer?)`                                       | instance | clipboard write                  |
|  [02]   | `IClipboard.TryGetDataAsync() / TryGetInProcessDataAsync()`                          | instance | clipboard read                   |
|  [03]   | `IClipboard.ClearAsync() / FlushAsync()`                                             | instance | clear and flush                  |
|  [04]   | `ClipboardExtensions.GetDataFormatsAsync()`                                          | static   | present-format probe             |
|  [05]   | `ClipboardExtensions.TryGetValueAsync<T>(DataFormat<T>) / TryGetValuesAsync<T>`      | static   | typed clip read                  |
|  [06]   | `ClipboardExtensions.SetValueAsync<T>(DataFormat<T>, T?) / SetValuesAsync<T>`        | static   | typed clip write                 |
|  [07]   | `ClipboardExtensions.TryGetTextAsync() / SetTextAsync(string?)`                      | static   | text clip read/write             |
|  [08]   | `ClipboardExtensions.TryGetFilesAsync() / TryGetBitmapAsync()`                       | static   | file and bitmap clip read        |
|  [09]   | `DataTransfer.Add(DataTransferItem)`                                                 | instance | item compose                     |
|  [10]   | `DataTransfer.Formats / Items`                                                       | property | format and item inventory        |
|  [11]   | `DataTransferItem.Create<T>(DataFormat<T>, T?) / Create<T>(DataFormat<T>, Func<T?>)` | factory  | per-format item make             |
|  [12]   | `DataTransferItem.CreateText(string?)`                                               | factory  | text item make                   |
|  [13]   | `DataTransferItem.SetText(string?) / Set<T>(DataFormat<T>, T?)`                      | instance | text and typed set               |
|  [14]   | `DataTransferItem.TryGetRaw(DataFormat)`                                             | instance | untyped per-format read          |
|  [15]   | `DataFormat.CreateBytesApplicationFormat / CreateStringApplicationFormat`            | static   | byte and string app format       |
|  [16]   | `DataFormat.CreateInProcessFormat<T> / Text / Bitmap / File`                         | static   | in-process and universal formats |
|  [17]   | `DragDrop.SetAllowDrop(Interactive, bool) / GetAllowDrop`                            | static   | enable drop target               |
|  [18]   | `DragDrop.DoDragDropAsync(PointerPressedEventArgs, IDataTransfer, DragDropEffects)`  | static   | drag start                       |
|  [19]   | `DragDrop.DragEnterEvent / DragOverEvent / DragLeaveEvent / DropEvent`               | static   | drop routed events               |

- `IClipboard.SetDataAsync`: Avalonia takes ownership of the passed `IAsyncDataTransfer` and disposes it once the transfer leaves the clipboard.
- `DragDrop.DoDragDropAsync`: returns the accepted `DragDropEffects`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every product UI concept enters as a typed retained surface — an `AvaloniaObject` property, a `StyledElement` tree node, a `Style` or `ResourceDictionary` entry — and its state flows through the property system, observed as a stream, never polled through a manual change-handler chain.
- Every cross-thread UI mutation crosses the `Dispatcher.UIThread` marshal; `CheckAccess`/`VerifyAccess` guard the affinity.

[STACKING]:
- `api-reactive.md`: `AvaloniaObjectExtensions.GetObservable(property)` and `GetPropertyChangedObservable` emit `IObservable<T>` for `System.Reactive` operators and ReactiveUI `WhenAnyValue`; a control-state reaction is `GetObservable(prop).Throttle(...).DistinctUntilChanged().Subscribe(...)` under a `CompositeDisposable`, and `AvaloniaObject.Bind(property, observable)` pushes a stream back into a property.
- `api-reactive.md`: `Dispatcher.UIThread` (imperative marshal) and `SynchronizationContextScheduler` (stream marshal) share one render-thread boundary; a live-data bind composes `ObserveOn(SynchronizationContextScheduler)` once at the bind edge, an imperative cross-thread write uses `Dispatcher.UIThread.Post`/`InvokeAsync`, and a TPL continuation pins to the render thread through `Dispatcher.UIThread.ToTaskScheduler()` handed to `TaskFactory.StartNew`/`Task.ContinueWith`.
- `Shell/input` `HOTKEY_DERIVATION`: hotkeys derive from the command table onto Avalonia primitives — a value-equal `KeyGesture(Key, KeyModifiers)` with `Parse`/`Matches`, `KeyBinding` rows carrying `Gesture`/`Command` through `InputElement.KeyBindings`; `RawInputModifiers` carries mouse buttons for the headless input harness.
- `Shell/input` `DRAG_CLIPBOARD`: a drop target binds through `DragDrop.SetAllowDrop(control, true)` with routed `DragOverEvent`/`DropEvent` handlers reading `DragEventArgs.DataTransfer` and writing `DragEventArgs.DragEffects`; drags start through `DragDrop.DoDragDropAsync(pointerArgs, dataTransfer, allowedEffects)`.
- `Shell/input` `DRAG_CLIPBOARD`: structured copy crosses one `IClipboard.SetDataAsync(IAsyncDataTransfer)` carrying a `DataTransfer` of one `DataTransferItem` per format, keyed by `DataFormat.CreateBytesApplicationFormat`/`CreateStringApplicationFormat` and built by `DataTransferItem.Create<T>`/`CreateText`; reads ride `TryGetDataAsync` gated by `ClipboardExtensions.GetDataFormatsAsync`, then `TryGetTextAsync`/`TryGetValueAsync<T>`/`TryGetRaw`.
- within-lib: settled-vocabulary value types (`Thinktecture` `[SmartEnum]`/`[ValueObject]`, `NodaTime` instants, `UnitsNet` quantities) bind into properties through compiled `{Binding}` or `Bind(property, observable)`; AppUi owners never re-model them as Avalonia types.
- `Theme/tokens.md` + `api-avalonia-fluent.md`: `ThemeVariant` is the sealed-record key requested by `Application.RequestedThemeVariant`, resolved by `StyledElement.ActualThemeVariant`/`ActualThemeVariantChanged`, overridden per subtree by `ThemeVariantScope`, and indexed by `FluentTheme.Palettes`; `Theme/tokens.md` owns the `[SmartEnum<string>]` `ThemeVariantRow` whose `Variant` member carries one `ThemeVariant` per row, the high-contrast row `new ThemeVariant("high-contrast", ThemeVariant.Dark)` falling through the dark inheritance chain.
- `Theme/tokens.md`: `IPlatformSettings.GetColorValues()` crosses the OS probe to `PlatformColorValues.ThemeVariant` and casts through the explicit `ThemeVariant` operator; `Mount`/`ApplyTo` write `application.RequestedThemeVariant = row.Variant` and key `FluentTheme.Palettes[ThemeVariant.Light]`/`[ThemeVariant.Dark]` from the same resolution.

[LOCAL_ADMISSION]:
- Product UI concepts enter through typed retained surfaces; `TopLevel.GetTopLevel(control)` resolves the per-surface `Clipboard`/`FocusManager`/`StorageProvider`, and generated and handwritten markup share one namescope through `AvaloniaXamlLoader.Load`/`Parse`.

[RAIL_LAW]:
- Package: `Avalonia`
- Owns: retained object, property, style, resource, input, routed-event, drag-drop, data-transfer, and render contracts behind the one `SurfaceHost` axis — application roots, top levels, windows, screens, and panels — and XAML load, style and resource include, template metadata, compiled-binding generation, and namescope identity.
- Accept: product UI concepts enter through typed retained surfaces observed via `GetObservable`; host, sidecar, companion, and diagnostics shells share one UI rail; generated and handwritten markup share one namescope and resource rail.
- Reject: untyped wrapper layers over controls, properties, resources, or events; separate UI families per host modality; reflection `{Binding}` where a compiled binding is admissible; the `DataObject`/`DataFormats`/`IDataObject` clipboard surface.
