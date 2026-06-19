# [PLATFORM_CONNECTIVITY]

One page owns the browser online/offline connectivity edge — `Connectivity`, the one `online` `SubscriptionRef` folded from the merged `online`/`offline` window events, plus the native `SyncManager` background-wake registration that feeds the same redial fold. It is the single connectivity owner extracted out of `offline-cache/background-sync-replay.md` because four concerns read it — `runtime-composition/capability-rank.md`'s rank input, `offline-cache/background-sync-replay.md`'s redial drain, `feature-flags`'s reconnect fallback, and `observability`'s connectivity gauge — so a private `navigator.onLine` read at any of them is the named per-component-probe defect. The redial that drains the offline queue is one fold over this one cell and the `SyncManager` wake; the page authors no decode and crosses no wire.

## [1]-[INDEX]

[CONNECTIVITY]: the online/offline cell, the redial edge, and the native SyncManager wake.

## [2]-[CONNECTIVITY]

- Owner: `Connectivity`, the single connectivity owner — one `online` `boolean` `SubscriptionRef` seeded from `navigator.onLine` and advanced only by the merged `online`/`offline` window-event fold, the `redials` `Stream` projecting each offline->online edge, and the native `SyncManager` sync-tag registration as the background wake. The `online` cell is the one connectivity vocabulary and a `navigator.onLine` read at a consumer is the named per-component-probe defect.
- Cases: `Connectivity` merges `BrowserStream.fromEventListenerWindow("online")` and `fromEventListenerWindow("offline")` into one capture stream folding each native event into the `online` cell, so the formerly-private `online` listener in `background-sync-replay.md` is one owned cell four concerns read; the `redials` stream projects the rising offline->online edge (a `false`->`true` transition on the cell) so a consumer subscribes to the exact redial edge it drains on rather than re-deriving the transition; `registerNativeSync` registers the `rasm-offline-drain` `SyncManager` tag where `navigator.serviceWorker.ready` carries a `sync` manager, an unsupported host folding to a no-op so the `SyncManager` absence is an optional capability, never a parallel queue — the native wake and the `redials` edge both feed the one downstream drain fold.
- Auto: the merged `online`/`offline` capture is forked `Effect.forkScoped` for the runtime lifetime; the `redials` projection is `Stream.changes` over the cell filtered to the rising edge, so `background-sync-replay.md` drains on `redials` and `capability-rank.md` reads the `online` cell from the one owner; `registerNativeSync` rides `Effect.tryPromise` over `navigator.serviceWorker.ready` with the `sync`-manager probe a precise `ServiceWorkerRegistration & { sync: SyncManager }` refinement, the registration `Effect.ignore`d where unsupported.
- Packages: `effect` `SubscriptionRef` for the `online` cell, `Stream.merge`/`Stream.changes`/`Effect.forkScoped` for the merged fold and the redial projection, and `Effect.tryPromise`/`Effect.ignore` for the optional native registration; `@effect/platform-browser` `BrowserStream.fromEventListenerWindow` for the `online`/`offline` ingress (both keyed on `WindowEventMap`); the native `SyncManager` for the background wake where present.
- Growth: a new connectivity-derived signal lands as one projection over the `online` cell, never a parallel probe; a `NetworkInformation` effective-type/save-data refinement (the experimental `navigator.connection`, absent from the DOM lib) lands as one row over a precise `Navigator & { connection: ... }` boundary refinement feeding the same cell, never a second connectivity owner.
- Boundary: `Connectivity` is the single online/offline owner — `capability-rank.md`'s rank input, `background-sync-replay.md`'s redial drain, `feature-flags`'s reconnect fallback, and `observability`'s connectivity gauge each read this one cell, so a private `navigator.onLine` read at any consumer is the named per-component-probe defect; the cell is advanced only by the merged event fold and the seed; the native `SyncManager` registration and the `redials` edge both feed the one downstream drain fold, never a second SW-resident queue; `Connectivity` emits no command, dials no transport, and `ui` reads the connectivity cell through the `AtomBinding`, never importing `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import { Effect, Option, Stream, SubscriptionRef } from "effect";
import * as BrowserStream from "@effect/platform-browser/BrowserStream";

// --- [SERVICES] ------------------------------------------------------------------------
interface Connectivity {
  readonly online: SubscriptionRef.SubscriptionRef<boolean>;
  readonly redials: Stream.Stream<void>;
  readonly registerNativeSync: Effect.Effect<void>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
const onlineEvents: Stream.Stream<boolean> = Stream.merge(
  BrowserStream.fromEventListenerWindow("online").pipe(Stream.as(true)),
  BrowserStream.fromEventListenerWindow("offline").pipe(Stream.as(false)),
);

// --- [COMPOSITION] ---------------------------------------------------------------------
class ConnectivityLive extends Effect.Service<ConnectivityLive>()("@rasm/ts/platform/Connectivity", {
  scoped: Effect.gen(function* () {
    const online = yield* SubscriptionRef.make<boolean>(navigator.onLine);

    const registerNativeSync: Effect.Effect<void> = Effect.promise(() => navigator.serviceWorker.ready).pipe(
      Effect.flatMap((reg) =>
        "sync" in reg
          ? Effect.tryPromise(() =>
              (reg as ServiceWorkerRegistration & { sync: SyncManager }).sync.register("rasm-offline-drain"),
            ).pipe(Effect.ignore)
          : Effect.void,
      ),
      Effect.ignore,
    );

    yield* onlineEvents.pipe(
      Stream.mapEffect((next) => SubscriptionRef.set(online, next)),
      Stream.runDrain,
      Effect.forkScoped,
    );

    const redials: Stream.Stream<void> = online.changes.pipe(
      Stream.zipWithPrevious,
      Stream.filterMap(([prev, next]) =>
        Option.isSome(prev) && !prev.value && next ? Option.some<void>(undefined) : Option.none<void>(),
      ),
    );

    return { online, redials, registerNativeSync } satisfies Connectivity;
  }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------
export { type Connectivity, ConnectivityLive };
```
