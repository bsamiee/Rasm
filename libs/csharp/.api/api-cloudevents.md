# [RASM_API_CLOUDEVENTS]

`CloudNative.CloudEvents` binds the CloudEvents 1.0 envelope algebra — one `CloudEvent` carrying a spec-version-scoped typed attribute map, per-instance extension attributes, and a `Data` payload — and `CloudNative.CloudEvents.SystemTextJson` binds the `JsonEventFormatter` rendering and parsing it in structured, binary, and batch mode over `System.Text.Json`. Two folders share the codec identity: `Rasm.Bim` lowers `BimEvent` onto the envelope at `Exchange/events#EVENTS`, and `Rasm.Persistence` projects the redacted op-log changefeed onto it at `Version/egress` `Egress.Envelope`, one `CloudEvent` per `OpLogEntry`. Transport bindings ride their own catalogues — `CloudNative.CloudEvents.Mqtt` at `api-cloudevents-mqtt.md`, the Kafka and AMQP bindings Persistence-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents`
- package: `CloudNative.CloudEvents` (Apache-2.0)
- assembly: `CloudNative.CloudEvents`
- namespace: `CloudNative.CloudEvents` (envelope, attribute, spec-version, formatter base), `.Core` (`MimeUtilities`/`BinaryDataUtilities`/`Validation`), `.Extensions` (`Partitioning`/`Sampling`/`Sequence`), `.Http`
- asset: `net10.0` beside `net8.0` and `netstandard2.0`; the consumer binds `lib/net10.0` — pure-managed AnyCPU IL, no native asset; the core depends on the BCL alone
- rail: event envelope

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.SystemTextJson`
- package: `CloudNative.CloudEvents.SystemTextJson` (Apache-2.0)
- assembly: `CloudNative.CloudEvents.SystemTextJson` (`net10.0` bound asset over `System.Text.Json`, BCL-shipped on `net10.0`)
- namespace: `CloudNative.CloudEvents.SystemTextJson` (`JsonEventFormatter`/`JsonEventFormatter<T>`)
- rail: event envelope

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: envelope and attribute algebra

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                                                          |
| :-----: | :----------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `CloudEvent`                   | sealed class   | one event — spec version, typed attribute map, extensions, `Data`     |
|  [02]   | `CloudEventAttribute`          | class          | one attribute's name, `Type`, required/extension flags, validator     |
|  [03]   | `CloudEventAttributeType`      | abstract class | the attribute value spaces; `Parse`/`Format`/`Validate` per type      |
|  [04]   | `CloudEventsSpecVersion`       | sealed class   | a spec version and its per-attribute singletons and rosters           |
|  [05]   | `CloudEventFormatter`          | abstract class | the codec contract — structured, binary, and batch in both directions |
|  [06]   | `CloudEventFormatterAttribute` | attribute      | `[CloudEventFormatter(typeof(T))]` on a payload CLR type              |
|  [07]   | `ContentMode`                  | enum           | `Structured`/`Binary` — how a binding carries the event               |

- [01]-[EVENT_CTORS]: `CloudEvent()`, `(CloudEventsSpecVersion)`, `(IEnumerable<CloudEventAttribute>?)`, `(CloudEventsSpecVersion, IEnumerable<CloudEventAttribute>?)` — the extension roster handed at construction IS the declared set.
- [01]-[EVENT_MEMBERS]: `SpecVersion`, the two indexers `this[CloudEventAttribute]` and `this[string]`, the typed slots `Data`/`DataContentType`/`Id`/`DataSchema`/`Source`/`Subject`/`Time`/`Type`, `ExtensionAttributes`, `IsValid`, `GetAttribute(string)`, `GetPopulatedAttributes()`, `SetAttributeFromString(string, string)`, and `Validate()` → `CloudEvent`, which THROWS on a malformed envelope and returns the event on success so it chains.
- [02]-[ATTRIBUTE]: `Type`/`Name`/`IsRequired`/`IsExtension`; statics `CreateExtension(string, CloudEventAttributeType)` and `CreateExtension(string, CloudEventAttributeType, Action<object>?)`; instance `Parse(string)`/`Format(object)`/`Validate(object)`. The required/optional factories are `internal` — only the spec-version singletons mint those.
- [03]-[ATTRIBUTE_TYPE]: the value spaces are `Binary`/`Boolean`/`Integer`/`String`/`Timestamp`/`Uri`/`UriReference`, each a static singleton exposing `Parse`/`Format`/`Validate`/`ClrType`; `Timestamp` carries a `DateTimeOffset` under RFC 3339.
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

