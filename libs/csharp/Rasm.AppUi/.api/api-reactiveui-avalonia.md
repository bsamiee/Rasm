# [RASM_APPUI_API_REACTIVEUI_AVALONIA]

`ReactiveUI.Avalonia` supplies Avalonia view bases, builder registration, activation binding, command binding, property observation, routing, and scheduling.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI.Avalonia`
- package: `ReactiveUI.Avalonia`
- assembly: `ReactiveUI.Avalonia`
- namespace: `ReactiveUI.Avalonia`
- asset: runtime library
- rail: reactive-ui

## [2]-[PUBLIC_TYPES]

[VIEW_TYPES]: typed view and host controls — rail: reactive-ui

| [INDEX] | [SYMBOL]                          | [KIND]          |
| :-----: | :-------------------------------- | :-------------- |
|   [1]   | `ReactiveUserControl<TViewModel>` | screen control  |
|   [2]   | `ReactiveWindow<TViewModel>`      | window surface  |
|   [3]   | `RoutedViewHost`                  | routed host     |
|   [4]   | `ViewModelViewHost`               | view-model host |

[INFRASTRUCTURE_TYPES]: Avalonia integration infrastructure — rail: reactive-ui

| [INDEX] | [SYMBOL]                              | [KIND]               |
| :-----: | :------------------------------------ | :------------------- |
|   [1]   | `AppBuilderExtensions`                | builder admission    |
|   [2]   | `AvaloniaActivationForViewFetcher`    | activation lookup    |
|   [3]   | `AvaloniaCreatesCommandBinding`       | command binding      |
|   [4]   | `AvaloniaObjectObservableForProperty` | property observation |
|   [5]   | `AvaloniaObjectReactiveExtensions`    | property subjects    |
|   [6]   | `AvaloniaScheduler`                   | UI scheduler         |
|   [7]   | `AutoSuspendHelper`                   | lifetime suspension  |
|   [8]   | `AutoDataTemplateBindingHook`         | template binding     |

## [3]-[ENTRYPOINTS]

[BUILDER_ENTRYPOINTS]: builder and registration operations
- rail: reactive-ui

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT]         | [RAIL]            |
| :-----: | :----------------------------------------- | :--------------------- | :---------------- |
|   [1]   | `UseReactiveUI`                            | `AppBuilderExtensions` | builder admission |
|   [2]   | `RegisterReactiveUIViews`                  | `AppBuilderExtensions` | view registration |
|   [3]   | `RegisterReactiveUIViewsFromAssemblyOf<T>` | `AppBuilderExtensions` | assembly scan     |
|   [4]   | `RegisterReactiveUIViewsFromEntryAssembly` | `AppBuilderExtensions` | entry assembly    |
|   [5]   | `UseReactiveUIWithDIContainer<T>`          | `AppBuilderExtensions` | DI admission      |
|   [6]   | `WithAvalonia`                             | `AppBuilderExtensions` | builder bridge    |

[BINDING_ENTRYPOINTS]: Avalonia reactive binding operations
- rail: reactive-ui

| [INDEX] | [SURFACE]                    | [SURFACE_ROOT]                        | [RAIL]              |
| :-----: | :--------------------------- | :------------------------------------ | :------------------ |
|   [1]   | `GetActivationForView`       | `AvaloniaActivationForViewFetcher`    | activation lookup   |
|   [2]   | `GetAffinityForView`         | `AvaloniaActivationForViewFetcher`    | activation affinity |
|   [3]   | `BindCommandToObject`        | `AvaloniaCreatesCommandBinding`       | command binding     |
|   [4]   | `GetAffinityForObject`       | `AvaloniaCreatesCommandBinding`       | binding affinity    |
|   [5]   | `GetNotificationForProperty` | `AvaloniaObjectObservableForProperty` | property stream     |
|   [6]   | `GetAffinityForObject`       | `AvaloniaObjectObservableForProperty` | property affinity   |
|   [7]   | `GetSubject`                 | `AvaloniaObjectReactiveExtensions`    | property subject    |
|   [8]   | `GetBindingSubject`          | `AvaloniaObjectReactiveExtensions`    | binding subject     |

[VIEW_HOST_ENTRYPOINTS]: view host operations
- rail: reactive-ui

| [INDEX] | [SURFACE]        | [SURFACE_ROOT]                    | [RAIL]        |
| :-----: | :--------------- | :-------------------------------- | :------------ |
|   [1]   | `ViewModel`      | `ReactiveUserControl<TViewModel>` | view model    |
|   [2]   | `ViewModel`      | `ReactiveWindow<TViewModel>`      | view model    |
|   [3]   | `Router`         | `RoutedViewHost`                  | router source |
|   [4]   | `ViewContract`   | `RoutedViewHost`                  | view contract |
|   [5]   | `DefaultContent` | `RoutedViewHost`                  | fallback view |
|   [6]   | `ViewModel`      | `ViewModelViewHost`               | model view    |
|   [7]   | `DefaultContent` | `ViewModelViewHost`               | fallback view |
|   [8]   | `Schedule`       | `AvaloniaScheduler`               | UI scheduling |

## [4]-[IMPLEMENTATION_LAW]

[REACTIVE_AVALONIA_LAW]:
- Package: `ReactiveUI.Avalonia`
- Owns: Avalonia builder admission, activation, command binding, property observation, routing, and UI scheduling
- Accept: bindings terminate at activated Avalonia views and typed view hosts
- Reject: undisposed subscriptions

[HOST_LAW]:
- Package: `ReactiveUI.Avalonia`
- Owns: common reactive binding rail for panels, companion windows, sidecars, diagnostics, and downstream shells
- Accept: routed screens and direct view-model hosts share one binding contract
- Reject: per-host reactive binding stacks
