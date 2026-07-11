# [RASM_GRASSHOPPER_API_ETO_PLATFORM]

`Eto.Platform` owns the handler substrate every `Eto.Forms` control and `Eto.Drawing` object resolves its native backing through: a widget is a thin managed facade, and its concrete behavior lives in a `WidgetHandler` the active `Platform` mints per widget type. `Style` scopes appearance and behavior onto a handler without subclassing; `NativeControlHost` hosts a raw platform view inside the managed tree, and the `Eto.Mac` platform, its conversion owners, and its `IMac*Handler` interfaces form the managed-to-AppKit bridge — the path by which an `Eto.Forms` control or Grasshopper2 canvas yields the concrete `NSView` the macOS-native compositing surface draws onto.

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

| [INDEX] | [SYMBOL]                     | [KIND]    | [CAPABILITY]                                             |
| :-----: | :--------------------------- | :-------- | :------------------------------------------------------- |
|  [01]   | `Platform`                   | abstract  | active-platform root; resolves a handler per widget type |
|  [02]   | `Platforms`                  | static    | well-known platform-identifier string anchors            |
|  [03]   | `PlatformFeatures`           | enum      | flags of per-platform capability                         |
|  [04]   | `HandlerAttribute`           | attribute | binds a widget type to its handler interface             |
|  [05]   | `PlatformExtensionAttribute` | attribute | registers a platform extension assembly                  |

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

| [INDEX] | [SYMBOL]                  | [KIND] | [CAPABILITY]                                      |
| :-----: | :------------------------ | :----- | :------------------------------------------------ |
|  [01]   | `NativeControlHost`       | class  | hosts a raw platform view inside the managed tree |
|  [02]   | `CreateNativeControlArgs` | class  | request carrier for native-control creation       |

[PUBLIC_TYPE_SCOPE]: the `Eto.Mac` managed-to-AppKit bridge

- rail: platform-handlers

