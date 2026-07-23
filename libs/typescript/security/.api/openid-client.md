# [TS_SECURITY_API_OPENID_CLIENT]

`openid-client` is the OpenID-certified relying-party client the `security/authn/workload` design composes for machine identity: one `Configuration` — server metadata, client id, client-auth strategy, DPoP handle — threaded through a flat `grant(config, …)` function surface. It owns the standards-complete grant set `arctic` does not: `clientCredentialsGrant`, `genericGrantRequest` under the RFC 8693 token-exchange grant-type, `refreshTokenGrant`, device, CIBA, and DPoP sender-constraint (RFC 9449) via `randomDPoPKeyPair` + `getDPoPHandle`.

Split custody is a hard line: `arctic` (`.api/arctic.md`) keeps the browser authorization-code redirect and its 60-plus provider rows; `openid-client` owns `discovery`-driven RP config, the machine grants, DPoP, PAR/JAR request objects, introspection/revocation, userinfo, and dynamic registration. Built over `oauth4webapi` + `jose` (WebCrypto + global `fetch`, ESM-only), every network member rejects with `ClientError`/`oauth` faults, and the DPoP `cnf.jkt` binding reuses the folder's own `Jwt` thumbprint (`Material.thumbprintUri`), never a second key custody.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `openid-client`
- package: `openid-client` (MIT)
- module format: ESM only (`type: module`); two export subpaths — the `openid-client` root barrel (the whole function+class surface) and `openid-client/passport` (a Passport `Strategy` adapter the design never consumes; Effect owns the transport boundary)
- runtime target: isomorphic (node, bun, browser, worker, edge) — global `fetch` + WebCrypto; runtime deps `jose` and `oauth4webapi` only, no native addon; the `oauth` namespace re-exports `oauth4webapi` response and error types (`TokenEndpointResponse`, `IDToken`, `UserInfoResponse`, `IntrospectionResponse`, `DeviceAuthorizationResponse`, `BackchannelAuthenticationResponse`)
- asset: pure-TypeScript runtime library (`.js` + `.d.ts`); `TokenEndpointResponse` index-signature fields are untyped, so a `Schema` at the seam is the real gate on the grant response shape
- rail: `security/authn/workload` — machine-grant + DPoP rows (admitted at the folder tier; catalogued here, not in the branch substrate)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the configuration object, the client-auth vocabulary, and the token accessor — consumer `authn/workload`
- rail: surfaces-and-dispatch

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]     | [CONSUMER]                                                |
| :-----: | :----------------------------------- | :---------------- | :-------------------------------------------------------- |
|  [01]   | `Configuration`                      | client config     | one RP handle threaded through every grant; `discovery`   |
|  [02]   | `ClientAuth` (6-strategy vocabulary) | auth-method row   | one `Match` arm per method; members at [01] below         |
|  [03]   | `ServerMetadata` / `ClientMetadata`  | metadata record   | discovery-resolved or explicit; `Schema`-guarded          |
|  [04]   | `TokenEndpointResponseHelpers`       | token accessor    | `.claims()`/`.expiresIn()`; → `session/token`             |
|  [05]   | `DPoPHandle`                         | proof handle      | sender-constraint nonce cache; passed `options.DPoP`      |
|  [06]   | `CryptoKeyPair`                      | DPoP key material | `randomDPoPKeyPair`; `cnf.jkt` = `Material.thumbprintUri` |

- [01]-[CLIENT_AUTH]: `ClientSecretPost`, `ClientSecretBasic`, `ClientSecretJwt`, `PrivateKeyJwt`, `TlsClientAuth`, `None` — each returns `ClientAuth`, chosen once at config time.

