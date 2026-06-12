# [POSTGRES]

PostgreSQL is one declared store surface per database. One `NpgsqlDataSourceBuilder` fold owns every per-database concern — type mapping, JSON policy, credentials, session state, pool and prepare budgets, tracing shape — and yields the one process-lifetime `NpgsqlDataSource` every later surface hangs off. SQL capability is consumed through a two-door routing law — provider-translated LINQ or interpolated typed SQL feeding one composable rail, no third lane — and every statement failure folds once over SQLSTATE class into a closed typed fault family. Extension capability is declared rows whose presence one verification-only admission fold proves: the process never alters its environment. Vector, geo, full-text, temporal, and relational predicates compose in one typed query rail gated by those rows; bulk admission is binary COPY into staging reconciled by one receipted MERGE; the change contract is ephemeral NOTIFY wake over a durable replication slot whose decode folds into the settled op-log shape. Growth lands as rows: a new wire type is a mapping row, a new extension a capability row, a new retrieval axis a lane row, a new maintenance duty one scheduler row.

## [1]-[POSTGRES_CHOOSER]

This table routes a store concern to its owning surface; the most specific row wins.

| [INDEX] | [CONCERN]             | [OWNER]                               | [REJECTED_FORM]              |
| :-----: | :-------------------- | :------------------------------------ | :--------------------------- |
|   [1]   | store configuration   | one data-source builder fold          | raw-string connections       |
|   [2]   | SQL construct routing | translated-LINQ or typed-SQL two-door | string-concatenated SQL      |
|   [3]   | statement failure     | SQLSTATE-class disposition fold       | per-statement catch arms     |
|   [4]   | extension capability  | declared row + admission verdict      | runtime `CREATE EXTENSION`   |
|   [5]   | multi-axis retrieval  | lane-composed typed query rail        | per-lane sibling queries     |
|   [6]   | bulk writes           | binary COPY + MERGE reconcile         | batched INSERT loops         |
|   [7]   | change consumption    | slot decode + NOTIFY wake             | payload-as-data notification |
|   [8]   | schema deployment     | compiled bundle + epoch publication   | migrate-at-startup           |
|   [9]   | store maintenance     | suite-scheduler rows                  | in-database cron             |
|  [10]   | driver telemetry      | store-profile tracing rows            | per-call-site spans          |

## [2]-[STORE_PROFILE]

[PROFILE_FOLD]:
- Law: `NpgsqlDataSourceBuilder` is the single configuration owner below the store profile — mapping (`MapEnum`, `MapComposite`), JSON (`EnableDynamicJson`, `ConfigureJsonOptions`), credentials, logging, type loading (`ConfigureTypeLoading`), tracing — and `Build()` yields the one process-lifetime `NpgsqlDataSource` per database; a connection from a raw string after the source exists forks pool identity and forgets every policy row, and `EnableParameterLogging`, `IncludeErrorDetail`, and `IncludeFailedBatchedCommand` default closed — opening any is a redaction-policy decision, never a debugging convenience.
- Law: credential rotation is builder policy — `UsePeriodicPasswordProvider` with distinct success and failure cadences — never connection-string ceremony; `ClearPool`/`ClearAllPools` are the cutover levers and `UnprepareAll` pairs with them after wholesale statement-shape change, because prepared state survives pooled reopen.
- Law: session state splits by mechanism — the connection-string `Options` parameter carries declarative floors applied at connection birth, `UsePhysicalConnectionInitializer` owns programmatic per-session state, and any initializer-established state disqualifies `NoResetOnClose`.
- Law: multi-host is one `BuildMultiHost()` source with `TargetSessionAttributes` per acquisition — read/write splitting is acquisition-time policy on one source, never two configured sources — and the mapped profile consumes the same source through its data-source seam, so ADO depth and LINQ querying share one pool.

[BINDING_AND_ROUND_TRIPS]:
- Law: the driver has one exclusivity concept — transactions, COPY, LISTEN, and replication bind a connector for a window, multiplexing serves the unbound query swarm — so the connection budget is a closed static table: listeners plus streams plus transaction ceiling plus COPY width, remainder multiplexed.
- Law: the three round-trip levers are orthogonal — batch what is sequential (`CreateBatch`, one round trip), multiplex what is concurrent, prepare what repeats (`MaxAutoPrepare` LRU, explicit `Prepare()` exempt from eviction) — and source-level `CreateCommand`/`CreateBatch` execute connection-less on the multiplexed fast path.
- Law: `EnableErrorBarriers` selects batch fault granularity — off for all-or-nothing transactional batches, on for independent-fact batches whose per-command outcomes fold into receipts; without it the first failure poisons the remainder.
- Law: the store owns its strategy — transient classification (`IsTransient`, SQLSTATE rows on `EnableRetryOnFailure`) never reaches an outbound-hop owner; the data source is the unit a strategy wraps, never the thing that retries itself.

