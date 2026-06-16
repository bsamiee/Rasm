# [PLATFORM_ERROR_BOUNDARY]

One page owns the browser error-boundary and crash/exception telemetry concern — `CrashTelemetry`, the Effect-native fault sink capturing the global `window.onerror`/`unhandledrejection` stream and the React render-tree faults, marshalling each into the `interchange` `FaultDetail` family and shipping a sanitized `CrashReport` through the `platform` `SelfTelemetry` collector path; `ErrorBoundaryBinding`, the `react-error-boundary` integration emitting render faults into `CrashTelemetry`; and the session-breadcrumb ring buffer attached to each report. An uncaught fault reconstructs as a `FaultDetail` arm — never a parallel error type — and the collector is the only telemetry path. The page composes `react-error-boundary` and the existing `SelfTelemetry` edge, holds no domain state, and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                |
| :-----: | :------------- | :-------------------------------------------------------------------- |
|   [1]   | ERROR_BOUNDARY | the global fault capture, the crash fold, and the recovery affordance |

## [2]-[ERROR_BOUNDARY]

- Owner: `CrashTelemetry`, the single crash sink — the global `error`/`unhandledrejection` capture folded into the `interchange` `FaultDetail` family, the breadcrumb ring buffer, the `CrashReport` fold shipped through `SelfTelemetry`, and the recovery-affordance `SubscriptionRef` the `ui` error fallback subscribes to; and `ErrorBoundaryBinding`, the `react-error-boundary` `ErrorBoundary` integration whose `onError` emits the render-tree fault into `CrashTelemetry`. `FaultDetail` is the one fault family and a parallel `CrashError`/`AppError` type is the named const-spam defect.
- Cases: `CrashTelemetry` binds `BrowserStream.fromEventListenerWindow("error")` and `BrowserStream.fromEventListenerWindow("unhandledrejection")` into one capture stream and reconstructs each uncaught fault as a `FaultDetail` arm through `crashFaultOf` — a thrown `Error` or rejected reason becomes `FaultDetail.HopFault({ code, evidence })` carrying the sanitized message and a stack digest, and a `FaultDetail` thrown from the Effect interior re-surfaces as itself rather than re-wrapping — so the capture path is total and an uncaught fault is never a bare `Error` escaping to the console; the breadcrumb ring buffer is a bounded `Ref`-backed array of the last navigation, command-dispatch, and lifecycle events, attached to each `CrashReport` so a crash ships with its causal trail; `ErrorBoundaryBinding` wraps each render subtree in the `react-error-boundary` `ErrorBoundary` and its `onError` callback runs the `CrashTelemetry` capture for the render-tree fault, with the `fallbackRender` reading the recovery-affordance cell so the user sees a re-mount affordance rather than a white screen; an accepted recovery resets the boundary and clears the recovery cell.
- Auto: the `CrashReport` fold sanitizes the exception envelope — it strips the bearer header, query-string secrets, and any `Redacted` field, retains the `FaultDetail` `code`/`evidence`/`correlation`, the breadcrumb trail, the route key, the build hash, and the user-agent, and ships it as one `MetricRegistry.span("crash.report", ...)` through the `SelfTelemetry` export edge to the collector, never a third telemetry path and never a direct collector POST; the report ships at most once per distinct `(code, stackDigest)` within a debounce window so a render-loop fault does not flood the collector, the dedupe key held in a bounded `Ref` set. The `"crash.report"` `SpanName` is one literal added to the closed `MetricRegistry` `SpanName` axis the `platform-substrate.md` page owns, never a free-string span name.
- Packages: `react-error-boundary` for the `ErrorBoundary` render integration and the `fallbackRender` affordance; `@effect/platform-browser` `BrowserStream.fromEventListenerWindow` for the global `error`/`unhandledrejection` ingress; `effect` for the `FaultDetail` reconstruction (`Match` over the captured shape), the breadcrumb `Ref` buffer, and the recovery `SubscriptionRef`; the existing `platform` `MetricRegistry` `span` over the `SelfTelemetry` edge for the collector ship.
- Growth: a new fault source lands as one arm on the `crashFaultOf` reconstruction, mapping into the existing `FaultDetail` family, never a new error type; a new breadcrumb kind lands as one literal on the breadcrumb event axis; a new sanitization rule lands as one row on the `CrashReport` fold; a new recovery affordance lands as one row on the recovery cell.
- Boundary: uncaught faults reconstruct as the `interchange` `FaultDetail` family (a `HopFault`/`ConfigError` arm) and never a parallel error type — the exhaustive-fault-family law reaches the crash boundary; the `CrashReport` ships as a `MetricRegistry.span("crash.report", ...)` over the `SelfTelemetry` collector edge, the only telemetry path the `platform-substrate` page fixes, so a direct collector POST or a second crash-reporter SDK is the named defect; the recovery-affordance cell is read by the `ui` error fallback through the one `AtomBinding`, never a second state binding; `CrashTelemetry` emits no command and dials no transport; `ui` reads the recovery cell and never imports `platform`.

