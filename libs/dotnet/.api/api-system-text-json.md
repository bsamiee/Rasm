# [RASM_API_SYSTEM_TEXT_JSON]

`System.Text.Json` is the branch's one JSON wire: a `JsonSerializerOptions` instance carries the whole contract — resolver chain, converters, naming, number and polymorphism posture — and freezes into an immutable identity every serialize, deserialize, and schema export then reads. Source generation resolves contracts ahead of time so the same surface serves reflection-free hosts, and the low-level reader, writer, and node models expose the same bytes without a second configuration.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Text.Json`
- package: `System.Text.Json` (MIT)
- assembly: `System.Text.Json.dll` (shared framework)
- namespace: `System.Text.Json`, `System.Text.Json.Nodes`, `System.Text.Json.Schema`, `System.Text.Json.Serialization`, `System.Text.Json.Serialization.Metadata`
- rail: contract-frozen JSON wire

## [02]-[SERIALIZER]

[SERIALIZER_TYPE_SCOPE]: entry surface and its one contract carrier

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `JsonSerializer`         | static class  | every read and write overload across span, string, stream, pipe   |
|  [02]   | `JsonSerializerOptions`  | sealed class  | the mutable-then-frozen contract carrier every overload binds     |
|  [03]   | `JsonSerializerDefaults` | enum          | preset seed — `General` / `Web` / `Strict`                        |
|  [04]   | `JsonException`          | class         | payload and contract failure carrying path, line, and byte offset |

[SERIALIZER_ENTRY_SCOPE]: `JsonSerializer` — each verb takes a `JsonTypeInfo`/`JsonTypeInfo<T>`, a `(Type, JsonSerializerContext)` pair, or `JsonSerializerOptions`; only the options form is reflection-bound

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------------------------- | :------ | :------------------------------------------ |
|  [01]   | `Serialize(object, JsonTypeInfo) -> string`                                   | static  | contract-bound text write                   |
|  [02]   | `SerializeToUtf8Bytes<TValue>(TValue, JsonTypeInfo<TValue>) -> byte[]`        | static  | allocation-bounded UTF-8 write              |
|  [03]   | `SerializeToElement(object, JsonTypeInfo) -> JsonElement`                     | static  | write into the read-only document model     |
|  [04]   | `SerializeToNode(object, JsonTypeInfo) -> JsonNode`                           | static  | write into the mutable node model           |
|  [05]   | `SerializeToDocument(object, JsonTypeInfo) -> JsonDocument`                   | static  | write into a poolable disposable document   |
|  [06]   | `Serialize(Utf8JsonWriter, object, JsonTypeInfo)`                             | static  | write onto a caller-owned writer            |
|  [07]   | `SerializeAsync(Stream, object, JsonTypeInfo, CancellationToken) -> Task`     | static  | streamed async write                        |
|  [08]   | `SerializeAsync(PipeWriter, object, JsonTypeInfo, CancellationToken) -> Task` | static  | pipeline async write                        |
|  [09]   | `Deserialize<TValue>(ReadOnlySpan<byte>, JsonTypeInfo<TValue>) -> TValue?`    | static  | UTF-8 span read, no transcode               |
|  [10]   | `Deserialize<TValue>(ref Utf8JsonReader, JsonTypeInfo<TValue>) -> TValue?`    | static  | resume a partially advanced reader          |
|  [11]   | `Deserialize<TValue>(this JsonElement, JsonTypeInfo<TValue>) -> TValue?`      | static  | project one already-parsed subtree          |
|  [12]   | `DeserializeAsync<TValue>(Stream, JsonTypeInfo<TValue>, CancellationToken)`   | static  | streamed async read                         |
|  [13]   | `DeserializeAsyncEnumerable<TValue>(Stream, JsonTypeInfo<TValue>, bool)`      | static  | streaming element yield off a root sequence |

