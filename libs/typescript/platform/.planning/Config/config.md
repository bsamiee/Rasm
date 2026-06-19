# [PLATFORM_CONFIG]

One page owns the single typed-config boundary — `RuntimeConfig`, one `effect` `Config` schema and one `ConfigProvider` layer over the browser env snapshot, the only surface past which a scattered `import.meta.env` read is the named defect. Every host owner reads its endpoints through this one owner; a missing or malformed key surfaces as a typed `ConfigError` at the read, never an unchecked `unknown` leaking past the boundary.

## [01]-[INDEX]

- [01]-[RUNTIME_CONFIG]: the one `Config` schema and the one `ConfigProvider` boundary.

## [02]-[RUNTIME_CONFIG]

- Owner: `RuntimeConfig`, the one typed `Config` schema and one `ConfigProvider` layer making the single-domain-config-value claim real.
- Cases: configuration enters as one domain value at the root through `RuntimeConfig`, never scattered flag reads; every config read is a typed `Config` access against the one schema and a scattered `import.meta.env` flag read is the deleted form; `AuthSession` (the OIDC authority/client-id/redirect-uri), `SocketTransport` (`socketUrl`), `WireTransport`, `SelfTelemetry`, `RemoteConfig`, `CrashTelemetry`, and `ServiceWorkerHost` all read their endpoints through this one owner, never a direct environment read.
- Auto: the browser variant feeds the `import.meta.env` snapshot into `ConfigProvider.fromJson`, whose `unknown` input is type-narrowed only at `Config` access against the one `RuntimeConfig` schema, so a missing or malformed key surfaces as a typed `ConfigError` at the read, never an unchecked `unknown` leaking past the boundary; `RuntimeConfig.provider` is the graph leaf the config-reading layers depend on, so the acyclic resolution holds in one pass.
- Packages: `effect` for the `Config`/`ConfigProvider` primitives and the `ConfigError` rail.
- Growth: a new config value lands as one field on the `RuntimeConfig` schema, never a second config read.
- Boundary: `RuntimeConfig` is the single config surface and a direct `import.meta.env` read in any owner is the named defect; it authors no decode and dials no transport.

```ts contract
interface RuntimeConfig {
  readonly apiBaseUrl: Effect.Effect<string, ConfigError.ConfigError>;
  readonly oidcAuthority: Effect.Effect<string, ConfigError.ConfigError>;
  readonly oidcClientId: Effect.Effect<string, ConfigError.ConfigError>;
  readonly oidcRedirectUri: Effect.Effect<string, ConfigError.ConfigError>;
  readonly collectorOtlpEndpoint: Effect.Effect<string, ConfigError.ConfigError>;
  readonly remoteConfigUrl: Effect.Effect<string, ConfigError.ConfigError>;
  readonly socketUrl: Effect.Effect<string, ConfigError.ConfigError>;
  readonly buildMode: Effect.Effect<"development" | "production", ConfigError.ConfigError>;
  readonly replaySampleRate: Effect.Effect<number, ConfigError.ConfigError>;
  readonly traceSampleRatio: Effect.Effect<number, ConfigError.ConfigError>;
  readonly provider: Layer.Layer<RuntimeConfig>;
}
```
