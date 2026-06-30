# [PERSISTENCE_ELEMENT_GRAPH]

Rasm.Persistence persists the one `Rasm.Element` `ElementGraph` through Marten on PostgreSQL as the write system of record: each model (or spatial partition) is ONE event stream keyed by `ModelId`, every durable change is a `GraphDelta` event body (`GraphCreated` opens the stream with the `Header`, `GraphRevised` extends it, `GraphRetired` carries the convergent retirement delta), and the whole `ElementGraph` rehydrates by folding the stream through an INLINE `SingleStreamProjection` written in the SAME `IDocumentSession` transaction as the events — so authoritative containment and topology are read-your-writes consistent and never route to an async lane (`Query/lane#READ_ROUTING`). AS-OF reconstruction at the graph altitude is `AggregateStreamAsync<GraphProjection>(version|timestamp)`, the inline aggregate IS the materialized read view bounding replay, and the relational `Element/identity#ELEMENT_IDENTITY` `ElementIdentity` row commits atomically with the event by riding the SAME session as a Marten document — one transaction owns identity plus event with no two-ORM gap. The event body carries the seam-validated `GraphDelta`, never a whole-graph snapshot; the seam's ONE `GraphDelta.ReplayOnto` fold is the only delta→graph materializer the inline projection AND the AS-OF reconstruction both run, so a rehydrated graph is bit-identical to the live state at that version. The inline projection document is NOT the seam `ElementGraph` itself (a sealed read-snapshot class with no deserialization path) but its STJ-rehydratable primitives (`Header` + the node map + the edge array + the folded version), materializing the frozen `ElementGraph` once through the seam `ElementGraph.Of` at the read boundary. `ClockPolicy` and `CorrelationId` arrive settled from `Rasm.AppHost`; the one `ElementJson` STJ serializer arrives from `Element/codec#CODEC_AXIS`; the `Element/identity#ELEMENT_IDENTITY` `ElementIdentity` row and its `IdentityStore.Stamp` co-commit arrive from the identity tier; the typed value/graph vocabulary (`ElementGraph`, `Header`, `Node`, `NodeId`, `Relationship`, `GraphDelta`, `GraphDelta.ReplayOnto`, `ElementGraph.Genesis`) arrives settled from `Rasm.Element`; the re-ingest `Version/merge#STRUCTURAL_DIFF` `Reconcile` aligns a fresh import's neutral `NodeId`s onto the durable ids BEFORE the delta reaches this store.

## [01]-[INDEX]

- [01]-[STREAM_GRAIN]: model-stream identity, the `GraphEvent` body family, optimistic append, and the schema-keyed event registration.
- [02]-[GRAPH_PROJECTION]: the inline `SingleStreamProjection` folding `GraphDelta` into the STJ-rehydratable `GraphProjection` over the seam `GraphDelta.ReplayOnto`, the materialized `ElementGraph` read boundary, and the read-your-writes consistency boundary.
- [03]-[STORE_RAIL]: the one `GraphStoreOp` operation family over the generated total `Switch`, the session bracket, AS-OF reconstruction, and the co-transactional identity commit.

## [02]-[STREAM_GRAIN]

