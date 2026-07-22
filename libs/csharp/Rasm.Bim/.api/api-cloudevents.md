# [RASM_BIM_API_CLOUDEVENTS]

`CloudNative.CloudEvents` is the CNCF CloudEvents 1.0 envelope model — the mutable `CloudEvent`
carrier over a typed attribute system (`CloudEventAttribute` rows bound to `CloudEventAttributeType`
value spaces), the spec-version vocabulary, and the abstract `CloudEventFormatter` codec contract —
and `CloudNative.CloudEvents.SystemTextJson` is its STJ realization: `JsonEventFormatter` encodes
and decodes the structured-mode `application/cloudevents+json` body and the binary-mode data
payload. The pair owns envelope shape and JSON serialization only; transport bindings (Kafka, MQTT,
AMQP, HTTP) are sibling packages the app composition admits, never this folder.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents`
- package: `CloudNative.CloudEvents`
- license: Apache-2.0
- assembly: `CloudNative.CloudEvents`
- namespace: `CloudNative.CloudEvents` (+ `.Extensions`, `.Core`, `.Http`)
- asset: `net10.0`
- rail: event envelope

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.SystemTextJson`
- package: `CloudNative.CloudEvents.SystemTextJson`
- license: Apache-2.0
- assembly: `CloudNative.CloudEvents.SystemTextJson`
- namespace: `CloudNative.CloudEvents.SystemTextJson`
- asset: `net10.0`
- serialization: `System.Text.Json` structured-mode and batch-mode JSON
- rail: event codec

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: envelope and attribute model
- namespace: `CloudNative.CloudEvents`
- rail: event envelope

| [INDEX] | [SYMBOL]                  | [CAPABILITY]                                                                                  |
| :-----: | :------------------------ | :-------------------------------------------------------------------------------------------- |
|  [01]   | `CloudEvent`              | sealed envelope                                                                               |
|  [02]   | `CloudEventAttribute`     | one typed attribute row                                                                       |
|  [03]   | `CloudEventAttributeType` | closed value-space vocabulary                                                                 |
|  [04]   | `CloudEventsSpecVersion`  | spec vocabulary                                                                               |
|  [05]   | `CloudEventFormatter`     | abstract codec — structured/binary/batch encode-decode contract every wire formatter realizes |

- [01]: `Id`, `Source` (`Uri?`), `Type`, `Subject`, `Time` (`DateTimeOffset?`), `DataContentType`, `DataSchema`, `Data` (`object?`)
- [02]: `Name`, `Type`, `IsRequired`, `IsExtension`, `Parse(string)`, `Format(object)`, `Validate(object)`
- [03]: statics `String`, `Boolean`, `Integer`, `Binary`, `Uri`, `UriReference`, `Timestamp`
- [04]: `V1_0`, `Default`, per-attribute rows (`TypeAttribute`, `SourceAttribute`, …), `FromVersionId(string?)`

[PUBLIC_TYPE_SCOPE]: STJ formatter
- namespace: `CloudNative.CloudEvents.SystemTextJson`
- rail: event codec

| [INDEX] | [SYMBOL]                | [CAPABILITY]                                                                         |
| :-----: | :---------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `JsonEventFormatter`    | `: CloudEventFormatter` — structured-mode JSON codec                                 |
|  [02]   | `JsonEventFormatter<T>` | data-typed variant serializing `Data` as `T` through the supplied serializer options |

- [01]: optional `(JsonSerializerOptions?, JsonDocumentOptions)` ctor

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: envelope construction and attribute reach
- namespace: `CloudNative.CloudEvents`
- rail: event envelope

| [INDEX] | [MEMBER]                              | [CAPABILITY]                                                                           |
| :-----: | :------------------------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `new CloudEvent`                      | envelope minted with its declared extension-attribute roster                           |
|  [02]   | `this[CloudEventAttribute attribute]` | typed attribute read/write — the extension stamp path (`object?` get/set)              |
|  [03]   | `CloudEvent.SetAttributeFromString`   | wire-string admission through the attribute's own `Parse`                              |
|  [04]   | `CloudEvent.GetPopulatedAttributes()` | `IEnumerable<KeyValuePair<CloudEventAttribute, object>>` census of set attributes      |
|  [05]   | `CloudEvent.Validate()`               | returns the envelope after asserting every spec-required attribute is populated        |
|  [06]   | `CloudEvent.IsValid`                  | non-throwing required-attribute probe                                                  |
|  [07]   | `CloudEventAttribute.CreateExtension` | extension-attribute mint — the distributed-tracing `traceparent`/`tracestate` row path |

