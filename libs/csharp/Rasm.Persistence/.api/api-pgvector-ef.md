# [RASM_PERSISTENCE_API_PGVECTOR_EF]

`Pgvector.EntityFrameworkCore` binds pgvector store types onto the Npgsql EF Core provider — type mapping, distance-operator translation, codec wiring, and design-time scaffolding — all admitted by one `UseVector` call. CLR value types and their binary codecs ship in the transitive `Pgvector` package; access methods, operator classes, index knobs, and vector functions are server SQL this catalog owns as the vocabulary provisioning and search lanes project. Vector retrieval on the `postgres-server` store profile composes from here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Pgvector.EntityFrameworkCore`
- package: `Pgvector.EntityFrameworkCore` (PostgreSQL, ankane)
- assembly: `Pgvector.EntityFrameworkCore`
- namespace: `Pgvector.EntityFrameworkCore` (plugins, mappings), `Microsoft.EntityFrameworkCore` (the options-builder extension)
- depends: `Pgvector` value types, `Npgsql.EntityFrameworkCore.PostgreSQL` provider host
- rail: store-provider, search-lanes

[PACKAGE_SURFACE]: `Pgvector`
- package: `Pgvector` (PostgreSQL, ankane)
- assembly: `Pgvector`
- namespace: `Pgvector` (CLR value types), `Pgvector.Npgsql` (streaming codecs), `Npgsql` (the type-mapper extension)
- rail: store-codec

[PACKAGE_SURFACE]: `vector` (pgvector server extension)
- package: pgvector, installed as extension `vector`
- asset: server SQL, no managed assembly
- rail: search-provisioning

## [02]-[PUBLIC_TYPES]

[VALUE_TYPES]: reference types in `Pgvector`, each pairing one store type with one streaming codec — `Vector`/`vector`/`VectorConverter`, `HalfVector`/`halfvec`/`HalfvecConverter`, `SparseVector`/`sparsevec`/`SparsevecConverter`.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------- | :------------ | :----------------------------------------------- |
|  [01]   | `Vector`       | class         | dense `float` embedding, `IEquatable<Vector>`    |
|  [02]   | `HalfVector`   | class         | dense `Half` embedding, `IEquatable<HalfVector>` |
|  [03]   | `SparseVector` | class         | index/value pairs over a declared dimension      |

`bit(N)` is the fourth pgvector store type and carries no `Pgvector` CLR type: `Npgsql` maps it from BCL `System.Collections.BitArray`, and the `EmbeddingArity.Bit` row at `Query/retrieval#SEARCH_PROVISIONING_PROBE` materializes it through `binary_quantize(emb)::bit(N)`.

[WIRE_CODECS]: `Pgvector.Npgsql` converters deriving `PgStreamingConverter<T>`, each overriding the whole `Read`/`ReadAsync`/`Write`/`WriteAsync`/`GetSize` quartet so the binary protocol streams with no sync-over-async bridge.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `VectorConverter`               | class         | `uint16 dim, uint16 0, float32[dim]`                      |
|  [02]   | `HalfvecConverter`              | class         | `uint16 dim, uint16 0, uint16[dim]` half bits             |
|  [03]   | `SparsevecConverter`            | class         | `int32 dim, int32 nnz, int32 0, int32[nnz], float32[nnz]` |
|  [04]   | `VectorTypeInfoResolverFactory` | class         | `PgTypeInfoResolverFactory`; scalar and array resolvers   |

