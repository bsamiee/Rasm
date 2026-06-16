# [API_CATALOGUE] effect-language-service

Grounded from the installed `node_modules/@effect/language-service` distribution (`@effect/language-service` 0.86.2). This package is a TypeScript **language-service plugin + CLI + `tsc` patcher**, not a runtime library: it ships only bundled JavaScript (`index.js`, `cli.js`, `transform.js`, `effect-lsp-patch-utils.js`) plus a JSON Schema (`schema.json`) and a `cli.js` bin (`effect-language-service`). There are **no `.d.ts` declarations and no `package.json` `exports`/`types`**, and `package.json` declares `"main": "index.cjs"` pointing at a file that is **not shipped** (the dir contains `index.js`, not `index.cjs`), so `require("@effect/language-service")` resolves nothing importable. Consequently `assay api query --key @effect/language-service` resolves the package (version `0.86.2`, `restored`) but reflects `0 types across 0 namespaces` — correctly, since there is no importable type surface. Nothing in the TS planning corpus imports a symbol from this package; it is a **build/editor tooling surface** whose contract is (1) the `tsconfig.json` plugin-options object, (2) the diagnostic-rule catalogue with per-rule default severities, (3) the in-source comment-directive language, and (4) the `effect-language-service` CLI command set. Those four surfaces are authoritative below from `schema.json` and `cli.js --help`; the runtime JS is intentionally not transcribed (no consumer composes it as a value).

---

## [1] — Admission posture (no value-import surface)

This package is admitted as **developer tooling**, not as a dependency any owner imports:

- It is configured, never imported. The only code-shaped touchpoint is the string literal `"@effect/language-service"` in a `tsconfig.json` `compilerOptions.plugins[]` entry, and `@effect-*` comment directives in `.ts` source.
- Its diagnostics surface in-editor (any TS-LSP editor) by default; they reach `tsc` build output only after the local `typescript` install is patched via `effect-language-service patch` (the patch rewrites `typescript.js` and `_tsc.js` in `node_modules/typescript/lib`). The patch is made durable across reinstalls by a `prepare` script running `effect-language-service patch`.
- It carries no Nx `@nx/enforce-module-boundaries` runtime tag (`browser` / `node` / `neutral`) because it produces no bundled value — it is a `devDependency` consumed by the TypeScript toolchain only. Monorepo guidance: install once at the repo root and configure once in the root `tsconfig.json` so every package inherits identical diagnostics; the LSP entry should be **last** in the `plugins[]` array (after e.g. a Svelte plugin) for correct LSP composition.

---

## [2]-[PLUGIN_OPTIONS]

Exact key set, types, and defaults from `schema.json` (`compilerOptionsDefinition.properties.compilerOptions.properties.plugins.items.anyOf[0]`). The object is the sole configuration contract; every key is optional except `name`.

```jsonc
{
  "$schema": "./node_modules/@effect/language-service/schema.json",
  "compilerOptions": {
    "plugins": [
      { "name": "@effect/language-service" } // must equal "@effect/language-service"; placed LAST in plugins[]
    ]
  }
}
```

Core plugin toggles and diagnostic output options share one object; every key is optional except `name`.

[CORE_OPTIONS]:

| [INDEX] | [KEY]             | [TYPE]     | [DEFAULT]        | [CONFIG_ROLE]         |
| :-----: | :---------------- | :--------- | :--------------- | :-------------------- |
|   [1]   | `name`            | `string`   | —                | plugin identity       |
|   [2]   | `refactors`       | `boolean`  | `true`           | refactor toggle       |
|   [3]   | `diagnostics`     | `boolean`  | `true`           | diagnostic toggle     |
|   [4]   | `diagnosticsName` | `boolean`  | `true`           | diagnostic labeling   |
|   [5]   | `noExternal`      | `boolean`  | `false`          | external-link policy  |
|   [6]   | `effectFn`        | `string[]` | `["span"]`       | `Effect.fn` shape set |
|   [7]   | `mermaidProvider` | `string`   | `"mermaid.live"` | diagram provider      |

[TSC_DIAGNOSTIC_OPTIONS]:

| [INDEX] | [KEY]                                  | [TYPE]     | [DEFAULT]   | [CONFIG_ROLE]             |
| :-----: | :------------------------------------- | :--------- | :---------- | :------------------------ |
|   [1]   | `missingDiagnosticNextLine`            | `Severity` | `"warning"` | unused directive severity |
|   [2]   | `includeSuggestionsInTsc`              | `boolean`  | `true`      | suggestion projection     |
|   [3]   | `ignoreEffectWarningsInTscExitCode`    | `boolean`  | `false`     | warning exit policy       |
|   [4]   | `ignoreEffectErrorsInTscExitCode`      | `boolean`  | `false`     | error exit policy         |
|   [5]   | `ignoreEffectSuggestionsInTscExitCode` | `boolean`  | `true`      | suggestion exit policy    |
|   [6]   | `diagnosticSeverity`                   | `Record`   | `{}`        | per-rule overrides        |

[EDITOR_FEATURE_OPTIONS]:

| [INDEX] | [KEY]                       | [TYPE]            | [DEFAULT]         | [CONFIG_ROLE]          |
| :-----: | :-------------------------- | :---------------- | :---------------- | :--------------------- |
|   [1]   | `quickinfo`                 | `boolean`         | `true`            | hover toggle           |
|   [2]   | `quickinfoEffectParameters` | `QuickinfoPolicy` | `"whenTruncated"` | hover parameter policy |
|   [3]   | `quickinfoMaximumLength`    | `number`          | `-1`              | hover length budget    |
|   [4]   | `completions`               | `boolean`         | `true`            | completion toggle      |
|   [5]   | `goto`                      | `boolean`         | `true`            | definition navigation  |
|   [6]   | `inlays`                    | `boolean`         | `true`            | inlay hint toggle      |
|   [7]   | `renames`                   | `boolean`         | `true`            | rename propagation     |
|   [8]   | `layerGraphFollowDepth`     | `number`          | `0`               | layer graph depth      |

[IMPORT_KEY_OPTIONS]:

| [INDEX] | [KEY]                       | [TYPE]         | [DEFAULT]  | [CONFIG_ROLE]               |
| :-----: | :-------------------------- | :------------- | :--------- | :-------------------------- |
|   [1]   | `skipDisabledOptimization`  | `boolean`      | `false`    | disabled-rule processing    |
|   [2]   | `extendedKeyDetection`      | `boolean`      | `false`    | custom key detection        |
|   [3]   | `allowedDuplicatedPackages` | `string[]`     | `[]`       | duplicate package allowlist |
|   [4]   | `namespaceImportPackages`   | `string[]`     | `[]`       | namespace import policy     |
|   [5]   | `topLevelNamedReexports`    | union          | `"ignore"` | re-export follow policy     |
|   [6]   | `barrelImportPackages`      | `string[]`     | `[]`       | barrel import policy        |
|   [7]   | `importAliases`             | `Record`       | `{}`       | import alias map            |
|   [8]   | `pipeableMinArgCount`       | `number`       | `2`        | pipeable diagnostic floor   |
|   [9]   | `keyPatterns`               | `KeyPattern[]` | see below  | deterministic key policy    |

`Severity` is `"off" | "error" | "warning" | "message" | "suggestion"`. `QuickinfoPolicy` is `"always" | "never" | "whenTruncated"`; the schema lowercases the last value to `whentruncated`. `topLevelNamedReexports` is `"ignore" | "follow"`. The `name` value must be `"@effect/language-service"`.

`includeSuggestionsInTsc` reports `suggestion` diagnostics as `[suggestion]`-prefixed `message` diagnostics when the `tsc` patch is active. The `ignoreEffect*InTscExitCode` keys prevent the matching Effect diagnostic severity from changing the `tsc` exit code when enabled. `extendedKeyDetection` requires `/** @effect-identifier */` on a custom identifier parameter; `effectFn` accepts `"untraced"`, `"span"`, `"suggested-span"`, `"inferred-span"`, and `"no-span"`.

Import-policy arrays may name packages such as `["effect", "@effect/*"]`; `topLevelNamedReexports: "follow"` rewrites top-level named re-exports to the module that publishes them, and `importAliases` maps package names to local aliases such as `{ "Array": "Arr" }`.

`keyPatterns[]` default: `[{ "target": "service", "pattern": "default", "skipLeadingPath": ["src/"] }, { "target": "custom", "pattern": "default", "skipLeadingPath": ["src/"] }]`. Each entry:

```jsonc
{
  "target": "service" | "error" | "custom",      // key kind to target
  "pattern": "default" | "default-hashed" | "package-identifier",
  "skipLeadingPath": ["src/"]                      // path prefixes stripped from the subpath segment
}
```

