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

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]      | [RAIL]                                                  |
| :-----: | :--------------------------------------- | :----------------- | :------------------------------------------------------ |
|  [01]   | `ConfigurationManager<T>`                | refresh manager    | `BaseConfigurationManager` cache + interval/LKG refresh |
|  [02]   | `StaticConfigurationManager<T>`          | pinned manager     | non-refreshing fixed-configuration manager              |
|  [03]   | `IConfigurationManager<T>`               | manager contract   | `GetConfigurationAsync(CT)` abstraction                 |
|  [04]   | `IConfigurationRetriever<T>`             | parse contract     | document text -> typed configuration                    |
|  [05]   | `IConfigurationValidator<T>`             | validator contract | configuration-sufficiency check                         |
|  [06]   | `ConfigurationValidationResult`          | validation result  | `Succeeded` + `ErrorMessage`                            |
|  [07]   | `IConfigurationEventHandler<T>`          | refresh hooks      | before-retrieve / after-update retrieval events         |
|  [08]   | `LastKnownGoodConfigurationCacheOptions` | LKG cache options  | size/expiration tuning for the LKG entry cache          |

[PUBLIC_TYPE_SCOPE]: document retrieval — `Microsoft.IdentityModel.Protocols`

- rail: protocols-config

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                                        |
| :-----: | :---------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `IDocumentRetriever`    | fetch contract | address -> raw document text                  |
|  [02]   | `HttpDocumentRetriever` | HTTP retriever | `HttpClient`-backed metadata fetch over HTTPS |
|  [03]   | `FileDocumentRetriever` | file retriever | local-file metadata fetch (test/offline)      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: manager construction and refresh — `ConfigurationManager<T>`

- rail: protocols-config

The discovery-fed seam constructs `ConfigurationManager<OpenIdConnectConfiguration>` from a metadata address, the OIDC `IConfigurationRetriever<T>`, and an `IDocumentRetriever`; the default form binds `HttpDocumentRetriever`. The manager inherits the refresh/LKG knobs from `BaseConfigurationManager` and is assigned to `TokenValidationParameters.ConfigurationManager`.

Every `ConfigurationManager<T>` constructor takes the metadata address before the listed arguments.

| [INDEX] | [ARGUMENTS]                                                                  | [KIND]    | [CAPABILITY]       |
| :-----: | :--------------------------------------------------------------------------- | :-------- | :----------------- |
|  [01]   | `IConfigurationRetriever<T>`                                                 | default   | HTTP retrieval     |
|  [02]   | `IConfigurationRetriever<T>, HttpClient`                                     | client    | host HTTP client   |
|  [03]   | `IConfigurationRetriever<T>, IDocumentRetriever`                             | retriever | explicit retrieval |
|  [04]   | `IConfigurationRetriever<T>, IDocumentRetriever, IConfigurationValidator<T>` | validated | JWKS sufficiency   |
|  [05]   | `…, LastKnownGoodConfigurationCacheOptions`                                  | LKG-tuned | LKG cache policy   |

The manager entrypoints own cached reads, base-typed consumption, forced refresh, and refresh hooks.

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY]  | [CAPABILITY]      |
| :-----: | :------------------------------------------------------------------------------ | :-------------- | :---------------- |
|  [01]   | `Task<T> GetConfigurationAsync([CancellationToken])`                            | cached read     | refreshes stale   |
|  [02]   | `override Task<BaseConfiguration> GetBaseConfigurationAsync(CancellationToken)` | base-typed read | validation source |
|  [03]   | `override void RequestRefresh()`                                                | forced refresh  | refetch flag      |
|  [04]   | `ConfigurationEventHandler { get; set; }`                                       | refresh hooks   | before and after  |

[ENTRYPOINT_SCOPE]: inherited refresh + LKG knobs — `BaseConfigurationManager` (from `Microsoft.IdentityModel.Tokens`)

- rail: protocols-config

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]       | [RAIL]                                           |
| :-----: | :----------------------------------------------------------- | :------------------- | :----------------------------------------------- |
|  [01]   | `string MetadataAddress { get; set; }`                       | source address       | the `.well-known` document URL                   |
|  [02]   | `TimeSpan AutomaticRefreshInterval { get; set; }`            | refresh cadence      | background re-fetch interval                     |
|  [03]   | `TimeSpan RefreshInterval { get; set; }`                     | manual-refresh floor | min interval between `RequestRefresh`            |
|  [04]   | `bool UseLastKnownGoodConfiguration { get; set; }`           | LKG toggle           | default `true`; survive transient fetch failures |
|  [05]   | `BaseConfiguration LastKnownGoodConfiguration { get; set; }` | LKG read             | the cached last-good configuration               |
|  [06]   | `TimeSpan LastKnownGoodLifetime { get; set; }`               | LKG lifetime         | how long an LKG entry stays valid                |

[ENTRYPOINT_SCOPE]: retrieval and validation contracts

