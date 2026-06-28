# [PERSISTENCE_STORE_PROVISIONING]

Rasm.Persistence provisions the PostgreSQL 18 server tier as VERIFICATION-FIRST: the server extensions every analytical and graph lane needs (`pg_duckdb`, `timescaledb-toolkit`, `postgis_raster`, `postgis_sfcgal`, `pgvector`, `pgvectorscale`, `pg_search`, `apache-age`, `h3-pg`, `pgrouting`, `pg_cron`, `pg_partman`, `pg_net`, `pg_graphql`) are admitted through raw `CREATE EXTENSION` SQL gated by each extension's preload/type/access-method requirement, and the cluster configuration (`shared_preload_libraries`, `wal_level=logical`, the memory/parallelism knobs) is READ and VERIFIED against the required set — a Rasm process NEVER executes runtime `ALTER SYSTEM` and NEVER spawns or bundles PostgreSQL, so provisioning is a typed `ProvisionVerdict` over what the operator-provisioned cluster already carries. Each extension is a `ServerExtension` row carrying its `CREATE EXTENSION` SQL, its preload requirement, and the lane it serves; `ClusterConfig` reads the `pg_settings`/`pg_available_extensions` catalogs into the verdict. Beneath the server tier the embedded SQLite floor (`Microsoft.Data.Sqlite` over `SQLitePCLRaw.bundle_e_sqlite3`) is the same idempotent open ritual (PRAGMA ladder, WAL, capability registration) for a single-process embedded store — the one engine sweep is closed, no second relational engine is admitted. `ClockPolicy`, `ReceiptSinkPort` arrive from AppHost; `Npgsql`/`Microsoft.Data.Sqlite` from the substrate; the analytical lanes that consume the extensions arrive from `Query/columnar`/`Query/cypher`.

## [01]-[INDEX]

- [01]-[SERVER_EXTENSIONS]: the extension roster, the preload-gated `CREATE EXTENSION` provisioning, and the cluster-config verification.
- [02]-[EMBEDDED_FLOOR]: the embedded SQLite open ritual, the WAL/pragma ladder, and the closed-engine law.

## [02]-[SERVER_EXTENSIONS]

- Owner: `ServerExtension` the `[SmartEnum<string>]` extension axis carrying its `CREATE EXTENSION` SQL, its `Preload` requirement, and the lane it serves; `ClusterSetting` the verified-setting vocabulary; `ProvisionVerdict` the closed verification verdict; `ClusterConfig` the static surface reading the catalogs and folding the required set into the verdict — never executing `ALTER SYSTEM`.
- Cases: `ServerExtension` rows — `pg_duckdb` (preload, the in-PG DuckDB analytical bridge), `timescaledb-toolkit` (preload, hypertable/continuous-aggregate analytics), `postgis_raster`/`postgis_sfcgal` (PostGIS raster + exact 3D geometry), `pgvector`/`pgvectorscale` (the HNSW + diskann ANN tier), `pg_search` (ParadeDB BM25), `apache-age` (preload, the optional openCypher graph, `Query/cypher`), `h3-pg` (the in-PG H3 cell index matching `pocketken.H3`), `pgrouting` (the network routing, `Query/cypher#ROUTING_LANE`), `pg_cron` (preload, in-database maintenance scheduler), `pg_partman` (declarative partition maintenance), `pg_net` (async HTTP from SQL), `pg_graphql` (in-PG GraphQL reflection); `ProvisionVerdict` is `Provisioned | MissingExtension | MissingPreload | SettingDrift`.
- Entry: `public static IO<ProvisionVerdict> Verify(NpgsqlDataSource source, Seq<ServerExtension> required)` reads `pg_available_extensions`/`pg_extension` and the `shared_preload_libraries` setting and folds the required set into one verdict; `public static IO<Fin<Unit>> Admit(IDocumentSession session, ServerExtension extension)` runs the `CREATE EXTENSION IF NOT EXISTS` SQL for an extension whose preload requirement the cluster already satisfies, never for one that needs an un-loaded library.
- Auto: provisioning is verification-first — `Verify` reads `pg_available_extensions` (installed but not created), `pg_extension` (created), `current_setting('shared_preload_libraries')`, and the `pg_settings` rows for every `ClusterSetting` (`wal_level=logical` plus the parallelism/worker knobs) then folds the required roster into `Provisioned` or a typed gap, a setting whose live value fails its row's `Satisfied` check folding `SettingDrift(name, expected, actual)`; a preload-gated extension (`pg_duckdb`/`timescaledb-toolkit`/`apache-age`/`pg_cron`) whose library is absent from `shared_preload_libraries` is `MissingPreload` (the operator must add it to the cluster config and restart, never a runtime `ALTER SYSTEM`); a created extension is admitted through `CREATE EXTENSION IF NOT EXISTS` inside the one `IDocumentSession` so the DDL commits with the schema migration; the `h3-pg` cell id matches the managed `pocketken.H3` so the same cell indexes at ingest and in SQL.
- Receipt: a verification rides `store.provision.verify` carrying the verdict and the missing set; an admission rides `store.provision.admit` carrying the extension.
- Packages: Npgsql, Marten, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new server extension is one `ServerExtension` row carrying its SQL and preload requirement; a new verified setting is one `ClusterSetting` row; zero new surface — a runtime `ALTER SYSTEM`, a Rasm-spawned PostgreSQL, a per-extension managed package, or a second relational engine row is the deleted form because provisioning is verification-first SQL and the engine sweep is closed.
- Boundary: a Rasm process NEVER spawns or bundles PostgreSQL and NEVER executes runtime `ALTER SYSTEM` — provisioning is verification-only over the operator-provisioned cluster, so a `MissingPreload`/`SettingDrift` verdict is a typed signal the operator resolves at the cluster config, never a self-mutation; the server extensions carry no managed assembly and are admitted through raw `CREATE EXTENSION` SQL gated by each extension's preload/type/access-method requirement; the `pg_duckdb` extension is the in-PG DuckDB bridge distinct from the in-process `DuckDB.NET` analytical lane (`Query/columnar`), the two meeting at the columnar SQL surface; `apache-age` is the OPTIONAL self-hosted openCypher graph (`Query/cypher#CYPHER_LANE`) demoted beneath the in-process QuikGraph, so its admission is gated and the lane is disabled by default; spatial→PG GiST (`postgis_raster`/`postgis_sfcgal`) and ANN→`pgvector`/`pgvectorscale` are the transactional index owners while DuckDB `spatial`/`vss` are the columnar aggregators (`L2`), never duplicated.

