# [PERSISTENCE_ELEMENT_GRAPH]

Rasm.Persistence persists the one `Rasm.Element` `ElementGraph` through Marten on PostgreSQL as the write system of record: each model (or spatial partition) is ONE event stream keyed by `ModelId`, every durable change is a `GraphDelta` event body (`GraphCreated` opens the stream with the `Header`, `GraphRevised` and `GraphRetired` extend it), and the whole `ElementGraph` rehydrates by folding the stream through an INLINE `SingleStreamProjection<GraphProjection, ModelId>` written in the SAME `IDocumentSession` transaction as the events — so authoritative containment and topology are read-your-writes consistent and never route to an async lane. AS-OF reconstruction is `AggregateStreamAsync(version|timestamp)`, periodic `AggregateSnapshot` rows bound replay depth, and the relational `Element/identity#ELEMENT_IDENTITY` `ElementIdentity` row commits atomically with the event by riding the SAME session as a Marten document — one transaction owns identity plus event with no two-ORM gap. The event body carries the `GraphDelta` the seam already validated, never a whole-graph snapshot; `GraphDelta.ReplayOnto` is the one immutable fold the projection and the AS-OF reconstruction both run, so a rehydrated graph is bit-identical to the live state at that version. `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `TenantContext` arrive settled from `Rasm.AppHost`; the codec, content address, sealed header, and chunker arrive from `Element/codec#CODEC_AXIS`; the typed value/graph vocabulary (`ElementGraph`, `Header`, `Node`, `NodeId`, `Relationship`, `GraphDelta`, `ContentAddress`) arrives settled from `Rasm.Element`.

## [01]-[INDEX]

- [01]-[STREAM_GRAIN]: model-stream identity, the `GraphEvent` body family, optimistic append, and the schema-keyed event registration.
- [02]-[GRAPH_PROJECTION]: the inline `SingleStreamProjection` folding `GraphDelta` into the `ElementGraph`, periodic `AggregateSnapshot`, and the read-your-writes consistency boundary.
- [03]-[STORE_RAIL]: the one `GraphStoreOp` operation family, the session bracket, AS-OF reconstruction, and the co-transactional identity commit.

## [02]-[STREAM_GRAIN]

