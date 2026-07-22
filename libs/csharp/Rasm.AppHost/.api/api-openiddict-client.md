# [RASM_APPHOST_API_OPENIDDICT_CLIENT]

`OpenIddict.Client` owns the standalone OAuth 2.0 / OpenID Connect relying-party client: `OpenIddictClientService` folds every token-acquisition flow, RP-initiated interactive sign-out, introspection, revocation, registration resolution, and server-metadata discovery through one polymorphic request-record entry. It is the AppHost agent-identity rail behind delegated and machine-to-machine credential flows; the access and identity tokens it acquires cross to `Microsoft.IdentityModel.JsonWebTokens` for validation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenIddict.Client`
- package: `OpenIddict.Client`
- license: `Apache-2.0`
- assembly: `OpenIddict.Client`
- namespace: `OpenIddict.Client`, `Microsoft.Extensions.DependencyInjection`
- asset: pure-managed runtime library; no native asset, no RID burden
- depends: `OpenIddict.Abstractions` (wire primitives `OpenIddictRequest` / `OpenIddictResponse` / `OpenIddictParameter` / `OpenIddictConstants`), `Microsoft.IdentityModel.JsonWebTokens`, `Microsoft.IdentityModel.Protocols`
- rail: oidc-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client service and configuration — `OpenIddict.Client`

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [CAPABILITY]                        |
| :-----: | :----------------------------- | :----------------- | :---------------------------------- |
|  [01]   | `OpenIddictClientService`      | service class      | flow-specific token acquisition     |
|  [02]   | `OpenIddictClientRegistration` | configuration root | per-provider client identity        |
|  [03]   | `OpenIddictClientOptions`      | options class      | global client policy and registry   |
|  [04]   | `OpenIddictClientModels`       | static record host | request/result record family        |
|  [05]   | `OpenIddictClientEndpointType` | enum               | redirection endpoint classification |

[PUBLIC_TYPE_SCOPE]: request/result records — `OpenIddictClientModels`

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `InteractiveChallengeRequest`            | request record | authorization-code+PKCE challenge     |
|  [02]   | `InteractiveChallengeResult`             | result record  | nonce-carrying challenge handle       |
|  [03]   | `InteractiveAuthenticationRequest`       | request record | authorization-code callback exchange  |
|  [04]   | `InteractiveAuthenticationResult`        | result record  | front/backchannel token bundle        |
|  [05]   | `InteractiveSignOutRequest`              | request record | end-session challenge                 |
|  [06]   | `InteractiveSignOutResult`               | result record  | nonce-carrying sign-out handle        |
|  [07]   | `ClientCredentialsAuthenticationRequest` | request record | machine-to-machine token request      |
|  [08]   | `ClientCredentialsAuthenticationResult`  | result record  | client-credentials token bundle       |
|  [09]   | `DeviceChallengeRequest`                 | request record | device-authorization start            |
|  [10]   | `DeviceChallengeResult`                  | result record  | user/device code and verification URI |
|  [11]   | `DeviceAuthenticationRequest`            | request record | device-code polling request           |
|  [12]   | `DeviceAuthenticationResult`             | result record  | device-flow token bundle              |
|  [13]   | `RefreshTokenAuthenticationRequest`      | request record | refresh-token grant                   |
|  [14]   | `RefreshTokenAuthenticationResult`       | result record  | refreshed token bundle                |
|  [15]   | `TokenExchangeAuthenticationRequest`     | request record | RFC 8693 token exchange               |
|  [16]   | `TokenExchangeAuthenticationResult`      | result record  | exchanged token bundle                |
|  [17]   | `CustomGrantAuthenticationRequest`       | request record | arbitrary grant-type request          |
|  [18]   | `CustomGrantAuthenticationResult`        | result record  | custom-grant token bundle             |
|  [19]   | `PasswordAuthenticationRequest`          | request record | resource-owner password grant         |
|  [20]   | `PasswordAuthenticationResult`           | result record  | password-grant token bundle           |
|  [21]   | `IntrospectionRequest`                   | request record | RFC 7662 token introspection          |
|  [22]   | `IntrospectionResult`                    | result record  | introspection principal and response  |
|  [23]   | `RevocationRequest`                      | request record | RFC 7009 token revocation             |
|  [24]   | `RevocationResult`                       | result record  | revocation response carrier           |

[PUBLIC_TYPE_SCOPE]: DI registration and builder — `Microsoft.Extensions.DependencyInjection`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :--------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `OpenIddictClientExtensions` | static class  | `AddClient` DI entry on the builder       |
|  [02]   | `OpenIddictClientBuilder`    | builder class | flow, credential, and registration wiring |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: token-acquisition verbs — instance methods on `OpenIddictClientService`

A challenge verb emits a nonce-bearing handle the matching authentication verb redeems; client-credentials, refresh, exchange, custom, and password flows are single-call. Each request record targets one registration by `Issuer`, `ProviderName`, or `RegistrationId`, and `RegistrationId` is required to disambiguate registrations sharing an issuer or provider.

| [INDEX] | [SURFACE]                                                                        | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `ChallengeInteractivelyAsync(InteractiveChallengeRequest)`                       | nonce-bearing authorization handle    |
|  [02]   | `AuthenticateInteractivelyAsync(InteractiveAuthenticationRequest)`               | redeem callback for token bundle      |
|  [03]   | `AuthenticateWithClientCredentialsAsync(ClientCredentialsAuthenticationRequest)` | machine-to-machine token bundle       |
|  [04]   | `ChallengeUsingDeviceAsync(DeviceChallengeRequest)`                              | user/device code and verification URI |
|  [05]   | `AuthenticateWithDeviceAsync(DeviceAuthenticationRequest)`                       | poll device code for token bundle     |
|  [06]   | `AuthenticateWithRefreshTokenAsync(RefreshTokenAuthenticationRequest)`           | refreshed token bundle                |
|  [07]   | `AuthenticateWithTokenExchangeAsync(TokenExchangeAuthenticationRequest)`         | RFC 8693 exchanged tokens             |
|  [08]   | `AuthenticateWithCustomGrantAsync(CustomGrantAuthenticationRequest)`             | arbitrary grant-type tokens           |
|  [09]   | `AuthenticateWithPasswordAsync(PasswordAuthenticationRequest)`                   | resource-owner password tokens        |
|  [10]   | `IntrospectTokenAsync(IntrospectionRequest)`                                     | introspection principal               |
|  [11]   | `RevokeTokenAsync(RevocationRequest)`                                            | revocation response                   |
|  [12]   | `SignOutInteractivelyAsync(InteractiveSignOutRequest)`                           | nonce-bearing end-session handle      |

[ENTRYPOINT_SCOPE]: registration and metadata resolution — instance methods on `OpenIddictClientService`

| [INDEX] | [SURFACE]                                                                         | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `GetClientRegistrationsAsync(CT) -> ImmutableArray<OpenIddictClientRegistration>` | every registered registration      |
|  [02]   | `GetClientRegistrationByIssuerAsync(Uri, CT)`                                     | resolve registration by issuer     |
|  [03]   | `GetClientRegistrationByProviderNameAsync(string, CT)`                            | resolve by provider name           |
|  [04]   | `GetClientRegistrationByIdAsync(string, CT)`                                      | resolve by registration id         |
|  [05]   | `GetServerConfigurationByIssuerAsync(Uri, CT) -> OpenIddictConfiguration`         | discover server metadata by issuer |
|  [06]   | `GetServerConfigurationByProviderNameAsync(string, CT)`                           | discover by provider name          |
|  [07]   | `GetServerConfigurationByRegistrationIdAsync(string, CT)`                         | discover by registration id        |

[ENTRYPOINT_SCOPE]: DI and builder wiring — `Microsoft.Extensions.DependencyInjection`

Development signing members supply development-only material.

| [INDEX] | [SURFACE]                                                            | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------- | :--------------------------- |
|  [01]   | `AddClient(this OpenIddictBuilder)`                                  | `OpenIddictClientBuilder`    |
|  [02]   | `AddClient(this OpenIddictBuilder, Action<OpenIddictClientBuilder>)` | `OpenIddictBuilder`          |
|  [03]   | `AddRegistration(OpenIddictClientRegistration)`                      | provider registration        |
|  [04]   | `AllowAuthorizationCodeFlow()`                                       | authorization code           |
|  [05]   | `AllowClientCredentialsFlow()`                                       | client credentials           |
|  [06]   | `AllowDeviceAuthorizationFlow()`                                     | device authorization         |
|  [07]   | `AllowRefreshTokenFlow()`                                            | refresh token                |
|  [08]   | `AllowTokenExchangeFlow()`                                           | token exchange               |
|  [09]   | `AllowCustomFlow(string)`                                            | custom grant                 |
|  [10]   | `AllowHybridFlow()`                                                  | hybrid flow                  |
|  [11]   | `AllowImplicitFlow()`                                                | implicit flow                |
|  [12]   | `AllowPasswordFlow()`                                                | password flow                |
|  [13]   | `AllowNoneFlow()`                                                    | none flow                    |
|  [14]   | `AddSigningCredentials`                                              | client-assertion credentials |
|  [15]   | `AddSigningCertificate`                                              | client-assertion certificate |
|  [16]   | `AddSigningKey`                                                      | client-assertion key         |
|  [17]   | `AddEncryptionCredentials`                                           | token-encryption credentials |
|  [18]   | `AddEncryptionCertificate`                                           | token-encryption certificate |
|  [19]   | `AddEncryptionKey`                                                   | token-encryption key         |
|  [20]   | `AddDevelopmentSigningCertificate()`                                 | development signing          |
|  [21]   | `AddEphemeralSigningKey()`                                           | ephemeral signing            |
|  [22]   | `SetRedirectionEndpointUris(...)`                                    | redirection callbacks        |
|  [23]   | `SetPostLogoutRedirectionEndpointUris(...)`                          | sign-out callbacks           |
|  [24]   | `SetStateTokenLifetime(TimeSpan)`                                    | state-token lifetime         |
|  [25]   | `SetClientAssertionLifetime(TimeSpan)`                               | assertion lifetime           |
|  [26]   | `DisableTokenStorage()`                                              | no server-side storage       |
|  [27]   | `AddEventHandler(...)`                                               | event handler                |
|  [28]   | `RemoveEventHandler(...)`                                            | handler removal              |
|  [29]   | `Configure(Action<OpenIddictClientOptions>)`                         | options override             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- package boundary: `OpenIddict.Client` is the relying-party client; no `OpenIddict.Server` package participates in token acquisition
- service entry: `OpenIddictClientService` is the single resolved service, and every flow is one polymorphic request record discriminated by record type, never a per-flow service
- challenge/redeem split: interactive and device flows pair a `Challenge*Async` verb emitting a `Nonce`-bearing `*ChallengeResult` with an `Authenticate*Async` verb that redeems the nonce
- registration selection: `Issuer`, `ProviderName`, and `RegistrationId` on each request resolve one `OpenIddictClientRegistration`; `RegistrationId` disambiguates registrations sharing an issuer or provider
- PKCE negotiation: `OpenIddictClientRegistration.CodeChallengeMethods` and `OpenIddictClientOptions.CodeChallengeMethods` drive code-challenge selection; `InteractiveChallengeRequest.CodeChallengeMethod` is an advanced override left null in normal use
- token binding: `OpenIddictClientRegistration.TokenBindingMethods` advertises DPoP/mTLS binding, and `TokenBindingCertificate` on interactive and credential requests supplies the X.509 certificate bound to issued tokens
- pushed authorization: PAR is on by default, suppressed per registration through `OpenIddictClientRegistration.DisablePushedAuthorizationRequests`
- capability sets: `GrantTypes`, `ResponseTypes`, `ResponseModes`, `ClientAuthenticationMethods`, `Scopes`, `SigningCredentials`, and `EncryptionCredentials` on the registration negotiate against discovered server metadata
- result shape: an authentication result carries `Principal` (merged claims), token strings (`AccessToken`, `RefreshToken`, `BackchannelAccessToken`, `FrontchannelAccessToken`, identity/userinfo tokens), expiration dates, and the raw `OpenIddictResponse`
- device challenge result: `DeviceChallengeResult` carries `UserCode`, `DeviceCode`, `VerificationUri`, `VerificationUriComplete`, `ExpiresIn`, and `Interval`; `DeviceAuthenticationRequest` consumes `DeviceCode`, `Interval`, and `Timeout` for the poll loop
- introspection/revocation: `IntrospectionResult` exposes a `Principal` and raw `IntrospectionResponse`, `RevocationResult` the raw `RevocationResponse`; both take `Token` with optional `TokenTypeHint`
- endpoint enum: `OpenIddictClientEndpointType` classifies inbound redirection — `Unknown`, `Redirection`, `PostLogoutRedirection`

[STACKING]:
- `Microsoft.IdentityModel.JsonWebTokens`(`.api/api-identitymodel-jwt.md`): the access and identity tokens `OpenIddictClientService` acquires feed `JsonWebTokenHandler.ValidateTokenAsync` against `TokenValidationParameters`; this catalog obtains, that handler validates
- `Microsoft.IdentityModel.Protocols`(`.api/api-identitymodel-protocols.md`): a `ConfigurationManager<OpenIdConnectConfiguration>` refreshes the `JsonWebKeySet` for the `OpenIddictConfiguration` this client discovers, assigned to `TokenValidationParameters.ConfigurationManager` so signing keys track JWKS rotation
- `Microsoft.Extensions.DependencyInjection`(`.api/api-di.md`): `services.AddOpenIddict().AddClient(...)` composes the builder onto `IServiceCollection`, and `OpenIddictClientService` resolves through `GetRequiredService`
- within-lib: `OpenIddictClientService` is the single polymorphic entry — a new flow is one request-record type discriminated at the verb, and `OpenIddictClientBuilder` folds flow, credential, and registration wiring at composition

[LOCAL_ADMISSION]:
- Resolve `OpenIddictClientService` from DI; never construct flow handlers directly.
- Register through `services.AddOpenIddict().AddClient(...)`, enabling exactly the flows the agent uses with the `Allow*Flow` builders, and register each provider once through `AddRegistration`.
- Drive machine-to-machine acquisition through `AuthenticateWithClientCredentialsAsync`, refresh through `AuthenticateWithRefreshTokenAsync`, and cross-service delegation through `AuthenticateWithTokenExchangeAsync`.
- Drive headless enrollment through `ChallengeUsingDeviceAsync` then `AuthenticateWithDeviceAsync`, honoring `Interval` and `Timeout` from the challenge result.
- Drive RP-initiated logout through `SignOutInteractivelyAsync` with the cached `IdentityTokenHint`, redeeming the returned `Nonce` exactly as the interactive challenge nonce, never a hand-built end-session URL.
- Leave PKCE, DPoP, PAR, and client-authentication-method selection to registration capability sets and server metadata; set an advanced override property only when a provider rejects the negotiated default.
- Read tokens and expiration from the typed result `required` members; treat the raw `OpenIddictResponse` and principals as the audit surface.

[RAIL_LAW]:
- Package: `OpenIddict.Client`
- Owns: OAuth 2.0 / OIDC relying-party token acquisition, RP-initiated interactive sign-out, introspection, revocation, registration resolution, and server-metadata discovery
- Accept: flow-specific request records through `OpenIddictClientService`; provider identity through `OpenIddictClientRegistration`; flow and credential enablement through `OpenIddictClientBuilder`
- Reject: hand-rolled authorization-URL/PKCE/DPoP construction, direct token-endpoint HTTP calls, per-flow service proliferation, bypassing registration capability negotiation with manual override properties
