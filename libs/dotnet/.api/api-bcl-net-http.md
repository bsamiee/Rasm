# [RASM_API_BCL_NET_HTTP]

`System.Net.Http` owns outbound HTTP as a handler chain an invoker drives: `HttpClient` holds one `HttpMessageHandler`, each `DelegatingHandler` link forwards to its inner handler, and a terminal `SocketsHttpHandler` or `HttpClientHandler` moves the bytes. Request and response are disposable messages carrying a validated header collection each, and payload rides a `HttpContent` subclass that serializes on demand rather than at construction.

Both send legs are twin members on every handler — synchronous `Send` beside asynchronous `SendAsync` — and neither delegates to the other, so a link overriding one alone silently passes the other through unobserved.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: invoker and handler chain, the message pair, the content family, headers, and the fault vocabulary.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]                                                             |
| :-----: | :------------------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `HttpMessageInvoker`       | class           | minimal send root over one handler; `IDisposable`                        |
|  [02]   | `HttpClient`               | class           | invoker refinement adding base address, timeout, and verb convenience    |
|  [03]   | `HttpMessageHandler`       | abstract class  | one chain link; `Send` virtual beside `SendAsync` abstract               |
|  [04]   | `DelegatingHandler`        | abstract class  | forwarding link owning a settable `InnerHandler`                         |
|  [05]   | `SocketsHttpHandler`       | sealed class    | managed terminal handler: pooling, keep-alive, connect callback          |
|  [06]   | `HttpClientHandler`        | class           | portable terminal handler over the platform default                      |
|  [07]   | `HttpRequestMessage`       | class           | method, uri, version, headers, options, and optional content             |
|  [08]   | `HttpResponseMessage`      | class           | status, reason, headers, trailers, and non-null content                  |
|  [09]   | `HttpContent`              | abstract class  | payload with its own headers; serializes per send                        |
|  [10]   | `ByteArrayContent`         | class           | memory-backed, re-readable content                                       |
|  [11]   | `StringContent`            | class           | encoding- and media-typed refinement of the byte content                 |
|  [12]   | `StreamContent`            | class           | stream-backed content; single-read unless buffered                       |
|  [13]   | `MultipartContent`         | class           | boundary-delimited composite of child contents                           |
|  [14]   | `MultipartFormDataContent` | class           | form-data refinement carrying per-part name and file name                |
|  [15]   | `HttpMethod`               | class           | interned verb value with equality and the standard statics               |
|  [16]   | `HttpRequestOptions`       | sealed class    | typed per-request property bag keyed by `HttpRequestOptionsKey<TValue>`  |
|  [17]   | `HttpCompletionOption`     | enum            | `ResponseContentRead`, `ResponseHeadersRead`                             |
|  [18]   | `HttpKeepAlivePingPolicy`  | enum            | `WithActiveRequests`, `Always`                                           |
|  [19]   | `HttpVersionPolicy`        | enum            | `RequestVersionOrLower`, `RequestVersionOrHigher`, `RequestVersionExact` |
|  [20]   | `HttpRequestError`         | enum            | twelve transport, TLS, protocol, and limit classes                       |
|  [21]   | `HttpRequestException`     | exception class | carries `HttpRequestError` beside an optional `HttpStatusCode`           |
|  [22]   | `HttpHeaders`              | abstract class  | validated multi-value header store; enumerates as name to values         |
|  [23]   | `HttpRequestHeaders`       | sealed class    | request-side strongly-typed header projection                            |
|  [24]   | `HttpResponseHeaders`      | sealed class    | response-side projection, also carrying trailers                         |
|  [25]   | `HttpContentHeaders`       | sealed class    | content-side projection: length, type, encoding, range, disposition      |
|  [26]   | `RangeHeaderValue`         | class           | byte-range request value over `RangeItemHeaderValue` items               |
|  [27]   | `MediaTypeHeaderValue`     | class           | media type with charset, admitted by content constructors                |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: chain construction, the two send legs, message and content mint, and header writes.

