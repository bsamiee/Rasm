# [PERSISTENCE_QUERY_LANE]

Rasm.Persistence routes every read by its consistency demand: interactive-correctness queries (clash, void-resolution, live QTO, containment) bind the synchronous authoritative lane — the inline `Element/graph` `GraphProjection` and the in-process `Query/topology` QuikGraph view — while analytical queries (aggregation, search, columnar rollup) bind the async watermarked columnar and cypher lanes. Any query demanding correctness from an async view blocks on the projection daemon's non-stale wait before reading, so a read-your-writes interactive query is correct by construction and never touches a daemon-lagged projection.

`QueryLane` is the lane axis carrying each lane's wait policy; `ReadRequest` discriminates correctness and query modality without boolean products; `StalenessWatermark` measures projection lag as the event-log head sequence against the daemon shard's high-water mark. Selection rides the Element algebra: `Rasm.Element` `Query/predicate#PREDICATE_ALGEBRA` owns the boolean closure (`Predicate<TLeaf>` with `All`/`Any`/`Not`/`Closure`, `Selection<TKey>`, `WalkDepth`, `MatchVerdict`) and this owner instantiates it over `SetPredicate`, the closed STORE leaf family every leaf lowers to an index predicate. `KeySelection` is the content-certified selection currency every clash/IDS/MVD/QTO surface consumes and produces — the algebra's `Selection<SetKey>` beside the `SetScope` the evaluation spanned and the framed preimage its content key hashes — membership is the model-qualified `SetKey`, and the `Closure` arm folds a bounded transitive walk over the `Query/topology` incidence. `Query/retrieval` takes every retrieval-shaped read into its fusion lane and read-through-caches on the `KeySelection.ContentKey` this owner mints. `NodeId`/`ElementGraph` arrive from `Rasm.Element`, `ModelId` from `Element/graph#STREAM_GRAIN`, and the inline projection and analytical lanes arrive from their owners.

## [01]-[INDEX]

- [02]-[READ_ROUTING]: consistency-demand routing law, the `QueryLane` axis, the staleness watermark, and the daemon non-stale wait gate.
- [03]-[ELEMENT_SET_ALGEBRA]: algebra closure instantiated over `SetPredicate` the store leaf family, `KeySelection` the content-certified selection currency, the set-valued pushdown fold, and the frozen parity preimage.

## [02]-[READ_ROUTING]

