# [RASM_APPHOST_API_IDENTITYMODEL_OIDC]

`Microsoft.IdentityModel.Protocols.OpenIdConnect` supplies the OpenID Connect discovery and protocol-validation leg: `OpenIdConnectConfiguration` is the parsed discovery document (a `BaseConfiguration` carrying every endpoint, the JWKS, and the provider capability flags), `OpenIdConnectConfigurationRetriever` fetches and parses it, and `OpenIdConnectProtocolValidator` validates the OIDC-specific protocol invariants (nonce, `c_hash`, `at_hash`, `state`) the bare JWT validation does not cover. It closes the validation loop: the discovery document's `JsonWebKeySet`, refreshed by the transitive `ConfigurationManager<OpenIdConnectConfiguration>`, becomes the signing-key source assigned to `TokenValidationParameters.ConfigurationManager` and consumed by `Microsoft.IdentityModel.JsonWebTokens`. It also supplies the parameter/scope/response-type/grant-type name constants that name the requests the `OpenIddict.Client` acquisition rail builds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- package: `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- assembly: `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- namespace: `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- asset: runtime library
- license: MIT
- abi: native `lib/net10.0` asset (consumer-bound)
- depends: `Microsoft.IdentityModel.Protocols` (supplies `ConfigurationManager<T>`, `HttpDocumentRetriever`, `IConfigurationRetriever<T>`, `IDocumentRetriever`, `IConfigurationValidator<T>` — cataloged at `api-identitymodel-protocols.md` as the refresh-manager seam), `Microsoft.IdentityModel.Tokens` (supplies `BaseConfiguration`, `JsonWebKeySet`, `BaseConfigurationManager`)
- rail: oidc-protocol

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: discovery, retrieval, and validation — `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- rail: oidc-protocol

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]      | [RAIL]                                        |
| :-----: | :--------------------------------------- | :----------------- | :-------------------------------------------- |
|  [01]   | `OpenIdConnectConfiguration`             | discovery document | `BaseConfiguration`: endpoints + JWKS + caps  |
|  [02]   | `OpenIdConnectConfigurationRetriever`    | retriever          | fetches/parses the discovery document         |
|  [03]   | `OpenIdConnectConfigurationValidator`    | config validator   | `IConfigurationValidator<…>` JWKS sufficiency |
|  [04]   | `OpenIdConnectProtocolValidator`         | protocol validator | nonce / c_hash / at_hash / state checks       |
|  [05]   | `OpenIdConnectProtocolValidationContext` | validation input   | id-token + request state for validation       |
|  [06]   | `OpenIdConnectMessage`                   | protocol message   | authorize/token request+response builder      |
|  [07]   | `OpenIdConnectRequestType`               | enum               | authentication / token / logout request kind  |

[PUBLIC_TYPE_SCOPE]: protocol name constants
- rail: oidc-protocol

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `OpenIdConnectParameterNames`    | constants     | request/response parameter literals       |
|  [02]   | `OpenIdConnectScope`             | constants     | `openid`/`profile`/`offline_access`/…     |
|  [03]   | `OpenIdConnectResponseType`      | constants     | `code`/`id_token`/`token` literals        |
|  [04]   | `OpenIdConnectResponseMode`      | constants     | `query`/`fragment`/`form_post`            |
|  [05]   | `OpenIdConnectGrantTypes`        | constants     | grant-type literals                       |
|  [06]   | `OpenIdConnectPrompt`            | constants     | `none`/`login`/`consent`/`select_account` |
|  [07]   | `OpenIdProviderMetadataNames`    | constants     | discovery-document field names            |
|  [08]   | `OpenIdConnectSessionProperties` | constants     | session/logout property keys              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: discovery retrieval — `OpenIdConnectConfigurationRetriever`
- rail: oidc-protocol

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :--------------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `static Task<OpenIdConnectConfiguration> GetAsync(string address, CT)`             | one-shot fetch | default `HttpDocumentRetriever`    |
|  [02]   | `static Task<OpenIdConnectConfiguration> GetAsync(string address, HttpClient, CT)` | one-shot fetch | host-supplied `HttpClient`         |
|  [03]   | `static Task<…> GetAsync(string address, IDocumentRetriever retriever, CT)`        | one-shot fetch | custom document retriever          |
|  [04]   | `static OpenIdConnectConfiguration Create(string json)`                            | parse          | parse a pre-fetched discovery JSON |

[ENTRYPOINT_SCOPE]: protocol validation — `OpenIdConnectProtocolValidator`
- rail: oidc-protocol

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY]    | [RAIL]                               |
| :-----: | :----------------------------------------------------------------------- | :---------------- | :----------------------------------- |
|  [01]   | `ValidateAuthenticationResponse(OpenIdConnectProtocolValidationContext)` | authn response    | validates nonce/c_hash/at_hash/state |
|  [02]   | `ValidateTokenResponse(OpenIdConnectProtocolValidationContext)`          | token response    | validates token-endpoint response    |
|  [03]   | `ValidateUserInfoResponse(OpenIdConnectProtocolValidationContext)`       | userinfo response | validates userinfo `sub` consistency |
|  [04]   | `GenerateNonce()`                                                        | nonce mint        | timestamped nonce for the request    |
|  [05]   | `GetHashAlgorithm(string algorithm)`                                     | hash select       | resolves the `*_hash` algorithm      |

[ENTRYPOINT_SCOPE]: discovery document projection — `OpenIdConnectConfiguration`
- rail: oidc-protocol

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY]  | [RAIL]                           |
| :-----: | :--------------------------------------------------------------------------------- | :-------------- | :------------------------------- |
|  [01]   | `AuthorizationEndpoint` / `TokenEndpoint` / `UserInfoEndpoint`                     | endpoint read   | core OAuth/OIDC endpoints        |
|  [02]   | `JwksUri` / `JsonWebKeySet`                                                        | signing keys    | JWKS URI + parsed key set        |
|  [03]   | `EndSessionEndpoint` / `RevocationEndpoint` / `IntrospectionEndpoint`              | endpoint read   | session/revocation/introspection |
|  [04]   | `DeviceAuthorizationEndpoint` / `PushedAuthorizationRequestEndpoint`               | endpoint read   | device + PAR endpoints           |
|  [05]   | `RequirePushedAuthorizationRequests` / `TlsClientCertificateBoundAccessTokens`     | capability flag | PAR-required + mTLS-bound tokens |
|  [06]   | `AuthorizationResponseIssParameterSupported` / `BackchannelAuthenticationEndpoint` | capability flag | iss-param + CIBA support         |

## [04]-[IMPLEMENTATION_LAW]

[OIDC_TOPOLOGY]:
- discovery document: `OpenIdConnectConfiguration : BaseConfiguration` is the parsed `.well-known/openid-configuration` carrying `AuthorizationEndpoint`, `TokenEndpoint`, `UserInfoEndpoint`, `EndSessionEndpoint`, `RevocationEndpoint`, `IntrospectionEndpoint`, `DeviceAuthorizationEndpoint`, `PushedAuthorizationRequestEndpoint`, `JwksUri`/`JsonWebKeySet`, and provider capability flags (`RequirePushedAuthorizationRequests`, `TlsClientCertificateBoundAccessTokens`, `AuthorizationResponseIssParameterSupported`, `ClaimsParameterSupported`); `Create(json)` parses a pre-fetched document
- refresh manager: `ConfigurationManager<OpenIdConnectConfiguration>` is a `BaseConfigurationManager` that wraps a metadata address, an `OpenIdConnectConfigurationRetriever`, and an `IDocumentRetriever` or `HttpDocumentRetriever`. It caches configuration through `AutomaticRefreshInterval`, `RefreshInterval`, and last-known-good state.
- validation handoff: assign the manager to `TokenValidationParameters.ConfigurationManager` to supply discovery JWKS data to token validators and the JWT handler. `OpenIdConnectConfigurationRetriever` implements `IConfigurationRetriever<OpenIdConnectConfiguration>`.
- retriever: `OpenIdConnectConfigurationRetriever.GetAsync` is the one-shot fetch (use the `ConfigurationManager` for the cached/refreshing path); the `HttpClient` overload threads the host's resilience/service-discovery pipeline into discovery fetch
- protocol validator: `OpenIdConnectProtocolValidator` checks the OIDC invariants JWT signature/lifetime validation does not — `RequireNonce`/`RequireState`/`RequireStateValidation`/`RequireAuthTime`/`RequireSub`/`RequireAzp`, the `nonce` round-trip, and the `c_hash`/`at_hash` binding of the id-token to the authorization code and access token via `GetHashAlgorithm`; `NonceLifetime` bounds the timestamped nonce from `GenerateNonce`
- validation context: `OpenIdConnectProtocolValidationContext` carries the validated id-token, the authorization-code/access-token, the expected nonce/state, and the userinfo response for the three `Validate*Response` passes
- config validator: `OpenIdConnectConfigurationValidator : IConfigurationValidator<OpenIdConnectConfiguration>` (the `IConfigurationValidator<T>` contract is from `Microsoft.IdentityModel.Protocols`) asserts the discovery document carries enough signing keys before it is trusted as a validation source, returning a `ConfigurationValidationResult`; wire it through the validating `ConfigurationManager<T>` ctor overload
- name constants: `OpenIdConnectParameterNames`, `OpenIdConnectScope`, `OpenIdConnectResponseType`, `OpenIdConnectResponseMode`, `OpenIdConnectGrantTypes`, `OpenIdConnectPrompt`, and `OpenIdProviderMetadataNames` are the canonical literal sets; `OpenIdConnectMessage : AuthenticationProtocolMessage` is the low-level request/response builder for hosts not delegating flow construction to `OpenIddict.Client`

[LOCAL_ADMISSION]:
- Build one `ConfigurationManager<OpenIdConnectConfiguration>` per provider (metadata address + `OpenIdConnectConfigurationRetriever` + `HttpDocumentRetriever`) and assign it to `TokenValidationParameters.ConfigurationManager`/`ValidationParameters.ConfigurationManager`; the validators pull `IssuerSigningKeys` from its refreshed `JsonWebKeySet` rather than a pinned key.
- Thread the discovery `HttpClient` through the host's resilience handler and `Microsoft.Extensions.ServiceDiscovery`; do not fetch the well-known document with a bare client.
- Run `OpenIdConnectProtocolValidator.ValidateAuthenticationResponse` after JWT validation for interactive flows to enforce the `nonce`/`c_hash`/`at_hash`/`state` binding; `RequireNonce`/`RequireStateValidation` stay on, and the nonce is minted by `GenerateNonce` and round-tripped through the request `state`.
- Read endpoints and capability flags off `OpenIdConnectConfiguration` (`PushedAuthorizationRequestEndpoint`, `RequirePushedAuthorizationRequests`, `TlsClientCertificateBoundAccessTokens`) to drive the PAR/DPoP/mTLS negotiation that the `OpenIddict.Client` registration capability sets consume — never hardcode an issuer's endpoints.
- Name request parameters and scopes through the constant types, not string literals, when building a raw `OpenIdConnectMessage`; prefer the `OpenIddict.Client` flow verbs over hand-built messages for the standard grants.

[RAIL_LAW]:
- Package: `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- Owns: OIDC discovery-document modeling/retrieval, refreshing JWKS configuration, OIDC protocol-invariant validation (nonce/c_hash/at_hash/state), and the OIDC protocol name constants
- Accept: `ConfigurationManager<OpenIdConnectConfiguration>` as the `TokenValidationParameters.ConfigurationManager` source; `OpenIdConnectProtocolValidator` over an `OpenIdConnectProtocolValidationContext`; discovery through `OpenIdConnectConfigurationRetriever`
- Reject: hardcoded issuer endpoints/keys, hand-rolled `.well-known` parsing, skipping nonce/hash binding for interactive flows, string-literal OIDC parameter/scope names, bare-client discovery fetch outside the host resilience pipeline
