# [PLATFORM_SYNC]

One page owns the redial-driven offline-queue drain — `BackgroundSyncReplay`, the redial drain reading the `local-persistence` `LocalPersistence.offlineQueue` into the `interchange` `CommandGateway` on reconnect. It subscribes to the `online` window event and registers a native `SyncManager` sync tag where present as the background wake; both feed the one drain fold, so `SyncManager` absence never opens a second queue. A replay whose gateway invoke faults re-enqueues the element over the one `interchange` `FaultDetail` channel, never a silent drop.

## [1]-[INDEX]

- [1]-[BACKGROUND_SYNC_REPLAY]: the redial drain of the offline queue into the gateway.

## [2]-[BACKGROUND_SYNC_REPLAY]

- Owner: `BackgroundSyncReplay`, the redial drain reading `LocalPersistence.offlineQueue` into the `interchange` `CommandGateway`.
- Auto: `BackgroundSyncReplay` subscribes to the `online` window event through `BrowserStream.fromEventListenerWindow("online")`, registers a native `SyncManager` sync tag where present as the background wake, and on each redial reads `LocalPersistence.offlineQueue.drainOnRedial` (yielding `ReadonlyArray<{ verb: ControlVerb; payload: CommandPayloadWire }>` from the `local-persistence` owner verbatim — the same resolved-intent pair `interchange` `IntentRegistry.resolve` yields) and replays each queued element through the `interchange` `CommandGateway.invoke`; the gateway's `AvailabilityStore` requirement rides the `invoke` `R` channel and stays surfaced in `run`'s `R` (provided at the SPA composition root per the `interchange` charter, never erased here), so the same offline-queue seam `local-persistence` names is drained by one owner on reconnect rather than a parallel SW-resident queue; a replay whose gateway invoke faults re-enqueues the element through `offlineQueue.enqueue` via `Effect.catchAll` over the one `interchange` `FaultDetail` error channel — every one of its five tags re-enqueued, never a silent drop; the native `SyncManager` registration and the `online`-event drain both feed the one drain fold, so the `SyncManager` absence on a host is an optional capability, never a parallel queue.
- Packages: `effect` `Stream`/`Effect.catchAll`/`Effect.forkScoped` for the drain pipeline; `@effect/platform-browser` `BrowserStream.fromEventListenerWindow` for the `online` ingress; the native `SyncManager` for the background wake where present.
- Growth: a new offline-replay concern lands as one row on `BackgroundSyncReplay`, never a parallel queue or a second worker.
- Boundary: `BackgroundSyncReplay` drains the single `LocalPersistence.offlineQueue` the `local-persistence` page owns — consuming `drainOnRedial`/`enqueue` over the `{ verb: ControlVerb; payload: CommandPayloadWire }` resolved-intent element verbatim (the element shape is owned and settled at `local-persistence`, never re-derived here) — and replays through the `interchange` `CommandGateway.invoke` across the folder seam, never re-dialing a transport here and never holding a parallel offline queue.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import { Effect, Layer, Stream } from "effect";
import * as BrowserStream from "@effect/platform-browser/BrowserStream";
import type { AvailabilityStore } from "../projection/availability-gate.ts";
import type { FaultDetail } from "../interchange/fault-family.ts";
import { CommandGatewayLive, type CommandPayloadWire, type ControlVerb } from "../interchange/command-gateway.ts";
import { LocalPersistenceLive } from "./local-persistence.ts";
import { ServiceWorkerHostLive } from "./service-worker-host.ts";

// --- [SERVICES] ------------------------------------------------------------------------
interface BackgroundSyncReplay {
  readonly run: Effect.Effect<void, never, AvailabilityStore>;
}

class BackgroundSyncReplayLive extends Effect.Service<BackgroundSyncReplayLive>()(
  "@rasm/ts/platform/BackgroundSyncReplay",
  {
    dependencies: [CommandGatewayLive.Default, LocalPersistenceLive.Default],
    scoped: Effect.gen(function* () {
      const gateway = yield* CommandGatewayLive;
      const persistence = yield* LocalPersistenceLive;

      const registerNativeSync: Effect.Effect<void> = Effect.promise(() => navigator.serviceWorker.ready).pipe(
        Effect.flatMap((reg) =>
          "sync" in reg
            ? Effect.tryPromise(() => (reg as ServiceWorkerRegistration & { sync: SyncManager }).sync.register("rasm-offline-drain")).pipe(Effect.ignore)
            : Effect.void,
        ),
        Effect.ignore,
      );

      const run: Effect.Effect<void, never, AvailabilityStore> = registerNativeSync.pipe(
        Effect.zipRight(
          BrowserStream.fromEventListenerWindow("online").pipe(
            Stream.mapEffect(() => persistence.offlineQueue.drainOnRedial),
            Stream.flatMap(Stream.fromIterable),
            Stream.mapEffect((item: { readonly verb: ControlVerb; readonly payload: CommandPayloadWire }) =>
              gateway.invoke(item.verb, item.payload).pipe(
                Effect.asVoid,
                Effect.catchAll((_fault: FaultDetail) => persistence.offlineQueue.enqueue(item)),
              )),
            Stream.runDrain,
          ),
        ),
      );

      return { run } satisfies BackgroundSyncReplay;
    }),
  },
) {}

// --- [COMPOSITION] ---------------------------------------------------------------------
const ServiceWorkerLayer = Layer.mergeAll(ServiceWorkerHostLive.Default, BackgroundSyncReplayLive.Default);

// --- [EXPORTS] -------------------------------------------------------------------------
export { type BackgroundSyncReplay, BackgroundSyncReplayLive, ServiceWorkerLayer };
```
