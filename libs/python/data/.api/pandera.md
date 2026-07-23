# [PY_DATA_API_PANDERA]

`pandera` mints dataframe and series contracts and enforces them across pandas, polars, pyspark, and geopandas backends, the engine bound by the imported namespace rather than a runtime branch. Object schemas (`DataFrameSchema` over `Column`/`Index`) and class schemas (`DataFrameModel` with `Field`) share one vocabulary — `to_schema()` lowers the class form to the object form. `validate` runs eager or `lazy`, raising `SchemaError`/`SchemaErrors` whose `.failure_cases` is a foldable frame; schemas round-trip through YAML/JSON and `infer_schema` bootstraps one from data.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pandera`
- package: `pandera` (MIT)
- module: `pandera`
- namespaces: `pandera.pandas`, `pandera.polars`, `pandera.pyspark`, `pandera.geopandas`
- rail: dataframe contract validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schema, component, and check types

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY]   | [CAPABILITY]                                            |
| :-----: | :--------------------------------------------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `DataFrameSchema`                                          | object schema   | column/index/check contract for a frame                 |
|  [02]   | `SeriesSchema`                                             | object schema   | contract for a single series                            |
|  [03]   | `DataFrameModel`                                           | class schema    | declarative class-based schema                          |
|  [04]   | `Column`                                                   | component       | typed column with checks and parsers                    |
|  [05]   | `Index`                                                    | component       | typed index with checks                                 |
|  [06]   | `MultiIndex`                                               | component       | hierarchical index contract                             |
|  [07]   | `Check`                                                    | predicate       | column/frame validation predicate                       |
|  [08]   | `Hypothesis`                                               | predicate       | statistical-test validation predicate                   |
|  [09]   | `Parser`                                                   | transform       | pre-validation column/frame transform                   |
|  [10]   | `DataType`                                                 | dtype           | pandera logical dtype base                              |
|  [11]   | `pandera.errors.SchemaError`                               | failure         | single failure; `.schema`/`.check`/`.failure_cases`     |
|  [12]   | `pandera.errors.SchemaErrors`                              | failure         | aggregated lazy failures; `.failure_cases` frame        |
|  [13]   | `pandera.errors.{SchemaInitError,SchemaDefinitionError}`   | failure         | build-time construction/ill-formed-definition errors    |
|  [14]   | `pandera.polars.{DataFrameSchema,DataFrameModel,Column}`   | polars backend  | native polars `LazyFrame`/`DataFrame` schema/model      |
|  [15]   | `pandera.pyspark.{DataFrameSchema,DataFrameModel}`         | pyspark backend | native PySpark SQL schema over distributed frames       |
|  [16]   | `pandera.geopandas.{GeoDataFrameSchema,GeoDataFrameModel}` | geo backend     | geometry-aware schema and model for GeoPandas frames    |
|  [17]   | `pandera.typing.{Series,DataFrame,Index}`                  | annotation      | typed annotations for `@check_types` (`.polars` mirror) |

[PUBLIC_TYPE_SCOPE]: dtype vocabulary

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                   |
| :-----: | :---------------------------- | :------------- | :----------------------------- |
|  [01]   | `Int` / `Int8/16/32/64`       | integer dtype  | signed integer dtypes          |
|  [02]   | `UInt` / `UInt8/16/32/64`     | integer dtype  | unsigned integer dtypes        |
|  [03]   | `Float` / `Float16/32/64/128` | float dtype    | float dtypes                   |
|  [04]   | `Complex` / `Complex64/128`   | complex dtype  | complex dtypes                 |
|  [05]   | `Bool`                        | boolean dtype  | boolean dtype                  |
|  [06]   | `String`                      | string dtype   | string dtype                   |
|  [07]   | `Category`                    | category dtype | categorical dtype              |
|  [08]   | `Date` / `DateTime`           | temporal dtype | date and datetime dtypes       |
|  [09]   | `Timedelta` / `Timestamp`     | temporal dtype | timedelta and timestamp dtypes |
|  [10]   | `Decimal`                     | decimal dtype  | fixed-precision decimal dtype  |
|  [11]   | `Object`                      | object dtype   | opaque Python-object dtype     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schema construction and validation

| [INDEX] | [SURFACE]                                       | [SHAPE]    | [CAPABILITY]                        |
| :-----: | :---------------------------------------------- | :--------- | :---------------------------------- |
|  [01]   | `DataFrameSchema(columns, checks, index, ...)`  | construct  | build an object-style frame schema  |
|  [02]   | `Column(dtype, checks, nullable, unique, ...)`  | construct  | declare a typed column              |
|  [03]   | `Index(dtype, checks, ...)` / `MultiIndex(...)` | construct  | declare typed index contracts       |
|  [04]   | `Field(gt, isin, str_matches, nullable, ...)`   | construct  | declare a class-schema field        |
|  [05]   | `validate(check_obj, lazy, head, sample, ...)`  | validate   | run validation, raise on failure    |
|  [06]   | `DataFrameModel.to_schema()` / `validate(df)`   | validate   | derive and run a class schema       |
|  [07]   | `infer_schema(pandas_obj)`                      | bootstrap  | infer a schema from data            |
|  [08]   | `update_column / add_columns / remove_columns`  | evolve     | mutate a schema immutably           |
|  [09]   | `select_columns / set_index / reset_index`      | evolve     | reshape schema columns and index    |
|  [10]   | `to_yaml / from_yaml / to_json / from_json`     | serialize  | round-trip schema definitions       |
|  [11]   | `to_script()` / `example(size)`                 | serialize  | emit schema code, synthesize data   |
|  [12]   | `strategy()` / `DataFrameModel.example()`       | synthesize | hypothesis strategy and sample data |

[ENTRYPOINT_SCOPE]: checks, parsers, and decorators

| [INDEX] | [SURFACE]                                        | [SHAPE]       | [CAPABILITY]                         |
| :-----: | :----------------------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `Check.gt/ge/lt/le/eq/ne`                        | check builtin | scalar comparison predicates         |
|  [02]   | `Check.in_range(min, max)` / `Check.between`     | check builtin | numeric range predicate              |
|  [03]   | `Check.isin(values)` / `Check.notin(values)`     | check builtin | membership predicates                |
|  [04]   | `Check.str_matches / str_contains / str_length`  | check builtin | string-pattern predicates            |
|  [05]   | `Check.unique_values_eq / is_monotonic`          | check builtin | column-shape predicates              |
|  [06]   | `Check(fn, element_wise, groupby, ...)`          | custom check  | custom predicate from a callable     |
|  [07]   | `Hypothesis.one_sample_ttest / two_sample_ttest` | hypothesis    | statistical-test predicates          |
|  [08]   | `Parser(parser_fn, element_wise, ...)`           | transform     | pre-validation transform             |
|  [09]   | `@check_types(lazy, head, tail, sample)`         | decorator     | validate annotated args and return   |
|  [10]   | `@check_input(schema, obj_getter)`               | decorator     | validate a function input arg        |
|  [11]   | `@check_output(schema, obj_getter)`              | decorator     | validate a function return value     |
|  [12]   | `@check_io(out=..., **inputs)`                   | decorator     | validate inputs and outputs together |
|  [13]   | `@check` / `@dataframe_check`                    | model check   | register a model method as a check   |
|  [14]   | `@parser` / `@dataframe_parser`                  | model parser  | register a model method as a parser  |

- `Check.gt`/`ge`/`lt`/`le`/`eq`/`ne` are short forms of `greater_than`/`greater_than_or_equal_to`/`less_than`/`less_than_or_equal_to`/`equal_to`/`not_equal_to`; both spellings resolve.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Namespace selects the engine: `pandera.pandas` validates pandas frames, `pandera.polars` the polars scan natively, `pandera.pyspark` distributed Spark frames, `pandera.geopandas` geometry-aware columns — one `Column`/`Check`/`Field` vocabulary, one backend binding, no runtime branch.
- Object and class schemas are equivalent; `DataFrameModel.to_schema()` lowers the class form to the object form, and `pandera.polars.DataFrameModel` lowers the same way for the lazy backend.
- `validate(check_obj, lazy=False)` raises `SchemaError` on first failure; `lazy=True` collects every violation into `SchemaErrors` whose `.failure_cases` is a frame keyed `schema_context`/`column`/`check`/`failure_case`/`index` — a dataframe folded over, never a string parsed.
- `head`, `tail`, `sample`, and `random_state` bound validation to a subset of a large frame.
- `Column`/`Field` carry `nullable`, `unique`, `coerce`, `required`, `regex`, `default`, and `drop_invalid_rows`; `coerce` casts before validation and `drop_invalid_rows` returns the surviving frame instead of raising.
- `Check` runs element-wise or vectorized and groups by another column; `Hypothesis` wraps a statistical test; `Parser` transforms data before checks run.
- Schema-evolution methods (`update_column`, `add_columns`, `set_index`, ...) return new immutable schemas.
- Decorators enforce `pandera.typing.Series`/`DataFrame`/`Index` annotations (`pandera.typing.polars.*` for the polars backend); the annotation is the contract.
- `SchemaInitError`/`SchemaDefinitionError` raise at schema-build time on a bad dtype or ill-formed model, the fail-fast definition rail distinct from the collect-all data rail.

[STACKING]:
- `dataframely`(`.api/dataframely.md`) / `pointblank`(`.api/pointblank.md`): one validation concern partitioned by engine — pandas and multi-backend declarative contracts here, Polars-native `Collection` integrity to dataframely, column-health grading to pointblank.
- `polars`(`.api/polars.md`) / `pandas`(`.api/pandas.md`): `pandera.polars` validates the `pl.LazyFrame`/`pl.DataFrame` scan and `pandera.pandas` the pandas frame; the validated frame stays in the backend owner.
- `ibis-framework`(`.api/ibis-framework.md`): `pandera.ibis` validates an Ibis table expression under the same `Column`/`Check` vocabulary before execution.
- `pydantic`(`libs/python/.api/pydantic.md`): `pandera.typing.Series`/`DataFrame` annotations gate a `@check_types` boundary the way a `BaseModel` gates a scalar one.
- data folder: the `tabular/contract` `DataQuality` owner folds `QualityRule` rows onto one `pandera.polars.DataFrameSchema`, maps each `CheckKind` to a concrete `pandera.Check` through `expression.collections.Map` tables, routes uniqueness to `Column(unique=)`, and folds `SchemaErrors.failure_cases` `column`/`check` pairs into `ContractClaim.breaches`.

[LOCAL_ADMISSION]:
- Declare a contract as a `DataFrameModel` subclass with typed `Field` constraints for a static-checkable reusable schema, `DataFrameSchema` where the schema is built dynamically; import the backend namespace matching the frame the page owns.
- Gate a function boundary with `@check_types` (or `@check_input`/`@check_output`/`@check_io`) so a typed `Series`/`DataFrame` annotation enforces at call time.
- Use `lazy=True` and fold `SchemaErrors.failure_cases` into a typed failure receipt that stacks into a `pointblank`/`dataframely`-style grade; use `coerce=True` and `Parser` to normalize before validation.
- On the polars plane, define `pandera.polars` schemas so validation runs on the polars frame natively; collect a `pl.LazyFrame` to a `pl.DataFrame` at the gate so `SchemaError`/`SchemaErrors` fires. Round-trip through `to_yaml`/`from_yaml` for a versioned contract; `infer_schema` and `to_script` bootstrap, never the final contract.

[RAIL_LAW]:
- Package: `pandera`
- Owns: declarative dataframe/series contracts across pandas/polars/pyspark/geopandas backends, validation execution, and function-boundary enforcement.
- Accept: backend frames and series via the matching namespace, typed `pandera.typing` annotations, and serialized schema definitions.
- Reject: an `assert`-based column check, a manual dtype/range validation loop, an untyped boundary function skipping schema enforcement, and collecting a `LazyFrame` to pandas solely to validate where `pandera.polars` validates the scan in place.
