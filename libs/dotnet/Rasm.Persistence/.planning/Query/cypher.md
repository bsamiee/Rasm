# [PERSISTENCE_QUERY_CYPHER]

`CypherEnablement.SelfHosted` opens Rasm.Persistence's analytical graph lane over Apache AGE and `pgrouting`, seated below the authoritative in-process `Query/topology` owner. `GraphQuery` is the closed verb family, `GraphDdl` owns lifecycle, `Identifier` and `CypherClause` admit AGE structure, and `H3Cell` is the routing node identity. `GraphResult` preserves node, cell, point, edge, mutation, and partition spaces; projections return `Option` so an incompatible space never masquerades as an empty successful result.

## [01]-[INDEX]

- [02]-[GRAPH_SESSION]: `CypherEnablement` self-hosted gate, the `CypherFault` closed boundary band, the `GraphDdl` graph/label lifecycle surface, and the `GraphSession` owning the per-physical-connection `LOAD 'age'`+`search_path` init, the async daemon projection, and the `agtype`/path/Edges-SQL decode primitives both lanes compose.
- [03]-[GRAPH_QUERY]: `GraphQuery` the ONE `[Union]` collapsing every AGE openCypher and `pgrouting` verb, the `RouteMode`/`FlowKind`/`CleaveKind` function-selector rows, the `GraphResult` typed carrier, the `H3Cell` vertex-id node space, and the total-`Switch` `Run` that lowers each case to its server-side SQL and decodes its rows into the NODE-space `KeySelection` (AGE), the CELL-space `H3Cell` mesh (`pgrouting`), or the raw EDGE space (`pgr_bridges`/`pgr_biconnectedComponents`/the flow labeling).

## [02]-[GRAPH_SESSION]

- Owner: `CypherEnablement` is the closed `Disabled | SelfHosted` availability family and the lane's admission owner through `Admit`. `CypherClause` admits the dollar-quoted body. `GraphDdl` owns graph and label lifecycle. `GraphSession` owns connection initialization, rebuild, execution, decoding, and edge projection.
- Cases: `CypherEnablement` is `Disabled` or `SelfHosted`; `CypherFault` is `Disabled`, `LaneUnrealizable`, `MatchFailed`, `RouteFailed`, or `Unresolvable`; `GraphDdl` carries `Create`, `Drop`, `VertexLabel`, `EdgeLabel`, `DropLabel`, `Rename`, `LoadVertices`, and `LoadEdges`; `DestructiveMode` supplies `Restrict` or `Cascade`; `VertexIdMode` supplies `Generated` or `Provided`.
- Entry: `CypherEnablement.Admit(StoreProfile, CypherEnablement)` is the lane's one gate and every enablement value below reaches composition through it; `Provision` installs `LOAD 'age'` and `search_path` through `UsePhysicalConnectionInitializer` when `CypherEnablement.SelfHosted`; `Rebuild(IDocumentStore)` starts the daemon and consumes `QueryLane.Cypher.WaitBudget`; `Define` lowers one `GraphDdl` case; `Decode` admits `properties.id`; `DecodePath` folds the registered `agtype`→`jsonb` path; `Vid` and `CellOf` preserve the `H3Cell` node space.
- Auto: the lane is `Disabled` by construction so a deployment that has not provisioned `age`/`pgrouting` reads the in-process topology and never reaches for a missing extension; when `SelfHosted`, `Provision` registers the connection-init hook so every physical connection runs `LOAD 'age'` and sets `search_path` once at open (a PL/pgSQL Cypher wrapper repeats it in its own body per `api-apache-age#SESSION_SETUP`, but the managed lane pays it at the physical-connection boundary, not per `cypher()` call); the AGE backing relations and the `pgrouting` network edge table materialize through the ONE `AgeGraphProjection` Marten async projection (`ProjectionLifecycle.Async`, registered at composition) folding committed `GraphDelta` events — the only steady-state producer, rebuilt from the one Marten stream so neither duplicates the event store, and `Rebuild` awaits exactly this projection's shard watermark; the Cypher read's OUTER `SELECT` extracts `(v->'properties'->>'id')` server-side through the registered `agtype` operators so `Decode` receives a PLAIN text column (the projection wrote the contract `NodeId.ToValue()` beside the owning `ModelId` as the vertex `id`/`model` properties), making the decoded node the REAL model-qualified `SetKey`, never the AGE internal `graphid` integer and never a client-side hand-parse; a PATH-shaped read (`Reach`) RETURNs the whole path `p` and the outer `SELECT` casts `(p)::jsonb` through the REGISTERED cast so `DecodePath` receives one typed JSON document — the alternating vertex/edge array — rather than a comma-split of the `::path` text; `Define` runs the graph/label lifecycle through the same gate, and the role owning `ag_catalog` owns its idempotence; a bulk `LoadVertices`/`LoadEdges` stays coherent with later `*`-range traversals because the `age_invalidate_graph_cache()` trigger bumps the graph version on every direct backing-relation write; `Vid` reinterprets the `H3Cell` `long` (the `h3-pg` cell convention `Element/identity#ELEMENT_IDENTITY` already stamps) so the `pgrouting` Edges-SQL vertex id, the GiST/BRIN H3 index, and the in-process `pocketken.H3` cell agree bit-for-bit.
- Packages: Npgsql (`NpgsqlDataSourceBuilder.UsePhysicalConnectionInitializer`/`NpgsqlDataSource`/`NpgsqlConnection`), Marten (`IDocumentStore.BuildProjectionDaemonAsync`/`WaitForNonStaleProjectionDataAsync`), pocketken.H3 (`H3Index`), Rasm.Element (`NodeId`), Rasm.Persistence (`Element/identity#ELEMENT_IDENTITY` `H3Cell`, `Query/lane#READ_ROUTING` `StalenessWatermark`, `Store/provisioning#SERVER_EXTENSIONS` `StoreProfile.Admits` — the lane-realizability axis), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.Text.Json, BCL inbox.
- Growth: a new enablement tier is one `CypherEnablement` row; a new boundary cause is one `CypherFault` case whose offset stays inside its band span; a new lifecycle op is one `GraphDdl` case lowering to its `ag_catalog` function; zero new surface — making AGE/`pgrouting` the default authoritative topology, hosting it in the same instance as Marten without the gate, a managed AGE/`pgrouting` driver wrapper, a per-query `LOAD 'age'`, a comma-split `agtype` or `::path` text decode, a page-local band constant beside the kernel `FaultBand` roster, or a `GetHashCode`-derived vertex id is the deleted form because QuikGraph is the default, the extensions are optional self-hosted analytical lanes driven through raw `Npgsql` against the registered `agtype` casts, the session-load is a physical-connection hook, and the node space is the `h3-pg` `H3Cell`.
- Boundary: lane realizability and deployment enablement are TWO axes refusing at two grains — `CypherEnablement.Admit` is the lane's one admission owner, refusing `LaneUnrealizable` where the engine cannot realize the graph lane at all, while the `SelfHosted` gates on `Provision`/`Define`/`Run` refuse `Disabled` where a capable engine was not armed; folding either into the other loses which repair applies, and a per-verb realizability test is the deleted form. AGE and `pgrouting` are OPTIONAL self-hosted-only read projections (`H5`) demoted beneath the in-process QuikGraph view which is the default authoritative topology owner — a deployment never ASSUMES one PostgreSQL instance hosts both an extension and Marten-9/Npgsql-10, so the lane gates on `CypherEnablement.SelfHosted` and falls back to the in-process topology when disabled; both are the `Query/lane#READ_ROUTING` ASYNC analytical lanes carrying a `StalenessWatermark`, so an interactive-correctness query (clash, void-resolution, live QTO) binds the synchronous topology and never reads here without `WaitForNonStaleProjectionDataAsync`; both are driven through raw `Npgsql` against the server-side SQL because neither ships a managed/EF driver, and the mandatory `AS (col agtype, ...)` / `AS (seq integer, ...)` column-definition list is required by PostgreSQL's `RETURNS SETOF record` contract, never optional (`api-apache-age#CYPHER_QUERY`, `api-pgrouting#IMPLEMENTATION_LAW`); the AGE backing relations and the `pgrouting` network edge table are async daemon projections off the one Marten stream so both are rebuildable and never a second event store; the session-load (`LOAD 'age'`+`search_path`) is a physical-connection-init obligation the `Npgsql` open-hook pays once per connection, never a per-query statement that re-loads the shared library; the AGE vertex decodes to the REAL model-qualified `SetKey` through the `properties.model`/`properties.id` pair (the projection stamped both off the stream id and the contract id) via the registered `agtype ->>` operator, and the `pgrouting` vertex id is the `Element/identity#ELEMENT_IDENTITY` `H3Cell` `long` (the `h3-pg` convention) the in-process `pocketken.H3` cell shares — a `graphid` integer surfacing as a `NodeId`, a comma-split text decode, or a `GetHashCode`-derived `bigint` vertex id is the named `docs/stacks/csharp/boundaries#ADMISSION` defect this page deletes; the graph/label lifecycle is a `SELECT`-function DDL surface (`GraphDdl` lowering to the `ag_catalog` functions, every argument Npgsql-bound), gate-checked like every lane op and DISTINCT from the one-shot frozen `ServerExtension.CreateSql` install row — a `Define` that re-issues `CREATE EXTENSION`, or a daemon projection writing into a graph no owner provisioned, is the deleted form.