[PLUGIN_TYPES]: `VectorDbContextOptionsExtension.ApplyServices` is the wiring root, `TryAdd`ing the data-source and translator plugins and registering the mapping-source plugin as a singleton, so one `UseVector` admits wire config, translation, and mapping together.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :------------------------------------ | :------------ | :----------------------------------------------------------- |
|  [01]   | `VectorDbContextOptionsExtension`     | class         | `IDbContextOptionsExtension`; log fragment `using vector `   |
|  [02]   | `VectorTypeMapping`                   | class         | `RelationalTypeMapping`; sizes `vector(N)`                   |
|  [03]   | `VectorTypeMappingSourcePlugin`       | class         | `IRelationalTypeMappingSourcePlugin`; store-type ↔ CLR       |
|  [04]   | `VectorDataSourceConfigurationPlugin` | class         | `INpgsqlDataSourceConfigurationPlugin`; calls `UseVector`    |
|  [05]   | `VectorDbFunctionsExtensions`         | static class  | the six server-translated distance methods                   |
|  [06]   | `VectorDbFunctionsTranslatorPlugin`   | class         | `IMethodCallTranslatorPlugin`; distance call to SQL operator |
|  [07]   | `VectorCodeGeneratorPlugin`           | class         | `ProviderCodeGeneratorPlugin`; scaffolds `.UseVector()`      |
|  [08]   | `VectorDesignTimeServices`            | class         | `IDesignTimeServices`; admits mapping and code-gen plugins   |

## [03]-[ENTRYPOINTS]

[ADMISSION]: `UseVector` admits the surface on either builder.

| [INDEX] | [SURFACE]                                                                              | [SHAPE] | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `VectorDbContextOptionsBuilderExtensions.UseVector() -> NpgsqlDbContextOptionsBuilder` | static  | admits the plugin inside `UseNpgsql` |
|  [02]   | `VectorExtensions.UseVector(INpgsqlTypeMapper) -> INpgsqlTypeMapper`                   | static  | registers the resolver on a mapper   |

- Surface [01] lives in `Microsoft.EntityFrameworkCore`, resolving with no `Pgvector.EntityFrameworkCore` using directive and chaining builder-preserving inside an `UseNpgsql(...)` lambda.
- Surface [02] is the bare-driver path the EF plugin auto-invokes, forwarding to `AddTypeInfoResolverFactory(new VectorTypeInfoResolverFactory())`; sealed `NpgsqlDataSourceBuilder` gets back the erased interface, so the `Store/provisioning#PROVISIONING` ceremony recovers the concrete builder by tuple-capture, `(builder.UseVector(), builder).Item2`.

[VALUE_MEMBERS]: `HalfVector` mirrors `Vector` member for member with `Half` substituted for `float`.

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `Vector(ReadOnlyMemory<float>)`                                 | ctor     | wraps existing memory, zero-copy         |
|  [02]   | `Vector(string)`                                                | ctor     | parses `[f,f,f]` invariant text          |
|  [03]   | `Vector.Memory -> ReadOnlyMemory<float>`                        | property | zero-copy backing read                   |
|  [04]   | `Vector.ToArray() -> float[]`                                   | instance | materializes the element array           |
|  [05]   | `Vector.ToString() -> string`                                   | instance | emits `[f,f,f]` invariant text           |
|  [06]   | `Vector.Equals(Vector) -> bool`                                 | instance | element-wise `SequenceEqual`             |
|  [07]   | `SparseVector(int, ReadOnlyMemory<int>, ReadOnlyMemory<float>)` | ctor     | pre-sorted pairs; length mismatch throws |
|  [08]   | `SparseVector(ReadOnlyMemory<float>)`                           | ctor     | sparsifies a dense span, dropping zeros  |
|  [09]   | `SparseVector(IDictionary<int, float>, int)`                    | ctor     | sorts by index, dropping zeros           |
|  [10]   | `SparseVector(string)`                                          | ctor     | parses `{i:v,...}/d` 1-based text        |
|  [11]   | `SparseVector.Dimensions -> int`                                | property | declared dimension count                 |
|  [12]   | `SparseVector.Indices -> ReadOnlyMemory<int>`                   | property | 0-based non-zero positions               |
|  [13]   | `SparseVector.Values -> ReadOnlyMemory<float>`                  | property | non-zero element values                  |
|  [14]   | `SparseVector.ToArray() -> float[]`                             | instance | materializes dense `float[Dimensions]`   |
|  [15]   | `SparseVector.ToString() -> string`                             | instance | emits `{i:v,...}/d` 1-based text         |
|  [16]   | `SparseVector.Equals(SparseVector) -> bool`                     | instance | dimension, index, and value equality     |

