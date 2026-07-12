# [RASM_GRASSHOPPER_API_ETO_PLATFORM]

`Eto.Platform` owns handler resolution for every `Eto.Forms` control and `Eto.Drawing` object. `WidgetHandler<TControl,TWidget>` exposes a concrete control through `IControlObjectSource.ControlObject`; `NativeControlHost` admits a platform object into the managed tree; and `Eto.Mac.Forms.IMacControlHandler` partitions the AppKit view roles used for layout, content, events, focus, and text input. Nullable native extraction enters through `MacControlExtensions`, while `MacConversions` and `CGConversions` carry values across the Eto/AppKit/CoreGraphics boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto` platform substrate

- host: `Grasshopper2` inside the Rhino 9 WIP process
- assembly: `Eto.dll` (core handler substrate), `Eto.macOS.dll` (`Eto.Mac` handler set)
- namespace: `Eto`, `Eto.Forms`, `Eto.Mac`, `Eto.Mac.Forms`, `Eto.Mac.Forms.Controls`
- platform: the concrete platform is `Eto.Mac.Platform`; handler resolution rides `Platform.Instance`
- rail: platform-handlers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: platform identity and capability

- rail: platform-handlers

| [INDEX] | [SYMBOL]                     | [KIND]    | [CAPABILITY]                                 |
| :-----: | :--------------------------- | :-------- | :------------------------------------------- |
|  [01]   | `Platform`                   | abstract  | active-platform root and handler factory     |
|  [02]   | `Platforms`                  | static    | assembly-qualified platform type identifiers |
|  [03]   | `PlatformFeatures`           | enum      | flags of per-platform capability             |
|  [04]   | `HandlerAttribute`           | attribute | binds a widget type to its handler interface |
|  [05]   | `PlatformExtensionAttribute` | attribute | registers a platform extension assembly      |

[PUBLIC_TYPE_SCOPE]: handler families and styling

- rail: platform-handlers

| [INDEX] | [SYMBOL]                                             | [KIND]    | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------- | :-------- | :-------------------------------------------------- |
|  [01]   | `WidgetHandler<TWidget>`                             | class     | base handler over a widget                          |
|  [02]   | `WidgetHandler<TControl,TWidget>`                    | class     | handler binding a native control to a widget        |
|  [03]   | `WidgetHandler<TControl,TWidget,TCallback>`          | class     | handler with a callback channel to the widget       |
|  [04]   | `Style`                                              | static    | style registry keyed by handler type and style name |
|  [05]   | `StyleWidgetHandler<TWidget>`                        | delegate  | style applied against a widget facade               |
|  [06]   | `StyleHandler<THandler>`                             | delegate  | style applied against a concrete handler            |
|  [07]   | `IThemedControlHandler`                              | interface | themed-handler contract                             |
|  [08]   | `ThemedControlHandler<TControl,TWidget,TCallback>`   | class     | control drawn from managed widgets, not native      |
|  [09]   | `ThemedContainerHandler<TControl,TWidget,TCallback>` | class     | themed container variant                            |

[PUBLIC_TYPE_SCOPE]: native-control hosting

- rail: platform-handlers

| [INDEX] | [SYMBOL]                     | [KIND]    | [CAPABILITY]                                            |
| :-----: | :--------------------------- | :-------- | :------------------------------------------------------ |
|  [01]   | `IControlObjectSource`       | interface | exposes the handler-created concrete control object     |
|  [02]   | `NativeControlHost`          | class     | hosts an admitted native object inside the managed tree |
|  [03]   | `CreateNativeControlArgs`    | class     | nullable native-object carrier for subclass creation    |
|  [04]   | `NativeControlHost.IHandler` | interface | creates the native host from the supplied object        |

[PUBLIC_TYPE_SCOPE]: the `Eto.Mac` managed-to-AppKit bridge

- rail: platform-handlers

| [INDEX] | [SYMBOL]                                      | [KIND]    | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------- | :-------- | :---------------------------------------------------- |
|  [01]   | `Eto.Mac.Platform`                            | class     | concrete macOS platform with `ID == "macOS"`          |
|  [02]   | `Eto.Mac.Forms.IMacControlHandler`            | interface | partitions the native `NSView` roles                  |
|  [03]   | `Eto.Mac.Forms.IMacViewHandler`               | interface | control behavior layered over `IMacControlHandler`    |
|  [04]   | `Eto.Mac.Forms.IMacWindow`                    | interface | window behavior layered over `IMacControlHandler`     |
|  [05]   | `Eto.Mac.Forms.IMacControl`                   | interface | weak handler reference carried by generated controls  |
|  [06]   | `Eto.Mac.Forms.MacControlExtensions`          | static    | nullable handler and container-view extraction        |
|  [07]   | `Eto.Mac.Forms.Controls.NativeControlHandler` | class     | admits `NSView`, `NSViewController`, or native handle |
|  [08]   | `Eto.Mac.MacConversions`                      | static    | AppKit and Eto value conversion                       |
|  [09]   | `Eto.Mac.CGConversions`                       | static    | CoreGraphics and Eto value conversion                 |
|  [10]   | `Eto.Mac.MacExtensions`                       | static    | low-level AppKit extensions used by concrete handlers |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Platform` — active platform, identity, and handler factory

