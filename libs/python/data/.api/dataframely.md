# [PY_DATA_API_DATAFRAMELY]

`dataframely` owns Polars-native dataframe contracts: a `Schema` subclass declares `Column` members with inline rules and cross-column `@rule` predicates, and a `Collection` binds member schemas under shared-primary-key integrity with `@filter` and `require_relationship_*` invariants. Validation runs eager or lazy through one `validate`/`is_valid`/`filter`/`cast` family, returning a `DataFrame[S]`/`LazyFrame[S]` or a `FailureInfo` receipt. `dataframely` feeds the data folder's CONTRACT_GATE_FOLD/COVENANT path as the row-level rule engine.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dataframely`
- package: `dataframely` (BSD-3-Clause)
- module: `dataframely`
- rail: Polars dataframe contract gate

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schema, collection, column, and failure roots

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [CAPABILITY]                                                 |
| :-----: | :------------------------ | :---------------- | :----------------------------------------------------------- |
|  [01]   | `Schema`                  | schema            | declarative single-frame column contract with rules          |
|  [02]   | `Collection`              | collection        | multi-member contract with shared-key integrity and filters  |
|  [03]   | `CollectionMember`        | member annotation | per-member behavior over `ignored_in_filters` and siblings   |
|  [04]   | `Column`                  | column base       | abstract base for the typed column family                    |
|  [05]   | `rule`                    | rule decorator    | cross-column / grouped validation predicate marker           |
|  [06]   | `filter`                  | filter decorator  | collection-level cross-member filter marker                  |
|  [07]   | `FailureInfo`             | failure receipt   | invalid rows, per-rule `counts`, `cooccurrence_counts`       |
|  [08]   | `Config`                  | config context    | `max_sampling_iterations` / `max_failure_examples` overrides |
|  [09]   | `DataFrame` / `LazyFrame` | frame alias       | `[S]`-tagged eager and lazy frames                           |
|  [10]   | `Validation`              | literal alias     | `"allow"`/`"warn"`/`"skip"` parquet-read validation policy   |
|  [11]   | `DeserializationError`    | error             | serialized schema/collection restore failure                 |

[PUBLIC_TYPE_SCOPE]: typed column catalogue

Every subtype maps to a Polars dtype and carries its inline validation policy through the constructor.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]                | [CAPABILITY]                                    |
| :-----: | :------------------------------------------- | :--------------------------- | :---------------------------------------------- |
|  [01]   | `Integer`, `Int8`, `Int16`, `Int32`, `Int64` | integer dtypes               | signed integer column with bounds / `is_in`     |
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

Every surface is a `classmethod` on the `Schema` subclass; `cast=True` coerces dtypes before rule evaluation and `eager` selects the eager or lazy return.

| [INDEX] | [SURFACE]                                                 | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `validate(df, /, *, cast, eager, **kwargs)`               | enforce schema, raise on violation                     |
|  [02]   | `is_valid(df, /, *, cast, **kwargs) -> bool`              | boolean conformance, collected eagerly, never raises   |
|  [03]   | `filter(df, /, *, cast, eager, **kwargs) -> FilterResult` | split into `(valid, failures)`; lazy `LazyFilterResult` |
|  [04]   | `cast(df, /)`                                             | drop extra columns and coerce dtypes, no content check |
|  [05]   | `create_empty(*, lazy)`                                   | empty schema-typed frame                               |
|  [06]   | `create_empty_if_none(df, *, lazy)`                       | impute `None` with an empty schema frame               |
|  [07]   | `sample(num_rows, *, overrides, generator)`              | random schema-conformant rows                          |
|  [08]   | `matches(other) -> bool`                                  | structural schema equality                             |
|  [09]   | `serialize() -> str`                                      | serialize the contract to a string                     |
|  [10]   | `columns() -> dict[str, Column]`                          | the declared column map                                |
|  [11]   | `primary_key() -> list[str]`                              | primary-key column names                               |
|  [12]   | `to_polars_schema() -> pl.Schema`                         | project to a Polars schema                             |
|  [13]   | `to_pyarrow_schema() -> pa.Schema`                        | project to a PyArrow schema                            |
|  [14]   | `to_sqlalchemy_columns(dialect)`                          | project to `list[sa.Column]`                           |
|  [15]   | `to_pydantic_model(name)`                                 | project to a `pydantic.BaseModel` subclass             |

