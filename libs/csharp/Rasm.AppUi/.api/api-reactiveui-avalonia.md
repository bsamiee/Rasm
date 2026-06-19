# [RASM_APPUI_API_REACTIVEUI_AVALONIA]

`ReactiveUI.Avalonia` supplies Avalonia view bases, builder registration, activation binding, command binding, property observation, routing, and scheduling.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI.Avalonia`
- package: `ReactiveUI.Avalonia`
- assembly: `ReactiveUI.Avalonia`
- namespace: `ReactiveUI.Avalonia`
- asset: runtime library
- rail: reactive-ui

## [02]-[PUBLIC_TYPES]

[VIEW_TYPES]: typed view and host controls — rail: reactive-ui

| [INDEX] | [SYMBOL]                          | [KIND]          |
| :-----: | :-------------------------------- | :-------------- |
|  [01]   | `ReactiveUserControl<TViewModel>` | screen control  |
|  [02]   | `ReactiveWindow<TViewModel>`      | window surface  |
|  [03]   | `RoutedViewHost`                  | routed host     |
|  [04]   | `ViewModelViewHost`               | view-model host |

[INFRASTRUCTURE_TYPES]: Avalonia integration infrastructure — rail: reactive-ui

| [INDEX] | [SYMBOL]                              | [KIND]               |
| :-----: | :------------------------------------ | :------------------- |
|  [01]   | `AppBuilderExtensions`                | builder admission    |
|  [02]   | `AvaloniaActivationForViewFetcher`    | activation lookup    |
|  [03]   | `AvaloniaCreatesCommandBinding`       | command binding      |
|  [04]   | `AvaloniaObjectObservableForProperty` | property observation |
|  [05]   | `AvaloniaObjectReactiveExtensions`    | property subjects    |
|  [06]   | `AvaloniaScheduler`                   | UI scheduler         |
|  [07]   | `AutoSuspendHelper`                   | lifetime suspension  |
|  [08]   | `AutoDataTemplateBindingHook`         | template binding     |

## [03]-[ENTRYPOINTS]

[BUILDER_ENTRYPOINTS]: builder and registration operations
- rail: reactive-ui

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT]         | [RAIL]            |
| :-----: | :----------------------------------------- | :--------------------- | :---------------- |
|  [01]   | `UseReactiveUI`                            | `AppBuilderExtensions` | builder admission |
|  [02]   | `RegisterReactiveUIViews`                  | `AppBuilderExtensions` | view registration |
|  [03]   | `RegisterReactiveUIViewsFromAssemblyOf<T>` | `AppBuilderExtensions` | assembly scan     |
|  [04]   | `RegisterReactiveUIViewsFromEntryAssembly` | `AppBuilderExtensions` | entry assembly    |
|  [05]   | `UseReactiveUIWithDIContainer<T>`          | `AppBuilderExtensions` | DI admission      |
|  [06]   | `WithAvalonia`                             | `AppBuilderExtensions` | builder bridge    |

[BINDING_ENTRYPOINTS]: Avalonia reactive binding operations
- rail: reactive-ui

| [INDEX] | [SURFACE]                    | [SURFACE_ROOT]                        | [RAIL]              |
| :-----: | :--------------------------- | :------------------------------------ | :------------------ |
|  [01]   | `GetActivationForView`       | `AvaloniaActivationForViewFetcher`    | activation lookup   |
|  [02]   | `GetAffinityForView`         | `AvaloniaActivationForViewFetcher`    | activation affinity |
|  [03]   | `BindCommandToObject`        | `AvaloniaCreatesCommandBinding`       | command binding     |
|  [04]   | `GetAffinityForObject`       | `AvaloniaCreatesCommandBinding`       | binding affinity    |
|  [05]   | `GetNotificationForProperty` | `AvaloniaObjectObservableForProperty` | property stream     |
|  [06]   | `GetAffinityForObject`       | `AvaloniaObjectObservableForProperty` | property affinity   |
|  [07]   | `GetSubject`                 | `AvaloniaObjectReactiveExtensions`    | property subject    |
|  [08]   | `GetBindingSubject`          | `AvaloniaObjectReactiveExtensions`    | binding subject     |

[VIEW_HOST_ENTRYPOINTS]: view host operations
- rail: reactive-ui

| [INDEX] | [SURFACE]        | [SURFACE_ROOT]                    | [RAIL]        |
| :-----: | :--------------- | :-------------------------------- | :------------ |
|  [01]   | `ViewModel`      | `ReactiveUserControl<TViewModel>` | view model    |
|  [02]   | `ViewModel`      | `ReactiveWindow<TViewModel>`      | view model    |
|  [03]   | `Router`         | `RoutedViewHost`                  | router source |
|  [04]   | `ViewContract`   | `RoutedViewHost`                  | view contract |
|  [05]   | `DefaultContent` | `RoutedViewHost`                  | fallback view |
|  [06]   | `ViewModel`      | `ViewModelViewHost`               | model view    |
|  [07]   | `DefaultContent` | `ViewModelViewHost`               | fallback view |
|  [08]   | `Schedule`       | `AvaloniaScheduler`               | UI scheduling |

## [04]-[IMPLEMENTATION_LAW]

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
