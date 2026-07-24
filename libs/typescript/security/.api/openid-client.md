# [TS_SECURITY_API_OPENID_CLIENT]

`openid-client` owns the OpenID-certified relying-party surface `security/authn/workload` composes for machine identity: one `Configuration` — server metadata, client id, client-auth strategy, DPoP handle — threads through a flat `grant(config, …)` function set, and RFC 9449 sender-constraint binds each issued token to a per-principal key. Browser-redirect custody stops at `arctic`; discovery-driven RP config, machine grants, PAR/JAR, introspection, revocation, and userinfo stop here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `openid-client`
- package: `openid-client` (MIT)
- module: ESM only (`type: module`); the root barrel carries the whole function and class surface, `openid-client/passport` a `Strategy` adapter the design never mounts
- runtime: isomorphic — global `fetch` and WebCrypto, no native addon; `jose` and `oauth4webapi` are its only runtime deps, and it re-exports the `oauth4webapi` response and fault types as flat root-barrel names
- asset: pure-TypeScript runtime library (`.js` + `.d.ts`); `TokenEndpointResponse` carries an untyped index signature, so a `Schema` at the seam is the real gate on a grant response
- rail: `security/authn/workload` — machine-grant and DPoP rows, catalogued at the folder tier

## [02]-[PUBLIC_TYPES]

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
|  [08]   | `DPoPHandle`                   | interface     | nonce-tracking proof handle passed as `options.DPoP`     |
|  [09]   | `CryptoKeyPair`                | interface     | DPoP signing pair; `cnf.jkt` is `Material.thumbprintUri` |

- [02]-[CLIENT_AUTH]: `ClientSecretPost(string?)` `ClientSecretBasic(string?)` `ClientSecretJwt(string?, opts?)` `PrivateKeyJwt(CryptoKey, opts?)` `TlsClientAuth()` `None()` — each returns `ClientAuth`.

[PUBLIC_TYPE_SCOPE]: the fault family every network member rejects into

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :------------------------------ | :------------ | :----------------------------------------------------- |
|  [01]   | `ClientError` (`code`)          | class         | request or validation refusal the module itself mints  |
|  [02]   | `ResponseBodyError` (`error`)   | class         | RFC 6749 token-endpoint error body; `error` is the arm |
|  [03]   | `AuthorizationResponseError`    | class         | error params on the authorization-response leg         |
|  [04]   | `WWWAuthenticateChallengeError` | class         | protected-resource 401 carrying a DPoP challenge       |

## [03]-[ENTRYPOINTS]

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Configuration` holds the whole client state and every grant, builder, and read takes it first, so `Match` over a closed grant-kind tuple dispatches the row and one client type serves every grant.
- `ClientAuth` binds once at config time, so no secret rides a per-call argument; `PrivateKeyJwt` reads the same `Material` key custody the folder already owns.
- Token exchange rides `genericGrantRequest` under the RFC 8693 grant-type string held as a folder constant, with `subject_token`, `actor_token`, and `requested_token_type` on `params`.
- DPoP binds per principal: `randomDPoPKeyPair` mints the pair, `getDPoPHandle` wraps it, `options.DPoP` carries it into every grant and resource call, and `cnf.jkt` is the folder's own `Material.thumbprintUri`, so proof binding and JWT identity share one key authority and a constrained token surfaces as `token_type: "dpop"`.
- `TokenEndpointResponse` index-signature fields, `.claims()`, and `fetchUserInfo` claims are ingress; a `Schema` decodes each before a field reaches the session.

[STACKING]:
- `effect`(`.api/effect.md`): `Effect.tryPromise` lifts each grant onto the rail; `Match.value` over `ClientError.code` and `ResponseBodyError.error` folds the fault surface into one tagged domain error; `Schema.decodeUnknown` brands `TokenEndpointResponse` and the `.claims()` `IDToken`; `Config.redacted` sources credentials at `Layer` construction, and `Redacted` carries `client_secret`, the DPoP private key, and every issued token; `Data.taggedEnum` mints the grant vocabulary `Match` dispatches.
- `@effect/platform`(`.api/effect-platform.md`): device and CIBA polling run as `Effect.repeat` over a `Schedule`, `fetchProtectedResource` fires from an `HttpApiEndpoint` handler, and `Headers.redact` keeps `DPoP` and `Authorization` proof headers out of logs; `customFetch` routes the library's own `fetch` through the shared retry client, so the handler owns the transport boundary.
- `jose`(`.api/jose.md`): the folder's `Jwt` owner mints and verifies the DPoP proof and the `cnf.jkt` binding; id-token claims from `fetchUserInfo` and `authorizationCodeGrant` re-verify through `createRemoteJWKSet` against the discovered `jwks_uri`, and `setJwksCache`/`getJwksCache` persist the same `{ jwks, uat }` snapshot jose's `jwksCache` symbol carries, so one WebCrypto key path serves both.
- `arctic`(`.api/arctic.md`): arctic owns the interactive browser authorization-code redirect and its provider rows; a first-party OIDC code flow demanding DPoP or PAR takes `authorizationCodeGrant` here.
- `security/authn/workload` (in-folder): the resolved machine principal projects into the per-call transport credential the runtime wave mounts, `expiresIn()` drives refresh rotation, and `tokenRevocation` fires on principal teardown.

[LOCAL_ADMISSION]:
- Thread one `Configuration` per issuer and dispatch the grant by `Match` over the closed kind tuple.
- Reach token exchange through `genericGrantRequest` with the RFC 8693 grant-type constant; a hand-built token-endpoint body duplicates a grant row.
- Mint one DPoP key pair per principal, bind `cnf.jkt` to `Material.thumbprintUri`, and pass the handle as `options.DPoP` wherever the AS advertises DPoP.

[RAIL_LAW]:
- Package: `openid-client`
- Owns: RP configuration and registration, the client-auth vocabulary, the machine-grant rows including token exchange and the device and CIBA legs, DPoP sender-constraint, PKCE/state/nonce mint, the authorization/PAR/JAR/end-session builders, protected-resource and userinfo reads, and token introspection and revocation
- Accept: one `Configuration` per issuer dispatched as a `Match` grant vocabulary, `Effect.tryPromise` around every network member, `Schema` decode of `TokenEndpointResponse` and `.claims()`, `jose` re-verification of id-token claims, `Redacted` secrets and DPoP keys, a per-principal handle bound to `Material.thumbprintUri`
- Reject: a client type or method family per grant, a hand-built token-endpoint body, a bearer machine token where DPoP is advertised, a second key custody for the DPoP proof, a helper return trusted before `Schema` and `jose`, a bare `Promise` reject in domain logic, the `openid-client/passport` adapter
