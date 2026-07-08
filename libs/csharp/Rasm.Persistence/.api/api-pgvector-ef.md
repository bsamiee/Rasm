# [RASM_PERSISTENCE_API_PGVECTOR_EF]

`Pgvector.EntityFrameworkCore` plugs pgvector values into the Npgsql EF Core provider,
supplying CLR-to-store type mapping, distance-function translation, data-source wire
configuration, and design-time scaffolding services. The CLR value types
(`Vector`, `HalfVector`, `SparseVector`) and the ADO wire codecs live in the transitive
`Pgvector` package; the EF plugin depends on them and re-uses `Npgsql.VectorExtensions.UseVector`
to register the wire resolver on the data source. The vector store types (`vector`, `halfvec`,
`sparsevec`) plus the `bit` quantization target are server-side pgvector SQL — the package owns no
index DDL, so the HNSW/ivfflat opclass, build-parameter, and query-GUC vocabulary the EF index
builder projects is catalogued here as the server-side surface the `Store/provisioning#SERVER_EXTENSIONS`
`Index` rows and `Query/retrieval#SEARCH_PROVISIONING_PROBE` planner compose against.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Pgvector.EntityFrameworkCore`
- package: `Pgvector.EntityFrameworkCore` (PostgreSQL/Apache-2.0, ankane)
- assembly: `Pgvector.EntityFrameworkCore`, asset TFM `net8.0` (single-target; binds under the `net10.0` consumer)
- namespace: `Pgvector.EntityFrameworkCore` (plugins/mappings) + `Microsoft.EntityFrameworkCore` (the `UseVector` builder extension)
- value package: `Pgvector` (PostgreSQL/Apache-2.0, ankane), assets `net6.0`/`netstandard2.0`/`net462` — `net6.0` binds under `net10.0`
- value namespace: `Pgvector` (CLR value types), `Pgvector.Npgsql` (wire codecs), `Npgsql` (wire registration extension)
- provider package: `Npgsql.EntityFrameworkCore.PostgreSQL` (provider host; owns the index builder, `HasPostgresExtension`, `PgUnknownBinaryExpression`)
- asset: runtime library (`build/net8.0/Pgvector.EntityFrameworkCore.targets` ships, no analyzer/source-gen)
- rail: store-provider, search-lanes
- server surface: pgvector extension SQL (`vector`/`halfvec`/`sparsevec`/`bit` types, `<->`/`<#>`/`<=>`/`<+>`/`<~>`/`<%>` operators, HNSW/ivfflat AMs, `binary_quantize`/`subvector`) — no managed assembly

## [02]-[PUBLIC_TYPES]

[VALUE_TYPES]: CLR value types — `Pgvector` package, namespace `Pgvector`

| [INDEX] | [SYMBOL]       | [STORE_TYPE] | [ELEMENT]               | [WIRE_CODEC]        | [CAPABILITY]                |
| :-----: | :------------- | :----------- | :---------------------- | :------------------ | :-------------------------- |
|  [01]   | `Vector`       | `vector`     | `float` / `float[]`     | `VectorConverter`   | full-precision dense vector |
|  [02]   | `HalfVector`   | `halfvec`    | `Half` / `Half[]`       | `HalfvecConverter`  | half-precision dense vector |
|  [03]   | `SparseVector` | `sparsevec`  | `float` indices+values  | `SparsevecConverter`| sparse vector               |

`bit(N)` is the fourth pgvector store type but has no `Pgvector` CLR type — it maps from BCL
`System.Collections.BitArray` and carries only the `<~>`/`<%>` bit-distance operators (no managed
codec in this package; `Npgsql` owns the `bit` wire mapping). The `EmbeddingArity.Bit` row at
`Query/retrieval#SEARCH_PROVISIONING_PROBE` materializes it through `binary_quantize(emb)::bit(N)`.

[VECTOR_MEMBERS]: `Pgvector.Vector` — `IEquatable<Vector>`

