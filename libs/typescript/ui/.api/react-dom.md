# [TS_UI_API_REACT_DOM]

`react-dom` renders the React element tree to the DOM, its surface split across runtime subpaths by stratum: the `.` view plane a `ui` row composes, the `./client` boot, and the `./server.*`/`./static.*` SSR and prerender lanes. Subpath choice pins the runtime contract — a `node:`-bound server entry never enters a browser bundle, mirroring the `@effect/platform-node`/`-browser` fence, and `ui` reaches `createRoot` only through the app composition root.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-dom`
- package: `react-dom` (MIT)
- module: ESM/CJS runtime-split `exports` — `.` (portal/flush/hints/form), `./client` (`createRoot`/`hydrateRoot`), `./server.node` (`renderToPipeableStream`), `./server.edge`/`./server.browser`/`./server.bun` (`renderToReadableStream`), `./static.node`/`./static.edge` (`prerender`)
- runtime: isomorphic, the runtime contract pinned per subpath so a `node:`-bound server entry never enters a browser bundle; peer `react` — renderer and reconciler ship in lockstep
- asset: runtime library shipping `.js` only, zero bundled `.d.ts`; `@types/react-dom` (`.api/types-react-dom.md`) is the declaration surface the type gate binds
- rail: the `ui` DOM-commit edge — the `.` view plane every `ui` row composes, its boot and SSR lanes app-root-owned

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: none owned — `react-dom` ships `.js` only
- `@types/react-dom` (`.api/types-react-dom.md`) declares every option and result type the `[03]` signatures name — `RootOptions`, `HydrationOptions`, `FormStatus`, `ReactPortal`, the resource-hint option space — and the `onUncaughtError`/`onCaughtError`/`onRecoverableError` triple; their shapes live once at that declaration owner.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the `.` view plane a `ui` row composes — portal, flush, resource hints, form status

| [INDEX] | [SURFACE]                                              | [CAPABILITY]    | [CONSUMER]                                                    |
| :-----: | :----------------------------------------------------- | :-------------- | :------------------------------------------------------------ |
|  [01]   | `createPortal(children, container, key?): ReactPortal` | portal          | roots the `react-aria` `Overlay`/`UNSAFE_PortalProvider` seam |
|  [02]   | `flushSync<R>(fn: () => R): R`                         | sync commit     | `FocusScope` restore, before `startViewTransition`            |
|  [03]   | `preload(href, options)` / `preinit(href, options)`    | resource hint   | prefetch/eval meshopt wasm, GLB, tiles, fonts pre-render      |
|  [04]   | `preloadModule` / `preinitModule`                      | module hint     | ESM `preload`/`preinit` variants, `(href, options)`           |
|  [05]   | `preconnect(href, options?)` / `prefetchDNS(href)`     | connection hint | warm a `viewer` geo/tile origin before first fetch            |
|  [06]   | `useFormStatus(): FormStatus`                          | form status     | submit button pending/data without prop-threading             |
|  [07]   | `requestFormReset(form: HTMLFormElement)`              | form reset      | imperative uncontrolled-form reset after success              |
|  [08]   | `unstable_batchedUpdates(cb, a)` / `version`           | interop         | non-React-event batching; renderer version string             |

[ENTRYPOINT_SCOPE]: boot + SSR lanes — `browser`/`edge` owned; a `ui` row never imports them, and `createRoot`/`hydrateRoot` return `Root`

| [INDEX] | [SURFACE]                                                 | [CAPABILITY]   | [CONSUMER]                                                  |
| :-----: | :-------------------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `createRoot(container, options?: RootOptions): Root`      | client boot    | the one mount; error trio pairs with `react-error-boundary` |
|  [02]   | `hydrateRoot(container, initialChildren, options?): Root` | hydration boot | attach to SSR markup; `onRecoverableError` on divergence    |
|  [03]   | `renderToReadableStream(children, options?)`              | edge stream    | Web-Streams SSR for edge/worker; browser-safe server entry  |
|  [04]   | `renderToPipeableStream(children, options?)`              | node stream    | Node-stream SSR; `node:`-bound, banned in a browser bundle  |
|  [05]   | `renderToString` / `renderToStaticMarkup`                 | sync render    | non-streaming recovery + static markup (email, snapshot)    |
|  [06]   | `prerender` / `prerenderToNodeStream`                     | prerender      | `edge`/`node` static partial prerender                      |
|  [07]   | `resumeAndPrerender` / `resumeAndPrerenderToNodeStream`   | resume         | resume a partial prerender on request                       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `react-dom` is the imperative edge: nothing renders until a `Root` mounts, and `flushSync` is the sole seam forcing a synchronous commit before an Effect-driven imperative action (focus, scroll, transition capture) reads layout.

[STACKING]:
- `react-aria`(`.api/react-aria.md`): `Overlay`/`UNSAFE_PortalProvider` root through `createPortal` so popovers, modals, and toasts escape the triggering subtree; `flushSync` gives `FocusScope` the synchronous DOM to move focus deterministically after an overlay toggles.
- `react-error-boundary`(`.api/react-error-boundary.md`): the `RootOptions` error triple frames the boundary — `onCaughtError` observes what an `ErrorBoundary` caught, `onUncaughtError` catches what escaped every boundary, `onRecoverableError` reports hydration mismatches.
- `@effect/platform-browser`(`libs/typescript/.api/effect-platform-browser.md`): `BrowserRuntime.runMain` boots the runtime and `createRoot(el).render(<App/>)` mounts the tree its services back, the renderer and Effect runtime sharing one imperative edge.
- `@effect/platform-node`(`libs/typescript/.api/effect-platform-node.md`): the `edge` SSR lane pairs `renderToPipeableStream` (node) / `renderToReadableStream` (edge/bun) with the matching platform runtime, the renderer's runtime-split subpaths tracking the platform split one-to-one.
- `viewer` decode pipeline: `preload`/`preinit` warm the meshopt-decoder wasm and GLB/tile assets the `browser` decode-worker port consumes, overlapping fetch with render instead of blocking first frame.

[LOCAL_ADMISSION]:
- Each folder imports only its stratum's subpath — `ui` takes `.`, `browser` takes `./client`, `edge` takes a `./server.*`/`./static.*` entry; a cross-stratum import is the defect the purity gate audits.
- `react-dom` and `@types/react-dom` move together — a runtime bump without the matching type bump is the drift defect.

[RAIL_LAW]:
- Package: `react-dom`
- Owns: the React DOM renderer — `createPortal`/`flushSync` and the resource-hint + form-status view capability, the `createRoot`/`hydrateRoot` boot, and the `renderToReadableStream`/`renderToPipeableStream`/`prerender` SSR lanes, split across runtime subpaths
- Accept: `.`-subpath portal/flush/hints/form in `ui`, `./client` boot in `browser` with the `RootOptions` error trio, the runtime-matched `./server.*`/`./static.*` entry in `edge`, `@types/react-dom` as the declaration type surface, `flushSync` only at the focus/measure/transition seam
- Reject: `createRoot`/`hydrateRoot` inside a `ui` row, a `node:`-bound server entry in a browser bundle, a hand-mounted portal container, gratuitous `flushSync`
