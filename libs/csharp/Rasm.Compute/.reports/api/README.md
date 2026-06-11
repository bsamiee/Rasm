# [RASM_COMPUTE_API]

This folder carries the Compute package API catalogue. It maps tensor, model, remote, units, staging, and stream APIs to one measured execution rail.

## [1]-[SURFACES]

This table routes catalogue pages by API family.

| [INDEX] | [READ_FOR]          | [OPEN]              |
| :-----: | :------------------ | :------------------ |
|   [1]   | tensor primitives   | [tensor](api-tensor.md) |
|   [2]   | model execution     | [model](api-model.md) |
|   [3]   | remote execution    | [remote](api-remote.md) |
|   [4]   | unit boundaries     | [units](api-units.md) |
|   [5]   | staging memory      | [staging](api-staging.md) |
|   [6]   | pooled streams      | [streams](api-streams.md) |

## [2]-[API_LOCATORS]

This table is a lookup by locator family.

| [INDEX] | [FAMILY]  | [API_LOCATOR]                         | [LOCAL_RAIL] |
| :-----: | :-------- | :------------------------------------ | :----------- |
|   [1]   | NuGet     | `.cache/nuget/packages/<package>/`    | package      |
|   [2]   | Decompile | `tools.assay api query` or `ilspycmd` | inspection   |

## [3]-[CAPABILITIES]

This table maps catalogue pages to execution rails.

| [INDEX] | [PAGE]   | [CAPABILITY]                 |
| :-----: | :------- | :--------------------------- |
|   [1]   | tensor   | tensor operations            |
|   [2]   | model    | ONNX Runtime inference       |
|   [3]   | remote   | gRPC and protobuf contracts  |
|   [4]   | units    | physical-unit boundaries     |
|   [5]   | staging  | span and memory helpers      |
|   [6]   | streams  | recyclable stream pooling    |

## [4]-[REJECTED]

Rejected execution packages appear on the catalogue page that owns the rail.
