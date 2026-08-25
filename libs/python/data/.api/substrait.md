# [PY_DATA_API_SUBSTRAIT]

`substrait` mints the standalone typed Substrait plan IR the data branch gates before execution: a protobuf `Plan` model over raw wire bytes, an `ExtensionRegistry` resolving functions by URN and signature, and `type_inference` validating a plan by its inferred output `NamedStruct`. Package owner folds `Plan.ParseFromString`, `ExtensionRegistry.lookup_urn`, and `infer_plan_schema` into one admission gate over inbound Persistence plan bytes. `datafusion` and the DuckDB substrait extension exchange this wire `Plan`, so the owner gates the shared artifact rather than either engine's parser.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plan model, registry, and renderer roots

`Plan` is the wire root the gate parses and validates; `PlanRel`/`Rel`/`RelRoot` stage its relational algebra, `Expression`/`ExtendedExpression` the scalar layer, `Type`/`NamedStruct` the schema layer — all `substrait.proto` messages. `ExtensionRegistry` roots function resolution and extension loading; `FunctionEntry` records a resolved function and `FunctionType` its kind.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]     | [CAPABILITY]                                                          |
| :-----: | :------------------------------------- | :---------------- | :-------------------------------------------------------------------- |
|  [01]   | `proto.Plan`                           | plan message      | wire root — versioned relations, extension URNs, and declarations     |
|  [02]   | `proto.PlanRel`                        | plan-relation     | one plan relation (`rel` or named `root`)                             |
|  [03]   | `proto.RelRoot`                        | named root        | output-relation wrapper carrying field names                          |
|  [04]   | `proto.Rel`                            | relation union    | `oneof` over the relation family below                                |
|  [05]   | `proto.ReadRel`                        | leaf relation     | named-table, virtual-table, or file scan source                       |
|  [06]   | `proto.ProjectRel`                     | relation          | column/expression projection                                          |
|  [07]   | `proto.FilterRel`                      | relation          | predicate filter                                                      |
|  [08]   | `proto.AggregateRel`                   | relation          | grouping and measure aggregation                                      |
|  [09]   | `proto.SortRel`                        | relation          | ordered relation                                                      |
|  [10]   | `proto.FetchRel`                       | relation          | offset/limit slice                                                    |
|  [11]   | `proto.JoinRel`                        | relation          | logical join (peers `HashJoinRel`/`MergeJoinRel`/`NestedLoopJoinRel`) |
|  [12]   | `proto.CrossRel`                       | relation          | cross product                                                         |
|  [13]   | `proto.SetRel`                         | relation          | set algebra over inputs                                               |
|  [14]   | `proto.WriteRel`                       | write relation    | table write sink (peer `DdlRel`/`UpdateRel`)                          |
|  [15]   | `proto.Expression`                     | expression        | scalar/predicate/literal/nested expression node                       |
|  [16]   | `proto.ExtendedExpression`             | expression bundle | named expressions over a base schema                                  |
|  [17]   | `proto.Type`                           | type              | Substrait type node (nested `Type.Struct`)                            |
|  [18]   | `proto.NamedStruct`                    | schema            | field-named struct schema                                             |
|  [19]   | `proto.Version`                        | version           | plan producer/substrait version stamp                                 |
|  [20]   | `proto.SimpleExtensionURN`             | extension         | extension-space declaration — `urn` beside `extension_urn_anchor`     |
|  [21]   | `proto.SimpleExtensionDeclaration`     | extension         | function/type extension binding                                       |
|  [22]   | `proto.AdvancedExtension`              | extension         | optimization/enhancement extension payload                            |
|  [23]   | `extension_registry.ExtensionRegistry` | registry          | extension-YAML/dict loader and function resolver                      |
|  [24]   | `extension_registry.FunctionEntry`     | record            | resolved-function definition record                                   |
|  [25]   | `extension_registry.FunctionType`      | enum              | scalar/aggregate/window function kind                                 |
|  [26]   | `utils.display.PlanPrinter`            | renderer          | plan/expression text renderer                                         |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Plan` byte ingress, egress, and inspection

Every surface is a `proto.Plan` method: the gate parses untrusted bytes with `ParseFromString` or the `FromString` classmethod, re-emits canonical bytes with `SerializeToString`, and probes presence with `HasField`/`WhichOneof`. `relations`, `version`, `extension_urns`, `extensions`, and `advanced_extensions` are the message fields the gate reads.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                        | [CAPABILITY]                                |
| :-----: | :------------------ | :---------------------------------- | :------------------------------------------ |
|  [01]   | `ParseFromString`   | `(serialized: bytes) -> int`        | parse untrusted wire bytes into the message |
|  [02]   | `FromString`        | `(serialized: bytes) -> Plan` (cls) | classmethod parse to a fresh `Plan`         |
|  [03]   | `MergeFromString`   | `(serialized: bytes) -> int`        | merge additional wire bytes                 |
|  [04]   | `SerializeToString` | `() -> bytes`                       | canonical protobuf byte egress              |
|  [05]   | `ByteSize`          | `() -> int`                         | wire byte length for gate diagnostics       |
|  [06]   | `HasField`          | `(field_name: str) -> bool`         | presence probe (peer `WhichOneof(oneof)`)   |
|  [07]   | `IsInitialized`     | `() -> bool`                        | required-field completeness check           |

[CONSUMER]: `tabular/query#QUERY` `_plan_refusal` parses the inbound `Federation.execute` wire with `ParseFromString` and reads `relations` and `extension_urns` off the parsed message; a `google.protobuf.message.DecodeError` is the `PlanRefusal.UNPARSEABLE` row and an empty `relations` the `NO_RELATION` row.