[TELEMETRY_ROWS]:
- Law: wiring is two composition-root rows — `AddNpgsql()` on the tracer, `AddNpgsqlInstrumentation()` on the meter — and depth is store-profile data on `ConfigureTracing`: command, batch, and COPY filters drop noise at the source, span-name providers collapse per-statement cardinality, enrichment callbacks attach domain tags; a per-call-site tracing decision is the rejected form, and governance — sampling, export, redaction — arrives settled.
- Law: `EnableFirstResponseEvent` splits server execution from result drain inside one span; `EnablePhysicalOpenTracing` makes pool starvation visible as open-span bursts instead of latency-tail inference.
- Exemption: the profile fold's builder mutation and initializer bodies are the platform-forced statement seam.

```csharp conceptual
public enum Band { Low, High }
public sealed record Slot(string Key, int Rank);

public static class StoreProfile {
    public static NpgsqlDataSource Open(string channel, Func<NpgsqlConnectionStringBuilder, CancellationToken, ValueTask<string>> secret, ILoggerFactory logs) {
        var profile = new NpgsqlDataSourceBuilder(channel);
        _ = profile.MapEnum<Band>("band").MapComposite<Slot>("slot").EnableDynamicJson().UseLoggerFactory(logs);
        _ = profile.UsePeriodicPasswordProvider(secret, successRefreshInterval: TimeSpan.FromMinutes(10), failureRefreshInterval: TimeSpan.FromSeconds(15));
        _ = profile.UsePhysicalConnectionInitializer(
            static session => { using var floor = session.CreateCommand(); floor.CommandText = "SET statement_timeout = 30000"; _ = floor.ExecuteNonQuery(); },
            static async session => { await using var floor = session.CreateCommand(); floor.CommandText = "SET statement_timeout = 30000"; _ = await floor.ExecuteNonQueryAsync().ConfigureAwait(false); });
        _ = profile.ConfigureTracing(static tracing => tracing
            .ConfigureCommandFilter(static command => !command.CommandText.StartsWith("LISTEN", StringComparison.Ordinal))
            .ConfigureCommandSpanNameProvider(static command => command.CommandText is ['M', 'E', 'R', 'G', 'E', ..] ? "<span-reconcile>" : "<span-ask>")
            .ConfigureCommandEnrichmentCallback(static (span, command) => span?.SetTag("store.lane", "<lane-a>"))
            .EnableFirstResponseEvent()
            .EnablePhysicalOpenTracing());
        return profile.Build();
    }

    public static TracerProviderBuilder Traces(TracerProviderBuilder root) => root.AddNpgsql();
    public static MeterProviderBuilder Meters(MeterProviderBuilder root) => root.AddNpgsqlInstrumentation();
}
```

## [3]-[SQL_LAW]

[ENGINE_GATE_AND_IDENTITY]:
- Law: `SetPostgresVersion` is mandatory store-profile policy — undeclared, the provider assumes a trailing default and silently withholds newer translations; a feature is admitted only when the engine floor carries it, the declared gate exposes it, and a typed spelling exists, and a feature failing the third test routes to typed SQL, never to absence.
- Law: `Guid.CreateVersion7()` translating to `uuidv7()` makes one timestamp-ordered key species serve client- and server-generated sites — generation site is a policy value, never two key shapes — and bulk lanes are client-keyed by construction because COPY carries no RETURNING channel; `uuid_extract_timestamp` makes a v7 key a free coarse creation-time axis, and composite `(low-cardinality discriminant, v7 key)` indexes stay append-local while skip scan serves their key-only lookups, so the bare-key second index is deletable redundancy and a query newly using a composite index is the feature working, not a plan regression.

[SCHEMA_DERIVATION]:
- Law: generated columns are virtual by default and `stored: true` is forced exactly by indexed-or-replicated — that implication is the whole decision table; a hot predicate on a virtual column is the promotion signal to stored-plus-index, and absent `stored:` in a migration diff is semantics, not noise.
- Law: index design is model data — `HasMethod`, `HasOperators`, `IncludeProperties` (projected columns leave the key), `AreNullsDistinct(false)` (single-null uniqueness without the partial-index workaround), `HasNullSortOrder`, `UseCollation`, `HasStorageParameter`, per-column `UseCompressionMethod` — never deploy-time hand DDL.
- Law: temporal keys are engine constraints — `PRIMARY KEY (id, period WITHOUT OVERLAPS)` with `PERIOD` foreign keys deletes the read-check-write overlap race; the trailing range rides a GiST shape whose scalar parts require the `btree_gist` extension, so a temporal constraint is also a capability row, and bespoke exclusion takes `EXCLUDE USING gist`, the non-referencable general form.
- Law: jsonb is canonical-by-construction and `json` survives only where byte identity is the contract; the GIN choice is a query-vocabulary declaration — `jsonb_ops` for mixed predicates, `jsonb_path_ops` containment-only, expression GIN for one hot path — and spelling extract-then-compare (`->>'k' =`) instead of containment forfeits the index, because predicate spelling is index policy.