- [01]-[JSON_FORMATTER]: ctors `JsonEventFormatter()` and `(JsonSerializerOptions?, JsonDocumentOptions)`, and overrides for every abstract and virtual member above; `JsonEventFormatter<T>` overrides only the binary-mode `Data` codec so the payload deserializes to `T`.
- [01]-[JSON_NATIVE]: `ConvertToJsonElement(CloudEvent)` → `JsonElement` and `ConvertFromJsonElement(JsonElement, IEnumerable<CloudEventAttribute>?)` → `CloudEvent` skip the byte body entirely; the protected `DecodeStructuredModeDataProperty`/`DecodeStructuredModeDataBase64Property` hooks, the `SerializerOptions`/`DocumentOptions` they read, and the `data`/`data_base64` names ride as protected members.
- [03]-[MIME]: `MediaType` (`application/cloudevents`) and `BatchMediaType` (`application/cloudevents-batch`) — each formatter EXTENDS them with its own format suffix, so a batch probe reads the prefix rather than a literal; `GetEncoding(ContentType?)`, `ToContentType(MediaTypeHeaderValue?)`, `ToMediaTypeHeaderValue(ContentType?)`, `CreateContentTypeOrNull(string?)`, `IsCloudEventsContentType(string?)`, `IsCloudEventsBatchContentType(string?)`.
- [04]-[GLUE]: `BinaryDataUtilities.AsArray`/`AsStream`/`ToReadOnlyMemory[Async]` bridge `Data` ⇄ `byte[]`/`Stream`/`ReadOnlyMemory<byte>`; `CloudEventFormatterAttribute.CreateFormatter(Type)` resolves a `[CloudEventFormatter]` formatter.
- [06]-[STANDARD_EXTENSIONS]: each helper publishes its own `AllAttributes` roster beside a `Set*`/`Get*` pair — `Partitioning.SetPartitionKey(ce, key)`/`GetPartitionKey`, `Sampling.SetSampledRate(ce, int)` (Integer, positive-validated), `Sequence.SetSequence(ce, object)`/`GetSequence{Value,String,Type}` — each `Set*` an extension method on `CloudEvent`, so a dimension the SDK already owns joins a declared roster from the package rather than a hand-spelled `CreateExtension` row.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `CloudEvent` construction and attribute access

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :----------------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `new CloudEvent([specVersion][, extensionAttributes])` | ctor     | v1.0 event; optional spec-version + pre-declared extensions |
|  [02]   | `Id` / `Source` / `Type` / `Time`                      | property | required `id`/`source`/`type` + optional `time`             |
|  [03]   | `Subject` / `DataSchema` / `DataContentType`           | property | optional context attributes                                 |
|  [04]   | `Data`                                                 | property | `object?` payload; format-decoded, set raw bytes or POCO    |
|  [05]   | `this[CloudEventAttribute]` / `this[string]`           | property | typed get/set by attribute or name (auto-extension)         |
|  [06]   | `SetAttributeFromString(string, string)`               | instance | parses + sets from canonical string form                    |
|  [07]   | `GetAttribute(string)`                                 | instance | resolves the `CloudEventAttribute` if populated/known       |
|  [08]   | `GetPopulatedAttributes()`                             | instance | `KeyValuePair<CloudEventAttribute, object>` non-null set    |
|  [09]   | `ExtensionAttributes`                                  | property | declared extension attributes on this event                 |
|  [10]   | `SpecVersion` / `IsValid` / `Validate()`               | instance | version; `IsValid` completeness, `Validate` throws          |

