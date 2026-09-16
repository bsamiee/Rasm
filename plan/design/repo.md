# [REPO]

Repository integration of the Creative Cloud work: catalog rows, project manifests, target bodies, the task graph edit, Biome and ast-grep, harness rows, install routes, the Effect 4 migration list, validation tooling, continuous integration, and the readbacks that close the join. Every row states a fact read from disk on 2026-09-15, a declaration file with its line, or a documentation page the row names.

## [01]-[CATALOG]

`pnpm-workspace.yaml` `catalog:` holds every TypeScript version under `catalogMode: strict` and `saveExact: true`, root `package.json` `devDependencies` mirrors each row as `"<name>": "catalog:"`, and a row sits as `name: version` under its `# <group>` header.

Added rows land in a new group `# Adobe hosts and fonts` after `# 3D and geospatial rendering`, except `@swc/cli` under `# Tooling`, `fast-check` under `# Testing`, `cheerio` under `# Serialization and messaging` beside `fast-xml-parser`, and `@modelcontextprotocol/inspector` under `# Tooling` beside `hostinger-api-mcp`, the other MCP command row.

| [INDEX] | [ROW]                     | [VERSION] | [REASON]                                                                     |
| :-----: | :------------------------ | :-------- | :--------------------------------------------------------------------------- |
|  [01]   | `types-for-adobe`         | 7.2.6     | Illustrator ExtendScript typings the ES3 entries compile against             |
|  [02]   | `@adobe-uxp-types/uxp`    | 0.1.4     | `uxp` module typings both plugin projects compile against                    |
|  [03]   | `@adobe-uxp-types/photoshop` | 0.1.8  | Photoshop DOM typings, named by the Photoshop plugin alone                   |
|  [04]   | `vite-uxp-plugin`         | 1.3.8     | Vite plugin that writes `manifest.json` into each plugin bundle              |
|  [05]   | `fontkit`                 | 2.0.4     | Reads a font file by path for the metrics the grid formulas take             |
|  [06]   | `@types/fontkit`          | 2.0.9     | `fontkit` declares no `types` field and no `types` export condition          |
|  [07]   | `@swc/cli`                | 0.8.1     | `swc` binary that lowers the Illustrator entries to ES3                      |
|  [08]   | `fast-check`              | 4.10.1    | `effect@4` re-exports `FastCheck` no longer and declares no dependency on it; `tests/typescript/support/properties.ts` imports its default export for the `scheduler`, `commands`, and `modelRun` families |
|  [09]   | `cheerio`                 | 1.2.0     | The tree's one HTML parser: Acrobat's accessibility report is HTML, not XML, so `fast-xml-parser` cannot read it; `acrobat_check_accessibility` selects its 32 rule rows by CSS selector (`acrobat.md` [04] row 14, unit 9) |
|  [10]   | `@modelcontextprotocol/inspector` | 2.6.0 | Stdio MCP client CLI, one process per assertion with a stable exit code; the `--cli` mode takes a command as its target and `--method`, so unit 10's gate is a checked exit code instead of a hand-run `initialize` ([12] row 16) |
|  [11]   | `ts-morph`                | 28.0.0    | Emits the generated host declarations: `indesign.d.ts` from the InDesign scripting dictionary (`indesign.md` [02]) and the Illustrator delta over `types-for-adobe` (`illustrator.md` [04] row 24), units 3 and 7 |

One row moves: `jszip` 3.10.1 → 3.10.2, published 2026-09-08, the first release of that line since 2022.

One row leaves. `@effect/doctest` has no consumer: `vitest.config.ts` declares no `plugins` entry, and `rg` over every `.ts` in `libs`, `apps`, `tests`, `tools`, and `infra` finds no `import.meta.vitest` fence and no `doctest` import. A catalog row with no consumer is removed rather than kept against a future one; it returns with the file that runs it.

One candidate stays out. `@modelcontextprotocol/conformance` tests a server over HTTP alone: its `server` mode requires `--url <url>` and its scenarios drive an MCP endpoint, while `creative-cloud` is a stdio server, so no unit can run it. Its npm `latest` is 0.1.16 of 2026-03-30 and the line that moves is the `alpha` tag, 0.2.0-alpha.11 of 2026-08-07; neither reaches a stdio target.

Rows move to `4.0.0-rc.115`, the newest `effect` release including prereleases (dist-tag `rc`, published 2026-09-11). One version number spans the ecosystem, so every package that still has a 4.x line sits at exactly that value.

| [INDEX] | [ROW]                      | [FROM]  | [INDEX] | [ROW]                    | [FROM] |
| :-----: | :------------------------- | :------ | :-----: | :----------------------- | :----- |
|  [01]   | `effect`                   | 3.22.1  |  [11]   | `@effect/sql-d1`         | 0.50.0 |
|  [02]   | `@effect/platform-node`    | 0.108.1 |  [12]   | `@effect/sql-mysql2`     | 0.53.0 |
|  [03]   | `@effect/platform-bun`     | 0.91.2  |  [13]   | `@effect/sql-mssql`      | 0.54.0 |
|  [04]   | `@effect/platform-browser` | 0.77.1  |  [14]   | `@effect/sql-clickhouse` | 0.50.0 |
|  [05]   | `@effect/opentelemetry`    | 0.64.0  |  [15]   | `@effect/ai-anthropic`   | 0.27.0 |
|  [06]   | `@effect/sql-pg`           | 0.53.0  |  [16]   | `@effect/ai-openai`      | 0.41.0 |
|  [07]   | `@effect/sql-sqlite-node`  | 0.53.0  |  [17]   | `@effect/ai-openrouter`  | 0.12.0 |
|  [08]   | `@effect/sql-sqlite-bun`   | 0.53.0  |  [18]   | `@effect/vitest`         | 0.30.0 |
|  [09]   | `@effect/sql-sqlite-wasm`  | 0.53.0  |  [19]   | `@effect/docgen`         | 0.5.2  |
|  [10]   | `@effect/sql-libsql`       | 0.42.0  |         |                          |        |

Rows leave. Every v3 `@effect/*` package peer-depends on `effect@^3` and cannot sit beside `effect@4`, so a merged row is deleted rather than pinned back.