[MUTATION_GRAMMAR]:
- Law: `RETURNING old.*, new.*` yields the before/after transition pair in one statement — read-before-write round trips are structurally obsolete — and data-modifying CTEs chain the receipt into further relational algebra in the same statement.
- Law: conflict routing is two rows — `INSERT ... ON CONFLICT DO UPDATE` for row-at-a-time idempotent writes (speculative locks, arbiter-required including partial-unique arbiters, `EXCLUDED` proposed row, cannot raise unique violations) and `MERGE ... RETURNING merge_action()` for set reconciliation against any row source with per-row verdicts and `WHEN NOT MATCHED BY SOURCE` arms; MERGE can raise unique violations under concurrency, so retry policy lives on the MERGE rail alone.

[TWO_DOOR_ROUTING]:
- Law: a construct lives in LINQ exactly when the provider owns its translation; everything else is interpolated typed SQL (`SqlQuery<T>`, `ExecuteSql`) whose holes become parameters mechanically — no third lane exists, and routing a translated construct to raw SQL forfeits compile-time shape checking for zero capability gain.
- Law: the composition hinge opens only at the root — a typed-SQL query returning a mapped type re-enters LINQ as a subquery, and a typed-SQL construct needed in a non-leaf position routes the whole query typed-SQL; misrouting toward LINQ fails loudly at query compilation, never by silent client evaluation.
- Law: parameter-collection membership translates to `= ANY(@array)` — one parameter for any element count, so statement text and cached plans survive collection-size change, which is also what keeps auto-prepared generic plans valid.
- Law: identifiers parameterize in neither lane — dynamic identifiers ride quoted interpolation at one declared seam over a closed vocabulary — and DDL routes through neither door: schema statements live in migration artifacts, and the deploy credential split makes a runtime DDL string fail on privilege, not on review.

| [INDEX] | [CONSTRUCT_FAMILY]                                    | [DOOR]    |
| :-----: | :---------------------------------------------------- | :-------- |
|   [1]   | array, range, network, KNN, temporal aggregates       | LINQ      |
|   [2]   | full-text, trigram, phonetic, `ILike`                 | LINQ      |
|   [3]   | JSON traversal, containment, `jsonb_set` updates      | LINQ      |
|   [4]   | `ArrayAgg`/`JsonbAgg` grouped projections, ltree      | LINQ      |
|   [5]   | `MERGE`, `ON CONFLICT`, `RETURNING old/new`, COPY     | typed SQL |
|   [6]   | `JSON_TABLE`, `LATERAL`, `DISTINCT ON`, recursive CTE | typed SQL |
|   [7]   | translated construct re-spelled as raw SQL            | rejected  |
|   [8]   | DDL text in either query lane                         | rejected  |

## [4]-[FAULT_DISPOSITION]

[SQLSTATE_FOLD]:
- Law: fault handling is one total dispatch over SQLSTATE class for every statement shape — per-statement catch arms are the rejected form — spelled as patterns over `PostgresErrorCodes` constants and the class prefix: conflicts (unique, exclusion) mint domain faults carrying constraint identity and are never retried; admission defects (foreign-key, check, not-null) carry column and constraint evidence retry cannot fix; serialization failure and deadlock are the only classes a store-level strategy may absorb silently; the 22-prefixed data-exception class is an admission defect that escaped the boundary, its production appearance evidence the validation seam has a hole.
- Law: the two-tier exception law splits transport from server — `NpgsqlException` carries `IsTransient`, `PostgresException` carries the structured error (`SqlState`, `ConstraintName`, `ColumnName`, `TableName`, `Detail`, `Hint`) — so conversion is lossless with zero message parsing, and `Detail`/`Hint` can carry row data: classification-gated evidence populated only under `IncludeErrorDetail`; escalation reads the typed fault, never message text.
- Exemption: the capture seam's catch arms are the platform-forced statement seam.

```csharp conceptual
[Union]
public abstract partial record StoreFault : Expected {
    private StoreFault(string detail, int code) : base(detail, code, None) { }
    public sealed record Conflict(string Constraint, string Detail) : StoreFault($"<conflict:{Constraint}>:{Detail}", 7711);
    public sealed record Defect(string Constraint, string Column, string Detail) : StoreFault($"<defect:{Constraint}:{Column}>:{Detail}", 7712);
    public sealed record Contention(string SqlState) : StoreFault($"<contention:{SqlState}>", 7713);
    public sealed record Escaped(string SqlState, string Detail) : StoreFault($"<escaped:{SqlState}>:{Detail}", 7714);
}

public static class StoreSeam {
    public static async Task<Fin<T>> Ask<T>(NpgsqlDataSource store, Func<NpgsqlDataSource, CancellationToken, Task<T>> statement, CancellationToken token) {
        ArgumentNullException.ThrowIfNull(statement);
        try { return Fin.Succ(await statement(store, token).ConfigureAwait(false)); }
        catch (PostgresException server) { return Fin.Fail<T>(Fold(server)); }
        catch (NpgsqlException driver) { return Fin.Fail<T>(Error.New(driver.IsTransient ? 7715 : 7716, $"<transport:{driver.Message}>")); }
    }

    public static Error Fold(PostgresException server) {
        ArgumentNullException.ThrowIfNull(server);
        return server.SqlState switch {
            PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ExclusionViolation => new StoreFault.Conflict(server.ConstraintName ?? "<unnamed>", server.MessageText),
            PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected => new StoreFault.Contention(server.SqlState),
            PostgresErrorCodes.ForeignKeyViolation or PostgresErrorCodes.CheckViolation or PostgresErrorCodes.NotNullViolation =>
                new StoreFault.Defect(server.ConstraintName ?? "<unnamed>", server.ColumnName ?? "<unnamed>", server.MessageText),
            ['2', '2', ..] => new StoreFault.Escaped(server.SqlState, server.MessageText),
            _ => Error.New(7710, $"<sqlstate:{server.SqlState}>:{server.MessageText}"),
        };
    }
}
```