[OPTIONS_ENTRY_SCOPE]: `JsonSerializerOptions` — construction, the freeze pair, and contract reads

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------------ | :------- | :-------------------------------------------------------------- |
|  [01]   | `JsonSerializerOptions(JsonSerializerDefaults)`         | ctor     | seed from a preset                                              |
|  [02]   | `JsonSerializerOptions(JsonSerializerOptions)`          | ctor     | copy a frozen instance back to mutable                          |
|  [03]   | `Default -> JsonSerializerOptions`                      | property | frozen reflection-backed singleton                              |
|  [04]   | `Web -> JsonSerializerOptions`                          | property | frozen camel-case case-insensitive singleton                    |
|  [05]   | `Strict -> JsonSerializerOptions`                       | property | frozen singleton refusing duplicate and unmapped rows           |
|  [06]   | `MakeReadOnly()`                                        | instance | freeze; a later mutation throws                                 |
|  [07]   | `MakeReadOnly(bool populateMissingResolver)`            | instance | freeze, seeding the reflection resolver when the chain is empty |
|  [08]   | `IsReadOnly -> bool`                                    | property | freeze audit bit                                                |
|  [09]   | `TypeInfoResolver -> IJsonTypeInfoResolver?`            | property | single-resolver slot; writing it resets the chain               |
|  [10]   | `TypeInfoResolverChain -> IList<IJsonTypeInfoResolver>` | property | ordered resolver chain, first non-null wins                     |
|  [11]   | `Converters -> IList<JsonConverter>`                    | property | user converter list, highest precedence                         |
|  [12]   | `GetTypeInfo(Type) -> JsonTypeInfo`                     | instance | resolve and freeze one contract, throwing on absence            |
|  [13]   | `TryGetTypeInfo(Type, out JsonTypeInfo?) -> bool`       | instance | resolve without throwing                                        |
|  [14]   | `GetConverter(Type) -> JsonConverter`                   | instance | resolve the converter the contract binds                        |
|  [15]   | `AddContext<TContext>()`                                | instance | append a generated context to the chain                         |

[OPTIONS_POLICY_SCOPE]: settable posture — every row throws once the instance is frozen

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `PropertyNamingPolicy -> JsonNamingPolicy?`                     | property | member-name projection                           |
|  [02]   | `DictionaryKeyPolicy -> JsonNamingPolicy?`                      | property | dictionary-key projection                        |
|  [03]   | `PropertyNameCaseInsensitive -> bool`                           | property | read-side name matching                          |
|  [04]   | `DefaultIgnoreCondition -> JsonIgnoreCondition`                 | property | member write and read suppression posture        |
|  [05]   | `NumberHandling -> JsonNumberHandling`                          | property | number format tolerance on both directions       |
|  [06]   | `UnmappedMemberHandling -> JsonUnmappedMemberHandling`          | property | `Skip` / `Disallow`                              |
|  [07]   | `PreferredObjectCreationHandling -> JsonObjectCreationHandling` | property | `Replace` / `Populate`                           |
|  [08]   | `UnknownTypeHandling -> JsonUnknownTypeHandling`                | property | `object` root materialization posture            |
|  [09]   | `AllowDuplicateProperties -> bool`                              | property | duplicate-name refusal on read                   |
|  [10]   | `AllowOutOfOrderMetadataProperties -> bool`                     | property | `$id`/`$type` position tolerance                 |
|  [11]   | `AllowTrailingCommas -> bool`                                   | property | trailing-comma tolerance                         |
|  [12]   | `ReadCommentHandling -> JsonCommentHandling`                    | property | comment disposition on read                      |
|  [13]   | `RespectNullableAnnotations -> bool`                            | property | non-nullable reference members refuse `null`     |
|  [14]   | `RespectRequiredConstructorParameters -> bool`                  | property | non-optional constructor parameters are required |
|  [15]   | `ReferenceHandler -> ReferenceHandler?`                         | property | cycle policy: reference ids or null              |
|  [16]   | `IncludeFields -> bool`                                         | property | field members enter the contract                 |
|  [17]   | `IgnoreReadOnlyFields` / `IgnoreReadOnlyProperties` -> `bool`   | property | read-only member suppression                     |
|  [18]   | `Encoder -> JavaScriptEncoder?`                                 | property | escaping policy for string payloads              |
|  [19]   | `MaxDepth -> int`                                               | property | recursion ceiling on both directions             |
|  [20]   | `DefaultBufferSize -> int`                                      | property | rented buffer size on stream and pipe paths      |
|  [21]   | `WriteIndented` / `IndentCharacter` / `IndentSize` / `NewLine`  | property | write-side layout                                |

[JsonIgnoreCondition]: `Never` `Always` `WhenWritingDefault` `WhenWritingNull` `WhenWriting` `WhenReading`
[JsonNumberHandling]: `Strict` `AllowReadingFromString` `WriteAsString` `AllowNamedFloatingPointLiterals`

## [03]-[CONTRACT_MODEL]