| [INDEX] | [ROW]                       | [REASON]                                                     |
| :-----: | :-------------------------- | :----------------------------------------------------------- |
|  [01]   | `@effect/platform`          | Merged into `effect`, no 4.x line                            |
|  [02]   | `@effect/experimental`      | Merged into `effect`, no 4.x line                            |
|  [03]   | `@effect/typeclass`         | Merged into `effect`, no 4.x line                            |
|  [04]   | `@effect/sql`               | Merged as `effect/unstable/sql`                              |
|  [05]   | `@effect/cluster`           | Merged as `effect/unstable/cluster`                          |
|  [06]   | `@effect/workflow`          | Merged as `effect/unstable/workflow`                         |
|  [07]   | `@effect/ai`                | Merged as `effect/unstable/ai`                               |
|  [08]   | `@effect/cli`               | Merged as `effect/unstable/cli`                              |
|  [09]   | `@effect/rpc`               | Merged as `effect/unstable/rpc`                              |
|  [10]   | `@effect/printer`           | Absorbed by `effect/unstable/cli`                            |
|  [11]   | `@effect/printer-ansi`      | Absorbed by `effect/unstable/cli`                            |
|  [12]   | `@effect/build-utils`       | Merged into `effect`, no 4.x line                            |
|  [13]   | `@effect/ai-amazon-bedrock` | Upstream publishes no 4.x line                               |
|  [14]   | `@effect/ai-google`         | Upstream publishes no 4.x line                               |
|  [15]   | `@effect-atom/atom`         | No 4.x line; `effect/unstable/reactivity/Atom` holds the API |
|  [16]   | `@effect-atom/atom-react`   | No 4.x line, no importing source file                        |

Rows join at `4.0.0-rc.115`, each a first-party package with a 4.x line and a role the tree holds (registry read on 2026-09-15):

| [INDEX] | [ROW]                      | [GROUP]                            | [REASON]                                                                                                   |
| :-----: | :------------------------- | :--------------------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `@effect/sql-pglite`       | Data access and storage            | `PgliteClient` over the `@electric-sql/pglite` row, a `SqlClient` with `listen`, the test support database |
|  [02]   | `@effect/sql-sqlite-do`    | Data access and storage            | SQLite driver for Cloudflare Durable Objects beside the `@effect/sql-d1` row                               |
|  [03]   | `@effect/ai-openai-compat` | Runtime services and observability | Provider for OpenAI-compatible endpoints beside the three provider rows                                     |
|  [04]   | `@effect/atom-react`       | Web UI                             | React bindings for `effect/unstable/reactivity` `Atom`, successor of `@effect-atom/atom-react`             |
|  [05]   | `scheduler`                | Web UI                             | Peer of `@effect/atom-react` at the `react-dom` canary's own version, one copy through an override row     |
|  [06]   | `@effect/openapi-generator`| Tooling                            | `openapigen` writes `Schema` types, clients, and `HttpApi` modules from OpenAPI, peers `@effect/platform-node` |

Rows stay out after the registry read:

| [INDEX] | [ROW]                                                                          | [REASON]                                                                                                    |
| :-----: | :----------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `@effect/platform-deno`                                                        | 4.x line depends on `@jsr/*` packages the npm registry answers 404 to, a registry row for a runtime `mise.toml` lacks |
|  [02]   | `@effect/atom-vue`, `@effect/atom-solid`, `@effect/sql-sqlite-react-native`    | 4.x lines for frameworks the tree lacks                                                                     |
|  [03]   | `@effect/sql-drizzle`, `@effect/sql-kysely`, `@effect/eslint-plugin`           | No 4.x line                                                                                                 |
|  [04]   | `@effect/tsgo`                                                                 | Second TypeScript compiler beside the `typescript` row, `@effect/language-service` is the Effect diagnostic  |
|  [05]   | `@effect/platform-node-shared`                                                 | Dependency of `@effect/platform-node` and `@effect/platform-bun`, no direct import                          |
|  [06]   | `tsx`, `@swc-node/register`                                                    | Loaders Node 26 type stripping replaces: `process.features.typescript` is `strip`, `erasableSyntaxOnly` keeps every file strippable, Nx 23 loads `tools/nx/workspace.ts` natively (`nx/dist/src/plugins/js/utils/register.js`, `isNativeStripPreferred`), the `@swc-node/register>typescript` override goes with the row |

`overrides` takes `'@effect/atom-react>scheduler': 'catalog:'` under the peer-range group. `tests/typescript/support/package.json` names `@effect/sql-pglite` in place of `@electric-sql/pglite`, which `@effect/sql-pglite` depends on.

`@effect/language-service` keeps 0.87.2: it serves both majors and its `outdatedApi` diagnostic reports every removed v3 call, which makes it the migration's checker. `@swc/core` sits at `1.16.4-nightly-20260913.1`, the newest published build, which `rasm:upgrade --configuration typescript` resolved on 2026-09-15. `overrides` takes one new row, `tsconfck>typescript: 'catalog:'`, under the peer-range group: `@effect/docgen@4.0.0-rc.115` depends on `tsconfck@3.1.6`, whose peer `typescript ^5.0.0` the catalog's `7.1.0-dev` crosses (`pnpm why tsconfck` on 2026-09-15). `@swc/cli` peers `@swc/core ^1.2.66` and `chokidar ^5.0.0`, both catalog rows, `@adobe-uxp-types/photoshop` peers `@adobe-uxp-types/uxp 0.1.4`, the catalog row, and `vite-uxp-plugin` declares no peer. `allowBuilds` takes one row, `'@modelcontextprotocol/inspector': true`: its `postinstall` runs `node scripts/install-clients.mjs`, which places the `mcp-inspector` binary the gate calls, and `pnpm install` refuses with `ERR_PNPM_IGNORED_BUILDS` until the row states it. `@swc/core` reads `true` already and no other added package runs a lifecycle script.

## [02]-[PROJECTS]

Projects join per manifest. Every TypeScript project is a `package.json` beside a `tsconfig.json` that extends `tsconfig.base.json`. Every Swift project is an `.xcodeproj` whose basename names the project, its scheme, and its product.

| [INDEX] | [PATH]                                     | [NAME]                        | [LANGUAGE] | [OWNS]                                      |
| :-----: | :----------------------------------------- | :---------------------------- | :--------- | :------------------------------------------ |
|  [01]   | `libs/typescript/typography/`        | `@rasm/typography`      | TypeScript | Grid formulas, preset parse, font metrics   |
|  [02]   | `apps/creative-cloud/server/`              | `@rasm/creative-cloud-server` | TypeScript | MCP server, host table, job model, channels |
|  [03]   | `apps/creative-cloud/indesign-plugin/`     | `@rasm/indesign-plugin`       | TypeScript | InDesign UXP plugin: client, tick, handlers |
|  [04]   | `apps/creative-cloud/photoshop-plugin/`    | `@rasm/photoshop-plugin`      | TypeScript | Photoshop UXP plugin of the same shape      |
|  [05]   | `apps/creative-cloud/illustrator-scripts/` | `@rasm/illustrator-scripts`   | TypeScript | One ES3 entry per Illustrator tool, prelude |
|  [06]   | `apps/creative-cloud/window-list/`         | `WindowList`                  | Swift      | Window-server reader the proofs run         |