- Equality: `==`, `!=`, and an element-folded `GetHashCode()` ride every value type beside `Equals`; `SparseVector` declares `Equals(SparseVector?)` directly where `Vector` and `HalfVector` carry `IEquatable<T>`.
- Sparse wire text is 1-based: the string ctor subtracts one into 0-based `Indices`, `ToString()` adds it back, and the dense and dictionary ctors drop zero elements, so `new SparseVector(dense)` is the canonical sparsification.

[TYPE_MAPPING]: `VectorTypeMapping` renders through `StoreTypePostfix.Size`, so a non-null size emits `vector(N)`/`halfvec(N)`/`sparsevec(N)`. `FindMapping` resolves by store-type-name base when the CLR type is absent and by CLR type otherwise, so `HasColumnType("vector(1536)")` and a bare CLR property both bind with no per-column registration.

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :--------------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `VectorTypeMapping.Default`                                            | property | unsized `vector` ↔ `Vector`    |
|  [02]   | `VectorTypeMapping(string, Type, int?)`                                | ctor     | sizes the column               |
|  [03]   | `VectorTypeMappingSourcePlugin.FindMapping(RelationalTypeMappingInfo)` | instance | resolves the mapping both ways |

[DISTANCE_FUNCTIONS]: `VectorDbFunctionsExtensions` methods extend `object` and return `double` as translation markers — a client-side call throws `InvalidOperationException(CoreStrings.FunctionOnClient(...))`. `VectorDbFunctionsTranslatorPlugin` matches the `MethodInfo` by reference and emits a `PgUnknownBinaryExpression` over both operands under the return type's default mapping; the server, never the translator, rejects an operator its operand type does not carry, so the first four apply to `vector`/`halfvec`/`sparsevec` and the last two to `bit`.

| [INDEX] | [SURFACE]                         | [SHAPE] | [CAPABILITY]                 |
| :-----: | :-------------------------------- | :------ | :--------------------------- |
|  [01]   | `L2Distance(object, object)`      | static  | `<->` euclidean distance     |
|  [02]   | `MaxInnerProduct(object, object)` | static  | `<#>` negative inner product |
|  [03]   | `CosineDistance(object, object)`  | static  | `<=>` cosine distance        |
|  [04]   | `L1Distance(object, object)`      | static  | `<+>` taxicab distance       |
|  [05]   | `HammingDistance(object, object)` | static  | `<~>` Hamming distance       |
|  [06]   | `JaccardDistance(object, object)` | static  | `<%>` Jaccard distance       |

## [04]-[SERVER_SURFACE]

[INDEX_OPCLASSES]: one operator class per store type and metric, spelled as the `HasOperators` literal.

| [INDEX] | [OPCLASS]              | [OPERATOR] | [HNSW] | [IVFFLAT] |
| :-----: | :--------------------- | :--------- | :----: | :-------: |
|  [01]   | `vector_l2_ops`        | `<->`      | `[O]`  |   `[O]`   |
|  [02]   | `vector_ip_ops`        | `<#>`      | `[O]`  |   `[O]`   |
|  [03]   | `vector_cosine_ops`    | `<=>`      | `[O]`  |   `[O]`   |
|  [04]   | `vector_l1_ops`        | `<+>`      | `[O]`  |   `[X]`   |
|  [05]   | `halfvec_l2_ops`       | `<->`      | `[O]`  |   `[O]`   |
|  [06]   | `halfvec_ip_ops`       | `<#>`      | `[O]`  |   `[O]`   |
|  [07]   | `halfvec_cosine_ops`   | `<=>`      | `[O]`  |   `[O]`   |
|  [08]   | `halfvec_l1_ops`       | `<+>`      | `[O]`  |   `[X]`   |
|  [09]   | `sparsevec_l2_ops`     | `<->`      | `[O]`  |   `[X]`   |
|  [10]   | `sparsevec_ip_ops`     | `<#>`      | `[O]`  |   `[X]`   |
|  [11]   | `sparsevec_cosine_ops` | `<=>`      | `[O]`  |   `[X]`   |
|  [12]   | `sparsevec_l1_ops`     | `<+>`      | `[O]`  |   `[X]`   |
|  [13]   | `bit_hamming_ops`      | `<~>`      | `[O]`  |   `[O]`   |
|  [14]   | `bit_jaccard_ops`      | `<%>`      | `[O]`  |   `[X]`   |

