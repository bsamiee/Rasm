# [TS_BRANCH_API_CLOUDEVENTS]

`cloudevents` owns the branch's CloudEvents 1.0 envelope plus JSON, HTTP, Kafka, and MQTT behavior. Generated Protobuf and the frozen Avro publisher asset supply the other event formats; no branch page transcribes an envelope or extension roster.

Barrel exports bound the admitted surface. Four HTTP header members and the `Detector` shape live in modules the barrel never re-exports, no `exports` map fences `dist/`, and the branch refuses the deep path reaching them; every such member is branch-owned instead.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the barrel's exported type surface — the message envelope, the transport-agnostic frame, and the per-transport frames

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]    | [CAPABILITY]                                                       |
| :-----: | :---------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `CloudEventV1<T>` / `CloudEventV1Attributes<T>` | message envelope | spec-1.0 attribute contract; `[key: string]: unknown` slot         |
|  [02]   | `CloudEvent<T = undefined>`                     | message envelope | frozen validating class; implements `CloudEventV1<T>`              |
|  [03]   | `ValidationError extends TypeError`             | typed fault      | `errors?: string[] \| ErrorObject[] \| null` from ajv              |
|  [04]   | `Message<T = string>`                           | wire frame       | `headers: Headers` beside `body: T \| string \| Buffer \| unknown` |
|  [05]   | `Headers extends IncomingHttpHeaders`           | header map       | `string \| string[] \| undefined` values, node-typed               |
|  [06]   | `Binding<B, S>`                                 | codec contract   | `binary`/`structured`/`toEvent`/`isEvent` — FOUR members           |
|  [07]   | `Serializer<M>` / `Deserializer`                | binding fns      | `Serializer` yields `M`; `Deserializer` yields one OR an array     |
|  [08]   | `KafkaMessage<T>` / `KafkaEvent<T>`             | kafka frame      | `key: string \| Buffer`, `value`, `timestamp?`; `partitionkey`     |
|  [09]   | `MQTTMessage<T>`                                | mqtt frame       | `PUBLISH`/`payload`/`User Properties` aliases over `Message`       |
|  [10]   | `TransportFunction` / `EmitterFunction`         | emit fns         | `(message, options?)` and `(event, options?)`, both `Promise`      |
|  [11]   | `Options`                                       | untyped bag      | `[key: string]: string \| Record<string, unknown> \| unknown`      |

- `dist/message/index.d.ts` declares `Detector`, the `Binding.isEvent` shape, and the barrel omits it, so a branch surface naming a detector declares its own predicate type.
- `Mode` is a TypeScript `enum`, which `erasableSyntaxOnly` refuses in branch code; its three members reach the branch as an owned literal union and cross to `emitterFor` at the seam alone.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: barrel-reachable construction, per-transport serialize and deserialize, and per-call emission

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `new CloudEvent(props, strict = true)`           | ctor     | freezes the instance; mints absent `id`/`time`/`specversion`    |
|  [02]   | `event.cloneWith(options, strict = true)`        | instance | two overloads — preserve `T`, or retype `data` through `<D>`    |
|  [03]   | `CloudEvent.cloneWith(event, options, strict?)`  | static   | `new CloudEvent(Object.assign({}, event, options), strict)`     |
|  [04]   | `event.toJSON()` / `event.toString()`            | instance | plain object with `time` re-formatted; `toString` stringifies   |
|  [05]   | `event.validate()`                               | instance | ajv re-check; wraps a foreign throw in `ValidationError`        |
|  [06]   | `HTTP.binary` / `HTTP.structured`                | static   | `ce-` headers + raw body; `application/cloudevents+json` body   |
|  [07]   | `HTTP.toEvent` / `HTTP.isEvent`                  | static   | binary, structured, AND batch decode; `isEvent` swallows throws |
|  [08]   | `Kafka.binary` / `Kafka.structured`              | static   | `ce_` headers; `partitionkey` projects to `KafkaMessage.key`    |
|  [09]   | `Kafka.toEvent` / `Kafka.isEvent`                | static   | binary, structured, and batch decode off the `ce_` header set   |
|  [10]   | `MQTT.binary` / `MQTT.structured`                | static   | UNPREFIXED attribute spread into `User Properties`              |
|  [11]   | `MQTT.toEvent` / `MQTT.isEvent`                  | static   | binary and structured only — no batch arm exists                |
|  [12]   | `MQTTMessageFactory(contentType, headers, body)` | static   | builds `PUBLISH`/`payload`/`User Properties` over one body      |
|  [13]   | `emitterFor(fn, options?)`                       | factory  | destructures `binding`/`mode` off an unchecked `Options` bag    |
|  [14]   | `httpTransport(sink)`                            | factory  | `TransportFunction` POSTing the serialized `Message`            |
|  [15]   | `Emitter.on` / `Emitter.emitEvent` / `emit()`    | static   | process-global `EventEmitter` singleton; the branch refuses it  |
|  [16]   | `CONSTANTS`                                      | const    | header names, MIME types, and the `CE_USE_BIG_INT` env key      |
|  [17]   | `V1` / `V03`                                     | const    | `"1.0"` and `"0.3"`; the branch mints `V1` alone                |

