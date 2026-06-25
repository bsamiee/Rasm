# [RASM_APPHOST_API_IDENTITYMODEL_PROTOCOLS]

`Microsoft.IdentityModel.Protocols` is the protocol-agnostic configuration-refresh substrate shared by the IdentityModel validation leg and the OIDC discovery leg: it owns `ConfigurationManager<T>` (the concrete `BaseConfigurationManager` that caches a discovery document, refreshes it on an interval or on demand, and falls back to last-known-good), the `IDocumentRetriever`/`HttpDocumentRetriever` fetch contract, the `IConfigurationRetriever<T>` parse contract, and the `IConfigurationValidator<T>` sufficiency contract. It carries no OIDC vocabulary itself — `Microsoft.IdentityModel.Protocols.OpenIdConnect` supplies the `T = OpenIdConnectConfiguration` retriever/validator that specialize these generics. This is the actual integration seam: a `ConfigurationManager<OpenIdConnectConfiguration>` constructed here is the instance assigned to `TokenValidationParameters.ConfigurationManager`/`ValidationParameters.ConfigurationManager`, so its refreshed `JsonWebKeySet` becomes the rotating signing-key source consumed by `Microsoft.IdentityModel.JsonWebTokens`. It is a transitive dependency of `Microsoft.IdentityModel.Protocols.OpenIdConnect`, version-locked to that admission; a design page that constructs the manager directly composes against this catalog.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IdentityModel.Protocols`
- package: `Microsoft.IdentityModel.Protocols`
- assembly: `Microsoft.IdentityModel.Protocols`
- namespace: `Microsoft.IdentityModel.Protocols`
- asset: runtime library
- license: MIT
- abi: native `lib/net10.0` asset (consumer-bound); also `net8.0`/`net6.0`/`netstandard2.0`/`net462`/`net472`
- admission: transitive via `Microsoft.IdentityModel.Protocols.OpenIdConnect` (no explicit central pin — version-locked to the OpenIdConnect admission); `BaseConfigurationManager`, `BaseConfiguration`, and the refresh knobs (`AutomaticRefreshInterval`, `RefreshInterval`, `LastKnownGoodConfiguration`, `LastKnownGoodLifetime`, `UseLastKnownGoodConfiguration`, `MetadataAddress`) live in `Microsoft.IdentityModel.Tokens`
- depends: `Microsoft.IdentityModel.Tokens` (supplies `BaseConfigurationManager`, `BaseConfiguration`)
- rail: protocols-config

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: refresh manager and configuration contracts — `Microsoft.IdentityModel.Protocols`
- rail: protocols-config

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]      | [RAIL]                                          |
| :-----: | :---------------------------------- | :----------------- | :---------------------------------------------- |
|  [01]   | `ConfigurationManager<T>`           | refresh manager    | `BaseConfigurationManager` cache + interval/LKG refresh |
|  [02]   | `StaticConfigurationManager<T>`     | pinned manager     | non-refreshing fixed-configuration manager      |
|  [03]   | `IConfigurationManager<T>`          | manager contract   | `GetConfigurationAsync(CT)` abstraction         |
|  [04]   | `IConfigurationRetriever<T>`        | parse contract     | document text -> typed configuration            |
|  [05]   | `IConfigurationValidator<T>`        | validator contract | configuration-sufficiency check                 |
|  [06]   | `ConfigurationValidationResult`     | validation result  | `Succeeded` + `ErrorMessage`                    |
|  [07]   | `IConfigurationEventHandler<T>`     | refresh hooks       | before-retrieve / after-update retrieval events |
|  [08]   | `LastKnownGoodConfigurationCacheOptions` | LKG cache options | size/expiration tuning for the LKG entry cache |

[PUBLIC_TYPE_SCOPE]: document retrieval — `Microsoft.IdentityModel.Protocols`
- rail: protocols-config

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                                       |
| :-----: | :------------------------ | :-------------- | :------------------------------------------- |
|  [01]   | `IDocumentRetriever`      | fetch contract  | address -> raw document text                 |
|  [02]   | `HttpDocumentRetriever`   | HTTP retriever  | `HttpClient`-backed metadata fetch over HTTPS |
|  [03]   | `FileDocumentRetriever`   | file retriever  | local-file metadata fetch (test/offline)     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: manager construction and refresh — `ConfigurationManager<T>`
- rail: protocols-config

The discovery-fed seam constructs `ConfigurationManager<OpenIdConnectConfiguration>` from a metadata address, the OIDC `IConfigurationRetriever<T>`, and an `IDocumentRetriever` (default `HttpDocumentRetriever`); it inherits the refresh/LKG knobs from `BaseConfigurationManager` and is assigned to `TokenValidationParameters.ConfigurationManager`.

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY]   | [RAIL]                                  |
| :-----: | :----------------------------------------------------------------------------------------- | :--------------- | :-------------------------------------- |
|  [01]   | `new ConfigurationManager<T>(string metadataAddress, IConfigurationRetriever<T>)`          | ctor (default)   | default `HttpDocumentRetriever`         |
|  [02]   | `new ConfigurationManager<T>(string, IConfigurationRetriever<T>, HttpClient)`              | ctor (client)    | host-supplied `HttpClient` retriever    |
|  [03]   | `new ConfigurationManager<T>(string, IConfigurationRetriever<T>, IDocumentRetriever)`      | ctor (retriever) | explicit document retriever             |
|  [04]   | `new ConfigurationManager<T>(string, IConfigurationRetriever<T>, IDocumentRetriever, IConfigurationValidator<T>)` | ctor (validated) | adds JWKS-sufficiency validation |
|  [05]   | `new ConfigurationManager<T>(string, …, LastKnownGoodConfigurationCacheOptions)`           | ctor (LKG-tuned) | tunes the LKG entry cache               |
|  [06]   | `Task<T> GetConfigurationAsync([CancellationToken])`                                       | cached read      | returns cached config, refreshes if stale |
|  [07]   | `override Task<BaseConfiguration> GetBaseConfigurationAsync(CancellationToken)`            | base-typed read  | `TokenValidationParameters` consumes this |
|  [08]   | `override void RequestRefresh()`                                                          | forced refresh   | flags next read to refetch (key-not-found) |
|  [09]   | `ConfigurationEventHandler { get; set; }`                                                 | refresh hooks    | `IConfigurationEventHandler<T>` before/after |

[ENTRYPOINT_SCOPE]: inherited refresh + LKG knobs — `BaseConfigurationManager` (from `Microsoft.IdentityModel.Tokens`)
- rail: protocols-config

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]   | [RAIL]                                  |
| :-----: | :--------------------------------------------------- | :--------------- | :-------------------------------------- |
|  [01]   | `string MetadataAddress { get; set; }`               | source address   | the `.well-known` document URL          |
|  [02]   | `TimeSpan AutomaticRefreshInterval { get; set; }`    | refresh cadence  | background re-fetch interval            |
|  [03]   | `TimeSpan RefreshInterval { get; set; }`             | manual-refresh floor | min interval between `RequestRefresh` |
|  [04]   | `bool UseLastKnownGoodConfiguration { get; set; }`   | LKG toggle       | default `true`; survive transient fetch failures |
|  [05]   | `BaseConfiguration LastKnownGoodConfiguration { get; set; }` | LKG read | the cached last-good configuration      |
|  [06]   | `TimeSpan LastKnownGoodLifetime { get; set; }`       | LKG lifetime     | how long an LKG entry stays valid       |

[ENTRYPOINT_SCOPE]: retrieval and validation contracts
- rail: protocols-config

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY]   | [RAIL]                                  |
| :-----: | :----------------------------------------------------------------------------- | :--------------- | :-------------------------------------- |
|  [01]   | `IConfigurationRetriever<T>.GetConfigurationAsync(string address, IDocumentRetriever, CT)` | parse | the OIDC retriever specializes `T` |
|  [02]   | `IDocumentRetriever.GetDocumentAsync(string address, CancellationToken)`       | fetch            | raw document text                       |
|  [03]   | `new HttpDocumentRetriever()` / `new HttpDocumentRetriever(HttpClient)`         | retriever ctor   | default vs host-`HttpClient` fetch      |
|  [04]   | `HttpDocumentRetriever.RequireHttps { get; set; }`                             | transport guard  | default `true`; rejects non-HTTPS addresses |
|  [05]   | `HttpDocumentRetriever.HttpVersion` / `HttpVersionPolicy`                       | protocol policy  | `Version` + `HttpVersionPolicy?` per request |
|  [06]   | `IConfigurationValidator<T>.Validate(T) : ConfigurationValidationResult`        | sufficiency      | `Succeeded`/`ErrorMessage` gate         |
|  [07]   | `new StaticConfigurationManager<T>(T configuration)`                            | pinned manager   | fixed configuration, `RequestRefresh` no-op |

## [04]-[IMPLEMENTATION_LAW]

[PROTOCOLS_TOPOLOGY]:
- refresh manager: `ConfigurationManager<T> : BaseConfigurationManager, IConfigurationManager<T> where T : class` wraps `(MetadataAddress, IConfigurationRetriever<T>, IDocumentRetriever)` and caches the parsed configuration; `GetConfigurationAsync` returns the cached value and re-fetches only when `AutomaticRefreshInterval` has elapsed, `GetBaseConfigurationAsync` is the `BaseConfiguration`-typed override `TokenValidationParameters` calls, and `RequestRefresh` flags the next read to refetch (the forced-refresh path on signature-key-not-found, throttled by `RefreshInterval`). Static defaults `DefaultAutomaticRefreshInterval`/`DefaultRefreshInterval`/`MinimumAutomaticRefreshInterval`/`MinimumRefreshInterval` floor the intervals.
- last-known-good: `UseLastKnownGoodConfiguration` (default `true`, inherited from `BaseConfigurationManager`) makes a transient retrieval failure fall back to `LastKnownGoodConfiguration` for `LastKnownGoodLifetime` rather than failing validation; `LastKnownGoodConfigurationCacheOptions` tunes the LKG entry cache. This is the resilience seam under JWKS rotation — it is the same `ValidateWithLKG` behavior the Tokens validators rely on.
- pinned manager: `StaticConfigurationManager<T>` is the non-refreshing form for a fixed in-memory configuration (`RequestRefresh` is a no-op); it is the test/offline counterpart to `ConfigurationManager<T>`.
- retrieval contract: `IDocumentRetriever.GetDocumentAsync(address, CT)` returns raw document text; `HttpDocumentRetriever` is the `HttpClient`-backed default that enforces `RequireHttps` (default `true`, rejects non-HTTPS addresses with `IDX20108`) and threads `HttpVersion`/`HttpVersionPolicy`; `FileDocumentRetriever` is the local-file form. `IConfigurationRetriever<T>.GetConfigurationAsync(address, IDocumentRetriever, CT)` parses the fetched text into the typed configuration — the OIDC package supplies the `OpenIdConnectConfigurationRetriever` that specializes `T = OpenIdConnectConfiguration`.
- validation contract: `IConfigurationValidator<T>.Validate(T)` returns `ConfigurationValidationResult` (`Succeeded` + `ErrorMessage`); the OIDC package's `OpenIdConnectConfigurationValidator` implements it to assert the discovery document carries enough signing keys before it is trusted, and the validating `ConfigurationManager<T>` ctor overload wires it in.
- event hooks: `IConfigurationEventHandler<T>` (assigned to `ConfigurationManager<T>.ConfigurationEventHandler`) exposes before-retrieve and after-update callbacks for observability and cache-coordination around each refresh.
- namespace split: this assembly owns only `ConfigurationManager<T>`, the retrievers, and the configuration contracts; the abstract `BaseConfigurationManager`/`BaseConfiguration` base types and every refresh/LKG knob (`MetadataAddress`, `AutomaticRefreshInterval`, `RefreshInterval`, `LastKnownGoodConfiguration`, `LastKnownGoodLifetime`, `UseLastKnownGoodConfiguration`) are declared in `Microsoft.IdentityModel.Tokens` and inherited — this is the seam boundary, base contract in Tokens, concrete refresh manager here.

[LOCAL_ADMISSION]:
- Construct one `ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever(resilientHttpClient))` per provider and assign it to `TokenValidationParameters.ConfigurationManager`/`ValidationParameters.ConfigurationManager`; leave `IssuerSigningKeys` unset so the validators pull keys from the refreshed `JsonWebKeySet` rather than a pinned key.
- Set `UseLastKnownGoodConfiguration = true` and a deliberate `LastKnownGoodLifetime` so a transient JWKS-fetch failure falls back to last-known-good instead of failing validation; tune `AutomaticRefreshInterval`/`RefreshInterval` to the provider's key-rotation cadence rather than accepting the defaults blindly.
- Pass the host's resilient/service-discovery `HttpClient` to `HttpDocumentRetriever`; keep `RequireHttps = true` and never fetch the well-known document with a bare client. Use `FileDocumentRetriever`/`StaticConfigurationManager<T>` only for tests or pinned-metadata offline paths.
- Add the validating ctor overload with `IConfigurationValidator<T>` (the OIDC `OpenIdConnectConfigurationValidator`) so a discovery document without sufficient signing keys is rejected before it is trusted; branch on `ConfigurationValidationResult.Succeeded`.
- Treat `RequestRefresh()` as the forced-refresh hook on signature-key-not-found, not a per-validation call; it is throttled by `RefreshInterval`, and the validators invoke it through `RefreshBeforeValidation`. Do not construct `BaseConfigurationManager` directly — `ConfigurationManager<T>` is the concrete owner.

[RAIL_LAW]:
- Package: `Microsoft.IdentityModel.Protocols`
- Owns: the concrete refreshing/last-known-good configuration manager (`ConfigurationManager<T>`), the pinned manager (`StaticConfigurationManager<T>`), document retrieval (`IDocumentRetriever`/`HttpDocumentRetriever`/`FileDocumentRetriever`), and the configuration parse/validate contracts (`IConfigurationRetriever<T>`/`IConfigurationValidator<T>`/`ConfigurationValidationResult`)
- Accept: `ConfigurationManager<OpenIdConnectConfiguration>` as the `TokenValidationParameters.ConfigurationManager` source, fed by the OIDC `IConfigurationRetriever<T>` over an `HttpDocumentRetriever`; LKG fallback through the inherited `BaseConfigurationManager` knobs; sufficiency through an `IConfigurationValidator<T>`
- Reject: direct `BaseConfigurationManager` subclassing, a pinned static `IssuerSigningKey` for a rotating provider, bare-client discovery fetch (`RequireHttps = false` / non-resilience `HttpClient`), `RequestRefresh` per validation, hardcoded refresh intervals that ignore the provider's rotation cadence
