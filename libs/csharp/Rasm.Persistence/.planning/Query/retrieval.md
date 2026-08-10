# [PERSISTENCE_QUERY_RETRIEVAL]

Rasm.Persistence owns the coupled ANN retrieval subsystem behind the `Query/lane#READ_ROUTING` `Retrieval` lane. One `Retrieval.Run(StoreProfile, RetrievalOp)` entry admits the vector lane and dispatches `Fuse`, `Train`, and `AdcScan`; every rejection rides `RetrievalFault`. `VectorRoute` couples residence to the settings that residence admits; `VectorFine` preserves recoverable `Float32` and `Int8Scalar` forms; `Bm25Predicate` owns lexical lowering; `FusionRank` applies one reciprocal-rank policy; `ElementSet.Receipt` and the operation content key jointly identify read-through reuse.

## [01]-[INDEX]

- [02]-[SEARCH_PROVISIONING_PROBE]: `EmbeddingArity`/`VectorMetric` store-binding axes, the `VectorRoute` residence cases, the server-side LINQ `ORDER BY` leg, and the `store.vector.route` fact.
- [03]-[LEXICAL_ALGEBRA]: `Bm25Predicate` typed builder/operator/cast union, the `SearchProjection` score/snippet surface, and the `LexicalRank` ts_rank fallback arm.
- [04]-[VECTOR_CODEBOOK]: `ProductCodebook` the codebook Compute encodes against, the per-subspace k-means training, the coarse→fine fine-form resolve, the amortized asymmetric-distance corpus scan, and the `RetrievalFault` band.
- [05]-[FUSION_AND_REUSE]: `RetrievalBranch` typed axis, the n-ary reciprocal-rank fusion with per-hit lineage, the receipt-keyed read-through reuse, and the one `RetrievalOp` entry behind the vector-lane admission.
- [06]-[DOCUMENT_CORPUS]: `DocumentCorpus` full-text index lane — the search-lane admission, its `CorpusKind` roster, index custody, the `DocumentPredicate` lowering, and the `DocumentQuery`/`DocumentHit` wire the app-shell search plane consumes.

## [02]-[SEARCH_PROVISIONING_PROBE]

