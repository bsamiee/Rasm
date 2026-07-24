# [TS_UI_API_BABEL_PLUGIN_REACT_COMPILER]

`babel-plugin-react-compiler` compiles each component and hook at build time, inferring reactive dependencies and rewriting the body to memoize values and JSX through a `const $ = _c(n)` cache-slot array, so `ui` bans hand-written `useMemo`/`useCallback`/`React.memo`. Its surface is a build-wiring config bag (`PluginOptions`), a `"use memo"`/`"use no memo"` directive escape hatch, and a programmatic compile and diagnostic API — never a runtime import.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `babel-plugin-react-compiler`
- package: `babel-plugin-react-compiler` (MIT)
- module: CJS `dist/index.js`, default export `BabelPluginReactCompiler(babel) -> PluginObj`; depends `@babel/types`
- asset: ships `dist/index.d.ts` with no `package.json` `types` field — resolution reads the declaration by path, not by field
- runtime: build-time Babel pass emitting `_c` calls into the compiler runtime, never imported by `ui` source
- plane: `plane:ui` (build) — enabled for the `ui` core and `ui/viewer` React projects, wired through the bundler
- rail: ui/compiler — the memoization-compilation pass paired with `react-compiler-runtime` (`.api/react-compiler-runtime.md`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the config bag, directive vocabulary, and diagnostic and HIR algebra the package exports

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `PluginOptions`                                                | config bag    | `Partial<>` axes validated by `parsePluginOptions` |
|  [02]   | `ExternalFunction`                                             | struct        | `{ source, importSpecifierName }` emission target  |
|  [03]   | `OPT_IN_DIRECTIVES` / `OPT_OUT_DIRECTIVES`                     | directive set | per-fn compile opt-in / opt-out                    |
|  [04]   | `CompilerError` / `CompilerErrorDetail` / `CompilerDiagnostic` | diagnostic    | typed bail-out the `logger` classifies             |
|  [05]   | `ErrorCategory` / `ErrorSeverity`                              | vocabulary    | bail-out severity and category                     |
|  [06]   | `CompilerSuggestionOperation` / `LintRules`                    | vocabulary    | fix-suggestion op and lint-rule roster             |
|  [07]   | `LoggerEvent`                                                  | union         | the `logger.logEvent` payload (cases below)        |
|  [08]   | `Effect` / `ValueKind` / `ValueReason` / `ProgramContext`      | HIR algebra   | internal inference vocabulary (tooling)            |

- `LoggerEvent` cases: `CompileSuccess` `CompileError` `CompileDiagnostic` `CompileSkip` `Timing`.

[PLUGIN_OPTIONS_AXES]: every `PluginOptions` axis is a policy value on one bag; extend `environment`, never fork the plugin

| [INDEX] | [OPTION]                   | [DECISION_BOUNDARY]                                                                         |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `target`                   | React runtime; `"19"` = built-in `react/compiler-runtime`, 17/18 = `react-compiler-runtime` |
|  [02]   | `compilationMode`          | `infer` (components+hooks by heuristic, default); `annotation` = only `"use memo"`; `all`   |
|  [03]   | `panicThreshold`           | `none` (prod: skip a fn on any diagnostic, never break the build) vs strict CI              |
|  [04]   | `sources`                  | the include predicate deciding which files compile                                          |
|  [05]   | `gating` / `dynamicGating` | static/runtime rollout: emit compiled + original, a flag picks                              |
|  [06]   | `logger`                   | the compile-diagnostic sink (success/error/skip/timing events)                              |
|  [07]   | `environment`              | inference tuning plus the dev-validator emission flags                                      |
|  [08]   | `noEmit`                   | analyze-only: compile + diagnose, emit nothing (the CI gate mode)                           |
|  [09]   | `eslintSuppressionRules`   | skip a fn already carrying a suppressed ESLint rule                                         |
|  [10]   | `flowSuppressions`         | skip a fn under a Flow suppression comment                                                  |
|  [11]   | `ignoreUseNoForget`        | compile even a `"use no memo"`-marked fn                                                    |
|  [12]   | `customOptOutDirectives`   | extend the opt-out directive vocabulary                                                     |

[ENVIRONMENT_EMISSION]: each dev-validator axis is an `ExternalFunction` (`{ source, importSpecifierName }`, default `null` = off) naming the `react-compiler-runtime` export a compiled dev body imports; production strips the call

| [INDEX] | [ENVIRONMENT_FLAG]                  | [EMITTED_CALL]                                    | [GUARD]                                     |
| :-----: | :---------------------------------- | :------------------------------------------------ | :------------------------------------------ |
|  [01]   | `enableEmitHookGuards`              | `$dispatcherGuard(kind: GuardKind)`               | throws on a conditional / indirect hook     |
|  [02]   | `enableChangeDetectionForDebugging` | `$structuralCheck(old, new, name, fn, kind, loc)` | diffs a stable value against recompute      |
|  [03]   | `enableEmitInstrumentForget`        | `useRenderCounter(name)`                          | rerender probe into `renderCounterRegistry` |
|  [04]   | `enableEmitFreeze`                  | `$makeReadOnly()`                                 | deep-freeze; runtime stub, flag stays off   |

- `enableEmitInstrumentForget` carries the richer `{ fn, gating, globalGating }` payload adding a per-emit gating fn and env-var name.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the plugin default export and the programmatic compile, config-codec, and directive-probe surface — used by tooling and the `logger` sink, not `ui` source

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------ | :------ | :------------------------------------ |
|  [01]   | `BabelPluginReactCompiler(BabelCore) -> PluginObj`                        | factory | the default-export Babel plugin       |
|  [02]   | `compile` / `compileProgram`                                              | static  | programmatic fn / program compile     |
|  [03]   | `runBabelPluginReactCompiler(text, file, language, options, includeAst?)` | static  | raw source to compiled program        |
|  [04]   | `parsePluginOptions(options)` / `validateEnvironmentConfig(env)`          | static  | normalize / validate config           |
|  [05]   | `findDirectiveEnablingMemoization` / `findDirectiveDisablingMemoization`  | static  | detect the opt-in / opt-out directive |
|  [06]   | `printHIR`                                                                | static  | HIR debug printer (tooling)           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `BabelPluginReactCompiler` runs first in the bundler's React Babel transform, emitting to the browser only `_c(N)` cache-slot allocations and, in dev, the `environment`-gated validator calls; `target: "19"` binds React's built-in `react/compiler-runtime` for `_c`, and a mismatched `target` breaks `_c` resolution.
- Memoization is compiled: a hand-written `useMemo`/`useCallback`/`React.memo` in a `ui` row is the defect the plugin's presence bans.

[STACKING]:
- `react-compiler-runtime`(`.api/react-compiler-runtime.md`): compiled output calls `const $ = _c(N)` to allocate the per-render memo cache; `target: "19"` routes `_c` to React's built-in runtime, leaving the sibling dormant as the sub-19 recovery and the dev-validator lane its `environment` `{ source, importSpecifierName }` flags import (`$dispatcherGuard`/`$structuralCheck`/`useRenderCounter`/`$makeReadOnly`).
- `react`(`.api/react.md`): compiled bodies compose with `useTransition`/`<Suspense>` and React's own `useMemoCache` untouched, no manual dependency tracking.
- `effect-atom-atom-react`(`.api/effect-atom-atom-react.md`): `useAtomValue`/`useAtomSet` are ordinary hooks, so `ONE_FOLD_ONE_BINDING` components and their selector-scoped reads compile with no annotation; the compiler stabilizes the hook results, so no `useMemo` wraps them.
- bundler Babel pass — `@vitejs/plugin-react` / `@rolldown/plugin-babel`: the plugin wires first in the React transform (`babel: { plugins: [["babel-plugin-react-compiler", { target: "19" }]] }`), seeing source before other transforms lower it.
- `@biomejs/biome` lint boundary: Rasm lints with Biome, so no `eslint-plugin-react-hooks` react-compiler rule fires; the diagnostic path is the build-time `logger` sink and `panicThreshold: "none"` (a non-compilable component skips silently), and `noEmit: true` is the CI analyze-only mode surfacing `CompilerError` without emitting.

[LOCAL_ADMISSION]:
- Wire the plugin first in the bundler React Babel pass with `{ target: "19", compilationMode: "infer", panicThreshold: "none" }`; never import this package from `ui` source — it is a build tool.
- `"use no memo"` marks a rare component the compiler mishandles, a defect flag to remove; `gating` admits only a deliberate incremental rollout.

[RAIL_LAW]:
- Package: `babel-plugin-react-compiler`
- Owns: the build-time memoization-compilation pass, the `PluginOptions` config bag, the `"use memo"`/`"use no memo"` directive control, and the programmatic compile and `CompilerError`/`LoggerEvent` diagnostic API
- Accept: the plugin wired first in the bundler React transform with `target: "19"`; `"use no memo"` as a marked escape hatch; the `logger`/`noEmit` diagnostic path; `gating` for a deliberate rollout
- Reject: importing this package from `ui` source; a `target` mismatched to the installed React; hand-written `useMemo`/`useCallback`/`React.memo` in `ui`; a standing `"use no memo"`; relying on an ESLint react-compiler rule
