# [API_CATALOGUE] babel-plugin-react-compiler

`babel-plugin-react-compiler` is the Babel transform that runs the React Compiler over component and hook functions, auto-memoizing them so the `ui` stack never hand-writes `useMemo`/`useCallback`/`React.memo`. Its consuming surface is small and build-time: the `default` export (a Babel plugin factory) plus `PluginOptions` configure it inside the Vite/Babel pipeline; `parsePluginOptions`/`validateEnvironmentConfig` normalize raw config through the Zod `EnvironmentConfig` schema. Behind that sits a large compiler pipeline — `compile`/`compileProgram`/`runBabelPluginReactCompiler` drivers, the `CompilerError`/`CompilerDiagnostic` classes, the `ErrorCategory`/`ErrorSeverity`/`Effect`/`ValueKind`/`ValueReason` IR enums, `LintRules`, and HIR/reactive debug printers — the advanced-instrumentation surface a CI or lint integration touches; the IR node types themselves (`HIRFunction`, `Place`, `Environment`) are internal and never consumed. In `ui` it has no design-page code fence: it is the `[REACT]`-group build plugin wired into the Vite `@vitejs/plugin-react` `babel.plugins` array, `target: "19"` binding React 19's built-in `c` cache while the sibling `react-compiler-runtime` serves the `target.runtimeModule` redirect for older runtimes. It underwrites the whole library's zero-manual-memoization posture — the `binding/atom` spine and `interaction/*` role owners stay declarative because the compiler owns memoization. Directly instantiating `Environment`/`ProgramContext` or an IR node in application code is the named defect.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `babel-plugin-react-compiler`
- package: `babel-plugin-react-compiler` (1.0.0, MIT; © Meta Platforms)
- module format: CJS `dist/index.js` (`main`), single bundled `dist/index.d.ts`; `default` export is the plugin factory; self-typed, no `@types`
- reflected: TSDECL — `node_modules/babel-plugin-react-compiler/dist/index.d.ts`
- runtime target: build-time only — runs inside Babel; peers `@babel/core`/`@babel/types`/`@babel/traverse` and `zod` (the `EnvironmentConfig`/`CompilerReactTarget`/`PanicThresholdOptions` schemas are `z.infer`, so config is validated + defaulted at parse)
- ABI: the exported enum `Effect` (IR mutation-effect kind) is NOT Effect-TS — alias on the rare cross-import; the internal `Result<T,E>` returned by `CompilerError.asResult()`/`findDirectiveEnablingMemoization` is a Rust-style monad, distinct from Effect `Result`, adapted at any boundary; `CompilationMode`/`CompilerReactTarget`/`PanicThresholdOptions`/`ParsedPluginOptions` are NOT importable names — they are the field value-spaces of `PluginOptions`/the `parsePluginOptions` return
- consumer: no `.planning` fence — the `ui` `[REACT]` Vite build plugin (`babel.plugins: [[default, PluginOptions]]`, `target: "19"`); sibling `react-compiler-runtime` serves the `target.runtimeModule` redirect
- rail: build / react-compiler

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin configuration family
- rail: build

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                                                    |
| :-----: | :-------------------- | :------------ | :---------------------------------------------------------------------------------------------- |
|  [01]   | `PluginOptions`       | `Partial<{}>` | top-level config — `environment`/`logger`/`gating`/`dynamicGating`/`panicThreshold`/`noEmit`/`compilationMode`/`target`/`sources`/`eslintSuppressionRules`/`flowSuppressions`/`ignoreUseNoForget`/`customOptOutDirectives`/`enableReanimatedCheck` |
|  [02]   | `EnvironmentConfig`   | type          | `z.infer<EnvironmentConfigSchema>` — the ~50-flag inner tuning record (`validate*`/`enable*` passes, `customHooks`, `inferEffectDependencies`) |
|  [03]   | `ExternalFunction`    | type          | `{source: string; importSpecifierName: string}` — the `gating`/emit-hook import ref             |
|  [04]   | `Hook`                | type          | `z.infer<HookSchema>` — a `customHooks` entry (`effectKind`/`valueKind`/`noAlias`/`transitiveMixedData`) |
|  [05]   | `compilationMode` space | union (field) | `"syntax" \| "infer" \| "annotation" \| "all"` — `PluginOptions.compilationMode` values (not an export) |
|  [06]   | `target` space        | union (field) | `"17" \| "18" \| "19" \| {kind: "donotuse_meta_internal"; runtimeModule: string}` — `PluginOptions.target` values (not an export) |
|  [07]   | `panicThreshold` space | union (field) | `"none" \| "all_errors" \| "critical_errors"` — abort policy (not an export)                    |
|  [08]   | `ParsedPluginOptions` (return only) | type | `Required<Omit<PluginOptions,'environment'>> & {environment: EnvironmentConfig}` — the `parsePluginOptions` result, not an importable name |