## [5]-[QUERY_RAIL]

[LANE_GRAMMAR]:
- Law: an extension lane is at most three declarations — a model `HasPostgresExtension` row, a wire admission, a translated vocabulary — and a new lane is three rows, zero architecture; codec-bearing lanes admit dually (`UseVector`, `UseNetTopologySuite` beside the wire admission), neither half standing alone — model-only fails at materialization, wire-only fails at translation — and lane sub-capability floors are row data: presence below the extension generation that carries a setting is absence for that sub-capability, so queries dispatch on the folded set, never probe.
- Law: one column family takes one predicate vocabulary — ordered scalars array, documents jsonb, paths ltree, string fuzz trigram — and minority predicates ride expression indexes on the same column, never a second column in a second vocabulary; read-dominant hierarchies take ltree's GiST ancestry, write-heavy graphs keep recursive CTEs over adjacency.
- Law: a query lane is a declared transform row — capability symbol plus an `IQueryable` arrow — and the rail folds admitted lanes over one root from either door, LINQ base or typed-SQL hinge, so a new retrieval axis is one row, the typed capability verdict decides composition, and zero call-site branches exist.

[VECTOR_LAW]:
- Law: storage is always full-precision `vector`; every cheaper representation is an expression index — `binary_quantize(col)::bit(d)` Hamming prefilter re-ranked by the true operator, half-precision casts — so index species, representation, and scan policy are three orthogonal rows, and the recall-cost frontier is walked by changing rows, never schema.
- Law: a selective filter over an ANN index silently truncates results without iterative scan — `hnsw.iterative_scan` with `max_scan_tuples` budgets is the fix, delivered as transaction-scoped settings so lanes with different scan policies share one pool — and `ef_search` floors the satisfiable `LIMIT`, a configuration defect detectable statically from declared rows.
- Law: every ANN tuning claim is (recall@k versus exact, latency) against the exact-scan baseline; cosine lanes admit only `l2_normalize`d nonzero vectors and then prefer the cheaper negated-inner-product operator; bulk lanes load-then-index, because per-row graph insertion is the slowest write path HNSW has.

[ORDERING_AND_TEXT]:
- Law: every distance, rank, and similarity member is index-served only as the bare outermost `ORDER BY` — wrapped in arithmetic or post-filtered it degrades to exact evaluation — one rule across vector, geo, full-text, and trigram lanes; opclass-operator agreement is the first sequential-scan diagnostic.
- Law: geodesic truth stores `geography` while hot planar predicates ride a stored generated planar projection — the generated-column law composing into the lane — and SRID is a per-factory type-level invariant with `Transform` the only sanctioned reprojection seam.
- Law: the full-text spine is three declarations — stored generated tsvector, GIN row, `Matches` predicates — with `WebSearchToTsQuery` the only parser admitted to user text because the full parser throws on malformed input; hybrid retrieval fuses lexical and semantic top-k as two index-served arms in one statement, never two client-fetched lists merged after paying two round trips.

