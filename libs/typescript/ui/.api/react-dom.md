# [TS_UI_API_REACT_DOM]

`react-dom` is the DOM host renderer for the React 19 element tree. Its surface splits across strata by concern, not by whim: the `ui` view plane composes `createPortal` (overlays escape their subtree), `flushSync` (synchronous commit for focus restoration and View Transitions), the resource-hint family (`preload`/`preinit`/`preconnect` for GLB, wasm, fonts, and tiles), and `useFormStatus` (form-submit pending); the `browser` boot lane owns `createRoot`/`hydrateRoot` and their error-callback `RootOptions`; and the `edge` render lane owns the streaming SSR entries (`renderToReadableStream`, `renderToPipeableStream`, `prerender`). The runtime-split subpaths — `./client`, `./server.node`, `./server.edge`, `./server.browser`, `./static.node`, `./static.edge` — mirror the `@effect/platform-node`/`-browser` fence: a browser bundle imports `./client` and `./server.browser`, never a `node:`-bound server entry. `react-dom` ships no bundled types; the declaration surface is the separate `@types/react-dom` package. Within `ui` the renderer is a view-plane capability (portal + flush); boot and SSR are `browser`/`edge` concerns — `ui` never imports `browser`, so it reaches `createRoot` only through the app composition root.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-dom`
- package: `react-dom`
- license: `MIT`
- react-peer: `react ^catalog` (lockstep — the renderer and reconciler ship and version together)
- asset: runtime library shipping `.js` only; types are the separate `@types/react-dom` (`catalog`, MIT) DefinitelyTyped package — `api resolve react-dom` finds the package root but zero bundled `.d.ts`, so the type gate runs against `@types/react-dom`
- exports (runtime-split): `.` (portal/flush/hints/form), `./client` (`createRoot`/`hydrateRoot`), `./server.node` (`renderToPipeableStream`), `./server.edge`/`./server.browser`/`./server.bun` (`renderToReadableStream`), `./static.node`/`./static.edge` (`prerender`), `./profiling`, `./test-utils`
- catalog-verdict: KEEP

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: root, portal, and resource-hint option shapes
- rail: browser/boot + view/primitive
- The root handle and the option records for boot, hydration, and resource hints. `RootOptions` carries the error-callback trio that pairs with `react-error-boundary`.

| [INDEX] | [SYMBOL]                                                                                                    | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                                                                                                                                        |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :--------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Root` (`.render(children)` / `.unmount()`)                                                                 | root handle      | `browser/boot` — the mounted-tree handle; one root per app, `unmount` on teardown                                                                                          |
|  [02]   | `RootOptions` (`identifierPrefix`, `onCaughtError`, `onUncaughtError`, `onRecoverableError`)                | boot policy      | `browser/boot` — `onCaughtError` fires for errors an error boundary caught, `onUncaughtError` for those that escaped, `onRecoverableError` for hydration mismatch recovery |
|  [03]   | `HydrationOptions`                                                                                          | hydration policy | `browser/boot` SSR-hydration path; `RootOptions` plus hydration-mismatch handling                                                                                          |
|  [04]   | `ReactPortal`                                                                                               | portal node      | `view/primitive` — the `createPortal` return; a React node rendering into a foreign container                                                                              |
|  [05]   | `PreloadOptions` / `PreinitOptions` / `PreconnectOptions` / `PreloadModuleOptions` / `PreinitModuleOptions` | hint options     | `view` + `viewer` — `as`/`crossOrigin`/`integrity`/`fetchPriority` for the resource the hint fetches                                                                       |
|  [06]   | `FormStatus` (`FormStatusPending \| FormStatusNotPending`)                                                  | form state       | `view/compose` — `useFormStatus` return; `pending`, `data` (`FormData`), `method`, `action` during submit                                                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: view-plane DOM capability — portal, flush, hints, form
- rail: view/primitive
- The surface `ui` composes directly. `createPortal` and `flushSync` are the overlay and imperative-commit seams; the resource hints prefetch heavy `viewer` assets; the form hooks read submit state.

