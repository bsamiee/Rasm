# [PLATFORM_CRASH]

One page owns the browser crash/fault sink — `CrashTelemetry`, the Effect-native fault sink capturing the global `error`/`unhandledrejection` stream, marshalling each uncaught fault into the `interchange` `FaultDetail` family through the total `crashFaultOf` projection, holding the breadcrumb ring, and shipping a sanitized `CrashReport` through the `observability` `MetricRegistry` span over the `SelfTelemetry` collector edge. An uncaught fault reconstructs as a `FaultDetail` arm — never a parallel error type — and the collector is the only telemetry path. The page holds no domain state and authors no decode.

## [1]-[INDEX]

- [1]-[CRASH_TELEMETRY]: the global fault capture, the crash fold, and the recovery cell.

## [2]-[CRASH_TELEMETRY]

- Owner: `CrashTelemetry`, the single crash sink — the global `error`/`unhandledrejection` capture folded into the `interchange` `FaultDetail` family, the breadcrumb ring buffer, the `CrashReport` fold shipped through `SelfTelemetry`, and the recovery-affordance `SubscriptionRef` the `ui` error fallback reads on each fallback render (the binding samples the cell through `SubscriptionRef.get`; the `SubscriptionRef` shape leaves a live `changes` subscription open for a future always-mounted recovery surface). `FaultDetail` is the one fault family and a parallel `CrashError`/`AppError` type is the named const-spam defect.
- Cases: `CrashTelemetry.installGlobalHandlers` is the one boot effect `composition-root`'s `bootSpa` runs — it merges `BrowserStream.fromEventListenerWindow("error")` (`ErrorEvent`) and `BrowserStream.fromEventListenerWindow("unhandledrejection")` (`PromiseRejectionEvent`) into one capture stream forked `Effect.forkDaemon` so it outlives `bootSpa`'s local scope and lives for the SPA runtime (a `forkScoped` daemon interrupts the listener the instant `bootSpa` returns, the deleted form), and `mapEffect`s each event into `capture` reading the rejection `reason` or the error-event `error`; `capture` reconstructs each uncaught fault as a `FaultDetail` arm through `crashFaultOf` — a thrown `Error` or rejected reason becomes `FaultDetail.HopFault({ reason: "uncaught", evidence })` against the `interchange`-owned closed `HopReason` vocabulary carrying the sanitized message and a stack digest, and a value already shaped as a `FaultDetail` re-surfaces as itself rather than re-wrapping — so the capture path is total, the browser-origin local hop carries a typed `reason` not a numeric `code`, and an uncaught fault is never a bare `Error` escaping to the console; the breadcrumb ring buffer is a bounded `Ref`-backed array of the last navigation, command-dispatch, and lifecycle events sliced to the last 32, attached to each `CrashReport`, and `breadcrumb` is the one writer every infra owner (the router transition, the gateway dispatch, the SW lifecycle) feeds.
- Auto: the `CrashReport` fold sanitizes the exception envelope — it strips the bearer header, query-string secrets, and any `Redacted` field (the route key reads `window.location.pathname` with its query string dropped), retains the `FaultDetail` `_tag`/`evidence` and the owner-total `renderFault` projection (`correlation` rides only where the arm carries it — only the `ComputeFault`/`StoreFault` cases do; the browser-origin `HopFault` this page constructs carries a typed `reason`, never a numeric `code`), the breadcrumb trail, the route key, the build mode read from `RuntimeConfig.buildMode`, the user-agent, and the `Observability/replay.md` `SessionRecorder` `windowId` as the one trace-correlated `replayWindow` annotation (the recorder window flushed first so the shipped crash reconstructs against the recorded session), and ships it as one `MetricRegistry.span("crash.report", ...)` through the `SelfTelemetry` export edge, never a third telemetry path and never a direct collector POST; the report ships at most once per distinct `(renderFault, stackDigest)` dedupe key held in a session `Ref` set so a render-loop fault does not flood the collector — the `renderFault` discriminant is total over the heterogeneous fault payloads where a raw `code` field is not, so the dedupe never assumes a field a `HopFault` lacks; the `crashFaultOf` guard narrows `_tag` against the closed `FaultTag` set (`interchange` `Ingress/fault#FAULT_FAMILY`), never a loose structural probe, so a render `ErrorInfo` is not trusted back as a `FaultDetail`.
- Packages: `@effect/platform-browser` `BrowserStream.fromEventListenerWindow` for the global `error`/`unhandledrejection` ingress as a `Stream`; `effect` for the `FaultDetail` reconstruction (`Match` over the captured shape), the breadcrumb `Ref` buffer, the recovery `SubscriptionRef`, the dedupe `Ref` set, and the `Option` replay-window read; the `observability` `MetricRegistry` `span` over the `SelfTelemetry` edge for the collector ship; the `Observability/replay.md` `SessionRecorder` `windowId` cell for the trace-correlated replay-window annotation and `flush` for the on-crash window ship.
- Growth: a new fault source lands as one arm on the `crashFaultOf` reconstruction, mapping into the existing `FaultDetail` family, never a new error type; a new breadcrumb kind lands as one literal on the breadcrumb event axis; a new sanitization rule lands as one row on the `CrashReport` fold; a new recovery affordance lands as one row on the recovery cell; a new correlated telemetry attribute lands as one annotation on the existing `crash.report` span reading the one owning cell, never a second telemetry path.
- Boundary: uncaught faults reconstruct as the `interchange` `FaultDetail` family and never a parallel error type — the exhaustive-fault-family law reaches the crash boundary; the `CrashReport` ships as a `MetricRegistry.span("crash.report", ...)` over the `SelfTelemetry` collector edge, the only telemetry path `observability` fixes, so a direct collector POST or a second crash-reporter SDK is the named defect; the recovery-affordance cell is read by the `ui` error fallback through the one `AtomBinding`, never a second state binding; `CrashTelemetry` emits no command and dials no transport; `ui` reads the recovery cell and never imports `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import { Data, Effect, Layer, Match, Option, Predicate, Ref, Stream, SubscriptionRef } from "effect";
import * as BrowserStream from "@effect/platform-browser/BrowserStream";
import { type FaultDetail, type FaultTag, FaultDetail as Fault, renderFault } from "../interchange/fault-family.ts";
import { MetricRegistry } from "../observability/metric-registry.ts";
import { RuntimeConfig } from "../runtime-config/runtime-config.ts";
import { SessionRecorder } from "../session-replay/session-recorder.ts";

// --- [TYPES] ---------------------------------------------------------------------------
type Breadcrumb = Data.TaggedEnum<{
  readonly Navigation: { readonly route: string; readonly at: number };
  readonly Command: { readonly intent: string; readonly at: number };
  readonly Lifecycle: { readonly phase: string; readonly at: number };
}>;
const Breadcrumb = Data.taggedEnum<Breadcrumb>();

type Recovery = Data.TaggedEnum<{
  readonly Healthy: {};
  readonly Crashed: { readonly fault: FaultDetail; readonly canRetry: boolean };
}>;
const Recovery = Data.taggedEnum<Recovery>();

// --- [CONSTANTS] -----------------------------------------------------------------------
const BREADCRUMB_RING: number = 32;
const STACK_DIGEST_BYTES: number = 256;

// --- [MODELS] --------------------------------------------------------------------------
interface CrashReport {
  readonly fault: FaultDetail;
  readonly stackDigest: string;
  readonly route: string;
  readonly buildMode: "development" | "production";
  readonly userAgent: string;
  readonly breadcrumbs: ReadonlyArray<Breadcrumb>;
  readonly replayWindow: Option.Option<string>;
}

// --- [SERVICES] ------------------------------------------------------------------------
interface CrashTelemetry {
  readonly recovery: SubscriptionRef.SubscriptionRef<Recovery>;
  readonly installGlobalHandlers: Effect.Effect<void>;
  readonly capture: (cause: unknown) => Effect.Effect<void>;
  readonly breadcrumb: (crumb: Breadcrumb) => Effect.Effect<void>;
  readonly recover: Effect.Effect<void>;
}
const CrashTelemetry = Effect.Tag("@rasm/ts/platform/CrashTelemetry")<CrashTelemetry, CrashTelemetry>();

// --- [OPERATIONS] ----------------------------------------------------------------------
const FAULT_TAGS: ReadonlySet<string> = new Set<FaultTag>(["ComputeFault", "StoreFault", "HopFault", "ConfigError", "Quarantine"]);
const isFaultDetail = (c: unknown): c is FaultDetail =>
  Predicate.isRecord(c) && "_tag" in c && Predicate.isString(c._tag) && FAULT_TAGS.has(c._tag) && "evidence" in c;

const crashFaultOf = (cause: unknown): FaultDetail =>
  Match.value(cause).pipe(
    Match.when(isFaultDetail, (c) => c),
    Match.when(Match.instanceOf(Error), (e) => Fault.HopFault({ reason: "uncaught", evidence: { message: e.message, name: e.name } })),
    Match.orElse((c) => Fault.HopFault({ reason: "uncaught", evidence: { message: String(c) } })),
  );

const stackDigestOf = (cause: unknown): string => (cause instanceof Error ? (cause.stack ?? "").slice(0, STACK_DIGEST_BYTES) : "");

const makeCrashTelemetry: Effect.Effect<CrashTelemetry, never, MetricRegistry | RuntimeConfig | SessionRecorder> = Effect.gen(function* () {
  const registry = yield* MetricRegistry;
  const config = yield* RuntimeConfig;
  const recorder = yield* SessionRecorder;
  const buildMode = yield* config.buildMode.pipe(Effect.orElseSucceed(() => "development" as const));
  const recovery = yield* SubscriptionRef.make<Recovery>(Recovery.Healthy());
  const crumbs = yield* Ref.make<ReadonlyArray<Breadcrumb>>([]);
  const seen = yield* Ref.make<ReadonlySet<string>>(new Set());

  const breadcrumb = (crumb: Breadcrumb): Effect.Effect<void> =>
    Ref.update(crumbs, (b) => [...b, crumb].slice(-BREADCRUMB_RING));

  const ship = (fault: FaultDetail, stackDigest: string): Effect.Effect<void> =>
    Effect.all({ breadcrumbs: Ref.get(crumbs), replayWindow: SubscriptionRef.get(recorder.windowId) }).pipe(
      Effect.tap(() => recorder.flush),
      Effect.map(({ breadcrumbs, replayWindow }): CrashReport => ({
        fault,
        stackDigest,
        route: window.location.pathname,
        buildMode,
        userAgent: navigator.userAgent,
        breadcrumbs,
        replayWindow,
      })),
      Effect.flatMap((report) =>
        registry.span(
          "crash.report",
          Effect.annotateCurrentSpan({
            faultTag: report.fault._tag,
            faultDetail: renderFault(report.fault),
            stackDigest: report.stackDigest,
            route: report.route,
            buildMode: report.buildMode,
            userAgent: report.userAgent,
            breadcrumbs: report.breadcrumbs.map((b) => b._tag),
            replayWindow: Option.getOrUndefined(report.replayWindow),
          }),
        )),
      Effect.asVoid,
    );

  const capture = (cause: unknown): Effect.Effect<void> =>
    Effect.suspend(() => {
      const fault = crashFaultOf(cause);
      const stackDigest = stackDigestOf(cause);
      const key = `${renderFault(fault)}:${stackDigest}`;
      return Ref.modify(seen, (s) => (s.has(key) ? [true, s] as const : [false, new Set([...s, key])] as const)).pipe(
        Effect.flatMap((dropped) =>
          dropped
            ? Effect.void
            : SubscriptionRef.set(recovery, Recovery.Crashed({ fault, canRetry: true })).pipe(Effect.zipRight(ship(fault, stackDigest)))),
      );
    });

  const installGlobalHandlers: Effect.Effect<void> = Stream.merge(
    BrowserStream.fromEventListenerWindow("error"),
    BrowserStream.fromEventListenerWindow("unhandledrejection"),
  ).pipe(
    Stream.mapEffect((ev) => capture("reason" in ev ? ev.reason : ev.error)),
    Stream.runDrain,
    Effect.forkDaemon,
    Effect.asVoid,
  );

  return CrashTelemetry.of({
    recovery,
    installGlobalHandlers,
    capture,
    breadcrumb,
    recover: SubscriptionRef.set(recovery, Recovery.Healthy()),
  });
});

// --- [COMPOSITION] ---------------------------------------------------------------------
const CrashTelemetryLive: Layer.Layer<CrashTelemetry, never, MetricRegistry | RuntimeConfig | SessionRecorder> = Layer.effect(CrashTelemetry, makeCrashTelemetry);

// --- [EXPORTS] -------------------------------------------------------------------------
export type { Breadcrumb, CrashReport, CrashTelemetry, Recovery };
export { CrashTelemetry, CrashTelemetryLive, crashFaultOf };
```
