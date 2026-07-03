# [API_CATALOGUE] react-error-boundary

`react-error-boundary` is the React render-tree fault owner: one `ErrorBoundary` class (which IS the context provider — `render()` returns a `FunctionComponentElement<ProviderProps<ErrorBoundaryContextType | null>>`) with three mutually-exclusive fallback strategies (`fallback` / `FallbackComponent` / `fallbackRender`, `never`-enforced), a `resetKeys` auto-reset diff engine (driven in `componentDidUpdate`), and the `useErrorBoundary` hook whose `showBoundary` is the ONLY bridge for faults React's render-catch cannot see. Its catch surface is narrow by design — it catches render-phase throws (and, in React 19, throws from a `startTransition` callback) but NOT SSR, event-handler, async, or self errors — which is exactly why `platform`'s Effect runtime routes every async/boundary-invisible failure through `showBoundary`. In `platform` it is the `Observability/boundary.md` owner, feeding `onError` into `Observability/crash.md` and correlating the caught frame to `Observability/replay.md` + `telemetry.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-error-boundary`
- package: `react-error-boundary`
- version: `6.1.2` (central pin `pnpm-workspace.yaml`)
- license: `MIT`
- runtime: browser CLIENT-only — a `"use client"` component; it catches nothing during server rendering, so it lives below the SPA hydration root, never in an RSC/SSR shell
- module: `react-error-boundary` — dual build (`dist/react-error-boundary.js` ESM, `.cjs` CJS, types `dist/react-error-boundary.d.ts`); single barrel, no subpaths, `exports` unset (resolves via `main`/`module`/`types`)
- peer: `react ^18.0.0 || ^19.0.0` (the sole peer; `react-dom` is transitive through the app) — the `startTransition` auto-catch is a React 19 capability
- side-effects: `false` (tree-shakeable)
- asset: runtime library (React component + hook + HOC)
- rail: ui — render-tree fault capture and imperative boundary control
- catalog-verdict: KEEP; the `Observability/boundary.md` render-fault owner; async/event faults route through `showBoundary`, never a bare `class extends Component` — no collapse target (the Effect failure channel is a different lane, this owns the React render tree)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: error-boundary type family
- rail: ui

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                                                 |
| :-----: | :-------------------------------- | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `ErrorBoundaryProps`              | union         | `PropsWithFallback \| PropsWithComponent \| PropsWithRender` — the strategy discriminant |
|  [02]   | `ErrorBoundaryPropsWithFallback`  | type alias    | static `fallback: ReactNode` strategy (no access to `error`/reset)                   |
|  [03]   | `ErrorBoundaryPropsWithComponent` | type alias    | `FallbackComponent: ComponentType<FallbackProps>` strategy                           |
|  [04]   | `ErrorBoundaryPropsWithRender`    | type alias    | `fallbackRender: (props: FallbackProps) => ReactNode` inline strategy                |
|  [05]   | `FallbackProps`                   | type alias    | `{ error: unknown; resetErrorBoundary: (...args: unknown[]) => void }` — the fallback contract |
|  [06]   | `ErrorBoundaryContextType`        | type alias    | `{ didCatch: boolean; error: unknown \| null; resetErrorBoundary: (...args) => void }` — deep-consumer state |
|  [07]   | `OnErrorCallback`                 | type alias    | `(error: unknown, info: ErrorInfo) => void` — the crash-ship hook shape              |
|  [08]   | `UseErrorBoundaryApi`             | type alias    | `{ error: unknown \| null; resetBoundary: () => void; showBoundary: (error: unknown) => void }` |

[PUBLIC_TYPE_SCOPE]: shared props base + the `onReset` discriminant
- rail: ui
- `ErrorBoundarySharedProps = PropsWithChildren<{ onError?, onReset?, resetKeys? }>` is the intersection base of all three strategies. `onError` is `OnErrorCallback`; `resetKeys?: unknown[]` is the auto-reset dependency list. `onReset` is NOT `(...args) => void` — it takes a DISCRIMINATED `details` object naming the reset cause:

```ts
// dist/react-error-boundary.d.ts — the real onReset signature
onReset?: (details:
  | { reason: "imperative-api"; args: unknown[] }                                    // resetErrorBoundary(...args) / resetBoundary()
  | { reason: "keys"; prev: unknown[] | undefined; next: unknown[] | undefined }      // a resetKeys entry changed
) => void;
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ErrorBoundary` class — the provider + catch mechanism
- rail: ui
- The class body is the catch engine: `getDerivedStateFromError` flips `didCatch`, `componentDidCatch` fires `onError`, `componentDidUpdate` runs the `resetKeys` diff and calls `onReset({ reason: "keys", … })`, and `render()` wraps children in the context provider. Consumers place the component and pass exactly one strategy — never subclass it.

```ts
export declare class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  static getDerivedStateFromError(error: Error): { didCatch: boolean; error: Error }  // render-phase catch -> fallback state
  componentDidCatch(error: unknown, info: ErrorInfo): void                            // fires onError (logging/ship, NOT recovery)
  componentDidUpdate(prevProps, prevState): void                                      // resetKeys diff -> auto-reset + onReset({reason:"keys"})
  resetErrorBoundary(...args: unknown[]): void                                        // imperative reset -> onReset({reason:"imperative-api"})
  render(): FunctionComponentElement<ProviderProps<ErrorBoundaryContextType | null>>  // the boundary IS the context provider
}

// three mutually-exclusive strategies, never-enforced at the type level
type ErrorBoundaryPropsWithComponent = ErrorBoundarySharedProps & { fallback?: never; FallbackComponent: ComponentType<FallbackProps>; fallbackRender?: never }
type ErrorBoundaryPropsWithRender    = ErrorBoundarySharedProps & { fallback?: never; FallbackComponent?: never; fallbackRender: (props: FallbackProps) => ReactNode }
type ErrorBoundaryPropsWithFallback  = ErrorBoundarySharedProps & { fallback: ReactNode; FallbackComponent?: never; fallbackRender?: never }
```

