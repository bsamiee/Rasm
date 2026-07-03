# [API_CATALOGUE] react-compiler-runtime

`react-compiler-runtime` is the compiler-emitted runtime substrate the React Compiler targets when `babel-plugin-react-compiler` is configured with `target: { kind: "donotuse_meta_internal", runtimeModule: "react-compiler-runtime" }` instead of a numeric React target. It owns the memo-cache slot allocator (`c`), the optional hook-order guard dispatcher (`$dispatcherGuard`), the change-detection instrumentation (`$structuralCheck`), and the render-counter registry — every symbol is emitted by the compiler, never hand-imported into application code. On a React 19 project the numeric `target: "19"` binds React's built-in `c` and this package is bypassed; it is admitted so memoization is deterministic and independent of React internals, and so the dev change-detection/render-count instrumentation is available under one runtime module.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-compiler-runtime`
- package: `react-compiler-runtime`
- version: `1.0.0`
- license: `MIT`
- module: `react-compiler-runtime` (CommonJS `dist/index.js`; no ESM `module`/`exports` map)
- abi: ships `dist/index.d.ts` (legacy `typings` key; no `types`/`exports`/`module` field), TSDECL-resolvable via `api resolve` and fully typed on a manual import; the export clause is 8 value symbols, zero exported types — the d.ts and `src/index.ts` agree
- runtime: peer `react@^17 || ^18 || ^19 || ^0.0.0-experimental`; reads `React.__COMPILER_RUNTIME` and the client secret-internals dispatcher handle
- target: selected via `babel-plugin-react-compiler` `target.runtimeModule`, never a `plugins`-array or source import
- rail: render

## [02]-[COMPILER_CONTRACT]

[COMPILER_CONTRACT_SCOPE]: emitted-call shapes (module-local, unexported)
- rail: render

These are the shapes the emitted calls reference; `dist/index.d.ts` declares all three module-locally but the export clause omits them (the 8 exports are values, zero are types), so a consuming design never names them — they document the memo-slot and guard contracts only.

| [INDEX] | [SYMBOL]    | [SHAPE]                                     | [CONTRACT]                                                                       |
| :-----: | :---------- | :------------------------------------------ | :------------------------------------------------------------------------------- |
|  [01]   | `MemoCache` | `Array<number \| typeof $empty>`            | the per-component slot array `c(size)` allocates and `$reset` refills            |
|  [02]   | `$empty`    | `Symbol.for('react.memo_cache_sentinel')`   | shared unpopulated-slot sentinel; `$[$empty]=true` tags the array for DevTools    |
|  [03]   | `GuardKind` | numeric enum `0..3`                         | `PushGuardContext \| PopGuardContext \| PushExpectHook \| PopExpectHook` guard op |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: memo-cache substrate
- rail: render

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [SIGNATURE / BEHAVIOR]                                                                           |
| :-----: | :--------------------------------- | :------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `c(size)`                          | cache alloc    | d.ts erases it to `any`; behaviorally `(size: number) => Array<number \| typeof $empty>` — delegates to `React.__COMPILER_RUNTIME.c` when present (React 19), else a `useMemo` polyfill filling `size` slots with `$empty` |
|  [02]   | `$reset($)`                        | cache reset    | `($: MemoCache) => void`; refills every slot with `$empty`, forcing full re-evaluation — dev/test only |

[ENTRYPOINT_SCOPE]: hook-order guard
- rail: render

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY] | [SIGNATURE / BEHAVIOR]                                                                                       |
| :-----: | :------------------------ | :------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `$dispatcherGuard(kind)`  | guard fn       | `(kind: GuardKind) => void`; swaps the current dispatcher to a throwing `LazyGuardDispatcher` on push and restores on pop, enforcing the no-conditional-hook-calls rule at runtime — emitted only when the compiler `enableEmitHookGuards` is on |
|  [02]   | `$makeReadOnly()`         | freeze fn      | `() => void`; **unimplemented in `1.0.0`** — throws `'TODO: implement $makeReadOnly in react-compiler-runtime'`; the freeze instrumentation is not wired to an executing runtime call in this build |

