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

[VIEW_TYPES]: typed view bases and view hosts — consumer-facing controls — rail: reactive-ui

| [INDEX] | [SYMBOL]                                         | [BASE]                                          | [ROLE]                                                     |
| :-----: | :----------------------------------------------- | :--------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `ReactiveUserControl<TViewModel>`                | `UserControl`, `IViewFor<TViewModel>`          | screen/control base with strongly-typed `ViewModel`        |
|  [02]   | `ReactiveWindow<TViewModel>`                     | `Window`, `IViewFor<TViewModel>`               | top-level window base with strongly-typed `ViewModel`      |
|  [03]   | `RoutedViewHost`                                 | `TransitioningContentControl`, `IActivatableView` | resolves the current `RoutingState.CurrentViewModel` to a view |
|  [04]   | `ViewModelViewHost`                              | `TransitioningContentControl`, `IViewFor`      | resolves a single bound `ViewModel` to a view              |
|  [05]   | `AvaloniaScheduler` (sealed `: LocalScheduler`)  | `LocalScheduler`                               | Rx scheduler that marshals onto the Avalonia UI thread     |
|  [06]   | `AutoSuspendHelper` (sealed `: IDisposable`)     | —                                              | drives ReactiveUI suspension from `IApplicationLifetime`   |

[INFRASTRUCTURE_TYPES]: builder admission + binders. The four fetchers are registered by `WithAvalonia`; only the noted ones are public — the command and property fetchers are `internal` and never constructed by consumers — rail: reactive-ui

| [INDEX] | [SYMBOL]                                            | [VISIBILITY] | [CONTRACT]                                  | [ROLE]                                              |
| :-----: | :-------------------------------------------------- | :----------- | :------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `AppBuilderExtensions`                              | public       | static                                      | `AppBuilder`/`IReactiveUIBuilder` admission methods |
|  [02]   | `AvaloniaActivationForViewFetcher`                  | public       | `IActivationForViewFetcher`                 | view activation from `Loaded`/`Unloaded` + visual tree |
|  [03]   | `AvaloniaObjectReactiveExtensions`                  | public       | static                                      | `AvaloniaProperty` <-> `ISubject` two-way bridge    |
|  [04]   | `AvaloniaScheduler`                                 | public       | `LocalScheduler`                            | UI-thread `Schedule`                                |
|  [05]   | `AutoDataTemplateBindingHook`                       | public       | `IPropertyBindingHook`                      | supplies a default `FuncDataTemplate` for VM binding |
|  [06]   | `AvaloniaCreatesCommandBinding`                     | internal     | `ICreatesCommandBinding`                    | registered command binder (routed-event + `ICommand`) |
|  [07]   | `AvaloniaObjectObservableForProperty`               | internal     | `ICreatesObservableForProperty`             | registered `AvaloniaProperty` change-stream provider |

## [03]-[ENTRYPOINTS]

[BUILDER_ENTRYPOINTS]: admission during `AppBuilder` startup. `UseReactiveUI` internally calls `IReactiveUIBuilder.WithAvalonia()`, which is what actually registers the fetchers — rail: reactive-ui

| [INDEX] | [SURFACE]                                                                                              | [SURFACE_ROOT]         | [NOTE]                                                          |
| :-----: | :---------------------------------------------------------------------------------------------------- | :--------------------- | :------------------------------------------------------------- |
|  [01]   | `UseReactiveUI(this AppBuilder, Action<ReactiveUIBuilder> withReactiveUIBuilder) : AppBuilder`        | `AppBuilderExtensions` | primary admission; the callback registers app services/views   |
|  [02]   | `WithAvalonia(this IReactiveUIBuilder) : IReactiveUIBuilder`                                           | `AppBuilderExtensions` | registers the four Avalonia fetchers/binders into the resolver |
|  [03]   | `RegisterReactiveUIViews(this AppBuilder, params Assembly[]) : AppBuilder`                             | `AppBuilderExtensions` | explicit-assembly `IViewFor<T>` registration                   |
|  [04]   | `RegisterReactiveUIViewsFromAssemblyOf<TMarker>(this AppBuilder) : AppBuilder`                         | `AppBuilderExtensions` | scan the marker type's assembly                                |
|  [05]   | `RegisterReactiveUIViewsFromEntryAssembly(this AppBuilder) : AppBuilder`                               | `AppBuilderExtensions` | scan `Assembly.GetEntryAssembly()`                             |
|  [06]   | `UseReactiveUIWithDIContainer<TContainer>(this AppBuilder, Func<TContainer> containerFactory, Action<TContainer> containerConfig, Func<TContainer, IDependencyResolver> dependencyResolverFactory, Action<ReactiveUIBuilder> configureReactiveUI) : AppBuilder` | `AppBuilderExtensions` | admit a custom DI container as the Splat resolver              |

