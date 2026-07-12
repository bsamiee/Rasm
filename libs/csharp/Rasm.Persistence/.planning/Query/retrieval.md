# [PERSISTENCE_QUERY_RETRIEVAL]

Rasm.Persistence owns the coupled ANN retrieval subsystem behind the `Query/lane#READ_ROUTING` `Cache` lane: one `Retrieval.Run(RetrievalOp)` entry dispatches the closed op family — `Fuse` (weighted n-ary reciprocal-rank fusion over the typed vector/spatial/lexical branches), `Train` (per-subspace PQ k-means minting the `ProductCodebook` the Compute `Model/embedding` lane encodes against), `AdcScan` (the amortized asymmetric-distance corpus scan) — and every fault rails the MINTED `RetrievalFault : Rasm.Domain.Expected` band 8410. Residency is ROW DATA on the one `VectorBackend` `[SmartEnum<string>]` axis: the always-present `ExactScan` correctness baseline, the in-PG `Hnsw`/`IvfFlat` (pgvector) and `DiskAnn` (pgvectorscale) approximate routes each carrying its `SET LOCAL` query-GUC binder rows, the in-process `PqAdc` hot-set lane this page's codebook powers, and the `QdrantScaleout` provider row for the corpus-scale ceiling the in-PG tier cannot reach — the taken route is an observable `search.vector.route` fact, never a silent degradation. The lexical branch is a TYPED algebra, not prose: `Bm25Predicate` is the closed `[Union]` projecting every `pg_search` `pdb.*` builder, bare match operator, and stacking cast modifier to its exact SQL, `SearchProjection` the score/snippet/aggregate projection surface, and `LexicalRank` the closed two-row fallback that degrades BM25 to the native `ts_rank` baseline inside the same CTE when the extension is absent. The fusion result read-through-caches on the content-addressed `ElementSet.Receipt` minted by `Query/lane#ELEMENT_SET_ALGEBRA` — the receipt currency stays the lane's, the reuse tier lives here. `NodeId` arrives from `Rasm.Element`; `ElementSet`/`SetExpr`/`SetResolve`/`ElementSetAlgebra` arrive from `Query/lane`; `VectorDbFunctionsExtensions`/`Pgvector.Vector` arrive from the pgvector EF plugin; `HybridCache` arrives from AppHost as an injected port value; `DiskAnnOps`/`ServerExtension` index DDL stays `Store/provisioning#SERVER_EXTENSIONS` — this page owns query-time routing, never the build-time `WITH` map.

## [01]-[INDEX]

- [01]-[SEARCH_PROVISIONING_PROBE]: the `EmbeddingArity`/`VectorMetric` store-binding axes, the `VectorBackend` residence axis with per-row `SET LOCAL` GUC binders, the server-side LINQ `ORDER BY` leg, and the `search.vector.route` fact.
- [02]-[LEXICAL_ALGEBRA]: the `Bm25Predicate` typed builder/operator/cast union, the `SearchProjection` score/snippet surface, and the `LexicalRank` ts_rank fallback arm.
- [03]-[VECTOR_CODEBOOK]: the `ProductCodebook` Compute encodes against, the per-subspace k-means training, the coarse→fine fine-form resolve, the amortized asymmetric-distance corpus scan, and the `RetrievalFault` band.
- [04]-[FUSION_AND_REUSE]: the typed `RetrievalBranch` axis, the weighted n-ary reciprocal-rank fusion with per-hit lineage, the receipt-keyed read-through reuse, and the one `RetrievalOp` entry.

## [02]-[SEARCH_PROVISIONING_PROBE]

- Owner: `EmbeddingArity` the `[SmartEnum<string>]` CLR-to-store vector arity axis (`vector`/`halfvec`/`sparsevec`/`bit`); `VectorMetric` the `[SmartEnum<string>]` closed six-metric pgvector distance axis carrying each metric's raw-CTE `Op` literal, EF-translator `Fn` member, and the server-side `Order` expression leg; `SearchTuning` the optional query-time tuning record whose bounded scan-mode field rides the typed `ScanOrder` `[SmartEnum<string>]` (`off`/`relaxed_order`/`strict_order` — never a raw mode string); `VectorBackend` the `[SmartEnum<string>]` ANN residence axis whose rows carry the in-PG flag, the route name the fact emits, and the `Bind` delegate deriving each row's `SET LOCAL` GUC statements from the tuning value; `SearchRoute` the static surface emitting the transaction-scoped binder statements and the route fact.
- Cases: `VectorBackend` is `ExactScan` (the always-present brute-force correctness baseline — zero GUCs), `Hnsw` (`hnsw.ef_search`/`hnsw.iterative_scan`/`hnsw.max_scan_tuples`/`hnsw.scan_mem_multiplier`), `IvfFlat` (`ivfflat.probes`/`ivfflat.max_probes`/`ivfflat.iterative_scan`), `DiskAnn` (`diskann.query_search_list_size`/`diskann.query_rescore`), `PqAdc` (the in-process hot-set ADC lane `#VECTOR_CODEBOOK` powers — no server GUC), `QdrantScaleout` (the external sharded store row — no PG GUC); `EmbeddingArity` is `Dense`/`Half`/`Sparse`/`Bit`; `VectorMetric` is `L2`/`InnerProduct`/`Cosine`/`L1`/`Hamming`/`Jaccard`.
- Entry: `public static Seq<string> SetLocal(VectorBackend route, SearchTuning tuning)` derives the `SET LOCAL <guc> = <value>` statements the store session executes transaction-scoped before the ANN `ORDER BY` — per-lane scan policy over one shared pool, exactly the transaction-scoped-settings law the pgvector catalog states; `VectorMetric.Order(Expression column, float[] probe)` builds the EF-translated `ORDER BY` projection `Expression.Call(typeof(VectorDbFunctionsExtensions), Fn, Type.EmptyTypes, column, Expression.Constant(new Vector(probe)))` so the server-side `<->`/`<=>` LINQ leg is a composed expression, never an unrealized `Fn` string.
- Auto: an absent `SearchTuning` option emits NO `SET LOCAL` row so the server default rules and the binder never fabricates a knob; the planner routes a `VectorMetric`-ordered query through whichever index the column carries (HNSW/ivfflat/diskann over the exact scan) with no query rewrite, so the BACKEND row selects the GUC vocabulary and the fact name while the index choice stays the planner's — a selective filter over an ANN index silently truncates without iterative scan, which is why `hnsw.iterative_scan` + `hnsw.max_scan_tuples` are binder rows rather than schema; `DiskAnn` widens recall through `diskann.query_search_list_size` and re-scores the SBQ pre-filter through `diskann.query_rescore` — query-time GUCs, never build options (the `DiskAnnOptions` `WITH` map is `Store/provisioning#SERVER_EXTENSIONS`'s, disjoint owners never crossed); `QdrantScaleout` executes on `QdrantClient.QueryAsync` with `PrefetchQuery` multi-stage hybrid prefetch, `Fusion.Rrf` cross-prefetch fusion, `Formula` score-boosting rerank, `QuantizationConfig` (scalar/product/binary) memory compression, and `ShardKey` multitenant partitioning — the billion-scale row beyond the in-PG ceiling, selected by deployment policy, never a code path fork.
- Receipt: a routed vector scan rides `search.vector.route` carrying the `VectorBackend` key and the bound GUC set, so exact-scan vs HNSW vs IVFFlat vs diskann vs ADC vs Qdrant is an observable discrimination and a route degradation (an ANN demand served by the exact scan) is visible evidence, never a silent slowdown.
- Packages: Pgvector.EntityFrameworkCore (`VectorDbFunctionsExtensions` six distance members), Pgvector (`Vector`), Qdrant.Client (`QdrantClient.QueryAsync`/`PrefetchQuery`/`Fusion`/`Formula`/`QuantizationConfig`/`ShardKey` — the scale-out row's provider surface), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Linq.Expressions`).
- Growth: a new ANN residence is one `VectorBackend` row carrying its GUC vocabulary and route name; a new tuning knob is one `SearchTuning` field plus the owning row's `Bind` read; a new arity is one `EmbeddingArity` row, a new distance one `VectorMetric` row; zero new surface — a per-backend service family, a build-time `query_*` knob, a query-time build option, a parallel column-type enum, or an unobservable silent exact-scan degrade is the deleted form because residency is row data on one axis and the route is a fact.
- Boundary: this owner is QUERY-TIME only — index DDL (`HasMethod("hnsw")`/`HasOperators`/`HasStorageParameter` on the EF builder, the `diskann`/`bm25` raw `MigrationBuilder.Sql` rows) belongs to `Element/identity#SCHEMA_VERDICT` and `Store/provisioning#SERVER_EXTENSIONS`, and the `DiskAnnOps` ops-class vocabulary stays the provisioning owner's — this page reads the same operator through `VectorMetric`, never a second ops-class enum; the exact brute-force scan is the correctness baseline every ANN claim measures against (recall@k vs exact, latency), so `ExactScan` is a first-class row, not an error path; `PqAdc` is the hot-set lane whose codebook, fine-form residence, and scan live in `#VECTOR_CODEBOOK` — corpus-scale ANN belongs to the pgvector/pgvectorscale indexes while the PQ/ADC row keeps the hot set, so the server-side LINQ row and the in-process row are complementary residences on one axis, neither redundant beside the other; `QdrantScaleout` is deployment DATA — the in-PG tier stays the default residence and the external store enters only where ANN cardinality or recall tuning exceeds what a pgvector HNSW index serves, its `VectorRow.ContentKey` identity staying the one content key so a hit resolves through the same fine-form residence regardless of which backend ranked it.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers.Binary;
using System.Globalization;
using System.IO.Hashing;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using LanguageExt;
using Microsoft.Extensions.Caching.Hybrid;
using NodaTime;
using Pgvector.EntityFrameworkCore;
using Rasm.Element;
using Thinktecture;
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// The CLR-to-store arity axis the `pgvector` plugin maps (`api-pgvector-ef`): one row per store type carrying
// its `vector`/`halfvec`/`sparsevec`/`bit` base name. `Bit` is the `binary_quantize(emb)::bit(N)` expression
// column the coarse Hamming gate reads; the other three map `Pgvector.Vector`/`HalfVector`/`SparseVector`. A
// new arity is one row, never a parallel column-type enum.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EmbeddingArity {
    public static readonly EmbeddingArity Dense = new("dense", "vector");
    public static readonly EmbeddingArity Half = new("half", "halfvec");
    public static readonly EmbeddingArity Sparse = new("sparse", "sparsevec");
    public static readonly EmbeddingArity Bit = new("bit", "bit");
    public string StoreType { get; }
    public string Column(int n) => $"{StoreType}({n})";
    private EmbeddingArity(string key, string storeType) : this(key) => StoreType = storeType;
}

