# [PERSISTENCE_QUERY_CYPHER]

Rasm.Persistence offers an OPTIONAL self-hosted-only graph-query lane over Apache AGE openCypher and PostGIS `pgrouting`, demoted beneath the in-process `Query/topology#GRAPH_TOPOLOGY` QuikGraph view (`H5`): the default authoritative topology owner is QuikGraph, and AGE is an analytical read projection a deployment enables only when one self-hosted PostgreSQL instance can host both AGE and the Marten event store. `CypherLane` drives AGE through raw `Npgsql` against the `agtype` result type (AGE ships no managed driver), running an `ag_catalog.cypher(graph, query)` path query over the async-projected graph and decoding the `agtype` rows into `NodeId`-keyed paths; `RoutingLane` drives `pgrouting` (`pgr_dijkstra`/`pgr_aStar`/`pgr_drivingDistance`) over the federated network for network/graph routing the in-process A* counterpart mirrors. Both are `ProjectionLifecycle.Async` analytical lanes carrying a `StalenessWatermark`, so an interactive-correctness query never routes here without `WaitForNonStaleProjectionDataAsync` and a clash/void-resolution read binds the synchronous topology instead. The async graph projection is rebuilt from the one Marten stream via the daemon. `NodeId` arrives from `Rasm.Element`; `ElementSet`/`StalenessWatermark`/`QueryLane` arrive from `Query/lane`; `Npgsql` arrives from the substrate; `ClockPolicy`, `ReceiptSinkPort` arrive from AppHost.

## [01]-[INDEX]

- [01]-[CYPHER_LANE]: the optional AGE openCypher path query over `agtype`, the enablement gate, and the async projection.
- [02]-[ROUTING_LANE]: the `pgrouting` network routing over the federated graph and its in-process A* mirror.

## [02]-[CYPHER_LANE]

- Owner: `CypherEnablement` the `[SmartEnum<string>]` lane-availability gate (`Disabled`/`SelfHosted`); `CypherQuery` the typed openCypher request carrying the graph name and the parameterized clause; `AgtypePath` the decoded `agtype` path result; `CypherFault` the closed AGE-boundary fault; `CypherLane` the static surface owning the enablement gate, the `agtype` query, and the path decode.
- Cases: `CypherEnablement` is `Disabled` (the default — the in-process QuikGraph view is authoritative) or `SelfHosted` (one PostgreSQL instance hosts AGE plus Marten); a `CypherQuery` carries the AGE graph name and a parameterized `MATCH … RETURN` clause; `AgtypePath` decodes a vertex/edge path into a `NodeId` sequence.
- Entry: `public static IO<Fin<Seq<AgtypePath>>> Match(NpgsqlDataSource source, CypherEnablement gate, CypherQuery query)` runs the `cypher()` path query when the lane is `SelfHosted` and returns `CypherFault.Disabled` otherwise; `public static ElementSet ToSet(Seq<AgtypePath> paths)` projects the decoded paths to the selection currency.
- Auto: the lane is `Disabled` by default so a deployment that has not provisioned AGE reads the in-process topology and never reaches for a missing extension; when `SelfHosted`, the query runs `SELECT * FROM ag_catalog.cypher('<graph>', $$ MATCH … RETURN … $$) AS (result agtype)` through raw `Npgsql` (AGE has no EF/managed driver), binding parameters through the AGE parameter mechanism and decoding the `agtype` rows; the AGE graph is an async daemon projection rebuilt from the one Marten stream so it never duplicates the event store.
- Receipt: a cypher query rides `store.cypher.match` carrying the path count and the watermark; a disabled-lane probe rides `store.cypher.disabled`.
- Packages: Npgsql, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new query shape is one `CypherQuery` projection; a new enablement tier is one `CypherEnablement` row; zero new surface — making AGE the default authoritative topology, hosting it in the same instance as Marten without the gate, or a managed AGE driver wrapper is the deleted form because QuikGraph is the default and AGE is the optional self-hosted analytical lane driven through raw Npgsql.
- Boundary: AGE is an OPTIONAL self-hosted-only read projection (`H5`), demoted beneath the in-process QuikGraph view which is the default authoritative topology owner — a deployment never ASSUMES one PostgreSQL instance hosts both AGE and Marten-9/Npgsql-10, so the lane gates on `CypherEnablement.SelfHosted` and falls back to the in-process topology when disabled; AGE is the `Query/lane#READ_ROUTING` ASYNC analytical lane carrying a `StalenessWatermark`, so an interactive-correctness query (clash, void-resolution) binds the synchronous topology and never reads the AGE projection without `WaitForNonStaleProjectionDataAsync`; AGE is driven through raw `Npgsql` against the `agtype` result type because it ships no managed/EF driver — a managed AGE wrapper is the deleted form; the AGE graph is an async daemon projection off the one Marten stream so it is rebuildable and never a second event store.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CypherEnablement {
    public static readonly CypherEnablement Disabled = new("disabled");
    public static readonly CypherEnablement SelfHosted = new("self-hosted");
}

