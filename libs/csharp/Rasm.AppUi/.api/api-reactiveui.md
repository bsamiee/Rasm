# [RASM_APPUI_API_REACTIVEUI]

`ReactiveUI` supplies reactive view models, commands, activation, interaction binding, routing, property change flow, and observable property helpers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI`
- package: `ReactiveUI`
- assembly: `ReactiveUI`
- namespace: `ReactiveUI`
- namespace: `ReactiveUI.Builder`
- namespace: `ReactiveUI.Reflection`
- asset: runtime library
- rail: reactive

## [02]-[PUBLIC_TYPES]

[VIEW_MODEL_TYPES]: reactive state and view-model contracts — rail: reactive

| [INDEX] | [SYMBOL]                                     | [KIND]           |
| :-----: | :------------------------------------------- | :--------------- |
|  [01]   | `ReactiveObject`                             | reactive state   |
|  [02]   | `IReactiveObject`                            | state contract   |
|  [03]   | `IReactiveNotifyPropertyChanged<TSender>`    | change contract  |
|  [04]   | `IReactivePropertyChangedEventArgs<TSender>` | change event     |
|  [05]   | `ObservableAsPropertyHelper<T>`              | derived property |
|  [06]   | `IHandleObservableErrors`                    | error stream     |

[COMMAND_AND_INTERACTION_TYPES]: command and dialog rails — rail: reactive

| [INDEX] | [SYMBOL]                                        | [KIND]               |
| :-----: | :---------------------------------------------- | :------------------- |
|  [01]   | `ReactiveCommand<TParam,TResult>`               | command execution    |
|  [02]   | `IReactiveCommand<TParam,TResult>`              | command contract     |
|  [03]   | `CombinedReactiveCommand<TParam,TResult>`       | command batch        |
|  [04]   | `Interaction<TInput,TOutput>`                   | interaction source   |
|  [05]   | `IInteraction<TInput,TOutput>`                  | interaction contract |
|  [06]   | `InteractionContext<TInput,TOutput>`            | interaction context  |
|  [07]   | `UnhandledInteractionException<TInput,TOutput>` | interaction error    |

[ACTIVATION_AND_ROUTING_TYPES]: screen lifecycle contracts — rail: reactive

| [INDEX] | [SYMBOL]                | [KIND]                |
| :-----: | :---------------------- | :-------------------- |
|  [01]   | `IActivatableView`      | view activation       |
|  [02]   | `IActivatableViewModel` | view-model activation |
|  [03]   | `ViewModelActivator`    | activation owner      |
|  [04]   | `IViewFor`              | view binding          |
|  [05]   | `IViewFor<TViewModel>`  | typed view binding    |
|  [06]   | `IScreen`               | routing host          |
|  [07]   | `IRoutableViewModel`    | routed screen         |

[BUILDER_AND_REGISTRY_TYPES]: registration and binding infrastructure — rail: reactive

| [INDEX] | [SYMBOL]                       | [KIND]             |
| :-----: | :----------------------------- | :----------------- |
|  [01]   | `IReactiveUIBuilder`           | builder contract   |
|  [02]   | `ReactiveUIBuilder`            | builder runtime    |
|  [03]   | `RxAppBuilder`                 | builder root       |
|  [04]   | `IRegistrar`                   | service registry   |
|  [05]   | `CommandBinder`                | command binder     |
|  [06]   | `PropertyBinderImplementation` | property binder    |
|  [07]   | `BindingTypeConverterRegistry` | converter registry |

## [03]-[ENTRYPOINTS]

[STATE_ENTRYPOINTS]: property and derived-state operations
- rail: reactive

| [INDEX] | [SURFACE]                    | [SURFACE_ROOT]                       | [RAIL]            |
| :-----: | :--------------------------- | :----------------------------------- | :---------------- |
|  [01]   | `RaiseAndSetIfChanged`       | reactive object mixins               | property update   |
|  [02]   | `WhenAnyValue`               | change notification mixins           | property stream   |
|  [03]   | `ToProperty`                 | `OAPHCreationHelperMixin`            | derived property  |
|  [04]   | `ObservableToProperty`       | `OAPHCreationHelperMixin`            | derived property  |
|  [05]   | `ThrownExceptions`           | `IHandleObservableErrors`            | error stream      |
|  [06]   | `SubscribeToExpressionChain` | `ReactiveNotifyPropertyChangedMixin` | expression stream |

[COMMAND_ENTRYPOINTS]: command and command-binding operations
- rail: reactive

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]                        | [RAIL]          |
| :-----: | :--------------------- | :------------------------------------ | :-------------- |
|  [01]   | `Create`               | `ReactiveCommand`                     | command factory |
|  [02]   | `CreateFromTask`       | `ReactiveCommand`                     | task command    |
|  [03]   | `CreateFromObservable` | `ReactiveCommand`                     | stream command  |
|  [04]   | `CreateCombined`       | `ReactiveCommand`                     | command batch   |
|  [05]   | `CanExecute`           | `ReactiveCommandBase<TParam,TResult>` | command gate    |
|  [06]   | `IsExecuting`          | `ReactiveCommandBase<TParam,TResult>` | execution state |
|  [07]   | `BindCommand`          | `CommandBinder`                       | command binding |

[ACTIVATION_ENTRYPOINTS]: lifecycle operations
- rail: reactive

| [INDEX] | [SURFACE]                   | [SURFACE_ROOT]       | [RAIL]               |
| :-----: | :-------------------------- | :------------------- | :------------------- |
|  [01]   | `WhenActivated`             | `ViewForMixins`      | activation scope     |
|  [02]   | `HandleViewActivation`      | `ViewForMixins`      | view lifecycle       |
|  [03]   | `HandleViewModelActivation` | `ViewForMixins`      | view-model lifecycle |
|  [04]   | `Activate`                  | activation contracts | activation signal    |
|  [05]   | `Deactivate`                | activation contracts | deactivation signal  |

[INTERACTION_ENTRYPOINTS]: interaction and dialog operations
- rail: reactive

| [INDEX] | [SURFACE]         | [SURFACE_ROOT]                       | [RAIL]              |
| :-----: | :---------------- | :----------------------------------- | :------------------ |
|  [01]   | `RegisterHandler` | `Interaction<TInput,TOutput>`        | handler admission   |
|  [02]   | `Handle`          | `Interaction<TInput,TOutput>`        | interaction request |
|  [03]   | `SetOutput`       | `InteractionContext<TInput,TOutput>` | result projection   |
|  [04]   | `BindInteraction` | `InteractionBindingMixins`           | view binding        |
|  [05]   | `GetHandlers`     | `Interaction<TInput,TOutput>`        | handler lookup      |

[BUILDER_ENTRYPOINTS]: registration operations
- rail: reactive

| [INDEX] | [SURFACE]                             | [SURFACE_ROOT]       | [RAIL]              |
| :-----: | :------------------------------------ | :------------------- | :------------------ |
|  [01]   | `CreateReactiveUIBuilder`             | `RxAppBuilder`       | builder creation    |
|  [02]   | `RegisterView<TView,TModel>`          | `IReactiveUIBuilder` | view registration   |
|  [03]   | `RegisterSingletonView<TView,TModel>` | `IReactiveUIBuilder` | singleton view      |
|  [04]   | `RegisterViewModel<TModel>`           | `IReactiveUIBuilder` | model registration  |
|  [05]   | `RegisterStandardConverters`          | `ReactiveUIBuilder`  | converter admission |
|  [06]   | `WithExceptionHandler`                | `IReactiveUIBuilder` | error policy        |

## [04]-[IMPLEMENTATION_LAW]

[REACTIVE_STATE_LAW]:
- Package: `ReactiveUI`
- Owns: reactive state, derived properties, commands, interactions, activation, routing, and registrations
- Accept: screen state is observable, disposable, command-driven, and activation-scoped
- Reject: manual event fanout

[INTERACTION_LAW]:
- Package: `ReactiveUI`
- Owns: command execution, interaction requests, dialog handoff, and observable error flow
- Accept: host, companion, sidecar, diagnostics, and downstream app interactions share one reactive rail
- Reject: per-view imperative callback chains
