# [PY_DATA_API_PANDERA]

`pandera` declares and enforces dataframe contracts through two equivalent surfaces: object-style `DataFrameSchema`/`Column`/`Index` and class-style `DataFrameModel` with `Field` annotations. `Check` and `Hypothesis` carry built-in and custom validation predicates, `Parser` applies pre-validation transforms, and `check_types`/`check_input`/`check_output`/`check_io` decorate functions to validate arguments and returns against typed `Series`/`DataFrame` annotations. `validate` runs eager or `lazy` (collect-all-errors) validation and raises `SchemaError`/`SchemaErrors`; schemas round-trip through `to_yaml`/`from_yaml`, `to_json`/`from_json`, and `to_script`, and `infer_schema` bootstraps a schema from data.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pandera`
- package: `pandera`
- module: `pandera`
- asset: pure Python
- rail: dataframe contract validation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schema, component, and check types
- rail: dataframe contract validation

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [ROLE]                                  |
| :-----: | :---------------- | :------------ | :-------------------------------------- |
|   [1]   | `DataFrameSchema` | object schema | column/index/check contract for a frame |
|   [2]   | `SeriesSchema`    | object schema | contract for a single series            |
|   [3]   | `DataFrameModel`  | class schema  | declarative class-based schema          |
|   [4]   | `Column`          | component     | typed column with checks and parsers    |
|   [5]   | `Index`           | component     | typed index with checks                 |
|   [6]   | `MultiIndex`      | component     | hierarchical index contract             |
|   [7]   | `Check`           | predicate     | column/frame validation predicate       |
|   [8]   | `Hypothesis`      | predicate     | statistical-test validation predicate   |
|   [9]   | `Parser`          | transform     | pre-validation column/frame transform   |
|  [10]   | `DataType`        | dtype         | pandera logical dtype base              |
|  [11]   | `SchemaError`     | failure       | single validation failure               |
|  [12]   | `SchemaErrors`    | failure       | aggregated lazy-validation failures     |

[PUBLIC_TYPE_SCOPE]: dtype vocabulary
- rail: dataframe contract validation

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [ROLE]                         |
| :-----: | :---------------------------- | :------------- | :----------------------------- |
|   [1]   | `Int` / `Int8/16/32/64`       | integer dtype  | signed integer dtypes          |
|   [2]   | `UInt` / `UInt8/16/32/64`     | integer dtype  | unsigned integer dtypes        |
|   [3]   | `Float` / `Float16/32/64/128` | float dtype    | float dtypes                   |
|   [4]   | `Complex` / `Complex64/128`   | complex dtype  | complex dtypes                 |
|   [5]   | `Bool`                        | boolean dtype  | boolean dtype                  |
|   [6]   | `String`                      | string dtype   | string dtype                   |
|   [7]   | `Category`                    | category dtype | categorical dtype              |
|   [8]   | `Date` / `DateTime`           | temporal dtype | date and datetime dtypes       |
|   [9]   | `Timedelta` / `Timestamp`     | temporal dtype | timedelta and timestamp dtypes |
|  [10]   | `Decimal`                     | decimal dtype  | fixed-precision decimal dtype  |
|  [11]   | `Object`                      | object dtype   | opaque Python-object dtype     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schema construction and validation
- rail: dataframe contract validation

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------- |
|   [1]   | `DataFrameSchema(columns, checks, index, ...)`  | construct      | build an object-style frame schema  |
|   [2]   | `Column(dtype, checks, nullable, unique, ...)`  | construct      | declare a typed column              |
|   [3]   | `Index(dtype, checks, ...)` / `MultiIndex(...)` | construct      | declare typed index contracts       |
|   [4]   | `Field(gt, isin, str_matches, nullable, ...)`   | construct      | declare a class-schema field        |
|   [5]   | `validate(check_obj, lazy, head, sample, ...)`  | validate       | run validation, raise on failure    |
|   [6]   | `DataFrameModel.to_schema()` / `validate(df)`   | validate       | derive and run a class schema       |
|   [7]   | `infer_schema(pandas_obj)`                      | bootstrap      | infer a schema from data            |
|   [8]   | `update_column / add_columns / remove_columns`  | evolve         | mutate a schema immutably           |
|   [9]   | `select_columns / set_index / reset_index`      | evolve         | reshape schema columns and index    |
|  [10]   | `to_yaml / from_yaml / to_json / from_json`     | serialize      | round-trip schema definitions       |
|  [11]   | `to_script()` / `example(size)`                 | serialize      | emit schema code, synthesize data   |
|  [12]   | `strategy()` / `DataFrameModel.example()`       | synthesize     | hypothesis strategy and sample data |

[ENTRYPOINT_SCOPE]: checks, parsers, and decorators
- rail: dataframe contract validation

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :----------------------------------------------- | :------------- | :----------------------------------- |
|   [1]   | `Check.gt/ge/lt/le/eq/ne`                        | check builtin  | scalar comparison predicates         |
|   [2]   | `Check.in_range(min, max)` / `Check.between`     | check builtin  | numeric range predicate              |
|   [3]   | `Check.isin(values)` / `Check.notin(values)`     | check builtin  | membership predicates                |
|   [4]   | `Check.str_matches / str_contains / str_length`  | check builtin  | string-pattern predicates            |
|   [5]   | `Check.unique_values_eq / is_monotonic`          | check builtin  | column-shape predicates              |
|   [6]   | `Check(fn, element_wise, groupby, ...)`          | custom check   | custom predicate from a callable     |
|   [7]   | `Hypothesis.one_sample_ttest / two_sample_ttest` | hypothesis     | statistical-test predicates          |
|   [8]   | `Parser(parser_fn, element_wise, ...)`           | transform      | pre-validation transform             |
|   [9]   | `@check_types(lazy, head, tail, sample)`         | decorator      | validate annotated args and return   |
|  [10]   | `@check_input(schema, obj_getter)`               | decorator      | validate a function input arg        |
|  [11]   | `@check_output(schema, obj_getter)`              | decorator      | validate a function return value     |
|  [12]   | `@check_io(out=..., **inputs)`                   | decorator      | validate inputs and outputs together |
|  [13]   | `@check` / `@dataframe_check`                    | model check    | register a model method as a check   |
|  [14]   | `@parser` / `@dataframe_parser`                  | model parser   | register a model method as a parser  |

## [4]-[IMPLEMENTATION_LAW]

[CONTRACT_TOPOLOGY]:
- object schemas (`DataFrameSchema` over `Column`/`Index`) and class schemas (`DataFrameModel` with `Field`) are equivalent; `DataFrameModel.to_schema()` lowers the class form to the object form
- `validate(check_obj, lazy=False)` raises `SchemaError` on first failure; `lazy=True` collects all violations into `SchemaErrors` with a failure-case report
- `head`, `tail`, `sample`, and `random_state` bound validation to a subset for large frames
- `Column`/`Field` carry `nullable`, `unique`, `coerce`, `required`, `regex`, `default`, and `drop_invalid_rows`; `coerce` casts before validation and `drop_invalid_rows` filters failing rows
- `Check` predicates run element-wise or vectorized and may group by another column; `Hypothesis` wraps statistical tests; `Parser` transforms data before checks run
- schema-evolution methods (`update_column`, `add_columns`, `set_index`, ...) return new immutable schemas
- decorators validate against `pandera.typing.Series`/`DataFrame` annotations; `check_types` enforces annotated function signatures

[LOCAL_ADMISSION]:
- Define contracts as `DataFrameModel` subclasses with typed `Field` constraints for static-checkable, reusable schemas; use `DataFrameSchema` where schemas are built dynamically.
- Gate function boundaries with `@check_types` (or `@check_input`/`@check_output`/`@check_io`) so typed `Series`/`DataFrame` annotations are enforced at call time.
- Use `lazy=True` to collect a full failure report at boundaries; use `coerce=True` and `Parser` to normalize before validation rather than pre-cleaning by hand.
- Round-trip schemas through `to_yaml`/`from_yaml` for versioned contracts and use `infer_schema` plus `to_script` to bootstrap, never as the final contract.

[RAIL_LAW]:
- Package: `pandera`
- Owns: declarative dataframe/series contracts, validation execution, and function-boundary enforcement
- Accept: pandas (and supported backend) frames and series, typed annotations, and serialized schema definitions
- Reject: ad-hoc `assert`-based column checks, manual dtype/range validation loops, untyped boundary functions that skip schema enforcement