[CONTRACT_TYPE_SCOPE]: resolved per-type contracts and the resolvers producing them

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                                |
| :-----: | :---------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `IJsonTypeInfoResolver`       | interface      | `GetTypeInfo(Type, JsonSerializerOptions) -> JsonTypeInfo?` |
|  [02]   | `JsonTypeInfoResolver`        | static class   | combinators over the resolver interface                     |
|  [03]   | `DefaultJsonTypeInfoResolver` | class          | reflection resolver with a mutable modifier list            |
|  [04]   | `JsonTypeInfo`                | abstract class | one resolved type contract, freezable in its own right      |
|  [05]   | `JsonTypeInfo<T>`             | abstract class | the typed contract every generic overload binds             |
|  [06]   | `JsonTypeInfoKind`            | enum           | `None` / `Object` / `Enumerable` / `Dictionary`             |
|  [07]   | `JsonPropertyInfo`            | abstract class | one member contract with get, set, and predicate delegates  |
|  [08]   | `JsonParameterInfo`           | abstract class | one constructor-parameter contract paired to its property   |
|  [09]   | `JsonPolymorphismOptions`     | class          | discriminator name, derived rows, unknown-type handling     |
|  [10]   | `JsonDerivedType`             | struct         | one derived-type row carrying its discriminator             |