```csharp conceptual
public sealed class Fact {
    public Guid Key { get; init; } = Guid.CreateVersion7();
    public string Title { get; init; } = "";
    public string Body { get; init; } = "";
    public int Grade { get; init; }
    public Vector Embedding { get; init; } = null!;
    public Point Site { get; init; } = null!;
    public NpgsqlRange<Instant> Window { get; init; }
    public NpgsqlTsVector Lexemes { get; init; } = null!;
}

public sealed class FactStore(DbContextOptions<FactStore> options) : DbContext(options) {
    public static FactStore Open(NpgsqlDataSource store) => new(
        new DbContextOptionsBuilder<FactStore>()
            .UseNpgsql(store, static npgsql => npgsql.SetPostgresVersion(18, 0).UseNodaTime().UseNetTopologySuite().UseVector()
                .EnableRetryOnFailure(maxRetryCount: 4, maxRetryDelay: TimeSpan.FromSeconds(2), errorCodesToAdd: null))
            .UseSnakeCaseNamingConvention()
            .Options);

    public IQueryable<Fact> PeakPerGrade(Instant floor) =>
        Set<Fact>().FromSql($"""SELECT DISTINCT ON (grade) * FROM fact WHERE lower("window") >= {floor} ORDER BY grade, key DESC""");

    protected override void OnModelCreating(ModelBuilder model) {
        ArgumentNullException.ThrowIfNull(model);
        _ = model.HasPostgresExtension("vector").HasPostgresExtension("postgis");
        _ = model.Entity<Fact>(static fact => {
            _ = fact.Property(static f => f.Embedding).HasColumnType("vector(768)");
            _ = fact.Property(static f => f.Site).HasColumnType("geography (point, 4326)");
            _ = fact.HasGeneratedTsVectorColumn(static f => f.Lexemes, "english", static f => new { f.Title, f.Body });
            _ = fact.HasIndex(static f => f.Embedding).HasMethod("hnsw").HasOperators("vector_cosine_ops").HasStorageParameter("m", 16).HasStorageParameter("ef_construction", 64);
            _ = fact.HasIndex(static f => f.Lexemes).HasMethod("gin");
            _ = fact.HasIndex(static f => f.Site).HasMethod("gist");
        });
    }
}

public sealed record ProbeShape(Vector Embedding, Point Anchor, double Meters, string Terms, Instant At);
public sealed record QueryLane(string Capability, Func<IQueryable<Fact>, ProbeShape, IQueryable<Fact>> Narrow) {
    public static readonly QueryLane Lexical = new("<lane-core>", static (facts, probe) =>
        facts.Where(fact => fact.Lexemes.Matches(EF.Functions.WebSearchToTsQuery("english", probe.Terms))));
    public static readonly QueryLane Near = new("postgis", static (facts, probe) =>
        facts.Where(fact => fact.Site.IsWithinDistance(probe.Anchor, probe.Meters)));
    public static readonly QueryLane Temporal = new("<lane-core>", static (facts, probe) =>
        facts.Where(fact => fact.Window.Contains(probe.At)));
    public static readonly QueryLane Semantic = new("vector", static (facts, probe) =>
        facts.OrderBy(fact => fact.Embedding.CosineDistance(probe.Embedding)));
}

public static class QueryRail {
    public static IQueryable<Fact> Compose(IQueryable<Fact> root, ProbeShape probe, CapabilityVerdict verdict, Seq<QueryLane> lanes) {
        ArgumentNullException.ThrowIfNull(verdict);
        return lanes.Filter(lane => verdict.Lanes.Exists(held => held == lane.Capability))
            .Fold(root, (facts, lane) => lane.Narrow(facts, probe))
            .Take(32);
    }
}
```

## [6]-[BULK_LANES]

[COPY_LAW]:
- Law: the importer's commit edge is inverted — `Complete()` commits, disposal without it cancels the COPY and discards every buffered row — so the success branch ends in `Complete`, exception safety means data is discarded rather than half-written, and the retry unit is always the whole COPY, which is what makes the lane composable with idempotent staging.
- Law: binary COPY has no server-side coercion — every ambiguous CLR mapping writes the discriminated overload (`NpgsqlDbType` or data-type name), and a wire-type mismatch surfaces mid-stream, never at `Complete`; registered codecs serve query, batch, and COPY identically, so rich domain rows bulk-admit with zero flattening and a staging-table-of-strings is a rejected form born of text-COPY habits.
- Law: any write set large enough to batch twice is large enough to COPY — the bound connector out-throughputs batched INSERT by an order of magnitude — and the importer carries its own `Timeout`; server-side filters live in the COPY SQL, defect budgets in the bulk-admission options (`ON_ERROR ignore`, `REJECT_LIMIT n`), and `BeginRawBinaryCopy` is the zero-materialization table-to-table pipe.
- Law: staging-then-MERGE is the highest-throughput receipted upsert the engine offers — COPY into unlogged staging on a bound connector, then one MERGE reconciles into the target with per-row verdicts — rows staged, rows inserted, rows updated, all receipted with zero application-side row iteration, and the receipt proves conservation: staged equals inserted plus updated, breach failing typed.
- Exemption: the COPY kernel — row loop, discard arm, verdict drain — is the platform-forced statement seam.

