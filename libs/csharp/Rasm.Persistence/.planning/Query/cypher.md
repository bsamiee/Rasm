# [PERSISTENCE_QUERY_CYPHER]

Rasm.Persistence offers an OPTIONAL self-hosted-only graph lane over the two server-side PostgreSQL graph extensions — Apache AGE openCypher and PostGIS `pgrouting` — both demoted beneath the in-process `Query/topology#GRAPH_TOPOLOGY` QuikGraph view (`H5`): QuikGraph is the DEFAULT authoritative topology owner and these are ASYNC analytical read projections a deployment enables only where one self-hosted PostgreSQL instance can host both the extension and the Marten event store, so an interactive-correctness query (clash, void-resolution, live QTO) NEVER routes here without `WaitForNonStaleProjectionDataAsync` and binds the synchronous topology instead. Both extensions carry NO managed assembly: every surface is server-side SQL the `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension` `CreateSql` (committed on the EF migration rail, `Element/identity#SCHEMA_VERDICT`) installs and this lane drives through raw `Npgsql`, and one `GraphSession` owns the per-connection `LOAD 'age'`+`search_path` init through a `NpgsqlDataSource` physical-connection hook so the session-load obligation is paid once per physical connection rather than re-issued per query. ONE `GraphQuery` `[Union]` collapses every openCypher verb (read `Match`, mutating `Mutate`, variable-length `Reach` — the FULL multi-vertex path decoded through the registered `agtype`→`jsonb` cast, never a one-vertex stub) AND every `pgrouting` verb (`Path` over Dijkstra/A*/bidirectional, `Via` over the `pgr_dijkstraVia` ordered-waypoint route, `Located` over the `pgr_withPoints` off-node point family, `Kth` over K-shortest alternatives, `Spread` over driving-distance, `Tour`/`TourPlanar` over the matrix and euclidean metric TSPs, `Flow` over the `FlowKind`-selected per-edge-labeled max-flow family, `Cleave` over the component/articulation/cut-edge/biconnected family) under one total generated `Switch`, parameter-bound through `params agtype` and the Edges-SQL `Npgsql` parameters — never a runtime-concatenated graph body and never a hand-parsed text decode; the per-graph lifecycle (`create_graph`/`create_vlabel`/`create_elabel`/bulk CSV loaders) the daemon projection assumes is ONE `GraphDdl` `[Union]` the session defines through, so no owner is missing for the graph the projection writes into. The graph node space is the `Element/identity#ELEMENT_IDENTITY` `H3Cell` (`[ValueObject<long>]`, the `h3-pg` bit-exact convention) for the `pgrouting` Edges-SQL `bigint` vertex ids, and the seam `NodeId.Value` string for the AGE vertex `properties.id` decoded through the registered `agtype ->>` operator; a process-randomized `GetHashCode` as a persisted vertex id is the named `boundaries.md#MEMO_KEY` defect this page deletes. The `pgrouting` `Path`/A* (the geometry-weighted metric route over the `H3Cell` cell-mesh) is the ANALYTICAL counterpart to the SYNCHRONOUS in-process `Query/topology#TRAVERSAL` `Path` (`ShortestPathsDijkstra` unit-weight TOPOLOGICAL distance over the seam graph, NO node coordinate) — the two own DIFFERENT distances (metric vs topological) and meet only at the result `H3Cell` mesh / `ElementSet`, neither mirroring the other's algorithm; the in-process topology lane holds NO `pocketken.H3` cell-path member, and the `H3Cell`→element hop is the one-hop `Element/identity#ELEMENT_IDENTITY` `NodeAt`. Every boundary failure rails one closed `CypherFault` `[Union]` over `Expected` — RENAMED off the `Element/graph#FAULT_TABLES` `GraphFault` simple-name collision (graph keeps the name) and deriving `Code => FaultBand.Cypher + n` off that one registry, never a bare integer and never `Error.New` — and every lane outcome rides a `ReceiptSinkPort` fact the composing `Query/lane#READ_ROUTING` router emits. `NodeId` arrives from `Rasm.Element`; `ElementSet`/`StalenessWatermark`/`QueryLane`/`ReadRouter`/`H3Cell` arrive from `Query/lane`/`Element/identity`; `H3Index` arrives from `pocketken.H3`; `NpgsqlDataSource`/`agtype` arrive from the substrate; `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId`/`TenantContext` arrive from AppHost.

## [01]-[INDEX]

- [01]-[GRAPH_SESSION]: the `CypherEnablement` self-hosted gate, the `CypherFault` closed boundary band, the `GraphDdl` graph/label lifecycle surface, and the `GraphSession` owning the per-physical-connection `LOAD 'age'`+`search_path` init, the async daemon projection, and the `agtype`/path/Edges-SQL decode primitives both lanes compose.
- [02]-[GRAPH_QUERY]: the ONE `GraphQuery` `[Union]` collapsing every AGE openCypher and `pgrouting` verb, the `RouteMode`/`FlowKind`/`CleaveKind` function-selector rows, the `GraphResult` typed carrier, the `H3Cell` vertex-id node space, and the total-`Switch` `Run` that lowers each case to its server-side SQL and decodes its rows into the NODE-space `ElementSet` (AGE), the CELL-space `H3Cell` mesh (`pgrouting`), or the raw EDGE space (`pgr_bridges`/`pgr_biconnectedComponents`/the flow labeling).

## [02]-[GRAPH_SESSION]

