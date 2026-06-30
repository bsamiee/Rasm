# [PERSISTENCE_QUERY_CYPHER]

Rasm.Persistence offers an OPTIONAL self-hosted-only graph lane over the two server-side PostgreSQL graph extensions — Apache AGE openCypher and PostGIS `pgrouting` — both demoted beneath the in-process `Query/topology#GRAPH_TOPOLOGY` QuikGraph view (`H5`): QuikGraph is the DEFAULT authoritative topology owner and these are ASYNC analytical read projections a deployment enables only where one self-hosted PostgreSQL instance can host both the extension and the Marten event store, so an interactive-correctness query (clash, void-resolution, live QTO) NEVER routes here without `WaitForNonStaleProjectionDataAsync` and binds the synchronous topology instead. Both extensions carry NO managed assembly: every surface is server-side SQL the `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension` `CreateSql` (committed on the EF migration rail, `Element/identity#SCHEMA_VERDICT`) installs and this lane drives through raw `Npgsql`, and one `GraphSession` owns the per-connection `LOAD 'age'`+`search_path` init through a `NpgsqlDataSource` physical-connection hook so the session-load obligation is paid once per physical connection rather than re-issued per query. ONE `GraphQuery` `[Union]` collapses every openCypher verb (read `Match`, mutating `Mutate`, variable-length `Reach`) AND every `pgrouting` verb (`Path` over Dijkstra/A*/bidirectional, `Spread` over driving-distance, `Tour` over the metric-TSP-over-an-in-database-cost-matrix, `Flow` over the max-flow family, `Cleave` over the component/articulation/cut-edge family) under one total generated `Switch`, parameter-bound through `params agtype` and the Edges-SQL `Npgsql` parameters — never a runtime-concatenated graph body and never a hand-parsed text decode. The graph node space is the `Element/identity#ELEMENT_IDENTITY` `H3Cell` (`[ValueObject<long>]`, the `h3-pg` bit-exact convention) for the `pgrouting` Edges-SQL `bigint` vertex ids, and the seam `NodeId.Value` string for the AGE vertex `properties.id` decoded through the registered `agtype ->>` operator; a process-randomized `GetHashCode` as a persisted vertex id is the named `boundaries.md#MEMO_KEY` defect this page deletes. The `pgrouting` `Path`/A* (the geometry-weighted metric route over the `H3Cell` cell-mesh) is the ANALYTICAL counterpart to the SYNCHRONOUS in-process `Query/topology#TRAVERSAL` `Path` (`ShortestPathsDijkstra` unit-weight TOPOLOGICAL distance over the seam graph, NO node coordinate) — the two own DIFFERENT distances (metric vs topological) and meet only at the result `H3Cell` mesh / `ElementSet`, neither mirroring the other's algorithm; the in-process topology lane holds NO `pocketken.H3` cell-path member, and the `H3Cell`→element hop is the one-hop `Element/identity#ELEMENT_IDENTITY` `NodeAt`. Every boundary failure rails one closed `GraphFault` `[Union]` over `Expected` (no bare `Error.New`), and every lane outcome rides a `ReceiptSinkPort` fact the composing `Query/lane#READ_ROUTING` router emits. `NodeId` arrives from `Rasm.Element`; `ElementSet`/`StalenessWatermark`/`QueryLane`/`ReadRouter`/`H3Cell` arrive from `Query/lane`/`Element/identity`; `H3Index` arrives from `pocketken.H3`; `NpgsqlDataSource`/`agtype` arrive from the substrate; `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId`/`TenantContext` arrive from AppHost.

## [01]-[INDEX]

- [01]-[GRAPH_SESSION]: the `CypherEnablement` self-hosted gate, the `GraphFault` closed boundary fault, and the `GraphSession` owning the per-physical-connection `LOAD 'age'`+`search_path` init, the async daemon projection, and the `agtype`/Edges-SQL decode primitives both lanes compose.
- [02]-[GRAPH_QUERY]: the ONE `GraphQuery` `[Union]` collapsing every AGE openCypher and `pgrouting` verb, the `GraphResult` typed carrier, the `H3Cell` vertex-id node space, and the total-`Switch` `Run` that lowers each case to its server-side SQL and decodes its rows into the NODE-space `ElementSet` (AGE) or the CELL-space `H3Cell` mesh (`pgrouting`).

## [02]-[GRAPH_SESSION]