[ENTRYPOINT_SCOPE]: attribute algebra, spec-version, and standard extensions

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------ | :------- | :----------------------------------------- |
|  [01]   | `CloudEventAttribute.CreateExtension(string, type[, validator])`          | factory  | declares an extension attribute            |
|  [02]   | `attribute.Parse(string)` / `Format(object)` / `Validate(object)`         | instance | typed string round-trip + validation       |
|  [03]   | `attribute.Type` / `Name` / `IsRequired` / `IsExtension`                  | property | attribute identity and role flags          |
|  [04]   | `attributeType.Parse` / `Format` / `Validate` / `ClrType`                 | instance | per-type parse/format; `ClrType` mapping   |
|  [05]   | `CloudEventsSpecVersion.V1_0` / `.Default` / `FromVersionId(string)`      | static   | v1.0 schema, `V1_0` default, id resolution |
|  [06]   | `specVersion.RequiredAttributes` / `OptionalAttributes` / `AllAttributes` | property | required/optional/full attribute schema    |
|  [07]   | `Partitioning.SetPartitionKey(ce, string)` / `GetPartitionKey(ce)`        | static   | `partitionkey` → a binding's message key   |
|  [08]   | `Sequence.SetSequence(ce, object)` / `GetSequence{Value,String,Type}(ce)` | static   | total event ordering (String-typed)        |
|  [09]   | `Sampling.SetSampledRate(ce, int)` / `GetSampledRate(ce)`                 | static   | sampling-rate hint (Integer, positive)     |
|  [10]   | `Partitioning`/`Sampling`/`Sequence` `.<name>Attribute`                   | static   | extension attributes to pre-register       |
|  [11]   | `CloudEventsSpecVersion.{Id,Source,Type,Time}Attribute`                   | static   | per-attribute singletons the indexer keys  |

[ENTRYPOINT_SCOPE]: `CloudEventFormatter` codec contract and `JsonEventFormatter`

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------------------------- |
|  [01]   | `new JsonEventFormatter([JsonSerializerOptions, JsonDocumentOptions])`    | ctor     | default or options-bound STJ formatter         |
|  [02]   | `new JsonEventFormatter<T>([JsonSerializerOptions, JsonDocumentOptions])` | ctor     | typed-`Data` formatter binding `Data` to `T`   |
|  [03]   | `EncodeStructuredModeMessage(ce, out ContentType)`                        | instance | full event as structured-mode JSON body        |
|  [04]   | `EncodeBinaryModeEventData(ce)`                                           | instance | binary-mode `Data` body                        |
|  [05]   | `EncodeBatchModeMessage(ces, out ContentType)`                            | instance | batch-mode JSON array body                     |
|  [06]   | `DecodeStructuredModeMessage(body, contentType, extensions)` / `...Async` | instance | `ReadOnlyMemory<byte>`/`Stream` → `CloudEvent` |
|  [07]   | `DecodeBatchModeMessage` / `...Async`                                     | instance | array body → `IReadOnlyList<CloudEvent>`       |
|  [08]   | `DecodeBinaryModeEventData(body, ce)`                                     | instance | binary-mode `Data` body into an event          |
|  [09]   | `ConvertToJsonElement(ce)` / `ConvertFromJsonElement(JsonElement, exts)`  | instance | `JsonElement` round-trip of the full event     |
|  [10]   | `GetOrInferDataContentType(ce)`                                           | instance | resolves the effective `datacontenttype`       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The extension roster handed to the ctor AND to every decode IS the wire contract: an attribute declared for a write and forgotten at its read decodes as an untyped string row, so one roster is spelled once and passed at both ends.
- `Validate()` throws rather than railing, so an envelope mint is a BOUNDARY: it captures through one `Try.lift` funnel onto the typed rail and a caller composing events never has a construction fault escape past its own `Fin` signature. `CloudEventsSpecVersion.V1_0` requires `id`/`source`/`type` and admits optional `subject`/`datacontenttype`/`dataschema`/`time`; `IsValid` checks required completeness.
- The `out ContentType` each encode member yields is the framing an app-tier transport binding stamps AND the discriminant a decode reads to pick its reader, so both travel as one carrier value and neither half is re-derived.
- Batch framing is the `BatchMediaType` PREFIX each formatter extends with its own suffix, so a framing probe reads the prefix constant and never a literal media type.
- `Data` is `object?` round-tripping through whatever `CloudEventFormatter` encodes it — raw `byte[]`/`ReadOnlyMemory<byte>` under `application/octet-stream`, a POCO under `JsonEventFormatter<T>`, or a `JsonElement` carried verbatim into the structured body with zero reflection metadata, decoded back as a `JsonElement` the same source-generated context deserializes.
- `ContentMode` places the event two ways: `Structured` packs attributes + data into the body under `application/cloudevents+json`; `Binary` writes attributes to transport metadata and only `Data` to the body, so a header-filtering broker routes without parsing the body.

