# [PERSISTENCE_ELEMENT_GRAPH]

Rasm.Persistence persists each `Rasm.Element` graph as one Marten stream keyed by `ModelId`. `GraphCreated`, `GraphRevised`, and `GraphRetired` carry seam-validated `GraphDelta` bodies; the inline `GraphProjection` and AS-OF reconstruction reuse `GraphDelta.ReplayOnto`. `IdentityStore.Stamp` queues the provider-bound `IdentityWriter` on the same `IDocumentSession`, so identity, event, lineage, and inline projection commit once without a second writer. `GraphWriteStamp` carries actor, origin, optional project, and the boot-composed identity writer only on write operation cases; read cases carry none of those knobs. `ProjectionContext` owns tenant, correlation, and clock values. `FaultBand` remains the single Persistence fault registry.

## [01]-[INDEX]

- [02]-[STREAM_GRAIN]: model-stream identity, the `GraphEvent` body family, optimistic append, and the schema-keyed event registration.
- [03]-[GRAPH_PROJECTION]: the inline `SingleStreamProjection` folding `GraphDelta` into the STJ-rehydratable `GraphProjection` over the seam `GraphDelta.ReplayOnto`, the materialized `ElementGraph` read boundary, and the read-your-writes consistency boundary.
- [04]-[STORE_RAIL]: the Persistence-owned frame shapes (`StoreActor`/`ProjectionContext`/`ResolvedProfile`), the one `GraphStoreOp` operation family over the generated total `Switch`, the session bracket, the exclusive-lock escalation, AS-OF reconstruction, the durable naming-lineage rows, and the co-transactional identity commit.
- [05]-[FAULT_TABLES]: the `FaultBand` `[SmartEnum<int>]` band-allocation registry (21 own decades + pinned foreign mirrors) and the `GraphFault` band it hosts.

## [02]-[STREAM_GRAIN]

- Owner: `ModelId` the `[ValueObject<Guid>]` per-model stream key under the `IObjectFactory` floor; `GraphEvent` the `[Union]` event-body family every model stream appends, carrying the `Body`/`Lifecycle` projections the `Version/ledger#CHANGEFEED` `OpLog.Project` reads off each Marten event; `EventLifecycle` the `[SmartEnum<string>]` create/revise/retire verb each event row carries; `ElementSchema` the static surface owning the `StoreOptions` event registration, the strong-typed value registration, the inline projection registration, and the per-model stream-start and append legs over the one `IDocumentSession`.
- Cases: `GraphCreated(Header Header, GraphDelta Delta)` opens a stream carrying the `Rasm.Element` `Header` (`ReleaseVersion`/`ModelView`/`GeoReference`/`Tolerance`/`Instant`/`StepHeader`) AND the assembled opening `GraphDelta` (the `Projection/projection#PROJECTION_CONTRACT` `Assemble` merged model-creating delta), so a model is created in ONE event rather than an empty open plus a separate content commit; `GraphRevised(GraphDelta Delta)` is the steady-state append; `GraphRetired(GraphDelta Delta, string Reason)` carries the retirement delta whose `GraphDelta` removes the retired nodes/edges — so retirement is a real convergent delta the projection folds, never an out-of-band tombstone; the event body is ALWAYS the seam `GraphDelta`, NEVER a whole-graph snapshot, because the delta replays deterministically through `GraphDelta.ReplayOnto` and a whole-graph body bloats every append by the model size.
- Entry: `public static StoreOptions Configure(StoreOptions opts, NpgsqlDataSource source)` registers the event types, the strong-typed `ModelId`/`NodeId` value types, the inline `GraphProjection` self-aggregating snapshot, and the metadata columns once at boot; `public static StreamAction Open(IDocumentSession session, ModelId model, Header header, GraphDelta opening)` calls `session.Events.StartStream<GraphProjection>(model.Value, new GraphEvent.GraphCreated(header, opening))` so the assembled opening delta is the one model-creating event; `public static StreamAction Append(IDocumentSession session, ModelId model, GraphEvent body, long expectedVersion)` appends with the inline optimistic version guard so a concurrent writer racing the same stream version aborts at `SaveChangesAsync` rather than silently interleaving.
- Auto: `StreamIdentity.AsGuid` keys one stream per `ModelId`. `EventAppendMode.Rich` remains invariant for authoring and re-ingest because `actor`/`origin`/`tenant` headers are durable blame inputs; Marten's `Quick` modes trade away metadata richness and therefore do not admit this stream. `GraphDelta` is the seam-validated event body. `RegisterValueType<ModelId>()`/`RegisterValueType<NodeId>()` preserve typed keys, and `UseSystemTextJsonForSerialization(ElementJson.Options, …)` binds the one generated serializer profile.
- Receipt: a stream open rides `store.element.open`, a delta append rides `store.element.commit` carrying the delta node/edge counts, a retirement rides `store.element.retire`; the `StreamAction.Version` is the optimistic guard the next append reads.
- Packages: Marten (`StartStream`/`Append`/`StreamAction`/`EventAppendMode`/`StreamIdentity`/`RegisterValueType`/`Snapshot`/`UseSystemTextJsonForSerialization`/`MetadataConfig`), Npgsql, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new durable change kind is one `GraphEvent` case plus one `EventLifecycle` row plus one projection `Apply` method the convention discovery forces; a richer model header is one field on the seam `Header`; a per-spatial-partition grain is one `ModelId` minting policy, never a second stream shape; zero new surface — a per-`NodeId` stream, a whole-graph event body, a second event table, or a bespoke `OpLogEntry` store beneath Marten is the deleted form because Marten owns the durable append and the rebuildable read views, and the `Version/` engine projects FROM these events (`Version/ledger#CHANGEFEED` `OpLog.Project` over `e.Data.Body`/`e.Data.Lifecycle`).
- Boundary: stream grain is ONE stream PER MODEL (or per spatial partition), never per-`NodeId`, and the event body is the `GraphDelta`, never a whole-graph snapshot; the `GraphDelta` is the seam-owned graph-mutation record the projection folds immutably through `GraphDelta.ReplayOnto`, so the durable history is a delta log the engine replays and the rehydrated graph is bit-identical to the live state at any version because the fold is the ONE `GraphDelta.ReplayOnto` the AS-OF reconstruction also runs; the optimistic append (`Append(stream, expectedVersion, …)`) is the inline guard, `AppendOptimistic` the read-then-guard, and the `GraphStoreOp.CommitExclusive` case the stream-level advisory-lock escalation (`FetchForExclusiveWriting<GraphProjection>`, `#STORE_RAIL`); the `GraphRetired` delta is a real convergent retirement the projection folds and the `Version/retention#RETENTION_CLASSES` sweep reclaims, never an `ArchiveStream` that hides the events from the fold (archive is the AS-OF cut boundary, not retirement); a `GraphCreated` carries the `Header` so the stream's `ReleaseVersion`/`GeoReference`/`Tolerance` are the first folded fact and every later delta's measure quantization (`Element/codec#CONTENT_ADDRESS`) reads the header tolerance; `EventAppendMode` trades metadata richness for throughput as a config value, never a per-call branch; the `GraphEvent` is the body family `Version/ledger#CHANGEFEED` lifts (`OpLog.Project(IEvent<GraphEvent>)` reads `e.Data.Body`/`e.Data.Lifecycle`), so this owner's body shape is the changefeed's input contract.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using JasperFx.Events;
using LanguageExt;
using LanguageExt.Common;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using NodaTime;
using Npgsql;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Relations;
using Thinktecture;
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Element;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<Guid>]
public readonly partial struct ModelId {
    public static ModelId New() => Create(Guid.CreateVersion7());
}