| [INDEX] | [MEMBER]                          | [RETURN]                | [CAPABILITY]                          |
| :-----: | :-------------------------------- | :---------------------- | :------------------------------------ |
|  [01]   | `Vector(ReadOnlyMemory<float> v)` | ctor                    | wraps existing memory (zero-copy)     |
|  [02]   | `Vector(string s)`                | ctor                    | parses `[f,f,f]` wire format          |
|  [03]   | `Memory`                          | `ReadOnlyMemory<float>` | backing memory (zero-copy read)       |
|  [04]   | `ToArray()`                       | `float[]`               | materialises element array            |
|  [05]   | `ToString()`                      | `string`                | invariant-culture `[f,f,f]` wire text |
|  [06]   | `Equals(Vector?)`, `==`, `!=`     | `bool`                  | element-wise `SequenceEqual`          |

[HALFVECTOR_MEMBERS]: `Pgvector.HalfVector` — `IEquatable<HalfVector>`

| [INDEX] | [MEMBER]                             | [RETURN]               | [CAPABILITY]                          |
| :-----: | :----------------------------------- | :--------------------- | :------------------------------------ |
|  [01]   | `HalfVector(ReadOnlyMemory<Half> v)` | ctor                   | wraps existing memory (zero-copy)     |
|  [02]   | `HalfVector(string s)`               | ctor                   | parses `[h,h,h]` wire format          |
|  [03]   | `Memory`                             | `ReadOnlyMemory<Half>` | backing memory (zero-copy read)       |
|  [04]   | `ToArray()`                          | `Half[]`               | materialises element array            |
|  [05]   | `ToString()`                         | `string`               | invariant-culture `[h,h,h]` wire text |
|  [06]   | `Equals(HalfVector?)`, `==`, `!=`    | `bool`                 | element-wise `SequenceEqual`          |

[SPARSEVECTOR_MEMBERS]: `Pgvector.SparseVector` — value type (no `IEquatable<>` interface; `Equals(SparseVector?)` declared directly)

| [INDEX] | [MEMBER]                                            | [RETURN]                | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------- | :---------------------- | :---------------------------------------------------- |
|  [01]   | `SparseVector(int dimensions, ReadOnlyMemory<int> indices, ReadOnlyMemory<float> values)` | ctor | pre-sorted sparse components; throws on length mismatch |
|  [02]   | `SparseVector(ReadOnlyMemory<float> v)`             | ctor                    | dense-to-sparse: drops zero elements                  |
|  [03]   | `SparseVector(IDictionary<int,float> dictionary, int dimensions)` | ctor      | index→value dictionary; sorts and drops zeros         |
|  [04]   | `SparseVector(string s)`                            | ctor                    | parses `{i:v,...}/d` wire format (1-based indices)     |
|  [05]   | `Dimensions`                                        | `int`                   | total dimension count                                 |
|  [06]   | `Indices`                                           | `ReadOnlyMemory<int>`   | non-zero index positions (0-based in CLR)             |
|  [07]   | `Values`                                            | `ReadOnlyMemory<float>` | non-zero element values                               |
|  [08]   | `ToArray()`                                         | `float[]`               | materialises dense `float[Dimensions]`                |
|  [09]   | `ToString()`                                        | `string`                | invariant `{i:v,...}/d` wire text (1-based, re-shifts) |
|  [10]   | `Equals(SparseVector?)`, `==`, `!=`                 | `bool`                  | dimension + indices + values `SequenceEqual`          |

The wire string is 1-based per pgvector; the `string` ctor subtracts 1 into 0-based `Indices` and
`ToString()` adds 1 back. The dense and dictionary ctors drop zero elements, so a round-trip through
`SparseVector(dense)` is the canonical sparsification.

[WIRE_CODECS]: ADO streaming converters — `Pgvector` package, namespace `Pgvector.Npgsql`

| [INDEX] | [SYMBOL]              | [BASE]                         | [WIRE_LAYOUT]                                    | [CAPABILITY]                       |
| :-----: | :-------------------- | :----------------------------- | :----------------------------------------------- | :--------------------------------- |
|  [01]   | `VectorConverter`     | `PgStreamingConverter<Vector>` | `uint16 dim, uint16 0, float32[dim]`             | dense float binary read/write      |
|  [02]   | `HalfvecConverter`    | `PgStreamingConverter<HalfVector>` | `uint16 dim, uint16 0, uint16[dim]` (half bits) | half-precision binary read/write   |
|  [03]   | `SparsevecConverter`  | `PgStreamingConverter<SparseVector>` | `int32 dim, int32 nnz, int32 0, int32[nnz], float32[nnz]` | sparse binary read/write |
|  [04]   | `VectorTypeInfoResolverFactory` | `PgTypeInfoResolverFactory` | —                                       | binds the three converters + array forms |

