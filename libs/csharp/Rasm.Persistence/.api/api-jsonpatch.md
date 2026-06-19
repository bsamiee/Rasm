# [RASM_PERSISTENCE_API_JSONPATCH]

`Microsoft.AspNetCore.JsonPatch.SystemTextJson` supplies RFC 6902 JSON Patch document
construction, serialisation via `System.Text.Json`, and application to object graphs
for Persistence snapshot partial-update profiles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- package: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- assembly: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- namespace: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`, `Microsoft.AspNetCore.JsonPatch.SystemTextJson.Operations`
- asset: runtime library
- rail: snapshot

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: patch document family
- rail: snapshot

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                  |
| :-----: | :--------------------- | :----------------- | :-------------------------------------------- |
|  [01]   | `JsonPatchDocument`    | untyped patch doc  | RFC 6902 operations list and apply            |
|  [02]   | `JsonPatchDocument<T>` | typed patch doc    | typed RFC 6902 operations list and apply      |
|  [03]   | `IJsonPatchDocument`   | patch doc contract | operations access and application             |
|  [04]   | `JsonPatchError`       | error value        | operation, error message, and affected object |

[PUBLIC_TYPE_SCOPE]: operations family
- rail: snapshot

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]  | [CAPABILITY]                                          |
| :-----: | :-------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `Operation`     | untyped op     | RFC 6902 operation with `op`, `path`, `from`, `value` |
|  [02]   | `OperationBase` | operation base | `op`, `path`, `from`, `OperationType` property        |
|  [03]   | `OperationType` | op enum        | `Add`, `Remove`, `Replace`, `Move`, `Copy`, `Test`    |

[PUBLIC_TYPE_SCOPE]: adapter and converter family
- rail: snapshot

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [CAPABILITY]                                     |
| :-----: | :----------------------- | :--------------- | :----------------------------------------------- |
|  [01]   | `IObjectAdapter`         | adapter contract | `Add`, `Remove`, `Replace`, `Move`, `Copy`       |
|  [02]   | `IObjectAdapterWithTest` | adapter contract | extends `IObjectAdapter` with `Test`             |
|  [03]   | `ObjectAdapter`          | default adapter  | reflection-based POCO adapter                    |
|  [04]   | `IAdapterFactory`        | factory contract | produces `IAdapter` for a given object type      |
|  [05]   | `AdapterFactory`         | default factory  | selects list, dictionary, POCO, or JSON adapters |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: typed document construction
- rail: snapshot

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `new JsonPatchDocument()`                                    | ctor           | creates empty untyped patch document           |
|  [02]   | `new JsonPatchDocument(operations, serializerOptions)`       | ctor           | creates from operation list and STJ options    |
|  [03]   | `JsonPatchDocument.Add(path, value)`                         | fluent op      | appends `add` operation                        |
|  [04]   | `JsonPatchDocument.Remove(path)`                             | fluent op      | appends `remove` operation                     |
|  [05]   | `JsonPatchDocument.Replace(path, value)`                     | fluent op      | appends `replace` operation                    |
|  [06]   | `JsonPatchDocument.Move(from, path)`                         | fluent op      | appends `move` operation                       |
|  [07]   | `JsonPatchDocument.Copy(from, path)`                         | fluent op      | appends `copy` operation                       |
|  [08]   | `JsonPatchDocument.Test(path, value)`                        | fluent op      | appends `test` operation                       |
|  [09]   | `JsonPatchDocument.ApplyTo(objectToApplyTo)`                 | apply          | applies all operations using default adapter   |
|  [10]   | `JsonPatchDocument.ApplyTo(objectToApplyTo, adapter)`        | apply          | applies with custom `IObjectAdapter`           |
|  [11]   | `JsonPatchDocument.ApplyTo(objectToApplyTo, logErrorAction)` | apply          | applies and invokes error callback per failure |

[ENTRYPOINT_SCOPE]: operation application
- rail: snapshot

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `Operation.Apply(objectToApplyTo, adapter)` | op apply       | dispatches to adapter method by `OperationType` |
|  [02]   | `OperationBase.OperationType`               | property       | typed `OperationType` discriminant              |
|  [03]   | `OperationBase.op`                          | property       | RFC 6902 operation string                       |
|  [04]   | `OperationBase.path`                        | property       | JSON Pointer path string                        |
|  [05]   | `OperationBase.from`                        | property       | JSON Pointer source path (move/copy)            |

## [04]-[IMPLEMENTATION_LAW]

[PATCH_TOPOLOGY]:
- `JsonPatchDocument` is decorated with `[JsonConverter(typeof(JsonPatchDocumentConverter))]`; STJ deserialisation is built-in
- `JsonPatchDocument.SerializerOptions` defaults to `JsonSerializerOptions.Default`; it is settable per-instance
- `Operation.Apply` dispatches over `OperationType`: `Add`, `Remove`, `Replace`, `Move`, `Copy`, `Test`
- `Test` operation requires `IObjectAdapterWithTest`; using `ObjectAdapter` (which does not implement it) throws `NotSupportedException`
- path strings are JSON Pointer format (RFC 6901); `PathHelpers.ValidateAndNormalizePath` is called internally on every fluent builder method
- list indexing uses `-` as an append sentinel per RFC 6902

[LOCAL_ADMISSION]:
- Patch documents persist as `List<Operation>` over the wire; deserialisation reconstructs them through the registered STJ converter.
- Error handling uses the `Action<JsonPatchError>` callback form rather than exception propagation for partial-apply scenarios.
- Custom adapter injection via `IAdapterFactory` is the extension point for non-POCO targets (e.g. `JsonNode`, `JsonObject`).

[RAIL_LAW]:
- Package: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- Owns: RFC 6902 patch document construction, serialisation, and application
- Accept: `JsonPatchDocument<T>` typed paths, `IObjectAdapter` for non-POCO targets
- Reject: JSON Patch document hand-rolling, Newtonsoft-backed patch documents
