# [RASM_PERSISTENCE_API_JSONSCHEMA_NET]

`JsonSchema.Net` owns in-process JSON Schema evaluation on the `validation` rail: schema parse, compiled-node build, dialect and vocabulary resolution, `$ref`/`$dynamicRef` document resolution, format assertion, and the three-tier output model. It computes application-side the boolean verdict a server-side `CHECK` constraint enforces at write, so one frozen schema text governs both residences. Schema is data, evaluation is a pure fold over `System.Text.Json`, and the verdict is a receipt; the four process-global registries are the only ambient state.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `JsonSchema.Net`
- package: `JsonSchema.Net` (MIT source under an Open Source Maintenance Fee EULA on the binary, Greg Dennis)
- assembly: `JsonSchema.Net`
- namespace: `Json.Schema`, `Json.Schema.Keywords`, `Json.Schema.Serialization`
- depends: `JsonPointer.Net` — the `JsonPointer` model `EvaluationPath`, `InstanceLocation`, and `FindSubschema` carry
- asset: runtime library
- rail: validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parse product, compiled tree, policy carriers, and the verdict

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `JsonSchema`              | class         | parse-once immutable value; `$ref`-resolvable document    |
|  [02]   | `JsonSchemaNode`          | class         | compiled node tree behind `JsonSchema.Root`               |
|  [03]   | `JsonSchemaBuilder`       | class         | code-first composition, implicit to `JsonSchema`          |
|  [04]   | `SchemaOrPropertyList`    | class         | a `dependencies` value — subschema or required-name list  |
|  [05]   | `BuildOptions`            | class         | parse-time registry set and dialect pin                   |
|  [06]   | `EvaluationOptions`       | class         | output tier, format assertion, culture, annotation policy |
|  [07]   | `EvaluationResults`       | class         | recursive per-keyword verdict tree                        |
|  [08]   | `OutputFormat`            | enum          | `Flag`, `List`, `Hierarchical` verdict shape              |
|  [09]   | `SchemaValueType`         | enum          | JSON type flags the `type` keyword resolves               |
|  [10]   | `IBaseDocument`           | interface     | `$ref`/`$dynamicRef`-resolvable document contract         |
|  [11]   | `JsonElementBaseDocument` | class         | adopts a raw `JsonElement` as a resolvable document       |
|  [12]   | `JsonSchemaException`     | class         | schema JSON is not itself a valid schema                  |
|  [13]   | `RefResolutionException`  | class         | unresolved reference; carries `BaseUri`, `Anchor`         |
|  [14]   | `AnchorType`              | enum          | `Static`, `Dynamic`, `Recursive` anchor kind              |

[PUBLIC_TYPE_SCOPE]: draft behavior, vocabulary bundles, meta-schemas, and the process-global resolvers

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `Dialect`            | class         | draft keyword-handler set; pins version semantics          |
|  [02]   | `DialectRegistry`    | class         | `$schema` URI to dialect resolution                        |
|  [03]   | `Vocabulary`         | class         | keyword bundle a meta-schema declares                      |
|  [04]   | `VocabularyRegistry` | class         | `$vocabulary` URI to bundle resolution                     |
|  [05]   | `MetaSchemas`        | class         | built-in meta-schema documents and their ids               |
|  [06]   | `SchemaRegistry`     | class         | `$ref` document registration, lookup, fetch hook, bundling |
|  [07]   | `DynamicScope`       | class         | the `$dynamicRef` scope an evaluation threads              |

[DIALECT_DRAFTS]: `Dialect.Draft06` `Dialect.Draft07` `Dialect.Draft201909` `Dialect.Draft202012` `Dialect.V1_2026` `Dialect.V1` `Dialect.Default`

`MetaSchemas` carries the meta-schema document and its `Uri` id for every draft and every declared vocabulary, each pre-registered on `SchemaRegistry.Global`, so a stock draft schema needs no registration. `Vocabulary` carries one static bundle per vocabulary the 2019-09 and 2020-12 meta-schemas declare.

