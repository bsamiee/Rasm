# [RASM_API_JSONPATCH]

`Microsoft.AspNetCore.JsonPatch.SystemTextJson` supplies RFC 6902 JSON Patch document parsing, operation application over untyped `JsonPatchDocument` and strongly typed `JsonPatchDocument<TModel>`, an `IObjectAdapter` contract for custom object mutation, and a `JsonPatchException` error rail for boundary consumers using `System.Text.Json` serialization.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- package: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- assembly: `Microsoft.AspNetCore.JsonPatch.SystemTextJson` (`lib/net10.0`, single-target)
- namespaces: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`, `.Operations`, `.Adapters`, `.Exceptions`
- asset: runtime library
- rail: boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and error family
- rail: boundary

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [RAIL]                                                                            |
| :-----: | :--------------------- | :---------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `IJsonPatchDocument`   | document contract | `GetOperations()` projection both docs expose                                     |
|  [02]   | `JsonPatchDocument`    | untyped document  | string-path RFC 6902 intake; `[JsonConverter]` internal converter                 |
|  [03]   | `JsonPatchDocument<T>` | typed document    | `Expression<Func<T,_>>` lambda-path op list; `IEndpointParameterMetadataProvider` |
|  [04]   | `JsonPatchError`       | error value       | operation failure capture                                                         |

Both `JsonPatchDocument` and `JsonPatchDocument<T>` implement `IEndpointParameterMetadataProvider`, so each is a first-class minimal-API request-body parameter registering `application/json-patch+json` accepts-metadata.

[PUBLIC_TYPE_SCOPE]: operation family
- rail: boundary

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [RAIL]                                                                                 |
| :-----: | :------------------------- | :------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `Operations.OperationBase` | operation base | `op`/`path`/`from` + parsed `OperationType`; `ShouldSerializeFrom()`                   |
|  [02]   | `Operations.Operation`     | untyped op     | `OperationBase` + `object value`; `Apply(object, IObjectAdapter)`                      |
|  [03]   | `Operations.Operation<T>`  | typed op       | `Operation` + `Apply(T, IObjectAdapter)`; element of `JsonPatchDocument<T>.Operations` |
|  [04]   | `Operations.OperationType` | operation enum | Add/Remove/Replace/Move/Copy/Test/Invalid                                              |

[PUBLIC_TYPE_SCOPE]: adapter family
- rail: boundary

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [RAIL]                                                                          |
| :-----: | :-------------------------------- | :---------------- | :------------------------------------------------------------------------------ |
|  [01]   | `Adapters.IObjectAdapter`         | adapter contract  | `Add`/`Remove`/`Replace`/`Move`/`Copy` `(Operation, object)`                    |
|  [02]   | `Adapters.IObjectAdapterWithTest` | test adapter      | `IObjectAdapter` + `Test(Operation, object)`                                    |
|  [03]   | `Exceptions.JsonPatchException`   | parse/apply error | `FailedOperation`/`AffectedObject`; folded to `JsonPatchError` inside `ApplyTo` |

The default adapter (`ObjectAdapter`/`AdapterFactory` dispatching list/dictionary/`JsonObject`/POCO) and the STJ converters (`JsonConverterForJsonPatchDocument…`, `AdapterFactory`) are `internal` in this STJ package — the ONLY public extension seam for a non-POCO target is implementing `IObjectAdapter`/`IObjectAdapterWithTest` and passing it to the `ApplyTo(obj, adapter)` overload. The public `IAdapterFactory`/`AdapterFactory`/`ObjectAdapter`/`JsonObjectAdapter` surface exists only in the Newtonsoft `Microsoft.AspNetCore.JsonPatch` package, which this rail rejects.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document construction
- rail: boundary

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `JsonPatchDocument()`                                       | default ctor   | empty document, default options |
|  [02]   | `JsonPatchDocument(List<Operation>, JsonSerializerOptions)` | explicit ctor  | pre-built operation list        |

[ENTRYPOINT_SCOPE]: document mutation operations
- rail: boundary

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY]   | [RAIL]             |
| :-----: | :--------------------- | :--------------- | :----------------- |
|  [01]   | `Add(path, value)`     | RFC 6902 add     | insert or append   |
|  [02]   | `Remove(path)`         | RFC 6902 remove  | remove at path     |
|  [03]   | `Replace(path, value)` | RFC 6902 replace | replace at path    |
|  [04]   | `Move(from, path)`     | RFC 6902 move    | remove + insert    |
|  [05]   | `Copy(from, path)`     | RFC 6902 copy    | duplicate value    |
|  [06]   | `Test(path, value)`    | RFC 6902 test    | equality assertion |

[ENTRYPOINT_SCOPE]: typed lambda-path construction (`JsonPatchDocument<T>`)
- rail: boundary
- The typed overloads bind the patch to the record's shape through `Expression` member paths, so a renamed property breaks the build rather than producing a silent no-op `path`; the untyped string-path form is for wire-decoded inbound patches only.
- Scalar paths use `Expression<Func<T, P>>`; list paths use `Expression<Func<T, IList<P>>>` plus optional source and destination positions.

| [INDEX] | [SURFACE]                              | [PATH]             | [CAPABILITY]                |
| :-----: | :------------------------------------- | :----------------- | :-------------------------- |
|  [01]   | `Add<P>`                               | scalar             | appends a member value      |
|  [02]   | `Add<P>`                               | list plus position | appends or inserts a value  |
|  [03]   | `Remove<P>` / `Replace<P>` / `Test<P>` | scalar or list     | mutates or asserts a value  |
|  [04]   | `Move<P>` / `Copy<P>`                  | source and target  | relocates across path kinds |

[ENTRYPOINT_SCOPE]: application operations
- rail: boundary

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]     | [RAIL]                        |
| :-----: | :-------------------------------------------------- | :----------------- | :---------------------------- |
|  [01]   | `ApplyTo(objectToApplyTo)`                          | default apply      | throws `JsonPatchException`   |
|  [02]   | `ApplyTo(objectToApplyTo, logErrorAction)`          | error-logged apply | logs errors, stops at first   |
|  [03]   | `ApplyTo(objectToApplyTo, adapter)`                 | adapter apply      | throws on error               |
|  [04]   | `ApplyTo(objectToApplyTo, adapter, logErrorAction)` | adapter + log      | logs errors via callback      |
|  [05]   | `IJsonPatchDocument.GetOperations()`                | operation list     | defensive copy of operations  |
|  [06]   | `Operation.Apply(objectToApplyTo, adapter)`         | single operation   | dispatches by `OperationType` |

[ENTRYPOINT_SCOPE]: adapter contract operations
- rail: boundary

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :----------------------------------------------- | :------------- | :------------------------- |
|  [01]   | `IObjectAdapter.Add(operation, target)`          | mutation       | add operation dispatch     |
|  [02]   | `IObjectAdapter.Remove(operation, target)`       | mutation       | remove operation dispatch  |
|  [03]   | `IObjectAdapter.Replace(operation, target)`      | mutation       | replace operation dispatch |
|  [04]   | `IObjectAdapter.Move(operation, target)`         | mutation       | move operation dispatch    |
|  [05]   | `IObjectAdapter.Copy(operation, target)`         | mutation       | copy operation dispatch    |
|  [06]   | `IObjectAdapterWithTest.Test(operation, target)` | assertion      | test operation dispatch    |

[ENTRYPOINT_SCOPE]: `JsonPatchError` members
- rail: boundary

The value handed to the `ApplyTo(obj, logErrorAction)` callback; read its fields to project a failed operation into the boundary failure rail instead of catching `JsonPatchException`.

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]  | [RAIL]                                                                           |
| :-----: | :------------------------------ | :-------------- | :------------------------------------------------------------------------------- |
|  [01]   | `JsonPatchError.AffectedObject` | error target    | `object AffectedObject { get; }` — object the failed op targeted                 |
|  [02]   | `JsonPatchError.Operation`      | error operation | `Operation Operation { get; }` — the failing op (`.op`/`.path`/`.from`/`.value`) |
|  [03]   | `JsonPatchError.ErrorMessage`   | error message   | `string ErrorMessage { get; }` — failure detail                                  |

## [04]-[IMPLEMENTATION_LAW]

[JSONPATCH_TOPOLOGY]:
- public namespaces: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`, `.Operations`, `.Adapters`, `.Exceptions` (the `.Converters` types are `internal`, reached only through the document `[JsonConverter]` attributes)
- document model: `List<Operation>` plus a settable per-instance `JsonSerializerOptions` (defaults to `JsonSerializerOptions.Default`); operations carry `op`, `path`, `from`, `value`
- serializer: `JsonPatchDocument` and `JsonPatchDocument<T>` carry internal `[JsonConverter]` factory attributes — STJ (de)serialization is built-in, no manual converter registration; media type `application/json-patch+json`
- `OperationType` cases: `Add`, `Remove`, `Replace`, `Move`, `Copy`, `Test`, `Invalid` (an unrecognized `op` string lands `Invalid`, the discriminant a defensive apply rejects)
- apply model: break-on-first-error — the first op that throws `JsonPatchException` folds into a `JsonPatchError` (via the `logErrorAction` or the internal default reporter) and the loop breaks; atomic all-or-nothing apply is a caller obligation (apply to a clone, swap on full success)
- endpoint metadata: `JsonPatchDocument` and `JsonPatchDocument<T>` implement `IEndpointParameterMetadataProvider`, registering `AcceptsMetadata` for `application/json-patch+json`
- `Test` requires `IObjectAdapterWithTest`; the internal default adapter implements it, so default `ApplyTo` executes `Test`

