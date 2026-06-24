# [PERSISTENCE_SERVER_PROVISIONING]

Rasm.Persistence holds two disjoint deploy-time owners on the self-provisioned PostgreSQL 18.4 server tier: `Bm25Predicate`/`SearchProjection`, the `pg_search` `@@@`/`pdb.*` query-projection axis whose query values bind as `$N` parameters and whose column/operator tokens interpolate at one closed-vocabulary seam; and `ClusterConfig`, the `postgresql.conf` GUC-fragment table verified read-only against `pg_settings` into a typed `GucVerdict` that degrades a soft fragment and aborts a hard one. The server-tier raw-SQL provisioning fold (`SchemaDdl.Sql`, the `Hypertable`/`ContinuousAggregate`/`RetentionPolicy`/`ColumnstorePolicy`/`DiskAnn`/`Bm25` index-build cases, `ProvisionSql`, `DiskAnnOptions`) is owned whole at `Schema/ddl#EXTENSION_DDL` and consumed here as settled; the service-deploy migration gate (`SchemaGate.Admit`, `MigrationLaw.Plan`/`Vehicle`/`Script`, `DeployVehicle`, `LockLightStep`, `SchemaEpoch`, `MigrationReceipt`) is owned whole at `Schema/migration` and consumed here as settled. This page mints no migration gate and no `SchemaDdl` case — those owners arrive resolved. The spine is Npgsql, Thinktecture vocabulary, LanguageExt rails, and NodaTime.

Wire posture: this page is host-local — both owners emit or verify server-side artifacts against the deploy-image PostgreSQL, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The preload-gated companions verify through the `Store/profiles#PROVISIONING_ROWS` `PreloadProbe` against the `ClusterConfig.Preload` `shared_preload_libraries` fragment before any provisioning step runs.

## [01]-[INDEX]

- [01]-[BM25_PROJECTION]: the `pg_search` `@@@`/`pdb.*` query-projection axis, `$N`-parameterized so query values never interpolate as literals.
- [02]-[CLUSTER_CONFIG]: `postgresql.conf` GUC fragments verified read-only against `pg_settings` into a degrade-or-abort `GucVerdict`.

## [02]-[BM25_PROJECTION]

