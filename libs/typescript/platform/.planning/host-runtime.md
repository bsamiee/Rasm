# [PLATFORM_HOST_RUNTIME]

One page owns the browser SPA composition root and host runtime — `CompositionRoot`, the one Layer graph and one `ManagedRuntime` providing the closed five app-services plus every platform-bound host owner; `BrowserPlatform`, the HTTP/key-value/worker platform layer; `AuthSession`, the OIDC authorization-code-with-PKCE credential owner co-located here as the auth boot edge; and `RuntimeConfig`, the one typed config schema and provider layer. `CompositionRoot` composes the neutral `interchange` and `projection` domains, the `ui` library, the `platform` substrate owners, and the five `platform` infrastructure owners into one running browser surface — the `./web` BROWSER publication entry at `platform/browser.ts`. The page consumes the runtime feed through `projection` and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                       |
| :-----: | :----------- | :----------------------------------------------------------- |
|   [1]   | HOST_RUNTIME | the Layer graph, the runtime, the platform, auth, and config |

## [2]-[HOST_RUNTIME]

- Owner: `CompositionRoot`, the one Layer graph and one `ManagedRuntime` plus the `./web` entry that boots the SPA, plus `BrowserPlatform`, the browser platform layer owning the HTTP client, the key-value store, and the worker-pool spawner, `AuthSession`, the browser credential owner bound under the platform layer as the auth boot edge, and `RuntimeConfig`, the one typed config schema and provider layer making the single-domain-config-value claim real.
- Cases: `CompositionRoot` composes the `interchange` and `projection` neutral domains, the `ui` library render and binding owners, the `platform-substrate` owners, and the five `platform` infrastructure owners (`AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, `PerformanceBudget`) into one acyclic Layer graph and one `ManagedRuntime`, and `platform/browser.ts` is the only module that calls `ManagedRuntime.make` and `runtime.runFork` on the root boot effect — the SPA has one runtime, never a per-route or per-component runtime; `BrowserPlatform` binds the platform services from one `Layer.mergeAll` of the `@effect/platform-browser` fetch, key-value, and worker layers; configuration enters as one domain value at the root through `RuntimeConfig`, never scattered flag reads; `AuthSession` owns the browser bearer credential — OIDC authorization-code-with-PKCE acquisition through `arctic` as the one browser-safe flow with no implicit grant and no client secret, the session-lifecycle fold over a `SubscriptionRef` carrying the current token with its expiry and refresh schedule, silent refresh through a `Schedule` firing before expiry, and `tokenHeader` the per-call producer the `interchange` `WireTransport` interceptor reads at call time — it unwraps the `Redacted` bearer inside `platform` and ships only the assembled `Bearer ...` header string across the seam (the `Redacted` value never crosses), so the browser never stamps a token cached past its expiry and `interchange` declares no OIDC dependency; optional second-factor and passkey enrolment ride the same session owner through the TOTP and WebAuthn surfaces, never a parallel auth owner.
- Entry: the five closed app-service owners — `WireClients`, `CommandGateway` (`interchange`), `SnapshotFeed`, `RuntimeFeed`, `EvidenceFeed` (`projection`) — are provided once in this one Layer graph; a sixth sibling app-service is the named defect, a new state or gateway capability landing as a method or row on one of the five; `AuthSession`, `BrowserPlatform`, `SelfTelemetry`, `MetricRegistry`, `AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, and `PerformanceBudget` are platform-bound HOST owners, not app-services, each provided once under the platform layer; the session status feeds the `ui/binding.md` login-logout leaf through the atom binding, the `routing-navigation.md` `NavigationGuard` reads `AuthSession.status` to gate a protected route, and an expired-or-rejected token folds to the `interchange` `FaultDetail` typed failure as a re-auth fault, never a silent redirect from inside a decode.
- Auto: `RuntimeConfig` is one typed `effect` `Config` schema and one `ConfigProvider` layer, so every config read is a typed `Config` access against the one schema and a scattered `import.meta.env` flag read is the deleted form; the browser variant feeds the `import.meta.env` snapshot into `ConfigProvider.fromJson` (whose `unknown` input is type-narrowed only at `Config` access against the one `RuntimeConfig` schema, so a missing or malformed key surfaces as a typed `ConfigError` at the read, never an unchecked `unknown` leaking past the boundary); `AuthSession`, `WireTransport`, `SelfTelemetry`, `RemoteConfig`, `CrashTelemetry`, and `ServiceWorkerHost` all read their endpoints through this one owner, never a direct environment read; the Layer graph is acyclic by construction — `RuntimeConfig.provider` is the leaf the config-reading layers depend on, `BrowserPlatform` (config-free, the stock `@effect/platform-browser` bindings) and `AuthSession` sit above it, the `interchange`/`projection` neutral layers above those, and the `ui` and `platform` infrastructure layers at the top, so `Layer.provide` resolves the graph in one pass with no `Layer.suspend` cycle break.
- Packages: `effect` for the Layer graph, `ManagedRuntime`, `Config`/`ConfigProvider`, and `SubscriptionRef`/`Schedule`, `@effect/platform` and `@effect/platform-browser` for the platform bindings and worker primitives, `arctic` as the single OAuth/OIDC authorization-code-with-PKCE owner for the token endpoint and refresh, `otplib` for the TOTP second factor, and `@simplewebauthn/server` for the passkey enrolment.
- Growth: a new host service capability lands as a method on one of the five app-service owners; a new platform binding lands as one platform-layer row on `BrowserPlatform`; a new credential modality on `AuthSession` lands as one row on its credential axis, never a parallel session owner; a new config value lands as one field on the `RuntimeConfig` schema, never a second config read; a new platform-bound host owner lands as one layer row in `CompositionRoot` tagged a host owner, never a sixth app-service.
- Boundary: the app-service-owner budget is closed at five; `AuthSession` holds session state as single-fiber host state inside its own `SubscriptionRef`, never a sixth store fold, so a parallel `AuthStore` arm is the named defect; the bearer stamp is designed-only growth that activates with the cross-origin growth row exactly as the C# side gates Bearer behind the cross-origin deployment; `RuntimeConfig` is the single config surface and a direct `import.meta.env` read in any owner is the named defect; `CompositionRoot` is the single composition site and a second `ManagedRuntime.make` anywhere in the branch is the named defect; no integration path resolves into the C# tree, only the inventoried wire contracts; `platform` imports `interchange`, `projection`, and `ui` and is never imported by `services`, and `ui` never imports `platform`.

```ts contract
interface RuntimeConfig {
  readonly apiBaseUrl: Effect.Effect<string, ConfigError.ConfigError>;
  readonly oidcAuthority: Effect.Effect<string, ConfigError.ConfigError>;
  readonly oidcClientId: Effect.Effect<string, ConfigError.ConfigError>;
  readonly collectorOtlpEndpoint: Effect.Effect<string, ConfigError.ConfigError>;
  readonly remoteConfigUrl: Effect.Effect<string, ConfigError.ConfigError>;
  readonly buildMode: Effect.Effect<"development" | "production", ConfigError.ConfigError>;
  readonly provider: Layer.Layer<RuntimeConfig>;
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

class CompositionRoot extends Effect.Service<CompositionRoot>()("@rasm/ts/web/CompositionRoot", {
  effect: Effect.gen(function* () {
    const runtime = yield* Effect.runtime<WireClients | CommandGateway | SnapshotFeed | RuntimeFeed | EvidenceFeed>();
    return { runtime };
  }),
  dependencies: [WireClientsLive, CommandGatewayLive, SnapshotFeedLive, RuntimeFeedLive, EvidenceFeedLive],
}) {}

const BrowserPlatformLayer: Layer.Layer<
  HttpClient.HttpClient | KeyValueStore.KeyValueStore | WorkerManager.WorkerManager
> = Layer.mergeAll(
  FetchHttpClient.layer,
  BrowserKeyValueStore.layerLocalStorage,
  BrowserWorker.layerPlatform,
);

const AppLayer: Layer.Layer<
  | WireClients | CommandGateway | SnapshotFeed | RuntimeFeed | EvidenceFeed
  | AuthSession | SelfTelemetry | MetricRegistry
  | HttpClient.HttpClient | KeyValueStore.KeyValueStore | WorkerManager.WorkerManager
  | AppRouter | ServiceWorkerHost | CrashTelemetry | RemoteConfig | PerformanceBudget,
  never,
  never
> = pipe(
  Layer.mergeAll(WireClientsLive, CommandGatewayLive, SnapshotFeedLive, RuntimeFeedLive, EvidenceFeedLive),
  Layer.provideMerge(Layer.mergeAll(AppRouterLive, ServiceWorkerHostLive, CrashTelemetryLive, RemoteConfigLive, PerformanceBudgetLive)),
  Layer.provideMerge(Layer.mergeAll(AuthSessionLive, SelfTelemetryLive, MetricRegistryLive)),
  Layer.provideMerge(BrowserPlatformLayer),
  Layer.provide(RuntimeConfigLive.provider),
);

const bootSpa: Effect.Effect<void, never, never> = Effect.gen(function* () {
  const crash = yield* CrashTelemetry;
  const sw = yield* ServiceWorkerHost;
  const router = yield* AppRouter;
  yield* crash.installGlobalHandlers;
  yield* sw.register;
  yield* router.start;
}).pipe(Effect.provide(AppLayer));

const runtime: ManagedRuntime.ManagedRuntime<Layer.Layer.Success<typeof AppLayer>, never> = ManagedRuntime.make(AppLayer);
runtime.runFork(bootSpa);
```