[PUBLIC_TYPE_SCOPE]: diagnostic family
- rail: build

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]    | [CAPABILITY]                                                    |
| :-----: | :---------------------------- | :--------------- | :------------------------------------------------------------- |
|  [01]   | `CompilerError`               | class (`Error`)  | aggregates `CompilerErrorDetail \| CompilerDiagnostic`; statics `invariant`/`throwDiagnostic`/`throwTodo`/`throwInvalidJS`/`throwInvalidReact`/`throwInvalidConfig`; `asResult(): Result<void, CompilerError>`, `merge`/`push`/`hasErrors`/`hasWarning`/`hasHints` |
|  [02]   | `CompilerDiagnostic`          | class            | one diagnostic — static `create`, `withDetails`, `severity`/`category`/`suggestions` getters, `printErrorMessage` |
|  [03]   | `CompilerErrorDetail`         | class            | one legacy error detail — `reason`/`loc`/`category`/`severity`  |
|  [04]   | `ErrorCategory`               | enum             | the 26-member lint taxonomy — `Hooks`/`Immutability`/`Refs`/`Globals`/`Purity`/`RenderSetState`/`EffectDependencies`/… |
|  [05]   | `ErrorSeverity`               | enum             | `Error \| Warning \| Hint \| Off`                              |
|  [06]   | `CompilerSuggestionOperation` | enum             | `InsertBefore(0) \| InsertAfter(1) \| Remove(2) \| Replace(3)` — codemod op |
|  [07]   | `CompilerDiagnosticOptions` / `CompilerDiagnosticDetail` / `CompilerErrorDetailOptions` | type | diagnostic construction/detail shapes |
|  [08]   | `SourceLocation`              | union            | `t.SourceLocation \| typeof GeneratedSource`                   |

[PUBLIC_TYPE_SCOPE]: logger, lint, and IR-instrumentation family
- rail: build

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                                   |
| :-----: | :---------------------- | :-------------- | :------------------------------------------------------------ |
|  [01]   | `Logger`                | type            | `{logEvent(filename: string \| null, event: LoggerEvent): void; debugLogIRs?(v: CompilerPipelineValue): void}` |
|  [02]   | `LoggerEvent`           | union           | `CompileSuccess \| CompileError \| CompileDiagnostic \| CompileSkip \| PipelineError \| Timing \| AutoDepsDecorations \| AutoDepsEligible` — `CompileSuccess` carries `memoSlots`/`memoBlocks`/`memoValues`/`prunedMemo*` stats |
|  [03]   | `CompilerPipelineValue` | union           | `{kind: "ast" \| "hir" \| "reactive" \| "debug"}` — the `debugLogIRs` payload |
|  [04]   | `LintRule` / `LintRules` | type / const   | `{category: ErrorCategory; severity; name; description; recommended}`; `LintRules: Array<LintRule>` — the full rule catalog for an ESLint integration |
|  [05]   | `Effect`                | enum (IR)       | mutation-effect kind — `Unknown \| Freeze \| Read \| Capture \| Mutate \| Store \| ConditionallyMutate…` (NOT Effect-TS) |
|  [06]   | `ValueKind`             | enum (IR)       | `MaybeFrozen \| Frozen \| Primitive \| Global \| Mutable \| Context` |
|  [07]   | `ValueReason`           | enum (IR)       | `Global \| JsxCaptured \| HookCaptured \| HookReturn \| State \| ReducerState \| Context \| …` |
|  [08]   | `ProgramContext`        | class           | per-program compile context — import management, hook detection; constructed by the pipeline, not application code |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin integration and configuration
- rail: build

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY]  | [CAPABILITY]                                               |
| :-----: | :--------------------------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `default` (`BabelPluginReactCompiler(_babel: typeof BabelCore)`) | babel plugin | `=> PluginObj`; drop into a `plugins` array (Vite/Babel) |
|  [02]   | `parsePluginOptions(obj: unknown): ParsedPluginOptions` | option parser | validate + normalize raw config; never build `ParsedPluginOptions` by hand |
|  [03]   | `validateEnvironmentConfig(partial): EnvironmentConfig` | env validator | apply Zod defaults over a `PartialEnvironmentConfig`      |

[ENTRYPOINT_SCOPE]: compile drivers (advanced instrumentation)
- rail: build

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `compileProgram(program, pass): CompileProgramMetadata \| null`    | program pass   | run the full compiler over a `t.Program`                 |
|  [02]   | `compile` (`compileFn(func, config, fnType, mode, programContext, logger, filename, code)`) | function compile | compile one function node → `CodegenFunction` |
|  [03]   | `runBabelPluginReactCompiler(text, file, language: 'flow' \| 'typescript', options: PluginOptions \| null, includeAst?)` | standalone | run the compiler outside a Babel host → `BabelFileResult` |

