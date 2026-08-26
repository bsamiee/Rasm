# [RASM_API_CLOUDEVENTS_PROTOBUF]

`CloudNative.CloudEvents.Protobuf` binds the CloudEvents Protobuf Event Format: one `ProtobufEventFormatter` over the generated `CloudNative.CloudEvents.V1.CloudEvent` and `CloudEventBatch` messages, reaching structured, batch, and a restricted binary content mode. `Rasm` admits it as one `EventFormat` row at `Rasm/Domain/event#FORMAT_CONTRACT`, so every consuming binding frames through that row's single formatter instance.

Generated envelope messages share the simple name `CloudEvent` with the core envelope type, so every fence touching this surface qualifies both sides.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the formatter and the generated envelope messages

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `Protobuf.ProtobufEventFormatter`                  | class         | `: CloudEventFormatter` — the Protobuf format |
|  [02]   | `V1.CloudEvent`                                    | sealed class  | generated envelope; NAME-COLLIDES with core   |
|  [03]   | `V1.CloudEventBatch`                               | sealed class  | generated batch over repeated envelopes       |
|  [04]   | `V1.CloudEvent.Types.CloudEventAttributeValue`     | sealed class  | attribute-value `oneof` over the value spaces |
|  [05]   | `V1.CloudEvent.DataOneofCase`                      | enum          | `BinaryData`/`TextData`/`ProtoData`/`None`    |
|  [06]   | `CloudEventAttributeValue.AttrOneofCase`           | enum          | `CeBoolean`/`CeBytes`/`CeInteger`/`CeString`  |
|  [07]   | `V1.CloudeventsReflection`/`ProtoSchemaReflection` | static class  | the generated file descriptors                |

