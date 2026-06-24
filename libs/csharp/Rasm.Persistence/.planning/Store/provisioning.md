# [PERSISTENCE_SERVER_PROVISIONING]

Rasm.Persistence holds two disjoint deploy-time owners on the self-provisioned PostgreSQL 18.4 server tier: `Bm25Predicate`/`SearchProjection`, the `pg_search` `@@@`/`pdb.*` query-projection axis whose query values bind as `$N` parameters and whose column/operator tokens interpolate at one closed-vocabulary seam; and `ClusterConfig`, the GUC-fragment owner that verifies `postgresql.conf` settings read-only against `pg_settings` into a typed `GucVerdict`. The verification fold is the shared contract: `Version/recovery#RECOVERY_LANES` hands it the `wal_level`/`archive_mode`/`archive_command` replication triad, `Store/encryption#AT_REST` the `data_encryption` TDE fragment, `Query/transaction#TWO_PHASE` the `max_prepared_transactions` floor, and `Version/retention#AUDIT_BINDING` the bound `pgaudit.log` audit classes; the owner carries the io/memory/WAL/autovacuum/checksums/preload baseline rows and folds every consumer's fragments through one `GucKind`-dispatched comparison — a `Required` miss aborts, a `Degradable` fragment satisfied by its fallback or a `pending_restart` fix folds a degrade receipt, an `Observational` gap records evidence only. The server-tier raw-SQL provisioning fold (`SchemaDdl.Sql`, the `Hypertable`/`ContinuousAggregate`/`RetentionPolicy`/`ColumnstorePolicy`/`DiskAnn`/`Bm25` index-build cases, `ProvisionSql`, `DiskAnnOptions`) is owned whole at `Schema/ddl#EXTENSION_DDL` and consumed here as settled; the service-deploy migration gate (`SchemaGate.Admit`, `MigrationLaw.Plan`/`Vehicle`/`Script`, `DeployVehicle`, `LockLightStep`, `SchemaEpoch`, `MigrationReceipt`) is owned whole at `Schema/migration` and consumed here as settled. This page mints no migration gate and no `SchemaDdl` case — those owners arrive resolved. The spine is Npgsql, Thinktecture vocabulary, LanguageExt rails, and NodaTime.

Wire posture: this page is host-local — both owners emit or verify server-side artifacts against the deploy-image PostgreSQL, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The preload-gated companions verify through the `Store/profiles#PROVISIONING_ROWS` `PreloadProbe` against the `ClusterConfig` `Preload` `shared_preload_libraries` fragment before any provisioning step runs.

## [01]-[INDEX]

- [01]-[BM25_PROJECTION]: the `pg_search` `@@@`/`pdb.*` query-projection axis, `$N`-parameterized so query values never interpolate as literals.
- [02]-[CLUSTER_CONFIG]: the shared `postgresql.conf` GUC-fragment verifier — baseline server rows plus every consumer's fragments folded read-only against `pg_settings` into a degrade-or-abort `GucVerdict`.

## [02]-[BM25_PROJECTION]

