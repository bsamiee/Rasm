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
- Cases: Containment | KeyExistence | None
- Packages: Microsoft.EntityFrameworkCore.Sqlite, Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new document column is one `JsonIndex` policy value on its index row; the text-preserving json column type lands later as one designed-only growth row; zero new surface.
- Boundary: `ComplexProperty(x => x.Detail, d => d.ToJson())` is the only document mapping — the owned-entity ToJson spelling is the rejected form; jsonb is the canonical server column while the embedded column stays TEXT json with `json_extract` translation, so `JsonIndex.None` is the only embedded row and the lane is index-asymmetric by construction — query shapes never assume jsonpath-index parity across profiles; NodaTime values inside documents persist through the registered plugin converters, never a DateTime sentinel; foreign schemaless payloads enter as one JsonDocument column through exactly one boundary converter; tz-suffixed path functions never enter an expression index.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class JsonIndex {
    public static readonly JsonIndex Containment = new("jsonb_path_ops");
    public static readonly JsonIndex KeyExistence = new("jsonb_ops");
    public static readonly JsonIndex None = new("none");
}
```

| [INDEX] | [SURFACE]                          | [STORE]         | [ROUTE]                | [LAW]                                                       |
| :-----: | ---------------------------------- | --------------- | ---------------------- | ----------------------------------------------------------- |
|   [1]   | jsonb document column              | postgres-server | provider mapping       | canonical document column; decomposed binary, set-indexable |
|   [2]   | `@?` / `@@` path predicates        | postgres-server | raw SQL over GIN       | containment and path predicates under the declared class    |
|   [3]   | `jsonb_path_query` function family | postgres-server | raw SQL                | non-tz forms only inside expression indexes                 |
|   [4]   | `JSON_TABLE` shredding             | postgres-server | FromSql projection     | SQL/JSON standard row-shred surface for set projections     |
|   [5]   | `json_extract` translation         | sqlite rows     | LINQ translation       | TEXT json column; zero index analogue                       |
|   [6]   | ExecuteUpdate into JSON paths      | both providers  | set-based write        | partial document update without entity materialization      |
|   [7]   | JsonDocument escape hatch          | both providers  | one boundary converter | schemaless foreign payload admission                        |

## [4]-[SEARCH_LANES]

- Owner: `VectorQuery` and `FullTextQuery` shapes with the `VectorMetric`, `FullTextMode`, and `EmbeddingIdentity` vocabularies.
- Cases: `VectorMetric` l2 | cosine | inner-product | l1; `FullTextMode` match | prefix | phrase | websearch
- Entry: `public static EmbeddingIdentity Of(ReadOnlySpan<byte> content, string modelId)` — pure value; embedding identity is content hash times model id, so re-embedding dedupes structurally.
- Packages: Pgvector.EntityFrameworkCore, Microsoft.Data.Sqlite, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new embedding family is one row carrying model id, dimensionality, and column type; a new tokenizer or rank grammar is one policy value on its provider row; a bit-vector metric family (hamming, jaccard) is one `VectorMetric` row paired with one bit-probe arity on `VectorQuery`; zero new surface.
- Boundary: one query shape per search concern projects to every provider — provider-twin query shapes are the deleted pattern; `VectorMetric.Op` carries the distance operator that the server row projects through `L2Distance`, `CosineDistance`, `MaxInnerProduct`, and `L1Distance`; the embedded vector route stays gate-resolved with the brute-force scan as the always-present correctness baseline; fuzzy admission normalizes through unaccent and prefilters through trigram similarity before rank.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class VectorMetric {
    public static readonly VectorMetric L2 = new("l2", op: "<->");
    public static readonly VectorMetric Cosine = new("cosine", op: "<=>");
    public static readonly VectorMetric InnerProduct = new("inner-product", op: "<#>");
    public static readonly VectorMetric L1 = new("l1", op: "<+>");

    public string Op { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class FullTextMode {
    public static readonly FullTextMode Match = new("match");
    public static readonly FullTextMode Prefix = new("prefix");
    public static readonly FullTextMode Phrase = new("phrase");
    public static readonly FullTextMode Websearch = new("websearch");
}

public readonly record struct EmbeddingIdentity(UInt128 ContentHash, string ModelId) {
    public static EmbeddingIdentity Of(ReadOnlySpan<byte> content, string modelId) =>
        new(XxHash128.HashToUInt128(content), modelId);
}

public sealed record VectorQuery<TRow>(
    ReadOnlyMemory<float> Probe,
    int K,
    VectorMetric Metric,
    Option<Expression<Func<TRow, bool>>> Filter = default);

public sealed record FullTextQuery(
    string Terms,
    FullTextMode Mode,
    bool Rank,
    bool Highlight);
```