Patterns: `default` chains `package + file-path + class-identifier` (`@effect/package/subpath/FileName/ClassIdentifier`, collapsing duplicate filename/identifier); `default-hashed` hashes the `default` result (hides service names in builds); `package-identifier` uses `package + identifier` (best for flat one-file-per-service layouts). `deterministicKeys` is `off` by default and must be raised to `error`/`warning` for key reporting to fire.

---

## [3] — `diagnosticSeverity` value domain

```jsonc
"diagnosticSeverity": {
  "<ruleName>": "off" | "error" | "warning" | "message" | "suggestion"
}
```

`additionalProperties` accepts any rule name; the schema also enumerates each of the 76 named rules in [4] with its compiled default. Overriding a rule here changes its project-wide baseline. `*` is **not** valid as a key here (wildcard is a comment-directive-only affordance — see [5]).

---

## [4] — Diagnostic-rule catalogue (76 rules, exact spellings + schema-default severity)

Rule names are the exact `diagnosticSeverity` keys and the exact tokens used in `@effect-diagnostics effect/<rule>:<severity>` directives. Defaults are the compiled-in baselines from `schema.json`; categories mirror the README grouping. Severity legend: `error`, `warning`, `message`, `suggestion`, `off`.

### Correctness — wrong, unsafe, or structurally invalid patterns

| Rule                          | Default   |
| ----------------------------- | --------- |
| `anyUnknownInErrorContext`    | `off`     |
| `classSelfMismatch`           | `error`   |
| `duplicatePackage`            | `warning` |
| `effectFnImplicitAny`         | `error`   |
| `floatingEffect`              | `error`   |
| `genericEffectServices`       | `warning` |
| `missingEffectContext`        | `error`   |
| `missingEffectError`          | `error`   |
| `missingLayerContext`         | `error`   |
| `missingReturnYieldStar`      | `error`   |
| `missingStarInYieldEffectGen` | `error`   |
| `nonObjectEffectServiceType`  | `error`   |
| `outdatedApi`                 | `warning` |
| `outdatedEffectCodegen`       | `warning` |
| `overriddenSchemaConstructor` | `error`   |
| `unsupportedServiceAccessors` | `warning` |

### Anti-pattern — discouraged patterns that often lead to bugs

| Rule                            | Default      |
| ------------------------------- | ------------ |
| `catchUnfailableEffect`         | `suggestion` |
| `effectFnIife`                  | `warning`    |
| `effectGenUsesAdapter`          | `warning`    |
| `effectInFailure`               | `warning`    |
| `effectInVoidSuccess`           | `warning`    |
| `globalErrorInEffectCatch`      | `warning`    |
| `globalErrorInEffectFailure`    | `warning`    |
| `layerMergeAllWithDependencies` | `warning`    |
| `lazyPromiseInEffectSync`       | `warning`    |
| `leakingRequirements`           | `suggestion` |
| `multipleEffectProvide`         | `warning`    |
| `returnEffectInGen`             | `suggestion` |
| `runEffectInsideEffect`         | `suggestion` |
| `schemaSyncInEffect`            | `suggestion` |
| `scopeInLayerEffect`            | `warning`    |
| `strictEffectProvide`           | `off`        |
| `tryCatchInEffectGen`           | `suggestion` |
| `unknownInEffectCatch`          | `warning`    |

### Effect-native — prefer Effect-native APIs over global/native ones

| Rule                        | Default |
| --------------------------- | ------- |
| `asyncFunction`             | `off`   |
| `cryptoRandomUUID`          | `off`   |
| `cryptoRandomUUIDInEffect`  | `off`   |
| `extendsNativeError`        | `off`   |
| `globalConsole`             | `off`   |
| `globalConsoleInEffect`     | `off`   |
| `globalDate`                | `off`   |
| `globalDateInEffect`        | `off`   |
| `globalFetch`               | `off`   |
| `globalFetchInEffect`       | `off`   |
| `globalRandom`              | `off`   |
| `globalRandomInEffect`      | `off`   |
| `globalTimers`              | `off`   |
| `globalTimersInEffect`      | `off`   |
| `instanceOfSchema`          | `off`   |
| `newPromise`                | `off`   |
| `nodeBuiltinImport`         | `off`   |
| `preferSchemaOverJson`      | `off`   |
| `processEnv`                | `off`   |
| `processEnvInEffect`        | `off`   |
| `unsafeEffectTypeAssertion` | `off`   |

