# [RASM_APPUI_API_SYSTEM_REACTIVE]

`System.Reactive` owns the observable stream algebra under the AppUi reactive stack: the `IObservable<T>`/`IObserver<T>` contracts, the `Observable` operator surface, the subject, scheduler, and disposable families, and the async/event bridges. It is the operator and lifetime tier beneath DynamicData change-sets, ReactiveUI commands, and Avalonia property streams — each emits `IObservable<T>`, and this package folds those streams, marshals them to the render thread, and bounds their lifetime.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Reactive`
- package: `System.Reactive` (MIT)
- assembly: `System.Reactive`
- target: `net6.0`
- namespace: `System.Reactive`, `System.Reactive.Linq`, `System.Reactive.Subjects`, `System.Reactive.Concurrency`, `System.Reactive.Disposables`, `System`
- rail: streams

## [02]-[PUBLIC_TYPES]

[STREAM_TYPES]: observable, observer, and notification value carriers

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]       |
| :-----: | :----------------------------------- | :------------ | :----------------- |
|  [01]   | `IObservable<T>`                     | interface     | stream contract    |
|  [02]   | `IObserver<T>`                       | interface     | observer contract  |
|  [03]   | `ObservableBase<T>`                  | class         | observable base    |
|  [04]   | `Observer`                           | class         | observer factory   |
|  [05]   | `Notification<T>`                    | class         | materialized value |
|  [06]   | `EventPattern<TEventArgs>`           | class         | event value        |
|  [07]   | `IGroupedObservable<TKey,TElement>`  | interface     | keyed substream    |
|  [08]   | `Timestamped<T>` / `TimeInterval<T>` | struct        | timed value        |
|  [09]   | `Unit`                               | struct        | void-stream token  |

[SUBJECT_TYPES]: subject and connection surfaces

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :------------------------------------------ | :------------ | :------------------------------------- |
|  [01]   | `Subject<T>`                                | class         | multicast subject (`HasObservers`)     |
|  [02]   | `BehaviorSubject<T>`                        | class         | current-value subject (`Value`)        |
|  [03]   | `ReplaySubject<T>`                          | class         | bounded/timed replay buffer            |
|  [04]   | `AsyncSubject<T>`                           | class         | terminal-value subject (`GetAwaiter`)  |
|  [05]   | `ISubject<T>` / `ISubject<TSource,TResult>` | interface     | subject contract                       |
|  [06]   | `IConnectableObservable<T>`                 | interface     | deferred-connect multicast (`Connect`) |

[SCHEDULER_TYPES]: scheduler and timing surfaces

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------------ | :------------ | :---------------------------- |
|  [01]   | `IScheduler`                                                        | interface     | schedule contract             |
|  [02]   | `Scheduler`                                                         | class         | schedule factory + extensions |
|  [03]   | `CurrentThreadScheduler`                                            | class         | trampolined current thread    |
|  [04]   | `EventLoopScheduler`                                                | class         | dedicated serial worker       |
|  [05]   | `TaskPoolScheduler` / `ThreadPoolScheduler`                         | class         | pooled concurrency            |
|  [06]   | `SynchronizationContextScheduler`                                   | class         | UI-thread marshal             |
|  [07]   | `VirtualTimeScheduler<TAbsolute,TRelative>` / `HistoricalScheduler` | class         | deterministic test clock      |

[DISPOSABLE_TYPES]: lifecycle ownership

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :----------------------------- | :------------ | :--------------------------------- |
|  [01]   | `CompositeDisposable`          | class         | grouped disposal (`Add`/`Clear`)   |
|  [02]   | `SerialDisposable`             | class         | replaceable slot, disposes prior   |
|  [03]   | `SingleAssignmentDisposable`   | class         | one-assignment slot                |
|  [04]   | `MultipleAssignmentDisposable` | class         | reassignable slot                  |
|  [05]   | `RefCountDisposable`           | class         | shared disposal with leases        |
|  [06]   | `CancellationDisposable`       | class         | `CancellationTokenSource` bridge   |
|  [07]   | `Disposable`                   | class         | `Create(Action)` / `Empty` factory |

## [03]-[ENTRYPOINTS]

[FACTORY_ENTRYPOINTS]: observable creation and async/event bridges over `Observable`

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------ | :------ | :----------------------------- |
|  [01]   | `Create` / `Defer` / `Generate`                                           | factory | stream factory / lazy / unfold |
|  [02]   | `Return` / `Empty` / `Never` / `Throw` / `Range`                          | factory | scalar / terminal / sequence   |
|  [03]   | `Interval` / `Timer`                                                      | factory | timed stream                   |
|  [04]   | `FromAsync(Func<Task<T>>)` / `FromAsync(Func<CancellationToken,Task<T>>)` | factory | Task -> stream bridge          |
|  [05]   | `FromEvent` / `FromEventPattern`                                          | factory | .NET event -> stream bridge    |
|  [06]   | `ToObservable(IEnumerable<T>, scheduler?)`                                | factory | sequence -> stream             |
|  [07]   | `ToEvent` / `ToEventPattern`                                              | fold    | stream -> .NET event sink      |
|  [08]   | `ForEachAsync` / `RunAsync` / `GetAwaiter`                                | fold    | stream -> Task await terminal  |
|  [09]   | `Observer.Create`                                                         | factory | observer factory               |

[QUERY_ENTRYPOINTS]: projection, stateful folds, and composition over `Observable`

| [INDEX] | [SURFACE]                                            | [SHAPE] | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------- | :------ | :------------------------------- |
|  [01]   | `Select` / `SelectMany` / `Where`                    | fold    | project / flatten / filter       |
|  [02]   | `Scan` / `Aggregate`                                 | fold    | running fold / terminal fold     |
|  [03]   | `Buffer` / `Window`                                  | fold    | count/time batching              |
|  [04]   | `GroupBy`                                            | fold    | keyed substreams                 |
|  [05]   | `Merge` / `Concat` / `Switch` / `Amb`                | fold    | combine / sequence / latest      |
|  [06]   | `CombineLatest` / `WithLatestFrom` / `Zip`           | fold    | combine state / gated-combine    |
|  [07]   | `DistinctUntilChanged` / `Distinct`                  | fold    | dedupe state                     |
|  [08]   | `StartWith` / `Do` / `Materialize` / `Dematerialize` | fold    | seed / side-effect / notify lift |
|  [09]   | `Throttle` / `Sample`                                | fold    | trailing rate-limit / pulse-gate |
|  [10]   | `TakeUntil` / `SkipUntil` / `Take` / `Skip`          | fold    | lifecycle window                 |

[HOT_STREAM_ENTRYPOINTS]: multicast, connection, and sharing over `Observable`

| [INDEX] | [SURFACE]                        | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------- | :------- | :------------------------------ |
|  [01]   | `Publish` / `Replay`             | fold     | -> `IConnectableObservable<T>`  |
|  [02]   | `Multicast(subjectFactory)`      | fold     | custom-subject multicast        |
|  [03]   | `RefCount`                       | fold     | auto connect/disconnect on subs |
|  [04]   | `IConnectableObservable.Connect` | instance | start the shared source         |

[SCHEDULING_AND_ERROR_ENTRYPOINTS]: scheduling, fault recovery, and subscription over `Observable`

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------ | :------ | :-------------------------------- |
|  [01]   | `ObserveOn` / `SubscribeOn`                                         | fold    | observer / subscription marshal   |
|  [02]   | `Synchronize`                                                       | fold    | serialize concurrent emissions    |
|  [03]   | `Catch` / `Retry` / `RetryWhen`                                     | fold    | recover / retry / signalled retry |
|  [04]   | `Timeout`                                                           | fold    | per-emission deadline             |
|  [05]   | `Finally` / `Using`                                                 | fold    | termination hook / resource scope |
|  [06]   | `ObservableExtensions.Subscribe(onNext, onError, onCompleted, ct?)` | fold    | terminal subscription             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every stream terminates at an explicit lifecycle scope carrying an explicit scheduler and an explicit disposable; internal composition stays operator-driven, never subject-mutation-driven, and the render-thread hop is a single `ObserveOn` per pipeline.

[STACKING]:
- `api-dynamicdata.md`(`.api/api-dynamicdata.md`): a change-set feed composes DynamicData `Filter`/`Transform`/`Page`/`Virtualise` into `IObservable<IChangeSet<T,TKey>>`, then drops to `ObserveOn(SynchronizationContextScheduler)` once at the bind edge; the `Throttle`/`Sample` cadence gate is a `System.Reactive` operator over that stream.
- `api-reactiveui.md`(`.api/api-reactiveui.md`): `ReactiveCommand`/`WhenAnyValue` emit `IObservable<T>` the command table folds with `Merge`/`CombineLatest`/`WithLatestFrom`, and `CanExecute` is a `DistinctUntilChanged` over a derived predicate stream; the `WhenActivated`/`DisposeWith` activation helpers are ReactiveUI surface over this package's `CompositeDisposable`.
- `api-avalonia.md`(`.api/api-avalonia.md`): property streams arrive through `AvaloniaObjectExtensions.GetObservable`/`GetPropertyChangedObservable`/`GetBindingObservable`, fold through `Throttle`/`Select`/`DistinctUntilChanged` here, and subscribe under a `CompositeDisposable` torn down with the view.
- Hot fan-out is one shared source: a sensor, feed, or timer driving multiple tiles is `Publish().RefCount()` (or `Replay(n).RefCount()` for late subscribers needing the last `n`), so the source runs once and `Connect` lifetime tracks the subscriber count.
- Async boundaries cross through `FromAsync` (a per-subscription Task), `FromEventPattern` (a host event), and the await terminal (`RunAsync`/`GetAwaiter`/`AsyncSubject.GetAwaiter`); the AppHost effect rails consume a terminal value through `await observable`.
- Schedulers compose one marshal hop: `SynchronizationContextScheduler` carries the captured Avalonia context at the bind-edge `ObserveOn`, `TaskPoolScheduler`/`EventLoopScheduler` carry off-thread work before that hop, and `VirtualTimeScheduler`/`HistoricalScheduler` drive deterministic time so a `Throttle`/`Timer` pipeline verifies without wall-clock flake.
- Subjects are imperative ingress only at a boundary: `BehaviorSubject<T>` holds current cross-filter/brush state (the dashboards `FilterState` spine), `ReplaySubject` buffers pre-subscription emissions, and `Subject` is a raw multicast hub.

[LOCAL_ADMISSION]:
- Shell, sidecar, companion, diagnostics, and downstream app streams share one scheduler and disposable vocabulary across every admitted surface.

[RAIL_LAW]:
- Package: `System.Reactive`
- Owns: the observable stream algebra, subjects, schedulers, notifications, disposables, and async/event bridges — the operator and lifetime tier beneath DynamicData, ReactiveUI, and Avalonia property streams.
- Accept: a stream terminating at an explicit lifecycle scope with an explicit scheduler and disposable; cold fan-out sharing through `Publish().RefCount()`.
- Reject: an event-handler chain as state transport; an unscheduled cross-thread `Subscribe` mutating UI state off the render thread; a second scheduling or disposal vocabulary.
