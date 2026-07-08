# [TS_UI_API_TYPES_REACT_DOM]

`@types/react-dom` is the declaration-only (`.d.ts`) type surface for the `react-dom` runtime (`.api/react-dom.md`), peer-locked to `@types/react` (`.api/types-react.md`) and sharing its `ReactNode`/`ReactElement`/`Ref`/`Container` vocabulary. It types three disjoint planes across its subpaths: `react-dom/client` is the mount (`createRoot`/`hydrateRoot` — an app-boot concern), `react-dom` is the in-tree DOM API a `view` row reaches for (`createPortal`, `flushSync`, the resource hints, `useFormStatus`), and `react-dom/server` + `react-dom/static` are the SSR/prerender streaming. Two of its families are parameterized spaces, not flat lists: the resource-hint family is one preload space discriminated by `as` (`preload`/`preinit`/`preloadModule`/`preinitModule`/`prefetchDNS`/`preconnect`), and the render family is one `renderTo*`/`prerender*` space parameterized by target runtime (pipeable Node stream, Web `ReadableStream`, string, static). React 19 collapses the mount to `createRoot`/`hydrateRoot` with the `onUncaughtError`/`onCaughtError`/`onRecoverableError` handler triple; `ReactDOM.render` is gone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@types/react-dom`
- package: `@types/react-dom`
- license: `MIT` (DefinitelyTyped)
- peer: `@types/react` (declared peer; shares `ReactNode`/`ReactElement`/`Ref`/`Component`); version-locked to the runtime major (19)
- deps: none
- catalog-verdict: KEEP
- asset: declaration-only — NO runtime, NO ABI; `tsc`/`tsgo` is the gate. The `react-dom` runtime (`.api/react-dom.md`) is the paired package
- exports: `.` (in-tree DOM API), `./client` (`createRoot`/`hydrateRoot`), `./server` + `./server.{node,browser,bun,edge}` (SSR streaming), `./static` + `./static.{node,browser,edge}` (prerender), `./canary`, `./experimental`, `./test-utils` (deprecated)
- marker: the server/static subpaths declare ambient `ReadableStream`/`WritableStream`/`AbortSignal` global stubs — a Node vs Web runtime is selected by subpath, never a flag

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client mount types — the root + React 19 error triple
- rail: app-boot / browser
- The `createRoot`/`hydrateRoot` option and result types. React 19 folds error handling into the root: `onUncaughtError`/`onCaughtError`/`onRecoverableError` on `RootOptions`/`HydrationOptions` replace the ad-hoc boundary-only model, and `identifierPrefix` scopes `useId`. `ui` never imports `browser`, so these are consumed at the app composition root that boots the tree — not inside a `view` row.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:----------------------------------------------------------------------- |:---------------- |:----------------------------------------------------------------------- |
| [01] | `Root` (`{ render(children: ReactNode): void; unmount(): void }`) | root handle | app-boot — the mounted tree handle; `render`/`unmount` drive the lifecycle |
| [02] | `RootOptions` / `HydrationOptions` | mount options | app-boot — carry the error triple + `identifierPrefix`; `HydrationOptions.formState` seeds SSR form state |
| [03] | `onUncaughtError` / `onCaughtError` / `onRecoverableError` (`ErrorInfo`) | error handler | app-boot — the React 19 root error handlers; `react-error-boundary` (sibling `view/primitive`) still owns in-tree recovery |
| [04] | `Container` (`Element \| DocumentFragment \| Document`) | mount target | app-boot — the DOM node `createRoot` mounts into |
| [05] | `ReactFormState` | opaque form state | SSR — the server-action form state threaded from `renderTo*` into `HydrationOptions.formState` |

[PUBLIC_TYPE_SCOPE]: the in-tree DOM API types — form status + the resource-hint parameter space
- rail: view/compose
- The types a `view` row reaches for: `FormStatus` for a form row's pending state, and the resource-hint option types. `preload`/`preinit`/`preloadModule`/`preinitModule` share one option shape discriminated by `as` (`PreloadAs`/`PreinitAs`) — one preload space, not four mechanisms.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:----------------------------------------------------------------------- |:---------------- |:----------------------------------------------------------------------- |
| [01] | `FormStatus` = `FormStatusPending \| FormStatusNotPending` | form status | `view/compose` `FormBinding` — `useFormStatus()` return; `pending`/`data`/`method`/`action` of the enclosing `<form>` |
| [02] | `PreloadOptions` (`as: PreloadAs`) / `PreloadAs` | preload param | `view` / app — asset hint options; `as` (`"script"`/`"style"`/`"font"`/`"image"`/…) discriminates the hint |
| [03] | `PreinitOptions` (`as: PreinitAs`) / `PreinitModuleOptions` | preinit param | `view` / app — eager script/style init; `PreinitAs` = `"script" \| "style"` |
| [04] | `PreconnectOptions` (`crossOrigin?`) | connect param | app — `preconnect`/`prefetchDNS` origin hints for the viewer tile/CDN host |

[PUBLIC_TYPE_SCOPE]: the server + static streaming types — one render space, per runtime
- rail: app-ssr
- The SSR/prerender option and result types. `RenderToPipeableStreamOptions` (Node) and `RenderToReadableStreamOptions` (Web) share the bootstrap/import-map/shell-callback vocabulary; the runtime is chosen by subpath. `PostponedState` is the resumable-prerender opaque state — the render family is parameterized by (target runtime, blocking-vs-resumable), never a separate API per host.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:----------------------------------------------------------------------- |:---------------- |:----------------------------------------------------------------------- |
| [01] | `RenderToPipeableStreamOptions` / `PipeableStream` | Node stream | app-ssr — `bootstrapScripts`/`onShellReady`/`onAllReady`/`progressiveChunkSize`; the Node SSR shell |
| [02] | `RenderToReadableStreamOptions` / `ServerOptions` | Web stream | app-ssr (edge) — the `ReadableStream` variant options; `signal` aborts the stream |
| [03] | `BootstrapScriptDescriptor` / `ReactImportMap` | shell asset | app-ssr — bootstrap script `src`/`integrity`/`crossOrigin`; the `<script type="importmap">` payload |
| [04] | `PostponedState` / `ResumeOptions` | resumable state | app-ssr — the opaque JSON-serializable prerender state `resume`/`resumeAndPrerender` continue from |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the client mount — `createRoot` / `hydrateRoot`
- rail: app-boot / browser
- The typed mount API. There is one mount for CSR (`createRoot`) and one for SSR-hydration (`hydrateRoot`); `ReactDOM.render`/`hydrate` are removed. Called at the app root that boots `ui` components — `ui` declares its runtime ports, `browser` provides Layers, the app root mounts.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `createRoot(container: Container, options?: RootOptions): Root` | CSR mount | app-boot — the client-render root; `Root.render(<App/>)` mounts the `ui` tree |
| [02] | `hydrateRoot(container, initialChildren: ReactNode, options?: HydrationOptions): Root` | SSR hydrate | app-boot — attach to server HTML; `options.formState` seeds the SSR-computed form state |

[ENTRYPOINT_SCOPE]: the in-tree DOM operations — portal, commit, resource hints, form status
- rail: view/primitive
- The DOM API a `view` row calls. `createPortal` roots overlays outside the subtree; `flushSync` forces a synchronous commit; the resource hints are one parameterized preload space; `useFormStatus`/`requestFormReset` are the form-action DOM seam.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `createPortal(children: ReactNode, container: Element \| DocumentFragment, key?: string): ReactPortal` | portal | `view/primitive` — `react-aria` overlays (`Overlay`/`PortalProvider`) root through this to escape `overflow`/`z-index` |
| [02] | `flushSync<R>(fn: () => R): R` | sync commit | `act/transition` + `view/primitive` — force the commit `FocusScope` restore and the View-Transition capture depend on |
| [03] | `preload` / `preinit` / `preloadModule` / `preinitModule` / `prefetchDNS` / `preconnect` | resource hint | `view` / app — one preload space discriminated by `as`; hint the viewer tile/font/model asset before use |
| [04] | `useFormStatus(): FormStatus` / `requestFormReset(form: HTMLFormElement)` | form action | `view/compose` `FormBinding` — read enclosing-form pending state; reset an uncontrolled form after a server action |
| [05] | `unstable_batchedUpdates<A, R>(cb, a)` / `version` | retired/identity | interop only — React 19 auto-batches, so `unstable_batchedUpdates` is a retired no-op-equivalent |

[ENTRYPOINT_SCOPE]: the server + static render — one space, per runtime target
- rail: app-ssr
- The SSR/prerender functions. `renderTo*` streams a live shell; `prerender*` produces static/resumable output. The target runtime is the subpath (`./server.node` vs `./server.edge`); the shape is uniform.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `renderToPipeableStream(children, options?): PipeableStream` | Node SSR | app-ssr — the streaming Node shell; pairs `@effect/platform` `HttpServer` response |
| [02] | `renderToReadableStream(children, options?): Promise<ReactDOMServerReadableStream>` | Web SSR | app-ssr (edge) — the `ReadableStream` shell for edge/worker runtimes |
| [03] | `renderToString` / `renderToStaticMarkup` | sync SSR | app-ssr — non-streaming string render; `renderToStaticMarkup` drops React attrs for emails/static pages |
| [04] | `prerender` / `prerenderToNodeStream` / `resume` / `resumeAndPrerender` | prerender | app-ssr — static prerender + `PostponedState` resume; the partial-prerender/PPR seam |

## [04]-[IMPLEMENTATION_LAW]

[RENDERER_TOPOLOGY]:
- Three disjoint planes by subpath: `react-dom/client` mounts (`createRoot`/`hydrateRoot` — app boot), `react-dom` is the in-tree DOM API (`createPortal`/`flushSync`/resource-hints/`useFormStatus` — `view` rows), and `react-dom/server`+`react-dom/static` stream SSR/prerender output (app SSR). A `ui` row touches only the middle plane; the mount and the stream are app/`browser`-root concerns because `ui` never imports `browser`.
- The resource-hint family is ONE preload space: `preload`/`preinit`/`preloadModule`/`preinitModule` share the option shape discriminated by `as` (`PreloadAs`/`PreinitAs`), and `prefetchDNS`/`preconnect` are the origin-hint arm. A new asset hint is a call with a different `as`, never a new function family.
- The render family is ONE space parameterized by (runtime target, blocking-vs-resumable): `renderToPipeableStream` (Node) and `renderToReadableStream` (Web) are the same shell over different stream primitives; `prerender*`/`resume*` are the resumable arm. The subpath picks the runtime.

[REACT_19_LAW]:
- `createRoot`/`hydrateRoot` are the only mounts — `ReactDOM.render`/`hydrate`/`unmountComponentAtNode` are removed. Error handling is root-level: `onUncaughtError`/`onCaughtError`/`onRecoverableError` on `RootOptions`/`HydrationOptions` are the app-wide net, while `react-error-boundary` (sibling `view/primitive`) owns in-tree recovery.
- `useFormState` is deprecated → `useActionState` moved to `react` (`.api/types-react.md`); `react-dom` keeps `useFormStatus` (ambient form pending state) and `requestFormReset`. React 19 auto-batches, so `unstable_batchedUpdates` is interop-only.
- Resource hints and document metadata are native: `preload`/`preinit` warm assets and React 19 hoists `<title>`/`<meta>`/`<link>` rendered anywhere in the tree, so the viewer's tile/font/model assets are hinted declaratively rather than through a head-manager library.

[STACKS_WITH]:
- `@types/react` (`.api/types-react.md`): the peer — shares `ReactNode`/`ReactElement`/`Ref`/`Component`; `createPortal` returns a `ReactPortal`, `hydrateRoot`/`renderTo*` take `ReactNode`, and `flushSync` forces the commit the React-side `ViewTransition` capture and `FocusScope` restore need. The DOM renderer types over the element types.
- `react-dom` (`.api/react-dom.md`): the paired runtime these types type; import named ESM symbols from the matching subpath (`import { createPortal, flushSync } from "react-dom"`, `import { createRoot } from "react-dom/client"`).
- `react-aria` (`.api/react-aria.md`): overlays root through `createPortal` (via `Overlay`/`PortalProvider`) to escape the DOM subtree; `flushSync` forces the synchronous commit `FocusScope` focus-restoration depends on. `react-dom` owns the portal + commit, `react-aria` the overlay behavior.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): SSR dehydrates the `Registry` into the HTML `renderTo*` emits and `HydrationBoundary` rehydrates it before children read; `hydrateRoot`'s `formState` and the atom hydration ride the same server-render pass, so a server-computed `AtomHttpApi` result renders without a client refetch.
- `@effect/platform` (`libs/typescript/.api/effect-platform.md`): `renderToPipeableStream`/`renderToReadableStream` feed the `HttpServerResponse` body; the SSR render is an `Effect` producing the stream, and the platform runtime that served the request owns the shell lifecycle and abort `signal`.
- `effect` `Schema` (`libs/typescript/.api/effect.md`): `ReactFormState`/`useFormStatus` carry the server-action result a `Schema` decodes at the boundary; a `FormBinding` folds the `FormStatus.pending`/`data` with the decoded value, so form submission state and wire decode share one owner.

[LOCAL_ADMISSION]:
- In a `view` row use only the in-tree DOM API — `createPortal` for overlays, `flushSync` where a synchronous commit is load-bearing, the resource hints for assets, `useFormStatus` for form state; leave `createRoot`/`hydrateRoot` and `renderTo*` to the app/`browser` boot root.
- Root overlays through `react-aria`'s `Overlay`/`PortalProvider` (which owns `createPortal`), not a bare `createPortal` call — the aria layer owns dismiss/focus/scroll-lock around the portal.
- Reach for `flushSync` only where a downstream synchronous read requires it (focus restore, transition capture); default to React's async commit. Never call `unstable_batchedUpdates` — React 19 auto-batches.
- Hint assets with `preload`/`preinit` + the correct `as`; never inject `<link rel=preload>` by hand or through a head-manager — React 19 hoists metadata natively.

[RAIL_LAW]:
- Package: `@types/react-dom`
- Owns: the DOM renderer type surface across three planes — the `react-dom/client` mount (`createRoot`/`hydrateRoot`, `RootOptions`/`HydrationOptions` + the React 19 error triple), the in-tree DOM API (`createPortal`/`flushSync`/`useFormStatus`/`requestFormReset` + the `preload`/`preinit`/`prefetchDNS`/`preconnect` resource-hint space), and the `react-dom/server`+`react-dom/static` render space (`renderToPipeableStream`/`renderToReadableStream`/`renderToString`/`prerender`/`resume`)
- Accept: `createPortal`/`flushSync` in `view` rows (via `react-aria` overlays), the resource hints discriminated by `as`, `useFormStatus` in a `FormBinding`, `createRoot`/`hydrateRoot` at the app boot root, `renderTo*` behind the `@effect/platform` SSR response, the peer `@types/react` element vocabulary
- Reject: `ReactDOM.render`/`hydrate`/`unmountComponentAtNode` (removed), `useFormState` (→ `react` `useActionState`), `unstable_batchedUpdates` for batching (auto in 19), a bare `createPortal` without the `react-aria` overlay layer, hand-injected `<link rel=preload>`/head-manager metadata, `createRoot`/`renderTo*` inside a `ui` `view` row
