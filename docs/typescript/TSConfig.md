# [TSCONFIG]

Compiler configuration of the TypeScript packages Nx builds and checks.

## [01]-[PRINCIPLES]

Rules every tsconfig file follows:
- One tsconfig file defines one program, with one `types` list, one `lib`, one root folder, and one emit decision
- Second files exist only where file sets differ in `types`, `lib`, root folder, or emit
- Shared options sit in one base with no files of its own, and every package config extends it
- `${configDir}` in base path options resolves against each extending file, and one base serves every package
- TypeScript finds only `tsconfig.json` on its own and reaches every other name through `-p`, `extends`, or `references`
- Nx core reads `paths` and `customConditions` from root `tsconfig.base.json`, then root `tsconfig.json`, and no other path
- Type checking runs as its own task, as Vite, Vitest, and bundlers strip types unchecked

## [02]-[LAYOUT]

tsconfig files and what each holds:

| [INDEX] | [FILE]                     | [HOLDS]                                                                   |
| :-----: | :------------------------- | :------------------------------------------------------------------------ |
|  [01]   | Root `tsconfig.base.json`  | Every shared compiler option, no `include`, no `files`                    |
|  [02]   | Root `tsconfig.json`       | `extends` base, `"include": []`, `references` to each project             |
|  [03]   | Package `tsconfig.json`    | `extends` base, `references`, runtime `types` and `lib`                   |
|  [04]   | Root `tsconfig.tests.json` | `noEmit: true` over root `vitest.config.ts`, `references` to test support |

Programs and their emit:
- Programs write into their own `dist` for `pnpm pack`, and `noEmit` is the one emit decision every `tsc` mode, editor, and Nx target reads
- Root `tsconfig.json` and `tsconfig.tests.json` emit nothing and write their `.tsbuildinfo` to `.cache/tsc`
- Test suite programs `references` reach each package they import, else TS6307 refuses the files outside their `include`
- Applications another project imports pair a composite `tsconfig.json` holding `"files": []` with a `noEmit` `tsconfig.app.json`
- `@rasm/nx-typescript` infers `check--tsc` for a `noEmit` program and `build--tsc` for an emitting one, where `check--tsc` runs the build
- Runtime `types`, DOM `lib`, and `jsx` sit in the package config that needs them, and files a second runtime runs take a program of their own
- Workerd test suites pair `tsconfig.node.json` with `tsconfig.worker.json` over `*.workers.test.ts`, and files both runtimes import sit in both

## [03]-[BASE]

```jsonc
{
    "compilerOptions": {
        "target": "esnext",
        "lib": ["esnext"],
        "module": "nodenext",
        "outDir": "${configDir}/dist",
        "composite": true,
        "sourceMap": true,
        "verbatimModuleSyntax": true,
        "erasableSyntaxOnly": true,
        "exactOptionalPropertyTypes": true,
        "noUncheckedIndexedAccess": true,
        "noPropertyAccessFromIndexSignature": true,
        "noImplicitOverride": true,
        "noImplicitReturns": true,
        "noFallthroughCasesInSwitch": true,
        "skipLibCheck": true,
        "plugins": [{ "name": "@effect/language-service" }]
    }
}
```

Package config, source at the package root:

```json
{
    "extends": "../../tsconfig.base.json",
    "references": [{ "path": "../<dependency>" }]
}
```

Defaults and runtime globals:
- `include` defaults to `**/*` and `exclude` to `node_modules` and `outDir`, and the base writes neither
- Programs name their runtime's globals: Node `["node"]`, Workers `["@cloudflare/workers-types"]`, browsers `["vite/client"]` with DOM `lib`
- Libraries reading no runtime global set no `types`

## [04]-[COMPILER_OPTIONS]

Written compiler options and the decision each states:

| [INDEX] | [OPTION]                             | [VALUE]                  | [REASON]                                                            |
| :-----: | :----------------------------------- | :----------------------- | :------------------------------------------------------------------ |
|  [01]   | `target`                             | `esnext`                 | Every Node `engines.node` allows runs emitted syntax unlowered      |
|  [02]   | `lib`                                | `["esnext"]`             | Compiler's newest ECMAScript library, matching `target`             |
|  [03]   | `module`                             | `nodenext`               | Code Node runs and published libraries, resolution following it     |
|  [04]   | `module`                             | `preserve`               | Bundled applications, and programs over types `nodenext` misreads   |
|  [05]   | `outDir`                             | `${configDir}/dist`      | Each package emits into its own `dist`                              |
|  [06]   | `composite`                          | `true`                   | Implies `declaration` and `incremental`, required for references    |
|  [07]   | `sourceMap`                          | `true` where `tsc` emits | Stack traces reach source lines                                     |
|  [08]   | `verbatimModuleSyntax`               | `true`                   | Type-only imports stay marked, implies `isolatedModules`            |
|  [09]   | `erasableSyntaxOnly`                 | `true`                   | Refuses syntax Node type stripping cannot run (`enum`, namespaces)  |
|  [10]   | `exactOptionalPropertyTypes`         | `true`                   | Optional keys and keys holding `undefined` stay distinct            |
|  [11]   | `noUncheckedIndexedAccess`           | `true`                   | Index reads include `undefined`                                     |
|  [12]   | `noPropertyAccessFromIndexSignature` | `true`                   | Index signature keys read through brackets alone                    |
|  [13]   | `skipLibCheck`                       | `true`                   | Dependency declarations go unchecked, their own builds checked them |
|  [14]   | `types`                              | Per program              | Default `[]` loads no globals                                       |
|  [15]   | `plugins`                            | Effect language service  | Patched `tsc` reports Effect diagnostics as compiler errors         |
|  [16]   | `jsx`                                | `react-jsx`              | Packages holding `.tsx` alone                                       |
|  [17]   | `noEmit`                             | `true`                   | Programs no other program references: suites, apps, tools           |

