# [RASM_PERSISTENCE_API_JSONSCHEMA_NET]

`JsonSchema.Net` supplies the in-process JSON Schema evaluator — schema parse, compiled-node build,
draft/dialect selection, vocabulary and meta-schema handling, `$ref`/`$dynamicRef` document
resolution, format assertion, and the three-tier output model — for the `validation` rail. It is the
application-side counterpart to the `pg_jsonschema` server extension: the SAME `json_matches_schema`/
`jsonb_matches_schema` boolean verdict a `CHECK` constraint enforces at write, computed in-process
when the operator-provisioned cluster lacks the pgrx-compiled extension. The pre-frozen document-lane
schema parses once to a `JsonSchema` and evaluates each candidate `JsonElement` to an
`EvaluationResults`, reading `IsValid` for the boolean verdict and `Details`/`Errors` for the
per-keyword failure rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `JsonSchema.Net`
- package: `JsonSchema.Net`
- license: MIT (the OSI license) with an Open Source Maintenance Fee EULA on the binary release — a monthly maintenance fee applies to revenue-generating users at or above US$10,000 annual gross revenue (this CORRECTS the design-page `Apache-2.0` assumption; the source is MIT, the binary carries the OSMF EULA)
- assembly: `JsonSchema.Net`
- namespace: `Json.Schema`, `Json.Schema.Serialization`
- target framework: `net9.0` asset binds on the `net10.0` floor (package ships `net9.0`/`net8.0`/`netstandard2.0`; the nuspec declares a `net10.0` dependency group over the `net9.0` lib)
- transitive: `JsonPointer.Net@7.0.1` (the `JsonPointer`/`JsonPath` model the `EvaluationPath`/`InstanceLocation`/`FindSubschema` surfaces carry) and its `Json.More.Net` core
- asset: runtime library
- rail: validation

This is a pure-managed evaluator over `System.Text.Json` — it consumes and produces `JsonElement`/
`JsonNode`, never a store connection or wire client. The schema is data (`JsonSchema`), evaluation is
a pure fold (`Evaluate`), and the verdict is a receipt (`EvaluationResults`) — no ambient state beyond
the process-global `SchemaRegistry`/`VocabularyRegistry`/`DialectRegistry`/`FormatRegistry`.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schema, evaluation, and results
- rail: validation

`JsonSchema : IBaseDocument` is the parsed, compiled, value-equatable schema (its `Root` is a
`JsonSchemaNode`); it doubles as a `$ref`-resolvable base document. `EvaluationOptions` carries the
evaluate-time policy; `EvaluationResults` is the recursive verdict tree.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]        | [CAPABILITY]                                                                    |
| :-----: | :----------------------- | :------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `JsonSchema`             | compiled schema      | parse-once immutable value; `$ref`-resolvable base document                     |
|  [02]   | `JsonSchemaNode`         | compiled node        | the built node tree `JsonSchema.Root` exposes; `FindSubschema` target           |
|  [03]   | `JsonSchemaBuilder`      | fluent builder       | composes a schema in-code without JSON text                                     |
|  [04]   | `BuildOptions`           | parse-time policy    | the parse-time registry set and dialect pin                                     |
|  [05]   | `EvaluationOptions`      | evaluate-time policy | output tier, format assertion, culture, annotation knobs                        |
|  [06]   | `EvaluationResults`      | verdict tree         | recursive per-keyword verdict; boolean, location, and detail rails              |
|  [07]   | `OutputFormat`           | output tier enum     | `Flag` (boolean only), `List` (flat failure list), `Hierarchical` (nested tree) |
|  [08]   | `IBaseDocument`          | document contract    | the `$ref`/`$dynamicRef`-resolvable schema-document seam                        |
|  [09]   | `JsonSchemaException`    | build failure        | thrown when schema JSON is not a valid schema                                   |
|  [10]   | `RefResolutionException` | resolution failure   | thrown when a `$ref`/`$dynamicRef` cannot resolve against the registry          |

