# [WORKSPACE]

pnpm workspace layout of an Effect-based TypeScript monorepo that Nx runs.

## [01]-[PRINCIPLES]

- Packages join the workspace by sitting in a folder a `packages` glob matches
- Package source sits at the package root
- Consumers import a sibling package by its name through its `node_modules` link and `exports`, like a package from the registry
- Every external version sits in the catalog of `pnpm-workspace.yaml`, and manifests name versions only through `catalog:` and `workspace:`
- Manifests hold no task scripts and no `project.json` exists, as Nx infers projects and targets from manifests and tool configuration

## [02]-[ROOT_MANIFEST]

Root `package.json` holds workspace identity and tools that root configuration files import:

```json
{
    "name": "@scope/monorepo",
    "private": true,
    "type": "module",
    "scripts": { "prepare": "effect-tsgo patch" },
    "devDependencies": {
      "@effect/tsgo": "catalog:",
      "@nx/js": "catalog:",
      "@types/node": "catalog:",
      "@typescript/native": "catalog:",
      "nx": "catalog:",
      "typescript": "catalog:",
      "vitest": "catalog:"
    }
}
```

| [INDEX] | [FIELD]           | [VALUE]                           | [REASON]                                                             |
| :-----: | :---------------- | :-------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `private`         | `true`                            | Root is never published                                              |
|  [02]   | `type`            | `"module"`                        | Node asks every package to state it                                  |
|  [03]   | `packageManager`  | Absent                            | `.config/mise.toml` pins pnpm and Node                               |
|  [04]   | `scripts`         | `prepare` alone                   | Install lifecycle step                                               |
|  [05]   | `devDependencies` | `catalog:` and `workspace:*` rows | Tools, workspace Nx plugins, and `types` packages root programs load |
|  [06]   | `workspaces`      | Absent                            | pnpm reads members from `pnpm-workspace.yaml` alone                  |
|  [07]   | `pnpm`            | Absent                            | pnpm reads no settings from a `pnpm` manifest key                    |

- Root `package.json` holding an `nx` key becomes an Nx project, and `"includedScripts": []` there keeps `prepare` from becoming a target
- `nx.json` `plugins` names `@rasm/nx-typescript` for `build--tsc` and `check--tsc` targets, and `@rasm/nx-vitest` for `test--vitest` targets
- Root project targets `check--jscpd` and `check--knip` read every workspace file, and option `knipConfig` names `.config/knip.json`, which knip never finds
- `useDaemonProcess: false` holds while the Nx daemon forces debug logs into an unrotated `daemon.log` and restarts each graph an edit interrupts

## [03]-[WORKSPACE_FILE]

`pnpm-workspace.yaml` holds members, catalogs, and every pnpm setting:

```yaml
packages:
  - packages/**
catalog:
  effect: <version>
catalogs:
  peers:
    effect: <range>
catalogMode: strict
minimumReleaseAge: 0
saveExact: true
allowBuilds:
  <package>: true
overrides:
  <package>: <version>
patchedDependencies:
  <package>@<version>: patches/<file>.patch
```

| [INDEX] | [SETTING]             | [DEFAULT] | [RECOMMENDED]                                                     |
| :-----: | :-------------------- | :-------- | :---------------------------------------------------------------- |
|  [01]   | `packages`            | None      | Recursive globs (`products/**`), so a new package edits nothing   |
|  [02]   | `catalogMode`         | `manual`  | `strict`: `pnpm add` refuses a version outside the catalog        |
|  [03]   | `minimumReleaseAge`   | `1440`    | `0`: install takes a release the day it publishes                 |
|  [04]   | `saveExact`           | `false`   | `true`: `pnpm add` writes an exact version in place of a range    |
|  [05]   | `blockExoticSubdeps`  | `true`    | Omit: transitive git and tarball sources stay refused             |
|  [06]   | `allowBuilds`         | None      | Name each package whose install script may run, `true` or `false` |
|  [07]   | `overrides`           | None      | Graph pins, `:peer` on the selector targets peer ranges alone     |
|  [08]   | `patchedDependencies` | None      | `<package>@<version>` to a patch file                             |

- `.npmrc` holds only auth, registry, TLS, proxy, and `local-address` settings, which pnpm reads from it
- `effect-tsgo patch` rewrites files under `node_modules`, which a global virtual store refuses, so `virtualStoreType` stays `project`

## [04]-[CATALOGS]

- `catalog` defines the default catalog, read as `catalog:`, and `catalogs.<name>` a named one, read as `catalog:<name>`
- `catalog` beside `catalogs.default` fails install, as the default catalog is then defined twice
- `catalog:` stands in `dependencies`, `devDependencies`, `peerDependencies`, and `optionalDependencies`
- Named catalog `peers` holds ranges for peer dependencies, and the default catalog exact versions for installs
- `pnpm pack` and `pnpm publish` replace each `catalog:` with its catalog range, so committed manifests hold no version
- Hand-written manifest versions pass `catalogMode: strict`, which governs `pnpm add` alone

