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

| [INDEX] | [FILE]                           | [HOLDS]                                                                                 |
| :-----: | :------------------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | Root `tsconfig.base.json`        | Every shared compiler option, no `include`, no `files`                                  |
|  [02]   | Root `tsconfig.json`             | `extends` the base, `"include": []`, `references` to each project                       |
|  [03]   | Package `tsconfig.json`          | `extends` the base, `references`, runtime `types` and `lib`                             |
|  [04]   | Root `tsconfig.unpublished.json` | `extends` the base, sets `rootDir` and `outDir` for every program publishing no package |
|  [05]   | Root `tsconfig.tests.json`       | `noEmit: true` over root `vitest.config.ts`, `references` to test support               |

- Release packages emit into their own `dist`, the folder `pnpm pack` reads, and apps, test suites, and tools write under `.artifacts/tsc/<projectRoot>`
- Root `tsconfig.json` and `tsconfig.tests.json` emit nothing and write their `.tsbuildinfo` to `.cache/tsc`
- Test suite programs `references` reach each package they import, else TS6307 refuses the files outside their `include`
- Applications another project imports pair a composite `tsconfig.json` holding `"files": []` with a `noEmit` `tsconfig.app.json`, as TS6310 refuses a `noEmit` reference
- `@rasm/nx-typescript` infers `check--tsc` for a `noEmit` program and `build--tsc` for an emitting one, its `check--tsc` running the build
- Runtime `types`, DOM `lib`, and `jsx` sit in the package config that needs them, and files a second runtime runs take a program of their own
- Workerd test suites pair `tsconfig.node.json` with `tsconfig.worker.json` over `*.workers.test.ts`, files both runtimes import sitting in both

## [03]-[BASE]

```jsonc
{
    "compilerOptions": {
        "target": "esnext",
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

- `include` defaults to `**/*` and `exclude` to `node_modules` and `outDir`, so the base writes neither
- Programs name the globals of the runtime running their files: Node `["node"]`, Workers `["@cloudflare/workers-types"]`, a browser `["vite/client"]` with `"lib": ["esnext", "dom"]`
- Libraries reading no runtime global set no `types`

## [04]-[COMPILER_OPTIONS]

| [INDEX] | [OPTION]                             | [VALUE]                  | [REASON]                                                               |
| :-----: | :----------------------------------- | :----------------------- | :--------------------------------------------------------------------- |
|  [01]   | `target`                             | `esnext`                 | Node at the `engines.node` floor runs emitted syntax unlowered         |
|  [02]   | `lib`                                | `["esnext"]`             | TypeScript 6 has no `es2026` lib, so ES2026 members sit in `esnext`    |
|  [03]   | `module`                             | `nodenext`               | Code Node runs and published libraries; resolution follows it          |
|  [04]   | `module`                             | `preserve`               | Bundled applications, and programs over types `nodenext` misreads      |
|  [05]   | `outDir`                             | `${configDir}/dist`      | Each package emits into its own `dist`                                 |
|  [06]   | `composite`                          | `true`                   | Implies `declaration` and `incremental`, required for references       |
|  [07]   | `declarationMap`                     | `true`                   | Editors jump from a referenced project's `.d.ts` to its source         |
|  [08]   | `sourceMap`                          | `true` where `tsc` emits | Stack traces reach source lines                                        |
|  [09]   | `verbatimModuleSyntax`               | `true`                   | Type-only imports stay marked, implies `isolatedModules`               |
|  [10]   | `erasableSyntaxOnly`                 | `true`                   | Refuses syntax Node type stripping cannot run (`enum`, namespaces)     |
|  [11]   | `exactOptionalPropertyTypes`         | `true`                   | Optional keys and keys holding `undefined` stay distinct               |
|  [12]   | `noUncheckedIndexedAccess`           | `true`                   | Index reads include `undefined`                                        |
|  [13]   | `noPropertyAccessFromIndexSignature` | `true`                   | Index signature keys read through brackets alone                       |
|  [14]   | `skipLibCheck`                       | `true`                   | Development programs skip `.d.ts` checks, a consumer check covers dist |
|  [15]   | `types`                              | Per program              | Default `[]` loads no globals, so each program names its runtime       |
|  [16]   | `plugins`                            | Effect language service  | Patched `tsc` reports Effect diagnostics as compiler errors            |
|  [17]   | `jsx`                                | `react-jsx`              | Packages holding `.tsx` alone                                          |
|  [18]   | `noEmit`                             | `true`                   | Programs Vite, Storybook, or Node runs from source                     |

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
- `noEmit` programs set `composite: false` to drop declaration errors (TS4094), and `declarationMap: false`, which TS5069 refuses without `composite`
- `module: preserve` serves `export =` modules read by named import (`@adobe-uxp-types`) and extensionless `.d.ts` re-exports (`icu`, Workers types)

## [05]-[PROJECT_REFERENCES]

- `references` in a package config list the workspace packages it imports, and `tsc -b` builds them in order, skipping unchanged ones
- Imports from a referenced project load its emitted `.d.ts`, so a package program re-checks no sibling source
- Referenced projects set `composite`, and a config with `"include": []` can reference a `noEmit` program (TS6310 needs input files)
- `@rasm/nx-typescript` runs `tsc --build` on a package `tsconfig.json` with `references`, `tsc` on one without
- `@nx/js` sync generator `typescript-sync` writes `references` from graph dependencies, run by `nx sync` and targets listing it in `syncGenerators`
- `typescript-sync` reports a project missing from root `references` on every run while its `tsconfig.json` is not composite
- `typescript-sync` writes dependency `references` into a `tsconfig.lib.json` or `tsconfig.app.json` the project holds, else into its `tsconfig.json`, a solution included
- `nx.sync.ignoredDependencies` in a runtime config keeps `typescript-sync` from writing a dependency its program never imports there
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
