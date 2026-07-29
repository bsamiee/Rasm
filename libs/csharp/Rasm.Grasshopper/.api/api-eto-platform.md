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
|  [06]   | `HandlerCreatedEventArgs`    | class         | handler-mint raise payload                   |
|  [07]   | `WidgetCreatedEventArgs`     | class         | widget-mint raise payload                    |

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

[ENTRYPOINT_SCOPE]: `Platform` — identity, capability probe, and the platform-row assertions

| [INDEX] | [SURFACE]                          | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :--------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Platform.Instance`                | static   | active platform, runtime-nullable    |
|  [02]   | `Platform.Detect`                  | static   | active platform or throw             |
|  [03]   | `Platform.ID`                      | property | platform identity string             |
|  [04]   | `Platform.IsMac`                   | property | macOS-platform assertion             |
|  [05]   | `Platform.IsWinForms`              | property | WinForms-platform assertion          |
|  [06]   | `Platform.IsWpf`                   | property | WPF-platform assertion               |
|  [07]   | `Platform.IsGtk`                   | property | GTK-platform assertion               |
|  [08]   | `Platform.IsIos`                   | property | iOS-platform assertion               |
|  [09]   | `Platform.IsAndroid`               | property | Android-platform assertion           |
|  [10]   | `Platform.IsDesktop`               | property | desktop-form-factor assertion        |
|  [11]   | `Platform.IsMobile`                | property | mobile-form-factor assertion         |
|  [12]   | `Platform.IsValid`                 | property | app-bundle validity                  |
|  [13]   | `Platform.SupportedFeatures`       | property | admitted capability flags            |
|  [14]   | `Platform.Supports<T>() -> bool`   | instance | capability probe by type parameter   |
|  [15]   | `Platform.Supports(Type) -> bool`  | instance | capability probe by type             |
|  [16]   | `Platforms.macOS`                  | static   | loader identity for the Mac assembly |
|  [17]   | `Eto.Mac.Platform.AddTo(Platform)` | static   | register the Mac handler set         |

- `Platforms.macOS` resolves the loader string `"Eto.Mac.Platform, Eto.macOS"`.
- Every `Is*` row is `virtual` and answers `false` on the base, so a platform row's probe reads the concrete override; `Eto.Mac.Platform` overrides `IsMac`, `IsDesktop`, `IsValid`, and `SupportedFeatures` alone.
- `Eto.Mac.Platform.SupportedFeatures` returns `CustomCellSupportsControlView | DrawableWithTransparentContent | TabIndexWithCustomContainers`.
- `[PlatformFeatures]`: `None` `CustomCellSupportsControlView` `DrawableWithTransparentContent` `TabIndexWithCustomContainers` `MultiThreadedUI` `Mnemonics`

[ENTRYPOINT_SCOPE]: `Platform` — handler registration, resolution, and the mint raises

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Platform.Add<T>(Func<T>)`                             | instance | register a handler instantiator        |
|  [02]   | `Platform.Add(Type, Func<object>)`                     | instance | register against an explicit contract  |
|  [03]   | `Platform.Find(Type) -> Func<object>?`                 | instance | handler-factory lookup, nullable       |
|  [04]   | `Platform.Find<T>() -> Func<T>?`                       | instance | typed factory lookup, nullable         |
|  [05]   | `Platform.Create<T>() -> T`                            | instance | instantiate a registered handler       |
|  [06]   | `Platform.Create(Type) -> object`                      | instance | instantiate by type                    |
|  [07]   | `Platform.CreateShared<T>() -> T`                      | instance | platform-cached singleton per contract |
|  [08]   | `Platform.CreateShared(Type) -> object`                | instance | shared instance by type                |
|  [09]   | `Platform.Cache<TKey,TValue>(object) -> Dictionary<…>` | instance | shared per-key dictionary slot         |
|  [10]   | `Platform.HandlerCreated`                              | event    | every handler mint raises              |
|  [11]   | `Platform.WidgetCreated`                               | event    | every widget mint raises               |
|  [12]   | `HandlerCreatedEventArgs.Instance -> object`           | property | the minted handler, read-only          |
|  [13]   | `WidgetCreatedEventArgs.Instance -> Widget`            | property | the minted widget, read-only           |

- `Add(Type, Func<object>)` registers under both the supplied type and its `[Handler]`-declared contract, then clears the resolved-handler cache; `Add<T>` forwards to it, so one registration serves both keys.
- `CreateShared` resolves through the same instantiator map as `Create` and memoizes per contract on the platform instance, so a shared handler outlives every widget that reaches it — `Create` is the per-call mint and the two never substitute.
- `Cache<TKey,TValue>` hands back a platform-lifetime dictionary keyed by an opaque cache key; it is the same shared-property store `CreateShared` uses.