// The six pgvector distance metrics as one closed axis (`api-pgvector-ef` `VectorDbFunctionsExtensions`): the
// `Op` column carries the raw-CTE operator literal (`<->`/`<#>`/`<=>`/`<+>`/`<~>`/`<%>`), the `Fn` column the
// EF-translator member name, and `Order` COMPOSES the server-side `ORDER BY` leg — the translator matches the
// MethodInfo by reference, so the expression call IS the `embedding <op> $probe` the index serves. A `Bit`
// arity routes through `Hamming`/`Jaccard`, a dense arity through the four float metrics.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VectorMetric {
    public static readonly VectorMetric L2 = new("l2", "<->", nameof(VectorDbFunctionsExtensions.L2Distance));
    public static readonly VectorMetric InnerProduct = new("ip", "<#>", nameof(VectorDbFunctionsExtensions.MaxInnerProduct));
    public static readonly VectorMetric Cosine = new("cosine", "<=>", nameof(VectorDbFunctionsExtensions.CosineDistance));
    public static readonly VectorMetric L1 = new("l1", "<+>", nameof(VectorDbFunctionsExtensions.L1Distance));
    public static readonly VectorMetric Hamming = new("hamming", "<~>", nameof(VectorDbFunctionsExtensions.HammingDistance));
    public static readonly VectorMetric Jaccard = new("jaccard", "<%>", nameof(VectorDbFunctionsExtensions.JaccardDistance));
    public string Op { get; }
    public string Fn { get; }
    private VectorMetric(string key, string op, string fn) : this(key) => (Op, Fn) = (op, fn);

    // The [V6] server-side ANN leg realized: the typed `ORDER BY` projection over the mapped column — the
    // translator emits `column <op> probe` server-side, so the LINQ row and the raw-CTE `Op` leg drive the
    // SAME index through two doors, never a client-side distance call.
    public Expression Order(Expression column, float[] probe) =>
        Expression.Call(typeof(VectorDbFunctionsExtensions), Fn, Type.EmptyTypes, column, Expression.Constant(new Pgvector.Vector(probe)));
}

// The bounded iterative-scan vocabulary (pgvector 0.8 GUC values): `Off` disables, `RelaxedOrder` admits
// slightly out-of-order results for filtered-scan throughput, `StrictOrder` preserves exact distance order —
// hnsw-only; ivfflat admits `off`/`relaxed_order`, so its binder row never emits `StrictOrder`. A raw scan-mode
// string a typo silently degrades is the deleted form — the `SpatialOp`/`JsonComparison` typed-row precedent.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScanOrder {
    public static readonly ScanOrder Off = new("off");
    public static readonly ScanOrder RelaxedOrder = new("relaxed_order");
    public static readonly ScanOrder StrictOrder = new("strict_order");
}

// Query-time tuning as ONE optional record — an absent option emits no SET LOCAL row (the server default
// rules), so the binder carries zero fabricated knobs and each backend row reads only the fields it owns.
public readonly record struct SearchTuning(
    Option<int> EfSearch,
    Option<ScanOrder> IterativeScan,
    Option<int> MaxScanTuples,
    Option<double> ScanMemMultiplier,
    Option<int> Probes,
    Option<int> MaxProbes,
    Option<int> SearchListSize,
    Option<int> Rescore) {
    public static readonly SearchTuning Default = new(None, None, None, None, None, None, None, None);
}