- `Schema.validate`/`filter`/`is_valid`: `**kwargs` pass to `polars.LazyFrame.collect` — set `engine="streaming"` for out-of-core validation over a lazy frame.
- Parquet/delta IO stays in the polars owner: read with `polars.read_parquet`/`scan_parquet` then call `Schema.validate` explicitly, write with `polars.DataFrame.write_parquet`/`LazyFrame.sink_parquet`; `read_parquet_metadata_schema(source)` recovers an embedded serialized `Schema` from parquet metadata, and `serialize`/`deserialize_schema` own the contract string.

[ENTRYPOINT_SCOPE]: `Collection` cross-frame integrity

Every surface is a `classmethod` accepting `data: Mapping[str, FrameType]` keyed by member name; member schemas and the collection `@filter` invariants enforce together.

| [INDEX] | [SURFACE]                                                                     | [CAPABILITY]                                           |
| :-----: | :---------------------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `validate(data, /, *, cast, eager, skip_member_validation, **kwargs) -> Self` | enforce member schemas and collection filters          |
|  [02]   | `is_valid(data, /, *, cast, **kwargs) -> bool`                                | boolean collection conformance                         |
|  [03]   | `filter(data, /, *, cast, eager, skip_member_validation, **kwargs)` | split into valid and per-member failures |
|  [04]   | `cast(data, /) -> Self`                                                       | cast every member to its schema, no invariant check    |
|  [05]   | `join(primary_keys, how, maintain_order) -> Self`                             | filter members by a shared-key frame (`how` semi/anti) |
|  [06]   | `collect_all(**kwargs) -> Self`                                               | collect all lazy members (`**kwargs` to `collect_all`) |
|  [07]   | `pipe(function, *args, **kwargs) -> T`                                        | thread the collection through a function, polars-style |
|  [08]   | `sample(num_rows, *, overrides, generator) -> Self`                           | random collection-conformant members                   |
|  [09]   | `create_empty() -> Self`                                                      | empty collection-typed members                         |
|  [10]   | `matches(other) -> bool`                                                      | structural collection equality                         |
|  [11]   | `serialize() -> str`                                                          | serialize the collection contract                      |
|  [12]   | `member_schemas() -> dict[str, type[Schema]]`                                 | the member-name-to-schema map                          |
|  [13]   | `common_primary_key() -> list[str]`                                           | primary key shared across members                      |

- `Collection.validate`/`filter`: `skip_member_validation=True` runs only the collection `@filter`/relationship invariants and skips per-member schema validation; `**kwargs` pass to `collect`.
- Per-member parquet IO: `read_parquet` `scan_parquet` `write_parquet` `sink_parquet` key each member to `<member>.parquet` under a directory; a read inspects embedded metadata and re-validates under the `validation` policy — pass `validation="skip"`, or call `validate` explicitly, to read without the implicit re-validation.
- `CollectionFilterResult` is a 2-field `NamedTuple` — `result` (valid collection) and `failure: dict[str, FailureInfo]` keyed by member — with a `collect_all()` method; `count`/`index` are inherited `tuple` members, not results.
- `require_relationship_one_to_one` / `require_relationship_one_to_at_least_one` carry `(lhs, rhs, /, on, *, drop_duplicates) -> pl.LazyFrame`, the 1:1 and 1:{1,N} referential-integrity expressions a `@filter` returns.
- `concat_collection_members(collections, /) -> dict[str, pl.LazyFrame]` concatenates same-typed collections member-wise.

[ENTRYPOINT_SCOPE]: rule and filter markers, column constructors, failure introspection, config

