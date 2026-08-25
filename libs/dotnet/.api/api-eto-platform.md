# [RASM_API_ETO_PLATFORM]

`Eto.Platform` is the ambient handler-factory root beneath every `Eto.Forms` control and `Eto.Drawing` object: one platform mints, caches, and resolves each widget's backend handler, `HandlerAttribute` binds the widget type to its handler interface, and `IControlObjectSource.ControlObject` exposes the handler-created native control without a second field. `Style` restyles by name at attach time and `NativeControlHost` admits a raw platform object into the managed tree. This branch catalogue owns the handler root every consuming folder crosses; each host-boundary folder registers it and tables only the platform-specific handler set or host seam its own boundary reaches.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: platform identity, capability, and mint payloads

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :--------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `Platform`                   | abstract      | active-platform root and handler factory     |
|  [02]   | `Platforms`                  | static        | assembly-qualified platform type identifiers |
|  [03]   | `PlatformFeatures`           | flags enum    | per-platform capability flags                |
|  [04]   | `HandlerAttribute`           | attribute     | binds a widget type to its handler interface |
|  [05]   | `PlatformExtensionAttribute` | attribute     | registers a platform extension assembly      |
|  [06]   | `HandlerCreatedEventArgs`    | class         | handler-mint raise payload                   |
|  [07]   | `WidgetCreatedEventArgs`     | class         | widget-mint raise payload                    |

[PLATFORM_FEATURES]: `None` `CustomCellSupportsControlView` `DrawableWithTransparentContent` `TabIndexWithCustomContainers` `MultiThreadedUI` `Mnemonics`

[PUBLIC_TYPE_SCOPE]: handler families and styling

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `WidgetHandler<TWidget>`                             | class         | base handler over a widget                     |
|  [02]   | `WidgetHandler<TControl,TWidget>`                    | class         | binds a native control to a widget             |
|  [03]   | `WidgetHandler<TControl,TWidget,TCallback>`          | class         | adds a callback channel to the widget          |
|  [04]   | `ThemedControlHandler<TControl,TWidget,TCallback>`   | class         | control drawn from managed widgets, not native |
|  [05]   | `ThemedContainerHandler<TControl,TWidget,TCallback>` | class         | themed container variant                       |
|  [06]   | `Style`                                              | static        | style registry keyed by handler type and name  |
|  [07]   | `StyleWidgetHandler<TWidget>`                        | delegate      | style applied against a widget facade          |
|  [08]   | `StyleHandler<THandler>`                             | delegate      | style applied against a concrete handler       |
|  [09]   | `IStyleProvider`                                     | interface     | style registry contract a provider swap seats  |

[PUBLIC_TYPE_SCOPE]: native-control hosting

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :--------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `IControlObjectSource`       | interface     | exposes the handler-created concrete control        |
|  [02]   | `NativeControlHost`          | class         | hosts an admitted native object in the managed tree |
|  [03]   | `CreateNativeControlArgs`    | class         | nullable native-object carrier for subclassing      |
|  [04]   | `NativeControlHost.IHandler` | interface     | creates the native host from the supplied object    |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Platform` — identity, capability probe, and the platform-row assertions

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Platform.Instance`               | static   | active platform, runtime-nullable  |
|  [02]   | `Platform.Detect`                 | static   | active platform or throw           |
|  [03]   | `Platform.ID`                     | property | platform identity string           |
|  [04]   | `Platform.IsMac`                  | property | macOS-platform assertion           |
|  [05]   | `Platform.IsWinForms`             | property | WinForms-platform assertion        |
|  [06]   | `Platform.IsWpf`                  | property | WPF-platform assertion             |
|  [07]   | `Platform.IsGtk`                  | property | GTK-platform assertion             |
|  [08]   | `Platform.IsIos`                  | property | iOS-platform assertion             |
|  [09]   | `Platform.IsAndroid`              | property | Android-platform assertion         |
|  [10]   | `Platform.IsDesktop`              | property | desktop-form-factor assertion      |
|  [11]   | `Platform.IsMobile`               | property | mobile-form-factor assertion       |
|  [12]   | `Platform.IsValid`                | property | app-bundle validity                |
|  [13]   | `Platform.SupportedFeatures`      | property | admitted capability flags          |
|  [14]   | `Platform.Supports<T>() -> bool`  | instance | capability probe by type parameter |
|  [15]   | `Platform.Supports(Type) -> bool` | instance | capability probe by type           |

