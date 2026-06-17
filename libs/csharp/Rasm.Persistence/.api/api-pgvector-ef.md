# [RASM_PERSISTENCE_API_PGVECTOR_EF]

`Pgvector.EntityFrameworkCore` plugs pgvector values into the EF Core
PostgreSQL provider, supplying vector type mapping, distance function
translation, data source configuration, and design-time services.
The CLR value types (`Vector`, `HalfVector`, `SparseVector`) live in the
transitive `Pgvector` package; the EF plugin depends on them.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Pgvector.EntityFrameworkCore`
- package: `Pgvector.EntityFrameworkCore` `0.3.0`
- assembly: `Pgvector.EntityFrameworkCore`
- namespace: `Pgvector.EntityFrameworkCore`
- value package: `Pgvector` `0.3.2` (transitive)
- value namespace: `Pgvector`, `Npgsql` (wire registration)
- provider package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[VALUE_TYPES]: CLR value types — `Pgvector` package, namespace `Pgvector`
- rail: store-provider

| [INDEX] | [SYMBOL]       | [STORE_TYPE] | [ELEMENT]              | [CAPABILITY]                |
| :-----: | :------------- | :----------- | :--------------------- | :-------------------------- |
|   [1]   | `Vector`       | `vector`     | `float` / `float[]`    | full-precision dense vector |
|   [2]   | `HalfVector`   | `halfvec`    | `Half` / `Half[]`      | half-precision dense vector |
|   [3]   | `SparseVector` | `sparsevec`  | `float` indices+values | sparse vector               |

[VECTOR_MEMBERS]: `Pgvector.Vector` — decompiled from `Pgvector.dll` 0.3.2

| [INDEX] | [MEMBER]                          | [RETURN]                | [CAPABILITY]                    |
| :-----: | :-------------------------------- | :---------------------- | :------------------------------ |
|   [1]   | `Vector(ReadOnlyMemory<float> v)` | ctor                    | wraps existing memory           |
|   [2]   | `Vector(string s)`                | ctor                    | parses `[f,f,f]` wire format    |
|   [3]   | `Memory`                          | `ReadOnlyMemory<float>` | backing memory (zero-copy read) |
|   [4]   | `ToArray()`                       | `float[]`               | materialises element array      |
|   [5]   | `Equals(Vector?)`, `==`, `!=`     | `bool`                  | element-wise equality           |

[HALFVECTOR_MEMBERS]: `Pgvector.HalfVector` — decompiled from `Pgvector.dll` 0.3.2

| [INDEX] | [MEMBER]                             | [RETURN]               | [CAPABILITY]                    |
| :-----: | :----------------------------------- | :--------------------- | :------------------------------ |
|   [1]   | `HalfVector(ReadOnlyMemory<Half> v)` | ctor                   | wraps existing memory           |
|   [2]   | `HalfVector(string s)`               | ctor                   | parses `[h,h,h]` wire format    |
|   [3]   | `Memory`                             | `ReadOnlyMemory<Half>` | backing memory (zero-copy read) |
|   [4]   | `ToArray()`                          | `Half[]`               | materialises element array      |
|   [5]   | `Equals(HalfVector?)`, `==`, `!=`    | `bool`                 | element-wise equality           |

[SPARSEVECTOR_MEMBERS]: `Pgvector.SparseVector` — decompiled from `Pgvector.dll` 0.3.2

| [INDEX] | [MEMBER]                            | [SHAPE]                     | [RETURN]                | [CAPABILITY]                      |
| :-----: | :---------------------------------- | :-------------------------- | :---------------------- | :-------------------------------- |
|   [1]   | `SparseVector`                      | dimensions, indices, values | ctor                    | from pre-sorted sparse components |
|   [2]   | `SparseVector`                      | dense memory                | ctor                    | dense-to-sparse conversion        |
|   [3]   | `SparseVector`                      | dictionary plus dimensions  | ctor                    | from index-to-value dictionary    |
|   [4]   | `SparseVector`                      | wire string                 | ctor                    | parses `{i:v,...}/d` wire format  |
|   [5]   | `Dimensions`                        | property                    | `int`                   | total dimension count             |
|   [6]   | `Indices`                           | property                    | `ReadOnlyMemory<int>`   | non-zero index positions          |
|   [7]   | `Values`                            | property                    | `ReadOnlyMemory<float>` | non-zero element values           |
|   [8]   | `ToArray()`                         | no args                     | `float[]`               | materialises dense float array    |
|   [9]   | `Equals(SparseVector?)`, `==`, `!=` | comparison                  | `bool`                  | element-wise equality             |

[PLUGIN_TYPES]: plugin admission and mapping — `Pgvector.EntityFrameworkCore` package
- rail: store-provider

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]                              |
| :-----: | :---------------------------------------- | :----------------- | :---------------------------------------- |
|   [1]   | `VectorDbContextOptionsBuilderExtensions` | builder extension  | admits plugin via `UseVector`             |
|   [2]   | `VectorDbContextOptionsExtension`         | options extension  | carries plugin policy                     |
|   [3]   | `VectorTypeMapping`                       | type mapping       | maps `Vector`/`HalfVector`/`SparseVector` |
|   [4]   | `VectorTypeMappingSourcePlugin`           | mapping plugin     | resolves `vector`/`halfvec`/`sparsevec`   |
|   [5]   | `VectorDataSourceConfigurationPlugin`     | data source plugin | enables vector wire type resolution       |
|   [6]   | `VectorDbFunctionsExtensions`             | function surface   | projects distance SQL operators           |
|   [7]   | `VectorDbFunctionsTranslatorPlugin`       | translator plugin  | translates distance calls to operators    |
|   [8]   | `VectorCodeGeneratorPlugin`               | scaffolding plugin | emits `UseVector` in scaffolded code      |
|   [9]   | `VectorDesignTimeServices`                | design services    | admits design tooling                     |

## [3]-[ENTRYPOINTS]

[WIRE_REGISTRATION]: `Npgsql.VectorExtensions` — decompiled from `Pgvector.dll` 0.3.2, namespace `Npgsql`

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]     | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------- | :--------------- | :-------------------------------------------------------- |
|   [1]   | `VectorExtensions.UseVector(INpgsqlTypeMapper)` | extension method | registers `VectorTypeInfoResolverFactory` for wire decode |

`NpgsqlDataSourceBuilder` implements `INpgsqlTypeMapper`; call `builder.UseVector()` on the data-source builder when using the driver without the EF plugin. The EF plugin wires this automatically through `VectorDataSourceConfigurationPlugin`.

[EF_PLUGIN_ADMISSION]: `VectorDbContextOptionsBuilderExtensions` — decompiled from `Pgvector.EntityFrameworkCore.dll` 0.3.0

| [INDEX] | [SURFACE]                                                      | [CALL_SHAPE]                              | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------- | :---------------------------------------- | :------------------------------ |
|   [1]   | `UseVector`                                                    | `NpgsqlDbContextOptionsBuilder` extension | admits EF plugin + wire mapping |
|   [2]   | `VectorTypeMapping.Default`                                    | `static VectorTypeMapping`                | maps `Vector` CLR to `vector`   |
|   [3]   | `VectorTypeMapping(string storeType, Type clrType, int? size)` | constructor                               | sizes vector columns            |

`VectorTypeMappingSourcePlugin` resolves `vector` → `Vector`, `halfvec` → `HalfVector`, and `sparsevec` → `SparseVector` bidirectionally by store-type name and CLR type.

[DISTANCE_FUNCTIONS]: `VectorDbFunctionsExtensions` — decompiled from `Pgvector.EntityFrameworkCore.dll` 0.3.0

| [INDEX] | [CLR_METHOD]      | [SQL_OPERATOR] | [APPLICABLE_TYPES]               | [CAPABILITY]           |
| :-----: | :---------------- | :------------- | :------------------------------- | :--------------------- |
|   [1]   | `L2Distance`      | `<->`          | `vector`, `halfvec`, `sparsevec` | Euclidean distance     |
|   [2]   | `MaxInnerProduct` | `<#>`          | `vector`, `halfvec`              | negative inner product |
|   [3]   | `CosineDistance`  | `<=>`          | `vector`, `halfvec`, `sparsevec` | cosine distance        |
|   [4]   | `L1Distance`      | `<+>`          | `vector`, `halfvec`, `sparsevec` | taxicab distance       |
|   [5]   | `HammingDistance` | `<~>`          | `bit`                            | Hamming distance       |
|   [6]   | `JaccardDistance` | `<%>`          | `bit`                            | Jaccard distance       |