Member surface per type:
- [01]-`JsonSchema`: `FromText`/`FromFile`/`Build` factories, `Evaluate`, `Root`, `BaseUri`, `BoolValue`, static `True`/`False`, implicit from `bool`.
- [03]-`JsonSchemaBuilder`: `Add(keyword, …)`, `RefRoot`, `Build`; static `Empty`/`True`/`False`.
- [04]-`BuildOptions`: `Default`, `SchemaRegistry`, `VocabularyRegistry`, `DialectRegistry`, `Dialect`.
- [05]-`EvaluationOptions`: `Default`, `OutputFormat`, `RequireFormatValidation`, `FormatRegistry`, `Culture`, annotation-collection knobs.
- [06]-`EvaluationResults`: `IsValid`, `EvaluationPath`, `InstanceLocation`, `SchemaLocation`, `Details`, `Annotations`, `Errors`, `Parent`, `ToList`/`ToFlag`.

[PUBLIC_TYPE_SCOPE]: dialect, vocabulary, meta-schema, and format registries
- rail: validation

Draft/version behavior is a `Dialect` (a keyword-handler set); a `Vocabulary` is a keyword bundle a
meta-schema declares; `MetaSchemas` holds the built-in draft meta-schemas; the four process-global
registries own dialect/vocabulary/document/format resolution. `ValidatingJsonConverter` lives in the
`Json.Schema.Serialization` namespace.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]       | [CAPABILITY]                                               |
| :-----: | :-------------------------- | :------------------ | :--------------------------------------------------------- |
|  [01]   | `Dialect`                   | draft behavior      | the draft keyword-handler set; pins version semantics      |
|  [02]   | `DialectRegistry`           | dialect resolver    | `Global`; `Register(Dialect)`/`Unregister(Uri)`            |
|  [03]   | `Vocabulary`                | keyword bundle      | a meta-schema's keyword bundle; static per-draft sets      |
|  [04]   | `VocabularyRegistry`        | vocabulary resolver | `Global`; `Register(Vocabulary)`/`Unregister(Uri)`         |
|  [05]   | `MetaSchemas`               | built-in schemas    | the pre-registered built-in draft meta-schema documents    |
|  [06]   | `SchemaRegistry`            | document resolver   | `$ref` document registration, lookup, fetch hook, bundling |
|  [07]   | `Format` / `FormatRegistry` | format assertion    | named-format validators and the global format registry     |
|  [08]   | `ValidatingJsonConverter`   | POCO gate           | schema-gated `System.Text.Json` deserialization            |

Member surface per type:
- [01]-`Dialect`: static `Draft06`/`Draft07`/`Draft201909`/`Draft202012`/`V1_2026` (`V1` alias); `Default`, `Id`, `RefIgnoresSiblingKeywords`, `AllowUnknownKeywords`.
- [03]-`Vocabulary`: static `Draft201909_*`/`Draft202012_*` (`Core`/`Validation`/`Applicator`/`Format*`/`Unevaluated`/…); `Id`, `Keywords`.
- [05]-`MetaSchemas`: `Draft6`/`Draft7`/`Draft201909`/`Draft202012` schemas + `*Id` `Uri` constants.
- [06]-`SchemaRegistry`: `Global`; `Register(IBaseDocument)`, `Get(Uri)`, `Fetch` (external `Func<Uri, …, IBaseDocument?>`), `CreateBundle`.
- [07]-`Format` / `FormatRegistry`: named-format validators; `FormatRegistry.Global`, `Register`/`Get`/`Unregister`; `PredicateFormat`/`RegexFormat`/`UnknownFormat` leaves.
- [08]-`ValidatingJsonConverter`: a `JsonConverterFactory` validating a `JsonElement` against a mapped `JsonSchema` during `System.Text.Json` deserialization (`MapType<T>`, `EvaluationOptions`).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse a schema
- rail: validation

