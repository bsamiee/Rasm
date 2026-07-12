# [RASM_APPUI_API_REACTIVEUI]

`ReactiveUI` is the MVVM rail: `ReactiveObject` change-notification, `ReactiveCommand` async/cancellable command execution, `WhenAnyValue` property streams, `ObservableAsPropertyHelper` derived properties, `Interaction` request/response for dialogs, `RoutingState` navigation, `ViewModelActivator`/`WhenActivated` lifecycle, `SuspensionHost` state persistence, and a `Bind`/`OneWayBind`/`BindCommand`/`BindInteraction` view-binding expression compiler. It sits on `System.Reactive` (every member returns/consumes `IObservable<T>`) and surfaces collection deltas as DynamicData `IChangeSet`; the Avalonia bridge (view bases, `AvaloniaScheduler`, builder admission) lives in the sibling `ReactiveUI.Avalonia` catalog. `ReactiveProperty<T>` carries its own core per-property `INotifyDataErrorInfo` (`AddValidationError`/`HasErrors`/`ObserveHasErrors`) independent of any package; the separate view-model-scoped `ValidationContext`/`ValidationRule`/`ReactiveValidationObject` aggregator lives in the sibling `ReactiveUI.Validation` catalog. This catalog documents the advanced surface and how it stacks those rails into one screen owner — the prior version's `RegisterView<…>`/`RegisterStandardConverters`/`WithExceptionHandler` builder methods do not exist in `23.2.28` and are removed.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI`
- package: `ReactiveUI`
- license: `MIT`
- assembly: `ReactiveUI` (AnyCPU IL)
- build-floor: `net10.0` (consumer-bound; multi-targets android/ios/maccatalyst/tizen/wasm — none bound here)
- namespace: `ReactiveUI` (core), `ReactiveUI.Builder` (app builder), `ReactiveUI.Helpers`
- asset: runtime library
- rail: reactive
- depends: `System.Reactive` (observable algebra), `DynamicData` (`IChangeSet`/`SourceList`), `Splat` (`IMutableDependencyResolver` / service location). Property/command binding (`Bind`/`BindCommand`) is platform-coupled and activated through `ReactiveUI.Avalonia`'s `UseReactiveUI`.

## [02]-[PUBLIC_TYPES]

[VIEW_MODEL_TYPES]: reactive state, derived properties, and the validation-aware property — rail: reactive

| [INDEX] | [SYMBOL]                                  | [KIND]                            |
| :-----: | :---------------------------------------- | :-------------------------------- |
|  [01]   | `ReactiveObject`                          | reactive state                    |
|  [02]   | `ReactiveRecord`                          | reactive record                   |
|  [03]   | `IReactiveObject`                         | state contract                    |
|  [04]   | `IReactiveNotifyPropertyChanged<TSender>` | change contract                   |
|  [05]   | `ObservableAsPropertyHelper<T>`           | derived property                  |
|  [06]   | `ReactiveProperty<T>`                     | bindable+self-validating property |
|  [07]   | `IHandleObservableErrors`                 | error stream                      |

[VIEW_MODEL_SIGNATURES]:
- `ReactiveObject`: `class : IReactiveObject` with `Changing`, `Changed`, and `ThrownExceptions`.
- `ReactiveRecord`: record-friendly `ReactiveObject` base.
- `IReactiveObject`: `interface : INotifyPropertyChanged, INotifyPropertyChanging, IReactiveNotifyPropertyChanged<IReactiveObject>, IHandleObservableErrors`.
- `IReactiveNotifyPropertyChanged<TSender>`: `interface` exposing `Changing` and `Changed` as `IReactivePropertyChangedEventArgs` streams.
- `ObservableAsPropertyHelper<T>`: `sealed class : IDisposable` exposing `Value` and `IsSubscribed`.
- `ReactiveProperty<T>`: `class : ReactiveObject, IReactiveProperty<T>, IObservable<T?>, ICancelable, INotifyDataErrorInfo` exposing `Value`, `HasErrors`, `ObserveHasErrors`, `ObserveErrorChanged`, `AddValidationError(Func<IObservable<T?>,IObservable<IEnumerable?>>, bool ignoreInitialError = false)`, and static `Create`.
- `IHandleObservableErrors`: `interface` exposing `ThrownExceptions`.