[ENTRYPOINT_SCOPE]: `Platform` — boot, context, and marshal (composition-root surfaces)

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :---------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Platform.Initialize(Platform)`                 | static   | seat the global platform            |
|  [02]   | `Platform.Initialize(string)`                   | static   | seat by loader type string          |
|  [03]   | `Platform.AllowReinitialize`                    | static   | settable re-seat admission          |
|  [04]   | `Platform.Get(string) -> Platform`              | static   | resolve a platform by loader string |
|  [05]   | `Platform.Copy(Platform?) -> Platform`          | static   | clone with the instantiator map     |
|  [06]   | `Platform.LoadAssembly(string)`                 | instance | load a platform extension by name   |
|  [07]   | `Platform.LoadAssembly(Assembly)`               | instance | load a platform extension assembly  |
|  [08]   | `Platform.Context -> IDisposable?`              | property | ambient context, runtime-nullable   |
|  [09]   | `Platform.ThreadStart() -> IDisposable`         | instance | UI-thread scope, base returns null  |
|  [10]   | `Eto.Mac.Platform.ThreadStart() -> IDisposable` | instance | macOS UI-thread scope               |
|  [11]   | `Platform.Invoke(Action)`                       | instance | run inside this platform's context  |
|  [12]   | `Platform.Invoke<T>(Func<T>) -> T`              | instance | run and return inside the context   |

- `Context` returns `null` when this platform is already the ambient instance and a fresh `PlatformContext` push otherwise, so a `using` over it is a no-op on the ambient path rather than a second frame; a null-guard at the call site restates what the language already tolerates.
- `Invoke` folds `Context` and the call into one member, so a foreign-platform body never spells the `using` itself; `ThreadStart` returns `null` on the base and a real scope only where a platform overrides it.
- `Copy` clones the instantiator map onto a fresh instance of the same platform type and throws when neither a global nor an argument platform exists; `Get` resolves a loader string and `Initialize(string)` is `Initialize(Get(...))`.

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

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                |
| :-----: | :--------------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `IMacControlHandler.ContainerControl -> NSView`                        | property | layout container role       |
|  [02]   | `IMacControlHandler.ContentControl -> NSView`                          | property | content role                |
|  [03]   | `IMacControlHandler.EventControl -> NSView`                            | property | event role                  |
|  [04]   | `IMacControlHandler.FocusControl -> NSView`                            | property | focus role                  |
|  [05]   | `IMacControlHandler.TextInputControl -> NSView`                        | property | text-input role             |
|  [06]   | `IMacControlHandler.MinimumSize`                                       | property | minimum extent              |
|  [07]   | `IMacControlHandler.IsEventHandled(string) -> bool`                    | instance | event-name handled probe    |
|  [08]   | `IMacControlHandler.RecalculateKeyViewLoop(ref NSView)`                | instance | rebuild the key-view chain  |
|  [09]   | `IMacControlHandler.InvalidateMeasure()`                               | instance | drop the cached measure     |
|  [10]   | `MacControlExtensions.GetMacControl(Control?) -> IMacControlHandler?`  | static   | extract the handler         |
|  [11]   | `MacControlExtensions.GetMacViewHandler(Control?) -> IMacViewHandler?` | static   | extract the view handler    |
|  [12]   | `MacControlExtensions.GetContainerView(Widget?) -> NSView?`            | static   | extract the container view  |
|  [13]   | `IControlObjectSource.ControlObject -> object?`                        | property | handler-created control     |
|  [14]   | `MacControlExtensions.HasDarkTheme(NSView) -> bool`                    | static   | per-view dark appearance    |
|  [15]   | `MacControlExtensions.CenterInParent(NSView)`                          | static   | centre within the superview |
|  [16]   | `string.ToAttributedStringWithMnemonic(NSDictionary?)`                 | static   | mnemonic-underlined string  |
|  [17]   | `MacControlExtensions.GetPreferredSize(Control, SizeF) -> SizeF`       | static   | measured preferred extent   |

- `GetMacControl`, `GetMacViewHandler`, and `GetContainerView` follow nested Eto `ControlObject` values before returning `null`; `GetContainerView` finally admits a direct `NSView` control object. `GetMacControl` returns `IMacControlHandler` and `GetMacViewHandler` the narrower `IMacViewHandler`, so a role read takes the former and a behavior read the latter; `MacControlExtensions` lives in `Eto.Mac.Forms` while `MacConversions`/`CGConversions` live in `Eto.Mac`.
- `HasDarkTheme` reads `view?.EffectiveAppearance ?? NSAppearance.CurrentAppearance` and matches `NameDarkAqua` or `NameAccessibilityHighContrastDarkAqua`, so it answers per-view — the correct grain for a canvas on a differently-appearanced screen, where the process-wide `CurrentAppearance` alone answers for the key window.
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

[ENTRYPOINT_SCOPE]: `MacConversions` — the Eto/AppKit value bridge

Every row is an extension over the named receiver; `ToNS`/`ToNSUI` is the Eto-to-AppKit direction and `ToEto` its inverse on the same owner, so a correspondence is one member pair, never two direction-named helpers.

| [INDEX] | [FAMILY]      | [SURFACE]                                                                                  |
| :-----: | :------------ | :----------------------------------------------------------------------------------------- |
|  [01]   | colour        | `Color.ToNSUI()`; `NSColor.ToEto([bool calibrated])`; `NSColor.ToEto(NSColorSpace)`        |
|  [02]   | colour        | `NSColor.ToEtoWithAppearance([bool calibrated])`; `CGColor.ToNS()`                         |
|  [03]   | image         | `Image.ToCG()`; `Image.ToNS(int? size)`; `NSImage.ToEto()`                                 |
|  [04]   | geometry      | `CGPoint.ToEto(NSView)`; `NSEdgeInsets.ToEtoSize()`; `NSEdgeInsets.ToEto() -> Padding`     |
|  [05]   | range         | `Range<int>.ToNS() -> NSRange`; `NSRange.ToEto() -> Range<int>`                            |
|  [06]   | input event   | `MacConversions.GetMouseEvent(IMacViewHandler, NSEvent, bool includeWheel)`                |
|  [07]   | input event   | `NSEvent.GetMouseButtons()`; `NSEvent.ToEtoKeyEventArgs()`                                 |
|  [08]   | grid cell     | `CreateCellEventArgs(GridColumn, NSView, int, int, object)`; `CreateCellMouseEventArgs(…)` |
|  [09]   | font          | `FontStyle.ToNS()`; `NSFontTraitMask.ToEto()`; `NSFont.ToEto()`; `Font.ToNS()`             |
|  [10]   | text layout   | `NSTextAlignment.ToEto()`; `TextAlignment.ToNS()`; `FormattedTextAlignment.ToNS()`         |
|  [11]   | text layout   | `FormattedTextTrimming.ToNS()`; `FormattedTextWrapMode.ToNS()`; `WrapMode.ToNS()`          |
|  [12]   | interpolation | `ImageInterpolation.ToNS()`; `NSImageInterpolation.ToEto()`                                |
|  [13]   | printing      | `PageOrientation.ToNS()`; `NSPrintingOrientation.ToEto()`                                  |
|  [14]   | printing      | `NSPrintInfo.ToEto() -> PrintSettings`; `PrintSettings.ToNS() -> NSPrintInfo`              |
|  [15]   | button        | `ButtonImagePosition.ToNS()`; `NSCellImagePosition.ToEto()`                                |
|  [16]   | window        | `NSWindowStyle.ToEtoWindowStyle()`; `WindowStyle.ToNS(NSWindowStyle existing)`             |
|  [17]   | calendar      | `NSDatePickerMode.ToEto() -> CalendarMode`; `CalendarMode.ToNS()`                          |
|  [18]   | dock          | `NSTabViewType.ToEto() -> DockPosition`; `DockPosition.ToNS()`                             |
|  [19]   | border        | `NSBorderType.ToEto()`; `BorderType.ToNS()`                                                |
|  [20]   | transfer      | `NSPasteboard.ToEto() -> DataObject`; `DataObject.ToNS()`                                  |
|  [21]   | transfer      | `DragEffects.ToNS() -> NSDragOperation`; `NSDragOperation.ToEto()`                         |
|  [22]   | chrome        | `ContextMenu.ToNS() -> NSMenu`; `Cursor.ToNS() -> NSCursor`                                |
|  [23]   | scalar        | `Uri.ToNS()`; `DateTime.ToNS()`; `DateTime?.ToNS()`; `NSDate.ToEto() -> DateTime?`         |

- `ToEtoWithAppearance` resolves a dynamic `NSColor` against `NSAppearance.CurrentAppearance` where `ToEto` resolves it against the archived one, so a swatch read across a light/dark flip is stale unless it took the appearance-aware member; `ToEto(NSColorSpace)` pins an explicit space instead.
- `GetMouseEvent`, `GetMouseButtons`, and `ToEtoKeyEventArgs` are the host's own `NSEvent`-to-Eto projection: modifier mask, button set, click count, location, and wheel delta decode once into `MouseEventArgs`/`KeyEventArgs`, so a raw `NSEventType` switch re-derives a shipped correspondence.

[ENTRYPOINT_SCOPE]: `CGConversions` — the Eto/CoreGraphics value bridge

| [INDEX] | [FAMILY]      | [SURFACE]                                                                            |
| :-----: | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | colour        | `NSColor.ToCG()`; `Color.ToCG()`; `CGColor.ToEto()`                                  |
|  [02]   | interpolation | `ImageInterpolation.ToCG()`; `CGInterpolationQuality.ToEto()`                        |
|  [03]   | matrix        | `CGAffineTransform.ToEto() -> IMatrix`; `IMatrix.ToCG()`; `DegreesToRadians(NFloat)` |
|  [04]   | pen           | `PenLineJoin.ToCG()`; `CGLineJoin.ToEto()`; `PenLineCap.ToCG()`; `CGLineCap.ToEto()` |
|  [05]   | paint apply   | `Pen.Apply(GraphicsHandler)`; `Pen.Finish(GraphicsHandler)`                          |
|  [06]   | paint apply   | `Brush.Draw(GraphicsHandler, bool stroke, FillMode, bool clip = true)`               |
|  [07]   | path          | `IGraphicsPath.ToCG() -> CGPath`; `IGraphicsPath.ToHandler() -> GraphicsPathHandler` |

- `NSColor.ToCG()` is a four-arm chain and the arm decides colour fidelity: a null receiver returns null; a non-null `color.CGColor` hands back the colour IN ITS OWN SPACE, so a `FromDisplayP3` mint crosses wide-gamut intact; otherwise `UsingColorSpace(NSColorSpace.SRGBColorSpace)` re-spaces to sRGB — a silent gamut clamp — and a null re-space falls to an opaque-black `CGColor(0, 0, 0, 1f)`. Any crossing that must stay wide asserts the returned `ColorSpace` rather than trusting the call, because the clamp and the floor are indistinguishable from success at the call site.
- `Color.ToCG()` unwraps a `ControlObject`-backed `NSColor` or `CGColor` and otherwise builds componentwise off the Eto colour's own space.
- `CGColor.ToEto()` throws on an unsupported component layout and `IMatrix.ToCG()` yields identity for a null matrix; both stay caught at the boundary.
- `Pen.Apply`/`Finish` and `Brush.Draw` bracket a CoreGraphics stroke or fill against a `GraphicsHandler`, so pen state and fill mode enter the context through the bridge rather than through hand-set `CGContext` properties.

[ENTRYPOINT_SCOPE]: `MacExtensions` — low-level AppKit reach

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]               |
| :-----: | :-------------------------------------------------------------------------- | :------ | :------------------------- |
|  [01]   | `NSColor.UsingColorSpaceFast(NSString) -> NSColor`                          | static  | fast colorspace conversion |
|  [02]   | `NSView.SetClipsToBounds(bool)`                                             | static  | set layer clipping         |
|  [03]   | `NSAttributedString.BoundingRect(CGSize, NSStringDrawingOptions) -> CGRect` | static  | measure attributed text    |
|  [04]   | `NSLayoutManager.DrawGlyphs(NSRange, CGPoint)`                              | static  | draw a glyph run           |
|  [05]   | `MacExtensions.Retain(nint)` / `Release(nint)`                              | static  | raw handle refcount        |
|  [06]   | `NSTextView.ShouldChangeTextNew(NSRange, string) -> bool`                   | static  | text-edit admission probe  |
|  [07]   | `NSPasteboard.CanReadItemWithDataConformingToTypes(NSString[]) -> bool`     | static  | UTI-conformance drop probe |
|  [08]   | `NSScrollView.FrameSizeForContentSize(CGSize, bool, bool) -> CGSize`        | static  | frame extent for content   |
|  [09]   | `NSScrollView.ContentSizeForFrame(CGSize, bool, bool) -> CGSize`            | static  | content extent for frame   |

- `Retain`/`Release` take a raw `nint` and bypass the managed lifetime entirely; a native handle held across a managed boundary pairs them inside one capsule or leaks, so they never appear as loose call-site statements.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every `Eto.Forms` and `Eto.Drawing` widget delegates to a `WidgetHandler` the active `Platform` mints; `HandlerAttribute` binds the widget type to its handler interface, and `IControlObjectSource.ControlObject` exposes the handler-created control without a second native field.
- `Style` restyles a control by name: a registered `StyleHandler<THandler>` or `StyleWidgetHandler<TWidget>` runs at attach time against the widget or its concrete handler, so a canvas or panel never subclasses to change appearance.
- `NativeControlHost` admits `NSView`, `NSViewController`, or a verified native handle; `GetContainerView` or `IMacControlHandler.ContainerControl` yields the Eto-backed view, and content, event, focus, and text-input work read their dedicated roles.
- `Eto.Mac.Platform` separates runtime identity (`IsMac`, `ID`), app-bundle validity (`IsValid`), and admitted capability (`SupportedFeatures`); `Platforms.macOS` is loader input, never the runtime `ID`.

[STACKING]:
- `Platform/handlers.md` is the folder's one composer of this surface: `PlatformSeam` reads `Instance`, `ID`, the six `Is*` assertions, `IsDesktop`/`IsMobile`/`IsValid`, `SupportedFeatures`, `Supports(Type)`, and `Context`; `Handlers` reads `Add<T>`, `Create(Type)`, `CreateShared(Type)`, `HandlerCreated`, `WidgetCreated`, and both `Instance` payloads; `Styler` reads the `Style` family; `Bridge` reads `NativeControlHost` and `CreateNativeControlArgs`. `Platform/native.md` composes the `MacConversions` event triple through `NativeInput`, and `Platform/composition.md` composes `MacConversions.ToEtoWithAppearance` through `WideColor.OfSystem` and `CGConversions.ToCG(this IGraphicsPath)` through `LayerPaint.Stroked`. `Initialize`, `Get`, `Copy`, `AllowReinitialize`, `LoadAssembly`, `Invoke`, and `ThreadStart` are boot and marshal surfaces `handlers.md` `[02]` routes away — the plugin shell spends them once and `Eto/runtime.md`'s `EtoDispatch` supersedes the marshal pair.
- `Eto.Forms`(`.api/api-eto-forms`) and `Eto.Drawing`(`.api/api-eto-drawing`): this substrate resolves the control object each consumes, and `MacConversions`/`CGConversions` carry color, image, point, matrix, and path values into AppKit/CoreGraphics with no local conversion layer.
- `.api/api-macos-native`: `GetContainerView` and `IMacControlHandler.ContainerControl` yield the Eto-backed `NSView`; the native catalog owns the `CALayer`/`NSEvent`/`CADisplayLink` work over that view's valid host lifetime.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): the identity vocabulary — `Platforms` ids and `PlatformFeatures` flags — maps at the folder boundary onto `[SmartEnum]` and flag owners, so a platform-gate decision is exhaustive dispatch rather than a stringly-keyed `ID` comparison.
- `LanguageExt.Core`(`.api/api-languageext`): runtime-nullable `Platform.Instance`, `Find`, `GetMacControl`, `GetMacViewHandler`, `GetContainerView`, and `CreateNativeControlArgs.NativeControl` lower onto `Option<T>`/`Fin<T>` at the folder boundary; `Platform.Create<T>()` and invalid native-host payloads stay throwing boundaries the same rail traps.

[LOCAL_ADMISSION]:
- Every widget composes its handler through `Platform.Create` or `Find`; a folder page never re-mints a `WidgetHandler` the active `Platform` already owns.
- Native-view work enters through `GetContainerView` or `IMacControlHandler.ContainerControl` behind the folder `Option<NSView>` boundary; raw AppKit runs only inside a verified host-valid window lifetime.
- Appearance changes ride a `Style` delegate registered by name, never a control subclass.

[RAIL_LAW]:
- Package: `Eto.Forms`
- Owns: per-widget handler resolution, scoped `Style` application, native-control hosting, AppKit `NSView` role partitioning, nullable container-view extraction, and Eto/AppKit/CoreGraphics value conversion.
- Accept: an initialized `Platform` identity, `HandlerAttribute`-bound handler interfaces, `IMacControlHandler` view roles, `MacControlExtensions` extraction, and the installed `MacConversions`/`CGConversions` owners.
- Reject: a control subclass where a `Style` delegate carries the change, a native lookup beside `GetContainerView`, a loader type string treated as runtime `ID`, an unguarded runtime-nullable extraction, or a local Eto-to-AppKit conversion beside `MacConversions`.