- Owner: `CypherEnablement` the `[SmartEnum<string>]` lane-availability gate (`Disabled`/`SelfHosted`) carrying whether the lane reaches the server extensions; `CypherFault` the closed AGE/`pgrouting`-boundary fault `[Union]` over `Expected` deriving `FaultBand.Cypher + n` (RENAMED — `Element/graph` keeps the `GraphFault` simple name); `GraphDdl` the closed graph/label lifecycle `[Union]` lowering to the `ag_catalog` `SELECT`-function DDL; `GraphSession` the static surface owning the `NpgsqlDataSource` physical-connection init hook, the async daemon projection rebuild, the lifecycle `Define`, the `agtype` scalar decode through the registered SQL casts, the FULL-path decode through the registered `agtype`→`jsonb` cast, and the Edges-SQL `bigint` node-space projection both lanes compose.
- Cases: `CypherEnablement` is `Disabled` (the default — the in-process QuikGraph view is authoritative and no connection ever reaches for `age`/`pgrouting`) or `SelfHosted` (one PostgreSQL instance hosts the extension PLUS Marten); `CypherFault` is `Disabled` (the lane was reached while gated off), `MatchFailed` (an AGE `cypher()` boundary fault), `RouteFailed` (a `pgrouting` boundary fault), and `Unresolvable` (a vertex with no `H3Cell` / a `NodeId` property absent from the decoded `agtype`) so the cause stays structurally addressable rather than one provider token; `GraphDdl` is `Create(graph)`/`Drop(graph, cascade)`/`VertexLabel(graph, label)`/`EdgeLabel(graph, label)`/`DropLabel(graph, label, force)`/`Rename(graph, to)` (the `create_graph`/`drop_graph`/`create_vlabel`/`create_elabel`/`drop_label`/`alter_graph('RENAME')` lifecycle) plus the bulk loaders `LoadVertices(graph, label, serverPath, idField)`/`LoadEdges(graph, label, serverPath)` (`load_labels_from_file`/`load_edges_from_file` — server-side CSV, VLE-coherent because `age` installs the `age_invalidate_graph_cache()` trigger on every label backing relation).
- Entry: `public static NpgsqlDataSourceBuilder Provision(NpgsqlDataSourceBuilder builder, CypherEnablement gate)` installs the per-physical-connection `LOAD 'age'; SET search_path = ag_catalog, "$user", public;` init through `UsePhysicalConnectionInitializer` ONLY when `SelfHosted` so the session-load obligation is paid once per physical connection (`api-apache-age#SESSION_SETUP`), never per query; `public static IO<Unit> Rebuild(IDocumentStore store, Duration timeout)` rebuilds the async graph projection from the one Marten stream via the daemon and blocks for non-stale on demand; `public static IO<Fin<Unit>> Define(NpgsqlDataSource source, CypherEnablement gate, GraphDdl ddl)` lowers ONE lifecycle case to its parameter-bound `ag_catalog` `SELECT`-function call (the graph/label DDL the daemon projection assumes exists — the one-shot `CREATE EXTENSION age` stays the frozen `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension.CreateSql` rail, never re-issued here); `public static Fin<NodeId> Decode(string? extractedId)` admits the seam `NodeId` from the vertex `properties.id` the Cypher `RETURN` already extracted SERVER-SIDE through the registered `agtype ->`/`->>` operators (`CypherFault.Unresolvable` on a NULL column), never a client-side comma-split or whole-vertex parse; `public static Fin<AgtypePath> DecodePath(string json)` decodes ONE full AGE path from the `(p)::jsonb` column the outer `SELECT` cast through the REGISTERED `agtype`→`jsonb` cast — the alternating `[vertex, edge, …]` array iterated with `JsonDocument`, every even element's `properties.id` admitted through `Decode` and every odd element's `properties.cost` summed into `Weight` — so `AgtypePath.Vertices` carries the REAL multi-vertex walk and `Weight` a real cost sum, never a length-1 stub with a fabricated `0.0`; `public static long Vid(H3Cell cell)` is the `pgrouting` `bigint` node-space projection (the `h3-pg` `long` cell), and `public static H3Cell CellOf(long vid)` its inverse.
- Auto: the lane is `Disabled` by construction so a deployment that has not provisioned `age`/`pgrouting` reads the in-process topology and never reaches for a missing extension; when `SelfHosted`, `Provision` registers the connection-init hook so every physical connection runs `LOAD 'age'` and sets `search_path` once at open (a PL/pgSQL Cypher wrapper repeats it in its own body per `api-apache-age#SESSION_SETUP`, but the managed lane pays it at the physical-connection seam, not per `cypher()` call); the AGE backing relations and the `pgrouting` network edge table are async daemon projections (`ProjectionLifecycle.Async`) rebuilt from the one Marten stream so neither duplicates the event store; the Cypher read's OUTER `SELECT` extracts `(v->'properties'->>'id')` server-side through the registered `agtype` operators so `Decode` receives a PLAIN text column (the seam `NodeId.Value` the projection wrote as the vertex `id` property), making the decoded node the REAL seam id, never the AGE internal `graphid` integer and never a client-side hand-parse; a PATH-shaped read (`Reach`) RETURNs the whole path `p` and the outer `SELECT` casts `(p)::jsonb` through the REGISTERED cast so `DecodePath` receives one typed JSON document — the alternating vertex/edge array — rather than a comma-split of the `::path` text; `Define` runs the graph/label lifecycle through the SAME gate (`age` 1.7.0 refuses to install into a foreign-owned `ag_catalog`, so the DDL surface is idempotent only against a role-owned catalog), and a bulk `LoadVertices`/`LoadEdges` stays coherent with later `*`-range traversals because the `age_invalidate_graph_cache()` trigger bumps the graph version on every direct backing-relation write; `Vid` reinterprets the `H3Cell` `long` (the `h3-pg` cell convention `Element/identity#ELEMENT_IDENTITY` already stamps) so the `pgrouting` Edges-SQL vertex id, the GiST/BRIN H3 index, and the in-process `pocketken.H3` cell agree bit-for-bit.
- Receipt: every lane outcome rides the `ReceiptSinkPort` — a session provision rides `store.graph.provision` carrying the gate; a daemon rebuild rides `store.graph.rebuild` carrying the watermark and elapsed wait; a lifecycle define rides `store.graph.define` carrying the graph, the case, and the affected label.
- Packages: Npgsql (`NpgsqlDataSourceBuilder.UsePhysicalConnectionInitializer`/`NpgsqlDataSource`/`NpgsqlConnection`), Marten (`IDocumentStore.BuildProjectionDaemonAsync`/`WaitForNonStaleProjectionDataAsync`), pocketken.H3 (`H3Index`), Rasm.Element (`NodeId`), Rasm.Persistence (`Element/identity#ELEMENT_IDENTITY` `H3Cell`, `Query/lane#READ_ROUTING` `StalenessWatermark`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, System.Text.Json, BCL inbox.
- Note: the `ReceiptSinkPort` envelope is emitted by the composing `Query/lane#READ_ROUTING` rail that routed the read (the lane primitives stay receipt-free, exactly as the sibling `Query/topology`/`Query/columnar` read lanes), so the receipt-ride below names the fact the router emits, not a sink call inside these primitives.
- Growth: a new enablement tier is one `CypherEnablement` row; a new boundary cause is one `CypherFault` case whose offset stays inside the registry decade; a new lifecycle op is one `GraphDdl` case lowering to its `ag_catalog` function; zero new surface — making AGE/`pgrouting` the default authoritative topology, hosting it in the same instance as Marten without the gate, a managed AGE/`pgrouting` driver wrapper, a per-query `LOAD 'age'`, a comma-split `agtype` or `::path` text decode, a page-local band constant beside the `FaultBand` registry, or a `GetHashCode`-derived vertex id is the deleted form because QuikGraph is the default, the extensions are optional self-hosted analytical lanes driven through raw `Npgsql` against the registered `agtype` casts, the session-load is a physical-connection hook, and the node space is the `h3-pg` `H3Cell`.
- Boundary: AGE and `pgrouting` are OPTIONAL self-hosted-only read projections (`H5`) demoted beneath the in-process QuikGraph view which is the default authoritative topology owner — a deployment never ASSUMES one PostgreSQL instance hosts both an extension and Marten-9/Npgsql-10, so the lane gates on `CypherEnablement.SelfHosted` and falls back to the in-process topology when disabled; both are the `Query/lane#READ_ROUTING` ASYNC analytical lanes carrying a `StalenessWatermark`, so an interactive-correctness query (clash, void-resolution, live QTO) binds the synchronous topology and never reads here without `WaitForNonStaleProjectionDataAsync`; both are driven through raw `Npgsql` against the server-side SQL because neither ships a managed/EF driver, and the mandatory `AS (col agtype, ...)` / `AS (seq integer, ...)` column-definition list is required by PostgreSQL's `RETURNS SETOF record` contract, never optional (`api-apache-age#CYPHER_QUERY`, `api-pgrouting#ROUTING_FUNCTIONS`); the AGE backing relations and the `pgrouting` network edge table are async daemon projections off the one Marten stream so both are rebuildable and never a second event store; the session-load (`LOAD 'age'`+`search_path`) is a physical-connection-init obligation the `Npgsql` open-hook pays once per connection, never a per-query statement that re-loads the shared library; the AGE vertex `id` decodes to the REAL seam `NodeId` through `properties.id` (the projection wrote the seam id as a vertex property) via the registered `agtype ->>` operator, and the `pgrouting` vertex id is the `Element/identity#ELEMENT_IDENTITY` `H3Cell` `long` (the `h3-pg` convention) the in-process `pocketken.H3` cell shares — a `graphid` integer surfacing as a `NodeId`, a comma-split text decode, or a `GetHashCode`-derived `bigint` vertex id is the named `boundaries.md#MEMO_KEY` defect this page deletes; the graph/label lifecycle is a `SELECT`-function DDL surface (`GraphDdl` lowering to the `ag_catalog` functions, every argument Npgsql-bound), gate-checked like every lane op and DISTINCT from the one-shot frozen `ServerExtension.CreateSql` install row — a `Define` that re-issues `CREATE EXTENSION`, or a daemon projection writing into a graph no owner provisioned, is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Npgsql;
using Rasm.Element;
using Rasm.Persistence.Element;                   // FaultBand — the Element/graph#FAULT_TABLES registry the band derives from
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
// The closed AGE/pgrouting boundary band, RENAMED CypherFault — Element/graph#FAULT_TABLES hosts the
// GraphFault simple name this rename resolves. A [Union] over the KERNEL `Rasm.Domain.Expected`
// (parameterless protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME
// federation base the seam `ElementFault` (2500) and `BimFault` (2600) realize — NOT `LanguageExt.Common.Expected`,
// whose `(string,int,Option)` ctor is the deleted form. Band membership derives `Code => FaultBand.Cypher + n`
// through the ONE registry row (never a bare integer, never `.Value`), so a recovery routes
// `error.IsType<CypherFault.RouteFailed>()` / `error.HasCode(8362)` / `error.Category()`, never a stringified
// provider token, and the bare case lifts onto `Fin<T>` with no `.ToError()` hop. No `[GenerateUnionOps]` —
// the kernel union-ops generator is strictly opt-in.
[Union]
public abstract partial record CypherFault : Expected, IValidationError<CypherFault> {
    private CypherFault() : base() { }
    public sealed record Disabled : CypherFault;
    public sealed record MatchFailed(string Detail) : CypherFault;
    public sealed record RouteFailed(string Detail) : CypherFault;
    public sealed record Unresolvable(string Detail) : CypherFault;

    public override int Code => FaultBand.Cypher + Switch(
        disabled:     static _ => 0,
        matchFailed:  static _ => 1,
        routeFailed:  static _ => 2,
        unresolvable: static _ => 3);

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

    public static CypherFault Create(string message) => new MatchFailed(message);
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
            await daemon.WaitForNonStaleData(timeout.ToTimeSpan()).ConfigureAwait(false);
            return unit;
        });

    // Decode ONE seam NodeId from the AGE vertex `properties.id` the Cypher RETURN already extracted SERVER-SIDE through the
    // registered agtype `->` / `->>` operators (`(v->'properties'->>'id')`, api-apache-age#AGTYPE), so the reader receives a
    // PLAIN text column — never a hand-parsed comma-split of the whole vertex body, never the AGE internal `graphid` integer.
    // The projection wrote the seam NodeId.Value as the vertex `id` property, so the extracted string IS the durable seam id;
    // a NULL column (the property absent) rails CypherFault.Unresolvable rather than fabricating an id.
    public static Fin<NodeId> Decode(string? extractedId) =>
        extractedId is { } value ? Fin.Succ(NodeId.Create(value)) : Fin.Fail<NodeId>(new CypherFault.Unresolvable("<vertex-id-absent>"));

    // Decode ONE full AGE path from the `(p)::jsonb` column the outer SELECT cast through the REGISTERED agtype→jsonb
    // cast (api-apache-age#AGTYPE `::jsonb`): the path is the alternating `[vertex, edge, …]` array, so the even elements'
    // `properties.id` admit through Decode and the odd elements' `properties.cost` (absent ⇒ 0.0 contribution) sum into
    // Weight — the populated multi-vertex carrier, never a length-1 stub, never a `::path` text comma-split.
    public static Fin<AgtypePath> DecodePath(string json) {
        using var path = JsonDocument.Parse(json);   // Exemption: the parse-probe `using` is the platform-forced admission-kernel seam (boundaries BYTE_IDENTITY)
        var steps = toSeq(path.RootElement.EnumerateArray().Select(static (element, index) => (Element: element, Index: index)));
        return steps.Filter(static s => (s.Index & 1) == 0)
            .Map(static s => Decode(s.Element.GetProperty("properties").TryGetProperty("id", out var id) ? id.GetString() : null))
            .TraverseM(identity).As()
            .Map(vertices => new AgtypePath(
                vertices,
                steps.Filter(static s => (s.Index & 1) == 1)
                    .Fold(0.0, static (cost, s) => cost + (s.Element.GetProperty("properties").TryGetProperty("cost", out var c) ? c.GetDouble() : 0.0))));
    }

    // The pgrouting bigint node space IS the Element/identity#ELEMENT_IDENTITY H3Cell `long` (the h3-pg convention), so the
    // Edges-SQL vertex id, the GiST/BRIN H3 index, and the in-process pocketken.H3 cell agree bit-for-bit (api-pgrouting#GEO_LANE_STACK).
    public static long Vid(H3Cell cell) => cell.Value;
    public static H3Cell CellOf(long vid) => H3Cell.Create(vid);

    // The per-graph lifecycle DDL the daemon projection assumes exists (api-apache-age#GRAPH_LABEL_LIFECYCLE +
    // #GRAPH_ALGORITHMS loaders): each case lowers to its `ag_catalog` SELECT function with every argument
    // Npgsql-bound; the one-shot `CREATE EXTENSION age` stays the frozen ServerExtension.CreateSql install rail.
    public static IO<Fin<Unit>> Define(NpgsqlDataSource source, CypherEnablement gate, GraphDdl ddl) =>
        gate.Reaches
            ? IO.liftAsync(async () => {
                  await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
                  await using var command = connection.CreateCommand();
                  var (sql, args) = ddl.Lower();
                  command.CommandText = sql;
                  args.Iter(argument => command.Parameters.AddWithValue(argument.Name, argument.Value));
                  await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                  return Fin.Succ(unit);
              }) | @catch<IO, Fin<Unit>>(static _ => true, static e => IO.pure(Fin<Unit>.Fail(new CypherFault.MatchFailed(e.Message))))
            : IO.pure(Fin<Unit>.Fail(new CypherFault.Disabled()));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The graph/label lifecycle union: SELECT-function DDL over ag_catalog (api-apache-age#GRAPH_LABEL_LIFECYCLE),
// one case per lifecycle op, the bulk CSV loaders included — VLE caches stay coherent because `age` installs
// the `age_invalidate_graph_cache()` trigger on every label backing relation.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphDdl {
    private GraphDdl() { }
    public sealed record Create(string Graph) : GraphDdl;
    public sealed record Drop(string Graph, bool Cascade) : GraphDdl;
    public sealed record VertexLabel(string Graph, string Label) : GraphDdl;
    public sealed record EdgeLabel(string Graph, string Label) : GraphDdl;
    public sealed record DropLabel(string Graph, string Label, bool Force) : GraphDdl;
    public sealed record Rename(string Graph, string To) : GraphDdl;
    public sealed record LoadVertices(string Graph, string Label, string ServerPath, bool IdField) : GraphDdl;
    public sealed record LoadEdges(string Graph, string Label, string ServerPath) : GraphDdl;

    // Every argument binds to a `name`/`cstring`/`text` parameter slot — never an interpolated identifier.
    public (string Sql, Seq<(string Name, object Value)> Args) Lower() => Switch<(string, Seq<(string, object)>)>(
        create:       static c => ("SELECT ag_catalog.create_graph(@g)", Seq<(string, object)>(("g", c.Graph))),
        drop:         static d => ("SELECT ag_catalog.drop_graph(@g, @cascade)", Seq<(string, object)>(("g", d.Graph), ("cascade", d.Cascade))),
        vertexLabel:  static v => ("SELECT ag_catalog.create_vlabel(@g, @l)", Seq<(string, object)>(("g", v.Graph), ("l", v.Label))),
        edgeLabel:    static e => ("SELECT ag_catalog.create_elabel(@g, @l)", Seq<(string, object)>(("g", e.Graph), ("l", e.Label))),
        dropLabel:    static d => ("SELECT ag_catalog.drop_label(@g, @l, @force)", Seq<(string, object)>(("g", d.Graph), ("l", d.Label), ("force", d.Force))),
        rename:       static r => ("SELECT ag_catalog.alter_graph(@g, 'RENAME', @to)", Seq<(string, object)>(("g", r.Graph), ("to", r.To))),
        loadVertices: static l => ("SELECT ag_catalog.load_labels_from_file(@g, @l, @path, @idField)", Seq<(string, object)>(("g", l.Graph), ("l", l.Label), ("path", l.ServerPath), ("idField", l.IdField))),
        loadEdges:    static l => ("SELECT ag_catalog.load_edges_from_file(@g, @l, @path)", Seq<(string, object)>(("g", l.Graph), ("l", l.Label), ("path", l.ServerPath))));
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
|  [08]   | fault rail          | `CypherFault` deriving `FaultBand.Cypher + n` | renamed off the graph collision; never `Error.New`, never a literal decade |
|  [09]   | graph lifecycle     | `GraphDdl` → `ag_catalog` `SELECT` functions | parameter-bound DDL + bulk loaders; extension install stays the frozen `ServerExtension.CreateSql` rail |

## [03]-[GRAPH_QUERY]

- Owner: `GraphQuery` the ONE `[Union]` collapsing every AGE openCypher verb (`Match`/`Mutate`/`Reach`) AND every `pgrouting` verb (`Path`/`Via`/`Located`/`Kth`/`Spread`/`Tour`/`TourPlanar`/`Flow`/`Cleave`) under one total generated `Switch`; `RouteMode` the `[SmartEnum<string>]` selecting the `pgrouting` path function (Dijkstra/A*/bidirectional) the `Path` case carries; `FlowKind` the `[SmartEnum<string>]` selecting the per-edge-LABELED max-flow function (`pgr_boykovKolmogorov`/`pgr_pushRelabel`/`pgr_edmondsKarp`) the `Flow` case carries; `CleaveKind` the `[SmartEnum<string>]` selecting the connectivity function on the `(NodeValued, Partitioned)` decode plane; `GraphResult` the typed `[Union]` carrier — the NODE-space `Paths`, the CELL-space `Routed`/`Ranked`/`Legged`/`Spanned`/`Toured`/`Cleaved`, the mixed cell-and-point `Traced`, the EDGE-space `Severed`/`Sundered`/`Flowed`; `GraphLane` the static surface owning the gate-checked lowering, the parameterized `cypher()`/`pgr_*` SQL, the server-side-extracted `agtype`/`SETOF record` decode, the `ToSet` node-space and `ToCells` cell-space projections.
- Cases: on `GraphQuery` — `Match(graph, clause, parameters)` (a read `MATCH … RETURN` over the AGE graph, parameter-bound through `params agtype`), `Mutate(graph, clause, parameters)` (a `CREATE`/`SET`/`DELETE` Cypher statement — the write half AGE owns, never folded with a trailing `RETURN` in one statement per `api-apache-age#CYPHER_QUERY`), `Reach(graph, from, minHops, maxHops)` (a bounded variable-length `*minHops..maxHops` traversal the AGE `age_vle` engine drives — RETURNing the whole path `p`, decoded FULLY through the registered `(p)::jsonb` cast), `Path(edges, from, to, mode)` (a `pgrouting` shortest path over Dijkstra/A*/bidirectional selected by `RouteMode`), `Via(edges, waypoints)` (a `pgr_dijkstraVia` ORDERED-waypoint route — the fixed visiting order, semantically distinct from the optimal-order `Tour` and the alternatives-producing `Kth`, so it is a verb, never a flag on either), `Located(edges, points, from, to)` (a `pgr_withPoints` off-node route — a NEGATIVE routing id is a Point mid-edge, the off-node origin the cell mesh admits), `Kth(edges, from, to, k)` (a `pgr_KSP` K-shortest distinct alternatives, `path_id`-ascending), `Spread(edges, root, radius)` (a `pgr_drivingDistance` reachability spanning-tree), `Tour(edges, stops, start, end)` (a `pgr_TSP` metric tour whose cost matrix is the in-database `pgr_dijkstraCostMatrix` composed server-side through PostgreSQL `format('%L', %L::bigint[])` — both bound args literal-quoted, the array re-cast `::bigint[]` — so they inject injection-safely into the matrix-SQL text pgRouting re-parses, never a C#-side concatenated matrix string), `TourPlanar(sites, start, end)` (a `pgr_TSPeuclidean` coordinate tour over the Coordinates-SQL `(id, x, y)` contract — NO cost-matrix pre-pass, the cheaper form whenever the cell coordinates are already in hand), `Flow(edges, source, sink, kind)` (the `FlowKind`-selected labeled max-flow over the `capacity` edge contract — per-edge `flow`/`residual_capacity`, the scalar deriving from the labeling), `Cleave(edges, kind)` (a `pgr_strongComponents`/`pgr_connectedComponents`/`pgr_articulationPoints`/`pgr_bridges`/`pgr_biconnectedComponents` connectivity partition); on `RouteMode` — `Dijkstra`/`AStar`/`Bidirectional` carrying the `pgr_*` function name; on `FlowKind` — `BoykovKolmogorov`/`PushRelabel`/`EdmondsKarp` carrying the labeled `pgr_*` function name; on `CleaveKind` — `Strong`/`Connected`/`Articulation`/`Bridges`/`Biconnected` carrying the connectivity function name AND the `(NodeValued, Partitioned)` decode-plane columns; on `GraphResult` — `Paths` (the NODE-space decoded AGE paths — `Reach` rows carrying the FULL multi-vertex walk and summed weight), `Routed`/`Spanned`/`Toured` (the CELL-space `H3Cell` sequences the navigable cell mesh walks), `Legged` (the CELL-space per-leg walks of one ordered-waypoint route plus the route total), `Traced` (the `pgr_withPoints` steps — a negative vid is a mid-edge Point, NEVER fabricated into a cell), `Ranked` (the CELL-space `path_id`-ascending K-shortest alternatives), `Cleaved` (the CELL-space `H3Cell` NODE-partition set — components + articulation cut-vertices), `Severed` (the raw cut-EDGE `long` ids `pgr_bridges` returns), `Sundered` (the `pgr_biconnectedComponents` EDGE-component partition — edge ids grouped by component, never cells), `Flowed` (the labeled per-edge flow assignment plus the derived scalar max flow).
- Entry: `public static IO<Fin<GraphResult>> Run(NpgsqlDataSource source, CypherEnablement gate, GraphQuery query)` is the ONE entrypoint — it short-circuits `CypherFault.Disabled` when the lane is gated off, else dispatches the total `Switch` lowering each case to its parameterized server-side SQL and decoding its rows; `public static ElementSet ToSet(GraphResult result)` projects the NODE-space AGE result to the `Query/lane#ELEMENT_SET_ALGEBRA` selection currency, and `public static Seq<H3Cell> ToCells(GraphResult result)` exposes the CELL-space `pgrouting` results a caller resolves to elements one-hop through `Element/identity#ELEMENT_IDENTITY` `NodeAt(cell)` (the per-element fine-cell resolver, not the coarse model-grain `Nearby`) — the spaces never conflate: a cut-edge id, a biconnected edge id, a flow edge label, and a `Traced` Point step are all structurally excluded from the cell projection.
- Auto: `Run` gates ONCE on `CypherEnablement.Reaches` so a disabled lane never opens a connection; an AGE case binds the graph name, the Cypher body, and the `params agtype` through `Npgsql` parameters and runs `SELECT * FROM ag_catalog.cypher(@graph, $$ … $$, @params) AS (result agtype)` with the mandatory column-definition list — `Match` extracts `(v->'properties'->>'id')` server-side through the registered `agtype` operators (a `MATCH … RETURN v` row IS one vertex, honestly a one-vertex path), `Mutate` runs the statement with no `RETURN` column, and `Reach` lowers the `minHops..maxHops` into the Cypher `*` range the `age_vle` engine plans, RETURNs the whole path `p`, casts `(p)::jsonb` through the REGISTERED cast, and decodes the alternating vertex/edge array through `GraphSession.DecodePath` so `AgtypePath` carries the real walk and the real cost sum; a `pgrouting` case binds the Edges-SQL string and the `H3Cell`-derived `bigint` vids (`GraphSession.Vid`) through `Npgsql` parameters and runs the `pgr_*` function with its PATH/VIA/MULTI-PATH/SPANTREE/TOUR/FLOW column-definition list (`api-pgrouting#ROUTING_FUNCTIONS`), reading a NODE `bigint` column back through `GraphSession.CellOf` into a CELL-space `H3Cell` sequence — `Via` groups the MULTI-PATH `path_id` rows into the route's LEGS and reads the route total off `route_agg_cost` (`edge = -2` marks the route-last node), `Located` marks a NEGATIVE `node` as a Point step (never `CellOf`'d), `TourPlanar` feeds the Coordinates-SQL directly (no matrix pre-pass), `Flow` reads the `(edge, start_vid, end_vid, flow, residual_capacity)` labeling and DERIVES the scalar max flow as the flow entering the sink (a scalar-only `pgr_maxFlow` that discards the labeling is the deleted hollow form), and `Cleave` routes its decode on the `(NodeValued, Partitioned)` row columns — node+partitioned components group to `Cleaved`, cut VERTICES land as one `Cleaved` partition, cut EDGES rail the raw `Severed` ids, and the biconnected EDGE-partition rails `Sundered` — never an enumerated `kind ==` roster check and never an edge id through `CellOf`; both legs lift any `PostgresException` through the `@catch` boundary into the typed `CypherFault` (`MatchFailed`/`RouteFailed`), and the A* mode upgrades the DEFAULT `EdgesSql.Network` to its xy quad (`pgr_aStar` requires the coordinate columns) while a caller-supplied Edges-SQL passes through UNMODIFIED — its xy contract is the caller's, never silently substituted.
- Receipt: every run rides the `ReceiptSinkPort` — an AGE read rides `store.graph.match` carrying the path count and watermark, an AGE mutation `store.graph.mutate` carrying the mutated-graph name (AGE's `cypher()` mutation is a `SETOF record` whose `ExecuteNonQuery` row count is not a reliable affected-row signal, so the mutation fact rides the graph identity, never a fabricated count), a route rides `store.graph.route` carrying the reached count and the aggregate cost, a flow rides `store.graph.flow` carrying the derived max flow and the labeled-edge count, a partition rides `store.graph.cleave` carrying the component count.
- Packages: Npgsql (`NpgsqlDataSource.OpenConnectionAsync`/`NpgsqlCommand`/`NpgsqlParameterCollection.AddWithValue`/`NpgsqlDataReader`/`PostgresException`), System.Text.Json (`JsonDocument` — the `(p)::jsonb` path decode), Rasm.Element (`NodeId`), Rasm.Persistence (`Element/identity#ELEMENT_IDENTITY` `H3Cell`, `Query/lane#ELEMENT_SET_ALGEBRA` `ElementSet`, `Element/graph#FAULT_TABLES` `FaultBand`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new openCypher or `pgrouting` verb is one `GraphQuery` case plus one `Switch` arm lowering to its server-side SQL; a new path function is one `RouteMode` row carrying the `pgr_*` name; a new flow algorithm is one `FlowKind` row; a new connectivity partition is one `CleaveKind` row carrying its `(NodeValued, Partitioned)` decode plane; a new result shape is one `GraphResult` case; zero new surface — a hand-rolled Dijkstra/BFS over the network table, a second routing engine beside `pgrouting`, a managed graph engine beside `age`, a runtime-concatenated Cypher/Edges-SQL body, a `cypher()` call without the column-definition list, a mutate-and-return in one Cypher statement, a fabricated `NodeId` from a cell string, a cut-EDGE or biconnected-EDGE id fed through `CellOf` into a phantom cell, a `pgr_withPoints` Point vid decoded as a cell, a scalar-only max flow discarding the per-edge labeling, an enumerated `kind ==` roster check beside the row columns, or a result outside the typed `GraphResult` is the deleted form because the extensions own the server-side algorithm, the in-process `Query/topology#TRAVERSAL` QuikGraph lane owns the synchronous TOPOLOGICAL counterpart, the AGE result composes at the `ElementSet`, the `pgrouting` NODE result at the `H3Cell` mesh, and the EDGE-valued results at their raw typed carriers.
- Boundary: `GraphQuery` is the ONE polymorphic verb surface so every AGE and `pgrouting` capability is one case under one total `Switch` — a sibling `Match`/`Route`/`DrivingDistance` method family, a `bool`/`mode` flag selecting a verb, or a thin slice of the rich `pgrouting` roster (Dijkstra/A*/bidirectional/VIA/withPoints/KSP/DD/TSP/TSPeuclidean/flow-family/components/biconnected) is the deleted form, `RouteMode`/`FlowKind`/`CleaveKind` carrying the function variance and the verb family carrying the rest; the parameterized form is mandatory — the Cypher body binds `params agtype` (`$name`) and the Edges/Points/Coordinates-SQL bind through `Npgsql` parameters, so a runtime-concatenated graph body is the rejected `api-apache-age`/`api-pgrouting` form and the `Match`/`Mutate`/`Reach` `Parameters` are LIVE, never a dead field; the `agtype` rows decode through `GraphSession.Decode`/`DecodePath` (the registered `->>`/`::jsonb` casts) so a returned vertex projects to the real seam `NodeId` and a returned path to the REAL multi-vertex walk, never the internal `graphid` integer, never a comma-split, never a one-vertex stub with a fabricated weight; `Via` and `Tour` stay TWO verbs because ordered-waypoint traversal and optimal-order touring are different problems (the input shape — a waypoint sequence versus a stop set with free order — is the discriminant, `MODAL_ARITY`), and `Tour`/`TourPlanar` discriminate on input shape too (an Edges-SQL cost matrix versus a Coordinates-SQL plane); the `pgrouting` NODE `bigint` is the `H3Cell` the in-process `pocketken.H3` cell shares so the in-database route and the in-process path agree on node identity (`api-pgrouting#GEO_LANE_STACK`), the cell→element resolution a one-hop `Element/identity#ELEMENT_IDENTITY` `NodeAt` the caller composes; a `pgr_bridges` cut-EDGE, a `pgr_biconnectedComponents` edge-component, a flow edge label, and a `pgr_withPoints` NEGATIVE Point vid are NOT nodes — they rail `Severed`/`Sundered`/`Flowed.Assignment`/`Traced` and never pass through `CellOf` (the fabricated-cell defect the decode-plane columns delete); the flow verbs return the PER-EDGE assignment because the cut, the bottleneck, and the residual analysis all need the labeling — the scalar is a derived fold, never the whole answer; the AGE write half (`Mutate`) is split from the read (`api-apache-age#CYPHER_QUERY` rejects a mutate-and-return statement) and the variable-length `Reach` lowers to the Cypher `*` range the `age_vle` engine plans, never a managed BFS; the network edge table and the AGE backing relations are async daemon projections off the one Marten stream so a routing/match query rides the one graph, never a second network store, and an interactive-correctness routing read binds the synchronous in-process counterpart.

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

// The labeled max-flow selector (api-pgrouting#DISTANCE_TSP_FLOW): every row returns the per-edge FLOW assignment
// `(seq, edge, start_vid, end_vid, flow, residual_capacity)` — the scalar max flow DERIVES from the labeling
// (the flow entering the sink), so `pgr_maxFlow`'s scalar-only answer is subsumed, never a sibling verb.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlowKind {
    public static readonly FlowKind BoykovKolmogorov = new("boykov-kolmogorov", "pgr_boykovKolmogorov");
    public static readonly FlowKind PushRelabel = new("push-relabel", "pgr_pushRelabel");
    public static readonly FlowKind EdmondsKarp = new("edmonds-karp", "pgr_edmondsKarp");
    public string Function { get; }
    private FlowKind(string key, string function) : this(key) => Function = function;
}

// The connectivity-partition selector on the (NodeValued, Partitioned) decode plane — the TWO row columns route
// the decode (node+partitioned → Cleaved groups, node+flat → cut VERTICES as one Cleaved partition, edge+flat →
// Severed cut edges, edge+partitioned → Sundered biconnected edge-components), replacing the enumerated
// `kind == Strong || kind == Connected` roster check. An EDGE id is NEVER fed through CellOf into a phantom cell.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CleaveKind {
    public static readonly CleaveKind Strong = new("strong", "pgr_strongComponents", nodeValued: true, partitioned: true);
    public static readonly CleaveKind Connected = new("connected", "pgr_connectedComponents", nodeValued: true, partitioned: true);
    public static readonly CleaveKind Articulation = new("articulation", "pgr_articulationPoints", nodeValued: true, partitioned: false);
    public static readonly CleaveKind Bridges = new("bridges", "pgr_bridges", nodeValued: false, partitioned: false);
    public static readonly CleaveKind Biconnected = new("biconnected", "pgr_biconnectedComponents", nodeValued: false, partitioned: true);
    public string Function { get; }
    public bool NodeValued { get; }
    public bool Partitioned { get; }
    private CleaveKind(string key, string function, bool nodeValued, bool partitioned) : this(key) => (Function, NodeValued, Partitioned) = (function, nodeValued, partitioned);
}

// --- [MODELS] -----------------------------------------------------------------------------
// The decoded AGE path — POPULATED: GraphSession.DecodePath iterates the `(p)::jsonb` alternating vertex/edge
// array, so Vertices is the real multi-vertex walk and Weight the real edge-cost sum (the length-1/0.0 stub was
// the E9 dead carrier this decode retires). The Edges-SQL contract (api-pgrouting#EDGES_SQL): id/source/target/
// cost (negative cost ⇒ edge absent), the xy quad for A*, projected from the federated Connect relationships.
public readonly record struct AgtypePath(Seq<NodeId> Vertices, double Weight);
public readonly record struct EdgesSql(string Inner) {
    // The federated network edge table the daemon projects from the Connect/Generic connection relationships — id/source/
    // target/cost for Dijkstra/components/flow, the xy quad appended for the A* heuristic, parameter-bound as one text arg.
    public static readonly EdgesSql Network = new("SELECT id, source, target, cost, reverse_cost FROM network_edge");
    public static readonly EdgesSql NetworkXy = new("SELECT id, source, target, cost, reverse_cost, x1, y1, x2, y2 FROM network_edge");
    public static readonly EdgesSql Capacity = new("SELECT id, source, target, capacity, reverse_capacity FROM network_edge");
}

// The pgr_withPoints Points-SQL contract `(pid, edge_id, fraction, side)`: a NEGATIVE routing id is a Point
// mid-edge — the off-node origin the cell mesh admits (api-pgrouting#ROUTING_FUNCTIONS row [10]).
public readonly record struct PointsSql(string Inner) {
    public static readonly PointsSql Network = new("SELECT pid, edge_id, fraction, side FROM network_point");
}

// The pgr_TSPeuclidean Coordinates-SQL contract `(id, x, y)`: coordinate TSP with NO cost-matrix pre-pass —
// cheaper than the `format('%L')` pgr_dijkstraCostMatrix composition whenever the cell coordinates are in hand.
public readonly record struct CoordinatesSql(string Inner) {
    public static readonly CoordinatesSql Vertices = new("SELECT id, x, y FROM network_vertex");
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
    public sealed record Via(EdgesSql Edges, Seq<H3Cell> Waypoints) : GraphQuery;
    public sealed record Located(EdgesSql Edges, PointsSql Points, long From, long To) : GraphQuery;
    public sealed record Kth(EdgesSql Edges, H3Cell From, H3Cell To, int K) : GraphQuery;
    public sealed record Spread(EdgesSql Edges, H3Cell Root, double Radius) : GraphQuery;
    public sealed record Tour(EdgesSql Edges, Seq<H3Cell> Stops, H3Cell Start, H3Cell End) : GraphQuery;
    public sealed record TourPlanar(CoordinatesSql Sites, H3Cell Start, H3Cell End) : GraphQuery;
    public sealed record Flow(EdgesSql Edges, H3Cell Source, H3Cell Sink, FlowKind Kind) : GraphQuery;
    public sealed record Cleave(EdgesSql Edges, CleaveKind Kind) : GraphQuery;
}

// AGE results are NODE-space (the decoded `properties.id` are real seam NodeIds → an ElementSet directly). pgrouting
// NODE results are CELL-space (the `node bigint` are H3 cells — NOT element NodeIds, many elements per cell), resolved
// cells→elements ONE-HOP through Element/identity#ELEMENT_IDENTITY NodeAt. EDGE-valued results (bridges, biconnected
// components, the flow labeling) and the withPoints NEGATIVE Point vids stay RAW typed carriers — never CellOf'd.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphResult {
    private GraphResult() { }
    public sealed record Paths(Seq<AgtypePath> Found) : GraphResult;                                    // AGE Match/Reach — node-space; Reach carries the FULL decoded walk
    public sealed record Routed(Seq<H3Cell> Path, double Cost) : GraphResult;                           // pgrouting Path — cell-space
    public sealed record Legged(Seq<(Seq<H3Cell> Leg, double Cost)> Legs, double Total) : GraphResult;  // pgr_dijkstraVia — ordered-waypoint legs + route total
    public sealed record Traced(Seq<(long Vid, bool Point)> Steps, double Cost) : GraphResult;          // pgr_withPoints — a negative vid is a mid-edge Point, never a cell
    public sealed record Ranked(Seq<(Seq<H3Cell> Path, double Cost)> Alternatives) : GraphResult;       // pgr_KSP — cell-space, path_id-ascending alternatives
    public sealed record Spanned(Seq<H3Cell> Reached, double Radius) : GraphResult;                     // pgr_drivingDistance — cell-space
    public sealed record Toured(Seq<H3Cell> Order, double Cost) : GraphResult;                          // pgr_TSP / pgr_TSPeuclidean — cell-space
    public sealed record Flowed(long MaxFlow, Seq<(long Edge, long Flow, long Residual)> Assignment) : GraphResult;  // labeled max-flow — edge-space; the scalar derives from the labeling
    public sealed record Cleaved(Seq<Seq<H3Cell>> Components) : GraphResult;                            // components/articulation — cell-space NODE partitions
    public sealed record Severed(Seq<long> Edges) : GraphResult;                                        // pgr_bridges — raw cut-EDGE ids
    public sealed record Sundered(Seq<Seq<long>> EdgeComponents) : GraphResult;                         // pgr_biconnectedComponents — EDGE partitions, never cells
}

