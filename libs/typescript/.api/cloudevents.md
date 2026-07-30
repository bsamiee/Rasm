# [TS_BRANCH_API_CLOUDEVENTS]

`cloudevents` mints the validated CloudEvents 1.0 envelope and its HTTP/Kafka/MQTT `Binding`s, serializing extension attributes to `ce-`-prefixed transport headers in binary mode and the whole envelope to an `application/cloudevents+json` document in structured mode. Two branch tiers compose it from one contract: `interchange/carrier` owns the typed propagation value whose `cloudevents` dialect row this package realizes, and the outbox and webhook egress lanes construct the envelope those headers ride on.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cloudevents`
- package: `cloudevents` (Apache-2.0)
- module: single CJS `main` (`dist/index.js`), message bindings re-exported flat from the barrel, no deep subpath imports
- runtime: the `CloudEvent`/`CONSTANTS`/extension-slot layer is dependency-light and isomorphic; the `Message`/`Binding`/`Emitter` layer references node `http`/`Buffer`/`events`, so binding serialization runs on a node/bun lane and `Message.headers` extends `IncomingHttpHeaders`
- rail: interchange/carrier — envelope construction and transport-header projection

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

- `CloudEventV1<T>`: `id`, `source`, `type`, and `specversion` are required and every other attribute optional; the `[key: string]: unknown` slot admits the extension attributes the carrier folds write.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: envelope construction and clone, per-transport serialize/deserialize, header projection, and per-call emission

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :-------------------------------------------------- | :------- | :----------------------------------------------------------- |
|  [01]   | `new CloudEvent(props, strict?)`                    | ctor     | envelope from owned attributes + branded tracing extensions |
|  [02]   | `event.cloneWith(options, strict?)`                 | instance | immutable re-attribute; `<D>` overload retypes `data`       |
|  [03]   | `event.toJSON()` / `.toString()` / `.validate()`    | instance | structured projection, stringify, explicit schema re-check  |
|  [04]   | `CloudEvent.cloneWith(event, options, strict?)`     | static   | clone a raw `CloudEventV1` off the class                    |
|  [05]   | `HTTP.binary(event)` / `HTTP.structured(event)`     | static   | `CloudEvent`→`Message`; binary emits `ce-` headers          |
|  [06]   | `HTTP.toEvent(message)` / `HTTP.isEvent(message)`   | static   | `Message`→`CloudEventV1` or `[]` batch; detect first        |
|  [07]   | `Kafka.binary` / `Kafka.toEvent`                    | static   | Kafka record-header dialect; `partitionkey`↔`key`           |
|  [08]   | `MQTT.binary` / `MQTT.toEvent`                      | static   | MQTT PUBLISH dialect; `MQTTMessageFactory` builder          |
|  [09]   | `headersFor(event)` / `sanitize(headers)`           | static   | binary-header projection; lowercase-normalize a frame       |
|  [10]   | `allowedContentTypes` / `requiredHeaders`           | static   | admitted binary content types; required `ce-*` literal set  |
|  [11]   | `emitterFor(fn, { binding, mode })`                 | factory  | per-call `EmitterFunction`; no shared singleton             |
|  [12]   | `httpTransport(sink)`                               | factory  | `TransportFunction` POSTing the serialized `Message`        |
|  [13]   | `CONSTANTS`                                         | const    | `ce-` header names, MIME types, `CE_USE_BIG_INT` env key    |
|  [14]   | `V1` / `V03`                                        | const    | `specversion` literal; the branch mints `V1` only           |

- `event.cloneWith` carries three overloads — the default excludes `data` and preserves `T`, the `<D>` form retypes `data`, and the static form clones a raw `CloudEventV1`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Tracing and baggage attributes ride the `[key:string]:unknown` extension slot as plain keys, never a typed SDK module.
- Binary mode prefixes each extension attribute with `CONSTANTS.EXTENSIONS_PREFIX` (`ce-`), so `traceparent` rides as `ce-traceparent`; structured mode JSON-envelopes the whole event under `application/cloudevents+json`.
- Extension attribute NAMES admit lowercase alphanumerics alone, so a dotted convention key bends at the projection and bends back at the read; the prefix and the bent key are two separate constraints and neither implies the other.
- `new CloudEvent(props)` runs `ajv` strict validation and throws `ValidationError` (a `TypeError` carrying `errors?: ErrorObject[]`) on a malformed envelope; `strict:false` bypasses validation and re-hydrates already-validated bytes alone.
- `HTTP.binary`/`HTTP.structured` return a transport-neutral `Message` whose body encodes to bytes exactly once upstream of any signature; framing never reserializes after a signature lands.
- Emission composes `emitterFor(transportFn)` per call site; the static `Emitter` and `CloudEvent.emit` back a process-global `EventEmitter`, so two branch apps would share one registry.

[STACKING]:
- `interchange/carrier`(`core/.planning/interchange/carrier.md`): the carrier folds the W3C triple and the promoted tenancy member to strings and sets them as `CloudEvent` extension attributes, and this package's `binary` binding emits them as `ce-`-prefixed headers, realizing the dialect table's `cloudevents` row; the inverse read hands the envelope straight back to `Carrier.extract`.
- `data/journal/append`(`data/.planning/journal/append.md`): the ONE member-level consumer in the branch — the outbox envelope projection constructs `CloudEvent` over a carrier-injected attribute record and its inverse decodes the same envelope back through the carrier, so core composes the package's CONTRACT (the dialect row) while the journal composes its MEMBERS. A second construction site is the split the one projection forecloses.
- `runtime` webhook egress: `HTTP.binary`/`HTTP.structured` output lands on the hook payload's headers and body, the body encodes to bytes once, and the signing service signs those exact bytes.
- `effect` `Schema`(`.api/effect.md`): `Binding.toEvent` yields `CloudEventV1<unknown>` or a batch array, and `Schema.decodeUnknown` decodes the `data` payload and the branded extensions into owned vocabulary, lifting a `ParseError` into the `Effect` error channel; a `BOUNDARY ADAPTER` lifts the construction throw through `Effect.try`.
- `effect` `Match`/`Data`(`.api/effect.md`): `Mode` and the HTTP/Kafka/MQTT `Binding` selection dispatch through `Match.exhaustive`, so a transport is a table row, never an `if`/`switch` ladder over content-type strings.
- `@bufbuild/protobuf`/`cbor-x`/`@msgpack/msgpack`(`core/.api/`): those own the `data` PAYLOAD codec, while `cloudevents` owns the ENVELOPE and its transport headers around that payload — the envelope codec never re-encodes the payload, the payload codec never mints an envelope.
- `mqtt`(`core/.api/mqtt.md`): `MQTT.binary(event)` publishes through the `mqtt` client's `publishAsync` with its `User Properties`/payload frame, and `MQTT.toEvent` decodes a delivered `IPublishPacket` — this binding mints the envelope, the `mqtt` client owns the connection.
- `value/fault`(`core/.planning/value/fault.md`): `ValidationError.errors` (the `ajv` `ErrorObject[]`) projects onto a `FaultClass` row at the `Effect.try` boundary — a typed decode fault, never a bare `TypeError`.

[LOCAL_ADMISSION]:
- Construct via `new CloudEvent(props)` strict-on inside `Effect.try` mapping `ValidationError` onto `FaultClass`; `strict:false` only re-hydrates already-validated bytes.
- Set and read tracing and baggage extension attributes only through the carrier folds, never a raw `CloudEvent[key]` read.
- Name headers through `CONSTANTS.EXTENSIONS_PREFIX`/`CONSTANTS.CE_HEADERS`; `CE_USE_BIG_INT` opts in only where a `data` payload carries i64 fidelity the JSON envelope drops.
- Select `binary` or `structured` explicitly and carry the returned headers exactly; encode the body to bytes once, before any signature.

[RAIL_LAW]:
- Package: `cloudevents`
- Owns: the CloudEvents 1.0 envelope (`CloudEvent`/`CloudEventV1`, `cloneWith`/`toJSON`/`toString`/`validate`), `ValidationError`, the transport-agnostic `Message`/`Binding` contract, the HTTP/Kafka/MQTT binary+structured bindings, the `ce-`-prefixed extension-attribute header mapping with `headersFor`/`sanitize`/`allowedContentTypes`/`requiredHeaders`, the `emitterFor`/`httpTransport` per-call emission factory, and `CONSTANTS`/`V1`/`V03`
- Accept: strict-validated construction wrapped in `Effect.try`, tracing and baggage set as branded extension attributes from the carrier folds, `Binding`/`Mode` selected as a `Match` row, `toEvent` output crossing `Schema.decodeUnknown` into owned vocabulary, `ValidationError` mapped onto `FaultClass`, per-call `emitterFor` emission, one body-to-bytes encoding before signing
- Reject: static `Emitter` singleton and `event.emit()`, raw `ValidationError` throw into a fold, bare SDK envelope in domain code, raw extension string bypassing the carrier folds, hand-rolled `ce-` header literals or a hand-built CloudEvents JSON envelope, `strict:false` on untrusted bytes, serialization after signing, `Message.body` read as bytes without narrowing
