# [PLATFORM_RANK]

One page owns the browser capability-rank spine — `CapabilityRank`, one closed ordered `Rank` vocabulary (`Full`/`Degraded`/`OfflineOnly`/`Draining`) carrying its retained-capability set and its recovery window as data, folded from the five health inputs (connectivity, service-worker phase, session status, projection availability, persistent-storage grant) into one `RankState` cell under escalate-fast/recover-slow hysteresis. It is the one capability cell `ui` reads — `CapabilityRank` PROJECTS capability and holds no behavior, the god-object guard, so a consumer asks whether a capability is retained, never which rank holds. The fold is the read-side mirror of the C#-side health-rank fold; the page authors no decode and crosses no wire.

## [1]-[INDEX]

- [1]-[CAPABILITY_RANK]: the closed `Rank` vocabulary, the four-input candidate fold, and the hysteresis advance.

## [2]-[CAPABILITY_RANK]

- Owner: `CapabilityRank`, the single capability-rank owner — one `Rank` `as const satisfies Record` vocabulary keyed by the ordered rank name, each row carrying the `ordinal` severity, the `window` consecutive-calm count gating recovery, and the `retained` capability set, plus one `RankState` `SubscriptionRef` carrying the current rank and the calm counter. The retained-capability set is the one rank-to-capability table and a per-input boolean `degraded` flag scattered at a consumer is the named per-component-flag defect.
- Cases: the five health inputs fold to a candidate rank through `candidateRank` — the worst-of join over the connectivity edge (`Runtime/connectivity.md`'s `online` cell), the service-worker phase (`Shell/serviceworker.md`'s `SwLifecycle` cell), the session status (`Session/session.md`'s `SessionStatus`), the projection availability (`projection`'s `AvailabilityStore`), and the persistent-storage grant (`Shell/capability.md`'s `BrowserCapability` per-kind `PermissionState` for `persistent-storage`, a denied grant degrading toward `OfflineOnly` since the IndexedDB store risks eviction under memory pressure) — each input projecting to its worst-admissible rank and `Order.max` over the `ordinal` column reducing the five to the dominant candidate, never a hand-listed `if` ladder; the `advance` fold is asymmetric — an escalation (a worse candidate) sets the new rank immediately with the calm counter reset, a recovery (a better candidate) lowers exactly one rank only after the current rank's `window` of consecutive calm evaluations elapses, and a same-rank candidate resets calm — so a flapping input cannot oscillate the cell and each transition is one `RankState` swap; `retains` is the one capability query a consumer reads — one `SubscriptionRef.get` over the rank cell projected to membership in the current rank's `retained` set — so `ui` gates an offline-only affordance by asking `retains("command-dispatch")` rather than reading the rank ordinal, the read riding the effect channel because the live cell is never read off-effect.
- Auto: the candidate fold subscribes to the five input cells' `changes` streams merged into one `Stream`, recomputing the candidate on each input edge and threading the `advance` hysteresis through `SubscriptionRef.update` so the cell mutates only on a real rank change, the recompute forked `Effect.forkScoped` for the runtime lifetime; the connectivity input is the `Runtime/connectivity.md` `online` cell (not a private `navigator.onLine` read), so the redial that drives `Shell/sync.md`'s drain and the rank that gates `ui` read one connectivity owner, never two; the persistent-storage input is the `BrowserCapability` `persistent-storage` `PermissionState` cell, not a private `navigator.storage.persist` probe.
- Packages: `effect` `SubscriptionRef` for the rank cell, `Order.max`/`Order.mapInput` for the worst-of candidate join over the `ordinal` column, `Match` for each input's worst-rank projection, `Stream.merge`/`Effect.forkScoped` for the five-input recompute, and the `Record`/`keyof typeof` rank vocabulary; the five input cells are read from `connectivity`, `offline-cache`, `identity-session`, `projection`, and `capabilities` (the `BrowserCapability` `persistent-storage` `PermissionState` cell) as settled owners.
- Growth: a new rank lands as one row on the `Rank` vocabulary carrying its `ordinal`, `window`, and `retained` set, the `keyof typeof` discriminant absorbing it; a new health input lands as one worst-rank projection arm and one merged-stream source on the candidate fold, never a parallel rank cell; a new gated capability lands as one literal in the affected ranks' `retained` sets, the `retains` query unchanged.
- Boundary: `CapabilityRank` PROJECTS capability and holds no behavior — it reads the five health-input cells and exposes the rank cell and the `retains` query, so a behavior method (a retry, a redial, a flush) authored here is the named god-object defect; the rank is advanced only by the hysteresis fold and never mutated by a consumer; the connectivity input is the `connectivity` owner's cell, never a private `navigator.onLine`; `ui` reads the rank cell and the `retains` query through the `AtomBinding` and never imports `platform`; `CapabilityRank` emits no command and dials no transport.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { SwLifecycle } from "../offline-cache/service-worker-host.ts";
import type { SessionStatus } from "../identity-session/auth-session.ts";
import type { PermissionState } from "../capabilities/browser-capability.ts";
import { rankInput } from "../capabilities/permission-grant-fold.ts";
import { Effect, Match, Order, Stream, SubscriptionRef } from "effect";
import { ConnectivityLive } from "../connectivity/connectivity.ts";
import { ServiceWorkerHostLive } from "../offline-cache/service-worker-host.ts";
import { AuthSessionLive } from "../identity-session/auth-session.ts";
import { BrowserCapabilityLive } from "../capabilities/browser-capability.ts";

// --- [TYPES] ---------------------------------------------------------------------------
const Rank = {
  Full: { ordinal: 0, window: 0, retained: ["live-feed", "command-dispatch", "offline-queue", "render"] },
  Degraded: { ordinal: 1, window: 2, retained: ["command-dispatch", "offline-queue", "render"] },
  OfflineOnly: { ordinal: 2, window: 3, retained: ["offline-queue", "render"] },
  Draining: { ordinal: 3, window: 4, retained: ["render"] },
} as const satisfies Record<string, { ordinal: number; window: number; retained: ReadonlyArray<string> }>;

type RankKey = keyof typeof Rank;
type Capability = (typeof Rank)[RankKey]["retained"][number];

// --- [MODELS] --------------------------------------------------------------------------
interface RankState {
  readonly current: RankKey;
  readonly calm: number;
}

interface HealthInputs {
  readonly online: boolean;
  readonly sw: SwLifecycle;
  readonly session: SessionStatus;
  readonly available: boolean;
  readonly storage: PermissionState;
}

// --- [SERVICES] ------------------------------------------------------------------------
interface CapabilityRank {
  readonly rank: SubscriptionRef.SubscriptionRef<RankState>;
  readonly retains: (capability: Capability) => Effect.Effect<boolean>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
const byOrdinal: Order.Order<RankKey> = Order.mapInput(Order.number, (key: RankKey) => Rank[key].ordinal);

const candidateRank = (inputs: HealthInputs): RankKey =>
  [
    inputs.online ? ("Full" as RankKey) : ("OfflineOnly" as RankKey),
    Match.value(inputs.sw).pipe(
      Match.tags({ Active: () => "Full" as RankKey, Reloading: () => "Draining" as RankKey }),
      Match.orElse(() => "Degraded" as RankKey),
    ),
    Match.value(inputs.session).pipe(
      Match.tags({ Authenticated: () => "Full" as RankKey, Authenticating: () => "Degraded" as RankKey }),
      Match.orElse(() => "OfflineOnly" as RankKey),
    ),
    inputs.available ? ("Full" as RankKey) : ("Degraded" as RankKey),
    rankInput(inputs.storage),
  ].reduce((worst, hit) => Order.max(byOrdinal)(worst, hit), "Full" as RankKey);

const advance = (state: RankState, candidate: RankKey): RankState => {
  const delta = Order.greaterThan(byOrdinal)(candidate, state.current)
    ? 1
    : Order.lessThan(byOrdinal)(candidate, state.current)
      ? -1
      : 0;
  return delta > 0
    ? { current: candidate, calm: 0 }
    : delta < 0 && state.calm + 1 >= Rank[state.current].window
      ? { current: lowerRank(state.current), calm: 0 }
      : delta < 0
        ? { ...state, calm: state.calm + 1 }
        : { ...state, calm: 0 };
};

const lowerRank = (key: RankKey): RankKey =>
  (Object.keys(Rank) as ReadonlyArray<RankKey>).reduce((best, candidate) =>
    Rank[candidate].ordinal === Rank[key].ordinal - 1 ? candidate : best, key);

// --- [COMPOSITION] ---------------------------------------------------------------------
class CapabilityRankLive extends Effect.Service<CapabilityRankLive>()("@rasm/ts/platform/CapabilityRank", {
  scoped: Effect.gen(function* () {
    const connectivity = yield* ConnectivityLive;
    const sw = yield* ServiceWorkerHostLive;
    const session = yield* AuthSessionLive;
    const capability = yield* BrowserCapabilityLive;
    const storageCell = capability.storageRetained;
    const rank = yield* SubscriptionRef.make<RankState>({ current: "Full", calm: 0 });

    const inputs: Effect.Effect<HealthInputs> = Effect.all({
      online: SubscriptionRef.get(connectivity.online),
      sw: SubscriptionRef.get(sw.lifecycle),
      session: SubscriptionRef.get(session.status),
      available: Effect.succeed(true),
      storage: SubscriptionRef.get(storageCell),
    });

    yield* Stream.merge(
      Stream.merge(connectivity.online.changes, storageCell.changes),
      Stream.merge(sw.lifecycle.changes, session.status.changes),
    ).pipe(
      Stream.mapEffect(() =>
        inputs.pipe(Effect.flatMap((live) => SubscriptionRef.update(rank, (state) => advance(state, candidateRank(live))))),
      ),
      Stream.runDrain,
      Effect.forkScoped,
    );

    const retains = (capability: Capability): Effect.Effect<boolean> =>
      SubscriptionRef.get(rank).pipe(
        Effect.map((state) => (Rank[state.current].retained as ReadonlyArray<string>).includes(capability)),
      );

    return { rank, retains } satisfies CapabilityRank;
  }),
  dependencies: [ConnectivityLive.Default, ServiceWorkerHostLive.Default, AuthSessionLive.Default, BrowserCapabilityLive.Default],
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------
export { type CapabilityRank, type RankKey, CapabilityRankLive };
```
