# [babel-plugin-react-compiler] — the build-time memoization pass; `useMemo`/`useCallback`/`memo` are compiled, never hand-written

`babel-plugin-react-compiler` is the React Compiler as a Babel plugin: it analyzes each component/hook, infers reactive dependencies, and rewrites the function to memoize values and JSX through a per-render cache slot array (`const $ = _c(n)`), so hand-written `useMemo`/`useCallback`/`React.memo` become a build artifact the `ui` React 19 spine never authors. Its surface is NOT a runtime API — it is a `PluginOptions` config bag consumed once at build wiring, a `"use memo"`/`"use no memo"` directive escape hatch, and a programmatic compile + diagnostic API. The config is fully parameterized: `target`, `compilationMode`, `panicThreshold`, `gating`, `logger`, `sources`, and the `environment` knob bag are policy values on one options object, never plugin variants. Inside Rasm both the `ui` core and `ui/viewer` Nx projects enable it; `target: "19"` binds React 19's built-in `react/compiler-runtime`, so the sibling `react-compiler-runtime` package is the sub-19 fallback + the dev structural-check lane, dormant at the shipped React version.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `babel-plugin-react-compiler`
- package: `babel-plugin-react-compiler` · version `1.0.0` · license `MIT`
- module: CJS `main` `dist/index.js`; dep `@babel/types`. The default export is the Babel plugin `BabelPluginReactCompiler(babel): PluginObj`.
- asset: ships `dist/index.d.ts` (51 KB, the full compiler surface) even though `package.json` declares NO `types` field — a resolution quirk; the TSDECL is present and `assay api resolve babel-plugin-react-compiler` restores it (the resolver's `types`-field read returns `empty`; the file exists at `dist/index.d.ts`).
- runtime: BUILD-TIME only — the plugin runs in the bundler's Babel pass and emits nothing to the browser except calls into the compiler runtime (`_c`); it is a `devDependency`-class tool, never imported by `ui` source.
- plane: `plane:ui` (build) — enabled for the `ui` core and `ui/viewer` React projects; wired through the bundler, not a folder edge.
- rail: ui/compiler; the `[COMPILER]` group (`babel-plugin-react-compiler`, `react-compiler-runtime`).
- role: the memoization-compilation pass that lets `ui` ban hand-written `useMemo`/`useCallback`/`memo`; paired with `react` 19 (`react/compiler-runtime` provides `_c`) and the sibling `react-compiler-runtime` (fallback + dev checks).

## [02]-[PLUGIN_OPTIONS]

`PluginOptions` is `Partial<>` of one config bag — every axis a policy value validated by `parsePluginOptions`/`validateEnvironmentConfig` (a zod schema internally). The load-bearing axes are `target` (React runtime — must match the installed React so `_c` resolves), `compilationMode` (which functions compile), and `panicThreshold` (bail-out severity). `gating`/`dynamicGating` enable incremental rollout: both the compiled and original function are emitted and a runtime flag picks. `environment` is the inference + emission bag: it tunes ref/effect inference AND carries the dev-validator emission flags (`enableEmitHookGuards`/`enableChangeDetectionForDebugging`/`enableEmitInstrumentForget`/`enableEmitFreeze`) that wire compiled dev output to `react-compiler-runtime` (`[ENVIRONMENT_EMISSION]`) — extend it, never fork the plugin.

| [INDEX] | [OPTION]                                   | [TYPE / VALUES]                                                          | [DECISION / BOUNDARY]                                              |
| :-----: | :----------------------------------------- | :---------------------------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `target`                                   | `"17" \| "18" \| "19" \| { kind: "donotuse_meta_internal"; runtimeModule: string }` | React runtime; `"19"` → built-in `react/compiler-runtime`; 17/18 → `{ runtimeModule: "react-compiler-runtime" }` |
|  [02]   | `compilationMode`                          | `"infer" \| "annotation" \| "syntax" \| "all"`                          | `infer` (components+hooks by heuristic, default); `annotation` = only `"use memo"`; `all` = every fn |
|  [03]   | `panicThreshold`                           | `"none" \| "critical_errors" \| "all_errors"`                           | `none` (prod: skip a fn on any diagnostic, never break the build) vs strict CI |
|  [04]   | `sources`                                  | `Array<string> \| ((filename: string) => boolean) \| null`              | which files compile — the include predicate                       |
|  [05]   | `gating` / `dynamicGating`                 | `{ source: string; importSpecifierName: string } \| null`               | static/runtime rollout: emit compiled + original, a flag picks    |
|  [06]   | `logger`                                    | `{ logEvent(filename: string \| null, event: LoggerEvent): void } \| null` | the compile-diagnostic sink (success/error/skip/timing events)    |
|  [07]   | `environment`                              | `Partial<EnvironmentConfig>`                                            | inference tuning (ref/effect assumptions) + dev-validator emission flags (`[ENVIRONMENT_EMISSION]`) — extend, never fork |
|  [08]   | `noEmit` / `eslintSuppressionRules` / `flowSuppressions` / `ignoreUseNoForget` / `customOptOutDirectives` | `boolean` / lint config | analyze-only + suppression/opt-out control    |

```ts contract
type PluginOptions = Partial<{
  target: CompilerReactTarget                              // "17" | "18" | "19" | { kind: "donotuse_meta_internal"; runtimeModule: string }
  compilationMode: "infer" | "annotation" | "syntax" | "all"
  panicThreshold: "none" | "critical_errors" | "all_errors"
  sources: Array<string> | ((filename: string) => boolean) | null
  gating: { source: string; importSpecifierName: string } | null
  dynamicGating: { source: string; importSpecifierName: string } | null
  logger: { logEvent: (filename: string | null, event: LoggerEvent) => void } | null
  environment: Partial<EnvironmentConfig>
  noEmit: boolean; flowSuppressions: boolean; ignoreUseNoForget: boolean
  eslintSuppressionRules: Array<string> | null | undefined; customOptOutDirectives: CustomOptOutDirective
}>
declare function BabelPluginReactCompiler(babel: typeof BabelCore): BabelCore.PluginObj   // the default export
// Rasm wiring — React 19.2, so target "19", prod panic behavior, compiled output calls react/compiler-runtime `_c`:
//   { target: "19", compilationMode: "infer", panicThreshold: "none" }
```

[ENVIRONMENT_EMISSION]: the dev-validator emission flags — how the plugin wires compiled dev output to `react-compiler-runtime`

Four `environment` axes decide whether the compiler EMITS a dev-validator call into each compiled body AND which module/export it imports the validator from. None is a boolean — each is one `ExternalFunction`-shaped config `{ source: string; importSpecifierName: string }` (nullable, default `null` = off), so the flag is a parameterized import target: `enableEmitHookGuards: { source: "react-compiler-runtime", importSpecifierName: "$dispatcherGuard" }` makes the compiler emit `import { $dispatcherGuard } from "react-compiler-runtime"` plus the guard call at each body head. This is the reciprocal seam to `.api/react-compiler-runtime.md`'s dev-validator surface — the flag names the emit, the runtime owns the impl — and all four calls are stripped from production output.

| [INDEX] | [ENVIRONMENT_FLAG]                  | [PAYLOAD]                                                                                                                        | [EMITTED CALL — `react-compiler-runtime` export]                                                                             |
| :-----: | :---------------------------------- | :------------------------------------------------------------------------------------------------------------------------------ | :------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `enableEmitHookGuards`              | `{ source; importSpecifierName } \| null`                                                                                        | `$dispatcherGuard(kind: GuardKind)` — wraps each compiled body, throws on a conditional / renamed / indirectly-called hook (the runtime source comment names this exact flag) |
|  [02]   | `enableChangeDetectionForDebugging` | `{ source; importSpecifierName } \| null`                                                                                        | `$structuralCheck(old, new, name, fn, kind, loc)` — deep-diffs a compiler-assumed-stable value against recompute and `console.error`s the divergence path |
|  [03]   | `enableEmitInstrumentForget`        | `{ fn: { source; importSpecifierName }; gating: { source; importSpecifierName } \| null; globalGating: string \| null } \| null` | `useRenderCounter(name)` — rerender probe into `renderCounterRegistry`; the richer shape carries a per-emit `gating` external-fn and a `globalGating` env-var name |
|  [04]   | `enableEmitFreeze`                  | `{ source; importSpecifierName } \| null`                                                                                        | `$makeReadOnly()` — the reserved deep-freeze; runtime `1.0.0` stubs it (throws), so this flag stays off until the runtime lands it |

## [03]-[DIRECTIVE_CONTROL]

The per-function escape hatch is a string directive, not a config fork: `"use memo"` opts a function IN (`OPT_IN_DIRECTIVES`), `"use no memo"` opts it OUT (`OPT_OUT_DIRECTIVES`) — the one admitted way to exclude a component the compiler mis-handles, and a defect marker to remove, never a standing pattern. `compilationMode: "annotation"` inverts the default so ONLY `"use memo"` functions compile. `customOptOutDirectives`/`ignoreUseNoForget` retune the opt-out vocabulary at the config seam.

| [INDEX] | [SYMBOL]                                                       | [KIND]        | [CAPABILITY / BOUNDARY]                                             |
| :-----: | :------------------------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `OPT_IN_DIRECTIVES` / `OPT_OUT_DIRECTIVES`                     | directive set | `"use memo"` / `"use no memo"` — the per-fn compile toggle          |
|  [02]   | `findDirectiveEnablingMemoization` / `findDirectiveDisablingMemoization` | probe | detect the directive on a function body (the compiler's own gate)  |
|  [03]   | `customOptOutDirectives` / `ignoreUseNoForget` (options)      | config axis   | retune / ignore the opt-out directive vocabulary                   |

## [04]-[PROGRAMMATIC_AND_DIAGNOSTICS]

Beyond the plugin, the package exposes the compile entry points and the diagnostic rail — used by tests, custom bundler integrations, and the `logger` sink, not by `ui` source. `CompilerError`/`CompilerDiagnostic` carry `ErrorCategory` + `ErrorSeverity` so a build can classify a bail-out; `LoggerEvent` is the discriminated event the `logger.logEvent` sink receives. `Effect`/`ValueKind`/`ValueReason` are the compiler's internal HIR inference algebra (exported for tooling, not a consumer surface).

| [INDEX] | [SYMBOL]                                                                  | [KIND]        | [CAPABILITY / BOUNDARY]                                             |
| :-----: | :------------------------------------------------------------------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `compile` / `compileProgram` / `runBabelPluginReactCompiler(text, file, language, options, includeAst?)` | compile entry | programmatic compile of a fn/program/source — custom integration/tests |
|  [02]   | `parsePluginOptions(options)` / `validateEnvironmentConfig(env)`          | config codec  | normalize/validate `PluginOptions`/`EnvironmentConfig` before use   |
|  [03]   | `CompilerError` / `CompilerErrorDetail` / `CompilerDiagnostic`            | diagnostic    | the typed bail-out carrier the `logger` + `panicThreshold` classify |
|  [04]   | `ErrorCategory` / `ErrorSeverity` / `CompilerSuggestionOperation` / `LintRules` | vocabulary | diagnostic severity/category + the lint-rule roster                |
|  [05]   | `LoggerEvent`                                                             | event union   | `CompileSuccess`/`CompileError`/`CompileDiagnostic`/`CompileSkip`/`Timing`/… — the `logEvent` payload |
|  [06]   | `Effect` / `ValueKind` / `ValueReason` / `ProgramContext` / `printHIR`    | internal HIR  | the compiler's inference algebra + debug printers (tooling, not consumer) |

## [05]-[STACKING]

- [STACK: the bundler Babel pass — `vite` 8 + `@vitejs/plugin-react` / `@rolldown/plugin-babel`] — the plugin runs as the FIRST Babel plugin in the React transform (order matters: it must see the source before other transforms lower it). It wires through `@vitejs/plugin-react`'s `babel: { plugins: [["babel-plugin-react-compiler", { target: "19" }]] }` or `@rolldown/plugin-babel` (both admitted in the tooling catalog). The compile is build-time; nothing about the plugin ships to the browser.
- [STACK: `react` 19.2 + `react-compiler-runtime` (the `_c` primitive)] — compiled output calls `const $ = _c(N)` to allocate the per-render memo-cache slot array. `target: "19"` binds React's BUILT-IN `react/compiler-runtime` for `_c`, so the sibling `react-compiler-runtime` package is dormant at the shipped version — it is the sub-19 `runtimeModule` fallback AND the dev-validator lane (`$dispatcherGuard`/`$structuralCheck`/`useRenderCounter`/`$makeReadOnly`) the `environment` emission flags (`[ENVIRONMENT_EMISSION]`) point their `{ source, importSpecifierName }` at. The `[COMPILER]` group carries both; only the plugin + built-in runtime are live.
- [STACK: `.api/effect-atom-atom-react.md` state hooks] — the compiler respects the Rules of React, and `@effect-atom/atom-react` hooks (`useAtomValue`/`useAtom`) are ordinary hooks, so `ONE_FOLD_ONE_BINDING` components compile with no annotation; the compiler memoizes the derived-atom reads `atom/derive` produces without hand-written `useMemo`. A component the compiler cannot prove is marked `"use no memo"` as a defect flag, not left hand-memoized.
- [STACK: Biome lint boundary — no ESLint react-compiler rule] — Rasm lints with `@biomejs/biome`, not ESLint, so the standard `eslint-plugin-react-hooks` react-compiler lint rule is NOT wired. The compile-diagnostic path is therefore the build-time `logger` sink + `panicThreshold` (`"none"` in production: a non-compilable component is silently skipped, never a build break) — `noEmit: true` is the analyze-only mode a CI gate would run to surface `CompilerError` diagnostics without emitting.
- [STACK: the `ui` memoization law] — because memoization is compiled, `ui`/`ui/viewer` source authors ZERO `useMemo`/`useCallback`/`React.memo`; those are the compiler's output. A hand-written memo in a `view`/`act`/`atom` row is the defect this plugin's presence bans.

## [06]-[RAIL_LAW]

- Owns: the build-time memoization-compilation pass (the `BabelPluginReactCompiler` default export), the `PluginOptions` config bag, the `"use memo"`/`"use no memo"` directive control, and the programmatic compile + `CompilerError`/`LoggerEvent` diagnostic API.
- Accept: the plugin wired FIRST in the bundler React Babel pass (`@vitejs/plugin-react`/`@rolldown/plugin-babel`) with `{ target: "19", compilationMode: "infer", panicThreshold: "none" }`; `"use no memo"` as a rare, marked escape hatch; the `logger`/`noEmit` diagnostic path since Biome owns lint; `gating` only for a deliberate incremental rollout.
- Reject: importing this package from `ui` source (it is a build tool, never a runtime import); a `target` mismatched to the installed React (breaks `_c` resolution); hand-written `useMemo`/`useCallback`/`memo` in `ui` (memoization is compiled); a standing `"use no memo"` (a defect marker, not a pattern); relying on an ESLint react-compiler rule (Biome is the linter — use `logger`/`noEmit`).
- Boundary: build-time only — nothing ships to the browser but `_c` calls into the compiler runtime (and, in dev, the `environment`-flag-gated `$dispatcherGuard`/`$structuralCheck`/`useRenderCounter` validators). `react/compiler-runtime` (built into React 19) provides `_c`; `react-compiler-runtime` is the sub-19 fallback + dev-validator lane. The `types` field is absent from `package.json` but `dist/index.d.ts` is present.
