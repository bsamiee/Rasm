# [API_CATALOGUE] arctic

`arctic` supplies a provider-per-class OAuth 2.0 and OIDC client layer: each provider class owns authorization URL construction (with optional PKCE), token exchange, token refresh, and revocation. The low-level `OAuth2Client` is the generic primitive when no built-in provider matches. `OAuth2Tokens`, `generateCodeVerifier`, `generateState`, and `decodeIdToken` are the shared flow utilities. Error types distinguish protocol errors from network and parsing failures.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arctic`
- package: `arctic`
- module: `arctic` (barrel re-export from `dist/index.d.ts`)
- asset: runtime library
- rail: auth

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core client and flow types
- rail: auth

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [RAIL]                              |
| :-----: | :--------------------------------- | :------------ | :---------------------------------- |
|   [1]   | `OAuth2Client`                     | class         | generic OAuth 2.0 client            |
|   [2]   | `CodeChallengeMethod`              | enum          | PKCE challenge method selection     |
|   [3]   | `OAuth2Tokens`                     | class         | token response wrapper              |
|   [4]   | `OAuth2RequestError`               | error class   | RFC 6749 protocol error             |
|   [5]   | `ArcticFetchError`                 | error class   | network-level failure               |
|   [6]   | `UnexpectedResponseError`          | error class   | non-JSON or non-error HTTP response |
|   [7]   | `UnexpectedErrorResponseBodyError` | error class   | parseable error body with bad shape |

[PUBLIC_TYPE_SCOPE]: selected provider classes
- rail: auth

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [RAIL]                              |
| :-----: | :----------------- | :------------- | :---------------------------------- |
|   [1]   | `GitHub`           | provider class | GitHub OAuth 2.0                    |
|   [2]   | `Google`           | provider class | Google OAuth 2.0 + OIDC + PKCE      |
|   [3]   | `MicrosoftEntraId` | provider class | Microsoft Entra ID OAuth 2.0 + PKCE |
|   [4]   | `Discord`          | provider class | Discord OAuth 2.0                   |
|   [5]   | `Apple`            | provider class | Apple Sign In                       |
|   [6]   | `Spotify`          | provider class | Spotify OAuth 2.0                   |
|   [7]   | `Twitter`          | provider class | Twitter / X OAuth 2.0               |
|   [8]   | `LinkedIn`         | provider class | LinkedIn OAuth 2.0                  |

(45 additional provider classes follow the same shape; see `dist/providers/*.d.ts`.)

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: OAuth2Client — generic low-level client
- rail: auth

```ts
// dist/client.d.ts
export declare class OAuth2Client {
  clientId: string;
  constructor(
    clientId: string,
    clientPassword: string | null,
    redirectURI: string | null,
  );
  createAuthorizationURL(
    authorizationEndpoint: string,
    state: string,
    scopes: string[],
  ): URL;
  createAuthorizationURLWithPKCE(
    authorizationEndpoint: string,
    state: string,
    codeChallengeMethod: CodeChallengeMethod,
    codeVerifier: string,
    scopes: string[],
  ): URL;
  validateAuthorizationCode(
    tokenEndpoint: string,
    code: string,
    codeVerifier: string | null,
  ): Promise<OAuth2Tokens>;
  refreshAccessToken(
    tokenEndpoint: string,
    refreshToken: string,
    scopes: string[],
  ): Promise<OAuth2Tokens>;
  revokeToken(
    tokenRevocationEndpoint: string,
    token: string,
  ): Promise<void>;
}

export declare enum CodeChallengeMethod {
  S256 = 0,
  Plain = 1,
}
```

[ENTRYPOINT_SCOPE]: OAuth2Tokens — token response accessors
- rail: auth

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
```

[ENTRYPOINT_SCOPE]: OIDC and flow utilities
- rail: auth

```ts
// dist/oidc.d.ts
export declare function decodeIdToken(idToken: string): object;
```

[ENTRYPOINT_SCOPE]: provider class pattern — GitHub and Google as canonical examples
- rail: auth

```ts
// dist/providers/github.d.ts
export declare class GitHub {
  constructor(
    clientId: string,
    clientSecret: string,
    redirectURI: string | null,
  );
  createAuthorizationURL(state: string, scopes: string[]): URL;
  validateAuthorizationCode(code: string): Promise<OAuth2Tokens>;
  refreshAccessToken(refreshToken: string): Promise<OAuth2Tokens>;
}

// dist/providers/google.d.ts
export declare class Google {
  constructor(
    clientId: string,
    clientSecret: string,
    redirectURI: string,
  );
  createAuthorizationURL(
    state: string,
    codeVerifier: string,
    scopes: string[],
  ): URL;
  validateAuthorizationCode(
    code: string,
    codeVerifier: string,
  ): Promise<OAuth2Tokens>;
  refreshAccessToken(refreshToken: string): Promise<OAuth2Tokens>;
  revokeToken(token: string): Promise<void>;
}

// dist/providers/microsoft-entra-id.d.ts
export declare class MicrosoftEntraId {
  constructor(
    tenant: string,
    clientId: string,
    clientSecret: string | null,
    redirectURI: string,
  );
  createAuthorizationURL(
    state: string,
    codeVerifier: string,
    scopes: string[],
  ): URL;
  validateAuthorizationCode(
    code: string,
    codeVerifier: string,
  ): Promise<OAuth2Tokens>;
  refreshAccessToken(
    refreshToken: string,
    scopes: string[],
  ): Promise<OAuth2Tokens>;
}
```

[ENTRYPOINT_SCOPE]: error types
- rail: auth

```ts
// dist/request.d.ts
export declare class OAuth2RequestError extends Error {
  code: string;
  description: string | null;
  uri: string | null;
  state: string | null;
  constructor(
    code: string,
    description: string | null,
    uri: string | null,
    state: string | null,
  );
}

export declare class ArcticFetchError extends Error {
  constructor(cause: unknown);
}

export declare class UnexpectedResponseError extends Error {
  status: number;
  constructor(responseStatus: number);
}

export declare class UnexpectedErrorResponseBodyError extends Error {
  status: number;
  data: unknown;
  constructor(status: number, data: unknown);
}
```

## [4]-[IMPLEMENTATION_LAW]

[AUTH_TOPOLOGY]:
- one class per provider; each class is constructed with `clientId`, `clientSecret`, and `redirectURI`
- providers that use PKCE (Google, MicrosoftEntraId, etc.) take `codeVerifier` in `createAuthorizationURL` and `validateAuthorizationCode`
- providers without PKCE (GitHub) omit those parameters entirely
- `OAuth2Client` is the escape hatch for providers not built-in; delegates to the same token request internals
- `decodeIdToken` returns `object`; callers cast or validate against their own schema

[LOCAL_ADMISSION]:
- `generateState` and `generateCodeVerifier` produce cryptographically random values for CSRF and PKCE flows.
- Token responses are wrapped in `OAuth2Tokens`; field access goes through typed accessors, not raw `data`.
- `OAuth2RequestError.code` carries the RFC 6749 `error` string; `description`, `uri`, and `state` are nullable.

[RAIL_LAW]:
- Package: `arctic`
- Owns: OAuth 2.0 authorization URL construction, code exchange, refresh, revocation, and PKCE helpers
- Accept: provider class per provider; `OAuth2Client` for custom endpoints
- Reject: hand-rolled `fetch` for token exchange, raw JSON token parsing outside `OAuth2Tokens`
