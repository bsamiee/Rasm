# [PERSISTENCE_ELEMENT_GRAPH]

Rasm.Persistence persists each `Rasm.Element` graph as one Marten stream keyed by `ModelId`: `GraphCreated`, `GraphRevised`, and `GraphRetired` carry seam-validated `GraphDelta` bodies, and the inline `GraphProjection` and the AS-OF reconstruction reuse the one `GraphDelta.ReplayOnto` fold.

`IdentityStore.Stamp` queues the provider-bound `IdentityWriter` on the same `IDocumentSession`, so identity, event, lineage, and inline projection commit once without a second writer; `GraphWriteStamp` carries actor, origin, optional project, and the boot-composed identity writer on write cases alone; `ProjectionContext` seats the kernel `Rasm/Parametric/projections#TIMELINE` `MonotonicTimeline` beside the kernel `CorrelationId`/`TenantContext` pair as VALUES, so every leg reads one tenancy, one correlation, and one monotonic frame with no per-seam lift.

Fault codes read the kernel `Rasm/Domain/rails#FAULT_BAND` roster, which allocates every Persistence band. This folder holds no registry of its own: one registry cannot collide with itself, so the kernel's static `Disjoint` proof partitions the WHOLE code space by `BandKind` at type initialization, where a per-folder registry with pinned mirror rows agreed with its neighbours only by inspection and re-stated fourteen foreign decades this folder never allocates from.

## [01]-[INDEX]

- [02]-[STREAM_GRAIN]: model-stream identity, the `GraphEvent` body family, optimistic append, and the schema-keyed event registration.
- [03]-[GRAPH_PROJECTION]: Inline `SingleStreamProjection` folding `GraphDelta` into the STJ-rehydratable `GraphProjection` over the seam `GraphDelta.ReplayOnto`, the materialized `ElementGraph` read boundary, and the read-your-writes consistency boundary.
- [04]-[STORE_RAIL]: Persistence-owned frame shapes (`StoreActor`/`ProjectionContext`) beside the imported `RecoveryObjective`, the one `GraphStoreOp` operation family over the generated total `Switch`, the session bracket, the exclusive-lock escalation, AS-OF reconstruction, the durable naming-lineage rows, and the co-transactional identity commit.
- [05]-[FAULT_TABLES]: the folder's routing pointer at the kernel `FaultBand` roster, and the `GraphFault` band this page owns on it.

## [02]-[STREAM_GRAIN]

- Owner: `ModelId` the `[ValueObject<Guid>]` per-model stream key under the `IObjectFactory` floor; `GraphEvent` the `[Union]` event-body family every model stream appends, carrying the `Body`/`Lifecycle` projections the `Version/ledger#CHANGEFEED` `OpLog.Project` reads off each Marten event; `EventLifecycle` the `[SmartEnum<string>]` create/revise/retire verb each event row carries; `ElementSchema` the static surface owning the `StoreOptions` event registration, the strong-typed value registration, the inline projection registration, and the per-model stream-start and append legs over the one `IDocumentSession`.
- Cases: `GraphCreated(Header Header, GraphDelta Delta)` opens a stream carrying the `Rasm.Element` `Header` (`ReleaseVersion`/`ModelView`/`GeoReference`/`Tolerance`/`Instant`/`StepHeader`) AND the assembled opening `GraphDelta` (the `Projection/projection#PROJECTION_CONTRACT` `Assemble` merged model-creating delta), so a model is created in ONE event rather than an empty open beside a separate content commit; `GraphRevised(GraphDelta Delta)` is the steady-state append; `GraphRetired(GraphDelta Delta, string Reason)` carries the retirement delta whose `GraphDelta` removes the retired nodes/edges — so retirement is a real convergent delta the projection folds, never an out-of-band tombstone; the event body is ALWAYS the seam `GraphDelta`, NEVER a whole-graph snapshot, because the delta replays deterministically through `GraphDelta.ReplayOnto` and a whole-graph body bloats every append by the model size.
- Entry: `public static StoreOptions Configure(StoreOptions opts, NpgsqlDataSource source)` registers the event types, the strong-typed `ModelId`/`NodeId` value types, the inline `GraphProjection` self-aggregating snapshot, and the metadata columns once at boot, the composition root folding each higher-stratum `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` contribution over the returned options; `public static StreamAction Open(IDocumentSession session, ModelId model, Header header, GraphDelta opening)` calls `session.Events.StartStream<GraphProjection>(model.Value, new GraphEvent.GraphCreated(header, opening))` so the assembled opening delta is the one model-creating event; `public static StreamAction Append(IDocumentSession session, ModelId model, GraphEvent body, long expectedVersion)` appends with the inline optimistic version guard so a concurrent writer racing the same stream version aborts at `SaveChangesAsync` rather than silently interleaving.
- Auto: `StreamIdentity.AsGuid` keys one stream per `ModelId`. `EventAppendMode.Rich` remains invariant for authoring and re-ingest because `actor`/`origin`/`tenant` headers are durable blame inputs; Marten's `Quick` modes trade away metadata richness and therefore do not admit this stream. `GraphDelta` is the seam-validated event body. `RegisterValueType<ModelId>()`/`RegisterValueType<NodeId>()` preserve typed keys, and `UseSystemTextJsonForSerialization(ElementJson.Options, …)` binds the one generated serializer profile.
- Receipt: a stream open rides `store.element.open`, a delta append rides `store.element.commit` carrying the delta node/edge counts, a retirement rides `store.element.retire`; the `StreamAction.Version` is the optimistic guard the next append reads.
- Packages: Marten (`StartStream`/`Append`/`StreamAction`/`EventAppendMode`/`StreamIdentity`/`RegisterValueType`/`Snapshot`/`UseSystemTextJsonForSerialization`/`MetadataConfig`), Npgsql, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new durable change kind is one `GraphEvent` case, one `EventLifecycle` row, and one projection `Apply` method the convention discovery forces; a richer model header is one field on the seam `Header`; a per-spatial-partition grain is one `ModelId` minting policy, never a second stream shape; zero new surface — a per-`NodeId` stream, a whole-graph event body, a second event table, or a bespoke `OpLogEntry` store beneath Marten is the deleted form because Marten owns the durable append and the rebuildable read views, and the `Version/` engine projects FROM these events (`Version/ledger#CHANGEFEED` `OpLog.Project` over `e.Data.Body`/`e.Data.Lifecycle`).
- Boundary: stream grain is ONE stream PER MODEL (or per spatial partition), never per-`NodeId`, and the event body is the `GraphDelta`, never a whole-graph snapshot; the `GraphDelta` is the seam-owned graph-mutation record the projection folds immutably through `GraphDelta.ReplayOnto`, so the durable history is a delta log the engine replays and the rehydrated graph is bit-identical to the live state at any version because the fold is the ONE `GraphDelta.ReplayOnto` the AS-OF reconstruction also runs; the optimistic append (`Append(stream, expectedVersion, …)`) is the inline guard, `AppendOptimistic` the read-then-guard, and the `GraphStoreOp.CommitExclusive` case the stream-level advisory-lock escalation (`FetchForExclusiveWriting<GraphProjection>`, `#STORE_RAIL`); the `GraphRetired` delta is a real convergent retirement the projection folds and the `Version/retention#RETENTION_CLASSES` sweep reclaims, never an `ArchiveStream` that hides the events from the fold (archive is the AS-OF cut boundary, not retirement); a `GraphCreated` carries the `Header` so the stream's `ReleaseVersion`/`GeoReference`/`Tolerance` are the first folded fact and every later delta's measure quantization (`Element/codec#CONTENT_ADDRESS`) reads the header tolerance; `EventAppendMode` trades metadata richness for throughput as a config value, never a per-call branch; the `GraphEvent` is the body family `Version/ledger#CHANGEFEED` lifts (`OpLog.Project(IEvent<GraphEvent>)` reads `e.Data.Body`/`e.Data.Lifecycle`), so this owner's body shape is the changefeed's input contract; `Configure` is the spine's `StoreOptions` seat and registers ONLY spine-owned mappings — a rolling-window declaration over a `Query`/`Version` document type makes this S0 surface name an S2/S3 type, the forbidden upward edge, so each such family publishes its own `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` contribution at its own owner and the composition root folds it over these options, one `StoreOptions` value threaded through both.

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
using Rasm.Domain;
using Rasm.Parametric;                             // MonotonicTimeline/MonotonicStamp — the frame's clock half
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Element;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<Guid>]
public readonly partial struct ModelId {
    public static ModelId New() => Create(Guid.CreateVersion7());
}

