# [RASM_APPUI_API_REACTIVEUI]

`ReactiveUI` supplies reactive view models, commands, activation, property change flow, and observable property helpers.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI`
- package: `ReactiveUI`
- assembly: `ReactiveUI`
- namespace: `ReactiveUI`
- asset: runtime library
- rail: reactive

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reactive family
- rail: reactive

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]              |
| :-----: | :-------------------------------- | :--------------- | :------------------------ |
|   [1]   | `ReactiveObject`                  | rail contract    | anchors reactive contract |
|   [2]   | `ReactiveCommand<TParam,TResult>` | result value     | projects receipt state    |
|   [3]   | `IReactiveObject`                 | contract surface | defines boundary contract |
|   [4]   | `IActivatableViewModel`           | contract surface | defines boundary contract |
|   [5]   | `ViewModelActivator`              | UI surface       | renders product surface   |
|   [6]   | `ObservableAsPropertyHelper<T>`   | rail contract    | anchors reactive contract |
|   [7]   | `Interaction<TInput,TOutput>`     | rail contract    | anchors reactive contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: reactive operations
- rail: reactive

| [INDEX] | [SURFACE]              | [CALL_SHAPE]   | [CAPABILITY]              |
| :-----: | :--------------------- | :------------- | :------------------------ |
|   [1]   | `RaiseAndSetIfChanged` | member surface | drives reactive behavior  |
|   [2]   | `WhenAnyValue`         | member surface | drives reactive behavior  |
|   [3]   | `ToProperty`           | member surface | drives reactive behavior  |
|   [4]   | `CreateFromTask`       | factory call   | creates configured handle |
|   [5]   | `Create`               | factory call   | creates configured handle |
|   [6]   | `ThrownExceptions`     | member surface | drives reactive behavior  |
|   [7]   | `WhenActivated`        | member surface | drives reactive behavior  |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `ReactiveUI`
- Owns: commands and reactive view models
- Accept: screen state is observable and disposable
- Reject: manual event fanout

