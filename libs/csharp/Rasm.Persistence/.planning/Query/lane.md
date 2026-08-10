# [PERSISTENCE_QUERY_LANE]

Rasm.Persistence routes every read by its consistency demand: interactive-correctness queries (clash, void-resolution, live QTO, containment) bind the synchronous authoritative lane — the inline `Element/graph` `GraphProjection` and the in-process `Query/topology` QuikGraph view — while analytical queries (aggregation, search, columnar rollup) bind the async watermarked columnar and cypher lanes. Any query demanding correctness from an async view blocks on the projection daemon's non-stale wait before reading, so a read-your-writes interactive query is correct by construction and never touches a daemon-lagged projection.

`QueryLane` is the lane axis carrying each lane's wait policy; `ReadRequest` discriminates correctness and query modality without boolean products; `StalenessWatermark` measures projection lag as the event-log head sequence against the daemon shard's high-water mark. `ElementSet` is the universal content-addressed selection currency every clash/IDS/MVD/QTO surface consumes and produces — membership is the model-qualified `SetKey`, evaluation spans the caller-supplied `SetScope` model roster — `SetExpr` its selection-tree algebra and `SetPredicate` its closed typed leaf algebra, and `Closure` folds a bounded transitive walk over the `Query/topology` incidence. `Query/retrieval` takes every retrieval-shaped read into its fusion lane and read-through-caches on the `ElementSet.Receipt` this owner mints. `NodeId`/`ElementGraph` arrive from `Rasm.Element`, `ModelId` from `Element/graph#STREAM_GRAIN`, and the inline projection and analytical lanes arrive from their owners.

## [01]-[INDEX]

- [02]-[READ_ROUTING]: consistency-demand routing law, the `QueryLane` axis, the staleness watermark, and the daemon non-stale wait gate.
- [03]-[ELEMENT_SET_ALGEBRA]: `ElementSet` composable content-addressed selection currency, the typed leaf algebra, and the stable receipt fold.

## [02]-[READ_ROUTING]