| [INDEX] | [SURFACE]                                                                               | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                                                                                                               |
| :-----: | :-------------------------------------------------------------------------------------- | :-------------- | :------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `createPortal(children, container: Element \| DocumentFragment, key?): ReactPortal`     | portal          | `view/primitive` — roots `react-aria` `Overlay`/`PortalProvider` so overlays escape `overflow`/`z-index` context                                  |
|  [02]   | `flushSync<R>(fn: () => R): R`                                                          | sync commit     | `view`/`act` — forces a synchronous commit for `react-aria` `FocusScope` restoration and before `document.startViewTransition`                    |
|  [03]   | `preload(href, options)` / `preinit(href, options)` / `preloadModule` / `preinitModule` | resource hint   | `viewer` — prefetch/eval the meshopt-decoder wasm, GLB assets, geo tiles, and fonts ahead of render                                               |
|  [04]   | `preconnect(href, options?)` / `prefetchDNS(href)`                                      | connection hint | `viewer` geo/tile origins — warm the connection before the first asset request                                                                    |
|  [05]   | `useFormStatus(): FormStatus`                                                           | form state      | `view/compose` `FormBinding` submit button — `pending`/`data` without prop-threading from the `<form>`                                            |
|  [06]   | `requestFormReset(form: HTMLFormElement)`                                               | form reset      | `view/compose` — imperative uncontrolled-form reset after a successful action                                                                     |
|  [07]   | `version` / `useFormState` / `unstable_batchedUpdates`                                  | retired / meta  | `version` gates feature detection; `useFormState` is superseded by React `useActionState`; `unstable_batchedUpdates` is retired (19 auto-batches) |

[ENTRYPOINT_SCOPE]: boot + SSR lanes — browser/edge owned, out of ui-core
- rail: browser/boot + edge/render
- The client boot (`browser`) and the server render (`edge`). `ui` never imports these; they are named here to fix the seam `react-error-boundary` and the runtime fence bind to.

| [INDEX] | [SURFACE]                                                                                                      | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                                                             |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | `createRoot(container, options?: RootOptions): Root` (`./client`)                                              | client boot    | `browser/boot` — the one mount; `onCaughtError`/`onUncaughtError` pair with the `react-error-boundary` tree     |
|  [02]   | `hydrateRoot(container, initialChildren, options?: HydrationOptions): Root` (`./client`)                       | hydration boot | `browser/boot` — attach to SSR markup; `onRecoverableError` reports hydration divergence                        |
|  [03]   | `renderToReadableStream(children, options?)` (`./server.edge` / `./server.browser`)                            | edge stream    | `edge/render` — Web-Streams SSR for edge/worker runtimes; the browser-safe server entry                         |
|  [04]   | `renderToPipeableStream(children, options?)` (`./server.node`)                                                 | node stream    | `edge/render` node lane — Node-stream SSR with backpressure; the `node:`-bound entry banned in a browser bundle |
|  [05]   | `renderToString` / `renderToStaticMarkup`                                                                      | sync render    | `edge` non-streaming recovery and static-markup emit (email, snapshot)                                          |
|  [06]   | `prerender` / `prerenderToNodeStream` / `resumeAndPrerender` / `resumeAndPrerenderToNodeStream` (`./static.*`) | prerender      | `edge` static + resumable prerender (partial pre-render, resume on request)                                     |

## [04]-[IMPLEMENTATION_LAW]

