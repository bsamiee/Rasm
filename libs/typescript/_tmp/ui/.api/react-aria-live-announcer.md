# [API_CATALOGUE] @react-aria/live-announcer

`@react-aria/live-announcer` is a three-function imperative surface over one process-wide ARIA live-region pair. `announce` posts a message at a politeness level, `clearAnnouncer` empties one level's region, `destroyAnnouncer` tears the region nodes out of the DOM. The package is a thin re-export of `react-aria`'s internal `LiveAnnouncer`; the barrel exports the three functions and NOTHING else — the `Assertiveness`/`Message` argument shapes are internal to the source module (sealed by a trailing `export {}`) and are not importable. It is the one imperative broadcast primitive for status changes with no owning widget; component-scoped live regions belong to `react-aria`'s `useToastRegion`/`useLandmark` and `react-aria-components`' `UNSTABLE_ToastRegion`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@react-aria/live-announcer`
- package: `@react-aria/live-announcer`
- version: `3.5.1`
- license: `Apache-2.0`
- module: `@react-aria/live-announcer` (barrel re-export of `react-aria/private/live-announcer/LiveAnnouncer`)
- namespace: three named function exports; zero exported types
- asset: dual CJS/ESM (`dist/main.js` / `dist/module.js`), `sideEffects: false`, tree-shakeable
- runtime: browser DOM driver (peer `react-dom ^19`); SSR-safe no-op when `document` is undefined
- rail: accessibility / imperative live-region broadcast

## [02]-[CONSUMED_ARGUMENT_SHAPES]

[ARGUMENT_SCOPE]: internal literal shapes — NOT re-exported, a consumer types its own
- rail: accessibility

| [INDEX] | [SHAPE]         | [DEFINITION]                                | [BOUNDARY_NOTE]                                                       |
| :-----: | :-------------- | :------------------------------------------ | :------------------------------------------------------------------- |
|  [01]   | `Assertiveness` | `'assertive' \| 'polite'`                   | argument-only union; no `'off'` — an off-level caller short-circuits before `announce` |
|  [02]   | `Message`       | `string \| { 'aria-labelledby': string }`   | the `aria-labelledby` form announces by DOM reference, no text copy   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: live-region operations
- rail: accessibility

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :--------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `announce(message: string \| { 'aria-labelledby': string }, assertiveness?: 'assertive' \| 'polite', timeout?: number): void` | imperative fn  | posts to the level's region; default `'polite'`     |
|  [02]   | `clearAnnouncer(assertiveness: 'assertive' \| 'polite'): void`                            | imperative fn  | empties the queued message body for that level      |
|  [03]   | `destroyAnnouncer(): void`                                                                | imperative fn  | removes both region nodes; teardown-only, singleton |

## [04]-[IMPLEMENTATION_LAW]

[ANNOUNCER_TOPOLOGY]:
- a lazily-created singleton owns two visually-hidden `role="log"` nodes in `document.body` — one `aria-live="assertive"`, one `aria-live="polite"` — minted on the first `announce` and reused thereafter, so `announce` is the constructor
- `assertiveness` defaults to `'polite'`; `'assertive'` interrupts the current utterance; the level selects which of the two persistent regions receives the text
- `timeout` clears the message after the elapsed window so a stale utterance is not re-read on focus; omitted leaves the message resident until the next `announce`/`clearAnnouncer`
- the `{ 'aria-labelledby': string }` message form sets `aria-labelledby` on the region instead of writing text, announcing existing DOM (an error region id, a status node) without duplicating its content
- the region pair is shared per document/process; `destroyAnnouncer` is global and belongs to full-page teardown or test cleanup, never a per-component unmount

[LOCAL_ADMISSION]:
- call `announce` for invisible state changes with no owning widget — async-load-complete, selection count, save receipt — where a toast or field-error region would be too heavy
- pass `timeout` for transient status; use `clearAnnouncer(level)` before swapping a major region to drop a stale queued message
- reserve `destroyAnnouncer` for teardown; competing manual live-region nodes beside this singleton are the deleted defect
- the `'off'` politeness of the role vocabulary has no live-region level here; filter it before the call rather than coercing to a default

[STACKING]:
- universal tier `effect`: the `interaction/announce.md#ACCESSIBILITY_BROADCAST` `announceRole` folds `announce` into `Match.value(announceFor(role))` — the `"off"` arm resolves to `Effect.void`, every other arm to `Effect.sync(() => announce(message, level, timeout))` — so the role vocabulary's `Politeness` (`'assertive' | 'polite' | 'off'`) narrows to this package's two-level axis at the one call site, the `"off"` short-circuit load-bearing because `announce` admits no off level
- sibling `react-stately`: `announce` (transient, screen-reader-only utterance) pairs with the `ToastQueue` external store (persistent, visible + SR toast) as the two distinct broadcast surfaces `announce.md` composes on one page — a status with a visible affordance is a toast, a pure utterance is an `announce`
- sibling `react-aria` / `react-aria-components`: managed, widget-owned live regions are `useToastRegion`/`useLandmark` and `UNSTABLE_ToastRegion` (with F6 landmark navigation); the standalone `announce` is only for broadcasts with no component to own the region, and `react-aria` re-exports the identical function from `react-aria/private/live-announcer/LiveAnnouncer`

[RAIL_LAW]:
- package: `@react-aria/live-announcer`
- owns: the one imperative, process-wide ARIA live-region pair and its assertive/polite text broadcast
- accept: a plain string or an `{ 'aria-labelledby': string }` DOM reference, an `'assertive' | 'polite'` level, an optional clear timeout
- reject: managing live-region nodes directly, minting a second competing live region, coercing an `'off'` politeness to a default announce level, calling `destroyAnnouncer` outside teardown, importing `Assertiveness`/`Message` as types (they are not exported)
