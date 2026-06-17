# [PERSISTENCE_DATA_LANES]

Every durable shape in Rasm.Persistence rides one closed `DataLane` axis — relational, document, key-value, vector, full-text, analytical, and blob — folded against the `StoreProfile` engine rows that carry it. The page owns the lane vocabulary and its profile capability fold, the single key-value entity shape, the per-column JSON index policy, the two search query shapes with their metric and mode vocabularies, the geometry residence rows over the NetTopologySuite value chain, and the analytical read lane with its attach, shred, and export policy. Lane variance is rows, cases, and policy values consumed by the package rails; per-lane services never exist.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                |
| :-----: | --------------- | --------------------------------------------------------------------- |
|   [1]   | LANE_AXIS       | Seven-case lane union; profile capability fold; the key-value shape   |
|   [2]   | DOCUMENT_LANE   | Document mapping law, per-column JSON index policy, index asymmetry   |
|   [3]   | SEARCH_LANES    | Vector and full-text query shapes; embedding identity; provider rows  |
|   [4]   | GEO_LANES       | Geometry residence rows; container window law; driver-native concerns |
|   [5]   | ANALYTICAL_LANE | Analytical read lane; live attach; parquet, tabular, shred policy     |

## [2]-[LANE_AXIS]

- Owner: `DataLane` `[Union]` with the profile capability fold and the `KvEntry` key-value shape.
- Cases: Relational | Document | KeyValue | Vector | FullText | Analytical | Blob
- Entry: `public static Fin<DataLane> Admit(DataLane lane, StoreProfile profile)` — `Fin<T>` aborts; a structurally unsupported lane-profile pairing is a typed rejection at composition, never a provider error at first use.
- Auto: `KvEntry` rows past `ExpiresAt` leave through one registered `ScheduleEntry` sweep row.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project), BCL inbox
- Growth: a new lane is one case plus one `Profiles` arm; a new lane-profile pairing is one element in an existing arm; zero new surface.
- Boundary: lane behavior enters the package operation dispatch as provider rows — per-lane repositories, per-provider query twins, and a wrapper layer over provider functions are the deleted patterns; `KvEntry` is the one key-value shape, its single upsert case derives INSERT ON CONFLICT DO UPDATE per engine, and it backs the package L2 cache contribution; the `Classification` field makes the classification column mandatory at write, and the codec id plus content hash columns make every payload self-describing.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record DataLane {
    private DataLane() { }

    public sealed record Relational : DataLane;

    public sealed record Document : DataLane;

    public sealed record KeyValue : DataLane;

    public sealed record Vector : DataLane;

    public sealed record FullText : DataLane;

    public sealed record Analytical : DataLane;

    public sealed record Blob : DataLane;

    public static Seq<StoreProfile> Profiles(DataLane lane) =>
        lane.Switch(
            relational: static _ => Seq(StoreProfile.SqliteEmbedded, StoreProfile.SqliteMemory, StoreProfile.PostgresServer),
            document:   static _ => Seq(StoreProfile.SqliteEmbedded, StoreProfile.SqliteMemory, StoreProfile.PostgresServer),
            keyValue:   static _ => Seq(StoreProfile.SqliteEmbedded, StoreProfile.SqliteMemory, StoreProfile.PostgresServer),
            vector:     static _ => Seq(StoreProfile.PostgresServer, StoreProfile.SqliteEmbedded, StoreProfile.SqliteMemory),
            fullText:   static _ => Seq(StoreProfile.SqliteEmbedded, StoreProfile.SqliteMemory, StoreProfile.PostgresServer),
            analytical: static _ => Seq(StoreProfile.DuckDbAnalytical),
            blob:       static _ => Seq(StoreProfile.FileSnapshot, StoreProfile.BlobRemote));

    public static Fin<DataLane> Admit(DataLane lane, StoreProfile profile) =>
        Profiles(lane).Contains(profile)
            ? Fin.Succ(lane)
            : Fin.Fail<DataLane>(Error.New($"<lane-unsupported:{lane}:{profile}>"));
}

public sealed record KvEntry(
    string Key,
    string Kind,
    ReadOnlyMemory<byte> Payload,
    string CodecId,
    UInt128 ContentHash,
    long Version,
    Option<Instant> ExpiresAt,
    DataClassification Classification);
```

## [3]-[DOCUMENT_LANE]

- Owner: `JsonIndex` `[SmartEnum<string>]` per-column index policy under the `StoreKeyPolicy` ordinal accessor.
- Cases: Containment | KeyExistence | TextDocument | None
- Packages: Microsoft.EntityFrameworkCore.Sqlite, Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new document column is one `JsonIndex` policy value on its index row; a new server json representation is one row on the same enum; a server-side document-shape invariant is one `CHECK (jsonb_matches_schema(...))` row, never a parallel validator; zero new surface.
- Boundary: `ComplexProperty(x => x.Detail, d => d.ToJson())` is the only document mapping — the owned-entity ToJson spelling is the rejected form; each `JsonIndex` key denotes one index strategy and the `ColumnType` and `OpClass` columns carry the server-column and GIN-opclass facts so the key axis stays homogeneous; jsonb is the canonical `ColumnType` while `TextDocument` carries the `json` `ColumnType` that preserves textual byte order, whitespace, and duplicate keys for round-trip-fidelity documents, so a foreign-document of-record column declares `TextDocument` and an indexable decomposed column declares `Containment` or `KeyExistence`; the embedded column stays TEXT json with `json_extract` translation, so `JsonIndex.None` is the only embedded row and the lane is index-asymmetric by construction — query shapes never assume jsonpath-index parity across profiles, and `TextDocument` carries no expression-index analogue because `json` is not set-indexable; NodaTime values inside documents persist through the registered plugin converters, never a DateTime sentinel; foreign schemaless payloads enter as one JsonDocument column through exactly one boundary converter; the IFC semantic-ingest model graph from `Bim/exchange/interchange#IMPORT_RAIL` enters this lane as the document residence of the `DatabaseIfc`-extracted graph — property sets, spatial hierarchy, quantities, materials, and type objects projected as jsonb documents keyed on the `indexes#ARTIFACT_BLOB_INDEX` `IfcSemantic` content-address so a spatial-hierarchy or property-set query rides the existing `@?`/`@@` path predicates and `JSON_TABLE` shred surfaces with zero IFC-specific lane, while the property-set free-text and the spatial-name corpora feed the `#SEARCH_LANES` BM25 and trigram routes as the search residence of the same graph — Compute owns the IFC parse and graph extraction (mechanics), this lane owns the durable model-graph residence and query (consequence), the ingest never tessellates BRep geometry, and a parallel IFC-entity table family or a second loose-DOM walker is the deleted form; a document column carrying a declared shape enforces it server-side through a `CHECK (jsonb_matches_schema(<schema>, detail))` invariant over the pg_jsonschema `json_matches_schema`/`jsonb_matches_schema` functions (the `pg_jsonschema` extension declaration is owned on `schema-rail#EXTENSION_DDL`), so an out-of-shape document is rejected at write rather than at read, and where the deploy image cannot supply the compiled extension the validation moves application-side; tz-suffixed path functions never enter an expression index.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class JsonIndex {
    public static readonly JsonIndex Containment = new("containment", columnType: "jsonb", opClass: "jsonb_path_ops");
    public static readonly JsonIndex KeyExistence = new("key-existence", columnType: "jsonb", opClass: "jsonb_ops");
    public static readonly JsonIndex TextDocument = new("text-document", columnType: "json", opClass: "none");
    public static readonly JsonIndex None = new("none", columnType: "jsonb", opClass: "none");

    public string ColumnType { get; }
    public string OpClass { get; }
}
```

| [INDEX] | [SURFACE]                          | [STORE]         | [ROUTE]                | [LAW]                                                                           |
| :-----: | ---------------------------------- | --------------- | ---------------------- | ------------------------------------------------------------------------------- |
|   [1]   | jsonb document column              | postgres-server | provider mapping       | canonical document column; decomposed binary, set-indexable                     |
|   [2]   | json document column               | postgres-server | provider mapping       | text-preserving column; byte order and duplicate keys held; no expression index |
|   [3]   | `@?` / `@@` path predicates        | postgres-server | raw SQL over GIN       | containment and path predicates under the declared class                        |
|   [4]   | `jsonb_path_query` function family | postgres-server | raw SQL                | non-tz forms only inside expression indexes                                     |
|   [5]   | `JSON_TABLE` shredding             | postgres-server | FromSql projection     | SQL/JSON standard row-shred surface for set projections                         |
|   [6]   | `json_extract` translation         | sqlite rows     | LINQ translation       | TEXT json column; zero index analogue                                           |
|   [7]   | ExecuteUpdate into JSON paths      | both providers  | set-based write        | partial document update without entity materialization                          |
|   [8]   | JsonDocument escape hatch          | both providers  | one boundary converter | schemaless foreign payload admission                                            |
|   [9]   | `jsonb_matches_schema` CHECK       | postgres-server | pg_jsonschema CHECK invariant | declared-shape documents validated server-side at write; extension declared on `schema-rail#EXTENSION_DDL`; falls to application-side when the compiled extension is unavailable |

