# [RASM_APPUI_API_SYSTEM_REACTIVE]

`System.Reactive` supplies the observable stream algebra under the AppUi reactive stack: `IObservable<T>`/`IObserver<T>` plus the `Observable` LINQ operators, the subject family (`Subject`/`BehaviorSubject`/`ReplaySubject`/`AsyncSubject`), the scheduler family (including the UI-marshalling `SynchronizationContextScheduler`), the disposable lifecycle family, and the async/event bridges (`FromAsync`/`FromEvent`/`FromEventPattern`/`ToObservable`/`RunAsync`). It is the lowest tier of the reactive stack: DynamicData change-sets, ReactiveUI commands, and Avalonia `AvaloniaObjectExtensions.GetObservable` all emit `IObservable<T>`, so this package owns the operators that fold those streams, the schedulers that marshal them to the render thread, and the disposables that bound their lifetime. The `DisposeWith`/`WhenActivated` lifecycle helpers belong to `api-reactiveui.md`, not here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Reactive`
- package: `System.Reactive` (version `6.1.0`, MIT)
- assembly: `System.Reactive` (bound TFM `net6.0` — the highest non-Windows lib asset; the package also ships `net472` and `net6.0-windows10.0.19041` UI-thread variants the workspace does not bind)
- namespace: `System.Reactive`
- namespace: `System.Reactive.Linq` (`Observable`, `Qbservable`-free in this asset)
- namespace: `System.Reactive.Subjects`
- namespace: `System.Reactive.Concurrency`
- namespace: `System.Reactive.Disposables`
- namespace: `System` (`ObservableExtensions.Subscribe`)
- asset: runtime library
- rail: streams

## [02]-[PUBLIC_TYPES]

[STREAM_TYPES]: observable, observer, and notification value carriers — rail: streams

| [INDEX] | [SYMBOL]                       | [KIND]               |
| :-----: | :----------------------------- | :------------------- |
|  [01]   | `IObservable<T>`               | stream contract      |
|  [02]   | `IObserver<T>`                 | observer contract    |
|  [03]   | `ObservableBase<T>`            | observable base      |
|  [04]   | `Observer`                     | observer factory     |
|  [05]   | `Notification<T>`              | materialized value   |
|  [06]   | `EventPattern<TEventArgs>`     | event value          |
|  [07]   | `IGroupedObservable<TKey,TElement>` | keyed substream |
|  [08]   | `Timestamped<T>` / `TimeInterval<T>` | timed value     |
|  [09]   | `Unit`                         | void-stream token    |

[SUBJECT_TYPES]: subject and connection surfaces — rail: streams

| [INDEX] | [SYMBOL]                    | [KIND]                                |
| :-----: | :-------------------------- | :------------------------------------ |
|  [01]   | `Subject<T>`                | multicast subject (`HasObservers`)    |
|  [02]   | `BehaviorSubject<T>`        | current-value subject (`Value`)       |
|  [03]   | `ReplaySubject<T>`          | bounded/timed replay buffer           |
|  [04]   | `AsyncSubject<T>`           | terminal-value subject (`GetAwaiter`) |
|  [05]   | `ISubject<T>` / `ISubject<TSource,TResult>` | subject contract      |
|  [06]   | `IConnectableObservable<T>` | deferred-connect multicast (`Connect`)|

[SCHEDULER_TYPES]: scheduler and timing surfaces — rail: streams

| [INDEX] | [SYMBOL]                                    | [KIND]                          |
| :-----: | :------------------------------------------ | :------------------------------ |
|  [01]   | `IScheduler`                                | schedule contract               |
|  [02]   | `Scheduler`                                 | schedule factory + extensions   |
|  [03]   | `CurrentThreadScheduler`                    | trampolined current thread      |
|  [04]   | `EventLoopScheduler`                        | dedicated serial worker         |
|  [05]   | `TaskPoolScheduler` / `ThreadPoolScheduler` | pooled concurrency              |
|  [06]   | `SynchronizationContextScheduler`           | UI-thread marshal               |
|  [07]   | `VirtualTimeScheduler<TAbsolute,TRelative>` / `HistoricalScheduler` | deterministic test clock |

[DISPOSABLE_TYPES]: lifecycle ownership — rail: streams

| [INDEX] | [SYMBOL]                       | [KIND]                          |
| :-----: | :----------------------------- | :------------------------------ |
|  [01]   | `CompositeDisposable`          | grouped disposal (`Add`/`Clear`)|
|  [02]   | `SerialDisposable`             | replaceable slot, disposes prior |
|  [03]   | `SingleAssignmentDisposable`   | one-assignment slot             |
|  [04]   | `MultipleAssignmentDisposable` | reassignable slot               |
|  [05]   | `RefCountDisposable`           | shared disposal with leases     |
|  [06]   | `CancellationDisposable`       | `CancellationTokenSource` bridge |
|  [07]   | `Disposable`                   | `Create(Action)` / `Empty` factory |

## [03]-[ENTRYPOINTS]

[FACTORY_ENTRYPOINTS]: observable creation and async/event bridges
- rail: streams

| [INDEX] | [SURFACE]                                          | [SURFACE_ROOT] | [RAIL]                         |
| :-----: | :------------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `Create` / `Defer` / `Generate`                    | `Observable`   | stream factory / lazy / unfold |
|  [02]   | `Return` / `Empty` / `Never` / `Throw` / `Range`   | `Observable`   | scalar / terminal / sequence   |
|  [03]   | `Interval` / `Timer`                               | `Observable`   | timed stream                   |
|  [04]   | `FromAsync(Func<Task<T>>)` / `FromAsync(Func<CancellationToken,Task<T>>)` | `Observable` | Task -> stream bridge |
|  [05]   | `FromEvent` / `FromEventPattern`                   | `Observable`   | .NET event -> stream bridge    |
|  [06]   | `ToObservable(IEnumerable<T>, scheduler?)`         | `Observable`   | sequence -> stream             |
|  [07]   | `ToEvent` / `ToEventPattern`                       | `Observable`   | stream -> .NET event sink      |
|  [08]   | `ForEachAsync` / `RunAsync` / `GetAwaiter`         | `Observable`   | stream -> Task await terminal  |
|  [09]   | `Create` (factory)                                 | `Observer`     | observer factory               |

[QUERY_ENTRYPOINTS]: projection, stateful folds, and composition
- rail: streams

| [INDEX] | [SURFACE]                                            | [SURFACE_ROOT] | [RAIL]                          |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `Select` / `SelectMany` / `Where`                    | `Observable`   | project / flatten / filter      |
|  [02]   | `Scan` / `Aggregate`                                 | `Observable`   | running fold / terminal fold    |
|  [03]   | `Buffer` / `Window`                                  | `Observable`   | count/time batching             |
|  [04]   | `GroupBy`                                            | `Observable`   | keyed substreams                |
|  [05]   | `Merge` / `Concat` / `Switch` / `Amb`                | `Observable`   | combine / sequence / latest     |
|  [06]   | `CombineLatest` / `WithLatestFrom` / `Zip`           | `Observable`   | combine state / gated-combine   |
|  [07]   | `DistinctUntilChanged` / `Distinct`                  | `Observable`   | dedupe state                    |
|  [08]   | `StartWith` / `Do` / `Materialize` / `Dematerialize` | `Observable`   | seed / side-effect / notify lift |
|  [09]   | `Throttle` / `Sample`                                | `Observable`   | trailing rate-limit / pulse-gate |
|  [10]   | `TakeUntil` / `SkipUntil` / `Take` / `Skip`          | `Observable`   | lifecycle window                |

[HOT_STREAM_ENTRYPOINTS]: multicast, connection, and sharing
- rail: streams

| [INDEX] | [SURFACE]                          | [SURFACE_ROOT] | [RAIL]                          |
| :-----: | :--------------------------------- | :------------- | :------------------------------ |
|  [01]   | `Publish` / `Replay`               | `Observable`   | -> `IConnectableObservable<T>`  |
|  [02]   | `Multicast(subjectFactory)`        | `Observable`   | custom-subject multicast        |
|  [03]   | `RefCount`                         | `Observable`   | auto connect/disconnect on subs |
|  [04]   | `Connect`                          | `IConnectableObservable` | start the shared source |

[SCHEDULING_AND_ERROR_ENTRYPOINTS]: scheduling, fault recovery, and subscription
- rail: streams

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT]         | [RAIL]                          |
| :-----: | :----------------------------------------- | :--------------------- | :------------------------------ |
|  [01]   | `ObserveOn` / `SubscribeOn`                | `Observable`           | observer / subscription marshal |
|  [02]   | `Synchronize`                              | `Observable`           | serialize concurrent emissions  |
|  [03]   | `Catch` / `Retry` / `RetryWhen`            | `Observable`           | recover / retry / signalled retry|
|  [04]   | `Timeout`                                  | `Observable`           | per-emission deadline           |
|  [05]   | `Finally` / `Using`                        | `Observable`           | termination hook / resource scope |
|  [06]   | `Subscribe(onNext, onError, onCompleted, ct?)` | `ObservableExtensions` | terminal subscription       |

## [04]-[IMPLEMENTATION_LAW]

[STREAM_LAW]:
- Package: `System.Reactive`
- Owns: the observable stream algebra, subjects, schedulers, notifications, disposables, and the async/event bridges; this is the operator and lifetime tier under DynamicData, ReactiveUI, and Avalonia property streams.
- Accept: streams terminate at an explicit lifecycle scope with an explicit scheduler and an explicit disposable; cold sources that fan out share through `Publish().RefCount()` rather than re-subscribing per consumer.
- Reject: an event-handler chain as state transport; an unscheduled cross-thread `Subscribe` that mutates UI state off the render thread.

[STACKING]:
- DynamicData change-set streams are `IObservable<IChangeSet<T,TKey>>` — the Charts/Editing live-data feeds compose DynamicData's `Filter`/`Transform`/`Page`/`Virtualise` into the change-set, then drop to this package's `ObserveOn(SynchronizationContextScheduler)` exactly once at the bind edge to marshal the materialized snapshot onto the render thread, and the `Throttle`/`Sample` cadence gate is a `System.Reactive` operator over that stream.
- ReactiveUI `ReactiveCommand` and `WhenAnyValue` emit `IObservable<T>`; the command table folds those with `Merge`/`CombineLatest`/`WithLatestFrom` from this package, and a command's `CanExecute` is a `DistinctUntilChanged` over a derived predicate stream. The `DisposeWith`/`WhenActivated` activation helpers are ReactiveUI surface (`api-reactiveui.md`); the underlying `CompositeDisposable` is this package.
- Avalonia property streams arrive through `AvaloniaObjectExtensions.GetObservable(property)`/`GetPropertyChangedObservable`/`GetBindingObservable` (`api-avalonia.md`); a control-state reaction folds those through `Throttle`/`Select`/`DistinctUntilChanged` here and subscribes under a `CompositeDisposable` torn down with the view.
- Hot fan-out is one shared source: a sensor/feed/timer stream that drives multiple tiles is `Publish().RefCount()` (or `Replay(n).RefCount()` for late subscribers needing the last `n`), so the source runs once and `Connect` lifetime tracks the subscriber count — never one cold subscription per consumer.
- Async boundaries cross through `FromAsync` (a per-subscription Task), `FromEventPattern` (a host event), and the await terminal (`RunAsync`/`GetAwaiter`/`AsyncSubject.GetAwaiter`); the AppHost effect rails consume a stream's terminal value through `await observable` where a single result is needed.

[SCHEDULER_LAW]:
- `SynchronizationContextScheduler` is the render-thread marshal: bind-edge `ObserveOn` carries the captured Avalonia `SynchronizationContext`, composed once per pipeline. `TaskPoolScheduler`/`EventLoopScheduler` carry off-thread work (a downsampling fold, a feed decode) before the single marshal hop. `VirtualTimeScheduler`/`HistoricalScheduler` drive deterministic time in snapshot tests so a `Throttle`/`Timer` pipeline verifies without wall-clock flake.
- A subject is the imperative ingress only at a boundary: `BehaviorSubject<T>` holds current cross-filter/brush state (the dashboards `FilterState` spine), `ReplaySubject` buffers pre-subscription emissions, `Subject` is a raw multicast hub; internal stream composition stays operator-driven, never subject-mutation-driven.

[UI_STREAM_LAW]:
- Package: `System.Reactive`
- Owns: the one observable lifecycle vocabulary shared under ReactiveUI, DynamicData, ReactiveUI.Validation, Xaml.Behaviors observable triggers, and control-state behavior across every admitted surface (shell, sidecar, companion, diagnostics, headless).
- Accept: shell, sidecar, companion, diagnostics, support, and downstream app streams share one scheduler/disposable vocabulary; the render-thread hop is a single `ObserveOn` per pipeline.
- Reject: per-control event fanout as state transport; a second scheduling/disposal vocabulary; `DisposeWith` framed as a `System.Reactive` member (it is ReactiveUI).