| [INDEX] | [SYMBOL]                 | [KIND]    | [CAPABILITY]                                          |
| :-----: | :----------------------- | :-------- | :---------------------------------------------------- |
|  [01]   | `Eto.Mac.Platform`       | class     | concrete macOS platform binding widgets to AppKit     |
|  [02]   | `Eto.Mac.MacConversions` | static    | `Eto.Drawing` to CoreGraphics/AppKit value conversion |
|  [03]   | `Eto.Mac.CGConversions`  | static    | `Eto.Drawing` to CoreGraphics geometry conversion     |
|  [04]   | `Eto.Mac.MacExtensions`  | static    | AppKit interop extension helpers                      |
|  [05]   | `Eto.Mac.Drawing`        | namespace | CoreGraphics-backed `Eto.Drawing` handler set         |
|  [06]   | `IMacViewHandler`        | interface | yields the backing `NSObject`/`NSView` of a control   |
|  [07]   | `IMacWindow`             | interface | yields the backing `NSWindow` and restore bounds      |
|  [08]   | `IMacControl`            | interface | native-control handler marker                         |
|  [09]   | `IColorDialogHandler`    | interface | macOS color-dialog handler                            |
|  [10]   | `ITextAreaHandler`       | interface | macOS text-area handler                               |
|  [11]   | `IMacTextBoxHandler`     | interface | macOS text-box handler                                |
|  [12]   | `IGridHandler`           | interface | macOS grid handler                                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Platform` — active platform and detection

- `static Platform Instance { get; }`, `static Platform Detect { get; }`, `abstract string ID { get; }`
- `virtual bool IsMac`, `virtual bool IsWinForms`, `virtual bool IsWpf`, `virtual bool IsGtk`, `virtual bool IsDesktop`, `virtual bool IsMobile`, `virtual bool IsValid`
- `virtual PlatformFeatures SupportedFeatures`
- `Platforms.macOS = "Eto.Mac.Platform, Eto.macOS"` is the concrete macOS identifier this host resolves to
- `PlatformFeatures`: `None`, `CustomCellSupportsControlView`, `DrawableWithTransparentContent`, `TabIndexWithCustomContainers`, `MultiThreadedUI`, `Mnemonics`

[ENTRYPOINT_SCOPE]: `Style` — scoped appearance and behavior

- `static IStyleProvider Provider { get; set; }`, `static event Action<Widget> StyleWidget`
- `static void Add<TWidget>(string style, StyleWidgetHandler<TWidget> handler) where TWidget : Widget`
- `static void Add<THandler>(string style, StyleHandler<THandler> styleHandler) where THandler : class, Widget.IHandler`

[ENTRYPOINT_SCOPE]: `NativeControlHost` — hosting a raw platform view

- `NativeControlHost(object nativeControl)`, `NativeControlHost()`, `protected virtual void OnCreateNativeControl(CreateNativeControlArgs e)`
- `CreateNativeControlArgs` is the request carrier the handler fills when the host mints its native view
- the backing view of an `Eto.Forms` control is recovered by casting its handler to `IMacViewHandler` and reading `IMacViewHandler.Control`

[ENTRYPOINT_SCOPE]: `IMacViewHandler` / `IMacWindow` — the AppKit backing

- `Eto.Mac.Forms.IMacViewHandler`: `NSObject Control { get; }`, `Widget Widget { get; }`, `NSObject SystemActions { get; }`, `bool TextInputCancelled { get; set; }`
- `Eto.Mac.Forms.IMacWindow`: `NSWindow Control { get; }`, `Rectangle RestoreBounds { get; set; }`, `Widget Widget { get; }`

## [04]-[IMPLEMENTATION_LAW]

[ETO_PLATFORM_TOPOLOGY]:

- Every `Eto.Forms` and `Eto.Drawing` widget is a facade whose behavior lives in a `WidgetHandler` the active `Platform` mints; `HandlerAttribute` binds the widget type to its handler interface, and `Platform.Instance` is the single resolution root — a control never carries native state directly.
- `Style` is the scoped-appearance mechanism: a style name plus a registered `StyleHandler<THandler>`/`StyleWidgetHandler<TWidget>` delegate runs against the widget or its concrete handler at attach time, so a canvas or panel restyles without subclassing the control.
- `NativeControlHost` and the `IMac*Handler` interfaces are two directions of one bridge: hosting embeds a raw `NSView` inside the managed tree, and extraction reads `control.Handler as IMacViewHandler` to recover the backing `NSView`/`NSWindow` — the entry the macOS-native compositing and gesture surfaces attach to.
- `Platform.IsMac` and `Platform.SupportedFeatures` gate platform-specific capability, so a cosmetic or handler path binds only where the concrete `Eto.Mac.Platform` admits it and the non-macOS branch skips it.

[STACKING]:

- `Eto.Forms`(`.api/api-eto-forms`) and `Eto.Drawing`(`.api/api-eto-drawing`): this substrate is the backing every control and drawing object resolves; `MacConversions`/`CGConversions` convert `Eto.Drawing.Color`/`RectangleF`/`GraphicsPath` into the AppKit and CoreGraphics values the concrete handlers apply.
- `.api/api-macos-native`: `IMacViewHandler.Control` and `Grasshopper2.UI.Canvas.Canvas.ControlObject` are the two sources of the concrete `NSView` the native surface composites onto; this catalog owns the managed extraction, and the native catalog owns the `CALayer`/`NSEvent`/`CADisplayLink` work performed on the extracted view.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): the platform-identity vocabulary (`Platforms` ids, `PlatformFeatures` capability flags) maps at the folder boundary onto `[SmartEnum]`/flag owners so a platform-gate decision is exhaustive dispatch, never a stringly-keyed `ID` comparison.
- `LanguageExt.Core`(`.api/api-languageext`): a handler cast that can miss (`control.Handler as IMacViewHandler`, `NativeControl` extraction, `Platform.Create<T>()`) lowers onto `Option<T>`/`Fin<T>` at the folder boundary — a null handler or absent native view is a `None`, never an unguarded cast that throws inside a paint or gesture path.

[RAIL_LAW]:

- Surface: `Eto.Platform` core substrate and the `Eto.Mac` handler bridge
- Owns: handler resolution per widget, scoped `Style` application, native-control hosting, and the managed-to-AppKit extraction of the backing `NSView`/`NSWindow`
- Accept: `Platform.Instance` as the resolution root, `HandlerAttribute`-bound handler interfaces, `IMacViewHandler`/`IMacWindow` as the AppKit-backing contract, `MacConversions`/`CGConversions` at the value boundary
- Reject: a widget subclass where a `Style` delegate carries the change, a hand-rolled `NSView` lookup beside `IMacViewHandler.Control`, a stringly-keyed platform `ID` switch the `[SmartEnum]` boundary owns, an unguarded handler cast that throws instead of lowering onto `Option<T>`