[PUBLIC_TYPE_SCOPE]: the fault surface — every network member rejects into this set — consumer `authn/workload`
- rail: rails-and-effects

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]   | [CONSUMER]                                             |
| :-----: | :------------------------------------ | :-------------- | :----------------------------------------------------- |
|  [01]   | `ClientError` (`code`, `cause`)       | client fault    | request/validation refusal minted by the module itself |
|  [02]   | `oauth.ResponseBodyError` (`error`)   | protocol fault  | RFC 6749 token-endpoint error body; `error` the arm    |
|  [03]   | `oauth.AuthorizationResponseError`    | callback fault  | error params on the authorization-response leg         |
|  [04]   | `oauth.WWWAuthenticateChallengeError` | challenge fault | protected-resource 401 with a DPoP `WWW-Authenticate`  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resolving the RP configuration — one `Configuration` seeds every downstream grant; consumer `authn/workload`
- rail: surfaces-and-dispatch
- `discovery` fetches the issuer well-known metadata; the `Configuration` constructor is the offline path when metadata is pinned; `dynamicClientRegistration` mints a client at the AS (RFC 7591). Client-auth strategy is chosen once at config time from the [02] `ClientAuth` vocabulary.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [CONSUMER]                                          |
| :-----: | :----------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `discovery(server, clientId, metadata?, …)`      | resolve config | issuer well-known fetch → `Configuration`; default  |
|  [02]   | `new Configuration(serverMeta, clientId, …)`     | pin config     | offline construction from pinned metadata           |
|  [03]   | `dynamicClientRegistration(server, metadata, …)` | register       | RFC 7591 self-registration → `Configuration`        |
|  [04]   | config toggles                                   | policy set     | non-repudiation / JWT-response-mode / decrypt; [04] |

[ENTRYPOINT_SCOPE]: the machine-grant rows — each is `grant(config, …)`; consumer `authn/workload`
- rail: surfaces-and-dispatch
- Token-producing calls return `Promise<oauth.TokenEndpointResponse & TokenEndpointResponseHelpers>`; device and CIBA initiation return their protocol responses, and their polling calls return token responses. Trailing `options?: DPoPOptions` carries the `DPoPHandle` that sender-constrains an issued token. Token exchange rides `genericGrantRequest` with the token-exchange grant type.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [CONSUMER]                                          |
| :-----: | :----------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `clientCredentialsGrant(config, params?)`        | machine token  | service/sidecar/headless principal; the default     |
|  [02]   | `genericGrantRequest(config, grantType, params)` | delegation     | `urn:…:grant-type:token-exchange` — RFC 8693        |
|  [03]   | `refreshTokenGrant(config, refreshToken)`        | token refresh  | rotation leg; DPoP-bound refresh stays constrained  |
|  [04]   | `authorizationCodeGrant(config, currentUrl, …)`  | code exchange  | first-party OIDC callback leg outside arctic's rows |
|  [05]   | device pair                                      | device flow    | input-constrained principal; methods at [01] below  |
|  [06]   | CIBA pair                                        | backchannel    | decoupled approval; methods at [02] below           |

- [01]-[DEVICE]: `initiateDeviceAuthorization(config, params): Promise<oauth.DeviceAuthorizationResponse>` initiates; `pollDeviceAuthorizationGrant(config, response, params?, opts?)` produces the token response.
- [02]-[CIBA]: `initiateBackchannelAuthentication(config, params): Promise<oauth.BackchannelAuthenticationResponse>` initiates; `pollBackchannelAuthenticationGrant(config, response, params?, opts?)` produces the token response.

[ENTRYPOINT_SCOPE]: DPoP mint, PKCE/state mint, and the authorization/end-session URL builders; consumer `authn/workload`
- rail: surfaces-and-dispatch

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CONSUMER]                                         |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `randomDPoPKeyPair(alg?, opts?)` → `CryptoKeyPair`     | DPoP key mint  | one pair per principal; `cnf.jkt` = thumbprint     |
|  [02]   | `getDPoPHandle(config, keyPair, opts?)` → `DPoPHandle` | DPoP handle    | passed `options.DPoP`; owns server-nonce retry     |
|  [03]   | PKCE/nonce/state mint                                  | request secret | `randomPKCECodeVerifier`/`randomState`; [03] below |
|  [04]   | `buildAuthorizationUrl(config, params)` → `URL`        | build redirect | first-party code leg; PAR/JAR at [03] below        |
|  [05]   | `buildEndSessionUrl(config, params?)` → `URL`          | RP-logout      | OIDC RP-initiated logout redirect                  |