```csharp conceptual
public sealed record StagedRow(Guid Key, Band Band, Slot Slot);
public sealed record ReconcileReceipt(ulong Staged, int Inserted, int Updated) { public bool Conserves => Staged == (ulong)(Inserted + Updated); }

public static class BulkLane {
    public static async Task<Fin<ReconcileReceipt>> Reconcile(NpgsqlDataSource store, Seq<StagedRow> rows, CancellationToken token) {
        ArgumentNullException.ThrowIfNull(store);
        await using var connection = await store.OpenConnectionAsync(token).ConfigureAwait(false);
        var importer = await connection.BeginBinaryImportAsync("COPY staging (key, band, slot) FROM STDIN (FORMAT BINARY)", token).ConfigureAwait(false);
        ulong staged;
        try {
            foreach (var row in rows) {
                await importer.StartRowAsync(token).ConfigureAwait(false);
                await importer.WriteAsync(row.Key, NpgsqlDbType.Uuid, token).ConfigureAwait(false);
                await importer.WriteAsync(row.Band, "band", token).ConfigureAwait(false);
                await importer.WriteAsync(row.Slot, "slot", token).ConfigureAwait(false);
            }
            staged = await importer.CompleteAsync(token).ConfigureAwait(false);
        }
        catch (Exception discarded) when (discarded is not OutOfMemoryException) {
            await importer.DisposeAsync().ConfigureAwait(false);
            return Fin.Fail<ReconcileReceipt>(Error.New(7731, $"<copy-discarded:{discarded.Message}>"));
        }
        await importer.DisposeAsync().ConfigureAwait(false);
        await using var reconcile = connection.CreateCommand();
        reconcile.CommandText = """
            MERGE INTO target AS held USING staging AS staged ON held.key = staged.key
            WHEN MATCHED THEN UPDATE SET band = staged.band, slot = staged.slot
            WHEN NOT MATCHED THEN INSERT (key, band, slot) VALUES (staged.key, staged.band, staged.slot)
            RETURNING merge_action()
            """;
        await using var verdicts = await reconcile.ExecuteReaderAsync(token).ConfigureAwait(false);
        var (inserted, updated) = (0, 0);
        while (await verdicts.ReadAsync(token).ConfigureAwait(false)) { (inserted, updated) = verdicts.GetString(0) is "INSERT" ? (inserted + 1, updated) : (inserted, updated + 1); }
        var receipt = new ReconcileReceipt(staged, inserted, updated);
        return receipt.Conserves ? Fin.Succ(receipt) : Fin.Fail<ReconcileReceipt>(Error.New(7732, $"<unconserved:{staged}:{inserted + updated}>"));
    }
}
```

## [7]-[CHANGE_CONTRACT]

[WAKE_ROW]:
- Law: NOTIFY is transactional ephemeral wake — delivered on commit, collapsed within a transaction, payload-capped, lost on every reconnect window — so payloads carry a cursor hint and the listener re-reads authoritative state through the query rail; the canonical listener is one dedicated bound connection per process running `LISTEN` per channel, demultiplexing on `Channel`, and looping the connection's `WaitAsync` timeout overload, whose elapsed-window `false` is the liveness heartbeat and the watermark catch-up point.
- Law: the `Notification` event without the wait loop is a latent bug — notifications surface only during protocol interaction — and the `Notice` event is informational server chatter, never control flow.
- Law: the fused consumer law — NOTIFY is a latency optimization over the slot's polling cadence, and correctness derives solely from the slot's LSN watermark; a consumer correct only when NOTIFY arrives is broken by definition, a consumer ignoring NOTIFY is merely slower.

[DECODE_FOLD]:
- Law: the slot is the durable replay channel — WAL retained until acknowledged, complete, ordered — and the contract is at-least-once: apply, then acknowledge via `SetReplicationStatus` with only what is durably applied; the applied-LSN watermark is the idempotency key and redelivery after crash is the normal path, while a lagging acknowledger is server-disk liability the verification fold gauges.
- Law: which before-image arrives (none, key-only, full) is the table's replica identity — a DDL fact, never a client option — and `RelationMessage` re-arrives on schema change, decoding as an in-stream schema fact while the driver resolves `Relation` references on data messages; tuples are consumed fully in order because the read buffer is reused, unchanged-TOAST columns decode as a distinct third state, never null, and truncate is one relation-set message decoding to a purge fact, never per-row retractions.
- Law: streamed in-progress transactions stage keyed by xid and release only on stream-commit — abort drops the stage — and commit order, not arrival order, is the op-log order; `IdentifySystem` pins system id and timeline, a watermark replayed against a different coordinate is re-bootstrap rather than resume, and bootstrap rides slot-creation snapshot mode `Use` for zero-gap, zero-overlap entry; with `Messages` enabled, application-emitted decoding messages arrive in-stream as transactional markers — an ordered, WAL-durable side channel for schema epochs and batch boundaries riding the same cursor as the data it annotates.
- Boundary: messages decode into the op-log shape whose law durability owns — this lane owns exactly the decode: message family to fact rows, before-image evidence, staging, watermark acknowledgement.
- Exemption: the pump's message switch and tuple drains are the platform-forced statement seam.