- Owner: `EmbeddingArity` is the CLR-to-store vector arity axis; `VectorMetric` is the closed distance axis; `VectorRoute` is the closed residence union whose cases carry only their legal query-time settings; `SearchRoute` lowers a route to transaction-scoped settings.
- Cases: `VectorRoute` is `ExactScan | Hnsw | IvfFlat | DiskAnn | PqAdc | QdrantScaleout`; each indexed case carries only its own settings. `EmbeddingArity` is `Dense | Half | Sparse | Bit`; `VectorMetric` is `L2 | InnerProduct | Cosine | L1 | Hamming | Jaccard`.
- Entry: `SetLocal(VectorRoute)` derives only the settings admitted by the active route case, railing `RetrievalFault.Mismatched` on a `strict_order` request against the `IvfFlat` row (`ivfflat.iterative_scan` admits `off|relaxed_order` only) rather than silently demoting it; `VectorMetric.Order` admits the metric/arity pair before building the EF-translated distance expression with the arity-owned probe type; `ScaleoutRoute.Query` executes the external route.
- Auto: absent setting values emit no `SET LOCAL` row; the active `VectorRoute` case selects the only legal GUC vocabulary, and index construction remains owned by provisioning.
- Receipt: a routed vector scan rides `store.vector.route` carrying the `VectorRoute` case and bound GUC set.
- Packages: Pgvector.EntityFrameworkCore (`VectorDbFunctionsExtensions` six distance members), Pgvector (`Vector`), Qdrant.Client (`QdrantClient.QueryAsync`/`PrefetchQuery`/`Fusion`/`Formula`/`QuantizationConfig`/`ShardKey` — the scale-out row's provider surface), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Linq.Expressions`).
- Growth: a new residence is one `VectorRoute` case carrying its legal settings; a new arity or distance is one smart-enum row.
- Boundary: this owner is QUERY-TIME only — index DDL (`HasMethod("hnsw")`/`HasOperators`/`HasStorageParameter` on the EF builder, the `diskann`/`bm25` raw `MigrationBuilder.Sql` rows) belongs to `Element/identity#SCHEMA_VERDICT` and `Store/provisioning#SERVER_EXTENSIONS`, and the `DiskAnnOps` ops-class vocabulary stays the provisioning owner's — this page reads the same operator through `VectorMetric`, never a second ops-class enum; the exact brute-force scan is the correctness baseline every ANN claim measures against (recall@k vs exact, latency), so `ExactScan` is a first-class row, not an error path; `PqAdc` is the hot-set lane whose codebook, fine-form residence, and scan live in `#VECTOR_CODEBOOK` — corpus-scale ANN belongs to the pgvector/pgvectorscale indexes while the PQ/ADC row keeps the hot set, so the server-side LINQ row and the in-process row are complementary residences on one axis, neither redundant beside the other; `QdrantScaleout` is deployment DATA — the in-PG tier stays the default residence and the external store enters only where ANN cardinality or recall tuning exceeds what a pgvector HNSW index serves, its `VectorRow.ContentKey` identity staying the one content key so a hit resolves through the same fine-form residence regardless of which backend ranked it.

```csharp signature
using System.Buffers.Binary;
using System.Globalization;
using System.IO.Hashing;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics.Tensors;
using LanguageExt;
using Microsoft.Extensions.Caching.Hybrid;
using NodaTime;
using Pgvector.EntityFrameworkCore;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Thinktecture;
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Rasm.Persistence.Store;                     // StoreProfile — the lane-realizability axis both entries admit against
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// `EmbeddingArity` maps vector, half, sparse, and bit storage to their provider CLR probes.
// Bit arity represents the binary-quantized expression used by coarse Hamming search.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EmbeddingArity {
    public static readonly EmbeddingArity Dense = new("dense", "vector", DenseProbe);
    public static readonly EmbeddingArity Half = new("half", "halfvec", HalfProbe);
    public static readonly EmbeddingArity Sparse = new("sparse", "sparsevec", SparseProbe);
    public static readonly EmbeddingArity Bit = new("bit", "bit", BitProbe);
    public string StoreType { get; }
    public string Column(int n) => $"{StoreType}({n})";
    private EmbeddingArity(string key, string storeType, Func<float[], object> probe) : this(key) => (StoreType, Probes) = (storeType, probe);
    private Func<float[], object> Probes { get; }

    // `Probe` mints the provider CLR type required by each arity's distance member.
    // Sparse probes use the provider's canonical dense-to-sparse conversion.
    public object Probe(float[] probe) => Probes(probe);

    static object DenseProbe(float[] probe) => new Pgvector.Vector(probe);
    static object HalfProbe(float[] probe) => new Pgvector.HalfVector([.. probe.Select(static v => (Half)v)]);
    static object SparseProbe(float[] probe) => new Pgvector.SparseVector(new ReadOnlyMemory<float>(probe));
    static object BitProbe(float[] probe) => new System.Collections.BitArray([.. probe.Select(static v => v > 0f)]);
}

// `VectorMetric` closes the six provider distances over raw operators and translated methods.
// Numeric arities admit dense metrics; bit arity admits Hamming and Jaccard.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VectorMetric {
    public static readonly VectorMetric L2 = Numeric("l2", "<->", nameof(VectorDbFunctionsExtensions.L2Distance));
    public static readonly VectorMetric InnerProduct = Numeric("ip", "<#>", nameof(VectorDbFunctionsExtensions.MaxInnerProduct));
    public static readonly VectorMetric Cosine = Numeric("cosine", "<=>", nameof(VectorDbFunctionsExtensions.CosineDistance));
    public static readonly VectorMetric L1 = Numeric("l1", "<+>", nameof(VectorDbFunctionsExtensions.L1Distance));
    public static readonly VectorMetric Hamming = Binary("hamming", "<~>", nameof(VectorDbFunctionsExtensions.HammingDistance));
    public static readonly VectorMetric Jaccard = Binary("jaccard", "<%>", nameof(VectorDbFunctionsExtensions.JaccardDistance));
    public string Op { get; }
    public string Fn { get; }
    private Seq<EmbeddingArity> Arities { get; }
    private VectorMetric(string key, string op, string fn, Seq<EmbeddingArity> arities) : this(key) =>
        (Op, Fn, Arities) = (op, fn, arities);

    // `Order` builds the provider-translated server expression from admitted metric, arity, column, and probe.
    // Arity-specific constants prevent dense probes from crossing into half, sparse, or bit columns.
    public Fin<Expression> Order(Expression column, EmbeddingArity arity, float[] probe) =>
        Arities.Exists(candidate => candidate == arity)
            ? Fin.Succ<Expression>(Expression.Call(typeof(VectorDbFunctionsExtensions), Fn, Type.EmptyTypes, column,
                Expression.Constant(arity.Probe(probe))))
            : Fin.Fail<Expression>(new RetrievalFault.Mismatched("metric-arity", Key, arity.Key));

    private static VectorMetric Numeric(string key, string op, string fn) =>
        new(key, op, fn, [EmbeddingArity.Dense, EmbeddingArity.Half, EmbeddingArity.Sparse]);

    private static VectorMetric Binary(string key, string op, string fn) =>
        new(key, op, fn, [EmbeddingArity.Bit]);
}

// `ScanOrder` closes disabled, relaxed, and strict iterative-scan policy.
// HNSW admits every row; IVFFlat admits only `Off` and `RelaxedOrder`.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScanOrder {
    public static readonly ScanOrder Off = new("off");
    public static readonly ScanOrder RelaxedOrder = new("relaxed_order");
    public static readonly ScanOrder StrictOrder = new("strict_order");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VectorRoute {
    private VectorRoute() { }
    public sealed record ExactScan : VectorRoute;
    public sealed record Hnsw(Option<int> EfSearch, Option<ScanOrder> IterativeScan, Option<int> MaxScanTuples, Option<double> ScanMemMultiplier) : VectorRoute;
    public sealed record IvfFlat(Option<int> Probes, Option<int> MaxProbes, Option<ScanOrder> IterativeScan) : VectorRoute;
    public sealed record DiskAnn(Option<int> SearchListSize, Option<int> Rescore) : VectorRoute;
    public sealed record PqAdc : VectorRoute;
    public sealed record QdrantScaleout : VectorRoute;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class SearchRoute {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.vector.route"), StoreSlot.Create("store.vector.train"), StoreSlot.Create("store.vector.adc"),
        StoreSlot.Create("store.vector.resolve"), StoreSlot.Create("store.fusion.rank"), StoreSlot.Create("store.cache.hit"),
        StoreSlot.Create("store.cache.produce"));

    // `SetLocal` rails typed at derivation on a scan order the backing index cannot honor — silently demoting
    // `strict_order` to `relaxed_order` is the deleted coercion (the `Math.Max` clamp class).
    public static Fin<Seq<string>> SetLocal(VectorRoute route) => route.Switch(
        exactScan: static _ => Fin.Succ(Seq<string>()),
        hnsw: static row => Fin.Succ(Settings(
            row.EfSearch.Map(static value => ("hnsw.ef_search", Invariant(value))),
            row.IterativeScan.Map(static value => ("hnsw.iterative_scan", value.Key)),
            row.MaxScanTuples.Map(static value => ("hnsw.max_scan_tuples", Invariant(value))),
            row.ScanMemMultiplier.Map(static value => ("hnsw.scan_mem_multiplier", Invariant(value))))),
        ivfFlat: static row => row.IterativeScan.Exists(static order => order == ScanOrder.StrictOrder)
            ? Fin.Fail<Seq<string>>(new RetrievalFault.Mismatched("scan-order", $"{ScanOrder.Off.Key}|{ScanOrder.RelaxedOrder.Key}", ScanOrder.StrictOrder.Key))
            : Fin.Succ(Settings(
                row.Probes.Map(static value => ("ivfflat.probes", Invariant(value))),
                row.MaxProbes.Map(static value => ("ivfflat.max_probes", Invariant(value))),
                row.IterativeScan.Map(static value => ("ivfflat.iterative_scan", value.Key)))),
        diskAnn: static row => Fin.Succ(Settings(
            row.SearchListSize.Map(static value => ("diskann.query_search_list_size", Invariant(value))),
            row.Rescore.Map(static value => ("diskann.query_rescore", Invariant(value))))),
        pqAdc: static _ => Fin.Succ(Seq<string>()),
        qdrantScaleout: static _ => Fin.Succ(Seq<string>()));

    static Seq<string> Settings(params Option<(string Guc, string Value)>[] settings) =>
        toSeq(settings).Somes().Map(static setting => $"SET LOCAL {setting.Guc} = {setting.Value}");

    static string Invariant<T>(T value) where T : IFormattable => value.ToString(null, CultureInfo.InvariantCulture);
}

public static class ScaleoutRoute {
    // Qdrant scaleout executes hybrid prefetch fusion through one tenant-sharded `QueryAsync` call.
    // Payload content keys resolve the same `VectorRow` residence regardless of ranking backend.
    public static IO<Fin<Seq<(UInt128 ContentKey, float Score)>>> Query(
        QdrantClient client, Identifier collection, ReadOnlyMemory<float> probe, Seq<PrefetchQuery> prefetch, ulong tenant, RetrievalLimit top) =>
        IO.liftAsync(async () => {
            IReadOnlyList<ScoredPoint> hits = await client.QueryAsync(
                (string)collection, query: probe.ToArray(), prefetch: [.. prefetch], limit: (ulong)top.Value, shardKeySelector: tenant).ConfigureAwait(false);
            return Try.lift(() => toSeq(hits).Map(static hit =>
                    (UInt128.Parse(hit.Payload["content-key"].StringValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture), hit.Score)))
                .Run()
                .MapFail(static error => (Error)new RetrievalFault.Rejected(error.Message));
        }) | @catch<IO, Fin<Seq<(UInt128, float)>>>(static error => error.IsExceptional, static error => IO.pure(Fin<Seq<(UInt128, float)>>.Fail(new RetrievalFault.Rejected(error.Message))));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                   |
| :-----: | :------------------ | :------------------------------------- | :---------------------------------------------------------- |
|  [01]   | residence           | `VectorRoute` case                     | residence and legal settings share one discriminant         |
|  [02]   | scan tuning         | `SET LOCAL` GUC binder per row         | query-time only; the `WITH` build map stays provisioning's  |
|  [03]   | route observability | `store.vector.route` fact              | a degradation is evidence, never a silent slowdown          |
|  [04]   | server-side ANN     | `VectorMetric.Order` `Expression.Call` | probe constant per `EmbeddingArity.Probe`; never dense-only |
|  [05]   | scale-out ceiling   | `ScaleoutRoute.Query` → `QueryAsync`   | prefetch fused server-side; tenant `ShardKeySelector`       |

## [03]-[LEXICAL_ALGEBRA]

- Owner: `Bm25Predicate` the closed `[Union]` projecting the `pg_search` v2 `pdb` surface — one case per `pdb.*` builder, per bare match operator, and per stacking cast modifier — whose `Sql(column)` switch emits the exact server SQL; `SearchProjection` the static score/snippet/aggregate projection surface; `LexicalRank` the `[SmartEnum<string>]` two-row rank axis carrying the BM25 arm and the native `ts_rank` fallback arm.
- Cases: builders `Parse | Match | RangeTerm | PhrasePrefix | MoreLikeThis | Regex | All` (right of `@@@`), bare operators `AnyToken`(`|||`) `| AllToken`(`&&&`) `| ExactTerm`(`===`) `| Phrase`(`###`) `| Proximity`(`##`/`##>` — the `Ordered` field selects the operator token), cast modifiers `Fuzzy | Boost | Const | Slop` composing over ANY inner predicate and stacking in cast order; `LexicalRank` is `Bm25` (`pdb.score(<key_field>)` over a `bm25` index) and `TsRank` (`ts_rank` over the generated tsvector — the degrade arm a profile without `pg_search` preloaded selects).
- Entry: `public string Sql(Identifier column)` on `Bm25Predicate` switches the union to the exact match expression (`col @@@ pdb.parse(…)`, `col ||| '…'`, `col @@@ ('a' ##> 2 ##> 'b')`, `<inner>::pdb.fuzzy(…)`) — the column an admitted `#COLUMNAR_LANE` trust-gate `Identifier` and every string payload crossing the ONE `Lit` quote-doubling seam; `SearchProjection.Score(keyColumn)`/`Snippet`/`Snippets`/`SnippetPositions`/`Agg` emit the `[05]` projection functions anchored on the index `key_field`; `LexicalRank.Rank(keyColumn, terms)` emits the row's rank projection so the fusion CTE composes either arm through one call.
- Auto: the cast modifiers STACK in cast order (`'<term>'::pdb.fuzzy(2)::pdb.boost(2)` applies typo tolerance then a score multiplier) because each cast case wraps its `Inner` and appends its own cast — composition is structural, never string concatenation at the call site; analyzed matching has two spellings the union keeps distinct — the per-field `pdb.match` builder carrying its own fuzzy `distance`/`prefix` (the `Match` case) and the bare `|||`/`&&&` column operators (the `AnyToken`/`AllToken` cases); the BM25 branch matches `corpus @@@ pdb.parse($terms)` and orders by `pdb.score(<key_field>)` — the index's declared `key_field` anchor, the content key the fusion re-queries the row store by, so the fusion projects IDENTITIES rather than re-materializing candidate payloads; every projection rides `FromSql`/`SqlQuery` raw SQL because `bm25` carries no EF translator.
- Receipt: a lexical rank rides the `#FUSION_AND_REUSE` `store.fusion.rank` branch lineage — the `RetrievalBranch.Lexical` row names which arm ranked (its `Index` reads `bm25` or the tsvector GIN), so the degrade is visible in the fused result's lineage.
- Packages: `pg_search` (server-side — the `pdb` schema, `@@@`/`|||`/`&&&`/`===`/`###`/`##`/`##>` operators, `bm25` access method; AGPL confined to the PG server tier, never linked into managed code), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new `pdb` builder, operator, or cast is ONE union case in the `Sql` switch — the catalog's member set is the union's case roster, never a sibling method; a new rank arm is one `LexicalRank` row; zero new surface — a free-string BM25 predicate, a per-builder method family, or a prose-only degrade claim is the deleted form because the predicate is a closed typed tree and the fallback is a vocabulary row.
- Boundary: value transport is STRUCTURAL, never a prose contract — identifiers (column, `key_field` anchor) admit once through the `#COLUMNAR_LANE` `Identifier` trust gate and every free-text payload (term, tag, sort key, aggregate JSON) crosses the ONE `Lit` quote-doubling seam at the lowering, so a quote-bearing caller string is inert literal text by construction (pdb predicates are literals inside raw SQL the prepared-parameter surface does not reach — the same platform-forced escape seam the columnar `CREATE SECRET` rail names), and the `key_field` join anchor carries a `UNIQUE` constraint with exactly one `bm25` index per table; the index DDL lands via raw `MigrationBuilder.Sql` on the EF migration rail (`Element/identity#SCHEMA_VERDICT`) — this owner emits QUERY SQL only; the degrade is a CLOSED arm, not a fault — a profile without `pg_search` preloaded selects `LexicalRank.TsRank` inside the same fusion CTE (`websearch_to_tsquery` the only parser admitted to user text), so the fused result stays correct at reduced lexical power and the arm taken is branch-lineage evidence; `pdb.agg` is the Elasticsearch-style facet projection — an aggregate over the matched set, composed as a projection column, never a second aggregation engine beside the columnar lane.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// `Bm25Predicate` closes pg_search builders, operators, and casts over admitted columns.
// Every text payload crosses one literal quote-doubling seam.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Bm25Predicate {
    private Bm25Predicate() { }
    public sealed record Parse(string Query, bool Lenient, bool Conjunction) : Bm25Predicate;
    public sealed record Match(string Query, Option<int> Distance, bool Prefix, bool Conjunction) : Bm25Predicate;
    public sealed record RangeTerm(string Value, string Relation, string RangeType) : Bm25Predicate;
    public sealed record PhrasePrefix(Seq<string> Terms, Option<int> MaxExpansions) : Bm25Predicate;
    public sealed record MoreLikeThis(string DocId, Seq<string> Fields, Option<int> MaxQueryTerms) : Bm25Predicate;
    public sealed record Regex(string Pattern) : Bm25Predicate;
    public sealed record All : Bm25Predicate;
    public sealed record AnyToken(string Terms) : Bm25Predicate;
    public sealed record AllToken(string Terms) : Bm25Predicate;
    public sealed record ExactTerm(string Term) : Bm25Predicate;
    public sealed record Phrase(string Terms) : Bm25Predicate;
    public sealed record Proximity(string Left, int Within, string Right, bool Ordered) : Bm25Predicate;
    public sealed record Fuzzy(Bm25Predicate Inner, int Distance, bool Prefix, bool TranspositionCostOne) : Bm25Predicate;
    public sealed record Boost(Bm25Predicate Inner, double Factor) : Bm25Predicate;
    public sealed record Const(Bm25Predicate Inner, double Score) : Bm25Predicate;
    public sealed record Slop(Bm25Predicate Inner, int Distance) : Bm25Predicate;

    // One lowering composes builders, bare operators, proximity, and stacked casts structurally.
    // Admitted columns and quote-doubled literals prevent caller text from becoming SQL structure.
    public string Sql(Identifier column) => Switch(
        parse:        p => $"{column} @@@ pdb.parse('{Lit(p.Query)}', lenient => {Bool(p.Lenient)}, conjunction_mode => {Bool(p.Conjunction)})",
        match:        m => $"{column} @@@ pdb.match('{Lit(m.Query)}'{m.Distance.Map(static d => $", distance => {d}").IfNone(string.Empty)}, prefix => {Bool(m.Prefix)}, conjunction_mode => {Bool(m.Conjunction)})",
        rangeTerm:    r => $"{column} @@@ pdb.range_term('{Lit(r.Value)}', relation => '{Lit(r.Relation)}', range_type => '{Lit(r.RangeType)}')",
        phrasePrefix: p => $"{column} @@@ pdb.phrase_prefix(ARRAY[{string.Join(", ", p.Terms.Map(static t => $"'{Lit(t)}'"))}]{p.MaxExpansions.Map(static n => $", max_expansions => {n}").IfNone(string.Empty)})",
        moreLikeThis: m => $"{column} @@@ pdb.more_like_this('{Lit(m.DocId)}', fields => ARRAY[{string.Join(", ", m.Fields.Map(static f => $"'{Lit(f)}'"))}]{m.MaxQueryTerms.Map(static n => $", max_query_terms => {n}").IfNone(string.Empty)})",
        regex:        r => $"{column} @@@ pdb.regex('{Lit(r.Pattern)}')",
        all:          _ => $"{column} @@@ pdb.all()",
        anyToken:     a => $"{column} ||| '{Lit(a.Terms)}'",
        allToken:     a => $"{column} &&& '{Lit(a.Terms)}'",
        exactTerm:    e => $"{column} === '{Lit(e.Term)}'",
        phrase:       p => $"{column} ### '{Lit(p.Terms)}'",
        proximity:    p => $"{column} @@@ ('{Lit(p.Left)}' {(p.Ordered ? "##>" : "##")} {p.Within} {(p.Ordered ? "##>" : "##")} '{Lit(p.Right)}')",
        fuzzy:        f => $"{f.Inner.Sql(column)}::pdb.fuzzy({f.Distance}, {Bool(f.Prefix)}, {Bool(f.TranspositionCostOne)})",
        boost:        b => $"{b.Inner.Sql(column)}::pdb.boost({b.Factor.ToString(CultureInfo.InvariantCulture)})",
        @const:       c => $"{c.Inner.Sql(column)}::pdb.const({c.Score.ToString(CultureInfo.InvariantCulture)})",
        slop:         s => $"{s.Inner.Sql(column)}::pdb.slop({s.Distance})");

    static string Bool(bool value) => value ? "true" : "false";

    // `Lit` doubles single quotes where the provider's builder syntax cannot bind parameters.
    internal static string Lit(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}

// Search projections bind admitted columns and quote-doubled text to raw provider projection functions.
// Ranking, snippets, and aggregate metadata share the BM25 index key.
public static class SearchProjection {
    public static string Score(Identifier keyColumn) => $"pdb.score({keyColumn})";
    public static string Snippet(Identifier column, string startTag = "<b>", string endTag = "</b>", int maxChars = 150) =>
        $"pdb.snippet({column}, start_tag => '{Bm25Predicate.Lit(startTag)}', end_tag => '{Bm25Predicate.Lit(endTag)}', max_num_chars => {maxChars})";
    public static string Snippets(Identifier column, int limit, int offset, string sortBy = "score") =>
        $"pdb.snippets({column}, \"limit\" => {limit}, \"offset\" => {offset}, sort_by => '{Bm25Predicate.Lit(sortBy)}')";
    public static string SnippetPositions(Identifier column) => $"pdb.snippet_positions({column})";
    public static string Agg(string esJson) => $"pdb.agg('{Bm25Predicate.Lit(esJson)}') OVER ()";
}

// `LexicalRank.TsRank` keeps fusion correct when pg_search is absent and records the fallback in lineage.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LexicalRank {
    public static readonly LexicalRank Bm25 = new("bm25",
        static (key, _) => $"pdb.score({key})",
        static (column, terms) => new Bm25Predicate.Parse(terms, Lenient: true, Conjunction: false).Sql(column));
    public static readonly LexicalRank TsRank = new("ts_rank",
        static (_, terms) => $"ts_rank(lexemes, websearch_to_tsquery('english', '{Bm25Predicate.Lit(terms)}'))",
        static (_, terms) => $"lexemes @@ websearch_to_tsquery('english', '{Bm25Predicate.Lit(terms)}')");

    [UseDelegateFromConstructor] public partial string Score(Identifier keyColumn, string terms);

    // `Score` is PRIMARY and `Rank` DERIVES its order fragment from it, so an arm projects and orders by one
    // expression alone, and a caller needing the score as a column takes the same row.
    public string Rank(Identifier keyColumn, string terms) => $"{Score(keyColumn, terms)} DESC";

    [UseDelegateFromConstructor] public partial string MatchSql(Identifier column, string terms);
}
```

| [INDEX] | [POLICY]          | [VALUE]                            | [BINDING]                                                    |
| :-----: | :---------------- | :--------------------------------- | :----------------------------------------------------------- |
|  [01]   | lexical predicate | `Bm25Predicate` closed union       | one case per builder/operator/cast; `Sql()` the one lowering |
|  [02]   | cast stacking     | modifier cases wrap `Inner`        | `::pdb.fuzzy(...)::pdb.boost(...)` in cast order, structural |
|  [03]   | score anchor      | `pdb.score(<key_field>)`           | identities projected; payloads never re-materialized         |
|  [04]   | order derivation  | `Rank` derives from `Score`        | one expression projects and orders; arms cannot disagree     |
|  [05]   | degrade           | `LexicalRank.TsRank` row           | same CTE, `websearch_to_tsquery` only; visible in lineage    |
|  [06]   | escaping          | `Identifier` gate + one `Lit` seam | structural; a quote-bearing term is inert literal text       |

## [04]-[VECTOR_CODEBOOK]

- Owner: `ProductCodebook` the product-quantization codebook value-object the `Model/embedding#EMBEDDING` Compute lane encodes against (subspace count, the flat `[subspace][code][dim]` centroid grid, code width, and a content `Id`); `VectorRow` the content-keyed fine-form-plus-codes residence the rerank and the ADC scan read; `RetrievalFault` the closed `Expected`-band (8410) the codebook admission rejections rail; `VectorCodebook` the static surface owning the per-subspace k-means TRAINING and the amortized asymmetric-distance corpus scan; `VectorIndex` the composition-supplied port carrier owning the codebook supply, the coarse-survivor fine-form resolve, and the PQ-coded corpus read — the ANN index residence read by reference, never embedded.
- Cases: `VectorFine` is `Float32 | Int8Scalar`; the quantized case carries `Scale` and `ZeroPoint`, so decode reconstructs magnitude. `RetrievalFault.Mismatched` rejects incoherent ADC layouts, and `RetrievalLimit` admits a positive result bound.
- Entry: `Train` fits and content-keys the codebook; `AdcScan(..., RetrievalLimit)` admits query, codebook, row layout, and result bound before table access; `VectorIndex` supplies codebooks, fine forms, and coded rows through injected ports.
- Auto: `Train` rejects an empty/ragged corpus, a dimension not divisible by `subspaces`, and a corpus smaller than `codesPerSubspace` (which leaves trailing centroid slots untrained at zero) to the typed `RetrievalFault` rail, slices each corpus vector's subspace window, seeds the centroid grid from the first `codesPerSubspace` sub-vectors (deterministic first-k seeding, reproducible across retrains), and iterates assignment (nearest centroid by `TensorPrimitives.Distance`) and mean recompute (`TensorPrimitives.Add` accumulate, `TensorPrimitives.Divide` by the cluster count) — the SAME `TensorPrimitives.Distance` the Compute `EncodeProduct` assigns with, so train-time and encode-time partitions agree bit-for-bit — then snapshots centroid storage and mints the content `Id` over little-endian layout and finite centroid scalars through seed-zero `XxHash128`, collapsing signed zero so equal codebooks key identically across RIDs; `AdcScan` builds the `Subspaces × CodesPerSubspace` table by `TensorPrimitives.Distance` of each query sub-vector against every centroid ONCE, then folds each coded row to the sum of its per-subspace table lookups and keeps the nearest `top` through a bounded `PriorityQueue` heap keyed on ascending distance (never a full sort, never a per-row centroid-distance recompute — the table amortizes it); `Probe` projects the float32 fine bytes onto the `Pgvector.Vector` ANN column the HNSW/diskann index residence is built over.
- Receipt: a codebook train rides `store.vector.train` carrying the subspace and code counts and the content `Id`; an ADC scan rides `store.vector.adc` carrying the corpus cardinality and the top; a fine-form resolve rides `store.vector.resolve` carrying the survivor count; the candidate recall/latency is the upstream Compute embedding owner's measured concern, never re-emitted here.
- Packages: System.Numerics.Tensors (`TensorPrimitives.Distance`/`Add`/`Divide`), System.IO.Hashing (`XxHash128` streaming `Append`/`GetCurrentHashAsUInt128`, seed zero — the kernel growth-row streaming member for a preimage that outgrows a one-shot span), Pgvector (`Vector`), Rasm.Persistence (`Query/lane#ELEMENT_SET_ALGEBRA` `SetKey`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a retrained codebook mints a new `Id`; a fine encoding is one `VectorFine` case; a richer ANN residence is one `VectorRoute` case.
- Boundary: `ProductCodebook` is the ONE PQ vocabulary the seam shares — Compute imports it by its `Rasm.Persistence (project)` reference and does nearest-centroid encode and centroid-reconstruction decode over it but NEVER fits it, so defining it in Compute forces a `Persistence → Compute` cycle (the dependency runs `Compute → Persistence` only) and a Compute-side k-means is the named drift defect; training uses the SAME `TensorPrimitives.Distance` Compute assigns with so the partition a centroid grid induces at train time and at encode time is identical, and the codebook is supplied content-keyed so a re-train mints a fresh `Id` that re-keys every dependent `product-quantized` artifact (the `Model/embedding#EMBEDDING` content key folds the codebook `Id`); the two-stage retrieval is honest — the `binary-hamming` coarse gate (Compute) returns content keys, `Resolve` reads the survivors' `int8-scalar`/`float32` fine forms by content key, and the Compute `Rank` reranks over those fine forms, so the magnitude a 1-bit encoding discards is recovered from the stored fine residence and never faked from the ±1 decode; the amortized ADC scan is Persistence's because this lane owns the index traversal and the `#FUSION_AND_REUSE` recency-bounded reuse while the BOUNDED rerank over the resolved survivors is Compute's, so the query→centroid table is built once and reused across the whole corpus and a per-candidate centroid-distance recompute is the deleted form; the vector branch (the ADC or in-PG HNSW ranked rows mapped through `VectorRow.Subject` to model-qualified `SetKey`s) feeds `#FUSION_AND_REUSE` `FusionRank.Fuse` as one ranked branch, and the `Probe` `vector(N)` column is the same pgvector store type the `Element/identity#ELEMENT_IDENTITY` `Embedding` per-model locator rides (the corpus-grain retrieval index here, the per-model bounding-envelope locator there — two grains, never one duplicated index); the residence holds the typed `VectorFine` form and the optional `SetKey` only, no `EmbeddingVector`/`VectorEncoding`/`VectorScore` Compute type, so the strata dependency stays one-directional exactly as the `#FUSION_AND_REUSE` and `Query/cache#MODEL_RESULT_INDEX` owners keep it.

```csharp signature
// --- [ERRORS] -----------------------------------------------------------------------------
// `RetrievalFault` closes `FaultBand.Retrieval` over `Rasm.Domain.Expected`.
// Cases lift directly onto `Fin<T>` without bare error integers.
[Union]
public abstract partial record RetrievalFault : Expected, IValidationError<RetrievalFault> {
    private RetrievalFault() : base() { }
    public sealed record EmptyCorpus : RetrievalFault;
    public sealed record Layout(int Dimension, int Subspaces, int Codes) : RetrievalFault;
    public sealed record Ragged(int Expected, int Found) : RetrievalFault;
    public sealed record Undersized(int Corpus, int Codes) : RetrievalFault;
    public sealed record Rejected(string Detail) : RetrievalFault;
    public sealed record Mismatched(string Axis, string Expected, string Found) : RetrievalFault;

    public override int Code => FaultBand.Retrieval + Switch(
        emptyCorpus: static _ => 0,
        layout:      static _ => 1,
        ragged:      static _ => 2,
        undersized:  static _ => 3,
        rejected:    static _ => 4,
        mismatched:  static _ => 5);

    public override string Message => Switch(
        emptyCorpus: static _ => "<codebook-empty-corpus>",
        layout:      static c => $"<codebook-layout:{c.Dimension}/{c.Subspaces}@{c.Codes}>",
        ragged:      static c => $"<codebook-ragged:{c.Expected}!={c.Found}>",
        undersized:  static c => $"<codebook-undersized:{c.Corpus}<{c.Codes}>",
        rejected:    static c => $"<retrieval-rejected:{c.Detail}>",
        mismatched:  static c => $"<retrieval-mismatch:{c.Axis}:{c.Expected}!={c.Found}>");

    public override string Category => Switch(
        emptyCorpus: static _ => "EmptyCorpus",
        layout:      static _ => "Layout",
        ragged:      static _ => "Ragged",
        undersized:  static _ => "Undersized",
        rejected:    static _ => "Rejected",
        mismatched:  static _ => "Mismatched");

    // String-bearing generator text preserves the rendered message — a structured case
    // minted with zeroed fields would erase the one piece of evidence the generator hands over.
    public static RetrievalFault Create(string message) => new Rejected(message);
}

[ValueObject<int>]
[ValidationError<RetrievalFault>]
public readonly partial struct RetrievalLimit {
    static partial void ValidateFactoryArguments(ref RetrievalFault? validationError, ref int value) {
        if (value <= 0) { validationError = new RetrievalFault.Rejected($"<retrieval-limit:{value}>"); }
    }
}

[ValueObject<int>]
[ValidationError<RetrievalFault>]
public readonly partial struct TrainingPasses {
    static partial void ValidateFactoryArguments(ref RetrievalFault? validationError, ref int value) {
        if (value <= 0) { validationError = new RetrievalFault.Rejected($"<training-passes:{value}>"); }
    }
}

[ValueObject<float>]
[ValidationError<RetrievalFault>]
public readonly partial struct QuantizationScale {
    static partial void ValidateFactoryArguments(ref RetrievalFault? validationError, ref float value) {
        if (!float.IsFinite(value) || value <= 0) { validationError = new RetrievalFault.Rejected($"<quantization-scale:{value}>"); }
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record ProductCodebook {
    public int Subspaces { get; }
    public int SubspaceDim { get; }
    public int CodesPerSubspace { get; }
    public ReadOnlyMemory<float> Centroids { get; }
    public UInt128 Id { get; }
    public int Dimension => Subspaces * SubspaceDim;

    private ProductCodebook(int subspaces, int subspaceDim, int codesPerSubspace, ReadOnlyMemory<float> centroids, UInt128 id) =>
        (Subspaces, SubspaceDim, CodesPerSubspace, Centroids, Id) = (subspaces, subspaceDim, codesPerSubspace, centroids, id);

    public ReadOnlySpan<float> Centroid(int subspace, int code) =>
        Centroids.Span.Slice((subspace * CodesPerSubspace + code) * SubspaceDim, SubspaceDim);

    // Content `Id` streams little-endian layout and finite centroid scalars through seed-zero `XxHash128`.
    // Signed zero collapses before hashing, and `Of` snapshots caller memory before minting the carrier.
    public static UInt128 KeyOf(int subspaces, int subspaceDim, int codesPerSubspace, ReadOnlySpan<float> centroids) {
        XxHash128 hash = new();
        Span<byte> layout = stackalloc byte[12];
        BinaryPrimitives.WriteInt32LittleEndian(layout[..4], subspaces);
        BinaryPrimitives.WriteInt32LittleEndian(layout[4..8], subspaceDim);
        BinaryPrimitives.WriteInt32LittleEndian(layout[8..], codesPerSubspace);
        hash.Append(layout);
        Span<byte> scalar = stackalloc byte[sizeof(float)];
        foreach (float centroid in centroids) {
            BinaryPrimitives.WriteSingleLittleEndian(scalar, centroid == 0f ? 0f : centroid);
            hash.Append(scalar);
        }
        return hash.GetCurrentHashAsUInt128();
    }

    public static Fin<ProductCodebook> Of(int subspaces, int subspaceDim, int codesPerSubspace, ReadOnlyMemory<float> centroids) {
        long expected = (long)subspaces * subspaceDim * codesPerSubspace;
        if (subspaces <= 0 || subspaceDim <= 0 || codesPerSubspace is <= 0 or > 256) {
            return Fin.Fail<ProductCodebook>(new RetrievalFault.Layout(0, subspaces, codesPerSubspace));
        }
        if (expected != centroids.Length) {
            return Fin.Fail<ProductCodebook>(new RetrievalFault.Mismatched("centroids-length", expected.ToString(CultureInfo.InvariantCulture), centroids.Length.ToString(CultureInfo.InvariantCulture)));
        }
        if (!TensorPrimitives.IsFiniteAll(centroids.Span)) {
            return Fin.Fail<ProductCodebook>(new RetrievalFault.Rejected("<codebook-centroids-nonfinite>"));
        }
        float[] owned = centroids.ToArray();
        return Fin.Succ(new ProductCodebook(subspaces, subspaceDim, codesPerSubspace, owned,
            KeyOf(subspaces, subspaceDim, codesPerSubspace, owned)));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VectorFine {
    private VectorFine() { }
    public sealed record Float32(ReadOnlyMemory<float> Values) : VectorFine;
    public sealed record Int8Scalar(ReadOnlyMemory<sbyte> Values, QuantizationScale Scale, sbyte ZeroPoint) : VectorFine;

    public int Dimension => Switch(
        float32: static fine => fine.Values.Length,
        int8Scalar: static fine => fine.Values.Length);

    public float[] Decode() => Switch(
        float32: static fine => fine.Values.ToArray(),
        int8Scalar: static fine => toSeq(fine.Values.ToArray())
            .Map(value => (value - fine.ZeroPoint) * fine.Scale.Value)
            .ToArray());
}

public readonly record struct VectorRow(
    UInt128 ContentKey,
    VectorFine Fine,
    ReadOnlyMemory<byte> Codes,
    UInt128 CodebookId,
    Option<SetKey> Subject) {
    public int Dimension => Fine.Dimension;
    public Pgvector.Vector Probe() => new(Fine.Decode());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class VectorCodebook {
    public static Fin<ProductCodebook> Train(Seq<ReadOnlyMemory<float>> corpus, int subspaces, int codesPerSubspace, TrainingPasses passes) {
        if (corpus.IsEmpty) { return Fin.Fail<ProductCodebook>(new RetrievalFault.EmptyCorpus()); }
        int dimension = corpus[0].Length;
        if (subspaces <= 0 || dimension % subspaces != 0 || codesPerSubspace is <= 0 or > 256) {
            return Fin.Fail<ProductCodebook>(new RetrievalFault.Layout(dimension, subspaces, codesPerSubspace));
        }
        if (corpus.Exists(vector => vector.Length != dimension)) {
            return Fin.Fail<ProductCodebook>(new RetrievalFault.Ragged(dimension, corpus.Filter(v => v.Length != dimension).Head.Map(static v => v.Length).IfNone(0)));
        }
        if (corpus.Exists(static vector => !TensorPrimitives.IsFiniteAll(vector.Span))) {
            return Fin.Fail<ProductCodebook>(new RetrievalFault.Rejected("<codebook-corpus-nonfinite>"));
        }
        // Every centroid slot requires a corpus seed; undersized corpora rail before training.
        if (corpus.Count < codesPerSubspace) { return Fin.Fail<ProductCodebook>(new RetrievalFault.Undersized(corpus.Count, codesPerSubspace)); }
        int subDim = dimension / subspaces;
        float[] centroids = new float[subspaces * codesPerSubspace * subDim];
        for (int subspace = 0; subspace < subspaces; subspace++) {
            Lloyd(corpus, subspace, subDim, codesPerSubspace, passes.Value, centroids.AsSpan(subspace * codesPerSubspace * subDim, codesPerSubspace * subDim));
        }
        return ProductCodebook.Of(subspaces, subDim, codesPerSubspace, centroids);
    }

    // ADC admits query width, codebook identity, code count, and code range before table construction.
    // Malformed rows rail `Mismatched` rather than indexing another subspace's table slab.
    public static Fin<Seq<(UInt128 ContentKey, float Distance)>> AdcScan(ReadOnlyMemory<float> query, ProductCodebook codebook, Seq<VectorRow> coded, RetrievalLimit top) {
        if (query.Length != codebook.Dimension) {
            return Fin.Fail<Seq<(UInt128 ContentKey, float Distance)>>(new RetrievalFault.Mismatched("query-dim", codebook.Dimension.ToString(CultureInfo.InvariantCulture), query.Length.ToString(CultureInfo.InvariantCulture)));
        }
        if (!TensorPrimitives.IsFiniteAll(query.Span)) {
            return Fin.Fail<Seq<(UInt128 ContentKey, float Distance)>>(new RetrievalFault.Rejected("<adc-query-nonfinite>"));
        }
        if (coded.IsEmpty) { return Fin.Succ(Seq<(UInt128 ContentKey, float Distance)>()); }
        foreach (VectorRow row in coded) {
            if (row.CodebookId != codebook.Id) {
                return Fin.Fail<Seq<(UInt128 ContentKey, float Distance)>>(new RetrievalFault.Mismatched("codebook-id", codebook.Id.ToString("x32", CultureInfo.InvariantCulture), row.CodebookId.ToString("x32", CultureInfo.InvariantCulture)));
            }
            if (row.Codes.Length != codebook.Subspaces) {
                return Fin.Fail<Seq<(UInt128 ContentKey, float Distance)>>(new RetrievalFault.Mismatched("codes-length", codebook.Subspaces.ToString(CultureInfo.InvariantCulture), row.Codes.Length.ToString(CultureInfo.InvariantCulture)));
            }
        }
        float[] table = new float[codebook.Subspaces * codebook.CodesPerSubspace];
        for (int subspace = 0; subspace < codebook.Subspaces; subspace++) {
            ReadOnlySpan<float> part = query.Span.Slice(subspace * codebook.SubspaceDim, codebook.SubspaceDim);
            for (int code = 0; code < codebook.CodesPerSubspace; code++) {
                table[subspace * codebook.CodesPerSubspace + code] = TensorPrimitives.Distance(part, codebook.Centroid(subspace, code));
            }
        }
        PriorityQueue<(UInt128 ContentKey, float Distance), float> heap = new(top.Value);
        foreach (VectorRow row in coded) {
            ReadOnlySpan<byte> codes = row.Codes.Span;
            float distance = 0f;
            for (int subspace = 0; subspace < codes.Length; subspace++) {
                if (codes[subspace] >= codebook.CodesPerSubspace) {
                    return Fin.Fail<Seq<(UInt128 ContentKey, float Distance)>>(new RetrievalFault.Mismatched("code-range", codebook.CodesPerSubspace.ToString(CultureInfo.InvariantCulture), codes[subspace].ToString(CultureInfo.InvariantCulture)));
                }
                distance += table[subspace * codebook.CodesPerSubspace + codes[subspace]];
            }
            if (heap.Count < top.Value) { heap.Enqueue((row.ContentKey, distance), -distance); }
            else if (heap.TryPeek(out _, out float worst) && -distance > worst) { heap.EnqueueDequeue((row.ContentKey, distance), -distance); }
        }
        int kept = heap.Count;
        (UInt128 ContentKey, float Distance)[] ordered = new (UInt128 ContentKey, float Distance)[kept];
        for (int slot = kept - 1; slot >= 0; slot--) { ordered[slot] = heap.Dequeue(); }
        return Fin.Succ(toSeq(ordered));
    }

    // Lloyd iteration uses `TensorPrimitives` for distance and centroid arithmetic.
    // Loops remain the span-kernel exemption because no primitive owns centroid argmin assignment.
    static void Lloyd(Seq<ReadOnlyMemory<float>> corpus, int subspace, int subDim, int codes, int iterations, Span<float> centroids) {
        int offset = subspace * subDim;
        for (int code = 0; code < codes; code++) { corpus[code].Span.Slice(offset, subDim).CopyTo(centroids.Slice(code * subDim, subDim)); }
        float[] sums = new float[codes * subDim];
        int[] counts = new int[codes];
        for (int iteration = 0; iteration < iterations; iteration++) {
            Array.Clear(sums);
            Array.Clear(counts);
            foreach (ReadOnlyMemory<float> vector in corpus) {
                ReadOnlySpan<float> part = vector.Span.Slice(offset, subDim);
                (float Nearest, int Code) best = (float.PositiveInfinity, 0);
                for (int code = 0; code < codes; code++) {
                    float distance = TensorPrimitives.Distance(part, centroids.Slice(code * subDim, subDim));
                    if (distance < best.Nearest) { best = (distance, code); }
                }
                TensorPrimitives.Add(sums.AsSpan(best.Code * subDim, subDim), part, sums.AsSpan(best.Code * subDim, subDim));
                counts[best.Code]++;
            }
            for (int code = 0; code < codes; code++) {
                if (counts[code] > 0) { TensorPrimitives.Divide(sums.AsSpan(code * subDim, subDim), counts[code], centroids.Slice(code * subDim, subDim)); }
            }
        }
    }
}

// Bare `Func` port rows exactly as `SetResolve` carries them; a forwarding method per port is the deleted hop.
public sealed record VectorIndex(
    Func<UInt128, IO<Option<ProductCodebook>>> Codebook,
    Func<ProductCodebook, IO<Unit>> Publish,
    Func<Seq<UInt128>, IO<Seq<VectorRow>>> Resolve,
    Func<UInt128, RetrievalLimit, IO<Seq<VectorRow>>> Coded);
```

| [INDEX] | [POLICY]            | [VALUE]                                  | [BINDING]                                                 |
| :-----: | :------------------ | :--------------------------------------- | :-------------------------------------------------------- |
|  [01]   | codebook owner      | `Train` here; Compute encodes only       | a Compute-side fit forces a `Persistence → Compute` cycle |
|  [02]   | partition agreement | the SAME `TensorPrimitives.Distance`     | train-time and encode-time centroids agree bit-for-bit    |
|  [03]   | codebook supply     | content-keyed by `Id`, read by reference | a re-train re-keys every `product-quantized` artifact     |
|  [04]   | coarse→fine rerank  | `Resolve` reads `int8`/`float32` fine    | magnitude recovered from the residence, never faked       |
|  [05]   | ADC amortization    | one query→centroid table per scan        | reused across the corpus; never a per-row recompute       |
|  [06]   | admission rail      | `RetrievalFault` 841x                    | the pre-split bare `Error.New(8360..8363)` is dead        |
|  [07]   | strata one-way      | `VectorFine` + `SetKey` only             | no Compute type crosses down                              |

## [05]-[FUSION_AND_REUSE]

- Owner: `RetrievalBranch` the `[SmartEnum<string>]` typed branch axis carrying each branch's index identity; `FusionHit` carries each element's fused rank and typed contributions; `FusionRank` owns the n-ary reciprocal-rank fold with the `RrfConstant` policy; `ResultCache` keys the read-through `HybridCache` tier on both `ElementSet.Receipt` and the operation content key; `RetrievalOp` is the request `[Union]`, and `Retrieval` is the polymorphic dispatcher.
- Cases: `RetrievalOp` is `Fuse | Train | AdcScan`; `RetrievalResult` is `Fused | Trained | Scanned`; `RetrievalBranch` is `Vector | Spatial | Lexical`, each carrying its index identity.
- Entry: `Run` admits the vector lane against the `StoreProfile`, then dispatches the closed op family; `FusionRank.Fuse` applies the single `RrfConstant` policy and preserves typed lineage; `ResultCache.Cached` read-through-caches the derived retrieval under the subject receipt.
- Auto: fusion applies `Score(e) = Σ_b 1 / (RrfConstant + rank_b(e))` and preserves typed lineage. `VectorRoute` selects vector residence, and spatial and lexical branches retain their index identities.
- Receipt: a fusion rides `store.fusion.rank` carrying the branch count and the fused cardinality; a cache hit rides `store.cache.hit`, a miss `store.cache.produce`; the routed vector branch's backend rides the `#SEARCH_PROVISIONING_PROBE` `store.vector.route` fact.
- Packages: Microsoft.Extensions.Caching.Hybrid (`HybridCache.GetOrCreateAsync`/`HybridCacheEntryOptions`/`RemoveByTagAsync`), Rasm.Persistence (`Store/provisioning#SERVER_EXTENSIONS` `StoreProfile.Admits` — the lane-realizability axis), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new retrieval branch is one `RetrievalBranch` row carrying its index name and one ranked list into the same `Fuse` fold; a new retrieval modality is one `RetrievalOp` case whose arm breaks `Run` loudly at compile time; zero new surface — a per-branch-pair fusion, a bespoke score blend, a positional `branch-{b}` string lineage, a free-string cache tag, or a sibling `FuseMany`/`TrainAndScan` entrypoint is the deleted form because the RRF is one n-ary fold over the typed branch axis and the op union owns modality.
- Boundary: `Run` is the vector lane's ONE admission owner — an engine whose `StoreProfile` cannot realize the ANN residence refuses there with the axis named, so `Fuse`, `Train`, and `AdcScan` execute on a proven lane and no arm carries a second realizability test; the lexical residence is a SEPARATE roster row admitting at `#DOCUMENT_CORPUS`, so this page holds two lane gates because it serves two lanes, never one gate standing for both. This owner is the search-lane binding the pgvector/pg_search/pgvectorscale/qdrant `.api` catalogs compose against — a catalogue's `VectorMetric`/`EmbeddingArity`/`Bm25Predicate`/RRF reference resolves here, never a parallel saved-search owner; the fusion is the one n-ary RRF fold over the typed `RetrievalBranch` axis so a hit's lineage names the index that ranked it — a bespoke per-pair blend or a positional string branch label is the deleted form; the cache is the AppHost `HybridCache` port keyed on the content-addressed `ElementSet.Receipt` (minted by `Query/lane#ELEMENT_SET_ALGEBRA`, a lane-owned identity this page never re-derives) with a receipt-derived tag, so a free-string tag rejects at admission because it is uninvalidatable by construction, and this SELECTION-RESULT cache is a DIFFERENT owner from `Query/cache`'s compute-result reuse index (`ArtifactIndexRow`/`ModelResultIndex`) — the fusion result seam feeds cache's index rows, never merges with them; spatial→PG GiST and ANN→pgvector are the index owners (DuckDB spatial/vss being the columnar aggregator only, not the transactional index), so the fusion branches read the federated row's GiST/HNSW/tsvector columns and never duplicate the index, the vector branch resolving through the `#VECTOR_CODEBOOK` `VectorRow.Subject`-mapped ranked rows.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// TYPED retrieval-branch axis carries fusion identity as vocabulary, never a positional
// `branch-{b}` string. Each row names the index class; vector residence rides `VectorRoute`.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetrievalBranch {
    public static readonly RetrievalBranch Vector = new("vector", "pgvector-hnsw");
    public static readonly RetrievalBranch Spatial = new("spatial", "postgis-gist");
    public static readonly RetrievalBranch Lexical = new("lexical", "pg_search-bm25");
    public string Index { get; }
    private RetrievalBranch(string key, string index) : this(key) => Index = index;
}

