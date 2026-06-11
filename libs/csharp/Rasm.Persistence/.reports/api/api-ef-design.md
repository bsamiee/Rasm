# [RASM_PERSISTENCE_API_EF_DESIGN]

`Microsoft.EntityFrameworkCore.Design` supplies design-time services, migrations scaffolding, compiled-model tooling, and tool-only EF build assets.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.EntityFrameworkCore.Design`
- package: `Microsoft.EntityFrameworkCore.Design`
- assembly: `Microsoft.EntityFrameworkCore.Design`
- namespace: `Microsoft.EntityFrameworkCore.Design`
- asset: design-time tool assets
- rail: schema

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: design-time family
- rail: schema

| [INDEX] | [ASSET]                                | [PACKAGE_ROLE]       | [CAPABILITY]           |
| :-----: | :------------------------------------- | :------------------- | :--------------------- |
|   [1]   | `Microsoft.EntityFrameworkCore.Design` | design-time assembly | supplies EF tooling    |
|   [2]   | design-time service graph              | tool service graph   | wires schema tooling   |
|   [3]   | migrations scaffolding assets          | generator asset      | emits migration source |
|   [4]   | compiled-model scaffolding assets      | generator asset      | emits compiled model   |
|   [5]   | reverse-engineer scaffolding assets    | generator asset      | emits reverse model    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: design-time operations
- rail: schema

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]      | [CAPABILITY]             |
| :-----: | :-------------------------- | :---------------- | :----------------------- |
|   [1]   | design-time context factory | tool input        | creates tooling context  |
|   [2]   | design-time services        | tool service hook | wires schema tooling     |
|   [3]   | migration scaffold output   | generated output  | emits migration source   |
|   [4]   | migration removal output    | generated output  | removes migration source |
|   [5]   | reverse-engineer output     | generated output  | emits reverse model      |
|   [6]   | compiled-model output       | generated output  | emits compiled model     |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Microsoft.EntityFrameworkCore.Design`
- Owns: design-time schema tooling
- Accept: design assets stay tool-only and source output enters the schema rail
- Reject: runtime dependency on design package
