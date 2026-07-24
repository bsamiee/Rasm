# [TS_UI_API_REACT_ERROR_BOUNDARY]

`react-error-boundary` owns the folder's one render-catch boundary: `ErrorBoundary`, whose recovery is a three-arm discriminated prop union, with `useErrorBoundary` escalating the carrier-less throws React never catches in render.

`ErrorBoundary` seams the Effect typed-error channel to the React tree: a failed `useAtomSuspense` read throws `Cause.squash(cause)` in render, so the boundary catches the tagged `E` and the recovery folds it through `Match` into the `wire/fault` projection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-error-boundary`
- package: `react-error-boundary` (MIT)
- module: self-typed ESM (`dist/react-error-boundary.d.ts`)
- runtime: client component, `"use client"`; peer `react`
- rail: view/primitive — the render-catch boundary every subtree fault folds through

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the recovery prop union, payload, and context contract

`ErrorBoundaryProps` discriminates three exported arms — `ErrorBoundaryPropsWithFallback` (static `ReactNode`), `ErrorBoundaryPropsWithComponent` (`ComponentType<FallbackProps>`), `ErrorBoundaryPropsWithRender` (`(FallbackProps) => ReactNode`) — exactly one arm set, the others `never`. Every arm carries `onError` telemetry, `resetKeys` re-arm, and `onReset` (reason `"keys"` with prev/next, or `"imperative-api"` with args), and delivers `FallbackProps` to its recovery.

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `ErrorBoundaryProps`                                             | props union   | one recovery arm set, the others `never`              |
|  [02]   | `FallbackProps` (`error`, `resetErrorBoundary`)                  | record        | the `Cause.squash`ed tagged `E`; renders `wire/fault` |
|  [03]   | `OnErrorCallback` (`(error, info: ErrorInfo) => void`)           | callback      | `info.componentStack` locates the throw site          |
|  [04]   | `ErrorBoundaryContext` / `ErrorBoundaryContextType`              | context       | `didCatch`/`error`/`resetErrorBoundary` for a reader  |
|  [05]   | `UseErrorBoundaryApi` (`error`, `resetBoundary`, `showBoundary`) | record        | the `useErrorBoundary` return contract                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: boundary component, escalation hook, HOC wrap, and message extractor

One boundary owner drives all recovery; the modality is the prop-union arm, never a second component. `withErrorBoundary` returns a `ForwardRefExoticComponent`.

| [INDEX] | [SURFACE]                                                    | [SHAPE]   | [CAPABILITY]                                           |
| :-----: | :----------------------------------------------------------- | :-------- | :----------------------------------------------------- |
|  [01]   | `<ErrorBoundary onError onReset resetKeys>`                  | component | wraps a subtree; render errors fold to the fault row   |
|  [02]   | `useErrorBoundary(): { error, resetBoundary, showBoundary }` | hook      | escalate a carrier-less throw; `resetBoundary` retries |
|  [03]   | `withErrorBoundary(Component, ErrorBoundaryProps)`           | HOC       | ref-forwarded wrap where JSX nesting is awkward        |
|  [04]   | `getErrorMessage(unknown): string \| undefined`              | function  | message from an unknown throw, pre-`Match`             |

- `useErrorBoundary`: valid only inside an `ErrorBoundary` subtree.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Whichever arm is set, the recovery receives `FallbackProps` (`error`, `resetErrorBoundary`), so reset is arm-independent.
- `ErrorBoundary` unwinds the subtree and mounts the recovery on a render throw; a failed `useAtomSuspense(atom)` read is exactly this path — throwing `Cause.squash(cause)` in render with no `showBoundary`, its `waiting` arm suspending to `<Suspense>` — so Suspense owns the pending arm and the boundary the failed arm.
- `useErrorBoundary().showBoundary(Cause.squash(cause))` escalates a throw with no render carrier — an event handler, a `setTimeout`/raw-promise rejection, a post-commit failure — re-throwing it in render on the next commit; a `startTransition` callback throw carries to the boundary directly, and passing the squashed `E` gives the recovery `Match` the same shape as the suspense path.
- `resetKeys` re-arms on a listed value change (bind it to the atom query key); `resetErrorBoundary(...args)` is the explicit retry; `onReset` receives the discriminated reason so the retry re-runs the failed `Effect`.

[STACKING]:
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): `useAtomSuspense(atom)` is the primary async rail — its `Failure` arm throws `Cause.squash(cause)` in render as `FallbackProps.error`, the tagged `E` that `Match.tagsExhaustive` needs where a raw `Cause` will not tag-match; `includeFailure: true` returns the `Failure` inline for a local `Result.match` instead; `resetKeys`/`onReset` re-run the atom through `useAtomRefresh`.
- `effect` (`libs/typescript/.api/effect.md`): the recovery dispatches the tagged fault family through `Match.tagsExhaustive`/`Data.taggedEnum().$match` into the `wire/fault` row, localized by `intl/format`; `resetErrorBoundary` re-enters the failed computation whose `Effect.retry(Schedule)` policy lives on the effect — the boundary owns the unwind, `Match` the branch, `intl` the message.
- `react-dom` (`.api/react-dom.md`): the `createRoot` `RootOptions` error trio frames the boundary — `onCaughtError` forwards a caught error to `telemetry`, `onUncaughtError` catches what escaped every boundary, `onRecoverableError` handles hydration divergence; boundary `onError` and root `onCaughtError` are one event at two altitudes.
- `view` rows (within-lib): every `view` row wraps its `useAtomSuspense` reads in one `<Suspense>` + `ErrorBoundary` pair, and the recovery is the sole fault-render surface.

[LOCAL_ADMISSION]:
- `ErrorBoundary` is the single boundary owner; one prop-union arm per boundary selects the recovery modality.
- Read async atoms through `useAtomSuspense` under a `<Suspense>` + this boundary; escalate only carrier-less throws with `showBoundary(Cause.squash(cause))`.
- Bind `resetKeys` to the atom/query input and re-run the failed `Effect` in `onReset`.
- Render the recovery through `Match` over the tagged fault family localized by `intl`; `getErrorMessage` is the last-resort unknown-throw extractor.

[RAIL_LAW]:
- Package: `react-error-boundary`
- Owns: the single `ErrorBoundary` render-catch owner with a three-arm discriminated recovery union, the `useErrorBoundary` escalation hook, the `withErrorBoundary` HOC, and the `FallbackProps`/`ErrorBoundaryContext` contract
- Accept: one recovery arm per boundary, `useAtomSuspense` as the primary async rail (`<Suspense>` waiting + boundary `Failure`), `showBoundary(Cause.squash(cause))` for carrier-less throws, `resetKeys` bound to the atom input with an `Effect`-re-running `onReset`, `FallbackProps.error` projected through `Match` + `intl`, the `react-dom` root error trio as the outer frame
- Reject: a hand-rolled `componentDidCatch` class, mixed recovery arms, a per-component `try/catch` around a `useAtomSuspense` read, a swallowed promise rejection, a reset that does not re-drive the failed `Effect`, an inline `instanceof`/`if` ladder where `Match` owns the fault family
