# [RASM_API_JSONPATCH]

`Microsoft.AspNetCore.JsonPatch.SystemTextJson` owns RFC 6902 patch documents under `System.Text.Json`: an untyped string-pointer document for wire-decoded intake, an `Expression`-pathed typed twin for authored edits, and a break-on-first-error apply folding each failure into a `JsonPatchError`. Extension stops at `IObjectAdapter` — every built-in target adapter and both converters stay assembly-internal, so a non-POCO target enters through a caller-supplied adapter.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- package: `Microsoft.AspNetCore.JsonPatch.SystemTextJson` (MIT, Microsoft)
- assembly: `Microsoft.AspNetCore.JsonPatch.SystemTextJson.dll`
- namespace: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`, `.Operations`, `.Adapters`, `.Exceptions`
- rail: boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, operation, adapter, and failure families

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :-------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `IJsonPatchDocument`              | interface     | op-list projection and options both docs expose  |
|  [02]   | `JsonPatchDocument`               | class         | string-pointer document for wire-decoded intake  |
|  [03]   | `JsonPatchDocument<TModel>`       | class         | `Expression`-pathed document, `TModel : class`   |
|  [04]   | `Operations.OperationBase`        | class         | wire fields and the parsed verb discriminant     |
|  [05]   | `Operations.Operation`            | class         | operation base carrying the untyped operand      |
|  [06]   | `Operations.Operation<TModel>`    | class         | typed operation element, `TModel : class`        |
|  [07]   | `Operations.OperationType`        | enum          | parsed verb discriminant                         |
|  [08]   | `Adapters.IObjectAdapter`         | interface     | five-verb mutation contract over a target        |
|  [09]   | `Adapters.IObjectAdapterWithTest` | interface     | adds the `Test` precondition verb                |
|  [10]   | `JsonPatchError`                  | class         | failure record the log callback receives         |
|  [11]   | `Exceptions.JsonPatchException`   | class         | throw-path failure carrying operation and target |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `JsonPatchDocument` authoring — each builder normalizes its pointer, appends one operation, and returns the document for chaining

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `JsonPatchDocument()`                                       | ctor     | empty list, `JsonSerializerOptions.Default` |
|  [02]   | `JsonPatchDocument(List<Operation>, JsonSerializerOptions)` | ctor     | adopts a pre-built operation list           |
|  [03]   | `Operations -> List<Operation>`                             | property | op list; private setter, mutable list       |
|  [04]   | `SerializerOptions -> JsonSerializerOptions`                | property | settable, serializer-ignored                |
|  [05]   | `Add(string, object)`                                       | instance | inserts or appends at a pointer             |
|  [06]   | `Remove(string)`                                            | instance | removes at a pointer                        |
|  [07]   | `Replace(string, object)`                                   | instance | overwrites at a pointer                     |
|  [08]   | `Move(string, string)`                                      | instance | removes from source, inserts at target      |
|  [09]   | `Copy(string, string)`                                      | instance | duplicates source to target                 |
|  [10]   | `Test(string, object)`                                      | instance | asserts equality at a pointer               |

[ENTRYPOINT_SCOPE]: `JsonPatchDocument<TModel>` authoring — `scalar` spells `Expression<Func<TModel, TProp>>` and `list` spells `Expression<Func<TModel, IList<TProp>>>`, every builder returning the typed document

`Add`, `Remove`, `Replace`, and `Test` each carry a `list` overload targeting the end-of-array pointer and a `list, int` overload targeting one position; `Move` and `Copy` take any source-and-target pairing of `scalar` and `list, int`. `Operations`, `SerializerOptions`, and the parameterless ctor mirror the untyped document.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------- |
|  [01]   | `JsonPatchDocument(List<Operation<TModel>>, JsonSerializerOptions)` | ctor     | adopts a pre-built typed op list |
|  [02]   | `Add<TProp>(scalar, TProp)`                                         | instance | sets a member value              |
|  [03]   | `Remove<TProp>(scalar)`                                             | instance | clears a member                  |
|  [04]   | `Replace<TProp>(scalar, TProp)`                                     | instance | overwrites a member value        |
|  [05]   | `Test<TProp>(scalar, TProp)`                                        | instance | asserts a member value           |
|  [06]   | `Move<TProp>(scalar, scalar)`                                       | instance | relocates between members        |
|  [07]   | `Copy<TProp>(scalar, scalar)`                                       | instance | duplicates between members       |

[ENTRYPOINT_SCOPE]: application — apply folds the operation list in order, stopping at the first failure; the typed document mirrors all four `ApplyTo` overloads with `TModel` in the target slot

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `ApplyTo(object)`                                               | instance | default adapter, throws on failure           |
|  [02]   | `ApplyTo(object, Action<JsonPatchError>)`                       | instance | default adapter, folds the failure and stops |
|  [03]   | `ApplyTo(object, IObjectAdapter)`                               | instance | caller adapter, throws on failure            |
|  [04]   | `ApplyTo(object, IObjectAdapter, Action<JsonPatchError>)`       | instance | caller adapter, folds the failure and stops  |
|  [05]   | `IJsonPatchDocument.GetOperations() -> IList<Operation>`        | instance | per-op defensive copy of the list            |
|  [06]   | `IJsonPatchDocument.SerializerOptions -> JsonSerializerOptions` | property | options seen through the interface           |
|  [07]   | `Operation.Apply(object, IObjectAdapter)`                       | instance | dispatches one op on `OperationType`         |
|  [08]   | `Operation<TModel>.Apply(TModel, IObjectAdapter)`               | instance | typed single-op dispatch                     |

- `ApplyTo(object, IObjectAdapter)`: `IObjectAdapter` is the one public extension seam; every built-in list, dictionary, `JsonObject`, and POCO adapter is assembly-internal, reached through the adapter-free overloads.
- `Operation.Apply`: a `Test` op against a bare `IObjectAdapter` throws `NotSupportedException`, and an `Invalid` verb matches no dispatch arm, so an unrecognized `op` applies silently.
- `IJsonPatchDocument.GetOperations`: explicit interface implementation on both documents, reachable only through an `IJsonPatchDocument` reference.

[ENTRYPOINT_SCOPE]: adapter contract — each verb takes `(Operation, object)` and mutates the target in place

[IObjectAdapter]: `Add` `Remove` `Replace` `Move` `Copy`
[IObjectAdapterWithTest]: `Test`

[ENTRYPOINT_SCOPE]: operation and failure reads — the wire model a hand-built op fills, and the fields a log callback or catch site projects

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------ | :------- | :-------------------------------------------- |
|  [01]   | `Operation(string, string, string, object)`       | ctor     | op, path, from, value                         |
|  [02]   | `OperationBase.op -> string`                      | property | wire verb; its setter parses the discriminant |
|  [03]   | `OperationBase.path -> string`                    | property | RFC 6901 target pointer                       |
|  [04]   | `OperationBase.from -> string`                    | property | source pointer for move and copy              |
|  [05]   | `OperationBase.OperationType -> OperationType`    | property | parsed discriminant, serializer-ignored       |
|  [06]   | `OperationBase.ShouldSerializeFrom() -> bool`     | instance | move and copy alone carry `from`              |
|  [07]   | `Operation.value -> object`                       | property | operand for add, replace, test                |
|  [08]   | `JsonPatchError(object, Operation, string)`       | ctor     | mints a failure record in hand                |
|  [09]   | `JsonPatchError.AffectedObject -> object`         | property | target the failed op ran against              |
|  [10]   | `JsonPatchError.Operation -> Operation`           | property | failing operation                             |
|  [11]   | `JsonPatchError.ErrorMessage -> string`           | property | failure detail                                |
|  [12]   | `JsonPatchException.FailedOperation -> Operation` | property | throw-path failing operation                  |
|  [13]   | `JsonPatchException.AffectedObject -> object`     | property | throw-path target                             |

[OperationType]: `Add` `Remove` `Replace` `Move` `Copy` `Test` `Invalid`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Both documents carry an internal `[JsonConverter]`, so STJ round-trips the wire form unregistered, and both implement `IEndpointParameterMetadataProvider`, registering `application/json-patch+json` accepts-metadata for a minimal-API body parameter.
- Apply breaks on the first failing operation, so an all-or-nothing patch is a caller obligation: patch a clone and swap on full success.
- An adapter-free apply dispatches the target by shape — `Dictionary<,>`, `JsonObject`, `JsonArray`, any `IList`, then POCO — through an internal factory whose adapter implements `IObjectAdapterWithTest`, so a default apply executes `Test`.

[STACKING]:
- `System.Text.Json`(`.api/api-json-schema.md`): one `JsonSerializerOptions` drives both seams, so schema export describes the record this surface patches and contract projection stays split from structured edit.
- `Generator.Equals`(`.api/api-generator-equals.md`): `Inequalities(T, T, MemberPath)` yields the `Inequality.Path` each `Replace` targets, so a changed leaf lands one member-granular pointer rather than a whole-node replace.
- `LanguageExt.Core`(`.api/api-languageext.md`): the `Action<JsonPatchError>` collector projects each failure onto `Validation<F, A>`, so a rejected patch accumulates typed faults instead of unwinding through a catch.
- `Rasm.Persistence` `Version/merge` folds a resolved edit script into one chained `JsonPatchDocument` as the `EntityEdit.Members` payload, and `Rasm.AppHost` `Runtime/config` applies that document to a live `JsonObject` section under `logErrorAction`, gating re-publication on the typed fold.

[LOCAL_ADMISSION]:
- Intake deserializes `application/json-patch+json` into the untyped `JsonPatchDocument`; `JsonPatchDocument<TModel>` is the authoring shape, its `Expression` paths breaking the build on a renamed member.
- Every apply passes a `logErrorAction` collector, so a failed operation lands on the typed failure rail with prior state intact.
- A non-POCO target injects a consumer `IObjectAdapterWithTest` through the adapter overloads.

[RAIL_LAW]:
- Package: `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- Owns: RFC 6902 document construction, STJ round-trip, and break-on-first-error application
- Accept: `Expression` typed paths, a caller `IObjectAdapterWithTest`, the `Action<JsonPatchError>` collector, chained builder authoring
- Reject: hand-rolled RFC 6902 dispatch, a Newtonsoft-backed patch document, RFC 7386 merge-patch
