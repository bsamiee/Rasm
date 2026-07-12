# [RASM_APPUI_API_REACTIVEUI_AVALONIA]

`ReactiveUI.Avalonia` is the Avalonia platform binding for ReactiveUI: it supplies typed view bases, the `WithAvalonia` builder admission that registers Avalonia's activation/command/property fetchers into the ReactiveUI/Splat resolver, routed and direct view hosts, the Avalonia main-thread scheduler, and lifetime suspension. It owns no view-model abstractions of its own — those live in `ReactiveUI` (`IViewFor<T>`, `ReactiveObject`, `RoutingState`, `ReactiveCommand`); this assembly wires them to Avalonia controls.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI.Avalonia`
- package: `ReactiveUI.Avalonia`
- license: `MIT`
- assembly: `ReactiveUI.Avalonia`
- build-floor: `net10.0` (consumer-bound; multi-targets net8.0/net9.0 — none bound here)
- namespace: `ReactiveUI.Avalonia` (13 types, 1 namespace)
- depends-on: `ReactiveUI` (view-model/command/routing core), `Avalonia` (control + `AppBuilder` surface), `Splat`/`Splat.Builder` (DI resolver the fetchers register into)
- asset: runtime library
- rail: reactive-ui

## [02]-[PUBLIC_TYPES]

[VIEW_TYPES]: typed view bases and view hosts — consumer-facing controls — rail: reactive-ui. `AvaloniaScheduler` and `AutoSuspendHelper` are sealed.

| [INDEX] | [SYMBOL]                          | [BASE]                                            | [ROLE]                          |
| :-----: | :-------------------------------- | :------------------------------------------------ | :------------------------------ |
|  [01]   | `ReactiveUserControl<TViewModel>` | `UserControl`, `IViewFor<TViewModel>`             | typed screen control            |
|  [02]   | `ReactiveWindow<TViewModel>`      | `Window`, `IViewFor<TViewModel>`                  | typed top-level window          |
|  [03]   | `RoutedViewHost`                  | `TransitioningContentControl`, `IActivatableView` | current-route view resolution   |
|  [04]   | `ViewModelViewHost`               | `TransitioningContentControl`, `IViewFor`         | bound view-model resolution     |
|  [05]   | `AvaloniaScheduler`               | `LocalScheduler`                                  | Avalonia UI scheduling          |
|  [06]   | `AutoSuspendHelper`               | `IDisposable`                                     | application-lifetime suspension |

[INFRASTRUCTURE_TYPES]: builder admission + binders. The four fetchers are registered by `WithAvalonia`; only the noted ones are public — the command and property fetchers are `internal` and never constructed by consumers — rail: reactive-ui

| [INDEX] | [SYMBOL]                              | [VISIBILITY] | [CONTRACT]                      | [ROLE]                    |
| :-----: | :------------------------------------ | :----------- | :------------------------------ | :------------------------ |
|  [01]   | `AppBuilderExtensions`                | public       | static                          | builder admission         |
|  [02]   | `AvaloniaActivationForViewFetcher`    | public       | `IActivationForViewFetcher`     | loaded-view activation    |
|  [03]   | `AvaloniaObjectReactiveExtensions`    | public       | static                          | two-way property subjects |
|  [04]   | `AvaloniaScheduler`                   | public       | `LocalScheduler`                | UI-thread dispatch        |
|  [05]   | `AutoDataTemplateBindingHook`         | public       | `IPropertyBindingHook`          | default VM data template  |
|  [06]   | `AvaloniaCreatesCommandBinding`       | internal     | `ICreatesCommandBinding`        | routed-event command bind |
|  [07]   | `AvaloniaObjectObservableForProperty` | internal     | `ICreatesObservableForProperty` | property-change stream    |

## [03]-[ENTRYPOINTS]

[BUILDER_ENTRYPOINTS]: admission during `AppBuilder` startup. `UseReactiveUI` calls `IReactiveUIBuilder.WithAvalonia()`, which registers the fetchers — rail: reactive-ui

Every builder entrypoint is rooted at `AppBuilderExtensions`.

| [INDEX] | [SURFACE]                                        | [ADMISSION]           |
| :-----: | :----------------------------------------------- | :-------------------- |
|  [01]   | `UseReactiveUI`                                  | services and views    |
|  [02]   | `WithAvalonia`                                   | Avalonia fetchers     |
|  [03]   | `RegisterReactiveUIViews`                        | explicit assemblies   |
|  [04]   | `RegisterReactiveUIViewsFromAssemblyOf<TMarker>` | marker assembly       |
|  [05]   | `RegisterReactiveUIViewsFromEntryAssembly`       | entry assembly        |
|  [06]   | `UseReactiveUIWithDIContainer<TContainer>`       | custom Splat resolver |