- Owner: `ModelId` the `[ValueObject<Guid>]` per-model stream key under the `IObjectFactory` floor; `GraphEvent` the `[Union]` event-body family every model stream appends; `EventLifecycle` the `[SmartEnum<string>]` create/revise/retire verb each event row carries; `ElementSchema` the static surface owning the `StoreOptions` event registration, the strong-typed value registration, and the per-model stream-start and append legs over the one `IDocumentSession`.
- Cases: `GraphCreated(Header Header, GraphDelta Delta)` opens a stream and carries the `Rasm.Element` `Header` (`ReleaseVersion`/`ModelView`/`GeoReference`/`Tolerance`/`Instant`/`StepHeader`); `GraphRevised(GraphDelta Delta)` is the steady-state append; `GraphRetired(GraphDelta Delta, string Reason)` carries the retirement delta whose `GraphDelta` removes the retired nodes/edges — so retirement is a real convergent delta the projection folds, never an out-of-band tombstone; the event body is ALWAYS the `GraphDelta`, NEVER a whole-graph snapshot, because the delta replays deterministically and a whole-graph body bloats every append by the model size.
- Entry: `public static StoreOptions Configure(StoreOptions opts, NpgsqlDataSource source)` registers the event types, the strong-typed `ModelId`/`NodeId` value types, the inline `GraphProjection`, the async analytical projections, and the snapshot lifecycle once at boot; `public static IEventStoreOperations Open(IDocumentSession session, ModelId model, Header header)` calls `session.Events.StartStream<GraphProjection>(model.Value, new GraphCreated(header, GraphDelta.Empty))`; `public static StreamAction Append(IDocumentSession session, ModelId model, GraphEvent body, long expectedVersion)` appends with the inline optimistic version guard so a concurrent writer racing the same stream version aborts at `SaveChangesAsync` rather than silently interleaving.
- Auto: the stream identity is `StreamIdentity.AsGuid` keyed by `ModelId` so a model is one stream and a spatial partition is one stream — never per-`NodeId`, because a node-grain stream multiplies stream count by element count and forecloses the whole-graph fold; `EventAppendMode.Rich` keeps the full causation/correlation metadata on the authoring path while bulk re-ingest switches to `QuickWithServerTimestamps` for append throughput; the `GraphDelta` the event carries is the SAME value the seam's `IGraphConstraint.Validate` admitted, so the projection apply is total over admitted deltas; `RegisterValueType<ModelId>()`/`RegisterValueType<NodeId>()` teach Marten the `[ValueObject]` keys so the stream key and every document id stay typed end to end and never decay to a bare `Guid`/`string` at the wire.
- Receipt: a stream open rides `store.element.open`, a delta append rides `store.element.commit` carrying the delta node/edge counts, a retirement rides `store.element.retire`; the `StreamAction.Version` is the optimistic guard the next append reads.
- Packages: Marten (`StartStream`/`Append`/`StreamAction`/`EventAppendMode`/`StreamIdentity`/`RegisterValueType`), Npgsql, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new durable change kind is one `GraphEvent` case plus one `EventLifecycle` row plus one projection `Apply` arm the generated dispatch forces; a richer model header is one field on the seam `Header`; a per-spatial-partition grain is one `ModelId` minting policy, never a second stream shape; zero new surface — a per-`NodeId` stream, a whole-graph event body, a second event table, or a bespoke `OpLogEntry` store beneath Marten is the deleted form because Marten owns the durable append and the rebuildable read views, and the `Version/` engine projects FROM these events (`Version/ledger#CHANGEFEED`).
- Boundary: stream grain is ONE stream PER MODEL (or per spatial partition), never per-`NodeId` — overriding the prior per-node draft — and the event body is the `GraphDelta`, never a whole-graph snapshot; the `GraphDelta` is the seam-owned `[Union]` graph-mutation request the projection folds immutably through `GraphDelta.ReplayOnto`, so the durable history is a delta log the engine replays, the snapshot is a periodic fold floor, and the rehydrated graph is bit-identical to the live state at any version because the fold is the one `GraphDelta.ReplayOnto` the AS-OF reconstruction also runs; the optimistic append (`Append(stream, expectedVersion, …)`) is the inline guard, `AppendOptimistic` the read-then-guard, and `FetchForWriting<GraphProjection>(model, expectedVersion)` the load-decide-append handle for a multi-writer-hostile section that takes a stream-level advisory lock through `FetchForExclusiveWriting`; the `GraphRetired` delta is a real convergent retirement the projection folds and the `Version/retention#RETENTION_CLASSES` sweep reclaims, never an `ArchiveStream` that hides the events from the fold (archive is the AS-OF cut boundary, not retirement); a `GraphCreated` carries the `Header` so the stream's `ReleaseVersion`/`GeoReference`/`Tolerance` are the first folded fact and every later delta's measure quantization (`Element/codec#CONTENT_ADDRESS`) reads the header tolerance; `EventAppendMode` trades metadata richness for throughput as a config value, never a per-call branch.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<Guid>]
public readonly partial struct ModelId {
    public static ModelId New() => Create(Guid.CreateVersion7());
}

[SmartEnum<string>]
public sealed partial class EventLifecycle {
    public static readonly EventLifecycle Created = new("created");
    public static readonly EventLifecycle Revised = new("revised");
    public static readonly EventLifecycle Retired = new("retired");
}

