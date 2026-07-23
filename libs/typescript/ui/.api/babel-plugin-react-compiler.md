# [TS_UI_API_BABEL_PLUGIN_REACT_COMPILER]

`babel-plugin-react-compiler` is the React Compiler as a Babel plugin: it analyzes each component/hook, infers reactive dependencies, and rewrites the function to memoize values and JSX through a per-render cache slot array (`const $ = _c(n)`), so hand-written `useMemo`/`useCallback`/`React.memo` become a build artifact the `ui` React 19 spine never authors. Its surface is NOT a runtime API — it is a `PluginOptions` config bag consumed once at build wiring, a `"use memo"`/`"use no memo"` directive escape hatch, and a programmatic compile + diagnostic API. The config is fully parameterized: `target`, `compilationMode`, `panicThreshold`, `gating`, `logger`, `sources`, and the `environment` knob bag are policy values on one options object, never plugin variants. Inside Rasm both the `ui` core and `ui/viewer` Nx projects enable it; `target: "19"` binds React 19's built-in `react/compiler-runtime`, so the sibling `react-compiler-runtime` package is the sub-19 recovery + the dev structural-check lane, dormant at the shipped React version.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `babel-plugin-react-compiler`
- package: `babel-plugin-react-compiler` (MIT)
- module: CJS `main` `dist/index.js`; dep `@babel/types`. The default export is the Babel plugin `BabelPluginReactCompiler(babel): PluginObj`.
- asset: ships `dist/index.d.ts` (51 KB, the full compiler surface) even though `package.json` declares NO `types` field — a resolution quirk; the TSDECL is present and `assay api resolve babel-plugin-react-compiler` restores it (the resolver's `types`-field read returns `empty`; the file exists at `dist/index.d.ts`).
- runtime: BUILD-TIME only — the plugin runs in the bundler's Babel pass and emits nothing to the browser except calls into the compiler runtime (`_c`); it is a `devDependency`-class tool, never imported by `ui` source.
- plane: `plane:ui` (build) — enabled for the `ui` core and `ui/viewer` React projects; wired through the bundler, not a folder edge.
- rail: ui/compiler; the `[COMPILER]` group (`babel-plugin-react-compiler`, `react-compiler-runtime`).
- role: the memoization-compilation pass that lets `ui` ban hand-written `useMemo`/`useCallback`/`memo`; paired with `react` 19 (`react/compiler-runtime` provides `_c`) and the sibling `react-compiler-runtime` (recovery + dev checks).

## [02]-[PLUGIN_OPTIONS]

`PluginOptions` is `Partial<>` of one config bag — every axis a policy value validated by `parsePluginOptions`/`validateEnvironmentConfig` (a zod schema internally). The load-bearing axes are `target` (React runtime — must match the installed React so `_c` resolves), `compilationMode` (which functions compile), and `panicThreshold` (bail-out severity). `gating`/`dynamicGating` enable incremental rollout: both the compiled and original function are emitted and a runtime flag picks. `environment` is the inference + emission bag: it tunes ref/effect inference AND carries the dev-validator emission flags (`enableEmitHookGuards`/`enableChangeDetectionForDebugging`/`enableEmitInstrumentForget`/`enableEmitFreeze`) that wire compiled dev output to `react-compiler-runtime` (`[ENVIRONMENT_EMISSION]`) — extend it, never fork the plugin.

Types are the signature below; the table carries the decision per axis.

| [INDEX] | [OPTION]                   | [DECISION_BOUNDARY]                                                                         |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `target`                   | React runtime; `"19"` = built-in `react/compiler-runtime`, 17/18 = `react-compiler-runtime` |
|  [02]   | `compilationMode`          | `infer` (components+hooks by heuristic, default); `annotation` = only `"use memo"`; `all`   |
|  [03]   | `panicThreshold`           | `none` (prod: skip a fn on any diagnostic, never break the build) vs strict CI              |
|  [04]   | `sources`                  | which files compile — the include predicate                                                 |
|  [05]   | `gating` / `dynamicGating` | static/runtime rollout: emit compiled + original, a flag picks                              |
|  [06]   | `logger`                   | the compile-diagnostic sink (success/error/skip/timing events)                              |
|  [07]   | `environment`              | inference tuning + the dev-validator emission flags; extend, never fork                     |
|  [08]   | `noEmit`                   | analyze-only: compile + diagnose, emit nothing (the CI gate mode)                           |
|  [09]   | `eslintSuppressionRules`   | skip a fn already carrying a suppressed ESLint rule                                         |
|  [10]   | `flowSuppressions`         | skip a fn under a Flow suppression comment                                                  |
|  [11]   | `ignoreUseNoForget`        | compile even a `"use no memo"`-marked fn                                                    |
|  [12]   | `customOptOutDirectives`   | extend the opt-out directive vocabulary                                                     |

[PLUGIN_OPTIONS]: `PluginOptions = Partial<…>`
[SURFACES]: `BabelPluginReactCompiler(typeof BabelCore) -> BabelCore.PluginObj`

[ENVIRONMENT_EMISSION]: the dev-validator emission flags — how the plugin wires compiled dev output to `react-compiler-runtime`

The `environment` axes decide whether the compiler emits a dev-validator call into each compiled body and which module/export it imports. None is a boolean — each is one `ExternalFunction`-shaped config `{ source: string; importSpecifierName: string }` (nullable, default `null` = off), so the flag is a parameterized import target: `enableEmitHookGuards: { source: "react-compiler-runtime", importSpecifierName: "$dispatcherGuard" }` emits `import { $dispatcherGuard } from "react-compiler-runtime"` plus the guard call at each body head. This is the reciprocal seam to `.api/react-compiler-runtime.md`'s dev-validator surface — the flag names the emit, the runtime owns the implementation — and the calls are stripped from production output.

| [INDEX] | [ENVIRONMENT_FLAG]                  | [EMITTED_CALL]                                    |
| :-----: | :---------------------------------- | :------------------------------------------------ |
|  [01]   | `enableEmitHookGuards`              | `$dispatcherGuard(kind: GuardKind)`               |
|  [02]   | `enableChangeDetectionForDebugging` | `$structuralCheck(old, new, name, fn, kind, loc)` |
|  [03]   | `enableEmitInstrumentForget`        | `useRenderCounter(name)`                          |
|  [04]   | `enableEmitFreeze`                  | `$makeReadOnly()`                                 |

- [01]-[HOOK_GUARDS]: wraps each compiled body; throws on a conditional, renamed, or indirectly-called hook (the runtime source comment names this exact flag).
- [02]-[CHANGE_DETECTION]: deep-diffs a compiler-assumed-stable value against recompute and `console.error`s the divergence path.
- [03]-[INSTRUMENT_FORGET]: rerender probe into `renderCounterRegistry`; the richer payload adds a per-emit `gating` external-fn and a `globalGating` env-var name (`{ fn; gating; globalGating }`).
- [04]-[EMIT_FREEZE]: the reserved deep-freeze; the runtime stubs it (throws), so the flag stays off until the runtime lands it.

## [03]-[DIRECTIVE_CONTROL]

The per-function escape hatch is a string directive, not a config fork: `"use memo"` opts a function IN (`OPT_IN_DIRECTIVES`), `"use no memo"` opts it OUT (`OPT_OUT_DIRECTIVES`) — the one admitted way to exclude a component the compiler mis-handles, and a defect marker to remove, never a standing pattern. `compilationMode: "annotation"` inverts the default so ONLY `"use memo"` functions compile. `customOptOutDirectives`/`ignoreUseNoForget` retune the opt-out vocabulary at the config seam.

| [INDEX] | [SYMBOL]                                                                 | [KIND]        | [CAPABILITY_BOUNDARY]                      |
| :-----: | :----------------------------------------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `OPT_IN_DIRECTIVES` / `OPT_OUT_DIRECTIVES`                               | directive set | the per-fn opt-in / opt-out compile toggle |
|  [02]   | `findDirectiveEnablingMemoization` / `findDirectiveDisablingMemoization` | probe         | detect the directive on a fn body          |
|  [03]   | `customOptOutDirectives` / `ignoreUseNoForget` (options)                 | config axis   | retune/ignore the opt-out vocabulary       |

## [04]-[PROGRAMMATIC_AND_DIAGNOSTICS]

Beyond the plugin, the package exposes the compile entry points and the diagnostic rail — used by tests, custom bundler integrations, and the `logger` sink, not by `ui` source. `CompilerError`/`CompilerDiagnostic` carry `ErrorCategory` + `ErrorSeverity` so a build can classify a bail-out; `LoggerEvent` is the discriminated event the `logger.logEvent` sink receives. `Effect`/`ValueKind`/`ValueReason` are the compiler's internal HIR inference algebra (exported for tooling, not a consumer surface).

| [INDEX] | [SYMBOL]                                                                        | [KIND]        | [CAPABILITY_BOUNDARY]                  |
| :-----: | :------------------------------------------------------------------------------ | :------------ | :------------------------------------- |
|  [01]   | `compile` / `compileProgram`                                                    | compile entry | programmatic fn/program compile        |
|  [02]   | `runBabelPluginReactCompiler(text, file, language, options, includeAst?)`       | compile entry | raw source→compiled program            |
|  [03]   | `parsePluginOptions(options)` / `validateEnvironmentConfig(env)`                | config codec  | normalize/validate config before use   |
|  [04]   | `CompilerError` / `CompilerErrorDetail` / `CompilerDiagnostic`                  | diagnostic    | typed bail-out the `logger` classifies |
|  [05]   | `ErrorCategory` / `ErrorSeverity` / `CompilerSuggestionOperation` / `LintRules` | vocabulary    | severity/category + lint-rule roster   |
|  [06]   | `LoggerEvent`                                                                   | event union   | the `logEvent` payload union ([07])    |
|  [07]   | `Effect` / `ValueKind` / `ValueReason` / `ProgramContext` / `printHIR`          | internal HIR  | HIR algebra + debug printers (tooling) |

- [07]-[LOGGER_EVENT]: `CompileSuccess`/`CompileError`/`CompileDiagnostic`/`CompileSkip`/`Timing`/… — the union `logger.logEvent` receives.

## [05]-[STACKING]

- Stack with the bundler Babel pass — `vite` 8 + `@vitejs/plugin-react` / `@rolldown/plugin-babel`: the plugin runs as the FIRST Babel plugin in the React transform (order matters: it must see the source before other transforms lower it). It wires through `@vitejs/plugin-react`'s `babel: { plugins: [["babel-plugin-react-compiler", { target: "19" }]] }` or `@rolldown/plugin-babel` (both admitted in the tooling catalog). The compile is build-time; nothing about the plugin ships to the browser.
- Stack with `react` 19.2 + `react-compiler-runtime` (the `_c` primitive): compiled output calls `const $ = _c(N)` to allocate the per-render memo-cache slot array. `target: "19"` binds React's BUILT-IN `react/compiler-runtime` for `_c`, so the sibling `react-compiler-runtime` package is dormant at the shipped version — it is the sub-19 `runtimeModule` recovery AND the dev-validator lane (`$dispatcherGuard`/`$structuralCheck`/`useRenderCounter`/`$makeReadOnly`) the `environment` emission flags (`[ENVIRONMENT_EMISSION]`) point their `{ source, importSpecifierName }` at. The `[COMPILER]` group carries both; only the plugin + built-in runtime are live.
- Stack with `.api/effect-atom-atom-react.md` state hooks: the compiler respects the Rules of React, and `@effect-atom/atom-react` hooks (`useAtomValue`/`useAtom`) are ordinary hooks, so `ONE_FOLD_ONE_BINDING` components compile with no annotation; the compiler memoizes the derived-atom reads `atom/derive` produces without hand-written `useMemo`. A component the compiler cannot prove is marked `"use no memo"` as a defect flag, not left hand-memoized.
- Stack with Biome lint boundary — no ESLint react-compiler rule: Rasm lints with `@biomejs/biome`, not ESLint, so the standard `eslint-plugin-react-hooks` react-compiler lint rule is NOT wired. The compile-diagnostic path is therefore the build-time `logger` sink + `panicThreshold` (`"none"` in production: a non-compilable component is silently skipped, never a build break) — `noEmit: true` is the analyze-only mode a CI gate does run to surface `CompilerError` diagnostics without emitting.
- Stack with the `ui` memoization law: because memoization is compiled, `ui`/`ui/viewer` source authors ZERO `useMemo`/`useCallback`/`React.memo`; those are the compiler's output. A hand-written memo in a `view`/`act`/`atom` row is the defect this plugin's presence bans.

## [06]-[RAIL_LAW]

- Owns: the build-time memoization-compilation pass (the `BabelPluginReactCompiler` default export), the `PluginOptions` config bag, the `"use memo"`/`"use no memo"` directive control, and the programmatic compile + `CompilerError`/`LoggerEvent` diagnostic API.
- Accept: the plugin wired FIRST in the bundler React Babel pass (`@vitejs/plugin-react`/`@rolldown/plugin-babel`) with `{ target: "19", compilationMode: "infer", panicThreshold: "none" }`; `"use no memo"` as a rare, marked escape hatch; the `logger`/`noEmit` diagnostic path since Biome owns lint; `gating` only for a deliberate incremental rollout.
- Reject: importing this package from `ui` source (it is a build tool, never a runtime import); a `target` mismatched to the installed React (breaks `_c` resolution); hand-written `useMemo`/`useCallback`/`memo` in `ui` (memoization is compiled); a standing `"use no memo"` (a defect marker, not a pattern); relying on an ESLint react-compiler rule (Biome is the linter — use `logger`/`noEmit`).
- Boundary: build-time only — nothing ships to the browser but `_c` calls into the compiler runtime (and, in dev, the `environment`-flag-gated `$dispatcherGuard`/`$structuralCheck`/`useRenderCounter` validators). `react/compiler-runtime` (built into React 19) provides `_c`; `react-compiler-runtime` is the sub-19 recovery + dev-validator lane. The `types` field is absent from `package.json` but `dist/index.d.ts` is present.