[BUILDER_SIGNATURES]:
- Use: `UseReactiveUI(this AppBuilder, Action<ReactiveUIBuilder> withReactiveUIBuilder) : AppBuilder`
- Register: `WithAvalonia(this IReactiveUIBuilder) : IReactiveUIBuilder`
- Explicit assemblies: `RegisterReactiveUIViews(this AppBuilder, params Assembly[]) : AppBuilder`
- Marker assembly: `RegisterReactiveUIViewsFromAssemblyOf<TMarker>(this AppBuilder) : AppBuilder`
- Entry assembly: `RegisterReactiveUIViewsFromEntryAssembly(this AppBuilder) : AppBuilder`
- Custom container: `UseReactiveUIWithDIContainer<TContainer>(this AppBuilder, Func<TContainer> containerFactory, Action<TContainer> containerConfig, Func<TContainer, IDependencyResolver> dependencyResolverFactory, Action<ReactiveUIBuilder> configureReactiveUI) : AppBuilder`

[VIEW_HOST_ENTRYPOINTS]: host/view-base members and the UI scheduler — rail: reactive-ui. Each `ViewModel`, `Router`, `ViewContract`, and `DefaultContent` member is backed by its same-named `Property` field.

| [INDEX] | [SURFACE]                     | [SURFACE_ROOT]                    | [ROLE]                     |
| :-----: | :---------------------------- | :-------------------------------- | :------------------------- |
|  [01]   | `ViewModel : TViewModel?`     | `ReactiveUserControl<TViewModel>` | two-way typed slot         |
|  [02]   | `ViewModel : TViewModel?`     | `ReactiveWindow<TViewModel>`      | typed slot                 |
|  [03]   | `Router : RoutingState?`      | `RoutedViewHost`                  | navigation stack           |
|  [04]   | `ViewContract : string?`      | `RoutedViewHost`                  | view discriminator         |
|  [05]   | `DefaultContent : object?`    | `RoutedViewHost`                  | empty-router fallback      |
|  [06]   | `ViewLocator : IViewLocator?` | `RoutedViewHost`                  | resolver override          |
|  [07]   | `ViewModel : object?`         | `ViewModelViewHost`               | direct view-model          |
|  [08]   | `ViewContract : string?`      | `ViewModelViewHost`               | view discriminator         |
|  [09]   | `DefaultContent : object?`    | `ViewModelViewHost`               | fallback content           |
|  [10]   | `ViewLocator : IViewLocator?` | `ViewModelViewHost`               | resolver override          |
|  [11]   | `Instance`                    | `AvaloniaScheduler`               | main-thread scheduler      |
|  [12]   | `Schedule<TState>`            | `AvaloniaScheduler`               | delayed UI-thread dispatch |

[SCHEDULER_SIGNATURE]: `Schedule<TState>(TState, TimeSpan, Func<IScheduler,TState,IDisposable>)`

[LIFETIME_BINDING_ENTRYPOINTS]: suspension, activation, and property-subject bridge — rail: reactive-ui. The property bridges default `BindingPriority` to `LocalValue`.

| [INDEX] | [SURFACE]                            | [SURFACE_ROOT]                     | [ROLE]                  |
| :-----: | :----------------------------------- | :--------------------------------- | :---------------------- |
|  [01]   | `AutoSuspendHelper`                  | `AutoSuspendHelper`                | lifetime binding        |
|  [02]   | `OnFrameworkInitializationCompleted` | `AutoSuspendHelper`                | suspension admission    |
|  [03]   | `GetActivationForView`               | `AvaloniaActivationForViewFetcher` | loaded-state stream     |
|  [04]   | `GetAffinityForView`                 | `AvaloniaActivationForViewFetcher` | view affinity           |
|  [05]   | `GetSubject`                         | `AvaloniaObjectReactiveExtensions` | object property subject |
|  [06]   | `GetSubject<T>`                      | `AvaloniaObjectReactiveExtensions` | typed property subject  |
|  [07]   | `GetBindingSubject`                  | `AvaloniaObjectReactiveExtensions` | object binding state    |
|  [08]   | `GetBindingSubject<T>`               | `AvaloniaObjectReactiveExtensions` | typed binding state     |

[LIFETIME_BINDING_SIGNATURES]:
- Lifetime: `AutoSuspendHelper(IApplicationLifetime lifetime)` / `OnFrameworkInitializationCompleted()`
- Activation: `GetActivationForView(IActivatableView) : IObservable<bool>` / `GetAffinityForView(Type) : int`
- Object property: `GetSubject(this AvaloniaObject, AvaloniaProperty, BindingPriority = LocalValue) : ISubject<object?>`
- Typed property: `GetSubject<T>(this AvaloniaObject, AvaloniaProperty<T>, BindingPriority = LocalValue) : ISubject<T>`
- Object binding: `GetBindingSubject(this AvaloniaObject, AvaloniaProperty, BindingPriority = LocalValue) : ISubject<BindingValue<object?>>`
- Typed binding: `GetBindingSubject<T>(this AvaloniaObject, AvaloniaProperty<T>, BindingPriority = LocalValue) : ISubject<BindingValue<T>>`