The pre-frozen document-lane schema parses ONCE to a reusable `JsonSchema`; the parsed schema is
immutable and thread-safe for concurrent `Evaluate`.

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `JsonSchema.FromText(string, BuildOptions?, Uri?, JsonDocumentOptions?)` | static factory | parses schema JSON text to a `JsonSchema`   |
|  [02]   | `JsonSchema.FromFile(string, BuildOptions?, Uri?)`                       | static factory | parses a schema file                        |
|  [03]   | `JsonSchema.Build(JsonElement, BuildOptions?, Uri?)`                     | static factory | builds from an already-parsed `JsonElement` |
|  [04]   | `new JsonSchemaBuilder().Add(keyword, …).Build()`                        | fluent build   | composes a schema in-code without JSON text |

[ENTRYPOINT_SCOPE]: evaluate an instance
- rail: validation
- note: `Evaluate` takes the `System.Text.Json` `JsonElement`; a `JsonNode` is read via `JsonSerializer`/`RootElement` first

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `JsonSchema.Evaluate(JsonElement, EvaluationOptions?)` | evaluate       | folds one instance to `EvaluationResults`               |
|  [02]   | `EvaluationResults.IsValid`                            | verdict        | the boolean matching server-side `jsonb_matches_schema` |
|  [03]   | `EvaluationResults.Details`                            | failure rail   | per-keyword nested verdicts under `List`/`Hierarchical` |
|  [04]   | `EvaluationResults.Errors` / `Annotations`             | detail maps    | keyword→message errors and keyword→`JsonElement` maps   |
|  [05]   | `EvaluationResults.ToFlag()` / `ToList()`              | reshape        | collapses the tree to flag or flat-list shape in place  |

[ENTRYPOINT_SCOPE]: policy, dialect, and reference resolution
- rail: validation

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :--------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `EvaluationOptions.OutputFormat = OutputFormat.List` | output policy  | selects `Flag`/`List`/`Hierarchical` verdict shape           |
|  [02]   | `EvaluationOptions.RequireFormatValidation = true`   | format policy  | promotes `format` from annotation to assertion               |
|  [03]   | `BuildOptions.Dialect = Dialect.Draft202012`         | draft policy   | pins the draft when the schema omits `$schema`               |
|  [04]   | `SchemaRegistry.Global.Register(IBaseDocument)`      | document reg   | registers an external schema for `$ref` resolution           |
|  [05]   | `SchemaRegistry.Fetch = (uri, reg) => …`             | fetch hook     | lazy external-document resolution                            |
|  [06]   | `FormatRegistry.Global.Register(Format)`             | format reg     | registers a custom named format                              |
|  [07]   | `ValidatingJsonConverter.MapType<T>(JsonSchema)`     | POCO map       | binds a schema to a CLR type for deserialize-time validation |

## [04]-[IMPLEMENTATION_LAW]

[SCHEMA_TOPOLOGY]:
- `JsonSchema` is a parsed, compiled, immutable value — parse once from the pre-frozen document-lane schema string, cache the `JsonSchema`, and `Evaluate` per instance; the parse cost never repeats and the compiled node tree (`Root`) is thread-safe for concurrent evaluation.
- the instance is a `System.Text.Json` `JsonElement`; the verdict is `EvaluationResults` whose `IsValid` is the boolean the server-side `jsonb_matches_schema` returns, so the fallback yields a bit-identical verdict.
- draft selection is a `Dialect`: the schema's `$schema` keyword picks the built-in `Draft202012`/`Draft201909`/`Draft07`/`Draft06` (or the upcoming `V1_2026`) dialect; `BuildOptions.Dialect` pins the draft when `$schema` is absent, so a version-less schema never silently defaults.
- vocabularies and meta-schemas: a `$vocabulary`-declaring meta-schema composes `Vocabulary` keyword bundles registered on `VocabularyRegistry.Global`; the built-in `MetaSchemas.Draft202012`/… are pre-registered, so a stock draft schema needs no registration.
- output tiers: `OutputFormat.Flag` is the boolean-only fast path (the `CHECK`-constraint parity), `List` is the flat per-keyword failure list for a `Validate` receipt, `Hierarchical` is the nested tree — `EvaluationOptions.OutputFormat` selects the tier, never a second evaluation pass.

