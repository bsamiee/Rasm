# [WORKSPACE]

pnpm workspace layout of the TypeScript packages Nx runs.

## [01]-[PRINCIPLES]

Rules every workspace package follows:
- Packages join the workspace by sitting in a folder a `packages` glob matches
- Package source sits at the package root
- Consumers import a sibling package by its name through its `node_modules` link and `exports`, like a package from the registry
- Manifests name versions only through `catalog:` and `workspace:`
- Manifests hold no task scripts, and no `project.json` exists

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

Root manifest fields:

| [INDEX] | [FIELD]           | [VALUE]                           | [REASON]                                                             |
| :-----: | :---------------- | :-------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `private`         | `true`                            | Root is never published                                              |
|  [02]   | `type`            | `"module"`                        | Node asks every package to state it                                  |
|  [03]   | `packageManager`  | Absent                            | `.config/mise.lock` pins pnpm and Node                               |
|  [04]   | `scripts`         | `prepare` alone                   | Install lifecycle step                                               |
|  [05]   | `devDependencies` | `catalog:` and `workspace:*` rows | Tools, workspace Nx plugins, and `types` packages root programs load |
|  [06]   | `workspaces`      | Absent                            | Members come from `pnpm-workspace.yaml` alone                        |
|  [07]   | `pnpm`            | Absent                            | Settings come from `pnpm-workspace.yaml` alone                       |

Root project and Nx settings:
- Root `package.json` holding an `nx` key becomes an Nx project, and `"includedScripts": []` there keeps `prepare` from becoming a target
- Root targets `check--jscpd` and `check--knip` read every workspace file, and `knipConfig` names `.config/knip.json`, as knip searches no `.config`
- Root target `check--stylelint` lints every `.css` file Git lists, with `stylelintConfig` naming `.config/stylelint.config.ts`
- `check--stylelint` hashes each listed file, each module and reference file `stylelintConfig` names, and each package it imports, extends, or plugs in
- `nx.json` sets `useDaemonProcess: false` while the Nx daemon forces debug logs into an unrotated `daemon.log` and restarts graphs edits interrupt

## [03]-[WORKSPACE_FILE]

`pnpm-workspace.yaml` holds members, catalogs, and every pnpm setting:

```yaml
packages:
  - products/**
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

Settings and their values against pnpm defaults:

| [INDEX] | [SETTING]             | [DEFAULT] | [VALUE]                                                     |
| :-----: | :-------------------- | :-------- | :---------------------------------------------------------- |
|  [01]   | `packages`            | None      | Recursive globs, new packages edit nothing                  |
|  [02]   | `catalogMode`         | `manual`  | `strict`: `pnpm add` refuses versions outside catalog       |
|  [03]   | `minimumReleaseAge`   | `1440`    | `0`: installs take releases on publish day                  |
|  [04]   | `saveExact`           | `false`   | `true`: `pnpm add` writes exact versions in place of ranges |
|  [05]   | `blockExoticSubdeps`  | `true`    | Omit: transitive git and tarball sources stay refused       |
|  [06]   | `allowBuilds`         | None      | `true` or `false` per package with install scripts          |
|  [07]   | `overrides`           | None      | Graph pins, selector `:peer` targets peer ranges alone      |
|  [08]   | `patchedDependencies` | None      | `<package>@<version>` to patch file                         |

Settings outside the workspace file:
- `.npmrc` holds only the auth, registry, TLS, proxy, and `local-address` settings pnpm reads from it
- `virtualStoreType` stays `project`, as `effect-tsgo patch` rewrites files under `node_modules` and a global virtual store refuses the writes

## [04]-[CATALOGS]

Catalog definitions and where `catalog:` resolves:
- `catalog` defines the default catalog, read as `catalog:`, and `catalogs.<name>` a named one, read as `catalog:<name>`
- `catalog` beside `catalogs.default` defines the default catalog twice and fails install
- `catalog:` stands in `dependencies`, `devDependencies`, `peerDependencies`, and `optionalDependencies`
- Named catalog `peers` holds ranges for peer dependencies, and the default catalog exact versions for installs
- `pnpm pack` and `pnpm publish` replace each `catalog:` with its catalog range, and committed manifests hold no version
- `catalogMode: strict` governs `pnpm add` alone, and hand-written manifest versions pass it

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

Library manifest fields:

| [INDEX] | [FIELD]         | [VALUE]                           | [REASON]                                                 |
| :-----: | :-------------- | :-------------------------------- | :------------------------------------------------------- |
|  [01]   | `version`       | Package's release version         | Release tooling reads and writes it in each manifest     |
|  [02]   | `private`       | `true` where no upload is wanted  | Stops `pnpm publish` alone                               |
|  [03]   | `type`          | `"module"`                        | Node treats `.js` files as CommonJS without it           |
|  [04]   | `sideEffects`   | `false`, or files that register   | Bundlers drop modules consumers leave unused             |
|  [05]   | `exports`       | `.` and one entry per runtime     | Closed entry map, unlisted paths stay unimportable       |
|  [06]   | `imports`       | Rows its specifiers use           | Package files reach each other by path and extension     |
|  [07]   | `files`         | `dist` `.js` and `.d.ts` globs    | Tarball allowlist                                        |
|  [08]   | `publishConfig` | `exports` and `imports` to `dist` | Both replace source entries at pack                      |
|  [09]   | `engines.node`  | `>=<major>`                       | Node major mise installs                                 |
|  [10]   | `nx`            | Facts no manifest row states      | Holds tags and target options in place of `project.json` |

Manifest rules:
- `main`, `module`, and top-level `types` serve resolvers older than `exports`, the field Node, TypeScript, and bundlers read in their place
- `sideEffects` rows name a file without its folder (`panel.*`), bundlers match them at any depth, and one row covers source and `dist`
- Test suites and applications state `private: true` and leave out `files` and `publishConfig`
- Packages a release package names through `workspace:` in any dependency field state the `version` `pnpm pack` requires

## [06]-[EXPORTS_AND_IMPORTS]

Entry points across packages and specifiers inside one:
- `exports` to source give TypeScript, Vite, and Node one resolver answer per runtime with no alias or build step
- Node strips types only outside `node_modules`, and a workspace link resolves to its real folder, where sibling source runs as written
- `.` resolves to `index.ts`, a file forwarding exports with no logic
- Entries beyond `.` exist per runtime or optional dependency family (`./node`, `./browser`), each with an optional peer where it adds one
- Entries callers import alike on every runtime take one key with a condition per runtime resolver (`workerd`), `default` serving Node
- `"./package.json"` stays exported, as `exports` hides every path it omits from tools reading the manifest
- Files inside a package import `#/<path>`, a TypeScript module by path alone and any other file with its extension and any query (`#/toast`, `#/panel.tsx`, `#/manifest.json`, `#/toast.css?inline`)
- Plugin `checks` of `@rasm/vite` resolves a `#/` specifier carrying a Vite query by its path, as row `#/*` otherwise takes the query and appends `.ts`
- Rule `import/extensions` of oxlint refuses a `.ts` or `.js` extension on a specifier, and `no-restricted-imports` refuses every relative one
- Rows take fixed forms: `#/*` to `./*.ts` for TypeScript modules, or `./*.js` in a JavaScript package, and `#/*.<extension>` to `./*.<extension>` for each other kind (`.tsx`, `.json`, `.css`)
- TSX modules keep their extension, as one row appends one extension, Node runs no `.tsx` file, and Node and Vite refuse a row whose target is a list
- Packages declare only the rows their specifiers use, in `imports` and `publishConfig.imports` alike, as knip reports an unused key
- TypeScript accepts a `.tsx` specifier only where its row's target spells the extension
- Node, TypeScript, and Vite take the matching row with the longest key, and Vite tries a specifier with its query before trying it without
- TypeScript resolves `#/` keys under `nodenext` and `bundler` resolution
- Conditional entries list `types` first and `default` last, as Node orders conditions by key position
- `null` targets block a path a wildcard entry opens (`"./internal/*": null`), and wildcards match nested folders
- Plain string targets need no `types` condition, as TypeScript finds each `.d.ts` beside its `.js` by extension

## [07]-[LINKING]

Dependency specifiers and where each one goes:

| [INDEX] | [SPECIFIER]     | [PUBLISHED_AS]        | [USE]                                                         |
| :-----: | :-------------- | :-------------------- | :------------------------------------------------------------ |
|  [01]   | `workspace:*`   | Exact sibling version | `dependencies` of packages released together, private members |
|  [02]   | `workspace:^`   | Caret range           | Sibling `peerDependencies`, consumers stay unpinned           |
|  [03]   | `catalog:`      | Catalog version       | Every external dependency                                     |
|  [04]   | `catalog:peers` | Catalog range         | Peer dependency on host packages (`effect`, frameworks)       |

Dependency field rules:
- Runtime imports of an unbundled package go in `dependencies`
- Peers a package imports repeat in `devDependencies` at their tested version
- `peerDependenciesMeta.<name>.optional` marks a peer only one subpath entry imports
- `npm:` aliases install one package at more than one version (TypeScript 6 and 7)
- `link:` and `file:` specifiers stay in fixtures