- Owner: `QueryLane` carries the composition-time wait policy; `ReadRequest` is the closed correctness/modality discriminant; `StalenessWatermark` is measured sequence evidence; `ReadPhase` is the registered latency-name vocabulary and `ReadLedger` its once-resolved token index; `ReadRouter` owns routing, non-stale admission, daemon-fan measurement, and the phase-ledger bracket; `GraphQlDocument` admits the web-native query document and `ReflectedRead` owns the in-database `graphql.resolve` door over the RLS-guarded identity relations.
- Cases: `ReadRequest` is `Interactive | GraphAnalytic | Retrieval | Aggregate | Reuse | Reflected`; `QueryLane` is `Topology | Columnar | Cypher | Retrieval | Cache | Reflected`, and each row carries `Option<Duration> WaitBudget` with its `TargetSessionAttributes` session demand instead of a parallel consistency vocabulary; `ReadPhase` is `Routed | Waited | Connected | Executed`, one registered checkpoint name per row beside the one `LanePivot` tag dimension the lane key fills.
- Entry: `Route` folds `ReadRequest` directly to its lane; `AwaitNonStale` consumes the lane-carried wait budget and the production `IProjectionDaemon.WaitForNonStaleData`, returning the MEASURED elapsed wait the `store.query.wait` receipt seals; `Connect` resolves the lane's session demand off the one multihost source; `Observed` brackets one pooled `ILatencyContext` over the whole route-wait-execute run, stamping each `ReadPhase` checkpoint through its pre-resolved token, tagging the lane pivot, and draining the frozen `LatencyData` to the exporter before the release arm returns the context to its pool; `ReadLedger.Bind` resolves every token once at composition; `Measure` folds `EventStoreStatistics.EventSequenceNumber` against `ShardState.Sequence`, and its plural arm selects the worst shard; `public static IO<Fin<JsonElement>> ReflectedRead.Resolve(NpgsqlDataSource store, GraphQlDocument query, JsonElement variables, ProjectionContext frame)` runs ONE `graphql.resolve` call — the query document and its variables bind as parameters, the tenant GUC sets in-session so the identity tier's RLS partition applies, and the returned message envelope's `errors` array folds to the typed fault because the resolver never raises.
- Auto: an interactive-correctness query (clash narrow-phase, void-resolution, live QTO, containment ancestry) routes to the synchronous lane by construction so it reads the inline `GraphProjection` and QuikGraph view written in the append transaction, never a daemon-lagged async projection; an analytical query carries the `StalenessWatermark` so its consumer reads the lag; a re-run analytical clash demanding correctness from an async view calls `AwaitNonStale` first so the daemon catches up to the head before the read; the reflected door is the ZERO-RESOLVER web contract — `pg_graphql` reflects the live `element_identity`/`node_cell` schema (tables → object types, FKs → connection fields, comments → `@graphql` directives) into a Relay-paginated, introspectable GraphQL schema browser and mobile clients page through, recomputed lazily and DDL-invalidated by the extension's own event triggers, so a hand-written GraphQL schema or an out-of-process gateway beside the reflected one is the deleted form.
- Receipt: a routed read rides `store.query.route` carrying the demand and the lane; an async-stale wait rides `store.query.wait` carrying the watermark and the elapsed wait; a reflected read rides `store.query.reflected` carrying the operation name and the message envelope's error count.
- Packages: Marten (`IProjectionDaemon.WaitForNonStaleData(TimeSpan)` the production non-stale block; `ShardState`/`ShardName`/`EventStoreStatistics`, `AdvancedOperations.FetchEventStoreStatistics`/`AllProjectionProgress`), Npgsql (`NpgsqlDataSource.CreateCommand`/`NpgsqlParameter` — the `graphql.resolve` door; `NpgsqlDbType.Jsonb`; `NpgsqlMultiHostDataSource.CreateConnection(TargetSessionAttributes)` — the lane-session multihost door, `LoadBalanceHosts` a provisioning-DSN fact), pg_graphql (`graphql.resolve(query, variables, operationName, extensions)` → `jsonb` per `api-pg-graphql` — server-side, no managed assembly), Microsoft.Extensions.Telemetry.Abstractions (`ILatencyContextProvider.CreateContext`, `ILatencyContextTokenIssuer.GetCheckpointToken`/`GetTagToken`, `ILatencyContext.AddCheckpoint`/`SetTag`/`Freeze`/`LatencyData`, `ILatencyDataExporter.ExportAsync` — the contract half an instrumented library binds; the `AddLatencyContext` activation and the `LatencyContextRegistrationOptions` name registration are AppHost composition surface), NodaTime (`Duration`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new read modality is one `ReadRequest` case and one generated `Route` arm; a new analytical wait posture is one `QueryLane` row value; a new timed phase is one `ReadPhase` row the registration projection and the ledger index both pick up with no record-site edit; a reflected-schema tuning is an `@graphql` comment directive riding the identity tier's reviewed-migration DDL, never a resolver code path.
- Boundary: authoritative topology and containment stay synchronous and co-transactional (`C2`) — the inline `GraphProjection` in the write transaction, the in-process QuikGraph view — so a read-your-writes interactive query is correct by construction; that synchronous lane is NOT infallible, since the `Query/topology` `Traversals.Run` it binds returns `Fin<TopologyResult>` railing the typed `TopologyFault` band, so a router consumer composes the topology `Fin` into its OWN rail rather than assuming success and an absent-root containment query surfaces as an honest typed fault, never a silent empty result; AGE and DuckDB are ANALYTICAL ONLY with an explicit `StalenessWatermark`, and interactive-correctness queries block on `WaitForNonStaleData` and never route to an async projection without the wait — a clash reading a daemon-lagged AGE view is the deleted form, and the gate rides the production `IProjectionDaemon`, not a test-only symbol; staleness is a MEASURED sequence gap (`EventSequenceNumber` head against `ShardState.Sequence`), never `ShardState.Timestamp`, a daemon-side recording stamp (`DateTimeOffset.UtcNow` at row construction) that measures read-latency rather than producer-to-projection lag — a `Measure` returning `Duration.Zero` on a trailing shard is the illusory form this owner forbids; strong-consistency reads go through the inline projection and the synchronous topology, never the columnar aggregate, so the columnar lane stays the rollup/search lane and the topology lane the correctness lane; the reflected door executes wholly in-database over the RLS-guarded identity relations — AppHost hosts the web endpoint and maps its principal onto the tenant frame at the port boundary, Persistence owns only the parameterized `graphql.resolve` call, and the reflected mutation fields (`insertInto*/update*/deleteFrom*Collection`) are unexposed BY PRIVILEGE, not by prose — the resolve transaction pins `SET LOCAL ROLE` to the SELECT-only serving role (`ReflectedRead.ReadRole`, granted no INSERT/UPDATE/DELETE on any exposed relation), and pg_graphql reflects mutation fields only off writable relations, so schema reflection under the serving identity carries query fields alone; the identity tier's one write authority stays the `Element/graph#STORE_RAIL` rail; the phase ledger and the receipt are two rails over disjoint questions — the receipt answers how ONE read resolved and carries the watermark and the elapsed wait as typed fields, the ledger answers where EVERY read spends across its four phases — so a gap measure duplicated onto the ledger, or a phase duration lifted out of the ledger into a receipt field, is the deleted second owner; the pooled context never escapes its bracket, because `LatencyData` projects its spans over backing the pool re-leases on release, and its names never spell a literal at either end since an unregistered name resolves to a positionless token whose writes drop with nothing raised.

```csharp signature
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Diagnostics;                         // Stopwatch — the measured non-stale wait bracket
using Microsoft.Extensions.Diagnostics.Latency;   // the pooled per-operation phase ledger
using LanguageExt;
using Marten;
using Marten.Events.Daemon;
using Marten.Events.Projections;
using NetTopologySuite.Geometries;
using NodaTime;
using Npgsql;
using NpgsqlTypes;
using Rasm.Domain;                                // TenantContext — the S0 tenancy the frame seats
using Rasm.Element.Graph;
using Thinktecture;
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES); H3Cell — the identity cell
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReadRequest {
    private ReadRequest() { }
    public sealed record Interactive : ReadRequest;
    public sealed record GraphAnalytic : ReadRequest;
    public sealed record Retrieval : ReadRequest;
    public sealed record Aggregate : ReadRequest;
    public sealed record Reuse : ReadRequest;
    public sealed record Reflected : ReadRequest;
}

public readonly record struct StalenessWatermark(long HeadSequence, long ProjectedSequence) {
    public long Gap => Math.Max(0L, HeadSequence - ProjectedSequence);
    public bool IsStale => Gap > 0;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class QueryLane {
    public static readonly QueryLane Topology = new("topology", None, TargetSessionAttributes.Primary);
    public static readonly QueryLane Columnar = new("columnar", Some(Duration.FromSeconds(5)), TargetSessionAttributes.PreferStandby);
    public static readonly QueryLane Cypher = new("cypher", Some(Duration.FromSeconds(5)), TargetSessionAttributes.PreferStandby);
    public static readonly QueryLane Retrieval = new("retrieval", Some(Duration.FromSeconds(5)), TargetSessionAttributes.PreferStandby);
    public static readonly QueryLane Cache = new("cache", None, TargetSessionAttributes.Any);
    // Reflected reads hit the transactionally-current identity relations; no daemon, no wait budget, primary-pinned
    // because RLS role state and correctness both bind the writable session.
    public static readonly QueryLane Reflected = new("reflected", None, TargetSessionAttributes.Primary);
    public Option<Duration> WaitBudget { get; }
    // Multihost session target — the third lane column: correctness lanes pin the primary, watermark-carrying
    // analytical lanes prefer a standby so rollups ride replicas, and the provisioning SlotLag gauge is the
    // admission evidence behind that preference; a lane never spells a host, only its session demand.
    public TargetSessionAttributes Session { get; }
    private QueryLane(string key, Option<Duration> waitBudget, TargetSessionAttributes session) : this(key) {
        WaitBudget = waitBudget;
        Session = session;
    }
}

// `ReadPhase` spells the lane's phase vocabulary. A latency name governs only where the SAME spelling registers at composition
// and resolves at the record site: an unregistered name resolves to a POSITIONLESS token whose writes drop with
// nothing raised (only `LatencyContextOptions.ThrowOnUnregisteredNames` promotes that lookup to a boot failure),
// so a hand-spelled string at either end is a ledger that reads instrumented and reports nothing. The row IS the
// name, `Names` is the registration projection the composition root binds, and a new phase is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReadPhase {
    public static readonly ReadPhase Routed = new("rasm.persistence.read.routed");
    public static readonly ReadPhase Waited = new("rasm.persistence.read.waited");
    public static readonly ReadPhase Connected = new("rasm.persistence.read.connected");
    public static readonly ReadPhase Executed = new("rasm.persistence.read.executed");

    // `LanePivot` is the one TAG dimension the ledger carries: a frozen set groups by lane with no token
    // per lane row, so a seventh QueryLane needs no registration edit.
    public const string LanePivot = "rasm.persistence.read.lane";

    public static Seq<string> Names => toSeq(Items).Map(static row => row.Key);
}

// `GraphQlDocument` holds the web-native query text: non-empty, NUL-free, bound as a parameter — never concatenated.
[ValueObject<string>]
[ValidationError<SelectionFault>]
public readonly partial struct GraphQlDocument {
    static partial void ValidateFactoryArguments(ref SelectionFault? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('\0')) { validationError = new SelectionFault.Reflected("<document>"); }
    }
}

// Tokens resolve ONCE per composition off the issuer — the positional-token rail exists so the hot path carries
// no name lookup and no allocation, and a per-read `GetCheckpointToken` call throws that away. The phase index
// derives from `Items`, so a phase row can never be present in the vocabulary and absent from the ledger.
public sealed record ReadLedger(FrozenDictionary<ReadPhase, CheckpointToken> Phases, TagToken Lane) {
    public static ReadLedger Bind(ILatencyContextTokenIssuer issuer) =>
        new(ReadPhase.Items.ToFrozenDictionary(static row => row, row => issuer.GetCheckpointToken(row.Key)),
            issuer.GetTagToken(ReadPhase.LanePivot));
}

public static class ReadRouter {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.query.route"), StoreSlot.Create("store.query.wait"), StoreSlot.Create("store.query.reflected"),
        StoreSlot.Create("store.elementset.eval"));

    public static QueryLane Route(ReadRequest request) => request.Switch(
        interactive: static _ => QueryLane.Topology,
        graphAnalytic: static _ => QueryLane.Cypher,
        retrieval: static _ => QueryLane.Retrieval,
        aggregate: static _ => QueryLane.Columnar,
        reuse: static _ => QueryLane.Cache,
        reflected: static _ => QueryLane.Reflected);

    // Lane session demand resolves a connection off the one multihost source — analytical reads land on a
    // standby when one serves, correctness lanes always the primary; `LoadBalanceHosts` stays a provisioning-DSN
    // fact so the router never spells a host.
    public static NpgsqlConnection Connect(NpgsqlMultiHostDataSource store, QueryLane lane) => store.CreateConnection(lane.Session);

    // Waits are MEASURED — the store.query.wait receipt's elapsed-wait field reads a clock this member starts,
    // never a forged zero: one timestamp bracket around the production non-stale block, the elapsed returned to the
    // caller that seals it beside the watermark. This elapsed is the RECEIPT's typed field for one read; the
    // cross-phase ledger `Observed` brackets is the other rail, and neither re-derives the other.
    public static IO<Duration> AwaitNonStale(IProjectionDaemon daemon, QueryLane lane) =>
        lane.WaitBudget.Match(
            Some: budget => IO.liftAsync(async () => {
                long start = Stopwatch.GetTimestamp();
                await daemon.WaitForNonStaleData(budget.ToTimeSpan()).ConfigureAwait(false);
                return Duration.FromTimeSpan(Stopwatch.GetElapsedTime(start));
            }),
            None: static () => IO.pure(Duration.Zero));

    // `Observed` measures the whole read as ONE pooled ledger, never one phase: a read routes, waits on the daemon,
    // reaches its lane session, and executes, and only the relative cost of those four says where a slow read is
    // slow — `store.query.wait` answers how ONE read resolved and the ledger answers where every read spends, so the
    // two rails coexist and neither re-derives the other. `Bracket` owns the pooled context on every exit path
    // including failure, because a leaked context starves the pool for every later read. `AddCheckpoint` stamps
    // once per context, so a re-entrant phase records a measure rather than a second stamp.
    public static IO<T> Observed<T>(ILatencyContextProvider pool, ILatencyDataExporter drain, ReadLedger ledger,
                                    IProjectionDaemon daemon, ReadRequest request, Func<QueryLane, IO<T>> read) =>
        IO.lift(pool.CreateContext).Bracket(
            Use: cell => Phased(cell, drain, ledger, daemon, Route(request), read),
            Fin: static cell => IO.lift(() => { cell.Dispose(); return unit; }));

    static IO<T> Phased<T>(ILatencyContext cell, ILatencyDataExporter drain, ReadLedger ledger,
                           IProjectionDaemon daemon, QueryLane lane, Func<QueryLane, IO<T>> read) =>
        from _pivot in IO.lift(() => { cell.SetTag(ledger.Lane, lane.Key); return unit; })
        from _route in Stamp(cell, ledger, ReadPhase.Routed)
        from _wait  in AwaitNonStale(daemon, lane).Bind(_ => Stamp(cell, ledger, ReadPhase.Waited))
        from value  in read(lane)
        from _done  in Stamp(cell, ledger, ReadPhase.Executed)
        from _drain in Sealed(cell, drain)
        select value;

    static IO<Unit> Stamp(ILatencyContext cell, ReadLedger ledger, ReadPhase phase) =>
        IO.lift(() => { cell.AddCheckpoint(ledger.Phases[phase]); return unit; });

    // `Sealed` drains the frozen set INSIDE the bracket: `LatencyData` projects its checkpoint, tag, and measure
    // spans over the context's POOLED backing, so a set carried past the release arm reads storage the pool has
    // already re-leased to another read and reports one read's phases under another's identity.
    static IO<Unit> Sealed(ILatencyContext cell, ILatencyDataExporter drain) =>
        IO.lift(() => { cell.Freeze(); return unit; })
            .Bind(_ => IO.liftAsync(async () => {
                await drain.ExportAsync(cell.LatencyData, CancellationToken.None).ConfigureAwait(false);
                return unit;
            }));

    // `ShardState.Timestamp` is daemon observation time; only event and shard sequences measure projection progress.
    public static StalenessWatermark Measure(EventStoreStatistics head, ShardState projection) =>
        new(head.EventSequenceNumber, projection.Sequence);

    // Daemon-fan staleness is the maximum shard gap; averaging masks stragglers, and an empty fan projects
    // sequence zero because no shard has advanced.
    public static StalenessWatermark Measure(EventStoreStatistics head, Seq<ShardState> shards) =>
        shards.IsEmpty
            ? new StalenessWatermark(head.EventSequenceNumber, 0L)
            : shards.Map(state => Measure(head, state))
                .Fold(new StalenessWatermark(head.EventSequenceNumber, head.EventSequenceNumber),
                    static (worst, next) => next.Gap > worst.Gap ? next : worst);

    public static IO<StalenessWatermark> Measure(IDocumentStore store, ShardName shard) =>
        from stats in IO.liftAsync(() => store.Advanced.FetchEventStoreStatistics())
        from progress in IO.liftAsync(() => store.Advanced.AllProjectionProgress())
        // Missing progress is evidence of no projected sequence and therefore measures from zero.
        select toSeq(progress).Find(s => s.ShardName == shard.Identity).Match(
            Some: state => Measure(stats, state),
            None: () => new StalenessWatermark(stats.EventSequenceNumber, 0L));
}

// `ReflectedRead` is the reflected door: one transaction pins the tenant GUC (RLS partition) and resolves the
// whole GraphQL operation in-database; the resolver never raises, so the errors message envelope folds typed.
public static class ReflectedRead {
    // `ReadRole` is the SELECT-only serving role: reflection surfaces mutation fields only off relations the
    // executing role can write, so the privilege pin IS the mutation gate — RLS partitions rows, the role removes the write surface.
    const string ReadRole = "rasm_graphql_read";

    public static IO<Fin<JsonElement>> Resolve(NpgsqlDataSource store, GraphQlDocument query, JsonElement variables, Option<string> operation, ProjectionContext frame) =>
        IO.liftAsync(async () => {
            await using NpgsqlConnection lane = await store.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlTransaction scope = await lane.BeginTransactionAsync().ConfigureAwait(false);
            try {
                await using NpgsqlBatch batch = lane.CreateBatch();
                NpgsqlBatchCommand role = new($"SET LOCAL ROLE {ReadRole}");
                // `TenantContext.TenantSlot` is the GUC key and its one `Entry` text the value, so the RLS
                // policy compares the same canonical spelling the durable column stores.
                NpgsqlBatchCommand pin = new($"SELECT set_config('{TenantContext.TenantSlot}', @tenant, true)");
                _ = pin.Parameters.AddWithValue("tenant", frame.Tenant.Entry);
                NpgsqlBatchCommand door = new("SELECT graphql.resolve(@query, @variables, @operation, NULL)");
                _ = door.Parameters.AddWithValue("query", (string)query);
                door.Parameters.Add(new NpgsqlParameter("variables", NpgsqlDbType.Jsonb) { Value = variables.GetRawText() });
                _ = door.Parameters.AddWithValue("operation", operation.Match<object>(Some: static name => name, None: static () => DBNull.Value));
                batch.BatchCommands.Add(role);
                batch.BatchCommands.Add(pin);
                batch.BatchCommands.Add(door);
                await using NpgsqlDataReader evidence = await batch.ExecuteReaderAsync().ConfigureAwait(false);
                _ = await evidence.NextResultAsync().ConfigureAwait(false);
                _ = await evidence.NextResultAsync().ConfigureAwait(false);
                string envelope = await evidence.ReadAsync().ConfigureAwait(false) ? evidence.GetString(0) : "{}";
                await evidence.DisposeAsync().ConfigureAwait(false);
                await scope.CommitAsync().ConfigureAwait(false);
                using JsonDocument parsed = JsonDocument.Parse(envelope);
                return parsed.RootElement.TryGetProperty("errors", out JsonElement errors) && errors.GetArrayLength() > 0
                    ? Fin<JsonElement>.Fail(new SelectionFault.Reflected(errors[0].GetRawText()))
                    : Fin<JsonElement>.Succ(parsed.RootElement.TryGetProperty("data", out JsonElement data) ? data.Clone() : default);
            }
            catch (PostgresException wire) { return Fin<JsonElement>.Fail(new SelectionFault.Reflected(wire.MessageText)); }
        });
}
```

| [INDEX] | [POLICY]                | [VALUE]                                       | [BINDING]                                             |
| :-----: | :---------------------- | :-------------------------------------------- | :---------------------------------------------------- |
|  [01]   | interactive correctness | the synchronous `Topology` lane               | inline projection + QuikGraph; never an async view    |
|  [02]   | analytical              | the async `Columnar`/`Cypher` lane            | carries the `StalenessWatermark`                      |
|  [03]   | request routing         | one `ReadRequest` case                        | impossible combinations are absent                    |
|  [04]   | non-stale gate          | `IProjectionDaemon.WaitForNonStaleData`       | the production runner member; not `TestingExtensions` |
|  [05]   | watermark               | `EventSequenceNumber` vs shard `Sequence`     | sequence evidence; no synthetic wall duration         |
|  [06]   | reflected door          | one `graphql.resolve` call, RLS tenant pinned | zero resolver code; errors message envelope folds typed       |
|  [07]   | phase ledger            | one pooled `ILatencyContext` per read         | bracketed; frozen set drains before the pool release  |
|  [08]   | latency names           | the `ReadPhase` roster projection             | one vocabulary registers and records; no literal      |

## [03]-[ELEMENT_SET_ALGEBRA]

- Owner: `ElementSet` the polymorphic composable selection record carrying a stable content-addressed receipt; `SetKey` the model-qualified member with the one cross-runtime total order; `SetScope` the caller-supplied model roster evaluation resolves across; `SetPredicate` the closed leaf-predicate algebra; `SetExpr` the selection-tree algebra; `WalkDepth` the admitted bounded-depth `[ValueObject<int>]` every bounded walk carries — the `Closure` fold, the `Cell` ring, the `Query/topology` `Ancestry`/`Descent`, the `Query/cypher` `Reach` hops all consume this ONE axis; `SelectionFault` the closed admission band (846x off the `Element/graph#FAULT_TABLES` registry) an invalid bound rails; `ElementSetAlgebra` the static surface owning literal selection, the boolean/spatial/cell/property/classification combinators, and the stable-receipt fold.
- Cases: `Spatial | Cell | Jsonpath | Classification | Containment | Material | Exists | Raster` on `SetPredicate` (the bounded operator within each typed — `SpatialOp` on `Spatial`, `JsonComparison` on `Jsonpath`, the admitted `WalkDepth` ring on `Cell`, `RasterOp` on `Raster`; `Containment` anchors on a model-qualified `SetKey`); `Literal | Predicate | ByRule | Union | Intersect | Difference | Closure` on `SetExpr`, `Literal` carrying `SetKey` members.
- Entry: `public static Fin<ElementSet> Evaluate(SetExpr expr, SetScope scope, SetResolve resolve)` aborts on an index or expansion failure, rails a literal key outside the scope, and otherwise folds the expression tree into a stable key set; `Receipt` derives the content-addressed set identity over the framed distinct-sorted preimage; `Canonical` is the preimage the parity corpus freezes.
- Auto: an element set is the universal BIM currency — clash, IDS, MVD, QTO, and rule surfaces all consume and produce `ElementSet` values, so a clash result is an `ElementSet`, an IDS pass-set is an `ElementSet`, and a QTO subject is an `ElementSet`; the set receipt is `XxHash128` over the FRAMED distinct-sorted `SetKey` preimage (a LE `int32` key count, then per key the fixed-width 16-byte big-endian model bytes and an LE `int32` node byte length with its UTF8 bytes) so two selections yielding the same members share one receipt AND two different key sets can never collide on an unframed concatenation; the boolean combinators fold over evaluated leaf sets, and the one `Predicate` leaf carries a `SetPredicate` — `Spatial` lowers to the GiST predicate the TYPED `SpatialOp` `.Key` (`ST_Intersects`/`ST_Within`/`ST_DWithin`/…) names so a typo is a missing vocabulary row at compile time rather than a silent sequential scan — the `Ranged` `ST_DWithin` row consumes the leaf's `Distance` radius, and a ranged op without `Some` rails `SelectionFault.Rejected` at leaf lowering rather than lowering a two-argument call the server rejects, `Cell` to the `h3-pg` grid-disk bucket predicate over the identity tier's cell column (`h3_grid_disk(anchor, k)` membership the cell index serves — the H3 counterpart of the `Spatial` GiST leaf, so a storey-band or proximity selection is index-served without a geometry decode), `Jsonpath` to a jsonb path predicate under the typed `JsonComparison` comparator, `Classification` to a tsvector/classification predicate, `Containment` to the containment-edge ancestry, `Material`/`Exists` to their jsonb existence forms, `Raster` to the `postgis_raster` in-db predicate the typed `RasterOp.Key` names — bare `ST_Intersects(rast, geom)` coverage membership, or the statistical `ST_SummaryStats(ST_Clip(rast, geom), band)` mean against the leaf `Threshold` — so a "sample the coverage under this footprint" selection pushes server-side onto the provisioned `postgis_raster` extension and never pays a full blob fetch and in-process decode (the extension's `Degradable` rank folds this leaf out at admission when the cluster lacks it); every bounded walk carries the admitted `WalkDepth` — a raw `int` depth never crosses into the interior, so a negative bound is a typed `SelectionFault.Depth` at admission, never a silent empty selection the `<= depth` predicate fakes; the `Closure` arm is a GENUINE bounded transitive fold — it evaluates its `Seed` sub-expression then folds `Depth` one-hop `Expand` waves accumulating the reachable frontier to its fixpoint, never an opaque leaf identical to `Predicate`.
- Receipt: an evaluation rides `store.elementset.eval` carrying the leaf count and the result cardinality; the stable receipt is the reuse key the `Query/retrieval#FUSION_AND_REUSE` read-through caches on.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of` — the one federation hasher, seed-zero `XxHash128` value-identical; `Expected` the band base), Rasm.Persistence (`Element/graph#STREAM_GRAIN` `ModelId` — the `SetKey` model half; `Element/identity#ELEMENT_IDENTITY` `H3Cell` — the `Cell` leaf anchor; `Element/graph#FAULT_TABLES` `FaultBand` — the `Selection` band registry row), System.Buffers (`ArrayBufferWriter`/`BinaryPrimitives`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NetTopologySuite, NodaTime, BCL inbox.
- Growth: a new selection primitive is one `SetPredicate` case (lowered by the `Predicate` leaf) or one `SetExpr` tree case; a new spatial operator is one `SpatialOp` row, a new jsonb comparator one `JsonComparison` row; a new bounded walk consumes the ONE `WalkDepth` admission, never a second depth carrier; a new combinator is one fold arm; zero new surface — a per-discipline selection class, a saved-search table, a string-query DSL, a raw-string leaf, or a free-string operator on a typed leaf is the deleted form because the algebra is one composable tree the planner lowers, every leaf predicate is a typed case, and every bounded operator within a leaf is a vocabulary row.
- Boundary: `ElementSet` is the one composable currency — every analysis surface takes an `ElementSet` and yields an `ElementSet` so results compose (a clash result intersected with a classification selection is one `SetExpr.Intersect`, never a join in application code); the receipt is content-addressed over the length-framed distinct-sorted preimage so it is stable across runs, peers, and tenants AND unambiguous — a positional or timestamp-keyed selection id, or an unframed byte concatenation two key sets collide on, is the deleted form; the `Closure` combinator is a real bounded transitive fold whose one-hop `Expand` is the `Query/topology#GRAPH_TOPOLOGY` incidence neighbour over the seam graph (the reachability owner stays the graph/topology owner, the bounded fold stays here), NEVER the `Version/ledger#CHANGEFEED` `Closure` — that ledger manifest is a representation-content-hash blob-transfer set keyed by `UInt128`, a DIFFERENT closure that cannot answer a `NodeId` reachability selection, so conflating the two is the deleted altitude error; every leaf predicate is a typed `SetPredicate` case and every bounded operator within it is a vocabulary row — the spatial operator is a `SpatialOp` smart-enum, the jsonb comparator a `JsonComparison` smart-enum — so a selection that promised a spatial intersection carries the typed `ST_*` operator the GiST index serves and the geometry, never a free string a typo degrades to a scan; selection evaluation pushes through the lane router so a `Spatial` leaf executes on the GiST index and a `Jsonpath` leaf on the jsonb index in the store, never client-side; scope is caller DATA — evaluation takes the `SetScope` roster as a value and reads no project rollup of its own, so read-your-writes holds per model and the async `ProjectGraph` roster is one legitimate supplier of a scope, never the evaluator's own read; a federated selection spans separate model streams WITHOUT minting a union graph — the seam `Federate` union stays the materialized-coordination path under its one header, and neither substitutes for the other; the `ElementSet.Preimage` framed byte shape — fixed-width big-endian model bytes beside the length-framed node text under the `SetKey` order — is what the `Version/commits#CRDT_WIRE` `ContentParityCorpus.Contribute(ParitySlot.ElementSet, set.Preimage)` freezes as the `elementset` parity vector (CONTRIBUTED by this owner, never reverse-imported into the Version owner), and a membership or framing change re-cuts that vector in the same pass.

```csharp signature
// Jsonb-predicate vocabulary (`@>`/`?`/`->>` comparisons the GIN `jsonb_ops` index serves) is one closed
// row set, never a free comparison string. `SetPath` admits the unbounded path data once.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JsonComparison {
    public static readonly JsonComparison Exists = new("exists");
    // `Eq`, never `Equals`: a static item field named `Equals` collides with the generated
    // `Equals(object?)`/`Equals(JsonComparison?)` members in the same partial type (CS0102); the wire key stays "eq".
    public static readonly JsonComparison Eq = new("eq");
    public static readonly JsonComparison Contains = new("contains");
    public static readonly JsonComparison GreaterThan = new("gt");
    public static readonly JsonComparison GreaterOrEqual = new("gte");
    public static readonly JsonComparison LessThan = new("lt");
    public static readonly JsonComparison LessOrEqual = new("lte");
    public static readonly JsonComparison Matches = new("matches");
}

// --- [ERRORS] -------------------------------------------------------------------------------
// `SelectionFault` closes the `FaultBand.Selection` decade over `Rasm.Domain.Expected`.
// `Depth` carries invalid bounded-walk admission rather than a silent empty selection.
[Union]
public abstract partial record SelectionFault : Expected, IValidationError<SelectionFault> {
    private SelectionFault() : base() { }
    public sealed record Depth(int Found) : SelectionFault;
    public sealed record Rejected(string Detail) : SelectionFault;
    public sealed record Reflected(string Detail) : SelectionFault;
    public sealed record Scope(string Detail) : SelectionFault;

    public override int Code => FaultBand.Selection + Switch(
        depth:     static _ => 0,
        rejected:  static _ => 1,
        reflected: static _ => 2,
        scope:     static _ => 3);

    public override string Message => Switch(
        depth:     static c => $"<selection-depth:{c.Found}>",
        rejected:  static c => $"<selection-rejected:{c.Detail}>",
        reflected: static c => $"<selection-reflected:{c.Detail}>",
        scope:     static c => $"<selection-scope:{c.Detail}>");

    public override string Category => Switch(
        depth:     static _ => "Depth",
        rejected:  static _ => "Rejected",
        reflected: static _ => "Reflected",
        scope:     static _ => "Scope");

    public static SelectionFault Create(string message) => new Rejected(message);
}

// `WalkDepth` is the sole bounded-depth axis for closure, cell rings, topology walks, and Cypher hops.
// Admission rails negative values before any interior predicate executes.
[ValueObject<int>]
[ValidationError<SelectionFault>]
public readonly partial struct WalkDepth {
    static partial void ValidateFactoryArguments(ref SelectionFault? validationError, ref int value) {
        if (value < 0) { validationError = new SelectionFault.Depth(value); }
    }
}

[ValueObject<string>]
[ValidationError<SelectionFault>]
public readonly partial struct SetPath {
    static partial void ValidateFactoryArguments(ref SelectionFault? validationError, ref string value) {
        value = value.Trim();
        if (value.Length == 0 || value.Contains('\0')) {
            validationError = new SelectionFault.Rejected("<set-path>");
        }
    }
}

// `SpatialOp` carries each GiST-served PostGIS function name as its key.
// Typed rows prevent misspelled operators from degrading into sequential scans; a `Ranged` row
// (`ST_DWithin`) consumes the leaf's `Distance`, and a ranged op without `Some` rails at leaf lowering.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpatialOp {
    public static readonly SpatialOp Intersects = new("ST_Intersects", ranged: false);
    public static readonly SpatialOp Contains = new("ST_Contains", ranged: false);
    public static readonly SpatialOp Within = new("ST_Within", ranged: false);
    public static readonly SpatialOp DWithin = new("ST_DWithin", ranged: true);
    public static readonly SpatialOp Overlaps = new("ST_Overlaps", ranged: false);
    public static readonly SpatialOp Touches = new("ST_Touches", ranged: false);
    public static readonly SpatialOp Covers = new("ST_Covers", ranged: false);
    public static readonly SpatialOp CoveredBy = new("ST_CoveredBy", ranged: false);
    public bool Ranged { get; }
    private SpatialOp(string key, bool ranged) : this(key) => Ranged = ranged;
}

// `RasterOp` carries each `postgis_raster` server predicate as its key — the coverage counterpart of `SpatialOp`,
// so an elevation or overlay selection pushes onto the in-db raster exactly as the `Spatial` leaf pushes onto the
// GiST index, never a full blob fetch plus in-process decode. `Statistical` rows clip the band under the element
// footprint (`ST_SummaryStats(ST_Clip(rast, geom), band)`) and compare the fold's mean against the leaf threshold,
// and its non-statistical row is bare raster-geometry `ST_Intersects` coverage membership.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RasterOp {
    public static readonly RasterOp Intersects = new("ST_Intersects", statistical: false);
    public static readonly RasterOp MeanAbove = new("ST_SummaryStats", statistical: true);
    public static readonly RasterOp MeanBelow = new("ST_SummaryStats", statistical: true);
    public bool Statistical { get; }
    private RasterOp(string key, bool statistical) : this(key) => Statistical = statistical;
}

// `SetKey` is the model-qualified member: the owning stream's `ModelId` beside the seam `NodeId`, ordered by
// model RFC-4122 big-endian wire bytes then ordinal over the node text — ONE total order every runtime derives
// from the same two byte sequences, never `Guid.CompareTo`'s field-wise order no peer reproduces.
public readonly record struct SetKey(ModelId Model, NodeId Node) : IComparable<SetKey> {
    public int CompareTo(SetKey other) {
        Span<byte> mine = stackalloc byte[16];
        Span<byte> theirs = stackalloc byte[16];
        Model.Value.TryWriteBytes(mine, bigEndian: true, out _);
        other.Model.Value.TryWriteBytes(theirs, bigEndian: true, out _);
        int byModel = mine.SequenceCompareTo(theirs);
        return byModel != 0 ? byModel : string.CompareOrdinal(Node.Value, other.Node.Value);
    }
}

// `SetScope` is the CALLER-supplied model roster leaf resolution spans — one model for a single-model
// selection, the roster a `ProjectGraph` read handed over for a project-altitude one. Scope arrives as DATA,
// so an interactive cross-model selection still binds the synchronous per-model projections and
// read-your-writes holds; an evaluator-side roster read would inherit the async daemon's lag.
public readonly record struct SetScope(Seq<ModelId> Models) {
    public static Fin<SetScope> Of(Seq<ModelId> models) =>
        models.IsEmpty
            ? Fin.Fail<SetScope>(new SelectionFault.Scope("<empty>"))
            : Fin.Succ(new SetScope(toSeq(models.Distinct())));
    public bool Admits(ModelId model) => Models.Contains(model);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SetPredicate {
    private SetPredicate() { }
    public sealed record Spatial(SpatialOp Op, Geometry Operand, Option<double> Distance) : SetPredicate;
    public sealed record Cell(H3Cell Anchor, WalkDepth Ring) : SetPredicate;
    public sealed record Jsonpath(SetPath Path, JsonComparison Cmp, Option<string> Value) : SetPredicate;
    public sealed record Classification(SetPath SystemPath, Option<string> Value) : SetPredicate;
    // `Ancestor` names its own model: a containment walk climbs ONE model's spatial tree, and that qualified
    // key lets a project-scoped expression seat per-model containment leaves side by side.
    public sealed record Containment(SetKey Ancestor, bool Subtree) : SetPredicate;
    public sealed record Material(Option<string> Value) : SetPredicate;
    public sealed record Exists(SetPath Path) : SetPredicate;
    // Coverage-raster leaf: elements whose geometry the named coverage admits under the raster predicate — a
    // statistical row demands `Some` threshold and rails `SelectionFault.Rejected` at lowering without one,
    // exactly as a ranged `Spatial` row demands its `Distance`.
    public sealed record Raster(RasterOp Op, string Coverage, int Band, Option<double> Threshold) : SetPredicate;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SetExpr {
    private SetExpr() { }
    public sealed record Literal(Seq<SetKey> Keys) : SetExpr;
    public sealed record Predicate(SetPredicate Leaf) : SetExpr;
    public sealed record ByRule(string RuleId) : SetExpr;
    public sealed record Union(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Intersect(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Difference(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Closure(SetExpr Seed, WalkDepth Depth) : SetExpr;
}

public readonly record struct ElementSet(UInt128 Receipt, Seq<SetKey> Keys, int Count, ReadOnlyMemory<byte> Preimage) {
    public static readonly ElementSet Empty = Of(Seq<SetKey>());
    // `Preimage` exposes the exact framed bytes hashed by `Receipt` and contributed to the parity corpus.
    public static ElementSet Of(Seq<SetKey> keys) {
        Seq<SetKey> sorted = toSeq(keys.Distinct().OrderBy(static k => k));
        ReadOnlyMemory<byte> preimage = ElementSetAlgebra.Canonical(sorted);
        return new ElementSet(ContentHash.Of(preimage.Span), sorted, sorted.Count, preimage);
    }
}

// `SetResolve` carries scope-threaded index-backed leaf resolution and one-hop topology expansion.
// Threaded ports keep reachability in the graph owner while this page owns algebraic closure; `Expand`
// crosses model boundaries only through the durable `ModelLink` edges the project view lifts.
public readonly record struct SetResolve(Func<SetExpr, SetScope, Fin<Seq<SetKey>>> Leaf, Func<Seq<SetKey>, Fin<Seq<SetKey>>> Expand);

public static class ElementSetAlgebra {
    // `Receipt` uses the kernel seed-zero `ContentHash.Of` entry over parity-frozen bytes.
    public static UInt128 Receipt(Seq<SetKey> sortedKeys) => ContentHash.Of(Canonical(sortedKeys).Span);

    // Cross-runtime parity: an LE `int32` key count, then per key the FIXED-WIDTH 16-byte RFC-4122 big-endian
    // model bytes UNFRAMED (the preimage-framing law length-frames only variable width) and the node text as
    // an LE `int32` byte length plus UTF8. Sorted under the `SetKey` comparator, so every runtime derives one
    // byte stream from one member set and framing distinguishes concatenation-equivalent rosters.
    public static ReadOnlyMemory<byte> Canonical(Seq<SetKey> sortedKeys) {
        ArrayBufferWriter<byte> buffer = new();
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4), sortedKeys.Count);
        buffer.Advance(4);
        foreach (SetKey key in sortedKeys) {
            key.Model.Value.TryWriteBytes(buffer.GetSpan(16), bigEndian: true, out _);
            buffer.Advance(16);
            int bytes = Encoding.UTF8.GetByteCount(key.Node.Value);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4), bytes);
            buffer.Advance(4);
            Encoding.UTF8.GetBytes(key.Node.Value, buffer.GetSpan(bytes));
            buffer.Advance(bytes);
        }
        return buffer.WrittenMemory;
    }

    public static Fin<ElementSet> Evaluate(SetExpr expr, SetScope scope, SetResolve resolve) => expr.Switch(
        (Scope: scope, Resolve: resolve),
        // `Evaluate` rails on a literal key naming a model outside the scope, keeping out every member no
        // leaf resolution admits.
        literal: static (s, lit) => lit.Keys.Find(key => !s.Scope.Admits(key.Model)).Match(
            Some: foreign => Fin.Fail<ElementSet>(new SelectionFault.Scope($"<literal-model:{foreign.Model.Value}>")),
            None: () => Fin.Succ(ElementSet.Of(lit.Keys))),
        predicate: static (s, e) => s.Resolve.Leaf(e, s.Scope).Map(ElementSet.Of),
        byRule: static (s, e) => s.Resolve.Leaf(e, s.Scope).Map(ElementSet.Of),
        union: static (s, u) =>
            from left in Evaluate(u.Left, s.Scope, s.Resolve)
            from right in Evaluate(u.Right, s.Scope, s.Resolve)
            select ElementSet.Of(left.Keys + right.Keys),
        intersect: static (s, i) =>
            from left in Evaluate(i.Left, s.Scope, s.Resolve)
            from right in Evaluate(i.Right, s.Scope, s.Resolve)
            select ElementSet.Of(toSeq(left.Keys.Intersect(right.Keys))),
        difference: static (s, d) =>
            from left in Evaluate(d.Left, s.Scope, s.Resolve)
            from right in Evaluate(d.Right, s.Scope, s.Resolve)
            select ElementSet.Of(toSeq(left.Keys.Except(right.Keys))),
        closure: static (s, c) => Evaluate(c.Seed, s.Scope, s.Resolve).Bind(seed => Closed(seed.Keys, c.Depth.Value, s.Resolve.Expand)));

    static Fin<ElementSet> Closed(Seq<SetKey> seed, int depth, Func<Seq<SetKey>, Fin<Seq<SetKey>>> expand) =>
        Range(0, depth).Fold(
            Fin.Succ((Reached: seed, Frontier: seed)),
            (state, _) => state.Bind(acc => acc.Frontier.IsEmpty
                ? Fin.Succ(acc)
                : expand(acc.Frontier)
                    .Map(next => toSeq(next.Except(acc.Reached)))
                    .Map(ring => (acc.Reached + ring, ring))))
        .Map(static result => ElementSet.Of(result.Reached));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                                 | [BINDING]                                                |
| :-----: | :----------------- | :------------------------------------------------------ | :------------------------------------------------------- |
|  [01]   | selection currency | `ElementSet` in and out                                 | every analysis surface composes; never an app join       |
|  [02]   | membership         | `SetKey` — `(ModelId, NodeId)` under one byte order     | federation-altitude members; comparator is cross-runtime |
|  [03]   | scope              | caller-supplied `SetScope` model roster                 | data, never an evaluator-side async roster read          |
|  [04]   | receipt            | `ContentHash.Of` over the framed preimage               | stable + collision-free; the reuse key + parity preimage |
|  [05]   | typed leaves       | `SetPredicate` + `SpatialOp`/`JsonComparison` operators | no raw-string predicate/op; lowered to a store index     |
|  [06]   | closure            | bounded transitive fold over topology                   | one-hop `Expand` is `Query/topology`; not the manifest   |
|  [07]   | bounded depth      | `WalkDepth` admitted once (`SelectionFault.Depth`)      | closure/cell/topology/cypher share ONE axis; no raw int  |
|  [08]   | cell leaf          | `Cell(H3Cell, WalkDepth)` grid-disk predicate           | `h3-pg` index-served; the H3 sibling of the GiST leaf    |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
