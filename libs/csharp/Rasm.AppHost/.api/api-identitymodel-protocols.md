# [RASM_APPHOST_API_IDENTITYMODEL_PROTOCOLS]

`Microsoft.IdentityModel.Protocols` owns the protocol-agnostic configuration-refresh substrate: `ConfigurationManager<T>` caches a discovery document, refreshes it on interval or demand, and falls back to last-known-good. `BaseConfigurationManager` and the OIDC vocabulary are sibling-owned; this assembly holds the concrete manager and retrievers. `ConfigurationManager<OpenIdConnectConfiguration>` assigned to `TokenValidationParameters.ConfigurationManager` feeds the JWT validation rail its refreshed `JsonWebKeySet` for rotating signing keys.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IdentityModel.Protocols`
- package: `Microsoft.IdentityModel.Protocols` (MIT)
- assembly: `Microsoft.IdentityModel.Protocols`
- namespace: `Microsoft.IdentityModel.Protocols`, `Microsoft.IdentityModel.Protocols.Configuration`
- asset: runtime library
- abi: native `lib/net10.0` asset (consumer-bound); also `net9.0`/`net8.0`/`net6.0`/`netstandard2.0`/`net462`/`net472`
- admission: transitive via `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- depends: `Microsoft.IdentityModel.Tokens` (supplies `BaseConfigurationManager`, `BaseConfiguration`)
- rail: protocols-config

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: refresh manager and configuration contracts

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]      | [CAPABILITY]                                            |
| :-----: | :--------------------------------------- | :----------------- | :------------------------------------------------------ |
|  [01]   | `ConfigurationManager<T>`                | refresh manager    | `BaseConfigurationManager` cache + interval/LKG refresh |
|  [02]   | `StaticConfigurationManager<T>`          | pinned manager     | non-refreshing fixed configuration                      |
|  [03]   | `IConfigurationManager<T>`               | manager contract   | `GetConfigurationAsync(CT)` abstraction                 |
|  [04]   | `IConfigurationRetriever<T>`             | parse contract     | document text -> typed configuration                    |
|  [05]   | `IConfigurationValidator<T>`             | validator contract | configuration-sufficiency check                         |
|  [06]   | `ConfigurationValidationResult`          | validation result  | `Succeeded` + `ErrorMessage`                            |
|  [07]   | `IConfigurationEventHandler<T>`          | refresh hooks      | before-retrieve / after-update events                   |
|  [08]   | `LastKnownGoodConfigurationCacheOptions` | LKG cache options  | LKG entry cache size/expiration                         |

[PUBLIC_TYPE_SCOPE]: document retrieval

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [CAPABILITY]                           |
| :-----: | :---------------------- | :------------- | :------------------------------------- |
|  [01]   | `IDocumentRetriever`    | fetch contract | address -> raw document text           |
|  [02]   | `HttpDocumentRetriever` | HTTP retriever | `HttpClient` metadata fetch over HTTPS |
|  [03]   | `FileDocumentRetriever` | file retriever | local-file metadata fetch              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: manager construction and refresh — `ConfigurationManager<T>`

Every constructor takes `(string metadataAddress, IConfigurationRetriever<T>, …)`; the trailing argument selects the retrieval or validation overload.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `ctor(…)`                                                  | ctor     | default `HttpDocumentRetriever` |
|  [02]   | `ctor(…, HttpClient)`                                      | ctor     | host HTTP client                |
|  [03]   | `ctor(…, IDocumentRetriever)`                              | ctor     | explicit retriever              |
|  [04]   | `ctor(…, IDocumentRetriever, IConfigurationValidator<T>)`  | ctor     | JWKS-sufficiency validated      |
|  [05]   | `ctor(…, LastKnownGoodConfigurationCacheOptions)`          | ctor     | LKG cache policy                |
|  [06]   | `GetConfigurationAsync(CT) -> Task<T>`                     | instance | cached read, refreshes stale    |
|  [07]   | `GetBaseConfigurationAsync(CT) -> Task<BaseConfiguration>` | instance | base-typed validation source    |
|  [08]   | `RequestRefresh()`                                         | instance | flag next read to refetch       |
|  [09]   | `ConfigurationEventHandler`                                | property | before/after refresh hooks      |

[ENTRYPOINT_SCOPE]: inherited refresh + LKG knobs — `BaseConfigurationManager` (`Microsoft.IdentityModel.Tokens`)

| [INDEX] | [SURFACE]                       | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------ | :------- | :----------------------------------------------- |
|  [01]   | `MetadataAddress`               | property | the `.well-known` document URL                   |
|  [02]   | `AutomaticRefreshInterval`      | property | background re-fetch interval                     |
|  [03]   | `RefreshInterval`               | property | min interval between `RequestRefresh`            |
|  [04]   | `UseLastKnownGoodConfiguration` | property | default `true`; survive transient fetch failures |
|  [05]   | `LastKnownGoodConfiguration`    | property | the cached last-good configuration               |
|  [06]   | `LastKnownGoodLifetime`         | property | LKG entry validity window                        |