`apps/creative-cloud/server/` holds `indesign/`, `illustrator/`, `photoshop/`, and `acrobat/`; each directory holds that host's tool table, its JSON data files, and for the osascript hosts the job assembly. Acyclic order: `typography` → `server` → both plugins; the ES3 project and `WindowList` stand alone.

`@rasm/typography` exports `./grid`, `./metrics`, `./page-sizes`, and `./presets`, each mapped to the `.ts` file of that name, and `@rasm/creative-cloud-server` exports `./frames`, mapped to `frames.ts`, the one module both plugins import. No other project is imported, so no other manifest declares `exports`, and every manifest carries `"private": true` and `"type": "module"`. `effect` and `@effect/platform-node` are `catalog:` rows of manifests [01] to [04]; the column names the rest.

| [INDEX] | [PROJECT]                     | [DEPENDENCIES]                                       | [NX_TARGETS]      |
| :-----: | :---------------------------- | :--------------------------------------------------- | :---------------- |
|  [01]   | `@rasm/typography`      | `fontkit`, `@types/fontkit`                          | `grid`, `oracle`  |
|  [02]   | `@rasm/creative-cloud-server` | `@rasm/typography` as `workspace:*`, `cheerio` | `deploy`          |
|  [03]   | `@rasm/indesign-plugin`       | `@rasm/creative-cloud-server` as `workspace:*`, `vite`, `vite-uxp-plugin`, `@adobe-uxp-types/uxp`, `fast-xml-parser`, `ts-morph` | `build`, `deploy`, `generate` |
|  [04]   | `@rasm/photoshop-plugin`      | Row [03] rows less `fast-xml-parser` and `ts-morph`, plus `@adobe-uxp-types/photoshop` | `build`, `deploy` |
|  [05]   | `@rasm/illustrator-scripts`   | `types-for-adobe`, `@swc/core`, `@swc/cli`, `fast-xml-parser`, `ts-morph` | `build`, `generate` |
|  [06]   | `WindowList`                  | None                                                 | None              |

Each `tsconfig.json` extends `../../../tsconfig.base.json` and sets `outDir` to `.cache/typescript/out/<project root>` under the workspace root. `@rasm/creative-cloud-server` lists one `references` row, `../../../libs/typescript/typography`, and each plugin lists `../server`, each row matching a `workspace:*` row; no other project references a sibling.

| [INDEX] | [PROJECT]                     | [TYPES]                                            | [OTHER]                                     |
| :-----: | :---------------------------- | :------------------------------------------------- | :------------------------------------------ |
|  [01]   | `@rasm/typography`      | `["node"]`                                         | None                                        |
|  [02]   | `@rasm/creative-cloud-server` | `["node"]`                                         | `include` adds `*/*.ts` per host directory  |
|  [03]   | `@rasm/indesign-plugin`       | `["@adobe-uxp-types/uxp"]`                         | Generated `indesign.d.ts` declares module `indesign` |
|  [04]   | `@rasm/photoshop-plugin`      | `["@adobe-uxp-types/uxp", "@adobe-uxp-types/photoshop"]` | None                                  |
|  [05]   | `@rasm/illustrator-scripts`   | `[]`                                               | `lib: []`, the `/// <reference types="types-for-adobe/Illustrator/2022"/>` line in `prelude.ts`, `/// <reference path="./prelude.ts"/>` per entry, one generated `.d.ts` |

`node` sits in `types` only where a file reads `import.meta.dirname` or a `node:` module: `@types/node/web-globals/importmeta.d.ts:6`–`:12` is the one declaration of `ImportMeta.dirname`, and no `effect/dist/*.d.ts` references `node:`, `NodeJS`, or `Buffer`. Rows [01] and [02] read `import.meta.dirname`; rows [03] and [04] import `frames.ts`, which pulls `effect` alone, so they carry the UXP typings alone; `tests/typescript/support` carries `[]`.

Row [02] overrides `include` because `tsconfig.base.json` sets `include: ["${configDir}/*.ts"]`, which reaches the project root alone, and the root `tsconfig.json` is the precedent for a project listing its own globs.

Row [05] sets `lib: []` rather than `noLib: true`, which the base's `lib` row refuses with `TS5053`. With `lib: []` the compiler loads no default library, and `types-for-adobe/Illustrator/2022/index.d.ts` pulls `shared/global.d.ts` and through it `shared/JavaScript.d.ts`, the ES3 globals. `types-for-adobe@7.2.6` declares no `exports`, `types`, `typings`, or `main`, publishes `Illustrator/2015.3` and `Illustrator/2022` alone, and documents one consumer route, a `/// <reference types="types-for-adobe/Illustrator/2022"/>` line in the source; a `types` array entry resolves against `typeRoots`, where the package does not sit, so `types` stays empty and `prelude.ts` carries the reference line while each entry references `prelude.ts`. The project holds one `.d.ts`, the generated delta of `plan/design/illustrator.md` [04] row 24; the host members neither it nor the typings carry sit in a `declare global` block at the top of `prelude.ts`, which is why the project keeps the base's `moduleDetection: "force"` (`plan/design/illustrator.md` [04] rows 03 to 06).

Rows [03] and [04] leave `DOM` out of `lib`, which Adobe's UXP TypeScript guide requires because UXP supplies `Document` and `HTMLElement` itself. No InDesign UXP typings exist from Adobe or from DefinitelyTyped, so row [03] compiles against `indesign.d.ts` generated from the host's scripting dictionary (`plan/design/indesign.md` [02]).

`WindowList.xcodeproj/project.pbxproj` is the manifest `tools/nx/workspace.ts` reads. It holds one macOS command-line-tool target and one shared scheme, both named `WindowList`, with the project root at `apps/creative-cloud/window-list/` and the Swift sources beside the `.xcodeproj`.

## [03]-[TARGETS]

One target calls one tool, arguments on the command and configuration in the tool's own file. Each body sits in its project's `package.json` `nx.targets` and runs with `cwd` at `{projectRoot}`; `cache`, `executor`, and `inputs` for `build` come from the `nx.json` entry of [04].

