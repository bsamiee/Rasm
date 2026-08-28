# [PERSISTENCE_ELEMENT_GRAPH]

Rasm.Persistence persists each `Rasm.Element` graph as one Marten stream keyed by `ModelId`: `GraphCreated`, `GraphRevised`, and `GraphRetired` carry boundary-validated `GraphDelta` bodies, and the inline `GraphProjection` and the AS-OF reconstruction reuse the one `GraphDelta.ReplayOnto` fold.

`IdentityStore.Stamp` queues the provider-bound `IdentityWriter` on the same `IDocumentSession`, so identity, event, lineage, and inline projection commit once without a second writer; `GraphWriteStamp` carries actor, origin, optional project, and the boot-composed identity writer on write cases alone; `ProjectionContext` seats the kernel `Rasm/Parametric/projections#TIMELINE` `MonotonicTimeline` beside the kernel `CorrelationId`/`TenantContext` pair as VALUES, so every leg reads one tenancy, one correlation, and one monotonic frame with no per-boundary lift.

Fault codes read the kernel `Rasm/Domain/results#FAULT_BAND` roster, which allocates every Persistence band. This folder holds no registry of its own: one registry cannot collide with itself, so the kernel's static `Disjoint` proof partitions the WHOLE code space by `BandKind` at type initialization, where a per-folder registry with pinned mirror rows agreed with its neighbours only by inspection and re-stated fourteen foreign decades this folder never allocates from.

## [01]-[INDEX]

- [02]-[STREAM_GRAIN]: model-stream identity, the `GraphEvent` body family, optimistic append, and the schema-keyed event registration.
- [03]-[GRAPH_PROJECTION]: Inline `SingleStreamProjection` folding `GraphDelta` into the STJ-rehydratable `GraphProjection` over the shared `GraphDelta.ReplayOnto`, the materialized `ElementGraph` read boundary, and the read-your-writes consistency boundary.
- [04]-[STORE_HOOKS]: Persistence-owned frame shapes (`StoreActor`/`ProjectionContext`) beside the imported `RecoveryObjective`, the one `GraphStoreOp` operation family over the generated total `Switch`, the session bracket, the exclusive-lock escalation, AS-OF reconstruction, the durable naming-lineage rows, and the co-transactional identity commit.
- [05]-[FAULT_TABLES]: the folder's routing pointer at the kernel `FaultBand` roster, and the `GraphFault` band this page owns on it.

## [02]-[STREAM_GRAIN]

