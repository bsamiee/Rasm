# [RASM_API_BCL_CHANNELS]

`System.Threading.Channels` owns asynchronous producer-consumer transport: one channel splits into a `ChannelReader<T>` observation half and a `ChannelWriter<T>` publication half, so a host or native callback publishes without blocking while a consumer drains at its own cadence. Each half reaches only its own direction, so the type separates publication ownership from observation ownership.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Threading.Channels`
- package: `System.Threading.Channels` (MIT)
- assembly: `System.Threading.Channels.dll` (shared framework)
- namespace: `System.Threading.Channels`
- rail: producer-consumer transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transport roots, halves, policy records, and the closed fault

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]   | [CAPABILITY]                                                    |
| :-----: | :-------------------------------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `Channel<TWrite, TRead>`                | abstract class  | asymmetric transport root owning both halves                    |
|  [02]   | `Channel<T>`                            | abstract class  | symmetric refinement of the root                                |
|  [03]   | `ChannelReader<T>`                      | abstract class  | observation half                                                |
|  [04]   | `ChannelWriter<T>`                      | abstract class  | publication half                                                |
|  [05]   | `Channel`                               | static class    | construction owner                                              |
|  [06]   | `ChannelOptions`                        | abstract class  | `SingleReader`, `SingleWriter`, `AllowSynchronousContinuations` |
|  [07]   | `BoundedChannelOptions`                 | sealed class    | `Capacity` and `FullMode` over the inherited flags              |
|  [08]   | `UnboundedChannelOptions`               | sealed class    | inherited flags alone                                           |
|  [09]   | `UnboundedPrioritizedChannelOptions<T>` | sealed class    | `Comparer` — the drain order                                    |
|  [10]   | `BoundedChannelFullMode`                | enum            | `Wait`, `DropNewest`, `DropOldest`, `DropWrite`                 |
|  [11]   | `ChannelClosedException`                | exception class | closed-channel fault over `InvalidOperationException`           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction, half hand-out, and both halves' members — every `Channel` static returns `Channel<T>` and every `CancellationToken` parameter defaults to `default`

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Channel.CreateBounded<T>(int)`                                                | static   | fixed capacity under the `Wait` default     |
|  [02]   | `Channel.CreateBounded<T>(BoundedChannelOptions)`                              | static   | binds the full policy record                |
|  [03]   | `Channel.CreateBounded<T>(BoundedChannelOptions, Action<T>?)`                  | static   | binds the drop observer                     |
|  [04]   | `Channel.CreateUnbounded<T>()`                                                 | static   | any reader and writer count                 |
|  [05]   | `Channel.CreateUnbounded<T>(UnboundedChannelOptions)`                          | static   | binds the arity flags                       |
|  [06]   | `Channel.CreateUnboundedPrioritized<T>()`                                      | static   | drains by `Comparer<T>.Default`             |
|  [07]   | `Channel.CreateUnboundedPrioritized<T>(UnboundedPrioritizedChannelOptions<T>)` | static   | drains by the supplied comparer             |
|  [08]   | `BoundedChannelOptions(int)`                                                   | ctor     | admits the capacity ceiling                 |
|  [09]   | `Channel<TWrite, TRead>.Reader -> ChannelReader<TRead>`                        | property | hands out the observation half              |
|  [10]   | `Channel<TWrite, TRead>.Writer -> ChannelWriter<TWrite>`                       | property | hands out the publication half              |
|  [11]   | `implicit operator ChannelReader<TRead>(Channel<TWrite, TRead>)`               | operator | channel binds a reader-typed slot           |
|  [12]   | `implicit operator ChannelWriter<TWrite>(Channel<TWrite, TRead>)`              | operator | channel binds a writer-typed slot           |
|  [13]   | `ChannelReader<T>.TryRead(out T) -> bool`                                      | instance | non-blocking dequeue                        |
|  [14]   | `ChannelReader<T>.WaitToReadAsync(CancellationToken) -> ValueTask<bool>`       | instance | `false` once no data will arrive            |
|  [15]   | `ChannelReader<T>.ReadAsync(CancellationToken) -> ValueTask<T>`                | instance | awaits and dequeues one element             |
|  [16]   | `ChannelReader<T>.ReadAllAsync(CancellationToken) -> IAsyncEnumerable<T>`      | instance | drains to channel completion                |
|  [17]   | `ChannelReader<T>.TryPeek(out T) -> bool`                                      | instance | inspects the head without dequeue           |
|  [18]   | `ChannelReader<T>.Completion -> Task`                                          | property | terminal await over the whole channel       |
|  [19]   | `ChannelReader<T>.Count -> int`                                                | property | buffered count; throws unless `CanCount`    |
|  [20]   | `ChannelReader<T>.CanCount -> bool`                                            | property | gates `Count`                               |
|  [21]   | `ChannelReader<T>.CanPeek -> bool`                                             | property | gates `TryPeek`                             |
|  [22]   | `ChannelWriter<T>.TryWrite(T) -> bool`                                         | instance | non-blocking publish                        |
|  [23]   | `ChannelWriter<T>.WaitToWriteAsync(CancellationToken) -> ValueTask<bool>`      | instance | space-available gate                        |
|  [24]   | `ChannelWriter<T>.WriteAsync(T, CancellationToken) -> ValueTask`               | instance | awaits capacity, then writes                |
|  [25]   | `ChannelWriter<T>.Complete(Exception?) -> void`                                | instance | throws `ChannelClosedException` if complete |
|  [26]   | `ChannelWriter<T>.TryComplete(Exception?) -> bool`                             | instance | idempotent form returning `false`           |