Every `Column` subtype constructor carries the base policy `nullable`, `primary_key`, `unique`, `check`, `alias`, `metadata`, `description`; each subtype row adds its own knobs.

| [INDEX] | [SURFACE]                                                      | [SHAPE]   | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------- | :-------- | :----------------------------------------- |
|  [01]   | `rule(*, group_by) -> RuleFactory`                             | decorator | cross-column / grouped `pl.Expr` predicate |
|  [02]   | `filter() -> Filter`                                           | decorator | collection cross-member filter             |
|  [03]   | `Integer(*, min, min_exclusive, max, max_exclusive, is_in, …)` | ctor      | integer bounds and membership              |
|  [04]   | `String(*, min_length, max_length, regex, …)`                  | ctor      | text length and pattern                    |
|  [05]   | `Decimal(precision, scale, *, min, max, …)`                    | ctor      | fixed-precision bounds                     |
|  [06]   | `FailureInfo.invalid() -> pl.DataFrame`                        | method    | the invalid input rows                     |
|  [07]   | `FailureInfo.details() -> pl.DataFrame`                        | method    | invalid rows and a per-rule status column  |
|  [08]   | `FailureInfo.counts() -> dict[str, int]`                       | method    | rule-name to failure-count map             |
|  [09]   | `FailureInfo.cooccurrence_counts() -> dict[frozenset, int]`    | method    | co-failing rule-set counts                 |
|  [10]   | `Config(**options)`                                            | ctx-mgr   | sampling and failure-example caps          |
|  [11]   | `deserialize_schema(data, strict) -> type[Schema]`             | function  | restore a serialized schema                |
|  [12]   | `deserialize_collection(data, strict) -> type[Collection]`     | function  | restore a serialized collection            |
|  [13]   | `read_parquet_metadata_schema(source)`                         | function  | embedded `Schema` from parquet metadata    |
|  [14]   | `read_parquet_metadata_collection(source)`                     | function  | embedded `Collection` from member metadata |
|  [15]   | `random.Generator(seed)`                                       | ctor      | deterministic RNG for `sample`             |

- `Config` exposes `set_max_sampling_iterations(n)`, `set_max_failure_examples(n)`, `restore_defaults()`, and `options`.
- `FailureInfo.details` adds one `Enum["valid", "invalid", "unknown"]` status column per rule name.
- `read_parquet_metadata_schema`/`read_parquet_metadata_collection` return `None` when the source carries no embedded contract; `deserialize_*` return `None` under `strict=False` on an unrecognized payload.
- `dy.random.Generator(seed=None)` seeds the sampler helpers (`regex_sample`, `date_matches_resolution`, …) that `Schema.sample`/`Collection.sample` consume.
- `Enum(categories, *, sqlalchemy_use_enum, sqlalchemy_enum_name, …)` projects through `to_sqlalchemy_columns` to a native SQL `Enum` when `sqlalchemy_use_enum=True` (named by `sqlalchemy_enum_name`), else to a text column.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Validation folds each `Column`'s inline rules and every `@rule`/`@filter` predicate as `pl.Expr`/`pl.LazyFrame` over the frame, so rule evaluation is Polars expression algebra over the Rust `_native` core, one pass per frame.
- `validate`/`is_valid`/`filter`/`cast` is one call-row-discriminated family: `validate` raises, `is_valid` returns a bool, `filter` splits into `(valid, FailureInfo)`, `cast` coerces dtypes; `cast` and `eager` are call rows.
- `Collection` binds annotated `dy.LazyFrame[MemberSchema]` members under a shared primary key; `@filter` methods and `require_relationship_*` build the referential `pl.LazyFrame` each filter returns.
- `FailureInfo` is the one failure receipt: `invalid` rows, `details`, per-rule `counts`, co-occurrence `cooccurrence_counts`.
- `serialize`/`deserialize_schema` carry the contract as a string embeddable in parquet metadata; `read_parquet_metadata_schema` recovers it and validation runs explicitly (the `Validation` policy governs `Collection` reads), while the `to_*` family projects the one contract to Polars, PyArrow, SQLAlchemy, and Pydantic.
- Each gate captures schema name, primary key, rule names, valid/invalid counts, per-rule and co-occurrence counts, and serializer kind as its receipt.

