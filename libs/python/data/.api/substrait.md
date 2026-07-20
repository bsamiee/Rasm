# [PY_DATA_API_SUBSTRAIT]

`substrait` supplies the standalone typed Substrait plan IR for the plan-gate rail: a protobuf object model (`Plan`, `PlanRel`, `Rel` and its relation family, `Expression`, `Type`, `NamedStruct`) whose `Plan` message parses and serializes raw wire bytes, an `ExtensionRegistry` loading simple-extension YAML/dict definitions and resolving functions by URN and signature, a `type_inference` checker validating a plan by inferring its output `NamedStruct`, callable-threaded `builders` constructing relations/expressions/types under a registry, and a `PlanPrinter` rendering a plan to text. Package owner composes `Plan.ParseFromString`, `infer_plan_schema`, and `ExtensionRegistry.lookup_function` into the SUBSTRAIT_PLAN_GATE admitting or rejecting inbound Persistence plan bytes pre-execution, re-implementing neither the protobuf schema, the function-resolution rules, nor the schema-inference algebra. `datafusion` and the DuckDB substrait extension exchange the same wire `Plan`, so this owner gates that shared artifact rather than trusting either engine's internal parser.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `substrait`
- package: `substrait[extensions]`
- import: `substrait`
- owner: `data`
- rail: plan-gate
- version: `0.29.0`
- entry points: library use is import-only; no console script
- capability: typed Substrait `Plan` protobuf model with `ParseFromString`/`SerializeToString` byte ingress and egress, an `ExtensionRegistry` loading simple-extension YAML/dict definitions and resolving functions by URN and signature, a `type_inference` validator inferring plan/rel/expression schemas and raising on malformed structure, callable-threaded plan/type/expression `builders`, a `simple_extension_utils` dict loader, a `derivation_expression` evaluator, and a `PlanPrinter` renderer

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plan model, registry, and renderer roots
- rail: plan-gate

