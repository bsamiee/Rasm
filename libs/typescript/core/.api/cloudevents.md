# [TS_CORE_API_CLOUDEVENTS]

`cloudevents` mints the CloudEvent envelope and its HTTP/Kafka/MQTT `Binding`s, serializing extension attributes to `ce-`-prefixed transport headers in binary mode and to a whole-envelope `application/cloudevents+json` document in structured mode. It realizes the CloudEvents dialect row of `interchange/carrier`, which owns the typed propagation value and its parse/print folds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cloudevents`
- package: `cloudevents` (Apache-2.0)
- module: single CJS `main` (`dist/index.js`), message bindings re-exported flat from the barrel, no deep subpath imports
- runtime: the `CloudEvent`/`CONSTANTS`/extension-slot layer is dependency-light; the `Message`/`Binding`/`Emitter` layer references node `http`/`Buffer`/`events`, so binding serialization runs on a node/bun lane
- rail: interchange/carrier

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the envelope, the transport-agnostic message, and the per-transport bindings

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]  | [CAPABILITY]                                             |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `CloudEventV1<T>` / `CloudEventV1Attributes<T>`     | envelope iface | spec-1.0 attribute contract; `[key:string]:unknown` slot |
|  [02]   | `CloudEvent<T = undefined>`                         | envelope class | validating envelope over required + optional attributes  |
|  [03]   | `ValidationError extends TypeError`                 | typed fault    | strict-validation failure; `errors?: ErrorObject[]`      |
|  [04]   | `Message<T = string>`                               | wire frame     | transport-agnostic `headers`/`body` frame                |
|  [05]   | `Binding<B, S>`                                     | codec contract | per-transport `binary`/`structured`/`toEvent`/`isEvent`  |
|  [06]   | `Mode`                                              | encoding enum  | `BINARY`/`STRUCTURED`/`BATCH` serializer selector        |
|  [07]   | `Serializer<M>` / `Deserializer` / `Detector`       | binding fns    | the `Binding` member function shapes                     |
|  [08]   | `Headers extends IncomingHttpHeaders`               | header map     | `ce-`-prefixed transport header carrier                  |
|  [09]   | `KafkaMessage<T>` / `KafkaEvent<T>`                 | kafka frame    | `key`/`value`/`timestamp`; `partitionkey`→`key`          |
|  [10]   | `MQTTMessage<T>`                                    | mqtt frame     | `PUBLISH`/`payload`/`User Properties` aliases            |
|  [11]   | `EmitterFunction` / `TransportFunction` / `Options` | emit fns       | per-call emit + transport function shapes                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: envelope construction, per-transport serialize/deserialize, and per-call emission

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------ | :------- | :---------------------------------------------------------- |
|  [01]   | `new CloudEvent(props, strict?)`                  | ctor     | envelope from owned attributes + branded tracing extensions |
|  [02]   | `event.cloneWith(options, strict?)`               | instance | immutable re-attribute; `<D>` overload retypes `data`       |
|  [03]   | `event.toJSON()` / `event.validate()`             | instance | structured projection; explicit schema re-check → `boolean` |
|  [04]   | `HTTP.binary(event)` / `HTTP.structured(event)`   | static   | `CloudEvent`→`Message`; binary emits `ce-` headers          |
|  [05]   | `HTTP.toEvent(message)` / `HTTP.isEvent(message)` | static   | `Message`→`CloudEventV1` or `[]` batch; detect first        |
|  [06]   | `Kafka.binary` / `Kafka.toEvent`                  | static   | Kafka record-header dialect; `partitionkey`↔`key`           |
|  [07]   | `MQTT.binary` / `MQTT.toEvent`                    | static   | MQTT PUBLISH dialect; `MQTTMessageFactory` builder          |
|  [08]   | `emitterFor(fn, { binding, mode })`               | factory  | per-call `EmitterFunction`; no shared singleton             |
|  [09]   | `httpTransport(sink)`                             | factory  | `TransportFunction` POSTing the serialized `Message`        |
|  [10]   | `CONSTANTS`                                       | const    | `ce-` header names, MIME types, `CE_USE_BIG_INT` env key    |
|  [11]   | `V1` / `V03`                                      | const    | `specversion` literal; the branch mints `V1` only           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Tracing and baggage attributes ride the `[key:string]:unknown` extension slot as plain keys, never a typed SDK module.
- Binary mode prefixes each extension attribute with `CONSTANTS.EXTENSIONS_PREFIX` (`ce-`), so `traceparent` rides as `ce-traceparent`; structured mode JSON-envelopes the whole event under `application/cloudevents+json`.
- `new CloudEvent(props)` runs `ajv` strict validation and throws `ValidationError` (a `TypeError` carrying `errors?: ErrorObject[]`) on a malformed envelope; `strict:false` bypasses validation.
- Emission composes `emitterFor(transportFn)` per call site; the static `Emitter` and `CloudEvent.emit` back a process-global `EventEmitter`, so two branch apps share one registry.

[STACKING]:
- `interchange/carrier`(`core/.planning/interchange/carrier.md`): the carrier folds `{TraceContext, Baggage}` and the promoted `rasm.tenant` member to strings and sets them as `CloudEvent` extension attributes, and this package's `binary` binding emits them as `ce-`-prefixed headers, realizing the table's `cloudevents` row.
- `effect` `Schema`(`.api/effect.md`): `Binding.toEvent` yields `CloudEventV1<unknown>` or a batch array, and `Schema.decodeUnknown` decodes the `data` payload and the branded extensions into owned vocabulary, lifting a `ParseError` into the `Effect` error channel.
- `effect` `Match`/`Data`(`.api/effect.md`): `Mode` and the HTTP/Kafka/MQTT `Binding` selection dispatch through `Match.exhaustive`, so a transport is a table row, never an `if`/`switch` ladder over content-type strings.
- `@bufbuild/protobuf`/`cbor-x`/`@msgpack/msgpack`(`core/.api/`): those own the `data` PAYLOAD codec, while `cloudevents` owns the ENVELOPE and its transport headers around that payload — the envelope codec never re-encodes the payload, the payload codec never mints an envelope.
- `mqtt`(`core/.api/mqtt.md`): `MQTT.binary(event)` publishes through the `mqtt` client's `publishAsync` with its `User Properties`/payload frame, and `MQTT.toEvent` decodes a delivered `IPublishPacket` — this binding mints the envelope, the `mqtt` client owns the connection.
- `value/fault`(`core/.planning/value/fault.md`): `ValidationError.errors` (the `ajv` `ErrorObject[]`) projects onto a `FaultClass` row at the `Effect.try` boundary — a typed decode fault, never a bare `TypeError`.

[LOCAL_ADMISSION]:
- Construct via `new CloudEvent(props)` strict-on inside `Effect.try` mapping `ValidationError` onto `FaultClass`; `strict:false` only re-hydrates already-validated bytes.
- Set and read tracing and baggage extension attributes only through the carrier folds (`core/.planning/interchange/carrier.md`), never a raw `CloudEvent[key]` read.
- Name headers through `CONSTANTS.EXTENSIONS_PREFIX`/`CONSTANTS.CE_HEADERS`; `CE_USE_BIG_INT` opts in only where a `data` payload carries i64 fidelity the JSON envelope drops.

[RAIL_LAW]:
- Package: `cloudevents`
- Owns: the CloudEvents envelope (`CloudEvent`/`CloudEventV1`, `cloneWith`/`toJSON`/`validate`), the transport-agnostic `Message`/`Binding` contract, the HTTP/Kafka/MQTT binary+structured bindings, the `ce-`-prefixed extension-attribute header mapping, the `emitterFor`/`httpTransport` per-call emission factory, and `CONSTANTS`/`V1`/`V03`
- Accept: strict-validated construction wrapped in `Effect.try`, tracing and baggage set as branded extension attributes from the carrier folds, `Binding`/`Mode` selected as a `Match` row, `toEvent` output crossing `Schema.decodeUnknown` into owned vocabulary, `ValidationError` mapped onto `FaultClass`, per-call `emitterFor` emission
- Reject: static `Emitter` singleton and `event.emit()`, raw `ValidationError` throw into a fold, bare SDK envelope in domain code, raw extension string bypassing the carrier folds, hand-rolled `ce-` header literals or hand-built CloudEvents JSON envelope, `strict:false` on untrusted bytes
