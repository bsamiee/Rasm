# [RASM_API_CLOUDEVENTS_AVRO]

`CloudNative.CloudEvents.Avro` binds the CloudEvents Avro Event Format: one `AvroEventFormatter` over the package's embedded CloudEvents Avro record schema, reaching STRUCTURED mode alone. `Rasm` admits it as one `EventFormat` row at `Rasm/Domain/event#FORMAT_CONTRACT` whose binary and batch columns both read false, so a caller asking for either refuses on the row rather than surfacing this package's `NotSupportedException` at its own boundary. `IGenericRecordSerializer` is the seat where a schema-registry-framed Avro leg binds its own reader and writer without re-spelling the envelope schema.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the formatter, its default serializer, and the serializer seat

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `Avro.AvroEventFormatter`                  | class         | `: CloudEventFormatter` — structured-mode Avro format |
|  [02]   | `Avro.BasicGenericRecordSerializer`        | class         | the default `IGenericRecordSerializer` implementation |
|  [03]   | `Avro.Interfaces.IGenericRecordSerializer` | interface     | the `Serialize`/`Deserialize` seat a custom leg binds |
|  [04]   | `CloudEvents.AvroEventFormatter`           | class         | `[Obsolete]` wrong-namespace shim; NEVER admitted     |

- [01]-[FORMATTER_SCHEMA]: `AvroSchema` is static and answers the parsed `Avro.RecordSchema` read once from the embedded resource, so a leg constructing its own `GenericRecord` reads the package's own schema rather than a transcription of it.
- [01]-[FORMATTER_CTORS]: `AvroEventFormatter()` binds `BasicGenericRecordSerializer`, and `AvroEventFormatter(IGenericRecordSerializer)` binds a caller's own.
- [01]-[FORMATTER_REACH]: `EncodeStructuredModeMessage` and both `DecodeStructuredModeMessage` arities are the whole reachable codec; `EncodeBatchModeMessage`, `DecodeBatchModeMessage`, `EncodeBinaryModeEventData`, and `DecodeBinaryModeEventData` are each overridden to THROW `NotSupportedException` with the mode named.
- [03]-[SERIALIZER_SEAT]: `ReadOnlyMemory<byte> Serialize(GenericRecord)` beside `GenericRecord Deserialize(Stream)`; the record handed to `Serialize` is guaranteed to match `AvroSchema`, so an implementation binds a registry-framed writer without re-validating the shape.
- [04]-[OBSOLETE_SHIM]: `CloudNative.CloudEvents.AvroEventFormatter` sits at the top level, carries `[Obsolete]` naming its own namespace as the defect, derives from the namespaced formatter, and exists for backward compatibility alone.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `AvroEventFormatter` structured-mode codec

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :---------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `new AvroEventFormatter()`                            | ctor     | formatter over the default record serializer |
|  [02]   | `new AvroEventFormatter(IGenericRecordSerializer)`    | ctor     | formatter over a registry-framed serializer  |
|  [03]   | `AvroEventFormatter.AvroSchema`                       | static   | the parsed embedded `RecordSchema`           |
|  [04]   | `EncodeStructuredModeMessage(ce, out ContentType)`    | instance | `application/cloudevents+avro` body          |
|  [05]   | `DecodeStructuredModeMessage(body\|stream, ct, exts)` | instance | Avro record → `CloudEvent`                   |
|  [06]   | `IGenericRecordSerializer.Serialize(GenericRecord)`   | instance | record → `ReadOnlyMemory<byte>`              |
|  [07]   | `IGenericRecordSerializer.Deserialize(Stream)`        | instance | stream → `GenericRecord`                     |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Structured mode is the WHOLE reach, and that matches the specification: the CloudEvents Avro event format defines a single record envelope with no batch wrapper and no binary content mode, so the four refusing overrides state a specification fact rather than a package gap.
- Records carry two fields — `attribute`, a map holding `specversion` beside every populated attribute, and `data`, the payload — so the envelope's whole attribute space rides one map and a required attribute has no dedicated field.
- Encode passes a `bool`, `int`, `byte[]`, or `string` attribute value through unchanged and formats every other value through the attribute's own `Format`, so a `Uri` and a `DateTimeOffset` cross as their canonical text.
- Decode is the inverse and asymmetric by design: a `bool`, `int`, or `byte[]` assigns through the envelope indexer, a `string` assigns through `SetAttributeFromString` so a declared typed extension parses, and any other Avro value type raises with the offending CLR type named.
- `Data` populates DIRECTLY from the Avro record, so its value carries the natural Avro deserialization type rather than the type a producer serialized; a consumer that needs its own shape converts at its own boundary.
- This formatter infers NO data content type, so an envelope arriving with none decodes with none.
- Encode stamps `application/cloudevents+avro` with NO charset parameter, unlike the JSON and Protobuf formatters, so a framing comparison that includes parameters mismatches where a media-type comparison holds.

[STACKING]:
- `api-cloudevents.md`: that catalogue owns the `CloudEventFormatter` contract, the envelope, the spec-version rosters, and `MimeUtilities.MediaType`; this package adds the format alone.
- `Apache.Avro`: `RecordSchema`, `Schema.Parse`, and `GenericRecord` (with its `ref`-shaped `TryGetValue` and its `Add`) are the runtime the formatter and every custom serializer compose.
- `Rasm.Persistence` registry seat: a Confluent-framed Avro leg binds its registry serde through `IGenericRecordSerializer` rather than beside the formatter, so the wire frame and the envelope schema stay one composition.
- Kernel owner anchor: `Rasm/Domain/event#FORMAT_CONTRACT` seats this formatter as the `avro` `EventFormat` row whose `Binary` and `Batches` columns both read false, so a batch or binary request refuses on the row and the package's `NotSupportedException` never reaches a caller's result.

[LOCAL_ADMISSION]:
- One `static readonly AvroEventFormatter` per process is the codec identity every binding shares.
- Obsolete top-level `CloudNative.CloudEvents.AvroEventFormatter` never enters a fence, a using, or a manifest reasoning; the namespaced type is the only admitted spelling.
- Legs needing a schema-registry frame bind `IGenericRecordSerializer` and never a second formatter or a transcribed envelope schema.
- Batches of Avro-framed events ship as N structured frames the transport batches, never as a CloudEvents batch body.