```csharp
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using JasperFx.Events.Daemon;
using LanguageExt;
using LanguageExt.Common;
using Marten;
using NodaTime;
using Npgsql;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Persistence.Element;
using Rasm.Persistence.Store;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CypherEnablement {
    private CypherEnablement() { }
    public sealed record Disabled : CypherEnablement;
    public sealed record SelfHosted : CypherEnablement;

    public const string Lane = "cypher";

    public static Fin<CypherEnablement> Admit(StoreProfile store, CypherEnablement declared) =>
        store.Admits(Lane)
            ? Fin.Succ(declared)
            : Fin.Fail<CypherEnablement>(new CypherFault.LaneUnrealizable(Lane));
}

[ValueObject<string>]
[ValidationError]
public readonly partial struct CypherClause {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)
            || value.IndexOfAny(['\'', '"', ';', '\0']) >= 0
            || value.Contains("$$", StringComparison.Ordinal)
            || value.Contains("//", StringComparison.Ordinal)
            || value.Contains("/*", StringComparison.Ordinal)) {
            validationError = ValidationError.Create($"<clause-refused:{value.Length}>");
        }
    }
}

[ValueObject<int>]
[ValidationError]
public readonly partial struct RouteAlternativeCount {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value < 1) validationError = ValidationError.Create($"<alternative-count:{value}>");
    }
}

[ValueObject<double>]
[ValidationError]
public readonly partial struct RouteRadius {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value) || value <= 0.0) validationError = ValidationError.Create($"<route-radius:{value.ToString(CultureInfo.InvariantCulture)}>");
    }
}

[ValueObject<long>]
[ValidationError]
public readonly partial struct RoutePointId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref long value) {
        if (value < 1) validationError = ValidationError.Create($"<route-point:{value}>");
    }
}

[ValueObject<string>]
[ValidationError]
public readonly partial struct RouteSql {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value) || value.Contains(';') || value.Contains('\0') || value.Contains("--", StringComparison.Ordinal) || value.Contains("/*", StringComparison.Ordinal)) {
            validationError = ValidationError.Create("<route-sql-refused>");
        }
    }

    public static readonly RouteSql Network = Create("SELECT id, source, target, cost, reverse_cost FROM network_edge");
    public static readonly RouteSql NetworkXy = Create("SELECT id, source, target, cost, reverse_cost, x1, y1, x2, y2 FROM network_edge");
    public static readonly RouteSql Capacity = Create("SELECT id, source, target, capacity, reverse_capacity FROM network_edge");
    public static readonly RouteSql Points = Create("SELECT pid, edge_id, fraction, side FROM network_point");
    public static readonly RouteSql Coordinates = Create("SELECT id, x, y FROM network_vertex");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RoutingLocation {
    private RoutingLocation() { }
    public sealed record Cell(H3Cell Value) : RoutingLocation;
    public sealed record Point(RoutePointId Value) : RoutingLocation;

    public long Vid => Switch(
        cell: static cell => GraphSession.Vid(cell.Value),
        point: static point => -(long)point.Value);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DestructiveMode {
    public static readonly DestructiveMode Restrict = new("restrict", false);
    public static readonly DestructiveMode Cascade = new("cascade", true);
    public bool Enabled { get; }
    private DestructiveMode(string key, bool enabled) : this(key) => Enabled = enabled;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VertexIdMode {
    public static readonly VertexIdMode Generated = new("generated", false);
    public static readonly VertexIdMode Provided = new("provided", true);
    public bool Exists { get; }
    private VertexIdMode(string key, bool exists) : this(key) => Exists = exists;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CypherFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Cypher;
    private CypherFault() { }

    [FaultCase(0)]
    public sealed partial record Disabled : CypherFault();
    [FaultCase(1)]
    public sealed partial record LaneUnrealizable(string Lane) : CypherFault();
    [FaultCase(2)]
    public sealed partial record MatchFailed(Error Cause) : CypherFault(), ICausedFault;
    [FaultCase(3)]
    public sealed partial record RouteFailed(Error Cause) : CypherFault(), ICausedFault;
    [FaultCase(4)]
    public sealed partial record Unresolvable(string Detail) : CypherFault();

    public override string Message => Switch(
        disabled:          static _ => "<graph-lane-disabled>",
        laneUnrealizable:  static c => $"<store-lane-unrealizable:{c.Lane}>",
        matchFailed:       static c => $"<cypher-match:{c.Cause.Message}>",
        routeFailed:       static c => $"<pgr-route:{c.Cause.Message}>",
        unresolvable:      static c => $"<graph-vertex-unresolvable:{c.Detail}>");
}
```