public readonly record struct FusionHit(SetKey Key, double Score, Seq<(RetrievalBranch Branch, int Rank)> Lineage);

public readonly record struct RetrievalCachePolicy(Identifier Namespace, Duration TimeToLive);

// One retrieval request family carries fuse, train, and scan as cases on one entry, never
// sibling entrypoints — a new retrieval modality is one case that breaks `Retrieval.Run` at compile time.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetrievalOp {
    private RetrievalOp() { }
    public sealed record Fuse(Seq<(RetrievalBranch Branch, Seq<SetKey> Ranked)> Branches) : RetrievalOp;
    public sealed record Train(Seq<ReadOnlyMemory<float>> Corpus, int Subspaces, int CodesPerSubspace, TrainingPasses Passes) : RetrievalOp;
    public sealed record AdcScan(ReadOnlyMemory<float> Query, ProductCodebook Codebook, Seq<VectorRow> Coded, RetrievalLimit Top) : RetrievalOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetrievalResult {
    private RetrievalResult() { }
    public sealed record Fused(Seq<FusionHit> Hits) : RetrievalResult;
    public sealed record Trained(ProductCodebook Codebook) : RetrievalResult;
    public sealed record Scanned(Seq<(UInt128 ContentKey, float Distance)> Nearest) : RetrievalResult;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class FusionRank {
    // Canonical reciprocal-rank constant `k=60` is the single RRF damping policy value.
    public const int RrfConstant = 60;

    // Indexed `Seq.Map` yields the one-based rank used by the fixed reciprocal-rank policy.
    public static Seq<FusionHit> Fuse(Seq<(RetrievalBranch Branch, Seq<SetKey> Ranked)> branches) =>
        toSeq(branches
            .Bind(static b => b.Ranked.Map((key, index) => (Key: key, b.Branch, Rank: index + 1)))
            .GroupBy(static c => c.Key)
            .Select(group => new FusionHit(
                group.Key,
                group.Sum(static contribution => 1.0 / (RrfConstant + contribution.Rank)),
                toSeq(group.Select(static c => (c.Branch, c.Rank)))))
            .OrderByDescending(static h => h.Score));
}

public static class ResultCache {
    // Read-through reuse keys derived retrieval by subject receipt, operation key, and cache policy.
    // Receipt-derived tags support invalidation, and state-threaded factories preserve single-flight production.
    public static IO<T> Cached<TState, T>(ElementSet subject, UInt128 operationKey, RetrievalCachePolicy policy, TState state,
        Func<TState, CancellationToken, ValueTask<T>> produce, HybridCache cache) {
        string subjectKey = subject.Receipt.ToString("x32", CultureInfo.InvariantCulture);
        string operation = operationKey.ToString("x32", CultureInfo.InvariantCulture);
        return IO.liftAsync(() => cache.GetOrCreateAsync(
            $"{policy.Namespace}:{subjectKey}:{operation}",
            state,
            produce,
            new HybridCacheEntryOptions { Expiration = policy.TimeToLive.ToTimeSpan() },
            tags: [$"elementset:{subjectKey}"]).AsTask());
    }
}

// --- [COMPOSITION] --------------------------------------------------------------------------
public static class Retrieval {
    // `VectorLane` is the token `StoreProfile.Lanes` spells for the ANN residence this entry serves.
    public const string VectorLane = "vector";

    // ONE entry per MODAL_ARITY: the op case is the discriminant, the generated Switch total — train and scan
    // rail the RetrievalFault band at their layout admissions, fuse is total over admitted values. The lane
    // admits ONCE here, so an engine that cannot realize the vector residence refuses at the entry rather than at
    // its first ANN probe, and no op arm carries a second realizability test.
    public static Fin<RetrievalResult> Run(StoreProfile store, RetrievalOp op) =>
        !store.Admits(VectorLane)
        ? Fin.Fail<RetrievalResult>(new RetrievalFault.Mismatched("store-lane", VectorLane, store.Key))
        : op.Switch(
            fuse:    static f => Fin.Succ<RetrievalResult>(new RetrievalResult.Fused(FusionRank.Fuse(f.Branches))),
            train:   static t => VectorCodebook.Train(t.Corpus, t.Subspaces, t.CodesPerSubspace, t.Passes)
                                     .Map(static trained => (RetrievalResult)new RetrievalResult.Trained(trained)),
            adcScan: static s => VectorCodebook.AdcScan(s.Query, s.Codebook, s.Coded, s.Top)
                                     .Map(static nearest => (RetrievalResult)new RetrievalResult.Scanned(nearest)));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                  | [BINDING]                                                         |
| :-----: | :----------------- | :--------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | entry              | `Run(StoreProfile, RetrievalOp)`         | fuse/train/scan are cases; no sibling entrypoint family           |
|  [02]   | fusion             | n-ary RRF over `RetrievalBranch`         | typed branch lineage; `RrfConstant = 60` named once               |
|  [03]   | cache key          | content-addressed `ElementSet.Receipt`   | cached under the SUBJECT receipt, not its own — the circular form |
|  [04]   | cache invalidation | receipt-derived tag → `RemoveByTagAsync` | a changefeed op-log change to a contributing node cuts it         |
|  [05]   | cache identity     | selection-result reuse                   | distinct from `Query/cache`'s compute-result index                |
|  [06]   | index ownership    | GiST spatial, pgvector ANN, BM25 lexical | DuckDB is the columnar aggregator, never the index                |
|  [07]   | lane admission     | `Admits(VectorLane)` inside `Run`        | refused once, axis named; `search` gates at #DOCUMENT_CORPUS      |

## [06]-[DOCUMENT_CORPUS]

- Owner: `CorpusKind` is the closed document-source roster whose keys ARE the wire tokens the consuming shell spells; `CorpusRow` is the indexed row the lane admits and the projection returns identity from; `DocumentPredicate` is the closed token-to-`Bm25Predicate` lowering; `DocumentQuery`/`DocumentHit` are the consumed query and answer wire; `DocumentCorpus` owns admission, statement composition, and hit shaping.
- Cases: `CorpusKind` is `Cell | Prose | Issue | Node | Evidence`, each row carrying its coverage columns — the subject, member, and body semantics its documents fill; `DocumentPredicate` is `Match | Phrase | PhrasePrefix | Regex`, one row per grammar the consumer's own vocabulary closes.
- Entry: `public static Fin<string> Statement(StoreProfile store, DocumentQuery query, LexicalRank rank)` admits the search lane and then the wire ONCE — non-blank terms, a non-empty scope of admitted `CorpusKind` keys, a bounded limit — then lowers the predicate token, the scope filter, the subject narrowing, and the rank arm's own score and order fragments into one statement; `public static Fin<DocumentHit> Shape(...)` folds one projected row into the answer wire.
- Auto: this corpus is a `RetrievalBranch.Lexical` residence, so a document search is a first-class branch a `#FUSION_AND_REUSE` `Fuse` can take beside the vector and spatial branches without a second ranked-list shape. Each predicate token selects its `Bm25Predicate` case and the whole-word column selects the exact-term operator inside the `Match` row alone — a phrase already bounds its tokens, a prefix contradicts a boundary by construction, and a pattern carries its own. Case sensitivity is NOT an index property: the `bm25` analyzer case-folds at build, so the lane narrows case-insensitively and gates the matched set with a positional containment test before ranking. Snippet, positions, and score all project through `#LEXICAL_ALGEBRA` — `SearchProjection.Snippet`, `SearchProjection.SnippetPositions`, and the rank row's own `Score` — so the degrade arm answers the same column set at reduced lexical power.
- Receipt: a document search rides the `#FUSION_AND_REUSE` `store.fusion.rank` branch lineage under `RetrievalBranch.Lexical`, so the arm that ranked it is visible without a second fact.
- Packages: `pg_search` (server-side — the `pdb` schema, the `bm25` access method), Rasm.Persistence (`Store/provisioning#SERVER_EXTENSIONS` `StoreProfile.Admits` — the lane-realizability axis), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new document source is one `CorpusKind` row carrying its coverage columns, and the consumer's source roster gains the matching key; a new grammar is one `DocumentPredicate` row lowering to an existing `Bm25Predicate` case; zero new surface and no second corpus relation.
- Boundary: `Statement` is the search lane's ONE admission owner — an engine whose `StoreProfile` cannot realize the lexical residence carries neither a `bm25` index nor a generated `lexemes` column, so the degrade arm is unreachable too and the refusal names the axis at the entry rather than surfacing as a failed statement; the vector lane is a SEPARATE roster row admitting at `#FUSION_AND_REUSE`. INDEX CUSTODY IS THIS OWNER'S and the consuming shell holds none — the corpus relation, its `bm25` index with `content_key` as the declared `key_field`, its generated `lexemes` tsvector for the degrade arm, and the ingest that lands rows from the durable owners all live here, so a shell-side index is the deleted form. Index DDL still lands via raw `MigrationBuilder.Sql` on the EF migration rail (`Element/identity#SCHEMA_VERDICT`) exactly as `#LEXICAL_ALGEBRA` rules — this section emits QUERY SQL only. `DocumentQuery` and `DocumentHit` are the WHOLE contract with `csharp:Rasm.AppUi/Document/search#INDEX_WIRE`, and that plane composes these declarations directly through its legal package reference — a member-for-member re-spelled record at the consumer is the deleted twin; the grammar crosses as a predicate token rather than a second vocabulary, the scope crosses as `CorpusKind` keys, and the limit ceiling admits at both ends against the one `LimitCeiling`. `Shape` returns IDENTITIES, snippet text, positions, and a score alone — the body a hit matched never re-crosses, because the row store already holds it and a returned payload forks residence. `DocumentHit` carries no rank-arm column: which branch ranked a hit is this lane's own receipt lineage, and a wire copy is a column the consumer never reads. Every identifier admits through the `#COLUMNAR_LANE` `Identifier` trust gate and every free-text payload crosses the one `Bm25Predicate.Lit` seam, so a quote-bearing term is inert literal text by construction.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// `CorpusKind` rosters the corpus: keys are the wire tokens a scope filter spells, and the coverage columns are
// row DATA naming what fills the indexed attribution triple for that kind — so admission, ingest, and the consumer's
// result decode all read one roster and an uncovered source is a missing ROW, never a silent refusal.
// `Member` is empty on a kind whose documents carry no sub-anchor; `Keyed` derives the presence test.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CorpusKind {
    public static readonly CorpusKind Cell = new("cell", subject: "notebook document id", member: "cell id", body: "cell source text");
    public static readonly CorpusKind Prose = new("prose", subject: "prose document id", member: "", body: "raw markdown source");
    public static readonly CorpusKind Issue = new("issue", subject: "issue topic id", member: "comment id", body: "issue and comment text");
    public static readonly CorpusKind Node = new("node", subject: "graph document id", member: "node key", body: "node title text");
    public static readonly CorpusKind Evidence = new("evidence", subject: "correlation id", member: "receipt kind", body: "receipt payload text");

    public string Subject { get; }
    public string Member { get; }
    public string Body { get; }
    public bool Keyed => Member.Length > 0;
    private CorpusKind(string key, string subject, string member, string body) : this(key) => (Subject, Member, Body) = (subject, member, body);
}

// One wire token lowers to ONE `Bm25Predicate` case. The whole-word column is read by the `Match` row
// alone — the exact-term operator IS word-boundary matching — because a phrase already bounds its tokens,
// a prefix contradicts a boundary by construction, and a pattern carries its own.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DocumentPredicate {
    public static readonly DocumentPredicate Match = new("match",
        static (terms, whole) => whole
            ? (Bm25Predicate)new Bm25Predicate.ExactTerm(terms)
            : new Bm25Predicate.Match(terms, Distance: None, Prefix: false, Conjunction: true));
    public static readonly DocumentPredicate Phrase = new("phrase",
        static (terms, _) => new Bm25Predicate.Phrase(terms));
    public static readonly DocumentPredicate PhrasePrefix = new("phrase-prefix",
        static (terms, _) => new Bm25Predicate.PhrasePrefix(
            toSeq(terms.Split(' ', StringSplitOptions.RemoveEmptyEntries)), MaxExpansions: None));
    public static readonly DocumentPredicate Regex = new("regex",
        static (terms, _) => new Bm25Predicate.Regex(terms));

    [UseDelegateFromConstructor] public partial Bm25Predicate Lower(string terms, bool wholeWords);
}

