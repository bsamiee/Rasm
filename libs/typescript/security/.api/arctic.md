# [TS_SECURITY_API_ARCTIC]

`arctic` is the OAuth 2.0 authorization-code client the `security/authn/oauth` design composes: one generic `OAuth2Client` plus 60-plus pre-bound provider classes that all share the same four-method ceremony shape — `createAuthorizationURL` (build the redirect), `validateAuthorizationCode` (exchange the code for `OAuth2Tokens`), optional `refreshAccessToken`, optional `revokeToken`. The provider roster is seed DATA over that shape, differing only in endpoint URLs, constructor identity (`clientSecret` vs Apple's `teamId`/`keyId`/`pkcs8PrivateKey` vs Entra's `tenant`), and whether the flow carries a PKCE `codeVerifier` or a `refresh_token`; the design binds it as a closed provider vocabulary, never a method family per provider. It is isomorphic (WebCrypto + global `fetch`, zero runtime dependencies), every ceremony call is a `Promise` that rejects with the typed request-fault family, and `decodeIdToken` is unsafe decode-only — OIDC id-token verification delegates to `jose` (`.api/jose.md`). PKCE state and code-verifier are per-request secrets the design parks in a short-lived `session` store across the two redirect legs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arctic`
- package: `arctic` (MIT, © pilcrowOnPaper)
- module format: ESM only (`type: module`); deep-import subpaths ship under `arctic/dist/*` but the flat `arctic` barrel is the import surface — provider classes, `OAuth2Client`, `OAuth2Tokens`, the fault family, and `decodeIdToken` all re-export from the root
- runtime target: isomorphic (node, bun, browser, worker, edge) — global `fetch` + WebCrypto (`generateCodeVerifier`/`generateState` draw from `crypto.getRandomValues`); zero runtime dependencies; no native addon
- asset: pure-TypeScript runtime library (`.js` + `.d.ts`); `OAuth2Tokens.data` is an untyped `object`, so a `Schema` at the seam is the real gate on the provider response shape
- rail: `security/authn` — OAuth ceremony provider rows (admitted in `authn/` only; catalogued at the folder tier)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the parameterized client, the provider vocabulary, and the token accessor — consumer is `authn/oauth`
- rail: surfaces-and-dispatch

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]    | [CONSUMER]                                                       |
| :-----: | :------------------------------------------ | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `OAuth2Client`                              | generic client   | the one authorization-code owner; base for an unlisted provider  |
|  [02]   | `Google` / `GitHub` / `Apple` / … (60-plus) | provider row     | pre-bound endpoint rows; a closed vocabulary, seed data          |
|  [03]   | `OAuth2Tokens`                              | token response   | → `session/token`; `.data` untyped, `Schema`-decoded first       |
|  [04]   | `CodeChallengeMethod` (`S256`, `Plain`)     | PKCE method enum | `S256` the only admitted arm on `createAuthorizationURLWithPKCE` |

[PUBLIC_TYPE_SCOPE]: the ceremony fault family (each ceremony call rejects into this closed set) — consumer `authn/oauth`
- rail: rails-and-effects

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY]   | [CONSUMER]                                             |
| :-----: | :----------------------------------------------------------- | :-------------- | :----------------------------------------------------- |
|  [01]   | `OAuth2RequestError` (`code`, `description`, `uri`, `state`) | provider fault  | RFC 6749 token error; `code` the `Match` discriminant  |
|  [02]   | `ArcticFetchError`                                           | transport fault | the `fetch` call itself threw (network/DNS); retryable |
|  [03]   | `UnexpectedResponseError` (`status`)                         | shape fault     | non-2xx, no parseable OAuth body; `status` telemetry   |
|  [04]   | `UnexpectedErrorResponseBodyError` (`status`, `data`)        | shape fault     | error status, unrecognized body; `data` quarantined    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the authorization-code ceremony — one shape across every provider row; consumer `authn/oauth`
- rail: surfaces-and-dispatch
- The three `fetch` legs return `Promise<OAuth2Tokens>`; `OAuth2Client.createAuthorizationURLWithPKCE(endpoint, state, CodeChallengeMethod.S256, codeVerifier, scopes)` builds the generic PKCE redirect, and the PKCE-mint trio is at [04] below.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CONSUMER]                                         |
| :-----: | :------------------------------------------------------------ | :------------- | :------------------------------------------------- |
|  [01]   | `new OAuth2Client(clientId, secret\|null, redirectURI\|null)` | construct      | the recovery client for an unlisted issuer         |
|  [02]   | `new <Provider>(clientId, clientSecret, redirectURI)`         | construct      | the provider row — pre-bound ctor identity         |
|  [03]   | `.createAuthorizationURL(...)` → HTTP 302                     | build redirect | folds `state`/`codeVerifier`/`scopes` into the URL |
|  [04]   | PKCE-mint trio                                                | PKCE mint      | `state` + verifier per request; at [04] below      |
|  [05]   | `.validateAuthorizationCode(code, codeVerifier\|null)`        | token exchange | callback leg; `Effect.tryPromise` → tagged fault   |
|  [06]   | `.refreshAccessToken(refreshToken, scopes?)`                  | token refresh  | refresh-rotation for rows that carry it            |
|  [07]   | `.revokeToken(token)`                                         | token revoke   | the sign-out leg for rows that carry it            |

- [04]-[PKCE_MINT]: `generateState()`, `generateCodeVerifier()`, `createS256CodeChallenge(verifier)` — minted per request on the authorization leg, required on the callback.

[ENTRYPOINT_SCOPE]: reading the token response and the id-token boundary — every read is an `OAuth2Tokens` method; consumer `authn/oauth`
- rail: boundaries

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [CONSUMER]                                                    |
| :-----: | :---------------------------------- | :-------------- | :------------------------------------------------------------ |
|  [01]   | access-grant reads                  | read access     | → `session/token`; methods at [01] below                      |
|  [02]   | grant-presence reads                | read grant      | presence-guarded; methods at [02] below                       |
|  [03]   | `OAuth2Tokens.idToken()`            | read OIDC token | raw id-token string handed to `jose.jwtVerify`, never trusted |
|  [04]   | `decodeIdToken(idToken)` → `object` | unsafe decode   | claim peek only (no signature check); NEVER verification      |

- [01]-[ACCESS]: `accessToken()`, `accessTokenExpiresAt()`, `accessTokenExpiresInSeconds()`, `tokenType()` — the access grant + its wall-clock expiry seed the session.
- [02]-[GRANT]: `hasRefreshToken()`, `refreshToken()`, `hasScopes()`, `scopes()` — presence-guarded reads; `hasRefreshToken` gates the refresh-rotation row.

## [04]-[IMPLEMENTATION_LAW]

[ARCTIC_TOPOLOGY]:
- Every provider class is a thin endpoint binding over the same ceremony: `createAuthorizationURL` → user redirect → `validateAuthorizationCode` → `OAuth2Tokens`. The variation across the 60-plus rows is data — endpoint URLs, constructor identity, PKCE presence, refresh/revoke presence — so the design models a provider as a vocabulary row `{ ctor, hasPKCE, hasRefresh, hasRevoke }` dispatched by `Match`, and reaches for the generic `OAuth2Client` only for an issuer outside the roster (self-hosted Keycloak/Authentik/Okta variants).
- The ceremony is stateless and side-effect-free until the `fetch` inside `validateAuthorizationCode`/`refreshAccessToken`/`revokeToken`. Those three are the only `Promise`-returning members and the only ones that reject; `createAuthorizationURL*` and the PKCE mints are synchronous and pure.
- `state` (CSRF) and the PKCE `codeVerifier` are per-request secrets minted on the authorization leg and required on the callback leg. Between the two legs the design holds them in a short-lived `session`-owned store keyed by `state`, never in a cookie the client can read.
- `OAuth2Tokens.data` is an untyped `object` and `decodeIdToken` returns `object` with no verification — both are ingress boundaries, not trusted values. The id-token string is verified by `jose`, and the token response is decoded by a `Schema` before any field reaches the session.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): `Effect.tryPromise({ try: () => client.validateAuthorizationCode(...), catch })` lifts the three fetch legs onto the rail; `Match.value(err)` over `OAuth2RequestError.code` and the three shape-fault constructors maps the arctic fault family to one tagged domain error; `Schema.decodeUnknown` parses `OAuth2Tokens.data` and the `decodeIdToken` peek into branded claims; `Redacted` holds `clientSecret` and the returned access/refresh tokens; `Config.redacted` sources the provider credentials at `Layer` construction; the provider roster is a `Data.taggedEnum`/`Match` vocabulary, not a switch ladder.
- `@effect/platform` (`.api/effect-platform.md`): the authorization and callback legs are `HttpApiEndpoint` handlers — the redirect endpoint returns the `createAuthorizationURL` result, the callback endpoint reads the `code`+`state` url-params and runs `validateAuthorizationCode`; `Headers.redact` keeps `Set-Cookie`/`Authorization` out of logs. Arctic owns its own `fetch`, so it is not routed through `HttpClient` — the boundary is the endpoint handler, not the transport.
- `jose` (`.api/jose.md`): `OAuth2Tokens.idToken()` is verified via `jose.jwtVerify(idToken, createRemoteJWKSet(new URL(provider.jwksUri)), { issuer, audience: clientId })` — arctic's `decodeIdToken` is decode-only and never substitutes for this. The verified claim set, not the arctic peek, seeds identity.
- `security/session/token` (in-folder): the verified `OAuth2Tokens` mint a session; `hasRefreshToken`/`refreshToken`/`accessTokenExpiresAt` drive the refresh-rotation law; `revokeToken` fires on the sign-out leg for providers whose row carries it.

[LOCAL_ADMISSION]:
- Use `OAuth2Client` + a provider vocabulary row dispatched by `Match`; never a `get*`/`refresh*` method family per provider, and never hand-roll an authorization-URL string a provider row already builds.
- Use `generateState`/`generateCodeVerifier` per request and require both on the callback; never reuse a verifier, never omit `state`, and never store either where the browser can read it.
- Verify `idToken()` through `jose`; never treat `decodeIdToken` or any `OAuth2Tokens` field as trusted before a `Schema` decode.
- Wrap the three fetch legs in `Effect.tryPromise` and fold the fault family with `Match`; never let an arctic reject escape as a bare `Promise` rejection into domain code.
- Hold `clientSecret` and every returned token in `Redacted`; unwrap only at the arctic call and the session mint.

[RAIL_LAW]:
- Package: `arctic`
- Owns: the OAuth 2.0 authorization-code ceremony (`OAuth2Client` + the 60-plus provider rows), PKCE mint (`generateState`/`generateCodeVerifier`/`createS256CodeChallenge`/`CodeChallengeMethod`), token exchange/refresh/revocation, the `OAuth2Tokens` accessor, the ceremony fault family, and the unsafe `decodeIdToken` peek
- Accept: provider rows bound as a `Match`/`Data.taggedEnum` vocabulary, `Effect.tryPromise` around the fetch legs, `Match` over the fault family, `Schema` decode of `OAuth2Tokens.data`, `jose` verification of `idToken()`, `Redacted` secrets, per-request `state`+`codeVerifier` in a short-lived store
- Reject: a method family per provider, a hand-built authorization URL, `decodeIdToken` used as verification, a bare `Promise` reject in domain logic, a reused or omitted PKCE verifier/`state`, a `clientSecret` or token outside `Redacted`