- `headersFor`, `sanitize`, `allowedContentTypes`, and `requiredHeaders` are declared in `dist/message/http/headers.d.ts` and a second `headersFor` in `dist/message/kafka/headers.d.ts`; `message/http/index.d.ts` re-exports `HTTP` alone, so none reaches the barrel and each is branch-owned.
- No `Binding` carries a batch serializer, so batch ENCODE is branch-owned at every transport while HTTP and Kafka decode a batch through `toEvent`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Construction mints an absent `id` from `uuid.v4()`, an absent `time` from `new Date().toISOString()`, and an absent `specversion` from `V1`, so a mint that omits any of the three reads ambient randomness and the wall clock inside the SDK where no `Clock` or `Random` service can reach.
- Construction ends at `Object.freeze(this)`, so a message envelope is immutable and every re-attribution is a `cloneWith` that re-runs the whole admission.
- `strict` gates the extension gate ALONE: names match `/^[a-z0-9]+$/` and values pass `isValidType`, while the specification's 20-character ceiling appears in the thrown message and is never enforced.
- `validateCloudEvent` RETURNS `false` for any `specversion` other than `V1` instead of throwing, so a `V03` message envelope passes strict construction unvalidated.
- Cross-version guards over `specversion`/`schemaurl` and `specversion`/`dataschema` throw a bare `TypeError`, not a `ValidationError`, so a catch narrowing on `ValidationError` alone loses that arm.
- Binary `data` auto-populates `data_base64` through `asBase64`, and an incoming `data_base64` auto-decodes to `data` as `Uint8Array`; `toJSON` drops `data` when both are present.
- `toJSON` re-formats `time` through `new Date(time).toISOString()`, so a `time` no `Date` parses throws a `RangeError` out of `toString`, `HTTP.structured`, and `MQTT.structured`.
- Attribute-name prefixing is PER TRANSPORT and no constant spans the three: HTTP prefixes `CONSTANTS.EXTENSIONS_PREFIX` (`ce-`), Kafka hard-codes `ce_`, and MQTT spreads attribute names UNPREFIXED into `User Properties`.
- `Kafka.headersFor` prefixes EVERY own property except `data` and `data_base64`, applying no header map, so a Kafka header name joins `ce_` to the attribute name verbatim.
- `HTTP.headersFor` stamps `ce-time` from `new Date(event.time).toISOString()` after the header map runs, so the wire instant is a re-formatting of the attribute rather than its transcription.
- `JSONParser.parse` assigns the GLOBAL `JSON` binding — to `json-bigint` when `process.env.CE_USE_BIG_INT === "true"`, back to a captured reference otherwise — so the flag is a process-wide swap of `JSON.parse` for every consumer, never a per-call knob.
- `HTTP.isEvent` and `Kafka.isEvent` implement detection by running the full deserialize inside `try`/`catch`, so a detect-then-decode pair parses the frame twice.
- `emitterFor` reads `binding` and `mode` off an index-signature `Options` bag, so a misspelled key silently takes the `HTTP`/`BINARY` default and `Mode.BATCH` reaches a bare `TypeError` at emit rather than a compile error.
- `Emitter` and `event.emit()` back one process-global `EventEmitter`, so two branch applications in one process share one registry.

