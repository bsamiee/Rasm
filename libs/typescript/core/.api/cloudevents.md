# [TS_CORE_API_CLOUDEVENTS]

`cloudevents` is the CloudEvents JS SDK — the one place `interchange/carrier` speaks the CloudEvents dialect of the closed per-transport propagation table, and the only wire owner mapping the W3C Distributed-Tracing extension attributes (`traceparent`, `tracestate`) onto a transport frame without hand-rolling a CloudEvents envelope. SDK v10 ships NO dedicated distributed-tracing module: the tracing attributes are ordinary extension attributes riding the generic `[key: string]: unknown` slot on `CloudEventV1`, and the binary-mode `HTTP`/`Kafka`/`MQTT` bindings serialize them to `ce-`-prefixed transport headers through `CONSTANTS.EXTENSIONS_PREFIX`. So the carrier owns the typed `traceparent`/`tracestate`/`baggage` value and its parse/print folds; this package owns envelope construction and the binary/structured `Binding` carrying the branded attributes across HTTP, Kafka, and MQTT. Two shipped hazards the boundary owner internalizes: `new CloudEvent(props)` runs strict schema validation and throws `ValidationError` (a `TypeError` subclass) on a non-conforming envelope — caught at `Effect.try` and lifted onto `FaultClass`, never a raw throw into a fold; and the `Emitter` static singleton (`Emitter.instance`/`getInstance`/`emitEvent`, which `CloudEvent.emit` delegates to) is a process-global `EventEmitter` two branch-composed apps collide on — the branch never touches it, routing emission through per-invocation `emitterFor(transportFn)` or the `interchange/invoke` interceptor.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cloudevents`
- package: `cloudevents` (Apache-2.0)
- effect-peer: none — the constructed `CloudEvent` and every `toEvent` deserialization cross into `effect` `Schema.decodeUnknown` at the `interchange/carrier` seam (`.api/effect.md`); no bundled peer
- catalog-verdict: KEEP — the CloudEvents-dialect arm of the `interchange/carrier` propagation table: `@connectrpc/connect` owns the Connect ASCII/`-bin` metadata dialect (`encodeBinaryHeader`/`decodeBinaryHeader`, `.api/connectrpc-connect.md`), this owns the CloudEvents envelope and its HTTP/Kafka/MQTT binary bindings — one carrier codec spans the dialects, no overlap with the Connect metadata arm
- runtime: mixed — the `CloudEvent` value, its extension slot, and `CONSTANTS` are isomorphic (UMD `bundles/cloudevents.js` ships a browser build); the `Message`/`Binding`/`Emitter` layer carries node type deps (`Headers extends http.IncomingHttpHeaders`, `Emitter` over node `events.EventEmitter`, `KafkaMessage#key: string | Buffer`), so binding serialization is a node/bun-lane surface
- transitive deps: `ajv` + `ajv-formats` (strict-mode JSON-Schema validation), `json-bigint` (`CE_USE_BIG_INT` env opt-in), `uuid`, `process`, `util`
- modules / subpaths: single entry — `main` `dist/index.js` (CJS), `types` `dist/index.d.ts`; the message bindings re-export flat from the barrel, no deep subpath imports

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the envelope, the transport-agnostic message, and the per-transport bindings
- rail: interchange/carrier
- `CloudEventV1<T>` is the spec-1.0 envelope interface with a `[key: string]: unknown` extension slot the tracing attributes ride; `CloudEvent<T>` is the validating class over it. `Message<T>` is the transport-agnostic `{headers, body}` frame; `Binding<B,S>` is the four-function serialize/deserialize/detect contract every transport implements; `Mode` picks binary versus structured encoding.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                                                 |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------------------------------------ |
|  [01]   | `CloudEventV1<T>` / `CloudEventV1Attributes<T>` | envelope iface | spec-1.0 attribute contract; `[key:string]:unknown` extension slot  |
|  [02]   | `CloudEvent<T = undefined>`                     | envelope class | validating envelope; `id`/`source`/`type`/`specversion` + optionals |
|  [03]   | `ValidationError extends TypeError`             | typed fault    | strict-validation failure `errors?: ErrorObject[]` → `FaultClass`   |
|  [04]   | `Message<T = string>` (`headers`, `body`)       | wire frame     | transport-agnostic frame the branch decodes into owned vocabulary   |
|  [05]   | `Binding<B, S>`                                 | codec contract | one per transport; `binary`/`structured`/`toEvent`/`isEvent`        |
|  [06]   | `Mode` (`BINARY`/`STRUCTURED`/`BATCH`)          | encoding enum  | serializer mode selector on `emitterFor`; `BATCH` structured-only   |
|  [07]   | `Serializer<M>` / `Deserializer` / `Detector`   | binding fns    | `Binding` member function shapes `Match`-dispatched at the seam     |
|  [08]   | `Headers extends IncomingHttpHeaders`           | header map     | `ce-`-prefixed transport headers carrying the extension attributes  |
|  [09]   | `KafkaMessage<T>` / `KafkaEvent<T>`             | kafka frame    | `key`/`value`/`timestamp`; `partitionkey`→`KafkaMessage#key`        |
|  [10]   | `MQTTMessage<T>` (`PUBLISH`/`payload`)          | mqtt frame     | MQTT v5 UserProperties/payload alias `headers`/`body`               |
|  [11]   | `EmitterFunction`/`TransportFunction`/`Options` | emit fns       | `emitterFor` composes a transport fn into a per-call event emitter  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: envelope construction, per-transport serialize/deserialize, and per-call emission
- rail: interchange/carrier
- `new CloudEvent(props, strict?)` constructs and validates the envelope; the tracing attributes are set as plain extension keys on `props`. `HTTP.binary`/`HTTP.structured` serialize to a `Message`, `HTTP.toEvent` deserializes back; `Kafka` and `MQTT` mirror the contract. `emitterFor(httpTransport(sink))` builds a per-call `EmitterFunction` — never the `Emitter` static singleton.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                              |
| :-----: | :--------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `new CloudEvent(props, strict?)`                     | construct      | envelope from owned attributes + branded tracing extensions      |
|  [02]   | `event.cloneWith(options, strict?)` / `cloneWith<D>` | augment        | immutable re-attribute; new `data` type via the `<D>` overload   |
|  [03]   | `event.toJSON()` / `event.validate()`                | project/verify | structured projection; explicit schema re-check → `boolean`      |
|  [04]   | `HTTP.binary(event)` / `HTTP.structured(event)`      | serialize      | `CloudEvent` → `Message`; binary maps extensions to `ce-` heads  |
|  [05]   | `HTTP.toEvent(message)` / `HTTP.isEvent(message)`    | deserialize    | `Message` → `CloudEventV1` (or `[]` batch); detect before decode |
|  [06]   | `Kafka.binary` / `Kafka.toEvent` (`KafkaMessage`)    | kafka codec    | `partitionkey`↔`key` mapping; Kafka record-header dialect        |
|  [07]   | `MQTT.binary` / `MQTT.toEvent`                       | mqtt codec     | MQTT v5 dialect; `MQTTMessageFactory` PUBLISH builder            |
|  [08]   | `emitterFor(fn, { binding, mode })`                  | emit factory   | per-call `EmitterFunction`; app-neutral, no shared singleton     |
|  [09]   | `httpTransport(sink)`                                | transport fn   | ready `TransportFunction` POSTing the serialized `Message`       |
|  [10]   | `CONSTANTS` (`EXTENSIONS_PREFIX`, `CE_HEADERS`, ...) | wire literals  | `ce-` header names, MIME types, `CE_USE_BIG_INT` env key         |
|  [11]   | `V1` (`"1.0"`) / `V03` (`"0.3"`)                     | spec version   | `specversion` literal; the branch mints `V1` envelopes only      |