[ENTRYPOINT_SCOPE]: retrieval and validation contracts

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `IConfigurationRetriever<T>.GetConfigurationAsync(string, IDocumentRetriever, CT)` | instance | parse text (OIDC specializes `T`)  |
|  [02]   | `IDocumentRetriever.GetDocumentAsync(string, CT)`                                  | instance | fetch raw document                 |
|  [03]   | `HttpDocumentRetriever()`                                                          | ctor     | default client                     |
|  [04]   | `HttpDocumentRetriever(HttpClient)`                                                | ctor     | host client                        |
|  [05]   | `HttpDocumentRetriever.RequireHttps`                                               | property | HTTPS enforcement (default `true`) |
|  [06]   | `HttpDocumentRetriever.HttpVersion`                                                | property | request `Version`                  |
|  [07]   | `HttpDocumentRetriever.HttpVersionPolicy`                                          | property | per-request version policy         |
|  [08]   | `IConfigurationValidator<T>.Validate(T)`                                           | instance | signing-key sufficiency            |
|  [09]   | `StaticConfigurationManager<T>(T)`                                                 | ctor     | pinned fixed config                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ConfigurationManager<T> : BaseConfigurationManager, IConfigurationManager<T>` serves the cached configuration through `GetConfigurationAsync`, re-fetches past `AutomaticRefreshInterval`, and exposes `GetBaseConfigurationAsync` as the `BaseConfiguration`-typed validation source; `RequestRefresh` flags a refetch throttled by `RefreshInterval`.
- `UseLastKnownGoodConfiguration` (default `true`) falls a transient retrieval failure back to `LastKnownGoodConfiguration` for `LastKnownGoodLifetime` rather than failing validation — the resilience seam under JWKS rotation, the LKG behavior the Tokens validators drive.
- `IConfigurationRetriever<T>.GetConfigurationAsync` parses the fetched text `IDocumentRetriever.GetDocumentAsync` returns, `HttpDocumentRetriever` the `HttpClient`-backed default rejecting non-HTTPS under `RequireHttps`; `IConfigurationValidator<T>.Validate` returns `ConfigurationValidationResult`, gating a document on signing-key sufficiency before trust.
- `StaticConfigurationManager<T>` is the non-refreshing pinned form (`RequestRefresh` a no-op) for fixed in-memory or offline metadata; `FileDocumentRetriever` fetches local-file metadata.
- `Microsoft.IdentityModel.Tokens` declares the `BaseConfigurationManager` base contract and its inherited refresh/LKG knobs; this assembly owns the concrete manager and retrievers.

[STACKING]:
- `Microsoft.IdentityModel.Tokens`(`api-identitymodel-tokens.md`): supplies the `BaseConfigurationManager` base class, the `BaseConfiguration` model, and the six inherited refresh/LKG knobs; a constructed manager is assigned to `TokenValidationParameters.ConfigurationManager`/`ValidationParameters.ConfigurationManager`.
- `Microsoft.IdentityModel.Protocols.OpenIdConnect`(`api-identitymodel-oidc.md`): supplies the `OpenIdConnectConfigurationRetriever` and `OpenIdConnectConfigurationValidator` that specialize `T = OpenIdConnectConfiguration`; the refreshed `JsonWebKeySet` off that configuration becomes the rotating signing-key source `Microsoft.IdentityModel.JsonWebTokens` consumes through `TokenValidationParameters`.
- within-lib: the host constructs one `ConfigurationManager<OpenIdConnectConfiguration>` per provider over a resilient `HttpDocumentRetriever`, wires the validating ctor overload, and threads LKG state and refresh cadence to the provider's key-rotation rhythm.

[LOCAL_ADMISSION]:
- Construct one `ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever(resilientHttpClient))` per provider and assign it to `TokenValidationParameters.ConfigurationManager`/`ValidationParameters.ConfigurationManager`; leave `IssuerSigningKeys` unset so validators pull keys from the refreshed `JsonWebKeySet`.
- Set `UseLastKnownGoodConfiguration = true` with a deliberate `LastKnownGoodLifetime`, and tune `AutomaticRefreshInterval`/`RefreshInterval` to the provider's key-rotation cadence.
- Pass the host's resilient/service-discovery `HttpClient` to `HttpDocumentRetriever` and keep `RequireHttps = true`; reserve `FileDocumentRetriever`/`StaticConfigurationManager<T>` for tests and pinned-metadata offline paths.
- Add the validating ctor overload with the OIDC `OpenIdConnectConfigurationValidator` so a discovery document without sufficient signing keys is rejected before trust; branch on `ConfigurationValidationResult.Succeeded`.
- Treat `RequestRefresh()` as the forced-refresh hook on signature-key-not-found, throttled by `RefreshInterval` and invoked by the validators through `RefreshBeforeValidation`; construct `ConfigurationManager<T>`, never `BaseConfigurationManager`.

[RAIL_LAW]:
- Package: `Microsoft.IdentityModel.Protocols`
- Owns: the concrete refreshing/LKG configuration manager (`ConfigurationManager<T>`), the pinned `StaticConfigurationManager<T>`, document retrieval (`IDocumentRetriever`/`HttpDocumentRetriever`/`FileDocumentRetriever`), and the configuration parse/validate contracts (`IConfigurationRetriever<T>`/`IConfigurationValidator<T>`/`ConfigurationValidationResult`)
- Accept: `ConfigurationManager<OpenIdConnectConfiguration>` as the `TokenValidationParameters.ConfigurationManager` source, fed by the OIDC `IConfigurationRetriever<T>` over an `HttpDocumentRetriever`; LKG fallback through the inherited `BaseConfigurationManager` knobs; sufficiency through an `IConfigurationValidator<T>`
- Reject: direct `BaseConfigurationManager` subclassing, a pinned static `IssuerSigningKey` for a rotating provider, bare-client discovery fetch, `RequestRefresh` per validation, hardcoded refresh intervals ignoring the provider's rotation cadence