- Owner: `Bm25Predicate` is the `pg_search` `@@@`/`pdb.*`/bare-column-operator query-projection algebra and `SearchProjection` its relevance/highlight/facet projection surface — the C# image of `.api/api-pg-search.md` sections `[03]`/`[04]`/`[05]`, consumed at `Query/lanes#SEARCH_LANES` by `HybridRetrieve.LexicalBranch` and folded into the row store by `FromSql`/`SqlQuery`. Every query VALUE is a `$N` bound parameter, never a literal; only the column, operator, relation, and namespace tokens — a closed vocabulary — interpolate, so the `[TWO_DOOR_ROUTING]` law "holes become parameters mechanically" holds at the one place runtime text reaches the engine.
- Cases: bare column operators (`AnyToken` `|||`, `AllToken` `&&&`, `ExactTerm` `===`, `Phrase` `###`); the `@@@` builders (`Parse`, `RangeTerm`, `PhrasePrefix`, `MoreLikeThis`, `Regex`, `All`); the two proximity builders (`Near` `##`, `OrderedNear` `##>` — `.api` `[03]` rows `[06]`/`[07]`); the cast-stack modifiers each over an inner predicate (`Fuzzy ::pdb.fuzzy`, `Boost ::pdb.boost`, `Const ::pdb.const`, `Slop ::pdb.slop`, `Token ::pdb.<tokenizer>` — `.api` `[04]` note); and `Boolean`, the must/should/must-not composite (`pdb.boolean`) that folds child predicates into one Tantivy boolean query. A modifier nests over any inner case and casts stack in cast order, so a new builder, operator, modifier, or tokenizer is one union case, never a sibling method.
- Entry: `public Bm25Sql Sql(int from = 1)` is the total generated `Switch` projecting `Bm25Sql(string Text, Seq<string> Binds)` — the predicate fragment carries `$from…$N` placeholders threaded by the ordinal the arm advances, and `Binds` is the parallel value vector the `FromSql`/`SqlQuery` boundary parameterizes; a nested modifier or `Boolean` child re-bases its inner placeholders at `from + Binds.Count` so one tree yields one contiguous parameter run. `SearchProjection` members project the `key_field`-anchored `pdb.score`/`pdb.snippet`/`pdb.snippets`/`pdb.snippet_positions`/`pdb.agg` SQL as bare strings — they sit in `SELECT`/`ORDER BY` beside the `WHERE` predicate, carry no runtime VALUE, and interpolate their author-fixed tags/sort/agg-spec config at the same seam the column identifier uses, so they never collide with the predicate's ordinal run.
- Auto: the BM25 index answers through `@@@` builder dispatch, the bare `|||`/`&&&`/`===`/`###` column operators, the `##`/`##>` proximity forms, and the `pdb.score`/`pdb.snippet` projections beside the always-present native FTS baseline — a profile without `pg_search` preloaded routes `HybridRetrieve.LexicalBranch(pgSearch: false)` to the `ts_rank` projection inside the same fusion CTE. The `paradedb.*` namespace was removed in `pg_search` 0.24.0 and is never emitted — only the `pdb` schema and the bare operators. The snippet projections default `<b>`/`</b>` tags and `150` `max_num_chars` (`.api` `[05]`); `pdb.agg` carries an Elasticsearch-shaped aggregation document for facet/metric rollups over the matched set. The `key_field` is the `UNIQUE`/primary column the index lists first and every `pdb.score`/`pdb.snippet` anchors on, so the fusion projects identities and re-queries the row store rather than re-materializing payloads.
- Receipt: the `search.bm25.score` fact on `Query/lanes#SEARCH_LANES` reads the live `pdb.score` the index serves and the `search.vector.route` fact the planner's chosen route; this axis projects SQL and owns no provisioning step, so it folds no `StoreFact` of its own.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new `pdb.*` builder, bare operator, proximity form, cast modifier, tokenizer cast, or boolean-composite arm is one `Bm25Predicate` union case; a new relevance/highlight/facet projection is one `SearchProjection` member; zero new surface.
- Boundary: `pg_search` runs in-process inside the PG18 server tier, never linked into managed code, so its AGPL boundary is the DB deployment; this axis is its C# query-projection image only. The index-build DDL (`SchemaDdl.Bm25`, one `CREATE INDEX … USING bm25` per table keyed `key_field`-first) and the extension's preload-gated install are owned at `Schema/ddl#EXTENSION_DDL` and consumed as settled — this section owns the query-time predicate algebra, never the index declaration. The column-type and the embedding/full-text query shape (`FullTextQuery`, `EmbeddingArity`, `HybridRetrieve.Fuse`) are owned at `Query/lanes#SEARCH_LANES`; that lane binds `$terms` into `corpus @@@ pdb.parse($terms)`, so `Bm25Predicate.Sql()` MUST project the same `$N`-parameter shape — a literal-interpolated query value here would fork the binder and reopen the injection surface the parameter run closes. The cast-stacking law is `pg_search`'s own (`'q'::pdb.fuzzy(2)::pdb.boost(2)` applies typo tolerance then a score multiplier); the union models it by nesting, never by a combinatorial method family.

