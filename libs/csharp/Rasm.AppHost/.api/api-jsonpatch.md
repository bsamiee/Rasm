# [RASM_APPHOST_API_JSONPATCH]

`Microsoft.AspNetCore.JsonPatch.SystemTextJson` supplies RFC 6902 JSON Patch document parsing, operation application over untyped `JsonPatchDocument` and strongly typed `JsonPatchDocument<TModel>`, an `IObjectAdapter` contract for custom object mutation, and a `JsonPatchException` error rail for boundary consumers using `System.Text.Json` serialization.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- package: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- assembly: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- namespace: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- asset: runtime library
- rail: boundary

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and error family
- rail: boundary

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]     | [RAIL]                    |
| :-----: | :------------------- | :---------------- | :------------------------ |
|   [1]   | `IJsonPatchDocument` | document contract | serializer options + ops  |
|   [2]   | `JsonPatchDocument`  | untyped document  | RFC 6902 patch intake     |
|   [3]   | `JsonPatchError`     | error value       | operation failure capture |

[PUBLIC_TYPE_SCOPE]: operation family
- rail: boundary

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [RAIL]                                    |
| :-----: | :------------------------- | :-------------- | :---------------------------------------- |
|   [1]   | `Operations.Operation`     | operation value | path + op + value + from                  |
|   [2]   | `Operations.OperationBase` | operation base  | op, path, from fields                     |
|   [3]   | `Operations.OperationType` | operation enum  | Add/Remove/Replace/Move/Copy/Test/Invalid |

[PUBLIC_TYPE_SCOPE]: adapter family
- rail: boundary

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]       | [RAIL]                    |
| :-----: | :--------------------------------------------- | :------------------ | :------------------------ |
|   [1]   | `Adapters.IObjectAdapter`                      | adapter contract    | five mutation operations  |
|   [2]   | `Adapters.IObjectAdapterWithTest`              | test adapter        | `IObjectAdapter` + `Test` |
|   [3]   | `Adapters.IAdapterFactory`                     | factory contract    | adapter selection         |
|   [4]   | `Adapters.JsonObjectAdapter`                   | JSON object adapter | `JsonObject` mutation     |
|   [5]   | `Exceptions.JsonPatchException`                | parse/apply error   | operation failure signal  |
|   [6]   | `Converters.JsonPatchDocumentConverter`        | STJ converter       | untyped document codec    |
|   [7]   | `Converters.JsonPatchDocumentConverterFactory` | STJ factory         | typed document codec      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document construction
- rail: boundary

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `JsonPatchDocument()`                                       | default ctor   | empty document, default options |
|   [2]   | `JsonPatchDocument(List<Operation>, JsonSerializerOptions)` | explicit ctor  | pre-built operation list        |

[ENTRYPOINT_SCOPE]: document mutation operations
- rail: boundary

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY]   | [RAIL]             |
| :-----: | :--------------------- | :--------------- | :----------------- |
|   [1]   | `Add(path, value)`     | RFC 6902 add     | insert or append   |
|   [2]   | `Remove(path)`         | RFC 6902 remove  | remove at path     |
|   [3]   | `Replace(path, value)` | RFC 6902 replace | replace at path    |
|   [4]   | `Move(from, path)`     | RFC 6902 move    | remove + insert    |
|   [5]   | `Copy(from, path)`     | RFC 6902 copy    | duplicate value    |
|   [6]   | `Test(path, value)`    | RFC 6902 test    | equality assertion |

[ENTRYPOINT_SCOPE]: application operations
- rail: boundary

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]     | [RAIL]                        |
| :-----: | :-------------------------------------------------- | :----------------- | :---------------------------- |
|   [1]   | `ApplyTo(objectToApplyTo)`                          | default apply      | throws `JsonPatchException`   |
|   [2]   | `ApplyTo(objectToApplyTo, logErrorAction)`          | error-logged apply | logs errors, stops at first   |
|   [3]   | `ApplyTo(objectToApplyTo, adapter)`                 | adapter apply      | throws on error               |
|   [4]   | `ApplyTo(objectToApplyTo, adapter, logErrorAction)` | adapter + log      | logs errors via callback      |
|   [5]   | `IJsonPatchDocument.GetOperations()`                | operation list     | defensive copy of operations  |
|   [6]   | `Operation.Apply(objectToApplyTo, adapter)`         | single operation   | dispatches by `OperationType` |

[ENTRYPOINT_SCOPE]: adapter contract operations
- rail: boundary

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :----------------------------------------------- | :------------- | :------------------------- |
|   [1]   | `IObjectAdapter.Add(operation, target)`          | mutation       | add operation dispatch     |
|   [2]   | `IObjectAdapter.Remove(operation, target)`       | mutation       | remove operation dispatch  |
|   [3]   | `IObjectAdapter.Replace(operation, target)`      | mutation       | replace operation dispatch |
|   [4]   | `IObjectAdapter.Move(operation, target)`         | mutation       | move operation dispatch    |
|   [5]   | `IObjectAdapter.Copy(operation, target)`         | mutation       | copy operation dispatch    |
|   [6]   | `IObjectAdapterWithTest.Test(operation, target)` | assertion      | test operation dispatch    |

## [4]-[IMPLEMENTATION_LAW]

[JSONPATCH_TOPOLOGY]:
- public namespaces: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`, `.Operations`, `.Adapters`, `.Exceptions`, `.Converters`
- document model: `List<Operation>` plus `JsonSerializerOptions`; operations carry `op`, `path`, `from`, `value`
- serializer: `[JsonConverter(typeof(JsonPatchDocumentConverter))]` on `JsonPatchDocument`; media type `application/json-patch+json`
- `OperationType` cases: `Add`, `Remove`, `Replace`, `Move`, `Copy`, `Test`, `Invalid`
- apply model: sequential; stops at first `JsonPatchException` when a `logErrorAction` delegate is supplied
- endpoint metadata: `JsonPatchDocument` implements `IEndpointParameterMetadataProvider`, registers `AcceptsMetadata` for `application/json-patch+json`
- `IObjectAdapterWithTest.Test` raises `NotSupportedException` when the adapter does not implement it

[LOCAL_ADMISSION]:
- Boundary intake deserializes from `application/json-patch+json` into `JsonPatchDocument`; typed consumers resolve `JsonPatchDocument<T>` from the converter factory.
- `ApplyTo` with a `logErrorAction` delegate is the safe form; bare `ApplyTo` throws `JsonPatchException` at the first invalid operation.
- Custom adapters implement `IObjectAdapter`; the built-in `IAdapterFactory` covers POCO, `IList`, and `JsonObject` targets.

[RAIL_LAW]:
- Package: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- Owns: RFC 6902 JSON Patch document intake and application
- Accept: `application/json-patch+json` payloads deserialized into `JsonPatchDocument`
- Reject: hand-rolled RFC 6902 operation dispatch or Newtonsoft-based `JsonPatchDocument` forms
