# [RASM_RHINO_API_ETO_PLATFORM]

`Eto.Platform` is the ambient backend-handler seam beneath every widget: one platform resolves each control, layout, graphics handle, and dialog to a native handler. This catalog owns platform identity and feature discovery (`Supports`/`Create`/`Find`), the `NativeControlHost` embedding lifecycle over `AttachNative`/`DetachNative`, and the `Style`/`TriggerStyleChanged` theme-transition seam that re-applies handlers when the host flips light or dark, including the Rhino `EtoExtensions.Get` notifier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto` 'platform handler surface'
- package: `Eto` (host-provided; bound in-place from the Rhino-loaded `Eto.dll`, never a second NuGet admission)
- license: BSD-3-Clause
- assembly: `Eto.dll` (Rhino `RhCore` framework); the theme notifier is `Rhino.UI.dll`
- namespace: `Eto`, `Eto.Forms`, `Rhino.UI`
- asset: one platform handler the Rhino process resolves for its loaded `Eto.dll`
- rail: platform-handler

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: platform identity and feature discovery
- namespace: `Eto`
- rail: platform-handler

| [INDEX] | [SYMBOL]                     | [KIND]          | [CAPABILITY]                                              |
| :-----: | :--------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `Platform`                   | backend         | the ambient handler factory every widget resolves through |
|  [02]   | `Platform.Instance`          | static property | the current process platform                              |
|  [03]   | `Platform.Detect`            | static property | the platform detected for the host environment            |
|  [04]   | `Platform.ID`                | property        | platform identifier string                                |
|  [05]   | `Platform.IsMac`             | property        | macOS backend predicate                                   |
|  [06]   | `Platform.IsWinForms`        | property        | WinForms backend predicate                                |
|  [07]   | `Platform.IsWpf`             | property        | WPF backend predicate                                     |
|  [08]   | `Platform.IsGtk`             | property        | GTK backend predicate                                     |
|  [09]   | `Platform.IsDesktop`         | property        | desktop-class backend predicate                           |
|  [10]   | `Platform.SupportedFeatures` | property        | the feature set the backend advertises                    |

[PUBLIC_TYPE_SCOPE]: native-control hosting
- namespace: `Eto.Forms`
- rail: platform-handler

| [INDEX] | [SYMBOL]                  | [KIND]     | [CAPABILITY]                                         |
| :-----: | :------------------------ | :--------- | :--------------------------------------------------- |
|  [01]   | `NativeControlHost`       | host       | embeds a platform-native control as an Eto control   |
|  [02]   | `CreateNativeControlArgs` | event args | native-control creation callback payload             |
|  [03]   | `Control.AttachNative()`  | member     | attaches the control under an external native parent |
|  [04]   | `Control.DetachNative()`  | member     | detaches from the native parent                      |

[PUBLIC_TYPE_SCOPE]: style and theme-transition seam
- namespace: `Eto`, `Eto.Forms`, `Rhino.UI`
- rail: platform-handler

| [INDEX] | [SYMBOL]                                      | [KIND]   | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Style`                                       | registry | named handler-level style callback registry   |
|  [02]   | `Widget.Style`                                | property | the style key applied to a widget             |
|  [03]   | `Control.TriggerStyleChanged()`               | member   | re-applies style handlers on a theme change   |
|  [04]   | `Rhino.UI.EtoExtensions.Get(Control control)` | seam     | Rhino style/theme notifier for an Eto control |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: feature-gated platform access
- namespace: `Eto`
- rail: platform-handler

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]             | [CAPABILITY]                                     |
| :-----: | :----------------------------------------- | :----------------------- | :----------------------------------------------- |
|  [01]   | `Platform.Supports<T>()`                   | instance → `bool`        | tests whether the backend implements a handler   |
|  [02]   | `Platform.Create<T>()`                     | instance → `T`           | constructs a per-instance handler                |
|  [03]   | `Platform.CreateShared<T>()`               | instance → `T`           | constructs or reuses a shared handler            |
|  [04]   | `Platform.Find<T>()`                       | instance → `Func<T>`     | resolves the factory delegate for a handler type |
|  [05]   | `Platform.Initialize(Platform platform)`   | static                   | sets the ambient platform for the current thread |
|  [06]   | `Platform.Initialize(string platformType)` | static                   | sets the ambient platform by type name           |
|  [07]   | `Platform.ThreadStart()`                   | instance → `IDisposable` | scopes the ambient platform to a worker thread   |
|  [08]   | `Platform.Invoke(Action action)`           | instance                 | runs an action under the platform context        |
|  [09]   | `Platform.Invoke<T>(Func<T> action)`       | instance → `T`           | runs a value projection under platform context   |

