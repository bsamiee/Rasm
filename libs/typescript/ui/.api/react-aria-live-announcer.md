# [TS_UI_API_REACT_ARIA_LIVE_ANNOUNCER]

[PACKAGE_SURFACE]:
- package: `@react-aria/live-announcer` (Apache-2.0)
- module: dual — `dist/import.mjs` (ESM `import`) + `dist/main.js` (CJS `main`); `sideEffects: false`; `.` barrel + `./package.json`. Re-exports the three functions from `react-aria`'s private `LiveAnnouncer` — the same global singleton `react-aria`/`react-aria-components` drive internally.
- asset: `dist/types/src/index.d.ts` (`restore: restored`).
- runtime: vanilla DOM — NOT React (no portal, no hook); a lazy global singleton region prepended to `document.body`. SSR-safe (guards `typeof document`). Internalizes `@swc/helpers`; peer `react-aria`.
- ABI: imperative and synchronous — `announce`/`clearAnnouncer`/`destroyAnnouncer` return `void`; the first announce defers ~100ms (Safari region-attach timing) unless in a React-act/test env.
- plane: `plane:runtime` (W4 `ui`); folder-local to `ui`, catalogued here.
- rail: `ui/view` live-region — the status/toast announcement primitive.
- role: `view/primitive.md` live-region announce/toast rows — imperative SR output for status and non-visual toast.

`@react-aria/live-announcer` is the imperative screen-reader announcement rail: `announce(message)` pushes text into an ARIA live region so assistive tech reads it, with no visible DOM and no React tree. It is a global vanilla-DOM singleton — one hidden container on `document.body` holding an `assertive` and a `polite` `role=log` region — created lazily on first announce, reused across every call, and torn down by `destroyAnnouncer()`. This is the primitive behind the `view/primitive` status/toast announce rows: a save-status fold, a result-count change, or a non-visual toast becomes an SR announcement through one call. It is deliberately React-free (the impl avoids `ReactDOM.render`/portals to survive multiple React versions), so it composes at the effect edge, never as a component.

## [01]-[ANNOUNCE_SURFACE]

The whole surface is three functions over a two-value assertiveness axis and a string-or-`aria-labelledby` message.

| [INDEX] | [SYMBOL]           | [KIND]   | [CAPABILITY_BOUNDARY]                                                                       |
| :-----: | :----------------- | :------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `announce`         | push     | append `message` to the matching region; defaults `'assertive'`, `7000`ms then node removed |
|  [02]   | `clearAnnouncer`   | flush    | empty a region's queued nodes (runtime clears both on a falsy arg; the type requires it)    |
|  [03]   | `destroyAnnouncer` | teardown | remove the singleton region from the DOM and null it (next announce re-creates)             |

[ASSERTIVENESS]: `Assertiveness = 'assertive'|'polite'`
[MESSAGE]: `Message = string|{'aria-labelledby':string}`
[SURFACES]: `announce(Message,Assertiveness?,number?) -> void` `clearAnnouncer(Assertiveness) -> void` `destroyAnnouncer() -> void`

Consumer note: `'assertive'` interrupts the reader immediately (faults, blocking status); `'polite'` waits for a pause (progress, counts). `timeout` is the ms the appended node lives before removal (default 7000); an empty-string message is appended but never auto-removed. The object `aria-labelledby` form announces an existing node's label — image/graph descriptions read without duplicating text.

## [02]-[SINGLETON_MECHANICS]

The lifecycle a consumer reasons against — the region is a persistent global, not a per-call element.

- One container `div[data-live-announcer]` is prepended to `document.body` on first `announce`, holding two children: `role=log aria-live=assertive aria-relevant=additions` and its `polite` twin; both use the same clip styles as `@radix-ui/react-visually-hidden`.
- The singleton is module-global and reused — the region attaches once, every announce appends a node into the matching log.
- SSR-safe: the constructor guards `typeof document !== 'undefined'`, so import + call are no-ops server-side.
- First-announce timing: outside a React-act/test env the first announce waits ~100ms for the region to attach (Safari drops announcements added too fast); subsequent announces fire immediately.
- `destroyAnnouncer()` is the lifecycle teardown — the global region must be cleared on host teardown/hot-reload to avoid a stale duplicated region.

## [03]-[INTEGRATION]

[BOUNDARY: `announce()` vs `react-aria-components` `UNSTABLE_Toast`/`ToastRegion` (`.api/react-aria-components.md`)] — RAC's `ToastRegion` + `UNSTABLE_ToastQueue` own the VISIBLE toast surface with a built-in live region; `announce()` is the INVISIBLE complement — status with no visible chrome ("Saved", "3 results", "Copied"). A visible RAC toast already announces itself; the design never double-announces it with `announce()`. Visible toast = `ToastRegion`; invisible status = `announce()`.

[STACK: `announce()` + `@effect-atom` + `Effect.sync` (`.api/effect-atom-atom-react.md`, `.api/effect.md`)] — the announcement is an effect-edge boundary, not a render: a `SubscriptionRef`/atom fold (save-status, result-count) drives `Effect.sync(() => announce(msg, 'polite'))`; the imperative singleton stays outside the render tree, and the one state binding (`ONE_FOLD_ONE_BINDING`) owns WHEN to announce.

[STACK: `announce()` + `intl` message plane + `effect/Match` (`.api/effect.md`)] — the message is localized first: the `intl/message` catalog folds a plural/select string over native `Intl` keyed by the kernel `Locale` brand, and `announce()` receives the resolved string. `Match.value(statusKind)` dispatches the assertiveness — fault/blocking → `'assertive'`, progress/count → `'polite'` — so the axis is a closed-arm decision, not an ad-hoc boolean.

[STACK: `destroyAnnouncer()` + host lifecycle `Layer` (`.api/effect.md`)] — the global region is torn down by a scoped finalizer: `Layer.scoped` (or the browser/host teardown) calls `destroyAnnouncer()` on shutdown so the singleton never leaks across app remounts.

## [04]-[RAIL_LAW]

- Owns: imperative ARIA live-region announcement — `announce()` for SR status/toast output, `clearAnnouncer` to flush, `destroyAnnouncer` to tear down the global region.
- Accept: `announce(localizedString, assertiveness)` from the effect edge; `'assertive'` for faults/blocking, `'polite'` for progress/counts; the object `aria-labelledby` form to announce an existing node's label; `destroyAnnouncer()` in a lifecycle finalizer.
- Reject: announcing a visible RAC toast twice (`ToastRegion` already has a live region); building a live region as a React component/portal (the singleton is deliberately React-free); a raw unlocalized string (route through the intl plane); treating the calls as async (they are synchronous `void`).
- Boundary: global vanilla-DOM singleton on `document.body`, SSR-guarded, lazy, reused; the first announce defers ~100ms outside a test env. `clearAnnouncer` is typed with a required `Assertiveness` though the impl clears both on a falsy arg. Peer `react-aria`; the same singleton `react-aria`/`react-aria-components` drive internally.