[ENTRYPOINT_SCOPE]: `type_inference` schema validation

`infer_plan_schema` walks the relation tree and returns the output `NamedStruct`, raising on a malformed or type-inconsistent relation — a returned struct is schema-valid, a raise is the gate's rejection. Peers infer at each altitude.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                         | [CAPABILITY]                            |
| :-----: | :--------------------------------- | :--------------------------------------------------- | :-------------------------------------- |
|  [01]   | `infer_plan_schema`                | `(plan: Plan) -> NamedStruct`                        | validate a plan by inferring its schema |
|  [02]   | `infer_rel_schema`                 | `(rel: Rel) -> Type.Struct`                          | infer one relation's output struct      |
|  [03]   | `infer_expression_type`            | `(expression, parent_schema: Type.Struct) -> Type`   | infer a scalar expression's type        |
|  [04]   | `infer_literal_type`               | `(literal: Expression.Literal) -> Type`              | infer a literal's type                  |
|  [05]   | `infer_nested_type`                | `(nested: Expression.Nested, parent_schema) -> Type` | infer a nested (list/map/struct) type   |
|  [06]   | `infer_extended_expression_schema` | `(ee: ExtendedExpression) -> Type.Struct`            | infer an extended-expression schema     |

[CONSUMER]: `tabular/query#QUERY` `_plan_refusal` calls `infer_plan_schema` as the last admission step and reads the raise alone, which arrives as a bare `Exception` for every unhandled relation and type shape alike — so the catch is as wide as this surface raises, and a successful inference is the `UNTYPED_OUTPUT` row not firing.

[ENTRYPOINT_SCOPE]: `ExtensionRegistry` function resolution and extension loading