// `ProjectId` names the altitude above the per-model stream: federated disciplines live in separate `ModelId`
// streams, and this id is the grouping key the `project` blame header carries and the `#STORE_RAIL`
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
// `GraphEvent` is the body family every model stream appends, ALWAYS carrying the seam `GraphDelta`
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
        // `Projections.Add<GraphProjection>` beside this call is the deleted double-registration, re-treating the
        // aggregate as a raw `IProjection`; the inline aggregate IS the materialized view that bounds replay.
        opts.Projections.Snapshot<GraphProjection>(SnapshotLifecycle.Inline);
        // `ProjectRollup` runs ASYNC by construction — a dashboard/roster view, never interactive correctness —
        // and the coordination edges index by project for the federated selection reads AND by the
        // `(FromModel, FromNode)` pair the project view's one-hop expansion resolves against.
        opts.Projections.Add(new ProjectRollup(), ProjectionLifecycle.Async);
        opts.Schema.For<ModelLink>().Index(static l => l.Project);
        opts.Schema.For<ModelLink>().Index(static l => new { l.FromModel, l.FromNode });
        // `Configure` registers only spine-owned mappings: the durable reference-axis rows here outlive every
        // window and partition nothing. A higher-stratum family whose whole table ages out publishes its own
        // `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` contribution, which the composition root folds
        // over these options after this call — a spine registration naming a `Query`/`Version` document type
        // walks the forbidden upward edge (`ARCHITECTURE#STRATA`).
        return opts;
    }

    // `GraphCreated` carries BOTH the `Header` AND the assembled opening `GraphDelta` (the one
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

| [INDEX] | [POLICY]          | [VALUE]                                       | [BINDING]                                                       |
| :-----: | :---------------- | :-------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | stream grain      | one stream per `ModelId`                      | `StreamIdentity.AsGuid`; never per-`NodeId`                     |
|  [02]   | event body        | the seam `GraphDelta`                         | never a whole-graph snapshot; folds via `GraphDelta.ReplayOnto` |
|  [03]   | append mode       | `EventAppendMode.Rich`                        | blame headers remain durable on every write                     |
|  [04]   | optimistic guard  | `Append(stream, expectedVersion, …)`          | concurrent same-version writer aborts at `SaveChangesAsync`     |
|  [05]   | strong-typed keys | `RegisterValueType<ModelId/NodeId>`           | typed stream key + document id, never a bare Guid/string        |
|  [06]   | partition seat    | family-published `RollingWindow` contribution | spine registers spine-owned mappings only; no upward edge       |

## [03]-[GRAPH_PROJECTION]