[COMMAND_AND_INTERACTION_TYPES]: command execution and dialog request rails — rail: reactive

| [INDEX] | [SYMBOL]                                        | [KIND]              |
| :-----: | :---------------------------------------------- | :------------------ |
|  [01]   | `ReactiveCommand`                               | command factory     |
|  [02]   | `ReactiveCommand<TParam,TResult>`               | command execution   |
|  [03]   | `ReactiveCommandBase<TParam,TResult>`           | command base        |
|  [04]   | `CombinedReactiveCommand<TParam,TResult>`       | command batch       |
|  [05]   | `Interaction<TInput,TOutput>`                   | interaction source  |
|  [06]   | `IInteractionContext<TInput,TOutput>`           | interaction context |
|  [07]   | `UnhandledInteractionException<TInput,TOutput>` | interaction error   |

[COMMAND_AND_INTERACTION_SIGNATURES]:
- `ReactiveCommand`: `static class` owning the `Create*` factory family.
- `ReactiveCommand<TParam,TResult>`: `class : ReactiveCommandBase<TParam,TResult>, ICommand` exposing `Execute`.
- `ReactiveCommandBase<TParam,TResult>`: `abstract` base exposing `CanExecute`, `IsExecuting`, and `ThrownExceptions`.
- `CombinedReactiveCommand<TParam,TResult>`: `class` fanning out over child commands.
- `Interaction<TInput,TOutput>`: `class : IInteraction<TInput,TOutput>` with an `IScheduler?` constructor argument.
- `IInteractionContext<TInput,TOutput>`: `interface` exposing `Input`, `IsHandled`, and `SetOutput(TOutput)`.
- `UnhandledInteractionException<TInput,TOutput>`: exception raised when no handler sets output.

[ACTIVATION_AND_ROUTING_TYPES]: lifecycle and navigation contracts — rail: reactive

| [INDEX] | [SYMBOL]                | [KIND]                |
| :-----: | :---------------------- | :-------------------- |
|  [01]   | `IActivatableViewModel` | view-model activation |
|  [02]   | `IActivatableView`      | view activation       |
|  [03]   | `ViewModelActivator`    | activation owner      |
|  [04]   | `IViewFor<TViewModel>`  | typed view binding    |
|  [05]   | `IScreen`               | routing host          |
|  [06]   | `IRoutableViewModel`    | routed screen         |
|  [07]   | `RoutingState`          | navigation owner      |

[ACTIVATION_AND_ROUTING_SIGNATURES]:
- `IActivatableViewModel`: `interface` exposing `ViewModelActivator Activator`.
- `IActivatableView`: `interface` marking views for `WhenActivated`.
- `ViewModelActivator`: `sealed : IDisposable` exposing `Activate()`, `Deactivate()`, `Activated`, and `Deactivated`.
- `IViewFor<TViewModel>`: `interface : IViewFor` exposing `TViewModel? ViewModel`.
- `IScreen`: `interface` exposing `RoutingState Router`.
- `IRoutableViewModel`: `interface` exposing `string? UrlPathSegment` and `IScreen HostScreen`.
- `RoutingState`: `class : ReactiveObject` exposing `NavigationStack`, `Navigate`, `NavigateBack`, `NavigateAndReset`, `CurrentViewModel`, and `NavigationChanged`.

[INFRASTRUCTURE_TYPES]: message bus, suspension, builder, view mapping — rail: reactive

| [INDEX] | [SYMBOL]                                   | [KIND]             |
| :-----: | :----------------------------------------- | :----------------- |
|  [01]   | `IMessageBus` / `MessageBus`               | decoupled bus      |
|  [02]   | `ISuspensionHost` / `SuspensionHost`       | app-state host     |
|  [03]   | `ISuspensionDriver`                        | state persistence  |
|  [04]   | `IReactiveUIBuilder` / `ReactiveUIBuilder` | builder            |
|  [05]   | `RxAppBuilder`                             | builder root       |
|  [06]   | `ViewMappingBuilder`                       | view registry      |
|  [07]   | `IViewLocator` / `DefaultViewLocator`      | view resolution    |
|  [08]   | `IBindingTypeConverter`                    | converter contract |