[LOCAL_ADMISSION]:
- `JsonSchema.Net` enters as the in-process `validation`-rail evaluator behind the `pg_jsonschema` server extension — the application-side computation of the SAME schema verdict, never a second loose-DOM validator family.
- the schema string is the pre-frozen document-lane shape (`Query/lanes#DOCUMENT_LANE`), never raw runtime input; parse it once at composition and reuse the `JsonSchema`.
- the four process-global registries (`SchemaRegistry`/`VocabularyRegistry`/`DialectRegistry`/`FormatRegistry`) are shared state — register external `$ref` documents and custom formats once at startup, not per-evaluate.
- `Evaluate` never throws on a schema-invalid instance (that is the verdict); `JsonSchema.FromText` throws `JsonSchemaException` only when the schema JSON is not itself a valid schema, and `$ref` resolution failure lifts `RefResolutionException` at evaluate — both fold into the `validation` fault rail, discriminated from a data-invalid `IsValid == false`.

[STACKING]:
- in-process fallback for `pg_jsonschema` (`Store/provisioning#SERVER_EXTENSIONS`): the `ServerExtension` row for `pg_jsonschema` is `Standalone`/`FailureRank.Observational` — when the operator-provisioned image carries the pgrx-compiled extension, the document lane rejects a malformed document at WRITE via `CHECK (jsonb_matches_schema('<schema>', <column>))`; when the extension is absent, the `Validate` fold moves the SAME check application-side to `JsonSchema.FromText(frozenSchema).Evaluate(candidate)` reading `EvaluationResults.IsValid`, so validation degrades to in-process rather than silently dropping — the design cross-reference is `api-pg-server-bgworkers.md` (`json_matches_schema`/`jsonb_matches_schema` fallback rows).
- richer receipt than the server `bool`: the server-side function returns only a boolean; the in-process path sets `EvaluationOptions.OutputFormat = OutputFormat.List` and reads `EvaluationResults.Details`/`Errors` to project the per-keyword failure into a `Validate` receipt the `CHECK` constraint cannot surface, so the fallback path carries STRICTLY more diagnostic capability than the extension it substitutes for.
- write-boundary POCO gate: `ValidatingJsonConverter.MapType<T>(schema)` binds a `JsonSchema` to a CLR shape so a `System.Text.Json` deserialize validates against the schema in one pass — the typed-document ingress gate beside the raw-`jsonb` `CHECK`, one schema authority for both.
- signature authority: the verified evaluate overload is `Evaluate(JsonElement, EvaluationOptions?)` (a `JsonNode` is read through `JsonSerializer`/`RootElement`), which supersedes the design-page `Evaluate(JsonNode?, …)` transcription — verified-local wins.

[RAIL_LAW]:
- Package: `JsonSchema.Net`
- Owns: in-process JSON Schema evaluation — parse, compiled-node build, dialect/vocabulary/meta-schema resolution, `$ref` document resolution, format assertion, and the three output tiers
- Accept: parse-once-evaluate-many of the pre-frozen document-lane schema as the `pg_jsonschema` in-process fallback, `EvaluationResults.IsValid` for the boolean verdict and `Details`/`Errors` for the receipt rail, `ValidatingJsonConverter` for the typed-document ingress gate
- Reject: re-parsing the schema per instance, a second loose-DOM validator path beside this one, treating the `Apache-2.0` design-page license as authoritative (it is MIT + OSMF EULA), and the design-page `Evaluate(JsonNode?, …)` signature (the real overload takes `JsonElement`)