[STACKING]:
- `core/interchange/carrier`(`core/.planning/interchange/carrier.md`): `Event.admit`/`mint` own strict SDK construction, `Event.format.protobuf` owns publisher Protobuf conversion, `Event.address` owns the `(source,id)` dedup coordinate, and `Event.rasm` owns the descriptor-derived profile while leaving `dataref` residence downstream.
- `core/interchange/format` (`core/.planning/interchange/format.md`): exact JSON, generated Protobuf, and lane-bound Avro codecs with optional batch members and exact parsed media identity.
- `core/interchange/codec`(`core/.planning/interchange/codec.md`): the wire registry excludes the message envelope by law, so no `Wire` family, landing class, or parity obligation names a CloudEvents shape.
- `data/journal/append` (`data/.planning/journal/append.md`): mints the stable `journal` capability source independently from the admitted type, projects each claimed row through `Event.rasm.Fact` and `Event.rasm.mint`, and keeps `dataschema` as the payload schema URI.
- `data/object/store`(`data/.planning/object/store.md`): `Dataref` externalizes and resolves subject-addressed bytes under one confined HTTPS residence, proving a dual inline/reference payload byte-equal.
- `runtime/serve/route`(`runtime/.planning/serve/route.md`): consumes the core closed frame, admitted JSON and Protobuf codecs, and the publisher-asset Avro codec; resolves `dataref` through the data port and deduplicates each member on `Event.address` before settlement.
- `runtime/net/channel`(`runtime/.planning/net/channel.md`): mints frames through `MQTTMessageFactory`, routes structured JSON through the SDK and Protobuf or Avro through their exact codecs, refuses every batch, and re-admits each SDK or Avro result strictly.
- `effect` `Schema`(`.api/effect.md`): `Binding.toEvent` yields `CloudEventV1<unknown>` or an array of them, so the landing narrows the arity before `Schema.decodeUnknown` decodes `data` and the typed extensions; `Effect.try` lifts every construction throw.
- `effect` `Match`(`.api/effect.md`): binding and content-mode selection dispatch through owned literal rows, since the package's `Mode` is an `enum` the branch cannot declare.
- `@bufbuild/protobuf`/`@msgpack/msgpack`(`core/.api/`) and `avsc`(`runtime/.api/avsc.md`): those own the `data` payload codec while this package owns the JSON message envelope and its transport headers around that payload.
- `mqtt`(`runtime/.api/mqtt.md`): owns connectivity; `MQTTMessageFactory` builds the frame this package's binding shapes and `mqtt` publishes.
- `runtime/net/pubsub`(`runtime/.planning/net/pubsub.md`): Kafka uses its SDK binding; NATS and browser tabs carry the exact structured JSON single codec; every decoded event crosses `Event.admit`.
- `runtime/work/deliver`(`runtime/.planning/work/deliver.md`): projects the stored announcement at claim time through the mode's own binding and signs the exact encoded transport octets once before any reserialization.
- `core/value/fault`(`core/.planning/value/fault.md`): `ValidationError.errors` becomes `Fault.Class` evidence inside `Effect.try`; a bare `TypeError` from the cross-version guard folds onto the same rail.

[LOCAL_ADMISSION]:
- Supply `id`, `time`, and `specversion` at every mint, so no message envelope carries an SDK-minted identity or wall-clock instant.
- Run strict construction inside `Effect.try` and catch `TypeError`, since `ValidationError` extends it and the cross-version guard throws the base class.
- Read and write tracing, baggage, and every rostered extension through the carrier folds and the branch roster; a raw `CloudEvent[key]` read bypasses the only surface that knows the roster.
- Leave `CE_USE_BIG_INT` unset: it swaps the process-wide `JSON` binding, so i64 fidelity rides the payload codec instead.
- Decode with the arity narrowed first — `Deserializer` returns one message envelope or an array — and never route a batch content type down a single-message path.
- Select the binding and the content mode as owned typed data above `emitterFor`, and never spell a `ce-`, `ce_`, or unprefixed header literal beside the binding that owns it.
- Refuse `Emitter`, `event.emit()`, `strict: false` on untrusted bytes, and every `cloudevents/dist/*` deep path.
- Encode the body to bytes once, before any signature, since `toString` and `toJSON` re-format `time` on every call.