- [03]-[REQUEST_HELPERS]: `randomPKCECodeVerifier()`/`calculatePKCECodeChallenge(v)`/`randomState()`/`randomNonce()` mint request secrets; `buildAuthorizationUrlWithPAR(config, params, opts?)` → `Promise<URL>` (RFC 9126 pushed request) and `buildAuthorizationUrlWithJAR(config, params, …)` (RFC 9101 signed request object) harden the request when the AS demands it.

[ENTRYPOINT_SCOPE]: reading the protected resource and the token lifecycle; consumer `authn/workload`
- rail: boundaries

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CONSUMER]                                          |
| :-----: | :------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `fetchProtectedResource(config, token, url, method, body?, …)` | resource call  | DPoP-bound RS call; `options.DPoP` proves each call |
|  [02]   | `fetchUserInfo(config, token, expectedSubject, opts?)`         | userinfo       | → identity claims; `expectedSubject` guards `sub`   |
|  [03]   | `tokenIntrospection(config, token, params?)`                   | introspect     | RFC 7662 active-check → `IntrospectionResponse`     |
|  [04]   | `tokenRevocation(config, token, params?)`                      | revoke         | RFC 7009 sign-out leg for the machine principal     |

## [04]-[IMPLEMENTATION_LAW]

[OPENID_CLIENT_TOPOLOGY]:
- One `Configuration` is the whole state; every grant, resource call, and URL builder is a free function taking it first. A single `Configuration` per issuer threads through a `Layer`, and `Match` over a closed `_kinds` tuple — `client-credentials`, `token-exchange`, `refresh`, `device`, `ciba` — dispatches the grant row; never a client subclass per grant.
- Client authentication is chosen once at config time from the closed [02] `ClientAuth` vocabulary; a private-key strategy (`PrivateKeyJwt`) reads the same `Material` key custody the folder owns, never a second key store. `discovery` is the default config path; the offline `Configuration` constructor is used only when issuer metadata is pinned to drop a network round-trip.
- Token exchange is not a named member — it is `genericGrantRequest(config, 'urn:ietf:params:oauth:grant-type:token-exchange', params, options)`; the grant-type string is a folder constant, and `subject_token`/`actor_token`/`requested_token_type` ride `params`.
- DPoP is per-principal: `randomDPoPKeyPair` mints the key pair, `getDPoPHandle` wraps it into a nonce-tracking handle passed as `options.DPoP` to every grant and `fetchProtectedResource`. Its `cnf.jkt` thumbprint is the folder's own `Material.thumbprintUri`, so DPoP binding and JWT identity share one key authority. A sender-constrained token surfaces as `token_type: "dpop"`.
- `TokenEndpointResponse` index-signature fields and `fetchUserInfo` claims are ingress boundaries, not trusted values; `.claims()` returns the parsed `IDToken` and a `Schema` decodes both before any field reaches the session.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): `Effect.tryPromise({ try: () => clientCredentialsGrant(config, params, { DPoP }), catch })` lifts each grant onto the rail; `Match.value(err)` over `ClientError.code` and `oauth.ResponseBodyError.error` folds the fault surface to one tagged domain error; `Schema.decodeUnknown` parses the `TokenEndpointResponse` and the `.claims()` `IDToken` into branded claims; `Redacted` holds `client_secret`, the DPoP private key, and every returned token; `Config.redacted` sources credentials at `Layer` construction; the grant roster is a `Data.taggedEnum`/`Match` vocabulary.
- `@effect/platform` (`.api/effect-platform.md`): the device and CIBA polling loops run as `Effect.repeat` over a `Schedule`, and `fetchProtectedResource` is invoked from an `HttpApiEndpoint` handler; `Headers.redact` keeps `DPoP`/`Authorization` proof headers out of logs. openid-client owns its own `fetch` (override via the `customFetch` symbol), so it is not routed through `HttpClient` — the boundary is the handler.
- `jose` (`.api/jose.md`): the folder's `Jwt` owner mints and verifies the DPoP proof and the `cnf.jkt` binding via `jose`; `fetchUserInfo`/`authorizationCodeGrant` id-token claims re-verify through `createRemoteJWKSet` against the discovered `jwks_uri`, never trusted from the helper alone. openid-client and `jose` share the one WebCrypto key path.
- `arctic` (`.api/arctic.md`): split custody — arctic owns the interactive browser authorization-code redirect and its provider rows; openid-client owns `discovery`-driven RP config, the machine grants, DPoP, PAR/JAR, introspection/revocation, and userinfo. A first-party OIDC code flow needing DPoP or PAR uses `authorizationCodeGrant` here; a social-login redirect uses arctic. Neither reimplements the other.
- `security/authn/workload` (in-folder): the resolved machine principal projects into the per-call transport credential (gRPC metadata, NATS auth-callout header) the runtime wave mounts; `expiresIn()` drives the refresh-rotation law; `tokenRevocation` fires on principal teardown.

