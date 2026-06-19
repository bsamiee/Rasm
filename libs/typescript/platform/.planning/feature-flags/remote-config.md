# [PLATFORM_REMOTE_CONFIG]

One page owns the browser feature-flag and remote-config read-side — `RemoteConfig`, the browser READ-side companion to the `services` `FeatureFlags` owner: a `RuntimeConfig`-fed remote-config fetch decoded once through `Schema.decodeUnknown` into one `FlagSet` `SubscriptionRef`, with flag evaluation as a total `Match` dispatch over a `Schema.Literal` flag-key axis whose percentage-bucket and variant VOCABULARY is consumed as settled from `services`, and whose read-side resolution is a local deterministic bucket over the decoded `FlagSet`. The flag-bucket/variant axis is declared once in `services` and referenced here, never re-authored — the anti-spam cluster law. `RemoteConfig` authors no wire shape; the `FlagSet` decode shape is the only platform-local `Schema`. The page composes `RuntimeConfig` and the browser `HttpClient` and surfaces a flag through the `ui` `AtomBinding`.

## [1]-[INDEX]

[REMOTE_CONFIG]: the remote-config fetch, the flag-key dispatch, and the refresh.

## [2]-[REMOTE_CONFIG]

- Owner: `RemoteConfig`, the single browser flag/config read surface — the remote-config fetch decoded into one `FlagSet` `SubscriptionRef`, `FlagEvaluation` the total `Match` dispatch over the flag-key axis, and the poll/refresh `Schedule`; `FlagKey`, the `Schema.Literal` flag axis whose membership references the `services` `FeatureFlags` flag set; and `FlagEvaluation`, the bucket-plus-variant resolution consuming the `services` percentage-bucket/variant vocabulary. The flag-bucket/variant `Schema.Literal` axis is declared once in `services` `persistence#WORK_AND_SIGNALS` and the only platform-local `Schema` is the `FlagSet` decode shape; a flag-bucket vocabulary re-declared here is the named anti-spam defect.
- Cases: `RemoteConfig` reads the config endpoint from `RuntimeConfig`, fetches the remote-config document through the browser `HttpClient`, and decodes it ONCE through `Schema.decodeUnknown(FlagSetSchema)` into the `FlagSet` `SubscriptionRef`, a decode failure folding to a typed `FaultDetail.ConfigError` and retaining the last-good `FlagSet` rather than clearing flags; `FlagEvaluation` is a total `Match` dispatch over the `FlagKey` axis — for a boolean flag it reads the `FlagSet` enabled set, for a bucketed flag it resolves the 0-100 rollout bucket against the subject key over the same flag vocabulary the `services` `FeatureFlags.enabled(flag, subjectKey)` owner gates server-side, and for a variant flag it selects the variant by the same bucket projection, so the browser evaluation is the read-side mirror of the one 0-100 rollout vocabulary the `services` owner fixes — a local deterministic bucket over the decoded `FlagSet`, never a second flag axis; the live ingress is the `feature-flags/flag-stream.md` `FlagStream` raw `text/event-stream` `HttpClient`-body SSE channel decoded through `Sse.makeParser` and patching the `FlagSet` cell in place per delta frame, and the fixed-interval poll `Schedule` here demotes to the reconnect/backfill fallback under that one retry policy, so a flag flip propagates in seconds over SSE and the poll backfills only when the stream is absent — the two feed one fold, never a parallel flag source.
- Auto: the `FlagSet` decode is the single boundary decode — the raw fetched document is `unknown` until `Schema.decodeUnknown(FlagSetSchema)` narrows it at the boundary, so a missing or malformed flag surfaces as a typed `ParseResult.ParseError` folded to `FaultDetail.ConfigError`, never an unchecked `unknown` leaking into evaluation; the evaluation never re-validates the decoded `FlagSet` in the interior.
- Packages: `effect` for the `Schema.Literal` flag axis, the `Schema.decodeUnknown` boundary decode, the `Match` evaluation dispatch, the `SubscriptionRef` flag cell, and the refresh `Schedule`; the browser `HttpClient` (`@effect/platform-browser` `BrowserHttpClient`) for the config fetch; `RuntimeConfig` for the config endpoint. No flag-vocabulary package and no second config read.
- Growth: a new flag lands as one literal on the `FlagKey` axis referencing the `services` `FeatureFlags` vocabulary; a new evaluation modality lands as one arm on the `FlagEvaluation` `Match` dispatch; a new live-delta operation lands as one arm on the `feature-flags/flag-stream.md` `FlagDelta` fold patching this cell, never a parallel ingress; the OpenFeature client-side provider shape (a `resolveBooleanEvaluation`/`resolveStringEvaluation`-style resolver returning a reason plus variant) lands as a thin projection of the existing total `Match` dispatch, never a new flag axis and never a re-authored vocabulary.
- Boundary: `RemoteConfig` is the browser READ-side companion to the `services` `FeatureFlags` owner — the percentage-bucket/variant `Schema.Literal` axis is declared once in `services` `persistence#WORK_AND_SIGNALS` and referenced here as settled vocabulary, so a re-authored bucket/variant axis is the named anti-spam cluster defect; the only platform-local `Schema` is the `FlagSet` decode shape and `RemoteConfig` authors no wire contract; the config endpoint is read through `RuntimeConfig` and a direct `import.meta.env` flag read is the named defect; a flag value reaches a component through the `ui` `AtomBinding`, never a second state binding; `RemoteConfig` dials no transport beyond the config fetch and emits no command; `ui` reads a flag through the binding and never imports `platform`.

