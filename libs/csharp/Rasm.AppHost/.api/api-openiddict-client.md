# [RASM_APPHOST_API_OPENIDDICT_CLIENT]

`OpenIddict.Client` supplies a standalone OAuth 2.0 / OpenID Connect relying-party client: `OpenIddictClientService` exposes flow-specific token-acquisition verbs (authorization-code with PKCE, client-credentials, device, refresh, token-exchange, password, custom-grant), token introspection and revocation, registration resolution, and server-metadata discovery. PKCE, DPoP/mTLS token binding, and pushed authorization requests are negotiated automatically from the per-`OpenIddictClientRegistration` capability sets. This is the AppHost agent/identity rail: the OIDC token-acquisition client behind delegated and machine-to-machine credential flows.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenIddict.Client`
- package: `OpenIddict.Client`
- assembly: `OpenIddict.Client`
- namespace: `OpenIddict.Client`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- depends: `OpenIddict.Abstractions`, `Microsoft.IdentityModel.JsonWebTokens`, `Microsoft.IdentityModel.Protocols` (the refresh-manager substrate shared with the validation leg — `api-identitymodel-protocols.md`)
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

[PUBLIC_TYPE_SCOPE]: DI registration and builder — `Microsoft.Extensions.DependencyInjection`
- rail: oidc-client

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :--------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `OpenIddictClientExtensions` | static class  | `AddClient` DI entry on the builder       |
|  [02]   | `OpenIddictClientBuilder`    | builder class | flow, credential, and registration wiring |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: token-acquisition verbs — `OpenIddictClientService`
- rail: oidc-client

The challenge verbs emit a nonce-bearing handle that the matching authentication verb redeems. Each request record discriminates the target registration by `Issuer`, `ProviderName`, or `RegistrationId`; setting `RegistrationId` is mandatory when multiple registrations share an issuer or provider.

| [INDEX] | [SURFACE]                                                                        | [ENTRY_FAMILY]      | [RAIL]                                  |
| :-----: | :------------------------------------------------------------------------------- | :------------------ | :-------------------------------------- |
|  [01]   | `ChallengeInteractivelyAsync(InteractiveChallengeRequest)`                       | challenge verb      | builds authorization URL, returns nonce |
|  [02]   | `AuthenticateInteractivelyAsync(InteractiveAuthenticationRequest)`               | authentication verb | redeems callback for token bundle       |
|  [03]   | `AuthenticateWithClientCredentialsAsync(ClientCredentialsAuthenticationRequest)` | authentication verb | client-credentials token bundle         |
|  [04]   | `ChallengeUsingDeviceAsync(DeviceChallengeRequest)`                              | challenge verb      | device/user code and verification URI   |
|  [05]   | `AuthenticateWithDeviceAsync(DeviceAuthenticationRequest)`                       | authentication verb | polls device code to token bundle       |
|  [06]   | `AuthenticateWithRefreshTokenAsync(RefreshTokenAuthenticationRequest)`           | authentication verb | refresh-token grant                     |
|  [07]   | `AuthenticateWithTokenExchangeAsync(TokenExchangeAuthenticationRequest)`         | authentication verb | RFC 8693 token exchange                 |
|  [08]   | `AuthenticateWithCustomGrantAsync(CustomGrantAuthenticationRequest)`             | authentication verb | arbitrary `GrantType` token request     |
|  [09]   | `AuthenticateWithPasswordAsync(PasswordAuthenticationRequest)`                   | authentication verb | resource-owner password grant           |
|  [10]   | `IntrospectTokenAsync(IntrospectionRequest)`                                     | introspection       | returns `IntrospectionResult` principal |
|  [11]   | `RevokeTokenAsync(RevocationRequest)`                                            | revocation          | returns `RevocationResult`              |

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

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY]  | [RAIL]                                     |
| :-----: | :------------------------------------------------------------------------------------------------ | :-------------- | :----------------------------------------- |
|  [01]   | `AddClient(this OpenIddictBuilder)`                                                               | DI registration | returns `OpenIddictClientBuilder`          |
|  [02]   | `AddClient(this OpenIddictBuilder, Action<OpenIddictClientBuilder>)`                              | DI registration | configures and returns `OpenIddictBuilder` |
|  [03]   | `AddRegistration(OpenIddictClientRegistration)`                                                   | builder fluent  | registers a provider client identity       |
|  [04]   | `AllowAuthorizationCodeFlow()`                                                                    | builder fluent  | enables authorization-code+PKCE            |
|  [05]   | `AllowClientCredentialsFlow()`                                                                    | builder fluent  | enables client-credentials grant           |
|  [06]   | `AllowDeviceAuthorizationFlow()`                                                                  | builder fluent  | enables device-authorization grant         |
|  [07]   | `AllowRefreshTokenFlow()`                                                                         | builder fluent  | enables refresh-token grant                |
|  [08]   | `AllowTokenExchangeFlow()` / `AllowCustomFlow(string)`                                            | builder fluent  | enables exchange and custom grants         |
|  [09]   | `AllowHybridFlow()` / `AllowImplicitFlow()` / `AllowPasswordFlow()` / `AllowNoneFlow()`           | builder fluent  | enables remaining interactive/legacy flows |
|  [10]   | `AddSigningCredentials` / `AddSigningCertificate` / `AddSigningKey` family                        | builder fluent  | client-assertion signing material          |
|  [11]   | `AddEncryptionCredentials` / `AddEncryptionCertificate` / `AddEncryptionKey` family               | builder fluent  | token-encryption material                  |
|  [12]   | `AddDevelopmentSigningCertificate()` / `AddEphemeralSigningKey()`                                 | builder fluent  | development-only signing material          |
|  [13]   | `SetRedirectionEndpointUris(...)` / `SetPostLogoutRedirectionEndpointUris(...)`                   | builder fluent  | callback URI registration                  |
|  [14]   | `SetStateTokenLifetime(TimeSpan)` / `SetClientAssertionLifetime(TimeSpan)`                        | builder fluent  | token-lifetime policy                      |
|  [15]   | `DisableTokenStorage()`                                                                           | builder fluent  | drops server-side state/token persistence  |
|  [16]   | `AddEventHandler(...)` / `RemoveEventHandler(...)` / `Configure(Action<OpenIddictClientOptions>)` | builder fluent  | event-pipeline and options override        |

## [04]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- standalone package: `OpenIddict.Client` is the relying-party client; no `OpenIddict.Server` package participates in token acquisition. It directly depends on `Microsoft.IdentityModel.JsonWebTokens` and the base `Microsoft.IdentityModel.Protocols` (the refresh-manager substrate cataloged at `api-identitymodel-protocols.md`) — the same base package the IdentityModel validation/discovery leg pulls in, unified to the central `8.19.1` line. The two legs are peers that meet at the validated token: this client acquires tokens (using its own internal `OpenIddictConfiguration` server discovery for the acquisition request), and `Microsoft.IdentityModel.JsonWebTokens` validates them against the JWKS that a `ConfigurationManager<OpenIdConnectConfiguration>` refreshes
- service entry: `OpenIddictClientService` is the single resolved service; every flow is one polymorphic request record discriminated by record type, not a per-flow service
- challenge/redeem split: interactive and device flows pair a `Challenge*Async` verb (emits a `Nonce`) with an `Authenticate*Async` verb that redeems the nonce; non-interactive flows (client-credentials, refresh, exchange, custom, password) are single-call
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
- Leave PKCE, DPoP, PAR, and client-authentication-method selection to registration capability sets and server metadata; set the advanced override properties only when a provider rejects the negotiated default.
- Read tokens and expiration from the typed result `required` members; treat the raw `OpenIddictResponse`/principals as the audit surface, the typed members as the contract.

[RAIL_LAW]:
- Package: `OpenIddict.Client`
- Owns: OAuth 2.0 / OIDC relying-party token acquisition, introspection, revocation, registration resolution, and server-metadata discovery
- Accept: flow-specific request records through `OpenIddictClientService`; provider identity through `OpenIddictClientRegistration`; flow and credential enablement through `OpenIddictClientBuilder`
- Reject: hand-rolled authorization-URL/PKCE/DPoP construction, direct token-endpoint HTTP calls, per-flow service proliferation, bypassing registration capability negotiation with manual override properties
