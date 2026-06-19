# [RASM_APPUI_API_SYSTEM_REACTIVE]

`System.Reactive` supplies observable streams, subjects, schedulers, disposables, notifications, event conversion, joins, and query operators.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Reactive`
- package: `System.Reactive`
- assembly: `System.Reactive`
- namespace: `System.Reactive`
- namespace: `System.Reactive.Linq`
- namespace: `System.Reactive.Subjects`
- namespace: `System.Reactive.Concurrency`
- namespace: `System.Reactive.Disposables`
- namespace: `System` (`ObservableExtensions`)
- asset: runtime library
- rail: streams

## [02]-[PUBLIC_TYPES]

[STREAM_TYPES]: observable and observer contracts — rail: streams

| [INDEX] | [SYMBOL]                   | [KIND]             |
| :-----: | :------------------------- | :----------------- |
|  [01]   | `IObservable<T>`           | stream contract    |
|  [02]   | `IObserver<T>`             | observer contract  |
|  [03]   | `ObservableBase<T>`        | observable base    |
|  [04]   | `Observer`                 | observer factory   |
|  [05]   | `Notification<T>`          | notification value |
|  [06]   | `EventPattern<TEventArgs>` | event value        |
|  [07]   | `Timestamped<T>`           | timestamp value    |
|  [08]   | `TimeInterval<T>`          | interval value     |

[SUBJECT_TYPES]: subject and connection surfaces — rail: streams

| [INDEX] | [SYMBOL]                    | [KIND]             |
| :-----: | :-------------------------- | :----------------- |
|  [01]   | `Subject<T>`                | multicast subject  |
|  [02]   | `BehaviorSubject<T>`        | current value      |
|  [03]   | `ReplaySubject<T>`          | replay buffer      |
|  [04]   | `AsyncSubject<T>`           | terminal value     |
|  [05]   | `ISubject<T>`               | subject contract   |
|  [06]   | `IConnectableObservable<T>` | connectable stream |

[SCHEDULER_TYPES]: scheduler and timing surfaces — rail: streams

| [INDEX] | [SYMBOL]                                    | [KIND]            |
| :-----: | :------------------------------------------ | :---------------- |
|  [01]   | `IScheduler`                                | schedule contract |
|  [02]   | `Scheduler`                                 | schedule factory  |
|  [03]   | `CurrentThreadScheduler`                    | current thread    |
|  [04]   | `EventLoopScheduler`                        | event loop        |
|  [05]   | `TaskPoolScheduler`                         | task pool         |
|  [06]   | `ThreadPoolScheduler`                       | thread pool       |
|  [07]   | `SynchronizationContextScheduler`           | UI context        |
|  [08]   | `VirtualTimeScheduler<TAbsolute,TRelative>` | virtual time      |

[DISPOSABLE_TYPES]: lifecycle ownership — rail: streams

| [INDEX] | [SYMBOL]                       | [KIND]              |
| :-----: | :----------------------------- | :------------------ |
|  [01]   | `CompositeDisposable`          | grouped disposal    |
|  [02]   | `SerialDisposable`             | replaceable slot    |
|  [03]   | `SingleAssignmentDisposable`   | one assignment      |
|  [04]   | `MultipleAssignmentDisposable` | multiple assignment |
|  [05]   | `RefCountDisposable`           | shared disposal     |
|  [06]   | `CancellationDisposable`       | token disposal      |
|  [07]   | `Disposable`                   | disposable factory  |

## [03]-[ENTRYPOINTS]

[FACTORY_ENTRYPOINTS]: observable and observer creation
- rail: streams

| [INDEX] | [SURFACE]          | [SURFACE_ROOT] | [RAIL]             |
| :-----: | :----------------- | :------------- | :----------------- |
|  [01]   | `Create`           | `Observable`   | stream factory     |
|  [02]   | `Defer`            | `Observable`   | lazy stream        |
|  [03]   | `Return`           | `Observable`   | scalar stream      |
|  [04]   | `Empty`            | `Observable`   | empty stream       |
|  [05]   | `Never`            | `Observable`   | nonterminal stream |
|  [06]   | `FromEventPattern` | `Observable`   | event stream       |
|  [07]   | `Interval`         | `Observable`   | interval stream    |
|  [08]   | `Timer`            | `Observable`   | timer stream       |
|  [09]   | `Create`           | `Observer`     | observer factory   |

[QUERY_ENTRYPOINTS]: stream projection and composition
- rail: streams

| [INDEX] | [SURFACE]              | [SURFACE_ROOT] | [RAIL]         |
| :-----: | :--------------------- | :------------- | :------------- |
|  [01]   | `Select`               | `Observable`   | projection     |
|  [02]   | `Where`                | `Observable`   | filter         |
|  [03]   | `SelectMany`           | `Observable`   | flatten        |
|  [04]   | `Merge`                | `Observable`   | merge streams  |
|  [05]   | `Switch`               | `Observable`   | latest stream  |
|  [06]   | `CombineLatest`        | `Observable`   | combine state  |
|  [07]   | `DistinctUntilChanged` | `Observable`   | distinct state |
|  [08]   | `StartWith`            | `Observable`   | seed stream    |
|  [09]   | `Throttle`             | `Observable`   | rate limit     |
|  [10]   | `TakeUntil`            | `Observable`   | lifecycle stop |

[SCHEDULING_AND_ERROR_ENTRYPOINTS]: scheduling and fault operations
- rail: streams

| [INDEX] | [SURFACE]     | [SURFACE_ROOT]         | [RAIL]                |
| :-----: | :------------ | :--------------------- | :-------------------- |
|  [01]   | `ObserveOn`   | `Observable`           | observer schedule     |
|  [02]   | `SubscribeOn` | `Observable`           | subscription schedule |
|  [03]   | `Catch`       | `Observable`           | error recovery        |
|  [04]   | `Retry`       | `Observable`           | retry policy          |
|  [05]   | `Finally`     | `Observable`           | termination hook      |
|  [06]   | `Using`       | `Observable`           | resource scope        |
|  [07]   | `Subscribe`   | `ObservableExtensions` | subscription          |
|  [08]   | `DisposeWith` | `DisposableMixins`     | lifecycle ownership   |

## [04]-[IMPLEMENTATION_LAW]

[STREAM_LAW]:
- Package: `System.Reactive`
- Owns: observable stream algebra, subjects, schedulers, notifications, disposables, and event conversion
- Accept: streams terminate at lifecycle scopes with explicit scheduling and disposal
- Reject: event-handler chains

[UI_STREAM_LAW]:
- Package: `System.Reactive`
- Owns: common observable rail under ReactiveUI, DynamicData, validation, and control behavior
- Accept: shell, sidecar, companion, diagnostics, support, and downstream app streams share one lifecycle vocabulary
- Reject: per-control event fanout as state transport
