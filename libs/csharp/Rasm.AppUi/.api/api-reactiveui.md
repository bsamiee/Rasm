# [RASM_APPUI_API_REACTIVEUI]

`ReactiveUI` is the MVVM rail: `ReactiveObject` change-notification, `ReactiveCommand` async/cancellable command execution, `WhenAnyValue` property streams, `ObservableAsPropertyHelper` derived properties, `Interaction` request/response for dialogs, `RoutingState` navigation, `ViewModelActivator`/`WhenActivated` lifecycle, `SuspensionHost` state persistence, and a `Bind`/`OneWayBind`/`BindCommand`/`BindInteraction` view-binding expression compiler. It sits on `System.Reactive` (every member returns/consumes `IObservable<T>`) and surfaces collection deltas as DynamicData `IChangeSet`; the Avalonia bridge (view bases, `AvaloniaScheduler`, builder admission) lives in the sibling `ReactiveUI.Avalonia` catalog. `ReactiveProperty<T>` carries its own core per-property `INotifyDataErrorInfo` (`AddValidationError`/`HasErrors`/`ObserveHasErrors`) independent of any package; the separate view-model-scoped `ValidationContext`/`ValidationRule`/`ReactiveValidationObject` aggregator lives in the sibling `ReactiveUI.Validation` catalog. This catalog documents the advanced surface and how it stacks those rails into one screen owner — the prior version's `RegisterView<…>`/`RegisterStandardConverters`/`WithExceptionHandler` builder methods do not exist in `23.2.28` and are removed.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI`
- package: `ReactiveUI`
- version: `23.2.28`
- license: `MIT`
- assembly: `ReactiveUI` (AnyCPU IL)
- build-floor: `net10.0` (consumer-bound; multi-targets android/ios/maccatalyst/tizen/wasm — none bound here)
- namespace: `ReactiveUI` (core), `ReactiveUI.Builder` (app builder), `ReactiveUI.Helpers`
- asset: runtime library
- rail: reactive
- depends: `System.Reactive` (observable algebra), `DynamicData` (`IChangeSet`/`SourceList`), `Splat` (`IMutableDependencyResolver` / service location). Property/command **binding** (`Bind`/`BindCommand`) is platform-coupled and activated through `ReactiveUI.Avalonia`'s `UseReactiveUI`.

## [02]-[PUBLIC_TYPES]

[VIEW_MODEL_TYPES]: reactive state, derived properties, and the validation-aware property — rail: reactive

| [INDEX] | [SYMBOL]                                     | [SIGNATURE]                                                         | [KIND]            |
| :-----: | :------------------------------------------- | :----------------------------------------------------------------- | :---------------- |
|  [01]   | `ReactiveObject`                             | `class : IReactiveObject` (`Changing`/`Changed`/`ThrownExceptions`)| reactive state    |
|  [02]   | `ReactiveRecord`                             | `record`-friendly `ReactiveObject` base                            | reactive record   |
|  [03]   | `IReactiveObject`                            | `interface : INotifyPropertyChanged, INotifyPropertyChanging, IReactiveNotifyPropertyChanged<IReactiveObject>, IHandleObservableErrors` | state contract |
|  [04]   | `IReactiveNotifyPropertyChanged<TSender>`    | `interface` (`Changing`/`Changed` of `IReactivePropertyChangedEventArgs`) | change contract |
|  [05]   | `ObservableAsPropertyHelper<T>`              | `sealed class : IDisposable` (`Value`, `IsSubscribed`)             | derived property  |
|  [06]   | `ReactiveProperty<T>`                        | `class : ReactiveObject, IReactiveProperty<T>, IObservable<T?>, ICancelable, INotifyDataErrorInfo` (`Value`, `HasErrors`, `ObserveHasErrors`, `ObserveErrorChanged`, `AddValidationError(Func<IObservable<T?>,IObservable<IEnumerable?>>, bool ignoreInitialError = false)`, static `Create`) | bindable+self-validating property |
|  [07]   | `IHandleObservableErrors`                    | `interface` (`ThrownExceptions`)                                   | error stream      |

[COMMAND_AND_INTERACTION_TYPES]: command execution and dialog request rails — rail: reactive