[LOCAL_ADMISSION]:
- Boundary intake deserializes from `application/json-patch+json` into `JsonPatchDocument`; typed consumers resolve `JsonPatchDocument<T>` from the converter factory.
- `ApplyTo` with a `logErrorAction` delegate is the safe form; bare `ApplyTo` throws `JsonPatchException` at the first invalid operation.
- A non-POCO target (`JsonNode`/`JsonObject`) injects a consumer `IObjectAdapterWithTest` through the `ApplyTo(obj, adapter, logErrorAction)` overload; the built-in list/dictionary/`JsonObject`/POCO adapters are internal, reached only through the parameterless `ApplyTo`.
- `JsonPatchDocument<TModel>` is the canonical authoring shape so patch paths track member names through `Expression`, leaving string-path `JsonPatchDocument` for wire-decoded inbound patches.

[RAIL_LAW]:
- Package: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- Owns: RFC 6902 JSON Patch document construction, STJ serialization, and break-on-first-error application
- Accept: `JsonPatchDocument<T>` typed `Expression` paths, `IObjectAdapter`/`IObjectAdapterWithTest` for non-POCO targets, the `Action<JsonPatchError>` collector
- Reject: hand-rolled RFC 6902 dispatch, Newtonsoft-backed patch documents, RFC 7386 merge-patch, the public `IAdapterFactory`/`AdapterFactory` surface
