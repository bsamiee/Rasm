# [API_CATALOGUE] effect-tsgo

Grounded from the installed package (`@effect/tsgo` 0.14.3, binary self-report `tsgo v0.14.3`). `@effect/tsgo` is a CLI tool, not a programmatic library: the package ships a single bundled executable `dist/effect-tsgo.js` (`#!/usr/bin/env node`, ~9.5 MB rolldown bundle wrapping `node:child_process`) plus one platform-native Go binary resolved from the seven `optionalDependencies` (`@effect/tsgo-{win32,linux,darwin}-{x64,arm,arm64}`). `package.json` declares only `bin: { "effect-tsgo": "./dist/effect-tsgo.js" }` — there is no `main`, no `exports`, no `types`, and zero `.d.ts` declarations, so `assay api query --key @effect/tsgo` reflects `0 types across 0 namespaces`. The load-bearing public surface is therefore the CLI subcommand grammar, the editor-plugin name + plugin-options schema it configures, and the diagnostic / refactor / completion / codegen / rename rule catalog the embedded Effect Language Service enforces. It targets Effect V4 (codename "smol") primarily and Effect V3 secondarily, and is a superset of Microsoft's TypeScript-Go (`tsgo`): a pinned upstream `tsgo` commit plus the Effect language-service layer. It replaces `tsgo`; running both produces duplicate diagnostics.

---

## [1] — CLI surface (`effect-tsgo`)

The package is invoked as `npx @effect/tsgo <subcommand>` or via the `effect-tsgo` bin. The argv parser self-presents as `tsgo`.

```text
USAGE
  tsgo <subcommand> [flags]

GLOBAL FLAGS
  --help, -h              Show help information
  --version               Show version information
  --completions choice    Print shell completion script (choices: bash, zsh, fish, sh)
  --log-level choice      Sets the minimum log level
                          (choices: all, trace, debug, info, warn, warning, error, fatal, none)

SUBCOMMANDS
  patch           Patch the Effect Language Service binary
  unpatch         Unpatch and restore the original TypeScript-Go binary
  get-exe-path    Print the Effect Language Service executable path
  setup           Setup @effect/tsgo for the given project using an interactive CLI
  config          Configure diagnostic severities for an existing tsconfig using the interactive rule picker
```

Subcommand semantics (no subcommand-local flags beyond the global set):
- `setup` — interactive bootstrap: adds the `@effect/tsgo` dependency, configures `tsconfig.json` to load the `@effect/language-service` plugin, adjusts plugin options, and emits editor-configuration hints. (`npx @effect/tsgo setup`.)
- `config` — interactive rule picker that edits `diagnosticSeverity` overrides in an existing `tsconfig.json`.
- `patch` / `unpatch` — install/remove the Effect language-service patch over the embedded native TypeScript-Go binary.
- `get-exe-path` — prints the resolved Effect Language Service executable path (the platform-native binary), for editor `typescript.tsserver`/native-preview wiring.

Companion install note: the native standard TypeScript install (`@typescript/native-preview`) is still required alongside `@effect/tsgo` at this alpha stage.

## [2] — Editor plugin + options schema

`setup` wires a `compilerOptions.plugins[]` entry named `@effect/language-service` into `tsconfig.json`. The plugin runs inside the language server and is configured by the option object below. Every knob with its default:

```jsonc
{
  "compilerOptions": {
    "plugins": [
      {
        "name": "@effect/language-service",
        "refactors": true,                        // enable Effect refactors
        "diagnostics": true,                       // enable Effect diagnostics
        "includeSuggestionsInTsc": true,           // include suggestion-level diagnostics in tsc CLI output
        "quickinfo": true,                         // enable Effect quickinfo (hover)
        "completions": true,                       // enable Effect completions
        "debug": false,                            // additional debug-only language-service output
        "goto": true,                              // enable Effect goto-references support
        "renames": true,                           // enable Effect rename helpers
        "ignoreEffectSuggestionsInTscExitCode": true,  // suggestion diagnostics do not affect tsc exit code
        "ignoreEffectWarningsInTscExitCode": false,    // warning diagnostics do not affect tsc exit code
        "ignoreEffectErrorsInTscExitCode": false,      // error diagnostics do not affect tsc exit code
        "skipDisabledOptimization": false,         // still process disabled diagnostics so directives can re-enable them
        "mermaidProvider": "mermaid.live",         // layer-graph render service: mermaid.live | mermaid.com | custom URL
        "noExternal": false,                       // suppress external Mermaid links in hover output
        "layerGraphFollowDepth": 0,                // depth the layer-graph extraction follows symbol references
        "inlays": false,                           // suppress redundant return-type inlay hints on Effect generators
        "namespaceImportPackages": [],             // packages preferring namespace imports
        "barrelImportPackages": [],                // packages preferring barrel named imports
        "importAliases": {},                       // package-level import aliases keyed by package name
        "topLevelNamedReexports": "ignore",        // whether named reexports are followed at package top level
        "extendedKeyDetection": false,             // match constructors with @effect-identifier annotations
        "pipeableMinArgCount": 2,                  // min contiguous pipeable transforms to trigger missedPipeableOpportunity
        "allowedDuplicatedPackages": [],           // packages allowed multiple versions without duplicatePackage
        "effectFn": ["span"],                      // effectFnOpportunity quickfix variants offered
        "diagnosticSeverity": {},                  // rule-name -> severity map; {} enables a rule at its default severity
        "keyPatterns": [                           // deterministicKeys key-pattern formulas
          { "target": "service", "pattern": "default", "skipLeadingPath": ["src/"] },
          { "target": "custom",  "pattern": "default", "skipLeadingPath": ["src/"] }
        ],
        "overrides": [                             // ordered per-file diagnostic option overrides
          { "include": ["src/**/*.ts"], "options": { "diagnosticSeverity": { "floatingEffect": "error" } } }
        ]
      }
    ]
  }
}
```

Per-rule severity values for `diagnosticSeverity` / `overrides[].options.diagnosticSeverity`: `off`, `message`, `suggestion`, `warning`, `error` (the severity vocabulary mirrored by the rule-catalog markers below). Source directives also drive severity at call sites: `@effect-diagnostics` (file-scope) and `@effect-diagnostics-next-line` (line-scope) comment directives toggle/re-enable individual rules, and `@effect-codegens` selects codegen targets.

## [3] — Diagnostics catalog

Each rule fires under the language server and `tsc` (subject to the exit-code knobs). Severity marker key: `off` = off by default, `error`, `warning`, `message`, `suggestion`; `fix` = quick fix available. `v3`/`v4` mark Effect-version applicability. Rules are grouped by the package's own four categories.

[CORRECTNESS] — wrong, unsafe, or structurally invalid patterns:

| Rule                          | Sev     | Fix | v3  | v4  |
| ----------------------------- | ------- | --- | --- | --- |
| `anyUnknownInErrorContext`    | off     |     | ✓   | ✓   |
| `classSelfMismatch`           | error   | fix | ✓   | ✓   |
| `duplicatePackage`            | warning |     | ✓   | ✓   |
| `effectFnImplicitAny`         | error   |     | ✓   | ✓   |
| `floatingEffect`              | error   |     | ✓   | ✓   |
| `genericEffectServices`       | warning |     | ✓   | ✓   |
| `missingEffectContext`        | error   |     | ✓   | ✓   |
| `missingEffectError`          | error   | fix | ✓   | ✓   |
| `missingLayerContext`         | error   |     | ✓   | ✓   |
| `missingReturnYieldStar`      | error   | fix | ✓   | ✓   |
| `missingStarInYieldEffectGen` | error   | fix | ✓   | ✓   |
| `nonObjectEffectServiceType`  | error   |     | ✓   |     |
| `outdatedApi`                 | warning |     |     | ✓   |
| `overriddenSchemaConstructor` | error   | fix | ✓   | ✓   |

[ANTI_PATTERN] — discouraged patterns that lead to bugs or confusing behavior:

| Rule                            | Sev        | Fix | v3  | v4  |
| ------------------------------- | ---------- | --- | --- | --- |
| `catchUnfailableEffect`         | suggestion |     | ✓   | ✓   |
| `effectFnIife`                  | warning    | fix | ✓   | ✓   |
| `effectGenUsesAdapter`          | warning    |     | ✓   | ✓   |
| `effectInFailure`               | warning    |     | ✓   | ✓   |
| `effectInVoidSuccess`           | warning    |     | ✓   | ✓   |
| `globalErrorInEffectCatch`      | warning    |     | ✓   | ✓   |
| `globalErrorInEffectFailure`    | warning    |     | ✓   | ✓   |
| `layerMergeAllWithDependencies` | warning    | fix | ✓   | ✓   |
| `lazyEffect`                    | suggestion |     |     | ✓   |
| `lazyPromiseInEffectSync`       | warning    |     | ✓   | ✓   |
| `leakingRequirements`           | suggestion |     | ✓   | ✓   |
| `multipleEffectProvide`         | warning    | fix | ✓   | ✓   |
| `returnEffectInGen`             | suggestion | fix | ✓   | ✓   |
| `runEffectInsideEffect`         | suggestion | fix | ✓   | ✓   |
| `schemaSyncInEffect`            | suggestion |     | ✓   |     |
| `scopeInLayerEffect`            | warning    | fix | ✓   |     |
| `strictEffectProvide`           | off        |     | ✓   | ✓   |
| `tryCatchInEffectGen`           | suggestion |     | ✓   | ✓   |
| `unknownInEffectCatch`          | warning    |     | ✓   | ✓   |

[EFFECT_NATIVE] — prefer Effect-native APIs over global/host primitives:

| Rule                        | Sev | Fix | v3  | v4  |
| --------------------------- | --- | --- | --- | --- |
| `asyncFunction`             | off |     | ✓   | ✓   |
| `cryptoRandomUUID`          | off |     |     | ✓   |
| `cryptoRandomUUIDInEffect`  | off |     |     | ✓   |
| `extendsNativeError`        | off |     | ✓   | ✓   |
| `globalConsole`             | off |     | ✓   | ✓   |
| `globalConsoleInEffect`     | off |     | ✓   | ✓   |
| `globalDate`                | off |     | ✓   | ✓   |
| `globalDateInEffect`        | off |     | ✓   | ✓   |
| `globalFetch`               | off |     | ✓   | ✓   |
| `globalFetchInEffect`       | off |     | ✓   | ✓   |
| `globalRandom`              | off |     | ✓   | ✓   |
| `globalRandomInEffect`      | off |     | ✓   | ✓   |
| `globalTimers`              | off |     | ✓   | ✓   |
| `globalTimersInEffect`      | off |     | ✓   | ✓   |
| `instanceOfSchema`          | off | fix | ✓   | ✓   |
| `newPromise`                | off |     | ✓   | ✓   |
| `nodeBuiltinImport`         | off |     | ✓   | ✓   |
| `preferSchemaOverJson`      | off |     | ✓   | ✓   |
| `processEnv`                | off |     | ✓   | ✓   |
| `processEnvInEffect`        | off |     | ✓   | ✓   |
| `unsafeEffectTypeAssertion` | off | fix | ✓   | ✓   |

[STYLE] — cleanup, consistency, idiomatic Effect:

| Rule                             | Sev        | Fix | v3  | v4  |
| -------------------------------- | ---------- | --- | --- | --- |
| `catchAllToMapError`             | suggestion | fix | ✓   | ✓   |
| `catchToOrElseSucceed`           | suggestion | fix | ✓   | ✓   |
| `deterministicKeys`              | off        | fix | ✓   | ✓   |
| `effectDoNotation`               | off        |     | ✓   | ✓   |
| `effectFnOpportunity`            | suggestion | fix | ✓   | ✓   |
| `effectMapFlatten`               | suggestion |     | ✓   | ✓   |
| `effectMapVoid`                  | suggestion | fix | ✓   | ✓   |
| `effectSucceedWithVoid`          | suggestion | fix | ✓   | ✓   |
| `missedPipeableOpportunity`      | off        | fix | ✓   | ✓   |
| `missingEffectServiceDependency` | off        |     | ✓   |     |
| `multipleCatchTag`               | suggestion |     |     | ✓   |
| `nestedEffectGenYield`           | off        |     | ✓   | ✓   |
| `newSchemaClass`                 | off        | fix |     | ✓   |
| `redundantMapError`              | suggestion |     | ✓   | ✓   |
| `redundantOrDie`                 | suggestion |     | ✓   | ✓   |
| `redundantSchemaTagIdentifier`   | suggestion | fix | ✓   | ✓   |
| `schemaNumber`                   | suggestion | fix |     | ✓   |
| `schemaStructWithTag`            | suggestion | fix | ✓   | ✓   |
| `schemaUnionOfLiterals`          | off        | fix | ✓   |     |
| `serviceNotAsClass`              | off        | fix |     | ✓   |
| `strictBooleanExpressions`       | off        |     | ✓   | ✓   |
| `unnecessaryArrowBlock`          | off        | fix | ✓   | ✓   |
| `unnecessaryEffectGen`           | suggestion | fix | ✓   | ✓   |
| `unnecessaryFailYieldableError`  | suggestion | fix | ✓   | ✓   |
| `unnecessaryPipe`                | suggestion | fix | ✓   | ✓   |
| `unnecessaryPipeChain`           | suggestion | fix | ✓   | ✓   |
| `unnecessaryTypeofType`          | suggestion | fix | ✓   | ✓   |

## [4] — Refactors / completions / codegens / renames

Editor refactors (offered through the language-service refactor menu; ✓ = available, off = not yet implemented):

| Refactor                     | v3  | v4  | Action                                                       |
| ---------------------------- | --- | --- | ------------------------------------------------------------ |
| `asyncAwaitToFn`             | ✓   | ✓   | async/await -> `Effect.fn`                                   |
| `asyncAwaitToFnTryPromise`   | ✓   | ✓   | async/await -> `Effect.fn` with Error ADT + `tryPromise`     |
| `asyncAwaitToGen`            | ✓   | ✓   | async/await -> `Effect.gen`                                  |
| `asyncAwaitToGenTryPromise`  | ✓   | ✓   | async/await -> `Effect.gen` with Error ADT + `tryPromise`    |
| `debugPerformance`           | off | off | insert performance timing debug comments                     |
| `effectGenToFn`              | ✓   | ✓   | `Effect.gen` -> `Effect.fn`                                  |
| `functionToArrow`            | ✓   | ✓   | function declaration -> arrow function                       |
| `layerMagic`                 | ✓   | ✓   | auto-compose layers with correct merge/provide               |
| `makeSchemaOpaque`           | ✓   | ✓   | Schema -> opaque type aliases                                |
| `makeSchemaOpaqueWithNs`     | ✓   | ✓   | Schema -> opaque types with namespace                        |
| `pipeableToDatafirst`        | ✓   | ✓   | pipeable calls -> data-first style                           |
| `removeUnnecessaryEffectGen` | ✓   | ✓   | remove redundant `Effect.gen` wrapper                        |
| `structuralTypeToSchema`     | ✓   | ✓   | type alias -> recursive Schema                               |
| `toggleLazyConst`            | ✓   | ✓   | toggle lazy/eager const declarations                         |
| `togglePipeStyle`            | ✓   | ✓   | toggle `pipe(x, f)` vs `x.pipe(f)`                           |
| `toggleReturnTypeAnnotation` | ✓   | ✓   | add/remove return type annotation                            |
| `toggleTypeAnnotation`       | ✓   | ✓   | add/remove variable type annotation                          |
| `typeToEffectSchema`         | ✓   | ✓   | type alias -> `Effect.Schema`                                |
| `typeToEffectSchemaClass`    | ✓   | ✓   | type alias -> `Schema.Class`                                 |
| `wrapWithEffectGen`          | ✓   | ✓   | wrap expression in `Effect.gen`                              |
| `wrapWithPipe`               | off | ✓   | wrap selection in `pipe(...)`                                |
| `writeTagClassAccessors`     | ✓   | off | generate static accessors for `Effect.Service`/`Tag` classes |

Completions (extends-clause / dot-access / comment snippets the language service offers):