- `ChannelReader<T>.TryRead` / `TryPeek`: the `out` element carries `[MaybeNullWhen(false)]`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Channel<TWrite, TRead>` owns the lifecycle and assigns both halves through protected setters; `Channel<T>` refines it symmetrically, so an asymmetric lane and a same-type lane share one root and one completion.
- `TryRead` and `WaitToReadAsync` on the reader and `TryWrite` and `WaitToWriteAsync` on the writer are the abstract members; every other member is virtual over them, so a custom channel binds those four and inherits the drain, peek, count, and completion surface.
- Construction consumes the options record once and the constructed channel exposes no policy member, so capacity, full mode, arity, and continuation inlining are fixed for the channel's life and a policy change mints a new channel.
- `ChannelClosedException` derives `InvalidOperationException` and carries every closed-channel fault; `Completion` instead faults with the writer's supplied error directly, so one terminal await separates producer failure from graceful drain.

[STACKING]:
- `LanguageExt.Core`(`.api/api-languageext.md`): the `itemDropped` delegate and a rejected `TryWrite` fold into one `Atom<A>.Swap` receipt cell, and a `ReadAllAsync` drain body lands on `Fin<A>`/`Eff<A>` so a consumer fault is a typed row.
- `System.Diagnostics.Metrics`(`.api/api-diagnostics-metrics.md`): `ChannelReader<T>.Count` is the level reader `Meter.CreateObservableGauge<T>` pulls at collection cadence, bound only where `CanCount` holds.
- `Rasm.Rhino` `Display/interaction`, `Rasm.Compute` `Runtime/scheduling`, `Rasm.AppHost` `Wire/livewire`: each owns one channel per lease, lane, or binding, publishes through a callback-local `TryWrite`, and hands consumers the `ChannelReader<T>` alone.
- Richest composition: `ReadAllAsync` feeds `Parallel.ForEachAsync` under a declared degree, `Channel<TWrite, TRead>` carries a decode-on-read transform lane, and `UnboundedPrioritizedChannelOptions<T>.Comparer` re-orders a drain by rank instead of arrival.

[LOCAL_ADMISSION]:
- A producer-consumer hand-off composes this surface; the owner retains the `ChannelWriter<T>` and publishes only the `ChannelReader<T>`, so a consumer cannot write or complete the lane it drains.
- A custom transport subclasses `Channel<TWrite, TRead>`; the implicit half operators foreclose a wrapper type that re-exposes reader members.

[RAIL_LAW]:
- Package: `System.Threading.Channels`
- Owns: asynchronous producer-consumer transport — buffered hand-off, capacity and full-mode policy, drop observation, priority drain, and the reader/writer split
- Accept: bounded, rendezvous, unbounded, and prioritized construction; callback-local `TryWrite`; the `WaitToReadAsync`/`TryRead` and `ReadAllAsync` drains; `Complete`/`TryComplete` shutdown
- Reject: a lock-and-semaphore queue, `BlockingCollection<T>` on an async path, a caller-side drop counter where `itemDropped` observes eviction
