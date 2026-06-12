# [RASM_APPUI_API_SYSTEM_REACTIVE]

`System.Reactive` supplies observable streams, subjects, schedulers, disposables, notifications, event conversion, joins, and query operators.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Reactive`
- package: `System.Reactive`
- assembly: `System.Reactive`
- namespace: `System.Reactive`
- namespace: `System.Reactive.Linq`
- namespace: `System.Reactive.Subjects`
- namespace: `System.Reactive.Concurrency`
- namespace: `System.Reactive.Disposables`
- namespace: `System.ObservableExtensions`
- asset: runtime library
- rail: streams

## [2]-[PUBLIC_TYPES]

[STREAM_TYPES]: observable and observer contracts
- rail: streams

| [INDEX] | [SYMBOL]                   | [RAIL]             |
| :-----: | :------------------------- | :----------------- |
|   [1]   | `IObservable<T>`           | stream contract    |
|   [2]   | `IObserver<T>`             | observer contract  |
|   [3]   | `ObservableBase<T>`        | observable base    |
|   [4]   | `Observer`                 | observer factory   |
|   [5]   | `Notification<T>`          | notification value |
|   [6]   | `EventPattern<TEventArgs>` | event value        |
|   [7]   | `Timestamped<T>`           | timestamp value    |
|   [8]   | `TimeInterval<T>`          | interval value     |

[SUBJECT_TYPES]: subject and connection surfaces
- rail: streams

| [INDEX] | [SYMBOL]                    | [RAIL]             |
| :-----: | :-------------------------- | :----------------- |
|   [1]   | `Subject<T>`                | multicast subject  |
|   [2]   | `BehaviorSubject<T>`        | current value      |
|   [3]   | `ReplaySubject<T>`          | replay buffer      |
|   [4]   | `AsyncSubject<T>`           | terminal value     |
|   [5]   | `ISubject<T>`               | subject contract   |
|   [6]   | `IConnectableObservable<T>` | connectable stream |

[SCHEDULER_TYPES]: scheduler and timing surfaces
- rail: streams

| [INDEX] | [SYMBOL]                                    | [RAIL]            |
| :-----: | :------------------------------------------ | :---------------- |
|   [1]   | `IScheduler`                                | schedule contract |
|   [2]   | `Scheduler`                                 | schedule factory  |
|   [3]   | `CurrentThreadScheduler`                    | current thread    |
|   [4]   | `EventLoopScheduler`                        | event loop        |
|   [5]   | `TaskPoolScheduler`                         | task pool         |
|   [6]   | `ThreadPoolScheduler`                       | thread pool       |
|   [7]   | `SynchronizationContextScheduler`           | UI context        |
|   [8]   | `VirtualTimeScheduler<TAbsolute,TRelative>` | virtual time      |

[DISPOSABLE_TYPES]: lifecycle ownership
- rail: streams

| [INDEX] | [SYMBOL]                       | [RAIL]              |
| :-----: | :----------------------------- | :------------------ |
|   [1]   | `CompositeDisposable`          | grouped disposal    |
|   [2]   | `SerialDisposable`             | replaceable slot    |
|   [3]   | `SingleAssignmentDisposable`   | one assignment      |
|   [4]   | `MultipleAssignmentDisposable` | multiple assignment |
|   [5]   | `RefCountDisposable`           | shared disposal     |
|   [6]   | `CancellationDisposable`       | token disposal      |
|   [7]   | `Disposable`                   | disposable factory  |

## [3]-[ENTRYPOINTS]

[FACTORY_ENTRYPOINTS]: observable and observer creation
- rail: streams

| [INDEX] | [SURFACE]          | [SURFACE_ROOT] | [RAIL]             |
| :-----: | :----------------- | :------------- | :----------------- |
|   [1]   | `Create`           | `Observable`   | stream factory     |
|   [2]   | `Defer`            | `Observable`   | lazy stream        |
|   [3]   | `Return`           | `Observable`   | scalar stream      |
|   [4]   | `Empty`            | `Observable`   | empty stream       |
|   [5]   | `Never`            | `Observable`   | nonterminal stream |
|   [6]   | `FromEventPattern` | `Observable`   | event stream       |
|   [7]   | `Interval`         | `Observable`   | interval stream    |
|   [8]   | `Timer`            | `Observable`   | timer stream       |
|   [9]   | `Create`           | `Observer`     | observer factory   |

[QUERY_ENTRYPOINTS]: stream projection and composition
- rail: streams

| [INDEX] | [SURFACE]              | [SURFACE_ROOT] | [RAIL]         |
| :-----: | :--------------------- | :------------- | :------------- |
|   [1]   | `Select`               | `Observable`   | projection     |
|   [2]   | `Where`                | `Observable`   | filter         |
|   [3]   | `SelectMany`           | `Observable`   | flatten        |
|   [4]   | `Merge`                | `Observable`   | merge streams  |
|   [5]   | `Switch`               | `Observable`   | latest stream  |
|   [6]   | `CombineLatest`        | `Observable`   | combine state  |
|   [7]   | `DistinctUntilChanged` | `Observable`   | distinct state |
|   [8]   | `StartWith`            | `Observable`   | seed stream    |
|   [9]   | `Throttle`             | `Observable`   | rate limit     |
|  [10]   | `TakeUntil`            | `Observable`   | lifecycle stop |

[SCHEDULING_AND_ERROR_ENTRYPOINTS]: scheduling and fault operations
- rail: streams

| [INDEX] | [SURFACE]     | [SURFACE_ROOT]         | [RAIL]                |
| :-----: | :------------ | :--------------------- | :-------------------- |
|   [1]   | `ObserveOn`   | `Observable`           | observer schedule     |
|   [2]   | `SubscribeOn` | `Observable`           | subscription schedule |
|   [3]   | `Catch`       | `Observable`           | error recovery        |
|   [4]   | `Retry`       | `Observable`           | retry policy          |
|   [5]   | `Finally`     | `Observable`           | termination hook      |
|   [6]   | `Using`       | `Observable`           | resource scope        |
|   [7]   | `Subscribe`   | `ObservableExtensions` | subscription          |
|   [8]   | `DisposeWith` | disposable extensions  | lifecycle ownership   |

## [4]-[IMPLEMENTATION_LAW]

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