```csharp signature
public sealed class ProvisionKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ProvisionKeyPolicy, string>]
[KeyMemberComparer<ProvisionKeyPolicy, string>]
public sealed partial class ServerExtension {
    public static readonly ServerExtension PgDuckdb = new("pg_duckdb", preload: true);
    public static readonly ServerExtension TimescaleToolkit = new("timescaledb_toolkit", preload: true);
    public static readonly ServerExtension PostgisRaster = new("postgis_raster", preload: false);
    public static readonly ServerExtension PostgisSfcgal = new("postgis_sfcgal", preload: false);
    public static readonly ServerExtension Pgvector = new("vector", preload: false);
    public static readonly ServerExtension Pgvectorscale = new("vectorscale", preload: false);
    public static readonly ServerExtension PgSearch = new("pg_search", preload: false);
    public static readonly ServerExtension ApacheAge = new("age", preload: true);
    public static readonly ServerExtension H3Pg = new("h3", preload: false);
    public static readonly ServerExtension Pgrouting = new("pgrouting", preload: false);
    public static readonly ServerExtension PgCron = new("pg_cron", preload: true);
    public static readonly ServerExtension PgPartman = new("pg_partman", preload: false);
    public static readonly ServerExtension PgNet = new("pg_net", preload: false);
    public static readonly ServerExtension PgGraphql = new("pg_graphql", preload: false);
    public bool Preload { get; }
    private ServerExtension(string key, bool preload) : this(key) => Preload = preload;
    public string CreateSql => $"CREATE EXTENSION IF NOT EXISTS {Key} CASCADE;";
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ProvisionKeyPolicy, string>]
[KeyMemberComparer<ProvisionKeyPolicy, string>]
public sealed partial class ClusterSetting {
    public static readonly ClusterSetting WalLevel = new("wal_level", expected: "logical", atLeast: false);
    public static readonly ClusterSetting MaxWorkerProcesses = new("max_worker_processes", expected: "8", atLeast: true);
    public static readonly ClusterSetting MaxParallelWorkers = new("max_parallel_workers", expected: "8", atLeast: true);
    public static readonly ClusterSetting MaxParallelWorkersPerGather = new("max_parallel_workers_per_gather", expected: "4", atLeast: true);
    public string Expected { get; }
    public bool AtLeast { get; }
    private ClusterSetting(string key, string expected, bool atLeast) : this(key) => (Expected, AtLeast) = (expected, atLeast);
    // A min-threshold knob is satisfied at or above its floor; an exact-match knob (wal_level) by value equality.
    public bool Satisfied(string actual) => AtLeast
        ? long.TryParse(actual, out var a) && long.TryParse(Expected, out var e) && a >= e
        : string.Equals(actual, Expected, StringComparison.OrdinalIgnoreCase);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvisionVerdict {
    private ProvisionVerdict() { }
    public sealed record Provisioned(Seq<ServerExtension> Present) : ProvisionVerdict;
    public sealed record MissingExtension(Seq<ServerExtension> Absent) : ProvisionVerdict;
    public sealed record MissingPreload(Seq<ServerExtension> Unloaded) : ProvisionVerdict;
    public sealed record SettingDrift(string Setting, string Expected, string Actual) : ProvisionVerdict;
}

public static class ClusterConfig {
    public static IO<ProvisionVerdict> Verify(NpgsqlDataSource source, Seq<ServerExtension> required) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var preloadCmd = new NpgsqlCommand("SELECT current_setting('shared_preload_libraries')", connection);
            var preloaded = (await preloadCmd.ExecuteScalarAsync().ConfigureAwait(false) as string ?? "").Split(',', StringSplitOptions.TrimEntries);
            var created = Seq<string>();
            await using (var availCmd = new NpgsqlCommand("SELECT extname FROM pg_extension", connection))
            await using (var reader = await availCmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                while (await reader.ReadAsync().ConfigureAwait(false)) created = created.Add(reader.GetString(0));
            }
            var settings = new Dictionary<string, string>(StringComparer.Ordinal);
            await using (var settingCmd = new NpgsqlCommand("SELECT name, setting FROM pg_settings WHERE name = ANY(@names)", connection)) {
                settingCmd.Parameters.AddWithValue("names", toSeq(ClusterSetting.Items).Map(static s => s.Key).ToArray());
                await using var settingReader = await settingCmd.ExecuteReaderAsync().ConfigureAwait(false);
                while (await settingReader.ReadAsync().ConfigureAwait(false)) settings[settingReader.GetString(0)] = settingReader.GetString(1);
            }
            var missingPreload = required.Filter(e => e.Preload && !preloaded.Contains(e.Key));
            var missing = required.Filter(e => !created.Contains(e.Key) && (!e.Preload || preloaded.Contains(e.Key)));
            var drift = toSeq(ClusterSetting.Items).Find(s => !s.Satisfied(settings.GetValueOrDefault(s.Key, "")));
            return !missingPreload.IsEmpty ? (ProvisionVerdict)new ProvisionVerdict.MissingPreload(missingPreload)
                : !missing.IsEmpty ? new ProvisionVerdict.MissingExtension(missing)
                : drift.Match(
                    Some: s => (ProvisionVerdict)new ProvisionVerdict.SettingDrift(s.Key, s.Expected, settings.GetValueOrDefault(s.Key, "")),
                    None: () => new ProvisionVerdict.Provisioned(required));
        });

    public static IO<Fin<Unit>> Admit(IDocumentSession session, ServerExtension extension) =>
        IO.liftAsync(async () => {
            session.QueueSqlCommand(extension.CreateSql);
            await session.SaveChangesAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }) | @catch<IO, Fin<Unit>>(static _ => true, e => IO.pure(Fin<Unit>.Fail(Error.New(8380, $"<provision-admit:{e.Message}>"))));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | provisioning stance | verification-first                     | never `ALTER SYSTEM`; never spawns PostgreSQL             |
|  [02]   | extension admission | raw `CREATE EXTENSION IF NOT EXISTS`   | preload-gated; no managed package per extension           |
|  [03]   | preload gap         | `MissingPreload` verdict               | operator resolves at cluster config + restart            |
|  [04]   | setting drift       | `pg_settings` vs `ClusterSetting`      | `wal_level=logical` + knobs; mismatch folds `SettingDrift`|
|  [05]   | h3 parity           | `h3-pg` matches `pocketken.H3`         | one cell id at ingest and in SQL                          |

## [03]-[EMBEDDED_FLOOR]

- Owner: `EmbeddedRitual` the idempotent open-ritual fold (identity check, per-connection pragmas, hardening, capability registration, epoch read); `EmbeddedStore` the static surface owning the dialed connection, the WAL-and-pragma ladder, and the closed-engine law.
- Entry: `public static SqliteConnection Dialed(string path)` opens the embedded connection with the canonical connection-string posture; `public static Fin<Seq<RitualFact>> Open(SqliteConnection store, EmbeddedRitual ritual)` folds the declared ritual sequence end-to-end idempotently, every throwing provider call (`Open`, the pragma `Execute`/`Scalar` rows, `BeginTransaction`, `gate.Commit`) staying INSIDE the `Fin` boundary so a provider fault converts to `Fin.Fail` and disposes the connection on every failure path rather than escaping the rail with a leaked live handle.
- Auto: every connection in every process folds the same declared sequence — identity check, per-connection pragma rows (`synchronous=NORMAL`, `journal_size_limit`, `temp_store=MEMORY`, `cache_size`), defensive hardening, schema-resident capability registration, and the epoch read — idempotent so bootstrap, crash-recovery reopen, and steady-state open are one fold; any transaction that may write begins IMMEDIATE so a deferred read attempting its first write never burns the busy budget on a stale-snapshot retry; `PRAGMA data_version` is the polling-free cross-process change probe.
- Receipt: an open rides `store.embedded.open` carrying the ritual fact count.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new pragma is one ritual row; a new capability is one registration row; zero new surface — a second embedded relational engine (libSQL, PGlite, LiteDB, hctree), a per-process bootstrap branch, or a nonzero `busy_timeout` is the deleted form because the engine sweep is closed and the provider already retries BUSY/LOCKED at managed quanta.
- Boundary: the embedded SQLite floor is the single-process embedded store beneath the server tier — the one engine sweep is CLOSED (libSQL, PGlite, LiteDB, RavenDB.Embedded, Realm, hctree, embedded-pg, EF InMemory all rejected) so a new engine row is the named defect; the WAL `-wal`/`-shm` sidecar set is the unit of copy/replace/delete (a main file separated from its sidecars is silent page-level corruption); STRICT tables are the typed admission gate and `RETURNING` supersedes write-then-read identity round trips; the ritual is the one open path so a per-process bootstrap branch is the deleted form; the embedded floor and the PostgreSQL server tier are two engines on the one store-profile axis, the profile selecting one by deployment, never a third.

```csharp signature
public readonly record struct RitualFact(string Row, long Applied);
public sealed record EmbeddedRitual(long Identity, long CompiledEpoch, Seq<(string Row, string Sql)> ConnectionRows) {
    public static readonly EmbeddedRitual Canonical = new(Identity: 0x5241_5731, CompiledEpoch: 1,
        ConnectionRows: [("<throughput>", "PRAGMA synchronous=NORMAL"), ("<wal-bound>", "PRAGMA journal_size_limit=8388608"), ("<spill>", "PRAGMA temp_store=MEMORY"), ("<budget>", "PRAGMA cache_size=-32768")]);
}

