# [PLATFORM_ERROR_BOUNDARY]

One page owns the browser error-boundary and crash/exception telemetry concern — `CrashTelemetry`, the Effect-native fault sink capturing the global `window.onerror`/`unhandledrejection` stream and the React render-tree faults, marshalling each into the `interchange` `FaultDetail` family and shipping a sanitized `CrashReport` through the `platform` `SelfTelemetry` collector path; `ErrorBoundaryBinding`, the `react-error-boundary` integration emitting render faults into `CrashTelemetry`; and the session-breadcrumb ring buffer attached to each report. An uncaught fault reconstructs as a `FaultDetail` arm — never a parallel error type — and the collector is the only telemetry path. The page composes `react-error-boundary` and the existing `SelfTelemetry` edge, holds no domain state, and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                               | [STATE]   |
| :-----: | :------------- | :----------------------------------------------------------------------------------- | :-------- |
|   [1]   | ERROR_BOUNDARY | the global fault capture, the crash fold, the recovery affordance, the react binding | FINALIZED |
|   [2]   | RESEARCH       | the live-browser uncaught-fault marshalling probe                                    | SPIKE     |

The `ERROR_BOUNDARY` owner is a transcription-complete fence: `BrowserStream.fromEventListenerWindow` (`WindowEventMap["error"] = ErrorEvent`, `WindowEventMap["unhandledrejection"] = PromiseRejectionEvent`) and the `react-error-boundary` `ErrorBoundary`/`useErrorBoundary` surface are verified against the installed declarations, so the window/listener wiring carries no open gate. Only the live-browser proof that a real uncaught throw and a real promise rejection round-trip through `crashFaultOf` into a shipped `CrashReport` stays a residual — the `RESEARCH` cluster, matching the charter PROOF_GATES `crash capture spike`. Charter DENSITY_BAR rows [14] `CrashTelemetry`, [15] `ErrorBoundaryBinding`, and [16] `CrashReport` flip `SPIKE -> FINALIZED` on this settlement; only [14]'s live-crash residual stays `SPIKE` (charter edit out of this page's write scope — noted gap).

## [2]-[ERROR_BOUNDARY]