public static class GraphLane {
    // The ONE entrypoint: gate once, then dispatch the total Switch. A disabled lane never opens a connection (it rails
    // CypherFault.Disabled); each arm lowers to its parameterized server-side SQL and decodes its rows — never a
    // concatenated body, never a hand-parse.
    public static IO<Fin<GraphResult>> Run(NpgsqlDataSource source, CypherEnablement gate, GraphQuery query) =>
        gate.Reaches
            ? query.Switch(
                state: source,
                match: static (db, m) => Cypher(db, m.Graph, m.Clause, m.Parameters, decode: true).Map(static r => r.Map(static p => (GraphResult)new GraphResult.Paths(p))),
                mutate: static (db, m) => Cypher(db, m.Graph, m.Clause, m.Parameters, decode: false).Map(static r => r.Map(static p => (GraphResult)new GraphResult.Paths(p))),
                reach: static (db, r) => Traverse(db, r.Graph, r.From, r.MinHops, r.MaxHops),
                path: static (db, p) => Route(db, p.Mode.Function, p.Mode.Xy && p.Edges == EdgesSql.Network ? EdgesSql.NetworkXy : p.Edges, p.From, p.To),
                via: static (db, v) => Via(db, v.Edges, v.Waypoints),
                located: static (db, l) => Located(db, l.Edges, l.Points, l.From, l.To),
                kth: static (db, k) => Ksp(db, k.Edges, k.From, k.To, k.K),
                spread: static (db, s) => Spread(db, s.Edges, s.Root, s.Radius),
                tour: static (db, t) => Tour(db, t.Edges, t.Stops, t.Start, t.End),
                tourPlanar: static (db, t) => TourPlanar(db, t.Sites, t.Start, t.End),
                flow: static (db, f) => Flow(db, f.Edges, f.Source, f.Sink, f.Kind),
                cleave: static (db, c) => Cleave(db, c.Edges, c.Kind))
            : IO.pure(Fin<GraphResult>.Fail(new CypherFault.Disabled()));

