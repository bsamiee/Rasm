# [RASM_APPUI_API_AVALONIA]

`Avalonia` owns the retained UI object model every `SurfaceMount` mounts onto: typed property and element trees, binding, styles, resources, input, routed events, and the render dispatcher. It holds the data-transfer boundary — clipboard and drag-drop — the shell input page composes, and marshals every cross-thread UI mutation through one render-thread hop. Every `SurfaceMount` case binds the whole substrate through the retained-ui rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia`
- package: `Avalonia` (MIT)
- assembly: `Avalonia.Base` (object model, input, data-transfer, threading, styling)
- assembly: `Avalonia.Controls` (controls, notifications, name scope)
- assembly: `Avalonia.Markup.Xaml` (XAML loader, markup extensions)
- assembly: `Avalonia.Dialogs` (managed file dialogs)
- namespace: `Avalonia`, `Avalonia.Automation`, `Avalonia.Automation.Peers`, `Avalonia.Controls`, `Avalonia.Controls.Embedding`, `Avalonia.Controls.Notifications`, `Avalonia.Controls.Primitives`, `Avalonia.Data`, `Avalonia.Input`, `Avalonia.Input.Platform`, `Avalonia.Interactivity`, `Avalonia.Layout`, `Avalonia.Threading`, `Avalonia.Markup.Xaml`, `Avalonia.Styling`, `Avalonia.Platform`
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
|  [06]   | `Orientation`   | enum          | layout axis vocabulary   |
|  [07]   | `ILogical`      | interface     | logical tree node        |
|  [08]   | `IResourceHost` | interface     | resource owner           |

[CONTROL_SURFACES]: product surface and shell controls

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :--------------------------------------------- | :------------ | :----------------------------- |
|  [01]   | `Application`                                  | class         | application root               |
|  [02]   | `AppBuilder`                                   | class         | application builder            |
|  [03]   | `TopLevel`                                     | class         | host root                      |
|  [04]   | `Window`                                       | class         | window shell                   |
|  [05]   | `UserControl`                                  | class         | screen surface                 |
|  [06]   | `Panel`                                        | class         | layout surface                 |
|  [07]   | `ContentControl`                               | class         | content host                   |
|  [08]   | `ItemsControl`                                 | class         | item host                      |
|  [09]   | `SelectingItemsControl`                        | class         | selection-carrying item host   |
|  [10]   | `Button`                                       | class         | command surface                |
|  [11]   | `TextBox`                                      | class         | text entry surface             |
|  [12]   | `NumericUpDown`                                | class         | bounded numeric entry          |
|  [13]   | `CalendarDatePicker`                           | class         | date entry surface             |
|  [14]   | `ComboBox` / `ComboBoxItem`                    | class         | bounded-choice surface         |
|  [15]   | `RadioButton` / `ToggleSwitch`                 | class         | exclusive and binary toggles   |
|  [16]   | `Slider`                                       | class         | ranged scalar surface          |
|  [17]   | `TreeView`                                     | class         | hierarchy surface              |
|  [18]   | `StackPanel` / `DockPanel` / `Grid`            | class         | layout panels                  |
|  [19]   | `ColumnDefinitions` / `RowDefinitions`         | class         | grid track collections         |
|  [20]   | `GridSplitter` / `GridResizeDirection`         | class, enum   | split track resize surface     |
|  [21]   | `Menu` / `TabControl` / `TabItem` / `Expander` | class         | container and disclosure hosts |

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
|  [13]   | `IBrush`                   | interface     | paint contract     |
|  [14]   | `SolidColorBrush`          | class         | mutable color fill |

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

[AUTOMATION_TYPES]: `Avalonia.Automation` — the accessibility surface every shell announcement and audit reads

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------ | :------------ | :------------------------------------------------------------- |
|  [01]   | `AutomationProperties`                      | static        | attached automation-property owner                             |
|  [02]   | `AutomationLiveSetting`                     | enum          | `Off` / `Polite` / `Assertive`                                 |
|  [03]   | `AutomationControlType`                     | enum          | control-type override vocabulary                               |
|  [04]   | `AutomationLandmarkType`                    | enum          | landmark override vocabulary                                   |
|  [05]   | `AccessibilityView` / `IsOffscreenBehavior` | enum          | tree-visibility and offscreen policy                           |
|  [06]   | `AutomationPeer` / `ControlAutomationPeer`  | class         | peer bases a synthesized region derives                        |
|  [07]   | `KeyboardNavigation`                        | static        | attached tab-navigation owner                                  |
|  [08]   | `KeyboardNavigationMode`                    | enum          | `Continue` / `Cycle` / `Contained` / `Once` / `None` / `Local` |

[EMBED_TYPES]: `Avalonia.Controls.Embedding` + `Avalonia.Platform` — the foreign-view boundary an in-host mount crosses

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :----------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `EmbeddableControlRoot`        | class         | `TopLevel` root hosted by a foreign view   |
|  [02]   | `IPlatformHandle`              | interface     | `nint Handle` + `string? HandleDescriptor` |
|  [03]   | `PlatformHandle`               | class         | concrete handle carrier                    |
|  [04]   | `IMacOSTopLevelPlatformHandle` | interface     | macOS `NSView`/`NSWindow` handle access    |

[SHELL_CHROME_TYPES]: `Avalonia.Controls` — OS-owned menu and tray chrome

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :------------------------------ | :------------ | :-------------------------------- |
|  [01]   | `NativeMenu` / `NativeMenuItem` | class         | OS menu model and item            |
|  [02]   | `NativeMenuItemSeparator`       | class         | menu separator item               |
|  [03]   | `NativeMenuBar`                 | class         | in-window managed menu control    |
|  [04]   | `TrayIcon` / `TrayIcons`        | class         | tray indicator and its collection |
|  [05]   | `MenuItemToggleType`            | enum          | menu-item toggle vocabulary       |

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

[STORAGE_TYPES]: per-surface file and folder picker surfaces

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :------------------------ | :------------ | :-------------------- |
|  [01]   | `IStorageProvider`        | interface     | picker contract       |
|  [02]   | `IStorageFile`            | interface     | selected file token   |
|  [03]   | `IStorageFolder`          | interface     | selected folder token |
|  [04]   | `FilePickerFileType`      | class         | one named filter      |
|  [05]   | `FilePickerOpenOptions`   | class         | open-picker options   |
|  [06]   | `FilePickerSaveOptions`   | class         | save-picker options   |
|  [07]   | `FolderPickerOpenOptions` | class         | folder-picker options |

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

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `AppBuilder.Configure<TApp>() / Configure`         | static   | application root        |
|  [02]   | `AvaloniaXamlLoader.Load / Parse`                  | static   | XAML materialize        |
|  [03]   | `Visual.InvalidateVisual`                          | instance | render refresh          |
|  [04]   | `Layoutable.InvalidateMeasure`                     | instance | layout refresh          |
|  [05]   | `Layoutable.InvalidateArrange`                     | instance | arrange refresh         |
|  [06]   | `TopLevel.RequestAnimationFrame(Action<TimeSpan>)` | instance | one frame-tick callback |

- `TopLevel.RequestAnimationFrame` delivers a single tick carrying the frame timestamp; re-requesting from inside the callback is the frame loop, and on an embedded root the host's own run loop is what advances it — `StartRendering()` beside that self-rescheduling callback needs no clock of the caller's own.

[HOST_BUILD_OPERATIONS]: application-builder option admission and native host handle

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]         |
| :-----: | :---------------------------------------------------- | :------- | :------------------- |
|  [01]   | `AppBuilder.With<T>(T) / With<T>(Func<T>)`            | instance | option registration  |
|  [02]   | `AppBuilder.SetupWithoutStarting()`                   | instance | run-loop-free setup  |
|  [03]   | `TopLevel.TryGetPlatformHandle() -> IPlatformHandle?` | instance | native window handle |

- `AppBuilder.SetupWithoutStarting`: builds and configures without entering the run loop.
- `TopLevel.TryGetPlatformHandle`: returns `IPlatformHandle?` whose `Handle` is `nint`.

[LAYOUT_PASS_OPERATIONS]: the measure/arrange pass a custom `Panel` overrides

| [INDEX] | [SURFACE]                                                    | [SHAPE]   | [CAPABILITY]                  |
| :-----: | :----------------------------------------------------------- | :-------- | :---------------------------- |
|  [01]   | `Layoutable.Measure(Size)` / `Arrange(Rect)`                 | instance  | drive a child's pass          |
|  [02]   | `Layoutable.MeasureOverride(Size)` / `ArrangeOverride(Size)` | protected | own the pass body             |
|  [03]   | `Layoutable.MeasureCore(Size)` / `ArrangeCore(Rect)`         | protected | pre-override pass scaffolding |
|  [04]   | `Layoutable.DesiredSize` (`Size`, private set)               | property  | last measured extent          |
|  [05]   | `Layoutable.IsMeasureValid` / `IsArrangeValid`               | property  | pass validity flags           |
|  [06]   | `Layoutable.AffectsMeasure<T>` / `AffectsArrange<T>`         | static    | property-to-invalidation bind |
|  [07]   | `Layoutable.UpdateLayout()`                                  | instance  | synchronous pass drive        |
|  [08]   | `Layoutable.LayoutUpdated` / `EffectiveViewportChanged`      | event     | post-pass and viewport edges  |

- `Measure` short-circuits when `IsMeasureValid` holds against the same `availableSize`, and it notifies the VISUAL parent only when the newly measured `DesiredSize` differs from the previous one; that notification is `internal` and invalidates the parent's measure only while the parent is not itself mid-measure, so a child measured inside the parent's own `MeasureOverride` never re-enters the pass, while an out-of-band child re-measure that moves its desired size does. `InvalidateMeasure` walks no ancestor — it flags the element and queues it on the layout manager — so parent re-entry rides the desired-size edge alone and never a subscription.

[AUTOMATION_OPERATIONS]: attached automation identity, live regions, peers, and keyboard navigation

| [INDEX] | [SURFACE]                                                               | [SHAPE]   | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------------------- | :-------- | :-------------------------- |
|  [01]   | `AutomationProperties.SetAutomationId / GetAutomationId(StyledElement)` | static    | stable automation identity  |
|  [02]   | `AutomationProperties.SetName / GetName(StyledElement)`                 | static    | announced name              |
|  [03]   | `AutomationProperties.SetHelpText / GetHelpText(StyledElement)`         | static    | announced description       |
|  [04]   | `AutomationProperties.SetLiveSetting / GetLiveSetting(StyledElement)`   | static    | live-region posture         |
|  [05]   | `AutomationProperties.SetAccessKey / GetAccessKey(StyledElement)`       | static    | announced accelerator text  |
|  [06]   | `AutomationProperties.SetLabeledBy / GetLabeledBy(StyledElement)`       | static    | external label association  |
|  [07]   | `Control.OnCreateAutomationPeer() -> AutomationPeer`                    | protected | per-control peer mint       |
|  [08]   | `KeyboardNavigation.SetTabIndex / GetTabIndex(IInputElement)`           | static    | tab rank                    |
|  [09]   | `KeyboardNavigation.SetTabNavigation / GetTabNavigation(InputElement)`  | static    | region navigation mode      |
|  [10]   | `KeyboardNavigation.SetIsTabStop / GetIsTabStop(InputElement)`          | static    | tab-stop admission          |
|  [11]   | `KeyboardNavigation.SetTabOnceActiveElement / GetTabOnceActiveElement`  | static    | `Once` region re-entry seat |
|  [12]   | `InputElement.IsHitTestVisible` (`bool`)                                | property  | pointer transparency        |

- `TabIndexProperty` defaults to `int.MaxValue` and `TabNavigationProperty` to `KeyboardNavigationMode.Continue`, so an unranked stop sorts last and an unset region continues outward; `LiveSettingProperty` defaults to `Off`, which is why a silent row states `Off` rather than omitting the write.

[EMBED_OPERATIONS]: foreign-view root lifecycle and native handle access

| [INDEX] | [SURFACE]                                                                  | [SHAPE]   | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------- | :-------- | :-------------------------------- |
|  [01]   | `new EmbeddableControlRoot()` / `(ITopLevelImpl)`                          | ctor      | embedded root construction        |
|  [02]   | `EmbeddableControlRoot.Prepare()`                                          | instance  | initialize and apply template     |
|  [03]   | `EmbeddableControlRoot.StartRendering() / StopRendering()`                 | instance  | render loop start and stop        |
|  [04]   | `EmbeddableControlRoot.EnforceClientSize` (`bool`)                         | protected | track the host view's client size |
|  [05]   | `EmbeddableControlRoot.Dispose()`                                          | instance  | root teardown                     |
|  [06]   | `IMacOSTopLevelPlatformHandle.NSView / NSWindow` (`nint`)                  | property  | unretained native handles         |
|  [07]   | `IMacOSTopLevelPlatformHandle.GetNSViewRetained() / GetNSWindowRetained()` | instance  | retained native handles           |

- `EmbeddableControlRoot` derives `TopLevel` and implements `IFocusScope` and `IDisposable`; `StartRendering`/`StopRendering` are `new` members shadowing the `TopLevel` pair, and `EnforceClientSize` is a protected setter reachable only from a derived capsule.
- `TopLevel.GetTopLevel(Visual)` is the ONLY public root query — `Avalonia.VisualTree.VisualExtensions` declares no `GetVisualRoot` and `Visual.VisualRoot` is `protected internal` — and it keeps answering the root after `EmbeddableControlRoot.Dispose()`, so it proves attachment and never liveness.
- `Dispose()` on an embedded root raises no `Closed`, `DetachedFromVisualTree`, or `DetachedFromLogicalTree` edge, and a second `Dispose()` or a post-dispose `StartRendering()` is inert, so teardown ordering is the caller's disposable and never a lifecycle subscription.
- `IMacOSTopLevelPlatformHandle` carries Avalonia's `[Unstable]` marker, and the two `…Retained` accessors hand back a retained pointer whose release the caller owns; the unretained `NSView`/`NSWindow` properties do not.

[SHELL_CHROME_OPERATIONS]: OS menu export and tray indicator composition

| [INDEX] | [SURFACE]                                                           | [SHAPE]         | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------ | :-------------- | :--------------------------- |
|  [01]   | `NativeMenu.MenuProperty` (`AttachedProperty<NativeMenu?>`)         | attached        | menu attach point            |
|  [02]   | `NativeMenu.SetMenu / GetMenu(AvaloniaObject)`                      | static          | menu assignment and lookup   |
|  [03]   | `NativeMenu.GetIsNativeMenuExported(TopLevel) -> bool`              | static          | OS-export probe              |
|  [04]   | `NativeMenu.Items` / `Add(NativeMenuItemBase)`                      | instance        | menu composition             |
|  [05]   | `NativeMenu.NeedsUpdate / Opening / Closed`                         | event           | menu lifecycle edges         |
|  [06]   | `NativeMenuItem.Header / Icon / ToolTip / Gesture`                  | property        | item presentation            |
|  [07]   | `NativeMenuItem.Command / CommandParameter / IsEnabled / IsVisible` | property        | item command and gating      |
|  [08]   | `NativeMenuItem.IsChecked / ToggleType` (`MenuItemToggleType`)      | property        | item toggle state            |
|  [09]   | `TrayIcon.IconsProperty` (`AttachedProperty<TrayIcons?>`)           | attached        | tray collection attach point |
|  [10]   | `TrayIcon.SetIcons / GetIcons(Application)`                         | static          | tray collection assignment   |
|  [11]   | `TrayIcon.Icon / ToolTipText / Menu / IsVisible`                    | property        | indicator presentation       |
|  [12]   | `TrayIcon.Command / CommandParameter` and `Clicked`                 | property, event | indicator activation         |

- `NativeMenu.IsNativeMenuExportedProperty` is the attached flag the platform sets; `GetIsNativeMenuExported` takes a `TopLevel` because the export is per-window, and `TrayIcon.SetIcons` takes the `Application` because the tray is per-process.

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

[COMPILED_TEMPLATE_OPERATIONS]: per-control compiled template and theme binding

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :----------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `TemplatedControl.Template -> IControlTemplate?` | property | compiled visual-tree template  |
|  [02]   | `TemplatedControl.TemplateProperty`              | static   | styled slot the template binds |
|  [03]   | `StyledElement.Theme -> ControlTheme?`           | property | per-element control-theme bind |
|  [04]   | `StyledElement.ThemeProperty`                    | static   | styled slot the theme binds    |

[CONTROL_PROPERTY_OPERATIONS]: the styled slots a materialize fold writes by property rather than by member

| [INDEX] | [SURFACE]                                                           | [SHAPE]          | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------ | :--------------- | :------------------------------ |
|  [01]   | `AvaloniaObject.SetValue / GetValue / ClearValue(AvaloniaProperty)` | instance         | untyped slot write, read, reset |
|  [02]   | `ItemsControl.ItemsSourceProperty` (`IEnumerable?`)                 | static           | item source slot                |
|  [03]   | `SelectingItemsControl.SelectedValueProperty` (`object?`)           | static           | selected VALUE slot             |
|  [04]   | `SelectingItemsControl.SelectedValueBinding` (`BindingBase?`)       | property         | value projection off the item   |
|  [05]   | `TextBox.TextProperty` / `Watermark` / `AcceptsReturn`              | static, property | text entry slots                |
|  [06]   | `NumericUpDown.ValueProperty` / `Minimum` / `Maximum` / `Increment` | static, property | `decimal` numeric slots         |
|  [07]   | `RangeBase.ValueProperty` / `Minimum` / `Maximum`                   | static, property | ranged scalar slots             |
|  [08]   | `ToggleButton.IsCheckedProperty` (`bool?`)                          | static           | tri-state toggle slot           |
|  [09]   | `ContentControl.ContentProperty` (`object?`)                        | static           | content slot                    |
|  [10]   | `TemplatedControl.ForegroundProperty` / `FontSizeProperty`          | static           | paint and metric slots          |
|  [11]   | `Layoutable.MinHeightProperty` / `MinWidthProperty`                 | static           | minimum-extent slots            |
|  [12]   | `GridSplitter.ResizeDirection` (`GridResizeDirection`)              | property         | split axis selection            |

- `NumericUpDown` slots are `decimal` while `RangeBase` slots are `double`, so a `double`-typed domain value casts at the `NumericUpDown` bind edge and nowhere else; `SelectedValueBinding` is what makes `SelectedValue` the option's own value rather than the container item, so a value-round-tripping bounded choice binds the pair, never `SelectedItem`.
- `ClearValue(AvaloniaProperty)` resets a slot to its default across every priority; the typed `ClearValue<T>` overloads exist for `AvaloniaProperty<T>`, `StyledProperty<T>`, and `DirectPropertyBase<T>`.

[NOTIFICATION_OPERATIONS]: toast presentation surfaces

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]          |
| :-----: | :------------------------------------------- | :------- | :-------------------- |
|  [01]   | `WindowNotificationManager.Show`             | instance | toast present         |
|  [02]   | `WindowNotificationManager.Close / CloseAll` | instance | toast close and clear |
|  [03]   | `WindowNotificationManager.Position`         | property | placement knob        |
|  [04]   | `WindowNotificationManager.MaxItems`         | property | queue cap             |

[STORAGE_OPERATIONS]: per-surface capsule resolution and picker dispatch

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `TopLevel.GetTopLevel(Visual?) -> TopLevel?`                      | static   | per-surface capsule resolve  |
|  [02]   | `TopLevel.StorageProvider -> IStorageProvider`                    | property | picker capsule               |
|  [03]   | `IStorageProvider.CanOpen / CanSave / CanPickFolder`              | property | per-kind platform capability |
|  [04]   | `IStorageProvider.OpenFilePickerAsync(FilePickerOpenOptions)`     | instance | open picker                  |
|  [05]   | `IStorageProvider.SaveFilePickerAsync(FilePickerSaveOptions)`     | instance | save picker                  |
|  [06]   | `IStorageProvider.OpenFolderPickerAsync(FolderPickerOpenOptions)` | instance | folder picker                |
|  [07]   | `FilePickerFileType(string?)` with `Patterns` / `MimeTypes`       | ctor     | one filter row               |
|  [08]   | `FilePickerOpenOptions.AllowMultiple / FileTypeFilter`            | property | open cardinality and filter  |
|  [09]   | `FilePickerSaveOptions.FileTypeChoices / DefaultExtension`        | property | save filter and extension    |

- `TopLevel.GetTopLevel` returns null for a visual attached to no root; `TopLevel.StorageProvider` NEVER returns null — an unserved platform yields an `internal` no-op provider whose three capability properties all answer false, so availability reads the capability the operation needs and a provider type test is unspellable outside the assembly.

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
- Owns: retained object, property, style, resource, input, routed-event, drag-drop, data-transfer, and render contracts behind the one `SurfaceMount` axis — application roots, top levels, windows, screens, and panels — and XAML load, style and resource include, template metadata, compiled-binding generation, and namescope identity.
- Accept: product UI concepts enter through typed retained surfaces observed via `GetObservable`; host, sidecar, companion, and diagnostics shells share one UI rail; generated and handwritten markup share one namescope and resource rail.
- Reject: untyped wrapper layers over controls, properties, resources, or events; separate UI families per host modality; reflection `{Binding}` where a compiled binding is admissible; the `DataObject`/`DataFormats`/`IDataObject` clipboard surface.
