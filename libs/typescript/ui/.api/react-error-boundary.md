# [TS_UI_API_REACT_ERROR_BOUNDARY]

`react-error-boundary` is the one error-boundary owner for the folder: a single `ErrorBoundary` component whose recovery strategy is a three-arm discriminated prop union (`fallback` static node, `fallbackRender` render prop, or `FallbackComponent`), plus the `useErrorBoundary` hook that escalates the failures React never catches in render — carrier-less event-handler and raw-promise throws — into the nearest boundary. It is the seam where the Effect typed-error channel meets the React tree: a failed `@effect-atom/atom-react` `useAtomSuspense(atom)` read throws `Cause.squash(cause)` — the squashed `E` out of the atom's `Result.Failure` — synchronously in render, so the boundary catches it natively and the recovery folds that tagged `E` through a `Match` into the `wire/fault` problem-detail projection. The async-failure rail is therefore Suspense (the `waiting` arm) plus this boundary (the `Failure` arm), never a per-component `try/catch`. React 19 widens the render catch to transitions, so an atom write awaited in a `startTransition` escalates its failed `Effect` automatically; only a throw with no atom/suspense/transition carrier reaches `showBoundary`. The boundary pairs with the `react-dom` root `onCaughtError`/`onUncaughtError` callbacks that frame it — never hand-roll a `componentDidCatch` class.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-error-boundary`
- package: `react-error-boundary`
- license: `MIT`
- react-peer: `react catalog` (the `catalog peer` major dropped `catalog peer`; client component — `"use client"`; the boundary uses class-component `getDerivedStateFromError`/`componentDidCatch`)
- asset: self-typed ESM+CJS runtime library (`dist/react-error-boundary.d.ts`)
- catches: errors thrown while rendering the subtree — including a failed `useAtomSuspense` read that throws `Cause.squash(cause)` in render; and (React 19) errors from a `startTransition`(`useTransition`) callback
- does-not-catch: SSR render, event handlers, async callbacks (`setTimeout`, unresolved promises), errors thrown inside the boundary itself — these escalate through `useErrorBoundary().showBoundary`
- catalog-verdict: KEEP

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the boundary prop union, recovery contract, and context
- rail: view/primitive
- `ErrorBoundaryProps` is a discriminated union — exactly one of the three recovery arms is set (the others typed `never`), so the recovery strategy is one prop choice, not three parallel components. The arms: `fallback` (static `ReactNode`), `FallbackComponent` (`ComponentType<FallbackProps>`), `fallbackRender` ((`FallbackProps`) => `ReactNode`). `FallbackProps` is the payload every arm receives.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                      |
| :-----: | :----------------------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `ErrorBoundaryProps`                                   | props union   | the three mutually-exclusive recovery arms (see lead)    |
|  [02]   | `FallbackProps` (`error`, `resetErrorBoundary`)        | payload       | the `Cause.squash`ed tagged `E`; renders `wire/fault`    |
|  [03]   | `ErrorBoundarySharedProps`                             | shared props  | `onError`→telemetry, `resetKeys` re-arm, `onReset` retry |
|  [04]   | `onReset` `{reason:"imperative-api", args}`            | reset kind    | an explicit `resetErrorBoundary()` call                  |
|  [05]   | `onReset` `{reason:"keys", prev, next}`                | reset kind    | a `resetKeys` dependency change                          |
|  [06]   | `OnErrorCallback` (`(error, info: ErrorInfo) => void`) | error cb      | `info.componentStack` locates the throw site             |
|  [07]   | `ErrorBoundaryContext` / `ErrorBoundaryContextType`    | context       | `didCatch`/`error`/`resetErrorBoundary` context          |
|  [08]   | `UseErrorBoundaryApi`                                  | hook api      | async/event escalation + reset                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: boundary component, escalation hook, and HOC wrap
- rail: view/primitive
- Three entries: the boundary component, the hook that pushes uncaught failures into it, and the HOC that wraps a row. One boundary owner; the modality (static / render-prop / component recovery) is the prop union arm. `withErrorBoundary` returns a `ForwardRefExoticComponent`.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                    |
| :-----: | :----------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `<ErrorBoundary … onError onReset resetKeys>`                | boundary       | wraps a subtree; render errors → fault projection      |
|  [02]   | `useErrorBoundary(): { error, resetBoundary, showBoundary }` | escalate       | escalate a carrier-less throw; `resetBoundary` retries |
|  [03]   | `withErrorBoundary(Component, errorBoundaryProps)`           | HOC wrap       | ref-forwarded HOC wrap where JSX nesting is awkward    |
|  [04]   | `getErrorMessage(thrown: unknown): string \| undefined`      | extract        | display message from an unknown throw; pre-`Match`     |

## [04]-[IMPLEMENTATION_LAW]