    // Project the NODE-space AGE result to the ElementSet selection currency so a graph query composes with every other
    // selection (a Match path intersected with a Classification selection is one SetExpr.Intersect, never an app join).
    public static ElementSet ToSet(GraphResult result) => result.Switch(
        paths: static p => ElementSet.Of(p.Found.Bind(static x => x.Vertices)),
        routed: static _ => ElementSet.Empty,
        legged: static _ => ElementSet.Empty,
        traced: static _ => ElementSet.Empty,
        ranked: static _ => ElementSet.Empty,
        spanned: static _ => ElementSet.Empty,
        toured: static _ => ElementSet.Empty,
        flowed: static _ => ElementSet.Empty,
        cleaved: static _ => ElementSet.Empty,
        severed: static _ => ElementSet.Empty,      // cut-EDGE ids are neither node-space nor cell-space
        sundered: static _ => ElementSet.Empty);    // biconnected EDGE partitions likewise

    // The CELL-space projection: the navigable H3 mesh the route/spread/tour/component lane walks, resolved to elements
    // via Element/identity#ELEMENT_IDENTITY NodeAt(cell). Edge-valued results and Point steps carry NO cells — a Traced
    // step projects only where it is a real vertex (never a negative Point vid), so the spaces cannot conflate.
    public static Seq<H3Cell> ToCells(GraphResult result) => result.Switch(
        paths: static _ => Seq<H3Cell>(),
        routed: static r => r.Path,
        legged: static l => l.Legs.Bind(static leg => leg.Leg),
        traced: static t => t.Steps.Filter(static s => !s.Point).Map(static s => GraphSession.CellOf(s.Vid)),
        ranked: static k => k.Alternatives.Bind(static a => a.Path),
        spanned: static s => s.Reached,
        toured: static t => t.Order,
        flowed: static _ => Seq<H3Cell>(),          // a flow edge label is an edge fact, not a cell
        cleaved: static c => c.Components.Bind(static x => x),
        severed: static _ => Seq<H3Cell>(),
        sundered: static _ => Seq<H3Cell>());