| [INDEX] | [PROJECT]                   | [TARGET] | [OUTPUTS]                                                     |
| :-----: | :-------------------------- | :------- | :------------------------------------------------------------ |
|  [01]   | `@rasm/indesign-plugin`     | `build`  | `{workspaceRoot}/.artifacts/creative-cloud/indesign-plugin`   |
|  [02]   | `@rasm/indesign-plugin`     | `deploy` | None                                                          |
|  [03]   | `@rasm/photoshop-plugin`    | `build`  | `{workspaceRoot}/.artifacts/creative-cloud/photoshop-plugin`  |
|  [04]   | `@rasm/photoshop-plugin`    | `deploy` | None                                                          |
|  [05]   | `@rasm/illustrator-scripts` | `build`  | `{workspaceRoot}/.artifacts/creative-cloud/illustrator/*.jsx` |
|  [06]   | `@rasm/typography`    | `grid`   | None                                                          |
|  [07]   | `@rasm/creative-cloud-server` | `deploy` | None                                                        |
|  [08]   | `@rasm/typography`    | `oracle` | None                                                          |
|  [09]   | `@rasm/indesign-plugin`     | `generate` | `{projectRoot}/indesign.d.ts`                               |
|  [10]   | `@rasm/illustrator-scripts` | `generate` | `{projectRoot}/illustrator.d.ts`                            |

```text
[01] vite build
[02] node automation.ts deploy
[03] vite build
[04] node automation.ts deploy
[05] swc . -d ../../../.artifacts/creative-cloud/illustrator --out-file-extension jsx --strip-leading-paths --ignore '**/node_modules/**'
[06] node automation.ts grid
[07] node automation.ts deploy
[08] node automation.ts oracle
[09] node automation.ts generate
[10] node automation.ts generate
```

Rows [02], [04], and [07] carry `"cache": false` and `"parallelism": false`, and rows [02] and [04] `"dependsOn": ["build"]`: each writes a store the running host reads. Rows [06] and [08] carry `"cache": false`, and row [06] takes its arguments after `--`. Rows [09] and [10] read the host bundle, so each names the bundle path and the `sdef` reader's version among its `inputs` and its emitted declaration file among its `outputs`, and `build` and `typecheck` depend on it. Row [07] copies `acrobat/rasm-trusted.js` into `~/Library/Application Support/Adobe/Acrobat/DC/JavaScripts/` and `acrobat/actions/*.sequ` into `DC/Sequences/`, refusing with `HostRunning{pid}` while `pgrep -x <acrobat.processName>` answers a process.

`WindowList` declares no body. `tools/nx/workspace.ts` gives a `.pbxproj` project empty `build`, `install`, `lint`, `format`, and `check` targets, and `nx.json` fills each for `tag:language:swift`: `xcodebuild … build` into `.cache/xcode/{projectRoot}`, `xcrun swift-format lint --strict --recursive`, `xcrun swift-format format --in-place --recursive`, and `check` over `build` and `lint`. `install` never runs; its `DSTROOT=/Applications` shape serves an app bundle.

Every TypeScript project also takes `typecheck` and `check` with empty bodies from the plugin, filled by `nx.json` for `tag:language:typescript`: `tsc --build --pretty false` with `cwd {projectRoot}` and `dependsOn ["^typecheck"]`, and `check` as `nx:noop` over `typecheck` and `test`.

[VITE_BUILD]: `vite.config.ts` in each plugin project calls `uxp(config)` from `vite-uxp-plugin`, whose signature is `uxp(config: UXP_Config, mode?: string) => Plugin`. The mode argument stays unset, so the plugin's `closeBundle` produces no archive; it emits `manifest.json` into the bundle folder from `config.manifest`. `build.outDir` names the artifacts path, `build.emptyOutDir` is `true` because Vite empties an out directory by default only when it sits under the project root, `build.target` is `es2022`, and `rollupOptions.external` lists `uxp` with `indesign` or `photoshop`, which the bundle keeps as `require` lookups the host resolves.

[UXP_CONFIG]: `uxp.config.ts` exports `const config: UXP_Config`, the one owner of every manifest field. `UXP_Config` extends `UXP_Config_Extra`, whose `hotReloadPort`, `webviewUi`, `webviewReloadPort`, and `copyZipAssets` are required, and adds `manifest: UXP_Manifest` with the keys `id`, `name`, `version`, `main`, `manifestVersion`, `host`, `entrypoints`, `featureFlags`, `requiredPermissions`, `addon`, and `icons`. `manifestVersion` is `5`: Adobe's InDesign manifest page documents v5 with `host.app "ID"` and a required `minVersion`, and the TK9 Photoshop plugins installed on this machine carry v5 with `host {app: "PS", minVersion: "23.3.0"}`. `host` is `[{ app: "ID", minVersion: "21.6.0" }]` for InDesign and `[{ app: "PS", minVersion: "27.11.0" }]` for Photoshop. `version` is the value the plugin folder name and the registry row of [08] take.

[SWC]: `apps/creative-cloud/illustrator-scripts/.swcrc` is the project's own file, the one configuration the swc target reads:

```json
{
    "$schema": "https://swc.rs/schema.json",
    "jsc": { "parser": { "syntax": "typescript" }, "target": "es3", "loose": true },
    "isModule": false
}
```

`jsc.target` accepts `es3`: the `JscTarget` union at `@swc/types@0.1.28/index.d.ts:572` lists it, and so does the `JscConfig.properties.target` enum of the published schema. The prose page documents `es5` as the default and the schema documents `es3`; both accept the value. `isModule` is a top-level key of `Options` (`index.d.ts:408`), and `false` keeps the emit free of a module wrapper. `jsc.parser.syntax` is one of `"ecmascript"`, `"typescript"`, and `"flow"`, and the CLI compiles a `.ts` input straight through `@swc/core` `transformFile` with no bundling stage. swc finds the file per input by walking up from each input file's directory and stopping at the working directory, so the target names no `--config-file`. `--ignore` takes comma-separated globs matched against the slash-normalized relative path, and `**/node_modules/**` keeps the project's `node_modules` out of the walk, which the CLI otherwise compiles: without it the target emitted 29 `@swc` package files beside the two sources. `jsc.loose` is required for the ES3 host, because the non-loose lowering of a tuple destructuring or an object spread calls `Symbol.iterator`, `Object.keys`, and `Object.defineProperty`.

Each entry emits one `.jsx` beside the prelude's `prelude.jsx` and loads it at run time through `$.evalFile` from its own folder, so the server runs the entry file as built and no entry carries an `import` or an `export`.

[DEPLOY]: `automation.ts` in each plugin project and in the server is one command built with `Command.make` from `effect/unstable/cli` and run by `Command.run(command, { version })`, whose config is `{ version: string; renderErrors?: boolean }` (`effect/unstable/cli/Command.d.ts:2233`) and which reads argv from the `Stdio` service rather than a `process` global. `NodeRuntime.runMain` provides `NodeServices.layer`, which supplies `ChildProcessSpawner`, `Crypto`, `FileSystem`, `Path`, `Stdio`, and `Terminal`. A plugin's command reads `HOME` through `Config.String`, removes the previous plugin folder, copies the built folder, and rewrites the registry row of [08]; the server's command copies the Acrobat files of row [07].