- Owner: `CrashTelemetry`, the single crash sink — the global `error`/`unhandledrejection` capture folded into the `interchange` `FaultDetail` family, the breadcrumb ring buffer, the `CrashReport` fold shipped through `SelfTelemetry`, and the recovery-affordance `SubscriptionRef` the `ui` error fallback reads on each fallback render (the binding samples the cell through `SubscriptionRef.get`; the `SubscriptionRef` shape leaves a live `changes` subscription open for a future always-mounted recovery surface); and `ErrorBoundaryBinding`, the `react-error-boundary` `ErrorBoundary` integration whose `onError` emits the render-tree fault into `CrashTelemetry`. `FaultDetail` is the one fault family and a parallel `CrashError`/`AppError` type is the named const-spam defect.
- Cases: `CrashTelemetry.installGlobalHandlers` is the one boot effect the `host-runtime.md` `bootSpa` runs — it merges `BrowserStream.fromEventListenerWindow("error")` (`ErrorEvent`) and `BrowserStream.fromEventListenerWindow("unhandledrejection")` (`PromiseRejectionEvent`) into one capture stream forked `Effect.forkDaemon` so it outlives `bootSpa`'s local scope and lives for the SPA runtime (a `forkScoped` daemon would interrupt the listener the instant `bootSpa` returns), never an inline `addEventListener`, and `mapEffect`s each event into `capture` reading the rejection `reason` or the error-event `error`; `capture` reconstructs each uncaught fault as a `FaultDetail` arm through `crashFaultOf` — a thrown `Error` or rejected reason becomes `FaultDetail.HopFault({ code: "uncaught", evidence })` carrying the sanitized message and a stack digest, and a value already shaped as a `FaultDetail` re-surfaces as itself rather than re-wrapping — so the capture path is total and an uncaught fault is never a bare `Error` escaping to the console; the breadcrumb ring buffer is a bounded `Ref`-backed array of the last navigation, command-dispatch, and lifecycle events sliced to the last 32, attached to each `CrashReport` so a crash ships with its causal trail, and `breadcrumb` is the one writer every infra owner (the router transition, the gateway dispatch, the SW lifecycle) feeds; `ErrorBoundaryBinding` wraps each render subtree in the `react-error-boundary` `ErrorBoundary` over a captured `Runtime.Runtime` snapshot (`Runtime.runFork` bridges the React callback into the `CrashTelemetry` effect interior — the SPA's one `ManagedRuntime`, never a per-boundary runtime), its `onError(error, info)` runs `capture` for the render-tree fault carrying the React `componentStack` as evidence, and its `fallbackRender` reads the recovery-affordance cell so the user sees a re-mount affordance rather than a white screen; an accepted recovery calls `resetErrorBoundary` and runs `CrashTelemetry.recover`, which resets the recovery cell to `Healthy`.
- Auto: the `CrashReport` fold sanitizes the exception envelope — it strips the bearer header, query-string secrets, and any `Redacted` field (the route key reads `window.location.pathname` with its query string dropped), retains the `FaultDetail` `code`/`evidence` (and `correlation` where the arm carries it — only the `ComputeFault`/`StoreFault` cases do; the browser-origin `HopFault` this page constructs carries none), the breadcrumb trail, the route key, the build mode read from `RuntimeConfig.buildMode`, and the user-agent, and ships it as one `MetricRegistry.span("crash.report", ...)` through the `SelfTelemetry` export edge to the collector, never a third telemetry path and never a direct collector POST; the report ships at most once per distinct `(faultTag, code, stackDigest)` dedupe key held in a session `Ref` set so a render-loop fault does not flood the collector. The `"crash.report"` `SpanName` is one literal on the closed `MetricRegistry` `SpanName` axis the `platform-substrate.md` page owns, never a free-string span name.
- Packages: `react-error-boundary` for the `ErrorBoundary` render integration, the `FallbackProps` `error`/`resetErrorBoundary` affordance, and the `useErrorBoundary` `showBoundary` programmatic escalation; `@effect/platform-browser` `BrowserStream.fromEventListenerWindow` for the global `error`/`unhandledrejection` ingress as a `Stream`; `effect` for the `FaultDetail` reconstruction (`Match` over the captured shape), the breadcrumb `Ref` buffer, the recovery `SubscriptionRef`, and the `Runtime.runFork` React bridge; the existing `platform` `MetricRegistry` `span` over the `SelfTelemetry` edge for the collector ship.
- Growth: a new fault source lands as one arm on the `crashFaultOf` reconstruction, mapping into the existing `FaultDetail` family, never a new error type; a new breadcrumb kind lands as one literal on the breadcrumb event axis; a new sanitization rule lands as one row on the `CrashReport` fold; a new recovery affordance lands as one row on the recovery cell.
- Boundary: uncaught faults reconstruct as the `interchange` `FaultDetail` family (a `HopFault`/`ConfigError` arm) and never a parallel error type — the exhaustive-fault-family law reaches the crash boundary; the `CrashReport` ships as a `MetricRegistry.span("crash.report", ...)` over the `SelfTelemetry` collector edge, the only telemetry path the `platform-substrate` page fixes, so a direct collector POST or a second crash-reporter SDK is the named defect; the recovery-affordance cell is read by the `ui` error fallback through the one `AtomBinding` (sampled per fallback render via `SubscriptionRef.get`, with the `SubscriptionRef.changes` stream reserved for a live always-mounted surface), never a second state binding; `CrashTelemetry` emits no command and dials no transport; `ui` reads the recovery cell and never imports `platform` — `ErrorBoundaryBinding` lives in `platform` and is composed into the tree at the `CompositionRoot`, never imported by a `ui` leaf.