### Style — cleanup, consistency, idiomatic Effect

| Rule                             | Default      |
| -------------------------------- | ------------ |
| `catchAllToMapError`             | `suggestion` |
| `deterministicKeys`              | `off`        |
| `effectDoNotation`               | `off`        |
| `effectFnOpportunity`            | `suggestion` |
| `effectMapFlatten`               | `suggestion` |
| `effectMapVoid`                  | `suggestion` |
| `effectSucceedWithVoid`          | `suggestion` |
| `importFromBarrel`               | `off`        |
| `missedPipeableOpportunity`      | `off`        |
| `missingEffectServiceDependency` | `off`        |
| `nestedEffectGenYield`           | `off`        |
| `redundantSchemaTagIdentifier`   | `suggestion` |
| `schemaStructWithTag`            | `suggestion` |
| `schemaUnionOfLiterals`          | `off`        |
| `serviceNotAsClass`              | `off`        |
| `strictBooleanExpressions`       | `off`        |
| `unnecessaryArrowBlock`          | `off`        |
| `unnecessaryEffectGen`           | `suggestion` |
| `unnecessaryFailYieldableError`  | `suggestion` |
| `unnecessaryPipe`                | `suggestion` |
| `unnecessaryPipeChain`           | `suggestion` |

Total: 76 rules. Rules with a quick-fix (`🔧` in README): `classSelfMismatch`, `missingEffectError`, `missingReturnYieldStar`, `missingStarInYieldEffectGen`, `outdatedEffectCodegen`, `overriddenSchemaConstructor`, `unsupportedServiceAccessors`, `effectFnIife`, `layerMergeAllWithDependencies`, `multipleEffectProvide`, `returnEffectInGen`, `runEffectInsideEffect`, `scopeInLayerEffect`, `instanceOfSchema`, `unsafeEffectTypeAssertion`, `catchAllToMapError`, `deterministicKeys`, `effectFnOpportunity`, `effectMapVoid`, `effectSucceedWithVoid`, `importFromBarrel`, `missedPipeableOpportunity`, `redundantSchemaTagIdentifier`, `schemaStructWithTag`, `schemaUnionOfLiterals`, `serviceNotAsClass`, `unnecessaryArrowBlock`, `unnecessaryEffectGen`, `unnecessaryFailYieldableError`, `unnecessaryPipe`, `unnecessaryPipeChain`.

---

## [5] — In-source comment-directive language

Per-file / per-region severity control via comments, overriding the project `diagnosticSeverity` baseline:

```ts
// @effect-diagnostics effect/<rule>:<off|error|warning|message|suggestion>   // from this point on in the file
// @effect-diagnostics-next-line effect/<rule>:<severity>                     // next line only
// @effect-diagnostics *:off                                                  // wildcard (all rules); rule-specific overrides still win
```

Rule tokens are namespaced `effect/<ruleName>` (the same names as [4]). The wildcard `*` is valid only in directives, not in the `diagnosticSeverity` object. An unused `@effect-diagnostics-next-line` is itself reported at `missingDiagnosticNextLine` severity (default `warning`).

Codegen directives drive the `codegen` CLI command and the editor codegen refactors:

```ts
// @effect-codegens annotate       // add type annotations to exported constants from initializer types
// @effect-codegens accessors      // implement service accessors in Effect.Service / Context.Tag / Effect.Tag
// @effect-codegens typeToSchema    // generate Effect Schema classes from TypeScript types
```

Custom key-pattern identifier marker (requires `extendedKeyDetection: true`):

```ts
export function Repository(/** @effect-identifier */ identifier: string) {
  return Context.Tag("Repository/" + identifier)
}
```

---

## [6]-[CLI_SURFACE]

Bin `effect-language-service` (→ `cli.js`). Global flags: `--help` / `-h`, `--version`, `--completions <bash|zsh|fish|sh>`, `--log-level <all|trace|debug|info|warn|warning|error|fatal|none>`. Run locally (not globally) so it loads the project's own `typescript` version.

