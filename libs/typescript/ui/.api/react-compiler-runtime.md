# [TS_UI_API_REACT_COMPILER_RUNTIME]

`react-compiler-runtime` is the runtime the React Compiler's output binds to: `babel-plugin-react-compiler` rewrites every component and hook to allocate a memoization cache with `const $ = _c(N)` and read/write stable values through `$[i]`, and `_c` is this package's `c`. On React 19 `c` delegates to React's built-in `React.__COMPILER_RUNTIME.c` (`useMemoCache`); the userspace `useMemo`-backed allocation is the polyfill for React < 19. It is the runtime half of the two-package compiler pair (`babel-plugin-react-compiler` is the build pass); the folder law is that memoization is compiled, never hand-written, so no `view`/`act`/`atom` row carries a `useMemo`, `useCallback`, `React.memo`, or a dependency array. Beyond the production `c`/`$reset` allocator surface, the package carries the compiler's development-mode validators — `$dispatcherGuard`, `$structuralCheck`, and the render-counter instrumentation — emitted only under the plugin's opt-in guard/instrumentation flags and stripped from production output.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-compiler-runtime`
- package: `react-compiler-runtime`
- license: `MIT`
- react-peer: `react catalog || ^catalog` (delegates to `React.__COMPILER_RUNTIME.c`/`React.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE` when present, else falls back to a `React.useMemo` polyfill)
- asset: source-only package shipping `src/index.ts` (no built `dist` types entry); the consuming toolchain transpiles it, so it participates in the folder `tsc` graph directly
- pairs-with: `babel-plugin-react-compiler` (`.api/babel-plugin-react-compiler.md`) — the compile pass that emits `_c(N)` calls; the plugin `target` selects the import source (`19` → React's built-in `react/compiler-runtime`; `17`/`18` → this package)
- catalog-verdict: KEEP

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the memo-cache representation and dev-guard vocabulary
- rail: compiler (cross-plane)
- Internal to the compiler contract — the caller never names these; they define the cache slot representation the emitted `$[i]` reads and the guard-transition kinds the dev validators dispatch on.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:--------------------------------------------------------- |:------------- |:--------------------------------------------------------------- |
| [01] | `MemoCache = Array<number \| typeof $empty>` | cache slots | the array `c(size)` returns; each slot holds a memoized value or the sentinel |
| [02] | `$empty = Symbol.for("react.memo_cache_sentinel")` | uninitialized | the sentinel marking a never-computed slot; DevTools reads the `$[$empty]=true` tag |
| [03] | `GuardKind` (`PushGuardContext`/`PopGuardContext`/`PushExpectHook`/`PopExpectHook`) | guard transition | the `$dispatcherGuard` dispatch discriminant framing hook-call boundaries |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the emitted memoization allocator (production surface)
- rail: compiler (cross-plane)
- The only surface a production build reaches. The compiler emits one `c(N)` per component/hook body and threads every stabilized value through the returned cache; on React 19 the allocation is React's native `useMemoCache`.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------- |:-------------- |:------------------------------------------------------------------- |
| [01] | `c(size: number): Array<unknown>` | allocate cache | emitted `const $ = _c(N)` at each compiled body head; delegates to `React.__COMPILER_RUNTIME.c` on React 19, else `useMemo` polyfill |
| [02] | `$reset($: MemoCache): void` | invalidate | resets every slot to `$empty` — the emitted cache-clear on a memoization-boundary reset |

[ENTRYPOINT_SCOPE]: development-mode validators (stripped in production)
- rail: compiler (cross-plane)
- Emitted only when the plugin's `environment` sets the matching emission flag (`enableEmitHookGuards` → `$dispatcherGuard`, `enableChangeDetectionForDebugging` → `$structuralCheck`, `enableEmitInstrumentForget` → `useRenderCounter`, `enableEmitFreeze` → `$makeReadOnly`); each flag is an `{ source, importSpecifierName }` config naming this package + export (see `.api/babel-plugin-react-compiler.md` `[ENVIRONMENT_EMISSION]`). They enforce the compiler's stricter rules-of-React at runtime and surface memoization mistakes; production builds never carry these calls.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------------------- |:---------------- |:-------------------------------------------------------------- |
| [01] | `$dispatcherGuard(kind: GuardKind): void` | hook-call guard | dev — swaps in a guard dispatcher that throws on a hook called indirectly, renamed, or from a compiled non-component call; emitted by plugin `environment.enableEmitHookGuards` |
| [02] | `$structuralCheck(old, new, variableName, fnName, kind, loc): void` | memo correctness | dev — deep-diffs a value the compiler assumed stable against the recomputed one and `console.error`s the divergence path; emitted by `environment.enableChangeDetectionForDebugging` |
| [03] | `useRenderCounter(name: string): void` / `renderCounterRegistry` / `clearRenderCounterRegistry()` | rerender probe | dev — counts component rerenders into a shared registry for compiler-effectiveness inspection; emitted by `environment.enableEmitInstrumentForget` |
| [04] | `$makeReadOnly()` | freeze (stub) | unimplemented in runtime (throws) — the reserved deep-freeze hook `environment.enableEmitFreeze` will emit for frozen values |

## [04]-[IMPLEMENTATION_LAW]

[COMPILED_MEMOIZATION_TOPOLOGY]:
- The compiler owns memoization; the source does not. `babel-plugin-react-compiler` analyzes each component/hook, determines which values are stable across renders, and emits a cache read/write against `$ = _c(N)`. Hand-written `useMemo`/`useCallback`/`React.memo` are not just redundant — they obstruct the compiler's analysis and are the named defect in a compiled folder.
- `c(N)` allocates once per body: an `N`-slot array seeded with the `react.memo_cache_sentinel`. A memoized value's slot holds either the sentinel (recompute + store) or the retained value (reuse). On React 19 the array is React's own `useMemoCache`, so the cache is reconciler-managed and cleared on the same schedule as the fiber; on React < 19 the `useMemo(() => Array(N)…, [])` polyfill approximates it.
- React 19 makes `c` a passthrough: `React.__COMPILER_RUNTIME.c` exists, so importing from this package on React 19 still routes to the built-in. The package is admitted as the explicit, pinnable runtime `target` and as the carrier of the dev-validator surface React's built-in runtime does not export.
- The dev validators are opt-in and erased in production: `$dispatcherGuard` enforces the no-conditional / no-indirect / must-be-named hook rules the compiler relies on; `$structuralCheck` catches a value mutated after the compiler froze it; `useRenderCounter` measures rerender reduction. They emit only under the plugin `environment` emission flags (`enableEmitHookGuards` / `enableChangeDetectionForDebugging` / `enableEmitInstrumentForget` / `enableEmitFreeze`), each an `{ source, importSpecifierName }` naming this package export, and never reach a production bundle.

[STACKS_WITH]:
- `babel-plugin-react-compiler` (`.api/babel-plugin-react-compiler.md`): the compile-time counterpart — it emits the `_c(N)` calls this runtime services and, in dev, the `$dispatcherGuard`/`$structuralCheck`/`useRenderCounter` calls gated by its `environment` emission flags. `target: "19"` points the emitted `_c` import at React's built-in `react/compiler-runtime`; a lower target points it here. One config decision spans both packages.
- `react` (`.api/react.md`): the delegate — `c` forwards to `React.__COMPILER_RUNTIME.c` (`useMemoCache`) on 19; the memo cache is a first-class reconciler concept, not a userspace `Map`. Compiled memoization composes with `useTransition`/`startTransition` and `<Suspense>` without manual dependency tracking.
- `@vitejs/plugin-react` / `@nx/react` (workspace tooling): the build integration that runs the babel compiler pass over every `view`/`act`/`atom`/`intl` module; the runtime import is injected there, never written by hand.
- `react-aria` / `@effect-atom/atom-react` (`.api/react-aria.md`, `.api/effect-atom-atom-react.md`): downstream beneficiaries — because memoization is compiled, `useAtomValue` results, `react-aria` hook bundles, and `mergeProps` outputs are stabilized automatically; no row wraps a hook result in `useMemo` to keep a child from rerendering.

[LOCAL_ADMISSION]:
- Never author `useMemo`, `useCallback`, `React.memo`, or a manual dependency array in a compiled row — the compiler owns memoization and hand-memoization degrades its analysis.
- Never import `c`/`$*` by hand — every reference is compiler-emitted; a source-level import is the named defect.
- Keep values the compiler memoizes immutable; a post-render mutation is exactly what `$structuralCheck` flags in dev and what silently breaks reuse in production.
- Treat the dev validators as diagnostics, not API: read `console.error` output and `renderCounterRegistry`, never call `$structuralCheck`/`$dispatcherGuard` directly.

[RAIL_LAW]:
- Package: `react-compiler-runtime`
- Owns: the `c(N)` memo-cache allocator the React Compiler emits against, the `$reset` invalidator, and the dev-mode `$dispatcherGuard`/`$structuralCheck`/`useRenderCounter` validators
- Accept: compiler-emitted `_c(N)` allocation only, React-19 delegation to the built-in `useMemoCache`, the dev validators as erasable diagnostics, `babel-plugin-react-compiler` as the paired emitter
- Reject: hand-written `useMemo`/`useCallback`/`React.memo`/dependency arrays, source-level imports of `c`/`$*`, post-render mutation of memoized values, treating the dev validators as a callable API