[RESOLVER_ENTRY_SCOPE]: `JsonTypeInfoResolver` combinators and `DefaultJsonTypeInfoResolver` state

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Combine(params ReadOnlySpan<IJsonTypeInfoResolver?>)`                 | static   | first-non-null resolution over an ordered set |
|  [02]   | `WithAddedModifier(IJsonTypeInfoResolver, Action<JsonTypeInfo>)`       | static   | wrap a resolver with a post-resolution hook   |
|  [03]   | `DefaultJsonTypeInfoResolver.Modifiers -> IList<Action<JsonTypeInfo>>` | property | modifiers applied in order at resolution      |

[TYPEINFO_ENTRY_SCOPE]: `JsonTypeInfo` construction, member set, callbacks, and its own freeze

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `CreateJsonTypeInfo<T>(JsonSerializerOptions)`                          | static   | mint a contract with no members            |
|  [02]   | `CreateJsonPropertyInfo(Type, string) -> JsonPropertyInfo`              | instance | mint a member row for `Properties`         |
|  [03]   | `Properties -> IList<JsonPropertyInfo>`                                 | property | ordered member set, mutable until frozen   |
|  [04]   | `CreateObject -> Func<object>?`                                         | property | parameterless construction delegate        |
|  [05]   | `OnSerializing` / `OnSerialized` / `OnDeserializing` / `OnDeserialized` | property | `Action<object>?` lifecycle callbacks      |
|  [06]   | `PolymorphismOptions -> JsonPolymorphismOptions?`                       | property | per-contract polymorphism, attribute-free  |
|  [07]   | `OriginatingResolver -> IJsonTypeInfoResolver?`                         | property | which chain member produced this contract  |
|  [08]   | `Kind -> JsonTypeInfoKind`                                              | property | object, enumerable, dictionary, or none    |
|  [09]   | `MakeReadOnly()`                                                        | instance | freeze one contract apart from its options |

[MEMBER_ENTRY_SCOPE]: `JsonPropertyInfo` and `JsonParameterInfo` — the per-member and per-parameter contract rows

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------------ | :------- | :----------------------------------------------------- |
|  [01]   | `Get -> Func<object, object?>?` / `Set -> Action<object, object?>?` | property | member access delegates                                |
|  [02]   | `ShouldSerialize -> Func<object, object?, bool>?`                   | property | per-member write predicate                             |
|  [03]   | `AttributeProvider -> ICustomAttributeProvider?`                    | property | metadata seam a modifier writes and export folds       |
|  [04]   | `CustomConverter -> JsonConverter?`                                 | property | member-scoped converter override                       |
|  [05]   | `IsRequired` / `Order` / `IsExtensionData`                          | property | member admission and layout                            |
|  [06]   | `IsGetNullable` / `IsSetNullable`                                   | property | per-direction nullability the annotation posture reads |
|  [07]   | `AssociatedParameter -> JsonParameterInfo?`                         | property | constructor parameter bound to this member             |
|  [08]   | `JsonParameterInfo.DefaultValue` / `Position` / `IsNullable`        | property | constructor-parameter contract reads                   |

## [04]-[SOURCE_GENERATION]

[SOURCEGEN_TYPE_SCOPE]: the generator's declaration grammar and its emitted resolver

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [CAPABILITY]                                                |
| :-----: | :------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `JsonSerializerContext`                | abstract class | generated partial base implementing `IJsonTypeInfoResolver` |
|  [02]   | `JsonSerializableAttribute`            | sealed class   | one declared root type per attribute                        |
|  [03]   | `JsonSourceGenerationOptionsAttribute` | sealed class   | compile-time mirror of the options posture                  |
|  [04]   | `JsonSourceGenerationMode`             | enum           | `Default` / `Metadata` / `Serialization`                    |
|  [05]   | `JsonKnownNamingPolicy`                | enum           | attribute-expressible naming policy selector                |
|  [06]   | `JsonKnownReferenceHandler`            | enum           | attribute-expressible reference-handler selector            |

[SOURCEGEN_ENTRY_SCOPE]: declaration members — the generator emits one `JsonTypeInfo<T>` property per `[JsonSerializable]` root

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `JsonSerializerContext(JsonSerializerOptions?)`                | ctor     | bind a context to caller-supplied options               |
|  [02]   | `JsonSerializerContext.GetTypeInfo(Type) -> JsonTypeInfo?`     | instance | resolver entry; `null` for an undeclared type           |
|  [03]   | `JsonSerializerContext.Options -> JsonSerializerOptions`       | property | the frozen options the generated contracts bind         |
|  [04]   | `JsonSerializerContext.GeneratedSerializerOptions`             | property | protected generated posture the base folds              |
|  [05]   | `JsonSerializableAttribute(Type)`                              | ctor     | declare one root type                                   |
|  [06]   | `JsonSerializableAttribute.TypeInfoPropertyName -> string?`    | property | rename the emitted contract property                    |
|  [07]   | `JsonSerializableAttribute.GenerationMode`                     | property | per-root override of the context mode                   |
|  [08]   | `JsonSourceGenerationOptionsAttribute(JsonSerializerDefaults)` | ctor     | seed the generated posture from a preset                |
|  [09]   | `JsonSourceGenerationOptionsAttribute.Converters -> Type[]?`   | property | converter types the generated options register          |
|  [10]   | `JsonSourceGenerationOptionsAttribute.UseStringEnumConverter`  | property | emit enums as names without a hand-registered converter |

## [05]-[CONVERTERS_AND_MEMBER_GRAMMAR]

[CONVERTER_TYPE_SCOPE]: the converter hierarchy and the shipped policies

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                                   |
| :-----: | :--------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `JsonConverter`              | abstract class | non-generic root carrying `CanConvert` and `Type`              |
|  [02]   | `JsonConverter<T>`           | abstract class | the typed converter every custom wire implements               |
|  [03]   | `JsonConverterFactory`       | abstract class | open-generic and family converters minted per closed type      |
|  [04]   | `JsonStringEnumConverter`    | class          | enum-as-name factory, naming policy and integer tolerance      |
|  [05]   | `JsonStringEnumConverter<T>` | class          | reflection-free enum-as-name converter for one enum            |
|  [06]   | `JsonNumberEnumConverter<T>` | class          | enum-as-number converter for one enum                          |
|  [07]   | `JsonNamingPolicy`           | abstract class | naming projections over one `ConvertName` hook                 |
|  [08]   | `ReferenceHandler`           | abstract class | `Preserve` and `IgnoreCycles` statics over `ReferenceResolver` |

[JsonNamingPolicy]: `CamelCase` `SnakeCaseLower` `SnakeCaseUpper` `KebabCaseLower` `KebabCaseUpper`

[CONVERTER_ENTRY_SCOPE]: converter overrides — `Read` receives the reader positioned on the value's first token and leaves it on the last

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `Read(ref Utf8JsonReader, Type, JsonSerializerOptions) -> T?`              | instance | value read; abstract                       |
|  [02]   | `Write(Utf8JsonWriter, T, JsonSerializerOptions)`                          | instance | value write; abstract                      |
|  [03]   | `ReadAsPropertyName(ref Utf8JsonReader, Type, JsonSerializerOptions) -> T` | instance | dictionary-key read; virtual               |
|  [04]   | `WriteAsPropertyName(Utf8JsonWriter, T, JsonSerializerOptions)`            | instance | dictionary-key write; virtual              |
|  [05]   | `HandleNull -> bool`                                                       | property | opt into receiving `null` tokens           |
|  [06]   | `CanConvert(Type) -> bool`                                                 | instance | admission predicate; factories narrow here |
|  [07]   | `JsonConverterFactory.CreateConverter(Type, JsonSerializerOptions)`        | instance | mint the closed converter                  |
|  [08]   | `JsonNamingPolicy.ConvertName(string) -> string`                           | instance | the one projection hook                    |

[MEMBER_ATTRIBUTE_SCOPE]: declaration attributes the reflection and generated resolvers both honor

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------ | :------- | :---------------------------------------------------- |
|  [01]   | `JsonPropertyNameAttribute(string)`                                 | ctor     | wire name overriding the naming policy                |
|  [02]   | `JsonPropertyOrderAttribute(int)`                                   | ctor     | write order                                           |
|  [03]   | `JsonIgnoreAttribute.Condition -> JsonIgnoreCondition`              | property | per-member ignore posture                             |
|  [04]   | `JsonIncludeAttribute`                                              | class    | admit a non-public or field member                    |
|  [05]   | `JsonRequiredAttribute`                                             | class    | absence on read is a payload failure                  |
|  [06]   | `JsonConstructorAttribute`                                          | class    | select the deserialization constructor                |
|  [07]   | `JsonConverterAttribute(Type)`                                      | ctor     | member- or type-scoped converter                      |
|  [08]   | `JsonExtensionDataAttribute`                                        | class    | overflow member capturing unmapped rows               |
|  [09]   | `JsonNumberHandlingAttribute(JsonNumberHandling)`                   | ctor     | member- or type-scoped number posture                 |
|  [10]   | `JsonObjectCreationHandlingAttribute(JsonObjectCreationHandling)`   | ctor     | populate an existing instance instead of replacing it |
|  [11]   | `JsonUnmappedMemberHandlingAttribute(JsonUnmappedMemberHandling)`   | ctor     | type-scoped unmapped-member refusal                   |
|  [12]   | `JsonStringEnumMemberNameAttribute(string)`                         | ctor     | per-enum-member wire name                             |
|  [13]   | `JsonPolymorphicAttribute.TypeDiscriminatorPropertyName -> string?` | property | discriminator name; default `$type`                   |
|  [14]   | `JsonPolymorphicAttribute.UnknownDerivedTypeHandling`               | property | unknown derived-type disposition                      |
|  [15]   | `JsonDerivedTypeAttribute(Type, string)`                            | ctor     | one derived row with a string discriminator           |
|  [16]   | `JsonDerivedTypeAttribute(Type, int)`                               | ctor     | one derived row with an integer discriminator         |

[JsonUnknownDerivedTypeHandling]: `FailSerialization` `FallBackToBaseType` `FallBackToNearestAncestor`

## [06]-[DOCUMENT_AND_NODE_MODEL]

[DOCUMENT_TYPE_SCOPE]: the two payload models — one read-only over pooled memory, one mutable tree

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]   | [CAPABILITY]                                             |
| :-----: | :--------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `JsonDocument`                           | sealed class    | pooled read-only parse; disposal returns the buffer      |
|  [02]   | `JsonElement`                            | readonly struct | one node view into a document, no allocation per read    |
|  [03]   | `JsonValueKind`                          | enum            | token classification an element reports                  |
|  [04]   | `JsonNode`                               | abstract class  | mutable tree node with parent, path, and index awareness |
|  [05]   | `JsonObject` / `JsonArray` / `JsonValue` | class           | the three node shapes                                    |
|  [06]   | `JsonNodeOptions`                        | struct          | node-tree case sensitivity                               |
|  [07]   | `JsonDocumentOptions`                    | struct          | parse-side comment, trailing-comma, and depth posture    |

[DOCUMENT_ENTRY_SCOPE]: parse, project, and write-back across both models

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `JsonDocument.Parse(ReadOnlyMemory<byte>, JsonDocumentOptions) -> JsonDocument` | static   | pooled parse over caller-owned memory   |
|  [02]   | `JsonDocument.ParseValue(ref Utf8JsonReader) -> JsonDocument`                   | static   | parse one value off a positioned reader |
|  [03]   | `JsonDocument.RootElement -> JsonElement`                                       | property | root view; invalid after disposal       |
|  [04]   | `JsonElement.TryGetProperty(ReadOnlySpan<byte>, out JsonElement) -> bool`       | instance | UTF-8 member probe, no transcode        |
|  [05]   | `JsonElement.EnumerateArray()` / `EnumerateObject()`                            | instance | allocation-free child walks             |
|  [06]   | `JsonElement.ValueEquals(ReadOnlySpan<byte>) -> bool`                           | instance | UTF-8 string compare, no materialize    |
|  [07]   | `JsonElement.GetString() -> string?`                                            | instance | materialize a string node               |
|  [08]   | `JsonElement.WriteTo(Utf8JsonWriter)`                                           | instance | copy a subtree onto a writer            |
|  [09]   | `JsonNode.Parse(ReadOnlySpan<byte>, JsonNodeOptions?, JsonDocumentOptions)`     | static   | mutable-tree parse                      |
|  [10]   | `JsonNode.this[string]` / `this[int]`                                           | property | member and index addressing             |
|  [11]   | `JsonNode.GetPath() -> string`                                                  | instance | JSON-pointer-style path from the root   |
|  [12]   | `JsonNode.DeepClone() -> JsonNode`                                              | instance | detached copy safe to reparent          |
|  [13]   | `JsonNode.DeepEquals(JsonNode?, JsonNode?) -> bool`                             | static   | structural value comparison             |
|  [14]   | `JsonNode.GetValueKind() -> JsonValueKind`                                      | instance | classify without materializing          |
|  [15]   | `JsonNode.ToJsonString(JsonSerializerOptions?) -> string`                       | instance | render under an options instance        |

## [07]-[LOW_LEVEL_WIRE]

[WIRE_TYPE_SCOPE]: the forward-only reader and writer every converter and hand-rolled codec binds

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                                                     |
| :-----: | :------------------ | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `Utf8JsonReader`    | ref struct      | forward-only UTF-8 token reader over span or sequence            |
|  [02]   | `JsonReaderState`   | readonly struct | resumable state carried across buffer boundaries                 |
|  [03]   | `JsonTokenType`     | enum            | current token classification                                     |
|  [04]   | `Utf8JsonWriter`    | sealed class    | forward-only UTF-8 writer over `IBufferWriter<byte>` or `Stream` |
|  [05]   | `JsonWriterOptions` | struct          | indent, encoder, depth, and validation posture                   |
|  [06]   | `JsonEncodedText`   | readonly struct | pre-escaped name or value cached across writes                   |

[WIRE_ENTRY_SCOPE]: reader advance and writer emission

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------ | :------- | :----------------------------------------------- |
|  [01]   | `Utf8JsonReader.Read() -> bool`                                     | instance | advance one token                                |
|  [02]   | `Utf8JsonReader.TrySkip() -> bool`                                  | instance | skip a whole subtree when the buffer holds it    |
|  [03]   | `Utf8JsonReader.ValueTextEquals(ReadOnlySpan<byte>) -> bool`        | instance | compare a name without decoding                  |
|  [04]   | `Utf8JsonReader.CopyString(Span<byte>) -> int`                      | instance | unescape into caller memory                      |
|  [05]   | `Utf8JsonReader.CurrentState -> JsonReaderState`                    | property | resume token for the next buffer                 |
|  [06]   | `Utf8JsonReader.BytesConsumed -> long`                              | property | advance the source span by this before refilling |
|  [07]   | `Utf8JsonWriter(IBufferWriter<byte>, JsonWriterOptions)`            | ctor     | bind to a pipeline buffer                        |
|  [08]   | `Utf8JsonWriter.Reset(IBufferWriter<byte>)`                         | instance | rebind one writer across payloads                |
|  [09]   | `Utf8JsonWriter.WriteRawValue(ReadOnlySpan<byte>, bool)`            | instance | splice pre-encoded bytes; validation is opt-out  |
|  [10]   | `Utf8JsonWriter.WriteStringValueSegment(ReadOnlySpan<byte>, bool)`  | instance | emit one string across chunks                    |
|  [11]   | `Utf8JsonWriter.WriteBase64StringSegment(ReadOnlySpan<byte>, bool)` | instance | emit one base64 string across chunks             |
|  [12]   | `Utf8JsonWriter.BytesPending -> int` / `BytesCommitted -> long`     | property | flush accounting                                 |
|  [13]   | `JsonEncodedText.Encode(ReadOnlySpan<char>, JavaScriptEncoder?)`    | static   | escape once, write many                          |

- `Utf8JsonWriter.Flush`: pending bytes reach the destination only here or at disposal, so an unflushed writer publishes nothing.
- `Utf8JsonReader`: crosses no `async` boundary as a ref struct — a streamed read owns its buffer loop and carries `CurrentState` forward.

## [08]-[SCHEMA_EXPORT]

[SCHEMA_TYPE_SCOPE]: exporter, its knob owner, and the per-node transform payload

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                  |
| :-----: | :-------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `JsonSchemaExporter`        | static class    | extension host for both export overloads      |
|  [02]   | `JsonSchemaExporterOptions` | sealed class    | init-only nullability and transform knobs     |
|  [03]   | `JsonSchemaExporterContext` | readonly struct | per-node addressing payload a transform reads |

[SCHEMA_ENTRY_SCOPE]: `JsonSchemaExporter` export — both overloads are extensions; a `null` exporter-options argument binds `JsonSchemaExporterOptions.Default`

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------------------- | :------ | :------------------------------ |
|  [01]   | `GetJsonSchemaAsNode(JsonSerializerOptions, Type, JsonSchemaExporterOptions)` | static  | resolves the type, then exports |
|  [02]   | `GetJsonSchemaAsNode(JsonTypeInfo, JsonSchemaExporterOptions)`                | static  | exports a pre-resolved contract |

[SCHEMA_KNOB_SCOPE]: `JsonSchemaExporterOptions` knobs (init-only) and the `JsonSchemaExporterContext` reads a transform folds over

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Default -> JsonSchemaExporterOptions`                     | property | zero-knob instance                          |
|  [02]   | `TreatNullObliviousAsNonNullable -> bool`                  | property | null-oblivious reference reads non-nullable |
|  [03]   | `TransformSchemaNode -> Func<Context, JsonNode, JsonNode>` | property | per-node post-transform seam                |
|  [04]   | `Context.Path -> ReadOnlySpan<string>`                     | property | node location as pointer segments           |
|  [05]   | `Context.TypeInfo -> JsonTypeInfo`                         | property | contract of the node's declared type        |
|  [06]   | `Context.PropertyInfo -> JsonPropertyInfo`                 | property | owning property, `null` off a member        |
|  [07]   | `Context.BaseTypeInfo -> JsonTypeInfo`                     | property | polymorphic base at a derived branch        |