`ExtensionRegistry(load_default_extensions=True)` loads the bundled simple-extension YAMLs at construction; `register_extension_yaml`/`register_extension_dict` add custom definitions. `lookup_urn` maps one declared extension space to its anchor or `None`, which is the resolution a gate over `Plan.extension_urns` reads; `lookup_function` resolves a `(urn, name, signature)` triple to a `(FunctionEntry, Type)` pair or `None`, the finer overload check a walk over `Plan.extensions` earns.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                   | [CAPABILITY]                           |
| :-----: | :--------------------------- | :------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `ExtensionRegistry`          | `(load_default_extensions=True)`                               | construct with bundled extensions      |
|  [02]   | `register_extension_yaml`    | `(fname: str \| pathlib.Path) -> None`                         | load a custom simple-extension YAML    |
|  [03]   | `register_extension_dict`    | `(definitions: dict) -> None`                                  | load extension definitions from a dict |
|  [04]   | `lookup_function`            | `(urn, name, sig: Seq[Type]) -> (FunctionEntry, Type) \| None` | resolve one function overload          |
|  [05]   | `list_functions`             | `(urn, name, sig) -> list[(FunctionEntry, Type)]`              | enumerate overloads within a URN       |
|  [06]   | `list_functions_across_urns` | `(name, sig) -> list[(FunctionEntry, Type)]`                   | enumerate overloads across URNs        |
|  [07]   | `lookup_urn`                 | `(urn: str) -> int \| None`                                    | map a URN to its extension anchor      |

[CONSUMER]: `tabular/query#QUERY` holds one default-loaded `_REGISTRY` and resolves every `extension_urns` entry through `lookup_urn`, a `None` answering the `PlanRefusal.UNKNOWN_EXTENSION` row; an estate function vocabulary beyond the bundled set registers there once through `register_extension_yaml`, so every inbound plan reads one vocabulary. `lookup_function` stays unbound: the gate resolves the extension SPACE a plan declares, and resolving each function overload needs the `(name, signature)` pair only a declaration walk carries.

[CONSUMER]: `tabular/query#QUERY` `_plan_provenance` re-reads `extension_urns` on the admitted plan and lands each `SimpleExtensionURN.urn` as a `("substrait-urn", urn)` lineage edge on the query result, so the foreign producer's function vocabulary survives the gate that resolved it.

[EXTENSION_SCHEMA_SKEW]: this installed distribution carries the URN-era extension schema and no URI-era field survives beside it — `Plan.extension_urns` sits at field 8 over `SimpleExtensionURN { extension_urn_anchor = 1, urn = 2 }`, field 1 is unassigned, and each `SimpleExtensionDeclaration` nested row (`ExtensionType`, `ExtensionTypeVariation`, `ExtensionFunction`) back-references its space through `extension_urn_reference` at field 4. Producers still minting the retired `extension_uris` at field 1 with `extension_uri_reference` at field 1 therefore parse CLEAN: proto3 files both retired fields into the unknown set, `extension_urns` reads empty, and every declaration's reference reads the 0 default. `libs/dotnet/Rasm.Persistence/.api/api-flowtide-substrait.md` records the producer end that does exactly this, so `tabular/query#QUERY` refuses declarations-without-spaces on its own `RETIRED_EXTENSION_SCHEMA` row ahead of the resolution check, which a vacuous empty list otherwise passes.

[ENTRYPOINT_SCOPE]: `builders.plan` relation construction

Every `builders.plan` surface returns `Callable[[ExtensionRegistry], Plan]` — a plan thunk resolved by applying a registry, so leaf builders and unary/binary combinators compose lazily and bind functions once. A relation input is a `Plan` or another thunk; an expression argument is an `ExtendedExpression` or a `Callable[[NamedStruct, ExtensionRegistry], ExtendedExpression]`. `read_named_table` is the leaf source, `write_named_table` the sink, the combinators mirroring the `proto` relation family.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                       | [CAPABILITY]                     |
| :-----: | :------------------- | :----------------------------------------------------------------- | :------------------------------- |
|  [01]   | `read_named_table`   | `(names: str \| Iterable[str], named_struct, ext=None)`            | leaf named-table scan thunk      |
|  [02]   | `project`            | `(input, expressions: Iterable[<expr>], ext=None)`                 | append projected expressions     |
|  [03]   | `select`             | `(plan, expressions: Iterable[<expr>], ext=None)`                  | select a subset of fields        |
|  [04]   | `filter`             | `(plan, expression: <expr>, ext=None)`                             | predicate filter                 |
|  [05]   | `aggregate`          | `(input, grouping_expressions, measures, ext=None)`                | grouping and measure aggregation |
|  [06]   | `sort`               | `(plan, expressions: Iterable[<expr> \| (<expr>, int)], ext=None)` | ordered relation                 |
|  [07]   | `fetch`              | `(plan, offset: <expr>, count: <expr>, ext=None)`                  | offset/limit slice               |
|  [08]   | `join`               | `(left, right, expression: <expr>, type, ext=None)`                | logical join on a predicate      |
|  [09]   | `cross`              | `(left, right, ext=None)`                                          | cross product                    |
|  [10]   | `set`                | `(inputs: Iterable[<plan>], op)`                                   | set algebra over inputs          |
|  [11]   | `write_named_table`  | `(table_names: str \| Iterable[str], input, create_mode=None)`     | named-table write sink           |
|  [12]   | `resolve_expression` | `(expr, base_schema: NamedStruct, registry) -> ExtendedExpression` | bind an expression thunk         |