- Owner: `ModelId` the `[ValueObject<Guid>]` per-model stream key under the `IObjectFactory` floor; `GraphEvent` the `[Union]` event-body family every model stream appends, carrying the `Body`/`Lifecycle` projections the `Version/ledger#CHANGEFEED` `OpLog.Project` reads off each Marten event; `EventLifecycle` the `[SmartEnum<string>]` create/revise/retire verb each event row carries; `ElementSchema` the static surface owning the `StoreOptions` event registration, the strong-typed value registration, the inline projection registration, and the per-model stream-start and append legs over the one `IDocumentSession`.
- Cases: `GraphCreated(Header Header, GraphDelta Delta)` opens a stream carrying the `Rasm.Element` `Header` (`ReleaseVersion`/`ModelView`/`GeoReference`/`Tolerance`/`Instant`/`StepHeader`) AND the assembled opening `GraphDelta` (the `Projection/projection#PROJECTION_CONTRACT` `Assemble` merged model-creating delta), so a model is created in ONE event rather than an empty open beside a separate content commit; `GraphRevised(GraphDelta Delta)` is the steady-state append; `GraphRetired(GraphDelta Delta, string Reason)` carries the retirement delta whose `GraphDelta` removes the retired nodes/edges — so retirement is a real convergent delta the projection folds, never an out-of-band tombstone; the event body is ALWAYS the shared `GraphDelta`, NEVER a whole-graph snapshot, because the delta replays deterministically through `GraphDelta.ReplayOnto` and a whole-graph body bloats every append by the model size.
- Entry: `public static StoreOptions Configure(StoreOptions opts, NpgsqlDataSource source)` registers the event types, the strong-typed `ModelId`/`NodeId` value types, the inline `GraphProjection` self-aggregating snapshot, and the metadata columns once at boot, the composition root folding each higher-stratum `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` contribution over the returned options; `public static StreamAction Open(IDocumentSession session, ModelId model, Header header, GraphDelta opening)` calls `session.Events.StartStream<GraphProjection>(model.Value, new GraphEvent.GraphCreated(header, opening))` so the assembled opening delta is the one model-creating event; `public static StreamAction Append(IDocumentSession session, ModelId model, GraphEvent body, long expectedVersion)` appends with the inline optimistic version guard so a concurrent writer racing the same stream version aborts at `SaveChangesAsync` rather than silently interleaving.
- Auto: `StreamIdentity.AsGuid` keys one stream per `ModelId`. `EventAppendMode.Rich` remains invariant for authoring and re-ingest because `actor`/`origin`/`tenant` headers are durable blame inputs; Marten's `Quick` modes trade away metadata richness and therefore do not admit this stream. `GraphDelta` is the boundary-validated event body. `RegisterValueType<ModelId>()`/`RegisterValueType<NodeId>()` preserve typed keys, and `UseSystemTextJsonForSerialization(ElementJson.Options, …)` binds the one generated serializer profile.
- Packages: Marten (`StartStream`/`Append`/`StreamAction`/`EventAppendMode`/`StreamIdentity`/`RegisterValueType`/`Snapshot`/`UseSystemTextJsonForSerialization`/`MetadataConfig`), Npgsql, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new durable change kind is one `GraphEvent` case, one `EventLifecycle` row, and one projection `Apply` method the convention discovery forces; a richer model header is one field on the shared `Header`; a per-spatial-partition grain is one `ModelId` minting policy, never a second stream shape; zero new surface — a per-`NodeId` stream, a whole-graph event body, a second event table, or a bespoke `OpLogEntry` store beneath Marten is the deleted form because Marten owns the durable append and the rebuildable read views, and the `Version/` engine projects FROM these events (`Version/ledger#CHANGEFEED` `OpLog.Project` over `e.Data.Body`/`e.Data.Lifecycle`).
- Boundary: stream grain is ONE stream PER MODEL (or per spatial partition), never per-`NodeId`, and the event body is the `GraphDelta`, never a whole-graph snapshot; the `GraphDelta` is the shared graph-mutation record the projection folds immutably through `GraphDelta.ReplayOnto`, so the durable history is a delta log the engine replays and the rehydrated graph is bit-identical to the live state at any version because the fold is the ONE `GraphDelta.ReplayOnto` the AS-OF reconstruction also runs; the optimistic append (`Append(stream, expectedVersion, …)`) is the inline guard, `AppendOptimistic` the read-then-guard, and the `GraphStoreOp.CommitExclusive` case the stream-level advisory-lock escalation (`FetchForExclusiveWriting<GraphProjection>`, `#STORE_HOOKS`); the `GraphRetired` delta is a real convergent retirement the projection folds and the `Version/retention#RETENTION_CLASSES` sweep reclaims, never an `ArchiveStream` that hides the events from the fold (archive is the AS-OF cut boundary, not retirement); a `GraphCreated` carries the `Header` so the stream's `ReleaseVersion`/`GeoReference`/`Tolerance` are the first folded fact and every later delta's measure quantization (`Element/codec#CONTENT_ADDRESS`) reads the header tolerance; `EventAppendMode` trades metadata richness for throughput as a config value, never a per-call branch; the `GraphEvent` is the body family `Version/ledger#CHANGEFEED` lifts (`OpLog.Project(IEvent<GraphEvent>)` reads `e.Data.Body`/`e.Data.Lifecycle`), so this owner's body shape is the changefeed's input contract; `Configure` is the spine's `StoreOptions` seat and registers ONLY spine-owned mappings — a rolling-window declaration over a `Query`/`Version` document type makes this S0 surface name an S2/S3 type, the forbidden upward edge, so each such family publishes its own `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` contribution at its own owner and the composition root folds it over these options, one `StoreOptions` value threaded through both.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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
using Rasm.Parametric;
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
        opts.Connection(source);
        opts.Events.StreamIdentity = StreamIdentity.AsGuid;
        opts.Events.AppendMode = EventAppendMode.Rich;
        opts.Events.MetadataConfig.CausationIdEnabled = true;
        opts.Events.MetadataConfig.CorrelationIdEnabled = true;
        opts.Events.MetadataConfig.HeadersEnabled = true;
        opts.UseSystemTextJsonForSerialization(ElementJson.Options, EnumStorage.AsString, Casing.CamelCase);
        opts.RegisterValueType<ModelId>();
        opts.RegisterValueType<NodeId>();
        opts.Schema.For<NameLineage>().Index([static l => l.Model, static l => l.Version]);
        opts.Projections.Snapshot<GraphProjection>(SnapshotLifecycle.Inline);
        opts.Projections.Add(new ProjectRollup(), ProjectionLifecycle.Async);
        opts.Schema.For<ModelLink>().Index(static l => l.Project);
        opts.Schema.For<ModelLink>().Index(static l => new { l.FromModel, l.FromNode });
        return opts;
    }

    public static StreamAction Open(IDocumentSession session, ModelId model, Header header, GraphDelta opening) {
        return session.Events.StartStream<GraphProjection>(model.Value, new GraphEvent.GraphCreated(header, opening));
    }

    public static StreamAction Append(IDocumentSession session, ModelId model, GraphEvent body, long expectedVersion) {
        return session.Events.Append(model.Value, expectedVersion, body);
    }
}
```

| [INDEX] | [POLICY]          | [VALUE]                                       | [BINDING]                                                       |
| :-----: | :---------------- | :-------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | stream grain      | one stream per `ModelId`                      | `StreamIdentity.AsGuid`; never per-`NodeId`                     |
|  [02]   | event body        | the shared `GraphDelta`                       | never a whole-graph snapshot; folds via `GraphDelta.ReplayOnto` |
|  [03]   | append mode       | `EventAppendMode.Rich`                        | blame headers remain durable on every write                     |
|  [04]   | optimistic guard  | `Append(stream, expectedVersion, …)`          | concurrent same-version writer aborts at `SaveChangesAsync`     |
|  [05]   | strong-typed keys | `RegisterValueType<ModelId/NodeId>`           | typed stream key + document id, never a bare Guid/string        |
|  [06]   | partition seat    | family-published `RollingWindow` contribution | spine registers spine-owned mappings only; no upward edge       |

## [03]-[GRAPH_PROJECTION]

- Owner: `GraphProjection` the inline self-aggregating snapshot AGGREGATE Marten folds one model stream into (registered through `opts.Projections.Snapshot<GraphProjection>(SnapshotLifecycle.Inline)` — the document carries the `Create`/`Apply` convention methods Marten discovers and wraps as a single-stream projection internally, so the record is the aggregate, never a hand-derived `SingleStreamProjection<,>` subclass) — the STJ-rehydratable carrier of one model's `Header`, node map, edge array, and folded version, written in the append transaction, materializing the shared `ElementGraph` ONCE through `ElementGraph.Of` at the read boundary; the aggregate's `Create`/`Apply` convention methods owning the one `GraphDelta.ReplayOnto` fold over the shared graph; faults land in the `#FAULT_TABLES` `GraphFault` band.
- Cases: `Create(GraphCreated)` seeds the genesis through `ElementGraph.Genesis(header)` and replays the opening delta; `Apply` replays each recorded revision or retirement through `GraphDelta.ReplayOnto`.
- Entry: `Create(IEvent<GraphEvent.GraphCreated>)` seeds `Model` from `StreamId` and `Version` from the stored message envelope; body-only `Apply(GraphRevised)` and `Apply(GraphRetired)` advance the aggregate version once per event. `Graph` memoizes the frozen `ElementGraph` materialization.
- Auto: the projection registers `SnapshotLifecycle.Inline` so the folded `GraphProjection` document is written in the SAME transaction as the appended events — a `Read` after a `Commit` in the same logical unit sees the new state with no daemon lag — and the inline aggregate IS the periodic materialized view, so a deep stream loads the head document rather than re-folding from genesis; the projection stores the STJ-serializable primitives (`Header`, `ImmutableDictionary<NodeId, Node>`, `ImmutableArray<Relationship>`) because the shared `ElementGraph` is a sealed read-snapshot class with no deserialization path, and the live authoring graph uses the shared `ImmutableDictionary`/HAMT structural-sharing form (`Graph/delta#GRAPH_DELTA`) while `ElementGraph.Of` freezes to `FrozenDictionary` + the incidence index + the lazy `QuikGraph` view only at the `Graph` materialization boundary, so the delta path stays O(log n) structural-sharing and the read snapshot stays O(1) lookup; `From` is the ONLY mint (each `Create`/`Apply` rebuilds the document and the lazy `Graph` memo from the folded snapshot) so a `with` can never alias a stale materialized graph.
- Packages: Marten (`SingleStreamProjection`/`SnapshotLifecycle`/`IEvent<T>`), Rasm.Element (`ElementGraph`/`ElementGraph.Genesis`/`ElementGraph.Of`/`GraphDelta`/`GraphDelta.ReplayOnto`/`Header`/`Node`/`NodeId`/`Relationship`), LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Collections.Immutable/Frozen, BCL inbox.
- Growth: a new event arm is one projection `Apply` method the convention discovery forces; the cross-model rollup is the realized `#STORE_HOOKS` `ProjectRollup` (header-sliced, roster + watermark only), so a richer project view is one field on `ProjectGraph`, never a second fold of the same delta; a co-transactional columnar egress is the `Query/lakehouse#FLAT_TABLE_EGRESS` `FlatTableProjection`; zero new surface — a hand-rolled stream folder, a second materializer, or a per-read whole-stream replay is the deleted form because the inline projection IS the materialized read and the AS-OF fold reuses the same `GraphDelta.ReplayOnto`.
- Boundary: the inline projection is the READ-YOUR-WRITES consistency boundary — authoritative containment, topology, and void-resolution reads go through this folded `GraphProjection.Graph`, NEVER an async lane, because an async daemon view lags the write (`Query/lane#READ_ROUTING` routes interactive correctness here by construction); the analytical lanes (`Query/columnar`, `Query/cypher`) are explicitly `ProjectionLifecycle.Async` with a staleness watermark and interactive-correctness reads block on `WaitForNonStaleProjectionDataAsync`; the projection apply is the SAME `GraphDelta.ReplayOnto` fold the `Version/timetravel#TIME_TRAVEL` AS-OF reconstruction runs (the live authoring path produces the deltas it replays, via the shared `Graph/delta#GRAPH_DELTA` `WorkingGraph.Apply`), so there is exactly ONE delta→graph materializer and a historical fold equals the live state field-for-field; the projection NEVER stores the shared `ElementGraph` directly (it has no public deserialization constructor — a sealed read-snapshot class whose only mint is `Of`/`Genesis`/`Apply`), so the document carries the rehydratable `Header`/node-map/edge-array and `Graph` materializes the frozen snapshot once through `ElementGraph.Of`; the inline aggregate is the materialized read floor bounding replay, never a second source of truth — `store.Advanced.RebuildSingleStreamAsync<GraphProjection>(model)` replays one stream's inline projection from zero when the fold logic changes; the projection never re-validates the delta because the projector `IGraphConstraint` and the shared `LegalLink` already gated it at the write boundary — re-validation in the projection is the deleted form because a validated delta in the stream is total by construction and a fold-time fault is a deployment defect surfaced as `GraphFault`, not a recoverable data path.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
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
|  [03]   | one materializer      | shared `GraphDelta.ReplayOnto`          | projection and AS-OF reconstruction fold the one delta |
|  [04]   | serializable document | `Header`/node-map/edge-array primitives | `ElementGraph` has no STJ ctor; `Of` materializes once |
|  [05]   | cross-model rollup    | `MultiStreamProjection`                 | sliced by project id, never a second delta fold        |

