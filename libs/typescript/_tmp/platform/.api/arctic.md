# [API_CATALOGUE] arctic

`arctic` supplies a provider-per-class OAuth 2.0 / OIDC public-client layer for the browser auth-session boot edge: each of the 64 provider classes owns authorization-URL construction (with optional PKCE), authorization-code exchange, token refresh, and revocation, over one uniform shape whose four methods and constructor-identity tuple vary per provider on a fixed axis set. The generic `OAuth2Client` is the escape hatch when no built-in provider matches; `OAuth2Tokens`, `generateState`, `generateCodeVerifier`, `createS256CodeChallenge`, and `decodeIdToken` are the shared flow utilities; a four-class error family separates protocol errors from transport and parse failures. The whole surface is `Promise`-returning and lifts through `Effect.tryPromise` into the closed `AuthFault` the `Session/session.md` fold dispatches; the flow itself is a two-half redirect round-trip over one persisted `{ state, codeVerifier }` record the in-memory cell cannot survive.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arctic`
- package: `arctic`
- version: `3.7.0`
- license: `MIT`
- module: barrel `arctic` (`dist/index.d.ts`) — re-exports all 64 provider classes plus the client, tokens, error, and OIDC surface; no `exports` map, so deep `arctic/dist/*` subpaths resolve but the barrel is complete
- asset: pure ESM (`"type": "module"`) — a CJS `require("arctic")` fails; zero dependencies; runs on the Web Crypto / `fetch` platform globals (no Node built-ins), so it is browser-native; no `engines` floor
- rail: auth — OAuth 2.0 authorization-code + PKCE, OIDC id-token decode

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core client, tokens, and flow utilities
- rail: auth

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                                      |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `OAuth2Client`          | class         | generic low-level client for a provider not built-in                |
|  [02]   | `CodeChallengeMethod`   | numeric enum  | `S256 = 0`, `Plain = 1` — the PKCE challenge method for `OAuth2Client` |
|  [03]   | `OAuth2Tokens`          | class         | token-response wrapper; typed accessors over the raw `data: object`   |
|  [04]   | `generateState()`       | fn            | cryptographically random CSRF `state`                               |
|  [05]   | `generateCodeVerifier()`| fn            | cryptographically random PKCE verifier                              |
|  [06]   | `createS256CodeChallenge(verifier)` | fn (deep `arctic/oauth2`) | S256 challenge from a verifier — the `OAuth2Client` PKCE derivation; provider classes derive it internally |
|  [07]   | `decodeIdToken(idToken)`| fn            | returns `object` — the caller validates the OIDC claims against its own `Schema` |

[PUBLIC_TYPE_SCOPE]: error family — protocol vs transport vs parse
- rail: auth
- All four are plain `extends Error` read by `instanceof`; the fold maps each to a closed `AuthFault` `Data.TaggedEnum` case, never a flat `reason` string.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                                       |
| :-----: | :--------------------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `OAuth2RequestError`               | error class   | RFC 6749 protocol error: `code`, `description\|null`, `uri\|null`, `state\|null` |
|  [02]   | `ArcticFetchError`                 | error class   | network-level `fetch` failure wrapping `cause: unknown`              |
|  [03]   | `UnexpectedResponseError`          | error class   | non-JSON / non-error HTTP response: `status: number`                 |
|  [04]   | `UnexpectedErrorResponseBodyError` | error class   | parseable error body with bad shape: `status`, `data: unknown`       |

[PUBLIC_TYPE_SCOPE]: provider classes — one parameterized shape, 64 instances
- rail: auth
- Every provider is a class re-exported from the barrel; the roster is seed data, never the mechanism. A provider is not a new surface to document one-by-one — it is one row selecting from four method-presence axes plus a constructor-identity variance. Reach for `OAuth2Client` only when a needed provider is absent from the 64.

| [INDEX] | [AXIS]                     | [VARIANCE]                                                                                     |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | constructor identity       | `(clientId, clientSecret, redirectURI)` baseline; `clientSecret: string \| null` = public vs confidential client; `redirectURI` may be `\| null`; extras prepend/replace: `MicrosoftEntraId(tenant, …)`, `Apple(clientId, teamId, keyId, pkcs8PrivateKey, redirectURI)` |
|  [02]   | `createAuthorizationURL`   | `(state, scopes)` no-PKCE, or `(state, codeVerifier, scopes)` PKCE; `codeVerifier: string \| null` where PKCE is optional |
|  [03]   | `validateAuthorizationCode`| `(code)` no-PKCE, or `(code, codeVerifier)` PKCE → `Promise<OAuth2Tokens>`                     |
|  [04]   | `refreshAccessToken`       | present or absent; `(refreshToken)` or `(refreshToken, scopes)` (scoped-refresh, e.g. `MicrosoftEntraId`) → `Promise<OAuth2Tokens>` |
|  [05]   | `revokeToken`              | present or absent; `(token) => Promise<void>`                                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: OAuth2Client — generic low-level escape hatch
- rail: auth
- The only class taking explicit endpoint URLs per call; a built-in provider bakes its endpoints in and drops the endpoint parameters.

```ts
// dist/client.d.ts
export declare class OAuth2Client {
  clientId: string;
  constructor(clientId: string, clientPassword: string | null, redirectURI: string | null);
  createAuthorizationURL(authorizationEndpoint: string, state: string, scopes: string[]): URL;
  createAuthorizationURLWithPKCE(
    authorizationEndpoint: string, state: string,
    codeChallengeMethod: CodeChallengeMethod, codeVerifier: string, scopes: string[],
  ): URL;
  validateAuthorizationCode(tokenEndpoint: string, code: string, codeVerifier: string | null): Promise<OAuth2Tokens>;
  refreshAccessToken(tokenEndpoint: string, refreshToken: string, scopes: string[]): Promise<OAuth2Tokens>;
  revokeToken(tokenRevocationEndpoint: string, token: string): Promise<void>;
}
export declare enum CodeChallengeMethod { S256 = 0, Plain = 1 }
```

[ENTRYPOINT_SCOPE]: OAuth2Tokens — typed token-response accessors
- rail: auth
- Field access goes through the typed accessors, never the raw `data`. `hasRefreshToken`/`hasScopes` guard the optional fields; `accessTokenExpiresAt` is the refresh-schedule input.

```ts
// dist/oauth2.d.ts
export declare class OAuth2Tokens {
  data: object;
  constructor(data: object);
  tokenType(): string;
  accessToken(): string;
  accessTokenExpiresInSeconds(): number;
  accessTokenExpiresAt(): Date;
  hasRefreshToken(): boolean;
  refreshToken(): string;
  hasScopes(): boolean;
  scopes(): string[];
  idToken(): string;
}
export declare function generateCodeVerifier(): string;
export declare function generateState(): string;
export declare function createS256CodeChallenge(codeVerifier: string): string;
```

[ENTRYPOINT_SCOPE]: provider-class shape — the five shape-defining samples
- rail: auth
- These five span the axis space: no-PKCE/refresh (`GitHub`), PKCE + refresh + revoke + OIDC (`Google`), tenant + scoped-refresh (`MicrosoftEntraId`), keypair-minted client secret with no refresh (`Apple`), and the AEC-relevant public-capable PKCE + revoke (`Autodesk` — Autodesk Platform Services). `Autodesk` is the provider a BIM/AEC host reaches for first; `Google`/`GitHub`/`MicrosoftEntraId` are the mainstream IdPs; the other 59 (Discord, Spotify, Twitter, LinkedIn, Slack, Okta, Auth0, Keycloak, …) instantiate the same shape.

```ts
// dist/providers/github.d.ts — confidential client, no PKCE, refresh, no revoke
export declare class GitHub {
  constructor(clientId: string, clientSecret: string, redirectURI: string | null);
  createAuthorizationURL(state: string, scopes: string[]): URL;
  validateAuthorizationCode(code: string): Promise<OAuth2Tokens>;
  refreshAccessToken(refreshToken: string): Promise<OAuth2Tokens>;
}

// dist/providers/google.d.ts — PKCE, refresh, revoke, OIDC id-token
export declare class Google {
  constructor(clientId: string, clientSecret: string, redirectURI: string);
  createAuthorizationURL(state: string, codeVerifier: string, scopes: string[]): URL;
  validateAuthorizationCode(code: string, codeVerifier: string): Promise<OAuth2Tokens>;
  refreshAccessToken(refreshToken: string): Promise<OAuth2Tokens>;
  revokeToken(token: string): Promise<void>;
}

// dist/providers/microsoft-entra-id.d.ts — tenant prefix, PKCE, scoped refresh
export declare class MicrosoftEntraId {
  constructor(tenant: string, clientId: string, clientSecret: string | null, redirectURI: string);
  createAuthorizationURL(state: string, codeVerifier: string, scopes: string[]): URL;
  validateAuthorizationCode(code: string, codeVerifier: string): Promise<OAuth2Tokens>;
  refreshAccessToken(refreshToken: string, scopes: string[]): Promise<OAuth2Tokens>;
}

// dist/providers/apple.d.ts — PKCS8 keypair mints a signed client-secret JWT internally; no refresh
export declare class Apple {
  constructor(clientId: string, teamId: string, keyId: string, pkcs8PrivateKey: Uint8Array, redirectURI: string);
  createAuthorizationURL(state: string, scopes: string[]): URL;
  validateAuthorizationCode(code: string): Promise<OAuth2Tokens>;
}

// dist/providers/autodesk.d.ts — AEC (Autodesk Platform Services); public-capable, PKCE, refresh, revoke
export declare class Autodesk {
  constructor(clientId: string, clientSecret: string | null, redirectURI: string);
  createAuthorizationURL(state: string, codeVerifier: string, scopes: string[]): URL;
  validateAuthorizationCode(code: string, codeVerifier: string): Promise<OAuth2Tokens>;
  refreshAccessToken(refreshToken: string): Promise<OAuth2Tokens>;
  revokeToken(token: string): Promise<void>;
}
```

[ENTRYPOINT_SCOPE]: OIDC and error types
- rail: auth

```ts
// dist/oidc.d.ts
export declare function decodeIdToken(idToken: string): object; // claims validated against a Schema, never trusted raw

// dist/request.d.ts — the four-class error family
export declare class OAuth2RequestError extends Error {
  code: string; description: string | null; uri: string | null; state: string | null;
  constructor(code: string, description: string | null, uri: string | null, state: string | null);
}
export declare class ArcticFetchError extends Error { constructor(cause: unknown); }
export declare class UnexpectedResponseError extends Error { status: number; constructor(responseStatus: number); }
export declare class UnexpectedErrorResponseBodyError extends Error { status: number; data: unknown; constructor(status: number, data: unknown); }
```

## [04]-[IMPLEMENTATION_LAW]

[AUTH_TOPOLOGY]:
- one class per provider, all barrel-re-exported; each is one row over the five axes above — provider growth is a row, never a new surface. `OAuth2Client` is the escape hatch for an absent provider and delegates to the same token-request internals.
- the PKCE axis is provider-fixed: PKCE providers take `codeVerifier` in both `createAuthorizationURL` and `validateAuthorizationCode`; no-PKCE providers omit it; optional-PKCE providers accept `string | null`. `OAuth2Client` externalizes the choice via `createAuthorizationURL` (no PKCE) vs `createAuthorizationURLWithPKCE` + `CodeChallengeMethod.S256`.
- `Apple` is the identity outlier: no shared secret — a PKCS8 private key mints a short-lived signed client-secret JWT internally; the caller supplies `teamId`/`keyId`/`pkcs8PrivateKey`.
- token responses are wrapped in `OAuth2Tokens`; field access goes through the typed accessors, guarded by `hasRefreshToken`/`hasScopes`. `decodeIdToken` returns `object` — the caller validates the claim set, never trusts it raw.

[INTEGRATION_LAW]:
- Stack with `effect` boundary lift (`.api/effect.md`): every provider method is `Promise`-returning, so each crosses through `Effect.tryPromise({ try, catch })` where `catch` narrows the thrown value by `instanceof` into the closed `AuthFault` `Data.TaggedEnum` (`Denied`/`Expired`/`RefreshFailed`/`ProtocolError`) the `Session/session.md` `faultOf` fold owns — `OAuth2RequestError` → `ProtocolError({ code })`, the transport/parse classes (`ArcticFetchError`/`UnexpectedResponseError`/`UnexpectedErrorResponseBodyError`) → the acquisition-failure case `RefreshFailed()` (a dedicated transport case landing as one growth row, never a parallel fault owner) — dispatched under `Match.tagsExhaustive`, never a bare rejected `Promise` or a flat `reason`.
- Stack with `effect` `Redacted` (`.api/effect.md`): the PKCE `codeVerifier` and any confidential `clientSecret` are held as `Redacted<string>` (never logged); `Redacted.value` unwraps only at the provider-method call boundary.
- REDIRECT-CONTINUITY ROUND-TRIP (the leg `OAuth2Client` PKCE acquisition + refresh alone leaves unexploited): the flow spans a full-page IdP redirect the in-memory cell cannot survive, so it is two halves over one persisted `{ state, codeVerifier, returnTo, nonce }` record. MINT-PERSIST-REDIRECT — `generateState`/`generateCodeVerifier` mint the CSRF `state` and PKCE verifier; the record persists into the `Session/store` `auth-flow` named store (the `codeVerifier` a `Redacted`, single-entry, cleared on completion); `provider.createAuthorizationURL(state, codeVerifier, scopes)` builds the URL the browser navigates to. EXTRACT-MATCH-VALIDATE-DECODE — the callback reads the persisted record back, matches the returned `state` against it as the CSRF check (a mismatch is `ProtocolError`), exchanges via `provider.validateAuthorizationCode(code, codeVerifier)`, and `decodeIdToken(tokens.idToken())` projects the OIDC claims (validated through a `Schema`) into the `SessionStatus.Authenticated` subject.
- Stack with the refresh/revoke schedule: `tokens.accessTokenExpiresAt()` feeds an `effect` `Schedule` that fires `refreshAccessToken(refreshToken)` before expiry; `revokeToken(token)` on logout invalidates the refresh token server-side rather than only clearing the cell.

[RAIL_LAW]:
- Package: `arctic`
- Owns: OAuth 2.0 authorization-URL construction, code exchange, refresh, revocation, PKCE + CSRF helpers, and OIDC id-token decode, over 64 provider classes and one generic client
- Accept: one provider class per provider (`Autodesk` for AEC, `Google`/`GitHub`/`MicrosoftEntraId` for the mainstream IdPs); `OAuth2Client` for a custom endpoint; every method lifted through `Effect.tryPromise` into `AuthFault`; the `codeVerifier` persisted `Redacted` across the redirect
- Reject: enumerating providers as parallel surfaces instead of the one axis-parameterized shape; hand-rolled `fetch` for token exchange; raw JSON token parsing outside `OAuth2Tokens`; a bare rejected `Promise` escaping the `AuthFault` rail; `require("arctic")` (ESM-only)