[STACKING]:
- `NodaTime`(`api-nodatime.md`): the `time` attribute is `CloudEventAttributeType.Timestamp` over a `DateTimeOffset`, so a NodaTime `Instant` seals through `Instant.ToDateTimeOffset()` and never a formatted string.
- `System.Text.Json`(`api-system-text-json.md`): a source-generated `JsonSerializerContext` projects the domain payload to a `JsonElement`, keeping the formatter reflection-free under a trimmed or AOT publish.
- `System.Diagnostics.DiagnosticSource`: distributed-tracing continuity stamps `traceparent`/`tracestate` from `Activity.Current` under W3C id format — the declared-attribute pattern the SDK's own `Partitioning`/`Sampling`/`Sequence` helpers hold, extended in place rather than forked per transport.
- transport bindings: `CloudNative.CloudEvents.Mqtt`(`api-cloudevents-mqtt.md`) is branch substrate; the Kafka binding (`Rasm.Persistence/.api/api-cloudevents-kafka.md`) and AMQP binding (`Rasm.Persistence/.api/api-cloudevents-amqp.md`) are Persistence-local egress legs over this one envelope.
- Bim consumer anchor: `Exchange/events#EVENTS` lowers a `BimEvent` onto one `CloudEvent` under a declared roster with the source-generated `JsonElement` `Data` projection and the `Instant` `time` seal; the folder references the core and the STJ codec alone and holds no transport binding.
- Persistence consumer anchor: `Version/egress` projects the `Version/ledger` `OpLogEntry` → `CloudEvent` via the `Egress.Envelope` projector — required `Id` (the content key), `Source` (`rasm:persistence/oplog`), `Type` (`rasm.oplog.{entityKind}.{kind}`), `Time`, the redacted payload bytes in `Data` under `application/octet-stream` — and each `EgressSink` row maps that one event onto its own transport. Trace continuity rides the `traceparent`/`redacted` extension attributes, and the Python `runtime/transport` decode recovers the W3C context; one `CloudEvent` is the single cross-consumer, cross-language vocabulary every `OutboundHop` consumer of the outbox spine drains, so a per-consumer re-pack is the drift defect.

[LOCAL_ADMISSION]:
- One `static readonly JsonEventFormatter` (or `JsonEventFormatter<T>` for a typed change record) is the codec identity every transport binding shares; serializer options fix at formatter construction, never per event, and a per-transport formatter instance is the rejected form.
- The W3C tracing pair is the ONE hand-declared extension family, because no SDK helper owns it; every dimension a helper DOES own joins the roster as that helper's `AllAttributes`; extension attributes declare once via `CreateExtension` and read back with the identical attribute enumerable.
- Transport bindings and delivery guarantees stay at their owning legs and never enter the Bim folder.

[RAIL_LAW]:
- Packages: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.SystemTextJson`
- Owns: the CloudEvents 1.0 envelope algebra — the typed attribute map, the spec-version rosters, extension declaration, the standard-extension helpers — and the `System.Text.Json` structured/binary/batch codec
- Accept: an event minted under a declared roster, `Data` as source-generated `JsonElement`, POCO, or raw bytes, the `Instant` `time` seal, the W3C trace rows, emit and admit through the shared formatter, partition key via `Partitioning`
- Reject: a hand-built JSON envelope where the formatter owns structured mode, a formatted timestamp string where `Instant.ToDateTimeOffset()` seals `time`, a per-transport or per-event formatter instance, an attribute declared at one end only, and a second envelope shape parallel to the one CloudEvents projection
