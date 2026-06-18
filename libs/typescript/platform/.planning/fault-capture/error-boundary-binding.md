# [PLATFORM_ERROR_BOUNDARY_BINDING]

One page owns the React render-tree fault integration — `ErrorBoundaryBinding`, the `react-error-boundary` `ErrorBoundary` integration whose `onError` emits the render-tree fault into `CrashTelemetry` over a captured `Runtime` snapshot, and `useCrashEscalation`, the one `ui`-facing escalation hook a leaf composes to surface a caught domain fault into the nearest boundary. The binding marshals through the SAME `capture` path the global handlers use; an accepted recovery resets the boundary and runs `CrashTelemetry.recover`. The page composes `react-error-boundary` and the `fault-capture` `CrashTelemetry` owner, holds no domain state, and authors no decode.

## [1]-[INDEX]

[ERROR_BOUNDARY_BINDING]: the react-error-boundary integration and the escalation hook.

## [2]-[ERROR_BOUNDARY_BINDING]

- Owner: `ErrorBoundaryBinding`, the `react-error-boundary` `ErrorBoundary` integration emitting render faults into `CrashTelemetry`, and `useCrashEscalation`, the imperative escalation hook surfacing a caught domain fault into the nearest boundary.
- Cases: `ErrorBoundaryBinding` wraps each render subtree in the `react-error-boundary` `ErrorBoundary` over a captured `Runtime` snapshot (`Runtime.runFork` bridges the React callback into the `CrashTelemetry` effect interior — the SPA's one `ManagedRuntime`, never a per-boundary runtime), its `onError(error, info)` runs `capture` for the render-tree fault carrying the React `componentStack` as evidence, and its `fallbackRender` reads the recovery-affordance cell so the user sees a re-mount affordance rather than a white screen; an accepted recovery calls `resetErrorBoundary` and runs `CrashTelemetry.recover`, which resets the recovery cell to `Healthy`; `useCrashEscalation` exposes the `react-error-boundary` `useErrorBoundary` `showBoundary` as the imperative counterpart of the `onError` sink, so a fault a component owns escalates to the SAME fallback the render crash reaches.
- Packages: `react-error-boundary` for the `ErrorBoundary` render integration, the `FallbackProps` `error`/`resetErrorBoundary` affordance, and the `useErrorBoundary` `showBoundary` programmatic escalation; `react` for the `createElement` host bridge; `effect` `Runtime.runFork`/`Runtime.runSync` for the React-callback bridge into the `CrashTelemetry` interior; the `fault-capture` `CrashTelemetry` owner for the capture and recovery surfaces.
- Growth: a new escalation surface lands as one row consuming the existing `CrashTelemetry` `capture`/`recover` surface, never a second runtime or a raw `addEventListener`.
- Boundary: `ErrorBoundaryBinding` lives in `platform` and is composed into the tree at the `CompositionRoot`, never imported by a `ui` leaf; it marshals into the one `CrashTelemetry` sink and authors no parallel error type; the recovery cell is read through the one `AtomBinding`, never a second state binding; the binding emits no command and dials no transport.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { ErrorInfo, ReactNode } from "react";
import type { FallbackProps } from "react-error-boundary";
import { Runtime, SubscriptionRef } from "effect";
import { ErrorBoundary, useErrorBoundary } from "react-error-boundary";
import { createElement } from "react";
import { type FaultDetail, FaultDetail as Fault } from "../interchange/fault-family.ts";
import { type CrashTelemetry, type Recovery, CrashTelemetry as CrashTag } from "./crash-telemetry.ts";

// --- [COMPOSITION] ---------------------------------------------------------------------
interface ErrorBoundaryBindingProps {
  readonly runtime: Runtime.Runtime<CrashTelemetry>;
  readonly fallback: (recovery: Recovery, retry: () => void) => ReactNode;
  readonly children: ReactNode;
}

const ErrorBoundaryBinding = ({ runtime, fallback, children }: ErrorBoundaryBindingProps): ReactNode => {
  const telemetry = Runtime.runSync(runtime, CrashTag);
  const fork = Runtime.runFork(runtime);
  const onError = (error: unknown, info: ErrorInfo): void => {
    fork(telemetry.capture(error instanceof Error ? error : Fault.HopFault({ reason: "render", evidence: { componentStack: info.componentStack ?? "" } })));
  };
  const onReset = (): void => void fork(telemetry.recover);
  const renderFallback = ({ resetErrorBoundary }: FallbackProps): ReactNode =>
    fallback(Runtime.runSync(runtime, SubscriptionRef.get(telemetry.recovery)), resetErrorBoundary);
  return createElement(ErrorBoundary, { onError, onReset, fallbackRender: renderFallback }, children);
};

const useCrashEscalation = (): { readonly escalate: (cause: unknown) => void; readonly reset: () => void } => {
  const { showBoundary, resetBoundary } = useErrorBoundary();
  return { escalate: showBoundary, reset: resetBoundary };
};

// --- [EXPORTS] -------------------------------------------------------------------------
export type { ErrorBoundaryBindingProps };
export { ErrorBoundaryBinding, useCrashEscalation };
```
