# [TSCONFIG]

TypeScript compiler configuration of an Effect-based pnpm and Nx monorepo.

## [01]-[PRINCIPLES]

- One tsconfig file defines one program, with one `types` list, one `lib`, one root folder, and one emit decision
- Second files exist only where file sets differ on one of those
- Shared options sit in one base with no files of its own, and every package config extends it
- Path options in a base use `${configDir}`, which resolves against each extending file, so one base serves every package
- TypeScript finds only the name `tsconfig.json` on its own, and every other name is reached through `-p`, `extends`, or `references`
- Nx core reads `paths` and `customConditions` from root `tsconfig.base.json`, then root `tsconfig.json`, and no other path
- Options equal to a TypeScript 6 default stay unwritten, and a written one states a decision
- Type checking runs as its own task, as Vite, Vitest, and bundlers strip types unchecked

## [02]-[LAYOUT]

| [INDEX] | [FILE]                          | [HOLDS]                                                             |
| :-----: | :------------------------------ | :------------------------------------------------------------------ |
|  [01]   | Root `tsconfig.base.json`       | Every shared compiler option, no `include`, no `files`              |
|  [02]   | Root `tsconfig.json`            | `extends` the base, `"include": []`, `references` to each project   |
|  [03]   | Package `tsconfig.json`         | `extends` the base, `references`, runtime `types` and `lib`         |
|  [04]   | Tests program, where one exists | `noEmit: true` over test and config files, `references` to packages |

- Tests inside a library package take one root program (`tsconfig.tests.json`) the root `tsconfig.json` references
- Tests program `references` reach each package it imports by `#/` or self-reference, else TS6307 refuses the files outside its `include`
- Test packages emit into their ignored `dist`, as `@nx/js/typescript` disables `typecheck` where a config or its references set `noEmit`
- Runtime differences (DOM `lib`, Workers `types`, `jsx`) sit in the package config that needs them

## [03]-[BASE]

```jsonc
{
    "compilerOptions": {
        "target": "es2025",
        "lib": ["esnext"],
        "module": "nodenext",
        "outDir": "${configDir}/dist",
        "composite": true,
        "declarationMap": true,
        "sourceMap": true,
        "verbatimModuleSyntax": true,
        "erasableSyntaxOnly": true,
        "exactOptionalPropertyTypes": true,
        "noUncheckedIndexedAccess": true,
        "noImplicitOverride": true,
        "noImplicitReturns": true,
        "noFallthroughCasesInSwitch": true,
        "skipLibCheck": true,
        "types": ["node"],
        "plugins": [{ "name": "@effect/language-service" }]
    },
    "exclude": ["${configDir}/dist", "${configDir}/test"]
}
```

Package config, source at the package root:

```json
{
    "extends": "../../tsconfig.base.json",
    "references": [{ "path": "../<dependency>" }]
}
```

- `include` defaults to `**/*`, and a written `exclude` drops the default `outDir` exclusion, so base `exclude` names `dist` and `test`
- Source under `src/` takes `"include": ["src"]` and base `"rootDir": "${configDir}/src"`, so `dist` holds one output per `src` file
- Packages with other globals replace `types` (`["@cloudflare/workers-types"]`), and a browser program sets `"lib": ["esnext", "dom"]`

## [04]-[COMPILER_OPTIONS]

| [INDEX] | [OPTION]                     | [VALUE]                  | [REASON]                                                               |
| :-----: | :--------------------------- | :----------------------- | :--------------------------------------------------------------------- |
|  [01]   | `target`                     | Oldest runtime's level   | `nodenext` implies `esnext`, above what the oldest supported Node runs |
|  [02]   | `lib`                        | `["esnext"]`             | TypeScript 6 has no `es2026` lib, so ES2026 members sit in `esnext`    |
|  [03]   | `module`                     | `nodenext`               | Code Node runs and published libraries; resolution follows it          |
|  [04]   | `module`                     | `preserve`               | Bundled applications alone; resolution becomes `bundler`               |
|  [05]   | `outDir`                     | `${configDir}/dist`      | Each package emits into its own `dist`                                 |
|  [06]   | `composite`                  | `true`                   | Implies `declaration` and `incremental`, required for references       |
|  [07]   | `declarationMap`             | `true`                   | Editors jump from a referenced project's `.d.ts` to its source         |
|  [08]   | `sourceMap`                  | `true` where `tsc` emits | Stack traces reach source lines                                        |
|  [09]   | `verbatimModuleSyntax`       | `true`                   | Type-only imports stay marked, implies `isolatedModules`               |
|  [10]   | `erasableSyntaxOnly`         | `true`                   | Refuses syntax Node type stripping cannot run (`enum`, namespaces)     |
|  [11]   | `exactOptionalPropertyTypes` | `true`                   | Optional keys and keys holding `undefined` stay distinct               |
|  [12]   | `noUncheckedIndexedAccess`   | `true`                   | Index reads include `undefined`                                        |
|  [13]   | `skipLibCheck`               | `true`                   | Development programs skip `.d.ts` checks, a consumer check covers dist |
|  [14]   | `types`                      | Explicit list            | Default `[]` loads no global types                                     |
|  [15]   | `plugins`                    | Effect language service  | Patched `tsc` reports Effect diagnostics as compiler errors            |
|  [16]   | `jsx`                        | `react-jsx`              | Packages holding `.tsx` alone                                          |
|  [17]   | `noEmit`                     | `true`                   | Root tests program, application programs a bundler builds              |

