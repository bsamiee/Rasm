# [RASM_APPUI_API_SYSTEM_REACTIVE]

`System.Reactive` supplies observable streams, subjects, schedulers, disposables, and query operators.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Reactive`
- package: `System.Reactive`
- assembly: `System.Reactive`
- namespace: `System.Reactive`
- asset: runtime library
- rail: streams

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reactive stream family
- rail: streams

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]   | [CAPABILITY]              |
| :-----: | :-------------------- | :--------------- | :------------------------ |
|   [1]   | `IObservable<T>`      | contract surface | defines boundary contract |
|   [2]   | `IObserver<T>`        | contract surface | defines boundary contract |
|   [3]   | `Subject<T>`          | rail contract    | anchors streams contract  |
|   [4]   | `BehaviorSubject<T>`  | rail contract    | anchors streams contract  |
|   [5]   | `ReplaySubject<T>`    | rail contract    | anchors streams contract  |
|   [6]   | `Observable`          | rail contract    | anchors streams contract  |
|   [7]   | `Scheduler`           | rail contract    | anchors streams contract  |
|   [8]   | `CompositeDisposable` | rail contract    | anchors streams contract  |
|   [9]   | `SerialDisposable`    | rail contract    | anchors streams contract  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stream operations
- rail: streams

| [INDEX] | [SURFACE]       | [CALL_SHAPE]   | [CAPABILITY]          |
| :-----: | :-------------- | :------------- | :-------------------- |
|   [1]   | `Select`        | query operator | projects stream state |
|   [2]   | `Where`         | query operator | projects stream state |
|   [3]   | `Merge`         | query operator | projects stream state |
|   [4]   | `CombineLatest` | query operator | projects stream state |
|   [5]   | `Throttle`      | query operator | projects stream state |
|   [6]   | `ObserveOn`     | query operator | projects stream state |
|   [7]   | `SubscribeOn`   | query operator | projects stream state |
|   [8]   | `TakeUntil`     | query operator | projects stream state |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `System.Reactive`
- Owns: observable stream algebra
- Accept: streams terminate at lifecycle scopes
- Reject: event-handler chains
