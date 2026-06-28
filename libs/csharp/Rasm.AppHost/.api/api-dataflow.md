# [RASM_APPHOST_API_DATAFLOW]

`System.Threading.Tasks.Dataflow` supplies bounded runtime blocks, execution blocks,
propagator blocks, grouping blocks, reservation contracts, backpressure, completion,
and ordered handoff for AppHost drain.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Threading.Tasks.Dataflow`
- package: `System.Threading.Tasks.Dataflow`
- assembly: `System.Threading.Tasks.Dataflow`
- namespace: `System.Threading.Tasks.Dataflow`
- asset: runtime library
- rail: work-queue

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: block family
- rail: work-queue

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]     | [CAPABILITY]          |
| :-----: | :----------------------------------- | :----------------- | :-------------------- |
|  [01]   | `BufferBlock<T>`                     | buffer block       | queues messages       |
|  [02]   | `ActionBlock<T>`                     | execution block    | consumes messages     |
|  [03]   | `TransformBlock<TInput,TOutput>`     | propagator block   | transforms messages   |
|  [04]   | `TransformManyBlock<TInput,TOutput>` | propagator block   | expands messages      |
|  [05]   | `BroadcastBlock<T>`                  | broadcast block    | fans out latest value |
|  [06]   | `BatchBlock<T>`                      | grouping block     | batches messages      |
|  [07]   | `JoinBlock<T1,T2>`                   | grouping block     | joins two streams     |
|  [08]   | `BatchedJoinBlock<T1,T2>`            | grouping block     | coalesces two streams |
|  [09]   | `WriteOnceBlock<T>`                  | single-value block | publishes one value   |
|  [10]   | `IDataflowBlock`                     | lifecycle contract | owns completion       |
|  [11]   | `IPropagatorBlock<TInput,TOutput>`   | block contract     | consumes and produces |
|  [12]   | `IReceivableSourceBlock<T>`          | receive contract   | supports pull receive |

[PUBLIC_TYPE_SCOPE]: block-options family
- rail: work-queue

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]                                                                                        |
| :-----: | :------------------------------ | :----------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | `DataflowBlockOptions`          | base block options | `BoundedCapacity` (`const Unbounded = -1`), `CancellationToken`, `EnsureOrdered`, `TaskScheduler`, `MaxMessagesPerTask`, `NameFormat` |
|  [02]   | `ExecutionDataflowBlockOptions` | execution options  | `: DataflowBlockOptions` + `MaxDegreeOfParallelism`, `SingleProducerConstrained` (action / transform blocks) |
|  [03]   | `GroupingDataflowBlockOptions`  | grouping options   | `: DataflowBlockOptions` + `Greedy`, `MaxNumberOfGroups` (batch / join / batched-join blocks)        |
|  [04]   | `DataflowLinkOptions`           | link options       | `PropagateCompletion`, `MaxMessages`, `Append` — passed to `LinkTo`                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: block operations
- rail: work-queue

| [INDEX] | [SURFACE]              | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :--------------------- | :---------------- | :------------------------ |
|  [01]   | `Post`                 | synchronous offer | offers message            |
|  [02]   | `SendAsync`            | async offer       | awaits backpressure       |
|  [03]   | `LinkTo`               | link operation    | connects dataflow blocks  |
|  [04]   | `Complete`             | completion signal | stops input               |
|  [05]   | `Completion`           | completion task   | awaits drain state        |
|  [06]   | `Fault`                | fault signal      | faults block              |
|  [07]   | `TryReceive`           | pull receive      | reads available message   |
|  [08]   | `TryReceiveAll`        | pull drain        | drains available messages |
|  [09]   | `OutputAvailableAsync` | readiness task    | awaits source readiness   |
|  [10]   | `TriggerBatch`         | batch trigger     | forces batch emission     |
|  [11]   | `Encapsulate`          | block composition | wraps target and source   |
|  [12]   | `Target1`/`Target2`    | join arm targets  | feed join/coalesce arms   |

## [04]-[IMPLEMENTATION_LAW]

[BLOCK_TOPOLOGY]:
- namespaces: `System.Threading.Tasks.Dataflow`
- producer contracts: `ITargetBlock<T>`, `ISourceBlock<T>`, `IPropagatorBlock<TInput,TOutput>`
- block families: buffer, action, transform, broadcast, batch, join, write-once
- policy knobs: bounded capacity, cancellation token, scheduler, max degree, ordered output, greedy grouping
- option types: `DataflowBlockOptions` (base) → `ExecutionDataflowBlockOptions` (`MaxDegreeOfParallelism` for action/transform) and `GroupingDataflowBlockOptions` (`Greedy`/`MaxNumberOfGroups` for batch/join); `DataflowLinkOptions.PropagateCompletion` fans completion across every `LinkTo`
- lifecycle: accept, complete, fault, propagate completion, await completion task

[MESSAGE_TOPOLOGY]:
- message identity: `DataflowMessageHeader`
- offer result: `DataflowMessageStatus`
- reservation rail: reserve, consume, release
- observer bridge: `AsObservable`, `AsObserver`
- null target: `NullTarget<T>` absorbs messages explicitly

[LOCAL_ADMISSION]:
- Runtime background work enters bounded blocks with explicit backpressure.
- Drain waits on `Completion` and faults the AppHost receipt rail on block failure.
- Dataflow blocks stay internal implementation material; AppHost public ports expose runtime intent and receipts.
- Grouping blocks are admitted only when batch or join semantics are part of the runtime receipt.

[RAIL_LAW]:
- Package: `System.Threading.Tasks.Dataflow`
- Owns: bounded queues and drainable blocks
- Accept: runtime work enters dataflow blocks
- Reject: unbounded local queues