[PUBLIC_TYPE_SCOPE]: format validators, custom-keyword authoring, and the serialization gate

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :---------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `Format`                            | class         | one named-format validator                             |
|  [02]   | `PredicateFormat`                   | class         | delegate-backed format, message-carrying overload      |
|  [03]   | `RegexFormat`                       | class         | pattern-backed format                                  |
|  [04]   | `UnknownFormat`                     | class         | unrecognized format key leaf                           |
|  [05]   | `FormatRegistry`                    | class         | named-format resolution                                |
|  [06]   | `Formats`                           | class         | built-in format instances and their registration       |
|  [07]   | `Duration`                          | struct        | ISO-8601 duration behind the `duration` format         |
|  [08]   | `ErrorMessages`                     | class         | per-keyword message templates and the culture override |
|  [09]   | `IKeywordHandler`                   | interface     | custom keyword — name, value check, build, evaluate    |
|  [10]   | `KeywordData`                       | class         | one parsed keyword — handler, raw value, subschemas    |
|  [11]   | `KeywordEvaluation`                 | struct        | one keyword verdict — validity, annotation, error      |
|  [12]   | `BuildContext`                      | struct        | build-time dialect, base URI, and pointer frame        |
|  [13]   | `EvaluationContext`                 | struct        | evaluate-time options, scope, instance, pointer frame  |
|  [14]   | `DependsOnAnnotationsFromAttribute` | class         | declares a handler's annotation dependency             |
|  [15]   | `ValidatingJsonConverter`           | class         | schema-gated `System.Text.Json` deserialization        |
|  [16]   | `JsonSchemaAttribute`               | class         | binds a POCO to a static `JsonSchema` member           |
|  [17]   | `JsonSchemaJsonConverter`           | class         | `JsonSchema` read and write over `System.Text.Json`    |
|  [18]   | `EvaluationResultsJsonConverter`    | class         | verdict read and write over `System.Text.Json`         |
|  [19]   | `JsonElementExtensions`             | class         | `JsonElement` to `SchemaValueType` projection          |
|  [20]   | `JsonMath`                          | class         | JSON-number compare, integrality, divisibility         |

[FORMAT_KEYS]: `date` `date-time` `duration` `email` `hostname` `idn-email` `idn-hostname` `ipv4` `ipv6` `iri` `iri-reference` `json-pointer` `regex` `relative-json-pointer` `time` `uri` `uri-reference` `uri-template` `uuid`

`ErrorMessages` carries a settable template and a `Get<Keyword>` reader per assertion keyword under a `Culture` override, `ReplaceToken` substituting a named token into a template.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse or compose a schema — `JsonSchemaBuilderExtensions` adds one fluent method per JSON Schema keyword over `JsonSchemaBuilder`, `Unrecognized` covering a keyword no draft declares

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `JsonSchema.FromText(string, BuildOptions?, Uri?, JsonDocumentOptions?)` | factory  | parse schema JSON text                 |
|  [02]   | `JsonSchema.FromFile(string, BuildOptions?, Uri?)`                       | factory  | parse a schema file                    |
|  [03]   | `JsonSchema.Build(JsonElement, BuildOptions?, Uri?)`                     | factory  | build from an already-parsed element   |
|  [04]   | `JsonSchema.BuildNode(BuildContext) -> JsonSchemaNode`                   | static   | build a subschema inside a handler     |
|  [05]   | `JsonSchemaBuilder.Add(string, JsonNode?)`                               | instance | write any keyword value raw            |
|  [06]   | `JsonSchemaBuilder.Add(string, JsonSchemaBuilder)`                       | instance | write a subschema keyword              |
|  [07]   | `JsonSchemaBuilder.Add(string, IEnumerable<JsonSchemaBuilder>)`          | instance | write a schema-array keyword           |
|  [08]   | `JsonSchemaBuilder.Build(BuildOptions?, Uri?) -> JsonSchema`             | instance | seal the composed schema               |
|  [09]   | `JsonSchemaBuilder.RefRoot()`                                            | static   | a `$ref` to the document root          |
|  [10]   | `JsonSchemaBuilder.RecursiveRefRoot()`                                   | static   | a `$recursiveRef` to the document root |

- `JsonSchemaBuilder.Add`: `IEnumerable<KeyValuePair<string, JsonSchemaBuilder>>` and `IEnumerable<(string, JsonSchemaBuilder)>` overloads write a keyed-subschema keyword.
- `JsonSchema`: a `bool` converts implicitly to the pass-all or fail-all schema, and a `JsonSchemaBuilder` converts implicitly to `JsonSchema`.

