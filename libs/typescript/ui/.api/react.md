# [react] — the React 19 runtime spine every view/act/atom row renders through: one hook surface where the compiler owns memoization, Suspense+transitions own async, and `useSyncExternalStore` is the single seam the `@effect-atom` binding threads state through

`react` is the component runtime the whole `ui` folder renders against — function components only, `react-compiler` enabled so memoization is compiled from the dataflow, never hand-written. The public surface is one hook family plus a thin set of top-level element/context/concurrent APIs: hooks are the capability rows (`useState`/`useReducer` for ephemeral local state, `useSyncExternalStore` for the external-store bind, `use`/`Suspense`/`useTransition`/`useDeferredValue` for the concurrent-async surface, `useEffect`/`useLayoutEffect`/`useEffectEvent` for the effect seam), and a component runs by composing them — there is no class hierarchy in play (`Component`/`PureComponent`/`forwardRef`/`createRef` are legacy the folder never authors). The React 19 line collapses old ceremony: `ref` is an ordinary prop (no `forwardRef`), `<Context>` is its own provider (no `.Provider`), and the compiler emits the `useMemo`/`useCallback`/`memo` the folder used to write by hand. State discipline is layered — `react` owns ephemeral *interaction-local* state, the `@effect-atom` binding (`.api/effect-atom-atom-react.md`) owns domain/app state and reaches React only through `useSyncExternalStore`. The runtime module ships JS only; its type surface resolves from `@types/react` (`.api/types-react.md`), and the DOM renderer that commits the tree is `react-dom` (`.api/react-dom.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react`
- package: `react`
- version: `19.2.7`
- license: `MIT` (© Meta Platforms)
- module format: ESM/CJS dual via conditional `exports`; subpaths `.` (barrel), `./jsx-runtime`, `./jsx-dev-runtime`, `./compiler-runtime`; a `react-server` condition swaps the RSC-safe build (no client hooks) at the `.` and jsx subpaths
- runtime target: isomorphic (browser/node/worker); zero dependencies; renderer-agnostic — `react` is the reconciler-facing runtime, a renderer package (`react-dom`) commits the tree
- asset: JS runtime shipping no own `.d.ts`; the type-level surface is `@types/react` (`.api/types-react.md`) and the compiler gate is `tsc`/`tsgo`, so a member here and its type there move as one wave
- compiler: `react-compiler` compiles memoization from the dataflow (`.api/babel-plugin-react-compiler.md` build pass, `.api/react-compiler-runtime.md` runtime companion); `__COMPILER_RUNTIME` is the internal handle the emitted code binds
- rail: the `ui` React spine — every `view`/`act`/`atom`/`intl` row is a function component or hook composed on this runtime
- not-Effect: `react` is imperative/promise-native; the `Effect` rail reaches it only through the `@effect-atom` binding, and the one imperative edge (`Effect.runFork`) lives at app boot in `browser`, never inside a component

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the runtime-value type shapes the hooks return and consume
- rail: shapes
- The full element/props/ref/child vocabulary (`ReactNode`, `ReactElement`, `ComponentProps`, `FC`, `PropsWithChildren`, `Ref`/`RefObject`, `Context<T>`) is declared in `@types/react` and catalogued at `.api/types-react.md`; the rows below are the React-19 runtime-behavior types a hook signature carries, so a row here composes the entrypoint tables without re-teaching the element vocabulary.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `Usable<T>` (`Promise<T>` \| `Context<T>`)          | suspendable read   | `use(usable)` argument — the value `Suspense` unwraps; the Effect→promise boundary in `view` feeds it |
|  [02]   | `Dispatch<A>` / `SetStateAction<S>` / `ActionDispatch<A>` | state dispatcher | the setter half of `useState`/`useReducer`/`useActionState`; ephemeral-state mutation, not domain mutation |
|  [03]   | `TransitionStartFunction`                           | transition gate    | the `startTransition` returned by `useTransition`; wraps a state update as non-urgent for `view` route/filter churn |
|  [04]   | `RefObject<T>` / `Ref<T>` / `RefCallback<T>`        | ref cell           | `useRef`/`useImperativeHandle` results; in React 19 a `Ref` is passed as an ordinary prop, so `forwardRef` is never authored |
|  [05]   | `Context<T>` / `Provider<T>`                        | context handle      | `createContext` result; rendered directly as `<Ctx value>` (the value provider) — `.Provider` is the legacy spelling |
|  [06]   | `ActivityProps` (`mode?: "visible" \| "hidden"`)    | activity mode       | `<Activity>` subtree pre-render/hide with preserved state — the `act/transition` gated upgrade row |
|  [07]   | `ReactNode` / `ReactElement` / `JSX.Element`        | render tree (from `@types/react`) | every component return; the headless `view` rows and `intl` message folds produce `ReactNode` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: local state, reducers, and the React-19 action surface
- rail: surfaces-and-dispatch
- `react` owns *ephemeral interaction-local* state — the value a widget needs to function this frame. Domain/app state is the `@effect-atom` binding (`ONE_FOLD_ONE_BINDING`), so `useState`/`useReducer` never mirror a domain fold, and the action hooks (`useActionState`/`useOptimistic`) are the pure-React form boundary the folder mostly supersedes with `Schema`→aria `FormBinding` + the atom.

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `useState(initial)` / `useReducer(reducer, initial, init?)`                             | local state    | ephemeral view state (an input's uncommitted text, a hovered index); never a domain fold — that is `atom` |
|  [02]   | `useOptimistic(state, updateFn)` / `useActionState(action, initial, permalink?)`       | action state   | React-native optimistic/pending form state; the folder routes mutation through `atom` + `FormBinding`, so this is a bare-React boundary only |
|  [03]   | `useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot?)`                     | external bind  | THE seam a `SubscriptionRef`/store binds through — `@effect-atom/atom-react` is built on it; `atom` reaches React here, nowhere else |
|  [04]   | `useContext(Context)` / `createContext(default)`                                        | context read   | ambient capability (`I18nProvider` locale in `intl/format`, a theme handle in `token/theme`); `<Context value>` provides |

[ENTRYPOINT_SCOPE]: the concurrent-async surface — `use`, Suspense, transitions, deferral
- rail: surfaces-and-dispatch
- The React 19 async surface: `use` unwraps a promise/context under `Suspense`, `useTransition`/`useDeferredValue` keep interaction responsive during heavy `view` churn, and `lazy` code-splits the `scope:viewer` heavy tier so non-spatial apps never pay for it.

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `use(usable)` — `Promise<T>` \| `Context<T>`                                            | suspend read   | reads an atom-suspended value or a context conditionally; a resolved `Effect` promise crosses here at a `view` boundary |
|  [02]   | `Suspense` (`fallback`) / `lazy(() => import(...))`                                     | boundary       | `viewer/scene/glb` and the geo tier are `lazy` behind `Suspense` — the `scope:viewer` split; skeletons are the fallback |
|  [03]   | `useTransition()` → `[isPending, startTransition]` / `startTransition(scope)`          | transition     | non-urgent updates for `view/compose` table/filter/route churn; `isPending` drives a pending affordance |
|  [04]   | `useDeferredValue(value, initial?)`                                                     | deferral       | a lagging copy of a fast-changing value (search box → virtualized `view` list) so typing stays responsive |

[ENTRYPOINT_SCOPE]: effects, refs, and identity
- rail: surfaces-and-dispatch
- The effect seam and imperative escape hatches. `react-compiler` owns memoization, so `useMemo`/`useCallback`/`memo` are compiler-emitted, not hand-authored; `useEffectEvent` extracts the non-reactive part of an effect so the dependency set stays honest.

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `useEffect(fn, deps?)` / `useLayoutEffect(fn, deps?)` / `useInsertionEffect(fn, deps?)`| effect seam    | subscription lifetime, DOM measurement (`floating-ui` anchor read), style insertion; cleanup is the returned fn |
|  [02]   | `useEffectEvent(fn)`                                                                    | effect event   | the non-reactive slice of an effect (latest handler without re-subscribing); keeps `useEffect` deps minimal under the compiler |
|  [03]   | `useRef(initial)` / `useImperativeHandle(ref, create, deps?)`                           | ref cell       | mutable handles (a virtualizer scroll node, an imperative `view` focus method); `ref` arrives as a prop in React 19 |
|  [04]   | `useMemo(fn, deps)` / `useCallback(fn, deps)` / `memo(Component, areEqual?)`            | memo (compiled)| compiler-emitted from the dataflow — authored by hand only at a proven FFI/identity boundary the compiler cannot see |
|  [05]   | `useId()` / `useDebugValue(value, format?)`                                             | identity/debug | SSR-stable ids for a11y wiring; `useDebugValue` labels a custom hook in devtools |

[ENTRYPOINT_SCOPE]: elements, roots-of-tree components, and gated concurrency
- rail: surfaces-and-dispatch
- Element construction is the JSX runtime's job (`./jsx-runtime`); the imperative `createElement`/`cloneElement`/`Children` forms are escape hatches. `Activity` is stable in 19.2 (the pre-render/hide row); `ViewTransition` is NOT yet in the stable runtime, so `act/transition` owns the native View Transitions API and keeps `<ViewTransition>` a gated future.

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `Fragment` / `StrictMode` / `Profiler(id, onRender)`                                    | tree wrapper   | grouping without a node, dev double-invoke, render-cost measurement around a `view` subtree |
|  [02]   | `Activity` (`mode`) — `<Activity mode="hidden">`                                        | pre-render/hide| the `act/transition` gated upgrade — hide a subtree keeping state + defer its effects, or pre-render a hidden route |
|  [03]   | `Suspense` list boundaries + `use` (above) compose the streaming/reveal order          | reveal order   | `view/compose` staged reveal; ordering is Suspense-boundary placement, not an API knob |
|  [04]   | `createElement` / `cloneElement` / `isValidElement` / `Children.{map,toArray,only}`     | element (escape)| slot/`asChild` internals (`@radix-ui/react-slot`), prop-injecting a passed element; JSX is the authored path |
|  [05]   | `createContext` / `forwardRef` / `createRef` / `Component` / `PureComponent`            | legacy         | present but never authored — ref-as-prop, `<Context>` provider, and function components supersede the class/forwardRef forms |
|  [06]   | `cache(fn)` / `cacheSignal()`                                                            | server memo    | RSC-only request-scoped memoization; inert in the client SPA build — a boundary row, not a client capability |
|  [07]   | `act(scope)` / `captureOwnerStack()` / `unstable_useCacheRefresh()`                     | test/dev       | kit-driven spec flushing (`@effect/vitest`), dev owner-stack capture, cache invalidation — not shipped-component surface |

## [04]-[IMPLEMENTATION_LAW]

[REACT_TOPOLOGY]:
- function components + hooks are the whole authoring surface: a component is a function returning `ReactNode`, capability is added by composing hooks, and there is no class hierarchy — `Component`/`PureComponent`/`forwardRef`/`createRef` exist for interop but the folder authors none. React 19 removed the ceremony they carried: `ref` is an ordinary prop, `<Context>` is its own provider, and `use(promise|context)` reads conditionally under `Suspense`.
- memoization is compiled, not written: `react-compiler` derives the memo boundaries from the dataflow and emits them against `react-compiler-runtime`. `useMemo`/`useCallback`/`memo` are therefore compiler-owned; a hand-authored one is a smell unless it guards a proven identity/FFI boundary the compiler provably cannot see. `useEffect` dependency sets stay honest because `useEffectEvent` extracts the non-reactive slice rather than lying with an under-specified array.
- state is layered by lifetime: `react` owns *ephemeral interaction-local* state (`useState`/`useReducer` — the uncommitted, this-frame value), and domain/app state is the `@effect-atom` fold. The only seam between them is `useSyncExternalStore`, on which `@effect-atom/atom-react` is built — an atom's value reaches a component through that subscription and nowhere else, so a component never holds a second copy of domain state.
- async is Suspense + transitions, not manual flags: `use`/`Suspense`/`lazy` express loading structurally (fallbacks, code-split boundaries), and `useTransition`/`useDeferredValue` keep interaction responsive while a heavy `view` update settles. The `scope:viewer` heavy tier is `lazy`-loaded behind a `Suspense` boundary so the non-spatial majority never downloads three/deck.gl.
- concurrency rows are gated by runtime availability: `Activity` (pre-render/hide with preserved state) is stable in 19.2 and is the real `act/transition` upgrade row; `ViewTransition` is not yet in the stable runtime, so the native browser View Transitions API is the current owner and `<ViewTransition>` stays a documented-but-inert future. `cache`/`cacheSignal` are RSC-only and inert in the client build.

[INTEGRATION_LAW]:
- Stack on `@effect-atom` (`.api/effect-atom-atom-react.md`, `.api/effect-atom-atom.md`): the atom binding is built on `useSyncExternalStore` — `useAtomValue`/`useAtom` subscribe a component to an atom's `SubscriptionRef`, Suspense-integrate an async atom (feeding `use`/`Suspense`), and surface the write half as a dispatcher. This is the ONE state binding; `useState` never duplicates a value an atom owns, and the `Effect` rail (retry/span/decode) lives entirely behind the atom, never inside a component body.
- Stack across the headless spine (`.api/react-aria-components.md`, `.api/react-aria.md`, `.api/react-stately.md`): the react-aria component/behavior/state hooks are React hooks — they run on this runtime, consume `useId` for a11y id stability, and return props a `view` component spreads. A `view` primitive is a react-aria hook composed with `token`/`tailwind` classes; the compiler memoizes the composition.
- Stack with `react-dom` (`.api/react-dom.md`): `react` describes the tree; `react-dom` commits it and owns `createRoot`/`flushSync`/`createPortal` and the form-status/DOM hooks. The one `createRoot(...).render` edge lives at app boot in `browser`, and `Suspense` fallbacks are what the renderer paints while `use` suspends.
- Stack with `react-error-boundary` (`.api/react-error-boundary.md`): a thrown render error (including an Effect fault surfaced through the atom) is caught by an error boundary that pairs with the `Suspense` fallback — the two together give every `view` subtree a typed loading + failure envelope.
- Stack with the compiler pair (`.api/babel-plugin-react-compiler.md`, `.api/react-compiler-runtime.md`): the build pass rewrites components to memoized form against the runtime companion; correct hook usage (rules-of-hooks, no conditional hooks) is the contract the pass depends on, so the folder writes hooks straight and lets the pass optimize.

[LOCAL_ADMISSION]:
- Author function components + hooks only; never a class component, `forwardRef`, or `createRef` — pass `ref` as a prop and compose hooks for behavior.
- Route domain/app state through `@effect-atom`; use `useState`/`useReducer` only for ephemeral interaction-local state, and bind any external store through `useSyncExternalStore`, never a hand-rolled subscribe-in-`useEffect`+`useState` pair.
- Let `react-compiler` emit memoization; never hand-write `useMemo`/`useCallback`/`memo` except at a proven identity/FFI boundary the compiler cannot see, and keep `useEffect` deps honest with `useEffectEvent` instead of an under-specified array.
- Express async with `use`/`Suspense`/`lazy` + `useTransition`/`useDeferredValue`; never a manual `isLoading` boolean ladder, and never run an `Effect` inside a component body — the atom binding owns the rail and the one `runFork` edge is at boot.
- Treat `Activity` as the stable pre-render/hide row and `<ViewTransition>` as gated (native View Transitions API owns transitions today); never ship `cache`/`cacheSignal` as client capability.

[RAIL_LAW]:
- Package: `react`
- Owns: the React 19 function-component runtime — the hook surface (`useState`/`useReducer`/`useContext`/`useRef`/`useEffect`/`useLayoutEffect`/`useInsertionEffect`/`useEffectEvent`/`useId`/`useDebugValue`/`useImperativeHandle`), the external-store bind (`useSyncExternalStore`), the concurrent-async surface (`use`/`Suspense`/`lazy`/`useTransition`/`useDeferredValue`/`startTransition`), the action hooks (`useActionState`/`useOptimistic`), context (`createContext`/`useContext` + `<Context>` provider), the element/tree APIs (`createElement`/`cloneElement`/`Children`/`Fragment`/`StrictMode`/`Profiler`), the gated `Activity`, the RSC `cache`/`cacheSignal`, and the test/compiler handles (`act`/`__COMPILER_RUNTIME`)
- Accept: function components, compiler-emitted memoization, `atom`-owned domain state bound via `useSyncExternalStore`, Suspense/transition async, ref-as-prop, `<Context>` provider, `lazy` code-split of the `scope:viewer` tier
- Reject: class components / `forwardRef` / `createRef` authoring, hand-written `useMemo`/`useCallback` outside a proven boundary, `useState` mirroring a domain fold, manual subscribe-in-effect stores, `isLoading` flag ladders, an `Effect` run inside a component body, and treating `ViewTransition`/`cache` as available client capability
