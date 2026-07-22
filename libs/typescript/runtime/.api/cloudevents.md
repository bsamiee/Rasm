# [TS_RUNTIME_API_CLOUDEVENTS]

`cloudevents` supplies the validated CloudEvents envelope and HTTP binding used by webhook egress. `CloudEvent` construction may throw `ValidationError`; `HTTP.binary` and `HTTP.structured` return a transport-neutral `Message` whose headers and body become the signed `HookPayload` bytes only after the runtime boundary performs one explicit byte encoding.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cloudevents`
- package: `cloudevents`
- license: `Apache-2.0`
- runtime: envelope construction is isomorphic; `Message.Headers` extends Node `IncomingHttpHeaders`
- modules: one package barrel exports `CloudEvent`, `ValidationError`, `HTTP`, `Binding`, `Message`, `Headers`, `Serializer`, and `Deserializer`

## [02]-[ENVELOPE]

[PUBLIC_TYPE_SCOPE]: CloudEvents 1.0 envelope construction
- rail: runtime/work webhook egress
- `CloudEventV1<T>` requires `id`, `source`, `type`, and `specversion`; optional members are `datacontenttype`, `dataschema`, `subject`, `time`, `data`, and `data_base64`; `[key: string]: unknown` admits extension attributes such as `traceparent`, `tracestate`, and `baggage`.

| [INDEX] | [SURFACE]                                                                                               | [RETURN]              |
| :-----: | :------------------------------------------------------------------------------------------------------ | :-------------------- |
|  [01]   | `new CloudEvent<T = undefined>(event: Partial<CloudEventV1<T>>, strict?: boolean)`                      | `CloudEvent<T>`       |
|  [02]   | `event.cloneWith(options: Partial<Exclude<CloudEventV1<never>, "data">>, strict?)`                      | `CloudEvent<T>`       |
|  [03]   | `event.cloneWith<D>(options: Partial<CloudEventV1<D>>, strict?)`                                        | `CloudEvent<D>`       |
|  [04]   | `CloudEvent.cloneWith(event: CloudEventV1<any>, options: Partial<CloudEventV1<any>>, strict?: boolean)` | `CloudEvent<any>`     |
|  [05]   | `event.toJSON()` / `event.toString()` / `event.validate()`                                              | record/string/boolean |

`strict` defaults to validation on. Invalid construction and validation throw `ValidationError`; runtime code lifts that throw through `Effect.try` before an envelope enters `HookPayload`.

## [03]-[HTTP_BINDING]

[ENTRYPOINT_SCOPE]: HTTP binary and structured serialization
- rail: runtime/work webhook egress
- `HTTP` is declared as `Binding`; its four members therefore inherit the generic `Serializer`, `Deserializer`, and `Detector` declarations exactly.

```typescript signature
interface Headers extends IncomingHttpHeaders {
  [key: string]: string | string[] | undefined
}

interface Message<T = string> {
  headers: Headers
  body: T | string | Buffer | unknown
}

interface Serializer<M extends Message> {
  <T>(event: CloudEventV1<T>): M
}

interface Deserializer {
  <T>(message: Message): CloudEventV1<T> | CloudEventV1<T>[]
}

interface Detector {
  (message: Message): boolean
}

interface Binding<B extends Message = Message, S extends Message = Message> {
  binary: Serializer<B>
  structured: Serializer<S>
  toEvent: Deserializer
  isEvent: Detector
}

declare const HTTP: Binding
```

| [INDEX] | [SURFACE]                   | [DECLARED_RESULT]                      | [WIRE_ROLE]                                |
| :-----: | :-------------------------- | :------------------------------------- | :----------------------------------------- |
|  [01]   | `HTTP.binary<T>(event)`     | `Message`                              | CloudEvents attributes in `ce-*` headers   |
|  [02]   | `HTTP.structured<T>(event)` | `Message`                              | envelope body plus structured content type |
|  [03]   | `HTTP.toEvent<T>(message)`  | `CloudEventV1<T> \| CloudEventV1<T>[]` | binary, structured, or batch decode        |
|  [04]   | `HTTP.isEvent(message)`     | `boolean`                              | pre-decode detection                       |
|  [05]   | `headersFor<T>(event)`      | `Headers`                              | exported HTTP binary-header projection     |
|  [06]   | `sanitize(headers)`         | `Headers`                              | lowercase and content-type normalization   |
|  [07]   | `allowedContentTypes`       | content-type literal array             | admitted binary payload content types      |
|  [08]   | `requiredHeaders`           | required `ce-*` literal array          | binary envelope minimum                    |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Construct one `CloudEvent` from the owned delivery identity, source, type, payload, media type, and carrier extensions; catch `ValidationError` at construction.
- Select `HTTP.binary` or `HTTP.structured` as an egress modality, preserve its returned headers, and encode its returned body exactly once before signing.
- Keep CloudEvents framing upstream of webhook signing: `HookPayload` signs the final encoded body and headers without reserializing the envelope.

[RAIL_LAW]:
- Package: `cloudevents`
- Owns: `CloudEvent`, `CloudEventV1`, `ValidationError`, `Message`, `Binding`, and the complete `HTTP` binary/structured/toEvent/isEvent family
- Accept: strict construction, explicit binary/structured selection, exact returned headers, one body-to-bytes encoding
- Reject: hand-built CloudEvents headers, unchecked constructor throws, post-signing serialization, and treating `Message.body` as bytes without narrowing