// The ANN residence axis — residency is ROW DATA (DECISION [V6]): in-PG planner routes, the in-process ADC
// hot-set lane, and the scale-out provider are rows on ONE axis, each carrying its route name (the fact
// discriminant), its in-PG flag (whether the SET LOCAL binder applies), and its Bind delegate deriving the
// row's own GUC statements from the tuning value. A new residence is one row; a per-backend service family
// or an unobservable exact-scan degrade is the deleted form.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VectorBackend {
    public static readonly VectorBackend ExactScan = new("exact", "seq-scan", inPostgres: true, static _ => []);
    public static readonly VectorBackend Hnsw = new("hnsw", "pgvector-hnsw", inPostgres: true, static t => toSeq(new[] {
        t.EfSearch.Map(static v => ("hnsw.ef_search", v.ToString(CultureInfo.InvariantCulture))),
        t.IterativeScan.Map(static v => ("hnsw.iterative_scan", v.Key)),
        t.MaxScanTuples.Map(static v => ("hnsw.max_scan_tuples", v.ToString(CultureInfo.InvariantCulture))),
        t.ScanMemMultiplier.Map(static v => ("hnsw.scan_mem_multiplier", v.ToString(CultureInfo.InvariantCulture))) }).Somes());
    public static readonly VectorBackend IvfFlat = new("ivfflat", "pgvector-ivfflat", inPostgres: true, static t => toSeq(new[] {
        t.Probes.Map(static v => ("ivfflat.probes", v.ToString(CultureInfo.InvariantCulture))),
        t.MaxProbes.Map(static v => ("ivfflat.max_probes", v.ToString(CultureInfo.InvariantCulture))),
        t.IterativeScan.Map(static v => ("ivfflat.iterative_scan", (v == ScanOrder.StrictOrder ? ScanOrder.RelaxedOrder : v).Key)) }).Somes());
    public static readonly VectorBackend DiskAnn = new("diskann", "vectorscale-diskann", inPostgres: true, static t => toSeq(new[] {
        t.SearchListSize.Map(static v => ("diskann.query_search_list_size", v.ToString(CultureInfo.InvariantCulture))),
        t.Rescore.Map(static v => ("diskann.query_rescore", v.ToString(CultureInfo.InvariantCulture))) }).Somes());
    public static readonly VectorBackend PqAdc = new("pq-adc", "in-process-adc", inPostgres: false, static _ => []);
    public static readonly VectorBackend QdrantScaleout = new("qdrant", "qdrant-scaleout", inPostgres: false, static _ => []);

    public string RouteName { get; }
    public bool InPostgres { get; }
    private VectorBackend(string key, string routeName, bool inPostgres, Func<SearchTuning, Seq<(string Guc, string Value)>> bind) : this(key) =>
        (RouteName, InPostgres, Bindings) = (routeName, inPostgres, bind);
    private Func<SearchTuning, Seq<(string Guc, string Value)>> Bindings { get; }
    public Seq<(string Guc, string Value)> Bind(SearchTuning tuning) => Bindings(tuning);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class SearchRoute {
    // Transaction-scoped scan policy: SET LOCAL rows so lanes with different recall/latency stances share one
    // pool (the pgvector-catalog law); values are typed ints/strings off the tuning record, never raw caller SQL.
    public static Seq<string> SetLocal(VectorBackend route, SearchTuning tuning) =>
        route.InPostgres ? route.Bind(tuning).Map(static b => $"SET LOCAL {b.Guc} = {b.Value}") : Seq<string>();
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                     |
| :-----: | :------------------ | :------------------------------------- | :------------------------------------------------------------ |
|  [01]   | residence           | `VectorBackend` row data               | exact/hnsw/ivfflat/diskann/pq-adc/qdrant on ONE axis          |
|  [02]   | scan tuning         | `SET LOCAL` GUC binder per row         | query-time only; the `WITH` build map stays provisioning's    |
|  [03]   | route observability | `search.vector.route` fact             | a degradation is evidence, never a silent slowdown            |
|  [04]   | server-side ANN     | `VectorMetric.Order` `Expression.Call` | the `<->`/`<=>` LINQ leg composed, never an unrealized string |
|  [05]   | scale-out ceiling   | `QdrantClient.QueryAsync` provider row | deployment data; in-PG stays the default residence            |

## [03]-[LEXICAL_ALGEBRA]

- Owner: `Bm25Predicate` the closed `[Union]` projecting the `pg_search` v2 `pdb` surface — one case per `pdb.*` builder, per bare match operator, and per stacking cast modifier — whose `Sql(column)` switch emits the exact server SQL; `SearchProjection` the static score/snippet/aggregate projection surface; `LexicalRank` the `[SmartEnum<string>]` two-row rank axis carrying the BM25 arm and the native `ts_rank` fallback arm.
- Cases: builders `Parse | Match | RangeTerm | PhrasePrefix | MoreLikeThis | Regex | All` (right of `@@@`), bare operators `AnyToken`(`|||`) `| AllToken`(`&&&`) `| ExactTerm`(`===`) `| Phrase`(`###`) `| Proximity`(`##`/`##>` — the `Ordered` field selects the operator token), cast modifiers `Fuzzy | Boost | Const | Slop` composing over ANY inner predicate and stacking in cast order; `LexicalRank` is `Bm25` (`pdb.score(<key_field>)` over a `bm25` index) and `TsRank` (`ts_rank` over the generated tsvector — the degrade arm a profile without `pg_search` preloaded selects).
- Entry: `public string Sql(string column)` on `Bm25Predicate` switches the union to the exact match expression (`col @@@ pdb.parse(…)`, `col ||| '…'`, `col @@@ ('a' ##> 2 ##> 'b')`, `<inner>::pdb.fuzzy(…)`); `SearchProjection.Score(keyColumn)`/`Snippet`/`Snippets`/`SnippetPositions`/`Agg` emit the `[05]` projection functions anchored on the index `key_field`; `LexicalRank.Rank(keyColumn, terms)` emits the row's rank projection so the fusion CTE composes either arm through one call.
- Auto: the cast modifiers STACK in cast order (`'<term>'::pdb.fuzzy(2)::pdb.boost(2)` applies typo tolerance then a score multiplier) because each cast case wraps its `Inner` and appends its own cast — composition is structural, never string concatenation at the call site; analyzed matching has two spellings the union keeps distinct — the per-field `pdb.match` builder carrying its own fuzzy `distance`/`prefix` (the `Match` case) and the bare `|||`/`&&&` column operators (the `AnyToken`/`AllToken` cases); the BM25 branch matches `corpus @@@ pdb.parse($terms)` and orders by `pdb.score(<key_field>)` — the index's declared `key_field` anchor, the content key the fusion re-queries the row store by, so the fusion projects IDENTITIES rather than re-materializing candidate payloads; every projection rides `FromSql`/`SqlQuery` raw SQL because `bm25` carries no EF translator.
- Receipt: a lexical rank rides the `#FUSION_AND_REUSE` `store.fusion.rank` branch lineage — the `RetrievalBranch.Lexical` row names which arm ranked (its `Index` reads `bm25` or the tsvector GIN), so the degrade is visible in the fused result's lineage.
- Packages: `pg_search` (server-side — the `pdb` schema, `@@@`/`|||`/`&&&`/`===`/`###`/`##`/`##>` operators, `bm25` access method; AGPL confined to the PG server tier, never linked into managed code), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new `pdb` builder, operator, or cast is ONE union case in the `Sql` switch — the catalog's member set is the union's case roster, never a sibling method; a new rank arm is one `LexicalRank` row; zero new surface — a free-string BM25 predicate, a per-builder method family, or a prose-only degrade claim is the deleted form because the predicate is a closed typed tree and the fallback is a vocabulary row.
- Boundary: query values arrive PRE-ESCAPED from the search-lane binder — the `Bm25Predicate` constructors carry already-bound columns and terms, never raw runtime input (the `api-pg-search` composition law), and the `key_field` join anchor carries a `UNIQUE` constraint with exactly one `bm25` index per table; the index DDL lands via raw `MigrationBuilder.Sql` on the EF migration rail (`Element/identity#SCHEMA_VERDICT`) — this owner emits QUERY SQL only; the degrade is a CLOSED arm, not a fault — a profile without `pg_search` preloaded selects `LexicalRank.TsRank` inside the same fusion CTE (`websearch_to_tsquery` the only parser admitted to user text), so the fused result stays correct at reduced lexical power and the arm taken is branch-lineage evidence; `pdb.agg` is the Elasticsearch-style facet projection — an aggregate over the matched set, composed as a projection column, never a second aggregation engine beside the columnar lane.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The C# projection of the pg_search v2 surface (api-pg-search [03]/[04]): one case per pdb.* builder, bare
// operator, and stacking cast. `Sql` emits the EXACT server spelling; constructors carry pre-bound terms
// (the search-lane binder escapes), so no case ever interpolates raw runtime input.
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

    // The one lowering: builders compose `col @@@ pdb.*`, bare operators emit the column operator, proximity
    // composes inside `@@@`, and the four casts wrap ANY inner and STACK in cast order — structural composition,
    // never call-site string concatenation.
    public string Sql(string column) => Switch(
        parse:        p => $"{column} @@@ pdb.parse('{p.Query}', lenient => {Bool(p.Lenient)}, conjunction_mode => {Bool(p.Conjunction)})",
        match:        m => $"{column} @@@ pdb.match('{m.Query}'{m.Distance.Map(static d => $", distance => {d}").IfNone(string.Empty)}, prefix => {Bool(m.Prefix)}, conjunction_mode => {Bool(m.Conjunction)})",
        rangeTerm:    r => $"{column} @@@ pdb.range_term('{r.Value}', relation => '{r.Relation}', range_type => '{r.RangeType}')",
        phrasePrefix: p => $"{column} @@@ pdb.phrase_prefix(ARRAY[{string.Join(", ", p.Terms.Map(static t => $"'{t}'"))}]{p.MaxExpansions.Map(static n => $", max_expansions => {n}").IfNone(string.Empty)})",
        moreLikeThis: m => $"{column} @@@ pdb.more_like_this('{m.DocId}', fields => ARRAY[{string.Join(", ", m.Fields.Map(static f => $"'{f}'"))}]{m.MaxQueryTerms.Map(static n => $", max_query_terms => {n}").IfNone(string.Empty)})",
        regex:        r => $"{column} @@@ pdb.regex('{r.Pattern}')",
        all:          _ => $"{column} @@@ pdb.all()",
        anyToken:     a => $"{column} ||| '{a.Terms}'",
        allToken:     a => $"{column} &&& '{a.Terms}'",
        exactTerm:    e => $"{column} === '{e.Term}'",
        phrase:       p => $"{column} ### '{p.Terms}'",
        proximity:    p => $"{column} @@@ ('{p.Left}' {(p.Ordered ? "##>" : "##")} {p.Within} {(p.Ordered ? "##>" : "##")} '{p.Right}')",
        fuzzy:        f => $"{f.Inner.Sql(column)}::pdb.fuzzy({f.Distance}, {Bool(f.Prefix)}, {Bool(f.TranspositionCostOne)})",
        boost:        b => $"{b.Inner.Sql(column)}::pdb.boost({b.Factor.ToString(CultureInfo.InvariantCulture)})",
        @const:       c => $"{c.Inner.Sql(column)}::pdb.const({c.Score.ToString(CultureInfo.InvariantCulture)})",
        slop:         s => $"{s.Inner.Sql(column)}::pdb.slop({s.Distance})");

    static string Bool(bool value) => value ? "true" : "false";
}