Send parameters resolve as `request` to `HttpRequestMessage`, `token` to `CancellationToken`, and `completion` to `HttpCompletionOption`; token-free overloads exist and carry no cancellation.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]   | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------------- | :-------- | :------------------------------------------- |
|  [01]   | `HttpClient(HttpMessageHandler)`                                            | ctor      | client OWNS and disposes the chain           |
|  [02]   | `HttpClient(HttpMessageHandler, bool)`                                      | ctor      | `disposeHandler` false leaves it to a pool   |
|  [03]   | `HttpClient.Timeout -> TimeSpan`                                            | property  | whole-operation bound, cancels the send      |
|  [04]   | `HttpClient.BaseAddress -> Uri?`                                            | property  | prefix relative request uris resolve on      |
|  [05]   | `HttpClient.DefaultRequestHeaders -> HttpRequestHeaders`                    | property  | per-client headers merged into every send    |
|  [06]   | `HttpClient.DefaultVersionPolicy -> HttpVersionPolicy`                      | property  | version negotiation posture for the client   |
|  [07]   | `HttpMessageInvoker.Send(request, token)`                                   | virtual   | SYNCHRONOUS leg; blocks the calling thread   |
|  [08]   | `HttpMessageInvoker.SendAsync(request, token)`                              | virtual   | asynchronous leg                             |
|  [09]   | `HttpClient.Send(request, completion, token)`                               | instance  | synchronous send under a completion mode     |
|  [10]   | `HttpClient.SendAsync(request, completion, token)`                          | instance  | asynchronous send under a completion mode    |
|  [11]   | `HttpMessageHandler.Send(request, token)`                                   | protected | `protected internal virtual` chain leg       |
|  [12]   | `HttpMessageHandler.SendAsync(request, token)`                              | protected | `protected internal abstract` chain leg      |
|  [13]   | `DelegatingHandler.InnerHandler -> HttpMessageHandler?`                     | property  | settable before the first send alone         |
|  [14]   | `DelegatingHandler(HttpMessageHandler)`                                     | protected | ctor form binding the inner link             |
|  [15]   | `SocketsHttpHandler.ConnectCallback`                                        | property  | supplies the transport stream per connect    |
|  [16]   | `SocketsHttpHandler.PooledConnectionIdleTimeout -> TimeSpan`                | property  | idle eviction bound on the pool              |
|  [17]   | `SocketsHttpHandler.PooledConnectionLifetime -> TimeSpan`                   | property  | absolute recycle bound, DNS-change safety    |
|  [18]   | `SocketsHttpHandler.KeepAlivePingDelay -> TimeSpan`                         | property  | HTTP/2 ping cadence                          |
|  [19]   | `SocketsHttpHandler.KeepAlivePingTimeout -> TimeSpan`                       | property  | ping response bound before teardown          |
|  [20]   | `SocketsHttpHandler.KeepAlivePingPolicy`                                    | property  | ping while streams are active, or always     |
|  [21]   | `SocketsHttpHandler.EnableMultipleHttp2Connections -> bool`                 | property  | lifts the per-authority stream ceiling       |
|  [22]   | `SocketsHttpHandler.ActivityHeadersPropagator`                              | property  | handler-side context injection seat          |
|  [23]   | `SocketsHttpHandler.MeterFactory -> IMeterFactory?`                         | property  | scopes the handler's own metric mint         |
|  [24]   | `SocketsHttpHandler.SslOptions -> SslClientAuthenticationOptions`           | property  | client certificate and validation custody    |
|  [25]   | `HttpClientHandler.MeterFactory -> IMeterFactory?`                          | property  | same scoping on the portable handler         |
|  [26]   | `HttpRequestMessage(HttpMethod, Uri?)`                                      | ctor      | method and target; null resolves the base    |
|  [27]   | `HttpRequestMessage.Content -> HttpContent?`                                | property  | settable payload; null on bodiless verbs     |
|  [28]   | `HttpRequestMessage.Headers -> HttpRequestHeaders`                          | property  | request headers, distinct from content's     |
|  [29]   | `HttpRequestMessage.Options -> HttpRequestOptions`                          | property  | typed per-request bag handlers read          |
|  [30]   | `HttpResponseMessage(HttpStatusCode)`                                       | ctor      | synthesizes a response with empty content    |
|  [31]   | `HttpResponseMessage.IsSuccessStatusCode -> bool`                           | property  | 2xx test with no throw                       |
|  [32]   | `HttpResponseMessage.EnsureSuccessStatusCode()`                             | instance  | throwing form; yields itself on success      |
|  [33]   | `HttpContent.CopyTo(Stream, TransportContext?, token)`                      | instance  | SYNCHRONOUS materialization of the payload   |
|  [34]   | `HttpContent.CopyToAsync(Stream, TransportContext?, token)`                 | instance  | asynchronous materialization                 |
|  [35]   | `HttpContent.LoadIntoBufferAsync(long, token)`                              | instance  | buffers, making a stream content re-readable |
|  [36]   | `HttpContent.ReadAsStream(token)`                                           | instance  | synchronous stream read of the payload       |
|  [37]   | `HttpContent.Headers -> HttpContentHeaders`                                 | property  | content-owned headers; length and encoding   |
|  [38]   | `ByteArrayContent(byte[])`                                                  | ctor      | re-readable memory payload                   |
|  [39]   | `ByteArrayContent(byte[], int, int)`                                        | ctor      | offset and count window over the array       |
|  [40]   | `StringContent(string, Encoding?, MediaTypeHeaderValue?)`                   | ctor      | text payload stamping charset and type       |
|  [41]   | `StreamContent(Stream, int)`                                                | ctor      | stream payload under a copy buffer size      |
|  [42]   | `MultipartFormDataContent.Add(HttpContent, string, string)`                 | instance  | part with its form name and file name        |
|  [43]   | `HttpMethod(string)`                                                        | ctor      | custom verb; statics cover the standard set  |
|  [44]   | `HttpMethod.Parse(ReadOnlySpan<char>)`                                      | static    | allocation-free admission of a known verb    |
|  [45]   | `HttpHeaders.TryAddWithoutValidation(string, IEnumerable<string?>) -> bool` | instance  | multi-value copy skipping parse and format   |
|  [46]   | `HttpHeaders.TryAddWithoutValidation(string, string?) -> bool`              | instance  | single-value form                            |
|  [47]   | `HttpHeaders.Add(string, string?)`                                          | instance  | validating add; throws on a malformed value  |
|  [48]   | `HttpHeaders.TryGetValues(string, out IEnumerable<string>?) -> bool`        | instance  | read without materializing an exception      |
|  [49]   | `HttpHeaders.NonValidated -> HttpHeadersNonValidated`                       | property  | raw view that never triggers lazy parsing    |
|  [50]   | `RangeHeaderValue(long?, long?)`                                            | ctor      | one byte range; null end reads to the tail   |
|  [51]   | `SslClientAuthenticationOptions.ClientCertificates`                         | property  | mutual-auth identity the handshake presents  |
|  [52]   | `SslClientAuthenticationOptions.CertificateChainPolicy`                     | property  | chain-build policy the server cert proves on |
|  [53]   | `SslClientAuthenticationOptions.RemoteCertificateValidationCallback`        | property  | caller verdict superseding the chain build   |

