# [RASM_APPHOST_API_HEALTHCHECKS_URIS]

`AspNetCore.HealthChecks.Uris` (Xabaril) is one concrete `IHealthCheck` (`UriHealthCheck`) that grades outbound HTTP(S) endpoint liveness across a URI set, scoring each probe against a status-code window, an optional exact body match, and a per-request timeout, registered through the `AddUrlGroup` builder family. It enters the AppHost health fold as one `Remote`-tagged driver probe over a service-discovery-resolved endpoint; a faulted upstream routes through the `ReducedRemote` degradation rule, never a thrown exception.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.Uris`
- package: `AspNetCore.HealthChecks.Uris`
- license: `Apache-2.0`
- assembly: `HealthChecks.Uris`
- namespace: `HealthChecks.Uris`
- namespace: `Microsoft.Extensions.DependencyInjection`
- target: `net8.0` (also `netstandard2.0`); the `net10.0` consumer binds the `net8.0` asset
- asset: runtime library
- rail: health-outbound

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: contributor and per-probe policy

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `UriHealthCheck`        | contributor   | `IHealthCheck` grading an HTTP(S) probe set   |
|  [02]   | `UriHealthCheckOptions` | options       | group default method, timeout, status window  |
|  [03]   | `IUriOptions`           | interface     | per-endpoint override contract                |
|  [04]   | `UriOptions`            | value         | `IUriOptions` value carrying one probe policy |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration overloads — `Microsoft.Extensions.DependencyInjection`

Every overload is a `static` extension on `IHealthChecksBuilder` returning it for chaining, carrying the shared `name`, `failureStatus`, `tags`, `timeout`, `configureClient`, and `configurePrimaryHttpMessageHandler` tail; `configureClient` (`Action<IServiceProvider, HttpClient>`) and `configurePrimaryHttpMessageHandler` (`Func<IServiceProvider, HttpMessageHandler>`) are the seam binding the probe's named `HttpClient` to the host resilience handler and service-discovery resolver.

| [INDEX] | [SURFACE]                                    | [CAPABILITY]                                   |
| :-----: | :------------------------------------------- | :--------------------------------------------- |
|  [01]   | `AddUrlGroup(Uri)`                           | one endpoint, default GET                      |
|  [02]   | `AddUrlGroup(Uri, HttpMethod)`               | one endpoint, explicit verb                    |
|  [03]   | `AddUrlGroup(IEnumerable<Uri>)`              | many endpoints, default GET                    |
|  [04]   | `AddUrlGroup(IEnumerable<Uri>, HttpMethod)`  | many endpoints, explicit verb                  |
|  [05]   | `AddUrlGroup(Action<UriHealthCheckOptions>)` | per-URI method, timeout, code, content, header |
|  [06]   | `AddUrlGroup(Func<IServiceProvider, Uri>)`   | endpoint resolved from DI (service discovery)  |

[ENTRYPOINT_SCOPE]: fluent group assembly — `UriHealthCheckOptions`

Each member is an `instance` fluent call returning `UriHealthCheckOptions`.

| [INDEX] | [SURFACE]                           | [CAPABILITY]                           |
| :-----: | :---------------------------------- | :------------------------------------- |
|  [01]   | `AddUri(Uri, Action<IUriOptions>?)` | add one endpoint with per-URI override |
|  [02]   | `UseGet()` / `UsePost()`            | group-wide GET/POST                    |
|  [03]   | `UseHttpMethod(HttpMethod)`         | group-wide arbitrary verb              |
|  [04]   | `UseTimeout(TimeSpan)`              | group-wide per-probe timeout           |
|  [05]   | `ExpectHttpCode(int)`               | exact expected code                    |
|  [06]   | `ExpectHttpCodes(int, int)`         | inclusive expected-code window         |

[ENTRYPOINT_SCOPE]: per-URI override — `IUriOptions`

Each member is an `instance` fluent call returning `IUriOptions`, overriding the group default for one endpoint.

| [INDEX] | [SURFACE]                         | [CAPABILITY]                 |
| :-----: | :-------------------------------- | :--------------------------- |
|  [01]   | `UseHttpMethod(HttpMethod)`       | override method for one URI  |
|  [02]   | `UseTimeout(TimeSpan)`            | override timeout for one URI |
|  [03]   | `ExpectHttpCode(int)`             | exact code for one URI       |
|  [04]   | `ExpectHttpCodes(int, int)`       | code window for one URI      |
|  [05]   | `ExpectContent(string)`           | exact required response body |
|  [06]   | `AddCustomHeader(string, string)` | request header (auth bearer) |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- contributor shape: `UriHealthCheck(UriHealthCheckOptions, Func<HttpClient>)` invokes the factory per URI and runs `CheckHealthAsync(HealthCheckContext, CancellationToken)` across the configured set.
- probe grading: a URI passes when its status falls inside the `ExpectedHttpCodes` `(int Min, int Max)` window and, when `ExpectedContent` is set, the whole response body equals it; the first failing URI short-circuits the group to `failureStatus`.
- per-URI vs group policy: `UriHealthCheckOptions` carries the group default method, timeout, and status window; `AddUri(uri, setup)` projects a `UriOptions` whose overrides replace the group default for that endpoint.
- timeout layering: each probe's effective timeout — per-URI `UriOptions.Timeout` when set, else the group `UseTimeout` — bounds one `SendAsync` through a linked `CancellationTokenSource`; the registration `timeout` is the outer `HealthCheckRegistration` ceiling `HealthCheckService` enforces around the whole contributor.
- client construction: each overload registers a named `HttpClient` through `IHttpClientFactory` under the registration name, mapping `configureClient` to `ConfigureHttpClient` and `configurePrimaryHttpMessageHandler` to `ConfigurePrimaryHttpMessageHandler`.

[STACKING]:
- `Microsoft.Extensions.ServiceDiscovery`(`api-service-discovery.md`): the `Func<IServiceProvider, Uri>` overload resolves each probe endpoint, targeting the resolved address the host calls rather than an embedded literal.
- `Microsoft.Extensions.Http.Resilience`(`api-resilience.md`): `configurePrimaryHttpMessageHandler` supplies the shared resilience handler, so probe and live traffic share one circuit-breaker state.
- `OpenIddict.Client`(`api-openiddict-client.md`): `AddCustomHeader` carries the bearer token the client rail acquires for an authenticated endpoint.
- `Microsoft.Extensions.Diagnostics.HealthChecks`(`api-health.md`): `UriHealthCheck` implements `IHealthCheck`, and its `HealthCheckResult` folds into the one `HealthReport`.
- `Observability/health.md`: `DriverProbe.Upstream` binds `UriHealthCheck` as one `Remote`-tagged contributor through `HealthContributorRow.Driver` and `HealthSurface.Register`; a faulted upstream drives `Rule(Remote, Unhealthy, ReducedRemote)`.

[LOCAL_ADMISSION]:
- `HealthContributorRow.Driver(DriverProbe.Upstream, cadence, UriHealthCheck)` admits the probe as one `Remote`-tagged row through `HealthSurface.Register`, never a standalone `AddUrlGroup` face beside the health fold.
- Resolve every probe endpoint through the `Func<IServiceProvider, Uri>` overload against `Microsoft.Extensions.ServiceDiscovery`, so the probe and the live call target one resolved address.
- Carry the probe `HttpClient` through `configurePrimaryHttpMessageHandler` so the resilience circuit-breaker state is shared between live calls and probes.
- Pair `ExpectHttpCodes` with `ExpectContent` for an endpoint whose 200 masks a body-level fault; carry the bearer token through `AddCustomHeader` for an authenticated endpoint.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.Uris`
- Owns: outbound HTTP(S) endpoint liveness graded by status window, exact body match, and per-probe timeout
- Accept: `AddUrlGroup` over a URI set with per-URI override through `UriHealthCheckOptions.AddUri`, its `HttpClient` bound to the host resilience and service-discovery seam
- Reject: a hand-rolled `HttpClient` ping loop, a bare-socket reachability check, a probe transport bypassing the host resilience/service-discovery pipeline, a string-status report