[ENTRYPOINT_SCOPE]: `builders.type` and `builders.extended_expression` construction

`builders.type` mints a `proto.Type` per primitive/parametric kind, each `(..., nullable=True)`; `named_struct(names, struct)` pairs field names with a non-nullable `struct`. `builders.extended_expression` mints expression thunks: `column`/`literal`/`cast` are leaves, `scalar_function`/`aggregate_function`/`window_function` reference an extension `(urn, function)` and resolve against the registry, and `if_then`/`switch`/`singular_or_list`/`multi_or_list` are conditional combinators.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                        | [CAPABILITY]                   |
| :-----: | :------------------------- | :------------------------------------------------------------------ | :----------------------------- |
|  [01]   | `type.<primitive>`         | `boolean/i8/i16/i32/i64/fp32/fp64/string/binary/date/uuid -> Type`  | primitive type node            |
|  [02]   | `type.decimal`             | `(scale: int, precision: int, nullable=True) -> Type`               | fixed-precision decimal        |
|  [03]   | `type.precision_timestamp` | `(precision: int, ...) -> Type` (peers `_tz`/`precision_time`)      | temporal type with precision   |
|  [04]   | `type.interval_day`        | `(precision: int, ...) -> Type` (peers `interval_year`/`_compound`) | interval type                  |
|  [05]   | `type.fixed_char`          | `(length: int, ...) -> Type` (peers `var_char`/`fixed_binary`)      | length-parameterized type      |
|  [06]   | `type.list` / `type.map`   | `list(type, ...)` / `map(key, value, ...) -> Type`                  | nested container type          |
|  [07]   | `type.struct`              | `(types: Iterable[Type], nullable=True) -> Type`                    | anonymous struct type          |
|  [08]   | `type.named_struct`        | `(names: Iterable[str], struct: Type) -> NamedStruct`               | field-named schema             |
|  [09]   | `ee.column` / `ee.literal` | `column(field: str \| int, ...)` / `literal(value, type, ...)`      | leaf expression thunk          |
|  [10]   | `ee.cast`                  | `(input: <expr>, type: Type, alias=None)`                           | typed cast expression          |
|  [11]   | `ee.scalar_function`       | `(urn: str, function: str, expressions: Iterable[<expr>], ...)`     | extension scalar-function call |
|  [12]   | `ee.aggregate_function`    | `(urn, function, expressions, alias=None)`                          | extension aggregate call       |
|  [13]   | `ee.window_function`       | `(urn, function, expressions, partitions=[], alias=None)`           | extension window call          |
|  [14]   | `ee.if_then` / `ee.switch` | `if_then(ifs, _else, ...)` / `switch(match, ifs, _else)`            | conditional combinator         |

[ENTRYPOINT_SCOPE]: extension loading, type derivation, and text rendering