| [INDEX] | [ROW]                        | [STORE]         | [SURFACE]                                                          | [LAW]                                                |
| :-----: | ---------------------------- | --------------- | ------------------------------------------------------------------ | ---------------------------------------------------- |
|   [1]   | vector / halfvec / sparsevec | postgres-server | `UseVector` column mapping                                         | dimensionality fixed per embedding-family row        |
|   [2]   | HNSW + IVFFlat index rows    | postgres-server | index metadata on schema rows                                      | approximate routes; exact scan stays the baseline    |
|   [3]   | distance ordering            | postgres-server | `L2Distance` / `CosineDistance` / `MaxInnerProduct` / `L1Distance` | metric row selects the projected operator            |
|   [4]   | gated embedded vector table  | sqlite rows     | loadable-extension gate                                            | brute-force fallback present on every profile        |
|   [5]   | FTS5 external-content tables | sqlite rows     | virtual table + rebuild triggers                                   | unicode61 and trigram tokenizer rows; bm25 rank      |
|   [6]   | tsvector generated column    | postgres-server | generated column + GIN                                             | websearch_to_tsquery admission; ts_rank; ts_headline |
|   [7]   | fuzzy rows                   | postgres-server | pg_trgm similarity, unaccent, fuzzystrmatch                        | candidate prefilter and normalization before rank    |

## [5]-[GEO_LANES]

- Owner: `GeoLayer` container row over the NetTopologySuite value chain.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, NetTopologySuite.IO.GeoPackage, Microsoft.Data.Sqlite
- Growth: a new geometry concern is one `GeoLayer` row or one driver-native type row; zero new surface.
- Boundary: NetTopologySuite is the store boundary projection of the one canonical wire geometry — a third managed geometry representation is the named defect; server geometry and geography columns enter through the single `UseNetTopologySuite` admission on provider options and data source; container window reads prefilter by `Envelope` against the R*Tree index and refine with managed `Geometry` predicates after blob decode; the `Ordinates` and `RepairRings` columns parameterize the GPB codec — `GeoPackageGeoWriter.HandleOrdinates` caps written ordinates and selects the header envelope kind, `HandleSRID` stamps the row's `Srid` on decode, and ring repair runs only where the row arms it; geodesy is owned server-side and the earthdistance route stays rejected.

```csharp signature
public sealed record GeoLayer(string Table, string GeometryColumn, int Srid, Envelope Extent, Ordinates Ordinates, bool RepairRings);
```

| [INDEX] | [CONCERN]               | [RESIDENCE]           | [SURFACE]                                                                             | [LAW]                                                                                                                                                                      |
| :-----: | ----------------------- | --------------------- | ------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | geometry / geography    | postgres-server       | NetTopologySuite column mapping                                                       | spatial predicates translate; indexes ride schema rows                                                                                                                     |
|   [2]   | embedded geo container  | sqlite container file | `GeoPackageGeoReader` / `GeoPackageGeoWriter`                                         | GPB blob codec; R*Tree window then managed refine                                                                                                                          |
|   [3]   | hierarchy paths         | postgres-server       | ltree driver-native rows                                                              | assembly and part trees; lquery operators raw SQL                                                                                                                          |
|   [4]   | N-dimensional intervals | postgres-server       | cube driver-native rows                                                               | GiST nearest-neighbor concerns                                                                                                                                             |
|   [5]   | scalar spans            | postgres-server       | `NpgsqlRange<T>` ranges and multiranges                                               | overlap predicates and exclusion constraints; Interval and DateInterval spans ride the NodaTime range mappings with `RangeAgg` / `RangeIntersectAgg` aggregate projections |
|   [6]   | geodesic query forms    | postgres-server       | `Distance` / `IsWithinDistance` / `DistanceKnn` / `Transform` / `Force2D` DbFunctions | spheroid math, KNN ordering, and SRID reprojection ride translated SQL, never client refine                                                                                |