## [04]-[IMPLEMENTATION_LAW]

[CARRIER_TOPOLOGY]:
- tracing attributes are plain extension keys, never a typed module: SDK v10 has no distributed-tracing class — `traceparent`/`tracestate` are CloudEvents extension attributes on the `[key: string]: unknown` slot of `CloudEventV1`. `interchange/carrier` owns the typed `{TraceContext, Baggage}` value and its total parse/print folds; it sets the printed strings as extension keys on the `CloudEvent` props and reads them back off `toEvent` output through the same folds — a raw extension string never reaches domain code untyped.
- binary mode maps extensions to `ce-` headers: `HTTP.binary`/`Kafka.binary`/`MQTT.binary` serialize each extension attribute to a transport header prefixed by `CONSTANTS.EXTENSIONS_PREFIX` (`"ce-"`), so `traceparent` rides as `ce-traceparent`. Carrier's CloudEvents dialect row states exactly this header shape; structured mode instead JSON-envelopes the whole event under `application/cloudevents+json`.
- construction validates and throws: `new CloudEvent(props)` runs `ajv` strict-mode validation and throws `ValidationError` (a `TypeError` subclass carrying `errors?: ErrorObject[]`) on a malformed envelope. Boundary wraps construction and every `toEvent` call in `Effect.try`, mapping `ValidationError` onto the owned `FaultClass` rail — a raw throw into a fold is the defect. `strict:false` bypasses validation and is admitted only for a re-hydration lane over already-trusted bytes.
- static `Emitter` singleton is banned: `Emitter.instance`/`getInstance`/`on`/`emitEvent` back a process-global `EventEmitter`, and `CloudEvent.emit` delegates to it — two branch-composed apps share one emitter registry, the exact app-neutrality collision the estate law forbids. Emission composes `emitterFor(transportFn)` per call site (or the `interchange/invoke` interceptor); `Emitter` and `event.emit()` are never touched.