- Owner: `Bm25Predicate` is the `pg_search` `@@@`/`pdb.*`/bare-column-operator query-projection algebra and `SearchProjection` its relevance/highlight/facet projection surface — the C# image of `.api/api-pg-search.md` sections `[03]`/`[04]`/`[05]`, consumed at `Query/lanes#SEARCH_LANES` by `HybridRetrieve.LexicalBranch` and folded into the row store by `FromSql`/`SqlQuery`. Every query VALUE is a `$N` bound parameter, never a literal; only the column, operator, relation, and namespace tokens — a closed vocabulary — interpolate, so the `[TWO_DOOR_ROUTING]` law "holes become parameters mechanically" holds at the one place runtime text reaches the engine.
- Cases: bare column operators (`AnyToken` `|||`, `AllToken` `&&&`, `ExactTerm` `===`, `Phrase` `###`); the `@@@` builders (`Parse`; `Match` — the analyzed builder carrying an inline tokenizer override, fuzzy `distance`/`prefix`, and `conjunction_mode` in one fragment, the column-targeted superset of the bare token operators; `RangeTerm` — range value cast to its range type, `RangeRelation` the positional `Intersects`/`Contains`/`Within` vocabulary, never a bound value; `PhrasePrefix` defaulting `max_expansions => 50`; `MoreLikeThis` over the `MoreLikeSource` `key_value`/`document` two-form carrying the full `MoreLikeTuning` term-selection surface — `min_term_frequency`/`min_doc_frequency`/`max_doc_frequency`/`max_query_terms`/`min_word_length`/`max_word_length`/`boost_factor`/`stopwords` — plus the key-form `fields` restriction array; `Regex`; `Exists` — the indexed field-presence builder; `All`); the two proximity builders (`Near` `##`, `OrderedNear` `##>` — `.api` `[03]` rows `[06]`/`[07]`); the cast-stack modifiers each over an inner predicate (`Fuzzy ::pdb.fuzzy`, `Boost ::pdb.boost`, `Const ::pdb.const`, `Slop ::pdb.slop`, `Token ::pdb.<tokenizer>` — `.api` `[04]` note); and `Boolean`, the must/should/must-not composite (`pdb.boolean(<clause> => ARRAY[…])`) keying each clause to one `ARRAY` of child predicates folded into one Tantivy boolean query. A modifier nests over any inner case and casts stack in cast order, so a new builder, operator, modifier, or tokenizer is one union case, never a sibling method. Every optional knob is `Option`-typed against its `NULL`-defaultable server arg, so only the set arguments emit and a fully-unset tuning yields the bare builder form.
- Entry: `public Bm25Sql Sql(int from = 1)` is the total generated `Switch` projecting `Bm25Sql(string Text, Seq<string> Binds)` — the predicate fragment carries `$from…$N` placeholders threaded by the ordinal the arm advances, and `Binds` is the parallel value vector the `FromSql`/`SqlQuery` boundary parameterizes; a nested modifier or `Boolean` child re-bases its inner placeholders at `from + Binds.Count` so one tree yields one contiguous parameter run. `SearchProjection` members project the `key_field`-anchored `pdb.score`/`pdb.snippet`/`pdb.snippets`/`pdb.snippet_positions`/`pdb.agg` SQL as bare strings — they sit in `SELECT`/`ORDER BY` beside the `WHERE` predicate, carry no runtime VALUE, and interpolate their author-fixed tags/sort/agg-spec config at the same seam the column identifier uses, so they never collide with the predicate's ordinal run.
- Auto: the BM25 index answers through `@@@` builder dispatch (`pdb.parse`/`pdb.match`/`pdb.range_term`/`pdb.phrase_prefix`/`pdb.more_like_this`/`pdb.regex`/`pdb.exists`/`pdb.all`/`pdb.boolean`), the bare `|||`/`&&&`/`===`/`###` column operators, the `##`/`##>` proximity forms, and the `pdb.score`/`pdb.snippet` projections beside the always-present native FTS baseline — a profile without `pg_search` preloaded routes `HybridRetrieve.LexicalBranch(pgSearch: false)` to the `ts_rank` projection inside the same fusion CTE. The `0.24.x` line is the v2 `pdb.*` API — the legacy `paradedb.*` namespace is removed, so only the `pdb` schema and the bare operators are ever emitted. The snippet projections default `<b>`/`</b>` tags and `150` `max_num_chars` (`.api` `[05]`); `pdb.agg` carries an Elasticsearch-shaped aggregation document for facet/metric rollups over the matched set. The `key_field` is the `UNIQUE`/primary column the index lists first and every `pdb.score`/`pdb.snippet` anchors on, so the fusion projects identities and re-queries the row store rather than re-materializing payloads.
- Receipt: the `search.bm25.score` fact on `Query/lanes#SEARCH_LANES` reads the live `pdb.score` the index serves and the `search.vector.route` fact the planner's chosen route; this axis projects SQL and owns no provisioning step, so it folds no `StoreFact` of its own.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new `pdb.*` builder, bare operator, proximity form, cast modifier, tokenizer cast, or boolean-composite arm is one `Bm25Predicate` union case; a new term-selection knob on `more_like_this` is one `Option` field on the `MoreLikeTuning` value object the builder already threads; a new relevance/highlight/facet projection is one `SearchProjection` member; zero new surface.
- Boundary: `pg_search` runs in-process inside the PG18 server tier, never linked into managed code, so its AGPL boundary is the DB deployment; this axis is its C# query-projection image only. The index-build DDL (`SchemaDdl.Bm25`, one `CREATE INDEX … USING bm25` per table keyed `key_field`-first) and the extension's preload-gated install are owned at `Schema/ddl#EXTENSION_DDL` and consumed as settled — this section owns the query-time predicate algebra, never the index declaration. The column-type and the embedding/full-text query shape (`FullTextQuery`, `EmbeddingArity`, `HybridRetrieve.Fuse`) are owned at `Query/lanes#SEARCH_LANES`; that lane binds `$terms` into `corpus @@@ pdb.parse($terms)`, so `Bm25Predicate.Sql()` MUST project the same `$N`-parameter shape — a literal-interpolated query value here would fork the binder and reopen the injection surface the parameter run closes. The cast-stacking law is `pg_search`'s own (`'q'::pdb.fuzzy(2)::pdb.boost(2)` applies typo tolerance then a score multiplier); the union models it by nesting, never by a combinatorial method family.