- Owner: `GraphProjection` the inline self-aggregating snapshot AGGREGATE Marten folds one model stream into (registered through `opts.Projections.Snapshot<GraphProjection>(SnapshotLifecycle.Inline)` — the document carries the `Create`/`Apply` convention methods Marten discovers and wraps as a single-stream projection internally, so the record is the aggregate, never a hand-derived `SingleStreamProjection<,>` subclass) — the STJ-rehydratable carrier of one model's `Header`, node map, edge array, and folded version, written in the append transaction, materializing the seam `ElementGraph` ONCE through `ElementGraph.Of` at the read boundary; the aggregate's `Create`/`Apply` convention methods owning the one `GraphDelta.ReplayOnto` fold over the seam graph; faults rail the `#FAULT_TABLES` `GraphFault` band.
- Cases: `Create(GraphCreated)` seeds the genesis through `ElementGraph.Genesis(header)` and replays the opening delta; `Apply` replays each recorded revision or retirement through `GraphDelta.ReplayOnto`.
- Entry: `Create(IEvent<GraphEvent.GraphCreated>)` seeds `Model` from `StreamId` and `Version` from the stored message envelope; body-only `Apply(GraphRevised)` and `Apply(GraphRetired)` advance the aggregate version once per event. `Graph` memoizes the frozen `ElementGraph` materialization.
- Auto: the projection registers `SnapshotLifecycle.Inline` so the folded `GraphProjection` document is written in the SAME transaction as the appended events — a `Read` after a `Commit` in the same logical unit sees the new state with no daemon lag — and the inline aggregate IS the periodic materialized view, so a deep stream loads the head document rather than re-folding from genesis; the projection stores the STJ-serializable primitives (`Header`, `ImmutableDictionary<NodeId, Node>`, `ImmutableArray<Relationship>`) because the seam `ElementGraph` is a sealed read-snapshot class with no deserialization path, and the live authoring graph uses the seam's `ImmutableDictionary`/HAMT structural-sharing form (`Graph/delta#GRAPH_DELTA`) while `ElementGraph.Of` freezes to `FrozenDictionary` + the incidence index + the lazy `QuikGraph` view only at the `Graph` materialization boundary, so the delta path stays O(log n) structural-sharing and the read snapshot stays O(1) lookup; `From` is the ONLY mint (each `Create`/`Apply` rebuilds the document and the lazy `Graph` memo from the folded snapshot) so a `with` can never alias a stale materialized graph.
- Receipt: a projection fold rides `store.element.project` carrying the folded delta count.
- Packages: Marten (`SingleStreamProjection`/`SnapshotLifecycle`/`IEvent<T>`), Rasm.Element (`ElementGraph`/`ElementGraph.Genesis`/`ElementGraph.Of`/`GraphDelta`/`GraphDelta.ReplayOnto`/`Header`/`Node`/`NodeId`/`Relationship`), LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Collections.Immutable/Frozen, BCL inbox.
- Growth: a new event arm is one projection `Apply` method the convention discovery forces; the cross-model rollup is the realized `#STORE_RAIL` `ProjectRollup` (header-sliced, roster + watermark only), so a richer project view is one field on `ProjectGraph`, never a second fold of the same delta; a co-transactional columnar egress is the `Query/lakehouse#FLAT_TABLE_EGRESS` `FlatTableProjection`; zero new surface — a hand-rolled stream folder, a second materializer, or a per-read whole-stream replay is the deleted form because the inline projection IS the materialized read and the AS-OF fold reuses the same `GraphDelta.ReplayOnto`.
- Boundary: the inline projection is the READ-YOUR-WRITES consistency boundary — authoritative containment, topology, and void-resolution reads go through this folded `GraphProjection.Graph`, NEVER an async lane, because an async daemon view lags the write (`Query/lane#READ_ROUTING` routes interactive correctness here by construction); the analytical lanes (`Query/columnar`, `Query/cypher`) are explicitly `ProjectionLifecycle.Async` with a staleness watermark and interactive-correctness reads block on `WaitForNonStaleProjectionDataAsync`; the projection apply is the SAME `GraphDelta.ReplayOnto` fold the `Version/timetravel#TIME_TRAVEL` AS-OF reconstruction runs (the live authoring path produces the deltas it replays, via the seam `Graph/delta#GRAPH_DELTA` `WorkingGraph.Apply`), so there is exactly ONE delta→graph materializer and a historical fold equals the live state field-for-field; the projection NEVER stores the seam `ElementGraph` directly (it has no public deserialization constructor — a sealed read-snapshot class whose only mint is `Of`/`Genesis`/`Apply`), so the document carries the rehydratable `Header`/node-map/edge-array and `Graph` materializes the frozen snapshot once through `ElementGraph.Of`; the inline aggregate is the materialized read floor bounding replay, never a second source of truth — `store.Advanced.RebuildSingleStreamAsync<GraphProjection>(model)` replays one stream's inline projection from zero when the fold logic changes; the projection never re-validates the delta because the projector `IGraphConstraint` and the seam `LegalLink` already gated it at the write boundary — re-validation in the projection is the deleted form because a validated delta in the stream is total by construction and a fold-time fault is a deployment defect surfaced as `GraphFault`, not a recoverable data path.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
// Marten folds the model stream into this inline single-stream aggregate inside the append transaction, so a
// same-unit `Read` is read-your-writes consistent. `GraphProjection` carries the STJ-rehydratable PRIMITIVES
// (`Header` + node map + edge array + version), NOT the seam `ElementGraph` — a sealed read-snapshot class with
// no deserialization path whose only mint is `Of`/`Genesis`/`Apply` — and materializes the frozen `ElementGraph`
// ONCE through `ElementGraph.Of` behind a memoized `Graph` accessor, the one place the incidence index,
// `QuikGraph` view, and `Bake` memo build. `GraphDelta.ReplayOnto` is the one immutable fold the AS-OF
// reconstruction also runs,
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