[INFRASTRUCTURE_SIGNATURES]:
- `IMessageBus` / `MessageBus`: `interface` exposing `Listen<T>`, `ListenIncludeLatest<T>`, `RegisterMessageSource<T>`, and `SendMessage<T>`; `MessageBus.Current` owns the shared instance.
- `ISuspensionHost` / `SuspensionHost`: `interface` exposing `AppState`, `CreateNewAppState`, `IsLaunchingNew`, `IsResuming`, and `ShouldPersistState`.
- `ISuspensionDriver`: `interface` exposing `LoadState`, `SaveState`, and `InvalidateState`.
- `IReactiveUIBuilder` / `ReactiveUIBuilder`: `interface` / `class` fluent registration pair.
- `RxAppBuilder`: `static class` exposing `CreateReactiveUIBuilder` and `EnsureInitialized`.
- `ViewMappingBuilder`: `sealed class` exposing `Map<TViewModel,TView>` and `MapFromServiceLocator<…>`.
- `IViewLocator` / `DefaultViewLocator`: `ResolveView<T>` interface and default implementation pair.
- `IBindingTypeConverter`: `interface` exposing `GetAffinityForObjects` and `TryConvert`.

## [03]-[ENTRYPOINTS]

[STATE_ENTRYPOINTS]: property mutation and derived-state streams
- rail: reactive

| [INDEX] | [SURFACE]                                                  | [ROOT]                                   |
| :-----: | :--------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `RaiseAndSetIfChanged`                                     | `IReactiveObjectExtensions`              |
|  [02]   | `WhenAnyValue`                                             | `WhenAnyMixin`                           |
|  [03]   | `ToProperty`                                               | `OAPHCreationHelperMixin`                |
|  [04]   | `ThrownExceptions`                                         | `ReactiveObject` / `ReactiveCommandBase` |
|  [05]   | `SuppressChangeNotifications` / `DelayChangeNotifications` | `ReactiveObject`                         |

[STATE_SIGNATURES]:
- `RaiseAndSetIfChanged`: `TRet RaiseAndSetIfChanged<TObj,TRet>(this TObj, ref TRet backingField, TRet newValue, [CallerMemberName] string? = null)`.
- `WhenAnyValue`: `IObservable<TRet> WhenAnyValue<TSender,…>(this TSender, Expression<Func<…>>…[, selector])` across one through N properties.
- `ToProperty`: `ObservableAsPropertyHelper<TRet> ToProperty<TObj,TRet>(...)`.
- `ThrownExceptions`: `IObservable<Exception> ThrownExceptions`.
- `SuppressChangeNotifications` / `DelayChangeNotifications`: `IDisposable` gates that batch or suspend `PropertyChanged`.

`WhenAnyValue` is the property-stream source: a single-property overload (`vm.WhenAnyValue(x => x.Name)`) or a multi-property overload with a combining selector (`vm.WhenAnyValue(x => x.A, x => x.B, (a,b) => a && b)`) — the latter is the `canExecute`/derived-property combiner. `ToProperty` projects an observable onto a read-only backing `ObservableAsPropertyHelper<T>`; the property getter returns `_helper.Value`. `deferSubscription` delays the subscription until the first `Value` read.

`ToProperty` accepts the source object, property expression, optional initial value factory, `deferSubscription`, and scheduler.

[COMMAND_ENTRYPOINTS]: command construction and the observable→command bridge
- rail: reactive

| [INDEX] | [SURFACE]                    | [ROOT]                   |
| :-----: | :--------------------------- | :----------------------- |
|  [01]   | `Create`                     | `ReactiveCommand`        |
|  [02]   | `CreateFromTask`             | `ReactiveCommand`        |
|  [03]   | `CreateFromObservable`       | `ReactiveCommand`        |
|  [04]   | `CreateRunInBackground`      | `ReactiveCommand`        |
|  [05]   | `CreateCombined`             | `ReactiveCommand`        |
|  [06]   | `Execute`                    | `ReactiveCommand<…>`     |
|  [07]   | `CanExecute` / `IsExecuting` | `ReactiveCommandBase<…>` |
|  [08]   | `InvokeCommand`              | `ReactiveCommandMixins`  |

