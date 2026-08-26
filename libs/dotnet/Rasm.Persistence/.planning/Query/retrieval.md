# [PERSISTENCE_QUERY_RETRIEVAL]

Rasm.Persistence owns the coupled ANN retrieval subsystem behind the `Query/lane#READ_ROUTING` `Retrieval` lane. One `Retrieval.Run(StoreProfile, RetrievalOp)` entry admits the vector lane and dispatches `Fuse`, `Train`, and `AdcScan`; every rejection rides `RetrievalFault`. `VectorRoute` couples the backend to the settings that backend admits; `VectorFine` preserves recoverable `Float32` and `Int8Scalar` forms; `Bm25Predicate` owns lexical lowering; `FusionRank` applies one reciprocal-rank policy; `KeySelection.ContentKey` and the operation content key jointly identify read-through reuse.

## [01]-[INDEX]

- [02]-[SEARCH_PROVISIONING_PROBE]: `EmbeddingArity`/`VectorMetric` store-binding axes, the `VectorRoute` backend cases, the server-side LINQ `ORDER BY` leg, and the `store.vector.route` fact.
- [03]-[LEXICAL_ALGEBRA]: `Bm25Predicate` typed builder/operator/cast union, the `SearchProjection` score/snippet surface, and the `LexicalRank` ts_rank fallback arm.
- [04]-[VECTOR_CODEBOOK]: `ProductCodebook` the codebook Compute encodes against, the per-subspace k-means training, the coarse→fine fine-form resolve, the amortized asymmetric-distance corpus scan, and the `RetrievalFault` band.
- [05]-[FUSION_AND_REUSE]: `RetrievalBranch` typed axis, the n-ary reciprocal-rank fusion with per-hit lineage, content-keyed read-through reuse, and the one `RetrievalOp` entry behind the vector-lane admission.
- [06]-[DOCUMENT_CORPUS]: `DocumentCorpus` full-text index lane — the search-lane admission, its `CorpusKind` roster, index custody, the `DocumentPredicate` lowering, and the `DocumentQuery`/`DocumentHit` wire the app-shell search plane consumes.

## [02]-[SEARCH_PROVISIONING_PROBE]

