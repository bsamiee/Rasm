# [RASM_API_BCL_CHANNELS]

`System.Threading.Channels` is the BCL inbox owner for asynchronous producer-consumer transport — no package pin, shared-framework surface (`System.Threading.Channels.dll`, net10.0, trimmable + AOT-compatible). One `Channel<T>` splits into a `ChannelReader<T>` observation half and a `ChannelWriter<T>` publication half, so a host callback publishes non-blocking while a reactive consumer drains at its own pace. Bounded capacity plus an explicit full-mode policy is the back-pressure contract; the drop-observer callback is the loss-evidence seam. Rhino viewport-interaction leases, Compute work lanes, and AppHost protocol subscriptions each own their channel and expose only the reader.

## [01]-[FACTORY]

`Channel` is the static construction owner. Capacity presence chooses bounded versus unbounded, the options overload carries the policy, and the prioritized family orders draining by a comparer instead of arrival.

- `Channel.CreateBounded<T>(int capacity) -> Channel<T>` constructs a bounded channel at fixed capacity under the default `Wait` full mode; `Channel.CreateBounded<T>(BoundedChannelOptions options) -> Channel<T>` binds the full policy record.
- `Channel.CreateBounded<T>(BoundedChannelOptions options, Action<T>? itemDropped) -> Channel<T>` observes every element evicted or rejected by a drop full mode — `itemDropped` is the loss-evidence seam because `TryWrite` can return `true` while the configured policy silently discards a different element.
- `Channel.CreateUnbounded<T>() -> Channel<T>` constructs an unbounded channel usable by any reader/writer count; `Channel.CreateUnbounded<T>(UnboundedChannelOptions options) -> Channel<T>` binds the single-reader/single-writer optimization flags.
- `Channel.CreateUnboundedPrioritized<T>() -> Channel<T>` drains by natural `Comparer<T>.Default` order; `Channel.CreateUnboundedPrioritized<T>(UnboundedPrioritizedChannelOptions<T> options) -> Channel<T>` drains by the options' supplied comparer — priority ordering costs the FIFO guarantee an ordinary channel keeps.

## [02]-[OPTIONS]

`ChannelOptions` is the abstract policy root; `BoundedChannelOptions`, `UnboundedChannelOptions`, and `UnboundedPrioritizedChannelOptions<T>` are its sealed refinements, and `BoundedChannelFullMode` closes the full-buffer policy.

- `ChannelOptions.SingleReader` / `ChannelOptions.SingleWriter -> bool` promise at-most-one concurrent read/write so the runtime selects a lock-free specialized implementation; a false promise under real concurrency corrupts state, so each flag is set only when the lease structurally guarantees it.
- `ChannelOptions.AllowSynchronousContinuations -> bool` defaults `false` so a completed await never runs its continuation inline on the producing thread — a host callback that writes must not execute a consumer continuation on the UI or native thread.
- `new BoundedChannelOptions(int capacity)` admits a positive capacity and carries `Capacity` and `FullMode` atop the inherited flags; capacity is the buffered-element ceiling the full mode arbitrates.
- `BoundedChannelOptions.FullMode -> BoundedChannelFullMode` selects behavior once the buffer is full: `Wait` parks the writer until space frees, `DropNewest` evicts the newest buffered element, `DropOldest` evicts the head, and `DropWrite` discards the incoming element — a non-blocking callback path admits only the three drop modes because it never waits.
- `UnboundedPrioritizedChannelOptions<T>.Comparer -> IComparer<T>?` supplies the priority order; a null comparer falls back to `Comparer<T>.Default`.

## [03]-[CHANNEL]

`Channel<T>` is the single-element-type channel (`Channel<T> : Channel<T, T>`); `Channel<TWrite, TRead>` is the read/write-asymmetric base the transform channels extend. Both own the two halves and never expose a combined read-write method.

- `Channel<TWrite, TRead>.Reader -> ChannelReader<TRead>` and `Channel<TWrite, TRead>.Writer -> ChannelWriter<TWrite>` split observation from publication while the channel stays the lifecycle owner; a consumer receives only `Reader` and a producer only `Writer`.
- `implicit operator ChannelReader<TRead>(Channel<TWrite, TRead>)` and `implicit operator ChannelWriter<TWrite>(Channel<TWrite, TRead>)` let a channel pass directly where a half is expected, so a field typed as the whole channel binds a reader-typed parameter without an explicit `.Reader` hop.

## [04]-[READER]

`ChannelReader<T>` is the abstract read half. `TryRead` and `WaitToReadAsync` are the abstract core; the drain loop, single-item await, peek, and metrics are virtual over it.