```csharp conceptual
[Union<string, Missing, Carried>(T1Name = "Text", T2Name = "Absent", T3Name = "Unchanged", T2IsStateless = true, T3IsStateless = true)]
public readonly partial struct Cell;
public readonly record struct Missing;
public readonly record struct Carried;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChangeFact {
    private ChangeFact() { }
    public sealed record Upsert(string Relation, Seq<Cell> Row, Seq<Cell> Image) : ChangeFact;
    public sealed record Retract(string Relation, Seq<Cell> Image) : ChangeFact;
    public sealed record Purge(Seq<string> Relations) : ChangeFact;
    public sealed record Marker(string Prefix) : ChangeFact;
}

public sealed record CommitBatch(NpgsqlLogSequenceNumber Cursor, Seq<ChangeFact> Facts);
public static class ChangeDecode {
    public static async Task Pump(LogicalReplicationConnection link, PgOutputReplicationSlot slot, Func<CommitBatch, Task> apply, CancellationToken token) {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentNullException.ThrowIfNull(apply);
        var (staged, open) = (HashMap<uint, Seq<ChangeFact>>(), Seq<ChangeFact>());
        var options = new PgOutputReplicationOptions(["<publication-a>"], PgOutputProtocolVersion.V4, binary: true, streamingMode: PgOutputStreamingMode.Parallel, messages: true);
        await foreach (var message in link.StartReplication(slot, options, token).ConfigureAwait(false)) {
            switch (message) {
                case RelationMessage relation:
                    (staged, open) = Filed(staged, open, relation.TransactionXid, new ChangeFact.Marker($"<relation:{relation.Namespace}.{relation.RelationName}:{relation.ReplicaIdentity}>")); break;
                case InsertMessage insert:
                    (staged, open) = Filed(staged, open, insert.TransactionXid, new ChangeFact.Upsert(insert.Relation.RelationName, await Drained(insert.NewRow, token).ConfigureAwait(false), [])); break;
                case UpdateMessage update:
                    (staged, open) = Filed(staged, open, update.TransactionXid, new ChangeFact.Upsert(update.Relation.RelationName,
                        Image: update switch { FullUpdateMessage full => await Drained(full.OldRow, token).ConfigureAwait(false), IndexUpdateMessage keyed => await Drained(keyed.Key, token).ConfigureAwait(false), _ => [] },
                        Row: await Drained(update.NewRow, token).ConfigureAwait(false))); break;
                case KeyDeleteMessage sparse:
                    (staged, open) = Filed(staged, open, sparse.TransactionXid, new ChangeFact.Retract(sparse.Relation.RelationName, await Drained(sparse.Key, token).ConfigureAwait(false))); break;
                case FullDeleteMessage dense:
                    (staged, open) = Filed(staged, open, dense.TransactionXid, new ChangeFact.Retract(dense.Relation.RelationName, await Drained(dense.OldRow, token).ConfigureAwait(false))); break;
                case TruncateMessage truncated:
                    (staged, open) = Filed(staged, open, truncated.TransactionXid, new ChangeFact.Purge(toSeq(truncated.Relations).Map(static relation => relation.RelationName))); break;
                case LogicalDecodingMessage emitted: (staged, open) = Filed(staged, open, emitted.TransactionXid, new ChangeFact.Marker(emitted.Prefix)); break;
                case StreamCommitMessage streamed:
                    await apply(new CommitBatch(streamed.TransactionEndLsn, staged.Find(streamed.TransactionXid).IfNone([]))).ConfigureAwait(false);
                    staged = staged.Remove(streamed.TransactionXid); link.SetReplicationStatus(streamed.TransactionEndLsn); break;
                case StreamAbortMessage aborted: staged = staged.Remove(aborted.TransactionXid); break;
                case CommitMessage committed:
                    await apply(new CommitBatch(committed.TransactionEndLsn, open)).ConfigureAwait(false);
                    open = []; link.SetReplicationStatus(committed.TransactionEndLsn); break;
            }
        }
    }

    static (HashMap<uint, Seq<ChangeFact>> Staged, Seq<ChangeFact> Open) Filed(HashMap<uint, Seq<ChangeFact>> staged, Seq<ChangeFact> open, uint? xid, ChangeFact fact) =>
        xid is uint id ? (staged.AddOrUpdate(id, held => held.Add(fact), Seq(fact)), open) : (staged, open.Add(fact));

    static async ValueTask<Seq<Cell>> Drained(ReplicationTuple row, CancellationToken token) {
        var cells = Seq<Cell>();
        await foreach (var value in row.ConfigureAwait(false)) {
            cells = cells.Add(value switch {
                { IsDBNull: true } => new Missing(),
                { IsUnchangedToastedValue: true } => new Carried(),
                _ => await value.Get<string>(token).ConfigureAwait(false),
            });
        }
        return cells;
    }
}
```

## [8]-[OPS_AND_VERIFICATION]

[DEPLOY_LAW]:
- Law: the deploy artifact triple is compiled bundle, idempotent script, schema epoch — built from one migration set, with the environment's execution-rights rung selecting bundle or script; the process never migrates itself, and the credential split — a migration role owning DDL whose default privileges pre-grant the runtime role — makes runtime DDL fail on privilege, self-enforcing the law at the store.
- Law: every migration classifies before it ships — metadata-only, concurrent-capable, or rewrite — and rewrite-class on hot relations decomposes into expand shadow columns, slot-aware key-range batched backfill, and contract swap; constraints tighten two-phase (declare not-valid, validate later) splitting deploy risk into a retry-trivial half and an interrupt-safe half.
- Law: concurrent index builds are single-purpose non-transactional migrations, individually retryable, with the invalid-index catalog scan as the retry preamble; the migration session sets a bounded lock timeout and the deploy retries on cadence — the lock-queue trap converts into bounded retry latency — with a preflight activity-and-standby read before any lock-taking migration.
- Law: the schema epoch is the ordered migration-id set's hash, stamped at deploy and published atomically with the capability verdict; cross-process compatibility is one manifest comparison over a derived tolerated span, contract-phase migrations gate on no live process spanning the contracted shape, and migrations roll forward only — a bad expand is reversed by a new forward migration, never a down artifact.
- Law: deployment completes when running processes re-resolve wire types — `ReloadTypes` on the owning source or a rolling restart — not when DDL lands; a migration adding enums, composites, or extension types otherwise leaves live processes resolving unknown.