public readonly record struct CypherQuery(string Graph, string Clause, HashMap<string, string> Parameters);
public readonly record struct AgtypePath(Seq<NodeId> Vertices, double Weight);

[Union]
public abstract partial record CypherFault : Expected, IValidationError<CypherFault> {
    private CypherFault(string detail, int code) : base(detail, code, None) { }
    public static CypherFault Create(string message) => new MatchFailed(message);
    public sealed record Disabled : CypherFault("<cypher-disabled>", 8360);
    public sealed record MatchFailed(string Detail) : CypherFault($"<cypher-match:{Detail}>", 8361);
}

public static class CypherLane {
    public static IO<Fin<Seq<AgtypePath>>> Match(NpgsqlDataSource source, CypherEnablement gate, CypherQuery query) =>
        gate == CypherEnablement.Disabled
            ? IO.pure(Fin<Seq<AgtypePath>>.Fail(new CypherFault.Disabled()))
            : IO.liftAsync(async () => {
                await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
                await using var load = new NpgsqlCommand("LOAD 'age'; SET search_path = ag_catalog, \"$user\", public;", connection);
                await load.ExecuteNonQueryAsync().ConfigureAwait(false);
                await using var command = new NpgsqlCommand($"SELECT * FROM ag_catalog.cypher(@graph, $${query.Clause}$$) AS (result agtype)", connection);
                command.Parameters.AddWithValue("graph", query.Graph);
                await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                var paths = Seq<AgtypePath>();
                while (await reader.ReadAsync().ConfigureAwait(false)) paths = paths.Add(DecodePath(reader.GetString(0)));
                return Fin<Seq<AgtypePath>>.Succ(paths);
            }) | @catch<IO, Fin<Seq<AgtypePath>>>(static _ => true, e => IO.pure(Fin<Seq<AgtypePath>>.Fail(new CypherFault.MatchFailed(e.Message))));

    public static ElementSet ToSet(Seq<AgtypePath> paths) => ElementSet.Of(paths.Bind(static p => p.Vertices));