Every cancellation-token parameter defaults to `default(CancellationToken)`.

| [INDEX] | [SURFACE]                                     | [KIND]           | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `TryRead(out T item) -> bool`                 | abstract         | non-blocking dequeue; `false` when empty     |
|  [02]   | `WaitToReadAsync(ct) -> ValueTask<bool>`      | abstract         | `true` when readable, `false` at completion  |
|  [03]   | `ReadAsync(ct) -> ValueTask<T>`               | virtual          | awaits and dequeues one element              |
|  [04]   | `ReadAllAsync(ct) -> IAsyncEnumerable<T>`     | virtual          | `await foreach` drain to channel completion  |
|  [05]   | `TryPeek(out T item) -> bool`                 | virtual          | inspects the head without dequeue            |
|  [06]   | `Completion -> Task`                          | virtual property | completes when no further data will arrive   |
|  [07]   | `Count -> int`                                | virtual property | buffered-element count when `CanCount`       |
|  [08]   | `CanCount` / `CanPeek -> bool`                | virtual property | capability probes gating `Count` / `TryPeek` |

- `TryRead` and `TryPeek` carry `[MaybeNullWhen(false)]` on the `out` element; `WaitToReadAsync`-then-`TryRead` is the canonical non-allocating drain pair (`while (await reader.WaitToReadAsync(ct)) while (reader.TryRead(out var item)) …`); `ReadAllAsync` is the equivalent `IAsyncEnumerable<T>` form threading the same `CancellationToken` an anyio/Task scope carries.
- `Completion` faults with the writer's completion exception when one was supplied, so a consumer distinguishes graceful drain from a producer failure at the single terminal await.

## [05]-[WRITER]

`ChannelWriter<T>` is the abstract write half. `TryWrite` and `WaitToWriteAsync` are the abstract core; the awaiting write and the two completion forms layer over it.

- `ChannelWriter<T>.TryWrite(T item) -> bool` is the non-blocking publish — the callback-local write. A `false` result records a closed channel or a `Wait`-mode full buffer; a drop-mode loss instead surfaces through the factory `itemDropped` observer, never as a `false` here.
- `ChannelWriter<T>.WaitToWriteAsync(CancellationToken = default) -> ValueTask<bool>` completes `true` when space is available and `false` once writing is permanently disallowed — the awaiting producer's back-pressure gate.
- `ChannelWriter<T>.WriteAsync(T item, CancellationToken = default) -> ValueTask` awaits capacity then writes; it is the blocking-producer counterpart to `TryWrite` for a path that must not drop.
- `ChannelWriter<T>.Complete(Exception? error = null) -> void` marks the channel complete and throws `InvalidOperationException` (a `ChannelClosedException` on read-after-close) if already completed; `ChannelWriter<T>.TryComplete(Exception? error = null) -> bool` is the idempotent form returning `false` instead of throwing — buffered elements stay readable after either, and a supplied `error` propagates through `Reader.Completion`.

## [06]-[STACK]

`PointerLease`/`PointerHook` and `WidgetHost` (`Rasm.Rhino` `Display/interaction`) own their `Channel<PointerFact>`/`Channel<WidgetFact>`, expose only the `ChannelReader<T>`, publish through one callback-local `TryWrite`, and map the `PointerOverflow` `[SmartEnum]` rows onto `BoundedChannelFullMode.DropOldest`/`DropNewest`/`DropWrite` so a Rhino input callback never blocks, awaits, or retains a host event argument; a `TryWrite` rejection increments an `Atom<long>` count and drop-mode loss counts through `itemDropped`. `Rasm.Compute` `Runtime/scheduling` constructs one bounded `Channel<WorkItem>` per `WorkLane` — the drop-lane form passes the `item => pressure(row, item, None)` observer so every eviction lands as a correlated `Backpressure` receipt, reads queue depth off `ChannelReader<WorkItem>.Count`, and drains through `ReadAllAsync(env.Token)`; `Model/inference` runs the `SingleReader = true` `FullMode = Wait` bounded queue under a `WaitToReadAsync`-then-`TryRead` loop. `Rasm.AppHost` `Wire/livewire` feeds one bounded `Channel<ExternalValue>` per binding under `DropOldest` from foreign OPC-UA/MQTT/serial/PubSub callback threads, draining each lane's head through `ReadAllAsync`, with the held client living in a token-gated `Atom<Gate>` cell. Every consumer threads `AllowSynchronousContinuations = false` so a native or UI callback never runs a downstream continuation inline, and the reader halves feed LanguageExt reactive rails without the channel owner leaking its `Writer`.