- rail: protocols-config

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY]  | [CAPABILITY]       |
| :-----: | :----------------------------------------------------------------------- | :-------------- | :----------------- |
|  [01]   | `IConfigurationRetriever<T>.GetConfigurationAsync`                       | parse           | OIDC specializes T |
|  [02]   | `IDocumentRetriever.GetDocumentAsync(string address, CancellationToken)` | fetch           | raw document       |
|  [03]   | `new HttpDocumentRetriever()`                                            | retriever ctor  | default client     |
|  [04]   | `new HttpDocumentRetriever(HttpClient)`                                  | retriever ctor  | host client        |
|  [05]   | `HttpDocumentRetriever.RequireHttps { get; set; }`                       | transport guard | HTTPS enforcement  |
|  [06]   | `HttpDocumentRetriever.HttpVersion`                                      | protocol policy | request version    |
|  [07]   | `HttpDocumentRetriever.HttpVersionPolicy`                                | protocol policy | version policy     |
|  [08]   | `IConfigurationValidator<T>.Validate(T)`                                 | sufficiency     | validation result  |
|  [09]   | `new StaticConfigurationManager<T>(T configuration)`                     | pinned manager  | fixed config       |

[CONFIGURATION_RETRIEVER_SIGNATURE]: `(string address, IDocumentRetriever, CT)`.
[TRANSPORT_DEFAULT]: `RequireHttps` defaults to `true` and rejects non-HTTPS addresses.
[PROTOCOL_POLICY]: `HttpVersion` carries `Version`; `HttpVersionPolicy` carries `HttpVersionPolicy?` per request.
[VALIDATION_RESULT]: `ConfigurationValidationResult` carries `Succeeded` and `ErrorMessage`.
[PINNED_REFRESH]: `StaticConfigurationManager<T>.RequestRefresh` is a no-op.

## [04]-[IMPLEMENTATION_LAW]

[PROTOCOLS_TOPOLOGY]:

[REFRESH_MANAGER]:

- Shape: `ConfigurationManager<T> : BaseConfigurationManager, IConfigurationManager<T> where T : class` wraps `(MetadataAddress, IConfigurationRetriever<T>, IDocumentRetriever)` and caches the parsed configuration.
- Read: `GetConfigurationAsync` returns the cached value and re-fetches after `AutomaticRefreshInterval`; `GetBaseConfigurationAsync` is the `BaseConfiguration`-typed validation source.
- Refresh: `RequestRefresh` flags the next read to refetch after signature-key-not-found and is throttled by `RefreshInterval`.
- Defaults: `DefaultAutomaticRefreshInterval`, `DefaultRefreshInterval`, `MinimumAutomaticRefreshInterval`, and `MinimumRefreshInterval` floor the intervals.

- last-known-good: `UseLastKnownGoodConfiguration` (default `true`, inherited from `BaseConfigurationManager`) makes a transient retrieval failure fall back to `LastKnownGoodConfiguration` for `LastKnownGoodLifetime` rather than failing validation; `LastKnownGoodConfigurationCacheOptions` tunes the LKG entry cache. This is the resilience seam under JWKS rotation — it is the same `ValidateWithLKG` behavior the Tokens validators rely on.
- pinned manager: `StaticConfigurationManager<T>` is the non-refreshing form for a fixed in-memory configuration (`RequestRefresh` is a no-op); it is the test/offline counterpart to `ConfigurationManager<T>`.

[RETRIEVAL_CONTRACT]:

- Fetch: `IDocumentRetriever.GetDocumentAsync(address, CT)` returns raw document text; `HttpDocumentRetriever` is the `HttpClient`-backed default.
- Transport: `RequireHttps` defaults to `true` and rejects non-HTTPS addresses with `IDX20108`; `HttpVersion` and `HttpVersionPolicy` govern each request.
- File: `FileDocumentRetriever` owns local-file retrieval.
- Parse: `IConfigurationRetriever<T>.GetConfigurationAsync(address, IDocumentRetriever, CT)` parses the text; the OIDC `OpenIdConnectConfigurationRetriever` specializes `T = OpenIdConnectConfiguration`.

- validation contract: `IConfigurationValidator<T>.Validate(T)` returns `ConfigurationValidationResult` (`Succeeded` + `ErrorMessage`); the OIDC package's `OpenIdConnectConfigurationValidator` implements it to assert the discovery document carries enough signing keys before it is trusted, and the validating `ConfigurationManager<T>` ctor overload wires it in.
- event hooks: `IConfigurationEventHandler<T>` (assigned to `ConfigurationManager<T>.ConfigurationEventHandler`) exposes before-retrieve and after-update callbacks for observability and cache-coordination around each refresh.

[NAMESPACE_SPLIT]:

- Protocols: this assembly owns `ConfigurationManager<T>`, the retrievers, and the configuration contracts.
- Tokens: `Microsoft.IdentityModel.Tokens` declares `BaseConfigurationManager`, `BaseConfiguration`, and the inherited refresh/LKG knobs: `MetadataAddress`, `AutomaticRefreshInterval`, `RefreshInterval`, `LastKnownGoodConfiguration`, `LastKnownGoodLifetime`, and `UseLastKnownGoodConfiguration`.
- Boundary: Tokens owns the base contract; Protocols owns the concrete refresh manager.

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
