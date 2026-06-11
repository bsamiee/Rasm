# [RASM_APPUI_API_REACTIVEUI_AVALONIA]

`ReactiveUI.Avalonia` supplies Avalonia view bases and activation bindings for ReactiveUI screens.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ReactiveUI.Avalonia`
- package: `ReactiveUI.Avalonia`
- assembly: `ReactiveUI.Avalonia`
- namespace: `ReactiveUI.Avalonia`
- asset: runtime library
- rail: reactive-ui

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: view family
- rail: reactive-ui

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]              |
| :-----: | :-------------------------------- | :--------------- | :------------------------ |
|   [1]   | `ReactiveUserControl<TViewModel>` | UI surface       | renders product surface   |
|   [2]   | `ReactiveWindow<TViewModel>`      | UI surface       | renders product surface   |
|   [3]   | `ReactiveView<TViewModel>`        | UI surface       | renders product surface   |
|   [4]   | `IViewFor<TViewModel>`            | contract surface | defines boundary contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: binding operations
- rail: reactive-ui

| [INDEX] | [SURFACE]       | [CALL_SHAPE]    | [CAPABILITY]                |
| :-----: | :-------------- | :-------------- | :-------------------------- |
|   [1]   | `WhenActivated` | member surface  | drives reactive-ui behavior |
|   [2]   | `Bind`          | mutation call   | admits configured surface   |
|   [3]   | `OneWayBind`    | member surface  | drives reactive-ui behavior |
|   [4]   | `BindCommand`   | command binding | binds command intent        |
|   [5]   | `DisposeWith`   | member surface  | drives reactive-ui behavior |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `ReactiveUI.Avalonia`
- Owns: Avalonia reactive binding
- Accept: bindings terminate at view activation
- Reject: undisposed subscriptions