[INDEX_KNOBS]: build parameters ride `HasStorageParameter` on the index; query GUCs are `<am>.<name>`, `SET LOCAL` per session by the search-lane binder.

| [INDEX] | [KNOB]                     | [PHASE] | [DEFAULT] | [CAPABILITY]                       |
| :-----: | :------------------------- | :------ | :-------- | :--------------------------------- |
|  [01]   | `m`                        | build   | `16`      | hnsw connections per node          |
|  [02]   | `ef_construction`          | build   | `64`      | hnsw build candidate breadth       |
|  [03]   | `lists`                    | build   | `100`     | ivfflat centroid-list count        |
|  [04]   | `hnsw.ef_search`           | query   | `40`      | search candidate-list breadth      |
|  [05]   | `hnsw.iterative_scan`      | query   | `off`     | resumes a filtered scan            |
|  [06]   | `hnsw.max_scan_tuples`     | query   | `20000`   | iterative-scan tuple ceiling       |
|  [07]   | `hnsw.scan_mem_multiplier` | query   | `1`       | iterative-scan `work_mem` multiple |
|  [08]   | `ivfflat.probes`           | query   | `1`       | lists probed per search            |
|  [09]   | `ivfflat.max_probes`       | query   | `32768`   | iterative-scan probe ceiling       |
|  [10]   | `ivfflat.iterative_scan`   | query   | `off`     | resumes a filtered scan            |

[SERVER_FUNCTIONS]: pgvector SQL the search lane composes in projections and index expressions; `binary_quantize`, `subvector`, `l2_normalize`, and `vector_dims` carry `halfvec` overloads, and `l2_normalize` a `sparsevec` overload.

| [INDEX] | [FUNCTION]                    | [RESULT]  | [CAPABILITY]                                |
| :-----: | :---------------------------- | :-------- | :------------------------------------------ |
|  [01]   | `binary_quantize(vector)`     | `bit`     | quantizes a dense embedding for `<~>`/`<%>` |
|  [02]   | `subvector(vector, int, int)` | `vector`  | slices a contiguous dimension window        |
|  [03]   | `l2_normalize(vector)`        | `vector`  | unit-normalizes so `<#>` ranks as cosine    |
|  [04]   | `vector_norm(vector)`         | `float8`  | L2 magnitude                                |
|  [05]   | `vector_dims(vector)`         | `integer` | declared dimension count                    |
|  [06]   | `avg(vector)`                 | `vector`  | centroid aggregate over a group             |
|  [07]   | `sum(vector)`                 | `vector`  | element-wise aggregate over a group         |

Casts convert every pair among `vector`, `halfvec`, and `sparsevec` and admit `integer[]`, `real[]`, `double precision[]`, and `numeric[]`, so a precision tier-down is a column cast; `+`, `-`, `*`, and `||` compose element-wise arithmetic and concatenation on `vector` and `halfvec`.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Vector mapping is a policy of the `postgres-server` `StoreProfile` row, the one profile whose `Vector` fact holds.
- `UseVector` is the single admission root on both builders, so wire codecs, type mapping, and distance translation arrive together or not at all.
- `VectorTypeMapping` over `Vector`/`HalfVector`/`SparseVector` is the mapping root for every vector column.
- Distance ranking is a query fact inside profile queries: `VectorDbFunctionsExtensions` folds to `PgUnknownBinaryExpression` and the operator lands in the emitted SQL.
- An exact brute-force scan is the standing correctness baseline; HNSW, ivfflat, and diskann are approximate routes the `store.vector.route` fact discriminates.