[COMMAND_SIGNATURES]:
- `Create`: `ReactiveCommand<TParam,TResult> Create[<…>](Action|Func<…>, IObservable<bool>? canExecute = null, IScheduler? outputScheduler = null)`.
- `CreateFromTask`: `ReactiveCommand<…> CreateFromTask[<…>](Func<…,CancellationToken,Task<…>> execute, IObservable<bool>? canExecute = null, IScheduler? = null)`.
- `CreateFromObservable`: `ReactiveCommand<…> CreateFromObservable[<…>](Func<…,IObservable<…>>, IObservable<bool>? canExecute = null, IScheduler? = null)`.
- `CreateRunInBackground`: `ReactiveCommand<…> CreateRunInBackground[<…>](…, IScheduler? backgroundScheduler = null, IScheduler? outputScheduler = null)`.
- `CreateCombined`: `CombinedReactiveCommand<TParam,TResult> CreateCombined<…>(IEnumerable<ReactiveCommandBase<…>>, IObservable<bool>? canExecute = null, IScheduler? = null)`.
- `Execute`: `IObservable<TResult> Execute([TParam])` returns the subscribed or awaited result stream.
- `CanExecute` / `IsExecuting`: `IObservable<bool>` gate and in-flight streams.
- `InvokeCommand`: `IDisposable InvokeCommand<…>(this IObservable<T>, ICommand|ReactiveCommandBase<…>|Expression<Func<TTarget,ICommand?>>)`.

`CreateFromTask` with a `Func<…,CancellationToken,Task>` is the canonical async leg: the token is cancelled when a subsequent execution starts or the command is disposed — the design page's long-running Compute calls bind here. `canExecute` is an `IObservable<bool>` produced by `WhenAnyValue`. `InvokeCommand` is the trigger bridge (`this.WhenAnyValue(x => x.Save).InvokeCommand(vm, x => x.SaveCommand)` or `keyStream.InvokeCommand(vm.SaveCommand)`) — it respects `CanExecute` and disposes the subscription cleanly. The command result is itself an observable from `Execute`, so chains stay reactive.

[ACTIVATION_AND_ROUTING_ENTRYPOINTS]: lifecycle scope and navigation
- rail: reactive

| [INDEX] | [SURFACE]                                         | [ROOT]               |
| :-----: | :------------------------------------------------ | :------------------- |
|  [01]   | `WhenActivated`                                   | `ViewForMixins`      |
|  [02]   | `Activate` / `Deactivate`                         | `ViewModelActivator` |
|  [03]   | `Navigate` / `NavigateBack` / `NavigateAndReset`  | `RoutingState`       |
|  [04]   | `CurrentViewModel`                                | `RoutingState`       |
|  [05]   | `NavigationChanged`                               | `RoutingState`       |
|  [06]   | `FindViewModelInStack<T>` / `GetCurrentViewModel` | `RoutingStateMixins` |

[ACTIVATION_AND_ROUTING_ENTRYPOINT_SIGNATURES]:
- `WhenActivated`: `IDisposable WhenActivated(this IActivatableView, Action<CompositeDisposable> block)` with `IActivatableViewModel` overloads.
- `Activate` / `Deactivate`: `IDisposable Activate()` and `void Deactivate(bool ignoreRefCount = false)`.
- `Navigate` / `NavigateBack` / `NavigateAndReset`: `ReactiveCommand<IRoutableViewModel,IRoutableViewModel>`, `ReactiveCommand<Unit,IRoutableViewModel>`, and corresponding command forms.
- `CurrentViewModel`: `IObservable<IRoutableViewModel>` top-of-stack stream.
- `NavigationChanged`: `IObservable<IChangeSet<IRoutableViewModel>>` DynamicData stack delta.
- `FindViewModelInStack<T>` / `GetCurrentViewModel`: `T?` and `IRoutableViewModel?` stack queries.

`WhenActivated(d => { … d(subscription); … })` registers subscriptions into a `CompositeDisposable` torn down on deactivation — every binding and `ToProperty` in a view is disposed here, and this is where a TextMate `Installation` or DynamicData subscription is scoped. `RoutingState.NavigationChanged` is a DynamicData `IChangeSet`, so a breadcrumb projection composes `.Bind(out var breadcrumbs)` directly. `Navigate.Execute(new SettingsViewModel(this))` is the push.

[INTERACTION_ENTRYPOINTS]: dialog request/response
- rail: reactive

| [INDEX] | [SURFACE]         | [ROOT]                        |
| :-----: | :---------------- | :---------------------------- |
|  [01]   | `RegisterHandler` | `Interaction<TInput,TOutput>` |
|  [02]   | `Handle`          | `Interaction<TInput,TOutput>` |
|  [03]   | `SetOutput`       | `IInteractionContext<…>`      |
|  [04]   | `BindInteraction` | `InteractionBindingMixins`    |