[LOCAL_ADMISSION]:
- Thread one `Configuration` per issuer and dispatch the grant by `Match` over the closed `_kinds` tuple; never a client subclass per grant, and never a `get*Grant` method family.
- Reach token exchange only through `genericGrantRequest` with the RFC 8693 grant-type constant; never hand-build the token-endpoint body a grant row already assembles.
- Mint one DPoP key pair per principal via `randomDPoPKeyPair`, bind `cnf.jkt` to `Material.thumbprintUri`, and pass the `getDPoPHandle` result as `options.DPoP` on every grant and resource call; never a second key custody, never an unbound bearer machine token where the AS advertises DPoP.
- Choose `ClientAuth` once at config time from the closed vocabulary; never pass `client_secret` per call, and hold it, every token, and the DPoP private key in `Redacted`.
- Decode `TokenEndpointResponse` and `.claims()` through `Schema`, and re-verify id-token claims through `jose`; never treat a helper return as trusted before the decode.
- Wrap every network member in `Effect.tryPromise` and fold `ClientError` + the `oauth` fault family with `Match`; never let a reject escape as a bare `Promise` rejection into domain code.

[RAIL_LAW]:
- Package: `openid-client`
- Owns: OIDC RP configuration (`discovery`, `Configuration`, `dynamicClientRegistration`), the client-auth vocabulary, the machine-grant rows (`clientCredentialsGrant`, token exchange via `genericGrantRequest`, `refreshTokenGrant`, device, CIBA, first-party `authorizationCodeGrant`), DPoP sender-constraint (`randomDPoPKeyPair`/`getDPoPHandle`/`DPoPHandle`), PKCE/state/nonce mint, the authorization/PAR/JAR/end-session URL builders, protected-resource + userinfo reads, and token introspection/revocation
- Accept: one `Configuration` per issuer dispatched as a `Match`/`Data.taggedEnum` grant vocabulary, `Effect.tryPromise` around every network member, `Match` over `ClientError` + the `oauth` fault family, `Schema` decode of `TokenEndpointResponse`/`.claims()`, `jose` re-verification of id-token claims, `Redacted` secrets and DPoP keys, a per-principal DPoP handle bound to `Material.thumbprintUri`
- Reject: a client subclass or method family per grant, a hand-built token-endpoint body, a bearer machine token where DPoP is advertised, a second key custody for the DPoP proof, `client_secret` passed per call or outside `Redacted`, a helper return trusted before `Schema`/`jose`, a bare `Promise` reject in domain logic, any use of the `openid-client/passport` adapter