Options that stay unwritten:

| [INDEX] | [OPTION]                           | [REASON]                                                                |
| :-----: | :--------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `strict`                           | `true` by default                                                       |
|  [02]   | `moduleResolution`                 | Follows `module`: `nodenext` from `nodenext`, `bundler` from `preserve` |
|  [03]   | `rootDir` at config folder         | Default                                                                 |
|  [04]   | `isolatedModules`                  | Implied by `verbatimModuleSyntax`                                       |
|  [05]   | `esModuleInterop`                  | Always on, `false` errors                                               |
|  [06]   | `noUncheckedSideEffectImports`     | `true` by default                                                       |
|  [07]   | `moduleDetection` under `nodenext` | `"type": "module"` package files are modules                            |
|  [08]   | `declarationMap`                   | Tarball `files` omit maps, and editors open referenced source           |

Options for one case alone:
- `rewriteRelativeImportExtensions` rewrites relative `.ts` specifiers alone and refuses `#/` ones (TS2877), and `#/` rows serve them as written
- `allowImportingTsExtensions` permits `.ts` specifiers in programs emitting no JavaScript, and `rewriteRelativeImportExtensions` implies it
- `isolatedDeclarations` lets Oxc write `.d.ts` per file with no checker, and requires explicit types on every export
- `customConditions` names a source condition only where `exports` hide source behind one
- `stripInternal` drops `@internal` members from declarations
- `composite: false` sits beside `noEmit` only for inferred types no declaration names (TS2883, TS4094) or imports outside `include` (TS6307)
- `module: preserve` serves `export =` modules read by named import (`@adobe-uxp-types`) and extensionless `.d.ts` re-exports (`icu`, Workers types)

## [05]-[PROJECT_REFERENCES]

Project references and the generator writing them:
- Programs without references type-check sibling `.ts` source under their own `lib`, `types`, and `paths`, and siblings of another runtime fail
- `references` in a package config list the workspace packages it imports, and `tsc -b` builds them in order and skips unchanged ones
- Imports from a referenced project load its emitted `.d.ts`, and a package program re-checks no sibling source
- Referenced projects set `composite`, and a config with `"files": []` can reference a `noEmit` program, as TS6310 needs input files
- `@rasm/nx-typescript` runs `tsc --build` on a package `tsconfig.json` with `references`, `tsc` on one without
- `@nx/js` sync generator `typescript-sync` writes `references` from graph dependencies, run by `nx sync` and targets listing it in `syncGenerators`
- `typescript-sync` reports a project missing from root `references` on every run while its `tsconfig.json` is not composite
- `typescript-sync` writes each project dependency into the project's `tsconfig.lib.json` or `tsconfig.app.json`, else its `tsconfig.json`
- `references` follow each program's own imports: `nx.sync.ignoredDependencies` in a runtime config drops a dependency only another program imports
- Programs `typescript-sync` never writes (`tsconfig.node.json`, `worker`, `storybook`, `server`) list the packages their own files import
- Suites importing nothing from a `noEmit` project name it in `ignoredDependencies`, as TS6310 refuses a `noEmit` reference
- `sync.applyChanges: true` syncs before a terminal task run

## [06]-[COMPILERS]

TypeScript 7 is the native compiler with no compiler API, and TypeScript 6 stays installed for tools importing one:

```yaml
catalog:
  typescript: npm:@typescript/typescript6@<6.x>
  '@typescript/native': npm:typescript@<7.x>
  '@effect/tsgo': <version>
```

Packages and binaries of each compiler:
- `typescript` resolves to TypeScript 6, with the 6.0 API and binary `tsc6`
- `@typescript/native` provides binary `tsc` from TypeScript 7, and `tsc` and `tsc -b` run the native compiler
- Nx core requires `typescript` from the workspace root
- `effect-tsgo patch` in root `prepare` patches the native compiler under `typescript`, else under `@typescript/native`
- `effect-tsgo tsc` runs a patched compiler without touching `node_modules`, for read-only or global package stores
- Plugin keys `ignoreEffect{Suggestions,Warnings,Errors}InTscExitCode` pick severities failing `tsc`, and the base fails every one
- Base `diagnosticSeverity` raises each rule it names to `error`, and its `overrides` turn `asyncFunction` off in test and story files
- `plugins` is one array, and a package config overriding it restates the whole entry
- TypeScript 7 refuses every option TypeScript 6 deprecated