[MAINTENANCE_ROWS]:
- Law: store maintenance is a closed table of suite-scheduler rows — partition lifecycle advance (the partition manager's one idempotent `run_maintenance` call, safe early, late, or doubled, with the pre-creation horizon as the scheduler-downtime budget), concurrent reindex after churn with the invalid-sibling drop preamble, event-keyed analyze consuming bulk-admission receipts, concurrent materialized-view refresh behind its verified unique-index precondition, and row-wise retention — each row receipted into the standard fact stream, because a row completing without evidence is indistinguishable from one that never ran.
- Law: cadence derives from tolerance — each row declares what accumulates while it is not running and the accumulation rate bounds its maximum cadence — and expensive rows condition-gate on measured evidence first, so "checked, not needed" is distinguishable from "never checked".
- Law: the in-database cron extension is deleted with three named reasons — operator-preloaded and unportable, a second cadence owner beside the suite scheduler, job state invisible to suite telemetry — at zero capability cost; engine daemons (autovacuum, background writer, checkpointer) are never scheduled, duplicated, or disabled by suite rows, and a schedule row re-implementing one is lateral creep into engine territory.

[VERIFICATION_FOLD]:
- Law: provisioning is verification-only — one read-only admission fold over catalog and settings reads (engine floor as the `server_version_num` integer, extension presence and generation floors, replication readiness and slot lag, invalid indexes, notification-queue headroom, privilege probes, audit binding, schedule-row preconditions) producing one typed capability verdict the process dispatches on; downstream code never re-probes, and no row touches user relations, so admission cost is data-volume-independent.
- Law: failure rank is row data — required refuses the profile and stays deliberately minimal (engine floor, core schema, epoch), degradable folds the lane out with a receipt so absence surfaces at admission instead of first query, observational records evidence — and settings rows carry their restart class, so a gap names its repair's disruption class.
- Law: the four provisioning rungs own creation — migrations rung one, idempotent seeds rung two, operator runbooks rung three, the environment rung four — and the fold reads all four; the process may emit repair artifacts (reconciliation grants, settings diffs) as typed verification outputs but never executes them, and a periodic re-verify stamps a verification epoch so environment drift becomes an observable event in the fact stream.
- Exemption: the verification read kernel is the platform-forced statement seam.

```csharp conceptual
[SmartEnum<int>]
public sealed partial class FailureRank {
    public static readonly FailureRank Required = new(0);
    public static readonly FailureRank Degradable = new(1);
    public static readonly FailureRank Observational = new(2);
}

public sealed record CapabilityRow(string Lane, string Extension, int Floor, FailureRank Rank);
public sealed record CapabilityVerdict(int Engine, Seq<string> Lanes, Seq<Error> Receipts);

public static class Verification {
    public static async Task<Fin<CapabilityVerdict>> Admit(NpgsqlDataSource store, Seq<CapabilityRow> rows, int engineFloor, CancellationToken token) {
        ArgumentNullException.ThrowIfNull(store);
        await using var batch = store.CreateBatch();
        batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT current_setting('server_version_num')::int"));
        batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT extname, split_part(extversion, '.', 1)::int FROM pg_extension"));
        await using var evidence = await batch.ExecuteReaderAsync(token).ConfigureAwait(false);
        var engine = await evidence.ReadAsync(token).ConfigureAwait(false) ? evidence.GetInt32(0) : 0;
        var (installed, _) = (HashMap<string, int>(), await evidence.NextResultAsync(token).ConfigureAwait(false));
        while (await evidence.ReadAsync(token).ConfigureAwait(false)) { installed = installed.AddOrUpdate(evidence.GetString(0), evidence.GetInt32(1)); }
        return engine < engineFloor
            ? Fin.Fail<CapabilityVerdict>(Error.New(7721, $"<engine-floor:{engine}:{engineFloor}>"))
            : rows.Fold(Fin.Succ(new CapabilityVerdict(engine, [], [])), (verdict, row) => verdict.Bind(held => Folded(held, row, installed)));
    }

    static Fin<CapabilityVerdict> Folded(CapabilityVerdict held, CapabilityRow row, HashMap<string, int> installed) =>
        installed.Find(row.Extension).Map(generation => generation >= row.Floor).IfNone(false)
            ? Fin.Succ(held with { Lanes = held.Lanes.Add(row.Lane) })
            : row.Rank.Switch(
                state: (Held: held, Row: row),
                required: static s => Fin.Fail<CapabilityVerdict>(Error.New(7722, $"<required-absent:{s.Row.Lane}>")),
                degradable: static s => Fin.Succ(s.Held with { Receipts = s.Held.Receipts.Add(Error.New(7723, $"<lane-folded:{s.Row.Lane}>")) }),
                observational: static s => Fin.Succ(s.Held with { Receipts = s.Held.Receipts.Add(Error.New(7724, $"<setting-gap:{s.Row.Lane}>")) }));
}
```