- Every `Is*` row is `virtual` and answers `false` on the base, so a platform row's probe reads the concrete override.
- `Supports<T>` gates a capability before `Create<T>` builds its handler, so a missing feature is a discovery result and never a construction failure.

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
- `Cache<TKey,TValue>` hands back a platform-lifetime dictionary keyed by an opaque cache key; it is the same shared-property store `CreateShared` uses, and it is the platform-scoped typed cache a hand-rolled spec-to-resource registry replaces.
- `HandlerCreated` and `WidgetCreated` are the only mint observation points, so a census of what the platform produced subscribes both rather than instrumenting construction sites.
- `Widget` is `IDisposable` over a `public void Dispose()` delegating to a `protected virtual void Dispose(bool disposing)` that disposes the handler once and latches `IsDisposed`; a widget subclass owning host children overrides the protected arity, and the latch is what makes a second pass a no-op rather than a double release.

[ENTRYPOINT_SCOPE]: `Platform` — boot, context, and marshal (composition-root surfaces)

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :-------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Platform.Initialize(Platform)`         | static   | seat the global platform            |
|  [02]   | `Platform.Initialize(string)`           | static   | seat by loader type string          |
|  [03]   | `Platform.AllowReinitialize`            | static   | settable re-seat admission          |
|  [04]   | `Platform.Get(string) -> Platform`      | static   | resolve a platform by loader string |
|  [05]   | `Platform.Copy(Platform?) -> Platform`  | static   | clone with the instantiator map     |
|  [06]   | `Platform.LoadAssembly(string)`         | instance | load a platform extension by name   |
|  [07]   | `Platform.LoadAssembly(Assembly)`       | instance | load a platform extension assembly  |
|  [08]   | `Platform.Context -> IDisposable?`      | property | ambient context, runtime-nullable   |
|  [09]   | `Platform.ThreadStart() -> IDisposable` | instance | UI-thread scope, base returns null  |
|  [10]   | `Platform.Invoke(Action)`               | instance | run inside this platform's context  |
|  [11]   | `Platform.Invoke<T>(Func<T>) -> T`      | instance | run and return inside the context   |

- `Context` returns `null` when this platform is already the ambient instance and a fresh `PlatformContext` push otherwise, so a `using` over it is a no-op on the ambient path rather than a second frame; a null-guard at the call site restates what the language already tolerates.
- `Invoke` folds `Context` and the call into one member, so a foreign-platform body never spells the `using` itself; `ThreadStart` returns `null` on the base and a real scope only where a platform overrides it.
- `Copy` clones the instantiator map onto a fresh instance of the same platform type and throws when neither a global nor an argument platform exists; `Get` resolves a loader string and `Initialize(string)` is `Initialize(Get(...))`.

[ENTRYPOINT_SCOPE]: `Style` — scoped appearance and behaviour

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :----------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Style.Provider -> IStyleProvider`                           | static   | active provider, get and set       |
|  [02]   | `Style.StyleWidget`                                          | static   | per-widget style event             |
|  [03]   | `Style.Add<TWidget>(string?, StyleWidgetHandler<TWidget>)`   | static   | register a widget-facade style     |
|  [04]   | `Style.Add<THandler>(string?, StyleHandler<THandler>)`       | static   | register a concrete-handler style  |
|  [05]   | `IStyleProvider.Inherit -> bool`                             | instance | cascading application declared     |
|  [06]   | `IStyleProvider.ApplyStyle(object, string)`                  | instance | apply one named style to a widget  |
|  [07]   | `IStyleProvider.ApplyCascadingStyle(object, object, string)` | instance | apply a container's style downward |
|  [08]   | `IStyleProvider.ApplyDefault(object)`                        | instance | apply the unnamed default style    |

