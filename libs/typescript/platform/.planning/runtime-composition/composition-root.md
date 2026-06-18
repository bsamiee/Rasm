# [PLATFORM_COMPOSITION_ROOT]

One page owns the browser SPA composition root and the single boot entry — `CompositionRoot`, the one `Layer` graph and one `ManagedRuntime` providing the closed five app-services plus every platform-bound host owner, and `browser.ts`, the `./web` BROWSER publication entry that calls `ManagedRuntime.make` and `runtime.runFork` exactly once. The root composes the neutral `interchange` and `projection` domains, the `ui` library, the substrate owners, and the infrastructure owners into one acyclic graph; it consumes the runtime feed through `projection` and authors no decode.

## [1]-[INDEX]

[COMPOSITION_ROOT]: the Layer graph, the one runtime, and the `./web` SPA boot entry.

## [2]-[COMPOSITION_ROOT]

- Owner: `CompositionRoot`, the one `Layer` graph and one `ManagedRuntime` over the closed five app-services, plus `browser.ts`, the single `./web` entry that boots the SPA.
- Cases: `CompositionRoot` composes the `interchange` and `projection` neutral domains, the `ui` render and binding owners, the `runtime-config`/`identity-session`/`observability`/`local-persistence`/`build-pipeline` substrate owners, and the five infrastructure owners (`AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, `PerformanceBudget`) into one acyclic `Layer` graph and one `ManagedRuntime`; `browser.ts` is the only module that calls `ManagedRuntime.make` and `runtime.runFork` on the root boot effect, so the SPA carries one runtime, never a per-route or per-component runtime; the boot effect installs the crash handlers, registers the service worker, and starts the router as the SPA's three boot acts.
- Entry: the five closed app-service owners — `WireClients`, `CommandGateway` (`interchange`), `SnapshotFeed`, `RuntimeFeed`, `EvidenceFeed` (`projection`) — are provided once in this graph; a sixth sibling app-service is the named defect, a new state or gateway capability landing as a method or row on one of the five; `AuthSession`, `BrowserPlatform`, `SelfTelemetry`, `MetricRegistry`, `AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, and `PerformanceBudget` are platform-bound HOST owners provided once under the platform layer, never app-services.
- Auto: the `Layer` graph is acyclic by construction — `RuntimeConfig.provider` is the leaf the config-reading layers depend on, `BrowserPlatform` and `AuthSession` sit above it, the `interchange`/`projection` neutral layers above those, and the `ui` and infrastructure layers at the top, so `Layer.provide` resolves the graph in one pass with no `Layer.suspend` cycle break.
- Packages: `effect` for the `Layer` graph, `ManagedRuntime`, and `Effect.Service`; `@effect/platform` and `@effect/platform-browser` for the platform bindings composed under the graph.
- Growth: a new host service capability lands as a method on one of the five app-service owners; a new platform-bound host owner lands as one layer row tagged a host owner, never a sixth app-service.
- Boundary: the app-service-owner budget is closed at five; `CompositionRoot` is the single composition site and a second `ManagedRuntime.make` anywhere in the branch is the named defect; no integration path resolves into the C# tree, only the inventoried wire contracts; `platform` imports `interchange`, `projection`, and `ui` and is never imported by `services`, and `ui` never imports `platform`.

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
