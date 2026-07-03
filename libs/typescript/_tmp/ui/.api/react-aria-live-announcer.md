# [API_CATALOGUE] @react-aria/live-announcer

`@react-aria/live-announcer` provides three imperative functions that manage an ARIA live region injected into the DOM for screen-reader announcement. `announce` posts a message at the requested assertiveness level, `clearAnnouncer` flushes the queue for a given level, and `destroyAnnouncer` removes the live region node from the DOM. The package re-exports from `react-aria`'s internal live-announcer module; the function signatures are identical to those in `react-aria/private/live-announcer/LiveAnnouncer`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@react-aria/live-announcer`
- package: `@react-aria/live-announcer`
- module: `@react-aria/live-announcer`
- namespace: `@react-aria/live-announcer`
- asset: runtime utility
- rail: accessibility / announcements

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: announcement types
- rail: accessibility

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :-------------- | :------------ | :---------------------------------------- |
|  [01]   | `Assertiveness` | type alias    | `'assertive' \| 'polite'`                 |
|  [02]   | `Message`       | type alias    | `string \| { 'aria-labelledby': string }` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: live-region operations
- rail: accessibility

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `announce(message, assertiveness?, timeout?)` | imperative fn  | posts message to ARIA live region      |
|  [02]   | `clearAnnouncer(assertiveness)`               | imperative fn  | flushes queued messages at given level |
|  [03]   | `destroyAnnouncer()`                          | imperative fn  | removes live region DOM node           |

## [04]-[IMPLEMENTATION_LAW]

[ANNOUNCER_TOPOLOGY]:
- the package re-exports `announce`, `clearAnnouncer`, `destroyAnnouncer` from `react-aria/private/live-announcer/LiveAnnouncer`
- `announce` creates the live region on first call if absent; subsequent calls reuse the same node
- `assertiveness` defaults to `'polite'` when omitted; `'assertive'` interrupts the current screen-reader utterance
- `timeout` controls how long the message remains in the live region before being cleared
- `Message` accepts either a plain string or an `{ 'aria-labelledby': string }` object for element-referenced announcements

[LOCAL_ADMISSION]:
- call `announce` after user actions that produce invisible state changes (e.g. async load complete, selection update)
- call `clearAnnouncer` before unmounting a major UI region to prevent stale announcements
- call `destroyAnnouncer` only in test teardown or full-page unmount scenarios; the live region is shared process-wide

[RAIL_LAW]:
- package: `@react-aria/live-announcer`
- owns: imperative ARIA live-region management
- accept: plain string messages or `aria-labelledby` references, `'assertive' | 'polite'` assertiveness level
- reject: managing live-region DOM nodes directly, creating multiple competing live-region elements
