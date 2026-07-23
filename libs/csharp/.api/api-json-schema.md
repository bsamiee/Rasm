# [RASM_API_JSON_SCHEMA]

`System.Text.Json.Schema` projects a JSON Schema document from a live serializer contract: the same `JsonSerializerOptions` instance decides the emitted schema exactly as it decides the wire, so a schema and the bytes it describes cannot drift. Export reads the contract, writes no value, and returns a mutable `JsonNode` the caller post-annotates in hand.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Text.Json`
- package: `System.Text.Json` (MIT)
- assembly: `System.Text.Json.dll` (shared framework)
- namespace: `System.Text.Json.Schema`
- rail: contract projection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter, its knob owner, and the per-node transform payload

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                  |
| :-----: | :-------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `JsonSchemaExporter`        | static class    | extension host for both export overloads      |
|  [02]   | `JsonSchemaExporterOptions` | sealed class    | init-only nullability and transform knobs     |
|  [03]   | `JsonSchemaExporterContext` | readonly struct | per-node addressing payload a transform reads |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `JsonSchemaExporter` export — both overloads are extensions; a `null` exporter-options argument binds `JsonSchemaExporterOptions.Default`

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------------------- | :------ | :------------------------------ |
|  [01]   | `GetJsonSchemaAsNode(JsonSerializerOptions, Type, JsonSchemaExporterOptions)` | static  | resolves the type, then exports |
|  [02]   | `GetJsonSchemaAsNode(JsonTypeInfo, JsonSchemaExporterOptions)`                | static  | exports a pre-resolved contract |

[ENTRYPOINT_SCOPE]: `JsonSchemaExporterOptions` knobs — settable members are init-only

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Default -> JsonSchemaExporterOptions`                                       | property | zero-knob instance                          |
|  [02]   | `TreatNullObliviousAsNonNullable -> bool`                                    | property | null-oblivious reference reads non-nullable |
|  [03]   | `TransformSchemaNode -> Func<JsonSchemaExporterContext, JsonNode, JsonNode>` | property | per-node post-transform seam                |

[ENTRYPOINT_SCOPE]: `JsonSchemaExporterContext` addressing reads a transform folds over

| [INDEX] | [SURFACE]                          | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :--------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Path -> ReadOnlySpan<string>`     | property | node location as pointer segments    |
|  [02]   | `TypeInfo -> JsonTypeInfo`         | property | contract of the node's declared type |
|  [03]   | `PropertyInfo -> JsonPropertyInfo` | property | owning property, `null` off a member |
|  [04]   | `BaseTypeInfo -> JsonTypeInfo`     | property | polymorphic base at a derived branch |

- `JsonSchemaExporter.GetJsonSchemaAsNode`: both overloads call `JsonSerializerOptions.MakeReadOnly()` first, freezing the options instance against a later converter or resolver mutation.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ReferenceHandler.Preserve` on the options rejects export with `NotSupportedException`.
- A repeated `(JsonTypeInfo, JsonPropertyInfo)` pair emits `{"$ref": "<json-pointer>"}` to its first occurrence in the same document, so a recursive graph terminates and no subtree extracts standalone.
- Nesting past `JsonSerializerOptions.MaxDepth` throws `InvalidOperationException`.
- A converter-backed contract carrying no built-in schema mapping exports the unconstrained `true` node, so `TransformSchemaNode` is the sole route to a described shape for a custom-converter type.
- `TransformSchemaNode` runs bottom-up during node materialization — every child completes before its parent — and its return replaces the node, a different JSON kind included.
- `TreatNullObliviousAsNonNullable` decides only where neither the property's get/set nullability nor the type's own nullability settles the node.

[STACKING]:
- `Thinktecture.Runtime.Extensions.Json`(`.api/api-thinktecture-json.md`): a generated `ThinktectureJsonConverter` describes no schema, so its owner exports as `true` until a `TransformSchemaNode` arm keyed on `JsonSchemaExporterContext.TypeInfo` writes the key or string form the converter emits.
- `NodaTime.Serialization.SystemTextJson`(`.api/api-nodatime-stj.md`): `ConfigureForNodaTime` registers the pattern converters onto the same options export reads, so an `Instant` node takes one transform arm rather than a second date policy.
- `System.IO.Hashing`(`.api/api-hashing.md`): the exported `JsonNode` serializes to the UTF-8 payload `ContentHash.Of` digests, minting the one schema identity every generated SDK binds.
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson`(`.api/api-jsonpatch.md`): export describes a config record and RFC 6902 application mutates its live `JsonObject`, splitting contract projection from structured edit across the two surfaces.
- `Rasm.AppHost` `Runtime/ports`: `SuiteContracts.Schema` composes the suite wire options with `TreatNullObliviousAsNonNullable = true` and owns every call into export.
- Richest composition: `JsonTypeInfoResolver.WithAddedModifier` seeds `JsonPropertyInfo.AttributeProvider` onto the resolved contract, `TransformSchemaNode` reads it back through `JsonSchemaExporterContext.PropertyInfo` and gates each annotation to a subtree by `Path`, so effect and cost metadata rides the resolver chain instead of a post-walk over the emitted tree.

[LOCAL_ADMISSION]:
- Every descriptor, argument record, and config record reaches export through `SuiteContracts.Schema`, the repo's one call site.

[RAIL_LAW]:
- Package: `System.Text.Json`
- Owns: JSON Schema projection of a live serializer contract — resolver-driven type mapping, nullability posture, recursion pointers, and the per-node transform seam
- Accept: options-keyed and `JsonTypeInfo`-keyed export, `TreatNullObliviousAsNonNullable`, `TransformSchemaNode` annotation arms, resolver-modifier metadata threading
- Reject: a hand-mirrored schema literal, a reflection walk over the CLR type, a second serializer configuration built for export