```csharp signature
public readonly record struct Bm25Sql(string Text, Seq<string> Binds) {
    public Bm25Sql Nest(Func<string, string> wrap) => this with { Text = wrap(Text) };
}

// The three `pdb.boolean` clause keys; the `Key` is the named-argument token the builder emits.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class BoolClause {
    public static readonly BoolClause Must = new("must");
    public static readonly BoolClause Should = new("should");
    public static readonly BoolClause MustNot = new("must_not");
}

// The `pdb.range_term` relation: a closed Tantivy vocabulary, emitted as the positional PascalCase token
// (`'Intersects'`), never a `$N` bind — a relation is grammar at the identifier seam, not a query VALUE.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class RangeRelation {
    public static readonly RangeRelation Intersects = new("Intersects");
    public static readonly RangeRelation Contains = new("Contains");
    public static readonly RangeRelation Within = new("Within");
}

// The `pdb.more_like_this` probe source: the `key_field` value of an indexed document (positional
// `$N`, with the optional `fields => ARRAY[...]` restriction) or a JSON document (positional `$N::json`).
// The two `pg_search` overloads (`more_like_this_id`/`more_like_this_fields`) resolve on the first
// argument's type, so the case selects the bind cast and whether the `fields` restriction emits; the
// value always binds, never quotes in. `Fields` is honored on `Key` only (the JSON document form names
// its own fields), so an empty `Fields` on a `Document` source is a no-op the builder drops.
[Union<long, string>(T1Name = "Key", T2Name = "Document")]
public readonly partial struct MoreLikeSource;

// The `pdb.more_like_this` term-selection tuning (`mlt.rs` `more_like_this_id`/`more_like_this_fields`):
// every knob is `NULL`-defaultable server-side, so `Option<int>`/`Option<double>` carries "unset" and the
// builder emits only the bound named arguments — a fully-`None` tuning yields the bare `pdb.more_like_this($N)`
// form. `Stopwords` binds as one `$N::text[]` array. One composed value, never eight flat record fields.
public readonly record struct MoreLikeTuning(
    Option<int> MinTermFrequency = default,
    Option<int> MinDocFrequency = default,
    Option<int> MaxDocFrequency = default,
    Option<int> MaxQueryTerms = default,
    Option<int> MinWordLength = default,
    Option<int> MaxWordLength = default,
    Option<double> BoostFactor = default,
    Seq<string> Stopwords = default) {
    public static readonly MoreLikeTuning None = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Bm25Predicate {
    private Bm25Predicate() { }

    // Bare column operators: the value binds as `$N`, never a quoted literal. `AnyToken`/`AllToken` are
    // the analyzed `match_disjunction`/`match_conjunction` route; `ExactTerm` the un-analyzed `===` term;
    // `Phrase` the ordered `###`.
    public sealed record AnyToken(string Column, string Query) : Bm25Predicate;
    public sealed record AllToken(string Column, string Query) : Bm25Predicate;
    public sealed record ExactTerm(string Column, string Term) : Bm25Predicate;
    public sealed record Phrase(string Column, string PhraseText) : Bm25Predicate;
    // The `@@@ pdb.match` analyzed builder carrying its per-match fuzzy `distance`/`prefix` and the
    // `conjunction_mode` toggle in one fragment — the column-targeted, fuzzy-tolerant superset of the bare
    // `AnyToken`/`AllToken` token operators. Every knob is `NULL`-defaultable, so `Option` carries "unset"
    // and only the bound named args emit; the tokenizer override rides the `Token` cast case, never here.
    public sealed record Match(string Column, string Query, Option<int> Distance, Option<bool> Prefix, Option<bool> ConjunctionMode) : Bm25Predicate;
    // `@@@` builders. The range value binds as `$N::<rangeType>` (a full range literal cast to its range
    // type), the relation is the positional bounded-vocabulary token, and the More-Like-This source is the
    // closed two-form `key_value` (a `key_field` value, optionally field-restricted) or `document` (a JSON
    // probe) the v2 builder admits, carrying its full term-selection tuning.
    public sealed record Parse(string Column, string QueryString, bool Lenient, bool ConjunctionMode) : Bm25Predicate;
    public sealed record RangeTerm(string Column, string Range, string RangeType, RangeRelation Relation) : Bm25Predicate;
    public sealed record PhrasePrefix(string Column, Seq<string> Tokens, int MaxExpansions = 50) : Bm25Predicate;
    public sealed record MoreLikeThis(string Column, MoreLikeSource Source, Seq<string> Fields, MoreLikeTuning Tuning) : Bm25Predicate;
    public sealed record Regex(string Column, string Pattern) : Bm25Predicate;
    // `@@@ pdb.exists()` — the field-presence builder: a row matches when the indexed field is non-null,
    // the indexed companion to a SQL `IS NOT NULL` that rides the BM25 index rather than the heap.
    public sealed record Exists(string Column) : Bm25Predicate;
    public sealed record All(string Column) : Bm25Predicate;
    // Proximity (`a ## n ## b`, `a ##> n ##> b`): both terms bind, the slack is a literal int.
    public sealed record Near(string Column, string Left, string Right, int Slack) : Bm25Predicate;
    public sealed record OrderedNear(string Column, string Left, string Right, int Slack) : Bm25Predicate;
    // Boolean composite: each clause keys ONE `=> ARRAY[...]` of child predicates, so a clause carrying
    // several terms emits one named-arg array (`must => ARRAY[$1, $2]`), never a repeated `must =>` arg.
    public sealed record Boolean(string Column, Seq<(BoolClause Clause, Seq<Bm25Predicate> Terms)> Clauses) : Bm25Predicate;
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
            // The analyzed match: the value binds, and only the set knobs emit as named args.
            match: m => new Bm25Sql($"{m.Column} @@@ pdb.match(${from}{Args(("distance", m.Distance.Map(Int)), ("prefix", m.Prefix.Map(Lit)), ("conjunction_mode", m.ConjunctionMode.Map(Lit)))})", [m.Query]),
            // The range value binds once, cast to its range type at the seam; the relation is the closed
            // PascalCase token, positional — never a parameter, so the value run carries exactly one hole.
            rangeTerm: r => new Bm25Sql($"{r.Column} @@@ pdb.range_term(${from}::{r.RangeType}, '{r.Relation.Key}')", [r.Range]),
            phrasePrefix: p => new Bm25Sql(
                $"{p.Column} @@@ pdb.phrase_prefix(ARRAY[{Holes(from, p.Tokens.Count)}], max_expansions => {p.MaxExpansions})", p.Tokens),
            // One dispatch builds the whole fragment per source: the `key_field` value binds as `$N::bigint`
            // with the optional positional `$N::text[]` `fields` restriction, or the JSON probe binds as
            // `$N::json`; the term-selection knobs emit only when set, so a fully-`None` tuning yields the
            // bare builder — positional args precede the named knobs so the PG call grammar stays valid.
            moreLikeThis: m => m.Source.Switch(
                key: k => MoreLike(m, from, "::bigint", k.ToString(CultureInfo.InvariantCulture)),
                document: d => MoreLike(m, from, "::json", d)),
            regex: r => new Bm25Sql($"{r.Column} @@@ pdb.regex(${from})", [r.Pattern]),
            exists: e => new Bm25Sql($"{e.Column} @@@ pdb.exists()", []),
            all: a => new Bm25Sql($"{a.Column} @@@ pdb.all()", []),
            near: n => new Bm25Sql($"{n.Column} @@@ (${from} ## {n.Slack} ## ${from + 1})", [n.Left, n.Right]),
            orderedNear: n => new Bm25Sql($"{n.Column} @@@ (${from} ##> {n.Slack} ##> ${from + 1})", [n.Left, n.Right]),
            // Each clause emits one `clause => ARRAY[...]` named arg whose children re-base onto the running
            // ordinal, so the whole boolean query yields one contiguous `$1…$N` run across every term.
            boolean: b => b.Clauses.Fold(new Bm25Sql($"{b.Column} @@@ pdb.boolean(", []), (acc, clause) => {
                    var array = clause.Terms.Fold(new Bm25Sql("", []), (inner, term) => {
                        var child = term.Sql(from + acc.Binds.Count + inner.Binds.Count);
                        return new Bm25Sql(inner.Binds.IsEmpty ? child.Text : $"{inner.Text}, {child.Text}", inner.Binds + child.Binds);
                    });
                    var lead = acc.Binds.IsEmpty ? "" : ", ";
                    return new Bm25Sql($"{acc.Text}{lead}{clause.Clause.Key} => ARRAY[{array.Text}]", acc.Binds + array.Binds);
                }).Nest(static text => $"{text})"),
            fuzzy: f => f.Inner.Sql(from).Nest(inner => $"{inner}::pdb.fuzzy({f.Distance}, {Lit(f.Prefix)}, {Lit(f.TranspositionCostOne)})"),
            boost: b => b.Inner.Sql(from).Nest(inner => $"{inner}::pdb.boost({b.Factor.ToString(CultureInfo.InvariantCulture)})"),
            @const: c => c.Inner.Sql(from).Nest(inner => $"{inner}::pdb.const({c.Score.ToString(CultureInfo.InvariantCulture)})"),
            slop: s => s.Inner.Sql(from).Nest(inner => $"{inner}::pdb.slop({s.Distance})"),
            token: t => t.Inner.Sql(from).Nest(inner => $"{inner}::pdb.{t.Tokenizer}"));

    static string Lit(bool flag) => flag ? "true" : "false";
    static string Int(int n) => n.ToString(CultureInfo.InvariantCulture);
    static string Holes(int from, int count) => string.Join(", ", Range(from, count).Map(static i => $"${i}"));

    // Render only the SET optional named arguments after a bound positional `$N`, so an all-`None` knob set
    // emits the bare builder form `pdb.x($N)` and a partial set emits exactly its present args in order.
    static string Args(params ReadOnlySpan<(string Name, Option<string> Value)> args) {
        var set = toSeq(args.ToArray()).Choose(a => a.Value.Map(v => $"{a.Name} => {v}"));
        return set.IsEmpty ? "" : $", {string.Join(", ", set)}";
    }

    // The `key_field`/`document` form: the probe binds positionally at `$from`, the `fields` restriction
    // (key form, non-empty) binds as one `$N::text[]` array, `stopwords` (when set) as a second, and every
    // scalar tuning knob emits only when present — so no term-selection value ever interpolates as a literal
    // and the bind vector is the contiguous probe → fields → stopwords run the SQL boundary parameterizes.
    static Bm25Sql MoreLike(MoreLikeThis m, int from, string cast, string probe) {
        var keyForm = cast.Contains("bigint", StringComparison.Ordinal);
        var fieldsBind = keyForm && !m.Fields.IsEmpty ? Some(PgArray(m.Fields)) : None;
        var stopBind = !m.Tuning.Stopwords.IsEmpty ? Some(PgArray(m.Tuning.Stopwords)) : None;
        var binds = Seq1(probe) + fieldsBind.ToSeq() + stopBind.ToSeq();
        var fields = fieldsBind.Map(_ => $", ${from + 1}::text[]").IfNone("");
        var stop = stopBind.Map(_ => $"${from + binds.Count - 1}::text[]");
        var t = m.Tuning;
        return new Bm25Sql(
            $"{m.Column} @@@ pdb.more_like_this(${from}{cast}{fields}{Args(("min_term_frequency", t.MinTermFrequency.Map(Int)), ("min_doc_frequency", t.MinDocFrequency.Map(Int)), ("max_doc_frequency", t.MaxDocFrequency.Map(Int)), ("max_query_terms", t.MaxQueryTerms.Map(Int)), ("min_word_length", t.MinWordLength.Map(Int)), ("max_word_length", t.MaxWordLength.Map(Int)), ("boost_factor", t.BoostFactor.Map(d => d.ToString(CultureInfo.InvariantCulture))), ("stopwords", stop))})", binds);
    }

    // A `Seq<string>` rendered as one PostgreSQL `text[]` array literal the boundary binds as a single
    // parameter — `{a,b,c}` with each token quoted, so the array crosses as one hole, never N spliced ones.
    static string PgArray(Seq<string> tokens) => $"{{{string.Join(",", tokens.Map(static t => $"\"{t.Replace("\"", "\\\"", StringComparison.Ordinal)}\""))}}}";
}