```csharp
// --- [SERVICES] ------------------------------------------------------------------------
public static class GraphSession {
    public static NpgsqlDataSourceBuilder Provision(NpgsqlDataSourceBuilder builder, CypherEnablement gate) =>
        gate is CypherEnablement.SelfHosted
            ? builder.UsePhysicalConnectionInitializer(
                connection => {
                    using NpgsqlCommand load = connection.CreateCommand();
                    load.CommandText = SessionLoad;
                    load.ExecuteNonQuery();
                },
                async connection => {
                    await using NpgsqlCommand load = connection.CreateCommand();
                    load.CommandText = SessionLoad;
                    await load.ExecuteNonQueryAsync().ConfigureAwait(false);
                })
            : builder;

    const string SessionLoad = "LOAD 'age'; SET search_path = ag_catalog, \"$user\", public;";

    public static IO<Duration> Rebuild(IDocumentStore store) =>
        IO.liftAsync(async () => await Try.lift(async _ => {
            await using IProjectionDaemon daemon = await store.BuildProjectionDaemonAsync().ConfigureAwait(false);
            await daemon.StartAllAsync().ConfigureAwait(false);
            return Fin<Duration>.Succ(await ReadRouter.AwaitNonStale(daemon, QueryLane.Cypher).RunAsync().ConfigureAwait(false));
        }).Run().Bind(static inner => inner).ConfigureAwait(false)).Bind(IO.lift);

    public static Fin<SetKey> Decode(string? extractedModel, string? extractedId) =>
        extractedModel is { } model && Guid.TryParse(model, out Guid parsed) && extractedId is { } value
            ? Fin.Succ(new SetKey(ModelId.Create(parsed), NodeId.Create(value)))
            : Fin.Fail<SetKey>(new CypherFault.Unresolvable("<vertex-identity-absent>"));

    public static Fin<AgtypePath> DecodePath(string json) =>
        Try.lift(() => {
            using JsonDocument path = JsonDocument.Parse(json);
            Seq<(JsonElement Element, int Index)> steps = toSeq(path.RootElement.EnumerateArray().Select(static (element, index) => (Element: element, Index: index)));
            return steps.Filter(static s => (s.Index & 1) == 0)
                .TraverseM(static s => Decode(TextProperty(s.Element, "model"), TextProperty(s.Element, "id"))).As()
                .Map(vertices => new AgtypePath(
                    vertices,
                    steps.Filter(static s => (s.Index & 1) == 1)
                        .Fold(0.0, static (cost, s) => cost + NumberProperty(s.Element, "cost"))));
        }).Run().Bind(static inner => inner);

    static string? TextProperty(JsonElement element, string name) {
        bool found = element.GetProperty("properties").TryGetProperty(name, out JsonElement value);
        return found ? value.GetString() : null;
    }

    static double NumberProperty(JsonElement element, string name) {
        bool found = element.GetProperty("properties").TryGetProperty(name, out JsonElement value);
        return found ? value.GetDouble() : 0.0;
    }

    public static long Vid(H3Cell cell) => cell.Value;
    public static H3Cell CellOf(long vid) => H3Cell.Create(vid);

    public static IO<Fin<Unit>> Define(NpgsqlDataSource source, CypherEnablement gate, GraphDdl ddl) =>
        gate is CypherEnablement.SelfHosted
            ? IO.liftAsync(async () => (await Try.lift(async _ => {
                  await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
                  await using NpgsqlCommand command = connection.CreateCommand();
                  (string Sql, Seq<(string Name, object Value)> Args) lowering = ddl.Lower();
                  command.CommandText = lowering.Sql;
                  lowering.Args.Iter(argument => command.Parameters.AddWithValue(argument.Name, argument.Value));
                  await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                  return Fin.Succ(unit);
              }).Run().Bind(static inner => inner).ConfigureAwait(false)).MapFail(static error => error.Exception.Case is NpgsqlException
                  ? (Error)new CypherFault.MatchFailed(error)
                  : error))
            : IO.pure(Fin<Unit>.Fail(new CypherFault.Disabled()));
}

public sealed class AgeGraphProjection(NpgsqlDataSource source, Identifier graph) : IProjection {
    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<StreamAction> streams, CancellationToken token) {
        await using NpgsqlConnection lane = await source.OpenConnectionAsync(token).ConfigureAwait(false);
        foreach (StreamAction stream in streams) {
            string model = stream.Id.ToString("D");
            foreach (GraphDelta delta in stream.Events.Select(static e => e.Data).OfType<GraphDelta>()) {
                foreach (Node node in delta.Added) { await Cypher(lane, "MERGE (n:Element {id: $id, model: $model})", ("id", node.Id.ToValue()), ("model", model), token).ConfigureAwait(false); }
                foreach (NodeId removed in delta.Removed) { await Cypher(lane, "MATCH (n:Element {id: $id, model: $model}) DETACH DELETE n", ("id", removed.ToValue()), ("model", model), token).ConfigureAwait(false); }
                foreach (Relationship edge in delta.Linked) {
                    await Cypher(lane, "MATCH (a:Element {id: $from, model: $model}), (b:Element {id: $to, model: $model}) MERGE (a)-[:REL {kind: $kind}]->(b)", ("from", edge.Relating.ToValue()), ("to", edge.Related.ToValue()), token, ("model", model), ("kind", edge.Kind.Key)).ConfigureAwait(false);
                }
            }
        }
    }

    static async Task Cypher(NpgsqlConnection lane, string body, (string, object) first, (string, object) second, CancellationToken token, params (string, object)[] rest) {
        await using NpgsqlCommand command = lane.CreateCommand();
        command.CommandText = $"SELECT * FROM cypher(@graph, $${body}$$, @params) AS (v agtype)";
        _ = command.Parameters.AddWithValue("graph", (string)graph);
        _ = command.Parameters.AddWithValue("params", System.Text.Json.JsonSerializer.Serialize(new[] { first, second }.Concat(rest).ToDictionary(static p => p.Item1, static p => p.Item2)));
        _ = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphDdl {
    private GraphDdl() { }
    public sealed record Create(Identifier Graph) : GraphDdl;
    public sealed record Drop(Identifier Graph, DestructiveMode Mode) : GraphDdl;
    public sealed record VertexLabel(Identifier Graph, Identifier Label) : GraphDdl;
    public sealed record EdgeLabel(Identifier Graph, Identifier Label) : GraphDdl;
    public sealed record DropLabel(Identifier Graph, Identifier Label, DestructiveMode Mode) : GraphDdl;
    public sealed record Rename(Identifier Graph, Identifier To) : GraphDdl;
    public sealed record LoadVertices(Identifier Graph, Identifier Label, StorePath ServerPath, VertexIdMode Ids) : GraphDdl;
    public sealed record LoadEdges(Identifier Graph, Identifier Label, StorePath ServerPath) : GraphDdl;

    public (string Sql, Seq<(string Name, object Value)> Args) Lower() => Switch<(string, Seq<(string, object)>)>(
        create:       static c => ("SELECT ag_catalog.create_graph(@g)", Seq<(string, object)>(("g", (string)c.Graph))),
        drop:         static d => ("SELECT ag_catalog.drop_graph(@g, @cascade)", Seq<(string, object)>(("g", (string)d.Graph), ("cascade", d.Mode.Enabled))),
        vertexLabel:  static v => ("SELECT ag_catalog.create_vlabel(@g, @l)", Seq<(string, object)>(("g", (string)v.Graph), ("l", (string)v.Label))),
        edgeLabel:    static e => ("SELECT ag_catalog.create_elabel(@g, @l)", Seq<(string, object)>(("g", (string)e.Graph), ("l", (string)e.Label))),
        dropLabel:    static d => ("SELECT ag_catalog.drop_label(@g, @l, @force)", Seq<(string, object)>(("g", (string)d.Graph), ("l", (string)d.Label), ("force", d.Mode.Enabled))),
        rename:       static r => ("SELECT ag_catalog.alter_graph(@g, 'RENAME', @to)", Seq<(string, object)>(("g", (string)r.Graph), ("to", (string)r.To))),
        loadVertices: static l => ("SELECT ag_catalog.load_labels_from_file(@g, @l, @path, @idField)", Seq<(string, object)>(("g", (string)l.Graph), ("l", (string)l.Label), ("path", (string)l.ServerPath), ("idField", l.Ids.Exists))),
        loadEdges:    static l => ("SELECT ag_catalog.load_edges_from_file(@g, @l, @path)", Seq<(string, object)>(("g", (string)l.Graph), ("l", (string)l.Label), ("path", (string)l.ServerPath))));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                       | [BINDING]                                                         |
| :-----: | :------------------ | :-------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | enablement          | `Disabled` default, `SelfHosted` opt          | QuikGraph is authoritative; extensions optional (`H5`)            |
|  [02]   | driver              | raw `Npgsql` over `agtype`/`SETOF record`     | no managed AGE/`pgrouting` driver; never EF                       |
|  [03]   | session-load        | `UsePhysicalConnectionInitializer`            | `LOAD 'age'`+`search_path` once per connection, never per query   |
|  [04]   | AGE node id         | `properties.model`+`properties.id` → `SetKey` | the model-qualified contract key, never the `graphid` integer     |
|  [05]   | `pgrouting` node id | `h3-pg` `H3Cell` `long`                       | shares the in-process `pocketken.H3` cell; never `GetHashCode`    |
|  [06]   | consistency stance  | async, `StalenessWatermark`                   | interactive correctness binds the synchronous topology            |
|  [07]   | graph source        | async daemon projection off Marten            | rebuildable, never a second event store                           |
|  [08]   | fault family        | `CypherFault` on `Fault`                      | one roster, sealed `Code`; never `Error.New`                      |
|  [09]   | graph lifecycle     | `GraphDdl` → `ag_catalog` DDL                 | parameter-bound DDL/loaders; `ServerExtension.CreateSql` boundary |
|  [10]   | lane admission      | `CypherEnablement.Admit` at composition       | `LaneUnrealizable` names the lane; `Disabled` the opt-out         |

## [03]-[GRAPH_QUERY]

- Owner: `GraphQuery` closes AGE and `pgrouting` verbs; `RouteSql` admits every server-reparsed inner query; `RouteMode`, `FlowKind`, and `CleaveKind` carry algorithm policy; `CleaveShape` carries result arity; `RoutingLocation` distinguishes cells from off-edge points; `GraphResult` preserves node, cell, point, edge, mutation, and partition spaces; `GraphLane` owns gate-checked lowering and projection.
- Cases: `GraphQuery` carries `Match`, `Mutate`, `Reach`, `Path`, `Via`, `Located`, `Kth`, `Spread`, `Tour`, `TourPlanar`, `Flow`, and `Cleave`; `RouteMode` carries `Dijkstra`, `AStar`, and `Bidirectional` with their default `RouteSql`; `FlowKind` carries the labeled max-flow algorithms; `CleaveKind` carries `Strong`, `Connected`, `Articulation`, `Bridges`, and `Biconnected` with one `CleaveShape`; `GraphResult` carries `Paths`, `Mutated`, `Routed`, `Legged`, `Traced`, `Ranked`, `Spanned`, `Toured`, `Flowed`, `Cleaved`, `Severed`, and `Sundered`.
- Entry: `Run` dispatches the total query family; `ToSet` and `ToCells` return `Option` projections; `CypherRead` and `CypherWrite` preserve AGE payload timing without a caller-selected boolean; `Route`, `Via`, `Located`, `Ksp`, `Spread`, `Tour`, `TourPlanar`, `Flow`, and `Cleave` execute the catalogued server algorithms.
- Auto: `Run` rejects `Disabled` before opening a connection. AGE embeds admitted `Identifier` and `CypherClause` structure and binds `params agtype`; `Reach` casts each path through `agtype`→`jsonb`. Routing binds admitted `RouteSql`, `H3Cell`, `RoutingLocation`, `RouteAlternativeCount`, and `RouteRadius` values. `RouteMode.DefaultEdges` selects the A* coordinate schema only when the caller retains `RouteSql.Network`. `CleaveShape` determines SQL columns and result identity. Provider failures lift into `CypherFault`.
- Packages: Npgsql (`NpgsqlDataSource.OpenConnectionAsync`/`NpgsqlCommand`/`NpgsqlParameterCollection.AddWithValue`/`NpgsqlDataReader`/`PostgresException`), System.Text.Json (`JsonDocument` — the `(p)::jsonb` path decode), Rasm.Element (`NodeId`), Rasm.Persistence (`Element/identity#ELEMENT_IDENTITY` `H3Cell`, `Query/lane#ELEMENT_SET_ALGEBRA` `KeySelection`/`SetKey`/`SetScope`/`Selections.Depth` — the certified selection currency and the band-owning depth admission, `Query/columnar#COLUMNAR_LANE` `Identifier` — the graph-name trust gate), Rasm.Element (`Query/predicate#PREDICATE_ALGEBRA` `WalkDepth` — the ONE bounded-walk carrier corpus-wide), Rasm (`Rasm.Domain` `FaultBand`/`[FaultCase]`/`Fault`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Law: `WalkDepth` resolves to the `Rasm.Element` `Query/predicate#PREDICATE_ALGEBRA` contract declaration — the ONE bounded-walk carrier corpus-wide — reached through the `Query/lane#ELEMENT_SET_ALGEBRA` `Selections.Depth` wrapper, which runs the contract admission and re-keys its refusal onto this folder's `SelectionFault.Depth` band. `Reach`'s two hop bounds therefore carry the same type a closure fold, a cell ring, and a topology walk carry, and the contract's `WalkDepth.Whole` sentinel is available to an unbounded `*`-range traversal without this page spelling an integer.
- Law: every result on this page is `Fin`-shaped end to end; `Op.Catch` captures provider raises before the two lane gates lower their documented operation contexts onto `CypherFault`.
- Growth: a new graph verb is one `GraphQuery` case and `Switch` arm; a new path or flow algorithm is one policy row; a new connectivity output is one `CleaveShape` case; a new structural inner query enters through `RouteSql`. Server algorithms remain AGE/`pgrouting` capabilities, node results compose at `KeySelection` or `H3Cell`, and edge results remain edge carriers.
- Boundary: `GraphQuery` is the ONE polymorphic verb surface so every AGE and `pgrouting` capability is one case under one total `Switch` — a sibling `Match`/`Route`/`DrivingDistance` method family, a `bool`/`mode` flag selecting a verb, or a thin slice of the rich `pgrouting` roster (Dijkstra/A*/bidirectional/VIA/withPoints/KSP/DD/TSP/TSPeuclidean/flow-family/components/biconnected) is the deleted form, `RouteMode`/`FlowKind`/`CleaveKind` carrying the function variance and the verb family carrying the rest; value transport is STRUCTURAL — the graph name admits once through the `Query/columnar#COLUMNAR_LANE` `Identifier` trust gate, the Cypher body once through `CypherClause` (rejecting the `$$` breakout token, the ONLY escape from the dollar-quote), dynamic values ride the `params agtype` `$name` channel, and the Edges/Points/Coordinates-SQL bind through `Npgsql` parameters — so a raw caller-concatenated graph body is unrepresentable, the literal embed of the two admitted values is the named AGE parse-transform boundary, and the `Match`/`Mutate`/`Reach` `Parameters` are LIVE, never a dead field; the `agtype` rows decode through `GraphSession.Decode`/`DecodePath` (the registered `->>`/`::jsonb` casts) so a returned vertex projects to the real contract `NodeId` and a returned path to the REAL multi-vertex walk, never the internal `graphid` integer, never a comma-split, never a one-vertex stub with a fabricated weight; `Via` and `Tour` stay TWO verbs because ordered-waypoint traversal and optimal-order touring are different problems (the input shape — a waypoint sequence versus a stop set with free order — is the discriminant, `MODAL_ARITY`), and `Tour`/`TourPlanar` discriminate on input shape too (an Edges-SQL cost matrix versus a Coordinates-SQL plane); the `pgrouting` NODE `bigint` is the `H3Cell` the in-process `pocketken.H3` cell shares so the in-database route and the in-process path agree on node identity (`api-pgrouting#IMPLEMENTATION_LAW`), the cell→element resolution a one-hop `Element/identity#STORE_OPERATION_BRACKET` `IdentityOp.Route` the caller composes; a `pgr_bridges` cut-EDGE, a `pgr_biconnectedComponents` edge-component, a flow edge label, and a `pgr_withPoints` NEGATIVE Point vid are NOT nodes — they decode to `Severed`/`Sundered`/`Flowed.Assignment`/`Traced` and never pass through `CellOf` (the fabricated-cell defect the decode-plane columns delete); the flow verbs return the PER-EDGE assignment because the cut, the bottleneck, and the residual analysis all need the labeling — the scalar is a derived fold, never the whole answer; the AGE write half (`Mutate`) is split from the read (`api-apache-age#CYPHER_QUERY` rejects a mutate-and-return statement) and the variable-length `Reach` lowers to the Cypher `*` range the `age_vle` engine plans, never a managed BFS; the network edge table and the AGE backing relations are async daemon projections off the one Marten stream so a routing/match query rides the one graph, never a second network store, and an interactive-correctness routing read binds the synchronous in-process counterpart.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RouteMode {
    public static readonly RouteMode Dijkstra = new("dijkstra", "pgr_dijkstra", RouteSql.Network);
    public static readonly RouteMode AStar = new("astar", "pgr_aStar", RouteSql.NetworkXy);
    public static readonly RouteMode Bidirectional = new("bidirectional", "pgr_bdDijkstra", RouteSql.Network);
    public string Function { get; }
    public RouteSql DefaultEdges { get; }
    private RouteMode(string key, string function, RouteSql defaultEdges) : this(key) => (Function, DefaultEdges) = (function, defaultEdges);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlowKind {
    public static readonly FlowKind BoykovKolmogorov = new("boykov-kolmogorov", "pgr_boykovKolmogorov");
    public static readonly FlowKind PushRelabel = new("push-relabel", "pgr_pushRelabel");
    public static readonly FlowKind EdmondsKarp = new("edmonds-karp", "pgr_edmondsKarp");
    public string Function { get; }
    private FlowKind(string key, string function) : this(key) => Function = function;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CleaveKind {
    public static readonly CleaveKind Strong = new("strong", "pgr_strongComponents", new CleaveShape.NodePartitions());
    public static readonly CleaveKind Connected = new("connected", "pgr_connectedComponents", new CleaveShape.NodePartitions());
    public static readonly CleaveKind Articulation = new("articulation", "pgr_articulationPoints", new CleaveShape.NodeCuts());
    public static readonly CleaveKind Bridges = new("bridges", "pgr_bridges", new CleaveShape.EdgeCuts());
    public static readonly CleaveKind Biconnected = new("biconnected", "pgr_biconnectedComponents", new CleaveShape.EdgePartitions());
    public string Function { get; }
    public CleaveShape Shape { get; }
    private CleaveKind(string key, string function, CleaveShape shape) : this(key) => (Function, Shape) = (function, shape);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CleaveShape {
    private CleaveShape() { }
    public sealed record NodePartitions : CleaveShape;
    public sealed record NodeCuts : CleaveShape;
    public sealed record EdgeCuts : CleaveShape;
    public sealed record EdgePartitions : CleaveShape;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AgtypePath(Seq<SetKey> Vertices, double Weight);
// --- [OPERATIONS] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphQuery {
    private GraphQuery() { }
    public sealed record Match(Identifier Graph, CypherClause Clause, HashMap<string, string> Parameters) : GraphQuery;
    public sealed record Mutate(Identifier Graph, CypherClause Clause, HashMap<string, string> Parameters) : GraphQuery;
    public sealed record Reach(Identifier Graph, NodeId From, WalkDepth MinHops, WalkDepth MaxHops) : GraphQuery;
    public sealed record Path(RouteSql Edges, H3Cell From, H3Cell To, RouteMode Mode) : GraphQuery;
    public sealed record Via(RouteSql Edges, Seq<H3Cell> Waypoints) : GraphQuery;
    public sealed record Located(RouteSql Edges, RouteSql Points, RoutingLocation From, RoutingLocation To) : GraphQuery;
    public sealed record Kth(RouteSql Edges, H3Cell From, H3Cell To, RouteAlternativeCount Count) : GraphQuery;
    public sealed record Spread(RouteSql Edges, H3Cell Root, RouteRadius Radius) : GraphQuery;
    public sealed record Tour(RouteSql Edges, Seq<H3Cell> Stops, H3Cell Start, H3Cell End) : GraphQuery;
    public sealed record TourPlanar(RouteSql Sites, H3Cell Start, H3Cell End) : GraphQuery;
    public sealed record Flow(RouteSql Edges, H3Cell Source, H3Cell Sink, FlowKind Kind) : GraphQuery;
    public sealed record Cleave(RouteSql Edges, CleaveKind Kind) : GraphQuery;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphResult {
    private GraphResult() { }
    public sealed record Paths(Seq<AgtypePath> Found) : GraphResult;
    public sealed record Mutated(Identifier Graph) : GraphResult;
    public sealed record Routed(Seq<H3Cell> Path, double Cost) : GraphResult;
    public sealed record Legged(Seq<(Seq<H3Cell> Leg, double Cost)> Legs, double Total) : GraphResult;
    public sealed record Traced(Seq<RoutingLocation> Steps, double Cost) : GraphResult;
    public sealed record Ranked(Seq<(Seq<H3Cell> Path, double Cost)> Alternatives) : GraphResult;
    public sealed record Spanned(Seq<H3Cell> Reached, double Radius) : GraphResult;
    public sealed record Toured(Seq<H3Cell> Order, double Cost) : GraphResult;
    public sealed record Flowed(long MaxFlow, Seq<(long Edge, long Flow, long Residual)> Assignment) : GraphResult;
    public sealed record Cleaved(Seq<Seq<H3Cell>> Components) : GraphResult;
    public sealed record Severed(Seq<long> Edges) : GraphResult;
    public sealed record Sundered(Seq<Seq<long>> EdgeComponents) : GraphResult;
}

public static class GraphLane {
    public static IO<Fin<GraphResult>> Run(NpgsqlDataSource source, CypherEnablement gate, GraphQuery query) =>
        gate is CypherEnablement.SelfHosted
            ? query.Switch(
                source,
                match: static (db, m) => CypherRead(db, m.Graph, m.Clause, m.Parameters).Map(static r => r.Map(static p => (GraphResult)new GraphResult.Paths(p))),
                mutate: static (db, m) => CypherWrite(db, m.Graph, m.Clause, m.Parameters).Map(result => result.Map(_ => (GraphResult)new GraphResult.Mutated(m.Graph))),
                reach: static (db, r) => Traverse(db, r.Graph, r.From, r.MinHops, r.MaxHops),
                path: static (db, p) => Route(db, p.Mode.Function, p.Edges == RouteSql.Network ? p.Mode.DefaultEdges : p.Edges, p.From, p.To),
                via: static (db, v) => Via(db, v.Edges, v.Waypoints),
                located: static (db, l) => Located(db, l.Edges, l.Points, l.From, l.To),
                kth: static (db, k) => Ksp(db, k.Edges, k.From, k.To, k.Count),
                spread: static (db, s) => Spread(db, s.Edges, s.Root, s.Radius),
                tour: static (db, t) => Tour(db, t.Edges, t.Stops, t.Start, t.End),
                tourPlanar: static (db, t) => TourPlanar(db, t.Sites, t.Start, t.End),
                flow: static (db, f) => Flow(db, f.Edges, f.Source, f.Sink, f.Kind),
                cleave: static (db, c) => Cleave(db, c.Edges, c.Kind))
            : IO.pure(Fin<GraphResult>.Fail(new CypherFault.Disabled()));

    public static Option<KeySelection> ToSet(GraphResult result) => result.Switch(
        paths: static value => Some(Selected(value.Found.Bind(static path => path.Vertices))),
        mutated: static _ => None,
        routed: static _ => None,
        legged: static _ => None,
        traced: static _ => None,
        ranked: static _ => None,
        spanned: static _ => None,
        toured: static _ => None,
        flowed: static _ => None,
        cleaved: static _ => None,
        severed: static _ => None,
        sundered: static _ => None);

    static KeySelection Selected(Seq<SetKey> vertices) =>
        KeySelection.Of(vertices, new SetScope(toSeq(vertices.Map(static key => key.Model).Distinct())));

    public static Option<Seq<H3Cell>> ToCells(GraphResult result) => result.Switch(
        paths: static _ => None,
        mutated: static _ => None,
        routed: static value => Some(value.Path),
        legged: static value => Some(value.Legs.Bind(static leg => leg.Leg)),
        traced: static value => Some(value.Steps.Choose(static step => step.Switch(
            cell: static cell => Some(cell.Value),
            point: static _ => None))),
        ranked: static value => Some(value.Alternatives.Bind(static alternative => alternative.Path)),
        spanned: static value => Some(value.Reached),
        toured: static value => Some(value.Order),
        flowed: static _ => None,
        cleaved: static value => Some(value.Components.Bind(static component => component)),
        severed: static _ => None,
        sundered: static _ => None);

    // --- [CYPHER] ----------------------------------------------------------------------
    static IO<Fin<Seq<AgtypePath>>> CypherRead(NpgsqlDataSource source, Identifier graph, CypherClause clause, HashMap<string, string> parameters) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT (v->'properties'->>'model') AS model, (v->'properties'->>'id') AS id FROM ag_catalog.cypher('{graph}', $${clause}$$, @params) AS (v agtype)";
            command.Parameters.AddWithValue("params", NpgsqlTypes.NpgsqlDbType.Unknown, JsonSerializer.Serialize(parameters.ToDictionary(static p => p.Key, static p => p.Value)));
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            List<AgtypePath> paths = [];
            while (await reader.ReadAsync().ConfigureAwait(false)) {
                Fin<AgtypePath> step = GraphSession.Decode(
                    await reader.IsDBNullAsync(0).ConfigureAwait(false) ? null : reader.GetString(0),
                    await reader.IsDBNullAsync(1).ConfigureAwait(false) ? null : reader.GetString(1)).Map(static key => new AgtypePath(Seq(), 0.0));
                if (step.IsFail) { return step.Map(static _ => Seq<AgtypePath>()); }
                step.Iter(paths.Add);
            }
            return Fin.Succ(toSeq(paths));
        }, static error => new CypherFault.MatchFailed(error));

    static IO<Fin<Unit>> CypherWrite(NpgsqlDataSource source, Identifier graph, CypherClause clause, HashMap<string, string> parameters) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM ag_catalog.cypher('{graph}', $${clause}$$, @params) AS (v agtype)";
            command.Parameters.AddWithValue("params", NpgsqlTypes.NpgsqlDbType.Unknown, JsonSerializer.Serialize(parameters.ToDictionary(static p => p.Key, static p => p.Value)));
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return Fin.Succ(unit);
        }, static error => new CypherFault.MatchFailed(error));

    static IO<Fin<GraphResult>> Traverse(NpgsqlDataSource source, Identifier graph, NodeId from, WalkDepth minHops, WalkDepth maxHops) =>
        maxHops.Value < minHops.Value
        ? IO.pure(Fin<GraphResult>.Fail(new CypherFault.Unresolvable($"<hop-range:{minHops.Value}>{maxHops.Value}>")))
        : Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT (p)::jsonb FROM ag_catalog.cypher('{graph}', $$MATCH p = (a {{id:$from}})-[*{minHops.Value.ToString(CultureInfo.InvariantCulture)}..{maxHops.Value.ToString(CultureInfo.InvariantCulture)}]->(b) RETURN p$$, @params) AS (p agtype)";
            command.Parameters.AddWithValue("params", NpgsqlTypes.NpgsqlDbType.Unknown, JsonSerializer.Serialize(new Dictionary<string, string> { ["from"] = from.ToValue() }));
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            List<AgtypePath> found = [];
            while (await reader.ReadAsync().ConfigureAwait(false)) {
                Fin<AgtypePath> step = GraphSession.DecodePath(reader.GetString(0));
                if (step.IsFail) { return step.Map(static _ => (GraphResult)new GraphResult.Paths(Seq<AgtypePath>())); }
                step.Iter(found.Add);
            }
            return Fin.Succ((GraphResult)new GraphResult.Paths(toSeq(found)));
        }, static error => new CypherFault.MatchFailed(error));

    // --- [ROUTING] ---------------------------------------------------------------------
    static IO<Fin<GraphResult>> Route(NpgsqlDataSource source, string function, RouteSql edges, H3Cell from, H3Cell to) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT node, agg_cost FROM {function}(@edges, @from, @to, directed => true) AS (seq integer, path_seq integer, start_vid bigint, end_vid bigint, node bigint, edge bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", (string)edges);
            command.Parameters.AddWithValue("from", GraphSession.Vid(from));
            command.Parameters.AddWithValue("to", GraphSession.Vid(to));
            return Fin.Succ((GraphResult)await ReadRoute(command).ConfigureAwait(false));
        }, static error => new CypherFault.RouteFailed(error));

    static IO<Fin<GraphResult>> Via(NpgsqlDataSource source, RouteSql edges, Seq<H3Cell> waypoints) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT path_id, node, agg_cost, route_agg_cost FROM pgr_dijkstraVia(@edges, @vids, directed => true) AS (seq integer, path_id integer, path_seq integer, start_vid bigint, end_vid bigint, node bigint, edge bigint, cost float, agg_cost float, route_agg_cost float)";
            command.Parameters.AddWithValue("edges", (string)edges);
            command.Parameters.AddWithValue("vids", waypoints.Map(GraphSession.Vid).ToArray());
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            List<(int Path, H3Cell Node, double Cost, double RouteCost)> rows = [];
            while (await reader.ReadAsync().ConfigureAwait(false)) { rows.Add((reader.GetInt32(0), GraphSession.CellOf(reader.GetInt64(1)), reader.GetDouble(2), reader.GetDouble(3))); }
            Seq<(Seq<H3Cell> Leg, double Cost)> legs = toSeq(rows.GroupBy(static r => r.Path).OrderBy(static g => g.Key)
                .Select(static g => (Leg: toSeq(g.Select(static r => r.Node)), Cost: g.Max(static r => r.Cost))));
            return Fin.Succ((GraphResult)new GraphResult.Legged(legs, rows.Count > 0 ? rows.Max(static r => r.RouteCost) : 0.0));
        }, static error => new CypherFault.RouteFailed(error));

    static IO<Fin<GraphResult>> Located(NpgsqlDataSource source, RouteSql edges, RouteSql points, RoutingLocation from, RoutingLocation to) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT node, agg_cost FROM pgr_withPoints(@edges, @points, @from, @to, directed => true) AS (seq integer, path_seq integer, start_vid bigint, end_vid bigint, node bigint, edge bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", (string)edges);
            command.Parameters.AddWithValue("points", (string)points);
            command.Parameters.AddWithValue("from", from.Vid);
            command.Parameters.AddWithValue("to", to.Vid);
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            List<RoutingLocation> steps = [];
            double cost = 0.0;
            while (await reader.ReadAsync().ConfigureAwait(false)) {
                long vid = reader.GetInt64(0);
                steps.Add(vid < 0 ? new RoutingLocation.Point(RoutePointId.Create(-vid)) : new RoutingLocation.Cell(GraphSession.CellOf(vid)));
                cost = reader.GetDouble(1);
            }
            return Fin.Succ((GraphResult)new GraphResult.Traced(toSeq(steps), cost));
        }, static error => new CypherFault.RouteFailed(error));

    static IO<Fin<GraphResult>> Ksp(NpgsqlDataSource source, RouteSql edges, H3Cell from, H3Cell to, RouteAlternativeCount count) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT path_id, node, agg_cost FROM pgr_KSP(@edges, @from, @to, @k, directed => true) AS (seq integer, path_id integer, path_seq integer, start_vid bigint, end_vid bigint, node bigint, edge bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", (string)edges);
            command.Parameters.AddWithValue("from", GraphSession.Vid(from));
            command.Parameters.AddWithValue("to", GraphSession.Vid(to));
            command.Parameters.AddWithValue("k", (int)count);
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            List<(int Path, H3Cell Node, double Cost)> rows = [];
            while (await reader.ReadAsync().ConfigureAwait(false)) { rows.Add((reader.GetInt32(0), GraphSession.CellOf(reader.GetInt64(1)), reader.GetDouble(2))); }
            Seq<(Seq<H3Cell> Path, double Cost)> alternatives = toSeq(rows.GroupBy(static r => r.Path).OrderBy(static g => g.Key)
                .Select(static g => (Path: toSeq(g.Select(static r => r.Node)), Cost: g.Max(static r => r.Cost))));
            return Fin.Succ((GraphResult)new GraphResult.Ranked(alternatives));
        }, static error => new CypherFault.RouteFailed(error));

    static IO<Fin<GraphResult>> Spread(NpgsqlDataSource source, RouteSql edges, H3Cell root, RouteRadius radius) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT node, agg_cost FROM pgr_drivingDistance(@edges, @root, @radius, directed => true) AS (seq bigint, depth bigint, start_vid bigint, pred bigint, node bigint, edge bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", (string)edges);
            command.Parameters.AddWithValue("root", GraphSession.Vid(root));
            command.Parameters.AddWithValue("radius", (double)radius);
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            List<H3Cell> reached = [];
            while (await reader.ReadAsync().ConfigureAwait(false)) { reached.Add(GraphSession.CellOf(reader.GetInt64(0))); }
            return Fin.Succ((GraphResult)new GraphResult.Spanned(toSeq(reached), (double)radius));
        }, static error => new CypherFault.RouteFailed(error));

    static IO<Fin<GraphResult>> Tour(NpgsqlDataSource source, RouteSql edges, Seq<H3Cell> stops, H3Cell start, H3Cell end) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT node, agg_cost FROM pgr_TSP(format('SELECT * FROM pgr_dijkstraCostMatrix(%L, %L::bigint[], directed => false)', @edges, @vids::text), start_id => @start, end_id => @end) AS (seq integer, node bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", (string)edges);
            command.Parameters.AddWithValue("vids", stops.Map(GraphSession.Vid).ToArray());
            command.Parameters.AddWithValue("start", GraphSession.Vid(start));
            command.Parameters.AddWithValue("end", GraphSession.Vid(end));
            return Fin.Succ((GraphResult)await ReadTour(command).ConfigureAwait(false));
        }, static error => new CypherFault.RouteFailed(error));

    static IO<Fin<GraphResult>> TourPlanar(NpgsqlDataSource source, RouteSql sites, H3Cell start, H3Cell end) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT node, agg_cost FROM pgr_TSPeuclidean(@sites, start_id => @start, end_id => @end) AS (seq integer, node bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("sites", (string)sites);
            command.Parameters.AddWithValue("start", GraphSession.Vid(start));
            command.Parameters.AddWithValue("end", GraphSession.Vid(end));
            return Fin.Succ((GraphResult)await ReadTour(command).ConfigureAwait(false));
        }, static error => new CypherFault.RouteFailed(error));

    static IO<Fin<GraphResult>> Flow(NpgsqlDataSource source, RouteSql edges, H3Cell flowSource, H3Cell sink, FlowKind kind) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT edge, start_vid, end_vid, flow, residual_capacity FROM {kind.Function}(@edges, @source, @sink) AS (seq integer, edge bigint, start_vid bigint, end_vid bigint, flow bigint, residual_capacity bigint)";
            command.Parameters.AddWithValue("edges", (string)edges);
            command.Parameters.AddWithValue("source", GraphSession.Vid(flowSource));
            command.Parameters.AddWithValue("sink", GraphSession.Vid(sink));
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            List<(long Edge, long Start, long End, long Flow, long Residual)> labeled = [];
            while (await reader.ReadAsync().ConfigureAwait(false)) { labeled.Add((reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt64(3), reader.GetInt64(4))); }
            long into = GraphSession.Vid(sink);
            return Fin.Succ((GraphResult)new GraphResult.Flowed(
                labeled.Where(row => row.End == into).Sum(static row => row.Flow),
                toSeq(labeled.Select(static row => (row.Edge, row.Flow, row.Residual)))));
        }, static error => new CypherFault.RouteFailed(error));

    static IO<Fin<GraphResult>> Cleave(NpgsqlDataSource source, RouteSql edges, CleaveKind kind) =>
        Captured(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = kind.Shape.Switch(
                nodePartitions: _ => $"SELECT component, node FROM {kind.Function}(@edges) AS (seq bigint, component bigint, node bigint)",
                nodeCuts: _ => $"SELECT id FROM {kind.Function}(@edges) AS t(id)",
                edgeCuts: _ => $"SELECT id FROM {kind.Function}(@edges) AS t(id)",
                edgePartitions: _ => $"SELECT component, edge FROM {kind.Function}(@edges) AS (seq bigint, component bigint, edge bigint)");
            command.Parameters.AddWithValue("edges", (string)edges);
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            List<(long Component, long Id)> rows = [];
            while (await reader.ReadAsync().ConfigureAwait(false)) {
                rows.Add(kind.Shape is CleaveShape.NodePartitions or CleaveShape.EdgePartitions
                    ? (reader.GetInt64(0), reader.GetInt64(1))
                    : (0L, reader.GetInt64(0)));
            }
            GraphResult result = kind.Shape.Switch<GraphResult>(
                nodePartitions: _ => new GraphResult.Cleaved(toSeq(rows.GroupBy(static row => row.Component).Select(static group => toSeq(group.Select(static row => GraphSession.CellOf(row.Id)))))),
                nodeCuts: _ => new GraphResult.Cleaved(Seq(toSeq(rows.Select(static row => GraphSession.CellOf(row.Id))))),
                edgeCuts: _ => new GraphResult.Severed(toSeq(rows.Select(static row => row.Id))),
                edgePartitions: _ => new GraphResult.Sundered(toSeq(rows.GroupBy(static row => row.Component).Select(static group => toSeq(group.Select(static row => row.Id))))));
            return Fin.Succ(result);
        }, static error => new CypherFault.RouteFailed(error));

    static IO<Fin<T>> Captured<T>(Func<System.Threading.Tasks.Task<Fin<T>>> crossing, Func<Error, CypherFault> arm) =>
        IO.liftAsync(async () => (await Try.lift(async _ => await crossing().ConfigureAwait(false)).Run().Bind(static inner => inner).ConfigureAwait(false))
            .MapFail(error => error.Exception.Case is NpgsqlException ? arm(error) : error));

    static async System.Threading.Tasks.Task<GraphResult> ReadRoute(NpgsqlCommand command) {
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        List<H3Cell> path = [];
        double cost = 0.0;
        while (await reader.ReadAsync().ConfigureAwait(false)) { path.Add(GraphSession.CellOf(reader.GetInt64(0))); cost = reader.GetDouble(1); }
        return new GraphResult.Routed(toSeq(path), cost);
    }

    static async System.Threading.Tasks.Task<GraphResult> ReadTour(NpgsqlCommand command) {
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        List<H3Cell> order = [];
        double cost = 0.0;
        while (await reader.ReadAsync().ConfigureAwait(false)) { order.Add(GraphSession.CellOf(reader.GetInt64(0))); cost = reader.GetDouble(1); }
        return new GraphResult.Toured(toSeq(order), cost);
    }
}
```

| [INDEX] | [POLICY]             | [VALUE]                                                 | [BINDING]                                             |
| :-----: | :------------------- | :------------------------------------------------------ | :---------------------------------------------------- |
|  [01]   | one verb surface     | `GraphQuery` `[Union]` + total `Switch`                 | every AGE + `pgrouting` verb; no sibling family       |
|  [02]   | parameterization     | `Identifier`+`CypherClause` gates; `params agtype` bind | body breakout unrepresentable; `Parameters` live      |
|  [03]   | `pgrouting` roster   | Path/Via/Located/Kth/Spread/Tour/TourPlanar/Flow/Cleave | variance in `RouteMode`/`FlowKind`/`CleaveKind` rows  |
|  [04]   | AGE read/write       | `Match` decode vs `Mutate` no-return                    | a mutate-and-return statement is rejected             |
|  [05]   | path decode          | `Reach` → `(p)::jsonb` cast → `DecodePath`              | `Vertices` walk + summed `Weight`; never a stub       |
|  [06]   | node-space agreement | `H3Cell` `long` ↔ in-process `pocketken.H3`             | in-db route + in-process path share node identity     |
|  [07]   | result spaces        | four typed `GraphResult` spaces (below)                 | spaces never conflate; an edge/Point never `CellOf`'d |
|  [08]   | cell→element resolve | `Element/identity#STORE_OPERATION_BRACKET` `Route`      | the caller's one-hop cell→element locator             |
|  [09]   | flow honesty         | `FlowKind` per-edge labeling; scalar derived            | a scalar-only `pgr_maxFlow` discards the labeling     |
|  [10]   | algorithm dedup      | server-side `pgr_*`/`age` vs in-process                 | meet at the shared node space, never re-implemented   |

- [07]-[RESULT_SPACES]: AGE → `KeySelection` (node); `pgrouting` NODE → `H3Cell` (cell); EDGE → `Severed`/`Sundered`/`Flowed`; a `Traced` negative vid stays a Point.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