- `HttpMethod` statics: `Get`, `Post`, `Put`, `Patch`, `Delete`, `Head`, `Options`, `Trace`, `Connect`, `Query`.
- `HttpRequestException(HttpRequestError, string?, Exception?, HttpStatusCode?)` is the classifying ctor.
- `HttpHeaders` enumerates as `KeyValuePair<string, IEnumerable<string>>`.
- `TryAddWithoutValidation` takes `IEnumerable<string?>`, so an enumerated header copies across under covariance.
- `SocketsHttpHandler.SslOptions` is the one client-TLS seat: `HttpClientHandler` exposes `ClientCertificates` and a validation callback but no chain policy, so a custom trust anchor reaches only the sockets handler; `CertificateChainPolicy` NARROWS the anchor set while `RemoteCertificateValidationCallback` replaces the verdict outright, and setting the callback to an unconditional true disables verification whatever the policy says.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `HttpMessageHandler` declares `SendAsync` abstract and `Send` virtual, so a link overriding the async leg alone compiles clean.
- Base `Send` throws instead of forwarding, so a one-leg `DelegatingHandler` observes half the traffic.
- `HttpClient.Send` drives `HttpMessageHandler.Send` down the whole chain and never enters `SendAsync`.
- `HttpClient(HttpMessageHandler)` disposes the chain with the client; `disposeHandler` false hands lifetime to a pool.
- Chains reach no other release seat, so a client nobody disposes leaks its connection pool.
- `HttpClient.Timeout` bounds the whole operation and surfaces as `TaskCanceledException`, never a timeout type.
- `HttpContent` serializes per send, so payload cost repeats on every attempt.
- `ByteArrayContent` and its refinements stay memory-backed and re-readable.
- `StreamContent` reads once until `LoadIntoBufferAsync` buffers it, so an unbuffered retry sends an empty body.
- `HttpContent.Headers` owns `Content-Length`, `Content-Type`, and `Content-Encoding`; the message owns the rest.
- Copy folds reading one header store alone reproduce half a request.
- Sending stacks derive `Content-Length` and `Transfer-Encoding` from the content handed them, so a copied framing header contradicts the body sent.
- `HttpStatusCode` and `HttpVersion` are `System.Net.Primitives` value types this assembly consumes.
- This assembly declares `HttpVersionPolicy` itself, under the `System.Net` namespace.