// The `pdb.snippets` `sort_by` is a closed two-value server vocabulary (`snippet.rs` rejects anything
// but `score`/`position`), so it is a `[SmartEnum]`, never an unbounded `string` the engine would reject.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class SnippetSort {
    public static readonly SnippetSort Score = new("score");
    public static readonly SnippetSort Position = new("position");
}

// Projections sit in SELECT/ORDER BY beside the WHERE predicate's `$N` run, so they carry NO bound
// parameter — the column is an identifier and the tags/sort/agg-spec are author-fixed closed-vocabulary
// config, never runtime query VALUES, so they interpolate at the same seam identifiers do and never
// collide with the predicate's ordinal run.
public static class SearchProjection {
    public static string Score(string keyColumn) => $"pdb.score({keyColumn})";
    public static string Snippet(string column, string startTag = "<b>", string endTag = "</b>", int maxChars = 150) =>
        $"pdb.snippet({column}, start_tag => '{startTag}', end_tag => '{endTag}', max_num_chars => {maxChars})";
    public static string Snippets(string column, int maxChars, int limit, int offset, SnippetSort? sortBy = null, string startTag = "<b>", string endTag = "</b>") =>
        $"""pdb.snippets({column}, start_tag => '{startTag}', end_tag => '{endTag}', max_num_chars => {maxChars}, "limit" => {limit}, "offset" => {offset}, sort_by => '{(sortBy ?? SnippetSort.Score).Key}')""";
    public static string SnippetPositions(string column) => $"pdb.snippet_positions({column})";
    public static string Agg(string esJson) => $"pdb.agg('{esJson}') OVER ()";
}
```

## [03]-[CLUSTER_CONFIG]

- Owner: `ClusterConfig` — the PG18 server-GUC verifier whose `Verify` fold is the one read-only `pg_settings` admission contract every consumer composes. `Version/recovery#RECOVERY_LANES` hands it the `wal_level`/`archive_mode`/`archive_command` replication triad, `Store/encryption#AT_REST` the `data_encryption` TDE fragment, `Query/transaction#TWO_PHASE` the `max_prepared_transactions` floor, and `Version/retention#AUDIT_BINDING` the bound `pgaudit.log` audit classes; the owner itself carries the `Baseline` server rows — io, memory, WAL, checkpoint, autovacuum, jit, checksums, and the `Preload` `shared_preload_libraries` set. Each fragment is a `GucRow` carrying its primary value, its portable fallback, the `GucKind` comparison axis (the `pg_settings.vartype` so a numeric GUC matches as a floor, a csv GUC as a token superset, a boolean GUC by normalized truth, and an enum or string by equality with the primary or the fallback — never one brittle `==`), a `GucRank` (`Required` aborts, `Degradable` folds a degrade receipt, `Observational` records evidence), and the `GucContext` restart class so a gap names its repair-disruption rung. The verification yields one typed `GucVerdict` carrying matched settings, degrade receipts (a fallback-satisfied fragment, a soft miss, or a value bound but `pending_restart`), and the abort set, so absence surfaces at admission, never at first query.
- Cases: `GucKind` — `Boolean` | `Integer` | `Real` | `Enumeration` | `Text` | `List`, the `pg_settings.vartype` rung that selects the comparison algebra (`Integer`/`Real` admit a server value at or above the floor over the bare `pg_settings.setting` integer in the setting's native unit, `List` admits a superset of required tokens, `Boolean` admits a normalized truth match across the `on`/`true`/`1` spellings, `Enumeration`/`Text` demand equality with the primary or the fallback). The primary/fallback pair, not a built-in ordering, encodes the only ranked enum the baseline needs (a server at the `wal_level` fallback `replica` degrades, one at the primary `logical` matches). `GucRank` — `Required` | `Degradable` | `Observational`. `GucContext` — `Internal` | `Postmaster` | `Sighup` | `Superuser` | `User`, the `pg_settings.context` repair-disruption rung. `Baseline` rows: `io_method` (`io_uring` primary, `worker` fallback, `Enumeration`, `Degradable`, `Postmaster`); `effective_io_concurrency`/`maintenance_io_concurrency` (`Integer` floor, `Observational`); `shared_buffers`/`effective_cache_size`/`work_mem`/`maintenance_work_mem` (`Integer` floor in the setting's native block/kB unit, `Observational` — a memory floor underrun is observable drift, not an abort); `wal_level` (`Enumeration` `logical` primary / `replica` fallback, `Degradable`); `wal_compression` (`Enumeration` `lz4`/`on`, `Observational`); `checkpoint_completion_target`/`max_wal_size` (`Real`/`Integer` floor, `Observational`); `autovacuum`/`autovacuum_vacuum_cost_limit` (`Boolean`/`Integer`, `Required`/`Observational` — autovacuum off is a hard provisioning fault); `jit` (`Boolean`, `Observational`); `data_checksums` (`Boolean` equality, `Required`, `Postmaster` — checksums are an initdb-time decision, so a gap demands a re-init); `pgaudit.log` (`List` superset, `Degradable`, `Sighup` — the soft `ddl,role` deploy default; the hard per-classification audit-class set is the `Required` fragment `Version/retention#AUDIT_BINDING` passes, sourced from its present `DataClassification` set, so a deployment with no classified data degrades rather than aborting); `shared_preload_libraries` (the `Preload` fragment, `List` superset, `Required`, `Postmaster`, carrying the bgworker-preload companion set).
- Entry: `public static Fin<GucVerdict> Verify(Seq<GucRow> rows, FrozenDictionary<string, GucReading> observed)` folds each row against the observed `pg_settings` reading through the row's `GucKind` comparison into a `GucVerdict` — a reading that satisfies the primary records `Matched`, a reading satisfied only by the fallback or one bound but flagged `pending_restart` records a degrade receipt naming the restart class, and a `Required` row failing on a hard reason (`GucDegrade.Aborts`) accrues an abort; a `Degradable`/`Observational` miss, a fallback hold, or a pending-restart on any rank folds into the verdict's receipt set, never the abort. The fragments are INDEPENDENT, so the fold accumulates every hard miss and aborts ONCE at the end through `Error.Many`, surfacing the full repair set in one verdict rather than the first miss per redeploy — symmetric with the accumulating `Version/retention#AUDIT_BINDING` `Validation` fold. The entrypoint parameterizes BOTH the row ingress and the reading ingress: `Verify(Baseline, observed)` checks the owner's server rows, a consumer passes its own fragments as `(setting, value, fallback)` triples the implicit `GucRow` conversion lifts to hard `Text`-equality rows, and the reading dictionary admits two shapes — the full `SettingsProbe` `GucReading` projection, or a bare `FrozenDictionary<string, string>` whose values lift through the implicit `string -> GucReading` so a consumer holding only `Show()` reads composes without a parallel verifier. `Version/recovery` hands `[("wal_level", "logical", "logical"), ("archive_mode", "on", "on"), ("archive_command", "set", "set")]` against its `Show()`-sourced string dictionary, `.MapFail`s the aggregated result into `RecoveryFault.ReplicationUnready`, and `.Bind`s past a satisfied one, so a hard recovery GUC stays an abort naming every breached setting while a soft store GUC degrades.
- Auto: GUC fragments are deploy-time `postgresql.conf` assets verified read-only after boot, never executed at runtime. The `GucReading` carries the observed `setting`, the `pending_restart` flag, the `pg_settings.source` rung, and the `reset_val` so a setting whose `ALTER SYSTEM`-staged value awaits a restart degrades rather than reads green — the `setting`/`pending_restart` pair makes a half-applied fragment an observable `pending-restart` receipt carrying the staged `reset_val`, not a silent pass, and `source = 'default'` is the authoritative `Unset` signal so a value an operator deliberately set equal to the compiled default grades `Below` rather than the `boot_val`-proxy false `Unset`. The `io_method` row carries `io_uring` (the Linux-guest value) with the `worker` portable fallback so a kernel lacking io_uring satisfies the verify as a fallback receipt rather than an abort, and the verdict's `search.io.method` fact records WHICH value the server bound. The `pgaudit.log` baseline row is a `Degradable` `List`-superset check against the soft `ddl,role` deploy default, so a server with no audit classes degrades with a receipt; the hard escalation is the `Required` `pgaudit.log` fragment the `Version/retention#AUDIT_BINDING` row passes carrying the exact classes its present `DataClassification` set demands, so a narrower-than-required bound class set is a provisioning miss naming the absent categories rather than an audit gap discovered at first sensitive write. The `Preload` fragment's `shared_preload_libraries` value (`timescaledb,pg_search,pg_partman_bgw,pg_squeeze,pgaudit,pg_cron,pg_stat_statements`) is the deploy-image bgworker-preload contract checked as a superset so an operator who preloads extra libraries still satisfies it — `timescaledb`/`pg_search`/`pg_cron`/`pg_partman_bgw`/`pg_squeeze`/`pgaudit`/`pg_stat_statements` each allocate startup shared memory and cannot load through `CREATE EXTENSION`, so each requires preload, while `vectorscale`/`pg_jsonschema`/`pgvector` load through their index AM or type registration and are correctly absent. The probe binds `$names` as an array parameter so the settings read is one round-trip over any row count with a stable cached plan, reading `name`, `setting`, `source`, `reset_val`, `pending_restart`, and `context` so the verdict's restart rung, never-set distinction, and staged-value receipt all derive from the live catalog rather than the declared row.
- Receipt: each fallback, each `pending_restart` fragment, and each `Degradable`/`Observational` miss folds a `GucReceipt` naming the setting, the observed-versus-expected pair (a `pending_restart` receipt naming the staged `live->reset_val` transition), the `GucContext` restart rung, and the degrade reason (`ViaFallback` | `PendingRestart` | `Below` | `Unset`, the last splitting a misconfigured fragment from one never set, read off `pg_settings.source = 'default'`) into the open receipt's proof rows, so a fleet-wide drift in io-method, io-concurrency, or a half-applied WAL setting is a recorded admission fact; the `Required` misses accumulate into one `Error.Many` carrying every breached `setting:reason:observed:expected:restart` tuple, so a multi-fragment provisioning gap names the whole repair set at once rather than one miss per redeploy.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Collections.Frozen`)
- Growth: one `GucRow` on `Baseline` (or one consumer-passed triple) per new fragment carrying its kind, rank, and restart class; one `GucKind`, `GucRank`, or `GucContext` row per new comparison or disposition rung the generated `Switch` then forces every fold site to handle; zero new surface.
- Boundary: this owner verifies, never executes — runtime `ALTER SYSTEM` is the rejected form, and the GUC fragments land as physical `postgresql.conf` assets at the first headless or web app root. The verify admits a setting at its primary or its declared fallback through the row's `GucKind`, so the portability split (io_uring on a Linux guest, worker elsewhere) is a `GucRow` field rather than a pinned literal, the floor-versus-equality split is the row's `GucKind` rather than a per-setting branch, and the degrade-versus-abort split is the row's `GucRank`, never a call-site branch. A numeric floor (`effective_io_concurrency`, `work_mem`, `max_wal_size`) admits a server tuned above the minimum, so a verify never faults a generously-provisioned host the way a `==` would. OAuth `pg_hba` posture and role grants are deploy-time `pg_hba`/grant assets verified through this same fold. `SetPostgresVersion(18, 0)` is the provider feature-gate floor owned at `Store/profiles#PROFILE_AXIS`, distinct from the PG18.4 deployment minimum these fragments target. Image composition, Docker/Compose mechanics, and native build/export stay Forge-owned; this page names the required GUC semantics and consumes Assay `ProvisionRun` observations before pinning a row, never a Dockerfile or image-build recipe.

