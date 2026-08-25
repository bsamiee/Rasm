# [TS_SECURITY_API_OPENID_CLIENT]

`openid-client` owns the OpenID-certified relying-party surface whole — the folder's SOLE OAuth custodian across the browser and machine-identity planes alike.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the RP handle, the client-auth vocabulary, and the token accessors

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Configuration`                | class         | one RP handle every grant, builder, and read takes first |
|  [02]   | `ClientAuth`                   | delegate      | client authentication, minted at [02] below              |
|  [03]   | `ServerMetadata`               | interface     | issuer well-known record, discovery-resolved or pinned   |
|  [04]   | `ClientMetadata`               | interface     | RP registration record carrying `client_secret`          |
|  [05]   | `TokenEndpointResponse`        | interface     | open grant response; index-signature fields untyped      |
|  [06]   | `TokenEndpointResponseHelpers` | interface     | `.claims()` and `.expiresIn()` on a resolved response    |
|  [07]   | `IDToken`                      | interface     | parsed id-token claim set `.claims()` returns            |
|  [08]   | `DPoPHandle`                   | interface     | nonce-tracking handle; `calculateThumbprint()` is `jkt`  |
|  [09]   | `CryptoKeyPair`                | interface     | DPoP signing pair the handle wraps                       |
|  [10]   | `AuthorizationCodeGrantChecks` | interface     | callback-leg checks: verifier, state, nonce, maxAge      |
|  [11]   | `ServerMetadataHelpers`        | interface     | `supportsPKCE(method?)` off the resolved metadata        |

- [02]-[CLIENT_AUTH]: `ClientSecretPost(string?)` `ClientSecretBasic(string?)` `ClientSecretJwt(string?, opts?)` `PrivateKeyJwt(CryptoKey | PrivateKey, opts?)` `TlsClientAuth()` `None()` — each returns `ClientAuth`. `PrivateKey` spells the asymmetric form as `{ key, kid? }`, whose `kid` becomes the JOSE header key id, so a vendor-issued key id rides the handle rather than a thumbprint.
- [02]-[ASSERTION_HOOK]: both JWT builders mint `{ jti, aud: as.issuer, exp: now + 60, iat, nbf, iss: client_id, sub: client_id }` and hand header and payload to `options[modifyAssertion]` before signing — the one seam an issuer demanding a divergent claim (Apple's team-scoped `iss`) takes without a hand-built assertion.
- [02]-[POLL_OPTIONS]: `DeviceAuthorizationGrantPollOptions` and `BackchannelAuthenticationGrantPollOptions` — each `DPoPOptions & { signal?: AbortSignal }`, the two poll legs' WHOLE caller surface; the deprecated `DeviceAutorizationGrantPollOptions` typo alias never lands.
- [02]-[POLL_PACING]: pacing is package-interior and spec-owned, never a knob — each poll waits the initiate response's own `interval` (5 seconds where the server states none), widens it by 5 seconds on `slow_down`, honors a served `Retry-After` on `authorization_pending`, and defaults its deadline to the initiate response's `expires_in`, so `signal` is the one caller-side bound and no interval or backoff member exists to configure.

[PUBLIC_TYPE_SCOPE]: the fault family every network member rejects into

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :------------------------------ | :------------ | :----------------------------------------------------- |
|  [01]   | `ClientError` (`code`)          | class         | request or validation refusal the module itself mints  |
|  [02]   | `ResponseBodyError` (`error`)   | class         | RFC 6749 token-endpoint error body; `error` is the arm |
|  [03]   | `AuthorizationResponseError`    | class         | error params on the authorization-response leg         |
|  [04]   | `WWWAuthenticateChallengeError` | class         | RS challenge; `cause[].parameters.error` is the arm    |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resolving one `Configuration` and requesting a token — token-producing members return `Promise<TokenEndpointResponse & TokenEndpointResponseHelpers>`, and a trailing `DPoPOptions` sender-constrains the issued token

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------------ | :------ | :------------------------------------------------ |
|  [01]   | `discovery(URL, string, metadata?, ClientAuth?, opts?)`             | factory | issuer well-known fetch to `Configuration`        |
|  [02]   | `new Configuration(ServerMetadata, string, …)`                      | ctor    | offline construction from pinned metadata         |
|  [03]   | `dynamicClientRegistration(URL, ClientMetadata, ClientAuth?, …)`    | factory | RFC 7591 self-registration to `Configuration`     |
|  [04]   | `clientCredentialsGrant(Configuration, params?, DPoPOptions?)`      | static  | machine-principal token; the default grant        |
|  [05]   | `genericGrantRequest(Configuration, string, params, DPoPOptions?)`  | static  | any grant type by string — RFC 8693 exchange      |
|  [06]   | `refreshTokenGrant(Configuration, string, params?, DPoPOptions?)`   | static  | rotation leg; a bound refresh stays constrained   |
|  [07]   | `authorizationCodeGrant(Configuration, URL \| Request, checks?, …)` | static  | first-party OIDC callback leg                     |
|  [08]   | `initiateDeviceAuthorization(Configuration, params)`                | static  | RFC 8628 start to `DeviceAuthorizationResponse`   |
|  [09]   | `pollDeviceAuthorizationGrant(Configuration, response, …)`          | static  | device polling leg to the token response          |
|  [10]   | `initiateBackchannelAuthentication(Configuration, params)`          | static  | CIBA start to `BackchannelAuthenticationResponse` |
|  [11]   | `pollBackchannelAuthenticationGrant(Configuration, response, …)`    | static  | CIBA polling leg to the token response            |

- [CONFIG_TOGGLE]: `enableNonRepudiationChecks` `useJwtResponseMode` `enableDecryptingResponses` `enableDetachedSignatureResponseChecks` `useCodeIdTokenResponseType` `useIdTokenResponseType` `allowInsecureRequests` `setJwksCache` `getJwksCache` — each mutates one `Configuration` in place at `Layer` construction.
- [CONFIG_READ]: `Configuration.serverMetadata() -> Readonly<ServerMetadata> & ServerMetadataHelpers` — the discovered record itself, so endpoints, `jwks_uri`, `revocation_endpoint`, `grant_types_supported`, and `id_token_signing_alg_values_supported` read off the issuer's own publication rather than a hand-asserted copy, and the helper's `supportsPKCE(method?)` answers `code_challenge_methods_supported` so the S256 decision is a read too.
- [CONFIG_DEADLINE]: `Configuration.timeout` is a settable number of SECONDS defaulting to 30 — it arms the `AbortSignal` every request off that handle carries, so a deadline seated here cancels the socket where an outer fiber timeout leaves it open.

[ENTRYPOINT_SCOPE]: minting request material, building redirects, and reading the protected resource

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `randomDPoPKeyPair(alg?, opts?) -> Promise<CryptoKeyPair>`             | static  | per-principal DPoP pair, `ES256` default     |
|  [02]   | `getDPoPHandle(Configuration, CryptoKeyPair, opts?) -> DPoPHandle`     | static  | nonce-tracking handle for `options.DPoP`     |
|  [03]   | `buildAuthorizationUrl(Configuration, params) -> URL`                  | static  | first-party code-leg redirect                |
|  [04]   | `buildAuthorizationUrlWithPAR(Configuration, params, opts?)`           | static  | RFC 9126 pushed authorization request        |
|  [05]   | `buildAuthorizationUrlWithJAR(Configuration, params, …)`               | static  | RFC 9101 signed request object               |
|  [06]   | `buildEndSessionUrl(Configuration, params?) -> URL`                    | static  | OIDC RP-initiated logout redirect            |
|  [07]   | `fetchProtectedResource(Configuration, string, URL, string, body?, …)` | static  | DPoP-proved RS call returning `Response`     |
|  [08]   | `fetchUserInfo(Configuration, string, string, opts?)`                  | static  | userinfo read; third arg pins expected `sub` |
|  [09]   | `tokenIntrospection(Configuration, string, params?)`                   | static  | RFC 7662 active-check                        |
|  [10]   | `tokenRevocation(Configuration, string, params?)`                      | static  | RFC 7009 sign-out leg                        |

- [REQUEST_SECRET]: `randomPKCECodeVerifier()` `calculatePKCECodeChallenge(string)` `randomState()` `randomNonce()` — request secrets minted per authorization leg.
- [SYMBOL_KEY]: `customFetch` `modifyAssertion` `clockSkew` `clockTolerance` `skipSubjectCheck` `skipStateCheck` — symbol-keyed overrides set on a `Configuration` or passed where a signature names the sentinel.
- `customFetch`: `CustomFetchOptions` hands the implementation `body`, `headers`, `method`, `redirect: "manual"`, an `AbortSignal` already armed from the configuration timeout, and `duplex` set to `"half"` whenever `body` is a `ReadableStream` — so forwarding the record wholesale streams a large protected-resource body rather than buffering it.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Configuration` holds the whole client state and every grant, builder, and read takes it first, so `Match` over a closed grant-kind tuple dispatches the row and one client type serves every grant.
- `ClientAuth` binds once at config time, so no secret rides a per-call argument; `PrivateKeyJwt` reads the same `Material` key custody the folder already owns.
- Token exchange rides `genericGrantRequest` under the RFC 8693 grant-type string held as a folder constant, with `subject_token`, `actor_token`, and `requested_token_type` on `params`.
- RFC 9449 DPoP binds per principal: `randomDPoPKeyPair` mints the pair, `getDPoPHandle` wraps it, `options.DPoP` carries it into every grant and resource call, and a constrained token surfaces as `token_type: "dpop"`.
- `calculateThumbprint()` answers the BARE RFC 7638 thumbprint serving as the confirmation value — what a resource server recomputes from the presented proof key — while the `urn:ietf:params:oauth:jwk-thumbprint` URI form is a subject spelling no verifier matches against `cnf.jkt`.
- One nonce demand travels two channels under one code: the token endpoint answers `ResponseBodyError` with `error` set to `use_dpop_nonce`, a resource server answers `WWWAuthenticateChallengeError` whose `cause` array carries the same code in `parameters.error`, and the handle records the served nonce as either lands — so recovery keys on the code across both classes and re-runs the leg once.
- `TokenEndpointResponse` index-signature fields, `.claims()`, and `fetchUserInfo` claims are ingress; a `Schema` decodes each before a field reaches the session.
- Browser ceremonies ride the same flat function set over the same handle — `buildAuthorizationUrl` out, `authorizationCodeGrant` back with `AuthorizationCodeGrantChecks` carrying the verifier, state, and nonce the app itself held across the redirect — so a provider is a `Configuration` source with a `ClientAuth`, never a per-issuer client class.
- `authorizationCodeGrant` strips the presented URL to derive the token request's `redirect_uri`, so that URL's origin and path must be the REGISTERED redirect and only its search may come from the request; `validateAuthResponse` inside it enforces the RFC 9207 `iss` whenever the issuer advertises it, so a callback URL rebuilt from selected params refuses at every such issuer.
- In-grant id-token handling runs a CLAIM check — `iss`, `aud`, `azp`, `exp`, and the advertised algorithm over a token delivered on the TLS-authenticated token-endpoint channel — and fetches no JWKS unless `enableNonRepudiationChecks` is set, so signature authority stays with the folder's `Jwt`.