All six methods are declared `this object a, object b` returning `double`; server-side translation by `VectorDbFunctionsTranslatorPlugin` emits the binary operator expression. Client-side invocation throws `InvalidOperationException`.

[INDEX_DECLARATION]: EF index builder — `NpgsqlIndexBuilderExtensions` in `Npgsql.EntityFrameworkCore.PostgreSQL`

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                        | [CAPABILITY]                            |
| :-----: | :-------------------- | :-------------------------------------------------- | :-------------------------------------- |
|   [1]   | `HasMethod`           | `IndexBuilder.HasMethod(string? method)`            | selects `hnsw` or `ivfflat`             |
|   [2]   | `HasOperators`        | `IndexBuilder.HasOperators(params string[]?)`       | sets ops class e.g. `vector_cosine_ops` |
|   [3]   | `HasStorageParameter` | `IndexBuilder.HasStorageParameter(string, object?)` | sets `m`, `ef_construction`, `lists`    |

## [4]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: the pgvector plugin is vector mapping policy for the PostgreSQL store profile
- EF admission root: `UseVector` on the `NpgsqlDbContextOptionsBuilder`
- driver admission root: `VectorExtensions.UseVector(NpgsqlDataSourceBuilder)` when used without EF
- mapping root: `VectorTypeMapping` covering `Vector`, `HalfVector`, and `SparseVector`
- query root: distance function translation inside profile queries

[LOCAL_ADMISSION]:
- Vector mapping enters through the PostgreSQL store-profile declaration via `UseVector`.
- The `vector` extension is a profile-declared PostgreSQL extension via `HasPostgresExtension("vector")`.
- Distance projections are query facts and stay inside profile queries.
- Vector dimensions are column metadata declared by the profile model via `HasColumnType("vector(N)")`.
- `HalfVector` maps to `halfvec(N)` and `SparseVector` to `sparsevec(N)` column types.

[RAIL_LAW]:
- Package: `Pgvector.EntityFrameworkCore`
- Owns: pgvector mapping for the PostgreSQL EF provider
- Accept: profile-declared vector mapping and distance queries
- Reject: vector-branded service families; calling distance methods client-side
