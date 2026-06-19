# [PLATFORM_SESSION_RECORDER]

One page owns the trace-correlated sampled session recorder — `SessionRecorder`, an Effect-scoped DOM/interaction recorder gated behind the `feature-flags/remote-config.md` `RemoteConfig` `session-replay` flag, sampled on the `RuntimeConfig` `replaySampleRate`, redacting `Redacted`/input fields at capture, holding a bounded event ring, and minting one replay-window id surfaced as a span attribute on the closed `observability/metric-registry.md` `MetricRegistry` `SpanName` axis. The window id is the one correlation handle `fault-capture`'s `crash.report` span and `web-vitals`' `web.vital.breach` span each annotate, so a shipped crash and a vital breach reconstruct against the exact recorded window over the one `SelfTelemetry` trace edge. No vendor replay SDK and no third telemetry path — the replay window is one `session.replay` span and one shared attribute, never a parallel collector. The page authors no decode and crosses no wire.

## [1]-[INDEX]

[SESSION_RECORDER]: the sampled scoped recorder, the redaction-at-capture fold, and the trace-correlated replay-window id.

## [2]-[SESSION_RECORDER]

- Owner: `SessionRecorder`, the single session-replay owner — the `RemoteConfig`-flag-and-sample gate, the scoped `MutationObserver`/interaction capture over `scopedEventStream`, the `RecordedEvent` redaction-at-capture fold into one bounded `Ref<Chunk<RecordedEvent>>` ring, the `windowId` `SubscriptionRef` the trace-correlated spans read, and the `session.replay` span ship over the `SelfTelemetry` edge. The window id is the one correlation handle and a second replay channel or a vendor recorder SDK is the named third-telemetry-path defect; redaction runs at capture and an unredacted node reaching the ring is the named leak defect.
- Cases: `SessionRecorder.record` is the one boot act — it evaluates the `session-replay` flag through `RemoteConfig.evaluate("session-replay", subjectKey)`, folds the `FlagEvaluation` through `$is("On")` to the recording gate, and rolls the per-session sample against `RuntimeConfig.replaySampleRate` once so a non-sampled session opens no observer and pays no capture cost; an in-sample session mints one `windowId` (a `crypto.randomUUID()` published into the `windowId` `SubscriptionRef`), opens a scoped `MutationObserver` over `document.documentElement` (`childList`/`attributes`/`characterData` with `subtree`) lifted through `scopedEventStream` and the pointer/keydown interaction stream lifted through the same bridge, redacts each captured node or interaction at capture through `redactEvent`, and appends the `RecordedEvent` to the bounded ring; `windowId` exposes `Option.none` for a non-sampled session and `Option.some(id)` while recording, so `CrashTelemetry`'s `crash.report` annotation and `PerformanceBudget`'s `web.vital.breach` annotation read the one cell and a fault outside a sampled window correlates to no window rather than a fabricated id.
- Auto: redaction is total at the only point the DOM node exists — `isRedactedTarget` gates an `input`/`textarea`/`contenteditable` value, a `password`/`email`/`tel` typed control, and any element carrying the `data-rasm-redact` opt-out attribute, and `redactText` projects a redacted node to the `"[redacted]"` placeholder inside the `MutationObserver`/interaction callbacks so the raw node text never reaches the projected `RecordedEvent`; `redactEvent` is the second-pass value-scrub fold over the already-projected event (the single-character `Keydown` key collapses to the placeholder so a typed secret never survives), the same redaction rule family `fault-capture/crash-telemetry.md`'s `CrashReport` sanitization fixes (bearer header, query-string secret, `Redacted` field) applied to the DOM-node surface rather than the fault envelope; the ring is a bounded `Ref<Chunk<RecordedEvent>>` `Chunk.takeRight`-sliced to the last `REPLAY_RING` events so a long session never grows an unbounded buffer; the window flushes as one `MetricRegistry.span("session.replay", ...)` carrying the `windowId`, the sampled event count, the route, and the per-event `_tag` digest on the `Hidden`/`Draining` `AppLifecycle` `transitions` edge and on an explicit `flush` driven by a correlated fault, so the capture fiber and the flush ride one scope forked `Effect.forkScoped` for the recording lifetime.
- Packages: `effect` `Data.TaggedEnum` for the `RecordedEvent` family, `Chunk`/`Ref` for the bounded ring, `SubscriptionRef` for the `windowId` cell, `Option` for the non-sampled absence, and `Effect.acquireRelease`/`Effect.forkScoped` for the scoped capture; `runtime-composition/scoped-event-stream.md` `scopedEventStream` for the `MutationObserver` and interaction ingress (a `MutationObserver` and the pointer/keydown targets are absent from `WindowEventMap`, so the generic bridge is the ingress); `feature-flags/remote-config.md` `RemoteConfig` for the flag gate; `runtime-config/runtime-config.md` `RuntimeConfig` for the sample rate; `runtime-composition/app-lifecycle.md` `AppLifecycle` for the terminal-flush edge; the `observability/metric-registry.md` `MetricRegistry` `span` over the `SelfTelemetry` edge; native `crypto.randomUUID` for the window id.
- Growth: a new captured event kind lands as one `RecordedEvent` `Data.TaggedEnum` arm and one redaction rule on `redactEvent`, never a parallel recorder; a new redaction rule lands as one row on the `redactEvent` fold aligned to the `CrashReport` sanitization family; a tightened sampling policy lands as one `replaySampleRate` read, never a per-event sampling flag; a correlated-span attribute beyond `crash.report`/`web.vital.breach` lands as one `windowId` read on the new sink, never a second window cell.
- Boundary: `SessionRecorder` is the single session-replay owner gated behind the `RemoteConfig` `session-replay` flag — the sampling rate and the redaction are owned in-platform, so a vendor replay SDK or a direct collector POST is the named defect; the replay window is one `session.replay` span on the closed `MetricRegistry` `SpanName` axis and the `windowId` is one shared span attribute `crash.report`/`web.vital.breach` annotate, never a third telemetry path; redaction runs at capture and an unredacted `Redacted`/input value reaching the ring is the named leak defect; the recorder reads the `RemoteConfig` flag and the `AppLifecycle` edge and never mutates either, emits no command, dials no transport beyond the `SelfTelemetry` ship, and `ui` reads no recorder state and never imports `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { Scope } from "effect";
import { Chunk, Data, Effect, Layer, Option, Ref, Stream, SubscriptionRef } from "effect";
import { FlagEvaluation, RemoteConfig } from "../feature-flags/remote-config.ts";
import { RuntimeConfig } from "../runtime-config/runtime-config.ts";
import { AppLifecycleLive, Phase } from "../runtime-composition/app-lifecycle.ts";
import { MetricRegistry } from "../observability/metric-registry.ts";
import { scopedEventStream } from "../runtime-composition/scoped-event-stream.ts";

// --- [TYPES] ---------------------------------------------------------------------------
type RecordedEvent = Data.TaggedEnum<{
  readonly Mutation: { readonly target: string; readonly text: string; readonly at: number };
  readonly Pointer: { readonly target: string; readonly x: number; readonly y: number; readonly at: number };
  readonly Keydown: { readonly target: string; readonly key: string; readonly at: number };
}>;
const RecordedEvent = Data.taggedEnum<RecordedEvent>();

// --- [CONSTANTS] -----------------------------------------------------------------------
const REPLAY_RING: number = 256;
const REDACT_PLACEHOLDER: string = "[redacted]";
const REDACTED_INPUT_TYPES: ReadonlySet<string> = new Set<string>(["password", "email", "tel"]);

// --- [SERVICES] ------------------------------------------------------------------------
interface SessionRecorder {
  readonly windowId: SubscriptionRef.SubscriptionRef<Option.Option<string>>;
  readonly record: (subjectKey: string) => Effect.Effect<void, never, Scope.Scope>;
  readonly flush: Effect.Effect<void>;
}
const SessionRecorder = Effect.Tag("@rasm/ts/platform/SessionRecorder")<SessionRecorder, SessionRecorder>();

// --- [OPERATIONS] ----------------------------------------------------------------------
const isRedactedTarget = (el: Element): boolean =>
  el.hasAttribute("data-rasm-redact") ||
  el.tagName === "TEXTAREA" ||
  (el as HTMLElement).isContentEditable ||
  (el.tagName === "INPUT" && REDACTED_INPUT_TYPES.has((el as HTMLInputElement).type));

const labelOf = (node: Node): string =>
  node instanceof Element ? `${node.tagName.toLowerCase()}${node.id ? `#${node.id}` : ""}` : node.nodeName;

const redactText = (node: Node): string =>
  node instanceof Element && isRedactedTarget(node) ? REDACT_PLACEHOLDER : (node.textContent ?? "").slice(0, 128);

const redactEvent = (event: RecordedEvent): RecordedEvent =>
  RecordedEvent.$match(event, {
    Mutation: (m) => RecordedEvent.Mutation(m),
    Pointer: (p) => RecordedEvent.Pointer(p),
    Keydown: (k) => RecordedEvent.Keydown({ ...k, key: k.key.length === 1 ? REDACT_PLACEHOLDER : k.key }),
  });

const mutationEvents: Stream.Stream<RecordedEvent> = scopedEventStream<RecordedEvent, MutationObserver>(
  (emit) => {
    const observer = new MutationObserver((records) => {
      for (const record of records) {
        const target = record.target;
        emit(RecordedEvent.Mutation({ target: labelOf(target), text: redactText(target), at: Date.now() }));
      }
    });
    observer.observe(document.documentElement, { childList: true, attributes: true, characterData: true, subtree: true });
    return observer;
  },
  (observer) => observer.disconnect(),
);

const interactionEvents: Stream.Stream<RecordedEvent> = Stream.merge(
  scopedEventStream<RecordedEvent, (e: PointerEvent) => void>(
    (emit) => {
      const fn = (e: PointerEvent): void =>
        emit(RecordedEvent.Pointer({ target: labelOf(e.target as Node), x: e.clientX, y: e.clientY, at: Date.now() }));
      window.addEventListener("pointerdown", fn);
      return fn;
    },
    (fn) => window.removeEventListener("pointerdown", fn),
  ),
  scopedEventStream<RecordedEvent, (e: KeyboardEvent) => void>(
    (emit) => {
      const fn = (e: KeyboardEvent): void =>
        emit(
          RecordedEvent.Keydown({
            target: labelOf(e.target as Node),
            key: e.target instanceof Element && isRedactedTarget(e.target) ? REDACT_PLACEHOLDER : e.key,
            at: Date.now(),
          }),
        );
      window.addEventListener("keydown", fn);
      return fn;
    },
    (fn) => window.removeEventListener("keydown", fn),
  ),
);

// --- [COMPOSITION] ---------------------------------------------------------------------
const makeSessionRecorder: Effect.Effect<SessionRecorder, never, RuntimeConfig | RemoteConfig | MetricRegistry | AppLifecycleLive> =
  Effect.gen(function* () {
    const config = yield* RuntimeConfig;
    const remote = yield* RemoteConfig;
    const registry = yield* MetricRegistry;
    const lifecycle = yield* AppLifecycleLive;
    const windowId = yield* SubscriptionRef.make<Option.Option<string>>(Option.none());
    const ring = yield* Ref.make<Chunk.Chunk<RecordedEvent>>(Chunk.empty());

    const flush: Effect.Effect<void> = Effect.all({ id: SubscriptionRef.get(windowId), events: Ref.get(ring) }).pipe(
      Effect.flatMap(({ id, events }) =>
        Option.match(id, {
          onNone: () => Effect.void,
          onSome: (replayWindow) =>
            registry.span(
              "session.replay",
              Effect.annotateCurrentSpan({
                replayWindow,
                eventCount: Chunk.size(events),
                route: window.location.pathname,
                tags: Chunk.toReadonlyArray(events).map((e) => e._tag),
              }),
            ).pipe(Effect.asVoid),
        }),
      ),
    );

    const record = (subjectKey: string): Effect.Effect<void, never, Scope.Scope> =>
      Effect.all({
        evaluation: remote.evaluate("session-replay", subjectKey),
        rate: config.replaySampleRate.pipe(Effect.orElseSucceed(() => 0)),
      }).pipe(
        Effect.flatMap(({ evaluation, rate }) =>
          FlagEvaluation.$is("On")(evaluation) && Math.random() < rate
            ? SubscriptionRef.set(windowId, Option.some(crypto.randomUUID())).pipe(
                Effect.zipRight(
                  Stream.merge(mutationEvents, interactionEvents).pipe(
                    Stream.map(redactEvent),
                    Stream.mapEffect((event) => Ref.update(ring, (log) => Chunk.takeRight(Chunk.append(log, event), REPLAY_RING))),
                    Stream.runDrain,
                    Effect.forkScoped,
                  ),
                ),
                Effect.zipRight(
                  lifecycle.transitions.pipe(
                    Stream.filter((phase) => Phase.$is("Hidden")(phase) || Phase.$is("Draining")(phase)),
                    Stream.mapEffect(() => flush),
                    Stream.runDrain,
                    Effect.forkScoped,
                  ),
                ),
                Effect.asVoid,
              )
            : Effect.void,
        ),
      );

    return SessionRecorder.of({ windowId, record, flush });
  });

const SessionRecorderLive: Layer.Layer<SessionRecorder, never, RuntimeConfig | RemoteConfig | MetricRegistry | AppLifecycleLive> =
  Layer.scoped(SessionRecorder, makeSessionRecorder);

// --- [EXPORTS] -------------------------------------------------------------------------
export { type RecordedEvent, type SessionRecorder, SessionRecorder, SessionRecorderLive };
```
