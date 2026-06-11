# [RASM_COMPUTE_API_HIGHPERFORMANCE]

`CommunityToolkit.HighPerformance` supplies span, memory, pooling, and buffer helpers for staging execution payloads.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CommunityToolkit.HighPerformance`
- package: `CommunityToolkit.HighPerformance`
- assembly: `CommunityToolkit.HighPerformance`
- namespace: `CommunityToolkit.HighPerformance`
- asset: runtime library
- rail: staging

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: memory family
- rail: staging

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]       | [CAPABILITY]             |
| :-----: | :------------------------- | :------------------- | :----------------------- |
|   [1]   | `Span2D<T>`                | memory shape         | bounds payload shape     |
|   [2]   | `ReadOnlySpan2D<T>`        | memory shape         | bounds payload shape     |
|   [3]   | `Memory2D<T>`              | memory shape         | bounds payload shape     |
|   [4]   | `ReadOnlyMemory2D<T>`      | memory shape         | bounds payload shape     |
|   [5]   | `MemoryOwner<T>`           | pooled memory owner  | bounds payload shape     |
|   [6]   | `ArrayPoolBufferWriter<T>` | pooled buffer writer | anchors staging contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: memory operations
- rail: staging

| [INDEX] | [SURFACE]               | [CALL_SHAPE]    | [CAPABILITY]            |
| :-----: | :---------------------- | :-------------- | :---------------------- |
|   [1]   | `Allocate`              | pool allocation | allocates pooled memory |
|   [2]   | `Span`                  | span property   | exposes stack span      |
|   [3]   | `Memory`                | memory property | exposes memory owner    |
|   [4]   | `DangerousGetReference` | ref accessor    | pins ref access         |
|   [5]   | `GetRowSpan`            | lookup call     | resolves typed value    |
|   [6]   | `AsBytes`               | byte projection | views raw bytes         |
|   [7]   | `Dispose`               | operation call  | executes operation      |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `CommunityToolkit.HighPerformance`
- Owns: staging memory shapes
- Accept: payloads use bounded memory surfaces
- Reject: custom span wrappers
