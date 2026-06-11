# [RASM_COMPUTE_API_STAGING]

Staging APIs supply span, memory, ref, and allocation-aware helpers for measured Compute hot paths.

## [1]-[SURFACES]

This table is a lookup by staging package.

| [INDEX] | [PACKAGE]                         | [ASSEMBLY]                         | [LOCAL_RAIL] |
| :-----: | :-------------------------------- | :--------------------------------- | :----------- |
|   [1]   | `CommunityToolkit.HighPerformance` | `CommunityToolkit.HighPerformance` | staging      |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                         | [NAMESPACE]                         | [USING]                              | [API_LOCATOR] |
| :-----: | :--------------------------------- | :---------------------------------- | :----------------------------------- | :------------ |
|   [1]   | `CommunityToolkit.HighPerformance` | `CommunityToolkit.HighPerformance`  | `CommunityToolkit.HighPerformance`   | `.cache/nuget/packages/communitytoolkit.highperformance/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]        | [ENTRY_SURFACE]            | [LOCAL_RAIL] |
| :-----: | :------------------- | :------------------------- | :----------- |
|   [1]   | span helpers         | allocation-aware staging   | staging      |
|   [2]   | memory helpers       | contiguous payload access  | staging      |
|   [3]   | ref helpers          | low-copy transforms        | staging      |
|   [4]   | pooling helpers      | measured allocation policy | staging      |

## [4]-[REJECTED]

This table is a lookup by rejected API.

| [INDEX] | [REJECT]         | [LOCAL_RAIL] | [REASON]                  |
| :-----: | :--------------- | :----------- | :------------------------ |
|   [1]   | helper wrappers  | staging      | package APIs are direct   |
|   [2]   | unmeasured pools | staging      | receipts own allocation   |
