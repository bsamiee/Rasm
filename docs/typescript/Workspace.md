# [WORKSPACE]

pnpm workspace layout of the TypeScript packages Nx runs.

## [01]-[PRINCIPLES]

Rules every workspace package follows:
- Packages join the workspace by sitting in a folder a `packages` glob matches
- Consumers import a sibling package by its name through its `node_modules` link and `exports`, like a package from the registry
- Manifests name versions only through `catalog:` and `workspace:`
- Manifests hold no task scripts, and no `project.json` exists

## [02]-[ROOT]

Root `package.json` and the settings beside it:
- Root `devDependencies` hold the tools, workspace Nx plugins, and `types` packages root configuration files load
- Root manifest states no `packageManager`, `workspaces`, or `pnpm` field, as mise pins pnpm and `pnpm-workspace.yaml` holds members and settings
- Root `package.json` holding an `nx` key becomes an Nx project, and `"includedScripts": []` there keeps `prepare` from becoming a target
- Root targets `check--jscpd` and `check--knip` read the workspace files their tool selects
- `nx.json` sets `useDaemonProcess: false` while the Nx daemon forces debug logs into an unrotated `daemon.log` and restarts graphs edits interrupt
- Mise `[env]` sets `NX_ISOLATE_PLUGINS` to `false`, as in-process plugins build an identical warm graph sooner than plugin workers
- `virtualStoreType` stays `project`, as `effect-tsgo patch` rewrites files under `node_modules` and a global virtual store refuses the writes
- `storeDir` and `cacheDir` keep the store and metadata cache in `.cache/pnpm`, on the checkout's volume, where pnpm clones or hard-links files
- `nx clean -c cache` empties the store with `.cache`, and the next `pnpm install` downloads every package
- `minimumReleaseAge: 0` installs releases on publish day, where pnpm waits `1440` minutes by default
- `saveExact: true` makes `pnpm add` write exact versions in place of ranges
- `allowBuilds` states `true` or `false` for each package with install scripts

## [03]-[CATALOGS]

Catalog definitions and where `catalog:` resolves:
- Default catalog `catalog` holds exact versions for installs, read as `catalog:`
- Named catalog `peers` holds ranges for peer dependencies, read as `catalog:peers`
- `catalog` beside `catalogs.default` defines the default catalog twice and fails install
- `pnpm pack` and `pnpm publish` replace each `catalog:` with its catalog range, and committed manifests hold no version
- `catalogMode: strict` makes `pnpm add` refuse versions outside a catalog, and hand-written manifest versions pass it

## [04]-[PACKAGE_MANIFEST]

Library manifest:

```json
{
    "name": "@scope/<package>",
    "version": "0.1.0",
    "private": true,
    "description": "<what the package is for>",
    "license": "MIT",
    "repository": { "type": "git", "url": "git+https://github.com/<owner>/<repository>.git", "directory": "<folder>" },
    "files": ["dist/**/*.js", "dist/**/*.d.ts"],
    "type": "module",
    "sideEffects": false,
    "imports": { "#/*": "./*.ts" },
    "exports": {
        "./<module>": "./<module>.ts",
        "./node": "./node.ts",
        "./package.json": "./package.json"
    },
    "publishConfig": {
        "imports": { "#/*": "./dist/*.js" },
        "exports": {
            "./<module>": "./dist/<module>.js",
            "./node": "./dist/node.js",
            "./package.json": "./package.json"
        },
        "access": "public"
    },
    "devDependencies": { "effect": "catalog:" },
    "peerDependencies": { "effect": "catalog:peers" },
    "engines": { "node": ">=<major>" }
}
```

Library manifest fields:

| [INDEX] | [FIELD]         | [VALUE]                           | [REASON]                                                 |
| :-----: | :-------------- | :-------------------------------- | :------------------------------------------------------- |
|  [01]   | `version`       | Package's release version         | Release tooling reads and writes it in each manifest     |
|  [02]   | `private`       | `true` where no upload is wanted  | Stops `pnpm publish` alone                               |
|  [03]   | `files`         | `dist` `.js` and `.d.ts` globs    | Tarball allowlist                                        |
|  [04]   | `type`          | `"module"`                        | Node treats `.js` files as CommonJS without it           |
|  [05]   | `sideEffects`   | `false`, or files that register   | Bundlers drop modules consumers leave unused             |
|  [06]   | `imports`       | Rows its specifiers use           | Package files reach each other through `#/`              |
|  [07]   | `exports`       | One entry per public module       | Closed entry map, unlisted paths stay unimportable       |
|  [08]   | `publishConfig` | `imports` and `exports` to `dist` | Both replace source entries at pack                      |
|  [09]   | `engines.node`  | `>=<major>`                       | Node major mise installs                                 |
|  [10]   | `nx`            | Facts no manifest row states      | Holds tags and target options in place of `project.json` |

Manifest rules:
- `main`, `module`, and top-level `types` serve resolvers older than `exports`, the field Node, TypeScript, and bundlers read in their place
- `sideEffects` rows name a file without its folder (`panel.*`), bundlers match them at any depth, and one row covers source and `dist`
- Test suites and applications state `private: true` and leave out `files` and `publishConfig`
- Packages a release package names through `workspace:` in any dependency field state the `version` `pnpm pack` requires

## [05]-[EXPORTS]

Entry points across packages:
- `exports` to source give TypeScript, Vite, and Node one resolver answer per runtime with no alias or build step
- Node strips types only outside `node_modules`, and a workspace link resolves to its real folder, where sibling source runs as written
- Each public module takes one entry named after its file (`./contract`, `./arabic/joining`), and consumers import that entry directly
- `.` exists only in a package with one public module and names that module's file, as consumers then import the package name alone
- No `index.ts` forwards exports, as CLAUDE.md forbids it, a member gains a second import path, and `tsc` and Vite's dev server load every module it names
- Entries per runtime or optional dependency family (`./node`, `./browser`) carry an optional peer where they add one
- Entries callers import alike on every runtime take one key with a condition per runtime resolver (`workerd`), `default` serving Node
- `"./package.json"` stays exported, as `exports` hides every path it omits from tools reading the manifest
- Conditional entries list `types` first and `default` last, as Node orders conditions by key position
- `null` targets block a path a wildcard entry opens (`"./internal/*": null`), and wildcards match nested folders
- Plain string targets need no `types` condition, as TypeScript finds each `.d.ts` beside its `.js` by extension

## [06]-[IMPORTS]

Specifiers inside one package:
- Files import `#/<path>`, a TypeScript module with no extension and every other kind with its own (`#/toast`, `#/panel.tsx`, `#/manifest.json`)
- Row `#/*` targets `./*.ts`, or `./*.js` in a JavaScript package, and row `#/*.<extension>` targets `./*.<extension>` for each other kind
- Packages declare only the rows their specifiers use, in `imports` and `publishConfig.imports` alike, as knip reports an unused key
- Node, TypeScript, and Vite take the longest matching key, and TypeScript reads rows under `nodenext` and `bundler` resolution
- TSX modules keep their extension, as one row appends one extension, Node runs no `.tsx` file, and Node and Vite refuse a row with a list target
- TypeScript accepts a `.tsx` specifier only where its row's target spells the extension
- Specifiers with a Vite query (`#/toast.css?inline`) match row `#/*` alone, and its target appends `.ts` after their query
- Plugin `checks` of `@rasm/vite/checks` resolves a queried `#/` specifier by its path and restores its query
- Rule `import/extensions` of oxlint enforces both extension forms on `#/` specifiers, and `no-restricted-imports` refuses every relative one

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