Each converter overrides the full `PgStreamingConverter<T>` quartet — `Read`/`ReadAsync`/`Write`/`WriteAsync`
plus `GetSize(SizeContext, T, ref object?)` — so the binary protocol streams without a synchronous-over-async
bridge. `VectorTypeInfoResolverFactory.CreateResolver()` registers the scalar `vector`/`halfvec`/`sparsevec`
type infos and `CreateArrayResolver()` adds the `vector[]`/`halfvec[]`/`sparsevec[]` array forms, so a
`Vector[]` column round-trips without a hand-written reader. These are the boundary the EF plugin and the
bare-driver `UseVector` both wire — internal code never instantiates a converter.

[PLUGIN_TYPES]: plugin admission and mapping — `Pgvector.EntityFrameworkCore` package, namespace `Pgvector.EntityFrameworkCore`

| [INDEX] | [SYMBOL]                                  | [BASE_CONTRACT]                          | [CAPABILITY]                                          |
| :-----: | :---------------------------------------- | :--------------------------------------- | :--------------------------------------------------- |
|  [01]   | `VectorDbContextOptionsExtension`         | `IDbContextOptionsExtension`             | carries plugin policy; `LogFragment "using vector "` |
|  [02]   | `VectorTypeMapping`                       | `RelationalTypeMapping`                  | maps `Vector`/`HalfVector`/`SparseVector`; sizes `vector(N)` |
|  [03]   | `VectorTypeMappingSourcePlugin`           | `IRelationalTypeMappingSourcePlugin`     | resolves `vector`/`halfvec`/`sparsevec` ↔ CLR both ways |
|  [04]   | `VectorDataSourceConfigurationPlugin`     | `INpgsqlDataSourceConfigurationPlugin`   | calls `VectorExtensions.UseVector` on the data source |
|  [05]   | `VectorDbFunctionsExtensions`             | static                                   | the six distance methods (server-translated)         |
|  [06]   | `VectorDbFunctionsTranslatorPlugin`       | `IMethodCallTranslatorPlugin`            | translates distance calls to `PgUnknownBinaryExpression` |
|  [07]   | `VectorCodeGeneratorPlugin`               | `ProviderCodeGeneratorPlugin`            | emits `.UseVector()` in scaffolded `OnConfiguring`   |
|  [08]   | `VectorDesignTimeServices`                | `IDesignTimeServices`                    | admits mapping + code-gen plugins for scaffolding    |

`VectorDbContextOptionsExtension.ApplyServices` is the wiring root: it `TryAdd`s
`VectorDataSourceConfigurationPlugin` and `VectorDbFunctionsTranslatorPlugin` and adds
`VectorTypeMappingSourcePlugin` as a singleton onto the Npgsql EF service collection, so one
`UseVector` call admits wire config, distance translation, and type mapping together.

## [03]-[ENTRYPOINTS]

[ADMISSION]: the dual `UseVector` surface — note the return-type asymmetry

| [INDEX] | [SURFACE]                                                            | [HOME_NAMESPACE]              | [RETURNS]                       | [CALL_SHAPE]                              | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------ | :---------------------------- | :------------------------------ | :---------------------------------------- | :---------------------------------------- |
|  [01]   | `VectorDbContextOptionsBuilderExtensions.UseVector()`               | `Microsoft.EntityFrameworkCore` | `NpgsqlDbContextOptionsBuilder` (preserving) | `NpgsqlDbContextOptionsBuilder` extension | admits the EF plugin (mapping + translate + wire) |
|  [02]   | `VectorExtensions.UseVector(this INpgsqlTypeMapper)`                | `Npgsql`                      | `INpgsqlTypeMapper` (**erased**)             | `NpgsqlDataSourceBuilder` extension       | registers `VectorTypeInfoResolverFactory` for wire decode only |

Surface [01] lives in `Microsoft.EntityFrameworkCore` (not `Pgvector.EntityFrameworkCore`), so the
EF admission call resolves without a `Pgvector.EntityFrameworkCore` using directive once
`Microsoft.EntityFrameworkCore` is in scope. It is builder-preserving and chains cleanly inside an
`UseNpgsql(...)` lambda.