- Owner: `QueryLane` carries the composition-time wait policy; `ReadRequest` is the closed correctness/modality discriminant; `StalenessWatermark` is measured sequence evidence; `ReadPhase` is the registered latency-name vocabulary and `ReadLedger` its once-resolved token index; `ReadRouter` owns routing, non-stale admission, daemon-fan measurement, and the phase-ledger bracket; `GraphQlDocument` admits the web-native query document and `ReflectedRead` owns the in-database `graphql.resolve` door over the RLS-guarded identity relations.
- Cases: `ReadRequest` is `Interactive | GraphAnalytic | Retrieval | Aggregate | Reuse | Reflected`; `QueryLane` is `Topology | Columnar | Cypher | Retrieval | Cache | Reflected`, and each row carries `Option<Duration> WaitBudget` with its `TargetSessionAttributes` session demand instead of a parallel consistency vocabulary; `ReadPhase` is `Routed | Waited | Connected | Executed`, one registered checkpoint name per row beside the one `LanePivot` tag dimension the lane key fills.
- Entry: `Route` folds `ReadRequest` directly to its lane; `AwaitNonStale` consumes the lane-carried wait budget and the production `IProjectionDaemon.WaitForNonStaleData`, returning the measured elapsed wait; `Connect` resolves the lane's session demand off the one multihost source; `Observed` brackets one pooled `ILatencyContext` over the whole route-wait-execute run, stamping each `ReadPhase` checkpoint through its pre-resolved token, tagging the lane pivot, and draining the frozen `LatencyData` to the exporter before the release arm returns the context to its pool; `ReadLedger.Bind` resolves every token once at composition; `Measure` folds `EventStoreStatistics.EventSequenceNumber` against `ShardState.Sequence`, and its plural arm selects the worst shard; `public static IO<Fin<JsonElement>> ReflectedRead.Resolve(NpgsqlDataSource store, GraphQlDocument query, JsonElement variables, ProjectionContext frame)` runs ONE `graphql.resolve` call — the query document and its variables bind as parameters, the tenant GUC sets in-session so the identity tier's RLS partition applies, and the returned message envelope's `errors` array folds to the typed fault because the resolver never raises.
- Auto: an interactive-correctness query (clash narrow-phase, void-resolution, live QTO, containment ancestry) routes to the synchronous lane by construction so it reads the inline `GraphProjection` and QuikGraph view written in the append transaction, never a daemon-lagged async projection; an analytical query carries the `StalenessWatermark` so its consumer reads the lag; a re-run analytical clash demanding correctness from an async view calls `AwaitNonStale` first so the daemon catches up to the head before the read; the reflected door is the ZERO-RESOLVER web contract — `pg_graphql` reflects the live `element_identity`/`node_cell` schema (tables → object types, FKs → connection fields, comments → `@graphql` directives) into a Relay-paginated, introspectable GraphQL schema browser and mobile clients page through, recomputed lazily and DDL-invalidated by the extension's own event triggers, so a hand-written GraphQL schema or an out-of-process gateway beside the reflected one is the deleted form.
- Packages: Marten (`IProjectionDaemon.WaitForNonStaleData(TimeSpan)` the production non-stale block; `ShardState`/`ShardName`/`EventStoreStatistics`, `AdvancedOperations.FetchEventStoreStatistics`/`AllProjectionProgress`), Npgsql (`NpgsqlDataSource.CreateCommand`/`NpgsqlParameter` — the `graphql.resolve` door; `NpgsqlDbType.Jsonb`; `NpgsqlMultiHostDataSource.CreateConnection(TargetSessionAttributes)` — the lane-session multihost door, `LoadBalanceHosts` a provisioning-DSN fact), pg_graphql (`graphql.resolve(query, variables, operationName, extensions)` → `jsonb` per `api-pg-graphql` — server-side, no managed assembly), Microsoft.Extensions.Telemetry.Abstractions (`ILatencyContextProvider.CreateContext`, `ILatencyContextTokenIssuer.GetCheckpointToken`/`GetTagToken`, `ILatencyContext.AddCheckpoint`/`SetTag`/`Freeze`/`LatencyData`, `ILatencyDataExporter.ExportAsync` — the contract half an instrumented library binds; the `AddLatencyContext` activation and the `LatencyContextRegistrationOptions` name registration are AppHost composition surface), NodaTime (`Duration`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new read modality is one `ReadRequest` case and one generated `Route` arm; a new analytical wait posture is one `QueryLane` row value; a new timed phase is one `ReadPhase` row the registration projection and the ledger index both pick up with no record-site edit; a reflected-schema tuning is an `@graphql` comment directive riding the identity tier's reviewed generation DDL, never a resolver code path.
- Boundary: authoritative topology and containment stay synchronous and co-transactional (`C2`) — the inline `GraphProjection` in the write transaction, the in-process QuikGraph view — so a read-your-writes interactive query is correct by construction; that synchronous lane is NOT infallible, since the `Query/topology` `Traversals.Run` it binds returns `Fin<TopologyResult>` carrying the typed `TopologyFault` band, so a router consumer composes the topology `Fin` into its OWN result rather than assuming success and an absent-root containment query surfaces as an honest typed fault, never a silent empty result; AGE and DuckDB are ANALYTICAL ONLY with an explicit `StalenessWatermark`, and interactive-correctness queries block on `WaitForNonStaleData` and never route to an async projection without the wait — a clash reading a daemon-lagged AGE view is the deleted form, and the gate rides the production `IProjectionDaemon`, not a test-only symbol; staleness is a MEASURED sequence gap (`EventSequenceNumber` head against `ShardState.Sequence`), never `ShardState.Timestamp`, a daemon-side recording stamp (`DateTimeOffset.UtcNow` at row construction) that measures read-latency rather than producer-to-projection lag — a `Measure` returning `Duration.Zero` on a trailing shard is the illusory form this owner forbids; strong-consistency reads go through the inline projection and the synchronous topology, never the columnar aggregate, so the columnar lane stays the rollup/search lane and the topology lane the correctness lane; the reflected door executes wholly in-database over the RLS-guarded identity relations — AppHost hosts the web endpoint and maps its principal onto the tenant frame at the port boundary, Persistence owns only the parameterized `graphql.resolve` call, and the reflected mutation fields (`insertInto*/update*/deleteFrom*Collection`) are unexposed BY PRIVILEGE, not by prose — the resolve transaction pins `SET LOCAL ROLE` to the SELECT-only serving role (`ReflectedRead.ReadRole`, granted no INSERT/UPDATE/DELETE on any exposed relation), and pg_graphql reflects mutation fields only off writable relations, so schema reflection under the serving identity carries query fields alone; the identity tier's one write authority stays the `Element/graph#STORE_HOOKS` owner; the phase ledger owns read-phase timings and the returned watermark owns projection lag; the pooled context never escapes its bracket, because `LatencyData` projects its spans over backing the pool re-leases on release, and its names never spell a literal at either end since an unregistered name resolves to a positionless token whose writes drop with nothing raised.

```csharp
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.Latency;
using LanguageExt;
using Marten;
using Marten.Events.Daemon;
using Marten.Events.Projections;
using NetTopologySuite.Geometries;
using NodaTime;
using Npgsql;
using NpgsqlTypes;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Query;
using Thinktecture;
using Rasm.Persistence.Element;
using SetQuery = Rasm.Element.Query.Predicate<Rasm.Persistence.Query.SetPredicate>;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ---------------------------------------------------------------------------
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
    public static readonly QueryLane Reflected = new("reflected", None, TargetSessionAttributes.Primary);
    public Option<Duration> WaitBudget { get; }
    public TargetSessionAttributes Session { get; }
    private QueryLane(string key, Option<Duration> waitBudget, TargetSessionAttributes session) : this() {
        WaitBudget = waitBudget;
        Session = session;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReadPhase {
    public static readonly ReadPhase Routed = new("rasm.persistence.read.routed");
    public static readonly ReadPhase Waited = new("rasm.persistence.read.waited");
    public static readonly ReadPhase Connected = new("rasm.persistence.read.connected");
    public static readonly ReadPhase Executed = new("rasm.persistence.read.executed");

    public const string LanePivot = "rasm.persistence.read.lane";

    public static Seq<string> Names => toSeq(Items).Map(static row => row.Key);
}

[ValueObject<string>]
[ValidationError]
public readonly partial struct GraphQlDocument {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('\0')) { validationError = ValidationError.Create("<document>"); }
    }
}

public sealed record ReadLedger(FrozenDictionary<ReadPhase, CheckpointToken> Phases, TagToken Lane) {
    public static ReadLedger Bind(ILatencyContextTokenIssuer issuer) =>
        new(ReadPhase.Items.ToFrozenDictionary(static row => row, row => issuer.GetCheckpointToken(row.Key)),
            issuer.GetTagToken(ReadPhase.LanePivot));
}

public static class ReadRouter {
    public static QueryLane Route(ReadRequest request) => request.Switch(
        interactive: static _ => QueryLane.Topology,
        graphAnalytic: static _ => QueryLane.Cypher,
        retrieval: static _ => QueryLane.Retrieval,
        aggregate: static _ => QueryLane.Columnar,
        reuse: static _ => QueryLane.Cache,
        reflected: static _ => QueryLane.Reflected);

    public static NpgsqlConnection Connect(NpgsqlMultiHostDataSource store, QueryLane lane) => store.CreateConnection(lane.Session);

    public static IO<Duration> AwaitNonStale(IProjectionDaemon daemon, QueryLane lane) =>
        lane.WaitBudget.Match(
            Some: budget => HostEdge.CapturedIO(async _ => {
                long start = Stopwatch.GetTimestamp();
                await daemon.WaitForNonStaleData(budget.ToTimeSpan()).ConfigureAwait(false);
                return Fin<Duration>.Succ(Duration.FromTimeSpan(Stopwatch.GetElapsedTime(start)));
            }).Bind(IO.lift),
            None: static () => IO.pure(Duration.Zero));

    public static IO<T> Observed<T>(ILatencyContextProvider pool, ILatencyDataExporter drain, ReadLedger ledger,
                                    IProjectionDaemon daemon, ReadRequest request, Func<QueryLane, IO<T>> read) =>
        IO.lift(() => Try.lift(() => pool.CreateContext()).Run()).Bracket(
            Use: cell => Phased(cell, drain, ledger, daemon, Route(request), read),
            Fin: static cell => Captured(() => { cell.Dispose(); return unit; }));

    static IO<T> Phased<T>(ILatencyContext cell, ILatencyDataExporter drain, ReadLedger ledger,
                           IProjectionDaemon daemon, QueryLane lane, Func<QueryLane, IO<T>> read) =>
        from _pivot in Captured(() => { cell.SetTag(ledger.Lane, lane.Key); return unit; })
        from _route in Stamp(cell, ledger, ReadPhase.Routed)
        from _wait  in AwaitNonStale(daemon, lane).Bind(_ => Stamp(cell, ledger, ReadPhase.Waited))
        from value  in read(lane)
        from _done  in Stamp(cell, ledger, ReadPhase.Executed)
        from _drain in Sealed(cell, drain)
        select value;

    static IO<Unit> Stamp(ILatencyContext cell, ReadLedger ledger, ReadPhase phase) =>
        Captured(() => { cell.AddCheckpoint(ledger.Phases[phase]); return unit; });

    static IO<Unit> Sealed(ILatencyContext cell, ILatencyDataExporter drain) =>
        Captured(() => { cell.Freeze(); return unit; })
            .Bind(_ => HostEdge.CapturedIO(async _ => {
                await drain.ExportAsync(cell.LatencyData, CancellationToken.None).ConfigureAwait(false);
                return Fin<Unit>.Succ(unit);
            }).Bind(IO.lift));

    static IO<T> Captured<T>(Func<T> crossing) =>
        IO.lift(() => Try.lift(() => Fin<T>.Succ(crossing())).Run().Bind(static inner => inner));

    public static StalenessWatermark Measure(EventStoreStatistics head, ShardState projection) =>
        new(head.EventSequenceNumber, projection.Sequence);

    public static StalenessWatermark Measure(EventStoreStatistics head, Seq<ShardState> shards) =>
        shards.IsEmpty
            ? new StalenessWatermark(head.EventSequenceNumber, 0L)
            : shards.Map(state => Measure(head, state))
                .Fold(new StalenessWatermark(head.EventSequenceNumber, head.EventSequenceNumber),
                    static (worst, next) => next.Gap > worst.Gap ? next : worst);

    public static IO<StalenessWatermark> Measure(IDocumentStore store, ShardName shard) =>
        HostEdge.CapturedIO(async _ => {
            EventStoreStatistics stats = await store.Advanced.FetchEventStoreStatistics().ConfigureAwait(false);
            IReadOnlyList<ShardState> progress = await store.Advanced.AllProjectionProgress().ConfigureAwait(false);
            return Fin<StalenessWatermark>.Succ(toSeq(progress).Find(s => s.ShardName == shard.Identity).Match(
                Some: state => Measure(stats, state),
                None: () => new StalenessWatermark(stats.EventSequenceNumber, 0L)));
        }).Bind(IO.lift);
}

public static class ReflectedRead {
    const string ReadRole = "rasm_graphql_read";

    public static IO<Fin<JsonElement>> Resolve(NpgsqlDataSource store, GraphQlDocument query, JsonElement variables, Option<string> operation, ProjectionContext frame) =>
        HostEdge.CapturedIO(async token => {
            await using NpgsqlConnection lane = await store.OpenConnectionAsync(token).ConfigureAwait(false);
            await using NpgsqlTransaction scope = await lane.BeginTransactionAsync(token).ConfigureAwait(false);
            await using NpgsqlBatch batch = lane.CreateBatch();
            NpgsqlBatchCommand role = new($"SET LOCAL ROLE {ReadRole}");
            NpgsqlBatchCommand pin = new($"SELECT set_config('{TenantContext.TenantSlot}', @tenant, true)");
            _ = pin.Parameters.AddWithValue("tenant", frame.Tenant.Entry);
            NpgsqlBatchCommand door = new("SELECT graphql.resolve(@query, @variables, @operation, NULL)");
            _ = door.Parameters.AddWithValue("query", (string)query);
            door.Parameters.Add(new NpgsqlParameter("variables", NpgsqlDbType.Jsonb) { Value = variables.GetRawText() });
            _ = door.Parameters.AddWithValue("operation", operation.Match<object>(Some: static name => name, None: static () => DBNull.Value));
            batch.BatchCommands.Add(role);
            batch.BatchCommands.Add(pin);
            batch.BatchCommands.Add(door);
            await using NpgsqlDataReader evidence = await batch.ExecuteReaderAsync(token).ConfigureAwait(false);
            _ = await evidence.NextResultAsync(token).ConfigureAwait(false);
            _ = await evidence.NextResultAsync(token).ConfigureAwait(false);
            string envelope = await evidence.ReadAsync(token).ConfigureAwait(false) ? evidence.GetString(0) : "{}";
            await evidence.DisposeAsync().ConfigureAwait(false);
            await scope.CommitAsync(token).ConfigureAwait(false);
            using JsonDocument parsed = JsonDocument.Parse(envelope);
            return parsed.RootElement.TryGetProperty("errors", out JsonElement errors) && errors.GetArrayLength() > 0
                ? Fin<JsonElement>.Fail(new SelectionFault.Reflected(errors[0].GetRawText()))
                : Fin<JsonElement>.Succ(parsed.RootElement.TryGetProperty("data", out JsonElement data) ? data.Clone() : default);
        });
}
```

| [INDEX] | [POLICY]                | [VALUE]                                       | [BINDING]                                               |
| :-----: | :---------------------- | :-------------------------------------------- | :------------------------------------------------------ |
|  [01]   | interactive correctness | the synchronous `Topology` lane               | inline projection + QuikGraph; never an async view      |
|  [02]   | analytical              | the async `Columnar`/`Cypher` lane            | carries the `StalenessWatermark`                        |
|  [03]   | request routing         | one `ReadRequest` case                        | impossible combinations are absent                      |
|  [04]   | non-stale gate          | `IProjectionDaemon.WaitForNonStaleData`       | the production runner member; not `TestingExtensions`   |
|  [05]   | watermark               | `EventSequenceNumber` vs shard `Sequence`     | sequence evidence; no synthetic wall duration           |
|  [06]   | reflected door          | one `graphql.resolve` call, RLS tenant pinned | zero resolver code; errors message envelope folds typed |
|  [07]   | phase ledger            | one pooled `ILatencyContext` per read         | bracketed; frozen set drains before the pool release    |
|  [08]   | latency names           | the `ReadPhase` roster projection             | one vocabulary registers and records; no literal        |

## [03]-[ELEMENT_SET_ALGEBRA]

- Owner: `SetPredicate` the closed STORE leaf family every leaf lowers to one index predicate; `SetQuery` the algebra's `Predicate<TLeaf>` closure instantiated over it, carrying `All`/`Any`/`Not`/`Closure` and the `Open` vacuous conjunction the algebra names; `KeySelection` the content-certified selection wrapper over the algebra's `Selection<SetKey>`; `SetKey` the model-qualified member with the one cross-runtime total order; `SetScope` the caller-supplied model roster evaluation resolves across; `SetResolve` the two-port index-and-expansion contract; `[FaultCase]`/`SelectionFault` the closed admission band over the kernel `Fault` floor; `Selections` the static surface owning depth admission, operand admission, the frozen parity preimage, the set-valued pushdown fold, and the closure verdict the algebra's `Holds` takes.
- Cases: `Literal | Rule | Spatial | Cell | Jsonpath | Classification | Containment | Material | Exists | Raster` on `SetPredicate` (the bounded operator within each typed — `SpatialPredicate` on `Spatial`, `JsonComparison` on `Jsonpath`, the algebra's `WalkDepth` ring on `Cell`, `RasterPredicate` on `Raster`; `Containment` anchors on a model-qualified `SetKey`, `Literal` carries a member roster, `Rule` a typed `RuleId`); the boolean tree is the algebra's five arms and nothing local.
- Entry: `public static Fin<KeySelection> Selections.Evaluate(SetQuery query, SetScope scope, SetResolve resolve)` folds the algebra tree set-valued — every leaf through `SetResolve.Leaf`, `All` as intersect-then-subtract, `Any` as union, `Closure` as a bounded transitive walk over `SetResolve.Expand` — refusing an unbounded selection typed; `Selections.Depth(int)` runs the algebra's `WalkDepth` admission and re-keys its refusal onto this folder's band; `Selections.Operand(demanded, carried, row)` admits a bounded-operator row against the scalar its leaf carries; `Selections.Preimage(sortedKeys)` is the frozen parity byte shape; `Selections.Reached(candidate, scope, resolve)` is the closure verdict the algebra's `Predicate.Holds` takes so an in-memory verdict and a pushed-down selection read ONE closure law; `KeySelection.Of` is the sole mint and always certifies.
- Auto: a selection is the universal BIM currency — clash, IDS, MVD, QTO, and rule surfaces all consume and produce `KeySelection` values, so a clash result is a `KeySelection`, an IDS pass-set is a `KeySelection`, and a QTO subject is a `KeySelection`; `ContentKey` is `XxHash128` over the FRAMED distinct-sorted `SetKey` preimage (a LE `int32` key count, then per key the fixed-width 16-byte big-endian model bytes and an LE `int32` node byte length with its UTF8 bytes) so equal selections share one key; scope admission runs on EVERY leaf answer and every expansion ring rather than the literal roster alone, so a store leaf whose index row names an unscoped model and a `ModelLink` crossing out of the roster both yield `SelectionFault.Scope`; `Spatial` lowers to the GiST predicate the TYPED `SpatialPredicate` `.Key` (`ST_Intersects`/`ST_Within`/`ST_DWithin`/…) names so a typo is a missing vocabulary row at compile time rather than a silent sequential scan — the `Ranged` `ST_DWithin` row consumes the leaf's `Distance` radius through `Operand`, which refuses BOTH the demanded-and-absent and the undemanded-and-present corners so a row's declaration and its leaf's carriage must agree, `Cell` to the `h3-pg` grid-disk bucket predicate over the identity tier's cell column (`h3_grid_disk(anchor, k)` membership the cell index serves — the H3 counterpart of the `Spatial` GiST leaf, so a storey-band or proximity selection is index-served without a geometry decode), `Jsonpath` to a jsonb path predicate under the typed `JsonComparison` comparator, `Classification` to a tsvector/classification predicate, `Containment` to the containment-edge ancestry, `Material`/`Exists` to their jsonb existence forms, `Raster` to the `postgis_raster` in-db predicate the typed `RasterPredicate.Key` names — bare `ST_Intersects(rast, geom)` coverage membership, or the statistical `ST_SummaryStats(ST_Clip(rast, geom), band)` mean against the leaf `Threshold` — so a "sample the coverage under this footprint" selection pushes server-side onto the provisioned `postgis_raster` extension and never pays a full blob fetch and in-process decode (the extension's `Degradable` rank folds this leaf out at admission when the cluster lacks it); every bounded walk carries the algebra's `WalkDepth` — a raw `int` depth never crosses into the interior, so a negative bound is a typed `SelectionFault.Depth` at admission, never a silent empty selection the `<= depth` predicate fakes; the `Closure` arm is a GENUINE bounded transitive fold — it evaluates its `Seed` sub-expression then folds one-hop `Expand` waves accumulating the reachable frontier, halting at the FIXPOINT rather than at the bound so `WalkDepth.Whole` walks its actual waves instead of spinning `int.MaxValue` no-op iterations after the frontier empties.
- Law: a set-valued reading needs a BOUND, and the algebra's boolean closure admits three shapes that carry none — a bare `Not`, an `Any` holding a negated operand, and the `Open` vacuous conjunction. Each demands the complement of a set over a universe no scope materializes, so `Evaluate` yields `SelectionFault.Rejected` with the offending shape named. NAMED LOSS: the binary `Union`/`Intersect`/`Difference` arms made "exactly two operands" and "a complement always has a bounded left operand" structural, where the n-ary algebra arms make both a runtime verdict. WITNESS: `Difference(a, b)` is `a.AndNot(b)` = `All([a, Not(b)])`, whose `Split` seats `a` as the bound and `b` as the subtrahend, while `Not(b)` alone — previously unspellable — is one typed refusal at the fold instead of a whole-table read.
- Law: the boolean tree resolves to the ALGEBRA's ONE declaration and this owner mints none — the local `SetExpr` tree, its `WalkDepth` twin, and the `ElementSet` carrier all delete onto `Predicate<TLeaf>`, `WalkDepth`, and `Selection<TKey>`. NAMED LOSS: the `SelectionFault.Depth` band no longer lands at the bound's own construction, since the algebra type yields `ElementFault.ValueRejected`. WITNESS: `Selections.Depth(bound)` runs the algebra admission and re-keys its refusal onto `SelectionFault.Depth(bound)`, so the BOUND type is the algebra's and the BAND stays this folder's — one declaration corpus-wide, one telemetry vocabulary per package.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of` — the one federation hasher, seed-zero `XxHash128` value-identical; `FaultBand`/`[FaultCase]`/`Fault` the fault floor), Rasm.Element (`Query/predicate#PREDICATE_ALGEBRA` `Predicate<TLeaf>`/`Selection<TKey>`/`WalkDepth`/`MatchVerdict` — the algebra closure, result carrier, walk bound, and verdict), Rasm.Persistence (`Element/graph#STREAM_GRAIN` `ModelId` — the `SetKey` model half; `Element/identity#ELEMENT_IDENTITY` `H3Cell` — the `Cell` leaf anchor), System.Buffers (`ArrayBufferWriter`/`BinaryPrimitives`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NetTopologySuite, NodaTime, BCL inbox.
- Growth: a new selection primitive is one `SetPredicate` case with its lowering; a new spatial operator is one `SpatialPredicate` row, a new jsonb comparator one `JsonComparison` row; a new bounded walk consumes the ONE algebra `WalkDepth`, never a second depth carrier; a new boolean combinator lands at the ALGEBRA or nowhere; zero new surface — a per-discipline selection class, a saved-search table, a string-query DSL, a raw-string leaf, or a free-string operator on a typed leaf is the deleted form because the closure is the algebra's one tree the planner lowers, every leaf predicate is a typed case, and every bounded operator within a leaf is a vocabulary row.
- Boundary: `KeySelection` is the one composable currency — every analysis surface takes a `KeySelection` and yields a `KeySelection` so results compose (a clash result intersected with a classification selection is one `All`, never a join in application code); `ContentKey` is stable across runs, peers, and tenants because it hashes the length-framed distinct-sorted preimage; the private mint makes certification TOTAL, so `default` cannot carry an invalid zero key; `Selection<SetKey>` is the projection a peer folder reads and it carries `Some` because this owner minted it — the algebra's own `Union`/`Intersect`/`Except` derivations answer `None` by the algebra's law and re-enter here only through `Of`, which re-frames and re-certifies; the `Closure` combinator is a real bounded transitive fold whose one-hop `Expand` is the `Query/topology#GRAPH_TOPOLOGY` incidence neighbour over the element graph (the reachability owner stays the graph/topology owner, the bounded fold stays here), NEVER the `Version/ledger#CHANGEFEED` `Closure` — that ledger manifest is a representation-content-hash blob-transfer set keyed by `UInt128`, a DIFFERENT closure that cannot answer a `NodeId` reachability selection, so conflating the two is the deleted altitude error; evaluation authority stays with THIS folder because the algebra is host-neutral vocabulary — `Predicate.Holds` is the algebra's per-candidate structural fold and `Selections.Reached` supplies its closure verdict, so an in-memory verdict and a pushed-down selection answer the same walk rather than two; selection evaluation pushes through the lane router so a `Spatial` leaf executes on the GiST index and a `Jsonpath` leaf on the jsonb index in the store, never client-side; scope is caller DATA — evaluation takes the `SetScope` roster as a value and reads no project rollup of its own, so read-your-writes holds per model and the async `ProjectGraph` roster is one legitimate supplier of a scope, never the evaluator's own read; a federated selection spans separate model streams WITHOUT minting a union graph — the algebra's `Federate` union stays the materialized-coordination path under its one header, and neither substitutes for the other; the `KeySelection.Preimage` framed byte shape — fixed-width big-endian model bytes beside the length-framed node text under the `SetKey` order — is what the `Version/commits#CRDT_WIRE` `ContentParityCorpus.Contribute(ParitySlot.ElementSet, selection.Preimage)` freezes as the `elementset` parity vector (CONTRIBUTED by this owner, never reverse-imported into the Version owner), the SLOT LABEL staying `elementset` because it is the cross-runtime corpus name the python and TypeScript ends bind, and a membership or framing change re-cuts that vector in the same pass.

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JsonComparison {
    public static readonly JsonComparison Exists = new("exists");
    public static readonly JsonComparison Eq = new("eq");
    public static readonly JsonComparison Contains = new("contains");
    public static readonly JsonComparison GreaterThan = new("gt");
    public static readonly JsonComparison GreaterOrEqual = new("gte");
    public static readonly JsonComparison LessThan = new("lt");
    public static readonly JsonComparison LessOrEqual = new("lte");
    public static readonly JsonComparison Matches = new("matches");
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SelectionFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Selection;
    private SelectionFault() { }

    [FaultCase(0)]
    public sealed partial record Depth(int Found) : SelectionFault();
    [FaultCase(1)]
    public sealed partial record Rejected(string Detail) : SelectionFault();
    [FaultCase(2)]
    public sealed partial record Reflected(string Detail) : SelectionFault();
    [FaultCase(3)]
    public sealed partial record Scope(string Detail) : SelectionFault();

    public override string Message => Switch(
        depth:     static c => string.Create(CultureInfo.InvariantCulture, $"<selection-depth:{c.Found}>"),
        rejected:  static c => $"<selection-rejected:{c.Detail}>",
        reflected: static c => $"<selection-reflected:{c.Detail}>",
        scope:     static c => $"<selection-scope:{c.Detail}>");
}

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError]
public readonly partial struct SetPath {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        if (value.Length == 0 || value.Contains('\0')) {
            validationError = ValidationError.Create("<set-path>");
        }
    }
}

[ValueObject<string>]
[ValidationError]
public readonly partial struct RuleId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        if (value.Length == 0 || value.Contains('\0')) {
            validationError = ValidationError.Create("<rule-id>");
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpatialPredicate {
    public static readonly SpatialPredicate Intersects = new("ST_Intersects", argued: false);
    public static readonly SpatialPredicate Contains = new("ST_Contains", argued: false);
    public static readonly SpatialPredicate Within = new("ST_Within", argued: false);
    public static readonly SpatialPredicate DWithin = new("ST_DWithin", argued: true);
    public static readonly SpatialPredicate Overlaps = new("ST_Overlaps", argued: false);
    public static readonly SpatialPredicate Touches = new("ST_Touches", argued: false);
    public static readonly SpatialPredicate Covers = new("ST_Covers", argued: false);
    public static readonly SpatialPredicate CoveredBy = new("ST_CoveredBy", argued: false);
    public bool Argued { get; }
    private SpatialPredicate(string key, bool argued) : this(key) => Argued = argued;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RasterPredicate {
    public static readonly RasterPredicate Intersects = new("ST_Intersects", argued: false);
    public static readonly RasterPredicate MeanAbove = new("ST_SummaryStats", argued: true);
    public static readonly RasterPredicate MeanBelow = new("ST_SummaryStats", argued: true);
    public bool Argued { get; }
    private RasterPredicate(string key, bool argued) : this(key) => Argued = argued;
}

public readonly record struct SetKey(ModelId Model, NodeId Node) : IComparable<SetKey> {
    public int CompareTo(SetKey other) {
        Span<byte> mine = stackalloc byte[16];
        Span<byte> theirs = stackalloc byte[16];
        Model.Value.TryWriteBytes(mine, bigEndian: true, out _);
        other.Model.Value.TryWriteBytes(theirs, bigEndian: true, out _);
        int byModel = mine.SequenceCompareTo(theirs);
        return byModel != 0 ? byModel : string.CompareOrdinal(Node.ToValue(), other.Node.ToValue());
    }
}

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
    public sealed record Literal(Seq<SetKey> Keys) : SetPredicate;
    public sealed record Rule(RuleId Id) : SetPredicate;
    public sealed record Spatial(SpatialPredicate Op, Geometry Operand, Option<double> Distance) : SetPredicate;
    public sealed record Cell(H3Cell Anchor, WalkDepth Ring) : SetPredicate;
    public sealed record Jsonpath(SetPath Path, JsonComparison Cmp, Option<string> Value) : SetPredicate;
    public sealed record Classification(SetPath SystemPath, Option<string> Value) : SetPredicate;
    public sealed record Containment(SetKey Ancestor, bool Subtree) : SetPredicate;
    public sealed record Material(Option<string> Value) : SetPredicate;
    public sealed record Exists(SetPath Path) : SetPredicate;
    public sealed record Raster(RasterPredicate Op, string Coverage, int Band, Option<double> Threshold) : SetPredicate;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record KeySelection {
    private KeySelection(Seq<SetKey> keys, SetScope scope, UInt128 contentKey, ReadOnlyMemory<byte> preimage) =>
        (Keys, Scope, ContentKey, Preimage) = (keys, scope, contentKey, preimage);

    public Seq<SetKey> Keys { get; }
    public SetScope Scope { get; }
    public UInt128 ContentKey { get; }
    public ReadOnlyMemory<byte> Preimage { get; }
    public int Count => Keys.Count;

    public Selection<SetKey> Members => new(Keys, Some(ContentKey));

    public static KeySelection Empty(SetScope scope) => Of(Seq<SetKey>(), scope);

    public static KeySelection Of(Seq<SetKey> keys, SetScope scope) {
        Seq<SetKey> sorted = toSeq(keys.Distinct().OrderBy(static key => key));
        ReadOnlyMemory<byte> preimage = Selections.Preimage(sorted);
        return new KeySelection(sorted, scope, ContentHash.Of(preimage.Span), preimage);
    }

    public static KeySelection Of(Selection<SetKey> members, SetScope scope) => Of(members.Keys, scope);
}

public readonly record struct SetResolve(
    Func<SetPredicate, SetScope, Fin<Seq<SetKey>>> Leaf,
    Func<Seq<SetKey>, Fin<Seq<SetKey>>> Expand);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Selections {
    public static ReadOnlyMemory<byte> Preimage(Seq<SetKey> sortedKeys) {
        ArrayBufferWriter<byte> buffer = new();
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4), sortedKeys.Count);
        buffer.Advance(4);
        foreach (SetKey key in sortedKeys) {
            key.Model.Value.TryWriteBytes(buffer.GetSpan(16), bigEndian: true, out _);
            buffer.Advance(16);
            int bytes = Encoding.UTF8.GetByteCount(key.Node.ToValue());
            BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4), bytes);
            buffer.Advance(4);
            Encoding.UTF8.GetBytes(key.Node.ToValue(), buffer.GetSpan(bytes));
            buffer.Advance(bytes);
        }
        return buffer.WrittenMemory;
    }

    public static Fin<WalkDepth> Depth(int bound) =>
        FactoryBridge.Accept<WalkDepth>(bound).MapFail(_ => new SelectionFault.Depth(bound));

    public static Fin<Option<double>> Operand(bool argued, Option<double> carried, string row) =>
        argued == carried.IsSome
            ? Fin.Succ(carried)
            : Fin.Fail<Option<double>>(new SelectionFault.Rejected(argued ? $"<operand-absent:{row}>" : $"<operand-unexpected:{row}>"));

    public static Fin<KeySelection> Evaluate(SetQuery query, SetScope scope, SetResolve resolve) => query.Switch(
        (Scope: scope, Resolve: resolve),
        leaf:    static (s, node) => Leafed(node.Value, s.Scope, s.Resolve),
        all:     static (s, node) => Conjoined(node.Operands, s.Scope, s.Resolve),
        any:     static (s, node) => Disjoined(node.Operands, s.Scope, s.Resolve),
        not:     static (_, _) => Fin.Fail<KeySelection>(new SelectionFault.Rejected("<unbounded:complement>")),
        closure: static (s, node) => Evaluate(node.Seed, s.Scope, s.Resolve)
            .Bind(seed => Walked(seed.Keys, seed.Keys, node.Depth.ToValue(), s.Scope, s.Resolve.Expand))
            .Map(reached => KeySelection.Of(reached, s.Scope)));

    static Fin<KeySelection> Leafed(SetPredicate leaf, SetScope scope, SetResolve resolve) =>
        resolve.Leaf(leaf, scope).Bind(keys => Scoped(keys, scope)).Map(keys => KeySelection.Of(keys, scope));

    static Fin<Seq<SetKey>> Scoped(Seq<SetKey> keys, SetScope scope) =>
        keys.Find(key => !scope.Admits(key.Model)).Match(
            Some: foreign => Fin.Fail<Seq<SetKey>>(new SelectionFault.Scope($"<leaf-model:{foreign.Model.Value}>")),
            None: () => Fin.Succ(keys));

    static (Seq<SetQuery> Held, Seq<SetQuery> Cut) Split(Seq<SetQuery> operands) =>
        operands.Fold((Held: Seq<SetQuery>(), Cut: Seq<SetQuery>()),
            static (parts, operand) => operand is SetQuery.Not negated
                ? (parts.Held, parts.Cut.Add(negated.Operand))
                : (parts.Held.Add(operand), parts.Cut));

    static Fin<KeySelection> Conjoined(Seq<SetQuery> operands, SetScope scope, SetResolve resolve) {
        (Seq<SetQuery> held, Seq<SetQuery> cut) = Split(operands);
        return from met in held.Fold(Fin.Succ(Option<Seq<SetKey>>.None), (acc, operand) =>
                   from carried in acc
                   from one in Evaluate(operand, scope, resolve)
                   select Some(carried.Match(Some: keys => toSeq(keys.Intersect(one.Keys)), None: () => one.Keys)))
               from bound in met.ToFin(new SelectionFault.Rejected(cut.IsEmpty ? "<unbounded:open>" : "<unbounded:complement>"))
               from kept in cut.Fold(Fin.Succ(bound), (acc, operand) =>
                   from carried in acc
                   from one in Evaluate(operand, scope, resolve)
                   select toSeq(carried.Except(one.Keys)))
               select KeySelection.Of(kept, scope);
    }

    static Fin<KeySelection> Disjoined(Seq<SetQuery> operands, SetScope scope, SetResolve resolve) {
        (Seq<SetQuery> held, Seq<SetQuery> cut) = Split(operands);
        return cut.IsEmpty
            ? held.TraverseM(operand => Evaluate(operand, scope, resolve).Map(static one => one.Keys)).As()
                .Map(rows => KeySelection.Of(rows.Bind(identity), scope))
            : Fin.Fail<KeySelection>(new SelectionFault.Rejected("<unbounded:complement>"));
    }

    static Fin<Seq<SetKey>> Walked(Seq<SetKey> reached, Seq<SetKey> frontier, int waves, SetScope scope,
                                   Func<Seq<SetKey>, Fin<Seq<SetKey>>> expand) =>
        waves <= 0 || frontier.IsEmpty
            ? Fin.Succ(reached)
            : expand(frontier)
                .Bind(found => Scoped(toSeq(found.Except(reached)), scope))
                .Bind(ring => Walked(reached + ring, ring, waves - 1, scope, expand));

    public static Func<SetQuery.Closure, MatchVerdict> Reached(SetKey candidate, SetScope scope, SetResolve resolve) =>
        walk => Evaluate(walk, scope, resolve).Match(
            Succ: reach => MatchVerdict.Of(reach.Keys.Contains(candidate)),
            Fail: fault => MatchVerdict.Fault(fault));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                                | [BINDING]                                              |