public static class EmbeddedStore {
    public static SqliteConnection Dialed(string path) => new(new SqliteConnectionStringBuilder {
        DataSource = path, Mode = SqliteOpenMode.ReadWriteCreate, Pooling = true, ForeignKeys = true,
    }.ConnectionString);

    public static Fin<Seq<RitualFact>> Open(SqliteConnection store, EmbeddedRitual ritual) {
        ArgumentNullException.ThrowIfNull(store);
        try {
            store.Open();
            var identity = Scalar(store, "PRAGMA application_id");
            if (identity != ritual.Identity && identity != 0L) { store.Dispose(); return Fin.Fail<Seq<RitualFact>>(Error.New(7701, $"<foreign-store:{identity:x8}>")); }
            var facts = ritual.ConnectionRows.Map(row => new RitualFact(row.Row, Execute(store, row.Sql)));
            using var gate = store.BeginTransaction(IsolationLevel.Serializable, deferred: false);
            var held = Scalar(store, "PRAGMA user_version");
            if (held > ritual.CompiledEpoch) { store.Dispose(); return Fin.Fail<Seq<RitualFact>>(Error.New(7702, $"<epoch-ahead:{held}>")); }
            if (held < ritual.CompiledEpoch) { _ = Execute(store, $"PRAGMA application_id={ritual.Identity}"); _ = Execute(store, $"PRAGMA user_version={ritual.CompiledEpoch}"); }
            gate.Commit();
            return Fin.Succ(facts.Add(new RitualFact("<epoch>", long.Max(held, ritual.CompiledEpoch))));
        }
        catch (Exception ex) {
            store.Dispose();
            return Fin.Fail<Seq<RitualFact>>(Error.New(7703, $"<embedded-open:{ex.Message}>"));
        }
    }

    static long Execute(SqliteConnection store, string sql) { using var c = store.CreateCommand(); c.CommandText = sql; return c.ExecuteNonQuery(); }
    static long Scalar(SqliteConnection store, string sql) { using var c = store.CreateCommand(); c.CommandText = sql; return Convert.ToInt64(c.ExecuteScalar(), CultureInfo.InvariantCulture); }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | open ritual         | one idempotent fold                    | bootstrap/recovery/steady-state are one path             |
|  [02]   | write transaction   | IMMEDIATE begin                        | a deferred-then-write burns the busy budget               |
|  [03]   | engine sweep        | closed (PostgreSQL + SQLite only)      | a new embedded engine row is the named defect             |
|  [04]   | sidecar unit        | `-wal`/`-shm` set                      | a main file without its sidecars is silent corruption     |
