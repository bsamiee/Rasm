# [NX_TYPESCRIPT]

`@rasm/nx-typescript` is an Nx 24 plugin for TypeScript 7 projects. It adds type checking, emit, Vite builds, Storybook builds, Cloudflare Worker types, Adobe plugin packages, and package checks to the projects Nx core makes from `package.json`, reading each configuration through its own tool: the compiler's `@typescript/native/sync` API and Vite's `resolveConfig`.

## [01]-[USE]

```json
{ "plugins": [{ "plugin": "@rasm/nx-typescript", "options": { "outputPath": ".artifacts" } }] }
```

| [INDEX] | [OPTION]     | [UNSET]                                                                     | [SET]                                                                                                                                                                        |
| :-----: | :----------- | :-------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `outputPath` | Packages land in the project folder, Storybook in `<root>/storybook-static` | Packages land in `<outputPath>/uxp/<project name>` and `<outputPath>/zxpsigncmd/<project name>`, Storybook in `<outputPath>/storybook/<project name>` through `--output-dir` |

- `outputPath` exists because no tool reads a setting for these folders: the UXP packer writes beside the manifest it packs, ZXPSignCmd writes where its caller names, and Storybook takes its output folder from `--output-dir` alone; its cache stays in its own `node_modules/.cache/storybook`
- TypeScript installs as Nx's TypeScript 7 guide pairs it: `@typescript/native` (`npm:typescript@7`) is the compiler this plugin reads, runs as `tsc`, and hashes, a peer the workspace supplies, while `typescript` (`npm:@typescript/typescript6`) keeps the 6.0 compiler API that Nx core, `@nx/js`, and Storybook's docgen load by that name; TypeScript 7 ships none, and without it Nx core's import scan fails the graph on a root tsconfig
- Vite builds are inferred here, since `@nx/vite` 24.0.5 always adds `build-deps` and `watch-deps` (no option drops them), `dev`, the deprecated `serve`, `preview`, and `serve-static` (an `@nx/web` executor not installed here); no Vite target outside the verb set is inferred, and a dev server runs as `vite` from the project folder
- Vitest and Playwright suites are inferred here too: `@nx/vitest` 24.0.5 adds `test` to any config holding a `test` field whether or not a test file exists and hashes `vitest` alone, and `@nx/playwright` 24.0.5 sets `parallelism: false` on every suite whose server is not an `nx run` command and adds `e2e-ci` targets no documented option drops

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                    | [GIVES]                                                                                                                                                   |
| :-----: | :------------------------------------------ | :-------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `tsconfig.json` beside a `package.json`     | One TypeScript program set per project                                                                                                                    |
|  [02]   | `parseConfigFile` project references        | `tsc --project` over the config holding files, else over each config a solution references; `tsc --build` only for a config that itself references others |
|  [03]   | `Program.getConfigFileNames()`              | Every extended layer outside the project, as inputs of `check--tsc`, `build--tsc`, `build--vite`, and `build--storybook`, the tools reading it            |
|  [04]   | Parsed `noEmit`, `outDir`, `declarationDir` | `build--tsc` with those folders as outputs where a config emits                                                                                           |
|  [05]   | `.storybook/main.*`                         | `build--storybook`                                                                                                                                        |
|  [06]   | `cloudflare.config.ts`                      | `build--cf`, which `check--tsc` depends on                                                                                                                |
|  [07]   | `vite.config.*` resolved by Vite            | `build--vite`, every environment's `build.outDir` as outputs                                                                                              |
|  [08]   | UXP `manifest.json` naming one `host`       | `build--uxp`                                                                                                                                              |
|  [09]   | CEP `CSXS/manifest.xml`                     | `build--zxpsigncmd`                                                                                                                                       |
|  [10]   | An emitting config                          | `check--publint` where `publint` resolves from the workspace root, private packages included                                                              |
|  [11]   | `vitest list --filesOnly --json`            | `test--vitest` where the `vitest.config.*`, else `vite.config.*`, Vitest reads lists a test file, its `coverage.reportsDirectory` the output                |
|  [12]   | `playwright test --list --reporter=json`    | `test--playwright` where the `playwright.config.*` lists a suite, every project `outputDir` the output                                                      |

- One compiler API process reads every project per graph build, with each Vite config resolved beside it, cached by every `tsconfig*.json`, each Vite config, the lockfile, the plugin source, the options, and the project set
- Every workspace module a Vite config loads joins the cache entry, recorded through Node's `registerHooks` while `resolveConfig` runs, since Vite's default config loader keeps package imports external and reports the config file alone in `configFileDependencies`; an edit to one of them reads the configs again
- Solution references are checked one config at a time with `--project`, which writes nothing under `noEmit`; `tsc --build` writes a `.tsbuildinfo` beside each config even with `--incremental false`, and Nx's cache already skips an unchanged check
- `@effect/tsgo` joins the type-check hash when the workspace installs it, since `effect-tsgo patch` replaces the compiler binary
- `publint` packs the package through the package manager it detects, scripts off, and lints the tarball against its `package.json` with `publishConfig` applied; `--strict` turns its warnings into errors, its suggestions print without failing, and the check caches on the project's files and its `build--*` outputs, so a missing `dist` file or a wrong `exports` row fails `check`
- Test runners list their suites in a child process from the project folder, through the runner the project itself resolves, so a listing writes no output, starts no server or browser, and leaves the plugin process environment untouched; a project whose runner finds no test gets no `test`, and the cache entry holds the names of every file under each suite so an added or removed test file reads the suites again
- Test targets hash the project and its dependencies, the tsconfig chain, `CI`, and the npm packages the project and its workspace dependencies declare, which hold the runner, its pool, browser provider, and plugins, never the whole lockfile
- Manifests count only when they decode: a `manifest.json` lacking `manifestVersion`, `id`, or a single-object `host`, or a `manifest.xml` lacking `ExtensionBundleId`, names nothing

