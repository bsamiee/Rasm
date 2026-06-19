# [API_CATALOGUE] react-dom

`react-dom` supplies the browser-side rendering roots, DOM resource preloading hints, form utilities, and SSR primitives consumed by the `ui` stack. The `react-dom/client` subpath owns `createRoot` and `hydrateRoot`; the top-level entry owns `createPortal`, `flushSync`, resource hints, and DOM-specific hooks.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-dom`
- package: `react-dom`
- module: `react-dom` + `react-dom/client` (typed by `@types/react-dom`)
- asset: DOM rendering roots, portals, resource hints, form hooks
- rail: render

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: root and hydration family
- rail: render

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [RAIL]                                        |
| :-----: | :----------------- | :---------------- | :-------------------------------------------- |
|   [1]   | `Root`             | render root       | `{ render(children): void; unmount(): void }` |
|   [2]   | `RootOptions`      | root config       | error handlers + `identifierPrefix`           |
|   [3]   | `HydrationOptions` | hydration config  | extends `RootOptions` + `formState`           |
|   [4]   | `Container`        | mount target      | `Element \| DocumentFragment \| Document`     |
|   [5]   | `ErrorInfo`        | error detail      | `{ componentStack?: string }`                 |
|   [6]   | `ReactFormState`   | form state opaque | SSR form state sigil                          |

[PUBLIC_TYPE_SCOPE]: resource hint family
- rail: render

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [RAIL]                                      |
| :-----: | :------------------ | :------------- | :------------------------------------------ |
|   [1]   | `PreloadOptions`    | preload config | `as`, `crossOrigin`, fetch priority, etc.   |
|   [2]   | `PreconnectOptions` | preconnect cfg | `crossOrigin?`                              |
|   [3]   | `PreloadAs`         | resource type  | audio, font, image, script, style, etc.     |
|   [4]   | `PreloadModuleAs`   | module type    | `RequestDestination`                        |
|   [5]   | `PreinitOptions`    | preinit config | `as`, `crossOrigin`, `precedence`, etc.     |
|   [6]   | `PreinitAs`         | preinit kind   | `"script" \| "style"`                       |
|   [7]   | `FormStatus`        | form state     | `FormStatusNotPending \| FormStatusPending` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: react-dom/client — rendering roots
- rail: render

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :-------------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `createRoot(container, options?)`                   | root factory   | mounts React tree into DOM    |
|   [2]   | `hydrateRoot(container, initialChildren, options?)` | hydration root | attaches to SSR-rendered HTML |

[ENTRYPOINT_SCOPE]: react-dom — portals and sync
- rail: render

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `createPortal(children, container, key?)`     | portal factory | renders children into another DOM node |
|   [2]   | `flushSync<R>(fn)`                            | sync flush     | forces synchronous React state flush   |
|   [3]   | `unstable_batchedUpdates<A, R>(callback, a?)` | batching       | groups multiple updates (legacy)       |

[ENTRYPOINT_SCOPE]: react-dom — resource hints
- rail: render

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------------ | :------------- | :------------------------------------- |
|   [1]   | `prefetchDNS(href)`             | DNS hint       | early DNS prefetch                     |
|   [2]   | `preconnect(href, options?)`    | TCP hint       | early TCP + TLS handshake              |
|   [3]   | `preload(href, options)`        | preload hint   | `<link rel="preload">` injection       |
|   [4]   | `preloadModule(href, options)`  | module preload | `<link rel="modulepreload">` injection |
|   [5]   | `preinit(href, options)`        | preinit hint   | early script / style parse + exec      |
|   [6]   | `preinitModule(href, options?)` | module preinit | early module parse                     |
|   [7]   | `requestFormReset(form)`        | form reset     | programmatic native form reset         |

[ENTRYPOINT_SCOPE]: react-dom — hooks (dom-coupled)
- rail: render

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY] | [RAIL]                                                                     |
| :-----: | :------------------ | :------------- | :------------------------------------------------------------------------- |
|   [1]   | `useFormStatus()`   | form status    | `FormStatus` — pending + data + method                                     |
|   [2]   | `useFormState(...)` | form state     | `[state, dispatch, isPending]` (deprecated in 19; prefer `useActionState`) |

## [4]-[IMPLEMENTATION_LAW]

[RENDER_TOPOLOGY]:
- `react-dom/client` is the browser render entrypoint; import it at the composition root only
- `createRoot` replaces legacy `ReactDOM.render`; `hydrateRoot` replaces `ReactDOM.hydrate`
- `flushSync` forces synchronous state updates — use sparingly; it blocks painting
- Resource hints (`prefetchDNS`, `preconnect`, `preload`, `preinit`) inject `<link>` into `<head>` and are idempotent
- `createPortal` renders children outside the parent DOM hierarchy but keeps them in the React tree for event bubbling
- `unstable_batchedUpdates` is legacy; React 18+ batches by default inside transitions and async handlers

[LOCAL_ADMISSION]:
- Mount once at app entry with `createRoot`; keep the `Root` reference for cleanup via `root.unmount()`.
- Use `hydrateRoot` only when SSR-rendered HTML is already in the document.
- `useFormStatus` reads state from the nearest `<form>` ancestor's action; use only in components inside a form.

[RAIL_LAW]:
- Package: `react-dom` (types: `@types/react-dom`)
- Owns: browser rendering roots, DOM portals, flush control, resource hints, form DOM hooks
- Accept: `createRoot`, `hydrateRoot`, `createPortal`, resource hints, `flushSync`
- Reject: `ReactDOM.render` (removed in 19), `ReactDOM.hydrate` (removed in 19)
