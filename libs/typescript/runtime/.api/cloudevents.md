# [TS_RUNTIME_API_CLOUDEVENTS]

`cloudevents` mints the validated CloudEvents 1.0 envelope and the HTTP binary/structured binding that carries webhook egress. `CloudEvent` construction throws `ValidationError`; `HTTP.binary` and `HTTP.structured` return a transport-neutral `Message` whose headers and body sign into `HookPayload` only after the runtime boundary encodes the body to bytes once.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cloudevents`
- package: `cloudevents` (Apache-2.0)
- module: one root barrel reaches every documented surface
- runtime: isomorphic envelope construction; `Message.headers` extends Node `IncomingHttpHeaders`
- rail: runtime webhook egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the 1.0 envelope, the transport-neutral message, and the binding protocol primitives

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :---------------- | :------------ | :--------------------------------- |
|  [01]   | `CloudEvent<T>`   | class         | validated 1.0 envelope carrier     |
|  [02]   | `CloudEventV1<T>` | interface     | the 1.0 attribute contract         |
|  [03]   | `ValidationError` | class         | non-conforming construction fault  |
|  [04]   | `Message<T>`      | interface     | transport-neutral headers and body |
|  [05]   | `Binding<B,S>`    | interface     | four-member transport protocol     |
|  [06]   | `Headers`         | interface     | `ce-*` string header map           |
|  [07]   | `Serializer<M>`   | interface     | event-to-message conversion        |
|  [08]   | `Deserializer`    | interface     | message-to-event conversion        |
|  [09]   | `Detector`        | interface     | message CloudEvent detection       |

- `CloudEventV1<T>`: `id`, `source`, `type`, `specversion` required; every other attribute optional, and `[key: string]: unknown` admits extension attributes such as `traceparent`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: envelope construction and clone, and HTTP binary/structured serialization

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :----------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `new CloudEvent<T>(Partial<CloudEventV1<T>>, strict?)`             | ctor     | construct and validate an envelope |
|  [02]   | `event.cloneWith(Partial<CloudEventV1>, strict?) -> CloudEvent`    | instance | clone with updated attributes      |
|  [03]   | `event.toJSON()` / `event.toString()` / `event.validate()`         | instance | project, stringify, validate       |
|  [04]   | `HTTP.binary<T>(event) -> Message`                                 | property | attributes into `ce-*` headers     |
|  [05]   | `HTTP.structured<T>(event) -> Message`                             | property | body plus structured content type  |
|  [06]   | `HTTP.toEvent<T>(message) -> CloudEventV1<T> \| CloudEventV1<T>[]` | property | decode any transport mode          |
|  [07]   | `HTTP.isEvent(message) -> boolean`                                 | property | pre-decode detection               |
|  [08]   | `headersFor<T>(event) -> Headers`                                  | static   | binary-header projection           |
|  [09]   | `sanitize(headers) -> Headers`                                     | static   | lowercase and normalize headers    |
|  [10]   | `allowedContentTypes`                                              | static   | admitted binary content-type set   |
|  [11]   | `requiredHeaders`                                                  | static   | required `ce-*` literal set        |

- `event.cloneWith`: three overloads clone — default excludes `data` and preserves `T`, `<D>` retypes data to `D`, and static `CloudEvent.cloneWith(event, options, strict?)` clones a raw `CloudEventV1`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Construct one `CloudEvent` from delivery identity, source, type, payload, media type, and carrier extensions; `strict` defaults on and a non-conforming envelope throws at construction.
- `HTTP.binary` and `HTTP.structured` return a transport-neutral `Message`; its body encodes to bytes exactly once upstream of webhook signing, and framing never reserializes after the signature lands.

[STACKING]:
- `effect`(`.api/effect.md`): a `BOUNDARY ADAPTER` lifts the `CloudEvent`/`validate` throw through `Effect.try`, so a `ValidationError` becomes a tagged fault on the typed error channel rather than an escaping exception.
- runtime `Hook` egress: `HTTP.binary`/`HTTP.structured` output lands on `HookPayload` headers and body, the body encodes to bytes once, and the `Crypto` service signs those exact bytes into the `webhook-signature` triple.

[RAIL_LAW]:
- Package: `cloudevents`
- Owns: the CloudEvents 1.0 envelope, `ValidationError`, the transport-neutral `Message`/`Binding` contract, and the `HTTP` binary/structured/toEvent/isEvent family
- Accept: strict construction, explicit binary or structured selection, exact returned headers, one body-to-bytes encoding before signing
- Reject: hand-built CloudEvents headers, an unchecked constructor throw, serialization after signing, `Message.body` read as bytes without narrowing
