# [PLATFORM_LIFECYCLE]

One page owns the browser page-lifecycle spine — `AppLifecycle`, one closed `Phase` `Data.TaggedEnum` (`Booting`/`Running`/`Hidden`/`Frozen`/`Draining`/`Stopped`) advanced by one fold over the merged `visibilitychange`, `pagehide` (`persisted: true` is the freeze edge), and `beforeunload` ingresses, held in one `SubscriptionRef`. It is the single lifecycle axis `web-vitals`'s terminal-flush, `feature-flags`'s foreground-refresh, and `fault-capture`'s drain each project from, replacing the three private `visibilitychange` ingresses those owners each opened. The fold is total over the merged native events; the page authors no decode and crosses no wire.

## [1]-[INDEX]

- [1]-[APP_LIFECYCLE]: the closed `Phase` enum and the merged page-lifecycle fold.

## [2]-[APP_LIFECYCLE]

- Owner: `AppLifecycle`, the single page-lifecycle owner — one `Phase` `Data.TaggedEnum` cell in a `SubscriptionRef`, advanced only by the merged lifecycle-event fold and the boot/stop edges, every up/hidden/frozen/draining question a projection of the one cell. `Phase` is the closed lifecycle vocabulary and a private `visibilitychange` listener opened at a host owner is the named three-ingress defect this owner retires.
- Cases: `Phase` carries the six page-lifecycle states (`Booting` at composition, `Running` while visible-and-active, `Hidden` on `visibilitychange` to `hidden`, `Frozen` on the `pagehide` freeze edge where `persisted` is `true` so the bfcache holds the document, `Draining` on `beforeunload` as the one terminal-flush window, `Stopped` after drain), advanced by `advancePhase` — one total fold keyed by the native event into the matching `Phase` constructor, so a `visibilitychange`-to-`visible` after `Hidden`/`Frozen` resumes to `Running`, a `pagehide` with `persisted: false` is the unload path folding to `Draining`, and a `pagehide` with `persisted: true` is the freeze path folding to `Frozen`; the `transitions` `Stream` projects each distinct phase edge so a consumer subscribes to the exact edge it flushes on (`web-vitals` to `Hidden`/`Draining`, `fault-capture` to `Draining`) rather than re-deriving the hidden-state test, and `phase` exposes the cell for a point read.
- Auto: the ingress merges three `BrowserStream.fromEventListenerWindow`/`fromEventListenerDocument` streams — `fromEventListenerDocument("visibilitychange")` (`Event`, reading `document.visibilityState`), `fromEventListenerWindow("pagehide")` (`PageTransitionEvent`, the `persisted` flag the freeze-versus-unload discriminant), and `fromEventListenerWindow("beforeunload")` (`BeforeUnloadEvent`, the terminal drain edge) — into one `Stream.merge`, each native event lifting to a `LifecycleSignal` `Data.TaggedEnum` arm through `signalOf` before the `advancePhase` fold, so the three formerly-private ingresses are one merged capture forked `Effect.forkScoped` for the runtime lifetime; the dedicated Page Lifecycle `freeze`/`resume` document events (absent from `DocumentEventMap`) are the designed-only refinement of the `pagehide`-`persisted` freeze edge, landing as one `signalOf` arm over a precise event refinement, never a parallel lifecycle cell.
- Packages: `effect` `Data.TaggedEnum` for the `Phase` and `LifecycleSignal` closed families, `SubscriptionRef` for the phase cell, `Match.tagsExhaustive` for the total advance fold, and `Stream.merge`/`Effect.forkScoped` for the merged ingress; `@effect/platform-browser` `BrowserStream.fromEventListenerWindow`/`fromEventListenerDocument` for the three native event sources (all keyed over `WindowEventMap`/`DocumentEventMap`).
- Growth: a new lifecycle state lands as one `Phase` arm breaking every `advancePhase` and consumer `$match` at compile time; a new lifecycle trigger lands as one `LifecycleSignal` arm and one `signalOf` row over its native event; the dedicated `freeze`/`resume` Page Lifecycle events land as one `signalOf` arm over a precise `Document & { ... }` event refinement, never a second lifecycle owner.
- Boundary: `AppLifecycle` is the single page-lifecycle axis — `Observability/vitals.md`'s `visibilityState`-hidden terminal CLS/INP flush, `Config/flags.md`'s foreground-refresh trigger, and `Observability/crash.md`'s drain each subscribe to the `transitions` stream or read the `phase` cell, so a private `visibilitychange`/`pagehide` listener reopened at any of those owners is the named three-ingress defect; the cell is advanced only by the fold and the boot/stop edges and never mutated by a consumer; `AppLifecycle` emits no command, dials no transport, and `ui` reads a lifecycle-derived affordance through the `AtomBinding`, never importing `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import { Data, Effect, Match, Stream, SubscriptionRef } from "effect";
import * as BrowserStream from "@effect/platform-browser/BrowserStream";

// --- [TYPES] ---------------------------------------------------------------------------
type Phase = Data.TaggedEnum<{
  readonly Booting: object;
  readonly Running: object;
  readonly Hidden: object;
  readonly Frozen: object;
  readonly Draining: object;
  readonly Stopped: object;
}>;
const Phase = Data.taggedEnum<Phase>();

type LifecycleSignal = Data.TaggedEnum<{
  readonly Visible: object;
  readonly Concealed: object;
  readonly Freeze: object;
  readonly Unload: object;
}>;
const LifecycleSignal = Data.taggedEnum<LifecycleSignal>();

// --- [SERVICES] ------------------------------------------------------------------------
interface AppLifecycle {
  readonly phase: SubscriptionRef.SubscriptionRef<Phase>;
  readonly transitions: Stream.Stream<Phase>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
const signalOf = (event: Event): LifecycleSignal =>
  event.type === "pagehide"
    ? (event as PageTransitionEvent).persisted
      ? LifecycleSignal.Freeze()
      : LifecycleSignal.Unload()
    : event.type === "beforeunload"
      ? LifecycleSignal.Unload()
      : document.visibilityState === "hidden"
        ? LifecycleSignal.Concealed()
        : LifecycleSignal.Visible();

const advancePhase = (signal: LifecycleSignal): Phase =>
  Match.value(signal).pipe(
    Match.tagsExhaustive({
      Visible: () => Phase.Running(),
      Concealed: () => Phase.Hidden(),
      Freeze: () => Phase.Frozen(),
      Unload: () => Phase.Draining(),
    }),
  );

const lifecycleSignals: Stream.Stream<LifecycleSignal> = Stream.merge(
  BrowserStream.fromEventListenerDocument("visibilitychange"),
  Stream.merge(
    BrowserStream.fromEventListenerWindow("pagehide"),
    BrowserStream.fromEventListenerWindow("beforeunload"),
  ),
).pipe(Stream.map(signalOf));

// --- [COMPOSITION] ---------------------------------------------------------------------
class AppLifecycleLive extends Effect.Service<AppLifecycleLive>()("@rasm/ts/platform/AppLifecycle", {
  scoped: Effect.gen(function* () {
    const phase = yield* SubscriptionRef.make<Phase>(Phase.Booting());
    yield* lifecycleSignals.pipe(
      Stream.mapEffect((signal) => SubscriptionRef.set(phase, advancePhase(signal))),
      Stream.runDrain,
      Effect.forkScoped,
    );
    yield* SubscriptionRef.set(phase, Phase.Running());
    return { phase, transitions: phase.changes } satisfies AppLifecycle;
  }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------
export { type AppLifecycle, AppLifecycleLive, Phase };
```