| Completion                  | v3  | v4  | Trigger                                                                       |
| --------------------------- | --- | --- | ----------------------------------------------------------------------------- |
| `contextSelfInClasses`      | ✓   | off | `Context.Tag` self-type snippets in extends clauses (V3-only)                 |
| `effectDataClasses`         | ✓   | ✓   | Data class constructor snippets in extends clauses                            |
| `effectSchemaSelfInClasses` | ✓   | ✓   | Schema/Model class constructor snippets in extends clauses                    |
| `effectSelfInClasses`       | ✓   | off | `Effect.Service`/`Effect.Tag` self-type snippets in extends clauses (V3-only) |
| `genFunctionStar`           | ✓   | ✓   | `gen(function*(){})` snippet on dot-access of callable `.gen`                 |
| `effectCodegensComment`     | ✓   | ✓   | `@effect-codegens` directive snippet with codegen-name choices                |
| `effectDiagnosticsComment`  | ✓   | ✓   | `@effect-diagnostics` / `@effect-diagnostics-next-line` directive snippets    |
| `rpcMakeClasses`            | ✓   | off | `Rpc.make` constructor snippet in extends clauses (V3-only)                   |
| `schemaBrand`               | ✓   | off | `brand("varName")` snippet on Schema dot-access in var declarations (V3-only) |
| `serviceMapSelfInClasses`   | ✓   | ✓   | Service-map self-type snippets in extends clauses                             |

Codegens (comment-directive driven; all `off` / not yet implemented in 0.14.3): `accessors` (generate Service accessor methods), `annotate` (generate type annotations), `typeToSchema` (generate Schema from a type alias).

Renames (all `off` / not yet implemented in 0.14.3): `keyStrings` (extend rename to include key string literals in Effect classes).

## [5] — TIER / ADMISSION

- Role: a development / editor-tooling binary, not a runtime or bundle dependency. It owns no import surface; no application module imports `@effect/tsgo`.
- Nx module-boundary tag: none applies — there is no importable surface to tag `browser` / `node` / `neutral`. It sits in tooling/devDependency posture alongside `@typescript/native-preview`.
- Platform binary: resolved at install from one of seven `optionalDependencies`; the active host (`darwin-arm64`) pulls `@effect/tsgo-darwin-arm64`. Version pinning is single-binary — the embedded upstream `tsgo` commit is recorded in the package's `flake.nix` (`typescript-go-src`) and advances only with `@effect/tsgo` releases.
- Replaces (does not augment) `tsgo`: configure the editor to use `effect-tsgo` as the sole TypeScript language server to avoid duplicate diagnostics.

## [6] — GAPS

- `assay api query --key @effect/tsgo` reflects `0 types across 0 namespaces`: the package ships zero `.d.ts` declarations and no `main`/`exports`/`types`, so no programmatic type member is reflectable. Catalog above is grounded from `package.json`, the binary's `--help` self-report (subcommands + global flags, version `tsgo v0.14.3`), and the package `README.md` (plugin-options schema + rule/refactor/completion/codegen/rename status tables). The diagnostic/refactor enforcement logic lives inside the bundled `dist/effect-tsgo.js` (`#!/usr/bin/env node`, ~9.5 MB, references `node:child_process`) and the platform-native Go binary, neither of which exposes a typed surface. The seven platform package names are authoritative from `package.json` `optionalDependencies`; in the bundle they are constructed dynamically (no literal `@effect/tsgo-<os>-<arch>` strings to grep).
- Subcommand-local flags: `setup`, `config`, `patch`, `unpatch`, `get-exe-path` each report only the global flag set under `--help`; any interactive-prompt-only inputs (project path, rule selection) are runtime-interactive and not declared as flags.
- `diagnosticSeverity` accepted severity tokens (`off`/`message`/`suggestion`/`warning`/`error`) are inferred from the rule-catalog severity markers and option-comment wording; the binary's exact accepted enum is not reflectable from a typed declaration.
- The `keyPatterns[].pattern` formula vocabulary (beyond the `"default"` literal) and `topLevelNamedReexports` accepted values (beyond `"ignore"`) are not enumerated in the package README or any typed declaration.