    // --- [CYPHER] -------------------------------------------------------------------------
    // AGE read/write: bind the graph name and the params agtype through Npgsql parameters, run cypher() with the mandatory
    // column-definition list. A Match read extracts `properties.id` SERVER-SIDE through the registered agtype operators
    // (a MATCH row IS one vertex); a mutate runs with no RETURN column and returns the empty path Seq.
    static IO<Fin<Seq<AgtypePath>>> Cypher(NpgsqlDataSource source, string graph, string clause, HashMap<string, string> parameters, bool decode) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
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
        }) | @catch<IO, Fin<Seq<AgtypePath>>>(static _ => true, static e => IO.pure(Fin<Seq<AgtypePath>>.Fail(new CypherFault.MatchFailed(e.Message))));

    // The Reach lowering: RETURN the WHOLE path and cast it `(p)::jsonb` through the REGISTERED agtype→jsonb cast
    // server-side, so the reader receives one typed JSON column per row and DecodePath populates the real multi-vertex
    // walk + cost sum — the length-1/0.0 stub was the E9 dead carrier this decode retires.
    static IO<Fin<GraphResult>> Traverse(NpgsqlDataSource source, string graph, NodeId from, int minHops, int maxHops) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT (p)::jsonb FROM ag_catalog.cypher(@graph, $$MATCH p = (a {id:$from})-[*" + minHops.ToString(CultureInfo.InvariantCulture) + ".." + maxHops.ToString(CultureInfo.InvariantCulture) + "]->(b) RETURN p$$, @params) AS (p agtype)";
            command.Parameters.AddWithValue("graph", graph);
            command.Parameters.AddWithValue("params", NpgsqlTypes.NpgsqlDbType.Unknown, JsonSerializer.Serialize(new Dictionary<string, string> { ["from"] = from.Value }));
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var found = new List<AgtypePath>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
            while (await reader.ReadAsync().ConfigureAwait(false)) {
                var step = GraphSession.DecodePath(reader.GetString(0));
                if (step.IsFail) { return step.Map(static _ => (GraphResult)new GraphResult.Paths(Seq<AgtypePath>())); }
                step.Iter(found.Add);
            }
            return Fin.Succ((GraphResult)new GraphResult.Paths(toSeq(found)));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new CypherFault.MatchFailed(e.Message))));

    // --- [ROUTING] ------------------------------------------------------------------------
    // pgrouting shortest path: bind the Edges-SQL string and the H3Cell-derived bigint vids through Npgsql parameters, run
    // the RouteMode-selected pgr_* function with the FULL PATH column-definition list (seq, path_seq, start_vid, end_vid,
    // node, edge, cost, agg_cost — api-pgrouting#ROUTING_FUNCTIONS), read `node bigint` back through CellOf. edge = -1 is
    // the last-node sentinel, so the path is the node column ascending agg_cost.
    static IO<Fin<GraphResult>> Route(NpgsqlDataSource source, string function, EdgesSql edges, H3Cell from, H3Cell to) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT node, agg_cost FROM {function}(@edges, @from, @to, directed => true) AS (seq integer, path_seq integer, start_vid bigint, end_vid bigint, node bigint, edge bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            command.Parameters.AddWithValue("from", GraphSession.Vid(from));
            command.Parameters.AddWithValue("to", GraphSession.Vid(to));
            return Fin.Succ((GraphResult)await ReadRoute(command).ConfigureAwait(false));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new CypherFault.RouteFailed(e.Message))));

    // pgr_dijkstraVia (api-pgrouting#ROUTING_FUNCTIONS): VIA output = MULTI-PATH + route_agg_cost, `edge = -2` on the
    // route-last node — the path_id groups are the LEGS of ONE ordered-waypoint route, the total the max route_agg_cost.
    static IO<Fin<GraphResult>> Via(NpgsqlDataSource source, EdgesSql edges, Seq<H3Cell> waypoints) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT path_id, node, agg_cost, route_agg_cost FROM pgr_dijkstraVia(@edges, @vids, directed => true) AS (seq integer, path_id integer, path_seq integer, start_vid bigint, end_vid bigint, node bigint, edge bigint, cost float, agg_cost float, route_agg_cost float)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            command.Parameters.AddWithValue("vids", waypoints.Map(GraphSession.Vid).ToArray());
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var rows = new List<(int Path, H3Cell Node, double Cost, double RouteCost)>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
            while (await reader.ReadAsync().ConfigureAwait(false)) { rows.Add((reader.GetInt32(0), GraphSession.CellOf(reader.GetInt64(1)), reader.GetDouble(2), reader.GetDouble(3))); }
            var legs = toSeq(rows.GroupBy(static r => r.Path).OrderBy(static g => g.Key)
                .Select(static g => (Leg: toSeq(g.Select(static r => r.Node)), Cost: g.Max(static r => r.Cost))));
            return Fin.Succ((GraphResult)new GraphResult.Legged(legs, rows.Count > 0 ? rows.Max(static r => r.RouteCost) : 0.0));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new CypherFault.RouteFailed(e.Message))));

    // pgr_withPoints: off-node routing over the Points-SQL — a NEGATIVE routing id is a Point mid-edge, so the decode
    // marks it a Point step and NEVER projects it through CellOf (the mixed carrier keeps the spaces honest).
    static IO<Fin<GraphResult>> Located(NpgsqlDataSource source, EdgesSql edges, PointsSql points, long from, long to) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT node, agg_cost FROM pgr_withPoints(@edges, @points, @from, @to, directed => true) AS (seq integer, path_seq integer, start_vid bigint, end_vid bigint, node bigint, edge bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            command.Parameters.AddWithValue("points", points.Inner);
            command.Parameters.AddWithValue("from", from);
            command.Parameters.AddWithValue("to", to);
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var steps = new List<(long Vid, bool Point)>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
            var cost = 0.0;
            while (await reader.ReadAsync().ConfigureAwait(false)) { long vid = reader.GetInt64(0); steps.Add((vid, vid < 0)); cost = reader.GetDouble(1); }
            return Fin.Succ((GraphResult)new GraphResult.Traced(toSeq(steps), cost));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new CypherFault.RouteFailed(e.Message))));

    // pgrouting K-shortest-paths (`pgr_KSP`): MULTI-PATH = PATH + path_id (path_id=1 the cheapest), so the alternatives
    // group by `path_id` ascending into the cell-space `Ranked` carrier — never a managed re-implementation of KSP.
    static IO<Fin<GraphResult>> Ksp(NpgsqlDataSource source, EdgesSql edges, H3Cell from, H3Cell to, int k) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT path_id, node, agg_cost FROM pgr_KSP(@edges, @from, @to, @k, directed => true) AS (seq integer, path_id integer, path_seq integer, start_vid bigint, end_vid bigint, node bigint, edge bigint, cost float, agg_cost float)";
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
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new CypherFault.RouteFailed(e.Message))));

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
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new CypherFault.RouteFailed(e.Message))));

    // The TSP Matrix SQL is NOT a CLIENT-concatenated string (a runtime-concatenated routing body is the rejected
    // api-pgrouting form): `pgr_TSP`'s first argument is a Matrix-SQL TEXT that pgRouting re-parses server-side, and
    // an `@edges`/`@vids` Npgsql parameter does NOT bind inside that re-parsed literal — so the matrix-SQL is composed
    // SERVER-SIDE through PostgreSQL `format()` with `%L` for BOTH bound args: the Edges-SQL literal-quotes injection-safe,
    // and the `bigint[]` vids render through `@vids::text` (the PG array OUTPUT form `{1,2,3}`) literal-quoted by `%L`
    // (`'{1,2,3}'`) then RE-CAST `::bigint[]` — a bare `%s` would inject the unquoted array OUTPUT form, never valid
    // array INPUT syntax. `directed => false` is the symmetric metric `pgr_TSP` requires; 4.0 `pgr_TSP` dropped the
    // annealing knobs (only `start_id`/`end_id` remain).
    static IO<Fin<GraphResult>> Tour(NpgsqlDataSource source, EdgesSql edges, Seq<H3Cell> stops, H3Cell start, H3Cell end) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT node, agg_cost FROM pgr_TSP(format('SELECT * FROM pgr_dijkstraCostMatrix(%L, %L::bigint[], directed => false)', @edges, @vids::text), start_id => @start, end_id => @end) AS (seq integer, node bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            command.Parameters.AddWithValue("vids", stops.Map(GraphSession.Vid).ToArray());
            command.Parameters.AddWithValue("start", GraphSession.Vid(start));
            command.Parameters.AddWithValue("end", GraphSession.Vid(end));
            return Fin.Succ((GraphResult)await ReadTour(command).ConfigureAwait(false));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new CypherFault.RouteFailed(e.Message))));

    // pgr_TSPeuclidean: the coordinate tour — the Coordinates-SQL binds directly, NO pgr_dijkstraCostMatrix pre-pass
    // and no format('%L') composition, the cheaper metric tour whenever the cell coordinates are already in hand.
    static IO<Fin<GraphResult>> TourPlanar(NpgsqlDataSource source, CoordinatesSql sites, H3Cell start, H3Cell end) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT node, agg_cost FROM pgr_TSPeuclidean(@sites, start_id => @start, end_id => @end) AS (seq integer, node bigint, cost float, agg_cost float)";
            command.Parameters.AddWithValue("sites", sites.Inner);
            command.Parameters.AddWithValue("start", GraphSession.Vid(start));
            command.Parameters.AddWithValue("end", GraphSession.Vid(end));
            return Fin.Succ((GraphResult)await ReadTour(command).ConfigureAwait(false));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new CypherFault.RouteFailed(e.Message))));

    // The labeled max-flow family (api-pgrouting#DISTANCE_TSP_FLOW): FLOW output = (seq, edge, start_vid, end_vid,
    // flow, residual_capacity) — the PER-EDGE assignment the cut/bottleneck/residual analyses need; the scalar max
    // flow DERIVES as the flow entering the sink, so the pgr_maxFlow scalar-only call (which discards the labeling)
    // is the deleted hollow form.
    static IO<Fin<GraphResult>> Flow(NpgsqlDataSource source, EdgesSql edges, H3Cell flowSource, H3Cell sink, FlowKind kind) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT edge, start_vid, end_vid, flow, residual_capacity FROM {kind.Function}(@edges, @source, @sink) AS (seq integer, edge bigint, start_vid bigint, end_vid bigint, flow bigint, residual_capacity bigint)";
            command.Parameters.AddWithValue("edges", edges.Inner);
            command.Parameters.AddWithValue("source", GraphSession.Vid(flowSource));
            command.Parameters.AddWithValue("sink", GraphSession.Vid(sink));
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var labeled = new List<(long Edge, long Start, long End, long Flow, long Residual)>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
            while (await reader.ReadAsync().ConfigureAwait(false)) { labeled.Add((reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt64(3), reader.GetInt64(4))); }
            long into = GraphSession.Vid(sink);
            return Fin.Succ((GraphResult)new GraphResult.Flowed(
                labeled.Where(row => row.End == into).Sum(static row => row.Flow),
                toSeq(labeled.Select(static row => (row.Edge, row.Flow, row.Residual)))));
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new CypherFault.RouteFailed(e.Message))));

    // FOUR decode shapes on the (NodeValued, Partitioned) plane (api-pgrouting#COMPONENTS_TOPOLOGY), routed by the row
    // columns — never an enumerated `kind ==` roster check: node+partitioned `(seq, component, node)` → Cleaved groups;
    // node+flat `SETOF bigint` cut VERTICES → one Cleaved partition; edge+flat `SETOF bigint` cut EDGES → Severed raw
    // ids; edge+partitioned `(seq, component, edge)` → Sundered edge-components. An EDGE id is NEVER fed through CellOf.
    static IO<Fin<GraphResult>> Cleave(NpgsqlDataSource source, EdgesSql edges, CleaveKind kind) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = (kind.NodeValued, kind.Partitioned) switch {
                (true, true) => $"SELECT component, node FROM {kind.Function}(@edges) AS (seq bigint, component bigint, node bigint)",
                (false, true) => $"SELECT component, edge FROM {kind.Function}(@edges) AS (seq bigint, component bigint, edge bigint)",
                _ => $"SELECT {kind.Function} AS id FROM {kind.Function}(@edges)",
            };
            command.Parameters.AddWithValue("edges", edges.Inner);
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var rows = new List<(long Component, long Id)>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
            while (await reader.ReadAsync().ConfigureAwait(false)) { rows.Add(kind.Partitioned ? (reader.GetInt64(0), reader.GetInt64(1)) : (0L, reader.GetInt64(0))); }
            return Fin.Succ((kind.NodeValued, kind.Partitioned) switch {
                (true, _) => (GraphResult)new GraphResult.Cleaved(toSeq(rows.GroupBy(static r => r.Component).Select(static g => toSeq(g.Select(static r => GraphSession.CellOf(r.Id)))))),
                (false, true) => new GraphResult.Sundered(toSeq(rows.GroupBy(static r => r.Component).Select(static g => toSeq(g.Select(static r => r.Id))))),
                (false, false) => new GraphResult.Severed(toSeq(rows.Select(static r => r.Id))),
            });
        }) | @catch<IO, Fin<GraphResult>>(static _ => true, static e => IO.pure(Fin<GraphResult>.Fail(new CypherFault.RouteFailed(e.Message))));

    static async System.Threading.Tasks.Task<GraphResult> ReadRoute(NpgsqlCommand command) {
        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        var path = new List<H3Cell>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
        var cost = 0.0;
        while (await reader.ReadAsync().ConfigureAwait(false)) { path.Add(GraphSession.CellOf(reader.GetInt64(0))); cost = reader.GetDouble(1); }
        return new GraphResult.Routed(toSeq(path), cost);
    }

    static async System.Threading.Tasks.Task<GraphResult> ReadTour(NpgsqlCommand command) {
        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        var order = new List<H3Cell>();   // Exemption: seam-local drain frozen once by toSeq; per-row Seq.Add is the rejected O(n²) form (data-interchange#CHUNK_ALGEBRA)
        var cost = 0.0;
        while (await reader.ReadAsync().ConfigureAwait(false)) { order.Add(GraphSession.CellOf(reader.GetInt64(0))); cost = reader.GetDouble(1); }
        return new GraphResult.Toured(toSeq(order), cost);
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                   | [BINDING]                                                  |
| :-----: | :------------------ | :---------------------------------------- | :--------------------------------------------------------- |
|  [01]   | one verb surface    | `GraphQuery` `[Union]` + total `Switch`   | every AGE + `pgrouting` verb; no sibling method family     |
|  [02]   | parameterization    | `params agtype` + Edges/Points/Coordinates-SQL `Npgsql` args | never a concatenated body; `Parameters` are live |
|  [03]   | `pgrouting` roster   | Dijkstra/A*/bidi/VIA/withPoints/KSP/DD/TSP/TSPeuclidean/flow-family/components+biconnected | the FULL roster; `RouteMode`/`FlowKind`/`CleaveKind` rows carry the function variance |
|  [04]   | AGE read/write       | `Match` decode vs `Mutate` no-return       | a mutate-and-return statement is the rejected AGE form     |
|  [05]   | path decode          | `Reach` → `(p)::jsonb` registered cast → `DecodePath` | populated multi-vertex `Vertices` + summed `Weight`; never a one-vertex stub |
|  [06]   | node-space agreement | `H3Cell` `long` ↔ in-process `pocketken.H3` | in-database route and in-process path share node identity  |
|  [07]   | result spaces        | AGE → `ElementSet`; `pgrouting` NODE → `H3Cell`; EDGE → `Severed`/`Sundered`/`Flowed`; a `Traced` negative vid stays a Point | spaces never conflate; an edge or Point id is never `CellOf`'d |
|  [08]   | cell→element resolve | `Element/identity#ELEMENT_IDENTITY` `NodeAt` | the caller's one-hop; a cell is the per-model envelope locator |
|  [09]   | flow honesty         | `FlowKind` per-edge labeling; scalar derived | a scalar-only `pgr_maxFlow` that discards the labeling is the deleted hollow form |
|  [10]   | algorithm dedup      | server-side `pgr_*`/`age` vs in-process    | the two meet at the shared node space, never re-implemented |