```csharp signature
// The `pg_settings.vartype` rung selects the comparison algebra: a numeric GUC admits a server value at
// or above the floor over the bare `setting` integer in its native unit, a `List` GUC admits a superset
// of required tokens, `Boolean` normalizes the `on`/`true`/`1` spellings, and `Enumeration`/`Text` demand
// equality. The `Satisfies` delegate is the row's behavior (POLICY_VALUES) so the fold never re-derives
// the var-type from a switch; the primary/fallback pair encodes the only ranked enum the baseline needs.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class GucKind {
    public static readonly GucKind Boolean = new("bool", static (want, have) => string.Equals(Norm(want), Norm(have), StringComparison.Ordinal));
    public static readonly GucKind Text = new("string", static (want, have) => string.Equals(want, have, StringComparison.Ordinal));
    public static readonly GucKind Enumeration = new("enum", static (want, have) => string.Equals(want, have, StringComparison.OrdinalIgnoreCase));
    public static readonly GucKind Integer = new("integer", static (want, have) => long.TryParse(have, NumberStyles.Integer, CultureInfo.InvariantCulture, out var h) && long.TryParse(want, NumberStyles.Integer, CultureInfo.InvariantCulture, out var w) && h >= w);
    public static readonly GucKind Real = new("real", static (want, have) => double.TryParse(have, NumberStyles.Float, CultureInfo.InvariantCulture, out var h) && double.TryParse(want, NumberStyles.Float, CultureInfo.InvariantCulture, out var w) && h >= w);
    public static readonly GucKind List = new("list", static (want, have) => Tokens(want).ForAll(t => Tokens(have).Contains(t)));

    [UseDelegateFromConstructor]
    public partial bool Satisfies(string required, string observed);

    static string Norm(string flag) => flag is "1" or "yes" or "true" ? "on" : flag is "0" or "no" or "false" ? "off" : flag;
    static Seq<string> Tokens(string csv) => csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToSeq();
}

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

// The reason a fragment failed the primary: its fallback held, the value is staged but awaits a restart,
// the operator set it wrong (`Below`), or it was never set at all (`Unset`, read off `source = 'default'`)
// — the last two split "misconfigured" from "forgotten" so a `Required` repair receipt is precise. The
// `Aborts` flag is the abort-disposition POLICY_VALUE: a `Required` row failing on a hard reason (the
// value is genuinely wrong or absent) aborts, while a soft reason (the fallback held, or a correct value
// merely awaits a restart) degrades even on a `Required` row — so the fold never branches on the reason.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class GucDegrade {
    public static readonly GucDegrade ViaFallback = new("via-fallback", aborts: false);
    public static readonly GucDegrade PendingRestart = new("pending-restart", aborts: false);
    public static readonly GucDegrade Below = new("below", aborts: true);
    public static readonly GucDegrade Unset = new("unset", aborts: true);

    public bool Aborts { get; }
}

// One `pg_settings` observation: the live `setting`, whether an `ALTER SYSTEM` change still awaits a
// restart, the `pg_settings.source` rung (`'default'` is the authoritative never-set signal — a value set
// equal to the default reads `'configuration file'`, so `source` splits "forgotten" from "deliberately
// equal" where the `boot_val` proxy cannot), and the `reset_val` a reload would apply (the staged value
// behind a pending restart). A bare value implicitly lifts so a consumer holding only the setting string composes.
public readonly record struct GucReading(string Value, bool PendingRestart, string Source, string ResetValue) {
    public bool NeverSet => string.Equals(Source, "default", StringComparison.Ordinal);

    public static implicit operator GucReading(string value) => new(value, PendingRestart: false, Source: "session", ResetValue: value);
}

public sealed record GucRow(string Setting, string Value, string Fallback, GucKind Kind, GucRank Rank, GucContext Restart) {
    // A consumer's `(setting, value, fallback)` triple lifts to a hard `Text`-equality row, so a
    // `Seq<GucRow>` collection expression of bare triples composes the same fold the baseline rows do.
    public static implicit operator GucRow((string Setting, string Value, string Fallback) triple) =>
        new(triple.Setting, triple.Value, triple.Fallback, GucKind.Text, GucRank.Required, GucContext.Sighup);

    public static GucRow Hard(string setting, string value, GucKind kind, GucContext restart) => new(setting, value, value, kind, GucRank.Required, restart);
    public static GucRow Floor(string setting, string value, GucContext restart) => new(setting, value, value, GucKind.Integer, GucRank.Observational, restart);

    // The comparison verdict for a live reading: `None` is a clean primary match, `Some(reason)` carries
    // the typed degrade (fallback held, restart pending) or the hard miss the rank turns into an abort —
    // `Unset` when `source` is `'default'` (operator never set it), `Below` when it was set wrong. The
    // `source` rung is authoritative over the old `value == boot_val` proxy, which false-flags a value an
    // operator deliberately set equal to the compiled default. A reading absent from the catalog grades `Unset`.
    public Option<GucDegrade> Grade(GucReading reading) =>
        Kind.Satisfies(Value, reading.Value)
            ? reading.PendingRestart ? Some(GucDegrade.PendingRestart) : None
            : Kind.Satisfies(Fallback, reading.Value) ? Some(GucDegrade.ViaFallback)
                : reading.NeverSet ? Some(GucDegrade.Unset) : Some(GucDegrade.Below);
}

public readonly record struct GucReceipt(string Setting, string Observed, string Expected, GucContext Restart, GucDegrade Reason);

public sealed record GucVerdict(Seq<string> Matched, Seq<GucReceipt> Degraded) {
    public static readonly GucVerdict Empty = new([], []);

    public bool Restarts => Degraded.Exists(static r => r.Reason == GucDegrade.PendingRestart);
}

public static class ClusterConfig {
    // The owner's own server-provisioning rows: io, memory, WAL, checkpoint, autovacuum, jit, checksums,
    // and the `Preload` bgworker set. Consumers pass their fragments as `(setting, value, fallback)` triples
    // the implicit `GucRow` conversion lifts. Numeric rows carry the bare `pg_settings.setting` integer in
    // the setting's native unit (8kB blocks for `*_buffers`/`effective_cache_size`, kB for `*work_mem`,
    // MB for `max_wal_size`) so the floor compares against the raw catalog read, never a suffixed literal.
    public static readonly Seq<GucRow> Baseline = [
        new("io_method", "io_uring", "worker", GucKind.Enumeration, GucRank.Degradable, GucContext.Postmaster),
        GucRow.Floor("effective_io_concurrency", "16", GucContext.User),
        GucRow.Floor("maintenance_io_concurrency", "16", GucContext.Sighup),
        GucRow.Floor("shared_buffers", "16384", GucContext.Postmaster),
        GucRow.Floor("effective_cache_size", "524288", GucContext.Sighup),
        GucRow.Floor("work_mem", "4096", GucContext.User),
        GucRow.Floor("maintenance_work_mem", "65536", GucContext.User),
        new("wal_level", "logical", "replica", GucKind.Enumeration, GucRank.Degradable, GucContext.Postmaster),
        new("wal_compression", "lz4", "on", GucKind.Enumeration, GucRank.Observational, GucContext.Superuser),
        GucRow.Floor("max_wal_size", "1024", GucContext.Sighup),
        new("checkpoint_completion_target", "0.9", "0.9", GucKind.Real, GucRank.Observational, GucContext.Sighup),
        GucRow.Hard("autovacuum", "on", GucKind.Boolean, GucContext.Sighup),
        GucRow.Floor("autovacuum_vacuum_cost_limit", "200", GucContext.Sighup),
        new("jit", "on", "off", GucKind.Boolean, GucRank.Observational, GucContext.Superuser),
        GucRow.Hard("data_checksums", "on", GucKind.Boolean, GucContext.Postmaster),
        new("pgaudit.log", "ddl,role", "ddl", GucKind.List, GucRank.Degradable, GucContext.Sighup),
        GucRow.Hard("shared_preload_libraries", "timescaledb,pg_search,pg_partman_bgw,pg_squeeze,pgaudit,pg_cron,pg_stat_statements", GucKind.List, GucContext.Postmaster),
    ];

    public const string SettingsProbe = "SELECT name, setting, source, reset_val, pending_restart, context FROM pg_settings WHERE name = ANY($names)";

    // A consumer holding only a bare `setting`-string read (`Show('wal_level')`, never the full
    // `SettingsProbe` projection) composes the same fold: each value lifts through the implicit
    // `string -> GucReading` so a `pending_restart`/`source`/`reset_val`-poor reading still grades against
    // primary-or-fallback. One semantic entrypoint, two ingress shapes — never a parallel verifier.
    public static Fin<GucVerdict> Verify(Seq<GucRow> rows, FrozenDictionary<string, string> observed) =>
        Verify(rows, observed.ToFrozenDictionary(static kv => kv.Key, static kv => (GucReading)kv.Value));

    // Independent fragments accumulate — every `Required` miss folds into ONE aggregated `Error.Many` so a
    // deployment with three forgotten GUCs surfaces all three in one verdict (the full repair set), never
    // one-per-redeploy as a `.Bind` short-circuit would. Degrade receipts (fallback, pending-restart, soft
    // miss) ride the verdict; the abort fires once at the end iff any hard miss accrued, with the staged
    // `reset_val` named on a pending-restart receipt. A `Required` reading clean or fallback-held passes.
    public static Fin<GucVerdict> Verify(Seq<GucRow> rows, FrozenDictionary<string, GucReading> observed) =>
        rows.Fold((Verdict: GucVerdict.Empty, Aborts: Seq<Error>.Empty), (acc, row) =>
            (observed.TryGetValue(row.Setting, out var reading) ? row.Grade(reading) : Some(GucDegrade.Unset))
                .Match(
                    None: () => (Verdict: acc.Verdict with { Matched = acc.Verdict.Matched.Add(row.Setting) }, Aborts: acc.Aborts),
                    Some: reason => row.Rank == GucRank.Required && reason.Aborts
                        ? (Verdict: acc.Verdict, Aborts: acc.Aborts.Add(Error.New($"<cluster-config-mismatch:{row.Setting}:{reason.Key}:{Seen(observed, row, reason)}:{row.Value}:{row.Restart.Key}>")))
                        : (Verdict: acc.Verdict with { Degraded = acc.Verdict.Degraded.Add(new GucReceipt(row.Setting, Seen(observed, row, reason), row.Value, row.Restart, reason)) }, Aborts: acc.Aborts)))
            is var s && s.Aborts.IsEmpty ? Fin.Succ(s.Verdict) : Fin.Fail<GucVerdict>(Error.Many(s.Aborts));

    // A pending-restart receipt names the staged `reset_val` the restart will apply, never the stale live value.
    static string Seen(FrozenDictionary<string, GucReading> observed, GucRow row, GucDegrade reason) =>
        observed.TryGetValue(row.Setting, out var reading)
            ? reason == GucDegrade.PendingRestart ? $"{reading.Value}->{reading.ResetValue}" : reading.Value
            : "<absent>";
}
```

