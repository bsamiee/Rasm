# [TYPESCRIPT]

pnpm resolves every version through the workspace catalog, `tsc --build` checks each package as a composite project.

## [01]-[CATALOG]

- `catalogMode: strict` fails a `pnpm add` outside the catalog range
- `workspace:*` on a sibling package resolves to the workspace copy alone
- `overrides` rows rewrite a resolved version across the graph, `<parent>><child>` scopes a row to one consumer
- `catalog:` as an override value keeps the catalog as the one version
- `peerDependencyRules.allowedVersions` silences the unmet peer warning and resolves nothing
- `pnpm patch <package>` extracts a package for editing, `pnpm patch-commit <dir>` writes the patch file and its `patchedDependencies` row

## [02]-[COMPILER]

- `${configDir}` in a base configuration's path fields resolves to the extending file's directory
- `tsc --build` on a `composite` project writes declarations and build info under `outDir`
- `references` names the `composite` projects a package imports, `tsc --build` builds them first