| [INDEX] | [SYMBOL]                                        | [SIGNATURE]                                                              | [KIND]               |
| :-----: | :---------------------------------------------- | :---------------------------------------------------------------------- | :------------------- |
|  [01]   | `ReactiveCommand`                               | `static class` — the `Create*` factory family                           | command factory      |
|  [02]   | `ReactiveCommand<TParam,TResult>`               | `class : ReactiveCommandBase<TParam,TResult>, ICommand` (`Execute`)      | command execution    |
|  [03]   | `ReactiveCommandBase<TParam,TResult>`           | `abstract` (`CanExecute`/`IsExecuting`/`ThrownExceptions`)               | command base         |
|  [04]   | `CombinedReactiveCommand<TParam,TResult>`       | `class` — fan-out over child commands                                   | command batch        |
|  [05]   | `Interaction<TInput,TOutput>`                   | `class : IInteraction<TInput,TOutput>` (ctor `IScheduler?`)             | interaction source   |
|  [06]   | `IInteractionContext<TInput,TOutput>`           | `interface` (`Input`, `IsHandled`, `SetOutput(TOutput)`)                | interaction context  |
|  [07]   | `UnhandledInteractionException<TInput,TOutput>` | `exception` — no handler set output                                     | interaction error    |

[ACTIVATION_AND_ROUTING_TYPES]: lifecycle and navigation contracts — rail: reactive

| [INDEX] | [SYMBOL]                | [SIGNATURE]                                                              | [KIND]                |
| :-----: | :---------------------- | :---------------------------------------------------------------------- | :-------------------- |
|  [01]   | `IActivatableViewModel` | `interface` (`ViewModelActivator Activator`)                            | view-model activation |
|  [02]   | `IActivatableView`      | `interface` (marker for `WhenActivated`)                                | view activation       |
|  [03]   | `ViewModelActivator`    | `sealed : IDisposable` (`Activate()`/`Deactivate()`/`Activated`/`Deactivated`) | activation owner |
|  [04]   | `IViewFor<TViewModel>`  | `interface : IViewFor` (`TViewModel? ViewModel`)                        | typed view binding    |
|  [05]   | `IScreen`               | `interface` (`RoutingState Router`)                                     | routing host          |
|  [06]   | `IRoutableViewModel`    | `interface` (`string? UrlPathSegment`, `IScreen HostScreen`)           | routed screen         |
|  [07]   | `RoutingState`          | `class : ReactiveObject` (`NavigationStack`, `Navigate`/`NavigateBack`/`NavigateAndReset`, `CurrentViewModel`, `NavigationChanged`) | navigation owner |

[INFRASTRUCTURE_TYPES]: message bus, suspension, builder, view mapping — rail: reactive

| [INDEX] | [SYMBOL]              | [SIGNATURE]                                                              | [KIND]             |
| :-----: | :-------------------- | :---------------------------------------------------------------------- | :----------------- |
|  [01]   | `IMessageBus` / `MessageBus` | `interface` (`Listen<T>`/`ListenIncludeLatest<T>`/`RegisterMessageSource<T>`/`SendMessage<T>`); `MessageBus.Current` | decoupled bus |
|  [02]   | `ISuspensionHost` / `SuspensionHost` | `interface` (`AppState`, `CreateNewAppState`, `IsLaunchingNew`/`IsResuming`/`ShouldPersistState`) | app-state host |
|  [03]   | `ISuspensionDriver`   | `interface` (`LoadState`/`SaveState`/`InvalidateState`)                  | state persistence  |
|  [04]   | `IReactiveUIBuilder` / `ReactiveUIBuilder` | `interface` / `class` — fluent registration             | builder            |
|  [05]   | `RxAppBuilder`        | `static class` (`CreateReactiveUIBuilder`, `EnsureInitialized`)         | builder root       |
|  [06]   | `ViewMappingBuilder`  | `sealed class` (`Map<TViewModel,TView>`, `MapFromServiceLocator<…>`)     | view registry      |
|  [07]   | `IViewLocator` / `DefaultViewLocator` | `interface` (`ResolveView<T>`) / default impl           | view resolution    |
|  [08]   | `IBindingTypeConverter` | `interface` (`GetAffinityForObjects`, `TryConvert`)                    | converter contract |

## [03]-[ENTRYPOINTS]