// --- [MODELS] -----------------------------------------------------------------------------
// `CorpusRow` is the indexed row. `ContentKey` is the declared `key_field` the score anchors on and the identity the
// projection returns, so a hit resolves to its owner with no payload re-materialization; `Kind`, `Subject`,
// and `Member` are the consumer's own attribution triple stored verbatim, so a hit needs no second lookup.
public readonly record struct CorpusRow(
    UInt128 ContentKey, CorpusKind Kind, string Subject, Option<string> Member, string Title, string Body);

// `DocumentQuery`/`DocumentHit` carry the query and answer wire. `csharp:Rasm.AppUi/Document/search#INDEX_WIRE`
// composes THESE declarations directly — one declaration serves both ends, so the shape cannot fork.
public sealed record DocumentQuery(
    string Terms,
    string Predicate,
    Seq<string> Sources,
    Option<string> Subject,
    int Limit,
    bool CaseSensitive,
    bool WholeWords);

// Spans cross as offset and LENGTH because the consumer's span type carries an inclusive end, so a raw end
// field leaves the two sides disagreeing by one character on every hit.
public sealed record DocumentHit(
    string Source,
    string Subject,
    Option<string> Member,
    string Title,
    int SpanStart,
    int SpanLength,
    string Snippet,
    double Score);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class DocumentCorpus {
    // `LimitCeiling` agrees with the consumer's own admitted ceiling, so neither end accepts what the other
    // refuses and a wire that drifted names itself at admission.
    public const int LimitCeiling = 1000;

    // `Relation` and its two addressed columns are the ONLY identifiers this lane names, each admitting
    // through the trust gate, so no free string reaches the statement as structure.
    public static readonly Identifier Relation = Identifier.Create("document_corpus");
    public static readonly Identifier KeyField = Identifier.Create("content_key");
    public static readonly Identifier BodyColumn = Identifier.Create("body");

    // `SearchLane` is the token `StoreProfile.Lanes` spells for the lexical residence this corpus lives in — a
    // DIFFERENT lane from the ANN residence `Retrieval.Run` admits, so this page serves two roster rows.
    public const string SearchLane = "search";

    // ONE lowering per query: the lane admits FIRST — an engine that cannot realize the lexical residence has no
    // `bm25` index and no `lexemes` column, so the degrade arm is unreachable too and the refusal belongs here
    // rather than at a failed statement. Then the query admission, the predicate case, the scope filter, the
    // optional subject narrowing, the case-sensitivity gate the analyzer cannot carry, and the rank row's own
    // score and order fragments — so the BM25 arm and the tsvector degrade arm compose the same statement shape.
    public static Fin<string> Statement(StoreProfile store, DocumentQuery query, LexicalRank rank) =>
        !store.Admits(SearchLane)
        ? Fin.Fail<string>(new RetrievalFault.Mismatched("store-lane", SearchLane, store.Key))
        : (from admitted in Admit(query)
           from predicate in (DocumentPredicate.TryGet(admitted.Predicate, out DocumentPredicate? row)
                   ? Optional(row)
                   : Option<DocumentPredicate>.None)
               .ToFin(new RetrievalFault.Mismatched(
                   "document-predicate", string.Join("|", DocumentPredicate.Items.Select(static p => p.Key)), admitted.Predicate))
           select Composed(admitted, predicate, rank));

    // `Shape` takes positions the index answers for a WHOLE document, so the wire's span pair is the first
    // match's; the consumer keys its cache on anchor-plus-offset, so this hit and its own scan of the same
    // document at the same offset collapse to one row while that document's later offsets stand beside it.
    public static Fin<DocumentHit> Shape(
        CorpusKind kind, string subject, Option<string> member, string title, string snippet,
        Seq<(int Start, int Length)> positions, double score) =>
        positions.Head
            .ToFin(new RetrievalFault.Mismatched("snippet-positions", "at least one match position", "none"))
            .Map(first => new DocumentHit(kind.Key, subject, member, title, first.Start, first.Length, snippet, score));

    // Case sensitivity is NOT an index property — the analyzer case-folds at build — so the lane narrows
    // case-insensitively through the index and gates the matched set with a positional containment test.
    static string Composed(DocumentQuery query, DocumentPredicate predicate, LexicalRank rank) =>
        $"""
         SELECT kind AS "Source", subject AS "Subject", member AS "Member", title AS "Title",
                {SearchProjection.Snippet(BodyColumn)} AS "Snippet",
                {SearchProjection.SnippetPositions(BodyColumn)} AS "Positions",
                {rank.Score(KeyField, query.Terms)} AS "Score"
         FROM {Relation}
         WHERE {predicate.Lower(query.Terms, query.WholeWords).Sql(BodyColumn)}
           AND kind = ANY(ARRAY[{string.Join(", ", query.Sources.Map(static source => $"'{Bm25Predicate.Lit(source)}'"))}])
         {query.Subject.Map(static subject => $"  AND subject = '{Bm25Predicate.Lit(subject)}'").IfNone(string.Empty)}
         {(query.CaseSensitive ? $"  AND position('{Bm25Predicate.Lit(query.Terms)}' in {BodyColumn}) > 0" : string.Empty)}
         ORDER BY {rank.Rank(KeyField, query.Terms)}
         LIMIT {query.Limit}
         """;

    // `Admit` is the ONE gate: terms present, every scoped key an admitted corpus row, the scope non-empty,
    // and the limit inside the shared ceiling — a refusal names the wire that drifted, never the user's query.
    static Fin<DocumentQuery> Admit(DocumentQuery query) =>
        string.IsNullOrWhiteSpace(query.Terms)
            ? Fin.Fail<DocumentQuery>(new RetrievalFault.Mismatched("document-terms", "non-blank terms", "blank"))
            : query.Sources.Find(static source => !CorpusKind.TryGet(source, out _)).Match(
                Some: unknown => Fin.Fail<DocumentQuery>(new RetrievalFault.Mismatched(
                    "corpus-kind", string.Join("|", CorpusKind.Items.Select(static kind => kind.Key)), unknown)),
                None: () => query.Sources.IsEmpty || query.Limit <= 0 || query.Limit > LimitCeiling
                    ? Fin.Fail<DocumentQuery>(new RetrievalFault.Mismatched(
                        "document-scope", $"1..{LimitCeiling} results over at least one corpus kind",
                        $"{query.Sources.Count}:{query.Limit}"))
                    : Fin.Succ(query));
}
```

| [INDEX] | [POLICY]         | [VALUE]                                | [BINDING]                                                          |
| :-----: | :--------------- | :------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | index custody    | this lane's corpus relation and index  | consumer holds none; a shell-side index is the deleted form        |
|  [02]   | index DDL        | raw `MigrationBuilder.Sql`             | `Element/identity#SCHEMA_VERDICT`; this section emits query SQL    |
|  [03]   | branch identity  | `RetrievalBranch.Lexical`              | a document search fuses beside vector and spatial, one shape       |
|  [04]   | grammar crossing | `DocumentPredicate` token              | one token per `Bm25Predicate` case; no second vocabulary           |
|  [05]   | word boundary    | exact-term operator inside `Match`     | the other three rows bound their tokens by construction            |
|  [06]   | case sensitivity | positional containment gate            | the `bm25` analyzer case-folds at build; the index cannot carry it |
|  [07]   | wire projection  | identities, snippet, positions, score  | the matched body never re-crosses; residence stays unforked        |
|  [08]   | rank arm         | receipt lineage, not a wire column     | a copy on the wire is a column the consumer never reads            |
|  [09]   | limit ceiling    | `LimitCeiling` here; consumer reads it | neither end accepts what the other refuses                         |
|  [10]   | lane admission   | `Admits(SearchLane)` in `Statement`    | the degrade arm needs the lane too; refused once, axis named       |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