[VIEW_HOST_ENTRYPOINTS]: host/view-base members and the UI scheduler — rail: reactive-ui

| [INDEX] | [SURFACE]                                                            | [SURFACE_ROOT]                    | [NOTE]                                                      |
| :-----: | :------------------------------------------------------------------ | :-------------------------------- | :--------------------------------------------------------- |
|  [01]   | `ViewModel : TViewModel?` (+ `ViewModelProperty`)                   | `ReactiveUserControl<TViewModel>` | typed `IViewFor<T>` view-model slot, two-way bindable      |
|  [02]   | `ViewModel : TViewModel?` (+ `ViewModelProperty`)                   | `ReactiveWindow<TViewModel>`      | typed `IViewFor<T>` view-model slot                        |
|  [03]   | `Router : RoutingState?` (+ `RouterProperty`)                       | `RoutedViewHost`                  | the navigation stack whose `CurrentViewModel` is resolved  |
|  [04]   | `ViewContract : string?` (+ `ViewContractProperty`)                 | `RoutedViewHost`                  | contract discriminator passed to the `IViewLocator`        |
|  [05]   | `DefaultContent : object?` (+ `DefaultContentProperty`)             | `RoutedViewHost`                  | fallback content when the router is empty                  |
|  [06]   | `ViewLocator : IViewLocator?`                                       | `RoutedViewHost`                  | per-host view resolver override (else the registered one)  |
|  [07]   | `ViewModel : object?` (+ `ViewModelProperty`)                       | `ViewModelViewHost`               | single view-model to resolve to a view                     |
|  [08]   | `ViewContract : string?` / `DefaultContent : object?` / `ViewLocator : IViewLocator?` | `ViewModelViewHost`               | contract, fallback, and resolver override                  |
|  [09]   | `AvaloniaScheduler.Instance` / `Schedule<TState>(TState, TimeSpan, Func<IScheduler,TState,IDisposable>)` | `AvaloniaScheduler`               | the `RxApp.MainThreadScheduler` for Avalonia               |

[LIFETIME_BINDING_ENTRYPOINTS]: suspension, activation, and property-subject bridge — rail: reactive-ui

| [INDEX] | [SURFACE]                                                                                   | [SURFACE_ROOT]                     | [NOTE]                                                       |
| :-----: | :------------------------------------------------------------------------------------------ | :--------------------------------- | :---------------------------------------------------------- |
|  [01]   | `AutoSuspendHelper(IApplicationLifetime lifetime)` / `OnFrameworkInitializationCompleted()` | `AutoSuspendHelper`                | wire in `App.OnFrameworkInitializationCompleted` for suspension |
|  [02]   | `GetActivationForView(IActivatableView) : IObservable<bool>` / `GetAffinityForView(Type) : int` | `AvaloniaActivationForViewFetcher` | activation stream from `Loaded`/`Unloaded` (the `WhenActivated` source) |
|  [03]   | `GetSubject(this AvaloniaObject, AvaloniaProperty, BindingPriority = LocalValue) : ISubject<object?>` | `AvaloniaObjectReactiveExtensions` | non-generic two-way property subject                        |
|  [04]   | `GetSubject<T>(this AvaloniaObject, AvaloniaProperty<T>, BindingPriority = LocalValue) : ISubject<T>` | `AvaloniaObjectReactiveExtensions` | typed two-way property subject                              |
|  [05]   | `GetBindingSubject(this AvaloniaObject, AvaloniaProperty, BindingPriority = LocalValue) : ISubject<BindingValue<object?>>` | `AvaloniaObjectReactiveExtensions` | non-generic subject carrying `BindingValue<object?>` (unset/error states) |
|  [06]   | `GetBindingSubject<T>(this AvaloniaObject, AvaloniaProperty<T>, BindingPriority = LocalValue) : ISubject<BindingValue<T>>` | `AvaloniaObjectReactiveExtensions` | typed subject carrying `BindingValue<T>` (unset/error states) |

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
