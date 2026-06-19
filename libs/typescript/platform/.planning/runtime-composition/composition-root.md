# [PLATFORM_COMPOSITION_ROOT]

One page owns the browser SPA composition root and the single boot entry — `CompositionRoot`, the one `Layer` graph providing the closed five app-services plus every platform-bound host owner and exposing the captured `Effect.runtime` snapshot the per-call interceptors resolve against, and `browser.ts`, the `./web` BROWSER publication entry that runs the fully-provided root boot effect through `BrowserRuntime.runMain` exactly once (teardown wired to the browser lifecycle, defects reported). The root composes the neutral `interchange` and `projection` domains, the `ui` library, the substrate owners, and the infrastructure owners — including the runtime-state spine (`AppLifecycle`, `CapabilityRank`, `Connectivity`) and the `worker/` decode pool — into one acyclic graph; it consumes the runtime feed through `projection` and authors no decode.

## [1]-[INDEX]

[COMPOSITION_ROOT]: the Layer graph, the one runtime, and the `./web` SPA boot entry.

## [2]-[COMPOSITION_ROOT]

- Owner: `CompositionRoot`, the one `Layer` graph and one `ManagedRuntime` over the closed five app-services, plus `browser.ts`, the single `./web` entry that boots the SPA.
- Cases: `CompositionRoot` composes the `interchange` and `projection` neutral domains, the `ui` render and binding owners, the `runtime-config`/`identity-session`/`observability`/`local-persistence` substrate owners, the runtime-state spine (`AppLifecycle`, `CapabilityRank`, `Connectivity`) and the `worker/` `DecodeWorkerPool`, the five infrastructure owners (`AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, `PerformanceBudget`), and the `session-replay` `SessionRecorder` host owner into one acyclic `Layer` graph; `browser.ts` is the only module that runs the fully-provided root boot effect through `BrowserRuntime.runMain`, so the SPA carries one runtime, never a per-route or per-component runtime; the boot effect installs the crash handlers, registers the service worker and the native sync wake, starts the router, and arms the flag-gated sampled session recorder against the current subject key as the SPA's boot acts.
- Entry: the five closed app-service owners — `WireClients`, `CommandGateway` (`interchange`), `SnapshotFeed`, `RuntimeFeed`, `EvidenceFeed` (`projection`) — are provided once in this graph; a sixth sibling app-service is the named defect, a new state or gateway capability landing as a method or row on one of the five; `AuthSession`, `BrowserPlatform`, `SelfTelemetry`, `MetricRegistry`, `AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, `PerformanceBudget`, `SessionRecorder`, and the runtime-state spine (`AppLifecycle`, `CapabilityRank`, `Connectivity`, `DecodeWorkerPool`) are platform-bound HOST owners provided once under the platform layer, never app-services — the closed five-app-service budget holds.
- Auto: the `Layer` graph is acyclic by construction — `RuntimeConfig.provider` is the leaf the config-reading layers depend on, `BrowserPlatform` and `AuthSession` sit above it, the runtime-state spine (`Connectivity`/`AppLifecycle`/`CapabilityRank`) and the `worker/` `DecodeWorkerPool` next, the `RemoteConfig`/`SessionRecorder` read-side band above those (the recorder reads `RemoteConfig`/`MetricRegistry`/`AppLifecycle`, so it provides below the `CrashTelemetry`/`PerformanceBudget` consumers reading its `windowId` correlation cell), the `interchange`/`projection` neutral layers above those, and the `ui` and infrastructure layers at the top, so `Layer.provide` resolves the graph in one pass with no `Layer.suspend` cycle break; `browser.ts` runs the fully-provided `bootSpa` through `@effect/platform-browser` `BrowserRuntime.runMain`, the one browser-tier entrypoint that wires teardown and interruption to the browser lifecycle and reports defects, so a `runtime.runFork` that swallows the boot defect is the retired form.
- Packages: `effect` for the `Layer` graph, `Effect.runtime`, and `Effect.Service`; `@effect/platform` and `@effect/platform-browser` for the platform bindings composed under the graph and the `BrowserRuntime.runMain` boot entry.
- Growth: a new host service capability lands as a method on one of the five app-service owners; a new platform-bound host owner lands as one layer row tagged a host owner, never a sixth app-service.
- Boundary: the app-service-owner budget is closed at five; `CompositionRoot` is the single composition site and a second `BrowserRuntime.runMain` anywhere in the branch is the named single-boot defect; no integration path resolves into the C# tree, only the inventoried wire contracts; `platform` imports `interchange`, `projection`, and `ui` and is never imported by `services`, and `ui` never imports `platform`.

```ts contract
class CompositionRoot extends Effect.Service<CompositionRoot>()("@rasm/ts/platform/CompositionRoot", {
  effect: Effect.gen(function* () {
    const runtime = yield* Effect.runtime<WireClients | CommandGateway | SnapshotFeed | RuntimeFeed | EvidenceFeed>();
    return { runtime };
  }),
  dependencies: [WireClientsLive, CommandGatewayLive, SnapshotFeedLive, RuntimeFeedLive, EvidenceFeedLive],
}) {}

const AppLayer: Layer.Layer<
  | WireClients | CommandGateway | SnapshotFeed | RuntimeFeed | EvidenceFeed
  | AuthSession | SelfTelemetry | MetricRegistry
  | HttpClient.HttpClient | KeyValueStore.KeyValueStore | Worker.WorkerManager
  | AppRouter | ServiceWorkerHost | CrashTelemetry | RemoteConfig | PerformanceBudget | SessionRecorder
  | AppLifecycle | CapabilityRank | Connectivity | DecodeWorkerPool,
  never,
  never
> = pipe(
  Layer.mergeAll(WireClientsLive, CommandGatewayLive, SnapshotFeedLive, RuntimeFeedLive, EvidenceFeedLive),
  Layer.provideMerge(Layer.mergeAll(ServiceWorkerHostLive, CrashTelemetryLive, PerformanceBudgetLive)),
  Layer.provideMerge(Layer.mergeAll(AppRouterLive, SessionRecorderLive)),
  Layer.provideMerge(RemoteConfigLive),
  Layer.provideMerge(Layer.mergeAll(AppLifecycleLive.Default, CapabilityRankLive.Default, ConnectivityLive.Default, DecodeWorkerPoolLive.Default)),
  Layer.provideMerge(Layer.mergeAll(AuthSessionLive, SelfTelemetryLive, MetricRegistryLive)),
  Layer.provideMerge(BrowserPlatformLayer),
  Layer.provide(RuntimeConfigLive.provider),
);

const bootSpa: Effect.Effect<void, never, never> = Effect.gen(function* () {
  const crash = yield* CrashTelemetry;
  const sw = yield* ServiceWorkerHost;
  const connectivity = yield* Connectivity;
  const router = yield* AppRouter;
  const session = yield* AuthSession;
  const recorder = yield* SessionRecorder;
  yield* crash.installGlobalHandlers;
  yield* sw.register;
  yield* connectivity.registerNativeSync;
  yield* router.start;
  const status = yield* SubscriptionRef.get(session.status);
  const subjectKey = SessionStatus.$is("Authenticated")(status) ? status.subject : "anonymous";
  yield* recorder.record(subjectKey);
}).pipe(Effect.scoped, Effect.provide(AppLayer));

BrowserRuntime.runMain(bootSpa);
```