| :-----: | :----------------- | :----------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | selection currency | `KeySelection` in and out                              | every analysis surface composes; never an app join     |
|  [02]   | boolean closure    | the algebra `Predicate<SetPredicate>`                  | one tree corpus-wide; no local tree family             |
|  [03]   | membership         | `SetKey` — `(ModelId, NodeId)` under one byte order    | federation-altitude; comparator is cross-runtime       |
|  [04]   | scope              | caller `SetScope`, admitted per leaf                   | data, never an evaluator-side async roster read        |
|  [05]   | content key        | `ContentHash.Of` over the framed preimage              | total by private mint; reuse key + parity preimage     |
|  [06]   | typed leaves       | `SetPredicate` + `SpatialPredicate` operator rows      | no raw-string leaf; lowered to a store index           |
|  [07]   | closure            | bounded transitive fold, halting at fixpoint           | one-hop `Expand` is `Query/topology`; not the manifest |
|  [08]   | bounded depth      | algebra `WalkDepth` re-keyed to `SelectionFault.Depth` | closure/cell/topology/cypher share ONE axis            |
|  [09]   | unbounded shapes   | bare `Not`, negated `Any`, `Open` → refusal            | no whole-scope scan; the refusal names the shape       |
|  [10]   | cell leaf          | `Cell(H3Cell, WalkDepth)` grid-disk predicate          | `h3-pg` index-served; the H3 sibling of the GiST leaf  |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
