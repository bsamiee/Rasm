# [PUBLISHING]

Library build and publish in an Effect-based pnpm and Nx monorepo.

## [01]-[PRINCIPLES]

- Product decides the build: a library publishes one module per source file, an application or CLI a bundle
- Packages publish ES modules alone, which `require()` loads from Node 20.19 and 22.12
- Every release package keeps its publish fields (`version`, `files`, `publishConfig`), private ones included
- Development resolves source and the tarball resolves `dist`, both from one manifest that pnpm rewrites at pack
- Packs happen in the package folder, filtered by `files`, with no generated publish folder
- `dist` and `*.tsbuildinfo` stay ignored by Git

## [02]-[BUILD_AND_EMIT]

| [INDEX] | [PRODUCT]                 | [BUILD]                                    | [CHECK]                     |
| :-----: | :------------------------ | :----------------------------------------- | :-------------------------- |
|  [01]   | Library with many entries | `tsc -b`, one `.js` and `.d.ts` per source | `tsc`, pack checks          |
|  [02]   | Library with few entries  | tsdown with `dts`                          | `tsc --noEmit`, pack checks |
|  [03]   | Private tool or script    | None, Node strips types                    | `tsc --noEmit`              |
|  [04]   | CLI or server, one file   | Rolldown or esbuild bundle                 | `tsc --noEmit`              |
|  [05]   | Browser application       | `vite build`                               | `tsc --noEmit`              |

- `tsc` writes one `.js` and one `.d.ts` into `dist` per source file, at its source path
- Specifiers `#/<module>` stay as written in emitted files, and the published `imports` map sends them to `./dist/*.js`
- Declarations come from the pass that writes JavaScript, or from a per-file emitter under `isolatedDeclarations`
- tsdown writes declarations through `rolldown-plugin-dts`, with Oxc under `isolatedDeclarations` and the TypeScript compiler otherwise
- Vite library mode writes no `.d.ts`, so a typed library builds with `tsc` or tsdown
- Bundled packages keep runtime libraries they inline in `devDependencies`
- Bundler externals derive from `dependencies` and `peerDependencies`
- Effect's own packages run one Babel pass marking `#__PURE__` calls over `dist`, since consumer tree shaking reads those marks
- Targets reading another package's `dist` depend on its build (`^build`)

## [03]-[PACKAGE_CONTENTS]

Manifest a consumer installs, after `pnpm pack` applies `publishConfig` and replaces `catalog:` and `workspace:`:

```json
{
    "name": "@scope/<package>",
    "version": "1.4.0",
    "type": "module",
    "sideEffects": false,
    "exports": {
        ".": "./dist/index.js",
        "./node": "./dist/node.js",
        "./package.json": "./package.json"
    },
    "imports": { "#/*": "./dist/*.js" },
    "files": ["dist/**/*.js", "dist/**/*.d.ts"],
    "peerDependencies": { "effect": "^4.1.0" },
    "publishConfig": { "access": "public" }
}
```

| [INDEX] | [CONTENT]                | [IN_TARBALL]      | [REASON]                                                   |
| :-----: | :----------------------- | :---------------- | :--------------------------------------------------------- |
|  [01]   | `dist/**/*.js`           | Yes               | Runtime modules `exports` and `imports` name               |
|  [02]   | `dist/**/*.d.ts`         | Yes               | TypeScript finds them beside each `.js`                    |
|  [03]   | `*.js.map`, `*.d.ts.map` | With source alone | Maps resolve only when the `.ts` files they name are there |
|  [04]   | `.ts` source             | Only for maps     | Consumers run `dist`, source serves maps alone             |
|  [05]   | Tests, fixtures, configs | No                | `files` allowlist leaves them out                          |
|  [06]   | `*.tsbuildinfo`          | No                | Build state `composite` writes into `dist`                 |
|  [07]   | `README.md`, `LICENSE`   | Yes               | npm adds them whatever `files` says                        |

- `files` names `dist/**/*.js` and `dist/**/*.d.ts`, as a `"dist"` row packs maps and `tsconfig.tsbuildinfo` with them
- Packages publishing maps add `dist/**/*.map` and their source globs to `files`, as Effect's packages list `src/**/*.ts`
- `publishConfig` maps match source maps key for key, as a key missing there leaves that entry out of a published package
- `publishConfig.access: "public"` publishes a scoped package as public
- Packages whose bundles inline dependencies collect those licenses into their tarball at build

## [04]-[PACK_CHECKS]

| [INDEX] | [CHECK]         | [COMMAND]                                                    | [FINDS]                                   |
| :-----: | :-------------- | :----------------------------------------------------------- | :---------------------------------------- |
|  [01]   | Pack            | `pnpm pack --out <file>.tgz`                                 | Tarball with `publishConfig` applied      |
|  [02]   | Type resolution | `attw <file>.tgz --profile esm-only`                         | Entries TypeScript cannot resolve         |
|  [03]   | Manifest lint   | `publint <file>.tgz --strict`                                | `exports`, `files`, and field defects     |
|  [04]   | Install test    | Tarball installed in an empty project outside its workspace  | Missing files, wrong ranges, broken `bin` |
|  [05]   | Consumer types  | `tsc` with `skipLibCheck: false` over a fixture importing it | Declaration errors a consumer would see   |

- `attw --pack` runs `npm pack`, which applies no `publishConfig` swap and keeps `catalog:` and `workspace:`, so attw reads a pnpm tarball
- `--profile esm-only` ignores CommonJS resolution failures
- `publint` lints a directory or a tarball, and `--pack pnpm` makes it pack with pnpm
- Each check runs against the packed file of one package, as an Nx target depending on its pack target
- Workspace links miss the `files`, `exports`, `bin`, and dependency range defects a tarball shows

## [05]-[VERSIONING]

- Nx Release groups projects with `projectsRelationship: "fixed"` by default, one bump versioning every package, and `"independent"` each alone
- `release.version.preserveLocalDependencyProtocols` defaults to `true`, so `workspace:` references stay in committed manifests
- Conventional commits or version plan files decide each bump and its changelog text

## [06]-[PUBLISH]

- `nx release publish` runs the workspace package manager's `publish`
- `pnpm -r publish` skips packages with a version their registry holds, so a rerun after a partial failure publishes what remains
- `pnpm publish --dry-run` does everything but the upload
- Trusted publishing from CI needs no stored registry token, and attaches provenance to a public package from a public repository
- Trusted publishing runs on cloud-hosted runners alone, so a self-hosted runner publishes with a registry token and no provenance
- Release jobs publish in dependency order, so a consumer's install never meets a missing sibling version

## [07]-[REGRESSIONS]

| [INDEX] | [WRONG]                                             | [CORRECT]                                           |
| :-----: | :-------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | Publish fields removed from private packages        | Same fields, `private: true` stops the upload alone |
|  [02]   | `publishConfig.directory` with a generated manifest | Pack in place, `publishConfig` overrides            |
|  [03]   | Dual CommonJS and ESM builds per entry              | ES modules alone                                    |
|  [04]   | `npm pack` or `attw --pack` on a pnpm package       | `pnpm pack`, checks on its tarball                  |
|  [05]   | Stale `files` rows or `publishConfig.bin` paths     | Pack checks that read the tarball                   |
|  [06]   | Hand-listed bundler externals                       | Externals derived from `dependencies` and peers     |
|  [07]   | `tsup`                                              | tsdown, which tsup's README names as its successor  |
|  [08]   | Parallel targets writing one `dist`                 | One build target others depend on                   |
|  [09]   | Versions stripped from private manifests            | Every manifest states its release version           |
