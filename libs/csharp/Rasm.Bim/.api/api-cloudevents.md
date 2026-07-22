# [RASM_BIM_API_CLOUDEVENTS]

`CloudNative.CloudEvents` owns the CNCF CloudEvents 1.0 envelope: the sealed-yet-mutable `CloudEvent` over a typed attribute system, the `CloudEventsSpecVersion` vocabulary, and the abstract `CloudEventFormatter` codec every wire formatter realizes. `CloudNative.CloudEvents.SystemTextJson` realizes it as `JsonEventFormatter`, encoding the structured-mode `application/cloudevents+json` body and the binary-mode data payload; envelope shape and JSON serialization are the whole surface, and transport bindings lower onto the same formatter at the app tier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents`
- package: `CloudNative.CloudEvents` (Apache-2.0, `cloudevents/sdk-csharp`)
- assembly: `CloudNative.CloudEvents` (`net10.0`)
- namespace: `CloudNative.CloudEvents` (+ `.Extensions`, `.Core`, `.Http`)
- rail: event envelope

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.SystemTextJson`
- package: `CloudNative.CloudEvents.SystemTextJson` (Apache-2.0, `cloudevents/sdk-csharp`)
- assembly: `CloudNative.CloudEvents.SystemTextJson` (`net10.0`)
- namespace: `CloudNative.CloudEvents.SystemTextJson`
- rail: `System.Text.Json` event codec

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: envelope and attribute model (`CloudNative.CloudEvents`)

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                                                 |
| :-----: | :------------------------ | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `CloudEvent`              | sealed class   | mutable envelope over the typed attribute map                                |
|  [02]   | `CloudEventAttribute`     | class          | one typed attribute row (`Name`, `Type`, `IsRequired`, `IsExtension`)        |
|  [03]   | `CloudEventAttributeType` | abstract class | closed value-space vocabulary                                                |
|  [04]   | `CloudEventsSpecVersion`  | sealed class   | spec-version vocabulary and required/optional attribute roster               |
|  [05]   | `CloudEventFormatter`     | abstract class | structured/binary/batch encode-decode contract every wire formatter realizes |

- `CloudEvent` attributes: `Id` `Type` `Subject` `DataContentType` are `string?`, `Source` and `DataSchema` are `Uri?`, `Time` is `DateTimeOffset?`, `Data` is `object?`.
- `[CloudEventAttributeType]`: `String` `Boolean` `Integer` `Binary` `Uri` `UriReference` `Timestamp` — the closed value-space anchors.
- `[CloudEventsSpecVersion]`: `V1_0` `Default` `FromVersionId(string?)`, the per-attribute rows `IdAttribute`/`SourceAttribute`/`TypeAttribute`/`TimeAttribute`/…, and the `RequiredAttributes`/`OptionalAttributes`/`AllAttributes` rosters.

[PUBLIC_TYPE_SCOPE]: STJ formatter (`CloudNative.CloudEvents.SystemTextJson`)

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                                          |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `JsonEventFormatter`    | class         | structured/binary/batch JSON codec realizing `CloudEventFormatter`                    |
|  [02]   | `JsonEventFormatter<T>` | class         | data-typed variant serializing `Data` as `T` through supplied `JsonSerializerOptions` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: envelope construction and attribute reach (`CloudNative.CloudEvents`) — `new CloudEvent(CloudEventsSpecVersion, IEnumerable<CloudEventAttribute>?)` mints the envelope; `GetPopulatedAttributes()` returns `IEnumerable<KeyValuePair<CloudEventAttribute, object>>`.

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `new CloudEvent(...)`                                                  | ctor     | envelope minted with its extension roster  |
|  [02]   | `CloudEvent[CloudEventAttribute]`                                      | property | typed attribute get/set (`object?`)        |
|  [03]   | `CloudEvent.SetAttributeFromString(string, string)`                    | instance | wire-string admission via the `Parse` path |
|  [04]   | `CloudEvent.GetPopulatedAttributes()`                                  | instance | set-attribute census                       |
|  [05]   | `CloudEvent.Validate() -> CloudEvent`                                  | instance | asserts required attributes, returns self  |
|  [06]   | `CloudEvent.IsValid`                                                   | property | non-throwing required-attribute probe      |
|  [07]   | `CloudEventAttribute.CreateExtension(string, CloudEventAttributeType)` | factory  | extension-attribute mint                   |
|  [08]   | `CloudEventAttribute.Parse(string)` / `.Format(object)`                | instance | attribute wire parse and format            |

- `CloudEventAttribute.CreateExtension` carries a second `(string, CloudEventAttributeType, Action<object>?)` overload admitting a custom validator.