[ENTRYPOINT_SCOPE]: directive utilities and lint catalog
- rail: build

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `findDirectiveEnablingMemoization(directives, opts: ParsedPluginOptions): Result<t.Directive \| null, CompilerError>` | opt-in finder | Result-wrapped `"use memo"` directive lookup |
|  [02]   | `findDirectiveDisablingMemoization(directives, {customOptOutDirectives}): t.Directive \| null` | opt-out finder | `"use no memo"` / custom opt-out directive lookup |
|  [03]   | `OPT_IN_DIRECTIVES` / `OPT_OUT_DIRECTIVES`                                             | directive set  | `Set<string>` of recognized opt-in/opt-out strings       |

[ENTRYPOINT_SCOPE]: debug and test utilities
- rail: build

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `printHIR(ir, options?)` / `printFunctionWithOutlined(fn)`                   | debug print    | HIR block / function text dump                 |
|  [02]   | `printReactiveFunction(fn)` / `printReactiveFunctionWithOutlined(fn)`        | debug print    | reactive-function text dump                    |
|  [03]   | `parseConfigPragmaForTests(pragma, defaults): PluginOptions`                 | test util      | parse a `@compilationMode`/flag test pragma    |

## [04]-[IMPLEMENTATION_LAW]

[BUILD_TOPOLOGY]:
- The `default` export is a Babel plugin factory — pass it directly in a `plugins` array; the React Compiler runs first in the transform so downstream Babel/Vite passes see already-memoized output.
- `PluginOptions.environment` is `Partial<EnvironmentConfig>`; unset flags default through the Zod schema. `parsePluginOptions(raw)` is the one validated entry — it resolves `environment` to a full `EnvironmentConfig` and fills every other field, returning `ParsedPluginOptions` (never construct that by hand).
- `compilationMode: "all"` compiles all eligible functions; `"annotation"` requires a `"use memo"` directive; `"infer"`/`"syntax"` gate by inference/syntax heuristics.
- `target` selects the runtime module: `"19"` uses React 19's built-in `c` cache; `{kind: "donotuse_meta_internal", runtimeModule: "react-compiler-runtime"}` redirects emitted cache calls to the sibling runtime for React 17/18.
- `panicThreshold` controls abort behaviour: `"none"` emits diagnostics and skips the function; `"critical_errors"` aborts on invariant failures; `"all_errors"` aborts on any.

[STACKING_LAW]:
- This is a build-time plugin, never an Effect runtime lib: it composes into the `ui` Vite config's `@vitejs/plugin-react` `babel.plugins` (`[[BabelPluginReactCompiler, { target: "19", … } satisfies PluginOptions]]`), not into any `Layer` or `Effect`.
- The sibling `react-compiler-runtime` is the `target.runtimeModule` counterpart — one catalog pair: this plugin emits cache/guard calls, that package resolves them when React < 19 lacks the built-in `c`.
- Auto-memoization is the load-bearing contract for the rest of `ui`: because the compiler owns memoization, the `binding/atom` `AtomBinding` spine, the `interaction/role` owners, and the `render/*` surfaces stay declarative and never hand-write `useMemo`/`useCallback`/`React.memo` — the named anti-pattern the compiler exists to remove.
- The `Logger` hook (`logEvent(filename, event)`) is the CI/telemetry seam: route `LoggerEvent` (esp. `CompileSuccess` memo stats and `CompileError`/`CompileSkip`) to a build report or an OTel span in the build pipeline; `LintRules` + `ErrorCategory` feed a paired ESLint `eslint-plugin-react-hooks` integration. Both are build-plane, decoupled from the app's `@effect/opentelemetry` runtime tracer.

[RAIL_LAW]:
- Package: `babel-plugin-react-compiler`
- Owns: the React Compiler Babel transform, plugin/environment configuration, compiler diagnostics, the lint-rule catalog, and IR debug/instrumentation utilities
- Accept: the `default` export as a Babel plugin; `PluginOptions` (validated via `parsePluginOptions`) for configuration; `Logger`/`LoggerEvent`/`LintRules` for build telemetry; `target: "19"` with `react-compiler-runtime` as the sub-19 redirect
- Reject: direct instantiation of `Environment`/`ProgramContext` or any IR node in application code; hand-constructing `ParsedPluginOptions`; treating the IR `Effect` enum as Effect-TS or the compiler's Rust-style `Result` as Effect `Result`; hand-written `useMemo`/`useCallback`/`React.memo` in `ui` (the compiler owns memoization); importing the non-exported `CompilationMode`/`CompilerReactTarget`/`PanicThresholdOptions` names (they are `PluginOptions` field value-spaces)