- Owner: `StoreActor` the Persistence-owned `[ComplexValueObject]` actor value (subject + role claims) AppHost's composition root MAPS its richer `Principal` onto at the port boundary — the AppHost simple name never crosses down, mirroring the `Grant`/`Capability` never-share-a-name law; `ProjectionContext` the Persistence-owned injected frame seating the kernel `MonotonicTimeline` beside the kernel `CorrelationId`/`TenantContext` pair as VALUES, with `Since` the one elapsed read every leg takes (AppHost fills the slots from its own `ClockPolicy` and the kernel causal frame at the boundary; every Persistence page threads this frame, never an app-platform type, and a leg emitting a signal, a receipt, or an RLS predicate reads the kernel pair straight off the frame — the per-seam lift from a raw `Guid`/`UInt128` slot is the deleted form, and the raw key scalars survive only where a durable column, an object-name prefix, or a series key demands the packed value through `TenantId.Value`); `RecoveryObjective` the `Rasm.AppHost/Runtime/profiles` declaration this rail IMPORTS whole and `Version/recovery` gauges against — a port shape earns its seat by RE-SHAPING the crossing, so an identical `(Rpo, Rto)` record here is a twin and the composition root threads the settled window in by value instead; `NameLineage` the durable REFERENCE-axis row persisting the kernel `Rasm/Spatial/naming` generational `Track(prior, rebuilt)` pairing across sessions; `ProjectId`/`LinkKind`/`ModelLink` the federated-coordination vocabulary — the durable cross-model reference edge IFC cannot carry, co-committed like `NameLineage`; `ProjectGraph`/`ProjectRollup` the realized project-altitude `MultiStreamProjection` sliced by the `project` blame header (async, roster + watermark only — never a second delta materializer); `GraphStoreOp` the `[Union]` operation family every durable graph interaction is a value in; `GraphStore` the static surface owning the one bracket over the generated total `Switch` — pooled session acquisition, the strong-typed append, the exclusive-lock escalation, the inline-projection read, the AS-OF fold, the co-transactional identity commit, and provider-fault conversion to `GraphFault`; `GraphReceipt` the typed per-op evidence carrying the model, the resulting version, and the elapsed `Duration`.
- Cases: write cases carry their required `ElementIdentity` and `GraphWriteStamp`; `Link` carries its project-scoped links and stamp; `Read`, `ReadAsOf`, and `State` carry only their read discriminants. `ModelLink` covers directed and symmetric cross-model relationships, validity interval, and extensible attributes; `LinkKind` carries directionality as row data.
- Entry: `Run(IDocumentSession, GraphStoreOp, ProjectionContext, CancellationToken)` dispatches the closed family, the token threading to every provider await so caller cancellation reaches the store instead of dying at the rail boundary. Write cases stamp their carrier and queue identity before `SaveChangesAsync`; read cases require no dummy identity, actor, origin, or project. `ReadAsOf` passes a nullable version only when the cut carries one and otherwise passes only the timestamp, preserving the version-XOR-time contract.
- Auto: the bracket runs the op through the generated total `GraphStoreOp.Switch` (compile-time exhaustive over the closed family — a new op breaks the build at the `Run` dispatch, never a runtime-silent `_` arm) and `SaveChangesAsync` commits events, the queued identity upsert, the lineage rows, and the inline projection in one Postgres transaction — there is no separate identity ORM and no two-phase dance because the identity write is the one model-derived statement `IdentityStore.Stamp` queues on the session; the read op calls `FetchLatest<GraphProjection>(model)` which returns the inline document when present or live-folds the tail, so a read after a commit in the same unit is consistent; the AS-OF op binds either a `version` or a `timestamp` (one or the other, never both) from the `TimeCut` so an historical read folds the SAME `GraphDelta.ReplayOnto` deterministically; a `Commit`/`CommitExclusive` carrying `Some(NameLineage)` stores the lineage rows in the same session so the kernel `NameTable.Track(prior, rebuilt)` reads a durable PRIOR generation on the next session — a durable projection of the kernel lineage as string pairs, never the kernel interior types crossing a wire (naming's interior-type law holds), the REFERENCE axis distinct from the merge-consumed per-node `NamingHash` CONTENT receipt; provider exceptions convert to `GraphFault` at the one bracket boundary and the interior never sees a raw `Marten.Exceptions.MartenException`, while caller cancellation passes through untyped.
- Receipt: an `Open`/`Commit`/`Retire` rides `store.element.<verb>` carrying the resulting `StreamAction.Version`; a `CommitExclusive` rides `store.element.commit-exclusive`; a `Link` rides `store.element.link` carrying the landed edge count; a `Read`/`ReadAsOf` rides `store.element.read` carrying the folded node count; the identity co-commit rides `store.element.identity` carrying the `NodeId` count (`Element/identity#ELEMENT_IDENTITY`); the op crosses the `rasm.persistence.element.append` veto and the settled receipt fires `rasm.persistence.element.committed` through the `Store/observability#HOOK_RAIL` `PersistenceHooks.Guarded` composition adapter — hook points are composition-mounted values, never rail parameters.
- Packages: Marten (`IDocumentSession`/`IQuerySession`/`SetHeader`/`SaveChangesAsync`/`FetchLatest`/`FetchForExclusiveWriting`/`IEventStream<T>.AppendOne`/`AggregateStreamAsync`/`FetchStreamStateAsync`/`Store`), Rasm (`Rasm/Parametric/projections#TIMELINE` `MonotonicTimeline`/`MonotonicStamp` — the frame's clock half; `Rasm/Domain/rails#FAULT_BAND` `FaultBand`; `Rasm/Domain/validation#FACTORY_BRIDGE` `Op.OrDefault`), Rasm.AppHost (project — `RecoveryObjective`), LanguageExt.Core (`IO`/`Fin`/`Option`), NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new durable interaction is one `GraphStoreOp` case and one branch in the generated total `Switch` the closed family forces; a new read modality is one op case discriminating on its input (a key resolves to one graph, a cut to an AS-OF graph, a state probe to a head version) whose leg projects through the one `Received` receipt fold (the `(version, nodes, edges)` extractor the only per-leg difference); a new frame ingredient is one slot on `ProjectionContext`, never a new signature parameter; a new durability dimension is one column on the AppHost `RecoveryObjective`, never a record minted here; a same-session SQL side-write (a coordination cursor, an outbox advance) is `IDocumentOperations.QueueSqlCommand` inside the one transaction, never a second connection; zero new surface — a repository per model, a per-verb service, an injected `persist` delegate, a per-read-leg receipt-construction-and-absence-arm copy, an AppHost frame type on a signature, or a separate identity transaction is the deleted form because the one rail owns the bracket, the one session owns identity-with-event atomicity, the one `Received` fold owns the receipt-and-absence projection, and the op family discriminates by value shape through the generated `Switch`.
- Boundary: the `IDocumentSession` is the one transaction owner for identity with event — the `ElementIdentity` row lands as the ONE model-derived upsert the `Element/identity#ELEMENT_IDENTITY` `IdentityStore.Stamp` owner queues on the same session as the appended events, so a single `SaveChangesAsync` commits both with no free two-ORM atomicity and no EF-versus-Marten gap (the EF identity model of `Element/identity` names the one `element_identity` relation for DDL and every H3/PostGIS/pgvector/ACL read while holding zero write authority — the session is the only writer); the blame headers are the WRITE side of the read-side blame contract — `Stage` stamps `actor`/`origin`/`tenant` so every appended event carries the slots `MetadataConfig.HeadersEnabled` persists, `Version/ledger#CHANGEFEED` `OpLog.Project` reading `actor` off them and `Version/timetravel#TIME_TRAVEL` `AuthorshipOf` admitting both on the rail; adjudication left those headers behind — `Version/ledger` breaks its `(Hlc, OriginStoreId)` LWW tie on `OperationId.Origin`, the store id `DotSource` mints into every dot, so ordering stays deterministic across peers whatever a header carries and the `Guid.Empty` bucket that once collapsed every origin into one is unreachable; omitting the stamp is still the deleted form because `AuthorshipOf` rails the blame/scrub read instead of inventing an anonymous actor or zero origin; the frame ingredients cross as VALUES on the Persistence-owned shapes this section defines — a `ClockPolicy` or `Principal` parameter on any Persistence signature is the named leak, since both re-shape at the boundary, while `RecoveryObjective` crosses as ITSELF because nothing re-shapes it and this package sits a rank above the spine that declares it, and the kernel causal frame (`CorrelationId`, `TenantId`/`TenantContext`, `ReceiptEnvelope`, `ReceiptSinkPort`) is S0 vocabulary this package composes directly and therefore SEATS on the frame rather than being re-derived at each seam, so every Persistence receipt spells `CorrelationId` (a neutral-`Guid` receipt field is the twin the kernel forecloses), every RLS predicate, blame header, and meter tag spells `TenantContext.Entry`, every census wire spells `Slug`, and `TenantId.Value` is reached only where a durable column, an object-name prefix, an AAD digest, or a series key packs the raw scalar; every Persistence page re-threads onto `ProjectionContext`/`StoreActor` in its own rebuild and takes the durability window as a `RecoveryObjective` parameter; the read op is read-your-writes through the inline projection and NEVER routes to an async analytical lane; the AS-OF op binds `version` XOR `timestamp` so a precise cut pins a version and an instant cut binds the wall clock, and the fold reuses the one `GraphDelta.ReplayOnto` so an historical graph equals the live state; optimistic concurrency is `Commit(model, delta, expectedVersion, …)` whose inline `Append(stream, expectedVersion, body)` aborts a racing same-version writer at `SaveChangesAsync` — surfacing as `Marten.Exceptions.ConcurrentUpdateException` wrapping the inner `JasperFx.Events.EventStreamUnexpectedMaxEventIdException`, both lifted to `GraphFault.StreamVersionConflict` carrying the head version — and the escalation is the `CommitExclusive` OP CASE, never prose: the advisory lock serializes hostile writers and its refusal is the typed `GraphFault.TxnConflict` (8302), so the folded transaction rail raises a registered sub-band row, never a loose 7001 integer; `SaveChangesAsync` is the only commit and the bracket never bypasses it; provider failure converts to `GraphFault` once at the bracket and the op-log changefeed (`Version/ledger#CHANGEFEED`) projects FROM the committed events, never a trigger-based second write path; a re-ingest of an existing model is aligned UPSTREAM by the `Version/merge#STRUCTURAL_DIFF` `Reconcile` (correlating the projector's freshly-minted rooted `NodeId`s onto the durable ids on `Node.Object.ExternalId`, the 1:1 IFC GlobalId) BEFORE the aligned `GraphDelta` reaches this `Commit`, so a re-import revises the existing stream rather than forking a duplicate model — this store appends the already-aligned delta, never re-deriving the alignment.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
// AppHost's composition root FILLS these Persistence-owned port-input shapes ([A.1]) from its own
// Principal, ClockPolicy, and the kernel causal frame at the PORT boundary, the ingredients crossing as
// delegate and wire VALUES. A port shape earns its declaration by RE-SHAPING the crossing — `StoreActor`
// narrows `Principal` under its own name, so the two never share one. The AppHost `RecoveryObjective` re-shapes
// into nothing, so `Rasm.AppHost/Runtime/profiles` declares it once and this package IMPORTS it; the root
// reads `ResolvedProfile.Recovery` and threads that value down, since a Persistence accessor over `.Recovery`
// is a forwarding wrapper. Every frame-referencing Persistence page re-threads onto these shapes in its own leg.
[ComplexValueObject]
public sealed partial class StoreActor {
    public string Subject { get; }
    public Seq<string> Roles { get; }
}

// `ProjectionContext` carries the kernel causal pair AS THE KERNEL TYPES, never their key scalars: `TenantContext`
// supplies `Partitions` (the structural absent-tenant arm — the root row is the single-tenant store and
// writes no partition text anywhere), `Entry` (the one fixed-width `x32` spelling the RLS predicate, the
// blame header, the object-name prefix, and the meter tag all compare), `Slug` (the census wire's tenancy
// text), and `TenantId.Value` (the raw `UInt128` a durable column or series key packs). A raw `Guid`/
// `UInt128` frame slot re-mints the lift at every seam and strands the absent-tenant arm on a zero sentinel.
//
// The CLOCK half is the kernel `Rasm/Parametric/projections#TIMELINE` `MonotonicTimeline`. The `Func<long> Mark`
// and `Func<long, Duration> Elapsed` pair it replaces IS the raw mark/elapsed pair that owner's boundary law
// names as the deleted form, and the delegates bought exactly the substitutability the timeline already offers
// through `MonotonicTimeline.Of(provider, key)`. What the pair could not do, the timeline does: it admits
// reference identity with the capturing provider before any elapsed read, so a stamp minted under one frame and
// measured under another refuses instead of yielding a plausible interval, and a backwards span refuses instead
// of landing a negative `Duration` on a receipt nothing downstream re-checks. The WALL clock stays a delegate —
// the timeline is purely monotonic and mints no `Instant`, so `Now` remains this frame's own slot and the
// composition root fills it from its `ClockPolicy` as before.
public sealed record ProjectionContext(MonotonicTimeline Timeline, Func<Instant> Now, CorrelationId Correlation, TenantContext Tenant) {
    // ONE span read, because "elapsed since this stamp, as the Duration a receipt carries" is the only timing
    // question this package asks. It captures the closing stamp, orders it against the opening one, and converts
    // at the single boundary where NodaTime's `Duration` meets the timeline's `TimeSpan` — three hops no leg
    // re-spells, and the `Fin` is the timeline's own refusal rail, not a new one.
    public Fin<Duration> Since(MonotonicStamp start, Op? key = null) =>
        Timeline.Capture(key).Bind(now => Timeline.Elapsed(start, now, key)).Map(Duration.FromTimeSpan);
}

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
    // `Link` is the federated-coordination write: durable cross-model edges land as rows in the same session, so a
    // clash pairing or provision-for-void reference commits with full blame headers under the project id.
    public sealed record Link(ModelId Model, ProjectId Project, Seq<ModelLink> Links, GraphWriteStamp Stamp) : GraphStoreOp;
    public sealed record Read(ModelId Model) : GraphStoreOp;
    public sealed record ReadAsOf(ModelId Model, TimeCut Cut) : GraphStoreOp;
    public sealed record State(ModelId Model) : GraphStoreOp;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct GraphReceipt(
    string Slot, ModelId Model, Option<long> Version, Option<int> Nodes, Option<int> Edges,
    Duration Elapsed, Instant At, CorrelationId Correlation);

// `NameLineage` is the durable REFERENCE-axis row: kernel `Rasm/Spatial/naming` `NameTable.Track(prior, rebuilt)`
// needs a PRIOR generation across sessions, so each rename-bearing commit persists the prior->rebuilt
// `TopoName` pairing as STRING pairs co-committed with the delta — a durable projection, never the kernel
// interior types crossing a wire. Distinct from the merge-consumed per-node `NamingHash` CONTENT receipt.
// `Id` is the Marten document identity (v7, insert-local); the prior-generation read keys `(Model, max
// Version)` through the `Configure` composite `(Model, Version)` computed index, index-served end to end.
public sealed record NameLineage(ModelId Model, long Version, HashMap<string, string> Track) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

// `LinkKind` and `ModelLink` carry the federated-coordination edge vocabulary: IFC declares no cross-file relationship, so the
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

// `ProjectRollup` REALIZES the project altitude (async daemon view — project dashboards, model rosters, whole-project
// watermarks), sliced by the `project` blame header `Blame` stamps so membership is a WRITE-time fact, never
// a fold-time join. It folds rosters and event watermarks ONLY — the per-model graph stays the inline
// aggregate's, so no second delta materializer exists.
public sealed record ProjectGraph(Guid Id, ImmutableHashSet<Guid> Models, long Events);

public sealed class ProjectRollup : MultiStreamProjection<ProjectGraph, Guid> {
    public ProjectRollup() => CustomGrouping(new ProjectHeaderGrouper());
    public static ProjectGraph Create(IEvent<GraphEvent> e) => new(HeaderProject(e), [e.StreamId], 1L);
    public ProjectGraph Apply(IEvent<GraphEvent> e, ProjectGraph view) => view with { Models = view.Models.Add(e.StreamId), Events = view.Events + 1L };
    static Guid HeaderProject(IEvent e) => ProjectHeaderGrouper.ProjectOf(e)
        .IfNone(() => throw new InvalidDataException("<project-rollup-header-missing>"));
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
    // `Slots` censuses every receipt kind this rail emits, registry-mounted (`Store/observability#SLOT_REGISTRY`).
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.element.open"), StoreSlot.Create("store.element.commit"), StoreSlot.Create("store.element.commit-exclusive"),
        StoreSlot.Create("store.element.retire"), StoreSlot.Create("store.element.link"), StoreSlot.Create("store.element.read"),
        StoreSlot.Create("store.element.identity"), StoreSlot.Create("store.element.project"), StoreSlot.Create("store.element.fault"));

    // `Run` is the one rail: the generated total `GraphStoreOp.Switch` (compile-time exhaustive over the
    // closed family, NO runtime-silent `_` default arm) dispatches each op to its bracket leg, so a new op
    // breaks the build here. Open/Commit/Retire share the co-transactional `Stage` write fold — blame headers
    // stamped, stream action staged, `IdentityStore.Stamp`, lineage rows, `SaveChangesAsync`; CommitExclusive
    // escalates to the advisory lock; Read/ReadAsOf/State are the read legs. `actor` spells the
    // Persistence-owned `StoreActor` and `storeId` the store's own origin Guid (the LWW tie-break origin),
    // matching the SAME `actor`/`origin` header slots the read side reads.
    // The opening capture is itself on the rail — a timeline that cannot mint a stamp cannot measure the op, so
    // the leg never runs rather than running unmeasured and reporting a fabricated span.
    public static IO<Fin<GraphReceipt>> Run(IDocumentSession session, GraphStoreOp op, ProjectionContext frame, CancellationToken cancellationToken) =>
        from opened in IO.lift(() => frame.Timeline.Capture())
        from outcome in opened.Match(
            Succ: mark => op.Switch(
                open: o => Stage(session, o.Identity, o.Stamp, o.Model, o.Opening, 0L, None, _ => ElementSchema.Open(session, o.Model, o.Header, o.Opening), frame, mark, "store.element.open", cancellationToken),
                commit: c => Stage(session, c.Identity, c.Stamp, c.Model, c.Delta, c.Expected, c.Lineage, _ => ElementSchema.Append(session, c.Model, new GraphEvent.GraphRevised(c.Delta), c.Expected), frame, mark, "store.element.commit", cancellationToken),
                commitExclusive: x => StageExclusive(session, x.Identity, x.Stamp, x.Model, x.Delta, x.Lineage, frame, mark, cancellationToken),
                retire: t => Stage(session, t.Identity, t.Stamp, t.Model, t.Delta, t.Expected, None, _ => ElementSchema.Append(session, t.Model, new GraphEvent.GraphRetired(t.Delta, t.Reason), t.Expected), frame, mark, "store.element.retire", cancellationToken),
                link: l => StageLinks(session, l, frame, mark, cancellationToken),
                read: r => ReadGraph(session, r.Model, frame, mark, cancellationToken),
                readAsOf: a => ReadGraphAsOf(session, a.Model, a.Cut, frame, mark, cancellationToken),
                state: s => ReadState(session, s.Model, frame, mark, cancellationToken)),
            Fail: error => IO.pure(Fin<GraphReceipt>.Fail(error)))
        select outcome;

    public static IO<Option<ElementGraph>> ReadAsOf(IQuerySession session, ModelId model, TimeCut cut, CancellationToken cancellationToken) =>
        ProjectAsOf(session, model, cut, cancellationToken).Map(o => o.Map(static p => p.Graph));

    static IO<Option<GraphProjection>> ProjectAsOf(IQuerySession session, ModelId model, TimeCut cut, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await Op.Of().Catch(async token => Fin<Option<GraphProjection>>.Succ(Optional(await session.Events.AggregateStreamAsync<GraphProjection>(
                model.Value,
                version: cut.StreamVersion.Match<long?>(Some: static version => version, None: static () => null),
                timestamp: cut.StreamVersion.IsSome ? (DateTimeOffset?)null : cut.At.ToDateTimeOffset(),
                token: token).ConfigureAwait(false))), cancellationToken).ConfigureAwait(false)).Bind(IO.liftFin);

    // `Stage` is the co-transactional write fold: it STAMPS the blame headers (`actor` = `StoreActor.Subject`,
    // `origin` = the store's own `storeId` Guid, `tenant` = the frame's RLS partition) onto the session so every
    // event this transaction appends carries them (`Configure` sets `MetadataConfig.HeadersEnabled`), stages the
    // stream action (open/append), stamps the identity row and the lineage rows in the SAME session, then lets
    // ONE `SaveChangesAsync` commit the event-with-headers, the identity document, the lineage rows, and the
    // inline projection atomically. `Lift` converts a provider failure to `GraphFault` at this one boundary.
    static IO<Fin<GraphReceipt>> Stage(IDocumentSession session, ElementIdentity identity, GraphWriteStamp stamp, ModelId model, GraphDelta delta, long expected, Option<NameLineage> lineage, Func<Unit, StreamAction> stage, ProjectionContext frame, MonotonicStamp mark, string slot, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await Op.Of().Catch(async token => {
            Blame(session, stamp, frame);
            StreamAction action = stage(unit);
            IdentityStore.Stamp(session, identity, stamp.Identity);
            lineage.IfSome(rows => session.Store(rows with { Version = action.Version }));
            await session.SaveChangesAsync(token).ConfigureAwait(false);
            return frame.Since(mark).Map(elapsed =>
                new GraphReceipt(slot, model, Some(action.Version), Some(delta.NodeCount), Some(delta.EdgeCount),
                    elapsed, frame.Now(), frame.Correlation));
        }, cancellationToken).ConfigureAwait(false)).Bind(captured => captured.Match(
            Succ: IO.pure,
            Fail: error => Lift(session, model, Some(expected), error, cancellationToken)));

    // `StageLinks` writes coordination edges: link rows land as Marten documents in the same blame-stamped session, so a
    // cross-model reference commits atomically with full actor/origin/tenant/project headers; the receipt's
    // edge count carries the landed link count.
    static IO<Fin<GraphReceipt>> StageLinks(IDocumentSession session, GraphStoreOp.Link op, ProjectionContext frame, MonotonicStamp mark, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await Op.Of().Catch(async token => {
            Blame(session, op.Stamp with { Project = Some(op.Project) }, frame);
            op.Links.Iter(link => session.Store(link));
            await session.SaveChangesAsync(token).ConfigureAwait(false);
            return frame.Since(mark).Map(elapsed =>
                new GraphReceipt("store.element.link", op.Model, None, None, Some(op.Links.Count),
                    elapsed, frame.Now(), frame.Correlation));
        }, cancellationToken).ConfigureAwait(false)).Bind(captured => captured.Match(
            Succ: IO.pure,
            Fail: error => Lift(session, op.Model, None, error, cancellationToken)));

    // `StageExclusive` escalates against hostile writers: `FetchForExclusiveWriting` takes the stream-level advisory lock,
    // so hostile writers serialize instead of racing the optimistic guard. A lock or serialization refusal
    // and any concurrent mutation refusal are the folded-transaction `GraphFault.TxnConflict` (8302); this shape
    // carries no expected-version value and cannot mint a stream-version conflict.
    static IO<Fin<GraphReceipt>> StageExclusive(IDocumentSession session, ElementIdentity identity, GraphWriteStamp stamp, ModelId model, GraphDelta delta, Option<NameLineage> lineage, ProjectionContext frame, MonotonicStamp mark, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await Op.Of().Catch(async token => {
            Blame(session, stamp, frame);
            IEventStream<GraphProjection> stream = await session.Events.FetchForExclusiveWriting<GraphProjection>(model.Value, token).ConfigureAwait(false);
            stream.AppendOne(new GraphEvent.GraphRevised(delta));
            long next = (stream.Aggregate?.Version ?? 0L) + 1L;
            IdentityStore.Stamp(session, identity, stamp.Identity);
            lineage.IfSome(rows => session.Store(rows with { Version = next }));
            await session.SaveChangesAsync(token).ConfigureAwait(false);
            return frame.Since(mark).Map(elapsed =>
                new GraphReceipt("store.element.commit-exclusive", model, Some(next), Some(delta.NodeCount), Some(delta.EdgeCount),
                    elapsed, frame.Now(), frame.Correlation));
        }, cancellationToken).ConfigureAwait(false)).Bind(captured => captured.Match(
            Succ: IO.pure,
            Fail: error => Lift(session, model, None, error, cancellationToken)));

    static void Blame(IDocumentSession session, GraphWriteStamp stamp, ProjectionContext frame) {
        session.SetHeader("actor", stamp.Actor.Subject);
        session.SetHeader("origin", stamp.Origin.ToString());
        // Kernel root partitions nothing, so an ABSENT tenant header IS the single-tenant fact the
        // `TryGetValue` read side folds to empty — a zero-valued partition string is the deleted sentinel.
        // `TenantContext.Key` is that absence read, carrying the one fixed-width `x32` `Entry` spelling the RLS
        // predicate and the meter tag both compare.
        frame.Tenant.Key.IfSome(entry => session.SetHeader("tenant", entry));
        // `project` carries ProjectRollup's grouping fact — stamped at write time, never joined at fold time.
        stamp.Project.IfSome(p => session.SetHeader("project", p.Value.ToString()));
    }

    // Three read legs differ ONLY in fetch shape and the (version, nodes, edges) triple each extracts; `Received`
    // owns both the Some -> store.element.read receipt and the None -> ModelAbsent absence arm as ONE shared
    // projection, so receipt construction and the absence rail are spelled once, never per read modality.
    static IO<Fin<GraphReceipt>> ReadGraph(IDocumentSession session, ModelId model, ProjectionContext frame, MonotonicStamp mark, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await Op.Of().Catch(async token => Fin<Option<GraphProjection>>.Succ(Optional(
            await session.Events.FetchLatest<GraphProjection>(model.Value, token).ConfigureAwait(false))), cancellationToken).ConfigureAwait(false))
            .Bind(IO.liftFin)
            .Map(p => Received(model, p,
                static g => (Some(g.Version), Some(g.Graph.Nodes.Count), Some(g.Graph.Edges.Length)), frame, mark));

    static IO<Fin<GraphReceipt>> ReadGraphAsOf(IDocumentSession session, ModelId model, TimeCut cut, ProjectionContext frame, MonotonicStamp mark, CancellationToken cancellationToken) =>
        ProjectAsOf(session, model, cut, cancellationToken)
            .Map(p => Received(model, p,
                static g => (Some(g.Version), Some(g.Graph.Nodes.Count), Some(g.Graph.Edges.Length)), frame, mark));

    static IO<Fin<GraphReceipt>> ReadState(IDocumentSession session, ModelId model, ProjectionContext frame, MonotonicStamp mark, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await Op.Of().Catch(async token => Fin<Option<StreamState>>.Succ(Optional(
            await session.Events.FetchStreamStateAsync(model.Value, token).ConfigureAwait(false))), cancellationToken).ConfigureAwait(false))
            .Bind(IO.liftFin)
            .Map(s => Received(model, s, static state => (Some(state.Version), Option<int>.None, Option<int>.None), frame, mark));

    // Absence sequences BEFORE the span read, so a missing model reports `ModelAbsent` rather than a timing
    // refusal that happened to fire first on a read that had nothing to time.
    static Fin<GraphReceipt> Received<T>(ModelId model, Option<T> found,
        Func<T, (Option<long> Version, Option<int> Nodes, Option<int> Edges)> read,
        ProjectionContext frame, MonotonicStamp mark) =>
        found.ToFin(new GraphFault.ModelAbsent(model))
            .Bind(value => frame.Since(mark).Map(elapsed => read(value) switch {
                (Option<long> version, Option<int> nodes, Option<int> edges) =>
                    new GraphReceipt("store.element.read", model, version, nodes, edges, elapsed, frame.Now(), frame.Correlation),
            }));

    // Provider-fault conversion at the one bracket boundary: an optimistic-version collision surfaces as
    // `Marten.Exceptions.ConcurrentUpdateException` (the wrapping write-collision) or its inner
    // `JasperFx.Events.EventStreamUnexpectedMaxEventIdException` (the expected-version mismatch) — both
    // lifted to `GraphFault.StreamVersionConflict` carrying the real head version read back through
    // `FetchStreamStateAsync`; a documented transient PostgreSQL refusal becomes `GraphFault.TxnConflict`, and every
    // other captured error remains exact.
    static IO<Fin<GraphReceipt>> Lift(IDocumentSession session, ModelId model, Option<long> expected, Error error, CancellationToken cancellationToken) =>
        error.Exception.Match(
            Some: ex => ex is Marten.Exceptions.ConcurrentUpdateException or JasperFx.Events.EventStreamUnexpectedMaxEventIdException
                ? expected.Match(
                    Some: guard => IO.liftAsync(async () => await Op.Of().Catch(async token => Fin<Option<StreamState>>.Succ(Optional(
                        await session.Events.FetchStreamStateAsync(model.Value, token).ConfigureAwait(false)), cancellationToken).ConfigureAwait(false))
                        .Bind(IO.liftFin)
                        .Map(state => state.Match(
                            Some: s => Fin<GraphReceipt>.Fail(new GraphFault.StreamVersionConflict(model, guard, s.Version, error)),
                            None: () => Fin<GraphReceipt>.Fail(error))),
                    None: () => IO.pure(Fin<GraphReceipt>.Fail(new GraphFault.TxnConflict(model, error))))
                : ex is PostgresException { IsTransient: true }
                    ? IO.pure(Fin<GraphReceipt>.Fail(new GraphFault.TxnConflict(model, error)))
                    : IO.pure(Fin<GraphReceipt>.Fail(error)),
            None: () => IO.pure(Fin<GraphReceipt>.Fail(error)));
}
```

| [INDEX] | [POLICY]               | [VALUE]                                    | [BINDING]                                                       |
| :-----: | :--------------------- | :----------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | one txn owner          | identity + event in one `IDocumentSession` | `IdentityStore.Stamp` then `SaveChangesAsync`                   |
|  [02]   | read consistency       | `FetchLatest<GraphProjection>`             | inline document or live tail fold; read-your-writes             |
|  [03]   | AS-OF fold             | `AggregateStreamAsync(version\|timestamp)` | version XOR instant; reuses `GraphDelta.ReplayOnto`             |
|  [04]   | optimistic concurrency | `Append(model, delta, expectedVersion)`    | racing writer aborts → `StreamVersionConflict`                  |
|  [05]   | exclusive escalation   | `CommitExclusive` op case                  | `FetchForExclusiveWriting`; refusal → `TxnConflict` 8302        |
|  [06]   | frame injection        | `StoreActor` + `ProjectionContext`         | AppHost fills slots at the port; no app-platform type crosses   |
|  [07]   | naming lineage         | `NameLineage` co-committed rows            | kernel `Track(prior, rebuilt)` reads a durable prior generation |
|  [08]   | coordination edges     | `ModelLink` rows + `Link` op case          | cross-model references durable, project-scoped, blame-stamped   |
|  [09]   | project rollup         | `ProjectRollup` header-sliced, async       | roster + watermark only; never a second delta materializer      |
|  [10]   | causal pair            | `CorrelationId` + `TenantContext` on frame | kernel types seat on the frame; raw scalars only at pack sites  |
|  [11]   | tenant text            | `TenantContext.Entry` fixed-width `x32`    | RLS predicate, blame header, and meter tag compare alike        |
|  [12]   | elapsed read           | kernel `MonotonicTimeline` via `Since`     | provider identity admitted; a mark/elapsed delegate pair is out |

## [05]-[FAULT_TABLES]

- Owner: the kernel `FaultBand` allocates the `GraphFault` direct union; generated identity derives its compact case codes from `FaultBand.Graph`.
- Entry: every Persistence fault family rides the kernel `Fault` floor — ONE roster realizing `[FaultCase]` declares the `Band` row and every case's `(Key, Offset)` pair, `Code` derive SEALED off the base, and `generated identity admission` proves offset uniqueness and span membership at first construction. Disjointness is the kernel's static `Disjoint` proof, folded at type initialization over the WHOLE roster and partitioned by `BandKind`, so an event band and a fault band may share a base while two bands of one kind may not.
- Growth: a new Persistence band is ONE row on the KERNEL roster beside the owning page's union derivation — the kernel's own accepted named loss, and the price of one proof over one code space; a new case inside a band is one union case and one roster row whose offset stays inside the row's `Span`; an outgrown union widens its row's `Span` into the neighbourhood's free tail at the kernel; zero new surface — a folder-local registry, a page-local band constant, a second roster beside a family's own, or a prose decade table is the deleted form.
- Boundary: `GraphFault.TxnConflict` carries the exact transient PostgreSQL cause; unknown provider errors remain exact.

```csharp signature
// --- [ERRORS] ---------------------------------------------------------------------------
// `GraphFault` derives directly from the kernel `Rasm.Domain.Fault` floor. Generated identity supplies `Code`,
// so the typed case lifts bare onto `Fin<T>`/`Validation<Error,T>` with
// no `.ToError()` hop and a recovery reads `error.IsType<GraphFault.StreamVersionConflict>()` /
// `error.HasCode(8300)` or the typed leaf, never a message substring. `TxnConflict` (8302) is the
// folded-transaction sub-band row — the advisory-lock/serialization refusal of the `CommitExclusive`
// escalation, never a loose 7001. No `[GenerateUnionOps]` — the kernel union-ops generator is strictly opt-in.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Graph;
    private GraphFault() { }

    [FaultCase(0)]
    public sealed partial record StreamVersionConflict(ModelId Model, long Expected, long Actual, Error Cause) : GraphFault(), ICausedFault;
    [FaultCase(1)]
    public sealed partial record ModelAbsent(ModelId Model) : GraphFault();
    [FaultCase(2)]
    public sealed partial record TxnConflict(ModelId Model, Error Cause) : GraphFault(), ICausedFault;

    public override string Message => Switch(
        streamVersionConflict: static c => $"<graph-version-conflict:{c.Model.Value}:{c.Expected}!={c.Actual}>:{c.Cause.Message}",
        modelAbsent:           static c => $"<graph-model-absent:{c.Model.Value}>",
        txnConflict:           static c => $"<graph-txn-conflict:{c.Model.Value}:{c.Cause.Message}>");
}
```

| [INDEX] | [POLICY]            | [VALUE]                               | [BINDING]                                                |
| :-----: | :------------------ | :------------------------------------ | :------------------------------------------------------- |
|  [01]   | band registry       | kernel `Rasm/Domain/rails#FAULT_BAND` | this folder declares no rows; the roster allocates all   |
|  [02]   | band disjointness   | kernel static `Disjoint` proof        | one proof over the whole code space, partitioned by kind |
|  [03]   | code derivation     | `[FaultCase]` ordinals                | `Code` seal off `Fault`                                  |
|  [04]   | provenance          | roster `Owner : TelemetrySource`      | the allocating package; no per-row page-anchor string    |
|  [05]   | folded-txn conflict | `GraphFault.TxnConflict` 8302         | registered sub-band row, never a loose 7001              |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