[ENTRYPOINT_SCOPE]: change-detection and render-count instrumentation
- rail: render

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]  | [SIGNATURE / BEHAVIOR]                                                                                     |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `$structuralCheck(oldValue, newValue, variableName, fnName, kind, loc)` | check fn        | all-`string` tail params; depth-2 recursive deep-compare (handles `Map`/`Set`/arrays/React elements/`NaN`), `console.error`s the first divergence per unique message via a `seenErrors` dedupe set — emitted by the change-detection pass, no-op-free only when unemitted |
|  [02]   | `useRenderCounter(name)`                                              | render hook     | `(name: string) => void`; increments a named counter each render and registers it into `renderCounterRegistry` on mount, deregistering on unmount |
|  [03]   | `renderCounterRegistry`                                               | registry value  | `Map<string, Set<{ count: number }>>` — live per-name render counters |
|  [04]   | `clearRenderCounterRegistry()`                                        | test utility    | `() => void`; zeroes every registered counter — call between test cases to avoid counter leakage |

## [04]-[IMPLEMENTATION_LAW]

[RUNTIME_TOPOLOGY]:
- `c` is a hook (`use memo` protocol): the compiler emits `c(n)` unconditionally at the top of each compiled component; the returned array is the memo cache read/written by slot index across the compiled body.
- React 19 path: when `React.__COMPILER_RUNTIME.c` is a function, `c` is that reference verbatim, so slots come from React's internal `useMemoCache`; the userspace polyfill only runs on React < 19.
- `$dispatcherGuard` manipulates `ReactCurrentDispatcher.current` through the client secret-internals handle: `PushGuardContext` saves the real dispatcher (first frame only) and installs the throwing guard; `PushExpectHook`/`PopExpectHook` temporarily restore the real dispatcher so an intentional hook call inside a guarded region is allowed. `useMemoCache` is the one allow-listed method on the guard dispatcher.
- `$structuralCheck` and the render-counter surface are diagnostic: they mutate module-level state (`seenErrors`, `renderCounterRegistry`) and emit to `console.error`; they carry no production behavior and exist for the compiler's change-detection pass and for test/telemetry harnesses.

[LOCAL_ADMISSION]:
- Never import `c`, `$reset`, `$dispatcherGuard`, `$structuralCheck`, or `$makeReadOnly` from hand-authored code — they are compiler output; a manual import is the named defect.
- Wire the module through the `babel-plugin-react-compiler` `target.runtimeModule` option (`.api/babel-plugin-react-compiler.md` `CompilerReactTarget`), pairing `enableEmitHookGuards` to activate `$dispatcherGuard` and the change-detection pass to activate `$structuralCheck` in dev builds.
- The only manually-consumable symbols are the instrumentation trio `useRenderCounter` / `renderCounterRegistry` / `clearRenderCounterRegistry`, admissible in a test harness that asserts render counts; treat `$makeReadOnly` as absent until a version implements it.

[STACKING]:
- Stacks under `babel-plugin-react-compiler`: the babel plugin is the producer, this package the runtime sink — the plugin's `target` names this module, its `enableEmitHookGuards`/change-detection options decide which of these symbols are emitted, and its `LoggerEvent` `CompileSuccess` memo stats report against the `c` slots allocated here.
- On this folder's React 19 pin the numeric `target: "19"` bypasses the module; admit the custom `runtimeModule` target only to decouple memoization from React internals or to surface `useRenderCounter`/`$structuralCheck` under the render-studio (`render/dashboard.md`) instrumentation lens.

[RAIL_LAW]:
- package: `react-compiler-runtime`
- owns: compiler-emitted memo-cache allocation, hook-order guard dispatch, structural change detection, render-count instrumentation
- accept: compiler-emitted calls via the `babel-plugin-react-compiler` `runtimeModule` target; manual use only of the render-count instrumentation trio
- reject: manual import of `c`/`$reset`/`$dispatcherGuard`/`$structuralCheck` in application code; reliance on `$makeReadOnly` (throwing stub in `1.0.0`); naming `MemoCache`/`GuardKind` as importable types (the package exports none)
