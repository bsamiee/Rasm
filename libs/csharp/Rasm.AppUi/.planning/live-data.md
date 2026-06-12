# [APPUI_LIVE_DATA]

Rasm.AppUi live data owns every change-set pipeline between data sources and screens: the six-case `DataSource` axis, the operator-row vocabulary, the one UI-thread `BindingCapsule`, and the aggregation rows feeding stat tiles and evidence. The engine is DynamicData over System.Reactive — every source folds into one keyed `SourceCache`, key selectors transcribe the Persistence IdentityPolicy vocabulary, the Ui scheduler arrives from the surface scheduler boundary fed by `UiSchedulerPort`, and change evidence leaves through the `ReceiptSinkPort` envelope. The live-data spine — host fact to projection write to tag transition to delta fetch to `IChangeSet` — is the page's composite automation, and screens consume pipelines as expression folds beside their catalog rows.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                          |
| :-----: | ----------------- | --------------------------------------------------------------- |
|   [1]   | DATA_SOURCES      | Six sourcing cases; one cache feed dispatch; the live-data spine |
|   [2]   | CHANGE_PIPELINES  | Operator rows; dynamic predicate, comparer, page, window streams |
|   [3]   | BINDING_CAPSULE   | One UI-thread binding edge; single ObserveOn; the fault rail     |
|   [4]   | AGGREGATION_SPINE | Stat folds, change-audit evidence, suspend-resume law            |

## [2]-[DATA_SOURCES]

- Owner: `HostDocumentFact`, `SourcePolicy`, `DataSource<TRow, TKey>` — the closed sourcing axis; one generated dispatch feeds one keyed cache per projection.
- Cases: HostDocumentEvents, PersistenceQuery, ComputeReceiptStream, InMemorySeq, RemoteCompanionStream, FakeDeterministic
- Entry: `public (IObservableCache<TRow, TKey> Cache, IDisposable Feed) Open(Func<TRow, TKey> key, SourcePolicy policy, Action<Error> fault)` — the cache is the replay substrate; the feed disposable registers into the caller's activation scope.
- Auto: the live-data spine — a host watch fact drives the Persistence projection write, the tag transition fires `Invalidations`, `Delta` fetches the changed rows, and the cache emits `IChangeSet`; one named pipeline, zero bespoke glue.
- Packages: DynamicData, System.Reactive, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime
- Growth: a new feed is one case on the closed family; a new bound is one policy value on `SourcePolicy`; zero new surface.
- Boundary: `Open` and `Admit` form the page's Rx-to-rail boundary capsule and this fence carries language-owned statement forms inside that capsule; hosts enter only as fact and envelope delegates — Rasm.Rhino `WatchEvent` projects to `HostDocumentFact` at the surface adapter (`WatchPhase` key, document serial, object ids, viewport change counter) and a case body never names a host API; key selectors transcribe the Persistence IdentityPolicy rows — uuidv7 surrogate, content hash, natural key — one key discipline per row model; late subscribers replay from cache state because `Connect` emits current state as the first change set, so per-source replay buffers are the deleted pattern; live rows feed on `TaskPoolScheduler` and the fake row on a `VirtualTimeScheduler` through `SourcePolicy.Source`; receipt-stream bounds trace to the cache-ttl `DeadlineClass` row; an event aggregator and per-source error handlers are the rejected forms — every fault lands in the one `Action<Error>` rail.