// The project altitude above the per-model stream: federated disciplines live in separate `ModelId` streams,
// and the project id is the grouping key the `project` blame header carries and the `#STORE_RAIL`
// `ProjectRollup` slices by.
[ValueObject<Guid>]
public readonly partial struct ProjectId {
    public static ProjectId New() => Create(Guid.CreateVersion7());
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
        // Composite computed index: the prior-generation read is `(Model, max Version)`, so a Model-only
        // index would force a per-model scan+sort — the multi-member overload serves it index-only.
        opts.Schema.For<NameLineage>().Index([static l => l.Model, static l => l.Version]);
        // ONE registration: `GraphProjection` is the self-aggregating inline snapshot — its `Create`/`Apply`
        // convention methods fold the stream into the document written in the SAME append transaction, so a
        // read-your-writes interactive query (`Query/lane#READ_ROUTING`) reads the head with no daemon lag.
        // A second `Projections.Add<GraphProjection>` is the deleted double-registration (it would re-treat the
        // aggregate as a raw `IProjection`); the inline aggregate IS the materialized view that bounds replay.
        opts.Projections.Snapshot<GraphProjection>(SnapshotLifecycle.Inline);
        // The project rollup is ASYNC by construction (a dashboard/roster view, never interactive correctness),
        // and the coordination edges index by project for the federated selection reads.
        opts.Projections.Add(new ProjectRollup(), ProjectionLifecycle.Async);
        opts.Schema.For<ModelLink>().Index(static l => l.Project);
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

| [INDEX] | [POLICY]          | [VALUE]                               | [BINDING]                                                       |
| :-----: | :---------------- | :------------------------------------ | :-------------------------------------------------------------- |
|  [01]   | stream grain      | one stream per `ModelId`              | `StreamIdentity.AsGuid`; never per-`NodeId`                     |
|  [02]   | event body        | the seam `GraphDelta`                 | never a whole-graph snapshot; folds via `GraphDelta.ReplayOnto` |
|  [03]   | append mode       | `EventAppendMode.Rich`                | blame headers remain durable on every write                     |
|  [04]   | optimistic guard  | `Append(stream, expectedVersion, …)`  | concurrent same-version writer aborts at `SaveChangesAsync`     |
|  [05]   | strong-typed keys | `RegisterValueType<ModelId/NodeId>`   | typed stream key + document id, never a bare Guid/string        |

## [03]-[GRAPH_PROJECTION]

- Owner: `GraphProjection` the inline self-aggregating snapshot AGGREGATE Marten folds one model stream into (registered through `opts.Projections.Snapshot<GraphProjection>(SnapshotLifecycle.Inline)` — the document carries the `Create`/`Apply` convention methods Marten discovers and wraps as a single-stream projection internally, so the record is the aggregate, never a hand-derived `SingleStreamProjection<,>` subclass) — the STJ-rehydratable carrier of one model's `Header`, node map, edge array, and folded version, written in the append transaction, materializing the seam `ElementGraph` ONCE through `ElementGraph.Of` at the read boundary; the aggregate's `Create`/`Apply` convention methods owning the one `GraphDelta.ReplayOnto` fold over the seam graph; faults rail the `#FAULT_TABLES` `GraphFault` band.
- Cases: `Create(GraphCreated)` seeds the genesis through the seam `ElementGraph.Genesis(header)` then replays the opening delta through `GraphDelta.ReplayOnto`; `Apply(GraphRevised, GraphProjection)` and `Apply(GraphRetired, GraphProjection)` replay their `GraphDelta` onto the materialized graph; the fold is the seam `GraphDelta.ReplayOnto : ElementGraph → ElementGraph`, TOTAL over the seam-validated delta because the projector's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` and the seam structural `Graph/delta#GRAPH_DELTA` `LegalLink` gated it at the write boundary (`ReplayOnto` re-applies the recorded add/remove/revise RAW, never re-validating), so a malformed delta in the stream is a deployment defect the `#STORE_RAIL` bracket surfaces as `GraphFault.DeltaRejected`, not a recoverable per-fold rail.
- Entry: `Create(IEvent<GraphEvent.GraphCreated>)` seeds `Model` from `StreamId` and `Version` from the stored envelope; body-only `Apply(GraphRevised)` and `Apply(GraphRetired)` advance the aggregate version once per event. `Graph` memoizes the frozen `ElementGraph` materialization.
- Auto: the projection registers `SnapshotLifecycle.Inline` so the folded `GraphProjection` document is written in the SAME transaction as the appended events — a `Read` after a `Commit` in the same logical unit sees the new state with no daemon lag — and the inline aggregate IS the periodic materialized view, so a deep stream loads the head document rather than re-folding from genesis; the projection stores the STJ-serializable primitives (`Header`, `ImmutableDictionary<NodeId, Node>`, `ImmutableArray<Relationship>`) because the seam `ElementGraph` is a sealed read-snapshot class with no deserialization path, and the live authoring graph uses the seam's `ImmutableDictionary`/HAMT structural-sharing form (`Graph/delta#GRAPH_DELTA`) while `ElementGraph.Of` freezes to `FrozenDictionary` + the incidence index + the lazy `QuikGraph` view only at the `Graph` materialization boundary, so the delta path stays O(log n) structural-sharing and the read snapshot stays O(1) lookup; `From` is the ONLY mint (each `Create`/`Apply` rebuilds the document and the lazy `Graph` memo from the folded snapshot) so a `with` can never alias a stale materialized graph.
- Receipt: a projection fold rides `store.element.project` carrying the folded delta count; a `GraphFault.DeltaRejected` rides `store.element.fault` carrying the rejecting delta detail and the seam structural-invariant code.
- Packages: Marten (`SingleStreamProjection`/`SnapshotLifecycle`/`IEvent<T>`), Rasm.Element (`ElementGraph`/`ElementGraph.Genesis`/`ElementGraph.Of`/`GraphDelta`/`GraphDelta.ReplayOnto`/`Header`/`Node`/`NodeId`/`Relationship`), LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Collections.Immutable/Frozen, BCL inbox.
- Growth: a new event arm is one projection `Apply` method the convention discovery forces; the cross-model rollup is the realized `#STORE_RAIL` `ProjectRollup` (header-sliced, roster + watermark only), so a richer project view is one field on `ProjectGraph`, never a second fold of the same delta; a co-transactional columnar egress is the `Query/columnar#COLUMNAR_LANE` `FlatTableProjection`; zero new surface — a hand-rolled stream folder, a second materializer, or a per-read whole-stream replay is the deleted form because the inline projection IS the materialized read and the AS-OF fold reuses the same `GraphDelta.ReplayOnto`.
- Boundary: the inline projection is the READ-YOUR-WRITES consistency boundary — authoritative containment, topology, and void-resolution reads go through this folded `GraphProjection.Graph`, NEVER an async lane, because an async daemon view lags the write (`Query/lane#READ_ROUTING` routes interactive correctness here by construction); the analytical lanes (`Query/columnar`, `Query/cypher`) are explicitly `ProjectionLifecycle.Async` with a staleness watermark and interactive-correctness reads block on `WaitForNonStaleProjectionDataAsync`; the projection apply is the SAME `GraphDelta.ReplayOnto` fold the `Version/timetravel#TIME_TRAVEL` AS-OF reconstruction runs (the live authoring path produces the deltas it replays, via the seam `Graph/delta#GRAPH_DELTA` `WorkingGraph.Apply`), so there is exactly ONE delta→graph materializer and a historical fold equals the live state field-for-field; the projection NEVER stores the seam `ElementGraph` directly (it has no public deserialization constructor — a sealed read-snapshot class whose only mint is `Of`/`Genesis`/`Apply`), so the document carries the rehydratable `Header`/node-map/edge-array and `Graph` materializes the frozen snapshot once through `ElementGraph.Of`; the inline aggregate is the materialized read floor bounding replay, never a second source of truth — `store.Advanced.RebuildSingleStreamAsync<GraphProjection>(model)` replays one stream's inline projection from zero when the fold logic changes; the projection never re-validates the delta because the projector `IGraphConstraint` and the seam `LegalLink` already gated it at the write boundary — re-validation in the projection is the deleted form because a validated delta in the stream is total by construction and a fold-time fault is a deployment defect surfaced as `GraphFault`, not a recoverable data path.

```csharp signature
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
        From(ModelId.Create(e.StreamId), e.Data.Delta.ReplayOnto(ElementGraph.Genesis(e.Data.Header)), e.Version);

    public GraphProjection Apply(GraphEvent.GraphRevised e) => From(Model, e.Delta.ReplayOnto(Graph), Version + 1L);

    public GraphProjection Apply(GraphEvent.GraphRetired e) => From(Model, e.Delta.ReplayOnto(Graph), Version + 1L);

    static GraphProjection From(ModelId model, ElementGraph folded, long version) =>
        new(model, folded.Header, folded.Nodes.ToImmutableDictionary(), folded.Edges, version);
}
```

| [INDEX] | [POLICY]              | [VALUE]                                 | [BINDING]                                              |
| :-----: | :-------------------- | :-------------------------------------- | :----------------------------------------------------- |
|  [01]   | authoritative read    | inline `GraphProjection.Graph`          | read-your-writes; never an async lane                  |
|  [02]   | replay floor          | inline self-aggregating snapshot        | head document loads, never a genesis re-fold per read  |
|  [03]   | one materializer      | seam `GraphDelta.ReplayOnto`            | projection and AS-OF reconstruction fold the one delta |
|  [04]   | serializable document | `Header`/node-map/edge-array primitives | `ElementGraph` has no STJ ctor; `Of` materializes once |
|  [05]   | cross-model rollup    | `MultiStreamProjection`                 | sliced by project id, never a second delta fold        |

## [04]-[STORE_RAIL]

- Owner: `StoreActor` the Persistence-owned `[ComplexValueObject]` actor value (subject + role claims) AppHost's composition root MAPS its richer `Principal` onto at the port boundary — the AppHost simple name never crosses down, mirroring the `Grant`/`Capability` never-share-a-name law; `ProjectionContext` the Persistence-owned injected frame carrying the clock delegates, the correlation `Guid`, and the tenant `UInt128` as VALUES (AppHost fills the slots from its own `ClockPolicy`/`CorrelationId`/`TenantContext` at the boundary; every Persistence page threads this frame, never an AppHost type); `RecoveryObjective`/`ResolvedProfile` the recovery-objective ingredient shapes `Version/recovery` re-threads onto (defined here, consumed there); `NameLineage` the durable REFERENCE-axis row persisting the kernel `Rasm/Spatial/naming` generational `Track(prior, rebuilt)` pairing across sessions; `ProjectId`/`LinkKind`/`ModelLink` the federated-coordination vocabulary — the durable cross-model reference edge IFC cannot carry, co-committed like `NameLineage`; `ProjectGraph`/`ProjectRollup` the realized project-altitude `MultiStreamProjection` sliced by the `project` blame header (async, roster + watermark only — never a second delta materializer); `GraphStoreOp` the `[Union]` operation family every durable graph interaction is a value in; `GraphStore` the static surface owning the one bracket over the generated total `Switch` — pooled session acquisition, the strong-typed append, the exclusive-lock escalation, the inline-projection read, the AS-OF fold, the co-transactional identity commit, and provider-fault conversion to `GraphFault`; `GraphReceipt` the typed per-op evidence carrying the model, the resulting version, and the elapsed `Duration`.
- Cases: write cases carry their required `ElementIdentity` and `GraphWriteStamp`; `Link` carries its project-scoped links and stamp; `Read`, `ReadAsOf`, and `State` carry only their read discriminants. `ModelLink` covers directed and symmetric cross-model relationships, validity interval, and extensible attributes; `LinkKind` carries directionality as row data.
- Entry: `Run(IDocumentSession, GraphStoreOp, ProjectionContext, CancellationToken)` dispatches the closed family, the token threading to every provider await so caller cancellation reaches the store instead of dying at the rail boundary. Write cases stamp their carrier and queue identity before `SaveChangesAsync`; read cases require no dummy identity, actor, origin, or project. `ReadAsOf` passes a nullable version only when the cut carries one and otherwise passes only the timestamp, preserving the version-XOR-time contract.
- Auto: the bracket runs the op through the generated total `GraphStoreOp.Switch` (compile-time exhaustive over the closed family — a new op breaks the build at the `Run` dispatch, never a runtime-silent `_` arm) and `SaveChangesAsync` commits events plus the queued identity upsert plus the lineage rows plus the inline projection in one Postgres transaction — there is no separate identity ORM and no two-phase dance because the identity write is the one model-derived statement `IdentityStore.Stamp` queues on the session; the read op calls `FetchLatest<GraphProjection>(model)` which returns the inline document when present or live-folds the tail, so a read after a commit in the same unit is consistent; the AS-OF op binds either a `version` or a `timestamp` (one or the other, never both) from the `TimeCut` so an historical read folds the SAME `GraphDelta.ReplayOnto` deterministically; a `Commit`/`CommitExclusive` carrying `Some(NameLineage)` stores the lineage rows in the same session so the kernel `NameTable.Track(prior, rebuilt)` reads a durable PRIOR generation on the next session — a durable projection of the kernel lineage as string pairs, never the kernel interior types crossing a wire (naming's interior-type law holds), the REFERENCE axis distinct from the merge-consumed per-node `NamingHash` CONTENT receipt; provider exceptions convert to `GraphFault` at the one bracket boundary and the interior never sees a raw `Marten.Exceptions.MartenException`, while caller cancellation passes through untyped.
- Receipt: an `Open`/`Commit`/`Retire` rides `store.element.<verb>` carrying the resulting `StreamAction.Version`; a `CommitExclusive` rides `store.element.commit-exclusive`; a `Link` rides `store.element.link` carrying the landed edge count; a `Read`/`ReadAsOf` rides `store.element.read` carrying the folded node count; the identity co-commit rides `store.element.identity` carrying the `NodeId` count (`Element/identity#ELEMENT_IDENTITY`).
- Packages: Marten (`IDocumentSession`/`IQuerySession`/`SetHeader`/`SaveChangesAsync`/`FetchLatest`/`FetchForExclusiveWriting`/`IEventStream<T>.AppendOne`/`AggregateStreamAsync`/`FetchStreamStateAsync`/`Store`), LanguageExt.Core (`IO`/`Fin`/`Option`), NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new durable interaction is one `GraphStoreOp` case plus one branch in the generated total `Switch` the closed family forces; a new read modality is one op case discriminating on its input (a key resolves to one graph, a cut to an AS-OF graph, a state probe to a head version) whose leg projects through the one `Received` receipt fold (the `(version, nodes, edges)` extractor the only per-leg difference); a new frame ingredient is one slot on `ProjectionContext`/`ResolvedProfile`, never a new signature parameter; a same-session SQL side-write (a coordination cursor, an outbox advance) is `IDocumentOperations.QueueSqlCommand` inside the one transaction, never a second connection; zero new surface — a repository per model, a per-verb service, an injected `persist` delegate, a per-read-leg receipt-construction-plus-absence-arm copy, a `using Rasm.AppHost` import, or a separate identity transaction is the deleted form because the one rail owns the bracket, the one session owns identity-plus-event atomicity, the one `Received` fold owns the receipt-and-absence projection, and the op family discriminates by value shape through the generated `Switch`.
- Boundary: the one transaction owner for identity plus event is the `IDocumentSession` — the `ElementIdentity` row lands as the ONE model-derived upsert the `Element/identity#ELEMENT_IDENTITY` `IdentityStore.Stamp` owner queues on the same session as the appended events, so a single `SaveChangesAsync` commits both with no free two-ORM atomicity and no EF-versus-Marten gap (the EF identity model of `Element/identity` names the one `element_identity` relation for DDL and every H3/PostGIS/pgvector/ACL read while holding zero write authority — the session is the only writer); the blame headers are the WRITE side of the read-side blame contract — `Stage` stamps `actor`/`origin`/`tenant` so every appended event carries the slots `MetadataConfig.HeadersEnabled` persists and `Version/ledger#CHANGEFEED` `OpLog.Project`/`Version/timetravel#TIME_TRAVEL` `ActorOf`/`OriginOf` read; omitting the stamp is the deleted form that collapses the `OriginStoreId` LWW tie-break to `Guid.Empty` and strands blame with no actor; the frame ingredients cross the strata as VALUES on the Persistence-owned shapes this section defines — a `ClockPolicy`/`CorrelationId`/`TenantContext`/`Principal` parameter on any Persistence signature is the named strata inversion, and every Persistence page re-threads onto `ProjectionContext`/`StoreActor`/`ResolvedProfile` in its own rebuild; the read op is read-your-writes through the inline projection and NEVER routes to an async analytical lane; the AS-OF op binds `version` XOR `timestamp` so a precise cut pins a version and an instant cut binds the wall clock, and the fold reuses the one `GraphDelta.ReplayOnto` so an historical graph equals the live state; optimistic concurrency is `Commit(model, delta, expectedVersion, …)` whose inline `Append(stream, expectedVersion, body)` aborts a racing same-version writer at `SaveChangesAsync` — surfacing as `Marten.Exceptions.ConcurrentUpdateException` wrapping the inner `JasperFx.Events.EventStreamUnexpectedMaxEventIdException`, both lifted to `GraphFault.StreamVersionConflict` carrying the head version — and the escalation is the `CommitExclusive` OP CASE, never prose: the advisory lock serializes hostile writers and its refusal is the typed `GraphFault.TxnConflict` (8303), so the folded transaction rail raises a registered sub-band row, never a loose 7001 integer; `SaveChangesAsync` is the only commit and the bracket never bypasses it; provider failure converts to `GraphFault` once at the bracket and the op-log changefeed (`Version/ledger#CHANGEFEED`) projects FROM the committed events, never a trigger-based second write path; a re-ingest of an existing model is aligned UPSTREAM by the `Version/merge#STRUCTURAL_DIFF` `Reconcile` (correlating the projector's freshly-minted rooted `NodeId`s onto the durable ids on `Node.Object.ExternalId`, the 1:1 IFC GlobalId) BEFORE the aligned `GraphDelta` reaches this `Commit`, so a re-import revises the existing stream rather than forking a duplicate model — this store appends the already-aligned delta, never re-deriving the alignment.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
// The Persistence-owned port-input shapes ([A.1]): AppHost's composition root FILLS these slots from its
// own Principal/ClockPolicy/CorrelationId/TenantContext/RecoveryObjective at the PORT boundary — the
// ingredients cross as delegate and wire VALUES, so no AppHost simple name resolves below the strata seam.
// Every frame-referencing Persistence page re-threads onto these shapes in its own leg.
[ComplexValueObject]
public sealed partial class StoreActor {
    public string Subject { get; }
    public Seq<string> Roles { get; }
}

public sealed record ProjectionContext(Func<long> Mark, Func<long, Duration> Elapsed, Func<Instant> Now, Guid Correlation, UInt128 Tenant);

public sealed record RecoveryObjective(Duration Rpo, Duration Rto);
public sealed record ResolvedProfile(RecoveryObjective Recovery, UInt128 Tenant);
public sealed record GraphWriteStamp(StoreActor Actor, Guid Origin, Option<ProjectId> Project, IdentityWriter Identity);

// `TimeCut` is the one temporal-cut value-object owned by `Version/timetravel#TIME_TRAVEL` (frozen-vocab
// contract): the inclusive `Hlc` ceiling plus the optional Marten stream version. The stream fold binds the
// version when present, else the `Ceiling.Physical` instant — one cut concept, never two parallel cut types.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphStoreOp {
    private GraphStoreOp() { }

    public sealed record Open(ModelId Model, Header Header, GraphDelta Opening, ElementIdentity Identity, GraphWriteStamp Stamp) : GraphStoreOp;
    public sealed record Commit(ModelId Model, GraphDelta Delta, long Expected, Option<NameLineage> Lineage, ElementIdentity Identity, GraphWriteStamp Stamp) : GraphStoreOp;
    public sealed record CommitExclusive(ModelId Model, GraphDelta Delta, Option<NameLineage> Lineage, ElementIdentity Identity, GraphWriteStamp Stamp) : GraphStoreOp;
    public sealed record Retire(ModelId Model, GraphDelta Delta, string Reason, long Expected, ElementIdentity Identity, GraphWriteStamp Stamp) : GraphStoreOp;
    // The federated-coordination write: durable cross-model edges land as rows in the same session, so a
    // clash pairing or provision-for-void reference commits with full blame headers under the project id.
    public sealed record Link(ModelId Model, ProjectId Project, Seq<ModelLink> Links, GraphWriteStamp Stamp) : GraphStoreOp;
    public sealed record Read(ModelId Model) : GraphStoreOp;
    public sealed record ReadAsOf(ModelId Model, TimeCut Cut) : GraphStoreOp;
    public sealed record State(ModelId Model) : GraphStoreOp;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct GraphReceipt(string Slot, ModelId Model, long Version, int Nodes, int Edges, Duration Elapsed, Instant At, Guid Correlation);

// The durable REFERENCE-axis lineage row: the kernel `Rasm/Spatial/naming` `NameTable.Track(prior, rebuilt)`
// needs a PRIOR generation across sessions, so each rename-bearing commit persists the prior->rebuilt
// `TopoName` pairing as STRING pairs co-committed with the delta — a durable projection, never the kernel
// interior types crossing a wire. Distinct from the merge-consumed per-node `NamingHash` CONTENT receipt.
// `Id` is the Marten document identity (v7, insert-local); the prior-generation read keys `(Model, max
// Version)` through the `Configure` composite `(Model, Version)` computed index, index-served end to end.
public sealed record NameLineage(ModelId Model, long Version, HashMap<string, string> Track) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

// The federated-coordination edge vocabulary and row: IFC carries no cross-file relationship, so the
// inter-model reference (a duct penetrating an arch wall, a provision-for-void pairing, a shared-grid
// alignment) is a first-class DURABLE row co-committed like `NameLineage` — the in-model seam `Relationship`
// stays single-graph and is never widened. `Query/lane#ELEMENT_SET_ALGEBRA` and `Query/topology` are the
// selection/traversal consumers; a new coordination relationship class is one `LinkKind` row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LinkKind {
    public static readonly LinkKind HostPenetration = new("host-penetration", directed: true);
    public static readonly LinkKind ProvisionForVoid = new("provision-for-void", directed: true);
    public static readonly LinkKind SystemContinuation = new("system-continuation", directed: true);
    public static readonly LinkKind Dependency = new("dependency", directed: true);
    public static readonly LinkKind Ownership = new("ownership", directed: true);
    public static readonly LinkKind Alignment = new("alignment", directed: false);
    public static readonly LinkKind SharedDatum = new("shared-datum", directed: false);
    public static readonly LinkKind SpatialInterface = new("spatial-interface", directed: false);
    public static readonly LinkKind Clash = new("clash", directed: false);
    public static readonly LinkKind Clearance = new("clearance", directed: false);
    public static readonly LinkKind Reference = new("reference", directed: false);

    public bool Directed { get; }
    private LinkKind(string key, bool directed) : this(key) => Directed = directed;
}

public sealed record ModelLink(
    ProjectId Project,
    ModelId FromModel,
    NodeId FromNode,
    ModelId ToModel,
    NodeId ToNode,
    LinkKind Kind,
    Instant ValidFrom,
    Option<Instant> ValidUntil,
    HashMap<string, JsonElement> Attributes) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

// The project-altitude rollup REALIZED (async daemon view — project dashboards, model rosters, whole-project
// watermarks), sliced by the `project` blame header `Blame` stamps so membership is a WRITE-time fact, never
// a fold-time join. It folds rosters and event watermarks ONLY — the per-model graph stays the inline
// aggregate's, so no second delta materializer exists.
public sealed record ProjectGraph(Guid Id, ImmutableHashSet<Guid> Models, long Events);

public sealed class ProjectRollup : MultiStreamProjection<ProjectGraph, Guid> {
    public ProjectRollup() => CustomGrouping(new ProjectHeaderGrouper());
    public static ProjectGraph Create(IEvent<GraphEvent> e) => new(HeaderProject(e), [e.StreamId], 1L);
    public ProjectGraph Apply(IEvent<GraphEvent> e, ProjectGraph view) => view with { Models = view.Models.Add(e.StreamId), Events = view.Events + 1L };
    static Guid HeaderProject(IEvent e) => ProjectHeaderGrouper.ProjectOf(e).IfNone(Guid.Empty);
}

// Events with no `project` header never group — a model outside any project simply has no rollup row.
// `TryGetValue` is load-bearing: the raw dictionary indexer THROWS on an absent header, so an unstamped
// event would crash the async daemon instead of being skipped.
file sealed class ProjectHeaderGrouper : IAggregateGrouper<Guid> {
    internal static Option<Guid> ProjectOf(IEvent e) =>
        e.Headers is { } headers && headers.TryGetValue("project", out object? raw) && raw is string project && Guid.TryParse(project, out Guid id)
            ? Some(id) : None;

    public Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<Guid> grouping) {
        foreach (IEvent @event in events) {
            ProjectOf(@event).IfSome(id => grouping.AddEvent(id, @event));   // Exemption: the grouper interface is the platform-forced statement seam
        }
        return Task.CompletedTask;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GraphStore {
    // The one rail — the generated total `GraphStoreOp.Switch` (compile-time exhaustive over the closed
    // family, NO runtime-silent `_` default arm) dispatches each op to its bracket leg; a new op breaks
    // the build here. Open/Commit/Retire share the co-transactional `Stage` write fold (stamp the blame
    // headers, stage the stream action, `IdentityStore.Stamp`, lineage rows, `SaveChangesAsync`);
    // CommitExclusive is the advisory-lock escalation; Read/ReadAsOf/State are the read legs. `actor` is
    // the Persistence-owned `StoreActor` and `storeId` the store's own origin Guid (the LWW tie-break
    // origin) — the SAME `actor`/`origin` header slots the read side reads.
    public static IO<Fin<GraphReceipt>> Run(IDocumentSession session, GraphStoreOp op, ProjectionContext frame, CancellationToken cancellationToken) =>
        from mark in IO.lift(frame.Mark)
        from outcome in op.Switch(
            open: o => Stage(session, o.Identity, o.Stamp, o.Model, o.Opening, 0L, None, _ => ElementSchema.Open(session, o.Model, o.Header, o.Opening), frame, mark, "store.element.open", cancellationToken),
            commit: c => Stage(session, c.Identity, c.Stamp, c.Model, c.Delta, c.Expected, c.Lineage, _ => ElementSchema.Append(session, c.Model, new GraphEvent.GraphRevised(c.Delta), c.Expected), frame, mark, "store.element.commit", cancellationToken),
            commitExclusive: x => StageExclusive(session, x.Identity, x.Stamp, x.Model, x.Delta, x.Lineage, frame, mark, cancellationToken),
            retire: t => Stage(session, t.Identity, t.Stamp, t.Model, t.Delta, t.Expected, None, _ => ElementSchema.Append(session, t.Model, new GraphEvent.GraphRetired(t.Delta, t.Reason), t.Expected), frame, mark, "store.element.retire", cancellationToken),
            link: l => StageLinks(session, l, frame, mark, cancellationToken),
            read: r => ReadGraph(session, r.Model, frame, mark, cancellationToken),
            readAsOf: a => ReadGraphAsOf(session, a.Model, a.Cut, frame, mark, cancellationToken),
            state: s => ReadState(session, s.Model, frame, mark, cancellationToken))
        select outcome;

    public static IO<Option<ElementGraph>> ReadAsOf(IQuerySession session, ModelId model, TimeCut cut, CancellationToken cancellationToken) =>
        ProjectAsOf(session, model, cut, cancellationToken).Map(o => o.Map(static p => p.Graph));

    static IO<Option<GraphProjection>> ProjectAsOf(IQuerySession session, ModelId model, TimeCut cut, CancellationToken cancellationToken) =>
        IO.liftAsync(() => session.Events.AggregateStreamAsync<GraphProjection>(
                model.Value,
                version: cut.StreamVersion.Match<long?>(Some: static version => version, None: static () => null),
                timestamp: cut.StreamVersion.IsSome ? (DateTimeOffset?)null : cut.At.ToDateTimeOffset(),
                token: cancellationToken))
            .Map(Optional);

    // The co-transactional write fold: STAMP the blame headers (`actor` = `StoreActor.Subject`, `origin` =
    // the store's own `storeId` Guid, `tenant` = the frame's RLS partition) onto the session so every event
    // this transaction appends carries them (`MetadataConfig.HeadersEnabled` is set in `Configure`), stage
    // the stream action (open/append), stamp the identity row and the lineage rows in the SAME session, then
    // ONE `SaveChangesAsync` commits the event-with-headers, the identity document, the lineage rows, and
    // the inline projection atomically. A provider failure converts to `GraphFault` at this one boundary via `Lift`.
    static IO<Fin<GraphReceipt>> Stage(IDocumentSession session, ElementIdentity identity, GraphWriteStamp stamp, ModelId model, GraphDelta delta, long expected, Option<NameLineage> lineage, Func<Unit, StreamAction> stage, ProjectionContext frame, long mark, string slot, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => {
            Blame(session, stamp, frame);
            StreamAction action = stage(unit);
            IdentityStore.Stamp(session, identity, stamp.Identity);
            lineage.IfSome(rows => session.Store(rows with { Version = action.Version }));
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Fin<GraphReceipt>.Succ(new GraphReceipt(slot, model, action.Version, delta.NodeCount, delta.EdgeCount, frame.Elapsed(mark), frame.Now(), frame.Correlation));
        }) | @catch<IO, Fin<GraphReceipt>>(static _ => true, error => Lift(session, model, expected, error));

    // The coordination-edge write: link rows land as Marten documents in the same blame-stamped session, so a
    // cross-model reference commits atomically with full actor/origin/tenant/project headers; the receipt's
    // edge count carries the landed link count.
    static IO<Fin<GraphReceipt>> StageLinks(IDocumentSession session, GraphStoreOp.Link op, ProjectionContext frame, long mark, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => {
            Blame(session, op.Stamp with { Project = Some(op.Project) }, frame);
            op.Links.Iter(link => session.Store(link));
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Fin<GraphReceipt>.Succ(new GraphReceipt("store.element.link", op.Model, 0L, 0, op.Links.Count, frame.Elapsed(mark), frame.Now(), frame.Correlation));
        }) | @catch<IO, Fin<GraphReceipt>>(static _ => true, error => Lift(session, op.Model, 0L, error));

    // The multi-writer-hostile escalation: `FetchForExclusiveWriting` takes the stream-level advisory lock,
    // so hostile writers serialize instead of racing the optimistic guard. A lock or serialization refusal
    // is the folded-transaction conflict `GraphFault.TxnConflict` (8303); a plain version race still lifts
    // through the shared `Lift` conversion.
    static IO<Fin<GraphReceipt>> StageExclusive(IDocumentSession session, ElementIdentity identity, GraphWriteStamp stamp, ModelId model, GraphDelta delta, Option<NameLineage> lineage, ProjectionContext frame, long mark, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => {
            Blame(session, stamp, frame);
            IEventStream<GraphProjection> stream = await session.Events.FetchForExclusiveWriting<GraphProjection>(model.Value, cancellationToken).ConfigureAwait(false);
            stream.AppendOne(new GraphEvent.GraphRevised(delta));
            long next = (stream.Aggregate?.Version ?? 0L) + 1L;
            IdentityStore.Stamp(session, identity, stamp.Identity);
            lineage.IfSome(rows => session.Store(rows with { Version = next }));
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Fin<GraphReceipt>.Succ(new GraphReceipt("store.element.commit-exclusive", model, next, delta.NodeCount, delta.EdgeCount, frame.Elapsed(mark), frame.Now(), frame.Correlation));
        }) | @catch<IO, Fin<GraphReceipt>>(static _ => true, error => error.Exception.Match(
            Some: ex => ex is Marten.Exceptions.ConcurrentUpdateException or JasperFx.Events.EventStreamUnexpectedMaxEventIdException
                ? Lift(session, model, 0L, error)
                : IO.pure(Fin<GraphReceipt>.Fail(new GraphFault.TxnConflict(model, ex.Message))),
            None: () => IO.pure(Fin<GraphReceipt>.Fail(new GraphFault.TxnConflict(model, error.Message)))));

    static void Blame(IDocumentSession session, GraphWriteStamp stamp, ProjectionContext frame) {
        session.SetHeader("actor", stamp.Actor.Subject);
        session.SetHeader("origin", stamp.Origin.ToString());
        session.SetHeader("tenant", frame.Tenant.ToString("x32"));
        // The project slot is the ProjectRollup's grouping fact — stamped at write time, never joined at fold time.
        stamp.Project.IfSome(p => session.SetHeader("project", p.Value.ToString()));
    }

    // The three read legs differ ONLY in the fetch shape and the (version, nodes, edges) triple each extracts; the
    // Some -> store.element.read receipt and the None -> ModelAbsent absence arm are ONE projection the legs share
    // (Received), so the receipt construction and the absence rail are owned once, never re-spelled per read modality.
    static IO<Fin<GraphReceipt>> ReadGraph(IDocumentSession session, ModelId model, ProjectionContext frame, long mark, CancellationToken cancellationToken) =>
        IO.liftAsync(() => session.Events.FetchLatest<GraphProjection>(model.Value, cancellationToken))
            .Map(p => Received(model, Optional(p), static g => (g.Version, g.Graph.Nodes.Count, g.Graph.Edges.Length), frame, mark));

    static IO<Fin<GraphReceipt>> ReadGraphAsOf(IDocumentSession session, ModelId model, TimeCut cut, ProjectionContext frame, long mark, CancellationToken cancellationToken) =>
        ProjectAsOf(session, model, cut, cancellationToken)
            .Map(p => Received(model, p, static g => (g.Version, g.Graph.Nodes.Count, g.Graph.Edges.Length), frame, mark));

    static IO<Fin<GraphReceipt>> ReadState(IDocumentSession session, ModelId model, ProjectionContext frame, long mark, CancellationToken cancellationToken) =>
        IO.liftAsync(() => session.Events.FetchStreamStateAsync(model.Value, cancellationToken))
            .Map(s => Received(model, Optional(s), static state => (state.Version, 0, 0), frame, mark));

    static Fin<GraphReceipt> Received<T>(ModelId model, Option<T> found, Func<T, (long Version, int Nodes, int Edges)> read, ProjectionContext frame, long mark) =>
        found.Match(
            Some: value => read(value) switch { (long version, int nodes, int edges) => Fin<GraphReceipt>.Succ(new GraphReceipt("store.element.read", model, version, nodes, edges, frame.Elapsed(mark), frame.Now(), frame.Correlation)) },
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

| [INDEX] | [POLICY]               | [VALUE]                                    | [BINDING]                                                       |
| :-----: | :--------------------- | :----------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | one txn owner          | identity + event in one `IDocumentSession` | `IdentityStore.Stamp` then `SaveChangesAsync`                   |
|  [02]   | read consistency       | `FetchLatest<GraphProjection>`             | inline document or live tail fold; read-your-writes             |
|  [03]   | AS-OF fold             | `AggregateStreamAsync(version\|timestamp)` | version XOR instant; reuses `GraphDelta.ReplayOnto`             |
|  [04]   | optimistic concurrency | `Append(model, delta, expectedVersion)`    | racing writer aborts → `StreamVersionConflict`                  |
|  [05]   | exclusive escalation   | `CommitExclusive` op case                  | `FetchForExclusiveWriting`; refusal → `TxnConflict` 8303        |
|  [06]   | frame injection        | `StoreActor` + `ProjectionContext`         | AppHost fills slots at the port; no AppHost type crosses down   |
|  [07]   | naming lineage         | `NameLineage` co-committed rows            | kernel `Track(prior, rebuilt)` reads a durable prior generation |
|  [08]   | coordination edges     | `ModelLink` rows + `Link` op case          | cross-model references durable, project-scoped, blame-stamped   |
|  [09]   | project rollup         | `ProjectRollup` header-sliced, async       | roster + watermark only; never a second delta materializer      |

## [05]-[FAULT_TABLES]

- Owner: `FaultBand` the `[SmartEnum<int>]` band-allocation registry — ONE row per Persistence fault decade (21 own decades) plus the pinned MIRROR rows reserving every foreign registry's integers against Persistence claimants, mirroring the seam `Rasm.Element` registry shape (its own/mirror discriminant included); `GraphFault` the store-rail band this page hosts as the registry's own exemplar (8300-8303), the `[Union]` over the kernel `Rasm.Domain.Expected` every projection/store failure rails.
- Entry: every Persistence fault union derives `Code => FaultBand.<Row> + n` through the generated implicit `SmartEnum`-to-`int` conversion — one line, never a `.Value` spelling and never a bare integer literal; a duplicate band integer FAILS the generated key lookup at type initialization, so cross-page disjointness is type-enforced, never prose (per-page decade prose is the deleted form; this registry is the one pointer).
- Growth: a new band is ONE row here plus the owning page's union derivation; a new case inside a band is one union case whose offset stays inside the row's decade; a foreign registry change is one mirror-row edit; zero new surface — a page-local band constant, a loose `Error.New(83xx)` integer, or a prose decade table is the deleted form.
- Boundary: own rows carry `Mirror: false` and name their owning page anchor for telemetry docs; mirror rows reserve the integer only (AppHost 1xxx/4100-4810, Compute 2200-2299 + Remote `WireFault` 4520-4532, AppUi 6xxx, the AEC registry 2300/2350/2400/2450/2470/2500/2600/2700-2710, kernel-substrate 9104) and no Persistence union ever derives from a mirror; the folded-transaction concurrency conflict is the registered `GraphFault.TxnConflict` sub-band row 8303, NEVER a loose 7001; `Element/authority` composes `IdentityFault` (8340) and carries no band of its own; `Version/timetravel`/`Version/merge`/`Version/provenance`/`Query/lane` are the no-band total algebras.

```csharp signature
// --- [TABLES] ----------------------------------------------------------------------------
// The Persistence band-allocation registry: a new band is ONE row; a duplicate integer fails the
// generated key lookup at type initialization — disjointness is type-enforced, never prose. Own rows
// name the declaring page; mirror rows reserve foreign registries' integers against every Persistence
// claimant. Every Persistence fault union derives `Code => FaultBand.<Row> + n` through the generated
// implicit int conversion.
[SmartEnum<int>]
public sealed partial class FaultBand {
    public static readonly FaultBand RemoteStore  = new(5400, "Store/blobstore#RemoteStoreFault");
    public static readonly FaultBand Embedded     = new(7710, "Store/provisioning#EmbeddedFault (absorbs the loose 7701/7702)");
    public static readonly FaultBand Sync         = new(8250, "Version/ledger#SyncFault");
    public static readonly FaultBand Commit       = new(8260, "Version/commits#CommitFault (minted; the wire-decode arm is DecodeDrift/Category=Wire — no second union)");
    public static readonly FaultBand Egress       = new(8270, "Version/egress#EgressFault");
    public static readonly FaultBand Retention    = new(8280, "Version/retention#RetentionFault");
    public static readonly FaultBand Recovery     = new(8290, "Version/recovery#RecoveryFault");
    public static readonly FaultBand Graph        = new(8300, "Element/graph#GraphFault (registry host; 8303 = folded-txn conflict)");
    public static readonly FaultBand Codec        = new(8310, "Element/codec#CodecFault (legal 831x-833x stride)");
    public static readonly FaultBand Identity     = new(8340, "Element/identity#IdentityFault (Element/authority composes, no own band)");
    public static readonly FaultBand Columnar     = new(8350, "Query/columnar#ColumnarFault (whole decade 8350-8356)");
    public static readonly FaultBand Cypher       = new(8360, "Query/cypher#CypherFault (renamed off the GraphFault simple-name collision)");
    public static readonly FaultBand Topology     = new(8370, "Query/topology#TopologyFault");
    public static readonly FaultBand Server       = new(8380, "Store/provisioning#ServerFault (re-banded off 835x)");
    public static readonly FaultBand Tabular      = new(8390, "Ingest/tabular#TabularFault (re-banded off 837x)");
    public static readonly FaultBand Schedule     = new(8400, "Ingest/schedule#ScheduleFault");
    public static readonly FaultBand Retrieval    = new(8410, "Query/retrieval#RetrievalFault (minted)");
    public static readonly FaultBand Federation   = new(8420, "Query/federation#FederationFault");
    public static readonly FaultBand Coordination = new(8430, "Store/coordination#CoordinationFault");
    public static readonly FaultBand GeoIngest    = new(8440, "Ingest/geospatial#GeoIngestFault");
    public static readonly FaultBand WideColumn   = new(8450, "Query/cache#WideColumnFault");
    // Pinned mirrors — foreign registries' integers reserved; no Persistence union derives from these rows.
    public static readonly FaultBand AppHostCore  = new(1000, "Rasm.AppHost 1xxx", mirror: true);
    public static readonly FaultBand ComputeCore  = new(2200, "Rasm.Compute 2200-2299", mirror: true);
    public static readonly FaultBand Component    = new(2300, "Rasm.Materials/Component", mirror: true);
    public static readonly FaultBand Generation   = new(2350, "Rasm.Generation (reserved)", mirror: true);
    public static readonly FaultBand Geometry     = new(2400, "Rasm kernel GeometryFault 2400-2449", mirror: true);
    public static readonly FaultBand Material     = new(2450, "Rasm.Materials/Appearance", mirror: true);
    public static readonly FaultBand Projection   = new(2470, "Rasm.Materials/Projection", mirror: true);
    public static readonly FaultBand Element      = new(2500, "Rasm.Element ElementFault", mirror: true);
    public static readonly FaultBand Bim          = new(2600, "Rasm.Bim BimFault", mirror: true);
    public static readonly FaultBand Fabrication  = new(2700, "Rasm.Fabrication 2700-2710", mirror: true);
    public static readonly FaultBand AppHostWire  = new(4100, "Rasm.AppHost wire 4100-4810", mirror: true);
    public static readonly FaultBand ComputeWire  = new(4520, "Rasm.Compute Remote WireFault 4520-4532", mirror: true);
    public static readonly FaultBand AppUi        = new(6000, "Rasm.AppUi 6xxx", mirror: true);
    public static readonly FaultBand Kernel       = new(9104, "Rasm kernel-substrate Fault.UnsupportedCode", mirror: true);

    public string Owner { get; }
    public bool Mirror { get; }
    private FaultBand(int key, string owner, bool mirror = false) : this(key) => (Owner, Mirror) = (owner, mirror);
}

// --- [ERRORS] --------------------------------------------------------------------------
// The projection/store fault band: a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the
// seam `ElementFault` (2500) and `BimFault` (2600) realize — NOT `LanguageExt.Common.Expected`, whose
// `(string,int,Option)` ctor is the deleted form. Band membership derives `Code => FaultBand.Graph + n`
// through the registry row, so the typed case lifts BARE onto `Fin<T>`/`Validation<Error,T>` with no
// `.ToError()` hop and a recovery reads `error.IsType<GraphFault.StreamVersionConflict>()` /
// `error.HasCode(8301)` / `error.Category()`, never a message substring. `TxnConflict` (8303) is the
// folded-transaction sub-band row — the advisory-lock/serialization refusal of the `CommitExclusive`
// escalation, never a loose 7001. No `[GenerateUnionOps]` — the kernel union-ops generator is strictly opt-in.
[Union]
public abstract partial record GraphFault : Expected, IValidationError<GraphFault> {
    private GraphFault() : base() { }

    public sealed record DeltaRejected(string Detail, int InvariantCode) : GraphFault;
    public sealed record StreamVersionConflict(ModelId Model, long Expected, long Actual) : GraphFault;
    public sealed record ModelAbsent(ModelId Model) : GraphFault;
    public sealed record TxnConflict(ModelId Model, string Detail) : GraphFault;

    public override int Code => FaultBand.Graph + Switch(
        deltaRejected:         static _ => 0,
        streamVersionConflict: static _ => 1,
        modelAbsent:           static _ => 2,
        txnConflict:           static _ => 3);

    public override string Message => Switch(
        deltaRejected:         static c => $"<graph-delta-rejected:{c.Detail}:{c.InvariantCode}>",
        streamVersionConflict: static c => $"<graph-version-conflict:{c.Model.Value}:{c.Expected}!={c.Actual}>",
        modelAbsent:           static c => $"<graph-model-absent:{c.Model.Value}>",
        txnConflict:           static c => $"<graph-txn-conflict:{c.Model.Value}:{c.Detail}>");

    public override string Category => Switch(
        deltaRejected:         static _ => "Delta",
        streamVersionConflict: static _ => "Concurrency",
        modelAbsent:           static _ => "Absent",
        txnConflict:           static _ => "Concurrency");

    public static GraphFault Create(string message) => new DeltaRejected(message, 0);
}
```

| [INDEX] | [POLICY]            | [VALUE]                           | [BINDING]                                                |
| :-----: | :------------------ | :-------------------------------- | :------------------------------------------------------- |
|  [01]   | band disjointness   | `[SmartEnum<int>]` key uniqueness | duplicate integer fails at type initialization           |
|  [02]   | code derivation     | `Code => FaultBand.<Row> + n`     | implicit int conversion; never `.Value`, never a literal |
|  [03]   | foreign reservation | pinned mirror rows                | no Persistence union derives from a mirror               |
|  [04]   | folded-txn conflict | `GraphFault.TxnConflict` 8303     | registered sub-band row, never a loose 7001              |
