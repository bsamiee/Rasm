# [PERSISTENCE_QUERY_LANE]

Rasm.Persistence routes every read by its consistency demand: interactive-correctness queries (clash, void-resolution, live QTO, containment) bind the synchronous authoritative lane — the inline `Element/graph` `GraphProjection` and the in-process `Query/topology` QuikGraph view — while analytical queries (aggregation, search, columnar rollup) bind the async watermarked columnar and cypher lanes. A query demanding correctness from an async view blocks on the projection daemon's non-stale wait before reading, so a read-your-writes interactive query is correct by construction and never touches a daemon-lagged projection.

`QueryLane` is the lane axis carrying each lane's wait policy; `ReadRequest` discriminates correctness and query modality without boolean products; `StalenessWatermark` measures projection lag as the event-log head sequence against the daemon shard's high-water mark. `ElementSet` is the universal content-addressed selection currency every clash/IDS/MVD/QTO surface consumes and produces, `SetExpr` its selection-tree algebra and `SetPredicate` its closed typed leaf algebra, and `Closure` folds a bounded transitive walk over the `Query/topology` incidence. A retrieval-shaped read routes to the `Query/retrieval` fusion lane, which read-through-caches on the `ElementSet.Receipt` this owner mints. `NodeId`/`ElementGraph` arrive from `Rasm.Element`, and the inline projection and analytical lanes arrive from their owners.

## [01]-[INDEX]

- [01]-[READ_ROUTING]: the consistency-demand routing law, the lane axis, the staleness watermark, and the daemon non-stale wait gate.
- [02]-[ELEMENT_SET_ALGEBRA]: the composable content-addressed selection currency, the typed leaf algebra, and the stable receipt fold.

## [02]-[READ_ROUTING]

