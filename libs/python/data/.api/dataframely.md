# [PY_DATA_API_DATAFRAMELY]

`dataframely` declares and enforces Polars dataframe contracts through class-style `Schema` definitions whose columns are typed `Column` instances carrying inline rules (`nullable`, `primary_key`, `unique`, `min`/`max`, `regex`, `is_in`), and cross-column `@rule` predicates. `Schema.validate`/`is_valid`/`filter`/`cast` run rule evaluation against a `pl.DataFrame`/`pl.LazyFrame`, returning a typed `DataFrame[Self]`/`LazyFrame[Self]` or a `FilterResult` whose `FailureInfo` carries the invalid rows, per-rule `counts`, and `cooccurrence_counts`. `Collection` groups named member schemas under shared-primary-key invariants and `@filter` methods, with `require_relationship_one_to_one`/`require_relationship_one_to_at_least_one` expressing referential integrity across members; the package owner composes `Schema`, `Collection`, `rule`, and `filter` into the CONTRACT_GATE_FOLD/COVENANT path, consuming the in-package Polars-native rule algebra rather than re-implementing row-level assertion, and never re-deriving a parallel validation engine over the frame.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dataframely`
- package: `dataframely`
- import: `dataframely`
- owner: `data`
- rail: contract
- installed: `2.10.1`
- license: BSD-3-Clause
- capability: declarative `Schema`/`Column` dataframe contracts with inline column rules and cross-column `@rule` predicates, `Collection` multi-member integrity with shared primary keys and `@filter` methods, eager/lazy `validate`/`is_valid`/`filter`/`cast`, `FailureInfo` failure introspection (invalid rows, per-rule and co-occurrence counts), schema/collection `serialize`/`deserialize`, parquet/delta read/scan/write/sink with embedded-schema validation, `read_parquet_metadata_*` schema-only metadata reads, `dy.random` deterministic conformant `sample` generation, and `to_polars_schema`/`to_sqlalchemy_columns`/`to_pyarrow_schema`/`to_pydantic_model` projection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schema, collection, column, and failure roots
- rail: contract

A `Schema` subclass declares columns by assignment; a `Collection` subclass declares typed members by annotation. `Column` is the abstract base for the typed column family; `rule` and `filter` mark cross-column and cross-member predicates. `FailureInfo` carries the invalid rows and rule diagnostics from a `filter` pass; `DeserializationError` is raised when serialized schema/collection data cannot be restored.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [RAIL]                                                                                      |
| :-----: | :--------------------- | :---------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Schema`               | schema            | declarative single-frame column contract with rules                                         |
|  [02]   | `Collection`           | collection        | multi-member contract with shared-key integrity and filters                                 |
|  [03]   | `CollectionMember`     | member annotation | per-member behavior (`ignored_in_filters`, `propagate_row_failures`, `inline_for_sampling`) |
|  [04]   | `Column`               | column base       | abstract base for the typed column family                                                   |
|  [05]   | `rule`                 | rule decorator    | cross-column / grouped validation predicate marker                                          |
|  [06]   | `filter`               | filter decorator  | collection-level cross-member filter marker                                                 |
|  [07]   | `FailureInfo`          | failure receipt   | invalid rows, per-rule `counts`, `cooccurrence_counts`                                      |
|  [08]   | `Config`               | config context    | `max_sampling_iterations` / `max_failure_examples` overrides                                |
|  [09]   | `DataFrame`            | typed frame alias | `DataFrame[S]` — eager frame tagged with schema `S`                                         |
|  [10]   | `LazyFrame`            | typed frame alias | `LazyFrame[S]` — lazy frame tagged with schema `S`                                          |
|  [11]   | `Validation`           | literal alias     | `"allow"`/`"warn"`/`"skip"` parquet-read validation policy                                  |
|  [12]   | `DeserializationError` | error             | serialized schema/collection restore failure                                                |

[PUBLIC_TYPE_SCOPE]: typed column catalogue
- rail: contract