[ERROR_BOUNDARY_TOPOLOGY]:
- One boundary owner, three recovery arms as a discriminated union: `fallback` for a static node, `fallbackRender` for an inline render prop, `FallbackComponent` for a named component — the props type forbids setting more than one. The recovery always receives `FallbackProps` (`error`, `resetErrorBoundary`), so the reset path is uniform regardless of arm.
- The boundary catches render errors: React unwinds the subtree on a throw during render and mounts the recovery. A failed `useAtomSuspense(atom)` read is exactly this path — the hook detects the `Failure` arm and `throw`s `Cause.squash(cause)` synchronously in render, so the boundary catches it with no `showBoundary`, while the atom's `waiting` arm suspends to the nearest `<Suspense>`. Suspense owns the pending arm, the boundary owns the failed arm — the async-read rail is those two, never a per-row `try/catch`.
- Only throws with no render carrier need imperative escalation: an event-handler throw, a `setTimeout`/raw-promise rejection, or a failure resolved after commit is invisible to the boundary, so `useErrorBoundary().showBoundary(Cause.squash(cause))` re-throws it inside the boundary's render on the next commit — pass the squashed `E` so the recovery `Match` sees the same shape as the suspense path.
- React 19 widens the catch to transitions: a throw from the `startTransition` returned by `useTransition` is caught by the nearest boundary, so an atom write awaited in a transition (`useAtomSet(atom, { mode: "promise" })`) surfaces its failed `Effect` at the boundary with no manual `showBoundary`.
- Reset is dependency-driven or imperative: `resetKeys` re-arms the boundary when a listed value changes (bind it to the atom's query key so a new input clears a stale error); `resetErrorBoundary(...args)` is the explicit retry. `onReset` receives the discriminated reason (`"keys"` with prev/next, or `"imperative-api"` with args) so the retry effect knows what triggered it and can re-run the failed `Effect`.

[INTEGRATION_LAW]:
- The Effect-fault seam (`.api/effect-atom-atom-react.md`): `useAtomSuspense(atom)` is the primary async-failure rail into this boundary. It reads an atom's `Result<A, E>`, suspends the `waiting` arm to `<Suspense>`, returns `Success`, and on `Failure` throws `Cause.squash(cause)` in render — so the boundary catches the failed read natively, with no `showBoundary`. Because `Cause.squash` unwraps the `Result.Failure`'s `Cause<E>` to the tagged error `E`, `FallbackProps.error` is that `E` directly, which is exactly what the recovery's `Match.tagsExhaustive` folds — a raw `Cause` does not tag-match. `includeFailure: true` is the escape hatch: the read returns the `Failure` arm inline for a local `Result.match` instead of throwing. A throw with no atom carrier escalates through `showBoundary(Cause.squash(cause))` so `FallbackProps.error` stays the same `E` regardless of path. The rail is Suspense + this boundary, never a per-component `try/catch`.
- Fault projection through `Match` (`libs/typescript/.api/effect.md`): the recovery dispatches over the tagged error family with `Match.tagsExhaustive`/`Data.taggedEnum().$match` and renders the `wire/fault` problem-detail row, localized through the `intl/format` formatters. The boundary owns the tree unwind; `Match` owns the branch; `intl` owns the message. No `if/instanceof` ladder in a recovery.
- The `react-dom` frame (`.api/react-dom.md`): the boundary is the inner catch; the `createRoot` `RootOptions` are the outer frame. `onCaughtError` observes what a boundary caught and forwards it to `telemetry`; `onUncaughtError` catches what escaped every boundary; `onRecoverableError` handles hydration divergence. `onError` on the boundary and `onCaughtError` on the root are the same event at two altitudes — the boundary log is the row, the root log is the app-wide sink.
- Retry as a rail (`libs/typescript/.api/effect.md`): `resetErrorBoundary`/`resetKeys` re-runs the atom's `Effect`; the `Effect.retry(Schedule)` policy lives on the effect, and the boundary is the manual, user-driven retry surface layered over the automatic one — the same failed computation, re-entered from the UI.

[LOCAL_ADMISSION]:
- Use `ErrorBoundary` as the single boundary owner; never author a `componentDidCatch`/`getDerivedStateFromError` class or a second boundary abstraction.
- Choose exactly one recovery arm per boundary; the props union rejects mixing `fallback`/`fallbackRender`/`FallbackComponent`.
- Read async atoms through `useAtomSuspense` under a `<Suspense>` + this boundary so the `waiting` arm suspends and the `Failure` arm throws to the boundary; escalate only carrier-less throws (event handlers, raw promises) with `showBoundary(Cause.squash(cause))`, and never swallow a rejected promise or re-implement boundary catch in a per-component `try/catch` that renders its own error UI.
- Bind `resetKeys` to the atom/query input and re-run the failed `Effect` in `onReset`; never reset a boundary without re-driving the computation that failed.
- Render the recovery through a `Match` over the tagged fault family localized by `intl`; never string-format an `unknown` error inline beyond the `getErrorMessage` last-resort.

[RAIL_LAW]:
- Package: `react-error-boundary`
- Owns: the single `ErrorBoundary` component with a three-arm discriminated recovery union, the `useErrorBoundary` escalation hook, the `withErrorBoundary` HOC, and the `ErrorBoundaryContext`/`FallbackProps` contract
- Accept: one recovery arm per boundary, `useAtomSuspense` as the primary async-read failure rail (Suspense `waiting` + boundary `Failure`), `showBoundary(Cause.squash(cause))` only for carrier-less event-handler/raw-promise throws, `resetKeys` bound to the atom input with an `Effect`-re-running `onReset`, `FallbackProps.error` (the squashed tagged `E`) projected through `Match` + `intl`, the `react-dom` root error callbacks as the outer frame
- Reject: a hand-rolled `componentDidCatch` class, mixing recovery arms, a per-component `try/catch` around a `useAtomSuspense` read where the boundary owns the `Failure` arm, swallowed promise rejections, a boundary reset that does not re-drive the failed `Effect`, an inline `instanceof`/`if` ladder where `Match` owns the fault family