```csharp signature
public readonly record struct Bm25Sql(string Text, Seq<string> Binds) {
    public Bm25Sql Nest(Func<string, string> wrap) => this with { Text = wrap(Text) };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class BoolClause {
    public static readonly BoolClause Must = new("must");
    public static readonly BoolClause Should = new("should");
    public static readonly BoolClause MustNot = new("must_not");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Bm25Predicate {
    private Bm25Predicate() { }

    // Bare column operators: the value binds as `$N`, never a quoted literal.
    public sealed record AnyToken(string Column, string Query) : Bm25Predicate;
    public sealed record AllToken(string Column, string Query) : Bm25Predicate;
    public sealed record ExactTerm(string Column, string Term) : Bm25Predicate;
    public sealed record Phrase(string Column, string PhraseText) : Bm25Predicate;
    // `@@@` builders.
    public sealed record Parse(string Column, string QueryString, bool Lenient, bool ConjunctionMode) : Bm25Predicate;
    public sealed record RangeTerm(string Column, string Value, string Relation, Option<string> RangeType) : Bm25Predicate;
    public sealed record PhrasePrefix(string Column, Seq<string> Tokens, int MaxExpansions) : Bm25Predicate;
    public sealed record MoreLikeThis(string Column, string DocumentId, Seq<string> Fields, int MaxQueryTerms) : Bm25Predicate;
    public sealed record Regex(string Column, string Pattern) : Bm25Predicate;
    public sealed record All(string Column) : Bm25Predicate;
    // Proximity (`a ## n ## b`, `a ##> n ##> b`): both terms bind, the slack is a literal int.
    public sealed record Near(string Column, string Left, string Right, int Slack) : Bm25Predicate;
    public sealed record OrderedNear(string Column, string Left, string Right, int Slack) : Bm25Predicate;
    // Boolean composite: child predicates fold under must/should/must-not into one Tantivy boolean query.
    public sealed record Boolean(string Column, Seq<(BoolClause Clause, Bm25Predicate Term)> Clauses) : Bm25Predicate;
    // Cast-stack modifiers over any inner predicate; casts stack in declaration order.
    public sealed record Fuzzy(Bm25Predicate Inner, int Distance, bool Prefix, bool TranspositionCostOne) : Bm25Predicate;
    public sealed record Boost(Bm25Predicate Inner, double Factor) : Bm25Predicate;
    public sealed record Const(Bm25Predicate Inner, double Score) : Bm25Predicate;
    public sealed record Slop(Bm25Predicate Inner, int Distance) : Bm25Predicate;
    // Tokenizer cast (`'running shoes'::pdb.whitespace`): the cast names an analyzer, the value still binds.
    public sealed record Token(Bm25Predicate Inner, string Tokenizer) : Bm25Predicate;

    // `from` threads the running ordinal so a nested modifier or boolean child re-bases onto a single
    // contiguous `$1…$N` run; the SQL boundary binds `Binds` positionally and no value is ever quoted in.
    public Bm25Sql Sql(int from = 1) =>
        Switch(
            anyToken: a => new Bm25Sql($"{a.Column} ||| ${from}", [a.Query]),
            allToken: a => new Bm25Sql($"{a.Column} &&& ${from}", [a.Query]),
            exactTerm: e => new Bm25Sql($"{e.Column} === ${from}", [e.Term]),
            phrase: p => new Bm25Sql($"{p.Column} ### ${from}", [p.PhraseText]),
            parse: p => new Bm25Sql(
                $"{p.Column} @@@ pdb.parse(${from}, lenient => {Lit(p.Lenient)}, conjunction_mode => {Lit(p.ConjunctionMode)})", [p.QueryString]),
            rangeTerm: r => new Bm25Sql(
                $"{r.Column} @@@ pdb.range_term(${from}, relation => ${from + 1}{r.RangeType.Match(Some: _ => $", range_type => ${from + 2}", None: static () => "")})",
                r.RangeType.Match(Some: t => Seq(r.Value, r.Relation, t), None: () => Seq(r.Value, r.Relation))),
            phrasePrefix: p => new Bm25Sql(
                $"{p.Column} @@@ pdb.phrase_prefix(ARRAY[{Holes(from, p.Tokens.Count)}], max_expansions => {p.MaxExpansions})", p.Tokens),
            moreLikeThis: m => new Bm25Sql(
                $"{m.Column} @@@ pdb.more_like_this(${from}, fields => ARRAY[{Holes(from + 1, m.Fields.Count)}], max_query_terms => {m.MaxQueryTerms})",
                m.DocumentId.Cons(m.Fields)),
            regex: r => new Bm25Sql($"{r.Column} @@@ pdb.regex(${from})", [r.Pattern]),
            all: a => new Bm25Sql($"{a.Column} @@@ pdb.all()", []),
            near: n => new Bm25Sql($"{n.Column} @@@ (${from} ## {n.Slack} ## ${from + 1})", [n.Left, n.Right]),
            orderedNear: n => new Bm25Sql($"{n.Column} @@@ (${from} ##> {n.Slack} ##> ${from + 1})", [n.Left, n.Right]),
            // Each child re-bases onto the running ordinal so the boolean query yields one contiguous parameter run.
            boolean: b => b.Clauses.Fold(new Bm25Sql($"{b.Column} @@@ pdb.boolean(", []), (acc, clause) => {
                    var child = clause.Term.Sql(from + acc.Binds.Count);
                    var lead = acc.Binds.IsEmpty ? "" : ", ";
                    return new Bm25Sql($"{acc.Text}{lead}{clause.Clause.Key} => {child.Text}", acc.Binds + child.Binds);
                }).Nest(static text => $"{text})"),
            fuzzy: f => f.Inner.Sql(from).Nest(inner => $"{inner}::pdb.fuzzy({f.Distance}, {Lit(f.Prefix)}, {Lit(f.TranspositionCostOne)})"),
            boost: b => b.Inner.Sql(from).Nest(inner => $"{inner}::pdb.boost({b.Factor.ToString(CultureInfo.InvariantCulture)})"),
            @const: c => c.Inner.Sql(from).Nest(inner => $"{inner}::pdb.const({c.Score.ToString(CultureInfo.InvariantCulture)})"),
            slop: s => s.Inner.Sql(from).Nest(inner => $"{inner}::pdb.slop({s.Distance})"),
            token: t => t.Inner.Sql(from).Nest(inner => $"{inner}::pdb.{t.Tokenizer}"));

    static string Lit(bool flag) => flag ? "true" : "false";
    static string Holes(int from, int count) => string.Join(", ", Range(from, count).Map(static i => $"${i}"));
}