## [6]-[ANALYTICAL_LANE]

- Owner: `TabularExportSpec` and `ParquetSchemaStamp` export policy records.
- Packages: DuckDB.NET.Data.Full, Sep, LanguageExt.Core, BCL inbox
- Growth: a new export shape is one `TabularExportSpec` row; a new parquet generation is one `ParquetSchemaStamp` row; a new shred source is one read_json row; zero new surface.
- Boundary: the analytical lane reads and projects and never owns source-of-truth writes; the server boundary is parquet-export-only — postgres_scanner stays rejected; live embedded reads ride one `ATTACH (TYPE sqlite, READ_ONLY)` row gated by the visibility research row; parquet, json, and icu ride statically inside the bundled engine while the sqlite scanner autoloads, so offline posture vendors the platform-matched extension binary beside the engine; xlsx and a second tabular engine stay rejected with Sep and parquet as the owners; Sep rows and columns are ref-struct projections that never escape the read scope and typed columns parse through `ISpanParsable` values, never string materialization; DuckDB fts, spatial, remote-filesystem, and vss rows stay out — full-text, geometry, remote-blob, and vector concerns have named lane owners and vss persistence is experimental with documented loss on unclean shutdown.

```csharp signature
public sealed record TabularExportSpec(
    Seq<string> Columns,
    bool WriteHeader,
    CultureInfo Culture,
    char Separator,
    bool Strict,
    SepTrim Trim,
    bool ParallelRead);

public sealed record ParquetSchemaStamp(string Stamp, FrozenDictionary<string, string> Fields);
```

| [INDEX] | [ROW]                     | [SURFACE]                                                                       | [LAW]                                                                                                                                              |
| :-----: | ------------------------- | ------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | live store attach         | `ATTACH 'store.db' AS live (TYPE sqlite, READ_ONLY)` raw SQL on `DuckDBCommand` | zero-export reads of the embedded store                                                                                                            |
|   [2]   | parquet export            | `COPY` rollup `TO` parquet via `DuckDBCommand`                                  | the only dashboard export; rows carry the stamp column                                                                                             |
|   [3]   | json shred                | `read_json` / `json_transform`                                                  | typed STRUCT and LIST coercion of receipt streams                                                                                                  |
|   [4]   | rollup pushdown           | group, window, percentile raw SQL                                               | aggregate shapes stay off the EF rails                                                                                                             |
|   [5]   | collation                 | icu rows inside the engine                                                      | locale-aware ordering inside exports                                                                                                               |
|   [6]   | tabular import/export     | `SepReader` / `SepWriter` under one spec record                                 | culture-aware parse; column selection; header policy; `Strict` hardening, `SepTrim` trim, and `ParallelEnumerate` projection ride the spec columns |
|   [7]   | receipt staging           | `CreateAppender` rows via `DuckDBAppender`                                      | derived rollup staging only; `Close` seals appended rows                                                                                           |
|   [8]   | long-rollup observability | `UseStreamingMode` + `GetQueryProgress` on `DuckDBCommand`                      | bounded-memory result streams; progress projects as maintain-kind facts                                                                            |

## [7]-[RESEARCH]

| [INDEX] | [ITEM]                                                                                                               | [PROOF]                                                                                                                                                                  | [GATE]          |
| :-----: | -------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------- |
|   [1]   | sqlite_scanner snapshot visibility against a live WAL store under one concurrent writer                              | libs/csharp/Rasm.Persistence/specs/AnalyticalAttach.spec.cs racing a writer transaction beside ATTACH (TYPE sqlite, READ_ONLY) reads                                     | ANALYTICAL_LANE |
|   [2]   | Predicate, ExecuteUpdate, and index translation parity for `ToJson` complex documents across the two store providers | libs/csharp/Rasm.Persistence/specs/DocumentLane.spec.cs asserting one document query shape translates on both providers and `JsonIndex.None` stays the only embedded row | DOCUMENT_LANE   |
|   [3]   | GeoPackage rtree-backed spatial index trigger conformance over the compiled R*Tree module                            | libs/csharp/Rasm.Persistence/specs/GeoPackageWindow.spec.cs round-tripping GeoPackageGeoWriter rows through an Envelope window query                                     | GEO_LANES       |