## [04]-[STORE_HOOKS]

- Owner: `StoreActor` the Persistence-owned `[ComplexValueObject]` actor value (subject + role claims) AppHost's composition root MAPS its richer `Principal` onto at the port boundary — the AppHost simple name never crosses down, mirroring the `Grant`/`Capability` never-share-a-name law; `ProjectionContext` the Persistence-owned injected frame seating the kernel `MonotonicTimeline`, the kernel `Hlc` cell, the kernel `CorrelationId`/`TenantContext` pair, and the composition's mounted `InstrumentSet` as VALUES, with `Since` the one elapsed read every leg takes (AppHost fills the slots from its own `ClockPolicy`, the kernel causal frame, its `Hlc` cell, and the contributor port's mounted set at the boundary; every Persistence page threads this frame, never an app-platform type, and a leg writing an instrument, stamping a dot, or binding an RLS predicate reads the kernel pair, the `Hlc` cell, and the mounted `InstrumentSet` straight off the frame — the per-boundary lift from a raw `Guid`/`UInt128` slot is the deleted form, and the raw key scalars survive only where a durable column, an object-name prefix, or a series key demands the packed value through `TenantId.ToValue()`); `RecoveryObjective` the `Rasm.AppHost/Runtime/profiles` declaration this owner IMPORTS whole and `Version/recovery` gauges against — a port shape earns its seat by RE-SHAPING the crossing, so an identical `(Rpo, Rto)` record here is a twin and the composition root threads the settled window in by value instead; `NameLineage` the durable REFERENCE-axis row persisting the kernel `Rasm/Spatial/naming` generational `Track(prior, rebuilt)` pairing across sessions; `ProjectId`/`LinkKind`/`ModelLink` the federated-coordination vocabulary — the durable cross-model reference edge IFC cannot carry, co-committed like `NameLineage`; `ProjectGraph`/`ProjectRollup` the realized project-altitude `MultiStreamProjection` sliced by the `project` blame header (async, roster + watermark only — never a second delta materializer); `GraphStoreOp` the `[Union]` operation family every durable graph interaction is a value in; `GraphStore` the static surface owning the one bracket over the generated total `Switch` — pooled session acquisition, the strong-typed append, the exclusive-lock escalation, the inline-projection read, the AS-OF fold, the co-transactional identity commit, and provider-fault conversion to `GraphFault`; `StreamHead` the typed per-op evidence carrying the model, the resulting version, and the elapsed `Duration`.
- Cases: write cases carry their required `ElementIdentity` and `GraphWriteStamp`; `Link` carries its project-scoped links and stamp; `Read`, `ReadAsOf`, and `State` carry only their read discriminants. `ModelLink` covers directed and symmetric cross-model relationships, validity interval, and extensible attributes; `LinkKind` carries directionality as row data.
- Entry: `Run(IDocumentSession, GraphStoreOp, ProjectionContext, CancellationToken)` dispatches the closed family, the token threading to every provider await so caller cancellation reaches the store instead of dying at the entry boundary. Write cases stamp their carrier and queue identity before `SaveChangesAsync`; read cases require no dummy identity, actor, origin, or project. `ReadAsOf` passes a nullable version only when the cut carries one and otherwise passes only the timestamp, preserving the version-XOR-time contract.
- Auto: the bracket runs the op through the generated total `GraphStoreOp.Switch` (compile-time exhaustive over the closed family — a new op breaks the build at the `Run` dispatch, never a runtime-silent `_` arm) and `SaveChangesAsync` commits events, the queued identity upsert, the lineage rows, and the inline projection in one Postgres transaction — there is no separate identity ORM and no two-phase dance because the identity write is the one model-derived statement `IdentityStore.Stamp` queues on the session; the read op calls `FetchLatest<GraphProjection>(model)` which returns the inline document when present or live-folds the tail, so a read after a commit in the same unit is consistent; the AS-OF op binds either a `version` or a `timestamp` (one or the other, never both) from the `TimeCut` so an historical read folds the SAME `GraphDelta.ReplayOnto` deterministically; a `Commit`/`CommitExclusive` carrying `Some(NameLineage)` stores the lineage rows in the same session so the kernel `NameTable.Track(prior, rebuilt)` reads a durable PRIOR generation on the next session — a durable projection of the kernel lineage as string pairs, never the kernel interior types crossing a wire (naming's interior-type law holds), the REFERENCE axis distinct from the merge-consumed per-node `NamingHash` content digest; provider exceptions convert to `GraphFault` at the one bracket boundary and the interior never sees a raw `Marten.Exceptions.MartenException`, while caller cancellation passes through untyped.
- Output: `StreamHead` — a write returns the settled `StreamAction.Version` with the delta's node and edge counts, a `Link` returns the landed edge count, `Read`/`ReadAsOf` return the folded version and counts, and `State` returns the head version alone.
- Packages: Marten (`IDocumentSession`/`IQuerySession`/`SetHeader`/`SaveChangesAsync`/`FetchLatest`/`FetchForExclusiveWriting`/`IEventStream<T>.AppendOne`/`AggregateStreamAsync`/`FetchStreamStateAsync`/`Store`), Rasm (`Rasm/Parametric/projections#TIMELINE` `MonotonicTimeline`/`MonotonicStamp` — the frame's clock half; `Rasm/Domain/results#FAULT_BAND` `FaultBand`), Rasm.AppHost (project — `RecoveryObjective`), LanguageExt.Core (`IO`/`Fin`/`Option`), NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new durable interaction is one `GraphStoreOp` case and one branch in the generated total `Switch` the closed family forces; a new read modality is one op case discriminating on its input (a key resolves to one graph, a cut to an AS-OF graph, a state probe to a head version) whose leg projects through the one `Received` head fold (the `(version, nodes, edges)` extractor the only per-leg difference); a new frame ingredient is one slot on `ProjectionContext`, never a new signature parameter; a new durability dimension is one column on the AppHost `RecoveryObjective`, never a record minted here; a same-session SQL side-write (a coordination cursor, an outbox advance) is `IDocumentOperations.QueueSqlCommand` inside the one transaction, never a second connection; zero new surface — a repository per model, a per-verb service, an injected `persist` delegate, a per-read-leg head-construction-and-absence-arm copy, an AppHost frame type on a signature, or a separate identity transaction is the deleted form because the one entry owns the bracket, the one session owns identity-with-event atomicity, the one `Received` fold owns the head-and-absence projection, and the op family discriminates by value shape through the generated `Switch`.
- Boundary: the `IDocumentSession` is the one transaction owner for identity with event — the `ElementIdentity` row lands as the ONE model-derived upsert the `Element/identity#ELEMENT_IDENTITY` `IdentityStore.Stamp` owner queues on the same session as the appended events, so a single `SaveChangesAsync` commits both with no free two-ORM atomicity and no EF-versus-Marten gap (the EF identity model of `Element/identity` names the one `element_identity` relation for DDL and every H3/PostGIS/pgvector/ACL read while holding zero write authority — the session is the only writer); the blame headers are the WRITE side of the read-side blame contract — `Stage` stamps `actor`/`origin`/`tenant` so every appended event carries the slots `MetadataConfig.HeadersEnabled` persists, `Version/ledger#CHANGEFEED` `OpLog.Project` reading `actor` off them and `Version/timetravel#TIME_TRAVEL` `AuthorshipOf` admitting both on the typed result; adjudication left those headers behind — `Version/ledger` breaks its `(Hlc, OriginStoreId)` LWW tie on `OperationId.Origin`, the store id `DotSource` mints into every dot, so ordering stays deterministic across peers whatever a header carries and the `Guid.Empty` bucket that once collapsed every origin into one is unreachable; omitting the stamp is still the deleted form because `AuthorshipOf` refuses the blame/scrub read instead of inventing an anonymous actor or zero origin; the frame ingredients cross as VALUES on the Persistence-owned shapes this section defines — a `ClockPolicy` or `Principal` parameter on any Persistence signature is the named leak, since both re-shape at the boundary, while `RecoveryObjective` crosses as ITSELF because nothing re-shapes it and this package sits a rank above the spine that declares it, and the kernel causal frame (`CorrelationId`, `TenantId`/`TenantContext`, the `Hlc` cell, the mounted `InstrumentSet`) is S0 vocabulary this package composes directly and therefore SEATS on the frame rather than being re-derived at each boundary, so every stamp is the frame's `Clock.Stamp`, every measurement a `frame.Instruments` write against a `Store/observability#STORE_INSTRUMENTS` row (a neutral-`Guid` correlation column or a folder-local `Meter` is the twin the kernel forecloses), every RLS predicate, blame header, and meter tag spells `TenantContext.Entry`, every census wire spells `Slug`, and `TenantId.ToValue()` is reached only where a durable column, an object-name prefix, an AAD digest, or a series key packs the raw scalar; every Persistence page re-threads onto `ProjectionContext`/`StoreActor` in its own rebuild and takes the durability window as a `RecoveryObjective` parameter; the read op is read-your-writes through the inline projection and NEVER routes to an async analytical lane; the AS-OF op binds `version` XOR `timestamp` so a precise cut pins a version and an instant cut binds the wall clock, and the fold reuses the one `GraphDelta.ReplayOnto` so an historical graph equals the live state; optimistic concurrency is `Commit(model, delta, expectedVersion, …)` whose inline `Append(stream, expectedVersion, body)` aborts a racing same-version writer at `SaveChangesAsync` — surfacing as `Marten.Exceptions.ConcurrentUpdateException` wrapping the inner `JasperFx.Events.EventStreamUnexpectedMaxEventIdException`, both lifted to `GraphFault.StreamVersionConflict` carrying the head version — and the escalation is the `CommitExclusive` OP CASE, never prose: the advisory lock serializes hostile writers and its refusal is the typed `GraphFault.TxnConflict` (8302), so the folded transaction path raises a registered sub-band row, never a loose 7001 integer; `SaveChangesAsync` is the only commit and the bracket never bypasses it; provider failure converts to `GraphFault` once at the bracket and the op-log changefeed (`Version/ledger#CHANGEFEED`) projects FROM the committed events, never a trigger-based second write path; a re-ingest of an existing model is aligned UPSTREAM by the `Version/merge#STRUCTURAL_DIFF` `Reconcile` (correlating the projector's freshly-minted rooted `NodeId`s onto the durable ids on `Node.Object.ExternalId`, the 1:1 IFC GlobalId) BEFORE the aligned `GraphDelta` reaches this `Commit`, so a re-import revises the existing stream rather than forking a duplicate model — this store appends the already-aligned delta, never re-deriving the alignment.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class StoreActor {
    public string Subject { get; }
    public Seq<string> Roles { get; }
}

public sealed record ProjectionContext(MonotonicTimeline Timeline, Func<Instant> Now, Hlc Clock, CorrelationId Correlation, TenantContext Tenant, InstrumentSet Instruments) {
    public Fin<Duration> Since(MonotonicStamp start) =>
        Timeline.Capture(key).Bind(now => Timeline.Elapsed(start, now)).Map(Duration.FromTimeSpan);
}

public sealed record GraphWriteStamp(StoreActor Actor, Guid Origin, Option<ProjectId> Project, IdentityWriter Identity);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphStoreOp {
    private GraphStoreOp() { }

    public sealed record Open(ModelId Model, Header Header, GraphDelta Opening, ElementIdentity Identity, GraphWriteStamp Stamp) : GraphStoreOp;
    public sealed record Commit(ModelId Model, GraphDelta Delta, long Expected, Option<NameLineage> Lineage, ElementIdentity Identity, GraphWriteStamp Stamp) : GraphStoreOp;
    public sealed record CommitExclusive(ModelId Model, GraphDelta Delta, Option<NameLineage> Lineage, ElementIdentity Identity, GraphWriteStamp Stamp) : GraphStoreOp;
    public sealed record Retire(ModelId Model, GraphDelta Delta, string Reason, long Expected, ElementIdentity Identity, GraphWriteStamp Stamp) : GraphStoreOp;
    public sealed record Link(ModelId Model, ProjectId Project, Seq<ModelLink> Links, GraphWriteStamp Stamp) : GraphStoreOp;
    public sealed record Read(ModelId Model) : GraphStoreOp;
    public sealed record ReadAsOf(ModelId Model, TimeCut Cut) : GraphStoreOp;
    public sealed record State(ModelId Model) : GraphStoreOp;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct StreamHead(ModelId Model, Option<long> Version, Option<int> Nodes, Option<int> Edges);

public sealed record NameLineage(ModelId Model, long Version, HashMap<string, string> Track) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

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

public sealed record ProjectGraph(Guid Id, ImmutableHashSet<Guid> Models, long Events);

public sealed class ProjectRollup : MultiStreamProjection<ProjectGraph, Guid> {
    public ProjectRollup() => CustomGrouping(new ProjectHeaderGrouper());

    public override ProjectGraph Evolve(ProjectGraph? snapshot, Guid id, IEvent e) =>
        snapshot is null
            ? new ProjectGraph(id, [e.StreamId], 1L)
            : snapshot with { Models = snapshot.Models.Add(e.StreamId), Events = snapshot.Events + 1L };
}

file sealed class ProjectHeaderGrouper : IAggregateGrouper<Guid> {
    static Option<Guid> ProjectOf(IEvent e) =>
        e.Headers is { } headers && headers.TryGetValue("project", out object? raw) && raw is string project && Guid.TryParse(project, out Guid id)
            ? Some(id) : None;

    public Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<Guid> grouping) {
        foreach (IEvent @event in events) {
            ProjectOf(@event).IfSome(id => grouping.AddEvent(id, @event));
        }
        return Task.CompletedTask;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GraphStore {
    public static IO<Fin<StreamHead>> Run(IDocumentSession session, GraphStoreOp op, ProjectionContext frame, CancellationToken cancellationToken) =>
        op.Switch(
            open: o => Stage(session, o.Identity, o.Stamp, o.Model, o.Opening, 0L, None, _ => ElementSchema.Open(session, o.Model, o.Header, o.Opening), frame, cancellationToken),
            commit: c => Stage(session, c.Identity, c.Stamp, c.Model, c.Delta, c.Expected, c.Lineage, _ => ElementSchema.Append(session, c.Model, new GraphEvent.GraphRevised(c.Delta), c.Expected), frame, cancellationToken),
            commitExclusive: x => StageExclusive(session, x.Identity, x.Stamp, x.Model, x.Delta, x.Lineage, frame, cancellationToken),
            retire: t => Stage(session, t.Identity, t.Stamp, t.Model, t.Delta, t.Expected, None, _ => ElementSchema.Append(session, t.Model, new GraphEvent.GraphRetired(t.Delta, t.Reason), t.Expected), frame, cancellationToken),
            link: l => StageLinks(session, l, frame, cancellationToken),
            read: r => ReadGraph(session, r.Model, frame, cancellationToken),
            readAsOf: a => ReadGraphAsOf(session, a.Model, a.Cut, frame, cancellationToken),
            state: s => ReadState(session, s.Model, frame, cancellationToken));

    public static IO<Option<ElementGraph>> ReadAsOf(IQuerySession session, ModelId model, TimeCut cut, CancellationToken cancellationToken) =>
        ProjectAsOf(session, model, cut, cancellationToken).Map(o => o.Map(static p => p.Graph));

    static IO<Option<GraphProjection>> ProjectAsOf(IQuerySession session, ModelId model, TimeCut cut, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await HostEdge.Captured(async token => Fin<Option<GraphProjection>>.Succ(Optional(await session.Events.AggregateStreamAsync<GraphProjection>(
                model.Value,
                version: cut.StreamVersion.Match<long?>(Some: static version => version, None: static () => null),
                timestamp: cut.StreamVersion.IsSome ? (DateTimeOffset?)null : cut.At.ToDateTimeOffset(),
                token: token).ConfigureAwait(false))), cancellationToken).ConfigureAwait(false)).Bind(IO.lift);

    static IO<Fin<StreamHead>> Stage(IDocumentSession session, ElementIdentity identity, GraphWriteStamp stamp, ModelId model, GraphDelta delta, long expected, Option<NameLineage> lineage, Func<Unit, StreamAction> stage, ProjectionContext frame, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await HostEdge.Captured(async token => {
            Blame(session, stamp, frame);
            StreamAction action = stage(unit);
            IdentityStore.Stamp(session, identity, stamp.Identity);
            lineage.IfSome(rows => session.Store(rows with { Version = action.Version }));
            await session.SaveChangesAsync(token).ConfigureAwait(false);
            return Fin.Succ(new StreamHead(model, Some(action.Version), Some(delta.NodeCount), Some(delta.EdgeCount)));
        }, cancellationToken).ConfigureAwait(false)).Bind(captured => captured.Match(
            Succ: IO.pure,
            Fail: error => Lift(session, model, Some(expected), error, cancellationToken)));

    static IO<Fin<StreamHead>> StageLinks(IDocumentSession session, GraphStoreOp.Link op, ProjectionContext frame, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await HostEdge.Captured(async token => {
            Blame(session, op.Stamp with { Project = Some(op.Project) }, frame);
            op.Links.Iter(link => session.Store(link));
            await session.SaveChangesAsync(token).ConfigureAwait(false);
            return Fin.Succ(new StreamHead(op.Model, None, None, Some(op.Links.Count)));
        }, cancellationToken).ConfigureAwait(false)).Bind(captured => captured.Match(
            Succ: IO.pure,
            Fail: error => Lift(session, op.Model, None, error, cancellationToken)));

    static IO<Fin<StreamHead>> StageExclusive(IDocumentSession session, ElementIdentity identity, GraphWriteStamp stamp, ModelId model, GraphDelta delta, Option<NameLineage> lineage, ProjectionContext frame, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await HostEdge.Captured(async token => {
            Blame(session, stamp, frame);
            IEventStream<GraphProjection> stream = await session.Events.FetchForExclusiveWriting<GraphProjection>(model.Value, token).ConfigureAwait(false);
            stream.AppendOne(new GraphEvent.GraphRevised(delta));
            long next = (stream.Aggregate?.Version ?? 0L) + 1L;
            IdentityStore.Stamp(session, identity, stamp.Identity);
            lineage.IfSome(rows => session.Store(rows with { Version = next }));
            await session.SaveChangesAsync(token).ConfigureAwait(false);
            return Fin.Succ(new StreamHead(model, Some(next), Some(delta.NodeCount), Some(delta.EdgeCount)));
        }, cancellationToken).ConfigureAwait(false)).Bind(captured => captured.Match(
            Succ: IO.pure,
            Fail: error => Lift(session, model, None, error, cancellationToken)));

    static void Blame(IDocumentSession session, GraphWriteStamp stamp, ProjectionContext frame) {
        session.SetHeader("actor", stamp.Actor.Subject);
        session.SetHeader("origin", stamp.Origin.ToString());
        frame.Tenant.Key.IfSome(entry => session.SetHeader("tenant", entry));
        stamp.Project.IfSome(p => session.SetHeader("project", p.Value.ToString()));
    }

    static IO<Fin<StreamHead>> ReadGraph(IDocumentSession session, ModelId model, ProjectionContext frame, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await HostEdge.Captured(async token => Fin<Option<GraphProjection>>.Succ(Optional(
            await session.Events.FetchLatest<GraphProjection>(model.Value, token).ConfigureAwait(false))), cancellationToken).ConfigureAwait(false))
            .Bind(IO.lift)
            .Map(p => Received(model, p,
                static g => (Some(g.Version), Some(g.Graph.Nodes.Count), Some(g.Graph.Edges.Length))));

    static IO<Fin<StreamHead>> ReadGraphAsOf(IDocumentSession session, ModelId model, TimeCut cut, ProjectionContext frame, CancellationToken cancellationToken) =>
        ProjectAsOf(session, model, cut, cancellationToken)
            .Map(p => Received(model, p,
                static g => (Some(g.Version), Some(g.Graph.Nodes.Count), Some(g.Graph.Edges.Length))));

    static IO<Fin<StreamHead>> ReadState(IDocumentSession session, ModelId model, ProjectionContext frame, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await HostEdge.Captured(async token => Fin<Option<StreamState>>.Succ(Optional(
            await session.Events.FetchStreamStateAsync(model.Value, token).ConfigureAwait(false))), cancellationToken).ConfigureAwait(false))
            .Bind(IO.lift)
            .Map(s => Received(model, s, static state => (Some(state.Version), Option<int>.None, Option<int>.None)));

    static Fin<StreamHead> Received<T>(ModelId model, Option<T> found, Func<T, (Option<long> Version, Option<int> Nodes, Option<int> Edges)> read) =>
        found.ToFin(new GraphFault.ModelAbsent(model))
            .Map(value => read(value) switch {
                (Option<long> version, Option<int> nodes, Option<int> edges) => new StreamHead(model, version, nodes, edges),
            });

    static IO<Fin<StreamHead>> Lift(IDocumentSession session, ModelId model, Option<long> expected, Error error, CancellationToken cancellationToken) =>
        error.Exception.Match(
            Some: ex => ex is Marten.Exceptions.ConcurrentUpdateException or JasperFx.Events.EventStreamUnexpectedMaxEventIdException
                ? expected.Match(
                    Some: guard => IO.liftAsync(async () => await HostEdge.Captured(async token => Fin<Option<StreamState>>.Succ(Optional(
                        await session.Events.FetchStreamStateAsync(model.Value, token).ConfigureAwait(false)), cancellationToken).ConfigureAwait(false))
                        .Bind(IO.lift)
                        .Map(state => state.Match(
                            Some: s => Fin<StreamHead>.Fail(new GraphFault.StreamVersionConflict(model, guard, s.Version, error)),
                            None: () => Fin<StreamHead>.Fail(error))),
                    None: () => IO.pure(Fin<StreamHead>.Fail(new GraphFault.TxnConflict(model, error))))
                : ex is PostgresException { IsTransient: true }
                    ? IO.pure(Fin<StreamHead>.Fail(new GraphFault.TxnConflict(model, error)))
                    : IO.pure(Fin<StreamHead>.Fail(error)),
            None: () => IO.pure(Fin<StreamHead>.Fail(error)));
}
```

| [INDEX] | [POLICY]               | [VALUE]                                    | [BINDING]                                                        |
| :-----: | :--------------------- | :----------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | one txn owner          | identity + event in one `IDocumentSession` | `IdentityStore.Stamp` then `SaveChangesAsync`                    |
|  [02]   | read consistency       | `FetchLatest<GraphProjection>`             | inline document or live tail fold; read-your-writes              |
|  [03]   | AS-OF fold             | `AggregateStreamAsync(version\|timestamp)` | version XOR instant; reuses `GraphDelta.ReplayOnto`              |
|  [04]   | optimistic concurrency | `Append(model, delta, expectedVersion)`    | racing writer aborts → `StreamVersionConflict`                   |
|  [05]   | exclusive escalation   | `CommitExclusive` op case                  | `FetchForExclusiveWriting`; refusal → `TxnConflict` 8302         |
|  [06]   | frame injection        | `StoreActor` + `ProjectionContext`         | AppHost fills clock, causal pair, and instrument set at the port |
|  [07]   | naming lineage         | `NameLineage` co-committed rows            | kernel `Track(prior, rebuilt)` reads a durable prior generation  |
|  [08]   | coordination edges     | `ModelLink` rows + `Link` op case          | cross-model references durable, project-scoped, blame-stamped    |
|  [09]   | project rollup         | `ProjectRollup` header-sliced, async       | roster + watermark only; never a second delta materializer       |
|  [10]   | causal pair            | `CorrelationId` + `TenantContext` on frame | kernel types seat on the frame; raw scalars only at pack sites   |
|  [11]   | tenant text            | `TenantContext.Entry` fixed-width `x32`    | RLS predicate, blame header, and meter tag compare alike         |
|  [12]   | elapsed read           | kernel `MonotonicTimeline` via `Since`     | provider identity admitted; a mark/elapsed delegate pair is out  |

## [05]-[FAULT_TABLES]

- Owner: the kernel `FaultBand` allocates the `GraphFault` direct union; generated identity derives its compact case codes from `FaultBand.Graph`.
- Entry: every Persistence fault family rides the kernel `Fault` floor — ONE roster realizing `[FaultCase]` declares the `Band` row and every case's `(Key, Offset)` pair, `Code` derive SEALED off the base, and `generated identity admission` proves offset uniqueness and span membership at first construction. Disjointness is the kernel's static `Disjoint` proof, folded at type initialization over the WHOLE roster and partitioned by `BandKind`, so an event band and a fault band may share a base while two bands of one kind may not.
- Growth: a new Persistence band is ONE row on the KERNEL roster beside the owning page's union derivation — the kernel's own accepted named loss, and the price of one proof over one code space; a new case inside a band is one union case and one roster row whose offset stays inside the row's `Span`; an outgrown union widens its row's `Span` into the neighbourhood's free tail at the kernel; zero new surface — a folder-local registry, a page-local band constant, a second roster beside a family's own, or a prose decade table is the deleted form.
- Boundary: `GraphFault.TxnConflict` carries the exact transient PostgreSQL cause; unknown provider errors remain exact.

```csharp
// --- [ERRORS] --------------------------------------------------------------------------
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

| [INDEX] | [POLICY]            | [VALUE]                                 | [BINDING]                                                |
| :-----: | :------------------ | :-------------------------------------- | :------------------------------------------------------- |
|  [01]   | band registry       | kernel `Rasm/Domain/results#FAULT_BAND` | this folder declares no rows; the roster allocates all   |
|  [02]   | band disjointness   | kernel static `Disjoint` proof          | one proof over the whole code space, partitioned by kind |
|  [03]   | code derivation     | `[FaultCase]` ordinals                  | `Code` seal off `Fault`                                  |
|  [04]   | provenance          | roster `Owner : TelemetrySource`        | the allocating package; no per-row page-anchor string    |
|  [05]   | folded-txn conflict | `GraphFault.TxnConflict` 8302           | registered sub-band row, never a loose 7001              |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