`Plan` is the wire root the gate parses and validates; `PlanRel`/`Rel`/`RelRoot` are its relational-algebra stages, `Expression`/`ExtendedExpression` the scalar layer, and `Type`/`NamedStruct` the schema layer — all protobuf messages under `substrait.proto`. `ExtensionRegistry` is the function-resolution and extension-loading root; `FunctionEntry` is a resolved-function record and `FunctionType` its kind. `PlanPrinter` renders a parsed plan to text.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]     | [RAIL]                                                                |
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
|  [20]   | `proto.SimpleExtensionURN`             | extension         | extension-space URN declaration                                       |
|  [21]   | `proto.SimpleExtensionDeclaration`     | extension         | function/type extension binding                                       |
|  [22]   | `proto.AdvancedExtension`              | extension         | optimization/enhancement extension payload                            |
|  [23]   | `extension_registry.ExtensionRegistry` | registry          | extension-YAML/dict loader and function resolver                      |
|  [24]   | `extension_registry.FunctionEntry`     | record            | resolved-function definition record                                   |
|  [25]   | `extension_registry.FunctionType`      | enum              | scalar/aggregate/window function kind                                 |
|  [26]   | `utils.display.PlanPrinter`            | renderer          | plan/expression text renderer                                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Plan` byte ingress, egress, and inspection
- rail: plan-gate

`proto.Plan` is a protobuf message; the gate parses untrusted bytes with `ParseFromString` (or the `FromString` classmethod), re-emits canonical bytes with `SerializeToString`, and inspects presence with `HasField`/`WhichOneof`. Every surface below is a `Plan` method; `relations`, `version`, `extension_urns`, `extensions`, and `advanced_extensions` are the message fields the gate reads.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                        | [CAPABILITY]                                |
| :-----: | :------------------ | :---------------------------------- | :------------------------------------------ |
|  [01]   | `ParseFromString`   | `(serialized: bytes) -> int`        | parse untrusted wire bytes into the message |
|  [02]   | `FromString`        | `(serialized: bytes) -> Plan` (cls) | classmethod parse to a fresh `Plan`         |
|  [03]   | `MergeFromString`   | `(serialized: bytes) -> int`        | merge additional wire bytes                 |
|  [04]   | `SerializeToString` | `() -> bytes`                       | canonical protobuf byte egress              |
|  [05]   | `ByteSize`          | `() -> int`                         | wire byte length for the gate receipt       |
|  [06]   | `HasField`          | `(field_name: str) -> bool`         | presence probe (peer `WhichOneof(oneof)`)   |
|  [07]   | `IsInitialized`     | `() -> bool`                        | required-field completeness check           |

[ENTRYPOINT_SCOPE]: `type_inference` schema validation
- rail: plan-gate

Schema inference is the structural validator: `infer_plan_schema` walks the plan's relation tree and returns the output `NamedStruct`, raising on a malformed or type-inconsistent relation, so the gate calls it to reject a plan whose algebra does not type-check. Peers infer at each altitude.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                         | [CAPABILITY]                            |
| :-----: | :--------------------------------- | :--------------------------------------------------- | :-------------------------------------- |
|  [01]   | `infer_plan_schema`                | `(plan: Plan) -> NamedStruct`                        | validate a plan by inferring its schema |
|  [02]   | `infer_rel_schema`                 | `(rel: Rel) -> Type.Struct`                          | infer one relation's output struct      |
|  [03]   | `infer_expression_type`            | `(expression, parent_schema: Type.Struct) -> Type`   | infer a scalar expression's type        |
|  [04]   | `infer_literal_type`               | `(literal: Expression.Literal) -> Type`              | infer a literal's type                  |
|  [05]   | `infer_nested_type`                | `(nested: Expression.Nested, parent_schema) -> Type` | infer a nested (list/map/struct) type   |
|  [06]   | `infer_extended_expression_schema` | `(ee: ExtendedExpression) -> Type.Struct`            | infer an extended-expression schema     |

[ENTRYPOINT_SCOPE]: `ExtensionRegistry` function resolution and extension loading
- rail: plan-gate

`ExtensionRegistry(load_default_extensions=True)` loads the bundled standard simple-extension YAMLs at construction; `register_extension_yaml`/`register_extension_dict` add custom definitions. `lookup_function` resolves a `(urn, function_name, signature)` triple to a `(FunctionEntry, Type)` pair or `None`, so the gate rejects a plan referencing an unresolved function; `list_functions`/`list_functions_across_urns` enumerate candidate overloads and `lookup_urn` maps a URN to its anchor.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                   | [CAPABILITY]                           |
| :-----: | :--------------------------- | :------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `ExtensionRegistry`          | `(load_default_extensions=True)`                               | construct with bundled extensions      |
|  [02]   | `register_extension_yaml`    | `(fname: str \| pathlib.Path) -> None`                         | load a custom simple-extension YAML    |
|  [03]   | `register_extension_dict`    | `(definitions: dict) -> None`                                  | load extension definitions from a dict |
|  [04]   | `lookup_function`            | `(urn, name, sig: Seq[Type]) -> (FunctionEntry, Type) \| None` | resolve one function overload          |
|  [05]   | `list_functions`             | `(urn, name, sig) -> list[(FunctionEntry, Type)]`              | enumerate overloads within a URN       |
|  [06]   | `list_functions_across_urns` | `(name, sig) -> list[(FunctionEntry, Type)]`                   | enumerate overloads across URNs        |
|  [07]   | `lookup_urn`                 | `(urn: str) -> int \| None`                                    | map a URN to its extension anchor      |

[ENTRYPOINT_SCOPE]: `builders.plan` relation construction
- rail: builder

Every `builders.plan` surface returns a `Callable[[ExtensionRegistry], Plan]` — a plan thunk resolved by applying a registry — so leaf builders and unary/binary combinators compose lazily and bind functions against the registry once. A relation input is a `Plan` or another thunk; an expression argument is an `ExtendedExpression` or a `Callable[[NamedStruct, ExtensionRegistry], ExtendedExpression]`. `read_named_table` is the leaf source; `write_named_table` the sink; the combinators mirror the `proto` relation family.

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
- rail: builder

`builders.type` mints a `proto.Type` per primitive/parametric kind, each `(..., nullable=True)`; `named_struct(names, struct)` pairs field names with a non-nullable `struct`. `builders.extended_expression` mints expression thunks: `column`/`literal`/`cast` are leaves, `scalar_function`/`aggregate_function`/`window_function` reference an extension `(urn, function)` and resolve against the registry, and `if_then`/`switch`/`singular_or_list`/`multi_or_list` are conditional combinators.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                        | [CAPABILITY]                   |
| :-----: | :------------------------- | :------------------------------------------------------------------ | :----------------------------- |
|  [01]   | `type.<primitive>`         | `bool/i8/i16/i32/i64/fp32/fp64/string/binary/date/uuid -> Type`     | primitive type node            |
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
- rail: interchange

`simple_extension_utils` builds `substrait_extensions` model objects from parsed YAML/dict payloads; `build_simple_extensions` is the whole-document loader and the `build_*` peers cover each function/type/argument node. `derivation_expression.evaluate` computes a parameterized output type from a derivation string. `utils.display` renders a parsed plan to text for gate diagnostics — `PlanPrinter().stringify_plan(plan)` returns a string, `print_plan` writes to stdout, and the `pretty_print_*` module functions carry indent/metadata/color knobs.

Each row drops its owning-module prefix — `simple_extension_utils.`, `derivation_expression.`, or `utils.display.` — named in the scope above.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                   | [CAPABILITY]                          |
| :-----: | :--------------------------- | :------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `build_simple_extensions`    | `(d: dict) -> SimpleExtensions`                                | load a whole extension document       |
|  [02]   | `build_scalar_function`      | `(d: dict) -> ScalarFunction` (peers aggregate/window)         | build one function node               |
|  [03]   | `build_type_model`           | `(d: dict) -> TypeModel` (peers type_variation/arg/options)    | build a type/argument node            |
|  [04]   | `evaluate`                   | `(x: str, values: dict \| None = None)`                        | evaluate a type-derivation expression |
|  [05]   | `PlanPrinter.stringify_plan` | `(plan: Plan) -> str` (peers `print_plan`)                     | render a plan to text                 |
|  [06]   | `pretty_print_plan`          | `(plan, indent_size=2, show_metadata=False, use_colors=False)` | print a plan with knobs               |

## [04]-[IMPLEMENTATION_LAW]

[PLAN_GATE]:
- import: `import substrait` and its submodules at boundary scope only; module-level import is banned by the manifest import policy.
- ingress axis: untrusted plan bytes enter through `proto.Plan().ParseFromString(buf)` or `proto.Plan.FromString(buf)`; the wire parse is the first structural gate, and a decode failure rejects before any inference; never hand-parse Substrait protobuf.
- validation axis: `type_inference.infer_plan_schema(plan)` is the structural validator — it type-checks the relation tree and raises on a malformed or inconsistent plan; a plan that returns a `NamedStruct` is schema-valid, a raise is a gate rejection.
- resolution axis: `ExtensionRegistry(load_default_extensions=True)` with `register_extension_yaml`/`register_extension_dict` loads the admitted function space; `lookup_function(urn, name, signature)` returning `None` rejects a plan referencing an unknown function, so the gate admits only plans over resolved extensions.
- egress axis: `Plan.SerializeToString()` re-emits canonical bytes after admission and `ByteSize()` stamps the gate receipt; the admitted artifact is the wire `Plan`, never a re-encoded copy.
- construction axis: `builders.plan`/`builders.type`/`builders.extended_expression` thunks `Callable[[ExtensionRegistry], Plan]` build a plan lazily and bind functions against one registry; a leaf `read_named_table` and unary/binary combinators own the relation family, never a hand-built protobuf tree.
- diagnostics axis: `PlanPrinter().stringify_plan(plan)` renders a rejected or admitted plan to text for the gate log; text rendering is a display row, never a parse path.
- evidence: each gate decision captures plan byte length, relation count, inferred output field names, the resolved function URNs, and the admit/reject verdict as a plan-gate receipt.
- boundary: `substrait` owns the typed `Plan` model, function resolution, and schema inference; `protobuf` owns the wire codec beneath `ParseFromString`/`SerializeToString`; `datafusion` and the DuckDB substrait extension own plan production and execution; downstream owners consume an admitted `Plan` or its bytes, never the engine-internal plan handles.

[INTEGRATION_STACKING]:
- gate-before-engine spine: `datafusion.substrait.Serde.serialize_bytes(sql, ctx)` and the DuckDB `con.get_substrait(sql)` BLOB emit the same wire `Plan`; the data owner runs `ParseFromString` + `infer_plan_schema` + `ExtensionRegistry.lookup_function` as one admission gate, then hands admitted bytes to `datafusion.substrait.Consumer.from_substrait_plan` or DuckDB `con.from_substrait(buf)`, so one validator guards the artifact both engines exchange.
- schema-contract seam: an inferred `NamedStruct` from `infer_plan_schema` names the plan's output fields, so a `dataframely`/`pandera` data contract binds against the gate's inferred schema before execution rather than after materialization; a plan whose inferred names or types violate the contract is rejected pre-execution.
- persistence federation seam: Persistence federates content-keyed frames and Substrait plans; a stored plan's `SerializeToString` bytes are content-hashed with the branch `xxhash` identity, and re-ingest re-runs the same gate, so a persisted plan is re-validated against the current `ExtensionRegistry` on replay rather than trusted from storage.

[RAIL_LAW]:
- Package: `substrait[extensions]`
- Owns: the typed Substrait `Plan` protobuf model with byte ingress/egress, an `ExtensionRegistry` loading simple-extension YAML/dict definitions and resolving functions by URN and signature, `type_inference` schema validation over plans/relations/expressions, callable-threaded plan/type/expression `builders`, a `simple_extension_utils` dict loader, a `derivation_expression` evaluator, and a `PlanPrinter` renderer
- Accept: gating inbound plan bytes pre-execution — `ParseFromString` structural parse, `infer_plan_schema` type-check, and `ExtensionRegistry.lookup_function` reference resolution — and round-tripping the same wire `Plan` with `datafusion.substrait` and the DuckDB substrait extension for cross-engine interchange
- Reject: wrapper-renames of `Plan`/`ExtensionRegistry`/`infer_plan_schema`; a hand-rolled Substrait protobuf parser, function-resolution table, or schema-inference walk; trusting a plan from an engine's internal parser where the standalone gate validates the wire artifact; a per-engine validation boundary where one `Plan` gate guards both engines; raw `substrait_antlr`/`_internal` handles crossing the package boundary