[ENTRYPOINT_SCOPE]: hook, HOC, context, utility
- rail: ui
- `useErrorBoundary` MUST run inside an `ErrorBoundary` subtree (it reads `ErrorBoundaryContext`); `showBoundary` is the imperative escalation for boundary-invisible faults.

```ts
export declare function useErrorBoundary(): UseErrorBoundaryApi
// { error: unknown|null; resetBoundary(): void; showBoundary(error: unknown): void }

export declare function withErrorBoundary<Type extends ComponentClass<unknown>, Props extends object>(
  Component: ComponentType<Props>,
  errorBoundaryProps: ErrorBoundaryProps,
): ForwardRefExoticComponent<PropsWithoutRef<Props> & RefAttributes<InstanceType<Type>>>  // ref forwards to the wrapped instance

export declare const ErrorBoundaryContext: Context<ErrorBoundaryContextType | null>       // didCatch/error for deep consumers
export declare function getErrorMessage(thrown: unknown): string | undefined              // safe .message extraction from unknown
```

## [04]-[IMPLEMENTATION_LAW]

[UI_TOPOLOGY]:
- exactly ONE of `fallback` / `FallbackComponent` / `fallbackRender` is required; the union uses `never` so passing two is a compile error. `fallback` (static) cannot read `error` or call reset — use it only for a fixed message; `FallbackComponent`/`fallbackRender` receive `FallbackProps` and are the default form.
- CATCH MATRIX (the load-bearing fact): the boundary catches render-phase throws in the subtree AND, in React 19, throws from a `startTransition`/`useTransition` callback. It does NOT catch SSR errors, event-handler errors, async callback/promise errors, or errors thrown by the boundary itself — those reach the boundary only via `showBoundary`.
- `resetKeys` is a dependency array diffed in `componentDidUpdate`; when any entry changes the boundary auto-resets and fires `onReset({ reason: "keys", prev, next })`. `resetErrorBoundary(...args)`/`resetBoundary()` reset imperatively and fire `onReset({ reason: "imperative-api", args })`.
- `onError` fires synchronously in `componentDidCatch` with the `ErrorInfo.componentStack`; it is a SHIP hook (log/report), never a recovery path — recovery is a reset.

[INTEGRATION_LAW]:
- Stack with the Effect runtime (`libs/typescript/.api/effect.md`): `platform` runs effects off the render path, so an `Effect` failure NEVER surfaces to React's render-catch. The bridge is `Effect.catchAllCause` (or a `runFork` failure handler) at the component seam calling `showBoundary(cause)` from `useErrorBoundary` — `Observability/boundary.md` owns this adapter so async faults land in the SAME boundary as render faults, with `Cause` carried as the thrown value (`FallbackProps.error: unknown`).
- Stack with `Observability/crash.md`: `onError` is the sink feed — it hands `(error, info)` to the global crash fold which mints a sanitized `CrashReport` (redacting before ship); the boundary owns the LOCAL recover-or-reset, the crash sink owns the GLOBAL uncaught fold, and `onError` is the one seam between them.
- Stack with `Observability/replay.md` + `telemetry.md`: the caught frame correlates to the trace-correlated replay-window id and a breach span; `onError`'s `ErrorInfo.componentStack` plus the active span context stamp `ATTR_EXCEPTION_*` keys (`libs/typescript/_tmp/platform/.api/opentelemetry-semantic-conventions.md`) so a React render fault and a server fault are wire-identical on the collector.
- Stack with `Session/router.md`: pass `resetKeys={[routeKey]}` so a navigation auto-resets the boundary via the `componentDidUpdate` diff — never a manual reset wired into the router; the route key IS the reset trigger.
- Stack with `interchange` fault family: `FallbackProps.error` is `unknown`; `getErrorMessage` extracts a display string, but the fallback should discriminate the reconstructed typed fault (`interchange` decode) rather than string-match — the boundary renders the fault, the fault family types it.

[LOCAL_ADMISSION]:
- `showBoundary(error)` is the mandated escalation for any fault outside render (Effect failure channel, event handler, `setTimeout`/promise) — it is why the bare `class extends Component` boundary is banned: only this package exposes the imperative bridge.
- `resetBoundary()` resets the nearest ancestor without a `resetKeys` change; `ErrorBoundaryContext` (`didCatch`/`error`) lets a deep consumer render degraded WITHOUT being the fallback component.
- `withErrorBoundary` forwards the ref to the wrapped instance — use it for a leaf that must be individually isolated; prefer the declarative `<ErrorBoundary>` at layout seams.

[RAIL_LAW]:
- Package: `react-error-boundary`
- Owns: React render-tree fault capture, the three fallback strategies, `resetKeys`/imperative reset, and the `showBoundary` imperative bridge for boundary-invisible faults
- Accept: `FallbackComponent`/`fallbackRender` for typed fallbacks; `showBoundary` for async/Effect/event faults; `resetKeys={[routeKey]}` for route-driven auto-reset; `onError` as the single feed into `Observability/crash.md`
- Reject: a bare `class extends Component` error boundary in new code; the phantom `onReset?: (...args) => void` signature (the real `onReset` takes the discriminated `{ reason }` details); `fallback` (static) where the fallback must read `error` or reset; treating `onError` as a recovery path; string-matching `FallbackProps.error` where the `interchange` typed fault discriminates
