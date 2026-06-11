# [RASM_COMPUTE_API_RECYCLABLE_STREAM]

`Microsoft.IO.RecyclableMemoryStream` supplies pooled memory streams and allocation telemetry for payload staging.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IO.RecyclableMemoryStream`
- package: `Microsoft.IO.RecyclableMemoryStream`
- assembly: `Microsoft.IO.RecyclableMemoryStream`
- namespace: `Microsoft.IO`
- asset: runtime library
- rail: staging

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stream pool family
- rail: staging

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE] | [CAPABILITY]         |
| :-----: | :-------------------------------------- | :------------- | :------------------- |
|   [1]   | `RecyclableMemoryStreamManager`         | memory shape   | bounds payload shape |
|   [2]   | `RecyclableMemoryStream`                | memory shape   | bounds payload shape |
|   [3]   | `RecyclableMemoryStreamManager.Options` | policy object  | carries policy input |
|   [4]   | `RecyclableMemoryStreamManager.Events`  | memory shape   | bounds payload shape |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stream operations
- rail: staging

| [INDEX] | [SURFACE]               | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :---------------------- | :---------------- | :------------------------ |
|   [1]   | `GetStream`             | factory call      | creates configured handle |
|   [2]   | `GetReadOnlySequence`   | lookup call       | resolves typed value      |
|   [3]   | `ToArray`               | byte copy         | copies stream bytes       |
|   [4]   | `WriteTo`               | operation call    | executes operation        |
|   [5]   | `Dispose`               | operation call    | executes operation        |
|   [6]   | `GenerateCallStacks`    | diagnostic toggle | enables allocation traces |
|   [7]   | `MaximumStreamCapacity` | property surface  | binds surface state       |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Microsoft.IO.RecyclableMemoryStream`
- Owns: pooled stream buffers
- Accept: large payloads use stream pool policy
- Reject: unbounded MemoryStream allocation

