# [TS_SECURITY_API_ARCTIC]

`arctic` owns the browser OAuth 2.0 authorization-code ceremony `security/authn/oauth` composes: one `OAuth2Client` with a pre-bound class per issuer, every class carrying the same four-method shape, so an issuer is a vocabulary row rather than a method family.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arctic`
- package: `arctic` (MIT)
- module: ESM-only (`type: module`); one root barrel carries the whole surface with no subpath export contract
- runtime: isomorphic (node, bun, browser, worker, edge) — global `fetch` with WebCrypto `crypto.getRandomValues`; `@oslojs/crypto`, `@oslojs/encoding`, and `@oslojs/jwt` carry the digest, codec, and JWT-decode legs; no native addon
- asset: pure-TypeScript library (`.js` + `.d.ts`); `OAuth2Tokens.data` stays an untyped `object`
- rail: `security/authn` — OAuth ceremony provider rows, admitted in `authn/` only

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client, the issuer vocabulary, and the token accessor

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `OAuth2Client`        | class         | endpoint-parameterized client for an unlisted issuer |
|  [02]   | `<Provider>`          | class         | one pre-bound issuer class; ctor identity varies     |
|  [03]   | `OAuth2Tokens`        | class         | accessor over the untyped `.data` response           |
|  [04]   | `CodeChallengeMethod` | enum          | `S256` and `Plain`; `S256` is the admitted arm       |

[PUBLIC_TYPE_SCOPE]: the ceremony fault family — a 400/401 carrying a parseable OAuth body mints `OAuth2RequestError`, every other non-200 mints `UnexpectedResponseError`

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :--------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `OAuth2RequestError`               | class         | RFC 6749 body; `code` is the `Match` discriminant |
|  [02]   | `ArcticFetchError`                 | class         | `fetch` itself threw; retryable transport fault   |
|  [03]   | `UnexpectedResponseError`          | class         | off-protocol status; `status` carries telemetry   |
|  [04]   | `UnexpectedErrorResponseBodyError` | class         | unrecognized body; `data` stays quarantined       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the ceremony — one shape across every issuer row

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `new OAuth2Client(string, string\|null, string\|null)`                     | ctor     | recovery client; each call takes its endpoint |
|  [02]   | `new <Provider>(string, string, string)`                                   | ctor     | pre-bound row; `Apple` takes key material     |
|  [03]   | `createAuthorizationURL(string, string[]) -> URL`                          | instance | redirect build; a PKCE row adds the verifier  |
|  [04]   | `validateAuthorizationCode(string, string\|null) -> Promise<OAuth2Tokens>` | instance | the callback exchange leg                     |
|  [05]   | `refreshAccessToken(string, string[]) -> Promise<OAuth2Tokens>`            | instance | rotation on rows that carry it                |
|  [06]   | `revokeToken(string) -> Promise<void>`                                     | instance | sign-out on rows that carry it                |

- `OAuth2Client.createAuthorizationURLWithPKCE(string, string, CodeChallengeMethod, string, string[]) -> URL` builds the PKCE redirect an issuer row folds in itself.
- [PKCE_MINT]: `generateState()` `generateCodeVerifier()` `createS256CodeChallenge(verifier)` — minted on the authorization leg, both required on the callback.

[ENTRYPOINT_SCOPE]: reading the token response — every member reads `OAuth2Tokens` except `decodeIdToken`

- [ACCESS]: `accessToken()` `tokenType()` `accessTokenExpiresInSeconds()` `refreshToken()` `scopes()` — each throws a bare `Error` on a missing or ill-typed field, outside the ceremony fault family.
- [GUARD]: `hasRefreshToken()` `hasScopes()` — presence reads gating their accessor; `hasRefreshToken` selects the refresh-rotation row.

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :-------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `accessTokenExpiresAt() -> Date`  | instance | adds `expires_in` to the read-time clock; read at mint |
|  [02]   | `idToken() -> string`             | instance | raw token handed to `jose.jwtVerify`                   |
|  [03]   | `decodeIdToken(string) -> object` | static   | claim peek, no signature check; throws on malformed    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every issuer class binds endpoints over one ceremony — `createAuthorizationURL`, redirect, `validateAuthorizationCode`, `OAuth2Tokens` — so the design carries an issuer as a vocabulary row `{ ctor, hasPKCE, hasRefresh, hasRevoke }` dispatched by `Match`, reaching `OAuth2Client` only for an issuer no row covers.
- `state` and the PKCE `codeVerifier` are per-request secrets minted on the authorization leg and required on the callback; a short-lived `session`-owned store keyed by `state` holds both across the two legs, out of client-readable reach.
- `OAuth2Tokens.data` and the `decodeIdToken` return are ingress boundaries: a `Schema` decodes the token response and `jose` verifies the id token before any field reaches the session, so the peek routes by claim ahead of verification and never stands in for it.

[STACKING]:
- `effect` (`.api/effect.md`): `Effect.tryPromise` lifts the three network legs; `Match.value` over `OAuth2RequestError.code` with the three shape faults folds the family to one tagged domain error; `Schema.decodeUnknown` parses `OAuth2Tokens.data` and the `decodeIdToken` peek into branded claims; `Config.redacted` sources issuer credentials at `Layer` construction, and `Redacted` carries `clientSecret` with every issued token.
- `@effect/platform` (`.api/effect-platform.md`): the authorization and callback legs are `HttpApiEndpoint` handlers — one returns the `createAuthorizationURL` result, the other reads the `code` and `state` url-params into `validateAuthorizationCode`; `Headers.redact` keeps `Set-Cookie` and `Authorization` out of logs. arctic owns its own `fetch`, so `HttpClient` never carries this transport.
- `jose` (`.api/jose.md`): `idToken()` verifies through `jose.jwtVerify(token, createRemoteJWKSet(jwksUri), { issuer, audience })`, and that verified claim set seeds identity.
- `openid-client` (`.api/openid-client.md`): split custody — a first-party OIDC code leg needing DPoP (`getDPoPHandle`) or a pushed request (`buildAuthorizationUrlWithPAR`) runs `authorizationCodeGrant` there, and `clientCredentialsGrant`/`genericGrantRequest` own every machine grant this ceremony has no row for.
- `@oslojs/crypto` (`.api/oslojs-crypto.md`) with `@oslojs/encoding` (`.api/oslojs-encoding.md`): `createS256CodeChallenge` folds `sha2.sha256` into `encodeBase64urlNoPadding`, so the PKCE challenge rides the digest and codec owners `sign/` already admits.
- `security/session/token` (in-folder): verified tokens mint a session, `hasRefreshToken` and `accessTokenExpiresAt` drive refresh rotation, and `revokeToken` fires on sign-out for rows that carry it.

[LOCAL_ADMISSION]:
- Dispatch issuer rows by `Match` over one vocabulary, and let the row build every authorization URL.
- Mint `generateState` and `generateCodeVerifier` per request, require both on the callback, and retire the verifier after one exchange.
- Lift the three network legs with `Effect.tryPromise` and fold the fault family with `Match`; an accessor read passes its `has*` guard or a `Schema` decode first.
- `clientSecret` and every issued token ride `Redacted`.

[RAIL_LAW]:
- Package: `arctic`
- Owns: the browser authorization-code ceremony (`OAuth2Client` and the pre-bound issuer rows), PKCE mint, token exchange, refresh and revocation, the `OAuth2Tokens` accessor, the ceremony fault family, and the unsafe `decodeIdToken` peek
- Accept: issuer rows bound as a `Match` vocabulary, `Effect.tryPromise` around the network legs, `Match` over the fault family, `Schema` decode of `OAuth2Tokens.data`, `jose` verification of `idToken()`, `Redacted` secrets, per-request `state` and `codeVerifier` in a short-lived store
- Reject: a method family per provider, a hand-built authorization URL, `decodeIdToken` used as verification, a bare `Promise` reject in domain logic, a reused verifier, a machine or DPoP-bound grant this ceremony carries no row for