[ENTRYPOINT_SCOPE]: evaluate an instance and read the verdict

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `JsonSchema.Evaluate(JsonElement, EvaluationOptions?)`            | instance | fold one instance to the verdict tree |
|  [02]   | `JsonSchema.FindSubschema(JsonPointer, BuildContext)`             | instance | resolve a pointer to a compiled node  |
|  [03]   | `JsonSchemaNode.Evaluate(EvaluationContext) -> EvaluationResults` | instance | evaluate one compiled node            |
|  [04]   | `EvaluationResults.IsValid`                                       | property | boolean verdict, the `CHECK` parity   |
|  [05]   | `EvaluationResults.Details`                                       | property | nested per-keyword verdicts           |
|  [06]   | `EvaluationResults.Errors`                                        | property | keyword to message map                |
|  [07]   | `EvaluationResults.Annotations`                                   | property | keyword to `JsonElement` map          |
|  [08]   | `EvaluationResults.EvaluationPath`                                | property | pointer into the schema               |
|  [09]   | `EvaluationResults.InstanceLocation`                              | property | pointer into the instance             |
|  [10]   | `EvaluationResults.SchemaLocation`                                | property | absolute schema URI                   |
|  [11]   | `EvaluationResults.Parent`                                        | property | owning node in the verdict tree       |
|  [12]   | `EvaluationResults.ToList()`                                      | instance | flatten to the `List` shape           |
|  [13]   | `EvaluationResults.ToFlag()`                                      | instance | collapse to the `Flag` shape          |

- `EvaluationResults.ToList` / `ToFlag`: both return `void`, reshaping the receipt in place rather than minting a second one.

[ENTRYPOINT_SCOPE]: policy, dialect derivation, registry admission, and extension — each registry exposes a process-global `Global` instance that `Register`/`Unregister` mutate and every parse and evaluation reads

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `EvaluationOptions.OutputFormat`                                      | property | select the verdict tier                |
|  [02]   | `EvaluationOptions.RequireFormatValidation`                           | property | promote `format` to assertion          |
|  [03]   | `EvaluationOptions.Culture`                                           | property | error-message culture                  |
|  [04]   | `EvaluationOptions.From(EvaluationOptions)`                           | factory  | copy an evaluation policy              |
|  [05]   | `BuildOptions.Dialect`                                                | property | pin the draft when `$schema` is absent |
|  [06]   | `Dialect(IEnumerable<IKeywordHandler>)`                               | ctor     | compose a dialect from handlers        |
|  [07]   | `Dialect.With(IEnumerable<IKeywordHandler>, Uri?, bool?, bool?)`      | static   | derive a dialect adding handlers       |
|  [08]   | `Dialect.Without(IEnumerable<IKeywordHandler>, Uri?, bool?, bool?)`   | static   | derive a dialect dropping handlers     |
|  [09]   | `Vocabulary(Uri, IEnumerable<IKeywordHandler>)`                       | ctor     | mint a keyword bundle                  |
|  [10]   | `SchemaRegistry.Register(IBaseDocument)`                              | instance | admit a `$ref`-resolvable document     |
|  [11]   | `SchemaRegistry.Register(Uri?, IBaseDocument)`                        | instance | admit under an explicit URI            |
|  [12]   | `SchemaRegistry.Get(Uri) -> IBaseDocument?`                           | instance | resolve a registered document          |
|  [13]   | `SchemaRegistry.Fetch`                                                | property | lazy external-document resolution hook |
|  [14]   | `SchemaRegistry.CreateBundle(Uri, Uri, BuildOptions?) -> JsonSchema?` | instance | fold a `$ref` graph into one document  |
|  [15]   | `FormatRegistry.Get(string) -> Format`                                | instance | resolve a named format                 |
|  [16]   | `Formats.CreateUnknown(string) -> Format`                             | static   | mint a pass-through format leaf        |
|  [17]   | `PredicateFormat(string, Func<JsonElement, bool>)`                    | ctor     | delegate-backed custom format          |
|  [18]   | `PredicateFormat(string, PredicateWithErrorMessage)`                  | ctor     | custom format carrying its message     |
|  [19]   | `RegexFormat(string, string)`                                         | ctor     | pattern-backed custom format           |
|  [20]   | `ErrorMessages.Get(CultureInfo?, string?) -> string`                  | static   | resolve a keyword message template     |
|  [21]   | `ValidatingJsonConverter.MapType<T>(JsonSchema)`                      | static   | bind a schema to a CLR type            |
|  [22]   | `ValidatingJsonConverter.RegisterConverter(Type, JsonConverter)`      | static   | register the gated converter           |
|  [23]   | `JsonElementBaseDocument(JsonElement, Uri)`                           | ctor     | adopt a raw element as a `$ref` target |

- `SchemaRegistry.Fetch`: a `Func<Uri, SchemaRegistry, IBaseDocument?>` the registry invokes on a miss.
- `UnknownFormat.Validate`: returns `true`, so an unregistered `format` key passes assertion instead of failing it.

