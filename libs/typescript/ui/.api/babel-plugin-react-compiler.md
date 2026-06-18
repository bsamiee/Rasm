# [API_CATALOGUE] babel-plugin-react-compiler

`babel-plugin-react-compiler` is the Babel transform that runs the React Compiler over component and hook functions. It exposes the plugin default export for Vite/Babel integration, `PluginOptions` for configuration, and a set of compiler internals (`compileFn`, `compileProgram`, diagnostic classes, enums) that advanced instrumentation consumers reference directly.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `babel-plugin-react-compiler`
- package: `babel-plugin-react-compiler`
- module: `babel-plugin-react-compiler`
- asset: Babel plugin transform, compiler pipeline, diagnostic types, `PluginOptions`
- rail: build

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin configuration family
- rail: build

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [RAIL]                                                                              |
| :-----: | :------------------------- | :---------------- | :---------------------------------------------------------------------------------- |
|   [1]   | `PluginOptions`            | config record     | top-level plugin options                                                            |
|   [2]   | `ParsedPluginOptions`      | normalized config | `Required<Omit<PluginOptions,'environment'>> & { environment: EnvironmentConfig }`  |
|   [3]   | `EnvironmentConfig`        | env config        | `z.infer<typeof EnvironmentConfigSchema>`                                           |
|   [4]   | `PartialEnvironmentConfig` | partial env       | `Partial<EnvironmentConfig>`                                                        |
|   [5]   | `CompilationMode`          | mode enum         | `"syntax" \| "infer" \| "annotation" \| "all"`                                      |
|   [6]   | `CompilerReactTarget`      | target union      | `"17" \| "18" \| "19" \| { kind: "donotuse_meta_internal"; runtimeModule: string }` |
|   [7]   | `PanicThresholdOptions`    | panic mode        | `"none" \| "all_errors" \| "critical_errors"`                                       |
|   [8]   | `ExternalFunction`         | import ref        | `{ source: string; importSpecifierName: string }`                                   |

[PUBLIC_TYPE_SCOPE]: diagnostic family
- rail: build

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]    | [RAIL]                                             |
| :-----: | :---------------------------- | :--------------- | :------------------------------------------------- |
|   [1]   | `CompilerError`               | error class      | aggregates `CompilerErrorDetail` list              |
|   [2]   | `CompilerErrorDetail`         | detail class     | single error with `reason`, `loc`, `category`      |
|   [3]   | `CompilerDiagnostic`          | diagnostic class | single diagnostic with `severity`                  |
|   [4]   | `ErrorCategory`               | category enum    | `Hooks \| Immutability \| Refs \| ...`             |
|   [5]   | `ErrorSeverity`               | severity enum    | `Error \| Warning \| Hint \| Off`                  |
|   [6]   | `CompilerSuggestionOperation` | op enum          | `InsertBefore \| InsertAfter \| Remove \| Replace` |
|   [7]   | `SourceLocation`              | location union   | `t.SourceLocation \| typeof GeneratedSource`       |

[PUBLIC_TYPE_SCOPE]: logger and pipeline family
- rail: build

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                                   |
| :-----: | :---------------------- | :-------------- | :------------------------------------------------------- |
|   [1]   | `Logger`                | logger contract | `{ logEvent; debugLogIRs? }`                             |
|   [2]   | `LoggerEvent`           | event union     | compile success/error/skip/timing/autodeps               |
|   [3]   | `CompileSuccessEvent`   | success event   | `kind:"CompileSuccess"` + memo stats                     |
|   [4]   | `CompileErrorEvent`     | error event     | `kind:"CompileError"` + detail                           |
|   [5]   | `CompilerPipelineValue` | pipeline value  | `ast \| hir \| reactive \| debug`                        |
|   [6]   | `LintRule`              | lint rule       | `{ category; severity; name; description; recommended }` |