[STATE_ENTRYPOINTS]: property mutation and derived-state streams
- rail: reactive

| [INDEX] | [SURFACE]                    | [SIGNATURE]                                                                                   | [SURFACE_ROOT]              | [RAIL]            |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------- | :-------------------------- | :---------------- |
|  [01]   | `RaiseAndSetIfChanged`       | `TRet RaiseAndSetIfChanged<TObj,TRet>(this TObj, ref TRet backingField, TRet newValue, [CallerMemberName] string? = null)` | `IReactiveObjectExtensions` | property update |
|  [02]   | `WhenAnyValue`               | `IObservable<TRet> WhenAnyValue<TSender,…>(this TSender, Expression<Func<…>>…[, selector])` (1..N props) | `WhenAnyMixin`        | property stream   |
|  [03]   | `ToProperty`                 | `ObservableAsPropertyHelper<TRet> ToProperty<TObj,TRet>(this IObservable<TRet>, TObj source, Expression<Func<TObj,TRet>> property, [initialValue|getInitialValue], bool deferSubscription = false, IScheduler? = null)` | `OAPHCreationHelperMixin` | derived property |
|  [04]   | `ThrownExceptions`           | `IObservable<Exception> ThrownExceptions`                                                     | `ReactiveObject` / `ReactiveCommandBase` | error stream |
|  [05]   | `SuppressChangeNotifications`/`DelayChangeNotifications` | `IDisposable` — batch/suspend `PropertyChanged`                   | `ReactiveObject`            | notification gate |

`WhenAnyValue` is the property-stream source: a single-property overload (`vm.WhenAnyValue(x => x.Name)`) or a multi-property overload with a combining selector (`vm.WhenAnyValue(x => x.A, x => x.B, (a,b) => a && b)`) — the latter is the `canExecute`/derived-property combiner. `ToProperty` projects an observable onto a read-only backing `ObservableAsPropertyHelper<T>`; the property getter returns `_helper.Value`. `deferSubscription` delays the subscription until the first `Value` read.

[COMMAND_ENTRYPOINTS]: command construction and the observable→command bridge
- rail: reactive

| [INDEX] | [SURFACE]              | [SIGNATURE]                                                                                                   | [SURFACE_ROOT]          | [RAIL]            |
| :-----: | :--------------------- | :----------------------------------------------------------------------------------------------------------- | :---------------------- | :---------------- |
|  [01]   | `Create`               | `ReactiveCommand<TParam,TResult> Create[<…>](Action|Func<…>, IObservable<bool>? canExecute = null, IScheduler? outputScheduler = null)` | `ReactiveCommand` | sync command |
|  [02]   | `CreateFromTask`       | `ReactiveCommand<…> CreateFromTask[<…>](Func<…,CancellationToken,Task<…>> execute, IObservable<bool>? canExecute = null, IScheduler? = null)` | `ReactiveCommand` | async+cancel command |
|  [03]   | `CreateFromObservable` | `ReactiveCommand<…> CreateFromObservable[<…>](Func<…,IObservable<…>>, IObservable<bool>? canExecute = null, IScheduler? = null)` | `ReactiveCommand` | stream command |
|  [04]   | `CreateRunInBackground`| `ReactiveCommand<…> CreateRunInBackground[<…>](…, IScheduler? backgroundScheduler = null, IScheduler? outputScheduler = null)` | `ReactiveCommand` | off-thread command |
|  [05]   | `CreateCombined`       | `CombinedReactiveCommand<TParam,TResult> CreateCombined<…>(IEnumerable<ReactiveCommandBase<…>>, IObservable<bool>? canExecute = null, IScheduler? = null)` | `ReactiveCommand` | command batch |
|  [06]   | `Execute`              | `IObservable<TResult> Execute([TParam])` — returns the result stream (subscribe or `await`)                 | `ReactiveCommand<…>`    | command run       |
|  [07]   | `CanExecute` / `IsExecuting` | `IObservable<bool>` — gate and in-flight state                                                       | `ReactiveCommandBase<…>`| command state     |
|  [08]   | `InvokeCommand`        | `IDisposable InvokeCommand<…>(this IObservable<T>, ICommand|ReactiveCommandBase<…>|Expression<Func<TTarget,ICommand?>>)` | `ReactiveCommandMixins` | observable→command |

