# [RASM_API_CLOUDEVENTS]

`CloudNative.CloudEvents` binds the CloudEvents 1.0 envelope algebra — one `CloudEvent` carrying a spec-version-scoped typed attribute map, per-instance extension attributes, and a `Data` payload — beside the standard-extension helpers, the shared core utilities, and the `System.Net`-era HTTP surfaces; `CloudNative.CloudEvents.SystemTextJson` binds the `JsonEventFormatter` pair rendering and parsing it in structured, binary, and batch mode over `System.Text.Json`.

`Rasm` owns the branch's one envelope algebra at `Rasm/Domain/event#ENVELOPE_MINT` and every other folder composes it: `Rasm.Bim` announces its fired `BimFact` rows onto that owner at `Exchange/events#EVENT_PROJECTION`, `Rasm.Persistence` projects the op-log changefeed at `Version/egress` `Egress.Envelope`, `Rasm.Compute` decodes the sensor ingest, and `Rasm.AppHost` serves the HTTP ingress. Format siblings and protocol bindings ride their own catalogues — `api-cloudevents-protobuf.md`, `api-cloudevents-avro.md`, `api-cloudevents-aspnetcore.md` at the branch tier, the Kafka and AMQP bindings Persistence-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents`
- package: `CloudNative.CloudEvents` (Apache-2.0)
- assembly: `CloudNative.CloudEvents`
- namespace: `CloudNative.CloudEvents` (envelope, attribute, spec-version, formatter base), `.Core` (`MimeUtilities`/`BinaryDataUtilities`/`Validation`/`CloudEventAttributeTypes`), `.Extensions` (`Partitioning`/`Sampling`/`Sequence`), `.Http`
- asset: `net10.0` beside `net8.0` and `netstandard2.0`; the consumer binds `lib/net10.0` — pure-managed AnyCPU IL, no native asset; the core depends on the BCL alone
- rail: message envelope

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.SystemTextJson`
- package: `CloudNative.CloudEvents.SystemTextJson` (Apache-2.0)
- assembly: `CloudNative.CloudEvents.SystemTextJson` (`net10.0` bound asset over `System.Text.Json`, BCL-shipped on `net10.0`)
- namespace: `CloudNative.CloudEvents.SystemTextJson` (`JsonEventFormatter`/`JsonEventFormatter<T>`)
- rail: message envelope

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: envelope and attribute algebra

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                                                          |
| :-----: | :----------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `CloudEvent`                   | sealed class   | one event — spec version, typed attribute map, extensions, `Data`     |
|  [02]   | `CloudEventAttribute`          | class          | one attribute's name, `Type`, required/extension flags, validator     |
|  [03]   | `CloudEventAttributeType`      | abstract class | the attribute value spaces; `Parse`/`Format`/`Validate`/`ClrType`     |
|  [04]   | `CloudEventsSpecVersion`       | sealed class   | a spec version and its per-attribute singletons and rosters           |
|  [05]   | `CloudEventFormatter`          | abstract class | the codec contract — structured, binary, and batch in both directions |
|  [06]   | `CloudEventFormatterAttribute` | sealed class   | `[CloudEventFormatter(typeof(T))]` on a payload CLR type              |
|  [07]   | `ContentMode`                  | enum           | `Structured`/`Binary` — how a binding carries the event               |

- [01]-[EVENT_CTORS]: `CloudEvent()`, `(CloudEventsSpecVersion)`, `(IEnumerable<CloudEventAttribute>?)`, `(CloudEventsSpecVersion, IEnumerable<CloudEventAttribute>?)` — the extension roster handed at construction IS the declared set.
- [01]-[EVENT_MEMBERS]: `SpecVersion`, the two indexers `this[CloudEventAttribute]` and `this[string]`, the typed slots `Data`/`DataContentType`/`Id`/`DataSchema`/`Source`/`Subject`/`Time`/`Type`, `ExtensionAttributes`, `IsValid`, `GetAttribute(string)`, `GetPopulatedAttributes()`, `SetAttributeFromString(string, string)`, and `Validate()` → `CloudEvent`, which THROWS on a malformed envelope and returns the event on success so it chains.
- [02]-[ATTRIBUTE]: `Type`/`Name`/`IsRequired`/`IsExtension`; statics `CreateExtension(string, CloudEventAttributeType)` and `CreateExtension(string, CloudEventAttributeType, Action<object>?)`; instance `Parse(string)`/`Format(object)`/`Validate(object)`. Required and optional factories are `internal` — only the spec-version singletons mint those.
- [02]-[NAME_GATE]: `ValidateName` stays private, refuses an empty name and any character outside `[a-z0-9]`, and enforces NO length bound; `CreateExtension` also refuses the reserved `specversion` and `data`. Specification bounds an extension name at twenty characters, this package enforces no such bound, and a branch wanting it owns it.
- [03]-[ATTRIBUTE_TYPE]: value spaces are `Binary`/`Boolean`/`Integer`/`String`/`Timestamp`/`Uri`/`UriReference`, each a static singleton exposing `Parse`/`Format`/`Validate` beside the `Name` and `ClrType` pair; `Timestamp` carries a `DateTimeOffset` under RFC 3339, `Binary` a `byte[]`, `Integer` an `int`.
- [04]-[SPEC_VERSION]: `V1_0` and `Default`; `VersionId`; the per-attribute singletons `IdAttribute`/`SourceAttribute`/`TypeAttribute`/`DataContentTypeAttribute`/`DataSchemaAttribute`/`SubjectAttribute`/`TimeAttribute`; the rosters `RequiredAttributes`/`OptionalAttributes`/`AllAttributes`; the static `SpecVersionAttribute`; `FromVersionId(string?)` → `CloudEventsSpecVersion?`.
- [05]-[FORMATTER]: abstract `DecodeStructuredModeMessage(ReadOnlyMemory<byte>, ContentType?, IEnumerable<CloudEventAttribute>?)`, `EncodeStructuredModeMessage(CloudEvent, out ContentType)`, `DecodeBatchModeMessage(ReadOnlyMemory<byte>, ContentType?, IEnumerable<CloudEventAttribute>?)`, `EncodeBatchModeMessage(IEnumerable<CloudEvent>, out ContentType)`, `DecodeBinaryModeEventData`/`EncodeBinaryModeEventData`; virtual `Stream` and `Async` overloads beside each decode; `GetOrInferDataContentType(CloudEvent)` public beside the protected virtual `InferDataContentType(object)`.
- [06]-[FORMATTER_ATTRIBUTE]: `FormatterType` beside the `(Type)` ctor; the static `CreateFormatter(Type targetType)` reads the attribute with `inherit: true`, activates the named type, and answers `null` where the target carries no attribute — so a payload type declares its own codec and a resolver never carries a type-to-formatter table.

[PUBLIC_TYPE_SCOPE]: the System.Text.Json codec, the shared utilities, and the standard extensions

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `JsonEventFormatter`       | class         | `: CloudEventFormatter` — the STJ structured/binary/batch codec |
|  [02]   | `JsonEventFormatter<T>`    | class         | `: JsonEventFormatter` — the same codec binding `Data` to `T`   |
|  [03]   | `MimeUtilities`            | static class  | the CloudEvents media-type constants and probes                 |
|  [04]   | `BinaryDataUtilities`      | static class  | body/stream narrowing helpers the formatters share              |
|  [05]   | `Validation`               | static class  | the argument gates the envelope raises through                  |
|  [06]   | `CloudEventAttributeTypes` | static class  | `GetOrdinal(CloudEventAttributeType)` → the switch discriminant |
|  [07]   | `Partitioning`             | static class  | the `partitionkey` standard extension                           |
|  [08]   | `Sampling`                 | static class  | the `sampledrate` standard extension                            |
|  [09]   | `Sequence`                 | static class  | the `sequence`/`sequencetype` standard extension                |

- [01]-[JSON_FORMATTER]: ctors `JsonEventFormatter()` and `(JsonSerializerOptions?, JsonDocumentOptions)`; overrides for every abstract member beside the `Stream` and `Async` decode virtuals; the protected `SerializerOptions`/`DocumentOptions` the overrides read, the protected `DataPropertyName`/`DataBase64PropertyName` constants, and the protected virtual `EncodeStructuredModeData(CloudEvent, Utf8JsonWriter)`, `DecodeStructuredModeDataProperty(JsonElement, CloudEvent)`, `DecodeStructuredModeDataBase64Property(JsonElement, CloudEvent)`, and `IsJsonMediaType(string)` hooks.
- [02]-[TYPED_FORMATTER]: `JsonEventFormatter<T>` adds `()` and `(JsonSerializerOptions, JsonDocumentOptions)` ctors and overrides exactly the five data members — both binary-mode legs and all three structured-mode data hooks — so `Data` deserializes to `T` while the whole attribute codec stays the base's.
- [03]-[MIME]: `MediaType` (`application/cloudevents`) and `BatchMediaType` (`application/cloudevents-batch`) — each formatter EXTENDS them with its own format suffix, so a batch probe reads the prefix rather than a literal; `GetEncoding(ContentType?)`, `ToContentType(MediaTypeHeaderValue?)`, `ToMediaTypeHeaderValue(ContentType?)`, `CreateContentTypeOrNull(string?)`, `IsCloudEventsContentType(string?)`, `IsCloudEventsBatchContentType(string?)`.
- [04]-[GLUE]: `BinaryDataUtilities.AsArray`/`AsStream`/`GetString`/`ToReadOnlyMemory[Async]`/`CopyToStreamAsync` bridge `Data` ⇄ `byte[]`/`Stream`/`ReadOnlyMemory<byte>`; `Validation.CheckNotNull`/`CheckCloudEventArgument`/`CheckCloudEventBatchArgument` are the argument gates every formatter and binding opens with.
- [07]-[STANDARD_EXTENSIONS]: each helper publishes its own `AllAttributes` roster beside its attribute singletons and a `Set*`/`Get*` extension-method pair on `CloudEvent` — `Partitioning.PartitionKeyAttribute`, `Sampling.SampledRateAttribute` (Integer, positive-validated), and `Sequence.SequenceAttribute`/`SequenceTypeAttribute`.
- [07]-[EXTENSION_ACCESSORS]: `SetPartitionKey(ce, string?)`/`GetPartitionKey`, `SetSampledRate(ce, int?)`/`GetSampledRate`, and `SetSequence(ce, object?)`/`GetSequenceString`/`GetSequenceType`/`GetSequenceValue` each assign through the same `CloudEvent` indexer a caller reaches directly, so composing a singleton composes the helper.
- [09]-[SEQUENCE_CEILING]: `SetSequence` throws `ArgumentException` for every value but `int`, and `GetSequenceValue` parses a `"Integer"`-typed sequence through an `int`-backed surrogate attribute. Specification types `sequence` as a String whose `sequencetype` names its domain, so a position past `int` is spec-legal and unreachable through this helper — the roster composes, the value crossing does not.

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

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `CloudEventAttribute.CreateExtension(string, type[, validator])`           | factory  | declares an extension attribute            |
|  [02]   | `attribute.Parse(string)` / `Format(object)` / `Validate(object)`          | instance | typed string round-trip + validation       |
|  [03]   | `attribute.Type` / `Name` / `IsRequired` / `IsExtension`                   | property | attribute identity and role flags          |
|  [04]   | `attributeType.Parse` / `Format` / `Validate` / `Name` / `ClrType`         | instance | per-type parse/format; CLR-type mapping    |
|  [05]   | `CloudEventsSpecVersion.V1_0` / `.Default` / `FromVersionId(string?)`      | static   | v1.0 schema, `V1_0` default, id resolution |
|  [06]   | `specVersion.RequiredAttributes` / `OptionalAttributes` / `AllAttributes`  | property | required/optional/full attribute schema    |
|  [07]   | `Partitioning.SetPartitionKey(ce, string?)` / `GetPartitionKey(ce)`        | static   | `partitionkey` → a binding's message key   |
|  [08]   | `Sequence.SetSequence(ce, object?)` / `GetSequence{Value,String,Type}`     | static   | total event ordering (String, `int`-bound) |
|  [09]   | `Sampling.SetSampledRate(ce, int?)` / `GetSampledRate(ce)`                 | static   | sampling-rate hint (Integer, positive)     |
|  [10]   | `Partitioning`/`Sampling`/`Sequence` `.<name>Attribute` / `.AllAttributes` | static   | extension attributes to pre-register       |
|  [11]   | `CloudEventsSpecVersion.{Id,Source,Type,Time}Attribute`                    | static   | per-attribute singletons the indexer keys  |
|  [12]   | `CloudEventFormatterAttribute.CreateFormatter(Type)`                       | static   | resolves a payload type's declared codec   |

[ENTRYPOINT_SCOPE]: `CloudEventFormatter` codec contract and `JsonEventFormatter`

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------------------------- |
|  [01]   | `new JsonEventFormatter([JsonSerializerOptions?, JsonDocumentOptions])`   | ctor     | default or options-bound STJ formatter         |
|  [02]   | `new JsonEventFormatter<T>([JsonSerializerOptions, JsonDocumentOptions])` | ctor     | typed-`Data` formatter binding `Data` to `T`   |
|  [03]   | `EncodeStructuredModeMessage(ce, out ContentType)`                        | instance | full event as structured-mode JSON body        |
|  [04]   | `EncodeBinaryModeEventData(ce)`                                           | instance | binary-mode `Data` body                        |
|  [05]   | `EncodeBatchModeMessage(ces, out ContentType)`                            | instance | batch-mode JSON array body                     |
|  [06]   | `DecodeStructuredModeMessage(body, contentType, extensions)` / `...Async` | instance | `ReadOnlyMemory<byte>`/`Stream` → `CloudEvent` |
|  [07]   | `DecodeBatchModeMessage` / `...Async`                                     | instance | array body → `IReadOnlyList<CloudEvent>`       |
|  [08]   | `DecodeBinaryModeEventData(body, ce)`                                     | instance | binary-mode `Data` body into an event          |
|  [09]   | `ConvertToJsonElement(ce)` / `ConvertFromJsonElement(JsonElement, exts)`  | instance | `JsonElement` round-trip of the full event     |
|  [10]   | `GetOrInferDataContentType(ce)`                                           | instance | resolves the effective `datacontenttype`       |

[ENTRYPOINT_SCOPE]: the `System.Net`-era HTTP surfaces (`CloudNative.CloudEvents.Http`) — each row states the host type it reaches, since only the listener surface carries a synchronous decode and only the client surface reaches both a request and a response

| [INDEX] | [SYMBOL]                 | [SURFACE]                                               | [CAPABILITY]                             |
| :-----: | :----------------------- | :------------------------------------------------------ | :--------------------------------------- |
|  [01]   | `HttpClientExtensions`   | `IsCloudEvent` / `IsCloudEventBatch`, both directions   | content-type or `ce-specversion` probe   |
|  [02]   | `HttpClientExtensions`   | `ToCloudEventAsync(formatter[, exts])`, both directions | message → one `CloudEvent`               |
|  [03]   | `HttpClientExtensions`   | `ToCloudEventBatchAsync(formatter[, exts])`             | batch body → `IReadOnlyList<CloudEvent>` |
|  [04]   | `HttpListenerExtensions` | `IsCloudEvent` / `IsCloudEventBatch`                    | listener-side content probe              |
|  [05]   | `HttpListenerExtensions` | `ToCloudEvent[Async]` / `ToCloudEventBatchAsync`        | the one SYNCHRONOUS decode here          |
|  [06]   | `HttpListenerExtensions` | `CopyToHttpListenerResponseAsync(rsp, mode, fmt)`       | single write; batch takes no mode        |
|  [07]   | `HttpWebExtensions`      | `CopyToHttpWebRequestAsync(req, mode, fmt)`             | single and batch onto `HttpWebRequest`   |
|  [08]   | `HttpUtilities`          | `HttpHeaderPrefix` / `SpecVersionHttpHeader`            | the binding's own header names           |
|  [09]   | `HttpUtilities`          | `GetAttributeNameFromHeaderName` / `*HeaderValue`       | the percent-encoding pair                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Extension rosters handed to the ctor AND to every decode ARE the wire contract: an attribute declared for a write and forgotten at its read decodes as an untyped string row, so one roster is spelled once and passed at both ends.
- `Validate()` throws rather than railing, so an envelope mint is a BOUNDARY: it captures through one `Op.Catch` funnel onto the typed rail and a caller composing events never has a construction fault escape past its own `Fin` signature. `CloudEventsSpecVersion.V1_0` requires `id`/`source`/`type` and admits optional `subject`/`datacontenttype`/`dataschema`/`time`; `IsValid` checks required completeness alone.
- Attribute assignment through either indexer runs the attribute's own validator and THROWS on refusal, so every extension write is part of the same boundary the mint funnels.
- Each encode member's `out ContentType` is the framing an app-tier transport binding stamps AND the discriminant a decode reads to pick its reader, so both travel as one carrier value and neither half is re-derived.
- Batch framing is the `BatchMediaType` PREFIX each formatter extends with its own suffix, so a framing probe reads the prefix constant and never a literal media type.
- `Data` is `object?` round-tripping through whatever `CloudEventFormatter` encodes it — raw `byte[]`/`ReadOnlyMemory<byte>` under `application/octet-stream`, a POCO under `JsonEventFormatter<T>`, or a `JsonElement` carried verbatim into the structured body with zero reflection metadata, decoded back as a `JsonElement` the same source-generated context deserializes.
- `ContentMode` places the event two ways: `Structured` packs attributes and data into the body under `application/cloudevents+json`; `Binary` writes attributes to transport metadata and only `Data` to the body, so a header-filtering broker routes without parsing the body.
- `JsonSerializerOptions` and `JsonDocumentOptions` each default `AllowDuplicateProperties` to TRUE on this runtime, so a producer emitting one attribute twice decodes last-write-wins with nothing raising; both halves arm at formatter construction or not at all.

[STACKING]:
- `NodaTime`(`api-nodatime.md`): the `time` attribute is `CloudEventAttributeType.Timestamp` over a `DateTimeOffset`, so a NodaTime `Instant` seals through `Instant.ToDateTimeOffset()` and never a formatted string.
- `api-nodatime-stj.md` and `api-thinktecture-json.md`: `ConfigureForNodaTime` and `ThinktectureJsonConverterFactory` register on the one formatter options identity, so a typed `Data` carrying semantic instants or generated owners round-trips without a per-lane converter.
- `System.Text.Json`(`api-system-text-json.md`): a source-generated `JsonSerializerContext` projects a domain payload to a `JsonElement`, keeping the formatter reflection-free under a trimmed or AOT publish.
- `System.Diagnostics.DiagnosticSource`: creation-time W3C context rides declared `traceparent`/`tracestate` extension attributes stamped from a captured `TraceCarrier` — the declared-attribute pattern the `Partitioning`/`Sampling`/`Sequence` helpers hold, extended in place rather than forked per transport.
- format siblings: `api-cloudevents-protobuf.md` and `api-cloudevents-avro.md` bind the two non-JSON event formats over this same envelope and this same `CloudEventFormatter` contract.
- HTTP binding: `api-cloudevents-aspnetcore.md` binds the ASP.NET Core request and response surfaces; the `.Http` classes above are the `HttpClient`, `HttpListener`, and `HttpWebRequest` surfaces the same package ships for hosts outside ASP.NET Core, and the estate reaches none of them — the AppNetCore surface serves the one HTTP ingress this branch hosts.
- transport bindings: the Kafka binding (`Rasm.Persistence/.api/api-cloudevents-kafka.md`) and AMQP binding (`Rasm.Persistence/.api/api-cloudevents-amqp.md`) are Persistence-local egress legs over this one envelope; MQTT and NATS are branch-owned bindings over `api-mqtt.md` and `api-nats.md` carriers, because the package publishes no NATS binding and its MQTT binding compiles against a retired carrier shape.
- Kernel owner anchor: `Rasm/Domain/event#ENVELOPE_MINT` holds the branch's one mint, `#EXTENSION_ROSTER` the one declared attribute set composing each standard helper's own singleton, `#EVENT_GRAMMAR` the admitted `type`/`source`/content-key vocabularies, and `#FORMAT_CONTRACT` the format rows and their one formatter instance each.
- Consumer anchors: `Rasm.Bim/Exchange/events#EVENT_PROJECTION` mints announcements onto that owner as an observe subscription over its hook roster; `Rasm.Persistence/Version/egress` projects `Version/ledger` `OpLogEntry` → `CloudEvent` per entry and maps it onto each sink's transport; `Rasm.Compute/Runtime/transport` decodes the sensor ingest onto its capture lane; `Rasm.AppHost` serves the HTTP ingress. One `CloudEvent` is the single cross-consumer, cross-language vocabulary every outbox consumer drains, so a per-consumer re-pack is the drift defect.

