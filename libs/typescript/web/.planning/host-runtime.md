# [WEB_HOST_RUNTIME]

One page owns the browser SPA composition root and platform substrate — `CompositionRoot`, the one Layer graph and one runtime providing the closed five app-services; `BrowserPlatform`, the HTTP/key-value/worker platform layer; `AuthSession`, the OIDC authorization-code-with-PKCE credential owner co-located here as the auth boot edge; and `RuntimeConfig`, the one typed config schema and provider layer. It assembles the `@rasm/interchange` and `@rasm/projection` domains plus the `@rasm/web` render and substrate owners into one running browser surface — the BROWSER publication entry. The page consumes the runtime feed through `@rasm/projection` and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                       |
| :-----: | :------------ | :--------------------------------------------------------- |
|   [1]   | HOST_RUNTIME  | the Layer graph, the runtime, the platform, auth, and config |

## [2]-[HOST_RUNTIME]

- Owner: `CompositionRoot`, the one Layer graph and one runtime, plus `BrowserPlatform`, the browser platform layer owning the HTTP client, the key-value store, and the worker pool, `AuthSession`, the browser credential owner bound under the platform layer as the auth boot edge, and `RuntimeConfig`, the one typed config schema and provider layer making the single-domain-config-value claim real.
- Cases: `CompositionRoot` assembles the `@rasm/interchange` and `@rasm/projection` domains plus the render and substrate owners into one Layer graph and one runtime; `BrowserPlatform` binds the platform services; configuration enters as one domain value at the root through `RuntimeConfig`, never scattered flag reads; `AuthSession` owns the browser bearer credential — OIDC authorization-code-with-PKCE acquisition through `arctic` as the one browser-safe flow with no implicit grant and no client secret, the session-lifecycle fold over a `SubscriptionRef` carrying the current token with its expiry and refresh schedule, silent refresh through a `Schedule` firing before expiry, and `tokenHeader` the per-call producer the `@rasm/interchange` `WireTransport` interceptor reads at call time — it unwraps the `Redacted` bearer inside `@rasm/web` and ships only the assembled `Bearer ...` header string across the seam (the `Redacted` value never crosses), so the browser never stamps a token cached past its expiry and `@rasm/interchange` declares no OIDC dependency; optional second-factor and passkey enrolment ride the same session owner through the TOTP and WebAuthn surfaces, never a parallel auth owner.
- Entry: the five closed app-service owners — `WireClients`, `CommandGateway` (`@rasm/interchange`), `SnapshotFeed`, `RuntimeFeed`, `EvidenceFeed` (`@rasm/projection`) — are provided once in this one Layer graph; a sixth sibling app-service is the named defect, a new state or gateway capability landing as a method or row on one of the five; `AuthSession` is a platform-bound host owner like `BrowserPlatform` and `SelfTelemetry`, not a sixth app-service, and is provided once under the platform layer; the session status feeds the `binding.md` login-logout leaf through the atom binding and an expired-or-rejected token folds to the `@rasm/interchange` `FaultDetail` typed failure as a re-auth fault, never a silent redirect from inside a decode.
- Auto: `RuntimeConfig` is one typed `effect` `Config` schema and one `ConfigProvider` layer, so every config read is a typed `Config` access against the one schema and a scattered `import.meta.env` flag read is the deleted form; the browser variant feeds the `import.meta.env` snapshot into `ConfigProvider.fromJson` (whose `unknown` input is type-narrowed only at `Config` access against the one `RuntimeConfig` schema, so a missing or malformed key surfaces as a typed `ConfigError` at the read, never an unchecked `unknown` leaking past the boundary); `BrowserPlatform`, `WireTransport`, and `SelfTelemetry` read the config through this owner, never a direct environment read.
- Packages: `effect` for the Layer graph and runtime, `@effect/platform` and `@effect/platform-browser` for the platform bindings and worker primitives, `arctic` as the single OAuth/OIDC authorization-code-with-PKCE owner for the token endpoint and refresh, `otplib` for the TOTP second factor, and `@simplewebauthn/server` for the passkey enrolment.
- Growth: a new host service capability lands as a method on one of the five owners; a new platform binding lands as one platform-layer row; a new credential modality on `AuthSession` lands as one row on its credential axis, never a parallel session owner; a new config value lands as one field on the `RuntimeConfig` schema, never a second config read.
- Boundary: the app-service-owner budget is closed at five; `AuthSession` holds session state as single-fiber host state inside its own `SubscriptionRef`, never a sixth store fold, so a parallel `AuthStore` arm is the named defect; the bearer stamp is designed-only growth that activates with the cross-origin growth row exactly as the C# side gates Bearer behind the cross-origin deployment; `RuntimeConfig` is the single config surface and a direct `import.meta.env` read in any owner is the named defect; no integration path resolves into the C# tree, only the inventoried wire contracts; this domain imports `@rasm/interchange` and `@rasm/projection` and is never imported by the node domain.

```ts contract
interface RuntimeConfig {
  readonly apiBaseUrl: Effect.Effect<string, ConfigError.ConfigError>;
  readonly oidcAuthority: Effect.Effect<string, ConfigError.ConfigError>;
  readonly oidcClientId: Effect.Effect<string, ConfigError.ConfigError>;
  readonly collectorOtlpEndpoint: Effect.Effect<string, ConfigError.ConfigError>;
  readonly buildMode: Effect.Effect<"development" | "production", ConfigError.ConfigError>;
  readonly provider: Layer.Layer<never>;
}

type SessionStatus =
  | { readonly _tag: "Anonymous" }
  | { readonly _tag: "Authenticating" }
  | { readonly _tag: "Authenticated"; readonly subject: string; readonly expiresAt: number }
  | { readonly _tag: "Expired" };

type BearerToken = { readonly value: Redacted.Redacted; readonly expiresAt: number; readonly refreshAt: number };

class AuthFault extends Data.TaggedError("AuthFault")<{ readonly reason: "denied" | "expired" | "refresh-failed" }> {}

interface AuthSession {
  readonly status: SubscriptionRef.SubscriptionRef<SessionStatus>;
  readonly login: Effect.Effect<void, AuthFault>;
  readonly logout: Effect.Effect<void>;
  readonly currentToken: Effect.Effect<Option.Option<BearerToken>, AuthFault>;
  readonly tokenHeader: Effect.Effect<Option.Option<string>>;
}

class CompositionRoot extends Effect.Service<CompositionRoot>()("@rasm/web/CompositionRoot", {
  effect: Effect.gen(function* () {
    const runtime = yield* Effect.runtime<WireClients | CommandGateway | SnapshotFeed | RuntimeFeed | EvidenceFeed>();
    return { runtime };
  }),
  dependencies: [],
}) {}
```