- `Style.Provider` is settable and defaults to the lazily created `DefaultStyleProvider` the first `Add` mints, so a provider swap replaces the whole registry rather than any per-key row and is never a per-registration act.
- `Style.Add` APPENDS into the active provider's per-key `IList<Action<object>>`; the only removal is the provider's whole-registry `Clear()`, so a second `Add` under a live key stacks a handler beside the first and retires nothing — a detachable registration owns its own dispatch cell and empties it, never re-`Add`s.

[ENTRYPOINT_SCOPE]: `NativeControlHost` — hosting a raw platform view

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `NativeControlHost(object?)`                                       | ctor     | host an admitted native object   |
|  [02]   | `NativeControlHost()`                                              | ctor     | empty host for subclass creation |
|  [03]   | `NativeControlHost.OnCreateNativeControl(CreateNativeControlArgs)` | instance | subclass native-creation hook    |
|  [04]   | `CreateNativeControlArgs.NativeControl`                            | property | native-object carrier, nullable  |
|  [05]   | `NativeControlHost.IHandler.Create(object?)`                       | instance | build the native host            |
|  [06]   | `IControlObjectSource.ControlObject -> object?`                    | property | handler-created control          |

- `WidgetHandler<TControl,TWidget>` implements `IControlObjectSource` explicitly and lazily returns `Control`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every `Eto.Forms` and `Eto.Drawing` widget delegates to a `WidgetHandler` the active `Platform` mints; `HandlerAttribute` binds the widget type to its handler interface, and `IControlObjectSource.ControlObject` exposes the handler-created control without a second native field.
- Handler resolution carries two modalities that never substitute: `Create` is the per-call mint and `CreateShared` the platform-cached singleton per contract, both resolving through one instantiator map that `Add` populates and clears.
- `Style` restyles a control by name: a registered `StyleHandler<THandler>` or `StyleWidgetHandler<TWidget>` runs at attach time against the widget or its concrete handler, so a surface never subclasses to change appearance.
- `NativeControlHost` bridges a host-native control into the managed tree eagerly through its constructor or lazily through `OnCreateNativeControl`.
- Boot, context, and marshal are composition-root surfaces a plugin shell spends once; a boundary that binds an already-resolved `Platform.Instance` never calls `Initialize` against the host thread.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): the identity vocabulary — `Platforms` ids and `PlatformFeatures` flags — maps at each boundary onto `[SmartEnum]` and flag owners, so a platform-gate decision is exhaustive dispatch rather than a stringly-keyed `ID` comparison collapsing an `IsMac`/`IsWpf` predicate ladder.
- `LanguageExt.Core`(`.api/api-languageext.md`): runtime-nullable `Platform.Instance`, `Find`, and `CreateNativeControlArgs.NativeControl` lower onto `Option<T>`/`Fin<T>` at each boundary; `Platform.Create<T>()` and an invalid native-host payload enter `Op.Catch` before `Eff<A>` scopes the already-railed native attach and detach lifecycle for deterministic release.
- `api-eto-forms`(`.api/api-eto-forms.md`) and `api-eto-drawing`(`.api/api-eto-drawing.md`): this root resolves the control object each consumes, and the `Themed*Handler` backend classes register through `Platform.Add<TWidget.IHandler>` at this seam rather than as widget-construction rows.

[LOCAL_ADMISSION]:
- Every widget composes its handler through `Platform.Create`, `CreateShared`, or `Find`; a page never re-mints a `WidgetHandler` the active `Platform` already owns.
- Appearance changes ride a `Style` delegate registered by name, never a control subclass.
- A mint census subscribes `HandlerCreated`/`WidgetCreated` rather than instrumenting each construction site, and a platform-scoped typed cache takes `Platform.Cache<TKey,TValue>`.