Every column is a `Column` subclass mapping to a Polars dtype; constructor rows carry the inline validation policy. `Integer`/`Int8`..`UInt64` add `min`/`min_exclusive`/`max`/`max_exclusive`/`is_in`; `Float`/`Float32`/`Float64` add the float bounds; `String` adds `min_length`/`max_length`/`regex`; `Decimal` adds `precision`/`scale` plus bounds; `Enum`/`Categorical` carry their category set; `Datetime`/`Date`/`Time`/`Duration` carry temporal bounds; `List`/`Array`/`Struct` carry nested inner column specifications.

| [INDEX] | [SYMBOL]                                     | [POLARS_DTYPE]               | [RAIL]                                          |
| :-----: | :------------------------------------------- | :--------------------------- | :---------------------------------------------- |
|  [01]   | `Integer`, `Int8`, `Int16`, `Int32`, `Int64` | integer dtypes               | numeric column with bounds / `is_in`            |
|  [02]   | `UInt8`, `UInt16`, `UInt32`, `UInt64`        | integer dtypes               | unsigned integer column with bounds             |
|  [03]   | `Float`, `Float32`, `Float64`                | float dtypes                 | floating column with bounds                     |
|  [04]   | `Decimal`                                    | `pl.Decimal`                 | fixed-precision column (`precision`/`scale`)    |
|  [05]   | `String`                                     | `pl.String`                  | text column (`min_length`/`max_length`/`regex`) |
|  [06]   | `Bool`                                       | `pl.Boolean`                 | boolean column                                  |
|  [07]   | `Enum`, `Categorical`                        | `pl.Enum` / `pl.Categorical` | bounded category column                         |
|  [08]   | `Date`, `Time`, `Datetime`, `Duration`       | temporal dtypes              | temporal column with bounds                     |
|  [09]   | `List`, `Array`, `Struct`                    | nested dtypes                | nested column over an inner column spec         |
|  [10]   | `Binary`                                     | `pl.Binary`                  | raw byte column                                 |
|  [11]   | `Object`, `Any`                              | `pl.Object` / dtype-agnostic | escape-hatch column                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Schema` validate, filter, and project
- rail: contract

`validate`/`is_valid`/`filter`/`cast` accept a positional `pl.DataFrame | pl.LazyFrame`; `cast=True` coerces wrong dtypes before rule evaluation, `eager` selects `DataFrame[Self]` versus `LazyFrame[Self]`. `filter` returns `(valid, failures)` with `failures: FailureInfo`. `sample` generates schema-conformant rows; `serialize`/`deserialize_schema` round-trip the contract; the `to_*` projections export the contract to Polars/SQLAlchemy/PyArrow/Pydantic.

| [INDEX] | [SURFACE]                      | [CAPABILITY]                                          |
| :-----: | :----------------------------- | :---------------------------------------------------- |
|  [01]   | `Schema.validate`              | enforce schema; raise on violation                    |
|  [02]   | `Schema.is_valid`              | boolean conformance (collects eagerly, never raises)  |
|  [03]   | `Schema.filter`                | split into `(valid, failures)`                        |
|  [04]   | `Schema.cast`                  | drop extra columns + coerce dtypes (no content check) |
|  [05]   | `Schema.create_empty`          | empty schema-typed frame                              |
|  [06]   | `Schema.create_empty_if_none`  | impute `None` with empty schema frame                 |
|  [07]   | `Schema.sample`                | random schema-conformant rows (testing)               |
|  [08]   | `Schema.matches`               | structural schema equality                            |
|  [09]   | `Schema.serialize`             | serialize the contract to a string                    |
|  [10]   | `Schema.read_parquet`          | read parquet, validate embedded schema                |
|  [11]   | `Schema.scan_parquet`          | lazily scan parquet with schema check                 |
|  [12]   | `Schema.write_parquet`         | write parquet with embedded serialized schema         |
|  [13]   | `Schema.sink_parquet`          | streaming-sink lazy frame to parquet                  |
|  [14]   | `Schema.read_delta`            | read delta table with schema check                    |
|  [15]   | `Schema.scan_delta`            | lazily scan delta with schema check                   |
|  [16]   | `Schema.write_delta`           | write delta with embedded serialized schema           |
|  [17]   | `Schema.to_polars_schema`      | project to a Polars schema                            |
|  [18]   | `Schema.to_sqlalchemy_columns` | project to SQLAlchemy columns                         |
|  [19]   | `Schema.to_pyarrow_schema`     | project to a PyArrow schema                           |
|  [20]   | `Schema.to_pydantic_model`     | project to a Pydantic model                           |
|  [21]   | `Schema.columns`               | the declared column map                               |
|  [22]   | `Schema.primary_key`           | primary-key column names                              |

- [01]-[VALIDATE]: `validate(df, /, *, cast=False, eager=True) -> DataFrame[Self] \| LazyFrame[Self]`
- [02]-[IS_VALID]: `is_valid(df, /, *, cast=False) -> bool`
- [03]-[FILTER]: `filter(df, /, *, cast=False, eager=True) -> FilterResult[Self] \| LazyFilterResult[Self]`
- [04]-[CAST]: `cast(df, /) -> DataFrame[Self] \| LazyFrame[Self]`
- [05]-[CREATE_EMPTY]: `create_empty(*, lazy=False) -> DataFrame[Self] \| LazyFrame[Self]`
- [06]-[CREATE_EMPTY_IF_NONE]: `create_empty_if_none(df, *, lazy=False) -> DataFrame[Self] \| LazyFrame[Self]`
- [07]-[SAMPLE]: `sample(num_rows=None, *, overrides=None, generator=None) -> DataFrame[Self]`
- [08]-[MATCHES]: `matches(other) -> bool`
- [09]-[SERIALIZE]: `serialize() -> str`
- [10]-[READ_PARQUET]: `read_parquet(source, *, validation="warn", **kwargs) -> DataFrame[Self]`
- [11]-[SCAN_PARQUET]: `scan_parquet(source, *, validation="warn", **kwargs) -> LazyFrame[Self]`
- [12]-[WRITE_PARQUET]: `write_parquet(df, ...) -> None`
- [13]-[SINK_PARQUET]: `sink_parquet(lf, ...) -> None`
- [14]-[READ_DELTA]: `read_delta(...) -> DataFrame[Self]`
- [15]-[SCAN_DELTA]: `scan_delta(...) -> LazyFrame[Self]`
- [16]-[WRITE_DELTA]: `write_delta(df, ...) -> None`
- [17]-[TO_POLARS_SCHEMA]: `to_polars_schema() -> pl.Schema`
- [18]-[TO_SQLALCHEMY_COLUMNS]: `to_sqlalchemy_columns(dialect) -> list[sa.Column]`
- [19]-[TO_PYARROW_SCHEMA]: `to_pyarrow_schema() -> pa.Schema`
- [20]-[TO_PYDANTIC_MODEL]: `to_pydantic_model(name=None) -> type[pydantic.BaseModel]`
- [21]-[COLUMNS]: `columns() -> dict[str, Column]`
- [22]-[PRIMARY_KEY]: `primary_key() -> list[str]`

[ENTRYPOINT_SCOPE]: `Collection` cross-frame integrity
- rail: contract

`Collection` methods accept `data: Mapping[str, FrameType]` keyed by member name. `validate`/`filter`/`cast` enforce each member schema plus the collection `@filter` invariants; `filter` returns a `CollectionFilterResult` carrying per-member `FailureInfo`. `join` filters every member by a shared-primary-key frame; `collect_all` collects all lazy members. The relationship functions build the `pl.LazyFrame` a `@filter` returns to express referential integrity.

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `Collection.validate`                                  | enforce all member schemas + collection filters      |
|  [02]   | `Collection.is_valid`                                  | boolean collection conformance                       |
|  [03]   | `Collection.filter`                                    | split members into valid + per-member failures       |
|  [04]   | `CollectionFilterResult.failure`                       | per-member `FailureInfo` map, keyed by member name   |
|  [05]   | `Collection.cast`                                      | cast every member to its schema (no invariant check) |
|  [06]   | `Collection.join`                                      | filter all members by a shared-key frame             |
|  [07]   | `Collection.collect_all`                               | collect all lazy members                             |
|  [08]   | `Collection.sample`                                    | random collection-conformant members (testing)       |
|  [09]   | `Collection.create_empty`                              | empty collection-typed members                       |
|  [10]   | `Collection.matches`                                   | structural collection equality                       |
|  [11]   | `Collection.serialize`                                 | serialize the collection contract                    |
|  [12]   | `Collection.read_parquet`                              | read `<member>.parquet` files with schema check      |
|  [13]   | `Collection.scan_parquet`                              | lazily scan member parquet files                     |
|  [14]   | `Collection.write_parquet`                             | write each member to `<member>.parquet`              |
|  [15]   | `Collection.sink_parquet`                              | streaming-sink each lazy member to parquet           |
|  [16]   | `Collection.read_delta` / `scan_delta` / `write_delta` | per-member delta IO with embedded schema check       |
|  [17]   | `Collection.member_schemas`                            | the member-name-to-schema map                        |
|  [18]   | `Collection.common_primary_key`                        | primary key shared across members                    |
|  [19]   | `require_relationship_one_to_one`                      | 1:1 referential-integrity filter expression          |
|  [20]   | `require_relationship_one_to_at_least_one`             | 1:{1,N} referential-integrity filter expression      |
|  [21]   | `concat_collection_members`                            | concatenate same-typed collections member-wise       |

- [01]-[VALIDATE]: `validate(data, /, *, cast=False, eager=True) -> Self`
- [02]-[IS_VALID]: `is_valid(data, /, *, cast=False) -> bool`
- [03]-[FILTER]: `filter(data, /, *, cast=False, eager=True) -> CollectionFilterResult[Self]`
- [04]-[FAILURE]: `CollectionFilterResult` is a 2-field `NamedTuple` (`result: C` valid collection, `failure: dict[str, FailureInfo]`) with a `collect_all()` method; `count`/`index` are inherited `tuple` methods, not result members.
- [05]-[CAST]: `cast(data, /) -> Self`
- [06]-[JOIN]: `join(primary_keys, how: Literal["semi","anti"]="semi", maintain_order: Literal["none","left"]="none") -> Self`
- [07]-[COLLECT_ALL]: `collect_all() -> Self`
- [08]-[SAMPLE]: `sample(num_rows=None, *, overrides=None, generator=None) -> Self`
- [09]-[CREATE_EMPTY]: `create_empty() -> Self`
- [10]-[MATCHES]: `matches(other) -> bool`
- [11]-[SERIALIZE]: `serialize() -> str`
- [12]-[READ_PARQUET]: `read_parquet(directory, *, validation="warn", **kwargs) -> Self`
- [13]-[SCAN_PARQUET]: `scan_parquet(directory, *, validation="warn", **kwargs) -> Self`
- [14]-[WRITE_PARQUET]: `write_parquet(directory, **kwargs) -> None`
- [15]-[SINK_PARQUET]: `sink_parquet(directory, **kwargs) -> None`
- [16]-[READ_DELTA]: `read_delta(...) -> Self` / `scan_delta(...) -> Self` / `write_delta(...) -> None`
- [17]-[MEMBER_SCHEMAS]: `member_schemas() -> dict[str, type[Schema]]`
- [18]-[COMMON_PRIMARY_KEY]: `common_primary_key() -> list[str]`
- [19]-[REQUIRE_RELATIONSHIP_ONE_TO_ONE]: `require_relationship_one_to_one(lhs, rhs, /, on, *, drop_duplicates=True) -> pl.LazyFrame`
- [20]-[REQUIRE_RELATIONSHIP_ONE_TO_AT_LEAST_ONE]: `require_relationship_one_to_at_least_one(lhs, rhs, /, on, *, drop_duplicates=True) -> pl.LazyFrame`
- [21]-[CONCAT_COLLECTION_MEMBERS]: `concat_collection_members(collections, /) -> dict[str, pl.LazyFrame]`

[ENTRYPOINT_SCOPE]: rule declaration, failure introspection, and config
- rail: contract

`rule` decorates a `Schema` method returning a boolean `pl.Expr`; `group_by` resolves grouped rules (`"primary_key"` binds to the schema key at class creation). `filter` decorates a `Collection` method returning the keep-set `pl.LazyFrame`. `FailureInfo` exposes the invalid rows and rule diagnostics; `Config` overrides sampling and failure-example caps as a context manager.

| [INDEX] | [SURFACE]                          | [CAPABILITY]                                                             |
| :-----: | :--------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `rule`                             | mark a cross-column / grouped validation predicate                       |
|  [02]   | `filter`                           | mark a collection cross-member filter                                    |
|  [03]   | `Column.init`                      | base inline column policy                                                |
|  [04]   | `Integer.init`                     | numeric column with bounds                                               |
|  [05]   | `String.init`                      | text column policy                                                       |
|  [06]   | `Decimal.init`                     | fixed-precision column policy                                            |
|  [07]   | `FailureInfo.invalid`              | the invalid rows of the input frame (a method, not a property)           |
|  [08]   | `FailureInfo.details`              | invalid rows + a per-rule status `Enum` column (method)                  |
|  [09]   | `FailureInfo.counts`               | rule-name to failure-count map                                           |
|  [10]   | `FailureInfo.cooccurrence_counts`  | co-failing rule-set counts                                               |
|  [11]   | `Config`                           | sampling + failure-example caps (context manager)                        |
|  [12]   | `deserialize_schema`               | restore a serialized schema                                              |
|  [13]   | `deserialize_collection`           | restore a serialized collection                                          |
|  [14]   | `read_parquet_metadata_schema`     | restore the embedded `Schema` from parquet metadata without reading rows |
|  [15]   | `read_parquet_metadata_collection` | restore the embedded `Collection` from member parquet metadata           |
|  [16]   | `dy.random`                        | deterministic RNG source for conformant-row generation                   |

- [01]-[RULE]: `rule(*, group_by=None) -> Callable[[ValidationFunction], RuleFactory]`
- [02]-[FILTER]: `filter() -> Callable[[Callable[[C], pl.LazyFrame]], Filter[C]]`
- [03]-[INIT]: `Column(*, nullable=False, primary_key=False, unique=False, check=None, alias=None, metadata=None, description=None)`
- [04]-[INIT]: `Integer(nullable=False, primary_key=False, unique=False, min=None, min_exclusive=None, max=None, max_exclusive=None, is_in=None, ...)`
- [05]-[INIT]: `String(nullable=False, primary_key=False, unique=False, min_length=None, max_length=None, regex=None, ...)`
- [06]-[INIT]: `Decimal(precision=None, scale=0, nullable=False, primary_key=False, unique=False, min=None, max=None, ...)`
- [07]-[INVALID]: `invalid() -> pl.DataFrame`
- [08]-[DETAILS]: `details() -> pl.DataFrame` — invalid rows plus one `Enum["valid","invalid","unknown"]` column per rule name.
- [09]-[COUNTS]: `counts() -> dict[str, int]`
- [10]-[COOCCURRENCE_COUNTS]: `cooccurrence_counts() -> dict[frozenset[str], int]`
- [11]-[CONFIG]: `Config(**options)` / `Config.set_max_sampling_iterations(n)` / `Config.set_max_failure_examples(n)` / `Config.restore_defaults()`
- [12]-[DESERIALIZE_SCHEMA]: `deserialize_schema(data, strict=True) -> type[Schema] \| None`
- [13]-[DESERIALIZE_COLLECTION]: `deserialize_collection(data, ...) -> type[Collection]`
- [14]-[READ_PARQUET_METADATA_SCHEMA]: `read_parquet_metadata_schema(source, **kwargs) -> type[Schema]`
- [15]-[READ_PARQUET_METADATA_COLLECTION]: `read_parquet_metadata_collection(directory, **kwargs) -> type[Collection]`
- [16]-[RANDOM]: module: `random.generator(seed=None)` / sampler entries consumed by `Schema.sample`/`Collection.sample`

## [04]-[IMPLEMENTATION_LAW]

[CONTRACT_DATAFRAMELY]:
- import: `import dataframely as dy` at boundary scope only; module-level import is banned by the manifest import policy.
- schema axis: one `Schema` subclass owns a single-frame contract; columns are typed `Column` assignments carrying inline rules (`nullable`/`primary_key`/`unique`/`min`/`max`/`regex`/`is_in`), never a parallel validator object; cross-column and grouped predicates are `@rule` methods returning a boolean `pl.Expr`, never imperative row loops.
- validation axis: `validate`/`is_valid`/`filter`/`cast` is the single discriminated entry family keyed by call row — `validate` raises on violation, `is_valid` returns a boolean, `filter` splits into `(valid, FailureInfo)`, `cast` coerces dtypes; `cast`/`eager` are call rows, never separate method names; CONTRACT_GATE_FOLD folds rule failures over the frame instead of re-checking per row.
- collection axis: one `Collection` subclass owns multi-member integrity; members are annotated `dy.LazyFrame[MemberSchema]` fields with a shared primary key; `@filter` methods express cross-member invariants, and `require_relationship_one_to_one`/`require_relationship_one_to_at_least_one` build the referential-integrity `pl.LazyFrame` the filter returns — COVENANT integrity is a filter row, never a hand-stitched anti/semi-join.
- failure axis: `FailureInfo` is the single failure receipt carrying `invalid` rows, `details`, per-rule `counts`, and `cooccurrence_counts`; failure diagnostics are receipt fields, never re-derived from the raw frame.
- serialization axis: `serialize`/`deserialize_schema`/`deserialize_collection` round-trip the contract as a string, and parquet/delta IO embeds the serialized schema so `read_parquet`/`scan_parquet` re-validate by `Validation` policy (`"allow"`/`"warn"`/`"skip"`); contract storage is the schema's own embedded metadata, never a side file.
- projection axis: `to_polars_schema`/`to_sqlalchemy_columns`/`to_pyarrow_schema`/`to_pydantic_model` export the one contract to the consuming runtime; downstream code projects the schema, never re-declares the column types.
- evidence: each gate captures schema name, primary key, rule names, valid/invalid row counts, per-rule and co-occurrence failure counts, and serializer kind as a contract receipt.
- boundary: dataframely owns Polars-native dataframe contract declaration, rule evaluation, and cross-frame integrity over its Rust `_native` core; row-level grading and reporting route to the grading owner; raw columnar transforms stay in the Polars owner; persistence of validated frames routes through the storage owner.

[STACKS_WITH]:
- polars: dataframely IS the contract layer over polars — `Schema.validate`/`filter` consume a `pl.DataFrame`/`pl.LazyFrame` and `@rule`/`@filter` predicates return `pl.Expr`/`pl.LazyFrame`; the typed `DataFrame[S]`/`LazyFrame[S]` outputs are polars frames carrying a schema tag, so all transform logic stays in the polars owner and only the contract gate lives here.
- pyarrow / arro3-core: `to_pyarrow_schema` projects the contract to a `pa.Schema` so a validated frame's schema is the wire schema for the columnar interop owner; an Arrow ingest (e.g. from `connectorx`/`daft.to_arrow`) is validated by reading into polars then through `Schema.validate`.
- connectorx / daft: a partitioned database/lakehouse read egresses a `pl.DataFrame` that enters `Schema.validate` at the ingest boundary — the schema is the single contract for both the read source and the downstream consumer, never re-declared.
- deltalake: `Schema.read_delta`/`scan_delta`/`write_delta` embed the serialized contract in the delta table so the contract round-trips with the data, never a side file; the deltalake owner owns the table transaction, dataframely owns the embedded-schema validation policy (`Validation`).
- pandera / pointblank: pandera and pointblank are the cross-engine/grading validation siblings; route Polars-native declarative contracts and `Collection` cross-frame integrity to dataframely, pandas/multi-backend schema checks to pandera, and column-health reporting to pointblank — one validation concern, partitioned by engine ownership, never a parallel validator per column kind.
- pydantic: `to_pydantic_model` projects the contract to a `BaseModel` so a row-shaped API/config boundary reuses the one schema definition instead of re-declaring the field types.

[RAIL_LAW]:
- Package: `dataframely`
- Owns: declarative Polars `Schema`/`Column` contracts with inline and cross-column rules, `Collection` cross-frame integrity with shared primary keys and filters, eager/lazy validate/filter/cast, `FailureInfo` introspection, and schema/collection serialization with embedded-schema parquet/delta IO
- Accept: dataframe contract declaration and enforcement feeding the CONTRACT_GATE_FOLD/COVENANT gate, with typed `DataFrame[S]`/`LazyFrame[S]` outputs flowing to the data and persistence owners
- Reject: wrapper-renames of `validate`/`filter`; a hand-rolled per-row assertion loop where `@rule`/`@filter` predicates own the algebra; a parallel validator type per column kind; a hand-stitched anti/semi-join where `require_relationship_*` owns referential integrity; a side-file schema store where parquet/delta embed the serialized contract; re-declaring column types the schema already projects