## [04]-[IMPLEMENTATION_LAW]

[ADMISSION_TOPOLOGY]:
- `UseReactiveUI(builder, b => ...)` defers to `AfterPlatformServicesSetup`: it creates a `ReactiveUIBuilder` via `RxAppBuilder.CreateReactiveUIBuilder()`, calls `WithAvalonia()` on it, runs the consumer callback, then `BuildApp()` unless the app is already built. `WithAvalonia()` composes the builder chain `WithMainThreadScheduler(AvaloniaScheduler.Instance) -> WithTaskPoolScheduler(TaskPoolScheduler.Default) -> WithRegistration(splat => { RegisterConstant<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher()); RegisterConstant<IPropertyBindingHook>(new AutoDataTemplateBindingHook()); RegisterConstant<ICreatesCommandBinding>(new AvaloniaCreatesCommandBinding()); RegisterConstant<ICreatesObservableForProperty>(new AvaloniaObjectObservableForProperty()); }) -> WithSuspensionHost<Unit>()` — so the UI scheduler is installed through `WithMainThreadScheduler` (setting `RxApp.MainThreadScheduler`), not a bare field assignment. `RegisterReactiveUIViews*` add `IViewFor<T>` registrations to `AppLocator.CurrentMutable`; `UseReactiveUIWithDIContainer<TContainer>` is the only path that assigns `RxSchedulers.MainThreadScheduler = AvaloniaScheduler.Instance` directly, after swapping the Splat locator to the supplied container's `IDependencyResolver`.
- `AvaloniaCreatesCommandBinding` and `AvaloniaObjectObservableForProperty` are `internal`: they are resolver-registered services, not types a consumer references. Consume their behavior through ReactiveUI's `BindCommand`/`WhenAnyValue`, never by constructing them.

[STACKING]:
- View bases stack with `ReactiveUI`: a `ReactiveUserControl<TVm>` / `ReactiveWindow<TVm>` pairs with a `ReactiveObject` (or `ReactiveValidationObject` from `ReactiveUI.Validation`) view-model, and the code-behind body is `this.WhenActivated(d => { this.Bind(...).DisposeWith(d); this.BindCommand(...).DisposeWith(d); })` — the `WhenActivated` block is fed by `AvaloniaActivationForViewFetcher` and every disposable is tied to the view's `Loaded`/`Unloaded` lifetime.
- `RoutedViewHost` stacks with `ReactiveUI` routing: bind `Router` to a view-model `RoutingState`; the host resolves `CurrentViewModel` through the registered (or per-host `ViewLocator`) resolver and transitions content. `ViewModelViewHost` is the non-stack variant for a single bound view-model. Both honor `ViewContract` to disambiguate multiple views of one view-model.
- The property-subject bridge stacks with `DynamicData` and `System.Reactive`: `GetSubject<T>` exposes any `AvaloniaProperty` as an `ISubject<T>`, so an Avalonia control property serves as a source/sink in a DynamicData change-set pipeline or a `ReactiveCommand` `canExecute` stream, with `BindingPriority` controlling precedence against styles/animations.
- Suspension stacks with Avalonia lifetimes: construct `AutoSuspendHelper(ApplicationLifetime)` in `App.OnFrameworkInitializationCompleted`, call its `OnFrameworkInitializationCompleted()`, and back `RxApp.SuspensionHost` with a driver so view-model state persists across launches.
- Docking stacks via `Dock.Model.ReactiveUI`: dock documents/tools are ReactiveUI view-models hosted through the same `IViewFor<T>` + `ViewModelViewHost` resolution path this assembly registers — one view-resolution contract spans the shell's dock layout, routed screens, and direct hosts.

[ACTIVATION_LAW]:
- Package: `ReactiveUI.Avalonia`
- Owns: Avalonia builder admission, activation, command binding, property observation, routing, and UI scheduling
- Accept: bindings terminate at activated Avalonia views and typed view hosts; every subscription disposes with the activation scope
- Reject: undisposed subscriptions; binding outside `WhenActivated`

[HOST_LAW]:
- Package: `ReactiveUI.Avalonia`
- Owns: one reactive binding + view-resolution rail for panels, companion windows, sidecars, diagnostics, dock layouts, and downstream shells
- Accept: routed screens, dock documents, and direct view-model hosts share one `IViewFor<T>` resolution contract registered once at startup
- Reject: per-host reactive binding stacks; per-host view registration