- [01]: `new CloudEvent(CloudEventsSpecVersion specVersion, IEnumerable<CloudEventAttribute>? extensionAttributes)`
- [02]: `this[CloudEventAttribute attribute]` / `this[string attributeName]`
- [03]: `CloudEvent.SetAttributeFromString(string name, string value)`
- [07]: `CloudEventAttribute.CreateExtension(string name, CloudEventAttributeType type)`

[ENTRYPOINT_SCOPE]: structured-mode JSON codec
- namespace: `CloudNative.CloudEvents.SystemTextJson`
- rail: event codec

| [INDEX] | [MEMBER]                                                  | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `EncodeStructuredModeMessage`                             | one-body JSON envelope emit (`application/cloudevents+json`) |
|  [02]   | `DecodeStructuredModeMessage`                             | one-body decode admitting declared extensions                |
|  [03]   | `EncodeBatchModeMessage`                                  | batch emit                                                   |
|  [04]   | `DecodeBatchModeMessage`                                  | batch decode                                                 |
|  [05]   | `ConvertToJsonElement` / `ConvertFromJsonElement`         | in-memory `JsonElement` bridge                               |
|  [06]   | `EncodeBinaryModeEventData` / `DecodeBinaryModeEventData` | binary-mode data-only codec for transport bindings           |

- [01]: `EncodeStructuredModeMessage(CloudEvent cloudEvent, out ContentType contentType)` → `ReadOnlyMemory<byte>`
- [02]: `DecodeStructuredModeMessage(ReadOnlyMemory<byte> body, ContentType? contentType, IEnumerable<CloudEventAttribute>? extensionAttributes)` → `CloudEvent`
- [03]: `EncodeBatchModeMessage(IEnumerable<CloudEvent> cloudEvents, out ContentType contentType)` → `ReadOnlyMemory<byte>`
- [04]: `DecodeBatchModeMessage(ReadOnlyMemory<byte> body, ContentType? contentType, IEnumerable<CloudEventAttribute>? extensionAttributes)` → `IReadOnlyList<CloudEvent>`
- [05]: `ConvertToJsonElement(CloudEvent cloudEvent)` / `ConvertFromJsonElement(JsonElement, IEnumerable<CloudEventAttribute>?)`
- [06]: `EncodeBinaryModeEventData(CloudEvent cloudEvent)` / `DecodeBinaryModeEventData(ReadOnlyMemory<byte>, CloudEvent)`

## [04]-[STACKING]

- `Data` set to a `JsonElement` serializes natively through the formatter with zero reflection
  metadata — a source-generated `JsonSerializerContext` projects the domain payload to a
  `JsonElement` first, and the formatter's structured-mode body carries it verbatim; on decode the
  `data` property lands back as a `JsonElement` the same context deserializes.
- Extension attributes are declaration-scoped: an attribute passed to the `CloudEvent` ctor and to
  every `DecodeStructuredModeMessage` call round-trips typed; an undeclared extension decodes as a
  string-typed surprise row, so the declared roster is the wire contract.
- `Timestamp` attribute values are RFC 3339 `DateTimeOffset` — a NodaTime `Instant` crosses through
  `Instant.ToDateTimeOffset()` at the seal and never as a formatted string.
- The 2.x SDK ships no distributed-tracing extension class — the CloudEvents distributed-tracing
  spec extension is two `CreateExtension` string rows (`traceparent`, `tracestate`) stamped from
  `Activity.Current.Id`/`Activity.Current.TraceStateString` under W3C id format.
- Transport bindings (`CloudNative.CloudEvents.Kafka`, `.Mqtt`, `.Amqp`) consume the same
  `CloudEventFormatter` instance for binary-mode content — the formatter is the one codec identity
  every transport shares, minted once as a `static readonly` field.
