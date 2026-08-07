# [RASM_BIM_API_CLOUDEVENTS]

`CloudNative.CloudEvents` binds the CloudEvents 1.0 envelope algebra — one `CloudEvent` carrying a spec-version-scoped typed attribute map, per-instance extension attributes, and a `Data` payload — and `CloudNative.CloudEvents.SystemTextJson` binds the `JsonEventFormatter` rendering and parsing it in structured and batch mode over `System.Text.Json`. `Exchange/events#EVENTS` composes both, and the Kafka, MQTT, and AMQP bindings stay outside this folder's closure.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents` + `CloudNative.CloudEvents.SystemTextJson`
- packages: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.SystemTextJson` (both Apache-2.0, direct `PackageReference`)
- assembly: `CloudNative.CloudEvents` / `CloudNative.CloudEvents.SystemTextJson`
- namespace: `CloudNative.CloudEvents` (envelope, attribute, spec-version, formatter base), `.Core` (`MimeUtilities`/`BinaryDataUtilities`/`Validation`), `.Extensions` (`Partitioning`/`Sampling`/`Sequence`), `.SystemTextJson` (`JsonEventFormatter`/`JsonEventFormatter<T>`)
- asset: `net10.0` beside `net8.0` and `netstandard2.0`; the consumer binds `lib/net10.0` — pure-managed AnyCPU IL, no native asset
- depends: the SystemTextJson binding on `System.Text.Json` alone; the core on the BCL
- consumer: `libs/csharp/Rasm.Bim` (the `BimEvent` emit lacing), `libs/csharp/Rasm.Persistence` (the durable outbox row)
- rail: event envelope

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: envelope and attribute algebra

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                                          |
| :-----: | :------------------------ | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `CloudEvent`              | sealed class   | one event — spec version, typed attribute map, extensions, `Data`     |
|  [02]   | `CloudEventAttribute`     | class          | one attribute's name, `Type`, required/extension flags, validator     |
|  [03]   | `CloudEventAttributeType` | abstract class | the attribute value spaces; `Parse`/`Format`/`Validate` per type      |
|  [04]   | `CloudEventsSpecVersion`  | sealed class   | a spec version and its per-attribute singletons and rosters           |
|  [05]   | `CloudEventFormatter`     | abstract class | the codec contract — structured, binary, and batch in both directions |
|  [06]   | `ContentMode`             | enum           | `Structured`/`Binary` — how a binding carries the event               |

- [01]-[EVENT_CTORS]: `CloudEvent()`, `(CloudEventsSpecVersion)`, `(IEnumerable<CloudEventAttribute>?)`, `(CloudEventsSpecVersion, IEnumerable<CloudEventAttribute>?)` — the extension roster handed at construction IS the declared set.
- [01]-[EVENT_MEMBERS]: `SpecVersion`, the two indexers `this[CloudEventAttribute]` and `this[string]`, the typed slots `Data`/`DataContentType`/`Id`/`DataSchema`/`Source`/`Subject`/`Time`/`Type`, `ExtensionAttributes`, `IsValid`, `GetAttribute(string)`, `GetPopulatedAttributes()`, `SetAttributeFromString(string, string)`, and `Validate()` → `CloudEvent`, which THROWS on a malformed envelope and returns the event on success so it chains.
- [02]-[ATTRIBUTE]: `Type`/`Name`/`IsRequired`/`IsExtension`; statics `CreateExtension(string, CloudEventAttributeType)` and `CreateExtension(string, CloudEventAttributeType, Action<object>?)`; instance `Parse(string)`/`Format(object)`/`Validate(object)`. The required/optional factories are `internal` — only the spec-version singletons mint those.
- [03]-[ATTRIBUTE_TYPE]: the value spaces are `Binary`/`Boolean`/`Integer`/`String`/`Timestamp`/`Uri`/`UriReference`; `Timestamp` carries a `DateTimeOffset` under RFC 3339.
- [04]-[SPEC_VERSION]: `V1_0` and `Default`; `VersionId`; the per-attribute singletons `IdAttribute`/`SourceAttribute`/`TypeAttribute`/`DataContentTypeAttribute`/`DataSchemaAttribute`/`SubjectAttribute`/`TimeAttribute`; the rosters `RequiredAttributes`/`OptionalAttributes`/`AllAttributes`; `SpecVersionAttribute`; `FromVersionId(string?)`.
- [05]-[FORMATTER]: abstract `DecodeStructuredModeMessage(ReadOnlyMemory<byte>, ContentType?, IEnumerable<CloudEventAttribute>?)`, `EncodeStructuredModeMessage(CloudEvent, out ContentType)`, `DecodeBatchModeMessage(ReadOnlyMemory<byte>, ContentType?, IEnumerable<CloudEventAttribute>?)`, `EncodeBatchModeMessage(IEnumerable<CloudEvent>, out ContentType)`, `DecodeBinaryModeEventData`/`EncodeBinaryModeEventData`; virtual `Stream` and `Async` overloads beside each; `GetOrInferDataContentType(CloudEvent)`.

[PUBLIC_TYPE_SCOPE]: the System.Text.Json codec and the shared utilities

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `JsonEventFormatter`    | class         | `: CloudEventFormatter` — the STJ structured/binary/batch codec |
|  [02]   | `JsonEventFormatter<T>` | class         | the same codec serializing `Data` as `T` directly               |
|  [03]   | `MimeUtilities`         | static class  | the CloudEvents media-type constants and probes                 |
|  [04]   | `BinaryDataUtilities`   | static class  | body/stream narrowing helpers the formatters share              |
|  [05]   | `Validation`            | static class  | the argument gates the envelope raises through                  |
|  [06]   | `Partitioning`          | static class  | the `partitionkey` standard extension                           |
|  [07]   | `Sampling`              | static class  | the `sampledrate` standard extension                            |
|  [08]   | `Sequence`              | static class  | the `sequence`/`sequencetype` standard extension                |

- [01]-[JSON_FORMATTER]: ctors `JsonEventFormatter()` and `(JsonSerializerOptions?, JsonDocumentOptions)`, and overrides for every abstract and virtual member above.
- [01]-[JSON_NATIVE]: `ConvertToJsonElement(CloudEvent)` → `JsonElement` and `ConvertFromJsonElement(JsonElement, IEnumerable<CloudEventAttribute>?)` → `CloudEvent` skip the byte body entirely; the protected `DecodeStructuredModeDataProperty`/`DecodeStructuredModeDataBase64Property` hooks, the `SerializerOptions`/`DocumentOptions` they read, and the `data`/`data_base64` names ride as protected members.
- [03]-[MIME]: `MediaType` (`application/cloudevents`) and `BatchMediaType` (`application/cloudevents-batch`) — each formatter EXTENDS them with its own format suffix, so a batch probe reads the prefix rather than a literal; `GetEncoding(ContentType?)`, `ToContentType(MediaTypeHeaderValue?)`, `ToMediaTypeHeaderValue(ContentType?)`, `CreateContentTypeOrNull(string?)`, `IsCloudEventsContentType(string?)`, `IsCloudEventsBatchContentType(string?)`.
- [06]-[STANDARD_EXTENSIONS]: each helper publishes its own `AllAttributes` roster beside a `Set*`/`Get*` pair, so a dimension the SDK already owns joins a declared roster from the package rather than a hand-spelled `CreateExtension` row.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: mint, emit, and admit one envelope or a batch

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `new CloudEvent(CloudEventsSpecVersion, IEnumerable<CloudEventAttribute>?)`    | ctor     | mint under a declared roster           |
|  [02]   | `event[attribute] = value` / `event[name]`                                     | indexer  | set or fetch one attribute value       |
|  [03]   | `event.Validate()` → `CloudEvent`                                              | instance | gate the envelope; THROWS on malformed |
|  [04]   | `formatter.EncodeStructuredModeMessage(CloudEvent, out ContentType)`           | instance | one envelope to a structured body      |
|  [05]   | `formatter.EncodeBatchModeMessage(IEnumerable<CloudEvent>, out ContentType)`   | instance | many envelopes to a batch array        |
|  [06]   | `formatter.DecodeStructuredModeMessage(ReadOnlyMemory<byte>, ContentType?, …)` | instance | a structured body back to one event    |
|  [07]   | `formatter.DecodeBatchModeMessage(ReadOnlyMemory<byte>, ContentType?, …)`      | instance | a batch array back to many events      |
|  [08]   | `CloudEventAttribute.CreateExtension(string, CloudEventAttributeType)`         | static   | declare an extension attribute         |
|  [09]   | `MimeUtilities.BatchMediaType`                                                 | static   | the batch prefix a framing probe reads |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The extension roster handed to the ctor AND to every decode IS the wire contract: an attribute declared for a write and forgotten at its read decodes as an untyped string row, so one roster is spelled once and passed at both ends.
- `Validate()` throws rather than railing, so an envelope mint is a BOUNDARY: it captures through one `Try.lift` funnel onto the typed rail and a caller composing events never has a construction fault escape past its own `Fin` signature.
- The `out ContentType` each encode member yields is the framing an app-tier transport binding stamps AND the discriminant a decode reads to pick its reader, so both travel as one carrier value and neither half is re-derived.
- Batch framing is the `BatchMediaType` PREFIX each formatter extends with its own suffix, so a framing probe reads the prefix constant and never a literal media type.
- `Data` set as a `JsonElement` carries verbatim into the structured body with zero reflection metadata, and decode lands it back as a `JsonElement` the same source-generated context deserializes.

[STACKING]:
- `NodaTime`(`libs/csharp/.api/api-nodatime.md`): the `time` attribute is `CloudEventAttributeType.Timestamp` over a `DateTimeOffset`, so a NodaTime `Instant` seals through `Instant.ToDateTimeOffset()` and never a formatted string.
- `System.Text.Json`: a source-generated `JsonSerializerContext` projects the domain payload to a `JsonElement`, keeping the formatter reflection-free under a trimmed or AOT publish.
- `System.Diagnostics.DiagnosticSource`: distributed-tracing continuity stamps `traceparent`/`tracestate` from `Activity.Current` under W3C id format — the declared-attribute pattern the SDK's own `Partitioning`/`Sampling`/`Sequence` helpers hold, extended in place rather than forked per transport.
- `Rasm.Persistence`(`libs/csharp/Rasm.Persistence/.api/api-cloudevents.md`): the durable outbox row joins on the ENCODED envelope bytes, so the two folders share this codec identity and neither re-mints an envelope shape.

[LOCAL_ADMISSION]:
- One `static readonly JsonEventFormatter` is the codec identity every app-tier transport binding shares; a per-transport formatter instance is the rejected form.
- The W3C tracing pair is the ONE hand-declared extension family, because no SDK helper owns it; every dimension a helper DOES own joins the roster as that helper's `AllAttributes`.
- Transport bindings and delivery guarantees stay app-tier and never enter this folder.

[RAIL_LAW]:
- Packages: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.SystemTextJson`
- Owns: the CloudEvents 1.0 envelope algebra — the typed attribute map, the spec-version rosters, extension declaration, and the `System.Text.Json` structured/binary/batch codec
- Accept: a `BimEvent` lowered onto one `CloudEvent` under a declared roster, `Data` carried as a source-generated `JsonElement`, the `Instant` `time` seal, the W3C trace rows, and emit or admit through the shared `JsonEventFormatter`
- Reject: a hand-built JSON envelope where the formatter owns structured mode, a formatted timestamp string where `Instant.ToDateTimeOffset()` seals `time`, a per-transport formatter instance, an attribute declared at one end only, and any transport binding