## [4]-[SEARCH_LANES]

- Owner: `VectorQuery` and `FullTextQuery` shapes with the `VectorMetric`, `FullTextMode`, `EmbeddingArity`, and `EmbeddingIdentity` vocabularies; `HybridRetrieve` is the fusion fold composing the dense/diskann vector route and the pg_search BM25 route into one reciprocal-rank-fused top-k result.
- Cases: `VectorMetric` l2 | cosine | inner-product | l1 | hamming | jaccard; `FullTextMode` match | prefix | phrase | websearch; `EmbeddingArity` dense | half | sparse | bit; `HybridRetrieve` fuses the two existing search routes — one vector branch keyed on `VectorQuery`, one BM25 branch keyed on `FullTextQuery.Bm25` — and never mints a third route.
- Entry: `public static EmbeddingIdentity Of(ReadOnlySpan<byte> content, string modelId, EmbeddingArity arity)` — pure value; embedding identity is content hash times model id times arity, so re-embedding the same content under a different precision dedupes per arity; `public static string Fuse<TRow>(VectorQuery<TRow> vector, FullTextQuery text, int k, int rrfConstant)` projects the reciprocal-rank-fusion CTE SQL that unions the two ranked candidate sets and re-ranks by `1.0 / (rrfConstant + rank)` summed across branches.
- Receipt: a `search.vector.route` fact discriminating exact-scan vs HNSW vs IVFFlat vs diskann route taken, a `search.bm25.score` fact carrying the pg_search overlay score, a `search.hybrid.fused` fact carrying the fused top-k count and the per-branch contribution split, and the always-present exact-scan baseline as the correctness fact so a route degradation is observable; all ride the interceptor fact stream under the `search.*` kind family.
- Packages: Pgvector.EntityFrameworkCore, Microsoft.Data.Sqlite, Microsoft.EntityFrameworkCore.Sqlite, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new embedding family is one row carrying model id, dimensionality, and column type; a new precision is one `EmbeddingArity` case carrying its store-type and CLR value shape; a new tokenizer or rank grammar is one policy value on its provider row; a bit-vector metric family (hamming, jaccard) is one `VectorMetric` row carrying its `Fn` `DbFunctions` member paired with one bit-probe arity on `VectorQuery`, and a new metric adds one `Order` `Switch` arm; an approximate-route family (HNSW, IVFFlat, diskann) is one row in the index-route table; a BM25 overlay is one FTS row beside the always-present native baseline; zero new surface.
- Boundary: one query shape per search concern projects to every provider — provider-twin query shapes are the deleted pattern; `EmbeddingArity` carries the four pgvector precisions as one closed axis so the column-type fact rides the arity row, never a parallel column-type enum — `Dense` maps `Pgvector.Vector`→`vector(N)`, `Half` maps `Pgvector.HalfVector`→`halfvec(N)`, `Sparse` maps `Pgvector.SparseVector`→`sparsevec(N)`, `Bit` maps `System.Collections.BitArray`→`bit(N)`, and the sparse-dict ingestion shape is the `SparseVector(Dictionary<int,float> elements, int dimensions)` constructor projecting `Dimensions`/`Indices`/`Values`; `VectorMetric.Order` is the one homogeneous fold projecting the `ORDER BY` distance `Expression` per metric row — a `Bit` false row routes through `L2Distance` (`<->`), `CosineDistance` (`<=>`), `MaxInnerProduct` (`<#>`), and `L1Distance` (`<+>`), while a `Bit` true row routes through the catalogued `HammingDistance` and `JaccardDistance` `DbFunctions` (`.api/api-pgvector-ef.md` rows [5]/[6]) over a `bit`-typed column, so the EF translator emits the bit-distance SQL from the catalogued member and the managed surface never needs the raw operator literal; the raw server operator strings these two functions emit still ride the `<bit-op-hamming>`/`<bit-op-jaccard>` placeholders on the `VectorMetric.Op` column, held open under `[BIT_VECTOR_COLUMNS]` as a tier-2 live-PG18 SPIKE because the `.api` catalogue carries the `DbFunctions` member but not the operator literal — the placeholder is read only by the `HybridRetrieve.Fuse` raw-CTE leg, never by the translated `Order` projection — so the metric axis stays one fold and a parallel bit-query shape is the deleted form, the `binary_quantize(emb)::bit(N)` expression-index form materializing the bit column from a dense embedding; the embedded vector route stays gate-resolved with the brute-force scan as the always-present correctness baseline; diskann and BM25 carry no EF translator, so their index DDL lands on `provisioning#SEARCH_PROVISIONING` via raw migration SQL while the column type stays the arity-row value and queries reuse the catalogued pgvector distance functions and the raw-SQL `@@@` operator whose right-hand side is always a `pdb.*` query builder (`corpus @@@ pdb.parse($terms)` for a free-term string, `pdb.regex`/`pdb.phrase_prefix`/`pdb.range_term`/`pdb.more_like_this` for the structured forms) — the diskann index is catalogued over a `vector` column only (`.api/api-pgvectorscale.md` `[INDEX_DDL]` column type `vector`, ops classes `vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops`), so the planner routes a `vector(N)` distance query through a diskann index transparently while the `halfvec(N)`-over-diskann column-type round-trip stays held under `[SEARCH_PROVISIONING_PROBE]`, and BM25 scoring rides `pdb.score`/`pdb.snippet` through `FromSql`/`SqlQuery` on the same boundary as `websearch_to_tsquery`/`ts_rank`/`ts_headline`; the BM25 overlay is the high-power FTS route while a profile without pg_search preloaded answers through the always-present native FTS baseline; pgvectorscale and pg_search run in-process inside the PG server tier, never linked into managed code; fuzzy admission normalizes through unaccent and prefilters through trigram similarity before rank; the embedded `Regex.IsMatch` predicate translates through `SqliteRegexMethodTranslator` so a LINQ regex filter projects to the sqlite `REGEXP` operator instead of client evaluation; the `search.vector.route` fact distinguishes exact-scan, HNSW, IVFFlat, and diskann route taken, the `search.bm25.score` fact carries the pg_search overlay score, and the always-present exact-scan baseline is the correctness fact so a route degradation is observable; `HybridRetrieve.Fuse` composes the two existing routes through one reciprocal-rank-fusion CTE — a vector branch ordered by the `VectorMetric` distance operator over the `EmbeddingArity` column and a BM25 branch matched through `corpus @@@ pdb.parse($terms)` and ordered by `pdb.score(<key_col>)` (the BM25 index's declared `key_field` join anchor) — and sums `1.0 / (rrfConstant + rank)` across the branches so the fusion needs no learned reranker model, never a third index or a parallel query shape, and the dense embedding the vector branch probes is generated upstream at `Compute/models#INFERENCE_MODES` `Embed` and stored on the `EmbeddingArity` column here, so generation stays a Compute concern and this fold owns only retrieval-and-fusion; a profile without pg_search preloaded degrades the BM25 branch to the native FTS `ts_rank` baseline inside the same fusion CTE so the fused result stays correct at reduced lexical power; the `rrfConstant` defaults to 60 per the standard RRF damping and the fused top-k re-queries the row store by the surviving content keys so the fusion projects identities, never re-materializing both candidate payloads.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class EmbeddingArity {
    public static readonly EmbeddingArity Dense = new("dense", clrType: "Pgvector.Vector", storeType: "vector");
    public static readonly EmbeddingArity Half = new("half", clrType: "Pgvector.HalfVector", storeType: "halfvec");
    public static readonly EmbeddingArity Sparse = new("sparse", clrType: "Pgvector.SparseVector", storeType: "sparsevec");
    public static readonly EmbeddingArity Bit = new("bit", clrType: "System.Collections.BitArray", storeType: "bit");

    public string ClrType { get; }
    public string StoreType { get; }

    public string Column(int dimensions) => $"{StoreType}({dimensions})";
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class VectorMetric {
    public static readonly VectorMetric L2 = new("l2", op: "<->", bit: false, fn: nameof(VectorDbFunctionsExtensions.L2Distance));
    public static readonly VectorMetric Cosine = new("cosine", op: "<=>", bit: false, fn: nameof(VectorDbFunctionsExtensions.CosineDistance));
    public static readonly VectorMetric InnerProduct = new("inner-product", op: "<#>", bit: false, fn: nameof(VectorDbFunctionsExtensions.MaxInnerProduct));
    public static readonly VectorMetric L1 = new("l1", op: "<+>", bit: false, fn: nameof(VectorDbFunctionsExtensions.L1Distance));
    public static readonly VectorMetric Hamming = new("hamming", op: "<bit-op-hamming>", bit: true, fn: nameof(VectorDbFunctionsExtensions.HammingDistance));
    public static readonly VectorMetric Jaccard = new("jaccard", op: "<bit-op-jaccard>", bit: true, fn: nameof(VectorDbFunctionsExtensions.JaccardDistance));

    public string Op { get; }
    public bool Bit { get; }
    public string Fn { get; }

    public Expression<Func<TRow, double>> Order<TRow, TColumn>(Expression<Func<TRow, TColumn>> column, ReadOnlyMemory<float> probe) {
        var row = column.Parameters[0];
        var operand = Bit
            ? Expression.Constant(BitProbe(probe.Span), typeof(BitArray))
            : Expression.Constant(new Vector(probe), typeof(Vector));
        var call = Expression.Call(
            typeof(VectorDbFunctionsExtensions), Fn, Type.EmptyTypes,
            column.Body, operand);
        return Expression.Lambda<Func<TRow, double>>(call, row);
    }

    private static BitArray BitProbe(ReadOnlySpan<float> probe) {
        var bits = new BitArray(probe.Length);
        for (var i = 0; i < probe.Length; i++)
            bits[i] = probe[i] > 0f;
        return bits;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class FullTextMode {
    public static readonly FullTextMode Match = new("match");
    public static readonly FullTextMode Prefix = new("prefix");
    public static readonly FullTextMode Phrase = new("phrase");
    public static readonly FullTextMode Websearch = new("websearch");
}

public readonly record struct EmbeddingIdentity(UInt128 ContentHash, string ModelId, EmbeddingArity Arity) {
    public static EmbeddingIdentity Of(ReadOnlySpan<byte> content, string modelId, EmbeddingArity arity) =>
        new(XxHash128.HashToUInt128(content), modelId, arity);
}

public sealed record VectorQuery<TRow>(
    ReadOnlyMemory<float> Probe,
    int K,
    VectorMetric Metric,
    EmbeddingArity Arity,
    Option<Expression<Func<TRow, bool>>> Filter = default);

public sealed record FullTextQuery(
    string Terms,
    FullTextMode Mode,
    bool Rank,
    bool Highlight,
    bool Bm25);

public static class HybridRetrieve {
    public const int DefaultRrfConstant = 60;

    public static string Fuse<TRow>(VectorQuery<TRow> vector, FullTextQuery text, int k, int rrfConstant) =>
        $"""
        WITH vector_branch AS (
            SELECT id, ROW_NUMBER() OVER (ORDER BY embedding {vector.Metric.Op} $probe) AS rank
            FROM corpus ORDER BY embedding {vector.Metric.Op} $probe LIMIT {k}),
        bm25_branch AS (
            SELECT id, ROW_NUMBER() OVER (ORDER BY pdb.score(id) DESC) AS rank
            FROM corpus WHERE corpus @@@ pdb.parse($terms) ORDER BY pdb.score(id) DESC LIMIT {k})
        SELECT id, SUM(1.0 / ({rrfConstant} + rank)) AS fused
        FROM (SELECT id, rank FROM vector_branch UNION ALL SELECT id, rank FROM bm25_branch) ranked
        GROUP BY id ORDER BY fused DESC LIMIT {k}
        """;
}
```

| [INDEX] | [ROW]                        | [STORE]         | [SURFACE]                                                          | [LAW]                                                |
| :-----: | ---------------------------- | --------------- | ------------------------------------------------------------------ | ---------------------------------------------------- |
|   [1]   | dense / half / sparse / bit  | postgres-server | `UseVector` column mapping per `EmbeddingArity` row                | dimensionality fixed per embedding-family row; `EmbeddingArity` carries the CLR-to-store mapping and `bit` rides `binary_quantize(emb)::bit(N)` |
|   [2]   | HNSW + IVFFlat + diskann     | postgres-server | HNSW/IVFFlat metadata on schema rows; diskann DDL on `provisioning#SEARCH_PROVISIONING` | approximate routes; exact scan stays the baseline; diskann scales disk-backed ANN beyond RAM-resident HNSW and complements never replaces |
|   [3]   | distance ordering            | postgres-server | `VectorMetric.Order` projects through `L2Distance` / `CosineDistance` / `MaxInnerProduct` / `L1Distance` (dense rows) and `HammingDistance` / `JaccardDistance` (bit rows) | one `Switch` fold builds the `ORDER BY` distance `Expression` per metric row; planner routes a dense/half distance query through diskann transparently |
|   [4]   | gated embedded vector table  | sqlite rows     | loadable-extension gate                                            | brute-force fallback present on every profile        |
|   [5]   | FTS5 external-content tables | sqlite rows     | virtual table + rebuild triggers                                   | unicode61 and trigram tokenizer rows; bm25 rank      |
|   [6]   | tsvector generated column    | postgres-server | generated column + GIN                                             | websearch_to_tsquery admission; ts_rank; ts_headline |
|   [7]   | fuzzy rows                   | postgres-server | pg_trgm similarity, unaccent, fuzzystrmatch                        | candidate prefilter and normalization before rank    |
|   [8]   | binary-vector metrics        | postgres-server | `VectorMetric.Order` projects through the catalogued `HammingDistance` / `JaccardDistance` `DbFunctions` over a `bit` column | `VectorMetric.Hamming` / `.Jaccard` bit rows project bit distance via the EF translator, exact scan baseline; the raw server operator literals (`<bit-op-hamming>` / `<bit-op-jaccard>`) stay unresolved placeholders under SPIKE — `.api/api-pgvector-ef.md` rows [5]/[6] catalogue the `DbFunctions` member but not the operator string, tier-2 live-PG18 closure under `[BIT_VECTOR_COLUMNS]` |
|   [9]   | BM25 overlay                 | postgres-server | `@@@ pdb.parse(...)` query-builder match, `pdb.score(<key_col>)` / `pdb.snippet` via `FromSql` / `SqlQuery`; BM25 index DDL on `provisioning#SEARCH_PROVISIONING` | high-power FTS overlay beside the always-present native baseline; `@@@` RHS is always a `pdb.*` builder, score/snippet anchor on the `key_field`; `FullTextQuery.Bm25` selects it; native FTS answers when pg_search is not preloaded |
|  [10]   | hybrid fusion                | postgres-server | `HybridRetrieve.Fuse` reciprocal-rank-fusion CTE over the vector and BM25 branches via `FromSql` | dense embedding from `Compute/models#INFERENCE_MODES` `Embed`; `1.0 / (rrfConstant + rank)` summed across branches, `rrfConstant` 60; native FTS branch when pg_search absent; projects identities then re-queries the store |

## [5]-[GEO_LANES]

- Owner: `GeoLayer` container row over the NetTopologySuite value chain; `SpatialOp` is the `[SmartEnum<string>]` DE-9IM predicate row family — `ST_Intersects`/`ST_Contains`/`ST_Covers`/`ST_Within`/`ST_Touches`/`ST_Crosses`/`ST_Overlaps`/`ST_Distance` — each row carrying its `Geometry` instance-method spelling, its `ST_*` server function, and its `SpatialConcern` (proximity/coverage/spatial-join) arm, and projecting one translated `Expression` predicate per row through `Predicate`/`Within`/`Knn`; `SpatialDiff` is the change-detection fold reading the op-log OLD/NEW geometry pair into a typed spatial-delta the AppUi live-geo overlay consumes; `CrsReconcile` is the georeferencing/CRS reconciliation kernel projecting a per-model transform-to-world from an EPSG/proj CRS pair plus the IFC map-conversion survey alignment.
- Cases: `SpatialOp` Intersects | Contains | Covers | Within | Touches | Crosses | Overlaps | Distance over the `SpatialConcern` proximity | coverage | spatial-join axis — proximity is the `DistanceKnn`/`IsWithinDistance` ordering carried by `Knn`/`Within`, coverage is the `Contains`/`Covers`/`Within` window, spatial-join is the `Intersects`/`Touches`/`Crosses`/`Overlaps` translated join, and `Distance` projects the `ST_Distance` measure-and-threshold predicate; `SpatialDiff` enter | exit | move | reshape over the OLD/NEW geometry pair.
- Entry: `public static SpatialDelta Diff(string entityKey, Option<Geometry> old, Option<Geometry> next)` discriminates enter (None old), exit (None next), move (centroid shift over `MoveThreshold`), and reshape (boundary change at equal centroid); `public static SpatialDelta Of(OpLogEntry old, OpLogEntry next)` decodes the WKB `Payload` of each op-log entry (Delete → `None`) into the operands, and `public static Seq<SpatialDelta> Fold(Seq<OpLogEntry> changefeed)` groups the `geometry` column family by `EntityKey`, HLC-orders each group, and derives the OLD/NEW pair so the geo-diff is one pure fold over the changefeed; `public Expression<Func<TRow, bool>> Predicate<TRow>(Expression<Func<TRow, Geometry>> column, Geometry probe)` projects the row's translated DE-9IM predicate (or the `ST_Distance <= 0` form for the `Distance` row) and the `Within`/`Knn` projections cover the proximity `DbFunctions`.
- Receipt: a `search.spatial.route` fact discriminating the GiST index route vs sequential scan taken per `SpatialOp` row, and a `geo.diff.kind` fact carrying the `SpatialDiff` delta-kind count per changefeed batch, both riding the interceptor fact stream under the `search.*` and `geo.*` kind families.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, NetTopologySuite.IO.GeoPackage, NetTopologySuite.IO.GeoJSON4STJ, Microsoft.Data.Sqlite, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new geometry concern is one `GeoLayer` row or one driver-native type row; a new container-write policy is one column on `GeoLayer`; a new spatial predicate is one `SpatialOp` row carrying its `Geometry` method, `ST_*` server function, and `SpatialConcern` arm plus one `Predicate` projection; a new change-detection kind is one `SpatialDeltaKind` case; a new CRS-reconciliation source is one `CrsSource` row and a new datum-shift is one `CrsReconcile` policy value; zero new surface.
- Boundary: NetTopologySuite is the store boundary projection of the one canonical wire geometry — a third managed geometry representation is the named defect; server geometry and geography columns enter through the single `UseNetTopologySuite` admission on provider options and data source; container window reads prefilter by `Envelope` against the R*Tree index and refine with managed `Geometry` predicates after blob decode — `Geometry.Intersects`, `Geometry.Contains`, and `Geometry.Buffer` over the decoded candidate set are the managed-refine vocabulary and `Geometry.AsBinary`/`Geometry.AsText` are the only egress projections, while server-side refine and a second managed geometry chain stay rejected; the `Ordinates`, `RepairRings`, and `RingOrientation` columns parameterize the GPB codec — `GeoPackageGeoWriter.HandleOrdinates` caps written ordinates and selects the header envelope kind, `HandleSRID` stamps the row's `Srid` on decode, ring repair runs only where the row arms it, and `RingOrientationOption` selects the polygon-ring orientation on write so a sign-deriving kernel reads a normalized winding; feature attributes stay element-backed read-only unless `WriteAttributes` admits the mutable `JsonObjectAttributesTable` adapter, and typed attribute projection rides `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>` and `TryGetJsonObjectPropertyValue<T>` so loose-table DOM walking is the deleted form; geodesy is owned server-side and the earthdistance route stays rejected; a grouped-geometry rollup rides the `NpgsqlNetTopologySuiteAggregateMethodCallTranslatorPlugin` spatial-aggregate translation as one row paired with the `NpgsqlRange<T>` `RangeAgg` aggregate, its concrete collect/union/extent method spelling research-held on `[MANAGED_REFINE_OPS]`, so a client-side geometry reduction over a result set is the deleted form; `SpatialOp` projects one translated DE-9IM predicate per row over the NetTopologySuite `Geometry` instance method the Npgsql member-call translator maps to the `ST_*` server function — `Intersects`/`Touches`/`Crosses`/`Overlaps` (spatial-join), `Contains`/`Covers`/`Within` (coverage), and `Distance` (proximity, projected as the `ST_Distance <= 0` containment form) — with the proximity `IsWithinDistance` window and `DistanceKnn` ordering carried by the `Within`/`Knn` `DbFunctions` projections, so the eight predicates fold under the three `SpatialConcern` arms and a parallel hand-built spatial query shape per concern is the deleted form, and the GiST index DDL each row needs is owned at `provisioning#SEARCH_PROVISIONING` and consumed here as settled, never executed by this fold; `SpatialDiff.Of` decodes the OLD/NEW `OpLogEntry` pair's WKB `Payload` through one `WKBReader` (a `SyncOpKind.Delete` or empty payload projecting `None`) into the `Option<Geometry>` operands, `SpatialDiff.Fold` groups the changefeed's `geometry` column family by `EntityKey` and HLC-orders each group to derive the pair — the same `OpLogEntry` `Payload` the changefeed carries with PG18 `OLD`/`NEW` RETURNING semantics on the write path — and `Diff` folds them into a typed `SpatialDelta` discriminating enter, exit, move, and reshape with `Histogram` projecting the per-kind `geo.diff.kind` count, so the geo-change detection is a pure projection over the existing changefeed, never a second trigger or a polling scan; the live-geometry overlay consequence — the sync-fed vector features painted in place — is owned at `AppUi/charts-dashboards#SERIES_TABLE`, which consumes the `SpatialDelta` stream as the overlay binding, so the diff mechanics live here and the rendering binding lives there; the centroid-shift `move` threshold rides a policy value rather than a literal so a coordinate-system change reparameterizes one column; `CrsReconcile` is the first-class georeferencing kernel projecting a per-model transform-to-world — `FromMapConversion` reads an IFC `IfcMapConversion`/`IfcProjectedCRS` (eastings/northings/orthogonal-height plus the X-axis abscissa/ordinate rotation and scale) into a 2D affine, `FromSurveyPoints` least-squares-fits a similarity transform from control-point pairs with a residual RMS so a survey-point alignment is a fitted transform rather than a hand-entered offset, and the EPSG/proj datum reprojection rides the PostGIS `ST_Transform` (`TransformSql`) so the heavy CRS math executes server-side over the proj library the postgis extension links, never a managed proj reimplementation; the `CrsSource` carries the EPSG code and the proj string so a model in a local engineering CRS reconciles to a geographic world CRS through one fitted transform, and a per-model transform-to-world is the column the federated entity's geometry reads so two models in different CRSs federate into one world frame — a hardcoded model offset or a managed coordinate-transform library is the deleted form.

```csharp signature
public sealed record GeoLayer(
    string Table,
    string GeometryColumn,
    int Srid,
    Envelope Extent,
    Ordinates Ordinates,
    bool RepairRings,
    RingOrientationOption RingOrientation,
    bool WriteAttributes);

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class SpatialConcern {
    public static readonly SpatialConcern Proximity = new("proximity");
    public static readonly SpatialConcern Coverage = new("coverage");
    public static readonly SpatialConcern SpatialJoin = new("spatial-join");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class SpatialOp {
    public static readonly SpatialOp Intersects = new("st-intersects", SpatialConcern.SpatialJoin, nameof(Geometry.Intersects), "ST_Intersects");
    public static readonly SpatialOp Contains = new("st-contains", SpatialConcern.Coverage, nameof(Geometry.Contains), "ST_Contains");
    public static readonly SpatialOp Covers = new("st-covers", SpatialConcern.Coverage, nameof(Geometry.Covers), "ST_Covers");
    public static readonly SpatialOp Within = new("st-within", SpatialConcern.Coverage, nameof(Geometry.Within), "ST_Within");
    public static readonly SpatialOp Touches = new("st-touches", SpatialConcern.SpatialJoin, nameof(Geometry.Touches), "ST_Touches");
    public static readonly SpatialOp Crosses = new("st-crosses", SpatialConcern.SpatialJoin, nameof(Geometry.Crosses), "ST_Crosses");
    public static readonly SpatialOp Overlaps = new("st-overlaps", SpatialConcern.SpatialJoin, nameof(Geometry.Overlaps), "ST_Overlaps");
    public static readonly SpatialOp Distance = new("st-distance", SpatialConcern.Proximity, nameof(Geometry.Distance), "ST_Distance");

    public SpatialConcern Concern { get; }
    public string Method { get; }
    public string ServerFn { get; }

    public Expression<Func<TRow, bool>> Predicate<TRow>(Expression<Func<TRow, Geometry>> column, Geometry probe) {
        var row = column.Parameters[0];
        var call = Equals(Distance)
            ? (Expression)Expression.LessThanOrEqual(
                Expression.Call(column.Body, nameof(Geometry.Distance), Type.EmptyTypes, Expression.Constant(probe, typeof(Geometry))),
                Expression.Constant(0.0))
            : Expression.Call(column.Body, Method, Type.EmptyTypes, Expression.Constant(probe, typeof(Geometry)));
        return Expression.Lambda<Func<TRow, bool>>(call, row);
    }

    public Expression<Func<TRow, bool>> Within<TRow>(Expression<Func<TRow, Geometry>> column, Geometry probe, double meters) {
        var row = column.Parameters[0];
        var call = Expression.Call(
            typeof(NpgsqlNetTopologySuiteDbFunctionsExtensions), nameof(NpgsqlNetTopologySuiteDbFunctionsExtensions.IsWithinDistance), Type.EmptyTypes,
            Expression.Constant(EF.Functions), column.Body, Expression.Constant(probe, typeof(Geometry)), Expression.Constant(meters));
        return Expression.Lambda<Func<TRow, bool>>(call, row);
    }

    public Expression<Func<TRow, double>> Knn<TRow>(Expression<Func<TRow, Geometry>> column, Geometry probe) {
        var row = column.Parameters[0];
        var call = Expression.Call(
            typeof(NpgsqlNetTopologySuiteDbFunctionsExtensions), nameof(NpgsqlNetTopologySuiteDbFunctionsExtensions.DistanceKnn), Type.EmptyTypes,
            Expression.Constant(EF.Functions), column.Body, Expression.Constant(probe, typeof(Geometry)));
        return Expression.Lambda<Func<TRow, double>>(call, row);
    }
}

[SmartEnum]
public sealed partial class SpatialDeltaKind {
    public static readonly SpatialDeltaKind Enter = new();
    public static readonly SpatialDeltaKind Exit = new();
    public static readonly SpatialDeltaKind Move = new();
    public static readonly SpatialDeltaKind Reshape = new();
}

public sealed record SpatialDelta(string EntityKey, SpatialDeltaKind Kind, Option<Geometry> Old, Option<Geometry> Next);

public static class SpatialDiff {
    public const double MoveThreshold = 1e-6;

    private static readonly WKBReader Wkb = new();

    public static SpatialDelta Diff(string entityKey, Option<Geometry> old, Option<Geometry> next) =>
        (old, next) switch {
            ({ IsSome: true, Case: Geometry o }, { IsSome: true, Case: Geometry n }) =>
                o.Centroid.Distance(n.Centroid) > MoveThreshold
                    ? new SpatialDelta(entityKey, SpatialDeltaKind.Move, old, next)
                    : new SpatialDelta(entityKey, SpatialDeltaKind.Reshape, old, next),
            ({ IsNone: true }, _) => new SpatialDelta(entityKey, SpatialDeltaKind.Enter, old, next),
            _ => new SpatialDelta(entityKey, SpatialDeltaKind.Exit, old, next),
        };

    public static SpatialDelta Of(OpLogEntry old, OpLogEntry next) =>
        Diff(next.EntityKey, Decode(old.Kind, old.Payload), Decode(next.Kind, next.Payload));

    public static Seq<SpatialDelta> Fold(Seq<OpLogEntry> changefeed) =>
        changefeed
            .Filter(static entry => entry.ColumnFamily == "geometry")
            .GroupBy(static entry => entry.EntityKey)
            .Map(static group => {
                var ordered = group.OrderBy(static e => e.Physical).ThenBy(static e => e.Logical).ToSeq();
                return Diff(
                    group.Key,
                    ordered.HeadOrNone().Bind(static e => Decode(e.Kind, e.Payload)),
                    ordered.LastOrNone().Bind(static e => Decode(e.Kind, e.Payload)));
            })
            .ToSeq();

    public static HashMap<SpatialDeltaKind, int> Histogram(Seq<SpatialDelta> deltas) =>
        deltas.Fold(
            HashMap<SpatialDeltaKind, int>(),
            static (acc, delta) => acc.AddOrUpdate(delta.Kind, static n => n + 1, 1));

    private static Option<Geometry> Decode(SyncOpKind kind, ReadOnlyMemory<byte> payload) =>
        kind == SyncOpKind.Delete || payload.IsEmpty
            ? None
            : Some(Wkb.Read(payload.Span));
}

public sealed record CrsSource(int Epsg, string ProjString, bool Geographic);

public sealed record MapConversion(
    double Eastings,
    double Northings,
    double OrthogonalHeight,
    double XAxisAbscissa,
    double XAxisOrdinate,
    double Scale);

public sealed record CrsTransform(
    CrsSource Source,
    CrsSource Target,
    double[] Matrix,
    Option<MapConversion> MapConversion,
    int SurveyPointCount,
    double ResidualRms);

public static class CrsReconcile {
    public const string TransformSql = "SELECT ST_AsBinary(ST_Transform(ST_GeomFromWKB($geom, $source), $target))";

    public static CrsTransform FromMapConversion(CrsSource source, CrsSource target, MapConversion conversion) {
        var rotation = Math.Atan2(conversion.XAxisOrdinate, conversion.XAxisAbscissa);
        var cos = Math.Cos(rotation) * conversion.Scale;
        var sin = Math.Sin(rotation) * conversion.Scale;
        return new CrsTransform(
            source, target,
            [cos, -sin, conversion.Eastings, sin, cos, conversion.Northings],
            Some(conversion), 0, 0.0);
    }

    public static CrsTransform FromSurveyPoints(CrsSource source, CrsSource target, Seq<((double X, double Y) Local, (double X, double Y) World)> pairs) {
        var n = pairs.Count;
        var meanLocal = (X: pairs.Sum(static p => p.Local.X) / n, Y: pairs.Sum(static p => p.Local.Y) / n);
        var meanWorld = (X: pairs.Sum(static p => p.World.X) / n, Y: pairs.Sum(static p => p.World.Y) / n);
        var sxx = pairs.Sum(p => (p.Local.X - meanLocal.X) * (p.World.X - meanWorld.X) + (p.Local.Y - meanLocal.Y) * (p.World.Y - meanWorld.Y));
        var sxy = pairs.Sum(p => (p.Local.X - meanLocal.X) * (p.World.Y - meanWorld.Y) - (p.Local.Y - meanLocal.Y) * (p.World.X - meanWorld.X));
        var theta = Math.Atan2(sxy, sxx);
        var cos = Math.Cos(theta);
        var sin = Math.Sin(theta);
        var tx = meanWorld.X - (cos * meanLocal.X - sin * meanLocal.Y);
        var ty = meanWorld.Y - (sin * meanLocal.X + cos * meanLocal.Y);
        var residual = Math.Sqrt(pairs.Sum(p => {
            var wx = cos * p.Local.X - sin * p.Local.Y + tx;
            var wy = sin * p.Local.X + cos * p.Local.Y + ty;
            return (wx - p.World.X) * (wx - p.World.X) + (wy - p.World.Y) * (wy - p.World.Y);
        }) / n);
        return new CrsTransform(source, target, [cos, -sin, tx, sin, cos, ty], None, n, residual);
    }

    public static (double X, double Y) ToWorld(CrsTransform transform, double x, double y) =>
        (transform.Matrix[0] * x + transform.Matrix[1] * y + transform.Matrix[2],
         transform.Matrix[3] * x + transform.Matrix[4] * y + transform.Matrix[5]);
}
```

| [INDEX] | [CONCERN]               | [RESIDENCE]           | [SURFACE]                                                                             | [LAW]                                                                                                                                                                                                                                                                                          |
| :-----: | ----------------------- | --------------------- | ------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | geometry / geography    | postgres-server       | NetTopologySuite column mapping                                                       | spatial predicates translate; indexes ride schema rows                                                                                                                                                                                                                                         |
|   [2]   | embedded geo container  | sqlite container file | `GeoPackageGeoReader` / `GeoPackageGeoWriter`                                         | GPB blob codec; R*Tree window then managed `Geometry.Intersects` / `Geometry.Contains` / `Geometry.Buffer` refine; `Geometry.AsBinary` / `Geometry.AsText` egress only                                                                                                                         |
|   [3]   | hierarchy paths         | postgres-server       | ltree driver-native rows                                                              | assembly and part trees; lquery operators raw SQL                                                                                                                                                                                                                                              |
|   [4]   | N-dimensional intervals | postgres-server       | cube driver-native rows                                                               | GiST nearest-neighbor concerns                                                                                                                                                                                                                                                                 |
|   [5]   | scalar spans            | postgres-server       | `NpgsqlRange<T>` ranges, multiranges, `NpgsqlInterval`                                | overlap predicates and exclusion constraints; `Period` / `YearMonth` / `DateInterval` columns and `Interval` / `DateInterval` spans ride NodaTime range and `IntervalMultirangeMapping` / `DateIntervalMultirangeMapping` mappings with `RangeAgg` / `RangeIntersectAgg` aggregate projections |
|   [6]   | geodesic query forms    | postgres-server       | `Distance` / `IsWithinDistance` / `DistanceKnn` / `Transform` / `Force2D` DbFunctions | spheroid math, KNN ordering, and SRID reprojection ride translated SQL, never client refine                                                                                                                                                                                                    |
|   [7]   | feature attribute write | both projections      | `JsonObjectAttributesTable` / `IPartiallyDeserializedAttributesTable`                 | mutable feature attributes only under `WriteAttributes`; typed projection via `TryDeserializeJsonObject<T>` / `TryGetJsonObjectPropertyValue<T>`; loose-table walking rejected                                                                                                                 |
|   [8]   | container window clip   | sqlite container file | GeoPackage R*Tree window predicate                                                    | envelope-overlap candidate set keyed on the integer primary key before managed-geometry refine                                                                                                                                                                                                 |
|   [9]   | spatial query shapes    | postgres-server       | `SpatialOp` rows — `ST_Intersects` / `ST_Contains` / `ST_Covers` / `ST_Within` / `ST_Touches` / `ST_Crosses` / `ST_Overlaps` / `ST_Distance` over `Geometry` DE-9IM methods, plus `IsWithinDistance` proximity and `DistanceKnn` ordering | eight DE-9IM predicate rows under three `SpatialConcern` arms (proximity / coverage / spatial-join); `Predicate` projects the translated `Expression` per row, `Within`/`Knn` project the `DbFunctions` proximity forms; GiST index DDL owned at `provisioning#SEARCH_PROVISIONING`; `search.spatial.route` fact discriminates GiST vs sequential scan |
|  [10]   | spatial change detection | postgres-server       | `SpatialDiff.Of` / `SpatialDiff.Fold` over the op-log OLD/NEW geometry pair          | pure projection over the existing changefeed; WKB `Payload` decode keyed on `SyncOpKind` (Delete → `None`); `Fold` groups the changefeed by `EntityKey` on the `geometry` column family and HLC-orders to derive the OLD/NEW operands; enter / exit / move / reshape `SpatialDeltaKind`; `Histogram` projects the `geo.diff.kind` fact per batch; the live overlay binding consumes the `SpatialDelta` stream at `AppUi/charts-dashboards#SERIES_TABLE` |

## [6]-[ANALYTICAL_LANE]

- Owner: `TabularExportSpec` and `ParquetSchemaStamp` export policy records, the `AnalyticalTraversal` changefeed-to-parquet extension block, and the table-function registration owner block — `RelationSource<TData,TProjection>` and `RelationSchema` policy records consumed by the `RelationLane` extension that registers an in-process managed sequence as a queryable relation, all folding under the one analytical owner.
- Packages: DuckDB.NET.Data.Full, Sep, MessagePack, LanguageExt.Core, BCL inbox
- Growth: a new export shape is one `TabularExportSpec` row; a new exchange direction is one `TabularDirection` policy value on the same record; a new parse policy is one column on `TabularExportSpec` — `ColNotSet` carries the unset-column policy and `Spec` round-trips the separator-culture value; a new parquet generation is one `ParquetSchemaStamp` row; a new shred source is one read_json row; a new in-process relation is one `RegisterTableFunction` row carrying its `ColumnInfo` schema and `CardinalityHint`, a scalar one its `ScalarFunctionOptions`; a new engine-fault projection is one `Fault` arm; the changefeed materialization is the `AnalyticalTraversal` extension block on `TabularExportSpec` with one `DuckDBOpLogMap` mapping descriptor, not a new owner; zero new surface.
- Boundary: the `DuckDBOpLogMap` base `DuckDBAppenderMap<T>` resolves from `DuckDB.NET.Data.Mapping` and the `CreateAppender<T, TMap>` return `DuckDBMappedAppender<T, TMap>` from `DuckDB.NET.Data`, so the mapped-appender import surface is the two namespaces; the analytical lane reads and projects and never owns source-of-truth writes; the standalone in-process DuckDB lane is the embedded and local analytical-read owner while server analytical rollups ride TimescaleDB continuous aggregates on `provisioning#TIMESCALE_PROVISIONING`, so there is no cross-engine server seam and no pg_duckdb row — pg_duckdb is rejected as redundant with this lane plus the continuous-aggregate rollup; the server boundary is parquet-export-only — postgres_scanner stays rejected; live embedded reads ride one `ATTACH (TYPE sqlite, READ_ONLY)` row gated by the live-attach research gate, and in-process concurrency rides `Duplicate` lanes over the one anchor handle so a streaming drain occupies its own connection while a write lane serializes at `BeginTransaction` under one `DuckDBTransaction` per database; foreign-store pre-flight is a `GetSchema` metadata read against the alias before any data moves, and `Prepare` amortizes repeated parameterized reads; parquet, json, and icu ride statically inside the bundled engine while the sqlite scanner autoloads, so offline posture vendors the platform-matched extension binary beside the engine; xlsx and a second tabular engine stay rejected with Sep and parquet as the owners; the `TabularDirection` policy value selects read versus write through `Direction.Switch` so one spec record derives both `SepReader` and `SepWriter` — `OpenReader` admits only under `Import` and `OpenWriter` only under `Export`, the opposite direction projecting to a typed `Fin` rejection so a misdirected open fails at composition rather than at first parse, `Fault` folds a `DuckDBException` engine fault into the same typed `Fin` rail so a native COPY or ATTACH failure never escapes as an unstructured throw, discriminating on the `DuckDBException.ErrorType` `DuckDBErrorType` member beside the catalogued `IsTransient`/`SqlState` projection; `Sep.Default.Separator` is `;` (char 59), never `,`, so the `Separator` column is always explicit and `Spec => Sep.New(Separator)` projects one declared value into both legs — a comma corpus declares `,` and never leans on `Sep.Default`; `ReadHeader` carries the header-presence axis for import, `Strict` and `Trim` fold onto the reader options value, `ColNotSet` carries the `SepColNotSetOption` (values `Throw`/`Empty`/`Skip`) unset-column policy as a spec column that binds the writer leg through `SepWriterOptions.ColNotSetOption` (the reader leg carries no unset-column knob), the header-named column window resolves through `Header.NamesStartingWith(prefix)` then `Header.IndicesOf(names.ToArray())` and `row[colIndices]` projects a `SepReader.Cols` range materialized only in-scope through `Cols.ToStringsArray()`/`Cols.ToStrings()` and parsed per-cell through `Col.TryParse<T>` (reserving `Col.Parse<T>` for validated columns) so a positional column slice never hardcodes ordinals, a zero-row export still writes the header row through `WriteHeader` so a header-only artifact stays a valid contract, `Sep.Auto` is the exploratory separator-sniff value never a contract profile, and the reader-to-writer fusion rides `SepReaderWriterExtensions.CopyTo` and `SepWriter.NewRow(readerRow)` so parse-materialize-rebuild is the rejected spelling; `SepReader.Row` and `SepReader.Cols` are non-boxable ref-struct projections that never escape the read scope, enter a closure, or reach a string-interpolation handler, strings route once through `SepToString` pooling, and typed columns parse through `ISpanParsable` values, never `ToString()` (which resolves to `ValueType.ToString`); the Sep fault taxonomy carries three types where only `InvalidDataException` (a ragged row) carries structured row/line context while `KeyNotFoundException` (an unknown column) and `ArgumentException` (a `Parse<T>` on a non-numeric cell) carry no cell coordinates, so the `Fault` fold injects column and row context when lifting either into the typed rail and the parse loop prefers `Col.TryParse<T>` to keep the rail non-exceptional; bulk receipt staging validates at construction through the `DuckDBAppenderMap<T>` `Map` rows so record-table drift is a construction failure caught on the rail, an absent staging value writes the column's engine default through the appender-map `DefaultValue()` mapping and the per-row `DuckDBAppenderRow.AppendDefault()` cell rather than forcing a null sentinel while `NullValue()` and `AppendNullValue()` carry the explicit null, and a `Clear` on the open `DuckDBAppender` discards the pending row batch so a mid-batch fault aborts cleanly before `Close` seals a partial run; a registered table function streams an in-process managed `IEnumerable<TDomain>` into the engine as a queryable relation through the high-level `RegisterTableFunction<TData,TProjection>(name, Func<IEnumerable<TData>>, Expression<Func<TData,TProjection>>)` extension (positional-SQL-parameter arities `RegisterTableFunction<T1,TData,TProjection>`… carry leading `Func<T1,…,IEnumerable<TData>>` argument types) or the low-level `RegisterTableFunction(name, Func<TableFunction>, Action<object?, IDuckDBDataWriter[], ulong>)` instance method, the projection inferring column names and CLR types from `NewExpression.Members` so it MUST be an anonymous type or member-init — a bare `ValueTuple` literal yields a registration-time `NullReferenceException` — and `WHERE`/`LIMIT` push down into the lazily-pulled enumerator so a filtered scan over a large sequence returns without materializing the whole relation, while a registered scalar declares purity and null-handling through `ScalarFunctionOptions` so a deterministic scalar participates in constant-folding and a volatile one stays per-row; `CardinalityHint(ulong Value, bool IsExact)` feeds the join planner with `IsExact` set when the count is known so a join over the managed relation plans correctly instead of against a staging table, and a bound `DuckDBParameter` pins its `DbType` so a parameterized read binds the engine type explicitly rather than inferring from the boxed value; vector chunk readers and writers move data at the engine's vector quantum so peak managed memory is one chunk wide and `DuckDBCommand.UseStreamingMode` with `GetQueryProgress()` bounds a long rollup to one chunk of result memory; DuckDB fts, spatial, remote-filesystem, and vss rows stay out — full-text, geometry, remote-blob, and vector concerns have named lane owners and vss persistence is experimental with documented loss on unclean shutdown.

```csharp signature
[SmartEnum]
public sealed partial class TabularDirection {
    public static readonly TabularDirection Import = new();
    public static readonly TabularDirection Export = new();
}

public sealed record TabularExportSpec(
    Seq<string> Columns,
    TabularDirection Direction,
    bool WriteHeader,
    bool ReadHeader,
    CultureInfo Culture,
    char Separator,
    bool Strict,
    SepTrim Trim,
    SepColNotSetOption ColNotSet,
    bool ParallelRead) {
    public Sep Spec => Sep.New(Separator);
}

public sealed record ParquetSchemaStamp(string Stamp, FrozenDictionary<string, string> Fields);

public static class TabularSpec {
    extension(TabularExportSpec spec) {
        public Fin<SepReader> OpenReader(Stream source) =>
            spec.Direction.Switch(
                state: (Spec: spec, Source: source),
                import: static s => Fin.Succ(ReaderOptions(s.Spec).From(s.Source)),
                export: static _ => Fin.Fail<SepReader>(Error.New("<tabular-direction:export-has-no-reader>")));

        public Fin<SepWriter> OpenWriter(Stream sink) =>
            spec.Direction.Switch(
                state: (Spec: spec, Sink: sink),
                import: static _ => Fin.Fail<SepWriter>(Error.New("<tabular-direction:import-has-no-writer>")),
                export: static s => Fin.Succ(
                    s.Spec.Spec
                        .Writer(o => o with { CultureInfo = s.Spec.Culture, WriteHeader = s.Spec.WriteHeader })
                        .To(s.Sink)));
    }

    public static Fin<T> Fault<T>(Func<T> run) =>
        Prelude.Try(run).Match(
            Succ: Fin.Succ,
            Fail: ex => ex is DuckDBException duck
                ? Fin.Fail<T>(Error.New("<duckdb-fault>", duck))
                : Fin.Fail<T>(Error.New(ex)));

    private static SepReaderOptions ReaderOptions(TabularExportSpec spec) =>
        spec.Spec.Reader(o => (spec.Strict ? o.Strict() : o) with {
            CultureInfo = spec.Culture,
            HasHeader = spec.ReadHeader,
            Trim = spec.Trim,
            Unescape = true,
        });
}
```

The analytical lane registers an in-process managed sequence as a queryable relation through the DuckDB.NET table-function surface — `RelationSource<TData,TProjection>` carries the registration name, the `IEnumerable<TData>` factory, and the anonymous-type-or-member-init projection the high-level extension reads through `NewExpression.Members`, and `RelationSchema` carries the low-level `TableFunction` schema with its `ColumnInfo` columns and `CardinalityHint`. The `Register` fold binds the high-level extension and the `Schema` fold the low-level `Func<TableFunction>`+writer overload, both on the one anchor `DuckDBConnection`; the projection is never a bare `ValueTuple` literal because the parser reads `NewExpression.Members`. `WriteRow` is the platform-forced statement seam — the per-column `IDuckDBDataWriter.WriteValue<T>` calls are the low-level writer's language-owned form — while the registration surface stays expression-shaped.

```csharp signature
public sealed record RelationSource<TData, TProjection>(
    string Name,
    Func<IEnumerable<TData>> Data,
    Expression<Func<TData, TProjection>> Projection);

public sealed record RelationSchema(
    string Name,
    Seq<ColumnInfo> Columns,
    IEnumerable Data,
    CardinalityHint Cardinality,
    Action<object?, IDuckDBDataWriter[], ulong> WriteRow);

public static class RelationLane {
    extension(DuckDBConnection lane) {
        public IO<Unit> Register<TData, TProjection>(RelationSource<TData, TProjection> source) =>
            IO.lift(() => { lane.RegisterTableFunction(source.Name, source.Data, source.Projection); return unit; });

        public IO<Unit> Schema(RelationSchema source) =>
            IO.lift(() => {
                lane.RegisterTableFunction(
                    source.Name,
                    () => new TableFunction([.. source.Columns], source.Data, source.Cardinality),
                    source.WriteRow);
                return unit;
            });
    }
}
```

| [INDEX] | [ROW]                     | [SURFACE]                                                                                                | [LAW]                                                                                                                                                                                                                                                                                                                                                                       |
| :-----: | ------------------------- | -------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | live store attach         | `ATTACH 'store.db' AS live (TYPE sqlite, READ_ONLY)` raw SQL on `DuckDBCommand`                          | zero-export reads of the embedded store; `Duplicate` lanes carry concurrent drains                                                                                                                                                                                                                                                                                          |
|   [2]   | parquet export            | `COPY` rollup `TO` parquet via `DuckDBCommand`                                                           | the only dashboard export; rows carry the stamp column                                                                                                                                                                                                                                                                                                                      |
|   [3]   | json shred                | `read_json` / `json_transform`                                                                           | typed STRUCT and LIST coercion of receipt streams                                                                                                                                                                                                                                                                                                                           |
|   [4]   | rollup pushdown           | group, window, percentile raw SQL                                                                        | aggregate shapes stay off the EF rails                                                                                                                                                                                                                                                                                                                                      |
|   [5]   | collation                 | icu rows inside the engine                                                                               | locale-aware ordering inside exports                                                                                                                                                                                                                                                                                                                                        |
|   [6]   | tabular import/export     | `SepReader` / `SepWriter` under one spec record                                                          | `TabularDirection` derives the reader/writer pair; `Spec` round-trips one `Sep.New` value into both legs; culture-aware parse; `Cols.Select` range-parse column window; `ReadHeader` and `WriteHeader` header policy with zero-row header still written; `SepColNotSetOption` unset policy; `Sep.Auto` exploratory sniff; `Strict` hardening, `SepTrim` trim, `SepToString` pooling, and `ParallelEnumerate` projection ride the spec columns; `SepReaderWriterExtensions.CopyTo` and `NewRow(readerRow)` fuse reader-to-writer; `Fault` folds `DuckDBException` into the typed rail |
|   [7]   | mapped receipt staging    | `CreateAppender<T,TMap>` over `DuckDBMappedAppender<T,TMap>` / `DuckDBAppenderMap<T>`; `AppendRecords` then `Close` | derived rollup staging only; record-table drift fails at `DuckDBAppenderMap<T>` `Map` construction; `AppendValue`/`AppendNullValue` write a typed cell or a null, the map `DefaultValue()`/`NullValue()` and row `AppendDefault()` write an engine-default cell; `DuckDBAppender.Clear` discards a pending batch before a mid-run abort; the batch flushes at the vector quantum and `Close` seals the run; a row-at-a-time appender open is rejected |
|   [8]   | in-process relations      | high-level `RegisterTableFunction<TData,TProjection>` + low-level `RegisterTableFunction(name, Func<TableFunction>, Action<object?, IDuckDBDataWriter[], ulong>)`; `RegisterScalarFunction` with `ScalarFunctionOptions` | a managed `IEnumerable<TDomain>` streams in as a relation with `CardinalityHint(Value, IsExact)` feeding the join planner and `WHERE`/`LIMIT` pushdown into the lazy enumerator, never a staging table; the projection is an anonymous type or member-init (a `ValueTuple` literal NREs at registration); `IDuckDBDataWriter.WriteValue<T>` writes the low-level cell; `ScalarFunctionOptions` declares purity and null-handling; a bound `DuckDBParameter` pins its `DbType` |
|   [9]   | vector chunk transfer     | `VectorDataReaderFactory` / `VectorDataWriterFactory` / `IDuckDBDataWriter` (`WriteValue` / `IsValid`)   | one-chunk-wide ingress and egress at the vector quantum; `IsValid` is the validity-mask read before every nullable column value                                                                                                                                                                                                                                             |
|  [10]   | long-rollup observability | `DuckDBCommand.UseStreamingMode` property + `DuckDBConnection.GetQueryProgress()`                        | bounded-memory result streams; progress projects as maintain-kind facts; a negative percentage projects to absence                                                                                                                                                                                                                                                          |

The `TabularExportSpec` owner carries one HLC-ordered changefeed-to-parquet traversal — `Materialize` drains the sync changefeed's length-delimited segments through `MessagePackStreamReader` into one materialized `Seq` (the changefeed entries consumed as settled vocabulary), orders that carrier by the HLC `(Physical, Logical, OriginStoreId)` key, and flushes the ordered run through the `DuckDBOpLogMap` mapped appender's `AppendRecords` in one batch sealed by `Close`, so the staging row and the streamed segment reader fold into one accumulate-or-abort `IO` pass; the row-at-a-time appender open is the deleted spelling, the sort is bounded to the materialized run, and no transport and no second changefeed enter. Exemption: `Drain`'s `MessagePackStreamReader` segment loop and `Stage`'s `using` mapped-appender capsule are this traversal's platform-forced statement seam — the ADO bulk-appender and the length-delimited segment reader carry language-owned statement forms, while the surrounding `Materialize` pipeline stays expression-shaped over `IO.liftAsync`, `Map`, and `Bind`; the same seam law governs the `OpenReader`/`OpenWriter` derivation, whose Sep options carry no statement body.

```csharp signature
public sealed class DuckDBOpLogMap : DuckDBAppenderMap<OpLogEntry> {
    public DuckDBOpLogMap() {
        Map(static entry => entry.Sequence);
        Map(static entry => entry.EntityKind);
        Map(static entry => entry.EntityKey);
        Map(static entry => entry.ColumnFamily);
        Map(static entry => entry.Kind.Key);
        Map(static entry => entry.Codec.Key);
        Map(static entry => entry.Actor);
        Map(static entry => entry.OriginStoreId);
        Map(static entry => (long)entry.ContentKey).WithName("content_key");
        Map(static entry => entry.Payload.ToArray()).WithName("payload");
        Map(static entry => entry.Physical.ToUnixTimeTicks());
        Map(static entry => (long)entry.Logical);
    }
}

public static class AnalyticalTraversal {
    extension(TabularExportSpec spec) {
        public IO<ParquetSchemaStamp> Materialize(
            DuckDBConnection lane,
            Stream changefeed,
            string table,
            ParquetSchemaStamp schema) =>
            IO.liftAsync(env => Drain(changefeed, env.Token))
                .Map(static entries => toSeq(entries
                    .OrderBy(static entry => entry.Physical)
                    .ThenBy(static entry => entry.Logical)
                    .ThenBy(static entry => entry.OriginStoreId)))
                .Bind(ordered => IO.lift(() => Stage(lane, table, ordered)))
                .Map(_ => schema);

        private static async ValueTask<Seq<OpLogEntry>> Drain(Stream changefeed, CancellationToken token) {
            using var reader = new MessagePackStreamReader(changefeed);
            var entries = Seq<OpLogEntry>();
            while (await reader.ReadAsync(token) is { } segment)
                entries = entries.Add(MessagePackSerializer.Deserialize<OpLogEntry>(segment, SnapshotCodec.Binary, token));
            return entries;
        }

        private static Unit Stage(DuckDBConnection lane, string table, Seq<OpLogEntry> ordered) {
            using var bulk = lane.CreateAppender<OpLogEntry, DuckDBOpLogMap>(table);
            bulk.AppendRecords([.. ordered]);
            bulk.Close();
            return unit;
        }
    }
}
```

## [7]-[RESEARCH]

- [LIVE_ATTACH]: sqlite_scanner snapshot visibility against a live WAL store under one concurrent writer, and `ATTACH (TYPE sqlite, READ_ONLY)` page-cache coherence across the `Duplicate` lane boundary mid-write.
- [MANAGED_REFINE_OPS]: managed `Centroid`, `ConvexHull`, and `Intersection` constructive-geometry forms over decoded container candidates beyond the catalogued `Geometry.Intersects` / `Geometry.Contains` / `Geometry.Buffer` predicate and `Geometry.Distance` measurement vocabulary; the `NpgsqlNetTopologySuiteAggregateMethodCallTranslatorPlugin` spatial-aggregate translation surface (collect, union, extent aggregate forms) over a grouped geometry column.
- [CRS_RECONCILE]: the PostGIS `ST_Transform` datum-shift accuracy for an EPSG-to-EPSG reprojection over the proj library the postgis extension links, the IFC `IfcMapConversion`/`IfcProjectedCRS` member shapes the `FromMapConversion` affine reads from the Compute interchange graph, and the survey-point similarity-fit residual RMS bound under which a control-point alignment is accepted versus rejected, verified against a live PG18 + postgis server.
- [SEARCH_PROVISIONING_PROBE]: the diskann column-type question — `.api/api-pgvectorscale.md` `[INDEX_DDL]` catalogues diskann over a `vector` column only (ops classes `vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops`), so whether a `halfvec(N)` column admits a diskann index at all, and whether `storage_layout=memory_optimized` with `num_bits_per_dimension` SBQ subsumes the dense-vs-half precision choice, is unverified against the catalogue and must be confirmed against a live PG18 + pgvectorscale server before the row [2]/line-113 `halfvec(N)`-over-diskann claim is settled; and the pg_search BM25 `@@@ pdb.parse(...)` / `pdb.score(<key_col>)` round-trip against a PG18 server carrying pgvectorscale and pg_search 0.24.0 preloaded — the index-route facts the `search.vector.route` and `search.bm25.score` streams emit are confirmed against the live planner.
- [BIT_VECTOR_COLUMNS] (SPIKE, tier-2 live-PG18): the raw server distance operators the catalogued `HammingDistance`/`JaccardDistance` `DbFunctions` emit over a `bit(N)` column — held as the `<bit-op-hamming>`/`<bit-op-jaccard>` placeholders on `VectorMetric.Hamming`/`VectorMetric.Jaccard` because `.api/api-pgvector-ef.md` rows [5]/[6] catalogue the member but not the operator literal. Resolved leg: `VectorMetric.Order` projects the bit-distance `Expression` through the catalogued `DbFunctions` member, so the translated EF query needs no literal. Open leg: only the `HybridRetrieve.Fuse` raw-CTE interpolation of `VectorMetric.Op` requires the literal, so a bit-metric vector branch through `Fuse` is the one path the placeholder still blocks. Closure is either a tier-1 decompile fold adding the operator strings to `.api/api-pgvector-ef.md` (then the `Fuse` interpolation is legal) or the bit branch of `Fuse` routes through a `HammingDistance(...)`/`JaccardDistance(...)` function call rather than the raw operator; also confirm the `halfvec`/`sparsevec`/`bit` column-type round-trip and the `binary_quantize(emb)::bit(N)` expression-index materialization against a live PG18 + pgvector server.