- Owner: `EmbeddingArity` is the CLR-to-store vector arity axis, realizing the kernel `ICapability` floor so a metric's admitted arities are one `CapabilitySet` carrying its own `Missing` evidence; `VectorMetric` is the closed distance axis; `VectorRoute` is the closed backend union whose cases carry only their legal query-time settings; `SearchRoute` lowers a route to transaction-scoped settings.
- Cases: `VectorRoute` is `ExactScan | Hnsw | IvfFlat | DiskAnn | PqAdc | QdrantScaleout`; each indexed case carries only its own settings. `EmbeddingArity` is `Dense | Half | Sparse | Bit`; `VectorMetric` is `L2 | InnerProduct | Cosine | L1 | Hamming | Jaccard`.
- Entry: `SetLocal(VectorRoute)` derives only the settings admitted by the active route case, refusing `RetrievalFault.Mismatched` on a `strict_order` request against the `IvfFlat` row (`ivfflat.iterative_scan` admits `off|relaxed_order` only) rather than silently demoting it; `VectorMetric.Order` admits the metric/arity pair through the ONE `CapabilitySet.Require` refusal door — whose refuse arm receives the missing set, so the fault names the arity that failed rather than restating the pair — before building the EF-translated distance expression with the arity-owned probe type; `ScaleoutRoute.Query` executes the external route.
- Auto: absent setting values emit no `SET LOCAL` row; the active `VectorRoute` case selects the only legal GUC vocabulary, and index construction remains owned by provisioning.
- Packages: Pgvector.EntityFrameworkCore (`VectorDbFunctionsExtensions` six distance members), Pgvector (`Vector`), Qdrant.Client (`QdrantClient.QueryAsync`/`PrefetchQuery`/`Fusion`/`Formula`/`QuantizationConfig`/`ShardKey` — the scale-out row's provider surface), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Linq.Expressions`).
- Growth: a new backend is one `VectorRoute` case carrying its legal settings; a new arity or distance is one smart-enum row.
- Boundary: this owner is QUERY-TIME only — index DDL (`HasMethod("hnsw")`/`HasOperators`/`HasStorageParameter` on the EF builder, the `diskann`/`bm25` raw generation-DDL rows) belongs to `Element/identity#SCHEMA_VERDICT` and `Store/provisioning#SERVER_EXTENSIONS`, and the `DiskAnnOps` ops-class vocabulary stays the provisioning owner's — this page reads the same operator through `VectorMetric`, never a second ops-class enum; the exact brute-force scan is the correctness baseline every ANN claim measures against (recall@k vs exact, latency), so `ExactScan` is a first-class row, not an error path; `PqAdc` is the hot-set lane whose codebook, fine-form storage, and scan live in `#VECTOR_CODEBOOK` — corpus-scale ANN belongs to the pgvector/pgvectorscale indexes while the PQ/ADC row keeps the hot set, so the server-side LINQ row and the in-process row are complementary backends on one axis, neither redundant beside the other; `QdrantScaleout` is deployment DATA — the in-PG tier stays the default backend and the external store enters only where ANN cardinality or recall tuning exceeds what a pgvector HNSW index serves, its `VectorRow.ContentKey` identity staying the one content key so a hit resolves through the same fine-form storage regardless of which backend ranked it.

```csharp
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
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Thinktecture;
using Rasm.Persistence.Element;
using Rasm.Persistence.Store;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EmbeddingArity : ICapability<EmbeddingArity> {
    public static readonly EmbeddingArity Dense = new("dense", "vector", DenseProbe);
    public static readonly EmbeddingArity Half = new("half", "halfvec", HalfProbe);
    public static readonly EmbeddingArity Sparse = new("sparse", "sparsevec", SparseProbe);
    public static readonly EmbeddingArity Bit = new("bit", "bit", BitProbe);
    public string StoreType { get; }
    public string Column(int n) => $"{StoreType}({n})";
    private EmbeddingArity(string key, string storeType, Func<float[], object> probe) : this(key) => (StoreType, Probes) = (storeType, probe);
    private Func<float[], object> Probes { get; }

    public object Probe(float[] probe) => Probes(probe);

    static object DenseProbe(float[] probe) => new Pgvector.Vector(probe);
    static object HalfProbe(float[] probe) => new Pgvector.HalfVector([.. probe.Select(static v => (Half)v)]);
    static object SparseProbe(float[] probe) => new Pgvector.SparseVector(new ReadOnlyMemory<float>(probe));
    static object BitProbe(float[] probe) => new System.Collections.BitArray([.. probe.Select(static v => v > 0f)]);
}

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
    private CapabilitySet<EmbeddingArity> Arities { get; }
    private VectorMetric(string key, string op, string fn, CapabilitySet<EmbeddingArity> arities) : this(key) =>
        (Op, Fn, Arities) = (op, fn, arities);

    public Fin<Expression> Order(Expression column, EmbeddingArity arity, float[] probe) =>
        Arities.Require(CapabilitySet<EmbeddingArity>.Of(arity),
                missing => new RetrievalFault.Mismatched("metric-arity", Key, missing.Wire))
            .Map(_ => (Expression)Expression.Call(typeof(VectorDbFunctionsExtensions), Fn, Type.EmptyTypes, column,
                Expression.Constant(arity.Probe(probe))));

    private static VectorMetric Numeric(string key, string op, string fn) =>
        new(key, op, fn, CapabilitySet<EmbeddingArity>.Of(EmbeddingArity.Dense, EmbeddingArity.Half, EmbeddingArity.Sparse));

    private static VectorMetric Binary(string key, string op, string fn) =>
        new(key, op, fn, CapabilitySet<EmbeddingArity>.Of(EmbeddingArity.Bit));
}

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SearchRoute {
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
    public static IO<Fin<Seq<(UInt128 ContentKey, float Score)>>> Query(
        QdrantClient client, Identifier collection, ReadOnlyMemory<float> probe, Seq<PrefetchQuery> prefetch, ulong tenant, RetrievalLimit top) =>
        IO.liftAsync(async () => await Op.Of().Catch(async _ => {
            IReadOnlyList<ScoredPoint> hits = await client.QueryAsync(
                (string)collection, query: probe.ToArray(), prefetch: [.. prefetch], limit: (ulong)top.Value, shardKeySelector: tenant).ConfigureAwait(false);
            return Fin.Succ(toSeq(hits).Map(static hit =>
                (UInt128.Parse(hit.Payload["content-key"].StringValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture), hit.Score)));
        }).ConfigureAwait(false));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                   |
| :-----: | :------------------ | :------------------------------------- | :---------------------------------------------------------- |
|  [01]   | backend             | `VectorRoute` case                     | backend and legal settings share one discriminant           |
|  [02]   | scan tuning         | `SET LOCAL` GUC binder per row         | query-time only; the `WITH` build map stays provisioning's  |
|  [03]   | route observability | `store.vector.route` fact              | a degradation is evidence, never a silent slowdown          |
|  [04]   | server-side ANN     | `VectorMetric.Order` `Expression.Call` | probe constant per `EmbeddingArity.Probe`; never dense-only |
|  [05]   | scale-out ceiling   | `ScaleoutRoute.Query` → `QueryAsync`   | prefetch fused server-side; tenant `ShardKeySelector`       |

## [03]-[LEXICAL_ALGEBRA]

- Owner: `MatchOption` the lexical-modifier capability vocabulary over the kernel `ICapability` floor, held as ONE `CapabilitySet<MatchOption>` column by every case that takes builder modifiers; `Bm25Predicate` the closed `[Union]` projecting the `pg_search` v2 `pdb` surface — one case per `pdb.*` builder, per bare match operator, and per stacking cast modifier — whose `Sql(column)` switch emits the exact server SQL; `SearchProjection` the static score/snippet/aggregate projection surface; `LexicalRank` the `[SmartEnum<string>]` two-row rank axis carrying the BM25 arm and the native `ts_rank` fallback arm.
- Cases: builders `Parse | Match | RangeTerm | PhrasePrefix | MoreLikeThis | Regex | All` (right of `@@@`), bare operators `AnyToken`(`|||`) `| AllToken`(`&&&`) `| ExactTerm`(`===`) `| Phrase`(`###`) `| Proximity`(`##`/`##>` — the held `MatchOption.Ordered` row selects the operator token), cast modifiers `Fuzzy | Boost | Const | Slop` composing over ANY inner predicate and stacking in cast order; `LexicalRank` is `Bm25` (`pdb.score(<key_field>)` over a `bm25` index) and `TsRank` (`ts_rank` over the generated tsvector — the degrade arm a profile without `pg_search` preloaded selects).
- Entry: `public string Sql(Identifier column)` on `Bm25Predicate` switches the union to the exact match expression (`col @@@ pdb.parse(…)`, `col ||| '…'`, `col @@@ ('a' ##> 2 ##> 'b')`, `<inner>::pdb.fuzzy(…)`) — the column an admitted `#COLUMNAR_LANE` trust-gate `Identifier` and every string payload crossing the ONE `Lit` quote-doubling boundary; `SearchProjection.Score(keyColumn)`/`Snippet`/`Snippets`/`SnippetPositions`/`Agg` emit the `[05]` projection functions anchored on the index `key_field`; `LexicalRank.Rank(keyColumn, terms)` emits the row's rank projection so the fusion CTE composes either arm through one call.
- Law: `Rank` DERIVES from `Score` on both rows — verified on the fence: the BM25 arm scores through `pdb.score(<key_field>)` and orders by that same call, the degrade arm through its own `ts_rank` expression and likewise, so no arm can order by an expression it did not project and the two can never disagree.
- Auto: the cast modifiers STACK in cast order (`'<term>'::pdb.fuzzy(2)::pdb.boost(2)` applies typo tolerance then a score multiplier) because each cast case wraps its `Inner` and appends its own cast — composition is structural, never string concatenation at the call site; analyzed matching has two spellings the union keeps distinct — the per-field `pdb.match` builder carrying its own fuzzy `distance`/`prefix` (the `Match` case) and the bare `|||`/`&&&` column operators (the `AnyToken`/`AllToken` cases); the BM25 branch matches `corpus @@@ pdb.parse($terms)` and orders by `pdb.score(<key_field>)` — the index's declared `key_field` anchor, the content key the fusion re-queries the row store by, so the fusion projects IDENTITIES rather than re-materializing candidate payloads; every projection rides `FromSql`/`SqlQuery` raw SQL because `bm25` carries no EF translator.
- Packages: `pg_search` (server-side — the `pdb` schema, `@@@`/`|||`/`&&&`/`===`/`###`/`##`/`##>` operators, `bm25` access method; AGPL confined to the PG server tier, never linked into managed code), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new `pdb` builder, operator, or cast is ONE union case in the `Sql` switch — the catalog's member set is the union's case roster, never a sibling method; a new rank arm is one `LexicalRank` row; zero new surface — a free-string BM25 predicate, a per-builder method family, or a prose-only degrade claim is the deleted form because the predicate is a closed typed tree and the fallback is a vocabulary row.
- Boundary: value transport is STRUCTURAL, never a prose contract — identifiers (column, `key_field` anchor) admit once through the `#COLUMNAR_LANE` `Identifier` trust gate and every free-text payload (term, tag, sort key, aggregate JSON) crosses the ONE `Lit` quote-doubling boundary at the lowering, so a quote-bearing caller string is inert literal text by construction (pdb predicates are literals inside raw SQL the prepared-parameter surface does not reach — the same platform-forced escape boundary the columnar `CREATE SECRET` path names), and the `key_field` join anchor carries a `UNIQUE` constraint with exactly one `bm25` index per table; the index DDL lands as a raw statement on the generation path (`Element/identity#SCHEMA_VERDICT`) — this owner emits QUERY SQL only; the degrade is a CLOSED arm, not a fault — a profile without `pg_search` preloaded selects `LexicalRank.TsRank` inside the same fusion CTE (`websearch_to_tsquery` the only parser admitted to user text), so the fused result stays correct at reduced lexical power and the arm taken is branch-lineage evidence; `pdb.agg` is the Elasticsearch-style facet projection — an aggregate over the matched set, composed as a projection column, never a second aggregation engine beside the columnar lane.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MatchOption : ICapability<MatchOption> {
    public static readonly MatchOption Lenient = new("lenient");
    public static readonly MatchOption Conjunction = new("conjunction");
    public static readonly MatchOption Prefix = new("prefix");
    public static readonly MatchOption Ordered = new("ordered");
    public static readonly MatchOption Transposition = new("transposition");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Bm25Predicate {
    private Bm25Predicate() { }
    public sealed record Parse(string Query, CapabilitySet<MatchOption> Options) : Bm25Predicate;
    public sealed record Match(string Query, Option<int> Distance, CapabilitySet<MatchOption> Options) : Bm25Predicate;
    public sealed record RangeTerm(string Value, string Relation, string RangeType) : Bm25Predicate;
    public sealed record PhrasePrefix(Seq<string> Terms, Option<int> MaxExpansions) : Bm25Predicate;
    public sealed record MoreLikeThis(string DocId, Seq<string> Fields, Option<int> MaxQueryTerms) : Bm25Predicate;
    public sealed record Regex(string Pattern) : Bm25Predicate;
    public sealed record All : Bm25Predicate;
    public sealed record AnyToken(string Terms) : Bm25Predicate;
    public sealed record AllToken(string Terms) : Bm25Predicate;
    public sealed record ExactTerm(string Term) : Bm25Predicate;
    public sealed record Phrase(string Terms) : Bm25Predicate;
    public sealed record Proximity(string Left, int Within, string Right, CapabilitySet<MatchOption> Options) : Bm25Predicate;
    public sealed record Fuzzy(Bm25Predicate Inner, int Distance, CapabilitySet<MatchOption> Options) : Bm25Predicate;
    public sealed record Boost(Bm25Predicate Inner, double Factor) : Bm25Predicate;
    public sealed record Const(Bm25Predicate Inner, double Score) : Bm25Predicate;
    public sealed record Slop(Bm25Predicate Inner, int Distance) : Bm25Predicate;

    public string Sql(Identifier column) => Switch(
        parse:        p => $"{column} @@@ pdb.parse('{Lit(p.Query)}', lenient => {Bool(p.Options, MatchOption.Lenient)}, conjunction_mode => {Bool(p.Options, MatchOption.Conjunction)})",
        match:        m => $"{column} @@@ pdb.match('{Lit(m.Query)}'{m.Distance.Map(static d => $", distance => {d}").IfNone(string.Empty)}, prefix => {Bool(m.Options, MatchOption.Prefix)}, conjunction_mode => {Bool(m.Options, MatchOption.Conjunction)})",
        rangeTerm:    r => $"{column} @@@ pdb.range_term('{Lit(r.Value)}', relation => '{Lit(r.Relation)}', range_type => '{Lit(r.RangeType)}')",
        phrasePrefix: p => $"{column} @@@ pdb.phrase_prefix(ARRAY[{string.Join(", ", p.Terms.Map(static t => $"'{Lit(t)}'"))}]{p.MaxExpansions.Map(static n => $", max_expansions => {n}").IfNone(string.Empty)})",
        moreLikeThis: m => $"{column} @@@ pdb.more_like_this('{Lit(m.DocId)}', fields => ARRAY[{string.Join(", ", m.Fields.Map(static f => $"'{Lit(f)}'"))}]{m.MaxQueryTerms.Map(static n => $", max_query_terms => {n}").IfNone(string.Empty)})",
        regex:        r => $"{column} @@@ pdb.regex('{Lit(r.Pattern)}')",
        all:          _ => $"{column} @@@ pdb.all()",
        anyToken:     a => $"{column} ||| '{Lit(a.Terms)}'",
        allToken:     a => $"{column} &&& '{Lit(a.Terms)}'",
        exactTerm:    e => $"{column} === '{Lit(e.Term)}'",
        phrase:       p => $"{column} ### '{Lit(p.Terms)}'",
        proximity:    p => $"{column} @@@ ('{Lit(p.Left)}' {Near(p.Options)} {p.Within} {Near(p.Options)} '{Lit(p.Right)}')",
        fuzzy:        f => $"{f.Inner.Sql(column)}::pdb.fuzzy({f.Distance}, {Bool(f.Options, MatchOption.Prefix)}, {Bool(f.Options, MatchOption.Transposition)})",
        boost:        b => $"{b.Inner.Sql(column)}::pdb.boost({b.Factor.ToString(CultureInfo.InvariantCulture)})",
        @const:       c => $"{c.Inner.Sql(column)}::pdb.const({c.Score.ToString(CultureInfo.InvariantCulture)})",
        slop:         s => $"{s.Inner.Sql(column)}::pdb.slop({s.Distance})");

    static string Bool(CapabilitySet<MatchOption> options, MatchOption option) => options.Admits(option) ? "true" : "false";

    static string Near(CapabilitySet<MatchOption> options) => options.Admits(MatchOption.Ordered) ? "##>" : "##";

    internal static string Lit(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}

public static class SearchProjection {
    public static string Score(Identifier keyColumn) => $"pdb.score({keyColumn})";
    public static string Snippet(Identifier column, string startTag = "<b>", string endTag = "</b>", int maxChars = 150) =>
        $"pdb.snippet({column}, start_tag => '{Bm25Predicate.Lit(startTag)}', end_tag => '{Bm25Predicate.Lit(endTag)}', max_num_chars => {maxChars})";
    public static string Snippets(Identifier column, int limit, int offset, string sortBy = "score") =>
        $"pdb.snippets({column}, \"limit\" => {limit}, \"offset\" => {offset}, sort_by => '{Bm25Predicate.Lit(sortBy)}')";
    public static string SnippetPositions(Identifier column) => $"pdb.snippet_positions({column})";
    public static string Agg(string esJson) => $"pdb.agg('{Bm25Predicate.Lit(esJson)}') OVER ()";
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LexicalRank {
    public static readonly LexicalRank Bm25 = new("bm25",
        static (key, _) => $"pdb.score({key})",
        static (column, terms) => new Bm25Predicate.Parse(terms, CapabilitySet<MatchOption>.Of(MatchOption.Lenient)).Sql(column));
    public static readonly LexicalRank TsRank = new("ts_rank",
        static (_, terms) => $"ts_rank(lexemes, websearch_to_tsquery('english', '{Bm25Predicate.Lit(terms)}'))",
        static (_, terms) => $"lexemes @@ websearch_to_tsquery('english', '{Bm25Predicate.Lit(terms)}')");

    [UseDelegateFromConstructor] public partial string Score(Identifier keyColumn, string terms);

    public string Rank(Identifier keyColumn, string terms) => $"{Score(keyColumn, terms)} DESC";

    [UseDelegateFromConstructor] public partial string MatchSql(Identifier column, string terms);
}
```

| [INDEX] | [POLICY]          | [VALUE]                                | [BINDING]                                                    |
| :-----: | :---------------- | :------------------------------------- | :----------------------------------------------------------- |
|  [01]   | lexical predicate | `Bm25Predicate` closed union           | one case per builder/operator/cast; `Sql()` the one lowering |
|  [02]   | cast stacking     | modifier cases wrap `Inner`            | `::pdb.fuzzy(...)::pdb.boost(...)` in cast order, structural |
|  [03]   | score anchor      | `pdb.score(<key_field>)`               | identities projected; payloads never re-materialized         |
|  [04]   | order derivation  | `Rank` derives from `Score`            | one expression projects and orders; arms cannot disagree     |
|  [05]   | degrade           | `LexicalRank.TsRank` row               | same CTE, `websearch_to_tsquery` only; visible in lineage    |
|  [06]   | escaping          | `Identifier` gate + one `Lit` boundary | structural; a quote-bearing term is inert literal text       |

## [04]-[VECTOR_CODEBOOK]

- Owner: `ProductCodebook` the product-quantization codebook value-object the `Model/embedding#EMBEDDING` Compute lane encodes against (subspace count, the flat `[subspace][code][dim]` centroid grid, code width, and a content `Id`); `VectorRow` the content-keyed fine-form-plus-codes store the rerank and the ADC scan read; `RetrievalFault` the closed `Fault` family (8410) the codebook admission rejections yield; `VectorCodebook` the static surface owning the per-subspace k-means TRAINING and the amortized asymmetric-distance corpus scan; `VectorIndex` the composition-supplied port carrier owning the codebook supply, the coarse-survivor fine-form resolve, and the PQ-coded corpus read — the ANN index store read by reference, never embedded.
- Cases: `VectorFine` is `Float32 | Int8Scalar`; the quantized case carries `Scale` and `ZeroPoint`, so decode reconstructs magnitude. `RetrievalFault.Mismatched` rejects incoherent ADC layouts, and `RetrievalLimit` admits a positive result bound.
- Entry: `Train` fits and content-keys the codebook; `AdcScan(..., RetrievalLimit)` admits query, codebook, row layout, and result bound before table access; `VectorIndex` supplies codebooks, fine forms, and coded rows through injected ports.
- Auto: `Train` rejects an empty/ragged corpus, a dimension not divisible by `subspaces`, and a corpus smaller than `codesPerSubspace` (which leaves trailing centroid slots untrained at zero) to the typed `RetrievalFault` channel, slices each corpus vector's subspace window, seeds the centroid grid from the first `codesPerSubspace` sub-vectors (deterministic first-k seeding, reproducible across retrains), and iterates assignment (nearest centroid by `TensorPrimitives.Distance`) and mean recompute (`TensorPrimitives.Add` accumulate, `TensorPrimitives.Divide` by the cluster count) — the SAME `TensorPrimitives.Distance` the Compute `EncodeProduct` assigns with, so train-time and encode-time partitions agree bit-for-bit — then snapshots centroid storage and mints the content `Id` over little-endian layout and finite centroid scalars through seed-zero `XxHash128`, collapsing signed zero so equal codebooks key identically across RIDs; `AdcScan` builds the `Subspaces × CodesPerSubspace` table by `TensorPrimitives.Distance` of each query sub-vector against every centroid ONCE, then folds each coded row to the sum of its per-subspace table lookups and keeps the nearest `top` through the kernel `Ranked` bounded-selection cell under `ExtremumDirection.Minimum` (O(n log k) — never a full sort, never a negated priority, never a per-row centroid-distance recompute — the table amortizes it); `Probe` projects the float32 fine bytes onto the `Pgvector.Vector` ANN column the HNSW/diskann index storage is built over.
- Packages: System.Numerics.Tensors (`TensorPrimitives.Distance`/`Add`/`Divide`), System.IO.Hashing (`XxHash128` streaming `Append`/`GetCurrentHashAsUInt128`, seed zero — the kernel growth-row streaming member for a preimage that outgrows a one-shot span), Pgvector (`Vector`), Rasm (`Domain/stats#ORDER_STATISTICS` `Ranked` bounded top-K cell + `ExtremumDirection`), Rasm.Persistence (`Query/lane#ELEMENT_SET_ALGEBRA` `SetKey`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a retrained codebook mints a new `Id`; a fine encoding is one `VectorFine` case; a richer ANN backend is one `VectorRoute` case.
- Boundary: `ProductCodebook` is the ONE PQ vocabulary the boundary shares — Compute imports it by its `Rasm.Persistence (project)` reference and does nearest-centroid encode and centroid-reconstruction decode over it but NEVER fits it, so defining it in Compute forces a `Persistence → Compute` cycle (the dependency runs `Compute → Persistence` only) and a Compute-side k-means is the named drift defect; training uses the SAME `TensorPrimitives.Distance` Compute assigns with so the partition a centroid grid induces at train time and at encode time is identical, and the codebook is supplied content-keyed so a re-train mints a fresh `Id` that re-keys every dependent `product-quantized` artifact (the `Model/embedding#EMBEDDING` content key folds the codebook `Id`); the two-stage retrieval is honest — the `binary-hamming` coarse gate (Compute) returns content keys, `Resolve` reads the survivors' `int8-scalar`/`float32` fine forms by content key, and the Compute `Rank` reranks over those fine forms, so the magnitude a 1-bit encoding discards is recovered from the stored fine form and never faked from the ±1 decode; the amortized ADC scan is Persistence's because this lane owns the index traversal and the `#FUSION_AND_REUSE` recency-bounded reuse while the BOUNDED rerank over the resolved survivors is Compute's, so the query→centroid table is built once and reused across the whole corpus and a per-candidate centroid-distance recompute is the deleted form; the vector branch (the ADC or in-PG HNSW ranked rows mapped through `VectorRow.Subject` to model-qualified `SetKey`s) feeds `#FUSION_AND_REUSE` `FusionRank.Fuse` as one ranked branch, and the `Probe` `vector(N)` column is the same pgvector store type the `Element/identity#ELEMENT_IDENTITY` `Embedding` per-model locator rides (the corpus-grain retrieval index here, the per-model bounding-envelope locator there — two grains, never one duplicated index); the store holds the typed `VectorFine` form and the optional `SetKey` only, no `EmbeddingVector`/`VectorEncoding`/`VectorScore` Compute type, so the strata dependency stays one-directional exactly as the `#FUSION_AND_REUSE` and `Query/cache#MODEL_RESULT_INDEX` owners keep it.

```csharp
// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetrievalFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Retrieval;
    private RetrievalFault() { }

    [FaultCase(0)]
    public sealed partial record EmptyCorpus : RetrievalFault();
    [FaultCase(1)]
    public sealed partial record Layout(int Dimension, int Subspaces, int Codes) : RetrievalFault();
    [FaultCase(2)]
    public sealed partial record Ragged(int Expected, int Found) : RetrievalFault();
    [FaultCase(3)]
    public sealed partial record Undersized(int Corpus, int Codes) : RetrievalFault();
    [FaultCase(4)]
    public sealed partial record Rejected(string Detail) : RetrievalFault();
    [FaultCase(5)]
    public sealed partial record Mismatched(string Axis, string Expected, string Found) : RetrievalFault();

    public override string Message => Switch(
        emptyCorpus: static _ => "<codebook-empty-corpus>",
        layout:      static c => string.Create(CultureInfo.InvariantCulture, $"<codebook-layout:{c.Dimension}/{c.Subspaces}@{c.Codes}>"),
        ragged:      static c => string.Create(CultureInfo.InvariantCulture, $"<codebook-ragged:{c.Expected}!={c.Found}>"),
        undersized:  static c => string.Create(CultureInfo.InvariantCulture, $"<codebook-undersized:{c.Corpus}<{c.Codes}>"),
        rejected:    static c => $"<retrieval-rejected:{c.Detail}>",
        mismatched:  static c => $"<retrieval-mismatch:{c.Axis}:{c.Expected}!={c.Found}>");
}

[ValueObject<int>]
[ValidationError]
public readonly partial struct RetrievalLimit {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value <= 0) { validationError = ValidationError.Create($"<retrieval-limit:{value}>"); }
    }
}

[ValueObject<int>]
[ValidationError]
public readonly partial struct TrainingPasses {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value <= 0) { validationError = ValidationError.Create($"<training-passes:{value}>"); }
    }
}

[ValueObject<float>]
[ValidationError]
public readonly partial struct QuantizationScale {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) {
        if (!float.IsFinite(value) || value <= 0) { validationError = ValidationError.Create($"<quantization-scale:{value}>"); }
    }
}

// --- [MODELS] --------------------------------------------------------------------------
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

    public static Fin<ProductCodebook> Of(int subspaces, int subspaceDim, int codesPerSubspace, ReadOnlyMemory<float> centroids) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(subspaces > 0 && subspaceDim > 0 && codesPerSubspace is > 0 and <= 256,
                subspaces, codesPerSubspace, static (parts, codes) => new RetrievalFault.Layout(0, parts, codes)),
            AdmissionSlots.Gate((long)subspaces * subspaceDim * codesPerSubspace == centroids.Length,
                (long)subspaces * subspaceDim * codesPerSubspace, centroids.Length,
                static (expected, found) => new RetrievalFault.Mismatched("centroids-length",
                    expected.ToString(CultureInfo.InvariantCulture),
                    found.ToString(CultureInfo.InvariantCulture))),
            AdmissionSlots.Gate(TensorPrimitives.IsFiniteAll(centroids.Span),
                unit, "<codebook-centroids-nonfinite>", static (_, detail) => new RetrievalFault.Rejected(detail))))
        .Map(_ => centroids.ToArray())
        .Map(owned => new ProductCodebook(subspaces, subspaceDim, codesPerSubspace, owned,
            KeyOf(subspaces, subspaceDim, codesPerSubspace, owned)))
        .ToFin();
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class VectorCodebook {
    public static Fin<ProductCodebook> Train(Seq<ReadOnlyMemory<float>> corpus, int subspaces, int codesPerSubspace, TrainingPasses passes) =>
        corpus.Head.ToFin(new RetrievalFault.EmptyCorpus())
            .Bind(first => Fitted(corpus, first.Length, subspaces, codesPerSubspace, passes));

    static Fin<ProductCodebook> Fitted(Seq<ReadOnlyMemory<float>> corpus, int dimension, int subspaces, int codesPerSubspace, TrainingPasses passes) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(subspaces > 0 && dimension % subspaces == 0 && codesPerSubspace is > 0 and <= 256,
                dimension, (Subspaces: subspaces, Codes: codesPerSubspace),
                static (int width, (int Subspaces, int Codes) shape) => new RetrievalFault.Layout(width, shape.Subspaces, shape.Codes)),
            SameWidth(corpus, dimension),
            AdmissionSlots.Gate(corpus.ForAll(static vector => TensorPrimitives.IsFiniteAll(vector.Span)),
                unit, "<codebook-corpus-nonfinite>", static (_, detail) => new RetrievalFault.Rejected(detail)),
            AdmissionSlots.Gate(corpus.Count >= codesPerSubspace, corpus.Count, codesPerSubspace, RetrievalFault.Undersized)))
        .ToFin()
        .Bind(_ => Fitted(corpus, dimension / subspaces, subspaces, codesPerSubspace, passes.Value));

    static Validation<Error, Unit> SameWidth(Seq<ReadOnlyMemory<float>> corpus, int expected) =>
        corpus.Find(vector => vector.Length != expected).Match(
            Some: vector => Fail<Error, Unit>(new RetrievalFault.Ragged(expected, vector.Length)),
            None: static () => Success<Error, Unit>(unit));

    static Fin<ProductCodebook> Fitted(Seq<ReadOnlyMemory<float>> corpus, int subDim, int subspaces, int codesPerSubspace, int passes) {
        float[] centroids = new float[subspaces * codesPerSubspace * subDim];
        for (int subspace = 0; subspace < subspaces; subspace++) {
            Lloyd(corpus, subspace, subDim, codesPerSubspace, passes, centroids.AsSpan(subspace * codesPerSubspace * subDim, codesPerSubspace * subDim));
        }
        return ProductCodebook.Of(subspaces, subDim, codesPerSubspace, centroids);
    }

    public static Fin<Seq<(UInt128 ContentKey, float Distance)>> AdcScan(ReadOnlyMemory<float> query, ProductCodebook codebook, Seq<VectorRow> coded, RetrievalLimit top) {
        if (query.Length != codebook.Dimension) {
            return Fin.Fail<Seq<(UInt128 ContentKey, float Distance)>>(new RetrievalFault.Mismatched("query-dim", codebook.Dimension.ToString(CultureInfo.InvariantCulture), query.Length.ToString(CultureInfo.InvariantCulture)));
        }
        if (!TensorPrimitives.IsFiniteAll(query.Span)) {
            return Fin.Fail<Seq<(UInt128 ContentKey, float Distance)>>(new RetrievalFault.Rejected("<adc-query-nonfinite>"));
        }
        if (coded.IsEmpty) { return Fin.Succ(Seq<(UInt128 ContentKey, float Distance)>()); }
        Fin<Unit> coherent = Coherent(codebook, coded);
        if (coherent.IsFail) { return coherent.Map(static _ => Seq<(UInt128 ContentKey, float Distance)>()); }
        float[] table = new float[codebook.Subspaces * codebook.CodesPerSubspace];
        for (int subspace = 0; subspace < codebook.Subspaces; subspace++) {
            ReadOnlySpan<float> part = query.Span.Slice(subspace * codebook.SubspaceDim, codebook.SubspaceDim);
            for (int code = 0; code < codebook.CodesPerSubspace; code++) {
                table[subspace * codebook.CodesPerSubspace + code] = TensorPrimitives.Distance(part, codebook.Centroid(subspace, code));
            }
        }
        Ranked<(UInt128 ContentKey, float Distance), float> nearest = new(top.Value, ExtremumDirection.Minimum);
        foreach (VectorRow row in coded) {
            ReadOnlySpan<byte> codes = row.Codes.Span;
            float distance = 0f;
            for (int subspace = 0; subspace < codes.Length; subspace++) {
                distance += table[subspace * codebook.CodesPerSubspace + codes[subspace]];
            }
            nearest.Offer((row.ContentKey, distance), distance);
        }
        return Fin.Succ(nearest.Drain());
    }

    static Fin<Unit> Coherent(ProductCodebook codebook, Seq<VectorRow> coded) =>
        coded.TraverseM(row =>
            row.CodebookId != codebook.Id
                ? Fin.Fail<Unit>(new RetrievalFault.Mismatched("codebook-id", codebook.Id.ToString("x32", CultureInfo.InvariantCulture), row.CodebookId.ToString("x32", CultureInfo.InvariantCulture)))
            : row.Codes.Length != codebook.Subspaces
                ? Fin.Fail<Unit>(new RetrievalFault.Mismatched("codes-length", codebook.Subspaces.ToString(CultureInfo.InvariantCulture), row.Codes.Length.ToString(CultureInfo.InvariantCulture)))
            : Ranged(codebook, row.Codes.Span)).As().Map(static _ => unit);

    static Fin<Unit> Ranged(ProductCodebook codebook, ReadOnlySpan<byte> codes) {
        foreach (byte code in codes) {
            if (code >= codebook.CodesPerSubspace) {
                return Fin.Fail<Unit>(new RetrievalFault.Mismatched("code-range", codebook.CodesPerSubspace.ToString(CultureInfo.InvariantCulture), code.ToString(CultureInfo.InvariantCulture)));
            }
        }
        return Fin.Succ(unit);
    }

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
|  [04]   | coarse→fine rerank  | `Resolve` reads `int8`/`float32` fine    | magnitude recovered from the store, never faked           |
|  [05]   | ADC amortization    | one query→centroid table per scan        | reused across the corpus; never a per-row recompute       |
|  [06]   | admission faults    | `RetrievalFault` 841x                    | generated direct-union identity                           |
|  [07]   | strata one-way      | `VectorFine` + `SetKey` only             | no Compute type crosses down                              |

## [05]-[FUSION_AND_REUSE]

- Owner: `RetrievalBranch` the `[SmartEnum<string>]` typed branch axis carrying each branch's index identity; `FusionHit` carries each element's fused rank and typed contributions; `FusionRank` owns the n-ary reciprocal-rank fold with the `RrfConstant` policy; `ResultCache` keys the read-through `HybridCache` tier on both `KeySelection.ContentKey` and the operation content key; `RetrievalOp` is the request `[Union]`, and `Retrieval` is the polymorphic dispatcher.
- Cases: `RetrievalOp` is `Fuse | Train | AdcScan`; `RetrievalResult` is `Fused | Trained | Scanned`; `RetrievalBranch` is `Vector | Spatial | Lexical`, each carrying its index identity.
- Entry: `Run` admits the vector lane against the `StoreProfile`, then dispatches the closed op family; `FusionRank.Fuse` applies the single `RrfConstant` policy and preserves typed lineage; `ResultCache.Cached` read-through-caches the derived retrieval under the subject content key.
- Auto: fusion applies `Score(e) = Σ_b 1 / (RrfConstant + rank_b(e))` and preserves typed lineage. `VectorRoute` selects the vector backend, and spatial and lexical branches retain their index identities.
- Packages: Microsoft.Extensions.Caching.Hybrid (`HybridCache.GetOrCreateAsync`/`HybridCacheEntryOptions`/`RemoveByTagAsync`), Rasm.Persistence (`Store/provisioning#SERVER_EXTENSIONS` `StoreProfile.Admits` — the lane-realizability axis), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new retrieval branch is one `RetrievalBranch` row carrying its index name and one ranked list into the same `Fuse` fold; a new retrieval modality is one `RetrievalOp` case whose arm breaks `Run` loudly at compile time; zero new surface — a per-branch-pair fusion, a bespoke score blend, a positional `branch-{b}` string lineage, a free-string cache tag, or a sibling `FuseMany`/`TrainAndScan` entrypoint is the deleted form because the RRF is one n-ary fold over the typed branch axis and the op union owns modality.
- Boundary: `Run` is the vector lane's ONE admission owner — an engine whose `StoreProfile` cannot realize the ANN backend refuses there with the axis named, so `Fuse`, `Train`, and `AdcScan` execute on a proven lane and no arm carries a second realizability test; the lexical store is a SEPARATE roster row admitting at `#DOCUMENT_CORPUS`, so this page holds two lane gates because it serves two lanes, never one gate standing for both. This owner is the search-lane binding the pgvector/pg_search/pgvectorscale/qdrant `.api` catalogs compose against — a catalogue's `VectorMetric`/`EmbeddingArity`/`Bm25Predicate`/RRF reference resolves here, never a parallel saved-search owner; the fusion is the one n-ary RRF fold over the typed `RetrievalBranch` axis so a hit's lineage names the index that ranked it; the cache is the AppHost `HybridCache` port keyed on the content-addressed `KeySelection.ContentKey` (minted by `Query/lane#ELEMENT_SET_ALGEBRA`) with a derived tag, and this SELECTION-RESULT cache is a DIFFERENT owner from `Query/cache`'s compute-result reuse index (`ArtifactIndexRow`/`ModelResultIndex`) — the fusion result boundary feeds cache's index rows, never merges with them; spatial→PG GiST and ANN→pgvector are the index owners (DuckDB spatial/vss being the columnar aggregator only, not the transactional index), so the fusion branches read the federated row's GiST/HNSW/tsvector columns and never duplicate the index, the vector branch resolving through the `#VECTOR_CODEBOOK` `VectorRow.Subject`-mapped ranked rows.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FusionRank {
    public const int RrfConstant = 60;

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
    public static IO<T> Cached<TState, T>(KeySelection subject, UInt128 operationKey, RetrievalCachePolicy policy, TState state,
        Func<TState, CancellationToken, ValueTask<T>> produce, HybridCache cache) {
        string subjectKey = subject.ContentKey.ToString("x32", CultureInfo.InvariantCulture);
        string operation = operationKey.ToString("x32", CultureInfo.InvariantCulture);
        return IO.liftAsync(async () => await Op.Of().Catch(async token => Fin<T>.Succ(await cache.GetOrCreateAsync(
            $"{policy.Namespace}:{subjectKey}:{operation}",
            state,
            produce,
            new HybridCacheEntryOptions { Expiration = policy.TimeToLive.ToTimeSpan() },
            tags: [$"elementset:{subjectKey}"],
            cancellationToken: token).ConfigureAwait(false))).ConfigureAwait(false)).Bind(IO.lift);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class Retrieval {
    public static Fin<RetrievalResult> Run(StoreProfile store, RetrievalOp op) =>
        !store.Admits(Lane.Vector)
        ? Fin.Fail<RetrievalResult>(new RetrievalFault.Mismatched("store-lane", Lane.Vector.Key, store.Key))
        : op.Switch(
            fuse:    static f => Fin.Succ<RetrievalResult>(new RetrievalResult.Fused(FusionRank.Fuse(f.Branches))),
            train:   static t => VectorCodebook.Train(t.Corpus, t.Subspaces, t.CodesPerSubspace, t.Passes)
                                     .Map(static trained => (RetrievalResult)new RetrievalResult.Trained(trained)),
            adcScan: static s => VectorCodebook.AdcScan(s.Query, s.Codebook, s.Coded, s.Top)
                                     .Map(static nearest => (RetrievalResult)new RetrievalResult.Scanned(nearest)));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                     | [BINDING]                                                        |
| :-----: | :----------------- | :------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | entry              | `Run(StoreProfile, RetrievalOp)`            | fuse/train/scan are cases; no sibling entrypoint family          |
|  [02]   | fusion             | n-ary RRF over `RetrievalBranch`            | typed branch lineage; `RrfConstant = 60` named once              |
|  [03]   | cache key          | content-addressed `KeySelection.ContentKey` | cached under the subject key                                     |
|  [04]   | cache invalidation | content-key tag → `RemoveByTagAsync`        | a changefeed op-log change to a contributing node cuts it        |
|  [05]   | cache identity     | selection-result reuse                      | distinct from `Query/cache`'s compute-result index               |
|  [06]   | index ownership    | GiST spatial, pgvector ANN, BM25 lexical    | DuckDB is the columnar aggregator, never the index               |
|  [07]   | lane admission     | `Admits(Lane.Vector)` inside `Run`          | typed row, refused once; `Lane.Search` gates at #DOCUMENT_CORPUS |

## [06]-[DOCUMENT_CORPUS]

- Owner: `CorpusKind` is the closed document-source roster whose keys ARE the wire tokens the consuming shell spells; `CorpusRow` is the indexed row the lane admits and the projection returns identity from; `DocumentPredicate` is the closed token-to-`Bm25Predicate` lowering; `DocumentQuery`/`DocumentHit` are the consumed query and answer wire; `DocumentCorpus` owns admission, statement composition, and hit shaping.
- Cases: `CorpusKind` is `Cell | Prose | Issue | Node`, each row carrying its coverage columns — the subject, member, and body semantics its documents fill; `DocumentPredicate` is `Match | Phrase | PhrasePrefix | Regex`, one row per grammar the consumer's own vocabulary closes.
- Entry: `public static Fin<string> Statement(StoreProfile store, DocumentQuery query, LexicalRank rank)` admits the search lane and then the wire ONCE — non-blank terms, a non-empty scope of admitted `CorpusKind` keys, a bounded limit — then lowers the predicate token, the scope filter, the subject narrowing, and the rank arm's own score and order fragments into one statement; `public static Fin<DocumentHit> Shape(...)` folds one projected row into the answer wire.
- Auto: this corpus is a `RetrievalBranch.Lexical` store, so a document search is a first-class branch a `#FUSION_AND_REUSE` `Fuse` can take beside the vector and spatial branches without a second ranked-list shape. Each predicate token selects its `Bm25Predicate` case and the whole-word column selects the exact-term operator inside the `Match` row alone — a phrase already bounds its tokens, a prefix contradicts a boundary by construction, and a pattern carries its own. Case sensitivity is NOT an index property: the `bm25` analyzer case-folds at build, so the lane narrows case-insensitively and gates the matched set with a positional containment test before ranking. Snippet, positions, and score all project through `#LEXICAL_ALGEBRA` — `SearchProjection.Snippet`, `SearchProjection.SnippetPositions`, and the rank row's own `Score` — so the degrade arm answers the same column set at reduced lexical power.
- Law: query modifiers cross as ONE `CapabilitySet<SearchOption>` column, not a bool pair. NAMED LOSS: two named `bool` positions a caller sets by name at the constructor. WITNESS: `dotnet:Rasm.AppUi/Document/search#INDEX_WIRE` composes `DocumentQuery` DIRECTLY rather than re-spelling it, so one declaration moves both ends together — a third modifier lands as one row instead of a third positional bool the decoder must know the order of, and the set's ordinal-key `Wire` projection is the crossing form. AppUi construction sites move in that plane's own pass.
- Law: each lane gate names the TYPED `Store/provisioning#SERVER_EXTENSIONS` `Lane` row — `Lane.Vector` at `Retrieval.Run` and `Lane.Search` here. That pair of page-local `const string` values spelled tokens the `Lane` vocabulary already owns, which is the deleted form on that owner's own law: a token minted at a call site cannot be checked against the roster that declares it.
- Packages: `pg_search` (server-side — the `pdb` schema, the `bm25` access method), Rasm.Persistence (`Store/provisioning#SERVER_EXTENSIONS` `StoreProfile.Admits` — the lane-realizability axis), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new document source is one `CorpusKind` row carrying its coverage columns, and the consumer's source roster gains the matching key; a new grammar is one `DocumentPredicate` row lowering to an existing `Bm25Predicate` case; zero new surface and no second corpus relation.
- Boundary: `Statement` is the search lane's ONE admission owner — an engine whose `StoreProfile` cannot realize the lexical store carries neither a `bm25` index nor a generated `lexemes` column, so the degrade arm is unreachable too and the refusal names the axis at the entry rather than surfacing as a failed statement; the vector lane is a SEPARATE roster row admitting at `#FUSION_AND_REUSE`. INDEX CUSTODY IS THIS OWNER'S and the consuming shell holds none — the corpus relation, its `bm25` index with `content_key` as the declared `key_field`, its generated `lexemes` tsvector for the degrade arm, and the ingest that lands rows from the durable owners all live here, so a shell-side index is the deleted form. Index DDL still lands as a raw statement on the generation path (`Element/identity#SCHEMA_VERDICT`) exactly as `#LEXICAL_ALGEBRA` rules — this section emits QUERY SQL only. `DocumentQuery` and `DocumentHit` are the WHOLE contract with `dotnet:Rasm.AppUi/Document/search#INDEX_WIRE`, and that plane composes these declarations directly through its legal package reference — a member-for-member re-spelled record at the consumer is the deleted twin; the grammar crosses as a predicate token rather than a second vocabulary, the scope crosses as `CorpusKind` keys, and the limit ceiling admits at both ends against the one `LimitCeiling`. `Shape` returns IDENTITIES, snippet text, positions, and a score alone — the body a hit matched never re-crosses, because the row store already holds it and a returned payload forks storage. `DocumentHit` carries no rank-arm column because `FusionHit.Lineage` already identifies the ranking branch. Every identifier admits through the `#COLUMNAR_LANE` `Identifier` trust gate and every free-text payload crosses the one `Bm25Predicate.Lit` boundary, so a quote-bearing term is inert literal text by construction.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CorpusKind {
    public static readonly CorpusKind Cell = new("cell", subject: "notebook document id", member: "cell id", body: "cell source text");
    public static readonly CorpusKind Prose = new("prose", subject: "prose document id", member: "", body: "raw markdown source");
    public static readonly CorpusKind Issue = new("issue", subject: "issue topic id", member: "comment id", body: "issue and comment text");
    public static readonly CorpusKind Node = new("node", subject: "graph document id", member: "node key", body: "node title text");

    public string Subject { get; }
    public string Member { get; }
    public string Body { get; }
    public bool Keyed => Member.Length > 0;
    private CorpusKind(string key, string subject, string member, string body) : this(key) => (Subject, Member, Body) = (subject, member, body);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DocumentPredicate {
    public static readonly DocumentPredicate Match = new("match",
        static (terms, options) => options.Admits(SearchOption.WholeWords)
            ? (Bm25Predicate)new Bm25Predicate.ExactTerm(terms)
            : new Bm25Predicate.Match(terms, Distance: None, CapabilitySet<MatchOption>.Of(MatchOption.Conjunction)));
    public static readonly DocumentPredicate Phrase = new("phrase",
        static (terms, _) => new Bm25Predicate.Phrase(terms));
    public static readonly DocumentPredicate PhrasePrefix = new("phrase-prefix",
        static (terms, _) => new Bm25Predicate.PhrasePrefix(
            toSeq(terms.Split(' ', StringSplitOptions.RemoveEmptyEntries)), MaxExpansions: None));
    public static readonly DocumentPredicate Regex = new("regex",
        static (terms, _) => new Bm25Predicate.Regex(terms));

    [UseDelegateFromConstructor] public partial Bm25Predicate Lower(string terms, CapabilitySet<SearchOption> options);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct CorpusRow(
    UInt128 ContentKey, CorpusKind Kind, string Subject, Option<string> Member, string Title, string Body);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SearchOption : ICapability<SearchOption> {
    public static readonly SearchOption CaseSensitive = new("case-sensitive");
    public static readonly SearchOption WholeWords = new("whole-words");
}

public sealed record DocumentQuery(
    string Terms,
    string Predicate,
    Seq<string> Sources,
    Option<string> Subject,
    int Limit,
    CapabilitySet<SearchOption> Options);

public sealed record DocumentHit(
    string Source,
    string Subject,
    Option<string> Member,
    string Title,
    int SpanStart,
    int SpanLength,
    string Snippet,
    double Score);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DocumentCorpus {
    public const int LimitCeiling = 1000;

    public static readonly Identifier Relation = Identifier.Create("document_corpus");
    public static readonly Identifier KeyField = Identifier.Create("content_key");
    public static readonly Identifier BodyColumn = Identifier.Create("body");


    public static Fin<string> Statement(StoreProfile store, DocumentQuery query, LexicalRank rank) =>
        !store.Admits(Lane.Search)
        ? Fin.Fail<string>(new RetrievalFault.Mismatched("store-lane", Lane.Search.Key, store.Key))
        : (from admitted in Admit(query)
           from predicate in Op.Of().Row<string, DocumentPredicate>(admitted.Predicate)
               .MapFail(_ => new RetrievalFault.Mismatched(
                   "document-predicate", string.Join("|", DocumentPredicate.Items.Select(static p => p.Key)), admitted.Predicate))
           select Composed(admitted, predicate, rank));

    public static Fin<DocumentHit> Shape(
        CorpusKind kind, string subject, Option<string> member, string title, string snippet,
        Seq<(int Start, int Length)> positions, double score) =>
        positions.Head
            .ToFin(new RetrievalFault.Mismatched("snippet-positions", "at least one match position", "none"))
            .Map(first => new DocumentHit(kind.Key, subject, member, title, first.Start, first.Length, snippet, score));

    static string Composed(DocumentQuery query, DocumentPredicate predicate, LexicalRank rank) =>
        $"""
         SELECT kind AS "Source", subject AS "Subject", member AS "Member", title AS "Title",
                {SearchProjection.Snippet(BodyColumn)} AS "Snippet",
                {SearchProjection.SnippetPositions(BodyColumn)} AS "Positions",
                {rank.Score(KeyField, query.Terms)} AS "Score"
         FROM {Relation}
         WHERE {predicate.Lower(query.Terms, query.Options).Sql(BodyColumn)}
           AND kind = ANY(ARRAY[{string.Join(", ", query.Sources.Map(static source => $"'{Bm25Predicate.Lit(source)}'"))}])
         {query.Subject.Map(static subject => $"  AND subject = '{Bm25Predicate.Lit(subject)}'").IfNone(string.Empty)}
         {(query.Options.Admits(SearchOption.CaseSensitive) ? $"  AND position('{Bm25Predicate.Lit(query.Terms)}' in {BodyColumn}) > 0" : string.Empty)}
         ORDER BY {rank.Rank(KeyField, query.Terms)}
         LIMIT {query.Limit}
         """;

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
|  [02]   | index DDL        | raw generation DDL                     | `Element/identity#SCHEMA_VERDICT`; this section emits query SQL    |
|  [03]   | branch identity  | `RetrievalBranch.Lexical`              | a document search fuses beside vector and spatial, one shape       |
|  [04]   | grammar crossing | `DocumentPredicate` token              | one token per `Bm25Predicate` case; no second vocabulary           |
|  [05]   | word boundary    | exact-term operator inside `Match`     | the other three rows bound their tokens by construction            |
|  [06]   | case sensitivity | positional containment gate            | the `bm25` analyzer case-folds at build; the index cannot carry it |
|  [07]   | wire projection  | identities, snippet, positions, score  | the matched body never re-crosses; storage stays unforked          |
|  [08]   | rank arm         | `FusionHit.Lineage`                    | a copy on the wire is a column the consumer never reads            |
|  [09]   | limit ceiling    | `LimitCeiling` here; consumer reads it | neither end accepts what the other refuses                         |
|  [10]   | lane admission   | `Admits(Lane.Search)` in `Statement`   | the degrade arm needs the lane too; refused once, axis named       |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