```csharp signature
public readonly record struct HostDocumentFact(int PhaseKey, uint DocumentSerial, Seq<Guid> ObjectIds, uint ChangeCounter);

public sealed record SourcePolicy(
    IScheduler Source,
    Option<Duration> Expiry = default,
    Option<int> SizeBound = default,
    Option<Duration> Refresh = default);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DataSource<TRow, TKey> where TRow : notnull where TKey : notnull {
    private DataSource() { }

    public sealed record HostDocumentEvents(
        Func<Action<HostDocumentFact>, IDisposable> Facts,
        Func<HostDocumentFact, Seq<TRow>> Project) : DataSource<TRow, TKey>;

    public sealed record PersistenceQuery(
        Func<Fin<Seq<TRow>>> Snapshot,
        Func<Action<string>, IDisposable> Invalidations,
        Func<string, Fin<Seq<TRow>>> Delta) : DataSource<TRow, TKey>;

    public sealed record ComputeReceiptStream(
        Func<Action<ReceiptEnvelope>, IDisposable> Receipts,
        Func<ReceiptEnvelope, Option<TRow>> Project) : DataSource<TRow, TKey>;

    public sealed record InMemorySeq(Seq<TRow> Rows) : DataSource<TRow, TKey>;

    public sealed record RemoteCompanionStream(
        Func<Action<ReceiptEnvelope>, IDisposable> Stream,
        Func<ReceiptEnvelope, Option<TRow>> Project) : DataSource<TRow, TKey>;

    public sealed record FakeDeterministic(Seq<(Duration At, Seq<TRow> Rows)> Script) : DataSource<TRow, TKey>;

    public (IObservableCache<TRow, TKey> Cache, IDisposable Feed) Open(Func<TRow, TKey> key, SourcePolicy policy, Action<Error> fault) {
        SourceCache<TRow, TKey> cache = new(key);
        return (cache, new CompositeDisposable(cache, Feed(cache, policy, fault)));
    }

    private IDisposable Feed(ISourceCache<TRow, TKey> cache, SourcePolicy policy, Action<Error> fault) =>
        Switch(
            state: (cache, policy, fault),
            hostDocumentEvents: static (s, c) => c.Facts(fact => s.cache.Edit(updater => c.Project(fact).Iter(row => updater.AddOrUpdate(row)))),
            persistenceQuery: static (s, c) => new CompositeDisposable(
                Admit(s.cache, c.Snapshot(), s.fault),
                c.Invalidations(tag => Admit(s.cache, c.Delta(tag), s.fault))),
            computeReceiptStream: static (s, c) => c.Receipts(envelope => s.cache.Edit(updater => c.Project(envelope).Iter(row => updater.AddOrUpdate(row)))),
            inMemorySeq: static (s, c) => Admit(s.cache, Fin.Succ(c.Rows), s.fault),
            remoteCompanionStream: static (s, c) => c.Stream(envelope => s.cache.Edit(updater => c.Project(envelope).Iter(row => updater.AddOrUpdate(row)))),
            fakeDeterministic: static (s, c) => new CompositeDisposable(
                c.Script.Map(step => Observable.Timer(step.At.ToTimeSpan(), s.policy.Source)
                    .Subscribe(_ => Admit(s.cache, Fin.Succ(step.Rows), s.fault)))));

    private static IDisposable Admit(ISourceCache<TRow, TKey> cache, Fin<Seq<TRow>> rows, Action<Error> fault) {
        switch (rows.Case) {
            case Seq<TRow> ok: cache.Edit(updater => ok.Iter(row => updater.AddOrUpdate(row))); break;
            case Error error: fault(error); break;
        }
        return Disposable.Empty;
    }
}
```

```mermaid
flowchart LR
    HostDocumentFact -->|projection write| Invalidations
    Invalidations -->|tag transition| Delta
    Snapshot -->|Admit| SourceCache
    Delta -->|Admit| SourceCache
    SourceCache -->|Connect| IChangeSet
    IChangeSet -->|operator rows| BindingCapsule
    BindingCapsule -->|Into| ObservableCollectionExtended
```

## [3]-[CHANGE_PIPELINES]

- Owner: `PipelineInputs<TRow>` — every dynamic pipeline parameter is an observable value, never a rebuilt pipeline.
- Packages: DynamicData
- Growth: a new operator concern is one operator row; a new bound is one policy value; zero new surface.
- Boundary: predicates, comparers, pages, and windows arrive as streams from screen state — re-filtering pushes a predicate and resubscription is the deleted pattern; grouping is one projection-policy choice per projection, with immutable-state grouping paired to paged and virtualized windows; the classified-exclusion row subtracts the deny projection driven by the AppHost `DataClassification` consequence — classification is never re-modeled here; pipelines are expression folds declared beside the screen catalog row, so a repository layer and per-screen pipeline classes are the rejected forms; a caching layer is equally rejected — caching lives at the AppHost cache port and the Persistence indexes.

```csharp signature
public sealed record PipelineInputs<TRow>(
    IObservable<Func<TRow, bool>> Predicates,
    IObservable<IComparer<TRow>> Comparers,
    IObservable<PageRequest> Pages,
    IObservable<VirtualRequest> Windows);
```

| [INDEX] | [ROW]                | [OPERATORS]             | [POLICY]                                                          |
| :-----: | -------------------- | ----------------------- | ----------------------------------------------------------------- |
|   [1]   | dynamic-filter       | Filter                  | predicate stream from `Predicates`; pushed value, zero resubscribe |
|   [2]   | comparative-sort     | Sort                    | comparer stream from `Comparers` for mid-pipeline order            |
|   [3]   | projection           | Transform               | row models projected from store and receipt shapes                 |
|   [4]   | flat-map             | TransformMany           | one host fact expands to N child rows                              |
|   [5]   | live-grouping        | Group                   | group change sets for live tiles                                   |
|   [6]   | stable-grouping      | GroupWithImmutableState | the projection-policy row for paged and virtualized projections    |
|   [7]   | property-refresh     | AutoRefresh             | `Refresh` buffer, 250 ms on host-fact rows                         |
|   [8]   | child-merge          | MergeMany               | child observable composition                                       |
|   [9]   | timed-expiry         | ExpireAfter             | `Expiry` = cache-ttl allotment on receipt-stream rows              |
|  [10]   | size-bound           | LimitSizeTo             | `SizeBound` = 10000 rows on receipt-stream rows                    |
|  [11]   | paging               | Page                    | `Pages` stream; PageRequest size 50 default                        |
|  [12]   | windowing            | Virtualise              | `Windows` stream; VirtualRequest window 100 default                |
|  [13]   | set-algebra          | And, Or, Except, Xor    | keyed source composition across `DataSource` outputs               |
|  [14]   | classified-exclusion | Except                  | subtracts the `DataClassification` deny projection                 |