## [04]-[RESEARCH]

- [BM25_QUERY_SHAPE]: the `pg_search` `0.24.x` v2 `pdb.*` query surface this axis projects, proven against the installed extension before the predicate fences pin — the `@@@ pdb.parse($1, lenient => true, conjunction_mode => …)` bound-parameter form, the bare `|||`/`&&&`/`===`/`###` column operators, the `@@@ pdb.match($1, distance => …, prefix => …, conjunction_mode => …)` analyzed builder (the inline tokenizer override riding the `::pdb.<tokenizer>` cast, not a `match` arg), the `@@@ pdb.exists()` field-presence form, the `a ## n ## b`/`a ##> n ##> b` proximity forms, the `::pdb.fuzzy`/`::pdb.boost`/`::pdb.const`/`::pdb.slop` cast-stacking order, the `pdb.range_term($1::int4range, 'Intersects')` positional-relation form, the `pdb.more_like_this($1)` key-field form (with the optional `$2::text[]` `fields` restriction) versus `pdb.more_like_this($1::json)` document form carrying the full `min_term_frequency`/`min_doc_frequency`/`max_doc_frequency`/`max_query_terms`/`min_word_length`/`max_word_length`/`boost_factor`/`stopwords` term-selection tuning (every arg `NULL`-defaultable, so only set knobs emit), the `pdb.boolean(must => ARRAY[…], should => ARRAY[…], must_not => ARRAY[…])` array-keyed composite, and the `pdb.score`/`pdb.snippet`/`pdb.snippets`/`pdb.snippet_positions`/`pdb.agg` projections at their default tags and `max_num_chars`; the removed `paradedb.*` namespace is asserted absent and the `$N` placeholders are confirmed to bind through `FromSql`/`SqlQuery` rather than interpolate.
- [CLUSTER_CONFIG_PORTABILITY]: the `pg_settings` round-trip the verify fold reads against the Forge-provisioned local PG18 — the `name`/`setting`/`source`/`reset_val`/`pending_restart`/`context` projection, whether `source = 'default'` is the authoritative `Unset` signal (a value an operator deliberately set equal to the compiled default reads `'configuration file'`, not `'default'`, so it grades `Below` rather than the `boot_val`-proxy false `Unset`), whether `io_method=io_uring` binds or falls back to `worker` with the `Postmaster` restart class, whether a numeric `GucKind.Integer`/`Real` floor (`effective_io_concurrency`, `work_mem`, `max_wal_size`, `checkpoint_completion_target`) compared against the bare `pg_settings.setting` integer in the setting's native unit admits an above-floor host without faulting, whether a `GucKind.List` superset check passes a `shared_preload_libraries`/`pgaudit.log` set carrying extra tokens, whether an `ALTER SYSTEM`-staged value surfaces through `pending_restart` as a `PendingRestart` degrade receipt carrying the staged `reset_val` rather than a green read, and whether each fragment's `pg_settings.context` matches the `GucContext` rung the row declares.