[INTERACTION_SIGNATURES]:
- `RegisterHandler`: `IDisposable RegisterHandler(Action<IInteractionContext<…>> | Func<…,Task> | Func<…,IObservable<T>>)`.
- `Handle`: `IObservable<TOutput> Handle(TInput input)`.
- `SetOutput`: `void SetOutput(TOutput output)` exactly once.
- `BindInteraction`: `IDisposable BindInteraction<…>(this TView, TViewModel?, Expression<Func<TViewModel,IInteraction<…>>>, Func<IInteractionContext<…>,Task> handler)`.

`Interaction<TInput,TOutput>` is the dialog rail: the view-model exposes `Interaction<DialogIntent, object?> ShowDialog { get; } = new();` and `await ShowDialog.Handle(intent)`; the view registers the actual dialog in `WhenActivated` via `BindInteraction` or `RegisterHandler(async ctx => ctx.SetOutput(await dialog.Show(ctx.Input)))`. An unhandled `Handle` throws `UnhandledInteractionException`. `IsHandled` guards a multi-handler chain.

[VIEW_BINDING_ENTRYPOINTS]: the expression-compiler binding surface (extension methods on `IViewFor` views; activated by `ReactiveUI.Avalonia`)
- rail: reactive

| [INDEX] | [SURFACE]     | [ROOT]                              |
| :-----: | :------------ | :---------------------------------- |
|  [01]   | `Bind`        | `PropertyBindingMixins`             |
|  [02]   | `OneWayBind`  | `PropertyBindingMixins`             |
|  [03]   | `BindTo`      | `PropertyBindingMixins`             |
|  [04]   | `BindCommand` | `CommandBinderImplementationMixins` |

[VIEW_BINDING_SIGNATURES]:
- `Bind`: `IReactiveBinding<…> Bind<…>(this TView, TViewModel?, Expression<Func<TViewModel,TVMProp?>> vmProperty, Expression<Func<TView,TVProp>> viewProperty[, conversion/converter overrides])`.
- `OneWayBind`: `IReactiveBinding<…> OneWayBind<…>(this TView, TViewModel?, Expression<Func<…>> vmProperty, Expression<Func<…>> viewProperty[, Func<TProp,TOut> selector])`.
- `BindTo`: `IDisposable BindTo<…>(this IObservable<TValue>, TTarget?, Expression<Func<TTarget,TTValue?>> property)`.
- `BindCommand`: `IReactiveBinding<…> BindCommand<…>(this ICommandBinderImplementation, TViewModel?, TView, Expression<Func<TViewModel,TProp?>> command, Expression<Func<TView,TControl>> control, string? toEvent = null)`.

These are the alternative to compiled-XAML bindings; they keep the view-model property names refactor-safe (expression trees) and route through the `IBindingTypeConverter` registry (the full string↔primitive converter set auto-registers via `PlatformRegistrations`). `BindCommand` maps a `ReactiveCommand` to a control and optionally a non-default event (`toEvent`).

[BUILDER_AND_SUSPENSION_ENTRYPOINTS]: registration and state persistence
- rail: reactive

| [INDEX] | [SURFACE]                                                            | [ROOT]                     |
| :-----: | :------------------------------------------------------------------- | :------------------------- |
|  [01]   | `CreateReactiveUIBuilder`                                            | `RxAppBuilder`             |
|  [02]   | `RegisterViews`                                                      | `BuilderMixins`            |
|  [03]   | `Map<TViewModel,TView>`                                              | `ViewMappingBuilder`       |
|  [04]   | `WithMainThreadScheduler` / `WithTaskPoolScheduler`                  | `ReactiveUIBuilder`        |
|  [05]   | `WithRegistration` / `WithRegistrationOnBuild`                       | `ReactiveUIBuilder`        |
|  [06]   | `WithViewsFromAssembly` / `WithPlatformModule<T>` / `WithMessageBus` | `ReactiveUIBuilder`        |
|  [07]   | `SetupDefaultSuspendResume`                                          | `SuspensionHostExtensions` |
|  [08]   | `GetAppState<T>` / `ObserveAppState<T>`                              | `SuspensionHostExtensions` |