- Owner: `ModelId` the `[ValueObject<Guid>]` per-model stream key under the `IObjectFactory` floor; `GraphEvent` the `[Union]` event-body family every model stream appends, carrying the `Body`/`Lifecycle` projections the `Version/ledger#CHANGEFEED` `OpLog.Project` reads off each Marten event; `EventLifecycle` the `[SmartEnum<string>]` create/revise/retire verb each event row carries; `ElementSchema` the static surface owning the `StoreOptions` event registration, the strong-typed value registration, the inline projection registration, and the per-model stream-start and append legs over the one `IDocumentSession`.
- Cases: `GraphCreated(Header Header, GraphDelta Delta)` opens a stream carrying the `Rasm.Element` `Header` (`ReleaseVersion`/`ModelView`/`GeoReference`/`Tolerance`/`Instant`/`StepHeader`) AND the assembled opening `GraphDelta` (the `Projection/projection#PROJECTION_CONTRACT` `Assemble` merged model-creating delta), so a model is created in ONE event rather than an empty open plus a separate content commit; `GraphRevised(GraphDelta Delta)` is the steady-state append; `GraphRetired(GraphDelta Delta, string Reason)` carries the retirement delta whose `GraphDelta` removes the retired nodes/edges — so retirement is a real convergent delta the projection folds, never an out-of-band tombstone; the event body is ALWAYS the seam `GraphDelta`, NEVER a whole-graph snapshot, because the delta replays deterministically through `GraphDelta.ReplayOnto` and a whole-graph body bloats every append by the model size.
- Entry: `public static StoreOptions Configure(StoreOptions opts, NpgsqlDataSource source)` registers the event types, the strong-typed `ModelId`/`NodeId` value types, the inline `GraphProjection` self-aggregating snapshot, and the metadata columns once at boot; `public static StreamAction Open(IDocumentSession session, ModelId model, Header header, GraphDelta opening)` calls `session.Events.StartStream<GraphProjection>(model.Value, new GraphEvent.GraphCreated(header, opening))` so the assembled opening delta is the one model-creating event; `public static StreamAction Append(IDocumentSession session, ModelId model, GraphEvent body, long expectedVersion)` appends with the inline optimistic version guard so a concurrent writer racing the same stream version aborts at `SaveChangesAsync` rather than silently interleaving.
- Auto: the stream identity is `StreamIdentity.AsGuid` keyed by `ModelId` so a model is one stream and a spatial partition is one stream — never per-`NodeId`, because a node-grain stream multiplies stream count by element count and forecloses the whole-graph fold; `EventAppendMode.Rich` keeps the full causation/correlation metadata on the authoring path while bulk re-ingest switches to `QuickWithServerTimestamps` for append throughput; the `GraphDelta` the event carries is the SAME value the seam's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` admitted at the projector boundary, so the projection apply is total over admitted deltas; `RegisterValueType<ModelId>()`/`RegisterValueType<NodeId>()` teach Marten the `[ValueObject]` keys so the stream key and every document id stay typed end to end and never decay to a bare `Guid`/`string` at the wire; `UseSystemTextJsonForSerialization(ElementJson.Options, …)` binds the one `Element/codec#CODEC_AXIS` STJ serializer so a stored `GraphEvent`, the inline `GraphProjection`, and an inspector projection share one Thinktecture converter set.
- Receipt: a stream open rides `store.element.open`, a delta append rides `store.element.commit` carrying the delta node/edge counts, a retirement rides `store.element.retire`; the `StreamAction.Version` is the optimistic guard the next append reads.
- Packages: Marten (`StartStream`/`Append`/`StreamAction`/`EventAppendMode`/`StreamIdentity`/`RegisterValueType`/`Snapshot`/`UseSystemTextJsonForSerialization`/`MetadataConfig`), Npgsql, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new durable change kind is one `GraphEvent` case plus one `EventLifecycle` row plus one projection `Apply` method the convention discovery forces; a richer model header is one field on the seam `Header`; a per-spatial-partition grain is one `ModelId` minting policy, never a second stream shape; zero new surface — a per-`NodeId` stream, a whole-graph event body, a second event table, or a bespoke `OpLogEntry` store beneath Marten is the deleted form because Marten owns the durable append and the rebuildable read views, and the `Version/` engine projects FROM these events (`Version/ledger#CHANGEFEED` `OpLog.Project` over `e.Data.Body`/`e.Data.Lifecycle`).
- Boundary: stream grain is ONE stream PER MODEL (or per spatial partition), never per-`NodeId`, and the event body is the `GraphDelta`, never a whole-graph snapshot; the `GraphDelta` is the seam-owned graph-mutation record the projection folds immutably through `GraphDelta.ReplayOnto`, so the durable history is a delta log the engine replays and the rehydrated graph is bit-identical to the live state at any version because the fold is the ONE `GraphDelta.ReplayOnto` the AS-OF reconstruction also runs; the optimistic append (`Append(stream, expectedVersion, …)`) is the inline guard, `AppendOptimistic` the read-then-guard, and `FetchForExclusiveWriting<GraphProjection>(model)` the stream-level advisory lock a multi-writer-hostile section escalates to; the `GraphRetired` delta is a real convergent retirement the projection folds and the `Version/retention#RETENTION_CLASSES` sweep reclaims, never an `ArchiveStream` that hides the events from the fold (archive is the AS-OF cut boundary, not retirement); a `GraphCreated` carries the `Header` so the stream's `ReleaseVersion`/`GeoReference`/`Tolerance` are the first folded fact and every later delta's measure quantization (`Element/codec#CONTENT_ADDRESS`) reads the header tolerance; `EventAppendMode` trades metadata richness for throughput as a config value, never a per-call branch; the `GraphEvent` is the body family `Version/ledger#CHANGEFEED` lifts (`OpLog.Project(IEvent<GraphEvent>)` reads `e.Data.Body`/`e.Data.Lifecycle`), so this owner's body shape is the changefeed's input contract.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using JasperFx.Events;
using LanguageExt;
using LanguageExt.Common;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using NodaTime;
using Npgsql;
using Rasm.Element;
using Thinktecture;
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