- `JsonSchemaExporter.GetJsonSchemaAsNode`: both overloads call `JsonSerializerOptions.MakeReadOnly()` first, freezing the options instance against a later converter or resolver mutation.

## [09]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One options instance is one wire contract: the first serialize, deserialize, or export freezes it, every later mutation throws `InvalidOperationException`, and `IsReadOnly` is the audit bit a composition asserts rather than a comment.
- `MakeReadOnly()` freezes without seeding a resolver, so an options instance carrying an empty chain throws on first use; `MakeReadOnly(populateMissingResolver: true)` seeds `DefaultJsonTypeInfoResolver` and is the reflection-admitting arm, never the default.
- Resolution precedence runs converters, then the resolver chain in order, first non-null winning — so two contexts resolving one type is an order-dependent fork, not a fallback, and disjointness proves at composition.
- `Web` sets camel-case naming, case-insensitive reads, and number-from-string tolerance; `Strict` refuses duplicate properties, out-of-order metadata, and unmapped members. Both are frozen singletons — a knob change copies through `new JsonSerializerOptions(source)`.
- `RespectNullableAnnotations` and `RespectRequiredConstructorParameters` read nullable metadata off the contract, so a generated context and a reflection resolver reach the same refusal only when both are set on the same instance.
- `ReferenceHandler.Preserve` writes `$id`/`$ref` and rejects schema export; `IgnoreCycles` writes `null` at the cycle and exports cleanly.
- Attributes decide nothing the contract model cannot: a resolver modifier reaches `JsonTypeInfo.Properties`, `PolymorphismOptions`, and every delegate slot, so runtime-computed shape needs no attribute and no second options instance.
- `JsonSerializerContext.GetTypeInfo` returns `null` for an undeclared type rather than throwing, so a chained context contributes its declared roots and defers the rest.
- `JsonDocument` rents pooled memory — every `JsonElement` view faults after disposal, and a value outliving the document clones through `JsonElement.Clone` or crosses as `JsonNode`.
- Schema export walks the contract: a repeated `(JsonTypeInfo, JsonPropertyInfo)` pair emits `{"$ref": "<json-pointer>"}` to its first occurrence so a recursive graph terminates, nesting past `MaxDepth` throws, and a converter-backed contract with no built-in mapping exports the unconstrained `true` node — `TransformSchemaNode` is the sole route to a described shape for a custom-converter type, running bottom-up with its return replacing the node.