Options that stay unwritten:

| [INDEX] | [OPTION]                           | [WHY]                                                                   |
| :-----: | :--------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `strict`                           | `true` by default from TypeScript 6                                     |
|  [02]   | `moduleResolution`                 | Follows `module`: `nodenext` from `nodenext`, `bundler` from `preserve` |
|  [03]   | `rootDir` at the config folder     | Default from TypeScript 6, and under `composite` before it              |
|  [04]   | `isolatedModules`                  | Implied by `verbatimModuleSyntax`                                       |
|  [05]   | `esModuleInterop`                  | Always on from TypeScript 6, `false` is an error                        |
|  [06]   | `noUncheckedSideEffectImports`     | `true` by default from TypeScript 6                                     |
|  [07]   | `moduleDetection` under `nodenext` | Files of a `"type": "module"` package are modules already               |

Options for one case alone:
- `rewriteRelativeImportExtensions` rewrites relative `.ts` specifiers to `.js` at emit, and `#/` specifiers written without extension need none
- `allowImportingTsExtensions` permits `.ts` specifiers in programs emitting no JavaScript, and `rewriteRelativeImportExtensions` implies it
- `isolatedDeclarations` lets a per-file emitter (Oxc, tsdown) write `.d.ts` with no checker, and requires explicit types on every export
- `customConditions` names a source condition only where `exports` hide source behind one
- `stripInternal` drops `@internal` members from declarations

## [05]-[PROJECT_REFERENCES]

- `references` in a package config list the workspace packages it imports, and `tsc -b` builds them in order, skipping unchanged ones
- Imports from a referenced project load its emitted `.d.ts`, so a package program re-checks no sibling source
- Referenced projects set `composite`, and a config with `"include": []` can reference a `noEmit` program (TS6310 needs input files)
- `@nx/js/typescript` runs `typecheck` through `tsc --build` where root `tsconfig.json` references every composite project
- `@nx/js/typescript` infers `build` where its `build` option names `configName: "tsconfig.json"` (default `tsconfig.lib.json`)
- `build.skipBuildCheck: true` keeps that `build` target where `exports` name source, which the plugin otherwise leaves without one
- `@nx/js` sync generator `typescript-sync` writes `references` from graph dependencies, run by `nx sync` and targets listing it in `syncGenerators`
- `sync.applyChanges: true` syncs before a terminal task run, other shells exit naming `nx sync`, and CI skips sync, which `nx sync:check` guards
- Without references, each package program type-checks its siblings' `.ts` source through `exports`, which costs time and needs no emit
- Bundled packages that emit declarations through their bundler take one `noEmit` check program in place of references

## [06]-[COMPILERS]

TypeScript 7 is the native compiler with no compiler API, and TypeScript 6 stays installed for tools importing one:

```yaml
catalog:
  typescript: npm:@typescript/typescript6@<6.x>
  '@typescript/native': npm:typescript@<7.x>
  '@effect/tsgo': <version>
```

- `typescript` resolves to TypeScript 6, whose package re-exports the 6.0 API and provides binary `tsc6`
- `@typescript/native` provides binary `tsc` from TypeScript 7, so `tsc` and `tsc -b` run the native compiler
- Nx core requires `typescript` from the workspace root
- `effect-tsgo patch` in root `prepare` patches the native compiler, trying `typescript`, then `@typescript/native`
- `effect-tsgo tsc` runs a patched compiler without touching `node_modules`, for read-only or global package stores
- Plugin keys `ignoreEffect{Suggestions,Warnings,Errors}InTscExitCode` decide which severities fail `tsc`, suggestions passing by default
- `plugins` is one array, so a package config that overrides it restates the whole entry
- TypeScript 7 refuses every option TypeScript 6 deprecated: `baseUrl`, `moduleResolution: node10`, `target: es5`, `downlevelIteration`, `module: amd`

## [07]-[REGRESSIONS]

| [INDEX] | [WRONG]                                                     | [CORRECT]                                                          |
| :-----: | :---------------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | Base tsconfig moved into a config folder                    | Root `tsconfig.base.json`, which Nx reads by literal name          |
|  [02]   | Root layers per environment (`tsconfig.web.json`)           | One base, runtime options in the package config needing them       |
|  [03]   | Separate emit layer for publishable packages                | One base emitting for every package                                |
|  [04]   | Per-package `tsconfig.lib.json`, `tsconfig.spec.json` pairs | One config per package, a second program only for another runtime  |
|  [05]   | `tsconfig.build.json` differing only in emit options        | Emit options in the base                                           |
|  [06]   | TypeScript 7 alone as `typescript`                          | TypeScript 6 as `typescript`, TypeScript 7 as `@typescript/native` |
|  [07]   | `module: preserve` in a library `tsc` publishes             | `nodenext`, as `bundler` resolution accepts code only bundlers run |
|  [08]   | `declaration: false, noEmit: true` restated per test config | One root tests program over every package's tests                  |
|  [09]   | `paths` mapping one package to another's source             | Workspace links and `exports`                                      |
|  [10]   | `ignoreDeprecations: "6.0"`                                 | Deprecated options removed                                         |
|  [11]   | `strict: false`, `experimentalDecorators`                   | Default `strict`, standard decorators                              |
|  [12]   | Loose `.d.ts` files beside source                           | Declarations emitted to an ignored `dist`                          |
