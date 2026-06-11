# [RASM_APPHOST_API_DATAFLOW]

`System.Threading.Tasks.Dataflow` supplies bounded runtime work channels for AppHost drain, backpressure, and ordered handoff.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Threading.Tasks.Dataflow`
- package: `System.Threading.Tasks.Dataflow`
- assembly: `System.Threading.Tasks.Dataflow`
- namespace: `System.Threading.Tasks.Dataflow`
- asset: runtime library
- rail: work-queue

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: block family
- rail: work-queue

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]   | [CAPABILITY]                |
| :-----: | :------------------------------- | :--------------- | :-------------------------- |
|   [1]   | `BufferBlock<T>`                 | rail contract    | anchors work-queue contract |
|   [2]   | `ActionBlock<T>`                 | rail contract    | anchors work-queue contract |
|   [3]   | `TransformBlock<TInput,TOutput>` | rail contract    | anchors work-queue contract |
|   [4]   | `BroadcastBlock<T>`              | rail contract    | anchors work-queue contract |
|   [5]   | `DataflowBlockOptions`           | policy object    | carries policy input        |
|   [6]   | `ExecutionDataflowBlockOptions`  | policy object    | carries policy input        |
|   [7]   | `DataflowLinkOptions`            | policy object    | carries policy input        |
|   [8]   | `ISourceBlock<T>`                | contract surface | defines boundary contract   |
|   [9]   | `ITargetBlock<T>`                | contract surface | defines boundary contract   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: block operations
- rail: work-queue

| [INDEX] | [SURFACE]              | [CALL_SHAPE]      | [CAPABILITY]             |
| :-----: | :--------------------- | :---------------- | :----------------------- |
|   [1]   | `Post`                 | operation call    | executes operation       |
|   [2]   | `SendAsync`            | async operation   | executes async work      |
|   [3]   | `LinkTo`               | block link method | connects dataflow blocks |
|   [4]   | `Complete`             | operation call    | executes operation       |
|   [5]   | `Completion`           | completion task   | awaits drain state       |
|   [6]   | `Fault`                | operation call    | executes operation       |
|   [7]   | `TryReceive`           | lookup call       | resolves typed value     |
|   [8]   | `OutputAvailableAsync` | readiness task    | awaits source readiness  |

## [4]-[IMPLEMENTATION_LAW]

[BLOCK_TOPOLOGY]:
- namespaces: `System.Threading.Tasks.Dataflow`
- producer contracts: `ITargetBlock<T>`, `ISourceBlock<T>`, `IPropagatorBlock<TInput,TOutput>`
- block families: buffer, action, transform, broadcast, batch, join, write-once
- policy knobs: bounded capacity, cancellation token, scheduler, max degree, ordered output
- lifecycle: accept, complete, fault, propagate completion, await completion task

[LOCAL_ADMISSION]:
- Runtime background work enters bounded blocks with explicit backpressure.
- Drain waits on `Completion` and faults the AppHost receipt rail on block failure.
- Dataflow blocks stay internal implementation material; AppHost public ports expose runtime intent and receipts.

[RAIL_LAW]:
- Package: `System.Threading.Tasks.Dataflow`
- Owns: bounded queues and drainable blocks
- Accept: runtime work enters dataflow blocks
- Reject: unbounded local queues