```ts contract
type FlagKey =
  | "glb-viewport"
  | "geo-series-interleave"
  | "evidence-skew-band"
  | "offline-command-queue"
  | "benchmark-route"
  | "collector-panel"
  | "session-replay";

const FlagKeySchema = Schema.Literal("glb-viewport", "geo-series-interleave", "evidence-skew-band", "offline-command-queue", "benchmark-route", "collector-panel", "session-replay");

const FlagValueSchema = Schema.Union(
  Schema.Struct({ _tag: Schema.Literal("Boolean"), enabled: Schema.Boolean }),
  Schema.Struct({ _tag: Schema.Literal("Bucketed"), rolloutBucket: Schema.Number }),
  Schema.Struct({ _tag: Schema.Literal("Variant"), rolloutBucket: Schema.Number, variants: Schema.Array(Schema.String) }),
);
type FlagValue = typeof FlagValueSchema.Type;

const FlagSetSchema = Schema.Struct({
  flags: Schema.ReadonlyMapFromRecord({ key: FlagKeySchema, value: FlagValueSchema }),
  fetchedAt: Schema.Number,
});
type FlagSet = typeof FlagSetSchema.Type;

type FlagEvaluation = Data.TaggedEnum<{
  readonly Off: object;
  readonly On: object;
  readonly Variant: { readonly variant: string };
}>;
const FlagEvaluation = Data.taggedEnum<FlagEvaluation>();

interface RemoteConfig {
  readonly flags: SubscriptionRef.SubscriptionRef<FlagSet>;
  readonly evaluate: (key: FlagKey, subjectKey: string) => Effect.Effect<FlagEvaluation>;
  readonly refresh: Effect.Effect<void, FaultDetail>;
}

const evaluateFlag = (flagKey: FlagKey, value: FlagValue, subjectKey: string): FlagEvaluation => {
  const bucket = Array.from(`${flagKey}:${subjectKey}`).reduce((acc, ch) => (acc * 31 + ch.charCodeAt(0)) >>> 0, 0) % 100;
  return Match.value(value).pipe(
    Match.tagsExhaustive({
      Boolean: (v) => (v.enabled ? FlagEvaluation.On() : FlagEvaluation.Off()),
      Bucketed: (v) => (bucket < v.rolloutBucket ? FlagEvaluation.On() : FlagEvaluation.Off()),
      Variant: (v) =>
        bucket < v.rolloutBucket
          ? FlagEvaluation.Variant({ variant: v.variants[bucket % v.variants.length] ?? v.variants[0]! })
          : FlagEvaluation.Off(),
    }),
  );
};

const makeRemoteConfig: Effect.Effect<RemoteConfig, never, Scope.Scope | RuntimeConfig | HttpClient.HttpClient> = Effect.gen(function* () {
  const config = yield* RuntimeConfig;
  const http = yield* HttpClient.HttpClient;
  const flags = yield* SubscriptionRef.make<FlagSet>({ flags: new Map(), fetchedAt: 0 });
  const refresh = config.apiBaseUrl.pipe(
    Effect.mapError(() => FaultDetail.ConfigError({ code: 0, evidence: { kind: "config-endpoint" } })),
    Effect.flatMap((base) => http.get(`${base}/config/flags`)),
    Effect.flatMap((res) => res.json),
    Effect.flatMap(Schema.decodeUnknown(FlagSetSchema)),
    Effect.flatMap((next) => SubscriptionRef.set(flags, next)),
    Effect.catchAll(() => Effect.fail(FaultDetail.ConfigError({ code: 0, evidence: { kind: "config-decode" } }))),
  );
  yield* refresh.pipe(Effect.ignore, Effect.repeat(Schedule.fixed("5 minutes")), Effect.forkScoped);
  const evaluate = (key: FlagKey, subjectKey: string) =>
    SubscriptionRef.get(flags).pipe(
      Effect.map((set) => {
        const value = set.flags.get(key);
        return value === undefined ? FlagEvaluation.Off() : evaluateFlag(key, value, subjectKey);
      }),
    );
  return { flags, evaluate, refresh } satisfies RemoteConfig;
});

const RemoteConfig = Effect.Tag("@rasm/ts/platform/RemoteConfig")<RemoteConfig, RemoteConfig>();
const RemoteConfigLive: Layer.Layer<RemoteConfig, never, RuntimeConfig | HttpClient.HttpClient> = Layer.scoped(RemoteConfig, makeRemoteConfig);

export { type FlagKey, type FlagSet, type FlagValue, FlagEvaluation, FlagKeySchema, FlagValueSchema, RemoteConfig, RemoteConfigLive };
```