[ANNOTATION_POLICY]: `EvaluationOptions.IgnoreAnnotationsFrom<T>()` `CollectAnnotationsFrom<T>()` `IgnoreAllAnnotations()` `ClearIgnoredAnnotations()` `IgnoredAnnotations` `PreserveDroppedAnnotations` `AddAnnotationForUnknownKeywords`

[KEYWORD_AUTHORING]: `JsonElementExtensions.GetSchemaValueType` `JsonMath.NumberCompare` `JsonMath.IsInteger` `JsonMath.Divides` `Duration.Parse` `Duration.TryParse`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `JsonSchema` is a parse-once immutable value: the frozen schema text parses at composition, the compiled tree behind `Root` is thread-safe for concurrent `Evaluate`, and parse cost never repeats.
- `Evaluate` folds a `JsonElement` instance to an `EvaluationResults` tree whose `IsValid` is the boolean the server-side check returns, so both residences yield one verdict from one schema text.
- Draft selection is a `Dialect`: a schema's `$schema` keyword resolves one through `DialectRegistry`, and `BuildOptions.Dialect` pins the draft when `$schema` is absent, so a version-less schema binds a stated draft.
- `EvaluationOptions.OutputFormat` selects the verdict tier inside one evaluation, and `ToList`/`ToFlag` reshape a standing receipt without a second pass.
- `Evaluate` answers a schema-invalid instance with a verdict, so throwing marks a schema fault alone: `JsonSchema.FromText` lifts `JsonSchemaException` on invalid schema JSON and evaluation lifts `RefResolutionException` on an unresolvable reference, both discriminated from `IsValid == false` on the `validation` fault rail.

[STACKING]:
- `System.Text.Json`(`.api/api-json-schema.md`): `JsonSchemaExporter.GetJsonSchemaAsNode(JsonTypeInfo, JsonSchemaExporterOptions)` projects a schema from the live serializer contract, and that `JsonNode` crosses to `JsonSchema.Build(JsonElement, BuildOptions?, Uri?)` through `JsonSerializer.SerializeToElement`, so a contract-derived schema and the bytes it describes cannot drift.
- `pg_jsonschema`(`.api/api-pg-server-bgworkers.md`): `jsonb_matches_schema` enforces the verdict inside a column `CHECK` at write; when that `ServerExtension` row folds out, `JsonSchema.Evaluate(JsonElement, EvaluationOptions?)` reading `IsValid` computes the identical verdict in process, and `OutputFormat.List` with `Details`/`Errors` projects the per-keyword failure a `CHECK` cannot surface — the in-process path carries strictly more diagnostic capability than the extension it substitutes.
- `JsonPointer.Net`: `EvaluationPath` and `InstanceLocation` are `JsonPointer` values, so a failure detail addresses schema and instance positions the receipt carries verbatim, and `FindSubschema(JsonPointer, BuildContext)` reads back the compiled node a pointer names.
- within-lib: store provisioning parses the frozen text once into its schema contract and threads that `JsonSchema` through every candidate, `SchemaRegistry.CreateBundle` collapsing a multi-document `$ref` graph into one self-contained schema before the freeze.

[LOCAL_ADMISSION]:
- `JsonSchema.Net` is the sole in-process JSON Schema evaluator on the `validation` rail, computing application-side the verdict the server extension computes at write.
- Schema text freezes at composition and parses once; the resulting `JsonSchema` is the value every evaluation binds.
- `SchemaRegistry`, `VocabularyRegistry`, `DialectRegistry`, and `FormatRegistry` are process-global: external `$ref` documents, vocabularies, dialects, and custom formats register once at startup.
- A `PredicateFormat` on `FormatRegistry.Global` asserts an identity shape no draft keyword expresses, and `IKeywordHandler` with `Dialect.With(...)` admits a lane-local keyword without forking the evaluator.
- `ValidatingJsonConverter.MapType<T>` gates typed-document ingress against the same schema the raw column checks.

[RAIL_LAW]:
- Package: `JsonSchema.Net`
- Owns: in-process JSON Schema evaluation — parse, compiled-node build, dialect/vocabulary/meta-schema resolution, `$ref` document resolution, format assertion, and the three output tiers
- Accept: parse-once-evaluate-many over the frozen document-lane schema, `IsValid` for the verdict and `Details`/`Errors` for the receipt rail, custom `Format`/`IKeywordHandler`/`Dialect` derivations registered at composition, `ValidatingJsonConverter` for typed-document ingress
- Reject: a per-instance re-parse of the schema, a second validator path beside this one, and a hand-rolled keyword check where a `Dialect` derivation carries it
