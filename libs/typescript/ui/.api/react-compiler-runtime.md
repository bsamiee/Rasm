# [TS_UI_API_REACT_COMPILER_RUNTIME]

`react-compiler-runtime` is the runtime half of the React Compiler pair — `babel-plugin-react-compiler` rewrites every component and hook to allocate a memo cache with `const $ = _c(N)` and thread stable values through `$[i]`, where `_c` is this package's `c`. On React 19 `c` forwards to React's built-in `React.__COMPILER_RUNTIME.c` (`useMemoCache`); below 19 a `useMemo`-backed allocation polyfills it, and opt-in `environment` flags emit dev-mode validators stripped from production.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-compiler-runtime`
- package: `react-compiler-runtime` (MIT)
- module: CJS `dist/index.js` exporting the compiler-emitted `_c`/`$*` symbols; no `types` field, so `dist/index.d.ts` resolves by path
- runtime: React peer; `c` forwards to `React.__COMPILER_RUNTIME.c` (`useMemoCache`) on React 19, else a `React.useMemo` polyfill — the plugin `target` (`17`/`18` vs `19`) selects whether emitted `_c` imports this package or React's built-in `react/compiler-runtime`
- rail: ui/compiler-runtime — the runtime the React Compiler's `_c(N)` output binds to, paired with `babel-plugin-react-compiler`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the memo-cache representation and dev-guard discriminant, internal to the compiler contract — a caller never names them; the emitted `$[i]` reads them and the dev validators dispatch on them.

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `MemoCache = Array<number \| typeof $empty>`       | alias         | the cache-slot array each `c(size)` allocates            |
|  [02]   | `$empty = Symbol.for("react.memo_cache_sentinel")` | sentinel      | an uninitialized slot; DevTools tags it `$[$empty]=true` |
|  [03]   | `GuardKind`                                        | enum          | the `$dispatcherGuard` transition discriminant           |

[GuardKind]: `PushGuardContext` `PopGuardContext` `PushExpectHook` `PopExpectHook`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the emitted memoization allocator, the only surface a production build reaches — one `c(N)` per compiled component/hook body.

| [INDEX] | [SURFACE]                     | [SHAPE] | [CAPABILITY]                                                           |
| :-----: | :---------------------------- | :------ | :--------------------------------------------------------------------- |
|  [01]   | `c(number) -> Array<unknown>` | factory | allocates the N-slot cache the emitted `const $ = _c(N)` head binds    |
|  [02]   | `$reset(MemoCache)`           | static  | resets every slot to `$empty` — the emitted memoization-boundary clear |

[ENTRYPOINT_SCOPE]: development-mode validators, emitted only under the paired plugin's `environment` flags (`.api/babel-plugin-react-compiler.md` `[ENVIRONMENT_EMISSION]`) and absent from production; each returns `void`.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------------------- | :------- | :----------------------------------------------------------- |
|  [01]   | `$dispatcherGuard(GuardKind)`                            | static   | throws on an indirect / renamed / non-component hook call    |
|  [02]   | `$structuralCheck(old, new, name, fn, kind, loc)`        | static   | `console.error`s a value mutated after the compiler froze it |
|  [03]   | `useRenderCounter(string)`                               | static   | counts rerenders into `renderCounterRegistry`                |
|  [04]   | `renderCounterRegistry` / `clearRenderCounterRegistry()` | property | the shared render-count registry and its reset               |
|  [05]   | `$makeReadOnly()`                                        | static   | unimplemented stub reserved for `enableEmitFreeze`           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `babel-plugin-react-compiler` owns memoization: it analyzes each component/hook, decides which values are stable across renders, and emits cache reads/writes against `$ = _c(N)`.
- `c(N)` allocates once per body — an N-slot array seeded with `$empty`, each slot holding the sentinel (recompute + store) or the retained value (reuse). On React 19 the array is React's own `useMemoCache`, reconciler-managed and cleared with the fiber; below 19 the `useMemo` polyfill approximates it.

[STACKING]:
- `babel-plugin-react-compiler`(`.api/babel-plugin-react-compiler.md`): the emitter — its compiled output calls `const $ = _c(N)` this runtime services, and its `environment` `{ source, importSpecifierName }` flags import the `$dispatcherGuard`/`$structuralCheck`/`useRenderCounter`/`$makeReadOnly` validators; `target: "19"` routes `_c` to React's built-in and leaves this package the sub-19 recovery. One config decision spans both packages.
- `react`(`.api/react.md`): the delegate — `c` forwards to `React.__COMPILER_RUNTIME.c` (`useMemoCache`), so the memo cache is a reconciler concept, and compiled memoization composes with `useTransition`/`startTransition`/`<Suspense>` without manual dependency tracking.
- `@vitejs/plugin-react` (bundler Babel pass): runs the compiler pass over every `view`/`act`/`atom` module and injects the `_c` import; source never imports it.
- `effect-atom-atom-react`(`.api/effect-atom-atom-react.md`) / `react-aria`(`.api/react-aria.md`): the compiler stabilizes `useAtomValue` reads, `react-aria` hook bundles, and `mergeProps` outputs automatically — no row wraps a hook result in `useMemo` to stop a child rerender.

[LOCAL_ADMISSION]:
- Memoization is compiled: no `view`/`act`/`atom` row carries a `useMemo`/`useCallback`/`React.memo` or a dependency array — hand-memoization obstructs the compiler's analysis — and every `c`/`$*` reference is compiler-emitted, so a source-level import is the named defect.
- Keep every memoized value immutable; a post-render mutation is what `$structuralCheck` flags in dev and silently breaks reuse in production. Read the dev validators as diagnostics — `console.error` output and `renderCounterRegistry` — never call them.

[RAIL_LAW]:
- Package: `react-compiler-runtime`
- Owns: the `c(N)` memo-cache allocator the React Compiler emits against, the `$reset` invalidator, and the dev-mode `$dispatcherGuard`/`$structuralCheck`/`useRenderCounter` validators
- Accept: compiler-emitted `_c(N)` allocation, React-19 delegation to the built-in `useMemoCache`, the dev validators as erasable diagnostics, `babel-plugin-react-compiler` as the paired emitter
- Reject: hand-written `useMemo`/`useCallback`/`React.memo`/dependency arrays, source-level imports of `c`/`$*`, post-render mutation of memoized values, calling the dev validators as API