[PUBLIC_TYPE_SCOPE]: IR enums
- rail: build

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                                                               |
| :-----: | :------------ | :------------ | :------------------------------------------------------------------- |
|   [1]   | `Effect`      | effect enum   | `Unknown \| Freeze \| Read \| Capture \| Mutate \| Store \| ...`     |
|   [2]   | `ValueKind`   | value enum    | `MaybeFrozen \| Frozen \| Primitive \| Global \| Mutable \| Context` |
|   [3]   | `ValueReason` | reason enum   | `Global \| JsxCaptured \| HookCaptured \| State \| ...`              |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin integration
- rail: build

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY]   | [RAIL]                                     |
| :-----: | :--------------------------------------------- | :--------------- | :----------------------------------------- |
|   [1]   | `default` (`BabelPluginReactCompiler(_babel)`) | babel plugin     | `(_babel: typeof BabelCore) => PluginObj`  |
|   [2]   | `parsePluginOptions(obj)`                      | option parser    | validates + returns `ParsedPluginOptions`  |
|   [3]   | `validateEnvironmentConfig(partialConfig)`     | env validator    | returns `EnvironmentConfig`                |
|   [4]   | `compileProgram(program, pass)`                | program pass     | runs full compiler pass over a `t.Program` |
|   [5]   | `compile` (`compileFn`)                        | function compile | compiles a single function node            |

[ENTRYPOINT_SCOPE]: directive utilities
- rail: build

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :---------------------------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `findDirectiveEnablingMemoization(directives, opts)`  | opt-in finder  | returns matching `t.Directive \| null` |
|   [2]   | `findDirectiveDisablingMemoization(directives, opts)` | opt-out finder | returns `t.Directive \| null`          |
|   [3]   | `OPT_IN_DIRECTIVES`                                   | directive set  | `Set<string>` of opt-in strings        |
|   [4]   | `OPT_OUT_DIRECTIVES`                                  | directive set  | `Set<string>` of opt-out strings       |

[ENTRYPOINT_SCOPE]: debug and test utilities
- rail: build

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :----------------------------------------------------------------- | :------------- | :---------------------------------- |
|   [1]   | `printFunctionWithOutlined(fn)`                                    | debug print    | HIR function text dump              |
|   [2]   | `printHIR(ir, options?)`                                           | debug print    | HIR block dump                      |
|   [3]   | `printReactiveFunction(fn)`                                        | debug print    | reactive function text dump         |
|   [4]   | `printReactiveFunctionWithOutlined(fn)`                            | debug print    | reactive + outlined text dump       |
|   [5]   | `runBabelPluginReactCompiler(text, file, lang, opts, includeAst?)` | standalone     | runs compiler outside Babel         |
|   [6]   | `parseConfigPragmaForTests(pragma, defaults)`                      | test util      | parses `@compilationMode` pragma    |
|   [7]   | `LintRules`                                                        | rule catalog   | `Array<LintRule>` of all lint rules |

## [4]-[IMPLEMENTATION_LAW]

[BUILD_TOPOLOGY]:
- The default export is a Babel plugin factory; pass it directly in `plugins` arrays for Vite/Babel config
- `PluginOptions.environment` is `Partial<EnvironmentConfig>` — unset fields default via Zod schema defaults
- `compilationMode: "all"` compiles all eligible functions; `"annotation"` requires `"use memo"` directive
- `target` selects the runtime module path: `"19"` uses React 19 built-in cache slots; `"donotuse_meta_internal"` with `runtimeModule` redirects to a custom runtime (e.g. `react-compiler-runtime`)
- `panicThreshold` controls whether compiler errors abort the build: `"none"` emits diagnostics and skips; `"critical_errors"` aborts on invariant failures
- `logger.logEvent` receives every `LoggerEvent`; attach for telemetry or CI diagnostics

[LOCAL_ADMISSION]:
- Consume only `PluginOptions`, `EnvironmentConfig`, `Logger`, and the default plugin export for Vite/Babel integration; internal IR types (`HIRFunction`, `Place`, `Identifier`, etc.) are not part of the consuming surface.
- `parsePluginOptions` validates and normalizes raw config objects; never construct `ParsedPluginOptions` manually.

[RAIL_LAW]:
- Package: `babel-plugin-react-compiler`
- Owns: React Compiler Babel transform, plugin options, compiler diagnostics, IR debug utilities
- Accept: default export as Babel plugin, `PluginOptions` for configuration
- Reject: direct instantiation of `Environment`, `ProgramContext`, or IR node types outside compiler tooling