// Projections sit in SELECT/ORDER BY beside the WHERE predicate's `$N` run, so they carry NO bound
// parameter — the column is an identifier and the tags/sort/agg-spec are author-fixed closed-vocabulary
// config, never runtime query VALUES, so they interpolate at the same seam identifiers do and never
// collide with the predicate's ordinal run.
public static class SearchProjection {
    public static string Score(string keyColumn) => $"pdb.score({keyColumn})";
    public static string Snippet(string column, string startTag = "<b>", string endTag = "</b>", int maxChars = 150) =>
        $"pdb.snippet({column}, start_tag => '{startTag}', end_tag => '{endTag}', max_num_chars => {maxChars})";
    public static string Snippets(string column, int maxChars, int limit, int offset, string sortBy = "score", string startTag = "<b>", string endTag = "</b>") =>
        $"""pdb.snippets({column}, start_tag => '{startTag}', end_tag => '{endTag}', max_num_chars => {maxChars}, "limit" => {limit}, "offset" => {offset}, sort_by => '{sortBy}')""";
    public static string SnippetPositions(string column) => $"pdb.snippet_positions({column})";
    public static string Agg(string esJson) => $"pdb.agg('{esJson}') OVER ()";
}
```

## [03]-[CLUSTER_CONFIG]

- Owner: `ClusterConfig` — the PG18 `postgresql.conf` GUC-fragment table and its read-only `pg_settings` verification fold, shared by `Version/recovery#RECOVERY_LANES` (the `wal_level`/`archive_mode`/`archive_command` replication triad), `Store/encryption#AT_REST` (the TDE GUC), and `Store/profiles#PROVISIONING_ROWS` (the io/checksums/preload baseline). Each fragment is a `GucRow` carrying its primary value, its portable fallback, a `GucRank` (a missing hard fragment aborts, a soft one degrades with a receipt), and the `GucContext` restart class so a gap names its repair-disruption rung. The verification yields one typed `GucVerdict` — matched primaries, fallback-satisfied fragments as degrade receipts, and the abort set — so absence surfaces at admission, never at first query.
- Cases: `io_method` (`io_uring` primary, `worker` fallback, `Degradable`, `postmaster` restart); `effective_io_concurrency`/`maintenance_io_concurrency` (`Observational`, `user`/`sighup`); `data_checksums` (`Required`, `postmaster` — checksums are an initdb-time decision, so a gap demands a re-init); `shared_preload_libraries` (the `Preload` fragment, `Required`, `postmaster`, carrying the bgworker-preload companion set). `GucRank` — Required | Degradable | Observational. `GucContext` — Internal | Postmaster | Sighup | Superuser | User, the `pg_settings.context` rung that names a setting's repair-disruption class.
- Entry: `public static Fin<GucVerdict> Verify(Seq<GucRow> rows, FrozenDictionary<string, string> observed)` folds each row against the observed `pg_settings` value into a `GucVerdict` — a row matching its primary records `Matched`, a row satisfied only by its fallback records `Fallback` (a degrade receipt naming the restart class), and a `Required` row matching neither aborts the `Fin`; a `Degradable`/`Observational` miss folds into the verdict's receipt set, never the abort. The `Fin` rail is the contract every consumer composes — `Version/recovery` `.MapFail`s a missing replication triad into `RecoveryFault.ReplicationUnready` and `.Bind`s past a satisfied one, so a hard recovery GUC stays an abort while a soft store GUC degrades.
- Auto: GUC fragments are deploy-time `postgresql.conf` assets verified read-only after boot, never executed at runtime; the `io_method` row carries `io_uring` (the Linux-guest value) with the `worker` portable fallback so a kernel lacking io_uring satisfies the verify as a `Fallback` receipt rather than an abort, and the verdict's `search.io.method` fact records WHICH value the server bound so a degraded path is observable, not silent; the `Preload` fragment's `shared_preload_libraries` value (`timescaledb,pg_search,pg_partman_bgw,pg_squeeze,pgaudit,pg_cron`) is the deploy-image bgworker-preload contract — `timescaledb`/`pg_search`/`pg_cron`/`pg_partman_bgw`/`pg_squeeze`/`pgaudit` each require preload, while `vectorscale`/`pg_jsonschema`/`pgvector` load through their index AM or type registration and are correctly absent. The probe binds `$names` as an array parameter so the settings read is one round-trip over any row count with a stable cached plan.
- Receipt: each `Fallback` and each `Degradable`/`Observational` miss folds a `GucVerdict` receipt naming the setting, the observed-versus-expected pair, and the `GucContext` restart rung into the open receipt's proof rows, so a fleet-wide drift in io-method or io-concurrency is a recorded admission fact; a `Required` miss is a typed provisioning fault carrying the same evidence.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Collections.Frozen`)
- Growth: one `GucRow` per new fragment carrying its rank and restart class; one `GucRank` or `GucContext` row per new disposition rung; zero new surface.
- Boundary: this owner verifies, never executes — runtime `ALTER SYSTEM` is the rejected form, and the GUC fragments land as physical `postgresql.conf` assets at the first headless or web app root. The verify admits a setting at its primary or its declared fallback, so the portability split (io_uring on a Linux guest, worker elsewhere) is a `GucRow` field rather than a pinned literal, and the degrade-versus-abort split is the row's `FailureRank`, never a call-site branch. OAuth `pg_hba` posture and role grants are deploy-time `pg_hba`/grant assets verified through this same fold. `SetPostgresVersion(18, 0)` is the provider feature-gate floor owned at `Store/profiles#PROFILE_AXIS`, distinct from the PG18.4 deployment minimum these fragments target. Image composition, Docker/Compose mechanics, and native build/export stay Forge-owned; this page names the required GUC semantics and consumes Assay `ProvisionRun` observations before pinning a row, never a Dockerfile or image-build recipe.

