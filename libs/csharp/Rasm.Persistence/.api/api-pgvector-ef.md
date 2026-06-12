# [RASM_PERSISTENCE_API_PGVECTOR_EF]

`Pgvector.EntityFrameworkCore` plugs pgvector values into the EF Core
PostgreSQL provider, supplying vector type mapping, distance function
translation, data source configuration, and design-time services.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Pgvector.EntityFrameworkCore`
- package: `Pgvector.EntityFrameworkCore`
- assembly: `Pgvector.EntityFrameworkCore`
- namespace: `Pgvector.EntityFrameworkCore`
- value package: `Pgvector`
- provider package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[PLUGIN_TYPES]: plugin admission and mapping
- rail: store-provider

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]              |
| :-----: | :---------------------------------------- | :----------------- | :------------------------ |
|   [1]   | `VectorDbContextOptionsBuilderExtensions` | builder extension  | admits plugin             |
|   [2]   | `VectorDbContextOptionsExtension`         | options extension  | carries plugin policy     |
|   [3]   | `VectorTypeMapping`                       | type mapping       | maps `Vector` to vector   |
|   [4]   | `VectorTypeMappingSourcePlugin`           | mapping plugin     | resolves vector mappings  |
|   [5]   | `VectorDataSourceConfigurationPlugin`     | data source plugin | enables vector wire       |
|   [6]   | `VectorDbFunctionsExtensions`             | function surface   | projects distance SQL     |
|   [7]   | `VectorDbFunctionsTranslatorPlugin`       | translator plugin  | translates distance calls |
|   [8]   | `VectorCodeGeneratorPlugin`               | scaffolding plugin | emits plugin admission    |
|   [9]   | `VectorDesignTimeServices`                | design services    | admits design tooling     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin admission and mapping
- rail: store-provider

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                         | [CAPABILITY]           |
| :-----: | :-------------------------- | :----------------------------------- | :--------------------- |
|   [1]   | `UseVector`                 | provider option                      | maps vector values     |
|   [2]   | `VectorTypeMapping.Default` | static mapping                       | maps `Vector` CLR type |
|   [3]   | `VectorTypeMapping(..)`     | constructor with store type and size | sizes vector columns   |

[ENTRYPOINT_SCOPE]: distance function projections
- rail: store-provider

| [INDEX] | [SURFACE]         | [CALL_SHAPE]    | [CAPABILITY]            |
| :-----: | :---------------- | :-------------- | :---------------------- |
|   [1]   | `L2Distance`      | value extension | projects `<->` distance |
|   [2]   | `MaxInnerProduct` | value extension | projects `<#>` distance |
|   [3]   | `CosineDistance`  | value extension | projects `<=>` distance |
|   [4]   | `L1Distance`      | value extension | projects `<+>` distance |
|   [5]   | `HammingDistance` | value extension | projects bit distance   |
|   [6]   | `JaccardDistance` | value extension | projects bit distance   |

## [4]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: the pgvector plugin is vector mapping policy for the PostgreSQL store profile
- admission root: `UseVector` on the PostgreSQL provider options
- mapping root: `VectorTypeMapping` over the `Vector` value type
- query root: distance function translation inside profile queries

[LOCAL_ADMISSION]:
- Vector mapping enters only through the PostgreSQL store-profile declaration.
- The `vector` extension is a profile-declared PostgreSQL extension.
- Distance projections are query facts and stay inside profile queries.
- Vector dimensions are column metadata declared by the profile model.

[RAIL_LAW]:
- Package: `Pgvector.EntityFrameworkCore`
- Owns: pgvector mapping for the PostgreSQL EF provider
- Accept: profile-declared vector mapping and distance queries
- Reject: vector-branded service families
