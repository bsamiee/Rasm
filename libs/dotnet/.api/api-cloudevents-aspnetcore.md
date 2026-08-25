# [RASM_API_CLOUDEVENTS_ASPNETCORE]

`CloudNative.CloudEvents.AspNetCore` binds the CloudEvents HTTP protocol binding onto ASP.NET Core's own request and response abstractions: two extension classes carrying the content probe, the single-event and batch decode off an `HttpRequest`, and the single-event and batch write onto an `HttpResponse`.

`Rasm.AppHost` is the one folder that reaches it, serving the estate's HTTP ingress and its abuse-protection handshake over the envelope owner at `Rasm/Domain/event#ENVELOPE_MINT`. This package ships no model binder, no input formatter, and no middleware — an endpoint calls the extensions directly.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two extension classes

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [CAPABILITY]                                        |
| :-----: | :----------------------- | :--------------- | :-------------------------------------------------- |
|  [01]   | `HttpRequestExtensions`  | extension static | `HttpRequest` content probe and single/batch decode |
|  [02]   | `HttpResponseExtensions` | extension static | `CloudEvent` and batch write onto an `HttpResponse` |

- [01]-[REQUEST_PROBE]: `IsCloudEvent(this HttpRequest)` answers true on a CloudEvents content type OR a present `ce-specversion` header, so it covers structured and binary mode and is FALSE for a batch; `IsCloudEventBatch(this HttpRequest)` answers the batch content type alone, so a route serving both calls both.
- [01]-[REQUEST_DECODE]: `ToCloudEventAsync(formatter[, extensions])` carries a `params CloudEventAttribute[]?` and an `IEnumerable<CloudEventAttribute>?` arity, the array form forwarding to the enumerable one; a CloudEvents content type takes the structured leg, otherwise the header leg reads `ce-*` headers through `HttpUtilities` and decodes the body as binary-mode data. Absent `ce-specversion` on a non-CloudEvents content type raises `ArgumentException`.
- [01]-[BATCH_DECODE]: `ToCloudEventBatchAsync(formatter[, extensions])` in the same two arities; it raises `ArgumentException` where the content type is not the batch media type — there is no header-mode batch.
- [02]-[RESPONSE_WRITE]: `CopyToHttpResponseAsync(this CloudEvent, HttpResponse, ContentMode, formatter)` writes structured or binary mode; `CopyToHttpResponseAsync(this IReadOnlyList<CloudEvent>, HttpResponse, formatter)` writes the batch body and takes NO content mode, because batch carries only structured framing.
- [02]-[HEADER_WRITE]: single-event writes append `ce-specversion` and one `ce-<name>` header per populated attribute EXCEPT `datacontenttype`, which lands on `Content-Type`; every value crosses `HttpUtilities.EncodeHeaderValue`.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: request decode and response write

| [INDEX] | [SURFACE]                                                            | [SHAPE]     | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------- | :---------- | :----------------------------------------------- |
|  [01]   | `request.IsCloudEvent()`                                             | probe       | single-event content or `ce-specversion` header  |
|  [02]   | `request.IsCloudEventBatch()`                                        | probe       | batch content type alone                         |
|  [03]   | `request.ToCloudEventAsync(formatter, params extensions)`            | ingress map | structured or binary → one `CloudEvent`          |
|  [04]   | `request.ToCloudEventAsync(formatter, IEnumerable<extensions>)`      | ingress map | the enumerable-roster arity both forms funnel to |
|  [05]   | `request.ToCloudEventBatchAsync(formatter, params extensions)`       | ingress map | batch body → `IReadOnlyList<CloudEvent>`         |
|  [06]   | `request.ToCloudEventBatchAsync(formatter, IEnumerable<extensions>)` | ingress map | the enumerable-roster batch arity                |
|  [07]   | `ce.CopyToHttpResponseAsync(response, contentMode, formatter)`       | egress map  | structured or binary write, headers stamped      |
|  [08]   | `events.CopyToHttpResponseAsync(response, formatter)`                | egress map  | batch write; no content-mode parameter exists    |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Extension methods are the WHOLE package: it registers nothing, hosts nothing, and carries no middleware, model binder, input formatter, or result type, so an endpoint composes the decode and the write directly and no startup call exists to forget.
- Binary-mode ingress reads every `ce-`-prefixed header through `HttpUtilities.GetAttributeNameFromHeaderName` and `DecodeHeaderValue`, sets each through `SetAttributeFromString`, then assigns `DataContentType` from the request's own `Content-Type` before the formatter decodes the body — so an extension declared in the roster crosses typed and an undeclared one crosses as a string.
- Both decode legs close on `Validation.CheckCloudEventArgument`, so an incomplete envelope raises at the boundary rather than reaching a handler.
- Single-event writes refuse an empty `datacontenttype` where the body is non-empty and write `Content-Length` before streaming the body, so a chunked response is not this surface's shape.
- Binary-mode writes resolve their content type through `formatter.GetOrInferDataContentType`, so a formatter that infers nothing and an envelope that declares nothing yield the refusal above rather than an untyped body.
- Batch surfaces are content-type-only in BOTH directions: a header-mode batch has no spelling, so a producer batching over HTTP frames structured.

[STACKING]:
- `api-cloudevents.md`: that catalogue owns the envelope, the `CloudEventFormatter` contract, `ContentMode`, `MimeUtilities`, and the `HttpUtilities` header grammar; this package binds them onto the ASP.NET Core abstractions alone, and the sibling `.Http` classes there serve `HttpClient`, `HttpListener`, and `HttpWebRequest` hosts this estate does not run.
- Kernel owner anchor: every call takes the `EventFormat` row's formatter; each ingress supplies a generated-descriptor declaration for the `event.Extensions` fields it consumes, so no kernel wire roster or attribute literal survives.
- `Rasm.AppHost` consumer anchor: the HTTP ingress route probes, decodes under its generated-descriptor declarations, and admits tenancy before its injected domain projection routes the envelope; the abuse-protection `OPTIONS` handshake and its `WebHook-*` headers remain route-owned.

[LOCAL_ADMISSION]:
- Every call takes the shared formatter instance and the one declared extension roster; a call-site roster literal and a per-request formatter are both the rejected forms.
- Routes serving both single and batch bodies probe with both members, because `IsCloudEvent` answers FALSE for a batch by design.
- Response writes go through these extensions, so no leg hand-stamps a `ce-` header or hand-serializes an envelope into a body.