    static AgtypePath DecodePath(string agtype) =>
        new(toSeq(agtype.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Where(static s => s.StartsWith("id:", StringComparison.Ordinal)).Select(static s => NodeId.Create(s[3..]))), 0.0);
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | enablement          | `Disabled` default, `SelfHosted` opt   | QuikGraph is authoritative; AGE is optional (`H5`)         |
|  [02]   | driver              | raw `Npgsql` over `agtype`             | no managed AGE driver; never an EF wrapper                 |
|  [03]   | consistency stance  | async, `StalenessWatermark`            | interactive correctness binds the synchronous topology    |
|  [04]   | graph source        | async daemon projection off Marten     | rebuildable, never a second event store                   |

## [03]-[ROUTING_LANE]

- Owner: `RoutingQuery` the typed `pgrouting` request; `RouteResult` the decoded path-with-cost; `RoutingLane` the static surface owning the `pgr_dijkstra`/`pgr_aStar`/`pgr_drivingDistance` query over the federated network.
- Cases: `Dijkstra(from, to)`, `AStar(from, to)`, `DrivingDistance(from, radius)` on `RoutingQuery` — the three `pgrouting` verbs over the network edge table the federated graph projects.
- Entry: `public static IO<Fin<RouteResult>> Route(NpgsqlDataSource source, RoutingQuery query)` runs the `pgrouting` function and decodes the path-with-cost; the result projects to the `Query/lane#ELEMENT_SET_ALGEBRA` selection currency.
- Auto: the routing lane runs `pgr_dijkstra`/`pgr_aStar`/`pgr_drivingDistance` over a network edge table projected from the federated graph's connection relationships, so a network routing query (a duct run, a egress path, a circulation route) executes server-side over the indexed network; the in-process `Query/topology#TRAVERSAL` A* `Path` query is the synchronous counterpart so an interactive routing read binds the in-process view and the analytical bulk routing binds `pgrouting`, the two meeting at the result `ElementSet`.
- Receipt: a routing query rides `store.routing.<verb>` carrying the path length and the cost.
- Packages: Npgsql, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new routing verb is one `RoutingQuery` case plus one `pgrouting` function; zero new surface — a hand-rolled Dijkstra over the network table, a second routing engine, or a routing result that is not an `ElementSet` is the deleted form because `pgrouting` owns the server-side routing and the in-process A* mirror owns the synchronous path, meeting at the `ElementSet`.
- Boundary: `pgrouting` is the server-side network routing over the federated connection graph (the GEO_LANES routing capability via raw SQL), and the in-process `Query/topology#TRAVERSAL` A* `Path` is its synchronous interactive counterpart — the analytical bulk routing binds `pgrouting`, the interactive routing binds the in-process view, the two meeting at the result `ElementSet` and never duplicating the algorithm; the network edge table is projected from the federated graph's connection relationships so a routing query rides the one graph, never a second network store.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RoutingQuery {
    private RoutingQuery() { }
    public sealed record Dijkstra(NodeId From, NodeId To) : RoutingQuery;
    public sealed record AStar(NodeId From, NodeId To) : RoutingQuery;
    public sealed record DrivingDistance(NodeId From, double Radius) : RoutingQuery;
}

public readonly record struct RouteResult(Seq<NodeId> Path, double Cost, ElementSet Reached);

public static class RoutingLane {
    public static IO<Fin<RouteResult>> Route(NpgsqlDataSource source, RoutingQuery query) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            var sql = query switch {
                RoutingQuery.Dijkstra => "SELECT node, agg_cost FROM pgr_dijkstra('SELECT id, source, target, cost FROM network_edge', @from::bigint, @to::bigint)",
                RoutingQuery.AStar => "SELECT node, agg_cost FROM pgr_aStar('SELECT id, source, target, cost, x1, y1, x2, y2 FROM network_edge', @from::bigint, @to::bigint)",
                _ => "SELECT node, agg_cost FROM pgr_drivingDistance('SELECT id, source, target, cost FROM network_edge', @from::bigint, @radius::float)",
            };
            await using var command = new NpgsqlCommand(sql, connection);
            Bind(command, query);
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var (path, cost) = (Seq<NodeId>(), 0.0);
            while (await reader.ReadAsync().ConfigureAwait(false)) { path = path.Add(NodeId.Create(reader.GetInt64(0).ToString(CultureInfo.InvariantCulture))); cost = reader.GetDouble(1); }
            return Fin<RouteResult>.Succ(new RouteResult(path, cost, ElementSet.Of(path)));
        }) | @catch<IO, Fin<RouteResult>>(static _ => true, e => IO.pure(Fin<RouteResult>.Fail(Error.New(8362, $"<routing:{e.Message}>"))));

    static void Bind(NpgsqlCommand command, RoutingQuery query) {
        switch (query) {
            case RoutingQuery.Dijkstra d: command.Parameters.AddWithValue("from", (long)d.From.GetHashCode()); command.Parameters.AddWithValue("to", (long)d.To.GetHashCode()); break;
            case RoutingQuery.AStar a: command.Parameters.AddWithValue("from", (long)a.From.GetHashCode()); command.Parameters.AddWithValue("to", (long)a.To.GetHashCode()); break;
            case RoutingQuery.DrivingDistance dd: command.Parameters.AddWithValue("from", (long)dd.From.GetHashCode()); command.Parameters.AddWithValue("radius", dd.Radius); break;
        }
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | server routing      | `pgr_dijkstra`/`pgr_aStar`/`pgr_drivingDistance` | over the federated connection network              |
|  [02]   | interactive mirror  | in-process A* `Path`                    | the synchronous counterpart; meet at the `ElementSet`     |
|  [03]   | network source      | projected connection relationships     | rides the one graph, never a second network store         |