// The relevance/highlight projection surface (api-pg-search [05]), anchored on the bm25 index key_field —
// each a raw-SQL projection column through FromSql/SqlQuery, never an EF-translated member.
public static class SearchProjection {
    public static string Score(string keyColumn) => $"pdb.score({keyColumn})";
    public static string Snippet(string column, string startTag = "<b>", string endTag = "</b>", int maxChars = 150) =>
        $"pdb.snippet({column}, start_tag => '{startTag}', end_tag => '{endTag}', max_num_chars => {maxChars})";
    public static string Snippets(string column, int limit, int offset, string sortBy = "score") =>
        $"pdb.snippets({column}, \"limit\" => {limit}, \"offset\" => {offset}, sort_by => '{sortBy}')";
    public static string SnippetPositions(string column) => $"pdb.snippet_positions({column})";
    public static string Agg(string esJson) => $"pdb.agg('{esJson}') OVER ()";
}

// The closed fallback arm: a profile without pg_search preloaded selects TsRank INSIDE the same fusion CTE —
// the fused result stays correct at reduced lexical power, the taken arm visible in branch lineage. A prose
// degrade claim with no selectable row is the illusory form this vocabulary deletes.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LexicalRank {
    public static readonly LexicalRank Bm25 = new("bm25",
        static (key, _) => $"pdb.score({key}) DESC",
        static (column, terms) => new Bm25Predicate.Parse(terms, Lenient: true, Conjunction: false).Sql(column));
    public static readonly LexicalRank TsRank = new("ts_rank",
        static (_, terms) => $"ts_rank(lexemes, websearch_to_tsquery('english', '{terms}')) DESC",
        static (_, terms) => $"lexemes @@ websearch_to_tsquery('english', '{terms}')");

    [UseDelegateFromConstructor] public partial string Rank(string keyColumn, string terms);
    [UseDelegateFromConstructor] public partial string MatchSql(string column, string terms);
}
```

| [INDEX] | [POLICY]          | [VALUE]                            | [BINDING]                                                    |
| :-----: | :---------------- | :--------------------------------- | :----------------------------------------------------------- |
|  [01]   | lexical predicate | `Bm25Predicate` closed union       | one case per builder/operator/cast; `Sql()` the one lowering |
|  [02]   | cast stacking     | modifier cases wrap `Inner`        | `::pdb.fuzzy(...)::pdb.boost(...)` in cast order, structural |
|  [03]   | score anchor      | `pdb.score(<key_field>)`           | identities projected; payloads never re-materialized         |
|  [04]   | degrade           | `LexicalRank.TsRank` row           | same CTE, `websearch_to_tsquery` only; visible in lineage    |
|  [05]   | escaping          | constructors carry pre-bound terms | never raw runtime input into the SQL switch                  |

## [04]-[VECTOR_CODEBOOK]

- Owner: `ProductCodebook` the product-quantization codebook value-object the `Model/embedding#EMBEDDING` Compute lane encodes against (subspace count, the flat `[subspace][code][dim]` centroid grid, code width, and a content `Id`); `VectorRow` the content-keyed fine-form-plus-codes residence the rerank and the ADC scan read; `RetrievalFault` the closed `Expected`-band (8410) the codebook admission rejections rail; `VectorCodebook` the static surface owning the per-subspace k-means TRAINING and the amortized asymmetric-distance corpus scan; `VectorIndex` the composition-supplied port carrier owning the codebook supply, the coarse-survivor fine-form resolve, and the PQ-coded corpus read — the ANN index residence read by reference, never embedded.
- Cases: `ProductCodebook.Centroid(subspace, code)` slices the one flat buffer at `(subspace · CodesPerSubspace + code) · SubspaceDim` so a `Subspaces × CodesPerSubspace` grid lives in one allocation and `Dimension = Subspaces · SubspaceDim`; `VectorRow.Encoding` is the fine encoding TAG as a string (`float32`/`int8-scalar`) and `VectorRow.Subject` is the optional `NodeId` the vector embeds (the fusion vector branch), so the residence holds no Compute type; `RetrievalFault` is `EmptyCorpus`(8410) `| Layout`(8411) `| Ragged`(8412) `| Undersized`(8413) — the four admission rejections that were bare `Error.New(8360..8363)` in the pre-split lane codebook, now the typed band — plus `Rejected`(8414), the string-bearing generator-text case `Create` mints so a rendered validation message is never discarded; the ADC distance is the per-row sum of one precomputed query→centroid table indexed by the row's PQ codes.
- Entry: `public static Fin<ProductCodebook> Train(Seq<ReadOnlyMemory<float>> corpus, int subspaces, int codesPerSubspace, int iterations)` runs Lloyd's k-means per subspace and content-keys the result; `public static Seq<(UInt128 ContentKey, float Distance)> AdcScan(ReadOnlyMemory<float> query, ProductCodebook codebook, Seq<VectorRow> coded, int top)` precomputes the query→centroid table once and bounded-top-K scans the coded corpus; `VectorIndex.Codebook(UInt128 id)` supplies the settled codebook to Compute, `VectorIndex.Publish(ProductCodebook)` records it content-keyed, `VectorIndex.Resolve(Seq<UInt128> survivors)` reads the coarse survivors' fine forms, `VectorIndex.Coded(UInt128 codebookId, int limit)` reads the PQ-coded corpus for the scan.
- Auto: `Train` rejects an empty/ragged corpus, a dimension not divisible by `subspaces`, and a corpus smaller than `codesPerSubspace` (which would leave trailing centroid slots untrained at zero) to the typed `RetrievalFault` rail, slices each corpus vector's subspace window, seeds the centroid grid from the first `codesPerSubspace` sub-vectors (Forgy initialization), and iterates assignment (nearest centroid by `TensorPrimitives.Distance`) and mean recompute (`TensorPrimitives.Add` accumulate, `TensorPrimitives.Divide` by the cluster count) — the SAME `TensorPrimitives.Distance` the Compute `EncodeProduct` assigns with, so train-time and encode-time partitions agree bit-for-bit — then mints the content `Id` over the layout ints plus the centroid bytes through the seed-zero `XxHash128` streaming fold; `AdcScan` builds the `Subspaces × CodesPerSubspace` table by `TensorPrimitives.Distance` of each query sub-vector against every centroid ONCE, then folds each coded row to the sum of its per-subspace table lookups and keeps the nearest `top` through a bounded `PriorityQueue` heap keyed on ascending distance (never a full sort, never a per-row centroid-distance recompute — the table amortizes it); `Probe` projects the float32 fine bytes onto the `Pgvector.Vector` ANN column the HNSW/diskann index residence is built over.
- Receipt: a codebook train rides `store.vector.train` carrying the subspace and code counts and the content `Id`; an ADC scan rides `store.vector.adc` carrying the corpus cardinality and the top; a fine-form resolve rides `store.vector.resolve` carrying the survivor count; the candidate recall/latency is the upstream Compute embedding owner's measured concern, never re-emitted here.
- Packages: System.Numerics.Tensors (`TensorPrimitives.Distance`/`Add`/`Divide`), System.IO.Hashing (`XxHash128` streaming `Append`/`GetCurrentHashAsUInt128`, seed zero — the kernel growth-row streaming member for a preimage that outgrows a one-shot span), Pgvector (`Vector`), Rasm.Element (`NodeId`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a re-trained codebook is one `Train` call whose new content `Id` re-keys every `product-quantized` artifact that depends on it; a new fine encoding is one `VectorRow.Encoding` tag string the Compute boundary resolves to its `VectorEncoding` row; a new admission rejection is one `RetrievalFault` case in-band 841x; a richer ANN route is one `#SEARCH_PROVISIONING_PROBE` `VectorBackend` row, never a managed scan; zero new surface — a Compute-side codebook fit, a second recency horizon, a per-encoding residence row, or an `EmbeddingVector`/`VectorEncoding` Compute type leaking into the residence is the deleted form because the codebook trains HERE, the encoding crosses as a string, and the index is read by reference.
- Boundary: `ProductCodebook` is the ONE PQ vocabulary the seam shares — Compute imports it by its `Rasm.Persistence (project)` reference and does nearest-centroid encode and centroid-reconstruction decode over it but NEVER fits it, so defining it in Compute would force a `Persistence → Compute` cycle (the dependency runs `Compute → Persistence` only) and a Compute-side k-means is the named drift defect; training uses the SAME `TensorPrimitives.Distance` Compute assigns with so the partition a centroid grid induces at train time and at encode time is identical, and the codebook is supplied content-keyed so a re-train mints a fresh `Id` that re-keys every dependent `product-quantized` artifact (the `Model/embedding#EMBEDDING` content key folds the codebook `Id`); the two-stage retrieval is honest — the `binary-hamming` coarse gate (Compute) returns content keys, `Resolve` reads the survivors' `int8-scalar`/`float32` fine forms by content key, and the Compute `Rank` reranks over those fine forms, so the magnitude a 1-bit encoding discards is recovered from the stored fine residence and never faked from the ±1 decode; the amortized ADC scan is Persistence's because this lane owns the index traversal and the `#FUSION_AND_REUSE` recency-bounded reuse while the BOUNDED rerank over the resolved survivors is Compute's, so the query→centroid table is built once and reused across the whole corpus and a per-candidate centroid-distance recompute is the deleted form; the vector branch (the ADC or in-PG HNSW ranked rows mapped through `VectorRow.Subject` to `NodeId`s) feeds `#FUSION_AND_REUSE` `FusionRank.Fuse` as one ranked branch, and the `Probe` `vector(N)` column is the same pgvector store type the `Element/identity#ELEMENT_IDENTITY` `Embedding` per-model locator rides (the corpus-grain retrieval index here, the per-model envelope locator there — two grains, never one duplicated index); the residence holds the encoding as a string and the optional `NodeId` only, no `EmbeddingVector`/`VectorEncoding`/`VectorScore` Compute type, so the strata dependency stays one-directional exactly as the `#FUSION_AND_REUSE` and `Query/cache#MODEL_RESULT_INDEX` owners keep it.

```csharp signature
// --- [ERRORS] -----------------------------------------------------------------------------
// The MINTED retrieval band (841x, DECISION [V4]/[V5b]): a [Union] over the KERNEL `Rasm.Domain.Expected`
// (parameterless protected ctor; `Category` virtual), the SAME federation base every Persistence band realizes —
// killing the pre-split lane codebook's bare `Error.New(8360..8363)`, the only un-banded lane owner. Cases lift
// BARE onto the Fin rail through the Expected derivation; band membership derives `Code => FaultBand.Retrieval + n` through the one `Element/graph#FAULT_TABLES` registry pointer (841x).
[Union]
public abstract partial record RetrievalFault : Expected, IValidationError<RetrievalFault> {
    private RetrievalFault() : base() { }
    public sealed record EmptyCorpus : RetrievalFault;
    public sealed record Layout(int Dimension, int Subspaces, int Codes) : RetrievalFault;
    public sealed record Ragged(int Expected, int Found) : RetrievalFault;
    public sealed record Undersized(int Corpus, int Codes) : RetrievalFault;
    public sealed record Rejected(string Detail) : RetrievalFault;

    public override int Code => FaultBand.Retrieval + Switch(
        emptyCorpus: static _ => 0,
        layout:      static _ => 1,
        ragged:      static _ => 2,
        undersized:  static _ => 3,
        rejected:    static _ => 4);

    public override string Message => Switch(
        emptyCorpus: static _ => "<codebook-empty-corpus>",
        layout:      static c => $"<codebook-layout:{c.Dimension}/{c.Subspaces}@{c.Codes}>",
        ragged:      static c => $"<codebook-ragged:{c.Expected}!={c.Found}>",
        undersized:  static c => $"<codebook-undersized:{c.Corpus}<{c.Codes}>",
        rejected:    static c => $"<retrieval-rejected:{c.Detail}>");

    public override string Category => Switch(
        emptyCorpus: static _ => "EmptyCorpus",
        layout:      static _ => "Layout",
        ragged:      static _ => "Ragged",
        undersized:  static _ => "Undersized",
        rejected:    static _ => "Rejected");

    // The string-bearing generator-text case: `Create` PRESERVES the rendered message — a structured case
    // minted with zeroed fields would erase the one piece of evidence the generator hands over.
    public static RetrievalFault Create(string message) => new Rejected(message);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record ProductCodebook(
    int Subspaces,
    int SubspaceDim,
    int CodesPerSubspace,
    ReadOnlyMemory<float> Centroids,
    UInt128 Id) {
    public int Dimension => Subspaces * SubspaceDim;

    public ReadOnlySpan<float> Centroid(int subspace, int code) =>
        Centroids.Span.Slice((subspace * CodesPerSubspace + code) * SubspaceDim, SubspaceDim);

    // The content Id streams layout ints + centroid bytes through the seed-zero XxHash128 fold — the kernel
    // streaming growth-row member, because a centroid grid outgrows a one-shot span preimage.
    public static UInt128 KeyOf(int subspaces, int subspaceDim, int codesPerSubspace, ReadOnlySpan<float> centroids) {
        var hash = new XxHash128();
        Span<byte> layout = stackalloc byte[12];
        BinaryPrimitives.WriteInt32LittleEndian(layout[..4], subspaces);
        BinaryPrimitives.WriteInt32LittleEndian(layout[4..8], subspaceDim);
        BinaryPrimitives.WriteInt32LittleEndian(layout[8..], codesPerSubspace);
        hash.Append(layout);
        hash.Append(MemoryMarshal.AsBytes(centroids));
        return hash.GetCurrentHashAsUInt128();
    }

    public static ProductCodebook Of(int subspaces, int subspaceDim, int codesPerSubspace, ReadOnlyMemory<float> centroids) =>
        new(subspaces, subspaceDim, codesPerSubspace, centroids, KeyOf(subspaces, subspaceDim, codesPerSubspace, centroids.Span));
}

public readonly record struct VectorRow(
    UInt128 ContentKey,
    string Encoding,
    int Dimension,
    ReadOnlyMemory<byte> Fine,
    ReadOnlyMemory<byte> Codes,
    UInt128 CodebookId,
    Option<NodeId> Subject) {
    // The float32 fine bytes projected onto the pgvector ANN column the HNSW/diskann index is built over;
    // valid for the float32 fine row, the exact-rerank ground truth the coarse gate resolves to.
    public Pgvector.Vector Probe() => new(MemoryMarshal.Cast<byte, float>(Fine.Span).ToArray());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class VectorCodebook {
    public static Fin<ProductCodebook> Train(Seq<ReadOnlyMemory<float>> corpus, int subspaces, int codesPerSubspace, int iterations) {
        if (corpus.IsEmpty) { return Fin.Fail<ProductCodebook>(new RetrievalFault.EmptyCorpus()); }
        int dimension = corpus[0].Length;
        if (subspaces <= 0 || dimension % subspaces != 0 || codesPerSubspace is <= 0 or > 256) {
            return Fin.Fail<ProductCodebook>(new RetrievalFault.Layout(dimension, subspaces, codesPerSubspace));
        }
        if (corpus.Exists(vector => vector.Length != dimension)) {
            return Fin.Fail<ProductCodebook>(new RetrievalFault.Ragged(dimension, corpus.Filter(v => v.Length != dimension).Head.Map(static v => v.Length).IfNone(0)));
        }
        // A corpus smaller than the codebook leaves trailing centroid slots untrained at zero, which the ADC
        // table would then treat as real centroids — a silent-quality defect. Reject it rather than ship a
        // half-trained codebook: every one of the `codesPerSubspace` slots must be seeded from a real vector.
        if (corpus.Count < codesPerSubspace) { return Fin.Fail<ProductCodebook>(new RetrievalFault.Undersized(corpus.Count, codesPerSubspace)); }
        int subDim = dimension / subspaces;
        var centroids = new float[subspaces * codesPerSubspace * subDim];
        for (int subspace = 0; subspace < subspaces; subspace++) {
            Lloyd(corpus, subspace, subDim, codesPerSubspace, Math.Max(1, iterations), centroids.AsSpan(subspace * codesPerSubspace * subDim, codesPerSubspace * subDim));
        }
        return Fin.Succ(ProductCodebook.Of(subspaces, subDim, codesPerSubspace, centroids));
    }

    public static Seq<(UInt128 ContentKey, float Distance)> AdcScan(ReadOnlyMemory<float> query, ProductCodebook codebook, Seq<VectorRow> coded, int top) {
        if (top <= 0 || coded.IsEmpty) { return Seq<(UInt128 ContentKey, float Distance)>(); }
        var table = new float[codebook.Subspaces * codebook.CodesPerSubspace];
        for (int subspace = 0; subspace < codebook.Subspaces; subspace++) {
            var part = query.Span.Slice(subspace * codebook.SubspaceDim, codebook.SubspaceDim);
            for (int code = 0; code < codebook.CodesPerSubspace; code++) {
                table[subspace * codebook.CodesPerSubspace + code] = TensorPrimitives.Distance(part, codebook.Centroid(subspace, code));
            }
        }
        var heap = new PriorityQueue<(UInt128 ContentKey, float Distance), float>(top);
        foreach (var row in coded) {
            var codes = row.Codes.Span;
            float distance = 0f;
            for (int subspace = 0; subspace < codes.Length; subspace++) { distance += table[subspace * codebook.CodesPerSubspace + codes[subspace]]; }
            if (heap.Count < top) { heap.Enqueue((row.ContentKey, distance), -distance); }
            else if (heap.TryPeek(out _, out float worst) && -distance > worst) { heap.EnqueueDequeue((row.ContentKey, distance), -distance); }
        }
        int kept = heap.Count;
        var ordered = new (UInt128 ContentKey, float Distance)[kept];
        for (int slot = kept - 1; slot >= 0; slot--) { ordered[slot] = heap.Dequeue(); }
        return toSeq(ordered);
    }

    // Lloyd's k-means over ONE subspace window: TensorPrimitives.Distance assigns, Add/Divide recomputes the
    // cluster mean; the destination span is the codebook's [subspace] slab. A loop is the named span-kernel
    // exemption (no TensorPrimitives member argmin-assigns a centroid grid), mirroring Compute's EncodeProduct.
    static void Lloyd(Seq<ReadOnlyMemory<float>> corpus, int subspace, int subDim, int codes, int iterations, Span<float> centroids) {
        int offset = subspace * subDim;
        for (int code = 0; code < codes; code++) { corpus[code].Span.Slice(offset, subDim).CopyTo(centroids.Slice(code * subDim, subDim)); }
        var sums = new float[codes * subDim];
        var counts = new int[codes];
        for (int iteration = 0; iteration < iterations; iteration++) {
            Array.Clear(sums);
            Array.Clear(counts);
            foreach (var vector in corpus) {
                var part = vector.Span.Slice(offset, subDim);
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
    Func<UInt128, IO<Option<ProductCodebook>>> ResolveCodebook,
    Func<ProductCodebook, IO<Unit>> RecordCodebook,
    Func<Seq<UInt128>, IO<Seq<VectorRow>>> ResolveFine,
    Func<UInt128, int, IO<Seq<VectorRow>>> ResolveCoded) {
    public IO<Option<ProductCodebook>> Codebook(UInt128 id) => ResolveCodebook(id);
    public IO<Unit> Publish(ProductCodebook codebook) => RecordCodebook(codebook);
    public IO<Seq<VectorRow>> Resolve(Seq<UInt128> survivors) => ResolveFine(survivors);
    public IO<Seq<VectorRow>> Coded(UInt128 codebookId, int limit) => ResolveCoded(codebookId, limit);
}
```

| [INDEX] | [POLICY]            | [VALUE]                                  | [BINDING]                                                      |
| :-----: | :------------------ | :--------------------------------------- | :------------------------------------------------------------- |
|  [01]   | codebook owner      | `Train` here; Compute encodes only       | a Compute-side fit would force a `Persistence → Compute` cycle |
|  [02]   | partition agreement | the SAME `TensorPrimitives.Distance`     | train-time and encode-time centroids agree bit-for-bit         |
|  [03]   | codebook supply     | content-keyed by `Id`, read by reference | a re-train re-keys every `product-quantized` artifact          |
|  [04]   | coarse→fine rerank  | `Resolve` reads `int8`/`float32` fine    | magnitude recovered from the residence, never faked            |
|  [05]   | ADC amortization    | one query→centroid table per scan        | reused across the corpus; never a per-row recompute            |
|  [06]   | admission rail      | `RetrievalFault` 841x                    | the pre-split bare `Error.New(8360..8363)` is dead             |
|  [07]   | strata one-way      | encoding string + `NodeId` only          | no `EmbeddingVector`/`VectorEncoding` Compute type leaks down  |

## [05]-[FUSION_AND_REUSE]

- Owner: `RetrievalBranch` the `[SmartEnum<string>]` typed branch axis (vector ANN / spatial GiST / lexical BM25) carrying each branch's in-PG `Index` name and RRF `Weight`; `FusionHit` the per-element fused rank carrying its typed branch contributions; `FusionRank` the weighted n-ary reciprocal-rank-fusion surface with the canonical `RrfConstant = 60` default; `ResultCache` the read-through `HybridCache` tier keyed on the content-addressed `ElementSet.Receipt`; `RetrievalOp` the request `[Union]` and `Retrieval` the ONE polymorphic entry dispatching it.
- Cases: `RetrievalOp` is `Fuse(Seq<(RetrievalBranch, Seq<NodeId>)> Branches, int RrfK)` `| Train(Seq<ReadOnlyMemory<float>> Corpus, int Subspaces, int CodesPerSubspace, int Iterations)` `| AdcScan(ReadOnlyMemory<float> Query, ProductCodebook Codebook, Seq<VectorRow> Coded, int Top)`; `RetrievalResult` is `Fused(Seq<FusionHit>)` `| Trained(ProductCodebook)` `| Scanned(Seq<(UInt128, float)>)`; `RetrievalBranch` rows are `Vector`/`Spatial`/`Lexical` each carrying `Index` and `Weight`.
- Entry: `public static Fin<RetrievalResult> Run(RetrievalOp op)` dispatches the closed op family through the generated total `Switch` — fuse folds `FusionRank.Fuse`, train rails `VectorCodebook.Train` through the `RetrievalFault` band, scan folds `VectorCodebook.AdcScan` — one entrypoint per MODAL_ARITY, the op case the discriminant; `public static Seq<FusionHit> Fuse(Seq<(RetrievalBranch Branch, Seq<NodeId> Ranked)> branches, int k = RrfConstant)` folds the per-branch ranked lists into one weighted reciprocal-rank result whose lineage names the typed branch; `public static IO<T> ResultCache.Cached<TState, T>(ElementSet subject, string lane, TState state, Func<TState, CancellationToken, ValueTask<T>> produce, HybridCache cache, Duration ttl)` read-through-caches the expensive DERIVED retrieval (the branch index queries plus the fusion) under the already-evaluated subject selection's receipt with a receipt-derived invalidation tag — never the selection under its own receipt, the circular key that saves nothing.
- Auto: the fusion is the one weighted n-ary RRF fold `Score(e) = Σ_b branch.Weight / (k + rank_b(e))` over each TYPED branch's ranked list — `k = 60` the canonical reciprocal-rank constant, named once, never a per-call literal — so a vector, spatial, and lexical retrieval combine into one ranking with per-branch lineage that names which index ranked the hit: the `Vector` branch orders by a `VectorMetric` (`Op` for the raw-CTE leg, `Order` for the EF-translated leg) over an `EmbeddingArity` column under the routed `#SEARCH_PROVISIONING_PROBE` `VectorBackend`, the `Spatial` branch by a PostGIS GiST `ST_*` predicate, the `Lexical` branch by the `#LEXICAL_ALGEBRA` `LexicalRank` arm (BM25 or the ts_rank degrade) — the same fold a two-branch and a three-branch retrieval both ride; the subject selection is evaluated ONCE (its `ElementSet` carries its own receipt), the cache keys the DERIVED retrieval on `ElementSet.Receipt` so a re-run over an unchanged selection against an unchanged graph reuses the fused result without re-touching an index, and the receipt-derived `tags` feed `RemoveByTagAsync` so an op-log change to a contributing node invalidates the cache through the `Version/ledger#CHANGEFEED` tag-cut.
- Receipt: a fusion rides `store.fusion.rank` carrying the branch count and the fused cardinality; a cache hit rides `store.cache.hit`, a miss `store.cache.produce`; the routed vector branch's backend rides the `#SEARCH_PROVISIONING_PROBE` `search.vector.route` fact.
- Packages: Microsoft.Extensions.Caching.Hybrid (`HybridCache.GetOrCreateAsync`/`HybridCacheEntryOptions`/`RemoveByTagAsync`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new retrieval branch is one `RetrievalBranch` row carrying its index name and weight plus one ranked list into the same `Fuse` fold; a new retrieval modality is one `RetrievalOp` case whose arm breaks `Run` loudly at compile time; zero new surface — a per-branch-pair fusion, a bespoke score blend, a positional `branch-{b}` string lineage, a free-string cache tag, or a sibling `FuseMany`/`TrainAndScan` entrypoint is the deleted form because the RRF is one weighted n-ary fold over the typed branch axis and the op union owns modality.
- Boundary: this owner is the search-lane binding the pgvector/pg_search/pgvectorscale/qdrant `.api` catalogs compose against — a catalogue's `VectorMetric`/`EmbeddingArity`/`Bm25Predicate`/RRF reference resolves here, never a parallel saved-search owner; the fusion is the one weighted n-ary RRF fold over the typed `RetrievalBranch` axis so a hit's lineage names the index that ranked it — a bespoke per-pair blend or a positional string branch label is the deleted form; the cache is the AppHost `HybridCache` port keyed on the content-addressed `ElementSet.Receipt` (minted by `Query/lane#ELEMENT_SET_ALGEBRA`, a lane-owned identity this page never re-derives) with a receipt-derived tag, so a free-string tag rejects at admission because it is uninvalidatable by construction, and this SELECTION-RESULT cache is a DIFFERENT owner from `Query/cache`'s compute-result reuse index (`ArtifactIndexRow`/`ModelResultIndex`) — the fusion result seam feeds cache's index rows, never merges with them; spatial→PG GiST and ANN→pgvector are the index owners (DuckDB spatial/vss being the columnar aggregator only, not the transactional index), so the fusion branches read the federated row's GiST/HNSW/tsvector columns and never duplicate the index, the vector branch resolving through the `#VECTOR_CODEBOOK` `VectorRow.Subject`-mapped ranked rows.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The TYPED retrieval-branch axis — the fusion's branch identity is a vocabulary value, never a positional
// `branch-{b}` string. Each row names the in-PG index it reads (`Index`) and its RRF `Weight`; the vector
// branch's ACTUAL residence rides the routed `VectorBackend`, this row naming the branch class.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetrievalBranch {
    public static readonly RetrievalBranch Vector = new("vector", "pgvector-hnsw", 1.0);
    public static readonly RetrievalBranch Spatial = new("spatial", "postgis-gist", 1.0);
    public static readonly RetrievalBranch Lexical = new("lexical", "pg_search-bm25", 1.0);
    public string Index { get; }
    public double Weight { get; }
    private RetrievalBranch(string key, string index, double weight) : this(key) => (Index, Weight) = (index, weight);
}

public readonly record struct FusionHit(NodeId Key, double Score, Seq<(RetrievalBranch Branch, int Rank)> Lineage);

// The one retrieval request family (DECISION [V5b]): fuse + train + scan are CASES on one entry, never
// sibling entrypoints — a new retrieval modality is one case that breaks `Retrieval.Run` at compile time.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetrievalOp {
    private RetrievalOp() { }
    public sealed record Fuse(Seq<(RetrievalBranch Branch, Seq<NodeId> Ranked)> Branches, int RrfK = FusionRank.RrfConstant) : RetrievalOp;
    public sealed record Train(Seq<ReadOnlyMemory<float>> Corpus, int Subspaces, int CodesPerSubspace, int Iterations) : RetrievalOp;
    public sealed record AdcScan(ReadOnlyMemory<float> Query, ProductCodebook Codebook, Seq<VectorRow> Coded, int Top) : RetrievalOp;
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
    // The canonical reciprocal-rank constant k=60 — the standard RRF damping policy value, named ONCE as
    // the default; a per-call literal re-deciding it is the deleted form.
    public const int RrfConstant = 60;

    // Weighted n-ary reciprocal-rank fusion `Score(e) = Σ_b branch.Weight / (k + rank_b(e))` over the TYPED
    // branches: each branch carries its `RetrievalBranch` identity so a hit's lineage names which index ranked
    // it, never an anonymous positional index; the same fold serves a two-branch and a three-branch retrieval,
    // the weight a per-branch policy value rather than a blend knob. The per-branch rank is the 1-BASED list
    // position (the canonical RRF rank, so the top hit scores `Weight/(k+1)` and `k+rank` never divides by `k`
    // alone) — projected with the indexed INSTANCE `Seq<A>.Map(Func<A,int,B>)`, whose argument order is (value, index).
    public static Seq<FusionHit> Fuse(Seq<(RetrievalBranch Branch, Seq<NodeId> Ranked)> branches, int k = RrfConstant) =>
        toSeq(branches
            .Bind(static b => b.Ranked.Map((key, index) => (Key: key, b.Branch, Rank: index + 1)))
            .GroupBy(static c => c.Key)
            .Select(group => new FusionHit(
                group.Key,
                group.Sum(c => c.Branch.Weight / (k + c.Rank)),
                toSeq(group.Select(static c => (c.Branch, c.Rank)))))
            .OrderByDescending(static h => h.Score));
}

public static class ResultCache {
    // The read-through reuse tier keyed on the content-addressed receipt: the SUBJECT selection arrives already
    // evaluated (its `ElementSet` carries its own `Receipt`), and the cached VALUE is the expensive DERIVED
    // retrieval — the branch index queries plus the fusion the factory produces — so a re-run over an unchanged
    // selection reuses the fused result without re-touching an index. Caching the `ElementSet` under its OWN
    // receipt is the deleted circular form: a key derived from the result can never save the evaluation that
    // produced it. `lane` namespaces the derived kind under one receipt (a `nameof`-derived symbol, never
    // freeform); the receipt-derived tag feeds `RemoveByTagAsync` when an op-log change to a contributing node
    // cuts the logical entry; `state` threads through `GetOrCreateAsync` so the factory captures nothing and a
    // concurrent identical miss folds onto one production (the stampede gate).
    public static IO<T> Cached<TState, T>(ElementSet subject, string lane, TState state,
        Func<TState, CancellationToken, ValueTask<T>> produce, HybridCache cache, Duration ttl) {
        var key = subject.Receipt.ToString("x32", CultureInfo.InvariantCulture);
        return IO.liftAsync(() => cache.GetOrCreateAsync(
            $"{lane}:{key}",
            state,
            produce,
            new HybridCacheEntryOptions { Expiration = ttl.ToTimeSpan() },
            tags: [$"elementset:{key}"]).AsTask());
    }
}

// --- [COMPOSITION] --------------------------------------------------------------------------
public static class Retrieval {
    // ONE entry per MODAL_ARITY: the op case is the discriminant, the generated Switch total — train rails the
    // RetrievalFault band, fuse and scan are total over admitted values.
    public static Fin<RetrievalResult> Run(RetrievalOp op) => op.Switch(
        fuse:    static f => Fin.Succ<RetrievalResult>(new RetrievalResult.Fused(FusionRank.Fuse(f.Branches, f.RrfK))),
        train:   static t => VectorCodebook.Train(t.Corpus, t.Subspaces, t.CodesPerSubspace, t.Iterations)
                                 .Map(static trained => (RetrievalResult)new RetrievalResult.Trained(trained)),
        adcScan: static s => Fin.Succ<RetrievalResult>(new RetrievalResult.Scanned(VectorCodebook.AdcScan(s.Query, s.Codebook, s.Coded, s.Top))));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                   | [BINDING]                                                                                                           |
| :-----: | :----------------- | :---------------------------------------- | :------------------------------------------------------------------------------------------------------------------ |
|  [01]   | entry              | `Retrieval.Run(RetrievalOp)`              | fuse/train/scan are cases; no sibling entrypoint family                                                             |
|  [02]   | fusion             | weighted n-ary RRF over `RetrievalBranch` | typed branch lineage; `RrfConstant = 60` named once                                                                 |
|  [03]   | cache key          | content-addressed `ElementSet.Receipt`    | the DERIVED retrieval cached under the subject receipt; a result under its own receipt is the circular deleted form |
|  [04]   | cache invalidation | receipt-derived tag → `RemoveByTagAsync`  | a changefeed op-log change to a contributing node cuts it                                                           |
|  [05]   | cache identity     | selection-result reuse                    | distinct from `Query/cache`'s compute-result index                                                                  |
|  [06]   | index ownership    | GiST spatial, pgvector ANN, BM25 lexical  | DuckDB is the columnar aggregator, never the index                                                                  |
