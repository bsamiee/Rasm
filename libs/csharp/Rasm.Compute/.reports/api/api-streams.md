# [RASM_COMPUTE_API_STREAMS]

Stream APIs supply pooled stream buffers for payload, model, remote, and artifact hot paths.

## [1]-[SURFACES]

This table is a lookup by stream package.

| [INDEX] | [PACKAGE]                            | [ASSEMBLY]                            | [LOCAL_RAIL] |
| :-----: | :----------------------------------- | :------------------------------------ | :----------- |
|   [1]   | `Microsoft.IO.RecyclableMemoryStream` | `Microsoft.IO.RecyclableMemoryStream` | streams      |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                            | [NAMESPACE]     | [USING]          | [API_LOCATOR] |
| :-----: | :------------------------------------ | :-------------- | :--------------- | :------------ |
|   [1]   | `Microsoft.IO.RecyclableMemoryStream` | `Microsoft.IO`  | `Microsoft.IO`   | `.cache/nuget/packages/microsoft.io.recyclablememorystream/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]                    | [ENTRY_SURFACE]          | [LOCAL_RAIL] |
| :-----: | :------------------------------- | :----------------------- | :----------- |
|   [1]   | `RecyclableMemoryStreamManager`  | stream pool owner        | streams      |
|   [2]   | recyclable streams               | pooled payload buffer    | streams      |
|   [3]   | stream diagnostics               | allocation receipt input | streams      |

## [4]-[REJECTED]

This table is a lookup by rejected API.

| [INDEX] | [REJECT]             | [LOCAL_RAIL] | [REASON]                 |
| :-----: | :------------------- | :----------- | :----------------------- |
|   [1]   | unowned `MemoryStream` pools | streams | stream pool owns lifetime |
|   [2]   | MessagePack in Compute | streams    | Persistence owns codecs  |
|   [3]   | MemoryPack in Compute | streams    | Persistence owns codecs  |