## [05]-[PACKAGE_MANIFEST]

Library manifest:

```json
{
    "name": "@scope/<package>",
    "version": "0.1.0",
    "type": "module",
    "sideEffects": false,
    "exports": {
        ".": "./index.ts",
        "./node": "./node.ts",
        "./package.json": "./package.json"
    },
    "imports": { "#/*": "./*.ts" },
    "files": ["dist/**/*.js", "dist/**/*.d.ts"],
    "peerDependencies": { "effect": "catalog:peers" },
    "devDependencies": { "effect": "catalog:" },
    "publishConfig": {
      "access": "public",
      "exports": {
          ".": "./dist/index.js",
          "./node": "./dist/node.js",
          "./package.json": "./package.json"
      },
      "imports": { "#/*": "./dist/*.js" }
    }
}
```

| [INDEX] | [FIELD]         | [VALUE]                           | [REASON]                                                       |
| :-----: | :-------------- | :-------------------------------- | :------------------------------------------------------------- |
|  [01]   | `version`       | Package's release version         | Release tooling reads and writes it in each manifest           |
|  [02]   | `private`       | `true` where no upload is wanted  | Stops `pnpm publish` alone, every other field stays            |
|  [03]   | `type`          | `"module"`                        | Node treats `.js` files as CommonJS without it                 |
|  [04]   | `sideEffects`   | `false`, or files that register   | Bundlers drop unused modules of a consumer                     |
|  [05]   | `exports`       | `.` plus one entry per runtime    | Closed entry map, a path absent from it cannot be imported     |
|  [06]   | `imports`       | One `#/*` row                     | Files inside a package reach each other without relative paths |
|  [07]   | `files`         | `dist` `.js` and `.d.ts` globs    | Tarball allowlist                                              |
|  [08]   | `publishConfig` | `exports` and `imports` to `dist` | pnpm hoists both over the source maps at pack                  |
|  [09]   | `engines.node`  | `>=27`                            | Node release mise installs                                     |
|  [10]   | `nx`            | Facts no manifest row states      | Holds tags and target options in place of `project.json`       |

- `main`, `module`, and top-level `types` serve resolvers older than `exports`, which Node, TypeScript, and bundlers read in their place
- `sideEffects` rows name a file without its folder (`panel.*`), which bundlers match at any depth, so one row covers source and `dist`
- Test suites and applications state `private: true` and leave out `files` and `publishConfig`
- Packages a release package names through `workspace:` in any dependency field state `version`, which `pnpm pack` requires

## [06]-[EXPORTS_AND_IMPORTS]

- `.` resolves to `index.ts`, which forwards exports and holds no logic
- Entries beyond `.` exist per runtime or optional dependency family (`./node`, `./browser`), each with an optional peer where it adds one
- Entries callers import alike on every runtime take one key with a condition per runtime resolver (`workerd`), `default` serving Node
- `"./package.json"` stays exported, since `exports` hides every path it omits from tools that read the manifest
- Files inside a package import `#/<module>` with no extension, and the `./*.ts` target supplies one for Node, TypeScript, and Vite
- TypeScript 6 resolves `#/` keys under `nodenext` and `bundler` resolution
- `#/*` puts the matched text into its target, so `./*.ts` reaches `.ts` modules alone and `.tsx` modules take their own row
- Conditional entries list `types` first and `default` last, as Node orders conditions by key position
- `null` targets block a path a wildcard entry would open (`"./internal/*": null`), and wildcards match nested folders
- Plain string targets need no `types` condition, as TypeScript finds each `.d.ts` beside its `.js` by extension

## [07]-[LINKING]

| [INDEX] | [SPECIFIER]     | [PUBLISHED_AS]        | [USE]                                                         |
| :-----: | :-------------- | :-------------------- | :------------------------------------------------------------ |
|  [01]   | `workspace:*`   | Exact sibling version | `dependencies` of packages released together, private members |
|  [02]   | `workspace:^`   | Caret range           | `peerDependencies` on a sibling, keeping consumers unpinned   |
|  [03]   | `catalog:`      | Catalog version       | Every external dependency                                     |
|  [04]   | `catalog:peers` | Catalog range         | Peer dependency on a host package (`effect`, a framework)     |

- Runtime imports of an unbundled package go in `dependencies`
- Peers the package imports repeat in `devDependencies` at the tested version
- `peerDependenciesMeta.<name>.optional` marks a peer only one subpath entry imports
- `npm:` aliases install one package at more than one version (TypeScript 6 and 7)
- `link:` and `file:` specifiers stay in fixtures

- `exports` to source give TypeScript, Vite, and Node one resolver answer per runtime with no alias or build step
- Node strips types only outside `node_modules`, and a workspace link resolves to its real folder, so sibling source runs as written