- Owner: `QueryLane` carries the composition-time wait policy; `ReadRequest` is the closed correctness/modality discriminant; `StalenessWatermark` is measured sequence evidence; `ReadRouter` owns routing, non-stale admission, and daemon-fan measurement; `GraphQlDocument` admits the web-native query document and `ReflectedRead` owns the in-database `graphql.resolve` door over the RLS-guarded identity relations.
- Cases: `ReadRequest` is `Interactive | GraphAnalytic | Retrieval | Aggregate | Reuse | Reflected`; `QueryLane` is `Topology | Columnar | Cypher | Retrieval | Cache | Reflected`, and each row carries `Option<Duration> WaitBudget` instead of a parallel consistency vocabulary.
- Entry: `Route` folds `ReadRequest` directly to its lane; `AwaitNonStale` consumes the lane-carried wait budget and the production `IProjectionDaemon.WaitForNonStaleData`; `Measure` folds `EventStoreStatistics.EventSequenceNumber` against `ShardState.Sequence`, and its plural arm selects the worst shard; `public static IO<Fin<JsonElement>> ReflectedRead.Resolve(NpgsqlDataSource store, GraphQlDocument query, JsonElement variables, ProjectionContext frame)` runs ONE `graphql.resolve` call — the query document and its variables bind as parameters, the tenant GUC sets in-session so the identity tier's RLS partition applies, and the returned envelope's `errors` array folds to the typed fault because the resolver never raises.
- Auto: an interactive-correctness query (clash narrow-phase, void-resolution, live QTO, containment ancestry) routes to the synchronous lane by construction so it reads the inline `GraphProjection` and QuikGraph view written in the append transaction, never a daemon-lagged async projection; an analytical query carries the `StalenessWatermark` so its consumer reads the lag; a re-run analytical clash demanding correctness from an async view calls `AwaitNonStale` first so the daemon catches up to the head before the read; the reflected door is the ZERO-RESOLVER web contract — `pg_graphql` reflects the live `element_identity`/`node_cell` schema (tables → object types, FKs → connection fields, comments → `@graphql` directives) into a Relay-paginated, introspectable GraphQL schema browser and mobile clients page through, recomputed lazily and DDL-invalidated by the extension's own event triggers, so a hand-written GraphQL schema or an out-of-process gateway beside the reflected one is the deleted form.
- Receipt: a routed read rides `store.query.route` carrying the demand and the lane; an async-stale wait rides `store.query.wait` carrying the watermark and the elapsed wait; a reflected read rides `store.query.reflected` carrying the operation name and the envelope's error count.
- Packages: Marten (`IProjectionDaemon.WaitForNonStaleData(TimeSpan)` the production non-stale block; `ShardState`/`ShardName`/`EventStoreStatistics`, `AdvancedOperations.FetchEventStoreStatistics`/`AllProjectionProgress`), Npgsql (`NpgsqlDataSource.CreateCommand`/`NpgsqlParameter` — the `graphql.resolve` door; `NpgsqlDbType.Jsonb`), pg_graphql (`graphql.resolve(query, variables, operationName, extensions)` → `jsonb` per `api-pg-graphql` — server-side, no managed assembly), NodaTime (`Duration`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new read modality is one `ReadRequest` case and one generated `Route` arm; a new analytical wait posture is one `QueryLane` row value; a reflected-schema tuning is an `@graphql` comment directive riding the identity tier's reviewed-migration DDL, never a resolver code path.
- Boundary: authoritative topology and containment stay synchronous and co-transactional (`C2`) — the inline `GraphProjection` in the write transaction, the in-process QuikGraph view — so a read-your-writes interactive query is correct by construction; that synchronous lane is NOT infallible, since the `Query/topology` `Traversals.Run` it binds returns `Fin<TopologyResult>` railing the typed `TopologyFault` band, so a router consumer composes the topology `Fin` into its OWN rail rather than assuming success and an absent-root containment query surfaces as an honest typed fault, never a silent empty result; AGE and DuckDB are ANALYTICAL ONLY with an explicit `StalenessWatermark`, and interactive-correctness queries block on `WaitForNonStaleData` and never route to an async projection without the wait — a clash reading a daemon-lagged AGE view is the deleted form, and the gate rides the production `IProjectionDaemon`, not a test-only symbol; staleness is a MEASURED sequence gap (`EventSequenceNumber` head against `ShardState.Sequence`), never `ShardState.Timestamp`, a daemon-side recording stamp (`DateTimeOffset.UtcNow` at row construction) that measures read-latency rather than producer-to-projection lag — a `Measure` returning `Duration.Zero` on a trailing shard is the illusory form this owner forbids; strong-consistency reads go through the inline projection and the synchronous topology, never the columnar aggregate, so the columnar lane stays the rollup/search lane and the topology lane the correctness lane; the reflected door executes wholly in-database over the RLS-guarded identity relations — AppHost hosts the web endpoint and maps its principal onto the tenant frame at the port boundary, Persistence owns only the parameterized `graphql.resolve` call, and the reflected mutation fields (`insertInto*/update*/deleteFrom*Collection`) are unexposed BY PRIVILEGE, not by prose — the resolve transaction pins `SET LOCAL ROLE` to the SELECT-only serving role (`ReflectedRead.ReadRole`, granted no INSERT/UPDATE/DELETE on any exposed relation), and pg_graphql reflects mutation fields only off writable relations, so schema reflection under the serving identity carries query fields alone; the identity tier's one write authority stays the `Element/graph#STORE_RAIL` rail.

```csharp signature
using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Text.Json;
using LanguageExt;
using Marten;
using Marten.Events.Daemon;
using Marten.Events.Projections;
using NetTopologySuite.Geometries;
using NodaTime;
using Npgsql;
using NpgsqlTypes;
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
    public static readonly QueryLane Topology = new("topology", None);
    public static readonly QueryLane Columnar = new("columnar", Some(Duration.FromSeconds(5)));
    public static readonly QueryLane Cypher = new("cypher", Some(Duration.FromSeconds(5)));
    public static readonly QueryLane Retrieval = new("retrieval", Some(Duration.FromSeconds(5)));
    public static readonly QueryLane Cache = new("cache", None);
    // Reflected reads hit the transactionally-current identity relations; no daemon, no wait budget.
    public static readonly QueryLane Reflected = new("reflected", None);
    public Option<Duration> WaitBudget { get; }
    private QueryLane(string key, Option<Duration> waitBudget) : this(key) => WaitBudget = waitBudget;
}

// The web-native query document: non-empty, NUL-free, bound as a parameter — never concatenated.
[ValueObject<string>]
[ValidationError<SelectionFault>]
public readonly partial struct GraphQlDocument {
    static partial void ValidateFactoryArguments(ref SelectionFault? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('\0')) { validationError = new SelectionFault.Reflected("<document>"); }
    }
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

    public static IO<Unit> AwaitNonStale(IProjectionDaemon daemon, QueryLane lane) =>
        lane.WaitBudget.Match(
            Some: budget => IO.liftAsync(async () => { await daemon.WaitForNonStaleData(budget.ToTimeSpan()).ConfigureAwait(false); return unit; }),
            None: static () => IO.pure(unit));

    // `ShardState.Timestamp` is daemon observation time; only event and shard sequences measure projection progress.
    public static StalenessWatermark Measure(EventStoreStatistics head, ShardState projection) =>
        new(head.EventSequenceNumber, projection.Sequence);

    // Daemon-fan staleness is the maximum shard gap; averaging masks stragglers.
    // An empty fan projects sequence zero because no shard has advanced.
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

// The reflected door: one transaction pins the tenant GUC (RLS partition) and resolves the whole
// GraphQL operation in-database; the resolver never raises, so the errors envelope folds typed.
public static class ReflectedRead {
    // The SELECT-only serving role: reflection exposes mutation fields only off relations the executing role can
    // write, so the privilege pin IS the mutation gate — RLS partitions rows, the role removes the write surface.
    const string ReadRole = "rasm_graphql_read";

    public static IO<Fin<JsonElement>> Resolve(NpgsqlDataSource store, GraphQlDocument query, JsonElement variables, Option<string> operation, ProjectionContext frame) =>
        IO.liftAsync(async () => {
            await using NpgsqlConnection lane = await store.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlTransaction scope = await lane.BeginTransactionAsync().ConfigureAwait(false);
            try {
                await using NpgsqlBatch batch = lane.CreateBatch();
                NpgsqlBatchCommand role = new($"SET LOCAL ROLE {ReadRole}");
                NpgsqlBatchCommand pin = new("SELECT set_config('rasm.tenant', @tenant, true)");
                _ = pin.Parameters.AddWithValue("tenant", frame.Tenant.ToString("x32", CultureInfo.InvariantCulture));
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

| [INDEX] | [POLICY]                | [VALUE]                                             | [BINDING]                                               |
| :-----: | :---------------------- | :-------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | interactive correctness | the synchronous `Topology` lane                     | inline projection + QuikGraph; never an async view      |
|  [02]   | analytical              | the async `Columnar`/`Cypher` lane                  | carries the `StalenessWatermark`                        |
|  [03]   | request routing         | one `ReadRequest` case                              | impossible combinations are absent                     |
|  [04]   | non-stale gate          | `IProjectionDaemon.WaitForNonStaleData`             | the production runner member; not `TestingExtensions`   |
|  [05]   | watermark               | `EventSequenceNumber` vs shard `Sequence`           | sequence evidence; no synthetic wall duration           |
|  [06]   | reflected door          | one `graphql.resolve` call, RLS tenant pinned       | zero resolver code; errors envelope folds typed         |

## [03]-[ELEMENT_SET_ALGEBRA]

- Owner: `ElementSet` the polymorphic composable selection record carrying a stable content-addressed receipt; `SetPredicate` the closed leaf-predicate algebra; `SetExpr` the selection-tree algebra; `WalkDepth` the admitted bounded-depth `[ValueObject<int>]` every bounded walk carries — the `Closure` fold, the `Cell` ring, the `Query/topology` `Ancestry`/`Descent`, the `Query/cypher` `Reach` hops all consume this ONE axis; `SelectionFault` the closed admission band (846x off the `Element/graph#FAULT_TABLES` registry) an invalid bound rails; `ElementSetAlgebra` the static surface owning literal selection, the boolean/spatial/cell/property/classification combinators, and the stable-receipt fold.
- Cases: `Spatial | Cell | Jsonpath | Classification | Containment | Material | Exists` on `SetPredicate` (the bounded operator within each typed — `SpatialOp` on `Spatial`, `JsonComparison` on `Jsonpath`, the admitted `WalkDepth` ring on `Cell`); `Literal | Predicate | ByRule | Union | Intersect | Difference | Closure` on `SetExpr`.
- Entry: `public static Fin<ElementSet> Evaluate(SetExpr expr, SetResolve resolve)` aborts on an index or expansion failure and otherwise folds the expression tree into a stable key set; `Receipt` derives the content-addressed set identity over the length-framed distinct-sorted preimage; `Canonical` is the preimage the parity corpus freezes.
- Auto: an element set is the universal BIM currency — clash, IDS, MVD, QTO, and rule surfaces all consume and produce `ElementSet` values, so a clash result is an `ElementSet`, an IDS pass-set is an `ElementSet`, and a QTO subject is an `ElementSet`; the set receipt is `XxHash128` over the LENGTH-FRAMED distinct-sorted `NodeId` preimage (a LE `int32` key count, then per key a LE `int32` byte length and its UTF8 bytes) so two selections yielding the same elements share one receipt AND two different key sets can never collide on an unframed concatenation; the boolean combinators fold over evaluated leaf sets, and the one `Predicate` leaf carries a `SetPredicate` — `Spatial` lowers to the GiST predicate the TYPED `SpatialOp` `.Key` (`ST_Intersects`/`ST_Within`/`ST_DWithin`/…) names so a typo is a missing vocabulary row at compile time rather than a silent sequential scan — the `Ranged` `ST_DWithin` row consumes the leaf's `Distance` radius, and a ranged op without `Some` rails `SelectionFault.Rejected` at leaf lowering rather than lowering a two-argument call the server rejects, `Cell` to the `h3-pg` grid-disk bucket predicate over the identity tier's cell column (`h3_grid_disk(anchor, k)` membership the cell index serves — the H3 counterpart of the `Spatial` GiST leaf, so a storey-band or proximity selection is index-served without a geometry decode), `Jsonpath` to a jsonb path predicate under the typed `JsonComparison` comparator, `Classification` to a tsvector/classification predicate, `Containment` to the containment-edge ancestry, `Material`/`Exists` to their jsonb existence forms; every bounded walk carries the admitted `WalkDepth` — a raw `int` depth never crosses into the interior, so a negative bound is a typed `SelectionFault.Depth` at admission, never a silent empty selection the `<= depth` predicate fakes; the `Closure` arm is a GENUINE bounded transitive fold — it evaluates its `Seed` sub-expression then folds `Depth` one-hop `Expand` waves accumulating the reachable frontier to its fixpoint, never an opaque leaf identical to `Predicate`.
- Receipt: an evaluation rides `store.elementset.eval` carrying the leaf count and the result cardinality; the stable receipt is the reuse key the `Query/retrieval#FUSION_AND_REUSE` read-through caches on.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of` — the one federation hasher, seed-zero `XxHash128` value-identical; `Expected` the band base), Rasm.Persistence (`Element/identity#ELEMENT_IDENTITY` `H3Cell` — the `Cell` leaf anchor; `Element/graph#FAULT_TABLES` `FaultBand` — the `Selection` band registry row), System.Buffers (`ArrayBufferWriter`/`BinaryPrimitives`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NetTopologySuite, NodaTime, BCL inbox.
- Growth: a new selection primitive is one `SetPredicate` case (lowered by the `Predicate` leaf) or one `SetExpr` tree case; a new spatial operator is one `SpatialOp` row, a new jsonb comparator one `JsonComparison` row; a new bounded walk consumes the ONE `WalkDepth` admission, never a second depth carrier; a new combinator is one fold arm; zero new surface — a per-discipline selection class, a saved-search table, a string-query DSL, a raw-string leaf, or a free-string operator on a typed leaf is the deleted form because the algebra is one composable tree the planner lowers, every leaf predicate is a typed case, and every bounded operator within a leaf is a vocabulary row.
- Boundary: `ElementSet` is the one composable currency — every analysis surface takes an `ElementSet` and yields an `ElementSet` so results compose (a clash result intersected with a classification selection is one `SetExpr.Intersect`, never a join in application code); the receipt is content-addressed over the length-framed distinct-sorted preimage so it is stable across runs, peers, and tenants AND unambiguous — a positional or timestamp-keyed selection id, or an unframed byte concatenation two key sets collide on, is the deleted form; the `Closure` combinator is a real bounded transitive fold whose one-hop `Expand` is the `Query/topology#GRAPH_TOPOLOGY` incidence neighbour over the seam graph (the reachability owner stays the graph/topology owner, the bounded fold stays here), NEVER the `Version/ledger#CHANGEFEED` `Closure` — that ledger manifest is a representation-content-hash blob-transfer set keyed by `UInt128`, a DIFFERENT closure that cannot answer a `NodeId` reachability selection, so conflating the two is the deleted altitude error; every leaf predicate is a typed `SetPredicate` case and every bounded operator within it is a vocabulary row — the spatial operator is a `SpatialOp` smart-enum, the jsonb comparator a `JsonComparison` smart-enum — so a selection that promised a spatial intersection carries the typed `ST_*` operator the GiST index serves and the geometry, never a free string a typo degrades to a scan; selection evaluation pushes through the lane router so a `Spatial` leaf executes on the GiST index and a `Jsonpath` leaf on the jsonb index in the store, never client-side; the `ElementSet.Preimage` length-framed byte shape is what the `Version/commits#CRDT_WIRE` `ContentParityCorpus.Contribute(ParitySlot.ElementSet, set.Preimage)` freezes as the `elementset` parity vector (CONTRIBUTED by this owner, never reverse-imported into the Version owner).

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

    public override int Code => FaultBand.Selection + Switch(
        depth:     static _ => 0,
        rejected:  static _ => 1,
        reflected: static _ => 2);

    public override string Message => Switch(
        depth:     static c => $"<selection-depth:{c.Found}>",
        rejected:  static c => $"<selection-rejected:{c.Detail}>",
        reflected: static c => $"<selection-reflected:{c.Detail}>");

    public override string Category => Switch(
        depth:     static _ => "Depth",
        rejected:  static _ => "Rejected",
        reflected: static _ => "Reflected");

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SetPredicate {
    private SetPredicate() { }
    public sealed record Spatial(SpatialOp Op, Geometry Operand, Option<double> Distance) : SetPredicate;
    public sealed record Cell(H3Cell Anchor, WalkDepth Ring) : SetPredicate;
    public sealed record Jsonpath(SetPath Path, JsonComparison Cmp, Option<string> Value) : SetPredicate;
    public sealed record Classification(SetPath SystemPath, Option<string> Value) : SetPredicate;
    public sealed record Containment(NodeId Ancestor, bool Subtree) : SetPredicate;
    public sealed record Material(Option<string> Value) : SetPredicate;
    public sealed record Exists(SetPath Path) : SetPredicate;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SetExpr {
    private SetExpr() { }
    public sealed record Literal(Seq<NodeId> Keys) : SetExpr;
    public sealed record Predicate(SetPredicate Leaf) : SetExpr;
    public sealed record ByRule(string RuleId) : SetExpr;
    public sealed record Union(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Intersect(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Difference(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Closure(SetExpr Seed, WalkDepth Depth) : SetExpr;
}

public readonly record struct ElementSet(UInt128 Receipt, Seq<NodeId> Keys, int Count, ReadOnlyMemory<byte> Preimage) {
    public static readonly ElementSet Empty = Of(Seq<NodeId>());
    // `Preimage` exposes the exact length-framed bytes hashed by `Receipt` and frozen by the parity corpus.
    public static ElementSet Of(Seq<NodeId> keys) {
        Seq<NodeId> sorted = toSeq(keys.Distinct().OrderBy(static k => k.Value, StringComparer.Ordinal));
        ReadOnlyMemory<byte> preimage = ElementSetAlgebra.Canonical(sorted);
        return new ElementSet(ContentHash.Of(preimage.Span), sorted, sorted.Count, preimage);
    }
}

// `SetResolve` carries index-backed leaf resolution and one-hop topology expansion.
// Threaded ports keep reachability in the graph owner while this page owns algebraic closure.
public readonly record struct SetResolve(Func<SetExpr, Fin<Seq<NodeId>>> Leaf, Func<Seq<NodeId>, Fin<Seq<NodeId>>> Expand);

public static class ElementSetAlgebra {
    // `Receipt` uses the kernel seed-zero `ContentHash.Of` entry over parity-frozen bytes.
    public static UInt128 Receipt(Seq<NodeId> sortedKeys) => ContentHash.Of(Canonical(sortedKeys).Span);

    // Cross-runtime parity frames distinct-sorted UTF-8 keys with little-endian lengths.
    // Framing distinguishes concatenation-equivalent key rosters.
    public static ReadOnlyMemory<byte> Canonical(Seq<NodeId> sortedKeys) {
        ArrayBufferWriter<byte> buffer = new();
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4), sortedKeys.Count);
        buffer.Advance(4);
        foreach (NodeId key in sortedKeys) {
            int bytes = Encoding.UTF8.GetByteCount(key.Value);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4), bytes);
            buffer.Advance(4);
            Encoding.UTF8.GetBytes(key.Value, buffer.GetSpan(bytes));
            buffer.Advance(bytes);
        }
        return buffer.WrittenMemory;
    }

    public static Fin<ElementSet> Evaluate(SetExpr expr, SetResolve resolve) => expr.Switch(
        resolve,
        literal: static (_, lit) => Fin.Succ(ElementSet.Of(lit.Keys)),
        predicate: static (r, e) => r.Leaf(e).Map(ElementSet.Of),
        byRule: static (r, e) => r.Leaf(e).Map(ElementSet.Of),
        union: static (r, u) =>
            from left in Evaluate(u.Left, r)
            from right in Evaluate(u.Right, r)
            select ElementSet.Of(left.Keys + right.Keys),
        intersect: static (r, i) =>
            from left in Evaluate(i.Left, r)
            from right in Evaluate(i.Right, r)
            select ElementSet.Of(toSeq(left.Keys.Intersect(right.Keys))),
        difference: static (r, d) =>
            from left in Evaluate(d.Left, r)
            from right in Evaluate(d.Right, r)
            select ElementSet.Of(toSeq(left.Keys.Except(right.Keys))),
        closure: static (r, c) => Evaluate(c.Seed, r).Bind(seed => Closed(seed.Keys, c.Depth.Value, r.Expand)));

    static Fin<ElementSet> Closed(Seq<NodeId> seed, int depth, Func<Seq<NodeId>, Fin<Seq<NodeId>>> expand) =>
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
|  [02]   | receipt            | `ContentHash.Of` over length-framed preimage            | stable + collision-free; the reuse key + parity preimage |
|  [03]   | typed leaves       | `SetPredicate` + `SpatialOp`/`JsonComparison` operators | no raw-string predicate/op; lowered to a store index     |
|  [04]   | closure            | bounded transitive fold over topology                   | one-hop `Expand` is `Query/topology`; not the manifest   |
|  [05]   | bounded depth      | `WalkDepth` admitted once (`SelectionFault.Depth`)      | closure/cell/topology/cypher share ONE axis; no raw int  |
|  [06]   | cell leaf          | `Cell(H3Cell, WalkDepth)` grid-disk predicate           | `h3-pg` index-served; the H3 sibling of the GiST leaf    |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