// --- [MODELS] --------------------------------------------------------------------------
// The event body family every model stream appends. The body is ALWAYS the seam `GraphDelta`
// (the validated graph-mutation request), never a whole-graph snapshot — `GraphCreated` adds the
// opening `Header`, `GraphRetired` adds the retirement reason, and all three fold through the one
// `GraphDelta.ReplayOnto` the projection runs, so the durable history is a deterministic delta log.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphEvent {
    private GraphEvent() { }

    public sealed record GraphCreated(Header Header, GraphDelta Delta) : GraphEvent;
    public sealed record GraphRevised(GraphDelta Delta) : GraphEvent;
    public sealed record GraphRetired(GraphDelta Delta, string Reason) : GraphEvent;

    public GraphDelta Body => this switch {
        GraphCreated c => c.Delta,
        GraphRevised r => r.Delta,
        GraphRetired t => t.Delta,
    };

    public EventLifecycle Lifecycle => this switch {
        GraphCreated => EventLifecycle.Created,
        GraphRevised => EventLifecycle.Revised,
        GraphRetired => EventLifecycle.Retired,
    };
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class ElementSchema {
    public static StoreOptions Configure(StoreOptions opts, NpgsqlDataSource source) {
        opts.Connection(source);
        opts.Events.StreamIdentity = StreamIdentity.AsGuid;
        opts.Events.AppendMode = EventAppendMode.Rich;
        opts.Events.MetadataConfig.CausationIdEnabled = true;
        opts.Events.MetadataConfig.CorrelationIdEnabled = true;
        opts.Events.MetadataConfig.HeadersEnabled = true;
        opts.UseSystemTextJsonForSerialization(ElementJson.Options, EnumStorage.AsString, Casing.CamelCase);
        opts.RegisterValueType<ModelId>();
        opts.RegisterValueType<NodeId>();
        opts.Projections.Snapshot<GraphProjection>(SnapshotLifecycle.Inline);
        opts.Projections.Add<GraphProjection>(ProjectionLifecycle.Inline);
        return opts;
    }

    public static StreamAction Open(IDocumentSession session, ModelId model, Header header) {
        ArgumentNullException.ThrowIfNull(session);
        return session.Events.StartStream<GraphProjection>(model.Value, new GraphEvent.GraphCreated(header, GraphDelta.Empty));
    }

    public static StreamAction Append(IDocumentSession session, ModelId model, GraphEvent body, long expectedVersion) {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(body);
        return session.Events.Append(model.Value, expectedVersion, body);
    }
}
```

| [INDEX] | [POLICY]               | [VALUE]                               | [BINDING]                                                  |
| :-----: | :--------------------- | :------------------------------------ | :-------------------------------------------------------- |
|  [01]   | stream grain           | one stream per `ModelId`              | `StreamIdentity.AsGuid`; never per-`NodeId`               |
|  [02]   | event body             | the seam `GraphDelta`                 | never a whole-graph snapshot; folds via `GraphDelta.ReplayOnto`|
|  [03]   | append mode            | `EventAppendMode.Rich` (bulk `Quick`) | full metadata on authoring; throughput on re-ingest       |
|  [04]   | optimistic guard       | `Append(stream, expectedVersion, …)`  | concurrent same-version writer aborts at `SaveChangesAsync`|
|  [05]   | strong-typed keys      | `RegisterValueType<ModelId/NodeId>`   | typed stream key + document id, never a bare Guid/string  |

## [03]-[GRAPH_PROJECTION]

- Owner: `GraphProjection` the inline `SingleStreamProjection<GraphProjection, ModelId>` Marten folds one model stream into — it IS the rehydrated `ElementGraph` plus its `ModelId` and folded version, written in the append transaction; `GraphFault` the `[Union]` projection fault deriving from `Expected`; the projection's `Create`/`Apply` convention methods owning the one `GraphDelta.ReplayOnto` fold over the seam graph.
- Cases: `Create(GraphCreated)` seeds the genesis `ElementGraph.Of(header, empty-nodes, empty-edges)` then replays the opening delta through `GraphDelta.ReplayOnto`; `Apply(GraphRevised, GraphProjection)` and `Apply(GraphRetired, GraphProjection)` replay their `GraphDelta` onto the held graph; the fold is `GraphDelta.ReplayOnto : ElementGraph → ElementGraph`, TOTAL over the seam-validated delta because the seam `Graph/delta#GRAPH_DELTA` produced it under `IGraphConstraint.Validate` at the write boundary (`ReplayOnto` re-applies the recorded add/remove/revise raw, never re-validating), so a malformed delta in the stream is a deployment defect the `#STORE_RAIL` bracket surfaces as `GraphFault.DeltaRejected`, not a recoverable per-fold rail.
- Entry: the Marten convention methods `public static GraphProjection Create(IEvent<GraphEvent.GraphCreated> e)` (seeding `Model` from the event's `StreamId`, never an empty key), `public GraphProjection Apply(GraphEvent.GraphRevised e)`, `public GraphProjection Apply(GraphEvent.GraphRetired e)` — Marten discovers them by convention and the source generator (`UseSourceGeneratedDiscovery`) emits the dispatch at compile time; `public ElementGraph Graph` is the rehydrated authoritative graph the synchronous reads serve.
- Auto: the projection registers `ProjectionLifecycle.Inline` so the folded `GraphProjection` document is written in the SAME transaction as the appended events — a `Read` after a `Commit` in the same logical unit sees the new state with no daemon lag; `Projections.Snapshot<GraphProjection>(SnapshotLifecycle.Inline)` writes a periodic `AggregateSnapshot` so a deep stream folds from the latest snapshot rather than genesis, bounding replay cost; the working/live authoring graph uses the seam's `ImmutableDictionary`/HAMT structural-sharing form while the projection freezes to `FrozenDictionary` only at the persistence/read-snapshot boundary (`Element/codec#CONTENT_ADDRESS`), so the delta path stays O(log n) structural-sharing and the read snapshot stays O(1) lookup.
- Receipt: a projection fold rides `store.element.project` carrying the folded delta count and the snapshot-hit flag; a `GraphFault.DeltaRejected` rides `store.element.fault` carrying the rejecting delta and the seam structural-invariant code.
- Packages: Marten (`SingleStreamProjection`/`ProjectionLifecycle`/`SnapshotLifecycle`/`AggregateSnapshot`/`IEvent<T>`), Rasm.Element (`ElementGraph`/`GraphDelta`/`Header`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new event arm is one projection `Apply` method the generated discovery forces; a cross-model rollup (a whole-project aggregate) is one `MultiStreamProjection<ProjectGraph, ProjectId>` slicing by `Identity<GraphEvent>(e => e.ProjectId)`, never a second fold of the same delta; a co-transactional columnar egress is the `Query/columnar#COLUMNAR_LANE` `FlatTableProjection`; zero new surface — a hand-rolled stream folder, a second materializer, or a per-read whole-stream replay is the deleted form because the inline projection IS the materialized read and the AS-OF fold reuses the same `GraphDelta.ReplayOnto`.
- Boundary: the inline projection is the READ-YOUR-WRITES consistency boundary — authoritative containment, topology, and void-resolution reads go through this folded `GraphProjection.Graph`, NEVER an async lane, because an async daemon view lags the write; the analytical lanes (`Query/columnar`, `Query/cypher`) are explicitly `ProjectionLifecycle.Async` with a staleness watermark and interactive-correctness reads block on `WaitForNonStaleProjectionDataAsync` (`Query/lane#READ_ROUTING`); the projection apply is the SAME `GraphDelta.ReplayOnto` fold the `Version/timetravel#TIME_TRAVEL` AS-OF reconstruction runs (the live authoring path produces the deltas it replays, via the seam `Graph/delta#GRAPH_DELTA` `WorkingGraph.Apply`), so there is exactly one delta→graph materializer and a historical fold equals the live state field-for-field; the snapshot is a periodic fold floor bounding replay, never a second source of truth — a deep stream rebuilds from the latest `AggregateSnapshot` plus the suffix, and `store.Advanced.RebuildSingleStreamAsync<GraphProjection>(model)` replays one stream's inline projection from zero when the fold logic changes; the projection never re-validates the delta because the seam `IGraphConstraint` already gated it at the write boundary — re-validation in the projection is the deleted form because a validated delta in the stream is total by construction and a fold-time fault is a deployment defect surfaced as `GraphFault`, not a recoverable data path.

```csharp signature
// --- [ERRORS] --------------------------------------------------------------------------
[Union]
public abstract partial record GraphFault : Expected, IValidationError<GraphFault> {
    private GraphFault(string detail, int code) : base(detail, code, None) { }
    public static GraphFault Create(string message) => new DeltaRejected(message, 0);

    public sealed record DeltaRejected(string Detail, int InvariantCode) : GraphFault($"<graph-delta-rejected:{Detail}:{InvariantCode}>", 8300);
    public sealed record StreamVersionConflict(ModelId Model, long Expected, long Actual) : GraphFault($"<graph-version-conflict:{Model.Value}:{Expected}!={Actual}>", 8301);
    public sealed record ModelAbsent(ModelId Model) : GraphFault($"<graph-model-absent:{Model.Value}>", 8302);
}

// --- [MODELS] --------------------------------------------------------------------------
// The inline single-stream aggregate — it IS the rehydrated `ElementGraph`. Marten folds the
// model stream into this document inside the append transaction, so a same-unit `Read` is
// read-your-writes consistent. `GraphDelta.ReplayOnto` is the one immutable fold the AS-OF
// reconstruction also runs, so a folded graph is bit-identical to the live state at that version.
public sealed record GraphProjection(ModelId Model, ElementGraph Graph, long Version) {
    public static GraphProjection Create(IEvent<GraphEvent.GraphCreated> e) =>
        new(ModelId.Create(e.StreamId), e.Data.Delta.ReplayOnto(ElementGraph.Of(e.Data.Header, FrozenDictionary<NodeId, Node>.Empty, ImmutableArray<Relationship>.Empty)), 1L);

    public GraphProjection Apply(GraphEvent.GraphRevised e) =>
        this with { Graph = e.Delta.ReplayOnto(Graph), Version = Version + 1L };

    public GraphProjection Apply(GraphEvent.GraphRetired e) =>
        this with { Graph = e.Delta.ReplayOnto(Graph), Version = Version + 1L };
}
```

| [INDEX] | [POLICY]                | [VALUE]                                | [BINDING]                                                  |
| :-----: | :---------------------- | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | authoritative read      | inline `GraphProjection.Graph`         | read-your-writes; never an async lane                     |
|  [02]   | replay bound            | inline `AggregateSnapshot`             | deep stream folds from the latest snapshot + suffix       |
|  [03]   | one materializer        | `GraphDelta.ReplayOnto`                     | projection and AS-OF reconstruction fold the one delta    |
|  [04]   | working vs frozen graph | HAMT delta path, `FrozenDictionary` snapshot | O(log n) authoring, O(1) read snapshot              |
|  [05]   | cross-model rollup      | `MultiStreamProjection`                | sliced by project id, never a second delta fold           |

## [04]-[STORE_RAIL]

- Owner: `GraphStoreOp` the `[Union]` operation family every durable graph interaction is a value in; `GraphStore` the static surface owning the one bracket — pooled session acquisition, the strong-typed append, the inline-projection read, the AS-OF fold, the co-transactional identity commit, and provider-fault conversion to `GraphFault`; `GraphReceipt` the typed per-op evidence carrying the model, the resulting version, and the elapsed `Duration`.
- Cases: `Open(ModelId, Header)` starts a stream, `Commit(ModelId, GraphDelta, long Expected)` appends a `GraphRevised`, `Retire(ModelId, GraphDelta, string Reason, long Expected)` appends a `GraphRetired` under the SAME optimistic version guard (never a hardcoded `long.MaxValue` that bypasses concurrency), `Read(ModelId)` returns the inline-folded `ElementGraph` (read-your-writes), `ReadAsOf(ModelId, TimeCut)` folds `AggregateStreamAsync` to a `version`/`timestamp` cut, `State(ModelId)` returns the head `StreamState` without folding — one op family discriminated by the input value's shape, never a repository per concern.
- Entry: `public static IO<Fin<GraphReceipt>> Run(IDocumentSession session, ElementIdentity identity, GraphStoreOp op, ClockPolicy clocks, CorrelationId correlation)` is the one rail — every write op stores the `ElementIdentity` row through `session.Store(identity)` in the SAME `IDocumentSession` before `SaveChangesAsync` so identity plus event commit atomically; `public static IO<Option<ElementGraph>> ReadAsOf(IQuerySession session, ModelId model, TimeCut cut)` folds the stream to the cut through `AggregateStreamAsync<GraphProjection>(model.Value, version: cut.Version, timestamp: cut.Instant)`.
- Auto: the bracket acquires a session from the `IDocumentStore`, runs the op, and `SaveChangesAsync` commits events plus the identity document plus the inline projection in one Postgres transaction — there is no separate identity ORM and no two-phase dance because the identity rides the same session as a Marten document; the read op calls `FetchLatest<GraphProjection>(model)` which returns the inline document when present or live-folds the tail, so a read after a commit in the same unit is consistent; the AS-OF op binds either a `version` or a `timestamp` (one or the other, never both) from the `TimeCut` so an historical read folds the SAME `GraphDelta.ReplayOnto` deterministically; provider exceptions convert to `GraphFault` at the one bracket boundary and the interior never sees a raw `MartenCommandException`, while caller cancellation passes through untyped.
- Receipt: an `Open`/`Commit`/`Retire` rides `store.element.<verb>` carrying the resulting `StreamAction.Version`; a `Read`/`ReadAsOf` rides `store.element.read` carrying the folded node count; the identity co-commit rides `store.element.identity` carrying the `NodeId` count.
- Packages: Marten (`IDocumentSession`/`IQuerySession`/`SaveChangesAsync`/`FetchLatest`/`AggregateStreamAsync`/`FetchStreamStateAsync`/`Store`), LanguageExt.Core (`IO`/`Fin`), NodaTime, BCL inbox.
- Growth: a new durable interaction is one `GraphStoreOp` case plus one bracket arm the total dispatch forces; a new read modality is one op case discriminating on its input (a key resolves to one graph, a cut to an AS-OF graph, a state probe to a head version); zero new surface — a repository per model, a per-verb service, an injected `persist` delegate, or a separate identity transaction is the deleted form because the one rail owns the bracket, the one session owns identity-plus-event atomicity, and the op family discriminates by value shape.
- Boundary: the one transaction owner for identity plus event is the `IDocumentSession` — the `ElementIdentity` row stores as a Marten document via `session.Store(identity)` in the same session as the appended events, so a single `SaveChangesAsync` commits both with no free two-ORM atomicity and no EF-versus-Marten gap (the EF identity model of `Element/identity` is the relational projection the Marten document feeds, queried through Npgsql for the H3/pgvector/ACL columns, but the WRITE of record is the one Marten session); the read op is read-your-writes through the inline projection and NEVER routes to an async analytical lane; the AS-OF op binds `version` XOR `timestamp` so a precise cut pins a version and an instant cut binds the wall clock, and the fold reuses the one `GraphDelta.ReplayOnto` so an historical graph equals the live state; optimistic concurrency is `Commit(model, delta, expectedVersion)` whose `StreamAction.ExpectedVersionOnServer` aborts a racing same-version writer at `SaveChangesAsync` as `GraphFault.StreamVersionConflict`, and a multi-writer-hostile section escalates to `FetchForExclusiveWriting<GraphProjection>(model)` for a stream-level advisory lock; `SaveChangesAsync` is the only commit and the bracket never bypasses it; provider failure converts to `GraphFault` once at the bracket and the op-log changefeed (`Version/ledger#CHANGEFEED`) projects FROM the committed events, never a trigger-based second write path; a re-ingest of an existing model aligns the projected `GraphDelta`'s freshly-minted rooted `NodeId`s to the durable ids through `Version/merge#STRUCTURAL_DIFF` `Reconcile` (correlating on `Node.Object.ExternalId`, the 1:1 IFC GlobalId, since the neutral `NodeId` is freshly minted each ingest) BEFORE `Commit`, so a re-import revises the existing stream rather than forking a duplicate model.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
// `TimeCut` is the one temporal-cut value-object owned by `Version/timetravel#TIME_TRAVEL`: the
// inclusive `Hlc` ceiling plus the optional Marten stream version. The Marten stream fold binds the
// version when present, else the `Ceiling.Physical` instant — so the engine HLC-cell cut and the
// stream-version cut are one concept, never two parallel cut types.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphStoreOp {
    private GraphStoreOp() { }

    public sealed record Open(ModelId Model, Header Header) : GraphStoreOp;
    public sealed record Commit(ModelId Model, GraphDelta Delta, long Expected) : GraphStoreOp;
    public sealed record Retire(ModelId Model, GraphDelta Delta, string Reason, long Expected) : GraphStoreOp;
    public sealed record Read(ModelId Model) : GraphStoreOp;
    public sealed record ReadAsOf(ModelId Model, TimeCut Cut) : GraphStoreOp;
    public sealed record State(ModelId Model) : GraphStoreOp;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct GraphReceipt(string Slot, ModelId Model, long Version, int Nodes, int Edges, Duration Elapsed, Instant At, CorrelationId Correlation);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GraphStore {
    public static IO<Fin<GraphReceipt>> Run(IDocumentSession session, ElementIdentity identity, GraphStoreOp op, ClockPolicy clocks, CorrelationId correlation) =>
        from mark in IO.lift(clocks.Mark)
        from outcome in op switch {
            GraphStoreOp.Open o => Commit(session, identity, o.Model, GraphDelta.Empty, 0L, _ => ElementSchema.Open(session, o.Model, o.Header), correlation, clocks, mark, "store.element.open"),
            GraphStoreOp.Commit c => Commit(session, identity, c.Model, c.Delta, c.Expected, _ => ElementSchema.Append(session, c.Model, new GraphEvent.GraphRevised(c.Delta), c.Expected), correlation, clocks, mark, "store.element.commit"),
            GraphStoreOp.Retire t => Commit(session, identity, t.Model, t.Delta, t.Expected, _ => ElementSchema.Append(session, t.Model, new GraphEvent.GraphRetired(t.Delta, t.Reason), t.Expected), correlation, clocks, mark, "store.element.retire"),
            GraphStoreOp.Read r => ReadGraph(session, r.Model, clocks, mark, correlation),
            GraphStoreOp.ReadAsOf a => ReadGraphAsOf(session, a.Model, a.Cut, clocks, mark, correlation),
            GraphStoreOp.State s => ReadState(session, s.Model, clocks, mark, correlation),
            _ => IO.pure(Fin<GraphReceipt>.Fail(new GraphFault.ModelAbsent(default))),
        }
        select outcome;

    public static IO<Option<ElementGraph>> ReadAsOf(IQuerySession session, ModelId model, TimeCut cut) =>
        ProjectAsOf(session, model, cut).Map(o => o.Map(static p => p.Graph));

    static IO<Option<GraphProjection>> ProjectAsOf(IQuerySession session, ModelId model, TimeCut cut) =>
        IO.liftAsync(() => session.Events.AggregateStreamAsync<GraphProjection>(
                model.Value,
                version: cut.StreamVersion.IfNone(0L),
                timestamp: cut.StreamVersion.IsSome ? (DateTimeOffset?)null : cut.At.ToDateTimeOffset()))
            .Map(Optional);

    static IO<Fin<GraphReceipt>> Commit(IDocumentSession session, ElementIdentity identity, ModelId model, GraphDelta delta, long expected, Func<Unit, StreamAction> stage, CorrelationId correlation, ClockPolicy clocks, long mark, string slot) =>
        IO.liftAsync(async () => {
            var action = stage(unit);
            session.Store(identity);
            await session.SaveChangesAsync().ConfigureAwait(false);
            return Fin<GraphReceipt>.Succ(new GraphReceipt(slot, model, action.Version, delta.NodeCount, delta.EdgeCount, clocks.Elapsed(mark), clocks.Now, correlation));
        }) | @catch<IO, Fin<GraphReceipt>>(static _ => true, error => Lift(session, model, expected, error));

    static IO<Fin<GraphReceipt>> ReadGraph(IDocumentSession session, ModelId model, ClockPolicy clocks, long mark, CorrelationId correlation) =>
        IO.liftAsync(() => session.Events.FetchLatest<GraphProjection>(model.Value))
            .Map(p => Optional(p).Match(
                Some: projection => Fin<GraphReceipt>.Succ(new GraphReceipt("store.element.read", model, projection.Version, projection.Graph.Nodes.Count, projection.Graph.Edges.Count, clocks.Elapsed(mark), clocks.Now, correlation)),
                None: () => Fin<GraphReceipt>.Fail(new GraphFault.ModelAbsent(model))));

    static IO<Fin<GraphReceipt>> ReadGraphAsOf(IDocumentSession session, ModelId model, TimeCut cut, ClockPolicy clocks, long mark, CorrelationId correlation) =>
        ProjectAsOf(session, model, cut).Map(projection => projection.Match(
            Some: p => Fin<GraphReceipt>.Succ(new GraphReceipt("store.element.read", model, p.Version, p.Graph.Nodes.Count, p.Graph.Edges.Count, clocks.Elapsed(mark), clocks.Now, correlation)),
            None: () => Fin<GraphReceipt>.Fail(new GraphFault.ModelAbsent(model))));

    static IO<Fin<GraphReceipt>> ReadState(IDocumentSession session, ModelId model, ClockPolicy clocks, long mark, CorrelationId correlation) =>
        IO.liftAsync(() => session.Events.FetchStreamStateAsync(model.Value))
            .Map(state => Optional(state).Match(
                Some: s => Fin<GraphReceipt>.Succ(new GraphReceipt("store.element.read", model, s.Version, 0, 0, clocks.Elapsed(mark), clocks.Now, correlation)),
                None: () => Fin<GraphReceipt>.Fail(new GraphFault.ModelAbsent(model))));

    static IO<Fin<GraphReceipt>> Lift(IDocumentSession session, ModelId model, long expected, Error error) =>
        error.Exception.Match(
            Some: ex => ex is JasperFx.ConcurrencyException or JasperFx.Events.EventStreamUnexpectedMaxEventIdException
                ? IO.liftAsync(() => session.Events.FetchStreamStateAsync(model.Value))
                    .Map(state => Fin<GraphReceipt>.Fail(new GraphFault.StreamVersionConflict(model, expected, Optional(state).Match(Some: static s => s.Version, None: static () => 0L))))
                : IO.pure(Fin<GraphReceipt>.Fail(new GraphFault.DeltaRejected(ex.Message, 0))),
            None: () => IO.pure(Fin<GraphReceipt>.Fail(new GraphFault.DeltaRejected(error.Message, error.Code))));
}
```

| [INDEX] | [POLICY]                 | [VALUE]                                  | [BINDING]                                                  |
| :-----: | :----------------------- | :--------------------------------------- | :-------------------------------------------------------- |
|  [01]   | one txn owner            | identity + event in one `IDocumentSession` | `session.Store(identity)` then `SaveChangesAsync`        |
|  [02]   | read consistency         | `FetchLatest<GraphProjection>`           | inline document or live tail fold; read-your-writes       |
|  [03]   | AS-OF fold               | `AggregateStreamAsync(version\|timestamp)` | version XOR instant; reuses `GraphDelta.ReplayOnto`           |
|  [04]   | optimistic concurrency   | `Commit(model, delta, expectedVersion)`  | racing same-version writer → `StreamVersionConflict`      |
|  [05]   | fault conversion         | one bracket boundary                      | provider exception → `GraphFault`; cancellation untyped   |