```csharp signature
[SmartEnum<int>]
public sealed partial class GucRank {
    public static readonly GucRank Required = new(0);      // miss aborts the admission
    public static readonly GucRank Degradable = new(1);    // fallback or miss folds a degrade receipt
    public static readonly GucRank Observational = new(2); // miss records evidence only
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class GucContext {
    public static readonly GucContext Internal = new("internal");
    public static readonly GucContext Postmaster = new("postmaster"); // requires full restart / re-init
    public static readonly GucContext Sighup = new("sighup");         // reload-on-SIGHUP
    public static readonly GucContext Superuser = new("superuser");
    public static readonly GucContext User = new("user");
}

public sealed record GucRow(string Setting, string Value, string Fallback, GucRank Rank, GucContext Restart) {
    public static GucRow Hard(string setting, string value, GucContext restart) => new(setting, value, value, GucRank.Required, restart);
}

public readonly record struct GucReceipt(string Setting, string Observed, string Expected, GucContext Restart, bool ViaFallback);

public sealed record GucVerdict(Seq<string> Matched, Seq<GucReceipt> Degraded) {
    public static readonly GucVerdict Empty = new([], []);
}

public static class ClusterConfig {
    public static readonly Seq<GucRow> Rows = [
        new("io_method", "io_uring", "worker", GucRank.Degradable, GucContext.Postmaster),
        new("effective_io_concurrency", "16", "16", GucRank.Observational, GucContext.User),
        new("maintenance_io_concurrency", "16", "16", GucRank.Observational, GucContext.Sighup),
        GucRow.Hard("data_checksums", "on", GucContext.Postmaster),
        GucRow.Hard("shared_preload_libraries", "timescaledb,pg_search,pg_partman_bgw,pg_squeeze,pgaudit,pg_cron", GucContext.Postmaster),
    ];

    public const string SettingsProbe = "SELECT name, setting FROM pg_settings WHERE name = ANY($names)";

    public static Fin<GucVerdict> Verify(Seq<GucRow> rows, FrozenDictionary<string, string> observed) =>
        rows.Fold(Fin.Succ(GucVerdict.Empty), static (acc, row) => acc.Bind(verdict => {
            var live = observed.TryGetValue(row.Setting, out var seen) ? seen : null;
            return live == row.Value
                ? Fin.Succ(verdict with { Matched = verdict.Matched.Add(row.Setting) })
                : live is not null && live == row.Fallback
                    ? Fin.Succ(verdict with { Degraded = verdict.Degraded.Add(new GucReceipt(row.Setting, live, row.Value, row.Restart, ViaFallback: true)) })
                    : row.Rank.Switch(
                        required: () => Fin.Fail<GucVerdict>(Error.New($"<cluster-config-mismatch:{row.Setting}:{live ?? "<absent>"}:{row.Value}:{row.Restart.Key}>")),
                        degradable: () => Fin.Succ(verdict with { Degraded = verdict.Degraded.Add(new GucReceipt(row.Setting, live ?? "<absent>", row.Value, row.Restart, ViaFallback: false)) }),
                        observational: () => Fin.Succ(verdict with { Degraded = verdict.Degraded.Add(new GucReceipt(row.Setting, live ?? "<absent>", row.Value, row.Restart, ViaFallback: false)) }));
        }));
}
```

## [04]-[RESEARCH]

- [BM25_QUERY_SHAPE]: the `pg_search` 0.24.0 query surface this axis projects, proven against the installed extension before the predicate fences pin — the `@@@ pdb.parse($1)` bound-parameter form, the bare `|||`/`&&&`/`===`/`###` column operators, the `a ## n ## b`/`a ##> n ##> b` proximity forms, the `::pdb.fuzzy`/`::pdb.boost` cast-stacking order, the `pdb.boolean(must => …, should => …)` composite, and the `pdb.score`/`pdb.snippet`/`pdb.snippets`/`pdb.snippet_positions`/`pdb.agg` projections at their default tags and `max_num_chars`; the removed `paradedb.*` namespace is asserted absent and the `$N` placeholders are confirmed to bind through `FromSql`/`SqlQuery` rather than interpolate.
- [CLUSTER_CONFIG_PORTABILITY]: the `io_method=io_uring` GUC against the Forge-provisioned local PG18 runtime versus the `worker` portable fallback — whether the kernel exposes io_uring, confirmed against the `pg_settings` observed value so the verify records a `Matched` primary or a `Fallback` degrade receipt with the `postmaster` restart class, and whether each fragment's `pg_settings.context` matches the `GucContext` rung the row declares.
