# [RASM_APPUI_API_GH2]

`Grasshopper2` supplies GH2 canvas, component, document, parameter, solve, and UI surfaces for AppUi host adapters.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Grasshopper2`
- host_assembly: `Grasshopper2`
- assembly: `Grasshopper2`
- namespace: `Grasshopper2`
- asset: host assembly
- rail: host-gh2

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: GH2 family
- rail: host-gh2

| [INDEX] | [SYMBOL]       | [PACKAGE_ROLE]    | [CAPABILITY]              |
| :-----: | :------------- | :---------------- | :------------------------ |
|   [1]   | `Document`     | rail contract     | anchors host-gh2 contract |
|   [2]   | `Canvas`       | drawing surface   | draws visual evidence     |
|   [3]   | `Component`    | rail contract     | anchors host-gh2 contract |
|   [4]   | `Parameter`    | rail contract     | anchors host-gh2 contract |
|   [5]   | `Wire`         | rail contract     | anchors host-gh2 contract |
|   [6]   | `Attributes`   | rail contract     | anchors host-gh2 contract |
|   [7]   | `SolveContext` | operation context | carries operation state   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GH2 operations
- rail: host-gh2

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]   | [CAPABILITY]              |
| :-----: | :-------------------------- | :------------- | :------------------------ |
|   [1]   | `ExpireSolution`            | member surface | drives host-gh2 behavior  |
|   [2]   | `ScheduleSolution`          | member surface | drives host-gh2 behavior  |
|   [3]   | `CreateAttributes`          | factory call   | creates configured handle |
|   [4]   | `Render`                    | rendering call | renders evidence          |
|   [5]   | `AppendAdditionalMenuItems` | member surface | drives host-gh2 behavior  |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Grasshopper2`
- Owns: GH2 host adapter surface
- Accept: GH2 APIs stay boundary-owned
- Reject: GH2 concepts as AppUi public types

