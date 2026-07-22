# [RASM_APPHOST_API_DATAFLOW]

`System.Threading.Tasks.Dataflow` owns bounded in-process message blocks for AppHost background work: a block drains under a `BoundedCapacity` bound with `SendAsync` backpressure, `LinkTo` wires a block graph that fans completion end to end, and the `Completion` task gates drain while surfacing faults. Broadcast, batch, and join blocks feed the drainable-queue and event-bus rails; every block stays internal, surfaced only as runtime intent on the public ports.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Threading.Tasks.Dataflow`
- package: `System.Threading.Tasks.Dataflow`
- assembly: `System.Threading.Tasks.Dataflow`
- namespace: `System.Threading.Tasks.Dataflow`
- asset: runtime library
- rail: work-queue

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: block family

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :----------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `BufferBlock<T>`                     | class         | buffers and re-offers messages FIFO  |
|  [02]   | `ActionBlock<T>`                     | class         | consumes each message via a delegate |
|  [03]   | `TransformBlock<TInput,TOutput>`     | class         | maps each input to one output        |
|  [04]   | `TransformManyBlock<TInput,TOutput>` | class         | expands each input to many outputs   |
|  [05]   | `BroadcastBlock<T>`                  | class         | fans latest value to all targets     |
|  [06]   | `BatchBlock<T>`                      | class         | groups messages into sized batches   |
|  [07]   | `JoinBlock<T1,T2>`                   | class         | pairs two streams into tuples        |
|  [08]   | `BatchedJoinBlock<T1,T2>`            | class         | coalesces two streams into batches   |
|  [09]   | `WriteOnceBlock<T>`                  | class         | publishes one immutable value        |
|  [10]   | `ITargetBlock<T>`                    | interface     | message consumer contract            |
|  [11]   | `ISourceBlock<T>`                    | interface     | message producer contract            |
|  [12]   | `IPropagatorBlock<TInput,TOutput>`   | interface     | consumer and producer in one         |
|  [13]   | `IReceivableSourceBlock<T>`          | interface     | source with pull receive             |
|  [14]   | `IDataflowBlock`                     | interface     | completion and fault lifecycle       |

[PUBLIC_TYPE_SCOPE]: block-options family

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :------------------------------ | :------------ | :------------------------------ |
|  [01]   | `DataflowBlockOptions`          | class         | base block-tuning knobs         |
|  [02]   | `ExecutionDataflowBlockOptions` | class         | action and transform execution  |
|  [03]   | `GroupingDataflowBlockOptions`  | class         | batch and join grouping         |
|  [04]   | `DataflowLinkOptions`           | class         | `LinkTo` propagation and bounds |

[OPTION_KNOBS]:
- `DataflowBlockOptions`: `BoundedCapacity` (`Unbounded = -1`) `CancellationToken` `EnsureOrdered` `TaskScheduler` `MaxMessagesPerTask` `NameFormat`
- `ExecutionDataflowBlockOptions` adds: `MaxDegreeOfParallelism` `SingleProducerConstrained`
- `GroupingDataflowBlockOptions` adds: `Greedy` `MaxNumberOfGroups`
- `DataflowLinkOptions`: `PropagateCompletion` `MaxMessages` `Append`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: block operations

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `DataflowBlock.Post(ITargetBlock<T>, T)`                | static   | offers a message without blocking  |
|  [02]   | `DataflowBlock.SendAsync(ITargetBlock<T>, T)`           | static   | awaits backpressure when full      |
|  [03]   | `ISourceBlock<T>.LinkTo(target, DataflowLinkOptions)`   | instance | wires a source into a target       |
|  [04]   | `IDataflowBlock.Complete()`                             | instance | signals end of input               |
|  [05]   | `IDataflowBlock.Completion`                             | property | drain and fault task to await      |
|  [06]   | `IDataflowBlock.Fault(Exception)`                       | instance | faults the block                   |
|  [07]   | `IReceivableSourceBlock<T>.TryReceive(out T)`           | instance | pulls one available message        |
|  [08]   | `IReceivableSourceBlock<T>.TryReceiveAll(out IList<T>)` | instance | drains available messages          |
|  [09]   | `DataflowBlock.OutputAvailableAsync(ISourceBlock<T>)`   | static   | awaits source readiness            |
|  [10]   | `BatchBlock<T>.TriggerBatch()`                          | instance | forces a partial batch             |
|  [11]   | `DataflowBlock.Encapsulate(target, source)`             | static   | wraps a block pair as a propagator |
|  [12]   | `JoinBlock<T1,T2>.Target1`                              | property | feeds the first join arm           |
|  [13]   | `JoinBlock<T1,T2>.Target2`                              | property | feeds the second join arm          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every block is `ITargetBlock<T>`, `ISourceBlock<T>`, or both through `IPropagatorBlock<TInput,TOutput>`; `LinkTo` wires a source into a target.
- A bounded block back-pressures at `BoundedCapacity`: `SendAsync` awaits a slot, `Post` refuses when full, `Unbounded = -1` opts out.
- Completion flows down a link only under `DataflowLinkOptions.PropagateCompletion`; `Completion` faults when the block faults.
- A message offer resolves to `DataflowMessageStatus` under a `DataflowMessageHeader` identity; a source reservation runs reserve, consume, release.
- `NullTarget<T>` absorbs and discards a message; `AsObservable` and `AsObserver` bridge a block to Rx.

[STACKING]:
- `Runtime/resources.md`: `DrainQueue<T>.Network(DrainSpec, ITargetBlock<T> Intake, IDataflowBlock Tail)` wraps every completion-propagating block graph, and `DrainSurface.Broadcast`/`.Join`/`.Coalesce` mint `BroadcastBlock`/`JoinBlock`/`BatchedJoinBlock` while `.NetworkOptions`/`.BroadcastOptions`/`.GroupingOptions`/`.LinkOptions` project each `DrainSpec` row onto its options record; the `Pipe` case rides `Channel<T>` instead.
- `Wire/topics.md`: `Topic<T>` and `Subscription` compose the `Runtime/resources` `DrainSurface` builders — `BroadcastBlock` topic fan, bounded `BufferBlock` into `ActionBlock` subscription, `JoinBlock` correlate, `BatchedJoinBlock` coalesce — never a raw block.

[LOCAL_ADMISSION]:
- Background work enters bounded blocks; drain awaits `Completion` and faults the AppHost receipt rail on block failure.
- Blocks stay internal implementation; public ports expose runtime intent and receipts, never a block.
- Grouping blocks enter only when batch or join semantics ride the runtime receipt.

[RAIL_LAW]:
- Package: `System.Threading.Tasks.Dataflow`
- Owns: bounded drainable message blocks with completion propagation
- Accept: background work over bounded blocks with explicit backpressure
- Reject: unbounded local queues and hand-rolled fan-out loops
