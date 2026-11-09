# [NX_TYPESCRIPT]

`@rasm/nx-typescript` is an Nx 24 plugin for TypeScript 7 projects. It adds type checking, emit, Vite builds, Storybook builds, Cloudflare Worker types, and Adobe plugin packages to the projects Nx core makes from `package.json`, reading each configuration through its own tool: the compiler's `typescript/sync` API and Vite's `resolveConfig`.

## [01]-[USE]

```json
{ "plugins": [{ "plugin": "@rasm/nx-typescript", "options": { "outputPath": ".artifacts" } }] }
```

| [INDEX] | [OPTION]     | [UNSET]                             | [SET]                                                                  |
| :-----: | :----------- | :---------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `outputPath` | Packages land in the project folder, Storybook in `<root>/storybook-static` | Packages land in `<outputPath>/uxp/<project name>` and `<outputPath>/zxpsigncmd/<project name>`, Storybook in `<outputPath>/storybook/<project name>` through `--output-dir` |

- `outputPath` exists because no tool reads a setting for these folders: the UXP packer writes beside the manifest it packs, ZXPSignCmd writes where its caller names, and Storybook takes its output folder from `--output-dir` alone; its cache stays in its own `node_modules/.cache/storybook`
- No root `tsconfig.json` or `tsconfig.base.json`: Nx core resolves imports through the classic TypeScript API whenever one exists, and TypeScript 7 publishes none
- Vitest suites come from `@nx/vitest`, whose one `test` target is the verb, and Playwright suites from `@nx/playwright`; Vite builds are inferred here, since `@nx/vite` 24.0.5 always adds `build-deps` and `watch-deps` (no option drops them), `dev`, the deprecated `serve`, `preview`, and `serve-static` (an `@nx/web` executor not installed here); no Vite target outside the verb set is inferred, and a dev server runs as `vite` from the project folder

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                    | [GIVES]                                                          |
| :-----: | :------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | `tsconfig.json` beside a `package.json`     | One TypeScript program set per project                           |
|  [02]   | `parseConfigFile` project references        | `tsc --project` over the config holding files, else over each config a solution references; `tsc --build` only for a config that itself references others |
|  [03]   | `Program.getConfigFileNames()`              | Every extended layer outside the project, as inputs of `check--tsc`, `build--tsc`, `build--vite`, and `build--storybook`, the tools reading it |
|  [04]   | Parsed `noEmit`, `outDir`, `declarationDir` | `build--tsc` with those folders as outputs where a config emits  |
|  [05]   | `.storybook/main.*`                         | `build--storybook`                                               |
|  [06]   | `cloudflare.config.ts`                      | `build--cf`, which `check--tsc` depends on                       |
|  [07]   | `vite.config.*` resolved by Vite            | `build--vite`, every environment's `build.outDir` as outputs   |
|  [08]   | UXP `manifest.json` naming one `host`       | `build--uxp`                                                     |
|  [09]   | CEP `CSXS/manifest.xml`                     | `build--zxpsigncmd`                                              |

- One compiler API process reads every project per graph build, with each Vite config resolved beside it, cached by every `tsconfig*.json`, each Vite config, the lockfile, the plugin source, the options, and the project set; a workspace library a config imports is not in the key
- A solution's references are checked one config at a time with `--project`, which writes nothing under `noEmit`; `tsc --build` writes a `.tsbuildinfo` beside each config even with `--incremental false`, and Nx's cache already skips an unchanged check
- `@effect/tsgo` joins the type-check hash when the workspace installs it, since `effect-tsgo patch` replaces the compiler binary
- Manifests count only when they decode: a `manifest.json` lacking `manifestVersion`, `id`, or a single-object `host`, or a `manifest.xml` lacking `ExtensionBundleId`, names nothing

## [03]-[TARGETS]

| [INDEX] | [TARGET]             | [RUNS]                                        | [WHEN]                                           | [CACHE] |
| :-----: | :------------------- | :-------------------------------------------- | :----------------------------------------------- | :-----: |
|  [01]   | `check--tsc`         | `tsc --project` per checked config            | Every program set without emit                   |   Yes   |
|  [02]   | `build--tsc`         | `tsc --project` or `tsc --build`              | A config that emits; `check--tsc` waits on it    |   Yes   |
|  [03]   | `build--cf`          | `cf workers types`                            | `cloudflare.config.ts` in the project            |   Yes   |
|  [04]   | `build--storybook`   | `storybook build`                             | `.storybook/main.*` in the project               |   Yes   |
|  [05]   | `build--uxp`         | `package` executor: `<project>.ccx`           | A UXP manifest, in every configuration           |   Yes   |
|  [06]   | `build--zxpsigncmd`  | `package` executor: `ZXPSignCmd -sign -tsa`   | A CEP manifest, in the `release` configuration   |   Yes   |
|  [07]   | `build--vite`        | `vite build`                                  | A `vite.config.*` in the project                 |   Yes   |
|  [08]   | `check`, `build`     | `nx:noop` over `<verb>--*` | Inputs open with `'...'` and union their parts   |   No    |

- A `.ccx` is a deterministic zip with no secret, so `build--uxp` packs in every configuration at one path; a `.zxp` needs the signing certificate, so `build--zxpsigncmd` writes only in `nx build <project> -c release`, the one configuration naming its `outputPath`, which Nx passes to every `build--*` task
- Both package targets depend on the project's other `build--*` targets and pack the folder among their outputs that holds the manifest, so the Vite or tsc build that wrote it decides the content, and hash those outputs and the signing variables with no npm package
- `.ccx` follows the UXP packer: a zip of the built plugin folder with `manifest.json` at its root, named for the project as `<project>.zxp` is, since UPIA reads the manifest inside and never the file name
- `.zxp` comes from `ZXPSignCmd -sign` with the base64 `.p12` in `ZXP_CERTIFICATE` and its password in `ZXP_CERTIFICATE_PASSWORD`, both task inputs; ZXPSignCmd signs a temporary copy of the built folder, since `-sign` writes `META-INF/` and `mimetype` into its input; the decoded `.p12` reaches ZXPSignCmd as `/dev/fd/3`, a pipe, never a file; timestamped against `timestampAuthority` (DigiCert's server by default)

## [04]-[RELEASE]

- A project holding a UXP or CEP manifest takes `release.version.versionActions: "@rasm/nx-typescript/release/version-actions"` and the `release` tag, so `nx.json` selects it with `tag:release`; npm packages need neither, since Nx core tags each `package.json` project `npm:public` or `npm:private` and versions it with `@nx/js`
- The manifest is the version file, since Adobe's hosts, UPIA, and the portals read it alone: `manifest.json` `version`, written with `jsonc-parser` keeping the file's layout; `CSXS/manifest.xml` `ExtensionBundleVersion`, the one version a CEP manifest requires (`ExtensionList` `Extension` `Version` is optional in the ExtensionManifest 7.0 schema and stays unwritten)
- A version other than `major.minor.patch` fails by name, since UXP takes `x.y.z` alone and a lower installed version refuses the package
- Adobe holds no registry version (Marketplace and Exchange take uploads by hand), so the registry current-version resolver fails; use the disk or git-tag resolver

## [05]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                                  |
| :-----: | :--------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts`   | Package entry: `createNodes`, the compiler and Vite reads, targets, and aggregates |
|  [02]   | `plugins/adobe-packages.ts`  | UXP and CEP manifest decoders, package names, and packers                |
|  [03]   | `executors/package/`         | `package` executor and its Nx schema                                     |
|  [04]   | `version-actions.ts`         | Nx release version actions writing the UXP and CEP manifest versions     |