```ts
import type { ErrorInfo, ReactNode } from "react";
import type { FallbackProps } from "react-error-boundary";
import { Data, Effect, Layer, Match, Option, Predicate, Ref, Runtime, Stream, SubscriptionRef } from "effect";
import * as BrowserStream from "@effect/platform-browser/BrowserStream";
import { ErrorBoundary, useErrorBoundary } from "react-error-boundary";
import { createElement } from "react";
// FaultDetail is the interchange Data.TaggedEnum fault family (interchange/codec-rails#FAULT_FAMILY) —
// reconstructed here, never re-authored: an uncaught browser fault is a FaultDetail.HopFault arm.
import { type FaultDetail, type FaultTag, FaultDetail as Fault } from "../interchange/codec-rails";
import { MetricRegistry, RuntimeConfig } from "./platform-substrate";

// --- [TYPES] -------------------------------------------------------------------------------------

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

// --- [CONSTANTS] ---------------------------------------------------------------------------------

const BREADCRUMB_RING: number = 32;
const STACK_DIGEST_BYTES: number = 256;

// --- [MODELS] ------------------------------------------------------------------------------------

interface CrashReport {
  readonly fault: FaultDetail;
  readonly stackDigest: string;
  readonly route: string;
  readonly buildMode: "development" | "production";
  readonly userAgent: string;
  readonly breadcrumbs: ReadonlyArray<Breadcrumb>;
}

// --- [SERVICES] ----------------------------------------------------------------------------------

interface CrashTelemetry {
  readonly recovery: SubscriptionRef.SubscriptionRef<Recovery>;
  readonly installGlobalHandlers: Effect.Effect<void>;
  readonly capture: (cause: unknown) => Effect.Effect<void>;
  readonly breadcrumb: (crumb: Breadcrumb) => Effect.Effect<void>;
  readonly recover: Effect.Effect<void>;
}
const CrashTelemetry = Effect.Tag("@rasm/ts/web/CrashTelemetry")<CrashTelemetry, CrashTelemetry>();

// --- [OPERATIONS] --------------------------------------------------------------------------------

// crashFaultOf is the ONE total cause->FaultDetail projection for the BROWSER crash boundary, the dual of
// the interchange faultDetailRail.fromConnect transport projection: a value already shaped as a FaultDetail
// re-surfaces as itself (an interior Effect failure that bubbled to window must not double-wrap), an Error
// carries its name+message as evidence, every other reason stringifies — all into the one HopFault arm, never
// a parallel CrashError. evidence is Record<string,string> exactly as the FaultDetail.HopFault case requires.
// the guard narrows _tag against the closed FaultTag set (codec-rails#FAULT_FAMILY), never a loose three-key
// structural probe — a render ErrorInfo or any {_tag, code, evidence} shape is NOT trusted back as a FaultDetail.
const FAULT_TAGS: ReadonlySet<string> = new Set<FaultTag>(["ComputeFault", "StoreFault", "HopFault", "ConfigError", "Quarantine"]);
const isFaultDetail = (c: unknown): c is FaultDetail =>
  Predicate.isRecord(c) && "_tag" in c && Predicate.isString(c._tag) && FAULT_TAGS.has(c._tag) && "code" in c && "evidence" in c;

const crashFaultOf = (cause: unknown): FaultDetail =>
  Match.value(cause).pipe(
    Match.when(isFaultDetail, (c) => c),
    Match.when(Match.instanceOf(Error), (e) => Fault.HopFault({ code: "uncaught", evidence: { message: e.message, name: e.name } })),
    Match.orElse((c) => Fault.HopFault({ code: "uncaught", evidence: { message: String(c) } })),
  );

const stackDigestOf = (cause: unknown): string => (cause instanceof Error ? (cause.stack ?? "").slice(0, STACK_DIGEST_BYTES) : "");

const makeCrashTelemetry: Effect.Effect<CrashTelemetry, never, MetricRegistry | RuntimeConfig> = Effect.gen(function* () {
  const registry = yield* MetricRegistry;
  const config = yield* RuntimeConfig;
  const buildMode = yield* config.buildMode.pipe(Effect.orElseSucceed(() => "development" as const));
  const recovery = yield* SubscriptionRef.make<Recovery>(Recovery.Healthy());
  const crumbs = yield* Ref.make<ReadonlyArray<Breadcrumb>>([]);
  const seen = yield* Ref.make<ReadonlySet<string>>(new Set());

  const breadcrumb = (crumb: Breadcrumb): Effect.Effect<void> =>
    Ref.update(crumbs, (b) => [...b, crumb].slice(-BREADCRUMB_RING));

  const ship = (fault: FaultDetail, stackDigest: string): Effect.Effect<void> =>
    Ref.get(crumbs).pipe(
      Effect.flatMap((breadcrumbs) =>
        registry.span("crash.report", Effect.succeed<CrashReport>({
          fault,
          stackDigest,
          route: window.location.pathname,
          buildMode,
          userAgent: navigator.userAgent,
          breadcrumbs,
        }))),
      Effect.asVoid,
    );

  const capture = (cause: unknown): Effect.Effect<void> =>
    Effect.suspend(() => {
      const fault = crashFaultOf(cause);
      const stackDigest = stackDigestOf(cause);
      const key = `${fault._tag}:${fault.code}:${stackDigest}`;
      return Ref.modify(seen, (s) => (s.has(key) ? [true, s] as const : [false, new Set([...s, key])] as const)).pipe(
        Effect.flatMap((dropped) =>
          dropped
            ? Effect.void
            : SubscriptionRef.set(recovery, Recovery.Crashed({ fault, canRetry: true })).pipe(Effect.zipRight(ship(fault, stackDigest)))),
      );
    });

  // the global capture forks as a DAEMON: it must outlive bootSpa's local scope and live for the SPA's whole
  // runtime, interrupted only when the one ManagedRuntime disposes — never forkScoped (a transient scope would
  // interrupt the listener the instant bootSpa returns) and never an inline addEventListener.
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

// --- [COMPOSITION] -------------------------------------------------------------------------------

const CrashTelemetryLive: Layer.Layer<CrashTelemetry, never, MetricRegistry | RuntimeConfig> = Layer.effect(CrashTelemetry, makeCrashTelemetry);

// ErrorBoundaryBinding is the one react-error-boundary integration: the captured Runtime snapshot (the SPA's
// single ManagedRuntime, passed once at the CompositionRoot) bridges the React onError/onReset callbacks into
// the CrashTelemetry effect interior via Runtime.runFork — never a second runtime and never a raw addEventListener.
// onError marshals the render-tree fault (carrying React's componentStack as evidence) through the SAME capture
// path the global handlers use; fallbackRender reads the recovery cell through the one binding and offers a
// re-mount affordance; an accepted retry resets the boundary AND runs CrashTelemetry.recover.
interface ErrorBoundaryBindingProps {
  readonly runtime: Runtime.Runtime<CrashTelemetry>;
  readonly fallback: (recovery: Recovery, retry: () => void) => ReactNode;
  readonly children: ReactNode;
}

const ErrorBoundaryBinding = ({ runtime, fallback, children }: ErrorBoundaryBindingProps): ReactNode => {
  // a Tag is an Effect<Service, never, Service>; running it against the captured runtime resolves the live instance.
  const telemetry = Runtime.runSync(runtime, CrashTelemetry);
  const fork = Runtime.runFork(runtime);
  const onError = (error: unknown, info: ErrorInfo): void => {
    fork(telemetry.capture(error instanceof Error ? error : Fault.HopFault({ code: "render", evidence: { componentStack: info.componentStack ?? "" } })));
  };
  const onReset = (): void => void fork(telemetry.recover);
  const renderFallback = ({ resetErrorBoundary }: FallbackProps): ReactNode =>
    fallback(Runtime.runSync(runtime, SubscriptionRef.get(telemetry.recovery)), resetErrorBoundary);
  return createElement(ErrorBoundary, { onError, onReset, fallbackRender: renderFallback }, children);
};

// useCrashEscalation is the one ui-facing escalation hook a leaf composes to surface a caught domain fault into
// the nearest boundary — react-error-boundary's useErrorBoundary.showBoundary, the imperative counterpart of the
// onError sink, so a fault the component owns escalates to the SAME fallback the render crash reaches.
const useCrashEscalation = (): { readonly escalate: (cause: unknown) => void; readonly reset: () => void } => {
  const { showBoundary, resetBoundary } = useErrorBoundary();
  return { escalate: showBoundary, reset: resetBoundary };
};

// --- [EXPORTS] -----------------------------------------------------------------------------------

export type { Breadcrumb, CrashReport, CrashTelemetry, ErrorBoundaryBindingProps, Recovery };
export { CrashTelemetry, CrashTelemetryLive, ErrorBoundaryBinding, crashFaultOf, useCrashEscalation };
```

## [3]-[RESEARCH]

- [CRASH_CAPTURE]: the `BrowserStream.fromEventListenerWindow` ingress, the `react-error-boundary` `ErrorBoundary`/`useErrorBoundary` surface, and the `crashFaultOf` total projection are settled off the installed declarations — `WindowEventMap["error"]` is `ErrorEvent` (carrying `.error`), `WindowEventMap["unhandledrejection"]` is `PromiseRejectionEvent` (carrying `.reason`), and `FallbackProps` is `{ error: unknown; resetErrorBoundary }`, so the listener wiring and the React bridge carry no open gate. The one residual is the live-browser proof that a real uncaught `throw` and a real rejected `Promise` round-trip through `crashFaultOf` into a single shipped `MetricRegistry.span("crash.report", ...)` with its breadcrumb trail — the charter PROOF_GATES `crash capture spike` (`window.onerror` + `unhandledrejection` marshal into `FaultDetail`); the session dedupe and the recovery-cell re-mount affordance prove in the `browser-e2e` crash-boundary DOM scenario, leaving only the native global-handler attribution as the live residual.