Surface [02] is the bare-driver path used when configuring an `NpgsqlDataSourceBuilder` without the
EF plugin (the EF plugin invokes it automatically through `VectorDataSourceConfigurationPlugin`).
`NpgsqlDataSourceBuilder` is `sealed` and implements `INpgsqlTypeMapper` (`.api/api-npgsql.md`), so
`builder.UseVector()` returns the erased `INpgsqlTypeMapper`, not the concrete builder — unlike
the builder-preserving `UseNodaTime`/`UseNetTopologySuite` generic extensions. The
`Store/provisioning#PROVISIONING` connect ceremony therefore binds vector via tuple-capture
(`(builder.UseVector(), builder).Item2`) so the concrete builder type survives the chain. Internally
it forwards to `AddTypeInfoResolverFactory(new VectorTypeInfoResolverFactory())` (`.api/api-npgsql.md`).

[TYPE_MAPPING]: `VectorTypeMapping` — `RelationalTypeMapping`

| [INDEX] | [SURFACE]                                                      | [CALL_SHAPE]                  | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------- | :---------------------------- | :------------------------------------------------- |
|  [01]   | `VectorTypeMapping.Default`                                    | `static VectorTypeMapping`    | unsized `vector` ↔ `Vector` (no `(N)`)             |
|  [02]   | `VectorTypeMapping(string storeType, Type clrType, int? size = null)` | constructor           | sizes the column; `StoreTypePostfix.Size` emits `<storeType>(<size>)` |

The mapping uses `StoreTypePostfix.Size`, so a non-null `size` renders `vector(N)`/`halfvec(N)`/`sparsevec(N)`;
EF `HasColumnType("vector(1536)")` and the plugin parse the size both ways.
`VectorTypeMappingSourcePlugin.FindMapping` resolves by store-type-name-base when the CLR type is
unknown (`"vector"→Vector`, `"halfvec"→HalfVector`, `"sparsevec"→SparseVector`) and by CLR type
otherwise (`typeof(Vector)→"vector"`, …), so model-first (`HasColumnType`) and convention-first
(property CLR type) both bind without a per-column mapping registration.

[DISTANCE_FUNCTIONS]: `VectorDbFunctionsExtensions` — namespace `Pgvector.EntityFrameworkCore`

| [INDEX] | [CLR_METHOD]      | [SQL_OPERATOR] | [APPLICABLE_STORE_TYPES]            | [CAPABILITY]                |
| :-----: | :---------------- | :------------- | :--------------------------------- | :-------------------------- |
|  [01]   | `L2Distance`      | `<->`          | `vector`, `halfvec`, `sparsevec`   | Euclidean distance          |
|  [02]   | `MaxInnerProduct` | `<#>`          | `vector`, `halfvec`, `sparsevec`   | negative inner product      |
|  [03]   | `CosineDistance`  | `<=>`          | `vector`, `halfvec`, `sparsevec`   | cosine distance             |
|  [04]   | `L1Distance`      | `<+>`          | `vector`, `halfvec`, `sparsevec`   | taxicab (L1) distance       |
|  [05]   | `HammingDistance` | `<~>`          | `bit`                              | Hamming distance            |
|  [06]   | `JaccardDistance` | `<%>`          | `bit`                              | Jaccard distance            |

All six are declared `this object a, object b` returning `double`. `VectorDbFunctionsTranslatorPlugin`
matches the `MethodInfo` by reference, picks the operator, and emits a `PgUnknownBinaryExpression`
(from `Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal`) wrapping both operands under
the return type's default mapping. Client-side invocation throws
`InvalidOperationException(CoreStrings.FunctionOnClient(...))`, so the methods are LINQ-translation
markers only. The applicable-store-type column is a pgvector server constraint, not a translator gate
— the translator emits the operator for any operand pair, and the server rejects an unsupported
type/operator pair. The `Query/retrieval#SEARCH_PROVISIONING_PROBE` `VectorMetric` rows bind each method by
`nameof(VectorDbFunctionsExtensions.L2Distance)` and build the `ORDER BY` projection with
`Expression.Call(typeof(VectorDbFunctionsExtensions), Fn, Type.EmptyTypes, columnExpr, Expression.Constant(new Vector(probe)))`.

