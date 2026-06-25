# [PY_DATA_API_PANDERA]

`pandera` declares and enforces dataframe contracts through two equivalent surfaces: object-style `DataFrameSchema`/`Column`/`Index` and class-style `DataFrameModel` with `Field` annotations, bound to a frame library by the imported backend namespace (`pandera.pandas`, `pandera.polars`, `pandera.pyspark`, `pandera.geopandas`) rather than a runtime branch — the polars backend pushes validation into the `LazyFrame` scan. `Check` and `Hypothesis` carry built-in and custom validation predicates, `Parser` applies pre-validation transforms, and `check_types`/`check_input`/`check_output`/`check_io` decorate functions to validate arguments and returns against typed `pandera.typing.Series`/`DataFrame`/`Index` annotations. `validate` runs eager or `lazy` (collect-all-errors) validation and raises `SchemaError`/`SchemaErrors` (whose `.failure_cases` is a foldable frame); `SchemaInitError`/`SchemaDefinitionError` fail fast at build time. Schemas round-trip through `to_yaml`/`from_yaml`, `to_json`/`from_json`, and `to_script`, and `infer_schema` bootstraps a schema from data.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pandera`
- package: `pandera`
- module: `pandera`
- floor: `>=0.31.1`
- license: MIT
- asset: pure Python (`py3-none-any`)
- rail: dataframe contract validation
- import: `import pandera.pandas as pa` is the canonical backend-namespaced import at the `>=0.31` floor; the bare `import pandera as pa` is the legacy path and resolves the pandas backend only — author against `pandera.pandas`/`pandera.polars`/`pandera.pyspark` so the schema engine binds to the frame library without a per-call branch.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schema, component, and check types
- rail: dataframe contract validation

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]  | [ROLE]                                                                                                         |
| :-----: | :---------------------------------------- | :------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `DataFrameSchema`                         | object schema  | column/index/check contract for a frame                                                                        |
|  [02]   | `SeriesSchema`                            | object schema  | contract for a single series                                                                                   |
|  [03]   | `DataFrameModel`                          | class schema   | declarative class-based schema                                                                                 |
|  [04]   | `Column`                                  | component      | typed column with checks and parsers                                                                           |
|  [05]   | `Index`                                   | component      | typed index with checks                                                                                        |
|  [06]   | `MultiIndex`                              | component      | hierarchical index contract                                                                                    |
|  [07]   | `Check`                                   | predicate      | column/frame validation predicate                                                                              |
|  [08]   | `Hypothesis`                              | predicate      | statistical-test validation predicate                                                                          |
|  [09]   | `Parser`                                  | transform      | pre-validation column/frame transform                                                                          |
|  [10]   | `DataType`                                | dtype          | pandera logical dtype base                                                                                     |
|  [11]   | `pandera.errors.SchemaError`              | failure        | single validation failure; `.schema`/`.check`/`.failure_cases` attrs                                           |
|  [12]   | `pandera.errors.SchemaErrors`             | failure        | aggregated lazy-validation failures; `.failure_cases` frame (`schema_context`/`column`/`check`/`failure_case`) |
|  [13]   | `pandera.errors.{SchemaInitError,SchemaDefinitionError}` | failure | schema-construction and ill-formed-definition errors raised at build time, not validation time          |
|  [14]   | `pandera.polars.{DataFrameSchema,DataFrameModel,Column}` | polars backend | native polars `LazyFrame`/`DataFrame` schema/model (`pandera.api.polars`), pushes validation into the lazy scan |
|  [15]   | `pandera.pyspark.{DataFrameSchema,DataFrameModel}` | pyspark backend | native PySpark SQL schema validating distributed frames; same `Column`/`Check`/`Field` vocabulary    |
|  [16]   | `pandera.geopandas.{GeoDataFrameSchema,GeoSeriesSchema}` | geo backend | geometry-aware schema/series for GeoPandas frames over the `geopandas` extra                            |
|  [17]   | `pandera.typing.{Series,DataFrame,Index}` | annotation     | typed annotations driving `@check_types`; `pandera.typing.polars.{Series,DataFrame}` mirror for the polars backend |

[PUBLIC_TYPE_SCOPE]: dtype vocabulary
- rail: dataframe contract validation

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [ROLE]                         |
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
- rail: dataframe contract validation

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `DataFrameSchema(columns, checks, index, ...)`  | construct      | build an object-style frame schema  |
|  [02]   | `Column(dtype, checks, nullable, unique, ...)`  | construct      | declare a typed column              |
|  [03]   | `Index(dtype, checks, ...)` / `MultiIndex(...)` | construct      | declare typed index contracts       |
|  [04]   | `Field(gt, isin, str_matches, nullable, ...)`   | construct      | declare a class-schema field        |
|  [05]   | `validate(check_obj, lazy, head, sample, ...)`  | validate       | run validation, raise on failure    |
|  [06]   | `DataFrameModel.to_schema()` / `validate(df)`   | validate       | derive and run a class schema       |
|  [07]   | `infer_schema(pandas_obj)`                      | bootstrap      | infer a schema from data            |
|  [08]   | `update_column / add_columns / remove_columns`  | evolve         | mutate a schema immutably           |
|  [09]   | `select_columns / set_index / reset_index`      | evolve         | reshape schema columns and index    |
|  [10]   | `to_yaml / from_yaml / to_json / from_json`     | serialize      | round-trip schema definitions       |
|  [11]   | `to_script()` / `example(size)`                 | serialize      | emit schema code, synthesize data   |
|  [12]   | `strategy()` / `DataFrameModel.example()`       | synthesize     | hypothesis strategy and sample data |

[ENTRYPOINT_SCOPE]: checks, parsers, and decorators
- rail: dataframe contract validation

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :----------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `Check.gt/ge/lt/le/eq/ne`                        | check builtin  | scalar comparison predicates         |
|  [02]   | `Check.in_range(min, max)` / `Check.between`     | check builtin  | numeric range predicate              |
|  [03]   | `Check.isin(values)` / `Check.notin(values)`     | check builtin  | membership predicates                |
|  [04]   | `Check.str_matches / str_contains / str_length`  | check builtin  | string-pattern predicates            |
|  [05]   | `Check.unique_values_eq / is_monotonic`          | check builtin  | column-shape predicates              |
|  [06]   | `Check(fn, element_wise, groupby, ...)`          | custom check   | custom predicate from a callable     |
|  [07]   | `Hypothesis.one_sample_ttest / two_sample_ttest` | hypothesis     | statistical-test predicates          |
|  [08]   | `Parser(parser_fn, element_wise, ...)`           | transform      | pre-validation transform             |
|  [09]   | `@check_types(lazy, head, tail, sample)`         | decorator      | validate annotated args and return   |
|  [10]   | `@check_input(schema, obj_getter)`               | decorator      | validate a function input arg        |
|  [11]   | `@check_output(schema, obj_getter)`              | decorator      | validate a function return value     |
|  [12]   | `@check_io(out=..., **inputs)`                   | decorator      | validate inputs and outputs together |
|  [13]   | `@check` / `@dataframe_check`                    | model check    | register a model method as a check   |
|  [14]   | `@parser` / `@dataframe_parser`                  | model parser   | register a model method as a parser  |

## [04]-[IMPLEMENTATION_LAW]

[CONTRACT_TOPOLOGY]:
- backend axis: the validation engine is selected by the namespace, not by a runtime branch — `pandera.pandas` validates pandas frames, `pandera.polars` validates polars frames natively under the same `Column`/`Check`/`Field` vocabulary (a `pl.LazyFrame` input defers unraised at the `0.31.1` floor, so the raise contract is materialization-bound — collect to a `pl.DataFrame` at the gate for `SchemaError`/`SchemaErrors` to fire), `pandera.pyspark` validates distributed Spark frames, and `pandera.geopandas` adds geometry-aware columns; one schema vocabulary, one backend binding
- object schemas (`DataFrameSchema` over `Column`/`Index`) and class schemas (`DataFrameModel` with `Field`) are equivalent; `DataFrameModel.to_schema()` lowers the class form to the object form, and the polars-native `pandera.polars.DataFrameModel` lowers the same way for the lazy backend
- `validate(check_obj, lazy=False)` raises `SchemaError` on first failure; `lazy=True` collects all violations into `SchemaErrors` whose `.failure_cases` is itself a frame (`schema_context`/`column`/`check`/`failure_case`/`index`) — the report is a dataframe to be folded over, not a string to be parsed
- `head`, `tail`, `sample`, and `random_state` bound validation to a subset for large frames
- `Column`/`Field` carry `nullable`, `unique`, `coerce`, `required`, `regex`, `default`, and `drop_invalid_rows`; `coerce` casts before validation and `drop_invalid_rows` filters failing rows (returns the surviving frame instead of raising)
- `Check` predicates run element-wise or vectorized and may group by another column; `Hypothesis` wraps statistical tests; `Parser` transforms data before checks run
- schema-evolution methods (`update_column`, `add_columns`, `set_index`, ...) return new immutable schemas
- decorators validate against `pandera.typing.Series`/`DataFrame`/`Index` annotations (and `pandera.typing.polars.*` for the polars backend); `check_types` enforces annotated function signatures
- `SchemaInitError`/`SchemaDefinitionError` raise at schema-build time (bad dtype, ill-formed model), distinct from the `SchemaError`/`SchemaErrors` validation rail — fail-fast on definition versus collect-all on data

[LOCAL_ADMISSION]:
- Define contracts as `DataFrameModel` subclasses with typed `Field` constraints for static-checkable, reusable schemas; use `DataFrameSchema` where schemas are built dynamically. Import the backend namespace (`pandera.polars`/`pandera.pandas`) matching the frame the page owns rather than the bare `pandera`.
- Gate function boundaries with `@check_types` (or `@check_input`/`@check_output`/`@check_io`) so typed `Series`/`DataFrame` annotations are enforced at call time; the typed annotation is the contract, not a docstring.
- Use `lazy=True` and fold `SchemaErrors.failure_cases` into a typed failure receipt — the failure-case frame stacks directly into a `pointblank`/`dataframely`-style grade rather than re-deriving per-column error counts by hand; use `coerce=True` and `Parser` to normalize before validation rather than pre-cleaning by hand.
- On the polars plane, define `pandera.polars.DataFrameModel`/`DataFrameSchema` so validation runs on the polars frame natively, never collecting to pandas to validate. The raise contract is materialization-bound at the `0.31.1` floor: `validate` handed a `pl.LazyFrame` returns the frame deferred and raises neither `SchemaError`/`SchemaErrors` at `validate` nor at the subsequent `.collect()`, so the error rail fires only on a materialized `pl.DataFrame` — collect the lazy plan once at the gate (`schema.validate(frame.collect(), ...)`) so a breach surfaces, rather than passing the `LazyFrame` and silently admitting every frame.
- Round-trip schemas through `to_yaml`/`from_yaml` for versioned contracts and use `infer_schema` plus `to_script` to bootstrap, never as the final contract.

[RAIL_LAW]:
- Package: `pandera`
- Owns: declarative dataframe/series contracts across pandas/polars/pyspark/geopandas backends, validation execution, and function-boundary enforcement
- Accept: backend frames and series via the matching namespace, typed `pandera.typing` annotations, and serialized schema definitions
- Reject: ad-hoc `assert`-based column checks, manual dtype/range validation loops, untyped boundary functions that skip schema enforcement, collecting a `LazyFrame` to pandas solely to validate when `pandera.polars` validates the scan in place, bare `import pandera as pa` where the backend namespace fixes the engine
