# [TS_UI_API_REACT_ARIA_LIVE_ANNOUNCER]

`@react-aria/live-announcer` pushes screen-reader status into an ARIA live region with no visible DOM and no React tree, so a save-status fold, result-count change, or non-visual toast reads through one `announce()` call.

React-free by construction, it composes at the effect edge, never as a component, over a lazy `document.body`-prepended singleton the `react-aria` barrel and `react-aria-components` share.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@react-aria/live-announcer`
- package: `@react-aria/live-announcer` (Apache-2.0)
- module: dual ESM/CJS, `sideEffects: false`; `.` barrel + `./package.json`
- asset: `dist/types/src/index.d.ts` re-exporting `react-aria`'s `LiveAnnouncer`
- runtime: vanilla DOM, no React tree — a lazy `document.body`-prepended singleton region, SSR-guarded; peer `react-aria`
- plane: `plane:runtime` (W4 `ui`), folder-local to `ui`
- rail: ui/view live-region — the status/toast announcement primitive for status and non-visual toast

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two-value assertiveness axis and the string-or-label-reference message every call routes into one region

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :----------------------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Assertiveness` (`'assertive'\|'polite'`)        | union         | interrupt (`'assertive'`) vs pause-wait (`'polite'`) delivery    |
|  [02]   | `Message` (`string\|{'aria-labelledby':string}`) | union         | raw text, or an existing node's label voiced without duplication |

`'assertive'` interrupts the reader for faults and blocking status; `'polite'` waits for a pause for progress and counts. `aria-labelledby` voices an existing node's label, reading an image or graph description without duplicating text.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the imperative singleton surface — push, flush, and teardown, each returning synchronous `void`

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------- | :------- | :-------------------------------------------------------------------- |
|  [01]   | `announce(Message, Assertiveness?, number?)` | function | append `message` to the matching region; node removed after `timeout` |
|  [02]   | `clearAnnouncer(Assertiveness)`              | function | empty a region's queued nodes                                         |
|  [03]   | `destroyAnnouncer()`                         | function | remove the singleton region and null it; next announce re-creates     |

- `announce`: `timeout` defaults to `7000`ms, the appended node's lifetime before removal; an empty-string message appends but never auto-removes.
- `clearAnnouncer`: types a required `Assertiveness` though the impl clears both regions on a falsy argument.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `div[data-live-announcer]` prepends to `document.body` on first announce and reuses, holding two `role=log aria-relevant=additions` children (`aria-live=assertive` and its `polite` twin); each announce appends a node into the matching log.
- Outside a React-act/test env the first announce waits ~100ms for attach (Safari drops nodes added too fast); later announces fire immediately.
- `destroyAnnouncer()` clears the global on host teardown or hot-reload, else a stale duplicated region survives the remount.

[STACKING]:
- `react-aria`(`.api/react-aria.md`): the barrel owns the same singleton and re-exports `announce`/`clearAnnouncer`/`destroyAnnouncer`, driving `useToast`/`useToastRegion` and collection announcements through it — one region backs both packages, so a design never mounts a second.
- `react-aria-components`(`.api/react-aria-components.md`): `UNSTABLE_ToastRegion` over `UNSTABLE_ToastQueue<T>` owns the visible toast surface with its own built-in live region; `announce()` is the invisible complement for status with no visible chrome ("Saved", "3 results", "Copied"), and a visible RAC toast announces itself, so the design never double-announces it.
- `@effect-atom/atom-react`(`.api/effect-atom-atom-react.md`): a `SubscriptionRef`/atom fold (save-status, result-count) drives `Effect.sync(() => announce(msg, 'polite'))`, the imperative singleton staying outside the render tree while one state binding owns when to announce.
- `effect`(`libs/typescript/.api/effect.md`): the `intl/message` catalog resolves a plural/select string over native `Intl` keyed by the kernel `Locale` brand before `announce()` receives it; `Match.value(statusKind)` dispatches the axis — fault/blocking → `'assertive'`, progress/count → `'polite'` — and a scoped `Layer.scoped` finalizer calls `destroyAnnouncer()` on shutdown so the singleton never leaks across remounts.

[LOCAL_ADMISSION]:
- Announce SR status through one `announce(localizedString, assertiveness)` call from the effect edge; the region is never a React component or portal.
- Route every message through the intl plane before `announce()`; a raw unlocalized string is the defect.
- Tear the singleton down in a lifecycle finalizer (`destroyAnnouncer()`); never await the calls (they return synchronous `void`).

[RAIL_LAW]:
- Package: `@react-aria/live-announcer`
- Owns: imperative ARIA live-region announcement — `announce()` for SR status/toast, `clearAnnouncer` to flush a region, `destroyAnnouncer` to tear down the global singleton
- Accept: `announce(localizedString, assertiveness)` from the effect edge (`'assertive'` for faults/blocking, `'polite'` for progress/counts), the `aria-labelledby` form to voice an existing node's label, `destroyAnnouncer()` in a lifecycle finalizer
- Reject: double-announcing a visible RAC toast (`UNSTABLE_ToastRegion` carries its own live region), building the region as a React component or portal, a raw unlocalized string, awaiting the synchronous `void` calls