[STACKING]:
- `api-npgsql.md`: `VectorExtensions.UseVector` registers `VectorTypeInfoResolverFactory` through `INpgsqlTypeMapper.AddTypeInfoResolverFactory`, whose `CreateResolver`/`CreateArrayResolver` pair round-trips scalar and `Vector[]` columns with no hand-written reader; the sealed `NpgsqlDataSourceBuilder` is the receiver the tuple-capture recovers.
- `api-npgsql-ef.md`: `NpgsqlIndexBuilderExtensions.HasMethod`/`HasOperators`/`HasStorageParameter` declare an HNSW or ivfflat index over the `[INDEX_OPCLASSES]` and `[INDEX_KNOBS]` literals, `HasPostgresExtension("vector")` declares the extension on the model, and `PgUnknownBinaryExpression` is the node the distance translator emits.
- `api-pgvectorscale.md`: `diskann` indexes the same `vector(N)` column under `vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops`, pulls `vector` as a `CASCADE` dependency, and reuses these distance functions unchanged at query time.
- `api-pg-search.md`: `FusionRank.Fuse` composes a `VectorMetric`-ordered branch with the `bm25` branch in one reciprocal-rank-fusion CTE; the `bm25` access method carries no EF translator, so its index DDL lands as raw migration SQL while its query leg reuses this catalog's operators.
- `Query/retrieval#SEARCH_PROVISIONING_PROBE`: `EmbeddingArity` is the single CLR-to-store axis — `Dense→Vector→vector(N)`, `Half→HalfVector→halfvec(N)`, `Sparse→SparseVector→sparsevec(N)`, `Bit→BitArray→bit(N)` — and `VectorMetric` carries the operator literal for the raw-CTE fusion leg beside `nameof(VectorDbFunctionsExtensions.L2Distance)` for the typed `Order` leg, built through `Expression.Call(typeof(VectorDbFunctionsExtensions), Fn, Type.EmptyTypes, columnExpr, Expression.Constant(new Vector(probe)))`; the probed embedding is generated upstream at `Compute/models#INFERENCE_MODES` `Embed`.

[LOCAL_ADMISSION]:
- Vector mapping enters through the `postgres-server` `StoreProfile` declaration via `UseVector`; a vector-branded service family is the rejected form.
- `vector` installs as a profile-declared model extension, and `vectorscale CASCADE` is the dependency-pulling form.
- Vector dimensions are column metadata — `HasColumnType("vector(N)")` or the `int? size` ctor argument — under one `StoreTypePostfix.Size` rule for all three store types.
- Index access-method, opclass, storage-parameter, and GUC literals belong to the `Store/provisioning#SERVER_EXTENSIONS` `Index` row and the search-lane binder; this catalog states the vocabulary they draw from.

[RAIL_LAW]:
- Package: `Pgvector.EntityFrameworkCore`, transitive `Pgvector`, server extension `vector`
- Owns: pgvector CLR value types, binary codecs, EF type mapping, distance-operator translation, and the server opclass, knob, function, and cast vocabulary for the Npgsql EF PostgreSQL provider
- Accept: profile-declared vector mapping through `UseVector`, server-translated distance projections, HNSW and ivfflat declarations through the Npgsql index builder, quantization, normalization, and cast composition in SQL
- Reject: a vector-branded service family, a client-side distance call, a hand-spelled `CREATE EXTENSION vector` beside the `HasPostgresExtension` annotation, a parallel column-type enum duplicating the `EmbeddingArity` axis, a hand-written `Vector[]` reader