[STACKING]:
- `System.Diagnostics.DiagnosticSource`(`.api/api-diagnostics-activity.md`): `ActivityHeadersPropagator` owns per-hop `traceparent` injection.
- `System.Diagnostics.Metrics`(`.api/api-diagnostics-metrics.md`): `MeterFactory` scopes each handler's `http.client.*` streams.
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`(`.api/api-opentelemetry-exporter-otlp.md`): `OtlpExporterOptions.HttpClientFactory` seats the chain.
- `OpenTelemetry.Instrumentation.Http`(`.api/api-otel-instrumentation-http.md`): instrumentation enriches off the message pair rowed here.
- `Microsoft.Extensions.Http.Resilience`: standard and hedging pipelines install as `DelegatingHandler` links, so link ORDER binds.
- `Rasm.AppHost` `Observability/telemetry#SIGNAL_GOVERNANCE`: `PersistentOtlpHandler` binds both legs; the exporter's client sends synchronously.
- `Rasm.AppHost` `Observability/telemetry#SIGNAL_GOVERNANCE`: `OtlpOfflineQueue` replays a failed batch as `ByteArrayContent`.
- `Rasm.AppHost` `Wire/outbound#KEYED_PIPELINES`: `Discovery.Connect` binds `ConnectCallback` to a Unix-domain socket.
- `Rasm.Persistence` `Store/blobstore`: ranged reads pair `RangeHeaderValue` with `HttpCompletionOption.ResponseHeadersRead` so an object streams.

[LOCAL_ADMISSION]:
- Composition roots own every terminal handler and client, handing consumers the client alone.
- Custom links override `Send` and `SendAsync` together, routing both to one core.
- Replayed payloads ride `ByteArrayContent`, with framing headers re-derived from the replay body.
- Faults classify on `HttpRequestError` and `HttpResponseMessage.StatusCode`.
