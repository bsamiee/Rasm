# [TS_UI_API_TYPES_REACT_DOM]

`@types/react-dom` is the declaration-only `.d.ts` surface for the `react-dom` runtime (`.api/react-dom.md`), peer-locked to `@types/react` (`.api/types-react.md`) and sharing its `ReactNode`/`ReactElement`/`Ref` vocabulary across three disjoint planes: `react-dom/client` mounts (`createRoot`/`hydrateRoot`), `react-dom` the in-tree DOM API a `view` row reaches for (`createPortal`/`flushSync`/resource hints/`useFormStatus`), and `react-dom/server`+`react-dom/static` the SSR/prerender streaming.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@types/react-dom`
- package: `@types/react-dom` (MIT)
- module: declaration-only `.d.ts`; subpaths `.` (in-tree DOM API), `./client` (`createRoot`/`hydrateRoot`), `./server` + `./server.{node,browser,bun,edge}` (SSR streaming), `./static` + `./static.{node,browser,edge}` (prerender), `./canary`, `./experimental`, `./test-utils` (deprecated)
- asset: no runtime, no ABI — `tsc` is the gate; peer-locked to `@types/react` (`.api/types-react.md`) sharing `ReactNode`/`ReactElement`/`Ref`, and consumed as the `react-dom` runtime's types (`.api/react-dom.md`)
- marker: the server/static subpaths declare ambient `ReadableStream`/`WritableStream`/`AbortSignal` global stubs — a Node vs Web runtime is selected by subpath, never a flag
- rail: the DOM-renderer type surface — `tsc` gates the three planes (client mount, in-tree DOM API, server/static render), no runtime output to test

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client mount types, the root + React error triple — `createRoot`/`hydrateRoot` option and result types fold error handling into the root, `onUncaughtError`/`onCaughtError`/`onRecoverableError` on `RootOptions`/`HydrationOptions` replacing the ad-hoc boundary-only model and `identifierPrefix` scoping `useId`; `ui` never imports `browser`, so these are consumed at the app composition root that boots the tree, not inside a `view` row.

Every row is consumed at app-boot; `react-error-boundary` (sibling `view/primitive`) owns in-tree recovery.

| [INDEX] | [SYMBOL]                                                                 | [TYPE_FAMILY]     | [CONSUMER]                              |
| :-----: | :----------------------------------------------------------------------- | :---------------- | :-------------------------------------- |
|  [01]   | `Root` (`{ render(children: ReactNode): void; unmount(): void }`)        | root handle       | mounted tree handle; `render`/`unmount` |
|  [02]   | `RootOptions` / `HydrationOptions`                                       | mount options     | error triple + `identifierPrefix`       |
|  [03]   | `onUncaughtError` / `onCaughtError` / `onRecoverableError` (`ErrorInfo`) | error handler     | the React root error net                |
|  [04]   | `Container` (`Element \| DocumentFragment \| Document`)                  | mount target      | DOM node `createRoot` mounts into       |
|  [05]   | `ReactFormState`                                                         | opaque form state | server-action state → `formState`       |

[PUBLIC_TYPE_SCOPE]: the in-tree DOM API types — form status + the resource-hint parameter space — `FormStatus` types a form row's pending state, and `preload`/`preinit`/`preloadModule`/`preinitModule` share one option shape discriminated by `as` (`PreloadAs`/`PreinitAs`), one preload space rather than four mechanisms.

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [CONSUMER]                                  |
| :-----: | :---------------------------------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `FormStatus` = `FormStatusPending \| FormStatusNotPending`  | form status   | `view/compose` — `useFormStatus()` return   |
|  [02]   | `PreloadOptions` (`as: PreloadAs`) / `PreloadAs`            | preload param | `view`/app — asset hint; `as` discriminates |
|  [03]   | `PreinitOptions` (`as: PreinitAs`) / `PreinitModuleOptions` | preinit param | `view`/app — eager script/style init        |
|  [04]   | `PreconnectOptions` (`crossOrigin?`)                        | connect param | app — `preconnect`/`prefetchDNS` hints      |

[PUBLIC_TYPE_SCOPE]: the server + static streaming types, one render space per runtime — `RenderToPipeableStreamOptions` (Node) and `RenderToReadableStreamOptions` (Web) share the bootstrap/import-map/shell-callback vocabulary with the runtime chosen by subpath, and `PostponedState` is the resumable-prerender opaque state; the render family is parameterized by (target runtime, blocking-vs-resumable), never a separate API per host.

Every row is consumed at app-ssr (edge for the Web stream).

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]   | [CONSUMER]                                               |
| :-----: | :------------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `RenderToPipeableStreamOptions` / `PipeableStream` | Node stream     | `bootstrapScripts`/`onShellReady`/`onAllReady` shell     |
|  [02]   | `RenderToReadableStreamOptions` / `ServerOptions`  | Web stream      | `ReadableStream` variant; `signal` aborts                |
|  [03]   | `BootstrapScriptDescriptor` / `ReactImportMap`     | shell asset     | bootstrap `src`/`integrity`/`crossOrigin`; importmap     |
|  [04]   | `PostponedState` / `ResumeOptions`                 | resumable state | opaque prerender state `resume`/`resumeAndPrerender` use |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the client mount — `createRoot` (CSR) and `hydrateRoot` (SSR-hydration) are the only mounts, `ReactDOM.render`/`hydrate` removed; called at the app root that boots `ui` components, where `ui` declares its runtime ports, `browser` binds the Layers, and the app root mounts.

Both mounts run at app-boot.

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CONSUMER]           |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :------------------- |
|  [01]   | `createRoot(container: Container, options?: RootOptions): Root`                        | CSR mount      | mounts the `ui` tree |
|  [02]   | `hydrateRoot(container, initialChildren: ReactNode, options?: HydrationOptions): Root` | SSR hydrate    | attach server HTML   |

[ENTRYPOINT_SCOPE]: the in-tree DOM operations — portal, commit, resource hints, form status — `createPortal` roots overlays outside the subtree, `flushSync` forces a synchronous commit, the resource hints are one parameterized preload space, and `useFormStatus`/`requestFormReset` are the form-action DOM seam.

Resource hints are one preload space discriminated by `as`; `flushSync` forces the commit `FocusScope` restore and View-Transition capture depend on.

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY]   | [CONSUMER]                 |
| :-----: | :---------------------------------------------------------------------------------- | :--------------- | :------------------------- |
|  [01]   | `createPortal(children, container: Element \| DocumentFragment, key?): ReactPortal` | portal           | `view/primitive` overlays  |
|  [02]   | `flushSync<R>(fn: () => R): R`                                                      | sync commit      | `act/transition` commit    |
|  [03]   | `preload` / `preinit` / `preloadModule` / `preinitModule`                           | resource hint    | `view`/app preload by `as` |
|  [04]   | `prefetchDNS` / `preconnect`                                                        | origin hint      | `view`/app origin hints    |
|  [05]   | `useFormStatus(): FormStatus` / `requestFormReset(form: HTMLFormElement)`           | form action      | `view/compose` form        |
|  [06]   | `unstable_batchedUpdates<A, R>(cb, a)` / `version`                                  | retired/identity | interop; auto-batches      |

[ENTRYPOINT_SCOPE]: the server + static render, one space per runtime target — `renderTo*` streams a live shell and `prerender*` produces static/resumable output, the subpath (`./server.node` vs `./server.edge`) picking the runtime target while the shape stays uniform.

Every row is consumed at app-ssr (edge for the Web shell); `renderToPipeableStream` pairs the `@effect/platform` `HttpServer` response.

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY] | [CONSUMER]                        |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `renderToPipeableStream(children, options?): PipeableStream`                        | Node SSR       | streaming Node shell              |
|  [02]   | `renderToReadableStream(children, options?): Promise<ReactDOMServerReadableStream>` | Web SSR        | `ReadableStream` edge shell       |
|  [03]   | `renderToString` / `renderToStaticMarkup`                                           | sync SSR       | non-streaming string render       |
|  [04]   | `prerender` / `prerenderToNodeStream` / `resume` / `resumeAndPrerender`             | prerender      | `PostponedState` resume; PPR seam |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Three disjoint planes by subpath: `react-dom/client` mounts (`createRoot`/`hydrateRoot` — app boot), `react-dom` is the in-tree DOM API (`createPortal`/`flushSync`/resource-hints/`useFormStatus` — `view` rows), and `react-dom/server`+`react-dom/static` stream SSR/prerender output (app SSR); a `ui` row touches only the middle plane, the mount and the stream app/`browser`-root concerns because `ui` never imports `browser`.
- `preload`/`preinit`/`preloadModule`/`preinitModule` share ONE preload option shape discriminated by `as` (`PreloadAs`/`PreinitAs`), with `prefetchDNS`/`preconnect` the origin-hint arm; a new asset hint is a call with a different `as`, never a new function family.
- `renderToPipeableStream` (Node) and `renderToReadableStream` (Web) are ONE render space over different stream primitives, `prerender*`/`resume*` the resumable arm, and the subpath picks the runtime target.
- `createRoot`/`hydrateRoot` are the only mounts — `ReactDOM.render`/`hydrate`/`unmountComponentAtNode` are removed; error handling is root-level, `onUncaughtError`/`onCaughtError`/`onRecoverableError` on `RootOptions`/`HydrationOptions` the app-wide net while `react-error-boundary` (sibling `view/primitive`) owns in-tree recovery.
- `useFormState` is deprecated → `useActionState` moved to `react` (`.api/types-react.md`); `react-dom` keeps `useFormStatus` (ambient form pending state) and `requestFormReset`, and auto-batching makes `unstable_batchedUpdates` interop-only.
- `preload`/`preinit` warm assets and React hoists `<title>`/`<meta>`/`<link>` rendered anywhere in the tree, so the viewer's tile/font/model assets are hinted declaratively rather than through a head-manager library.

[STACKING]:
- `@types/react` (`.api/types-react.md`): the peer — shares `ReactNode`/`ReactElement`/`Ref`; `createPortal` returns a `ReactPortal`, `hydrateRoot`/`renderTo*` take `ReactNode`, and `flushSync` forces the commit the React-side `ViewTransition` capture and `FocusScope` restore need; the DOM renderer types over the element types.
- `react-dom` (`.api/react-dom.md`): the paired runtime these types type; import named ESM symbols from the matching subpath (`import { createPortal, flushSync } from "react-dom"`, `import { createRoot } from "react-dom/client"`).
- `react-aria` (`.api/react-aria.md`): overlays root through `createPortal` (via `Overlay`/`UNSAFE_PortalProvider`) to escape the DOM subtree; `flushSync` forces the synchronous commit `FocusScope` focus-restoration depends on. `react-dom` owns the portal + commit, `react-aria` the overlay behavior.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): SSR dehydrates the `Registry` into the HTML `renderTo*` emits and `HydrationBoundary` rehydrates it before children read; `hydrateRoot`'s `formState` and the atom hydration ride the same server-render pass, so a server-computed `AtomHttpApi` result renders without a client refetch.
- `@effect/platform` (`libs/typescript/.api/effect-platform.md`): `renderToPipeableStream`/`renderToReadableStream` feed the `HttpServerResponse` body; the SSR render is an `Effect` producing the stream, and the platform runtime that served the request owns the shell lifecycle and abort `signal`.
- `effect` `Schema` (`libs/typescript/.api/effect.md`): `ReactFormState`/`useFormStatus` carry the server-action result a `Schema` decodes at the boundary; a `FormBinding` folds the `FormStatus.pending`/`data` with the decoded value, so form submission state and wire decode share one owner.
- `view`/`browser`/`edge` (within-lib): a `view` row types against the in-tree DOM API alone, `browser` against the `./client` mount options, and `edge` against the `./server.*`/`./static.*` render options — the subpath split is the stratum boundary.

[LOCAL_ADMISSION]:
- In a `view` row use only the in-tree DOM API — `createPortal` for overlays, `flushSync` where a synchronous commit is load-bearing, the resource hints for assets, `useFormStatus` for form state; leave `createRoot`/`hydrateRoot` and `renderTo*` to the app/`browser` boot root.
- Root overlays through `react-aria`'s `Overlay`/`UNSAFE_PortalProvider` (which owns `createPortal`), not a bare `createPortal` call — the aria layer owns dismiss/focus/scroll-lock around the portal.
- Reach for `flushSync` only where a downstream synchronous read requires it (focus restore, transition capture); default to React's async commit. Never call `unstable_batchedUpdates` — React auto-batches.
- Hint assets with `preload`/`preinit` + the correct `as`; never inject `<link rel=preload>` by hand or through a head-manager — React hoists metadata natively.

[RAIL_LAW]:
- Package: `@types/react-dom`
- Owns: the DOM renderer type surface across three planes — the `react-dom/client` mount (`createRoot`/`hydrateRoot`, `RootOptions`/`HydrationOptions` + the React error triple), the in-tree DOM API (`createPortal`/`flushSync`/`useFormStatus`/`requestFormReset` + the `preload`/`preinit`/`prefetchDNS`/`preconnect` resource-hint space), and the `react-dom/server`+`react-dom/static` render space (`renderToPipeableStream`/`renderToReadableStream`/`renderToString`/`prerender`/`resume`)
- Accept: `createPortal`/`flushSync` in `view` rows (via `react-aria` overlays), the resource hints discriminated by `as`, `useFormStatus` in a `FormBinding`, `createRoot`/`hydrateRoot` at the app boot root, `renderTo*` behind the `@effect/platform` SSR response, the peer `@types/react` element vocabulary
- Reject: `ReactDOM.render`/`hydrate`/`unmountComponentAtNode` (removed), `useFormState` (→ `react` `useActionState`), `unstable_batchedUpdates` for batching (auto-batched), a bare `createPortal` without the `react-aria` overlay layer, hand-injected `<link rel=preload>`/head-manager metadata, `createRoot`/`renderTo*` inside a `ui` `view` row