[INTEGRATION_LAW]:
- Stack with `interchange/carrier` (`core/.planning/interchange/carrier.md`): the carrier owns the closed per-transport dialect table; this package IS the CloudEvents row's realization — `carrier` folds `{TraceContext, Baggage}` to strings, sets them as extension attributes on a `CloudEvent`, and the `binary` binding emits `ce-`-prefixed headers. Connect ASCII/`-bin` metadata is `@connectrpc/connect`'s row (`.api/connectrpc-connect.md`), NATS/Kafka/MQTT header shapes are their own rows — one carrier value, many dialect serializers.
- Stack with `effect` `Schema` (`.api/effect.md`): `Binding.toEvent` yields an untyped `CloudEventV1<unknown>` (or a batch array); `Schema.decodeUnknown` decodes the `data` payload and the branded extension attributes once into owned vocabulary, lifting a `ParseError` into the `Effect` error channel. Raw SDK envelopes never leak past the seam.
- Stack with `effect` `Match`/`Data` (`.api/effect.md`): `Mode`, the `Binding` selection across HTTP/Kafka/MQTT, and the `isEvent` detect-then-decode gate dispatch through `Match.exhaustive` — a transport is a table row, never an `if`/`switch` ladder over content-type strings.
- Stack with `@bufbuild/protobuf` / `cbor-x` / `@msgpack/msgpack` (`core/.api/`): those own the `data` PAYLOAD codec by C# mint format; `cloudevents` owns the ENVELOPE and its transport headers around that payload. A structured-mode CloudEvent whose `data` is a proto message composes both — the envelope codec never re-encodes the payload, the payload codec never mints an envelope.
- Stack with `value/fault` (`core/.planning/value/fault.md`): `ValidationError.errors` (the `ajv` `ErrorObject[]`) projects onto a `FaultClass` diagnostic row at the `Effect.try` boundary; the carrier surfaces a typed decode fault, never a bare `TypeError`.
- Stack with `value/identity` (`core/.planning/value/identity.md`): the `rasm.tenant` baggage promotion rides the CloudEvents extension slot as a branded attribute keyed by the one identity projection, so tenant context survives every broker hop the CloudEvents binding crosses.

[LOCAL_ADMISSION]:
- construct through `new CloudEvent(props)` with strict validation ON; wrap in `Effect.try` and map `ValidationError` onto `FaultClass`. Reach for `strict:false` only on a re-hydration lane over already-validated bytes.
- set `traceparent`/`tracestate`/`baggage` as extension attributes from the carrier's parse/print folds; never read the raw extension string off the event into domain logic — the branded value is the only typed form.
- select the `Binding` (`HTTP`/`Kafka`/`MQTT`) and `Mode` by transport as a `Match` row; binary mode for header-carried extensions, structured mode for whole-envelope JSON transport.
- emit through `emitterFor(transportFn)` per call site; never `Emitter.getInstance`/`Emitter.emitEvent` or `event.emit()` — the static singleton is a cross-app collision.
- decode `Binding.toEvent` output through `Schema.decodeUnknown` before any consumer reads it; a bare `CloudEvent`/`CloudEventV1` in domain code is the leak defect.
- use `CONSTANTS.EXTENSIONS_PREFIX`/`CONSTANTS.CE_HEADERS` for header names, never string literals; the `CE_USE_BIG_INT` env opt-in is admitted only where a `data` payload carries i64 fidelity the JSON envelope otherwise drops.

[RAIL_LAW]:
- Package: `cloudevents`
- Owns: the CloudEvents envelope (`CloudEvent`/`CloudEventV1`, `cloneWith`/`toJSON`/`validate`), the transport-agnostic `Message`/`Binding` contract, the `HTTP`/`Kafka`/`MQTT` binary+structured bindings, the `ce-`-prefixed extension-attribute header mapping that realizes the carrier's CloudEvents dialect row, the `emitterFor`/`httpTransport` per-call emission factory, and `CONSTANTS`/`V1`/`V03`
- Accept: strict-validated construction wrapped in `Effect.try`, tracing/baggage set as branded extension attributes from the carrier folds, `Binding`/`Mode` selected as a `Match` row, `toEvent` output crossing `Schema.decodeUnknown` into owned vocabulary, `ValidationError` mapped onto `FaultClass`, per-call `emitterFor` emission
- Reject: the `Emitter` static singleton and `event.emit()` (process-global `EventEmitter`, cross-app collision), a raw `ValidationError` throw into a fold, a bare SDK envelope in domain code, a raw extension string read bypassing the carrier folds, hand-rolled `ce-` header literals or a hand-built CloudEvents JSON envelope where the `Binding` owns the wire, `strict:false` on untrusted bytes