[STACKING]:
- `Thinktecture.Runtime.Extensions.Json`(`.api/api-thinktecture-json.md`): `ThinktectureJsonConverterFactory` on `JsonSerializerOptions.Converters` projects every `[SmartEnum]`/`[ValueObject]` key onto the wire before the freeze, and the generated converter describes no schema, so its owner exports as `true` until a `TransformSchemaNode` arm keyed on `JsonSchemaExporterContext.TypeInfo` writes the key or string form.
- `NodaTime.Serialization.SystemTextJson`(`.api/api-nodatime-stj.md`): `ConfigureForNodaTime` registers the pattern converters onto the same options instance the freeze seals and export reads, so `Instant` takes one transform arm rather than a second date policy.
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson`(`.api/api-jsonpatch.md`): one frozen options instance drives both seams — export describes the record and RFC 6902 application mutates its live `JsonObject` — splitting contract projection from structured edit without forking configuration.
- `Microsoft.Extensions.AI`(`.api/api-extensions-ai.md`): `AIJsonUtilities.CreateJsonSchema` and `CreateFunctionJsonSchema` project one `JsonSerializerOptions` contract into the `JsonElement` that `ChatResponseFormat.ForJsonSchema` and `AIFunctionDeclaration.JsonSchema` both bind, so wire, tool manifest, and structured-output schema cannot drift.
- `System.IO.Hashing`(`.api/api-hashing.md`): `JsonSerializer.SerializeToUtf8Bytes(value, typeInfo)` hands the canonical UTF-8 payload straight to `ContentHash.Of`, minting the one content key a schema identity and its bytes share.
- `Google.Protobuf`(`.api/api-protobuf.md`): the binary contract and the suite JSON contract are disjoint codecs over one shape, so a wire surface declares exactly one and a second observed codec is a composition conflict receipt, never a re-encode fallback.
- `Rasm` `Drawing/pack`: `EvidenceWire.Json` freezes `DDoubleJsonConverter` into one static read-only options identity beside the exact binary block, so the JSON and binary evidence lanes carry the same 106-bit value.
- `Rasm.AppHost` `Runtime/ports`: `SuiteContracts.Wire` merges the residual package `JsonSerializerContext`s — surfaces no `rasm.contracts` family carries, the discovery manifest among them — through `JsonTypeInfoResolver.Combine`, seeds from the `Strict` preset, and freezes with `MakeReadOnly()` at the mint; every generated message crosses as ProtoJSON through `WireJson` instead, and no schema export pins a contract.
- `Rasm.Compute` `Runtime/tiles#TILE_PARTITION`: `Utf8JsonWriter.BytesPending` gates the manifest commit over a pooled `RecyclableMemoryStream`, so a tileset emit flushes on the writer's own buffered measure rather than a guessed chunk size.
- `Rasm.Compute` `Model/run`: `JsonDocument.Parse` over the ONNX chrome trace reads execution evidence no managed session member exposes — `EnumerateArray` walks node events and `TryGetProperty` folds each event's assigned provider into the graph-partition count a stage result publishes — and the document disposes inside the fold, so no `JsonElement` view outlives the pooled rental.
- Richest composition: `JsonTypeInfoResolver.WithAddedModifier` seeds `JsonPropertyInfo.AttributeProvider` onto the resolved contract, `TransformSchemaNode` reads it back through `JsonSchemaExporterContext.PropertyInfo` and gates each annotation to a subtree by `Path`, so effect and cost metadata rides the resolver chain instead of a post-walk over the emitted tree.

[LOCAL_ADMISSION]:
- Every wire surface below the composition root declares its contract through a `JsonSerializerContext`; the reflection resolver enters only where an app root admits it explicitly through `MakeReadOnly(populateMissingResolver: true)`.
- AppHost native capability schemas resolve from source-generated `JsonTypeInfo` through `AIJsonUtilities`; published MCP schemas remain verbatim.

[RAIL_LAW]:
- Package: `System.Text.Json`
- Owns: the branch JSON wire whole — contract resolution and freezing, converter dispatch, polymorphic discrimination, source-generated metadata, document and node models, the forward-only reader and writer, and JSON Schema projection of a live contract
- Accept: generated-context chains, resolver modifiers, member and parameter contract mutation, typed converters over the reader and writer, frozen options identities, `TransformSchemaNode` annotation arms
- Reject: a per-call options graph, a hand-mirrored schema literal, a reflection walk over the CLR type, a second serializer configuration built for one seam, string concatenation onto a JSON payload
