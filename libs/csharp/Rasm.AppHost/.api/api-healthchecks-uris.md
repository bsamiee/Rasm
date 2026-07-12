# [RASM_APPHOST_API_HEALTHCHECKS_URIS]

`AspNetCore.HealthChecks.Uris` supplies a single `IHealthCheck` contributor (`UriHealthCheck`) that probes one or more outbound HTTP(S) endpoints and grades each against an expected-status-code window, an expected response substring, and a per-probe timeout, registered through the `AddUrlGroup` builder family. It is the outbound-dependency leg of the AppHost health rail: where `AspNetCore.HealthChecks.NpgSql`, `.Redis`, and `.Kafka` probe owned backing services, `.Uris` probes the upstream HTTP services the host calls — the same endpoints `Microsoft.Extensions.ServiceDiscovery` resolves and `Microsoft.Extensions.Http.Resilience` guards — and folds the result into the one `HealthReport` projected by `Microsoft.Extensions.Diagnostics.HealthChecks`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.Uris`
- package: `AspNetCore.HealthChecks.Uris`
- assembly: `HealthChecks.Uris`
- namespace: `HealthChecks.Uris`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- license: Apache-2.0
- abi: ships `lib/net8.0` + `lib/netstandard2.0`; no `net10.0` asset, so the workspace `net10.0` consumer binds the `net8.0` facade (forward-compatible, no consumer-TFM surface drift)
- rail: health-outbound

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: contributor and per-probe policy — `HealthChecks.Uris`
- rail: health-outbound

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]        | [RAIL]                                   |
| :-----: | :---------------------- | :------------------- | :--------------------------------------- |
|  [01]   | `UriHealthCheck`        | contributor class    | `IHealthCheck` over an HTTP probe set    |
|  [02]   | `UriHealthCheckOptions` | fluent options       | URI set, method, timeout, status window  |
|  [03]   | `IUriOptions`           | per-URI policy       | per-endpoint method/timeout/code/headers |
|  [04]   | `UriOptions`            | per-URI policy value | `IUriOptions` implementation             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration overloads — `Microsoft.Extensions.DependencyInjection`
- rail: health-outbound

Every overload returns `IHealthChecksBuilder` for chaining, and every overload accepts the shared `name`, `failureStatus`, `tags`, `timeout`, `configureClient`, and `configurePrimaryHttpMessageHandler` tail. The `Action<IServiceProvider, HttpClient>` and `Func<IServiceProvider, HttpMessageHandler>` hooks are the seam where the probe's `HttpClient` adopts the host's resilience handler and service-discovery resolver rather than a bare socket.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY]   | [RAIL]                                        |
| :-----: | :------------------------------------------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `AddUrlGroup(Uri uri, …)`                                      | single-URI GET   | one endpoint, default GET                     |
|  [02]   | `AddUrlGroup(Uri uri, HttpMethod httpMethod, …)`               | single-URI verb  | one endpoint, explicit method                 |
|  [03]   | `AddUrlGroup(IEnumerable<Uri> uris, …)`                        | URI-group GET    | many endpoints, default GET                   |
|  [04]   | `AddUrlGroup(IEnumerable<Uri> uris, HttpMethod httpMethod, …)` | URI-group verb   | many endpoints, explicit method               |
|  [05]   | `AddUrlGroup(Action<UriHealthCheckOptions> uriOptions, …)`     | options-driven   | per-URI method/timeout/code/content/headers   |
|  [06]   | `AddUrlGroup(Func<IServiceProvider, Uri> uriProvider, …)`      | resolved-URI GET | endpoint resolved from DI (service discovery) |

[ENTRYPOINT_SCOPE]: fluent option assembly — `UriHealthCheckOptions`
- rail: health-outbound

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]  | [RAIL]                                  |
| :-----: | :-------------------------------------------------- | :-------------- | :-------------------------------------- |
|  [01]   | `AddUri(Uri uriToAdd, Action<IUriOptions>? setup)`  | URI admission   | adds one endpoint with per-URI override |
|  [02]   | `UseGet()` / `UsePost()`                            | default method  | sets group-wide GET/POST                |
|  [03]   | `UseHttpMethod(HttpMethod methodToUse)`             | default method  | sets group-wide arbitrary verb          |
|  [04]   | `UseTimeout(TimeSpan timeout)`                      | default timeout | sets group-wide per-probe timeout       |
|  [05]   | `ExpectHttpCode(int codeToExpect)`                  | status policy   | exact expected code                     |
|  [06]   | `ExpectHttpCodes(int minCodeToExpect, int maxCode)` | status policy   | inclusive expected-code window          |

[ENTRYPOINT_SCOPE]: per-URI policy override — `IUriOptions`
- rail: health-outbound

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]  | [RAIL]                                  |
| :-----: | :----------------------------------------- | :-------------- | :-------------------------------------- |
|  [01]   | `UseHttpMethod(HttpMethod method)`         | per-URI method  | overrides the group method for one URI  |
|  [02]   | `UseTimeout(TimeSpan timeout)`             | per-URI timeout | overrides the group timeout for one URI |
|  [03]   | `ExpectHttpCode(int code)`                 | per-URI status  | exact code for one URI                  |
|  [04]   | `ExpectHttpCodes(int min, int max)`        | per-URI status  | code window for one URI                 |
|  [05]   | `ExpectContent(string expectedContent)`    | per-URI body    | required response substring             |
|  [06]   | `AddCustomHeader(string name, string val)` | per-URI header  | request header (e.g. auth bearer)       |

## [04]-[IMPLEMENTATION_LAW]

[URIS_TOPOLOGY]:
- contributor shape: `UriHealthCheck(UriHealthCheckOptions options, Func<HttpClient> httpClientFactory)` resolves a fresh `HttpClient` per evaluation from the factory and runs `CheckHealthAsync(HealthCheckContext, CancellationToken)` across the configured URI set
- probe grading: each URI passes when the response status falls inside its `ExpectedHttpCodes` `(int Min, int Max)?` window and, when set, the body contains `ExpectedContent`; the first failing URI short-circuits the group to the registered `failureStatus`
- per-URI vs group policy: `UriHealthCheckOptions` carries the group default method/timeout/status; `AddUri(uri, setup)` projects an `IUriOptions` whose `UseHttpMethod`/`UseTimeout`/`ExpectHttpCode(s)`/`ExpectContent`/`AddCustomHeader` override the group default for that one endpoint
- timeout layering: `UriOptions.Timeout` is the per-probe `HttpClient.Timeout`; the registration `timeout` is the outer `HealthCheckRegistration.Timeout` ceiling the `HealthCheckService` enforces around the whole contributor
- client construction: the `AddUrlGroup` overloads register a named `HttpClient` through `IHttpClientFactory`; `configureClient` mutates that client and `configurePrimaryHttpMessageHandler` supplies its primary handler — the hooks that thread the host's resilience pipeline and service-discovery resolver into the probe transport

[LOCAL_ADMISSION]:
- Register one `AddUrlGroup` per outbound dependency class, naming each probe and tagging it (`"outbound"`, `"ready"`) so the readiness projection filters to outbound liveness through `CheckHealthAsync(predicate)`.
- Resolve endpoints through the `Func<IServiceProvider, Uri>` overload or `configureClient` against `Microsoft.Extensions.ServiceDiscovery` rather than embedding absolute URIs, so the probe targets the same resolved endpoint the host calls.
- Set `failureStatus` to `HealthStatus.Degraded` for soft dependencies and `Unhealthy` for hard ones; the status maps into the typed degradation receipt the host's health rail projects, never an exception.
- Use `ExpectHttpCodes(min, max)` plus `ExpectContent` for endpoints whose 200 masks a body-level fault; use `AddCustomHeader` to carry the bearer token acquired through the `OpenIddict.Client` rail when the probed endpoint is authenticated.
- Carry the probe `HttpClient` through `configurePrimaryHttpMessageHandler` so the resilience handler's circuit-breaker state is shared, not duplicated, between live calls and health probes.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.Uris`
- Owns: outbound HTTP(S) endpoint liveness probing graded by status window, body content, and per-probe timeout
- Accept: `AddUrlGroup` registration over a URI set with named/tagged/failure-status policy; per-URI override through `UriHealthCheckOptions.AddUri`
- Reject: hand-rolled `HttpClient` ping loops, bare-socket reachability checks, probe clients that bypass the host's resilience/service-discovery pipeline, string-status health reporting