## [4]-[BINDING_CAPSULE]

- Owner: `BindingCapsule` — the single UI-thread binding edge.
- Entry: `public IDisposable Into<TRow, TKey>(IObservable<IChangeSet<TRow, TKey>> pipeline, ObservableCollectionExtended<TRow> target, Option<IObservable<IComparer<TRow>>> order = default)` — sorted binding rides the comparer stream; absent order is the bare bind.
- Packages: DynamicData, System.Reactive, LanguageExt.Core
- Growth: a new binding posture is one policy value on the capsule record; zero new surface.
- Boundary: the capsule is the UI-thread boundary capsule and this fence carries the subscription edge under that carve-out; `ObserveOn` applies exactly once here — a second `ObserveOn` anywhere in a pipeline is the named defect; `Ui` arrives from the surface scheduler boundary fed by `UiSchedulerPort`; every `Into` disposable registers into the caller's activation scope, whose disposal receipts are the screens law — no second disposal stream exists here; faults reach the screen fault state through `Fault` and silent failure is structurally impossible; bulk admissions batch through `SuspendNotifications` on `ObservableCollectionExtended` at load edges.

```csharp signature
public sealed record BindingCapsule(IScheduler Ui, Action<Error> Fault) {
    public IDisposable Into<TRow, TKey>(
        IObservable<IChangeSet<TRow, TKey>> pipeline,
        ObservableCollectionExtended<TRow> target,
        Option<IObservable<IComparer<TRow>>> order = default)
        where TRow : notnull where TKey : notnull =>
        (order.Case switch {
            IObservable<IComparer<TRow>> comparers => pipeline.ObserveOn(Ui).SortAndBind(target, comparers),
            _ => pipeline.ObserveOn(Ui).Bind(target),
        })
        .DisposeMany()
        .Subscribe(static _ => { }, raw => Fault(Error.New(raw)));
}
```

## [5]-[AGGREGATION_SPINE]

- Owner: `LiveDataOps` — stat folds and change audit attach to the capsule as one extension block.
- Entry: `public IDisposable Tile<TRow, TKey>(IObservable<IChangeSet<TRow, TKey>> pipeline, Func<IObservable<IChangeSet<TRow, TKey>>, IObservable<double>> fold, Action<double> render)` — one entrypoint serves every stat row.
- Receipt: change-audit rows project `ChangeSummary` into the evidence stream as `ReceiptSinkPort` envelope payloads — process-local, HLC-correlated.
- Packages: DynamicData, System.Reactive, LanguageExt.Core
- Growth: a new statistic is one stat row mapping a fold; zero new surface.
- Boundary: suspend and resume ride the activation scope — surface visibility drives activation at the screens owner, a hidden surface holds zero live subscriptions, and cache state delivers instant replay on resume; gauge and stat tiles on the dashboard surfaces consume `Tile` streams as rows; an OAPH mirror of change-set state, a stats service, and a notification-center history store are the rejected forms.

```csharp signature
public static class LiveDataOps {
    extension(BindingCapsule capsule) {
        public IDisposable Tile<TRow, TKey>(
            IObservable<IChangeSet<TRow, TKey>> pipeline,
            Func<IObservable<IChangeSet<TRow, TKey>>, IObservable<double>> fold,
            Action<double> render)
            where TRow : notnull where TKey : notnull =>
            fold(pipeline).ObserveOn(capsule.Ui).Subscribe(render, raw => capsule.Fault(Error.New(raw)));
    }
}
```

| [INDEX] | [ROW]        | [FOLD]                              | [CONSUMER]                                  |
| :-----: | ------------ | ----------------------------------- | ------------------------------------------- |
|   [1]   | count        | Count                               | stat tiles                                  |
|   [2]   | sum          | Sum                                 | stat tiles                                  |
|   [3]   | average      | Avg                                 | stat tiles                                  |
|   [4]   | minimum      | Minimum                             | stat tiles                                  |
|   [5]   | maximum      | Maximum                             | stat tiles                                  |
|   [6]   | deviation    | StdDev                              | stat tiles                                  |
|   [7]   | change-audit | CollectUpdateStats to ChangeSummary | evidence stream via `ReceiptSinkPort` envelope |

## [6]-[RESEARCH]

| [INDEX] | [ITEM]                                                                                       | [PROOF]                                                                                                     | [GATE]           |
| :-----: | --------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ | ---------------- |
|   [1]   | VirtualTimeScheduler-driven determinism of ExpireAfter and Timer on the fake-deterministic row | uv run python -m tools.assay test run --target Rasm.AppUi                                                     | DATA_SOURCES     |
|   [2]   | GroupWithImmutableState ordering stability under Page and Virtualise windows                   | uv run python -m tools.assay test run --target Rasm.AppUi grouped-window ordering spec                        | CHANGE_PIPELINES |