`CreateFromTask` with a `Func<…,CancellationToken,Task>` is the canonical async leg: the token is cancelled when a subsequent execution starts or the command is disposed — the design page's long-running Compute calls bind here. `canExecute` is an `IObservable<bool>` produced by `WhenAnyValue`. `InvokeCommand` is the trigger bridge (`this.WhenAnyValue(x => x.Save).InvokeCommand(vm, x => x.SaveCommand)` or `keyStream.InvokeCommand(vm.SaveCommand)`) — it respects `CanExecute` and disposes the subscription cleanly. The command result is itself an observable from `Execute`, so chains stay reactive.

[ACTIVATION_AND_ROUTING_ENTRYPOINTS]: lifecycle scope and navigation
- rail: reactive

| [INDEX] | [SURFACE]                   | [SIGNATURE]                                                                          | [SURFACE_ROOT]       | [RAIL]               |
| :-----: | :-------------------------- | :---------------------------------------------------------------------------------- | :------------------- | :------------------- |
|  [01]   | `WhenActivated`             | `IDisposable WhenActivated(this IActivatableView, Action<CompositeDisposable> block)` (and `IActivatableViewModel` overloads) | `ViewForMixins` | activation scope |
|  [02]   | `Activate` / `Deactivate`   | `IDisposable Activate()` / `void Deactivate(bool ignoreRefCount = false)`            | `ViewModelActivator` | activation signal    |
|  [03]   | `Navigate` / `NavigateBack` / `NavigateAndReset` | `ReactiveCommand<IRoutableViewModel,IRoutableViewModel>` / `ReactiveCommand<Unit,IRoutableViewModel>` / `…` | `RoutingState` | navigation |
|  [04]   | `CurrentViewModel`          | `IObservable<IRoutableViewModel>` — top-of-stack stream                              | `RoutingState`       | active screen        |
|  [05]   | `NavigationChanged`         | `IObservable<IChangeSet<IRoutableViewModel>>` — DynamicData stack delta              | `RoutingState`       | stack delta          |
|  [06]   | `FindViewModelInStack<T>` / `GetCurrentViewModel` | `T?` / `IRoutableViewModel?`                                  | `RoutingStateMixins` | stack query          |

`WhenActivated(d => { … d(subscription); … })` registers subscriptions into a `CompositeDisposable` torn down on deactivation — every binding and `ToProperty` in a view is disposed here, and this is where a TextMate `Installation` or DynamicData subscription is scoped. `RoutingState.NavigationChanged` is a DynamicData `IChangeSet`, so a breadcrumb projection composes `.Bind(out var breadcrumbs)` directly. `Navigate.Execute(new SettingsViewModel(this))` is the push.

[INTERACTION_ENTRYPOINTS]: dialog request/response
- rail: reactive

| [INDEX] | [SURFACE]         | [SIGNATURE]                                                                                      | [SURFACE_ROOT]                 | [RAIL]              |
| :-----: | :---------------- | :---------------------------------------------------------------------------------------------- | :----------------------------- | :------------------ |
|  [01]   | `RegisterHandler` | `IDisposable RegisterHandler(Action<IInteractionContext<…>> | Func<…,Task> | Func<…,IObservable<T>>)` | `Interaction<TInput,TOutput>` | handler admission   |
|  [02]   | `Handle`          | `IObservable<TOutput> Handle(TInput input)`                                                      | `Interaction<TInput,TOutput>`  | interaction request |
|  [03]   | `SetOutput`       | `void SetOutput(TOutput output)` (exactly once)                                                  | `IInteractionContext<…>`       | result projection   |
|  [04]   | `BindInteraction` | `IDisposable BindInteraction<…>(this TView, TViewModel?, Expression<Func<TViewModel,IInteraction<…>>>, Func<IInteractionContext<…>,Task> handler)` | `InteractionBindingMixins` | view binding |

`Interaction<TInput,TOutput>` is the dialog rail: the view-model exposes `Interaction<DialogIntent, object?> ShowDialog { get; } = new();` and `await ShowDialog.Handle(intent)`; the view registers the actual dialog in `WhenActivated` via `BindInteraction` or `RegisterHandler(async ctx => ctx.SetOutput(await dialog.Show(ctx.Input)))`. An unhandled `Handle` throws `UnhandledInteractionException`. `IsHandled` guards a multi-handler chain.