[BUILDER_AND_SUSPENSION_SIGNATURES]:
- `CreateReactiveUIBuilder`: `static ReactiveUIBuilder CreateReactiveUIBuilder([this IMutableDependencyResolver])`.
- `RegisterViews`: `IReactiveUIBuilder RegisterViews(this IReactiveUIBuilder, Action<ViewMappingBuilder> configure)`.
- `Map<TViewModel,TView>`: `ViewMappingBuilder Map<TViewModel,TView>([Func<TView> factory,] string? contract = null)`.
- `WithMainThreadScheduler` / `WithTaskPoolScheduler`: `IReactiveUIBuilder With…(IScheduler, bool setRxApp = true)`.
- `WithRegistration` / `WithRegistrationOnBuild`: `IReactiveUIBuilder With…(Action<IMutableDependencyResolver>)`.
- `WithViewsFromAssembly` / `WithPlatformModule<T>` / `WithMessageBus`: assembly scan, platform module, and bus admission.
- `SetupDefaultSuspendResume`: `IDisposable SetupDefaultSuspendResume<TAppState>(this ISuspensionHost<TAppState>, JsonTypeInfo<TAppState> typeInfo, ISuspensionDriver? = null)`.
- `GetAppState<T>` / `ObserveAppState<T>`: `T` and `IObservable<T>` typed app-state access.

View registration is `RegisterViews(m => m.Map<HomeViewModel, HomeView>())` over a `ViewMappingBuilder` — not a per-view `RegisterView<…>` method. `SetupDefaultSuspendResume` has an AOT-safe `JsonTypeInfo<TAppState>` overload that composes the `System.Text.Json` source-generated context the manifest already uses, so app-state persistence needs no reflection. The whole `string`↔primitive `IBindingTypeConverter` set registers automatically; there is no `RegisterStandardConverters` call to make.

## [04]-[INTEGRATION_LAW]

[SCREEN_OWNER_LAW]:
- Stack: a screen view-model is `ReactiveObject` + `IActivatableViewModel`; mutable inputs use `RaiseAndSetIfChanged`; derived outputs use `WhenAnyValue(...).ToProperty(this, x => x.Derived)`; user actions are `ReactiveCommand.CreateFromTask(token => …, canExecute: this.WhenAnyValue(x => x.IsValid))`; dialogs are `Interaction<TIn,TOut>` resolved in the view's `WhenActivated`; navigation is `IScreen.Router` (`RoutingState`); cross-component facts ride `MessageBus` or a shared `Interaction`. Every subscription is scoped into the `WhenActivated` `CompositeDisposable`.
- Accept: state is observable, disposable, command-driven, and activation-scoped; collection state surfaces as DynamicData `IChangeSet` (e.g. `RoutingState.NavigationChanged`) so list views bind deltas, not full resets.
- Reject: manual event fanout; `async void` command bodies (use `CreateFromTask`); undisposed `IObservable` subscriptions; `RegisterView<…>`/`RegisterStandardConverters`/`WithExceptionHandler` (non-existent in `23.2.28`).

[RAIL_BOUNDARY_LAW]:
- `System.Reactive` owns the stream algebra (`Throttle`/`Select`/`ObserveOn`) every ReactiveUI member composes; `DynamicData` owns collection deltas; `ReactiveUI.Avalonia` owns the `AvaloniaScheduler` (`outputScheduler`/main-thread), view bases, and `Bind`/`BindCommand` activation; `ReactiveUI.Validation` owns the view-model-scoped `ValidationContext`/`ValidationRule`/`ReactiveValidationObject` aggregator (its `INotifyDataErrorInfo` fed by the context), which sits beside — not on top of — the core `ReactiveProperty<T>` per-property `INotifyDataErrorInfo` (`AddValidationError`/`HasErrors`/`ObserveHasErrors`) this catalog already owns. This catalog's owner never re-implements a scheduler, a collection-delta engine, or the validation aggregator — it composes those rails.
- Accept: host panel, GH2 companion window, standalone desktop, sidecar, and headless proof share one reactive rail — the same `ReactiveCommand`/`Interaction`/`RoutingState` vocabulary across every `SurfaceHost` modality.
- Reject: per-view imperative callback chains; a second observable/property-change framework alongside ReactiveUI; treating `AvaloniaScheduler` or validation as a ReactiveUI-core concern.
