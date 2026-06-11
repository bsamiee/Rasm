# [RASM_COMPUTE_API_TENSOR]

Tensor APIs supply measured tensor execution over the Compute substrate-selection rail.

## [1]-[SURFACES]

This table is a lookup by tensor package.

| [INDEX] | [PACKAGE]                 | [ASSEMBLY]                 | [LOCAL_RAIL] |
| :-----: | :------------------------ | :------------------------- | :----------- |
|   [1]   | `System.Numerics.Tensors` | `System.Numerics.Tensors`  | tensor       |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                | [NAMESPACE]              | [USING]                   | [API_LOCATOR] |
| :-----: | :------------------------ | :----------------------- | :------------------------ | :------------ |
|   [1]   | `System.Numerics.Tensors` | `System.Numerics.Tensors` | `System.Numerics.Tensors` | `.cache/nuget/packages/system.numerics.tensors/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]       | [ENTRY_SURFACE]          | [LOCAL_RAIL] |
| :-----: | :------------------ | :----------------------- | :----------- |
|   [1]   | tensor primitives   | dense numeric operations | tensor       |
|   [2]   | tensor spans        | allocation-aware input   | tensor       |
|   [3]   | tensor operators    | measured substrate call  | tensor       |

## [4]-[REJECTED]

This table is a lookup by rejected package.

| [INDEX] | [REJECT]               | [LOCAL_RAIL] | [REASON]                 |
| :-----: | :--------------------- | :----------- | :----------------------- |
|   [1]   | PLINQ tensor lane      | tensor       | substrate rail owns parallelism |
|   [2]   | DirectML/GPU packages  | tensor       | no direct GPU ownership  |
|   [3]   | ComputeSharp/Metal     | tensor       | no direct Metal compute  |
