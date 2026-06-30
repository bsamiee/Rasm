# [RASM_PERSISTENCE_API_JSONPATCH]

`Microsoft.AspNetCore.JsonPatch.SystemTextJson` supplies RFC 6902 JSON Patch document
construction, `System.Text.Json` serialisation through a registered converter factory, and
break-on-first-error application to object graphs — the document-granular partial-update fallback
the `Version/ledger#SYNC_TRANSPORTS` op-log changefeed (the `HttpDelta` `OutboundHop` leg) falls back to (RFC 7386 merge-patch is the
rejected form). It is the STJ-native variant: the patch document is serialised by STJ
(`Utf8JsonReader`/`Utf8JsonWriter`), never Newtonsoft, and it carries a public minimal-API binding
seam (`IEndpointParameterMetadataProvider`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- package: `Microsoft.AspNetCore.JsonPatch.SystemTextJson` (10.0.9)
- assembly: `Microsoft.AspNetCore.JsonPatch.SystemTextJson` (`lib/net10.0` — single-target, no TFM fallback)
- namespace: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`, `.Operations`, `.Adapters`, `.Exceptions`
- license: MIT
- asset: runtime library
- rail: snapshot, collaboration-fallback

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: patch document family
- rail: snapshot

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                                              |
| :-----: | :--------------------- | :----------------- | :----------------------------------------------------------------------- |
|  [01]   | `JsonPatchDocument`    | untyped patch doc  | string-path RFC 6902 op list + apply; `[JsonConverter(JsonPatchDocumentConverter)]` |
|  [02]   | `JsonPatchDocument<T>` | typed patch doc    | `Expression<Func<T,_>>` lambda-path op list + apply; `[JsonConverter(JsonPatchDocumentConverterFactory)]` |
|  [03]   | `IJsonPatchDocument`   | patch doc contract | `GetOperations()`; the untyped operation projection both docs expose     |
|  [04]   | `JsonPatchError`       | error value        | `AffectedObject`, `Operation`, `ErrorMessage` — the `logErrorAction` payload |

`JsonPatchDocument` and `JsonPatchDocument<T>` both implement `IEndpointParameterMetadataProvider`
(`static PopulateMetadata(ParameterInfo, EndpointBuilder)`), so each is a first-class minimal-API
request-body parameter that registers its `application/json-patch+json` accepts-metadata.

[PUBLIC_TYPE_SCOPE]: operations family (`.Operations`)
- rail: snapshot

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [CAPABILITY]                                                          |
| :-----: | :---------------- | :--------------- | :------------------------------------------------------------------- |
|  [01]   | `OperationBase`   | operation base   | `op` (string), `path`, `from` (JSON Pointer), `OperationType` (parsed); `ShouldSerializeFrom()` |
|  [02]   | `Operation`       | untyped op       | `OperationBase` + `object value`; `Apply(object, IObjectAdapter)` dispatch |
|  [03]   | `Operation<T>`    | typed op         | `Operation` + `Apply(T, IObjectAdapter)`; the element type of `JsonPatchDocument<T>.Operations` |
|  [04]   | `OperationType`   | op enum          | `Add`, `Remove`, `Replace`, `Move`, `Copy`, `Test`, **`Invalid`**     |

`OperationBase.op` parses to `OperationType` via `Enum.TryParse(ignoreCase)` — an unrecognised `op`
string lands `OperationType.Invalid`, the discriminant a defensive apply must reject.

[PUBLIC_TYPE_SCOPE]: adapter + exception family (`.Adapters`, `.Exceptions`)
- rail: snapshot

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [CAPABILITY]                                                              |
| :-----: | :----------------------- | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `IObjectAdapter`         | adapter contract | `Add`, `Remove`, `Replace`, `Move`, `Copy` `(Operation, object)` — the per-graph apply seam |
|  [02]   | `IObjectAdapterWithTest` | adapter contract | extends `IObjectAdapter` with `Test(Operation, object)`                   |
|  [03]   | `JsonPatchException`     | typed failure    | `FailedOperation`, `AffectedObject`; thrown per-op, caught inside `ApplyTo`, folded to `JsonPatchError` |

The default adapter (`ObjectAdapter` + its `AdapterFactory` dispatching list/dictionary/`JsonObject`/POCO
adapters) is `internal`: it is constructed by the parameterless `ApplyTo`, never a consumer surface.
The STJ variant exposes NO public `IAdapterFactory`/`AdapterFactory`/`ObjectAdapter` — the public
extension seam for a non-POCO target (e.g. a `JsonNode` snapshot) is implementing `IObjectAdapter` /
`IObjectAdapterWithTest` and passing it to the `ApplyTo(obj, adapter)` overload. (Those factory types
exist only in the legacy Newtonsoft `Microsoft.AspNetCore.JsonPatch` package, which this rail rejects.)

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: untyped string-path construction
- rail: snapshot

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `new JsonPatchDocument()`                                    | ctor           | empty doc; `SerializerOptions` defaults to `JsonSerializerOptions.Default` |
|  [02]   | `new JsonPatchDocument(List<Operation>, JsonSerializerOptions)` | ctor        | rehydrate from operation list under explicit STJ options |
|  [03]   | `Add(path, value)` / `Remove(path)` / `Replace(path, value)` | fluent op      | append; each returns `this` for chaining, path normalised internally |
|  [04]   | `Move(from, path)` / `Copy(from, path)` / `Test(path, value)` | fluent op     | append move/copy/test; `from`/`path` are JSON Pointer (RFC 6901) |

[ENTRYPOINT_SCOPE]: typed lambda-path construction (`JsonPatchDocument<T>`)
- rail: snapshot

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY]   | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------------ | :--------------- | :--------------------------------------------------- |
|  [01]   | `Add<P>(Expression<Func<T,P>> path, P value)`                             | typed scalar op  | compile-checked member path; refactor-safe vs string |
|  [02]   | `Add<P>(Expression<Func<T,IList<P>>> path, P value [, int position])`     | typed list op    | append (`-` sentinel) or insert at index             |
|  [03]   | `Remove<P>(Expression<Func<T,P>> path)` / `Remove<P>(IList path [, int position])` | typed op | scalar clear or list element removal at index        |
|  [04]   | `Replace<P>` / `Test<P>` (scalar + `IList<P>` + `int position` variants)  | typed op         | typed replace/test mirroring the scalar/list/positional arity |
|  [05]   | `Move<P>` / `Copy<P>` (scalar↔scalar, list↔scalar, scalar↔list, list↔list with `positionFrom`/`positionTo`) | typed relocation | every endpoint-arity combination of source/target index |

The typed overloads are the integration default: a `JsonPatchDocument<TSnapshot>` built from
`Expression` member paths binds the patch to the snapshot record's shape, so a renamed property breaks
the build rather than producing a silent no-op `path` at apply.

[ENTRYPOINT_SCOPE]: application + dispatch
- rail: snapshot

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `ApplyTo(objectToApplyTo)`                             | apply          | default internal `ObjectAdapter`; throws `JsonPatchException` on first failure |
|  [02]   | `ApplyTo(objectToApplyTo, Action<JsonPatchError>)`    | apply          | break-on-first-error; the failing op reports through the callback, no throw |
|  [03]   | `ApplyTo(objectToApplyTo, IObjectAdapter)`            | apply          | custom adapter (non-POCO target), throwing form      |
|  [04]   | `ApplyTo(objectToApplyTo, IObjectAdapter, Action<JsonPatchError>)` | apply | custom adapter + error callback — the fullest control point |
|  [05]   | `Operation.Apply(objectToApplyTo, IObjectAdapter)`    | op dispatch    | switch over `OperationType` to the adapter's `Add`/`Remove`/`Replace`/`Move`/`Copy`/`Test` |
|  [06]   | `IJsonPatchDocument.GetOperations()`                  | projection     | untyped `IList<Operation>` view for serialisation/inspection |

`ApplyTo` is break-on-first-error: it iterates `Operations`, and the first op that throws
`JsonPatchException` is folded into a `JsonPatchError` (via the supplied `logErrorAction` or the
internal `ErrorReporter.Default`) and the loop **breaks** — later operations do not run. Atomic
all-or-nothing apply is therefore a caller obligation (apply to a clone, swap on full success).

## [04]-[IMPLEMENTATION_LAW]

[PATCH_TOPOLOGY]:
- `JsonPatchDocument` carries `[JsonConverter(typeof(JsonPatchDocumentConverter))]`; `JsonPatchDocument<T>` carries `[JsonConverter(typeof(JsonPatchDocumentConverterFactory))]` — STJ (de)serialisation is built-in, no manual converter registration. Both converters and the factory are `internal`, reached only through the attributes.
- `SerializerOptions` is a settable per-instance `JsonSerializerOptions` defaulting to `JsonSerializerOptions.Default`; it governs how `value` payloads inside operations bind to/from member types.
- `Operation.Apply` dispatches over `OperationType`; `OperationType.Invalid` (unknown `op` string) is unhandled by the switch and surfaces as a failure.
- the `Test` operation requires `IObjectAdapterWithTest`; routing `Test` through a bare `IObjectAdapter` is a contract gap — the internal default adapter implements `IObjectAdapterWithTest`, so default `ApplyTo` supports `Test`.
- list indexing uses `-` as the RFC 6902 append sentinel; out-of-range indices fault as `JsonPatchError` rather than silently clamping. Internal `PathHelpers.ValidateAndNormalizePath` runs on every fluent builder call, so a malformed pointer faults at build time.

[LOCAL_ADMISSION]:
- Patch documents persist as `List<Operation>` on the wire; deserialisation reconstructs them through the registered STJ converter under the same `JsonSerializerOptions` the snapshot rail's `PersistenceWireContext` carries.
- Error handling uses the `Action<JsonPatchError>` callback form, not exception propagation: the `Version/ledger#SYNC_TRANSPORTS` document-granular fallback applies a patch to a snapshot clone with a callback that lifts each `JsonPatchError`/`JsonPatchException` once at the seam into an `Error.New(...)` on the sync receipt, mirroring the Speckle marshal-fault lift — never a raw STJ fault crossing into the merge law.
- A non-POCO snapshot target (`JsonNode`/`JsonObject`) injects a consumer `IObjectAdapterWithTest` through the `ApplyTo(obj, adapter, logErrorAction)` overload — there is no public factory to register, so the adapter is the extension point.
- The typed `JsonPatchDocument<TSnapshot>` is the canonical authoring shape so patch paths track the snapshot record's member names through `Expression`, leaving string-path `JsonPatchDocument` for wire-decoded inbound patches only.

[RAIL_LAW]:
- Package: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- Owns: RFC 6902 patch document construction, STJ serialisation, and break-on-first-error application
- Accept: `JsonPatchDocument<T>` typed `Expression` paths, `IObjectAdapter`/`IObjectAdapterWithTest` for non-POCO targets, the `Action<JsonPatchError>` collector
- Reject: JSON Patch hand-rolling, Newtonsoft-backed patch documents, RFC 7386 merge-patch, the legacy `IAdapterFactory`/`AdapterFactory` surface