[ENTRYPOINT_SCOPE]: native-control lifecycle
- namespace: `Eto.Forms`
- rail: platform-handler

| [INDEX] | [SURFACE]                                                            | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------- | :------------------------------ |
|  [01]   | `NativeControlHost(object nativeControl)`                            | wraps a native control          |
|  [02]   | `NativeControlHost.OnCreateNativeControl(CreateNativeControlArgs e)` | lazy native-control supply      |
|  [03]   | `Control.AttachNative()`                                             | attaches to a native parent     |
|  [04]   | `Control.DetachNative()`                                             | detaches from the native parent |

[ENTRYPOINT_SCOPE]: theme-transition notification
- namespace: `Eto.Forms`, `Rhino.UI`
- rail: platform-handler

| [INDEX] | [SURFACE]                                                               | [CALL_SHAPE] | [CAPABILITY]                                 |
| :-----: | :---------------------------------------------------------------------- | :----------- | :------------------------------------------- |
|  [01]   | `Style.Add<TWidget>(string style, StyleWidgetHandler<TWidget> handler)` | registration | registers a named widget-style callback      |
|  [02]   | `Rhino.UI.EtoExtensions.Get(Control control)`                           | seam         | resolves a control's Rhino theme notifier    |
|  [03]   | `Control.TriggerStyleChanged()`                                         | member       | re-applies style handlers on host theme flip |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_TOPOLOGY]:
- Every `Widget` resolves its backend handler through the ambient `Platform`; identity predicates (`IsMac`/`IsWinForms`/`IsWpf`/`IsGtk`) select platform-specific behaviour, and `Supports<T>` gates a capability before `Create<T>` builds its handler, so a missing feature is a discovery result, never a construction failure.
- `NativeControlHost` bridges a host-native control into the Eto tree eagerly through its constructor or lazily through `OnCreateNativeControl`; `AttachNative`/`DetachNative` move an Eto control under an external native parent, the seam a Rhino-hosted panel uses to dock Eto content.
- `Style` registers named handler-level callbacks, `Widget.Style` selects one, and `TriggerStyleChanged` re-applies them; the Rhino `EtoExtensions.Get` notifier fires on host light/dark transitions so a control re-styles without polling.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): a `[SmartEnum]` owns the closed platform-identity and advertised-feature vocabulary, collapsing an `IsMac`/`IsWpf` predicate ladder into a generated `Switch`.
- `LanguageExt.Core`(`../../.api/api-languageext.md`): `Option<A>` carries the `Supports<T>`/`Find<T>` result where a feature or handler may be absent; `Eff<A>` wraps the `AttachNative`/`DetachNative` lifecycle for deterministic release; the theme notifier feeds a `Fin<A>`-railed re-style.
- `api-rhino-ui.md`: `EtoExtensions.Get` and the Rhino native windowing surface (`RhinoEtoApp`, `Panels`, `UseRhinoStyle`) dock and style the Eto content this handler places.
- `api-macos-native.md`: on the macOS backend a `NativeControlHost` bridges to an AppKit `NSView`, where `AppKit`/`CoreAnimation` native pacing composes rather than in this seam.

[LOCAL_ADMISSION]:
- Rasm.Rhino binds `Platform.Instance` — the platform the Rhino process already resolved for its `Eto.dll` — and never calls `Initialize` against the host thread; a worker thread touching Eto scopes the platform through `ThreadStart`.
- Feature discovery, native hosting, and the style seam stay behind the Rasm.Rhino UI owner; `Eto.*` platform types never leak past it.

[RAIL_LAW]:
- Package: `Eto` (platform handler surface)
- Owns: platform identity and feature discovery, the `NativeControlHost`/`AttachNative`/`DetachNative` native-hosting lifecycle, and the `Style`/`TriggerStyleChanged` and Rhino `EtoExtensions.Get` theme-transition seam.
- Accept: backend feature gating, native-control embedding, worker-thread platform scoping, host theme-change notification.
- Reject: widget construction and layout (`api-eto-forms.md`), immediate 2D painting (`api-eto-drawing.md`), Rhino document windowing and panel registration (`api-rhino-ui.md`), macOS native pacing (`api-macos-native.md`), and re-initializing the ambient platform the host owns.