- Owner: `CypherEnablement` the `[SmartEnum<string>]` lane-availability gate (`Disabled`/`SelfHosted`) carrying whether the lane reaches the server extensions; `GraphFault` the closed AGE/`pgrouting`-boundary fault `[Union]` over `Expected`; `GraphSession` the static surface owning the `NpgsqlDataSource` physical-connection init hook, the async daemon projection rebuild, the `agtype` scalar decode through the registered SQL casts, and the Edges-SQL `bigint` node-space projection both lanes compose.
- Cases: `CypherEnablement` is `Disabled` (the default — the in-process QuikGraph view is authoritative and no connection ever reaches for `age`/`pgrouting`) or `SelfHosted` (one PostgreSQL instance hosts the extension PLUS Marten); `GraphFault` is `Disabled` (the lane was reached while gated off), `MatchFailed` (an AGE `cypher()` boundary fault), `RouteFailed` (a `pgrouting` boundary fault), and `Unresolvable` (a vertex with no `H3Cell` / a `NodeId` property absent from the decoded `agtype`) so the cause stays structurally addressable rather than one provider token.
- Entry: `public static NpgsqlDataSourceBuilder Provision(NpgsqlDataSourceBuilder builder, CypherEnablement gate)` installs the per-physical-connection `LOAD 'age'; SET search_path = ag_catalog, "$user", public;` init through `UsePhysicalConnectionInitializer` ONLY when `SelfHosted` so the session-load obligation is paid once per physical connection (`api-apache-age#SESSION_SETUP`), never per query; `public static IO<Unit> Rebuild(IDocumentStore store, Duration timeout)` rebuilds the async graph projection from the one Marten stream via the daemon and blocks for non-stale on demand; `public static Fin<NodeId> Decode(string? extractedId)` admits the seam `NodeId` from the vertex `properties.id` the Cypher `RETURN` already extracted SERVER-SIDE through the registered `agtype ->`/`->>` operators (`GraphFault.Unresolvable` on a NULL column), never a client-side comma-split or whole-vertex parse; `public static long Vid(H3Cell cell)` is the `pgrouting` `bigint` node-space projection (the `h3-pg` `long` cell), and `public static H3Cell CellOf(long vid)` its inverse.
- Auto: the lane is `Disabled` by construction so a deployment that has not provisioned `age`/`pgrouting` reads the in-process topology and never reaches for a missing extension; when `SelfHosted`, `Provision` registers the connection-init hook so every physical connection runs `LOAD 'age'` and sets `search_path` once at open (a PL/pgSQL Cypher wrapper repeats it in its own body per `api-apache-age#SESSION_SETUP`, but the managed lane pays it at the physical-connection seam, not per `cypher()` call); the AGE backing relations and the `pgrouting` network edge table are async daemon projections (`ProjectionLifecycle.Async`) rebuilt from the one Marten stream so neither duplicates the event store; the Cypher read's OUTER `SELECT` extracts `(v->'properties'->>'id')` server-side through the registered `agtype` operators so `Decode` receives a PLAIN text column (the seam `NodeId.Value` the projection wrote as the vertex `id` property), making the decoded node the REAL seam id, never the AGE internal `graphid` integer and never a client-side hand-parse; `Vid` reinterprets the `H3Cell` `long` (the `h3-pg` cell convention `Element/identity#ELEMENT_IDENTITY` already stamps) so the `pgrouting` Edges-SQL vertex id, the GiST/BRIN H3 index, and the in-process `pocketken.H3` cell agree bit-for-bit.
- Receipt: every lane outcome rides the `ReceiptSinkPort` — a session provision rides `store.graph.provision` carrying the gate; a daemon rebuild rides `store.graph.rebuild` carrying the watermark and elapsed wait.
- Packages: Npgsql (`NpgsqlDataSourceBuilder.UsePhysicalConnectionInitializer`/`NpgsqlDataSource`/`NpgsqlConnection`), Marten (`IDocumentStore.BuildProjectionDaemonAsync`/`WaitForNonStaleProjectionDataAsync`), pocketken.H3 (`H3Index`), Rasm.Element (`NodeId`), Rasm.Persistence (`Element/identity#ELEMENT_IDENTITY` `H3Cell`, `Query/lane#READ_ROUTING` `StalenessWatermark`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.Text.Json, BCL inbox.
- Note: the `ReceiptSinkPort` envelope is emitted by the composing `Query/lane#READ_ROUTING` rail that routed the read (the lane primitives stay receipt-free, exactly as the sibling `Query/topology`/`Query/columnar` read lanes), so the receipt-ride below names the fact the router emits, not a sink call inside these primitives.
- Growth: a new enablement tier is one `CypherEnablement` row; a new boundary cause is one `GraphFault` case; zero new surface — making AGE/`pgrouting` the default authoritative topology, hosting it in the same instance as Marten without the gate, a managed AGE/`pgrouting` driver wrapper, a per-query `LOAD 'age'`, a comma-split `agtype` decode, or a `GetHashCode`-derived vertex id is the deleted form because QuikGraph is the default, the extensions are optional self-hosted analytical lanes driven through raw `Npgsql` against the registered `agtype` casts, the session-load is a physical-connection hook, and the node space is the `h3-pg` `H3Cell`.
- Boundary: AGE and `pgrouting` are OPTIONAL self-hosted-only read projections (`H5`) demoted beneath the in-process QuikGraph view which is the default authoritative topology owner — a deployment never ASSUMES one PostgreSQL instance hosts both an extension and Marten-9/Npgsql-10, so the lane gates on `CypherEnablement.SelfHosted` and falls back to the in-process topology when disabled; both are the `Query/lane#READ_ROUTING` ASYNC analytical lanes carrying a `StalenessWatermark`, so an interactive-correctness query (clash, void-resolution, live QTO) binds the synchronous topology and never reads here without `WaitForNonStaleProjectionDataAsync`; both are driven through raw `Npgsql` against the server-side SQL because neither ships a managed/EF driver, and the mandatory `AS (col agtype, ...)` / `AS (seq integer, ...)` column-definition list is required by PostgreSQL's `RETURNS SETOF record` contract, never optional (`api-apache-age#CYPHER_QUERY`, `api-pgrouting#ROUTING_FUNCTIONS`); the AGE backing relations and the `pgrouting` network edge table are async daemon projections off the one Marten stream so both are rebuildable and never a second event store; the session-load (`LOAD 'age'`+`search_path`) is a physical-connection-init obligation the `Npgsql` open-hook pays once per connection, never a per-query statement that re-loads the shared library; the AGE vertex `id` decodes to the REAL seam `NodeId` through `properties.id` (the projection wrote the seam id as a vertex property) via the registered `agtype ->>` operator, and the `pgrouting` vertex id is the `Element/identity#ELEMENT_IDENTITY` `H3Cell` `long` (the `h3-pg` convention) the in-process `pocketken.H3` cell shares — a `graphid` integer surfacing as a `NodeId`, a comma-split text decode, or a `GetHashCode`-derived `bigint` vertex id is the named `boundaries.md#MEMO_KEY` defect this page deletes.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Npgsql;
using Rasm.Element;
using Thinktecture;
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — the alias wins over LanguageExt.Common.Expected for the bare name
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CypherEnablement {
    public static readonly CypherEnablement Disabled = new("disabled", reaches: false);
    public static readonly CypherEnablement SelfHosted = new("self-hosted", reaches: true);
    public bool Reaches { get; }
    private CypherEnablement(string key, bool reaches) : this(key) => Reaches = reaches;
}

// --- [ERRORS] -----------------------------------------------------------------------------
// The closed AGE/pgrouting boundary fault band (836x): a [Union] over the KERNEL `Rasm.Domain.Expected`
// (parameterless protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation
// base the seam `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND`
// `BimFault` (2600) realize — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)`
// ctor (no `Category` to override) is the deleted form. `[SkipUnionOps]` skips the generated implicit-conversion ops
// while the generated `Switch`/`Map` survives; band membership is a per-case `Code => 836x` override, `Message`
// projects the addressable cause, `Category` the telemetry label, so a recovery routes `error.IsType<GraphFault.RouteFailed>()`
// / `error.HasCode(8362)` / `error.Category()`, never a stringified provider token, and the bare case lifts onto
// `Fin<T>` with no `.ToError()` hop.
[SkipUnionOps]
[Union]
public abstract partial record GraphFault : Expected, IValidationError<GraphFault> {
    private GraphFault() : base() { }
    public sealed record Disabled : GraphFault;
    public sealed record MatchFailed(string Detail) : GraphFault;
    public sealed record RouteFailed(string Detail) : GraphFault;
    public sealed record Unresolvable(string Detail) : GraphFault;

    public override int Code => Switch(
        disabled:     static _ => 8360,
        matchFailed:  static _ => 8361,
        routeFailed:  static _ => 8362,
        unresolvable: static _ => 8363);

    public override string Message => Switch(
        disabled:     static _ => "<graph-lane-disabled>",
        matchFailed:  static c => $"<cypher-match:{c.Detail}>",
        routeFailed:  static c => $"<pgr-route:{c.Detail}>",
        unresolvable: static c => $"<graph-vertex-unresolvable:{c.Detail}>");

    public override string Category => Switch(
        disabled:     static _ => "Disabled",
        matchFailed:  static _ => "Match",
        routeFailed:  static _ => "Route",
        unresolvable: static _ => "Vertex");

    public static GraphFault Create(string message) => new MatchFailed(message);
}
```

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
public static class GraphSession {
    // The per-PHYSICAL-CONNECTION session-load hook (api-apache-age#SESSION_SETUP): `LOAD 'age'` loads the shared
    // library into the backend and the search_path resolves the unqualified cypher/agtype symbols, both ONCE at open
    // rather than re-issued per query (a per-query LOAD is the deleted form). Installed only when SelfHosted so a
    // Disabled deployment never runs it; pgrouting needs no session-load (its pgr_* functions are public-schema).
    public static NpgsqlDataSourceBuilder Provision(NpgsqlDataSourceBuilder builder, CypherEnablement gate) =>
        gate.Reaches
            ? builder.UsePhysicalConnectionInitializer(
                connection => {
                    using var load = connection.CreateCommand();
                    load.CommandText = SessionLoad;
                    load.ExecuteNonQuery();
                },
                async connection => {
                    await using var load = connection.CreateCommand();
                    load.CommandText = SessionLoad;
                    await load.ExecuteNonQueryAsync().ConfigureAwait(false);
                })
            : builder;

    const string SessionLoad = "LOAD 'age'; SET search_path = ag_catalog, \"$user\", public;";

    // The async graph projection (AGE backing relations + pgrouting network edge table) is rebuilt from the ONE Marten
    // stream via the daemon (never a second event store); an interactive-correctness consumer blocks for non-stale here
    // before reading, but by construction routes to the synchronous topology instead (Query/lane#READ_ROUTING, C2/H5).
    public static IO<Unit> Rebuild(IDocumentStore store, Duration timeout) =>
        IO.liftAsync(async () => {
            await using var daemon = await store.BuildProjectionDaemonAsync().ConfigureAwait(false);
            await daemon.StartAllAsync().ConfigureAwait(false);
            await store.WaitForNonStaleProjectionDataAsync(timeout.ToTimeSpan()).ConfigureAwait(false);
            return unit;
        });

    // Decode ONE seam NodeId from the AGE vertex `properties.id` the Cypher RETURN already extracted SERVER-SIDE through the
    // registered agtype `->` / `->>` operators (`(v->'properties'->>'id')`, api-apache-age#AGTYPE), so the reader receives a
    // PLAIN text column — never a hand-parsed comma-split of the whole vertex body, never the AGE internal `graphid` integer.
    // The projection wrote the seam NodeId.Value as the vertex `id` property, so the extracted string IS the durable seam id;
    // a NULL column (the property absent) rails GraphFault.Unresolvable rather than fabricating an id.
    public static Fin<NodeId> Decode(string? extractedId) =>
        extractedId is { } value ? Fin.Succ(NodeId.Create(value)) : Fin.Fail<NodeId>(new GraphFault.Unresolvable("<vertex-id-absent>"));

    // The pgrouting bigint node space IS the Element/identity#ELEMENT_IDENTITY H3Cell `long` (the h3-pg convention), so the
    // Edges-SQL vertex id, the GiST/BRIN H3 index, and the in-process pocketken.H3 cell agree bit-for-bit (api-pgrouting#GEO_LANE_STACK).
    public static long Vid(H3Cell cell) => cell.Value;
    public static H3Cell CellOf(long vid) => H3Cell.Create(vid);
}
```

| [INDEX] | [POLICY]            | [VALUE]                                   | [BINDING]                                                  |
| :-----: | :------------------ | :---------------------------------------- | :-------------------------------------------------------- |
|  [01]   | enablement          | `Disabled` default, `SelfHosted` opt      | QuikGraph is authoritative; extensions optional (`H5`)     |
|  [02]   | driver              | raw `Npgsql` over `agtype`/`SETOF record` | no managed AGE/`pgrouting` driver; never EF                |
|  [03]   | session-load        | `UsePhysicalConnectionInitializer`        | `LOAD 'age'`+`search_path` once per connection, never per query |
|  [04]   | AGE node id         | `properties.id` → seam `NodeId`           | the real seam id; never the `graphid` integer or comma-split |
|  [05]   | `pgrouting` node id | the `h3-pg` `H3Cell` `long`               | shared with the in-process `pocketken.H3` cell; never `GetHashCode` |
|  [06]   | consistency stance  | async, `StalenessWatermark`               | interactive correctness binds the synchronous topology    |
|  [07]   | graph source        | async daemon projection off Marten        | rebuildable, never a second event store                   |
|  [08]   | fault rail          | `GraphFault` `[Union]` over `Expected`    | every cause structurally addressable; never `Error.New`    |

## [03]-[GRAPH_QUERY]

- Owner: `GraphQuery` the ONE `[Union]` collapsing every AGE openCypher verb (`Match`/`Mutate`/`Reach`) AND every `pgrouting` verb (`Path`/`Kth`/`Spread`/`Tour`/`Flow`/`Cleave`) under one total generated `Switch`; `RouteMode` the `[SmartEnum<string>]` selecting the `pgrouting` path function (Dijkstra/A*/bidirectional) the `Path` case carries; `CleaveKind` the `[SmartEnum<string>]` selecting the connectivity function the `Cleave` case carries; `GraphResult` the typed `[Union]` carrier (the NODE-space `Paths`, the CELL-space `Routed`/`Ranked`/`Spanned`/`Toured`/`Cleaved`, the scalar `Flowed`, the cut-EDGE-id `Severed`); `GraphLane` the static surface owning the gate-checked lowering, the parameterized `cypher()`/`pgr_*` SQL, the server-side-extracted `agtype`/`SETOF record` decode, the `ToSet` node-space and `ToCells` cell-space projections.
- Cases: on `GraphQuery` — `Match(graph, clause, parameters)` (a read `MATCH … RETURN` over the AGE graph, parameter-bound through `params agtype`), `Mutate(graph, clause, parameters)` (a `CREATE`/`SET`/`DELETE` Cypher statement — the write half AGE owns, never folded with a trailing `RETURN` in one statement per `api-apache-age#CYPHER_QUERY`), `Reach(graph, from, minHops, maxHops)` (a bounded variable-length `*minHops..maxHops` traversal the AGE `age_vle` engine drives), `Path(edges, from, to, mode)` (a `pgrouting` shortest path over Dijkstra/A*/bidirectional selected by `RouteMode`), `Kth(edges, from, to, k)` (a `pgr_KSP` K-shortest distinct alternatives, `path_id`-ascending), `Spread(edges, root, radius)` (a `pgr_drivingDistance` reachability spanning-tree), `Tour(edges, stops, start, end)` (a `pgr_TSP` metric tour whose cost matrix is the in-database `pgr_dijkstraCostMatrix` over the same `edges` + the `stops` vid set, composed server-side through PostgreSQL `format('%L', %L::bigint[])` — the bound Edges-SQL and the `@vids::text` array both literal-quoted, the array re-cast `::bigint[]` — so they inject injection-safely into the matrix-SQL text pgRouting re-parses, never a C#-side concatenated matrix string), `Flow(edges, source, sink)` (a `pgr_maxFlow` over the `capacity` edge contract), `Cleave(edges, kind)` (a `pgr_strongComponents`/`pgr_connectedComponents`/`pgr_articulationPoints`/`pgr_bridges` connectivity partition); on `RouteMode` — `Dijkstra`/`AStar`/`Bidirectional` carrying the `pgr_*` function name; on `CleaveKind` — `Strong`/`Connected`/`Articulation`/`Bridges` carrying the connectivity function name AND a `NodeValued` decode discriminant (node-valued for components + articulation cut-vertices, edge-valued for `pgr_bridges` cut-edges); on `GraphResult` — `Paths` (the NODE-space decoded AGE `agtype` paths, real seam `NodeId`s), `Routed`/`Spanned`/`Toured` (the CELL-space `H3Cell` sequences the navigable cell mesh walks), `Ranked` (the CELL-space `path_id`-ascending K-shortest alternatives), `Cleaved` (the CELL-space `H3Cell` NODE-partition set — components + articulation cut-vertices), `Severed` (the raw cut-EDGE `long` ids `pgr_bridges` returns, NEVER fabricated into cells), `Flowed` (a scalar max-flow value).
- Entry: `public static IO<Fin<GraphResult>> Run(NpgsqlDataSource source, CypherEnablement gate, GraphQuery query)` is the ONE entrypoint — it short-circuits `GraphFault.Disabled` when the lane is gated off, else dispatches the total `Switch` lowering each case to its parameterized server-side SQL and decoding its rows; `public static ElementSet ToSet(GraphResult result)` projects the NODE-space AGE result to the `Query/lane#ELEMENT_SET_ALGEBRA` selection currency, and `public static Seq<H3Cell> ToCells(GraphResult result)` exposes the CELL-space `pgrouting` results a caller resolves to elements one-hop through `Element/identity#ELEMENT_IDENTITY` `NodeAt(cell)` (the per-element fine-cell resolver, not the coarse model-grain `Nearby`) — the two spaces never conflate and a cell is never fabricated into a `NodeId`.
- Auto: `Run` gates ONCE on `CypherEnablement.Reaches` so a disabled lane never opens a connection; an AGE case binds the graph name, the Cypher body, and the `params agtype` through `Npgsql` parameters and runs `SELECT * FROM ag_catalog.cypher(@graph, $$ … $$, @params) AS (result agtype)` with the mandatory column-definition list, decoding each `agtype` vertex through `GraphSession.Decode` (the registered cast, never a hand-parse) — `Mutate` runs the statement with no `RETURN` column and `Reach` lowers the `minHops..maxHops` into the Cypher `*` range the `age_vle` engine plans; a `pgrouting` case binds the Edges-SQL string and the `H3Cell`-derived `bigint` vids (`GraphSession.Vid`) through `Npgsql` parameters and runs the `pgr_*` function with the PATH/SPANTREE/COST/FLOW column-definition list (`api-pgrouting#ROUTING_FUNCTIONS`), reading a NODE `bigint` column back through `GraphSession.CellOf` into a CELL-space `H3Cell` sequence — the geometry-weighted METRIC route this lane owns, the ANALYTICAL counterpart to the in-process `Query/topology#TRAVERSAL` `Path` (`ShortestPathsDijkstra` unit-weight TOPOLOGICAL distance over the seam graph, a DIFFERENT distance, NOT a `pocketken.H3` cell-path the in-process lane does not hold) — which a caller resolves to elements one-hop through `Element/identity#ELEMENT_IDENTITY` `NodeAt` (a cell is the per-model envelope locator, never an element `NodeId`); `Tour` composes its `pgr_TSP` matrix-SQL server-side via `format('SELECT * FROM pgr_dijkstraCostMatrix(%L, %L::bigint[], directed => false)', @edges, @vids::text)` — pgRouting re-parses the matrix-SQL text so an `@edges`/`@vids` Npgsql parameter cannot bind inside it, and `format` with `%L` for BOTH the Edges-SQL and the `@vids::text` array (literal-quoted then re-cast `::bigint[]`) is the injection-safe server-side composition (a bare `%s` would inject the unquoted `{1,2,3}` array OUTPUT form that is never valid array INPUT syntax) rather than a C#-side concat; `Cleave` routes its decode on `CleaveKind.NodeValued` — the component + `pgr_articulationPoints` cut-VERTEX legs decode NODE `bigint` to cells, but the `pgr_bridges` cut-EDGE leg reads its `SETOF bigint` of EDGE ids onto `Severed` as raw `long`s, NEVER through `CellOf` (a cut-edge id is an edge, not a cell — the fabricated-cell defect); both legs lift any `PostgresException` through the `@catch` boundary into the typed `GraphFault` (`MatchFailed`/`RouteFailed`), and the A* mode supplies the xy Edges-SQL the `pgr_aStar` heuristic requires.
- Receipt: every run rides the `ReceiptSinkPort` — an AGE read rides `store.graph.match` carrying the path count and watermark, an AGE mutation `store.graph.mutate` carrying the mutated-graph name (AGE's `cypher()` mutation is a `SETOF record` whose `ExecuteNonQuery` row count is not a reliable affected-row signal, so the mutation fact rides the graph identity, never a fabricated count), a route `store.graph.route` carrying the reached count and the aggregate cost, a partition `store.graph.cleave` carrying the component count.
- Packages: Npgsql (`NpgsqlDataSource.OpenConnectionAsync`/`NpgsqlCommand`/`NpgsqlParameterCollection.AddWithValue`/`NpgsqlDataReader`/`PostgresException`), Rasm.Element (`NodeId`), Rasm.Persistence (`Element/identity#ELEMENT_IDENTITY` `H3Cell`, `Query/lane#ELEMENT_SET_ALGEBRA` `ElementSet`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new openCypher or `pgrouting` verb is one `GraphQuery` case plus one `Switch` arm lowering to its server-side SQL; a new path function is one `RouteMode` row carrying the `pgr_*` name; a new result shape is one `GraphResult` case; zero new surface — a hand-rolled Dijkstra/BFS over the network table, a second routing engine beside `pgrouting`, a managed graph engine beside `age`, a runtime-concatenated Cypher/Edges-SQL body, a `cypher()` call without the column-definition list, a mutate-and-return in one Cypher statement, a fabricated `NodeId` from a cell string, a cut-EDGE id fed through `CellOf` into a phantom cell, or a result outside the typed `GraphResult` is the deleted form because the extensions own the server-side algorithm, the in-process `Query/topology#TRAVERSAL` QuikGraph lane owns the synchronous TOPOLOGICAL counterpart (unit-weight distance over the seam graph — a different distance, not a mirror of the `pgrouting` metric route), the AGE result composes at the `ElementSet`, the `pgrouting` NODE result at the `H3Cell` mesh, and the `pgr_bridges` EDGE result at the raw `Severed` edge ids.
- Boundary: `GraphQuery` is the ONE polymorphic verb surface so every AGE and `pgrouting` capability is one case under one total `Switch` — a sibling `Match`/`Route`/`DrivingDistance` method family, a `bool`/`mode` flag selecting a verb, or a thin 3-verb slice of the rich `pgrouting` roster (Dijkstra/A*/bidirectional/KSP/TSP/flow/component) is the deleted form, `RouteMode` carrying the path-function variance and the verb family carrying the rest; the parameterized form is mandatory — the Cypher body binds `params agtype` (`$name`) and the Edges-SQL binds the `bigint` vids through `Npgsql` parameters, so a runtime-concatenated graph body is the rejected `api-apache-age`/`api-pgrouting` form and the `Match`/`Mutate`/`Reach` `Parameters` are LIVE, never a dead field; the `agtype` rows decode through `GraphSession.Decode` (the registered `->>`/`::text` cast) so a returned vertex projects to the real seam `NodeId`, never the internal `graphid` integer and never a comma-split; the `pgrouting` NODE `bigint` is the `H3Cell` the in-process `pocketken.H3` cell shares so the in-database route and the in-process path agree on node identity (`api-pgrouting#GEO_LANE_STACK`), the analytical bulk routing binding `pgrouting` and the interactive routing binding the in-process `Query/topology#TRAVERSAL` `Path`, the two meeting at the result `H3Cell` mesh and never duplicating the algorithm, the cell→element resolution a one-hop `Element/identity#ELEMENT_IDENTITY` `NodeAt` the caller composes (a fabricated `NodeId` from a cell is the deleted phantom-id form); a `pgr_bridges` cut-EDGE result is NOT a NODE so it never decodes through `CellOf` — its `SETOF bigint` of edge ids rails the raw `Severed` carrier and `CleaveKind.NodeValued` is the decode discriminant that keeps an edge id out of the cell space, while `Tour` derives its cost matrix from an in-database `pgr_dijkstraCostMatrix` composed server-side via `format('%L', %L::bigint[])` (the bound Edges-SQL and the `@vids::text` array both `%L`-quoted, the array re-cast `::bigint[]`, injection-safe since pgRouting re-parses the matrix-SQL text and an Npgsql parameter cannot bind inside it) rather than a caller-concatenated matrix string; the AGE write half (`Mutate`) is split from the read (`api-apache-age#CYPHER_QUERY` rejects a mutate-and-return statement) and the variable-length `Reach` lowers to the Cypher `*` range the `age_vle` engine plans, never a managed BFS; the network edge table and the AGE backing relations are async daemon projections off the one Marten stream so a routing/match query rides the one graph, never a second network store, and an interactive-correctness routing read binds the synchronous in-process counterpart.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The pgrouting path-function selector — Dijkstra (unit/weighted), A* (needs the xy Edges-SQL), bidirectional
// (the two-ended frontier). A new path function is one row carrying its `pgr_*` name; the verb stays `Path`.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RouteMode {
    public static readonly RouteMode Dijkstra = new("dijkstra", "pgr_dijkstra", xy: false);
    public static readonly RouteMode AStar = new("astar", "pgr_aStar", xy: true);
    public static readonly RouteMode Bidirectional = new("bidirectional", "pgr_bdDijkstra", xy: false);
    public string Function { get; }
    public bool Xy { get; }
    private RouteMode(string key, string function, bool xy) : this(key) => (Function, Xy) = (function, xy);
}

// The pgrouting connectivity-partition selector — Tarjan SCC (directed), connected components (undirected),
// articulation points (cut VERTICES), bridges (cut EDGES). One row per pgr_* connectivity function, each carrying
// `NodeValued`: the component + articulation functions return NODE ids (the H3-cell node space → cell-space `Cleaved`),
// but `pgr_bridges` returns EDGE ids (a `SETOF bigint` of cut EDGE identities, NOT vertices) — so a bridge id is NEVER
// fed through `CellOf` into an `H3Cell` (the fabricated-cell defect this discriminant deletes); an edge-valued cleave
// rails the raw `long` edge ids on the `Severed` carrier instead. `NodeValued` is the decode discriminant.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CleaveKind {
    public static readonly CleaveKind Strong = new("strong", "pgr_strongComponents", nodeValued: true);
    public static readonly CleaveKind Connected = new("connected", "pgr_connectedComponents", nodeValued: true);
    public static readonly CleaveKind Articulation = new("articulation", "pgr_articulationPoints", nodeValued: true);
    public static readonly CleaveKind Bridges = new("bridges", "pgr_bridges", nodeValued: false);
    public string Function { get; }
    public bool NodeValued { get; }
    private CleaveKind(string key, string function, bool nodeValued) : this(key) => (Function, NodeValued) = (function, nodeValued);
}

// --- [MODELS] -----------------------------------------------------------------------------
// The decoded AGE agtype path: a NodeId vertex sequence plus the path weight, decoded through GraphSession.Decode
// (the registered cast), NEVER a comma-split text scan. The Edges-SQL contract (api-pgrouting#EDGES_SQL): id/source/
// target/cost (negative cost ⇒ edge absent), the xy quad for A*, projected from the federated Connect relationships.
public readonly record struct AgtypePath(Seq<NodeId> Vertices, double Weight);
public readonly record struct EdgesSql(string Inner) {
    // The federated network edge table the daemon projects from the Connect/Generic connection relationships — id/source/
    // target/cost for Dijkstra/components/flow, the xy quad appended for the A* heuristic, parameter-bound as one text arg.
    public static readonly EdgesSql Network = new("SELECT id, source, target, cost, reverse_cost FROM network_edge");
    public static readonly EdgesSql NetworkXy = new("SELECT id, source, target, cost, reverse_cost, x1, y1, x2, y2 FROM network_edge");
    public static readonly EdgesSql Capacity = new("SELECT id, source, target, capacity, reverse_capacity FROM network_edge");
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The ONE polymorphic graph verb [Union] — every AGE openCypher AND pgrouting verb is one case under one total Switch,
// so a new verb breaks every dispatch site at compile time rather than growing a sibling Match/Route/Tour family.
// ConversionFromValue = None deletes the implicit value-to-union conversion; the private ctor seals the case family.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphQuery {
    private GraphQuery() { }
    public sealed record Match(string Graph, string Clause, HashMap<string, string> Parameters) : GraphQuery;
    public sealed record Mutate(string Graph, string Clause, HashMap<string, string> Parameters) : GraphQuery;
    public sealed record Reach(string Graph, NodeId From, int MinHops, int MaxHops) : GraphQuery;
    public sealed record Path(EdgesSql Edges, H3Cell From, H3Cell To, RouteMode Mode) : GraphQuery;
    public sealed record Kth(EdgesSql Edges, H3Cell From, H3Cell To, int K) : GraphQuery;
    public sealed record Spread(EdgesSql Edges, H3Cell Root, double Radius) : GraphQuery;
    public sealed record Tour(EdgesSql Edges, Seq<H3Cell> Stops, H3Cell Start, H3Cell End) : GraphQuery;
    public sealed record Flow(EdgesSql Edges, H3Cell Source, H3Cell Sink) : GraphQuery;
    public sealed record Cleave(EdgesSql Edges, CleaveKind Kind) : GraphQuery;
}

// AGE results are NODE-space (the decoded `properties.id` are real seam NodeIds → an ElementSet directly). pgrouting results
// are CELL-space (the `node bigint` are H3 cells, the navigable cell mesh — NOT element NodeIds, since an H3Cell is the
// per-MODEL envelope locator Element/identity#ELEMENT_IDENTITY owns, many elements per cell), so they stay Seq<H3Cell> and a
// caller resolves cells→elements ONE-HOP through Element/identity#ELEMENT_IDENTITY NodeAt — fabricating a NodeId from a cell
// string is the named phantom-id defect. ToSet projects only the node-space results; ToCells exposes the cell-space ones.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphResult {
    private GraphResult() { }
    public sealed record Paths(Seq<AgtypePath> Found) : GraphResult;       // AGE Match/Reach — node-space
    public sealed record Routed(Seq<H3Cell> Path, double Cost) : GraphResult;   // pgrouting Path — cell-space
    public sealed record Ranked(Seq<(Seq<H3Cell> Path, double Cost)> Alternatives) : GraphResult;  // pgrouting KSP — cell-space, path_id-ascending alternatives
    public sealed record Spanned(Seq<H3Cell> Reached, double Radius) : GraphResult;  // pgrouting drivingDistance — cell-space
    public sealed record Toured(Seq<H3Cell> Order, double Cost) : GraphResult;  // pgrouting TSP — cell-space
    public sealed record Flowed(long MaxFlow) : GraphResult;               // pgrouting maxFlow — scalar
    public sealed record Cleaved(Seq<Seq<H3Cell>> Components) : GraphResult;  // pgrouting components/articulation — cell-space NODE partitions
    public sealed record Severed(Seq<long> Edges) : GraphResult;           // pgrouting bridges — raw cut-EDGE ids, NOT cells (a bridge id is an edge, never a node/H3Cell)
}

public static class GraphLane {
    // The ONE entrypoint: gate once, then dispatch the total Switch. A disabled lane never opens a connection (it rails
    // GraphFault.Disabled); each arm lowers to its parameterized server-side SQL and decodes its rows. The AGE arms bind
    // `params agtype` and run cypher() with the mandatory `AS (v agtype)` list (the OUTER SELECT extracts the id server-side);
    // the pgrouting arms bind the H3Cell vids and run the pgr_* function with the PATH/SPANTREE/COST/FLOW list — never a
    // concatenated body, never a hand-parse.
    public static IO<Fin<GraphResult>> Run(NpgsqlDataSource source, CypherEnablement gate, GraphQuery query) =>
        gate.Reaches
            ? query.Switch(
                state: source,
                match: static (db, m) => Cypher(db, m.Graph, m.Clause, m.Parameters, decode: true).Map(static r => r.Map(static p => (GraphResult)new GraphResult.Paths(p))),
                mutate: static (db, m) => Cypher(db, m.Graph, m.Clause, m.Parameters, decode: false).Map(static r => r.Map(static p => (GraphResult)new GraphResult.Paths(p))),
                reach: static (db, r) => Cypher(db, r.Graph, "MATCH (a {id:$from})-[*" + r.MinHops.ToString(CultureInfo.InvariantCulture) + ".." + r.MaxHops.ToString(CultureInfo.InvariantCulture) + "]->(b) RETURN b", HashMap(("from", r.From.Value)), decode: true).Map(static r => r.Map(static p => (GraphResult)new GraphResult.Paths(p))),
                path: static (db, p) => Route(db, p.Mode.Function, p.Mode.Xy ? EdgesSql.NetworkXy : p.Edges, p.From, p.To),
                kth: static (db, k) => Ksp(db, k.Edges, k.From, k.To, k.K),
                spread: static (db, s) => Spread(db, s.Edges, s.Root, s.Radius),
                tour: static (db, t) => Tour(db, t.Edges, t.Stops, t.Start, t.End),
                flow: static (db, f) => Flow(db, f.Edges, f.Source, f.Sink),
                cleave: static (db, c) => Cleave(db, c.Edges, c.Kind))
            : IO.pure(Fin<GraphResult>.Fail(new GraphFault.Disabled()));

    // Project the NODE-space AGE result to the ElementSet selection currency so a graph query composes with every other
    // selection (a Match path intersected with a Classification selection is one SetExpr.Intersect, never an app join). The
    // CELL-space pgrouting results (Routed/Spanned/Toured/Cleaved) are NOT NodeIds — ToCells exposes them and a caller resolves
    // cells→elements one-hop through Element/identity#ELEMENT_IDENTITY NodeAt; fabricating a NodeId from a cell is the deleted form.
    public static ElementSet ToSet(GraphResult result) => result.Switch(
        paths: static p => ElementSet.Of(p.Found.Bind(static x => x.Vertices)),
        routed: static _ => ElementSet.Empty,
        ranked: static _ => ElementSet.Empty,
        spanned: static _ => ElementSet.Empty,
        toured: static _ => ElementSet.Empty,
        flowed: static _ => ElementSet.Empty,
        cleaved: static _ => ElementSet.Empty,
        severed: static _ => ElementSet.Empty);   // cut-EDGE ids are neither node-space nor cell-space; the caller reads `Severed.Edges` directly

    // The CELL-space projection for the pgrouting results: the navigable H3 cell mesh the route/spread/tour/component lane
    // walks, which a caller resolves to elements via Element/identity#ELEMENT_IDENTITY NodeAt(cell). A node-space AGE
    // result carries no cells (it is already an ElementSet), so it folds to empty here — the two spaces never conflate.
    public static Seq<H3Cell> ToCells(GraphResult result) => result.Switch(
        paths: static _ => Seq<H3Cell>(),
        routed: static r => r.Path,
        ranked: static k => k.Alternatives.Bind(static a => a.Path),
        spanned: static s => s.Reached,
        toured: static t => t.Order,
        flowed: static _ => Seq<H3Cell>(),
        cleaved: static c => c.Components.Bind(static x => x),
        severed: static _ => Seq<H3Cell>());   // a cut-EDGE id is not an H3 cell — never CellOf'd, so it carries no cell mesh

    // --- [CYPHER] -------------------------------------------------------------------------
    // AGE read/write: bind the graph name and the params agtype through Npgsql parameters, run cypher() with the mandatory
    // column-definition list. A read decodes each agtype vertex through GraphSession.Decode; a mutate runs with no RETURN
    // column and returns the empty path Seq. The $$ … $$ dollar-quote isolates the Cypher body and `decode` discriminates.
    static IO<Fin<Seq<AgtypePath>>> Cypher(NpgsqlDataSource source, string graph, string clause, HashMap<string, string> parameters, bool decode) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            // The inner cypher() RETURNs the vertex agtype; the OUTER SELECT extracts `properties.id` SERVER-SIDE through the
            // registered agtype `->`/`->>` operators into a plain text column — the api's typed-extraction path, never a hand-parse.
            command.CommandText = decode
                ? "SELECT (v->'properties'->>'id') AS id FROM ag_catalog.cypher(@graph, $$" + clause + "$$, @params) AS (v agtype)"
                : "SELECT * FROM ag_catalog.cypher(@graph, $$" + clause + "$$, @params) AS (v agtype)";
            command.Parameters.AddWithValue("graph", graph);
            command.Parameters.AddWithValue("params", NpgsqlTypes.NpgsqlDbType.Unknown, JsonSerializer.Serialize(parameters.ToDictionary(static p => p.Key, static p => p.Value)));
            if (!decode) { await command.ExecuteNonQueryAsync().ConfigureAwait(false); return Fin.Succ(Seq<AgtypePath>()); }
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var paths = new List<AgtypePath>();   // Exemption: the chunk drain fills a seam-local list frozen once by toSeq; per-row Seq.Add forcing is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
            while (await reader.ReadAsync().ConfigureAwait(false)) {
                var step = GraphSession.Decode(await reader.IsDBNullAsync(0).ConfigureAwait(false) ? null : reader.GetString(0)).Map(static id => new AgtypePath(Seq(id), 0.0));
                if (step.IsFail) { return step.Map(static _ => Seq<AgtypePath>()); }
                step.Iter(paths.Add);
            }
            return Fin.Succ(toSeq(paths));
        }) | @catch<IO, Fin<Seq<AgtypePath>>>(static _ => true, static e => IO.pure(Fin<Seq<AgtypePath>>.Fail(new GraphFault.MatchFailed(e.Message))));

    // --- [ROUTING] ------------------------------------------------------------------------
    // pgrouting shortest path: bind the Edges-SQL string and the H3Cell-derived bigint vids through Npgsql parameters, run
    // the RouteMode-selected pgr_* function with the PATH column-definition list, read `node bigint` back through CellOf so
    // the routed cells project to the SAME node space the in-process pocketken.H3 GridPathCells walks. edge = -1 is the
    // last-node sentinel (api-pgrouting#ROUTING_FUNCTIONS), so the path is the node column ascending agg_cost.
    static IO<Fin<GraphResult>> Route(NpgsqlDataSource source, string function, EdgesSql edges, H3Cell from, H3Cell to) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT node, agg_cost FROM {function}(@edges, @from, @to, directed => true) AS (seq integer, path_seq integer, node bigint, edge bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            command.Parameters.AddWithValue("from", GraphSession.Vid(from));
            command.Parameters.AddWithValue("to", GraphSession.Vid(to));
            return Fin.Succ((GraphResult)await ReadRoute(command).ConfigureAwait(false));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new GraphFault.RouteFailed(e.Message))));

    // pgrouting K-shortest-paths (`pgr_KSP`, api-pgrouting#ROUTING_FUNCTIONS): the MULTI-PATH output is PATH + `path_id`
    // (path_id=1 the cheapest), so the alternatives group by `path_id` ascending into the cell-space `Ranked` carrier —
    // the distinct-paths counterpart of the single-shortest `Route`, never a managed re-implementation of KSP.
    static IO<Fin<GraphResult>> Ksp(NpgsqlDataSource source, EdgesSql edges, H3Cell from, H3Cell to, int k) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT path_id, node, agg_cost FROM pgr_KSP(@edges, @from, @to, @k, directed => true) AS (seq integer, path_seq integer, path_id integer, start_vid bigint, end_vid bigint, node bigint, edge bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            command.Parameters.AddWithValue("from", GraphSession.Vid(from));
            command.Parameters.AddWithValue("to", GraphSession.Vid(to));
            command.Parameters.AddWithValue("k", k);
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var rows = new List<(int Path, H3Cell Node, double Cost)>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
            while (await reader.ReadAsync().ConfigureAwait(false)) { rows.Add((reader.GetInt32(0), GraphSession.CellOf(reader.GetInt64(1)), reader.GetDouble(2))); }
            var alternatives = toSeq(rows.GroupBy(static r => r.Path).OrderBy(static g => g.Key)
                .Select(static g => (Path: toSeq(g.Select(static r => r.Node)), Cost: g.Max(static r => r.Cost))));
            return Fin.Succ((GraphResult)new GraphResult.Ranked(alternatives));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new GraphFault.RouteFailed(e.Message))));

    static IO<Fin<GraphResult>> Spread(NpgsqlDataSource source, EdgesSql edges, H3Cell root, double radius) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT node, agg_cost FROM pgr_drivingDistance(@edges, @root, @radius, directed => true) AS (seq bigint, depth bigint, start_vid bigint, pred bigint, node bigint, edge bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            command.Parameters.AddWithValue("root", GraphSession.Vid(root));
            command.Parameters.AddWithValue("radius", radius);
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var reached = new List<H3Cell>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
            while (await reader.ReadAsync().ConfigureAwait(false)) { reached.Add(GraphSession.CellOf(reader.GetInt64(0))); }
            return Fin.Succ((GraphResult)new GraphResult.Spanned(toSeq(reached), radius));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new GraphFault.RouteFailed(e.Message))));

    // The TSP Matrix SQL is NOT a CLIENT-concatenated string (a runtime-concatenated routing body is the rejected
    // api-pgrouting form): `pgr_TSP`'s first argument is a Matrix-SQL TEXT that pgRouting re-parses server-side, and
    // an `@edges`/`@vids` Npgsql parameter does NOT bind inside that re-parsed literal — so the matrix-SQL is composed
    // SERVER-SIDE through PostgreSQL `format()` with `%L` for BOTH bound args: the Edges-SQL literal-quotes injection-safe,
    // and the `bigint[]` vids render through `@vids::text` (the PG array OUTPUT form `{1,2,3}`) literal-quoted by `%L`
    // (`'{1,2,3}'`) then RE-CAST `::bigint[]`, deriving `pgr_dijkstraCostMatrix(<edges>, '{…}'::bigint[], directed => false)`
    // — a bare `%s` of the array text would inject the unquoted `{1,2,3}` (an array OUTPUT form, never valid array INPUT
    // syntax) and fail to parse, so the `%L`-quote-plus-`::bigint[]`-recast is the only correct server-side composition.
    // `directed => false` is the symmetric metric `pgr_TSP` requires; 4.0 `pgr_TSP` dropped the annealing knobs (only
    // `start_id`/`end_id` remain).
    static IO<Fin<GraphResult>> Tour(NpgsqlDataSource source, EdgesSql edges, Seq<H3Cell> stops, H3Cell start, H3Cell end) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT node, agg_cost FROM pgr_TSP(format('SELECT * FROM pgr_dijkstraCostMatrix(%L, %L::bigint[], directed => false)', @edges, @vids::text), start_id => @start, end_id => @end) AS (seq integer, node bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            command.Parameters.AddWithValue("vids", stops.Map(GraphSession.Vid).ToArray());
            command.Parameters.AddWithValue("start", GraphSession.Vid(start));
            command.Parameters.AddWithValue("end", GraphSession.Vid(end));
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var order = new List<H3Cell>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
            var cost = 0.0;
            while (await reader.ReadAsync().ConfigureAwait(false)) { order.Add(GraphSession.CellOf(reader.GetInt64(0))); cost = reader.GetDouble(1); }
            return Fin.Succ((GraphResult)new GraphResult.Toured(toSeq(order), cost));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new GraphFault.RouteFailed(e.Message))));

    static IO<Fin<GraphResult>> Flow(NpgsqlDataSource source, EdgesSql edges, H3Cell flowSource, H3Cell sink) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT pgr_maxFlow(@edges, @source, @sink)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            command.Parameters.AddWithValue("source", GraphSession.Vid(flowSource));
            command.Parameters.AddWithValue("sink", GraphSession.Vid(sink));
            return Fin.Succ((GraphResult)new GraphResult.Flowed((long)(await command.ExecuteScalarAsync().ConfigureAwait(false) ?? 0L)));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new GraphFault.RouteFailed(e.Message))));

    // THREE decode shapes the `NodeValued` discriminant routes (api-pgrouting#COMPONENTS_TOPOLOGY): `pgr_strongComponents`/
    // `pgr_connectedComponents` return `(seq, component, node)` — NODE ids grouped by component into cell-space `Cleaved`
    // partitions; `pgr_articulationPoints` returns `SETOF bigint` of cut VERTEX ids — one cell-space `Cleaved` partition;
    // `pgr_bridges` returns `SETOF bigint` of cut EDGE ids — these are EDGE identities, NOT vertices, so they rail the raw
    // `long` ids on `Severed` and are NEVER fed through `CellOf` (the fabricated-cell defect). The bare `SETOF bigint` column
    // is named by the function itself, not `node`, since there is no node column for a scalar-returning routine.
    static IO<Fin<GraphResult>> Cleave(NpgsqlDataSource source, EdgesSql edges, CleaveKind kind) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            var partitioned = kind == CleaveKind.Strong || kind == CleaveKind.Connected;
            command.CommandText = partitioned
                ? $"SELECT component, node FROM {kind.Function}(@edges) AS (seq bigint, component bigint, node bigint)"
                : $"SELECT {kind.Function} AS id FROM {kind.Function}(@edges)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            if (!kind.NodeValued) {
                var cut = new List<long>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
                while (await reader.ReadAsync().ConfigureAwait(false)) { cut.Add(reader.GetInt64(0)); }   // cut-EDGE id — never CellOf
                return Fin.Succ((GraphResult)new GraphResult.Severed(toSeq(cut)));
            }
            var rows = new List<(long Component, H3Cell Node)>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
            while (await reader.ReadAsync().ConfigureAwait(false)) { rows.Add(partitioned ? (reader.GetInt64(0), GraphSession.CellOf(reader.GetInt64(1))) : (0L, GraphSession.CellOf(reader.GetInt64(0)))); }
            return Fin.Succ((GraphResult)new GraphResult.Cleaved(toSeq(rows.GroupBy(static r => r.Component).Select(static g => toSeq(g.Select(static r => r.Node))))));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new GraphFault.RouteFailed(e.Message))));

    static async System.Threading.Tasks.Task<GraphResult> ReadRoute(NpgsqlCommand command) {
        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        var path = new List<H3Cell>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
        var cost = 0.0;
        while (await reader.ReadAsync().ConfigureAwait(false)) { path.Add(GraphSession.CellOf(reader.GetInt64(0))); cost = reader.GetDouble(1); }
        return new GraphResult.Routed(toSeq(path), cost);
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                   | [BINDING]                                                  |
| :-----: | :------------------ | :---------------------------------------- | :-------------------------------------------------------- |
|  [01]   | one verb surface    | `GraphQuery` `[Union]` + total `Switch`   | every AGE + `pgrouting` verb; no sibling method family     |
|  [02]   | parameterization    | `params agtype` + Edges-SQL `Npgsql` args | never a concatenated body; `Parameters` are live           |
|  [03]   | `pgrouting` roster   | Dijkstra/A*/bidi/KSP/DD/TSP/flow/components | the rich roster, not a 3-verb slice; `RouteMode`/`CleaveKind`/`Kth` |
|  [04]   | AGE read/write       | `Match` decode vs `Mutate` no-return       | a mutate-and-return statement is the rejected AGE form     |
|  [05]   | node-space agreement | `H3Cell` `long` ↔ in-process `pocketken.H3` | in-database route and in-process path share node identity  |
|  [06]   | result spaces        | AGE → `ElementSet`; `pgrouting` NODE → `H3Cell`; `pgr_bridges` EDGE → `Severed` | three spaces never conflate; a cut-edge id never `CellOf`'d into a cell |
|  [07]   | cell→element resolve | `Element/identity#ELEMENT_IDENTITY` `NodeAt` | the caller's one-hop; a cell is the per-model envelope locator |
|  [08]   | algorithm dedup      | server-side `pgr_*`/`age` vs in-process    | the two meet at the shared node space, never re-implemented |