## [04]-[TASK_GRAPH]

`nx.json` `targetDefaults.build` gains a fourth entry after the `{ "cache": true }`, dotnet, and swift entries:

```json
{
    "filter": { "projects": ["tag:language:typescript"] },
    "executor": "nx:run-commands",
    "inputs": ["production", "^production", "typescript", { "externalDependencies": ["vite", "@swc/core", "@swc/cli"] }]
}
```

`cache: true` comes from the first entry; commands and `outputs` sit in each project's `nx` block. No `deploy`, `grid`, or `generate` default exists: a target one project owns holds its body in that project.

`nx.json` `targetDefaults.typecheck`, the `tag:language:typescript` entry, replaces `"^production"` in `inputs` with `{ "dependentTasksOutputFiles": "**/*.d.ts", "transitive": true }`. The key is an `inputs` member of its own object form, `"dependentTasksOutputFiles"` of type `string` beside an optional `"transitive"` boolean, `node_modules/nx/schemas/nx-schema.json:758`–`:768`, and it matches the glob against the resolved outputs of the tasks this task depends on. `typecheck` emits declarations alone, so its output tree under `.cache/typescript/out/{projectRoot}` holds `.d.ts`, `.d.ts.map`, and `.tsbuildinfo` files; under `^production` any edit to a dependency's source invalidated every downstream `typecheck` even when the emitted declarations were byte-identical, and under this entry only a changed declaration does.

`tools/nx/workspace.ts` needs no change. Its `configurations` record is keyed by extension (`.csproj`, `.json`, `.pbxproj`, `.toml`) and selected with `path.extname(file)`, and its glob already matches `{apps,libs,tests}/**/tsconfig.json` and `{apps,libs,tests,tools}/**/*.xcodeproj/project.pbxproj`. Every joining manifest is a `tsconfig.json` or a `project.pbxproj`, forms the record already holds.

Project dependencies come from `package.json` `dependencies` alone, because `pluginsConfig["@nx/js"].analyzeSourceFiles` is `false`. A `workspace:*` row is one static edge, and the `references` row of the same project's `tsconfig.json` mirrors it so `tsc --build` compiles the referenced project first. The graph gains three edges: `@rasm/creative-cloud-server` → `@rasm/typography`, and each plugin → `@rasm/creative-cloud-server`.

`@nx/vitest` infers `test` where a `vitest.config.ts` sits beside a `package.json`. A project takes one by adding `vitest.config.ts` re-exporting the root factory and `*.test.ts` beside its sources; `check` then runs it.

## [05]-[BIOME]

`biome.json` `overrides` takes one entry per concern, and the [CHANGE] column says which entries widen and which join. Order follows the file, from the tree-wide entry to the narrowest.

| [INDEX] | [CONCERN]                | [CHANGE]  | [CAUSE]                                                                     |
| :-----: | :----------------------- | :-------- | :-------------------------------------------------------------------------- |
|  [01]   | Vite configuration files | Widened   | Vite configuration file default-exports its config                          |
|  [02]   | UXP host modules         | Added     | `indesign`, `photoshop`, and `uxp` are host modules, not npm packages       |
|  [03]   | ExtendScript names       | Added     | `tsc` over the typings owns undeclared names for the scripts project        |
|  [04]   | CLI argv read            | Unchanged | `infra/automation.ts` alone names `process`; a deploy command reads `Stdio` |
|  [05]   | Qwik domain              | Moved     | `linter.domains.qwik: none` once, the per-file `domains` toggles go         |

```text
[01] includes  **/vitest.config.ts, **/vite.config.ts
[01] rules     style.noDefaultExport off
[02] includes  apps/creative-cloud/indesign-plugin/**, apps/creative-cloud/photoshop-plugin/**
[02] rules     correctness.noUndeclaredDependencies off, correctness.noUnresolvedImports off (no `domains.qwik` key: Biome 2.5 activates the qwik domain by dependency alone, a probe under either plugin root reported `noUndeclaredVariables` with or without it)
[03] includes  apps/creative-cloud/illustrator-scripts/**
[03] rules     correctness.noUndeclaredVariables off
[04] includes  infra/automation.ts
[04] rules     correctness.noProcessGlobal off
```

Entry [02] repeats the rule set the `.claude/plugins/**` entry turns off, under its own includes: each concern retires on its own day. Entry [05] holds because a group set to `error` enables every rule of the group, the qwik domain's `useQwikValidLexicalScope` included, whatever the manifest depends on (Biome analyzer contributing guide: a recommended rule with a domain is enabled through the domain's `recommended` or `all` setting, a group severity enables the group whole); `.claude/plugins/**` and `infra/automation.ts` reported it, so its one setting sits at `linter.domains` where the tree's other domains sit. `files.includes` needs no change: `.artifacts/` is gitignored and `vcs.useIgnoreFile` is `true`, so no emitted `.jsx` or plugin bundle is ever checked.

## [06]-[RULES]

Rules sit at `tools/ast-grep/rules/<language>/<family>/<id>.yml` with `language: tsx`, `severity: error`, and a `files:` list relative to the directory holding `sgconfig.yml`; utilities sit at `tools/ast-grep/utils/typescript/`. Each rule lands once a second instance of its defect is recorded in the tree and no checker already reports it. AppleScript has no ast-grep grammar, so an AppleScript rule matches the TypeScript template string that carries the script.

| [INDEX] | [ID]                                      | [DEFECT]                                                                   |
| :-----: | :---------------------------------------- | :------------------------------------------------------------------------- |
|  [01]   | `extendscript/no-export`                  | Exporting entry takes a wrapper whose `default` key the host reads instead |
|  [02]   | `extendscript/no-import`                  | Entry with an import is a module; the host evaluates one script text       |
|  [03]   | `extendscript/no-app-undo`                | `app.undo()` after a swatch-group add crashes Illustrator                  |
|  [04]   | `uxp/no-bracket-index-on-collection`      | Bracket index returns `undefined` on an InDesign DOM collection            |
|  [05]   | `uxp/no-dom-equality`                     | Equality over DOM objects and enumerators is false unless references match |
|  [06]   | `uxp/no-innerhtml`                        | `innerHTML` assignment writes unvalidated text into a UXP panel            |
|  [07]   | `effect/no-timeout-without-detached-fork` | `Effect.timeout` over a child interrupts the fiber and kills the process   |
|  [08]   | `applescript/no-posix-file-inside-tell`   | `POSIX file` inside a `tell` compiles as a specifier and fails `-1728`     |

