# [RASM_APPUI_API_REACTIVEUI]

`ReactiveUI` supplies reactive view models, commands, activation, interaction binding, routing, property change flow, and observable property helpers.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI`
- package: `ReactiveUI`
- assembly: `ReactiveUI`
- namespace: `ReactiveUI`
- namespace: `ReactiveUI.Builder`
- namespace: `ReactiveUI.Reflection`
- asset: runtime library
- rail: reactive

## [2]-[PUBLIC_TYPES]

[VIEW_MODEL_TYPES]: reactive state and view-model contracts — rail: reactive

| [INDEX] | [SYMBOL]                                     | [KIND]           |
| :-----: | :------------------------------------------- | :--------------- |
|   [1]   | `ReactiveObject`                             | reactive state   |
|   [2]   | `IReactiveObject`                            | state contract   |
|   [3]   | `IReactiveNotifyPropertyChanged<TSender>`    | change contract  |
|   [4]   | `IReactivePropertyChangedEventArgs<TSender>` | change event     |
|   [5]   | `ObservableAsPropertyHelper<T>`              | derived property |
|   [6]   | `IHandleObservableErrors`                    | error stream     |

[COMMAND_AND_INTERACTION_TYPES]: command and dialog rails — rail: reactive

| [INDEX] | [SYMBOL]                                        | [KIND]               |
| :-----: | :---------------------------------------------- | :------------------- |
|   [1]   | `ReactiveCommand<TParam,TResult>`               | command execution    |
|   [2]   | `IReactiveCommand<TParam,TResult>`              | command contract     |
|   [3]   | `CombinedReactiveCommand<TParam,TResult>`       | command batch        |
|   [4]   | `Interaction<TInput,TOutput>`                   | interaction source   |
|   [5]   | `IInteraction<TInput,TOutput>`                  | interaction contract |
|   [6]   | `InteractionContext<TInput,TOutput>`            | interaction context  |
|   [7]   | `UnhandledInteractionException<TInput,TOutput>` | interaction error    |

[ACTIVATION_AND_ROUTING_TYPES]: screen lifecycle contracts — rail: reactive

| [INDEX] | [SYMBOL]                | [KIND]                |
| :-----: | :---------------------- | :-------------------- |
|   [1]   | `IActivatableView`      | view activation       |
|   [2]   | `IActivatableViewModel` | view-model activation |
|   [3]   | `ViewModelActivator`    | activation owner      |
|   [4]   | `IViewFor`              | view binding          |
|   [5]   | `IViewFor<TViewModel>`  | typed view binding    |
|   [6]   | `IScreen`               | routing host          |
|   [7]   | `IRoutableViewModel`    | routed screen         |

[BUILDER_AND_REGISTRY_TYPES]: registration and binding infrastructure — rail: reactive

| [INDEX] | [SYMBOL]                       | [KIND]             |
| :-----: | :----------------------------- | :----------------- |
|   [1]   | `IReactiveUIBuilder`           | builder contract   |
|   [2]   | `ReactiveUIBuilder`            | builder runtime    |
|   [3]   | `RxAppBuilder`                 | builder root       |
|   [4]   | `IRegistrar`                   | service registry   |
|   [5]   | `CommandBinder`                | command binder     |
|   [6]   | `PropertyBinderImplementation` | property binder    |
|   [7]   | `BindingTypeConverterRegistry` | converter registry |

## [3]-[ENTRYPOINTS]

[STATE_ENTRYPOINTS]: property and derived-state operations
- rail: reactive

| [INDEX] | [SURFACE]                    | [SURFACE_ROOT]                       | [RAIL]            |
| :-----: | :--------------------------- | :----------------------------------- | :---------------- |
|   [1]   | `RaiseAndSetIfChanged`       | reactive object mixins               | property update   |
|   [2]   | `WhenAnyValue`               | change notification mixins           | property stream   |
|   [3]   | `ToProperty`                 | `OAPHCreationHelperMixin`            | derived property  |
|   [4]   | `ObservableToProperty`       | `OAPHCreationHelperMixin`            | derived property  |
|   [5]   | `ThrownExceptions`           | `IHandleObservableErrors`            | error stream      |
|   [6]   | `SubscribeToExpressionChain` | `ReactiveNotifyPropertyChangedMixin` | expression stream |

[COMMAND_ENTRYPOINTS]: command and command-binding operations
- rail: reactive

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]                        | [RAIL]          |
| :-----: | :--------------------- | :------------------------------------ | :-------------- |
|   [1]   | `Create`               | `ReactiveCommand`                     | command factory |
|   [2]   | `CreateFromTask`       | `ReactiveCommand`                     | task command    |
|   [3]   | `CreateFromObservable` | `ReactiveCommand`                     | stream command  |
|   [4]   | `CreateCombined`       | `ReactiveCommand`                     | command batch   |
|   [5]   | `CanExecute`           | `ReactiveCommandBase<TParam,TResult>` | command gate    |
|   [6]   | `IsExecuting`          | `ReactiveCommandBase<TParam,TResult>` | execution state |
|   [7]   | `BindCommand`          | `CommandBinder`                       | command binding |

[ACTIVATION_ENTRYPOINTS]: lifecycle operations
- rail: reactive

| [INDEX] | [SURFACE]                   | [SURFACE_ROOT]       | [RAIL]               |
| :-----: | :-------------------------- | :------------------- | :------------------- |
|   [1]   | `WhenActivated`             | `ViewForMixins`      | activation scope     |
|   [2]   | `HandleViewActivation`      | `ViewForMixins`      | view lifecycle       |
|   [3]   | `HandleViewModelActivation` | `ViewForMixins`      | view-model lifecycle |
|   [4]   | `Activate`                  | activation contracts | activation signal    |
|   [5]   | `Deactivate`                | activation contracts | deactivation signal  |

[INTERACTION_ENTRYPOINTS]: interaction and dialog operations
- rail: reactive

| [INDEX] | [SURFACE]         | [SURFACE_ROOT]                       | [RAIL]              |
| :-----: | :---------------- | :----------------------------------- | :------------------ |
|   [1]   | `RegisterHandler` | `Interaction<TInput,TOutput>`        | handler admission   |
|   [2]   | `Handle`          | `Interaction<TInput,TOutput>`        | interaction request |
|   [3]   | `SetOutput`       | `InteractionContext<TInput,TOutput>` | result projection   |
|   [4]   | `BindInteraction` | `InteractionBindingMixins`           | view binding        |
|   [5]   | `GetHandlers`     | `Interaction<TInput,TOutput>`        | handler lookup      |

[BUILDER_ENTRYPOINTS]: registration operations
- rail: reactive

| [INDEX] | [SURFACE]                             | [SURFACE_ROOT]       | [RAIL]              |
| :-----: | :------------------------------------ | :------------------- | :------------------ |
|   [1]   | `CreateReactiveUIBuilder`             | `RxAppBuilder`       | builder creation    |
|   [2]   | `RegisterView<TView,TModel>`          | `IReactiveUIBuilder` | view registration   |
|   [3]   | `RegisterSingletonView<TView,TModel>` | `IReactiveUIBuilder` | singleton view      |
|   [4]   | `RegisterViewModel<TModel>`           | `IReactiveUIBuilder` | model registration  |
|   [5]   | `RegisterStandardConverters`          | `ReactiveUIBuilder`  | converter admission |
|   [6]   | `WithExceptionHandler`                | `IReactiveUIBuilder` | error policy        |

## [4]-[IMPLEMENTATION_LAW]

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
