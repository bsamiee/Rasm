# [RASM_APPUI_API_AVALONIA]

`Avalonia` supplies retained UI primitives for shell, screen, command, theme, resource, input, and visual state.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia`
- package: `Avalonia`
- assembly: `Avalonia.Base`
- namespace: `Avalonia`
- asset: runtime library
- rail: retained-ui

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: UI type family
- rail: retained-ui

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]     | [CAPABILITY]                 |
| :-----: | :------------------- | :----------------- | :--------------------------- |
|   [1]   | `Application`        | application root   | starts UI shell              |
|   [2]   | `StyledElement`      | style participant  | binds retained state         |
|   [3]   | `Visual`             | visual tree node   | anchors retained-ui contract |
|   [4]   | `Control`            | UI surface         | renders product surface      |
|   [5]   | `TopLevel`           | top-level host     | starts UI shell              |
|   [6]   | `Window`             | UI surface         | renders product surface      |
|   [7]   | `UserControl`        | UI surface         | renders product surface      |
|   [8]   | `Panel`              | UI surface         | renders product surface      |
|   [9]   | `AvaloniaProperty`   | property identity  | binds retained state         |
|  [10]   | `StyledProperty<T>`  | styled property    | binds retained state         |
|  [11]   | `DirectProperty<T>`  | direct property    | binds retained state         |
|  [12]   | `Binding`            | binding expression | binds retained state         |
|  [13]   | `Styles`             | style collection   | binds retained state         |
|  [14]   | `ResourceDictionary` | resource scope     | binds retained state         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: UI operations
- rail: retained-ui

| [INDEX] | [SURFACE]          | [CALL_SHAPE]    | [CAPABILITY]              |
| :-----: | :----------------- | :-------------- | :------------------------ |
|   [1]   | `Register`         | mutation call   | admits configured surface |
|   [2]   | `RegisterDirect`   | mutation call   | admits configured surface |
|   [3]   | `Bind`             | mutation call   | admits configured surface |
|   [4]   | `GetObservable`    | lookup call     | resolves typed value      |
|   [5]   | `FindResource`     | lookup call     | resolves typed value      |
|   [6]   | `InvalidateVisual` | rendering call  | renders evidence          |
|   [7]   | `Focus`            | focus method    | moves keyboard focus      |
|   [8]   | `AttachDevTools`   | diagnostic hook | opens diagnostics         |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Avalonia`
- Owns: retained UI object model
- Accept: product UI concepts map to controls
- Reject: public Avalonia vocabulary

