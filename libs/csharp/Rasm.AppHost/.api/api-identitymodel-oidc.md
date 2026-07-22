# [RASM_APPHOST_API_IDENTITYMODEL_OIDC]

`Microsoft.IdentityModel.Protocols.OpenIdConnect` owns the OpenID Connect discovery-and-protocol-validation leg: `OpenIdConnectConfiguration` parses the `.well-known/openid-configuration` document, and `OpenIdConnectProtocolValidator` enforces the OIDC invariants bare JWT validation skips — the `nonce` round-trip and the `c_hash`/`at_hash`/`state` binding. Its refreshed discovery `JsonWebKeySet` is the rotating signing-key source the token-validation leg consumes, and its OIDC name constants name the requests the acquisition client builds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- package: `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- assembly: `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- namespace: `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- asset: runtime library
- license: MIT
- abi: native `lib/net10.0` asset (consumer-bound)
- depends: `Microsoft.IdentityModel.Protocols`, `Microsoft.IdentityModel.Tokens`
- rail: oidc-protocol

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: discovery, retrieval, and validation

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]      | [CAPABILITY]                                  |
| :-----: | :--------------------------------------- | :----------------- | :-------------------------------------------- |
|  [01]   | `OpenIdConnectConfiguration`             | discovery document | `BaseConfiguration`: endpoints + JWKS + caps  |
|  [02]   | `OpenIdConnectConfigurationRetriever`    | retriever          | fetches/parses the discovery document         |
|  [03]   | `OpenIdConnectConfigurationValidator`    | config validator   | `IConfigurationValidator<…>` JWKS sufficiency |
|  [04]   | `OpenIdConnectProtocolValidator`         | protocol validator | nonce / c_hash / at_hash / state checks       |
|  [05]   | `OpenIdConnectProtocolValidationContext` | validation input   | id-token + request state for validation       |
|  [06]   | `OpenIdConnectMessage`                   | protocol message   | authorize/token request+response builder      |
|  [07]   | `OpenIdConnectRequestType`               | enum               | authentication / token / logout request kind  |

[PUBLIC_TYPE_SCOPE]: protocol name constants

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                              |
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

| [INDEX] | [SURFACE]                                   | [SHAPE] | [CAPABILITY]                       |
| :-----: | :------------------------------------------ | :------ | :--------------------------------- |
|  [01]   | `.GetAsync(string, CT)`                     | static  | default `HttpDocumentRetriever`    |
|  [02]   | `.GetAsync(string, HttpClient, CT)`         | static  | host-supplied `HttpClient`         |
|  [03]   | `.GetAsync(string, IDocumentRetriever, CT)` | static  | custom document retriever          |
|  [04]   | `OpenIdConnectConfiguration.Create(string)` | factory | parse a pre-fetched discovery JSON |

[ENTRYPOINT_SCOPE]: protocol validation — `OpenIdConnectProtocolValidator`

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :----------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `.ValidateAuthenticationResponse(context)` | instance | validates nonce/c_hash/at_hash/state |
|  [02]   | `.ValidateTokenResponse(context)`          | instance | validates token-endpoint response    |
|  [03]   | `.ValidateUserInfoResponse(context)`       | instance | validates userinfo `sub` consistency |
|  [04]   | `.GenerateNonce()`                         | instance | timestamped nonce for the request    |
|  [05]   | `.GetHashAlgorithm(string)`                | instance | resolves the `*_hash` algorithm      |

[ENTRYPOINT_SCOPE]: discovery-document projection — `OpenIdConnectConfiguration`

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `AuthorizationEndpoint` / `TokenEndpoint` / `UserInfoEndpoint`                     | property | core OAuth/OIDC endpoints        |
|  [02]   | `JwksUri` / `JsonWebKeySet`                                                        | property | JWKS URI + parsed key set        |
|  [03]   | `EndSessionEndpoint` / `RevocationEndpoint` / `IntrospectionEndpoint`              | property | session/revocation/introspection |
|  [04]   | `DeviceAuthorizationEndpoint` / `PushedAuthorizationRequestEndpoint`               | property | device + PAR endpoints           |
|  [05]   | `RequirePushedAuthorizationRequests` / `TlsClientCertificateBoundAccessTokens`     | property | PAR-required + mTLS-bound tokens |
|  [06]   | `AuthorizationResponseIssParameterSupported` / `BackchannelAuthenticationEndpoint` | property | iss-param + CIBA support         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `OpenIdConnectConfiguration : BaseConfiguration` is the parsed `.well-known/openid-configuration`; `Create(json)` parses a pre-fetched document, and its endpoint/JWKS/capability-flag properties drive every provider negotiation rather than a hardcoded issuer.
- `OpenIdConnectProtocolValidator` is the layer over JWT signature/lifetime validation: `RequireNonce`/`RequireState`/`RequireStateValidation`/`RequireSub`/`RequireAzp` gate the `nonce` round-trip and the `c_hash`/`at_hash` binding of the id-token to the authorization code and access token, `GetHashAlgorithm` resolves the `*_hash` digest, and `NonceLifetime` bounds the `GenerateNonce` timestamp.
- `OpenIdConnectProtocolValidationContext` carries the id-token, the code/access-token, the expected nonce/state, and the userinfo response through the three `Validate*Response` passes.
- `OpenIdConnect*` name-constant sets are the canonical literal source; `OpenIdConnectMessage : AuthenticationProtocolMessage` is the raw request/response builder for hosts not delegating flow construction.

[STACKING]:
- `Microsoft.IdentityModel.Protocols`(`.api/api-identitymodel-protocols.md`): `OpenIdConnectConfigurationRetriever` and `OpenIdConnectConfigurationValidator` specialize `IConfigurationRetriever<T>` and `IConfigurationValidator<T>` at `T = OpenIdConnectConfiguration`, minting the `ConfigurationManager<OpenIdConnectConfiguration>` that wraps a metadata address and `HttpDocumentRetriever` and caches through the inherited refresh/last-known-good knobs.
- `Microsoft.IdentityModel.Tokens`(`.api/api-identitymodel-tokens.md`): that manager assigns to `TokenValidationParameters.ConfigurationManager`/`ValidationParameters.ConfigurationManager`, so the refreshed `JsonWebKeySet` becomes the rotating `IssuerSigningKeys` source; `OpenIdConnectConfiguration : BaseConfiguration`.
- `Microsoft.IdentityModel.JsonWebTokens`(`.api/api-identitymodel-jwt.md`): `JsonWebTokenHandler` validation reads the discovery JWKS through that same `ConfigurationManager` slot.
- `OpenIddict.Client`(`.api/api-openiddict-client.md`): the `OpenIdConnect*` name-constant sets name the parameters, scopes, and grant-types the acquisition rail builds; a hand-built `OpenIdConnectMessage` covers only hosts bypassing the client flow verbs.
- AppHost OIDC rail: one `ConfigurationManager<OpenIdConnectConfiguration>` per provider drives discovery, and `ValidateAuthenticationResponse` runs after JWT validation to bind nonce/c_hash/at_hash/state.

[LOCAL_ADMISSION]:
- Build one `ConfigurationManager<OpenIdConnectConfiguration>` per provider and assign it to `TokenValidationParameters.ConfigurationManager`; leave `IssuerSigningKeys` unset so validators pull from the refreshed `JsonWebKeySet`.
- Thread the discovery `HttpClient` through the host resilience handler and `Microsoft.Extensions.ServiceDiscovery`; fetch the well-known document with a resilient client.
- Run `ValidateAuthenticationResponse` after JWT validation for interactive flows with `RequireNonce`/`RequireStateValidation` on, minting the nonce through `GenerateNonce` and round-tripping it in the request `state`.
- Read endpoints and capability flags off `OpenIdConnectConfiguration` to drive the PAR/DPoP/mTLS negotiation the `OpenIddict.Client` registration sets consume; name parameters and scopes through the constant types, and build the standard grants through the `OpenIddict.Client` flow verbs rather than a hand-built `OpenIdConnectMessage`.

[RAIL_LAW]:
- Package: `Microsoft.IdentityModel.Protocols.OpenIdConnect`
- Owns: OIDC discovery-document modeling and retrieval, OIDC protocol-invariant validation (nonce/c_hash/at_hash/state), and the OIDC protocol name constants
- Accept: `ConfigurationManager<OpenIdConnectConfiguration>` as the `TokenValidationParameters.ConfigurationManager` source; `OpenIdConnectProtocolValidator` over an `OpenIdConnectProtocolValidationContext`; discovery through `OpenIdConnectConfigurationRetriever`
- Reject: hardcoded issuer endpoints/keys, hand-rolled `.well-known` parsing, skipped nonce/hash binding for interactive flows, string-literal OIDC parameter/scope names, bare-client discovery fetch outside the host resilience pipeline
