# [RASM_APPHOST_API]

This folder carries the AppHost package API catalogue. It maps admitted runtime and companion packages to assemblies, namespaces, usings, type families, operation families, and local rails.

## [1]-[SURFACES]

This table routes catalogue pages by API family.

| [INDEX] | [READ_FOR]              | [OPEN]              |
| :-----: | :---------------------- | :------------------ |
|   [1]   | runtime primitives      | [runtime](api-runtime.md) |
|   [2]   | functional substrate    | [functional](api-functional.md) |
|   [3]   | companion bootstrap     | [companion](api-companion.md) |

## [2]-[API_LOCATORS]

This table is a lookup by locator family.

| [INDEX] | [FAMILY]    | [API_LOCATOR]                         | [LOCAL_RAIL] |
| :-----: | :---------- | :------------------------------------ | :----------- |
|   [1]   | NuGet       | `.cache/nuget/packages/<package>/`    | package      |
|   [2]   | BCL         | shared framework reference assemblies | runtime      |
|   [3]   | Decompile   | `tools.assay api query` or `ilspycmd` | inspection   |

## [3]-[CAPABILITIES]

This table maps catalogue pages to package law.

| [INDEX] | [PAGE]        | [CAPABILITY]                       |
| :-----: | :------------ | :--------------------------------- |
|   [1]   | runtime       | time, logging, diagnostics, flow   |
|   [2]   | functional    | effects, results, generated shapes |
|   [3]   | companion     | host, options, health, export      |

## [4]-[REJECTED]

No rejected package belongs to this index. Rejected packages appear on the catalogue page that owns the rail.