- `Eto.Platform`: runtime-nullable `public static Platform? Instance { get; }`; non-null-or-throw `public static Platform Detect { get; }`; `public abstract string ID { get; }`; runtime-nullable `public IDisposable? Context { get; }`
- `public bool Supports<T>() where T : class`, `public virtual bool Supports(Type type)`, runtime-nullable `public Func<object>? Find(Type type)`, `public T Create<T>()`, `public object Create(Type type)`, `public void Add<T>(Func<T> instantiator) where T : class`
- `Eto.Platforms.macOS` is the assembly-qualified `"Eto.Mac.Platform, Eto.macOS"` loader identity; `Eto.Mac.Platform.ID` returns `"macOS"`
- `Eto.Mac.Platform`: `public override bool IsMac => true`, `public override bool IsDesktop => true`, `public override bool IsValid { get; }`, `public override PlatformFeatures SupportedFeatures => CustomCellSupportsControlView | DrawableWithTransparentContent | TabIndexWithCustomContainers`, `public static void AddTo(Eto.Platform platform)`, `public override IDisposable ThreadStart()`
- `PlatformFeatures`: `None`, `CustomCellSupportsControlView`, `DrawableWithTransparentContent`, `TabIndexWithCustomContainers`, `MultiThreadedUI`, `Mnemonics`

[ENTRYPOINT_SCOPE]: `Style` — scoped appearance and behavior

- runtime-nullable `public static IStyleProvider? Provider { get; set; }`, `public static event Action<Widget>? StyleWidget`
- `public static void Add<TWidget>(string? style, StyleWidgetHandler<TWidget> handler) where TWidget : Widget`
- `public static void Add<THandler>(string? style, StyleHandler<THandler> styleHandler) where THandler : class, Widget.IHandler`

[ENTRYPOINT_SCOPE]: `NativeControlHost` — hosting a raw platform view

- `Eto.Forms.NativeControlHost`: `public NativeControlHost(object? nativeControl)`, `public NativeControlHost()`, `protected virtual void OnCreateNativeControl(CreateNativeControlArgs e)`
- `CreateNativeControlArgs`: runtime-nullable `public object? NativeControl { get; set; }`; `NativeControlHost.IHandler`: `void Create(object? nativeControl)`
- `Eto.Mac.Forms.Controls.NativeControlHandler`: public parameterless, `NSView`, and `NSViewController` constructors; `public override NSView ContainerControl { get; }`; `public void Create(object? nativeControl)`
- `NativeControlHandler.Create` admits `null`, `NSView`, `NSViewController`, or an `nint` resolving to `NSView`; `null` creates `MacPanelView`, an unsupported object throws `NotSupportedException`, and a non-view handle throws `InvalidOperationException`

[ENTRYPOINT_SCOPE]: `IMacControlHandler` — AppKit view roles and nullable extraction

- `public interface IMacControlHandler`: `NSView ContainerControl { get; }`, `NSView ContentControl { get; }`, `NSView EventControl { get; }`, `NSView FocusControl { get; }`, `NSView TextInputControl { get; }`, `Size MinimumSize { get; set; }`, `bool IsEventHandled(string eventName)`, `void RecalculateKeyViewLoop(ref NSView last)`, `void InvalidateMeasure()`
- `public static IMacControlHandler? MacControlExtensions.GetMacControl(this Control? control)`, `public static IMacViewHandler? GetMacViewHandler(this Control? control)`, `public static NSView? GetContainerView(this Widget? control)`; each follows nested Eto `ControlObject` values before returning `null`, and `GetContainerView` finally admits a direct `NSView` control object
- `public interface IControlObjectSource`: runtime-nullable `object? ControlObject { get; }`; `WidgetHandler<TControl,TWidget>` implements it explicitly and lazily returns `Control`

[ENTRYPOINT_SCOPE]: `IMacViewHandler` / `IMacWindow` — behavior over native roles