```text
[01] before  export default main;
[01] after   main();
[01] core    {kind: export_statement}
[02] before  import { measure } from './prelude.ts';
[02] after   (no statement; the driver concatenates prelude text and entry text)
[02] core    {kind: import_statement}
[03] before  app.undo();
[03] after   (no call; the tool reports its result and the caller decides)
[03] core    {kind: call_expression, has: {field: function, regex: '^app\.undo$'}}
[04] before  doc.pages[0]
[04] after   doc.pages.item(0)
[04] core    subscript_expression with a number index whose object property is in the collection list
[05] before  page.side === PageSideOptions.LEFT_HAND
[05] after   String(page.side) === 'LEFT_HAND'
[05] core    binary_expression with an equality operator over a member on an enumerator identifier
[06] before  status.innerHTML = text;
[06] after   status.textContent = text;
[06] core    {kind: assignment_expression, has: {field: left, has: {field: property, regex: '^innerHTML$'}}}
[07] before  spawner.string(command).pipe(Effect.timeout(seconds))
[07] after   Effect.forkDetach(run).pipe(Effect.flatMap((f) => Fiber.join(f).pipe(Effect.timeoutOrElse({ duration, orElse }))))
[07] core    Effect.timeout* over a spawner.(string|lines|exitCode|streamLines) call
[08] before  `tell application "<host>" ... POSIX file "<path>" ... end tell`
[08] after   set f to POSIX file "<path>" before the tell block
[08] core    {kind: template_string, regex: 'tell application[\s\S]*POSIX file[\s\S]*end tell'}
```

Files each family scans: `extendscript/` reads `apps/creative-cloud/illustrator-scripts/*.ts`; `uxp/` reads `apps/creative-cloud/indesign-plugin/*.ts` and `apps/creative-cloud/photoshop-plugin/*.ts`; `effect/` and `applescript/` read `apps/creative-cloud/**/*.ts` and `libs/typescript/**/*.ts`. Rows [02], [04], and [06] carry a `fix` because their after form is mechanical; the rest state their form in `message` and `note`.

No `extendscript/no-for-of` rule exists: `typescript/syntax/no-for-of-loop.yml` already reports every `for…of` over the whole tree, and a swc `for…of` lowering that throws on a host collection is that rule's own case. No rule reports an exported tagged-error class or a nullable return either, because `tsc` reports `TS9021` under `isolatedDeclarations` and `typescript/syntax/no-nullable-return.yml` is already in place.

`tools/ast-grep/rules/typescript/syntax/require-exit-code-decision.yml` keys its last alternative on a `string`, `lines`, `streamString`, `streamLines`, or `exitCode` call on a spawner bound from `effect/unstable/process` `ChildProcessSpawner`, through `yield*` or as the arrow parameter beside the service argument: each helper drops the exit code or the streams, so `spawner.spawn` and the handle's `exitCode` beside `stdout` and `stderr` is the form. The tree's checkers and writers read nothing under `plan/inputs/` or the research bundles: `.gitignore` holds both, keeping the research READMEs, scripts, decompiled output, and Grid Calculator data tracked, and Biome, ruff, and ast-grep each honour the git ignore file, so no checker carries its own exclusion.

## [07]-[HARNESS]

`.mcp.json` holds twenty rows before and after: one row joins and one leaves.

| [INDEX] | [FILE]                  | [ROW]                         | [CHANGE] |
| :-----: | :---------------------- | :---------------------------- | :------- |
|  [01]   | `.mcp.json`             | `creative-cloud`              | Unit 10  |
|  [02]   | `.mcp.json`             | `indesign-sidekick`           | Deleted  |
|  [03]   | `.claude/settings.json` | `"mcp__creative-cloud__*"`    | Unit 10  |
|  [04]   | `.claude/settings.json` | `"mcp__indesign-sidekick__*"` | Deleted  |
|  [05]   | `.claude/settings.json` | `"Bash(swc *)"`               | Added    |
|  [06]   | `.claude/settings.json` | `"Bash(vite *)"`              | Added    |

```json
"creative-cloud": { "type": "stdio", "command": "mise", "args": ["exec", "--", "node", "apps/creative-cloud/server/main.ts"] }
```

Server row runs from source: Node strips types, `mise exec` resolves `mise.toml` from the project root so the path stays project-relative, and a workspace package's `exports` map to `.ts` files resolves through the pnpm symlink. No `serve` target and no server build exists. The allow list holds 66 rows today and 68 after; `Bash(osascript *)`, `Bash(screencapture *)`, `Bash(/usr/libexec/PlistBuddy *)`, `Bash(defaults *)`, `Bash(difft *)`, `mcp__illustrator__*`, and `mcp__computer-use__*` are rows already. `indesign-sidekick` appears in no other file in the tree, so rows [02] and [04] are its whole footprint.

## [08]-[INSTALL]

One route per host, with no second attempt behind it. Adobe documents `.ccx` double-click and UPIA for UXP plugins, neither for a Beta build, and UPIA answers `-411` on this machine. Folder-and-registry route is proven here by the installed TK9 Photoshop plugins and by Sidekick.

| [INDEX] | [HOST]      | [ROUTE]                                                                                                         |
| :-----: | :---------- | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | InDesign    | `deploy` copies the built folder into the plugin store, then rewrites `ID.json`                                 |
|  [02]   | Photoshop   | Same copy and the matching row of `PS.json`                                                                     |
|  [03]   | Illustrator | Nothing installed: Illustrator 30 loads no third-party UXP, so each job's `.jsx` text goes over `do javascript` |
|  [04]   | Acrobat     | Nothing installed: each job's script text goes over `do script`                                                 |

```text
plugin folder   ~/Library/Application Support/Adobe/UXP/Plugins/External/<id>_<version>/
InDesign row    ~/Library/Application Support/Adobe/UXP/PluginsInfo/v1/ID.json   mode 0644
Photoshop row   ~/Library/Application Support/Adobe/UXP/PluginsInfo/v1/PS.json   mode 0600
```

Each registry file holds one key, `plugins`, an array of rows. Each row holds `hostMinVersion`, `name`, `path` as `$localPlugins/External/<id>_<version>`, `pluginId`, `status` as `"enabled"`, `type` as `"uxp"`, and `versionString`. The `deploy` command decodes the file with `Schema.fromJsonString`, replaces the row whose `pluginId` matches, encodes, and writes the file back with its mode kept.

No `.ccx` is produced: the `vite-uxp-plugin` mode argument that writes one stays unset, so no `package` target and no archive path exist. UPIA and the UXP Developer Tool are named nowhere.

## [09]-[MIGRATION]

