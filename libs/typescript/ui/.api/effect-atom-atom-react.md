# [@effect-atom/atom-react] — React hooks binding the atom algebra into the component spine

`@effect-atom/atom-react` is the React binding for `@effect-atom/atom`: it re-exports the entire atom algebra (`Atom`, `Result`, `Registry`, `AtomRef`, `AtomHttpApi`, `AtomRpc`, `Hydration`) and adds only the hooks and providers that thread atoms through the React 19 render tree — the concrete surface every `view` row consumes to read and drive the one `ONE_FOLD_ONE_BINDING` store. Reads are dependency-tracked and slice-scoped (`useAtomValue(atom, selector)` re-renders only on the selected projection); writes carry a `"value" | "promise" | "promiseExit"` mode so a mutation is either fire-and-forget or awaitable inside an async event handler / transition; `useAtomSuspense` folds the async `Result` into React Suspense so `waiting` suspends and `Failure` throws `Cause.squash(cause)` (the squashed tagged `E`) to the nearest error boundary. `RegistryProvider` supplies the store, `ScopedAtom` gives a component-subtree-local atom, and `HydrationBoundary` rehydrates a server-dehydrated registry before children read — the SSR handoff. react-compiler memoizes the callers, so no hook result is manually wrapped.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect-atom/atom-react`
- package: `@effect-atom/atom-react` (0.5.0, MIT)
- module format: ESM + CJS dual (`.d.ts` under `dist/dts`), `sideEffects: []`; subpaths `.`, `./Hooks`, `./RegistryContext`, `./ScopedAtom`, `./ReactHydration`; the barrel re-exports every `@effect-atom/atom` namespace so a `view` row imports `Atom`/`Result`/`Registry` from here
- runtime target: React 19 client + server (SSR via `HydrationBoundary`); the `scheduler` peer drives concurrent-mode task scheduling, react-compiler compiles memoization
- peer: `effect@^3.19`, `react@>=18 <20`, `scheduler@*`; dep `@effect-atom/atom@^0.5.0`
- asset: pure-TypeScript React hook library — hooks + `RegistryProvider`/`ScopedAtom`/`HydrationBoundary` over the atom `Registry`
- rail: state binding (the React-facing edge of `ONE_FOLD_ONE_BINDING`; every `view` row reads it, `atom/binding` provides the registry)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: re-exported atom algebra — the input to every hook
- rail: state binding

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]    | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------ | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `Atom` / `Registry` / `Result` namespaces (re-export)  | algebra re-export | `view` rows import `Atom.make`/`Result.match`/`Registry.layer` from this barrel; `Result<A,E> = Initial \| Success \| Failure` (the `Failure` arm carries `Cause<E>`) is the value every read hook surfaces — the data source and the failure source; catalogued in `effect-atom-atom.md` |
|  [02]   | `AtomRef` / `AtomHttpApi` / `AtomRpc` / `Hydration` (re-export) | algebra re-export | the binding rows and fine-grained refs reach through the same barrel |
|  [03]   | `ScopedAtom<A, Input>` (`.use()`, `.Provider`, `.Context`) | subtree-scoped atom | `view/compose` — a per-instance atom (per-row, per-panel state) provided down a subtree, optionally seeded with `Input` |
|  [04]   | `HydrationBoundaryProps` (`{ state?, children? }`)      | SSR props         | app root — the `DehydratedAtom[]` a server emits, rehydrated before children read |
|  [05]   | write `mode` union `"value" \| "promise" \| "promiseExit"` | write modality  | `view` — selects whether a setter is fire-and-forget, awaitable to `Success`, or awaitable to `Exit` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: reading atoms — dependency-tracked, slice-scoped
- rail: state binding

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `useAtomValue(atom)` / `useAtomValue(atom, (a) => b)`                                            | read / select  | `view` — the primary read; the selector overload re-renders only when the projected slice changes |
|  [02]   | `useAtomSuspense(atom, { suspendOnWaiting?, includeFailure? })`                                  | suspense read  | `view` — returns `Result.Success`; suspends while `waiting`, throws `Cause.squash(cause)` (the squashed `E`) to the error boundary unless `includeFailure` returns the `Failure` arm inline |
|  [03]   | `useAtomMount(atom)`                                                                             | keep-mounted   | `view` — pin an atom hot for the component's lifetime without reading its value |
|  [04]   | `useAtomSubscribe(atom, (a) => …, { immediate? })`                                               | side-effect    | `view` — run an imperative effect on each atom change (focus, scroll, analytics) without re-render |

[ENTRYPOINT_SCOPE]: writing and refreshing atoms — the three write modes
- rail: state binding

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `useAtom(atom, { mode? })`                                                                       | read + write   | `view` — the `[value, write]` tuple; `mode` selects the setter shape (below) |
|  [02]   | `useAtomSet(atom, { mode? })`                                                                    | write only     | `view` — a setter with no subscription; `"value"` → `(W \| (R=>W)) => void`, `"promise"` → `(W) => Promise<Success>`, `"promiseExit"` → `(W) => Promise<Exit>` for awaitable async handlers |
|  [03]   | `useAtomRefresh(atom)`                                                                           | refresh        | `view` — returns a `() => void` that re-runs an effect/pull atom (pull-to-refresh, retry) |
|  [04]   | `useAtomInitialValues(iterable)`                                                                 | seed           | `view` — seed atoms with initial values at first render (server-provided data, route params) |

[ENTRYPOINT_SCOPE]: fine-grained refs — sub-value cursors without re-running the owner
- rail: state binding

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `useAtomRef(ref)`                                                                                | ref read       | `view/compose` — read an `AtomRef`/`ReadonlyRef` value with `Equal`-based change detection |
|  [02]   | `useAtomRefProp(ref, key)`                                                                       | ref narrow     | `view/compose` — derive a child `AtomRef` for one property (a single form field of a form-state ref) |
|  [03]   | `useAtomRefPropValue(ref, key)`                                                                  | ref prop read  | `view/compose` — read one property value directly, re-rendering only when that property changes |

[ENTRYPOINT_SCOPE]: providers, scoped atoms, and SSR
- rail: composition

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `RegistryProvider({ children, initialValues?, scheduleTask?, timeoutResolution?, defaultIdleTTL? })` / `RegistryContext` | store provider | app root — supplies the one `Registry`; a nested provider is a test/story isolation boundary |
|  [02]   | `ScopedAtom.make((input?) => atom)` → `.Provider` / `.use()` / `.Context`                        | subtree atom   | `view/compose` — a per-instance atom scoped to a subtree (per-row expansion, per-panel draft), optionally seeded with `Input` |
|  [03]   | `HydrationBoundary({ state, children })`                                                         | SSR handoff    | app root — rehydrate `Hydration.dehydrate(registry)` output into the client registry before children read |

## [04]-[IMPLEMENTATION_LAW]

[HOOK_TOPOLOGY]:
- Every hook resolves the ambient `Registry` from `RegistryContext` and subscribes through React's `useSyncExternalStore` — the one external-store seam `react` (`.api/react.md`) and `@types/react` (`.api/types-react.md`) reserve for this package; `useAtom`/`useAtomValue`/`useAtomSet` are built on it, so `react` reaches the atom binding through `useSyncExternalStore` and nowhere else. The store is React context, the reactivity is the atom graph, and React's render is only the projection. A component reads with `useAtomValue`, and the selector overload (`useAtomValue(atom, f)`) scopes the subscription to the projected slice so an unrelated field change does not re-render; this replaces a `useMemo`-over-selector idiom entirely, and react-compiler handles the residual memoization.
- Async is folded, not branched: `useAtomSuspense` turns a `Result` atom into a Suspense boundary — `waiting` suspends to the nearest `<Suspense>` fallback, `Failure` throws `Cause.squash(cause)` (the squashed tagged `E`, so the boundary fallback `Match.tags` it directly) to the nearest error boundary, and `Success` is the returned value, so a component body never carries `if (loading)`/`if (error)` ladders. When inline handling is wanted, `includeFailure: true` returns the `Failure` arm and the component folds it with `Result.match`.
- Writes carry a modality: `useAtom`/`useAtomSet` accept `mode` — `"value"` for fire-and-forget UI writes, `"promise"`/`"promiseExit"` for handlers that must `await` the effect's completion (form submit that awaits the server `Result`, an optimistic write reconciled in a `startTransition`). One hook, three write shapes, discriminated by the mode value — no `useAtomSetAsync` sibling.
- `AtomRef` hooks give fine-grained cursors: `useAtomRefProp(ref, key)` derives a child `AtomRef` for one property so a large form-state atom re-renders only the edited field, and an undo/redo cursor (an `AtomRef.collection`) subscribes per item — the fine-grained counterpart to the coarse `useAtomValue`.
- `ScopedAtom` is per-instance state done right: `ScopedAtom.make(f)` yields a `{ Provider, use, Context }` triple so a component subtree owns its own atom instance (one draft per open editor) without a global key registry, and the atom still participates in the same `Registry` lifecycle and devtools.

[STACKS_WITH]:
- `@effect-atom/atom` (`libs/typescript/ui/.api/effect-atom-atom.md`): the dependency and the entire model layer — this package adds only hooks. `Atom.make`/`Result.match`/`Registry.layer`/`AtomHttpApi.Tag` come through this barrel; the hooks are the React edge of that algebra and nothing more.
- `react` (peer `>=18 <20`) + `scheduler` (peer): every hook binds through React's `useSyncExternalStore` — the single external-store seam `react` (`.api/react.md`) / `@types/react` (`.api/types-react.md`) reserve for this package; `useAtomSuspense` integrates React Suspense, the `"promise"` write modes compose with `startTransition`/`use()`, and the `scheduler` peer drives concurrent scheduling of atom notifications — `RegistryProvider`'s `scheduleTask` routes writes through React's scheduler so updates batch with the render loop.
- `effect` (peer, `libs/typescript/.api/effect.md`): `Result` and `Cause` are folded directly in JSX (`Result.match`/`Result.builder`), and the atoms hooks read are Effect-backed — the component tree is the terminal fold of the Effect graph, with no separate view-model layer.
- `react-error-boundary` (sibling folder row, `view/primitive`, `.api/react-error-boundary.md`): the atom is the failure SOURCE — a failed `useAtomSuspense` read throws `Cause.squash(cause)` (the squashed tagged `E`) in render, landing in the folder's error-boundary row as `FallbackProps.error`, while an async `Result.Failure` that resolves after render is escalated with `showBoundary(cause)`; the async-failure rail is Suspense (`waiting`) plus that boundary (`Failure`), never a per-component try/catch. The reset closes the loop back: `resetKeys` bound to the atom input (or `resetErrorBoundary()`) re-runs the failed atom `Effect` through `useAtomRefresh` in the boundary's `onReset`.
- react-aria / react-stately (sibling rows `view/primitive`, `view/compose`, `.api/react-aria.md`, `.api/react-stately.md`): the atom is the store and the DATA SOURCE — `react-stately` derives its collection/selection state from atom values, and `react-aria` hooks turn that state into ARIA props — `useAtomValue` feeds the stately hook's controlled value (options, selected keys, collection rows) and a react-aria `onChange`/selection callback calls the atom setter (`useAtomSet`). The atom owns the truth; react-aria owns the DOM contract.
- `@tanstack/react-table` + `@tanstack/react-virtual` (`.api/tanstack-react-table.md`, `.api/tanstack-react-virtual.md`, `view/compose`): the `ONE_FOLD_ONE_BINDING` law binds a headless collection grid — the table's sorting/filter/selection/pagination fold is one atom, each `on*Change` writes it via `useAtomSet` (the setter a table `makeStateUpdater` targets) and `useAtomValue` reads `state` back; a `GlobalId` selection atom read through `useAtomValue` drives the virtualizer's `scrollToIndex(align:'center')` so table selection and viewport stay in sync through the one fold.
- `@effect/platform` server (`libs/typescript/.api/effect-platform.md`): server-side rendering dehydrates the `Registry` (`Hydration.dehydrate`) into the HTML payload and `HydrationBoundary` rehydrates it client-side, so a server-computed `AtomHttpApi` result renders without a client refetch — the SSR seam rides the same platform runtime that served the request.

[LOCAL_ADMISSION]:
- Read with `useAtomValue` and pass a selector for any derived slice; never read the whole atom and destructure in the body when a projection re-renders less. Never wrap a hook result in `useMemo`/`useCallback` — react-compiler owns memoization here.
- Fold async with `useAtomSuspense` under a `<Suspense>` + error boundary, or `useAtomValue` + `Result.match` for inline states; never thread a manual `isLoading`/`error` boolean pair through props.
- Choose the write `mode` deliberately: `"value"` for UI state, `"promise"`/`"promiseExit"` only where a handler awaits completion; never poll an atom to detect a mutation finishing.
- Provide one `RegistryProvider` at the app root; use a nested provider only to isolate a story/test/preview. Use `ScopedAtom` for per-instance component state, never a global atom keyed by a component id.
- Rehydrate SSR through `HydrationBoundary` with the server's `dehydrate` output; never refetch on mount data the server already computed.

[RAIL_LAW]:
- Package: `@effect-atom/atom-react`
- Owns: the React hook surface over the atom `Registry`, built on React's `useSyncExternalStore` (`useAtom`/`useAtomValue`/`useAtomSet`/`useAtomSuspense`/`useAtomRefresh`/`useAtomSubscribe`/`useAtomMount`/`useAtomInitialValues`), the `AtomRef` cursor hooks, `RegistryProvider`/`RegistryContext`, `ScopedAtom`, and the `HydrationBoundary` SSR row; re-exports the whole `@effect-atom/atom` algebra
- Accept: `Atom`/`Writable`/`Result` values from `atom/binding`, a selector on reads, a `mode` on writes, one root `RegistryProvider`, `ScopedAtom` for per-instance state, `HydrationBoundary` for SSR
- Reject: manual `isLoading`/`error` state pairs where `useAtomSuspense`/`Result.match` fold it, `useMemo`/`useCallback` around hook results under react-compiler, global keyed atoms where `ScopedAtom` scopes to a subtree, client refetch of server-dehydrated data, a data-fetching library beside the `AtomHttpApi`/`AtomRpc` binding