| [INDEX] | [COMMAND]     | [CAPABILITY]             | [LOCAL_FLAGS] |
| :-----: | :------------ | :----------------------- | :------------ |
|   [1]   | `setup`       | setup wizard             | —             |
|   [2]   | `config`      | severity picker          | —             |
|   [3]   | `patch`       | TypeScript patch         | record below  |
|   [4]   | `unpatch`     | patch revert             | record below  |
|   [5]   | `check`       | patch status             | record below  |
|   [6]   | `diagnostics` | diagnostic report        | record below  |
|   [7]   | `quickfixes`  | quick-fix diff report    | record below  |
|   [8]   | `codegen`     | codegen directive runner | record below  |
|   [9]   | `overview`    | export overview          | record below  |
|  [10]   | `layerinfo`   | layer composition report | record below  |

[CLI_FLAG_RECORDS]:
- `patch`: `--dir <directory>`, `--module <tsc|typescript>`, `--force`.
- `unpatch`: `--dir <directory>`, `--module <tsc|typescript>`.
- `check`: `--dir <directory>`.
- `diagnostics`: `--file <file>`, `--project <file>`, `--format <json|pretty|text|github-actions>`, `--strict`, `--severity <error,warning,message>`, `--progress`, `--lspconfig '<json>'`.
- `quickfixes`: `--file <file>`, `--project <file>`, `--code <string>`, `--line <integer>`, `--column <integer>`, `--fix <string>`.
- `codegen`: `--file <file>`, `--project <file>`, `--verbose`, `--force`.
- `overview`: `--file <file>`, `--project <file>`, `--max-symbol-depth <integer>`.
- `layerinfo`: `--file <file>`, `--name <string>`, `--outputs <string>`.

`patch` is the build-time integration point: with it applied, the `includeSuggestionsInTsc` / `ignoreEffect*InTscExitCode` plugin options (from [2]) govern how Effect diagnostics map to `tsc` output and exit code.

---

## [7] — Editor capability surface (non-importable, behavioral)

Quickinfo: extended Effect type on hover; `yield*` hover shows Effect type parameters; layer-variable hover shows service involvement and attempts a dependency graph. Completions: `Self` autocompletion in `Effect.Service` / `Context.Tag` / `Schema.TaggedClass` / `Schema.TaggedRequest` / `ServiceMap.Service` / `Model.Class`; `Effect.gen` → `function*(){}`; `Effect.fn` span-name from exported member; `DurationInput` string completions; namespace-import style; `Schema.brand` suggestions; comment directives. Refactors: async-function → `Effect.gen` / `Effect.fn` (optionally generating a tagged error per promise call); `Effect.gen` → `Effect.fn`; implement service accessors; function-calls ↔ `pipe()`; pipe ↔ datafirst; toggle return-type annotation; remove single-yield `Effect.gen`; wrap expression in `Effect.gen`; toggle `X.pipe(Y)` ↔ `pipe(X, Y)`; "Layer Magic" auto-composition; structural-type → Schema class (reusing existing schemas); `Effect.Service` → `Context.Tag` with static `Layer` property (`effect`/`scoped`/`sync`/`succeed` + `dependencies`). Miscellaneous: class renames propagate to `TaggedError`/`TaggedClass` identifiers; `RpcClient` "go to definition" resolves to the `Rpc` definition.

---

## [8] — Gaps

- **No reflectable type surface.** `assay api query --key @effect/language-service` returns `status: empty` / `0 types across 0 namespaces` because the package ships no `.d.ts` and declares no `package.json` `exports`/`types`; its declared `"main": "index.cjs"` even points at a file that is not shipped. This is not a missing-install gap — version `0.86.2` is installed and `restored` — it is the package's nature (bundled JS LSP plugin + CLI + `tsc` patcher). Every surface above is grounded from `schema.json` (30-key plugin options at `compilerOptionsDefinition.properties.compilerOptions.properties.plugins.items.anyOf[0]`; 76-rule severity map at `definitions.effectLanguageServicePluginDiagnosticSeverityDefinition`) and from each subcommand's own `--help`, the package's own authoritative contracts. There is no importable symbol to add a `--symbol`/`--full` reflection table for.
- **Runtime JS internals not transcribed.** `index.js`, `transform.js`, and `effect-lsp-patch-utils.js` are minified bundles with source maps; their internal functions are implementation, not a consumer contract, and no planning owner composes them as values. Intentionally omitted.
- **Rule descriptions abbreviated.** Full per-rule prose lives in the README diagnostics table and the upstream `src/diagnostics` folder; this page captures exact rule names + schema-default severities (the load-bearing facts for `diagnosticSeverity` / directive authoring) rather than duplicating prose.