[VIEW_BINDING_ENTRYPOINTS]: the expression-compiler binding surface (extension methods on `IViewFor` views; activated by `ReactiveUI.Avalonia`)
- rail: reactive

| [INDEX] | [SURFACE]      | [SIGNATURE]                                                                                                                | [SURFACE_ROOT]                 | [RAIL]            |
| :-----: | :------------- | :------------------------------------------------------------------------------------------------------------------------ | :----------------------------- | :---------------- |
|  [01]   | `Bind`         | `IReactiveBinding<…> Bind<…>(this TView, TViewModel?, Expression<Func<TViewModel,TVMProp?>> vmProperty, Expression<Func<TView,TVProp>> viewProperty[, conversion/converter overrides])` | `PropertyBindingMixins` | two-way bind |
|  [02]   | `OneWayBind`   | `IReactiveBinding<…> OneWayBind<…>(this TView, TViewModel?, Expression<Func<…>> vmProperty, Expression<Func<…>> viewProperty[, Func<TProp,TOut> selector])` | `PropertyBindingMixins` | one-way bind |
|  [03]   | `BindTo`       | `IDisposable BindTo<…>(this IObservable<TValue>, TTarget?, Expression<Func<TTarget,TTValue?>> property)`                    | `PropertyBindingMixins`        | observable→target |
|  [04]   | `BindCommand`  | `IReactiveBinding<…> BindCommand<…>(this ICommandBinderImplementation, TViewModel?, TView, Expression<Func<TViewModel,TProp?>> command, Expression<Func<TView,TControl>> control, string? toEvent = null)` | `CommandBinderImplementationMixins` | command bind |

These are the alternative to compiled-XAML bindings; they keep the view-model property names refactor-safe (expression trees) and route through the `IBindingTypeConverter` registry (the full string↔primitive converter set auto-registers via `PlatformRegistrations`). `BindCommand` maps a `ReactiveCommand` to a control and optionally a non-default event (`toEvent`).

[BUILDER_AND_SUSPENSION_ENTRYPOINTS]: registration and state persistence
- rail: reactive

| [INDEX] | [SURFACE]                    | [SIGNATURE]                                                                          | [SURFACE_ROOT]            | [RAIL]              |
| :-----: | :--------------------------- | :---------------------------------------------------------------------------------- | :------------------------ | :------------------ |
|  [01]   | `CreateReactiveUIBuilder`    | `static ReactiveUIBuilder CreateReactiveUIBuilder([this IMutableDependencyResolver])`| `RxAppBuilder`            | builder creation    |
|  [02]   | `RegisterViews`              | `IReactiveUIBuilder RegisterViews(this IReactiveUIBuilder, Action<ViewMappingBuilder> configure)` | `BuilderMixins` | view registration |
|  [03]   | `Map<TViewModel,TView>`      | `ViewMappingBuilder Map<TViewModel,TView>([Func<TView> factory,] string? contract = null)` | `ViewMappingBuilder` | view mapping      |
|  [04]   | `WithMainThreadScheduler` / `WithTaskPoolScheduler` | `IReactiveUIBuilder With…(IScheduler, bool setRxApp = true)`         | `ReactiveUIBuilder`       | scheduler config    |
|  [05]   | `WithRegistration` / `WithRegistrationOnBuild` | `IReactiveUIBuilder With…(Action<IMutableDependencyResolver>)`         | `ReactiveUIBuilder`       | service admission   |
|  [06]   | `WithViewsFromAssembly` / `WithPlatformModule<T>` / `WithMessageBus` | assembly scan / platform module / bus admission     | `ReactiveUIBuilder`       | bulk registration   |
|  [07]   | `SetupDefaultSuspendResume`  | `IDisposable SetupDefaultSuspendResume<TAppState>(this ISuspensionHost<TAppState>, JsonTypeInfo<TAppState> typeInfo, ISuspensionDriver? = null)` | `SuspensionHostExtensions` | state persistence |
|  [08]   | `GetAppState<T>` / `ObserveAppState<T>` | `T` / `IObservable<T>` — typed app-state access                         | `SuspensionHostExtensions`| app state           |

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
