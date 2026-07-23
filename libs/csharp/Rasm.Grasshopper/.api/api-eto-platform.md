# [RASM_GRASSHOPPER_API_ETO_PLATFORM]

`Eto.Platform` mints the handler behind every `Eto.Forms` control and `Eto.Drawing` object, and `WidgetHandler<TControl,TWidget>` exposes that native control through `IControlObjectSource.ControlObject`. `NativeControlHost` admits a raw platform object into the managed tree, while `Eto.Mac.Forms.IMacControlHandler` partitions the AppKit `NSView` roles for layout, content, events, focus, and text input. `MacControlExtensions` extracts the Eto-backed view, and `MacConversions`/`CGConversions` carry color, image, point, matrix, and path values across the Eto/AppKit/CoreGraphics boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Forms`
- package: `Eto.Forms` (BSD-3-Clause)
- assembly: `Eto.dll` (core handler substrate), `Eto.macOS.dll` (`Eto.Mac` handler set)
- namespace: `Eto`, `Eto.Forms`, `Eto.Mac`, `Eto.Mac.Forms`, `Eto.Mac.Forms.Controls`
- target: in-process ALC reference inside the Rhino 9 WIP bundle, not a NuGet asset
- rail: platform-handlers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: platform identity and capability

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :--------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `Platform`                   | abstract      | active-platform root and handler factory     |
|  [02]   | `Platforms`                  | static        | assembly-qualified platform type identifiers |
|  [03]   | `PlatformFeatures`           | enum          | per-platform capability flags                |
|  [04]   | `HandlerAttribute`           | attribute     | binds a widget type to its handler interface |
|  [05]   | `PlatformExtensionAttribute` | attribute     | registers a platform extension assembly      |

[PUBLIC_TYPE_SCOPE]: handler families and styling

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `WidgetHandler<TWidget>`                             | class         | base handler over a widget                     |
|  [02]   | `WidgetHandler<TControl,TWidget>`                    | class         | binds a native control to a widget             |
|  [03]   | `WidgetHandler<TControl,TWidget,TCallback>`          | class         | adds a callback channel to the widget          |
|  [04]   | `Style`                                              | static        | style registry keyed by handler type and name  |
|  [05]   | `StyleWidgetHandler<TWidget>`                        | delegate      | style applied against a widget facade          |
|  [06]   | `StyleHandler<THandler>`                             | delegate      | style applied against a concrete handler       |
|  [07]   | `ThemedControlHandler<TControl,TWidget,TCallback>`   | class         | control drawn from managed widgets, not native |
|  [08]   | `ThemedContainerHandler<TControl,TWidget,TCallback>` | class         | themed container variant                       |

[PUBLIC_TYPE_SCOPE]: native-control hosting

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :--------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `IControlObjectSource`       | interface     | exposes the handler-created concrete control        |
|  [02]   | `NativeControlHost`          | class         | hosts an admitted native object in the managed tree |
|  [03]   | `CreateNativeControlArgs`    | class         | nullable native-object carrier for subclassing      |
|  [04]   | `NativeControlHost.IHandler` | interface     | creates the native host from the supplied object    |

[PUBLIC_TYPE_SCOPE]: the `Eto.Mac` managed-to-AppKit bridge

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------ | :------------ | :---------------------------------------------------- |
|  [01]   | `Eto.Mac.Platform`              | class         | concrete macOS platform with `ID == "macOS"`          |
|  [02]   | `IMacControlHandler`            | interface     | partitions the native `NSView` roles                  |
|  [03]   | `IMacViewHandler`               | interface     | control behavior over `IMacControlHandler`            |
|  [04]   | `IMacWindow`                    | interface     | window behavior over `IMacControlHandler`             |
|  [05]   | `IMacControl`                   | interface     | weak handler reference on generated controls          |
|  [06]   | `MacControlExtensions`          | static        | nullable handler and container-view extraction        |
|  [07]   | `Controls.NativeControlHandler` | class         | admits `NSView`, `NSViewController`, or native handle |
|  [08]   | `MacConversions`                | static        | AppKit and Eto value conversion                       |
|  [09]   | `CGConversions`                 | static        | CoreGraphics and Eto value conversion                 |
|  [10]   | `MacExtensions`                 | static        | low-level AppKit extensions for concrete handlers     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Platform` — active platform, identity, and handler factory

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :---------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Platform.Instance`                             | static   | active platform, runtime-nullable    |
|  [02]   | `Platform.Detect`                               | static   | active platform or throw             |
|  [03]   | `Platform.ID`                                   | property | platform identity string             |
|  [04]   | `Platform.Context`                              | property | ambient context, runtime-nullable    |
|  [05]   | `Platform.Supports<T>() -> bool`                | instance | capability probe by type parameter   |
|  [06]   | `Platform.Supports(Type) -> bool`               | instance | capability probe by type             |
|  [07]   | `Platform.Find(Type) -> Func<object>?`          | instance | handler-factory lookup, nullable     |
|  [08]   | `Platform.Create<T>() -> T`                     | instance | instantiate a registered handler     |
|  [09]   | `Platform.Create(Type) -> object`               | instance | instantiate by type                  |
|  [10]   | `Platform.Add<T>(Func<T>)`                      | instance | register a handler instantiator      |
|  [11]   | `Platforms.macOS`                               | static   | loader identity for the Mac assembly |
|  [12]   | `Eto.Mac.Platform.IsMac`                        | property | macOS-platform assertion             |
|  [13]   | `Eto.Mac.Platform.IsDesktop`                    | property | desktop-platform assertion           |
|  [14]   | `Eto.Mac.Platform.IsValid`                      | property | app-bundle validity                  |
|  [15]   | `Eto.Mac.Platform.SupportedFeatures`            | property | admitted capability flags            |
|  [16]   | `Eto.Mac.Platform.AddTo(Platform)`              | static   | register the Mac handler set         |
|  [17]   | `Eto.Mac.Platform.ThreadStart() -> IDisposable` | instance | UI-thread scope                      |

- `Platforms.macOS` resolves the loader string `"Eto.Mac.Platform, Eto.macOS"`.
- `Eto.Mac.Platform.SupportedFeatures` returns `CustomCellSupportsControlView | DrawableWithTransparentContent | TabIndexWithCustomContainers`.
- `[PlatformFeatures]`: `None` `CustomCellSupportsControlView` `DrawableWithTransparentContent` `TabIndexWithCustomContainers` `MultiThreadedUI` `Mnemonics`

[ENTRYPOINT_SCOPE]: `Style` — scoped appearance and behavior

| [INDEX] | [SURFACE]                                                  | [SHAPE] | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `Style.Provider`                                           | static  | active style provider, nullable   |
|  [02]   | `Style.StyleWidget`                                        | static  | per-widget style event            |
|  [03]   | `Style.Add<TWidget>(string?, StyleWidgetHandler<TWidget>)` | static  | register a widget-facade style    |
|  [04]   | `Style.Add<THandler>(string?, StyleHandler<THandler>)`     | static  | register a concrete-handler style |

[ENTRYPOINT_SCOPE]: `NativeControlHost` — hosting a raw platform view

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `NativeControlHost(object?)`                                       | ctor     | host an admitted native object   |
|  [02]   | `NativeControlHost()`                                              | ctor     | empty host for subclass creation |
|  [03]   | `NativeControlHost.OnCreateNativeControl(CreateNativeControlArgs)` | instance | subclass native-creation hook    |
|  [04]   | `CreateNativeControlArgs.NativeControl`                            | property | native-object carrier, nullable  |
|  [05]   | `NativeControlHost.IHandler.Create(object?)`                       | instance | build the native host            |
|  [06]   | `NativeControlHandler()`                                           | ctor     | empty native host                |
|  [07]   | `NativeControlHandler(NSView)`                                     | ctor     | host an existing `NSView`        |
|  [08]   | `NativeControlHandler(NSViewController)`                           | ctor     | host a view controller           |
|  [09]   | `NativeControlHandler.ContainerControl -> NSView`                  | property | the hosting container view       |
|  [10]   | `NativeControlHandler.Create(object?)`                             | instance | admit the native payload         |

- `NativeControlHandler.Create` admits `null`, `NSView`, `NSViewController`, or an `nint` resolving to `NSView`; `null` mints `MacPanelView`, an unsupported object throws `NotSupportedException`, and a non-view handle throws `InvalidOperationException`.

[ENTRYPOINT_SCOPE]: `IMacControlHandler` — AppKit view roles and nullable extraction

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]               |
| :-----: | :--------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `IMacControlHandler.ContainerControl -> NSView`                        | property | layout container role      |
|  [02]   | `IMacControlHandler.ContentControl -> NSView`                          | property | content role               |
|  [03]   | `IMacControlHandler.EventControl -> NSView`                            | property | event role                 |
|  [04]   | `IMacControlHandler.FocusControl -> NSView`                            | property | focus role                 |
|  [05]   | `IMacControlHandler.TextInputControl -> NSView`                        | property | text-input role            |
|  [06]   | `IMacControlHandler.MinimumSize`                                       | property | minimum extent             |
|  [07]   | `IMacControlHandler.IsEventHandled(string) -> bool`                    | instance | event-name handled probe   |
|  [08]   | `IMacControlHandler.RecalculateKeyViewLoop(ref NSView)`                | instance | rebuild the key-view chain |
|  [09]   | `IMacControlHandler.InvalidateMeasure()`                               | instance | drop the cached measure    |
|  [10]   | `MacControlExtensions.GetMacControl(Control?) -> IMacControlHandler?`  | static   | extract the handler        |
|  [11]   | `MacControlExtensions.GetMacViewHandler(Control?) -> IMacViewHandler?` | static   | extract the view handler   |
|  [12]   | `MacControlExtensions.GetContainerView(Widget?) -> NSView?`            | static   | extract the container view |
|  [13]   | `IControlObjectSource.ControlObject -> object?`                        | property | handler-created control    |

- `GetMacControl`, `GetMacViewHandler`, and `GetContainerView` follow nested Eto `ControlObject` values before returning `null`; `GetContainerView` finally admits a direct `NSView` control object.
- `IControlObjectSource.ControlObject`: `WidgetHandler<TControl,TWidget>` implements it explicitly and lazily returns `Control`.

[ENTRYPOINT_SCOPE]: `IMacViewHandler` / `IMacWindow` — behavior over native roles

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `IMacViewHandler.Widget -> Control`                         | property | owning Eto control           |
|  [02]   | `IMacViewHandler.Callback -> Control.ICallback`             | property | control callback channel     |
|  [03]   | `IMacViewHandler.UserPreferredSize -> Size`                 | property | requested extent             |
|  [04]   | `IMacViewHandler.CurrentCursor -> Cursor?`                  | property | active cursor                |
|  [05]   | `IMacViewHandler.BackgroundColor -> Color`                  | property | settable background color    |
|  [06]   | `IMacViewHandler.SystemActions -> Dictionary<nint,Command>` | property | selector-to-command map      |
|  [07]   | `IMacViewHandler.ShouldHaveFocus -> bool?`                  | property | settable focus override      |
|  [08]   | `IMacViewHandler.TextInputCancelled -> bool`                | property | settable text-input cancel   |
|  [09]   | `IMacViewHandler.TextInputImplemented -> bool`              | property | text-input capability        |
|  [10]   | `IMacViewHandler.AutoAttachNative -> bool`                  | property | settable auto-attach flag    |
|  [11]   | `IMacWindow.Control -> NSWindow`                            | property | native window                |
|  [12]   | `IMacWindow.Widget -> Window`                               | property | owning Eto window            |
|  [13]   | `IMacWindow.Callback -> Window.ICallback`                   | property | window callback channel      |
|  [14]   | `IMacWindow.RestoreBounds -> Rectangle?`                    | property | settable pre-maximize bounds |
|  [15]   | `IMacWindow.MenuBar -> NSMenu?`                             | property | attached menu bar            |
|  [16]   | `IMacWindow.FieldEditorClient -> NSObject?`                 | property | settable field-editor client |
|  [17]   | `IMacWindow.CloseWindow(Action<CancelEventArgs>?) -> bool`  | instance | cancellable close            |
|  [18]   | `IMacControl.WeakHandler -> WeakReference`                  | property | weak handler link            |

- `IMacControl.WeakHandler` lets a generated AppKit control recover its handler; it is not a native-control-handler marker.

[ENTRYPOINT_SCOPE]: macOS value and native-view conversions

`MacConversions`, `CGConversions`, and `MacExtensions` extend the receiver types below.

| [INDEX] | [SURFACE]                                            | [SHAPE] | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------- | :------ | :------------------------------- |
|  [01]   | `Color.ToNSUI() -> NSColor`                          | static  | Eto color to AppKit color        |
|  [02]   | `NSColor.ToEto(bool) -> Color`                       | static  | AppKit color to Eto, calibrated  |
|  [03]   | `CGPoint.ToEto(NSView) -> PointF`                    | static  | window point to view-space point |
|  [04]   | `Image.ToCG() -> CGImage`                            | static  | Eto image to CoreGraphics image  |
|  [05]   | `Image.ToNS(int?) -> NSImage`                        | static  | Eto image to AppKit image        |
|  [06]   | `CGColor.ToNS() -> NSColor`                          | static  | CoreGraphics color to AppKit     |
|  [07]   | `NSColor.ToCG() -> CGColor`                          | static  | AppKit color to CoreGraphics     |
|  [08]   | `Color.ToCG() -> CGColor`                            | static  | Eto color to CoreGraphics        |
|  [09]   | `CGColor.ToEto() -> Color`                           | static  | CoreGraphics color to Eto        |
|  [10]   | `CGAffineTransform.ToEto() -> IMatrix`               | static  | CoreGraphics transform to Eto    |
|  [11]   | `IMatrix.ToCG() -> CGAffineTransform`                | static  | Eto matrix to CoreGraphics       |
|  [12]   | `IGraphicsPath.ToCG() -> CGPath`                     | static  | Eto path to CoreGraphics path    |
|  [13]   | `NSColor?.UsingColorSpaceFast(NSString) -> NSColor?` | static  | fast colorspace conversion       |
|  [14]   | `NSView.SetClipsToBounds(bool)`                      | static  | set layer clipping               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every `Eto.Forms` and `Eto.Drawing` widget delegates to a `WidgetHandler` the active `Platform` mints; `HandlerAttribute` binds the widget type to its handler interface, and `IControlObjectSource.ControlObject` exposes the handler-created control without a second native field.
- `Style` restyles a control by name: a registered `StyleHandler<THandler>` or `StyleWidgetHandler<TWidget>` runs at attach time against the widget or its concrete handler, so a canvas or panel never subclasses to change appearance.
- `NativeControlHost` admits `NSView`, `NSViewController`, or a verified native handle; `GetContainerView` or `IMacControlHandler.ContainerControl` yields the Eto-backed view, and content, event, focus, and text-input work read their dedicated roles.
- `Eto.Mac.Platform` separates runtime identity (`IsMac`, `ID`), app-bundle validity (`IsValid`), and admitted capability (`SupportedFeatures`); `Platforms.macOS` is loader input, never the runtime `ID`.

[STACKING]:
- `Eto.Forms`(`.api/api-eto-forms`) and `Eto.Drawing`(`.api/api-eto-drawing`): this substrate resolves the control object each consumes, and `MacConversions`/`CGConversions` carry color, image, point, matrix, and path values into AppKit/CoreGraphics with no local conversion layer.
- `.api/api-macos-native`: `GetContainerView` and `IMacControlHandler.ContainerControl` yield the Eto-backed `NSView`; the native catalog owns the `CALayer`/`NSEvent`/`CADisplayLink` work over that view's valid host lifetime.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): the identity vocabulary — `Platforms` ids and `PlatformFeatures` flags — maps at the folder boundary onto `[SmartEnum]` and flag owners, so a platform-gate decision is exhaustive dispatch rather than a stringly-keyed `ID` comparison.
- `LanguageExt.Core`(`.api/api-languageext`): runtime-nullable `Platform.Instance`, `Find`, `GetMacControl`, `GetMacViewHandler`, `GetContainerView`, and `CreateNativeControlArgs.NativeControl` lower onto `Option<T>`/`Fin<T>` at the folder boundary; `Platform.Create<T>()` and invalid native-host payloads stay throwing boundaries the same rail traps.

[LOCAL_ADMISSION]:
- A widget composes its handler through `Platform.Create` or `Find`; a folder page never re-mints a `WidgetHandler` the active `Platform` already owns.
- Native-view work enters through `GetContainerView` or `IMacControlHandler.ContainerControl` behind the folder `Option<NSView>` boundary; raw AppKit runs only inside a verified host-valid window lifetime.
- Appearance changes ride a `Style` delegate registered by name, never a control subclass.

[RAIL_LAW]:
- Package: `Eto.Forms`
- Owns: per-widget handler resolution, scoped `Style` application, native-control hosting, AppKit `NSView` role partitioning, nullable container-view extraction, and Eto/AppKit/CoreGraphics value conversion.
- Accept: an initialized `Platform` identity, `HandlerAttribute`-bound handler interfaces, `IMacControlHandler` view roles, `MacControlExtensions` extraction, and the installed `MacConversions`/`CGConversions` owners.
- Reject: a control subclass where a `Style` delegate carries the change, a native lookup beside `GetContainerView`, a loader type string treated as runtime `ID`, an unguarded runtime-nullable extraction, or a local Eto-to-AppKit conversion beside `MacConversions`.