## [03]-[TARGETS]

| [INDEX] | [TARGET]            | [RUNS]                                      | [WHEN]                                         | [CACHE] |
| :-----: | :------------------ | :------------------------------------------ | :--------------------------------------------- | :-----: |
|  [01]   | `check--tsc`        | `tsc --project` per checked config          | Every program set without emit                 |   Yes   |
|  [02]   | `build--tsc`        | `tsc --project` or `tsc --build`            | Emitting config; `check--tsc` waits on it      |   Yes   |
|  [03]   | `build--cf`         | `cf workers types`                          | `cloudflare.config.ts` in the project          |   Yes   |
|  [04]   | `build--storybook`  | `storybook build`                           | `.storybook/main.*` in the project             |   Yes   |
|  [05]   | `build--uxp`        | `package` executor: `<project>.ccx`         | UXP manifest, every configuration              |   Yes   |
|  [06]   | `build--zxpsigncmd` | `package` executor: `ZXPSignCmd -sign -tsa` | CEP manifest, `release` configuration          |   Yes   |
|  [07]   | `build--vite`       | `vite build`                                | `vite.config.*` in the project                 |   Yes   |
|  [08]   | `check--publint`    | `publint --strict` after `build--*`         | Config emits, `publint` installed              |   Yes   |
|  [09]   | `test--vitest`      | `vitest run`                                | Vitest lists a test file                       |   Yes   |
|  [10]   | `test--playwright`  | `playwright test`                           | Playwright lists a suite                       |   Yes   |
|  [11]   | `check`, `build`, `test` | `nx:noop` over `<verb>--*`             | Inputs open with `'...'` and union their parts |   No    |

- `.ccx` files are deterministic zips with no secret, so `build--uxp` packs in every configuration at one path; `.zxp` signing needs the certificate, so `build--zxpsigncmd` writes only in `nx build <project> -c release`, the one configuration naming its `outputPath`, which Nx passes to every `build--*` task
- Both package targets pack the folder holding the manifest: in a project with a Vite build or a `tsc` emit, the output of its `build--*` targets that holds one, hashing those outputs; in a project with neither (Adobe's plain JavaScript template), its source folder as Adobe's packer reads it, the files Nx tracks there with every `.gitignore` applied, hashing `default`; both hash the signing variables and the plugin itself: its project's `default` files while it is a workspace project, else its npm version through `externalDependencies`, since Nx 24 hashes an executor's package for `@nx/` executors alone
- `.ccx` follows the UXP packer: a zip of the plugin folder with `manifest.json` at its root, named for the project as `<project>.zxp` is, since UPIA reads the manifest inside and never the file name
- `.zxp` comes from `ZXPSignCmd -sign` with the base64 `.p12` in `ZXP_CERTIFICATE` and its password in `ZXP_CERTIFICATE_PASSWORD`, both task inputs; ZXPSignCmd signs a temporary copy of the built folder, since `-sign` writes `META-INF/` and `mimetype` into its input; the decoded `.p12` reaches ZXPSignCmd as `/dev/fd/3`, a pipe, never a file; timestamped against `timestampAuthority` (DigiCert's server by default)

## [04]-[RELEASE]

- Packages emit through their `tsconfig.json` (`declaration`, `outDir` at `dist`, no `noEmit`), private ones included, so `publint` holds every package to the published layout before its `private` flag goes; a project shipping no package states `noEmit` with `declaration` off, since declaration checks run under `noEmit` too; `build--tsc` writes JavaScript and declarations per source file into `dist`, the same `tsc --project` run the editor and a direct build make; `publishConfig.exports` and `publishConfig.imports` (`"#/*": "./dist/*.js"`) name those files and pnpm applies them at pack, so extensionless `#/` self-imports emit unchanged and resolve in `dist`
- `nx-release-publish` comes from Nx core with `dependsOn: ["^nx-release-publish"]` alone, and a plugin target cannot add `build` to it (Nx merges core's `package.json` targets after every plugin), so `nx release` builds through `release.version.preVersionCommand` in `nx.json`
- Projects holding a UXP or CEP manifest take `release.version.versionActions: "@rasm/nx-typescript/release/version-actions"` and the `release` tag, so `nx.json` selects them with `tag:release`; npm packages need neither, since Nx core tags each `package.json` project `npm:public` or `npm:private` and versions it with `@nx/js`
- Manifests are the version file, since Adobe's hosts, UPIA, and the portals read it alone: `manifest.json` `version`, written with `jsonc-parser` keeping the file's layout; `CSXS/manifest.xml` `ExtensionBundleVersion`, written at the attribute value's offsets from `@xml-tools/ast` keeping the file's layout, the one version a CEP manifest requires (`ExtensionList` `Extension` `Version` is optional in the ExtensionManifest 7.0 schema and stays unwritten)
- Versions other than `major.minor.patch` fail by name, since UXP takes `x.y.z` alone and a lower installed version refuses the package
- Adobe holds no registry version (Marketplace and Exchange take uploads by hand), so the registry current-version resolver fails; use the disk or git-tag resolver

## [05]-[LAYOUT]

| [INDEX] | [PATH]                      | [HOLDS]                                                                            |
| :-----: | :-------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts`  | Package entry: `createNodes`, the compiler, Vite, and test runner reads, targets, and aggregates |
|  [02]   | `plugins/adobe-packages.ts` | UXP and CEP manifest decoders, package names, packers, and the package error       |
|  [03]   | `executors/package/`        | `package` executor and its Nx schema                                               |
|  [04]   | `version-actions.ts`        | Nx release version actions writing the UXP and CEP manifest versions               |