- [01]-[FORMATTER_CTORS]: `ProtobufEventFormatter()` uses the `DefaultTypeUrlPrefix` constant, and `ProtobufEventFormatter(string typeUrlPrefix)` gates its argument through `Validation.CheckNotNull`; the resolved value reads back on the `TypeUrlPrefix` property.
- [01]-[FORMATTER_CONSTANT]: `DefaultTypeUrlPrefix` is `"type.googleapis.com"` — the prefix Protobuf libraries themselves use when packing an `Any`, so a peer resolves a packed message's type without knowing the producing service.
- [01]-[FORMATTER_OVERRIDES]: every abstract member of the base and the `Stream` decode virtuals — `EncodeStructuredModeMessage`, `EncodeBatchModeMessage`, both `DecodeStructuredModeMessage` arities, both `DecodeBatchModeMessage` arities, `EncodeBinaryModeEventData`, `DecodeBinaryModeEventData`.
- [01]-[FORMATTER_CROSSINGS]: `ConvertToProto(CloudEvent)` and `ConvertFromProto(V1.CloudEvent, IEnumerable<CloudEventAttribute>?)` are public and carry the message-level crossing a registry-framed leg composes instead of re-encoding a body the formatter already holds.
- [01]-[FORMATTER_HOOKS]: `protected virtual EncodeStructuredModeData(CloudEvent, V1.CloudEvent)` and `protected virtual DecodeStructuredModeData(V1.CloudEvent, CloudEvent)` are the specialization seats — one to unwrap `ProtoData` into generated code, one to pack a caller's own message shape.
- [06]-[ATTR_ONEOF]: `AttrOneofCase` closes on `CeBoolean`/`CeBytes`/`CeInteger`/`CeString`/`CeTimestamp`/`CeUri`/`CeUriRef`, one case per `CloudEventAttributeType` value space.
- [02]-[GENERATED_MESSAGES]: generated messages carry the standard `Google.Protobuf` surface — `Parser`, `Descriptor`, `Clone`, `CalculateSize`, `WriteTo`, `MergeFrom`, and the `IMessage` implementation `MessageExtensions.ToByteArray` consumes.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ProtobufEventFormatter` codec and message crossings

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `new ProtobufEventFormatter([typeUrlPrefix])`         | ctor     | default or repo-pinned `Any` type-URL prefix  |
|  [02]   | `TypeUrlPrefix`                                       | property | the resolved prefix, never null               |
|  [03]   | `EncodeStructuredModeMessage(ce, out ContentType)`    | instance | `application/cloudevents+protobuf` body       |
|  [04]   | `EncodeBatchModeMessage(ces, out ContentType)`        | instance | `application/cloudevents-batch+protobuf` body |
|  [05]   | `DecodeStructuredModeMessage(body\|stream, ct, exts)` | instance | envelope message → `CloudEvent`               |
|  [06]   | `DecodeBatchModeMessage(body\|stream, ct, exts)`      | instance | batch message → `IReadOnlyList<CloudEvent>`   |
|  [07]   | `EncodeBinaryModeEventData(ce)`                       | instance | `byte[]` or `text/*` string data ONLY         |
|  [08]   | `DecodeBinaryModeEventData(body, ce)`                 | instance | `text/*` → string, otherwise a byte array     |
|  [09]   | `ConvertToProto(ce)`                                  | instance | `CloudEvent` → `V1.CloudEvent`                |
|  [10]   | `ConvertFromProto(V1.CloudEvent, exts)`               | instance | `V1.CloudEvent` → `CloudEvent`                |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Structured mode carries three data shapes and no more: a `string` lands in `TextData`, a `byte[]` in `BinaryData`, and an `IMessage` in `ProtoData` packed as an `Any` under `TypeUrlPrefix` — except an `IMessage` that already IS an `Any`, which propagates directly so a decode-then-re-encode never double-wraps. Any other `Data` type raises `ArgumentException` at the encode.
- Decode leaves `ProtoData` as an `Any` and never unpacks it, so a relay stores and forwards a payload whose message type it does not reference; unpacking is the consuming leg's own `Any.Unpack<T>()`.
- Binary mode is RESTRICTED by design: only a `byte[]` and a `string` under a `text/`-prefixed `datacontenttype` encode, because the specification fixes no content type distinguishing a directly-serialized message from an `Any`-packed one. Message payloads take a structured-mode frame or an explicit byte-array projection.
- Framing derives from `MimeUtilities.MediaType`/`BatchMediaType` under a `+protobuf` suffix and both encodes stamp `charset=utf-8`, so a framing probe reads the media-type prefix rather than a literal.
- Required attributes arriving inside the proto `attributes` map raise rather than merge: `id`, `source`, and `type` ride their own top-level proto fields, and a type mismatch between a declared extension and its proto `oneof` case raises with both spellings named.
- This formatter infers NO data content type — `GetOrInferDataContentType` answers the envelope's own value, so a producer leaving it unset ships an envelope whose consumer decodes by convention.

[STACKING]:
- `api-cloudevents.md`: that catalogue owns the `CloudEventFormatter` contract, the `CloudEvent` envelope, the spec-version rosters, and the `MimeUtilities` framing constants; this package adds the format alone.
- `api-protobuf.md`: `Google.Protobuf`'s `IMessage`, `Any.Pack`/`Unpack`, `ByteString`, `Timestamp`, `MessageExtensions.ToByteArray`, and `MessageParser.ParseFrom` are the runtime surface the generated messages and the formatter both compose.
- Kernel owner anchor: `Rasm/Domain/event#FORMAT_CONTRACT` seats this formatter as the `protobuf` `EventFormat` row carrying `Binary` and `Batches` both true, so a caller selects the row and never the type.

[LOCAL_ADMISSION]:
- One `static readonly ProtobufEventFormatter` per process is the codec identity every binding shares; a per-message or per-transport instance is the rejected form.
- Every fence naming the generated envelope qualifies it as `CloudNative.CloudEvents.V1.CloudEvent`, because the simple name collides with the core envelope inside any file that binds both namespaces.
- Message payloads ride structured mode; a binary-mode frame carries an explicit byte-array projection the producing leg made, never a raw `IMessage`.