Every file in the tree that imports Effect takes one edit. The specifiers they name today collapse to `effect`, `@effect/platform-node`, and `@effect/vitest`. `FastCheck`, `ParseResult`, `MetricKey`, `MetricPair`, `MetricState`, and `TestServices` leave the `effect` index, `Runtime` stays as the `runMain` teardown module without `Runtime.runPromise`, and inside surviving modules `Effect.validateAll`, `Effect.runtime`, `Data.tagged`, `Data.struct`, `Stream.asyncPush`, `Layer.scoped`, `Order.array`, `Option.fromNullable`, `Schema.transformOrFail`, `Schema.parseJson`, `Schema.filter`, and `Context.GenericTag` are gone (`effect@4.0.0-rc.115` `dist/*.d.ts`, read 2026-09-15); every other imported name survives. The `function-hooks` plugin imports no Effect and needs no edit.

| [INDEX] | [FILE]                                    | [NAMES_THAT_MOVE]                                                           |
| :-----: | :---------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `tools/nx/workspace.ts`                   | `FileSystem`, `Path`, `ParseResult`, `NodeContext`, `decodeUnknown`         |
|  [02]   | `tools/yak/rhino-mcp-platform.ts`         | `Command`, `CommandExecutor`, `FileSystem`, `Path`, `NodeContext`, `Config` |
|  [03]   | `infra/automation.ts`                     | `Command`, `Options`, `Path`, `NodeContext`, `Runtime`                      |
|  [04]   | `infra/program.ts`                        | None                                                                        |
|  [05]   | `vitest.config.ts`                        | `FileSystem`, `Path`, `NodeContext`, `Config`, `parseJson`, `decodeUnknown` |
|  [06]   | `tests/typescript/support/arbitraries.ts` | `FastCheck`                                                                 |
|  [07]   | `tests/typescript/support/properties.ts`  | `FastCheck`, `TestServices`, the Schema filter and decode names             |
|  [08]   | `tests/typescript/support/resources.ts`   | `HttpApp`, `HttpServer`, `HttpServerError`, `ParseResult`, `Context.Tag`    |
|  [09]   | `tests/typescript/support/setup.ts`       | None: `addEqualityTesters` is still exported                                |
|  [10]   | `tests/typescript/support/telemetry.ts`   | `MetricKey`, `MetricPair`, `MetricState`                                    |

| [INDEX] | [EFFECT_3]                                 | [EFFECT_4]                                                     |
| :-----: | :----------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `@effect/platform` `FileSystem`, `Path`    | `effect` `FileSystem`, `Path`                                  |
|  [02]   | `NodeContext.layer`                        | `NodeServices.layer`                                           |
|  [03]   | `@effect/platform` `Command`               | `effect/unstable/process` `ChildProcess`                       |
|  [04]   | `CommandExecutor`                          | `ChildProcessSpawner`, with the command as an argument         |
|  [05]   | `@effect/cli` `Command`, `Options`         | `effect/unstable/cli` `Command`, `Flag`                        |
|  [06]   | `Options.boolean`                          | `Flag.Boolean` piped through `Flag.withDefault`                |
|  [07]   | `Command.run(cmd)(argv)`                   | `Command.runWith(cmd, argv)`, or `Command.run` reading `Stdio` |
|  [08]   | `Effect.runtime<R>()` and `Runtime.runPromise` | `Effect.context<R>()` and `Effect.runPromiseWith(context)`; `Effect.runPromise` where `R` is `never`, as `infra/program.ts` is |
|  [09]   | `ParseResult` inside `Schema.transformOrFail` | `Schema.decodeTo(to, { decode, encode })` with `SchemaGetter.transformEffect` and `SchemaGetter.forbidden`, failing with a `SchemaIssue.InvalidValue` |
|  [10]   | `Schema.parseJson`                         | `Schema.fromJsonString`                                        |
|  [11]   | `Schema.decodeUnknown`, `decode`, `encode`, `equivalence`, `Literal(...spread)` | `Schema.decodeUnknownEffect`, `decodeEffect`, `encodeEffect`, `toEquivalence`, `Literals([array])` |
|  [12]   | `Config.string`, `Config.boolean`          | `Config.String`, `Config.Boolean`                              |
|  [13]   | `FastCheck` from `effect`                  | `effect/unstable/arbitrary` `Arbitrary` for every value `it.effect.prop` takes, since `Vitest.Arbitraries` accepts `Arbitrary` or `Schema` alone (`@effect/vitest` `index.d.ts:36`); `fast-check`, whose `fc` is the default export, for `scheduler`, `commands`, `modelRun`, and `asyncModelRun`, run through `fc.assert` inside `it.effect` |
|  [14]   | `TestServices`                             | None, `it.effect` supplies the test services and a predicate carries `R` alone |
|  [15]   | `@effect/platform` `HttpApp`, `HttpServer` | `effect/unstable/http`, `HttpApp.Default<E>` as `Effect<HttpServerResponse, E, HttpServerRequest \| Scope>` |
|  [16]   | `Context.Tag(id)<Self, Shape>()`, `Context.GenericTag<S>(id)` | `Context.Service<Self, Shape>()(id)`, `Context.Service<S>(id)` |
|  [17]   | `MetricKey`, `MetricPair`, `MetricState`   | `Metric.Metric.Snapshot`, a union by `type` carrying `id`, `attributes`, and `state` |
|  [18]   | `Effect.forkDaemon`                        | `Effect.forkDetach`                                            |
|  [19]   | `Effect.catchAll`, `Effect.catchAllCause`  | `Effect.catch`, `Effect.catchCause`                            |
|  [20]   | `Schema.filter`, `Schema.pattern`, `int`, `endsWith` | `Schema.check` over `Schema.isPattern`, `isInt`, `isEndsWith`, `Schema.makeFilter` for a new predicate |
|  [21]   | `Tool.make` with a fields record           | `Tool.make` with a Schema for `parameters`                     |
|  [22]   | `McpServer.layerStdio({name, version})`    | Same with a non-empty `protocols`, reading the `Stdio` service |
|  [23]   | `Effect.validateAll`                       | `Effect.partition`, then `Array.match` over the failures       |
|  [24]   | `Stream.asyncPush((emit) => …)`            | `Stream.callback((queue) => …)` with `Queue.offerUnsafe`       |
|  [25]   | `Layer.scoped`                             | `Layer.effect`, which runs the effect under the layer scope    |
|  [26]   | `Data.tagged`, `Data.struct`               | `Data.taggedEnum`; plain objects, which `Equal.equals` and `Hash.hash` treat structurally |
|  [27]   | `Option.fromNullable`                      | `Option.fromNullishOr`                                         |
|  [28]   | `Effect.tapDefect((cause) => …)`           | The callback receives the defect; `Cause.die` rebuilds the cause for `Cause.pretty` |
|  [29]   | `Order.array(Order.number)` over bytes     | `Order.make` over `Buffer.compare`                             |
|  [30]   | `Array.filterMap` with `Option.liftPredicate` | `Array.filterMap` takes a `Filter`; a predicate over mapped values is `Array.filter` after `Array.map` |

