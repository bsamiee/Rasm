# [RASM_GRASSHOPPER_API_ETO_PLATFORM]

`Eto.macOS` is the AppKit backend behind every Eto widget a Grasshopper2 panel raises on macOS: `Eto.Mac.Platform` seats the handler set, `IMacControlHandler` partitions the native `NSView` roles for layout, content, events, focus, and text input, `MacControlExtensions` extracts the Eto-backed view, `NativeControlHandler` admits a raw AppKit object, and `MacConversions`/`CGConversions` carry every value across the Eto, AppKit, and CoreGraphics boundary. Handler-factory root itself is the branch surface this partition registers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.macOS` — the AppKit handler partition
- package: `Eto.Forms` macOS backend (BSD-3-Clause)
- assembly: `Eto.macOS.dll` (`Eto.Mac` handler set) over the `Eto.dll` handler root
- namespace: `Eto.Mac`, `Eto.Mac.Forms`, `Eto.Mac.Forms.Controls`
- target: in-process ALC reference inside the Rhino 9 WIP bundle, not a NuGet asset
- rail: platform-handlers

## [02]-[PUBLIC_TYPES]

- Registers the `Eto` platform-handler root (`libs/csharp/.api/api-eto-platform.md`): `Platform` identity, capability probes, the handler-registration map with `Create`/`CreateShared`/`Find`/`Add`, the `HandlerCreated`/`WidgetCreated` mint events, `Platform.Cache<TKey,TValue>`, the boot, context, and marshal surfaces, the `WidgetHandler` family, the `Style` registry, and `NativeControlHost`/`CreateNativeControlArgs`/`IControlObjectSource` carry their algebra there; the rows below are the macOS backend this partition adds beyond it.

[PUBLIC_TYPE_SCOPE]: the `Eto.Mac` managed-to-AppKit bridge

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------ | :------------ | :---------------------------------------------------- |
|  [01]   | `Eto.Mac.Platform`              | class         | concrete macOS platform with `ID == "macOS"`          |
|  [02]   | `IMacControlHandler`            | interface     | partitions the native `NSView` roles                  |
|  [03]   | `IMacViewHandler`               | interface     | control behaviour over `IMacControlHandler`           |
|  [04]   | `IMacWindow`                    | interface     | window behaviour over `IMacControlHandler`            |
|  [05]   | `IMacControl`                   | interface     | weak handler reference on generated controls          |
|  [06]   | `MacControlExtensions`          | static        | nullable handler and container-view extraction        |
|  [07]   | `Controls.NativeControlHandler` | class         | admits `NSView`, `NSViewController`, or native handle |
|  [08]   | `MacConversions`                | static        | AppKit and Eto value conversion                       |
|  [09]   | `CGConversions`                 | static        | CoreGraphics and Eto value conversion                 |
|  [10]   | `MacExtensions`                 | static        | low-level AppKit extensions for concrete handlers     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Eto.Mac.Platform` — macOS backend seating and overrides

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :---------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Platforms.macOS`                               | static   | loader identity for the Mac assembly |
|  [02]   | `Eto.Mac.Platform.AddTo(Platform)`              | static   | register the Mac handler set         |
|  [03]   | `Eto.Mac.Platform.ThreadStart() -> IDisposable` | instance | macOS UI-thread scope                |

- `Platforms.macOS` resolves the loader string `"Eto.Mac.Platform, Eto.macOS"`, which is loader input and never the runtime `ID`.
- `Eto.Mac.Platform` overrides `IsMac`, `IsDesktop`, `IsValid`, and `SupportedFeatures` alone; every other registered `Is*` row answers `false` off the base.
- `Eto.Mac.Platform.SupportedFeatures` returns `CustomCellSupportsControlView | DrawableWithTransparentContent | TabIndexWithCustomContainers`.

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
|  [13]   | `MacControlExtensions.GetPreferredSize(Control, SizeF) -> SizeF`       | static   | measured preferred extent   |
|  [14]   | `MacControlExtensions.HasDarkTheme(NSView) -> bool`                    | static   | per-view dark appearance    |
|  [15]   | `MacControlExtensions.CenterInParent(NSView)`                          | static   | centre within the superview |
|  [16]   | `string.ToAttributedStringWithMnemonic(NSDictionary?)`                 | static   | mnemonic-underlined string  |

- `GetMacControl`, `GetMacViewHandler`, and `GetContainerView` follow nested Eto control objects before returning `null`; `GetContainerView` finally admits a direct `NSView` control object. `GetMacControl` returns `IMacControlHandler` and `GetMacViewHandler` the narrower `IMacViewHandler`, so a role read takes the former and a behaviour read the latter; `MacControlExtensions` lives in `Eto.Mac.Forms` while `MacConversions`/`CGConversions` live in `Eto.Mac`.
- `HasDarkTheme` reads `view?.EffectiveAppearance ?? NSAppearance.CurrentAppearance` and matches `NameDarkAqua` or `NameAccessibilityHighContrastDarkAqua`, so it answers per-view — the correct grain for a canvas on a differently-appearanced screen, where the process-wide current appearance alone answers for the key window.

[ENTRYPOINT_SCOPE]: `IMacViewHandler` / `IMacWindow` — behaviour over native roles

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `IMacViewHandler.Widget -> Control`                         | property | owning Eto control           |
|  [02]   | `IMacViewHandler.Callback -> Control.ICallback`             | property | control callback channel     |
|  [03]   | `IMacViewHandler.UserPreferredSize -> Size`                 | property | requested extent             |
|  [04]   | `IMacViewHandler.CurrentCursor -> Cursor?`                  | property | active cursor                |
|  [05]   | `IMacViewHandler.BackgroundColor -> Color`                  | property | settable background colour   |
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

[ENTRYPOINT_SCOPE]: `NativeControlHandler` — admitting a raw AppKit object

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------------------ | :------- | :------------------------- |
|  [01]   | `NativeControlHandler()`                          | ctor     | empty native host          |
|  [02]   | `NativeControlHandler(NSView)`                    | ctor     | host an existing `NSView`  |
|  [03]   | `NativeControlHandler(NSViewController)`          | ctor     | host a view controller     |
|  [04]   | `NativeControlHandler.ContainerControl -> NSView` | property | the hosting container view |
|  [05]   | `NativeControlHandler.Create(object?)`            | instance | admit the native payload   |

- `Create` admits `null`, `NSView`, `NSViewController`, or an `nint` resolving to `NSView`; `null` mints `MacPanelView`, an unsupported object throws `NotSupportedException`, and a non-view handle throws `InvalidOperationException`.

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

- `ToEtoWithAppearance` resolves a dynamic `NSColor` against the current appearance where `ToEto` resolves it against the archived one, so a swatch read across a light or dark flip is stale unless it took the appearance-aware member; `ToEto(NSColorSpace)` pins an explicit space instead.
- `GetMouseEvent`, `GetMouseButtons`, and `ToEtoKeyEventArgs` are the host's own `NSEvent`-to-Eto projection: modifier mask, button set, click count, location, and wheel delta decode once into the Eto event payloads, so a raw `NSEventType` switch re-derives a shipped correspondence.

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
- `Color.ToCG()` unwraps a control-object-backed `NSColor` or `CGColor` and otherwise builds componentwise off the Eto colour's own space.
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
- `Eto.Mac.Platform` separates runtime identity (`IsMac`, `ID`), app-bundle validity (`IsValid`), and admitted capability (`SupportedFeatures`), each read off the concrete override rather than the registered base.
- `IMacControlHandler` partitions one Eto control into five named `NSView` roles, so layout, content, event, focus, and text-input work each read their dedicated role and never the container by default.
- `NativeControlHandler` admits `NSView`, `NSViewController`, or a verified native handle; `GetContainerView` or `ContainerControl` yields the Eto-backed view every AppKit call runs against.
- Conversion is one bidirectional owner per correspondence: `MacConversions` carries the AppKit direction and `CGConversions` the CoreGraphics direction, and this partition is their single home — a second conversion table beside them is the dual-home defect in miniature.

[STACKING]:
- `api-eto-platform`(`libs/csharp/.api/api-eto-platform.md`): the registered handler root this backend seats into — `Platform/handlers.md` reads the registered identity, registration, mint-event, and shared-instance surfaces and this partition supplies their macOS answers.
- `api-eto-forms`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-forms.md`) and `api-eto-drawing`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-drawing.md`): the handler root resolves the control object each consumes, and the conversion owners here carry colour, image, point, matrix, and path values into AppKit and CoreGraphics with no local conversion layer.
- `api-macos-native`(`libs/csharp/Rasm.Grasshopper/.api/api-macos-native.md`): `GetContainerView` and `ContainerControl` yield the Eto-backed `NSView`; the native catalog owns the layer, event, and display-link work over that view's valid host lifetime.
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the registered platform identity and feature vocabularies map onto `[SmartEnum]` and flag owners, so a platform gate is exhaustive dispatch rather than a stringly-keyed comparison.
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): the runtime-nullable extractions — `GetMacControl`, `GetMacViewHandler`, `GetContainerView` — lower onto `Option<T>`/`Fin<T>` at the folder boundary, and the throwing conversions stay caught boundaries the same rail traps.

[LOCAL_ADMISSION]:
- Native-view work enters through `GetContainerView` or `IMacControlHandler.ContainerControl` behind the folder nullable boundary; raw AppKit runs only inside a verified host-valid window lifetime.
- Every Eto-to-AppKit or Eto-to-CoreGraphics value crossing takes a `MacConversions` or `CGConversions` member; a local conversion beside them is the deleted form.
- Handler resolution, style registration, and native-host construction take the registered branch surface; this partition never re-mints them.

[RAIL_LAW]:
- Partition: `Eto.macOS` AppKit backend — the `Eto.Mac` handler set, native `NSView` role partitioning, container-view extraction, and the Eto/AppKit/CoreGraphics conversion surface
- Owns: the macOS backend and its value bridge over the registered branch handler root
- Accept: the seated `Eto.Mac` platform and its overrides, `IMacControlHandler` view roles, `MacControlExtensions` extraction, admitted AppKit payloads, and the installed conversion owners
- Reject: a re-tabling of the branch handler root, a native lookup beside `GetContainerView`, a local Eto-to-AppKit conversion beside `MacConversions`, an unguarded runtime-nullable extraction, and a loader type string treated as runtime `ID`