- `public interface IMacViewHandler : IMacControlHandler`: `Control Widget { get; }`, `Control.ICallback Callback { get; }`, `Size UserPreferredSize { get; }`, runtime-nullable `Cursor? CurrentCursor { get; }`, `Color BackgroundColor { get; set; }`, `Dictionary<nint,Command> SystemActions { get; }`, `bool? ShouldHaveFocus { get; set; }`, `bool TextInputCancelled { get; set; }`, `bool TextInputImplemented { get; }`, `bool AutoAttachNative { get; set; }`
- `public interface IMacWindow : IMacControlHandler`: `NSWindow Control { get; }`, `Window Widget { get; }`, `Window.ICallback Callback { get; }`, `Rectangle? RestoreBounds { get; set; }`, runtime-nullable `NSMenu? MenuBar { get; }`, runtime-nullable `NSObject? FieldEditorClient { get; set; }`, `bool CloseWindow(Action<CancelEventArgs>? closing = null)`
- `public interface IMacControl`: `WeakReference WeakHandler { get; set; }`; this weak link lets generated AppKit controls recover their handler and is not a native-control handler marker

[ENTRYPOINT_SCOPE]: macOS value and native-view conversions

- `Eto.Mac.MacConversions`: `public static NSColor ToNSUI(this Color color)`, `public static Color ToEto(this NSColor color, bool calibrated)`, `public static PointF ToEto(this CGPoint locationInWindow, NSView view)`, `public static CGImage ToCG(this Image image)`, `public static NSImage ToNS(this Image image, int? size = null)`, `public static NSColor ToNS(this CGColor color)`
- `Eto.Mac.CGConversions`: `public static CGColor ToCG(this NSColor color)`, `public static CGColor ToCG(this Color color)`, `public static Color ToEto(this CGColor color)`, `public static IMatrix ToEto(this CGAffineTransform matrix)`, `public static CGAffineTransform ToCG(this IMatrix matrix)`, `public static CGPath ToCG(this IGraphicsPath path)`
- `Eto.Mac.MacExtensions`: runtime-nullable `public static NSColor? UsingColorSpaceFast(this NSColor? color, NSString colorSpace)`, `public static void SetClipsToBounds(this NSView view, bool clipsToBounds)`

## [04]-[IMPLEMENTATION_LAW]

[ETO_PLATFORM_TOPOLOGY]:

- Every `Eto.Forms` and `Eto.Drawing` widget delegates to a `WidgetHandler` the active `Platform` mints; `HandlerAttribute` binds the widget type to its handler interface, while `IControlObjectSource.ControlObject` exposes a handler-created control without inventing a second native field.
- `Style` is the scoped-appearance mechanism: a style name plus a registered `StyleHandler<THandler>`/`StyleWidgetHandler<TWidget>` delegate runs against the widget or its concrete handler at attach time, so a canvas or panel restyles without subclassing the control.
- `NativeControlHost` admits `NSView`, `NSViewController`, or a verified native view handle; extraction composes `GetContainerView` or `IMacControlHandler.ContainerControl`, while content, event, focus, and text-input work read their dedicated roles.
- `Eto.Mac.Platform.IsMac`, `ID`, `IsValid`, and `SupportedFeatures` separate runtime identity, app-bundle validity, and admitted capability; the assembly-qualified `Platforms.macOS` string is loader input rather than the runtime identity.

[STACKING]:

- `Eto.Forms`(`.api/api-eto-forms`) and `Eto.Drawing`(`.api/api-eto-drawing`): this substrate owns control-object resolution; `MacConversions`/`CGConversions` carry color, image, point, matrix, and path values into AppKit/CoreGraphics without a local conversion layer.
- `.api/api-macos-native`: `GetContainerView` and `IMacControlHandler.ContainerControl` yield the Eto-backed `NSView`; the native catalog owns the `CALayer`/`NSEvent`/`CADisplayLink` work performed during that view's valid host lifetime.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): the platform-identity vocabulary (`Platforms` ids, `PlatformFeatures` capability flags) maps at the folder boundary onto `[SmartEnum]`/flag owners so a platform-gate decision is exhaustive dispatch, never a stringly-keyed `ID` comparison.
- `LanguageExt.Core`(`.api/api-languageext`): runtime-nullable `Platform.Instance`, `Find`, `GetMacControl`, `GetMacViewHandler`, `GetContainerView`, and `CreateNativeControlArgs.NativeControl` lower onto `Option<T>`/`Fin<T>` at the folder boundary; `Platform.Create<T>()` and invalid native-host payloads remain throwing boundaries caught by the same rail.

[RAIL_LAW]:

- Surface: `Eto.Platform` core substrate and the `Eto.Mac` handler bridge
- Owns: handler resolution per widget, scoped `Style` application, native-control hosting, AppKit role partitioning, nullable container-view extraction, and Eto/AppKit/CoreGraphics conversion
- Accept: initialized `Platform` identity, `HandlerAttribute`-bound handler interfaces, `IMacControlHandler` view roles, `MacControlExtensions` extraction, and the installed conversion owners
- Reject: a widget subclass where a `Style` delegate carries the change, a native lookup beside `GetContainerView`, a loader type string treated as runtime `ID`, or an unguarded runtime-nullable extraction