namespace Rasm.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<Guid>]
public readonly partial struct ModelId {
    public static ModelId New() => Create(Guid.CreateVersion7());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EventLifecycle {
    public static readonly EventLifecycle Created = new("created");
    public static readonly EventLifecycle Revised = new("revised");
    public static readonly EventLifecycle Retired = new("retired");
}

// --- [MODELS] --------------------------------------------------------------------------
// The event body family every model stream appends. The body is ALWAYS the seam `GraphDelta`
// (the validated graph-mutation record), never a whole-graph snapshot — `GraphCreated` adds the
// opening `Header`, `GraphRetired` adds the retirement reason, and all three fold through the one
// `GraphDelta.ReplayOnto` the projection runs, so the durable history is a deterministic delta log.
// `Body`/`Lifecycle` are the projections `Version/ledger#CHANGEFEED` `OpLog.Project` reads off each
// committed `IEvent<GraphEvent>`, so this body shape is the changefeed's input contract.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphEvent {
    private GraphEvent() { }

    public sealed record GraphCreated(Header Header, GraphDelta Delta) : GraphEvent;
    public sealed record GraphRevised(GraphDelta Delta) : GraphEvent;
    public sealed record GraphRetired(GraphDelta Delta, string Reason) : GraphEvent;

    public GraphDelta Body => this.Switch(
        graphCreated: static c => c.Delta,
        graphRevised: static r => r.Delta,
        graphRetired: static t => t.Delta);

    public EventLifecycle Lifecycle => this.Switch(
        graphCreated: static _ => EventLifecycle.Created,
        graphRevised: static _ => EventLifecycle.Revised,
        graphRetired: static _ => EventLifecycle.Retired);
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class ElementSchema {
    public static StoreOptions Configure(StoreOptions opts, NpgsqlDataSource source) {
        ArgumentNullException.ThrowIfNull(opts);
        ArgumentNullException.ThrowIfNull(source);
        opts.Connection(source);
        opts.Events.StreamIdentity = StreamIdentity.AsGuid;
        opts.Events.AppendMode = EventAppendMode.Rich;
        opts.Events.MetadataConfig.CausationIdEnabled = true;
        opts.Events.MetadataConfig.CorrelationIdEnabled = true;
        opts.Events.MetadataConfig.HeadersEnabled = true;
        opts.UseSystemTextJsonForSerialization(ElementJson.Options, EnumStorage.AsString, Casing.CamelCase);
        opts.RegisterValueType<ModelId>();
        opts.RegisterValueType<NodeId>();
        // ONE registration: `GraphProjection` is the self-aggregating inline snapshot — its `Create`/`Apply`
        // convention methods fold the stream into the document written in the SAME append transaction, so a
        // read-your-writes interactive query (`Query/lane#READ_ROUTING`) reads the head with no daemon lag.
        // A second `Projections.Add<GraphProjection>` is the deleted double-registration (it would re-treat the
        // aggregate as a raw `IProjection`); the inline aggregate IS the materialized view that bounds replay.
        opts.Projections.Snapshot<GraphProjection>(SnapshotLifecycle.Inline);
        return opts;
    }

    // The model-creating event carries BOTH the `Header` AND the assembled opening `GraphDelta` (the one
    // `Projection/projection#PROJECTION_CONTRACT` `Assemble` merged delta, header folded in via `Reheader`),
    // so a model is created in ONE event the inline projection's `Create` folds — never an empty open plus a
    // separate content commit. A from-scratch model opens with `GraphDelta.Empty` at the call site.
    public static StreamAction Open(IDocumentSession session, ModelId model, Header header, GraphDelta opening) {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(opening);
        return session.Events.StartStream<GraphProjection>(model.Value, new GraphEvent.GraphCreated(header, opening));
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

- Owner: `GraphProjection` the inline self-aggregating snapshot AGGREGATE Marten folds one model stream into (registered through `opts.Projections.Snapshot<GraphProjection>(SnapshotLifecycle.Inline)` — the document carries the `Create`/`Apply` convention methods Marten discovers and wraps as a single-stream projection internally, so the record is the aggregate, never a hand-derived `SingleStreamProjection<,>` subclass) — the STJ-rehydratable carrier of one model's `Header`, node map, edge array, and folded version, written in the append transaction, materializing the seam `ElementGraph` ONCE through `ElementGraph.Of` at the read boundary; `GraphFault` the `[Union]` projection/store fault band (830x) deriving from the kernel `Rasm.Domain.Expected`; the aggregate's `Create`/`Apply` convention methods owning the one `GraphDelta.ReplayOnto` fold over the seam graph.
- Cases: `Create(GraphCreated)` seeds the genesis through the seam `ElementGraph.Genesis(header)` then replays the opening delta through `GraphDelta.ReplayOnto`; `Apply(GraphRevised, GraphProjection)` and `Apply(GraphRetired, GraphProjection)` replay their `GraphDelta` onto the materialized graph; the fold is the seam `GraphDelta.ReplayOnto : ElementGraph → ElementGraph`, TOTAL over the seam-validated delta because the projector's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` and the seam structural `Graph/delta#GRAPH_DELTA` `LegalLink` gated it at the write boundary (`ReplayOnto` re-applies the recorded add/remove/revise RAW, never re-validating), so a malformed delta in the stream is a deployment defect the `#STORE_RAIL` bracket surfaces as `GraphFault.DeltaRejected`, not a recoverable per-fold rail.
- Entry: the Marten convention methods `public static GraphProjection Create(IEvent<GraphEvent.GraphCreated> e)` (seeding `Model` from the event's `StreamId`, never an empty key), `public GraphProjection Apply(GraphEvent.GraphRevised e)`, `public GraphProjection Apply(GraphEvent.GraphRetired e)` — Marten discovers them by convention; `public ElementGraph Graph` materializes the rehydrated authoritative graph the synchronous reads serve, memoized so the frozen snapshot (incidence index + `QuikGraph` view + `Bake` memo, all built once by `ElementGraph.Of`) is built once per loaded projection.
- Auto: the projection registers `SnapshotLifecycle.Inline` so the folded `GraphProjection` document is written in the SAME transaction as the appended events — a `Read` after a `Commit` in the same logical unit sees the new state with no daemon lag — and the inline aggregate IS the periodic materialized view, so a deep stream loads the head document rather than re-folding from genesis; the projection stores the STJ-serializable primitives (`Header`, `ImmutableDictionary<NodeId, Node>`, `ImmutableArray<Relationship>`) because the seam `ElementGraph` is a sealed read-snapshot class with no deserialization path, and the live authoring graph uses the seam's `ImmutableDictionary`/HAMT structural-sharing form (`Graph/delta#GRAPH_DELTA`) while `ElementGraph.Of` freezes to `FrozenDictionary` + the incidence index + the lazy `QuikGraph` view only at the `Graph` materialization boundary, so the delta path stays O(log n) structural-sharing and the read snapshot stays O(1) lookup; `From` is the ONLY mint (each `Create`/`Apply` rebuilds the document and the lazy `Graph` memo from the folded snapshot) so a `with` can never alias a stale materialized graph.
- Receipt: a projection fold rides `store.element.project` carrying the folded delta count; a `GraphFault.DeltaRejected` rides `store.element.fault` carrying the rejecting delta detail and the seam structural-invariant code.
- Packages: Marten (`SingleStreamProjection`/`SnapshotLifecycle`/`IEvent<T>`), Rasm.Element (`ElementGraph`/`ElementGraph.Genesis`/`ElementGraph.Of`/`GraphDelta`/`GraphDelta.ReplayOnto`/`Header`/`Node`/`NodeId`/`Relationship`), LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Collections.Immutable/Frozen, BCL inbox.
- Growth: a new event arm is one projection `Apply` method the convention discovery forces; a cross-model rollup (a whole-project aggregate) is one `MultiStreamProjection<ProjectGraph, ProjectId>` slicing by `Identity<GraphEvent>(e => …)`, never a second fold of the same delta; a co-transactional columnar egress is the `Query/columnar#COLUMNAR_LANE` `FlatTableProjection`; zero new surface — a hand-rolled stream folder, a second materializer, or a per-read whole-stream replay is the deleted form because the inline projection IS the materialized read and the AS-OF fold reuses the same `GraphDelta.ReplayOnto`.
- Boundary: the inline projection is the READ-YOUR-WRITES consistency boundary — authoritative containment, topology, and void-resolution reads go through this folded `GraphProjection.Graph`, NEVER an async lane, because an async daemon view lags the write (`Query/lane#READ_ROUTING` routes interactive correctness here by construction); the analytical lanes (`Query/columnar`, `Query/cypher`) are explicitly `ProjectionLifecycle.Async` with a staleness watermark and interactive-correctness reads block on `WaitForNonStaleProjectionDataAsync`; the projection apply is the SAME `GraphDelta.ReplayOnto` fold the `Version/timetravel#TIME_TRAVEL` AS-OF reconstruction runs (the live authoring path produces the deltas it replays, via the seam `Graph/delta#GRAPH_DELTA` `WorkingGraph.Apply`), so there is exactly ONE delta→graph materializer and a historical fold equals the live state field-for-field; the projection NEVER stores the seam `ElementGraph` directly (it has no public deserialization constructor — a sealed read-snapshot class whose only mint is `Of`/`Genesis`/`Apply`), so the document carries the rehydratable `Header`/node-map/edge-array and `Graph` materializes the frozen snapshot once through `ElementGraph.Of`; the inline aggregate is the materialized read floor bounding replay, never a second source of truth — `store.Advanced.RebuildSingleStreamAsync<GraphProjection>(model)` replays one stream's inline projection from zero when the fold logic changes; the projection never re-validates the delta because the projector `IGraphConstraint` and the seam `LegalLink` already gated it at the write boundary — re-validation in the projection is the deleted form because a validated delta in the stream is total by construction and a fold-time fault is a deployment defect surfaced as `GraphFault`, not a recoverable data path.

```csharp signature
// --- [ERRORS] --------------------------------------------------------------------------
// The projection/store fault band (830x): a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the seam
// `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND` `BimFault`
// (2600) realize — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)` ctor (no
// `Category` to override) is the deleted form. Band membership is a per-case `Code => 830x` override, `Message`/
// `Category` projecting through the generated `Switch`, so the typed case lifts BARE onto `Fin<T>`/`Validation<Error,T>`
// with no `.ToError()` hop and a recovery reads `error.IsType<GraphFault.StreamVersionConflict>()` / `error.HasCode(8301)`
// / `error.Category()`, never a message substring. `[SkipUnionOps]` is the canonical fault-band annotation (the
// production `UiFault` shape) — it skips the generated implicit-conversion ops while the generated `Switch`/`Map` survives.
[SkipUnionOps]
[Union]
public abstract partial record GraphFault : Expected, IValidationError<GraphFault> {
    private GraphFault() : base() { }

    public sealed record DeltaRejected(string Detail, int InvariantCode) : GraphFault;
    public sealed record StreamVersionConflict(ModelId Model, long Expected, long Actual) : GraphFault;
    public sealed record ModelAbsent(ModelId Model) : GraphFault;

    public override int Code => Switch(
        deltaRejected:         static _ => 8300,
        streamVersionConflict: static _ => 8301,
        modelAbsent:           static _ => 8302);

    public override string Message => Switch(
        deltaRejected:         static c => $"<graph-delta-rejected:{c.Detail}:{c.InvariantCode}>",
        streamVersionConflict: static c => $"<graph-version-conflict:{c.Model.Value}:{c.Expected}!={c.Actual}>",
        modelAbsent:           static c => $"<graph-model-absent:{c.Model.Value}>");

    public override string Category => Switch(
        deltaRejected:         static _ => "Delta",
        streamVersionConflict: static _ => "Concurrency",
        modelAbsent:           static _ => "Absent");

    public static GraphFault Create(string message) => new DeltaRejected(message, 0);
}

// --- [MODELS] --------------------------------------------------------------------------
// The inline single-stream aggregate. Marten folds the model stream into this document inside the
// append transaction, so a same-unit `Read` is read-your-writes consistent. It carries the STJ-
// rehydratable PRIMITIVES (`Header` + node map + edge array + version), NOT the seam `ElementGraph`
// — a sealed read-snapshot class with no deserialization path whose only mint is `Of`/`Genesis`/
// `Apply` — and materializes the frozen `ElementGraph` ONCE through the seam `ElementGraph.Of` via
// the memoized `Graph` accessor (the one place the incidence index, `QuikGraph` view, and `Bake`
// memo build). `GraphDelta.ReplayOnto` is the one immutable fold the AS-OF reconstruction also runs,
// so a folded graph is bit-identical to the live state at that version. `From` is the ONLY mint, so
// no `with` copy can alias the materialized `graph` cache across folds.
public sealed record GraphProjection(
    ModelId Model, Header Header, ImmutableDictionary<NodeId, Node> Nodes, ImmutableArray<Relationship> Edges, long Version) {

    [JsonIgnore] ElementGraph? graph;
    public ElementGraph Graph => graph ??= ElementGraph.Of(Header, Nodes.ToFrozenDictionary(), Edges);

    public static GraphProjection Create(IEvent<GraphEvent.GraphCreated> e) =>
        From(ModelId.Create(e.StreamId), e.Data.Delta.ReplayOnto(ElementGraph.Genesis(e.Data.Header)), 1L);

    public GraphProjection Apply(GraphEvent.GraphRevised e) => From(Model, e.Delta.ReplayOnto(Graph), Version + 1L);

    public GraphProjection Apply(GraphEvent.GraphRetired e) => From(Model, e.Delta.ReplayOnto(Graph), Version + 1L);

    static GraphProjection From(ModelId model, ElementGraph folded, long version) =>
        new(model, folded.Header, folded.Nodes.ToImmutableDictionary(), folded.Edges, version);
}
```

| [INDEX] | [POLICY]                | [VALUE]                                | [BINDING]                                                  |
| :-----: | :---------------------- | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | authoritative read      | inline `GraphProjection.Graph`         | read-your-writes; never an async lane                     |
|  [02]   | replay floor            | inline self-aggregating snapshot       | head document loads, never a genesis re-fold per read     |
|  [03]   | one materializer        | seam `GraphDelta.ReplayOnto`           | projection and AS-OF reconstruction fold the one delta    |
|  [04]   | serializable document   | `Header`/node-map/edge-array primitives| `ElementGraph` has no STJ ctor; `Of` materializes once    |
|  [05]   | cross-model rollup      | `MultiStreamProjection`                | sliced by project id, never a second delta fold           |

## [04]-[STORE_RAIL]

- Owner: `GraphStoreOp` the `[Union]` operation family every durable graph interaction is a value in; `GraphStore` the static surface owning the one bracket over the generated total `Switch` — pooled session acquisition, the strong-typed append, the inline-projection read, the AS-OF fold, the co-transactional identity commit, and provider-fault conversion to `GraphFault`; `GraphReceipt` the typed per-op evidence carrying the model, the resulting version, and the elapsed `Duration`.
- Cases: `Open(ModelId, Header, GraphDelta Opening)` starts a stream carrying the header plus the assembled opening delta (the one model-creating event), `Commit(ModelId, GraphDelta, long Expected)` appends a `GraphRevised`, `Retire(ModelId, GraphDelta, string Reason, long Expected)` appends a `GraphRetired` under the SAME optimistic version guard (never a hardcoded `long.MaxValue` that bypasses concurrency), `Read(ModelId)` returns the inline-folded `ElementGraph` (read-your-writes), `ReadAsOf(ModelId, TimeCut)` folds `AggregateStreamAsync` to a `version`/`timestamp` cut, `State(ModelId)` returns the head `StreamState` without folding — one op family discriminated by the input value's shape through the generated total `Switch`, never a repository per concern.
- Entry: `public static IO<Fin<GraphReceipt>> Run(IDocumentSession session, ElementIdentity identity, GraphStoreOp op, Principal actor, Guid storeId, ClockPolicy clocks, CorrelationId correlation)` is the one rail — every write op STAMPS the `actor`/`origin` blame headers (`session.SetHeader("actor", actor.Subject)`/`SetHeader("origin", storeId.ToString())`, the WRITE side of the blame contract `Version/ledger#CHANGEFEED`/`Version/timetravel#TIME_TRAVEL` read) AND stamps the `ElementIdentity` row through the `Element/identity#ELEMENT_IDENTITY` `IdentityStore.Stamp` owner into the SAME `IDocumentSession` before `SaveChangesAsync` so headers, identity, and event commit atomically; `public static IO<Option<ElementGraph>> ReadAsOf(IQuerySession session, ModelId model, TimeCut cut)` folds the stream to the cut through `AggregateStreamAsync<GraphProjection>(model.Value, version: cut.StreamVersion.IfNone(0L), timestamp: …)` and maps the materialized `GraphProjection.Graph`.
- Auto: the bracket runs the op through the generated total `GraphStoreOp.Switch` (compile-time exhaustive over the closed family — a new op breaks the build at the `Run` dispatch, never a runtime-silent `_` arm) and `SaveChangesAsync` commits events plus the identity document plus the inline projection in one Postgres transaction — there is no separate identity ORM and no two-phase dance because the identity rides the same session as a Marten document; the read op calls `FetchLatest<GraphProjection>(model)` which returns the inline document when present or live-folds the tail, so a read after a commit in the same unit is consistent; the AS-OF op binds either a `version` or a `timestamp` (one or the other, never both) from the `TimeCut` so an historical read folds the SAME `GraphDelta.ReplayOnto` deterministically; provider exceptions convert to `GraphFault` at the one bracket boundary and the interior never sees a raw `Marten.Exceptions.MartenException`, while caller cancellation passes through untyped.
- Receipt: an `Open`/`Commit`/`Retire` rides `store.element.<verb>` carrying the resulting `StreamAction.Version`; a `Read`/`ReadAsOf` rides `store.element.read` carrying the folded node count; the identity co-commit rides `store.element.identity` carrying the `NodeId` count (`Element/identity#ELEMENT_IDENTITY`).
- Packages: Marten (`IDocumentSession`/`IQuerySession`/`SetHeader`/`SaveChangesAsync`/`FetchLatest`/`AggregateStreamAsync`/`FetchStreamStateAsync`/`Store`), Rasm.AppHost (`Principal` the verified inbound actor — `Rasm.AppHost/Agent/identity#PRINCIPAL`), LanguageExt.Core (`IO`/`Fin`/`Option`), NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new durable interaction is one `GraphStoreOp` case plus one branch in the generated total `Switch` the closed family forces; a new read modality is one op case discriminating on its input (a key resolves to one graph, a cut to an AS-OF graph, a state probe to a head version) whose leg projects through the one `Received` receipt fold (the `(version, nodes, edges)` extractor the only per-leg difference); zero new surface — a repository per model, a per-verb service, an injected `persist` delegate, a per-read-leg receipt-construction-plus-absence-arm copy, or a separate identity transaction is the deleted form because the one rail owns the bracket, the one session owns identity-plus-event atomicity, the one `Received` fold owns the receipt-and-absence projection, and the op family discriminates by value shape through the generated `Switch`.
- Boundary: the one transaction owner for identity plus event is the `IDocumentSession` — the `ElementIdentity` row stores as a Marten document via the `Element/identity#ELEMENT_IDENTITY` `IdentityStore.Stamp` owner in the same session as the appended events, so a single `SaveChangesAsync` commits both with no free two-ORM atomicity and no EF-versus-Marten gap (the EF identity model of `Element/identity` is the relational projection the Marten document feeds, queried through Npgsql for the H3/pgvector/ACL columns, but the WRITE of record is the one Marten session); the blame headers are the WRITE side of the read-side blame contract — `Stage` calls `session.SetHeader("actor", actor.Subject)`/`SetHeader("origin", storeId.ToString())` so every appended event carries the `actor`/`origin` slots `MetadataConfig.HeadersEnabled` persists and `Version/ledger#CHANGEFEED` `OpLog.Project`/`Version/timetravel#TIME_TRAVEL` `ActorOf`/`OriginOf` read; omitting the stamp is the deleted form that collapses the `OriginStoreId` LWW tie-break to `Guid.Empty` and strands blame with no actor; the read op is read-your-writes through the inline projection and NEVER routes to an async analytical lane; the AS-OF op binds `version` XOR `timestamp` so a precise cut pins a version and an instant cut binds the wall clock, and the fold reuses the one `GraphDelta.ReplayOnto` so an historical graph equals the live state; optimistic concurrency is `Commit(model, delta, expectedVersion)` whose inline `Append(stream, expectedVersion, body)` aborts a racing same-version writer at `SaveChangesAsync` — surfacing as `Marten.Exceptions.ConcurrentUpdateException` wrapping the inner `JasperFx.Events.EventStreamUnexpectedMaxEventIdException`, both lifted to `GraphFault.StreamVersionConflict` carrying the head version — and a multi-writer-hostile section escalates to `FetchForExclusiveWriting<GraphProjection>(model)` for a stream-level advisory lock; `SaveChangesAsync` is the only commit and the bracket never bypasses it; provider failure converts to `GraphFault` once at the bracket and the op-log changefeed (`Version/ledger#CHANGEFEED`) projects FROM the committed events, never a trigger-based second write path; a re-ingest of an existing model is aligned UPSTREAM by the `Version/merge#STRUCTURAL_DIFF` `Reconcile` (correlating the projector's freshly-minted rooted `NodeId`s onto the durable ids on `Node.Object.ExternalId`, the 1:1 IFC GlobalId) BEFORE the aligned `GraphDelta` reaches this `Commit`, so a re-import revises the existing stream rather than forking a duplicate model — this store appends the already-aligned delta, never re-deriving the alignment.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
// `TimeCut` is the one temporal-cut value-object owned by `Version/timetravel#TIME_TRAVEL`: the
// inclusive `Hlc` ceiling plus the optional Marten stream version. The Marten stream fold binds the
// version when present, else the `Ceiling.Physical` instant — so the engine HLC-cell cut and the
// stream-version cut are one concept, never two parallel cut types.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphStoreOp {
    private GraphStoreOp() { }

    public sealed record Open(ModelId Model, Header Header, GraphDelta Opening) : GraphStoreOp;
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
    // The one rail — the generated total `GraphStoreOp.Switch` (compile-time exhaustive over the closed
    // family, NO runtime-silent `_` default arm) dispatches each op to its bracket leg; a new op breaks
    // the build here. Open/Commit/Retire share the co-transactional `Stage` write fold (stamp the blame
    // headers, stage the stream action, `IdentityStore.Stamp`, `SaveChangesAsync`); Read/ReadAsOf/State are
    // the read legs. `actor` is the `Rasm.AppHost/Agent/identity#PRINCIPAL` `Principal` and `storeId` the
    // store's own origin Guid (the LWW tie-break origin) — the SAME `actor`/`origin` header slots the read
    // side reads (`Version/ledger#CHANGEFEED` `OpLog.Project`, `Version/timetravel#TIME_TRAVEL` `ActorOf`/`OriginOf`).
    public static IO<Fin<GraphReceipt>> Run(IDocumentSession session, ElementIdentity identity, GraphStoreOp op, Principal actor, Guid storeId, ClockPolicy clocks, CorrelationId correlation) =>
        from mark in IO.lift(clocks.Mark)
        from outcome in op.Switch(
            open: o => Stage(session, identity, actor, storeId, o.Model, o.Opening, 0L, _ => ElementSchema.Open(session, o.Model, o.Header, o.Opening), correlation, clocks, mark, "store.element.open"),
            commit: c => Stage(session, identity, actor, storeId, c.Model, c.Delta, c.Expected, _ => ElementSchema.Append(session, c.Model, new GraphEvent.GraphRevised(c.Delta), c.Expected), correlation, clocks, mark, "store.element.commit"),
            retire: t => Stage(session, identity, actor, storeId, t.Model, t.Delta, t.Expected, _ => ElementSchema.Append(session, t.Model, new GraphEvent.GraphRetired(t.Delta, t.Reason), t.Expected), correlation, clocks, mark, "store.element.retire"),
            read: r => ReadGraph(session, r.Model, clocks, mark, correlation),
            readAsOf: a => ReadGraphAsOf(session, a.Model, a.Cut, clocks, mark, correlation),
            state: s => ReadState(session, s.Model, clocks, mark, correlation))
        select outcome;

    public static IO<Option<ElementGraph>> ReadAsOf(IQuerySession session, ModelId model, TimeCut cut) =>
        ProjectAsOf(session, model, cut).Map(o => o.Map(static p => p.Graph));

    static IO<Option<GraphProjection>> ProjectAsOf(IQuerySession session, ModelId model, TimeCut cut) =>
        IO.liftAsync(() => session.Events.AggregateStreamAsync<GraphProjection>(
                model.Value,
                version: cut.StreamVersion.IfNone(0L),
                timestamp: cut.StreamVersion.IsSome ? (DateTimeOffset?)null : cut.At.ToDateTimeOffset()))
            .Map(Optional);

    // The co-transactional write fold: STAMP the blame headers (`actor` = the `Principal.Subject`, `origin` =
    // the store's own `storeId` Guid) onto the session so every event this transaction appends carries them
    // (`MetadataConfig.HeadersEnabled` is set in `Configure`), stage the stream action (open/append), stamp the
    // identity row in the SAME session, then ONE `SaveChangesAsync` commits the event-with-headers, the identity
    // document, and the inline projection atomically. The header stamp is the WRITE side of the blame contract the
    // read side reads (`Version/ledger#CHANGEFEED` `OpLog.Project` `actor`/`origin`, `Version/timetravel#TIME_TRAVEL`
    // `ActorOf`/`OriginOf`) — without it the `OriginStoreId` LWW tie-break collapses to `Guid.Empty` and blame names
    // no actor. A provider failure converts to `GraphFault` at this one boundary via `Lift`.
    static IO<Fin<GraphReceipt>> Stage(IDocumentSession session, ElementIdentity identity, Principal actor, Guid storeId, ModelId model, GraphDelta delta, long expected, Func<Unit, StreamAction> stage, CorrelationId correlation, ClockPolicy clocks, long mark, string slot) =>
        IO.liftAsync(async () => {
            session.SetHeader("actor", actor.Subject);
            session.SetHeader("origin", storeId.ToString());
            StreamAction action = stage(unit);
            IdentityStore.Stamp(session, identity);
            await session.SaveChangesAsync().ConfigureAwait(false);
            return Fin<GraphReceipt>.Succ(new GraphReceipt(slot, model, action.Version, delta.NodeCount, delta.EdgeCount, clocks.Elapsed(mark), clocks.Now, correlation));
        }) | @catch<IO, Fin<GraphReceipt>>(static _ => true, error => Lift(session, model, expected, error));

    // The three read legs differ ONLY in the fetch shape and the (version, nodes, edges) triple each extracts; the
    // Some -> store.element.read receipt and the None -> ModelAbsent absence arm are ONE projection the legs share
    // (Received), so the receipt construction and the absence rail are owned once, never re-spelled per read modality.
    static IO<Fin<GraphReceipt>> ReadGraph(IDocumentSession session, ModelId model, ClockPolicy clocks, long mark, CorrelationId correlation) =>
        IO.liftAsync(() => session.Events.FetchLatest<GraphProjection>(model.Value))
            .Map(p => Received(model, Optional(p), static g => (g.Version, g.Graph.Nodes.Count, g.Graph.Edges.Length), clocks, mark, correlation));

    static IO<Fin<GraphReceipt>> ReadGraphAsOf(IDocumentSession session, ModelId model, TimeCut cut, ClockPolicy clocks, long mark, CorrelationId correlation) =>
        ProjectAsOf(session, model, cut)
            .Map(p => Received(model, p, static g => (g.Version, g.Graph.Nodes.Count, g.Graph.Edges.Length), clocks, mark, correlation));

    static IO<Fin<GraphReceipt>> ReadState(IDocumentSession session, ModelId model, ClockPolicy clocks, long mark, CorrelationId correlation) =>
        IO.liftAsync(() => session.Events.FetchStreamStateAsync(model.Value))
            .Map(s => Received(model, Optional(s), static state => (state.Version, 0, 0), clocks, mark, correlation));

    static Fin<GraphReceipt> Received<T>(ModelId model, Option<T> found, Func<T, (long Version, int Nodes, int Edges)> read, ClockPolicy clocks, long mark, CorrelationId correlation) =>
        found.Match(
            Some: value => read(value) switch { var (version, nodes, edges) => Fin<GraphReceipt>.Succ(new GraphReceipt("store.element.read", model, version, nodes, edges, clocks.Elapsed(mark), clocks.Now, correlation)) },
            None: () => Fin<GraphReceipt>.Fail(new GraphFault.ModelAbsent(model)));

    // Provider-fault conversion at the one bracket boundary: an optimistic-version collision surfaces as
    // `Marten.Exceptions.ConcurrentUpdateException` (the wrapping write-collision) or its inner
    // `JasperFx.Events.EventStreamUnexpectedMaxEventIdException` (the expected-version mismatch) — both
    // lifted to `GraphFault.StreamVersionConflict` carrying the real head version read back through
    // `FetchStreamStateAsync`; every other provider exception is `GraphFault.DeltaRejected`; caller
    // cancellation passes through untyped (never lifted to a domain fault).
    static IO<Fin<GraphReceipt>> Lift(IDocumentSession session, ModelId model, long expected, Error error) =>
        error.Exception.Match(
            Some: ex => ex is Marten.Exceptions.ConcurrentUpdateException or JasperFx.Events.EventStreamUnexpectedMaxEventIdException
                ? IO.liftAsync(() => session.Events.FetchStreamStateAsync(model.Value))
                    .Map(state => Fin<GraphReceipt>.Fail(new GraphFault.StreamVersionConflict(model, expected, Optional(state).Match(Some: static s => s.Version, None: static () => 0L))))
                : IO.pure(Fin<GraphReceipt>.Fail(new GraphFault.DeltaRejected(ex.Message, 0))),
            None: () => IO.pure(Fin<GraphReceipt>.Fail(new GraphFault.DeltaRejected(error.Message, error.Code))));
}
```

| [INDEX] | [POLICY]                 | [VALUE]                                  | [BINDING]                                                  |
| :-----: | :----------------------- | :--------------------------------------- | :-------------------------------------------------------- |
|  [01]   | one txn owner            | identity + event in one `IDocumentSession` | `IdentityStore.Stamp` then `SaveChangesAsync`            |
|  [02]   | read consistency         | `FetchLatest<GraphProjection>`           | inline document or live tail fold; read-your-writes       |
|  [03]   | AS-OF fold               | `AggregateStreamAsync(version\|timestamp)` | version XOR instant; reuses `GraphDelta.ReplayOnto`        |
|  [04]   | optimistic concurrency   | `Append(model, delta, expectedVersion)`  | racing writer → `ConcurrentUpdateException` → `StreamVersionConflict`|
|  [05]   | fault conversion         | one bracket boundary                      | provider exception → `GraphFault`; cancellation untyped   |