```ts contract
type Breadcrumb =
  | { readonly _tag: "Navigation"; readonly route: string; readonly at: number }
  | { readonly _tag: "Command"; readonly intent: string; readonly at: number }
  | { readonly _tag: "Lifecycle"; readonly phase: string; readonly at: number };

interface CrashReport {
  readonly fault: FaultDetail;
  readonly stackDigest: string;
  readonly route: string;
  readonly buildHash: string;
  readonly userAgent: string;
  readonly breadcrumbs: ReadonlyArray<Breadcrumb>;
}

type Recovery =
  | { readonly _tag: "Healthy" }
  | { readonly _tag: "Crashed"; readonly fault: FaultDetail; readonly canRetry: boolean };

interface CrashTelemetry {
  readonly recovery: SubscriptionRef.SubscriptionRef<Recovery>;
  readonly capture: (cause: unknown) => Effect.Effect<void>;
  readonly breadcrumb: (crumb: Breadcrumb) => Effect.Effect<void>;
  readonly recover: Effect.Effect<void>;
}

const crashFaultOf = (cause: unknown): FaultDetail =>
  Match.value(cause).pipe(
    Match.when((c): c is FaultDetail => typeof c === "object" && c !== null && "_tag" in c && "code" in c, (c) => c),
    Match.when(Match.instanceOf(Error), (e) => FaultDetail.HopFault({ code: "uncaught", evidence: { message: e.message, name: e.name } })),
    Match.orElse(() => FaultDetail.HopFault({ code: "uncaught", evidence: { message: String(cause) } })),
  );

const makeCrashTelemetry: Effect.Effect<CrashTelemetry, never, Scope.Scope | MetricRegistry | RuntimeConfig> = Effect.gen(function* () {
  const registry = yield* MetricRegistry;
  const config = yield* RuntimeConfig;
  const buildHash = yield* config.buildMode.pipe(Effect.orElseSucceed(() => "development"));
  const recovery = yield* SubscriptionRef.make<Recovery>({ _tag: "Healthy" });
  const crumbs = yield* Ref.make<ReadonlyArray<Breadcrumb>>([]);
  const seen = yield* Ref.make<ReadonlySet<string>>(new Set());
  const breadcrumb = (crumb: Breadcrumb) => Ref.update(crumbs, (b) => [...b, crumb].slice(-32));
  const ship = (fault: FaultDetail, stackDigest: string) =>
    Ref.get(crumbs).pipe(
      Effect.flatMap((breadcrumbs) =>
        registry.span("crash.report", Effect.succeed<CrashReport>({
          fault,
          stackDigest,
          route: window.location.pathname,
          buildHash,
          userAgent: navigator.userAgent,
          breadcrumbs,
        }))),
      Effect.asVoid,
    );
  const capture = (cause: unknown) => {
    const fault = crashFaultOf(cause);
    const stackDigest = cause instanceof Error ? (cause.stack ?? "").slice(0, 256) : "";
    const key = `${fault._tag}:${stackDigest}`;
    return Ref.get(seen).pipe(
      Effect.flatMap((s) =>
        s.has(key)
          ? Effect.void
          : Ref.update(seen, (x) => new Set([...x, key])).pipe(
              Effect.zipRight(SubscriptionRef.set(recovery, { _tag: "Crashed", fault, canRetry: true })),
              Effect.zipRight(ship(fault, stackDigest)),
            )),
    );
  };
  yield* Stream.merge(
    BrowserStream.fromEventListenerWindow("error"),
    BrowserStream.fromEventListenerWindow("unhandledrejection"),
  ).pipe(
    Stream.mapEffect((ev) => capture("reason" in ev ? (ev as PromiseRejectionEvent).reason : (ev as ErrorEvent).error)),
    Stream.runDrain,
    Effect.forkScoped,
  );
  return { recovery, capture, breadcrumb, recover: SubscriptionRef.set(recovery, { _tag: "Healthy" }) } satisfies CrashTelemetry;
});
```