[STACKING]:
- `effect`(`.api/effect.md`): `Effect.tryPromise` lifts each grant onto the rail; `Match.value` over `ClientError.code` and `ResponseBodyError.error` folds the fault surface into one tagged domain error; `Schema.decodeUnknown` brands `TokenEndpointResponse` and the `.claims()` `IDToken`; `Config.redacted` sources credentials at `Layer` construction, and `Redacted` carries `client_secret`, the DPoP private key, and every issued token; `Data.taggedEnum` mints the grant vocabulary `Match` dispatches.
- `@effect/platform`(`.api/effect-platform.md`): each device and CIBA poll brackets as ONE `Effect.tryPromise((signal) => poll(..., { signal }))` — the members self-pace on the server's `interval`/`slow_down`/`Retry-After` per `[02]-[POLL_OPTIONS]`, so a `Schedule` re-poll around them re-runs a settled ceremony and the fiber's own signal is the whole interrupt path; `fetchProtectedResource` fires from an `HttpApiEndpoint` handler, and `Headers.redact` keeps `DPoP` and `Authorization` proof headers out of logs; `customFetch` routes the library's own `fetch` through the shared retry client, forwarding `duplex` beside `body` so a streamed request survives the hop, and the handler owns the transport boundary.
- `jose`(`.api/jose.md`): the folder's `Jwt` owner mints and verifies the DPoP proof and the `cnf.jkt` binding; id-token claims from `fetchUserInfo` and `authorizationCodeGrant` re-verify through `createRemoteJWKSet` against the discovered `jwks_uri`, and `setJwksCache`/`getJwksCache` persist the same `{ jwks, uat }` snapshot jose's `jwksCache` symbol carries, so one WebCrypto key path serves both.
- `security/authn/oauth` (in-folder): the browser ceremony's roster is provider rows over `Issuer.Discovered`/`Issuer.Pinned` — `discovery` for the OIDC issuers and a `ServerMetadata` literal for plain-OAuth GitHub — with `randomState`/`randomPKCECodeVerifier`/`calculatePKCECodeChallenge`/`randomNonce` minting the per-request secrets a single-use snapshot carries across the redirect, `PrivateKeyJwt` under the `modifyAssertion` hook building Apple's team-issued client assertion, and `supportsPKCE`, `grant_types_supported`, `revocation_endpoint`, `end_session_endpoint`, `pushed_authorization_request_endpoint`, and `request_object_signing_alg_values_supported` deciding every capability the ceremony gates on — `buildAuthorizationUrlWithPAR`/`buildAuthorizationUrlWithJAR` carry the bind-elected request carriage and `buildEndSessionUrl` the RP-initiated logout leg.
- `security/authn/workload` (in-folder): the resolved machine principal projects into the per-call transport credential the runtime wave mounts, `expiresIn()` drives refresh rotation, and `tokenRevocation` fires on principal teardown.

[LOCAL_ADMISSION]:
- Thread one `Configuration` per issuer and dispatch the grant by `Match` over the closed kind tuple.
- Reach token exchange through `genericGrantRequest` with the RFC 8693 grant-type constant; a hand-built token-endpoint body duplicates a grant row.
- Mint one DPoP key pair per principal, read `cnf.jkt` off the handle's `calculateThumbprint()`, and pass the handle as `options.DPoP` wherever the AS advertises DPoP.
- Carry the whole authorization response into `authorizationCodeGrant` and rebase it onto the registered redirect, so `iss`, `error`, and `state` all reach the validator and the token request's `redirect_uri` stays the registered value.
- Read every issuer capability off `serverMetadata()` — endpoints, `jwks_uri`, PKCE support, the grant roster, the revocation leg — rather than asserting it beside the row.