[LOCAL_ADMISSION]:
- One `static readonly` formatter instance per event format is the codec identity every transport binding shares; serializer options fix at formatter construction, never per event, and a per-transport or per-event formatter instance is the rejected form.
- Every dimension a helper owns joins the declared roster as that helper's OWN attribute singleton; a dimension no helper owns mints through the branch's ceiling-checked extension-name gate, because the package enforces the alphabet and not the specification's length bound.
- Extension attributes declare once and read back with the identical attribute enumerable at every decode.
- Transport bindings, delivery guarantees, and per-binding `dataref` policy stay at their owning legs and never enter the envelope owner.

[RAIL_LAW]:
- Packages: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.SystemTextJson`
- Owns: the CloudEvents 1.0 envelope algebra — the typed attribute map, the spec-version rosters, extension declaration, the standard-extension helpers, the shared MIME and validation utilities, the four `System.Net`-era HTTP surfaces — and the `System.Text.Json` structured/binary/batch codec in both its untyped and typed forms
- Accept: an event minted under a declared roster, `Data` as source-generated `JsonElement`, POCO, or raw bytes, the `Instant` `time` seal, the declared W3C trace rows, emit and admit through the shared formatter, partition key via `Partitioning`
- Reject: a hand-built JSON envelope where the formatter owns structured mode, a formatted timestamp string where `Instant.ToDateTimeOffset()` seals `time`, a per-transport or per-event formatter instance, an attribute declared at one end only, an extension name past the specification ceiling the package admits, a `sequence` value crossed through the `int`-bound helper, and a second envelope shape parallel to the one branch mint
- `Rasm.Element` (`Graph/wire#EVENT_ENVELOPE`): `CloudEvent` is the envelope VALUE crossing `GraphCrossing.Mint`/`Frame`/`Admit` — construction, rostered writes, and codecs all ride the kernel `EventEnvelope` owner; `Graph/corpus` `CorpusGate.Announce` is the in-folder Snapshot producer.
