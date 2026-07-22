# [RASM_RHINO_API_ETO_PLATFORM]

`Eto.Platform` is the backend-handler seam beneath every widget — one ambient platform resolves each control, layout, graphics handle, and dialog to a native handler, and this catalog owns the platform identity and feature-discovery surface (`Supports`/`Create`/`Find`), the `NativeControlHost` lifecycle that embeds a host-native control through `AttachNative`/`DetachNative`, and the style-and-theme-transition seam including `TriggerStyleChanged` and the Rhino `EtoExtensions.Get` theme notifier. Feature discovery gates a capability before construction, native hosting bridges a platform control into the Eto tree, and the style seam re-applies handlers when the host flips light/dark.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto` 'platform handler surface'
- package: `Eto` (host-provided; bound in-place from the Rhino-loaded `Eto.dll`, never a second NuGet admission)
- license: BSD-3-Clause
- assembly: `Eto.dll` (Rhino `RhCore` framework); the theme notifier is `Rhino.UI.dll`
- namespace: `Eto`, `Eto.Forms`
- asset: the Rhino process resolves one platform handler for its loaded `Eto.dll`; Rasm.Rhino binds that ambient platform rather than initializing a second one
- rail: platform-handler

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: platform identity and feature discovery
- namespace: `Eto`
- rail: platform-handler

`Platform` is the ambient backend the process resolves; its identity predicates and `Supports<T>` gate select a handler before `Create<T>` builds one.

| [INDEX] | [SYMBOL]                     | [KIND]              | [CAPABILITY]                                              |
| :-----: | :--------------------------- | :------------------ | :-------------------------------------------------------- |
|  [01]   | `Platform`                   | backend             | the ambient handler factory every widget resolves through |
|  [02]   | `Platform.Instance`          | static property     | the current process platform                              |
|  [03]   | `Platform.Detect()`          | static → `Platform` | detects the platform for the host environment             |
|  [04]   | `Platform.ID`                | property            | platform identifier string                                |
|  [05]   | `Platform.IsMac`             | property            | macOS backend predicate                                   |
|  [06]   | `Platform.IsWinForms`        | property            | WinForms backend predicate                                |
|  [07]   | `Platform.IsWpf`             | property            | WPF backend predicate                                     |
|  [08]   | `Platform.IsGtk`             | property            | GTK backend predicate                                     |
|  [09]   | `Platform.IsDesktop`         | property            | desktop-class backend predicate                           |
|  [10]   | `Platform.SupportedFeatures` | property            | the feature set the backend advertises                    |

[PUBLIC_TYPE_SCOPE]: native-control hosting
- namespace: `Eto.Forms`
- rail: platform-handler

`NativeControlHost` embeds a host-native control into the Eto tree; `AttachNative`/`DetachNative` on `Control` move an Eto control under an external native parent.

| [INDEX] | [SYMBOL]                  | [KIND]     | [CAPABILITY]                                         |
| :-----: | :------------------------ | :--------- | :--------------------------------------------------- |
|  [01]   | `NativeControlHost`       | host       | embeds a platform-native control as an Eto control   |
|  [02]   | `CreateNativeControlArgs` | event args | native-control creation callback payload             |
|  [03]   | `Control.AttachNative()`  | member     | attaches the control under an external native parent |
|  [04]   | `Control.DetachNative()`  | member     | detaches from the native parent                      |

[PUBLIC_TYPE_SCOPE]: style and theme-transition seam
- namespace: `Eto`, `Eto.Forms`, `Rhino.UI`
- rail: platform-handler

`Style` registers named handler-level style callbacks; `TriggerStyleChanged` re-applies them, and the Rhino `EtoExtensions.Get` notifier surfaces host theme transitions.

| [INDEX] | [SYMBOL]                                      | [KIND]   | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------------- | :------- | :---------------------------------------------------------------- |
|  [01]   | `Style`                                       | registry | named handler-level style callback registry                       |
|  [02]   | `Widget.Style`                                | property | the style key applied to a widget                                 |
|  [03]   | `Control.TriggerStyleChanged()`               | member   | re-applies style handlers on a theme change                       |
|  [04]   | `Rhino.UI.EtoExtensions.Get(Control control)` | seam     | Rhino style/theme notifier for an Eto control (`api-rhino-ui.md`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: feature-gated platform access
- namespace: `Eto`
- rail: platform-handler

`Supports<T>` gates a handler capability; `Create<T>`/`CreateShared<T>` build the handler; `Find<T>` locates a registered instance. `Initialize` establishes the ambient platform for a thread that has none.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]             | [CAPABILITY]                                     |
| :-----: | :----------------------------------------- | :----------------------- | :----------------------------------------------- |
|  [01]   | `Platform.Supports<T>()`                   | instance → `bool`        | tests whether the backend implements a handler   |
|  [02]   | `Platform.Create<T>()`                     | instance → `T`           | constructs a per-instance handler                |
|  [03]   | `Platform.CreateShared<T>()`               | instance → `T`           | constructs or reuses a shared handler            |
|  [04]   | `Platform.Find<T>()`                       | instance → `T`           | locates a registered handler type                |
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

| [INDEX] | [SURFACE]                                                               | [CALL_SHAPE] | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------------------------- | :----------- | :-------------------------------------------------- |
|  [01]   | `Style.Add<TWidget>(string style, StyleWidgetHandler<TWidget> handler)` | registration | registers a named widget-style callback             |
|  [02]   | `Rhino.UI.EtoExtensions.Get(Control control)`                           | seam         | resolves a control's Rhino theme notifier           |
|  [03]   | `Control.TriggerStyleChanged()`                                         | member       | re-applies style handlers on host theme flip        |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_TOPOLOGY]:
- Every `Widget` resolves a backend handler through the ambient `Platform`; the identity predicates (`IsMac`/`IsWinForms`/`IsWpf`/`IsGtk`) select platform-specific behaviour, and `Supports<T>` gates a capability before `Create<T>` builds its handler, so a missing feature is a discovery result rather than a construction failure.
- `NativeControlHost` bridges a host-native control into the Eto tree either eagerly through the constructor or lazily through `OnCreateNativeControl`; `AttachNative`/`DetachNative` move an Eto control under an external native parent, which is the seam a Rhino-hosted panel uses to place Eto content inside a native dock.
- The style seam is handler-level: `Style` registers named callbacks, `Widget.Style` selects one, and `TriggerStyleChanged` re-applies them; the Rhino `EtoExtensions.Get` notifier fires on host light/dark transitions so a control re-styles without polling the theme.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): a `[SmartEnum]` owns the closed platform-identity and advertised-feature vocabulary a generator-shaped UI layer branches on, replacing an `IsMac`/`IsWpf` predicate ladder with a generated `Switch`.
- `LanguageExt.Core`(`../../.api/api-languageext.md`): `Option<A>` carries the result of `Supports<T>`/`Find<T>` where a feature or handler may be absent; `Eff<A>` wraps the native-attach lifecycle so `AttachNative`/`DetachNative` release deterministically; the theme-transition notifier feeds a `Fin<A>`-railed re-style.
- `api-rhino-ui.md`: `EtoExtensions.Get` and the Rhino native windowing surface (`RhinoEtoApp`, `Panels`, `UseRhinoStyle`) are the host-integration counterpart this seam couples to; the platform handler places Eto content, and the Rhino UI owner docks and styles it.
- `api-macos-native.md`: on the macOS backend a `NativeControlHost` bridges to an AppKit `NSView`, and `AppKit`/`CoreAnimation` native pacing composes there rather than in this seam.

[LOCAL_ADMISSION]:
- The ambient platform is the one the Rhino process already resolved for its loaded `Eto.dll`; Rasm.Rhino binds `Platform.Instance` and never calls `Initialize` against the host thread. A worker thread that must touch Eto scopes the platform through `ThreadStart`.
- Feature discovery and native hosting stay behind the Rasm.Rhino UI owner; `Eto.Platform`, `NativeControlHost`, and the style seam project host capability into the UI owner, and `Eto.*` platform types never leak past it.

[RAIL_LAW]:
- Package: `Eto` (platform handler surface)
- Owns: platform identity and feature discovery (`Supports`/`Create`/`Find`), the `NativeControlHost` and `AttachNative`/`DetachNative` native-hosting lifecycle, and the `Style`/`TriggerStyleChanged` plus Rhino `EtoExtensions.Get` theme-transition seam.
- Accept: backend feature gating, native-control embedding, worker-thread platform scoping, and host theme-change notification.
- Reject: widget construction and layout (`api-eto-forms.md`), immediate 2D painting (`api-eto-drawing.md`), Rhino document windowing and panel registration (`api-rhino-ui.md`), macOS native pacing and Objective-C ownership (`api-macos-native.md`), and re-initializing the ambient platform the host already owns.
