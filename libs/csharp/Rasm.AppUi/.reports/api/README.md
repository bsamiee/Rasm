# [RASM_APPUI_API]

This folder carries the AppUi package API catalogue. It maps retained UI, reactive/live, visual/asset, controls/dialogs, and host-boundary APIs to local UI rails.

## [1]-[SURFACES]

This table routes catalogue pages by API family.

| [INDEX] | [READ_FOR]              | [OPEN]              |
| :-----: | :---------------------- | :------------------ |
|   [1]   | retained UI             | [retained](api-retained.md) |
|   [2]   | reactive live state     | [reactive](api-reactive.md) |
|   [3]   | visuals and assets      | [visuals](api-visuals.md) |
|   [4]   | controls and dialogs    | [controls](api-controls.md) |
|   [5]   | host boundary           | [host](api-host.md) |

## [2]-[API_LOCATORS]

This table is a lookup by locator family.

| [INDEX] | [FAMILY]    | [API_LOCATOR]                         | [LOCAL_RAIL] |
| :-----: | :---------- | :------------------------------------ | :----------- |
|   [1]   | NuGet       | `.cache/nuget/packages/<package>/`    | package      |
|   [2]   | Host        | RhinoWIP resource assemblies          | host         |
|   [3]   | Decompile   | `tools.assay api query` or `ilspycmd` | inspection   |

## [3]-[CAPABILITIES]

This table maps catalogue pages to UI rails.

| [INDEX] | [PAGE]     | [CAPABILITY]                      |
| :-----: | :--------- | :-------------------------------- |
|   [1]   | retained   | shell, controls, platform, theme  |
|   [2]   | reactive   | activation, commands, validation  |
|   [3]   | visuals    | charts, drawing, SVG, typography  |
|   [4]   | controls   | inspectors, dialogs, behaviors    |
|   [5]   | host       | Rhino/GH/Eto/macOS boundaries     |

## [4]-[REJECTED]

Rejected UI packages appear on the catalogue page that owns the rail.