[STACKING]:
- `polars`(`.api/polars.md`): dataframely is the contract layer over polars — `validate`/`filter` consume `pl.DataFrame`/`pl.LazyFrame`, forward `**kwargs` to `polars.LazyFrame.collect` (`engine="streaming"`), and `@rule`/`@filter` return `pl.Expr`/`pl.LazyFrame`; the `DataFrame[S]`/`LazyFrame[S]` outputs are polars frames carrying a schema tag, so transforms and parquet IO (`read_parquet`/`scan_parquet`/`write_parquet`/`sink_parquet`) stay in the polars owner with `Schema.validate` run explicitly on the result.
- `pyarrow`(`.api/pyarrow.md`) / `arro3-core`(`.api/arro3-core.md`): `to_pyarrow_schema` projects the contract to the wire `pa.Schema`; an Arrow ingest reads into polars, then through `Schema.validate`.
- `connectorx`(`.api/connectorx.md`) / `daft`(`.api/daft.md`): a partitioned database or lakehouse read egresses a `pl.DataFrame` entering `Schema.validate` at the ingest boundary — one contract for source and consumer.
- `deltalake`(`.api/deltalake.md`): `deltalake.write_deltalake`/`DeltaTable` and `polars.scan_delta`/`write_delta` own the delta transaction and IO; dataframely validates the resulting `pl.DataFrame`/`LazyFrame` through `Schema.validate`/`Collection.validate` at the ingest boundary.
- `pandera`(`.api/pandera.md`) / `pointblank`(`.api/pointblank.md`): one validation concern partitioned by engine — Polars-native declarative contracts and `Collection` integrity here, pandas and multi-backend checks to pandera, column-health grading to pointblank.
- `pydantic`(`libs/python/.api/pydantic.md`): `to_pydantic_model` projects the contract to a `BaseModel`, so a row-shaped API or config boundary reuses the one definition.
- data folder: the `contract` page folds `Schema` covenants and `Collection` filters onto one `ContractClaim` through CONTRACT_GATE_FOLD/COVENANT, and the `FailureInfo` receipt stacks into the `profile` grade.

[LOCAL_ADMISSION]:
- Import `import dataframely as dy` at boundary scope; declare one `Schema` per frame contract and one `Collection` per multi-frame integrity set, columns assigned as typed `Column` instances.
- Fold `filter` failures into the `FailureInfo` receipt for a graded gate instead of re-deriving per-column counts; express referential integrity with `require_relationship_*` returned from a `@filter`.
- Read and write frames through the polars owner, then run `Schema.validate`/`Collection.validate` explicitly at the boundary; recover an embedded contract with `read_parquet_metadata_schema`/`read_parquet_metadata_collection` and project to a consuming runtime with the `to_*` family, so a downstream reader binds the projected schema rather than re-declaring the column types.

[RAIL_LAW]:
- Package: `dataframely`
- Owns: declarative Polars `Schema`/`Column` contracts with inline and cross-column rules, `Collection` cross-frame integrity over shared primary keys and filters, eager/lazy validate/filter/cast, `FailureInfo` introspection, and schema/collection serialization with metadata recovery over polars-owned parquet/delta IO.
- Accept: dataframe contract declaration and enforcement feeding the CONTRACT_GATE_FOLD/COVENANT gate, with `DataFrame[S]`/`LazyFrame[S]` outputs flowing to the data and persistence owners.
- Reject: a wrapper-rename of `validate`/`filter`; a per-row assertion loop where `@rule`/`@filter` own the algebra; one validator type per column kind; a hand-stitched anti/semi-join where `require_relationship_*` owns referential integrity; a side-file schema store where `serialize`/`read_parquet_metadata_schema` recover the contract from parquet metadata; re-declared column types the schema already projects.
