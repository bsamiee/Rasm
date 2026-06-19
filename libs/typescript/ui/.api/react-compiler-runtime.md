# [API_CATALOGUE] react-compiler-runtime

`react-compiler-runtime` provides the memo-cache slot management, optional hook-guard assertions, structural change detection, and render counter utilities that the React Compiler emits calls to when `target` is set to a custom runtime (instead of the built-in React 19 `c` export). It acts as the compiler-generated code's runtime substrate.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-compiler-runtime`
- package: `react-compiler-runtime`
- module: `react-compiler-runtime`
- asset: compiler-emitted runtime helpers — memo cache, dispatcher guards, structural checks, render counters
- rail: render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cache and guard types
- rail: render

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [RAIL]                                                                   |
| :-----: | :---------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `MemoCache` | cache array   | `Array<number \| typeof $empty>`                                         |
|  [02]   | `GuardKind` | guard enum    | `PushGuardContext \| PopGuardContext \| PushExpectHook \| PopExpectHook` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: runtime helpers
- rail: render

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RAIL]                                                     |
| :-----: | :----------------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `c`                                                                | cache hook     | `any` — compiler-emitted memo cache slot hook              |
|  [02]   | `$reset($)`                                                        | cache reset    | clears all slots in a `MemoCache` for re-render            |
|  [03]   | `$dispatcherGuard(kind)`                                           | guard fn       | push/pop hook-order enforcement context                    |
|  [04]   | `$makeReadOnly()`                                                  | freeze fn      | marks a value as readonly in dev mode                      |
|  [05]   | `$structuralCheck(oldValue, newValue, varName, fnName, kind, loc)` | check fn       | structural equality assertion for debugging                |
|  [06]   | `useRenderCounter(name)`                                           | render hook    | increments a named render counter in dev                   |
|  [07]   | `clearRenderCounterRegistry()`                                     | test utility   | resets all render counters between tests                   |
|  [08]   | `renderCounterRegistry`                                            | registry       | `Map<string, Set<{ count: number }>>` — live render counts |

## [04]-[IMPLEMENTATION_LAW]

[RUNTIME_TOPOLOGY]:
- `c` is a React hook (`use memo` protocol); it must be called at the top level of a compiled component — the compiler emits all `c(...)` calls unconditionally
- `$empty` is a unique symbol sentinel meaning "slot not yet populated"; `MemoCache` entries are initialized to `$empty` before first render
- `$reset($)` sets every slot back to `$empty`, forcing a full re-evaluation on the next render — used only in dev or test tooling
- `$dispatcherGuard` assertions are emitted only in development builds when the compiler's `enableEmitHookGuards` option points at this package
- `$structuralCheck` is emitted by the compiler's change-detection instrumentation pass; it is a no-op in production
- `renderCounterRegistry` is a module-level `Map`; `clearRenderCounterRegistry()` must be called between test cases to avoid counter leakage

[LOCAL_ADMISSION]:
- Reference this package only through `babel-plugin-react-compiler`'s `target: { kind: "donotuse_meta_internal", runtimeModule: "react-compiler-runtime" }` option; do not import its helpers manually in application code.
- In React 19 projects without a custom runtime module, the compiler uses React's built-in `cache` slot and this package is not required.

[RAIL_LAW]:
- Package: `react-compiler-runtime`
- Owns: compiler-emitted memo cache, hook-guard dev assertions, structural equality checks, render counters
- Accept: compiler-emitted calls via `babel-plugin-react-compiler` custom target
- Reject: manual import of `c`, `$reset`, or `$structuralCheck` in hand-authored application code
