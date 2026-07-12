# [RASM_APPHOST_API_OPENIDDICT_CLIENT]

`OpenIddict.Client` supplies a standalone OAuth 2.0 / OpenID Connect relying-party client: `OpenIddictClientService` exposes flow-specific token-acquisition verbs (authorization-code with PKCE, client-credentials, device, refresh, token-exchange, password, custom-grant), RP-initiated interactive sign-out, token introspection and revocation, registration resolution, and server-metadata discovery. PKCE, DPoP/mTLS token binding, and pushed authorization requests are negotiated automatically from the per-`OpenIddictClientRegistration` capability sets. This is the AppHost agent/identity rail: the OIDC token-acquisition client behind delegated and machine-to-machine credential flows.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenIddict.Client`
- package: `OpenIddict.Client`
- license: `Apache-2.0`
- assembly: `OpenIddict.Client`
- namespace: `OpenIddict.Client` (service, models, registration, options, endpoint enum), `Microsoft.Extensions.DependencyInjection` (`OpenIddictClientBuilder`, `OpenIddictClientExtensions`)
- target: multi-target (`net462`, `net472`, `net48`, `net8.0`, `net9.0`, `net10.0`, `netstandard2.0`, `netstandard2.1`); the `net10.0` consumer binds `lib/net10.0` — a first-class `net10.0` asset ships, not a `net8.0` fallback
- asset: pure-managed runtime library; no native asset, no RID burden
- depends: `OpenIddict.Abstractions` `7.5.0`, `Microsoft.IdentityModel.JsonWebTokens` `8.16.0`, `Microsoft.IdentityModel.Protocols` `8.16.0`, `Microsoft.Extensions.Logging` `10.0.7` in the `net10.0` group
- abstractions: `OpenIddict.Abstractions` carries `OpenIddictRequest`, `OpenIddictResponse`, `OpenIddictParameter`, and `OpenIddictConstants`
- dependency alignment: `Microsoft.IdentityModel.Protocols` is the shared base the validation leg pulls; its `Microsoft.IdentityModel.Protocols.OpenIdConnect` `8.19.1` pin makes NuGet unify the `Microsoft.IdentityModel.*` graph to the `8.19.1` line
- rail: oidc-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client service and configuration — `OpenIddict.Client`
- rail: oidc-client

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                              |
| :-----: | :----------------------------- | :----------------- | :---------------------------------- |
|  [01]   | `OpenIddictClientService`      | service class      | flow-specific token acquisition     |
|  [02]   | `OpenIddictClientRegistration` | configuration root | per-provider client identity        |
|  [03]   | `OpenIddictClientOptions`      | options class      | global client policy and registry   |
|  [04]   | `OpenIddictClientModels`       | static record host | request/result record family        |
|  [05]   | `OpenIddictClientEndpointType` | enum               | redirection endpoint classification |

[PUBLIC_TYPE_SCOPE]: request/result records — `OpenIddictClientModels`
- rail: oidc-client

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]  | [RAIL]                                |
| :-----: | :--------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `InteractiveChallengeRequest`            | request record | authorization-code+PKCE challenge     |
|  [02]   | `InteractiveChallengeResult`             | result record  | nonce-carrying challenge handle       |
|  [03]   | `InteractiveAuthenticationRequest`       | request record | authorization-code callback exchange  |
|  [04]   | `InteractiveAuthenticationResult`        | result record  | front/backchannel token bundle        |
|  [05]   | `ClientCredentialsAuthenticationRequest` | request record | machine-to-machine token request      |
|  [06]   | `ClientCredentialsAuthenticationResult`  | result record  | client-credentials token bundle       |
|  [07]   | `DeviceChallengeRequest`                 | request record | device-authorization start            |
|  [08]   | `DeviceChallengeResult`                  | result record  | user/device code and verification URI |
|  [09]   | `DeviceAuthenticationRequest`            | request record | device-code polling request           |
|  [10]   | `DeviceAuthenticationResult`             | result record  | device-flow token bundle              |
|  [11]   | `RefreshTokenAuthenticationRequest`      | request record | refresh-token grant                   |
|  [12]   | `TokenExchangeAuthenticationRequest`     | request record | RFC 8693 token exchange               |
|  [13]   | `CustomGrantAuthenticationRequest`       | request record | arbitrary grant-type token request    |
|  [14]   | `PasswordAuthenticationRequest`          | request record | resource-owner password grant         |
|  [15]   | `IntrospectionRequest`                   | request record | RFC 7662 token introspection          |
|  [16]   | `IntrospectionResult`                    | result record  | introspection principal and response  |
|  [17]   | `RevocationRequest`                      | request record | RFC 7009 token revocation             |
|  [18]   | `RevocationResult`                       | result record  | revocation response carrier           |
|  [19]   | `InteractiveSignOutRequest`              | request record | end-session challenge                 |
|  [20]   | `InteractiveSignOutResult`               | result record  | nonce-carrying sign-out handle        |

[PUBLIC_TYPE_SCOPE]: DI registration and builder — `Microsoft.Extensions.DependencyInjection`
- rail: oidc-client

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :--------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `OpenIddictClientExtensions` | static class  | `AddClient` DI entry on the builder       |
|  [02]   | `OpenIddictClientBuilder`    | builder class | flow, credential, and registration wiring |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: token-acquisition verbs — `OpenIddictClientService`
- rail: oidc-client

The challenge verbs emit a nonce-bearing handle that the matching authentication verb redeems. Each request record discriminates the target registration by `Issuer`, `ProviderName`, or `RegistrationId`; setting `RegistrationId` is mandatory when multiple registrations share an issuer or provider. `SignOutInteractivelyAsync` starts RP-initiated logout and returns an `InteractiveSignOutResult` nonce handle.

| [INDEX] | [SURFACE]                                                                        | [OUTCOME]             |
| :-----: | :------------------------------------------------------------------------------- | :-------------------- |
|  [01]   | `ChallengeInteractivelyAsync(InteractiveChallengeRequest)`                       | authorization handle  |
|  [02]   | `AuthenticateInteractivelyAsync(InteractiveAuthenticationRequest)`               | token bundle          |
|  [03]   | `AuthenticateWithClientCredentialsAsync(ClientCredentialsAuthenticationRequest)` | token bundle          |
|  [04]   | `ChallengeUsingDeviceAsync(DeviceChallengeRequest)`                              | device challenge      |
|  [05]   | `AuthenticateWithDeviceAsync(DeviceAuthenticationRequest)`                       | token bundle          |
|  [06]   | `AuthenticateWithRefreshTokenAsync(RefreshTokenAuthenticationRequest)`           | refreshed tokens      |
|  [07]   | `AuthenticateWithTokenExchangeAsync(TokenExchangeAuthenticationRequest)`         | exchanged tokens      |
|  [08]   | `AuthenticateWithCustomGrantAsync(CustomGrantAuthenticationRequest)`             | custom-grant tokens   |
|  [09]   | `AuthenticateWithPasswordAsync(PasswordAuthenticationRequest)`                   | password-grant tokens |
|  [10]   | `IntrospectTokenAsync(IntrospectionRequest)`                                     | principal             |
|  [11]   | `RevokeTokenAsync(RevocationRequest)`                                            | revocation response   |
|  [12]   | `SignOutInteractivelyAsync(InteractiveSignOutRequest)`                           | sign-out handle       |

[FLOW_DETAILS]:
- `ChallengeInteractivelyAsync`: builds the authorization URL and returns its nonce handle
- `ChallengeUsingDeviceAsync`: returns the user code, device code, and verification URI
- `AuthenticateWithDeviceAsync`: polls the device code for a token bundle
- `AuthenticateWithTokenExchangeAsync`: executes RFC 8693 token exchange
- `AuthenticateWithCustomGrantAsync`: submits an arbitrary `GrantType` token request
- `IntrospectTokenAsync`: returns the `IntrospectionResult` principal
- `RevokeTokenAsync`: returns the `RevocationResult`

[ENTRYPOINT_SCOPE]: registration and metadata resolution — `OpenIddictClientService`
- rail: oidc-client

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY]     | [RAIL]                                         |
| :-----: | :-------------------------------------------------------- | :----------------- | :--------------------------------------------- |
|  [01]   | `GetClientRegistrationsAsync(CT)`                         | registration query | `ImmutableArray<OpenIddictClientRegistration>` |
|  [02]   | `GetClientRegistrationByIssuerAsync(Uri, CT)`             | registration query | resolve by issuer                              |
|  [03]   | `GetClientRegistrationByProviderNameAsync(string, CT)`    | registration query | resolve by provider name                       |
|  [04]   | `GetClientRegistrationByIdAsync(string, CT)`              | registration query | resolve by registration id                     |
|  [05]   | `GetServerConfigurationByIssuerAsync(Uri, CT)`            | metadata discovery | `OpenIddictConfiguration` by issuer            |
|  [06]   | `GetServerConfigurationByProviderNameAsync(string, CT)`   | metadata discovery | `OpenIddictConfiguration` by provider          |
|  [07]   | `GetServerConfigurationByRegistrationIdAsync(string, CT)` | metadata discovery | `OpenIddictConfiguration` by registration      |

[ENTRYPOINT_SCOPE]: DI and builder wiring — `Microsoft.Extensions.DependencyInjection`
- rail: oidc-client

Credential member names denote their overload families; the development signing members supply development-only material.

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

[CLIENT_TOPOLOGY]:
- package boundary: `OpenIddict.Client` is the relying-party client; no `OpenIddict.Server` package participates in token acquisition
- acquisition stack: `OpenIddict.Client` requests `Microsoft.IdentityModel.JsonWebTokens` `8.16.0` and the `Microsoft.IdentityModel.Protocols` `8.16.0` refresh-manager substrate
- validation convergence: direct `Microsoft.IdentityModel.Protocols.OpenIdConnect` and `Microsoft.IdentityModel.JsonWebTokens` `8.19.1` pins unify the transitive IdentityModel graph to the lockfile-resolved `8.19.1` line
- leg boundary: this client discovers its internal `OpenIddictConfiguration` and acquires tokens; `Microsoft.IdentityModel.JsonWebTokens` validates them against JWKS refreshed by `ConfigurationManager<OpenIdConnectConfiguration>`
- service entry: `OpenIddictClientService` is the single resolved service; every flow is one polymorphic request record discriminated by record type, not a per-flow service
- challenge/redeem split: interactive and device flows pair a `Challenge*Async` verb emitting a `Nonce`-bearing `*ChallengeResult` with an `Authenticate*Async` verb that redeems the nonce; client-credentials, refresh, exchange, custom, and password flows are single-call
- sign-out: `SignOutInteractivelyAsync` is the end-session peer of `ChallengeInteractivelyAsync`; it consumes an `InteractiveSignOutRequest` carrying `IdentityTokenHint`, `Issuer`, `ProviderName`, or `RegistrationId` and returns an `InteractiveSignOutResult` nonce handle through the same service
- registration selection: `Issuer`, `ProviderName`, and `RegistrationId` on each request resolve one `OpenIddictClientRegistration`; `RegistrationId` is required to disambiguate registrations that share an issuer or provider
- PKCE negotiation: `OpenIddictClientRegistration.CodeChallengeMethods` and `OpenIddictClientOptions.CodeChallengeMethods` drive automatic code-challenge selection; `InteractiveChallengeRequest.CodeChallengeMethod` is an advanced override and is left null in normal use
- token binding (DPoP / mTLS): `OpenIddictClientRegistration.TokenBindingMethods` advertises supported binding; `TokenBindingCertificate` on interactive and credential requests supplies the X.509 certificate bound to issued access/refresh tokens
- pushed authorization: PAR is on by default and suppressed per registration through `OpenIddictClientRegistration.DisablePushedAuthorizationRequests`
- capability sets: `GrantTypes`, `ResponseTypes`, `ResponseModes`, `ClientAuthenticationMethods`, `Scopes`, `SigningCredentials`, and `EncryptionCredentials` on the registration are negotiated against discovered server metadata
- result shape: authentication results carry `Principal` (merged claims) plus token strings (`AccessToken`, `RefreshToken`, `BackchannelAccessToken`, `FrontchannelAccessToken`, identity/userinfo tokens), expiration dates, and the raw `OpenIddictResponse`
- device challenge result: `DeviceChallengeResult` carries `UserCode`, `DeviceCode`, `VerificationUri`, `VerificationUriComplete`, `ExpiresIn`, and `Interval` for the poll loop; `DeviceAuthenticationRequest` consumes `DeviceCode`, `Interval`, and `Timeout`
- introspection/revocation: `IntrospectionResult` exposes a `Principal` and raw `IntrospectionResponse`; `RevocationResult` carries the raw `RevocationResponse`; both take `Token` plus optional `TokenTypeHint`
- endpoint enum: `OpenIddictClientEndpointType` classifies inbound redirection (`Redirection`, `PostLogoutRedirection`, `Unknown`) for callback handling

[LOCAL_ADMISSION]:
- Resolve `OpenIddictClientService` from DI; never construct flow handlers directly.
- Register the client through `services.AddOpenIddict().AddClient(...)`; enable exactly the flows the agent uses with the `Allow*Flow` builders, and register each provider once through `AddRegistration`.
- Drive machine-to-machine acquisition through `AuthenticateWithClientCredentialsAsync`; refresh through `AuthenticateWithRefreshTokenAsync`; cross-service delegation through `AuthenticateWithTokenExchangeAsync`.
- Drive headless/device enrollment through `ChallengeUsingDeviceAsync` then `AuthenticateWithDeviceAsync`, honoring `Interval` and `Timeout` from the challenge result.
- Drive RP-initiated logout through `SignOutInteractivelyAsync`, passing the cached `IdentityTokenHint`; redeem the returned `Nonce` exactly as the interactive challenge nonce is redeemed, never a hand-built end-session URL.
- Leave PKCE, DPoP, PAR, and client-authentication-method selection to registration capability sets and server metadata; set the advanced override properties only when a provider rejects the negotiated default.
- Read tokens and expiration from the typed result `required` members; treat the raw `OpenIddictResponse`/principals as the audit surface, the typed members as the contract.

[RAIL_LAW]:
- Package: `OpenIddict.Client`
- Owns: OAuth 2.0 / OIDC relying-party token acquisition, RP-initiated interactive sign-out, introspection, revocation, registration resolution, and server-metadata discovery
- Accept: flow-specific request records through `OpenIddictClientService`; provider identity through `OpenIddictClientRegistration`; flow and credential enablement through `OpenIddictClientBuilder`
- Reject: hand-rolled authorization-URL/PKCE/DPoP construction, direct token-endpoint HTTP calls, per-flow service proliferation, bypassing registration capability negotiation with manual override properties