[INDEX_DECLARATION]: EF index builder — `NpgsqlIndexBuilderExtensions` in `Npgsql.EntityFrameworkCore.PostgreSQL` (`.api/api-npgsql-ef.md`)

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                  | [CAPABILITY]                            |
| :-----: | :-------------------- | :----------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `HasMethod`           | `IndexBuilder.HasMethod(string? method)` (+ `<TEntity>`)     | selects `hnsw` or `ivfflat`             |
|  [02]   | `HasOperators`        | `IndexBuilder.HasOperators(params string[]? operators)` (+ `<TEntity>`) | sets ops class e.g. `vector_cosine_ops` |
|  [03]   | `HasStorageParameter` | `IndexBuilder.HasStorageParameter(string parameterName, object? parameterValue)` (+ `<TEntity>`) | sets `m`, `ef_construction`, `lists` |

The vector AM/opclass/build-parameter literals these rows carry are server-side pgvector SQL, catalogued in `[SERVER_SURFACE]`.

## [04]-[SERVER_SURFACE]

The pgvector extension carries no managed assembly; these are the server SQL the EF index builder
(`HasMethod`/`HasOperators`/`HasStorageParameter`) and the `Store/provisioning#SERVER_EXTENSIONS` `Index`/`Extension`
rows project, and the `Query/retrieval#SEARCH_PROVISIONING_PROBE` planner reads. `CREATE EXTENSION vector` is declared
through `HasPostgresExtension("vector")` (`.api/api-npgsql-ef.md`); `vectorscale` pulls it as a CASCADE
dependency (`.api/api-pgvectorscale.md`).

[INDEX_OPCLASSES]: per-store-type opclass per access method

| [INDEX] | [STORE_TYPE] | [HNSW_OPCLASSES]                                                        | [IVFFLAT_OPCLASSES]                              |
| :-----: | :----------- | :--------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `vector`     | `vector_l2_ops`, `vector_ip_ops`, `vector_cosine_ops`, `vector_l1_ops` | `vector_l2_ops`, `vector_ip_ops`, `vector_cosine_ops` |
|  [02]   | `halfvec`    | `halfvec_l2_ops`, `halfvec_ip_ops`, `halfvec_cosine_ops`, `halfvec_l1_ops` | `halfvec_l2_ops`                              |
|  [03]   | `sparsevec`  | `sparsevec_l2_ops`, `sparsevec_ip_ops`, `sparsevec_cosine_ops`, `sparsevec_l1_ops` | _(none — HNSW only)_                  |
|  [04]   | `bit`        | `bit_hamming_ops`, `bit_jaccard_ops`                                   | `bit_hamming_ops`                                |

`vectorscale`'s `diskann` AM is a third index path over a `vector` column only, under
`vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops` (`.api/api-pgvectorscale.md`).

[BUILD_AND_QUERY]: build parameters (`HasStorageParameter`) and session GUCs (search lane sets at query time)

| [INDEX] | [AM]      | [BUILD_PARAMS]                       | [QUERY_GUCS]                                                            |
| :-----: | :-------- | :----------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `hnsw`    | `m` (16), `ef_construction` (64)     | `hnsw.ef_search` (40), `hnsw.iterative_scan`, `hnsw.max_scan_tuples` (20000), `hnsw.scan_mem_multiplier` (1) |
|  [02]   | `ivfflat` | `lists` (required)                   | `ivfflat.probes` (1), `ivfflat.max_probes`, `ivfflat.iterative_scan`   |

[UTILITY_FUNCTIONS]: server functions the bit-quantization and subvector forms compose

| [INDEX] | [FUNCTION]                              | [RESULT]    | [CAPABILITY]                                              |
| :-----: | :-------------------------------------- | :---------- | :------------------------------------------------------- |
|  [01]   | `binary_quantize(vector)`               | `bit`       | quantizes a dense embedding to a `bit` for `<~>`/`<%>` ANN; backs the `EmbeddingArity.Bit` `binary_quantize(emb)::bit(N)` expression-index column |
|  [02]   | `subvector(vector, integer, integer)`   | `vector`    | slices a contiguous dimension window for sub-space indexing |

## [05]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: the pgvector plugin is the vector-mapping policy of the `postgres-server` `StoreProfile` row (the only `Vector: true` engine; sqlite/duckdb/blob rows are `Vector: false`)
- EF admission root: `UseVector()` on the `NpgsqlDbContextOptionsBuilder` inside `UseNpgsql`
- driver admission root: `VectorExtensions.UseVector(NpgsqlDataSourceBuilder)` (erased return — tuple-capture at the connect ceremony), auto-invoked by the EF plugin
- mapping root: `VectorTypeMapping` over `Vector`/`HalfVector`/`SparseVector`, sized by `StoreTypePostfix.Size`
- query root: distance-function translation (`VectorDbFunctionsExtensions` → `PgUnknownBinaryExpression`) inside profile queries