[RENDERER_TOPOLOGY]:
- One renderer, surface split by stratum: the `.` subpath is the isomorphic view-plane capability (`createPortal`/`flushSync`/hints/form); `./client` is the `browser` boot; `./server.*`/`./static.*` are the `edge` render lanes. A folder imports only the subpath its stratum owns — `ui` takes `.`, `browser` takes `./client`, `edge` takes a server entry.
- The runtime-split subpaths are a purity fence: `./server.node` is `node:`-bound and must never enter a browser bundle; `./server.edge`/`./server.browser` are Web-Streams-based for edge/worker/browser. This mirrors the `@effect/platform-node` vs `@effect/platform-browser` split — the same `runtime:node` / `runtime:browser` ledger the edge enforces, applied to the renderer.
- `react-dom` is the imperative edge: nothing renders until a `Root` mounts. In an Effect app the `@effect/platform-browser` `BrowserRuntime.runMain` boots the runtime and `createRoot(el).render(<App/>)` mounts the tree the runtime's services back; `flushSync` is the seam where an Effect-driven imperative action (focus, scroll-into-view) forces a synchronous commit before reading layout.
- Types are external: react-dom ships runtime `.js` only. `@types/react-dom` is the declaration surface, versioned independently and pinned in the workspace catalog alongside `@types/react`; a react-dom bump without a matching `@types/react-dom` bump is the drift defect.

[STACKS_WITH]:
- `react-aria` (`.api/react-aria.md`): `createPortal` is what `Overlay`/`PortalProvider` root through, so popovers, modals, and toasts escape the triggering subtree; `flushSync` gives `FocusScope` the synchronous DOM it needs to move focus deterministically after an overlay opens or closes.
- `react-error-boundary` (`.api/react-error-boundary.md`): the `RootOptions` error trio is the boundary's outer frame — `onCaughtError` observes what an `ErrorBoundary` caught (for `telemetry`), `onUncaughtError` catches what escaped every boundary, `onRecoverableError` reports hydration mismatches. The boundary catches render errors; the root callbacks catch the rest.
- `@effect/platform-browser` (`libs/typescript/.api/effect-platform-browser.md`): `BrowserRuntime.runMain` is the single boot the `browser` folder owns; `createRoot().render()` mounts inside it. The renderer and the Effect runtime share one imperative edge, not two.
- `@effect/platform-node`/`-bun` (`libs/typescript/.api/effect-platform-node.md`): the `edge` SSR lane pairs `renderToPipeableStream` (node) / `renderToReadableStream` (edge/bun) with the matching platform runtime — the renderer's runtime-split subpaths track the platform package's runtime split one-to-one.
- `viewer` decode pipeline: `preload`/`preinit` warm the meshopt-decoder wasm and GLB/tile assets the `browser` decode-worker port will consume, so the fetch overlaps render instead of blocking first frame.

[LOCAL_ADMISSION]:
- In `ui`, import only the `.` subpath — `createPortal`, `flushSync`, the resource hints, and `useFormStatus`. Reaching `createRoot`/`hydrateRoot` from a `ui` row is the boot-in-a-component defect; boot is `browser`'s, wired at the app root.
- Never import a `./server.node` entry into a browser bundle or a `./server.edge` entry expecting `node:` APIs — the subpath is the runtime contract the purity gate audits.
- Root every overlay through `createPortal` via the `react-aria` `Overlay` seam; never mount a detached portal container by hand.
- Reach for `flushSync` only at the focus/measure/transition seam; a `flushSync` inside render or an event handler that does not need synchronous layout is the anti-pattern React 19 auto-batching removes the need for.
- Keep `react-dom` and `@types/react-dom` versions moving together in the workspace catalog.

[RAIL_LAW]:
- Package: `react-dom`
- Owns: the React 19 DOM renderer — `createPortal`/`flushSync` and the resource-hint + form-status view capability, the `createRoot`/`hydrateRoot` boot, and the `renderToReadableStream`/`renderToPipeableStream`/`prerender` SSR lanes, split across runtime subpaths
- Accept: `.`-subpath portal/flush/hints/form in `ui`, `./client` boot in `browser` with the `RootOptions` error trio, the runtime-matched `./server.*`/`./static.*` entry in `edge`, `@types/react-dom` as the versioned type surface, `flushSync` only at the focus/measure/transition seam
- Reject: `createRoot`/`hydrateRoot` inside a `ui` row, a `node:`-bound server entry in a browser bundle, a hand-mounted portal container, gratuitous `flushSync`, a react-dom/`@types/react-dom` version split