`simple_extension_utils` builds `substrait_extensions` model objects from parsed YAML/dict payloads — `build_simple_extensions` loads the whole document and the `build_*` peers cover each function/type/argument node. `derivation_expression.evaluate` computes a parameterized output type from a derivation string. `utils.display` renders a parsed plan to gate-diagnostic text. Each row drops its owning-module prefix — `simple_extension_utils.`, `derivation_expression.`, or `utils.display.` — named in this scope.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                   | [CAPABILITY]                          |
| :-----: | :--------------------------- | :------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `build_simple_extensions`    | `(d: dict) -> SimpleExtensions`                                | load a whole extension document       |
|  [02]   | `build_scalar_function`      | `(d: dict) -> ScalarFunction` (peers aggregate/window)         | build one function node               |
|  [03]   | `build_type_model`           | `(d: dict) -> TypeModel` (peers type_variation/arg/options)    | build a type/argument node            |
|  [04]   | `evaluate`                   | `(x: str, values: dict \| None = None)`                        | evaluate a type-derivation expression |
|  [05]   | `PlanPrinter.stringify_plan` | `(plan: Plan) -> str` (peers `print_plan`)                     | render a plan to text                 |
|  [06]   | `pretty_print_plan`          | `(plan, indent_size=2, show_metadata=False, use_colors=False)` | print a plan with knobs               |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One admission gate folds five checks over inbound bytes in cost order: `proto.Plan.ParseFromString` structural parse, a non-empty `relations`, a schema-skew probe (declarations present while `extension_urns` is empty), `ExtensionRegistry.lookup_urn` over every `extension_urns` entry (a `None` rejects), then `type_inference.infer_plan_schema` (a returned `NamedStruct` is valid, a raise is rejection). Skew precedes resolution because a retired-schema plan presents as an extension-free plan and otherwise passes an empty resolution loop vacuously; resolution precedes inference because an unresolvable extension space names its own reason where inference reports only that the shape is unknowable.
- `Plan` crosses as the wire artifact itself, never a re-encoded copy: `SerializeToString` and `ByteSize` re-emit and measure a plan this package MINTS, while a gated inbound plan hands its original bytes onward untouched so the producer's content key survives.
- `builders.plan`/`builders.type`/`builders.extended_expression` thunk `Callable[[ExtensionRegistry], Plan]`, building a plan lazily and binding functions against one registry; `read_named_table` and unary/binary combinators own the relation family.
- `PlanPrinter.stringify_plan` renders a rejected or admitted plan to gate-log text — a display path, never a parse path.
- Rejection answers a typed refusal row naming which of the four checks stopped, never an engine fault lifted onto the rail; admission returns the admitted `Plan`, and the consuming query owns any caller-required plan census.
- `protobuf` owns the wire codec beneath `ParseFromString`/`SerializeToString`; `datafusion` and the DuckDB substrait extension own plan production and execution; downstream owners consume an admitted `Plan` or its bytes.

[STACKING]:
- `datafusion`(`.api/datafusion.md`) / `duckdb`(`.api/duckdb.md`): `datafusion.substrait.Serde.serialize_bytes(sql, ctx)` and the BLOB DuckDB's `con.execute("CALL get_substrait(?)", [sql]).fetchone()[0]` returns emit the same wire `Plan`; the gate runs `ParseFromString` + `lookup_urn` + `infer_plan_schema` once, then hands admitted bytes to `datafusion.substrait.Consumer.from_substrait_plan` or DuckDB `con.execute("CALL from_substrait(?)", [buf])`, so one validator guards the artifact both engines exchange. DuckDB reaches substrait through the extension's SQL table functions alone, never a connection-bound method — `.api/duckdb-extensions.md` `[03]` is the shape.
- `dataframely`(`.api/dataframely.md`) / `pandera`(`.api/pandera.md`): the `NamedStruct` from `infer_plan_schema` names the plan's output fields, so a data contract binds the inferred schema pre-execution and rejects a plan whose inferred names or types violate it before materialization.
- within-lib: Persistence content-hashes a stored plan's `SerializeToString` bytes under the branch `xxhash` identity and re-runs the gate on re-ingest, so a persisted plan re-validates against the current `ExtensionRegistry` on replay rather than trusting storage.

[LOCAL_ADMISSION]:
- import `substrait` and its submodules at boundary scope only; the branch admits it as the sole typed Substrait `Plan` model, function resolver, and schema-inference gate.
