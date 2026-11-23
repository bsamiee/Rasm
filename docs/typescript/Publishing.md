# [PUBLISHING]

Library build and publish of the TypeScript packages Nx releases.

## [01]-[PRINCIPLES]

Rules every TypeScript package build follows:
- Product decides the build: a library publishes one module per source file, an application or CLI a bundle
- Packages publish ES modules alone, and Node's `require()` loads them
- Every release package keeps its publish fields (`version`, `files`, `publishConfig`), private ones included
- Development resolves source and the tarball resolves `dist`, both from one manifest that pnpm rewrites at pack
- Packs happen in the package folder, filtered by `files`, with no generated publish folder

## [02]-[BUILD_AND_EMIT]

Build and check per product kind:

| [INDEX] | [PRODUCT]               | [BUILD]                                 | [CHECK]            |
| :-----: | :---------------------- | :-------------------------------------- | :----------------- |
|  [01]   | Library                 | `tsc`, one `.js` and `.d.ts` per source | `tsc`, pack checks |
|  [02]   | Private tool or script  | None, Node strips types                 | `tsc --noEmit`     |
|  [03]   | CLI or server, one file | `vite build`, server environment        | `tsc --noEmit`     |
|  [04]   | Browser application     | `vite build`                            | `tsc --noEmit`     |

Emit and bundle rules:
- `tsc` writes each source file's `.js` and `.d.ts` into `dist` at its source path, with syntax at `target` `esnext`
- Emitted files keep specifiers `#/<path>` and `#/<path>.tsx`, and published rows `#/*` and `#/*.tsx` send them to `./dist/*.js`
- Published rows of every other kind serve files at their source path, and `files` lists each one
- Vite library mode writes no `.d.ts`, and a typed library builds with `tsc`
- Vite bundles through Rolldown and transforms and minifies through Oxc
- Rolldown `checks` emit warnings, and every Vite build includes plugin `checks` of `@rasm/vite`
- Plugin `checks` fails a build on each warning that marks wrong output, and `PLUGIN_TIMINGS` stays a warning
- Server and CLI bundles set `noExternal: true`, and the runtime libraries Rolldown inlines sit in `devDependencies`
- Targets reading another package's `dist` depend on its build (`^build`)

## [03]-[PACKAGE_CONTENTS]

Tarball contents after `pnpm pack` moves `publishConfig` entries over their source entries:

| [INDEX] | [CONTENT]                | [IN_TARBALL] | [REASON]                                     |
| :-----: | :----------------------- | :----------- | :------------------------------------------- |
|  [01]   | `dist/**/*.js`           | Yes          | Runtime modules `exports` and `imports` name |
|  [02]   | `dist/**/*.d.ts`         | Yes          | TypeScript finds them beside each `.js`      |
|  [03]   | `*.js.map`, `*.d.ts.map` | No           | Maps resolve only beside `.ts` source        |
|  [04]   | `.ts` source             | No           | Consumers run `dist`                         |
|  [05]   | Tests, fixtures, configs | No           | `files` allowlist leaves them out            |
|  [06]   | `*.tsbuildinfo`          | No           | Build state `composite` writes into `dist`   |
|  [07]   | `README.md`, `LICENSE`   | Yes          | Packed whatever `files` says                 |

Rules of `files` and `publishConfig`:
- `files` names `dist/**/*.js` and `dist/**/*.d.ts`, as a `"dist"` row packs maps and `tsconfig.tsbuildinfo` with them
- `publishConfig` entries match source entries key for key, as a key missing there leaves its entry out of the published package
- `publishConfig.access: "public"` publishes a scoped package as public

## [04]-[PACK_CHECKS]

Targets checking a package as its tarball:

| [INDEX] | [TARGET]         | [COMMAND]                    | [FINDS]                                                       |
| :-----: | :--------------- | :--------------------------- | :------------------------------------------------------------ |
|  [01]   | `pack--pnpm`     | `pnpm pack --out <file>.tgz` | `workspace:` sibling without `version`, unresolved `catalog:` |
|  [02]   | `check--publint` | `publint --strict`           | `exports`, `imports`, and `bin` targets missing from tarball  |
|  [03]   | `check--knip`    | `knip`                       | Imports of packages no manifest field lists                   |

Reach of each check:
- `check--publint` runs in the package folder after its build and packs it with pnpm, as `pnpm publish` does
- Workspace links miss the `files`, `exports`, `bin`, and dependency range defects a tarball shows
- Runtime imports of a package listed in `devDependencies` alone reach no check, as bundled packages list their inlined libraries there

## [05]-[VERSIONING]

Release versions and notes:
- `release.version.preserveLocalDependencyProtocols` defaults to `true`, and committed manifests keep `workspace:` references
- Conventional commits after a package's last tag write its GitHub release notes, and no changelog file exists

## [06]-[PUBLISH]

Registry upload routes and their credentials:
- `nx release publish` runs `pnpm publish` on the tarball each project's `pack--pnpm` wrote and `attach--gh` uploads
- `pnpm publish` trades the job's OIDC token for a publish token of each package with a trusted publisher
- Packages with no trusted publisher publish with `NODE_AUTH_TOKEN`, and `PNPM_CONFIG__AUTH` hands it to pnpm for the npm registry
- Trusted publishers attach to a package npm holds, and each package's first version publishes with `NODE_AUTH_TOKEN`
- `npm trust github <package> --file release.yml --repository <owner>/<repository> --environment release --allow-publish` adds a trusted publisher
- New trusted publishers expire unless a publish through them succeeds within 2 days
- Trusted publishing accepts GitHub-hosted runners alone
- Trusted publishing attaches provenance to a public package from a public repository with no `--provenance` flag
- Release jobs publish in dependency order, and a consumer's install never meets a missing sibling version