`tests/typescript/support/package.json` drops its `@effect/platform` row and keeps `@effect/platform-node`, `@effect/vitest`, `@electric-sql/pglite`, `effect`, and `vitest`. `vitest: 5.0.0` already satisfies `@effect/vitest@4`'s `vitest >=5.0.0 <6.0.0` peer requirement, so the catalog row stays. Parsing failures arrive as `Schema.SchemaError` carrying a `SchemaIssue`, and `Schema.Union`, `Tuple`, `Literals`, and `TemplateLiteral` take arrays.

## [10]-[VALIDATION]

`apps/creative-cloud/window-list/` is the validators' one compiled tool. `WindowList <pid>` reads `CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID)`, decodes each dictionary through `PropertyListDecoder` into one `Codable` value, keeps the entries whose `kCGWindowOwnerPID` matches, and emits one JSON array with one row per window carrying `windowId` from `kCGWindowNumber`, `ownerPid` from `kCGWindowOwnerPID`, `name` from `kCGWindowName` where the window server holds one, `layer` from `kCGWindowLayer`, and `bounds` from `kCGWindowBounds` through `CGRect(dictionaryRepresentation:)` as `x`, `y`, `w`, and `h`. A missing or non-numeric argument prints the usage line and exits 2, a session with no window server exits 1. The read goes to the window server and touches no accessibility API, so it answers while a host is too busy to answer an accessibility request. `nx run WindowList:build` writes the binary to `.cache/xcode/apps/creative-cloud/window-list/Build/Products/Debug/WindowList`.

| [INDEX] | [PROOF]          | [PROCEDURE]                                                                                   |
| :-----: | :--------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | Window capture   | Read the window list, take the target `windowId`, run `screencapture -x -l <windowId> <path>` |
|  [02]   | Dialog opened    | Read the window list before and after the step, poll for a fresh `windowId`, capture it       |
|  [03]   | Dialog closed    | Read the window list again; the `windowId` is gone                                            |
|  [04]   | Document state   | Compare the tool's own read call against the values the step sent                             |
|  [05]   | Preference state | Read the preference file after the host quits, compare each key with the value the step set   |
|  [06]   | Drive save       | Hash every file under the save folder and compare the manifest with the previous run's        |

`screencapture -l <windowId>` is the capture form rather than `-R <rect>`: the hosts sit behind the terminal window and a rect capture returns whatever is on top. PNGs and window-list JSON land under `.artifacts/creative-cloud/validation/<host>/`.

One Drive write exists, `<drive.designLibrary>/05.Software Related Assets/99.Default Profiles/<App>/`, where `<drive.designLibrary>` is `~/Library/CloudStorage/GoogleDrive-b.samiee@mzn-group.com/My Drive/03.Digital Asset Database/` and whose entries today are `00.Adobe Illustrator`, `InDesign`, `Photoshop`, `Rhino3D`, `Shared`, and `Typeface`. Each save writes a manifest beside the run's artifacts holding one row per saved file with its path, byte count, and `shasum -a 256` digest, and the next run compares digests to tell a rewritten file from an untouched one.

## [11]-[CONTINUOUS_INTEGRATION]

`.github/workflows/ci.yml` and `.github/actions/setup/action.yml` need no change. Job `check` on `ubuntu-24.04` runs `nx run rasm:check` and `nx affected -t check --exclude=tag:host:macos`, which typechecks the TypeScript projects. Job `macos` on `xcode-27` runs `nx affected -t check --exclude='!tag:host:macos'` with ad-hoc signing, which builds and lints `WindowList`: the workspace plugin tags a `.pbxproj` project `language:swift` and `host:macos`, and Swift `check` depends on `build` and `lint`. Job `format` runs `nx run rasm:format` then `git diff --exit-code`.

`build`, `deploy`, and `grid` sit outside `check` and run in no job. The setup action's `pnpm install` installs the added catalog rows, and its NuGet, uv, and pnpm caches key off files this change already touches.

## [12]-[PROOFS]

Each row runs from the repository root under `mise exec --`, and the join closes when every exit code reads 0.

| [INDEX] | [COMMAND]                                              | [READBACK]                                                       |
| :-----: | :----------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `pnpm install`                                         | Added rows at their catalog versions, no ignored-build notice    |
|  [02]   | `ls node_modules/.bin/swc node_modules/.bin/vite`      | Both binaries present                                            |
|  [03]   | `nx show projects`                                     | Listing gains every name of [02]                                 |
|  [04]   | `nx run rasm:lint`                                     | Exit 0 from ast-grep, Biome, ruff, yamllint, and actionlint      |
|  [05]   | `nx run rasm:check`                                    | Exit 0                                                           |
|  [06]   | `nx run-many -t typecheck`                             | Exit 0 for every TypeScript project                              |
|  [07]   | `nx run-many -t check`                                 | Exit 0, `WindowList:check` building through `xcodebuild`         |
|  [08]   | `nx run rasm:format` twice                             | Second pass writes nothing and `git diff --exit-code` exits 0    |
|  [09]   | `nx run @rasm/illustrator-scripts:build`               | One `.jsx` per entry, no `.d.jsx`, no `const`, arrow, or `class` |
|  [10]   | `nx run @rasm/photoshop-plugin:build`                  | `manifest.json` and a bundle keeping `require("photoshop")`      |
|  [11]   | `nx run @rasm/indesign-plugin:build`                   | `manifest.json` and a bundle keeping `require("indesign")`       |
|  [12]   | `nx run WindowList:build`                              | Binary under `.cache/xcode/apps/creative-cloud/window-list/`     |
|  [13]   | `node apps/creative-cloud/server/main.ts`              | `initialize` result naming `creative-cloud` on stdio             |
|  [14]   | `nx run @rasm/typography:grid -- <w> <baseline>` | Grid rows for that pair on stdout                                |
|  [15]   | `claude mcp list` in a fresh session                   | `creative-cloud` connects and `indesign-sidekick` is absent      |
|  [16]   | `mcp-inspector --cli --method tools/list --format json -- node apps/creative-cloud/server/main.ts` | Every tool of `creative-cloud.md` [05] on stdout, exit 0         |
|  [17]   | The same with `--method tools/call --tool-name health` | One `health` row per host, exit 0                                |
