# [TS_UI_API_EFFECT_ATOM_ATOM_REACT]

`@effect-atom/atom-react` binds the `@effect-atom/atom` store into the React render tree: it re-exports the whole atom algebra and adds the hooks and providers a `view` row reads and drives the one `ONE_FOLD_ONE_BINDING` store through. Reads are dependency-tracked and slice-scoped, writes carry a `mode`, and `useAtomSuspense` folds a `Result` atom into Suspense and an error boundary; react-compiler owns memoization.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect-atom/atom-react`
- package: `@effect-atom/atom-react` (MIT)
- module: pure-TypeScript, ESM + CJS dual (`.d.ts` under `dist/dts`), `sideEffects: []`; per-namespace subpath exports with the barrel re-export of every `@effect-atom/atom` namespace
- runtime: React client + server; peers `effect`, `react`, `scheduler`, dep `@effect-atom/atom`; SSR rehydrates through `HydrationBoundary`, `scheduler` drives concurrent scheduling, and react-compiler owns memoization
- rail: state binding — the React-facing edge of `ONE_FOLD_ONE_BINDING`; every `view` row reads it, `atom/binding` owns the registry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the barrel re-exports every `@effect-atom/atom` namespace — the algebra every hook consumes, owned in `effect-atom-atom.md`:
[RE_EXPORT]: `Atom` `Registry` `Result` `AtomRef` `AtomHttpApi` `AtomRpc` `Hydration`

[PUBLIC_TYPE_SCOPE]: this package's own React-facing types

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `ScopedAtom<A, Input>`   | interface     | `.use()`/`.Provider`/`.Context` — per-instance subtree atom, seeded `Input` |
|  [02]   | `HydrationBoundaryProps` | interface     | `{ state?: Iterable<DehydratedAtom>, children? }` — SSR rehydrate props     |
|  [03]   | write `mode` union       | union         | `"value" \| "promise" \| "promiseExit"` — the setter discriminant           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: reading atoms — dependency-tracked, slice-scoped

| [INDEX] | [SURFACE]                                                       | [SHAPE]     | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------------- | :---------- | :-------------------------------------------------- |
|  [01]   | `useAtomValue(atom)` / `useAtomValue(atom, (a) => b)`           | read/select | primary read; selector scopes the re-render slice   |
|  [02]   | `useAtomSuspense(atom, { suspendOnWaiting?, includeFailure? })` | suspense    | Suspense fold; `includeFailure` inlines the failure |
|  [03]   | `useAtomMount(atom)`                                            | keep-hot    | pins an atom mounted without reading its value      |
|  [04]   | `useAtomSubscribe(atom, (a) => …, { immediate? })`              | effect      | runs an effect per change, no re-render             |

[ENTRYPOINT_SCOPE]: writing and refreshing — `mode` on `useAtom`/`useAtomSet` selects the setter: `"value"` → `(W | (R=>W)) => void`, `"promise"` → `(W) => Promise<Success>`, `"promiseExit"` → `(W) => Promise<Exit<Success, Failure>>`

| [INDEX] | [SURFACE]                        | [SHAPE]    | [CAPABILITY]                                                 |
| :-----: | :------------------------------- | :--------- | :----------------------------------------------------------- |
|  [01]   | `useAtom(atom, { mode? })`       | read+write | the `[value, write]` tuple; `mode` picks the setter shape    |
|  [02]   | `useAtomSet(atom, { mode? })`    | write      | a setter with no subscription; `mode`-selected shape         |
|  [03]   | `useAtomRefresh(atom)`           | refresh    | a `() => void` re-running an effect/pull atom                |
|  [04]   | `useAtomInitialValues(iterable)` | seed       | seeds atoms at first render from server data or route params |

[ENTRYPOINT_SCOPE]: fine-grained refs — sub-value cursors without re-running the owner

| [INDEX] | [SURFACE]                       | [SHAPE]    | [CAPABILITY]                                                         |
| :-----: | :------------------------------ | :--------- | :------------------------------------------------------------------- |
|  [01]   | `useAtomRef(ref)`               | ref read   | reads an `AtomRef`/`ReadonlyRef` with `Equal`-based change detection |
|  [02]   | `useAtomRefProp(ref, key)`      | ref narrow | derives a child `AtomRef` for one property, a form field             |
|  [03]   | `useAtomRefPropValue(ref, key)` | ref read   | reads one property, re-rendering only on that property               |

[ENTRYPOINT_SCOPE]: providers, scoped atoms, and SSR — `RegistryProvider` accepts `{ children?, initialValues?, scheduleTask?, timeoutResolution?, defaultIdleTTL? }`

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                                                        |
| :-----: | :-------------------------------------------- | :------- | :------------------------------------------------------------------ |
|  [01]   | `RegistryProvider(props)` / `RegistryContext` | provider | the app-root `Registry`; a nested provider isolates a test or story |
|  [02]   | `ScopedAtom.make((input?) => atom)`           | factory  | `.Provider`/`.use()`/`.Context` — per-instance subtree atom         |
|  [03]   | `HydrationBoundary({ state, children })`      | provider | rehydrates the server `dehydrate` output before children read       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every hook resolves the ambient `Registry` from `RegistryContext` and subscribes through `useSyncExternalStore` — the store is React context, the reactivity is the atom graph, and the render is only the projection.
- Async folds, never branches: `useAtomSuspense` routes `waiting` to the nearest `<Suspense>` and `Failure`'s `Cause.squash(cause)` (the squashed tagged `E`) to the error boundary, so a component body carries no `if (loading)`/`if (error)` ladder.
- One hook per axis discriminates its modality on an argument — the write `mode`, the suspense `includeFailure`, the `useAtomValue` selector — never a `useAtomSetAsync` sibling.
- `useAtomRefProp` derives a per-property `AtomRef` cursor so a large form-state atom re-renders only the edited field, the fine-grained counterpart to `useAtomValue`; `ScopedAtom.make` yields `{ Provider, use, Context }` for per-instance subtree state in the same `Registry` lifecycle.

[STACKING]:
- `@effect-atom/atom` (`.api/effect-atom-atom.md`): the dependency and whole model layer; the barrel re-exports it, and `Atom.make`/`Result.match`/`Registry.layer`/`AtomHttpApi.Tag` reach through it — the hooks are the React edge and nothing more.
- `react` + `scheduler` (`.api/react.md`, `.api/types-react.md`): every hook binds through `useSyncExternalStore`, the one external-store seam `react` reserves here; `useAtomSuspense` integrates Suspense, the `"promise"` modes compose with `startTransition`/`use()`, and `RegistryProvider`'s `scheduleTask` routes writes through the `scheduler` peer so updates batch with the render loop.
- `effect` (`libs/typescript/.api/effect.md`): `Result`/`Cause` fold directly in JSX through `Result.match`/`Result.builder`, so the component tree is the terminal fold of the Effect graph with no view-model layer.
- `react-error-boundary` (`.api/react-error-boundary.md`): a failed `useAtomSuspense` throws `Cause.squash(cause)` in render, landing as `FallbackProps.error`; a post-render `Result.Failure` escalates with `showBoundary(cause)`; `resetKeys` on the atom input or `resetErrorBoundary()` re-runs the failed atom through `useAtomRefresh` in `onReset`.
- `react-aria` / `react-stately` (`.api/react-aria.md`, `.api/react-stately.md`): the atom owns the truth — `useAtomValue` feeds a `react-stately` hook's controlled value, and a `react-aria` `onChange`/selection callback calls `useAtomSet`.
- `@tanstack/react-table` + `@tanstack/react-virtual` (`.api/tanstack-react-table.md`, `.api/tanstack-react-virtual.md`): one atom holds the table's sort/filter/selection/pagination fold — each `on*Change` writes it via `useAtomSet` (the `makeStateUpdater` target), `useAtomValue` reads `state` back, and a `GlobalId` selection atom drives the virtualizer's `scrollToIndex({ align: 'center' })`.
- `@effect/platform` (`libs/typescript/.api/effect-platform.md`): SSR dehydrates the `Registry` via `Hydration.dehydrate` into the HTML payload and `HydrationBoundary` rehydrates it, so a server-computed `AtomHttpApi` result renders without a client refetch.
- `view` rows + `system/hook` owner (within-lib): every `view` row reads the store through these hooks and `atom/binding` owns the registry — the React edge of `ONE_FOLD_ONE_BINDING`, with domain state never mirrored in a component.

[LOCAL_ADMISSION]:
- Read with `useAtomValue` and pass a selector for any derived slice — a projection re-renders less than a whole-atom read, and react-compiler owns memoization, so no hook result wraps in `useMemo`/`useCallback`.
- Fold async with `useAtomSuspense` under a `<Suspense>` + error boundary, or `useAtomValue` + `Result.match` for inline states — the `waiting`/`Failure`/`Success` arms replace a manual `isLoading`/`error` pair.
- Choose the write `mode` by handler: `"value"` for UI state, `"promise"`/`"promiseExit"` where a handler awaits completion — the mode signals a finished mutation, read directly rather than polled.
- One `RegistryProvider` at the app root; a nested provider isolates a story, test, or preview. `ScopedAtom` owns per-instance component state, scoping a subtree atom where a global keyed atom leaks.
- `HydrationBoundary` with the server's `dehydrate` output rehydrates SSR data the server already computed, rendering it without a mount refetch.

[RAIL_LAW]:
- Package: `@effect-atom/atom-react`
- Owns: the React hook surface over the atom `Registry` via `useSyncExternalStore` — read, write, suspense, refresh, subscribe, and mount hooks, the `AtomRef` cursor hooks, `RegistryProvider`/`RegistryContext`, `ScopedAtom`, and the `HydrationBoundary` SSR row; re-exports the whole `@effect-atom/atom` algebra
- Accept: `Atom`/`Writable`/`Result` values from `atom/binding`, a selector on reads, a `mode` on writes, one root `RegistryProvider`, `ScopedAtom` for per-instance state, `HydrationBoundary` for SSR
- Reject: manual `isLoading`/`error` state pairs that `useAtomSuspense`/`Result.match` fold, `useMemo`/`useCallback` around hook results under react-compiler, a global keyed atom where `ScopedAtom` scopes to a subtree, a client refetch of server-dehydrated data, a data-fetching library beside the `AtomHttpApi`/`AtomRpc` binding