[INTEGRATION_RAIL]: how the plugin stacks into the one Persistence search rail
- The `Query/retrieval#SEARCH_PROVISIONING_PROBE` `EmbeddingArity` `[SmartEnum]` is the single CLR-to-store axis: `Dense→Vector→vector(N)`, `Half→HalfVector→halfvec(N)`, `Sparse→SparseVector→sparsevec(N)`, `Bit→BitArray→bit(N)`. The first three CLR types and their wire codecs come from this catalog; `Bit` rides `binary_quantize(emb)::bit(N)` from `[UTILITY_FUNCTIONS]`.
- `VectorMetric` binds each distance method by `nameof(VectorDbFunctionsExtensions.X)` and projects `ORDER BY` through `Expression.Call` over a `Pgvector.Vector` probe constant; the six metrics are one closed axis whose `Op` column carries the `<->`/`<#>`/`<=>`/`<+>`/`<~>`/`<%>` operator literal for the raw-CTE fusion leg and whose `Fn` column carries the translator member for the typed `Order` leg. A `Bit` row routes through `HammingDistance`/`JaccardDistance` over a `bit` column; a non-`Bit` row routes through the four dense methods.
- The index path is server SQL, not a managed call: `HasMethod("hnsw")`/`HasOperators("vector_cosine_ops")`/`HasStorageParameter("m", 16)` declare an HNSW or ivfflat index via the Npgsql index builder against the `[INDEX_OPCLASSES]` matrix; `vectorscale`'s `diskann` AM and `pg_search`'s `bm25` AM carry no EF translator, so their index DDL lands on `Store/provisioning#SERVER_EXTENSIONS` as raw migration SQL while queries reuse these catalogued distance functions. The exact brute-force scan stays the always-present correctness baseline; HNSW/ivfflat/diskann are approximate routes the `search.vector.route` fact discriminates.
- `FusionRank.Fuse` composes the `VectorMetric`-ordered vector branch and the `pg_search` BM25 branch into one reciprocal-rank-fusion CTE; the vector branch's `ORDER BY embedding <op> $probe` uses the `VectorMetric.Op` literal from this catalog, and the dense embedding it probes is generated upstream at `Compute/models#INFERENCE_MODES` `Embed`.
- Wire round-trips ride `VectorTypeInfoResolverFactory` (scalar + array forms) registered by `UseVector`; a `Vector[]` column needs no hand-written reader. The codecs override the full `PgStreamingConverter<T>` async-mirror quartet, so the binary protocol streams without a sync-over-async bridge.

[LOCAL_ADMISSION]:
- Vector mapping enters through the `postgres-server` `StoreProfile` declaration via `UseVector` — a vector-branded service family is the rejected form.
- The `vector` extension is a profile-declared PostgreSQL extension via `HasPostgresExtension("vector")`; `vectorscale CASCADE` is the dependency-pulling form.
- Distance projections are query facts inside profile queries, never a client-side method call.
- Vector dimensions are column metadata: `HasColumnType("vector(N)")` or the `int? size` ctor; `HalfVector→halfvec(N)`, `SparseVector→sparsevec(N)` follow the same `StoreTypePostfix.Size` rule.
- Index AM/opclass/build-parameter/GUC literals are the `Store/provisioning#SERVER_EXTENSIONS` `Index` row and search-lane GUC concern; this catalog states the vocabulary, not a parallel index emitter.

[RAIL_LAW]:
- Package: `Pgvector.EntityFrameworkCore` (+ transitive `Pgvector`, server-side pgvector SQL)
- Owns: pgvector CLR types, wire codecs, EF type mapping, distance translation, and the server index/opclass/GUC vocabulary for the Npgsql EF PostgreSQL provider
- Accept: profile-declared vector mapping, distance queries, and HNSW/ivfflat index declarations through the Npgsql index builder
- Reject: vector-branded service families; client-side distance calls; a hand-spelled `CREATE EXTENSION vector` beside the `HasPostgresExtension` annotation; a parallel column-type enum duplicating the `EmbeddingArity` axis