[ENTRYPOINT_SCOPE]: structured-mode JSON codec, instance overrides on `JsonEventFormatter` (`CloudNative.CloudEvents.SystemTextJson`) — decode surfaces take `(ReadOnlyMemory<byte> body, ContentType?, IEnumerable<CloudEventAttribute>? extensions)`; encode surfaces take the event(s) and an `out ContentType`.

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------ | :-------------------------------------------- |
|  [01]   | `EncodeStructuredModeMessage(...) -> ReadOnlyMemory<byte>`          | one-body `application/cloudevents+json` emit  |
|  [02]   | `DecodeStructuredModeMessage(...) -> CloudEvent`                    | one-body decode admitting declared extensions |
|  [03]   | `EncodeBatchModeMessage(...) -> ReadOnlyMemory<byte>`               | batch emit                                    |
|  [04]   | `DecodeBatchModeMessage(...) -> IReadOnlyList<CloudEvent>`          | batch decode                                  |
|  [05]   | `ConvertToJsonElement(CloudEvent)` / `ConvertFromJsonElement(...)`  | in-memory `JsonElement` bridge                |
|  [06]   | `EncodeBinaryModeEventData(...)` / `DecodeBinaryModeEventData(...)` | binary-mode data-only codec                   |

- `Stream`-bodied `DecodeStructuredModeMessageAsync`/`DecodeBatchModeMessageAsync` overloads carry the same contract for a streamed body.

[ENTRYPOINT_SCOPE]: typed extension-attribute helpers, static extension methods on `CloudEvent` (`CloudNative.CloudEvents.Extensions`).

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `Partitioning.SetPartitionKey(string?)` / `.GetPartitionKey()` | `partitionkey` extension round-trip          |
|  [02]   | `Sampling.SetSampledRate(int?)` / `.GetSampledRate()`          | `sampledrate` positive-integer extension     |
|  [03]   | `Sequence.SetSequence(object?)` / `.GetSequenceString()`       | `sequence`/`sequencetype` ordering extension |

- Each helper exposes its `*Attribute` singleton and an `AllAttributes` roster for the declared-extension path a decode requires; `Sequence` adds `.GetSequenceType()` and `.GetSequenceValue()`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `CloudEvent` is a typed attribute map keyed by `CloudEventAttribute`; every read and write folds through the attribute's `Validate`, admitting or rejecting a value against its declared `CloudEventAttributeType` at the set boundary, and the extension roster passed to the ctor and to every decode is the wire contract — an undeclared extension decodes as a string-typed row.

[STACKING]:
- `NodaTime`(`libs/csharp/.api/api-nodatime.md`): the `time` attribute (`CloudEventAttributeType.Timestamp`, RFC 3339 `DateTimeOffset`) takes a NodaTime `Instant` through `Instant.ToDateTimeOffset()` at the seal, never a formatted string.
- `JsonEventFormatter` composes `System.Text.Json`: a source-generated `JsonSerializerContext` projects the domain payload to a `JsonElement` set as `Data`, and the structured-mode body carries it verbatim with zero reflection metadata; on decode `data` lands back as a `JsonElement` the same context deserializes, and `JsonEventFormatter<T>` serializes `Data` as `T` directly through supplied `JsonSerializerOptions`.
- Distributed-tracing continuity rides two `CloudEventAttribute.CreateExtension` string rows (`traceparent`, `tracestate`) stamped from `Activity.Current.Id`/`.TraceStateString` under W3C id format — the declared-attribute pattern the SDK's `Partitioning`/`Sampling`/`Sequence` helpers own, extended in place rather than forked per transport.

[LOCAL_ADMISSION]:
- envelope shape and JSON serialization enter through this pair; transport bindings (Kafka, MQTT, AMQP) consume the same `CloudEventFormatter` instance at the app tier and never enter this folder, and the formatter is minted once as a `static readonly` codec identity every transport shares.

[RAIL_LAW]:
- Package: `CloudNative.CloudEvents` + `CloudNative.CloudEvents.SystemTextJson` (Apache-2.0, pure-managed `net10.0`)
- Owns: the CloudEvents 1.0 envelope (`CloudEvent`), the typed attribute system (`CloudEventAttribute`/`CloudEventAttributeType`), the `CloudEventsSpecVersion` vocabulary, and the STJ `JsonEventFormatter` structured/binary/batch codec.
- Accept: a `BimEvent` lowered onto a `CloudEvent` with its declared extension roster, `Data` carried as a source-generated `JsonElement`, W3C trace continuity as `traceparent`/`tracestate` extension rows, and JSON emit/decode through the shared `JsonEventFormatter`.
- Reject: an undeclared extension left to decode as a string-typed surprise row; a hand-built JSON envelope where `JsonEventFormatter` owns structured mode; a formatted timestamp string where `Instant.ToDateTimeOffset()` seals the `time` attribute; a per-transport formatter instance where one `static readonly` identity serves all.
